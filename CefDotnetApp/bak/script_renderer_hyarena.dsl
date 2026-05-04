// Note:The design philosophy behind these DSL scripts is to be stateless;
// all state resides within C# or JS. The DSL's global variables are utilized
// for configuring constants, and each hot-reload operation executes independently.
script(init_global_consts)
{
	@EnableLlmPM = true;
	//@LlmProviderId = "ollama_local";
	@LlmProviderId = "auto_metadsl";

	if (processtype == 1) {
		@ProjectIdentity = get_project_identity();
		@ProjectDirectory = get_project_dir();
		if (!isnullorempty(@ProjectIdentity)) {
			@MarquisHistory = @ProjectIdentity + "_marquis_history";
			@ChiliarchHistory = @ProjectIdentity + "_chiliarch_history";
			@CenturionHistory = @ProjectIdentity + "_centurion_history";
			@DecurionHistory = @ProjectIdentity + "_decurion_history";
			@LegionnaireHistory = @ProjectIdentity + "_legionnaire_history";
			@EpisodicMemory = @ProjectIdentity + "_episodic_memory";
			@PatternMemory = @ProjectIdentity + "_pattern_memory";
			@MetaCognitionMemory = @ProjectIdentity + "_meta_cognition_memory";
		};
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
		$targetBrowserId = get_context_var("TargetBrowserId", "session");
		if (isnull($targetBrowserId) || $targetBrowserId <= 0) {
			$targetBrowserId = find_browser_id_by_url_key("hyarena.woa.com/chat");
			if ($targetBrowserId <= 0) {
				$targetBrowserId = find_browser_id_by_url_key("localhost:8080");
			};
			if ($targetBrowserId > 0) {
				set_context_var("TargetBrowserId", $targetBrowserId, "session");
			};
		};
		if ($targetBrowserId > 0) {
			if (!set_context_by_id($targetBrowserId)) {
				// Browser no longer valid, reset cache to re-search next heartbeat
				set_context_var("TargetBrowserId", 0, "session");
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

	if (stringcontainsany($url, "file:///", "http://localhost") && stringcontainsany($url, "AgentCore/hotreload_test.html", "http://localhost:8080/agent.html", "http://localhost:8081")) {
		$cmdLine.AppendSwitch("disable-web-security");
		$cmdLine.AppendSwitch("allow-file-access-from-files");
	}
	elif (stringcontainsany($url, "https://evaluation.woa.com/chat")) {
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
	$cmdLine.AppendSwitchWithValue("user-agent-product", "Chrome/144.0.7559.172");

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
	if (string_contains_any($url, "https://hyarena.woa.com/chat", "http://localhost:8080/agent.html") && ($isMainFrame == "True" || $isMainFrame == true)) {
		$base = combine_path(basepath, "managed/inject_modules/");
		$sb = new_string_builder();
		append_line($sb, "(function() {");
		append_line($sb, "  'use strict';");
		append_line($sb, "  let bridge = null;");
		append_line($sb, "  let pageAdapter = null;");
		append_line($sb, "  let metadslMonitor = null;");
		append_line($sb, "  let panel = null;");
		append_line($sb, "  let projectWindow = null;");
		append_line($sb, "  let chatRoomWindow = null;");
		append_line($sb, read_file(combine_path($base, "config.js")));
		append_line($sb, read_file(combine_path($base, "logger.js")));
		append_line($sb, read_file(combine_path($base, "secret_store.js")));
		append_line($sb, read_file(combine_path($base, "ws_worker.js")));
		append_line($sb, read_file(combine_path($base, "bridge.js")));
		append_line($sb, read_file(combine_path($base, "plan_decider.js")));
		append_line($sb, read_file(combine_path($base, "input_monitor.js")));
		append_line($sb, read_file(combine_path($base, "state_machine.js")));
		append_line($sb, read_file(combine_path($base, "page_adapter.js")));
		append_line($sb, read_file(combine_path($base, "metadsl_monitor.js")));
		append_line($sb, read_file(combine_path($base, "panel.js")));
		append_line($sb, read_file(combine_path($base, "chat_input.js")));
		append_line($sb, read_file(combine_path($base, "relay_ws.js")));
		append_line($sb, read_file(combine_path($base, "relay_panel.js")));
		append_line($sb, read_file(combine_path($base, "project_panel.js")));
		append_line($sb, read_file(combine_path($base, "spellcheck.js")));
		append_line($sb, read_file(combine_path($base, "ws_manager.js")));
		append_line($sb, read_file(combine_path($base, "main.js")));
		append_line($sb, "})();");
		$code = string_builder_to_string($sb);
		nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
		return((true, $code));
	}
	elif (string_contains_any($url, "https://hyarena.woa.com/chat") && ($isMainFrame == "True" || $isMainFrame == true)) {
		$base = combine_path(basepath, "managed/inject_modules/");
		$sb = new_string_builder();
		append_line($sb, read_file(combine_path($base, "hyarena_opus.js")));
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

script(on_receive_cef_message)params($msg,$args,$srcProcId)
{
	nativelog("[dsl] on_receive_cef_message:{0} argnum:{1} from:{2} processtype:{3}",$msg,listsize($args),$srcProcId,processtype);
	if (processtype == 1) {
		//Renderer
		if (funcexists($msg)) {
			redirectcall($msg, $args);
		}
		else {
			redirectcall("handle_" + $msg, $args);
			//nativeapi.SendJavascriptCallForDSL("alert", [$msg]);
		};
	};
};

script(on_call_metadsl)params($func,$args)
{
	nativelog("[dsl] on_call_metadsl: func={0}, args={1}", $func, to_json($args));
};

// Return system prompt text loaded from docs/system_prompt.txt (zero-arg, sync)
script(get_system_prompt)params()
{
	return(read_file(combine_path(basepath, "docs/system_prompt.txt")));
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
	$queuedCount = str_to_int(nativeapi.CallJavascriptFuncInRendererForDSL("window.AgentAPI.getQueuedCount",[]));

	if ($tag == "llm_pm_project") {
		$planFile = combine_path(@ProjectDirectory, "docs/plan.txt");
		write_file($planFile, $reply);
		set_plan($reply);
		llm_clear_history(@LlmProviderId, $tag);

		$replyWithReminder = format("{0}\n\n[Reminder: Check if soul.md needs updating. Keep under 500 chars, no empty slogans.]", $reply);
		send_command_to_inject("send_message", to_json({text: $replyWithReminder}));
	}
	elif ($tag == "llm_pm_context") {
		$contextFile = combine_path(@ProjectDirectory, "docs/context.txt");
		write_file($contextFile, $reply);
		set_context($reply);
		llm_clear_history(@LlmProviderId, $tag);
	}
	elif ($tag == "llm_pm_align") {
		$todoFile = combine_path(@ProjectDirectory, "docs/todo.txt");
		write_file($todoFile, $reply);
		set_todo($reply);
		llm_clear_history(@LlmProviderId, $tag);
	}
	elif ($tag == "reflection") {
		semantic_add(@EpisodicMemory, $reply, to_json({source: "reflection", date: date_time_str(), type: "episodic"}));
		llm_clear_history(@LlmProviderId, "reflection");
		nativelog("[dsl] Episodic memory saved: {0}", getstringinlength($reply, 500, 0));
	}
	elif ($tag == "pattern_recognition") {
		parse_and_save_pattern_md($reply);
		$summary = getstringinlength($reply, 500, 0);
		semantic_add(@PatternMemory, $summary, to_json({source: "pattern_recognition", date: date_time_str(), type: "pattern", storage: "md_files"}));
		llm_clear_history(@LlmProviderId, "pattern_recognition");
		nativelog("[dsl] Pattern memory saved to MD files, summary: {0}", $summary);
	}
	elif ($tag == "meta_cognition") {
		$commonDir = combine_path(basepath, "docs/memory");
		if(!dir_exists($commonDir)){
			create_dir($commonDir);
		};
		$metaPath = combine_path($commonDir, "meta_cognition.md");
		write_file($metaPath, $reply);
		nativelog("[dsl] Meta-cognition MD saved: {0} chars", stringlength($reply));
		$summary = getstringinlength($reply, 500, 0);
		semantic_add(@MetaCognitionMemory, $summary, to_json({source: "meta_cognition", date: date_time_str(), type: "meta_cognition", storage: "md_file"}));
		llm_clear_history(@LlmProviderId, "meta_cognition");
		nativelog("[dsl] Meta-cognition summary saved: {0}", $summary);
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
	else {
		if ($queuedCount > 0) {
			$reply = format("{0}\n**还有{1}个代码要执行，不要再发新代码，回复继续即可**", $reply, $queuedCount);
		};
		send_command_to_inject("send_message", to_json({text: $reply}));
	};
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
	elif ($command == "llm_set_provider") {
		handle_llm_set_provider_command($id, $params);
	}
	elif ($command == "brave_set_api_key") {
		handle_brave_set_api_key_command($id, $params);
	}
	elif ($command == "searxng_set_url") {
		handle_searxng_set_url_command($id, $params);
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
	llm_set_provider("ollama", "ollama", "http://localhost:11434", "", "gemma3:4b");
	llm_set_provider("auto_metadsl", "auto_metadsl", "https://knot.woa.com/apigw/api/v1/agents/agui/114631ca85184f639f69572bbcfcbe7a", "%person_token%", "glm-5");

	// Search services configured here; apiKey uses %var% placeholders
	brave_set_api_key("%brave_api_key%");
	searxng_set_url("https://www.gamexyz.net:8090");
};

// Handle get_initial_project_identity command from inject.js project panel
// Returns initial projectIdentity from C#
script(handle_get_initial_project_identity_command)params($id, $params)
{
	nativelog("[dsl] Handling get_initial_project_identity command, ID: {0}", $id);

	$projectIdentity = initialprojectidentity;

	nativelog("[dsl] get_initial_project_identity: identity={0}", $projectIdentity);

	$result = to_json({projectIdentity: $projectIdentity});
	$response = build_agent_response($id, true, $result, "");
	send_response_to_inject($response);
};

// Handle update_project_config command from inject.js project panel
// params: projectDir, projectIdentity
script(handle_update_project_config_command)params($id, $params)
{
	nativelog("[dsl] Handling update_project_config command, ID: {0}", $id);

	$projectDir = get_message_param($params, "projectDir");
	$projectIdentity = get_message_param($params, "projectIdentity");

	nativelog("[dsl] update_project_config: dir={0} identity={1}", $projectDir, $projectIdentity);

	if (strlen($projectDir) == 0 || strlen($projectIdentity) == 0) {
		$response = build_agent_response($id, false, "", "projectDir and projectIdentity are required");
		send_response_to_inject($response);
		return("error");
	};

	// Update C# agent state
	set_project_dir($projectDir);
	set_project_identity($projectIdentity);

	// Update DSL global variables
	init_global_consts();

	// Re-init semantic history collections for new project
	semantic_init(@MarquisHistory);
	semantic_init(@ChiliarchHistory);
	semantic_init(@CenturionHistory);
	semantic_init(@DecurionHistory);
	semantic_init(@LegionnaireHistory);
	semantic_init(@EpisodicMemory);
	semantic_init(@PatternMemory);
	semantic_init(@MetaCognitionMemory);

	$response = build_agent_response($id, true, "ok", "");
	send_response_to_inject($response);

	nativelog("[dsl] update_project_config done: path={0} identity={1}", @ProjectDirectory, @ProjectIdentity);
};

script(induction_info)params($batch, $infos, $session)
{
	nativelog("[dsl] induction_info, batch: {0}, infos: {1}, session: {2}", $batch, count($infos), $session);

	loop($batch) {
		$i = $$;
		$induction = new_string_builder();
		loop(10){
			$j = $i*10 + $$;
			append_line($induction, $infos[$j]);
		};

		llm_chat_callback(@LlmProviderId, $session, "induction", format("{0}\n\n以上是最近10次工作信息，在不丢失关键信息的前提下，总结成一段话（一次回复输出完成）", string_builder_to_string($induction)));
	};
};

script(induction_plan)params()
{
	$planFile = combine_path(@ProjectDirectory, "docs/plan.txt");

	$planHistory = read_file(combine_path(@ProjectDirectory, "docs/plan.txt"));
	$todoHistory = read_file(combine_path(@ProjectDirectory, "docs/todo.txt"));
	$contextHistory = read_file(combine_path(@ProjectDirectory, "docs/context.txt"));
	$conversationHistory = read_file(combine_path(@ProjectDirectory, "docs/history.txt"));

	$prompt = format("【以下是最近的开发计划】：\n{0}\n" +
		"【以下是最近的待办事项】：\n{1}\n" +
		"【以下是最近上下文信息】：\n{2}\n" +
		"【以下是最近对话历史】：\n{3}", $planHistory, $todoHistory, $contextHistory, $conversationHistory);

	$prompt = format("{0}\n\n以PM身份重述一下项目概况、开发计划与待办事项" +
		"，基于事实，重点突出，不要猜测臆想（分章节分阶段分步骤，一次回复输出完成，控制在1000字以内）", $prompt);

	if (@EnableLlmPM) {
		$prompt = getstringinlength($prompt, 100 * 1024, 1);
		llm_chat_callback(@LlmProviderId, "llm_pm_project", "project_history", $prompt);
	}
	else {
		$prompt = format("{0}\n\n并使用metadsl代码写入{1}，\n" +
			"记得metadsl代码里不能有markdown代码块标记，所以文档内容格式要简洁", $prompt, $planFile);
		$prompt = getstringinlength($prompt, 100 * 1024, 1);
		send_command_to_inject("send_message", to_json({text: $prompt}));
	};
};

script(induction_context)params($count,$queuedCount,$pageType)
{
	$contextFile = combine_path(@ProjectDirectory, "docs/context.txt");
	// Load recent conversation history from semantic index
	$marquisHistory = semantic_get_recent(@MarquisHistory, 1);
	$chiliarchHistory = semantic_get_recent(@ChiliarchHistory, 1);
	$centurionHistory = semantic_get_recent(@CenturionHistory, 1);
	$decurionHistory = semantic_get_recent(@DecurionHistory, 5);
	$conversationHistory = semantic_get_recent(@LegionnaireHistory, $count);

	set_context_var("LastHistoryCount", semantic_count(@LegionnaireHistory), "session");

	$planHistory = read_file(combine_path(@ProjectDirectory, "docs/plan.txt"));
	$todoHistory = read_file(combine_path(@ProjectDirectory, "docs/todo.txt"));
	$contextHistory = read_file(combine_path(@ProjectDirectory, "docs/context.txt"));

	$prompt = format("【以下是最近的开发计划】：\n{0}\n" +
		"【以下是最近的待办事项】：\n{1}\n" +
		"【以下是最近项目状态信息】：\n{2}\n" +
		"【以下是最近对话历史】：\n{3}{4}{5}{6}{7}", $planHistory, $todoHistory, $contextHistory, $marquisHistory, $chiliarchHistory, $centurionHistory, $decurionHistory, $conversationHistory);
	$prompt = format("{0}\n\n根据以上内容，以PM身份总结更新项目状态，" +
		"，基于事实，重点突出，不要猜测臆想（分章节分阶段分步骤，一次回复输出完成，控制在1500字以内）", $prompt);

	if (@EnableLlmPM) {
		$prompt = getstringinlength($prompt, 100 * 1024, 1);
		llm_chat_callback(@LlmProviderId, "llm_pm_context", "context_summary", $prompt);
	}
	else {
		$prompt = format("{0}\n\n并使用metadsl代码写入{1}，\n" +
			"记得metadsl代码里不能有markdown代码块标记，所以文档内容格式要简洁", $prompt, $contextFile);
		$prompt = getstringinlength($prompt, 100 * 1024, 1);
		send_command_to_inject("send_message", to_json({text: $prompt}));
	};
};

script(induction_todo)params($count,$queuedCount,$pageType)
{
	$todoFile = combine_path(@ProjectDirectory, "docs/todo.txt");
	// Load recent conversation history from semantic index
	$conversationHistory = semantic_get_recent(@LegionnaireHistory, $count);

	$todoHistory = read_file(combine_path(@ProjectDirectory, "docs/todo.txt"));
	$contextHistory = read_file(combine_path(@ProjectDirectory, "docs/context.txt"));

	$prompt = format("【以下是最近的待办事项】：\n{0}\n" +
		"【以下是最近上下文信息】：\n{1}\n" +
		"【以下是最近对话历史】：\n{2}", $todoHistory, $contextHistory, $conversationHistory);
	$prompt = format("{0}\n\n根据以上信息，复述当前todo工作，只以事实为准更新完成状态，缺少相关信息默认未完成，" +
		"已完成内容使用简要描述条目并标记完成状态，当前工作保留详细信息（工作介绍与进展细节），未完成工作保留条目信息" +
		"（一次回复输出完成,字数控制到300~500字左右）", $prompt);

	if (@EnableLlmPM) {
		$prompt = getstringinlength($prompt, 100 * 1024, 1);
		llm_chat_callback(@LlmProviderId, "llm_pm_align", "align_target", $prompt);
	}
	else {
		$prompt = format("{0}\n\n并使用metadsl代码写入{1}，\n" +
			"记得metadsl代码里不能有markdown代码块标记，所以文档内容格式要简洁", $prompt, $todoFile);
		$prompt = getstringinlength($prompt, 100 * 1024, 1);
		send_command_to_inject("send_message", to_json({text: $prompt}));
	};
};

script(trigger_reflection)params()
{
	nativelog("[dsl] trigger_reflection called");

	// Collect recent conversation history
	$legionnaireHistory = semantic_get_recent(@LegionnaireHistory, 20);
	$todoHistory = read_file(combine_path(@ProjectDirectory, "docs/todo.txt"));
	$contextHistory = read_file(combine_path(@ProjectDirectory, "docs/context.txt"));

	$prompt = format("【最近对话历史】：\n{0}\n\n【当前待办】：\n{1}\n\n【当前上下文】：\n{2}", $legionnaireHistory, $todoHistory, $contextHistory);

	// Set reflection system prompt
	$sysPrompt = "根据对话历史提取结构化经验记录。" +
		"严格要求：只记录具体事实和操作，禁止抽象总结、口号式描述。" +
		"每条必须包含具体文件名、函数名、命令或操作步骤。\n" +
		"输出格式：\n" +
		"【任务】：具体做了什么（含文件/函数）\n" +
		"【方法】：用了什么工具和具体命令\n" +
		"【结果】：成功/失败及具体表现\n" +
		"【教训】：踩过的坑（附具体场景）\n\n" +
		"控制在300字以内，一次回复输出完成（不要使用记忆tool，结果数据持续入库，不要使用编号）";
	llm_set_system_prompt(@LlmProviderId, "reflection", "reflection", $sysPrompt);

	// Send reflection request
	$prompt = format("{0}\n\n请根据以上最近的工作对话，提取结构化的经验记录。", $prompt);
	$prompt = getstringinlength($prompt, 100 * 1024, 1);
	llm_chat_callback(@LlmProviderId, "reflection", "reflection", $prompt);

	nativelog("[dsl] trigger_reflection: reflection request sent");
};

script(read_all_pattern_md)params()
{
	$commonDir = combine_path(basepath, "docs/memory");
	$projectDir = combine_path(get_project_dir(), "docs/memory");
	$categories = "tool_usage,code_refactor,architecture,project_mgmt,debugging,general";
	$catList = string_split($categories, ",");
	$result = new_string_builder();
	looplist($catList){
		$cat = $$;
		$filePath = combine_path($commonDir, format("{0}.md", $cat));
		if(file_exists($filePath)){
			$content = read_file($filePath);
			if(!isnullorempty($content)){
				append_format_line($result, "[CATEGORY:{0}]", $cat);
				append_line($result, $content);
				append_format_line($result, "[/CATEGORY]");
			};
		};
	};
	// Read project-specific memory
	$projFile = combine_path($projectDir, "project_specific.md");
	if(file_exists($projFile)){
		$projContent = read_file($projFile);
		if(!isnullorempty($projContent)){
			append_format_line($result, "[CATEGORY:project_specific]");
			append_line($result, $projContent);
			append_format_line($result, "[/CATEGORY]");
		};
	};
	return(string_builder_to_string($result));
};

script(parse_and_save_pattern_md)params($reply)
{
	$commonDir = combine_path(basepath, "docs/memory");
	$projectDir = combine_path(get_project_dir(), "docs/memory");
	if(!dir_exists($commonDir)){
		create_dir($commonDir);
	};
	if(!dir_exists($projectDir)){
		create_dir($projectDir);
	};
	$categories = "tool_usage,code_refactor,architecture,project_mgmt,debugging,general";
	$catList = string_split($categories, ",");
	$savedCount = 0;
	looplist($catList){
		$cat = $$;
		$beginTag = format("[CATEGORY:{0}]", $cat);
		$endTag = "[/CATEGORY]";
		$startIdx = string_index_of($reply, $beginTag);
		if($startIdx >= 0){
			$contentStart = $startIdx + stringlength($beginTag);
			$endIdx = string_index_of($reply, $endTag, $contentStart);
			if($endIdx > $contentStart){
				$content = string_trim(string_substring($reply, $contentStart, $endIdx - $contentStart));
				if(!isnullorempty($content)){
					$filePath = combine_path($commonDir, format("{0}.md", $cat));
					write_file($filePath, $content);
					$savedCount = $savedCount + 1;
					nativelog("[dsl] Pattern MD saved: {0} ({1} chars)", $cat, stringlength($content));
				};
			};
		};
	};
	// Handle project_specific category separately (save to project dir)
	$psBeginTag = "[CATEGORY:project_specific]";
	$psEndTag = "[/CATEGORY]";
	$psStartIdx = string_index_of($reply, $psBeginTag);
	if($psStartIdx >= 0){
		$psContentStart = $psStartIdx + stringlength($psBeginTag);
		$psEndIdx = string_index_of($reply, $psEndTag, $psContentStart);
		if($psEndIdx > $psContentStart){
			$psContent = string_trim(string_substring($reply, $psContentStart, $psEndIdx - $psContentStart));
			if(!isnullorempty($psContent)){
				$psFilePath = combine_path($projectDir, "project_specific.md");
				write_file($psFilePath, $psContent);
				$savedCount = $savedCount + 1;
				nativelog("[dsl] Pattern MD saved: project_specific ({0} chars)", stringlength($psContent));
			};
		};
	};
	// If no category tags found, save entire reply to general.md
	if($savedCount == 0 && !isnullorempty($reply)){
		$filePath = combine_path($commonDir, "general.md");
		write_file($filePath, $reply);
		nativelog("[dsl] Pattern MD saved to general.md (no tags found)");
	};
	return($savedCount);
};

script(trigger_pattern_recognition)params($recentCount)
{
	nativelog("[dsl] trigger_pattern_recognition called");

	// Collect recent episodic memories
	$episodicMemories = semantic_get_recent(@EpisodicMemory, ($recentCount > 0 ? $recentCount : 20));
	if(isnullorempty($episodicMemories) || $episodicMemories == "[]"){
		nativelog("[dsl] trigger_pattern_recognition: no episodic memories, skip");
		return;
	};

	// Read existing pattern MD files
	$existingPatterns = read_all_pattern_md();

	$prompt = format("【情景记忆列表】：\n{0}", $episodicMemories);
	if(!isnullorempty($existingPatterns)){
		$prompt = format("{0}\n\n【已有模式记忆（需合并去重）】：\n{1}", $prompt, $existingPatterns);
	};

	// Set pattern recognition system prompt with category output requirement
	$sysPrompt = "根据情景记忆识别重复模式，与已有模式记忆合并去重。" +
		"严格要求：每个模式必须引用具体情景记忆作为证据，禁止空洞归纳。" +
		"禁止出现'建立xxx机制'、'深化xxx认知'等口号式描述。\n" +
		"必须按以下类别分类输出，每个类别用标签包裹：\n" +
		"[CATEGORY:tool_usage] 工具使用经验 [/CATEGORY]\n" +
		"[CATEGORY:code_refactor] 代码改造经验 [/CATEGORY]\n" +
		"[CATEGORY:architecture] 架构设计经验 [/CATEGORY]\n" +
		"[CATEGORY:project_mgmt] 项目管理经验 [/CATEGORY]\n" +
		"[CATEGORY:debugging] 调试排错经验 [/CATEGORY]\n" +
		"[CATEGORY:general] 通用经验 [/CATEGORY]\n\n" +
		"每个类别内容格式：\n" +
		"- 模式：具体行为模式描述\n" +
		"  证据：支撑的具体事例\n" +
		"  建议：具体操作建议\n\n" +
		"如果某类别无内容则省略该标签。与已有内容合并时去除重复，保留最精炼版本。" +
		"每个类别控制在200字以内，一次回复输出完成（不要使用记忆tool，结果数据持续入库）";
	llm_set_system_prompt(@LlmProviderId, "pattern_recognition", "pattern_recognition", $sysPrompt);

	// Send pattern recognition request
	$prompt = format("{0}\n\n请识别模式并按类别输出，与已有内容合并去重。", $prompt);
	$prompt = getstringinlength($prompt, 100 * 1024, 1);
	llm_chat_callback(@LlmProviderId, "pattern_recognition", "pattern_recognition", $prompt);

	nativelog("[dsl] trigger_pattern_recognition: pattern recognition request sent");
};

script(trigger_meta_cognition)params()
{
	nativelog("[dsl] trigger_meta_cognition called");

	// Collect recent pattern memories
	$patternMemories = semantic_get_recent(@PatternMemory, 20);
	if(isnullorempty($patternMemories) || $patternMemories == "[]"){
		nativelog("[dsl] trigger_meta_cognition: no pattern memories, skip");
		return;
	};

	$episodicMemories = semantic_get_recent(@EpisodicMemory, 10);

	// Read existing meta_cognition.md
	$commonDir = combine_path(basepath, "docs/memory");
	$metaPath = combine_path($commonDir, "meta_cognition.md");
	$existingMeta = "";
	if(file_exists($metaPath)){
		$existingMeta = read_file($metaPath);
	};

	$prompt = format("【模式记忆】：\n{0}\n\n【近期情景记忆】：\n{1}", $patternMemories, $episodicMemories);
	if(!isnullorempty($existingMeta)){
		$prompt = format("{0}\n\n【已有元认知（需合并去重）】：\n{1}", $prompt, $existingMeta);
	};

	// Set meta-cognition system prompt
	$sysPrompt = "根据模式记忆和情景记忆，进行高层反思，与已有元认知合并去重。" +
		"严格要求：每条原则必须附带具体案例，禁止空洞口号。" +
		"禁止出现'建立xxx机制'、'深化xxx认知'、'强化xxx能力'等虚无描述。\n" +
		"输出格式：\n" +
		"## 决策原则\n" +
		"- 原则描述（附具体案例）\n\n" +
		"## 认知偏差\n" +
		"- 偏差描述（附触发场景）\n\n" +
		"## 改进建议\n" +
		"- 具体可执行的操作建议\n\n" +
		"与已有内容合并时去除重复，保留最精炼版本。" +
		"控制在500字以内，一次回复输出完成（不要使用记忆tool，结果数据持续入库）";
	llm_set_system_prompt(@LlmProviderId, "meta_cognition", "meta_cognition", $sysPrompt);

	// Send meta-cognition request
	$prompt = format("{0}\n\n请进行元认知反思，与已有内容合并去重。", $prompt);
	$prompt = getstringinlength($prompt, 100 * 1024, 1);
	llm_chat_callback(@LlmProviderId, "meta_cognition", "meta_cognition", $prompt);

	nativelog("[dsl] trigger_meta_cognition: meta-cognition request sent");
};
script(SaveHistory)params()
{
	$conversationCount = semantic_count(@LegionnaireHistory);
	$lastHistoryCount = get_context_var("LastHistoryCount", "session");
	if (isnull($lastHistoryCount) || $lastHistoryCount == 0) {
		set_context_var("LastHistoryCount", $conversationCount, "session");
		return;
	};
	$historyCount = $conversationCount - $lastHistoryCount;
	$histories = semantic_get_recent_as_list(@LegionnaireHistory, $historyCount);
	$history = new_string_builder();
	looplist($histories) {
		$rec = $$;
		append_format_line($history, "{0} {1}", $rec.Content, $rec.Metadata);
	};
	$historyFile = combine_path(@ProjectDirectory, "docs/history.txt");
	$historyStr = string_builder_to_string($history);
	write_file($historyFile, $historyStr);
	set_history($historyStr);
};

// Handle agent notification
script(handle_agent_notification)params($jsonData)
{
	nativelog("[dsl] handle_agent_notification called with: {0}", getstringinlength($jsonData,100));

	// Parse the notification using parse_agent_notification API
	$notif = parse_agent_notification($jsonData);
	$type = get_message_param($notif, "type");

	nativelog("[dsl] Notification type: {0}", $type);

	if ($type == "agent_ready") {
		nativelog("[dsl] Agent ready notification: {0}", getstringinlength($jsonData,100));

		$data = get_message_param($notif, "data");
		$pageType = get_message_param($data, "pageType");
		$url = get_message_param($data, "url");

		nativelog("[dsl] Agent initialized on page type: {0}, url: {1}", $pageType, $url);

		// Agent is initialized and ready to receive commands
		// You can send initial commands here
		ws_start_server(9527);

		// Auto-start MetaDSL Worker on port 9527 with auto-reconnect enabled
		send_command_to_inject("ws_start", to_json({port: 9527}));

		if (@EnableLlmPM) {
			llm_clear_history(@LlmProviderId, "llm_pm_align");
			llm_clear_history(@LlmProviderId, "llm_pm_context");
			llm_clear_history(@LlmProviderId, "llm_pm_project");
		};
	}
	elif ($type == "llm_update_system_prompt") {
		nativelog("[dsl] LLM update system prompt notification received");

		$data = get_message_param($notif, "data");
		$pageType = get_message_param($data, "pageType");

		// Read context file
		$basePromptFile = combine_path(basepath, "docs/system_prompt.txt");
		$toplevelRulesFile = combine_path(basepath, "docs/rules_toplevel.txt");
		$emphasizeFile = combine_path(basepath, "docs/emphasize.txt");
		$projectPromptFile = combine_path(@ProjectDirectory, "docs/project_prompt.txt");
		$planFile = combine_path(@ProjectDirectory, "docs/plan.txt");
		$soulFile = combine_path(@ProjectDirectory, "docs/soul.md");

		$basePrompt = read_file($basePromptFile);
		$toplevelRules = read_file($toplevelRulesFile);
		$projectPrompt = read_file($projectPromptFile);
		$emphasize = read_file($emphasizeFile);
		$emphasize = format("{0}\n\n## 当前日期\n{1}\n（请对照 soul.md 的‘回顾节奏’小节，如已到或逾期‘下次回顾’日期，请在本轮回复开头主动提醒用户进行回顾）", $emphasize, date_time_str("yyyy-MM-dd"));
		$plan = read_file($planFile);
		$soul = read_file($soulFile);

		nativelog("[dsl] Base prompt length: {0}", $basePrompt.Length);
		nativelog("[dsl] Toplevel rules length: {0}", $toplevelRules.Length);
		nativelog("[dsl] Project prompt length: {0}", $projectPrompt.Length);
		nativelog("[dsl] Emphasize length: {0}", $emphasize.Length);
		nativelog("[dsl] Plan length: {0}", $plan.Length);
		nativelog("[dsl] Soul length: {0}", $soul.Length);

		if ($pageType == "local-agent") {
			$prompt = $projectPrompt + "\n\n" + $emphasize;
		} else {
			$prompt = $basePrompt + "\n\n" + $toplevelRules + "\n\n" + $projectPrompt + "\n\n" + $emphasize;
		};
		set_system_prompt($prompt);
		set_project_prompt($projectPrompt);
		set_emphasize($emphasize);
		set_plan($plan);
		set_soul($soul);
		set_context(read_file(combine_path(@ProjectDirectory, "docs/context.txt")));
		set_todo(read_file(combine_path(@ProjectDirectory, "docs/todo.txt")));
		set_history(read_file(combine_path(@ProjectDirectory, "docs/history.txt")));

		send_command_to_inject("update_system_prompt", to_json({prompt: $prompt}));

		if (@EnableLlmPM) {
			$note = "你作为PM，要特别注意，一切以对话事实信息为准，不要猜测，缺少信息的保持现状不修改";
			$llm_sys_prompt = format("{0}\n\n{1}", $note, $projectPrompt);
			llm_set_system_prompt(@LlmProviderId, "llm_pm_align", $llm_sys_prompt);
			llm_set_system_prompt(@LlmProviderId, "llm_pm_context", $llm_sys_prompt);
			llm_set_system_prompt(@LlmProviderId, "llm_pm_project", $llm_sys_prompt);
		};

		induction_plan();
	}
	elif ($type == "llm_context_count_down") {
		nativelog("[dsl] LLM context count down notification received");

		// Read context file
		$contextFile = combine_path(@ProjectDirectory, "docs/context.txt");

		$data = get_message_param($notif, "data");
		$pageType = get_message_param($data, "pageType");
		$queuedCount = get_message_param($data, "queuedCount");
		$count = get_message_param($data, "count");

		nativelog("[dsl] llm_context_count_down pageType: {0}, queuedCount: {1}, count: {2}", $pageType, $queuedCount, $count);

		induction_context($count, $queuedCount, $pageType);
		trigger_reflection();
	}
	elif ($type == "llm_align_target") {
		nativelog("[dsl] LLM align target notification received");

		$data = get_message_param($notif, "data");
		$pageType = get_message_param($data, "pageType");
		$queuedCount = get_message_param($data, "queuedCount");
		$count = get_message_param($data, "count");

		nativelog("[dsl] llm_align_target pageType: {0}, queuedCount: {1}, count: {2}", $pageType, $queuedCount, $count);

		induction_todo($count, $queuedCount, $pageType);
	}
	elif ($type == "episodic_reflection") {
		nativelog("[dsl] episodic_reflection notification received");
		trigger_reflection();
	}
	elif ($type == "save_conversation_history") {
		nativelog("[dsl] save_conversation_history notification received");

		$data = get_message_param($notif, "data");
		$conversations = get_message_param($data, "conversations");
		$count = size($conversations);

		set_plan(read_file(combine_path(@ProjectDirectory, "docs/plan.txt")));
		set_context(read_file(combine_path(@ProjectDirectory, "docs/context.txt")));
		set_todo(read_file(combine_path(@ProjectDirectory, "docs/todo.txt")));

		nativelog("[dsl] Saving {0} new conversation(s) to history", $count);

		loop($count) {
			$i = $$;
			$conv = $conversations[$i];
			$user = get_message_param($conv, "user");
			$assistant = get_message_param($conv, "assistant");
			$content = format("User:\n{0}\n\nAssistant:\n{1}", $user, $assistant);
			semantic_add(@LegionnaireHistory, $content, to_json({source: "inject", index: $i, date: date_time_str()}));
			nativelog("[dsl] Saved conversation {0}/{1}", $i + 1, $count);
		};

		SaveHistory();

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

		induction_info($legionnaireBatch, $legionnaires, "llm_pm_decurion");
		induction_info($decurionBatch, $decurions, "llm_pm_centurion");
		induction_info($centurionBatch, $centurions, "llm_pm_chiliarch");
		induction_info($chiliarchBatch, $chiliarchs, "llm_pm_marquis");

		nativelog("[dsl] Saved Induction, LegionnaireBatch: {0}, DecurionBatch: {1}, CenturionBatch: {2}, ChiliarchBatch: {3}", $legionnaireBatch, $decurionBatch, $centurionBatch, $chiliarchBatch);

		// Check episodic memory count and trigger pattern recognition
		$episodicCount = semantic_count(@EpisodicMemory);
		$patternCount = semantic_count(@PatternMemory);
		if ($episodicCount >= ($patternCount + 1) * 5) {
			$newCount = $episodicCount - $patternCount * 5;
			nativelog("[dsl] Episodic memory count {0}, pattern count {1}, triggering pattern recognition", $episodicCount, $patternCount);
			trigger_pattern_recognition($newCount);
		};
		$metaCount = semantic_count(@MetaCognitionMemory);
		if ($patternCount >= ($metaCount + 1) * 3) {
			nativelog("[dsl] Pattern memory count {0}, meta-cognition count {1}, triggering meta-cognition", $patternCount, $metaCount);
			trigger_meta_cognition();
		};
		if (add_cur_context_rounds() == 0) {
			$prompt = format("【最近会话】:{0}\n\n【todo】:{1}\n\n【上下文信息】:{2}", get_history(), get_todo(), get_context());
			send_command_to_inject("send_message", to_json({text: $prompt}));
		};
	}
	elif ($type == "agent_need_to_plan") {
		// Planning heuristics have been moved to JS (plan_decider.js).
		// JS now only sends this notification when it decides planning is needed.
		nativelog("[dsl] agent_need_to_plan notification received (trigger_plan)");

		$data = get_message_param($notif, "data");
		$queuedCount = get_message_param($data, "queuedCount");
		$pageType = get_message_param($data, "pageType");
		$count = get_message_param($data, "count");
		$lockAgent = get_message_param($data, "lockAgent");
		$planPath = combine_path(@ProjectDirectory, "docs/plan.txt");
		$soulPath = combine_path(@ProjectDirectory, "docs/soul.md");

		set_soul(read_file($soulPath));

		nativelog("[dsl] agent_need_to_plan: queued:{0} page:{1} count:{2}", $queuedCount, $pageType, $count);

		if (file_exists($planPath)) {
			nativelog("[dsl] induction_plan triggered");
			induction_plan();

			if ($lockAgent) {
				$prompt = "长时间开发模式下不要等用户桷认（用户不在线），请参考todo与plan的内容继续开发工作";
			}
			else {
				$prompt = "正在更新计划，请继续";
			};
			send_command_to_inject("send_message", to_json({text: $prompt}));
		}
		else {
			nativelog("[dsl] agent_need_to_plan: plan file not found, skip induction_plan");
		};
	}
	else {
		nativelog("[dsl] Unknown notification type: {0}", $type);
	};
};
