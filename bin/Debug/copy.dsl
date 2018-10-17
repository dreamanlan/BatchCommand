script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	copyfile("BatchCommand.exe", "d:/work/Client/BatchCommand.exe");
	copyfile("BatchCommand.exe", "d:/work/Publish/BatchCommand.exe");
	
	return(0);
};