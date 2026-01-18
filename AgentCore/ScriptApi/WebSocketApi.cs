using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    /// <summary>
    /// WebSocket server singleton manager
    /// </summary>
    public static class WebSocketServerManager
    {
        private static Core.WebSocketServer _server;
        private static readonly object _lockObj = new object();

        /// <summary>
        /// Gets or creates the WebSocket server instance
        /// </summary>
        public static Core.WebSocketServer GetServer()
        {
            lock (_lockObj)
            {
                if (_server == null)
                {
                    _server = new Core.WebSocketServer();
                }
                return _server;
            }
        }

        /// <summary>
        /// Gets whether server is running
        /// </summary>
        public static bool IsRunning()
        {
            lock (_lockObj)
            {
                return _server?.IsRunning ?? false;
            }
        }

        /// <summary>
        /// Stops and disposes the server
        /// </summary>
        public static void StopServer()
        {
            lock (_lockObj)
            {
                _server?.Stop();
                _server?.Dispose();
                _server = null;
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
            if (operands.Count < 1)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("ws_start_server requires port parameter");
                return BoxedValue.FromBool(false);
            }

            try
            {
                int port = operands[0].GetInt();
                if (port <= 0 || port > 65535)
                {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Invalid port number: {port}");
                    return BoxedValue.FromBool(false);
                }

                var server = WebSocketServerManager.GetServer();
                bool result = server.Start(port);
                return BoxedValue.FromBool(result);
            }
            catch (Exception ex)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"ws_start_server error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Stops the WebSocket server
    /// Usage: ws_stop_server()
    /// </summary>
    sealed class WsStopServerExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                WebSocketServerManager.StopServer();
                return BoxedValue.FromBool(true);
            }
            catch (Exception ex)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"ws_stop_server error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Gets the number of connected clients
    /// Usage: ws_get_client_count()
    /// Returns: integer count
    /// </summary>
    sealed class WsGetClientCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                var server = WebSocketServerManager.GetServer();
                if (!server.IsRunning)
                {
                    return BoxedValue.From(0);
                }

                int count = server.GetClientCount();
                return BoxedValue.From(count);
            }
            catch (Exception ex)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"ws_get_client_count error: {ex.Message}");
                return BoxedValue.From(0);
            }
        }
    }

    /// <summary>
    /// Gets the count of pending messages in receive queue
    /// Usage: ws_get_receive_queue_count()
    /// Returns: integer count
    /// </summary>
    sealed class WsGetReceiveQueueCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                var server = WebSocketServerManager.GetServer();
                if (!server.IsRunning)
                {
                    return BoxedValue.From(0);
                }

                int count = server.GetReceiveQueueCount();
                return BoxedValue.From(count);
            }
            catch (Exception ex)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"ws_get_receive_queue_count error: {ex.Message}");
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
            BatchCommand.BatchScript.Register("ws_start_server", "ws_start_server(port)",
                new ExpressionFactoryHelper<WsStartServerExp>());
            BatchCommand.BatchScript.Register("ws_stop_server", "ws_stop_server()",
                new ExpressionFactoryHelper<WsStopServerExp>());
            BatchCommand.BatchScript.Register("ws_get_client_count", "ws_get_client_count()",
                new ExpressionFactoryHelper<WsGetClientCountExp>());
            BatchCommand.BatchScript.Register("ws_get_receive_queue_count", "ws_get_receive_queue_count()",
                new ExpressionFactoryHelper<WsGetReceiveQueueCountExp>());
        }
    }
}