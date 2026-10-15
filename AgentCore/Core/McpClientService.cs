using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ScriptableFramework;
using BatchCommand;
using BatchCommand.Utils;

namespace AgentCore.Core
{
    /// <summary>
    /// Minimal runtime contract for MCP transports, shared by both protocol
    /// eras (the legacy initialize-handshake era and the stateless 2026-07-28
    /// revision): connect, send a JSON-RPC request, check liveness, disconnect.
    /// </summary>
    internal interface IMcpTransport : IDisposable
    {
        bool IsConnected { get; }
        /// <summary>
        /// Effective protocol version, fixed at connect time. The service layer
        /// decorates request _meta from this value; HTTP transports must stamp
        /// the matching MCP-Protocol-Version header on every request.
        /// </summary>
        string ProtocolVersion { get; }
        Task ConnectAsync();
        /// <summary>
        /// Sends a JSON-RPC request and returns the raw JSON-RPC response string.
        /// </summary>
        Task<string> SendRequestAsync(string jsonRpcRequest);
        void Disconnect();
    }

    /// <summary>
    /// Transport contract for the legacy (initialize-handshake, pre-2026-07-28)
    /// MCP era. Adds handshake-era members on top of the shared IMcpTransport
    /// runtime contract. Frozen: new protocol features must not be added here.
    /// </summary>
    internal interface IMcpOldTransport : IMcpTransport
    {
        /// <summary>
        /// Protocol version the server answered with in the initialize
        /// handshake (null when unreported by pre-negotiation servers).
        /// </summary>
        string? NegotiatedVersion { get; set; }
        /// <summary>
        /// Sends a JSON-RPC notification (no id, no response expected).
        /// Implementations MUST NOT wait for any response.
        /// </summary>
        Task SendNotificationAsync(string jsonRpcNotification);
        /// <summary>
        /// Raised when the server sends a notification the transport does not
        /// handle itself (e.g. notifications/prompts/list_changed). May never
        /// fire on transports without a server-to-client notification channel.
        /// </summary>
        event Action<string>? ServerNotification;
    }

    /// <summary>
    /// Helper to build JSON-RPC 2.0 request strings.
    /// </summary>
    internal static class JsonRpcHelper
    {
        public static string BuildRequest(string method, object? parameters, int id)
        {
            if (parameters == null) {
                var req = new { jsonrpc = "2.0", id, method };
                return System.Text.Json.JsonSerializer.Serialize(req);
            }
            var reqWithParams = new { jsonrpc = "2.0", id, method, @params = parameters };
            return System.Text.Json.JsonSerializer.Serialize(reqWithParams);
        }

        public static System.Text.Json.JsonElement? ParseResult(string response)
        {
            try {
                using var doc = System.Text.Json.JsonDocument.Parse(response);
                if (doc.RootElement.TryGetProperty("result", out var result))
                    return result.Clone();
            }
            catch { }
            return null;
        }

        public static string? ParseError(string response)
        {
            try {
                using var doc = System.Text.Json.JsonDocument.Parse(response);
                if (doc.RootElement.TryGetProperty("error", out var err)) {
                    if (err.TryGetProperty("message", out var msg))
                        return msg.GetString();
                    return err.GetRawText();
                }
            }
            catch { }
            return null;
        }
    }

    /// <summary>
    /// MCP protocol version constants and initialize-handshake helpers.
    /// Default targets 2025-11-25 (latest handshake-era revision). The negotiated
    /// result is accepted as-is because the client's feature subset (initialize /
    /// tools/list / tools/call / resources/list / resources/read) has stable
    /// semantics across all versions from 2024-11-05 on.
    /// 2026-07-28 removes the initialize handshake entirely (stateless, per-request
    /// _meta versioning); it is kept here as a constant and excluded from
    /// KnownVersions until the stateless-mode migration is implemented.
    /// </summary>
    internal static class McpProtocol
    {
        public const string V2024_11_05 = "2024-11-05";
        public const string V2025_03_26 = "2025-03-26";
        public const string V2025_06_18 = "2025-06-18";
        public const string V2025_11_25 = "2025-11-25";
        public const string V2026_07_28 = "2026-07-28";
        public const string Default = V2025_11_25;

        /// <summary>Handshake-era versions a pre-2026 server may negotiate.</summary>
        public static readonly string[] KnownVersions = {
            V2024_11_05, V2025_03_26, V2025_06_18, V2025_11_25
        };

        public static string BuildInitializeRequest(int id, string protocolVersion)
        {
            return JsonRpcHelper.BuildRequest("initialize", new {
                protocolVersion,
                capabilities = new { },
                clientInfo = new { name = "AgentCore", version = "1.0" }
            }, id);
        }

        /// <summary>
        /// Extracts the server-negotiated protocolVersion from an initialize
        /// response, or null when the server did not report one.
        /// </summary>
        public static string? ParseNegotiatedVersion(string initializeResponse)
        {
            try {
                using var doc = System.Text.Json.JsonDocument.Parse(initializeResponse);
                if (doc.RootElement.TryGetProperty("result", out var result) &&
                    result.TryGetProperty("protocolVersion", out var v) &&
                    v.ValueKind == System.Text.Json.JsonValueKind.String) {
                    return v.GetString();
                }
            }
            catch { }
            return null;
        }

        /// <summary>JSON-RPC error code for UnsupportedProtocolVersionError (renumbered to the MCP-reserved range in 2026-07-28).</summary>
        public const int UnsupportedProtocolVersionErrorCode = -32022;

        /// <summary>
        /// True for error codes in the MCP-reserved range of the 2026-07-28
        /// revision (-32099..-32020); a recognized modern error identifies a
        /// stateless-era server during era detection.
        /// </summary>
        public static bool IsModernErrorCode(int code) => code >= -32099 && code <= -32020;

        /// <summary>True for versions of the stateless 2026-07-28 era.</summary>
        public static bool IsModernVersion(string? version) => version == V2026_07_28;

        /// <summary>
        /// Builds a server/discover request (2026-07-28 capability discovery,
        /// also used as the era probe). The request carries the required
        /// _meta decoration.
        /// </summary>
        public static string BuildDiscoverRequest(int id, string protocolVersion)
        {
            return BuildDecoratedRequest("server/discover", null, id, protocolVersion);
        }

        /// <summary>
        /// Builds a JSON-RPC request decorated for the stateless 2026-07-28
        /// revision: params._meta carries protocolVersion, clientInfo and
        /// clientCapabilities on every request.
        /// </summary>
        public static string BuildDecoratedRequest(string method, object? parameters, int id, string protocolVersion)
        {
            var paramsNode = parameters == null
                ? new System.Text.Json.Nodes.JsonObject()
                : System.Text.Json.JsonSerializer.SerializeToNode(parameters)?.AsObject() ?? new System.Text.Json.Nodes.JsonObject();
            paramsNode["_meta"] = new System.Text.Json.Nodes.JsonObject {
                ["io.modelcontextprotocol/protocolVersion"] = protocolVersion,
                ["io.modelcontextprotocol/clientInfo"] = new System.Text.Json.Nodes.JsonObject {
                    ["name"] = "AgentCore",
                    ["version"] = "1.0"
                },
                ["io.modelcontextprotocol/clientCapabilities"] = new System.Text.Json.Nodes.JsonObject()
            };
            var req = new System.Text.Json.Nodes.JsonObject {
                ["jsonrpc"] = "2.0",
                ["id"] = id,
                ["method"] = method,
                ["params"] = paramsNode
            };
            return req.ToJsonString();
        }

