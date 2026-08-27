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

    if (stringcontainsany($url, "file:///", "http://localhost") && stringcontainsany($url, "AgentCore/hotreload_test.html", "http://localhost:8080/agent.html", "http://localhost:8081", "http://localhost:8082")) {
        $cmdLine.AppendSwitch("disable-web-security");
        $cmdLine.AppendSwitch("allow-file-access-from-files");
    }
    elif (stringcontainsany($url, "https://evaluation.woa.com/chat", "https://www.google.com/ai", "https://www.google.com/search", "https://gemini.google.com/app")) {
        $cmdLine.AppendSwitch("disable-web-security");
    }
    elif (stringcontainsany($url, "gamexyz.net")) {
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
    if (stringcontainsany($request.Url, "gamexyz.net:8080/proxysite/", "www.google.com/ai", "www.google.com/search", "gemini.google.com", "chatgpt.com", "chat.openai.com")) {
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

// Called on the browser process IO thread when a target host (or proxy)
// requests HTTP authentication credentials.
// Return (handled, user, pass):
//   handled == false -> DSL declines; Chromium shows its native login prompt.
//   handled == true  -> silently use the returned credentials.
// The default implementation always declines so that the user is prompted.
script(on_get_auth_credentials)params($isProxy,$host,$port,$realm,$scheme,$originUrl)
{
    nativelog("[dsl] on_get_auth_credentials: isProxy={0}, host={1}, port={2}, realm={3}, scheme={4}, origin={5}", $isProxy, $host, $port, $realm, $scheme, $originUrl);
    // Supply hard-coded credentials for the outbound proxy. Only apply to
    // proxy challenges; leave target-server auth (e.g. site logins) alone so
    // the user is still prompted for those.
    if($isProxy){
        //return((true, "dreaman", "nopasswd"));
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
// Default implementation always declines so that the user is prompted.
script(on_request_media_access_permission)params($origin,$requested,$menuDisabled)
{
    nativelog("[dsl] on_request_media_access_permission: origin={0}, requested={1}, menu_disabled={2}", $origin, $requested, $menuDisabled);
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
// Safety nets if a taken over query is never completed: CEF cancels it on
// navigation / renderer termination / window.cefQueryCancel (the entry is then
// discarded), the native registry expires it after 60s, and everything the
// browser owns is released when it closes.
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
        };
    };

    // Not handled here: answer synchronously with a failure.
    return((false, -1));
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
