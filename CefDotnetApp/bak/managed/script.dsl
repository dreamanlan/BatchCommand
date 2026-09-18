// Note:The design philosophy behind these DSL scripts is to be stateless;
// all state resides within C# or JS. The DSL's global variables are utilized
// for configuring constants, and each hot-reload operation executes independently.
script(init_global_consts)
{
    // Initialize global constants for browser here
    setenv("PLAYWRIGHT_DRIVER_SEARCH_PATH", combinepath(basepath, "managed"));
    // Standalone agent process (AgentCore host) config.
    // 9527: the relay port (default of MetaDslExecutor.SetProcessInfo,
    // overridable with --agentport; the site ports are retired).
    // Note: the launcher is the native host BatchCmdDslHost at the webagent
    // root (managed/BatchCmdDsl is just an empty-main apphost). On macOS the
    // bundle layout puts it in webagent.app/Contents/MacOS/ (basepath is
    // webagent.app/Contents there, no .exe suffix).
    @AgentPort = 9527;
    if (ismac) {
        @AgentExe = combinepath(basepath, "MacOS", "BatchCmdDslHost");
    }
    else {
        @AgentExe = combinepath(basepath, "BatchCmdDslHost.exe");
    };
    @AgentArgs = format("--plugin=managed/AgentCore.dll --interval=50 --agentport={0}", @AgentPort);
    // Relay segment: the browser process is the only websocket client of the
    // standalone AgentCore (page js reaches it through cefQuery + this relay).
    // NOTE: must be "localhost", not "127.0.0.1" - the AgentCore HttpListener
    // prefix is http://localhost:{port}/ and it rejects mismatched Host headers.
    @AgentWsUrl = format("ws://localhost:{0}", @AgentPort);
    // WeChat bridge upstream for the relay_lite pages (option A: one
    // browser-side connection per agent, "wxb_<agentId>"; the bridge keeps
    // routing by registered connection name, zero bridge changes).
    @LiteBridgeUrl = "ws://localhost:3000";
};

// Start the standalone agent host process when it is not running.
// The agent service (AgentCore) lives in its own process so the renderer
// processes can keep the sandbox enabled; communication with inject.js goes
// over the websocket server started by script_agent.dsl on_init.
script(start_agent_process)
{
    $ct = count_process("BatchCmdDslHost.exe");
    if ($ct <= 0) {
        // Pass the project identity through so the agent process knows the
        // initial identity (initialprojectidentity global in script_agent.dsl).
        $args = @AgentArgs;
        if (!isnullorempty(initialprojectidentity)) {
            $args = format("{0} --projectidentity={1}", $args, initialprojectidentity);
        };
        $pid = launch_process(@AgentExe, $args, basepath);
        // Record the pid: on browser exit the agent host is stopped ONLY if
        // this instance launched it (see on_browser_finalize).
        set_context_var("agentSelfLaunchPid", $pid);
        nativelog("[dsl] start_agent_process: launched pid={0} args={1}", $pid, $args);
    };
};
script(on_init)
{
    nativelog("[dsl] on_init finish, pid:{0}, type:{1}", pid(), processtype);
    fileecho(true);
    start_agent_process();
    // Relay: a fast heartbeat drives wsclient event dispatch on the main
    // thread (~10-20ms relay latency); the heavy keep-alive work in
    // on_heart_beat is throttled by a beat counter.
    set_heartbeat_interval(10);
    set_context_var("hbCount", 0);
    set_context_var("agentRetryAt", 0);
    // 3rd arg 10: link-layer retry budget - the C# manager self-heals
    // transient drops with the same id; exhaustion reports "failed".
    wsclient_open(@AgentWsUrl, "agent", 10);
    // no_sandbox return: browser-process only, GLOBAL (all children).
    // Per-type disabling: use OnBeforeChildProcessLaunch
    // (NOT on_before_command_line_processing — that only fires at each process's own startup, too late on the child side).
    // no-sandbox = false
    return(false);
};
script(on_finalize)
{
    nativelog("[dsl] on_finalize finish, pid:{0}, type:{1}", pid(), processtype);
};

