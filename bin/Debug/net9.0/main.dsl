script(main)
{
	file_echo(true);
	clear();
	echo("{0} {1} {2} {3} {4}", get_title(), get_buffer_width(), get_buffer_height(), get_bg_color(), get_fg_color());
	set_title("测试c#批处理工具");
	$encoding = get_input_encoding();
	set_encoding("gb2312");

	echo("os:{0}",os());
	echo("osplatform:{0}",osplatform());
	echo("osversion:{0}",osversion());

	echo("2-1={0}",(2-1).ToString());
	echo("2-1={0}",(2-1).m_value);
	$a = (2-1).ToString();
	$b = (2-1).m_value;
	echo("2-1={0},{1}",$a,$b);

	@g1=123;
	@g2=456;
	echo("global:{0}", call("calc1"));

	$l1=123;
	$l2=456;
	echo("local:{0}", call("calc2"));
	echo("local:{0}", call("calc3",$l1,$l2));

	process("cmd","/c dir"){
		output("$txt");
	};
	echo("dir result:{0}", $txt);

	loop_list(plist("")){
		echo("{0} {1}", $$.Id, $$.ProcessName);
	};

	echo("pid:{0}", pid());
	echo("kill:{0}", kill("BatchCommand"));

	set_fg_color("DarkBlue");
	echo("read_line:");
	$var1 = read_line();
	echo("read:");
	$var2 = read();
	echo();
	$var3 = make_string($var2,$var2,$var2);
	echo("make_string:{0}", $var3);
	echo("press any key ...");
	process("cmd", "/c dir"){
		nowait(true);
		createwindow(true);
		windowstyle("Normal");
	};
	waitall();
	echo("press any key ...");
	command{
		unix{:cat copy.dsl:};
	}command{
		unix{:grep command:};
	};
	pause();
	reset_color();
	echo("kill me:{0}",pid());
	killme();
	echo("you should not see this.");
};

script(calc1)
{
	return(@g1*@g2);
};

script(calc2)
{
	$l1=1;
	$l2=2;
	return($l1*$l2);
};

script(calc3)
{
	return(arg(0)*arg(1));
};