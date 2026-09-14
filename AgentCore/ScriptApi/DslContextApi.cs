using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using AgentCore.Core;
using static AgentCore.ScriptApi.AgentIdResolver;

namespace AgentCore.ScriptApi
{
    // ========== Global Context Variable APIs ==========
    // set_context_var/get_context_var/remove_context_var/clear_context_vars
    // moved to BatchCommand.Api (Api/DslContextApi.cs): they operate on the
    // global store owned by DslHost, shared by all hosts.

    // ========== Agent Instance Context Variable APIs ==========
    // Same implementation class (BatchCommand.Utils.DslContextManagement), but
    // the store lives on the AgentInstance identified by agent id, which is
    // always the first parameter. Each instance has its own single set of
    // key/value pairs.

    // Set context variable on an agent instance
    sealed class AgentSetContextVarExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: agent_set_context_var(agentId, key, value)");
                return BoxedValue.From(false);
            }

            try {
                var inst = Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
                string key = operands[1].AsString;
                object value = operands[2].GetObject();

                bool result = inst.DslContextManager.SetContextVariable(key, value);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"AgentSetContextVar error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Get context variable from an agent instance
    sealed class AgentGetContextVarExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: agent_get_context_var(agentId, key)");
                return BoxedValue.NullObject;
            }

            try {
                var inst = Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
                string key = operands[1].AsString;

                object? value = inst.DslContextManager.GetContextVariable(key);
                return value != null ? BoxedValue.FromObject(value) : BoxedValue.NullObject;
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"AgentGetContextVar error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Remove context variable from an agent instance
    sealed class AgentRemoveContextVarExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: agent_remove_context_var(agentId, key)");
                return BoxedValue.FromBool(false);
            }

            try {
                var inst = Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
                string key = operands[1].AsString;

                bool r = inst.DslContextManager.RemoveContextVariable(key);
                return r;
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"AgentRemoveContextVar error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    // Clear context variables on an agent instance
    sealed class AgentClearContextVarsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: agent_clear_context_vars(agentId)");
                return BoxedValue.FromBool(false);
            }

            try {
                var inst = Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
                inst.DslContextManager.ClearVariables();
                return true;
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"AgentClearContextVars error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

}
