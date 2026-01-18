using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using ScriptableFramework;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;

[StructLayout(LayoutKind.Sequential)]
public struct HostApi
{
    public IntPtr NativeLog;
    public IntPtr TerminateProcess;
    public IntPtr CountProcess;
}

// delegate for native host_test_fn
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostNativeLogDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string c);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostTerminateProcessDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string key);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostCountProcessDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string key);

namespace DotNetLib
{
    public class NativeApi
    {
        public NativeApi(IntPtr apis)
        {
            HostApi hostApi = Marshal.PtrToStructure<HostApi>(apis);
            m_HostNativeLogApi = Marshal.GetDelegateForFunctionPointer<HostNativeLogDelegation>(hostApi.NativeLog);
            m_HostTerminateProcessApi = Marshal.GetDelegateForFunctionPointer<HostTerminateProcessDelegation>(hostApi.TerminateProcess);
            m_HostCountProcessApi = Marshal.GetDelegateForFunctionPointer<HostCountProcessDelegation>(hostApi.CountProcess);
        }

        public void OutputLog(string message)
        {
            if (null != m_HostNativeLogApi) {
                m_HostNativeLogApi(message);
            }
        }
        public int TerminateProcess(string key)
        {
            if (null != m_HostTerminateProcessApi) {
                return m_HostTerminateProcessApi(key);
            }
            return 0;
        }
        public int CountProcess(string key)
        {
            if (null != m_HostCountProcessApi) {
                return m_HostCountProcessApi(key);
            }
            return 0;
        }

        private HostNativeLogDelegation m_HostNativeLogApi;
        private HostTerminateProcessDelegation m_HostTerminateProcessApi;
        private HostCountProcessDelegation m_HostCountProcessApi;
    }
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
            Program.NativeLogNoLock(str);
            return str;
        }
    }
}
public static class Program
{
    public static void Main()
    {
        //When the host program runs CoreCLR, we don't execute the `Main` method because we can't register APIs before it.
        //We use several custom entry points instead. However, for the compiler (for console application types), we must
        //retain the `Main` method.
    }
    [UnmanagedCallersOnly]
    public static int RegisterApi(IntPtr apis)
    {
        s_NativeApi = new DotNetLib.NativeApi(apis);
        return 0;
    }
    public delegate int InitDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string cmd_line, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    public static int Init(string cmdLine, string basePath)
    {
        s_CmdLine = cmdLine;
        s_BasePath = basePath;

        lock (s_Lock) {
            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();
                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    var r = BatchCommand.BatchScript.Call("on_init", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    if (!r.IsNullObject) {
                        return r.GetInt();
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
        }
        return 0;
    }
    public delegate int LoopDelegation();
    public static int Loop()
    {
        int exitCode = 0;
        while (exitCode == 0) {
            exitCode = Tick();
            Thread.Sleep(1000);
        }
        return exitCode;
    }
    public static int Tick()
    {
        lock (s_Lock) {
            try {
                if (null != s_NativeApi) {
                    TryLoadDSL();
                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    var r = BatchCommand.BatchScript.Call("on_tick", vargs);
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    if (!r.IsNullObject) {
                        return r.GetInt();
                    }
                }
            }
            catch (Exception e) {
                NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
        }
        return 0;
    }

    public static void NativeLogNoLock(string msg)
    {
        if (null != s_NativeApi) {
            bool isMainThread = Thread.CurrentThread.ManagedThreadId == Program.s_MainThreadId;
            string txt = string.Format("thread:{0} {1}{2}: {3}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name, isMainThread ? "(main)" : string.Empty, msg);
            //Console.WriteLine(txt);
            var lines = txt.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) {
                s_NativeApi.OutputLog(line);
            }
        }
    }

    public static string BasePath
    {
        get {
            return s_BasePath;
        }
    }
    private static void TryLoadDSL()
    {
        PrepareBatchScript();
        string path = Path.Combine(s_BasePath, "./managed/Monitor.dsl");
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

    private static void RegisterBatchScriptApi()
    {
        BatchCommand.BatchScript.Register("nativelog", "nativelog(fmt, ...)", new ExpressionFactoryHelper<DotNetLib.NativeLogExp>());
    }

    private static void PrepareBatchScript()
    {
        if (!s_BatchScriptInited) {
            BatchCommand.BatchScript.Init();
            RegisterBatchScriptApi();
            s_BatchScriptInited = true;
        }
    }

    private static string s_BasePath = string.Empty;
    private static int s_MainThreadId = 0;
    private static bool s_BatchScriptInited = false;
    private static string s_CmdLine = string.Empty;
    private static int s_ProcessType = -1;
    private static string s_DslScriptPath = string.Empty;
    private static DateTime s_DslScriptTime = DateTime.Now;
    private static object s_Lock = new object();

    private static List<string> s_EmptyArgs = new List<string>();
    private static StringBuilder s_StringBuilder = new StringBuilder();
    private static StringWriter s_StringWriter = new StringWriter(s_StringBuilder);
    private static DotNetLib.NativeApi? s_NativeApi;
}