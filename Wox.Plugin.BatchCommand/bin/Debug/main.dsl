//注意:Result的Title与SubTitle相同时Wox认为是相同的结果（亦即，不会更新Action等其他字段！
//在Action处理里，BatchCommand插件尝试根据Title与SubTitle来定位正确的Result，所以结果列表
//里不要出现Title与SubTitle都相同的项）
script(main)args($id, $metadata, $context)
{
    clearkeywords($id);
    addkeyword($id, "dsl");
    addkeyword($id, "unity");
    addkeyword($id, "ue");
    addkeyword($id, "vscode");
    @phase = 0;
    @unity = "";
    @ue = "";
    @vscode = "";
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
    }elseif($key=="unity"){
        everythingreset();
        everythingsetdefault();
        if(@phase==0){
            $list = everythingsearch("\\unity.exe");
            looplist($list){
                addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_unity", $query);
            };
        }elseif(@phase==1){
            $list = everythingsearch($query.FirstSearch);
            looplist($list){
                addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_unity_proj", $query);
            };
        };
    }elseif($key=="ue"){
        everythingreset();
        everythingsetdefault();
        if(@phase==0){
            $list = everythingsearch("\\uedit64.exe");
            looplist($list){
                addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_ue", $query);
            };
        }elseif(@phase==1){
            $list = everythingsearch($query.FirstSearch);
            looplist($list){
                addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_ue_proj", $query);
            };
        };
    }elseif($key=="vscode"){
        everythingreset();
        everythingsetdefault();
        if(@phase==0){
            $list = everythingsearch("\\code.exe");
            looplist($list){
                addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_vscode", $query);
            };
        }elseif(@phase==1){
            $list = everythingsearch($query.FirstSearch);
            looplist($list){
                addresult($$[0], ""+$$[1]+" "+$$[2], "", "on_action_vscode_proj", $query);
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

script(on_action_unity)args($query, $result, $actionContext)
{
    @unity = $result.Title;
    @phase = 1;
    api().ChangeQuery("unity \\Assets", true);
    return(0);  
};
script(on_action_unity_proj)args($query, $result, $actionContext)
{
    $path = $result.Title;
    if($path.EndsWith("\\Assets")){
        $path = getdirectoryname($path);
    };
    process(@unity, "-projectPath "+$path){
        nowait(true);
    };
    @phase = 0;
    return(1);
};

script(on_action_ue)args($query, $result, $actionContext)
{
    @ue = $result.Title;
    @phase = 1;
    api().ChangeQuery("ue *.dsl", true);
    return(0);  
};
script(on_action_ue_proj)args($query, $result, $actionContext)
{
    $path = $result.Title;
    process(@ue, $path){
        nowait(true);
    };
    @phase = 0;
    return(1);
};

script(on_action_vscode)args($query, $result, $actionContext)
{
    @vscode = $result.Title;
    @phase = 1;
    api().ChangeQuery("vscode *.dsl", true);
    return(0);  
};
script(on_action_vscode_proj)args($query, $result, $actionContext)
{
    $path = $result.Title;    
    process(@vscode, $path){
        nowait(true);
    };
    @phase = 0;
    return(1);
};