        /// <summary>
        /// Classifies a probe response for era detection: a JSON-RPC result or
        /// an error inside the MCP-reserved modern range identifies a 2026-07-28
        /// server; anything else (including unparseable responses) is legacy.
        /// For ModernError, supportedVersions carries the error's data.supported list.
        /// </summary>
        public static McpEraClassification ClassifyEra(string response, out List<string> supportedVersions)
        {
            supportedVersions = new List<string>();
            try {
                using var doc = System.Text.Json.JsonDocument.Parse(response);
                var root = doc.RootElement;
                if (root.TryGetProperty("result", out _))
                    return McpEraClassification.ModernResult;
                if (root.TryGetProperty("error", out var err) &&
                    err.TryGetProperty("code", out var codeEl) &&
                    codeEl.ValueKind == System.Text.Json.JsonValueKind.Number &&
                    codeEl.TryGetInt32(out int code) &&
                    IsModernErrorCode(code)) {
                    if (err.TryGetProperty("data", out var data) &&
                        data.TryGetProperty("supported", out var sup) &&
                        sup.ValueKind == System.Text.Json.JsonValueKind.Array) {
                        foreach (var v in sup.EnumerateArray()) {
                            if (v.ValueKind == System.Text.Json.JsonValueKind.String)
                                supportedVersions.Add(v.GetString() ?? string.Empty);
                        }
                    }
                    return McpEraClassification.ModernError;
                }
            }
            catch { }
            return McpEraClassification.Legacy;
        }
    }

    /// <summary>Era classification of a probe response (see McpProtocol.ClassifyEra).</summary>
    internal enum McpEraClassification
    {
        /// <summary>Not a recognized modern result/error: legacy handshake-era server (or unparseable response).</summary>
        Legacy,
        /// <summary>Valid modern (2026-07-28) result.</summary>
        ModernResult,
        /// <summary>Recognized modern JSON-RPC error (code in the MCP-reserved range).</summary>
        ModernError
    }

    // -------------------------------------------------------------------------
    // McpClientService: manages connections to multiple MCP servers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Represents a single MCP server connection with its cached tool list.
    /// </summary>
    internal class McpServerConnection : IDisposable
    {
        public IMcpTransport Transport { get; }
        public List<McpTool> Tools { get; } = new List<McpTool>();
        public string Type { get; }
        public string Target { get; }
        private int _requestId;

        public McpServerConnection(IMcpTransport transport, string type, string target)
        {
            Transport = transport;
            Type = type;
            Target = target;
        }

        public int NextId() => Interlocked.Increment(ref _requestId);

        public void Dispose()
        {
            Transport.Dispose();
        }
    }

    /// <summary>
    /// Describes a single MCP tool (name, title, description, parameter schema).
    /// </summary>
    public class McpTool
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        /// <summary>Raw JSON of the inputSchema object.</summary>
        public string InputSchemaJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resolved transport options extracted from pending options.
    /// </summary>
    internal struct TransportOptions
    {
        public Dictionary<string, string>? Headers;
        public string? ProtocolVersion;
        public int TimeoutMs;
        public bool UseLspFraming;
        public int StartupDelayMs;
        public int RetryDelayMs;
        public int RetryCount;
    }

    /// <summary>
    /// Manages connections to multiple MCP servers.
    /// Each server is identified by a serverId string.
    /// Tool calls are executed asynchronously; results are forwarded via NativeApi.EnqueueMcpCallback.
    /// </summary>
    public class McpClientService
    {
        private static readonly Lazy<McpClientService> s_instance =
            new Lazy<McpClientService>(() => new McpClientService());
        public static McpClientService Instance => s_instance.Value;

        private readonly ConcurrentDictionary<string, McpServerConnection> _servers = new();
        private readonly ConcurrentDictionary<string, Dictionary<string, List<string>>> _pendingOptions = new();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _reconnectLocks = new();

        // Cached protocol era per server (true = stateless 2026-07-28), detected
        // once to avoid re-probing on reconnect (spec: era is a server property).
        private readonly ConcurrentDictionary<string, bool> _modernEraCache = new();

        // Per-session serial queues for async callback calls (key = "serverId:tag").
        // Each session runs its items one at a time; up to max_queue_len items may
        // wait in line, and further submissions are rejected with "busy".
        private readonly ConcurrentDictionary<string, McpSession> _sessions = new();
        private const int c_defaultQueueSize = 10;

        // Busy-call tracking: records when each call became busy (for stuck detection)
        private readonly ConcurrentDictionary<string, bool> _busyCalls = new();
        private readonly ConcurrentDictionary<string, DateTime> _busySince = new();
        // Active CancellationTokenSource per call (for external cancel)
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeCts = new();
        // Watchdog timer for auto-cancelling stuck calls
        private readonly Timer _watchdogTimer;
        private const int c_watchdogIntervalMs = 10000;
        private const int c_defaultMaxBusySeconds = 600;
        private static int s_syncCallId = 0;

        private McpClientService()
        {
            _watchdogTimer = new Timer(WatchdogCallback, null, c_watchdogIntervalMs, c_watchdogIntervalMs);
        }

        /// <summary>
        /// Sets a connection option for a server before calling Connect.
        /// Supported keys: "timeout" (milliseconds), "header" (Name:Value, can be called multiple times),
        /// "protocol_version" ('2026-07-28' forces the stateless era, older values force the handshake era;
        /// default auto-detects the server era and requests 2025-11-25 for legacy servers).
        /// If value contains %var% patterns, they are resolved at Connect time (not here).
        /// Options are NOT consumed by Connect; call ClearOptions to remove them.
        /// </summary>
        public void SetOption(string serverId, string key, string value)
        {
            // Store raw value (may contain %var% templates); resolved at Connect time
            var opts = _pendingOptions.GetOrAdd(serverId, _ => new Dictionary<string, List<string>>());
            lock (opts) {
                if (!opts.TryGetValue(key, out var list)) {
                    list = new List<string>();
                    opts[key] = list;
                }
                list.Add(value);
            }
        }

        /// <summary>
        /// Clears all pending options for a server.
        /// </summary>
        public void ClearOptions(string serverId)
        {
            _pendingOptions.TryRemove(serverId, out _);
        }

