// Agent main script for the standalone AgentCore process.
//
// Lifecycle (host main thread, stateful interpreter):
//   init_global_consts -> on_init -> (on_tick | event callbacks)* -> on_finalize
// Worker threads (raw metadsl execution, call_metadsl_task) build their own
// interpreters and only see init_global_consts state (stateless design).
//
// Communication with inject.js (browser relay / direct ws, both handled by the
// same envelope machinery):
//   js -> c#: {"type":"agent_call","id":..,"func":..,"args":[..]}    dispatched to handle_agent_command etc., reply is agent_result
//   js -> c#: {"type":"agent_notify","func":..,"args":[..]}          dispatched to handle_agent_notification, no reply
//   c# -> js: async service results                                  dispatched to handle_llm_callback etc. (this script), or direct agent_callback push when the hook is missing
//   c# -> js: send_command_to_inject / send_response_to_inject       agent_command / agent_result push (targets the requesting connection)
//
// Full flow set ported from script_renderer.dsl (bundles 2a/2b/3 of the
// architecture migration): PM flows, freebie flows and all notification
// branches. P2 adaptation: js state (queue counts, llm category) is no longer
// queried via CallJavascriptFuncInRenderer (renderer api, unavailable here) —
// the js side attaches a jsState block to every notification and it is cached
// per agent in context vars.

// ----------------------------------------------------------------------------
// constants (P1b: unconditional, this IS the agent process; processtype is -1)
// ----------------------------------------------------------------------------

script(init_global_consts)
{
    setenv("PLAYWRIGHT_DRIVER_SEARCH_PATH", combinepath(basepath, "managed"));

    // Site table (P1c, data-driven): agentId -> port / project dir / identity /
    // max result size / context injection. 9540 = the relay port (agentport).
    // C-b: instances are keyed by agent id strings; ports are transport details.
    @AgentId = "webagent";

    @SiteIdentity = hashtable("webagent":"webagent","hyarena":"hyarena","venus":"venus","aichat":"aichat","gemini":"gemini","openai":"openai","imate":"imate","with":"with","google":"google");
    @SiteDir = hashtable("webagent":"", "hyarena":"../AiFreebie/AiArena", "venus":"../AiFreebie/AiVenus", "aichat":"../AiFreebie/AiChat", "gemini":"../AiFreebie/AiGemini", "openai":"../AiFreebie/AiOpenai", "imate":"../AiFreebie/AiImate", "with":"../AiFreebie/AiWith", "google":"../AiFreebie/AiGoogle");
    @SiteMaxSize = hashtable("webagent":50*1024,"hyarena":50*1024,"venus":40*1024,"aichat":50*1024,"gemini":40*1024,"openai":40*1024,"imate":40*1024,"with":40*1024,"google":7*1024);
    @SiteContextInjection = hashtable("webagent":false,"hyarena":false,"venus":true,"aichat":false,"gemini":false,"openai":false,"imate":false,"with":false,"google":false);

    if (ismac) {
        @UserName = getenv("USER");
    }
    else {
        @UserName = getenv("USERNAME");
    };
    @EnableLlmPM = (@UserName == "dreamanlan" || @UserName == "dreaman" || @UserName == "lanxiang");
    @LlmProviderId = "auto_metadsl";

    @ProjectIdentity = agent_get_project_identity(@AgentId);
    @ProjectDirectory = agent_get_project_dir(@AgentId);
    // P1a: the browser passes --projectidentity through to this process; seed
    // the agent state with it when nothing is configured yet.
    if (isnullorempty(@ProjectIdentity) && !isnullorempty(initialprojectidentity)) {
        @ProjectIdentity = initialprojectidentity;
        agent_set_project_identity(@AgentId, @ProjectIdentity);
    };
    if (!isnullorempty(@ProjectIdentity)) {
        @MarquisHistory = @ProjectIdentity + "_marquis_history";
        @ChiliarchHistory = @ProjectIdentity + "_chiliarch_history";
        @CenturionHistory = @ProjectIdentity + "_centurion_history";
        @DecurionHistory = @ProjectIdentity + "_decurion_history";
        @LegionnaireHistory = @ProjectIdentity + "_legionnaire_history";
        @EpisodicMemory = @ProjectIdentity + "_episodic_memory";
    };
};

script(on_init)
{
    nativelog("[agent] on_init, starting websocket servers (relay port {0} + site ports)", agentport);
    init_global_consts();
    $ok = ws_start_server(agentport, @AgentId);
    nativelog("[agent] ws_start_server({0}, {1}) = {2}", agentport, @AgentId, $ok);
    // Site ports retired: every page (main + single-page agents) reaches this
    // process through the relay port; per-agent resolution is the agent_bind
    // connection binding, see the raw MetaDSL path in WebSocketServer.
};

script(on_finalize)
{
    nativelog("[agent] on_finalize");
    ws_stop_server(agentport);
};

script(on_tick)
{
    // Main flow entry, driven by the host loop (about 50ms). Keep it light;
    // slow work belongs on call_metadsl_task worker threads.
};

script(on_ws_client_connected)params($port)
{
    nativelog("[agent] ws client connected on port {0}", $port);
};

script(on_ws_client_disconnected)params($port)
{
    nativelog("[agent] ws client disconnected from port {0}", $port);
};

// ----------------------------------------------------------------------------
// js state cache (P2): every notification from the js side carries a jsState
// block { operationQueueCount, sendQueueCount, receiveQueueCount, llmCategory }
// (main page: from window.AgentAPI, site pages: from window.MetaDSLBridge).
// The last value per agent is cached here; async flows (e.g. llm callbacks)
// read the cache — the flows the state used to be queried for synchronously.
// ----------------------------------------------------------------------------

script(cache_js_state)params($data, $agentId)
{
    $jsState = get_message_param($data, "jsState");
    if ($jsState == null) {
        return;
    };
    if (isnullorempty($agentId)) {
        $agentId = "main";
    };
    set_context_var("jsState_" + $agentId, to_json($jsState));
};

script(js_queue_count)params($agentId, $kind)
{
    if (isnullorempty($agentId)) {
        $agentId = "main";
    };
    $s = get_context_var("jsState_" + $agentId);
    if (isnull($s)) {
        return(0);
    };
    $v = from_json($s);
    $n = get_message_param($v, $kind);
    // str_to_int only parses strings (a numeric BoxedValue yields 0), so
    // normalize through to_string first.
    return(str_to_int(to_string($n)));
};

script(js_llm_category)params($agentId)
{
    if (isnullorempty($agentId)) {
        $agentId = "main";
    };
    $s = get_context_var("jsState_" + $agentId);
    if (isnull($s)) {
        return("");
    };
    $v = from_json($s);
    return(get_message_param($v, "llmCategory"));
};

// ----------------------------------------------------------------------------
// js command / notification dispatch (ported from script_renderer.dsl)
// ----------------------------------------------------------------------------

// Handle agent command
script(handle_agent_command)params($jsonData)
{
    nativelog("[agent] handle_agent_command: {0}", get_string_in_length($jsonData, 100));

    $cmd = parse_agent_command($jsonData);
    if ($cmd == null) {
        $errorResponse = build_agent_response(0, false, "", "Failed to parse command");
        send_response_to_inject($errorResponse);
        return("error");
    };

    $id = get_message_param($cmd, "id");
    $command = get_message_param($cmd, "command");
    $params = get_message_param($cmd, "params");

    nativelog("[agent] Command: {0}, ID: {1}", $command, $id);

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
    elif ($command == "get_initial_project_identity") {
        handle_get_initial_project_identity_command($id, $params);
    }
    elif ($command == "update_project_config") {
        handle_update_project_config_command($id, $params);
    }
    else {
        nativelog("[agent] Unknown command: {0}", $command);
        $errorResponse = build_agent_response($id, false, "", format("Unknown command: {0}", $command));
        send_response_to_inject($errorResponse);
    };

    return("ok");
};

