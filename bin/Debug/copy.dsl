script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	copyfile("BatchCommand.exe", "c:/Code/Client/Tools/BatchCommand.exe");
	copyfile("BatchCommand.exe", "c:/Code/Product/Tools/BatchCommand.exe");
	copyfile("BatchCommand.exe", "c:/Code/Publish/BatchCommand.exe");
	
	return(0);
};