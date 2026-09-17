using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgentCore.Core
{
    /// <summary>
    /// Standalone local HTTP reverse proxy (HttpListener based), started and
    /// stopped from dsl (httpproxy_start_server / httpproxy_stop_server, see
    /// ScriptApi/HttpProxyApi.cs; mirrors the ws_start_server family).
    ///
    /// Purpose: pages served from a local origin (e.g. http://localhost:8082)
    /// cannot fetch upstream APIs directly when the upstream does not answer
    /// the CORS preflight for that origin. The page fetches this server
    /// instead, with the absolute upstream URL in the 'target' query
    /// parameter:
    ///
    ///   http://localhost:&lt;port&gt;/&lt;any-path&gt;?target=&lt;absolute-http(s)-url&gt;
    ///
    /// WebSocket upgrades on the same listener are tunnelled the same way:
    ///
    ///   ws://localhost:&lt;port&gt;/&lt;any-path&gt;?target=&lt;absolute-ws(s)-url&gt;
    ///
    /// This is our own service formally declaring the cross-origin permission
    /// (Access-Control-Allow-Origin echoes the request origin), while the
    /// forwarding happens natively (HttpClient, no CORS rules apply).
    ///
    /// Semantics (generic passthrough):
    ///   - OPTIONS: answered as a CORS preflight (204 + allow headers).
    ///   - any other verb: body verbatim, request headers forwarded except
    ///     hop-by-hop / local-context ones, response status + headers copied,
    ///     body streamed chunk by chunk (SSE friendly, per-chunk flush).
    ///   - cookies set by an upstream are kept in a shared cookie container
    ///     and resent on later proxied calls to the same domain (approximates
    ///     the pages' credentials:'include' behaviour).
    ///   - no upstream restriction (user decision 2026-09-17: local pages may
    ///     reach any external site through this proxy).
    ///
    /// Exposure: the listener binds loopback only. Public pages are kept away
    /// by LNA: their preflight carries the PNA header and this server never
    /// answers Access-Control-Allow-Private-Network, so Chromium blocks the
    /// request before it reaches us.
    /// </summary>
    public sealed class HttpProxyServer : IDisposable
    {
        private HttpListener? _listener;
        private CancellationTokenSource? _cts;
        private int _port;
        private volatile bool _isRunning;
        private long _requestCount;

        /// <summary>Gets whether the server is currently running.</summary>
        public bool IsRunning => _isRunning;

        /// <summary>Gets the port the server is listening on.</summary>
        public int Port => _port;

        /// <summary>Total number of accepted requests since Start.</summary>
        public long RequestCount => Interlocked.Read(ref _requestCount);

        // Shared across all proxy instances: one connection pool, one cookie
        // container (upstream cookies are domain scoped, sharing is safe).
        private static readonly HttpClient s_Client = CreateClient();

        private static HttpClient CreateClient()
        {
            var handler = new HttpClientHandler {
                CookieContainer = new CookieContainer(),
                UseProxy = false,  // never route through a system proxy
                AllowAutoRedirect = true,
            };
            // SSE streams can be long lived; each request gets its own 30min cts.
            return new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        }

        /// <summary>
        /// Starts the proxy server on the specified port (loopback only).
        /// </summary>
        public bool Start(int port)
        {
            if (_isRunning) {
                AgentCore.Instance.Logger.Warning("HttpProxy server is already running");
                return false;
            }
            try {
                _port = port;
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://localhost:{port}/");
                _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
                _listener.Start();
                _isRunning = true;
                _cts = new CancellationTokenSource();
                _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
                AgentCore.Instance.Logger.Info($"HttpProxy server started on port {port}");
                return true;
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error($"Failed to start HttpProxy server: {ex.Message}\nStack: {ex.StackTrace}");
                _isRunning = false;
                return false;
            }
        }

        /// <summary>
        /// Stops the proxy server.
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) {
                return;
            }
            try {
                _isRunning = false;
                _cts?.Cancel();
                _listener?.Stop();
                _listener?.Close();
                AgentCore.Instance.Logger.Info("HttpProxy server stopped");
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error($"Error stopping HttpProxy server: {ex.Message}\nStack: {ex.StackTrace}");
            }
        }

        private async Task AcceptLoopAsync(CancellationToken ct)
        {
            var listener = _listener!;
            while (_isRunning && !ct.IsCancellationRequested) {
                try {
                    if (!listener.IsListening) {
                        break;
                    }
                    var context = await listener.GetContextAsync();
                    Interlocked.Increment(ref _requestCount);
                    _ = Task.Run(() => HandleAsync(context));
                }
                catch (HttpListenerException) when (ct.IsCancellationRequested) {
                    break;
                }
                catch (Exception ex) {
                    AgentCore.Instance.Logger.Error($"HttpProxy accept error: {ex.Message}\nStack: {ex.StackTrace}");
                }
            }
        }

        private static async Task HandleAsync(HttpListenerContext context)
        {
            HttpListenerRequest req = context.Request;
            HttpListenerResponse resp = context.Response;
            string? origin = req.Headers["Origin"];
            try {
                if (req.IsWebSocketRequest) {
                    // WebSocket upgrade: tunnel to the ws(s) 'target'.
                    await HandleWebSocketAsync(context);
                    return;
                }
                if (req.HttpMethod == "OPTIONS") {
                    // CORS preflight (local page -> local proxy: no PNA involved;
                    // public pages are blocked by LNA before reaching us).
                    resp.StatusCode = 204;
                    resp.AddHeader("Access-Control-Allow-Origin", string.IsNullOrEmpty(origin) ? "*" : origin);
                    resp.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS");
                    string? reqHeaders = req.Headers["Access-Control-Request-Headers"];
                    resp.AddHeader("Access-Control-Allow-Headers", string.IsNullOrEmpty(reqHeaders)
                        ? "Content-Type, Authorization, Accept, X-Anydev-Host, X-Username, x-knot-api-token, x-knot-token"
                        : reqHeaders);
                    resp.AddHeader("Access-Control-Max-Age", "86400");
                    resp.Close();
                    return;
                }

                string? target = req.QueryString["target"];
                if (string.IsNullOrEmpty(target) ||
                    (!target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                     !target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))) {
                    resp.StatusCode = 400;
                    byte[] bad = Encoding.UTF8.GetBytes("httpproxy: missing or invalid 'target' query parameter (expected an absolute http(s) url)");
                    resp.ContentLength64 = bad.Length;
                    resp.ContentType = "text/plain; charset=utf-8";
                    resp.OutputStream.Write(bad, 0, bad.Length);
                    resp.Close();
                    return;
                }

                // Body verbatim.
                byte[] body;
                using (var ms = new MemoryStream()) {
                    await req.InputStream.CopyToAsync(ms);
                    body = ms.ToArray();
                }
                using var upstream = new HttpRequestMessage(new HttpMethod(req.HttpMethod), target) {
                    Content = new ByteArrayContent(body),
                };
                // Forward every header except hop-by-hop / local-context ones.
                foreach (string? name in req.Headers) {
                    if (string.IsNullOrEmpty(name) || IsHopHeader(name)) {
                        continue;
                    }
                    string?[] values = req.Headers.GetValues(name) ?? Array.Empty<string?>();
                    if (!upstream.Content.Headers.TryAddWithoutValidation(name, values)) {
                        upstream.Headers.TryAddWithoutValidation(name, values);
                    }
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
                using var upstreamResp = await s_Client.SendAsync(upstream,
                    HttpCompletionOption.ResponseHeadersRead, cts.Token);

                resp.StatusCode = (int)upstreamResp.StatusCode;
                resp.ContentType = upstreamResp.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
                foreach (var h in upstreamResp.Content.Headers) {
                    if (h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
                        h.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }
                    try { resp.AddHeader(h.Key, string.Join(", ", h.Value)); } catch { }
                }
                foreach (var h in upstreamResp.Headers) {
                    try { resp.AddHeader(h.Key, string.Join(", ", h.Value)); } catch { }
                }
                resp.AddHeader("Access-Control-Allow-Origin", string.IsNullOrEmpty(origin) ? "*" : origin);
                // Stream the body chunk by chunk (SSE needs per-event flush).
                resp.SendChunked = true;
                using (var src = await upstreamResp.Content.ReadAsStreamAsync(cts.Token)) {
                    var buffer = new byte[16 * 1024];
                    int read;
                    while ((read = await src.ReadAsync(buffer, 0, buffer.Length, cts.Token)) > 0) {
                        await resp.OutputStream.WriteAsync(buffer, 0, read, cts.Token);
                        await resp.OutputStream.FlushAsync(cts.Token);
                    }
                }
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error($"httpproxy error: {ex.Message}\nStack: {ex.StackTrace}");
                try {
                    // When the failure happened mid-stream the headers are
                    // already out and these setters throw - the catch below
                    // just closes the response then.
                    resp.StatusCode = 502;
                    resp.ContentType = "text/plain; charset=utf-8";
                    resp.AddHeader("Access-Control-Allow-Origin", string.IsNullOrEmpty(origin) ? "*" : origin);
                    byte[] err = Encoding.UTF8.GetBytes("httpproxy error: " + ex.Message);
                    resp.ContentLength64 = err.Length;
                    resp.OutputStream.Write(err, 0, err.Length);
                }
                catch { }
            }
            finally {
                try { resp.Close(); } catch { }
            }
        }

        // WebSocket tunnel: the local page opens
        //   ws://localhost:<port>/<any-path>?target=<absolute-ws(s)-url>
        // and every frame is relayed verbatim to/from the upstream socket.
        // Serves pages that need a browser-side WebSocket object against an
        // upstream that rejects the local origin or expects extra handshake
        // headers (the browser ws API cannot set them). Query params:
        //   target  (required) absolute ws:// or wss:// upstream url
        //   origin  (optional) custom Origin header for the upstream handshake
        // Sub-protocols requested by the client are forwarded and the one
        // selected upstream is echoed back to the client; Authorization and
        // Cookie handshake headers are forwarded. The local Origin/Referer
        // are never forwarded (localhost values are meaningless upstream).
        private static async Task HandleWebSocketAsync(HttpListenerContext context)
        {
            HttpListenerRequest req = context.Request;
            HttpListenerResponse resp = context.Response;
            try {
                string? target = req.QueryString["target"];
                if (string.IsNullOrEmpty(target) ||
                    (!target.StartsWith("ws://", StringComparison.OrdinalIgnoreCase) &&
                     !target.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))) {
                    resp.StatusCode = 400;
                    byte[] bad = Encoding.UTF8.GetBytes("httpproxy ws: missing or invalid 'target' query parameter (expected an absolute ws(s) url)");
                    resp.ContentLength64 = bad.Length;
                    resp.ContentType = "text/plain; charset=utf-8";
                    resp.OutputStream.Write(bad, 0, bad.Length);
                    resp.Close();
                    return;
                }

                List<string> requestedProtocols = new();
                string? protocols = req.Headers["Sec-WebSocket-Protocol"];
                if (!string.IsNullOrEmpty(protocols)) {
                    foreach (string p in protocols.Split(',')) {
                        string s = p.Trim();
                        if (s.Length > 0) {
                            requestedProtocols.Add(s);
                        }
                    }
                }

                using var upstream = new ClientWebSocket();
                foreach (string p in requestedProtocols) {
                    upstream.Options.AddSubProtocol(p);
                }
                foreach (string name in new[] { "Authorization", "Cookie" }) {
                    string? value = req.Headers[name];
                    if (!string.IsNullOrEmpty(value)) {
                        upstream.Options.SetRequestHeader(name, value);
                    }
                }
                string? customOrigin = req.QueryString["origin"];
                if (!string.IsNullOrEmpty(customOrigin)) {
                    upstream.Options.SetRequestHeader("Origin", customOrigin);
                }

                await upstream.ConnectAsync(new Uri(target), CancellationToken.None);

                // Accept locally only after the upstream handshake succeeded,
                // echoing the sub-protocol the upstream selected (only when
                // the client actually requested sub-protocols).
                string? selectedProtocol = requestedProtocols.Count > 0 ? upstream.SubProtocol : null;
                HttpListenerWebSocketContext localContext = await context.AcceptWebSocketAsync(selectedProtocol);
                using WebSocket local = localContext.WebSocket;

                await TunnelAsync(local, upstream);
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error($"httpproxy ws error: {ex.Message}\nStack: {ex.StackTrace}");
                // The upgrade may already have happened (no status line can
                // be sent any more) - just make sure the socket is torn down.
                try { resp.Close(); } catch { }
            }
        }

        // Relay frames both ways until either side closes or errors. A close
        // on one side is echoed to the other; anything still pending after a
        // short grace period is aborted so no pump stays blocked.
        private static async Task TunnelAsync(WebSocket local, WebSocket upstream)
        {
            using var cts = new CancellationTokenSource();
            Task clientToUpstream = PumpAsync(local, upstream, cts.Token);
            Task upstreamToClient = PumpAsync(upstream, local, cts.Token);
            Task both = Task.WhenAll(clientToUpstream, upstreamToClient);
            Task finished = await Task.WhenAny(both, Task.Delay(TimeSpan.FromSeconds(5)));
            if (finished != both) {
                cts.Cancel();
                try { local.Abort(); } catch { }
                try { upstream.Abort(); } catch { }
            }
            try { await both; } catch { }
        }

        private static async Task PumpAsync(WebSocket source, WebSocket dest, CancellationToken ct)
        {
            var buffer = new byte[16 * 1024];
            while (true) {
                WebSocketReceiveResult result = await source.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (result.MessageType == WebSocketMessageType.Close) {
                    await dest.CloseOutputAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                        result.CloseStatusDescription, ct);
                    return;
                }
                await dest.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count),
                    result.MessageType, result.EndOfMessage, ct);
            }
        }

        // Hop-by-hop or local-context headers that must not be forwarded.
        private static bool IsHopHeader(string name)
        {
            switch (name.ToLowerInvariant()) {
                case "host":
                case "connection":
                case "content-length":
                case "transfer-encoding":
                case "keep-alive":
                case "origin":
                case "referer":
                case "expect":
                case "proxy-connection":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Disposes the server resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }
}
