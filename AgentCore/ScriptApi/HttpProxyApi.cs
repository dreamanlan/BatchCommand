using System;
using System.Collections.Generic;
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
    /// Registers all HTTP proxy APIs
    /// </summary>
    public static class HttpProxyApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("httpproxy_start_server", "httpproxy_start_server(port) - start the local CORS reverse proxy (loopback only; http/https upstream in the 'target' query parameter, websocket upgrades tunnelled to ws/wss targets), returns bool",
                new ExpressionFactoryHelper<HttpProxyStartServerExp>());
            BatchCommand.BatchScript.Register("httpproxy_stop_server", "httpproxy_stop_server(port) - stop the local CORS reverse proxy, returns bool",
                new ExpressionFactoryHelper<HttpProxyStopServerExp>());
            BatchCommand.BatchScript.Register("httpproxy_get_request_count", "httpproxy_get_request_count(port) - total proxied request count since start, returns integer",
                new ExpressionFactoryHelper<HttpProxyGetRequestCountExp>());
        }
    }
}
