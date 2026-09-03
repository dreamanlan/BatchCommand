script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	fileecho(true);

	copyfiles("bin/Debug/net8.0", "../../WebAgent/cefclient/managed","CefDotnetApp.*");
	copyfiles("bin/Debug/net8.0", "../../WebAgent/cefclient/managed","TextCopy.dll");
	copyfiles("bin/Debug/net8.0", "../../WebAgent/cefclient/managed","Microsoft.Extensions.DependencyInjection.Abstractions.dll");

	if (argnum() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};