// Handle ping command
script(handle_ping_command)params($id, $params)
{
    nativelog("[agent] Handling ping command, ID: {0}", $id);
    $response = build_agent_response($id, true, "pong", "");
    send_response_to_inject($response);
};

// Handle llm_chat command from inject.js chat panel
// params: providerId, tag, topic, text
script(handle_llm_chat_command)params($id, $params)
{
    nativelog("[agent] Handling llm_chat command, ID: {0}", $id);

    $providerId = get_message_param($params, "providerId");
    $tag = get_message_param($params, "tag");
    $topic = get_message_param($params, "topic");
    $text = get_message_param($params, "text");

    nativelog("[agent] llm_chat: provider={0} tag={1} topic={2} text_len={3}", $providerId, $tag, $topic, strlen($text));

    if (strlen($text) == 0) {
        $response = build_agent_response($id, false, "", "text is empty");
        send_response_to_inject($response);
        return("error");
    };

    $result = llm_chat_callback($providerId, $tag, $topic, $text);

    $response = build_agent_response($id, true, $result, "");
    send_response_to_inject($response);

    nativelog("[agent] llm_chat sent: {0}", $result);
};

// Handle set_agent_environment command from inject.js config panel
// params: category, group, key, value
script(handle_set_agent_environment_command)params($id, $params)
{
    nativelog("[agent] Handling set_agent_environment command, ID: {0}", $id);

    $category = get_message_param($params, "category");
    $group = get_message_param($params, "group");
    $key = get_message_param($params, "key");
    $value = get_message_param($params, "value");

    nativelog("[agent] set_agent_environment: category={0} group={1} key={2}", $category, $group, $key);

    $result = set_agent_environment($category, $group, $key, $value);

    $response = build_agent_response($id, $result, "ok", "");
    send_response_to_inject($response);

    nativelog("[agent] set_agent_environment result: {0}", $result);
};

// Handle update_agent_configs command - notified when agent environments are fully loaded
script(handle_update_agent_configs_command)params($id, $params)
{
    nativelog("[agent] Handling update_agent_configs command, ID: {0}", $id);
    $response = build_agent_response($id, true, "ok", "");
    send_response_to_inject($response);

    // LLM providers configured here; apiKey uses %var% placeholders expanded via agent environment
    llm_set_provider("ollama", "ollama", "http://localhost:11434", "", "qwen3.8:27b");

    $pmModel = "hy3";
    if (ismac) {
        $pmModel = "deepseek-v4-flash";
    };
    llm_set_provider("auto_metadsl", "auto_metadsl", "https://knot.woa.com/apigw/api/v1/agents/agui/%agent_id%", "%person_token%", $pmModel);

    // Search services configured here; apiKey uses %var% placeholders
    // brave_set_api_key("%brave_api_key%");
    searxng_set_engines("bing,yahoo,360search,baidu,sogou,quark");
    searxng_set_url("https://www.gamexyz.net:8090");
};

// P1a: the renderer host's initialprojectidentity global does not exist in the
// agent process; the initial identity is stored here (context var) when the js
// project panel sends update_project_config, and this getter returns it.
script(handle_get_initial_project_identity_command)params($id, $params)
{
    nativelog("[agent] Handling get_initial_project_identity command, ID: {0}", $id);

    $projectIdentity = get_context_var("InitialProjectIdentity");
    if (isnull($projectIdentity)) {
        $projectIdentity = "";
    };

    nativelog("[agent] get_initial_project_identity: identity={0}", $projectIdentity);

    $result = to_json({projectIdentity: $projectIdentity});
    $response = build_agent_response($id, true, $result, "");
    send_response_to_inject($response);
};

// P1a: update the agent-side project state from the js project panel.
// params: projectDir, projectIdentity
script(handle_update_project_config_command)params($id, $params)
{
    nativelog("[agent] Handling update_project_config command, ID: {0}", $id);

    $projectDir = get_message_param($params, "projectDir");
    $projectIdentity = get_message_param($params, "projectIdentity");

    nativelog("[agent] update_project_config: dir={0} identity={1}", $projectDir, $projectIdentity);

    if (strlen($projectDir) == 0 || strlen($projectIdentity) == 0) {
        $response = build_agent_response($id, false, "", "projectDir and projectIdentity are required");
        send_response_to_inject($response);
        return("error");
    };

    agent_set_project_dir(@AgentId, $projectDir);
    agent_set_project_identity(@AgentId, $projectIdentity);
    set_context_var("InitialProjectIdentity", $projectIdentity);

    // Refresh the dsl globals (semantic history collection names etc.).
    init_global_consts();

    // Re-init semantic history collections for the new project.
    semantic_init(@MarquisHistory);
    semantic_init(@ChiliarchHistory);
    semantic_init(@CenturionHistory);
    semantic_init(@DecurionHistory);
    semantic_init(@LegionnaireHistory);
    semantic_init(@EpisodicMemory);

    $response = build_agent_response($id, true, "ok", "");
    send_response_to_inject($response);

    nativelog("[agent] update_project_config done: dir={0} identity={1}", $projectDir, $projectIdentity);
};

// ----------------------------------------------------------------------------
// Async service callback forwarders
// ----------------------------------------------------------------------------

