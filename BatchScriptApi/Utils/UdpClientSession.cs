using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BatchCommand.Utils
{
    // One bound socket, one receiver and one sender. No retries or delivery ACKs.
    internal sealed class UdpClientSession
    {
        private readonly object m_Lock = new object();
        private readonly UdpClient m_Socket;
        private readonly AddressFamily m_AddressFamily;
        private readonly CancellationTokenSource m_Stop = new CancellationTokenSource();
        private readonly UdpClientSendQueue m_SendQueue;
        private readonly int m_MaxPayloadBytes;
        private bool m_Closing;
        private bool m_Bound;
        private string m_DrainState = "not_requested";
        private string m_DrainError = string.Empty;
        private Timer? m_DrainTimer;
        private readonly TaskCompletionSource<(string State, string Error)> m_DrainCompletion =
            new TaskCompletionSource<(string State, string Error)>(TaskCreationOptions.RunContinuationsAsynchronously);
        private int m_Started;

        internal UdpClientSession(AddressFamily addressFamily, int maxPayloadBytes,
            int maxQueuedMessages, long maxQueuedBytes)
        {
            if (addressFamily != AddressFamily.InterNetwork &&
                addressFamily != AddressFamily.InterNetworkV6) {
                throw new ArgumentOutOfRangeException(nameof(addressFamily));
            }
            // Conservative common limit; IPv6 jumbograms are not supported.
            if (maxPayloadBytes <= 0 || maxPayloadBytes > 65507) {
                throw new ArgumentOutOfRangeException(nameof(maxPayloadBytes));
            }
            m_SendQueue = new UdpClientSendQueue(
                maxQueuedMessages, maxPayloadBytes, maxQueuedBytes);
            m_AddressFamily = addressFamily;
            m_MaxPayloadBytes = maxPayloadBytes;
            m_Socket = new UdpClient(addressFamily);
        }

        internal bool TrySend(byte[] payload, IPEndPoint remoteEndPoint)
        {
            ArgumentNullException.ThrowIfNull(payload);
            ArgumentNullException.ThrowIfNull(remoteEndPoint);
            lock (m_Lock) {
                return m_Bound && !m_Closing &&
                    remoteEndPoint.AddressFamily == m_AddressFamily &&
                    m_SendQueue.TryEnqueue(payload, remoteEndPoint);
            }
        }

        // Stop admission and drain local sends; no delivery acknowledgment.
        internal bool DrainSend(int timeoutMs = 10000)
        {
            lock (m_Lock) {
                if (timeoutMs <= 0 || !m_Bound || m_Closing) {
                    return false;
                }
                if (m_DrainState == "not_requested") {
                    m_DrainState = "draining";
                    m_DrainTimer = new Timer(_ => {
                        lock (m_Lock) {
                            if (m_DrainState != "draining") {
                                return;
                            }
                            FinishDrainLocked("timed_out", "UDP send drain timed out.");
                            Close();
                        }
                    }, null, timeoutMs, Timeout.Infinite);
                    m_SendQueue.Drain();
                }
                return m_DrainState == "draining" || m_DrainState == "succeeded";
            }
        }

        internal (string State, string Error) GetSendDrainStatus()
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

        // Interrupt socket I/O and event backpressure, discarding unsent items.
        internal void Close()
        {
            lock (m_Lock) {
                if (m_Closing) {
                    return;
                }
                FinishDrainLocked("cancelled", "UDP session closed before send drain completed.");
                m_Closing = true;
                m_Bound = false;
                m_SendQueue.Complete();
                try {
                    m_Stop.Cancel();
                }
                finally {
                    m_Socket.Dispose();
                }
            }
        }

        // Run exactly once, even after an early Close, to complete cleanup.
        // Callbacks enqueue internal events only; they must not invoke user code.
        internal async Task RunAsync(IPEndPoint localEndPoint,
            Func<IPEndPoint, CancellationToken, Task> onBound,
            Func<UdpClientEventQueue.Datagram, CancellationToken, Task> onMessage)
        {
            if (Interlocked.Exchange(ref m_Started, 1) != 0) {
                throw new InvalidOperationException("UDP session cannot be reused.");
            }
            Task receive = Task.CompletedTask;
            Task send = Task.CompletedTask;
            CancellationToken token = m_Stop.Token;
            try {
                ArgumentNullException.ThrowIfNull(localEndPoint);
                ArgumentNullException.ThrowIfNull(onBound);
                ArgumentNullException.ThrowIfNull(onMessage);
                IPEndPoint boundEndPoint;
                lock (m_Lock) {
                    token.ThrowIfCancellationRequested();
                    if (localEndPoint.AddressFamily != m_AddressFamily) {
                        throw new ArgumentException("UDP address family mismatch.");
                    }
                    m_Socket.Client.Bind(localEndPoint);
                    boundEndPoint = (IPEndPoint)m_Socket.Client.LocalEndPoint!;
                    m_Bound = true;
                }
                // Sending must not depend on state-event queue capacity.
                send = SendAsync(token);
                await onBound(boundEndPoint, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                receive = ReceiveAsync(onMessage, token);
                Task first = await Task.WhenAny(receive, send).ConfigureAwait(false);
                await first.ConfigureAwait(false);
                if (ReferenceEquals(first, send)) {
                    // Local send drain does not close the receive direction.
                    await receive.ConfigureAwait(false);
                }
            }
            catch (Exception error) {
                lock (m_Lock) {
                    FinishDrainLocked("failed", error.GetType().Name + ": " + error.Message);
                }
                throw;
            }
            finally {
                try {
                    Close();
                }
                finally {
                    try {
                        await Task.WhenAll(receive, send).ConfigureAwait(false);
                    }
                    catch (Exception) {
                        // The first completion determines the primary outcome.
                    }
                    lock (m_Lock) {
                        m_Stop.Dispose();
                    }
                }
            }
        }

        private async Task SendAsync(CancellationToken token)
        {
            try {
                while (true) {
                    var item = await m_SendQueue.DequeueAsync(token).ConfigureAwait(false);
                    if (!item.HasValue) {
                        lock (m_Lock) {
                            token.ThrowIfCancellationRequested();
                            FinishDrainLocked("succeeded", string.Empty);
                        }
                        return;
                    }
                    token.ThrowIfCancellationRequested();
                    var datagram = item.Value;
                    int sent = await m_Socket.SendAsync(
                        datagram.Data.AsMemory(), datagram.RemoteEndPoint, token)
                        .ConfigureAwait(false);
                    if (sent != datagram.Data.Length) {
                        throw new InvalidOperationException("Incomplete UDP datagram send.");
                    }
                }
            }
            catch (Exception error) {
                lock (m_Lock) {
                    m_Bound = false;
                    FinishDrainLocked("failed", error.GetType().Name + ": " + error.Message);
                }
                throw;
            }
        }

        private async Task ReceiveAsync(
            Func<UdpClientEventQueue.Datagram, CancellationToken, Task> onMessage,
            CancellationToken token)
        {
            while (true) {
                // UdpClient receives into a full-size buffer, not the configured cap.
                // Never expose an oversized datagram as a valid truncated message.
                UdpReceiveResult result = await m_Socket.ReceiveAsync(token)
                    .ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (result.Buffer.Length > m_MaxPayloadBytes) {
                    throw new InvalidOperationException("UDP datagram exceeds payload limit.");
                }
                var datagram = new UdpClientEventQueue.Datagram(result.Buffer,
                    result.RemoteEndPoint.Address.ToString(), result.RemoteEndPoint.Port);
                // Empty datagrams are messages, not EOF. Transfer array ownership.
                await onMessage(datagram, token).ConfigureAwait(false);
            }
        }
    }
}
