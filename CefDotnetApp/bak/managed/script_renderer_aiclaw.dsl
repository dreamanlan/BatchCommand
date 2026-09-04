// Note:The design philosophy behind these DSL scripts is to be stateless;
// all state resides within C# or JS. The DSL's global variables are utilized
// for configuring constants, and each hot-reload operation executes independently.
script(init_global_consts)
{
    setenv("PLAYWRIGHT_DRIVER_SEARCH_PATH", combinepath(basepath, "managed"));

    @AgentPorts = hashtable("webagent":9527,"hyarena":9528,"venus":9529,"aichat":9530,"gemini":9531,"openai":9532,"imate":9533,"with":9534,"google":9535);

    @EnableLlmPM = false;
    //@LlmProviderId = "ollama";
    @LlmProviderId = "auto_metadsl";

    if (processtype == 1) {
    };
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

script(on_renderer_init)params($url)
{
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
    elif (stringcontainsany($url, "https://evaluation.woa.com/chat", ".woa.com/", "https://www.google.com/ai", "https://www.google.com/search", "https://gemini.google.com/app", "https://chatgpt.com", "https://chat.openai.com")) {
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

    //--disable-chrome-login-prompt --proxy-pac-url=http://www.gamexyz.net/google_proxy.pac --ignore-certificate-errors-spki-list=2jcZDMGiVyFnDdB4jNPPeNmF0Vwn+SZ4BddAfhVyeV4=
    $cmdLine.AppendSwitch("disable-chrome-login-prompt");
    $cmdLine.AppendSwitchWithValue("proxy-pac-url", "http://www.gamexyz.net/google_proxy.pac");
    $cmdLine.AppendSwitchWithValue("ignore-certificate-errors-spki-list", "2jcZDMGiVyFnDdB4jNPPeNmF0Vwn+SZ4BddAfhVyeV4=");

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

script(on_renderer_load_start)params($url,$transitionType,$isMainFrame)
{
    nativelog("[dsl] on_renderer_load_start:{0} {1} {2}", $url, $transitionType, $isMainFrame);
};
script(on_renderer_load_end)params($url,$httpStatusCode,$isMainFrame)
{
    nativelog("[dsl] on_renderer_load_end:{0} {1} {2}", $url, $httpStatusCode, $isMainFrame);
    if (string_contains_any($url, "https://hyarena.woa.com/chat") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $base = combine_path(basepath, "managed/inject_modules/");
        $sb = new_string_builder();
        append_line($sb, read_file(combine_path($base, "hyarena_opus.js")));
        $code = string_builder_to_string($sb);
        nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://ai.woa.com/#/chat") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $base = combine_path(basepath, "managed/inject_modules/");
        $sb = new_string_builder();
        append_line($sb, read_file(combine_path($base, "venus_llm.js")));
        $code = string_builder_to_string($sb);
        nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://imate.woa.com/chat") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $base = combine_path(basepath, "managed/inject_modules/");
        $sb = new_string_builder();
        append_line($sb, read_file(combine_path($base, "imate_llm.js")));
        $code = string_builder_to_string($sb);
        nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://with.woa.com/chats") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $base = combine_path(basepath, "managed/inject_modules/");
        $sb = new_string_builder();
        append_line($sb, read_file(combine_path($base, "with_llm.js")));
        $code = string_builder_to_string($sb);
        nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://gemini.google.com/app") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $base = combine_path(basepath, "managed/inject_modules/");
        $sb = new_string_builder();
        append_line($sb, read_file(combine_path($base, "google_gemini.js")));
        $code = string_builder_to_string($sb);
        nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "gamexyz.net:8080") && ($isMainFrame == "False" || $isMainFrame == false)) {
        $base = combine_path(basepath, "managed/inject_modules/");
        $sb = new_string_builder();
        append_line($sb, read_file(combine_path($base, "google_gemini.js")));
        $code = string_builder_to_string($sb);
        nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://chatgpt.com", "https://chat.openai.com") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $base = combine_path(basepath, "managed/inject_modules/");
        $sb = new_string_builder();
        append_line($sb, read_file(combine_path($base, "openai_chat.js")));
        $code = string_builder_to_string($sb);
        nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
        return((true, $code));
    };
    if (string_contains_any($url, "https://www.google.com/ai", "https://www.google.com/search") && ($isMainFrame == "True" || $isMainFrame == true)) {
        $base = combine_path(basepath, "managed/inject_modules/");
        $sb = new_string_builder();
        append_line($sb, read_file(combine_path($base, "google_ai_search.js")));
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
        return(getenv("USERNAME"));
    };
};
script(get_arena_system_prompt)params()
{
    $arenaPrompt = read_file(combine_path(basepath,"docs/arena_prompt.txt"));
    return(format("{0}",$arenaPrompt));
};
script(get_venus_system_prompt)params()
{
    $mergePrompt = read_file(combine_path(basepath,"docs/venus_prompt.txt"));
    return(format("{0}",$mergePrompt));
};
script(get_google_prompt_1)params()
{
    $googlePrompt = read_file(combine_path(basepath,"docs/google_prompt_1.txt"));
    return(format("{0}",$googlePrompt));
};
script(get_google_prompt_2)params()
{
    $googlePrompt = read_file(combine_path(basepath,"docs/google_prompt_2.txt"));
    return(format("{0}",$googlePrompt));
};
script(get_google_prompt_3)params()
{
    $googlePrompt = read_file(combine_path(basepath,"docs/google_prompt_3.txt"));
    return(format("{0}",$googlePrompt));
};
script(get_google_prompt)params()
{
    $googlePrompt = read_file(combine_path(basepath,"docs/google_prompt.txt"));
    return(format("{0}",$googlePrompt));
};
script(get_openai_prompt)params()
{
    $openAiPrompt = read_file(combine_path(basepath,"docs/openai_prompt.txt"));
    return(format("{0}",$openAiPrompt));
};
script(get_imate_prompt)params()
{
    $imatePrompt = read_file(combine_path(basepath,"docs/imate_prompt.txt"));
    return(format("{0}",$imatePrompt));
};
script(get_with_prompt)params()
{
    $withPrompt = read_file(combine_path(basepath,"docs/with_prompt.txt"));
    return(format("{0}",$withPrompt));
};
script(get_todo)params($agentId)
{
    // $agentId is a key of @AgentPorts (see init_global_consts), e.g. "imate".
    // Unlike the prompt files under basepath/docs, todo.txt lives in the
    // per-agent project directory set by agent_set_project_dir.
    $port = hashtableget(@AgentPorts, $agentId);
    $dir = agent_get_project_dir($port);
    $soulPrompt = read_file(combine_path($dir, "docs/soul.md"));
    $todoPrompt = read_file(combine_path($dir, "docs/todo.txt"));
    return(format("{0}\n\n{1}", $soulPrompt, $todoPrompt));
};

// Handle nativelog batch
script(handle_nativelog_batch)params($jsonData)
{
    $parsed = from_json($jsonData);
    if ($parsed == null) {
        nativelog("[dsl] Failed to parse batch");
        return("error");
    };

    // Extract batch fields
    $logs = get_message_param($parsed, "logs");
    $timestamp = get_message_param($parsed, "timestamp");

    nativelog("[dsl] Batch log: {0}, Logs count: {1}", $timestamp, size($logs));

    // Send each log entry to native log
    loop(size($logs)) {
        $i = $$;
        $logEntry = $logs[$i];
        $level = get_message_param($logEntry, "level");
        $message = get_message_param($logEntry, "message");
        nativelog("[dsl] Log entry: {0} [{1}]", $message, $level);
    };
};
// Handle MCP callback - forward MCP tool result to LLM as agent reply
// $serverId: MCP server id, $callbackTag: caller tag, $resultText: tool result
script(handle_mcp_callback)params($serverId, $callbackTag, $resultText)
{
    nativelog("[dsl] mcp_callback: server={0} tag={1} result_len={2}", $serverId, $callbackTag, strlen($resultText));
    $text = format("[MCP Tool Result] server={0} tag={1}\n{2}", $serverId, $callbackTag, $resultText);
    send_command_to_inject("send_message", to_json({text: $text}));
};

// Handle LLM callback (Ollama / OpenAI / Claude / AutoMetaDSL providers)
// $providerId: provider id, $tag: session tag, $topic: topic, $reply: full reply text
script(handle_llm_callback)params($providerId, $tag, $topic, $reply)
{
    nativelog("[dsl] llm_callback: provider={0} tag={1} topic={2} reply_len={3}", $providerId, $tag, $topic, strlen($reply));

    send_command_to_inject("send_message", to_json({text: $reply}));
};

script(handle_skill_callback)params($cmdStr, $argStr, $workDir, $resultText)
{
    nativelog("[dsl] skill_callback: cmd={0} arg={1} work_dir={2} result={3}", $cmdStr, $argStr, $workDir, getstringinlength($resultText,100));

    $text = format("[skill Result] cmd={0} arg={1} work_dir={2}\n{3}", $cmdStr, $argStr, $workDir, $resultText);
    send_command_to_inject("send_message", to_json({text: $text}));
};

script(handle_command_callback)params($cmdStr, $argStr, $workDir, $resultText)
{
    nativelog("[dsl] command_callback: cmd={0} arg={1} work_dir={2} result={3}", $cmdStr, $argStr, $workDir, getstringinlength($resultText,100));

    $text = format("[command Result] cmd={0} arg={1} work_dir={2}\n{3}", $cmdStr, $argStr, $workDir, $resultText);
    send_command_to_inject("send_message", to_json({text: $text}));
};

// Handle agent command
script(handle_agent_command)params($jsonData)
{
    nativelog("[dsl] handle_agent_command called with: {0}", getstringinlength($jsonData,100));

    // Parse the command using parse_agent_command API
    $cmd = parse_agent_command($jsonData);

    if ($cmd == null) {
        nativelog("[dsl] Failed to parse command");
        $errorResponse = build_agent_response(0, false, "", "Failed to parse command");
        send_response_to_inject($errorResponse);
        return("error");
    };

    // Extract command fields
    $id = get_message_param($cmd, "id");
    $command = get_message_param($cmd, "command");
    $params = get_message_param($cmd, "params");
    $timestamp = get_message_param($cmd, "timestamp");

    nativelog("[dsl] Command: {0}, ID: {1}", $command, $id);

    // Dispatch command to appropriate handler
    if ($command == "ping") {
        handle_ping_command($id, $params);
    }
    elif ($command == "handle_thread_queue") {
        handle_thread_queue();
    }
    elif ($command == "llm_chat") {
        handle_llm_chat_command($id, $params);
    }
    elif ($command == "set_agent_environment") {
        handle_set_agent_environment_command($id, $params);
    }
    elif ($command == "update_agent_configs") {
        handle_update_agent_configs_command($id, $params);
    }
    else {
        nativelog("[dsl] Unknown command: {0}", $command);
        $errorResponse = build_agent_response($id, false, "", format("Unknown command: {0}", $command));
        send_response_to_inject($errorResponse);
    };

    return("ok");
};

// Handle ping command
script(handle_ping_command)params($id, $params)
{
    nativelog("[dsl] Handling ping command, ID: {0}", $id);

    // Build success response with "pong" data
    $response = build_agent_response($id, true, "pong", "");
    send_response_to_inject($response);

    nativelog("[dsl] Ping response sent");
};

// Handle llm_set_provider command from inject.js config panel
// params: providerId, type, url, apiKey, model, authMode, username
script(handle_llm_chat_command)params($id, $params)
{
    nativelog("[dsl] Handling llm_chat command, ID: {0}", $id);

    $providerId = get_message_param($params, "providerId");
    $tag = get_message_param($params, "tag");
    $topic = get_message_param($params, "topic");
    $text = get_message_param($params, "text");

    nativelog("[dsl] llm_chat: provider={0} tag={1} topic={2} text_len={3}", $providerId, $tag, $topic, strlen($text));

    if (strlen($text) == 0) {
        $response = build_agent_response($id, false, "", "text is empty");
        send_response_to_inject($response);
        return("error");
    };

    $result = llm_chat_callback($providerId, $tag, $topic, $text);

    $response = build_agent_response($id, true, $result, "");
    send_response_to_inject($response);

    nativelog("[dsl] llm_chat sent: {0}", $result);
};

// Handle set_agent_environment command from inject.js config panel
// params: category, group, key, value
script(handle_set_agent_environment_command)params($id, $params)
{
    nativelog("[dsl] Handling set_agent_environment command, ID: {0}", $id);

    $category = get_message_param($params, "category");
    $group = get_message_param($params, "group");
    $key = get_message_param($params, "key");
    $value = get_message_param($params, "value");

    nativelog("[dsl] set_agent_environment: category={0} group={1} key={2}", $category, $group, $key);

    $result = set_agent_environment($category, $group, $key, $value);

    $response = build_agent_response($id, $result, "ok", "");
    send_response_to_inject($response);

    nativelog("[dsl] set_agent_environment result: {0}", $result);
};

// Handle update_agent_configs command - notified when agent environments are fully loaded
script(handle_update_agent_configs_command)params($id, $params)
{
    nativelog("[dsl] Handling update_agent_configs command, ID: {0}", $id);
    $response = build_agent_response($id, true, "ok", "");
    send_response_to_inject($response);

    // LLM providers configured here; apiKey uses %var% placeholders expanded via agent environment
    llm_set_provider("ollama", "ollama", "http://localhost:11434", "", "qwen3.8:27b");
    llm_set_provider("auto_metadsl", "auto_metadsl", "https://knot.woa.com/apigw/api/v1/agents/agui/%agent_id%", "%person_token%", "kimi-k2.7-code");

    // Search services configured here; apiKey uses %var% placeholders
    //brave_set_api_key("%brave_api_key%");
    searxng_set_engines("bing,yahoo,360search,baidu,sogou,quark");
    searxng_set_url("https://www.gamexyz.net:8090");
};

script(induction_freebie_info)params($batch, $infos, $session, $port)
{
    nativelog("[dsl] induction_freebie_info, batch: {0}, infos: {1}, session: {2}, port: {3}", $batch, count($infos), $session, $port);

    $operationQueueCount = str_to_int(nativeapi.CallJavascriptFuncInRenderer("window.MetaDSLBridge.getOperationQueueCount",[]));
    $sendQueueCount = str_to_int(nativeapi.CallJavascriptFuncInRenderer("window.MetaDSLBridge.getSendQueueCount",[]));
    $receiveQueueCount = str_to_int(nativeapi.CallJavascriptFuncInRenderer("window.MetaDSLBridge.getReceiveQueueCount",[]));
    $ct = $operationQueueCount + $receiveQueueCount;

    $histId = "_decurion_history";
    if ($session == "llm_pm_marquis") {
        $histId = "_marquis_history";
    }
    elif ($session == "llm_pm_chiliarch") {
        $histId = "_chiliarch_history";
    }
    elif ($session == "llm_pm_centurion") {
        $histId = "_centurion_history";
    }
    elif ($session == "llm_pm_decurion") {
        $histId = "_decurion_history";
    };

    if ($batch > 0) {
        $induction = new_string_builder();
    };
    loop($batch) {
        $i = $$;
        loop(10){
            $j = $i*10 + $$;
            append_line($induction, $infos[$j]);
        };

        if ($i + 1 < $batch) {
            $id = semantic_add(agent_get_project_identity($port) + $histId, '.', to_json({source: 'inject', date: date_time_str()}));

            nativelog("[dsl] induction_freebie_info, order: {0}, skip: {1}, history id: {2}", $i, $id, $histId);
        };
    };

    if ($batch > 0) {
        if ($ct > 0) {
            $id = semantic_add(agent_get_project_identity($port) + $histId, '.', to_json({source: 'inject', date: date_time_str()}));

            nativelog("[dsl] induction_freebie_info, order: {0}, skip: {1}, history id: {2}", $batch - 1, $id, $histId);
        }
        else {
            $maxLen = 30 * 1024;
            if ($port == 9535) {
                $maxLen = 4 * 1024;
            };
            $prompt = format("{0}\n\n以上是最近工作信息，请按以下规则归纳成一段话（一次回复输出完成，200字左右，不超过300字）：产出以关键词/名词短语流为主，可适当润色方便理解；只反映上述信息中已有的事实，不凭空生造未涉及的内容；能用已有关键词准确概括时优先复用，不能准确概括时允许提炼意义上的新词。然后使用`{1}`写到库里。\n\n至关重要：切勿遗漏变量名、路径或公式中的任何下划线（_）。请务必严格保持所有 snake_case 格式。", getstringinlength(to_pretty_string(string_builder_to_string($induction)), $maxLen, 1),
                format("semantic_add(agent_get_project_identity({0})+'{1}', [[归纳内容]], to_json({{source: 'inject', date: date_time_str()}}));", $port, $histId)
                );
            // Fallback when PM is disabled: just send "continue" to keep LLM moving.
            send_command_to_inject("send_message", to_json({text: $prompt}));

            nativelog("[dsl] induction_freebie_info, prompt: {0}, history id: {1}", get_string_in_length($prompt, 100), $histId);
        };
    };
};

script(trigger_freebie_reflection)params($port)
{
    nativelog("[dsl] trigger_reflection called");

    $projectDirectory = agent_get_project_dir($port);
    $projectIdentity = agent_get_project_identity($port);
    $legionnaireHistory = $projectIdentity + "_legionnaire_history";

    // Collect recent conversation history
    //$legionnaireHistory = getstringinlength(to_pretty_string(semantic_get_recent($legionnaireHistory, 20)), 30 * 1024, 1);
    $contextHistory = read_file(combine_path($projectDirectory, "docs/context.txt"));

    $prompt = format("{0}\n\n请结合你的上下文记忆，提取结构化的经验记录（300字以内），然后使用`{1}`写到库里。\n\n至关重要：切勿遗漏变量名、路径或公式中的任何下划线（_）。请务必严格保持所有 snake_case 格式。", $contextHistory,
        format("semantic_add(agent_get_project_identity({0})+'_episodic_memory', [[经验记录]], to_json({{source: 'reflection', date: date_time_str(), type: 'episodic'}}));", $port)
        );
    send_command_to_inject("send_message", to_json({text: $prompt}));

    nativelog("[dsl] trigger_reflection: reflection request sent");
};

script(save_freebie_context)params($count,$port)
{
    $projectDirectory = agent_get_project_dir($port);
    $projectIdentity = agent_get_project_identity($port);
    $marquisHistory = $projectIdentity + "_marquis_history";
    $chiliarchHistory = $projectIdentity + "_chiliarch_history";
    $centurionHistory = $projectIdentity + "_centurion_history";
    $decurionHistory = $projectIdentity + "_decurion_history";
    $legionnaireHistory = $projectIdentity + "_legionnaire_history";

    $contextFile = combine_path($projectDirectory, "docs/context.txt");
    // Load recent conversation history from semantic index
    $marquisHistory = semantic_get_recent_as_list($marquisHistory, 1);
    $chiliarchHistory = semantic_get_recent_as_list($chiliarchHistory, 1);
    $centurionHistory = semantic_get_recent_as_list($centurionHistory, 3);
    $decurionHistory = semantic_get_recent_as_list($decurionHistory, 5);
    $conversationHistory = semantic_get_recent_as_list($legionnaireHistory, $count);

    agent_set_context_var($port, "FreebieLastHistoryCount", semantic_count($legionnaireHistory));

    $context = new_string_builder();
    looplist($marquisHistory) {
        $rec = $$;
        if ($rec.Content == ".") {
            continue;
        };
        append_format_line($context, "{0} {1}", $rec.Content, $rec.Metadata);
    };
    looplist($chiliarchHistory) {
        $rec = $$;
        if ($rec.Content == ".") {
            continue;
        };
        append_format_line($context, "{0} {1}", $rec.Content, $rec.Metadata);
    };
    looplist($centurionHistory) {
        $rec = $$;
        if ($rec.Content == ".") {
            continue;
        };
        append_format_line($context, "{0} {1}", $rec.Content, $rec.Metadata);
    };
    looplist($decurionHistory) {
        $rec = $$;
        if ($rec.Content == ".") {
            continue;
        };
        append_format_line($context, "{0} {1}", $rec.Content, $rec.Metadata);
    };
    $contextStr = string_builder_to_string($context);

    $contextFile = combine_path($projectDirectory, "docs/context.txt");
    write_file($contextFile, $contextStr);
    agent_set_context($port, $contextStr);
};

script(save_freebie_history)params($port)
{
    $projectDirectory = agent_get_project_dir($port);
    $projectIdentity = agent_get_project_identity($port);
    $legionnaireHistory = $projectIdentity + "_legionnaire_history";

    $conversationCount = semantic_count($legionnaireHistory);
    $lastHistoryCount = agent_get_context_var($port, "FreebieLastHistoryCount");
    if (isnull($lastHistoryCount) || $lastHistoryCount == 0) {
        agent_set_context_var($port, "FreebieLastHistoryCount", $conversationCount);
        return;
    };
    $historyCount = $conversationCount - $lastHistoryCount;
    if ($historyCount > 0) {
        $histories = semantic_get_recent_as_list($legionnaireHistory, $historyCount);
        $history = new_string_builder();
        looplist($histories) {
            $rec = $$;
            append_format_line($history, "{0} {1}", $rec.Content, $rec.Metadata);
        };
        $historyFile = combine_path($projectDirectory, "docs/history.txt");
        $historyStr = string_builder_to_string($history);
        write_file($historyFile, $historyStr);
        agent_set_history($port, $historyStr);
    };
};

// Body of the task queued by the freebie_save_conversation_history notification: the
// semantic adds and the history file update, which are the slow part and need no browser
// context. The counts and the induction stay on the main thread because
// induction_freebie_info calls into the renderer.
// $jsonData is the raw notification so the task re-parses its own copy of the
// conversations. Parsed message params are JSON backed mutable state owned by the
// caller, and handing those to another thread would share them; re-parsing the string
// is cheap and gives the task a structure nobody else can see.
script(save_freebie_conversations_task)params($port, $jsonData)
{
    $notif = parse_agent_notification($jsonData);
    $data = get_message_param($notif, "data");
    $conversations = get_message_param($data, "conversations");
    $count = size($conversations);

    $projectIdentity = agent_get_project_identity($port);
    $legionnaireHistory = $projectIdentity + "_legionnaire_history";

    nativelog("[dsl] Saving {0} new conversation(s) to history (task, port:{1})", $count, $port);

    loop($count) {
        $i = $$;
        $conv = $conversations[$i];
        $user = get_message_param($conv, "user");
        $assistant = get_message_param($conv, "assistant");
        $content = format("User:\n{0}\n\nAssistant:\n{1}", $user, $assistant);
        semantic_add($legionnaireHistory, $content, to_json({source: "inject", index: $i, date: date_time_str()}));
        nativelog("[dsl] Saved conversation {0}/{1}", $i + 1, $count);
    };

    save_freebie_history($port);
};

// Handle agent notification
script(handle_agent_notification)params($jsonData)
{
    nativelog("[dsl] handle_agent_notification called with: {0}", getstringinlength($jsonData,100));

    // Parse the notification using parse_agent_notification API
    $notif = parse_agent_notification($jsonData);
    $type = get_message_param($notif, "type");

    nativelog("[dsl] Notification type: {0}", $type);

    if ($type == "hyarena_ready") {
        nativelog("[dsl] Hyarena ready notification: {0}", getstringinlength($jsonData,100));
        $port = hashtableget(@AgentPorts,"hyarena");

        $data = get_message_param($notif, "data");
        $url = get_message_param($data, "url");
        nativelog("[dsl] Hyarena initialized, url: {0}", $url);

        agent_set_max_result_size($port, 50*1024);
        agent_enable_context_injection($port, false);
        agent_set_project_dir($port, combinepath(basepath, "../AiFreebie/AiArena"));
        agent_set_project_identity($port, "hyarena");
        // Start WebSocket server on port $port for hyarena bridge communication
        ws_start_server($port);

        // Notify JS to connect multi-slot WS to port $port
        send_command_to_inject("ws_start_hyarena", to_json({port: $port}));

        nativelog("[dsl] Hyarena bridge WS server started on {0}", $port);
    }
    elif ($type == "venus_ready") {
        nativelog("[dsl] Venus ready notification: {0}", getstringinlength($jsonData,100));
        $port = hashtableget(@AgentPorts,"venus");

        $data = get_message_param($notif, "data");
        $url = get_message_param($data, "url");
        nativelog("[dsl] Venus initialized, url: {0}", $url);

        agent_set_max_result_size($port, 40*1024);
        agent_enable_context_injection($port, true);
        agent_set_project_dir($port, combinepath(basepath, "../AiFreebie/AiVenus"));
        agent_set_project_identity($port, "venus");
        // Start WebSocket server on port $port for venus bridge communication
        ws_start_server($port);

        // Notify JS to connect multi-slot WS to port $port
        send_command_to_inject("ws_start_venus", to_json({port: $port}));

        nativelog("[dsl] Venus bridge WS server started on {0}", $port);
    }
    elif ($type == "aichat_ready") {
        nativelog("[dsl] AiChat ready notification: {0}", getstringinlength($jsonData,100));
        $port = hashtableget(@AgentPorts,"aichat");

        $data = get_message_param($notif, "data");
        $url = get_message_param($data, "url");
        nativelog("[dsl] AiChat initialized, url: {0}", $url);

        agent_set_max_result_size($port, 50*1024);
        agent_enable_context_injection($port, false);
        agent_set_project_dir($port, combinepath(basepath, "../AiFreebie/AiChat"));
        agent_set_project_identity($port, "aichat");
        // Start WebSocket server on port $port for aichat bridge communication
        ws_start_server($port);

        // Notify JS to connect multi-slot WS to port $port
        send_command_to_inject("ws_start_aichat", to_json({port: $port}));

        nativelog("[dsl] AiChat bridge WS server started on {0}", $port);
    }
    elif ($type == "gemini_ready") {
        nativelog("[dsl] Gemini ready notification: {0}", getstringinlength($jsonData,100));
        $port = hashtableget(@AgentPorts,"gemini");

        $data = get_message_param($notif, "data");
        $url = get_message_param($data, "url");
        nativelog("[dsl] Gemini initialized, url: {0}", $url);

        agent_set_max_result_size($port, 40*1024);
        agent_enable_context_injection($port, false);
        agent_set_project_dir($port, combinepath(basepath, "../AiFreebie/AiGemini"));
        agent_set_project_identity($port, "gemini");
        // Start WebSocket server on port $port for gemini bridge communication
        ws_start_server($port);

        // Notify JS to connect multi-slot WS to port $port
        send_command_to_inject("ws_start_gemini", to_json({port: $port}));

        nativelog("[dsl] Gemini bridge WS server started on {0}", $port);
    }
    elif ($type == "openai_ready") {
        nativelog("[dsl] OpenAI ready notification: {0}", getstringinlength($jsonData,100));
        $port = hashtableget(@AgentPorts,"openai");

        $data = get_message_param($notif, "data");
        $url = get_message_param($data, "url");
        nativelog("[dsl] OpenAI initialized, url: {0}", $url);

        agent_set_max_result_size($port, 40*1024);
        agent_enable_context_injection($port, false);
        agent_set_project_dir($port, combinepath(basepath, "../AiFreebie/AiOpenai"));
        agent_set_project_identity($port, "openai");
        // Start WebSocket server on port $port for openai bridge communication
        ws_start_server($port);

        // Notify JS to connect multi-slot WS to port $port
        send_command_to_inject("ws_start_openai", to_json({port: $port}));

        nativelog("[dsl] OpenAI bridge WS server started on {0}", $port);
    }
    elif ($type == "imate_ready") {
        nativelog("[dsl] IMate ready notification: {0}", getstringinlength($jsonData,100));
        $port = hashtableget(@AgentPorts,"imate");

        $data = get_message_param($notif, "data");
        $url = get_message_param($data, "url");
        nativelog("[dsl] IMate initialized, url: {0}", $url);

        agent_set_max_result_size($port, 40*1024);
        agent_enable_context_injection($port, false);
        agent_set_project_dir($port, combinepath(basepath, "../AiFreebie/AiImate"));
        agent_set_project_identity($port, "imate");
        // Start WebSocket server on port $port for imate bridge communication
        ws_start_server($port);

        // Notify JS to connect multi-slot WS to port $port
        send_command_to_inject("ws_start_imate", to_json({port: $port}));

        nativelog("[dsl] IMate bridge WS server started on {0}", $port);
    }
    elif ($type == "with_ready") {
        nativelog("[dsl] With ready notification: {0}", getstringinlength($jsonData,100));
        $port = hashtableget(@AgentPorts,"with");

        $data = get_message_param($notif, "data");
        $url = get_message_param($data, "url");
        nativelog("[dsl] With initialized, url: {0}", $url);

        agent_set_max_result_size($port, 40*1024);
        agent_enable_context_injection($port, false);
        agent_set_project_dir($port, combinepath(basepath, "../AiFreebie/AiWith"));
        agent_set_project_identity($port, "with");
        // Start WebSocket server on port $port for venus bridge communication
        ws_start_server($port);

        // Notify JS to connect multi-slot WS to port $port
        send_command_to_inject("ws_start_with", to_json({port: $port}));

        nativelog("[dsl] With bridge WS server started on {0}", $port);
    }
    elif ($type == "google_ready") {
        nativelog("[dsl] Google AI ready notification: {0}", getstringinlength($jsonData,100));
        $port = hashtableget(@AgentPorts,"google");

        $data = get_message_param($notif, "data");
        $url = get_message_param($data, "url");
        nativelog("[dsl] Google AI initialized, url: {0}", $url);

        agent_set_max_result_size($port, 7*1024);
        agent_enable_context_injection($port, false);
        agent_set_project_dir($port, combinepath(basepath, "../AiFreebie/AiGoogle"));
        agent_set_project_identity($port, "google");
        // Start WebSocket server on port $port for google bridge communication
        ws_start_server($port);

        // Notify JS to connect multi-slot WS to port $port
        send_command_to_inject("ws_start_google", to_json({port: $port}));

        nativelog("[dsl] Google AI bridge WS server started on {0}", $port);
    }
    elif ($type == "freebie_context_count_down") {
        nativelog("[dsl] freebie context count down notification received");

        $data = get_message_param($notif, "data");
        $agentId = get_message_param($data, "agentId");
        $count = get_message_param($data, "count");

        $port = hashtableget(@AgentPorts, $agentId);
        $projectDirectory = agent_get_project_dir($port);
        $projectIdentity = agent_get_project_identity($port);

        nativelog("[dsl] freebie_context_count_down, agent: {0}, count: {1}", $agentId, $count);

        trigger_freebie_reflection($port);
        save_freebie_context($count, $port);

        $todoFile = combine_path($projectDirectory, "docs/todo.txt");
        $time1 = get_file_last_write_time($todoFile);
        $time2 = now();
        $seconds = get_diff_time_seconds($time1, $time2);
        if ($seconds > 1800) {
            $prompt = "可以将最新进展使用MetaDSL更新到todo.txt（页面浏览器本地，非远端工作空间）后再继续工作了";
            send_command_to_inject("send_message", to_json({text: $prompt}));
        };
    }
    elif ($type == "freebie_save_conversation_history") {
        nativelog("[dsl] freebie_save_conversation_history notification received");

        $data = get_message_param($notif, "data");
        $conversations = get_message_param($data, "conversations");
        $agentId = get_message_param($data, "agentId");
        $count = size($conversations);

        $port = hashtableget(@AgentPorts, $agentId);
        $projectDirectory = agent_get_project_dir($port);
        $projectIdentity = agent_get_project_identity($port);

        nativelog("[dsl] freebie_save_conversation_history, count: {0} agent id:{1} port:{2}", $count, $agentId, $port);

        $marquisHistory = $projectIdentity + "_marquis_history";
        $chiliarchHistory = $projectIdentity + "_chiliarch_history";
        $centurionHistory = $projectIdentity + "_centurion_history";
        $decurionHistory = $projectIdentity + "_decurion_history";
        $legionnaireHistory = $projectIdentity + "_legionnaire_history";
        $episodicMemory = $projectIdentity + "_episodic_memory";

        agent_set_todo($port, read_file(combine_path($projectDirectory, "docs/todo.txt")));
        agent_set_context($port, read_file(combine_path($projectDirectory, "docs/context.txt")));

        // The sqlite writes and the history file update are handed to a background worker.
        // They are the slow part and need no browser, while the counts and the induction
        // below must stay on the main thread because induction_freebie_info calls into the
        // renderer. Consequence: this round's counts may not yet include the conversations
        // being saved now. The batches are cumulative, so an induction they would have
        // triggered happens on a later round instead of being lost.
        call_metadsl_task(0, "save_freebie_conversations_task", $port, $jsonData);

        $legionnaireCount = semantic_count($legionnaireHistory);
        $decurionCount = semantic_count($decurionHistory);
        $centurionCount = semantic_count($centurionHistory);
        $chiliarchCount = semantic_count($chiliarchHistory);
        $marquisCount = semantic_count($marquisHistory);

        $legionnaireBatch = ($legionnaireCount - $decurionCount * 10) / 10;
        $decurionBatch = ($decurionCount - $centurionCount * 10) / 10;
        $centurionBatch = ($centurionCount - $chiliarchCount * 10) / 10;
        $chiliarchBatch = ($chiliarchCount - $marquisCount * 10) / 10;

        $legionnaires = semantic_get_recent_as_list($legionnaireHistory, $legionnaireCount - $decurionCount * 10);
        $decurions = semantic_get_recent_as_list($decurionHistory, $decurionCount - $centurionCount * 10);
        $centurions = semantic_get_recent_as_list($centurionHistory, $centurionCount - $chiliarchCount * 10);
        $chiliarchs = semantic_get_recent_as_list($chiliarchHistory, $chiliarchCount - $marquisCount * 10);

        induction_freebie_info($chiliarchBatch, $chiliarchs, "llm_pm_marquis", $port);
        induction_freebie_info($centurionBatch, $centurions, "llm_pm_chiliarch", $port);
        induction_freebie_info($decurionBatch, $decurions, "llm_pm_centurion", $port);
        induction_freebie_info($legionnaireBatch, $legionnaires, "llm_pm_decurion", $port);

        nativelog("[dsl] Saved Freebie Induction, LegionnaireBatch: {0}, DecurionBatch: {1}, CenturionBatch: {2}, ChiliarchBatch: {3}", $legionnaireBatch, $decurionBatch, $centurionBatch, $chiliarchBatch);

        // Check episodic memory count and trigger pattern recognition
        $episodicCount = semantic_count($episodicMemory);
        $lastPatternEpisodicCount = agent_get_context_var($port, "LastFreebiePatternEpisodicCount");
        if (isnull($lastPatternEpisodicCount) || $lastPatternEpisodicCount == 0) {
            $lastPatternEpisodicCount = $episodicCount;
            agent_set_context_var($port, "LastFreebiePatternEpisodicCount", $episodicCount);
        };
        if ($episodicCount - $lastPatternEpisodicCount >= 30) {
            agent_set_context_var($port, "LastFreebiePatternEpisodicCount", $episodicCount);
            nativelog("[dsl] Episodic memory count {0}, last pattern trigger at {1}, triggering pattern recognition", $episodicCount, $lastPatternEpisodicCount);
            $prompt = format("最近反思记录已超过30条，请基于反思数据总结新模式。先读 read_file(\"{0}/docs/patterns.md\") 了解已有模式，再用 semantic_get_recent(\"{1}_episodic_memory\",30) 拉最近反思，聚类归纳新增/修订模式后追加到 patterns.md（保持简洁，无空话套话）。", agent_get_project_dir($port), agent_get_project_identity($port));
            send_command_to_inject("send_message", to_json({text: $prompt}));
        };
        if (agent_is_context_injection_enabled($port) && agent_add_cur_context_rounds($port) == 0) {
            $prompt = format("【todo】:{0}\n\n【上下文信息】:{1}\n\n【最近会话】:{2}", agent_get_todo($port), agent_get_context($port), agent_get_history($port));
            send_command_to_inject("send_message", to_json({text: $prompt}));
        };
    }
    else {
        nativelog("[dsl] Unknown notification type: {0}", $type);
    };
};
