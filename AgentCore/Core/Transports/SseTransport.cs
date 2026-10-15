using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BatchCommand.Utils;

namespace AgentCore.Core
{
    /// <summary>
    /// HTTP+SSE transport: communicates with a remote MCP server via HTTP POST.
    /// target = base URL, e.g. "http://localhost:3000"
    /// </summary>
    internal class SseTransport : IMcpOldTransport
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private bool _connected;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _lockTimeout;
        private string? _sessionId;
        private readonly string _protocolVersion;

        public bool IsConnected => _connected;

        public string? NegotiatedVersion { get; set; }

        public string ProtocolVersion => NegotiatedVersion ?? _protocolVersion;

#pragma warning disable CS0067 // never raised: this transport has no server-to-client notification channel
        /// <summary>
        /// Never raised: each POST returns only the matching response, so
        /// server-initiated notifications cannot be received here.
        /// </summary>
        public event Action<string>? ServerNotification;
#pragma warning restore CS0067

        public SseTransport(string target, Dictionary<string, string>? headers = null, int timeoutMs = 300000, string? protocolVersion = null)
        {
            _protocolVersion = protocolVersion ?? McpProtocol.Default;
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
            string initReq = McpProtocol.BuildInitializeRequest(0, _protocolVersion);
            string resp = await PostAsync(initReq);
            if (string.IsNullOrEmpty(resp))
                throw new InvalidOperationException($"MCP server at {_baseUrl} did not respond to initialize");
            string? rpcError = JsonRpcHelper.ParseError(resp);
            if (rpcError != null)
                throw new InvalidOperationException($"MCP server at {_baseUrl} rejected initialize (protocol {_protocolVersion}): {rpcError}. Hint: try mcp_set_option with key 'protocol_version' and an older value such as '2024-11-05'");
            NegotiatedVersion = McpProtocol.ParseNegotiatedVersion(resp);
            _connected = true;
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
                return McpHttpHelpers.ExtractSseJson(response, body);
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
            _sessionId = null;
        }

        public void Dispose()
        {
            Disconnect();
            _httpClient.Dispose();
        }
    }
}
