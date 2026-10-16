using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BatchCommand.Utils;

namespace AgentCore.Core
{
    /// <summary>
    /// Streamable HTTP transport for the stateless MCP 2026-07-28 revision:
    /// no initialize handshake, no Mcp-Session-Id, no GET stream. Every POST
    /// carries the required MCP-Protocol-Version / Mcp-Method (and Mcp-Name
    /// where applicable) headers mirrored from the request body. After
    /// connecting, a subscriptions/listen stream is opened for the change
    /// notifications the client opted in to (tools/prompts/resources
    /// list_changed); it is re-sent automatically when the stream ends.
    /// </summary>
    internal class ModernHttpTransport : IMcpTransport
    {
        private readonly string _url;
        private readonly HttpClient _httpClient;
        private readonly HttpClient _streamClient;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _lockTimeout;
        private bool _connected;
        private string _protocolVersion;
        private CancellationTokenSource? _listenCts;
        private Task? _listenTask;

        public bool IsConnected => _connected;

        public string ProtocolVersion => _protocolVersion;

        /// <summary>
        /// Raised for notifications received on the subscriptions/listen
        /// stream (acknowledgment plus the opted-in list_changed types).
        /// </summary>
        public event Action<string>? ServerNotification;

        public ModernHttpTransport(string url, Dictionary<string, string>? headers = null, int timeoutMs = 300000, string? protocolVersion = null)
        {
            _protocolVersion = protocolVersion ?? McpProtocol.V2026_07_28;
            _lockTimeout = TimeSpan.FromMilliseconds(timeoutMs + 5000);
            _url = url.TrimEnd('/');
            var socketHandler = new SocketsHttpHandler {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
                ConnectTimeout = TimeSpan.FromSeconds(10)
            };
            _httpClient = new HttpClient(RedirectHandler.Create(socketHandler)) {
                Timeout = TimeSpan.FromMilliseconds(timeoutMs)
            };
            // Dedicated client for the listen stream: no HTTP-level timeout
            var streamSocketHandler = new SocketsHttpHandler {
                PooledConnectionLifetime = Timeout.InfiniteTimeSpan,
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                ConnectTimeout = TimeSpan.FromSeconds(10)
            };
            _streamClient = new HttpClient(RedirectHandler.Create(streamSocketHandler)) {
                Timeout = Timeout.InfiniteTimeSpan
            };
            if (headers != null) {
                foreach (var kv in headers) {
                    _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(kv.Key, kv.Value);
                    _streamClient.DefaultRequestHeaders.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }
        }

        /// <summary>
        /// Connects via server/discover (the 2026-07-28 revision has no
        /// initialize handshake). On UnsupportedProtocolVersionError (-32022)
        /// retries once with a mutually supported modern version from the
        /// error's data.supported list.
        /// </summary>
        public async Task ConnectAsync()
        {
            string req = McpProtocol.BuildDiscoverRequest(0, _protocolVersion);
            string resp = await PostAsync(req);
            var cls = McpProtocol.ClassifyEra(resp, out var supported);
            if (cls == McpEraClassification.ModernError) {
                string? modern = supported.FirstOrDefault(McpProtocol.IsModernVersion);
                if (modern != null && modern != _protocolVersion) {
                    _protocolVersion = modern;
                    req = McpProtocol.BuildDiscoverRequest(1, _protocolVersion);
                    resp = await PostAsync(req);
                    cls = McpProtocol.ClassifyEra(resp, out supported);
                }
                else if (modern == null && supported.Any(v => McpProtocol.KnownVersions.Contains(v))) {
                    throw new InvalidOperationException($"server at {_url} supports no 2026-07-28-era version (supported: {string.Join(", ", supported)}); do not force protocol_version 2026-07-28 for this server");
                }
            }
            if (cls != McpEraClassification.ModernResult) {
                string? rpcError = JsonRpcHelper.ParseError(resp);
                throw new InvalidOperationException(rpcError != null
                    ? $"MCP server at {_url} rejected server/discover (protocol {_protocolVersion}): {rpcError}"
                    : $"MCP server at {_url} did not return a server/discover result");
            }
            _connected = true;
            StartListenStream();
        }

        /// <summary>
        /// Starts the subscriptions/listen stream (2026-07-28): a long-lived
        /// POST whose response stream carries the opted-in change notifications
        /// (toolsListChanged / promptsListChanged / resourcesListChanged).
        /// The server holds no subscription state, so the listen request is
        /// re-sent automatically whenever the stream ends (graceful closure or
        /// transport drop) while the transport stays connected.
        /// </summary>
        private void StartListenStream()
        {
            try { _listenCts?.Cancel(); } catch { }
            try { _listenCts?.Dispose(); } catch { }
            _listenCts = new CancellationTokenSource();
            var token = _listenCts.Token;
            _listenTask = Task.Run(() => ListenLoopAsync(token), token);
        }

        private async Task ListenLoopAsync(CancellationToken token)
        {
            int backoffMs = 500;
            while (!token.IsCancellationRequested && _connected) {
                try {
                    string body = McpProtocol.BuildDecoratedRequest("subscriptions/listen",
                        new { notifications = new { toolsListChanged = true, promptsListChanged = true, resourcesListChanged = true } },
                        2, _protocolVersion);
                    using var request = new HttpRequestMessage(HttpMethod.Post, _url);
                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                    request.Headers.TryAddWithoutValidation("Accept", "text/event-stream");
                    request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", _protocolVersion);
                    request.Headers.TryAddWithoutValidation("Mcp-Method", "subscriptions/listen");
                    using var response = await _streamClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                    if ((int)response.StatusCode == 405 || (int)response.StatusCode == 404) {
                        AgentCore.Instance.Logger.Info($"[ModernHttp] server at {_url} does not support subscriptions/listen ({(int)response.StatusCode}); change notifications disabled");
                        return;
                    }
                    if (!response.IsSuccessStatusCode) {
                        AgentCore.Instance.Logger.Info($"[ModernHttp] listen stream returned {(int)response.StatusCode}, retrying in {backoffMs}ms");
                        await Task.Delay(backoffMs, token);
                        backoffMs = Math.Min(backoffMs * 2, 5000);
                        continue;
                    }
                    backoffMs = 500;
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    var eventData = new StringBuilder();
                    string? line;
                    while (!token.IsCancellationRequested && (line = await reader.ReadLineAsync()) != null) {
                        if (line.Length == 0) {
                            if (eventData.Length > 0) {
                                string payload = eventData.ToString();
                                eventData.Clear();
                                HandleListenMessage(payload);
                            }
                        }
                        else if (line.StartsWith("data:", StringComparison.Ordinal)) {
                            string val = line.Substring(5);
                            if (val.StartsWith(" ", StringComparison.Ordinal))
                                val = val.Substring(1);
                            if (eventData.Length > 0) eventData.Append('\n');
                            eventData.Append(val);
                        }
                        // SSE comments (:) and other fields (event:, id:, retry:) ignored
                    }
                    // stream ended: graceful closure (complete result) or drop;
                    // loop around and re-send subscriptions/listen
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex) {
                    AgentCore.Instance.Logger.Info($"[ModernHttp] listen stream error: {ex.Message}, retrying in {backoffMs}ms");
                    try { await Task.Delay(backoffMs, token); }
                    catch { break; }
                    backoffMs = Math.Min(backoffMs * 2, 5000);
                }
            }
        }

        private void HandleListenMessage(string json)
        {
            try {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("method", out _)) {
                    // notification (incl. notifications/subscriptions/acknowledged):
                    // forward to subscribers; responses (graceful-closure results)
                    // end the stream and are handled by the listen loop
                    ServerNotification?.Invoke(json);
                }
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Info($"[ModernHttp] failed to handle listen message: {ex.Message}");
            }
        }

