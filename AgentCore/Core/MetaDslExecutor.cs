using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ScriptableFramework;
using DotnetStoryScript.DslExpression;
using BatchCommand.Api;

namespace AgentCore.Core
{
    /// <summary>
    /// AgentCore's dsl host adapter. All script execution machinery (per-thread
    /// interpreter init, timestamp based hot reload, script/func execution,
    /// unified error collection, result truncation, the call_metadsl_task
    /// worker pool and the common framework apis) lives in the shared
    /// BatchCommand.Api.DslHost; this class wires the host up to AgentCore
    /// (logger, agent api set, agentport global, DslContext task routing) and
    /// keeps the thin static facade used across AgentCore.
    /// </summary>
    internal static class MetaDslExecutor
    {
        private static DslHost? s_Host;
        private static int s_AgentPort = 9527;
        private static int s_HttpProxyPort = 9528;
        private static string s_InitialProjectIdentity = string.Empty;

        internal static DslHost Host {
            get {
                if (null == s_Host) {
                    s_Host = new DslHost {
                        Name = "agent",
                        Log = s => AgentCore.Instance.Logger.Info(s),
                        RegisterHostApis = RegisterAgentApis,
                        SetHostGlobalVars = SetAgentGlobalVars,
                        SearchHelp = SearchHelp,
                        CaptureTaskState = () => CurrentContext,
                        OnTaskBegin = state => {
                            // The initiator's context travels with the task: async APIs
                            // started by the task deliver their results to the connection
                            // that queued the task.
                            if (state is DslContext ctx) {
                                PushContext(ctx);
                            }
                            return true;
                        },
                        OnTaskEnd = state => {
                            if (state is DslContext) {
                                PopContext();
                            }
                        },
                    };
                }
                return s_Host;
            }
        }

        // The full agent api set. The shared framework/utility apis are already
        // registered by DslHost.RegisterCommonApis; re-registrations below win.
        private static void RegisterAgentApis()
        {
            ScriptApiRegistrar.RegisterAllApis();
        }

        // Agent specific dsl globals (the common ones come from
        // DslHost.RefreshGlobalVars): agentport, initialprojectidentity and
        // the fixed processtype (-1: the standalone agent is not a CEF
        // process; the former DslHost fields moved back here).
        private static void SetAgentGlobalVars()
        {
            BatchCommand.BatchScript.SetGlobalVariable("agentport", BoxedValue.From(s_AgentPort));
            BatchCommand.BatchScript.SetGlobalVariable("httpproxyport", BoxedValue.From(s_HttpProxyPort));
            BatchCommand.BatchScript.SetGlobalVariable("initialprojectidentity", BoxedValue.FromString(s_InitialProjectIdentity));
            BatchCommand.BatchScript.SetGlobalVariable("processtype", BoxedValue.From(-1));
        }

        // ---- dsl host facade (delegates to the shared DslHost) --------------

        internal static void TryLoadDSL()
        {
            Host.TryLoadDSL();
        }

        internal static void RefreshGlobalVars()
        {
            Host.RefreshGlobalVars();
        }

        internal static void CheckDslError()
        {
            Host.CheckDslError();
        }

        // Execute MetaDSL script; errors (api + dsl) are collected uniformly by
        // the host and appended to the result text (see ApiErrorInfo).
        internal static string ExecuteMetaDslScript(string script, int maxResultSize, out bool hasError)
        {
            return Host.ExecuteScript(script, maxResultSize, out hasError);
        }

        // Executes a full dsl FILE (function definitions supported, main()
        // entry) for web pages - isolated on a dedicated interpreter thread,
        // see DslHost.ExecuteDslFileInWorker.
        internal static string ExecuteDslFileInWorker(string path, out bool hasError)
        {
            return Host.ExecuteDslFileInWorker(path, out hasError);
        }

        internal static string LoadFunc(string func, string code, IList<string> paramNames, bool update)
        {
            return Host.LoadFunc(func, code, paramNames, update);
        }

        internal static string LoadDslFunc(string func, string code, IList<string> paramNames, bool update)
        {
            return Host.LoadFunc(func, code, paramNames, update);
        }

