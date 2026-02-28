using System;
using System.Collections.Generic;
using DotnetStoryScript;

using AgentPlugin.Abstractions;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Command structure from inject.js
    /// </summary>
    public class AgentCommand
    {
        public long Id { get; set; }
        public string? Command { get; set; }
        public Dictionary<string, object>? Params { get; set; }
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// Notification structure from inject.js (no response expected)
    /// Format: { type: "notification_type", data: { ... } }
    /// </summary>
    public class AgentNotification
    {
        public string? Type { get; set; }
        public Dictionary<string, object>? Data { get; set; }
    }

    /// <summary>
    /// Response structure to inject.js
    /// </summary>
    public class AgentResponse
    {
        public long Id { get; set; }
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// AgentBridge - Communication bridge with inject.js
    /// Responsible for structured protocol communication (commands/responses) with inject.js
    /// Uses window.onAgentCommand and window.onAgentResponse for bidirectional communication
    /// </summary>
    public class AgentBridge
    {
        private Action<string, string[]>? _sendJsCallAction;
        private readonly Action<string> _log;

        public AgentBridge(Action<string, string[]>? sendJsCallAction, Action<string> log)
        {
            _sendJsCallAction = sendJsCallAction;
            _log = log;
        }

        // Set the callback to send JavaScript call
        public void SetSendJsCallAction(Action<string, string[]> sendJsCallAction)
        {
            _sendJsCallAction = sendJsCallAction;
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

                var options = new System.Text.Json.JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };
                string json = System.Text.Json.JsonSerializer.Serialize(cmd, options);

                // All C# to JS calls go through window object methods
                // Pass JSON as array parameter
                _sendJsCallAction?.Invoke("window.onAgentCommand", new string[] { json });
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
                _sendJsCallAction?.Invoke("window.onAgentResponse", new string[] { responseJson });
                _log($"[AgentResponse] Sending response to inject.js");
            }
            catch (Exception ex) {
                _log($"[AgentResponse] Error sending response: {ex.Message}");
            }
        }
    }
}