        /// <summary>
        /// Connects to an MCP server and caches its tool list.
        /// type: "stdio" or "sse"
        /// target: command line (stdio) or base URL (sse)
        /// Returns "ok" or an error string.
        /// </summary>
        public string Connect(string serverId, string type, string target)
        {
            if (_servers.ContainsKey(serverId))
                Disconnect(serverId);

            var opts = ExtractTransportOptions(serverId, 60000);
            bool forceModern = opts.ProtocolVersion != null && McpProtocol.IsModernVersion(opts.ProtocolVersion);
            bool forceLegacy = opts.ProtocolVersion != null && !forceModern;

            IMcpTransport transport;
            if (type == "stdio") {
                transport = new StdioTransport(target, opts.TimeoutMs, opts.UseLspFraming);
            }
            else if (type == "sse") {
                if (forceModern)
                    return "error: transport 'sse' (HTTP+SSE) does not support protocol 2026-07-28; use 'streamable-http'";
                transport = new SseTransport(target, opts.Headers, opts.TimeoutMs, opts.ProtocolVersion);
            }
            else if (type == "streamable-http") {
                // Era detection per the 2026-07-28 spec: attempt a modern
                // server/discover probe; unrecognized/legacy responses fall
                // back to the initialize-handshake transport.
                bool modern;
                if (forceModern)
                    modern = true;
                else if (forceLegacy)
                    modern = false;
                else if (_modernEraCache.TryGetValue(serverId, out bool cachedModern))
                    modern = cachedModern;
                else {
                    var probe = ModernHttpTransport.ProbeEraAsync(target, opts.Headers, Math.Min(opts.TimeoutMs, 30000)).GetAwaiter().GetResult();
                    modern = probe.IsModern;
                }
                _modernEraCache[serverId] = modern;
                transport = modern
                    ? new ModernHttpTransport(target, opts.Headers, opts.TimeoutMs, opts.ProtocolVersion)
                    : new StreamableHttpTransport(target, opts.Headers, opts.TimeoutMs, opts.ProtocolVersion);
            }
            else {
                return $"error: unknown transport type '{type}', use 'stdio', 'sse' or 'streamable-http'";
            }

            // Route server-initiated notifications (e.g. prompts/list_changed) to the service (legacy-era transports only)
            if (transport is IMcpOldTransport notifTransport)
                notifTransport.ServerNotification += json => HandleServerNotification(serverId, json);

            try {
                transport.ConnectAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex) {
                transport.Dispose();
                return $"error: connect failed - {ex.Message}";
            }

            var conn = new McpServerConnection(transport, type, target);

            // For stdio, detect the protocol era over the process pipe, then run
            // the legacy initialize handshake when needed (the 2026-07-28
            // revision has no handshake)
            if (type == "stdio") {
                bool modern;
                if (forceModern)
                    modern = true;
                else if (forceLegacy)
                    modern = false;
                else if (_modernEraCache.TryGetValue(serverId, out bool cachedModern))
                    modern = cachedModern;
                else
                    modern = DetectStdioEraModern(serverId, transport);
                _modernEraCache[serverId] = modern;

                if (modern) {
                    var modernStdio = (IMcpOldTransport)transport;
                    if (modernStdio.NegotiatedVersion == null)
                        modernStdio.NegotiatedVersion = forceModern ? opts.ProtocolVersion : McpProtocol.V2026_07_28;
                    AgentCore.Instance.Logger.Info($"[McpClientService] server '{serverId}' (stdio) uses stateless protocol {transport.ProtocolVersion}");
                }
                else {
                    var oldTransport = (IMcpOldTransport)transport;
                    try {
                        Thread.Sleep(opts.StartupDelayMs);
                        string requestedVersion = opts.ProtocolVersion ?? McpProtocol.Default;
                        string initReq = McpProtocol.BuildInitializeRequest(conn.NextId(), requestedVersion);
                        string? lastInitResp = null;
                        for (int i = 0; i < opts.RetryCount; i++) {
                            string initResp = transport.SendRequestAsync(initReq).GetAwaiter().GetResult();
                            lastInitResp = initResp;
                            string? initError = JsonRpcHelper.ParseError(initResp);
                            if (initError != null)
                                throw new InvalidOperationException($"server rejected initialize (protocol {requestedVersion}): {initError}. Hint: try mcp_set_option with key 'protocol_version' and an older value such as '2024-11-05'");
                            oldTransport.NegotiatedVersion = McpProtocol.ParseNegotiatedVersion(initResp);
                            if (TryParseRequestId(initReq, out var id) && IsResponseForId(initResp, id))
                                break;
                            Thread.Sleep(opts.RetryDelayMs);
                        }
                        AgentCore.Instance.Logger.Info($"[McpClientService] initialize for '{serverId}': requested {requestedVersion}, negotiated {oldTransport.NegotiatedVersion ?? "<unreported>"}, response '{lastInitResp}'");
                        // Send initialized notification (no response expected per JSON-RPC 2.0)
                        string notif = System.Text.Json.JsonSerializer.Serialize(new { jsonrpc = "2.0", method = "notifications/initialized" });
                        oldTransport.SendNotificationAsync(notif).GetAwaiter().GetResult();
                    }
                    catch (Exception ex) {
                        conn.Dispose();
                        return $"error: initialize failed - {ex.Message}";
                    }
                }
            }
            else if (transport is IMcpOldTransport legacyHttpTransport) {
                // Legacy sse / streamable-http: Transport.ConnectAsync already sent initialize; complete handshake with notifications/initialized
                try {
                    Thread.Sleep(opts.StartupDelayMs);
                    string notif = System.Text.Json.JsonSerializer.Serialize(new { jsonrpc = "2.0", method = "notifications/initialized" });
                    legacyHttpTransport.SendNotificationAsync(notif).GetAwaiter().GetResult();
                    AgentCore.Instance.Logger.Info($"[McpClientService] initialize for '{serverId}' (transport '{type}'): requested {opts.ProtocolVersion ?? McpProtocol.Default}, negotiated {legacyHttpTransport.NegotiatedVersion ?? "<unreported>"}");
                }
                catch (Exception ex) {
                    AgentCore.Instance.Logger.Info($"[McpClientService] initialized notification failed for server '{serverId}' (transport '{type}'): {ex}");
                    conn.Dispose();
                    return $"error: initialized notification failed - {ex.Message}";
                }

            }
            else {
                // Modern streamable-http: stateless, no initialized notification
                AgentCore.Instance.Logger.Info($"[McpClientService] server '{serverId}' (streamable-http) uses stateless protocol {transport.ProtocolVersion}");
            }

            // Cache tool list
            try {
                string listReq = BuildRpcRequest(conn, "tools/list", null, conn.NextId());
                string listResp = transport.SendRequestAsync(listReq).GetAwaiter().GetResult();
                ParseAndCacheTools(conn, listResp);
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Info($"[McpClientService] failed to list tools for server '{serverId}' (transport '{type}'): {ex}");
                MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] Warning: failed to list tools for '{serverId}': {ex.Message}");
            }