        internal static string CallDslFunc(string func, IList<BoxedValue> args)
        {
            var bvals = BatchCommand.BatchScript.NewCalculatorValueList();
            foreach (var arg in args) {
                bvals.Add(arg);
            }
            var result = BatchCommand.BatchScript.Call(func, bvals);
            BatchCommand.BatchScript.RecycleCalculatorValueList(bvals);
            if (result.IsNullObject) {
                return "null";
            }
            else {
                return DslHost.ResultToString(result);
            }
        }

        internal static string ResultToString(BoxedValue result)
        {
            return DslHost.ResultToString(result);
        }

        internal static string GetMetaDslResult(int maxResultSize, StringBuilder resSb, StringBuilder errSb)
        {
            return DslHost.GetMetaDslResult(maxResultSize, resSb, errSb);
        }

        internal static string GetStringInLength(string str, int len, int beginOrEndOrBeginEnd)
        {
            return DslHost.GetStringInLength(str, len, beginOrEndOrBeginEnd);
        }

        internal static string QuoteString(string? value)
        {
            return DslHost.QuoteString(value);
        }

        internal static string StripQuotes(string? s)
        {
            return DslHost.StripQuotes(s);
        }

        internal static bool TryGetSwitchValueFromRawCommandLine(string cmdLine, string switchName, out string switchValue)
        {
            return DslHost.TryGetSwitchValueFromRawCommandLine(cmdLine, switchName, out switchValue);
        }

        // ---- api error recording (shared thread-static collector) ------------

        internal static void ClearApiErrorInfo()
        {
            ApiErrorInfo.Clear();
        }
        internal static void AppendApiErrorInfo(string msg)
        {
            ApiErrorInfo.Append(msg);
        }
        internal static void AppendApiErrorInfoLine(string msg)
        {
            ApiErrorInfo.AppendLine(msg);
        }
        internal static void AppendApiErrorInfoFormat(string fmt, params object[] args)
        {
            ApiErrorInfo.AppendFormat(fmt, args);
        }
        internal static void AppendApiErrorInfoFormatLine(string fmt, params object[] args)
        {
            ApiErrorInfo.AppendFormatLine(fmt, args);
        }
        internal static bool HasApiErrorInfo {
            get {
                return ApiErrorInfo.HasInfo;
            }
        }
        internal static string GetApiErrorInfo()
        {
            return ApiErrorInfo.GetInfo();
        }

        // ---- process info ----------------------------------------------------

        internal static string CmdLine {
            get {
                return Host.CmdLine;
            }
        }

        internal static string BasePath {
            get {
                return Host.BasePath;
            }
        }

        internal static string AppDir {
            get {
                return Host.AppDir;
            }
        }

        internal static bool IsMac {
            get {
                return Host.IsMac;
            }
        }

        // Called once by the plugin host (main thread) before any dsl
        // execution. Mirrors CefDotnetApp's process info setup.
        internal static void SetProcessInfo(string cmdLine, string basePath, string appDir, bool isMac)
        {
            var host = Host;
            host.CmdLine = cmdLine ?? string.Empty;
            host.BasePath = basePath ?? string.Empty;
            host.AppDir = appDir ?? string.Empty;
            host.IsMac = isMac;
            // The standalone agent process is not a sandboxed cef renderer.
            host.NoSandbox = false;
            host.DslScriptFile = "script_agent.dsl";
            if (DslHost.TryGetSwitchValueFromRawCommandLine(host.CmdLine, "agentscript", out var scriptFile) && !string.IsNullOrEmpty(scriptFile)) {
                host.DslScriptFile = scriptFile;
            }
            // Default relay port 9527 (the original relay port, site ports
            // are retired); overridable with --agentport.
            s_AgentPort = 9527;
            if (DslHost.TryGetSwitchValueFromRawCommandLine(host.CmdLine, "agentport", out var portValue) && int.TryParse(portValue, out int port) && port > 0 && port <= 65535) {
                s_AgentPort = port;
            }
            // Default http proxy port 9528; overridable with --httpproxyport.
            s_HttpProxyPort = 9528;
            if (DslHost.TryGetSwitchValueFromRawCommandLine(host.CmdLine, "httpproxyport", out var proxyPortValue) && int.TryParse(proxyPortValue, out int proxyPort) && proxyPort > 0 && proxyPort <= 65535) {
                s_HttpProxyPort = proxyPort;
            }
            // The browser relays its --projectidentity switch so the agent
            // process knows the initial project identity (exposed to dsl as
            // the initialprojectidentity global via SetAgentGlobalVars).
            if (DslHost.TryGetSwitchValueFromRawCommandLine(host.CmdLine, "projectidentity", out var identityValue) && !string.IsNullOrEmpty(identityValue)) {
                s_InitialProjectIdentity = identityValue;
            }
        }

