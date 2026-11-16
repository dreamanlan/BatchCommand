script(main)
{
	curdir = get_script_directory();
	cd(curdir);
	file_echo(true);

	copy_files(".", "../../x64/Debug/Managed","MyApp.*");

	if (arg_num() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};
