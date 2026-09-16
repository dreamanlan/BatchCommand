// Note:The design philosophy behind these DSL scripts is to be stateless;
// all state resides within C# or JS. The DSL's global variables are utilized
// for configuring constants, and each hot-reload operation executes independently.
script(init_global_consts)
{
    setenv("PLAYWRIGHT_DRIVER_SEARCH_PATH", combinepath(basepath, "managed"));

    if (ismac) {
        @UserName = getenv("USER");
    }
    else {
        @UserName = getenv("USERNAME");
        if (isnullorempty(@UserName)) {
            @UserName = getfilename(getdirectoryname(getdirectoryname(getenv("LOCALAPPDATA"))));
        };
    };
};
script(on_init)
{
    nativelog("[dsl] on_init finish, pid:{0}, type:{1}", pid(), processtype);
    fileecho(true);
    // no-sandbox = false
    // Always return false in the renderer.
    return(false);
};
script(on_finalize)
{
    nativelog("[dsl] on_finalize finish, pid:{0}, type:{1}", pid(), processtype);
};

// Default handler for the C# hot reload watcher (watch_file / watch_dir):
// invoked unconditionally on watched file changes; the dsl reloads itself.
// THREAD CAVEAT: may run on any .NET threadpool thread with a worker-style
// interpreter — keep handlers light and stateless.
script(on_file_changed)params($filePath, $fileType)
{
    nativelog("[dsl] on_file_changed: {0} ({1})", $filePath, $fileType);
    return((true, ""));
};

