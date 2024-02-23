using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Wox.Plugin;
using Wox.Infrastructure;
using Wox.Infrastructure.Storage;
using BatchCommand;
using DslExpression;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Globalization;
using System.Windows.Documents;
using System.Runtime.Remoting.Contexts;
using Wox.Infrastructure.UserSettings;
using System.Runtime;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

/// <remarks>
/// 目前插件只用于1.3.524版本的Wox，更新的版本因为插件初始化不在主线程，MessageWindow的功能会不正常
/// 注意:Result的Title与SubTitle相同时Wox认为是相同的结果（亦即，不会更新Action等其他字段！）
/// </remarks>
public sealed class Main : IPlugin, IContextMenu, IReloadable, IPluginI18n, ISavable
{
    public Main()
    {
        //注：插件构造不是在主线程执行的

        m_Storage = new PluginJsonStorage<PluginSettings>();
        m_Settings = m_Storage.Load();

        s_ShellCtxMenu.FolderUseDefaultOnly = m_Settings.FolderUseDefaultOnly;
        s_ShellCtxMenu.FileUseDefaultOnly = m_Settings.FileUseDefaultOnly;
    }
    public void Init(PluginInitContext context)
    {
        s_StartupThread = Thread.CurrentThread;
        s_StartupThreadId = s_StartupThread.ManagedThreadId;
        s_Context = context;
        var p = Process.GetCurrentProcess();
        string exe = p.MainModule.FileName;
        string dir = Path.GetDirectoryName(exe);
        Directory.SetCurrentDirectory(dir);
        s_ErrWriter = new StringWriterWithLock(s_ErrorBuilder, s_LogLock);
        Console.SetOut(s_ErrWriter);
        Console.SetError(s_ErrWriter);
        using (var sw = new StreamWriter(s_LogFile, false)) {
            sw.WriteLine("dir:{0} exe:{1}", dir, exe);
            sw.WriteLine("Startup Thread {0}.", s_StartupThreadId);
            sw.Close();
        }
        s_MessageWindow = new MessageWindow();
        s_MessageWindow.Hide();
        StartScriptThread();
    }
    public List<Result> Query(Query query)
    {
        const int c_MaxQueueNum = 64;
        Debug.Assert(!IsScriptThread());
        LogLine("query key:{0} first:{1} left:{2} from thread {3}", query.ActionKeyword, query.FirstSearch, query.SecondToEndSearch, Thread.CurrentThread.ManagedThreadId);
        if (EveryThingSDK.EverythingExists()) {
            TryFindEverything();

            if (query.ActionKeyword == "dsl" && query.FirstSearch == "restart") {
                //重启功能在c#端完成，防止某些功能导致dsl脚本线程卡死后无法重启
                var item = new Result { Title = "restart", SubTitle = "restart Wox", IcoPath = c_IcoPath, ContextData = query };
                item.Action = e => { return Main.OnAction("on_action_restart", query, item, e); };
                s_Results.Clear();
                s_Results.Add(item);
            }
            else if (s_ScriptQueryQueue.Count < c_MaxQueueNum) {
                var evt = GetThreadEvent();
                evt.Reset();
                QueueScriptQuery(evt, () => {
                    //执行新查询时才清空列表，这时候应该没有线程还在使用了，也可以每次都构建一个新列表，不过还是减少一些GC吧
                    s_NewResults.Clear();
                    BatchScript.Call("on_query", CalculatorValue.FromObject(query));
                    FlushLog("Query");
                    //swap
                    var t = s_Results;
                    s_Results = s_NewResults;
                    s_NewResults = t;
                });
                //wait
                Wait(evt);
            }
        }
        else {
            LogLine("query [{0}] can't find everthing, run everything and restart Wox from thread {1}", query.ToString(), Thread.CurrentThread.ManagedThreadId);
            if (string.IsNullOrEmpty(s_EverythingFullPath)) {
                LogLine("can't find everything install location!");
            }
            else {
                Process.Start(s_EverythingFullPath);
            }
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                s_Context.API.RestarApp();
            });
        }
        return s_Results;
    }
    public List<Result> LoadContextMenus(Result selectedResult)
    {
        Debug.Assert(!IsScriptThread());
        Query query = selectedResult.ContextData as Query;
        LogLine("load context menu for [{0}|{1}], query [{2}], count {3} from thread {4}", selectedResult.Title, selectedResult.SubTitle, query.ToString(), s_ContextMenus.Count, Thread.CurrentThread.ManagedThreadId);
        //这个操作串行执行
        var evt = GetThreadEvent();
        var funcs = GetThreadFuncs();
        evt.Reset();
        QueueScriptAction(evt, funcs, () => {
            //执行新查询时才清空列表，这时候应该没有线程还在使用了，也可以每次都构建一个新列表，不过还是减少一些GC吧
            s_NewContextMenus.Clear();
            BatchScript.Call("on_context_menus", CalculatorValue.FromObject(query), CalculatorValue.FromObject(selectedResult));
            FlushLog("LoadContextMenus");
            //swap
            var t = s_ContextMenus;
            s_ContextMenus = s_NewContextMenus;
            s_NewContextMenus = t;
        });
        //wait
        WaitAndGetResult(evt, funcs);
        return s_ContextMenus;
    }
    internal static bool OnMenuAction(string action, Query query, Result result, Result menu, ActionContext e)
    {
        Debug.Assert(!IsScriptThread());
        LogLine("menu action for [{0}|{1}], query [{2}], result [{3}|{4}] from thread {5}", menu.Title, menu.SubTitle, query.ToString(), result.Title, result.SubTitle, Thread.CurrentThread.ManagedThreadId);
        //由于Wox实现上的缺陷，这里根据menu的Title与SubTitle在菜单列表里查找真正的菜单项再调action
        int ix = s_ContextMenus.IndexOf(menu);
        if (ix < 0) {
            s_Context.API.ShowMsg("Can't find menu " + menu.Title, menu.SubTitle, menu.IcoPath);
            return false;
        }
        if (s_ContextMenus[ix].ContextData != menu.ContextData || s_ContextMenus[ix].Action != menu.Action) {
            return s_ContextMenus[ix].Action(e);
        }
        //这里开始串行执行
        var evt = GetThreadEvent();
        var funcs = GetThreadFuncs();
        evt.Reset();
        QueueScriptAction(evt, funcs, () => {
            var r = BatchScript.Call(action, CalculatorValue.FromObject(query), CalculatorValue.FromObject(result), CalculatorValue.FromObject(menu), CalculatorValue.FromObject(e));
            Log("menu action for [{0}|{1}] return {2} from thread {3}", menu.Title, menu.SubTitle, r.IsNullObject ? false : r.GetBool(), Thread.CurrentThread.ManagedThreadId);
            FlushLog("OnMenuAction");
            if (s_NeedReload) {
                s_NeedReload = false;
                ReloadDsl();
            }
            if (!r.IsNullObject) {
                bool ret = r.GetBool();
                funcs.Enqueue(() => ret);
            }
            else {
                funcs.Enqueue(() => false);
            }
        });
        //wait
        return WaitAndGetResult(evt, funcs);
    }
    internal static bool OnAction(string action, Query query, Result result, ActionContext e)
    {
        Debug.Assert(!IsScriptThread());
        LogLine("action for [{0}|{1}], query [{2}] from thread {3}", result.Title, result.SubTitle, query.ToString(), Thread.CurrentThread.ManagedThreadId);
        //由于Wox实现上的缺陷，这里根据result的Title与SubTitle在结果列表里查找真正的result项再调action
        int ix = s_Results.IndexOf(result);
        if (ix < 0) {
            s_Context.API.ShowMsg("Can't find result " + result.Title, result.SubTitle, result.IcoPath);
            return false;
        }
        if (action == "on_action_restart") {
            //这里应该已经是主线程，还是dispatch一次，下一帧执行
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                s_Context.API.RestarApp();
            });
            return true;
        }
        if (s_Results[ix].ContextData != result.ContextData || s_Results[ix].Action != result.Action) {
            return s_Results[ix].Action(e);
        }
        //这里开始串行执行
        var evt = GetThreadEvent();
        var funcs = GetThreadFuncs();
        evt.Reset();
        QueueScriptAction(evt, funcs, () => {
            var r = BatchScript.Call(action, CalculatorValue.FromObject(query), CalculatorValue.FromObject(result), CalculatorValue.FromObject(e));
            Log("action for [{0}|{1}] return {2} from thread {3}", result.Title, result.SubTitle, r.IsNullObject ? false : r.GetBool(), Thread.CurrentThread.ManagedThreadId);
            FlushLog("OnAction");
            if (s_NeedReload) {
                s_NeedReload = false;
                ReloadDsl();
            }
            if (!r.IsNullObject) {
                bool ret = r.GetBool();
                funcs.Enqueue(() => ret);
            }
            else {
                funcs.Enqueue(() => false);
            }
        });
        //wait
        return WaitAndGetResult(evt, funcs);
    }

    public void ReloadData()
    {
        //nothing to do
    }

    public string GetTranslatedPluginTitle()
    {
        //return s_Context.API.GetTranslation("wox_plugin_batchcommand_plugin_name");
        return "Dsl BatchCommand Plugin";
    }

    public string GetTranslatedPluginDescription()
    {
        //return s_Context.API.GetTranslation("wox_plugin_batchcommand_plugin_description");
        return "use MetaDSL extend Wox";
    }

    public void Save()
    {
        m_Settings.FolderUseDefaultOnly = s_ShellCtxMenu.FolderUseDefaultOnly;
        m_Settings.FileUseDefaultOnly = s_ShellCtxMenu.FileUseDefaultOnly;
        m_Storage.Save();
    }

    private class PluginSettings
    {
        public bool FolderUseDefaultOnly { get; set; } = true;
        public bool FileUseDefaultOnly { get; set; } = false;
    }

    private readonly PluginSettings m_Settings;
    private readonly PluginJsonStorage<PluginSettings> m_Storage;

    internal static void ReloadDsl()
    {
        string dslPath = Path.Combine(s_Context.CurrentPluginMetadata.PluginDirectory, "main.dsl");
        var vargs = BatchScript.NewCalculatorValueList();
        vargs.Add(s_Context.CurrentPluginMetadata.ID);
        vargs.Add(CalculatorValue.FromObject(s_Context.CurrentPluginMetadata));
        vargs.Add(CalculatorValue.FromObject(s_Context));
        BatchScript.Run(dslPath, vargs);
        BatchScript.RecycleCalculatorValueList(vargs);
        Log("Reload Dsl from thread {0}", Thread.CurrentThread.ManagedThreadId);
        FlushLog("ReloadDsl");
    }
    internal static void FlushLog(string tag)
    {
        string log, err;
        lock (s_LogLock) {
            log = s_LogBuilder.ToString();
            s_LogBuilder.Length = 0;
            err = s_ErrorBuilder.ToString();
            s_ErrorBuilder.Length = 0;
        }
        if (!string.IsNullOrEmpty(err)) {
            if (null != s_StandardOutput) {
                s_StandardOutput.Write(err);
            }
        }
        bool logIsEmpty = string.IsNullOrEmpty(log);
        bool errIsEmpty = string.IsNullOrEmpty(err);
        if (!logIsEmpty || !errIsEmpty) {
            using (var sw = File.AppendText(s_LogFile)) {
                if (!logIsEmpty)
                    sw.WriteLine(log.TrimEnd());
                if (!errIsEmpty)
                    sw.WriteLine(err.TrimEnd());
                sw.Close();
            }
        }
    }
    internal static void Log(string fmt, params object[] args)
    {
        lock (s_LogLock) {
            if (args.Length == 0)
                s_LogBuilder.Append(fmt);
            else
                s_LogBuilder.AppendFormat(fmt, args);
        }
    }
    internal static void LogLine(string fmt, params object[] args)
    {
        lock (s_LogLock) {
            if (args.Length == 0)
                s_LogBuilder.Append(fmt);
            else
                s_LogBuilder.AppendFormat(fmt, args);
            s_LogBuilder.AppendLine();
        }
    }
    internal static bool IsScriptThread()
    {
        return s_ScriptThreadId == Thread.CurrentThread.ManagedThreadId;
    }
    internal static void TryFindEverything()
    {
        if (string.IsNullOrEmpty(s_EverythingFullPath)) {
            const int c_Capacity = 4096;
            const string c_Key = "Everything.exe";
            var dir = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\voidtools\\Everything", "InstallLocation", string.Empty) as string;
            if (!string.IsNullOrEmpty(dir)) {
                var fullPath = Path.Combine(dir, c_Key);
                if (File.Exists(fullPath)) {
                    s_EverythingFullPath = fullPath;
                }
            }
            if (string.IsNullOrEmpty(s_EverythingFullPath)) {
                if (EveryThingSDK.EverythingExists()) {
                    EveryThingSDK.Everything_SetMatchPath(false);
                    EveryThingSDK.Everything_SetMatchCase(false);
                    EveryThingSDK.Everything_SetMatchWholeWord(true);
                    EveryThingSDK.Everything_SetRegex(false);
                    EveryThingSDK.Everything_SetSort(EveryThingSDK.EVERYTHING_SORT_PATH_ASCENDING); 
                    
                    EveryThingSDK.Everything_SetSearchW(c_Key);
                    EveryThingSDK.Everything_SetOffset(0);
                    EveryThingSDK.Everything_SetMax(100);
                    EveryThingSDK.Everything_SetRequestFlags(EveryThingSDK.EVERYTHING_REQUEST_FILE_NAME | EveryThingSDK.EVERYTHING_REQUEST_PATH);
                    if (EveryThingSDK.Everything_QueryW(true)) {
                        uint num = EveryThingSDK.Everything_GetNumResults();
                        for (uint i = 0; i < num; ++i) {
                            var sb = new StringBuilder(c_Capacity);
                            string fileName = Marshal.PtrToStringUni(EveryThingSDK.Everything_GetResultFileNameW(i));
                            EveryThingSDK.Everything_GetResultFullPathNameW(i, sb, c_Capacity);
                            if(string.Compare(fileName, c_Key, true) == 0) {
                                var fullPath = sb.ToString();
                                if(File.Exists(fullPath)) {
                                    s_EverythingFullPath = fullPath;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    internal static void ShowConsole()
    {
        var handle = GetConsoleWindow();

        if (handle == IntPtr.Zero) {
            if (!AttachConsole(ATTACH_PARENT_PROCESS)) {
                AllocConsole();
            }
            IntPtr stdHandle = GetStdHandle(StandardHandle.Output);
            IntPtr errHandle = GetStdHandle(StandardHandle.Error);

            if (IsRedirected(stdHandle)) {
                Console.WriteLine("[ShowConsole] standard output is redirected.");
            }
            bool errorRedirected = IsRedirected(errHandle);
            if (!errorRedirected) {
                SetStdHandle(StandardHandle.Error, stdHandle);
            }
            SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
            FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            Encoding encoding = Encoding.Default;
            s_StandardOutput = new StreamWriter(fileStream, encoding);
            s_StandardOutput.AutoFlush = true;
            s_StandardOutput.Flush();
            Console.Clear();
        }
        else if(!IsWindowVisible(handle)) {
            ShowWindow(handle, SW_SHOW);
        }
    }
    internal static void HideConsole()
    {
        var handle = GetConsoleWindow();
        if (IsWindowVisible(handle)) {
            ShowWindow(handle, SW_HIDE);
        }
    }
    private static bool IsRedirected(IntPtr handle)
    {
        FileType fileType = GetFileType(handle);
        return (fileType == FileType.Disk) || (fileType == FileType.Pipe);
    }

    private static void StartScriptThread()
    {
        TryFindEverything();

        s_ScriptThread = new Thread(ScriptThread);
        s_ScriptThread.Name = "Script Thread";
        s_ScriptThread.Start();
    }
    private static void ScriptThread()
    {
        InitScript();

        while (s_StartupThread.IsAlive) {
            ProcessOnce(c_ActionNumPerTick, c_QueryNumPerTick);
            Thread.Sleep(10);
        }
        ProcessOnce(int.MaxValue, int.MaxValue);
        LogLine("Script Thread {0} Terminated.", s_ScriptThreadId);
        FlushLog("ScriptThread");
        FreeConsole();
    }
    private static void InitScript()
    {
        s_ScriptThreadId = Thread.CurrentThread.ManagedThreadId;
        Log("Script Thread {0}", s_ScriptThreadId);
        FlushLog("InitScript");

        BatchScript.Init();
        BatchScript.Register("contextmenu", "contextment api", new ExpressionFactoryHelper<ShellContextMenuExp>());
        BatchScript.Register("context", "context api", new ExpressionFactoryHelper<ContextExp>());
        BatchScript.Register("api", "api api", new ExpressionFactoryHelper<ApiExp>());
        BatchScript.Register("metadata", "metadata api", new ExpressionFactoryHelper<MetadataExp>());
        BatchScript.Register("showmsg", "showmsg api", new ExpressionFactoryHelper<ShowMsgExp>());
        BatchScript.Register("restart", "restart api", new ExpressionFactoryHelper<RestartExp>());
        BatchScript.Register("show", "show api", new ExpressionFactoryHelper<ShowConsoleExp>());
        BatchScript.Register("hide", "hide api", new ExpressionFactoryHelper<HideConsoleExp>());
        BatchScript.Register("reloaddsl", "reloaddsl api", new ExpressionFactoryHelper<ReloadDslExp>());
        BatchScript.Register("evaldsl", "evaldsl api", new ExpressionFactoryHelper<EvalDslExp>());
        BatchScript.Register("changequery", "changequery api", new ExpressionFactoryHelper<ChangeQueryExp>());
        BatchScript.Register("addresult", "addresult api", new ExpressionFactoryHelper<AddResultExp>());
        BatchScript.Register("addcontextmenu", "addcontextmenu api", new ExpressionFactoryHelper<AddContextMenuExp>());
        BatchScript.Register("keywordregistered", "keywordregistered api", new ExpressionFactoryHelper<ActionKeywordRegisteredExp>());
        BatchScript.Register("clearkeywords", "clearkeywords api", new ExpressionFactoryHelper<ClearActionKeywordsExp>());
        BatchScript.Register("addkeyword", "addkeyword api", new ExpressionFactoryHelper<AddActionKeywordExp>());
        BatchScript.Register("showcontextmenu", "showcontextmenu api", new ExpressionFactoryHelper<ShowContextMenuExp>());
        BatchScript.Register("tryfindeverything", "tryfindeverything api", new ExpressionFactoryHelper<TryFindEverythingExp>());
        BatchScript.Register("everythingexists", "everythingexists api", new ExpressionFactoryHelper<EverythingExistsExp>());
        BatchScript.Register("everythingreset", "everythingreset api", new ExpressionFactoryHelper<EverythingResetExp>());
        BatchScript.Register("everythingsetdefault", "everythingsetdefault api", new ExpressionFactoryHelper<EverythingSetDefaultExp>());
        BatchScript.Register("everythingmatchpath", "everythingmatchpath api", new ExpressionFactoryHelper<EverythingMatchPathExp>());
        BatchScript.Register("everythingmatchcase", "everythingmatchcase api", new ExpressionFactoryHelper<EverythingMatchCaseExp>());
        BatchScript.Register("everythingmatchwholeword", "everythingmatchwholeword api", new ExpressionFactoryHelper<EverythingMatchWholeWordExp>());
        BatchScript.Register("everythingregex", "everythingregex api", new ExpressionFactoryHelper<EverythingRegexExp>());
        BatchScript.Register("everythingsort", "everythingsort api", new ExpressionFactoryHelper<EverythingSortExp>());
        BatchScript.Register("everythingsearch", "everythingsearch api", new ExpressionFactoryHelper<EverythingSearchExp>());

        ReloadDsl();
    }
    private static void ProcessOnce(int maxActionNum, int maxQueryNum)
    {
        for (int ct = 0; ct < maxActionNum && s_ScriptActionQueue.Count > 0; ++ct) {
            if (s_ScriptActionQueue.TryDequeue(out var action)) {
                try {
                    s_CurFuncs = action.Funcs;
                    action.Action();
                    s_CurFuncs = null;
                }
                catch (Exception ex) {
                    Console.WriteLine("exception:{0}", ex.Message);
                }
                finally {
                    //notify
                    action.Event.Set();
                }
            }
        }
        for (int ct = 0; ct < maxQueryNum && s_ScriptQueryQueue.Count > 0; ++ct) {
            if (s_ScriptQueryQueue.TryDequeue(out var action)) {
                try {
                    //只执行最后队列里的最后一个查询（相当于连续输入的中间查询不执行）
                    if (s_ScriptQueryQueue.Count == 0) {
                        action.Action();
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("exception:{0}", ex.Message);
                }
                finally {
                    //notify
                    action.Event.Set();
                }
            }
        }
    }
    private static void QueueScriptQuery(AutoResetEvent evt, Action action)
    {
        s_ScriptQueryQueue.Enqueue(new ScriptQueryInfo { Event = evt, Action = action });
    }
    private static void QueueScriptAction(AutoResetEvent evt, ConcurrentQueue<Func<bool>> funcs, Action action)
    {
        s_ScriptActionQueue.Enqueue(new ScriptActionInfo { Event = evt, Action = action, Funcs = funcs });
    }
    private static void Wait(AutoResetEvent evt)
    {
        evt.WaitOne(c_WaitTimeout);
    }
    private static bool WaitAndGetResult(AutoResetEvent evt, ConcurrentQueue<Func<bool>> funcs)
    {
        evt.WaitOne(c_WaitTimeout);
        bool rt = false;
        while (funcs.Count > 0) {
            if (funcs.TryDequeue(out var f)) {
                rt = f();
            }
        }
        return rt;
    }

    private class ScriptQueryInfo
    {
        internal AutoResetEvent Event;
        internal Action Action;
    }
    private class ScriptActionInfo
    {
        internal AutoResetEvent Event;
        internal Action Action;
        internal ConcurrentQueue<Func<bool>> Funcs;
    }

    internal static ShellApi.ShellContextMenu s_ShellCtxMenu = new ShellApi.ShellContextMenu();
    internal static MessageWindow s_MessageWindow = null;
    internal static PluginInitContext s_Context = null;
    internal static List<Result> s_Results = new List<Result>();
    internal static List<Result> s_NewResults = new List<Result>();
    internal static List<Result> s_ContextMenus = new List<Result>();
    internal static List<Result> s_NewContextMenus = new List<Result>();
    internal static string s_EverythingFullPath = string.Empty;
    internal static bool s_NeedReload = false;
    internal static ConcurrentQueue<Func<bool>> s_CurFuncs = null;
    internal const string c_IcoPath = "Images\\dsl.png";

    private static int s_StartupThreadId = 0;
    private static Thread s_StartupThread = null;
    private static int s_ScriptThreadId = 0;
    private static Thread s_ScriptThread = null;
    private static string s_LogFile = "BatchCommand.log";
    private static StringWriterWithLock s_ErrWriter = null;
    private static StreamWriter s_StandardOutput = null;
    private static StringBuilder s_ErrorBuilder = new StringBuilder();
    private static StringBuilder s_LogBuilder = new StringBuilder();
    private static object s_LogLock = new object();

    private static ConcurrentQueue<ScriptQueryInfo> s_ScriptQueryQueue = new ConcurrentQueue<ScriptQueryInfo>();
    private static ConcurrentQueue<ScriptActionInfo> s_ScriptActionQueue = new ConcurrentQueue<ScriptActionInfo>();

    private static AutoResetEvent GetThreadEvent()
    {
        if (null == s_Event)
            s_Event = new AutoResetEvent(false);
        return s_Event;
    }
    private static ConcurrentQueue<Func<bool>> GetThreadFuncs()
    {
        if (null == s_Funcs)
            s_Funcs = new ConcurrentQueue<Func<bool>>();
        return s_Funcs;
    }
    [ThreadStatic]
    private static AutoResetEvent s_Event = null;
    [ThreadStatic]
    private static ConcurrentQueue<Func<bool>> s_Funcs = null;

    private const int c_QueryNumPerTick = 2;
    private const int c_ActionNumPerTick = 16;
    private const int c_WaitTimeout = 10000;

    private const int ATTACH_PARENT_PROCESS = -1;
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    private enum StandardHandle : uint
    {
        Input = unchecked((uint)-10),
        Output = unchecked((uint)-11),
        Error = unchecked((uint)-12)
    }
    private enum FileType : uint
    {
        Unknown = 0x0000,
        Disk = 0x0001,
        Char = 0x0002,
        Pipe = 0x0003
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int dwProcessId);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeConsole();
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(StandardHandle nStdHandle);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetStdHandle(StandardHandle nStdHandle, IntPtr handle);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern FileType GetFileType(IntPtr handle);
}

public sealed class StringWriterWithLock : StringWriter
{
    public StringWriterWithLock(StringBuilder sb, object lockObj):base(sb)
    {
        m_LockObj = lockObj;
    }

    public override void Write(char value)
    {
        lock (m_LockObj) {
            base.Write(value);
        }
    }

    public override void Write(char[] buffer, int index, int count)
    {
        lock (m_LockObj) {
            base.Write(buffer, index, count);
        }
    }

    public override void Write(string value)
    {
        lock (m_LockObj) {
            base.Write(value);
        }
    }

    private object m_LockObj;
}

///<remarks>
///Form好像必须在主线程初始化才会起作用，目前wox调插件Init方法时是在主线程的，但1.4版本之后就不是了
///</remarks>
public sealed class MessageWindow : Form
{
    public MessageWindow()
    {
        m_EverythingQueryEvent = new AutoResetEvent(false);
        var accessHandle = this.Handle;
    }

    public void Query()
    {
        m_EverythingQueryEvent.Reset();
        m_CheckReply = true;
        EveryThingSDK.Everything_SetReplyWindow(m_Handle);
        EveryThingSDK.Everything_SetReplyID(MY_REPLY_ID);
        EveryThingSDK.Everything_QueryW(false);
        m_EverythingQueryEvent.WaitOne(WAIT_TIME_OUT);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ChangeToMessageOnlyWindow();
    }

    private void ChangeToMessageOnlyWindow()
    {
        IntPtr HWND_MESSAGE = new IntPtr(-3);
        SetParent(this.Handle, HWND_MESSAGE);
        m_Handle = this.Handle;
    }

    protected override void WndProc(ref Message m)
    {
        if (m_CheckReply && EveryThingSDK.Everything_IsQueryReply(m.Msg, m.WParam, m.LParam, MY_REPLY_ID)) {
            if (null != m_EverythingQueryEvent) {
                m_EverythingQueryEvent.Set();
            }
        }
        base.WndProc(ref m);
    }

    private AutoResetEvent m_EverythingQueryEvent = null;
    private bool m_CheckReply = false;
    private IntPtr m_Handle = IntPtr.Zero;

    [DllImport("user32.dll")]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    private const int MY_REPLY_ID = 0;
    private const int WAIT_TIME_OUT = 10000;
}

internal sealed class ShellContextMenuExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        return CalculatorValue.FromObject(Main.s_ShellCtxMenu);
    }
}
internal sealed class ContextExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        return CalculatorValue.FromObject(Main.s_Context);
    }
}
internal sealed class ApiExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        return CalculatorValue.FromObject(Main.s_Context.API);
    }
}
internal sealed class MetadataExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        return CalculatorValue.FromObject(Main.s_Context.CurrentPluginMetadata);
    }
}
internal sealed class ShowMsgExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count > 0) {
            string title = operands[0].ToString();
            string subtitle = operands.Count > 1 ? operands[1].ToString() : string.Empty;
            string icon = operands.Count > 2 ? operands[2].ToString() : Main.c_IcoPath;

