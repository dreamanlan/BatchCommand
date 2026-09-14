using System;
using System.Collections.Concurrent;
using BatchCommand.Utils;

namespace AgentCore.Core
{
    /// <summary>
    /// Represents a single agent instance keyed by agent id (string, e.g.
    /// "webagent" / "hyarena"). Port numbers are transport details of the
    /// websocket servers and are no longer instance keys (C-b, 2026-09-15);
    /// Port is kept only as an optional diagnostic association.
    /// </summary>
    public class AgentInstance
    {
        public string AgentId { get; }
        /// <summary>Optional ws port association (diagnostics only, 0 = none).</summary>
        public int Port { get; internal set; }
        public WebSocketServer? WsServer { get; set; }

        // Agent state properties - readable/writable by DSL scripts
        public string ProjectDir { get; set; } = string.Empty;
        public string ProjectIdentity { get; set; } = string.Empty;
        public string FoundationPrompt { get; set; } = string.Empty;
        public string ProjectPrompt { get; set; } = string.Empty;
        public string Emphasize { get; set; } = string.Empty;
        public string Soul { get; set; } = string.Empty;
        public string Backlog { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public string History { get; set; } = string.Empty;
        public string InjectJsCode { get; set; } = string.Empty;
        public int MaxResultSize { get; set; } = 0;

        // Per-instance DSL context variables (key/value store), independent from the global one
        public DslContextManagement DslContextManager { get; }

        // Whether to inject context info into MetaDSL results at specified rounds
        public bool ContextInjectionEnabled { get; set; } = false;
        // How often to append context info in WebSocket responses (0 = every round)
        public int MaxContextRounds { get; set; } = 6;
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

        public AgentInstance(string agentId)
        {
            AgentId = agentId;
            DslContextManager = new DslContextManagement();
        }
    }
}
