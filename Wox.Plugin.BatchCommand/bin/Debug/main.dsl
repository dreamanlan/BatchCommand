//注意:Result的Title与SubTitle相同时Wox认为是相同的结果（亦即，不会更新Action等其他字段！
//在Action处理里，BatchCommand插件尝试根据Title与SubTitle来定位正确的Result，所以结果列表
//里不要出现Title与SubTitle都相同的项）
script(main)args($id, $metadata, $context)
{
    clearkeywords($id);
    addkeyword($id, "dsl");
    addkeyword($id, "cmd");
    addkeyword($id, "open");
    addkeyword($id, "unity");
    addkeyword($id, "ue");
    addkeyword($id, "vscode");
    @curkey = "";
    @phase = 0;
    @exe = "";
    @cfg = {
        "unity" => ["\\unity.exe", "unity \\Assets", "{0}", "-projectPath {0}"],
        "ue" => ["\\uedit64.exe", "ue *.dsl", "{0}", "{0}"],
        "vscode" => ["\\code.exe", "vscode *.dsl", "{0}", "{0}"],
    };
    return(0);
};
script(on_query)args($query)
{
    $key = $query.ActionKeyword;
    if($key=="dsl"){
        $param = $query.FirstSearch;
        if($param=="reload"){
            addresult("reload", "reload main.dsl.", "", "on_action_reload_dsl", $query);
        }elseif($param=="eval"){
            addresult("eval", "evaluate dsl code.", "", "on_action_eval_dsl", $query);
        }else{
            addresult("reload", "reload main.dsl", "", "on_action_change", $query);
            addresult("eval", "evaluate dsl code", "", "on_action_change", $query);
        };
    }elseif($key=="cmd"){
        everythingreset();
        everythingsetdefault();
        $list = everythingsearch($query.FirstSearch);
        looplist($list){
            addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_cmd", $query);
        };
    }elseif($key=="open"){
        if(@curkey!=$key){
            @curkey = $key;
            @phase = 0;
            @exe = "";
        };
        
        everythingreset();
        everythingsetdefault();
        if(@phase==0){
            $list = everythingsearch($query.FirstSearch);
            looplist($list){
                addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_open", $query);
            };
        }elseif(@phase==1){
            $list = everythingsearch($query.FirstSearch);
            looplist($list){
                addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_open_proj", $query);
            };
        };
    }else{
        if(@curkey!=$key){
            @curkey = $key;
            @phase = 0;
            @exe = "";
        };
        
        everythingreset();
        everythingsetdefault();
        if(@phase==0){
            $cfglist = @cfg[$key];
            $list = everythingsearch($cfglist[0]);
            looplist($list){
                addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_app", $query);
            };
        }elseif(@phase==1){
            $list = everythingsearch($query.FirstSearch);
            looplist($list){
                addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_app_proj", $query);
            };
        };
    };
    return(0);
};
script(on_context_menus)args($query, $result)
{
    addcontextmenu("Test Menu", "this is a test menu", "", "on_menu_action_test", $query, $result);
    return(0);
};
script(on_menu_action_test)args($query, $result, $menu, $actionContext)
{
    api().ShowMsg($menu.Title, $menu.SubTitle, $menu.IcoPath);
    return(1);
};

script(on_action_change)args($query, $result, $actionContext)
{
    api().ChangeQuery("dsl "+$result.Title, false);
    return(0);
};
script(on_action_reload_dsl)args($query, $result, $actionContext)
{
    reloaddsl();
    return(0);
};
script(on_action_eval_dsl)args($query, $result, $actionContext)
{
    evaldsl($query.SecondToEndSearch, $query, $result, $actionContext);
    return(0);
};

script(on_action_cmd)args($query, $result, $actionContext)
{
    cmdpath = $result.Title;
    process("cmd", "/c start /d %cmdpath% /b cmd"){
        nowait(true);
        useshellexecute(true);
        verb("open");
    };
    return(1);  
};

script(on_action_open)args($query, $result, $actionContext)
{
    @exe = $result.Title;
    @phase = 1;
    api().ChangeQuery("open *", true);
    return(0);  
};
script(on_action_open_proj)args($query, $result, $actionContext)
{
    $path = $result.Title;
    process(@exe, $path){
        nowait(true);
    };
    @phase = 0;
    return(1); 
};

script(on_action_app)args($query, $result, $actionContext)
{
    $key = $query.ActionKeyword;
    @exe = $result.Title;
    @phase = 1;
    api().ChangeQuery(@cfg[$key][1], true);
    return(0);  
};
script(on_action_app_proj)args($query, $result, $actionContext)
{
    $key = $query.ActionKeyword;
    $path = $result.Title;    
    //unity工程可以借助子目录名来筛选
    if($key=="unity" && $path.EndsWith("\\Assets")){
        $path = getdirectoryname($path);
    };
    process(format(@cfg[$key][2],@exe), format(@cfg[$key][3],$path)){
        nowait(true);
    };
    @phase = 0;
    return(1);
};