// Relay callbacks (drained on the browser main thread, see WsClientManager).
script(on_wsclient_state)params($id, $state)
{
    nativelog("[relay] wsclient {0} state: {1}", $id, $state);
    if ($state == "connected") {
        // Pending agent_register answer: return the client id as the payload.
        $pending = get_context_var("relayPending_" + $id);
        if (!isnull($pending)) {
            remove_context_var("relayPending_" + $id);
            complete_native_callback($pending, true, $id);
        };
        // Auto (re-)bind in the browser dsl: after a link-layer reconnect
        // the server-side connection is new and lost the slot's agent id.
        // Doing it here (not only via the page push) makes the re-send
        // independent of renderer pushes - it cannot be lost.
        $bindAgentId = get_context_var("relayAgent_" + $id);
        if (!isnull($bindAgentId)) {
            wsclient_send($id, to_json({"type": "agent_bind", "agentId": $bindAgentId}));
        };
        // Notify the page: after a link-layer reconnect the server-side
        // connection is new (slots may re-send agent_bind too; idempotent).
        $urlKey = get_context_var("relayUrl_" + $id);
        if (!isnull($urlKey)) {
            relay_push_to_page($id, to_json({"type": "relay_state", "state": "connected"}), $urlKey);
        };
    };
    if ($id == "agent" && $state == "connected") {
        // Smoke test: full round trip through the standalone AgentCore
        // (agent_call -> handle_agent_command -> ping ->
        //  send_response_to_inject -> agent_result back to on_wsclient_message).
        // Verdict is logged by on_wsclient_message (SMOKE PASS) or the
        // heartbeat watchdog (SMOKE FAIL) - look for "[relay] SMOKE".
        $cmd = to_json({"id": 1, "command": "ping", "params": {}});
        $envelope = to_json({"type": "agent_call", "id": 1, "func": "handle_agent_command", "args": [$cmd]});
        $ok = wsclient_send($id, $envelope);
        set_context_var("smokeSentAt", now());
        set_context_var("smokeResult", "");
        nativelog("[relay] smoke ping sent: {0}", $ok);
    };
    // Link-layer retry in progress (same wsclient id): notify the page but
    // keep the url-key mapping and any pending register - the link layer
    // either reconnects (connected) or exhausts its budget (failed).
    if ($state == "reconnecting") {
        $urlKey = get_context_var("relayUrl_" + $id);
        if (!isnull($urlKey)) {
            relay_push_to_page($id, to_json({"type": "relay_state", "state": $state}), $urlKey);
        };
    };
    // Registered relay clients: the connection is dead (retry budget
    // exhausted or explicit close). Notify the page (relay_transport
    // re-registers) and drop the url-key mapping.
    if ($state == "disconnected" || $state == "failed") {
        $pending = get_context_var("relayPending_" + $id);
        if (!isnull($pending)) {
            // The register never completed: fail the query instead of hanging it.
            remove_context_var("relayPending_" + $id);
            complete_native_callback($pending, false, "", -3);
        };
        $urlKey = get_context_var("relayUrl_" + $id);
        if (!isnull($urlKey)) {
            relay_push_to_page($id, to_json({"type": "relay_state", "state": $state}), $urlKey);
            remove_context_var("relayUrl_" + $id);
            remove_context_var("relayAgent_" + $id);
            remove_context_var("relayBrowser_" + $id);
        };
    };
};

script(on_wsclient_message)params($id, $msg)
{
    // The plain "agent" connection is the smoke link: judge the round trip.
    $urlKey = get_context_var("relayUrl_" + $id);
    if (isnull($urlKey)) {
        if ($id == "agent") {
            $env = dev_tools_parse_bytes($msg);
            $type = "";
            if ($env != null) {
                $type = $env["type"];
            };
            if ($type == "agent_result") {
                $prev = get_context_var("smokeResult");
                if (isnull($prev) || $prev == "") {
                    // Log once per ping cycle: the envelope machinery replies
                    // to every command (branch response first, envelope reply
                    // second), later results are just noise.
                    $elapsed = get_diff_time_seconds(get_context_var("smokeSentAt"), now());
                    set_context_var("smokeResult", "pass");
                    nativelog("[relay] SMOKE PASS: browser->AgentCore->browser round trip OK in {0}s (success={1}, data={2}, error={3})", $elapsed, $env["success"], $env["data"], $env["error"]);
                };
            }
            else {
                nativelog("[relay] wsclient agent message (non-result, expecting agent_result): {0}", get_string_in_length($msg, 200));
            };
        }
        else {
            nativelog("[relay] wsclient {0} message: {1}", $id, get_string_in_length($msg, 200));
        };
        return;
    };
    // Registered relay clients: push the message through to the page
    // (envelopes and raw MetaDSL texts alike; relay_transport.js classifies).
    relay_push_to_page($id, $msg, $urlKey);
};

// Push a relay message to the page that registered $id: find its browser by
// the url key recorded at registration and call window.onAgentEvent in it.
// NOTE: to_json cannot serialize plain strings (LitJson limitation) - use
// json_escape(s, true) to build the JSON string literals.
script(relay_push_to_page)params($id, $msg, $urlKey)
{
    $bid = find_browser_id_by_url_key($urlKey);
    if ($bid > 0 && set_context_by_id($bid)) {
        $js = format("if (window.onAgentEvent) window.onAgentEvent({0}, {1});", json_escape($id, true), json_escape($msg, true));
        send_javascript_code($js);
    }
    else {
        nativelog("[relay] push failed: no browser for url key {0} (client {1}, bid={2})", $urlKey, $id, $bid);
    };
};