// Handle LLM callback (async llm_chat_callback results) — full PM version
// ported from script_renderer.dsl. $providerId: provider id, $tag: session
// tag, $topic: topic, $reply: full reply text.
// The context is the connection that initiated the llm chat (main page relay),
// so send_command_to_inject pushes land on that page.
script(handle_llm_callback)params($providerId, $tag, $topic, $reply)
{
    // P2: queue counts come from the js state cache (the js side attaches
    // jsState to every notification; values may lag by one notification).
    $operationQueueCount = js_queue_count("", "operationQueueCount");
    $sendQueueCount = js_queue_count("", "sendQueueCount");
    $receiveQueueCount = js_queue_count("", "receiveQueueCount");
    $activeWorkers = agent_get_active_workers(@AgentId);
    nativelog("[agent] llm_callback: provider={0} tag={1} topic={2} reply_len={3} operation={4} send={5} receive={6} active_workers={7}", $providerId, $tag, $topic, strlen($reply), $operationQueueCount, $sendQueueCount, $receiveQueueCount, $activeWorkers);

    if ($tag == "llm_pm_align") {
        $planFile = combine_path(@ProjectDirectory, "docs/plan.txt");
        write_file($planFile, $reply);
        agent_set_plan(@AgentId, $reply);
        llm_clear_history(@LlmProviderId, $tag);
    }
    elif ($tag == "reflection") {
        if (strlen($reply) > 1500) {
            llm_chat_callback(@LlmProviderId, "reflection", "reflection", "超长了，请控制到300字左右");
            nativelog("[agent] Episodic memory too long: {0}", get_string_in_length($reply, 500));
        }
        else {
            semantic_add(@EpisodicMemory, $reply, to_json({source: "reflection", date: date_time_str(), type: "episodic"}));
            llm_clear_history(@LlmProviderId, "reflection");
            nativelog("[agent] Episodic memory saved: {0}", get_string_in_length($reply, 500));
        };
    }
    elif ($tag == "llm_pm_marquis") {
        semantic_add(@MarquisHistory, $reply, to_json({source: "inject", date: date_time_str()}));
        llm_clear_history(@LlmProviderId, "llm_pm_marquis");
    }
    elif ($tag == "llm_pm_chiliarch") {
        semantic_add(@ChiliarchHistory, $reply, to_json({source: "inject", date: date_time_str()}));
        llm_clear_history(@LlmProviderId, "llm_pm_chiliarch");
    }
    elif ($tag == "llm_pm_centurion") {
        semantic_add(@CenturionHistory, $reply, to_json({source: "inject", date: date_time_str()}));
        llm_clear_history(@LlmProviderId, "llm_pm_centurion");
    }
    elif ($tag == "llm_pm_decurion") {
        semantic_add(@DecurionHistory, $reply, to_json({source: "inject", date: date_time_str()}));
        llm_clear_history(@LlmProviderId, "llm_pm_decurion");
    }
    elif ($tag == "llm_pm_decision") {
        // Forward PM's fixed decision text as user reply to LLM.
        $tags = extract_tags($reply, "action", 1);
        if (count($tags) > 0) {
            $actions = $tags[0];
            if (count($actions) > 0) {
                $action = $actions[0];
                $autoPlanStr = $actions[1];
                $lockAgentStr = $actions[2];
                $autoPlan = $autoPlanStr=="true" || $autoPlanStr=="True";
                $lockAgent = $lockAgentStr=="true" || $lockAgentStr=="True";
                if ($action=="plan"){
                    trigger_plan($autoPlan, $lockAgent);
                }
                elif ($action=="plan_if"){
                    if ($autoPlan) {
                        trigger_plan($autoPlan, $lockAgent);
                    }
                    else {
                        nativelog("[agent] plan_if: auto_plan={0} lock_agent={1}", $autoPlan, $lockAgent);
                    };
                }
                elif ($action=="reply_if"){
                    $info = $actions[3];
                    if ($autoPlan) {
                        send_command_to_inject("send_message", to_json({text: $info}));
                    }
                    else {
                        nativelog("[agent] reply_if: auto_plan={0} lock_agent={1} info={2}", $autoPlan, $lockAgent, $info);
                    };
                };
            };
        }
        else{
            $workers = agent_get_active_workers(@AgentId);
            if (string_contains($reply, "没有待执行代码了") && $workers > 0) {
                // Remain silent
                nativelog("[agent] skip reply: info={0} workers={1}", $reply, $workers);
            }
            else {
                send_command_to_inject("send_message", to_json({text: $reply}));
            };
        };
        llm_clear_history(@LlmProviderId, "llm_pm_decision");
    }
    else {
        $workers = agent_get_active_workers(@AgentId);
        if ($operationQueueCount > 0 || $workers > 0 || $sendQueueCount > 0 || $receiveQueueCount > 0) {
            $reply = format("{0}\n\n**还有{1}个代码在排队执行，{2}个请求在排队发送，{3}个结果在排队接收，当前有{4}个代码正在执行中**", $reply, $operationQueueCount, $sendQueueCount, $receiveQueueCount, $workers);
        };
        if ($operationQueueCount > 0) {
            $reply = format("{0}\n**不要再发新代码，回复继续即可**", $reply);
        };
        send_command_to_inject("send_message", to_json({text: $reply}));
    };
};

// $serverId: MCP server id, $callbackTag: caller tag, $resultText: tool result
script(handle_mcp_callback)params($serverId, $callbackTag, $resultText)
{
    nativelog("[agent] mcp_callback: server={0} tag={1} result_len={2}", $serverId, $callbackTag, strlen($resultText));
    $text = format("[MCP Tool Result] server={0} tag={1}\n{2}", $serverId, $callbackTag, $resultText);
    send_command_to_inject("send_message", to_json({text: $text}));
};

script(handle_skill_callback)params($cmdStr, $argStr, $workDir, $resultText)
{
    nativelog("[agent] skill_callback: cmd={0} arg={1} work_dir={2} result={3}", $cmdStr, $argStr, $workDir, get_string_in_length($resultText, 100));
    $text = format("[skill Result] cmd={0} arg={1} work_dir={2}\n{3}", $cmdStr, $argStr, $workDir, $resultText);
    send_command_to_inject("send_message", to_json({text: $text}));
};

script(handle_command_callback)params($cmdStr, $argStr, $workDir, $resultText)
{
    nativelog("[agent] command_callback: cmd={0} arg={1} work_dir={2} result={3}", $cmdStr, $argStr, $workDir, get_string_in_length($resultText, 100));
    $text = format("[command Result] cmd={0} arg={1} work_dir={2}\n{3}", $cmdStr, $argStr, $workDir, $resultText);
    send_command_to_inject("send_message", to_json({text: $text}));
};

script(handle_http_auth_callback)params($url, $tag, $response)
{
    nativelog("[agent] http_auth_callback: url={0} tag={1} response={2}", $url, $tag, get_string_in_length($response, 100));
    $text = format("[http Auth Result] url={0} tag={1}\n{2}", $url, $tag, $response);
    send_command_to_inject("send_message", to_json({text: $text}));
};

// ----------------------------------------------------------------------------
// PM flows (bundle 2a, ported from script_renderer.dsl)
// ----------------------------------------------------------------------------

script(update_system_prompt)params($pageType,$isFirst)
{
    // Read context file
    $foundationPromptFile = combine_path(basepath, "docs/foundation_prompt.txt");
    $toplevelRulesFile = combine_path(basepath, "docs/rules_toplevel.txt");
    $emphasizeFile = combine_path(basepath, "docs/emphasize.txt");
    $soulFile = combine_path(@ProjectDirectory, "docs/soul.md");
    $projectPromptFile = combine_path(@ProjectDirectory, "docs/project_prompt.txt");
    $backlogFile = combine_path(@ProjectDirectory, "docs/backlog.txt");
    $planFile = combine_path(@ProjectDirectory, "docs/plan.txt");
    $contextFile = combine_path(@ProjectDirectory, "docs/context.txt");
    $historyFile = combine_path(@ProjectDirectory, "docs/history.txt");

    $foundationPrompt = read_file($foundationPromptFile);
    $toplevelRules = read_file($toplevelRulesFile);
    $emphasize = read_file($emphasizeFile);
    $soul = read_file($soulFile);
    $projectPrompt = read_file($projectPromptFile);
    $backlog = read_file($backlogFile);
    $plan = read_file($planFile);
    $context = read_file($contextFile);
    $history = read_file($historyFile);

    nativelog("[agent] prompt lengths: foundation={0} rules={1} emphasize={2} soul={3} project={4} backlog={5} plan={6} context={7} history={8}", strlen($foundationPrompt), strlen($toplevelRules), strlen($emphasize), strlen($soul), strlen($projectPrompt), strlen($backlog), strlen($plan), strlen($context), strlen($history));

    // P2: llm category from the js state cache instead of CallJavascriptFuncInRenderer.
    $llmCategory = js_llm_category("");
    //now we use dynamic system prompts
    if ($pageType == "local-agent") {
        $prompt = $emphasize + "\n\n" + $soul + "\n\n" + $projectPrompt + "\n\n" + $plan + "\n\n" + $context;
        if ($isFirst) {
            $prompt = $prompt + "\n\n" + $history;
        };
        if ($llmCategory == "ollama" || $llmCategory == "local_openai") {
            $prompt = $foundationPrompt + "\n\n" + $toplevelRules + "\n\n" + $prompt;
        };
    } else {
        $prompt = $foundationPrompt + "\n\n" + $toplevelRules + "\n\n" + $emphasize + "\n\n" + $soul + "\n\n" + $projectPrompt + "\n\n" + $plan + "\n\n" + $context + "\n\n" + $history;
    };
    agent_set_foundation_prompt(@AgentId, $foundationPrompt);
    agent_set_project_prompt(@AgentId, $projectPrompt);

    if ($isFirst) {
        agent_set_emphasize(@AgentId, $emphasize);
        agent_set_soul(@AgentId, $soul);
        agent_set_backlog(@AgentId, $backlog);
        agent_set_plan(@AgentId, $plan);
        agent_set_context(@AgentId, $context);
        agent_set_history(@AgentId, $history);
    };

    send_command_to_inject("update_system_prompt", to_json({prompt: $prompt}));

    if (@EnableLlmPM) {
        $note = "你作为PM，要特别注意，一切以对话事实信息为准，不要猜测，缺少信息的保持现状不修改";
        $llm_sys_prompt = format("{0}\n\n{1}", $note, $projectPrompt);
        llm_set_system_prompt(@LlmProviderId, "llm_pm_decision", "");
        llm_set_system_prompt(@LlmProviderId, "llm_pm_align", $llm_sys_prompt);
    };

    if ($isFirst) {
        $time1 = get_file_last_write_time($soulFile);
        $time2 = now();
        $days = get_diff_time_days($time1, $time2);
        if ($days > 7) {
            $reply = format("{0}\n\n[Reminder: Check if soul.md needs updating. Before refining, use read_file(\"{1}/docs/patterns.md\") and semantic_get_recent(\"{2}_episodic_memory\",30) to form the abstraction chain (episodic->pattern->metacognition). Keep soul.md updates under 500 chars, no empty slogans.]", $plan, agent_get_project_dir(@AgentId), agent_get_project_identity(@AgentId));
        }
        else {
            $reply = $plan;
        };
        send_command_to_inject("send_message", to_json({text: $reply}));
    };
};