        // ---- help search -----------------------------------------------------

        internal static void SearchHelp(StringBuilder sb, SortedList<string, string> apiDocs, IList<Regex> regexes, HashSet<string> matchedApiKeys)
        {
            // semantic search over UserApiDocs
            if (regexes.Count > 0) {
                var queries = new List<string>(regexes.Count);
                foreach (var regex in regexes) {
                    string q = CleanStringData(regex.ToString());
                    if (!string.IsNullOrWhiteSpace(q))
                        queries.Add(q);
                }
                var semanticResults = Core.AgentCore.Instance.SemanticSearch(
                    queries,
                    apiDocs.Select(p => (p.Key, p.Key + ": " + p.Value)),
                    5);
                if (semanticResults != null) {
                    foreach (var (key, text, score) in semanticResults) {
                        if (!matchedApiKeys.Contains(key)) {
                            sb.AppendLine(string.Format("{0} ({1})", text, score));
                        }
                    }
                }
                sb.Append(Core.AgentCore.Instance.TakeHelpSearchDebugInfo());
            }
            string infos = Core.AgentCore.Instance.GetSkillHelp(regexes);
            sb.Append(infos);
        }

        /// <summary>
        /// Strip all non-alphanumeric characters from a string to produce clean tokens for semantic search.
        /// Replaces any character that is not a Unicode letter or digit (including CJK punctuation) with a space.
        /// </summary>
        internal static string CleanStringData(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return pattern;
            // Replace any character that is not a Unicode letter (\p{L}) or digit (\p{N}) with space.
            // This covers ASCII punctuation, CJK punctuation, and all other non-word characters.
            string s = Regex.Replace(pattern, @"[^\p{L}\p{N}]", " ");
            return Regex.Replace(s, @" {2,}", " ").Trim();
        }

        // ---- Agent main-thread lifecycle and event queue ------------------
        // The plugin host calls Init/Tick/Shutdown on its main thread. The
        // dsl interpreter is [ThreadStatic], so that thread owns the stateful
        // main interpreter: runtime globals assigned by one callback stay
        // visible to the next one. Worker threads (call_metadsl_task, the
        // websocket worker pool) build their own interpreters and only see
        // init_global_consts state, which keeps the stateless-script design.

        private static readonly ConcurrentQueue<Action> s_AgentEventQueue = new();

        internal static int InitAgent()
        {
            try {
                // Shared websocket client manager hooks (core + wsclient_* apis
                // live in BatchScriptApi, shared with the CefDotnetApp host):
                // log through the agent logger; events are dispatched to the
                // on_wsclient_message / on_wsclient_state dsl callbacks on THIS
                // main thread (drained every tick, see TickAgent).
                BatchCommand.Utils.WebSocketClientManager.Log = s => AgentCore.Instance.Logger.Info(s);
                BatchCommand.Utils.WebSocketClientManager.Dispatch = DispatchWsClientEvent;
                // HostBridge wiring for the shared http/process services
                // (BatchScriptApi): callbacks flow through the agent event
                // queue like every other async result.
                BatchCommand.Utils.HostBridge.Log = s => AgentCore.Instance.Logger.Info(s);
                BatchCommand.Utils.HostBridge.CaptureCallbackContext = () => CurrentContext;
                BatchCommand.Utils.HostBridge.DeliverCallback = (msgName, args, context) => {
                    if (context is DslContext dslCtx) {
                        EnqueueCallback(dslCtx, msgName, args);
                    }
                };
                BatchCommand.Utils.HostBridge.GetTemplateEnvs = () => AgentCore.Instance.SkillMgr.Envs;
                TryLoadDSL();
                return CallOptionalFunc("on_init");
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error("[csharp] InitAgent failed: " + ex.Message + "\n" + ex.StackTrace);
                return 0;
            }
        }

        internal static int TickAgent()
        {
            try {
                TryLoadDSL();
                DrainAgentEvents();
                // Shared wsclient events -> dsl callbacks (main thread).
                BatchCommand.Utils.WebSocketClientManager.DrainQueue(256);
                return CallOptionalFunc("on_tick");
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error("[csharp] TickAgent failed: " + ex.Message + "\n" + ex.StackTrace);
                return 0;
            }
        }

