using System;
using System.Collections.Generic;
using System.IO;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using BatchCommand;

namespace AgentCore.ScriptApi
{
    /// <summary>
    /// HTTP proxy server manager supporting multiple ports (mirrors
    /// WebSocketServerManager). Instances are created by
    /// httpproxy_start_server(port) and disposed by httpproxy_stop_server(port).
    /// </summary>
    public static class HttpProxyServerManager
    {
        private static readonly Dictionary<int, Core.HttpProxyServer> _servers = new();
        private static readonly object _lockObj = new object();

        /// <summary>
        /// Gets or creates a proxy server instance for the specified port
        /// </summary>
        public static Core.HttpProxyServer GetServer(int port)
        {
            lock (_lockObj) {
                if (!_servers.TryGetValue(port, out var server)) {
                    server = new Core.HttpProxyServer();
                    _servers[port] = server;
                }
                return server;
            }
        }

        /// <summary>
        /// Gets whether the proxy on the specified port is running
        /// </summary>
        public static bool IsRunning(int port)
        {
            lock (_lockObj) {
                return _servers.TryGetValue(port, out var server) && server.IsRunning;
            }
        }

        /// <summary>
        /// Stops and disposes the proxy on the specified port
        /// </summary>
        public static void StopServer(int port)
        {
            lock (_lockObj) {
                if (_servers.TryGetValue(port, out var server)) {
                    server.Stop();
                    server.Dispose();
                    _servers.Remove(port);
                }
            }
        }

        /// <summary>
        /// Stops and disposes all proxies
        /// </summary>
        public static void StopAll()
        {
            lock (_lockObj) {
                foreach (var server in _servers.Values) {
                    server.Stop();
                    server.Dispose();
                }
                _servers.Clear();
            }
        }
    }

