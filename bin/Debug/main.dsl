script(main)
{
	clear();
	echo("{0} {1} {2} {3} {4}", gettitle(), getbufferwidth(), getbufferheight(), getbgcolor(), getfgcolor());
	settitle("测试c#批处理工具");

	echo("os:{0}",os());
	echo("osplatform:{0}",osplatform());
	echo("osversion:{0}",osversion());
	
	setfgcolor("DarkBlue");
	echo("readline:");
	var(1) = readline();
	echo("read:");
	var(2) = read();
	echo();
	var(3) = makestring(var(2),var(2),var(2));
	echo("makestring:{0}", var(3));
	echo("press any key ...");
	process("cmd", "/c dir"){
		nowait(true);
		newwindow(true);
		windowstyle("Normal");		
	};
	echo("press any key ...");
	command{
		unix{:cat copy.dsl:};
	}command{
		unix{:grep command:};
	};
	pause();
	resetcolor();
};