using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BatchCommand.Utils;

namespace AgentCore.Core
{
    /// <summary>
    /// Streamable HTTP transport: communicates with a remote MCP server via HTTP POST.
    /// Unlike SseTransport, uses the URL directly (no /message suffix),
    /// supports custom headers, configurable timeout, and Mcp-Session-Id management.
    /// </summary>
    internal class StreamableHttpTransport : IMcpBefore2026Transport
    {
        private readonly string _url;
        private readonly HttpClient _httpClient;
        private readonly HttpClient _streamClient;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _lockTimeout;
        private bool _connected;
        private string? _sessionId;
        private CancellationTokenSource? _getStreamCts;
        private Task? _getStreamTask;
        private readonly string _protocolVersion;

        public bool IsConnected => _connected;

        public string? NegotiatedVersion { get; set; }

        public string ProtocolVersion => NegotiatedVersion ?? _protocolVersion;

        /// <summary>
        /// Fired for server-initiated notifications received on the GET stream
        /// (method, no id). Server requests (method+id) are answered in-place;
        /// plain responses (id, no method) are ignored.
        /// </summary>
        public event Action<string>? ServerNotification;

        public StreamableHttpTransport(string url, Dictionary<string, string>? headers = null, int timeoutMs = 300000, string? protocolVersion = null)
        {
            _protocolVersion = protocolVersion ?? McpProtocol.Default;
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
            // Dedicated client for GET SSE long-poll: no HTTP-level timeout
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

        public async Task ConnectAsync()
        {
            // Verify server is reachable by sending an initialize request
            string initReq = McpProtocol.BuildInitializeRequest(0, _protocolVersion);
            string resp = await PostAsync(initReq);
            if (string.IsNullOrEmpty(resp))
                throw new InvalidOperationException($"MCP server at {_url} did not respond to initialize");
            string? rpcError = JsonRpcHelper.ParseError(resp);
            if (rpcError != null)
                throw new InvalidOperationException($"MCP server at {_url} rejected initialize (protocol {_protocolVersion}): {rpcError}. Hint: try mcp_set_option with key 'protocol_version' and an older value such as '2024-11-05'");
            NegotiatedVersion = McpProtocol.ParseNegotiatedVersion(resp);
            _connected = true;
        }

        private void StartGetStream()
        {
            try { _getStreamCts?.Cancel(); } catch { }
            try { _getStreamCts?.Dispose(); } catch { }
            _getStreamCts = new CancellationTokenSource();
            var token = _getStreamCts.Token;
            _getStreamTask = Task.Run(() => GetStreamLoopAsync(token), token);
        }

        private async Task GetStreamLoopAsync(CancellationToken token)
        {
            int backoffMs = 500;
            while (!token.IsCancellationRequested && _connected) {
                try {
                    using var request = new HttpRequestMessage(HttpMethod.Get, _url);
                    request.Headers.TryAddWithoutValidation("Accept", "text/event-stream");
                    if (NegotiatedVersion != null)
                        request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", NegotiatedVersion);
                    if (_sessionId != null)
                        request.Headers.TryAddWithoutValidation("Mcp-Session-Id", _sessionId);
                    using var response = await _streamClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                    if ((int)response.StatusCode == 405) {
                        AgentCore.Instance.Logger.Info($"[StreamableHttp] server at {_url} does not support GET SSE (405); long-poll disabled");
                        return;
                    }
                    if ((int)response.StatusCode == 404) {
                        AgentCore.Instance.Logger.Info($"[StreamableHttp] GET stream got 404 (session expired), stopping long-poll");
                        _connected = false;
                        return;
                    }
                    if (!response.IsSuccessStatusCode) {
                        AgentCore.Instance.Logger.Info($"[StreamableHttp] GET stream returned {(int)response.StatusCode}, retrying in {backoffMs}ms");
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
                                _ = HandleServerMessageAsync(payload);
                            }
                        }
                        else if (line.StartsWith("data:", StringComparison.Ordinal)) {
                            string val = line.Substring(5);
                            if (val.StartsWith(" ", StringComparison.Ordinal))
                                val = val.Substring(1);
                            if (eventData.Length > 0) eventData.Append('\n');
                            eventData.Append(val);
                        }
                        // other SSE fields (event:, id:, retry:) and comments (:) ignored
                    }
                    // stream closed by server; flush any pending event and retry
                    if (eventData.Length > 0) {
                        _ = HandleServerMessageAsync(eventData.ToString());
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex) {
                    AgentCore.Instance.Logger.Info($"[StreamableHttp] GET stream error: {ex.Message}, retrying in {backoffMs}ms");
                    try { await Task.Delay(backoffMs, token); }
                    catch { break; }
                    backoffMs = Math.Min(backoffMs * 2, 5000);
                }
            }
        }

