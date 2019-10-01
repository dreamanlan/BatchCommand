script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	copyfile("BatchCommand.exe", "c:/Code/Client/Tools/BatchCommand.exe");
	copyfile("BatchCommand.exe", "c:/Code/Product/Tools/BatchCommand.exe");
	copyfile("BatchCommand.exe", "c:/Code/Publish/BatchCommand.exe");
	
	copyfile("BatchCommand.exe", "d:/Code/Client/Tools/BatchCommand.exe");
	copyfile("BatchCommand.exe", "d:/Code/Product/Tools/BatchCommand.exe");
	copyfile("BatchCommand.exe", "d:/Code/Publish/BatchCommand.exe");
	
	return(0);
};