            if (null != Main.s_CurFuncs) {
                //转到主线程执行（点击列表触发的请求好像都是从主线程发起）
                Main.s_CurFuncs.Enqueue(() => {
                    Main.s_Context.API.ShowMsg(title, subtitle, icon);
                    return false;
                });
            }
            else {
                Main.s_Context.API.ShowMsg(title, subtitle, icon);
            }
        }
        return CalculatorValue.NullObject;
    }
}
internal sealed class RestartExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (null != Main.s_CurFuncs) {
            //RestarApp方法需要在主线程执行（点击列表触发的请求好像都是从主线程发起）
            Main.s_CurFuncs.Enqueue(() => {
                Main.s_Context.API.RestarApp();
                return false;
            });
        }
        return CalculatorValue.NullObject;
    }
}
internal sealed class ShowConsoleExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        CalculatorValue r = CalculatorValue.NullObject;
        Main.ShowConsole();
        return r;
    }
}
internal sealed class HideConsoleExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        CalculatorValue r = CalculatorValue.NullObject;
        Main.HideConsole();
        return r;
    }
}
internal sealed class ReloadDslExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        Main.s_NeedReload = true;
        return CalculatorValue.NullObject;
    }
}
internal sealed class EvalDslExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        CalculatorValue r = CalculatorValue.NullObject;
        if (operands.Count >= 1) {
            string code = operands[0].As<string>();
            var args = BatchScript.NewCalculatorValueList();
            for (int i = 1; i < operands.Count; ++i) {
                var arg = operands[i];
                args.Add(arg);
            }
            if (code == "help" || code.StartsWith("help ")) {
                Main.ShowConsole();
                string filter = code.Substring(4).Trim();
                foreach (var pair in BatchScript.ApiDocs) {
                    if (pair.Key.Contains(filter) || pair.Value.Contains(filter)) {
                        Console.WriteLine("[{0}]:{1}", pair.Key, pair.Value);
                    }
                }
            }
            else {
                var id = BatchScript.EvalAsFunc(code, new string[] { "$query", "$result", "$actionContext" });
                if (null != id) {
                    Main.ShowConsole();
                    r = BatchScript.Call(id, args);
                }
            }
            BatchScript.RecycleCalculatorValueList(args);
            if (!r.IsNullObject) {
                Console.WriteLine("[result]:{0}", r.ToString());
            }
        }
        return r;
    }
}
internal sealed class ChangeQueryExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 2) {
            string queryStr = operands[0].AsString;
            bool requery = operands[1].GetBool();

