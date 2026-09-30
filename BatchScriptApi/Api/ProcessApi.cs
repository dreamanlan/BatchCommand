using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using BatchCommand.Utils;
using Dsl;

namespace BatchCommand.Api
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
                m_Script = callData.GetParamId(0);
                callData = callData.ThisOrLowerOrderCall;
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
                        ApiErrorInfo.AppendLine("Expected: " + UsageHint);
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
            var envs = HostBridge.GetTemplateEnvs?.Invoke() ?? new System.Collections.Generic.Dictionary<string, string>();
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            return TemplateCode.CalcBlockString(text, bindingVals, envs, sb1, sb2, m_BeginChars, m_EndChars);
        }

        // Build command and arguments for script execution based on file extension
        protected static (string command, string arguments) BuildScriptCommand(string ext, string file, string? cmdAndArgs)
        {
            string cmd = string.Empty;
            string args = string.Empty;
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (ext == ".py") {
                cmd = isWindows ? "python" : "python3";
                args = file;
            }
            else if (ext == ".sh") {
                cmd = "bash";
                args = file;
            }
            else if (ext == ".zsh") {
                cmd = "zsh";
                args = file;
            }
            else if (ext == ".js") {
                cmd = "node";
                args = file;
            }
            else if (ext == ".pl") {
                cmd = "perl";
                args = file;
            }
            else if (ext == ".rb") {
                cmd = "ruby";
                args = file;
            }
            else if (ext == ".ps1") {
                cmd = isWindows ? "powershell" : "pwsh";
                args = "-File " + file;
            }
            else if (ext == ".bat" || ext == ".cmd") {
                if (!isWindows)
                    throw new PlatformNotSupportedException("BAT and CMD scripts are only supported on Windows.");
                cmd = "cmd";
                args = "/c " + file;
            }
            else {
                throw new NotSupportedException("Unsupported script extension: " + ext);
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
            { "zsh", ".zsh" },
            { "nodejs", ".js" },
            { "node", ".js" },
            { "perl", ".pl" },
            { "ruby", ".rb" },
            { "powershell", ".ps1" },
            { "bat", ".bat" },
            { "cmd", ".cmd" }
        };
    }

    // Execute script synchronously
    sealed class ExecuteScriptExp : ProcessCommandExpBase
    {
        protected override bool NeedExternScript => true;
        protected override string UsageHint => "execute_script([language, workingDir, timeout_def_30000ms, cmd_and_args_str])[bindings($a,$b,...)delimiter(begin_template_code_chars,end_template_code_chars)]{: script_code :};";

        protected override BoxedValue OnCalc(IList<BoxedValue> operands, Dictionary<string, string> bindingVals)
        {
            if (operands.Count > 4) {
                ApiErrorInfo.AppendLine("Expected: " + UsageHint);
                return BoxedValue.NullObject;
            }

            try {
                string language = operands.Count > 0 ? operands[0].ToString().Trim().ToLower() : "python";
                string? workingDir = operands.Count > 1 ? operands[1].AsString : null;
                int timeout = operands.Count > 2 ? operands[2].GetInt() : 30000;
                string? cmdAndArgs = operands.Count > 3 ? operands[3].AsString : null;

                if (!s_Extensions.TryGetValue(language, out var ext)) {
                    ApiErrorInfo.AppendLine("We only support Python, NodeJS, Perl, Ruby, PowerShell, BAT, Bash, and Zsh scripts. !");
                    return BoxedValue.NullObject;
                }

                var rawScript = m_Script ?? string.Empty;
                var templatedScript = ApplyTemplate(rawScript, bindingVals);
                var script = TrimScriptBlankLines(templatedScript);
                string file = ProcessOperations.GetUniqueRandomFilePath(ext);
                try {
                    var operandText = new StringBuilder();
                    for (int i = 0; i < operands.Count; ++i) {
                        if (i > 0)
                            operandText.Append(" | ");
                        operandText.Append(operands[i].ToString());
                    }
                    string preview = script.Replace("\r", "\\r").Replace("\n", "\\n");
                    if (preview.Length > 500)
                        preview = preview.Substring(0, 500);
                    File.AppendAllText("E:/tmp/execute_script_diagnostic.log",
                        $"before_write raw_length={rawScript.Length} template_length={templatedScript.Length} script_length={script.Length} operands_count={operands.Count} operands={operandText} file={file} preview={preview}{Environment.NewLine}");
                }
                catch {
                }
                File.WriteAllText(file, script);
                try {
                    File.AppendAllText("E:/tmp/execute_script_diagnostic.log",
                        $"after_write file_length={new FileInfo(file).Length}{Environment.NewLine}");
                }
                catch {
                }

                var (command, arguments) = BuildScriptCommand(ext, file, cmdAndArgs);
                ProcessResult result;
                try {
                    result = ProcessOperations.Shared.ExecuteCommand(command, arguments, workingDir, timeout);
                }
                finally {
                    try { File.Delete(file); } catch { }
                }

                var dict = new Dictionary<string, object> {
                    ["success"] = result.Success,
                    ["exitCode"] = result.ExitCode,
                    ["output"] = result.Output,
                    ["error"] = result.Error,
                    ["executionTime"] = result.ExecutionTime.TotalMilliseconds,
                    ["diagnosticRawLength"] = rawScript.Length,
                    ["diagnosticTemplateLength"] = templatedScript.Length,
                    ["diagnosticScriptLength"] = script.Length,
                    ["diagnosticScriptPreview"] = script.Length > 500 ? script.Substring(0, 500) : script,
                    ["diagnosticTempFile"] = file,
                    ["diagnosticBuildMarker"] = $"{typeof(ExecuteScriptExp).Assembly.FullName}|{typeof(ExecuteScriptExp).Assembly.Location}"
                };

                return BoxedValue.FromObject(dict);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"ExecuteCommand error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Execute script asynchronously with callback via command_callback CEF message
    sealed class ExecuteScriptCallbackExp : ProcessCommandExpBase
    {
        protected override bool NeedExternScript => true;
        protected override string UsageHint => "execute_script_callback('command_callback'[, language, workingDir, timeout_def_30000ms, cmd_and_args_str])[bindings($a,$b,...)delimiter(begin_template_code_chars,end_template_code_chars)]{: script_code :};";

        protected override BoxedValue OnCalc(IList<BoxedValue> operands, Dictionary<string, string> bindingVals)
        {
            if (operands.Count < 1 || operands.Count > 5) {
                ApiErrorInfo.AppendLine("Expected: " + UsageHint);
                return BoxedValue.FromString("error: invalid arguments");
            }

            try {
                string callbackMsg = operands[0].AsString;
                string language = operands.Count > 1 ? operands[1].ToString().Trim().ToLower() : "python";
                string? workingDir = operands.Count > 2 ? operands[2].AsString : null;
                int timeout = operands.Count > 3 ? operands[3].GetInt() : 30000;
                string? cmdAndArgs = operands.Count > 4 ? operands[4].AsString : null;

                if (!s_Extensions.TryGetValue(language, out var ext)) {
                    ApiErrorInfo.AppendLine("We only support Python, NodeJS, Perl, Ruby, PowerShell, BAT, Bash, and Zsh scripts. !");
                    return BoxedValue.NullObject;
                }

                var script = TrimScriptBlankLines(ApplyTemplate(m_Script, bindingVals));
                string file = ProcessOperations.GetUniqueRandomFilePath(ext);
                File.WriteAllText(file, script);

                var (command, arguments) = BuildScriptCommand(ext, file, cmdAndArgs);
                ProcessOperations.Shared.ExecuteCommandWithCallback(command, arguments, workingDir, timeout, callbackMsg, file);
                return BoxedValue.FromString($"ok, async exec '{file}', result via command_callback");
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"ExecuteCommandAsync error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    // Execute command synchronously
    sealed class ExecuteCommandExp : ProcessCommandExpBase
    {
        protected override string UsageHint => "execute_command(command[, args_str, workingDir, timeout_def_30000ms])[bindings($a,$b,...)delimiter(begin_chars,end_chars)]";

        protected override BoxedValue OnCalc(IList<BoxedValue> operands, Dictionary<string, string> bindingVals)
        {
            if (operands.Count < 1 || operands.Count > 4) {
                ApiErrorInfo.AppendLine("Expected: " + UsageHint);
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

                var result = ProcessOperations.Shared.ExecuteCommand(command, arguments, workingDir, timeout);

                var dict = new Dictionary<string, object> {
                    ["success"] = result.Success,
                    ["exitCode"] = result.ExitCode,
                    ["output"] = result.Output,
                    ["error"] = result.Error,
                    ["executionTime"] = result.ExecutionTime.TotalMilliseconds,
                    ["diagnosticBuildMarker"] = $"{typeof(ExecuteScriptExp).Assembly.FullName}|{typeof(ExecuteScriptExp).Assembly.Location}"
                };

                return BoxedValue.FromObject(dict);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"ExecuteCommand error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Execute command asynchronously with callback via command_callback CEF message
    sealed class ExecuteCommandCallbackExp : ProcessCommandExpBase
    {
        protected override string UsageHint => "execute_command_callback('command_callback', command[, args_str, workingDir, timeout_def_30000ms])[bindings($a,$b,...)delimiter(begin_chars,end_chars)]";

        protected override BoxedValue OnCalc(IList<BoxedValue> operands, Dictionary<string, string> bindingVals)
        {
            if (operands.Count < 2 || operands.Count > 5) {
                ApiErrorInfo.AppendLine("Expected: " + UsageHint);
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

                ProcessOperations.Shared.ExecuteCommandWithCallback(command, arguments, workingDir, timeout, callbackMsg);
                return BoxedValue.FromString("ok, async exec, result via command_callback");
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"ExecuteCommandAsync error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    // Start a background process
    sealed class StartProcessExp : ProcessCommandExpBase
    {
        protected override string UsageHint => "start_process(processId, command[, args_str, workingDir])[params($a,$b,...)delimiter(begin_chars,end_chars)]";

        protected override BoxedValue OnCalc(IList<BoxedValue> operands, Dictionary<string, string> argVals)
        {
            if (operands.Count < 2 || operands.Count > 4) {
                ApiErrorInfo.AppendLine("Expected: " + UsageHint);
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

                string id = ProcessOperations.Shared.StartProcess(processId, command, arguments, workingDir);
                return BoxedValue.FromString(id);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"StartProcess error: {ex.Message}");
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
                ApiErrorInfo.AppendLine("Expected: stop_process(processId[, timeout_def_5000ms])");
                return BoxedValue.From(false);
            }

            try {
                string processId = operands[0].AsString;
                int timeout = operands.Count > 1 ? operands[1].GetInt() : 5000;

                bool result = ProcessOperations.Shared.StopProcess(processId, timeout);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"StopProcess error: {ex.Message}");
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
                ApiErrorInfo.AppendLine("Expected: is_process_running(processId)");
                return BoxedValue.From(false);
            }

            try {
                string processId = operands[0].AsString;
                bool result = ProcessOperations.Shared.IsProcessRunning(processId);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"IsProcessRunning error: {ex.Message}");
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
                ApiErrorInfo.AppendLine("Expected: write_process_input(processId, input)");
                return BoxedValue.From(false);
            }

            try {
                string processId = operands[0].AsString;
                string input = operands[1].AsString;

                bool result = ProcessOperations.Shared.WriteProcessInput(processId, input);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"WriteProcessInput error: {ex.Message}");
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
                ApiErrorInfo.AppendLine("Expected: read_process_output(processId)");
                return BoxedValue.NullObject;
            }

            try {
                string processId = operands[0].AsString;
                string? output = ProcessOperations.Shared.ReadProcessOutput(processId);
                return output != null ? BoxedValue.FromString(output) : BoxedValue.NullObject;
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"ReadProcessOutput error: {ex.Message}");
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
                ApiErrorInfo.AppendLine("Expected: read_process_error(processId)");
                return BoxedValue.NullObject;
            }

            try {
                string processId = operands[0].AsString;
                string? error = ProcessOperations.Shared.ReadProcessError(processId);
                return error != null ? BoxedValue.FromString(error) : BoxedValue.NullObject;
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"ReadProcessError error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Get active callback command status
    sealed class GetCommandStatusExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                return BoxedValue.FromString(ProcessOperations.Shared.GetActiveCommandStatus());
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"GetCommandStatus error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }
    // Memory footprint of a process, in MB: the current process (no argument),
    // a pid, or the largest process carrying a given name.
    //
    // This is the number a memory guard has to look at. The JS heap alone is
    // misleading: on a long lived page the heap reported by performance.memory
    // was ~1.3GB while the renderer process held ~6GB, the difference being
    // Blink objects and allocator pages that are never returned. The renderer
    // process runs managed code too (see the per-process dsl script selection),
    // so calling this with no argument reports the very process that is growing.
    sealed class GetProcessMemoryExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count > 1) {
                ApiErrorInfo.AppendLine("Expected: get_process_memory([name_or_pid])");
                return BoxedValue.From(0.0);
            }
            try {
                long bytes;
                if (operands.Count == 1 && operands[0].IsInteger) {
                    bytes = ProcessOperations.Shared.GetProcessMemoryBytes(operands[0].GetInt());
                }
                else if (operands.Count == 1) {
                    bytes = ProcessOperations.Shared.GetProcessMemoryBytes(0, operands[0].AsString);
                }
                else {
                    bytes = ProcessOperations.Shared.GetProcessMemoryBytes();
                }
                return BoxedValue.From(bytes / (1024.0 * 1024.0));
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"get_process_memory error: {ex.Message}");
                return BoxedValue.From(0.0);
            }
        }
    }

    /// <summary>
    /// Process / script execution api set (moved from AgentCore; service core
    /// in BatchCommand.Utils.ProcessOperations). Async variants deliver via
    /// HostBridge callbacks (command_callback / handle_command_callback).
    /// </summary>
    public static class ProcessApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("execute_script", "execute_script([language, workingDir, timeout_def_30000ms, cmd_and_args_str])[bindings($a,$b,...)delimiter(begin_template_code_chars,end_template_code_chars)]{: script_code :}; return Object(success/exitCode/output/error/executionTime), use 'to_string' to convert to a string. The default template code delimiters are \"{%\" and \"%}\"; specifically, {% %} serve as the template variable brackets.", new ExpressionFactoryHelper<ExecuteScriptExp>());
            BatchCommand.BatchScript.Register("execute_script_callback", "execute_script_callback('command_callback'[, language, workingDir, timeout_def_30000ms, cmd_and_args_str])[bindings($a,$b,...)delimiter(begin_template_code_chars,end_template_code_chars)]{: script_code :}; - async exec, result via command_callback. The default template code delimiters are \"{%\" and \"%}\"; specifically, {% %} serve as the template variable brackets.", new ExpressionFactoryHelper<ExecuteScriptCallbackExp>());
            BatchCommand.BatchScript.Register("execute_command", "execute_command(command[, args_str, workingDir, timeout_def_30000ms])[bindings($a,$b,...)delimiter(begin_chars,end_chars)] return Object(success/exitCode/output/error/executionTime), use 'to_string' to convert to a string", new ExpressionFactoryHelper<ExecuteCommandExp>());
            BatchCommand.BatchScript.Register("execute_command_callback", "execute_command_callback('command_callback', command[, args_str, workingDir, timeout_def_30000ms])[bindings($a,$b,...)delimiter(begin_chars,end_chars)] - async exec, result via command_callback", new ExpressionFactoryHelper<ExecuteCommandCallbackExp>());
            BatchCommand.BatchScript.Register("start_process", "start_process(processId, command[, args_str, workingDir])[params($a,$b,...)delimiter(begin_chars,end_chars)]", new ExpressionFactoryHelper<StartProcessExp>());
            BatchCommand.BatchScript.Register("stop_process", "stop_process(processId[, timeout_def_5000ms]) - gracefully stop a child process started via start_process/execute_script", new ExpressionFactoryHelper<StopProcessExp>());
            BatchCommand.BatchScript.Register("is_process_running", "is_process_running(processId)", new ExpressionFactoryHelper<IsProcessRunningExp>());
            BatchCommand.BatchScript.Register("write_process_input", "write_process_input(processId, input)", new ExpressionFactoryHelper<WriteProcessInputExp>());
            BatchCommand.BatchScript.Register("read_process_output", "read_process_output(processId)", new ExpressionFactoryHelper<ReadProcessOutputExp>());
            BatchCommand.BatchScript.Register("read_process_error", "read_process_error(processId)", new ExpressionFactoryHelper<ReadProcessErrorExp>());
            BatchCommand.BatchScript.Register("get_command_status", "get_command_status() - returns status of all active callback commands (id, duration, command)", new ExpressionFactoryHelper<GetCommandStatusExp>());
            BatchCommand.BatchScript.Register("get_process_memory", "get_process_memory([name_or_pid]) - memory footprint in MB: the current process when called without arguments, a pid, or the largest process with that name (with or without .exe); returns 0 when no process matches. Private bytes on Windows, working set elsewhere.", new ExpressionFactoryHelper<GetProcessMemoryExp>());
            // OS-level process management (merged from CefDotnetApp; one set for
            // every host). launch_process spawns and returns the OS pid;
            // search_process searches by name and / or command line and returns
            // the matching pids; kill_process kills by name or pid (vs
            // stop_process, which stops a spawned child gracefully).
            BatchCommand.BatchScript.Register("launch_process", "launch_process(exe[, args, working_dir]) - start an OS process, returns its pid (0 on error)", new ExpressionFactoryHelper<LaunchProcessExp>());
            BatchCommand.BatchScript.Register("get_process_parent_id", "get_process_parent_id([pid]) - parent process id of the given process (default: the current process), 0 when it cannot be determined; on Windows the value is captured at process creation and keeps pointing at the parent even after it died, which is what a parent watchdog needs (pair it with is_process_alive)", new ExpressionFactoryHelper<GetProcessParentIdExp>());
            BatchCommand.BatchScript.Register("is_process_alive", "is_process_alive(pid[, name_key]) - whether the process exists and, when name_key is given, whether its name contains the key (name matching does not rule out pid reuse by a process with a matching name), returns bool", new ExpressionFactoryHelper<IsProcessAliveExp>());
            BatchCommand.BatchScript.Register("search_process", "search_process([name_key[, cmd_line_key]]) - search running processes: name_key matches the process name and cmd_line_key the command line (both case insensitive substrings, AND-ed, an empty key skips that check, the .exe suffix of the name key is optional); returns the list of matching pids (empty list when nothing matches), use listsize() to count", new ExpressionFactoryHelper<SearchProcessExp>());
            BatchCommand.BatchScript.Register("kill_process", "kill_process(name_or_pid) - kill OS processes by name (with or without .exe) or by pid, returns the killed count", new ExpressionFactoryHelper<KillProcessExp>());
        }
    }

    // ------------------------------------------------------------------------
    // OS-level process management (merged from CefDotnetApp: one api set for
    // every host). Logging goes through HostBridge.Log.
    // ------------------------------------------------------------------------

    // Launch an OS process, returns its pid (0 on error).
    sealed class LaunchProcessExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3) {
                BatchCommand.Utils.HostBridge.Log?.Invoke("Expected: launch_process(exe[, args, working_dir])");
                return BoxedValue.From(0);
            }
            try {
                string exe = operands[0].AsString;
                string args = operands.Count > 1 ? operands[1].AsString : string.Empty;
                string workingDir = operands.Count > 2 ? operands[2].AsString : string.Empty;
                if (string.IsNullOrEmpty(exe)) {
                    BatchCommand.Utils.HostBridge.Log?.Invoke("launch_process: empty exe path");
                    return BoxedValue.From(0);
                }
                var psi = new System.Diagnostics.ProcessStartInfo {
                    FileName = exe,
                    Arguments = args,
                    UseShellExecute = false,
                };
                if (!string.IsNullOrEmpty(workingDir)) {
                    psi.WorkingDirectory = workingDir;
                }
                using var proc = System.Diagnostics.Process.Start(psi);
                if (null == proc) {
                    BatchCommand.Utils.HostBridge.Log?.Invoke("launch_process: Process.Start returned null: " + exe);
                    return BoxedValue.From(0);
                }
                BatchCommand.Utils.HostBridge.Log?.Invoke("launch_process: " + exe + " " + args + " -> pid " + proc.Id);
                return BoxedValue.From(proc.Id);
            }
            catch (Exception ex) {
                BatchCommand.Utils.HostBridge.Log?.Invoke("launch_process failed: " + ex.Message);
                return BoxedValue.From(0);
            }
        }
    }

    // The parent pid of a process. There is no managed API for it, so this is
    // per platform:
    //   - Windows: NtQueryInformationProcess -> InheritedFromUniqueProcessId.
    //     The value is captured when the process is created and is NOT updated
    //     when the parent dies, which is exactly what a "is my parent still
    //     alive" watchdog needs: it keeps pointing at the parent pid, and the
    //     caller decides whether that pid is still a live process.
    //   - Linux: the PPid: line of /proc/<pid>/status.
    //   - macOS: ps (there is no /proc, and marshalling kinfo_proc costs more
    //     than it is worth here).
    // Returns false when it cannot be determined (parentPid is then 0).
    static class ProcessParentId
    {
        public static bool TryGet(int pid, out int parentPid)
        {
            parentPid = 0;
            try {
                if (OperatingSystem.IsWindows()) {
                    return TryGetWindows(pid, out parentPid);
                }
                if (OperatingSystem.IsLinux()) {
                    return TryGetProcFs(pid, out parentPid);
                }
                if (OperatingSystem.IsMacOS()) {
                    return TryGetMac(pid, out parentPid);
                }
            }
            catch {
                // fall through to the failure path below
            }
            parentPid = 0;
            return false;
        }

        private const uint c_ProcessQueryLimitedInformation = 0x1000;

        [StructLayout(LayoutKind.Sequential)]
        private struct ProcessBasicInfo
        {
            public IntPtr ExitStatus;
            public IntPtr PebBaseAddress;
            public IntPtr AffinityMask;
            public IntPtr BasePriority;
            public IntPtr UniqueProcessId;
            public IntPtr InheritedFromUniqueProcessId;
        }

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr handle, int infoClass, ref ProcessBasicInfo info, int size, out int returned);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint access, bool inherit, int pid);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        private static bool TryGetWindows(int pid, out int parentPid)
        {
            parentPid = 0;
            IntPtr handle = OpenProcess(c_ProcessQueryLimitedInformation, false, pid);
            if (handle == IntPtr.Zero) {
                return false;
            }
            try {
                var pbi = new ProcessBasicInfo();
                int returned;
                if (NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf<ProcessBasicInfo>(), out returned) != 0) {
                    return false;
                }
                long id = pbi.InheritedFromUniqueProcessId.ToInt64();
                if (id <= 0) {
                    return false;
                }
                parentPid = (int)id;
                return true;
            }
            finally {
                CloseHandle(handle);
            }
        }

        private static bool TryGetProcFs(int pid, out int parentPid)
        {
            parentPid = 0;
            string path = "/proc/" + pid.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/status";
            if (!File.Exists(path)) {
                return false;
            }
            foreach (var line in File.ReadLines(path)) {
                if (line.StartsWith("PPid:", StringComparison.Ordinal)) {
                    return int.TryParse(line.Substring(5).Trim(), System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out parentPid) && parentPid > 0;
                }
            }
            return false;
        }

        private static bool TryGetMac(int pid, out int parentPid)
        {
            parentPid = 0;
            var psi = new System.Diagnostics.ProcessStartInfo {
                FileName = "/bin/ps",
                Arguments = "-o ppid= -p " + pid.ToString(System.Globalization.CultureInfo.InvariantCulture),
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            if (null == proc) {
                return false;
            }
            string output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            return int.TryParse(output.Trim(), System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out parentPid) && parentPid > 0;
        }
    }

    // Parent pid of a process (the current one when called without arguments).
    sealed class GetProcessParentIdExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count > 1) {
                BatchCommand.Utils.HostBridge.Log?.Invoke("Expected: get_process_parent_id([pid])");
                return BoxedValue.From(0);
            }
            int pid;
            if (operands.Count == 1) {
                pid = operands[0].GetInt();
            }
            else {
                pid = System.Diagnostics.Process.GetCurrentProcess().Id;
            }
            int parentPid;
            if (pid <= 0 || !ProcessParentId.TryGet(pid, out parentPid)) {
                return BoxedValue.From(0);
            }
            return BoxedValue.From(parentPid);
        }
    }

    // Check whether a process with the given pid exists at query time.
    // The optional name key requires a case-insensitive process-name substring.
    // A matching name does not rule out pid reuse or establish process identity.
    // The process may exit immediately after this check.
    sealed class IsProcessAliveExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                BatchCommand.Utils.HostBridge.Log?.Invoke("Expected: is_process_alive(pid[, name_key])");
                return BoxedValue.FromBool(false);
            }
            int pid = operands[0].GetInt();
            if (pid <= 0) {
                return BoxedValue.FromBool(false);
            }
            string nameKey = operands.Count >= 2 ? (operands[1].AsString ?? string.Empty) : string.Empty;
            try {
                using var p = System.Diagnostics.Process.GetProcessById(pid);
                if (string.IsNullOrEmpty(nameKey)) {
                    return BoxedValue.FromBool(true);
                }
                string name;
                try {
                    name = p.ProcessName;
                }
                catch {
                    name = string.Empty;
                }
                return BoxedValue.FromBool(!string.IsNullOrEmpty(name) && name.IndexOf(nameKey, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            catch (ArgumentException) {
                // The process is gone.
                return BoxedValue.FromBool(false);
            }
            catch (Exception ex) {
                BatchCommand.Utils.HostBridge.Log?.Invoke("is_process_alive failed: " + ex.Message);
                return BoxedValue.FromBool(false);
            }
        }
    }

    // Search running processes by name and / or command line. Both keys are
    // case insensitive substrings and both are optional: an empty name key
    // matches every name, an empty command line key skips the command line
    // check (the two are AND-ed when both are given). Returns the list of the
    // matching pids (empty list when nothing matches) - listsize() gives the
    // old count semantics.
    //
    // Reading a command line is far more expensive than the name check, so
    // leave the second key empty unless instances of the same executable have
    // to be told apart by their arguments (two copies of the same host started
    // with different switches). A process whose command line cannot be read
    // never matches a non-empty command line key.
    sealed class SearchProcessExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var ids = new List<BoxedValue>();
            if (operands.Count > 2) {
                BatchCommand.Utils.HostBridge.Log?.Invoke("Expected: search_process([name_key[, cmd_line_key]])");
                return BoxedValue.FromObject(ids);
            }
            string nameKey = operands.Count >= 1 ? (operands[0].AsString ?? string.Empty) : string.Empty;
            string cmdLineKey = operands.Count >= 2 ? (operands[1].AsString ?? string.Empty) : string.Empty;
            if (nameKey.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
                nameKey = nameKey.Substring(0, nameKey.Length - 4);
            }
            bool matchName = !string.IsNullOrEmpty(nameKey);
            bool matchCmdLine = !string.IsNullOrEmpty(cmdLineKey);
            try {
                var procs = System.Diagnostics.Process.GetProcesses();
                foreach (var p in procs) {
                    try {
                        bool matched = true;
                        if (matched && matchName) {
                            string name;
                            try {
                                name = p.ProcessName;
                            }
                            catch {
                                name = string.Empty;
                            }
                            matched = !string.IsNullOrEmpty(name) && name.IndexOf(nameKey, StringComparison.OrdinalIgnoreCase) >= 0;
                        }
                        if (matched && matchCmdLine) {
                            string cmdLine;
                            matched = ProcessCommandLine.TryGet(p.Id, out cmdLine)
                                && cmdLine.IndexOf(cmdLineKey, StringComparison.OrdinalIgnoreCase) >= 0;
                        }
                        if (matched) {
                            ids.Add(BoxedValue.From(p.Id));
                        }
                    }
                    catch (Exception ex) {
                        BatchCommand.Utils.HostBridge.Log?.Invoke("search_process: skip a process: " + ex.Message);
                    }
                    finally {
                        p.Dispose();
                    }
                }
            }
            catch (Exception ex) {
                BatchCommand.Utils.HostBridge.Log?.Invoke("search_process failed: " + ex.Message);
            }
            return BoxedValue.FromObject(ids);
        }
    }

    // Read the command line of a running process, cross platform:
    //   - Windows: PEB via NtQueryInformationProcess + ReadProcessMemory (the
    //     same path the native host uses; a WOW64 target keeps 32-bit offsets).
    //   - Linux: /proc/<pid>/cmdline.
    //   - macOS: the KERN_PROCARGS2 sysctl (there is no /proc there).
    // Returns false when the command line cannot be read (access denied,
    // protected process, process gone, unsupported platform) - the caller falls
    // back to the process name so a process is never silently dropped.
    static class ProcessCommandLine
    {
        public static bool TryGet(int pid, out string cmdLine)
        {
            cmdLine = string.Empty;
            try {
                if (OperatingSystem.IsWindows()) {
                    return TryGetWindows(pid, out cmdLine);
                }
                if (OperatingSystem.IsLinux()) {
                    return TryGetProcFs(pid, out cmdLine);
                }
                if (OperatingSystem.IsMacOS()) {
                    return TryGetMac(pid, out cmdLine);
                }
            }
            catch {
                // fall through to the failure path below
            }
            cmdLine = string.Empty;
            return false;
        }

        private static bool TryGetProcFs(int pid, out string cmdLine)
        {
            cmdLine = string.Empty;
            string path = "/proc/" + pid.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/cmdline";
            if (!File.Exists(path)) {
                return false;
            }
            // The kernel separates the arguments with NUL bytes.
            cmdLine = File.ReadAllText(path).Replace('\0', ' ').Trim();
            return cmdLine.Length > 0;
        }

        private const int c_ProcessBasicInformation = 0;
        private const int c_ProcessWow64Information = 26;
        private const uint c_ProcessQueryLimitedInformation = 0x1000;
        private const uint c_ProcessVmRead = 0x0010;
        // PEB.ProcessParameters / RTL_USER_PROCESS_PARAMETERS.CommandLine offsets.
        // A 32-bit target keeps the 32-bit layout even when we run as 64-bit.
        private const int c_PebProcessParameters32 = 0x10;
        private const int c_PebProcessParameters64 = 0x20;
        private const int c_CommandLine32 = 0x40;
        private const int c_CommandLine64 = 0x70;

        [StructLayout(LayoutKind.Sequential)]
        private struct ProcessBasicInfo
        {
            public IntPtr ExitStatus;
            public IntPtr PebBaseAddress;
            public IntPtr AffinityMask;
            public IntPtr BasePriority;
            public IntPtr UniqueProcessId;
            public IntPtr InheritedFromUniqueProcessId;
        }

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr handle, int infoClass, ref ProcessBasicInfo info, int size, out int returned);
        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr handle, int infoClass, ref IntPtr info, int size, out int returned);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint access, bool inherit, int pid);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr handle, IntPtr address, byte[] buffer, UIntPtr size, out UIntPtr read);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool IsWow64Process(IntPtr handle, out bool wow64);

        private static bool TryGetWindows(int pid, out string cmdLine)
        {
            cmdLine = string.Empty;
            IntPtr handle = OpenProcess(c_ProcessQueryLimitedInformation | c_ProcessVmRead, false, pid);
            if (handle == IntPtr.Zero) {
                return false;
            }
            try {
                bool wow64 = false;
                if (!IsWow64Process(handle, out wow64)) {
                    return false;
                }
                // A 32-bit reader cannot address a native 64-bit target.
                if (IntPtr.Size == 4 && Environment.Is64BitOperatingSystem && !wow64) {
                    return false;
                }
                bool target32 = wow64 || !Environment.Is64BitOperatingSystem;
                int ptrSize = target32 ? 4 : 8;

                int returned;
                IntPtr peb = IntPtr.Zero;
                if (IntPtr.Size == 8 && wow64) {
                    if (NtQueryInformationProcess(handle, c_ProcessWow64Information, ref peb, IntPtr.Size, out returned) != 0) {
                        return false;
                    }
                }
                else {
                    var pbi = new ProcessBasicInfo();
                    if (NtQueryInformationProcess(handle, c_ProcessBasicInformation, ref pbi, Marshal.SizeOf<ProcessBasicInfo>(), out returned) != 0) {
                        return false;
                    }
                    peb = pbi.PebBaseAddress;
                }
                if (peb == IntPtr.Zero) {
                    return false;
                }
                IntPtr processParameters;
                if (!TryReadPointer(handle, peb + (target32 ? c_PebProcessParameters32 : c_PebProcessParameters64), ptrSize, out processParameters)
                    || processParameters == IntPtr.Zero) {
                    return false;
                }
                // UNICODE_STRING: Length, MaximumLength, Buffer (8 bytes on x86,
                // 16 on x64 where the buffer starts at offset 8).
                int usSize = target32 ? 8 : 16;
                var usBytes = new byte[usSize];
                UIntPtr read;
                if (!ReadProcessMemory(handle, processParameters + (target32 ? c_CommandLine32 : c_CommandLine64), usBytes, (UIntPtr)(uint)usSize, out read) || read.ToUInt64() != (ulong)usSize) {
                    return false;
                }
                int length = BitConverter.ToUInt16(usBytes, 0);
                IntPtr buffer = target32
                    ? (IntPtr.Size == 8 ? new IntPtr((long)BitConverter.ToUInt32(usBytes, 4)) : new IntPtr(BitConverter.ToInt32(usBytes, 4)))
                    : new IntPtr(BitConverter.ToInt64(usBytes, 8));
                if (length <= 0 || buffer == IntPtr.Zero) {
                    return false;
                }
                var data = new byte[length];
                if (!ReadProcessMemory(handle, buffer, data, (UIntPtr)(uint)length, out read) || read.ToUInt64() != (ulong)length) {
                    return false;
                }
                cmdLine = Encoding.Unicode.GetString(data);
                return cmdLine.Length > 0;
            }
            finally {
                CloseHandle(handle);
            }
        }

        private static bool TryReadPointer(IntPtr handle, IntPtr address, int ptrSize, out IntPtr value)
        {
            value = IntPtr.Zero;
            var buf = new byte[ptrSize];
            UIntPtr read;
            if (!ReadProcessMemory(handle, address, buf, (UIntPtr)(uint)ptrSize, out read) || read.ToUInt64() != (ulong)ptrSize) {
                return false;
            }
            value = ptrSize == 4
                ? (IntPtr.Size == 8 ? new IntPtr((long)BitConverter.ToUInt32(buf, 0)) : new IntPtr(BitConverter.ToInt32(buf, 0)))
                : new IntPtr(BitConverter.ToInt64(buf, 0));
            return true;
        }

        private const int c_CtlKern = 1;
        private const int c_KernProcArgs2 = 49;

        [DllImport("libc")]
        private static extern int sysctl(int[] name, uint namelen, IntPtr oldp, ref IntPtr oldlenp, IntPtr newp, IntPtr newlen);

        private static bool TryGetMac(int pid, out string cmdLine)
        {
            cmdLine = string.Empty;
            var mib = new int[] { c_CtlKern, c_KernProcArgs2, pid };
            IntPtr len = IntPtr.Zero;
            // First call: query the buffer size.
            if (sysctl(mib, 3, IntPtr.Zero, ref len, IntPtr.Zero, IntPtr.Zero) != 0 || len.ToInt64() <= 0) {
                return false;
            }
            long capacity = len.ToInt64();
            if (capacity < sizeof(int) || capacity > int.MaxValue) {
                return false;
            }
            IntPtr buffer = Marshal.AllocHGlobal(len);
            try {
                if (sysctl(mib, 3, buffer, ref len, IntPtr.Zero, IntPtr.Zero) != 0) {
                    return false;
                }
                long returnedSize = len.ToInt64();
                if (returnedSize < sizeof(int) || returnedSize > capacity) {
                    return false;
                }
                int size = (int)returnedSize;
                var data = new byte[size];
                Marshal.Copy(buffer, data, 0, size);
                int argc = BitConverter.ToInt32(data, 0);
                if (argc < 0) {
                    return false;
                }
                // Layout: argc, exec path, padding, argv, then environment.
                int pos = sizeof(int);
                int pathStart = pos;
                while (pos < size && data[pos] != 0) {
                    ++pos;
                }
                if (pos == size) {
                    return false;
                }
                string execPath = Encoding.UTF8.GetString(data, pathStart, pos - pathStart);
                ++pos;
                if (argc == 0) {
                    cmdLine = execPath;
                    return cmdLine.Length > 0;
                }
                // Skip padding only before argv[0], not between arguments.
                // An empty argv[0] is indistinguishable from padding here.
                while (pos < size && data[pos] == 0) {
                    ++pos;
                }
                if (argc > size - pos) {
                    return false;
                }
                var argv = new List<string>();
                for (int i = 0; i < argc; ++i) {
                    int start = pos;
                    while (pos < size && data[pos] != 0) {
                        ++pos;
                    }
                    if (pos == size) {
                        return false;
                    }
                    argv.Add(Encoding.UTF8.GetString(data, start, pos - start));
                    ++pos;
                }
                cmdLine = string.Join(" ", argv);
                return cmdLine.Length > 0;
            }
            finally {
                Marshal.FreeHGlobal(buffer);
            }

        }
    }

    // Kill OS processes by name (with or without the .exe suffix) or by pid.
    // Returns the number of processes killed.
    sealed class KillProcessExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                BatchCommand.Utils.HostBridge.Log?.Invoke("Expected: kill_process(name_or_pid)");
                return BoxedValue.From(0);
            }
            try {
                int stopped = 0;
                if (operands[0].IsInteger) {
                    int pid = operands[0].GetInt();
                    try {
                        using var proc = System.Diagnostics.Process.GetProcessById(pid);
                        proc.Kill();
                        stopped = 1;
                    }
                    catch (ArgumentException) {
                        // process already gone
                    }
                }
                else {
                    string name = operands[0].AsString ?? string.Empty;
                    if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
                        name = name.Substring(0, name.Length - 4);
                    }
                    if (string.IsNullOrEmpty(name)) {
                        return BoxedValue.From(0);
                    }
                    foreach (var proc in System.Diagnostics.Process.GetProcessesByName(name)) {
                        try {
                            proc.Kill();
                            stopped++;
                        }
                        catch (Exception ex) {
                            BatchCommand.Utils.HostBridge.Log?.Invoke("kill_process kill failed: " + ex.Message);
                        }
                        finally {
                            proc.Dispose();
                        }
                    }
                }
                return BoxedValue.From(stopped);
            }
            catch (Exception ex) {
                BatchCommand.Utils.HostBridge.Log?.Invoke("kill_process failed: " + ex.Message);
                return BoxedValue.From(0);
            }
        }
    }
}