        /// <summary>
        /// One-shot era probe for a streamable-http endpoint per the 2026-07-28
        /// spec: attempt a modern server/discover request; on 400 inspect the
        /// body - a recognized modern JSON-RPC error identifies a modern
        /// server, anything else (including an unreachable endpoint) falls
        /// back to the legacy initialize-handshake era.
        /// </summary>
        public static async Task<(bool IsModern, List<string> SupportedVersions)> ProbeEraAsync(
            string url, Dictionary<string, string>? headers, int timeoutMs)
        {
            try {
                var socketHandler = new SocketsHttpHandler { ConnectTimeout = TimeSpan.FromSeconds(10) };
                using var client = new HttpClient(RedirectHandler.Create(socketHandler)) {
                    Timeout = TimeSpan.FromMilliseconds(timeoutMs)
                };
                if (headers != null) {
                    foreach (var kv in headers)
                        client.DefaultRequestHeaders.TryAddWithoutValidation(kv.Key, kv.Value);
                }
                string body = McpProtocol.BuildDiscoverRequest(0, McpProtocol.V2026_07_28);
                using var request = new HttpRequestMessage(HttpMethod.Post, url.TrimEnd('/'));
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
                request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", McpProtocol.V2026_07_28);
                request.Headers.TryAddWithoutValidation("Mcp-Method", "server/discover");
                using var response = await client.SendAsync(request);
                string respBody = await response.Content.ReadAsStringAsync();
                string json = McpHttpHelpers.ExtractSseJson(response, respBody);
                var cls = McpProtocol.ClassifyEra(json, out var supported);
                if (cls == McpEraClassification.Legacy)
                    return (false, supported);
                if (cls == McpEraClassification.ModernError &&
                    !supported.Any(McpProtocol.IsModernVersion) &&
                    supported.Any(v => McpProtocol.KnownVersions.Contains(v)))
                    return (false, supported); // dual-era server exposing only handshake-era versions
                return (true, supported);
            }
            catch (Exception) {
                // Unreachable or broken endpoint: let the legacy transport
                // produce the real connection error.
                return (false, new List<string>());
            }
        }

