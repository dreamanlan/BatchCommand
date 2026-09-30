script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	fileecho(true);

	copyfiles("bin/Debug/net9.0", "../managed","DotnetApp.*");
	copyfiles("bin/Debug/net9.0", "../managed","Common.*");
	copyfiles("bin/Debug/net9.0", "../managed","DotnetStoryScript.*");
	copyfiles("bin/Debug/net9.0", "../managed","Dsl.*");
	copyfiles("bin/Debug/net9.0", "../managed","LitJson.*");
	copyfiles("bin/Debug/net9.0", "../managed","ScriptFrameworkLibrary.*");

	if (argnum() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};