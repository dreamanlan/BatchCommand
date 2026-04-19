using System;
using System.Text;
using System.Text.RegularExpressions;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;
using CefDotnetApp.AgentCore.Utils;
using Dsl;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // Base class for process/script command expressions that use params/delimiter/template
    abstract class ProcessCommandExpBase : AbstractExpression
    {
        // Matches a closing double-quote not preceded by a backslash
        private static readonly Regex s_unescapedQuoteRegex = new Regex(@"(?<!\\)""", RegexOptions.Compiled);
        // Whether this expression needs extern script block {: ... :}
        protected virtual bool NeedExternScript => false;
        // Usage hint for error messages
        protected abstract string UsageHint { get; }

        protected override BoxedValue DoCalc()
        {
            var operands = new List<BoxedValue>();
            for (int i = 0; i < m_Expressions.Count; i++) {
                operands.Add(m_Expressions[i].Calc());
            }
            var bindingVals = new Dictionary<string, string>();
            for (int i = 0; i < m_BindingNames.Count; i++) {
                string bindingName = m_BindingNames[i];
                if (Calculator.TryGetVariable(bindingName, out var value) && !value.IsNullObject) {
                    bindingVals[bindingName] = value.ToString();
                }
                else {
                    bindingVals[bindingName] = string.Empty;
                }
            }
            return OnCalc(operands, bindingVals);
        }
        protected abstract BoxedValue OnCalc(IList<BoxedValue> operands, Dictionary<string, string> bindingVals);

        protected override bool Load(Dsl.FunctionData callData)
        {
            if (NeedExternScript) {
                callData = callData.ThisOrLowerOrderBody;
            }
            for (int i = 0; i < callData.GetParamNum(); ++i) {
                Dsl.ISyntaxComponent param = callData.GetParam(i);
                m_Expressions.Add(Calculator.Load(param));
            }
            return true;
        }
        protected override bool Load(StatementData statementData)
        {
            var first = statementData.First.AsFunction;
            if (first != null) {
                if (NeedExternScript)
                    Load(first.ThisOrLowerOrderCall);
                else
                    Load(first);
            }
            for (int i = 1; i < statementData.GetFunctionNum(); ++i) {
                var func = statementData.GetFunction(i).AsFunction;
                if (null != func) {
                    if (NeedExternScript)
                        func = func.ThisOrLowerOrderCall;
                    var id = func.GetId();
                    if (id == "bindings") {
                        LoadBindingNames(func);
                    }
                    else if (id == "delimiter" && func.GetParamNum() == 2) {
                        m_BeginChars = func.GetParamId(0);
                        m_EndChars = func.GetParamId(1);
                    }
                }
            }
            if (NeedExternScript) {
                var last = statementData.Last.AsFunction;
                if (last != null) {
                    if (last.HaveExternScript()) {
                        m_Script = last.GetParamId(0);
                    }
                    else {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: " + UsageHint);
                    }
                }
            }
            return true;
        }
        private void LoadBindingNames(Dsl.FunctionData callData)
        {
            for (int i = 0; i < callData.GetParamNum(); ++i) {
                string name = callData.GetParamId(i);
                m_BindingNames.Add(name);
            }
        }

        // Apply template substitution with params and skill envs
        protected string ApplyTemplate(string text, Dictionary<string, string> bindingVals)
        {
            var envs = Core.AgentCore.Instance.SkillMgr.Envs;
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            return TemplateCode.CalcBlockString(text, bindingVals, envs, sb1, sb2, m_BeginChars, m_EndChars);
        }

        // Build command and arguments for script execution based on file extension
        protected static (string command, string arguments) BuildScriptCommand(string ext, string file, string? cmdAndArgs)
        {
            string cmd = string.Empty;
            string args = string.Empty;
            if (ext == ".py") {
                cmd = "python";
                args = "-File " + file;
            }
            else if (ext == ".sh") {
                cmd = "sh";
                args = file;
            }
            else if (ext == ".ps1") {
                cmd = "powershell";
                args = "-File " + file;
            }
            else {
                cmd = "cmd";
                args = "/c " + file;
            }
            if (!string.IsNullOrEmpty(cmdAndArgs)) {
                string trimmedCmdAndArgs = cmdAndArgs.Trim();
                if (trimmedCmdAndArgs.Length > 0) {
                    if (trimmedCmdAndArgs[0] == '-' || trimmedCmdAndArgs[0] == '/') {
                        // Extra arguments only, prepend to existing args
                        args = trimmedCmdAndArgs + " " + args;
                    }
                    else if (trimmedCmdAndArgs[0] == '"') {
                        // Double-quoted command path, supports backslash-escaped quotes (\")
                        var quoteMatch = s_unescapedQuoteRegex.Match(trimmedCmdAndArgs, 1);
                        if (quoteMatch.Success) {
                            int cmdEnd = quoteMatch.Index;
                            cmd = trimmedCmdAndArgs.Substring(1, cmdEnd - 1).Replace("\\\"", "\"");
                            string extraArgs = (cmdEnd + 1 < trimmedCmdAndArgs.Length)
                                ? trimmedCmdAndArgs.Substring(cmdEnd + 1).TrimStart()
                                : string.Empty;
                            if (extraArgs.Length > 0)
                                args = extraArgs + " " + args;
                        }
                    }
                    else if (trimmedCmdAndArgs[0] == '\'') {
                        // Single-quoted command path, no escape support
                        int cmdEnd = trimmedCmdAndArgs.IndexOf('\'', 1);
                        if (cmdEnd > 0) {
                            cmd = trimmedCmdAndArgs.Substring(1, cmdEnd - 1);
                            string extraArgs = (cmdEnd + 1 < trimmedCmdAndArgs.Length)
                                ? trimmedCmdAndArgs.Substring(cmdEnd + 1).TrimStart()
                                : string.Empty;
                            if (extraArgs.Length > 0)
                                args = extraArgs + " " + args;
                        }
                    }
                    else {
                        // Unquoted command, split by first space
                        int cmdEnd = trimmedCmdAndArgs.IndexOf(' ');
                        if (cmdEnd > 0) {
                            cmd = trimmedCmdAndArgs.Substring(0, cmdEnd);
                            string extraArgs = trimmedCmdAndArgs.Substring(cmdEnd + 1).TrimStart();
                            if (extraArgs.Length > 0)
                                args = extraArgs + " " + args;
                        }
                        else {
                            cmd = trimmedCmdAndArgs;
                        }
                    }
                }
            }
            return (cmd, args);
        }

        // Trim leading and trailing blank lines from script content
        protected static string TrimScriptBlankLines(string script)
        {
            if (string.IsNullOrEmpty(script))
                return script;
            int start = 0;
            int len = script.Length;
            // Skip leading blank lines: scan forward line by line
            while (start < len) {
                int pos = start;
                while (pos < len && script[pos] != '\n' && script[pos] != '\r')
                    pos++;
                // Check if the line [start..pos) is all whitespace
                bool isBlank = true;
                for (int i = start; i < pos; i++) {
                    if (script[i] != ' ' && script[i] != '\t') {
                        isBlank = false;
                        break;
                    }
                }
                if (!isBlank) break;
                // Move past line ending (\r\n or \n or \r)
                if (pos < len && script[pos] == '\r') pos++;
                if (pos < len && script[pos] == '\n') pos++;
                start = pos;
            }
            // Skip trailing blank lines: scan backward line by line
            int end = len;
            while (end > start) {
                int pos = end;
                // Move back past line ending
                if (pos > start && script[pos - 1] == '\n') pos--;
                if (pos > start && script[pos - 1] == '\r') pos--;
                int lineEnd = pos;
                // Find the start of this line
                while (pos > start && script[pos - 1] != '\n')
                    pos--;
                // Check if the line [pos..lineEnd) is all whitespace
                bool isBlank = true;
                for (int i = pos; i < lineEnd; i++) {
                    if (script[i] != ' ' && script[i] != '\t') {
                        isBlank = false;
                        break;
                    }
                }
                if (!isBlank) break;
                end = pos;
            }
            if (start >= end)
                return string.Empty;
            if (start == 0 && end == len)
                return script;
            return script.Substring(start, end - start);
        }

        protected List<IExpression> m_Expressions = new List<IExpression>();
        protected List<string> m_BindingNames = new List<string>();
        protected string m_BeginChars = string.Empty;
        protected string m_EndChars = string.Empty;
        protected string m_Script = string.Empty;

        internal static Dictionary<string, string> s_Extensions = new Dictionary<string, string> {
            { "python", ".py" },
            { "bash", ".sh" },
            { "powershell", ".ps1" },
            { "bat", ".bat" },
            { "cmd", ".cmd" }
        };
    }

    // Execute script synchronously
    sealed class ExecuteScriptExp : ProcessCommandExpBase
    {
        protected override bool NeedExternScript => true;
        protected override string UsageHint => "execute_script([language, workingDir, timeout_def_30000ms, cmd_and_args])[bindings($a,$b,...)delimiter(begin_chars,end_chars)]{: script_code :};";

        protected override BoxedValue OnCalc(IList<BoxedValue> operands, Dictionary<string, string> bindingVals)
        {
            if (operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: " + UsageHint);
                return BoxedValue.NullObject;
            }

            try {
                string language = operands.Count > 0 ? operands[0].ToString().Trim().ToLower() : "python";
                string? workingDir = operands.Count > 1 ? operands[1].AsString : null;
                int timeout = operands.Count > 2 ? operands[2].GetInt() : 30000;
                string? cmdAndArgs = operands.Count > 3 ? operands[3].AsString : null;

                if (!s_Extensions.TryGetValue(language, out var ext)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("We only support Python, PowerShell, BAT, and Bash scripts. !");
                    return BoxedValue.NullObject;
                }

                var script = TrimScriptBlankLines(ApplyTemplate(m_Script, bindingVals));
                string file = ProcessOperations.GetUniqueRandomFilePath(ext);
                File.WriteAllText(file, script);

                var (command, arguments) = BuildScriptCommand(ext, file, cmdAndArgs);
                ProcessResult result;
                try {
                    result = Core.AgentCore.Instance.ProcessOps.ExecuteCommand(command, arguments, workingDir, timeout);
                }
                finally {
                    try { File.Delete(file); } catch { }
                }

                var dict = new Dictionary<string, object> {
                    ["success"] = result.Success,
                    ["exitCode"] = result.ExitCode,
                    ["output"] = result.Output,
                    ["error"] = result.Error,
                    ["executionTime"] = result.ExecutionTime.TotalMilliseconds
                };

                return BoxedValue.FromObject(dict);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"ExecuteCommand error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Execute script asynchronously with callback via command_callback CEF message
    sealed class ExecuteScriptCallbackExp : ProcessCommandExpBase
    {
        protected override bool NeedExternScript => true;
        protected override string UsageHint => "execute_script_callback(callbackMsg[, language, workingDir, timeout_def_30000ms, cmd_and_args])[bindings($a,$b,...)delimiter(begin_chars,end_chars)]{: script_code :};";

        protected override BoxedValue OnCalc(IList<BoxedValue> operands, Dictionary<string, string> bindingVals)
        {
            if (operands.Count < 1 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: " + UsageHint);
                return BoxedValue.FromString("error: invalid arguments");
            }

            try {
                string callbackMsg = operands[0].AsString;
                string language = operands.Count > 1 ? operands[1].ToString().Trim().ToLower() : "python";
                string? workingDir = operands.Count > 2 ? operands[2].AsString : null;
                int timeout = operands.Count > 3 ? operands[3].GetInt() : 30000;
                string? cmdAndArgs = operands.Count > 4 ? operands[4].AsString : null;

                if (!s_Extensions.TryGetValue(language, out var ext)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("We only support Python, PowerShell, BAT, and Bash scripts. !");
                    return BoxedValue.NullObject;
                }

                var script = TrimScriptBlankLines(ApplyTemplate(m_Script, bindingVals));
                string file = ProcessOperations.GetUniqueRandomFilePath(ext);
                File.WriteAllText(file, script);

                var (command, arguments) = BuildScriptCommand(ext, file, cmdAndArgs);
                Core.AgentCore.Instance.ProcessOps.ExecuteCommandWithCallback(command, arguments, workingDir, timeout, callbackMsg, file);
                return BoxedValue.FromString($"ok, async exec '{file}', result via command_callback");
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"ExecuteCommandAsync error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    // Execute command synchronously
    sealed class ExecuteCommandExp : ProcessCommandExpBase
    {
        protected override string UsageHint => "execute_command(command[, arguments, workingDir, timeout_def_30000ms])[bindings($a,$b,...)delimiter(begin_chars,end_chars)]";

        protected override BoxedValue OnCalc(IList<BoxedValue> operands, Dictionary<string, string> bindingVals)
        {
            if (operands.Count < 1 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: " + UsageHint);
                return BoxedValue.NullObject;
            }

            try {
                string command = operands[0].AsString;
                string? arguments = operands.Count > 1 ? operands[1].AsString : null;
                string? workingDir = operands.Count > 2 ? operands[2].AsString : null;
                int timeout = operands.Count > 3 ? operands[3].GetInt() : 30000;

                command = ApplyTemplate(command, bindingVals);
                if (arguments != null)
                    arguments = ApplyTemplate(arguments, bindingVals);

                var result = Core.AgentCore.Instance.ProcessOps.ExecuteCommand(command, arguments, workingDir, timeout);

                var dict = new Dictionary<string, object> {
                    ["success"] = result.Success,
                    ["exitCode"] = result.ExitCode,
                    ["output"] = result.Output,
                    ["error"] = result.Error,
                    ["executionTime"] = result.ExecutionTime.TotalMilliseconds
                };

                return BoxedValue.FromObject(dict);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"ExecuteCommand error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Execute command asynchronously with callback via command_callback CEF message
    sealed class ExecuteCommandCallbackExp : ProcessCommandExpBase
    {
        protected override string UsageHint => "execute_command_callback(callbackMsg, command[, arguments, workingDir, timeout_def_30000ms])[bindings($a,$b,...)delimiter(begin_chars,end_chars)]";

        protected override BoxedValue OnCalc(IList<BoxedValue> operands, Dictionary<string, string> bindingVals)
        {
            if (operands.Count < 2 || operands.Count > 5) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: " + UsageHint);
                return BoxedValue.FromString("error: invalid arguments");
            }

            try {
                string callbackMsg = operands[0].AsString;
                string command = operands[1].AsString;
                string? arguments = operands.Count > 2 ? operands[2].AsString : null;
                string? workingDir = operands.Count > 3 ? operands[3].AsString : null;
                int timeout = operands.Count > 4 ? operands[4].GetInt() : 30000;

                command = ApplyTemplate(command, bindingVals);
                if (arguments != null)
                    arguments = ApplyTemplate(arguments, bindingVals);

                Core.AgentCore.Instance.ProcessOps.ExecuteCommandWithCallback(command, arguments, workingDir, timeout, callbackMsg);
                return BoxedValue.FromString("ok, async exec, result via command_callback");
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"ExecuteCommandAsync error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    // Start a background process
    sealed class StartProcessExp : ProcessCommandExpBase
    {
        protected override string UsageHint => "start_process(processId, command[, arguments, workingDir])[params($a,$b,...)delimiter(begin_chars,end_chars)]";

        protected override BoxedValue OnCalc(IList<BoxedValue> operands, Dictionary<string, string> argVals)
        {
            if (operands.Count < 2 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: " + UsageHint);
                return BoxedValue.NullObject;
            }

            try {
                string processId = operands[0].AsString;
                string command = operands[1].AsString;
                string? arguments = operands.Count > 2 ? operands[2].AsString : null;
                string? workingDir = operands.Count > 3 ? operands[3].AsString : null;

                command = ApplyTemplate(command, argVals);
                if (arguments != null)
                    arguments = ApplyTemplate(arguments, argVals);

                string id = Core.AgentCore.Instance.ProcessOps.StartProcess(processId, command, arguments, workingDir);
                return BoxedValue.FromString(id);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"StartProcess error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Stop a background process
    sealed class StopProcessExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: stop_process(processId[, timeout_def_5000ms])");
                return BoxedValue.From(false);
            }

            try {
                string processId = operands[0].AsString;
                int timeout = operands.Count > 1 ? operands[1].GetInt() : 5000;

                bool result = Core.AgentCore.Instance.ProcessOps.StopProcess(processId, timeout);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"StopProcess error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Check if process is running
    sealed class IsProcessRunningExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: is_process_running(processId)");
                return BoxedValue.From(false);
            }

            try {
                string processId = operands[0].AsString;
                bool result = Core.AgentCore.Instance.ProcessOps.IsProcessRunning(processId);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"IsProcessRunning error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Write to process input
    sealed class WriteProcessInputExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: write_process_input(processId, input)");
                return BoxedValue.From(false);
            }

            try {
                string processId = operands[0].AsString;
                string input = operands[1].AsString;

                bool result = Core.AgentCore.Instance.ProcessOps.WriteProcessInput(processId, input);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"WriteProcessInput error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Read process output
    sealed class ReadProcessOutputExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: read_process_output(processId)");
                return BoxedValue.NullObject;
            }

            try {
                string processId = operands[0].AsString;
                string? output = Core.AgentCore.Instance.ProcessOps.ReadProcessOutput(processId);
                return output != null ? BoxedValue.FromString(output) : BoxedValue.NullObject;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"ReadProcessOutput error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Read process error
    sealed class ReadProcessErrorExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: read_process_error(processId)");
                return BoxedValue.NullObject;
            }

            try {
                string processId = operands[0].AsString;
                string? error = Core.AgentCore.Instance.ProcessOps.ReadProcessError(processId);
                return error != null ? BoxedValue.FromString(error) : BoxedValue.NullObject;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"ReadProcessError error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }
}