script(induction_info)params($batch, $infos, $session)
{
    nativelog("[agent] induction_info, batch: {0}, infos: {1}, session: {2}", $batch, count($infos), $session);

    if (@EnableLlmPM) {
        loop($batch) {
            $i = $$;
            $induction = new_string_builder();
            loop(10){
                $j = $i*10 + $$;
                append_line($induction, $infos[$j]);
            };

            llm_chat_callback(@LlmProviderId, $session, "induction", format("{0}\n\n以上是最近10次工作信息，请按以下规则归纳成一段话（一次回复输出完成，200字左右，不超过300字）：产出以关键词/名词短语流为主，可适当润色方便理解；只反映上述信息中已有的事实，不凭空生造未涉及的内容；能用已有关键词准确概括时优先复用，不能准确概括时允许提炼意义上的新词。\n\n至关重要：切勿遗漏变量名、路径或公式中的任何下划线（_）。请务必严格保持所有 snake_case 格式。", get_string_in_length(to_pretty_string(string_builder_to_string($induction)), 100 * 1024, 1)));
        };
    }
    else {
        // P2: queue counts from the js state cache.
        $operationQueueCount = js_queue_count("", "operationQueueCount");
        $receiveQueueCount = js_queue_count("", "receiveQueueCount");
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
                $id = semantic_add(agent_get_project_identity(@AgentId) + $histId, '.', to_json({source: 'inject', date: date_time_str()}));

                nativelog("[agent] induction_info, order: {0}, skip: {1}, history id: {2}", $i, $id, $histId);
            };
        };

        if ($batch > 0) {
            if ($ct > 0) {
                $id = semantic_add(agent_get_project_identity(@AgentId) + $histId, '.', to_json({source: 'inject', date: date_time_str()}));

                nativelog("[agent] induction_info, order: {0}, skip: {1}, history id: {2}", $batch - 1, $id, $histId);
            }
            else {
                $prompt = format("{0}\n\n以上是最近若干次工作信息，请按以下规则归纳成一段话（一次回复输出完成，200字左右，不超过300字）：产出以关键词/名词短语流为主，可适当润色方便理解；只反映上述信息中已有的事实，不凭空生造未涉及的内容；能用已有关键词准确概括时优先复用，不能准确概括时允许提炼意义上的新词。然后使用`{1}`写到库里。\n\n至关重要：切勿遗漏变量名、路径或公式中的任何下划线（_）。请务必严格保持所有 snake_case 格式。", get_string_in_length(to_pretty_string(string_builder_to_string($induction)), 75 * 1024, 1),
                    format("semantic_add(agent_get_project_identity(\"{0}\")+'{1}', [[归纳内容]], to_json({{source: 'inject', date: date_time_str()}}));", @AgentId, $histId)
                    );
                // Fallback when PM is disabled: just send "continue" to keep LLM moving.
                send_command_to_inject("send_message", to_json({text: $prompt}));

                nativelog("[agent] induction_info, prompt: {0}, history id: {1}", get_string_in_length($prompt, 100), $histId);
            };
        };
    };
};

script(induction_decision)params($lastMsg,$autoPlan,$lockAgent)
{
    $promptTpl = read_file(combine_path(basepath, "docs/reply_prompt.txt"));
    $prompt = format($promptTpl, $autoPlan, $lockAgent, $lastMsg);

    if (@EnableLlmPM) {
        llm_chat_callback(@LlmProviderId, "llm_pm_decision", "reply_decision", $prompt);
    }
    elif ($autoPlan == true || $autoPlan == "True" || $autoPlan == "true") {
        // Fallback when PM is disabled: just send "continue" to keep LLM moving.
        send_command_to_inject("send_message", to_json({text: "请继续（目前没有待执行代码了）"}));
    };
};

script(induction_plan)params($count,$pageType)
{
    $planFile = combine_path(@ProjectDirectory, "docs/plan.txt");
    // Load recent conversation history from semantic index
    $conversationHistory = to_pretty_string(semantic_get_recent(@LegionnaireHistory, $count));

    $planHistory = read_file(combine_path(@ProjectDirectory, "docs/plan.txt"));
    $contextHistory = read_file(combine_path(@ProjectDirectory, "docs/context.txt"));

    $prompt = format("【以下是当前计划】：\n{0}\n" +
        "【以下是最近上下文信息】：\n{1}\n" +
        "【以下是最近对话历史】：\n{2}", $planHistory, $contextHistory, $conversationHistory);
    $prompt = format("{0}\n\n根据以上信息，复述当前计划工作，只以事实为准更新完成状态，缺少相关信息默认未完成，" +
        "已完成内容使用简要描述条目并标记完成状态，当前工作保留详细信息（工作介绍与进展细节），未完成工作保留条目信息" +
        "（一次回复输出完成,字数控制到300~500字左右）。\n\n至关重要：切勿遗漏变量名、路径或公式中的任何下划线（_）。请务必严格保持所有 snake_case 格式。", $prompt);

    if (@EnableLlmPM) {
        llm_chat_callback(@LlmProviderId, "llm_pm_align", "align_target", $prompt);
    }
    else {
        $prompt = format("{0}\n\n并使用metadsl代码写入{1}，\n" +
            "记得metadsl代码里不能有markdown代码块标记，所以文档内容格式要简洁", $prompt, $planFile);
        send_command_to_inject("send_message", to_json({text: $prompt}));
    };
};

