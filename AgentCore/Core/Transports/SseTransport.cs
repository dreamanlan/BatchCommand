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

        private async Task<string> PostAsync(string jsonBody)
        {
            if (!await _lock.WaitAsync(_lockTimeout))
                throw new TimeoutException("SseTransport: timed out waiting for send lock");
            try
            {
                using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                using var response = await _httpClient.PostAsync($"{_baseUrl}/message", content);
                await HttpResponseHelper.EnsureSuccessOrThrowDetailedAsync(response);
                return await response.Content.ReadAsStringAsync();
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

        public void Disconnect()
        {
            _connected = false;
        }

        public void Dispose()
        {
            Disconnect();
            _httpClient.Dispose();
        }
    }
}