        public async Task<string> SendRequestAsync(string jsonRpcRequest)
        {
            if (!_connected)
                throw new InvalidOperationException("ModernHttpTransport not connected");
            return await PostAsync(jsonRpcRequest);
        }

        private async Task<string> PostAsync(string jsonBody)
        {
            if (!await _lock.WaitAsync(_lockTimeout))
                throw new TimeoutException("ModernHttpTransport: timed out waiting for send lock");
            try {
                // The connection pool may hand back a connection the server already
                // closed (HTTP/1.0 servers close after each response and omit the
                // Connection header, so the pool still marks them reusable). Such a
                // request fails before any response headers arrive (ResponseEnded).
                // Retry once; the failed attempt evicts the dead connection from
                // the pool and the retry goes out on a fresh connection.
                const int maxAttempts = 2;
                for (int attempt = 1; attempt <= maxAttempts; attempt++) {
                    bool responseHeadersReceived = false;
                    try {
                        McpHttpHelpers.ReadMethodAndName(jsonBody, out var method, out var name);
                        using var request = new HttpRequestMessage(HttpMethod.Post, _url);
                        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                        request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
                        request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", _protocolVersion);
                        if (method != null)
                            request.Headers.TryAddWithoutValidation("Mcp-Method", method);
                        if (method != null && name != null &&
                            (method == "tools/call" || method == "resources/read" || method == "prompts/get"))
                            request.Headers.TryAddWithoutValidation("Mcp-Name", McpHttpHelpers.EncodeMcpName(name));
                        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                        responseHeadersReceived = true;
                        if ((int)response.StatusCode == 400) {
                            // The 2026-07-28 spec requires inspecting the 400 body: a
                            // recognized modern JSON-RPC error is returned as the
                            // response so callers can act on it (e.g. version retry).
                            string errBody = await response.Content.ReadAsStringAsync();
                            string errJson = McpHttpHelpers.ExtractSseJson(response, errBody);
                            if (McpProtocol.ClassifyEra(errJson, out _) != McpEraClassification.Legacy)
                                return errJson;
                            throw new InvalidOperationException($"MCP server at {_url} returned 400 Bad Request: {Truncate(errBody)}");
                        }
                        await HttpResponseHelper.EnsureSuccessOrThrowDetailedAsync(response);
                        string body = await response.Content.ReadAsStringAsync();
                        string result = McpHttpHelpers.ExtractSseJson(response, body);
                        if (string.IsNullOrEmpty(result))
                            throw new InvalidOperationException($"MCP server at {_url} did not respond");
                        return result;
                    }
                    catch (Exception ex) when (attempt < maxAttempts && !responseHeadersReceived &&
                                               ex is HttpRequestException { HttpRequestError: HttpRequestError.ResponseEnded }) {
                        // No response headers arrived, so the request died on a stale
                        // pooled connection (or the server dropped it before reacting)
                        // and resending is safe. Do not flip _connected.
                        AgentCore.Instance.Logger.Info($"[ModernHttp] response ended prematurely before headers (attempt {attempt}), retrying on a fresh connection: {ex.Message}");
                        continue;
                    }
                    catch (Exception) when (!_connected) {
                        throw;
                    }
                    catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException) {
                        _connected = false;
                        throw;
                    }
                }
                // Unreachable: every loop iteration returns or throws.
                throw new InvalidOperationException("ModernHttpTransport: POST retry loop exited unexpectedly");
            }
            finally {
                _lock.Release();
            }
        }

        private static string Truncate(string s) => s.Length > 512 ? s.Substring(0, 512) + "...(truncated)" : s;

        public void Disconnect()
        {
            _connected = false;
            try { _listenCts?.Cancel(); } catch { }
        }

        public void Dispose()
        {
            Disconnect();
            try { _listenCts?.Dispose(); } catch { }
            _listenCts = null;
            _httpClient.Dispose();
            _streamClient.Dispose();
        }
    }
}