script(trigger_plan)params($autoPlan,$lockAgent)
{
    $backlogPath = combine_path(@ProjectDirectory, "docs/backlog.txt");
    $soulPath = combine_path(@ProjectDirectory, "docs/soul.md");

    agent_set_soul(@AgentId, read_file($soulPath));

    nativelog("[agent] trigger_plan: auto_plan:{0} lock_agent:{1}", $autoPlan, $lockAgent);

    if (file_exists($backlogPath) && $autoPlan) {
        nativelog("[agent] plan triggered");

        if ($lockAgent) {
            $prompt = "没有识别到代码。长时间开发模式下不要等用户确认（用户不在线），请更新需求库backlog.txt与计划plan.txt状态（清理backlog条目，更新plan条目状态），然后选取新工作更新plan.txt后继续";
        }
        else {
            $prompt = "没有识别到代码。请更新需求库backlog.txt与计划plan.txt状态（清理backlog条目，更新plan条目状态）。如果计划工作尚未完成，请继续发MetaDSL代码执行；如果工作已完成，请停止自动计划以避免重复提醒";
        };
        send_command_to_inject("send_message", to_json({text: $prompt}));
    }
    else {
        nativelog("[agent] trigger_plan: plan_file not found or auto_plan is false, skip plan");
    };
};

script(trigger_reflection)params()
{
    nativelog("[agent] trigger_reflection called");

    // Collect recent conversation history
    $legionnaireHistory = get_string_in_length(to_pretty_string(semantic_get_recent(@LegionnaireHistory, 20)), 75 * 1024, 1);
    $planHistory = read_file(combine_path(@ProjectDirectory, "docs/plan.txt"));
    $contextHistory = read_file(combine_path(@ProjectDirectory, "docs/context.txt"));

    $prompt = format("【最近对话历史】：\n{0}\n\n【当前待办】：\n{1}\n\n【当前上下文】：\n{2}", $legionnaireHistory, $planHistory, $contextHistory);

    // Set reflection system prompt
    $sysPrompt = read_file(combine_path(basepath, "docs/reflection_prompt.txt"));
    llm_set_system_prompt(@LlmProviderId, "reflection", $sysPrompt);

    // Send reflection request
    if (@EnableLlmPM) {
        $prompt = format("{0}\n\n请根据以上最近的工作对话，提取结构化的经验记录（300字以内）。\n\n至关重要：切勿遗漏变量名、路径或公式中的任何下划线（_）。请务必严格保持所有 snake_case 格式。", $prompt);
        llm_chat_callback(@LlmProviderId, "reflection", "reflection", $prompt);
    }
    else {
        $prompt = format("{0}\n\n请根据以上最近的工作对话，提取结构化的经验记录（300字以内），然后使用`{1}`写到库里。\n\n至关重要：切勿遗漏变量名、路径或公式中的任何下划线（_）。请务必严格保持所有 snake_case 格式。", $prompt,
            format("semantic_add(agent_get_project_identity(\"{0}\")+'_episodic_memory', [[经验记录]], to_json({{source: 'reflection', date: date_time_str(), type: 'episodic'}}));", @AgentId)
            );
        send_command_to_inject("send_message", to_json({text: $prompt}));
    };

    nativelog("[agent] trigger_reflection: reflection request sent");
};

script(save_context)params($count,$pageType)
{
    $contextFile = combine_path(@ProjectDirectory, "docs/context.txt");
    // Load recent conversation history from semantic index
    $marquisHistory = semantic_get_recent_as_list(@MarquisHistory, 1);
    $chiliarchHistory = semantic_get_recent_as_list(@ChiliarchHistory, 1);
    $centurionHistory = semantic_get_recent_as_list(@CenturionHistory, 3);
    $decurionHistory = semantic_get_recent_as_list(@DecurionHistory, 5);
    $conversationHistory = semantic_get_recent_as_list(@LegionnaireHistory, $count);

    agent_set_context_var(@AgentId, "LastHistoryCount", semantic_count(@LegionnaireHistory));

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

    $contextFile = combine_path(@ProjectDirectory, "docs/context.txt");
    write_file($contextFile, $contextStr);
    agent_set_context(@AgentId, $contextStr);
};

script(save_history)params()
{
    $conversationCount = semantic_count(@LegionnaireHistory);
    $lastHistoryCount = agent_get_context_var(@AgentId, "LastHistoryCount");
    if (isnull($lastHistoryCount) || $lastHistoryCount == 0) {
        agent_set_context_var(@AgentId, "LastHistoryCount", $conversationCount);
        return;
    };
    $historyCount = $conversationCount - $lastHistoryCount;
    if ($historyCount > 0) {
        $histories = semantic_get_recent_as_list(@LegionnaireHistory, $historyCount);
        $history = new_string_builder();
        looplist($histories) {
            $rec = $$;
            append_format_line($history, "{0} {1}", $rec.Content, $rec.Metadata);
        };
        $historyFile = combine_path(@ProjectDirectory, "docs/history.txt");
        $historyStr = string_builder_to_string($history);
        write_file($historyFile, $historyStr);
        agent_set_history(@AgentId, $historyStr);
    };
};

// Body of the task queued by the save_conversation_history notification (the
// state_machine path). Runs on a call_metadsl_task worker thread: the sqlite
// writes and the history file update are the slow part and need no browser.
script(save_conversations_task)params($jsonData)
{
    $notif = parse_agent_notification($jsonData);
    $data = get_message_param($notif, "data");
    $conversations = get_message_param($data, "conversations");
    $count = size($conversations);

    nativelog("[agent] Saving {0} new conversation(s) to history (task)", $count);

    loop($count) {
        $i = $$;
        $conv = $conversations[$i];
        $user = get_message_param($conv, "user");
        $assistant = get_message_param($conv, "assistant");
        $content = format("User:\n{0}\n\nAssistant:\n{1}", $user, $assistant);
        semantic_add(@LegionnaireHistory, $content, to_json({source: "inject", index: $i, date: date_time_str()}));
        nativelog("[agent] Saved conversation {0}/{1}", $i + 1, $count);
    };

    save_history();
};

// ----------------------------------------------------------------------------
// Freebie flows (bundle 2b, single-page agents; $agentId keys the site table,
// $agentId is its agent id — both worlds share the same per-agent state here)
// ----------------------------------------------------------------------------

// P2: the enableFreebieButton page callback becomes a targeted command push
// (the context is the site page connection that sent the notification).
script(enable_freebie_button)params($agentId, $name)
{
    send_command_to_inject("enable_freebie_button", to_json({name: $name, agentId: $agentId}));
};

script(induction_freebie_info)params($batch, $infos, $session, $agentId)
{
    nativelog("[agent] induction_freebie_info, batch: {0}, infos: {1}, session: {2}, agent: {3}", $batch, count($infos), $session, $agentId);

    // P2: queue counts from the js state cache for this agent.
    $operationQueueCount = js_queue_count($agentId, "operationQueueCount");
    $receiveQueueCount = js_queue_count($agentId, "receiveQueueCount");
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
            $id = semantic_add(agent_get_project_identity($agentId) + $histId, '.', to_json({source: 'inject', date: date_time_str()}));

            nativelog("[agent] induction_freebie_info, order: {0}, skip: {1}, history id: {2}", $i, $id, $histId);
        };
    };

    if ($batch > 0) {
        if ($ct > 0) {
            $id = semantic_add(agent_get_project_identity($agentId) + $histId, '.', to_json({source: 'inject', date: date_time_str()}));

            nativelog("[agent] induction_freebie_info, order: {0}, skip: {1}, history id: {2}", $batch - 1, $id, $histId);
        }
        else {
            $maxLen = 30 * 1024;
            if ($agentId == "google") {
                $maxLen = 4 * 1024;
            };
            $prompt = format("{0}\n\n以上是最近工作信息，请按以下规则归纳成一段话（一次回复输出完成，200字左右，不超过300字）：产出以关键词/名词短语流为主，可适当润色方便理解；只反映上述信息中已有的事实，不凭空生造未涉及的内容；能用已有关键词准确概括时优先复用，不能准确概括时允许提炼意义上的新词。然后使用`{1}`写到库里。\n\n至关重要：切勿遗漏变量名、路径或公式中的任何下划线（_）。请务必严格保持所有 snake_case 格式。", get_string_in_length(to_pretty_string(string_builder_to_string($induction)), $maxLen, 1),
                format("semantic_add(agent_get_project_identity(\"{0}\")+'{1}', [[归纳内容]], to_json({{source: 'inject', date: date_time_str()}}));", $agentId, $histId)
                );
            // Fallback when PM is disabled: just send "continue" to keep LLM moving.
            // Notify page to enable the induce button instead of auto sending
            enable_freebie_button($agentId, "induce");

            nativelog("[agent] induction_freebie_info, prompt: {0}, history id: {1}", get_string_in_length($prompt, 100), $histId);
        };
    };
};

