// Process monitor for webagent, and the opt-in gate of the whole self-upgrade
// flow.
//
// Chain: upgrade_and_restart.bat/.sh -> THIS monitor -> webagent -> agent.
// Loaded by a BARE BatchCmdDslHost.exe (no --plugin, no --script); start it
// from the webagent dir with:   BatchCmdDslHost.exe
//
// Responsibilities:
//   - keep-alive: when webagent dies, start it again with the command line
//     recorded in webagent_cmdline.txt (webagent root, NOT managed/ - no
//     generated files there);
//   - upgrade gate: when webagent is gone and upgrade_request.json
//     exists (the LLM's restart request; the file existing IS the request),
//     launch the one-shot upgrade script DETACHED and then STOP
//     OURSELVES - this monitor holds managed/BatchScriptApi.dll, so it has to
//     be gone before the script can deploy anything. The script waits for
//     every webagent / BatchCmdDslHost process to disappear, builds, deploys,
//     and then starts the monitor again: the chain closes by itself;
//   - heartbeat: monitor_alive.txt (webagent root) is refreshed on every check, so
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

    @WebAgentExe = combine_path(basepath, "webagent.exe");
    @RequestFile = combine_path(basepath, "upgrade_request.json");
    @StateFile = combine_path(basepath, "upgrade_state.json");
    @CmdLineFile = combine_path(basepath, "webagent_cmdline.txt");
    @AliveFile = combine_path(basepath, "monitor_alive.txt");
    // Used when webagent_cmdline.txt does not exist yet (first run).
    @DefaultCmdLine = " --url=https://evaluation.woa.com/chat --projectidentity=aiclaw --metadsl=script.dsl,script_renderer.dsl --remote-debugging-port=9228 --remote-allow-origins=*";

    // Ticks between two checks (the host default is 1000ms per tick).
    @CheckEveryTicks = 3;
    // How many checks to wait for the agent host to exit by itself (it watches
    // its parent browser and exits on its own, see script_agent.dsl on_tick).
    @AgentExitChecks = 10;
    // RSI attempt counting: how long stage=verify_pending (deployed, no
    // agent_ready yet) may hang before the monitor counts a failed attempt
    // (webagent takes ~30s to boot, then the page load).
    @VerifyTimeoutSeconds = 120;
    // How long stage=updating may hang after the request was consumed (the
    // one-shot script builds 4 projects before restarting us - 900s covers
    // a slow full build).
    @UpdatingTimeoutSeconds = 900;

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
    check_upgrade_stuck();

    if (webagent_alive()) {
        set_context_var("agentWaitChecks", 0);
        return(0);
    };

    nativelog("[monitor] webagent is not running");

    // ---- upgrade gate: hand over to the one-shot script and stop ourselves ----
    if (file_exists(@RequestFile)) {
        nativelog("[monitor] update requested (upgrade_request.json), launching the upgrade script and stopping this monitor");
        consume_upgrade_request();
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

// RSI: consume the LLM's restart request. Merge its level/goal/resumePrompt
// into upgrade_state.json (webagent root, the code-owned progress file the
// agent side resumes from), mark stage=updating, then DELETE the request - before
// the upgrade script even starts, so a failed build/restart of this monitor
// can never re-read the same request and loop the upgrade forever.
script(consume_upgrade_request)
{
    $req = from_json("{}");
    if (file_exists(@RequestFile)) {
        $txt = read_file(@RequestFile);
        if (!isnullorempty($txt)) {
            $parsed = from_json($txt);
            if (!isnull($parsed)) {
                $req = $parsed;
            };
        };
    };
    $v = from_json("{}");
    if (file_exists(@StateFile)) {
        $s = read_file(@StateFile);
        if (!isnullorempty($s)) {
            $parsed = from_json($s);
            if (!isnull($parsed)) {
                $v = $parsed;
            };
        };
    };
    dict_or_json_set($v, "level", dict_or_json_get($req, "level"));
    dict_or_json_set($v, "goal", dict_or_json_get($req, "goal"));
    dict_or_json_set($v, "resumePrompt", dict_or_json_get($req, "resumePrompt"));
    dict_or_json_set($v, "stage", "updating");
    dict_or_json_set($v, "stageSince", now());
    write_file(@StateFile, to_json($v));
    delete_file(@RequestFile);
    nativelog("[monitor] upgrade request consumed (goal={0}), state stage -> updating", to_string(dict_or_json_get($req, "goal")));
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
    // RSI (see d:/AiClaw/docs/webagent_recursive_self_improvement.md): when
    // the one-shot upgrade script deployed a new build and restarted us, the
    // state file still says stage=updating (what we set when consuming the
    // request). Advance it before relaunching webagent, so the agent_ready
    // handler in script_agent.dsl recognizes the restart and pushes the
    // resumePrompt back to the page.
    advance_upgrade_stage_after_deploy();
    $cmdline = read_cmdline();
    $wpid = launch_process(@WebAgentExe, $cmdline, basepath);
    set_context_var("webagentPid", $wpid);
    write_file(@CmdLineFile, $cmdline);
    nativelog("[monitor] launched webagent pid={0} args={1}", $wpid, $cmdline);
};

// RSI: stage updating -> verify_pending (before relaunching webagent after a
// deploy), preserving every other field (goal, resumePrompt, attempt, ...).
script(advance_upgrade_stage_after_deploy)
{
    $txt = read_file(@StateFile);
    if (isnullorempty($txt)) {
        return;
    };
    $v = from_json($txt);
    if (isnull($v)) {
        return;
    };
    if (to_string(dict_or_json_get($v, "stage")) != "updating") {
        return;
    };
    dict_or_json_set($v, "stage", "verify_pending");
    dict_or_json_set($v, "stageSince", now());
    write_file(@StateFile, to_json($v));
    nativelog("[monitor] upgrade deployed, stage -> verify_pending");
};

// RSI attempt counting: the two non-terminal stages must not hang.
// stage=verify_pending with no agent_ready for @VerifyTimeoutSeconds, or
// stage=updating with the request already consumed for
// @UpdatingTimeoutSeconds (the one-shot script died before restarting us -
// while it works normally this monitor is not running, so the deadline
// cannot misfire during a normal build), counts as one failed attempt.
// Two strikes -> stage=aborted + lastError. The keep-alive in on_tick still
// restarts webagent (service first - aborted only blocks the upgrade flow;
// the agent-side resume_after_upgrade tells the LLM when it fires).
// stageSince (written at every stage change) keeps the deadline alive
// across a monitor restart.
script(check_upgrade_stuck)
{
    $txt = read_file(@StateFile);
    if (isnullorempty($txt)) {
        return;
    };
    $v = from_json($txt);
    if (isnull($v)) {
        return;
    };
    $stage = to_string(dict_or_json_get($v, "stage"));
    if ($stage != "updating" && $stage != "verify_pending") {
        return;
    };
    $t = parse_date_time(to_string(dict_or_json_get($v, "stageSince")));
    if (isnull($t)) {
        return;
    };
    if ($stage == "verify_pending") {
        if (get_diff_time_seconds($t, now()) < @VerifyTimeoutSeconds) {
            return;
        };
    }
    else {
        // updating is legitimate while the request still waits for the
        // webagent-dead check above to consume it.
        if (file_exists(@RequestFile)) {
            return;
        };
        if (get_diff_time_seconds($t, now()) < @UpdatingTimeoutSeconds) {
            return;
        };
    };
    $attempt = str_to_int(to_string(dict_or_json_get($v, "attempt"))) + 1;
    dict_or_json_set($v, "attempt", $attempt);
    dict_or_json_set($v, "stageSince", now());
    if ($attempt >= 2) {
        dict_or_json_set($v, "stage", "aborted");
        dict_or_json_set($v, "lastError", format("upgrade stalled in {0}", $stage));
        nativelog("[monitor] upgrade aborted (attempt={0}, stalled in {1})", $attempt, $stage);
    }
    else {
        nativelog("[monitor] upgrade stalled in {0} (attempt={1}), new deadline started", $stage, $attempt);
    };
    write_file(@StateFile, to_json($v));
};

// Launch the one-shot upgrade script detached (own console/session, no
// coupling with ours): it survives our exit and restarts the monitor when it
// is done.
script(launch_upgrade_script)
{
    if (ismac) {
        launch_process_detached("/bin/bash", "upgrade_and_restart.sh", basepath);
    }
    else {
        launch_process_detached("cmd.exe", "/c upgrade_and_restart.bat", basepath);
    };
};
