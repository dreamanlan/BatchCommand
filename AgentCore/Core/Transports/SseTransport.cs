using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// HTTP+SSE transport: communicates with a remote MCP server via HTTP POST.
    /// target = base URL, e.g. "http://localhost:3000"
    /// </summary>
    internal class SseTransport : IMcpTransport
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private bool _connected;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _lockTimeout;
        private string? _sessionId;

        public bool IsConnected => _connected;

        public SseTransport(string target, Dictionary<string, string>? headers = null, int timeoutMs = 300000)
        {
            _lockTimeout = TimeSpan.FromMilliseconds(timeoutMs + 5000);
            _baseUrl = target.TrimEnd('/');
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
            if (headers != null)
            {
                foreach (var kv in headers)
                    _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(kv.Key, kv.Value);
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
                throw new InvalidOperationException($"MCP server at {_baseUrl} did not respond to initialize");
        }

        public async Task<string> SendRequestAsync(string jsonRpcRequest)
        {
            if (!_connected)
                throw new InvalidOperationException("SseTransport not connected");
            return await PostAsync(jsonRpcRequest);
        }

        public async Task SendNotificationAsync(string jsonRpcNotification)
        {
            if (!_connected)
                throw new InvalidOperationException("SseTransport not connected");
            // POST is required by HTTP semantics; discard the response body (typically 202 Accepted)
            await PostAsync(jsonRpcNotification);
        }

        private async Task<string> PostAsync(string jsonBody)
        {
            if (!await _lock.WaitAsync(_lockTimeout))
                throw new TimeoutException("SseTransport: timed out waiting for send lock");
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
                if (_sessionId != null)
                    request.Headers.TryAddWithoutValidation("Mcp-Session-Id", _sessionId);
                using var response = await _httpClient.SendAsync(request);
                await HttpResponseHelper.EnsureSuccessOrThrowDetailedAsync(response);
                if (response.Headers.TryGetValues("Mcp-Session-Id", out var vals))
                {
                    foreach (var v in vals) { _sessionId = v; break; }
                }
                string body = await response.Content.ReadAsStringAsync();
                return ExtractJsonFromResponse(response, body);
            }
            catch (Exception) when (!_connected)
            {
                throw; // already marked disconnected, just rethrow
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
            _sessionId = null;
        }

        public void Dispose()
        {
            Disconnect();
            _httpClient.Dispose();
        }
    }
}
