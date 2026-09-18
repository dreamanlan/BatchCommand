using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using BatchCommand;

namespace AgentCore.ScriptApi
{
    /// <summary>
    /// Static web server manager supporting multiple ports (mirrors
    /// HttpProxyServerManager). Instances are created by
    /// webserver_start_server(port, root, ...) and disposed by
    /// webserver_stop_server(port).
    /// </summary>
    public static class WebServerManager
    {
        private static readonly Dictionary<int, Core.WebServer> _servers = new();
        private static readonly object _lockObj = new object();

        /// <summary>
        /// Gets or creates a web server instance for the specified port
        /// </summary>
        public static Core.WebServer GetServer(int port)
        {
            lock (_lockObj) {
                if (!_servers.TryGetValue(port, out var server)) {
                    server = new Core.WebServer();
                    _servers[port] = server;
                }
                return server;
            }
        }

        /// <summary>
        /// Gets whether the web server on the specified port is running
        /// </summary>
        public static bool IsRunning(int port)
        {
            lock (_lockObj) {
                return _servers.TryGetValue(port, out var server) && server.IsRunning;
            }
        }

        /// <summary>
        /// Stops and disposes the web server on the specified port
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
        /// Stops and disposes all web servers
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
    /// Starts a static web server on the specified port (loopback only)
    /// serving the given document root. *.dsl urls are executed as dsl pages
    /// (main() entry); see Core/WebServer.cs.
    /// Usage: webserver_start_server(port, root [, headers_json [, use_https]])
    ///   headers_json: [{"match":"*.html","headers":{"Name":"value"}}] (ordered glob rules)
    ///   use_https: requires the certificate pre-bound with netsh (see webserver_setup_selfsigned_cert)
    /// Returns: true if successful, false otherwise
    /// </summary>
    sealed class WebServerStartServerExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 4) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: webserver_start_server(port, root [, headers_json [, use_https]])");
                return BoxedValue.FromBool(false);
            }

            try {
                int port = operands[0].GetInt();
                if (port <= 0 || port > 65535) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"Invalid port number: {port}");
                    return BoxedValue.FromBool(false);
                }
                string root = operands[1].AsString ?? string.Empty;
                List<Core.WebServer.HeaderRule>? rules = null;
                if (operands.Count > 2 && !operands[2].IsNullObject && !string.IsNullOrEmpty(operands[2].AsString)) {
                    try {
                        rules = Core.WebServer.ParseHeaderRules(operands[2].AsString);
                    }
                    catch (Exception ex) {
                        AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"Invalid headers_json: {ex.Message}");
                        return BoxedValue.FromBool(false);
                    }
                }
                bool useHttps = operands.Count > 3 ? operands[3].GetBool() : false;

                var server = WebServerManager.GetServer(port);
                bool result = server.Start(port, root, useHttps);
                if (result && rules != null) {
                    server.SetHeaderRules(rules);
                }
                return BoxedValue.FromBool(result);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"webserver_start_server error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Stops a static web server
    /// Usage: webserver_stop_server(port)
    /// </summary>
    sealed class WebServerStopServerExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: webserver_stop_server(port)");
                return BoxedValue.FromBool(false);
            }
            try {
                int port = operands[0].GetInt();
                WebServerManager.StopServer(port);
                return BoxedValue.FromBool(true);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"webserver_stop_server error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Replaces the custom response header rules of a running web server
    /// Usage: webserver_set_headers(port, headers_json)
    /// Returns: true if successful, false otherwise
    /// </summary>
    sealed class WebServerSetHeadersExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: webserver_set_headers(port, headers_json)");
                return BoxedValue.FromBool(false);
            }
            try {
                int port = operands[0].GetInt();
                if (!WebServerManager.IsRunning(port)) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"web server on port {port} is not running");
                    return BoxedValue.FromBool(false);
                }
                var rules = Core.WebServer.ParseHeaderRules(operands[1].AsString ?? string.Empty);
                WebServerManager.GetServer(port).SetHeaderRules(rules);
                return BoxedValue.FromBool(true);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"webserver_set_headers error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Gets the total number of requests served since the web server started
    /// Usage: webserver_get_request_count(port)
    /// Returns: integer count (0 when not running)
    /// </summary>
    sealed class WebServerGetRequestCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: webserver_get_request_count(port)");
                return BoxedValue.From(0);
            }
            try {
                int port = operands[0].GetInt();
                if (!WebServerManager.IsRunning(port)) {
                    return BoxedValue.From(0);
                }
                var server = WebServerManager.GetServer(port);
                return BoxedValue.From(server.RequestCount);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"webserver_get_request_count error: {ex.Message}");
                return BoxedValue.From(0);
            }
        }
    }

    /// <summary>
    /// Adds or overrides a MIME mapping for the web servers (process wide,
    /// applies to every running web server). The extension is normalized to
    /// a lowercase ".ext" form; an empty content type removes the entry.
    /// Usage: webserver_set_mime(ext, content_type)
    ///   e.g. webserver_set_mime(".model", "application/octet-stream")
    /// Returns: true
    /// </summary>
    sealed class WebServerSetMimeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: webserver_set_mime(ext, content_type)");
                return BoxedValue.FromBool(false);
            }
            try {
                Core.WebServer.SetMimeType(operands[0].AsString ?? string.Empty, operands[1].AsString ?? string.Empty);
                return BoxedValue.FromBool(true);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"webserver_set_mime error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Adds a response url filter to the web server on the given port.
    /// Usage: webserver_add_filter(port [, pattern ...] [, fields_json])
    ///   pattern: plain substring of the request path (case insensitive;
    ///     only plain characters, anything else compiles as a regex, "re:"
    ///     forces regex) - several patterns are AND-ed
    ///   fields_json: {"host":"..","path":"..","method":"..","type":".."} (one
    ///     clause, AND of its fields) or an array of such objects (OR between
    ///     clauses); "type" matches the served content type; the table is
    ///     AND-ed with the positional patterns
    /// Filters are OR-ed: the first matching filter wins. Returns the filter
    /// id (increasing per port, -1 on error).
    /// </summary>
    sealed class WebServerAddFilterExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: webserver_add_filter(port [, pattern ...] [, fields_json])");
                return BoxedValue.From(-1);
            }
            try {
                int port = operands[0].GetInt();
                var patterns = new List<string>();
                List<Dictionary<string, string>>? clauses = null;
                for (int i = 1; i < operands.Count; i++) {
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
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("webserver_add_filter requires at least one pattern or a fields_json");
                    return BoxedValue.From(-1);
                }
                var server = WebServerManager.GetServer(port);
                return BoxedValue.From(server.Filters.AddFilter(Core.UrlFilterEngine.FilterDirection.Response, patterns, clauses ?? new List<Dictionary<string, string>>()));
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"webserver_add_filter error: {ex.Message}");
                return BoxedValue.From(-1);
            }
        }
    }

    /// <summary>
    /// Removes a filter previously added with webserver_add_filter.
    /// Usage: webserver_remove_filter(port, filter_id)
    /// Returns: true when the filter was found and removed
    /// </summary>
    sealed class WebServerRemoveFilterExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: webserver_remove_filter(port, filter_id)");
                return BoxedValue.FromBool(false);
            }
            try {
                int port = operands[0].GetInt();
                int id = operands[1].GetInt();
                return BoxedValue.FromBool(WebServerManager.GetServer(port).Filters.RemoveFilter(id));
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"webserver_remove_filter error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Removes every filter of the web server on the given port.
    /// Usage: webserver_clear_filters(port)
    /// Returns: true
    /// </summary>
    sealed class WebServerClearFiltersExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: webserver_clear_filters(port)");
                return BoxedValue.FromBool(false);
            }
            try {
                int port = operands[0].GetInt();
                WebServerManager.GetServer(port).Filters.ClearFilters();
                return BoxedValue.FromBool(true);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"webserver_clear_filters error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Sets the filter callback dsl file for the web server on the given
    /// port. The file defines the callback (per filter, optional):
    ///   on_webserver_response(filter_id, status, path, headers_json, body_b64)
    ///     return "" (no change) or a json
    ///     {"status":n,"set_headers":{..},"del_headers":[..],"body":"<b64>",
    ///      "abort":true}
    /// Any returned modification takes over the response (Range requests
    /// then get a full 200 body). One active filter script per process is
    /// the intended usage; the file is hot reloaded on timestamp change.
    /// Usage: webserver_set_filter_dsl(port, path)
    /// Returns: true when the file exists
    /// </summary>
    sealed class WebServerSetFilterDslExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: webserver_set_filter_dsl(port, path)");
                return BoxedValue.FromBool(false);
            }
            try {
                int port = operands[0].GetInt();
                string path = operands[1].AsString ?? string.Empty;
                if (!System.IO.File.Exists(path)) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"filter dsl file not found: {path}");
                    return BoxedValue.FromBool(false);
                }
                WebServerManager.GetServer(port).SetFilterDsl(path);
                return BoxedValue.FromBool(true);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"webserver_set_filter_dsl error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Registers all web server APIs
    /// </summary>
    public static class WebServerApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("webserver_start_server", "webserver_start_server(port, root [, headers_json [, use_https]]) - start a static web server on the port serving the root dir (loopback only; *.dsl urls execute as dsl pages with a main() entry; headers_json = ordered glob rules [{\"match\":\"*.html\",\"headers\":{...}}]), returns bool",
                new ExpressionFactoryHelper<WebServerStartServerExp>());
            BatchCommand.BatchScript.Register("webserver_stop_server", "webserver_stop_server(port) - stop the static web server, returns bool",
                new ExpressionFactoryHelper<WebServerStopServerExp>());
            BatchCommand.BatchScript.Register("webserver_set_headers", "webserver_set_headers(port, headers_json) - replace the custom response header rules of a running web server, returns bool",
                new ExpressionFactoryHelper<WebServerSetHeadersExp>());
            BatchCommand.BatchScript.Register("webserver_set_mime", "webserver_set_mime(ext, content_type) - add or override a MIME mapping for the web servers (process wide; empty content type removes the entry), returns bool",
                new ExpressionFactoryHelper<WebServerSetMimeExp>());
            BatchCommand.BatchScript.Register("webserver_get_request_count", "webserver_get_request_count(port) - total request count since start, returns integer",
                new ExpressionFactoryHelper<WebServerGetRequestCountExp>());
            BatchCommand.BatchScript.Register("webserver_add_filter", "webserver_add_filter(port [, pattern ...] [, fields_json]) - add a response url filter; patterns are case insensitive substrings of the request path (regex metacharacters compile as a regex, 're:' forces regex), several are AND-ed; fields_json = {\"host\",\"path\",\"method\",\"type\"} clause (AND) or an array of clauses (OR), AND-ed with the patterns ('type' matches the served content type); filters are OR-ed, first match wins; returns the filter id or -1",
                new ExpressionFactoryHelper<WebServerAddFilterExp>());
            BatchCommand.BatchScript.Register("webserver_remove_filter", "webserver_remove_filter(port, filter_id) - remove one filter added by webserver_add_filter, returns bool",
                new ExpressionFactoryHelper<WebServerRemoveFilterExp>());
            BatchCommand.BatchScript.Register("webserver_clear_filters", "webserver_clear_filters(port) - remove every filter of the web server, returns bool",
                new ExpressionFactoryHelper<WebServerClearFiltersExp>());
            BatchCommand.BatchScript.Register("webserver_set_filter_dsl", "webserver_set_filter_dsl(port, path) - set the filter callback dsl file (on_webserver_response, see Core/WebCallbacks.cs for the callback contract; one active filter script per process), returns bool",
                new ExpressionFactoryHelper<WebServerSetFilterDslExp>());
        }
    }
}
