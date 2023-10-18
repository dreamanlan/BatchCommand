//注意:Result的Title与SubTitle相同时Wox认为是相同的结果（亦即，不会更新Action等其他字段！
//在Action处理里，BatchCommand插件尝试根据Title与SubTitle来定位正确的Result，所以结果列表
//里不要出现Title与SubTitle都相同的项）
//ActionKeyword注册为*时，Query.ActionKeyword为空串，输入的第一个部分是Query.FirstSearch，这与非*的ActionKeyword注册不同。
//这里还是采用具体的ActionKeyword注册。
/*
//脚本api
public class PluginMetadata : BaseModel
{
    public string ID;
    public string Name;
    public string Author;
    public string Version;
    public string Language;
    public string Description;
    public string Website;
    public bool Disabled;
    public bool InitInMainThread;
    public string ExecuteFilePath;
    public string ExecuteFileName;
    public string PluginDirectory;
    public string ActionKeyword;
    public List<string> ActionKeywords;
    public string IcoPath;
    public long InitTime;
    public long AvgQueryTime;
    public int QueryCount;
}
public interface IPublicAPI
{
    event WoxGlobalKeyboardEventHandler GlobalKeyboardEvent;
    void ChangeQuery(string query, bool requery = false);
    void RestarApp();
    void ShowMsg(string title, string subTitle = "", string iconPath = "");
    void OpenSettingDialog();
    void InstallPlugin(string path);
    string GetTranslation(string key);
    List<PluginPair> GetAllPlugins();
}
public class PluginInitContext
{
    public PluginMetadata CurrentPluginMetadata;
    public IPublicAPI API;
}
public class Query
{
    public const string TermSeperater = " ";
    public const string ActionKeywordSeperater = ";";
    public const string GlobalPluginWildcardSign = "*";
    public string RawQuery;
    public string Search;
    public string[] Terms;
    public string ActionKeyword;
    public string FirstSearch => SplitSearch(0);
    public string SecondToEndSearch
    {
        get
        {
            int count = (string.IsNullOrEmpty(ActionKeyword) ? 1 : 2);
            return string.Join(" ", Terms.Skip(count).ToArray());
        }
    }
    public string SecondSearch => SplitSearch(1);
    public string ThirdSearch => SplitSearch(2);

    private string SplitSearch(int index)
    {
        try
        {
            return string.IsNullOrEmpty(ActionKeyword) ? Terms[index] : Terms[index + 1];
        }
        catch (IndexOutOfRangeException)
        {
            return string.Empty;
        }
    }
}
public class Result
{
    public delegate ImageSource IconDelegate();
    public IconDelegate Icon;
    public string Title;
    public string SubTitle;
    public string IcoPath;
    public Func<ActionContext, bool> Action;
    public int Score;
    public string PluginDirectory;
    public object ContextData;
    public string PluginID;
}
public class SpecialKeyState
{
    public bool CtrlPressed;
    public bool ShiftPressed;
    public bool AltPressed;
    public bool WinPressed;
}
public class ActionContext
{
    public SpecialKeyState SpecialKeyState;
}
context() 返回PluginInitContext对象
api() 返回IPublicAPI对象，一般不需要使用，有的有限制，比如RestarApp不能在脚本线程调用
metadata() 返回PluginMetadata对象
showmsg(title[,subtitle[,icon]]) 显示弹出消息,缺少的参数为string.Empty
restart() 重启wox
show() 显示console窗口，evaldsl会自动显示窗口
hide() 隐藏console窗口
reloaddsl() 重新加载main.dsl
evaldsl(dsl代码, query, result, actioncontext) 执行dsl代码，代码里可以使用$query,$result,$actioncontext访问相应参数
changequery(query_string, requery) 修改查询，requery是bool类型
addresult(title, subtitle, icopath, action, query) 添加一条结果到列表，query是查询对象，其它都是字符串，action是dsl里的脚本函数名，
用户点击时执行此脚本，向脚本传3个参数query,result,actioncontext
addcontextmenu(tilte, subtitle, icopath, action, query, result) 添加一个上下文菜单项，query是查询对象，result是结果对象，其它都是字符串
，action是dsl里的脚本函数名，用户点击时执行，向脚本传4个参数query,result,menu,actioncontext, menu也是Result对象，包含菜单项信息
keywordregistered(keyword) 判断指定keyword是否已注册
clearkeywords(id) 清空当前插件所有keyword, id是插件id
addkeyword(id, keyword) 添加一个keyword, id是插件id, keyword是字符串
showcontextmenu(path, ctrl, shift) 显示指定路径path关联的上下文菜单，ctrl与shift指明是否像按下ctrl与shift键一样

tryfindeverything() 如果还没有记录everything全路径，尝试查找，返回everything.exe的全路径
everythingexists() 判断everything是否在运行
everythingreset() 清空everything搜索
everythingsetdefault() 设置everythin默认搜索设置，matchpath false, matchcase false, matchwholdword false, regex false, sort by path asc
everythingmatchpath(bool) 设置matchpath，无参数返回当前设置
everythingmatchcase(bool) 设置matchcase，无参数返回当前设置
everythingmatchwholeword(bool) 设置matchwholeword，无参数返回当前设置
everythingregex(bool) 设置match regex，无参数返回当前设置
everythingsort(type, asc) 设置sort，type可以是path/size/time, asc是bool类型，为true表示升序，无参数返回当前设置
everythingsort(sort) 设置sort, 参数为整数（参见下面的常量），无参数返回当前设置
everythingsearch(key[,offset[,maxcount]]) 执行搜索，key为搜索关键字，offset默认为0，maxcount默认为100，返回一个三个元素的数组：full_path, size, file_date_time
，size是整数，其它元素是字符串

regread(key_name, val_name[, def_val]) 读取注册表值
regwrite(key_name, val_name, val[, val_kind]) 写注册表值
regdelete(key_name[, val_name]) 删除注册表key或val

key_name为路径串，HKey如下：
HKEY_CURRENT_USER
HKEY_LOCAL_MACHINE
HKEY_CLASSES_ROOT
HKEY_USERS
HKEY_PERFORMANCE_DATA
HKEY_CURRENT_CONFIG

val_kind为整数，来自枚举：
public enum RegistryValueKind
{
    String = 1,
    ExpandString = 2,
    Binary = 3,
    DWord = 4,
    MultiString = 7,
    QWord = 11,
    Unknown = 0,
    [ComVisible(false)]
    None = -1
}

public const int EVERYTHING_SORT_NAME_ASCENDING = 1;
public const int EVERYTHING_SORT_NAME_DESCENDING = 2;
public const int EVERYTHING_SORT_PATH_ASCENDING = 3;
public const int EVERYTHING_SORT_PATH_DESCENDING = 4;
public const int EVERYTHING_SORT_SIZE_ASCENDING = 5;
public const int EVERYTHING_SORT_SIZE_DESCENDING = 6;
public const int EVERYTHING_SORT_EXTENSION_ASCENDING = 7;
public const int EVERYTHING_SORT_EXTENSION_DESCENDING = 8;
public const int EVERYTHING_SORT_TYPE_NAME_ASCENDING = 9;
public const int EVERYTHING_SORT_TYPE_NAME_DESCENDING = 10;
public const int EVERYTHING_SORT_DATE_CREATED_ASCENDING = 11;
public const int EVERYTHING_SORT_DATE_CREATED_DESCENDING = 12;
public const int EVERYTHING_SORT_DATE_MODIFIED_ASCENDING = 13;
public const int EVERYTHING_SORT_DATE_MODIFIED_DESCENDING = 14;
public const int EVERYTHING_SORT_ATTRIBUTES_ASCENDING = 15;
public const int EVERYTHING_SORT_ATTRIBUTES_DESCENDING = 16;
public const int EVERYTHING_SORT_FILE_LIST_FILENAME_ASCENDING = 17;
public const int EVERYTHING_SORT_FILE_LIST_FILENAME_DESCENDING = 18;
public const int EVERYTHING_SORT_RUN_COUNT_ASCENDING = 19;
public const int EVERYTHING_SORT_RUN_COUNT_DESCENDING = 20;
public const int EVERYTHING_SORT_DATE_RECENTLY_CHANGED_ASCENDING = 21;
public const int EVERYTHING_SORT_DATE_RECENTLY_CHANGED_DESCENDING = 22;
public const int EVERYTHING_SORT_DATE_ACCESSED_ASCENDING = 23;
public const int EVERYTHING_SORT_DATE_ACCESSED_DESCENDING = 24;
public const int EVERYTHING_SORT_DATE_RUN_ASCENDING = 25;
public const int EVERYTHING_SORT_DATE_RUN_DESCENDING = 26;
*/
//id:插件id，字符串
//metadata:元数据对象，PluginMetadata
//context:上下文对象，PluginInitContext
script(main)args($id, $metadata, $context)
{
    clearkeywords($id);
    addkeyword($id, "dsl");
    addkeyword($id, "menu");
    addkeyword($id, "file");
    addkeyword($id, "cmd");
    addkeyword($id, "exe");
    addkeyword($id, "start");
    addkeyword($id, "unity");
    addkeyword($id, "notepad");
    addkeyword($id, "vscode");
    addkeyword($id, "renderdoc");

    @curkey = "";
    @phase = 0;
    @exe = "";
    @path = "";
    @args = "";
    //配置应用参数：exe查询串，选择exe后的替代查询串（keyword + " " + 新查询），执行的命令格式化串(使用{0}引用第一次选择的结果即exe路径)，
    //执行的命令参数格式化串(使用{0}引用第二次选择的结果，一般是文件路径)
    @cfg = {
        "unity" => ["\\unity.exe", "unity \\Assets", "{0}", "-projectPath {0}"],
        "notepad" => ["\\notepad++.exe", "notepad *.dsl", "{0}", "{0}"],
        "vscode" => ["\\code.exe", "vscode *.dsl", "{0}", "{0}"],
        "renderdoc" => ["\\qrenderdoc.exe", "renderdoc *.exe", "{0}", "{0}"],
    };
    return(0);
};
script(on_query)args($query)
{
    $key = $query.ActionKeyword;
    if(@curkey!=$key && $key!="dsl"){
        @curkey = $key;
        @phase = 0;
        @exe = "";
        @path = "";
        @args = "";
    };
    if($key=="dsl"){
        $param = $query.FirstSearch;
        if($param=="eval"){
            addresult("eval", "evaluate dsl code.", "", "on_action_eval_dsl", $query);
        }elseif($param=="info"){
            addresult("info", "show query state", "", "on_action_info", $query);
        }elseif($param=="clear"){
            addresult("clear", "clear query", "", "on_action_clear", $query);
        }elseif($param=="reload"){
            addresult("reload", "reload main.dsl", "", "on_action_reload", $query);
        }else{
            addresult("eval", "evaluate dsl code", "", "on_action_change", $query);
            addresult("info", "show query state", "", "on_action_change", $query);
            addresult("clear", "clear query", "", "on_action_change", $query);
            addresult("reload", "reload main.dsl", "", "on_action_change", $query);
            //restart直接在c#里处理，dsl部分只处理ui显示
            addresult("restart", "restart Wox", "", "on_action_change", $query);
        };
    }elseif($key=="menu"){
        everythingsetdefault();
        everythingmatchpath(true);
        $list = everythingsearch($query.FirstSearch);
        looplist($list){
            addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_menu", $query);
        };
    }elseif($key=="file"){
        everythingsetdefault();
        $list = everythingsearch($query.FirstSearch);
        looplist($list){
            addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_file", $query);
        };
    }elseif($key=="cmd"){
        everythingsetdefault();
        $list = everythingsearch($query.FirstSearch);
        looplist($list){
            addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_cmd", $query);
        };
    }elseif($key=="exe"){
        everythingsetdefault();
        $list = everythingsearch($query.FirstSearch);
        looplist($list){
            addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_exe", $query);
        };
    }elseif($key=="start"){
        everythingsetdefault();
        if(@phase==0){
            $list = everythingsearch($query.FirstSearch);
            looplist($list){
                addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_start", $query);
            };
        }elseif(@phase==1){
            $list = everythingsearch($query.SecondSearch);
            looplist($list){
                addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_start_proj", $query);
            };
        };
    }elseif(!isnull($key)){
        if(!isnull(hashtableget(@cfg, $key))){
            everythingsetdefault();
            if(@phase == 0){
                $cfglist = @cfg[$key];
                $list = everythingsearch($cfglist[0]);
                looplist($list){
                    addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_app", $query);
                };
            }elseif(@phase == 1){
                $list = everythingsearch($query.FirstSearch);
                looplist($list){
                    addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_app_proj", $query);
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
//$menu也是Result对象
script(on_menu_action_explore_menu)args($query, $result, $menu, $actionContext)
{
    @path = $result.Title;
    $ctrl = $actionContext.SpecialKeyState.CtrlPressed;
    $shift = $actionContext.SpecialKeyState.ShiftPressed;
    showcontextmenu(@path, $ctrl, $shift);
    return(0);
};

script(on_action_change)args($query, $result, $actionContext)
{
    changequery("dsl " + $result.Title, false);
    return(0);
};
script(on_action_eval_dsl)args($query, $result, $actionContext)
{
    evaldsl($query.SecondToEndSearch, $query, $result, $actionContext);
    return(0);
};
script(on_action_info)args($query, $result, $actionContext)
{
    show();
    echo("key:{0} phase:{1} exe:{2} path:{3} args:{4}", @curkey, @phase, @exe, @path, @args);
    return(0);
};
script(on_action_clear)args($query, $result, $actionContext)
{
    @curkey = "";
    @phase = 0;
    @exe = "";
    @path = "";
    @args = "";
    return(0);
};
script(on_action_reload)args($query, $result, $actionContext)
{
    reloaddsl();
    return(1);
};

script(on_action_menu)args($query, $result, $actionContext)
{
    @path = $result.Title;
    $ctrl = $actionContext.SpecialKeyState.CtrlPressed;
    $shift = $actionContext.SpecialKeyState.ShiftPressed;
    showcontextmenu(@path, $ctrl, $shift);
    return(0);
};

script(on_action_file)args($query, $result, $actionContext)
{
    @path = $result.Title;
    @args = $query.SecondToEndSearch;
    process(@path, @args){
        nowait(true);
        useshellexecute(true);
        verb("open");
    };
    return(1);
};

script(on_action_cmd)args($query, $result, $actionContext)
{
    @path = $result.Title;
    if(fileexist(@path)){
        @path = getdirectoryname(@path);
    };
    @args = $query.SecondToEndSearch;
    process("cmd", "/c start /d " + quotepath(@path) + " " + @args){
        nowait(true);
        useshellexecute(true);
        verb("open");
    };
    return(1);
};

script(on_action_exe)args($query, $result, $actionContext)
{
    @exe = $result.Title;
    @args = $query.SecondToEndSearch;
    process(@exe, @args){
        nowait(true);
    };
    return(1);
};

script(on_action_start)args($query, $result, $actionContext)
{
    @exe = $result.Title;
    @phase = 1;
    changequery("start " + getfilename(@exe), false);
    return(0);
};
script(on_action_start_proj)args($query, $result, $actionContext)
{
    @path = $result.Title;
    @args = $query.SecondToEndSearch.Substring($query.SecondSearch.Length);
    process(@exe, quotepath(@path) + @args){
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
    changequery(@cfg[$key][1], true);
    return(0);
};
script(on_action_app_proj)args($query, $result, $actionContext)
{
    $key = $query.ActionKeyword;
    @args = $query.SecondToEndSearch;
    @path = $result.Title;
    //unity工程可以借助子目录名来筛选
    if($key=="unity" && @path.EndsWith("\\Assets")){
        @path = getdirectoryname(@path);
    };
    process(format(@cfg[$key][2], @exe), format(@cfg[$key][3], quotepath(@path)) + " " + @args){
        nowait(true);
    };
    @phase = 0;
    return(1);
};