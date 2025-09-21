script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	fileecho(true);
	
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
	
	if (argnum() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};