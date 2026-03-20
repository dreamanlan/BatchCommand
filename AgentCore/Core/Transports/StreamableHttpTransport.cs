using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _lockTimeout;
        private bool _connected;
        private string? _sessionId;

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
                throw new InvalidOperationException($"MCP server at {_url} did not respond to initialize");
        }

        public async Task<string> SendRequestAsync(string jsonRpcRequest)
        {
            if (!_connected)
                throw new InvalidOperationException("StreamableHttpTransport not connected");
            return await PostAsync(jsonRpcRequest);
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
                return await response.Content.ReadAsStringAsync();
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
