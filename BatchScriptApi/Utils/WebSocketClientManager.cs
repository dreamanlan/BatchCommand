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
        private sealed class Client
        {
            public string Id = string.Empty;
            public string Url = string.Empty;
            public ClientWebSocket? Ws;
            public Thread? Thread;
            public volatile bool Closing;
            public long State;  // 0=connecting 1=connected 2=disconnected 3=failed 4=reconnecting
            public int ReconnectCount;  // link-layer retry budget (0 = one-shot)
        }

        private static readonly object s_Lock = new object();
        private static readonly Dictionary<string, Client> s_Clients = new Dictionary<string, Client>();
        private static int s_AutoId = 0;

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
                var client = new Client { Id = id, Url = url, ReconnectCount = reconnectCount };
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
                client.Ws = ws;
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
                    try { ws.Dispose(); } catch { }
                    if (ReferenceEquals(client.Ws, ws)) {
                        client.Ws = null;
                    }
                }
                // The link dropped (connect failure, io error or server close).
                if (client.Closing) {
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
            Client? client;
            lock (s_Lock) {
                s_Clients.TryGetValue(id, out client);
            }
            if (null == client || client.Ws is not { State: WebSocketState.Open }) {
                return false;
            }
            var ws = client.Ws;
            var bytes = Encoding.UTF8.GetBytes(message ?? string.Empty);
            // Fire-and-forget on a pool thread: a websocket allows a single
            // outstanding send, the per-client lock serializes wsclient_send
            // calls; never blocks the calling (main) thread.
            Task.Run(() => {
                lock (client) {
                    try {
                        ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None)
                          .GetAwaiter().GetResult();
                    }
                    catch (Exception ex) {
                        Log?.Invoke("[csharp] wsclient_send failed (id=" + id + "): " + ex.Message);
                    }
                }
            });
            return true;
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
            client.Closing = true;
            var ws = client.Ws;
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