script(on_browser_init)
{
    nativelog("[dsl] on_browser_init finish");
    $browser = nativeapi.GetBrowser();
    if (!isnull($browser)) {
        $browser.SetCspBypass(true);
    };
    $notFirst = get_context_var("notFirst");
    if (isnull($notFirst)) {
        set_context_var("notFirst", 1);
        deletefile(combinepath(basepath, "agentcore.log"));
        deletefile(combinepath(basepath, "debug.log"));
        deletefile(combinepath(basepath, "webagent_cache/chrome_debug.log"));
    };
};
// Default handler for the C# hot reload watcher (watch_file / watch_dir /
// HotReloadManager): invoked unconditionally on every watched file change.
// THREAD CAVEAT: this may run on ANY .NET threadpool thread (the watcher
// callback), where the interpreter is worker-style — init_global_consts
// globals and host-level context vars are visible, but main-thread runtime
// state is NOT. Keep handlers light, thread-safe and stateless; anything
// heavier should enqueue to the main thread instead.
script(on_file_changed)params($filePath, $fileType)
{
    nativelog("[dsl] on_file_changed: {0} ({1})", $filePath, $fileType);
    return((true, ""));
};
script(on_browser_finalize)params($remainingBrowsers)
{
    // The global teardown (stop the self-launched agent host, close every
    // relay connection) belongs to the LAST live browser only: popups/tabs
    // close while other pages still need the agent. Per-page cleanup is
    // handled by pagehide (js), on_render_process_terminated and the
    // heartbeat reaper instead.
    if ($remainingBrowsers > 0) {
        nativelog("[dsl] on_browser_finalize: {0} browser(s) still open, skip the agent teardown", $remainingBrowsers);
        return;
    };
    // Stop the standalone agent host IF this browser instance launched it
    // (pid recorded in start_agent_process). When it was already running
    // (owned by another webagent instance), leave it alone. If a sharing
    // instance is still running, its heartbeat keep-alive relaunches the
    // agent within ~2s, so the multi-instance case stays safe.
    $agentPid = get_context_var("agentSelfLaunchPid");
    if (!isnull($agentPid) && $agentPid > 0) {
        $stopped = kill_process($agentPid);
        remove_context_var("agentSelfLaunchPid");
        nativelog("[dsl] on_browser_finalize: stopped self-launched agent host (pid={0}, stopped={1})", $agentPid, $stopped);
    };
    // The relay connections belong to this browser process.
    wsclient_close_all();
    nativelog("[dsl] on_browser_finalize finish");
};

script(on_heart_beat)params($processType,$deltaTime)
{
    // Do something every heart beat
    if ($processType == 0) {
        handle_thread_queue();
        // Relay: drain wsclient events (also auto-drained by the host before
        // this callback; explicit drain keeps the ordering obvious).
        handle_wsclient_queue(100);
        // Throttle the process keep-alive + relay reconnect to ~2s.
        $hbCount = get_context_var("hbCount");
        if (isnull($hbCount)) {
            $hbCount = 0;
        };
        $hbCount = $hbCount + 1;
        set_context_var("hbCount", $hbCount);
        if (($hbCount % 200) == 0) {
            // Keep the standalone agent host alive (restarts it if it died).
            start_agent_process();
            // Zombie relay reaper: close wsclient connections whose owning
            // browser no longer exists (page closed / renderer died without
            // unregistering; the graceful pagehide path is in relay_transport).
            $wscList = wsclient_list();
            looplist($wscList) {
                $entry = $$;
                $parts = split($entry, "|");
                $cid = $parts[0];
                $urlKey = get_context_var("relayUrl_" + $cid);
                if (!isnull($urlKey)) {
                    $bid = get_context_var("relayBrowser_" + $cid);
                    if (!isnull($bid) && $bid > 0) {
                        if (!set_context_by_id($bid)) {
                            nativelog("[relay] reaper: browser {0} gone, closing zombie connection {1} (url key {2})", $bid, $cid, $urlKey);
                            wsclient_close($cid);
                            remove_context_var("relayUrl_" + $cid);
                            remove_context_var("relayAgent_" + $cid);
                            remove_context_var("relayBrowser_" + $cid);
                        };
                    };
                };
            };
            // Smoke watchdog: the ping was sent but no agent_result came back.
            $smokeSentAt = get_context_var("smokeSentAt");
            $smokeResult = get_context_var("smokeResult");
            if (!isnull($smokeSentAt) && (isnull($smokeResult) || $smokeResult == "")) {
                $elapsed = get_diff_time_seconds($smokeSentAt, now());
                if ($elapsed > 10) {
                    set_context_var("smokeResult", "fail");
                    nativelog("[relay] SMOKE FAIL: no agent_result within {0}s. Check: 1) AgentCore console (script_agent.dsl on_init / ws server on port {1}) 2) wsclient state: {2} 3) ws clients: {3}", $elapsed, @AgentPort, wsclient_state("agent"), to_json(wsclient_list()));
                };
            };
        };
        // Reconnect the relay "agent" link when the C# link layer gave up
        // ("failed": retry budget exhausted - it already retried by itself;
        // "unknown": entry gone). Transient drops are healed by the link
        // layer (wsclient_open reconnect budget, see on_init).
        // Throttled by the beat counter (~1s at the 10ms heartbeat): time()
        // in this engine is unix SECONDS, a time()+N gate would wait N*1000s.
        if (($hbCount % 100) == 0) {
            $state = wsclient_state("agent");
            if ($state == "failed" || $state == "unknown") {
                if ($state != "unknown") {
                    wsclient_close("agent");
                };
                $id = wsclient_open(@AgentWsUrl, "agent");
                nativelog("[relay] reconnect attempt from state {0}: {1}", $state, $id);
            };
        };
    };
};
script(on_console_log)params($level,$message,$source,$line,$maxLogSize)
{
    return((false, $maxLogSize));
};

