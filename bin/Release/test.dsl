script(main)
{
	timestat(true);
    $ct = 0;
    loop(10000000){
        $ct = $ct + 1;
    };
    echo($ct);
};
