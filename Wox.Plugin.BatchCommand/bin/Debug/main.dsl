//注意:Result的Title与SubTitle相同时Wox认为是相同的结果（亦即，不会更新Action等其他字段！
//在Action处理里，BatchCommand插件尝试根据Title与SubTitle来定位正确的Result，所以结果列表
//里不要出现Title与SubTitle都相同的项）
//ActionKeyword注册为*时，Query.ActionKeyword为空串，输入的第一个部分是Query.FirstSearch，这与非*的ActionKeyword注册不同。
//这里还是采用具体的ActionKeyword注册。
script(main)args($id, $metadata, $context)
{
    clearkeywords($id);
    addkeyword($id, "dsl");
    addkeyword($id, "start");
    addkeyword($id, "menu");
    addkeyword($id, "file");
    addkeyword($id, "open");
    addkeyword($id, "unity");
    addkeyword($id, "ue");
    addkeyword($id, "vscode");
    
    @curkey = "";
    @phase = 0;
    @exe = "";
    @cmdpath = "";
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
    if(@curkey!=$key){
        @curkey = $key;
        @phase = 0;
        @exe = "";
        @cmdpath = "";
    };
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
    }elseif($key=="menu"){        
        everythingreset();
        everythingsetdefault();
        $list = everythingsearch($query.FirstSearch);
        looplist($list){
            addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_menu", $query);
        };
    }elseif($key=="file"){        
        everythingreset();
        everythingsetdefault();
        $list = everythingsearch($query.FirstSearch);
        looplist($list){
            addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_file", $query);
        };
    }elseif($key=="start"){        
        everythingreset();
        everythingsetdefault();
        if(@phase==0){
            $list = everythingsearch($query.FirstSearch);
            looplist($list){
                addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_cmd", $query);
            };
        }elseif(@phase==1){
            $list = everythingsearch($query.SecondSearch);
            looplist($list){
                addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_cmd_args", $query);
            };
        };
    }elseif($key=="open"){
        everythingreset();
        everythingsetdefault();
        if(@phase==0){
            $list = everythingsearch($query.FirstSearch);
            looplist($list){
                addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_open", $query);
            };
        }elseif(@phase==1){
            $list = everythingsearch($query.SecondSearch);
            looplist($list){
                addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_open_proj", $query);
            };
        };
    }elseif(!isnull($key)){
        if(!isnull(hashtableget(@cfg, $key))){
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
    };
    return(0);
};
script(on_context_menus)args($query, $result)
{
    addcontextmenu("Show Explore Menu", "show explore menu", "", "on_menu_action_explore_menu", $query, $result);
    return(0);
};
script(on_menu_action_explore_menu)args($query, $result, $menu, $actionContext)
{
    $path = $result.Title;
    $ctrl = $actionContext.SpecialKeyState.CtrlPressed;
    $shift = $actionContext.SpecialKeyState.ShiftPressed;
    showcontextmenu($path, $ctrl, $shift);
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

script(on_action_menu)args($query, $result, $actionContext)
{
    $path = $result.Title;
    $ctrl = $actionContext.SpecialKeyState.CtrlPressed;
    $shift = $actionContext.SpecialKeyState.ShiftPressed;
    showcontextmenu($path, $ctrl, $shift);
    return(0);  
};

script(on_action_file)args($query, $result, $actionContext)
{
    $path = $result.Title;
    $args = $query.SecondToEndSearch.Substring($query.SecondSearch.Length);
    process($path, $args){
        nowait(true);
        useshellexecute(true);
        verb("open");
    };
    return(1);  
};

script(on_action_cmd)args($query, $result, $actionContext)
{
    @cmdpath = $result.Title;
    @phase = 1;
    return(0);  
};
script(on_action_cmd_args)args($query, $result, $actionContext)
{
    cmdpath = @cmdpath;
    $path = $result.Title;
    $args = $query.SecondToEndSearch.Substring($query.SecondSearch.Length);
    process("cmd", "/c start /d %cmdpath% /b "+$path+$args){
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
    return(0);  
};
script(on_action_open_proj)args($query, $result, $actionContext)
{
    $path = $result.Title;
    $args = $query.SecondToEndSearch.Substring($query.SecondSearch.Length);
    process(@exe, $path+$args){
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
    $args = $query.SecondToEndSearch;
    $path = $result.Title;    
    //unity工程可以借助子目录名来筛选
    if($key=="unity" && $path.EndsWith("\\Assets")){
        $path = getdirectoryname($path);
    };
    process(format(@cfg[$key][2],@exe), format(@cfg[$key][3],$path)+" "+$args){
        nowait(true);
    };
    @phase = 0;
    return(1);
};