        // Dispatch one queued wsclient event to the dsl callbacks. Called on
        // the agent main thread from WebSocketClientManager.DrainQueue; the
        // callbacks are optional (missing = silently skipped), same semantics
        // as the CefDotnetApp host.
        private static void DispatchWsClientEvent(string id, string kind, string payload)
        {
            try {
                string func = kind == "message" ? "on_wsclient_message" : "on_wsclient_state";
                if (BatchCommand.BatchScript.Calculator.TryGetFuncInfo(func, out _)) {
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    try {
                        vargs.Add(BoxedValue.FromString(id));
                        vargs.Add(BoxedValue.FromString(payload));
                        BatchCommand.BatchScript.Call(func, vargs);
                    }
                    finally {
                        BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    }
                    CheckDslError();
                }
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error("[csharp] wsclient dispatch error (" + kind + "): " + ex.Message);
            }
        }

        internal static int FinalizeAgent()
        {
            try {
                TryLoadDSL();
                var ret = CallOptionalFunc("on_finalize");
                // Close the shared wsclient connections (e.g. the relay server
                // link) on shutdown.
                BatchCommand.Utils.WebSocketClientManager.CloseAll();
                return ret;
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error("[csharp] FinalizeAgent failed: " + ex.Message + "\n" + ex.StackTrace);
                return 0;
            }
        }

        // Calls a dsl function when it exists; missing functions are silently
        // skipped (optional callbacks). Returns the function's int result.
        private static int CallOptionalFunc(string func)
        {
            if (BatchCommand.BatchScript.Calculator.TryGetFuncInfo(func, out _)) {
                var r = BatchCommand.BatchScript.Call(func);
                CheckDslError();
                if (!r.IsNullObject) {
                    return r.GetInt();
                }
            }
            return 0;
        }

        internal static void EnqueueAgentEvent(Action action)
        {
            s_AgentEventQueue.Enqueue(action);
        }

