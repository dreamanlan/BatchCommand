script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	fileecho(true);

	echo("curdir: " + curdir);

	if (stringcontainsany(stringtolower(curdir), "e:/github/", "e:\\github\\") && direxist("d:/GitHub")) {
		foreach("GameDemo", "CSharpGameFramework", "apkstudio/DotnetApp/bin/Debug/net8.0", "myuzu/tools/dbg_scp_compiler", "Hlsl2Python/gencode", "Hlsl2Numpy/gencode") {
			setenv("CopyTargetDir", $$);
			copyfiles(".", "d:/GitHub/%CopyTargetDir%", "BatchCommand.*");
			copyfiles(".", "d:/GitHub/%CopyTargetDir%", "Common.*");
			copyfiles(".", "d:/GitHub/%CopyTargetDir%", "DotnetStoryScript.*");
			copyfiles(".", "d:/GitHub/%CopyTargetDir%", "dsl.*");

			copyfile("TextCopy.dll", "d:/GitHub/%CopyTargetDir%/TextCopy.dll");
			copyfile("LitJson.dll", "d:/GitHub/%CopyTargetDir%/LitJson.dll");
			copyfile("ScriptFrameworkLibrary.dll", "d:/GitHub/%CopyTargetDir%/ScriptFrameworkLibrary.dll");
			copyfile("Microsoft.Extensions.DependencyInjection.Abstractions.dll", "d:/GitHub/%CopyTargetDir%/Microsoft.Extensions.DependencyInjection.Abstractions.dll");
		};
	}
	else{
		foreach("GameDemo", "CSharpGameFramework", "apkstudio/DotnetApp/bin/Debug/net8.0", "myuzu/tools/dbg_scp_compiler", "Hlsl2Python/gencode", "Hlsl2Numpy/gencode") {
			setenv("CopyTargetDir", $$);
			copyfiles(".", "../../../../%CopyTargetDir%", "BatchCommand.*");
			copyfiles(".", "../../../../%CopyTargetDir%", "Common.*");
			copyfiles(".", "../../../../%CopyTargetDir%", "DotnetStoryScript.*");
			copyfiles(".", "../../../../%CopyTargetDir%", "dsl.*");

			copyfile("TextCopy.dll", "../../../../%CopyTargetDir%/TextCopy.dll");
			copyfile("LitJson.dll", "../../../../%CopyTargetDir%/LitJson.dll");
			copyfile("ScriptFrameworkLibrary.dll", "../../../../%CopyTargetDir%/ScriptFrameworkLibrary.dll");
			copyfile("Microsoft.Extensions.DependencyInjection.Abstractions.dll", "../../../../%CopyTargetDir%/Microsoft.Extensions.DependencyInjection.Abstractions.dll");
		};
	};

	if (argnum() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};