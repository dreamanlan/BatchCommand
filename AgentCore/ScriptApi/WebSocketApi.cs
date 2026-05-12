using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    /// <summary>
    /// WebSocket server manager supporting multiple ports
    /// </summary>
    public static class WebSocketServerManager
    {
        private static readonly Dictionary<int, Core.WebSocketServer> _servers = new();
        private static readonly object _lockObj = new object();

        /// <summary>
        /// Gets or creates a WebSocket server instance for the specified port
        /// </summary>
        public static Core.WebSocketServer GetServer(int port)
        {
            lock (_lockObj) {
                if (!_servers.TryGetValue(port, out var server)) {
                    server = new Core.WebSocketServer();
                    _servers[port] = server;
                }
                return server;
            }
        }

        /// <summary>
        /// Gets whether server on specified port is running
        /// </summary>
        public static bool IsRunning(int port)
        {
            lock (_lockObj) {
                return _servers.TryGetValue(port, out var server) && server.IsRunning;
            }
        }

        /// <summary>
        /// Stops and disposes the server on specified port
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
        /// Stops and disposes all servers
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
    /// Starts WebSocket server on specified port
    /// Usage: ws_start_server(port)
    /// Returns: true if successful, false otherwise
    /// </summary>
    sealed class WsStartServerExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: ws_start_server(port)");
                return BoxedValue.FromBool(false);
            }

            try {
                int port = operands[0].GetInt();
                if (port <= 0 || port > 65535) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Invalid port number: {port}");
                    return BoxedValue.FromBool(false);
                }

                var server = WebSocketServerManager.GetServer(port);
                bool result = server.Start(port);
                if (result) {
                    // Bind the WS server to the AgentInstance for this port
                    var instance = Core.AgentCore.Instance.GetOrCreateInstance(port);
                    instance.WsServer = server;
                }
                return BoxedValue.FromBool(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"ws_start_server error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Stops the WebSocket server
    /// Usage: ws_stop_server(port)
    /// </summary>
    sealed class WsStopServerExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: ws_stop_server(port)");
                return BoxedValue.FromBool(false);
            }
            try {
                int port = operands[0].GetInt();
                WebSocketServerManager.StopServer(port);
                return BoxedValue.FromBool(true);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"ws_stop_server error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Gets the number of connected clients
    /// Usage: ws_get_client_count(port)
    /// Returns: integer count
    /// </summary>
    sealed class WsGetClientCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: ws_get_client_count(port)");
                return BoxedValue.From(0);
            }
            try {
                int port = operands[0].GetInt();
                if (!WebSocketServerManager.IsRunning(port)) {
                    return BoxedValue.From(0);
                }
                var server = WebSocketServerManager.GetServer(port);
                int count = server.GetClientCount();
                return BoxedValue.From(count);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"ws_get_client_count error: {ex.Message}");
                return BoxedValue.From(0);
            }
        }
    }

    /// <summary>
    /// Gets the count of pending messages in receive queue
    /// Usage: ws_get_receive_queue_count(port)
    /// Returns: integer count
    /// </summary>
    sealed class WsGetReceiveQueueCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: ws_get_receive_queue_count(port)");
                return BoxedValue.From(0);
            }
            try {
                int port = operands[0].GetInt();
                if (!WebSocketServerManager.IsRunning(port)) {
                    return BoxedValue.From(0);
                }
                var server = WebSocketServerManager.GetServer(port);
                int count = server.GetReceiveQueueCount();
                return BoxedValue.From(count);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"ws_get_receive_queue_count error: {ex.Message}");
                return BoxedValue.From(0);
            }
        }
    }

    /// <summary>
    /// Registers all WebSocket APIs
    /// </summary>
    public static class WebSocketApi
    {
        public static void RegisterApis()
        {
            AgentFrameworkService.Instance.DslEngine!.Register("ws_start_server", "ws_start_server(port)",
                new ExpressionFactoryHelper<WsStartServerExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("ws_stop_server", "ws_stop_server(port)",
                new ExpressionFactoryHelper<WsStopServerExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("ws_get_client_count", "ws_get_client_count(port)",
                new ExpressionFactoryHelper<WsGetClientCountExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("ws_get_receive_queue_count", "ws_get_receive_queue_count(port)",
                new ExpressionFactoryHelper<WsGetReceiveQueueCountExp>());
        }
    }
}
