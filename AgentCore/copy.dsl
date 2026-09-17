script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	fileecho(true);

	copyfiles("bin/Debug/net9.0", "../../WebAgent/webagent/managed","AgentCore.*");
	copyfiles("bin/Debug/net9.0", "../../WebAgent/webagent/managed","BatchScriptApi.*");
	// Lucene codecs (separate nuget package since 4.8-beta) and the WinRT
	// projections TextCopy probes at runtime (deps references).
	copyfiles("bin/Debug/net9.0", "../../WebAgent/webagent/managed","Lucene.Net.Codecs.*");
	copyfiles("bin/Debug/net9.0", "../../WebAgent/webagent/managed","Microsoft.Windows.SDK.NET.*");
	copyfiles("bin/Debug/net9.0", "../../WebAgent/webagent/managed","WinRT.Runtime.*");
	// Self-signed https cert setup script for the webserver (standalone, run manually or via launch_process_with_admin).
	copyfiles(".", "../../WebAgent/webagent", "ssl_selfsign_setup.ps1");

	if (argnum() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};