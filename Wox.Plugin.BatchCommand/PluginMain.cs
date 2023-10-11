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
using static System.Net.Mime.MediaTypeNames;

//注意:Result的Title与SubTitle相同时Wox认为是相同的结果（亦即，不会更新Action等其他字段！）
public class Main : IPlugin, IContextMenu
{
    public void Init(PluginInitContext context)
    {
        s_StartupThread = Thread.CurrentThread;
        s_StartupThreadId = s_StartupThread.ManagedThreadId;
        s_Context = context;
        string exe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
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
        StartScriptThread();
    }
    public List<Result> Query(Query query)
    {
        Debug.Assert(!IsScriptThread());
        Log("query key:{0} first:{1} left:{2} from thread {3}", query.ActionKeyword, query.FirstSearch, query.SecondToEndSearch, Thread.CurrentThread.ManagedThreadId);
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
            evt.WaitOne(c_WaitTimeout);
        }
        return s_Results;
    }
    public List<Result> LoadContextMenus(Result selectedResult)
    {
        Debug.Assert(!IsScriptThread());
        Query query = selectedResult.ContextData as Query;
        Log("load context menu for [{0}|{1}], query [{2}], count {3} from thread {4}", selectedResult.Title, selectedResult.SubTitle, query.ToString(), s_ContextMenus.Count, Thread.CurrentThread.ManagedThreadId);
        //这个操作串行执行
        var evt = GetThreadEvent();
        s_ContextMenus.Clear();
        QueueScriptAction(evt, () => {
            BatchScript.Call("on_context_menus", CalculatorValue.FromObject(query), CalculatorValue.FromObject(selectedResult));
            FlushLog("LoadContextMenus");
        });
        //wait
        evt.WaitOne(c_WaitTimeout);
        return s_ContextMenus;
    }
    internal static bool OnMenuAction(string action, Query query, Result result, Result menu, ActionContext e)
    {
        Debug.Assert(!IsScriptThread());
        Log("menu action for [{0}|{1}], query [{2}], result [{3}|{4}] from thread {5}", menu.Title, menu.SubTitle, query.ToString(), result.Title, result.SubTitle, Thread.CurrentThread.ManagedThreadId);
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
        var queue = GetThreadQueue();
        QueueScriptAction(evt, () => {
            var r = BatchScript.Call(action, CalculatorValue.FromObject(query), CalculatorValue.FromObject(result), CalculatorValue.FromObject(menu), CalculatorValue.FromObject(e));
            Log("menu action for [{0}|{1}] return {2} from thread {3}", menu.Title, menu.SubTitle, r.IsNullObject ? false : r.GetBool(), Thread.CurrentThread.ManagedThreadId);
            FlushLog("OnMenuAction");
            if (s_NeedReload) {
                s_NeedReload = false;
                ReloadDsl();
            }
            if (!r.IsNullObject) {
                bool ret = r.GetBool();
                queue.Enqueue(() => ret);
            }
            else {
                queue.Enqueue(() => false);
            }
        });
        //wait
        evt.WaitOne(c_WaitTimeout);
        if(queue.TryDequeue(out var func)) {
            return func();
        }
        else {
            return false;
        }
    }
    internal static bool OnAction(string action, Query query, Result result, ActionContext e)
    {
        Debug.Assert(!IsScriptThread());
        Log("action for [{0}|{1}], query [{2}] from thread {3}", result.Title, result.SubTitle, query.ToString(), Thread.CurrentThread.ManagedThreadId);
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
        var queue = GetThreadQueue();
        QueueScriptAction(evt, () => {
            var r = BatchScript.Call(action, CalculatorValue.FromObject(query), CalculatorValue.FromObject(result), CalculatorValue.FromObject(e));
            Log("action for [{0}|{1}] return {2} from thread {3}", result.Title, result.SubTitle, r.IsNullObject ? false : r.GetBool(), Thread.CurrentThread.ManagedThreadId);
            FlushLog("OnAction");
            if (s_NeedReload) {
                s_NeedReload = false;
                ReloadDsl();
            }
            if (!r.IsNullObject) {
                bool ret = r.GetBool();
                queue.Enqueue(() => ret);
            }
            else {
                queue.Enqueue(() => false);
            }
        });
        //wait
        evt.WaitOne(c_WaitTimeout);
        if (queue.TryDequeue(out var func)) {
            return func();
        }
        else {
            return false;
        }
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

    private static void StartScriptThread()
    {
        s_ScriptThread = new Thread(ScriptThread);
        s_ScriptThread.Start();
    }
    private static void ScriptThread()
    {
        InitScript();

        while (s_StartupThread.IsAlive) {
            ProcessOnce(c_ActionNumPerTick, c_QueryNumPerTick);
            Thread.Sleep(0);
        }
        ProcessOnce(int.MaxValue, int.MaxValue);
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
        BatchScript.Register("reloaddsl", new ExpressionFactoryHelper<ReloadDslExp>());
        BatchScript.Register("evaldsl", new ExpressionFactoryHelper<EvalDslExp>());
        BatchScript.Register("addresult", new ExpressionFactoryHelper<AddResultExp>());
        BatchScript.Register("addcontextmenu", new ExpressionFactoryHelper<AddContextMenuExp>());
        BatchScript.Register("keywordregistered", new ExpressionFactoryHelper<ActionKeywordRegisteredExp>());
        BatchScript.Register("clearkeywords", new ExpressionFactoryHelper<ClearActionKeywordsExp>());
        BatchScript.Register("addkeyword", new ExpressionFactoryHelper<AddActionKeywordExp>());
        BatchScript.Register("showcontextmenu", new ExpressionFactoryHelper<ShowContextMenuExp>());
        BatchScript.Register("everythingreset", new ExpressionFactoryHelper<EverythingResetExp>());
        BatchScript.Register("everythingsetdefault", new ExpressionFactoryHelper<EverythingSetDefaultExp>());
        BatchScript.Register("everythingmatchpath", new ExpressionFactoryHelper<EverythingMatchPathExp>());
        BatchScript.Register("everythingmatchcase", new ExpressionFactoryHelper<EverythingMatchCaseExp>());
        BatchScript.Register("everythingmatchwholeword", new ExpressionFactoryHelper<EverythingMatchWholeWordExp>());
        BatchScript.Register("everythingregex", new ExpressionFactoryHelper<EverythingRegexExp>());
        BatchScript.Register("everythingsort", new ExpressionFactoryHelper<EverythingSortExp>());
        BatchScript.Register("everythingsearch", new ExpressionFactoryHelper<EverythingSearchExp>());

        ReloadDsl();

        while (s_StartupThread.IsAlive) {
            for (int ct = 0; ct < c_ActionNumPerTick && s_ScriptActionQueue.Count > 0; ++ct) {
                if (s_ScriptActionQueue.TryDequeue(out var action)) {
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
            for (int ct = 0; ct < c_QueryNumPerTick && s_ScriptQueryQueue.Count > 0; ++ct) {
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
            Thread.Sleep(0);
        }
    }
    private static void ProcessOnce(int maxActionNum, int maxQueryNum)
    {
        for (int ct = 0; ct < maxActionNum && s_ScriptActionQueue.Count > 0; ++ct) {
            if (s_ScriptActionQueue.TryDequeue(out var action)) {
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
        s_ScriptQueryQueue.Enqueue(new ScriptActionInfo { Event = evt, Action = action });
    }
    private static void QueueScriptAction(AutoResetEvent evt, Action action)
    {
        s_ScriptActionQueue.Enqueue(new ScriptActionInfo { Event = evt, Action = action });
    }

    private class ScriptActionInfo
    {
        internal AutoResetEvent Event;
        internal Action Action;
    }

    internal static PluginInitContext s_Context = null;
    internal static List<Result> s_Results = new List<Result>();
    internal static List<Result> s_NewResults = new List<Result>();
    internal static List<Result> s_ContextMenus = new List<Result>();
    internal static bool s_NeedReload = false;
    internal const string c_IcoPath = "Images\\dsl.png";

    private static int s_StartupThreadId = 0;
    private static Thread s_StartupThread = null;
    private static int s_ScriptThreadId = 0;
    private static Thread s_ScriptThread = null;
    private static string s_LogFile = "BatchCommand.log";
    private static StringBuilder s_ErrorBuilder = new StringBuilder();
    private static StringBuilder s_LogBuilder = new StringBuilder();
    private static object s_LogLock = new object();

    private static ConcurrentQueue<ScriptActionInfo> s_ScriptQueryQueue = new ConcurrentQueue<ScriptActionInfo>();
    private static ConcurrentQueue<ScriptActionInfo> s_ScriptActionQueue = new ConcurrentQueue<ScriptActionInfo>();

    private static AutoResetEvent GetThreadEvent()
    {
        if (null == s_Event)
            s_Event = new AutoResetEvent(false);
        return s_Event;
    }
    private static ConcurrentQueue<Func<bool>> GetThreadQueue()
    {
        if (null == s_Queue)
            s_Queue = new ConcurrentQueue<Func<bool>>();
        return s_Queue;
    }
    [ThreadStatic]
    private static AutoResetEvent s_Event = null;
    [ThreadStatic]
    private static ConcurrentQueue<Func<bool>> s_Queue = null;

    private const int c_QueryNumPerTick = 2;
    private const int c_ActionNumPerTick = 16;
    private const int c_WaitTimeout = 10000;
}

public class StringWriterWithLock : StringWriter
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

internal class ContextExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        return CalculatorValue.FromObject(Main.s_Context);
    }
}
internal class ApiExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        return CalculatorValue.FromObject(Main.s_Context.API);
    }
}
internal class MetadataExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        return CalculatorValue.FromObject(Main.s_Context.CurrentPluginMetadata);
    }
}
internal class ShowMsgExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count > 0) {
            string title = operands[0].ToString();
            string subtitle = operands.Count > 1 ? operands[1].ToString() : string.Empty;
            string icon = operands.Count > 2 ? operands[2].ToString() : Main.c_IcoPath;
            Main.s_Context.API.ShowMsg(title, subtitle, icon);
        }
        return CalculatorValue.NullObject;
    }
}
internal class ReloadDslExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        Main.s_NeedReload = true;
        return CalculatorValue.NullObject;
    }
}
internal class EvalDslExp : SimpleExpressionBase
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
internal class AddResultExp : SimpleExpressionBase
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
internal class AddContextMenuExp : SimpleExpressionBase
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
internal class ActionKeywordRegisteredExp : SimpleExpressionBase
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
internal class ClearActionKeywordsExp : SimpleExpressionBase
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
internal class AddActionKeywordExp : SimpleExpressionBase
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
internal class ShowContextMenuExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 3) {
            string path = operands[0].AsString;
            bool ctrl = operands[1].GetBool();
            bool shift = operands[2].GetBool();

