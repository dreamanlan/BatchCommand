using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AbstractAgent;
using ScriptableFramework;

namespace AgentCore.Core
{
    /// <summary>
    /// Generic local loopback HTTP redirect capturer. Provider-agnostic: works
    /// for any OAuth 2.0 / OIDC "native app" flow (RFC 8252, e.g. Google) as well
    /// as any custom sign-in / callback scheme. Flow:
    ///   1. start a loopback HTTP server on 127.0.0.1 (auto free port),
    ///   2. open the system browser to an authorization URL whose redirect_uri
    ///      points back to this server,
    ///   3. capture the first request that carries query parameters and hand back
    ///      the full raw query string verbatim (nothing extracted or dropped, so
    ///      code / token / state / any custom params all survive),
    ///   4. deliver it to the script via the "http_auth_callback" CEF
    ///      message (url, tag, token_or_response),
    ///   5. self destroy once captured (or on timeout / manual stop).
    /// Note: implicit-flow fragments (#access_token=...) never reach a server,
    /// so callers must use response_type=code (or any query-based redirect).
    /// </summary>
    public sealed class HttpAuthServerService
    {
        public const string CallbackMessage = "http_auth_callback";
        private const int c_defaultTimeoutSeconds = 300;

        private static readonly HttpAuthServerService s_instance = new HttpAuthServerService();
        public static HttpAuthServerService Instance => s_instance;

        private sealed class ServerEntry
        {
            public HttpListener Listener = null!;
            public int Port;
            public string Url = string.Empty;   // original url arg, echoed to callback
            public string Tag = string.Empty;
            public CancellationTokenSource Cts = null!;
        }

        // Keyed by listening port; guarantees single completion per server.
        private readonly ConcurrentDictionary<int, ServerEntry> _servers = new();

