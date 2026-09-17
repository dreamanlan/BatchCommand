using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Flow.Launcher.Plugin;
using BatchCommand;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Globalization;
using System.Windows.Documents;
using System.Runtime;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using ScriptableFramework;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using System.Net.Mail;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows;

/// <remarks>
/// Note: When the Title and SubTitle of the Result are the same, Wox considers them to be the same result (i.e., it will not update other fields such as Action!).
/// </remarks>
public sealed class Main : IAsyncPlugin, IContextMenu, IAsyncReloadable, IPluginI18n, ISavable, IResultUpdated
{
    public enum MainThreadSyncExecutionTypeEnum
    {
        Dispatcher = 0,
        Rendering,
        Message,
        Num
    }

    public Main()
    {
        //Note: The construction of the plugin is not executed in the main thread.
        //Settings are loaded in Init because the Flow storage API requires the plugin context.
        s_Instance = this;
    }
    public Task InitAsync(PluginInitContext context)
    {
        s_StartupThread = Thread.CurrentThread;
        s_StartupThreadId = s_StartupThread.ManagedThreadId;
        s_Context = context;
        m_Settings = context.API.LoadSettingJsonStorage<PluginSettings>();
        s_ShellCtxMenu.FolderUseDefaultOnly = m_Settings.FolderUseDefaultOnly;
        s_ShellCtxMenu.FileUseDefaultOnly = m_Settings.FileUseDefaultOnly;
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

        //The message window must live on the WPF UI thread: its WndProc receives the
        //Everything IPC replies (WM_COPYDATA), which requires a pumping message loop.
        //Flow 1.4+ runs plugin InitAsync on a worker thread (no message loop), so
        //create the window through the dispatcher instead.
        Main.Dispatcher().Invoke(() => {
            s_MessageWindow = new MessageWindow();
            s_MessageWindow.Hide();
        });

        CompositionTarget.Rendering += CompositionTarget_Rendering;
        StartScriptThread();
        return Task.CompletedTask;
    }

