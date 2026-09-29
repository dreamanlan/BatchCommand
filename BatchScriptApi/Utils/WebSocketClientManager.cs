using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BatchCommand.Utils
{
    /// <summary>
    /// Generic per-process websocket client manager (shared by every host:
    /// CefDotnetApp browser process, the standalone AgentCore process and the
    /// console host). Host-agnostic core: connections with background receive
    /// threads (fragmented frames are accumulated until EndOfMessage),
    /// serialized fire-and-forget sends, cooperative close and an event queue.
    ///
    /// Host integration points (set once at startup):
    ///   Log      - diagnostics routing (Lib.NativeLog / AgentCore logger / ...)
    ///   Dispatch - (id, kind, payload) invoked on the DRAINING thread for
    ///              every dequeued event; hosts call the dsl callbacks
    ///              on_wsclient_message(id, message) / on_wsclient_state(id, state)
    ///              there (kind: "message" | "state",
    ///              state: connecting|connected|reconnecting|disconnected|failed).
    ///              When Dispatch is null, drained events are dropped.
    ///
    /// wsclient_open(url[, id[, reconnect_count]]) starts the connection
    /// thread. With reconnect_count == 0 (default) the connection is
    /// one-shot and the reconnect policy is left to the dsl glue. With
    /// reconnect_count > 0 the manager acts as a link-layer keepalive: an
    /// unexpected drop (io error / server close, never an explicit
    /// wsclient_close) is retried with the SAME id until reconnect_count
    /// consecutive attempts fail, then the connection dies with "failed"
    /// (logical disconnect: the upper layer recovers by opening a fresh
    /// connection, which resets the budget). A successful connect resets
    /// the failure counter. Extra state during retries: "reconnecting".
    /// Terminal states: "failed" (dead, will not return), "disconnected"
    /// (explicit close, or a drop in one-shot mode).
    /// </summary>
    public static class WebSocketClientManager
    {
        // All mutable session fields are protected by s_Lock.
        private sealed class SendSession
        {
            public Task Tail = Task.CompletedTask;
            public int Pending;
            public bool Retired;
            public string SendError = string.Empty;
            public string DrainState = "not_requested";
            public string DrainError = string.Empty;
            public Timer? DrainTimer;
            public readonly TaskCompletionSource<(string State, string Error)> Completion =
                new TaskCompletionSource<(string State, string Error)>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            public void FinishDrain(string state, string error)
            {
                if (DrainState != "draining") {
                    return;
                }
                DrainState = state;
                DrainError = error;
                DrainTimer?.Dispose();
                DrainTimer = null;
                Completion.TrySetResult((state, error));
            }
        }

        private sealed class Client
        {
            public string Id = string.Empty;
            public string Url = string.Empty;
            public ClientWebSocket? Ws;
            public SendSession? Session;
            public long Generation;
            public Thread? Thread;
            public volatile bool Closing;
            public volatile bool DrainRequested;
            public long State;  // 0=connecting 1=connected 2=disconnected 3=failed 4=reconnecting
            public int ReconnectCount;  // link-layer retry budget (0 = one-shot)
        }

        private static readonly object s_Lock = new object();
        private static readonly Dictionary<string, Client> s_Clients = new Dictionary<string, Client>();
        private static int s_AutoId = 0;
        private static long s_Generation;

        // (id, kind, payload); kind: "message" | "state"
        private static readonly ConcurrentQueue<Tuple<string, string, string>> s_Queue
            = new ConcurrentQueue<Tuple<string, string, string>>();

        /// <summary>Host diagnostics routing; null = silent.</summary>
        public static Action<string>? Log { get; set; }

        /// <summary>Host event dispatch (id, kind, payload) on the draining thread.</summary>
        public static Action<string, string, string>? Dispatch { get; set; }

        public static string Open(string url, string? id, int reconnectCount = 0)
        {
            if (string.IsNullOrEmpty(url)) {
                return string.Empty;
            }
            if (reconnectCount < 0) {
                reconnectCount = 0;
            }
            lock (s_Lock) {
                if (string.IsNullOrEmpty(id)) {
                    do {
                        id = "wsc" + Interlocked.Increment(ref s_AutoId);
                    } while (s_Clients.ContainsKey(id));
                }
                else if (s_Clients.ContainsKey(id)) {
                    return string.Empty;  // id already in use
                }
                var client = new Client { Id = id, Url = url, ReconnectCount = reconnectCount,
                    Generation = ++s_Generation };
                s_Clients[id] = client;
                client.Thread = new Thread(() => ClientLoop(client)) {
                    IsBackground = true,
                    Name = "wsclient_" + id,
                };
                client.Thread.Start();
            }
            Enqueue(id!, "state", "connecting");
            return id!;
        }

        private static async void ClientLoop(Client client)
        {
            bool everConnected = false;
            int failures = 0;
            while (!client.Closing) {
                var ws = new ClientWebSocket();
                // Never route through a system proxy: the targets are local relay
                // links (or explicitly configured direct urls).
                ws.Options.Proxy = null;
                var session = new SendSession();
                lock (s_Lock) {
                    if (client.Closing || client.DrainRequested) {
                        ws.Dispose();
                        break;
                    }
                    client.Ws = ws;
                    client.Session = session;
                }
                try {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    await ws.ConnectAsync(new Uri(client.Url), cts.Token);
                    if (client.Closing) {
                        break;
                    }
                    Interlocked.Exchange(ref client.State, 1);
                    Enqueue(client.Id, "state", "connected");
                    everConnected = true;
                    failures = 0;  // link recovered: reset the retry budget
                    var buffer = new byte[64 * 1024];
                    bool closedByServer = false;
                    while (!client.Closing) {
                        var sb = new StringBuilder();
                        WebSocketReceiveResult result;
                        do {
                            result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                            if (result.MessageType == WebSocketMessageType.Close) {
                                closedByServer = true;  // graceful close by the server
                                break;
                            }
                            if (result.Count > 0) {
                                sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                            }
                        } while (!result.EndOfMessage);
                        if (closedByServer) {
                            break;
                        }
                        if (sb.Length > 0) {
                            Enqueue(client.Id, "message", sb.ToString());
                        }
                    }
                }
                catch (Exception ex) {
                    // connect failure / io error / disposed: surface the reason so
                    // the host log shows why a client keeps failing (refused /
                    // timeout / tls / proxy / ...).
                    Log?.Invoke("[csharp] wsclient connect/recv error (id=" + client.Id + ", url=" + client.Url + "): " + ex.GetType().Name + ": " + ex.Message);
                }
                finally {
                    lock (s_Lock) {
                        session.Retired = true;
                        session.FinishDrain("failed", "WebSocket session ended before send drain completed.");
                        if (ReferenceEquals(client.Ws, ws)) {
                            client.Ws = null;
                        }
                    }
                    try { ws.Dispose(); } catch { }
                }
                // The link dropped (connect failure, io error or server close).
                if (client.Closing || client.DrainRequested) {
                    break;
                }
                if (client.ReconnectCount <= 0) {
                    break;  // one-shot mode: terminal report below, no retry
                }
                failures = failures + 1;
                if (failures >= client.ReconnectCount) {
                    break;  // budget exhausted: logical disconnect for the upper layer
                }
                Interlocked.Exchange(ref client.State, 4);
                Enqueue(client.Id, "state", "reconnecting");
                try {
                    await Task.Delay(500);  // short backoff, local links fail fast
                }
                catch (Exception) {
                    break;
                }
            }
            // Terminal report (same wording as the original one-shot loop):
            // explicit close -> disconnected; budget exhausted -> failed;
            // one-shot never connected -> failed; one-shot drop -> disconnected.
            string terminal;
            if (client.Closing) {
                terminal = "disconnected";
            }
            else if (client.ReconnectCount > 0) {
                terminal = "failed";
            }
            else {
                terminal = everConnected ? "disconnected" : "failed";
            }
            Interlocked.Exchange(ref client.State, terminal == "failed" ? 3 : 2);
            Enqueue(client.Id, "state", terminal);
            lock (s_Lock) {
                if (s_Clients.TryGetValue(client.Id, out var cur) && cur == client) {
                    s_Clients.Remove(client.Id);
                }
            }
        }

        public static bool Send(string id, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message ?? string.Empty);
            lock (s_Lock) {
                if (!s_Clients.TryGetValue(id, out var client) ||
                    client.Closing || client.DrainRequested ||
                    client.Ws is not { State: WebSocketState.Open } ||
                    client.Session == null || client.Session.Retired ||
                    client.Session.SendError.Length != 0) {
                    return false;
                }
                var ws = client.Ws;
                var session = client.Session;
                var previous = session.Tail;
                session.Pending++;
                try {
                    session.Tail = Task.Run(() => SendQueuedAsync(id, ws, session, previous, bytes));
                }
                catch {
                    session.Pending--;
                    throw;
                }
                return true;
            }
        }

        private static async Task SendQueuedAsync(string id, ClientWebSocket ws,
            SendSession session, Task previous, byte[] bytes)
        {
            string error = string.Empty;
            try {
                await previous.ConfigureAwait(false);
                lock (s_Lock) {
                    if (session.Retired || session.SendError.Length != 0) {
                        throw new OperationCanceledException("WebSocket send session is no longer writable.");
                    }
                }
                await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text,
                    true, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex) {
                error = ex.Message;
            }
            finally {
                lock (s_Lock) {
                    session.Pending--;
                    if (error.Length != 0) {
                        if (session.SendError.Length == 0) {
                            session.SendError = error;
                        }
                        session.FinishDrain("failed", session.SendError);
                    }
                    else if (session.Pending == 0 && !session.Retired) {
                        session.FinishDrain("succeeded", string.Empty);
                    }
                }
            }
            if (error.Length != 0) {
                try {
                    Log?.Invoke("[csharp] wsclient_send failed (id=" + id + "): " + error);
                }
                catch {
                    // Diagnostics must not fault the send chain.
                }
            }
        }

        // Drains accepted sends without closing the receive side on success.
        public static bool DrainSend(string id, int timeoutMs = 10000)
        {
            if (string.IsNullOrEmpty(id) || timeoutMs <= 0) {
                return false;
            }
            lock (s_Lock) {
                if (!s_Clients.TryGetValue(id, out var client) ||
                    client.Closing ||
                    client.Ws is not { State: WebSocketState.Open } ||
                    client.Session == null || client.Session.Retired ||
                    client.Session.SendError.Length != 0) {
                    return false;
                }
                if (client.DrainRequested) {
                    return true;
                }
                var ws = client.Ws;
                var session = client.Session;
                client.DrainRequested = true;
                session.DrainState = "draining";
                if (session.Pending == 0) {
                    session.FinishDrain("succeeded", string.Empty);
                }
                else {
                    session.DrainTimer = new Timer(_ => {
                        lock (s_Lock) {
                            if (session.DrainState != "draining") {
                                return;
                            }
                            session.Retired = true;
                            client.Closing = true;
                            session.FinishDrain("timed_out", "WebSocket send drain timed out.");
                        }
                        // Abort only the captured socket, never a replacement session.
                        try {
                            ws.Abort();
                        }
                        catch (Exception) {
                            // The receive loop may already have disposed the socket.
                        }
                    }, null, timeoutMs, Timeout.Infinite);
                }
                return true;
            }
        }

        public static Dictionary<string, object> GetSendDrainStatus(string id)
        {
            lock (s_Lock) {
                long generation = 0L;
                string state = "unknown";
                string error = string.Empty;
                if (!string.IsNullOrEmpty(id) &&
                    s_Clients.TryGetValue(id, out var client)) {
                    generation = client.Generation;
                    state = client.Session?.DrainState ?? "not_requested";
                    error = client.Session?.DrainError ?? string.Empty;
                }
                return new Dictionary<string, object> {
                    ["state"] = state,
                    ["error"] = error,
                    ["generation"] = generation
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
                            ? (session.DrainState, session.DrainError)
                            : ("not_requested", string.Empty);
                        if (status.State == "draining" && session != null) {
                            // Pin this session even if the ID is replaced while waiting.
                            completion = session.Completion.Task;
                        }
                    }
                }
            }
            bool waitTimedOut = false;
            if (completion != null) {
                if (completion.Wait(waitTimeoutMs)) {
                    status = completion.GetAwaiter().GetResult();
                }
                else {
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

        public static bool Close(string id)
        {
            Client? client;
            lock (s_Lock) {
                s_Clients.TryGetValue(id, out client);
            }
            if (null == client) {
                return false;
            }
            CloseClient(client);
            return true;
        }

        private static void CloseClient(Client client)
        {
            ClientWebSocket? ws;
            lock (s_Lock) {
                client.Closing = true;
                ws = client.Ws;
                if (client.Session != null) {
                    client.Session.Retired = true;
                    client.Session.FinishDrain("cancelled", "WebSocket client closed before send drain completed.");
                }
            }
            if (null != ws) {
                Task.Run(async () => {
                    try {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "client closed", cts.Token);
                    }
                    catch (Exception) {
                        // receive loop notices and exits by itself
                    }
                });
            }
        }

        public static int CloseAll()
        {
            List<Client> clients;
            lock (s_Lock) {
                clients = new List<Client>(s_Clients.Values);
            }
            foreach (var client in clients) {
                CloseClient(client);
            }
            return clients.Count;
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
                if (!s_Clients.TryGetValue(id, out var client)) {
                    return "unknown";
                }
                return client.State switch {
                    0 => "connecting",
                    1 => "connected",
                    2 => "disconnected",
                    3 => "failed",
                    4 => "reconnecting",
                    _ => "unknown",
                };
            }
        }

        public static List<string> List()
        {
            var result = new List<string>();
            lock (s_Lock) {
                foreach (var client in s_Clients.Values) {
                    result.Add(client.Id + "|" + GetState(client.Id) + "|" + client.Url);
                }
            }
            return result;
        }

        private static void Enqueue(string id, string kind, string payload)
        {
            s_Queue.Enqueue(Tuple.Create(id, kind, payload));
        }

        /// <summary>
        /// Drains queued events on the calling thread, invoking the host
        /// Dispatch action per event. Hosts call this on their main thread
        /// (CefDotnetApp: every heartbeat; AgentCore: every agent tick).
        /// </summary>
        public static int DrainQueue(int maxCount)
        {
            if (maxCount <= 0) {
                maxCount = 100;
            }
            int drained = 0;
            while (drained < maxCount && s_Queue.TryDequeue(out var item)) {
                ++drained;
                Dispatch?.Invoke(item.Item1, item.Item2, item.Item3);
            }
            return drained;
        }
    }
}
