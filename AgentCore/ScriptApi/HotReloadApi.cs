using System;

using ScriptableFramework;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using AgentCore.Core;

namespace AgentCore.ScriptApi
{
    /// <summary>
    /// Hot reload expression
    /// </summary>
    sealed class HotReloadExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                var agentCore = Core.AgentCore.Instance;
                if (agentCore == null) {
                    return "Error: AgentCore not initialized";
                }

                agentCore.Logger.Info($"Hot reloading");

                // Trigger hot reload through AgentCore
                agentCore.TriggerHotReload();

                return $"Hot reload triggered";
            }
            catch (Exception ex) {
                Core.AgentCore.Instance?.Logger.Error($"Error hot reloading: {ex.Message}");
                if (Core.AgentCore.IsInitialized) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"hot_reload error: {ex.Message}");
                }
                return $"Error: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Restart page expression.
    /// Asks the page to close the window, terminate the renderers and reopen
    /// with the current url: the same C++ flow as hot_reload, with no dll
    /// involved (hot_reload with component "restart", see main.js). This is the
    /// plain "the renderer has grown too big, give it a fresh process" path,
    /// handy for testing the memory guard without waiting for it to trigger.
    /// </summary>
    sealed class RestartPageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                var agentCore = Core.AgentCore.Instance;
                if (agentCore == null) {
                    return "Error: AgentCore not initialized";
                }

                agentCore.Logger.Info("Restarting the page");

                agentCore.TriggerRestartPage();

                return "Page restart triggered";
            }
            catch (Exception ex) {
                Core.AgentCore.Instance?.Logger.Error($"Error restarting the page: {ex.Message}");
                if (Core.AgentCore.IsInitialized) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"restart_page error: {ex.Message}");
                }
                return $"Error: {ex.Message}";
            }
        }
    }
}