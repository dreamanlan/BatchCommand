using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace BatchCommand.Api
{
    /// <summary>
    /// Shared dsl script host for all dotnet hosts (AgentCore, CefDotnetApp, ...):
    /// per-thread interpreter init + api/doc registration, timestamp based script
    /// hot reload, script/function execution with unified error collection
    /// (api errors + dsl errors, see ApiErrorInfo), result size truncation, and
    /// the call_metadsl_task worker pool.
    ///
    /// One instance per process: the BatchScript interpreter state is [ThreadStatic]
    /// and process wide, so the per-thread init flag and script reload state in
    /// this class are thread static too. Host specific behavior is injected via
    /// the public hook fields before first use. DslHost.Current is set by the
    /// constructor and is used by the shared framework expressions.
    /// </summary>
    public sealed class DslHost
    {
        // ---- host hooks (set before first use) ----

        /// <summary>Host log sink; receives "[csharp] ..." lines.</summary>
        public Action<string>? Log;
        /// <summary>Registers host specific apis; called once per thread after the common apis.</summary>
        public Action? RegisterHostApis;
        /// <summary>Sets host specific dsl globals (e.g. nativeapi, agentport); called on every RefreshGlobalVars.</summary>
        public Action? SetHostGlobalVars;
        /// <summary>Extra help search over api docs (e.g. semantic search); null = plain regex help only.</summary>
        public Action<StringBuilder, SortedList<string, string>, IList<Regex>, HashSet<string>>? SearchHelp;
        /// <summary>Captures per-call state attached to a call_metadsl_task job (e.g. the AgentCore DslContext).</summary>
        public Func<object?>? CaptureTaskState;
        /// <summary>Runs at task start; return false to skip the task entirely (e.g. host shut down).</summary>
        public Func<object?, bool>? OnTaskBegin;
        /// <summary>Runs at task end (even on exception) when OnTaskBegin returned true.</summary>
        public Action<object?>? OnTaskEnd;

        // ---- process info exposed as dsl globals (set before first use) ----
        // Only the truly common fields live here. Host specific process info
        // (processtype/startupurl/... for CEF, agentport/initialprojectidentity
        // for the agent) is owned by the host module and set through the
        // SetHostGlobalVars hook, keeping this class host agnostic.

        public string Name = "dsl";
        public string CmdLine = string.Empty;
        public string BasePath = string.Empty;
        public string AppDir = string.Empty;
        public string DslScriptFile = string.Empty;
        public bool IsMac = false;
        public bool NoSandbox = false;

        /// <summary>The process wide host instance used by the shared framework expressions.</summary>
        public static DslHost? Current { get; private set; }

        // ---- shared services ---------------------------------------------------

        private Utils.FileOperations? s_FileOps;
        private string? s_FileOpsBasePath;

        /// <summary>
        /// The process wide file operations service (Utils.FileOperations: read/
        /// write/search/log-file helpers used by the shared file apis). Lazily
        /// created from the host info and recreated when BasePath changes (e.g.
        /// SetProcessInfo runs after the first access).
        /// </summary>
        public Utils.FileOperations FileOps {
            get {
                if (null == s_FileOps || s_FileOpsBasePath != BasePath) {
                    s_FileOps = new Utils.FileOperations(BasePath, AppDir, IsMac);
                    s_FileOpsBasePath = BasePath;
                }
                return s_FileOps;
            }
        }

        /// <summary>The process wide clipboard service (Utils.ClipboardOperations, TextCopy based, stateless).</summary>
        public Utils.ClipboardOperations ClipboardOps { get; } = new Utils.ClipboardOperations();

        /// <summary>
        /// The process wide global context variable store, used by the shared
        /// set/get_context_var apis (Api/DslContextApi.cs). Agent instances
        /// that need per-instance isolation create their own
        /// Utils.DslContextManagement (see AgentCore.AgentInstance).
        /// </summary>
        public Utils.DslContextManagement DslContextManager { get; } = new Utils.DslContextManagement();

        private static readonly List<string> s_EmptyArgs = new List<string>();

        // Per-thread per-instance init tracking: the interpreter state is
        // process wide [ThreadStatic] (one calculator per thread shared by
        // every DslHost in the process), but EACH instance must run its own
        // Init + api registration on a thread. A second instance re-Inits,
        // which resets the per-thread api registry - the last registration
        // wins, matching the pre-DslHost plugin-host behavior (console host
        // Init followed by the agent plugin Init on the same thread).
        [ThreadStatic]
        private static HashSet<DslHost>? tls_InitedHosts;
        [ThreadStatic]
        private static DateTime tls_ScriptTime;
        [ThreadStatic]
        private static string? tls_ScriptPath;

        public DslHost()
        {
            Current = this;
        }

        /// <summary>Path of the dsl script loaded on the current thread (the dslpath global).</summary>
        public string DslScriptPath {
            get {
                return tls_ScriptPath ?? string.Empty;
            }
        }

        // ---- lifecycle -------------------------------------------------------

        // Initializes the thread's interpreter exactly once: interpreter init,
        // common api docs, BatchScriptApi api registration (see
        // BatchScriptApiRegistrar), then host apis.
        public void Prepare()
        {
            tls_InitedHosts ??= new HashSet<DslHost>();
            if (tls_InitedHosts.Add(this)) {
                BatchScript.Init();
                AddCommonApiDocs();
                BatchScriptApiRegistrar.RegisterAllApis();
                RegisterHostApis?.Invoke();
            }
        }

        // Loads/reloads the dsl script for the current thread (timestamp based
        // hot reload), refreshes globals and clears the api error buffer.
        public void TryLoadDSL()
        {
            Prepare();
            bool loaded = false;
            string path = Path.Combine(BasePath, "managed", DslScriptFile);
            var fi = new FileInfo(path);
            if (fi.Exists) {
                if (fi.LastWriteTime != tls_ScriptTime || tls_ScriptPath != path) {
                    tls_ScriptTime = fi.LastWriteTime;
                    tls_ScriptPath = path;

                    string errorMsg = string.Empty;
                    if (File.Exists(fi.FullName)) {
                        loaded = true;
                        BatchScript.Load(fi.FullName);
                        CheckDslError();
                        Log?.Invoke("[csharp] Load dsl script: " + fi.FullName);
                    }
                    else {
                        errorMsg = "DSL script file does not exist";
                        Log?.Invoke("[csharp] " + errorMsg + ": " + fi.FullName);
                    }
                }
            }
            else if (NoSandbox) {
                Log?.Invoke("[csharp] Can't find dsl script: " + fi.FullName);
            }
            RefreshGlobalVars();
            ApiErrorInfo.Clear();
            if (loaded) {
                // Optional entry point: only call it when the script defines
                // it (e.g. monitor.dsl does not).
                if (BatchScript.Calculator.TryGetFuncInfo("init_global_consts", out _)) {
                    BatchScript.Call("init_global_consts");
                    CheckDslError();
                }
            }
        }

        // Resets the common dsl globals from the host info, lets the host set
        // its own globals, and clears dsl errors.
        public void RefreshGlobalVars()
        {
            //reset global vars
            BatchScript.SetGlobalVariable("commandline", BoxedValue.FromString(CmdLine));
            BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(BasePath));
            BatchScript.SetGlobalVariable("appdir", BoxedValue.FromString(AppDir));
            BatchScript.SetGlobalVariable("ismac", BoxedValue.From(IsMac));
            BatchScript.SetGlobalVariable("nosandbox", BoxedValue.From(NoSandbox));
            BatchScript.SetGlobalVariable("dslpath", BoxedValue.FromString(DslScriptPath));
            BatchScript.SetGlobalVariable("dslfile", BoxedValue.FromString(DslScriptFile));
            SetHostGlobalVars?.Invoke();
            BatchScript.ClearDslErrors();
        }

        // ---- execution -------------------------------------------------------

        // Execute a dsl code snippet and return its text result; on error
        // hasError is set and the error text (api errors + dsl errors, see
        // ApiErrorInfo) is appended to the result after truncation.
        public string ExecuteScript(string script, int maxResultSize, out bool hasError)
        {
            try {
                hasError = false;
                TryLoadDSL();
                var id = BatchScript.EvalAsFunc(script, s_EmptyArgs);
                var resSb = new StringBuilder();
                if (!BatchScript.HasDslErrors) {
                    var result = BatchScript.Call(id);
                    string resultStr;
                    if (result.IsNullObject) {
                        resultStr = "null";
                    }
                    else {
                        resultStr = ResultToString(result);
                    }
                    resSb.AppendLine(resultStr);
                }
                var errSb = new StringBuilder();
                if (ApiErrorInfo.HasInfo) {
                    hasError = true;
                    errSb.AppendLine();
                    errSb.AppendLine(ApiErrorInfo.GetInfo());
                }
                if (BatchScript.HasDslErrors) {
                    hasError = true;
                    errSb.AppendLine();
                    errSb.AppendLine(BatchScript.GetDslErrors());
                }
                return GetMetaDslResult(maxResultSize, resSb, errSb);
            }
            catch (Exception ex) {
                hasError = true;
                Log?.Invoke($"[AgentCommand] Error executing MetaDSL script: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        // Loads/updates a single dsl function from code; returns the dsl error
        // text on failure, string.Empty on success.
        public string LoadFunc(string func, string code, IList<string> paramNames, bool update)
        {
            try {
                Prepare();
                // Execute the script directly using the DSL interpreter
                BatchScript.ClearDslErrors();
                BatchScript.LoadFunc(func, code, paramNames, update);
                if (BatchScript.HasDslErrors) {
                    return BatchScript.GetDslErrors();
                }
                return string.Empty;
            }
            catch (Exception ex) {
                return $"Error: {ex.Message}";
            }
        }

        // ---- full dsl file execution (web pages, filters and the like) -----
        //
        // ExecuteScript wraps the snippet with EvalAsFunc, which cannot define
        // functions; a full dsl file needs BatchScript.Load, and Load CLEARS
        // the thread's calculator (script functions and globals - the same
        // clear every hot reload performs). Running that on the caller's
        // thread would throw away that thread's main script state, so file
        // execution is isolated on dedicated worker threads instead (same
        // idea as the call_metadsl_task pool: the BatchScript interpreter is
        // [ThreadStatic], isolation is per thread). The worker threads never
        // hold main script state - every job loads its own file - so there is
        // nothing to break and nothing to restore.
        //
        // Worker pool (elastic, unlike the sticky call_metadsl_task pool):
        // every worker owns its queue; jobs are routed by workerIndex
        // (>= 0 = that worker - an index past the live count TEMPORARILY
        // grows the pool without touching the configured count, the extra
        // workers retire after being idle for c_DslFileIdleMs; < 0 =
        // automatic, the worker with the shortest queue). The pool never
        // shrinks below DslFileWorkerFloor (the configured count, the host
        // minimum and one live worker). set_dsl_file_worker_count /
        // get_dsl_file_worker_count (dsl) read and write the configured
        // count; lowering it retires idle workers tail first, so queued work
        // always runs.
        //
        // Worker convention of the built-in callers (AgentCore): the proxy /
        // web server FILTER callbacks run on worker 0 (quick, mostly header
        // munging - a slow one degrades through the caller timeout, not by
        // blocking the pool), the web server dsl PAGES run on worker 1 (slow
        // jobs must never occupy the filter worker). Each worker therefore
        // keeps its script loaded without switching.
        //
        // Loaded script caching: every worker caches the script it last
        // loaded (per thread - each worker owns its own calculator) and hot
        // reloads it when the file timestamp changes; a different path
        // simply switches the script. Globals of a cached script therefore
        // SURVIVE between jobs on the same worker thread - scripts must not
        // rely on per-call global state.
        //
        // Convention: the caller decides the entry function name AND the
        // argument list (the web server pages use main(request, result), the
        // proxy / web server filters use on_proxy_request / on_proxy_response
        // / on_webserver_response). The raw entry return value is passed back
        // AS IS (object references included - never a string conversion);
        // errors are reported through hasError/error.
        //
        // timeoutMs: > 0 waits at most that long (on timeout the job is
        // abandoned - fail-open at the caller - and its result is dropped
        // whenever a worker eventually finishes it); < 0 (Timeout.Infinite)
        // waits forever; 0 does not wait at all (fire and forget - the job is
        // queued, the call returns immediately with a null value and no
        // error).
        //
        // Reentrancy note (filters): if a filter script itself calls through
        // the proxy, every worker could be busy, the proxy thread times out
        // and forwards the request unmodified - the call then completes, so
        // the worst case is one timeout delay, not a deadlock. Filter scripts
        // should call upstream sites directly, never through the proxy.

        private sealed class DslFileJob
        {
            public string Path = string.Empty;
            public string Func = string.Empty;
            public List<BoxedValue> Args = new();
            public TaskCompletionSource<(BoxedValue Value, string Error, bool HasError)> Tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private sealed class DslFileWorker
        {
            public readonly BlockingCollection<DslFileJob> Queue = new BlockingCollection<DslFileJob>();
        }

        // Guards _dslFileWorkers and _dslFileWorkerCount only. It is never
        // held while running dsl.
        private readonly object _dslFileLock = new object();
        private readonly List<DslFileWorker> _dslFileWorkers = new List<DslFileWorker>();
        // The minimum worker count a host may not shrink below (hosts that
        // reserve leading workers raise this: AgentCore sets 2 - worker 0 =
        // filter callbacks, worker 1 = web pages, see the convention above).
        // The pool itself always keeps at least one worker alive regardless
        // of this value. Also the default worker index of the
        // execute_dsl_file_in_worker dsl api (scripts start past the reserved
        // workers).
        public static int MinDslFileWorkerCount = 0;
        private int _dslFileWorkerCount = MinDslFileWorkerCount;
        // How long a worker waits for work before checking whether it should retire.
        private const int c_DslFileIdleMs = 30000;
        // Hard upper bound for a workerIndex (protects against typos / abuse;
        // growing is meant for giving one slow job its own thread).
        private const int c_MaxDslFileWorkerIndex = 100;

        // Caller must hold _dslFileLock. The pool never shrinks below this:
        // the configured count, the host minimum (reserved workers) and one
        // live worker.
        private int DslFileWorkerFloor
        {
            get {
                int floor = _dslFileWorkerCount > MinDslFileWorkerCount ? _dslFileWorkerCount : MinDslFileWorkerCount;
                return floor > 1 ? floor : 1;
            }
        }

        // Per worker thread: the script this thread last loaded (+ its write
        // timestamp), so a cached script keeps running without a reload and
        // hot reloads when the file changes.
        [ThreadStatic]
        private static string? tls_LoadedDslPath;
        [ThreadStatic]
        private static DateTime tls_LoadedDslWriteUtc;
        // True while this thread is running a dsl file job (guards against
        // call_dsl_file deadlocking a worker on itself, see ExecuteDslFileInWorker).
        [ThreadStatic]
        private static bool tls_InDslFileWorker;

        /// <summary>True when called from a dsl file worker thread (the worker pool must not wait on itself).</summary>
        public static bool IsDslFileWorkerThread => tls_InDslFileWorker;

        // Executes a full dsl file (function definitions supported) with the
        // given entry function and arguments on the worker threads, and
        // returns the raw entry return value. workerIndex >= 0 routes the
        // job to that worker (temporarily growing the pool past the live
        // count without touching the configured count - the extra workers
        // retire after being idle; indices above c_MaxDslFileWorkerIndex are
        // rejected), < 0 picks the worker with the shortest queue
        // automatically. Blocks the caller per the timeoutMs semantics
        // above.
        public BoxedValue ExecuteDslFileInWorker(string path, string func, IList<BoxedValue>? args, int timeoutMs, int workerIndex, out bool hasError, out string error)
        {
            hasError = false;
            error = string.Empty;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) {
                hasError = true;
                error = "Error: dsl file not found: " + path;
                return BoxedValue.NullObject;
            }
            if (string.IsNullOrEmpty(func)) {
                hasError = true;
                error = "Error: entry function name must not be empty";
                return BoxedValue.NullObject;
            }
            var job = new DslFileJob { Path = path, Func = func };
            if (args != null) {
                job.Args.AddRange(args);
            }
            lock (_dslFileLock) {
                if (workerIndex > c_MaxDslFileWorkerIndex) {
                    hasError = true;
                    error = string.Format("worker index {0} is above the maximum {1}, func: {2}",
                        workerIndex, c_MaxDslFileWorkerIndex, func);
                    return BoxedValue.NullObject;
                }
                // A directed index past the live count grows the pool for
                // this job only - the configured count stays, the extra
                // workers retire once idle.
                int need = DslFileWorkerFloor;
                if (workerIndex >= 0 && workerIndex + 1 > need) {
                    need = workerIndex + 1;
                }
                EnsureDslFileWorkers(need);
                DslFileWorker worker;
                if (workerIndex >= 0) {
                    worker = _dslFileWorkers[workerIndex];
                }
                else {
                    // Automatic: the shortest queue wins, first on ties.
                    worker = _dslFileWorkers[0];
                    int min = worker.Queue.Count;
                    for (int i = 1; i < _dslFileWorkers.Count; i++) {
                        int count = _dslFileWorkers[i].Queue.Count;
                        if (count < min) {
                            min = count;
                            worker = _dslFileWorkers[i];
                        }
                    }
                }
                worker.Queue.Add(job);
            }
            if (timeoutMs == 0) {
                // Fire and forget: a worker runs the job whenever it gets
                // there, the result is dropped.
                return BoxedValue.NullObject;
            }
            if (!job.Tcs.Task.Wait(timeoutMs < 0 ? Timeout.Infinite : timeoutMs)) {
                hasError = true;
                error = "Error: dsl callback timeout: " + func;
                return BoxedValue.NullObject;
            }
            var (value, errText, err) = job.Tcs.Task.Result;
            hasError = err;
            error = errText;
            return value;
        }

        // Caller must hold _dslFileLock. Grows the pool to at least num
        // workers and returns the live count.
        private int EnsureDslFileWorkers(int num)
        {
            while (_dslFileWorkers.Count < num) {
                int index = _dslFileWorkers.Count;
                var worker = new DslFileWorker();
                var thread = new Thread(() => DslWorkerLoop(worker)) {
                    IsBackground = true,
                    Name = "dsl_worker_" + index,
                };
                _dslFileWorkers.Add(worker);
                thread.Start();
            }
            return _dslFileWorkers.Count;
        }

        // Worker loop. Waits for work, and once it has been idle for a while
        // it retires itself if set_dsl_file_worker_count has since lowered
        // the count below the live number.
        //
        // Only the LAST worker may retire, which is what keeps workerIndex
        // meaningful: an index IS a position in _dslFileWorkers, so removing
        // from the middle would silently renumber every worker above it.
        // Shrinking therefore peels off the tail, and an idle worker in the
        // middle retires once the ones after it are gone.
        private void DslWorkerLoop(DslFileWorker worker)
        {
            while (true) {
                if (worker.Queue.TryTake(out var job, c_DslFileIdleMs)) {
                    RunDslFileJob(job);
                    continue;
                }
                // Idle. Decide under the lock so this cannot interleave with
                // a producer picking this worker and queueing to it. The
                // queue is re-checked here because work may have arrived
                // since TryTake gave up.
                lock (_dslFileLock) {
                    int last = _dslFileWorkers.Count - 1;
                    if (_dslFileWorkers.Count > DslFileWorkerFloor
                        && last >= 0
                        && _dslFileWorkers[last] == worker
                        && worker.Queue.Count == 0) {
                        _dslFileWorkers.RemoveAt(last);
                        // Nothing can reach this worker any more: producers
                        // only read the list while holding the lock this
                        // thread is holding right now.
                        worker.Queue.Dispose();
                        return;
                    }
                }
            }
        }

        private void RunDslFileJob(DslFileJob job)
        {
            var value = BoxedValue.NullObject;
            var errSb = new StringBuilder();
            bool hasError = false;
            tls_InDslFileWorker = true;
            try {
                // Registers apis on this thread once (Prepare is idempotent);
                // this thread never loads the main script - every job loads
                // its own file below.
                Prepare();
                if (!LoadScriptCached(job.Path)) {
                    hasError = true;
                    errSb.AppendLine("Error: dsl file failed to load: " + job.Path);
                    if (BatchScript.HasDslErrors) {
                        errSb.AppendLine(BatchScript.GetDslErrors());
                    }
                }
                else if (!BatchScript.Calculator.TryGetFuncInfo(job.Func, out _)) {
                    hasError = true;
                    errSb.AppendLine("dsl file must define a '" + job.Func + "' entry function: " + job.Path);
                }
                else {
                    // BatchScript.Load cleared the globals: re-establish the
                    // common + host ones, with dslpath pointing at the
                    // executed file, before running the entry.
                    tls_ScriptPath = job.Path;
                    RefreshGlobalVars();
                    ApiErrorInfo.Clear();
                    BatchScript.ClearDslErrors();
                    // The return value is passed through AS IS (object
                    // references included): the caller decides how to
                    // interpret it, never a string conversion.
                    value = BatchScript.Call(job.Func, job.Args);
                    if (ApiErrorInfo.HasInfo) {
                        hasError = true;
                        errSb.AppendLine(ApiErrorInfo.GetInfo());
                    }
                    if (BatchScript.HasDslErrors) {
                        hasError = true;
                        errSb.AppendLine(BatchScript.GetDslErrors());
                    }
                }
            }
            catch (Exception ex) {
                hasError = true;
                errSb.AppendLine("Error: " + ex.Message);
            }
            finally {
                tls_InDslFileWorker = false;
            }
            job.Tcs.SetResult((value, errSb.ToString().TrimEnd(), hasError));
        }

        // Loads the dsl file on this worker thread when the path differs
        // from the one this thread last loaded or its file timestamp changed
        // (hot reload). Returns false on a load error (the thread's loaded
        // path is reset so the next job retries).
        private static bool LoadScriptCached(string path)
        {
            try {
                DateTime stamp = File.GetLastWriteTimeUtc(path);
                if (!string.Equals(path, tls_LoadedDslPath, StringComparison.OrdinalIgnoreCase) || stamp != tls_LoadedDslWriteUtc) {
                    BatchScript.Load(path);
                    if (BatchScript.HasDslErrors) {
                        tls_LoadedDslPath = null;
                        return false;
                    }
                    tls_LoadedDslPath = path;
                    tls_LoadedDslWriteUtc = stamp;
                }
                return true;
            }
            catch {
                tls_LoadedDslPath = null;
                return false;
            }
        }

        // Sets the configured worker count and returns the live count
        // afterwards. Raising it creates the missing workers at once.
        // It never goes below MinDslFileWorkerCount (host reserved workers);
        // the pool itself always keeps DslFileWorkerFloor workers alive
        // (configured count, host minimum, one live worker), and lowering
        // kills nothing immediately: each worker above the floor retires on
        // its own once it has been idle for c_DslFileIdleMs, tail first, so
        // queued work always still runs. That means the returned count can
        // be larger than num until the extra workers go idle.
        public int SetDslFileWorkerCount(int num)
        {
            if (num < MinDslFileWorkerCount) {
                num = MinDslFileWorkerCount;
            }
            lock (_dslFileLock) {
                _dslFileWorkerCount = num;
                return EnsureDslFileWorkers(DslFileWorkerFloor);
            }
        }

        public int GetDslFileWorkerCount()
        {
            lock (_dslFileLock) {
                return _dslFileWorkers.Count;
            }
        }

        public void CheckDslError()
        {
            if (BatchScript.HasDslErrors) {
                Log?.Invoke("[csharp] Dsl error: " + BatchScript.GetDslErrors());
            }
        }

        // ---- shared static helpers -------------------------------------------

        public static string ResultToString(BoxedValue result)
        {
            var sb = new StringBuilder();
            Utils.DslHelper.ConvertToString(result, sb, 0, true);
            return sb.ToString();
        }

        // Combines the result and error builders, truncating to maxResultSize
        // (0 = unlimited) with a trailing truncation marker.
        public static string GetMetaDslResult(int maxResultSize, StringBuilder resSb, StringBuilder errSb)
        {
            var sb = new StringBuilder();
            if (maxResultSize > 0) {
                if (resSb.Length > maxResultSize) {
                    if (errSb.Length > maxResultSize * 1 / 3) {
                        sb.Append(resSb.ToString(0, maxResultSize * 2 / 3));
                        sb.AppendLine("...");
                        sb.Append(errSb.ToString(0, maxResultSize * 1 / 3));
                        sb.Append("... [truncated, exceeded max result size ");
                        sb.Append(maxResultSize);
                        sb.AppendLine("]");
                    }
                    else {
                        sb.Append(resSb.ToString(0, maxResultSize - errSb.Length));
                        sb.AppendLine("...");
                        sb.Append(errSb.ToString());
                        sb.Append("... [truncated, exceeded max result size ");
                        sb.Append(maxResultSize);
                        sb.AppendLine("]");
                    }
                }
                else {
                    sb.Append(resSb.ToString());
                    if (errSb.Length > maxResultSize - resSb.Length) {
                        sb.AppendLine(errSb.ToString(0, maxResultSize - resSb.Length));
                        sb.Append("... [truncated, exceeded max result size ");
                        sb.Append(maxResultSize);
                        sb.AppendLine("]");
                    }
                    else {
                        sb.Append(errSb.ToString());
                    }
                }
            }
            else {
                sb.Append(resSb.ToString());
                sb.Append(errSb.ToString());
            }
            return sb.ToString();
        }

        public static string GetStringInLength(string str, int len, int beginOrEndOrBeginEnd)
        {
            if (!string.IsNullOrEmpty(str)) {
                if (str.Length <= len) {
                    return str;
                }
                switch (beginOrEndOrBeginEnd) {
                    case 1:
                        return "..." + str.Substring(str.Length - len, len);
                    case 2:
                        return str.Substring(0, len / 2) + "..." + str.Substring(str.Length - len / 2, len / 2);
                    case 0:
                    default:
                        return str.Substring(0, len) + "...";
                }
            }
            return string.Empty;
        }

        // Compact rendering of api operands for error messages.
        public static string GetStringInLength(IList<BoxedValue> args)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var arg in args) {
                if (first) {
                    first = false;
                }
                else {
                    sb.Append('|');
                }
                sb.Append(GetStringInLength(arg.IsString ? arg.AsString : arg.ToString(), 100, 0));
            }
            return sb.ToString();
        }

        public static string QuoteString(string? value)
        {
            if (value == null) value = string.Empty;
            // if numeric, no quotes needed
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                return value;
            // wrap in double quotes, escape internal double quotes and backslashes
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        public static string StripQuotes(string? s)
        {
            if (s == null) return string.Empty;
            if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"')
                return s.Substring(1, s.Length - 2);
            if (s.Length >= 2 && s[0] == '\'' && s[s.Length - 1] == '\'')
                return s.Substring(1, s.Length - 2);
            return s;
        }

        // Parse --switchname=value from raw command line string
        public static bool TryGetSwitchValueFromRawCommandLine(string cmdLine, string switchName, out string switchValue)
        {
            switchValue = string.Empty;
            string prefix = "--" + switchName + "=";
            int idx = cmdLine.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                return false;
            int start = idx + prefix.Length;
            // Handle quoted value
            if (start < cmdLine.Length && cmdLine[start] == '"') {
                int end = cmdLine.IndexOf('"', start + 1);
                switchValue = end > start ? cmdLine.Substring(start + 1, end - start - 1) : string.Empty;
                return true;
            }
            // Unquoted: take until next space
            int spaceIdx = cmdLine.IndexOf(' ', start);
            switchValue = spaceIdx > start ? cmdLine.Substring(start, spaceIdx - start) : cmdLine.Substring(start);
            return true;
        }

        // ---- common api registration -----------------------------------------
        // The actual registrations live in BatchScriptApiRegistrar; Prepare
        // calls BatchScriptApiRegistrar.RegisterAllApis() per thread.

        private static void AddCommonApiDocs()
        {
            BatchScript.AddUserApiDoc("clone", "clone(list_or_hashtable) api");
            BatchScript.AddUserApiDoc("args", "args() api");
            BatchScript.AddUserApiDoc("arg", "arg(ix) api");
            BatchScript.AddUserApiDoc("argnum", "argnum() api");
            BatchScript.AddUserApiDoc("inc", "inc(var) or inc(var,val) api");
            BatchScript.AddUserApiDoc("dec", "dec(var) or dec(var,val) api");
            BatchScript.AddUserApiDoc("+", "add operator");
            BatchScript.AddUserApiDoc("-", "sub operator");
            BatchScript.AddUserApiDoc("*", "mul operator");
            BatchScript.AddUserApiDoc("/", "div operator");
            BatchScript.AddUserApiDoc("%", "mod operator");
            BatchScript.AddUserApiDoc("&", "bitand operator");
            BatchScript.AddUserApiDoc("|", "bitor operator");
            BatchScript.AddUserApiDoc("^", "bitxor operator");
            BatchScript.AddUserApiDoc("~", "bitnot operator");
            BatchScript.AddUserApiDoc("<<", "left shift operator");
            BatchScript.AddUserApiDoc(">>", "right shift operator");
            BatchScript.AddUserApiDoc(">", "great operator");
            BatchScript.AddUserApiDoc(">=", "great equal operator");
            BatchScript.AddUserApiDoc("<", "less operator");
            BatchScript.AddUserApiDoc("<=", "less equal operator");
            BatchScript.AddUserApiDoc("==", "equal operator");
            BatchScript.AddUserApiDoc("!=", "not equal operator");
            BatchScript.AddUserApiDoc("&&", "logical and operator");
            BatchScript.AddUserApiDoc("||", "logical or operator");
            BatchScript.AddUserApiDoc("!", "logical not operator");
            BatchScript.AddUserApiDoc("?", "conditional expression");
            BatchScript.AddUserApiDoc("if", "if(cond)func(args); or if(cond){...}[elseif/elif(cond){...}else{...}]; statement");
            BatchScript.AddUserApiDoc("while", "while(cond)func(args); or while(cond){...}; statement, iterator is $$");
            BatchScript.AddUserApiDoc("loop", "loop(ct)func(args); or loop(ct){...}; statement, iterator is $$");
            BatchScript.AddUserApiDoc("looplist", "looplist(list)func(args); or looplist(list){...}; statement, iterator is $$");
            BatchScript.AddUserApiDoc("foreachvalue", "foreachvalue(arg1,arg2,...)func(args); or foreachvalue(arg1,arg2,...){...}; statement, iterator is $$");
            BatchScript.AddUserApiDoc("return", "return([val]) api");
            BatchScript.AddUserApiDoc("dotnetcall", "dotnetcall api, internal implementation, using csharp object syntax");
            BatchScript.AddUserApiDoc("dotnetset", "dotnetset api, internal implementation, using csharp object syntax");
            BatchScript.AddUserApiDoc("dotnetget", "dotnetget api, internal implementation, using csharp object syntax");
            BatchScript.AddUserApiDoc("collectioncall", "collectioncall api, internal implementation, using csharp object syntax");
            BatchScript.AddUserApiDoc("collectionset", "collectionset api, internal implementation, using csharp object syntax");
            BatchScript.AddUserApiDoc("collectionget", "collectionget api, internal implementation, using csharp object syntax");
            BatchScript.AddUserApiDoc("linq", "linq(list,method,arg1,arg2,...) statement, internal implementation, using obj.method(arg1,arg2,...) syntax, method can be where/filter/select/map/top/take/skip/distinct/concat/groupby/orderby/orderbydesc/aggregate/reduce/any/all/count/first/last/tolist/sum/min/max/average, iterator is $$ (An additional iterator $$acc for aggregate/reduce operators)");
            BatchScript.AddUserApiDoc("null", "null() api");
            BatchScript.AddUserApiDoc("propset", "propset(varname,val) - set variable");
            BatchScript.AddUserApiDoc("propget", "propget(varname[,defval]) - get variable");
            BatchScript.AddUserApiDoc("propexists", "propexists(varname) - check variable");
            BatchScript.AddUserApiDoc("max", "max(v1,v2)");
            BatchScript.AddUserApiDoc("min", "min(v1,v2)");
            BatchScript.AddUserApiDoc("abs", "abs(v)");
            BatchScript.AddUserApiDoc("sin", "sin(v)");
            BatchScript.AddUserApiDoc("cos", "cos(v)");
            BatchScript.AddUserApiDoc("tan", "tan(v)");
            BatchScript.AddUserApiDoc("asin", "asin(v)");
            BatchScript.AddUserApiDoc("acos", "acos(v)");
            BatchScript.AddUserApiDoc("atan", "atan(v)");
            BatchScript.AddUserApiDoc("atan2", "atan2(v1,v2)");
            BatchScript.AddUserApiDoc("sinh", "sinh(v)");
            BatchScript.AddUserApiDoc("cosh", "cosh(v)");
            BatchScript.AddUserApiDoc("tanh", "tanh(v)");
            BatchScript.AddUserApiDoc("pow", "pow(v1,v2)");
            BatchScript.AddUserApiDoc("sqrt", "sqrt(v)");
            BatchScript.AddUserApiDoc("exp", "exp(v)");
            BatchScript.AddUserApiDoc("exp2", "exp2(v)");
            BatchScript.AddUserApiDoc("log", "log(v)");
            BatchScript.AddUserApiDoc("log2", "log2(v)");
            BatchScript.AddUserApiDoc("log10", "log10(v)");
            BatchScript.AddUserApiDoc("floor", "floor(v)");
            BatchScript.AddUserApiDoc("ceiling", "ceiling(v)");
            BatchScript.AddUserApiDoc("round", "round(v)");
            BatchScript.AddUserApiDoc("bool", "bool(v)");
            BatchScript.AddUserApiDoc("sbyte", "sbyte(v)");
            BatchScript.AddUserApiDoc("byte", "byte(v)");
            BatchScript.AddUserApiDoc("char", "char(v)");
            BatchScript.AddUserApiDoc("short", "short(v)");
            BatchScript.AddUserApiDoc("ushort", "ushort(v)");
            BatchScript.AddUserApiDoc("int", "int(v)");
            BatchScript.AddUserApiDoc("uint", "uint(v)");
            BatchScript.AddUserApiDoc("long", "long(v)");
            BatchScript.AddUserApiDoc("ulong", "ulong(v)");
            BatchScript.AddUserApiDoc("float", "float(v)");
            BatchScript.AddUserApiDoc("double", "double(v)");
            BatchScript.AddUserApiDoc("decimal", "decimal(v)");
            BatchScript.AddUserApiDoc("datetime", "datetime(v) api");
            BatchScript.AddUserApiDoc("isobject", "isobject(v)");
            BatchScript.AddUserApiDoc("isstring", "isstring(v)");
            BatchScript.AddUserApiDoc("isboolean", "isboolean(v)");
            BatchScript.AddUserApiDoc("ischar", "ischar(v)");
            BatchScript.AddUserApiDoc("isinteger", "isinteger(v)");
            BatchScript.AddUserApiDoc("issignedinteger", "issignedinteger(v)");
            BatchScript.AddUserApiDoc("isunsignedinteger", "isunsignedinteger(v)");
            BatchScript.AddUserApiDoc("isnumber", "isnumber(v)");
            BatchScript.AddUserApiDoc("isdatetime", "isdatetime(v)");
            BatchScript.AddUserApiDoc("istuple", "istuple(v)");
            BatchScript.AddUserApiDoc("boxedvaluetype", "boxedvaluetype(v)");
            BatchScript.AddUserApiDoc("boxedvaluetypename", "boxedvaluetypename(v)");
            BatchScript.AddUserApiDoc("ftoi", "ftoi(v) api");
            BatchScript.AddUserApiDoc("itof", "itof(v) api");
            BatchScript.AddUserApiDoc("ftou", "ftou(v) api");
            BatchScript.AddUserApiDoc("utof", "utof(v) api");
            BatchScript.AddUserApiDoc("dtol", "dtol(v) api");
            BatchScript.AddUserApiDoc("ltod", "ltod(v) api");
            BatchScript.AddUserApiDoc("dtou", "dtou(v) api");
            BatchScript.AddUserApiDoc("utod", "utod(v) api");
            BatchScript.AddUserApiDoc("lerp", "lerp(a,b,t)");
            BatchScript.AddUserApiDoc("clamp01", "clamp01(v)");
            BatchScript.AddUserApiDoc("clamp", "clamp(v,v1,v2)");
            BatchScript.AddUserApiDoc("approximately", "approximately(v1,v2)");
            BatchScript.AddUserApiDoc("format", "format(fmt,arg1,arg2,...)");
            BatchScript.AddUserApiDoc("tuple", "(v1,v2,...) or tuple(v1,v2,...) object");
            BatchScript.AddUserApiDoc("array", "[v1,v2,...] or array(v1,v2,...) object");
            BatchScript.AddUserApiDoc("list", "list(v1,v2,...) object");
            BatchScript.AddUserApiDoc("hashtable", "{k1=>v1,k2=>v2,...} or {k1:v1,k2:v2,...} or hashtable(k1=>v1,k2=>v2,...) or hashtable(k1:v1,k2:v2,...) object");
            BatchScript.AddUserApiDoc("peek", "peek(queue_or_stack)");
            BatchScript.AddUserApiDoc("stack", "stack(v1,v2,...) object");
            BatchScript.AddUserApiDoc("push", "push(stack,v)");
            BatchScript.AddUserApiDoc("pop", "pop(stack)");
            BatchScript.AddUserApiDoc("queue", "queue(v1,v2,...) object");
            BatchScript.AddUserApiDoc("enqueue", "enqueue(queue,v)");
            BatchScript.AddUserApiDoc("dequeue", "dequeue(queue)");
            BatchScript.AddUserApiDoc("expand", "expand(str)");
            BatchScript.AddUserApiDoc("envs", "envs()");
            BatchScript.AddUserApiDoc("cd", "cd(path)");
            BatchScript.AddUserApiDoc("pwd", "pwd()");
            BatchScript.AddUserApiDoc("os", "os()");
            BatchScript.AddUserApiDoc("echo", "echo(fmt,arg1,arg2,...) api, Console.WriteLine");
            BatchScript.AddUserApiDoc("calcmd5", "calcmd5(file) api");
            BatchScript.AddUserApiDoc("pid", "pid() api");
            BatchScript.AddUserApiDoc("sleep", "sleep(milliseconds) api");
            BatchScript.AddUserApiDoc("now", "now() api");
            BatchScript.AddUserApiDoc("isnullorempty", "isnullorempty(str) api");
            BatchScript.AddUserApiDoc("time", "time() or timestamp() api, return milliseconds since startup");
            BatchScript.AddUserApiDoc("timestamp", "time() or timestamp() api, return milliseconds since startup");
            BatchScript.AddUserApiDoc("getelapsedms", "getelapsedms() api, return elapsed milliseconds (time)");
            BatchScript.AddUserApiDoc("getelapsedus", "getelapsedus() api, return elapsed microseconds (time)");
        }

        // ---- call_metadsl_task worker pool -----------------------------------

        // A worker thread of the call_metadsl_task pool. Long lived on purpose: BatchScript
        // state is [ThreadStatic], so TryLoadDSL builds a full interpreter (Init + Load +
        // init_global_consts) the first time any thread runs a task. Reusing a fixed set of
        // threads pays that cost once per worker instead of once per task, and serializing
        // the work queued to a worker keeps several tasks from fighting over sqlite's
        // single writer lock.
        private sealed class MetaDslTaskWorker
        {
            public readonly BlockingCollection<Tuple<string, List<BoxedValue>, object?>> Queue
                = new BlockingCollection<Tuple<string, List<BoxedValue>, object?>>();
        }
        // Guards s_MetaDslTaskWorkers and s_MetaDslTaskNum only. It is never held while running dsl.
        private static readonly object s_MetaDslTaskLock = new object();
        private static readonly List<MetaDslTaskWorker> s_MetaDslTaskWorkers = new List<MetaDslTaskWorker>();
        private static int s_MetaDslTaskNum = 2;
        // How long a worker waits for work before checking whether it should retire.
        private const int c_MetaDslTaskIdleMs = 30000;
        // How far past the default num an index may reach in one call. Growing is meant for
        // giving one slow job its own thread, so a jump larger than this is almost always a
        // typo, and acting on it would spawn that many permanent threads.
        private const int c_MetaDslTaskIndexSlack = 10;

        // Caller must hold s_MetaDslTaskLock. Grows the pool to at least num workers.
        private int EnsureMetaDslTaskWorkers(int num)
        {
            while (s_MetaDslTaskWorkers.Count < num) {
                int index = s_MetaDslTaskWorkers.Count;
                var worker = new MetaDslTaskWorker();
                var thread = new Thread(() => MetaDslTaskLoop(worker));
                // Background so a worker waiting on its queue cannot keep the process alive.
                thread.IsBackground = true;
                thread.Name = "metadsl_task_" + index;
                s_MetaDslTaskWorkers.Add(worker);
                thread.Start();
            }
            return s_MetaDslTaskWorkers.Count;
        }
        // Worker loop. Waits for work, and once it has been idle for a while it retires
        // itself if set_metadsl_task_num has since lowered the default below the live count.
        //
        // Only the LAST worker may retire, which is what keeps task_index meaningful: an
        // index IS a position in s_MetaDslTaskWorkers, so removing from the middle would
        // silently renumber every worker above it. Shrinking therefore peels off the tail,
        // and an idle worker in the middle retires once the ones after it are gone.
        private void MetaDslTaskLoop(MetaDslTaskWorker worker)
        {
            while (true) {
                if (worker.Queue.TryTake(out var item, c_MetaDslTaskIdleMs)) {
                    OnCallMetaDslTask(item.Item1, item.Item2, item.Item3);
                    continue;
                }
                // Idle. Decide under the lock so this cannot interleave with a producer
                // picking this worker and queueing to it (see EnqueueMetaDslTask). The queue
                // is re-checked here because work may have arrived since TryTake gave up.
                lock (s_MetaDslTaskLock) {
                    int last = s_MetaDslTaskWorkers.Count - 1;
                    if (s_MetaDslTaskWorkers.Count > s_MetaDslTaskNum
                        && last >= 0
                        && s_MetaDslTaskWorkers[last] == worker
                        && worker.Queue.Count == 0) {
                        s_MetaDslTaskWorkers.RemoveAt(last);
                        // Nothing can reach this worker any more: producers only read the
                        // list while holding the lock this thread is holding right now.
                        worker.Queue.Dispose();
                        return;
                    }
                }
            }
        }
        // Queues func_name to worker task_index. An index at or past the default count
        // raises the default to task_index + 1, so a script can give a slow job its own
        // thread just by picking a fresh index. Raising the default (rather than only
        // growing the list) is what makes that thread stick around: a worker above the
        // default retires when it goes idle, which would otherwise throw away the
        // interpreter this index just paid to build. An index more than
        // c_MetaDslTaskIndexSlack past the default is rejected as a typo.
        public bool EnqueueMetaDslTask(int task_index, string func_name, List<BoxedValue> args, object? state)
        {
            if (task_index < 0) {
                task_index = 0;
            }
            try {
                // Add INSIDE the lock: a retiring worker checks its queue under this same
                // lock, so adding outside it would let an item land in the queue of a worker
                // that just exited, where it would never run. This is cheap - the collection
                // is unbounded so Add never blocks, and no dsl code runs here.
                lock (s_MetaDslTaskLock) {
                    if (task_index >= s_MetaDslTaskNum + c_MetaDslTaskIndexSlack) {
                        string err = string.Format("call_metadsl_task: task_index {0} is more than {1} past the current task num {2}, looks like a typo, func:{3}", task_index, c_MetaDslTaskIndexSlack, s_MetaDslTaskNum, func_name);
                        ApiErrorInfo.AppendLine(err);
                        Log?.Invoke("[csharp] " + err);
                        return false;
                    }
                    if (task_index + 1 > s_MetaDslTaskNum) {
                        s_MetaDslTaskNum = task_index + 1;
                    }
                    EnsureMetaDslTaskWorkers(s_MetaDslTaskNum);
                    s_MetaDslTaskWorkers[task_index].Queue.Add(Tuple.Create(func_name, args, state));
                }
                return true;
            }
            catch (Exception ex) {
                Log?.Invoke("[csharp] Exception in EnqueueMetaDslTask:" + ex.Message);
                return false;
            }
        }
        // Sets the default worker count and returns the live count afterwards. Raising it
        // creates the missing workers at once. Lowering it kills nothing immediately: each
        // worker above the new default retires on its own once it has been idle for
        // c_MetaDslTaskIdleMs, tail first, so queued work always still runs. That means the
        // returned count can be larger than num until the extra workers go idle.
        public int SetMetaDslTaskNum(int num)
        {
            if (num < 1) {
                num = 1;
            }
            lock (s_MetaDslTaskLock) {
                s_MetaDslTaskNum = num;
                return EnsureMetaDslTaskWorkers(num);
            }
        }
        public int GetMetaDslTaskNum()
        {
            lock (s_MetaDslTaskLock) {
                return s_MetaDslTaskWorkers.Count;
            }
        }

        // Body of a call_metadsl_task job. Runs on a pool worker thread, so slow work here
        // (sqlite writes and the like) cannot block the caller thread.
        //
        // Note the interpreter is per thread, so a task only sees globals set up by
        // init_global_consts, NOT variables assigned at runtime on another thread.
        // The args are passed as BoxedValue, not string, so numbers stay numbers and
        // lists stay lists. The optional state travels with the task: hosts use it to
        // route async callbacks (AgentCore DslContext) or reset thread context.
        //
        // Caveat: the values are handed to the worker by reference. Strings and numbers
        // are immutable so they are safe, but if a caller passes a collection it must not
        // mutate it after queueing, because the task may be reading it on another thread.
        // Pass a copy in that case.
        private void OnCallMetaDslTask(string func_name, List<BoxedValue> args, object? state)
        {
            if (null != OnTaskBegin && !OnTaskBegin(state)) {
                return;
            }
            try {
                TryLoadDSL();

                bool funcExists = BatchScript.Calculator.TryGetFuncInfo(func_name, out var finfo);
                var vargs = BatchScript.NewCalculatorValueList();
                try {
                    foreach (var arg in args) {
                        vargs.Add(arg);
                    }
                    if (funcExists) {
                        BatchScript.Call(func_name, vargs);
                    }
                    else {
                        BatchScript.Call("on_call_metadsl_task", BoxedValue.FromString(func_name), BoxedValue.FromObject(vargs));
                    }
                    CheckDslError();
                }
                finally {
                    BatchScript.RecycleCalculatorValueList(vargs);
                }
            }
            catch (Exception ex) {
                // Nothing above us can observe this failure, so never swallow it.
                Log?.Invoke("[csharp] Exception in OnCallMetaDslTask:" + ex.Message + "\n" + ex.StackTrace);
            }
            finally {
                OnTaskEnd?.Invoke(state);
            }
        }
    }
}
