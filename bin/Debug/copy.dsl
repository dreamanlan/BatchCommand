script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	copyfile("BatchCommand.exe", "c:/data/workspace/Client/Tools/BatchCommand.exe");
	copyfile("BatchCommand.exe", "c:/data/workspace/Product/Tools/BatchCommand.exe");
	copyfile("BatchCommand.exe", "c:/data/workspace/Publish/BatchCommand.exe");
	
	return(0);
};