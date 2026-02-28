using System;
using AgentPlugin.Abstractions;
using ScriptableFramework;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
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
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"hot_reload error: {ex.Message}");
                }
                return $"Error: {ex.Message}";
            }
        }
    }
}