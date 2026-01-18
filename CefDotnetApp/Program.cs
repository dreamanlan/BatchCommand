
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
using CefDotnetApp.Interfaces;
using System.Text.RegularExpressions;

public static class Program
{
    public static void Main()
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

namespace DotNetLib
{
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
                    sb.AppendLine(info);
                }
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
                    sb.AppendLine(info);
                }
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
    public class NativeApi
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
        }

        public void NativeLog(string msg)
        {
            if (m_NativeLogApi == null) {
                return;
            }
            m_NativeLogApi.Invoke(msg, Browser, Frame);
        }
        public void JavascriptLog(string msg)
        {
            SendJavascriptCall("console.log", new string[] { msg });
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
            ApiErrorInfo.AppendFormat(fmt, args);
        }
        public void AppendApiErrorInfoFormatLineForDSL(string fmt, params object[] args)
        {
            ApiErrorInfo.AppendFormat(fmt, args);
            ApiErrorInfo.AppendLine();
        }
        public bool HasApiErrorInfoForDSL => ApiErrorInfo.Length > 0;
        public string GetApiErrorInfoForDSL() => ApiErrorInfo.ToString();

        public static void ClearApiErrorInfo()
        {
            ApiErrorInfo.Clear();
        }
        public static void AppendApiErrorInfo(string msg)
        {
            ApiErrorInfo.Append(msg);
        }
        public static void AppendApiErrorInfoLine(string msg)
        {
            ApiErrorInfo.AppendLine(msg);
        }
        public static void AppendApiErrorInfoFormat(string fmt, params object[] args)
        {
            ApiErrorInfo.AppendFormat(fmt, args);
        }
        public static void AppendApiErrorInfoFormatLine(string fmt, params object[] args)
        {
            ApiErrorInfo.AppendFormat(fmt, args);
            ApiErrorInfo.AppendLine();
        }
        public static bool HasApiErrorInfo => ApiErrorInfo.Length > 0;
        public static string GetApiErrorInfo() => ApiErrorInfo.ToString();

        public static StringBuilder ApiErrorInfo
        {
            get {
                if (s_ApiErrorInfo == null) {
                    s_ApiErrorInfo = new StringBuilder();
                }
                return s_ApiErrorInfo!;
            }
        }
        public static void SetContext(IntPtr browser, IntPtr frame)
        {
            s_Browser = browser;
            s_Frame = frame;
        }
        public static void SetLastContext(IntPtr browser, IntPtr frame)
        {
            s_LastBrowser = browser;
            s_LastFrame = frame;
        }
        public static void SetCommandLine(IntPtr command_line)
        {
            s_CommandLine = command_line;
        }
        public static nint Browser
        {
            get => IntPtr.Zero != s_LastBrowser ? s_LastBrowser : s_Browser;
            set => s_Browser = value;
        }
        public static nint Frame
        {
            get => IntPtr.Zero != s_LastFrame ? s_LastFrame : s_Frame;
            set => s_Frame = value;
        }
        public static int LastSourceProcessId { get => s_LastSourceProcessId; set => s_LastSourceProcessId = value; }

        public bool HasSwitch(string name)
        {
            if (s_CommandLine == IntPtr.Zero || string.IsNullOrEmpty(name) || m_CommandLineHasSwitchApi == null) {
                return false;
            }
            return m_CommandLineHasSwitchApi(s_CommandLine, name);
        }

        public string GetSwitchValue(string name)
        {
            if (s_CommandLine == IntPtr.Zero || string.IsNullOrEmpty(name) || m_CommandLineGetSwitchValueApi == null) {
                return string.Empty;
            }
            IntPtr resultPtr = m_CommandLineGetSwitchValueApi(s_CommandLine, name);
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

        public void AppendSwitch(string name)
        {
            if (s_CommandLine == IntPtr.Zero || string.IsNullOrEmpty(name) || m_CommandLineAppendSwitchApi == null) {
                return;
            }
            m_CommandLineAppendSwitchApi(s_CommandLine, name);
        }

        public void AppendSwitchWithValue(string name, string value)
        {
            if (s_CommandLine == IntPtr.Zero || string.IsNullOrEmpty(name) || m_CommandLineAppendSwitchWithValueApi == null) {
                return;
            }
            m_CommandLineAppendSwitchWithValueApi(s_CommandLine, name, value ?? string.Empty);
        }

        public void RemoveSwitch(string name)
        {
            if (s_CommandLine == IntPtr.Zero || string.IsNullOrEmpty(name) || m_CommandLineRemoveSwitchApi == null) {
                return;
            }
            m_CommandLineRemoveSwitchApi(s_CommandLine, name);
        }

        internal void EnqueueNativeLog(string log)
        {
            s_NativeLogQueue.Enqueue(log);
        }

        internal void EnqueueJsLog(string log)
        {
            s_JsLogQueue.Enqueue(log);
        }

        internal void HandleAllQueues(int maxNativeCount, int maxJsCount, int maxCodeCount, int maxFuncCount)
        {
            bool isMainThread = Thread.CurrentThread.ManagedThreadId == Lib.MainThreadId;
            if (!isMainThread) {
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

        [ThreadStatic]
        private static IntPtr s_Browser = IntPtr.Zero;
        [ThreadStatic]
        private static IntPtr s_Frame = IntPtr.Zero;
        [ThreadStatic]
        private static IntPtr s_LastBrowser = IntPtr.Zero;
        [ThreadStatic]
        private static IntPtr s_LastFrame = IntPtr.Zero;
        [ThreadStatic]
        private static int s_LastSourceProcessId = -1;
        [ThreadStatic]
        private static IntPtr s_CommandLine = IntPtr.Zero;
        [ThreadStatic]
        private static StringBuilder? s_ApiErrorInfo = null;

        private static System.Collections.Concurrent.ConcurrentQueue<string> s_NativeLogQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();
        private static System.Collections.Concurrent.ConcurrentQueue<string> s_JsLogQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();
        private static System.Collections.Concurrent.ConcurrentQueue<string> s_JavascriptCodeQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();
        private static System.Collections.Concurrent.ConcurrentQueue<Tuple<string, string[]>> s_JavascriptFuncQueue = new System.Collections.Concurrent.ConcurrentQueue<Tuple<string, string[]>>();

        private const int c_max_path_length = 1024;
        private const int c_max_info_length = 4096;
    }
    public static class Lib
    {
        [UnmanagedCallersOnly]
        public static int RegisterApi(IntPtr apis)
        {
            s_NativeApi = new NativeApi(apis);
            //We must load AgentCore's dependencies before loading AgentCore itself.
            PrepareBatchScript();
            return 0;
        }

        public delegate void OnInitDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string cmd_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, int process_type, [MarshalAs(UnmanagedType.LPUTF8Str)] string app_dir, bool is_mac);
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
        public delegate void OnLoadErrorDelegation(IntPtr browser, IntPtr frame, int error_code, [MarshalAs(UnmanagedType.LPUTF8Str)] string error_text, [MarshalAs(UnmanagedType.LPUTF8Str)] string failed_url);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnRenderProcessTerminatedDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string startup_url, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, int status, int error_code, [MarshalAs(UnmanagedType.LPUTF8Str)] string error_string);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool OnLoadEndDelegation(IntPtr browser, IntPtr frame, [MarshalAs(UnmanagedType.LPUTF8Str)] string url, int http_status_code, bool inject_all_frame, bool is_main, IntPtr js_code, ref int code_size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnReceiveCefMessageDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, IntPtr args, int argCount, IntPtr browser, IntPtr frame, int source_process_id);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnReceiveJsMessageDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, IntPtr args, int argCount, IntPtr browser, IntPtr frame);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool OnExecuteMetaDSLDelegation(IntPtr args, int argCount, IntPtr resultStr, ref int resultSize, IntPtr browser, IntPtr frame);

        public static void OnInit(string cmd_line, string path, int process_type, string app_dir, bool is_mac)
        {
            s_MainThreadId = Thread.CurrentThread.ManagedThreadId;

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

                        // Register assembly resolve event handler before loading AgentCore
                        // This ensures that assembly dependencies are loaded from the correct location
                        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

                        // Only load AgentCore and hot reload manager in renderer process
                        // Load AgentCore plugin
                        bool loadSuccess = LoadAgentPlugin();
                        if (loadSuccess && s_AgentPlugin != null) {
                            NativeLogNoLock("[csharp] AgentPlugin loaded successfully");

                            // Set native API for the plugin
                            s_AgentPlugin.SetNativeApi(s_NativeApi);
                            NativeLogNoLock("[csharp] NativeApi set successfully for AgentPlugin");

                            // Initialize hot reload manager
                            if (s_HotReloadManager == null) {
                                s_HotReloadManager = new HotReloadManager(s_BasePath);
                                s_HotReloadManager.SetCallback(OnFileChanged);
                                s_HotReloadManager.StartWatching();
                                NativeLogNoLock("[csharp] Hot reload manager started in renderer process");
                            }
                        }
                        else {
                            NativeLogNoLock("[csharp] Warning: AgentPlugin loading failed, agent features will not be available");
                        }
                    }

                    TryLoadDSL();
                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                    BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                    BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
                    BoxedValue r = BatchCommand.BatchScript.Call("on_init");
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
        }
        public static void OnFinalize()
        {
            NativeLogNoLock("[csharp] Finalize");

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_finalize"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                    BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                    BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
                    BoxedValue r = BatchCommand.BatchScript.Call("on_finalize");
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }

            if (null != s_AgentPlugin) {
                s_AgentPlugin.Shutdown();
            }

            NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
            NativeApi.SetLastContext(IntPtr.Zero, IntPtr.Zero);
            NativeApi.LastSourceProcessId = -1;
        }

        public static void OnBrowserInit(IntPtr browser)
        {
            s_MainThreadId = Thread.CurrentThread.ManagedThreadId;
            NativeApi.SetContext(browser, IntPtr.Zero);
            NativeLogNoLock("[csharp] Browser Init");

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_browser_init"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                    BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                    BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
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

        public static void OnBrowserFinalize(IntPtr browser)
        {
            NativeApi.SetContext(browser, IntPtr.Zero);
            NativeLogNoLock("[csharp] Browser Finalize");

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_browser_finalize"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                    BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                    BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
                    BoxedValue r = BatchCommand.BatchScript.Call("on_browser_finalize");
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }

            NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
            NativeApi.SetLastContext(IntPtr.Zero, IntPtr.Zero);
            NativeApi.LastSourceProcessId = -1;
        }

        public static bool OnBrowserHotReloadCopyFiles(string url)
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

        public static void OnBrowserHotReloadCompleted(IntPtr browser, IntPtr frame, string url)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock("[csharp] Browser Hot Reload Completed, url: " + url);

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_browser_hot_reload_completed"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                    BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                    BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
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

        public static int OnBrowserCefQuery(IntPtr browser, IntPtr frame, long query_id, string request, bool persistent)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock(string.Format("[csharp] Browser Cef Query: query_id={0}, request={1}, persistent={2}", query_id, request, persistent));

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_browser_cef_query"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                    BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                    BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
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

        public static void OnRendererInit(IntPtr browser, IntPtr frame, string url)
        {
            s_MainThreadId = Thread.CurrentThread.ManagedThreadId;
            NativeApi.SetContext(browser, frame);

            NativeLogNoLock($"[csharp] Renderer Init, url={url}");

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_renderer_init"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                    BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                    BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
                    BatchCommand.BatchScript.SetGlobalVariable("startupurl", BoxedValue.From(url));
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

        public static void OnRendererFinalize(IntPtr browser, IntPtr frame)
        {
            NativeApi.SetContext(browser, frame);
            NativeLogNoLock("[csharp] Renderer Finalize");

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_renderer_finalize"));

                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                    BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                    BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
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
            NativeApi.SetLastContext(IntPtr.Zero, IntPtr.Zero);
            NativeApi.LastSourceProcessId = -1;
        }

        public static bool OnLoadEnd(IntPtr browser, IntPtr frame, string url, int http_status_code, bool inject_all_frame, bool is_main, IntPtr js_code, ref int code_size)
        {
            NativeApi.SetLastContext(browser, frame);
            NativeLogNoLock($"[csharp] OnLoadEnd: url={url}, http_status_code={http_status_code}, inject_all_frame={inject_all_frame}, is_main={is_main}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                    BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                    BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
                    BatchCommand.BatchScript.SetGlobalVariable("loadedurl", BoxedValue.FromString(url));
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

        public static void OnLoadingStateChange(IntPtr browser, IntPtr frame, string url, bool is_loading, bool can_go_back, bool can_go_forward)
        {
            NativeApi.SetLastContext(browser, frame);
            NativeLogNoLock($"[csharp] OnLoadingStateChange: url={url}, is_loading={is_loading}, can_go_back={can_go_back}, can_go_forward={can_go_forward}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                    BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                    BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
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

        public static void OnLoadError(IntPtr browser, IntPtr frame, int error_code, string error_text, string failed_url)
        {
            NativeApi.SetLastContext(browser, frame);
            NativeLogNoLock($"[csharp] OnLoadError: error_code={error_code}, error_text={error_text}, failed_url={failed_url}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                    BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                    BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
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

        public static void OnRenderProcessTerminated(IntPtr browser, IntPtr frame, string startup_url, string url, int status, int error_code, string error_string)
        {
            NativeApi.SetLastContext(browser, frame);
            NativeLogNoLock($"[csharp] OnRenderProcessTerminated: startup_url={startup_url}, url={url}, status={status}, error_code={error_code}, error_string={error_string}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                    BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                    BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
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

        public static void OnBeforeCommandLineProcessing(int process_type, IntPtr command_line)
        {
            NativeApi.SetCommandLine(command_line);
            NativeLogNoLock($"[csharp] OnBeforeCommandLineProcessing: process_type={process_type}");

            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();

                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                    BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                    BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    vargs.Add(BoxedValue.From(process_type));
                    BatchCommand.BatchScript.Call("on_before_command_line_processing", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception in OnBeforeCommandLineProcessing:" + e.Message + "\n" + e.StackTrace);
            }

            NativeApi.SetCommandLine(IntPtr.Zero);
        }

        public static void OnReceiveCefMessage(string msg, IntPtr args, int argCount, IntPtr browser, IntPtr frame, int source_process_id)
        {
            string[] argArray = new string[argCount];
            for (int i = 0; i < argCount; i++) {
                IntPtr strPtr = Marshal.ReadIntPtr(args, i * IntPtr.Size);
                argArray[i] = Marshal.PtrToStringUTF8(strPtr) ?? string.Empty;
            }
            OnReceiveCefMessage(msg, new List<string>(argArray), browser, frame, source_process_id);
        }

        public static void OnReceiveJsMessage(string msg, IntPtr args, int argCount, IntPtr browser, IntPtr frame)
        {
            string[] argArray = new string[argCount];
            for (int i = 0; i < argCount; i++) {
                IntPtr strPtr = Marshal.ReadIntPtr(args, i * IntPtr.Size);
                argArray[i] = Marshal.PtrToStringUTF8(strPtr) ?? string.Empty;
            }
            OnReceiveJsMessage(msg, new List<string>(argArray), browser, frame);
        }

        public static bool OnExecuteMetaDSL(IntPtr args, int argCount, IntPtr resultStr, ref int resultSize, IntPtr browser, IntPtr frame)
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

        public static string OnExecuteMetaDSL(List<string> args, IntPtr browser, IntPtr frame)
        {
            lock (s_Lock) {
                NativeApi.SetLastContext(browser, frame);

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

        public static void OnReceiveCefMessage(string msg, List<string> args, IntPtr browser, IntPtr frame, int source_process_id)
        {
            lock (s_Lock) {
                NativeApi.SetLastContext(browser, frame);
                NativeApi.LastSourceProcessId = source_process_id;

                try {
                    NativeLogNoLock(string.Format("[csharp] Call csharp OnReceiveCefMessage, msg:{0} arg:{1} from process:{2} process type:{3}", msg, string.Join(",", args), source_process_id, s_ProcessType));

                    if (null != s_NativeApi) {

                        BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                        BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                        BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                        BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                        BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                        BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
                        BatchCommand.BatchScript.SetGlobalVariable("sourceprocessid", BoxedValue.From(source_process_id));
                        var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
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

        public static void OnReceiveJsMessage(string msg, List<string> args, IntPtr browser, IntPtr frame)
        {
            lock (s_Lock) {
                NativeApi.SetLastContext(browser, frame);

                try {
                    NativeLogNoLock(string.Format("[csharp] Call csharp OnReceiveJsMessage, msg:{0} arg:{1} process type:{2}", msg, string.Join(",", args), s_ProcessType));

                    if (null != s_NativeApi) {

                        BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                        BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                        BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                        BatchCommand.BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(s_AppDir));
                        BatchCommand.BatchScript.SetGlobalVariable("ismac", BoxedValue.From(s_IsMac));
                        BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
                        BatchCommand.BatchScript.SetGlobalVariable("sourceprocessid", BoxedValue.From(NativeApi.LastSourceProcessId));
                        var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
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

        public static string BasePath
        {
            get {
                return s_BasePath;
            }
        }

        public static IAgentPlugin? AgentPlugin
        {
            get {
                return s_AgentPlugin;
            }
        }

        public static int MainThreadId
        {
            get {
                return s_MainThreadId;
            }
        }

        public static void NativeLog(string msg)
        {
            lock (s_Lock) {
                NativeLogNoLock(msg);
            }
        }
        public static void JsLog(string msg)
        {
            lock (s_Lock) {
                JsLogNoLock(msg);
            }
        }
        public static void HandleThreadQueue(int maxNativeCount, int maxJsCount, int maxCodeCount, int maxFuncCount)
        {
            bool isMainThread = Thread.CurrentThread.ManagedThreadId == s_MainThreadId;
            if (isMainThread && null != s_NativeApi) {
                s_NativeApi.HandleAllQueues(maxNativeCount, maxJsCount, maxCodeCount, maxFuncCount);
            }
        }
        internal static void NativeLogNoLock(string msg)
        {
            if (null != s_NativeApi) {
                bool isMainThread = Thread.CurrentThread.ManagedThreadId == s_MainThreadId;
                string txt = string.Format("thread:{0} {1}{2}: {3}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name, isMainThread ? "(main)" : string.Empty, msg);
                //Console.WriteLine(txt);
                var lines = txt.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines) {
                    if (isMainThread) {
                        s_NativeApi.NativeLog(line);
                    }
                    else {
                        s_NativeApi.EnqueueNativeLog(line);
                    }
                }
            }
        }
        internal static void JsLogNoLock(string msg)
        {
            if (null != s_NativeApi) {
                bool isMainThread = Thread.CurrentThread.ManagedThreadId == s_MainThreadId;
                string txt = string.Format("thread:{0} {1}{2}: {3}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name, isMainThread ? "(main)" : string.Empty, msg);
                //Console.WriteLine(txt);
                var lines = txt.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines) {
                    if (isMainThread) {
                        s_NativeApi.JavascriptLog(line);
                    }
                    else {
                        s_NativeApi.EnqueueJsLog(line);
                    }
                }
            }
        }

        private static void TryLoadDSL()
        {
            PrepareBatchScript();
            string path = Path.Combine(s_BasePath, "./managed/Script.dsl");
            var fi = new FileInfo(path);
            if (fi.Exists) {
                if (fi.LastWriteTime != s_DslScriptTime || s_DslScriptPath != path) {
                    s_DslScriptTime = fi.LastWriteTime;
                    s_DslScriptPath = path;

                    string errorMsg = string.Empty;
                    if (File.Exists(fi.FullName)) {
                        BatchCommand.BatchScript.Load(fi.FullName);
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
        }
        // Execute MetaDSL script
        private static string ExecuteMetaDslScript(string script)
        {
            try {
                // Execute the script directly using the DSL interpreter
                BatchCommand.BatchScript.ClearDslErrors();
                NativeApi.ClearApiErrorInfo();
                var id = BatchCommand.BatchScript.EvalAsFunc(script, s_EmptyArgs);
                var sb = new StringBuilder();
                if (!BatchCommand.BatchScript.HasDslErrors) {
                    var result = BatchCommand.BatchScript.Call(id);
                    if (result.IsNullObject) {
                        sb.AppendLine("null");
                    }
                    else if (null != s_AgentPlugin) {
                        sb.AppendLine(s_AgentPlugin.ResultToString(result));
                    }
                    else {
                        sb.AppendLine(result.ToString());
                    }
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

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try {
                string? assemblyName = new AssemblyName(args.Name).Name;
                NativeLogNoLock($"[csharp] AssemblyResolve: Requesting {assemblyName}");

                // If the assembly is already loaded, return it
                Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in loadedAssemblies) {
                    if (assembly.GetName().Name == assemblyName) {
                        NativeLogNoLock($"[csharp] AssemblyResolve: {assemblyName} already loaded");
                        return assembly;
                    }
                }

                // Try to load from the managed directory
                string managedPath = Path.Combine(s_BasePath, "managed");
                string assemblyPath = Path.Combine(managedPath, assemblyName + ".dll");

                if (File.Exists(assemblyPath)) {
                    NativeLogNoLock($"[csharp] AssemblyResolve: Loading {assemblyName} from {assemblyPath}");
                    return Assembly.LoadFrom(assemblyPath);
                }

                NativeLogNoLock($"[csharp] AssemblyResolve: Could not find {assemblyName} in {managedPath}");
                return null;
            }
            catch (Exception ex) {
                NativeLogNoLock($"[csharp] AssemblyResolve exception: {ex.Message}");
                return null;
            }
        }

        private static bool LoadAgentPlugin()
        {
            try {
                string pluginPath = Path.Combine(s_BasePath, "managed", "AgentCore.dll");
                if (!File.Exists(pluginPath)) {
                    NativeLogNoLock($"[csharp] AgentCore.dll not found at: {pluginPath}");
                    return false;
                }

                NativeLogNoLock($"[csharp] Loading AgentCore.dll from: {pluginPath}");

                // Use Assembly.Load instead of LoadFrom to share assembly load context
                // This ensures interface types are compatible across assemblies
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(pluginPath);
                NativeLogNoLock($"[csharp] Assembly name: {assemblyName.FullName}");

                // Load the assembly into the default load context
                Assembly? pluginAssembly = Assembly.Load(assemblyName);
                if (pluginAssembly == null) {
                    NativeLogNoLock("[csharp] Failed to load AgentCore.dll using Assembly.Load");
                    // Fallback to LoadFrom if Load fails
                    NativeLogNoLock("[csharp] Trying fallback to Assembly.LoadFrom...");
                    pluginAssembly = Assembly.LoadFrom(pluginPath);
                }

                NativeLogNoLock($"[csharp] Loaded assembly: {pluginAssembly.FullName}");

                // Find the AgentPlugin type
                Type? pluginType = pluginAssembly.GetType("CefDotnetApp.AgentCore.AgentPlugin");
                if (pluginType == null) {
                    NativeLogNoLock("[csharp] AgentPlugin type not found in AgentCore.dll");
                    // List all available types for debugging
                    NativeLogNoLock("[csharp] Available types in AgentCore.dll:");
                    foreach (var type in pluginAssembly.GetTypes()) {
                        NativeLogNoLock($"[csharp]   - {type.FullName}");
                    }
                    return false;
                }

                NativeLogNoLock($"[csharp] Found AgentPlugin type: {pluginType.FullName}");

                // Create instance and use interface casting
                object? pluginInstance = Activator.CreateInstance(pluginType);
                if (pluginInstance == null) {
                    NativeLogNoLock("[csharp] Failed to create AgentPlugin instance (Activator.CreateInstance returned null)");
                    return false;
                }

                NativeLogNoLock($"[csharp] Created instance of type: {pluginInstance.GetType().FullName}");

                // Try to cast to IAgentPlugin
                s_AgentPlugin = pluginInstance as IAgentPlugin;
                if (s_AgentPlugin == null) {
                    NativeLogNoLock("[csharp] Failed to cast AgentPlugin instance to IAgentPlugin");

                    // Check if instance implements the interface
                    Type interfaceType = typeof(IAgentPlugin);
                    NativeLogNoLock($"[csharp] IAgentPlugin interface: {interfaceType.AssemblyQualifiedName}");
                    NativeLogNoLock($"[csharp] Instance implements IAgentPlugin: {interfaceType.IsAssignableFrom(pluginType)}");

                    // List all implemented interfaces
                    NativeLogNoLock("[csharp] Interfaces implemented by AgentPlugin:");
                    foreach (var iface in pluginType.GetInterfaces()) {
                        NativeLogNoLock($"[csharp]   - {iface.AssemblyQualifiedName}");
                    }

                    return false;
                }

                NativeLogNoLock("[csharp] Successfully cast to IAgentPlugin");

                // Initialize the plugin
                s_AgentPlugin.Initialize(s_BasePath, s_AppDir, s_IsMac);
                NativeLogNoLock("[csharp] AgentPlugin loaded and initialized successfully");

                // Register Script APIs through the plugin
                s_AgentPlugin.RegisterScriptApis();
                NativeLogNoLock("[csharp] Script APIs registered successfully");

                return true;
            }
            catch (Exception ex) {
                NativeLogNoLock($"[csharp] Error loading AgentPlugin: {ex.Message}");
                NativeLogNoLock($"[csharp] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null) {
                    NativeLogNoLock($"[csharp] Inner exception: {ex.InnerException.Message}");
                    NativeLogNoLock($"[csharp] Inner stack trace: {ex.InnerException.StackTrace}");
                }
                return false;
            }
        }

        private static void RegisterBatchScriptApi()
        {
            CefDotnetAppApi.AddCommonApiDocs();
            // Basic framework APIs (defined in Program.cs)
            BatchCommand.BatchScript.Register("nativelog", "nativelog(fmt, ...)", new ExpressionFactoryHelper<NativeLogExp>());
            BatchCommand.BatchScript.Register("javascriptlog", "javascriptlog(fmt, ...)", new ExpressionFactoryHelper<JavascriptLogExp>());
            BatchCommand.BatchScript.Register("help", "help(pattern, ...), agent api help", new ExpressionFactoryHelper<HelpExp>());
            BatchCommand.BatchScript.Register("helpall", "helpall(pattern, ...), agent and framework api help", new ExpressionFactoryHelper<HelpAllExp>());

            // Agent-related APIs are registered by AgentCore plugin via LoadAgentPlugin()

            // Only valid in MainThread
            BatchCommand.BatchScript.Register("handlethreadqueue", "handlethreadqueue([max_native_logs,max_js_logs,max_code_count,max_func_count]), only valid in main thread", new ExpressionFactoryHelper<HandleThreadQueueExp>());
        }

        private static void OnFileChanged(string filePath, string fileType)
        {
            NativeLogNoLock($"[csharp] File changed detected: {fileType} - {filePath}");

            try {
                switch (fileType) {
                    case "DSL Script":
                        // Script.dsl is reloaded automatically on next message processing
                        NativeLogNoLock("[csharp] Script.dsl will be reloaded on next message");
                        break;

                    case "AgentCore DLL":
                        // Need to reload the plugin - this requires careful handling
                        NativeLogNoLock("[csharp] AgentCore.dll changed - hot reload scheduled");
                        // Note: Full plugin reload requires app restart or careful unloading
                        break;

                    case "Inject Script":
                        // Directly trigger hot reload in current renderer process
                        // HotReloadManager only runs in renderer, so we can directly execute JavaScript
                        if (s_NativeApi != null) {
                            NativeLogNoLock("[csharp] Inject script changed, triggering hot reload directly");
                            // All C# to JS calls go through window object methods
                            s_NativeApi.SendJavascriptCode("window.onAgentCommand(JSON.stringify({command:'hot_reload',params:{component:'inject'}}))");
                        }
                        break;
                }
            }
            catch (Exception ex) {
                NativeLogNoLock($"[csharp] Error handling file change: {ex.Message}");
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

        private static bool s_BatchScriptInited = false;
        private static string s_CmdLine = string.Empty;
        private static string s_BasePath = string.Empty;
        private static string s_AppDir = string.Empty;
        private static bool s_IsMac = false;
        private static int s_ProcessType = -1;
        private static string s_DslScriptPath = string.Empty;
        private static DateTime s_DslScriptTime = DateTime.Now;
        private static int s_MainThreadId = 0;
        private static object s_Lock = new object();
        private static IAgentPlugin? s_AgentPlugin = null;
        private static HotReloadManager? s_HotReloadManager = null;

        private static List<string> s_EmptyArgs = new List<string>();
        private static StringBuilder s_StringBuilder = new StringBuilder();
        private static StringWriter s_StringWriter = new StringWriter(s_StringBuilder);
        private static NativeApi? s_NativeApi;
    }
    public static class CefDotnetAppApi
    {
        // Execute MetaDSL script
        public static string ExecuteMetaDslScript(string script)
        {
            try {
                PrepareBatchScript();
                // Execute the script directly using the DSL interpreter
                BatchCommand.BatchScript.ClearDslErrors();
                NativeApi.ClearApiErrorInfo();
                var id = BatchCommand.BatchScript.EvalAsFunc(script, s_EmptyArgs);
                var sb = new StringBuilder();
                if (!BatchCommand.BatchScript.HasDslErrors) {
                    var result = BatchCommand.BatchScript.Call(id);
                    if (result.IsNullObject) {
                        sb.Append("null");
                    }
                    else if (null != Lib.AgentPlugin) {
                        sb.Append(Lib.AgentPlugin.ResultToString(result));
                    }
                    else {
                        sb.Append(result.ToString());
                    }
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
                Lib.NativeLog($"[AgentCommand] Error executing MetaDSL script: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
        internal static void AddCommonApiDocs()
        {
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
        private static void RegisterBatchScriptApi()
        {
            AddCommonApiDocs();

            // Basic framework APIs (defined in Program.cs)
            BatchCommand.BatchScript.Register("nativelog", "nativelog(fmt, ...)", new ExpressionFactoryHelper<NativeLogExp>());
            BatchCommand.BatchScript.Register("javascriptlog", "javascriptlog(fmt, ...)", new ExpressionFactoryHelper<JavascriptLogExp>());
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

        private static bool s_BatchScriptInited = false;
        private static List<string> s_EmptyArgs = new List<string>();
    }
}