    public Task<List<Result>> QueryAsync(Query query, CancellationToken token)
    {
        const int c_MaxQueueNum = 64;
        Debug.Assert(!IsScriptThread());
        LogLine("query key:{0} first:{1} second to end:{2} from thread {3}", query.ActionKeyword, query.FirstSearch, query.SecondToEndSearch, Thread.CurrentThread.ManagedThreadId);
        if (EveryThingSDK.EverythingExists()) {
            TryFindEverything();

            if (query.ActionKeyword == "dsl" && query.FirstSearch == "restart") {
                //The restart function is implemented in C# to prevent the DSL script thread from getting stuck and unable to restart due to certain functions.
                var item = new Result { Title = "restart", SubTitle = "restart Flow-Launcher", IcoPath = c_IcoPath, ContextData = query };
                item.AsyncAction = e => Main.OnActionAsync("on_action_restart", query, item, e);
                s_Results.Clear();
                s_Results.Add(item);
            }
            else if (token.IsCancellationRequested) {
                //Flow canceled this query before we even queued it
                return Task.FromResult(s_Results);
            }
            else if (s_ScriptQueryQueue.Count < c_MaxQueueNum) {
                //Queue to the script thread and complete the returned task when the script
                //is done; no thread is blocked while waiting and cancellation is honored.
                var tcs = new TaskCompletionSource<List<Result>>(TaskCreationOptions.RunContinuationsAsynchronously);
                var reg = token.Register(() => tcs.TrySetCanceled(token));
                QueueScriptQuery(tcs, reg, () => {
                    //The result list is cleared only when a new query is executed. At this point, there should be no threads still using it.
                    //Alternatively, we could construct a new list every time, but let's reduce some garbage collection for now.
                    s_NewResults.Clear();
                    BatchScript.Call("on_query", BoxedValue.FromObject(query));
                    CheckDslError();
                    FlushLog("Query");
                    //swap
                    var t = s_Results;
                    s_Results = s_NewResults;
                    s_NewResults = t;
                });
                return tcs.Task;
            }
        }
        else {
            LogLine("query [{0}] can't find everthing, run everything and restart Flow-Launcher from thread {1}", query.ToString(), Thread.CurrentThread.ManagedThreadId);
            if (string.IsNullOrEmpty(s_EverythingFullPath)) {
                LogLine("can't find everything install location!");
            }
            else {
                Process.Start(s_EverythingFullPath);
            }
            AsyncExecuteOnMainThread(() => {
                s_Context.API.RestartApp();
            });
        }
        return Task.FromResult(s_Results);
    }
    public List<Result> LoadContextMenus(Result selectedResult)
    {
        Debug.Assert(!IsScriptThread());
        Query query = selectedResult.ContextData as Query;
        LogLine("load context menu for [{0}|{1}], query [{2}], count {3} from thread {4}", selectedResult.Title, selectedResult.SubTitle, query.ToString(), s_ContextMenus.Count, Thread.CurrentThread.ManagedThreadId);
        //This operation is executed in a serial manner.
        var evt = GetThreadEvent();
        var funcs = GetThreadFuncs();
        evt.Reset();
        QueueScriptAction(evt, funcs, () => {
            //The result list is cleared only when a new query is executed. At this point, there should be no threads still using it.
            //Alternatively, we could construct a new list every time, but let's reduce some garbage collection for now.
            s_NewContextMenus.Clear();
            BatchScript.Call("on_context_menus", BoxedValue.FromObject(query), BoxedValue.FromObject(selectedResult));
            CheckDslError();
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
    internal static ValueTask<bool> OnMenuActionAsync(string action, Query query, Result result, Result menu, ActionContext e)
    {
        Debug.Assert(!IsScriptThread());
        LogLine("menu action for [{0}|{1}], query [{2}], result [{3}|{4}] from thread {5}", menu.Title, menu.SubTitle, query.ToString(), result.Title, result.SubTitle, Thread.CurrentThread.ManagedThreadId);
        //Due to the implementation flaw of Flow-Launcher, we need to search for the actual result item in the result list based on
        //the Title and SubTitle of the result before calling the action.
        //The host may pass a stale menu/result object after the result list was updated (async
        //queries make this race more likely). The script only reads the title and query from it,
        //so dispatch the stale object instead of failing.
        int ix = s_ContextMenus.IndexOf(menu);
        if (ix >= 0 && (s_ContextMenus[ix].ContextData != menu.ContextData || s_ContextMenus[ix].Action != menu.Action)) {
            return s_ContextMenus[ix].AsyncAction?.Invoke(e) ?? new ValueTask<bool>(s_ContextMenus[ix].Action?.Invoke(e) ?? false);
        }
        if (ix < 0) {
            LogLine("menu action for [{0}|{1}] dispatches a stale menu", menu.Title, menu.SubTitle);
        }
        //Serial execution starts here. The Flow UI thread returns immediately (AsyncAction),
        //so cross-thread calls from the script no longer need a pumping waiter: they are
        //marshalled to the (idle) UI thread dispatcher by the completion callback below.
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var funcs = new ConcurrentQueue<Func<bool>>();
        QueueScriptAction(null, funcs, () => {
            var r = BatchScript.Call(action, BoxedValue.FromObject(query), BoxedValue.FromObject(result), BoxedValue.FromObject(menu), BoxedValue.FromObject(e));
            CheckDslError();
            Log("menu action for [{0}|{1}] return {2} from thread {3}", menu.Title, menu.SubTitle, r.IsNullObject ? false : r.GetBool(), Thread.CurrentThread.ManagedThreadId);
            FlushLog("OnMenuAction");
            if (s_NeedReload) {
                s_NeedReload = false;
                ReloadDsl();
            }
            //Flow API calls queued by the script (changequery etc.) run on the UI thread,
            //in order, before the task completes.
            while (funcs.TryDequeue(out var f)) {
                var f2 = f;
                Dispatcher().InvokeAsync(() => f2());
            }
            bool ret = r.IsNullObject ? false : r.GetBool();
            Dispatcher().InvokeAsync(() => tcs.TrySetResult(ret));
        });
        return new ValueTask<bool>(tcs.Task);
    }
    internal static ValueTask<bool> OnActionAsync(string action, Query query, Result result, ActionContext e)
    {
        Debug.Assert(!IsScriptThread());
        LogLine("action for [{0}|{1}], query [{2}] from thread {3}", result.Title, result.SubTitle, query.ToString(), Thread.CurrentThread.ManagedThreadId);
        //Due to the implementation flaw of Flow-Launcher, we need to search for the actual result item in the result list based on
        //the Title and SubTitle of the result before calling the action.
        //The host may pass a stale result object after the result list was updated (async
        //queries make this race more likely). The script only reads the title and query from it,
        //so dispatch the stale object instead of failing.
        int ix = s_Results.IndexOf(result);
        if (ix < 0) {
            LogLine("action for [{0}|{1}] dispatches a stale result, query [{2}]", result.Title, result.SubTitle, query.ToString());
        }
        if (action == "on_action_restart") {
            //This should already be the main thread. But let's dispatch once and execute it in the next frame.
            AsyncExecuteOnMainThread(() => {
                s_Context.API.RestartApp();
            });
            return new ValueTask<bool>(true);
        }
        if (ix >= 0 && (s_Results[ix].ContextData != result.ContextData || s_Results[ix].Action != result.Action)) {
            return s_Results[ix].AsyncAction?.Invoke(e) ?? new ValueTask<bool>(s_Results[ix].Action?.Invoke(e) ?? false);
        }
        //Serial execution starts here. The Flow UI thread returns immediately (AsyncAction),
        //so cross-thread calls from the script no longer need a pumping waiter: they are
        //marshalled to the (idle) UI thread dispatcher by the completion callback below.
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var funcs = new ConcurrentQueue<Func<bool>>();
        QueueScriptAction(null, funcs, () => {
            var r = BatchScript.Call(action, BoxedValue.FromObject(query), BoxedValue.FromObject(result), BoxedValue.FromObject(e));
            CheckDslError();
            Log("action for [{0}|{1}] return {2} from thread {3}", result.Title, result.SubTitle, r.IsNullObject ? false : r.GetBool(), Thread.CurrentThread.ManagedThreadId);
            FlushLog("OnAction");
            if (s_NeedReload) {
                s_NeedReload = false;
                ReloadDsl();
            }
            //Flow API calls queued by the script (changequery etc.) run on the UI thread,
            //in order, before the task completes.
            while (funcs.TryDequeue(out var f)) {
                var f2 = f;
                Dispatcher().InvokeAsync(() => f2());
            }
            bool ret = r.IsNullObject ? false : r.GetBool();
            Dispatcher().InvokeAsync(() => tcs.TrySetResult(ret));
        });
        return new ValueTask<bool>(tcs.Task);
    }

    public Task ReloadDataAsync()
    {
        //nothing to do
        return Task.CompletedTask;
    }

    public event ResultUpdatedEventHandler ResultsUpdated;

    //Post the results accumulated by addresult so far to the Flow UI immediately,
    //while the DSL query script keeps running (IResultUpdated partial results).
    //The list is snapshotted: Flow updates the UI on its dispatcher thread while
    //the script thread keeps appending to s_NewResults.
    private static Main s_Instance;
    internal static void PostResults(Query query)
    {
        var inst = s_Instance;
        if (null == inst || null == inst.ResultsUpdated) {
            return;
        }
        List<Result> results;
        lock (s_NewResults) {
            results = new List<Result>(s_NewResults);
        }
        if (results.Count > 0) {
            inst.ResultsUpdated(inst, new ResultUpdatedEventArgs { Query = query, Results = results });
        }
    }

    public string GetTranslatedPluginTitle()
    {
        var title = s_Context.API.GetTranslation("flow_launcher_plugin_batchcommand_plugin_name");
        return string.IsNullOrEmpty(title) ? "Dsl BatchCommand Plugin" : title;
    }

    public string GetTranslatedPluginDescription()
    {
        var desc = s_Context.API.GetTranslation("flow_launcher_plugin_batchcommand_plugin_description");
        return string.IsNullOrEmpty(desc) ? "use MetaDSL extend Flow-Launcher" : desc;
    }

    public void Save()
    {
        m_Settings.FolderUseDefaultOnly = s_ShellCtxMenu.FolderUseDefaultOnly;
        m_Settings.FileUseDefaultOnly = s_ShellCtxMenu.FileUseDefaultOnly;
        s_Context.API.SaveSettingJsonStorage<PluginSettings>();
    }

    public class PluginSettings
    {
        public bool FolderUseDefaultOnly { get; set; } = true;
        public bool FileUseDefaultOnly { get; set; } = false;
    }

    private PluginSettings m_Settings;

    internal static void ReloadDsl()
    {
        s_AutoCompletions = LoadAutoComplete(s_Context.CurrentPluginMetadata.PluginDirectory);

        string dslPath = Path.Combine(s_Context.CurrentPluginMetadata.PluginDirectory, "main.dsl");
        var vargs = BatchScript.NewCalculatorValueList();
        vargs.Add(s_Context.CurrentPluginMetadata.ID);
        vargs.Add(BoxedValue.FromObject(s_Context.CurrentPluginMetadata));
        vargs.Add(BoxedValue.FromObject(s_Context));
        BatchScript.Run(dslPath, vargs);
        BatchScript.RecycleCalculatorValueList(vargs);
        CheckDslError();
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
    internal static ConcurrentDictionary<string, string> LoadAutoComplete(string fileDir)
    {
        var autoCompletions = new ConcurrentDictionary<string, string>();
        string autoCompleteFile = Path.Combine(fileDir, "AutoComplete.txt");
        if (File.Exists(autoCompleteFile)) {
            var lines = File.ReadAllLines(autoCompleteFile);
            foreach (var line in lines) {
                int si = line.IndexOf("=>");
                if (si > 0) {
                    string key = line.Substring(0, si);
                    string val = line.Substring(si + 2);
                    autoCompletions.TryAdd(key, val);
                }
            }
        }
        return autoCompletions;
    }

    private void CompositionTarget_Rendering(object sender, EventArgs e)
    {
        if (s_MainThreadSyncExecutionType == MainThreadSyncExecutionTypeEnum.Rendering) {
            HandleMainThreadActionOnce();
        }
    }
    internal static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (s_MainThreadSyncExecutionType == MainThreadSyncExecutionTypeEnum.Message) {
            HandleMainThreadActionOnce();
        }
        handled = false;
        return IntPtr.Zero;
    }
    internal static void HandleMainThreadActionOnce()
    {
        for (int ct = 0; s_MainActions.Count > 0; ++ct) {
            if (s_MainActions.TryDequeue(out var action)) {
                try {
                    action();
                }
                catch (Exception ex) {
                    Console.WriteLine("exception:{0}", ex.Message);
                }
                finally {
                }
            }
        }
    }

    internal static void SyncExecuteOnMainThread(Action action)
    {
        switch (s_MainThreadSyncExecutionType) {
            case MainThreadSyncExecutionTypeEnum.Dispatcher:
                Dispatcher().Invoke(action, System.Windows.Threading.DispatcherPriority.Send);
                break;
            default:
                s_MainEvent.Reset();
                s_MainActions.Enqueue(() => {
                    action();
                    s_MainEvent.Set();
                });
                s_MainEvent.WaitOne();
                break;
        }
    }
    internal static System.Windows.Threading.DispatcherOperation AsyncExecuteOnMainThread(Action action)
    {
        return Dispatcher().InvokeAsync(action, System.Windows.Threading.DispatcherPriority.Send);
    }
    internal static System.Windows.Threading.Dispatcher Dispatcher()
    {
        return System.Windows.Application.Current.Dispatcher;
    }
    // Owner for WinForms common dialogs: a tiny, off-screen, topmost helper window.
    // Owning the dialogs to the host main window would hide them together with it
    // (the host hides its window when it loses activation, e.g. when the dialog takes
    // focus), while an independent topmost owner keeps the dialogs visible above
    // everything regardless of the host window state. Must be called on the main thread.
    private static System.Windows.Forms.Form s_DialogOwnerForm;
    internal static System.Windows.Forms.IWin32Window GetDialogOwner()
    {
        if (null == s_DialogOwnerForm) {
            s_DialogOwnerForm = new DialogOwnerForm {
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None,
                ShowInTaskbar = false,
                StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                Location = new System.Drawing.Point(-32000, -32000),
                Size = new System.Drawing.Size(1, 1),
                TopMost = true,
            };
            s_DialogOwnerForm.Show();
        }
        return s_DialogOwnerForm;
    }

    // Temporarily suppress the host's hide-on-deactivate behavior while a modal
    // dialog is up, so the main window stays visible during a multi-dialog DSL
    // flow (it should only hide when the whole flow finishes). Reflection based
    // on the Flow API internals; gives up silently when they change.
    // Returns the original setting value to pass to RestoreHideWhenDeactivated.
    private static object s_FlowSettings;
    private static System.Reflection.PropertyInfo s_HideWhenDeactivatedProp;
    internal static bool SuppressHideWhenDeactivated()
    {
        try {
            if (null == s_HideWhenDeactivatedProp) {
                var api = s_Context?.API;
                if (null == api) {
                    return true;
                }
                const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                foreach (var f in api.GetType().GetFields(flags)) {
                    var prop = f.FieldType.GetProperty("HideWhenDeactivated");
                    if (null != prop && prop.CanWrite) {
                        s_FlowSettings = f.GetValue(api);
                        s_HideWhenDeactivatedProp = prop;
                        break;
                    }
                }
                if (null == s_HideWhenDeactivatedProp) {
                    return true;
                }
            }
            bool orig = (bool)s_HideWhenDeactivatedProp.GetValue(s_FlowSettings);
            s_HideWhenDeactivatedProp.SetValue(s_FlowSettings, false);
            return orig;
        }
        catch (Exception ex) {
            LogLine("SuppressHideWhenDeactivated failed: {0}", ex.Message);
            return true;
        }
    }
    internal static void RestoreHideWhenDeactivated(bool orig)
    {
        try {
            if (null != s_HideWhenDeactivatedProp) {
                s_HideWhenDeactivatedProp.SetValue(s_FlowSettings, orig);
            }
        }
        catch (Exception ex) {
            LogLine("RestoreHideWhenDeactivated failed: {0}", ex.Message);
        }
    }

    internal static void CheckDslError()
    {
        if (BatchCommand.BatchScript.HasDslErrors) {
            LogLine("[csharp] Dsl error: {0}", BatchCommand.BatchScript.GetDslErrors());
            BatchCommand.BatchScript.ClearDslErrors();
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

            handle = GetConsoleWindow();
            IntPtr hMenu = GetSystemMenu(handle, false);
            EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
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

    //Why a single script thread (do not "optimize" this into a thread pool):
    //1. The API/environment layer below the scripts assumes a single writer: the
    //   Everything SDK is process-global query state (search text/offset/results are
    //   set per query), the result lists (s_Results/s_NewResults) follow a
    //   clear/add/swap lifecycle whose reference is handed to Flow, and the
    //   script-to-UI callback queue (s_CurFuncs) is a plain static. (The DSL
    //   interpreters themselves would be fine on multiple threads: each thread
    //   gets its own interpreter instance with its own script globals.)
    //2. Queries: continuous typing floods the queue, and only the last queued query is
    //   actually executed (see ProcessOnce) while canceled ones are skipped, so serial
    //   execution merges consecutive keystrokes into a single script run. Worker threads
    //   would run every doomed query in parallel and pay for results that get discarded.
    //3. The only thing more threads could buy is avoiding head-of-line blocking on a slow
    //   query, but queries are millisecond-level (Everything IPC). The long operations
    //   (modal dialogs) live in actions, and blocking there is semantically correct:
    //   the user's focus is on the dialog, no concurrent queries are expected.
    private static void StartScriptThread()
    {
        TryFindEverything();

        s_ScriptThread = new Thread(ScriptThread);
        s_ScriptThread.Name = "Script Thread";
        //Background: the loop no longer watches the startup thread (Flow inits plugins
        //on a short-lived worker thread, which would stop the loop right after Init),
        //so the thread must not keep the process alive at exit.
        s_ScriptThread.IsBackground = true;
        s_ScriptThread.Start();
    }
    private static void ScriptThread()
    {
        InitScript();

        //Stop when the host application is shutting down (its dispatcher shuts down),
        //replacing the old check on the init thread's lifetime.
        while (true) {
            var app = System.Windows.Application.Current;
            if (null != app && app.Dispatcher.HasShutdownStarted)
                break;
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
        BatchScript.Register("max_artificial_score", "max_artificial_score()", new ExpressionFactoryHelper<MaxArtificialScoreExp>());
        BatchScript.Register("set_sync_execution_type", "set_sync_execution_type(type), 0--rendering 1--message 2--dispatcher", new ExpressionFactoryHelper<MainThreadSyncExecutionTypeExp>());
        BatchScript.Register("folderdlg", "folderdlg(desc[,int_spec_dir_enum])", new ExpressionFactoryHelper<FolderDialogExp>());
        BatchScript.Register("openfiledlg", "openfiledlg(title,init_dir,filter[,filter_index])", new ExpressionFactoryHelper<OpenFileDialogExp>());
        BatchScript.Register("openfilesdlg", "openfilesdlg(title,init_dir,filter[,filter_index])", new ExpressionFactoryHelper<OpenFilesDialogExp>());
        BatchScript.Register("savefiledlg", "savefiledlg(title,init_dir,filter[,filter_index])", new ExpressionFactoryHelper<SaveFileDialogExp>());
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
        BatchScript.Register("postresult", "postresult(query) posts the results added by addresult so far to the UI immediately, the query script keeps running; can be called multiple times to show results progressively", new ExpressionFactoryHelper<PostResultExp>());
        BatchScript.Register("addcontextmenu", "addcontextmenu api", new ExpressionFactoryHelper<AddContextMenuExp>());
        BatchScript.Register("keywordregistered", "keywordregistered api", new ExpressionFactoryHelper<ActionKeywordRegisteredExp>());
        BatchScript.Register("clearkeywords", "clearkeywords api", new ExpressionFactoryHelper<ClearActionKeywordsExp>());
        BatchScript.Register("addkeyword", "addkeyword api", new ExpressionFactoryHelper<AddActionKeywordExp>());
        BatchScript.Register("showcontextmenu", "showcontextmenu api", new ExpressionFactoryHelper<ShowContextMenuExp>());
        BatchScript.Register("reloadautocomplete", "reloadautocomplete api", new ExpressionFactoryHelper<ReloadAutoCompleteExp>());
        BatchScript.Register("getautocomplete", "getautocomplete api", new ExpressionFactoryHelper<GetAutoCompleteExp>());
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
                    action.Event?.Set();
                }
            }
        }
        for (int ct = 0; ct < maxQueryNum && s_ScriptQueryQueue.Count > 0; ++ct) {
            if (s_ScriptQueryQueue.TryDequeue(out var action)) {
                //skip queries canceled while waiting in the queue: Flow has already moved
                //on to a newer query, running the script would only waste time
                if (action.Tcs.Task.IsCanceled) {
                    action.Reg.Dispose();
                    continue;
                }
                try {
                    //It appears that only the last query in the queue is being executed,
                    //and the intermediate queries that are entered consecutively are not being executed.
                    if (s_ScriptQueryQueue.Count == 0) {
                        action.Action();
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("exception:{0}", ex.Message);
                }
                finally {
                    //notify: complete the query task even on failure or skip, so the
                    //awaiter never hangs; TrySetResult is a no-op when already canceled.
                    action.Reg.Dispose();
                    action.Tcs.TrySetResult(s_Results);
                }
            }
        }
    }
    private static void QueueScriptQuery(TaskCompletionSource<List<Result>> tcs, CancellationTokenRegistration reg, Action action)
    {
        s_ScriptQueryQueue.Enqueue(new ScriptQueryInfo { Tcs = tcs, Reg = reg, Action = action });
    }
    private static void QueueScriptAction(AutoResetEvent evt, ConcurrentQueue<Func<bool>> funcs, Action action)
    {
        s_ScriptActionQueue.Enqueue(new ScriptActionInfo { Event = evt, Action = action, Funcs = funcs });
    }
    private static bool WaitAndGetResult(AutoResetEvent evt, ConcurrentQueue<Func<bool>> funcs)
    {
        int waitTime = 0;
        while (waitTime < c_WaitTimeout) {
            if(evt.WaitOne(10)) {
                break;
            }
            else {
                Dispatcher().Invoke(s_EmptyAction, System.Windows.Threading.DispatcherPriority.Background);
                if (s_MainThreadSyncExecutionType != MainThreadSyncExecutionTypeEnum.Dispatcher) {
                    HandleMainThreadActionOnce();
                }
                waitTime += c_WaitTimeSlice;
            }
        }
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
        internal TaskCompletionSource<List<Result>> Tcs;
        internal CancellationTokenRegistration Reg;
        internal Action Action;
    }
    private class ScriptActionInfo
    {
        internal AutoResetEvent Event;
        internal Action Action;
        internal ConcurrentQueue<Func<bool>> Funcs;
    }

    internal static MainThreadSyncExecutionTypeEnum s_MainThreadSyncExecutionType = MainThreadSyncExecutionTypeEnum.Dispatcher;
    internal static Action s_EmptyAction = () => { };
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
    internal static ConcurrentDictionary<string, string> s_AutoCompletions = null;
    internal const string c_IcoPath = "Images\\dsl.png";
    internal const int c_MaxArtificialScore = 100000000;
    internal const int c_MaxSearchScore = 10000;
    internal const int c_MaxSearchResults = 1000;
    internal const int c_DefMaxSearchResults = 100;

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

    private static ConcurrentQueue<Action> s_MainActions = new ConcurrentQueue<Action>();
    private static AutoResetEvent s_MainEvent = new AutoResetEvent(false);

    private static ConcurrentQueue<ScriptQueryInfo> s_ScriptQueryQueue = new ConcurrentQueue<ScriptQueryInfo>();
    private static ConcurrentQueue<ScriptActionInfo> s_ScriptActionQueue = new ConcurrentQueue<ScriptActionInfo>();

    private static AutoResetEvent GetThreadEvent()
    {
        if (null == tls_Event)
            tls_Event = new AutoResetEvent(false);
        return tls_Event;
    }
    private static ConcurrentQueue<Func<bool>> GetThreadFuncs()
    {
        if (null == tls_Funcs)
            tls_Funcs = new ConcurrentQueue<Func<bool>>();
        return tls_Funcs;
    }
    [ThreadStatic]
    private static AutoResetEvent tls_Event = null;
    [ThreadStatic]
    private static ConcurrentQueue<Func<bool>> tls_Funcs = null;

    private const int c_QueryNumPerTick = 2;
    private const int c_ActionNumPerTick = 16;
    private const int c_WaitTimeout = 10000;
    private const int c_WaitTimeSlice = 10;

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

    const uint SC_CLOSE = 0xF060;
    const uint MF_BYCOMMAND = 0x00000000;
    const uint MF_GRAYED = 0x00000001;
    const uint MF_ENABLED = 0x00000000;

    [DllImport("user32.dll")]
    static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
    [DllImport("user32.dll")]
    static extern int EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
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
///It seems that the Form needs to be initialized in the main thread to function properly.
///</remarks>
public sealed class MessageWindow : Form
{
    public MessageWindow()
    {
        m_EverythingQueryEvent = new AutoResetEvent(false);
        var accessHandle = this.Handle;
    }

    public bool Query()
    {
        m_EverythingQueryEvent.Reset();
        m_CheckReply = true;
        EveryThingSDK.Everything_SetReplyWindow(m_Handle);
        EveryThingSDK.Everything_SetReplyID(MY_REPLY_ID);
        EveryThingSDK.Everything_QueryW(false);
        return m_EverythingQueryEvent.WaitOne(WAIT_TIME_OUT);
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

        if (!m_MessageHookInited) {
            //The window is created on the plugin init thread, which is not the WPF UI
            //thread, so MainWindow must be accessed through the application dispatcher.
            Main.Dispatcher().Invoke(() => {
                if (!m_MessageHookInited) {
                    var mainWindow = System.Windows.Application.Current.MainWindow;
                    if (null != mainWindow) {
                        var hwndSource = PresentationSource.FromVisual(mainWindow) as HwndSource;
                        if (null != hwndSource) {
                            hwndSource.AddHook(new HwndSourceHook(Main.WndProc));
                            m_MessageHookInited = true;
                        }
                    }
                }
            });
        }
        base.WndProc(ref m);
    }

    private AutoResetEvent m_EverythingQueryEvent = null;
    private bool m_CheckReply = false;
    private bool m_MessageHookInited = false;
    private IntPtr m_Handle = IntPtr.Zero;

    [DllImport("user32.dll")]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    private const int MY_REPLY_ID = 0;
    private const int WAIT_TIME_OUT = 10000;
}

internal sealed class MaxArtificialScoreExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        BoxedValue r = Main.c_MaxArtificialScore;
        return r;
    }
}
internal sealed class MainThreadSyncExecutionTypeExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        BoxedValue r = 0;
        if (operands.Count >= 1) {
            int v = operands[0].GetInt();
            Main.s_MainThreadSyncExecutionType = (Main.MainThreadSyncExecutionTypeEnum)v;
            r = v;
        }
        return r;
    }
}
// Invisible topmost owner for WinForms common dialogs: never takes activation itself.
internal sealed class DialogOwnerForm : System.Windows.Forms.Form
{
    protected override bool ShowWithoutActivation
    {
        get { return true; }
    }
}
internal sealed class FolderDialogExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        string ret = string.Empty;
        if (operands.Count >= 1) {
            string desc = operands[0].AsString;
            Environment.SpecialFolder rootDir = Environment.SpecialFolder.MyComputer;
            bool showNew = false;
            if (operands.Count >= 2) {
                rootDir = (Environment.SpecialFolder)operands[1].GetInt();
            }
            if (operands.Count >= 3) {
                showNew = operands[2].GetBool();
            }

            Main.SyncExecuteOnMainThread(() => {
                var fbd = new System.Windows.Forms.FolderBrowserDialog();
                fbd.Description = desc;
                fbd.RootFolder = rootDir;
                fbd.ShowNewFolderButton = showNew;
                bool hideOrig = Main.SuppressHideWhenDeactivated();
                try {
                    var result = fbd.ShowDialog(Main.GetDialogOwner());
                    if (result == DialogResult.OK) {
                        ret = fbd.SelectedPath;
                    }
                    Main.LogLine("folderdlg returns {0}, path {1}", result, ret);
                }
                finally {
                    Main.RestoreHideWhenDeactivated(hideOrig);
                }
            });
        }
        return BoxedValue.FromString(ret);
    }
}
internal sealed class OpenFileDialogExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        string ret = string.Empty;
        if (operands.Count >= 3) {
            string title = operands[0].AsString;
            string initDir = operands[1].AsString;
            string filter = operands[2].AsString;
            int filterIndex = 0;
            if (operands.Count >= 4) {
                filterIndex = operands[3].GetInt();
            }

            Main.SyncExecuteOnMainThread(() => {
                var ofd = new System.Windows.Forms.OpenFileDialog();
                ofd.Title = title;
                ofd.InitialDirectory = initDir;
                ofd.Filter = filter;
                ofd.FilterIndex = filterIndex;
                ofd.RestoreDirectory = true;
                ofd.Multiselect = false;
                bool hideOrig = Main.SuppressHideWhenDeactivated();
                try {
                    var result = ofd.ShowDialog(Main.GetDialogOwner());
                    if (result == DialogResult.OK) {
                        ret = ofd.FileName;
                    }
                    Main.LogLine("openfiledlg returns {0}, file {1}", result, ret);
                }
                finally {
                    Main.RestoreHideWhenDeactivated(hideOrig);
                }
            });
        }
        return BoxedValue.FromString(ret);
    }
}
internal sealed class OpenFilesDialogExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        string[] ret = null;
        if (operands.Count >= 3) {
            string title = operands[0].AsString;
            string initDir = operands[1].AsString;
            string filter = operands[2].AsString;
            int filterIndex = 0;
            if (operands.Count >= 4) {
                filterIndex = operands[3].GetInt();
            }

            Main.SyncExecuteOnMainThread(() => {
                var ofd = new System.Windows.Forms.OpenFileDialog();
                ofd.Title = title;
                ofd.InitialDirectory = initDir;
                ofd.Filter = filter;
                ofd.FilterIndex = filterIndex;
                ofd.RestoreDirectory = true;
                ofd.Multiselect = true;
                bool hideOrig = Main.SuppressHideWhenDeactivated();
                try {
                    var result = ofd.ShowDialog(Main.GetDialogOwner());
                    if (result == DialogResult.OK) {
                        ret = ofd.FileNames;
                    }
                    Main.LogLine("openfilesdlg returns {0}, files {1}", result, null == ret ? string.Empty : string.Join("|", ret));
                }
                finally {
                    Main.RestoreHideWhenDeactivated(hideOrig);
                }
            });
        }
        if (null == ret) {
            ret = new string[0];
        }
        return BoxedValue.FromObject(ret);
    }
}
internal sealed class SaveFileDialogExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        string ret = string.Empty;
        if (operands.Count >= 3) {
            string title = operands[0].AsString;
            string initDir = operands[1].AsString;
            string filter = operands[2].AsString;
            int filterIndex = 0;
            if (operands.Count >= 4) {
                filterIndex = operands[3].GetInt();
            }

            Main.SyncExecuteOnMainThread(() => {
                var sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.Title = title;
                sfd.InitialDirectory = initDir;
                sfd.Filter = filter;
                sfd.FilterIndex = filterIndex;
                sfd.SupportMultiDottedExtensions = true;
                sfd.RestoreDirectory = true;
                bool hideOrig = Main.SuppressHideWhenDeactivated();
                try {
                    var result = sfd.ShowDialog(Main.GetDialogOwner());
                    if (result == DialogResult.OK) {
                        ret = sfd.FileName;
                    }
                    Main.LogLine("savefiledlg returns {0}, file {1}", result, ret);
                }
                finally {
                    Main.RestoreHideWhenDeactivated(hideOrig);
                }
            });
        }
        return BoxedValue.FromString(ret);
    }
}
internal sealed class ShellContextMenuExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        return BoxedValue.FromObject(Main.s_ShellCtxMenu);
    }
}
internal sealed class ContextExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        return BoxedValue.FromObject(Main.s_Context);
    }
}
internal sealed class ApiExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        return BoxedValue.FromObject(Main.s_Context.API);
    }
}
internal sealed class MetadataExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        return BoxedValue.FromObject(Main.s_Context.CurrentPluginMetadata);
    }
}
internal sealed class ShowMsgExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        if (operands.Count > 0) {
            string title = operands[0].ToString();
            string subtitle = operands.Count > 1 ? operands[1].ToString() : string.Empty;
            string icon = operands.Count > 2 ? operands[2].ToString() : Main.c_IcoPath;

