// Note:The design philosophy behind these DSL scripts is to be stateless;
// all state resides within C# or JS. The DSL's global variables are utilized
// for configuring constants, and each hot-reload operation executes independently.
script(init_global_consts)
{
    set_env("PLAYWRIGHT_DRIVER_SEARCH_PATH", combine_path(base_path, "managed"));

    if (is_mac) {
        @UserName = get_env("USER");
    }
    else {
        @UserName = get_env("USERNAME");
        if (is_null_or_empty(@UserName)) {
            @UserName = get_file_name(get_directory_name(get_directory_name(get_env("LOCALAPPDATA"))));
        };
    };
};
script(on_init)
{
    native_log("[dsl] on_init finish, pid:{0}, type:{1}", pid(), process_type);
    file_echo(true);
    // no-sandbox = false
    // Always return false in the renderer.
    return(false);
};
script(on_finalize)
{
    native_log("[dsl] on_finalize finish, pid:{0}, type:{1}", pid(), process_type);
};

// Default handler for the C# hot reload watcher (watch_file / watch_dir):
// invoked unconditionally on watched file changes; the dsl reloads itself.
// THREAD CAVEAT: may run on any .NET threadpool thread with a worker-style
// interpreter — keep handlers light and stateless.
script(on_file_changed)params($filePath, $fileType)
{
    native_log("[dsl] on_file_changed: {0} ({1})", $filePath, $fileType);
    return((true, ""));
};

script(on_renderer_init)params($url)
{
    // on_renderer_init runs AFTER LowerToken() (full sandbox lockdown: deny-only SIDs + LOW integrity).
    // The earlier bootstrap window (initial token, where .NET + DSL were first loaded) is gone;
    // use cefQuery/JsBridge for any file access now.
    native_log("[dsl] on_renderer_init finish, url: {0}", $url);
};
script(on_renderer_finalize)
{
    native_log("[dsl] on_renderer_finalize finish");
};

// The renderer process id for the inject side. The sandboxed renderer cannot
// read its own process handle, so get_process_memory() returns 0 here and the
// memory guard reading lives in script.dsl on the browser process (the
// renderer_memory cefQuery action); this script only hands the pid over so
// the js side can address it.
script(get_self_pid)
{
    return(pid());
};

script(on_heartbeat)params($processType,$deltaTime)
{
    // Renderer process: ensure context points to the correct browser/frame
    if ($processType == 1) {
        $targetBrowserId = get_context_var("TargetBrowserId");
        if (is_null($targetBrowserId) || $targetBrowserId <= 0) {
            $targetBrowserId = find_browser_id_by_url_key("evaluation.woa.com/chat");
            if ($targetBrowserId <= 0) {
                $targetBrowserId = find_browser_id_by_url_key("localhost:8080");
            };
            if ($targetBrowserId > 0) {
                set_context_var("TargetBrowserId", $targetBrowserId);
            };
        };
        if ($targetBrowserId > 0) {
            if (!set_context_by_id($targetBrowserId)) {
                // Browser no longer valid, reset cache to re-search next heartbeat
                set_context_var("TargetBrowserId", 0);
            };
        };
        handle_thread_queue();
    };
};

script(on_before_command_line_processing)params($processType, $cmdLine)
{
    // At this point, the sandbox has not yet fully taken effect, so files can still be accessed.
    if ($processType == 1) {
        //debugger_launch();
    };

    $url = $cmdLine.GetSwitchValue("url");

    native_log("[dsl] on_before_command_line_processing: process_type={0}, url={1}, pid={2}", $processType, $url, pid());

};


// ----------------------------------------------------------------------------
// inj_read: fresh read while the renderer can still read files; under the
// sandbox read_file returns null and the copy cached at process startup (see
// cache_inject_sources in script_renderer.dsl, the initially loaded dsl) is
// used instead. Context vars live on the shared DslHost, so the cache set
// before the set_dsl_file switch is visible here.
// ----------------------------------------------------------------------------

script(inj_read)params($name)
{
    $fresh = read_file(combine_path(base_path, "managed/inject_modules/" + $name));
    if (!is_null($fresh) && $fresh != "") {
        return($fresh);
    };
    $v = get_context_var("inj_" + $name);
    if (!is_null($v) && $v != "") {
        return($v);
    };
    return("");
};

