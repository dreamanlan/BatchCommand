using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Wox.Plugin;
using BatchCommand;
using DslExpression;

//注意:Result的Title与SubTitle相同时Wox认为是相同的结果（亦即，不会更新Action等其他字段！）
public class Main : IPlugin, IContextMenu
{
    public void Init(PluginInitContext context)
    {
        string exe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        string dir = Path.GetDirectoryName(exe);
        Directory.SetCurrentDirectory(dir);
        s_StartupThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        s_Context = context;
        var txtWriter = new StringWriter(s_LogBuilder);
        Console.SetOut(txtWriter);
        Console.SetError(txtWriter);
        using(var sw = new StreamWriter(s_LogFile, false)) {
            sw.WriteLine("dir:{0} exe:{1}", dir, exe);
            sw.WriteLine("Startup Thread {0}.", s_StartupThreadId);
            sw.Close();
        }
        BatchScript.Init();
        BatchScript.Register("context", new ExpressionFactoryHelper<ContextExp>());
        BatchScript.Register("api", new ExpressionFactoryHelper<ApiExp>());
        BatchScript.Register("metadata", new ExpressionFactoryHelper<MetadataExp>());
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
    }
    public List<Result> Query(Query query)
    {
        lock (s_PluginLock) {
            s_Results.Clear();
            BatchScript.Call("on_query", CalculatorValue.FromObject(query));
            ShowLog("Query");
            return s_Results;
        }
    }
    public List<Result> LoadContextMenus(Result selectedResult)
    {
        lock (s_PluginLock) {
            s_ContextMenus.Clear();
            Query query = selectedResult.ContextData as Query;
            BatchScript.Call("on_context_menus", CalculatorValue.FromObject(query), CalculatorValue.FromObject(selectedResult));
            ShowLog("LoadContextMenus");
            return s_ContextMenus;
        }
    }

    internal static void ReloadDsl()
    {
        lock (s_PluginLock) {
            string dslPath = Path.Combine(s_Context.CurrentPluginMetadata.PluginDirectory, "main.dsl");
            var vargs = BatchScript.NewCalculatorValueList();
            vargs.Add(s_Context.CurrentPluginMetadata.ID);
            vargs.Add(CalculatorValue.FromObject(s_Context.CurrentPluginMetadata));
            vargs.Add(CalculatorValue.FromObject(s_Context));
            BatchScript.Run(dslPath, vargs);
            BatchScript.RecycleCalculatorValueList(vargs);
            ShowLog("ReloadDsl");
        }
    }
    internal static bool OnMenuAction(string action, Query query, Result result, Result menu, ActionContext e)
    {
        lock (s_PluginLock) {
            //由于Wox实现上的缺陷，这里根据menu的Title与SubTitle在菜单列表里查找真正的菜单项再调action
            int ix = s_ContextMenus.IndexOf(menu);
            if (ix < 0) {
                s_Context.API.ShowMsg("Can't find menu " + menu.Title, menu.SubTitle, menu.IcoPath);
                return false;
            }
            if (s_ContextMenus[ix].ContextData != menu.ContextData || s_ContextMenus[ix].Action != menu.Action) {
                return s_ContextMenus[ix].Action(e);
            }
            object r = BatchScript.Call(action, CalculatorValue.FromObject(query), CalculatorValue.FromObject(result), CalculatorValue.FromObject(menu), CalculatorValue.FromObject(e));
            ShowLog("OnMenuAction");
            if (s_NeedReload) {
                s_NeedReload = false;
                ReloadDsl();
            }
            if (null != r) {
                bool ret = (bool)Convert.ChangeType(r, typeof(bool));
                return ret;
            }
            else {
                return false;
            }
        }
    }
    internal static bool OnAction(string action, Query query, Result result, ActionContext e)
    {
        lock (s_PluginLock) {
            //由于Wox实现上的缺陷，这里根据result的Title与SubTitle在结果列表里查找真正的result项再调action
            int ix = s_Results.IndexOf(result);
            if (ix < 0) {
                s_Context.API.ShowMsg("Can't find result " + result.Title, result.SubTitle, result.IcoPath);
                return false;
            }
            if (s_Results[ix].ContextData != result.ContextData || s_Results[ix].Action != result.Action) {
                return s_Results[ix].Action(e);
            }
            var r = BatchScript.Call(action, CalculatorValue.FromObject(query), CalculatorValue.FromObject(result), CalculatorValue.FromObject(e));
            ShowLog("OnAction");
            if (s_NeedReload) {
                s_NeedReload = false;
                ReloadDsl();
            }
            if (r.IsBoolean) {
                bool ret = r.GetBool();
                return ret;
            }
            else {
                return false;
            }
        }
    }
    internal static void ShowLog(string tag)
    {
        var txt = s_LogBuilder.ToString();
        s_LogBuilder.Length = 0;
        if (!string.IsNullOrEmpty(txt)) {
            s_Context.API.ShowMsg(txt);
        }
        using (var sw = File.AppendText(s_LogFile)) {
            var threadLog = string.Format("{0} Thread {1}.", tag, System.Threading.Thread.CurrentThread.ManagedThreadId);
            sw.WriteLine(threadLog);
            if (!string.IsNullOrEmpty(txt))
                sw.WriteLine(txt);
            sw.Close();
        }
    }

    internal static int s_StartupThreadId = 0;
    internal static PluginInitContext s_Context = null;
    internal static List<Result> s_Results = new List<Result>();
    internal static List<Result> s_ContextMenus = new List<Result>();
    internal static bool s_NeedReload = false;

    private static string s_LogFile = "BatchCommand.log";
    private static StringBuilder s_LogBuilder = new StringBuilder();
    private static object s_PluginLock = new object();
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
            var id = BatchScript.Eval(code, new string[] { "$query", "$result", "$actionContext" });
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

            var item = new Result { Title = title, SubTitle = subTitle, IcoPath = icoPath, ContextData = query };
            item.Action = e => { return Main.OnAction(action, query, item, e); };
            Main.s_Results.Add(item);
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
            bool ctrl = (bool)Convert.ChangeType(operands[1], typeof(bool));
            bool shift = (bool)Convert.ChangeType(operands[2], typeof(bool));

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
            bool val = (bool)Convert.ChangeType(operands[0], typeof(bool));
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
            bool val = (bool)Convert.ChangeType(operands[0], typeof(bool));
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
            bool val = (bool)Convert.ChangeType(operands[0], typeof(bool));
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
            bool val = (bool)Convert.ChangeType(operands[0], typeof(bool));
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
                asc = (bool)Convert.ChangeType(operands[1], typeof(bool));
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
                    sort = (uint)Convert.ChangeType(operands[0], typeof(uint));
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
                    return CalculatorValue.FromObject(list);
                }
            }
        }
        return CalculatorValue.FromObject(s_EmptyList);
    }

    private static List<object[]> s_EmptyList = new List<object[]>();
    private const int c_Capacity = 4096;
}