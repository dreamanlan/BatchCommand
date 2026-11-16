script(main)
{
	curdir = get_script_directory();
	cd(curdir);
	file_echo(true);

	copy_files("bin/Debug/net9.0", "../managed","DotnetApp.*");
	copy_files("bin/Debug/net9.0", "../managed","BatchScriptApi.*");
	copy_files("bin/Debug/net9.0", "../managed","Common.*");
	copy_files("bin/Debug/net9.0", "../managed","DotnetStoryScript.*");
	copy_files("bin/Debug/net9.0", "../managed","Dsl.*");
	copy_files("bin/Debug/net9.0", "../managed","LitJson.*");
	copy_files("bin/Debug/net9.0", "../managed","ScriptFrameworkLibrary.*");

	if (arg_num() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};