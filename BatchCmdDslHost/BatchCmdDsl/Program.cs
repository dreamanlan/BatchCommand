using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using BatchCmdDsl;
using ScriptableFramework;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using System.Net.Sockets;

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
    sealed class RedirectToPluginExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                throw new Exception("Expected: redirecttoplugin(dllpath[, init_cmdline]) api");
            }
            string dllPath = operands[0].AsString;
            string initCmdLine = string.Empty;
            if (operands.Count > 1) {
                initCmdLine = operands[1].AsString;
            }
            bool r = Program.RedirectToPlugin(dllPath, initCmdLine);
            return BoxedValue.FromBool(r);
        }
    }
    sealed class SetIntervalExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                throw new Exception("Expected: setinterval(ms) api");
            }
            int ms = operands[0].GetInt();
            Program.SetTickInterval(ms);
            return BoxedValue.FromObject(ms);
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
                    ParseCommandLineOptions();
                    int r = 0;
                    if (null != s_ScriptFile) {
                        r = CallDslFunction("on_init");
                    }
                    if (!string.IsNullOrEmpty(s_PluginPath)) {
                        TryLoadPluginFromCommandLine();
                    }
                    return r;
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
            if (exitCode == 0) {
                int interval = s_IntervalMs;
                if (interval < 0) {
                    interval = 0;
                }
                Thread.Sleep(interval);
            }
        }
        exitCode = Shutdown();
        return exitCode;
    }
    public static int Tick()
    {
        lock (s_Lock) {
            if (s_FatalError) {
                return -1;
            }
            if (null != s_Plugin) {
                try {
                    return s_Plugin.Tick();
                }
                catch (Exception e) {
                    NativeLogNoLock("[csharp] Plugin tick exception:" + e.Message + "\n" + e.StackTrace);
                    return -1;
                }
            }
            if (null != s_ScriptFile) {
                return CallDslFunction("on_tick");
            }
        }
        return 0;
    }
    public static int Shutdown()
    {
        lock (s_Lock) {
            if (null != s_Plugin) {
                try {
                    return s_Plugin.Shutdown();
                }
                catch (Exception e) {
                    NativeLogNoLock("[csharp] Plugin shutdown exception:" + e.Message + "\n" + e.StackTrace);
                    return -1;
                }
            }
            if (null != s_ScriptFile) {
                return CallDslFunction("on_shutdown");
            }
        }
        return 0;
    }

    /// <summary>
    /// Load a plugin dll implementing IBatchCmdPlugin and hand over tick/shutdown to it.
    /// Called by the redirecttoplugin dsl api. Once it succeeds, dsl on_tick/on_shutdown
    /// are no longer called.
    /// </summary>
    public static bool RedirectToPlugin(string dllPath, string initCmdLine)
    {
        try {
            if (string.IsNullOrEmpty(dllPath)) {
                NativeLogNoLock("[csharp] redirecttoplugin: empty dll path");
                return false;
            }
            string path = ResolveInputPath(dllPath);
            string cmdLine = string.IsNullOrEmpty(initCmdLine) ? s_CmdLine : initCmdLine;
            var plugin = PluginService.Instance.LoadPlugin(path, cmdLine, s_BasePath);
            if (null == plugin) {
                NativeLogNoLock("[csharp] redirecttoplugin: failed to load plugin: " + path);
                return false;
            }
            s_Plugin = plugin;
            NativeLogNoLock("[csharp] redirecttoplugin: redirected to plugin: " + path);
            return true;
        }
        catch (Exception e) {
            NativeLogNoLock("[csharp] redirecttoplugin exception:" + e.Message + "\n" + e.StackTrace);
            return false;
        }
    }

    /// <summary>
    /// Set the tick interval in milliseconds. Called by the setinterval dsl api.
    /// </summary>
    public static void SetTickInterval(int ms)
    {
        if (ms < 0) {
            ms = 0;
        }
        s_IntervalMs = ms;
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

    private static void ParseCommandLineOptions()
    {
        s_Options = CommandLineParser.Parse(s_CmdLine);

        string? intervalValue = s_Options.GetSwitchValue("interval");
        if (!string.IsNullOrEmpty(intervalValue)) {
            if (int.TryParse(intervalValue, out int ms) && ms >= 0) {
                s_IntervalMs = ms;
            }
            else {
                NativeLogNoLock("[csharp] Invalid --interval value: " + intervalValue + ", keep " + s_IntervalMs + " ms");
            }
        }

        // Resolve script path: --script switch first, then the first positional arg with .dsl suffix
        string? scriptValue = s_Options.GetSwitchValue("script");
        if (!string.IsNullOrEmpty(scriptValue)) {
            s_ScriptFile = ResolveInputPath(scriptValue);
        }
        else {
            string? arg = TakeFirstArgByExtension(".dsl");
            if (!string.IsNullOrEmpty(arg)) {
                s_ScriptFile = ResolveInputPath(arg);
            }
        }

        // Resolve plugin path: --plugin switch first, then the first positional arg with .dll suffix
        string? pluginValue = s_Options.GetSwitchValue("plugin");
        if (!string.IsNullOrEmpty(pluginValue)) {
            s_PluginPath = ResolveInputPath(pluginValue);
        }
        else {
            string? arg = TakeFirstArgByExtension(".dll");
            if (!string.IsNullOrEmpty(arg)) {
                s_PluginPath = ResolveInputPath(arg);
            }
        }

        // Default to the process monitor script when neither plugin nor script is requested
        if (null == s_ScriptFile && null == s_PluginPath) {
            s_ScriptFile = Path.Combine(s_BasePath, "managed", "monitor.dsl");
        }

        // Expose parsed args and switches to dsl scripts (after mode args are consumed)
        s_CmdArgs = new List<string>(s_Options.Args);
        s_CmdSwitches = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in s_Options.Switches) {
            s_CmdSwitches[pair.Key] = (pair.Value.Count > 0) ? pair.Value[0] : "true";
        }
    }

    private static string? TakeFirstArgByExtension(string ext)
    {
        if (s_Options.Args.Count > 0) {
            string first = s_Options.Args[0];
            if (first.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) {
                s_Options.RemoveFirstArg();
                return first;
            }
        }
        return null;
    }

    private static string ResolveInputPath(string path)
    {
        if (string.IsNullOrEmpty(path) || Path.IsPathRooted(path)) {
            return path;
        }
        // Try the current working directory first, then the host base path
        if (File.Exists(path)) {
            return Path.GetFullPath(path);
        }
        return Path.Combine(s_BasePath, path);
    }

    private static void TryLoadPluginFromCommandLine()
    {
        NativeLogNoLock("[csharp] Loading plugin from command line: " + s_PluginPath);
        var plugin = PluginService.Instance.LoadPlugin(s_PluginPath!, s_CmdLine, s_BasePath);
        if (null != plugin) {
            s_Plugin = plugin;
        }
        else {
            s_FatalError = true;
            NativeLogNoLock("[csharp] Failed to load plugin, exit with error");
        }
    }

    private static int CallDslFunction(string functionName)
    {
        try {
            TryLoadDSL();
            BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
            BatchCommand.BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(s_CmdLine));
            BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
            BatchCommand.BatchScript.SetGlobalVariable("cmdargs", BoxedValue.FromObject(s_CmdArgs));
            BatchCommand.BatchScript.SetGlobalVariable("cmdswitches", BoxedValue.FromObject(s_CmdSwitches));
            var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
            var r = BatchCommand.BatchScript.Call(functionName, vargs);
            BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
            CheckDslError();
            if (!r.IsNullObject) {
                return r.GetInt();
            }
        }
        catch (Exception e) {
            NativeLogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
        }
        return 0;
    }

    private static void TryLoadDSL()
    {
        if (null == s_ScriptFile) {
            return;
        }
        BatchCommand.BatchScript.ClearDslErrors();
        PrepareBatchScript();
        string path = s_ScriptFile;
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
        BatchCommand.BatchScript.Register("setinterval", "setinterval(ms) api, set tick interval in milliseconds", new ExpressionFactoryHelper<DotNetLib.SetIntervalExp>());
        BatchCommand.BatchScript.Register("redirecttoplugin", "redirecttoplugin(dllpath[, init_cmdline]) api, load a plugin dll implementing IBatchCmdPlugin and hand over tick/shutdown to it", new ExpressionFactoryHelper<DotNetLib.RedirectToPluginExp>());
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
            BatchCommand.BatchScript.ClearDslErrors();
        }
    }

    [ThreadStatic]
    private static bool s_BatchScriptInited = false;
    [ThreadStatic]
    private static string? s_DslScriptPath;
    [ThreadStatic]
    private static DateTime s_DslScriptTime;

    private static string s_BasePath = string.Empty;
    private static int s_MainThreadId = 0;
    private static string s_CmdLine = string.Empty;
    private static object s_Lock = new object();

    private static List<string> s_EmptyArgs = new List<string>();
    private static StringBuilder s_StringBuilder = new StringBuilder();
    private static StringWriter s_StringWriter = new StringWriter(s_StringBuilder);
    private static DotNetLib.NativeApi? s_NativeApi;

    private const int DefaultTickIntervalMs = 1000;
    private static CommandLineOptions s_Options = new CommandLineOptions();
    private static List<string> s_CmdArgs = new List<string>();
    private static Dictionary<string, string> s_CmdSwitches = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private static string? s_ScriptFile = null;
    private static string? s_PluginPath = null;
    private static IBatchCmdPlugin? s_Plugin = null;
    private static bool s_FatalError = false;
    private static int s_IntervalMs = DefaultTickIntervalMs;
}
