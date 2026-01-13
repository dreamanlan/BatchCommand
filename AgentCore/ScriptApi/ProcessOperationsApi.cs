using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // Execute command synchronously
    sealed class ExecuteCommandExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string command = operands[0].AsString;
                string arguments = operands.Count > 1 ? operands[1].AsString : null;
                string workingDir = operands.Count > 2 ? operands[2].AsString : null;
                int timeout = operands.Count > 3 ? operands[3].GetInt() : 30000;

                var result = Core.AgentCore.Instance.ProcessOps.ExecuteCommand(command, arguments, workingDir, timeout);

                var dict = new Dictionary<string, object>
                {
                    ["success"] = result.Success,
                    ["exitCode"] = result.ExitCode,
                    ["output"] = result.Output,
                    ["error"] = result.Error,
                    ["executionTime"] = result.ExecutionTime.TotalMilliseconds
                };

                return BoxedValue.FromObject(dict);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"ExecuteCommand error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Start a background process
    sealed class StartProcessExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            try
            {
                string processId = operands[0].AsString;
                string command = operands[1].AsString;
                string arguments = operands.Count > 2 ? operands[2].AsString : null;
                string workingDir = operands.Count > 3 ? operands[3].AsString : null;

                string id = Core.AgentCore.Instance.ProcessOps.StartProcess(processId, command, arguments, workingDir);
                return BoxedValue.FromString(id);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"StartProcess error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Stop a background process
    sealed class StopProcessExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(false);

            try
            {
                string processId = operands[0].AsString;
                int timeout = operands.Count > 1 ? operands[1].GetInt() : 5000;

                bool result = Core.AgentCore.Instance.ProcessOps.StopProcess(processId, timeout);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"StopProcess error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Check if process is running
    sealed class IsProcessRunningExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(false);

            try
            {
                string processId = operands[0].AsString;
                bool result = Core.AgentCore.Instance.ProcessOps.IsProcessRunning(processId);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"IsProcessRunning error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Write to process input
    sealed class WriteProcessInputExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.From(false);

            try
            {
                string processId = operands[0].AsString;
                string input = operands[1].AsString;

                bool result = Core.AgentCore.Instance.ProcessOps.WriteProcessInput(processId, input);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"WriteProcessInput error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Read process output
    sealed class ReadProcessOutputExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string processId = operands[0].AsString;
                string output = Core.AgentCore.Instance.ProcessOps.ReadProcessOutput(processId);
                return output != null ? BoxedValue.FromString(output) : BoxedValue.NullObject;
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"ReadProcessOutput error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Read process error
    sealed class ReadProcessErrorExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string processId = operands[0].AsString;
                string error = Core.AgentCore.Instance.ProcessOps.ReadProcessError(processId);
                return error != null ? BoxedValue.FromString(error) : BoxedValue.NullObject;
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"ReadProcessError error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }
}