        private async Task HandleServerMessageAsync(string json)
        {
            try {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                bool hasMethod = root.TryGetProperty("method", out var methodEl);
                bool hasId = root.TryGetProperty("id", out var idEl);
                if (hasMethod && hasId) {
                    // Server-initiated request: respond
                    string method = methodEl.GetString() ?? string.Empty;
                    string idRaw = idEl.GetRawText();
                    string responseJson;
                    if (method == "ping") {
                        responseJson = "{\"jsonrpc\":\"2.0\",\"id\":" + idRaw + ",\"result\":{}}";
                    }
                    else {
                        responseJson = "{\"jsonrpc\":\"2.0\",\"id\":" + idRaw +
                            ",\"error\":{\"code\":-32601,\"message\":\"Method not found\"}}";
                    }
                    await PostFireAndForgetAsync(responseJson);
                }
                else if (hasMethod) {
                    // server-initiated notification: forward to subscribers
                    ServerNotification?.Invoke(json);
                }
                // responses (id, no method) are ignored
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Info($"[StreamableHttp] failed to handle server message: {ex.Message}");
            }
        }

        private async Task PostFireAndForgetAsync(string jsonBody)
        {
            try {
                using var request = new HttpRequestMessage(HttpMethod.Post, _url);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
                if (NegotiatedVersion != null)
                    request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", NegotiatedVersion);
                if (_sessionId != null)
                    request.Headers.TryAddWithoutValidation("Mcp-Session-Id", _sessionId);
                using var response = await _httpClient.SendAsync(request);
                if (response.Content != null)
                    await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Info($"[StreamableHttp] fire-and-forget POST failed: {ex.Message}");
            }
        }

        public async Task<string> SendRequestAsync(string jsonRpcRequest)
        {
            if (!_connected)
                throw new InvalidOperationException("StreamableHttpTransport not connected");
            return await PostAsync(jsonRpcRequest);
        }

        public async Task SendNotificationAsync(string jsonRpcNotification)
        {
            if (!_connected)
                throw new InvalidOperationException("StreamableHttpTransport not connected");
            // POST is required by HTTP semantics; discard the response body (typically 202 Accepted)
            await PostAsync(jsonRpcNotification);
            // Start the GET stream only after the initialized notification succeeds.
            if (!string.IsNullOrEmpty(_sessionId)) {
                using var notification = JsonDocument.Parse(jsonRpcNotification);
                if (notification.RootElement.TryGetProperty("method", out var method) &&
                    method.GetString() == "notifications/initialized")
                    StartGetStream();
            }
        }

