
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.IO;
using ScriptableFramework;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Net;

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
    public IntPtr SendCefMessage0;
    public IntPtr SendCefMessage1;
    public IntPtr SendCefMessage2;
    public IntPtr SendCefMessage3;
    public IntPtr SendCefMessage4;
    public IntPtr SendCefMessage5;
    public IntPtr SendCefMessage6;
    public IntPtr SendCefMessage7;
    public IntPtr SendCefMessage8;
    public IntPtr SendCefMessage9;
    public IntPtr ExecuteJavascript;
    public IntPtr CallJavascript0;
    public IntPtr CallJavascript1;
    public IntPtr CallJavascript2;
    public IntPtr CallJavascript3;
    public IntPtr CallJavascript4;
    public IntPtr CallJavascript5;
    public IntPtr CallJavascript6;
    public IntPtr CallJavascript7;
    public IntPtr CallJavascript8;
    public IntPtr CallJavascript9;
}

// delegate for native api
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostNativeLogDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostSendCefMessage0Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, IntPtr browser, IntPtr frame, int cef_process_id);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostSendCefMessage1Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg, IntPtr browser, IntPtr frame, int cef_process_id);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostSendCefMessage2Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, IntPtr browser, IntPtr frame, int cef_process_id);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostSendCefMessage3Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, IntPtr browser, IntPtr frame, int cef_process_id);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostSendCefMessage4Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, IntPtr browser, IntPtr frame, int cef_process_id);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostSendCefMessage5Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, IntPtr browser, IntPtr frame, int cef_process_id);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostSendCefMessage6Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, IntPtr browser, IntPtr frame, int cef_process_id);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostSendCefMessage7Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg7, IntPtr browser, IntPtr frame, int cef_process_id);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostSendCefMessage8Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg7, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg8, IntPtr browser, IntPtr frame, int cef_process_id);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostSendCefMessage9Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg7, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg8, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg9, IntPtr browser, IntPtr frame, int cef_process_id);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostExecuteJavascriptDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string code, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCallJavascript0Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCallJavascript1Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCallJavascript2Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCallJavascript3Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCallJavascript4Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCallJavascript5Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCallJavascript6Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCallJavascript7Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg7, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCallJavascript8Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg7, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg8, IntPtr browser, IntPtr frame);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostCallJavascript9Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg7, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg8, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg9, IntPtr browser, IntPtr frame);

