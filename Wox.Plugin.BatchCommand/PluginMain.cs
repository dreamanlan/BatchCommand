using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Wox.Plugin;
using BatchCommand;

//注意:Result的Title与SubTitle相同时Wox认为是相同的结果（亦即，不会更新Action等其他字段！）
public class Main : IPlugin, IContextMenu
{
    public void Init(PluginInitContext context)
    {
        s_StartupThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        s_Context = context;
        var txtWriter = new StringWriter(s_LogBuilder);
        Console.SetOut(txtWriter);
        Console.SetError(txtWriter);
        using(var sw = new StreamWriter(s_LogFile, false)) {
            sw.WriteLine("Startup Thread {0}.", s_StartupThreadId);
            sw.Close();
        }
        BatchScript.Init();
        BatchScript.Register("context", new Calculator.ExpressionFactoryHelper<ContextExp>());
        BatchScript.Register("api", new Calculator.ExpressionFactoryHelper<ApiExp>());
        BatchScript.Register("metadata", new Calculator.ExpressionFactoryHelper<MetadataExp>());
        BatchScript.Register("reloaddsl", new Calculator.ExpressionFactoryHelper<ReloadDslExp>());
        BatchScript.Register("evaldsl", new Calculator.ExpressionFactoryHelper<EvalDslExp>());
        BatchScript.Register("addresult", new Calculator.ExpressionFactoryHelper<AddResultExp>());
        BatchScript.Register("addcontextmenu", new Calculator.ExpressionFactoryHelper<AddContextMenuExp>());
        BatchScript.Register("keywordregistered", new Calculator.ExpressionFactoryHelper<ActionKeywordRegisteredExp>());
        BatchScript.Register("clearkeywords", new Calculator.ExpressionFactoryHelper<ClearActionKeywordsExp>());
        BatchScript.Register("addkeyword", new Calculator.ExpressionFactoryHelper<AddActionKeywordExp>());
        BatchScript.Register("showcontextmenu", new Calculator.ExpressionFactoryHelper<ShowContextMenuExp>());
        BatchScript.Register("everythingreset", new Calculator.ExpressionFactoryHelper<EverythingResetExp>());
        BatchScript.Register("everythingsetdefault", new Calculator.ExpressionFactoryHelper<EverythingSetDefaultExp>());
        BatchScript.Register("everythingmatchpath", new Calculator.ExpressionFactoryHelper<EverythingMatchPathExp>());
        BatchScript.Register("everythingmatchcase", new Calculator.ExpressionFactoryHelper<EverythingMatchCaseExp>());
        BatchScript.Register("everythingmatchwholeword", new Calculator.ExpressionFactoryHelper<EverythingMatchWholeWordExp>());
        BatchScript.Register("everythingregex", new Calculator.ExpressionFactoryHelper<EverythingRegexExp>());
        BatchScript.Register("everythingsort", new Calculator.ExpressionFactoryHelper<EverythingSortExp>());
        BatchScript.Register("everythingsearch", new Calculator.ExpressionFactoryHelper<EverythingSearchExp>());
        ReloadDsl();
    }
    public List<Result> Query(Query query)
    {
        lock (s_PluginLock) {
            s_Results.Clear();
            BatchScript.Call("on_query", query);
            ShowLog("Query");
            return s_Results;
        }
    }
    public List<Result> LoadContextMenus(Result selectedResult)
    {
        lock (s_PluginLock) {
            s_ContextMenus.Clear();
            Query query = selectedResult.ContextData as Query;
            BatchScript.Call("on_context_menus", query, selectedResult);
            ShowLog("LoadContextMenus");
            return s_ContextMenus;
        }
    }

