// Process monitor for webagent, and the opt-in gate of the whole self-upgrade
// flow.
//
// Chain: upgrade_and_restart.bat/.sh -> THIS monitor -> webagent -> agent.
// Loaded by a BARE BatchCmdDslHost.exe (no --plugin, no --script); start it
// from the webagent dir with:   BatchCmdDslHost.exe
//
// Responsibilities:
//   - keep-alive: when webagent dies, start it again with the command line
//     recorded in managed/webagent_cmdline.txt;
//   - upgrade gate: when webagent is gone and managed/upgrade_state.json asks
//     for an update, launch the one-shot upgrade script DETACHED and then STOP
//     OURSELVES - this monitor holds managed/BatchScriptApi.dll, so it has to
//     be gone before the script can deploy anything. The script waits for
//     every webagent / BatchCmdDslHost process to disappear, builds, deploys,
//     and then starts the monitor again: the chain closes by itself;
//   - heartbeat: managed/monitor_alive.txt is refreshed on every check, so
//     the agent side can tell whether it is supervised before it exits for an
//     update (unsupervised, nobody would ever restart it).
//
// When this monitor is NOT running the upgrade flow does not exist at all:
// webagent behaves like a plain browser (agent started by the browser, agent
// exits on its own when the browser dies).
//
// on_tick MUST end with return(0) on every keep-alive path - its value is the
// host main loop return and anything non-zero stops the host (the trap that
// once restarted the agent forever). The ONE intentional exception is the
// upgrade path, which returns 1 to stop this monitor gracefully
// (Program.Loop -> Shutdown -> on_finalize).

script(on_init)
{
    nativelog("[monitor] on_init, monitor pid={0}, basepath={1}", pid(), basepath);

    @DataDir = combine_path(basepath, "managed");
    @WebAgentExe = combine_path(basepath, "webagent.exe");
    @StateFile = combine_path(@DataDir, "upgrade_state.json");
    @CmdLineFile = combine_path(@DataDir, "webagent_cmdline.txt");
    @AliveFile = combine_path(@DataDir, "monitor_alive.txt");
    // Used when webagent_cmdline.txt does not exist yet (first run).
    @DefaultCmdLine = " --url=https://evaluation.woa.com/chat --projectidentity=aiclaw --metadsl=script.dsl,script_renderer.dsl --remote-debugging-port=9228 --remote-allow-origins=*";

    // Ticks between two checks (the host default is 1000ms per tick).
    @CheckEveryTicks = 3;
    // How many checks to wait for the agent host to exit by itself (it watches
    // its parent browser and exits on its own, see script_agent.dsl on_tick).
    @AgentExitChecks = 10;

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
    heartbeat();

    if (webagent_alive()) {
        set_context_var("agentWaitChecks", 0);
        return(0);
    };

    nativelog("[monitor] webagent is not running");

    // ---- upgrade gate: hand over to the one-shot script and stop ourselves ----
    $phase = upgrade_phase();
    if ($phase == "build_pending" || $phase == "restart_pending") {
        nativelog("[monitor] update requested (phase={0}), launching the upgrade script and stopping this monitor", $phase);
        launch_upgrade_script();
        return(1);
    };

    // ---- keep-alive path: wait for the agent host to release the dlls ----
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

    start_webagent();
    return(0);
};

// Refresh the supervision heartbeat: the agent side refuses to exit for an
// update when this file is stale (nobody would restart it).
script(heartbeat)
{
    write_file(@AliveFile, to_string(now()));
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
    $phase = dict_or_json_get($v, "phase");
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

// Launch the one-shot upgrade script detached (own console/session, no
// coupling with ours): it survives our exit and restarts the monitor when it
// is done.
script(launch_upgrade_script)
{
    if (ismac) {
        launch_process_detached("/bin/bash", "managed/upgrade_and_restart.sh", basepath);
    }
    else {
        launch_process_detached("cmd.exe", "/c managed/upgrade_and_restart.bat", basepath);
    };
};
