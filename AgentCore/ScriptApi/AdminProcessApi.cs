using System;
using System.Collections.Generic;
using System.Diagnostics;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using BatchCommand;

namespace AgentCore.ScriptApi
{
    /// <summary>
    /// Launches a process elevated (UAC prompt, runas verb). Use for setup
    /// scripts that need admin rights (e.g. netsh http add sslcert for the
    /// webserver https listener).
    /// Usage: launch_process_with_admin(path [, args])
    /// Returns: true if the process was launched (not whether it succeeded -
    /// it runs asynchronously), false when the UAC prompt was cancelled or
    /// the launch failed. Windows only.
    /// </summary>
    sealed class LaunchProcessWithAdminExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: launch_process_with_admin(path [, args])");
                return BoxedValue.FromBool(false);
            }

            try {
                if (AgentCore.Core.MetaDslExecutor.IsMac) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("launch_process_with_admin is Windows only (runas verb)");
                    return BoxedValue.FromBool(false);
                }
                string path = operands[0].AsString ?? string.Empty;
                string args = operands.Count > 1 ? (operands[1].AsString ?? string.Empty) : string.Empty;
                if (string.IsNullOrEmpty(path)) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("launch_process_with_admin: path is empty");
                    return BoxedValue.FromBool(false);
                }
                var psi = new ProcessStartInfo {
                    FileName = path,
                    Arguments = args,
                    UseShellExecute = true,
                    Verb = "runas",
                };
                using (Process.Start(psi)) { }
                return BoxedValue.FromBool(true);
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("launch_process_with_admin: the UAC prompt was cancelled");
                return BoxedValue.FromBool(false);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"launch_process_with_admin error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    /// <summary>
    /// Registers the admin process launch API
    /// </summary>
    public static class AdminProcessApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("launch_process_with_admin", "launch_process_with_admin(path [, args]) - launch a process elevated (UAC prompt), Windows only, returns bool (launch status)",
                new ExpressionFactoryHelper<LaunchProcessWithAdminExp>());
        }
    }
}
