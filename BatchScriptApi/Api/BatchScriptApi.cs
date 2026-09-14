using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace BatchCommand.Api
{
    // BatchScript-related api expressions used by every DslHost (the companion
    // of BatchScript.cs, the interpreter itself): script file imports, in-script
    // calls, sub-script execution, the script task pool, host logging, quoting
    // helpers and the api help search. Host specific behavior (logging, process
    // info, help search, task state) is taken from DslHost.Current. Errors are
    // recorded through ApiErrorInfo and returned to the caller as part of the
    // unified execution error text.
    // Registered by BatchScriptApiRegistrar.RegisterBatchScriptApis().

    // import(dsl_file, ...) - load additional dsl files (relative to managed/)
    public sealed class ImportExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var host = DslHost.Current;
            var files = new List<string>();
            for (int ix = 0; ix < operands.Count; ix++) {
                var str = operands[ix].AsString;
                if (!string.IsNullOrEmpty(str)) {
                    string path;
                    if (Path.IsPathRooted(str)) {
                        path = str;
                    }
                    else {
                        path = Path.Combine(host?.BasePath ?? string.Empty, "managed", str);
                    }
                    files.Add(path);
                }
            }
            BatchScript.LoadImportFiles(files);
            if (BatchScript.HasDslErrors)
                return BoxedValue.FromBool(false);
            if (null != host) {
                foreach (var file in files) {
                    host.Log?.Invoke("[csharp] Import: " + file);
                }
            }
            return BoxedValue.FromBool(true);
        }
    }

    // redirectcall(func_name[, args[, extra...]]) - call a dsl func with an arg list
    public sealed class RedirectCallExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            int num = operands.Count;
            if (num < 1) {
                ApiErrorInfo.AppendLine("Expected: redirectcall(func_name) or redirectcall(func_name, args) or redirectcall(func_name, args, ...)");
                return BoxedValue.EmptyString;
            }
            else {
                string func_name = operands[0].AsString;
                if (num == 1) {
                    return BatchScript.Call(func_name);
                }
                else if (num == 2) {
                    // expand original args
                    var args = operands[1].As<IList<BoxedValue>>();
                    return BatchScript.Call(func_name, args);
                }
                else {
                    // expand original args
                    var args = operands[1].As<IList<BoxedValue>>();
                    // add other args
                    var newArgs = BatchScript.NewCalculatorValueList();
                    newArgs.AddRange(args);
                    for (int ix = 2; ix < num; ix++) {
                        newArgs.Add(operands[ix]);
                    }
                    BoxedValue r = BatchScript.Call(func_name, newArgs);
                    BatchScript.RecycleCalculatorValueList(newArgs);
                    return r;
                }
            }
        }
    }

    // execute_metadsl(dsl_code) - run a dsl snippet, return (bool, result)
    public sealed class ExecuteMetaDslExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: execute_metadsl(dsl_code), aliased as executemetadsl");
                return BoxedValue.From(-1);
            }
            string dslCode = operands[0].AsString;
            bool hasError;
            string res;
            res = DslHost.Current!.ExecuteScript(dslCode, 0, out hasError);
            return BoxedValue.From(Tuple.Create(BoxedValue.FromBool(hasError), BoxedValue.FromString(res)));
        }
    }

    // call_metadsl_task(task_index, func_name, args...) - queue a dsl func to a worker thread
    public sealed class CallMetaDslTaskExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var host = DslHost.Current;
            if (null == host) {
                return BoxedValue.From(false);
            }
            if (operands.Count < 2) {
                ApiErrorInfo.AppendLine("Expected: call_metadsl_task(task_index, func_name, arg1, arg2, ...)");
                return BoxedValue.From(false);
            }
            int taskIndex = operands[0].GetInt();
            string funcName = operands[1].AsString;
            if (string.IsNullOrEmpty(funcName)) {
                ApiErrorInfo.AppendLine("Expected: call_metadsl_task(task_index, func_name, arg1, arg2, ...), func_name is empty");
                return BoxedValue.From(false);
            }
            // Build the args on the calling thread; the list is handed to the worker and
            // never touched again here, so no synchronization is needed on it.
            // The values keep their BoxedValue type: nothing here forces a string round
            // trip, so numbers stay numbers and lists stay lists.
            // Strings and numbers are immutable, so they are safe to share; a mutable
            // collection must not be modified by the caller after this point.
            var args = new List<BoxedValue>();
            for (int ix = 2; ix < operands.Count; ix++) {
                args.Add(operands[ix]);
            }
            // Host supplied state (e.g. the AgentCore context of the requesting
            // execution) travels with the task so async callbacks initiated
            // inside it are delivered to the originating connection.
            var state = host.CaptureTaskState?.Invoke();
            // Fire-and-forget: queue to a worker thread so slow work such as sqlite
            // writes cannot block the caller.
            return BoxedValue.From(host.EnqueueMetaDslTask(taskIndex, funcName, args, state));
        }
    }

    // set_metadsl_task_num(num) - set default worker count
    public sealed class SetMetaDslTaskNumExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                ApiErrorInfo.AppendLine("Expected: set_metadsl_task_num(num)");
                return BoxedValue.From(0);
            }
            return BoxedValue.From(DslHost.Current!.SetMetaDslTaskNum(operands[0].GetInt()));
        }
    }

    // get_metadsl_task_num() - live worker count
    public sealed class GetMetaDslTaskNumExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.From(DslHost.Current!.GetMetaDslTaskNum());
        }
    }

    // nativelog(fmt, ...) - log to the host log sink
    public sealed class NativeLogExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count == 0) {
                return BoxedValue.EmptyString;
            }
            string str;
            if (operands.Count == 1) {
                // Single parameter: output directly without string.Format
                str = operands[0].AsString;
            }
            else {
                string fmt = operands[0].AsString;
                var al = new System.Collections.ArrayList();
                for (int ix = 1; ix < operands.Count; ix++) {
                    al.Add(operands[ix].IsNullObject ? null : operands[ix].GetObject());
                }
                try {
                    str = string.Format(fmt, al.ToArray());
                }
                catch (Exception ex) {
                    ApiErrorInfo.AppendLine("nativelog format error: " + ex.Message);
                    return BoxedValue.FromString(fmt);
                }
            }
            DslHost.Current?.Log?.Invoke(str);
            return BoxedValue.FromString(str);
        }
    }

    // quotestring(str) - quote a string for dsl code (numbers stay bare)
    public sealed class QuoteStringExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: quotestring(str)");
                return BoxedValue.EmptyString;
            }
            string str = operands[0].AsString;
            return BoxedValue.FromString(DslHost.QuoteString(str));
        }
    }

    // stripquotes(str) - remove surrounding double/single quotes
    public sealed class StripQuotesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: stripquotes(str)");
                return BoxedValue.EmptyString;
            }
            string str = operands[0].AsString;
            return BoxedValue.FromString(DslHost.StripQuotes(str));
        }
    }

    // try_get_raw_command_line_switch(str) - read --switch=value, return (bool, str)
    public sealed class TryGetRawCommandLineSwitchExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: try_get_raw_command_line_switch(str), return (bool, str)");
                return BoxedValue.EmptyString;
            }
            string str = operands[0].AsString;
            if (!string.IsNullOrEmpty(str)) {
                if (DslHost.TryGetSwitchValueFromRawCommandLine(DslHost.Current?.CmdLine ?? string.Empty, str, out var val)) {
                    return Tuple.Create(BoxedValue.FromBool(true), BoxedValue.FromString(val));
                }
            }
            return Tuple.Create(BoxedValue.FromBool(true), BoxedValue.EmptyString);
        }
    }

    // getdotnetinfo() - dotnet runtime info
    public sealed class GetDotnetInfoExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var host = DslHost.Current;
            var sb = new StringBuilder();
            sb.AppendLine("AppContext.BaseDirectory: " + AppContext.BaseDirectory);
            sb.AppendLine("AppDomain.BaseDirectory: " + AppDomain.CurrentDomain.BaseDirectory);
            var entry = Assembly.GetEntryAssembly()?.Location ?? "<null>";
            sb.AppendLine("EntryAssembly.Location: " + entry);
            sb.AppendLine("ExecutingAssembly.Location: " + Assembly.GetExecutingAssembly().Location);
            sb.AppendLine("Process.MainModule: " + Process.GetCurrentProcess().MainModule?.FileName);
            sb.AppendLine("Environment.CurrentDirectory: " + Environment.CurrentDirectory);
            sb.AppendLine("BasePath: " + (host?.BasePath ?? string.Empty));
            sb.AppendLine("AppDir: " + (host?.AppDir ?? string.Empty));
            sb.AppendLine("IsMac: " + (host?.IsMac ?? false));
            sb.AppendLine("FrameworkDescription: " + RuntimeInformation.FrameworkDescription);
            sb.AppendLine("OSArchitecture: " + RuntimeInformation.OSArchitecture);
            sb.AppendLine("OSDescription: " + RuntimeInformation.OSDescription);
            sb.AppendLine("ProcessArchitecture: " + RuntimeInformation.ProcessArchitecture);
            sb.AppendLine("RuntimeIdentifier: " + RuntimeInformation.RuntimeIdentifier);
            return sb.ToString();
        }
    }

    // get_string_in_length(str, len[, begin0_end1_or_beginend2])
    public sealed class GetStringInLengthExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: get_string_in_length(str,len[,begin0_end1_or_beginend2])");
                return BoxedValue.EmptyString;
            }
            string str = operands[0].AsString;
            int len = operands[1].GetInt();
            int beginOrEndOrBeginEnd = operands.Count > 2 ? operands[2].GetInt() : 0;
            return BoxedValue.FromString(DslHost.GetStringInLength(str, len, beginOrEndOrBeginEnd));
        }
    }

    // help(pattern, ...) - regex search over user api docs
    public sealed class HelpExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var sb = new StringBuilder();
            var regexes = new List<Regex>();
            foreach (var op in operands) {
                string pattern = op.ToString();
                regexes.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
            }
            // regex match over UserApiDocs
            var matchedApiKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in BatchScript.UserApiDocs) {
                bool match = regexes.Count == 0;
                string info = string.Format("{0}: {1}", pair.Key, pair.Value);
                foreach (var regex in regexes) {
                    if (regex.IsMatch(info)) {
                        match = true;
                        break;
                    }
                }
                if (match) {
                    matchedApiKeys.Add(pair.Key);
                    sb.AppendLine(info);
                }
            }
            DslHost.Current?.SearchHelp?.Invoke(sb, BatchScript.UserApiDocs, regexes, matchedApiKeys);
            return sb.ToString();
        }
    }

    // helpall(pattern, ...) - regex search over all api docs
    public sealed class HelpAllExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var sb = new StringBuilder();
            var regexes = new List<Regex>();
            foreach (var op in operands) {
                string pattern = op.ToString();
                regexes.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
            }
            var matchedApiKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in BatchScript.ApiDocs) {
                bool match = regexes.Count == 0;
                string info = string.Format("{0}: {1}", pair.Key, pair.Value);
                foreach (var regex in regexes) {
                    if (regex.IsMatch(info)) {
                        match = true;
                        break;
                    }
                }
                if (match) {
                    matchedApiKeys.Add(pair.Key);
                    sb.AppendLine(info);
                }
            }
            DslHost.Current?.SearchHelp?.Invoke(sb, BatchScript.ApiDocs, regexes, matchedApiKeys);
            return sb.ToString();
        }
    }
}
