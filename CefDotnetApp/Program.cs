
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
using System.Threading;
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
    public IntPtr ExecuteJavascriptInRenderer;
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
    // DevTools host actions
    public IntPtr BrowserShowDevTools;
    public IntPtr BrowserCloseDevTools;
    public IntPtr BrowserHasDevTools;
    public IntPtr BrowserSendDevToolsMessage;
    public IntPtr BrowserExecuteDevToolsMethod;
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
    public IntPtr RequestSetUrl;
    public IntPtr RequestSetFlags;
    public IntPtr RequestSetFirstPartyForCookies;
    public IntPtr RequestSetHeaderByName;
    public IntPtr RequestRemoveHeaderByName;
    public IntPtr RequestSetHeaderMap;
    public IntPtr RequestSetReferrer;
    // CefResponse properties
    public IntPtr ResponseIsReadOnly;
    public IntPtr ResponseGetStatus;
    public IntPtr ResponseSetStatus;
    public IntPtr ResponseGetStatusText;
    public IntPtr ResponseSetStatusText;
    public IntPtr ResponseGetMimeType;
    public IntPtr ResponseSetMimeType;
    public IntPtr ResponseGetCharset;
    public IntPtr ResponseSetCharset;
    public IntPtr ResponseGetUrl;
    public IntPtr ResponseGetHeaderMap;
    public IntPtr ResponseGetHeaderByName;
    public IntPtr ResponseSetHeaderByName;
    public IntPtr ResponseRemoveHeaderByName;
    public IntPtr ResponseSetHeaderMap;
    public IntPtr ResponseGetError;
    public IntPtr ResponseSetError;
    public IntPtr ResponseSetUrl;
    // Heartbeat control
    public IntPtr SetHeartbeatInterval;
    // Generic async callback completion (see native_callbacks.h on native side)
    public IntPtr NativeCallbackComplete;
}

