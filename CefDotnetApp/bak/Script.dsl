script(init_global_vars)
{
	@CefDotnetAppSrcPath = "D:/GitHub/BatchCommand/CefDotnetApp";
	@AgentCoreSrcPath = "D:/GitHub/BatchCommand/AgentCore";
};
script(on_init)
{
	init_global_vars();

    nativelog("[dsl] on_init finish");
    fileecho(true);
};
script(on_finalize)
{
    nativelog("[dsl] on_finalize finish");
};

script(on_browser_init)
{
    nativelog("[dsl] on_browser_init finish");
};
script(on_browser_finalize)
{
    nativelog("[dsl] on_browser_finalize finish");
};

script(on_renderer_init)params($url)
{
    nativelog("[dsl] on_renderer_init finish, url: {0}", $url);
};
script(on_renderer_finalize)
{
    nativelog("[dsl] on_renderer_finalize finish");
};

script(on_before_command_line_processing)params($processType)
{
	// Add command line switches here
	// Example: nativeapi.AppendSwitch("disable-gpu");
	// Example: nativeapi.AppendSwitchWithValue("remote-debugging-port", "9222");

	// Check if a switch exists
	// if (!nativeapi.HasSwitch("disable-gpu")) {
	//     nativeapi.AppendSwitch("disable-gpu");
	// };

	$url = nativeapi.GetSwitchValue("url");

	nativelog("[dsl] on_before_command_line_processing: process_type={0}, url={1}", $processType, $url);

	if (stringcontainsany($url, "file:///", "http://localhost") && stringcontainsany($url, "AgentCore/hotreload_test.html", "http://localhost:8080/agent.html", "http://localhost:8081")) {
		nativeapi.AppendSwitch("disable-web-security");
		nativeapi.AppendSwitch("allow-file-access-from-files");
	}
	elif (stringcontainsany($url, "https://evaluation.woa.com/chat")) {
		nativeapi.AppendSwitch("disable-web-security");
	};
	//nativeapi.AppendSwitch("disable-web-security");
	//nativeapi.AppendSwitch("allow-file-access-from-files");
	nativeapi.AppendSwitch("disable-site-isolation-trials");
};

script(on_load_end)params($url,$httpStatusCode,$injectAllFrame,$isMainFrame)
{
	nativelog("[dsl] on_load_end:{0} {1} {2} {3}", $url, $httpStatusCode, $injectAllFrame, $isMainFrame);
	return((false, "alert('custom_code')"));
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
};

script(on_receive_cef_message)params($msg,$arg1)
{
	nativelog("[dsl] on_receive_cef_message:{0} arg:{1} arg_num:{2} from:{3} processtype:{4}",$msg,$arg1,argnum(),sourceprocessid,processtype);
	if (processtype == 0) {
		//browser echo
		nativeapi.SendCefMessageForDSL($msg,[$arg1],sourceprocessid);
	}
	elif (processtype == 1) {
		//renderer echo
		nativeapi.SendJavascriptCallForDSL("alert", [$msg]);
	};
};

