using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BatchCommand.Utils
{
    public static class TcpClientManager
    {
        private const int MaxPayloadBytes = 4 * 1024 * 1024;
        private const int MaxSendMessages = 64;
        private const int MaxEventMessages = 128;
        private const long MaxQueuedBytes = 16L * 1024 * 1024;

        private sealed class Client
        {
            internal required string Id;
            internal required string Host;
            internal required string FrameMode;
            internal int Port;
            internal int ReconnectCount;
            internal long Generation;
            internal string State = "connecting";
            internal bool Closing;
            internal bool Finished;
            internal bool ClosePending;
            internal bool SendShutdownRequested;
            internal (string State, string Error) SendShutdownStatus =
                ("not_requested", string.Empty);
            internal CancellationTokenSource? Stop = new CancellationTokenSource();
            internal TcpClientSession? Session;
            internal readonly TcpClientEventQueue Events =
                new TcpClientEventQueue(MaxEventMessages, MaxQueuedBytes);
        }

        private static readonly object s_Lock = new object();
        private static readonly Dictionary<string, Client> s_Clients =
            new Dictionary<string, Client>(StringComparer.Ordinal);
        private static long s_Generation;
        private static int s_DrainCursor;
        private static int s_Draining;
        private static Client? s_DispatchingClient;
        private static int s_DispatchThreadId;

        public static Action<string>? Log { get; set; }

        // Payload is byte[] for messages and string for state/error.
        // Called only by DrainQueue, outside internal locks.
        public static Action<string, string, object>? Dispatch { get; set; }

        public static string Open(string host, int port, string? id = null,
            string frameMode = "raw", int reconnectCount = 0)
        {
            if (string.IsNullOrWhiteSpace(host) || port < 1 || port > 65535 ||
                (frameMode != "raw" && frameMode != "length32_be")) {
                return string.Empty;
            }
            lock (s_Lock) {
                if (string.IsNullOrEmpty(id)) {
                    id = Guid.NewGuid().ToString("N");
                }
                // Keep the reservation even when CloseAll clears s_Clients.
                // The callback itself may close and reopen its own ID.
                if (s_DispatchingClient?.Id == id &&
                    s_DispatchThreadId != Environment.CurrentManagedThreadId) {
                    return string.Empty;
                }
                if (s_Clients.TryGetValue(id, out var previous)) {
                    if (!previous.Closing && !previous.Finished) {
                        return string.Empty;
                    }
                    RetireLocked(previous);
                }
                var client = new Client {
                    Id = id,
                    Host = host,
                    Port = port,
                    FrameMode = frameMode,
                    ReconnectCount = Math.Max(0, reconnectCount),
                    Generation = ++s_Generation
                };
                s_Clients[id] = client;
                _ = Task.Run(() => RunClientAsync(client));
                return id;
            }
        }

        private static bool IsCurrentLocked(Client client)
        {
            return s_Clients.TryGetValue(client.Id, out var current) &&
                ReferenceEquals(current, client);
        }

        private static async Task PublishAsync(Client client, string kind,
            object payload, CancellationToken token)
        {
            if (!await client.Events.EnqueueAsync(
                client.Generation, kind, payload, token).ConfigureAwait(false)) {
                throw new OperationCanceledException("TCP client retired.", token);
            }
        }

        private static async Task SetStateAsync(Client client, string state,
            CancellationToken token)
        {
            lock (s_Lock) {
                token.ThrowIfCancellationRequested();
                if (!IsCurrentLocked(client) || client.Closing) {
                    throw new OperationCanceledException("TCP client retired.", token);
                }
                client.State = state;
            }
            await PublishAsync(client, "state", state, token).ConfigureAwait(false);
        }

        private static string ErrorText(Exception error)
        {
            string text = error.GetType().Name + ": " + error.Message;
            return text.Length <= 2048 ? text : text.Substring(0, 2048);
        }

        private static async Task RunClientAsync(Client client)
        {
            CancellationToken token;
            lock (s_Lock) {
                token = client.Stop!.Token;
            }
            bool everConnected = false;
            int failures = 0;
            try {
                await SetStateAsync(client, "connecting", token).ConfigureAwait(false);
                while (true) {
                    token.ThrowIfCancellationRequested();
                    var session = new TcpClientSession(client.FrameMode,
                        MaxPayloadBytes, MaxSendMessages, MaxQueuedBytes);
                    lock (s_Lock) {
                        client.Session = session;
                        if (client.Closing || !IsCurrentLocked(client)) {
                            session.Close();
                        }
                    }
                    bool attemptFailed = false;
                    try {
                        await session.RunAsync(client.Host, client.Port,
                            async sessionToken => {
                                everConnected = true;
                                failures = 0;
                                await SetStateAsync(client, "connected",
                                    sessionToken).ConfigureAwait(false);
                            },
                            (data, sessionToken) =>
                                PublishAsync(client, "message", data, sessionToken)
                        ).ConfigureAwait(false);
                    }
                    catch (Exception error) {
                        attemptFailed = true;
                        token.ThrowIfCancellationRequested();
                        await PublishAsync(client, "error", ErrorText(error), token)
                            .ConfigureAwait(false);
                    }
                    finally {
                        lock (s_Lock) {
                            if (ReferenceEquals(client.Session, session)) {
                                client.SendShutdownStatus = session.GetSendShutdownStatus();
                                client.Session = null;
                            }
                        }
                    }
                    token.ThrowIfCancellationRequested();
                    bool sendShutdownRequested;
                    lock (s_Lock) {
                        sendShutdownRequested = client.SendShutdownRequested;
                    }
                    if (sendShutdownRequested) {
                        await SetStateAsync(client,
                            attemptFailed ? "failed" : "disconnected", token)
                            .ConfigureAwait(false);
                        break;
                    }
                    // Match the existing retry-budget convention: positive N
                    // permits N consecutive ended attempts; success resets it.
                    ++failures;
                    if (client.ReconnectCount <= 0 ||
                        failures >= client.ReconnectCount) {
                        string terminal = client.ReconnectCount > 0 || !everConnected
                            ? "failed" : "disconnected";
                        await SetStateAsync(client, terminal, token).ConfigureAwait(false);
                        break;
                    }
                    await SetStateAsync(client, "reconnecting", token).ConfigureAwait(false);
                    await Task.Delay(500, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) {
                // Explicit close or replacement owns the terminal notification.
            }
            catch (Exception error) {
                try {
                    token.ThrowIfCancellationRequested();
                    await PublishAsync(client, "error", ErrorText(error), token)
                        .ConfigureAwait(false);
                    await SetStateAsync(client, "failed", token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested) {
                }
                catch (Exception reportError) {
                    SafeLog("TCP event reporting failed: " + ErrorText(reportError));
                }
            }
            finally {
                lock (s_Lock) {
                    client.Finished = true;
                    client.Stop?.Dispose();
                    client.Stop = null;
                }
            }
        }

        public static bool Send(string id, byte[] payload)
        {
            if (string.IsNullOrEmpty(id) || payload == null) {
                return false;
            }
            lock (s_Lock) {
                return s_Clients.TryGetValue(id, out var client) &&
                    !client.Closing && client.State == "connected" &&
                    !client.SendShutdownRequested &&
                    client.Session != null && client.Session.TrySend(payload);
            }
        }

        // Nonblocking and idempotent while the session remains connected.
        // Acceptance disables reconnect even if draining later fails.
        public static bool ShutdownSend(string id, int timeoutMs = 10000)
        {
            if (string.IsNullOrEmpty(id) || timeoutMs <= 0) {
                return false;
            }
            lock (s_Lock) {
                if (!s_Clients.TryGetValue(id, out var client) ||
                    client.Closing || client.State != "connected" ||
                    client.Session == null || !client.Session.ShutdownSend(timeoutMs)) {
                    return false;
                }
                client.SendShutdownRequested = true;
                return true;
            }
        }

        // Results belong to the current ID generation, not to dispatched events.
        public static Dictionary<string, object> GetSendShutdownStatus(string id)
        {
            lock (s_Lock) {
                if (string.IsNullOrEmpty(id) ||
                    !s_Clients.TryGetValue(id, out var client)) {
                    return new Dictionary<string, object> {
                        ["state"] = "unknown",
                        ["error"] = string.Empty,
                        ["generation"] = 0L
                    };
                }
                var status = client.Session != null
                    ? client.Session.GetSendShutdownStatus() : client.SendShutdownStatus;
                return new Dictionary<string, object> {
                    ["state"] = status.State,
                    ["error"] = status.Error,
                    ["generation"] = client.Generation
                };
            }
        }

        // Wait only: never requests draining, cancels I/O, or dispatches callbacks.
        public static Dictionary<string, object> WaitSendShutdown(
            string id, int waitTimeoutMs = 1000)
        {
            long generation = 0L;
            (string State, string Error) status = ("unknown", string.Empty);
            Task<(string State, string Error)>? completion = null;
            if (waitTimeoutMs < 0) {
                status = ("invalid_argument", "waitTimeoutMs must be nonnegative.");
            }
            else {
                lock (s_Lock) {
                    if (!string.IsNullOrEmpty(id) &&
                        s_Clients.TryGetValue(id, out var client)) {
                        generation = client.Generation;
                        var session = client.Session;
                        status = session != null
                            ? session.GetSendShutdownStatus() : client.SendShutdownStatus;
                        if (status.State == "draining" && session != null) {
                            // Pin this session even if the ID is replaced while waiting.
                            completion = session.SendDrainCompletion;
                        }
                    }
                }
            }
            bool waitTimedOut = false;
            if (completion != null) {
                // The session completion signal publishes results, never exceptions.
                if (completion.Wait(waitTimeoutMs)) {
                    status = completion.GetAwaiter().GetResult();
                }
                else {
                    // Preserve the last observed drain state; do not cancel the drain.
                    waitTimedOut = true;
                }
            }
            return new Dictionary<string, object> {
                ["state"] = status.State,
                ["error"] = status.Error,
                ["generation"] = generation,
                ["wait_timed_out"] = waitTimedOut
            };
        }

        private static void RetireLocked(Client client)
        {
            client.Closing = true;
            client.State = "disconnected";
            client.Events.Complete();
            try {
                client.Stop?.Cancel();
            }
            finally {
                client.Session?.Close();
            }
        }

        public static bool Close(string id)
        {
            if (string.IsNullOrEmpty(id)) {
                return false;
            }
            lock (s_Lock) {
                if (!s_Clients.TryGetValue(id, out var client)) {
                    return false;
                }
                if (!client.Closing) {
                    RetireLocked(client);
                    // One bounded control slot, independent of event backpressure.
                    client.ClosePending = true;
                }
                return true;
            }
        }

        // Shutdown discards pending callbacks and cancels all workers.
        // It does not synchronously wait for network tasks.
        public static int CloseAll()
        {
            lock (s_Lock) {
                int count = s_Clients.Count;
                foreach (var client in s_Clients.Values) {
                    RetireLocked(client);
                    client.ClosePending = false;
                }
                s_Clients.Clear();
                s_DrainCursor = 0;
                return count;
            }
        }

        public static int Count()
        {
            lock (s_Lock) {
                return s_Clients.Count;
            }
        }

        public static string GetState(string id)
        {
            lock (s_Lock) {
                return !string.IsNullOrEmpty(id) &&
                    s_Clients.TryGetValue(id, out var client) ? client.State : "unknown";
            }
        }

        public static List<string> List()
        {
            lock (s_Lock) {
                return new List<string>(s_Clients.Keys);
            }
        }

        public static int DrainQueue(int maxCount)
        {
            if (maxCount <= 0 || Interlocked.CompareExchange(ref s_Draining, 1, 0) != 0) {
                return 0;
            }
            int drained = 0;
            try {
                while (drained < maxCount) {
                    Client? selected = null;
                    TcpClientEventQueue.Entry item = default;
                    bool explicitClose = false;
                    lock (s_Lock) {
                        var clients = new List<Client>(s_Clients.Values);
                        for (int i = 0; i < clients.Count; ++i) {
                            if (s_DrainCursor >= clients.Count) {
                                s_DrainCursor = 0;
                            }
                            var client = clients[s_DrainCursor++];
                            if (client.ClosePending) {
                                client.ClosePending = false;
                                selected = client;
                                explicitClose = true;
                                item = new TcpClientEventQueue.Entry(
                                    client.Generation, "state", "disconnected", 24);
                                break;
                            }
                            if (!client.Closing && client.Events.TryDequeue(out item)) {
                                selected = client;
                                break;
                            }
                        }
                    }
                    if (selected == null) {
                        break;
                    }
                    ++drained;
                    bool valid;
                    lock (s_Lock) {
                        valid = IsCurrentLocked(selected) &&
                            item.Generation == selected.Generation &&
                            (explicitClose || !selected.Closing);
                        if (valid) {
                            s_DispatchingClient = selected;
                            s_DispatchThreadId = Environment.CurrentManagedThreadId;
                        }
                    }
                    // Reservation is the dispatch admission point. Close does not
                    // revoke admitted callbacks, even before Invoke is entered.
                    if (valid) {
                        try {
                            Dispatch?.Invoke(selected.Id, item.Kind, item.Payload);
                        }
                        catch (Exception error) {
                            SafeLog("TCP dispatch failed: " + ErrorText(error));
                        }
                        finally {
                            lock (s_Lock) {
                                s_DispatchingClient = null;
                                s_DispatchThreadId = 0;
                            }
                        }
                    }
                    if (explicitClose) {
                        lock (s_Lock) {
                            if (IsCurrentLocked(selected)) {
                                s_Clients.Remove(selected.Id);
                            }
                        }
                    }
                }
                return drained;
            }
            finally {
                Volatile.Write(ref s_Draining, 0);
            }
        }

        private static void SafeLog(string text)
        {
            try {
                Log?.Invoke(text);
            }
            catch (Exception) {
                // Logging must not interrupt draining or worker cleanup.
            }
        }
    }
}
