script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	fileecho(true);

	copyfiles("bin/Debug/net8.0", "../../CEF_Src_Build/cefclient/managed","AgentCore.*");

	if (argnum() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};