script(trigger_freebie_reflection)params($agentId)
{
    nativelog("[agent] trigger_freebie_reflection called");

    $projectDirectory = agent_get_project_dir($agentId);
    $projectIdentity = agent_get_project_identity($agentId);
    $legionnaireHistory = $projectIdentity + "_legionnaire_history";

    // Collect recent conversation history
    $contextHistory = read_file(combine_path($projectDirectory, "docs/context.txt"));

    $prompt = format("{0}\n\n请结合你的上下文记忆，提取结构化的经验记录（300字以内），然后使用`{1}`写到库里。\n\n至关重要：切勿遗漏变量名、路径或公式中的任何下划线（_）。请务必严格保持所有 snake_case 格式。", $contextHistory,
        format("semantic_add(agent_get_project_identity(\"{0}\")+'_episodic_memory', [[经验记录]], to_json({{source: 'reflection', date: date_time_str(), type: 'episodic'}}));", $agentId)
        );
    // Notify page to enable the reflect button instead of auto sending
    enable_freebie_button($agentId, "reflect");

    nativelog("[agent] trigger_freebie_reflection: reflection request sent");
};

script(save_freebie_context)params($count,$agentId)
{
    $projectDirectory = agent_get_project_dir($agentId);
    $projectIdentity = agent_get_project_identity($agentId);
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

    agent_set_context_var($agentId, "FreebieLastHistoryCount", semantic_count($legionnaireHistory));

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
    agent_set_context($agentId, $contextStr);
};

script(save_freebie_history)params($agentId)
{
    $projectDirectory = agent_get_project_dir($agentId);
    $projectIdentity = agent_get_project_identity($agentId);
    $legionnaireHistory = $projectIdentity + "_legionnaire_history";

    $conversationCount = semantic_count($legionnaireHistory);
    $lastHistoryCount = agent_get_context_var($agentId, "FreebieLastHistoryCount");
    if (isnull($lastHistoryCount) || $lastHistoryCount == 0) {
        agent_set_context_var($agentId, "FreebieLastHistoryCount", $conversationCount);
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
        agent_set_history($agentId, $historyStr);
    };
};

// Prompt getters for the single-page agents, called via agent_call from the
// js side (relay_transport.callAgent). $agentId identifies the site.
script(freebie_get_induction_prompt)params($agentId)
{
    if (isnullorempty($agentId)) {
        $agentId = "webagent";
    };
    nativelog("[agent] freebie_get_induction_prompt, agent: {0}", $agentId);
    $legionnaireHistory = agent_get_project_identity($agentId) + "_legionnaire_history";
    $infos = semantic_get_recent_as_list($legionnaireHistory, 10);
    $induction = new_string_builder();
    looplist($infos) {
        $rec = $$;
        if ($rec.Content == ".") {
            continue;
        };
        append_format_line($induction, "{0} {1}", $rec.Content, $rec.Metadata);
    };
    $histId = "_decurion_history";
    $maxLen = 30 * 1024;
    if ($agentId == "google") {
        $maxLen = 4 * 1024;
    };
    $prompt = format("{0}\n\n以上是最近工作信息，请按以下规则归纳成一段话（一次回复输出完成，200字左右，不超过300字）：产出以关键词/名词短语流为主，可适当润色方便理解；只反映上述信息中已有的事实，不凭空生造未涉及的内容；能用已有关键词准确概括时优先复用，不能准确概括时允许提炼意义上的新词。然后使用`{1}`写到库里。\n\n至关重要：切勿遗漏变量名、路径或公式中的任何下划线（_）。请务必严格保持所有 snake_case 格式。", get_string_in_length(to_pretty_string(string_builder_to_string($induction)), $maxLen, 1),
        format("semantic_add(agent_get_project_identity(\"{0}\")+'{1}', [[归纳内容]], to_json({{source: 'inject', date: date_time_str()}}));", $agentId, $histId)
        );
    return($prompt);
};

script(freebie_get_reflection_prompt)params($agentId)
{
    if (isnullorempty($agentId)) {
        $agentId = "webagent";
    };
    nativelog("[agent] freebie_get_reflection_prompt, agent: {0}", $agentId);
    $projectDirectory = agent_get_project_dir($agentId);
    $contextHistory = read_file(combine_path($projectDirectory, "docs/context.txt"));
    $prompt = format("{0}\n\n请结合你的上下文记忆，提取结构化的经验记录（300字以内），然后使用`{1}`写到库里。\n\n至关重要：切勿遗漏变量名、路径或公式中的任何下划线（_）。请务必严格保持所有 snake_case 格式。", $contextHistory,
        format("semantic_add(agent_get_project_identity(\"{0}\")+'_episodic_memory', [[经验记录]], to_json({{source: 'reflection', date: date_time_str(), type: 'episodic'}}));", $agentId)
        );
    return($prompt);
};

script(freebie_get_pattern_prompt)params($agentId)
{
    if (isnullorempty($agentId)) {
        $agentId = "webagent";
    };
    nativelog("[agent] freebie_get_pattern_prompt, agent: {0}", $agentId);
    $prompt = format("最近反思记录已超过30条，请基于反思数据总结新模式。先读 read_file(\"{0}/docs/patterns.md\") 了解已有模式，再用 semantic_get_recent(\"{1}_episodic_memory\",30) 拉最近反思，聚类归纳新增/修订模式后追加到 patterns.md（保持简洁，无空话套话）。", agent_get_project_dir($agentId), agent_get_project_identity($agentId));
    return($prompt);
};

// Body of the task queued by the freebie_save_conversation_history
// notification: the semantic adds and the history file update, which are the
// slow part and need no browser context.
script(save_freebie_conversations_task)params($agentId, $jsonData)
{
    $notif = parse_agent_notification($jsonData);
    $data = get_message_param($notif, "data");
    $conversations = get_message_param($data, "conversations");
    $count = size($conversations);

    $projectIdentity = agent_get_project_identity($agentId);
    $legionnaireHistory = $projectIdentity + "_legionnaire_history";

    nativelog("[agent] Saving {0} new conversation(s) to history (task, agent:{1})", $count, $agentId);

    loop($count) {
        $i = $$;
        $conv = $conversations[$i];
        $user = get_message_param($conv, "user");
        $assistant = get_message_param($conv, "assistant");
        $content = format("User:\n{0}\n\nAssistant:\n{1}", $user, $assistant);
        semantic_add($legionnaireHistory, $content, to_json({source: "inject", index: $i, date: date_time_str()}));
        nativelog("[agent] Saved conversation {0}/{1}", $i + 1, $count);
    };

    save_freebie_history($agentId);
};

// get_plan for the single-page agents (called via agent_call, replaces the
// old synchronous callMetaDSL('get_plan', AGENT_ID) in the adapters).
script(get_plan)params($agentId)
{
    if (isnullorempty($agentId)) {
        $agentId = @AgentId;
    };
    $dir = agent_get_project_dir($agentId);
    $soulPrompt = read_file(combine_path($dir, "docs/soul.md"));
    $planPrompt = read_file(combine_path($dir, "docs/plan.txt"));
    return(format("{0}\n\n{1}", $soulPrompt, $planPrompt));
};

