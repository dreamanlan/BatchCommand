// Note:The design philosophy behind these DSL scripts is to be stateless;
// all state resides within C# or JS. The DSL's global variables are utilized
// for configuring constants, and each hot-reload operation executes independently.
script(init_global_consts)
{
	@EnableLlmPM = true;
	//@LlmProviderId = "ollama_local";
	@LlmProviderId = "auto_metadsl";

	if (processtype == 1) {
		@ProjectIdentity = agent_get_project_identity(9530);
		@ProjectDirectory = agent_get_project_dir(9530);
		if (!isnullorempty(@ProjectIdentity)) {
			@MarquisHistory = @ProjectIdentity + "_marquis_history";
			@ChiliarchHistory = @ProjectIdentity + "_chiliarch_history";
			@CenturionHistory = @ProjectIdentity + "_centurion_history";
			@DecurionHistory = @ProjectIdentity + "_decurion_history";
			@LegionnaireHistory = @ProjectIdentity + "_legionnaire_history";
			@EpisodicMemory = @ProjectIdentity + "_episodic_memory";
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
			$targetBrowserId = find_browser_id_by_url_key("evaluation.woa.com/chat");
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
	elif (stringcontainsany($url, "https://evaluation.woa.com/chat", "https://www.google.com")) {
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
	if (string_contains_any($url, "https://www.google.com/ai", "https://www.google.com/search") && ($isMainFrame == "True" || $isMainFrame == true)) {
		$base = combine_path(basepath, "managed/inject_modules/");
		$sb = new_string_builder();
		append_line($sb, read_file(combine_path($base, "google_ai_search.js")));
		$code = string_builder_to_string($sb);
		nativelog("[dsl] on_renderer_load_end: injecting {0} bytes of JS code", strlen($code));
		agent_set_max_result_size(9530, 7168);
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
		elif (string_contains($msg, ".")) {
			//ignore messages with dot in name, which are likely from cef
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

script(get_arena_system_prompt)params()
{
	$arenaPrompt = read_file("d:/AiClaw/docs/arena_prompt.txt");
	return(format("{0}",$arenaPrompt));
};
script(get_venus_system_prompt)params()
{
	$mergePrompt = read_file("d:/AiClaw/docs/venus_prompt.txt");
	return(format("{0}",$mergePrompt));
};
script(get_system_prompt_1)params()
{
	$googlePrompt = read_file("d:/AiClaw/docs/google_prompt_1.txt");
	return(format("{0}",$googlePrompt));
};
script(get_system_prompt_2)params()
{
	$googlePrompt = read_file("d:/AiClaw/docs/google_prompt_2.txt");
	return(format("{0}",$googlePrompt));
};
script(get_system_prompt_3)params()
{
	$googlePrompt = read_file("d:/AiClaw/docs/google_prompt_3.txt");
	return(format("{0}",$googlePrompt));
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
	llm_set_provider("ollama", "ollama", "http://localhost:11434", "", "gemma3:4b");
	llm_set_provider("auto_metadsl", "auto_metadsl", "https://knot.woa.com/apigw/api/v1/agents/agui/114631ca85184f639f69572bbcfcbe7a", "%person_token%", "glm-5");

	// Search services configured here; apiKey uses %var% placeholders
	brave_set_api_key("%brave_api_key%");
	searxng_set_url("https://www.gamexyz.net:8090");
};

// Handle agent notification
script(handle_agent_notification)params($jsonData)
{
	nativelog("[dsl] handle_agent_notification called with: {0}", getstringinlength($jsonData,100));

	// Parse the notification using parse_agent_notification API
	$notif = parse_agent_notification($jsonData);
	$type = get_message_param($notif, "type");

	nativelog("[dsl] Notification type: {0}", $type);

	if ($type == "aiclaw_ready") {
		nativelog("[dsl] AiClaw ready notification: {0}", getstringinlength($jsonData,100));

		$data = get_message_param($notif, "data");
		$url = get_message_param($data, "url");
		nativelog("[dsl] AiClaw initialized, url: {0}", $url);

		agent_enable_context_injection(9530, false);
		agent_set_project_dir(9530, "d:/AiClaw");
		agent_set_project_identity(9530, "aiclaw");
		// Start WebSocket server on port 9530 for aiclaw bridge communication
		ws_start_server(9530);

		// Notify JS to connect multi-slot WS to port 9530
		send_command_to_inject("ws_start", to_json({port: 9530}));

		nativelog("[dsl] AiClaw bridge WS server started on 9530");
	}
	else {
		nativelog("[dsl] Unknown notification type: {0}", $type);
	};
};