script(on_renderer_init)params($url)
{
    // on_renderer_init runs AFTER LowerToken() (full sandbox lockdown: deny-only SIDs + LOW integrity).
    // The earlier bootstrap window (initial token, where .NET + DSL were first loaded) is gone;
    // use cefQuery/JsBridge for any file access now.
    nativelog("[dsl] on_renderer_init finish, url: {0}", $url);
};
script(on_renderer_finalize)
{
    nativelog("[dsl] on_renderer_finalize finish");
};
script(on_heart_beat)params($processType,$deltaTime)
{
    // Renderer process: ensure context points to the correct browser/frame
    if ($processType == 1) {
        $targetBrowserId = get_context_var("TargetBrowserId");
        if (isnull($targetBrowserId) || $targetBrowserId <= 0) {
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
        //debuggerlaunch();
    };

    $url = $cmdLine.GetSwitchValue("url");

    nativelog("[dsl] on_before_command_line_processing: process_type={0}, url={1}, pid={2}", $processType, $url, pid());

};


// ----------------------------------------------------------------------------
// inj_read: fresh read while the renderer can still read files; under the
// sandbox read_file returns null and the copy cached at process startup (see
// cache_inject_sources in script_renderer.dsl, the initially loaded dsl) is
// used instead. Context vars live on the shared DslHost, so the cache set
// before the setdslfile switch is visible here.
// ----------------------------------------------------------------------------

script(inj_read)params($name)
{
    $fresh = read_file(combine_path(basepath, "managed/inject_modules/" + $name));
    if (!isnull($fresh) && $fresh != "") {
        return($fresh);
    };
    $v = get_context_var("inj_" + $name);
    if (!isnull($v) && $v != "") {
        return($v);
    };
    return("");
};

script(on_renderer_load_start)params($url,$transitionType,$isMainFrame)
{
    nativelog("[dsl] on_renderer_load_start:{0} {1} {2}", $url, $transitionType, $isMainFrame);
};
script(on_renderer_load_end)params($url,$httpStatusCode,$isMainFrame)
{
    nativelog("[dsl] on_renderer_load_end:{0} {1} {2}", $url, $httpStatusCode, $isMainFrame);
    if (string_contains_any($url, "https://hyarena.woa.com/chat") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $sb = new_string_builder();
        append_line($sb, inj_read("hyarena_opus.js"));
        append_line($sb, inj_read("relay_transport.js"));
        append_line($sb, inj_read("relay_agent_lite.js"));
        $code = string_builder_to_string($sb);
        nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://ai.woa.com/#/chat") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $sb = new_string_builder();
        append_line($sb, inj_read("venus_llm.js"));
        append_line($sb, inj_read("relay_transport.js"));
        append_line($sb, inj_read("relay_agent_lite.js"));
        $code = string_builder_to_string($sb);
        nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://imate.woa.com/chat") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $sb = new_string_builder();
        append_line($sb, inj_read("imate_llm.js"));
        append_line($sb, inj_read("relay_transport.js"));
        append_line($sb, inj_read("relay_agent_lite.js"));
        $code = string_builder_to_string($sb);
        nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://with.woa.com/chats") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $sb = new_string_builder();
        append_line($sb, inj_read("with_llm.js"));
        append_line($sb, inj_read("relay_transport.js"));
        append_line($sb, inj_read("relay_agent_lite.js"));
        $code = string_builder_to_string($sb);
        nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://gemini.google.com/app") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $sb = new_string_builder();
        append_line($sb, inj_read("google_gemini.js"));
        append_line($sb, inj_read("relay_transport.js"));
        append_line($sb, inj_read("relay_agent_lite.js"));
        $code = string_builder_to_string($sb);
        nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://chatgpt.com", "https://chat.openai.com") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $sb = new_string_builder();
        append_line($sb, inj_read("openai_chat.js"));
        append_line($sb, inj_read("relay_transport.js"));
        append_line($sb, inj_read("relay_agent_lite.js"));
        $code = string_builder_to_string($sb);
        nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://www.google.com/ai", "https://www.google.com/search") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $sb = new_string_builder();
        append_line($sb, inj_read("google_ai_search.js"));
        append_line($sb, inj_read("relay_transport.js"));
        append_line($sb, inj_read("relay_agent_lite.js"));
        $code = string_builder_to_string($sb);
        nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    return((false, ""));
};
script(on_renderer_loading_state_change)params($url,$isLoading,$canGoBack,$canGoForward)
{
    nativelog("[dsl] on_renderer_loading_state_change: url={0}, isLoading={1}, canGoBack={2}, canGoForward={3}", $url, $isLoading, $canGoBack, $canGoForward);
};
script(on_renderer_load_error)params($errorCode,$errorText,$failedUrl)
{
    nativelog("[dsl] on_renderer_load_error:{0} {1} {2}", $errorCode, $errorText, $failedUrl);
};
script(on_render_process_terminated)params($startupUrl,$url,$status,$errorCode,$errorString)
{
    nativelog("[dsl] on_render_process_terminated: startup_url={0}, url={1}, status={2}, error_code={3}, error_string={4}", $startupUrl, $url, $status, $errorCode, $errorString);
};

//$args is a string list
script(on_receive_cef_message)params($msg,$args,$srcProcId)
{
    nativelog("[dsl] on_receive_cef_message:{0} argnum:{1} from:{2} processtype:{3}",$msg,listsize($args),$srcProcId,processtype);
    if (processtype == 1) {
        //Renderer
        if (funcexists($msg)) {
            redirectcall($msg, $args);
        }
        elif (string_contains($msg, ".")) {
            //ignore messages with dot in name, which are likely from cef
        }
        else {
            redirectcall("handle_" + $msg, $args);
            //nativeapi.SendJavascriptCall("alert", [$msg]);
        };
    };
};

//$args is a string list
script(on_call_metadsl)params($func,$args)
{
    nativelog("[dsl] on_call_metadsl: func={0}, args={1}", $func, to_json($args));
};

script(get_user_name)
{
    if (ismac) {
        return(getenv("USER"));
    }
    else {
        $userName = getenv("USERNAME");
        if (isnullorempty($userName)) {
            return(getfilename(getdirectoryname(getdirectoryname(getenv("LOCALAPPDATA")))));
        };
    };
};
