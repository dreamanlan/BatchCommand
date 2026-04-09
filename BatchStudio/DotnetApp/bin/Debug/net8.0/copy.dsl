script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	fileecho(true);
	
	copyfiles(".", "../../../../managed","DotnetApp.*");
	copyfiles(".", "../../../../managed","Common.*");
	copyfiles(".", "../../../../managed","DotnetStoryScript.*");
	copyfiles(".", "../../../../managed","Dsl.*");
	copyfiles(".", "../../../../managed","LitJson.*");
	copyfiles(".", "../../../../managed","ScriptFrameworkLibrary.*");
	
	if (argnum() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};