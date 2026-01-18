using System;
using System.Collections.Generic;
using System.Text.Json;
using DotnetStoryScript;

using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Command structure from inject.js
    /// </summary>
    public class AgentCommand
    {
        public long Id { get; set; }
        public string Command { get; set; }
        public Dictionary<string, object> Params { get; set; }
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// Response structure to inject.js
    /// </summary>
    public class AgentResponse
    {
        public long Id { get; set; }
        public bool Success { get; set; }
        public object Data { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// AgentBridge - Communication bridge with inject.js
    /// Responsible for structured protocol communication (commands/responses) with inject.js
    /// Uses window.onAgentCommand and window.onAgentResponse for bidirectional communication
    /// </summary>
    public class AgentBridge
    {
        private Action<string, string[]> _executeJsAction;
        private readonly Action<string> _log;

        public AgentBridge(Action<string, string[]> executeJsAction, Action<string> log)
        {
            _executeJsAction = executeJsAction;
            _log = log;
        }

        // Set the callback to execute JavaScript
        public void SetExecuteJsAction(Action<string, string[]> executeJsAction)
        {
            _executeJsAction = executeJsAction;
        }

        /// <summary>
        /// Send command to inject.js via window.onAgentCommand
        /// </summary>
        public void SendCommandToInject(string command, Dictionary<string, object> parameters)
        {
            try {
                var cmd = new {
                    command = command,
                    @params = parameters
                };

                var options = new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                string json = JsonSerializer.Serialize(cmd, options);

                // All C# to JS calls go through window object methods
                // Pass JSON as array parameter
                _executeJsAction("window.onAgentCommand", new string[] { json });
                _log($"[AgentCommand] Sending command to inject.js: {command}");
            }
            catch (Exception ex) {
                _log($"[AgentCommand] Error sending command: {ex.Message}");
            }
        }

        /// <summary>
        /// Send response to inject.js via window.onAgentResponse
        /// </summary>
        public void SendResponseToInject(string responseJson)
        {
            try {
                // All C# to JS calls go through window object methods
                // Pass JSON as array parameter
                _executeJsAction("window.onAgentResponse", new string[] { responseJson });
                _log($"[AgentResponse] Sending response to inject.js");
            }
            catch (Exception ex) {
                _log($"[AgentResponse] Error sending response: {ex.Message}");
            }
        }
    }
}
