using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // ========== Context Management APIs ==========

    // Set context variable
    sealed class SetContextVarExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: set_context_var(key, value[, scope])");
                return BoxedValue.From(false);
            }

            try {
                string key = operands[0].AsString;
                object value = operands[1].GetObject();
                string scopeStr = operands.Count > 2 ? operands[2].AsString : "session";

                ContextScope scope = scopeStr.ToLower() == "workspace" ? ContextScope.Workspace : ContextScope.Session;
                bool result = Core.AgentCore.Instance.DslContextManager.SetContextVariable(key, value, scope);
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
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_context_var(key[, scope])");
                return BoxedValue.NullObject;
            }

            try {
                string key = operands[0].AsString;
                string scopeStr = operands.Count > 1 ? operands[1].AsString : "session";

                ContextScope scope = scopeStr.ToLower() == "workspace" ? ContextScope.Workspace : ContextScope.Session;
                object? value = Core.AgentCore.Instance.DslContextManager.GetContextVariable(key, scope);
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
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: remove_context_var(key[, scope])");
                return BoxedValue.FromBool(false);
            }

            try {
                string key = operands[0].AsString;
                string scopeStr = operands.Count > 1 ? operands[1].AsString : "session";

                ContextScope scope = scopeStr.ToLower() == "workspace" ? ContextScope.Workspace : ContextScope.Session;
                bool r = Core.AgentCore.Instance.DslContextManager.RemoveContextVariable(key, scope);
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
            if (operands.Count > 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: clear_context_vars([scope])");
                return BoxedValue.FromBool(false);
            }

            try {
                string scopeStr = operands.Count > 0 ? operands[0].AsString : "session";

                ContextScope scope = scopeStr.ToLower() == "workspace" ? ContextScope.Workspace : ContextScope.Session;
                if (scope == ContextScope.Session) {
                    Core.AgentCore.Instance.DslContextManager.ClearSessionVariables();
                    return true;
                }
                else if (scope == ContextScope.Workspace) {
                    Core.AgentCore.Instance.DslContextManager.ClearWorkspaceVariables();
                    return true;
                }
                return false;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"RemoveContextVar error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    // Clear all context variables
    sealed class ClearAllContextVarsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                Core.AgentCore.Instance.DslContextManager.ClearSessionVariables();
                Core.AgentCore.Instance.DslContextManager.ClearWorkspaceVariables();
                return true;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"RemoveContextVar error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }
}
