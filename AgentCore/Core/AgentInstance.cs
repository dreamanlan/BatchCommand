using System;
using System.Collections.Concurrent;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Represents a single agent instance keyed by port number.
    /// Each instance holds its own state properties and WebSocketServer reference.
    /// </summary>
    public class AgentInstance
    {
        public int Port { get; }
        public WebSocketServer? WsServer { get; set; }

        // Agent state properties - readable/writable by DSL scripts
        public string ProjectDir { get; set; } = string.Empty;
        public string ProjectIdentity { get; set; } = string.Empty;
        public string SystemPrompt { get; set; } = string.Empty;
        public string ProjectPrompt { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string Emphasize { get; set; } = string.Empty;
        public string ToDo { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public string History { get; set; } = string.Empty;
        public string InjectJsCode { get; set; } = string.Empty;
        public string Soul { get; set; } = string.Empty;
        public int MaxResultSize { get; set; } = 0;

        // Whether to inject context info into MetaDSL results at specified rounds
        public bool ContextInjectionEnabled { get; set; } = false;
        // How often to append context info in WebSocket responses (0 = every round)
        public int MaxContextRounds { get; set; } = 3;
        // Current context round counter (atomic via Interlocked)
        private int _curContextRounds = 0;
        public int CurContextRounds
        {
            get => System.Threading.Interlocked.CompareExchange(ref _curContextRounds, 0, 0);
            set => System.Threading.Interlocked.Exchange(ref _curContextRounds, value);
        }

        /// <summary>
        /// Atomically increments CurContextRounds by 1, modulo MaxContextRounds.
        /// When MaxContextRounds is 0, always returns 0.
        /// Returns the new value after the operation.
        /// </summary>
        public int AddCurContextRounds()
        {
            int max = MaxContextRounds;
            if (max <= 0)
                return 0;
            int oldVal, newVal;
            do {
                oldVal = _curContextRounds;
                newVal = (oldVal + 1) % max;
            } while (System.Threading.Interlocked.CompareExchange(ref _curContextRounds, newVal, oldVal) != oldVal);
            return newVal;
        }

        public AgentInstance(int port)
        {
            Port = port;
        }
    }
}