script(on_before_command_line_processing)params($processType, $cmdLine)
{
    if ($processType == 0) {
        //debuggerlaunch();
    };

    // Add command line switches here
    // Example: $cmdLine.AppendSwitch("disable-gpu");
    // Example: $cmdLine.AppendSwitchWithValue("remote-debugging-port", "9222");

    // Check if a switch exists
    // if (!$cmdLine.HasSwitch("disable-gpu")) {
    //     $cmdLine.AppendSwitch("disable-gpu");
    // };

    $url = $cmdLine.GetSwitchValue("url");

    nativelog("[dsl] on_before_command_line_processing: process_type={0}, url={1}, pid={2}", $processType, $url, pid());

    if (stringcontainsany($url, "file:///", "http://localhost") && stringcontainsany($url, "AgentCore/hotreload_test.html", "http://localhost:8080/agent.html", "http://localhost:8081", "http://localhost:8082")) {
        $cmdLine.AppendSwitch("allow-file-access-from-files");
    };

    //$cmdLine.AppendSwitch("disable-web-security");
    //$cmdLine.AppendSwitch("allow-file-access-from-files");
    //$cmdLine.AppendSwitch("disable-site-isolation-trials");
    // Prevent throttling/priority reduction when window is minimized or in background
    $cmdLine.AppendSwitch("disable-background-timer-throttling");
    $cmdLine.AppendSwitch("disable-renderer-backgrounding");
    $cmdLine.AppendSwitch("disable-backgrounding-occluded-windows");

    //--disable-chrome-login-prompt --proxy-pac-url=http://www.gamexyz.net/google_proxy.pac --ignore-certificate-errors-spki-list=2jcZDMGiVyFnDdB4jNPPeNmF0Vwn+SZ4BddAfhVyeV4=
    $cmdLine.AppendSwitch("disable-chrome-login-prompt");
    $cmdLine.AppendSwitchWithValue("proxy-pac-url", "http://www.gamexyz.net/google_proxy.pac");
    $cmdLine.AppendSwitchWithValue("ignore-certificate-errors-spki-list", "2jcZDMGiVyFnDdB4jNPPeNmF0Vwn+SZ4BddAfhVyeV4=");

    // Override user-agent-product to look like standard Chrome
    $cmdLine.AppendSwitchWithValue("user-agent-product", "Chrome/150.0.7871.187");
};

script(on_before_child_process_launch)params($processType, $cmdLine)
{
    // $cmdLine is the child process command line (passed as parameter)
    nativelog("[dsl] on_before_child_process_launch {0}", $processType);
    //debuggerbreak();
};

script(on_already_running_app_relaunch)params($cmdLine, $curDir)
{
    nativelog("[dsl] on_already_running_app_relaunch {0} {1}", $cmdLine.CommandLineString, $curDir);
    // Return true to use default behavior (create new chrome window)
    // Return false to indicate relaunch was NOT handled (CEF default)
    return(false);
};

script(on_before_browse)params($request,$userGesture,$isRedirect)
{
    nativelog("[dsl] on_before_browse: url={0} method={1} userGesture={2} isRedirect={3}", $request.Url, $request.Method, $userGesture, $isRedirect);
    return((false, false));
};

// Note: this function will be called on the browser process IO thread.
// $handle identifies the parked native CefCallback. It is only meaningful when
// returning RV_CONTINUE_ASYNC(2): the script must then call
// complete_native_callback($handle, true) to resume the request or
// complete_native_callback($handle, false) to cancel it, otherwise the request
// stays pending until the browser closes.
// Return (handled, cef_return_value_t): RV_CANCEL=0, RV_CONTINUE=1, RV_CONTINUE_ASYNC=2.
script(on_before_resource_load)params($request,$handle)
{
    //nativelog("[dsl] on_before_resource_load: type={0} url={1} method={2}", $request.ResourceType, $request.Url, $request.Method);
    return((false, 1));
};

// Note: this function will be called on the browser process UI thread.
// Called for JavaScript alert / confirm / prompt and beforeunload dialogs.
// $dialogType: 0=alert, 1=confirm, 2=prompt, 3=beforeunload
// Return (handled, decision):
//   0 = not taken over, use the CEF default dialog (same as no handler)
//   1 = taken over, this script shows a custom dialog in the page
//   2 = suppress the message silently (ignored for beforeunload)
//   3 = taken over, the script handles display and completion itself
// When taking over (1 or 3) the script MUST eventually call
// complete_native_callback($handle, ok[, text]) or the page hangs:
//   confirm  -> ok is the true/false result
//   prompt   -> text is the entered string
//   beforeunload -> ok=true LEAVES the page, ok=false STAYS
// The display JavaScript must be sent inside this callback: the browser context
// is cleared when the callback returns (send_javascript_code only posts to the
// renderer, so it does not block the UI thread).
script(on_js_dialog)params($dialogType,$originUrl,$message,$defaultText,$handle)
{
    nativelog("[dsl] on_js_dialog: type={0} origin={1} handle={2}", $dialogType, $originUrl, $handle);

    // Default: take nothing over, CEF shows its own dialogs (current behavior).
    // To use the in-page AgentDialog component instead, uncomment the block
    // below. Keep beforeunload ($dialogType == 3) on the CEF default: the page
    // is already unloading and an injected dialog may never render.
    // show_native_js_dialog escapes the payload, so any message content is safe.
    // AgentDialog reports the result back through window.cefQuery, which lands
    // in on_browser_cef_query below; if the page has no AgentDialog it reports
    // js_dialog_unavailable instead and the dialog gets canceled.
    //
    // if ($dialogType != 3 && stringcontainsany($originUrl, "localhost:8080")) {
    //     show_native_js_dialog($handle, $dialogType, $message, $defaultText);
    //     return((true, 1));
    // };

    return((false, 0));
};

