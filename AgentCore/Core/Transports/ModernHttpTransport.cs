using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BatchCommand.Utils;

namespace AgentCore.Core
{
    /// <summary>
    /// Streamable HTTP transport for the stateless MCP 2026-07-28 revision:
    /// no initialize handshake, no Mcp-Session-Id, no GET stream. Every POST
    /// carries the required MCP-Protocol-Version / Mcp-Method (and Mcp-Name
    /// where applicable) headers mirrored from the request body.
    /// </summary>
    internal class ModernHttpTransport : IMcpTransport
    {
        private readonly string _url;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _lockTimeout;
        private bool _connected;
        private string _protocolVersion;

        public bool IsConnected => _connected;

        public string ProtocolVersion => _protocolVersion;

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
            if (headers != null) {
                foreach (var kv in headers)
                    _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(kv.Key, kv.Value);
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
                using var response = await _httpClient.SendAsync(request);
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
            catch (Exception) when (!_connected) {
                throw;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException) {
                _connected = false;
                throw;
            }
            finally {
                _lock.Release();
            }
        }

        private static string Truncate(string s) => s.Length > 512 ? s.Substring(0, 512) + "...(truncated)" : s;

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
