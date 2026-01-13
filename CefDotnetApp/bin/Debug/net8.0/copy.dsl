script(main)
{
	curdir = getscriptdir();
	cd(curdir);
	fileecho(true);

	copyfiles(".", "../../../../../CEF_Src_Build/cefclient/managed","CefDotnetApp.*");
	copyfiles(".", "../../../../../CEF_Src_Build/cefclient/managed","TextCopy.dll");
	copyfiles(".", "../../../../../CEF_Src_Build/cefclient/managed","Microsoft.Extensions.DependencyInjection.Abstractions.dll");

	if (argnum() <= 1) {
		echo("press any key ...");
		read();
	};
	return(0);
};