using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Streamable HTTP transport: communicates with a remote MCP server via HTTP POST.
    /// Unlike SseTransport, uses the URL directly (no /message suffix),
    /// supports custom headers, configurable timeout, and Mcp-Session-Id management.
    /// </summary>
    internal class StreamableHttpTransport : IMcpTransport
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

        public bool IsConnected => _connected;

        public StreamableHttpTransport(string url, Dictionary<string, string>? headers = null, int timeoutMs = 300000)
        {
            _lockTimeout = TimeSpan.FromMilliseconds(timeoutMs + 5000);
            _url = url.TrimEnd('/');
            var socketHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
                ConnectTimeout = TimeSpan.FromSeconds(10)
            };
            _httpClient = new HttpClient(RedirectHandler.Create(socketHandler))
            {
                Timeout = TimeSpan.FromMilliseconds(timeoutMs)
            };
            // Dedicated client for GET SSE long-poll: no HTTP-level timeout
            var streamSocketHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = Timeout.InfiniteTimeSpan,
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                ConnectTimeout = TimeSpan.FromSeconds(10)
            };
            _streamClient = new HttpClient(RedirectHandler.Create(streamSocketHandler))
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(kv.Key, kv.Value);
                    _streamClient.DefaultRequestHeaders.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }
        }

        public async Task ConnectAsync()
        {
            // Verify server is reachable by sending an initialize request
            string initReq = JsonRpcHelper.BuildRequest("initialize", new
            {
                protocolVersion = "2024-11-05",
                capabilities = new { },
                clientInfo = new { name = "AgentCore", version = "1.0" }
            }, 0);
            string resp = await PostAsync(initReq);
            if (!string.IsNullOrEmpty(resp))
                _connected = true;
            else
                throw new InvalidOperationException($"MCP server at {_url} did not respond to initialize");
            // Start GET SSE long-poll to receive server-initiated messages (ping, notifications, etc.)
            if (!string.IsNullOrEmpty(_sessionId))
                StartGetStream();
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
            while (!token.IsCancellationRequested && _connected)
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, _url);
                    request.Headers.TryAddWithoutValidation("Accept", "text/event-stream");
                    if (_sessionId != null)
                        request.Headers.TryAddWithoutValidation("Mcp-Session-Id", _sessionId);
                    using var response = await _streamClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                    if ((int)response.StatusCode == 405)
                    {
                        AgentCore.Instance.Logger.Info($"[StreamableHttp] server at {_url} does not support GET SSE (405); long-poll disabled");
                        return;
                    }
                    if ((int)response.StatusCode == 404)
                    {
                        AgentCore.Instance.Logger.Info($"[StreamableHttp] GET stream got 404 (session expired), stopping long-poll");
                        _connected = false;
                        return;
                    }
                    if (!response.IsSuccessStatusCode)
                    {
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
                    while (!token.IsCancellationRequested && (line = await reader.ReadLineAsync()) != null)
                    {
                        if (line.Length == 0)
                        {
                            if (eventData.Length > 0)
                            {
                                string payload = eventData.ToString();
                                eventData.Clear();
                                _ = HandleServerMessageAsync(payload);
                            }
                        }
                        else if (line.StartsWith("data:", StringComparison.Ordinal))
                        {
                            string val = line.Substring(5);
                            if (val.StartsWith(" ", StringComparison.Ordinal))
                                val = val.Substring(1);
                            if (eventData.Length > 0) eventData.Append('\n');
                            eventData.Append(val);
                        }
                        // other SSE fields (event:, id:, retry:) and comments (:) ignored
                    }
                    // stream closed by server; flush any pending event and retry
                    if (eventData.Length > 0)
                    {
                        _ = HandleServerMessageAsync(eventData.ToString());
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    AgentCore.Instance.Logger.Info($"[StreamableHttp] GET stream error: {ex.Message}, retrying in {backoffMs}ms");
                    try { await Task.Delay(backoffMs, token); }
                    catch { break; }
                    backoffMs = Math.Min(backoffMs * 2, 5000);
                }
            }
        }

        private async Task HandleServerMessageAsync(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                bool hasMethod = root.TryGetProperty("method", out var methodEl);
                bool hasId = root.TryGetProperty("id", out var idEl);
                if (hasMethod && hasId)
                {
                    // Server-initiated request: respond
                    string method = methodEl.GetString() ?? string.Empty;
                    string idRaw = idEl.GetRawText();
                    string responseJson;
                    if (method == "ping")
                    {
                        responseJson = "{\"jsonrpc\":\"2.0\",\"id\":" + idRaw + ",\"result\":{}}";
                    }
                    else
                    {
                        responseJson = "{\"jsonrpc\":\"2.0\",\"id\":" + idRaw +
                            ",\"error\":{\"code\":-32601,\"message\":\"Method not found\"}}";
                    }
                    await PostFireAndForgetAsync(responseJson);
                }
                // notification (method, no id) and response (id, no method) ignored for now
            }
            catch (Exception ex)
            {
                AgentCore.Instance.Logger.Info($"[StreamableHttp] failed to handle server message: {ex.Message}");
            }
        }

        private async Task PostFireAndForgetAsync(string jsonBody)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, _url);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
                if (_sessionId != null)
                    request.Headers.TryAddWithoutValidation("Mcp-Session-Id", _sessionId);
                using var response = await _httpClient.SendAsync(request);
                if (response.Content != null)
                    await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
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
        }

        private async Task<string> PostAsync(string jsonBody)
        {
            if (!await _lock.WaitAsync(_lockTimeout))
                throw new TimeoutException("StreamableHttpTransport: timed out waiting for send lock");
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, _url);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
                if (_sessionId != null)
                    request.Headers.TryAddWithoutValidation("Mcp-Session-Id", _sessionId);
                using var response = await _httpClient.SendAsync(request);
                await HttpResponseHelper.EnsureSuccessOrThrowDetailedAsync(response);
                // Capture session id from response headers
                if (response.Headers.TryGetValues("Mcp-Session-Id", out var vals))
                {
                    foreach (var v in vals) { _sessionId = v; break; }
                }
                string body = await response.Content.ReadAsStringAsync();
                return ExtractJsonFromResponse(response, body);
            }
            catch (Exception) when (!_connected)
            {
                throw;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is InvalidOperationException)
            {
                _connected = false;
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }

        private static string ExtractJsonFromResponse(HttpResponseMessage response, string body)
        {
            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (!string.Equals(mediaType, "text/event-stream", StringComparison.OrdinalIgnoreCase))
                return body;
            // Parse SSE frames: aggregate all lines starting with "data:" (strip prefix, join by newline)
            var sb = new StringBuilder();
            using var reader = new System.IO.StringReader(body);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("data:", StringComparison.Ordinal))
                {
                    string payload = line.Substring(5);
                    if (payload.StartsWith(" ", StringComparison.Ordinal))
                        payload = payload.Substring(1);
                    if (sb.Length > 0) sb.Append('\n');
                    sb.Append(payload);
                }
            }
            return sb.Length > 0 ? sb.ToString() : body;
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