// Note: this function will be called on the browser process IO thread.
script(on_resource_redirect)params($request,$response,$new_url)
{
    return((false, ""));
};

// Note: this function will be called on the browser process IO thread.
// $response is writable only during this callback. Status, status text, MIME
// type, charset and response headers may be changed before CEF processes them.
script(on_before_resource_response)params($request,$response)
{
    if (stringcontainsany($request.Url, "gamexyz.net:8080", "www.google.com/ai", "www.google.com/search", "gemini.google.com", "chatgpt.com", "chat.openai.com")) {
        $response.RemoveHeaderByName("Content-Security-Policy");
    };
};

// Note: this function will be called on the browser process IO thread after a
// resource load completes. Request and response are read-only here.
script(on_resource_load_complete)params($request,$response,$status,$received_content_length)
{
};

// Note: this function will be called on the browser process IO thread for an
// unknown URL scheme. Return (handled, allow_os_execution).
script(on_protocol_execution)params($request,$allow_os_execution)
{
    return((false, $allow_os_execution));
};

// Note: this function will be called on the browser process IO thread.
// Response inspection point (GetResourceResponseFilter): $response is the
// actual upstream response (read-only; mutations are silently dropped by CEF).
// Return (handled, replace_content):
//   handled: true = register MyResponseFilter for body filtering.
//   replace_content: false = skip the body filter (inspection only).
script(on_resource_response_filter)params($request, $response)
{
    //nativelog("[dsl] on_resource_response_filter: inspection headers: {0}", $response.HeaderMap);
    return((false, false));
};

// Note: this function will be called on the browser process IO thread.
// Response body filter. Streams body chunks through DSL for transformation.
// $data_in is a byte[] of the current chunk (capped to 4MB by native).
// Return (handled_bool, status_int, output_byte_array, bytes_read):
//   handled_bool: true = use DSL's outputs below; false = native passes the
//   chunk through unchanged (ignores the other fields).
//   status_int: 0=DONE, 1=NEED_MORE_DATA, 2=ERROR (matches
//   cef_response_filter_status_t).
//   output_byte_array: filtered output (byte[], may be empty; length is
//   clamped to chromium's buffer size by the C# side).
//   bytes_read: how many input bytes DSL consumed (0..$data_in.length).
//   Used when input > output buffer (e.g. decompress filter that fills the
//   4MB staging before consuming all input); native keeps the unconsumed
//   remainder for the next call.
// Default: handled=true, pass through unchanged (consume all input, produce
// input as-is).
script(on_response_content_filter)params($data_in)
{
    return((false, 0, $data_in, len($data_in)));
};

script(on_load_start)params($url,$transitionType,$isMainFrame)
{
    nativelog("[dsl] on_load_start:{0} {1} {2}", $url, $transitionType, $isMainFrame);
};
script(on_load_end)params($url,$httpStatusCode,$injectAllFrame,$isMainFrame)
{
    nativelog("[dsl] on_load_end:{0} {1} {2} {3}", $url, $httpStatusCode, $injectAllFrame, $isMainFrame);
    return((true, ""));
};
// Note: this function will be called on the browser process UI thread.
// A custom scheme request is offered to the script, which decides whether to
// serve the content. $handle identifies the parked native callback. Mime is
// fixed to text/html for the synchronous path. Return (handled, html):
//   (false, "")      -> C++ serves its built-in fallback (tabbar page or 404).
//   (true, "<html>") -> synchronous: the html is returned directly as
//                       text/html (200); do NOT call complete_native_callback.
//   (true, "")       -> async takeover: the script MUST later call
//                       complete_native_callback($handle, true, responseJson, 0)
//                       or the request hangs until it times out. responseJson is
//                       {"status":int,"mime":string,"body":string,"base64":bool}
//                       (a bare string is treated as an html body); ok=false
//                       cancels the request with an error status.
script(on_custom_scheme)params($scheme,$url,$method,$referrer,$handle)
{
    nativelog("[dsl] on_custom_scheme: scheme={0} url={1} method={2} referrer={3} handle={4}", $scheme, $url, $method, $referrer, $handle);
    return((false, ""));
};
script(on_loading_state_change)params($url,$isLoading,$canGoBack,$canGoForward)
{
    nativelog("[dsl] on_loading_state_change: url={0}, isLoading={1}, canGoBack={2}, canGoForward={3}", $url, $isLoading, $canGoBack, $canGoForward);
};
script(on_load_error)params($errorCode,$errorText,$failedUrl)
{
    nativelog("[dsl] on_load_error:{0} {1} {2}", $errorCode, $errorText, $failedUrl);
};
script(on_render_process_terminated)params($startupUrl,$url,$status,$errorCode,$errorString)
{
    nativelog("[dsl] on_render_process_terminated: startup_url={0}, url={1}, status={2}, error_code={3}, error_string={4}", $startupUrl, $url, $status, $errorCode, $errorString);
    // The renderer serving $url is dead: its relay registrations are zombies
    // now (the js could not unregister). Close every connection whose url key
    // matches the dead renderer's url. A live same-site tab sharing this url
    // key gets its connections closed too — its js auto re-registers via the
    // relay_state push, so the worst case is a brief reconnect.
    $wscList = wsclient_list();
    looplist($wscList) {
        $entry = $$;
        $parts = split($entry, "|");
        $cid = $parts[0];
        $urlKey = get_context_var("relayUrl_" + $cid);
        if (!isnull($urlKey) && string_contains($url, $urlKey)) {
            wsclient_close($cid);
            remove_context_var("relayUrl_" + $cid);
            remove_context_var("relayAgent_" + $cid);
            remove_context_var("relayBrowser_" + $cid);
            nativelog("[relay] renderer terminated: closed connection {0} (url key {1})", $cid, $urlKey);
        };
    };
    return((true, ""));
};

