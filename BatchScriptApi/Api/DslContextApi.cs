using System;
using System.Collections.Generic;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace BatchCommand.Api
{
    // Global context variable apis (moved from AgentCore ScriptApi/DslContextApi.cs).
    // They operate on the single global context store owned by the DslHost
    // (Utils.DslContextManagement). Per-instance variants (agent_set_context_var
    // etc.) stay in AgentCore, built on the same public DslContextManagement
    // class with a private store per AgentInstance.

    // set_context_var(key, value) - set a global context variable
    public sealed class SetContextVarExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                ApiErrorInfo.AppendLine("Expected: set_context_var(key, value)");
                return BoxedValue.From(false);
            }

            try {
                string key = operands[0].AsString;
                object value = operands[1].GetObject();

                bool result = DslHost.Current!.DslContextManager.SetContextVariable(key, value);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"SetContextVar error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // get_context_var(key) - get a global context variable
    public sealed class GetContextVarExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: get_context_var(key)");
                return BoxedValue.NullObject;
            }

            try {
                string key = operands[0].AsString;

                object? value = DslHost.Current!.DslContextManager.GetContextVariable(key);
                return value != null ? BoxedValue.FromObject(value) : BoxedValue.NullObject;
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"GetContextVar error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // remove_context_var(key) - remove a global context variable
    public sealed class RemoveContextVarExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: remove_context_var(key)");
                return BoxedValue.FromBool(false);
            }

            try {
                string key = operands[0].AsString;

                bool r = DslHost.Current!.DslContextManager.RemoveContextVariable(key);
                return r;
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"RemoveContextVar error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    // clear_context_vars() - clear all global context variables
    public sealed class ClearContextVarsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count > 0) {
                ApiErrorInfo.AppendLine("Expected: clear_context_vars()");
                return BoxedValue.FromBool(false);
            }

            try {
                DslHost.Current!.DslContextManager.ClearVariables();
                return true;
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"ClearContextVars error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }
}
