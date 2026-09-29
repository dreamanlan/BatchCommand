using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BatchCommand.Utils
{
    // One connection attempt only. The manager owns retries and event identity.
    internal sealed class TcpClientSession
    {
        private readonly object m_Lock = new object();
        private readonly TcpClient m_Socket = new TcpClient();
        private readonly CancellationTokenSource m_Stop = new CancellationTokenSource();
        private readonly TcpClientSendQueue m_SendQueue;
        private readonly bool m_LengthFramed;
        private readonly int m_MaxPayloadBytes;
        private bool m_Closing;
        private bool m_Connected;
        private bool m_SendShutdownRequested;
        private string m_DrainState = "not_requested";
        private string m_DrainError = string.Empty;
        private Timer? m_DrainTimer;
        private readonly TaskCompletionSource<(string State, string Error)> m_DrainCompletion =
            new TaskCompletionSource<(string State, string Error)>(TaskCreationOptions.RunContinuationsAsynchronously);
        private int m_Started;

        internal TcpClientSession(string frameMode, int maxPayloadBytes,
            int maxQueuedMessages, long maxQueuedBytes)
        {
            if (frameMode != "raw" && frameMode != "length32_be") {
                throw new ArgumentException("Unsupported TCP frame mode.", nameof(frameMode));
            }
            m_SendQueue = new TcpClientSendQueue(
                maxQueuedMessages, maxPayloadBytes, maxQueuedBytes);
            m_LengthFramed = frameMode == "length32_be";
            m_MaxPayloadBytes = maxPayloadBytes;
        }

        internal bool TrySend(byte[] payload)
        {
            lock (m_Lock) {
                return m_Connected && !m_Closing && m_SendQueue.TryEnqueue(payload);
            }
        }

        // True means accepted, not drained or acknowledged by the peer.
        internal bool ShutdownSend(int timeoutMs = 10000)
        {
            lock (m_Lock) {
                if (timeoutMs <= 0 || !m_Connected || m_Closing) {
                    return false;
                }
                if (!m_SendShutdownRequested) {
                    m_SendShutdownRequested = true;
                    m_DrainState = "draining";
                    m_DrainTimer = new Timer(_ => {
                        lock (m_Lock) {
                            if (m_DrainState != "draining") {
                                return;
                            }
                            FinishDrainLocked("timed_out", "TCP send drain timed out.");
                            Close();
                        }
                    }, null, timeoutMs, Timeout.Infinite);
                    m_SendQueue.Drain();
                }
                return true;
            }
        }

        internal (string State, string Error) GetSendShutdownStatus()
        {
            lock (m_Lock) {
                return (m_DrainState, m_DrainError);
            }
        }

        // Completes only for a requested drain; callers must check the current status first.
        internal Task<(string State, string Error)> SendDrainCompletion => m_DrainCompletion.Task;

        // Caller holds m_Lock. A terminal result is immutable.
        private void FinishDrainLocked(string state, string error)
        {
            if (m_DrainState != "draining") {
                return;
            }
            m_DrainState = state;
            m_DrainError = error;
            m_DrainTimer?.Dispose();
            m_DrainTimer = null;
            m_DrainCompletion.TrySetResult((state, error));
        }

        // Also interrupts connect, blocked reads, writes and event backpressure.
        internal void Close()
        {
            lock (m_Lock) {
                if (m_Closing) {
                    return;
                }
                FinishDrainLocked("cancelled", "TCP session closed before send drain completed.");
                m_Closing = true;
                m_Connected = false;
                m_SendQueue.Complete();
                try {
                    m_Stop.Cancel();
                }
                finally {
                    m_Socket.Dispose();
                }
            }
        }

        // Call exactly once, including after an early Close, to finish cleanup.
        // Callbacks enqueue manager events only; they never invoke user code.
        internal async Task RunAsync(string host, int port,
            Func<CancellationToken, Task> onConnected,
            Func<byte[], CancellationToken, Task> onMessage)
        {
            if (Interlocked.Exchange(ref m_Started, 1) != 0) {
                throw new InvalidOperationException("TCP session cannot be reused.");
            }
            Task receive = Task.CompletedTask;
            Task send = Task.CompletedTask;
            CancellationToken token = m_Stop.Token;
            try {
                ArgumentNullException.ThrowIfNull(onConnected);
                ArgumentNullException.ThrowIfNull(onMessage);
                token.ThrowIfCancellationRequested();
                using (var connectStop = CancellationTokenSource.CreateLinkedTokenSource(token)) {
                    connectStop.CancelAfter(TimeSpan.FromSeconds(10));
                    await m_Socket.ConnectAsync(host, port, connectStop.Token).ConfigureAwait(false);
                }
                NetworkStream stream;
                lock (m_Lock) {
                    token.ThrowIfCancellationRequested();
                    stream = m_Socket.GetStream();
                    m_Connected = true;
                }
                // Sending must not depend on state-event queue capacity.
                send = SendAsync(stream, token);
                await onConnected(token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                receive = ReceiveAsync(stream, onMessage, token);
                Task first = await Task.WhenAny(receive, send).ConfigureAwait(false);
                await first.ConfigureAwait(false);
                if (ReferenceEquals(first, send)) {
                    // A drained sender leaves the receive direction open.
                    await receive.ConfigureAwait(false);
                }
                else {
                    bool draining;
                    lock (m_Lock) {
                        draining = m_SendShutdownRequested;
                        if (!draining) {
                            // Serialize EOF teardown against a new drain request.
                            Close();
                        }
                    }
                    if (draining) {
                        // Peer FIN ends receiving, not our accepted outgoing writes.
                        await send.ConfigureAwait(false);
                    }
                }
            }
            catch (Exception error) {
                lock (m_Lock) {
                    FinishDrainLocked("failed",
                        error.GetType().Name + ": " + error.Message);
                }
                throw;
            }
            finally {
                try {
                    Close();
                }
                finally {
                    // Observe both tasks before disposing their cancellation source.
                    // The first completion above determines the primary outcome.
                    try {
                        await Task.WhenAll(receive, send).ConfigureAwait(false);
                    }
                    catch (Exception) {
                        // The manager receives the primary failure from RunAsync.
                    }
                    lock (m_Lock) {
                        m_Stop.Dispose();
                    }
                }
            }
        }

        private async Task SendAsync(NetworkStream stream, CancellationToken token)
        {
            try {
                while (true) {
                    byte[]? payload = await m_SendQueue.DequeueAsync(token).ConfigureAwait(false);
                    if (payload == null) {
                        lock (m_Lock) {
                            token.ThrowIfCancellationRequested();
                            if (!m_Closing && m_SendShutdownRequested) {
                                // All accepted writes have completed before sending FIN.
                                m_Socket.Client.Shutdown(SocketShutdown.Send);
                                FinishDrainLocked("succeeded", string.Empty);
                            }
                        }
                        return;
                    }
                    token.ThrowIfCancellationRequested();
                    if (m_LengthFramed) {
                        await TcpClientFraming.WriteAsync(
                            stream, payload, m_MaxPayloadBytes, token).ConfigureAwait(false);
                    }
                    else {
                        await stream.WriteAsync(payload.AsMemory(), token).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception error) {
                lock (m_Lock) {
                    m_Connected = false;
                    FinishDrainLocked("failed", error.GetType().Name + ": " + error.Message);
                }
                throw;
            }
        }

        private async Task ReceiveAsync(NetworkStream stream,
            Func<byte[], CancellationToken, Task> onMessage, CancellationToken token)
        {
            if (m_LengthFramed) {
                while (true) {
                    byte[]? payload = await TcpClientFraming.ReadAsync(
                        stream, m_MaxPayloadBytes, token).ConfigureAwait(false);
                    if (payload == null) {
                        return;
                    }
                    await onMessage(payload, token).ConfigureAwait(false);
                }
            }
            else {
                // Raw chunks are not application message boundaries.
                var buffer = new byte[Math.Min(64 * 1024, m_MaxPayloadBytes)];
                while (true) {
                    int count = await stream.ReadAsync(buffer.AsMemory(), token).ConfigureAwait(false);
                    if (count == 0) {
                        return;
                    }
                    var payload = new byte[count];
                    Buffer.BlockCopy(buffer, 0, payload, 0, count);
                    await onMessage(payload, token).ConfigureAwait(false);
                }
            }
        }
    }
}