        internal static void DrainAgentEvents()
        {
            while (s_AgentEventQueue.TryDequeue(out var action)) {
                try {
                    action();
                }
                catch (Exception ex) {
                    AgentCore.Instance.Logger.Error("[csharp] Agent event failed: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

        // Queue an agent_call/agent_notify from a websocket client for
        // execution on the main thread. id <= 0 means notify (fire and
        // forget, no reply sent).
        internal static void EnqueueAgentCall(long id, string func, IList<BoxedValue> args, DslContext ctx)
        {
            EnqueueAgentEvent(() => {
                // Run with the requester's context pushed: dsl-level
                // send_command_to_inject / send_response_to_inject inside the
                // handler must target the calling connection (same pattern as
                // EnqueueCallback below), not fall back to a broadcast.
                PushContext(ctx);
                try {
                    bool hasError;
                    string result = CallAgentFunc(func, args, out hasError);
                    if (id > 0) {
                        var payload = new Dictionary<string, object?> {
                            ["type"] = "agent_result",
                            ["id"] = id,
                            ["success"] = !hasError,
                            ["data"] = hasError ? null : result,
                            ["error"] = hasError ? result : string.Empty,
                        };
                        ctx.Send(BatchCommand.Utils.JsonHelper.ToJson(payload));
                    }
                }
                finally {
                    PopContext();
                }
            });
        }

        // Queue an optional main-thread callback carrying only a port
        // (websocket client connected/disconnected notifications).
        internal static void EnqueueAgentPortEvent(string func, int port)
        {
            EnqueueAgentEvent(() => {
                if (BatchCommand.BatchScript.Calculator.TryGetFuncInfo(func, out _)) {
                    var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                    try {
                        vargs.Add(BoxedValue.From(port));
                        BatchCommand.BatchScript.Call(func, vargs);
                        CheckDslError();
                    }
                    finally {
                        BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                    }
                }
            });
        }

        // Runs a dsl function on the calling (main) thread and returns its
        // result string. On error, hasError is set and the error text is
        // returned instead of the result.
        internal static string CallAgentFunc(string func, IList<BoxedValue> args, out bool hasError)
        {
            hasError = false;
            try {
                TryLoadDSL();
                if (!BatchCommand.BatchScript.Calculator.TryGetFuncInfo(func, out _)) {
                    hasError = true;
                    return "no such function: " + func;
                }
                var vargs = BatchCommand.BatchScript.NewCalculatorValueList();
                try {
                    foreach (var arg in args) {
                        vargs.Add(arg);
                    }
                    var result = BatchCommand.BatchScript.Call(func, vargs);
                    string resultStr = result.IsNullObject ? "null" : DslHost.ResultToString(result);
                    if (ApiErrorInfo.HasInfo || BatchCommand.BatchScript.HasDslErrors) {
                        hasError = true;
                        var errSb = new StringBuilder();
                        if (ApiErrorInfo.HasInfo) {
                            errSb.AppendLine(ApiErrorInfo.GetInfo());
                        }
                        if (BatchCommand.BatchScript.HasDslErrors) {
                            errSb.AppendLine(BatchCommand.BatchScript.GetDslErrors());
                        }
                        return errSb.ToString();
                    }
                    return resultStr;
                }
                finally {
                    BatchCommand.BatchScript.RecycleCalculatorValueList(vargs);
                }
            }
            catch (Exception ex) {
                hasError = true;
                AgentCore.Instance.Logger.Error("[csharp] CallAgentFunc (" + func + ") failed: " + ex.Message + "\n" + ex.StackTrace);
                return "Error: " + ex.Message;
            }
        }

        // ---- Async callback delivery ------------------------------------------
        // Async services capture CurrentContext when they initiate work, and on
        // completion enqueue (context, msg, args) onto the agent main-thread event
        // queue. On the main thread, when the script defines handle_<msgName> the dsl
        // function runs with the requester's context pushed (so its own pushes reach
        // the right connection); otherwise the raw agent_callback JSON is sent
        // directly. Items without a context (work initiated outside a request, e.g.
        // from a metadsl task worker) are dropped and logged.

        [ThreadStatic]
        private static Stack<DslContext>? tls_ContextStack;

        internal static DslContext? CurrentContext {
            get {
                var stack = tls_ContextStack;
                return (null != stack && stack.Count > 0) ? stack.Peek() : null;
            }
        }
        internal static void PushContext(DslContext ctx)
        {
            if (null == tls_ContextStack) {
                tls_ContextStack = new Stack<DslContext>();
            }
            tls_ContextStack.Push(ctx);
        }
        internal static void PopContext()
        {
            tls_ContextStack?.Pop();
        }

        internal static void EnqueueCallback(DslContext? ctx, string msgName, IList<BoxedValue> args)
        {
            if (null == ctx) {
                AgentCore.Instance.Logger.Info("[csharp] Drop callback without context: " + msgName);
                return;
            }
            EnqueueAgentEvent(() => {
                string funcName = "handle_" + msgName;
                TryLoadDSL();
                if (BatchCommand.BatchScript.Calculator.TryGetFuncInfo(funcName, out _)) {
                    PushContext(ctx);
                    try {
                        CallAgentFunc(funcName, args, out _);
                    }
                    finally {
                        PopContext();
                    }
                }
                else {
                    try {
                        ctx.Send(FormatCallback(msgName, args));
                    }
                    catch (Exception ex) {
                        AgentCore.Instance.Logger.Error("[csharp] Callback deliver failed (" + msgName + "): " + ex.Message);
                    }
                }
            });
        }

        private static string FormatCallback(string msgName, IList<BoxedValue> args)
        {
            var argArr = new object?[args.Count];
            for (int i = 0; i < args.Count; i++) {
                var v = args[i];
                argArr[i] = v.IsNullObject ? null : (v.IsString ? (object?)v.AsString : v.GetObject() ?? v.ToString());
            }
            var payload = new Dictionary<string, object?> {
                ["type"] = "agent_callback",
                ["msg"] = msgName,
                ["args"] = argArr,
            };
            return BatchCommand.Utils.JsonHelper.ToJson(payload);
        }
    }

    /// <summary>
    /// Delivery context of a MetaDSL execution: how to send a message back to the
    /// originating connection. Created by the host (WebSocketServer) per request and
    /// pushed onto MetaDslExecutor's thread-local stack for the duration of the
    /// execution, so async APIs initiated by the script can route their results back.
    /// </summary>
    internal sealed class DslContext
    {
        internal DslContext(Action<string> send)
        {
            Send = send;
        }
        // Fire-and-forget send to the originating connection; drops and logs when the connection is gone.
        internal Action<string> Send { get; }
    }
}