            _servers[serverId] = conn;
            return "ok";
        }
        private static bool TryParseRequestId(string jsonRpc, out int id)
        {
            try {
                using var doc = System.Text.Json.JsonDocument.Parse(jsonRpc);
                if (doc.RootElement.TryGetProperty("id", out var idEl) &&
                    idEl.ValueKind == System.Text.Json.JsonValueKind.Number &&
                    idEl.TryGetInt32(out id)) {
                    return true;
                }
            }
            catch { }
            id = 0;
            return false;
        }
        private static bool IsResponseForId(string jsonRpc, int expectedId)
        {
            try {
                using var doc = System.Text.Json.JsonDocument.Parse(jsonRpc);
                if (doc.RootElement.TryGetProperty("id", out var idEl) &&
                    idEl.ValueKind == System.Text.Json.JsonValueKind.Number &&
                    idEl.TryGetInt32(out int id)) {
                    return id == expectedId;
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Disconnects and removes the server connection.
        /// </summary>
        public void Disconnect(string serverId)
        {
            // Cancel all active calls for this server
            string prefix = $"{serverId}:";
            foreach (var kv in _activeCts) {
                if (kv.Key.StartsWith(prefix)) {
                    try { kv.Value.Cancel(); } catch (ObjectDisposedException) { }
                }
            }

            if (_servers.TryRemove(serverId, out var conn))
                conn.Dispose();
        }

        /// <summary>
        /// Returns true if the server is connected.
        /// </summary>
        public bool IsConnected(string serverId)
        {
            return _servers.TryGetValue(serverId, out var conn) && conn.Transport.IsConnected;
        }

        /// <summary>
        /// Returns a human-readable tool list string for LLM consumption.
        /// Format: "tool_name: description\n  params: param1(type,required) param2(type) ..."
        /// </summary>
        public string ListTools(string serverId)
        {
            if (!_servers.TryGetValue(serverId, out var conn))
                return $"error: server '{serverId}' not connected";

            if (conn.Tools.Count == 0)
                return "(no tools available)";

            var sb = new StringBuilder();
            foreach (var tool in conn.Tools) {
                string namePart = string.IsNullOrEmpty(tool.Title) ? tool.Name : $"{tool.Name} ({tool.Title})";
                sb.AppendLine($"{namePart}: {tool.Description}");
                if (!string.IsNullOrEmpty(tool.InputSchemaJson)) {
                    string paramDesc = FormatParams(tool.InputSchemaJson);
                    if (!string.IsNullOrEmpty(paramDesc))
                        sb.AppendLine($"  params: {paramDesc}");
                }
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Lists resources exposed by the server (cursor-paginated, max 100 pages).
        /// Format: "uri: name - description (mimeType)" per line.
        /// </summary>
        public string ListResources(string serverId)
        {
            if (!_servers.TryGetValue(serverId, out var conn))
                return $"error: server '{serverId}' not connected";
            try {
                var lines = new List<string>();
                string? cursor = null;
                for (int page = 0; page < 100; page++) {
                    object? prm = cursor == null ? null : new { cursor };
                    string req = BuildRpcRequest(conn, "resources/list", prm, conn.NextId());
                    string resp = conn.Transport.SendRequestAsync(req).GetAwaiter().GetResult();
                    string? rpcError = JsonRpcHelper.ParseError(resp);
                    if (rpcError != null)
                        return $"[error] {rpcError}";
                    var result = JsonRpcHelper.ParseResult(resp);
                    if (result == null)
                        break;
                    if (result.Value.TryGetProperty("resources", out var resEl)) {
                        foreach (var r in resEl.EnumerateArray()) {
                            string uri = r.TryGetProperty("uri", out var u) ? u.GetString() ?? string.Empty : string.Empty;
                            string name = r.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty;
                            string desc = r.TryGetProperty("description", out var d) ? d.GetString() ?? string.Empty : string.Empty;
                            string mime = r.TryGetProperty("mimeType", out var m) ? m.GetString() ?? string.Empty : string.Empty;
                            string line = $"{uri}: {name}";
                            if (!string.IsNullOrEmpty(desc)) line += $" - {desc}";
                            if (!string.IsNullOrEmpty(mime)) line += $" ({mime})";
                            lines.Add(line);
                        }
                    }
                    if (!result.Value.TryGetProperty("nextCursor", out var ncEl) ||
                        ncEl.ValueKind != System.Text.Json.JsonValueKind.String ||
                        string.IsNullOrEmpty(ncEl.GetString()))
                        break;
                    cursor = ncEl.GetString();
                }
                return lines.Count == 0 ? "(no resources available)" : string.Join("\n", lines);
            }
            catch (Exception ex) {
                MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] ListResources error for '{serverId}': {ex.Message}");
                return $"[error] {ex.Message}";
            }
        }

        /// <summary>
        /// Reads a resource by uri and returns its content as text.
        /// Blob contents are summarized as "[binary N base64 chars]".
        /// </summary>
        public Task<string> ReadResource(string serverId, string uri)
        {
            return Task.Run(async () => {
                if (!_servers.TryGetValue(serverId, out var conn))
                    return $"error: server '{serverId}' not connected";
                try {
                    string req = BuildRpcRequest(conn, "resources/read", new { uri }, conn.NextId());

                    // Auto-reconnect if transport is no longer connected
                    if (!conn.Transport.IsConnected) {
                        var reconnectLock = _reconnectLocks.GetOrAdd(serverId, _ => new SemaphoreSlim(1, 1));
                        await reconnectLock.WaitAsync();
                        try {
                            if (_servers.TryGetValue(serverId, out var latest))
                                conn = latest;
                            if (!conn.Transport.IsConnected) {
                                MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] '{serverId}' disconnected, reconnecting...");
                                string reconnResult = Connect(serverId, conn.Type, conn.Target);
                                if (!reconnResult.StartsWith("ok"))
                                    return $"[error] reconnect failed: {reconnResult}";
                                if (!_servers.TryGetValue(serverId, out conn!))
                                    return "[error] server not found after reconnect";
                            }
                            req = BuildRpcRequest(conn, "resources/read", new { uri }, conn.NextId());
                        }
                        finally {
                            reconnectLock.Release();
                        }
                    }

                    string resp = await conn.Transport.SendRequestAsync(req);
                    return ExtractResourceContent(resp);
                }
                catch (Exception ex) {
                    MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] ReadResource error for '{serverId}' uri '{uri}': {ex.Message}");
                    return $"[error] {ex.Message}";
                }
            });
        }

        /// <summary>
        /// Reads an MCP resource asynchronously.
        /// tag: identifier forwarded to the mcp_callback CEF message.
        /// Returns "ok" immediately; result arrives via mcp_callback(serverId, tag, resultText).
        /// Shares the per-session queue with tool calls (key "serverId:tag").
        /// </summary>
        public string ReadResourceCallback(string serverId, string uri, string tag)
        {
            if (!_servers.TryGetValue(serverId, out var conn))
                return $"error: server '{serverId}' not connected";

            var ctx = MetaDslExecutor.CurrentContext;
            if (ctx == null)
                return "error: no request context";

            var session = GetSession(serverId, tag);
            var item = new WorkItem {
                ServerId = serverId,
                Tag = tag,
                Run = ct => ExecuteResourceRead(serverId, uri, tag, ct),
                Deliver = result => MetaDslExecutor.EnqueueCallback(ctx, "mcp_callback", new BoxedValue[] { serverId, tag, result })
            };
            return Submit(session, item) ? "ok" : "busy";
        }

        /// <summary>
        /// Performs a single resources/read request (with auto-reconnect) and
        /// returns the result text (or "[error] ..."). Used by the per-session
        /// queue worker.
        /// </summary>
        private async Task<string> ExecuteResourceRead(string serverId, string uri, string tag, CancellationToken token)
        {
            if (!_servers.TryGetValue(serverId, out var conn))
                return $"[error] server '{serverId}' not connected";
            try {
                string req = BuildRpcRequest(conn, "resources/read", new { uri }, conn.NextId());

                // Auto-reconnect if transport is no longer connected (with lock to prevent concurrent reconnects)
                if (!conn.Transport.IsConnected) {
                    var reconnectLock = _reconnectLocks.GetOrAdd(serverId, _ => new SemaphoreSlim(1, 1));
                    await reconnectLock.WaitAsync();
                    try {
                        if (_servers.TryGetValue(serverId, out var latest))
                            conn = latest;
                        if (!conn.Transport.IsConnected) // double-check inside lock
                        {
                            MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] '{serverId}' disconnected, reconnecting...");
                            string reconnResult = Connect(serverId, conn.Type, conn.Target);
                            if (!reconnResult.StartsWith("ok"))
                                return $"[error] reconnect failed: {reconnResult}";
                            if (!_servers.TryGetValue(serverId, out conn!))
                                return "[error] server not found after reconnect";
                        }
                        req = BuildRpcRequest(conn, "resources/read", new { uri }, conn.NextId());
                    }
                    finally {
                        reconnectLock.Release();
                    }
                }

                token.ThrowIfCancellationRequested();
                var sendTask = conn.Transport.SendRequestAsync(req);
                var cancelTask = Task.Delay(Timeout.Infinite, token);
                var completed = await Task.WhenAny(sendTask, cancelTask);
                if (completed == cancelTask)
                    throw new OperationCanceledException("MCP resource read cancelled");
                string resp = await sendTask;
                return ExtractResourceContent(resp);
            }
            catch (Exception ex) {
                MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] ReadResource error for '{serverId}' uri '{uri}': {ex.Message}");
                return $"[error] {ex.Message}";
            }
        }