// Called on the browser process IO thread when a target host (or proxy)
// requests HTTP authentication credentials.
// Return (handled, user, pass):
//   handled == false                 -> DSL declines; the C++ side runs its
//                                       CredUI fallback (Credential Manager on
//                                       Windows, Keychain + NSAlert on mac).
//   handled == true,  user non-empty -> silently use the returned credentials.
//   handled == true,  user empty     -> DSL takes ownership of $handle and must
//                                       complete it later via
//                                       native_callback_complete.
// $attempt is 0 for the first call on a given target within this process, 1
// when a previously supplied credential was rejected (the fallback then purges
// the stale saved entry before prompting again).
// The default implementation always declines so that the user is prompted.
script(on_get_auth_credentials)params($isProxy,$host,$port,$realm,$scheme,$originUrl,$handle,$attempt)
{
    nativelog("[dsl] on_get_auth_credentials: isProxy={0}, host={1}, port={2}, realm={3}, scheme={4}, origin={5}, handle={6}, attempt={7}", $isProxy, $host, $port, $realm, $scheme, $originUrl, $handle, $attempt);
    // Supply hard-coded credentials for the outbound proxy. Only apply to
    // proxy challenges; leave target-server auth (e.g. site logins) alone so
    // the user is still prompted for those.
    if($isProxy){
        return((true, "dreaman", "nopasswd"));
    };
    return((false, "", ""));
};

// Called on the CEF UI thread when a page requests camera / microphone / etc.
// $requested is a bitmask of CEF_MEDIA_PERMISSION_* values; $menuDisabled is
// the current state of the "media handling disabled" menu switch.
// Return (handled, allowedBits):
//   handled == false -> DSL declines; C++ falls back to the menu switch, then
//                       to Chromium's native permission prompt.
//   handled == true  -> silently grant exactly the bits in allowedBits
//                       (0 = deny everything).
//
// IMPORTANT: for getUserMedia requests (device audio/video capture) the grant
// is all-or-nothing - allowedBits must equal $requested, otherwise the call
// fails instead of being partially granted. Returning a strict subset is only
// meaningful for getDisplayMedia (screen/desktop capture).
//
// Note: adding --enable-media-stream to the command line grants every request
// and skips this handler entirely (media_access_query.cc CheckCommandLinePermission).
// Default implementation always declines so that the user is prompted.
script(on_request_media_access_permission)params($origin,$requested,$menuDisabled)
{
    nativelog("[dsl] on_request_media_access_permission: origin={0}, requested={1}, menu_disabled={2}", $origin, $requested, $menuDisabled);
    return((false, 0));
};

// Called on the CEF UI thread when Chromium raises a permission bubble
// (notifications / geolocation / clipboard / storage-access / ...). This
// covers BOTH alloy-style and chrome-style windows -- every window flavor
// (main window, chrome-style popup, --use-default-popup, Views overlay)
// funnels through BaseClientHandler::MaybeHandlePermissionPromptViaDSL.
// $requested is a bitmask of CEF_PERMISSION_TYPE_* values from cef_types.h:
//   NOTIFICATIONS = 1 << 15, GEOLOCATION = 1 << 8, CLIPBOARD = 1 << 3,
//   MEDIASTREAM_MIC = 1 << 12, MEDIASTREAM_CAMERA = 1 << 11, ... (see cef).
// $promptId is CEF's opaque prompt id (unique within this browser).
// Return (handled, action):
//   handled == false -> DSL declines; C++ falls back to CEF default handling
//                       (chrome-style: native bubble; alloy-style: IGNORE,
//                        which means the JS Promise never resolves).
//   handled == true  -> action selects the outcome:
//                         0 = default (equivalent to handled=false),
//                         1 = accept (silently grant),
//                         2 = deny   (silently deny).
// Default: silently accept Notifications so alloy windows can toast without
// UI prompts (Chromium's platform bridge still delivers the OS toast); every
// other permission is declined so the standard Chromium bubble shows in
// chrome-style windows (alloy will IGNORE, which is the pre-existing
// behaviour before this handler existed).
script(on_show_permission_prompt)params($promptId,$origin,$requested)
{
    nativelog("[dsl] on_show_permission_prompt: prompt_id={0}, origin={1}, requested={2}", $promptId, $origin, $requested);
    // CEF_PERMISSION_TYPE_NOTIFICATIONS = 1 << 15 = 0x8000 = 32768
    if($requested == 32768){
        return((true, 1));
    };
    return((false, 0));
};

