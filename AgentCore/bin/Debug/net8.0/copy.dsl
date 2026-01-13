script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	fileecho(true);

	copyfiles(".", "../../../../../CEF_Src_Build/cefclient/managed","AgentCore.*");
	copyfiles(".", "../../../../../CEF_Src_Build/cefclient/managed","LibGit2Sharp.dll");
	copyfiles(".", "../../../../../CEF_Src_Build/cefclient/managed","runtimes","*");

	if (argnum() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};