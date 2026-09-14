using System;
using System.Collections.Generic;
using ScriptableFramework;
using BatchCommand;
using BatchCommand.Utils;

namespace AgentCore.Core
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

    // Note: the former AgentBridge class (window.onAgentCommand/onAgentResponse
    // native js calls) has been retired. Delivery to inject.js now goes through
    // AgentPush over the websocket connections.
}
