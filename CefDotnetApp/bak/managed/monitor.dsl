// Process monitor for webagent.
//
// Loaded by a BARE BatchCmdDslHost.exe (no --plugin, no --script): the console
// host falls back to managed/monitor.dsl (BatchCmdDsl/BatchCmdDsl/Program.cs:295).
// Start it with (from the webagent dir):
//     BatchCmdDslHost.exe
//     BatchCmdDslHost.exe --interval=2000
//
// It owns exactly one webagent instance - the one it launched itself:
//   - alive check: get_process_memory() on the pid returned by launch_process,
//     with a global name search as the fallback (an instance started outside of
//     this monitor is detected but never duplicated: we only ever start one
//     when no webagent process exists at all).
//   - when the browser is gone: run the upgrade (build + deploy) if
//     managed/upgrade_state.json asks for it, then start webagent again with the
//     command line recorded in managed/webagent_cmdline.txt.
//
// on_tick MUST end with return(0): its value is the host main loop return and
// anything non-zero ends this process (same trap as script_agent.dsl).
//
// Not handled yet (the rest of the self-improvement flow lives in script*.dsl):
//   - writing upgrade_state.json (the agent side owns it);
//   - rolling back a failed build (dll backup) - a failed build is logged and
//     webagent is started with whatever is in managed.

script(on_init)
{
    nativelog("[monitor] on_init, monitor pid={0}, basepath={1}", pid(), basepath);

    @DataDir = combine_path(basepath, "managed");
    @WebAgentExe = combine_path(basepath, "webagent.exe");
    @StateFile = combine_path(@DataDir, "upgrade_state.json");
    @CmdLineFile = combine_path(@DataDir, "webagent_cmdline.txt");
    @BuildLog = combine_path(@DataDir, "upgrade_build.log");
    // Used when webagent_cmdline.txt does not exist yet (first run).
    @DefaultCmdLine = " --url=https://evaluation.woa.com/chat --projectidentity=aiclaw --metadsl=script.dsl,script_renderer.dsl --remote-debugging-port=9228 --remote-allow-origins=*";

    @BuildRoot = "d:/GitHub/BatchCommand";
    @BatchCommandExe = "d:/GitHub/BatchCommand/bin/Debug/net9.0/BatchCommand.exe";

    // Ticks between two checks (the host default is 1000ms per tick).
    @CheckEveryTicks = 3;
    // How many checks to wait for the agent host to exit by itself (it watches
    // its parent browser and exits on its own, see script_agent.dsl on_tick).
    @AgentExitChecks = 10;
    @BuildTimeoutMs = 600000;
    @DeployTimeoutMs = 120000;

    set_context_var("monitorTick", 0);
    set_context_var("agentWaitChecks", 0);
};

script(on_finalize)
{
    nativelog("[monitor] on_finalize");
};

script(on_tick)
{
    $n = get_context_var("monitorTick");
    if (isnull($n)) {
        $n = 0;
    };
    $n = $n + 1;
    set_context_var("monitorTick", $n);
    if ($n < @CheckEveryTicks) {
        return(0);
    };
    set_context_var("monitorTick", 0);

    if (webagent_alive()) {
        set_context_var("agentWaitChecks", 0);
        return(0);
    };

    nativelog("[monitor] webagent is not running");

    // ---- wait for the agent host to release managed/AgentCore.dll ----
    // It exits on its own (parent watchdog); only kill it when it does not, and
    // then by pid - a kill by name would hit this monitor too (same exe name).
    $agentIds = search_process("", "--plugin=managed/AgentCore.dll");
    if (listsize($agentIds) > 0) {
        $w = get_context_var("agentWaitChecks");
        if (isnull($w)) {
            $w = 0;
        };
        $w = $w + 1;
        set_context_var("agentWaitChecks", $w);
        if ($w <= @AgentExitChecks) {
            nativelog("[monitor] agent host still alive ({0}), wait for its own exit", $w);
            return(0);
        };
        nativelog("[monitor] agent host did not exit, kill pid={0}", $agentIds[0]);
        kill_process($agentIds[0]);
        return(0);
    };
    set_context_var("agentWaitChecks", 0);

    // ---- upgrade: build + deploy when the manifest asks for it ----
    $phase = upgrade_phase();
    if ($phase == "build_pending" || $phase == "restart_pending") {
        run_upgrade();
    };

    // ---- start the browser again ----
    start_webagent();
    return(0);
};

// Whether a webagent instance is running. The pid we launched is the fast path;
// the name search only detects an instance we did not start (never duplicated).
script(webagent_alive)
{
    $wpid = get_context_var("webagentPid");
    if (!isnull($wpid) && $wpid > 0) {
        if (get_process_memory($wpid) > 0) {
            return(true);
        };
        nativelog("[monitor] our webagent (pid={0}) is gone", $wpid);
        remove_context_var("webagentPid");
    };
    $ids = search_process("webagent");
    if (listsize($ids) > 0) {
        return(true);
    };
    return(false);
};