script(on_renderer_load_start)params($url,$transitionType,$isMainFrame)
{
    native_log("[dsl] on_renderer_load_start:{0} {1} {2}", $url, $transitionType, $isMainFrame);
};
script(on_renderer_load_end)params($url,$httpStatusCode,$isMainFrame)
{
    native_log("[dsl] on_renderer_load_end:{0} {1} {2}", $url, $httpStatusCode, $isMainFrame);
    if (string_contains_any($url, "https://hyarena.woa.com/chat") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $sb = new_string_builder();
        append_line($sb, inj_read("hyarena_opus.js"));
        append_line($sb, inj_read("relay_transport.js"));
        append_line($sb, inj_read("relay_agent_lite.js"));
        $code = string_builder_to_string($sb);
        native_log("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://ai.woa.com/#/chat") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $sb = new_string_builder();
        append_line($sb, inj_read("venus_llm.js"));
        append_line($sb, inj_read("relay_transport.js"));
        append_line($sb, inj_read("relay_agent_lite.js"));
        $code = string_builder_to_string($sb);
        native_log("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://vedas.woa.com/#/file-preview")) {
        $sb = new_string_builder();
        append_line($sb, inj_read("vedas_responder.js"));
        $code = string_builder_to_string($sb);
        native_log("[dsl] on_renderer_load_end: injecting vedas responder {0} bytes", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://imate.woa.com/chat") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $sb = new_string_builder();
        append_line($sb, inj_read("imate_llm.js"));
        append_line($sb, inj_read("relay_transport.js"));
        append_line($sb, inj_read("relay_agent_lite.js"));
        $code = string_builder_to_string($sb);
        native_log("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://with.woa.com/chats") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $sb = new_string_builder();
        append_line($sb, inj_read("with_llm.js"));
        append_line($sb, inj_read("relay_transport.js"));
        append_line($sb, inj_read("relay_agent_lite.js"));
        $code = string_builder_to_string($sb);
        native_log("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://gemini.google.com/app") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $sb = new_string_builder();
        append_line($sb, inj_read("google_gemini.js"));
        append_line($sb, inj_read("relay_transport.js"));
        append_line($sb, inj_read("relay_agent_lite.js"));
        $code = string_builder_to_string($sb);
        native_log("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://chatgpt.com", "https://chat.openai.com") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $sb = new_string_builder();
        append_line($sb, inj_read("openai_chat.js"));
        append_line($sb, inj_read("relay_transport.js"));
        append_line($sb, inj_read("relay_agent_lite.js"));
        $code = string_builder_to_string($sb);
        native_log("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://www.google.com/ai", "https://www.google.com/search") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $sb = new_string_builder();
        append_line($sb, inj_read("google_ai_search.js"));
        append_line($sb, inj_read("relay_transport.js"));
        append_line($sb, inj_read("relay_agent_lite.js"));
        $code = string_builder_to_string($sb);
        native_log("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    return((false, ""));
};
script(on_renderer_loading_state_change)params($url,$isLoading,$canGoBack,$canGoForward)
{
    native_log("[dsl] on_renderer_loading_state_change: url={0}, isLoading={1}, canGoBack={2}, canGoForward={3}", $url, $isLoading, $canGoBack, $canGoForward);
};
script(on_renderer_load_error)params($errorCode,$errorText,$failedUrl)
{
    native_log("[dsl] on_renderer_load_error:{0} {1} {2}", $errorCode, $errorText, $failedUrl);
};
script(on_render_process_terminated)params($startupUrl,$url,$status,$errorCode,$errorString)
{
    native_log("[dsl] on_render_process_terminated: startup_url={0}, url={1}, status={2}, error_code={3}, error_string={4}", $startupUrl, $url, $status, $errorCode, $errorString);
};

//$args is a string list
script(on_receive_cef_message)params($msg,$args,$srcProcId)
{
    native_log("[dsl] on_receive_cef_message:{0} arg_num:{1} from:{2} process_type:{3}",$msg,list_size($args),$srcProcId,process_type);
    if (process_type == 1) {
        //Renderer
        if (func_exists($msg)) {
            redirect_call($msg, $args);
        }
        elif (string_contains($msg, ".")) {
            //ignore messages with dot in name, which are likely from cef
        }
        else {
            redirect_call("handle_" + $msg, $args);
            //native_api.SendJavascriptCall("alert", [$msg]);
        };
    };
};

//$args is a string list
script(on_call_metadsl)params($func,$args)
{
    native_log("[dsl] on_call_metadsl: func={0}, args={1}", $func, to_json($args));
};

script(get_user_name)
{
    if (is_mac) {
        return(get_env("USER"));
    }
    else {
        $userName = get_env("USERNAME");
        if (is_null_or_empty($userName)) {
            return(get_file_name(get_directory_name(get_directory_name(get_env("LOCALAPPDATA")))));
        };
    };
};