            if (null != Main.s_CurFuncs) {
                //转到主线程执行（点击列表触发的请求好像都是从主线程发起）
                Main.s_CurFuncs.Enqueue(() => {
                    Main.s_Context.API.ChangeQuery(queryStr, requery);
                    return false;
                });
            }
            else {
                Main.s_Context.API.ChangeQuery(queryStr, requery);
            }
        }
        return CalculatorValue.NullObject;
    }
}
internal sealed class AddResultExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 5) {
            string title = operands[0].AsString;
            string subTitle = operands[1].AsString;
            string icoPath = operands[2].AsString;
            string action = operands[3].AsString;
            Query query = operands[4].As<Query>();

            if (string.IsNullOrEmpty(icoPath))
                icoPath = Main.c_IcoPath;

            var item = new Result { Title = title, SubTitle = subTitle, IcoPath = icoPath, ContextData = query };
            item.Action = e => { return Main.OnAction(action, query, item, e); };
            Main.s_NewResults.Add(item);
        }
        return CalculatorValue.NullObject;
    }
}
internal sealed class AddContextMenuExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 6) {
            string title = operands[0].AsString;
            string subTitle = operands[1].AsString;
            string icoPath = operands[2].AsString;
            string action = operands[3].AsString;
            Query query = operands[4].As<Query>();
            Result result = operands[5].As<Result>();

