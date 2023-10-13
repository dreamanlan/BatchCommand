using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Wox.Plugin;
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

/// <remarks>
/// 目前插件只用于1.3.524版本的Wox，更新的版本因为插件初始化不在主线程，MessageWindow的功能会不正常
/// 注意:Result的Title与SubTitle相同时Wox认为是相同的结果（亦即，不会更新Action等其他字段！）
/// </remarks>
public sealed class Main : IPlugin, IContextMenu
{
    public void Init(PluginInitContext context)
    {
        s_StartupThread = Thread.CurrentThread;
        s_StartupThreadId = s_StartupThread.ManagedThreadId;
        s_Context = context;
        var p = Process.GetCurrentProcess();
        string exe = p.MainModule.FileName;
        string dir = Path.GetDirectoryName(exe);
        Directory.SetCurrentDirectory(dir);
        var errWriter = new StringWriterWithLock(s_ErrorBuilder, s_LogLock);
        Console.SetOut(errWriter);
        Console.SetError(errWriter);
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
        Debug.Assert(!IsScriptThread());
        LogLine("query key:{0} first:{1} left:{2} from thread {3}", query.ActionKeyword, query.FirstSearch, query.SecondToEndSearch, Thread.CurrentThread.ManagedThreadId);
        if (EveryThingSDK.EverythingExists()) {
            TryFindEverything();

            if (s_ScriptQueryQueue.Count == 0) {
                var evt = GetThreadEvent();
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
            else if (query.ActionKeyword == "dsl") {
                if (query.FirstSearch == "restart") {
                    System.Windows.Application.Current.Dispatcher.Invoke(() => {
                        s_Context.API.RestarApp();
                    });
                }
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
        s_ContextMenus.Clear();
        QueueScriptAction(evt, funcs, () => {
            BatchScript.Call("on_context_menus", CalculatorValue.FromObject(query), CalculatorValue.FromObject(selectedResult));
            FlushLog("LoadContextMenus");
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
        if (s_Results[ix].ContextData != result.ContextData || s_Results[ix].Action != result.Action) {
            return s_Results[ix].Action(e);
        }
        //这里开始串行执行
        var evt = GetThreadEvent();
        var funcs = GetThreadFuncs();
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
            s_Context.API.ShowMsg(err, tag, c_IcoPath);
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
    }
    private static void InitScript()
    {
        s_ScriptThreadId = Thread.CurrentThread.ManagedThreadId;
        Log("Script Thread {0}", s_ScriptThreadId);
        FlushLog("InitScript");

        BatchScript.Init();
        BatchScript.Register("context", new ExpressionFactoryHelper<ContextExp>());
        BatchScript.Register("api", new ExpressionFactoryHelper<ApiExp>());
        BatchScript.Register("metadata", new ExpressionFactoryHelper<MetadataExp>());
        BatchScript.Register("showmsg", new ExpressionFactoryHelper<ShowMsgExp>());
        BatchScript.Register("restart", new ExpressionFactoryHelper<RestartExp>());
        BatchScript.Register("reloaddsl", new ExpressionFactoryHelper<ReloadDslExp>());
        BatchScript.Register("evaldsl", new ExpressionFactoryHelper<EvalDslExp>());
        BatchScript.Register("changequery", new ExpressionFactoryHelper<ChangeQueryExp>());
        BatchScript.Register("addresult", new ExpressionFactoryHelper<AddResultExp>());
        BatchScript.Register("addcontextmenu", new ExpressionFactoryHelper<AddContextMenuExp>());
        BatchScript.Register("keywordregistered", new ExpressionFactoryHelper<ActionKeywordRegisteredExp>());
        BatchScript.Register("clearkeywords", new ExpressionFactoryHelper<ClearActionKeywordsExp>());
        BatchScript.Register("addkeyword", new ExpressionFactoryHelper<AddActionKeywordExp>());
        BatchScript.Register("showcontextmenu", new ExpressionFactoryHelper<ShowContextMenuExp>());
        BatchScript.Register("tryfindeverything", new ExpressionFactoryHelper<TryFindEverythingExp>());
        BatchScript.Register("everythingexists", new ExpressionFactoryHelper<EverythingExistsExp>());
        BatchScript.Register("everythingreset", new ExpressionFactoryHelper<EverythingResetExp>());
        BatchScript.Register("everythingsetdefault", new ExpressionFactoryHelper<EverythingSetDefaultExp>());
        BatchScript.Register("everythingmatchpath", new ExpressionFactoryHelper<EverythingMatchPathExp>());
        BatchScript.Register("everythingmatchcase", new ExpressionFactoryHelper<EverythingMatchCaseExp>());
        BatchScript.Register("everythingmatchwholeword", new ExpressionFactoryHelper<EverythingMatchWholeWordExp>());
        BatchScript.Register("everythingregex", new ExpressionFactoryHelper<EverythingRegexExp>());
        BatchScript.Register("everythingsort", new ExpressionFactoryHelper<EverythingSortExp>());
        BatchScript.Register("everythingsearch", new ExpressionFactoryHelper<EverythingSearchExp>());

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
                    action.Action();
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
    internal static string s_EverythingFullPath = string.Empty;
    internal static bool s_NeedReload = false;
    internal static ConcurrentQueue<Func<bool>> s_CurFuncs = null;
    internal const string c_IcoPath = "Images\\dsl.png";

    private static int s_StartupThreadId = 0;
    private static Thread s_StartupThread = null;
    private static int s_ScriptThreadId = 0;
    private static Thread s_ScriptThread = null;
    private static string s_LogFile = "BatchCommand.log";
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
            var id = BatchScript.EvalAsFunc(code, new string[] { "$query", "$result", "$actionContext" });
            if (null != id) {
                r = BatchScript.Call(id, args);
            }
            BatchScript.RecycleCalculatorValueList(args);
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
            Main.s_ContextMenus.Add(item);
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