            if (null != Main.s_CurFuncs) {
                //Switch to the main thread for execution (it seems that requests triggered by clicking on the list are always initiated from the main thread).
                //execute after dsl (see WaitAndGetResult)
                Main.s_CurFuncs.Enqueue(() => {
                    Main.s_Context.API.ShowMsg(title, subtitle, icon);
                    return false;
                });
            }
            else {
                Main.s_Context.API.ShowMsg(title, subtitle, icon);
            }
        }
        return BoxedValue.NullObject;
    }
}
internal sealed class RestartExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        if (null != Main.s_CurFuncs) {
            //The RestarApp method needs to be executed on the main thread (it seems that
            //all requests triggered by clicking the list are initiated from the main thread).
            //execute after dsl (see WaitAndGetResult)
            Main.s_CurFuncs.Enqueue(() => {
                Main.s_Context.API.RestartApp();
                return false;
            });
        }
        return BoxedValue.NullObject;
    }
}
internal sealed class ShowConsoleExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        BoxedValue r = BoxedValue.NullObject;
        Main.ShowConsole();
        return r;
    }
}
internal sealed class HideConsoleExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        BoxedValue r = BoxedValue.NullObject;
        Main.HideConsole();
        return r;
    }
}
internal sealed class ReloadDslExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        Main.s_NeedReload = true;
        return BoxedValue.NullObject;
    }
}
internal sealed class EvalDslExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        BoxedValue r = BoxedValue.NullObject;
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
                    Main.CheckDslError();
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
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        if (operands.Count >= 2) {
            string queryStr = operands[0].AsString;
            bool requery = operands[1].GetBool();

            if (null != Main.s_CurFuncs) {
                //Switch to the main thread for execution (it seems that requests triggered by clicking on the list are always initiated from the main thread).
                //execute after dsl (see WaitAndGetResult)
                Main.s_CurFuncs.Enqueue(() => {
                    Main.s_Context.API.ChangeQuery(queryStr, requery);
                    return false;
                });
            }
            else {
                Main.s_Context.API.ChangeQuery(queryStr, requery);
            }
        }
        return BoxedValue.NullObject;
    }
}
internal sealed class AddResultExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        if (operands.Count >= 6) {
            string title = operands[0].AsString;
            string subTitle = operands[1].AsString;
            string icoPath = operands[2].AsString;
            string action = operands[3].AsString;
            Query query = operands[4].As<Query>();
            string search = operands[5].AsString;