    /// <summary>
    /// Starts the local HTTP reverse proxy on the specified port (loopback
    /// only, CORS enabled, upstream passed in the 'target' query parameter;
    /// see Core/HttpProxyServer.cs).
    /// Usage: httpproxy_start_server(port)
    /// Returns: true if successful, false otherwise
    /// </summary>
    sealed class HttpProxyStartServerExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: httpproxy_start_server(port)");
                return BoxedValue.FromBool(false);
            }

            try {
                int port = operands[0].GetInt();
                if (port <= 0 || port > 65535) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"Invalid port number: {port}");
                    return BoxedValue.FromBool(false);
                }

                var server = HttpProxyServerManager.GetServer(port);
                bool result = server.Start(port);
                return BoxedValue.FromBool(result);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"httpproxy_start_server error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Stops the local HTTP reverse proxy
    /// Usage: httpproxy_stop_server(port)
    /// </summary>
    sealed class HttpProxyStopServerExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: httpproxy_stop_server(port)");
                return BoxedValue.FromBool(false);
            }
            try {
                int port = operands[0].GetInt();
                HttpProxyServerManager.StopServer(port);
                return BoxedValue.FromBool(true);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"httpproxy_stop_server error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Gets the total number of proxied requests since the proxy started
    /// Usage: httpproxy_get_request_count(port)
    /// Returns: integer count (0 when not running)
    /// </summary>
    sealed class HttpProxyGetRequestCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: httpproxy_get_request_count(port)");
                return BoxedValue.From(0);
            }
            try {
                int port = operands[0].GetInt();
                if (!HttpProxyServerManager.IsRunning(port)) {
                    return BoxedValue.From(0);
                }
                var server = HttpProxyServerManager.GetServer(port);
                return BoxedValue.From(server.RequestCount);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"httpproxy_get_request_count error: {ex.Message}");
                return BoxedValue.From(0);
            }
        }
    }

    /// <summary>
    /// Adds a url filter to the http proxy on the given port.
    /// Usage: httpproxy_add_filter(port, direction [, pattern ...] [, fields_json])
    ///   direction: "request" | "response"
    ///   pattern: plain substring of the full target url (case insensitive;
    ///     only plain characters, anything else compiles as a regex, "re:"
    ///     forces regex) - several patterns are AND-ed
    ///   fields_json: {"host":"..","path":"..","method":"..","type":".."} (one
    ///     clause, AND of its fields) or an array of such objects (OR between
    ///     clauses); "type" matches the response content type and is only
    ///     valid for the "response" direction; the table is AND-ed with the
    ///     positional patterns
    /// Filters are OR-ed: the first matching filter wins. Returns the filter
    /// id (increasing per port, -1 on error).
    /// </summary>
    sealed class HttpProxyAddFilterExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: httpproxy_add_filter(port, direction [, pattern ...] [, fields_json])");
                return BoxedValue.From(-1);
            }
            try {
                int port = operands[0].GetInt();
                string dir = (operands[1].AsString ?? string.Empty).Trim().ToLowerInvariant();
                Core.UrlFilterEngine.FilterDirection direction;
                if (dir == "request") {
                    direction = Core.UrlFilterEngine.FilterDirection.Request;
                }
                else if (dir == "response") {
                    direction = Core.UrlFilterEngine.FilterDirection.Response;
                }
                else {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"Invalid direction '{dir}' (expected 'request' or 'response')");
                    return BoxedValue.From(-1);
                }
                var patterns = new List<string>();
                List<Dictionary<string, string>>? clauses = null;
                for (int i = 2; i < operands.Count; i++) {
                    string s = operands[i].AsString ?? string.Empty;
                    if (s.Length == 0) {
                        continue;
                    }
                    // A trailing json object/array is the fields table, not a url pattern.
                    if ((s.StartsWith("{") || s.StartsWith("[")) && clauses == null) {
                        if (!Core.UrlFilterEngine.TryParseFieldClauses(s, out var parsed, out string fieldError)) {
                            AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"Invalid fields_json: {fieldError}");
                            return BoxedValue.From(-1);
                        }
                        clauses = parsed;
                    }
                    else {
                        patterns.Add(s);
                    }
                }
                if (patterns.Count == 0 && clauses == null) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("httpproxy_add_filter requires at least one pattern or a fields_json");
                    return BoxedValue.From(-1);
                }
                if (clauses != null) {
                    foreach (var clause in clauses) {
                        if (clause.ContainsKey("type") && direction == Core.UrlFilterEngine.FilterDirection.Request) {
                            AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("field 'type' is only valid for the 'response' direction");
                            return BoxedValue.From(-1);
                        }
                    }
                }
                var server = HttpProxyServerManager.GetServer(port);
                return BoxedValue.From(server.Filters.AddFilter(direction, patterns, clauses ?? new List<Dictionary<string, string>>()));
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"httpproxy_add_filter error: {ex.Message}");
                return BoxedValue.From(-1);
            }
        }
    }

    /// <summary>
    /// Removes a filter previously added with httpproxy_add_filter.
    /// Usage: httpproxy_remove_filter(port, filter_id)
    /// Returns: true when the filter was found and removed
    /// </summary>
    sealed class HttpProxyRemoveFilterExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: httpproxy_remove_filter(port, filter_id)");
                return BoxedValue.FromBool(false);
            }
            try {
                int port = operands[0].GetInt();
                int id = operands[1].GetInt();
                return BoxedValue.FromBool(HttpProxyServerManager.GetServer(port).Filters.RemoveFilter(id));
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"httpproxy_remove_filter error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Removes every filter of the http proxy on the given port.
    /// Usage: httpproxy_clear_filters(port)
    /// Returns: true
    /// </summary>
    sealed class HttpProxyClearFiltersExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: httpproxy_clear_filters(port)");
                return BoxedValue.FromBool(false);
            }
            try {
                int port = operands[0].GetInt();
                HttpProxyServerManager.GetServer(port).Filters.ClearFilters();
                return BoxedValue.FromBool(true);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"httpproxy_clear_filters error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Sets the filter callback dsl file for the http proxy on the given
    /// port. The file defines the callbacks (per filter, all optional):
    ///   on_proxy_request(filter_id, method, url, headers_json, body_b64)
    ///     return "" (no change) or a json
    ///     {"status":n,"set_headers":{..},"del_headers":[..],"body":"<b64>",
    ///      "follow_redirects":false,"abort":true}
    ///   on_proxy_response(filter_id, status, url, headers_json, body_b64)
    ///     same return json (follow_redirects ignored)
    /// One active filter script per process is the intended usage (callback
    /// names distinguish proxy / web server); the file is hot reloaded on
    /// timestamp change.
    /// Usage: httpproxy_set_filter_dsl(port, path)
    /// Returns: true when the file exists
    /// </summary>
    sealed class HttpProxySetFilterDslExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: httpproxy_set_filter_dsl(port, path)");
                return BoxedValue.FromBool(false);
            }
            try {
                int port = operands[0].GetInt();
                string path = operands[1].AsString ?? string.Empty;
                if (!File.Exists(path)) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"filter dsl file not found: {path}");
                    return BoxedValue.FromBool(false);
                }
                HttpProxyServerManager.GetServer(port).SetFilterDsl(path);
                return BoxedValue.FromBool(true);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"httpproxy_set_filter_dsl error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Registers all HTTP proxy APIs
    /// </summary>
    public static class HttpProxyApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("httpproxy_start_server", "httpproxy_start_server(port) - start the local CORS reverse proxy (loopback only; http/https upstream in the 'target' query parameter, websocket upgrades tunnelled to ws/wss targets, optional 'origin' query parameter overrides the upstream Origin header), returns bool",
                new ExpressionFactoryHelper<HttpProxyStartServerExp>());
            BatchCommand.BatchScript.Register("httpproxy_stop_server", "httpproxy_stop_server(port) - stop the local CORS reverse proxy, returns bool",
                new ExpressionFactoryHelper<HttpProxyStopServerExp>());
            BatchCommand.BatchScript.Register("httpproxy_get_request_count", "httpproxy_get_request_count(port) - total proxied request count since start, returns integer",
                new ExpressionFactoryHelper<HttpProxyGetRequestCountExp>());
            BatchCommand.BatchScript.Register("httpproxy_add_filter", "httpproxy_add_filter(port, direction [, pattern ...] [, fields_json]) - add a url filter (direction 'request'|'response'); patterns are case insensitive substrings of the target url (regex metacharacters compile as a regex, 're:' forces regex), several are AND-ed; fields_json = {\"host\",\"path\",\"method\",\"type\"} clause (AND) or an array of clauses (OR), AND-ed with the patterns ('type' matches the response content type, response direction only); filters are OR-ed, first match wins; returns the filter id or -1",
                new ExpressionFactoryHelper<HttpProxyAddFilterExp>());
            BatchCommand.BatchScript.Register("httpproxy_remove_filter", "httpproxy_remove_filter(port, filter_id) - remove one filter added by httpproxy_add_filter, returns bool",
                new ExpressionFactoryHelper<HttpProxyRemoveFilterExp>());
            BatchCommand.BatchScript.Register("httpproxy_clear_filters", "httpproxy_clear_filters(port) - remove every filter of the proxy, returns bool",
                new ExpressionFactoryHelper<HttpProxyClearFiltersExp>());
            BatchCommand.BatchScript.Register("httpproxy_set_filter_dsl", "httpproxy_set_filter_dsl(port, path) - set the filter callback dsl file (on_proxy_request / on_proxy_response, see Core/WebCallbacks.cs for the callback contract; one active filter script per process), returns bool",
                new ExpressionFactoryHelper<HttpProxySetFilterDslExp>());
        }
    }
}