            if (Directory.Exists(path)) {
                var dis = new DirectoryInfo[] { new DirectoryInfo(path) };
                var scm = new ShellApi.ShellContextMenu();
                scm.ShowContextMenu(dis, ctrl, shift);
            } else if (File.Exists(path)) {
                var fis = new FileInfo[] { new FileInfo(path) };
                var scm = new ShellApi.ShellContextMenu();
                scm.ShowContextMenu(fis, ctrl, shift);
            }
            return true;
        }
        return false;
    }
}
internal class EverythingResetExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        EveryThingSDK.Everything_Reset();
        return CalculatorValue.NullObject;
    }
}
internal class EverythingSetDefaultExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        EveryThingSDK.Everything_SetMatchPath(false);
        EveryThingSDK.Everything_SetMatchCase(false);
        EveryThingSDK.Everything_SetMatchWholeWord(false);
        EveryThingSDK.Everything_SetRegex(false);
        EveryThingSDK.Everything_SetSort(EveryThingSDK.EVERYTHING_SORT_PATH_ASCENDING);
        return CalculatorValue.NullObject;
    }
}
internal class EverythingMatchPathExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 1) {
            bool val = operands[0].GetBool();
            EveryThingSDK.Everything_SetMatchPath(val);
            return true;
        } else {
            return EveryThingSDK.Everything_GetMatchPath();
        }
    }
}
internal class EverythingMatchCaseExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 1) {
            bool val = operands[0].GetBool();
            EveryThingSDK.Everything_SetMatchCase(val);
            return true;
        } else {
            return EveryThingSDK.Everything_GetMatchCase();
        }
    }
}
internal class EverythingMatchWholeWordExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 1) {
            bool val = operands[0].GetBool();
            EveryThingSDK.Everything_SetMatchWholeWord(val);
            return true;
        }
        else {
            return EveryThingSDK.Everything_GetMatchWholeWord();
        }
    }
}
internal class EverythingRegexExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 1) {
            bool val = operands[0].GetBool();
            EveryThingSDK.Everything_SetRegex(val);
            return true;
        }
        else {
            return EveryThingSDK.Everything_GetRegex();
        }
    }
}
internal class EverythingSortExp : SimpleExpressionBase
{
    protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
    {
        if (operands.Count >= 1) {
            string type = operands[0].AsString;
            bool asc = true;
            if (operands.Count >= 2)
                asc = operands[1].GetBool();
            if (type == "path") {
                if(asc)
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
            } else {
                uint sort = 0;
                if (null != type) {
                    uint.TryParse(type, out sort);
                } else {
                    sort = operands[0].GetUInt();
                }
                EveryThingSDK.Everything_SetSort(sort);
                return sort.ToString();
            }
            return type + "," + (asc ? "asc" : "desc");
        }
        else {
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
    }
}
internal class EverythingSearchExp : SimpleExpressionBase
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
                EveryThingSDK.Everything_SetSearchW(str);
                EveryThingSDK.Everything_SetOffset(offset);
                EveryThingSDK.Everything_SetMax(maxCount);
                EveryThingSDK.Everything_SetRequestFlags(EveryThingSDK.EVERYTHING_REQUEST_FILE_NAME | EveryThingSDK.EVERYTHING_REQUEST_PATH | EveryThingSDK.EVERYTHING_REQUEST_SIZE | EveryThingSDK.EVERYTHING_REQUEST_DATE_MODIFIED);
                if (EveryThingSDK.Everything_QueryW(true)) {
                    List<object[]> list = new List<object[]>();
                    uint num = EveryThingSDK.Everything_GetNumResults();
                    for (uint i = 0; i < num; ++i) {
                        var sb = new StringBuilder(c_Capacity);
                        EveryThingSDK.Everything_GetResultFullPathName(i, sb, c_Capacity);
                        long size;
                        EveryThingSDK.Everything_GetResultSize(i, out size);
                        long time;
                        EveryThingSDK.Everything_GetResultDateModified(i, out time);
                        var dt = new DateTime(1601, 1, 1, 8, 0, 0, DateTimeKind.Utc) + new TimeSpan(time);
                        list.Add(new object[] { sb.ToString(), size, dt.ToString("yyyy-MM-dd HH:mm:ss") });
                    }
                    Main.LogLine("everything_search '{0}', result:{1}", str, num);
                    return CalculatorValue.FromObject(list);
                }
                else {
                    Main.LogLine("everything_search '{0}' failed.", str);
                }
            }
        }
        return CalculatorValue.FromObject(s_EmptyList);
    }

    private static List<object[]> s_EmptyList = new List<object[]>();
    private const int c_Capacity = 4096;
}