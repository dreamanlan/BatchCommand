script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	fileecho(true);

	copyfiles("bin/Debug/net9.0", "../../WebAgent/webagent/managed","BatchScriptApi.*");

	if (argnum() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};