            if (string.IsNullOrEmpty(icoPath))
                icoPath = Main.c_IcoPath;

            var item = new Result { Title = title, SubTitle = subTitle, IcoPath = icoPath, ContextData = query };
            item.Action = e => { return Main.OnMenuAction(action, query, result, item, e); };
            Main.s_NewContextMenus.Add(item);
        }
        return CalculatorValue.NullObject;
    }
}
internal sealed class ActionKeywordRegisteredExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 1) {
            string keyword = operands[0].AsString;
            Wox.Core.Plugin.PluginManager.ActionKeywordRegistered(keyword);
        }
        return false;
    }
}
internal sealed class ClearActionKeywordsExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 1) {
            string id = operands[0].AsString;
            var plugin = Wox.Core.Plugin.PluginManager.GetPluginForId(id);
            if (null != plugin) {
                var keywords = plugin.Metadata.ActionKeywords.ToArray();
                foreach(var keyword in keywords) {
                    Wox.Core.Plugin.PluginManager.RemoveActionKeyword(id, keyword);
                }
                return true;
            }
        }
        return false;
    }
}
internal sealed class AddActionKeywordExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 2) {
            string id = operands[0].AsString;
            string keyword = operands[1].AsString;
            if (Wox.Core.Plugin.PluginManager.ActionKeywordRegistered(keyword)) {
                return false;
            }
            else {
                var plugin = Wox.Core.Plugin.PluginManager.GetPluginForId(id);
                if (null != plugin) {
                    if (plugin.Metadata.ActionKeywords.IndexOf(keyword) < 0) {
                        Wox.Core.Plugin.PluginManager.AddActionKeyword(id, keyword);
                    }
                    return true;
                }
            }
        }
        return false;
    }
}
internal sealed class ShowContextMenuExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 3) {
            string path = operands[0].AsString;
            bool ctrl = operands[1].GetBool();
            bool shift = operands[2].GetBool();