script(on_receive_js_message)params($msg,$arg1)
{
	nativelog("[dsl] on_receive_js_message:{0} arg:{1} arg_num:{2} processtype:{3}",$msg,$arg1,argnum(),processtype);

	// Handle debug log from inject.js
	if ($msg == "debug_log") {
		nativelog("[inject.js] {0}", $arg1);
	}
	elif ($msg == "debug_log_batch") {
		handle_nativelog_batch($arg1);
	}
	// Handle agent commands from inject.js
	elif ($msg == "agent_command") {
		handle_agent_command($arg1);
	}
	elif ($msg == "agent_notification") {
		handle_agent_notification($arg1);
	}
	else {
		// Default: transfer to browser process
		nativeapi.SendCefMessage1($msg,$arg1,0);
	};
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
// Handle agent command
script(handle_agent_command)params($jsonData)
{
	nativelog("[dsl] handle_agent_command called with: {0}", $jsonData);

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
		handlethreadqueue();
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

// Handle agent notification
script(handle_agent_notification)params($jsonData)
{
	nativelog("[dsl] handle_agent_notification called with: {0}", $jsonData);

	// Parse the notification JSON to get the data field
	$parsed = from_json($jsonData);
	$type = get_message_param($parsed, "type");

	nativelog("[dsl] Notification type: {0}", $type);

	if ($type == "agent_ready") {
		nativelog("[dsl] Agent ready notification: {0}", $jsonData);

		$data = get_message_param($parsed, "data");
		$pageType = get_message_param($data, "pageType");
		$url = get_message_param($data, "url");

		nativelog("[dsl] Agent initialized on page type: {0}, url: {1}", $pageType, $url);

		// Agent is initialized and ready to receive commands
		// You can send initial commands here
		ws_start_server(9527);

		// Auto-start MetaDSL Worker on port 9527 with auto-reconnect enabled
		send_command_to_inject("ws_start", to_json({port: 9527}));
	}
	elif ($type == "llm_update_system_prompt") {
		nativelog("[dsl] LLM update system prompt notification received");

		$data = get_message_param($parsed, "data");
		$pageType = get_message_param($data, "pageType");

		// Read context file
		$basePromptFile = combine_path(@CefDotnetAppSrcPath, "system_prompt.txt");
		$toplevelRulesFile = combine_path(@CefDotnetAppSrcPath, "rules_toplevel.txt");
		$gamePromptFile = combine_path(@AgentCoreSrcPath, "game_prompt.txt");

		$basePrompt = readalltext($basePromptFile);
		$toplevelRules = readalltext($toplevelRulesFile);
		$gamePrompt = readalltext($gamePromptFile);

		nativelog("[dsl] Base prompt length: {0}", $basePrompt.Length);
		nativelog("[dsl] Toplevel rules length: {0}", $toplevelRules.Length);
		nativelog("[dsl] Game prompt length: {0}", $gamePrompt.Length);

		if ($pageType == "local-agent") {
			$prompt = $gamePrompt;
		} else {
			$prompt = $basePrompt + "\n\n" + $toplevelRules + "\n\n" + $gamePrompt;
		};
		nativelog("[dsl] Sending context prompt to LLM, length: {0}", $prompt.Length);

		// Send to LLM via inject.js
		// Use send_command_to_inject to call inject.js API
		send_command_to_inject("update_system_prompt", to_json({prompt: $prompt}));
	}
	elif ($type == "llm_context_count_down") {
		nativelog("[dsl] LLM context count down notification received");

		delete_file(combine_path(appdir, "console.log"));

		// Read context file
		$contextFile = combine_path(@CefDotnetAppSrcPath, "context.txt");

		$data = get_message_param($parsed, "data");
		$pageType = get_message_param($data, "pageType");
		$history = get_message_param($data, "history");

		//writealltext($contextFile, $history);

		// Organize context data with prompt
		$len = strlen($history);
		$prompt = ($len > 128*1024 ? substring($history, $len - 128*1024, 128*1024) : $history);
		$prompt = format("{0}\n\n以上内容为最近对话历史，结合系统介绍、开发计划与开发进度，你总结一下关键内容，\n" +
				"并使用metadsl代码写入{1}，\n" +
				"记得metadsl代码里不能有markdown代码块标记，所以文档内容格式要简洁", $prompt, $contextFile);

		nativelog("[dsl] Sending context prompt to LLM, length: {0}", $prompt.Length);

		// Send to LLM via inject.js
		// Use send_command_to_inject to call inject.js API
		send_command_to_inject("send_message", to_json({text: $prompt}));
	}
	elif ($type == "llm_align_target") {
		nativelog("[dsl] LLM align target notification received");

		// Read prompt file
		$promptFile = combine_path(@AgentCoreSrcPath, "todo.txt");
		$prompt = "我们对齐一下目标，你不要一直查看代码，按目标做实际工作。\n" + readalltext($promptFile);
		$prompt = $prompt + "\n\n【你最近一次的上下文总结：】\n" + readalltext(combine_path(@CefDotnetAppSrcPath, "context.txt"));

		nativelog("[dsl] Sending system prompt to LLM, length: {0}", $prompt.Length);

		// Send to LLM via inject.js
		// Use send_command_to_inject to call inject.js API
		send_command_to_inject("send_message", to_json({text: $prompt}));
	}
	elif ($type == "agent_need_to_plan") {
		nativelog("[dsl] agent_need_to_plan notification received");

		$data = get_message_param($parsed, "data");
		$lastFromLLM = get_message_param($data, "lastFromLLM");
		$lastMessage = string_trim(get_message_param($data, "lastMessage"));

		nativelog("[dsl] agent_need_to_plan notification: {0} {1}", $lastFromLLM, len($lastMessage)>100 ? substring($lastMessage,0,100)+"..." : $lastMessage);

		$planPath = combine_path(@AgentCoreSrcPath, "plan.txt");

		if ($lastFromLLM == "True") {
			if ($lastMessage == "继续" || $lastMessage == "继续。") {
				send_command_to_inject("send_message", to_json({text: "没有代码要执行了"}));
				return(true);
			}
			elif (string_contains($lastMessage, "需要", "继续", "吗") && string_length($lastMessage) > 16) {
				send_command_to_inject("send_message", to_json({text: "继续"}));
				return(true);
			}
			elif (string_contains($lastMessage, "Error", "Occur")) {
				send_command_to_inject("send_message", to_json({text: "继续"}));
				return(true);
			};
		}
		elif (string_contains($lastMessage, "MetaDSL", "hot_reload")) {
			send_command_to_inject("send_message", to_json({text: "热更完成，继续"}));
			return(true);
		}
		elif (file_exists($planPath)) {
			$plan = readalltext($planPath);
			if (strlen($plan) > 0) {
				nativelog("[dsl] Found existing plan: {0}", $plan);
				// Send existing plan to LLM
				send_command_to_inject("send_message", to_json({text: $plan}));
				nativelog("[dsl] Sending planning prompt to LLM");
				return(true);
			};
		}
		else {
			send_command_to_inject("send_message", to_json({text: "你现在在做什么呢？"}));
			return(true);
		};
	};
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