// delegate for native api
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostNativeLogDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostSendCefMessageDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, IntPtr args, int argCount, IntPtr browser, IntPtr frame, int cef_process_id);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
public delegate bool HostSendJavascriptCodeDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string code, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
public delegate bool HostSendJavascriptCallDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, IntPtr args, int argCount, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostCallJavascriptFuncInRendererDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, IntPtr args, int argCount, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostExecuteJavascriptInRendererDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string code, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostFreeNativeStringDelegation(IntPtr str);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
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
[return: MarshalAs(UnmanagedType.U1)]
public delegate bool HostCommandLineIsValidDelegation(IntPtr command_line);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
public delegate bool HostCommandLineIsReadOnlyDelegation(IntPtr command_line);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
public delegate bool HostCommandLineHasSwitchesDelegation(IntPtr command_line);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
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
[return: MarshalAs(UnmanagedType.U1)]
public delegate bool HostBrowserIsLoadingDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
public delegate bool HostBrowserIsPopupDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
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
// DevTools host actions
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostBrowserShowDevToolsDelegation(IntPtr browser, int inspect_x, int inspect_y, int has_inspect_point);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostBrowserCloseDevToolsDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostBrowserHasDevToolsDelegation(IntPtr browser);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostBrowserSendDevToolsMessageDelegation(IntPtr browser, IntPtr message, int size);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostBrowserExecuteDevToolsMethodDelegation(IntPtr browser, int message_id, [MarshalAs(UnmanagedType.LPUTF8Str)] string method, [MarshalAs(UnmanagedType.LPUTF8Str)] string? params_json);
// Frame properties
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostFrameGetUrlDelegation(IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostFrameGetNameDelegation(IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostFrameGetIdentifierDelegation(IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
public delegate bool HostFrameIsMainDelegation(IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
public delegate bool HostFrameIsValidDelegation(IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
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
[return: MarshalAs(UnmanagedType.U1)]
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
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostRequestSetUrlDelegation(IntPtr request, [MarshalAs(UnmanagedType.LPUTF8Str)] string? url);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostRequestSetFlagsDelegation(IntPtr request, int flags);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostRequestSetFirstPartyForCookiesDelegation(IntPtr request, [MarshalAs(UnmanagedType.LPUTF8Str)] string? url);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostRequestSetHeaderByNameDelegation(IntPtr request, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, [MarshalAs(UnmanagedType.LPUTF8Str)] string? value, int overwrite);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostRequestRemoveHeaderByNameDelegation(IntPtr request, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostRequestSetHeaderMapDelegation(IntPtr request, [MarshalAs(UnmanagedType.LPUTF8Str)] string? header_map_str);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostRequestSetReferrerDelegation(IntPtr request, [MarshalAs(UnmanagedType.LPUTF8Str)] string? referrer_url, int referrer_policy);
// CefResponse properties
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
public delegate bool HostResponseIsReadOnlyDelegation(IntPtr response);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostResponseGetStatusDelegation(IntPtr response);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostResponseSetStatusDelegation(IntPtr response, int status);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostResponseGetStatusTextDelegation(IntPtr response);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostResponseSetStatusTextDelegation(IntPtr response, [MarshalAs(UnmanagedType.LPUTF8Str)] string? status_text);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostResponseGetMimeTypeDelegation(IntPtr response);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostResponseSetMimeTypeDelegation(IntPtr response, [MarshalAs(UnmanagedType.LPUTF8Str)] string? mime_type);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostResponseGetCharsetDelegation(IntPtr response);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostResponseSetCharsetDelegation(IntPtr response, [MarshalAs(UnmanagedType.LPUTF8Str)] string? charset);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostResponseGetUrlDelegation(IntPtr response);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostResponseGetHeaderMapDelegation(IntPtr response);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr HostResponseGetHeaderByNameDelegation(IntPtr response, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostResponseSetHeaderByNameDelegation(IntPtr response, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, [MarshalAs(UnmanagedType.LPUTF8Str)] string? value, int overwrite);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostResponseRemoveHeaderByNameDelegation(IntPtr response, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostResponseSetHeaderMapDelegation(IntPtr response, [MarshalAs(UnmanagedType.LPUTF8Str)] string? header_map_str);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostResponseGetErrorDelegation(IntPtr response);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostResponseSetErrorDelegation(IntPtr response, int error);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostResponseSetUrlDelegation(IntPtr response, [MarshalAs(UnmanagedType.LPUTF8Str)] string? url);
// Heartbeat control
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostSetHeartbeatIntervalDelegation(int interval_ms);
// Generic async callback completion. Callable from any managed thread: the
// native side posts the pending CEF callback to the thread it belongs to.
// |code| is an interface specific numeric result (0 when unused, e.g. the error
// code of a failed cefQuery answer).
// Returns 1 when the handle was found (0 = already completed or unknown).
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostNativeCallbackCompleteDelegation(long handle, int ok, [MarshalAs(UnmanagedType.LPUTF8Str)] string? data, int code);

namespace DotNetLib
{
    sealed class SetDslFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                NativeApi.AppendApiErrorInfoLine("Expected: setdslfile(dsl_file)");
                return BoxedValue.EmptyString;
            }
            string file = operands[0].AsString;
            if (!string.IsNullOrEmpty(file)) {
                Lib.DslScriptFile = file;
                BatchCommand.BatchScript.SetGlobalVariable("dslfile", BoxedValue.FromString(Lib.DslScriptFile));
            }
            return BoxedValue.FromString(Lib.DslScriptFile);
        }
    }
    sealed class ImportExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var files = new List<string>();
            for (int ix = 0; ix < operands.Count; ix++) {
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
            BatchScript.LoadImportFiles(files);
            if (BatchScript.HasDslErrors)
                return BoxedValue.FromBool(false);
            foreach (var file in files) {
                Lib.NativeLog($"Import: {file}");
            }
            return BoxedValue.FromBool(true);
        }
    }
    sealed class RedirectCallExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            int num = operands.Count;
            if (num < 1) {
                NativeApi.AppendApiErrorInfoLine("Expected: redirectcall(func_name) or redirectcall(func_name, args) or redirectcall(func_name, args, ...)");
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
    sealed class ExecuteMetaDslExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                NativeApi.AppendApiErrorInfoLine("Expected: execute_metadsl(dsl_code), aliased as executemetadsl");
                return BoxedValue.From(-1);
            }
            string dslCode = operands[0].AsString;
            bool hasError;
            string res;
            if (Thread.CurrentThread.ManagedThreadId == Lib.MainThreadId) {
                res = Lib.ExecuteMetaDslScript(dslCode, 0, out hasError);
            }
            else {
                res = CefDotnetAppApi.ExecuteMetaDslScript(dslCode, 0, out hasError);
            }
            return BoxedValue.From(Tuple.Create(BoxedValue.FromBool(hasError), BoxedValue.FromString(res)));
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
    sealed class EnqueueCefMessageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                throw new Exception("Expected: startasynctask(func_name, args...) api");
            string funcName = operands[0].AsString;
            var args = new string[operands.Count - 1];
            for (int i = 1; i < operands.Count; i++) {
                args[i - 1] = operands[i].AsString;
            }
            bool r = Lib.EnqueueCefMessage(funcName, args);
            return BoxedValue.FromBool(r);
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
            bool r = Lib.HandleThreadQueue(maxNativeCount, maxJsCount, maxCodeCount, maxFuncCount);
            return BoxedValue.FromBool(r);
        }
    }
    sealed class SetHeartBeatIntervalExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                NativeApi.AppendApiErrorInfoLine("Expected: set_heartbeat_interval(interval_ms)");
                return BoxedValue.FromBool(false);
            }
            int intervalMs = operands[0].GetInt();
            bool r = Lib.SetHeartbeatInterval(intervalMs);
            return BoxedValue.FromBool(r);
        }
    }
    sealed class SendJavascriptCodeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                NativeApi.AppendApiErrorInfoLine("Expected: send_javascript_code(code)");
                return BoxedValue.FromBool(false);
            }
            string code = operands[0].AsString ?? string.Empty;
            if (string.IsNullOrEmpty(code)) {
                return BoxedValue.FromBool(false);
            }
            // Posts the code to the renderer for the browser/frame of the
            // current context; does not wait for a result.
            bool r = Lib.SendJavascriptCodeToRenderer(code);
            return BoxedValue.FromBool(r);
        }
    }
    sealed class SendJavascriptCallExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                NativeApi.AppendApiErrorInfoLine("Expected: send_javascript_call(func, arg1, arg2, ...)");
                return BoxedValue.FromBool(false);
            }
            string func = operands[0].AsString ?? string.Empty;
            if (string.IsNullOrEmpty(func)) {
                return BoxedValue.FromBool(false);
            }
            var args = new List<BoxedValue>();
            for (int i = 1; i < operands.Count; i++) {
                args.Add(operands[i]);
            }
            // Posts the call to the renderer for the browser/frame of the
            // current context; does not wait for a result.
            bool r = Lib.SendJavascriptCallToRenderer(func, args);
            return BoxedValue.FromBool(r);
        }
    }
    // Shows the in-page AgentDialog for a JS dialog taken over by the script.
    // The payload is JSON-encoded here so message/default text cannot break the
    // generated JavaScript (quotes, newlines, backslashes are all escaped).
    sealed class ShowNativeJsDialogExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                NativeApi.AppendApiErrorInfoLine("Expected: show_native_js_dialog(handle, dialog_type, message[, default_text])");
                return BoxedValue.FromBool(false);
            }
            long handle = operands[0].GetLong();
            int dialogType = operands[1].GetInt();
            string message = operands[2].AsString ?? string.Empty;
            string defaultText = operands.Count >= 4 ? (operands[3].AsString ?? string.Empty) : string.Empty;
            bool r = Lib.ShowNativeJsDialog(handle, dialogType, message, defaultText);
            return BoxedValue.FromBool(r);
        }
    }
    sealed class CompleteNativeCallbackExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                NativeApi.AppendApiErrorInfoLine("Expected: complete_native_callback(handle, ok[, data, code])");
                return BoxedValue.FromBool(false);
            }
            // The handle travels through JavaScript as a string because a JS
            // number only holds 53 bits, so accept both forms here.
            long handle;
            if (operands[0].IsString) {
                if (!long.TryParse(operands[0].AsString, out handle)) {
                    NativeApi.AppendApiErrorInfoLine("complete_native_callback: invalid handle string");
                    return BoxedValue.FromBool(false);
                }
            }
            else {
                handle = operands[0].GetLong();
            }
            bool ok = operands[1].GetBool();
            string? data = null;
            if (operands.Count >= 3) {
                data = operands[2].AsString;
            }
            // Interface specific numeric result, e.g. the error code passed to
            // CefMessageRouterBrowserSide::Callback::Failure. Unused by the JS
            // dialog and resource load entry points.
            int code = operands.Count >= 4 ? operands[3].GetInt() : 0;
            bool r = Lib.CompleteNativeCallback(handle, ok, data, code);
            return BoxedValue.FromBool(r);
        }
    }
    sealed class GetBrowserIdsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var ids = Lib.GetAllContextBrowserIds();
            if (null == ids)
                return BoxedValue.NullObject;
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
    // Parse a UTF-8 JSON payload (byte[] or string) into a DSL value tree.
    // Uses System.Text.Json for efficient in-place UTF-8 parsing without extra string allocation.
    sealed class DevToolsParseBytesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                NativeApi.AppendApiErrorInfoLine("Expected: dev_tools_parse_bytes(bytes_or_string)");
                return BoxedValue.NullObject;
            }
            try {
                var obj = operands[0].GetObject();
                if (obj is byte[] bytes) {
                    if (bytes.Length == 0) return BoxedValue.NullObject;
                    var reader = new System.Text.Json.Utf8JsonReader(bytes);
                    using (var doc = System.Text.Json.JsonDocument.ParseValue(ref reader)) {
                        return JsonElementToBoxed(doc.RootElement);
                    }
                }
                else {
                    string s = operands[0].AsString ?? string.Empty;
                    if (string.IsNullOrEmpty(s)) return BoxedValue.NullObject;
                    using (var doc = System.Text.Json.JsonDocument.Parse(s)) {
                        return JsonElementToBoxed(doc.RootElement);
                    }
                }
            }
            catch (Exception ex) {
                NativeApi.AppendApiErrorInfoLine("dev_tools_parse_bytes failed: " + ex.Message);
                return BoxedValue.NullObject;
            }
        }

        private static BoxedValue JsonElementToBoxed(System.Text.Json.JsonElement elem)
        {
            switch (elem.ValueKind) {
                case System.Text.Json.JsonValueKind.Object: {
                        var d = new Dictionary<BoxedValue, BoxedValue>();
                        foreach (var p in elem.EnumerateObject()) {
                            d.Add(BoxedValue.FromString(p.Name), JsonElementToBoxed(p.Value));
                        }
                        return BoxedValue.FromObject(d);
                    }
                case System.Text.Json.JsonValueKind.Array: {
                        var l = new List<BoxedValue>();
                        foreach (var v in elem.EnumerateArray()) {
                            l.Add(JsonElementToBoxed(v));
                        }
                        return BoxedValue.FromObject(l);
                    }
                case System.Text.Json.JsonValueKind.String:
                    return BoxedValue.FromString(elem.GetString() ?? string.Empty);
                case System.Text.Json.JsonValueKind.Number:
                    if (elem.TryGetInt64(out var i64)) {
                        if (i64 >= int.MinValue && i64 <= int.MaxValue) return BoxedValue.From((int)i64);
                        return BoxedValue.From(i64);
                    }
                    if (elem.TryGetDouble(out var dbl)) return BoxedValue.From(dbl);
                    return BoxedValue.FromString(elem.GetRawText());
                case System.Text.Json.JsonValueKind.True:
                    return BoxedValue.FromBool(true);
                case System.Text.Json.JsonValueKind.False:
                    return BoxedValue.FromBool(false);
                case System.Text.Json.JsonValueKind.Null:
                case System.Text.Json.JsonValueKind.Undefined:
                default:
                    return BoxedValue.NullObject;
            }
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
                sb.Append(Lib.AgentPlugin.TakeHelpSearchDebugInfo());
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
                sb.Append(Lib.AgentPlugin.TakeHelpSearchDebugInfo());
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
            m_ExecuteJavascriptInRendererApi = Marshal.GetDelegateForFunctionPointer<HostExecuteJavascriptInRendererDelegation>(hostApi.ExecuteJavascriptInRenderer);
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
            m_BrowserShowDevToolsApi = Marshal.GetDelegateForFunctionPointer<HostBrowserShowDevToolsDelegation>(hostApi.BrowserShowDevTools);
            m_BrowserCloseDevToolsApi = Marshal.GetDelegateForFunctionPointer<HostBrowserCloseDevToolsDelegation>(hostApi.BrowserCloseDevTools);
            m_BrowserHasDevToolsApi = Marshal.GetDelegateForFunctionPointer<HostBrowserHasDevToolsDelegation>(hostApi.BrowserHasDevTools);
            m_BrowserSendDevToolsMessageApi = Marshal.GetDelegateForFunctionPointer<HostBrowserSendDevToolsMessageDelegation>(hostApi.BrowserSendDevToolsMessage);
            m_BrowserExecuteDevToolsMethodApi = Marshal.GetDelegateForFunctionPointer<HostBrowserExecuteDevToolsMethodDelegation>(hostApi.BrowserExecuteDevToolsMethod);
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
            m_RequestSetUrlApi = Marshal.GetDelegateForFunctionPointer<HostRequestSetUrlDelegation>(hostApi.RequestSetUrl);
            m_RequestSetFlagsApi = Marshal.GetDelegateForFunctionPointer<HostRequestSetFlagsDelegation>(hostApi.RequestSetFlags);
            m_RequestSetFirstPartyForCookiesApi = Marshal.GetDelegateForFunctionPointer<HostRequestSetFirstPartyForCookiesDelegation>(hostApi.RequestSetFirstPartyForCookies);
            m_RequestSetHeaderByNameApi = Marshal.GetDelegateForFunctionPointer<HostRequestSetHeaderByNameDelegation>(hostApi.RequestSetHeaderByName);
            m_RequestRemoveHeaderByNameApi = Marshal.GetDelegateForFunctionPointer<HostRequestRemoveHeaderByNameDelegation>(hostApi.RequestRemoveHeaderByName);
            m_RequestSetHeaderMapApi = Marshal.GetDelegateForFunctionPointer<HostRequestSetHeaderMapDelegation>(hostApi.RequestSetHeaderMap);
            m_RequestSetReferrerApi = Marshal.GetDelegateForFunctionPointer<HostRequestSetReferrerDelegation>(hostApi.RequestSetReferrer);
            // CefResponse properties
            m_ResponseIsReadOnlyApi = Marshal.GetDelegateForFunctionPointer<HostResponseIsReadOnlyDelegation>(hostApi.ResponseIsReadOnly);
            m_ResponseGetStatusApi = Marshal.GetDelegateForFunctionPointer<HostResponseGetStatusDelegation>(hostApi.ResponseGetStatus);
            m_ResponseSetStatusApi = Marshal.GetDelegateForFunctionPointer<HostResponseSetStatusDelegation>(hostApi.ResponseSetStatus);
            m_ResponseGetStatusTextApi = Marshal.GetDelegateForFunctionPointer<HostResponseGetStatusTextDelegation>(hostApi.ResponseGetStatusText);
            m_ResponseSetStatusTextApi = Marshal.GetDelegateForFunctionPointer<HostResponseSetStatusTextDelegation>(hostApi.ResponseSetStatusText);
            m_ResponseGetMimeTypeApi = Marshal.GetDelegateForFunctionPointer<HostResponseGetMimeTypeDelegation>(hostApi.ResponseGetMimeType);
            m_ResponseSetMimeTypeApi = Marshal.GetDelegateForFunctionPointer<HostResponseSetMimeTypeDelegation>(hostApi.ResponseSetMimeType);
            m_ResponseGetCharsetApi = Marshal.GetDelegateForFunctionPointer<HostResponseGetCharsetDelegation>(hostApi.ResponseGetCharset);
            m_ResponseSetCharsetApi = Marshal.GetDelegateForFunctionPointer<HostResponseSetCharsetDelegation>(hostApi.ResponseSetCharset);
            m_ResponseGetUrlApi = Marshal.GetDelegateForFunctionPointer<HostResponseGetUrlDelegation>(hostApi.ResponseGetUrl);
            m_ResponseGetHeaderMapApi = Marshal.GetDelegateForFunctionPointer<HostResponseGetHeaderMapDelegation>(hostApi.ResponseGetHeaderMap);
            m_ResponseGetHeaderByNameApi = Marshal.GetDelegateForFunctionPointer<HostResponseGetHeaderByNameDelegation>(hostApi.ResponseGetHeaderByName);
            m_ResponseSetHeaderByNameApi = Marshal.GetDelegateForFunctionPointer<HostResponseSetHeaderByNameDelegation>(hostApi.ResponseSetHeaderByName);
            m_ResponseRemoveHeaderByNameApi = Marshal.GetDelegateForFunctionPointer<HostResponseRemoveHeaderByNameDelegation>(hostApi.ResponseRemoveHeaderByName);
            m_ResponseSetHeaderMapApi = Marshal.GetDelegateForFunctionPointer<HostResponseSetHeaderMapDelegation>(hostApi.ResponseSetHeaderMap);
            m_ResponseGetErrorApi = Marshal.GetDelegateForFunctionPointer<HostResponseGetErrorDelegation>(hostApi.ResponseGetError);
            m_ResponseSetErrorApi = Marshal.GetDelegateForFunctionPointer<HostResponseSetErrorDelegation>(hostApi.ResponseSetError);
            m_ResponseSetUrlApi = Marshal.GetDelegateForFunctionPointer<HostResponseSetUrlDelegation>(hostApi.ResponseSetUrl);
            // Heartbeat control
            m_SetHeartbeatIntervalApi = Marshal.GetDelegateForFunctionPointer<HostSetHeartbeatIntervalDelegation>(hostApi.SetHeartbeatInterval);
            // Generic async callback completion
            m_NativeCallbackCompleteApi = Marshal.GetDelegateForFunctionPointer<HostNativeCallbackCompleteDelegation>(hostApi.NativeCallbackComplete);
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
        public string ExecuteJavascriptInRenderer(string code)
        {
            if (m_ExecuteJavascriptInRendererApi == null) {
                return "";
            }
            IntPtr resultPtr = m_ExecuteJavascriptInRendererApi.Invoke(code, Browser, Frame);
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

        public BrowserProxy? GetBrowser()
        {
            if (s_Browser != IntPtr.Zero) {
                return new BrowserProxy(s_Browser, this);
            }
            return null;
        }
        public FrameProxy? GetFrame()
        {
            if (s_Frame != IntPtr.Zero) {
                return new FrameProxy(s_Frame, this);
            }
            return null;
        }

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

        internal static StringBuilder ApiErrorInfo {
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
        IEnumerable<string> INativeApi.GetHelpDocs()
        {
            return BatchCommand.BatchScript.ApiDocs
                .Concat(BatchCommand.BatchScript.UserApiDocs)
                .Select(pair => string.Format("{0}: {1}", pair.Key, pair.Value))
                .ToArray();
        }

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
        string IDslEngine.ExecuteMetaDslScript(string script, int maxResultSize, out bool hasError) => CefDotnetAppApi.ExecuteMetaDslScript(script, maxResultSize, out hasError);
        void IDslEngine.Register(string name, string doc, IExpressionFactory factory) => BatchCommand.BatchScript.Register(name, doc, factory);
        void IDslEngine.Register(string name, string doc, bool addToUserApiDoc, IExpressionFactory factory) => BatchCommand.BatchScript.Register(name, doc, addToUserApiDoc, factory);

        internal static nint Browser {
            get => s_Browser;
            set => s_Browser = value;
        }
        internal static nint Frame {
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

        // --- DevTools host actions ---
        public void BrowserShowDevTools(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserShowDevToolsApi == null) return;
            m_BrowserShowDevToolsApi(browser, 0, 0, 0);
        }
        public void BrowserShowDevTools(IntPtr browser, int inspectX, int inspectY)
        {
            if (browser == IntPtr.Zero || m_BrowserShowDevToolsApi == null) return;
            m_BrowserShowDevToolsApi(browser, inspectX, inspectY, 1);
        }
        public void BrowserCloseDevTools(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserCloseDevToolsApi == null) return;
            m_BrowserCloseDevToolsApi(browser);
        }
        public bool BrowserHasDevTools(IntPtr browser)
        {
            if (browser == IntPtr.Zero || m_BrowserHasDevToolsApi == null) return false;
            return m_BrowserHasDevToolsApi(browser) != 0;
        }
        public bool BrowserSendDevToolsMessage(IntPtr browser, byte[] messageBytes)
        {
            if (browser == IntPtr.Zero || m_BrowserSendDevToolsMessageApi == null) return false;
            if (messageBytes == null || messageBytes.Length == 0) return false;
            var handle = GCHandle.Alloc(messageBytes, GCHandleType.Pinned);
            try {
                return m_BrowserSendDevToolsMessageApi(browser, handle.AddrOfPinnedObject(), messageBytes.Length) != 0;
            }
            finally {
                handle.Free();
            }
        }
        public int BrowserExecuteDevToolsMethod(IntPtr browser, int messageId, string method, string? paramsJson)
        {
            if (browser == IntPtr.Zero || m_BrowserExecuteDevToolsMethodApi == null) return 0;
            if (string.IsNullOrEmpty(method)) return 0;
            return m_BrowserExecuteDevToolsMethodApi(browser, messageId, method, paramsJson);
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
        // CefRequest setters (no-op when request is read-only; DSL side should
        // check RequestIsReadOnly first).
        public void RequestSetUrl(IntPtr request, string? url)
        {
            if (request == IntPtr.Zero) return;
            m_RequestSetUrlApi?.Invoke(request, url);
        }
        public void RequestSetFlags(IntPtr request, int flags)
        {
            if (request == IntPtr.Zero) return;
            m_RequestSetFlagsApi?.Invoke(request, flags);
        }
        public void RequestSetFirstPartyForCookies(IntPtr request, string? url)
        {
            if (request == IntPtr.Zero) return;
            m_RequestSetFirstPartyForCookiesApi?.Invoke(request, url);
        }
        public void RequestSetHeaderByName(IntPtr request, string name, string? value, bool overwrite)
        {
            if (request == IntPtr.Zero || string.IsNullOrEmpty(name)) return;
            m_RequestSetHeaderByNameApi?.Invoke(request, name, value, overwrite ? 1 : 0);
        }
        public void RequestRemoveHeaderByName(IntPtr request, string name)
        {
            if (request == IntPtr.Zero || string.IsNullOrEmpty(name)) return;
            m_RequestRemoveHeaderByNameApi?.Invoke(request, name);
        }
        public void RequestSetHeaderMap(IntPtr request, string? header_map_str)
        {
            if (request == IntPtr.Zero) return;
            m_RequestSetHeaderMapApi?.Invoke(request, header_map_str);
        }
        public void RequestSetReferrer(IntPtr request, string? referrer_url, int referrer_policy)
        {
            if (request == IntPtr.Zero) return;
            m_RequestSetReferrerApi?.Invoke(request, referrer_url, referrer_policy);
        }
        // CefResponse properties
        public bool ResponseIsReadOnly(IntPtr response)
        {
            if (response == IntPtr.Zero || m_ResponseIsReadOnlyApi == null) return true;
            return m_ResponseIsReadOnlyApi(response);
        }
        public int ResponseGetStatus(IntPtr response)
        {
            if (response == IntPtr.Zero || m_ResponseGetStatusApi == null) return 0;
            return m_ResponseGetStatusApi(response);
        }
        public void ResponseSetStatus(IntPtr response, int status)
        {
            if (response == IntPtr.Zero) return;
            m_ResponseSetStatusApi?.Invoke(response, status);
        }
        public string ResponseGetStatusText(IntPtr response)
        {
            if (response == IntPtr.Zero || m_ResponseGetStatusTextApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_ResponseGetStatusTextApi(response));
        }
        public void ResponseSetStatusText(IntPtr response, string? status_text)
        {
            if (response == IntPtr.Zero) return;
            m_ResponseSetStatusTextApi?.Invoke(response, status_text);
        }
        public string ResponseGetMimeType(IntPtr response)
        {
            if (response == IntPtr.Zero || m_ResponseGetMimeTypeApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_ResponseGetMimeTypeApi(response));
        }
        public void ResponseSetMimeType(IntPtr response, string? mime_type)
        {
            if (response == IntPtr.Zero) return;
            m_ResponseSetMimeTypeApi?.Invoke(response, mime_type);
        }
        public string ResponseGetCharset(IntPtr response)
        {
            if (response == IntPtr.Zero || m_ResponseGetCharsetApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_ResponseGetCharsetApi(response));
        }
        public void ResponseSetCharset(IntPtr response, string? charset)
        {
            if (response == IntPtr.Zero) return;
            m_ResponseSetCharsetApi?.Invoke(response, charset);
        }
        public string ResponseGetUrl(IntPtr response)
        {
            if (response == IntPtr.Zero || m_ResponseGetUrlApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_ResponseGetUrlApi(response));
        }
        public string ResponseGetHeaderMap(IntPtr response)
        {
            if (response == IntPtr.Zero || m_ResponseGetHeaderMapApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_ResponseGetHeaderMapApi(response));
        }
        public string ResponseGetHeaderByName(IntPtr response, string name)
        {
            if (response == IntPtr.Zero || m_ResponseGetHeaderByNameApi == null) return string.Empty;
            return ReadAndFreeNativeString(m_ResponseGetHeaderByNameApi(response, name));
        }
        public void ResponseSetHeaderByName(IntPtr response, string name, string? value, bool overwrite)
        {
            if (response == IntPtr.Zero) return;
            m_ResponseSetHeaderByNameApi?.Invoke(response, name, value, overwrite ? 1 : 0);
        }
        public void ResponseRemoveHeaderByName(IntPtr response, string name)
        {
            if (response == IntPtr.Zero) return;
            m_ResponseRemoveHeaderByNameApi?.Invoke(response, name);
        }
        public void ResponseSetHeaderMap(IntPtr response, string? header_map_str)
        {
            if (response == IntPtr.Zero) return;
            m_ResponseSetHeaderMapApi?.Invoke(response, header_map_str);
        }
        public int ResponseGetError(IntPtr response)
        {
            if (response == IntPtr.Zero || m_ResponseGetErrorApi == null) return 0;
            return m_ResponseGetErrorApi(response);
        }
        public void ResponseSetError(IntPtr response, int error)
        {
            if (response == IntPtr.Zero) return;
            m_ResponseSetErrorApi?.Invoke(response, error);
        }
        public void ResponseSetUrl(IntPtr response, string? url)
        {
            if (response == IntPtr.Zero) return;
            m_ResponseSetUrlApi?.Invoke(response, url);
        }
        public void SetHeartbeatInterval(int intervalMs)
        {
            m_SetHeartbeatIntervalApi?.Invoke(intervalMs);
        }
        // Completes a CEF async callback that managed code took over (JS dialog,
        // deferred resource load, cefQuery, ...). Safe to call from any thread and
        // safe to call twice: the native side ignores unknown handles.
        public bool NativeCallbackComplete(long handle, bool ok, string? data, int code)
        {
            if (handle == 0 || m_NativeCallbackCompleteApi == null) return false;
            return m_NativeCallbackCompleteApi(handle, ok ? 1 : 0, data, code) != 0;
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
            if (s_Browser == IntPtr.Zero) {
                s_Browser = Lib.GetBrowsersFirstValid();
                s_Frame = BrowserGetMainFrame(s_Browser);
            }
            if (s_Browser == IntPtr.Zero) {
                Lib.NativeLogNoLock($"[csharp] Error HandleAllQueues, browser is null");
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
        private HostExecuteJavascriptInRendererDelegation? m_ExecuteJavascriptInRendererApi;
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
        // DevTools host actions
        private HostBrowserShowDevToolsDelegation? m_BrowserShowDevToolsApi;
        private HostBrowserCloseDevToolsDelegation? m_BrowserCloseDevToolsApi;
        private HostBrowserHasDevToolsDelegation? m_BrowserHasDevToolsApi;
        private HostBrowserSendDevToolsMessageDelegation? m_BrowserSendDevToolsMessageApi;
        private HostBrowserExecuteDevToolsMethodDelegation? m_BrowserExecuteDevToolsMethodApi;
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
        private HostRequestSetUrlDelegation? m_RequestSetUrlApi;
        private HostRequestSetFlagsDelegation? m_RequestSetFlagsApi;
        private HostRequestSetFirstPartyForCookiesDelegation? m_RequestSetFirstPartyForCookiesApi;
        private HostRequestSetHeaderByNameDelegation? m_RequestSetHeaderByNameApi;
        private HostRequestRemoveHeaderByNameDelegation? m_RequestRemoveHeaderByNameApi;
        private HostRequestSetHeaderMapDelegation? m_RequestSetHeaderMapApi;
        private HostRequestSetReferrerDelegation? m_RequestSetReferrerApi;
        // CefResponse properties
        private HostResponseIsReadOnlyDelegation? m_ResponseIsReadOnlyApi;
        private HostResponseGetStatusDelegation? m_ResponseGetStatusApi;
        private HostResponseSetStatusDelegation? m_ResponseSetStatusApi;
        private HostResponseGetStatusTextDelegation? m_ResponseGetStatusTextApi;
        private HostResponseSetStatusTextDelegation? m_ResponseSetStatusTextApi;
        private HostResponseGetMimeTypeDelegation? m_ResponseGetMimeTypeApi;
        private HostResponseSetMimeTypeDelegation? m_ResponseSetMimeTypeApi;
        private HostResponseGetCharsetDelegation? m_ResponseGetCharsetApi;
        private HostResponseSetCharsetDelegation? m_ResponseSetCharsetApi;
        private HostResponseGetUrlDelegation? m_ResponseGetUrlApi;
        private HostResponseGetHeaderMapDelegation? m_ResponseGetHeaderMapApi;
        private HostResponseGetHeaderByNameDelegation? m_ResponseGetHeaderByNameApi;
        private HostResponseSetHeaderByNameDelegation? m_ResponseSetHeaderByNameApi;
        private HostResponseRemoveHeaderByNameDelegation? m_ResponseRemoveHeaderByNameApi;
        private HostResponseSetHeaderMapDelegation? m_ResponseSetHeaderMapApi;
        private HostResponseGetErrorDelegation? m_ResponseGetErrorApi;
        private HostResponseSetErrorDelegation? m_ResponseSetErrorApi;
        private HostResponseSetUrlDelegation? m_ResponseSetUrlApi;
        private HostSetHeartbeatIntervalDelegation? m_SetHeartbeatIntervalApi;
        private HostNativeCallbackCompleteDelegation? m_NativeCallbackCompleteApi;

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

        // --- DevTools ---
        public void ShowDevTools() => m_Api.BrowserShowDevTools(m_Browser);
        public void ShowDevTools(int inspectX, int inspectY) => m_Api.BrowserShowDevTools(m_Browser, inspectX, inspectY);
        public void CloseDevTools() => m_Api.BrowserCloseDevTools(m_Browser);
        public bool HasDevTools => m_Api.BrowserHasDevTools(m_Browser);

        public bool SendDevToolsMessage(byte[] messageBytes) => m_Api.BrowserSendDevToolsMessage(m_Browser, messageBytes);
        public bool SendDevToolsMessage(string messageJson)
        {
            if (string.IsNullOrEmpty(messageJson)) return false;
            return m_Api.BrowserSendDevToolsMessage(m_Browser, System.Text.Encoding.UTF8.GetBytes(messageJson));
        }

        public int ExecuteDevToolsMethod(string method, string? paramsJson = null, int messageId = 0)
        {
            return m_Api.BrowserExecuteDevToolsMethod(m_Browser, messageId, method, paramsJson);
        }

        /// <summary>
        /// Convenience wrapper: enable Network domain and toggle CSP bypass.
        /// Requires the browser DevTools agent to be attached; call again after navigation if needed.
        /// </summary>
        public void SetCspBypass(bool enable)
        {
            ExecuteDevToolsMethod("Network.enable");
            ExecuteDevToolsMethod("Page.setBypassCSP", enable ? "{\"enabled\":true}" : "{\"enabled\":false}");
        }

        // DevTools observer callbacks are dispatched directly to DSL (see Lib.OnDevTools*).
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
        public string Url {
            get => m_Api.RequestGetUrl(m_Request);
            set => m_Api.RequestSetUrl(m_Request, value);
        }
        public string Method => m_Api.RequestGetMethod(m_Request);
        public string ReferrerUrl => m_Api.RequestGetReferrerUrl(m_Request);
        public int ReferrerPolicy => m_Api.RequestGetReferrerPolicy(m_Request);
        /// <summary>
        /// Get header map as newline-separated "key:value" pairs (split by first ':').
        /// </summary>
        public string HeaderMap => m_Api.RequestGetHeaderMap(m_Request);
        public string GetHeaderByName(string name) => m_Api.RequestGetHeaderByName(m_Request, name);
        public int Flags {
            get => m_Api.RequestGetFlags(m_Request);
            set => m_Api.RequestSetFlags(m_Request, value);
        }
        public string FirstPartyForCookies {
            get => m_Api.RequestGetFirstPartyForCookies(m_Request);
            set => m_Api.RequestSetFirstPartyForCookies(m_Request, value);
        }
        public int ResourceType => m_Api.RequestGetResourceType(m_Request);
        public int TransitionType => m_Api.RequestGetTransitionType(m_Request);
        public ulong Identifier => m_Api.RequestGetIdentifier(m_Request);
        /// <summary>
        /// Replace the entire header map from newline-separated "key:value" pairs.
        /// No-op if the request is read-only (check IsReadOnly first).
        /// </summary>
        public void SetHeaderMap(string? headerMapStr) => m_Api.RequestSetHeaderMap(m_Request, headerMapStr);
        /// <summary>
        /// Set a header. overwrite=true replaces all existing values for the name.
        /// No-op if the request is read-only. To delete a header use RemoveHeaderByName,
        /// not SetHeaderByName with an empty value (empty value sets an empty header).
        /// </summary>
        public void SetHeaderByName(string name, string? value, bool overwrite = true) => m_Api.RequestSetHeaderByName(m_Request, name, value, overwrite);
        /// <summary>
        /// Remove all values for the given header name (case-insensitive).
        /// No-op if the request is read-only.
        /// </summary>
        public void RemoveHeaderByName(string name) => m_Api.RequestRemoveHeaderByName(m_Request, name);
        /// <summary>
        /// Set the Referer via CEF's SetReferrer (goes through the browser's referrer
        /// policy pipeline). Prefer this over SetHeaderByName("Referer", ...) when the
        /// referrer policy should be honored. policy defaults to REFERRER_POLICY_DEFAULT
        /// (0). No-op if the request is read-only.
        /// </summary>
        public void SetReferrer(string? url, int policy = 0) => m_Api.RequestSetReferrer(m_Request, url, policy);
    }

    /// <summary>
    /// Proxy wrapper for a native CefResponse pointer.
    /// It is writable only during OnBeforeResourceResponse. Other resource
    /// callbacks receive a read-only response, so write APIs are ignored by CEF.
    /// </summary>
    public class CefResponseProxy
    {
        private readonly IntPtr m_Response;
        private readonly NativeApi m_Api;

        public CefResponseProxy(IntPtr response, NativeApi api)
        {
            m_Response = response;
            m_Api = api;
        }

        public IntPtr NativePtr => m_Response;
        public bool IsReadOnly => m_Api.ResponseIsReadOnly(m_Response);
        public int Status {
            get => m_Api.ResponseGetStatus(m_Response);
            set => m_Api.ResponseSetStatus(m_Response, value);
        }
        public string StatusText {
            get => m_Api.ResponseGetStatusText(m_Response);
            set => m_Api.ResponseSetStatusText(m_Response, value);
        }
        public string MimeType {
            get => m_Api.ResponseGetMimeType(m_Response);
            set => m_Api.ResponseSetMimeType(m_Response, value);
        }
        public string Charset {
            get => m_Api.ResponseGetCharset(m_Response);
            set => m_Api.ResponseSetCharset(m_Response, value);
        }
        public string Url {
            get => m_Api.ResponseGetUrl(m_Response);
            set => m_Api.ResponseSetUrl(m_Response, value);
        }
        /// <summary>
        /// CEF error code carried by the response (cef_errorcode_t). ERR_NONE (0) when
        /// there is no error. Setting a non-zero value marks the response as failed
        /// (used when proxying to reflect an upstream network error).
        /// </summary>
        public int Error {
            get => m_Api.ResponseGetError(m_Response);
            set => m_Api.ResponseSetError(m_Response, value);
        }
        /// <summary>
        /// Get header map as newline-separated "key:value" pairs (split by first ':').
        /// </summary>
        public string HeaderMap => m_Api.ResponseGetHeaderMap(m_Response);
        /// <summary>
        /// Replace the entire header map from newline-separated "key:value" pairs.
        /// </summary>
        public void SetHeaderMap(string? headerMapStr) => m_Api.ResponseSetHeaderMap(m_Response, headerMapStr);
        public string GetHeaderByName(string name) => m_Api.ResponseGetHeaderByName(m_Response, name);
        /// <summary>
        /// Set a header. overwrite=true replaces all existing values for the name.
        /// </summary>
        public void SetHeaderByName(string name, string? value, bool overwrite = true) => m_Api.ResponseSetHeaderByName(m_Response, name, value, overwrite);
        /// <summary>
        /// Remove all values for the given header name (case-insensitive).
        /// </summary>
        public void RemoveHeaderByName(string name) => m_Api.ResponseRemoveHeaderByName(m_Response, name);
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
        public string Program {
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

        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnInitDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string cmd_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, int process_type, [MarshalAs(UnmanagedType.LPUTF8Str)] string app_dir, [MarshalAs(UnmanagedType.U1)] bool is_mac);
        public delegate void OnFinalizeDelegation();
        public delegate void OnBrowserInitDelegation(IntPtr browser);
        public delegate void OnBrowserFinalizeDelegation(IntPtr browser);
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnBrowserHotReloadCopyFilesDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string url);
        public delegate void OnBrowserHotReloadCompletedDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnBrowserCefQueryDelegation(IntPtr browser, IntPtr frame, long query_id, [MarshalAs(UnmanagedType.LPUTF8Str)] string request, [MarshalAs(UnmanagedType.U1)] bool persistent, long handle, ref int out_result);
        public delegate void OnRendererInitDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url);
        public delegate void OnRendererFinalizeDelegation(IntPtr browser, IntPtr frame);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnLoadingStateChangeDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, [MarshalAs(UnmanagedType.U1)] bool is_loading, [MarshalAs(UnmanagedType.U1)] bool can_go_back, [MarshalAs(UnmanagedType.U1)] bool can_go_forward);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnBeforeCommandLineProcessingDelegation(int process_type, IntPtr command_line);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnBeforeChildProcessLaunchDelegation(int process_type, IntPtr command_line);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnAlreadyRunningAppRelaunchDelegation(IntPtr command_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string current_directory);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnLoadErrorDelegation(IntPtr browser, IntPtr frame, int error_code, [MarshalAs(UnmanagedType.LPUTF8Str)] string error_text, [MarshalAs(UnmanagedType.LPUTF8Str)] string failed_url);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnRenderProcessTerminatedDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string startup_url, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, int status, int error_code, [MarshalAs(UnmanagedType.LPUTF8Str)] string error_string, IntPtr reload_url, ref int reload_url_size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnLoadStartDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, int transition_type, [MarshalAs(UnmanagedType.U1)] bool is_main);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnLoadEndDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, int http_status_code, [MarshalAs(UnmanagedType.U1)] bool inject_all_frame, [MarshalAs(UnmanagedType.U1)] bool is_main, IntPtr js_code, ref int code_size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnGetAuthCredentialsDelegation([MarshalAs(UnmanagedType.U1)] bool is_proxy, [MarshalAs(UnmanagedType.LPUTF8Str)] string host, int port, [MarshalAs(UnmanagedType.LPUTF8Str)] string realm, [MarshalAs(UnmanagedType.LPUTF8Str)] string scheme, [MarshalAs(UnmanagedType.LPUTF8Str)] string origin_url, IntPtr username, ref int username_size, IntPtr password, ref int password_size, long handle, int attempt);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnRequestMediaAccessPermissionDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string requesting_origin, uint requested_permissions, [MarshalAs(UnmanagedType.U1)] bool menu_disabled, ref uint allowed_permissions);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnCertificateErrorDelegation(int cert_error, [MarshalAs(UnmanagedType.LPUTF8Str)] string request_url, ref int out_action);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnRendererLoadStartDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, int transition_type, [MarshalAs(UnmanagedType.U1)] bool is_main);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnRendererLoadEndDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, int http_status_code, [MarshalAs(UnmanagedType.U1)] bool is_main, IntPtr js_code, ref int code_size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnRendererLoadingStateChangeDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, [MarshalAs(UnmanagedType.U1)] bool is_loading, [MarshalAs(UnmanagedType.U1)] bool can_go_back, [MarshalAs(UnmanagedType.U1)] bool can_go_forward);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnRendererLoadErrorDelegation(IntPtr browser, IntPtr frame, int error_code, [MarshalAs(UnmanagedType.LPUTF8Str)] string error_text, [MarshalAs(UnmanagedType.LPUTF8Str)] string failed_url);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnReceiveCefMessageDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, IntPtr args, int argCount, IntPtr browser, IntPtr frame, int source_process_id);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnExecuteMetaDSLDelegation(IntPtr args, int argCount, IntPtr resultStr, ref int resultSize, IntPtr browser, IntPtr frame);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnBeforeBrowseDelegation(IntPtr browser, IntPtr frame, IntPtr request, [MarshalAs(UnmanagedType.U1)] bool user_gesture, [MarshalAs(UnmanagedType.U1)] bool is_redirect, IntPtr out_return_value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnBeforeResourceLoadDelegation(IntPtr browser, IntPtr frame, IntPtr request, long handle, ref int out_return_value);
        // JS dialog hook (browser process UI thread). Returns the decision:
        // 0=CEF default dialog, 1=custom dialog, 2=suppress, 3=script owned.
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int OnJsDialogDelegation(IntPtr browser, int dialog_type, [MarshalAs(UnmanagedType.LPUTF8Str)] string origin_url, [MarshalAs(UnmanagedType.LPUTF8Str)] string message_text, [MarshalAs(UnmanagedType.LPUTF8Str)] string default_prompt_text, long handle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnHeartBeatDelegation(int process_type, float delta_time);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnCallMetaDSLDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func_name, IntPtr args, int argCount, IntPtr resultStr, ref int resultSize, IntPtr browser, IntPtr frame);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnConsoleLogDelegation(IntPtr browser, int level, [MarshalAs(UnmanagedType.LPUTF8Str)] string message, [MarshalAs(UnmanagedType.LPUTF8Str)] string source, int line, ref int maxLogSize);

        // Resource interception callbacks (invoked on browser process IO thread).
        // OnResourceResponseFilter: inspection point (GetResourceResponseFilter).
        // request/response are the actual CEF request and upstream response
        // (read-only inspection). Return true to register MyResponseFilter for
        // body filtering; out_replace_content false skips the body filter.
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnResourceResponseFilterDelegation(IntPtr browser, IntPtr frame, IntPtr request, IntPtr response, IntPtr out_replace_content);
        // Body filter: streams body chunks through C# for transformation.
        // Returns true if DSL handled the chunk (use DSL's outputs), false to
        // pass through unchanged. out_status receives the filter status (0=DONE,
        // 1=NEED_MORE_DATA, 2=ERROR, matches cef_response_filter_status_t).
        // out_data_in_read / out_data_out_written receive consumed/written byte
        // counts (ref int, matches on_before_resource_load_fn's int* pattern).
        // No browser/frame: CefResourceHandler::Read / CefResponseFilter::Filter
        // signatures do not carry them; body filter is a pure data transform.
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnResponseContentFilterDelegation(IntPtr data_in, int data_in_size, IntPtr data_out, int data_out_size, ref int out_data_in_read, ref int out_data_out_written, ref int out_status);
        // OnResourceRedirect: called when a resource request is redirected.
        // DSL can inspect the redirect and optionally provide a replacement URL.
        // Returns true if a new URL is provided (written to out_url as UTF-8).
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnResourceRedirectDelegation(IntPtr browser, IntPtr frame, IntPtr request, IntPtr response, [MarshalAs(UnmanagedType.LPUTF8Str)] string new_url, IntPtr out_url, ref int out_url_size);
        // OnBeforeResourceResponse: response is writable only during this
        // callback, before CEF processes the received response headers.
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void OnBeforeResourceResponseDelegation(IntPtr browser, IntPtr frame, IntPtr request, IntPtr response);
        // OnResourceLoadComplete: status is cef_urlrequest_status_t and the
        // length is the number of response bytes actually read.
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void OnResourceLoadCompleteDelegation(IntPtr browser, IntPtr frame, IntPtr request, IntPtr response, int status, long received_content_length);
        // OnProtocolExecution: return true only when the callback supplies an
        // allow value through the one-byte C++ bool pointer.
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool OnProtocolExecutionDelegation(IntPtr browser, IntPtr frame, IntPtr request, IntPtr out_allow_os_execution);

        // DevTools observer callbacks (invoked on browser process UI thread).
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int OnDevToolsMessageDelegation(IntPtr browser, IntPtr msg, int size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnDevToolsMethodResultDelegation(IntPtr browser, int message_id, int success, IntPtr result, int size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnDevToolsEventDelegation(IntPtr browser, [MarshalAs(UnmanagedType.LPUTF8Str)] string method, IntPtr @params, int size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnDevToolsAgentAttachedDelegation(IntPtr browser);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnDevToolsAgentDetachedDelegation(IntPtr browser);

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

                    if ((int)CefProcessType.RendererProcess == process_type) {
                        s_InitialDslScriptFile = "script_renderer.dsl";
                    }
                    else {
                        s_InitialDslScriptFile = "script.dsl";
                    }
                    s_InitialProjectIdentity = string.Empty;

                    if (TryGetSwitchValueFromRawCommandLine(cmd_line, "metadsl", out string switchValue)) {
                        var vals = switchValue.Split(",", StringSplitOptions.RemoveEmptyEntries);
                        if (vals.Length == 2) {
                            s_MetaDslSwitch = switchValue;
                            NativeLogNoLock(string.Format("[csharp] parse --metadsl:{0}", s_MetaDslSwitch));

                            if ((int)CefProcessType.RendererProcess == process_type) {
                                s_InitialDslScriptFile = vals[1];
                            }
                            else {
                                s_InitialDslScriptFile = vals[0];
                            }
                        }
                        else {
                            NativeLogNoLock(string.Format("[csharp] parse --metadsl:{0} error, the value must adhere to the format 'xxx.dsl,xxx_renderer.dsl'", switchValue));
                        }
                    }
                    if (TryGetSwitchValueFromRawCommandLine(cmd_line, "projectidentity", out string prjSwitchValue)) {
                        s_ProjectSwitch = prjSwitchValue;
                        NativeLogNoLock(string.Format("[csharp] parse --projectidentity:{0}", s_ProjectSwitch));

                        s_InitialProjectIdentity = s_ProjectSwitch;
                    }
                    s_DslScriptFile = s_InitialDslScriptFile;
                    TryLoadDSL();
                    BoxedValue r = BatchCommand.BatchScript.Call("on_init");
                    CheckDslError();
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
                    CheckDslError();
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
            finally {
                AgentFrameworkService.Instance.ShutdownPlugin();

                NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
                NativeApi.LastSourceProcessId = -1;
            }
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
                    CheckDslError();
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
                    CheckDslError();
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
            finally {
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
        }

        // --- DevTools observer callbacks (browser process UI thread) ---
        // All callbacks dispatch directly to DSL (no C# event layer).
        // Keep handlers fast; heavy work should be dispatched off-thread by DSL side.

        internal static int OnDevToolsMessage(IntPtr browser, IntPtr msg, int size)
        {
            NativeApi.SetContext(browser, IntPtr.Zero);
            try {
                if (s_NativeApi != null) {
                    TryLoadDSL();
                    byte[] buf = ReadNativeBytes(msg, size);
                    BoxedValue r = BatchCommand.BatchScript.Call("on_dev_tools_message",
                        BoxedValue.FromObject(buf));
                    CheckDslError();
                    if (!r.IsNullObject) return r.GetInt();
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] OnDevToolsMessage Exception:" + e.Message + "\n" + e.StackTrace);
            }
            return 0;
        }

        internal static void OnDevToolsMethodResult(IntPtr browser, int message_id, int success, IntPtr result, int size)
        {
            NativeApi.SetContext(browser, IntPtr.Zero);
            try {
                if (s_NativeApi != null) {
                    TryLoadDSL();
                    byte[] buf = ReadNativeBytes(result, size);
                    BatchCommand.BatchScript.Call("on_dev_tools_method_result",
                        BoxedValue.From(message_id),
                        BoxedValue.FromBool(success != 0),
                        BoxedValue.FromObject(buf));
                    CheckDslError();
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] OnDevToolsMethodResult Exception:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static void OnDevToolsEvent(IntPtr browser, string method, IntPtr @params, int size)
        {
            NativeApi.SetContext(browser, IntPtr.Zero);
            try {
                if (s_NativeApi != null) {
                    TryLoadDSL();
                    byte[] buf = ReadNativeBytes(@params, size);
                    BatchCommand.BatchScript.Call("on_dev_tools_event",
                        BoxedValue.FromString(method ?? string.Empty),
                        BoxedValue.FromObject(buf));
                    CheckDslError();
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] OnDevToolsEvent Exception:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static void OnDevToolsAgentAttached(IntPtr browser)
        {
            NativeApi.SetContext(browser, IntPtr.Zero);
            try {
                if (s_NativeApi != null) {
                    TryLoadDSL();
                    BatchCommand.BatchScript.Call("on_dev_tools_agent_attached");
                    CheckDslError();
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] OnDevToolsAgentAttached Exception:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static void OnDevToolsAgentDetached(IntPtr browser)
        {
            NativeApi.SetContext(browser, IntPtr.Zero);
            try {
                if (s_NativeApi != null) {
                    TryLoadDSL();
                    BatchCommand.BatchScript.Call("on_dev_tools_agent_detached");
                    CheckDslError();
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] OnDevToolsAgentDetached Exception:" + e.Message + "\n" + e.StackTrace);
            }
        }

        private static byte[] ReadNativeBytes(IntPtr ptr, int size)
        {
            if (ptr == IntPtr.Zero || size <= 0) return Array.Empty<byte>();
            byte[] buf = new byte[size];
            Marshal.Copy(ptr, buf, 0, size);
            return buf;
        }

        internal static bool OnBrowserHotReloadCopyFiles(string url)
        {
            NativeLogNoLock("[csharp] Browser Hot Reload Copy Files, url: " + url);

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_browser_hot_reload_copyfiles"));

                if (null != s_NativeApi) {
                    TryLoadDSL();
                    BoxedValue r = BatchCommand.BatchScript.Call("on_browser_hot_reload_copyfiles", url);
                    CheckDslError();
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
                    CheckDslError();
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
        }

        //Note: this method will be called on the browser process main/UI thread.
        internal static bool OnBrowserCefQuery(IntPtr browser, IntPtr frame, long query_id, string request, bool persistent, long handle, ref int out_result)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock(string.Format("[csharp] Browser Cef Query: query_id={0}, request={1}, persistent={2}, handle={3}", query_id, GetStringInLength(request), persistent, handle));

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_browser_cef_query"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(query_id));
                    vargs.Add(BoxedValue.FromString(request));
                    vargs.Add(BoxedValue.FromBool(persistent));
                    vargs.Add(BoxedValue.From(handle));
                    BoxedValue r = BatchCommand.BatchScript.Call("on_browser_cef_query", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();
                    // Return value convention: (handled, result_int)
                    // handled == true  -> the script took the query over and must
                    //   later call complete_native_callback($handle, ok, response
                    //   [, error_code]); nothing reaches the page until then.
                    // handled == false -> answered synchronously, out_result is the
                    //   result code (0 = Success, non-zero = Failure with that code).
                    // Returning false is also the degraded path on any failure, so
                    // a broken script cannot leave the query hanging.
                    if (r.Type == (int)BoxedValue.c_Tuple2Type) {
                        var tuple2 = r.GetTuple2();
                        if (null != tuple2) {
                            bool takenOver = tuple2.Item1.GetBool();
                            if (!takenOver) {
                                out_result = tuple2.Item2.GetInt();
                            }
                            return takenOver;
                        }
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
            finally {
                NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
            }
            // Nothing usable came back: answer synchronously with a failure so the
            // page's onFailure runs instead of hanging.
            out_result = -1;
            return false;
        }

        internal static void OnRendererInit(IntPtr browser, IntPtr frame, string url)
        {
            s_MainThreadId = Thread.CurrentThread.ManagedThreadId;
            AgentFrameworkService.Instance.SetMainThreadId(s_MainThreadId);
            NativeApi.SetContext(browser, frame);
            if (string.IsNullOrEmpty(s_StartupUrl)) {
                s_StartupUrl = url;
            }

            // Track main-frame browser id for renderer process. Only the id
            // is stored; the native ref map holds the CefRefPtr and the main
            // frame is retrieved on demand via BrowserGetMainFrame.
            if (s_NativeApi != null && s_NativeApi.FrameIsMain(frame)) {
                int browserId = s_NativeApi.BrowserGetId(browser);
                if (browserId > 0) {
                    s_RendererBrowserIds.Add(browserId);
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
                    CheckDslError();
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

            // Untrack main-frame browser id for renderer process. Only untrack when
            // the finalized frame is the main frame; navigation-driven sub frame
            // finalize should not remove the browser id.
            if (s_NativeApi != null && s_NativeApi.FrameIsMain(frame)) {
                int browserId = s_NativeApi.BrowserGetId(browser);
                if (browserId > 0 && s_RendererBrowserIds.Remove(browserId)) {
                    NativeLogNoLock($"[csharp] Renderer browser untracked: id={browserId}");
                }
            }

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_renderer_finalize"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BoxedValue r = BatchCommand.BatchScript.Call("on_renderer_finalize");
                    CheckDslError();
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
            finally {
                NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
                NativeApi.LastSourceProcessId = -1;
            }
        }

        internal static void OnLoadStart(IntPtr browser, IntPtr frame, string url, int transition_type, bool is_main)
        {
            NativeApi.SetContext(browser, frame);
            if (string.IsNullOrEmpty(s_StartupUrl)) {
                if (is_main) {
                    s_StartupUrl = url;
                }
            }
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
                    CheckDslError();
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnLoadStart:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static bool OnLoadEnd(IntPtr browser, IntPtr frame, string url, int http_status_code, bool inject_all_frame, bool is_main, IntPtr js_code, ref int code_size)
        {
            NativeApi.SetContext(browser, frame);
            if (is_main) {
                s_LastLoadedMainUrl = url;
            }
            s_LastLoadedUrl = url;
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
                    CheckDslError();
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
            code_size = 0;
            return false;
        }

        internal static bool OnGetAuthCredentials(bool is_proxy, string host, int port, string realm, string scheme, string origin_url, IntPtr username, ref int username_size, IntPtr password, ref int password_size, long handle, int attempt)
        {
            NativeLogNoLock($"[csharp] OnGetAuthCredentials: is_proxy={is_proxy}, host={host}, port={port}, realm={realm}, scheme={scheme}, origin={origin_url}, handle={handle}, attempt={attempt}");
            try {
                string user = string.Empty;
                string pass = string.Empty;

                // Ask the DSL layer synchronously (same pattern as
                // OnExecuteMetaDSL: serialized by s_Lock, safe to call from
                // the CEF IO thread).                lock (s_Lock) {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.FromBool(is_proxy));
                    vargs.Add(BoxedValue.FromString(host ?? ""));
                    vargs.Add(BoxedValue.From(port));
                    vargs.Add(BoxedValue.FromString(realm ?? ""));
                    vargs.Add(BoxedValue.FromString(scheme ?? ""));
                    vargs.Add(BoxedValue.FromString(origin_url ?? ""));
                    vargs.Add(BoxedValue.From(handle));
                    vargs.Add(BoxedValue.From(attempt));
                    var r = BatchCommand.BatchScript.Call("on_get_auth_credentials", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();
                    // Return value convention: (handled, username, password)
                    //   handled=false                 -> DSL declines; C++ runs
                    //                                    its credui fallback.
                    //   handled=true,  user empty     -> DSL took ownership of
                    //                                    |handle| and will
                    //                                    complete it later.
                    //   handled=true,  user non-empty -> DSL supplied
                    //                                    credentials
                    //                                    synchronously.
                    if (r.Type == (int)BoxedValue.c_Tuple3Type) {
                        var tuple = r.GetTuple3();
                        if (null != tuple) {
                            bool handled = tuple.Item1.GetBool();
                            if (handled) {
                                user = tuple.Item2.GetString();
                                pass = tuple.Item3.GetString();
                                if (string.IsNullOrEmpty(user)) {
                                    // Async takeover: DSL will complete
                                    // |handle| via native_callback_complete
                                    // (see HostApi.native_callback_complete).
                                    NativeLogNoLock($"[csharp] OnGetAuthCredentials: DSL took over handle={handle}");
                                    username_size = 0;
                                    password_size = 0;
                                    return true;
                                }
                                NativeLogNoLock($"[csharp] OnGetAuthCredentials: DSL handled sync (user={user}, pass_len={pass?.Length ?? 0})");
                            }
                            else {
                                NativeLogNoLock("[csharp] OnGetAuthCredentials: DSL declined (handled=false)");
                                return false;
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(user)) {
                    NativeLogNoLock("[csharp] OnGetAuthCredentials: no credentials provided by DSL, falling back to credui");
                    return false;
                }

                byte[] userBytes = System.Text.Encoding.UTF8.GetBytes(user);
                byte[] passBytes = System.Text.Encoding.UTF8.GetBytes(pass ?? string.Empty);
                if (userBytes.Length >= username_size || passBytes.Length >= password_size) {
                    NativeLogNoLock($"[csharp] OnGetAuthCredentials: buffer too small (user={userBytes.Length}/{username_size}, pass={passBytes.Length}/{password_size})");
                    return false;
                }

                Marshal.Copy(userBytes, 0, username, userBytes.Length);
                Marshal.Copy(passBytes, 0, password, passBytes.Length);
                username_size = userBytes.Length;
                password_size = passBytes.Length;
                return true;
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnGetAuthCredentials:" + e.Message + "\n" + e.StackTrace);
                return false;
            }
        }

        internal static bool OnRequestMediaAccessPermission(string requesting_origin, uint requested_permissions, bool menu_disabled, ref uint allowed_permissions)
        {
            NativeLogNoLock($"[csharp] OnRequestMediaAccessPermission: origin={requesting_origin}, requested=0x{requested_permissions:X}, menu_disabled={menu_disabled}");
            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.FromString(requesting_origin ?? ""));
                    vargs.Add(BoxedValue.From((int)requested_permissions));
                    vargs.Add(BoxedValue.FromBool(menu_disabled));
                    var r = BatchCommand.BatchScript.Call("on_request_media_access_permission", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();
                    // Return value convention: (handled, allowed_bits)
                    // handled=false -> DSL declines, C# returns false so the C++
                    // side falls back to the menu kill-switch / native prompt.
                    if (r.Type == (int)BoxedValue.c_Tuple2Type) {
                        var tuple = r.GetTuple2();
                        if (null != tuple) {
                            bool handled = tuple.Item1.GetBool();
                            if (handled) {
                                int allowed = tuple.Item2.GetInt();
                                allowed_permissions = (uint)allowed;
                                NativeLogNoLock($"[csharp] OnRequestMediaAccessPermission: DSL handled (allowed=0x{allowed_permissions:X})");
                                return true;
                            }
                            NativeLogNoLock("[csharp] OnRequestMediaAccessPermission: DSL declined (handled=false)");
                        }
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnRequestMediaAccessPermission:" + e.Message + "\n" + e.StackTrace);
            }
            return false;
        }

        internal static bool OnCertificateError(int cert_error, string request_url, ref int out_action)
        {
            NativeLogNoLock($"[csharp] OnCertificateError: cert_error={cert_error}, url={request_url}");
            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(cert_error));
                    vargs.Add(BoxedValue.FromString(request_url ?? ""));
                    var r = BatchCommand.BatchScript.Call("on_certificate_error", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();
                    // Return value convention: (handled, action)
                    // action = 0 default / 1 continue / 2 cancel.
                    // handled=false -> DSL declines, C++ falls back to the
                    // Chromium interstitial (return false on the C++ side).
                    if (r.Type == (int)BoxedValue.c_Tuple2Type) {
                        var tuple = r.GetTuple2();
                        if (null != tuple) {
                            bool handled = tuple.Item1.GetBool();
                            if (handled) {
                                out_action = tuple.Item2.GetInt();
                                NativeLogNoLock($"[csharp] OnCertificateError: DSL handled (action={out_action})");
                                return true;
                            }
                            NativeLogNoLock("[csharp] OnCertificateError: DSL declined (handled=false)");
                        }
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnCertificateError:" + e.Message + "\n" + e.StackTrace);
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
                    CheckDslError();
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
                    CheckDslError();
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
                    CheckDslError();
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnRendererLoadStart:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static bool OnRendererLoadEnd(IntPtr browser, IntPtr frame, string url, int http_status_code, bool is_main, IntPtr js_code, ref int code_size)
        {
            NativeApi.SetContext(browser, frame);
            if (is_main) {
                s_LastLoadedMainUrl = url;
            }
            s_LastLoadedUrl = url;
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
                    CheckDslError();
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
            code_size = 0;
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
                    CheckDslError();
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
                    CheckDslError();
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnRendererLoadError:" + e.Message + "\n" + e.StackTrace);
            }
        }

        internal static bool OnRenderProcessTerminated(IntPtr browser, IntPtr frame, string startup_url, string url, int status, int error_code, string error_string, IntPtr reload_url, ref int reload_url_size)
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
                    var r = BatchCommand.BatchScript.Call("on_render_process_terminated", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();
                    // Return value convention: (should_reload, reload_url)
                    if (!r.IsNullObject) {
                        if (r.Type == (int)BoxedValue.c_Tuple2Type) {
                            var tuple = r.GetTuple2();
                            if (null != tuple) {
                                bool shouldReload = tuple.Item1.GetBool();
                                string reloadUrl = tuple.Item2.GetString();
                                if (shouldReload) {
                                    if (string.IsNullOrEmpty(reloadUrl)) {
                                        reload_url_size = 0;
                                        return true;
                                    }
                                    else {
                                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(reloadUrl);
                                        if (bytes.Length < reload_url_size) {
                                            Marshal.Copy(bytes, 0, reload_url, bytes.Length);
                                            reload_url_size = bytes.Length;
                                            return true;
                                        }
                                        else {
                                            NativeLogNoLock($"[csharp] reload_url buffer too small: needed={bytes.Length}, provided={reload_url_size}");
                                            // Report required size to caller; caller will fallback.
                                            reload_url_size = bytes.Length;
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnRenderProcessTerminated:" + e.Message + "\n" + e.StackTrace);
            }
            reload_url_size = 0;
            return false;
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
                    CheckDslError();
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
                    if (!string.IsNullOrEmpty(s_MetaDslSwitch)) {
                        cmdLineProxy.AppendSwitchWithValue("metadsl", s_MetaDslSwitch);
                        NativeLogNoLock($"[dsl] on_before_child_process_launch: copied --metadsl={s_MetaDslSwitch} to child process");
                    }
                    if (!string.IsNullOrEmpty(s_ProjectSwitch)) {
                        cmdLineProxy.AppendSwitchWithValue("projectidentity", s_ProjectSwitch);
                        NativeLogNoLock($"[dsl] on_before_child_process_launch: copied --projectidentity={s_ProjectSwitch} to child process");
                    }

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(process_type));
                    vargs.Add(BoxedValue.From(cmdLineProxy));
                    BatchCommand.BatchScript.Call("on_before_child_process_launch", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();
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
                    CheckDslError();
                    return r.GetBool();
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnAlreadyRunningAppRelaunch:" + e.Message + "\n" + e.StackTrace);
            }
            return false;
        }

        internal static bool OnBeforeBrowse(IntPtr browser, IntPtr frame, IntPtr request, bool user_gesture, bool is_redirect, IntPtr out_return_value)
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
                    CheckDslError();
                    // Return value convention: (handled, return_value)
                    // If handled is true, out_return_value is set and we return true
                    if (r.Type == (int)BoxedValue.c_Tuple2Type) {
                        var tuple2 = r.GetTuple2();
                        if (null != tuple2) {
                            bool handled = tuple2.Item1.GetBool();
                            if (handled) {
                                bool retVal = tuple2.Item2.GetBool();
                                if (out_return_value != IntPtr.Zero) Marshal.WriteByte(out_return_value, (byte)(retVal ? 1 : 0));
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnBeforeBrowse:" + e.Message + "\n" + e.StackTrace);
            }
            if (out_return_value != IntPtr.Zero) Marshal.WriteByte(out_return_value, (byte)0);
            return false;
        }

        //Note: this method will be called on the browser process IO thread.
        internal static bool OnBeforeResourceLoad(IntPtr browser, IntPtr frame, IntPtr request, long handle, ref int out_return_value)
        {
            NativeApi.SetContext(browser, frame);

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var requestProxy = new CefRequestProxy(request, s_NativeApi);
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(requestProxy));
                    vargs.Add(BoxedValue.From(handle));
                    var r = BatchCommand.BatchScript.Call("on_before_resource_load", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();
                    // Return value convention: (handled, return_value_int)
                    // If handled is true, out_return_value is set and we return true
                    // DSL returns raw cef_return_value_t enum (RV_CANCEL=0, RV_CONTINUE=1, RV_CONTINUE_ASYNC=2).
                    // RV_CONTINUE_ASYNC is now genuinely supported: the script must
                    // later call complete_native_callback($handle, ok) to resume or
                    // cancel the request, otherwise it stays pending.
                    if (r.Type == (int)BoxedValue.c_Tuple2Type) {
                        var tuple2 = r.GetTuple2();
                        if (null != tuple2) {
                            bool handled = tuple2.Item1.GetBool();
                            if (handled) {
                                out_return_value = tuple2.Item2.GetInt();
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnBeforeResourceLoad:" + e.Message + "\n" + e.StackTrace);
            }
            finally {
                NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
            }
            return false;
        }

        //Note: this method will be called on the browser process UI thread.
        //DECISION ONLY: keep it fast and never wait for the page here. When the
        //script takes over it should send the display JavaScript synchronously
        //inside the DSL callback (send_javascript_code only posts to the
        //renderer, it does not block) because the browser context is cleared
        //when this method returns.
        internal static int OnJsDialog(IntPtr browser, int dialog_type, string origin_url, string message_text, string default_prompt_text, long handle)
        {
            NativeApi.SetContext(browser, IntPtr.Zero);
            NativeLogNoLock($"[csharp] OnJsDialog: type={dialog_type}, origin={origin_url}, handle={handle}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(dialog_type));
                    vargs.Add(BoxedValue.FromString(origin_url));
                    vargs.Add(BoxedValue.FromString(message_text));
                    vargs.Add(BoxedValue.FromString(default_prompt_text));
                    vargs.Add(BoxedValue.From(handle));
                    var r = BatchCommand.BatchScript.Call("on_js_dialog", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();
                    // Return value convention: (handled, decision_int)
                    // decision: 0=CEF default dialog, 1=custom dialog shown by the
                    // script, 2=suppress, 3=script owned. Not handled -> 0.
                    if (r.Type == (int)BoxedValue.c_Tuple2Type) {
                        var tuple2 = r.GetTuple2();
                        if (null != tuple2) {
                            bool handled = tuple2.Item1.GetBool();
                            if (handled) {
                                return tuple2.Item2.GetInt();
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnJsDialog:" + e.Message + "\n" + e.StackTrace);
            }
            finally {
                NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
            }
            return 0;
        }

        //Note: this method will be called on the browser process IO thread.
        internal static void OnBeforeResourceResponse(IntPtr browser, IntPtr frame, IntPtr request, IntPtr response)
        {
            NativeApi.SetContext(browser, frame);
            try {
                if (s_NativeApi != null) {
                    TryLoadDSL();
                    var requestProxy = new CefRequestProxy(request, s_NativeApi);
                    var responseProxy = new CefResponseProxy(response, s_NativeApi);
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(requestProxy));
                    vargs.Add(BoxedValue.From(responseProxy));
                    BatchCommand.BatchScript.Call("on_before_resource_response", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] OnBeforeResourceResponse Exception:" + e.Message + "\n" + e.StackTrace);
            }
            finally {
                NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
            }
        }

        //Note: this method will be called on the browser process IO thread.
        internal static bool OnResourceResponseFilter(IntPtr browser, IntPtr frame, IntPtr request, IntPtr response, IntPtr out_replace_content)
        {
            NativeApi.SetContext(browser, frame);
            try {
                if (s_NativeApi != null) {
                    TryLoadDSL();
                    var requestProxy = new CefRequestProxy(request, s_NativeApi);
                    var responseProxy = new CefResponseProxy(response, s_NativeApi);
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(requestProxy));
                    vargs.Add(BoxedValue.From(responseProxy));
                    var r = BatchCommand.BatchScript.Call("on_resource_response_filter", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();
                    // DSL return value: (handled, replace_content) or just handled.
                    if (r.IsTuple) {
                        int len = r.TupleLength;
                        if (len >= 1) {
                            bool handled = r.GetTupleValue(0).GetBool();
                            bool replaceContent = len >= 2 ? r.GetTupleValue(1).GetBool() : false;
                            if (out_replace_content != IntPtr.Zero) Marshal.WriteByte(out_replace_content, (byte)(replaceContent ? 1 : 0));
                            return handled;
                        }
                    }
                    // Single bool return: treated as handled, replace_content defaults to false.
                    else if (!r.IsNullObject) {
                        if (out_replace_content != IntPtr.Zero) Marshal.WriteByte(out_replace_content, (byte)0);
                        return r.GetBool();
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] OnResourceResponseFilter Exception:" + e.Message + "\n" + e.StackTrace);
            }
            finally {
                NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
            }
            if (out_replace_content != IntPtr.Zero) Marshal.WriteByte(out_replace_content, (byte)0);
            return false;
        }

        //Note: this method will be called on the browser process IO thread.
        internal static bool OnResponseContentFilter(IntPtr data_in, int data_in_size, IntPtr data_out, int data_out_size, ref int out_data_in_read, ref int out_data_out_written, ref int out_status)
        {
            // Default: not handled (native will pass through unchanged).
            // DSL return value: (handled, status, output_bytes, bytes_read).
            //   handled: true = use DSL's outputs, false = pass through.
            //   status: 0=DONE, 1=NEED_MORE_DATA, 2=ERROR.
            //   output_bytes: byte[] filtered output. It must fit in
            //     data_out_size; oversized output is rejected as ERROR rather
            //     than silently truncated.
            //   bytes_read: how many input bytes DSL consumed (0..data_in_size).
            // Empty input is a completion flush: DSL may return pending output
            // with NEED_MORE_DATA, or DONE with no output when finished.
            // No SetContext here: body filter is a pure data transform, no
            // browser/frame is available from the native side.
            out_status = 0;  // DONE
            out_data_in_read = data_in_size;
            out_data_out_written = 0;

            try {
                if (s_NativeApi != null) {
                    TryLoadDSL();
                    byte[] inputBuf = data_in_size > 0
                        ? ReadNativeBytes(data_in, data_in_size)
                        : Array.Empty<byte>();
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.FromObject(inputBuf));
                    var r = BatchCommand.BatchScript.Call("on_response_content_filter", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();

                    bool handled = false;
                    byte[]? outputBuf = null;
                    int bytesRead = data_in_size;
                    if (r.IsTuple) {
                        int len = r.TupleLength;
                        if (len >= 1) {
                            handled = r.GetTupleValue(0).GetBool();
                            if (handled) {
                                if (len >= 2) {
                                    out_status = r.GetTupleValue(1).GetInt();
                                }
                                if (len >= 3) {
                                    outputBuf = r.GetTupleValue(2).As<byte[]>();
                                }
                                if (len >= 4) {
                                    bytesRead = r.GetTupleValue(3).GetInt();
                                }
                            }
                        }
                    }
                    else {
                        handled = r.GetBool();
                    }
                    if (handled) {
                        // The native callback has no per-stream C# output
                        // continuation state. Truncating an oversized DSL
                        // byte[] would lose its tail after the input is
                        // consumed, so reject it explicitly instead.
                        if (outputBuf != null && outputBuf.Length > data_out_size) {
                            NativeLogNoLock("[csharp] OnResponseContentFilter output exceeds native buffer: " + outputBuf.Length + ">" + data_out_size);
                            out_status = 2;  // RESPONSE_FILTER_ERROR
                            out_data_in_read = 0;
                            out_data_out_written = 0;
                            return true;
                        }

                        int written = 0;
                        if (outputBuf != null && data_out != IntPtr.Zero && data_out_size > 0) {
                            written = outputBuf.Length;
                            Marshal.Copy(outputBuf, 0, data_out, written);
                        }
                        out_data_out_written = written;

                        // Clamp bytes_read to [0, data_in_size].
                        if (bytesRead < 0) bytesRead = 0;
                        if (bytesRead > data_in_size) bytesRead = data_in_size;
                        out_data_in_read = bytesRead;

                        return true;
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] OnResponseContentFilter Exception:" + e.Message + "\n" + e.StackTrace);
            }
            // Not handled: native falls back to pass-through.
            return false;
        }

        //Note: this method will be called on the browser process IO thread.
        internal static void OnResourceLoadComplete(IntPtr browser, IntPtr frame, IntPtr request, IntPtr response, int status, long received_content_length)
        {
            NativeApi.SetContext(browser, frame);
            try {
                if (s_NativeApi != null) {
                    TryLoadDSL();
                    var requestProxy = new CefRequestProxy(request, s_NativeApi);
                    var responseProxy = new CefResponseProxy(response, s_NativeApi);
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(requestProxy));
                    vargs.Add(BoxedValue.From(responseProxy));
                    vargs.Add(BoxedValue.From(status));
                    vargs.Add(BoxedValue.From(received_content_length));
                    BatchCommand.BatchScript.Call("on_resource_load_complete", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] OnResourceLoadComplete Exception:" + e.Message + "\n" + e.StackTrace);
            }
            finally {
                NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
            }
        }

        //Note: this method will be called on the browser process IO thread.
        internal static bool OnProtocolExecution(IntPtr browser, IntPtr frame, IntPtr request, IntPtr out_allow_os_execution)
        {
            NativeApi.SetContext(browser, frame);
            try {
                if (s_NativeApi != null) {
                    TryLoadDSL();
                    var requestProxy = new CefRequestProxy(request, s_NativeApi);
                    bool allowOsExecution = out_allow_os_execution != IntPtr.Zero && Marshal.ReadByte(out_allow_os_execution) != 0;
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(requestProxy));
                    vargs.Add(BoxedValue.From(allowOsExecution));
                    var r = BatchCommand.BatchScript.Call("on_protocol_execution", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();
                    if (r.IsTuple && r.TupleLength >= 1 && r.GetTupleValue(0).GetBool()) {
                        bool dslAllowOsExecution = r.TupleLength >= 2
                            ? r.GetTupleValue(1).GetBool()
                            : allowOsExecution;
                        if (out_allow_os_execution != IntPtr.Zero) {
                            Marshal.WriteByte(out_allow_os_execution, (byte)(dslAllowOsExecution ? 1 : 0));
                        }
                        return true;
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] OnProtocolExecution Exception:" + e.Message + "\n" + e.StackTrace);
            }
            finally {
                NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
            }
            return false;
        }

        //Note: this method will be called on the browser process IO thread.
        internal static bool OnResourceRedirect(IntPtr browser, IntPtr frame, IntPtr request, IntPtr response, string new_url, IntPtr out_url, ref int out_url_size)
        {
            NativeApi.SetContext(browser, frame);
            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var requestProxy = new CefRequestProxy(request, s_NativeApi);
                    var responseProxy = new CefResponseProxy(response, s_NativeApi);
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(requestProxy));
                    vargs.Add(BoxedValue.From(responseProxy));
                    vargs.Add(BoxedValue.FromString(new_url ?? string.Empty));
                    var r = BatchCommand.BatchScript.Call("on_resource_redirect", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();

                    // DSL return value: (handled, redirect_url_string)
                    // If handled is true and redirect_url is non-empty, write it to out_url.
                    if (r.Type == (int)BoxedValue.c_Tuple2Type) {
                        var tuple2 = r.GetTuple2();
                        if (null != tuple2) {
                            bool handled = tuple2.Item1.GetBool();
                            string redirectUrl = tuple2.Item2.GetString();
                            if (handled && !string.IsNullOrEmpty(redirectUrl)) {
                                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(redirectUrl);
                                if (bytes.Length <= out_url_size) {
                                    Marshal.Copy(bytes, 0, out_url, bytes.Length);
                                    out_url_size = bytes.Length;
                                    return true;
                                }
                                else {
                                    NativeLogNoLock($"[csharp] OnResourceRedirect: out_url buffer too small: needed={bytes.Length}, available={out_url_size}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnResourceRedirect:" + e.Message + "\n" + e.StackTrace);
            }
            finally {
                NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
            }
            out_url_size = 0;
            return false;
        }

        internal static bool OnConsoleLog(IntPtr browser, int level, string message, string source, int line, ref int maxLogSize)
        {
            NativeApi.SetContext(browser, IntPtr.Zero);

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(level));
                    vargs.Add(BoxedValue.FromString(message));
                    vargs.Add(BoxedValue.FromString(source));
                    vargs.Add(BoxedValue.From(line));
                    vargs.Add(BoxedValue.From(maxLogSize));
                    var r = BatchCommand.BatchScript.Call("on_console_log", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    CheckDslError();
                    if (r.Type == (int)BoxedValue.c_Tuple2Type) {
                        var tuple = r.GetTuple2();
                        if (tuple != null) {
                            bool handled = tuple.Item1.GetBool();
                            if (handled) {
                                return true;
                            }
                            int newSize = tuple.Item2.GetInt();
                            if (newSize > 0) {
                                maxLogSize = newSize;
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnConsoleLog:" + e.Message + "\n" + e.StackTrace);
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
                    CheckDslError();
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

                        bool funcExists = BatchScript.Calculator.TryGetFuncInfo(func_name, out var finfo);
                        var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                        foreach (var arg in args) {
                            vargs.Add(BoxedValue.FromString(arg));
                        }
                        BoxedValue r;
                        if (funcExists) {
                            r = BatchCommand.BatchScript.Call(func_name, vargs);
                        }
                        else {
                            r = BatchCommand.BatchScript.Call("on_call_metadsl", BoxedValue.FromString(func_name), BoxedValue.FromObject(vargs));
                        }
                        BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                        CheckDslError();
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
                        foreach (var arg in args) {
                            vargs.Add(BoxedValue.FromString(arg));
                        }
                        // In C#, we do not directly invoke `msg` as a function, because `cef_message` may be received
                        // in either the browser process or the renderer process—and is typically forwarded internally
                        // within the browser process. Instead, we can utilize the `redirectcall` directive within the
                        // DSL to invoke `msg` as a function.
                        BoxedValue r = BatchCommand.BatchScript.Call("on_receive_cef_message", BoxedValue.FromString(msg), BoxedValue.FromObject(vargs), BoxedValue.From(source_process_id));
                        BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                        CheckDslError();
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

        internal static string CmdLine {
            get {
                return s_CmdLine;
            }
        }

        internal static string BasePath {
            get {
                return s_BasePath;
            }
        }

        internal static string AppDir {
            get {
                return s_AppDir;
            }
        }

        internal static bool IsMac {
            get {
                return s_IsMac;
            }
        }

        internal static int ProcessType {
            get {
                return s_ProcessType;
            }
        }

        internal static int MainThreadId {
            get {
                return s_MainThreadId;
            }
        }

        internal static IAgentPlugin? AgentPlugin {
            get {
                return AgentFrameworkService.Instance.AgentPlugin;
            }
        }

        internal static string DslScriptFile {
            get {
                return s_DslScriptFile;
            }
            set {
                s_DslScriptFile = value;
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
        internal static bool EnqueueCefMessage(string msg, params string[] args)
        {
            if (null != s_NativeApi) {
                s_NativeApi.EnqueueCefMessage(msg, args);
                return true;
            }
            return false;
        }
        internal static bool HandleThreadQueue(int maxNativeCount, int maxJsCount, int maxCodeCount, int maxFuncCount)
        {
            bool isMainThread = Thread.CurrentThread.ManagedThreadId == s_MainThreadId;
            if (isMainThread && null != s_NativeApi) {
                s_NativeApi.HandleAllQueues(maxNativeCount, maxJsCount, maxCodeCount, maxFuncCount);
                return true;
            }
            return false;
        }
        internal static bool SetHeartbeatInterval(int intervalMs)
        {
            s_NativeApi?.SetHeartbeatInterval(intervalMs);
            return null != s_NativeApi;
        }
        // Sends JavaScript to the renderer for the current context's browser.
        // Fire-and-forget: it only posts the code, it never waits for the page.
        internal static bool SendJavascriptCodeToRenderer(string code)
        {
            if (null == s_NativeApi)
                return false;
            s_NativeApi.SendJavascriptCode(code);
            return true;
        }
        // Calls a JavaScript function in the renderer for the current context's
        // browser. Fire-and-forget, like SendJavascriptCodeToRenderer.
        internal static bool SendJavascriptCallToRenderer(string func, IList<BoxedValue> args)
        {
            if (null == s_NativeApi)
                return false;
            s_NativeApi.SendJavascriptCallForDSL(func, args);
            return true;
        }
        // Renders the in-page AgentDialog for a JS dialog taken over by the
        // script. The payload is serialized with System.Text.Json so the message
        // and default text can contain quotes, newlines or backslashes safely.
        // The handle is passed as a string because a JS number only holds 53 bits.
        internal static bool ShowNativeJsDialog(long handle, int dialogType, string message, string defaultText)
        {
            if (null == s_NativeApi)
                return false;
            var payload = new Dictionary<string, object?> {
                { "dialogId", handle.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                { "type", dialogType },
                { "message", message ?? string.Empty },
                { "defaultText", defaultText ?? string.Empty },
            };
            string json = System.Text.Json.JsonSerializer.Serialize(payload);
            // Falls back to reporting js_dialog_unavailable when the page has no
            // AgentDialog module, so the pending dialog gets canceled instead of
            // hanging.
            string code = "(function(p){if(window.__agentDialogShow){window.__agentDialogShow(p);}"
                + "else if(typeof window.cefQuery==='function'){window.cefQuery({request:JSON.stringify({action:'js_dialog_unavailable',handle:p.dialogId}),onSuccess:function(){},onFailure:function(){}});}})("
                + json + ");";
            s_NativeApi.SendJavascriptCode(code);
            return true;
        }
        // Completes a CEF async callback previously taken over by the script
        // (JS dialog, deferred resource load). Callable from any thread; the
        // native side resumes the callback on the thread it belongs to.
        internal static bool CompleteNativeCallback(long handle, bool ok, string? data, int code)
        {
            if (null == s_NativeApi)
                return false;
            return s_NativeApi.NativeCallbackComplete(handle, ok, data, code);
        }
        internal static IntPtr GetBrowsersFirstValid()
        {
            if (s_NativeApi == null)
                return IntPtr.Zero;
            if (s_ProcessType == (int)CefProcessType.RendererProcess) {
                foreach (var id in s_BrowserBrowserIds) {
                    var pair = s_NativeApi.GetRendererBrowserFrameById(id);
                    if (pair.browser != IntPtr.Zero) {
                        return pair.browser;
                    }
                }
            }
            else if (s_ProcessType == (int)CefProcessType.BrowserProcess) {
                foreach (var id in s_RendererBrowserIds) {
                    IntPtr browser = s_NativeApi.GetBrowserById(id);
                    if (browser != IntPtr.Zero) {
                        return browser;
                    }
                }
            }
            return IntPtr.Zero;
        }
        /// <summary>
        /// Get all tracked browser IDs for the current process.
        /// Browser process: returns IDs from the C#-maintained browser id set.
        /// Renderer process: returns IDs from the tracked renderer browser/frame dictionary.
        /// </summary>
        internal static int[]? GetAllContextBrowserIds()
        {
            if (s_ProcessType == (int)CefProcessType.RendererProcess) {
                return s_RendererBrowserIds.ToArray();
            }
            else if (s_ProcessType == (int)CefProcessType.BrowserProcess) {
                return s_BrowserBrowserIds.ToArray();
            }
            return null;
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
                    // Sync: remove stale entry from C# id set
                    s_RendererBrowserIds.Remove(browserId);
                    return false;
                }
                NativeApi.SetContext(pair.browser, pair.frame);
                return true;
            }
            else if (s_ProcessType == (int)CefProcessType.BrowserProcess) {
                // Browser process
                IntPtr browser = s_NativeApi.GetBrowserById(browserId);
                if (browser == IntPtr.Zero) {
                    // Sync: remove stale entry from C# id set
                    s_BrowserBrowserIds.Remove(browserId);
                    return false;
                }
                IntPtr frame = s_NativeApi.BrowserGetMainFrame(browser);
                if (frame == IntPtr.Zero) return false;
                NativeApi.SetContext(browser, frame);
                return true;
            }
            return false;
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
                    // Sync: remove stale entry from C# id set
                    s_RendererBrowserIds.Remove(browserId);
                }
                return pair.browser;
            }
            else if (s_ProcessType == (int)CefProcessType.BrowserProcess) {
                // Browser process
                IntPtr browser = s_NativeApi.GetBrowserById(browserId);
                if (browser == IntPtr.Zero) {
                    // Sync: remove stale entry from C# id set
                    s_BrowserBrowserIds.Remove(browserId);
                }
                return browser;
            }
            return IntPtr.Zero;
        }
        /// <summary>
        /// Find a browser ID whose URL contains the given key substring.
        /// Returns the first matching browser ID, or -1 if not found.
        /// Works for both browser and renderer processes.
        /// </summary>
        internal static int FindBrowserIdByUrlKey(string urlKey)
        {
            if (s_NativeApi == null || string.IsNullOrEmpty(urlKey))
                return -1;
            var ids = GetAllContextBrowserIds();
            if (null == ids)
                return -1;
            foreach (var id in ids) {
                string url = string.Empty;
                if (s_ProcessType == (int)CefProcessType.RendererProcess) {
                    // In renderer process, use GetRendererBrowserFrameById to get valid pointers.
                    // NOTE: do NOT call FrameIsValid / FrameGetUrl on the returned frame ptr.
                    // The native impl fetches the main frame on-the-fly and may not keep a
                    // ref beyond the call, so the frame ptr can point to an already-released
                    // CefFrame object; CToCpp GetWrapperStruct will hit NOTREACHED and abort.
                    // The browser ptr is safer because native ref map holds a strong ref.
                    var pair = s_NativeApi.GetRendererBrowserFrameById(id);
                    if (pair.browser == IntPtr.Zero || !s_NativeApi.BrowserIsValid(pair.browser)) {
                        // Sync: remove stale entry from C# id set
                        s_RendererBrowserIds.Remove(id);
                        continue;
                    }
                    url = s_NativeApi.BrowserGetUrl(pair.browser);
                }
                else if (s_ProcessType == (int)CefProcessType.BrowserProcess) {
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
            BatchCommand.BatchScript.SetGlobalVariable("startupurl", BoxedValue.FromString(s_StartupUrl));
            BatchCommand.BatchScript.SetGlobalVariable("lastloadedmainurl", BoxedValue.FromString(s_LastLoadedMainUrl));
            BatchCommand.BatchScript.SetGlobalVariable("lastloadedurl", BoxedValue.FromString(s_LastLoadedUrl));
            BatchCommand.BatchScript.SetGlobalVariable("dslpath", BoxedValue.FromString(s_DslScriptPath));
            BatchCommand.BatchScript.SetGlobalVariable("dslfile", BoxedValue.FromString(s_DslScriptFile));
            BatchCommand.BatchScript.SetGlobalVariable("initialdslfile", BoxedValue.FromString(s_InitialDslScriptFile));
            BatchCommand.BatchScript.SetGlobalVariable("initialprojectidentity", BoxedValue.FromString(s_InitialProjectIdentity));
            BatchCommand.BatchScript.ClearDslErrors();
        }
        internal static void AddCommonApiDocs()
        {
            BatchCommand.BatchScript.AddUserApiDoc("clone", "clone(list_or_hashtable) api");
            BatchCommand.BatchScript.AddUserApiDoc("args", "args() api");
            BatchCommand.BatchScript.AddUserApiDoc("arg", "arg(ix) api");
            BatchCommand.BatchScript.AddUserApiDoc("argnum", "argnum() api");
            BatchCommand.BatchScript.AddUserApiDoc("inc", "inc(var) or inc(var,val) api");
            BatchCommand.BatchScript.AddUserApiDoc("dec", "dec(var) or dec(var,val) api");
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
            BatchCommand.BatchScript.AddUserApiDoc("foreachvalue", "foreachvalue(arg1,arg2,...)func(args); or foreachvalue(arg1,arg2,...){...}; statement, iterator is $$");
            BatchCommand.BatchScript.AddUserApiDoc("return", "return([val]) api");
            BatchCommand.BatchScript.AddUserApiDoc("dotnetcall", "dotnetcall api, internal implementation, using csharp object syntax");
            BatchCommand.BatchScript.AddUserApiDoc("dotnetset", "dotnetset api, internal implementation, using csharp object syntax");
            BatchCommand.BatchScript.AddUserApiDoc("dotnetget", "dotnetget api, internal implementation, using csharp object syntax");
            BatchCommand.BatchScript.AddUserApiDoc("collectioncall", "collectioncall api, internal implementation, using csharp object syntax");
            BatchCommand.BatchScript.AddUserApiDoc("collectionset", "collectionset api, internal implementation, using csharp object syntax");
            BatchCommand.BatchScript.AddUserApiDoc("collectionget", "collectionget api, internal implementation, using csharp object syntax");
            BatchCommand.BatchScript.AddUserApiDoc("linq", "linq(list,method,arg1,arg2,...) statement, internal implementation, using obj.method(arg1,arg2,...) syntax, method can be where/filter/select/map/top/take/skip/distinct/concat/groupby/orderby/orderbydesc/aggregate/reduce/any/all/count/first/last/tolist/sum/min/max/average, iterator is $$ (An additional iterator $$acc for aggregate/reduce operators)");
            BatchCommand.BatchScript.AddUserApiDoc("null", "null() api");
            BatchCommand.BatchScript.AddUserApiDoc("propset", "propset(varname,val) - set variable");
            BatchCommand.BatchScript.AddUserApiDoc("propget", "propget(varname[,defval]) - get variable");
            BatchCommand.BatchScript.AddUserApiDoc("propexists", "propexists(varname) - check variable");
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
            BatchCommand.BatchScript.AddUserApiDoc("datetime", "datetime(v) api");
            BatchCommand.BatchScript.AddUserApiDoc("isobject", "isobject(v)");
            BatchCommand.BatchScript.AddUserApiDoc("isstring", "isstring(v)");
            BatchCommand.BatchScript.AddUserApiDoc("isboolean", "isboolean(v)");
            BatchCommand.BatchScript.AddUserApiDoc("ischar", "ischar(v)");
            BatchCommand.BatchScript.AddUserApiDoc("isinteger", "isinteger(v)");
            BatchCommand.BatchScript.AddUserApiDoc("issignedinteger", "issignedinteger(v)");
            BatchCommand.BatchScript.AddUserApiDoc("isunsignedinteger", "isunsignedinteger(v)");
            BatchCommand.BatchScript.AddUserApiDoc("isnumber", "isnumber(v)");
            BatchCommand.BatchScript.AddUserApiDoc("isdatetime", "isdatetime(v)");
            BatchCommand.BatchScript.AddUserApiDoc("istuple", "istuple(v)");
            BatchCommand.BatchScript.AddUserApiDoc("boxedvaluetype", "boxedvaluetype(v)");
            BatchCommand.BatchScript.AddUserApiDoc("boxedvaluetypename", "boxedvaluetypename(v)");
            BatchCommand.BatchScript.AddUserApiDoc("ftoi", "ftoi(v) api");
            BatchCommand.BatchScript.AddUserApiDoc("itof", "itof(v) api");
            BatchCommand.BatchScript.AddUserApiDoc("ftou", "ftou(v) api");
            BatchCommand.BatchScript.AddUserApiDoc("utof", "utof(v) api");
            BatchCommand.BatchScript.AddUserApiDoc("dtol", "dtol(v) api");
            BatchCommand.BatchScript.AddUserApiDoc("ltod", "ltod(v) api");
            BatchCommand.BatchScript.AddUserApiDoc("dtou", "dtou(v) api");
            BatchCommand.BatchScript.AddUserApiDoc("utod", "utod(v) api");
            BatchCommand.BatchScript.AddUserApiDoc("lerp", "lerp(a,b,t)");
            BatchCommand.BatchScript.AddUserApiDoc("clamp01", "clamp01(v)");
            BatchCommand.BatchScript.AddUserApiDoc("clamp", "clamp(v,v1,v2)");
            BatchCommand.BatchScript.AddUserApiDoc("approximately", "approximately(v1,v2)");
            BatchCommand.BatchScript.AddUserApiDoc("format", "format(fmt,arg1,arg2,...)");
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
            BatchCommand.BatchScript.AddUserApiDoc("calcmd5", "calcmd5(file) api");
            BatchCommand.BatchScript.AddUserApiDoc("pid", "pid() api");
            BatchCommand.BatchScript.AddUserApiDoc("sleep", "sleep(milliseconds) api");
            BatchCommand.BatchScript.AddUserApiDoc("now", "now() api");
            BatchCommand.BatchScript.AddUserApiDoc("isnullorempty", "isnullorempty(str) api");

            BatchCommand.BatchScript.AddUserApiDoc("time", "time() or timestamp() api, return milliseconds since startup");
            BatchCommand.BatchScript.AddUserApiDoc("timestamp", "time() or timestamp() api, return milliseconds since startup");
            BatchCommand.BatchScript.AddUserApiDoc("getelapsedms", "getelapsedms() api, return elapsed milliseconds (time)");
            BatchCommand.BatchScript.AddUserApiDoc("getelapsedus", "getelapsedus() api, return elapsed microseconds (time)");
        }
        internal static string GetMetaDslResult(int maxResultSize, StringBuilder resSb, StringBuilder errSb)
        {
            var sb = new StringBuilder();
            if (maxResultSize > 0) {
                if (resSb.Length > maxResultSize) {
                    if (errSb.Length > maxResultSize * 1 / 3) {
                        sb.Append(resSb.ToString(0, maxResultSize * 2 / 3));
                        sb.AppendLine("...");
                        sb.Append(errSb.ToString(0, maxResultSize * 1 / 3));
                        sb.Append("... [truncated, exceeded max result size ");
                        sb.Append(maxResultSize);
                        sb.AppendLine("]");
                    }
                    else {
                        sb.Append(resSb.ToString(0, maxResultSize - errSb.Length));
                        sb.AppendLine("...");
                        sb.Append(errSb.ToString());
                        sb.Append("... [truncated, exceeded max result size ");
                        sb.Append(maxResultSize);
                        sb.AppendLine("]");
                    }
                }
                else {
                    sb.Append(resSb.ToString());
                    if (errSb.Length > maxResultSize - resSb.Length) {
                        sb.AppendLine(errSb.ToString(0, maxResultSize - resSb.Length));
                        sb.Append("... [truncated, exceeded max result size ");
                        sb.Append(maxResultSize);
                        sb.AppendLine("]");
                    }
                    else {
                        sb.Append(errSb.ToString());
                    }
                }
            }
            else {
                sb.Append(resSb.ToString());
                sb.Append(errSb.ToString());
            }
            return sb.ToString();
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
            bool loaded = false;
            string path = Path.Combine(s_BasePath, "managed", s_DslScriptFile);
            var fi = new FileInfo(path);
            if (fi.Exists) {
                if (fi.LastWriteTime != s_DslScriptTime || s_DslScriptPath != path) {
                    s_DslScriptTime = fi.LastWriteTime;
                    s_DslScriptPath = path;

                    string errorMsg = string.Empty;
                    if (File.Exists(fi.FullName)) {
                        loaded = true;
                        BatchCommand.BatchScript.Load(fi.FullName);
                        CheckDslError();
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
            if (loaded) {
                BatchCommand.BatchScript.Call("init_global_consts");
                CheckDslError();
            }
        }
        // Execute MetaDSL script
        private static string ExecuteMetaDslScript(string script)
        {
            return ExecuteMetaDslScript(script, 0, out var hasError);
        }
        internal static string ExecuteMetaDslScript(string script, int maxResultSize, out bool hasError)
        {
            try {
                hasError = false;
                PrepareBatchScript();
                // Execute the script directly using the DSL interpreter
                RefreshGlobalVars();
                NativeApi.ClearApiErrorInfo();
                var id = BatchCommand.BatchScript.EvalAsFunc(script, s_EmptyArgs);
                var resSb = new StringBuilder();
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
                    resSb.AppendLine(resultStr);
                }
                var errSb = new StringBuilder();
                if (NativeApi.HasApiErrorInfo) {
                    hasError = true;
                    errSb.AppendLine();
                    errSb.AppendLine(NativeApi.GetApiErrorInfo());
                }
                if (BatchCommand.BatchScript.HasDslErrors) {
                    hasError = true;
                    errSb.AppendLine();
                    errSb.AppendLine(BatchCommand.BatchScript.GetDslErrors());
                }
                return GetMetaDslResult(maxResultSize, resSb, errSb);
            }
            catch (Exception ex) {
                hasError = true;
                NativeLogNoLock($"[AgentCommand] Error executing MetaDSL script: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        private static void RegisterBatchScriptApi()
        {
            AddCommonApiDocs();
            // Basic framework APIs (defined in Program.cs)
            BatchCommand.BatchScript.Register("setdslfile", "setdslfile(dsl_file,...)", false, new ExpressionFactoryHelper<SetDslFileExp>());
            BatchCommand.BatchScript.Register("import", "import(dsl_file,...)", false, new ExpressionFactoryHelper<ImportExp>());
            BatchCommand.BatchScript.Register("redirectcall", "redirectcall(func_name) or redirectcall(func_name,args) or redirectcall(func_name,args,...)", false, new ExpressionFactoryHelper<RedirectCallExp>());
            BatchCommand.BatchScript.Register("executemetadsl", "executemetadsl(dsl_code), return (bool, result)", false, new ExpressionFactoryHelper<ExecuteMetaDslExp>());
            BatchCommand.BatchScript.Register("execute_metadsl", "execute_metadsl(dsl_code), return (bool, result)", new ExpressionFactoryHelper<ExecuteMetaDslExp>());
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
            BatchCommand.BatchScript.Register("enqueue_cef_message", "enqueue_cef_message(msg,arg1,arg2,...), enqueue message to browser", false, new ExpressionFactoryHelper<EnqueueCefMessageExp>());
            BatchCommand.BatchScript.Register("get_string_in_length", "get_string_in_length(str,len[,begin0_end1_or_beginend2])", new ExpressionFactoryHelper<GetStringInLengthExp>());
            BatchCommand.BatchScript.Register("help", "help(pattern, ...), agent api help", new ExpressionFactoryHelper<HelpExp>());
            BatchCommand.BatchScript.Register("helpall", "helpall(pattern, ...), agent and framework api help", new ExpressionFactoryHelper<HelpAllExp>());

            // Agent-related APIs are registered by AgentCore plugin via LoadAgentPlugin()

            // Only valid in MainThread
            BatchCommand.BatchScript.Register("handle_thread_queue", "handle_thread_queue([max_native_logs,max_js_logs,max_code_count,max_func_count]), only valid in main thread", false, new ExpressionFactoryHelper<HandleThreadQueueExp>());
            BatchCommand.BatchScript.Register("set_heart_beat_interval", "set_heart_beat_interval(interval_ms), set heartbeat interval in ms (10-60000)", false, new ExpressionFactoryHelper<SetHeartBeatIntervalExp>());
            BatchCommand.BatchScript.Register("complete_native_callback", "complete_native_callback(handle, ok[, data, code]) - complete a CEF async callback taken over by the script (JS dialog, deferred resource load, cefQuery)", false, new ExpressionFactoryHelper<CompleteNativeCallbackExp>());
            BatchCommand.BatchScript.Register("send_javascript_code", "send_javascript_code(code) - post JavaScript to the renderer of the current context browser", false, new ExpressionFactoryHelper<SendJavascriptCodeExp>());
            BatchCommand.BatchScript.Register("send_javascript_call", "send_javascript_call(func, arg1, arg2, ...) - call a JavaScript function in the renderer of the current context browser", false, new ExpressionFactoryHelper<SendJavascriptCallExp>());
            BatchCommand.BatchScript.Register("show_native_js_dialog", "show_native_js_dialog(handle, dialog_type, message[, default_text]) - show the in-page AgentDialog for a taken over JS dialog", false, new ExpressionFactoryHelper<ShowNativeJsDialogExp>());
            BatchCommand.BatchScript.Register("get_browser_ids", "get_browser_ids() - get all browser IDs in current process", false, new ExpressionFactoryHelper<GetBrowserIdsExp>());
            BatchCommand.BatchScript.Register("set_context_by_id", "set_context_by_id(browser_id) - set current context by browser ID, returns bool", false, new ExpressionFactoryHelper<SetContextByIdExp>());
            BatchCommand.BatchScript.Register("find_browser_id_by_url_key", "find_browser_id_by_url_key(url_key) - find browser ID by URL substring, returns -1 if not found", false, new ExpressionFactoryHelper<FindBrowserIdByUrlKeyExp>());
            BatchCommand.BatchScript.Register("dev_tools_parse_bytes", "dev_tools_parse_bytes(bytes_or_string) - parse UTF-8 JSON to DSL value tree (dict/list/primitives)", new ExpressionFactoryHelper<DevToolsParseBytesExp>());
        }
        private static void PrepareBatchScript()
        {
            if (!s_BatchScriptInited) {
                BatchCommand.BatchScript.Init();
                RegisterBatchScriptApi();
                s_BatchScriptInited = true;
            }
        }
        private static void CheckDslError()
        {
            if (BatchCommand.BatchScript.HasDslErrors) {
                NativeLogNoLock("[csharp] Dsl error: " + BatchCommand.BatchScript.GetDslErrors());
            }
        }

        [ThreadStatic]
        private static bool s_BatchScriptInited = false;
        [ThreadStatic]
        private static DateTime s_DslScriptTime;
        [ThreadStatic]
        private static string? s_DslScriptPath;

        private static string s_DslScriptFile = string.Empty;

        private static string s_CmdLine = string.Empty;
        private static string s_BasePath = string.Empty;
        private static string s_AppDir = string.Empty;
        private static bool s_IsMac = false;
        private static int s_ProcessType = -1;
        // Renderer process: tracked main-frame browser ids (native ref map owns the CefRefPtr)
        private static readonly HashSet<int> s_RendererBrowserIds = new();
        // Browser process: tracked browser IDs (maintained by OnBrowserInit/OnBrowserFinalize)
        private static readonly HashSet<int> s_BrowserBrowserIds = new();
        private static string s_StartupUrl = string.Empty;
        private static string s_LastLoadedMainUrl = string.Empty;
        private static string s_LastLoadedUrl = string.Empty;
        private static string s_InitialDslScriptFile = string.Empty;
        private static string s_InitialProjectIdentity = string.Empty;
        private static int s_MainThreadId = 0;
        private static object s_Lock = new object();

        private static string s_MetaDslSwitch = string.Empty;
        private static string s_ProjectSwitch = string.Empty;
        private static List<string> s_EmptyArgs = new List<string>();
        private static StringBuilder s_StringBuilder = new StringBuilder();
        private static TextWriter s_StringWriter = StreamWriter.Synchronized(new StringWriter(s_StringBuilder));
        private static NativeApi? s_NativeApi;
    }
    internal static class CefDotnetAppApi
    {
        // Execute MetaDSL script
        internal static string ExecuteMetaDslScript(string script, int maxResultSize, out bool hasError)
        {
            try {
                hasError = false;
                PrepareBatchScript();
                // Execute the script directly using the DSL interpreter
                Lib.RefreshGlobalVars();
                NativeApi.ClearApiErrorInfo();
                var id = BatchCommand.BatchScript.EvalAsFunc(script, s_EmptyArgs);
                var resSb = new StringBuilder();
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
                    resSb.AppendLine(resultStr);
                }
                var errSb = new StringBuilder();
                if (NativeApi.HasApiErrorInfo) {
                    hasError = true;
                    errSb.AppendLine();
                    errSb.AppendLine(NativeApi.GetApiErrorInfo());
                }
                if (BatchCommand.BatchScript.HasDslErrors) {
                    hasError = true;
                    errSb.AppendLine();
                    errSb.AppendLine(BatchCommand.BatchScript.GetDslErrors());
                }
                return Lib.GetMetaDslResult(maxResultSize, resSb, errSb);
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
            BatchCommand.BatchScript.Register("setdslfile", "setdslfile(dsl_file,...)", false, new ExpressionFactoryHelper<SetDslFileExp>());
            BatchCommand.BatchScript.Register("import", "import(dsl_file,...)", false, new ExpressionFactoryHelper<ImportExp>());
            BatchCommand.BatchScript.Register("redirectcall", "redirectcall(func_name) or redirectcall(func_name,args) or redirectcall(func_name, args, ...)", false, new ExpressionFactoryHelper<RedirectCallExp>());
            BatchCommand.BatchScript.Register("executemetadsl", "executemetadsl(dsl_code), return (bool, result)", false, new ExpressionFactoryHelper<ExecuteMetaDslExp>());
            BatchCommand.BatchScript.Register("execute_metadsl", "execute_metadsl(dsl_code), return (bool, result)", new ExpressionFactoryHelper<ExecuteMetaDslExp>());
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
            BatchCommand.BatchScript.Register("enqueue_cef_message", "enqueue_cef_message(msg,arg1,arg2,...), enqueue message to browser", false, new ExpressionFactoryHelper<EnqueueCefMessageExp>());
            BatchCommand.BatchScript.Register("get_string_in_length", "get_string_in_length(str,len[,begin0_end1_or_beginend2])", new ExpressionFactoryHelper<GetStringInLengthExp>());
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
