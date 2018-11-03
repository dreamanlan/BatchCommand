script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	copyfile("BatchCommand.exe", "d:/work/Client/Tools/BatchCommand.exe");
	copyfile("BatchCommand.exe", "d:/work/Product/Tools/BatchCommand.exe");
	copyfile("BatchCommand.exe", "d:/work/Publish/BatchCommand.exe");
	
	return(0);
};