            int score = Main.c_MaxSearchScore - Main.s_NewResults.Count;
            if (operands.Count >= 7) {
                score = operands[6].GetInt();
            }
            bool highlight = false;
            if (operands.Count >= 8) {
                highlight = operands[7].GetBool();
            }
            string key = query.ActionKeyword;
            if (key == Query.GlobalPluginWildcardSign)
                key = string.Empty;

            if (string.IsNullOrEmpty(icoPath))
                icoPath = Main.c_IcoPath;

            var sb = new StringBuilder();
            sb.Append(subTitle);
            sb.Append(' ');
            sb.Append('(');
            if (!string.IsNullOrEmpty(search)) {
                sb.Append("search:");
                sb.Append(search);
                sb.Append(' ');
            }
            if (!string.IsNullOrEmpty(key)) {
                sb.Append("key:");
                sb.Append(key);
                sb.Append(' ');
            }
            sb.Append("score:");
            sb.Append(score);
            sb.Append(')');
            string annotation = sb.ToString();

            var item = new Result { Title = title, SubTitle = subTitle, SubTitleToolTip = annotation, IcoPath = icoPath, ContextData = query, Score = score };
            if (null != query) {
                item.ActionKeywordAssigned = key;
            }
            if (highlight) {
                var list = new List<int>();
                for(int i = 0; i < title.Length; ++i) {
                    list.Add(i);
                }
                item.TitleHighlightData = list;
            }
            item.AsyncAction = e => Main.OnActionAsync(action, query, item, e);
            Main.s_NewResults.Add(item);
        }
        return BoxedValue.NullObject;
    }
}
internal sealed class PostResultExp : SimpleExpressionBase{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        if (operands.Count >= 1) {
            Query query = operands[0].As<Query>();
            if (null != query) {
                Main.PostResults(query);
            }
        }
        return BoxedValue.NullObject;
    }
}
internal sealed class AddContextMenuExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
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
            item.AsyncAction = e => Main.OnMenuActionAsync(action, query, result, item, e);
            Main.s_NewContextMenus.Add(item);
        }
        return BoxedValue.NullObject;
    }
}
internal sealed class ActionKeywordRegisteredExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        if (operands.Count >= 1) {
            string keyword = operands[0].AsString;
            return Main.s_Context.API.ActionKeywordAssigned(keyword);
        }
        return false;
    }
}
internal sealed class ClearActionKeywordsExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        if (operands.Count >= 1) {
            string id = operands[0].AsString;
            var plugin = Main.s_Context.API.GetAllPlugins().FirstOrDefault(p => p.Metadata.ID == id);
            if (null != plugin) {
                var keywords = plugin.Metadata.ActionKeywords.ToArray();
                foreach(var keyword in keywords) {
                    Main.s_Context.API.RemoveActionKeyword(id, keyword);
                }
                return true;
            }
        }
        return false;
    }
}
internal sealed class AddActionKeywordExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        if (operands.Count >= 2) {
            string id = operands[0].AsString;
            string keyword = operands[1].AsString;
            if (Main.s_Context.API.ActionKeywordAssigned(keyword)) {
                return false;
            }
            else {
                var plugin = Main.s_Context.API.GetAllPlugins().FirstOrDefault(p => p.Metadata.ID == id);
                if (null != plugin) {
                    if (plugin.Metadata.ActionKeywords.IndexOf(keyword) < 0) {
                        Main.s_Context.API.AddActionKeyword(id, keyword);
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
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        if (operands.Count >= 3) {
            string path = operands[0].AsString;
            bool ctrl = operands[1].GetBool();
            bool shift = operands[2].GetBool();

            Debug.Assert(null != Main.s_CurFuncs);
            //Shell operations are pushed to the thread that initiated the request (it seems that requests triggered by clicking on the list are always initiated from the main thread).
            //execute after dsl (see WaitAndGetResult)
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
internal sealed class ReloadAutoCompleteExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        Main.s_AutoCompletions = Main.LoadAutoComplete(Main.s_Context.CurrentPluginMetadata.PluginDirectory);
        return Main.s_AutoCompletions.Count > 0;
    }
}
internal sealed class GetAutoCompleteExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        if (operands.Count >= 1) {
            string key = operands[0].AsString;
            if(Main.s_AutoCompletions.TryGetValue(key, out var val)) {
                return BoxedValue.From(val);
            }
            else {
                return BoxedValue.From(key);
            }
        }
        return BoxedValue.From(string.Empty);
    }
}
internal sealed class TryFindEverythingExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        Main.TryFindEverything();
        return BoxedValue.From(Main.s_EverythingFullPath);
    }
}
internal sealed class EverythingExistsExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        return BoxedValue.From(EveryThingSDK.EverythingExists());
    }
}
internal sealed class EverythingResetExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        if (EveryThingSDK.EverythingExists()) {
            EveryThingSDK.Everything_Reset();
        }
        return BoxedValue.NullObject;
    }
}
internal sealed class EverythingSetDefaultExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        if (EveryThingSDK.EverythingExists()) {
            EveryThingSDK.Everything_SetMatchPath(false);
            EveryThingSDK.Everything_SetMatchCase(false);
            EveryThingSDK.Everything_SetMatchWholeWord(false);
            EveryThingSDK.Everything_SetRegex(false);
            EveryThingSDK.Everything_SetSort(EveryThingSDK.EVERYTHING_SORT_PATH_ASCENDING);
        }
        return BoxedValue.NullObject;
    }
}
internal sealed class EverythingMatchPathExp : SimpleExpressionBase
{
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
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
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
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
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
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
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
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
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
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
    protected override BoxedValue OnCalc(IList<BoxedValue> operands)
    {
        if (operands.Count >= 1) {
            string str = operands[0].AsString;
            uint offset = 0;
            uint maxCount = Main.c_DefMaxSearchResults;
            if (operands.Count >= 2)
                offset = operands[1].GetUInt();
            if (operands.Count >= 3)
                maxCount = operands[2].GetUInt();
            if (maxCount > Main.c_MaxSearchResults)
                maxCount = Main.c_MaxSearchResults;
            if (null != str) {
                if (EveryThingSDK.EverythingExists()) {
                    EveryThingSDK.Everything_SetSearchW(str);
                    EveryThingSDK.Everything_SetOffset(offset);
                    EveryThingSDK.Everything_SetMax(maxCount);
                    EveryThingSDK.Everything_SetRequestFlags(EveryThingSDK.EVERYTHING_REQUEST_FILE_NAME | EveryThingSDK.EVERYTHING_REQUEST_PATH | EveryThingSDK.EVERYTHING_REQUEST_SIZE | EveryThingSDK.EVERYTHING_REQUEST_DATE_MODIFIED);
                    if (!Main.s_MessageWindow.Query()) {
                        Main.LogLine("everything_search '{0}' no reply from Everything (timeout).", str);
                    }
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
                        return BoxedValue.FromObject(list);
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
        return BoxedValue.FromObject(s_EmptyList);
    }

    private static List<object[]> s_EmptyList = new List<object[]>();
    private const int c_Capacity = 4096;
}