// Called on the CEF UI thread on any SSL / certificate error before the
// interstitial page is shown. $certError is a Chromium net error code
// (e.g. -200 = ERR_CERT_COMMON_NAME_INVALID, -201 = ERR_CERT_DATE_INVALID,
// -202 = ERR_CERT_AUTHORITY_INVALID).
// Return (handled, action):
//   handled == false -> DSL declines; Chromium shows its default interstitial.
//   handled == true  -> action selects the outcome:
//                         0 = default (fall back to the interstitial),
//                         1 = Continue (silently proceed despite the error),
//                         2 = Cancel   (silently cancel without an interstitial).
// Default implementation always declines so the user sees the standard UI.
script(on_certificate_error)params($certError,$requestUrl)
{
    nativelog("[dsl] on_certificate_error: cert_error={0}, url={1}", $certError, $requestUrl);
    return((false, 0));
};

script(on_receive_cef_message)params($msg,$args,$srcProcId)
{
    nativelog("[dsl] on_receive_cef_message:{0} argnum:{1} from:{2} processtype:{3}",$msg,listsize($args),$srcProcId,processtype);
    if (processtype == 0) {
        //Browser: forward all cef messages back to renderer
        //Note: The API in AgentCore.dll cannot be used.
        nativeapi.SendCefMessage($msg,$args,$srcProcId);
    };
};

script(on_call_metadsl)params($func,$args)
{
    nativelog("[dsl] on_call_metadsl: func={0}, args={1}", $func, to_json($args));
};

script(on_browser_hot_reload_copyfiles)params($url)
{
    nativelog("[dsl] on_browser_hot_reload_copyfiles called, url: {0}", $url);
    return(false);
};

// Called after browser hot reload completes (AgentCore.dll updated)
script(on_browser_hot_reload_completed)params($url)
{
    nativelog("[dsl] on_browser_hot_reload called - AgentCore.dll has been reloaded, url: {0}", $url);
    nativelog("[dsl] Browser window was closed, DLL updated, and window reopened");

    // You can add initialization logic here after hot reload
    // For example: reload configuration, reinitialize state, etc.
};

// Called when browser receives a CEF query (browser process, main/UI thread).
// This is also the return path for custom JS dialogs: window.cefQuery is the
// only renderer -> browser channel available here (callMetaDSL is routed to the
// renderer side DSL, which cannot see handles registered in this process).
// $handle is the parked CefMessageRouterBrowserSide::Callback.
// Return (taken_over, result):
//   taken_over == false -> answered synchronously; result 0 sends Success("OK"),
//                       any other value sends Failure(result, ...). This is also
//                       the safe default, so a script error cannot hang a query.
//   taken_over == true  -> the query is taken over; nothing reaches the page
//                       until complete_native_callback($handle, ok, response
//                       [, code]) is called. ok=true sends Success(response),
//                       ok=false sends Failure(code, response).
// Persistent queries ($persistent == true): $handle stays valid across ok=true
//                       completions (each one pushes another onSuccess to the
//                       page) and never times out; ok=false cancels the query.
// Safety nets if a taken over query is never completed: CEF cancels it on
// navigation / renderer termination / window.cefQueryCancel (the entry is then
// discarded and on_browser_cef_query_canceled is notified), a non-persistent
// query expires in the native registry after 60s, and everything the browser
// owns is released when it closes.
script(on_browser_cef_query)params($query_id, $request, $persistent, $handle)
{
    nativelog("[dsl] on_browser_cef_query called - query_id: {0}, request: {1}, persistent: {2}, handle: {3}", $query_id, $request, $persistent, $handle);

    $msg = dev_tools_parse_bytes($request);
    if ($msg != null) {
        $action = $msg["action"];
        if ($action == "js_dialog_result") {
            // { action, handle (string), ok (bool), input (string) }
            // Note: $msg["handle"] is the JS dialog handle, not $handle.
            complete_native_callback($msg["handle"], $msg["ok"], $msg["input"]);
            return((false, 0));
        }
        elif ($action == "js_dialog_unavailable") {
            // The page has no AgentDialog implementation: cancel the dialog so
            // the pending confirm() cannot hang.
            nativelog("[dsl] js dialog UI unavailable, canceling handle {0}", $msg["handle"]);
            complete_native_callback($msg["handle"], false, "");
            return((false, 0));
        }
        // ---- relay transport (relay_transport.js -> wsclient -> AgentCore) ----
        elif ($action == "agent_register") {
            // { action, urlKey, agentId } -> new wsclient connection to
            // AgentCore, the wsclient id is the relay client id. The answer
            // is deferred until on_wsclient_state reports connected (the
            // connect itself is asynchronous), so the js side never races a
            // send. agentId (RelaySlot): stored so the browser dsl re-sends
            // the agent_bind envelope itself on every (re)connect - this
            // path does not depend on page pushes, so it cannot be lost.
            $urlKey = $msg["urlKey"];
            // "" id -> auto id; 10 = link-layer retry budget (same id).
            $clientId = wsclient_open(@AgentWsUrl, "", 10);
            if ($clientId == "") {
                return((false, -1));
            };
            set_context_var("relayUrl_" + $clientId, $urlKey);
            $agentId = $msg["agentId"];
            if (!isnullorempty($agentId)) {
                set_context_var("relayAgent_" + $clientId, $agentId);
            };
            set_context_var("relayPending_" + $clientId, $handle);
            // Owner browser id for the zombie reaper (see on_heart_beat).
            set_context_var("relayBrowser_" + $clientId, find_browser_id_by_url_key($urlKey));
            return((true, 0));
        }
        elif ($action == "agent_send") {
            // { action, clientId, message } -> forward over the wsclient.
            // Fire-and-forget: responses and pushes come back through
            // on_wsclient_message -> relay_push_to_page.
            $ok = wsclient_send($msg["clientId"], $msg["message"]);
            if ($ok) {
                return((false, 0));
            };
            return((false, -2));
        }
        elif ($action == "agent_unregister") {
            // { action, clientId } -> close the relay connection.
            wsclient_close($msg["clientId"]);
            remove_context_var("relayUrl_" + $msg["clientId"]);
            remove_context_var("relayAgent_" + $msg["clientId"]);
            remove_context_var("relayBrowser_" + $msg["clientId"]);
            return((false, 0));
        }
        elif ($action == "agent_state") {
            // { action, clientId } -> current wsclient state string
            // (connecting|connected|reconnecting|disconnected|failed|unknown).
            // Pull-based resync for the page: heals a stuck local state when
            // a relay_state push was lost (see the relay_transport.js
            // watchdog). Answered synchronously via a taken-over callback.
            $state = wsclient_state($msg["clientId"]);
            complete_native_callback($handle, true, $state);
            return((true, 0));
        }
        elif ($action == "lite_open") {
            // { action, connId, url, urlKey } -> one browser-side upstream
            // wsclient connection (relay_lite / relay_ws migration: no page js
            // direct websockets anymore). Pure dumb pipe: the page module
            // owns the wire protocol (auth/register etc. go through the
            // existing agent_send action once the connection is up). The
            // connection reuses the relay tables, so the zombie reaper,
            // renderer-terminated cleanup and relay_push_to_page routing all
            // work unchanged. Empty url falls back to the WeChat bridge
            // default (@LiteBridgeUrl).
            $cid = $msg["connId"];
            $url = $msg["url"];
            $urlKey = $msg["urlKey"];
            if (isnullorempty($cid) || isnull($urlKey)) {
                return((false, -1));
            };
            if (isnullorempty($url)) {
                $url = @LiteBridgeUrl;
            };
            set_context_var("relayUrl_" + $cid, $urlKey);
            set_context_var("relayBrowser_" + $cid, find_browser_id_by_url_key($urlKey));
            $state = wsclient_state($cid);
            if ($state == "connected" || $state == "connecting") {
                // Already up (page refresh race): answer immediately.
                complete_native_callback($handle, true, $cid);
                return((false, 0));
            };
            if ($state == "disconnected" || $state == "failed") {
                // Stale entry from a previous connection: release the id first.
                wsclient_close($cid);
            };
            $newId = wsclient_open($url, $cid);
            if ($newId == "") {
                remove_context_var("relayUrl_" + $cid);
                remove_context_var("relayBrowser_" + $cid);
                return((false, -1));
            };
            set_context_var("relayPending_" + $cid, $handle);
            return((true, 0));
        };
    };

    // Not handled here: answer synchronously with a failure.
    return((false, -1));
};

