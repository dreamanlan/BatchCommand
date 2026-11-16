script(main)
{
	curdir = get_script_directory();
	cd(curdir);
	file_echo(true);

	copy_files("bin/Debug/net9.0", "../../WebAgent/webagent/managed","AgentCore.*");
	// Lucene codecs (separate nuget package since 4.8-beta) and the WinRT
	// projections TextCopy probes at runtime (deps references).
	copy_files("bin/Debug/net9.0", "../../WebAgent/webagent/managed","Lucene.Net.Codecs.*");
	copy_files("bin/Debug/net9.0", "../../WebAgent/webagent/managed","Microsoft.Windows.SDK.NET.*");
	copy_files("bin/Debug/net9.0", "../../WebAgent/webagent/managed","WinRT.Runtime.*");
	// Self-signed https cert setup script for the webserver (standalone, run manually or via launch_process_with_admin).
	copy_files(".", "../../WebAgent/webagent", "ssl_selfsign_setup.ps1");

	if (arg_num() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};
