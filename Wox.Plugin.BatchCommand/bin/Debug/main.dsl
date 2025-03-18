// Note: When the Title and SubTitle of the Result are the same, Wox considers them to be the same result (that is, it will not update the Action and other fields!
// In the Action processing, the BatchCommand plugin tries to locate the correct Result based on the Title and SubTitle, so there should not be any items in the
// result list with both the Title and SubTitle the same.
// When ActionKeyword is registered as '*', Query.ActionKeyword is an empty string, and the first part of the input is Query.FirstSearch,
// which is different from the non- ActionKeyword registration. Here, use a specific ActionKeyword registration.
/*
//script api
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
context() returns the PluginInitContext object
api() returns the IPublicAPI object, which generally does not need to be used. Some have restrictions. For example, RestarApp cannot be called in the script thread.
metadata() returns the PluginMetadata object
showmsg(title[,subtitle[,icon]]) displays a pop-up message, the missing parameter is string.Empty
restart() restart wox
show() displays the console window, evaldsl will automatically display the window
hide() hides the console window
reloaddsl() reloads main.dsl
evaldsl(dsl code, query, result, actioncontext) executes the dsl code. You can use $query, $result, $actioncontext to access the corresponding parameters in the code.
changequery(query_string, requery) Modifies the query, requery is of bool type
addresult(title, subtitle, icopath, action, query, search[, score[, highlight]]) adds a result to the list, query is the query object, the others are strings, action is the script function name in dsl,
the search parameter is the keyword for Everything, the score is used for sorting, and highlight is a bool value indicating whether the title is fully highlighted (it seems to have no effect),
this script is executed when the user clicks, and the three parameters query, result, and actioncontext are passed to the script.
addcontextmenu(tilte, subtitle, icopath, action, query, result) adds a context menu item, query is the query object, result is the result object, and the others are strings
, action is the name of the script function in dsl. It is executed when the user clicks. It passes 4 parameters query, result, menu, actioncontext to the script. Menu is also a Result object, containing menu item information.
keywordregistered(keyword) determines whether the specified keyword has been registered
clearkeywords(id) clears all keywords of the current plug-in, id is the plug-in id
addkeyword(id, keyword) adds a keyword, id is the plug-in id, keyword is a string
showcontextmenu(path, ctrl, shift) displays the context menu associated with the specified path path. ctrl and shift indicate whether it is the same as pressing the ctrl and shift keys.
reloadautocomplete() reload autocomplete info from the file autocomplete.txt in plugin dir
getautocomplete(key) get autocomplete string, return key if no autocomplete info

tryfindeverything() If the full path of everything has not been recorded, try to find it and return the full path of everything.exe.
everythingexists() determines whether everything is running
everythingreset() clears everything search
everythingsetdefault() sets the default search settings for everything, matchpath false, matchcase false, matchwholdword false, regex false, sort by path asc
everythingmatchpath(bool) sets matchpath, returns the current setting without parameters
everythingmatchcase(bool) sets matchcase, returns the current setting without parameters
everythingmatchwholeword(bool) sets matchwholeword, returns the current setting without parameters.
everythingregex(bool) sets match regex, returns the current setting without parameters
everythingsort(type, asc) sets sort, type can be path/size/time, asc is bool type, true means ascending order, no parameters return the current setting
everythingsort(sort) sets sort, the parameter is an integer (see constants below), no parameters return the current setting
everythingsearch(key[,offset[,maxcount]]) performs a search, key is the search keyword, offset defaults to 0, maxcount defaults to 100, and returns an array of three elements: full_path, size, file_date_time
, size is an integer, other elements are strings

regread(key_name, val_name[, def_val]) reads the registry value
regwrite(key_name, val_name, val[, val_kind]) writes registry values
regdelete(key_name[, val_name]) deletes the registry key or val

key_name is a path string, HKey is as follows:
HKEY_CURRENT_USER
HKEY_LOCAL_MACHINE
HKEY_CLASSES_ROOT
HKEY_USERS
HKEY_PERFORMANCE_DATA
HKEY_CURRENT_CONFIG

val_kind is an integer, from the enumeration:
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
//id:plugin_id，string
//metadata:metadata object，PluginMetadata
//context:context object，PluginInitContext
script(main)args($id, $metadata, $context)
{
    clearkeywords($id);
    addkeyword($id, "dsl");
    addkeyword($id, "menu");
    addkeyword($id, "foldermenu");
    addkeyword($id, "file");
    addkeyword($id, "cmd");
    addkeyword($id, "exe");
    addkeyword($id, "start");
    addkeyword($id, "unity");
    addkeyword($id, "unityfile");
    addkeyword($id, "notepad");
    addkeyword($id, "vscode");
    addkeyword($id, "renderdoc");

    @lastEvalSecondSearch = "";
    @curkey = "";
    @phase = 0;
    @exe = "";
    @path = "";
    @args = "";
    // Configure application parameters: exe query string, alternative query string after selecting exe (keyword + " " + new query),
    // formatted string for the command to be executed (using {0} to reference the first selected result, which is the exe path),
    // formatted string for the command arguments (using {0} to reference the second selected result, generally the file path)
    @cfg = {
        "unity" => ["\\unity.exe", "unity \\Assets", "{0}", "-projectPath {0}"],
        "unityfile" => ["\\unity.exe", "unityfile *.unity", "{0}", "-openfile {0}"],
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
            $autoComplete = getautocomplete($query.SecondSearch);
            if(@lastEvalSecondSearch.Length < $query.SecondSearch.Length && $autoComplete.Length > 0 && $autoComplete != $query.SecondSearch){
                changequery("dsl eval " + $autoComplete, false);
            };
            @lastEvalSecondSearch = $query.SecondSearch;
            addresult("eval", "evaluate dsl code.", "", "on_action_eval_dsl", $query, "");
        }elseif($param=="info"){
            addresult("info", "show query state", "", "on_action_info", $query, "");
        }elseif($param=="clear"){
            addresult("clear", "clear query", "", "on_action_clear", $query, "");
        }elseif($param=="reload"){
            addresult("reload", "reload main.dsl", "", "on_action_reload", $query, "");
        }else{
            addresult("eval", "evaluate dsl code", "", "on_action_change", $query, "");
            addresult("info", "show query state", "", "on_action_change", $query, "");
            addresult("clear", "clear query", "", "on_action_change", $query, "");
            addresult("reload", "reload main.dsl", "", "on_action_change", $query, "");
            //restart is directly handled in C#, and the DSL part only handles the UI show.
            addresult("restart", "restart Wox", "", "on_action_change", $query, "");
        };
    }elseif($key=="menu"){
        everythingsetdefault();
        everythingmatchpath(true);
        $list = everythingsearch($query.FirstSearch);
        addresult("file browser", "select a file", "", "on_action_menu", $query, "", max_artificial_score(), true);
        looplist($list){
            addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_menu", $query, $query.FirstSearch);
        };
    }elseif($key=="foldermenu"){
        everythingsetdefault();
        everythingmatchpath(true);
        $list = everythingsearch($query.FirstSearch);
        addresult("folder browser", "select a folder", "", "on_action_foldermenu", $query, "", max_artificial_score(), true);
        looplist($list){
            addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_foldermenu", $query, $query.FirstSearch);
        };
    }elseif($key=="file"){
        everythingsetdefault();
        $list = everythingsearch($query.FirstSearch);
        addresult("file browser", "select a file", "", "on_action_file", $query, "", max_artificial_score(), true);
        addresult("folder browser", "select a folder", "", "on_action_file", $query, "", max_artificial_score() - 1, true);
        looplist($list){
            addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_file", $query, $query.FirstSearch);
        };
    }elseif($key=="cmd"){
        everythingsetdefault();
        $list = everythingsearch($query.FirstSearch);
        addresult("file browser", "select a file", "", "on_action_cmd", $query, "", max_artificial_score(), true);
        looplist($list){
            addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_cmd", $query, $query.FirstSearch);
        };
    }elseif($key=="exe"){
        everythingsetdefault();
        $list = everythingsearch($query.FirstSearch);
        addresult("file browser", "select a file", "", "on_action_exe", $query, "", max_artificial_score(), true);
        looplist($list){
            addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_exe", $query, $query.FirstSearch);
        };
    }elseif($key=="start"){
        everythingsetdefault();
        if(@phase==0){
            $list = everythingsearch($query.FirstSearch);
            addresult("file browser", "select a file", "", "on_action_start", $query, "", max_artificial_score(), true);
            looplist($list){
                addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_start", $query, $query.FirstSearch);
            };
        }elseif(@phase==1){
            $list = everythingsearch($query.SecondSearch);
            addresult("file browser", "select a file", "", "on_action_start_proj", $query, "", max_artificial_score(), true);
            addresult("folder browser", "select a folder", "", "on_action_start_proj", $query, "", max_artificial_score() - 1, true);
            looplist($list){
                addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_start_proj", $query, $query.SecondSearch);
            };
        };
    }elseif(!isnull($key)){
        if(!isnull(hashtableget(@cfg, $key))){
            everythingsetdefault();
            if(@phase == 0){
                $cfglist = @cfg[$key];
                $searchKey = $cfglist[0];
                if(!isnullorempty($query.FirstSearch)){
                    $searchKey = $query.FirstSearch;
                };
                $list = everythingsearch($searchKey);
                addresult("file browser", "select a file", "", "on_action_app", $query, "", max_artificial_score(), true);
                looplist($list){
                    addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_app", $query, $searchKey);
                };
            }elseif(@phase == 1){
                $list = everythingsearch($query.FirstSearch);
                addresult("file browser", "select a file", "", "on_action_app_proj", $query, "", max_artificial_score(), true);
                addresult("folder browser", "select a folder", "", "on_action_app_proj", $query, "", max_artificial_score() - 1, true);
                looplist($list){
                    addresult($$[0], "" + $$[1] + " " + $$[2], "", "on_action_app_proj", $query, $query.FirstSearch);
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
//$menu is also a Result object
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

script(try_select_file_or_folder)args($title)
{
    $p = "";
    if(@path=="file browser"){
        $p = openfiledlg("select a file", "", "txt|*.txt;*.log;*.dsl;*.htm;*.html|code|*.cs;*.h;*.hpp;*.hxx;*.c;*.cc;*.cpp;*.cxx;*.m;*.mm;*.py;*.pl;*.js;*.lua|doc|*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx|exe|*.exe|all|*.*", 5);
    }
    elseif(@path=="folder browser"){
        $p = folderdlg("select a folder");
    };
    return($p);
};

script(on_action_menu)args($query, $result, $actionContext)
{
    @path = $result.Title;
    $ctrl = $actionContext.SpecialKeyState.CtrlPressed;
    $shift = $actionContext.SpecialKeyState.ShiftPressed;

    $p = try_select_file_or_folder(@path);
    if(!isnullorempty($p)){
        @path = $p;
    };

    showcontextmenu(@path, $ctrl, $shift);
    return(0);
};

script(on_action_foldermenu)args($query, $result, $actionContext)
{
    @path = $result.Title;
    $ctrl = $actionContext.SpecialKeyState.CtrlPressed;
    $shift = $actionContext.SpecialKeyState.ShiftPressed;

    $p = try_select_file_or_folder(@path);
    if(!isnullorempty($p)){
        @path = $p;
    };

    if(fileexist(@path)){
        @path = getdirectoryname(@path);
    };

    showcontextmenu(@path, $ctrl, $shift);
    return(0);
};

script(on_action_file)args($query, $result, $actionContext)
{
    @path = $result.Title;
    @args = $query.SecondToEndSearch;

    $p = try_select_file_or_folder(@path);
    if(!isnullorempty($p)){
        @path = $p;
    };

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
    @args = $query.SecondToEndSearch;

    $p = try_select_file_or_folder(@path);
    if(!isnullorempty($p)){
        @path = $p;
    };
    
    if(fileexist(@path)){
        @path = getdirectoryname(@path);
    };

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

    $p = try_select_file_or_folder(@exe);
    if(!isnullorempty($p)){
        @exe = $p;
    };

    process(@exe, @args){
        nowait(true);
    };
    return(1);
};

script(on_action_start)args($query, $result, $actionContext)
{
    @exe = $result.Title;
    @phase = 1;

    $p = try_select_file_or_folder(@exe);
    if(!isnullorempty($p)){
        @exe = $p;
    };

    changequery("start " + getfilename(@exe), false);
    return(0);
};
script(on_action_start_proj)args($query, $result, $actionContext)
{
    @path = $result.Title;
    @args = $query.SecondToEndSearch.Substring($query.SecondSearch.Length);

    $p = try_select_file_or_folder(@path);
    if(!isnullorempty($p)){
        @path = $p;
    };

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

    $p = try_select_file_or_folder(@exe);
    if(!isnullorempty($p)){
        @exe = $p;
    };

    changequery(@cfg[$key][1], true);
    return(0);
};
script(on_action_app_proj)args($query, $result, $actionContext)
{
    $key = $query.ActionKeyword;
    @args = $query.SecondToEndSearch;
    @path = $result.Title;

    $p = try_select_file_or_folder(@path);
    if(!isnullorempty($p)){
        @path = $p;
    };

    //The Unity project can be filtered by subdirectory.
    if($key=="unity"){
        if(@path.EndsWith("\\Assets")){
            @path = getdirectoryname(@path);
        }
        else{
            $ix = @path.IndexOf("\\Assets\\", 0);
            if($ix>0){
                @path = @path.Substring(0, $ix);
            };
        };
    };
    process(format(@cfg[$key][2], @exe), format(@cfg[$key][3], quotepath(@path)) + " " + @args){
        nowait(true);
    };
    @phase = 0;
    return(1);
};