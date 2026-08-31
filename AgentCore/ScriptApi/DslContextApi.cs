using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // ========== Global Context Variable APIs ==========
    // Operate on the single global context store. No scope parameter:
    // there is exactly one set of key/value pairs.

    // Set context variable
    sealed class SetContextVarExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: set_context_var(key, value)");
                return BoxedValue.From(false);
            }

            try {
                string key = operands[0].AsString;
                object value = operands[1].GetObject();

                bool result = Core.AgentCore.Instance.DslContextManager.SetContextVariable(key, value);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"SetContextVar error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Get context variable
    sealed class GetContextVarExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_context_var(key)");
                return BoxedValue.NullObject;
            }

            try {
                string key = operands[0].AsString;

                object? value = Core.AgentCore.Instance.DslContextManager.GetContextVariable(key);
                return value != null ? BoxedValue.FromObject(value) : BoxedValue.NullObject;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"GetContextVar error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Remove context variable
    sealed class RemoveContextVarExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: remove_context_var(key)");
                return BoxedValue.FromBool(false);
            }

            try {
                string key = operands[0].AsString;

                bool r = Core.AgentCore.Instance.DslContextManager.RemoveContextVariable(key);
                return r;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"RemoveContextVar error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    // Clear context variables
    sealed class ClearContextVarsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count > 0) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: clear_context_vars()");
                return BoxedValue.FromBool(false);
            }

            try {
                Core.AgentCore.Instance.DslContextManager.ClearVariables();
                return true;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"ClearContextVars error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    // ========== Agent Instance Context Variable APIs ==========
    // Same implementation class, but the store lives on the AgentInstance
    // identified by port, which is always the first parameter. Each instance
    // has its own single set of key/value pairs.

    // Set context variable on an agent instance
    sealed class AgentSetContextVarExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: agent_set_context_var(port, key, value)");
                return BoxedValue.From(false);
            }

            try {
                var inst = Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
                string key = operands[1].AsString;
                object value = operands[2].GetObject();

                bool result = inst.DslContextManager.SetContextVariable(key, value);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"AgentSetContextVar error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: agent_get_context_var(port, key)");
                return BoxedValue.NullObject;
            }

            try {
                var inst = Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
                string key = operands[1].AsString;

                object? value = inst.DslContextManager.GetContextVariable(key);
                return value != null ? BoxedValue.FromObject(value) : BoxedValue.NullObject;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"AgentGetContextVar error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: agent_remove_context_var(port, key)");
                return BoxedValue.FromBool(false);
            }

            try {
                var inst = Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
                string key = operands[1].AsString;

                bool r = inst.DslContextManager.RemoveContextVariable(key);
                return r;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"AgentRemoveContextVar error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: agent_clear_context_vars(port)");
                return BoxedValue.FromBool(false);
            }

            try {
                var inst = Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
                inst.DslContextManager.ClearVariables();
                return true;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"AgentClearContextVars error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

}