            Debug.Assert(null != Main.s_CurFuncs);
            //shell操作推到发起请求的线程执行（点击列表触发的请求好像都是从主线程发起）
            if (Directory.Exists(path)) {
                Main.s_CurFuncs.Enqueue(() => {
                    var dis = new DirectoryInfo[] { new DirectoryInfo(path) };
                    string errMsg = Main.s_ShellCtxMenu.ShowContextMenu(dis, ctrl, shift);
                    if (!string.IsNullOrEmpty(errMsg))
                        Main.LogLine(errMsg);
                    return false;
                });
            }
            else if (File.Exists(path)) {
                Main.s_CurFuncs.Enqueue(() => {
                    var fis = new FileInfo[] { new FileInfo(path) };
                    string errMsg = Main.s_ShellCtxMenu.ShowContextMenu(fis, ctrl, shift);
                    if (!string.IsNullOrEmpty(errMsg))
                        Main.LogLine(errMsg);
                    return false;
                });
            }
            return true;
        }
        return false;
    }
}
internal sealed class TryFindEverythingExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        Main.TryFindEverything();
        return CalculatorValue.From(Main.s_EverythingFullPath);
    }
}
internal sealed class EverythingExistsExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        return CalculatorValue.From(EveryThingSDK.EverythingExists());
    }
}
internal sealed class EverythingResetExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (EveryThingSDK.EverythingExists()) {
            EveryThingSDK.Everything_Reset();
        }
        return CalculatorValue.NullObject;
    }
}
internal sealed class EverythingSetDefaultExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (EveryThingSDK.EverythingExists()) {
            EveryThingSDK.Everything_SetMatchPath(false);
            EveryThingSDK.Everything_SetMatchCase(false);
            EveryThingSDK.Everything_SetMatchWholeWord(false);
            EveryThingSDK.Everything_SetRegex(false);
            EveryThingSDK.Everything_SetSort(EveryThingSDK.EVERYTHING_SORT_PATH_ASCENDING);
        }
        return CalculatorValue.NullObject;
    }
}
internal sealed class EverythingMatchPathExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 1) {
            bool val = operands[0].GetBool();
            if (EveryThingSDK.EverythingExists()) {
                EveryThingSDK.Everything_SetMatchPath(val);
                return true;
            }
            return false;
        }
        else if (EveryThingSDK.EverythingExists()) {
            return EveryThingSDK.Everything_GetMatchPath();
        }
        else {
            return false;
        }
    }
}
internal sealed class EverythingMatchCaseExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 1) {
            bool val = operands[0].GetBool();
            if (EveryThingSDK.EverythingExists()) {
                EveryThingSDK.Everything_SetMatchCase(val);
                return true;
            }
            return false;
        }
        else if (EveryThingSDK.EverythingExists()) {
            return EveryThingSDK.Everything_GetMatchCase();
        }
        else {
            return false;
        }
    }
}
internal sealed class EverythingMatchWholeWordExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 1) {
            bool val = operands[0].GetBool();
            if (EveryThingSDK.EverythingExists()) {
                EveryThingSDK.Everything_SetMatchWholeWord(val);
                return true;
            }
            return false;
        }
        else if (EveryThingSDK.EverythingExists()) {
            return EveryThingSDK.Everything_GetMatchWholeWord();
        }
        else {
            return false;
        }
    }
}
internal sealed class EverythingRegexExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 1) {
            bool val = operands[0].GetBool();
            if (EveryThingSDK.EverythingExists()) {
                EveryThingSDK.Everything_SetRegex(val);
                return true;
            }
            return false;
        }
        else if (EveryThingSDK.EverythingExists()) {
            return EveryThingSDK.Everything_GetRegex();
        }
        else {
            return false;
        }
    }
}
internal sealed class EverythingSortExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 1) {
            string type = operands[0].AsString;
            bool asc = true;
            if (operands.Count >= 2)
                asc = operands[1].GetBool();
            if (EveryThingSDK.EverythingExists()) {
                if (type == "path") {
                    if (asc)
                        EveryThingSDK.Everything_SetSort(EveryThingSDK.EVERYTHING_SORT_PATH_ASCENDING);
                    else
                        EveryThingSDK.Everything_SetSort(EveryThingSDK.EVERYTHING_SORT_PATH_DESCENDING);
                }
                else if (type == "size") {
                    if (asc)
                        EveryThingSDK.Everything_SetSort(EveryThingSDK.EVERYTHING_SORT_SIZE_ASCENDING);
                    else
                        EveryThingSDK.Everything_SetSort(EveryThingSDK.EVERYTHING_SORT_SIZE_DESCENDING);
                }
                else if (type == "time") {
                    if (asc)
                        EveryThingSDK.Everything_SetSort(EveryThingSDK.EVERYTHING_SORT_DATE_MODIFIED_ASCENDING);
                    else
                        EveryThingSDK.Everything_SetSort(EveryThingSDK.EVERYTHING_SORT_DATE_MODIFIED_DESCENDING);
                }
                else {
                    uint sort = 0;
                    if (null != type) {
                        uint.TryParse(type, out sort);
                    }
                    else {
                        sort = operands[0].GetUInt();
                    }
                    EveryThingSDK.Everything_SetSort(sort);
                    return sort.ToString();
                }
                return type + "," + (asc ? "asc" : "desc");
            }
            else {
                return string.Empty;
            }
        }
        else if (EveryThingSDK.EverythingExists()) {
            uint sort = EveryThingSDK.Everything_GetSort();
            switch (sort) {
                case EveryThingSDK.EVERYTHING_SORT_PATH_ASCENDING:
                    return "path,asc";
                case EveryThingSDK.EVERYTHING_SORT_PATH_DESCENDING:
                    return "path,desc";
                case EveryThingSDK.EVERYTHING_SORT_SIZE_ASCENDING:
                    return "size,asc";
                case EveryThingSDK.EVERYTHING_SORT_SIZE_DESCENDING:
                    return "size,desc";
                case EveryThingSDK.EVERYTHING_SORT_DATE_MODIFIED_ASCENDING:
                    return "time,asc";
                case EveryThingSDK.EVERYTHING_SORT_DATE_MODIFIED_DESCENDING:
                    return "time,desc";
                default:
                    return sort.ToString();
            }
        }
        else {
            return string.Empty;
        }
    }
}
internal sealed class EverythingSearchExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 1) {
            string str = operands[0].AsString;
            uint offset = 0;
            uint maxCount = 100;
            if (operands.Count >= 2)
                offset = operands[1].GetUInt();
            if (operands.Count >= 3)
                maxCount = operands[2].GetUInt();
            if (null != str) {
                if (EveryThingSDK.EverythingExists()) {
                    EveryThingSDK.Everything_SetSearchW(str);
                    EveryThingSDK.Everything_SetOffset(offset);
                    EveryThingSDK.Everything_SetMax(maxCount);
                    EveryThingSDK.Everything_SetRequestFlags(EveryThingSDK.EVERYTHING_REQUEST_FILE_NAME | EveryThingSDK.EVERYTHING_REQUEST_PATH | EveryThingSDK.EVERYTHING_REQUEST_SIZE | EveryThingSDK.EVERYTHING_REQUEST_DATE_MODIFIED);
                    Main.s_MessageWindow.Query();
                    uint tot = EveryThingSDK.Everything_GetTotResults();
                    if (tot > 0) {
                        List<object[]> list = new List<object[]>();
                        uint num = EveryThingSDK.Everything_GetNumResults();
                        for (uint i = 0; i < num; ++i) {
                            var sb = new StringBuilder(c_Capacity);
                            EveryThingSDK.Everything_GetResultFullPathNameW(i, sb, c_Capacity);
                            long size;
                            EveryThingSDK.Everything_GetResultSize(i, out size);
                            long time;
                            EveryThingSDK.Everything_GetResultDateModified(i, out time);
                            var dt = new DateTime(1601, 1, 1, 8, 0, 0, DateTimeKind.Utc) + new TimeSpan(time);
                            list.Add(new object[] { sb.ToString(), size, dt.ToString("yyyy-MM-dd HH:mm:ss") });
                        }
                        Main.LogLine("everything_search '{0}', result:{1}, total:{2}", str, num, tot);
                        return CalculatorValue.FromObject(list);
                    }
                    else {
                        Main.LogLine("everything_search '{0}' failed.", str);
                    }
                }
                else {
                    Main.LogLine("everything can not be found.", str);
                }
            }
        }
        return CalculatorValue.FromObject(s_EmptyList);
    }

    private static List<object[]> s_EmptyList = new List<object[]>();
    private const int c_Capacity = 4096;
}