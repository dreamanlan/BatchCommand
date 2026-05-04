// Note:The design philosophy behind these DSL scripts is to be stateless;
// all state resides within C# or JS. The DSL's global variables are utilized
// for configuring constants, and each hot-reload operation executes independently.
script(init_global_consts)
{
	// Initialize global constants for browser here
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
};
script(on_browser_finalize)
{
    nativelog("[dsl] on_browser_finalize finish");
};

script(on_heart_beat)params($processType,$deltaTime)
{
	// Do something every heart beat
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
script(on_render_process_terminated)params($startupUrl,$url,$status,$errorCode,$errorString)
{
	nativelog("[dsl] on_render_process_terminated: startup_url={0}, url={1}, status={2}, error_code={3}, error_string={4}", $startupUrl, $url, $status, $errorCode, $errorString);
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