        private static string ExtractResourceContent(string response)
        {
            var errMsg = JsonRpcHelper.ParseError(response);
            if (errMsg != null)
                return $"[error] {errMsg}";

            var result = JsonRpcHelper.ParseResult(response);
            if (result == null)
                return response; // fallback: return raw

            // MCP resources/read result: { contents: [ { uri, mimeType?, text? | blob? }, ... ] }
            if (!result.Value.TryGetProperty("contents", out var contentsEl))
                return result.Value.GetRawText();

            var sb = new StringBuilder();
            foreach (var c in contentsEl.EnumerateArray()) {
                if (c.TryGetProperty("text", out var textEl) && textEl.ValueKind == System.Text.Json.JsonValueKind.String) {
                    if (sb.Length > 0) sb.Append('\n');
                    sb.Append(textEl.GetString());
                }
                else if (c.TryGetProperty("blob", out var blobEl) && blobEl.ValueKind == System.Text.Json.JsonValueKind.String) {
                    if (sb.Length > 0) sb.Append('\n');
                    sb.Append($"[binary {blobEl.GetString()?.Length ?? 0} base64 chars]");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Lists prompts defined on the server (cursor-paginated, max 100 pages,
        /// with auto-reconnect). Format: "name: description" plus an optional
        /// "  args: arg1(required) arg2" line per prompt.
        /// </summary>
        public string ListPrompts(string serverId)
        {
            if (!_servers.TryGetValue(serverId, out var conn))
                return $"error: server '{serverId}' not connected";
            try {
                var lines = new List<string>();
                string? cursor = null;
                for (int page = 0; page < 100; page++) {
                    string? c = cursor;
                    conn = EnsureConnectedAsync(serverId, conn).GetAwaiter().GetResult();
                    object? prm = c == null ? null : new { cursor = c };
                    string req = BuildRpcRequest(conn, "prompts/list", prm, conn.NextId());
                    string resp = conn.Transport.SendRequestAsync(req).GetAwaiter().GetResult();
                    string? rpcError = JsonRpcHelper.ParseError(resp);
                    if (rpcError != null)
                        return $"[error] {rpcError}";
                    var result = JsonRpcHelper.ParseResult(resp);
                    if (result == null)
                        break;
                    if (result.Value.TryGetProperty("prompts", out var promptsEl)) {
                        foreach (var p in promptsEl.EnumerateArray()) {
                            string name = p.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty;
                            string desc = p.TryGetProperty("description", out var d) ? d.GetString() ?? string.Empty : string.Empty;
                            lines.Add($"{name}: {desc}");
                            if (p.TryGetProperty("arguments", out var argsEl) && argsEl.ValueKind == System.Text.Json.JsonValueKind.Array) {
                                var parts = new List<string>();
                                foreach (var a in argsEl.EnumerateArray()) {
                                    string argName = a.TryGetProperty("name", out var an) ? an.GetString() ?? string.Empty : string.Empty;
                                    bool argReq = a.TryGetProperty("required", out var rq) && rq.ValueKind == System.Text.Json.JsonValueKind.True;
                                    parts.Add(argReq ? $"{argName}(required)" : argName);
                                }
                                if (parts.Count > 0)
                                    lines.Add($"  args: {string.Join(" ", parts)}");
                            }
                        }
                    }
                    if (!result.Value.TryGetProperty("nextCursor", out var ncEl) ||
                        ncEl.ValueKind != System.Text.Json.JsonValueKind.String ||
                        string.IsNullOrEmpty(ncEl.GetString()))
                        break;
                    cursor = ncEl.GetString();
                }
                return lines.Count == 0 ? "(no prompts available)" : string.Join("\n", lines);
            }
            catch (Exception ex) {
                MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] ListPrompts error for '{serverId}': {ex.Message}");
                return $"[error] {ex.Message}";
            }
        }

        /// <summary>
        /// Renders a server-defined prompt template (prompts/get, with
        /// auto-reconnect) and returns the message texts joined as "[role] text"
        /// lines. resource_link contents are inlined via resources/read when possible.
        /// </summary>
        public Task<string> ReadPrompt(string serverId, string promptName, string argsJson)
        {
            return Task.Run(async () => {
                if (!_servers.TryGetValue(serverId, out var conn))
                    return $"error: server '{serverId}' not connected";
                try {
                    conn = await EnsureConnectedAsync(serverId, conn);
                    string req = BuildPromptGetRequest(conn, promptName, argsJson);
                    string resp = await conn.Transport.SendRequestAsync(req);
                    return ExtractPromptContent(resp, conn);
                }
                catch (Exception ex) {
                    MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] ReadPrompt error for '{serverId}/{promptName}': {ex.Message}");
                    return $"[error] {ex.Message}";
                }
            });
        }

        /// <summary>
        /// Renders a server-defined prompt template asynchronously.
        /// tag: identifier forwarded to the mcp_callback CEF message.
        /// Returns "ok" immediately; result arrives via mcp_callback(serverId, tag, resultText).
        /// Shares the per-session queue with tool/resource calls (key "serverId:tag").
        /// </summary>
        public string ReadPromptCallback(string serverId, string promptName, string argsJson, string tag)
        {
            if (!_servers.TryGetValue(serverId, out var conn))
                return $"error: server '{serverId}' not connected";

            var ctx = MetaDslExecutor.CurrentContext;
            if (ctx == null)
                return "error: no request context";

            var session = GetSession(serverId, tag);
            var item = new WorkItem {
                ServerId = serverId,
                Tag = tag,
                Run = ct => ExecutePromptGet(serverId, promptName, argsJson, tag, ct),
                Deliver = result => MetaDslExecutor.EnqueueCallback(ctx, "mcp_callback", new BoxedValue[] { serverId, tag, result })
            };
            return Submit(session, item) ? "ok" : "busy";
        }

        /// <summary>
        /// Performs a single prompts/get request (with auto-reconnect) and
        /// returns the result text (or "[error] ..."). Used by the per-session
        /// queue worker.
        /// </summary>
        private async Task<string> ExecutePromptGet(string serverId, string promptName, string argsJson, string tag, CancellationToken token)
        {
            if (!_servers.TryGetValue(serverId, out var conn))
                return $"[error] server '{serverId}' not connected";
            try {
                conn = await EnsureConnectedAsync(serverId, conn);
                string req = BuildPromptGetRequest(conn, promptName, argsJson);
                token.ThrowIfCancellationRequested();
                var sendTask = conn.Transport.SendRequestAsync(req);
                var cancelTask = Task.Delay(Timeout.Infinite, token);
                var completed = await Task.WhenAny(sendTask, cancelTask);
                if (completed == cancelTask)
                    throw new OperationCanceledException("MCP prompt get cancelled");
                string resp = await sendTask;
                return ExtractPromptContent(resp, conn);
            }
            catch (Exception ex) {
                MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] ReadPrompt error for '{serverId}/{promptName}': {ex.Message}");
                return $"[error] {ex.Message}";
            }
        }

        private static string BuildPromptGetRequest(McpServerConnection conn, string promptName, string argsJson)
        {
            object? arguments = null;
            if (!string.IsNullOrWhiteSpace(argsJson) && argsJson != "{}") {
                using var doc = System.Text.Json.JsonDocument.Parse(argsJson);
                arguments = doc.RootElement.Clone();
            }
            return BuildRpcRequest(conn, "prompts/get", new {
                name = promptName,
                arguments = arguments ?? new { }
            }, conn.NextId());
        }

        private static string ExtractPromptContent(string response, McpServerConnection conn)
        {
            var errMsg = JsonRpcHelper.ParseError(response);
            if (errMsg != null)
                return $"[error] {errMsg}";

            var result = JsonRpcHelper.ParseResult(response);
            if (result == null)
                return response; // fallback: return raw

            // MCP prompts/get result: { messages: [ { role, content }, ... ] }
            if (!result.Value.TryGetProperty("messages", out var msgsEl))
                return result.Value.GetRawText();

            var sb = new StringBuilder();
            foreach (var msg in msgsEl.EnumerateArray()) {
                string role = msg.TryGetProperty("role", out var r) ? r.GetString() ?? "user" : "user";
                if (sb.Length > 0) sb.Append('\n');
                sb.Append($"[{role}] ");
                if (msg.TryGetProperty("content", out var contentEl))
                    sb.Append(ExtractPromptContentItem(contentEl, conn));
            }
            return sb.ToString();
        }

        private static string ExtractPromptContentItem(System.Text.Json.JsonElement contentEl, McpServerConnection conn)
        {
            string? type = contentEl.TryGetProperty("type", out var t) ? t.GetString() : null;
            if (type == "text")
                return contentEl.TryGetProperty("text", out var textEl) && textEl.ValueKind == System.Text.Json.JsonValueKind.String
                    ? textEl.GetString() ?? string.Empty
                    : string.Empty;
            if (type == "image")
                return "[image]";
            if (type == "audio")
                return "[audio]";
            if (type == "resource") {
                // embedded resource: { resource: { uri, mimeType?, text? | blob? } }
                if (contentEl.TryGetProperty("resource", out var resEl)) {
                    if (resEl.TryGetProperty("text", out var rtEl) && rtEl.ValueKind == System.Text.Json.JsonValueKind.String)
                        return rtEl.GetString() ?? string.Empty;
                    if (resEl.TryGetProperty("blob", out var rbEl) && rbEl.ValueKind == System.Text.Json.JsonValueKind.String)
                        return $"[binary {rbEl.GetString()?.Length ?? 0} base64 chars]";
                }
                return "[resource]";
            }
            if (type == "resource_link" && contentEl.TryGetProperty("uri", out var uriEl)) {
                // uri reference: resolve to text via resources/read when possible
                string uri = uriEl.GetString() ?? string.Empty;
                return ReadResourceLinkText(conn, uri);
            }
            return contentEl.GetRawText();
        }

        private static string ReadResourceLinkText(McpServerConnection conn, string uri)
        {
            try {
                string req = BuildRpcRequest(conn, "resources/read", new { uri }, conn.NextId());
                string resp = conn.Transport.SendRequestAsync(req).GetAwaiter().GetResult();
                string content = ExtractResourceContent(resp);
                if (content.StartsWith("[error]"))
                    return $"[resource link: {uri}] ({content})";
                return content;
            }
            catch (Exception ex) {
                return $"[resource link: {uri}] ({ex.Message})";
            }
        }

        /// <summary>
        /// Calls an MCP tool asynchronously.
        /// argsJson: JSON object string of tool arguments, e.g. {"path":"/tmp/foo.txt"}
        /// tag: identifier forwarded to the mcp_callback CEF message
        /// Returns "ok" immediately; result arrives via mcp_callback(serverId, tag, resultText)
        /// </summary>
        public string CallToolCallback(string serverId, string toolName, string argsJson, string tag)
        {
            if (!_servers.TryGetValue(serverId, out var conn))
                return $"error: server '{serverId}' not connected";

            var ctx = MetaDslExecutor.CurrentContext;
            if (ctx == null)
                return "error: no request context";

            var session = GetSession(serverId, tag);
            var item = new WorkItem {
                ServerId = serverId,
                Tag = tag,
                Run = ct => ExecuteToolCall(serverId, toolName, argsJson, tag, ct),
                Deliver = result => MetaDslExecutor.EnqueueCallback(ctx, "mcp_callback", new BoxedValue[] { serverId, tag, result })
            };
            return Submit(session, item) ? "ok" : "busy";
        }

        /// <summary>
        /// Performs a single MCP tool call (with auto-reconnect) and returns the
        /// result text (or "[error] ...") . Used by the per-session queue worker.
        /// </summary>
        private async Task<string> ExecuteToolCall(string serverId, string toolName, string argsJson, string tag, CancellationToken token)
        {
            if (!_servers.TryGetValue(serverId, out var conn))
                return $"[error] server '{serverId}' not connected";
            try {
                object? args = null;
                if (!string.IsNullOrWhiteSpace(argsJson) && argsJson != "{}") {
                    using var doc = System.Text.Json.JsonDocument.Parse(argsJson);
                    args = doc.RootElement.Clone();
                }

                string req = BuildRpcRequest(conn, "tools/call", new {
                    name = toolName,
                    arguments = args ?? new { }
                }, conn.NextId());

                // Auto-reconnect if transport is no longer connected (with lock to prevent concurrent reconnects)
                if (!conn.Transport.IsConnected) {
                    var reconnectLock = _reconnectLocks.GetOrAdd(serverId, _ => new SemaphoreSlim(1, 1));
                    await reconnectLock.WaitAsync();
                    try {
                        if (_servers.TryGetValue(serverId, out var latest))
                            conn = latest;
                        if (!conn.Transport.IsConnected) // double-check inside lock
                        {
                            MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] '{serverId}' disconnected, reconnecting...");
                            string reconnResult = Connect(serverId, conn.Type, conn.Target);
                            if (!reconnResult.StartsWith("ok"))
                                return $"[error] reconnect failed: {reconnResult}";
                            if (!_servers.TryGetValue(serverId, out conn!))
                                return "[error] server not found after reconnect";
                        }
                        req = JsonRpcHelper.BuildRequest("tools/call", new {
                            name = toolName,
                            arguments = args ?? new { }
                        }, conn.NextId());
                    }
                    finally {
                        reconnectLock.Release();
                    }
                }

                token.ThrowIfCancellationRequested();
                var sendTask = conn.Transport.SendRequestAsync(req);
                var cancelTask = Task.Delay(Timeout.Infinite, token);
                var completed = await Task.WhenAny(sendTask, cancelTask);
                if (completed == cancelTask)
                    throw new OperationCanceledException("MCP tool call cancelled");
                string resp = await sendTask;
                return ExtractToolResult(resp);
            }
            catch (Exception ex) {
                MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] CallTool error for '{serverId}/{toolName}': {ex.Message}");
                return $"[error] {ex.Message}";
            }
        }

        /// <summary>
        /// Calls an MCP tool and returns the result directly via Task.
        /// Used by synchronous script expressions (mcp_call_tool).
        /// </summary>
        public Task<string> CallTool(string serverId, string toolName, string argsJson)
        {
            if (!_servers.TryGetValue(serverId, out var conn))
                return Task.FromResult($"error: server '{serverId}' not connected");

            string callKey = $"{serverId}:sync{Interlocked.Increment(ref s_syncCallId)}";
            _busyCalls[callKey] = true;
            _busySince[callKey] = DateTime.UtcNow;
            var cts = new CancellationTokenSource();
            _activeCts[callKey] = cts;

            return Task.Run(async () => {
                try {
                    object? args = null;
                    if (!string.IsNullOrWhiteSpace(argsJson) && argsJson != "{}") {
                        using var doc = System.Text.Json.JsonDocument.Parse(argsJson);
                        args = doc.RootElement.Clone();
                    }

                    string req = JsonRpcHelper.BuildRequest("tools/call", new {
                        name = toolName,
                        arguments = args ?? new { }
                    }, conn.NextId());

                    // Auto-reconnect if transport is no longer connected
                    if (!conn.Transport.IsConnected) {
                        var reconnectLock = _reconnectLocks.GetOrAdd(serverId, _ => new SemaphoreSlim(1, 1));
                        await reconnectLock.WaitAsync();
                        try {
                            if (_servers.TryGetValue(serverId, out var latest))
                                conn = latest;
                            if (!conn.Transport.IsConnected) {
                                MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] '{serverId}' disconnected, reconnecting...");
                                string reconnResult = Connect(serverId, conn.Type, conn.Target);
                                if (!reconnResult.StartsWith("ok"))
                                    return $"[error] reconnect failed: {reconnResult}";
                                if (!_servers.TryGetValue(serverId, out conn!))
                                    return "[error] server not found after reconnect";
                            }
                            req = BuildRpcRequest(conn, "tools/call", new {
                                name = toolName,
                                arguments = args ?? new { }
                            }, conn.NextId());
                        }
                        finally {
                            reconnectLock.Release();
                        }
                    }


                    cts.Token.ThrowIfCancellationRequested();
                    var sendTask = conn.Transport.SendRequestAsync(req);
                    var cancelTask = Task.Delay(Timeout.Infinite, cts.Token);
                    var completed = await Task.WhenAny(sendTask, cancelTask);
                    if (completed == cancelTask)
                        throw new OperationCanceledException("MCP tool call cancelled");
                    string resp = await sendTask;
                    return ExtractToolResult(resp);
                }
                catch (Exception ex) {
                    MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] CallToolForScript error for '{serverId}/{toolName}': {ex.Message}");
                    return $"[error] {ex.Message}";
                }
                finally {
                    _busyCalls[callKey] = false;
                    _busySince.TryRemove(callKey, out _);
                    _activeCts.TryRemove(callKey, out var oldCts);
                    oldCts?.Dispose();
                }
            });
        }

        /// <summary>Returns how many seconds the call has been busy (0 if not busy).</summary>
        public int GetBusyDuration(string serverId, string tag)
        {
            string callKey = $"{serverId}:{tag}";
            if (_busySince.TryGetValue(callKey, out var since))
                return (int)(DateTime.UtcNow - since).TotalSeconds;
            return 0;
        }

        /// <summary>Cancels an active MCP tool call for the given session.</summary>
        public string Cancel(string serverId, string tag)
        {
            string callKey = $"{serverId}:{tag}";
            if (_activeCts.TryGetValue(callKey, out var cts)) {
                try { cts.Cancel(); }
                catch (ObjectDisposedException) { }
                AgentCore.Instance.Logger.Info($"[McpClientService] Cancel requested for call '{callKey}'");
                return "ok";
            }
            return "error: call not active";
        }

        /// <summary>
        /// Returns the per-session queue (creating it lazily). The queue depth
        /// limit is refreshed from the server's max_queue_len option each call,
        /// so changes via mcp_set_option take effect immediately.
        /// </summary>
        private McpSession GetSession(string serverId, string tag)
        {
            string sessionKey = $"{serverId}:{tag}";
            var session = _sessions.GetOrAdd(sessionKey, k => new McpSession(k, GetMaxQueueLen(serverId)));
            session.MaxQueue = GetMaxQueueLen(serverId);
            return session;
        }

        /// <summary>
        /// Enqueues a work item and starts the session worker if it is idle.
        /// Returns false when the session queue is full (caller should report busy).
        /// </summary>
        private bool Submit(McpSession session, WorkItem item)
        {
            if (!session.TryEnqueue(item, out bool startWorker))
                return false;
            if (startWorker)
                StartWorker(session);
            return true;
        }

        /// <summary>
        /// Drains a session's queue on a background task, running items one at a
        /// time. Busy/cancellation tracking is maintained per running item so the
        /// watchdog and mcp_cancel keep working as before.
        /// </summary>
        private void StartWorker(McpSession session)
        {
            Task.Run(async () => {
                while (true) {
                    var item = session.Dequeue();
                    if (item == null)
                        break;

                    string sessionKey = session.Key;
                    var cts = new CancellationTokenSource();
                    _busyCalls[sessionKey] = true;
                    _busySince[sessionKey] = DateTime.UtcNow;
                    _activeCts[sessionKey] = cts;

                    string result;
                    try {
                        result = await item.Run(cts.Token);
                    }
                    catch (Exception ex) {
                        result = $"[error] {ex.Message}";
                    }
                    finally {
                        _busyCalls[sessionKey] = false;
                        _busySince.TryRemove(sessionKey, out _);
                        _activeCts.TryRemove(sessionKey, out var oldCts);
                        oldCts?.Dispose();
                    }

                    try { item.Deliver(result); }
                    catch (Exception ex) {
                        AgentCore.Instance.Logger.Error($"[McpClientService] Deliver error for '{item.ServerId}/{item.Tag}': {ex.Message}");
                    }
                }
            });
        }

        /// <summary>Reads max_queue_len option for a server (default c_defaultQueueSize).</summary>
        private int GetMaxQueueLen(string serverId)
        {
            if (_pendingOptions.TryGetValue(serverId, out var opts)) {
                lock (opts) {
                    if (opts.TryGetValue("max_queue_len", out var list) && list.Count > 0 &&
                        int.TryParse(list[list.Count - 1], out var len) && len >= 0)
                        return len;
                }
            }
            return c_defaultQueueSize;
        }

        /// <summary>Reads max_busy_seconds option for a server (default 600s).</summary>
        private int GetMaxBusySeconds(string serverId)
        {
            if (_pendingOptions.TryGetValue(serverId, out var opts)) {
                lock (opts) {
                    if (opts.TryGetValue("max_busy_seconds", out var list) && list.Count > 0 &&
                        int.TryParse(list[list.Count - 1], out var seconds) && seconds > 0)
                        return seconds;
                }
            }
            return c_defaultMaxBusySeconds;
        }

        /// <summary>Watchdog callback: auto-cancel calls busy longer than max_busy_seconds.</summary>
        private void WatchdogCallback(object? state)
        {
            try {
                var now = DateTime.UtcNow;
                foreach (var kv in _busySince) {
                    string callKey = kv.Key;
                    int duration = (int)(now - kv.Value).TotalSeconds;
                    int colonIdx = callKey.IndexOf(':');
                    if (colonIdx <= 0) continue;
                    string serverId = callKey.Substring(0, colonIdx);
                    int maxBusy = GetMaxBusySeconds(serverId);
                    if (duration > maxBusy) {
                        AgentCore.Instance.Logger.Info($"[McpClientService] Watchdog: call '{callKey}' busy for {duration}s (limit {maxBusy}s), auto-cancelling");
                        if (_activeCts.TryGetValue(callKey, out var cts)) {
                            try { cts.Cancel(); }
                            catch (ObjectDisposedException) { }
                        }
                    }
                }
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error($"[McpClientService] Watchdog error: {ex.Message}");
            }
        }

        // --- private helpers ---

        /// <summary>
        /// Returns a connected connection for the server, reconnecting (with a
        /// per-server lock to prevent concurrent reconnects) when the transport
        /// is no longer connected. Throws on reconnect failure.
        /// </summary>
        private async Task<McpServerConnection> EnsureConnectedAsync(string serverId, McpServerConnection conn)
        {
            if (conn.Transport.IsConnected)
                return conn;
            var reconnectLock = _reconnectLocks.GetOrAdd(serverId, _ => new SemaphoreSlim(1, 1));
            await reconnectLock.WaitAsync();
            try {
                if (_servers.TryGetValue(serverId, out var latest))
                    conn = latest;
                if (!conn.Transport.IsConnected) // double-check inside lock
                {
                    MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] '{serverId}' disconnected, reconnecting...");
                    string reconnResult = Connect(serverId, conn.Type, conn.Target);
                    if (!reconnResult.StartsWith("ok"))
                        throw new InvalidOperationException($"reconnect failed: {reconnResult}");
                    if (!_servers.TryGetValue(serverId, out conn!))
                        throw new InvalidOperationException("server not found after reconnect");
                }
                return conn;
            }
            finally {
                reconnectLock.Release();
            }
        }

        /// <summary>
        /// Handles server-initiated notifications received by the transport
        /// (currently only fired by the streamable-http GET stream).
        /// </summary>
        private void HandleServerNotification(string serverId, string json)
        {
            try {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("method", out var methodEl))
                    return;
                string method = methodEl.GetString() ?? string.Empty;
                if (method == "notifications/prompts/list_changed") {
                    // Prompt lists are fetched on demand (not cached), nothing to invalidate.
                    AgentCore.Instance.Logger.Info($"[McpClientService] server '{serverId}' reported prompts/list_changed");
                    MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] Info: server '{serverId}' prompt list changed, mcp_list_prompts returns the fresh list");
                }
                else if (method == "notifications/tools/list_changed") {
                    AgentCore.Instance.Logger.Info($"[McpClientService] server '{serverId}' reported tools/list_changed");
                    MetaDslExecutor.AppendApiErrorInfoLine($"[McpClientService] Info: server '{serverId}' tool list changed (cached list is refreshed on reconnect)");
                }
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Info($"[McpClientService] failed to handle server notification for '{serverId}': {ex.Message}");
            }
        }

        /// <summary>
        /// Builds a JSON-RPC request for a connection: 2026-07-28 (stateless)
        /// connections get _meta decoration (protocolVersion / clientInfo /
        /// clientCapabilities), legacy-era requests are sent undecorated.
        /// </summary>
        private static string BuildRpcRequest(McpServerConnection conn, string method, object? parameters, int id)
        {
            if (McpProtocol.IsModernVersion(conn.Transport.ProtocolVersion))
                return McpProtocol.BuildDecoratedRequest(method, parameters, id, conn.Transport.ProtocolVersion);
            return JsonRpcHelper.BuildRequest(method, parameters, id);
        }

        /// <summary>
        /// Detects the protocol era of a stdio server by probing server/discover
        /// over the started process pipe. Per the 2026-07-28 spec, fall back to
        /// the legacy handshake on any response that is not a recognized modern
        /// error/result. Returns true when the server speaks the stateless era.
        /// </summary>
        private bool DetectStdioEraModern(string serverId, IMcpTransport transport)
        {
            string version = McpProtocol.V2026_07_28;
            for (int attempt = 0; attempt < 2; attempt++) {
                try {
                    string req = McpProtocol.BuildDiscoverRequest(attempt + 1, version);
                    string resp = transport.SendRequestAsync(req).GetAwaiter().GetResult();
                    var cls = McpProtocol.ClassifyEra(resp, out var supported);
                    if (cls == McpEraClassification.ModernResult) {
                        if (transport is IMcpOldTransport old)
                            old.NegotiatedVersion = version;
                        return true;
                    }
                    if (cls == McpEraClassification.ModernError) {
                        string? modern = supported.FirstOrDefault(McpProtocol.IsModernVersion);
                        if (modern != null && modern != version) {
                            version = modern;
                            continue; // retry server/discover with the supported modern version
                        }
                        if (modern == null && supported.Any(v => McpProtocol.KnownVersions.Contains(v)))
                            return false; // dual-era server exposing only handshake-era versions
                        return true; // modern server; usage errors will surface on the first real request
                    }
                    return false; // legacy era (or unparseable response)
                }
                catch (Exception ex) {
                    AgentCore.Instance.Logger.Info($"[McpClientService] stdio era probe failed for '{serverId}' ({ex.Message}), assuming legacy handshake");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Extracts headers, timeout, and lsp_framing from pending options for a server.
        /// Header values containing %var% patterns are resolved here at Connect time.
        /// </summary>
        private TransportOptions ExtractTransportOptions(string serverId, int defaultTimeoutMs = 300000)
        {
            var result = new TransportOptions {
                TimeoutMs = defaultTimeoutMs,
                StartupDelayMs = 1000,
                RetryDelayMs = 500,
                RetryCount = 3
            };
            if (_pendingOptions.TryGetValue(serverId, out var opts)) {
                lock (opts) {
                    if (opts.TryGetValue("timeout", out var timeoutList) && timeoutList.Count > 0) {
                        if (int.TryParse(timeoutList[timeoutList.Count - 1], out int t) && t > 0)
                            result.TimeoutMs = t;
                    }
                    if (opts.TryGetValue("protocol_version", out var pvList) && pvList.Count > 0 &&
                        !string.IsNullOrWhiteSpace(pvList[pvList.Count - 1]))
                        result.ProtocolVersion = pvList[pvList.Count - 1].Trim();
                    if (opts.TryGetValue("header", out var headerList)) {
                        var rawValues = headerList.ToArray();
                        var resolved = AgentCore.Instance.ResolveEnvironmentValues("mcp", serverId, rawValues);
                        result.Headers = new Dictionary<string, string>();
                        for (int i = 0; i < resolved.Length; i++) {
                            int colonIdx = resolved[i].IndexOf(':');
                            if (colonIdx > 0)
                                result.Headers[resolved[i].Substring(0, colonIdx).Trim()] = resolved[i].Substring(colonIdx + 1).Trim();
                        }
                    }
                    if (opts.TryGetValue("lsp_framing", out var lspList) && lspList.Count > 0)
                        result.UseLspFraming = string.Equals(lspList[0], "true", StringComparison.OrdinalIgnoreCase);
                    if (opts.TryGetValue("startup_delay", out var sdList) && sdList.Count > 0) {
                        if (int.TryParse(sdList[sdList.Count - 1], out int sd) && sd >= 0)
                            result.StartupDelayMs = sd;
                    }
                    if (opts.TryGetValue("retry_delay", out var rdList) && rdList.Count > 0) {
                        if (int.TryParse(rdList[rdList.Count - 1], out int rd) && rd >= 0)
                            result.RetryDelayMs = rd;
                    }
                    if (opts.TryGetValue("retry_count", out var rcList) && rcList.Count > 0) {
                        if (int.TryParse(rcList[rcList.Count - 1], out int rc) && rc > 0)
                            result.RetryCount = rc;
                    }
                }
            }
            return result;
        }

        private static void ParseAndCacheTools(McpServerConnection conn, string listResp)
        {
            var result = JsonRpcHelper.ParseResult(listResp);
            if (result == null) return;

            if (!result.Value.TryGetProperty("tools", out var toolsEl)) return;

            foreach (var toolEl in toolsEl.EnumerateArray()) {
                var tool = new McpTool();
                if (toolEl.TryGetProperty("name", out var nameEl))
                    tool.Name = nameEl.GetString() ?? string.Empty;
                if (toolEl.TryGetProperty("title", out var titleEl))
                    tool.Title = titleEl.GetString() ?? string.Empty;
                if (toolEl.TryGetProperty("description", out var descEl))
                    tool.Description = descEl.GetString() ?? string.Empty;
                if (toolEl.TryGetProperty("inputSchema", out var schemaEl))
                    tool.InputSchemaJson = schemaEl.GetRawText();
                if (!string.IsNullOrEmpty(tool.Name))
                    conn.Tools.Add(tool);
            }
        }

        private static string FormatParams(string schemaJson)
        {
            try {
                using var doc = System.Text.Json.JsonDocument.Parse(schemaJson);
                var root = doc.RootElement;
                if (!root.TryGetProperty("properties", out var props)) return string.Empty;

                var required = new HashSet<string>();
                if (root.TryGetProperty("required", out var reqEl))
                    foreach (var r in reqEl.EnumerateArray())
                        required.Add(r.GetString() ?? string.Empty);

                var parts = new List<string>();
                foreach (var prop in props.EnumerateObject()) {
                    string ptype = "any";
                    if (prop.Value.TryGetProperty("type", out var typeEl))
                        ptype = typeEl.GetString() ?? "any";
                    string req = required.Contains(prop.Name) ? ",required" : "";
                    parts.Add($"{prop.Name}({ptype}{req})");
                }
                return string.Join(" ", parts);
            }
            catch {
                return string.Empty;
            }
        }

        private static string ExtractToolResult(string response)
        {
            var errMsg = JsonRpcHelper.ParseError(response);
            if (errMsg != null)
                return $"[error] {errMsg}";

            var result = JsonRpcHelper.ParseResult(response);
            if (result == null)
                return response; // fallback: return raw

            // 2026-07-28 MRTR (resultType "input_required"): the server needs
            // additional input; the call must be retried with inputResponses.
            // Surfaced to the caller instead of auto-answering.
            if (result.Value.TryGetProperty("resultType", out var rtEl) &&
                rtEl.ValueKind == System.Text.Json.JsonValueKind.String &&
                rtEl.GetString() == "input_required") {
                string inputRequests = result.Value.TryGetProperty("inputRequests", out var irEl) ? irEl.GetRawText() : "[]";
                return $"[input required] server needs additional input, retry with inputResponses; inputRequests: {inputRequests}";
            }

            // MCP tools/call result: { content: [...], structuredContent: {...}, isError: bool }
            string text = result.Value.TryGetProperty("content", out var contentEl)
                ? ExtractContentText(contentEl)
                : string.Empty;

            // structuredContent (2025-03-26+): fallback when no textual content is present
            if (text.Length == 0 && result.Value.TryGetProperty("structuredContent", out var structuredEl))
                text = structuredEl.GetRawText();

            if (text.Length == 0 && !result.Value.TryGetProperty("content", out _))
                return result.Value.GetRawText(); // no content array: return raw result

            bool isError = result.Value.TryGetProperty("isError", out var isErrEl) &&
                isErrEl.ValueKind == System.Text.Json.JsonValueKind.True;
            return isError ? $"[tool error] {text}" : text;
        }

        private static string ExtractContentText(System.Text.Json.JsonElement contentEl)
        {
            var sb = new StringBuilder();
            foreach (var item in contentEl.EnumerateArray()) {
                string? type = item.TryGetProperty("type", out var typeEl) ? typeEl.GetString() : null;
                if (item.TryGetProperty("text", out var textEl) && textEl.ValueKind == System.Text.Json.JsonValueKind.String)
                    sb.Append(textEl.GetString());
                else if (type == "image")
                    sb.Append("[image]");
                else if (type == "audio")
                    sb.Append("[audio]");
                else if (type == "resource" && item.TryGetProperty("resource", out var resEl) &&
                         resEl.TryGetProperty("text", out var resTextEl) && resTextEl.ValueKind == System.Text.Json.JsonValueKind.String)
                    sb.Append(resTextEl.GetString());
            }
            return sb.ToString();
        }

        /// <summary>A single queued MCP tool call.</summary>
        private sealed class WorkItem
        {
            public string ServerId = "";
            public string Tag = "";
            public Func<CancellationToken, Task<string>> Run = _ => Task.FromResult("");
            public Action<string> Deliver = _ => { };
        }

        /// <summary>
        /// A per-session FIFO queue with a single active worker. At most one item
        /// runs at a time; up to MaxQueue additional items may wait in line.
        /// </summary>
        private sealed class McpSession
        {
            public readonly string Key;
            public int MaxQueue;
            private readonly Queue<WorkItem> _pending = new();
            private bool _running;

            public McpSession(string key, int maxQueue)
            {
                Key = key;
                MaxQueue = maxQueue;
            }

            /// <summary>
            /// Enqueues an item. When the session is idle the item is accepted and
            /// startWorker is set true. When busy the item is queued unless the
            /// queue already holds MaxQueue waiting items, in which case false is
            /// returned (caller should report busy).
            /// </summary>
            public bool TryEnqueue(WorkItem item, out bool startWorker)
            {
                startWorker = false;
                lock (_pending) {
                    if (_running) {
                        if (_pending.Count >= MaxQueue)
                            return false;
                        _pending.Enqueue(item);
                        return true;
                    }
                    _pending.Enqueue(item);
                    _running = true;
                    startWorker = true;
                    return true;
                }
            }

            /// <summary>
            /// Returns the next item, or null when the queue is empty (which also
            /// marks the session idle so a future submit restarts a worker).
            /// </summary>
            public WorkItem? Dequeue()
            {
                lock (_pending) {
                    if (_pending.Count == 0) {
                        _running = false;
                        return null;
                    }
                    return _pending.Dequeue();
                }
            }
        }
    }
}