// ----------------------------------------------------------------------------
// Site prompt getters (called via agent_call / relayTransport.callAgent).
// Moved from script_renderer.dsl: the sandboxed renderer cannot read files,
// all file reads live in this process now.
// ----------------------------------------------------------------------------

script(get_arena_system_prompt)params()
{
    return(read_file(combine_path(basepath, "docs/arena_prompt.txt")));
};

script(get_venus_system_prompt)params()
{
    return(read_file(combine_path(basepath, "docs/venus_prompt.txt")));
};

script(get_google_prompt_1)params()
{
    return(read_file(combine_path(basepath, "docs/google_prompt_1.txt")));
};

script(get_google_prompt_2)params()
{
    return(read_file(combine_path(basepath, "docs/google_prompt_2.txt")));
};

script(get_google_prompt_3)params()
{
    return(read_file(combine_path(basepath, "docs/google_prompt_3.txt")));
};

script(get_google_prompt)params()
{
    return(read_file(combine_path(basepath, "docs/google_prompt.txt")));
};

script(get_openai_prompt)params()
{
    return(read_file(combine_path(basepath, "docs/openai_prompt.txt")));
};

script(get_imate_prompt)params()
{
    return(read_file(combine_path(basepath, "docs/imate_prompt.txt")));
};

script(get_with_prompt)params()
{
    return(read_file(combine_path(basepath, "docs/with_prompt.txt")));
};

// ----------------------------------------------------------------------------
// Notification dispatch (bundle 3): agent_ready (main page relay), <site>_ready
// for every single-page agent (data driven via the site table), and the
// PM/freebie notification branches. The context of every notify is the
// connection that sent it, so command pushes land on the right page.
// ----------------------------------------------------------------------------