        private async Task<string> PostAsync(string jsonBody)
        {
            if (!await _lock.WaitAsync(_lockTimeout))
                throw new TimeoutException("StreamableHttpTransport: timed out waiting for send lock");
            string? requestId = ReadDiagnosticId(jsonBody, out var method);
            string context = $"method={method} requestId={requestId ?? "<missing-or-unavailable>"}";
            try {
                // The connection pool may hand back a connection the server already
                // closed: HTTP/1.0 servers close after each response and omit the
                // Connection header, so the pool still marks them reusable. Such a
                // request fails before any response headers arrive (ResponseEnded).
                // Retry once; the failed attempt evicts the dead connection from
                // the pool and the retry goes out on a fresh connection.
                const int maxAttempts = 2;
                for (int attempt = 1; attempt <= maxAttempts; attempt++) {
                    string stage = "prepare-request";
                    try {
                        using var request = new HttpRequestMessage(HttpMethod.Post, _url);
                        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                        request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
                        if (NegotiatedVersion != null)
                            request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", NegotiatedVersion);
                        if (_sessionId != null)
                            request.Headers.TryAddWithoutValidation("Mcp-Session-Id", _sessionId);
                        using var requestCts = new CancellationTokenSource();
                        requestCts.CancelAfter(_httpClient.Timeout);
                        stage = "send-and-read-headers";
                        LogPostDiagnostic($"{context} stage={stage} completion=ResponseHeadersRead");
                        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, requestCts.Token);
                        LogPostDiagnostic($"{context} stage=response-headers status={(int)response.StatusCode} httpVersion={response.Version} contentType={response.Content.Headers.ContentType} contentLength={response.Content.Headers.ContentLength} transferEncoding={response.Headers.TransferEncoding} connection={response.Headers.Connection}");
                        stage = "read-body";
                        string body = await response.Content.ReadAsStringAsync(requestCts.Token);
                        LogPostDiagnostic($"{context} stage=body-read bodyChars={body.Length}");
                        stage = "validate-status";
                        await HttpResponseHelper.EnsureSuccessOrThrowDetailedAsync(response);
                        stage = "capture-session";
                        // Capture session id from response headers
                        if (response.Headers.TryGetValues("Mcp-Session-Id", out var vals)) {
                            foreach (var v in vals) { _sessionId = v; break; }
                        }
                        stage = "extract-json";
                        string result = McpHttpHelpers.ExtractSseJson(response, body);
                        string? responseId = ReadDiagnosticId(result, out _);
                        string idMatch = requestId == null || responseId == null
                            ? "unavailable"
                            : (string.Equals(requestId, responseId, StringComparison.Ordinal) ? "true" : "false");
                        LogPostDiagnostic($"{context} stage=complete responseId={responseId ?? "<missing-or-unavailable>"} rawIdMatch={idMatch}");
                        return result;
                    }
                    catch (Exception ex) when (attempt < maxAttempts && stage == "send-and-read-headers" &&
                                               ex is HttpRequestException { HttpRequestError: HttpRequestError.ResponseEnded }) {
                        // No response headers arrived: the request died on a stale
                        // pooled connection (or the server dropped it before
                        // reacting), so resending is safe. Do not flip _connected:
                        // this is not a transport-level failure yet.
                        LogPostDiagnostic($"{context} stage={stage} attempt={attempt} response ended prematurely, retrying on a fresh connection: {ex.Message}");
                        continue;
                    }
                    catch (Exception ex) when (!_connected) {
                        LogPostDiagnostic($"{context} stage={stage} failed: {ex}");
                        throw;
                    }
                    catch (Exception ex) when (ex is HttpRequestException || ex is IOException || ex is OperationCanceledException || ex is InvalidOperationException) {
                        _connected = false;
                        LogPostDiagnostic($"{context} stage={stage} failed: {ex}");
                        throw;
                    }
                    catch (Exception ex) {
                        LogPostDiagnostic($"{context} stage={stage} failed: {ex}");
                        throw;
                    }
                }
                // Unreachable: every loop iteration returns or throws.
                throw new InvalidOperationException("StreamableHttpTransport: POST retry loop exited unexpectedly");
            }
            finally {
                _lock.Release();
            }
        }

        private static string? ReadDiagnosticId(string json, out string method)
        {
            method = "<missing-or-unavailable>";
            try {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                    return null;
                if (root.TryGetProperty("method", out var methodValue))
                    method = methodValue.GetRawText();
                return root.TryGetProperty("id", out var id) ? id.GetRawText() : null;
            }
            catch (Exception) {
                // Diagnostic parsing must not change transport behavior.
                return null;
            }
        }

        private static void LogPostDiagnostic(string message)
        {
            try {
                AgentCore.Instance.Logger.Info($"[StreamableHttp] POST utc={DateTime.UtcNow:O} {message}");
            }
            catch (Exception) {
                // Diagnostic logging must not mask the transport result.
            }
        }

        public void Disconnect()
        {
            _connected = false;
            try { _getStreamCts?.Cancel(); } catch { }
            _sessionId = null;
        }

        public void Dispose()
        {
            Disconnect();
            try { _getStreamCts?.Dispose(); } catch { }
            _getStreamCts = null;
            _httpClient.Dispose();
            _streamClient.Dispose();
        }
    }
}