// The phase recorded in managed/upgrade_state.json ("" when there is none).
script(upgrade_phase)
{
    if (!file_exists(@StateFile)) {
        return("");
    };
    $txt = read_file(@StateFile);
    if (isnullorempty($txt)) {
        return("");
    };
    $v = from_json($txt);
    if (isnull($v)) {
        return("");
    };
    $phase = get_dict_or_json_param($v, "phase");
    if (isnull($phase)) {
        return("");
    };
    return(to_string($phase));
};

// The webagent command line: the recorded one (so the project identity, and
// with it the SQLite memory collections, survive a restart), else the default.
script(read_cmdline)
{
    if (file_exists(@CmdLineFile)) {
        $s = read_file(@CmdLineFile);
        if (!isnullorempty($s)) {
            return($s);
        };
    };
    return(@DefaultCmdLine);
};

script(start_webagent)
{
    $cmdline = read_cmdline();
    $wpid = launch_process(@WebAgentExe, $cmdline, basepath);
    set_context_var("webagentPid", $wpid);
    write_file(@CmdLineFile, $cmdline);
    nativelog("[monitor] launched webagent pid={0} args={1}", $wpid, $cmdline);
};

// Build the targets listed in upgrade_state.json and deploy the results.
// A failed build is logged (upgrade_build.log) and does NOT stop the restart:
// webagent is always started again, with whatever is in managed.
script(run_upgrade)
{
    $txt = read_file(@StateFile);
    $state = from_json($txt);
    $targets = get_dict_or_json_param($state, "targets");
    $count = listsize($targets);
    nativelog("[monitor] upgrade: {0} target(s)", $count);

    $log = format("[monitor] upgrade start {0}", date_time_str());
    write_file(@BuildLog, $log);

    $failed = "";
    loop($count) {
        $i = $$;
        $t = to_string($targets[$i]);
        $csproj = resolve_target($t);
        nativelog("[monitor] build {0}", $csproj);
        $r = execute_command("dotnet", format("build \"{0}\"", $csproj), @BuildRoot, @BuildTimeoutMs);
        $code = get_dict_or_json_param($r, "exitCode");
        $out = to_string(get_dict_or_json_param($r, "output"));
        append_file(@BuildLog, format("=== {0} === exit={1}{2}{3}", $csproj, $code, "\n", $out));
        if ($code != 0) {
            $failed = $csproj;
            nativelog("[monitor] build FAILED: {0} (see {1})", $csproj, @BuildLog);
        };
    };

    if (!isnullorempty($failed)) {
        // Keep the system usable: do not deploy a half built set, just report.
        append_file(@BuildLog, format("[monitor] upgrade aborted after failure: {0}", $failed));
        return(false);
    };

    deploy();
    append_file(@BuildLog, format("[monitor] upgrade done {0}", date_time_str()));
    nativelog("[monitor] upgrade done");
    return(true);
};

// A target is either a full path or a path relative to the build root
// (e.g. "AgentCore/AgentCore.csproj").
script(resolve_target)params($t)
{
    if (string_contains($t, "/") || string_contains($t, ":")) {
        return($t);
    };
    return(combine_path(@BuildRoot, $t));
};

// Deploy: the copy.dsl scripts cd to their own directory, so the paths inside
// them (bin/Debug/net9.0 -> ../../WebAgent/webagent/managed) stay correct.
// AgentCore / BatchCmdDsl / BatchScriptApi also deploy from their own PostBuild;
// CefDotnetApp has none (its PostBuild is commented out), so all four are run.
script(deploy)
{
    $r = execute_command(@BatchCommandExe, "d:/GitHub/BatchCommand/CefDotnetApp/copy.dsl bat", @BuildRoot, @DeployTimeoutMs);
    nativelog("[monitor] deploy CefDotnetApp exit={0}", get_dict_or_json_param($r, "exitCode"));
    $r = execute_command(@BatchCommandExe, "d:/GitHub/BatchCommand/BatchCmdDslHost/BatchCmdDsl/copy.dsl bat", @BuildRoot, @DeployTimeoutMs);
    nativelog("[monitor] deploy BatchCmdDsl exit={0}", get_dict_or_json_param($r, "exitCode"));
    $r = execute_command(@BatchCommandExe, "d:/GitHub/BatchCommand/BatchScriptApi/copy.dsl bat", @BuildRoot, @DeployTimeoutMs);
    nativelog("[monitor] deploy BatchScriptApi exit={0}", get_dict_or_json_param($r, "exitCode"));
    $r = execute_command(@BatchCommandExe, "d:/GitHub/BatchCommand/AgentCore/copy.dsl bat", @BuildRoot, @DeployTimeoutMs);
    nativelog("[monitor] deploy AgentCore exit={0}", get_dict_or_json_param($r, "exitCode"));
};