    internal static void ReloadDsl()
    {
        lock (s_PluginLock) {
            string dslPath = Path.Combine(s_Context.CurrentPluginMetadata.PluginDirectory, "main.dsl");
            BatchScript.Run(dslPath, s_Context.CurrentPluginMetadata.ID, s_Context.CurrentPluginMetadata, s_Context);
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
            object r = BatchScript.Call(action, query, result, menu, e);
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
            object r = BatchScript.Call(action, query, result, e);
            ShowLog("OnAction");
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

internal class ContextExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        return Main.s_Context;
    }
}
internal class ApiExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        return Main.s_Context.API;
    }
}
internal class MetadataExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        return Main.s_Context.CurrentPluginMetadata;
    }
}
internal class ReloadDslExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        Main.s_NeedReload = true;
        return null;
    }
}
internal class EvalDslExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        if (operands.Count >= 1) {
            string code = operands[0] as string;
            ArrayList args = new ArrayList();
            for (int i = 1; i < operands.Count; ++i) {
                var arg = operands[i];
                args.Add(arg);
            }
            return BatchScript.Eval(code, args.ToArray());
        }
        return null;
    }
}
internal class AddResultExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        if (operands.Count >= 5) {
            string title = operands[0] as string;
            string subTitle = operands[1] as string;
            string icoPath = operands[2] as string;
            string action = operands[3] as string;
            Query query = operands[4] as Query;

            var item = new Result { Title = title, SubTitle = subTitle, IcoPath = icoPath, ContextData = query };
            item.Action = e => { return Main.OnAction(action, query, item, e); };
            Main.s_Results.Add(item);
        }
        return null;
    }
}
internal class AddContextMenuExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        if (operands.Count >= 6) {
            string title = operands[0] as string;
            string subTitle = operands[1] as string;
            string icoPath = operands[2] as string;
            string action = operands[3] as string;
            Query query = operands[4] as Query;
            Result result = operands[5] as Result;

            var item = new Result { Title = title, SubTitle = subTitle, IcoPath = icoPath, ContextData = query };
            item.Action = e => { return Main.OnMenuAction(action, query, result, item, e); };
            Main.s_ContextMenus.Add(item);
        }
        return null;
    }
}
internal class ActionKeywordRegisteredExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        if (operands.Count >= 1) {
            string keyword = operands[0] as string;
            Wox.Core.Plugin.PluginManager.ActionKeywordRegistered(keyword);
        }
        return false;
    }
}
internal class ClearActionKeywordsExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        if (operands.Count >= 1) {
            string id = operands[0] as string;
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
internal class AddActionKeywordExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        if (operands.Count >= 2) {
            string id = operands[0] as string;
            string keyword = operands[1] as string;
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
internal class ShowContextMenuExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        if (operands.Count >= 3) {
            string path = operands[0] as string;
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
internal class EverythingResetExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        EveryThingSDK.Everything_Reset();
        return null;
    }
}
internal class EverythingSetDefaultExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        EveryThingSDK.Everything_SetMatchPath(false);
        EveryThingSDK.Everything_SetMatchCase(false);
        EveryThingSDK.Everything_SetMatchWholeWord(false);
        EveryThingSDK.Everything_SetRegex(false);
        EveryThingSDK.Everything_SetSort(EveryThingSDK.EVERYTHING_SORT_PATH_ASCENDING);
        return null;
    }
}
internal class EverythingMatchPathExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
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
internal class EverythingMatchCaseExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
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
internal class EverythingMatchWholeWordExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
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
internal class EverythingRegexExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
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
internal class EverythingSortExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        if (operands.Count >= 1) {
            string type = operands[0] as string;
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
internal class EverythingSearchExp : Calculator.SimpleExpressionBase
{
    protected override object OnCalc(IList<object> operands)
    {
        if (operands.Count >= 1) {
            string str = operands[0] as string;
            uint offset = 0;
            uint maxCount = 100;
            if (operands.Count >= 2)
                offset = (uint)Convert.ChangeType(operands[1], typeof(uint));
            if (operands.Count >= 3)
                maxCount = (uint)Convert.ChangeType(operands[2], typeof(uint));
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
                    return list;
                }
            }
        }
        return s_EmptyList;
    }

    private static List<object[]> s_EmptyList = new List<object[]>();
    private const int c_Capacity = 4096;
}