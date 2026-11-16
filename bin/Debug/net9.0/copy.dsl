script(main)
{
	curdir = get_script_directory();
	cd(curdir);
	file_echo(true);

	echo("curdir: " + curdir);

	for_each_value("GameDemo", "CSharpGameFramework", "apkstudio/DotnetApp/copytool", "myuzu/tools/dbg_scp_compiler", "Hlsl2Python/gencode", "Hlsl2Numpy/gencode") {
		copy_files(".", "../../../../" + $$, "BatchCommand.*");
		copy_files(".", "../../../../" + $$, "BatchScriptApi.*");
		copy_files(".", "../../../../" + $$, "Common.*");
		copy_files(".", "../../../../" + $$, "DotnetStoryScript.*");
		copy_files(".", "../../../../" + $$, "dsl.*");

		copy_file("TextCopy.dll", "../../../../" + $$ + "/TextCopy.dll");
		copy_file("LitJson.dll", "../../../../" + $$ + "/LitJson.dll");
		copy_file("ScriptFrameworkLibrary.dll", "../../../../" + $$ + "/ScriptFrameworkLibrary.dll");
		copy_file("Microsoft.Extensions.DependencyInjection.Abstractions.dll", "../../../../" + $$ + "/Microsoft.Extensions.DependencyInjection.Abstractions.dll");
	};

	// DebugScript VM (only used by the myuzu debug-script compiler toolchain)
	copy_files(".", "../../../../myuzu/tools/dbg_scp_compiler", "DebugScriptVM.*");

	if (arg_num() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};
