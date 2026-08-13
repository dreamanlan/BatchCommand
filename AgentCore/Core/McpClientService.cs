using System;
using AgentPlugin.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Abstraction for MCP transport layer (stdio, HTTP+SSE, streamable-http, etc.).
    /// </summary>
    internal interface IMcpTransport : IDisposable
    {
        bool IsConnected { get; }
        Task ConnectAsync();
        /// <summary>
        /// Sends a JSON-RPC request and returns the raw JSON-RPC response string.
        /// </summary>
        Task<string> SendRequestAsync(string jsonRpcRequest);
        /// <summary>
        /// Sends a JSON-RPC notification (no id, no response expected).
        /// Implementations MUST NOT wait for any response.
        /// </summary>
        Task SendNotificationAsync(string jsonRpcNotification);
        void Disconnect();
    }

    /// <summary>
    /// Helper to build JSON-RPC 2.0 request strings.
    /// </summary>
    internal static class JsonRpcHelper
    {
        public static string BuildRequest(string method, object? parameters, int id)
        {
            if (parameters == null)
            {
                var req = new { jsonrpc = "2.0", id, method };
                return System.Text.Json.JsonSerializer.Serialize(req);
            }
            var reqWithParams = new { jsonrpc = "2.0", id, method, @params = parameters };
            return System.Text.Json.JsonSerializer.Serialize(reqWithParams);
        }

        public static System.Text.Json.JsonElement? ParseResult(string response)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(response);
                if (doc.RootElement.TryGetProperty("result", out var result))
                    return result.Clone();
            }
            catch { }
            return null;
        }

        public static string? ParseError(string response)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(response);
                if (doc.RootElement.TryGetProperty("error", out var err))
                {
                    if (err.TryGetProperty("message", out var msg))
                        return msg.GetString();
                    return err.GetRawText();
                }
            }
            catch { }
            return null;
        }
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
    /// Describes a single MCP tool (name, description, parameter schema).
    /// </summary>
    public class McpTool
    {
        public string Name { get; set; } = string.Empty;
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
        /// Supported keys: "timeout" (milliseconds), "header" (Name:Value, can be called multiple times).
        /// If value contains %var% patterns, they are resolved at Connect time (not here).
        /// Options are NOT consumed by Connect; call ClearOptions to remove them.
        /// </summary>
        public void SetOption(string serverId, string key, string value)
        {
            // Store raw value (may contain %var% templates); resolved at Connect time
            var opts = _pendingOptions.GetOrAdd(serverId, _ => new Dictionary<string, List<string>>());
            lock (opts)
            {
                if (!opts.TryGetValue(key, out var list))
                {
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

            IMcpTransport transport;
            if (type == "stdio")
            {
                var opts = ExtractTransportOptions(serverId, 60000);
                transport = new StdioTransport(target, opts.TimeoutMs, opts.UseLspFraming);
            }
            else if (type == "sse")
                transport = BuildSseTransport(serverId, target);
            else if (type == "streamable-http")
                transport = BuildStreamableHttpTransport(serverId, target);
            else
                return $"error: unknown transport type '{type}', use 'stdio', 'sse' or 'streamable-http'";

            try
            {
                transport.ConnectAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                transport.Dispose();
                return $"error: connect failed - {ex.Message}";
            }

            var conn = new McpServerConnection(transport, type, target);

            // For stdio, send initialize handshake
            if (type == "stdio")
            {
                try
                {
                    var stdioOpts = ExtractTransportOptions(serverId, 60000);
                    Thread.Sleep(stdioOpts.StartupDelayMs);
                    string initReq = JsonRpcHelper.BuildRequest("initialize", new
                    {
                        protocolVersion = "2024-11-05",
                        capabilities = new { },
                        clientInfo = new { name = "AgentCore", version = "1.0" }
                    }, conn.NextId());
                    for (int i = 0; i < stdioOpts.RetryCount; i++) {
                        string initResp = transport.SendRequestAsync(initReq).GetAwaiter().GetResult();
                        AgentCore.Instance.Logger.Info($"Initialized handshake received from server '{initResp}'");
                        if (TryParseRequestId(initReq, out var id) && IsResponseForId(initResp, id))
                            break;
                        Thread.Sleep(stdioOpts.RetryDelayMs);
                    }
                    // Send initialized notification (no response expected per JSON-RPC 2.0)
                    string notif = System.Text.Json.JsonSerializer.Serialize(new { jsonrpc = "2.0", method = "notifications/initialized" });
                    transport.SendNotificationAsync(notif).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    conn.Dispose();
                    return $"error: initialize failed - {ex.Message}";
                }
            }
            else if (type == "sse" || type == "streamable-http")
            {
                try
                {
                    // Transport.ConnectAsync already sent initialize; complete handshake with notifications/initialized
                    string notif = System.Text.Json.JsonSerializer.Serialize(new { jsonrpc = "2.0", method = "notifications/initialized" });
                    transport.SendNotificationAsync(notif).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    conn.Dispose();
                    return $"error: initialized notification failed - {ex.Message}";
                }
            }

            // Cache tool list
            try
            {
                string listReq = JsonRpcHelper.BuildRequest("tools/list", null, conn.NextId());
                string listResp = transport.SendRequestAsync(listReq).GetAwaiter().GetResult();
                ParseAndCacheTools(conn, listResp);
            }
            catch (Exception ex)
            {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[McpClientService] Warning: failed to list tools for '{serverId}': {ex.Message}");
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
            foreach (var kv in _activeCts)
            {
                if (kv.Key.StartsWith(prefix))
                {
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
            foreach (var tool in conn.Tools)
            {
                sb.AppendLine($"{tool.Name}: {tool.Description}");
                if (!string.IsNullOrEmpty(tool.InputSchemaJson))
                {
                    string paramDesc = FormatParams(tool.InputSchemaJson);
                    if (!string.IsNullOrEmpty(paramDesc))
                        sb.AppendLine($"  params: {paramDesc}");
                }
            }
            return sb.ToString().TrimEnd();
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

            var nativeApi = AgentCore.Instance.GetNativeApi();
            if (nativeApi == null)
                return "error: nativeApi not available";

            string callKey = $"{serverId}:{tag}";
            if (_busyCalls.TryGetValue(callKey, out var busy) && busy)
                return "busy";

            _busyCalls[callKey] = true;
            _busySince[callKey] = DateTime.UtcNow;
            var cts = new CancellationTokenSource();
            _activeCts[callKey] = cts;

            Task.Run(async () =>
            {
                try
                {
                    object? args = null;
                    if (!string.IsNullOrWhiteSpace(argsJson) && argsJson != "{}")
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(argsJson);
                        args = doc.RootElement.Clone();
                    }

                    string req = JsonRpcHelper.BuildRequest("tools/call", new
                    {
                        name = toolName,
                        arguments = args ?? new { }
                    }, conn.NextId());

                    // Auto-reconnect if transport is no longer connected (with lock to prevent concurrent reconnects)
                    if (!conn.Transport.IsConnected)
                    {
                        var reconnectLock = _reconnectLocks.GetOrAdd(serverId, _ => new SemaphoreSlim(1, 1));
                        await reconnectLock.WaitAsync();
                        try
                        {
                            if (_servers.TryGetValue(serverId, out var latest))
                                conn = latest;
                            if (!conn.Transport.IsConnected) // double-check inside lock
                            {
                                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[McpClientService] '{serverId}' disconnected, reconnecting...");
                                string reconnResult = Connect(serverId, conn.Type, conn.Target);
                                if (!reconnResult.StartsWith("ok"))
                                {
                                nativeApi.EnqueueCefMessage("mcp_callback", new string[] { serverId, tag, $"[error] reconnect failed: {reconnResult}" });
                                return;
                                }
                                if (!_servers.TryGetValue(serverId, out conn!))
                                {
                                nativeApi.EnqueueCefMessage("mcp_callback", new string[] { serverId, tag, "[error] server not found after reconnect" });
                                return;
                                }
                            }
                            req = JsonRpcHelper.BuildRequest("tools/call", new
                            {
                                name = toolName,
                                arguments = args ?? new { }
                            }, conn.NextId());
                        }
                        finally
                        {
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
                    string result = ExtractToolResult(resp);
                    nativeApi.EnqueueCefMessage("mcp_callback", new string[] { serverId, tag, result });
                }
                catch (Exception ex)
                {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[McpClientService] CallTool error for '{serverId}/{toolName}': {ex.Message}");
                    nativeApi.EnqueueCefMessage("mcp_callback", new string[] { serverId, tag, $"[error] {ex.Message}" });
                }
                finally
                {
                    _busyCalls[callKey] = false;
                    _busySince.TryRemove(callKey, out _);
                    _activeCts.TryRemove(callKey, out var oldCts);
                    oldCts?.Dispose();
                }
            });

            return "ok";
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

            return Task.Run(async () =>
            {
                try
                {
                    object? args = null;
                    if (!string.IsNullOrWhiteSpace(argsJson) && argsJson != "{}")
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(argsJson);
                        args = doc.RootElement.Clone();
                    }

                    string req = JsonRpcHelper.BuildRequest("tools/call", new
                    {
                        name = toolName,
                        arguments = args ?? new { }
                    }, conn.NextId());

                    // Auto-reconnect if transport is no longer connected
                    if (!conn.Transport.IsConnected)
                    {
                        var reconnectLock = _reconnectLocks.GetOrAdd(serverId, _ => new SemaphoreSlim(1, 1));
                        await reconnectLock.WaitAsync();
                        try
                        {
                            if (_servers.TryGetValue(serverId, out var latest))
                                conn = latest;
                            if (!conn.Transport.IsConnected)
                            {
                                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[McpClientService] '{serverId}' disconnected, reconnecting...");
                                string reconnResult = Connect(serverId, conn.Type, conn.Target);
                                if (!reconnResult.StartsWith("ok"))
                                    return $"[error] reconnect failed: {reconnResult}";
                                if (!_servers.TryGetValue(serverId, out conn!))
                                    return "[error] server not found after reconnect";
                            }
                            req = JsonRpcHelper.BuildRequest("tools/call", new
                            {
                                name = toolName,
                                arguments = args ?? new { }
                            }, conn.NextId());
                        }
                        finally
                        {
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
                catch (Exception ex)
                {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[McpClientService] CallToolForScript error for '{serverId}/{toolName}': {ex.Message}");
                    return $"[error] {ex.Message}";
                }
                finally
                {
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
            if (_activeCts.TryGetValue(callKey, out var cts))
            {
                try { cts.Cancel(); }
                catch (ObjectDisposedException) { }
                AgentFrameworkService.Instance.Log($"[McpClientService] Cancel requested for call '{callKey}'");
                return "ok";
            }
            return "error: call not active";
        }

        /// <summary>Reads max_busy_seconds option for a server (default 600s).</summary>
        private int GetMaxBusySeconds(string serverId)
        {
            if (_pendingOptions.TryGetValue(serverId, out var opts))
            {
                lock (opts)
                {
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
            try
            {
                var now = DateTime.UtcNow;
                foreach (var kv in _busySince)
                {
                    string callKey = kv.Key;
                    int duration = (int)(now - kv.Value).TotalSeconds;
                    int colonIdx = callKey.IndexOf(':');
                    if (colonIdx <= 0) continue;
                    string serverId = callKey.Substring(0, colonIdx);
                    int maxBusy = GetMaxBusySeconds(serverId);
                    if (duration > maxBusy)
                    {
                        AgentFrameworkService.Instance.Log($"[McpClientService] Watchdog: call '{callKey}' busy for {duration}s (limit {maxBusy}s), auto-cancelling");
                        if (_activeCts.TryGetValue(callKey, out var cts))
                        {
                            try { cts.Cancel(); }
                            catch (ObjectDisposedException) { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AgentFrameworkService.Instance.Log($"[McpClientService] Watchdog error: {ex.Message}");
            }
        }

        // --- private helpers ---

        /// <summary>
        /// Extracts headers, timeout, and lsp_framing from pending options for a server.
        /// Header values containing %var% patterns are resolved here at Connect time.
        /// </summary>
        private TransportOptions ExtractTransportOptions(string serverId, int defaultTimeoutMs = 300000)
        {
            var result = new TransportOptions
            {
                TimeoutMs = defaultTimeoutMs,
                StartupDelayMs = 1000,
                RetryDelayMs = 500,
                RetryCount = 3
            };
            if (_pendingOptions.TryGetValue(serverId, out var opts))
            {
                lock (opts)
                {
                    if (opts.TryGetValue("timeout", out var timeoutList) && timeoutList.Count > 0)
                    {
                        if (int.TryParse(timeoutList[timeoutList.Count - 1], out int t) && t > 0)
                            result.TimeoutMs = t;
                    }
                    if (opts.TryGetValue("header", out var headerList))
                    {
                        var rawValues = headerList.ToArray();
                        var resolved = AgentCore.Instance.ResolveEnvironmentValues("mcp", serverId, rawValues);
                        result.Headers = new Dictionary<string, string>();
                        for (int i = 0; i < resolved.Length; i++)
                        {
                            int colonIdx = resolved[i].IndexOf(':');
                            if (colonIdx > 0)
                                result.Headers[resolved[i].Substring(0, colonIdx).Trim()] = resolved[i].Substring(colonIdx + 1).Trim();
                        }
                    }
                    if (opts.TryGetValue("lsp_framing", out var lspList) && lspList.Count > 0)
                        result.UseLspFraming = string.Equals(lspList[0], "true", StringComparison.OrdinalIgnoreCase);
                    if (opts.TryGetValue("startup_delay", out var sdList) && sdList.Count > 0)
                    {
                        if (int.TryParse(sdList[sdList.Count - 1], out int sd) && sd >= 0)
                            result.StartupDelayMs = sd;
                    }
                    if (opts.TryGetValue("retry_delay", out var rdList) && rdList.Count > 0)
                    {
                        if (int.TryParse(rdList[rdList.Count - 1], out int rd) && rd >= 0)
                            result.RetryDelayMs = rd;
                    }
                    if (opts.TryGetValue("retry_count", out var rcList) && rcList.Count > 0)
                    {
                        if (int.TryParse(rcList[rcList.Count - 1], out int rc) && rc > 0)
                            result.RetryCount = rc;
                    }
                }
            }
            return result;
        }

        private SseTransport BuildSseTransport(string serverId, string url)
        {
            var opts = ExtractTransportOptions(serverId);
            return new SseTransport(url, opts.Headers, opts.TimeoutMs);
        }

        private StreamableHttpTransport BuildStreamableHttpTransport(string serverId, string url)
        {
            var opts = ExtractTransportOptions(serverId);
            return new StreamableHttpTransport(url, opts.Headers, opts.TimeoutMs);
        }

        private static void ParseAndCacheTools(McpServerConnection conn, string listResp)
        {
            var result = JsonRpcHelper.ParseResult(listResp);
            if (result == null) return;

            if (!result.Value.TryGetProperty("tools", out var toolsEl)) return;

            foreach (var toolEl in toolsEl.EnumerateArray())
            {
                var tool = new McpTool();
                if (toolEl.TryGetProperty("name", out var nameEl))
                    tool.Name = nameEl.GetString() ?? string.Empty;
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
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(schemaJson);
                var root = doc.RootElement;
                if (!root.TryGetProperty("properties", out var props)) return string.Empty;

                var required = new HashSet<string>();
                if (root.TryGetProperty("required", out var reqEl))
                    foreach (var r in reqEl.EnumerateArray())
                        required.Add(r.GetString() ?? string.Empty);

                var parts = new List<string>();
                foreach (var prop in props.EnumerateObject())
                {
                    string ptype = "any";
                    if (prop.Value.TryGetProperty("type", out var typeEl))
                        ptype = typeEl.GetString() ?? "any";
                    string req = required.Contains(prop.Name) ? ",required" : "";
                    parts.Add($"{prop.Name}({ptype}{req})");
                }
                return string.Join(" ", parts);
            }
            catch
            {
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

            // MCP tools/call result: { content: [ { type: "text", text: "..." }, ... ] }
            if (result.Value.TryGetProperty("content", out var contentEl))
            {
                var sb = new StringBuilder();
                foreach (var item in contentEl.EnumerateArray())
                {
                    if (item.TryGetProperty("text", out var textEl))
                        sb.Append(textEl.GetString());
                    else if (item.TryGetProperty("type", out var typeEl) && typeEl.GetString() == "image")
                        sb.Append("[image]");
                }
                return sb.ToString();
            }

            return result.Value.GetRawText();
        }
    }
}