script(handle_agent_notification)params($jsonData)
{
    nativelog("[agent] handle_agent_notification: {0}", get_string_in_length($jsonData, 100));

    $notif = parse_agent_notification($jsonData);
    $type = get_message_param($notif, "type");
    $data = get_message_param($notif, "data");

    nativelog("[agent] Notification type: {0}", $type);

    // P2: cache the js state block carried by every notification.
    $agentIdForKey = get_message_param($data, "agentId");
    cache_js_state($data, $agentIdForKey);

    if ($type == "agent_ready") {
        $pageType = get_message_param($data, "pageType");
        $url = get_message_param($data, "url");

        nativelog("[agent] Agent initialized on page type: {0}, url: {1}", $pageType, $url);

        agent_set_max_result_size(@AgentId, 50*1024);
        agent_enable_context_injection(@AgentId, false);

    }
    elif (string_ends_with($type, "_ready")) {
        // Single-page agent ready (hyarena_ready, venus_ready, ...): data
        // driven via the site table.
        $agentId = string_substring($type, 0, strlen($type) - strlen("_ready"));
        if (isnull(hashtableget(@SiteIdentity, $agentId))) {
            nativelog("[agent] Unknown site ready notification: {0}", $type);
            return;
        };

        $url = get_message_param($data, "url");
        nativelog("[agent] {0} initialized, url: {1}", $agentId, $url);

        agent_set_max_result_size($agentId, hashtableget(@SiteMaxSize, $agentId));
        agent_enable_context_injection($agentId, hashtableget(@SiteContextInjection, $agentId));
        $siteDir = hashtableget(@SiteDir, $agentId);
        if ($siteDir != "") {
            agent_set_project_dir($agentId, combinepath(basepath, $siteDir));
        };
        agent_set_project_identity($agentId, hashtableget(@SiteIdentity, $agentId));

        // Tell the page to (re)create its execution slot: the single-slot
        // adapters still key their slot creation off this push. The port
        // value is legacy-only — wsCreate builds a RelaySlot (relay, agent_bind)
        // and ignores it.
        send_command_to_inject("ws_start_" + $agentId, to_json({port: agentport}));

        nativelog("[agent] {0} ready", $agentId);
    }
    elif ($type == "freebie_context_count_down") {
        nativelog("[agent] freebie context count down notification received");

        $agentId = get_message_param($data, "agentId");
        $count = get_message_param($data, "count");

            $projectDirectory = agent_get_project_dir($agentId);
        $projectIdentity = agent_get_project_identity($agentId);

        nativelog("[agent] freebie_context_count_down, agent: {0}, count: {1}", $agentId, $count);

        trigger_freebie_reflection($agentId);
        save_freebie_context($count, $agentId);

        $planFile = combine_path($projectDirectory, "docs/plan.txt");
        $time1 = get_file_last_write_time($planFile);
        $time2 = now();
        $seconds = get_diff_time_seconds($time1, $time2);
        if ($seconds > 1800) {
            $prompt = format("可以将最新进展使用MetaDSL更新到{0}（页面浏览器本地，非远端工作空间）后再继续工作，同时清理已完成plan条目", $planFile);
            send_command_to_inject("send_message", to_json({text: $prompt}));
        };
    }
    elif ($type == "freebie_save_conversation_history") {
        nativelog("[agent] freebie_save_conversation_history notification received");

        $conversations = get_message_param($data, "conversations");
        $agentId = get_message_param($data, "agentId");
        $count = size($conversations);

            $projectDirectory = agent_get_project_dir($agentId);
        $projectIdentity = agent_get_project_identity($agentId);

        nativelog("[agent] freebie_save_conversation_history, count: {0} agent id:{1}", $count, $agentId);

        $marquisHistory = $projectIdentity + "_marquis_history";
        $chiliarchHistory = $projectIdentity + "_chiliarch_history";
        $centurionHistory = $projectIdentity + "_centurion_history";
        $decurionHistory = $projectIdentity + "_decurion_history";
        $legionnaireHistory = $projectIdentity + "_legionnaire_history";
        $episodicMemory = $projectIdentity + "_episodic_memory";

        agent_set_plan($agentId, read_file(combine_path($projectDirectory, "docs/plan.txt")));
        agent_set_context($agentId, read_file(combine_path($projectDirectory, "docs/context.txt")));

        // The sqlite writes and the history file update are handed to a
        // background worker (call_metadsl_task); the counts and the induction
        // stay on the main thread because they push to the renderer.
        call_metadsl_task(0, "save_freebie_conversations_task", $agentId, $jsonData);

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

        induction_freebie_info($chiliarchBatch, $chiliarchs, "llm_pm_marquis", $agentId);
        induction_freebie_info($centurionBatch, $centurions, "llm_pm_chiliarch", $agentId);
        induction_freebie_info($decurionBatch, $decurions, "llm_pm_centurion", $agentId);
        induction_freebie_info($legionnaireBatch, $legionnaires, "llm_pm_decurion", $agentId);

        nativelog("[agent] Saved Freebie Induction, LegionnaireBatch: {0}, DecurionBatch: {1}, CenturionBatch: {2}, ChiliarchBatch: {3}", $legionnaireBatch, $decurionBatch, $centurionBatch, $chiliarchBatch);

        // Check episodic memory count and trigger pattern recognition
        $episodicCount = semantic_count($episodicMemory);
        $lastPatternEpisodicCount = agent_get_context_var($agentId, "LastFreebiePatternEpisodicCount");
        if (isnull($lastPatternEpisodicCount) || $lastPatternEpisodicCount == 0) {
            $lastPatternEpisodicCount = $episodicCount;
            agent_set_context_var($agentId, "LastFreebiePatternEpisodicCount", $episodicCount);
        };
        if ($episodicCount - $lastPatternEpisodicCount >= 30) {
            agent_set_context_var($agentId, "LastFreebiePatternEpisodicCount", $episodicCount);
            nativelog("[agent] Episodic memory count {0}, last pattern trigger at {1}, triggering pattern recognition", $episodicCount, $lastPatternEpisodicCount);
            // Notify page to enable the pattern button instead of auto sending
            enable_freebie_button($agentId, "pattern");
        };
        if (agent_is_context_injection_enabled($agentId) && agent_add_cur_context_rounds($agentId) == 0) {
            $prompt = format("【计划】:{0}\n\n【上下文信息】:{1}\n\n【最近会话】:{2}", agent_get_plan($agentId), agent_get_context($agentId), agent_get_history($agentId));
            send_command_to_inject("send_message", to_json({text: $prompt}));
        };
    }
    elif ($type == "llm_update_system_prompt") {
        nativelog("[agent] LLM update system prompt notification received");

        $pageType = get_message_param($data, "pageType");

        nativelog("[agent] LLM update system prompt pageType: {0}", $pageType);

        update_system_prompt($pageType, true);
    }
    elif ($type == "llm_context_count_down") {
        nativelog("[agent] LLM context count down notification received");

        $pageType = get_message_param($data, "pageType");
        $count = get_message_param($data, "count");

        nativelog("[agent] llm_context_count_down pageType: {0}, count: {1}", $pageType, $count);

        trigger_reflection();
        save_context($count, $pageType);

        $planFile = combine_path(@ProjectDirectory, "docs/plan.txt");
        $time1 = get_file_last_write_time($planFile);
        $time2 = now();
        $seconds = get_diff_time_seconds($time1, $time2);
        if ($seconds > 1800) {
            $prompt = "可以将最新进展更新到plan.txt后再继续工作，同时清理已完成plan条目（不要停自动计划!）";
            send_command_to_inject("send_message", to_json({text: $prompt}));
        };
    }
    elif ($type == "llm_align_target") {
        nativelog("[agent] LLM align target notification received");

        $pageType = get_message_param($data, "pageType");
        $count = get_message_param($data, "count");

        nativelog("[agent] llm_align_target pageType: {0}, count: {1}", $pageType, $count);

        induction_plan($count, $pageType);
    }
    elif ($type == "episodic_reflection") {
        nativelog("[agent] episodic_reflection notification received");
        trigger_reflection();
    }
    elif ($type == "save_conversation_history") {
        nativelog("[agent] save_conversation_history notification received");

        $conversations = get_message_param($data, "conversations");
        $pageType = get_message_param($data, "pageType");
        $count = size($conversations);

        agent_set_backlog(@AgentId, read_file(combine_path(@ProjectDirectory, "docs/backlog.txt")));
        agent_set_plan(@AgentId, read_file(combine_path(@ProjectDirectory, "docs/plan.txt")));
        agent_set_context(@AgentId, read_file(combine_path(@ProjectDirectory, "docs/context.txt")));

        // Same split as the freebie branch: the sqlite writes and the history
        // file update go to a background worker, while the induction and the
        // system prompt update stay on the main thread.
        call_metadsl_task(0, "save_conversations_task", $jsonData);

        $legionnaireCount = semantic_count(@LegionnaireHistory);
        $decurionCount = semantic_count(@DecurionHistory);
        $centurionCount = semantic_count(@CenturionHistory);
        $chiliarchCount = semantic_count(@ChiliarchHistory);
        $marquisCount = semantic_count(@MarquisHistory);

        $legionnaireBatch = ($legionnaireCount - $decurionCount * 10) / 10;
        $decurionBatch = ($decurionCount - $centurionCount * 10) / 10;
        $centurionBatch = ($centurionCount - $chiliarchCount * 10) / 10;
        $chiliarchBatch = ($chiliarchCount - $marquisCount * 10) / 10;

        $legionnaires = semantic_get_recent_as_list(@LegionnaireHistory, $legionnaireCount - $decurionCount * 10);
        $decurions = semantic_get_recent_as_list(@DecurionHistory, $decurionCount - $centurionCount * 10);
        $centurions = semantic_get_recent_as_list(@CenturionHistory, $centurionCount - $chiliarchCount * 10);
        $chiliarchs = semantic_get_recent_as_list(@ChiliarchHistory, $chiliarchCount - $marquisCount * 10);

        induction_info($chiliarchBatch, $chiliarchs, "llm_pm_marquis");
        induction_info($centurionBatch, $centurions, "llm_pm_chiliarch");
        induction_info($decurionBatch, $decurions, "llm_pm_centurion");
        induction_info($legionnaireBatch, $legionnaires, "llm_pm_decurion");

        nativelog("[agent] Saved Induction, LegionnaireBatch: {0}, DecurionBatch: {1}, CenturionBatch: {2}, ChiliarchBatch: {3}", $legionnaireBatch, $decurionBatch, $centurionBatch, $chiliarchBatch);

        // Check episodic memory count and trigger pattern recognition
        $episodicCount = semantic_count(@EpisodicMemory);
        $lastPatternEpisodicCount = agent_get_context_var(@AgentId, "LastPatternEpisodicCount");
        if (isnull($lastPatternEpisodicCount) || $lastPatternEpisodicCount == 0) {
            $lastPatternEpisodicCount = $episodicCount;
            agent_set_context_var(@AgentId, "LastPatternEpisodicCount", $episodicCount);
        };
        if ($episodicCount - $lastPatternEpisodicCount >= 30) {
            agent_set_context_var(@AgentId, "LastPatternEpisodicCount", $episodicCount);
            nativelog("[agent] Episodic memory count {0}, last pattern trigger at {1}, triggering pattern recognition", $episodicCount, $lastPatternEpisodicCount);
            $prompt = format("最近反思记录已超过30条，请基于反思数据总结新模式。先读 read_file(\"{0}/docs/patterns.md\") 了解已有模式，再用 semantic_get_recent(\"{1}_episodic_memory\",30) 拉最近反思，聚类归纳新增/修订模式后追加到 patterns.md（保持简洁，无空话套话）。", agent_get_project_dir(@AgentId), agent_get_project_identity(@AgentId));
            send_command_to_inject("send_message", to_json({text: $prompt}));
        };
        if (agent_is_context_injection_enabled(@AgentId) && agent_add_cur_context_rounds(@AgentId) == 0) {
            $prompt = format("【plan】:{0}\n\n【上下文信息】:{1}\n\n【最近会话】:{2}", agent_get_plan(@AgentId), agent_get_context(@AgentId), agent_get_history(@AgentId));
            send_command_to_inject("send_message", to_json({text: $prompt}));
        };

        update_system_prompt($pageType, false);
    }
    elif ($type == "agent_need_to_decide") {
        // Lightweight PM reply channel for C-class semantic keywords
        nativelog("[agent] agent_need_to_decide notification received (trigger_decision)");

        $lastScannedMessage = get_message_param($data, "lastScannedMessage");
        $pageType = get_message_param($data, "pageType");
        $count = get_message_param($data, "count");
        $autoPlan = get_message_param($data, "autoPlan");
        $lockAgent = get_message_param($data, "lockAgent");
        $soulPath = combine_path(@ProjectDirectory, "docs/soul.md");

        agent_set_soul(@AgentId, read_file($soulPath));

        nativelog("[agent] agent_need_to_decide: page:{0} lastMsg:{1}", $pageType, $lastScannedMessage);
        induction_decision($lastScannedMessage, $autoPlan, $lockAgent);
    }
    else {
        nativelog("[agent] Unknown notification type: {0}", $type);
    };
};