        /// <summary>
        /// Starts a loopback auth server and opens the system browser.
        /// listenPort: 0 to auto-pick a free port, otherwise the fixed port.
        /// urlFormat: authorization URL with a "%redirect_url%" placeholder that is
        ///   replaced by the URL-encoded redirect_uri (http://127.0.0.1:{port}/).
        /// tag: opaque correlation string echoed back to the callback.
        /// Returns the actual listening port (>0), or -1 on failure.
        /// </summary>
        public int Start(int listenPort, string urlFormat, string tag)
        {
            try {
                int port = listenPort > 0 ? listenPort : PickFreePort();
                string redirectUri = $"http://127.0.0.1:{port}/";

                var listener = new HttpListener();
                listener.Prefixes.Add(redirectUri);
                listener.Start();

                var entry = new ServerEntry {
                    Listener = listener,
                    Port = port,
                    Url = urlFormat,
                    Tag = tag,
                    Cts = new CancellationTokenSource()
                };
                if (!_servers.TryAdd(port, entry)) {
                    try { listener.Close(); } catch { }
                    AgentCore.Instance.Logger.Error($"[HttpAuthServer] port {port} already used by another auth server");
                    return -1;
                }

                string authUrl = BuildAuthUrl(urlFormat, redirectUri);
                Task.Run(() => AcceptLoopAsync(entry));
                Task.Run(() => WatchdogAsync(entry, c_defaultTimeoutSeconds));
                OpenSystemBrowser(authUrl);

                AgentCore.Instance.Logger.Info($"[HttpAuthServer] started on {redirectUri} tag={tag}");
                return port;
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error($"[HttpAuthServer] start failed: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Stops a running auth server. Delivers "error: cancelled" to the
        /// callback so the script always receives exactly one terminal event.
        /// Returns true if a server was found for the port.
        /// </summary>
        public bool Stop(int port)
        {
            if (_servers.TryGetValue(port, out var entry)) {
                TryComplete(entry, "error: cancelled");
                return true;
            }
            return false;
        }

        private async Task AcceptLoopAsync(ServerEntry entry)
        {
            while (!entry.Cts.IsCancellationRequested && entry.Listener.IsListening) {
                HttpListenerContext ctx;
                try {
                    ctx = await entry.Listener.GetContextAsync().ConfigureAwait(false);
                }
                catch (Exception) {
                    break; // listener stopped / disposed
                }

                try {
                    // Generic capture: the first request carrying query parameters
                    // is the terminal redirect. Requests without a query (favicon,
                    // health probes, fragment-only implicit redirects) are ignored
                    // so the server keeps waiting. The full raw query string is
                    // handed back verbatim (leading '?' stripped) so the caller can
                    // parse code / token / state / any provider-specific param
                    // without loss.
                    string rawQuery = ctx.Request.Url?.Query ?? string.Empty;
                    if (rawQuery.Length == 0 || rawQuery == "?") {
                        WriteResponse(ctx, 200, "<html><body>Waiting for authorization...</body></html>");
                        continue;
                    }
                    string response = rawQuery.StartsWith("?") ? rawQuery.Substring(1) : rawQuery;
                    WriteResponse(ctx, 200, "<html><body>Login complete. You can close this window.</body></html>");
                    TryComplete(entry, response);
                    break;
                }
                catch (Exception ex) {
                    AgentCore.Instance.Logger.Error($"[HttpAuthServer] request handling failed: {ex.Message}");
                    try { WriteResponse(ctx, 500, "<html><body>Internal error.</body></html>"); } catch { }
                }
            }
        }

        private async Task WatchdogAsync(ServerEntry entry, int timeoutSeconds)
        {
            try {
                await Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), entry.Cts.Token).ConfigureAwait(false);
            }
            catch (Exception) {
                return; // cancelled because the server already completed
            }
            TryComplete(entry, "error: timeout");
        }

        // Removes the server from the table (single-winner), delivers the result
        // via the CEF message, then tears the listener down.
        private void TryComplete(ServerEntry entry, string response)
        {
            if (!_servers.TryRemove(entry.Port, out _))
                return; // already completed by another path

            try {
                var nativeApi = AgentCore.Instance.GetNativeApi();
                if (nativeApi != null) {
                    nativeApi.EnqueueCefMessage(CallbackMessage,
                        new BoxedValue[] { entry.Url, entry.Tag, response });
                }
                else {
                    AgentCore.Instance.Logger.Warning("[HttpAuthServer] nativeApi not available, callback dropped");
                }
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error($"[HttpAuthServer] deliver failed: {ex.Message}");
            }
            finally {
                CloseEntry(entry);
            }
        }

        private void CloseEntry(ServerEntry entry)
        {
            try { entry.Cts.Cancel(); } catch { }
            try { entry.Listener.Stop(); } catch { }
            try { entry.Listener.Close(); } catch { }
            try { entry.Cts.Dispose(); } catch { }
        }

        // Picks a free loopback TCP port. HttpListener does not support prefix
        // port 0, so we probe with a throwaway TcpListener. Small TOCTOU window
        // is acceptable for local OAuth capture.
        private static int PickFreePort()
        {
            var probe = new TcpListener(IPAddress.Loopback, 0);
            probe.Start();
            int port = ((IPEndPoint)probe.LocalEndpoint).Port;
            probe.Stop();
            return port;
        }

        // Substitutes the URL-encoded redirect_uri into the "%redirect_url%"
        // placeholder (Windows env-var style, consistent with the rest of the
        // project) via plain string replacement (every occurrence). If the
        // placeholder is absent the URL is opened as-is.
        private static string BuildAuthUrl(string urlFormat, string redirectUri)
        {
            string encoded = Uri.EscapeDataString(redirectUri);
            return urlFormat.Replace("%redirect_url%", encoded);
        }

        private static void WriteResponse(HttpListenerContext ctx, int status, string html)
        {
            byte[] buf = Encoding.UTF8.GetBytes(html);
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "text/html; charset=utf-8";
            ctx.Response.ContentLength64 = buf.Length;
            ctx.Response.OutputStream.Write(buf, 0, buf.Length);
            ctx.Response.OutputStream.Close();
        }

        private static void OpenSystemBrowser(string url)
        {
            try {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                    Process.Start("open", url);
                }
                else {
                    Process.Start("xdg-open", url);
                }
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error($"[HttpAuthServer] open browser failed: {ex.Message}");
            }
        }
    }
}
