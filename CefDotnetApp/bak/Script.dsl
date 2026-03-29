// Note:The design philosophy behind these DSL scripts is to be stateless;
// all state resides within C# or JS. The DSL's global variables are utilized
// for configuring constants, and each hot-reload operation executes independently.
script(init_global_consts)
{
	@EnableLlmPM = true;
	//@LlmProviderId = "ollama_local";
	@LlmProviderId = "auto_metadsl";
	@LastHistoryCount = 0;

	@ProjectIdentity = get_project_identity();
	@ProjectDirectory = get_project_dir();
	if (!stringisnullorempty(@ProjectIdentity)) {
		@MarquisHistory = @ProjectIdentity + "_marquis_history";
		@ChiliarchHistory = @ProjectIdentity + "_chiliarch_history";
		@CenturionHistory = @ProjectIdentity + "_centurion_history";
		@DecurionHistory = @ProjectIdentity + "_decurion_history";
		@LegionnaireHistory = @ProjectIdentity + "_legionnaire_history";
	};
};
script(on_init)
{
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

script(on_before_child_process_launch)params($processType, $cmdLine)
{
	// $cmdLine is the child process command line (passed as parameter)
	nativelog("[dsl] on_before_child_process_launch {0}", $processType);
	//debuggerbreak();

	// Copy custom switches from browser process to child process
	if (initialprojectidentityinited) {
		$cmdLine.AppendSwitchWithValue("projectidentity", initialprojectidentity);
		nativelog("[dsl] on_before_child_process_launch: copied --initialprojectidentity={0} to child process", initialprojectidentity);
	};
	if (dslfilechanged) {
		$cmdLine.AppendSwitchWithValue("metadsl", dslfile);
		nativelog("[dsl] on_before_child_process_launch: copied --metadsl={0} to child process", dslfile);
	};
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
script(on_before_resource_load)params($request)
{
	nativelog("[dsl] on_before_resource_load: url={0} method={1}", $request.Url, $request.Method);
	return((false, 1));
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
script(on_renderer_load_start)params($url,$transitionType,$isMainFrame)
{
	nativelog("[dsl] on_renderer_load_start:{0} {1} {2}", $url, $transitionType, $isMainFrame);
};
script(on_renderer_load_end)params($url,$httpStatusCode,$isMainFrame)
{
	nativelog("[dsl] on_renderer_load_end:{0} {1} {2}", $url, $httpStatusCode, $isMainFrame);
	if ($isMainFrame == "True" || $isMainFrame == true) {
		$base = combine_path(basepath, "managed/inject_modules/");
		$sb = new_string_builder();
		append_line($sb, "(function() {");
		append_line($sb, "  'use strict';");
		append_line($sb, "  let bridge = null;");
		append_line($sb, "  let pageAdapter = null;");
		append_line($sb, "  let metadslMonitor = null;");
		append_line($sb, "  let panel = null;");
		append_line($sb, "  let gameWindow = null;");
		append_line($sb, read_file(combine_path($base, "config.js")));
		append_line($sb, read_file(combine_path($base, "secret_store.js")));
		append_line($sb, read_file(combine_path($base, "ws_worker.js")));
		append_line($sb, read_file(combine_path($base, "logger.js")));
		append_line($sb, read_file(combine_path($base, "bridge.js")));
		append_line($sb, read_file(combine_path($base, "input_monitor.js")));
		append_line($sb, read_file(combine_path($base, "state_machine.js")));
		append_line($sb, read_file(combine_path($base, "page_adapter.js")));
		append_line($sb, read_file(combine_path($base, "metadsl_monitor.js")));
		append_line($sb, read_file(combine_path($base, "panel.js")));
		append_line($sb, read_file(combine_path($base, "chat_input.js")));
		append_line($sb, read_file(combine_path($base, "openclaw_http.js")));
		append_line($sb, read_file(combine_path($base, "openclaw_ws.js")));
		append_line($sb, read_file(combine_path($base, "openclaw_panel.js")));
		append_line($sb, read_file(combine_path($base, "project_panel.js")));
		append_line($sb, read_file(combine_path($base, "spellcheck.js")));
		append_line($sb, read_file(combine_path($base, "ws_manager.js")));
		append_line($sb, read_file(combine_path($base, "main.js")));
		append_line($sb, "})();");
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

script(on_receive_cef_message)params($srcProcId,$msg,$arg1,$arg2,$arg3,$arg4)
{
	nativelog("[dsl] on_receive_cef_message:{0} argnum:{1} from:{2} processtype:{3}",$msg,argnum(),$srcProcId,processtype);
	if (processtype == 0) {
		//Browser: forward all cef messages back to renderer
		//Note: The API in AgentCore.dll cannot be used.
		$targs = clone(args());
		listremoveat($targs, 0);
		listremoveat($targs, 0);
		nativeapi.SendCefMessageForDSL($msg,$targs,$srcProcId);
	}
	elif (processtype == 1) {
		//Renderer
		if ($msg == "llm_callback") {
			handle_llm_callback($arg1, $arg2, $arg3, $arg4);
		}
		elif ($msg == "mcp_callback") {
			handle_mcp_callback($arg1, $arg2, $arg3);
		}
		elif ($msg == "skill_callback") {
			handle_skill_callback($arg1, $arg2, $arg3, $arg4);
		}
		elif ($msg == "command_callback") {
			handle_command_callback($arg1, $arg2, $arg3, $arg4);
		}
		else {
			//nativeapi.SendJavascriptCallForDSL("alert", [$msg]);
		};
	};
};

script(on_receive_js_message)params($srcProcId,$msg,$arg1)
{
	nativelog("[dsl] on_receive_js_message:{0} arg_num:{1} processtype:{2}",$msg,argnum(),processtype);

	// Handle debug log from inject.js
	if ($msg == "debug_log") {
		nativelog("[inject.js] {0}", getstringinlength($arg1,100));
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

		send_command_to_inject("send_message", to_json({text: $reply}));
	}
	elif ($tag == "llm_pm_context") {
		$contextFile = combine_path(@ProjectDirectory, "docs/context.txt");
		write_file($contextFile, $reply);
		set_context($reply);
		llm_clear_history(@LlmProviderId, $tag);
	}
	elif ($tag == "llm_pm_plan") {
		$planFile = combine_path(@ProjectDirectory, "docs/plan.txt");
		write_file($planFile, $reply);
		set_plan($reply);
		llm_clear_history(@LlmProviderId, $tag);
	}
	elif ($tag == "llm_pm_align") {
		$todoFile = combine_path(@ProjectDirectory, "docs/todo.txt");
		write_file($todoFile, $reply);
		set_todo($reply);
		llm_clear_history(@LlmProviderId, $tag);
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
		send_command_to_inject("send_llm_callback", to_json({text: $reply}));
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
		handlethreadqueue();
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

	$result = llm_chat($providerId, $tag, $topic, $text);

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

		llm_chat(@LlmProviderId, $session, "induction", format("{0}\n\n以上是最近10次工作信息，在不丢失关键信息的前提下，总结成一段话（一次回复输出完成）", string_builder_to_string($induction)));
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
		"，基于事实，不要猜测臆想（分章节分阶段分步骤，一次回复输出完成，控制在2000字以内）", $prompt);

	if (@EnableLlmPM) {
		$prompt = getstringinlength($prompt, 100 * 1024, 1);
		llm_chat(@LlmProviderId, "llm_pm_project", "project_history", $prompt);
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

	@LastHistoryCount = semantic_count(@LegionnaireHistory);

	$planHistory = read_file(combine_path(@ProjectDirectory, "docs/plan.txt"));
	$todoHistory = read_file(combine_path(@ProjectDirectory, "docs/todo.txt"));
	$contextHistory = read_file(combine_path(@ProjectDirectory, "docs/context.txt"));

	$prompt = format("【以下是最近的开发计划】：\n{0}\n" +
		"【以下是最近的待办事项】：\n{1}\n" +
		"【以下是最近上下文信息】：\n{2}\n" +
		"【以下是最近对话历史】：\n{3}{4}{5}{6}{7}", $planHistory, $todoHistory, $contextHistory, $marquisHistory, $chiliarchHistory, $centurionHistory, $decurionHistory, $conversationHistory);
	$prompt = format("{0}\n\n以上内容结合系统介绍、开发计划与开发进度，以PM身份总结一下清晰详尽的关键内容" +
		"，基于事实，不要猜测臆想（分章节分阶段分步骤，一次回复输出完成），" +
		"除总结外，增加一个近期会话章节，总结最近的会话内容（重新整理，去掉json格式与重复信息，控制在3000字以内）", $prompt);

	if (@EnableLlmPM) {
		$prompt = getstringinlength($prompt, 100 * 1024, 1);
		llm_chat(@LlmProviderId, "llm_pm_context", "context_summary", $prompt);
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
		llm_chat(@LlmProviderId, "llm_pm_align", "align_target", $prompt);
	}
	else {
		$prompt = format("{0}\n\n并使用metadsl代码写入{1}，\n" +
			"记得metadsl代码里不能有markdown代码块标记，所以文档内容格式要简洁", $prompt, $todoFile);
		$prompt = getstringinlength($prompt, 100 * 1024, 1);
		send_command_to_inject("send_message", to_json({text: $prompt}));
	};
};

script(SaveHistory)params()
{
	$conversationCount = semantic_count(@LegionnaireHistory);
	if (@LastHistoryCount == 0) {
		@LastHistoryCount = $conversationCount;
		return;
	};
	$historyCount = $conversationCount - @LastHistoryCount;
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
			llm_clear_history(@LlmProviderId, "llm_pm_plan");
			llm_clear_history(@LlmProviderId, "llm_pm_align");
			llm_clear_history(@LlmProviderId, "llm_pm_context");
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

		$basePrompt = read_file($basePromptFile);
		$toplevelRules = read_file($toplevelRulesFile);
		$projectPrompt = read_file($projectPromptFile);
		$emphasize = read_file($emphasizeFile);
		$plan = read_file($planFile);

		nativelog("[dsl] Base prompt length: {0}", $basePrompt.Length);
		nativelog("[dsl] Toplevel rules length: {0}", $toplevelRules.Length);
		nativelog("[dsl] Project prompt length: {0}", $projectPrompt.Length);
		nativelog("[dsl] Emphasize length: {0}", $emphasize.Length);
		nativelog("[dsl] Plan length: {0}", $plan.Length);

		if ($pageType == "local-agent") {
			$prompt = $projectPrompt + "\n\n" + $emphasize;
		} else {
			$prompt = $basePrompt + "\n\n" + $toplevelRules + "\n\n" + $projectPrompt + "\n\n" + $emphasize;
		};
		set_system_prompt($prompt);
		set_project_prompt($projectPrompt);
		set_emphasize($emphasize);
		set_plan($plan);
		set_context(read_file(combine_path(@ProjectDirectory, "docs/context.txt")));
		set_todo(read_file(combine_path(@ProjectDirectory, "docs/todo.txt")));
		set_history(read_file(combine_path(@ProjectDirectory, "docs/history.txt")));

		send_command_to_inject("update_system_prompt", to_json({prompt: $prompt}));

		if (@EnableLlmPM) {
			$note = "你作为PM，要特别注意，一切以对话事实信息为准，不要猜测，缺少信息的保持现状不修改";
			$llm_sys_prompt = format("{0}\n\n{1}", $note, $projectPrompt);
			llm_set_system_prompt(@LlmProviderId, "llm_pm_plan", $llm_sys_prompt);
			llm_set_system_prompt(@LlmProviderId, "llm_pm_align", $llm_sys_prompt);
			llm_set_system_prompt(@LlmProviderId, "llm_pm_context", $llm_sys_prompt);
			llm_set_system_prompt(@LlmProviderId, "llm_pm_project", $llm_sys_prompt);
		};

		induction_plan();
	}
	elif ($type == "llm_context_count_down") {
		nativelog("[dsl] LLM context count down notification received");

		nativelog("[llm] delete {0}/console.log", appdir);
		delete_file(combine_path(appdir, "console.log"));

		// Read context file
		$contextFile = combine_path(@ProjectDirectory, "docs/context.txt");

		$data = get_message_param($notif, "data");
		$pageType = get_message_param($data, "pageType");
		$queuedCount = get_message_param($data, "queuedCount");
		$count = get_message_param($data, "count");

		nativelog("[dsl] llm_context_count_down pageType: {0}, queuedCount: {1}, count: {2}", $pageType, $queuedCount, $count);

		induction_context($count, $queuedCount, $pageType);
	}
	elif ($type == "llm_align_target") {
		nativelog("[dsl] LLM align target notification received");

		nativelog("[llm] delete {0}/console.log", appdir);
		delete_file(combine_path(appdir, "console.log"));

		$data = get_message_param($notif, "data");
		$pageType = get_message_param($data, "pageType");
		$queuedCount = get_message_param($data, "queuedCount");
		$count = get_message_param($data, "count");

		nativelog("[dsl] llm_align_target pageType: {0}, queuedCount: {1}, count: {2}", $pageType, $queuedCount, $count);

		induction_todo($count, $queuedCount, $pageType);
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

		if (add_cur_context_rounds() == 0) {
			$prompt = format("【最近会话】:{0}\n\n【todo】:{1}\n\n【上下文信息】:{2}", get_history(), get_todo(), get_context());
			send_command_to_inject("send_message", to_json({text: $prompt}));
		};
	}
	elif ($type == "agent_need_to_plan") {
		nativelog("[dsl] agent_need_to_plan notification received");

		nativelog("[llm] delete {0}/console.log", appdir);
		delete_file(combine_path(appdir, "console.log"));

		$data = get_message_param($notif, "data");
		$lastFromLLM = get_message_param($data, "lastFromLLM");
		$lastScannedMessage = get_message_param($data, "lastScannedMessage");
		$isLastResponse = get_message_param($data, "isLastResponse");
		$queuedCount = get_message_param($data, "queuedCount");
		$pageType = get_message_param($data, "pageType");
		$count = get_message_param($data, "count");
		$planPath = combine_path(@ProjectDirectory, "docs/plan.txt");

		nativelog("[dsl] agent_need_to_plan notification: {0}({1}) queued:{2} page:{3} count:{4} {5}", $lastFromLLM, gettypename($lastFromLLM), $queuedCount, $pageType, $count, getstringinlength($lastScannedMessage,100));

		if ($isLastResponse != true) {
			nativelog("[dsl] skip the planning, isLastResponse={0} lastScannedMessage={1}", $isLastResponse, $lastScannedMessage);
			return(true);
		}
		elif ($queuedCount > 0) {
			send_command_to_inject("send_message", to_json({text: format("还有{0}个代码要执行，不要再发新代码，回复继续即可", $queuedCount)}));
			return(true);
		}
		elif ($lastFromLLM == "True" || $lastFromLLM == true) {
			nativelog("[dsl] lastFromLLM=true, queuedCount={0}", $queuedCount);

			if (string_contains($lastScannedMessage, "需要", "继续", "吗") && string_length($lastScannedMessage) > 16) {
				nativelog("[dsl] Sending '继续' to LLM");

				send_command_to_inject("send_message", to_json({text: "继续"}));
				return(true);
			}
			elif (string_contains($lastScannedMessage, "确定吗") || string_contains($lastScannedMessage, "确认吗")) {
				nativelog("[dsl] Sending '确定' to LLM");

				send_command_to_inject("send_message", to_json({text: "确定"}));
				return(true);
			}
			elif (string_contains($lastScannedMessage, "Error", "Occur")) {
				nativelog("[dsl] Sending '继续' to LLM");

				send_command_to_inject("send_message", to_json({text: "继续"}));
				return(true);
			}
			elif (string_contains_any($lastScannedMessage, "继续", "等待") && string_length($lastScannedMessage) <= 32) {
				nativelog("[dsl] Sending '没有代码要执行了' to LLM");

				send_command_to_inject("send_message", to_json({text: "没有代码要执行了"}));
				return(true);
			}
			elif (string_contains($lastScannedMessage, "启动Agent") && string_length($lastScannedMessage) <= 32) {
				nativelog("[dsl] Sending '启动Agent' to inject");

				send_command_to_inject("start_agent", to_json({}));
				return(true);
			}
			elif (string_contains($lastScannedMessage, "停止Agent") && string_length($lastScannedMessage) <= 32) {
				nativelog("[dsl] Sending '停止Agent' to inject");

				send_command_to_inject("stop_agent", to_json({}));
				return(true);
			}
			elif (string_contains($lastScannedMessage, "//", "@execute") || string_contains($lastScannedMessage, "MetaDSL", "{:", ":}")  || string_contains($lastScannedMessage, "MetaDsl", "{:", ":}") || string_contains($lastScannedMessage, "metadsl", "{:", ":}")) {
				if (string_contains($lastScannedMessage, "js_request", "keep_llm_context")) {
					if (@EnableLlmPM) {
						nativelog("[dsl] Sending '稍后请阅读context.txt与history.txt了解上下文' to LLM");

						$prompt = format("ref{{:\n{0}\n:}};\n\n已提交上下文更新请求，稍后请阅读context.txt与history.txt了解上下文", $lastScannedMessage);
						send_command_to_inject("send_message", to_json({text: $prompt}));
					};
					return(true);
				}
				else {
					nativelog("[dsl] Sending 'metadsl代码要使用markdown代码块语法' to LLM");

					$prompt = format("ref{{:\n{0}\n:}};\n\nmetadsl代码要使用markdown代码块语法", $lastScannedMessage);
					send_command_to_inject("send_message", to_json({text: $prompt}));
					return(true);
				};
			}
			elif (file_exists($planPath)) {
				nativelog("[dsl] lastFromLLM=true, queuedCount={0} induction_plan", $queuedCount);
				induction_plan();
			}
			else {
				nativelog("[dsl] lastFromLLM=true, queuedCount={0} enter else branch", $queuedCount);
			};
		}
		else {
			nativelog("[dsl] lastFromLLM=false, queuedCount={0}", $queuedCount);

			if (string_contains($lastScannedMessage, "MetaDSL", "hot_reload")) {
				nativelog("[dsl] Sending '热更完成，继续' to LLM");

				send_command_to_inject("send_message", to_json({text: "热更完成，继续"}));
				return(true);
			}
			elif (file_exists($planPath)) {
				nativelog("[dsl] lastFromLLM=false, queuedCount={0} induction_plan", $queuedCount);
				induction_plan();
			}
			else {
				nativelog("[dsl] lastFromLLM=false, queuedCount={0} enter else branch", $queuedCount);
			};
		};
	}
	else {
		nativelog("[dsl] Unknown notification type: {0}", $type);
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