namespace DotNetLib
{
    sealed class NativeLogExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            string fmt = string.Empty;
            var al = new System.Collections.ArrayList();
            for (int ix = 0; ix < operands.Count; ix++) {
                BoxedValue v = operands[ix];
                if (ix == 0) {
                    fmt = v.AsString;
                }
                else {
                    al.Add(v.GetObject());
                }
            }
            string str = string.Format(fmt, al.ToArray());
            Lib.NativeLogNoLock(str);
            return str;
        }
    }
    sealed class JavascriptLogExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            string fmt = string.Empty;
            var al = new System.Collections.ArrayList();
            for (int ix = 0; ix < operands.Count; ix++) {
                BoxedValue v = operands[ix];
                if (ix == 0) {
                    fmt = v.AsString;
                }
                else {
                    al.Add(v.GetObject());
                }
            }
            string str = string.Format(fmt, al.ToArray());
            Lib.JsLogNoLock(str);
            return str;
        }
    }
    sealed class WriteFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            bool r = false;
            if (operands.Count >= 2) {
                string path = operands[0].GetString();
                string txt = operands[1].GetString();
                path = Path.Combine(Lib.BasePath, path);
                string? dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(path, txt);
            }
            return r;
        }
    }
    enum CefProcessType
    {
        BrowserProcess,
        RendererProcess,
        ZygoteProcess,
        OtherProcess,
    };
    enum CefProcessId
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
            m_SendCefMessage0Api = Marshal.GetDelegateForFunctionPointer<HostSendCefMessage0Delegation>(hostApi.SendCefMessage0);
            m_SendCefMessage1Api = Marshal.GetDelegateForFunctionPointer<HostSendCefMessage1Delegation>(hostApi.SendCefMessage1);
            m_SendCefMessage2Api = Marshal.GetDelegateForFunctionPointer<HostSendCefMessage2Delegation>(hostApi.SendCefMessage2);
            m_SendCefMessage3Api = Marshal.GetDelegateForFunctionPointer<HostSendCefMessage3Delegation>(hostApi.SendCefMessage3);
            m_SendCefMessage4Api = Marshal.GetDelegateForFunctionPointer<HostSendCefMessage4Delegation>(hostApi.SendCefMessage4);
            m_SendCefMessage5Api = Marshal.GetDelegateForFunctionPointer<HostSendCefMessage5Delegation>(hostApi.SendCefMessage5);
            m_SendCefMessage6Api = Marshal.GetDelegateForFunctionPointer<HostSendCefMessage6Delegation>(hostApi.SendCefMessage6);
            m_SendCefMessage7Api = Marshal.GetDelegateForFunctionPointer<HostSendCefMessage7Delegation>(hostApi.SendCefMessage7);
            m_SendCefMessage8Api = Marshal.GetDelegateForFunctionPointer<HostSendCefMessage8Delegation>(hostApi.SendCefMessage8);
            m_SendCefMessage9Api = Marshal.GetDelegateForFunctionPointer<HostSendCefMessage9Delegation>(hostApi.SendCefMessage9);
            m_ExecuteJavascriptApi = Marshal.GetDelegateForFunctionPointer<HostExecuteJavascriptDelegation>(hostApi.ExecuteJavascript);
            m_CallJavascript0Api = Marshal.GetDelegateForFunctionPointer<HostCallJavascript0Delegation>(hostApi.CallJavascript0);
            m_CallJavascript1Api = Marshal.GetDelegateForFunctionPointer<HostCallJavascript1Delegation>(hostApi.CallJavascript1);
            m_CallJavascript2Api = Marshal.GetDelegateForFunctionPointer<HostCallJavascript2Delegation>(hostApi.CallJavascript2);
            m_CallJavascript3Api = Marshal.GetDelegateForFunctionPointer<HostCallJavascript3Delegation>(hostApi.CallJavascript3);
            m_CallJavascript4Api = Marshal.GetDelegateForFunctionPointer<HostCallJavascript4Delegation>(hostApi.CallJavascript4);
            m_CallJavascript5Api = Marshal.GetDelegateForFunctionPointer<HostCallJavascript5Delegation>(hostApi.CallJavascript5);
            m_CallJavascript6Api = Marshal.GetDelegateForFunctionPointer<HostCallJavascript6Delegation>(hostApi.CallJavascript6);
            m_CallJavascript7Api = Marshal.GetDelegateForFunctionPointer<HostCallJavascript7Delegation>(hostApi.CallJavascript7);
            m_CallJavascript8Api = Marshal.GetDelegateForFunctionPointer<HostCallJavascript8Delegation>(hostApi.CallJavascript8);
            m_CallJavascript9Api = Marshal.GetDelegateForFunctionPointer<HostCallJavascript9Delegation>(hostApi.CallJavascript9);
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
            CallJavascript1("console.log", msg);
        }
        public void SendCefMessage0(string msg, int cef_process_id)
        {
            if (m_SendCefMessage0Api == null) {
                return;
            }
            m_SendCefMessage0Api.Invoke(msg, Browser, Frame, cef_process_id);
        }
        public void SendCefMessage1(string msg, string arg, int cef_process_id)
        {
            if (m_SendCefMessage1Api == null) {
                return;
            }
            m_SendCefMessage1Api.Invoke(msg, arg, Browser, Frame, cef_process_id);
        }
        public void SendCefMessage2(string msg, string arg1, string arg2, int cef_process_id)
        {
            if (m_SendCefMessage2Api == null) {
                return;
            }
            m_SendCefMessage2Api.Invoke(msg, arg1, arg2, Browser, Frame, cef_process_id);
        }
        public void SendCefMessage3(string msg, string arg1, string arg2, string arg3, int cef_process_id)
        {
            if (m_SendCefMessage3Api == null) {
                return;
            }
            m_SendCefMessage3Api.Invoke(msg, arg1, arg2, arg3, Browser, Frame, cef_process_id);
        }
        public void SendCefMessage4(string msg, string arg1, string arg2, string arg3, string arg4, int cef_process_id)
        {
            if (m_SendCefMessage4Api == null) {
                return;
            }
            m_SendCefMessage4Api.Invoke(msg, arg1, arg2, arg3, arg4, Browser, Frame, cef_process_id);
        }
        public void SendCefMessage5(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, int cef_process_id)
        {
            if (m_SendCefMessage5Api == null) {
                return;
            }
            m_SendCefMessage5Api.Invoke(msg, arg1, arg2, arg3, arg4, arg5, Browser, Frame, cef_process_id);
        }
        public void SendCefMessage6(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, int cef_process_id)
        {
            if (m_SendCefMessage6Api == null) {
                return;
            }
            m_SendCefMessage6Api.Invoke(msg, arg1, arg2, arg3, arg4, arg5, arg6, Browser, Frame, cef_process_id);
        }
        public void SendCefMessage7(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, int cef_process_id)
        {
            if (m_SendCefMessage7Api == null) {
                return;
            }
            m_SendCefMessage7Api.Invoke(msg, arg1, arg2, arg3, arg4, arg5, arg6, arg7, Browser, Frame, cef_process_id);
        }
        public void SendCefMessage8(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, int cef_process_id)
        {
            if (m_SendCefMessage8Api == null) {
                return;
            }
            m_SendCefMessage8Api.Invoke(msg, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, Browser, Frame, cef_process_id);
        }
        public void SendCefMessage9(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string arg9, int cef_process_id)
        {
            if (m_SendCefMessage9Api == null) {
                return;
            }
            m_SendCefMessage9Api.Invoke(msg, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, Browser, Frame, cef_process_id);
        }
        public void ExecuteJavascript(string code)
        {
            if (m_ExecuteJavascriptApi == null) {
                return;
            }
            m_ExecuteJavascriptApi.Invoke(code, Browser, Frame);
        }
        public void CallJavascript0(string func)
        {
            if (m_CallJavascript0Api == null) {
                return;
            }
            m_CallJavascript0Api.Invoke(func, Browser, Frame);
        }
        public void CallJavascript1(string func, string arg)
        {
            if (m_CallJavascript1Api == null) {
                return;
            }
            m_CallJavascript1Api.Invoke(func, arg, Browser, Frame);
        }
        public void CallJavascript2(string func, string arg1, string arg2)
        {
            if (m_CallJavascript2Api == null) {
                return;
            }
            m_CallJavascript2Api.Invoke(func, arg1, arg2, Browser, Frame);
        }
        public void CallJavascript3(string func, string arg1, string arg2, string arg3)
        {
            if (m_CallJavascript3Api == null) {
                return;
            }
            m_CallJavascript3Api.Invoke(func, arg1, arg2, arg3, Browser, Frame);
        }
        public void CallJavascript4(string func, string arg1, string arg2, string arg3, string arg4)
        {
            if (m_CallJavascript4Api == null) {
                return;
            }
            m_CallJavascript4Api.Invoke(func, arg1, arg2, arg3, arg4, Browser, Frame);
        }
        public void CallJavascript5(string func, string arg1, string arg2, string arg3, string arg4, string arg5)
        {
            if (m_CallJavascript5Api == null) {
                return;
            }
            m_CallJavascript5Api.Invoke(func, arg1, arg2, arg3, arg4, arg5, Browser, Frame);
        }
        public void CallJavascript6(string func, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6)
        {
            if (m_CallJavascript6Api == null) {
                return;
            }
            m_CallJavascript6Api.Invoke(func, arg1, arg2, arg3, arg4, arg5, arg6, Browser, Frame);
        }
        public void CallJavascript7(string func, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7)
        {
            if (m_CallJavascript7Api == null) {
                return;
            }
            m_CallJavascript7Api.Invoke(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, Browser, Frame);
        }
        public void CallJavascript8(string func, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8)
        {
            if (m_CallJavascript8Api == null) {
                return;
            }
            m_CallJavascript8Api.Invoke(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, Browser, Frame);
        }
        public void CallJavascript9(string func, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string arg9)
        {
            if (m_CallJavascript9Api == null) {
                return;
            }
            m_CallJavascript9Api.Invoke(func, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, Browser, Frame);
        }
        public void SetContext(IntPtr browser, IntPtr frame)
        {
            m_Browser = browser;
            m_Frame = frame;
        }
        public void SetLastContext(IntPtr browser, IntPtr frame)
        {
            m_LastBrowser = browser;
            m_LastFrame = frame;
        }

        public nint Browser
        {
            get => IntPtr.Zero != m_LastBrowser ? m_LastBrowser : m_Browser;
            set => m_Browser = value;
        }
        public nint Frame
        {
            get => IntPtr.Zero != m_LastFrame ? m_LastFrame : m_Frame;
            set => m_Frame = value;
        }
        public int LastSourceProcessId { get => m_LastSourceProcessId; set => m_LastSourceProcessId = value; }

        private HostNativeLogDelegation? m_NativeLogApi;
        private HostSendCefMessage0Delegation? m_SendCefMessage0Api;
        private HostSendCefMessage1Delegation? m_SendCefMessage1Api;
        private HostSendCefMessage2Delegation? m_SendCefMessage2Api;
        private HostSendCefMessage3Delegation? m_SendCefMessage3Api;
        private HostSendCefMessage4Delegation? m_SendCefMessage4Api;
        private HostSendCefMessage5Delegation? m_SendCefMessage5Api;
        private HostSendCefMessage6Delegation? m_SendCefMessage6Api;
        private HostSendCefMessage7Delegation? m_SendCefMessage7Api;
        private HostSendCefMessage8Delegation? m_SendCefMessage8Api;
        private HostSendCefMessage9Delegation? m_SendCefMessage9Api;
        private HostExecuteJavascriptDelegation? m_ExecuteJavascriptApi;
        private HostCallJavascript0Delegation? m_CallJavascript0Api;
        private HostCallJavascript1Delegation? m_CallJavascript1Api;
        private HostCallJavascript2Delegation? m_CallJavascript2Api;
        private HostCallJavascript3Delegation? m_CallJavascript3Api;
        private HostCallJavascript4Delegation? m_CallJavascript4Api;
        private HostCallJavascript5Delegation? m_CallJavascript5Api;
        private HostCallJavascript6Delegation? m_CallJavascript6Api;
        private HostCallJavascript7Delegation? m_CallJavascript7Api;
        private HostCallJavascript8Delegation? m_CallJavascript8Api;
        private HostCallJavascript9Delegation? m_CallJavascript9Api;
        private IntPtr m_Browser = IntPtr.Zero;
        private IntPtr m_Frame = IntPtr.Zero;
        private IntPtr m_LastBrowser = IntPtr.Zero;
        private IntPtr m_LastFrame = IntPtr.Zero;
        private int m_LastSourceProcessId = -1;

        private const int c_max_path_length = 1024;
        private const int c_max_info_length = 4096;
    }
    public static class Lib
    {
        [UnmanagedCallersOnly]
        public static int RegisterApi(IntPtr apis)
        {
            s_NativeApi = new NativeApi(apis);

            return 0;
        }

        public delegate void OnInitDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string cmd_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, int process_type);
        public delegate void OnFinalizeDelegation();
        public delegate void OnBrowserInitDelegation(IntPtr browser);
        public delegate void OnBrowserFinalizeDelegation(IntPtr browser);
        public delegate void OnRendererInitDelegation(IntPtr browser, IntPtr frame);
        public delegate void OnRendererFinalizeDelegation(IntPtr browser, IntPtr frame);
        public delegate void OnReceiveCefMessage0Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, IntPtr browser, IntPtr frame, int source_process_id);
        public delegate void OnReceiveCefMessage1Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg, IntPtr browser, IntPtr frame, int source_process_id);
        public delegate void OnReceiveCefMessage2Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, IntPtr browser, IntPtr frame, int source_process_id);
        public delegate void OnReceiveCefMessage3Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, IntPtr browser, IntPtr frame, int source_process_id);
        public delegate void OnReceiveCefMessage4Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, IntPtr browser, IntPtr frame, int source_process_id);
        public delegate void OnReceiveCefMessage5Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, IntPtr browser, IntPtr frame, int source_process_id);
        public delegate void OnReceiveCefMessage6Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, IntPtr browser, IntPtr frame, int source_process_id);
        public delegate void OnReceiveCefMessage7Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg7, IntPtr browser, IntPtr frame, int source_process_id);
        public delegate void OnReceiveCefMessage8Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg7, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg8, IntPtr browser, IntPtr frame, int source_process_id);
        public delegate void OnReceiveCefMessage9Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg7, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg8, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg9, IntPtr browser, IntPtr frame, int source_process_id);
        public delegate void OnReceiveJsMessage0Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, IntPtr browser, IntPtr frame);
        public delegate void OnReceiveJsMessage1Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg, IntPtr browser, IntPtr frame);
        public delegate void OnReceiveJsMessage2Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, IntPtr browser, IntPtr frame);
        public delegate void OnReceiveJsMessage3Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, IntPtr browser, IntPtr frame);
        public delegate void OnReceiveJsMessage4Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, IntPtr browser, IntPtr frame);
        public delegate void OnReceiveJsMessage5Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, IntPtr browser, IntPtr frame);
        public delegate void OnReceiveJsMessage6Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, IntPtr browser, IntPtr frame);
        public delegate void OnReceiveJsMessage7Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg7, IntPtr browser, IntPtr frame);
        public delegate void OnReceiveJsMessage8Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg7, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg8, IntPtr browser, IntPtr frame);
        public delegate void OnReceiveJsMessage9Delegation([MarshalAs(UnmanagedType.LPUTF8Str)] string func, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg2, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg3, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg4, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg5, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg6, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg7, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg8, [MarshalAs(UnmanagedType.LPUTF8Str)] string arg9, IntPtr browser, IntPtr frame);

        public static void OnInit(string cmd_line, string path, int process_type)
        {
            s_MainThreadId = Thread.CurrentThread.ManagedThreadId;

            NativeLogNoLock("[csharp] Init CommandLine: " + cmd_line);
            NativeLogNoLock("[csharp] Init BasePath: " + path);
            s_CmdLine = cmd_line;
            s_BasePath = path;
            s_ProcessType = process_type;
            Console.SetOut(s_StringWriter);
            Console.SetError(s_StringWriter);

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_init"));

                if (null != s_NativeApi) {
                    TryLoadDSL();
                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
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
                    s_NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
                    s_NativeApi.SetLastContext(IntPtr.Zero, IntPtr.Zero);
                    s_NativeApi.LastSourceProcessId = -1;
                    TryLoadDSL();
                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
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
        }
        public static void OnBrowserInit(IntPtr browser)
        {
            s_MainThreadId = Thread.CurrentThread.ManagedThreadId;

            NativeLogNoLock("[csharp] Browser Init");

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_browser_init"));

                if (null != s_NativeApi) {
                    s_NativeApi.SetContext(browser, IntPtr.Zero);
                    TryLoadDSL();
                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
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
            NativeLogNoLock("[csharp] Browser Finalize");

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_browser_finalize"));

                if (null != s_NativeApi) {
                    s_NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
                    s_NativeApi.SetLastContext(IntPtr.Zero, IntPtr.Zero);
                    s_NativeApi.LastSourceProcessId = -1;
                    TryLoadDSL();
                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
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
        }
        public static void OnRendererInit(IntPtr browser, IntPtr frame)
        {
            s_MainThreadId = Thread.CurrentThread.ManagedThreadId;

            NativeLogNoLock("[csharp] Renderer Init");

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_renderer_init"));

                if (null != s_NativeApi) {
                    s_NativeApi.SetContext(browser, frame);
                    TryLoadDSL();
                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
                    BoxedValue r = BatchCommand.BatchScript.Call("on_renderer_init");
                    if (!r.IsNullObject) {
                        NativeLogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
        }
        public static void OnRendererFinalize(IntPtr browser, IntPtr frame)
        {
            NativeLogNoLock("[csharp] Renderer Finalize");

            try {
                NativeLogNoLock(string.Format("[csharp] Call dsl on_renderer_finalize"));

                if (null != s_NativeApi) {
                    s_NativeApi.SetContext(IntPtr.Zero, IntPtr.Zero);
                    s_NativeApi.SetLastContext(IntPtr.Zero, IntPtr.Zero);
                    s_NativeApi.LastSourceProcessId = -1;
                    TryLoadDSL();
                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
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
        }

        public static void OnReceiveCefMessage0(string msg, IntPtr browser, IntPtr frame, int source_process_id)
        {
            OnReceiveCefMessage(msg, new List<string>(), browser, frame, source_process_id);
        }
        public static void OnReceiveCefMessage1(string msg, string arg, IntPtr browser, IntPtr frame, int source_process_id)
        {
            OnReceiveCefMessage(msg, new List<string> { arg }, browser, frame, source_process_id);
        }
        public static void OnReceiveCefMessage2(string msg, string arg1, string arg2, IntPtr browser, IntPtr frame, int source_process_id)
        {
            OnReceiveCefMessage(msg, new List<string> { arg1, arg2 }, browser, frame, source_process_id);
        }
        public static void OnReceiveCefMessage3(string msg, string arg1, string arg2, string arg3, IntPtr browser, IntPtr frame, int source_process_id)
        {
            OnReceiveCefMessage(msg, new List<string> { arg1, arg2, arg3 }, browser, frame, source_process_id);
        }
        public static void OnReceiveCefMessage4(string msg, string arg1, string arg2, string arg3, string arg4, IntPtr browser, IntPtr frame, int source_process_id)
        {
            OnReceiveCefMessage(msg, new List<string> { arg1, arg2, arg3, arg4 }, browser, frame, source_process_id);
        }
        public static void OnReceiveCefMessage5(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, IntPtr browser, IntPtr frame, int source_process_id)
        {
            OnReceiveCefMessage(msg, new List<string> { arg1, arg2, arg3, arg4, arg5 }, browser, frame, source_process_id);
        }
        public static void OnReceiveCefMessage6(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, IntPtr browser, IntPtr frame, int source_process_id)
        {
            OnReceiveCefMessage(msg, new List<string> { arg1, arg2, arg3, arg4, arg5, arg6 }, browser, frame, source_process_id);
        }
        public static void OnReceiveCefMessage7(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, IntPtr browser, IntPtr frame, int source_process_id)
        {
            OnReceiveCefMessage(msg, new List<string> { arg1, arg2, arg3, arg4, arg5, arg6, arg7 }, browser, frame, source_process_id);
        }
        public static void OnReceiveCefMessage8(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, IntPtr browser, IntPtr frame, int source_process_id)
        {
            OnReceiveCefMessage(msg, new List<string> { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 }, browser, frame, source_process_id);
        }
        public static void OnReceiveCefMessage9(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string arg9, IntPtr browser, IntPtr frame, int source_process_id)
        {
            OnReceiveCefMessage(msg, new List<string> { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 }, browser, frame, source_process_id);
        }
        public static void OnReceiveJsMessage0(string msg, IntPtr browser, IntPtr frame)
        {
            OnReceiveJsMessage(msg, new List<string>(), browser, frame);
        }
        public static void OnReceiveJsMessage1(string msg, string arg, IntPtr browser, IntPtr frame)
        {
            OnReceiveJsMessage(msg, new List<string> { arg }, browser, frame);
        }
        public static void OnReceiveJsMessage2(string msg, string arg1, string arg2, IntPtr browser, IntPtr frame)
        {
            OnReceiveJsMessage(msg, new List<string> { arg1, arg2 }, browser, frame);
        }
        public static void OnReceiveJsMessage3(string msg, string arg1, string arg2, string arg3, IntPtr browser, IntPtr frame)
        {
            OnReceiveJsMessage(msg, new List<string> { arg1, arg2, arg3 }, browser, frame);
        }
        public static void OnReceiveJsMessage4(string msg, string arg1, string arg2, string arg3, string arg4, IntPtr browser, IntPtr frame)
        {
            OnReceiveJsMessage(msg, new List<string> { arg1, arg2, arg3, arg4 }, browser, frame);
        }
        public static void OnReceiveJsMessage5(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, IntPtr browser, IntPtr frame)
        {
            OnReceiveJsMessage(msg, new List<string> { arg1, arg2, arg3, arg4, arg5 }, browser, frame);
        }
        public static void OnReceiveJsMessage6(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, IntPtr browser, IntPtr frame)
        {
            OnReceiveJsMessage(msg, new List<string> { arg1, arg2, arg3, arg4, arg5, arg6 }, browser, frame);
        }
        public static void OnReceiveJsMessage7(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, IntPtr browser, IntPtr frame)
        {
            OnReceiveJsMessage(msg, new List<string> { arg1, arg2, arg3, arg4, arg5, arg6, arg7 }, browser, frame);
        }
        public static void OnReceiveJsMessage8(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, IntPtr browser, IntPtr frame)
        {
            OnReceiveJsMessage(msg, new List<string> { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 }, browser, frame);
        }
        public static void OnReceiveJsMessage9(string msg, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string arg9, IntPtr browser, IntPtr frame)
        {
            OnReceiveJsMessage(msg, new List<string> { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 }, browser, frame);
        }

        public static void OnReceiveCefMessage(string msg, List<string> args, IntPtr browser, IntPtr frame, int source_process_id)
        {
            lock (s_Lock) {
                try {
                    NativeLogNoLock(string.Format("[csharp] Call csharp OnReceiveCefMessage, msg:{0} arg:{1} from process:{2} process type:{3}", msg, string.Join(",", args), source_process_id, s_ProcessType));

                    if (null != s_NativeApi) {
                        s_NativeApi.SetLastContext(browser, frame);
                        s_NativeApi.LastSourceProcessId = source_process_id;
                        TryLoadDSL();
                        BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                        BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                        BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
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
                try {
                    NativeLogNoLock(string.Format("[csharp] Call csharp OnReceiveJsMessage, msg:{0} arg:{1} process type:{2}", msg, string.Join(",", args), s_ProcessType));

                    if (null != s_NativeApi) {
                        s_NativeApi.SetLastContext(browser, frame);
                        TryLoadDSL();
                        BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                        BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                        BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                        BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(s_ProcessType));
                        BatchCommand.BatchScript.SetGlobalVariable("sourceprocessid", BoxedValue.From(s_NativeApi.LastSourceProcessId));
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
        public static void NativeLogNoLock(string msg)
        {
            if (null != s_NativeApi) {
                bool isMainThread = Thread.CurrentThread.ManagedThreadId == s_MainThreadId;
                string txt = string.Format("thread:{0} {1}{2}: {3}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name, isMainThread ? "(main)" : string.Empty, msg);
                //Console.WriteLine(txt);
                var lines = txt.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines) {
                    s_NativeApi.NativeLog(line);
                }
            }
        }
        public static void JsLogNoLock(string msg)
        {
            if (null != s_NativeApi) {
                bool isMainThread = Thread.CurrentThread.ManagedThreadId == s_MainThreadId;
                string txt = string.Format("thread:{0} {1}{2}: {3}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name, isMainThread ? "(main)" : string.Empty, msg);
                //Console.WriteLine(txt);
                var lines = txt.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines) {
                    s_NativeApi.JavascriptLog(line);
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
                    BatchCommand.BatchScript.Load(fi.FullName);

                    NativeLogNoLock("[csharp] Load dsl script: " + fi.FullName);
                }
            }
            else {
                NativeLogNoLock("[csharp] Can't find dsl script: " + fi.FullName);
            }
        }
        private static void RegisterBatchScriptApi()
        {
            BatchCommand.BatchScript.Register("nativelog", "nativelog(fmt, ...)", new ExpressionFactoryHelper<NativeLogExp>());
            BatchCommand.BatchScript.Register("javascriptlog", "javascriptlog(fmt, ...)", new ExpressionFactoryHelper<JavascriptLogExp>());
            BatchCommand.BatchScript.Register("writefile", "writefile(path, txt)", new ExpressionFactoryHelper<WriteFileExp>());
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
        private static int s_ProcessType = -1;
        private static string s_DslScriptPath = string.Empty;
        private static DateTime s_DslScriptTime = DateTime.Now;
        private static int s_MainThreadId = 0;
        private static object s_Lock = new object();

        private static List<string> s_EmptyArgs = new List<string>();
        private static StringBuilder s_StringBuilder = new StringBuilder();
        private static StringWriter s_StringWriter = new StringWriter(s_StringBuilder);
        private static NativeApi? s_NativeApi;
    }
}