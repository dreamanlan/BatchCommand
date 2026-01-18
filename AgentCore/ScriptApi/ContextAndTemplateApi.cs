using System;
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
            if (operands.Count < 2)
                return BoxedValue.From(false);

            try {
                string key = operands[0].AsString;
                object value = operands[1].GetObject();
                string scopeStr = operands.Count > 2 ? operands[2].AsString : "session";

                ContextScope scope = scopeStr.ToLower() == "workspace" ? ContextScope.Workspace : ContextScope.Session;
                bool result = Core.AgentCore.Instance.ContextManager.SetContextVariable(key, value, scope);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"SetContextVar error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Get context variable
    sealed class GetContextVarExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try {
                string key = operands[0].AsString;
                string scopeStr = operands.Count > 1 ? operands[1].AsString : "session";

                ContextScope scope = scopeStr.ToLower() == "workspace" ? ContextScope.Workspace : ContextScope.Session;
                object value = Core.AgentCore.Instance.ContextManager.GetContextVariable(key, scope);
                return value != null ? BoxedValue.FromObject(value) : BoxedValue.NullObject;
            }
            catch (Exception ex) {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"GetContextVar error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }
}
