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
        }
    }
}