// Pure notification (browser process, UI thread): a query that
// on_browser_cef_query took over ($handle) was canceled from elsewhere -
// window.cefQueryCancel, navigation, renderer termination or browser close.
// The native side has already discarded $handle; drop any state kept for it
// (e.g. stop a subscription feed). Do NOT call complete_native_callback on it,
// that would be a no-op. No return value.
script(on_browser_cef_query_canceled)params($query_id, $handle)
{
    nativelog("[dsl] on_browser_cef_query_canceled - query_id: {0}, handle: {1}", $query_id, $handle);
};

// DevTools observer callbacks (browser process only, fired on UI thread).
// $bytes is a managed byte[] holding a UTF-8 JSON CDP payload; use
// dev_tools_parse_bytes($bytes) to get a dict/list/primitive tree.

// Raw CDP message received from the agent. Return non-zero to swallow it
// (prevent CEF default handling). Default: 0 = let CEF process normally.
// The active browser is the one C# set via SetContext (see on_dev_tools_* entry).
script(on_dev_tools_message)params($bytes)
{
    //nativelog("[dsl] on_dev_tools_message");
    return(0);
};

// Result of a previous ExecuteDevToolsMethod call, matched by $message_id.
script(on_dev_tools_method_result)params($message_id, $success, $bytes)
{
    nativelog("[dsl] on_dev_tools_method_result: message_id={0} success={1}", $message_id, $success);
};

// Unsolicited CDP event from the agent (e.g. Network.responseReceived).
script(on_dev_tools_event)params($method, $bytes)
{
    //nativelog("[dsl] on_dev_tools_event: method={0}", $method);
};

// DevTools agent attached to the browser (CDP channel is ready).
script(on_dev_tools_agent_attached)
{
    nativelog("[dsl] on_dev_tools_agent_attached");
};

// DevTools agent detached from the browser.
script(on_dev_tools_agent_detached)
{
    nativelog("[dsl] on_dev_tools_agent_detached");
};

script(get_user_name)
{
    if (ismac) {
        return(getenv("USER"));
    }
    else {
        return(getenv("USERNAME"));
    };
};
