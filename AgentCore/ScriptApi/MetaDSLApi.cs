using System;
using ScriptableFramework;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    /// <summary>
    /// MetaDSL script APIs
    /// </summary>
    public static class MetaDSLApi
    {
        public static void RegisterApis()
        {
            // Register MetaDSL execution API
            BatchCommand.BatchScript.Register("exec_metadsl", "exec_metadsl(script)", new ExpressionFactoryHelper<ExecuteMetaDSLExp>());

            // Register hot reload API
            BatchCommand.BatchScript.Register("hot_reload", "hot_reload(component)", new ExpressionFactoryHelper<HotReloadExp>());

        }

        /// <summary>
        /// Execute MetaDSL script expression
        /// </summary>
        sealed class ExecuteMetaDSLExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count == 0)
                {
                    return "Error: script parameter required";
                }

                try
                {
                    string script = operands[0].GetString();
                    var executor = new MetaDSL.Executor();

                    // Get AgentCore instance from static access
                    var agentCore = Core.AgentCore.Instance;
                    if (agentCore == null)
                    {
                        return "Error: AgentCore not initialized";
                    }

                    string result = executor.Execute(script, agentCore);

                    agentCore.Logger.Info($"MetaDSL executed: {script}");

                    return result;
                }
                catch (Exception ex)
                {
                    Core.AgentCore.Instance?.Logger.Error($"Error executing MetaDSL: {ex.Message}");
                    return $"Error: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Hot reload expression
        /// </summary>
        sealed class HotReloadExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                string component = "all";

                if (operands.Count > 0)
                {
                    component = operands[0].GetString();
                }

                try
                {
                    var agentCore = Core.AgentCore.Instance;
                    if (agentCore == null)
                    {
                        return "Error: AgentCore not initialized";
                    }

                    agentCore.Logger.Info($"Hot reloading: {component}");

                    // Trigger hot reload through AgentCore
                    agentCore.TriggerHotReload(component);

                    return $"Hot reload triggered: {component}";
                }
                catch (Exception ex)
                {
                    Core.AgentCore.Instance?.Logger.Error($"Error hot reloading: {ex.Message}");
                    return $"Error: {ex.Message}";
                }
            }
        }
    }
}