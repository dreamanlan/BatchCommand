script(main)
{
	curdir = get_script_directory();
	cd(curdir);
	file_echo(true);

	copy_files("bin/Debug/net9.0", "../../../WebAgent/webagent/managed","BatchCmdDsl.*");

	if (arg_num() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};
