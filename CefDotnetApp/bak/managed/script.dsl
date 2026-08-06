// Note:The design philosophy behind these DSL scripts is to be stateless;
// all state resides within C# or JS. The DSL's global variables are utilized
// for configuring constants, and each hot-reload operation executes independently.
script(init_global_consts)
{
    // Initialize global constants for browser here
    setenv("PLAYWRIGHT_DRIVER_SEARCH_PATH", combinepath(basepath, "managed"));
};
script(on_init)
{
    nativelog("[dsl] on_init finish");
    fileecho(true);
    //no-sandbox = false
    return(false);
};
script(on_finalize)
{
    nativelog("[dsl] on_finalize finish");
};

script(on_browser_init)
{
    nativelog("[dsl] on_browser_init finish");
    $browser = nativeapi.GetBrowser();
    if (!isnull($browser)) {
        $browser.SetCspBypass(true);
    };
};
script(on_browser_finalize)
{
    nativelog("[dsl] on_browser_finalize finish");
};

script(on_heart_beat)params($processType,$deltaTime)
{
    // Do something every heart beat
    if ($processType == 0) {
        handle_thread_queue();
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

    nativelog("[dsl] on_before_command_line_processing: process_type={0}, url={1}", $processType, $url);

    if (stringcontainsany($url, "file:///", "http://localhost") && stringcontainsany($url, "AgentCore/hotreload_test.html", "http://localhost:8080/agent.html", "http://localhost:8081")) {
        $cmdLine.AppendSwitch("disable-web-security");
        $cmdLine.AppendSwitch("allow-file-access-from-files");
    }
    elif (stringcontainsany($url, "https://evaluation.woa.com/chat", "https://gemini.google.com/app")) {
        $cmdLine.AppendSwitch("disable-web-security");
    };
    //$cmdLine.AppendSwitch("disable-web-security");
    //$cmdLine.AppendSwitch("allow-file-access-from-files");
    //$cmdLine.AppendSwitch("disable-site-isolation-trials");
    // Prevent throttling/priority reduction when window is minimized or in background
    $cmdLine.AppendSwitch("disable-background-timer-throttling");
    $cmdLine.AppendSwitch("disable-renderer-backgrounding");
    $cmdLine.AppendSwitch("disable-backgrounding-occluded-windows");

    // Override user-agent-product to look like standard Chrome
    $cmdLine.AppendSwitchWithValue("user-agent-product", "Chrome/150.0.7871.187");

    $platform = osplatform();
    nativelog("[dsl] on_before_command_line_processing platform:{0}", $platform);
    if (stringcontains($platform,"Win32")) {
        $cmdLine.AppendSwitch("hide-frame");
        $cmdLine.AppendSwitch("hide-top-menu");

        nativelog("[dsl] add hide-frame hide-top-menu");
    };
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
script(on_before_resource_load)params($request)
{
    nativelog("[dsl] on_before_resource_load: url={0} method={1}", $request.Url, $request.Method);
    return((false, 1));
};

// Note: this function will be called on the browser process IO thread.
// Resource interception hook.
// Decision mode (GetResourceHandler): $response is an empty writable
// CefResponse; fill header overrides into it and return (true, ...) to
// intercept the resource with MyResourceHandler. Header overrides are merged
// onto the upstream response headers (same-name overwrite; empty value means
// delete).
// Inspection mode (GetResourceResponseFilter): $response is the actual
// upstream response (read-only; mutations are silently dropped by CEF).
// Return (handled, replace_content):
//   handled: decision mode = intercept the resource; inspection mode =
//   register MyResponseFilter for body filtering.
//   replace_content: whether to enable body filtering via
//   on_response_content_filter. false = skip body filter (inspection mode:
//   don't register MyResponseFilter; decision mode: MyResourceHandler only
//   applies header overrides, passes body through unchanged). true = enable
//   body filter (default).
script(on_resource_response_filter)params($request, $response)
{
    nativelog("[dsl] on_resource_response_filter: url={0}", $request.Url);
    // Diagnostic bypass: let chromium's native stack handle common static
    // assets (js/css/images/fonts). If the SSO site renders correctly after
    // this change, our custom-handler forwarding path is losing auth cookies
    // (either Set-Cookie ingestion or UR_FLAG_ALLOW_STORED_CREDENTIALS).
    // Note: substring match is intentionally loose; version-query strings
    // like "vue.min.js?v=xxx" are still covered.
    if (stringcontainsany($request.Url, "account", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", ".woff", ".woff2", ".ttf")) {
        return((false, false));
    };
    if ($response.IsReadOnly) {
        nativelog("[dsl] on_resource_response_filter: {0}", $response.HeaderMap);
    }
    else {
        $response.AddPendingRemoveHeaderByName("Content-Security-Policy");
        return((true, false));
    };
    // Example: intercept evaluation.woa.com and strip CSP headers.
    // if (strcontains($request.Url, "evaluation.woa.com")) {
    //     $response.AddPendingRemoveHeaderByName("Content-Security-Policy");
    //     $response.AddPendingRemoveHeaderByName("Content-Security-Policy-Report-Only");
    //     return((true, true));
    // };
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
    return((true, ""));
};

script(on_receive_cef_message)params($msg,$args,$srcProcId)
{
    nativelog("[dsl] on_receive_cef_message:{0} argnum:{1} from:{2} processtype:{3}",$msg,listsize($args),$srcProcId,processtype);
    if (processtype == 0) {
        //Browser: forward all cef messages back to renderer
        //Note: The API in AgentCore.dll cannot be used.
        nativeapi.SendCefMessageForDSL($msg,$args,$srcProcId);
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

// Called when browser receives a CEF query
script(on_browser_cef_query)params($query_id, $request, $persistent)
{
    nativelog("[dsl] on_browser_cef_query called - query_id: {0}, request: {1}, persistent: {2}", $query_id, $request, $persistent);

    // Return 0 to indicate success
    // Return non-zero error code to indicate failure
    return(-1);
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
