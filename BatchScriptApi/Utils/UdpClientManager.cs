using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BatchCommand.Utils
{
    public static class UdpClientManager
    {
        private const int MaxPayloadBytes = 65507;
        private const int MaxSendMessages = 64;
        private const int MaxEventMessages = 128;
        private const long MaxQueuedBytes = 16L * 1024 * 1024;

        // Public carrier for host dispatch; data ownership transfers to the host.
        public readonly record struct Datagram(
            byte[] Data, string RemoteAddress, int RemotePort);

        private sealed class Client
        {
            internal required string Id;
            internal required IPEndPoint LocalEndPoint;
            internal long Generation;
            internal string State = "binding";
            internal bool Closing;
            internal bool Finished;
            internal bool ClosePending;
            internal CancellationTokenSource? Stop = new CancellationTokenSource();
            internal UdpClientSession? Session;
            internal (string State, string Error) SendDrainStatus =
             ("not_requested", string.Empty);
            internal readonly UdpClientEventQueue Events =
                new UdpClientEventQueue(MaxEventMessages, MaxQueuedBytes);
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

        // Payload is Datagram for messages and string for state/error.
        // Invoked only by DrainQueue, outside internal locks.
        public static Action<string, string, object>? Dispatch { get; set; }

        // Numeric IP addresses only. Port zero requests an ephemeral local port.
        public static string Open(string localAddress, int localPort, string? id = null)
        {
            if (!IPAddress.TryParse(localAddress, out var address) ||
                (address.AddressFamily != AddressFamily.InterNetwork &&
                 address.AddressFamily != AddressFamily.InterNetworkV6) ||
                localPort < 0 || localPort > 65535) {
                return string.Empty;
            }
            lock (s_Lock) {
                if (string.IsNullOrEmpty(id)) {
                    id = Guid.NewGuid().ToString("N");
                }
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
                    LocalEndPoint = new IPEndPoint(address, localPort),
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
                throw new OperationCanceledException("UDP client retired.", token);
            }
        }

        private static async Task SetStateAsync(Client client, string state,
            CancellationToken token)
        {
            lock (s_Lock) {
                token.ThrowIfCancellationRequested();
                if (!IsCurrentLocked(client) || client.Closing) {
                    throw new OperationCanceledException("UDP client retired.", token);
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
            try {
                await SetStateAsync(client, "binding", token).ConfigureAwait(false);
                var session = new UdpClientSession(client.LocalEndPoint.AddressFamily,
                    MaxPayloadBytes, MaxSendMessages, MaxQueuedBytes);
                lock (s_Lock) {
                    client.Session = session;
                    if (client.Closing || !IsCurrentLocked(client)) {
                        session.Close();
                    }
                }
                try {
                    // Run even after an early Close so the session completes cleanup.
                    await session.RunAsync(client.LocalEndPoint,
                        async (boundEndPoint, sessionToken) => {
                            lock (s_Lock) {
                                sessionToken.ThrowIfCancellationRequested();
                                if (!IsCurrentLocked(client) || client.Closing) {
                                    throw new OperationCanceledException(
                                        "UDP client retired.", sessionToken);
                                }
                                client.LocalEndPoint = boundEndPoint;
                            }
                            await SetStateAsync(client, "bound", sessionToken)
                                .ConfigureAwait(false);
                        },
                        (datagram, sessionToken) =>
                            PublishAsync(client, "message", datagram, sessionToken)
                    ).ConfigureAwait(false);
                }
                finally {
                    lock (s_Lock) {
                        if (ReferenceEquals(client.Session, session)) {
                         client.SendDrainStatus = session.GetSendDrainStatus();
                         client.Session = null;
                        }
                    }
                }
                token.ThrowIfCancellationRequested();
                await SetStateAsync(client, "closed", token).ConfigureAwait(false);
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
                    SafeLog("UDP event reporting failed: " + ErrorText(reportError));
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

        public static bool Send(string id, byte[] payload, string remoteAddress,
            int remotePort)
        {
            if (string.IsNullOrEmpty(id) || payload == null ||
                remotePort < 1 || remotePort > 65535 ||
                !IPAddress.TryParse(remoteAddress, out var address)) {
                return false;
            }
            lock (s_Lock) {
                return s_Clients.TryGetValue(id, out var client) &&
                    !client.Closing && client.State == "bound" &&
                    client.Session != null &&
                    client.Session.TrySend(payload, new IPEndPoint(address, remotePort));
            }
        }

        // True means accepted locally, not completed or delivered.
        public static bool DrainSend(string id, int timeoutMs = 10000)
        {
         if (string.IsNullOrEmpty(id) || timeoutMs <= 0) {
          return false;
         }
         lock (s_Lock) {
          return s_Clients.TryGetValue(id, out var client) &&
           !client.Closing && client.State == "bound" &&
           client.Session != null && client.Session.DrainSend(timeoutMs);
         }
        }

        // Results belong to the current ID generation, not to dispatched events.
        public static Dictionary<string, object> GetSendDrainStatus(string id)
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
           ? client.Session.GetSendDrainStatus() : client.SendDrainStatus;
          return new Dictionary<string, object> {
           ["state"] = status.State,
           ["error"] = status.Error,
           ["generation"] = client.Generation
          };
         }
        }

        // Wait only: never requests draining, cancels I/O, or dispatches callbacks.
        public static Dictionary<string, object> WaitSendDrain(
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
                            ? session.GetSendDrainStatus() : client.SendDrainStatus;
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
            client.State = "closed";
            client.ClosePending = false;
            client.Events.Complete();
            client.Stop?.Cancel();
            client.Session?.Close();
        }

        public static bool Close(string id)
        {
            if (string.IsNullOrEmpty(id)) {
                return false;
            }
            lock (s_Lock) {
                if (!s_Clients.TryGetValue(id, out var client) || client.Closing) {
                    return false;
                }
                RetireLocked(client);
                client.ClosePending = true;
                return true;
            }
        }

        // Does not wait for network tasks or revoke admitted callbacks.
        public static int CloseAll()
        {
            lock (s_Lock) {
                int count = s_Clients.Count;
                foreach (var client in s_Clients.Values) {
                    RetireLocked(client);
                }
                s_Clients.Clear();
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
                return !string.IsNullOrEmpty(id) && s_Clients.TryGetValue(id, out var client)
                    ? client.State : "unknown";
            }
        }

        // Returns the actual bound endpoint, including an assigned ephemeral port.
        public static string GetLocalEndPoint(string id)
        {
            lock (s_Lock) {
                return !string.IsNullOrEmpty(id) &&
                    s_Clients.TryGetValue(id, out var client) && client.State == "bound"
                    ? client.LocalEndPoint.ToString() : string.Empty;
            }
        }

        public static List<string> List()
        {
            lock (s_Lock) {
                return new List<string>(s_Clients.Keys);
            }
        }

        public static int DrainQueue(int maxCount = 100)
        {
            if (maxCount <= 0 || Interlocked.CompareExchange(ref s_Draining, 1, 0) != 0) {
                return 0;
            }
            try {
                int drained = 0;
                while (drained < maxCount) {
                    Client? selected = null;
                    UdpClientEventQueue.Entry item = default;
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
                                item = new UdpClientEventQueue.Entry(
                                    client.Generation, "state", "closed", 0);
                                break;
                            }
                            if (!client.Closing && client.Events.TryDequeue(out item)) {
                                selected = client;
                                break;
                            }
                        }
                        if (selected != null &&
                            IsCurrentLocked(selected) && item.Generation == selected.Generation &&
                            (explicitClose || !selected.Closing)) {
                            s_DispatchingClient = selected;
                            s_DispatchThreadId = Environment.CurrentManagedThreadId;
                        }
                        else {
                            selected = null;
                        }
                    }
                    if (selected == null) {
                        break;
                    }
                    ++drained;
                    // Reservation admits this callback. Close cannot revoke it.
                    try {
                        object payload = item.Payload;
                        if (payload is UdpClientEventQueue.Datagram datagram) {
                            payload = new Datagram(
                                datagram.Data, datagram.RemoteAddress, datagram.RemotePort);
                        }
                        Dispatch?.Invoke(selected.Id, item.Kind, payload);
                    }
                    catch (Exception error) {
                        SafeLog("UDP dispatch failed: " + ErrorText(error));
                    }
                    finally {
                        lock (s_Lock) {
                            s_DispatchingClient = null;
                            s_DispatchThreadId = 0;
                            if (explicitClose && IsCurrentLocked(selected)) {
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
                // Logging must not interrupt dispatch or worker cleanup.
            }
        }
    }
}
