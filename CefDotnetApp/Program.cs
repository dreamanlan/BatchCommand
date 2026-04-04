
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.IO;
using System.Reflection;
using ScriptableFramework;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Net;
using AgentPlugin.Abstractions;
using System.Text.RegularExpressions;
using System.Linq;
using BatchCommand;

internal static class Program
{
    internal static void Main()
    {
        Console.WriteLine("[csharp] Program.Main");
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct HostApi
{
    public IntPtr NativeLog;
    public IntPtr SendCefMessage;
    public IntPtr SendJavascriptCode;
    public IntPtr SendJavascriptCall;
    public IntPtr CallJavascriptFuncInRenderer;
    public IntPtr FreeNativeString;
    public IntPtr CommandLineHasSwitch;
    public IntPtr CommandLineGetSwitchValue;
    public IntPtr CommandLineAppendSwitch;
    public IntPtr CommandLineAppendSwitchWithValue;
    public IntPtr CommandLineRemoveSwitch;
    // CommandLine extended
    public IntPtr CommandLineIsValid;
    public IntPtr CommandLineIsReadOnly;
    public IntPtr CommandLineHasSwitches;
    public IntPtr CommandLineHasArguments;
    public IntPtr CommandLineGetProgram;
    public IntPtr CommandLineSetProgram;
    public IntPtr CommandLineGetCommandLineString;
    public IntPtr CommandLineGetArgv;
    public IntPtr CommandLineGetSwitches;
    public IntPtr CommandLineGetArguments;
    public IntPtr CommandLineAppendArgument;
    public IntPtr CommandLinePrependWrapper;
    public IntPtr CommandLineGetGlobal;
    // Browser traversal
    public IntPtr GetBrowserById;
    public IntPtr BrowserIsValid;
    public IntPtr GetRendererBrowserFrameById;
    // Browser properties
    public IntPtr BrowserGetId;
    public IntPtr BrowserGetUrl;
    public IntPtr BrowserIsLoading;
    public IntPtr BrowserIsPopup;
    public IntPtr BrowserHasDocument;
    // Browser frame access
    public IntPtr BrowserGetFrameCount;
    public IntPtr BrowserGetFrameIdentifiers;
    public IntPtr BrowserGetFrameNames;
    public IntPtr BrowserGetMainFrame;
    public IntPtr BrowserGetFocusedFrame;
    public IntPtr BrowserGetFrameByIdentifier;
    public IntPtr BrowserGetFrameByName;
    // Browser actions
    public IntPtr BrowserReload;
    public IntPtr BrowserReloadIgnoreCache;
    public IntPtr BrowserStopLoad;
    // Browser host actions
    public IntPtr BrowserClose;
    public IntPtr BrowserSetFocus;
    public IntPtr BrowserGetOpenerId;
    // Frame properties
    public IntPtr FrameGetUrl;
    public IntPtr FrameGetName;
    public IntPtr FrameGetIdentifier;
    public IntPtr FrameIsMain;
    public IntPtr FrameIsValid;
    public IntPtr FrameIsFocused;
    public IntPtr FrameGetParent;
    public IntPtr FrameGetBrowser;
    // Frame actions
    public IntPtr FrameLoadUrl;
    // CefRequest properties
    public IntPtr RequestIsReadOnly;
    public IntPtr RequestGetUrl;
    public IntPtr RequestGetMethod;
    public IntPtr RequestGetReferrerUrl;
    public IntPtr RequestGetReferrerPolicy;
    public IntPtr RequestGetHeaderMap;
    public IntPtr RequestGetHeaderByName;
    public IntPtr RequestGetFlags;
    public IntPtr RequestGetFirstPartyForCookies;
    public IntPtr RequestGetResourceType;
    public IntPtr RequestGetTransitionType;
    public IntPtr RequestGetIdentifier;
    // Heartbeat control
    public IntPtr SetHeartbeatInterval;
}

// delegate for native api
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostNativeLogDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostSendCefMessageDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, IntPtr args, int argCount, IntPtr browser, IntPtr frame, int cef_process_id);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostSendJavascriptCodeDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string code, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostSendJavascriptCallDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, IntPtr args, int argCount, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostCallJavascriptFuncInRendererDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, IntPtr args, int argCount, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostFreeNativeStringDelegation(IntPtr str);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCommandLineHasSwitchDelegation(IntPtr command_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostCommandLineGetSwitchValueDelegation(IntPtr command_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostCommandLineAppendSwitchDelegation(IntPtr command_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostCommandLineAppendSwitchWithValueDelegation(IntPtr command_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostCommandLineRemoveSwitchDelegation(IntPtr command_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
// CommandLine extended
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCommandLineIsValidDelegation(IntPtr command_line);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCommandLineIsReadOnlyDelegation(IntPtr command_line);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCommandLineHasSwitchesDelegation(IntPtr command_line);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCommandLineHasArgumentsDelegation(IntPtr command_line);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostCommandLineGetProgramDelegation(IntPtr command_line);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostCommandLineSetProgramDelegation(IntPtr command_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string program);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostCommandLineGetCommandLineStringDelegation(IntPtr command_line);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostCommandLineGetArgvDelegation(IntPtr command_line);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostCommandLineGetSwitchesDelegation(IntPtr command_line);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostCommandLineGetArgumentsDelegation(IntPtr command_line);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostCommandLineAppendArgumentDelegation(IntPtr command_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string argument);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostCommandLinePrependWrapperDelegation(IntPtr command_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string wrapper);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostCommandLineGetGlobalDelegation();
// Browser traversal
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostGetBrowserByIdDelegation(int browser_id);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
public delegate bool HostBrowserIsValidDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
public delegate bool HostGetRendererBrowserFrameByIdDelegation(int browser_id, out IntPtr browser, out IntPtr frame);
// Browser properties
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostBrowserGetIdDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostBrowserGetUrlDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostBrowserIsLoadingDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostBrowserIsPopupDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostBrowserHasDocumentDelegation(IntPtr browser);
// Browser frame access
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostBrowserGetFrameCountDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostBrowserGetFrameIdentifiersDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostBrowserGetFrameNamesDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostBrowserGetMainFrameDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostBrowserGetFocusedFrameDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostBrowserGetFrameByIdentifierDelegation(IntPtr browser, [MarshalAs(UnmanagedType.LPUTF8Str)] string identifier);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostBrowserGetFrameByNameDelegation(IntPtr browser, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
// Browser actions
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostBrowserReloadDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostBrowserReloadIgnoreCacheDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostBrowserStopLoadDelegation(IntPtr browser);
// Browser host actions
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostBrowserCloseDelegation(IntPtr browser, int force_close);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostBrowserSetFocusDelegation(IntPtr browser, int focus);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostBrowserGetOpenerIdDelegation(IntPtr browser);
// Frame properties
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostFrameGetUrlDelegation(IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostFrameGetNameDelegation(IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostFrameGetIdentifierDelegation(IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostFrameIsMainDelegation(IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostFrameIsValidDelegation(IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostFrameIsFocusedDelegation(IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostFrameGetParentDelegation(IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostFrameGetBrowserDelegation(IntPtr frame);
// Frame actions
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostFrameLoadUrlDelegation(IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url);
// CefRequest properties
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostRequestIsReadOnlyDelegation(IntPtr request);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostRequestGetUrlDelegation(IntPtr request);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostRequestGetMethodDelegation(IntPtr request);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostRequestGetReferrerUrlDelegation(IntPtr request);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostRequestGetReferrerPolicyDelegation(IntPtr request);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostRequestGetHeaderMapDelegation(IntPtr request);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostRequestGetHeaderByNameDelegation(IntPtr request, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostRequestGetFlagsDelegation(IntPtr request);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostRequestGetFirstPartyForCookiesDelegation(IntPtr request);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostRequestGetResourceTypeDelegation(IntPtr request);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostRequestGetTransitionTypeDelegation(IntPtr request);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate ulong HostRequestGetIdentifierDelegation(IntPtr request);
// Heartbeat control
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostSetHeartbeatIntervalDelegation(int interval_ms);

namespace DotNetLib
{
    sealed class ImportExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var files = new List<string>();
            for (int ix = 1; ix < operands.Count; ix++) {
                var str = operands[ix].AsString;
                if (!string.IsNullOrEmpty(str)) {
                    string path;
                    if (Path.IsPathRooted(str)) {
                        path = str;
                    }
                    else {
                        path = Path.Combine(Lib.BasePath, "managed", str);
                    }
                    files.Add(path);
                }
            }
            BatchScript.LoadIncludes(files);
            if (BatchScript.HasDslErrors)
                return BoxedValue.FromBool(false);
            return BoxedValue.FromBool(true);
        }
    }
    sealed class NativeLogExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            string str;
            if (operands.Count == 1) {
                // Single parameter: output directly without string.Format
                str = operands[0].AsString;
            }
            else {
                // Multiple parameters: use string.Format
                string fmt = operands[0].AsString;
                var al = new System.Collections.ArrayList();
                for (int ix = 1; ix < operands.Count; ix++) {
                    al.Add(operands[ix].GetObject());
                }
                str = string.Format(fmt, al.ToArray());
            }
            Lib.NativeLog(str);
            return str;
        }
    }
    sealed class JavascriptLogExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            string str;
            if (operands.Count == 1) {
                // Single parameter: output directly without string.Format
                str = operands[0].AsString;
            }
            else {
                // Multiple parameters: use string.Format
                string fmt = operands[0].AsString;
                var al = new System.Collections.ArrayList();
                for (int ix = 1; ix < operands.Count; ix++) {
                    al.Add(operands[ix].GetObject());
                }
                str = string.Format(fmt, al.ToArray());
            }
            Lib.JsLog(str);
            return str;
        }
    }
    sealed class QuoteStringExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                NativeApi.AppendApiErrorInfoLine("Expected: quotestring(str)");
                return BoxedValue.EmptyString;
            }
            string str = operands[0].AsString;
            if (!string.IsNullOrEmpty(str)) {
                NativeApi.QuoteString(str);
            }
            return BoxedValue.EmptyString;
        }
    }
    sealed class StripQuotesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                NativeApi.AppendApiErrorInfoLine("Expected: stripquotes(str)");
                return BoxedValue.EmptyString;
            }
            string str = operands[0].AsString;
            if (!string.IsNullOrEmpty(str)) {
                NativeApi.QuoteString(str);
            }
            return BoxedValue.EmptyString;
        }
    }
    sealed class TryGetRawCommandLineSwitchExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                NativeApi.AppendApiErrorInfoLine("Expected: try_get_raw_command_line_switch(str), return (bool, str)");
                return BoxedValue.EmptyString;
            }
            string str = operands[0].AsString;
            if (!string.IsNullOrEmpty(str)) {
                if (Lib.TryGetSwitchValueFromRawCommandLine(Lib.CmdLine, str, out var val)) {
                    return Tuple.Create(BoxedValue.FromBool(true), BoxedValue.FromString(val));
                }
            }
            return Tuple.Create(BoxedValue.FromBool(true), BoxedValue.EmptyString);
        }
    }
    sealed class GetDotnetInfoExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var sb = new StringBuilder();
            sb.AppendLine("AppContext.BaseDirectory: " + AppContext.BaseDirectory);
            sb.AppendLine("AppDomain.BaseDirectory: " + AppDomain.CurrentDomain.BaseDirectory);
            var entry = Assembly.GetEntryAssembly()?.Location ?? "<null>";
            sb.AppendLine("EntryAssembly.Location: " + entry);
            sb.AppendLine("ExecutingAssembly.Location: " + Assembly.GetExecutingAssembly().Location);
            sb.AppendLine("Process.MainModule: " + Process.GetCurrentProcess().MainModule?.FileName);
            sb.AppendLine("Environment.CurrentDirectory: " + Environment.CurrentDirectory);
            sb.AppendLine("BasePath: " + Lib.BasePath);
            sb.AppendLine("AppDir: " + Lib.AppDir);
            sb.AppendLine("IsMac: " + Lib.IsMac);
            return sb.ToString();
        }
    }
    sealed class HandleThreadQueueExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            int maxNativeCount = int.MaxValue;
            int maxJsCount = int.MaxValue;
            int maxCodeCount = int.MaxValue;
            int maxFuncCount = int.MaxValue;
            if (operands.Count >= 1) {
                maxNativeCount = operands[0].GetInt();
                maxJsCount = maxNativeCount;
                maxCodeCount = maxNativeCount;
                maxFuncCount = maxNativeCount;
            }
            if (operands.Count >= 2) {
                maxJsCount = operands[1].GetInt();
            }
            if (operands.Count >= 3) {
                maxCodeCount = operands[2].GetInt();
            }
            if (operands.Count >= 4) {
                maxFuncCount = operands[3].GetInt();
            }
            Lib.HandleThreadQueue(maxNativeCount, maxJsCount, maxCodeCount, maxFuncCount);
            return BoxedValue.NullObject;
        }
    }
    sealed class SetHeartBeatIntervalExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                NativeApi.AppendApiErrorInfoLine("Expected: set_heartbeat_interval(interval_ms)");
                return BoxedValue.NullObject;
            }
            int intervalMs = operands[0].GetInt();
            Lib.SetHeartbeatInterval(intervalMs);
            return BoxedValue.NullObject;
        }
    }
    sealed class GetBrowserIdsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var ids = Lib.GetAllContextBrowserIds();
            var list = new List<BoxedValue>(ids.Length);
            foreach (var id in ids) {
                list.Add(BoxedValue.From(id));
            }
            return BoxedValue.FromObject(list);
        }
    }
    sealed class SetContextByIdExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                NativeApi.AppendApiErrorInfoLine("Expected: set_context_by_id(browser_id)");
                return BoxedValue.FromBool(false);
            }
            int browserId = operands[0].GetInt();
            bool ok = Lib.SetContextById(browserId);
            return BoxedValue.FromBool(ok);
        }
    }
    sealed class FindBrowserIdByUrlKeyExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                NativeApi.AppendApiErrorInfoLine("Expected: find_browser_id_by_url_key(url_key)");
                return BoxedValue.From(-1);
            }
            string urlKey = operands[0].AsString;
            int id = Lib.FindBrowserIdByUrlKey(urlKey);
            return BoxedValue.From(id);
        }
    }
    sealed class HelpExp : SimpleExpressionBase
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
            foreach (var pair in BatchCommand.BatchScript.UserApiDocs) {
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
            // semantic search over UserApiDocs
            if (regexes.Count > 0 && Lib.AgentPlugin != null) {
                var queries = new List<string>(regexes.Count);
                foreach (var regex in regexes) {
                    string q = NativeApi.CleanStringData(regex.ToString());
                    if (!string.IsNullOrWhiteSpace(q))
                        queries.Add(q);
                }
                var semanticResults = Lib.AgentPlugin.SemanticSearch(
                    queries,
                    BatchCommand.BatchScript.UserApiDocs.Select(p => (p.Key, p.Key + ": " + p.Value)),
                    5);
                if (semanticResults != null) {
                    foreach (var (key, text, score) in semanticResults) {
                        if (!matchedApiKeys.Contains(key)) {
                            sb.AppendLine(string.Format("{0} ({1})", text, score));
                        }
                    }
                }
            }
            if (null != Lib.AgentPlugin) {
                string infos = Lib.AgentPlugin.SkillHelp(regexes);
                sb.Append(infos);
            }
            return sb.ToString();
        }
    }
    sealed class HelpAllExp : SimpleExpressionBase
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
            foreach (var pair in Calculator.ApiDocs) {
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
            // semantic search over apiDocs
            if (regexes.Count > 0 && Lib.AgentPlugin != null) {
                var queries = new List<string>(regexes.Count);
                foreach (var regex in regexes) {
                    string q = NativeApi.CleanStringData(regex.ToString());
                    if (!string.IsNullOrWhiteSpace(q))
                        queries.Add(q);
                }
                var semanticResults = Lib.AgentPlugin.SemanticSearch(
                    queries,
                    BatchCommand.BatchScript.ApiDocs.Select(p => (p.Key, p.Key + ": " + p.Value)),
                    5);
                if (semanticResults != null) {
                    foreach (var (key, text, score) in semanticResults) {
                        if (!matchedApiKeys.Contains(key)) {
                            sb.AppendLine(string.Format("{0} ({1})", text, score));
                        }
                    }
                }
            }
            if (null != Lib.AgentPlugin) {
                string infos = Lib.AgentPlugin.SkillHelp(regexes);
                sb.Append(infos);
            }
            return sb.ToString();
        }
    }
    public enum CefProcessType
    {
        BrowserProcess,
        RendererProcess,
        ZygoteProcess,
        OtherProcess,
    };
    public enum CefProcessId
    {
        PID_BROWSER,
        PID_RENDERER,
    }
    public class NativeApi : INativeApi, IErrorReporter, IDslEngine
    {
        public NativeApi(IntPtr apis)
        {
            HostApi hostApi = Marshal.PtrToStructure<HostApi>(apis);
            m_NativeLogApi = Marshal.GetDelegateForFunctionPointer<HostNativeLogDelegation>(hostApi.NativeLog);
            m_SendCefMessageApi = Marshal.GetDelegateForFunctionPointer<HostSendCefMessageDelegation>(hostApi.SendCefMessage);
            m_SendJavascriptCodeApi = Marshal.GetDelegateForFunctionPointer<HostSendJavascriptCodeDelegation>(hostApi.SendJavascriptCode);
            m_SendJavascriptCallApi = Marshal.GetDelegateForFunctionPointer<HostSendJavascriptCallDelegation>(hostApi.SendJavascriptCall);
            m_CallJavascriptFuncInRendererApi = Marshal.GetDelegateForFunctionPointer<HostCallJavascriptFuncInRendererDelegation>(hostApi.CallJavascriptFuncInRenderer);
            m_FreeNativeStringApi = Marshal.GetDelegateForFunctionPointer<HostFreeNativeStringDelegation>(hostApi.FreeNativeString);
            m_CommandLineHasSwitchApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineHasSwitchDelegation>(hostApi.CommandLineHasSwitch);
            m_CommandLineGetSwitchValueApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineGetSwitchValueDelegation>(hostApi.CommandLineGetSwitchValue);
            m_CommandLineAppendSwitchApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineAppendSwitchDelegation>(hostApi.CommandLineAppendSwitch);
            m_CommandLineAppendSwitchWithValueApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineAppendSwitchWithValueDelegation>(hostApi.CommandLineAppendSwitchWithValue);
            m_CommandLineRemoveSwitchApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineRemoveSwitchDelegation>(hostApi.CommandLineRemoveSwitch);
            // CommandLine extended
            m_CommandLineIsValidApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineIsValidDelegation>(hostApi.CommandLineIsValid);
            m_CommandLineIsReadOnlyApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineIsReadOnlyDelegation>(hostApi.CommandLineIsReadOnly);
            m_CommandLineHasSwitchesApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineHasSwitchesDelegation>(hostApi.CommandLineHasSwitches);
            m_CommandLineHasArgumentsApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineHasArgumentsDelegation>(hostApi.CommandLineHasArguments);
            m_CommandLineGetProgramApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineGetProgramDelegation>(hostApi.CommandLineGetProgram);
            m_CommandLineSetProgramApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineSetProgramDelegation>(hostApi.CommandLineSetProgram);
            m_CommandLineGetCommandLineStringApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineGetCommandLineStringDelegation>(hostApi.CommandLineGetCommandLineString);
            m_CommandLineGetArgvApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineGetArgvDelegation>(hostApi.CommandLineGetArgv);
            m_CommandLineGetSwitchesApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineGetSwitchesDelegation>(hostApi.CommandLineGetSwitches);
            m_CommandLineGetArgumentsApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineGetArgumentsDelegation>(hostApi.CommandLineGetArguments);
            m_CommandLineAppendArgumentApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineAppendArgumentDelegation>(hostApi.CommandLineAppendArgument);
            m_CommandLinePrependWrapperApi = Marshal.GetDelegateForFunctionPointer<HostCommandLinePrependWrapperDelegation>(hostApi.CommandLinePrependWrapper);
            m_CommandLineGetGlobalApi = Marshal.GetDelegateForFunctionPointer<HostCommandLineGetGlobalDelegation>(hostApi.CommandLineGetGlobal);
            m_GetBrowserByIdApi = Marshal.GetDelegateForFunctionPointer<HostGetBrowserByIdDelegation>(hostApi.GetBrowserById);
            m_BrowserIsValidApi = Marshal.GetDelegateForFunctionPointer<HostBrowserIsValidDelegation>(hostApi.BrowserIsValid);
            m_GetRendererBrowserFrameByIdApi = Marshal.GetDelegateForFunctionPointer<HostGetRendererBrowserFrameByIdDelegation>(hostApi.GetRendererBrowserFrameById);
            m_BrowserGetIdApi = Marshal.GetDelegateForFunctionPointer<HostBrowserGetIdDelegation>(hostApi.BrowserGetId);
            m_BrowserGetUrlApi = Marshal.GetDelegateForFunctionPointer<HostBrowserGetUrlDelegation>(hostApi.BrowserGetUrl);
            m_BrowserIsLoadingApi = Marshal.GetDelegateForFunctionPointer<HostBrowserIsLoadingDelegation>(hostApi.BrowserIsLoading);
            m_BrowserIsPopupApi = Marshal.GetDelegateForFunctionPointer<HostBrowserIsPopupDelegation>(hostApi.BrowserIsPopup);
            m_BrowserHasDocumentApi = Marshal.GetDelegateForFunctionPointer<HostBrowserHasDocumentDelegation>(hostApi.BrowserHasDocument);
            m_BrowserGetFrameCountApi = Marshal.GetDelegateForFunctionPointer<HostBrowserGetFrameCountDelegation>(hostApi.BrowserGetFrameCount);
            m_BrowserGetFrameIdentifiersApi = Marshal.GetDelegateForFunctionPointer<HostBrowserGetFrameIdentifiersDelegation>(hostApi.BrowserGetFrameIdentifiers);
            m_BrowserGetFrameNamesApi = Marshal.GetDelegateForFunctionPointer<HostBrowserGetFrameNamesDelegation>(hostApi.BrowserGetFrameNames);
            m_BrowserGetMainFrameApi = Marshal.GetDelegateForFunctionPointer<HostBrowserGetMainFrameDelegation>(hostApi.BrowserGetMainFrame);
            m_BrowserGetFocusedFrameApi = Marshal.GetDelegateForFunctionPointer<HostBrowserGetFocusedFrameDelegation>(hostApi.BrowserGetFocusedFrame);
            m_BrowserGetFrameByIdentifierApi = Marshal.GetDelegateForFunctionPointer<HostBrowserGetFrameByIdentifierDelegation>(hostApi.BrowserGetFrameByIdentifier);
            m_BrowserGetFrameByNameApi = Marshal.GetDelegateForFunctionPointer<HostBrowserGetFrameByNameDelegation>(hostApi.BrowserGetFrameByName);
            m_BrowserReloadApi = Marshal.GetDelegateForFunctionPointer<HostBrowserReloadDelegation>(hostApi.BrowserReload);
            m_BrowserReloadIgnoreCacheApi = Marshal.GetDelegateForFunctionPointer<HostBrowserReloadIgnoreCacheDelegation>(hostApi.BrowserReloadIgnoreCache);
            m_BrowserStopLoadApi = Marshal.GetDelegateForFunctionPointer<HostBrowserStopLoadDelegation>(hostApi.BrowserStopLoad);
            m_BrowserCloseApi = Marshal.GetDelegateForFunctionPointer<HostBrowserCloseDelegation>(hostApi.BrowserClose);
            m_BrowserSetFocusApi = Marshal.GetDelegateForFunctionPointer<HostBrowserSetFocusDelegation>(hostApi.BrowserSetFocus);
            m_BrowserGetOpenerIdApi = Marshal.GetDelegateForFunctionPointer<HostBrowserGetOpenerIdDelegation>(hostApi.BrowserGetOpenerId);
            m_FrameGetUrlApi = Marshal.GetDelegateForFunctionPointer<HostFrameGetUrlDelegation>(hostApi.FrameGetUrl);
            m_FrameGetNameApi = Marshal.GetDelegateForFunctionPointer<HostFrameGetNameDelegation>(hostApi.FrameGetName);
            m_FrameGetIdentifierApi = Marshal.GetDelegateForFunctionPointer<HostFrameGetIdentifierDelegation>(hostApi.FrameGetIdentifier);
            m_FrameIsMainApi = Marshal.GetDelegateForFunctionPointer<HostFrameIsMainDelegation>(hostApi.FrameIsMain);
            m_FrameIsValidApi = Marshal.GetDelegateForFunctionPointer<HostFrameIsValidDelegation>(hostApi.FrameIsValid);
            m_FrameIsFocusedApi = Marshal.GetDelegateForFunctionPointer<HostFrameIsFocusedDelegation>(hostApi.FrameIsFocused);
            m_FrameGetParentApi = Marshal.GetDelegateForFunctionPointer<HostFrameGetParentDelegation>(hostApi.FrameGetParent);
            m_FrameGetBrowserApi = Marshal.GetDelegateForFunctionPointer<HostFrameGetBrowserDelegation>(hostApi.FrameGetBrowser);
            m_FrameLoadUrlApi = Marshal.GetDelegateForFunctionPointer<HostFrameLoadUrlDelegation>(hostApi.FrameLoadUrl);
            // CefRequest properties
            m_RequestIsReadOnlyApi = Marshal.GetDelegateForFunctionPointer<HostRequestIsReadOnlyDelegation>(hostApi.RequestIsReadOnly);
            m_RequestGetUrlApi = Marshal.GetDelegateForFunctionPointer<HostRequestGetUrlDelegation>(hostApi.RequestGetUrl);
            m_RequestGetMethodApi = Marshal.GetDelegateForFunctionPointer<HostRequestGetMethodDelegation>(hostApi.RequestGetMethod);
            m_RequestGetReferrerUrlApi = Marshal.GetDelegateForFunctionPointer<HostRequestGetReferrerUrlDelegation>(hostApi.RequestGetReferrerUrl);
            m_RequestGetReferrerPolicyApi = Marshal.GetDelegateForFunctionPointer<HostRequestGetReferrerPolicyDelegation>(hostApi.RequestGetReferrerPolicy);
            m_RequestGetHeaderMapApi = Marshal.GetDelegateForFunctionPointer<HostRequestGetHeaderMapDelegation>(hostApi.RequestGetHeaderMap);
            m_RequestGetHeaderByNameApi = Marshal.GetDelegateForFunctionPointer<HostRequestGetHeaderByNameDelegation>(hostApi.RequestGetHeaderByName);
            m_RequestGetFlagsApi = Marshal.GetDelegateForFunctionPointer<HostRequestGetFlagsDelegation>(hostApi.RequestGetFlags);
            m_RequestGetFirstPartyForCookiesApi = Marshal.GetDelegateForFunctionPointer<HostRequestGetFirstPartyForCookiesDelegation>(hostApi.RequestGetFirstPartyForCookies);
            m_RequestGetResourceTypeApi = Marshal.GetDelegateForFunctionPointer<HostRequestGetResourceTypeDelegation>(hostApi.RequestGetResourceType);
            m_RequestGetTransitionTypeApi = Marshal.GetDelegateForFunctionPointer<HostRequestGetTransitionTypeDelegation>(hostApi.RequestGetTransitionType);
            m_RequestGetIdentifierApi = Marshal.GetDelegateForFunctionPointer<HostRequestGetIdentifierDelegation>(hostApi.RequestGetIdentifier);
            // Heartbeat control
            m_SetHeartbeatIntervalApi = Marshal.GetDelegateForFunctionPointer<HostSetHeartbeatIntervalDelegation>(hostApi.SetHeartbeatInterval);
        }

        public void NativeLog(string msg)
        {
            bool isMainThread = Thread.CurrentThread.ManagedThreadId == Lib.MainThreadId;
            string txt = string.Format("thread:{0} {1}{2}: {3}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name, isMainThread ? "(main)" : string.Empty, msg);
            //Console.WriteLine(txt);
            var lines = txt.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) {
                if (isMainThread) {
                    NativeLogImpl(line);
                }
                else {
                    EnqueueNativeLog(line);
                }
            }
        }
        public void JavascriptLog(string msg)
        {
            bool isMainThread = Thread.CurrentThread.ManagedThreadId == Lib.MainThreadId;
            string txt = string.Format("thread:{0} {1}{2}: {3}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name, isMainThread ? "(main)" : string.Empty, msg);
            //Console.WriteLine(txt);
            var lines = txt.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) {
                if (isMainThread) {
                    JavascriptLogImpl(line);
                }
                else {
                    EnqueueJsLog(line);
                }
            }
        }
        public void SendCefMessage(string msg, string[] args, int cef_process_id)
        {
            if (m_SendCefMessageApi == null) {
                return;
            }
            IntPtr[] argPtrs = new IntPtr[args.Length];
            try {
                for (int i = 0; i < args.Length; i++) {
                    argPtrs[i] = Marshal.StringToCoTaskMemUTF8(args[i]);
                }
                GCHandle handle = GCHandle.Alloc(argPtrs, GCHandleType.Pinned);
                try {
                    m_SendCefMessageApi.Invoke(msg, handle.AddrOfPinnedObject(), args.Length, Browser, Frame, cef_process_id);
                }
                finally {
                    handle.Free();
                }
            }
            finally {
                foreach (var ptr in argPtrs) {
                    if (ptr != IntPtr.Zero) {
                        Marshal.FreeCoTaskMem(ptr);
                    }
                }
            }
        }
        public void SendCefMessageForDSL(string msg, IList<BoxedValue> args, int cef_process_id)
        {
            string[] strArgs = new string[args.Count];
            for (int i = 0; i < args.Count; i++) {
                strArgs[i] = args[i].AsString;
            }
            SendCefMessage(msg, strArgs, cef_process_id);
        }
        public void SendJavascriptCode(string code)
        {
            if (m_SendJavascriptCodeApi == null) {
                return;
            }
            bool isMainThread = Thread.CurrentThread.ManagedThreadId == Lib.MainThreadId;
            if (isMainThread) {
                m_SendJavascriptCodeApi.Invoke(code, Browser, Frame);
            }
            else {
                s_JavascriptCodeQueue.Enqueue(code);
            }
        }
        public void SendJavascriptCall(string func, string[] args)
        {
            if (m_SendJavascriptCallApi == null) {
                return;
            }
            bool isMainThread = Thread.CurrentThread.ManagedThreadId == Lib.MainThreadId;
            if (isMainThread) {
                IntPtr[] argPtrs = new IntPtr[args.Length];
                try {
                    for (int i = 0; i < args.Length; i++) {
                        argPtrs[i] = Marshal.StringToCoTaskMemUTF8(args[i]);
                    }
                    GCHandle handle = GCHandle.Alloc(argPtrs, GCHandleType.Pinned);
                    try {
                        m_SendJavascriptCallApi.Invoke(func, handle.AddrOfPinnedObject(), args.Length, Browser, Frame);
                    }
                    finally {
                        handle.Free();
                    }
                }
                finally {
                    foreach (var ptr in argPtrs) {
                        if (ptr != IntPtr.Zero) {
                            Marshal.FreeCoTaskMem(ptr);
                        }
                    }
                }
            }
            else {
                s_JavascriptFuncQueue.Enqueue(new Tuple<string, string[]>(func, args));
            }
        }
        public void SendJavascriptCallForDSL(string func, IList<BoxedValue> args)
        {
            string[] strArgs = new string[args.Count];
            for (int i = 0; i < args.Count; i++) {
                strArgs[i] = args[i].AsString;
            }
            SendJavascriptCall(func, strArgs);
        }
        public string CallJavascriptFuncInRenderer(string func, string[] args)
        {
            if (m_CallJavascriptFuncInRendererApi == null) {
                return "";
            }
            IntPtr[] argPtrs = new IntPtr[args.Length];
            IntPtr resultPtr = IntPtr.Zero;
            try {
                for (int i = 0; i < args.Length; i++) {
                    argPtrs[i] = Marshal.StringToCoTaskMemUTF8(args[i]);
                }
                GCHandle handle = GCHandle.Alloc(argPtrs, GCHandleType.Pinned);
                try {
                    resultPtr = m_CallJavascriptFuncInRendererApi.Invoke(func, handle.AddrOfPinnedObject(), args.Length, Browser, Frame);
                }
                finally {
                    handle.Free();
                }
            }
            finally {
                foreach (var ptr in argPtrs) {
                    if (ptr != IntPtr.Zero) {
                        Marshal.FreeCoTaskMem(ptr);
                    }
                }
            }

            if (resultPtr == IntPtr.Zero) {
                return "";
            }

            try {
                string result = Marshal.PtrToStringUTF8(resultPtr) ?? "";
                return result;
            }
            finally {
                // Free the native string
                if (m_FreeNativeStringApi != null) {
                    m_FreeNativeStringApi.Invoke(resultPtr);
                }
            }
        }
        public string CallJavascriptFuncInRendererForDSL(string func, IList<BoxedValue> args)
        {
            string[] strArgs = new string[args.Count];
            for (int i = 0; i < args.Count; i++) {
                strArgs[i] = args[i].AsString;
            }
            return CallJavascriptFuncInRenderer(func, strArgs);
        }

        public void ClearApiErrorInfoForDSL()
        {
            ApiErrorInfo.Clear();
        }
        public void AppendApiErrorInfoForDSL(string msg)
        {
            ApiErrorInfo.Append(msg);
        }
        public void AppendApiErrorInfoLineForDSL(string msg)
        {
            ApiErrorInfo.AppendLine(msg);
        }
        public void AppendApiErrorInfoFormatForDSL(string fmt, params object[] args)
        {
            if (args.Length == 0) {
                ApiErrorInfo.Append(fmt);
            }
            else {
                ApiErrorInfo.AppendFormat(fmt, args);
            }
        }
        public void AppendApiErrorInfoFormatLineForDSL(string fmt, params object[] args)
        {
            if (args.Length == 0) {
                ApiErrorInfo.AppendLine(fmt);
            }
            else {
                ApiErrorInfo.AppendFormat(fmt, args);
                ApiErrorInfo.AppendLine();
            }
        }
        public bool HasApiErrorInfoForDSL => ApiErrorInfo.Length > 0;
        public string GetApiErrorInfoForDSL() => ApiErrorInfo.ToString();

        internal static string LoadDslFunc(string func, string code, IList<string> paramNames, bool update)
        {
            if (Thread.CurrentThread.ManagedThreadId == Lib.MainThreadId) {
                return Lib.LoadFunc(func, code, paramNames, update);
            }
            else {
                return CefDotnetAppApi.LoadFunc(func, code, paramNames, update);
            }
        }
        internal static string CallDslFunc(string func, List<string> args)
        {
            var bvals = BatchScript.NewCalculatorValueList();
            foreach (var arg in args) {
                bvals.Add(arg);
            }
            var result = BatchScript.Call(func, bvals);
            BatchScript.RecycleCalculatorValueList(bvals);
            if (result.IsNullObject) {
                return "null";
            }
            else if (null != Lib.AgentPlugin) {
                return Lib.AgentPlugin.ResultToString(result);
            }
            else {
                return result.ToString();
            }
        }
        internal static void ClearApiErrorInfo()
        {
            ApiErrorInfo.Clear();
        }
        internal static void AppendApiErrorInfo(string msg)
        {
            ApiErrorInfo.Append(msg);
        }
        internal static void AppendApiErrorInfoLine(string msg)
        {
            ApiErrorInfo.AppendLine(msg);
        }
        internal static void AppendApiErrorInfoFormat(string fmt, params object[] args)
        {
            if (args.Length == 0)
                ApiErrorInfo.Append(fmt);
            else
                ApiErrorInfo.AppendFormat(fmt, args);
        }
        internal static void AppendApiErrorInfoFormatLine(string fmt, params object[] args)
        {
            if (args.Length == 0)
                ApiErrorInfo.AppendLine(fmt);
            else {
                ApiErrorInfo.AppendFormat(fmt, args);
                ApiErrorInfo.AppendLine();
            }
        }
        internal static string GetStringInLength(string str, int len, int beginOrEndOrBeginEnd)
        {
            if (!string.IsNullOrEmpty(str)) {
                if (str.Length <= len) {
                    return str;
                }
                switch (beginOrEndOrBeginEnd) {
                    case 1:
                        return "..." + str.Substring(str.Length - len, len);
                    case 2:
                        return str.Substring(0, len / 2) + "..." + str.Substring(str.Length - len / 2, len / 2);
                    case 0:
                    default:
                        return str.Substring(0, len) + "...";
                }
            }
            return string.Empty;
        }
        internal static string QuoteString(string? value)
        {
            if (value == null) value = string.Empty;
            // if numeric, no quotes needed
            if (double.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out _))
                return value;
            // wrap in double quotes, escape internal double quotes and backslashes
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }
        internal static string StripQuotes(string? s)
        {
            if (s == null) return string.Empty;
            if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"')
                return s.Substring(1, s.Length - 2);
            if (s.Length >= 2 && s[0] == '\'' && s[s.Length - 1] == '\'')
                return s.Substring(1, s.Length - 2);
            return s;
        }
        /// <summary>
        /// Strip all non-alphanumeric characters from a string to produce clean tokens for semantic search.
        /// Replaces any character that is not a Unicode letter or digit (including CJK punctuation) with a space.
        /// </summary>
        internal static string CleanStringData(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return pattern;
            // Replace any character that is not a Unicode letter (\p{L}) or digit (\p{N}) with space.
            // This covers ASCII punctuation, CJK punctuation, and all other non-word characters.
            string s = Regex.Replace(pattern, @"[^\p{L}\p{N}]", " ");
            return Regex.Replace(s, @" {2,}", " ").Trim();
        }
        internal static bool HasApiErrorInfo => ApiErrorInfo.Length > 0;
        internal static string GetApiErrorInfo() => ApiErrorInfo.ToString();

        internal static StringBuilder ApiErrorInfo
        {
            get {
                if (s_ApiErrorInfo == null) {
                    s_ApiErrorInfo = new StringBuilder();
                }
                return s_ApiErrorInfo!;
            }
        }
        internal static void SetContext(IntPtr browser, IntPtr frame)
        {
            s_Browser = browser;
            s_Frame = frame;
        }

        //INativeApi explicit interface implementation(delegates to static methods)
        string INativeApi.GetStringInLength(string str, int len, int beginOrEndOrBeginEnd) => GetStringInLength(str, len, beginOrEndOrBeginEnd);
        string INativeApi.QuoteString(string? value) => QuoteString(value);
        string INativeApi.StripQuotes(string? s) => StripQuotes(s);

        // IErrorReporter explicit interface implementation (delegates to static methods)
        void IErrorReporter.ClearApiErrorInfo() => ClearApiErrorInfo();
        void IErrorReporter.AppendApiErrorInfo(string msg) => AppendApiErrorInfo(msg);
        void IErrorReporter.AppendApiErrorInfoLine(string msg) => AppendApiErrorInfoLine(msg);
        void IErrorReporter.AppendApiErrorInfoFormat(string fmt, params object[] args) => AppendApiErrorInfoFormat(fmt, args);
        void IErrorReporter.AppendApiErrorInfoFormatLine(string fmt, params object[] args) => AppendApiErrorInfoFormatLine(fmt, args);
        bool IErrorReporter.HasApiErrorInfo => HasApiErrorInfo;
        string IErrorReporter.GetApiErrorInfo() => GetApiErrorInfo();

        // IDslEngine explicit interface implementation (delegates to static methods)
        string IDslEngine.LoadDslFunc(string func, string code, IList<string> paramNames, bool update) => LoadDslFunc(func, code, paramNames, update);
        string IDslEngine.CallDslFunc(string func, List<string> args) => CallDslFunc(func, args);
        string IDslEngine.ExecuteMetaDslScript(string script, out bool hasError) => CefDotnetAppApi.ExecuteMetaDslScript(script, out hasError);
        void IDslEngine.Register(string name, string doc, IExpressionFactory factory) => BatchCommand.BatchScript.Register(name, doc, factory);
        void IDslEngine.Register(string name, string doc, bool addToUserApiDoc, IExpressionFactory factory) => BatchCommand.BatchScript.Register(name, doc, addToUserApiDoc, factory);

        internal static nint Browser
        {
            get => s_Browser;
            set => s_Browser = value;
        }
        internal static nint Frame
        {
            get => s_Frame;
            set => s_Frame = value;
        }
        internal static int LastSourceProcessId { get => s_LastSourceProcessId; set => s_LastSourceProcessId = value; }

        internal bool CommandLineHasSwitch(IntPtr commandLine, string name)
        {
            if (commandLine == IntPtr.Zero || string.IsNullOrEmpty(name) || m_CommandLineHasSwitchApi == null) {
                return false;
            }
            return m_CommandLineHasSwitchApi(commandLine, name);
        }
        internal string CommandLineGetSwitchValue(IntPtr commandLine, string name)
        {
            if (commandLine == IntPtr.Zero || string.IsNullOrEmpty(name) || m_CommandLineGetSwitchValueApi == null) {
                return string.Empty;
            }
            IntPtr resultPtr = m_CommandLineGetSwitchValueApi(commandLine, name);
            if (resultPtr == IntPtr.Zero) {
                return string.Empty;
            }
            try {
                return Marshal.PtrToStringUTF8(resultPtr) ?? string.Empty;
            }
            finally {
                m_FreeNativeStringApi?.Invoke(resultPtr);
            }
        }
        internal void CommandLineAppendSwitch(IntPtr commandLine, string name)
        {
            if (commandLine == IntPtr.Zero || string.IsNullOrEmpty(name) || m_CommandLineAppendSwitchApi == null) {
                return;
            }
            m_CommandLineAppendSwitchApi(commandLine, name);
        }
        internal void CommandLineAppendSwitchWithValue(IntPtr commandLine, string name, string value)
        {
            if (commandLine == IntPtr.Zero || string.IsNullOrEmpty(name) || m_CommandLineAppendSwitchWithValueApi == null) {
                return;
            }
            m_CommandLineAppendSwitchWithValueApi(commandLine, name, value ?? string.Empty);
        }
        internal void CommandLineRemoveSwitch(IntPtr commandLine, string name)
        {
            if (commandLine == IntPtr.Zero || string.IsNullOrEmpty(name) || m_CommandLineRemoveSwitchApi == null) {
                return;
            }
            m_CommandLineRemoveSwitchApi(commandLine, name);
        }
        internal bool CommandLineIsValid(IntPtr commandLine)
        {
            if (commandLine == IntPtr.Zero || m_CommandLineIsValidApi == null) return false;
            return m_CommandLineIsValidApi(commandLine);
        }
        internal bool CommandLineIsReadOnly(IntPtr commandLine)
        {
            if (commandLine == IntPtr.Zero || m_CommandLineIsReadOnlyApi == null) return false;
            return m_CommandLineIsReadOnlyApi(commandLine);
        }
        internal bool CommandLineHasSwitches(IntPtr commandLine)
        {
            if (commandLine == IntPtr.Zero || m_CommandLineHasSwitchesApi == null) return false;
            return m_CommandLineHasSwitchesApi(commandLine);
        }
        internal bool CommandLineHasArguments(IntPtr commandLine)
        {
            if (commandLine == IntPtr.Zero || m_CommandLineHasArgumentsApi == null) return false;
            return m_CommandLineHasArgumentsApi(commandLine);
        }
        internal string CommandLineGetProgram(IntPtr commandLine)
        {
            if (commandLine == IntPtr.Zero || m_CommandLineGetProgramApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_CommandLineGetProgramApi(commandLine));
        }
        internal void CommandLineSetProgram(IntPtr commandLine, string program)
        {
            if (commandLine == IntPtr.Zero || string.IsNullOrEmpty(program) || m_CommandLineSetProgramApi == null) return;
            m_CommandLineSetProgramApi(commandLine, program);
        }
        internal string CommandLineGetCommandLineString(IntPtr commandLine)
        {
            if (commandLine == IntPtr.Zero || m_CommandLineGetCommandLineStringApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_CommandLineGetCommandLineStringApi(commandLine));
        }
        internal string CommandLineGetArgv(IntPtr commandLine)
        {
            if (commandLine == IntPtr.Zero || m_CommandLineGetArgvApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_CommandLineGetArgvApi(commandLine));
        }
        internal string CommandLineGetSwitches(IntPtr commandLine)
        {
            if (commandLine == IntPtr.Zero || m_CommandLineGetSwitchesApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_CommandLineGetSwitchesApi(commandLine));
        }
        internal string CommandLineGetArguments(IntPtr commandLine)
        {
            if (commandLine == IntPtr.Zero || m_CommandLineGetArgumentsApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_CommandLineGetArgumentsApi(commandLine));
        }
        internal void CommandLineAppendArgument(IntPtr commandLine, string argument)
        {
            if (commandLine == IntPtr.Zero || string.IsNullOrEmpty(argument) || m_CommandLineAppendArgumentApi == null) return;
            m_CommandLineAppendArgumentApi(commandLine, argument);
        }
        internal void CommandLinePrependWrapper(IntPtr commandLine, string wrapper)
        {
            if (commandLine == IntPtr.Zero || string.IsNullOrEmpty(wrapper) || m_CommandLinePrependWrapperApi == null) return;
            m_CommandLinePrependWrapperApi(commandLine, wrapper);
        }
        internal IntPtr CommandLineGetGlobal()
        {
            if (m_CommandLineGetGlobalApi == null) return IntPtr.Zero;
            return m_CommandLineGetGlobalApi();
        }
        /// <summary>
        /// Get a CommandLineProxy wrapping the global (read-only) CefCommandLine.
        /// Only valid after CefInitialize has completed.
        /// </summary>
        public CommandLineProxy? GetGlobalCommandLine()
        {
            IntPtr ptr = CommandLineGetGlobal();
            if (ptr == IntPtr.Zero) return null;
            return new CommandLineProxy(ptr, this);
        }

        // Helper: read a native string returned by C++ and free it
        private string ReadAndFreeNativeString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return string.Empty;
            try {
                return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
            }
            finally {
                m_FreeNativeStringApi?.Invoke(ptr);
            }
        }

        // --- Browser traversal ---
        public IntPtr GetBrowserById(int browserId)
        {
            if (m_GetBrowserByIdApi == null) return IntPtr.Zero;
            return m_GetBrowserByIdApi(browserId);
        }
        public bool BrowserIsValid(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserIsValidApi == null) return false;
            return m_BrowserIsValidApi(browser);
        }
        /// <summary>
        /// Get renderer process browser/frame pair by browser ID from C++ ref map.
        /// Returns (browser, frame) tuple. Both are IntPtr.Zero if not found.
        /// </summary>
        public (IntPtr browser, IntPtr frame) GetRendererBrowserFrameById(int browserId)
        {
            if (m_GetRendererBrowserFrameByIdApi == null) return (IntPtr.Zero, IntPtr.Zero);
            bool ok = m_GetRendererBrowserFrameByIdApi(browserId, out IntPtr browser, out IntPtr frame);
            if (!ok) return (IntPtr.Zero, IntPtr.Zero);
            return (browser, frame);
        }

        // --- Browser properties ---
        public int BrowserGetId(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserGetIdApi == null) return 0;
            return m_BrowserGetIdApi(browser);
        }
        public string BrowserGetUrl(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserGetUrlApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_BrowserGetUrlApi(browser));
        }
        public bool BrowserIsLoading(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserIsLoadingApi == null) return false;
            return m_BrowserIsLoadingApi(browser);
        }
        public bool BrowserIsPopup(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserIsPopupApi == null) return false;
            return m_BrowserIsPopupApi(browser);
        }
        public bool BrowserHasDocument(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserHasDocumentApi == null) return false;
            return m_BrowserHasDocumentApi(browser);
        }

        // --- Browser frame access ---
        public int BrowserGetFrameCount(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserGetFrameCountApi == null) return 0;
            return m_BrowserGetFrameCountApi(browser);
        }
        public string[] BrowserGetFrameIdentifiers(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserGetFrameIdentifiersApi == null) return Array.Empty<string>();
            string raw = ReadAndFreeNativeString(m_BrowserGetFrameIdentifiersApi(browser));
            if (string.IsNullOrEmpty(raw)) return Array.Empty<string>();
            return raw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        }
        public string[] BrowserGetFrameNames(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserGetFrameNamesApi == null) return Array.Empty<string>();
            string raw = ReadAndFreeNativeString(m_BrowserGetFrameNamesApi(browser));
            if (string.IsNullOrEmpty(raw)) return Array.Empty<string>();
            return raw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        }
        public IntPtr BrowserGetMainFrame(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserGetMainFrameApi == null) return IntPtr.Zero;
            return m_BrowserGetMainFrameApi(browser);
        }
        public IntPtr BrowserGetFocusedFrame(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserGetFocusedFrameApi == null) return IntPtr.Zero;
            return m_BrowserGetFocusedFrameApi(browser);
        }
        public IntPtr BrowserGetFrameByIdentifier(IntPtr browser, string identifier)
        {
            if (browser == IntPtr.Zero || m_BrowserGetFrameByIdentifierApi == null) return IntPtr.Zero;
            return m_BrowserGetFrameByIdentifierApi(browser, identifier);
        }
        public IntPtr BrowserGetFrameByName(IntPtr browser, string name)
        {
            if (browser == IntPtr.Zero || m_BrowserGetFrameByNameApi == null) return IntPtr.Zero;
            return m_BrowserGetFrameByNameApi(browser, name);
        }

        // --- Browser actions ---
        public void BrowserReload(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserReloadApi == null) return;
            m_BrowserReloadApi(browser);
        }
        public void BrowserReloadIgnoreCache(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserReloadIgnoreCacheApi == null) return;
            m_BrowserReloadIgnoreCacheApi(browser);
        }
        public void BrowserStopLoad(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserStopLoadApi == null) return;
            m_BrowserStopLoadApi(browser);
        }

        // --- Browser host actions ---
        public void BrowserClose(IntPtr browser, bool forceClose = false)
        {
            if (browser == IntPtr.Zero || m_BrowserCloseApi == null) return;
            m_BrowserCloseApi(browser, forceClose ? 1 : 0);
        }
        public void BrowserSetFocus(IntPtr browser, bool focus)
        {
            if (browser == IntPtr.Zero || m_BrowserSetFocusApi == null) return;
            m_BrowserSetFocusApi(browser, focus ? 1 : 0);
        }
        public int BrowserGetOpenerId(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserGetOpenerIdApi == null) return 0;
            return m_BrowserGetOpenerIdApi(browser);
        }

        // --- Frame properties ---
        public string FrameGetUrl(IntPtr frame)
        {
            if (frame == IntPtr.Zero || m_FrameGetUrlApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_FrameGetUrlApi(frame));
        }
        public string FrameGetName(IntPtr frame)
        {
            if (frame == IntPtr.Zero || m_FrameGetNameApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_FrameGetNameApi(frame));
        }
        public string FrameGetIdentifier(IntPtr frame)
        {
            if (frame == IntPtr.Zero || m_FrameGetIdentifierApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_FrameGetIdentifierApi(frame));
        }
        public bool FrameIsMain(IntPtr frame)
        {
            if (frame == IntPtr.Zero || m_FrameIsMainApi == null) return false;
            return m_FrameIsMainApi(frame);
        }
        public bool FrameIsValid(IntPtr frame)
        {
            if (frame == IntPtr.Zero || m_FrameIsValidApi == null) return false;
            return m_FrameIsValidApi(frame);
        }
        public bool FrameIsFocused(IntPtr frame)
        {
            if (frame == IntPtr.Zero || m_FrameIsFocusedApi == null) return false;
            return m_FrameIsFocusedApi(frame);
        }
        public IntPtr FrameGetParent(IntPtr frame)
        {
            if (frame == IntPtr.Zero || m_FrameGetParentApi == null) return IntPtr.Zero;
            return m_FrameGetParentApi(frame);
        }
        public IntPtr FrameGetBrowser(IntPtr frame)
        {
            if (frame == IntPtr.Zero || m_FrameGetBrowserApi == null) return IntPtr.Zero;
            return m_FrameGetBrowserApi(frame);
        }

        // --- Frame actions ---
        public void FrameLoadUrl(IntPtr frame, string url)
        {
            if (frame == IntPtr.Zero || string.IsNullOrEmpty(url) || m_FrameLoadUrlApi == null) return;
            m_FrameLoadUrlApi(frame, url);
        }

        // --- CefRequest properties ---
        public bool RequestIsReadOnly(IntPtr request)
        {
            if (request == IntPtr.Zero || m_RequestIsReadOnlyApi == null) return true;
            return m_RequestIsReadOnlyApi(request);
        }
        public string RequestGetUrl(IntPtr request)
        {
            if (request == IntPtr.Zero || m_RequestGetUrlApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_RequestGetUrlApi(request));
        }
        public string RequestGetMethod(IntPtr request)
        {
            if (request == IntPtr.Zero || m_RequestGetMethodApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_RequestGetMethodApi(request));
        }
        public string RequestGetReferrerUrl(IntPtr request)
        {
            if (request == IntPtr.Zero || m_RequestGetReferrerUrlApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_RequestGetReferrerUrlApi(request));
        }
        public int RequestGetReferrerPolicy(IntPtr request)
        {
            if (request == IntPtr.Zero || m_RequestGetReferrerPolicyApi == null) return 0;
            return m_RequestGetReferrerPolicyApi(request);
        }
        public string RequestGetHeaderMap(IntPtr request)
        {
            if (request == IntPtr.Zero || m_RequestGetHeaderMapApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_RequestGetHeaderMapApi(request));
        }
        public string RequestGetHeaderByName(IntPtr request, string name)
        {
            if (request == IntPtr.Zero || string.IsNullOrEmpty(name) || m_RequestGetHeaderByNameApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_RequestGetHeaderByNameApi(request, name));
        }
        public int RequestGetFlags(IntPtr request)
        {
            if (request == IntPtr.Zero || m_RequestGetFlagsApi == null) return 0;
            return m_RequestGetFlagsApi(request);
        }
        public string RequestGetFirstPartyForCookies(IntPtr request)
        {
            if (request == IntPtr.Zero || m_RequestGetFirstPartyForCookiesApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_RequestGetFirstPartyForCookiesApi(request));
        }
        public int RequestGetResourceType(IntPtr request)
        {
            if (request == IntPtr.Zero || m_RequestGetResourceTypeApi == null) return 0;
            return m_RequestGetResourceTypeApi(request);
        }
        public int RequestGetTransitionType(IntPtr request)
        {
            if (request == IntPtr.Zero || m_RequestGetTransitionTypeApi == null) return 0;
            return m_RequestGetTransitionTypeApi(request);
        }
        public ulong RequestGetIdentifier(IntPtr request)
        {
            if (request == IntPtr.Zero || m_RequestGetIdentifierApi == null) return 0;
            return m_RequestGetIdentifierApi(request);
        }
        public void SetHeartbeatInterval(int intervalMs)
        {
            m_SetHeartbeatIntervalApi?.Invoke(intervalMs);
        }

        public void EnqueueCefMessage(string msgName, string[] args)
        {
            s_CefMessageQueue.Enqueue(new Tuple<string, string[]>(msgName, args));
        }

        internal void HandleAllQueues(int maxNativeCount, int maxJsCount, int maxCodeCount, int maxFuncCount)
        {
            bool isMainThread = Thread.CurrentThread.ManagedThreadId == Lib.MainThreadId;
            if (!isMainThread) {
                return;
            }
            if (s_Browser == IntPtr.Zero || s_Frame == IntPtr.Zero) {
                Lib.NativeLogNoLock($"[csharp] Error HandleAllQueues, browser:{s_Browser} frame:{s_Frame}");
                return;
            }

            // Process native log queue
            int nativeCountdown = maxNativeCount;
            while (nativeCountdown > 0 && s_NativeLogQueue.TryDequeue(out var log)) {
                NativeLog(log);
                --nativeCountdown;
            }

            // Process js log queue
            int jsCountdown = maxJsCount;
            while (jsCountdown > 0 && s_JsLogQueue.TryDequeue(out var jslog)) {
                NativeLog(jslog);
                --jsCountdown;
            }

            // Process JavascriptCode queue
            if (m_SendJavascriptCodeApi != null) {
                int codeCountdown = maxCodeCount;
                while (codeCountdown > 0 && s_JavascriptCodeQueue.TryDequeue(out var code)) {
                    try {
                        SendJavascriptCode(code);
                        --codeCountdown;
                    }
                    catch (Exception ex) {
                        Lib.NativeLogNoLock($"[csharp] Error processing JavascriptCode queue: {ex.Message}");
                    }
                }
            }

            // Process JavascriptFunc queue
            if (m_SendJavascriptCallApi != null) {
                int funcCountdown = maxFuncCount;
                while (funcCountdown > 0 && s_JavascriptFuncQueue.TryDequeue(out var funcItem)) {
                    try {
                        SendJavascriptCall(funcItem.Item1, funcItem.Item2);
                        --funcCountdown;
                    }
                    catch (Exception ex) {
                        Lib.NativeLogNoLock($"[csharp] Error processing JavascriptFunc queue: {ex.Message}");
                    }
                }
            }

            // Process unified CefMessage callback queue
            if (m_SendCefMessageApi != null) {
                while (s_CefMessageQueue.TryDequeue(out var cefItem)) {
                    try {
                        SendCefMessage(cefItem.Item1, cefItem.Item2, 0);
                    }
                    catch (Exception ex) {
                        Lib.NativeLogNoLock($"[csharp] Error processing CefMessage queue ({cefItem.Item1}): {ex.Message}");
                    }
                }
            }
        }

        private void EnqueueNativeLog(string log)
        {
            s_NativeLogQueue.Enqueue(log);
        }
        private void EnqueueJsLog(string log)
        {
            s_JsLogQueue.Enqueue(log);
        }
        private void NativeLogImpl(string msg)
        {
            if (m_NativeLogApi == null) {
                return;
            }
            m_NativeLogApi.Invoke(msg, Browser, Frame);
        }
        private void JavascriptLogImpl(string msg)
        {
            SendJavascriptCall("console.log", new string[] { msg });
        }

        private HostNativeLogDelegation? m_NativeLogApi;
        private HostSendCefMessageDelegation? m_SendCefMessageApi;
        private HostSendJavascriptCodeDelegation? m_SendJavascriptCodeApi;
        private HostSendJavascriptCallDelegation? m_SendJavascriptCallApi;
        private HostCallJavascriptFuncInRendererDelegation? m_CallJavascriptFuncInRendererApi;
        private HostFreeNativeStringDelegation? m_FreeNativeStringApi;
        private HostCommandLineHasSwitchDelegation? m_CommandLineHasSwitchApi;
        private HostCommandLineGetSwitchValueDelegation? m_CommandLineGetSwitchValueApi;
        private HostCommandLineAppendSwitchDelegation? m_CommandLineAppendSwitchApi;
        private HostCommandLineAppendSwitchWithValueDelegation? m_CommandLineAppendSwitchWithValueApi;
        private HostCommandLineRemoveSwitchDelegation? m_CommandLineRemoveSwitchApi;
        // CommandLine extended
        private HostCommandLineIsValidDelegation? m_CommandLineIsValidApi;
        private HostCommandLineIsReadOnlyDelegation? m_CommandLineIsReadOnlyApi;
        private HostCommandLineHasSwitchesDelegation? m_CommandLineHasSwitchesApi;
        private HostCommandLineHasArgumentsDelegation? m_CommandLineHasArgumentsApi;
        private HostCommandLineGetProgramDelegation? m_CommandLineGetProgramApi;
        private HostCommandLineSetProgramDelegation? m_CommandLineSetProgramApi;
        private HostCommandLineGetCommandLineStringDelegation? m_CommandLineGetCommandLineStringApi;
        private HostCommandLineGetArgvDelegation? m_CommandLineGetArgvApi;
        private HostCommandLineGetSwitchesDelegation? m_CommandLineGetSwitchesApi;
        private HostCommandLineGetArgumentsDelegation? m_CommandLineGetArgumentsApi;
        private HostCommandLineAppendArgumentDelegation? m_CommandLineAppendArgumentApi;
        private HostCommandLinePrependWrapperDelegation? m_CommandLinePrependWrapperApi;
        private HostCommandLineGetGlobalDelegation? m_CommandLineGetGlobalApi;
        // Browser traversal
        private HostGetBrowserByIdDelegation? m_GetBrowserByIdApi;
        private HostBrowserIsValidDelegation? m_BrowserIsValidApi;
        private HostGetRendererBrowserFrameByIdDelegation? m_GetRendererBrowserFrameByIdApi;
        // Browser properties
        private HostBrowserGetIdDelegation? m_BrowserGetIdApi;
        private HostBrowserGetUrlDelegation? m_BrowserGetUrlApi;
        private HostBrowserIsLoadingDelegation? m_BrowserIsLoadingApi;
        private HostBrowserIsPopupDelegation? m_BrowserIsPopupApi;
        private HostBrowserHasDocumentDelegation? m_BrowserHasDocumentApi;
        // Browser frame access
        private HostBrowserGetFrameCountDelegation? m_BrowserGetFrameCountApi;
        private HostBrowserGetFrameIdentifiersDelegation? m_BrowserGetFrameIdentifiersApi;
        private HostBrowserGetFrameNamesDelegation? m_BrowserGetFrameNamesApi;
        private HostBrowserGetMainFrameDelegation? m_BrowserGetMainFrameApi;
        private HostBrowserGetFocusedFrameDelegation? m_BrowserGetFocusedFrameApi;
        private HostBrowserGetFrameByIdentifierDelegation? m_BrowserGetFrameByIdentifierApi;
        private HostBrowserGetFrameByNameDelegation? m_BrowserGetFrameByNameApi;
        // Browser actions
        private HostBrowserReloadDelegation? m_BrowserReloadApi;
        private HostBrowserReloadIgnoreCacheDelegation? m_BrowserReloadIgnoreCacheApi;
        private HostBrowserStopLoadDelegation? m_BrowserStopLoadApi;
        // Browser host actions
        private HostBrowserCloseDelegation? m_BrowserCloseApi;
        private HostBrowserSetFocusDelegation? m_BrowserSetFocusApi;
        private HostBrowserGetOpenerIdDelegation? m_BrowserGetOpenerIdApi;
        // Frame properties
        private HostFrameGetUrlDelegation? m_FrameGetUrlApi;
        private HostFrameGetNameDelegation? m_FrameGetNameApi;
        private HostFrameGetIdentifierDelegation? m_FrameGetIdentifierApi;
        private HostFrameIsMainDelegation? m_FrameIsMainApi;
        private HostFrameIsValidDelegation? m_FrameIsValidApi;
        private HostFrameIsFocusedDelegation? m_FrameIsFocusedApi;
        private HostFrameGetParentDelegation? m_FrameGetParentApi;
        private HostFrameGetBrowserDelegation? m_FrameGetBrowserApi;
        // Frame actions
        private HostFrameLoadUrlDelegation? m_FrameLoadUrlApi;
        // CefRequest properties
        private HostRequestIsReadOnlyDelegation? m_RequestIsReadOnlyApi;
        private HostRequestGetUrlDelegation? m_RequestGetUrlApi;
        private HostRequestGetMethodDelegation? m_RequestGetMethodApi;
        private HostRequestGetReferrerUrlDelegation? m_RequestGetReferrerUrlApi;
        private HostRequestGetReferrerPolicyDelegation? m_RequestGetReferrerPolicyApi;
        private HostRequestGetHeaderMapDelegation? m_RequestGetHeaderMapApi;
        private HostRequestGetHeaderByNameDelegation? m_RequestGetHeaderByNameApi;
        private HostRequestGetFlagsDelegation? m_RequestGetFlagsApi;
        private HostRequestGetFirstPartyForCookiesDelegation? m_RequestGetFirstPartyForCookiesApi;
        private HostRequestGetResourceTypeDelegation? m_RequestGetResourceTypeApi;
        private HostRequestGetTransitionTypeDelegation? m_RequestGetTransitionTypeApi;
        private HostRequestGetIdentifierDelegation? m_RequestGetIdentifierApi;
        private HostSetHeartbeatIntervalDelegation? m_SetHeartbeatIntervalApi;

        [ThreadStatic]
        private static IntPtr s_Browser = IntPtr.Zero;
        [ThreadStatic]
        private static IntPtr s_Frame = IntPtr.Zero;
        [ThreadStatic]
        private static int s_LastSourceProcessId = -1;
        [ThreadStatic]
        private static StringBuilder? s_ApiErrorInfo = null;

        private static System.Collections.Concurrent.ConcurrentQueue<string> s_NativeLogQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();
        private static System.Collections.Concurrent.ConcurrentQueue<string> s_JsLogQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();
        private static System.Collections.Concurrent.ConcurrentQueue<string> s_JavascriptCodeQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();
        private static System.Collections.Concurrent.ConcurrentQueue<Tuple<string, string[]>> s_JavascriptFuncQueue = new System.Collections.Concurrent.ConcurrentQueue<Tuple<string, string[]>>();
        // Unified CefMessage callback queue: (msgName, args)
        private static System.Collections.Concurrent.ConcurrentQueue<Tuple<string, string[]>> s_CefMessageQueue = new System.Collections.Concurrent.ConcurrentQueue<Tuple<string, string[]>>();

        private const int c_max_path_length = 1024;
        private const int c_max_info_length = 4096;
    }

    /// <summary>
    /// Proxy wrapper for a native CefBrowser pointer.
    /// The pointer is valid only within the current call stack; do not store long-term.
    /// </summary>
    public class BrowserProxy
    {
        private readonly IntPtr m_Browser;
        private readonly NativeApi m_Api;

        public BrowserProxy(IntPtr browser, NativeApi api)
        {
            m_Browser = browser;
            m_Api = api;
        }

        public IntPtr NativePtr => m_Browser;
        public bool IsValid => m_Browser != IntPtr.Zero && m_Api.BrowserIsValid(m_Browser);

        public int Id => m_Api.BrowserGetId(m_Browser);
        public string Url => m_Api.BrowserGetUrl(m_Browser);
        public bool IsLoading => m_Api.BrowserIsLoading(m_Browser);
        public bool IsPopup => m_Api.BrowserIsPopup(m_Browser);
        public bool HasDocument => m_Api.BrowserHasDocument(m_Browser);
        public int FrameCount => m_Api.BrowserGetFrameCount(m_Browser);
        public int OpenerId => m_Api.BrowserGetOpenerId(m_Browser);

        public string[] GetFrameIdentifiers() => m_Api.BrowserGetFrameIdentifiers(m_Browser);
        public string[] GetFrameNames() => m_Api.BrowserGetFrameNames(m_Browser);

        public FrameProxy? GetMainFrame()
        {
            var ptr = m_Api.BrowserGetMainFrame(m_Browser);
            return ptr != IntPtr.Zero ? new FrameProxy(ptr, m_Api) : null;
        }
        public FrameProxy? GetFocusedFrame()
        {
            var ptr = m_Api.BrowserGetFocusedFrame(m_Browser);
            return ptr != IntPtr.Zero ? new FrameProxy(ptr, m_Api) : null;
        }
        public FrameProxy? GetFrameByIdentifier(string identifier)
        {
            var ptr = m_Api.BrowserGetFrameByIdentifier(m_Browser, identifier);
            return ptr != IntPtr.Zero ? new FrameProxy(ptr, m_Api) : null;
        }
        public FrameProxy? GetFrameByName(string name)
        {
            var ptr = m_Api.BrowserGetFrameByName(m_Browser, name);
            return ptr != IntPtr.Zero ? new FrameProxy(ptr, m_Api) : null;
        }

        public void Reload() => m_Api.BrowserReload(m_Browser);
        public void ReloadIgnoreCache() => m_Api.BrowserReloadIgnoreCache(m_Browser);
        public void StopLoad() => m_Api.BrowserStopLoad(m_Browser);
        public void Close(bool forceClose = false) => m_Api.BrowserClose(m_Browser, forceClose);
        public void SetFocus(bool focus) => m_Api.BrowserSetFocus(m_Browser, focus);
    }

    /// <summary>
    /// Proxy wrapper for a native CefFrame pointer.
    /// The pointer is valid only within the current call stack; do not store long-term.
    /// </summary>
    public class FrameProxy
    {
        private readonly IntPtr m_Frame;
        private readonly NativeApi m_Api;

        public FrameProxy(IntPtr frame, NativeApi api)
        {
            m_Frame = frame;
            m_Api = api;
        }

        public IntPtr NativePtr => m_Frame;
        public bool IsValid => m_Frame != IntPtr.Zero && m_Api.FrameIsValid(m_Frame);
        public bool IsMain => m_Api.FrameIsMain(m_Frame);
        public bool IsFocused => m_Api.FrameIsFocused(m_Frame);
        public string Url => m_Api.FrameGetUrl(m_Frame);
        public string Name => m_Api.FrameGetName(m_Frame);
        public string Identifier => m_Api.FrameGetIdentifier(m_Frame);

        public FrameProxy? GetParent()
        {
            var ptr = m_Api.FrameGetParent(m_Frame);
            return ptr != IntPtr.Zero ? new FrameProxy(ptr, m_Api) : null;
        }
        public BrowserProxy? GetBrowser()
        {
            var ptr = m_Api.FrameGetBrowser(m_Frame);
            return ptr != IntPtr.Zero ? new BrowserProxy(ptr, m_Api) : null;
        }

        public void LoadUrl(string url) => m_Api.FrameLoadUrl(m_Frame, url);
    }

    /// <summary>
    /// Proxy wrapper for a native CefRequest pointer.
    /// The pointer is valid only within the current call stack; do not store long-term.
    /// </summary>
    public class CefRequestProxy
    {
        private readonly IntPtr m_Request;
        private readonly NativeApi m_Api;

        public CefRequestProxy(IntPtr request, NativeApi api)
        {
            m_Request = request;
            m_Api = api;
        }

        public IntPtr NativePtr => m_Request;
        public bool IsReadOnly => m_Api.RequestIsReadOnly(m_Request);
        public string Url => m_Api.RequestGetUrl(m_Request);
        public string Method => m_Api.RequestGetMethod(m_Request);
        public string ReferrerUrl => m_Api.RequestGetReferrerUrl(m_Request);
        public int ReferrerPolicy => m_Api.RequestGetReferrerPolicy(m_Request);
        /// <summary>
        /// Get header map as newline-separated "key:value" pairs (split by first ':').
        /// </summary>
        public string HeaderMap => m_Api.RequestGetHeaderMap(m_Request);
        public string GetHeaderByName(string name) => m_Api.RequestGetHeaderByName(m_Request, name);
        public int Flags => m_Api.RequestGetFlags(m_Request);
        public string FirstPartyForCookies => m_Api.RequestGetFirstPartyForCookies(m_Request);
        public int ResourceType => m_Api.RequestGetResourceType(m_Request);
        public int TransitionType => m_Api.RequestGetTransitionType(m_Request);
        public ulong Identifier => m_Api.RequestGetIdentifier(m_Request);
    }

    /// <summary>
    /// Proxy wrapper for a native CefCommandLine pointer.
    /// Passed as parameter during OnBeforeCommandLineProcessing / OnBeforeChildProcessLaunch callbacks,
    /// or obtained via GetGlobalCommandLine() for the read-only global instance (after CefInitialize).
    /// </summary>
    public class CommandLineProxy
    {
        private readonly IntPtr m_CommandLine;
        private readonly NativeApi m_Api;

        public CommandLineProxy(IntPtr commandLine, NativeApi api)
        {
            m_CommandLine = commandLine;
            m_Api = api;
        }

        public IntPtr NativePtr => m_CommandLine;
        public bool IsValid => m_Api.CommandLineIsValid(m_CommandLine);
        public bool IsReadOnly => m_Api.CommandLineIsReadOnly(m_CommandLine);
        public bool HasSwitches => m_Api.CommandLineHasSwitches(m_CommandLine);
        public bool HasArguments => m_Api.CommandLineHasArguments(m_CommandLine);
        public string Program
        {
            get => m_Api.CommandLineGetProgram(m_CommandLine);
            set => m_Api.CommandLineSetProgram(m_CommandLine, value);
        }
        public string CommandLineString => m_Api.CommandLineGetCommandLineString(m_CommandLine);

        public bool HasSwitch(string name) => m_Api.CommandLineHasSwitch(m_CommandLine, name);
        public string GetSwitchValue(string name) => m_Api.CommandLineGetSwitchValue(m_CommandLine, name);
        public void AppendSwitch(string name) => m_Api.CommandLineAppendSwitch(m_CommandLine, name);
        public void AppendSwitchWithValue(string name, string value) => m_Api.CommandLineAppendSwitchWithValue(m_CommandLine, name, value);
        public void RemoveSwitch(string name) => m_Api.CommandLineRemoveSwitch(m_CommandLine, name);

        /// <summary>
        /// Get the original command line string as a newline-separated list.
        /// </summary>
        public string GetArgv() => m_Api.CommandLineGetArgv(m_CommandLine);
        /// <summary>
        /// Get all switches as key=value pairs separated by newline.
        /// </summary>
        public string GetSwitches() => m_Api.CommandLineGetSwitches(m_CommandLine);
        /// <summary>
        /// Get non-switch arguments as a newline-separated list.
        /// </summary>
        public string GetArguments() => m_Api.CommandLineGetArguments(m_CommandLine);
        public void AppendArgument(string argument) => m_Api.CommandLineAppendArgument(m_CommandLine, argument);
        public void PrependWrapper(string wrapper) => m_Api.CommandLinePrependWrapper(m_CommandLine, wrapper);
    }

    internal static class Lib
    {
        [UnmanagedCallersOnly]
        internal static int RegisterApi(IntPtr apis)
        {
            s_NativeApi = new NativeApi(apis);
            // Initialize the AgentFrameworkService singleton with concrete implementations
            AgentFrameworkService.Instance.SetNativeApi(s_NativeApi);
            AgentFrameworkService.Instance.SetErrorReporter(s_NativeApi);
            AgentFrameworkService.Instance.SetDslEngine(s_NativeApi);
            //We must load AgentCore's dependencies before loading AgentCore itself.
            PrepareBatchScript();
            return 0;
        }

        public delegate bool OnInitDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string cmd_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, int process_type, [MarshalAs(UnmanagedType.LPUTF8Str)] string app_dir, bool is_mac);
        public delegate void OnFinalizeDelegation();
        public delegate void OnBrowserInitDelegation(IntPtr browser);
        public delegate void OnBrowserFinalizeDelegation(IntPtr browser);
        public delegate bool OnBrowserHotReloadCopyFilesDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string url);
        public delegate void OnBrowserHotReloadCompletedDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int OnBrowserCefQueryDelegation(IntPtr browser, IntPtr frame, long query_id, [MarshalAs(UnmanagedType.LPUTF8Str)] string request, bool persistent);
        public delegate void OnRendererInitDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url);
        public delegate void OnRendererFinalizeDelegation(IntPtr browser, IntPtr frame);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnLoadingStateChangeDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, bool is_loading, bool can_go_back, bool can_go_forward);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnBeforeCommandLineProcessingDelegation(int process_type, IntPtr command_line);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnBeforeChildProcessLaunchDelegation(int process_type, IntPtr command_line);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool OnAlreadyRunningAppRelaunchDelegation(IntPtr command_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string current_directory);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnLoadErrorDelegation(IntPtr browser, IntPtr frame, int error_code, [MarshalAs(UnmanagedType.LPUTF8Str)] string error_text, [MarshalAs(UnmanagedType.LPUTF8Str)] string failed_url);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnRenderProcessTerminatedDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string startup_url, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, int status, int error_code, [MarshalAs(UnmanagedType.LPUTF8Str)] string error_string);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnLoadStartDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, int transition_type, bool is_main);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool OnLoadEndDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, int http_status_code, bool inject_all_frame, bool is_main, IntPtr js_code, ref int code_size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnRendererLoadStartDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, int transition_type, bool is_main);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool OnRendererLoadEndDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, int http_status_code, bool is_main, IntPtr js_code, ref int code_size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnRendererLoadingStateChangeDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, bool is_loading, bool can_go_back, bool can_go_forward);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnRendererLoadErrorDelegation(IntPtr browser, IntPtr frame, int error_code, [MarshalAs(UnmanagedType.LPUTF8Str)] string error_text, [MarshalAs(UnmanagedType.LPUTF8Str)] string failed_url);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnReceiveCefMessageDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, IntPtr args, int argCount, IntPtr browser, IntPtr frame, int source_process_id);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnReceiveJsMessageDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, IntPtr args, int argCount, IntPtr browser, IntPtr frame);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool OnExecuteMetaDSLDelegation(IntPtr args, int argCount, IntPtr resultStr, ref int resultSize, IntPtr browser, IntPtr frame);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool OnBeforeBrowseDelegation(IntPtr browser, IntPtr frame, IntPtr request, bool user_gesture, bool is_redirect, ref bool out_return_value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool OnBeforeResourceLoadDelegation(IntPtr browser, IntPtr frame, IntPtr request, ref int out_return_value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnHeartBeatDelegation(int process_type, float delta_time);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool OnCallMetaDSLDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func_name, IntPtr args, int argCount, IntPtr resultStr, ref int resultSize, IntPtr browser, IntPtr frame);

        internal static bool OnInit(string cmd_line, string path, int process_type, string app_dir, bool is_mac)
        {
            s_MainThreadId = Thread.CurrentThread.ManagedThreadId;
            AgentFrameworkService.Instance.SetMainThreadId(s_MainThreadId);
            NativeLogNoLock("[csharp] Init CommandLine: " + cmd_line);
            NativeLogNoLock("[csharp] Init BasePath: " + path);
            NativeLogNoLock("[csharp] Init AppDir: " + app_dir);
            NativeLogNoLock("[csharp] Init IsMac: " + is_mac);
            s_CmdLine = cmd_line;
            s_BasePath = path;
            s_AppDir = app_dir;
            s_IsMac = is_mac;
            s_ProcessType = process_type;
            Console.SetOut(s_StringWriter);
            Console.SetError(s_StringWriter);

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_init"));

                if (null != s_NativeApi) {
                    if ((int)CefProcessType.RendererProcess == process_type) {
                        // Before loading the DSL script, we must register all APIs.

                        var framework = AgentFrameworkService.Instance;
                        // Load AgentCore and hot reload manager in renderer process
                        bool loadSuccess = framework.LoadAgentPlugin(s_BasePath, s_AppDir, s_IsMac);
                        if (loadSuccess) {
                            NativeLogNoLock("[csharp] AgentPlugin loaded successfully");
                        }
                        else {
                            NativeLogNoLock("[csharp] Warning: AgentPlugin loading failed, agent features will not be available");
                        }
                    }

                    if (TryGetSwitchValueFromRawCommandLine(cmd_line, "metadsl", out string switchValue)) {
                        s_DslScriptFile = switchValue;
                        s_DslScriptFileChanged = true;
                        NativeLogNoLock(string.Format("[csharp] parse --metadsl:{0}, set DslScriptFile", s_DslScriptFile));
                    }
                    if (TryGetSwitchValueFromRawCommandLine(cmd_line, "projectidentity", out string prjIdentity)) {
                        s_InitialProjectIdentity = prjIdentity;
                        s_InitialProjectIdentityInited = true;
                        NativeLogNoLock(string.Format("[csharp] parse --projectidentity:{0}, set InitialProjectIdentity", s_InitialProjectIdentity));
                    }
                    TryLoadDSL();
                    BoxedValue r = BatchCommand.BatchScript.Call("on_init");
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                    return r.GetBool();
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
            return false;  // default: no_sandbox=false (sandbox enabled)
        }
        internal static void OnFinalize()
        {
            NativeLogNoLock("[csharp] Finalize");

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_finalize"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BoxedValue r = BatchCommand.BatchScript.Call("on_finalize");
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }

            AgentFrameworkService.Instance.ShutdownPlugin();

            NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
            NativeApi.LastSourceProcessId = -1;
        }

        internal static void OnBrowserInit(IntPtr browser)
        {
            s_MainThreadId = Thread.CurrentThread.ManagedThreadId;
            AgentFrameworkService.Instance.SetMainThreadId(s_MainThreadId);
            NativeApi.SetContext(browser, IntPtr.Zero);
            NativeLogNoLock("[csharp] Browser Init");

            // Track browser id in C# side
            if (s_NativeApi != null) {
                int browserId = s_NativeApi.BrowserGetId(browser);
                if (browserId > 0) {
                    s_BrowserBrowserIds.Add(browserId);
                    NativeLogNoLock($"[csharp] Browser tracked: id={browserId}");
                }
            }

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_browser_init"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BoxedValue r = BatchCommand.BatchScript.Call("on_browser_init");
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static void OnBrowserFinalize(IntPtr browser)
        {
            NativeApi.SetContext(browser, IntPtr.Zero);
            NativeLogNoLock("[csharp] Browser Finalize");

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_browser_finalize"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BoxedValue r = BatchCommand.BatchScript.Call("on_browser_finalize");
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }

            // Untrack browser id in C# side
            if (s_NativeApi != null) {
                int browserId = s_NativeApi.BrowserGetId(browser);
                if (browserId > 0) {
                    s_BrowserBrowserIds.Remove(browserId);
                    NativeLogNoLock($"[csharp] Browser untracked: id={browserId}");
                }
            }
            NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
            NativeApi.LastSourceProcessId = -1;
        }

        internal static bool OnBrowserHotReloadCopyFiles(string url)
        {
            NativeLogNoLock("[csharp] Browser Hot Reload Copy Files, url: " + url);

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_browser_hot_reload_copyfiles"));

                if (null != s_NativeApi) {
                    TryLoadDSL();
                    BoxedValue r = BatchCommand.BatchScript.Call("on_browser_hot_reload_copyfiles", url);
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                        return r.GetBool();
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
            return false;
        }

        internal static void OnBrowserHotReloadCompleted(IntPtr browser, IntPtr frame, string url)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock("[csharp] Browser Hot Reload Completed, url: " + url);

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_browser_hot_reload_completed"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BoxedValue r = BatchCommand.BatchScript.Call("on_browser_hot_reload_completed", url);
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static int OnBrowserCefQuery(IntPtr browser, IntPtr frame, long query_id, string request, bool persistent)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock(string.Format("[csharp] Browser Cef Query: query_id={0}, request={1}, persistent={2}", query_id, GetStringInLength(request), persistent));

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_browser_cef_query"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BoxedValue r = BatchCommand.BatchScript.Call("on_browser_cef_query", query_id, request, persistent);
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                        return r.GetInt();
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
            return -1;
        }

        internal static void OnRendererInit(IntPtr browser, IntPtr frame, string url)
        {
            s_MainThreadId = Thread.CurrentThread.ManagedThreadId;
            AgentFrameworkService.Instance.SetMainThreadId(s_MainThreadId);
            NativeApi.SetContext(browser, frame);
            s_StartupUrl = url;

            // Track main frame browser/frame pair for renderer process
            if (s_NativeApi != null && s_NativeApi.FrameIsMain(frame)) {
                int browserId = s_NativeApi.BrowserGetId(browser);
                if (browserId > 0) {
                    s_RendererBrowserFrames[browserId] = (browser, frame);
                    NativeLogNoLock($"[csharp] Renderer browser tracked: id={browserId}");
                }
            }

            NativeLogNoLock($"[csharp] Renderer Init, url={url}");

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_renderer_init"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.FromString(url));
                    BatchCommand.BatchScript.Call("on_renderer_init", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static void OnRendererFinalize(IntPtr browser, IntPtr frame)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock("[csharp] Renderer Finalize");

            // Untrack main frame browser/frame pair for renderer process
            // Only remove if the frame pointer matches (avoid removing a newer frame after navigation)
            if (s_NativeApi != null) {
                int browserId = s_NativeApi.BrowserGetId(browser);
                if (browserId > 0 && s_RendererBrowserFrames.TryGetValue(browserId, out var existing) && existing.frame == frame) {
                    s_RendererBrowserFrames.Remove(browserId);
                    NativeLogNoLock($"[csharp] Renderer browser untracked: id={browserId}");
                }
            }

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_renderer_finalize"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BoxedValue r = BatchCommand.BatchScript.Call("on_renderer_finalize");
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }

            NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
            NativeApi.LastSourceProcessId = -1;
        }

        internal static void OnLoadStart(IntPtr browser, IntPtr frame, string url, int transition_type, bool is_main)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock($"[csharp] OnLoadStart: url={url}, transition_type={transition_type}, is_main={is_main}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.FromString(url));
                    vargs.Add(BoxedValue.From(transition_type));
                    vargs.Add(BoxedValue.FromBool(is_main));
                    BatchCommand.BatchScript.Call("on_load_start", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnLoadStart:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static bool OnLoadEnd(IntPtr browser, IntPtr frame, string url, int http_status_code, bool inject_all_frame, bool is_main, IntPtr js_code, ref int code_size)
        {
            NativeApi.SetContext(browser, frame);
            s_LoadedUrl = url;
            NativeLogNoLock($"[csharp] OnLoadEnd: url={url}, http_status_code={http_status_code}, inject_all_frame={inject_all_frame}, is_main={is_main}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.FromString(url));
                    vargs.Add(BoxedValue.From(http_status_code));
                    vargs.Add(BoxedValue.FromBool(inject_all_frame));
                    vargs.Add(BoxedValue.FromBool(is_main));
                    var r = BatchCommand.BatchScript.Call("on_load_end", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    if (!r.IsNullObject) {
                        NativeLogNoLock($"[csharp] on_load_end result type: {r.Type}");

                        if (r.Type == (int)BoxedValue.c_Tuple2Type) {
                            var tuple = r.GetTuple2();
                            if (null != tuple) {
                                bool useCustomCode = tuple.Item1.GetBool();
                                string jsCode = tuple.Item2.GetString();

                                NativeLogNoLock($"[csharp] on_load_end returned: useCustomCode={useCustomCode}, jsCode.Length={jsCode?.Length ?? 0}");

                                if (useCustomCode) {
                                    if (string.IsNullOrEmpty(jsCode)) {
                                        code_size = 0;
                                        return true;
                                    }
                                    else {
                                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsCode);
                                        if (bytes.Length < code_size) {
                                            Marshal.Copy(bytes, 0, js_code, bytes.Length);
                                            code_size = bytes.Length;
                                            return true;
                                        }
                                        else {
                                            NativeLogNoLock($"[csharp] JS code too large: {bytes.Length} >= {code_size}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnLoadEnd:" + e.Message + "\n" + e.StackTrace);
            }

            return false;
        }

        internal static void OnLoadingStateChange(IntPtr browser, IntPtr frame, string url, bool is_loading, bool can_go_back, bool can_go_forward)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock($"[csharp] OnLoadingStateChange: url={url}, is_loading={is_loading}, can_go_back={can_go_back}, can_go_forward={can_go_forward}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.FromString(url ?? ""));
                    vargs.Add(BoxedValue.From(is_loading));
                    vargs.Add(BoxedValue.From(can_go_back));
                    vargs.Add(BoxedValue.From(can_go_forward));
                    BatchCommand.BatchScript.Call("on_loading_state_change", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnLoadingStateChange:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static void OnLoadError(IntPtr browser, IntPtr frame, int error_code, string error_text, string failed_url)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock($"[csharp] OnLoadError: error_code={error_code}, error_text={error_text}, failed_url={failed_url}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(error_code));
                    vargs.Add(BoxedValue.FromString(error_text));
                    vargs.Add(BoxedValue.FromString(failed_url));
                    BatchCommand.BatchScript.Call("on_load_error", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnLoadError:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static void OnRendererLoadStart(IntPtr browser, IntPtr frame, string url, int transition_type, bool is_main)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock($"[csharp] OnRendererLoadStart: url={url}, transition_type={transition_type}, is_main={is_main}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.FromString(url));
                    vargs.Add(BoxedValue.From(transition_type));
                    vargs.Add(BoxedValue.FromBool(is_main));
                    BatchCommand.BatchScript.Call("on_renderer_load_start", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnRendererLoadStart:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static bool OnRendererLoadEnd(IntPtr browser, IntPtr frame, string url, int http_status_code, bool is_main, IntPtr js_code, ref int code_size)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock($"[csharp] OnRendererLoadEnd: url={url}, http_status_code={http_status_code}, is_main={is_main}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.FromString(url));
                    vargs.Add(BoxedValue.From(http_status_code));
                    vargs.Add(BoxedValue.FromBool(is_main));
                    var r = BatchCommand.BatchScript.Call("on_renderer_load_end", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    if (!r.IsNullObject) {
                        NativeLogNoLock($"[csharp] on_renderer_load_end result type: {r.Type}");

                        if (r.Type == (int)BoxedValue.c_Tuple2Type) {
                            var tuple = r.GetTuple2();
                            if (null != tuple) {
                                bool useCustomCode = tuple.Item1.GetBool();
                                string jsCode = tuple.Item2.GetString();

                                NativeLogNoLock($"[csharp] on_renderer_load_end returned: useCustomCode={useCustomCode}, jsCode.Length={jsCode?.Length ?? 0}");

                                if (useCustomCode) {
                                    if (string.IsNullOrEmpty(jsCode)) {
                                        code_size = 0;
                                        return true;
                                    }
                                    else {
                                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsCode);
                                        if (bytes.Length < code_size) {
                                            Marshal.Copy(bytes, 0, js_code, bytes.Length);
                                            code_size = bytes.Length;
                                            return true;
                                        }
                                        else {
                                            NativeLogNoLock($"[csharp] Renderer JS code too large: {bytes.Length} >= {code_size}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnRendererLoadEnd:" + e.Message + "\n" + e.StackTrace);
            }

            return false;
        }

        internal static void OnRendererLoadingStateChange(IntPtr browser, IntPtr frame, string url, bool is_loading, bool can_go_back, bool can_go_forward)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock($"[csharp] OnRendererLoadingStateChange: url={url}, is_loading={is_loading}, can_go_back={can_go_back}, can_go_forward={can_go_forward}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.FromString(url ?? ""));
                    vargs.Add(BoxedValue.From(is_loading));
                    vargs.Add(BoxedValue.From(can_go_back));
                    vargs.Add(BoxedValue.From(can_go_forward));
                    BatchCommand.BatchScript.Call("on_renderer_loading_state_change", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnRendererLoadingStateChange:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static void OnRendererLoadError(IntPtr browser, IntPtr frame, int error_code, string error_text, string failed_url)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock($"[csharp] OnRendererLoadError: error_code={error_code}, error_text={error_text}, failed_url={failed_url}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(error_code));
                    vargs.Add(BoxedValue.FromString(error_text));
                    vargs.Add(BoxedValue.FromString(failed_url));
                    BatchCommand.BatchScript.Call("on_renderer_load_error", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnRendererLoadError:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static void OnRenderProcessTerminated(IntPtr browser, IntPtr frame, string startup_url, string url, int status, int error_code, string error_string)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock($"[csharp] OnRenderProcessTerminated: startup_url={startup_url}, url={url}, status={status}, error_code={error_code}, error_string={error_string}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.FromString(startup_url));
                    vargs.Add(BoxedValue.FromString(url));
                    vargs.Add(BoxedValue.From(status));
                    vargs.Add(BoxedValue.From(error_code));
                    vargs.Add(BoxedValue.FromString(error_string));
                    BatchCommand.BatchScript.Call("on_render_process_terminated", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnRenderProcessTerminated:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static void OnBeforeCommandLineProcessing(int process_type, IntPtr command_line)
        {
            NativeLogNoLock($"[csharp] OnBeforeCommandLineProcessing: process_type={process_type}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var cmdLineProxy = new CommandLineProxy(command_line, s_NativeApi);
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(process_type));
                    vargs.Add(BoxedValue.From(cmdLineProxy));
                    BatchCommand.BatchScript.Call("on_before_command_line_processing", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnBeforeCommandLineProcessing:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static void OnBeforeChildProcessLaunch(int process_type, IntPtr command_line)
        {
            NativeLogNoLock($"[csharp] OnBeforeChildProcessLaunch process_type={process_type}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var cmdLineProxy = new CommandLineProxy(command_line, s_NativeApi);
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(process_type));
                    vargs.Add(BoxedValue.From(cmdLineProxy));
                    BatchCommand.BatchScript.Call("on_before_child_process_launch", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnBeforeChildProcessLaunch:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static bool OnAlreadyRunningAppRelaunch(IntPtr command_line, string current_directory)
        {
            NativeLogNoLock($"[csharp] OnAlreadyRunningAppRelaunch current_directory={current_directory}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var cmdLineProxy = new CommandLineProxy(command_line, s_NativeApi);
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(cmdLineProxy));
                    vargs.Add(BoxedValue.FromString(current_directory ?? ""));
                    var r = BatchCommand.BatchScript.Call("on_already_running_app_relaunch", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    return r.GetBool();
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnAlreadyRunningAppRelaunch:" + e.Message + "\n" + e.StackTrace);
            }
            return false;
        }

        internal static bool OnBeforeBrowse(IntPtr browser, IntPtr frame, IntPtr request, bool user_gesture, bool is_redirect, ref bool out_return_value)
        {
            NativeApi.SetContext(browser, frame);

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var requestProxy = new CefRequestProxy(request, s_NativeApi);
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(requestProxy));
                    vargs.Add(BoxedValue.From(user_gesture));
                    vargs.Add(BoxedValue.From(is_redirect));
                    var r = BatchCommand.BatchScript.Call("on_before_browse", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    // Return value convention: (handled, return_value)
                    // If handled is true, out_return_value is set and we return true
                    if (!r.IsNullObject) {
                        var list = r.GetObject() as IList<BoxedValue>;
                        if (list != null && list.Count >= 2) {
                            bool handled = list[0].GetBool();
                            if (handled) {
                                out_return_value = list[1].GetBool();
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnBeforeBrowse:" + e.Message + "\n" + e.StackTrace);
            }
            return false;
        }

        internal static bool OnBeforeResourceLoad(IntPtr browser, IntPtr frame, IntPtr request, ref int out_return_value)
        {
            NativeApi.SetContext(browser, frame);

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var requestProxy = new CefRequestProxy(request, s_NativeApi);
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(requestProxy));
                    var r = BatchCommand.BatchScript.Call("on_before_resource_load", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    // Return value convention: (handled, return_value_int)
                    // If handled is true, out_return_value is set and we return true
                    if (!r.IsNullObject) {
                        var list = r.GetObject() as IList<BoxedValue>;
                        if (list != null && list.Count >= 2) {
                            bool handled = list[0].GetBool();
                            if (handled) {
                                out_return_value = list[1].GetInt();
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnBeforeResourceLoad:" + e.Message + "\n" + e.StackTrace);
            }
            return false;
        }

        internal static void OnHeartBeat(int process_type, float delta_time)
        {
            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(process_type));
                    vargs.Add(BoxedValue.From(delta_time));
                    BatchCommand.BatchScript.Call("on_heart_beat", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnHeartBeat:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static bool OnCallMetaDSL(string func_name, IntPtr args, int argCount, IntPtr resultStr, ref int resultSize, IntPtr browser, IntPtr frame)
        {
            string[] argArray = new string[argCount];
            for (int i = 0; i < argCount; i++) {
                IntPtr strPtr = Marshal.ReadIntPtr(args, i * IntPtr.Size);
                argArray[i] = Marshal.PtrToStringUTF8(strPtr) ?? string.Empty;
            }

            string result = OnCallMetaDSL(func_name, new List<string>(argArray), browser, frame);
            if (string.IsNullOrEmpty(result)) {
                resultSize = 0;
                return false;
            }

            byte[] resultBytes = System.Text.Encoding.UTF8.GetBytes(result);
            if (resultSize < resultBytes.Length + 1) {
                resultSize = resultBytes.Length + 1;
                return false;
            }

            Marshal.Copy(resultBytes, 0, resultStr, resultBytes.Length);
            Marshal.WriteByte(resultStr, resultBytes.Length, 0);
            resultSize = resultBytes.Length;
            return true;
        }

        internal static string OnCallMetaDSL(string func_name, List<string> args, IntPtr browser, IntPtr frame)
        {
            lock (s_Lock) {
                NativeApi.SetContext(browser, frame);

                try {
                    if (null != s_NativeApi) {
                        TryLoadDSL();

                        var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                        foreach (var arg in args) {
                            vargs.Add(BoxedValue.FromString(arg));
                        }
                        var r = BatchCommand.BatchScript.Call(func_name, vargs);
                        BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                        if (!r.IsNullObject) {
                            return r.ToString();
                        }
                    }
                }
                catch (Exception e) {
                    NativeLogNoLock("[csharp] Exception in OnCallMetaDSL:" + e.Message + "\n" + e.StackTrace);
                }
            }
            return string.Empty;
        }

        internal static void OnReceiveCefMessage(string msg, IntPtr args, int argCount, IntPtr browser, IntPtr frame, int source_process_id)
        {
            string[] argArray = new string[argCount];
            for (int i = 0; i < argCount; i++) {
                IntPtr strPtr = Marshal.ReadIntPtr(args, i * IntPtr.Size);
                argArray[i] = Marshal.PtrToStringUTF8(strPtr) ?? string.Empty;
            }
            OnReceiveCefMessage(msg, new List<string>(argArray), browser, frame, source_process_id);
        }

        internal static void OnReceiveJsMessage(string msg, IntPtr args, int argCount, IntPtr browser, IntPtr frame)
        {
            string[] argArray = new string[argCount];
            for (int i = 0; i < argCount; i++) {
                IntPtr strPtr = Marshal.ReadIntPtr(args, i * IntPtr.Size);
                argArray[i] = Marshal.PtrToStringUTF8(strPtr) ?? string.Empty;
            }
            OnReceiveJsMessage(msg, new List<string>(argArray), browser, frame);
        }

        internal static bool OnExecuteMetaDSL(IntPtr args, int argCount, IntPtr resultStr, ref int resultSize, IntPtr browser, IntPtr frame)
        {
            string[] argArray = new string[argCount];
            for (int i = 0; i < argCount; i++) {
                IntPtr strPtr = Marshal.ReadIntPtr(args, i * IntPtr.Size);
                argArray[i] = Marshal.PtrToStringUTF8(strPtr) ?? string.Empty;
            }

            string result = OnExecuteMetaDSL(new List<string>(argArray), browser, frame);
            if (string.IsNullOrEmpty(result)) {
                resultSize = 0;
                return false;
            }

            byte[] resultBytes = System.Text.Encoding.UTF8.GetBytes(result);
            if (resultSize < resultBytes.Length + 1) {
                resultSize = resultBytes.Length + 1;
                return false;
            }

            Marshal.Copy(resultBytes, 0, resultStr, resultBytes.Length);
            Marshal.WriteByte(resultStr, resultBytes.Length, 0);
            resultSize = resultBytes.Length;
            return true;
        }

        internal static string OnExecuteMetaDSL(List<string> args, IntPtr browser, IntPtr frame)
        {
            lock (s_Lock) {
                NativeApi.SetContext(browser, frame);

                try {
                    if (args.Count == 1) {
                        return ExecuteMetaDslScript(args[0]);
                    }
                    else {
                        var sb = new StringBuilder();
                        foreach (var arg in args) {
                            sb.Append(arg);
                            sb.Append(';');
                            sb.AppendLine();
                        }
                        return ExecuteMetaDslScript(sb.ToString());
                    }
                }
                catch (Exception e) {
                    NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
                }
            }
            return string.Empty;
        }

        internal static void OnReceiveCefMessage(string msg, List<string> args, IntPtr browser, IntPtr frame, int source_process_id)
        {
            lock (s_Lock) {
                NativeApi.SetContext(browser, frame);
                NativeApi.LastSourceProcessId = source_process_id;

                try {
                    NativeLogNoLock(string.Format("[csharp] Call csharp OnReceiveCefMessage, msg:{0} arg:{1} from process:{2} process type:{3}", msg, GetStringInLength(args), source_process_id, s_ProcessType));

                    if (null != s_NativeApi) {
                        TryLoadDSL();

                        var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                        vargs.Add(BoxedValue.From(source_process_id));
                        vargs.Add(BoxedValue.FromString(msg));
                        foreach (var arg in args) {
                            vargs.Add(BoxedValue.FromString(arg));
                        }
                        var r = BatchCommand.BatchScript.Call("on_receive_cef_message", vargs);
                        BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                        if (!r.IsNullObject) {
                            NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                        }
                    }
                }
                catch (Exception e) {
                    NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
                }
            }
        }

        internal static void OnReceiveJsMessage(string msg, List<string> args, IntPtr browser, IntPtr frame)
        {
            lock (s_Lock) {
                NativeApi.SetContext(browser, frame);

                try {
                    NativeLogNoLock(string.Format("[csharp] Call csharp OnReceiveJsMessage, msg:{0} arg:{1} process type:{2}", msg, GetStringInLength(args), s_ProcessType));

                    if (null != s_NativeApi) {
                        TryLoadDSL();

                        var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                        vargs.Add(BoxedValue.From(NativeApi.LastSourceProcessId));
                        vargs.Add(BoxedValue.FromString(msg));
                        foreach (var arg in args) {
                            vargs.Add(BoxedValue.FromString(arg));
                        }
                        var r = BatchCommand.BatchScript.Call("on_receive_js_message", vargs);
                        BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                        if (!r.IsNullObject) {
                            NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                        }
                    }
                }
                catch (Exception e) {
                    NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
                }
            }
        }

        internal static string CmdLine
        {
            get {
                return s_CmdLine;
            }
        }

        internal static string BasePath
        {
            get {
                return s_BasePath;
            }
        }

        internal static string AppDir
        {
            get {
                return s_AppDir;
            }
        }

        internal static bool IsMac
        {
            get {
                return s_IsMac;
            }
        }

        internal static IAgentPlugin? AgentPlugin
        {
            get {
                return AgentFrameworkService.Instance.AgentPlugin;
            }
        }

        internal static int MainThreadId
        {
            get {
                return s_MainThreadId;
            }
        }

        // Parse --metadsl=value from raw command line string
        internal static bool TryGetSwitchValueFromRawCommandLine(string cmdLine, string switchName, out string switchValue)
        {
            switchValue = string.Empty;
            string prefix = "--" + switchName + "=";
            int idx = cmdLine.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                return false;
            int start = idx + prefix.Length;
            // Handle quoted value
            if (start < cmdLine.Length && cmdLine[start] == '"') {
                int end = cmdLine.IndexOf('"', start + 1);
                switchValue = end > start ? cmdLine.Substring(start + 1, end - start - 1) : string.Empty;
                return true;
            }
            // Unquoted: take until next space
            int spaceIdx = cmdLine.IndexOf(' ', start);
            switchValue = spaceIdx > start ? cmdLine.Substring(start, spaceIdx - start) : cmdLine.Substring(start);
            return true;
        }
        internal static void NativeLog(string msg)
        {
            lock (s_Lock) {
                NativeLogNoLock(msg);
            }
        }
        internal static void JsLog(string msg)
        {
            lock (s_Lock) {
                JsLogNoLock(msg);
            }
        }
        internal static void HandleThreadQueue(int maxNativeCount, int maxJsCount, int maxCodeCount, int maxFuncCount)
        {
            bool isMainThread = Thread.CurrentThread.ManagedThreadId == s_MainThreadId;
            if (isMainThread && null != s_NativeApi) {
                s_NativeApi.HandleAllQueues(maxNativeCount, maxJsCount, maxCodeCount, maxFuncCount);
            }
        }
        internal static void SetHeartbeatInterval(int intervalMs)
        {
            s_NativeApi?.SetHeartbeatInterval(intervalMs);
        }
        /// <summary>
        /// Get all tracked browser IDs for the current process.
        /// Browser process: returns IDs from the C#-maintained browser id set.
        /// Renderer process: returns IDs from the tracked renderer browser/frame dictionary.
        /// </summary>
        internal static int[] GetAllContextBrowserIds()
        {
            if (s_ProcessType == (int)CefProcessType.RendererProcess) {
                return s_RendererBrowserFrames.Keys.ToArray();
            }
            return s_BrowserBrowserIds.ToArray();
        }
        /// <summary>
        /// Set the current context (Browser/Frame) by browser ID.
        /// Browser process: uses native GetBrowserById + BrowserGetMainFrame.
        /// Renderer process: uses native GetRendererBrowserFrameById.
        /// Returns true if the context was set successfully.
        /// </summary>
        internal static bool SetContextById(int browserId)
        {
            if (s_NativeApi == null) return false;
            if (s_ProcessType == (int)CefProcessType.RendererProcess) {
                var pair = s_NativeApi.GetRendererBrowserFrameById(browserId);
                if (pair.browser == IntPtr.Zero) {
                    // Sync: remove stale entry from C# dictionary
                    s_RendererBrowserFrames.Remove(browserId);
                    return false;
                }
                NativeApi.SetContext(pair.browser, pair.frame);
                return true;
            }
            // Browser process
            IntPtr browser = s_NativeApi.GetBrowserById(browserId);
            if (browser == IntPtr.Zero) {
                // Sync: remove stale entry from C# id set
                s_BrowserBrowserIds.Remove(browserId);
                return false;
            }
            IntPtr frame = s_NativeApi.BrowserGetMainFrame(browser);
            NativeApi.SetContext(browser, frame);
            return true;
        }
        /// <summary>
        /// Get browser pointer by browser ID. Works for both processes.
        /// Browser process: uses native GetBrowserById (CefBrowserHost::GetBrowserByIdentifier).
        /// Renderer process: uses native GetRendererBrowserFrameById from C++ ref map.
        /// Returns IntPtr.Zero if not found (and removes stale entry from C# tracking).
        /// </summary>
        internal static IntPtr GetBrowserById(int browserId)
        {
            if (s_NativeApi == null) return IntPtr.Zero;
            if (s_ProcessType == (int)CefProcessType.RendererProcess) {
                var pair = s_NativeApi.GetRendererBrowserFrameById(browserId);
                if (pair.browser == IntPtr.Zero) {
                    // Sync: remove stale entry from C# dictionary
                    s_RendererBrowserFrames.Remove(browserId);
                }
                return pair.browser;
            }
            // Browser process
            IntPtr browser = s_NativeApi.GetBrowserById(browserId);
            if (browser == IntPtr.Zero) {
                // Sync: remove stale entry from C# id set
                s_BrowserBrowserIds.Remove(browserId);
            }
            return browser;
        }
        /// <summary>
        /// Find a browser ID whose URL contains the given key substring.
        /// Returns the first matching browser ID, or -1 if not found.
        /// Works for both browser and renderer processes.
        /// </summary>
        internal static int FindBrowserIdByUrlKey(string urlKey)
        {
            if (s_NativeApi == null || string.IsNullOrEmpty(urlKey)) return -1;
            var ids = GetAllContextBrowserIds();
            foreach (var id in ids) {
                string url = string.Empty;
                if (s_ProcessType == (int)CefProcessType.RendererProcess) {
                    // In renderer process, use GetRendererBrowserFrameById to get valid pointers
                    var pair = s_NativeApi.GetRendererBrowserFrameById(id);
                    if (pair.browser == IntPtr.Zero) {
                        // Sync: remove stale entry from C# dictionary
                        s_RendererBrowserFrames.Remove(id);
                        continue;
                    }
                    if (pair.frame != IntPtr.Zero && s_NativeApi.FrameIsValid(pair.frame)) {
                        url = s_NativeApi.FrameGetUrl(pair.frame);
                    }
                }
                else {
                    IntPtr browser = s_NativeApi.GetBrowserById(id);
                    if (browser != IntPtr.Zero) {
                        url = s_NativeApi.BrowserGetUrl(browser);
                    }
                    else {
                        // Sync: remove stale entry from C# id set
                        s_BrowserBrowserIds.Remove(id);
                    }
                }
                if (!string.IsNullOrEmpty(url) && url.Contains(urlKey, StringComparison.OrdinalIgnoreCase)) {
                    return id;
                }
            }
            return -1;
        }
        internal static void NativeLogNoLock(string msg)
        {
            if (null != s_NativeApi) {
                s_NativeApi.NativeLog(msg);
            }
        }
        internal static void JsLogNoLock(string msg)
        {
            if (null != s_NativeApi) {
                s_NativeApi.JavascriptLog(msg);
            }
        }
        internal static string LoadFunc(string func, string code, IList<string> paramNames, bool update)
        {
            try {
                PrepareBatchScript();
                // Execute the script directly using the DSL interpreter
                BatchCommand.BatchScript.ClearDslErrors();
                BatchScript.LoadFunc(func, code, paramNames, update);
                if (BatchCommand.BatchScript.HasDslErrors) {
                    return BatchCommand.BatchScript.GetDslErrors();
                }
                return string.Empty;
            }
            catch (Exception ex) {
                return $"Error: {ex.Message}";
            }
        }
        internal static void RefreshGlobalVars()
        {
            //reset global vars
            BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
            BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
            BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
            BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
            BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
            BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
            BatchCommand.BatchScript.SetGlobalVariable("startupurl", BoxedValue.From(s_StartupUrl));
            BatchCommand.BatchScript.SetGlobalVariable("loadedurl", BoxedValue.FromString(s_LoadedUrl));
            BatchCommand.BatchScript.SetGlobalVariable("dslpath", BoxedValue.FromString(s_DslScriptPath));
            BatchCommand.BatchScript.SetGlobalVariable("dslfile", BoxedValue.FromString(s_DslScriptFile));
            BatchCommand.BatchScript.SetGlobalVariable("dslfilechanged", BoxedValue.FromBool(s_DslScriptFileChanged));
            BatchCommand.BatchScript.SetGlobalVariable("initialprojectidentity", BoxedValue.FromString(s_InitialProjectIdentity));
            BatchCommand.BatchScript.SetGlobalVariable("initialprojectidentityinited", BoxedValue.FromBool(s_InitialProjectIdentityInited));
            BatchCommand.BatchScript.ClearDslErrors();
        }
        internal static void AddCommonApiDocs()
        {
            BatchCommand.BatchScript.AddUserApiDoc("clone", "clone(list_or_dict) api");
            BatchCommand.BatchScript.AddUserApiDoc("args", "args() api");
            BatchCommand.BatchScript.AddUserApiDoc("arg", "arg(ix) api");
            BatchCommand.BatchScript.AddUserApiDoc("argnum", "argnum() api");
            BatchCommand.BatchScript.AddUserApiDoc("+", "add operator");
            BatchCommand.BatchScript.AddUserApiDoc("-", "sub operator");
            BatchCommand.BatchScript.AddUserApiDoc("*", "mul operator");
            BatchCommand.BatchScript.AddUserApiDoc("/", "div operator");
            BatchCommand.BatchScript.AddUserApiDoc("%", "mod operator");
            BatchCommand.BatchScript.AddUserApiDoc("&", "bitand operator");
            BatchCommand.BatchScript.AddUserApiDoc("|", "bitor operator");
            BatchCommand.BatchScript.AddUserApiDoc("^", "bitxor operator");
            BatchCommand.BatchScript.AddUserApiDoc("~", "bitnot operator");
            BatchCommand.BatchScript.AddUserApiDoc("<<", "left shift operator");
            BatchCommand.BatchScript.AddUserApiDoc(">>", "right shift operator");
            BatchCommand.BatchScript.AddUserApiDoc(">", "great operator");
            BatchCommand.BatchScript.AddUserApiDoc(">=", "great equal operator");
            BatchCommand.BatchScript.AddUserApiDoc("<", "less operator");
            BatchCommand.BatchScript.AddUserApiDoc("<=", "less equal operator");
            BatchCommand.BatchScript.AddUserApiDoc("==", "equal operator");
            BatchCommand.BatchScript.AddUserApiDoc("!=", "not equal operator");
            BatchCommand.BatchScript.AddUserApiDoc("&&", "logical and operator");
            BatchCommand.BatchScript.AddUserApiDoc("||", "logical or operator");
            BatchCommand.BatchScript.AddUserApiDoc("!", "logical not operator");
            BatchCommand.BatchScript.AddUserApiDoc("?", "conditional expression");
            BatchCommand.BatchScript.AddUserApiDoc("if", "if(cond)func(args); or if(cond){...}[elseif/elif(cond){...}else{...}]; statement");
            BatchCommand.BatchScript.AddUserApiDoc("while", "while(cond)func(args); or while(cond){...}; statement, iterator is $$");
            BatchCommand.BatchScript.AddUserApiDoc("loop", "loop(ct)func(args); or loop(ct){...}; statement, iterator is $$");
            BatchCommand.BatchScript.AddUserApiDoc("looplist", "looplist(list)func(args); or looplist(list){...}; statement, iterator is $$");
            BatchCommand.BatchScript.AddUserApiDoc("foreach", "foreach(arg1,arg2,...)func(args); or foreach(arg1,arg2,...){...}; statement, iterator is $$");
            BatchCommand.BatchScript.AddUserApiDoc("return", "return([val]) api");
            BatchCommand.BatchScript.AddUserApiDoc("dotnetcall", "dotnetcall api, internal implementation, using csharp object syntax");
            BatchCommand.BatchScript.AddUserApiDoc("dotnetset", "dotnetset api, internal implementation, using csharp object syntax");
            BatchCommand.BatchScript.AddUserApiDoc("dotnetget", "dotnetget api, internal implementation, using csharp object syntax");
            BatchCommand.BatchScript.AddUserApiDoc("collectioncall", "collectioncall api, internal implementation, using csharp object syntax");
            BatchCommand.BatchScript.AddUserApiDoc("collectionset", "collectionset api, internal implementation, using csharp object syntax");
            BatchCommand.BatchScript.AddUserApiDoc("collectionget", "collectionget api, internal implementation, using csharp object syntax");
            BatchCommand.BatchScript.AddUserApiDoc("linq", "linq(list,method,arg1,arg2,...) statement, internal implementation, using obj.method(arg1,arg2,...) syntax, method can be orderby/orderbydesc/where/top, iterator is $$");
            BatchCommand.BatchScript.AddUserApiDoc("max", "max(v1,v2)");
            BatchCommand.BatchScript.AddUserApiDoc("min", "min(v1,v2)");
            BatchCommand.BatchScript.AddUserApiDoc("abs", "abs(v)");
            BatchCommand.BatchScript.AddUserApiDoc("sin", "sin(v)");
            BatchCommand.BatchScript.AddUserApiDoc("cos", "cos(v)");
            BatchCommand.BatchScript.AddUserApiDoc("tan", "tan(v)");
            BatchCommand.BatchScript.AddUserApiDoc("asin", "asin(v)");
            BatchCommand.BatchScript.AddUserApiDoc("acos", "acos(v)");
            BatchCommand.BatchScript.AddUserApiDoc("atan", "atan(v)");
            BatchCommand.BatchScript.AddUserApiDoc("atan2", "atan2(v1,v2)");
            BatchCommand.BatchScript.AddUserApiDoc("sinh", "sinh(v)");
            BatchCommand.BatchScript.AddUserApiDoc("cosh", "cosh(v)");
            BatchCommand.BatchScript.AddUserApiDoc("tanh", "tanh(v)");
            BatchCommand.BatchScript.AddUserApiDoc("pow", "pow(v1,v2)");
            BatchCommand.BatchScript.AddUserApiDoc("sqrt", "sqrt(v)");
            BatchCommand.BatchScript.AddUserApiDoc("exp", "exp(v)");
            BatchCommand.BatchScript.AddUserApiDoc("exp2", "exp2(v)");
            BatchCommand.BatchScript.AddUserApiDoc("log", "log(v)");
            BatchCommand.BatchScript.AddUserApiDoc("log2", "log2(v)");
            BatchCommand.BatchScript.AddUserApiDoc("log10", "log10(v)");
            BatchCommand.BatchScript.AddUserApiDoc("floor", "floor(v)");
            BatchCommand.BatchScript.AddUserApiDoc("ceiling", "ceiling(v)");
            BatchCommand.BatchScript.AddUserApiDoc("round", "round(v)");
            BatchCommand.BatchScript.AddUserApiDoc("bool", "bool(v)");
            BatchCommand.BatchScript.AddUserApiDoc("sbyte", "sbyte(v)");
            BatchCommand.BatchScript.AddUserApiDoc("byte", "byte(v)");
            BatchCommand.BatchScript.AddUserApiDoc("char", "char(v)");
            BatchCommand.BatchScript.AddUserApiDoc("short", "short(v)");
            BatchCommand.BatchScript.AddUserApiDoc("ushort", "ushort(v)");
            BatchCommand.BatchScript.AddUserApiDoc("int", "int(v)");
            BatchCommand.BatchScript.AddUserApiDoc("uint", "uint(v)");
            BatchCommand.BatchScript.AddUserApiDoc("long", "long(v)");
            BatchCommand.BatchScript.AddUserApiDoc("ulong", "ulong(v)");
            BatchCommand.BatchScript.AddUserApiDoc("float", "float(v)");
            BatchCommand.BatchScript.AddUserApiDoc("double", "double(v)");
            BatchCommand.BatchScript.AddUserApiDoc("decimal", "decimal(v)");
            BatchCommand.BatchScript.AddUserApiDoc("lerp", "lerp(a,b,t)");
            BatchCommand.BatchScript.AddUserApiDoc("clamp01", "clamp01(v)");
            BatchCommand.BatchScript.AddUserApiDoc("clamp", "clamp(v,v1,v2)");
            BatchCommand.BatchScript.AddUserApiDoc("approximately", "approximately(v1,v2)");
            BatchCommand.BatchScript.AddUserApiDoc("format", "format(fmt,arg1,arg2,...)");
            BatchCommand.BatchScript.AddUserApiDoc("null", "null()");
            BatchCommand.BatchScript.AddUserApiDoc("tuple", "(v1,v2,...) or tuple(v1,v2,...) object");
            BatchCommand.BatchScript.AddUserApiDoc("array", "[v1,v2,...] or array(v1,v2,...) object");
            BatchCommand.BatchScript.AddUserApiDoc("list", "list(v1,v2,...) object");
            BatchCommand.BatchScript.AddUserApiDoc("hashtable", "{k1=>v1,k2=>v2,...} or {k1:v1,k2:v2,...} or hashtable(k1=>v1,k2=>v2,...) or hashtable(k1:v1,k2:v2,...) object");
            BatchCommand.BatchScript.AddUserApiDoc("peek", "peek(queue_or_stack)");
            BatchCommand.BatchScript.AddUserApiDoc("stack", "stack(v1,v2,...) object");
            BatchCommand.BatchScript.AddUserApiDoc("push", "push(stack,v)");
            BatchCommand.BatchScript.AddUserApiDoc("pop", "pop(stack)");
            BatchCommand.BatchScript.AddUserApiDoc("queue", "queue(v1,v2,...) object");
            BatchCommand.BatchScript.AddUserApiDoc("enqueue", "enqueue(queue,v)");
            BatchCommand.BatchScript.AddUserApiDoc("dequeue", "dequeue(queue)");
            BatchCommand.BatchScript.AddUserApiDoc("expand", "expand(str)");
            BatchCommand.BatchScript.AddUserApiDoc("envs", "envs()");
            BatchCommand.BatchScript.AddUserApiDoc("cd", "cd(path)");
            BatchCommand.BatchScript.AddUserApiDoc("pwd", "pwd()");
            BatchCommand.BatchScript.AddUserApiDoc("os", "os()");
            BatchCommand.BatchScript.AddUserApiDoc("echo", "echo(fmt,arg1,arg2,...) api, Console.WriteLine");
        }

        private static string GetStringInLength(List<string> args)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var arg in args) {
                if (first) {
                    first = false;
                }
                else {
                    sb.Append('|');
                }
                sb.Append(GetStringInLength(arg));
            }
            return sb.ToString();
        }
        private static string GetStringInLength(string str)
        {
            return NativeApi.GetStringInLength(str, 100, 0);
        }

        private static void TryLoadDSL()
        {
            PrepareBatchScript();
            string path = Path.Combine(s_BasePath, "managed", s_DslScriptFile);
            var fi = new FileInfo(path);
            if (fi.Exists) {
                if (fi.LastWriteTime != s_DslScriptTime || s_DslScriptPath != path) {
                    s_DslScriptTime = fi.LastWriteTime;
                    s_DslScriptPath = path;

                    string errorMsg = string.Empty;
                    if (File.Exists(fi.FullName)) {
                        BatchCommand.BatchScript.Load(fi.FullName);
                        BatchCommand.BatchScript.Call("init_global_consts");
                        NativeLogNoLock("[csharp] Load dsl script: " + fi.FullName);
                    }
                    else {
                        errorMsg = "DSL script file does not exist";
                        NativeLogNoLock("[csharp] " + errorMsg + ": " + fi.FullName);
                    }
                }
            }
            else {
                NativeLogNoLock("[csharp] Can't find dsl script: " + fi.FullName);
            }
            RefreshGlobalVars();
            NativeApi.ClearApiErrorInfo();
        }
        // Execute MetaDSL script
        private static string ExecuteMetaDslScript(string script)
        {
            try {
                // Execute the script directly using the DSL interpreter
                RefreshGlobalVars();
                NativeApi.ClearApiErrorInfo();
                var id = BatchCommand.BatchScript.EvalAsFunc(script, s_EmptyArgs);
                var sb = new StringBuilder();
                if (!BatchCommand.BatchScript.HasDslErrors) {
                    var result = BatchCommand.BatchScript.Call(id);
                    string resultStr;
                    if (result.IsNullObject) {
                        resultStr = "null";
                    }
                    else if (null != Lib.AgentPlugin) {
                        resultStr = Lib.AgentPlugin.ResultToString(result);
                    }
                    else {
                        resultStr = result.ToString();
                    }
                    int maxResultSize = Lib.AgentPlugin?.GetMaxResultSize() ?? 0;
                    if (maxResultSize > 0 && resultStr.Length > maxResultSize) {
                        resultStr = resultStr.Substring(0, maxResultSize) + "\n... [truncated, exceeded max result size " + maxResultSize + "]";
                    }
                    sb.AppendLine(resultStr);
                }
                if (NativeApi.HasApiErrorInfo) {
                    sb.AppendLine();
                    sb.Append(NativeApi.GetApiErrorInfo());
                }
                if (BatchCommand.BatchScript.HasDslErrors) {
                    sb.AppendLine();
                    sb.Append(BatchCommand.BatchScript.GetDslErrors());
                }
                return sb.ToString();
            }
            catch (Exception ex) {
                NativeLogNoLock($"[AgentCommand] Error executing MetaDSL script: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        private static void RegisterBatchScriptApi()
        {
            AddCommonApiDocs();
            // Basic framework APIs (defined in Program.cs)
            BatchCommand.BatchScript.Register("import", "import(dsl_file,...)", false, new ExpressionFactoryHelper<ImportExp>());
            BatchCommand.BatchScript.Register("nativelog", "nativelog(fmt, ...)", new ExpressionFactoryHelper<NativeLogExp>());
            BatchCommand.BatchScript.Register("javascriptlog", "javascriptlog(fmt, ...)", new ExpressionFactoryHelper<JavascriptLogExp>());
            BatchCommand.BatchScript.Register("quotestring", "quotestring(str)", false, new ExpressionFactoryHelper<QuoteStringExp>());
            BatchCommand.BatchScript.Register("quote_string", "quote_string(str)", new ExpressionFactoryHelper<QuoteStringExp>());
            BatchCommand.BatchScript.Register("stripquotes", "stripquotes(str)", false, new ExpressionFactoryHelper<StripQuotesExp>());
            BatchCommand.BatchScript.Register("strip_quotes", "strip_quotes(str)", new ExpressionFactoryHelper<StripQuotesExp>());
            BatchCommand.BatchScript.Register("trygetrawswitch", "trygetrawswitch(str)", false, new ExpressionFactoryHelper<TryGetRawCommandLineSwitchExp>());
            BatchCommand.BatchScript.Register("try_get_raw_switch", "try_get_raw_switch(str)", new ExpressionFactoryHelper<TryGetRawCommandLineSwitchExp>());
            BatchCommand.BatchScript.Register("getdotnetinfo", "getdotnetinfo()", false, new ExpressionFactoryHelper<GetDotnetInfoExp>());
            BatchCommand.BatchScript.Register("get_dotnet_info", "get_dotnet_info()", false, new ExpressionFactoryHelper<GetDotnetInfoExp>());
            BatchCommand.BatchScript.Register("help", "help(pattern, ...), agent api help", new ExpressionFactoryHelper<HelpExp>());
            BatchCommand.BatchScript.Register("helpall", "helpall(pattern, ...), agent and framework api help", new ExpressionFactoryHelper<HelpAllExp>());

            // Agent-related APIs are registered by AgentCore plugin via LoadAgentPlugin()

            // Only valid in MainThread
            BatchCommand.BatchScript.Register("handle_thread_queue", "handle_thread_queue([max_native_logs,max_js_logs,max_code_count,max_func_count]), only valid in main thread", false, new ExpressionFactoryHelper<HandleThreadQueueExp>());
            BatchCommand.BatchScript.Register("set_heart_beat_interval", "set_heart_beat_interval(interval_ms), set heartbeat interval in ms (10-60000)", false, new ExpressionFactoryHelper<SetHeartBeatIntervalExp>());
            BatchCommand.BatchScript.Register("get_browser_ids", "get_browser_ids() - get all browser IDs in current process", false, new ExpressionFactoryHelper<GetBrowserIdsExp>());
            BatchCommand.BatchScript.Register("set_context_by_id", "set_context_by_id(browser_id) - set current context by browser ID, returns bool", false, new ExpressionFactoryHelper<SetContextByIdExp>());
            BatchCommand.BatchScript.Register("find_browser_id_by_url_key", "find_browser_id_by_url_key(url_key) - find browser ID by URL substring, returns -1 if not found", false, new ExpressionFactoryHelper<FindBrowserIdByUrlKeyExp>());
        }
        private static void PrepareBatchScript()
        {
            if (!s_BatchScriptInited) {
                BatchCommand.BatchScript.Init();
                RegisterBatchScriptApi();
                s_BatchScriptInited = true;
            }
        }

        [ThreadStatic]
        private static bool s_BatchScriptInited = false;
        private static string s_CmdLine = string.Empty;
        private static string s_BasePath = string.Empty;
        private static string s_AppDir = string.Empty;
        private static bool s_IsMac = false;
        private static int s_ProcessType = -1;
        // Renderer process: tracked browser/frame pairs (browserId -> (browser, frame))
        private static readonly Dictionary<int, (IntPtr browser, IntPtr frame)> s_RendererBrowserFrames = new();
        // Browser process: tracked browser IDs (maintained by OnBrowserInit/OnBrowserFinalize)
        private static readonly HashSet<int> s_BrowserBrowserIds = new();
        private static string s_StartupUrl = string.Empty;
        private static string s_LoadedUrl = string.Empty;
        private static string s_DslScriptPath = string.Empty;
        private static string s_DslScriptFile = "Script.dsl";
        private static bool s_DslScriptFileChanged = false;
        private static string s_InitialProjectIdentity = string.Empty;
        private static bool s_InitialProjectIdentityInited = false;
        private static DateTime s_DslScriptTime = DateTime.Now;
        private static int s_MainThreadId = 0;
        private static object s_Lock = new object();

        private static List<string> s_EmptyArgs = new List<string>();
        private static StringBuilder s_StringBuilder = new StringBuilder();
        private static StringWriter s_StringWriter = new StringWriter(s_StringBuilder);
        private static NativeApi? s_NativeApi;
    }
    internal static class CefDotnetAppApi
    {
        // Execute MetaDSL script
        internal static string ExecuteMetaDslScript(string script, out bool hasError)
        {
            try {
                hasError = false;
                PrepareBatchScript();
                // Execute the script directly using the DSL interpreter
                Lib.RefreshGlobalVars();
                NativeApi.ClearApiErrorInfo();
                var id = BatchCommand.BatchScript.EvalAsFunc(script, s_EmptyArgs);
                var sb = new StringBuilder();
                if (!BatchCommand.BatchScript.HasDslErrors) {
                    var result = BatchCommand.BatchScript.Call(id);
                    string resultStr;
                    if (result.IsNullObject) {
                        resultStr = "null";
                    }
                    else if (null != Lib.AgentPlugin) {
                        resultStr = Lib.AgentPlugin.ResultToString(result);
                    }
                    else {
                        resultStr = result.ToString();
                    }
                    int maxResultSize = Lib.AgentPlugin?.GetMaxResultSize() ?? 0;
                    if (maxResultSize > 0 && resultStr.Length > maxResultSize) {
                        resultStr = resultStr.Substring(0, maxResultSize) + "\n... [truncated, exceeded max result size " + maxResultSize + "]";
                    }
                    sb.Append(resultStr);
                }
                if (NativeApi.HasApiErrorInfo) {
                    hasError = true;
                    sb.AppendLine();
                    sb.Append(NativeApi.GetApiErrorInfo());
                }
                if (BatchCommand.BatchScript.HasDslErrors) {
                    hasError = true;
                    sb.AppendLine();
                    sb.Append(BatchCommand.BatchScript.GetDslErrors());
                }
                return sb.ToString();
            }
            catch (Exception ex) {
                hasError = true;
                Lib.NativeLog($"[AgentCommand] Error executing MetaDSL script: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
        internal static string LoadFunc(string func, string code, IList<string> paramNames, bool update)
        {
            try {
                PrepareBatchScript();
                // Execute the script directly using the DSL interpreter
                BatchCommand.BatchScript.ClearDslErrors();
                BatchScript.LoadFunc(func, code, paramNames, update);
                if (BatchCommand.BatchScript.HasDslErrors) {
                    return BatchCommand.BatchScript.GetDslErrors();
                }
                return string.Empty;
            }
            catch (Exception ex) {
                return $"Error: {ex.Message}";
            }
        }
        private static void RegisterBatchScriptApi()
        {
            Lib.AddCommonApiDocs();

            // Basic framework APIs (defined in Program.cs)
            BatchCommand.BatchScript.Register("import", "import(dsl_file,...)", false, new ExpressionFactoryHelper<ImportExp>());
            BatchCommand.BatchScript.Register("nativelog", "nativelog(fmt, ...)", new ExpressionFactoryHelper<NativeLogExp>());
            BatchCommand.BatchScript.Register("javascriptlog", "javascriptlog(fmt, ...)", new ExpressionFactoryHelper<JavascriptLogExp>());
            BatchCommand.BatchScript.Register("quotestring", "quotestring(str)", false, new ExpressionFactoryHelper<QuoteStringExp>());
            BatchCommand.BatchScript.Register("quote_string", "quote_string(str)", new ExpressionFactoryHelper<QuoteStringExp>());
            BatchCommand.BatchScript.Register("stripquotes", "stripquotes(str)", false, new ExpressionFactoryHelper<StripQuotesExp>());
            BatchCommand.BatchScript.Register("strip_quotes", "strip_quotes(str)", new ExpressionFactoryHelper<StripQuotesExp>());
            BatchCommand.BatchScript.Register("trygetrawswitch", "trygetrawswitch(str)", false, new ExpressionFactoryHelper<TryGetRawCommandLineSwitchExp>());
            BatchCommand.BatchScript.Register("try_get_raw_switch", "try_get_raw_switch(str)", new ExpressionFactoryHelper<TryGetRawCommandLineSwitchExp>());
            BatchCommand.BatchScript.Register("getdotnetinfo", "getdotnetinfo()", false, new ExpressionFactoryHelper<GetDotnetInfoExp>());
            BatchCommand.BatchScript.Register("get_dotnet_info", "get_dotnet_info()", false, new ExpressionFactoryHelper<GetDotnetInfoExp>());
            BatchCommand.BatchScript.Register("help", "help(pattern, ...), agent api help", new ExpressionFactoryHelper<HelpExp>());
            BatchCommand.BatchScript.Register("helpall", "helpall(pattern, ...), agent and framework api help", new ExpressionFactoryHelper<HelpAllExp>());

            // Agent-related APIs are registered by AgentCore plugin via LoadAgentPlugin()
            if (null != Lib.AgentPlugin) {
                Lib.AgentPlugin.RegisterScriptApis();
            }
        }
        private static void PrepareBatchScript()
        {
            if (!s_BatchScriptInited) {
                BatchCommand.BatchScript.Init();
                RegisterBatchScriptApi();
                s_BatchScriptInited = true;
            }
        }

        [ThreadStatic]
        private static bool s_BatchScriptInited = false;
        private static List<string> s_EmptyArgs = new List<string>();
    }
}