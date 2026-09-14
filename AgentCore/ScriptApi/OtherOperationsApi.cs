using System;

using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using AgentCore.Core;
using AgentCore.Models;
using BatchCommand.Utils;
using System.Text;
using System.Collections;
using J2N;
using J2N.Collections.Generic.Extensions;

namespace AgentCore.ScriptApi
{
    // Clipboard apis moved to BatchCommand.Api (OtherOperationsApi.cs, TextCopy
    // based ClipboardOperations lives in BatchCommand.Utils).

    // Logging Operations
    sealed class LogInfoExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: log_info(fmt, ...)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string fmt = operands[0].AsString;
                    var args = new object[operands.Count - 1];
                    for (int i = 1; i < operands.Count; i++) {
                        args[i - 1] = operands[i].GetObject();
                    }
                    Core.AgentCore.Instance.Logger.Info(fmt, args);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"loginfo error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class LogErrorExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: log_error(fmt, ...)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string fmt = operands[0].AsString;
                    var args = new object[operands.Count - 1];
                    for (int i = 1; i < operands.Count; i++) {
                        args[i - 1] = operands[i].GetObject();
                    }
                    Core.AgentCore.Instance.Logger.Error(fmt, args);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"logerror error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class LogWarningExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: log_warning(fmt, ...)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string fmt = operands[0].AsString;
                    var args = new object[operands.Count - 1];
                    for (int i = 1; i < operands.Count; i++) {
                        args[i - 1] = operands[i].GetObject();
                    }
                    Core.AgentCore.Instance.Logger.Warning(fmt, args);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"logwarning error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }
}
