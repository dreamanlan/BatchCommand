using System;
using System.Collections.Generic;
using ScriptableFramework;

namespace BatchCommand.Utils
{
    /// <summary>
    /// Host integration hooks for the shared async services (http / process
    /// operations). Hosts wire these once at startup; the shared services run
    /// host-agnostically and route every host-specific concern through here:
    ///
    ///   Log                    - diagnostics routing.
    ///   CaptureCallbackContext - opaque callback context of the CALLING thread
    ///                            (AgentCore: MetaDslExecutor.CurrentContext, a
    ///                            DslContext). null = no request context: the
    ///                            async callback is dropped.
    ///   DeliverCallback        - (msgName, args, context) delivered on the host
    ///                            main thread (AgentCore: EnqueueCallback ->
    ///                            handle_&lt;msg&gt; dsl / agent_callback push).
    ///   GetTemplateEnvs        - template variable envs for script execution
    ///                            (AgentCore: SkillMgr.Envs).
    /// </summary>
    public static class HostBridge
    {
        public static Action<string>? Log { get; set; }
        public static Func<object?>? CaptureCallbackContext { get; set; }
        public static Action<string, BoxedValue[], object?>? DeliverCallback { get; set; }
        public static Func<Dictionary<string, string>?>? GetTemplateEnvs { get; set; }
    }
}
