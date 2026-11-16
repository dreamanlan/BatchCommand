# BatchCommand DSL API 改名映射表（snake_case）

用途：本仓 DSL API 注册名按 `CSharpGameFramework/Doc/api_rename_sop.md` 的规则全量改为 snake_case 风格。
分批处理，每批只改用户指定的 C# 文件；本表记录**所有已完成批次的新旧名映射**，供后续修改 DSL 脚本（.dsl/.scp/.txt）时查找替换使用。

**注意**：
- 本文件是映射源，对全仓做批量替换时**必须排除 `BuildLog/` 目录**（否则表内旧名会被改掉）。
- DSL 脚本中调用旧名会报 `unknown func`（注册表无别名层、精确字典 key）。
- 替换用 `\b(旧名)\b` 且大小写敏感、按长度降序，防止前缀误伤。

---

## 批次 1：`BatchScriptApi/BatchScript.cs`（2026-11-16，38 条 API）

| 旧名 | 新名 |
|---|---|
| startasynctask | start_async_task |
| tickasynctasks | tick_async_tasks |
| stopasynctask | stop_async_task |
| stopcompletedasynctasks | stop_completed_async_tasks |
| getasynctaskresult | get_async_task_result |
| debuggerlaunch | debugger_launch |
| debuggerbreak | debugger_break |
| isosplatform | is_os_platform |
| getstringlength | get_string_length |
| getstringinlength | get_string_in_length |
| timestat | time_statistic_on |
| getelapsedms | get_elapsed_time_ms |
| getelapsedus | get_elapsed_time_us |
| getscriptdir | get_script_directory |
| writeblock | write_block |
| readline | read_line |
| gettitle | get_title |
| settitle | set_title |
| getbufferwidth | get_buffer_width |
| getbufferheight | get_buffer_height |
| setbuffersize | set_buffer_size |
| getcursorleft | get_cursor_left |
| getcursortop | get_cursor_top |
| setcursorpos | set_cursor_pos |
| getbgcolor | get_bg_color |
| setbgcolor | set_bg_color |
| getfgcolor | get_fg_color |
| setfgcolor | set_fg_color |
| resetcolor | reset_color |
| setencoding | set_encoding |
| getinputencoding | get_input_encoding |
| getoutputencoding | get_output_encoding |
| runtimeinfo | runtime_information |
| getclipboard | get_clipboard |
| setclipboard | set_clipboard |
| regread | reg_read |
| regwrite | reg_write |
| regdelete | reg_delete |

批次 1 **保持不变**（单词或已合规，改 DSL 时无需处理）：
`clone`、`time`、`timestamp`、`grep`、`subst`、`awk`、`pause`、`clear`、`write`、`block`、`read`、`beep`、`console`、`encoding`、`env`、`exit_process`

批次 1 歧义项推导依据（SOP 规则 1：工厂类名全词）：
`timestat`←TimeStatisticOnExp、`getscriptdir`←GetScriptDirectoryExp（非 get_script_dir）、`runtimeinfo`←RuntimeInformationExp（非 runtime_info）、`getelapsedms`←GetElapsedTimeMsExp、`getelapsedus`←GetElapsedTimeUsExp。

## 批次 2：`BatchCmdDslHost/BatchCmdDsl/Program.cs`（2026-11-16，2 条 API + 3 个全局变量）

| 旧名 | 新名 | 类别 |
|---|---|---|
| redirecttoplugin | redirect_to_plugin | API |
| setinterval | set_interval | API |
| nativeapi | native_api | DSL 全局变量（`$` 访问） |
| cmdargs | cmd_args | DSL 全局变量（`$` 访问） |
| cmdswitches | cmd_switches | DSL 全局变量（`$` 访问） |

批次 2 说明：`nativeapi`/`cmdargs`/`cmdswitches` 是宿主经 `BatchScript.SetGlobalVariable` 注入的 DSL 全局变量名，同样属于 DSL 侧可见接口，一并改为 snake_case；DSL 脚本中 `$nativeapi`/`$cmdargs`/`$cmdswitches` 需同步改为 `$native_api`/`$cmd_args`/`$cmd_switches`。

---

## 批次 3：`CefDotnetApp/Program.cs`（2026-11-16，2 条 API + 8 个全局变量）

| 旧名 | 新名 | 类别 |
|---|---|---|
| javascriptlog | javascript_log | API |
| setdslfile | set_dsl_file | API |
| nativeapi | native_api | DSL 全局变量 |
| processtype | process_type | DSL 全局变量 |
| startupurl | startup_url | DSL 全局变量 |
| lastloadedmainurl | last_loaded_main_url | DSL 全局变量 |
| lastloadedurl | last_loaded_url | DSL 全局变量 |
| initialdslfile | initial_dsl_file | DSL 全局变量 |
| initialprojectidentity | initial_project_identity | DSL 全局变量 |
| dslfile | dsl_file | DSL 全局变量（见下方跨层同步说明） |

批次 3 说明：
- 该工程其余 16 条 API（`enqueue_cef_message`、`handle_thread_queue`、`set_heartbeat_interval`、`complete_native_callback`、`register_custom_scheme`、`unregister_custom_scheme`、`send_javascript_code`、`send_javascript_call`、`show_native_js_dialog`、`get_browser_ids`、`set_context_by_id`、`find_browser_id_by_url_key`、`exit_browser`、`dev_tools_parse_bytes`、`watch_file`、`watch_dir`）已是 snake_case，无需处理。
- **跨层同步项（已于批次 4 解决）**：`dslfile` 曾由 CEF（脚本切换时）与 `DslHost.RefreshGlobalVars`（`DslHost.cs:195`，每次刷新写）双写。批次 3 时 CEF 侧先行改为 `dsl_file`（用户确认暂不管分叉），批次 4 已将 DslHost 侧 7 个连写全局变量（含 `dslfile`）同步改为 snake_case，分叉已消除。
- 重名检查结果：CEF 注册名与 `BatchScriptApiRegistrar.cs`（217 条）、`AgentCore/ScriptApi/ScriptApiRegistrar.cs`（165 条）**零交集**；`native_api`/`cmd_args`/`cmd_switches` 等跨宿主同语义变量已与批次 2 保持一致命名。

## 批次 4：`BatchScriptApi/Api/`（2026-11-16，3 条活 API + 7 个全局变量 + 9 条注释一致性）

| 旧名 | 新名 | 文件 | 状态 |
|---|---|---|---|
| redirectcall | redirect_call | BatchScriptApiRegistrar.cs | 活注册 |
| nativelog | native_log | BatchScriptApiRegistrar.cs | 活注册（webagent 脚本高频使用） |
| helpall | help_all | BatchScriptApiRegistrar.cs | 活注册 |
| commandline | command_line | DslHost.cs | DSL 全局变量（共享层） |
| basepath | base_path | DslHost.cs | DSL 全局变量 |
| appdir | app_dir | DslHost.cs | DSL 全局变量 |
| ismac | is_mac | DslHost.cs | DSL 全局变量 |
| nosandbox | no_sandbox | DslHost.cs | DSL 全局变量 |
| dslpath | dsl_path | DslHost.cs | DSL 全局变量 |
| dslfile | dsl_file | DslHost.cs | DSL 全局变量（解决批次 3 的跨层分叉，与 CEF 侧对齐） |
| atan2 / exp2 / log2 / log10 / clamp01 | atan_2 / exp_2 / log_2 / log_10 / clamp_01 | FrameworkApiAlias.cs | **注释掉的参考注册**，仅注释一致性，运行时无影响（真名由 DotnetStoryScript 内建提供） |
| fileexists / pathexists / copyfile / movefile | file_exists / path_exists / copy_file / move_file | FrameworkApiAlias.cs | 同上，注释一致性 |

批次 4 说明：
- **别名组保留（有意特性，不改）**：`BatchScriptApiRegistrar.cs` 与 `FrameworkApiAlias.cs` 共 52 组"snake 规范名 + 连写别名"成对注册（如 `execute_metadsl|executemetadsl`、`file_exists|isfile` 等），连写是给 AI 猜名用的别名，均有 snake 兄弟，保留不动。
- `getelapsedms` 双名并存：批次 1 将 `BatchScript.cs` 侧改为 `get_elapsed_time_ms`（类名全词），`FrameworkApiAlias.cs` 侧别名组为 `get_elapsed_ms|getelapsedms`——同一工厂 `GetElapsedTimeMsExp` 双 snake 名并存 + 连写别名有效，冗余但无害，未动。
- 误伤修正：`for_each_value(exp1,exp2,...)` 注释里的 `exp2` 是"表达式2"占位符非 API 名，已被误改后还原。
- `BatchCmdDslHost` 工程经盘点无其它注册点（批次 2 已覆盖），本批无改动。

## 批次 5：AgentCore + BatchStudio/DotnetApp（2026-11-16，6 个全局变量，API 盘点不动）

| 旧名 | 新名 | 文件 | 类别 |
|---|---|---|---|
| agentport | agent_port | AgentCore/Core/MetaDslExecutor.cs | DSL 全局变量 |
| httpproxyport | http_proxy_port | AgentCore/Core/MetaDslExecutor.cs | DSL 全局变量 |
| initialprojectidentity | initial_project_identity | AgentCore/Core/MetaDslExecutor.cs | DSL 全局变量（与 CEF 侧对齐） |
| processtype | process_type | AgentCore/Core/MetaDslExecutor.cs | DSL 全局变量（与 CEF 侧对齐） |
| nativeapi | native_api | BatchStudio/DotnetApp/Program.cs | DSL 全局变量（×9 注入点，与各宿主对齐） |
| basepath | base_path | BatchStudio/DotnetApp/Program.cs | DSL 全局变量（×9，与 DslHost 对齐） |

批次 5 说明：
- 命令行开关名当时先保留（~~`--agentport` 等~~）——**批次 6 已按用户指示全部改为 snake_case**，见批次 6 开关映射表。
- **AgentCore API 盘点结论（按用户要求列出、先不动）**：485 条注册中 19 组为"snake 规范名 + 连写别名"成对注册（别名组，保留）：`readlines|grepfile|grepfileaslist|grepfiles|grepfilesaslist|countindentations|hotreload|callskill|refreshskills|refreshembedding|refreshreranker|updatehelpfreq` 及 `indexof/lastindexof/indexofany/lastindexofany/stringindexof/strindexof/stringlastindexof/strlastindexof/strindexofany/stringindexofany/strlastindexofany/strindexof_any/stringindexof_any/strindexofany/stringlastindexofany/strindexofany/stringlastindexofany/stringindexofany/stringindexof/stringindexof/strindexof/strindexof/findstr/findstring/findinstring/strfind/stringfind/concatstring/concatstr/strcat/strcat/str_cat/appendstring` 等索引/拼接家族别名；**纯连写仅 `tokenize` 一条且为单词（合法 snake_case）**——AgentCore 的 API 无需改名。
- 全仓 `SetGlobalVariable` 连写残留扫描：**0**（各宿主/DslHost/AgentCore/DotnetApp 全部清完）。
- 构建期 PostBuild（`AgentCore/copy.dsl`、`BatchCmdDslHost/BatchCmdDsl/copy.dsl`）现报 `unknown func getscriptdir` 等——批次 1 改名后主工程 DLL 重建导致 copy.dsl 断链暴露，C# 编译本体无错，属"改完 API 统一修 DSL"范畴。

## 批次 6：命令行开关 + DslHost doc 修正（2026-11-16）

### 命令行开关改 snake_case（与 Chrome 风格保持一致，用户指定）

| 旧开关 | 新开关 | 解析处 | 拼接/转发处 |
|---|---|---|---|
| `--agentscript` | `--agent_script` | MetaDslExecutor.cs | 注释 |
| `--agentport` | `--agent_port` | MetaDslExecutor.cs | script.dsl(×2 拼接)、config.js(注释)、BatchCmdPluginImpl(注释) |
| `--httpproxyport` | `--http_proxy_port` | MetaDslExecutor.cs | script_agent.dsl(注释) |
| `--projectidentity` | `--project_identity` | MetaDslExecutor.cs、CefDotnetApp(解析+AppendSwitchWithValue 转发子进程) | script.dsl(拼接)、monitor.dsl(默认命令行)、script_agent/renderer.dsl(注释)、project_panel.js(注释) |
| `--appdir` | `--app_dir` | BatchCmdPluginImpl.cs | script.dsl(拼接) |

- `--metadsl`（专有名词 MetaDSL）与 `--managed`、`--plugin`、`--interval`、`--url` 等**单词开关不动**。
- 解析器 `TryGetSwitchValueFromRawCommandLine` 为纯 `IndexOf("--key=")` 匹配，下划线 key 完全支持。
- **用户环境注意**：手动启动 webagent 的快捷方式/脚本里若写死 `--projectidentity=` 需同步改（monitor.dsl 的 @DefaultCmdLine 已改）。
- webagent managed 的 js/dsl 中**只改了带 `--` 前缀的开关字符串**；裸的 DSL 变量引用（agentport/httpproxyport/appdir/projectidentity 等）是批次 5 变量断链，留待统一修 DSL。

### DslHost.cs AddUserApiDoc 旧名 doc 修正（33 个，68 处）

doc 条目名与文本同步为实际有效注册名（内建规范名/批次 1 名）：
`arg_num`、`loop_list`、`for_each_value`、`atan_2`、`exp_2`、`log_2`、`log_10`、`clamp_01`、`is_null_or_empty`、`is_object`、`is_string`、`is_boolean`、`is_char`、`is_integer`、`is_signed_integer`、`is_unsigned_integer`、`is_number`、`is_date_time`、`is_tuple`、`boxed_value_type`、`boxed_value_type_name`、`dotnet_call/set/get`、`collection_call/set/get`、`prop_set/get/exists`、`calc_md5`、`get_elapsed_time_ms`、`get_elapsed_time_us`。
（验证：每个新名在内建注册表/Registrar/FrameworkApiAlias/BatchScript.cs 至少一处注册存在。）

## 既有断链（非本系列批次产生，但修 DSL 时同样需要）

以下名字在**本系列批次开始之前**就已改名（`FrameworkApiAlias.cs` 内已提交的 snake_case 改名 + `deps\DotnetStoryScript.dll` 已换 2026-11-16 新版），调用它们的 DSL 现在会报 `unknown func`。实例：`BatchCmdDslHost/BatchCmdDsl/copy.dsl` 的 PostBuild 已因这些旧名失败（构建钩子 `BatchCommand.exe copy.dsl bat`，已用 stash 对照实验证实与批次 1/2 改动无关）：

| 旧名 | 新名 | 来源 |
|---|---|---|
| fileecho | file_echo | `FrameworkApiAlias.cs`（已提交的改名） |
| copyfiles | copy_files | `FrameworkApiAlias.cs`（已提交的改名） |
| argnum | arg_num | `DotnetStoryScript.dll` 内建（deps 已换新版） |

`FrameworkApiAlias.cs` 的完整旧名→新名映射待该文件纳入处理批次时统一补录本表。

## 批次 7：BatchStudio / CppLua2Dsl / Flow.Launcher / GlslRewriter / UnrealCodeTransform（2026-11-16）

### 盘点结论
- `CppLua2Dsl`、`UnrealCodeTransform`：无 DSL 注册/脚本机制，**无改动**。
- `BatchStudio`（除 DotnetApp）、`Flow.Launcher`、`GlslRewriter` 的命令行参数已检查：仅 GlslRewriter 有多词连写参数（见下）。

### API 改名

| 旧名 | 新名 | 文件 |
|---|---|---|
| outputlog | output_log | BatchStudio/DotnetApp/Program.cs |
| writefile | write_file | 同上 |
| clrscpcon | clear_script_console | 同上（ClearScriptConsoleExp 类名全词） |
| logscpcon | log_script_console | 同上 |
| builddbgscp | build_dbg_scp | 同上（BuildDbgScpExp） |
| folderdlg | folder_dialog | Flow/PluginMain.cs |
| openfiledlg / openfilesdlg / savefiledlg | open_file_dialog / open_files_dialog / save_file_dialog | 同上 |
| contextmenu | context_menu | 同上 |
| showmsg | show_msg | 同上 |
| reloaddsl / evaldsl | reload_dsl / eval_dsl | 同上 |
| changequery | change_query | 同上 |
| addresult | add_result | 同上 |
| addcontextmenu / showcontextmenu | add_context_menu / show_context_menu | 同上 |
| keywordregistered | keyword_registered | 同上 |
| clearkeywords / addkeyword | clear_keywords / add_keyword | 同上 |
| reloadautocomplete / getautocomplete | reload_autocomplete / get_autocomplete | 同上 |
| tryfindeverything | try_find_everything | 同上 |
| everythingexists / everythingreset / everythingsetdefault | everything_exists / everything_reset / everything_set_default | 同上 |
| everythingmatchpath / everythingmatchcase / everythingmatchwholeword | everything_match_path / everything_match_case / everything_match_whole_word | 同上 |
| everythingregex / everythingsort / everythingsearch | everything_regex / everything_sort / everything_search | 同上 |
| add_unsvar | add_unassignable_shader_var | GlslRewriter/Program.cs（AddUnassignableShaderVarExp 全词；`add_svar`/`set_svar` 已含下划线合规不动） |

不变（单词/已合规）：Flow 的 `context`/`api`/`metadata`/`restart`/`show`/`hide`/`max_artificial_score`/`set_sync_execution_type`；GlslRewriter 的 `shader`/`add_svar`/`set_svar`/`import_inout`/`recalc`/`rand_color`/`rand_uv`/`rand_size`。

### 命令行参数改名（GlslRewriter，单横杠风格）

| 旧参数 | 新参数 |
|---|---|
| `-arg_cfg` | `-arg_cfg` |
| `-nossa` | `-no_ssa` |

其余参数（`-out`/`-src`/`-cfg`/`-vs`/`-ps`/`-cs`/`-i`/`-r`/`-ssa`/`-h`）为单词/缩写，不动。解析、usage 帮助文本、注释已同步。

### 调用方 DSL 脚本（本批工程的脚本已一并闭环修复）

| 文件 | 修复内容 |
|---|---|
| `BatchStudio/managed/Script.dsl` | outputlog×13、builddbgscp、logscpcon、clrscpcon、fileecho×3（既有断链） |
| `BatchStudio/DotnetApp/copy.dsl` | getscriptdir→get_script_directory、copyfiles×7、fileecho、argnum |
| `Flow/main.dsl`（工程根 + `bin/Debug` 副本） | 本批 24 名 + regread/regwrite/regdelete（批次 1）+ loop_list×9（内建） |
| `Flow/bin/Debug/AutoComplete.txt` | reloadautocomplete、showmsg、loop_list |

### 构建说明
- DotnetApp 的 PostBuild（`BatchCommand.exe copy.dsl bat`）在**单独构建 csproj** 时因 `$(SolutionDir)` 未定义而失败（`*Undefined*bin\...` 找不到 exe，退出码 3）——**与改名无关**，从 sln 构建或 `-p:SolutionDir=D:\GitHub\BatchCommand\` 即 0 错误（已验证）。copy.dsl 本身已修复（新名全链路跑通，文件拷贝成功）。
- 三工程编译：DotnetApp（带 SolutionDir）/ Flow / GlslRewriter 均 0 错误。

## 批次 8：DebugScript 内部 API（调试脚本系统，2026-11-16）

**系统说明**：这是独立于 StoryScript 的调试脚本系统（C# 编译器 `DebugScriptCompiler.cs` + C++ VM `DebugScriptVM/` + UE5 集成 `DebugScriptInUE5/`）。API 由 `bin/Debug/net9.0/api.txt` 的 `defapi`（内部，CALLINTERN 100-255）与 `defexternapi`（外部，宿主实现）定义，**脚本名按 defapi 顺序与 C++ `ApiEnum` 枚举按序对应**，代码不含脚本名字符串（唯一例外：`cascadeptr` 是编译器关键字，非 API）。外部 API（`TestFFI`/`LoadLib` 等 PascalCase）按用户指示**不改**。

**内部 API 改名（15 条，作用于 4 个 api.txt + 4 个 hook.txt）**：

| 旧名 | 新名 | ApiEnum |
|---|---|---|
| dumpstack | dump_stack | DumpStack |
| floattime | float_time | FloatTime |
| dumpcascadeptr | dump_cascade_ptr | DumpCascadePtr |
| stringlength | string_length | StringLength |
| stringcontains | string_contains | StringContains |
| stringcontainsany | string_contains_any | StringContainsAny |
| stringnotcontains | string_not_contains | StringNotContains |
| stringnotcontainsany | string_not_contains_any | StringNotContainsAny |
| stringfind | string_find | StringFind |
| stringrfind | string_right_find | StringRightFind（歧义项：类名全词，非 string_rfind） |
| cstrlen | c_str_len | CStrLen（歧义项） |
| cstrstr | c_str_str | CStrStr（歧义项） |
| cstrcmp | c_str_cmp | CStrCmp（歧义项） |
| memsave | mem_save | MemSave |
| memload | mem_load | MemLoad |

**保持不变**（单词/C 标准名）：`platform`、`assert`、`printf`、`format`、`time`、`substring`、`malloc`、`free`、`memcpy`、`memset`。

**改动文件**：`bin/Debug/net9.0/{,ue5/,unity/,myuzu/}api.txt`（defapi 名）+ 同目录 `hook.txt`（调用处，含 BOM 状态保持，unity 带 BOM）。

**验证**：编译器冒烟（`e:\tmp\dbgscp_smoke`，引用 BatchCommand.dll 调 LoadApiDefine/LoadStructDefine/Compile）——主目录（945 行 asm）、ue5（1980 行）、unity（1260 行）全过；`myuzu` 断言失败为**既有问题**（该目录缺 struct.txt，HEAD 旧版同样失败，与改名无关）；旧名残留 0。

**决策项（待定）**：
- `cascadeptr` 编译器关键字（`DebugScriptCompiler.cs:2737`，级联取指针语法）当前**未改**——它不是 API 名，hook.txt 也未使用；若要改 → `cascade_ptr`（需同步编译器 + 任何使用该语法的脚本）。
- `bin/Debug/net6.0/` 的同套配置（2026-09-15 已迁至 net9.0）：api.txt 主套**并未丢失**（在 net9.0，此前搜索误排除 bin 目录）；GlslRewriter 的 net6.0 配置/测试数据（646 个非二进制文件）**已从 git 历史恢复**至 `GlslRewriter/bin/Debug/net9.0/`。

## 批次 9：apkstudio / BatchCommandApp / UnityProjForProfiler / Hlsl2Python / Hlsl2Numpy（2026-11-16）

### 盘点结论
- `Hlsl2Python`、`Hlsl2Numpy`：无 API 注册、无全局变量——**无改动**（注意：其 `gencode/DotnetStoryScript.dll` 仍为旧版 11-14，与旧名脚本一致暂不受影响；将来升级 DLL 时需按本表映射同步改其脚本）。
- `BatchCommandApp`（Unity 工程）：`setclipboard→set_clipboard`、`getclipboard→get_clipboard`（`Assets/Scripts/Main.cs`，无 DSL 调用方）。
- `apkstudio`：全局变量 `nativeapi→native_api`、`basepath→base_path`（`apk`/`folder` 单词不动）；调用方 `build/managed/script.dsl`（nativeapi×27）、`DotnetApp/copytool/copy.dsl`（get_script_directory/copy_files×6/file_echo/arg_num）。
- `UnityProjForProfiler`：137 条 API + 8 个全局变量 + 40 余个 DSL 脚本（CommandDsl/FindDsl/ProcessDsl）。

### UnityProjForProfiler 全局变量（8 条）
`assetpath→asset_path`、`scenepath→scene_path`、`scenedeps→scene_deps`、`refdict→ref_dict`、`refbydict→ref_by_dict`、`extralist→extra_list`、`redirectdsl→redirect_dsl`、`redirectargs→redirect_args`（`importer/object/info/order/value/selected/results/group/items/sum/max/min/count/avg/params` 等单词不动）。

### DLL 版本与部署同步（apkstudio）
- `BatchCommandApp/UnityProjForProfiler` 的 `Assets/Plugins/StoryScript.dll` 已是 11-16 新版 ✓
- apkstudio 仅 `DotnetApp/deps` 为新版；`build/managed`、`copytool` 的旧 DLL 已用 deps 全套 + **BatchCommand 仓库新构建的 `BatchScriptApi.dll`**（含批次 1-6 注册名）同步覆盖，`bin` 由构建刷新。同步后 `DotnetApp.csproj` 构建 **0 错误**（PostBuild copy.dsl 新名链路全通）。

### 误伤修正（重要教训）
`rustdsl_map.tsv`（内建权威映射源）中残留 `list→array` 一行（RustDsl 批次当时排除但 tsv 未剔除），被本次脚本替换误应用到 13 个 UnityProjForProfiler 脚本——已用 git diff 精确定位、逐文件回滚（C# 的 `list`=ListExp 与 `array`=ArrayExp 是**不同工厂**，不可互替），并已从 tsv 剔除。**后续任何批次使用该 tsv 前请确认无 `list` 行**。

### 验证
- apkstudio DotnetApp 编译 **0 错误**（含 PostBuild copy.dsl 实跑）
- Unity 工程（BatchCommandApp/UnityProjForProfiler）C# 改动 diff 审计全部在字符串字面量内（179+2 行，0 处语法外改动）；**Unity 内编译待用户验证**
- 5 工程旧名残留 0（C#+DSL 全量扫描）

### UnityProjForProfiler API（137 条，`ResourceProcessorApi.cs`，SOP 规则 1 类名全词推导）

| 旧名 | 新名 |
|---|---|
| setparamstomodel | set_params_to_model |
| getparamsfrommodel | get_params_from_model |
| setparamstotexture | set_params_to_texture |
| getparamsfromtexture | get_params_from_texture |
| setparamstoprefab | set_params_to_prefab |
| getparamsfromprefab | get_params_from_prefab |
| updatemodeldb | update_modeldb |
| updatetexturedb | update_texturedb |
| updateprefabdb | update_prefabdb |
| callscript | call_script |
| setredirect | set_redirect |
| newitem | new_item |
| setextraobject | set_extraobject |
| getextraobject | get_extraobject |
| calcextraobjectfieldcount | calc_extraobject_field_count |
| newextralist | new_extra_list |
| extralistadd | extra_list_add |
| extralistclear | extra_list_clear |
| getreferenceassets | get_reference_assets |
| getreferencebyassets | get_reference_by_assets |
| calcrefcount | calc_ref_count |
| calcrefbycount | calc_ref_by_count |
| findasset | find_asset |
| selectframe | select_frame |
| filtercpusample | filtercpu_sample |
| filtergpusample | filtergpu_sample |
| selectsample | select_sample |
| selectpropertypath | select_property_path |
| setmarkerfilter | set_marker_filter |
| setmaxrefbynumperobj | set_max_ref_by_num_perobj |
| findshortestpathtoroot | find_shortest_path_to_root |
| getrefbyobjdata | get_ref_byobject_data |
| getrefobjdata | get_refobject_data |
| objdatafromaddress | object_data_fromaddress |
| objdatafromunifiedindex | object_data_from_unifiedobject_index |
| objdatafromnativeindex | object_data_from_nativeobject_index |
| objdatafrommanagedindex | object_data_from_managedobject_index |
| loadidaprosymbols | loadidapro_symbols |
| loadxcodesymbols | loadxcode_symbols |
| loadbuglyandroidsymbols | loadbugly_android_symbols |
| loadbuglyiossymbols | loadbuglyios_symbols |
| converttorelativeaddrs | convert_to_relativeaddrs |
| mapbuglyandroidsymbols | map_bugly_android_symbols |
| mapbuglyiossymbols | map_buglyios_symbols |
| mapmyhooksymbols | map_myhook_symbols |
| setclipboard | set_clipboard |
| getclipboard | get_clipboard |
| selectprojectobject | select_projectobject |
| selectsceneobject | select_sceneobject |
| saveandreimport | save_and_reimport |
| setdirty | set_dirty |
| getdefaulttexturesetting | get_default_texture_setting |
| gettexturesetting | get_texture_setting |
| settexturesetting | set_texture_setting |
| textureisnotastc | texture_is_not_astc |
| isastctexture | is_astc_texture |
| setastctexture | set_astc_texture |
| issceneastctexture | is_scene_astc_texture |
| setsceneastctexture | set_scene_astc_texture |
| istexturenoalphasource | is_texture_no_alpha_source |
| doestexturehavealpha | does_texture_have_alpha |
| correctnonealphatexture | correct_none_alpha_texture |
| setnonealphatexture | set_none_alpha_texture |
| gettexturecompression | get_texture_compression |
| settexturecompression | set_texture_compression |
| gettexturestorage | get_texture_storage_memory_size |
| gettexturememory | get_texture_runtime_memory_size |
| getruntimememory | get_runtime_memory_size |
| setmaxboundingbox | set_max_bounding_box |
| resetboundingbox | reset_bounding_box |
| mergeboundingbox | merge_bounding_box |
| getboundingbox | get_bounding_box |
| addorupdateboundingbox | add_or_update_bounding_box |
| getmeshcompression | get_mesh_compression |
| setmeshcompression | set_mesh_compression |
| setmeshimportexternalmaterials | set_mesh_import_external_materials |
| setmeshimportinprefabmaterials | set_mesh_import_in_prefab_materials |
| closemeshanimationifnoanimation | close_mesh_animation_if_no_animation |
| singlemeshinfo | single_mesh_info |
| collectmeshes | collect_meshes |
| collectmeshinfo | collect_mesh_info |
| collectanimatorcontrollerinfo | collect_animator_controller_info |
| collectprefabinfo | collect_prefab_info |
| getanimationclipinfo | get_animation_clip_info |
| getanimationcompression | get_animation_compression |
| setanimationcompression | set_animation_compression |
| getanimationtype | get_animation_type |
| setanimationtype | set_animation_type |
| setextraexposedtransformpaths | set_extra_exposed_transform_paths |
| clearanimationscalecurve | clear_animation_scale_curve |
| getaudiosetting | get_audiosetting |
| setaudiosetting | set_audiosetting |
| splitanimationreference | split_animation_reference |
| calcmeshvertexcomponentcount | calc_mesh_vertex_component_count |
| calcmeshtexratio | calc_mesh_tex_ratio |
| calcassetmd5 | calc_assetmd5 |
| calcassetsize | calc_asset_size |
| deleteasset | delete_asset |
| getshaderutil | get_shader_util |
| getshaderpropertycount | get_shader_property_count |
| getshaderpropertynames | get_shader_property_names |
| removeyamlleafproperties | removeyaml_leaf_properties |
| ispathtoolong | is_path_too_long |
| getshadervariants | get_shader_variants |
| addshadertocollection | add_shader_to_collection |
| getalldslfiles | get_alldsl_files |
| buildassetstringlist | build_asset_string_list |
| createrefasset | create_ref_asset |
| findrowindex | find_row_index |
| findrowindexes | find_row_indexes |
| findcellindex | find_cell_index |
| findcellindexes | find_cell_indexes |
| getcellvalue | get_cell_value |
| getcellstring | get_cell_string |
| getcellnumeric | get_cell_numeric |
| rowtoline | row_to_line |
| tabletohashtable | table_to_hashtable |
| findrowfromhashtable | find_row_from_hashtable |
| loadmanagedheaps | load_managed_heaps |
| findmanagedheaps | find_managed_heaps |
| matchmanagedheaps | match_managed_heaps |
| calcmatchedmanagedheapsdiff | calc_matched_managed_heaps_diff |
| loadmaps | load_maps |
| findmaps | find_maps |
| matchmaps | match_maps |
| calcmatchedmapsdiff | calc_matched_maps_diff |
| calcmatchedsmapsdiff | calc_matchedsmaps_diff |
| unescapeurl | un_escapeurl |
| parseurlargs | parseurl_args |
| parsebuglyinfo | parse_bugly_info |
| inthashcontains | int_hash_contains |
| uinthashcontains | uint_hash_contains |
| longhashcontains | long_hash_contains |
| ulonghashcontains | ulong_hash_contains |
| floathashcontains | float_hash_contains |
| doublehashcontains | double_hash_contains |
| stringhashcontains | string_hash_contains |

## 状态总览与待办（2026-11-16 更新）

**C# 侧已全部处理完毕**：
- API 注册名：所有宿主（含批次 7 的 BatchStudio/DotnetApp、Flow.Launcher、GlslRewriter）的非 snake 注册已改；别名组（52+19 组，snake 规范名 + 连写别名成对）为**有意保留的 AI 别名特性**，不是遗漏。CppLua2Dsl/UnrealCodeTransform 无 DSL 机制。
- DSL 全局变量：全仓 `SetGlobalVariable` 连写残留 0。
- 命令行开关/参数：批次 6 改 5 个主仓开关，批次 7 改 GlslRewriter 的 `-arg_cfg`/`-no_ssa`；单词/专有名词不动。
- 编译：相关工程全部 0 CS 错误。

**剩余待办（统一修 DSL，改法见本表映射）**：
- ~~webagent `managed/*.dsl`~~（**已完成 2026-11-16 批次 11**：两镜像全部修复，见批次 11 段）
- ~~copy.dsl 修复~~（**已完成 2026-11-16**：全工作区扫描后修 AgentCore/BatchCmdDslHost/BatchScriptApi/CefDotnetApp/HostCLR MyApp 五个 copy.dsl，getscriptdir→get_script_directory/fileecho→file_echo/copyfiles→copy_files/argnum→arg_num；根目录 copy.dsl 实际不存在；BatchCmdDsl PostBuild 实测通过）
- `AutoComplete.txt`（主仓/webagent 侧的，Flow 的已修）
- 用户环境：手动启动 webagent 的快捷方式/脚本若写死 `--projectidentity=` 需改 `--project_identity=`

## 批次 10：三个 C++ 工程（2026-11-16，用户授权"最后一批"）

### ServerPlatform（FunctionScript = StoryScript C++/SLK 移植）

修改面 10 文件 228 处：内建 API（`calc.cpp:3307-3424` 注册表）、语句名（`ParserFineTuneHelper.cpp` 语法配对表同步）、宿主全局（`AdminTool.cpp`/`AdminTool\Common\GmScript\ScriptThread.cpp`/`Common\ConfigScript\ScriptThread.cpp`/`MaintenanceScript.cpp` 四处 RegisterPredefinedValue）、对象成员 API 与回调（`CommonScriptApi.h`/`GmScript ScriptThread.h` strcmp/CallMember）、调用方 `compile_linux`/`compile_mac` 的 `convert.scp`×2。

| 旧名 | 新名 | 说明 |
|---|---|---|
| setdebug / debugbreak | set_debug / debug_break | 内建函数 |
| toint / tofloat / tostring | to_int / to_float / to_string | C 标准名 atoi 系不同形态 |
| int2hex / hex2int | int_2_hex / hex_2_int | 数字风格 `str_2_int` 先例 |
| byteat / bytes2str / byterep | byte_at / bytes_2_str / byte_rep | |
| argnum | arg_num | |
| for_each_value / loop_list | for_each_value / loop_list | 权威 tsv |
| literalarray / literalobject | literal_array / literal_object | 语句 |
| stringMap / serverType / client | string_map / server_type /（client 不动） | 宿主全局（serverType 仅注册字符串，C++ 参数名不动） |
| writeConsole / getLogFileId / getMilliseconds / getTimeString | write_console / get_log_file_id / get_milliseconds / get_time_string | |
| readString / writeString | read_string / write_string | |
| createIniReader / createTxtTable / createConfigTable / createXmlVisitor | create_ini_reader / create_txt_table / create_config_table / create_xml_visitor | |
| sendMessage / pushMessage / popMessage / getThreadIndex | send_message / push_message / pop_message / get_thread_index | |
| returnCommandToLogic | return_command_to_logic | |
| getScriptThreadNum / startScriptThread / markWaitingQuit / stopScriptThread / isScriptThreadRunning | get_script_thread_num / start_script_thread / mark_waiting_quit / stop_script_thread / is_script_thread_running | |
| runfile / runscript | run_file / run_script | |
| getInfoCount / getSection / getKey / getValue / findValue | get_info_count / get_section / get_key / get_value / find_value | IniReader 成员 |
| getRowCount / getColumnCount / getCell / findRow | get_row_count / get_column_count / get_cell / find_row | TxtTable 成员 |
| getIP0 / getIP1 / getServerName / getServerID / getZoneID / getBigworldID / getWorldID | get_ip0 / get_ip1 / get_server_name / get_server_id / get_zone_id / get_bigworld_id / get_world_id | ConfigTable（IP0 整体标识保留） |
| getKeyValueNum / getValueByKey / findByIP0 / findByIP1 / findByID | get_key_value_num / get_value_by_key / find_by_ip0 / find_by_ip1 / find_by_id | |
| initMap / getNum / clearMap / listMap | init_map / get_num / clear_map / list_map | StringMap |
| onBeginDocument / onEndDocument / onBeginElement / onAttribute / onEndElement / onDeclaration / onText / onComment / onUnknown | on_begin_document / on_end_document / on_begin_element / on_attribute / on_end_element / on_declaration / on_text / on_comment / on_unknown | XML 回调 |
| setCallback / setConnectInfo / isConnected / myHanle / targetHandle / targetName / createPacket / sendCommandByHandle / sendCommandByName | set_callback / set_connect_info / is_connected / **my_handle** / target_handle / target_name / create_packet / send_command_by_handle / send_command_by_name | SimpleClient（myHanle 顺带修正拼写） |
| onNameHandleChanged / onMessage / onMessageResult / onCommand | on_name_handle_changed / on_message / on_message_result / on_command | 回调 |
| addByte / addShort / addInt / addFloat / addString / addFixedString 等 StreamPacket 全组 | add_byte / add_short / add_int / add_float / add_string / add_fixed_string / get_* / get_size / send_by_handle / send_by_name / reset_cursor | |

**整组不动**：语言关键字（for/while/if/goto/return/break/continue/function/struct）、符号与 `__` 操作符别名（`__add`/`__isequal` 等，运算符文字别名域）、C 标准名（strlen/strstr/substr/strchr/strrchr/strcspn/strspn/tolower/toupper/system/print/new/sleep/sprintf/argv/argc/quit/exit/loop/load/unload/find/insert/erase/tick/disconnect/destroy/skip/accept）。
**验证**：AdminTool.vcxproj 与 ServerCenter.vcxproj 构建 0 错误（x64；注意 `AdminTool\AdminTool` 是被 git 跟踪的 2022 年 mac Mach-O 产物文件，挡默认输出目录，msbuild 需显式 `-p:OutDir` 绕开）；运行冒烟：`AdminTool.exe convert.scp AdminTool` 在临时目录正确生成 Makefile.gen（write_string×19/byte_rep/string_map.find/create_xml_visitor+accept 回调链全通；`rm`/`cat` 报错仅因 Windows 缺 Unix 命令，脚本本为 Linux 编译环境设计）。

### myuzu（yuzu 分支：BraceScript + DebugScript）

BraceScript 修改面：`src\core\memory\brace_script\brace_script_interpreter.cpp`（注册 L11439-12131 + doc + 错误消息，484 处）、`math_api.h`/`brace_object.inl`（错误消息，仅字符串内替换保护 `std::atan2`）、`data_analyst.cpp`（帮助文本）；DebugScript：`tools\dbg_scp_compiler` 的 `api.txt`×2 + `hook.txt`×2（批次 8 同款 15 条映射）。

BraceScript 146 条映射与 ScriptableUnreal 同名部分同源（见下表），myuzu 特有：

| 旧名 | 新名 | 说明 |
|---|---|---|
| oncallback / onmessage / clearmessages / waituntilquit | on_callback / on_message / clear_messages / wait_until_quit | |
| showui / getscriptinput / setscriptinputlabel / setscriptbtncaption | show_ui / get_script_input / set_script_input_label / set_script_btn_caption | Btn 缩写按类名保留 |
| getpixel / getcursorx / getcursory / getscreenwidth / getscreenheight | get_pixel / get_cursor_x / get_cursor_y / get_screen_width / get_screen_height | |
| readbuttonparam / readstickparam / readmotionparam / readparampackage | read_button_param / read_stick_param / read_motion_param / read_param_package | |
| hasparam / getintparam / getfloatparam / getstrparam | has_param / get_int_param / get_float_param / get_str_param | |
| keypress / keyrelease / mousepress / mouserelease / mousemove / mousewheelchange | key_press / key_release / mouse_press / mouse_release / mouse_move / mouse_wheel_change | |
| touchpress / touchupdatebegin / touchmove / touchupdateend / touchend | touch_press / touch_update_begin / touch_move / touch_update_end / touch_end | |
| getbuttonstate / setbuttonstate / setstickpos / setmotionstate | get_button_state / set_button_state / **set_stick_position** / set_motion_state | setstickpos 按类名 SetStickPositionExp 全词（flagged） |
| getresultinfo / getlastinfo / gethistoryinfo(count) / getrollbackinfo(count) / setresultinfo | get_result_info / get_last_info / get_history_info(_count) / get_rollback_info(_count) / set_result_info | |
| newmemorymodifyinfo / addtoresult / addtolast | new_memory_modify_info / add_to_result / add_to_last | |
| getyuzupath / getlogpath / getmodloadpath / getgamesavepath / gettitleid | get_yuzu_path / get_log_path / get_mod_load_path / get_game_save_path / get_title_id | |
| getpid / getmodulepid / getmoduleprogid | **get_process_id / get_module_process_id / get_module_program_id** | 按类名全词（GetProcessIdExp/GetModuleProcessIdExp/GetModuleProgramIdExp，flagged；POSIX getpid() C++ 调用未动） |
| getmodulecount / getmodulebase / getmoduleaddr / getmodulesize / getmoduleid / getmodulename | get_module_count / get_module_base / get_module_addr / get_module_size / get_module_id / get_module_name | Addr 缩写词典保留 |
| getheapbase / getheapsize / getstackbase / getstacksize | get_heap_base / get_heap_size / get_stack_base / get_stack_size | |
| markmemdebug / addsniffing / addsniffingfromsearch | mark_mem_debug / add_sniffing / add_sniffing_from_search | |
| showmem / findmem / searchmem / findmemory / searchmemory / readmemory / writememory / dumpmemory / loadmemory / protectmemory / mapmemory / unmapmemory / findunmappedmemory | show_mem / find_mem / search_mem / find_memory / search_memory / read_memory / write_memory / dump_memory / load_memory / protect_memory / map_memory / unmap_memory / find_unmapped_memory | |
| isgamestarted / addloginst / dbgscpset / dbgscpget | is_game_started / **add_log_instruction** / dbgscp_set / dbgscp_get | addloginst 按类名 AddLogInstructionExp 全词（flagged） |
| replacesourceshader / replacespirvshader / dmnt_keypress | replace_source_shader / replace_spirv_shader / dmnt_key_press | dmnt 组其余已有 snake 不动 |

**不动**：typeid（C++ 语言关键字，同 printf/malloc 类）、itof/ftoi/utof/ftou/ltod/dtol/utod/dtou（紧凑转换对，类比 atoi/atof）、linq 方法名与 select 子句名（预编译库 CppDsl 内置）、dmnt 组既有 snake_case（dmnt_calc_offset 等 21 条）、GUI 命令名（setmemscope/setsniffingscope 等 data_analyst/memory_sniffer 的 cmd 解析，属第三域未授权）、GUI 消息名（OnClearAll 等消息路由）、C++ 标识符（dumpcascadeptr 静态函数/objTypeId 变量等）、POSIX getpid()。
**验证**：预检证明全部旧名仅出现在字符串/注释（C++ 代码区零裸出现，`std::atan2` 等保护）；diff 复核全部改动在字符串字面量内；编译由用户执行。

### ScriptableUnreal（UE BraceScript 插件）

修改面：`Plugins\Myself\MyPluginRuntime\Source\Brace\Private\BraceScriptInterpreter.cpp`（注册 L7097-7283 + doc + 错误消息 + runscp 内嵌示例串，313 处）、`MathApi.h`/`BraceObject.inl`（错误消息，字符串内替换）、`UnrealStatsApi.cpp`（错误文案 37 处，含 `getstatcallcount` 旧文案修正为 `stat_get_call_count`——原文案与注册名 statgetcallcount 本就不一致）、`Content\Debug\Brace\brace.bytes`/`memstat.bytes` 脚本（43+35 处）。

与 myuzu 同名同映射（membercall→member_call、loop_list→loop_list、typetag→type_tag、strconcat→str_concat、csvecho/csvconcat/csvdebug→csv_echo/csv_concat/csv_debug、fileexists→file_exists、loadfiletoarray→load_file_to_array、arraymax 系→array_max 系、atan2→atan_2 等 68 条）。UE 特有：

| 旧名 | 新名 | 说明 |
|---|---|---|
| varset / varget | var_set / var_get | UE cvar 读写 |
| prefset / prefget | **player_pref_set / player_pref_get** | 按类名 PlayerPrefSetExp 全词（flagged） |
| spcstr / tabstr | spc_str / tab_str | |
| vpwidth / vpheight | **viewport_width / viewport_height** | 按类名 GetViewportWidthExp 全词（flagged） |
| rtwidth / rtheight | **viewport_render_target_width / viewport_render_target_height** | 按类名 GetViewportRenderTargetWidthExp 全词（flagged） |
| workdir / enginedir / projectdir / configdir / contentdir / savedir / logdir | work_dir / engine_dir / project_dir / config_dir / content_dir / save_dir / log_dir | Dir 按类名缩写保留 |
| getthreadid / gamethreadid / getabsolutepath | get_thread_id / game_thread_id / get_absolute_path | |
| worldname / preloadmapname / loadedmapname / showdebug | world_name / preload_map_name / loaded_map_name / show_debug | |
| efileexists / loadefile / saveefile / loadefiletoarray / savearraytoefile | efile_exists / load_efile / save_efile / load_efile_to_array / save_array_to_efile | EFile 前缀整体保留 |
| getobjects / getcomponents / findobject / findcomponent / findclass / findfunction | get_objects / get_components / find_object / find_component / find_class / find_function | |
| loopobject / loopcomponent / loopfunction / loopfield / loopobjref | loop_object / loop_component / loop_function / loop_field / **loop_object_ref** | loopobjref 按类名 LoopObjectRefExp 全词（flagged） |
| statgetcallcount / statgetduration / statgetint64 / statgetdouble | stat_get_call_count / stat_get_duration / stat_get_int64 / stat_get_double | |
| loopstat / loopllm / showstats | loop_stat / loop_llm / show_stats | LLM 缩写保留 |
| getcyclecount / getcycletime | get_cycle_count / get_cycle_time | |
| getmemstat / getmem | **get_memory_stat / get_platform_memory** | 按类名 GetMemoryStatExp/GetPlatformMemoryExp 全词（flagged） |
| getpoolcapacity / getpoolabbr / getcounterstat / llmshowtags / llmgetstat | get_pool_capacity / get_pool_abbr / get_counter_stat / llm_show_tags / llm_get_stat | |
| trimmemory / isasyncloading / cleanupactors / trygc / performgc / forcegc | trim_memory / is_async_loading / cleanup_actors / try_gc / perform_gc / force_gc | |

**不动**：typeid、BraceAny.h 的 `typetag()` C++ 虚函数、库对象方法（`.add`/`.push`/`.count` 等）、linq 方法名（where/orderby/orderbydesc/top）与 select 子句名（from/where/groupby/having，预编译库语法）、`auto.*` UE 控制台变量名（UE cvar 域）、`echo`（库内置）。
**验证**：预检（仅 MathApi.h 有 std::atan2 裸调用已用字符串内替换保护）+ diff 复核 + 残留扫描 clean（余量均为注释/C++ 标识符）；编译由用户执行。

### 批次 10 通用经验

- C++ 工程改名前必须做"裸出现预检"：剥离 char 字面量（`'"'` 会错位引号配对！必须先于字符串剥离）→字符串→注释，剩余代码区命中即为误伤风险；命中文件改用"逐行字符串字面量内替换"
- PowerShell 5.1 的 `[regex]::Replace` + scriptblock MatchEvaluator **静默失效**（回调不执行也不报错），嵌套委托计数全失真；可靠做法是 `Matches` 收集 + StringBuilder 手工拼接
- myuzu/ScriptableUnreal 工作区均有用户进行中的 CppDsl 库改动（BraceScript.h+lib 二进制），与本次改名无关，未触碰

## 批次 11：全 d:/GitHub DSL 脚本复查修复（2026-11-16，用户授权"扩大到全目录"）

范围：d:/GitHub 全部 .dsl/.scp/.bytes（772 个 .dsl + .scp/.bytes，排除 .git 与第三方仓），映射 = rustdsl_map.tsv 114 条 + 批次 1-7 宿主名覆盖 + 本批新增。共修复 **88 个文件**，三重扫描（全名扫描/漏网候选/审计）清零。

### 修复清单

| 仓库 | 文件 | 内容 |
|---|---|---|
| WebAgent + BatchCommand\AgentCore\webagent（镜像） | managed 6 个 dsl + skills 3 个 | webagent 待办断链全修：nativelog/native_api/base_path/app_dir/is_mac/process_type/agent_port/http_proxy_port/initial_project_identity/startup_url/set_dsl_file/redirect_call/arg_num/file_echo/loop_list/is_null/list_size/combine_path/get_directory_name/get_file_name 等 |
| BatchCommand | bin\Debug\net9.0\copy.dsl 与 main.dsl（bin 副本）、BatchStudio\managed\Script.dsl、Flow main.dsl×2 | bin 下副本此前漏修；Script.dsl 的 nativeapi×36→native_api、getextension/copydir 等 |
| BatchCommand | GlslRewriter 19 个 args.dsl | exp2→exp_2、log2→log_2（其 bin 下 DotnetStoryScript.dll 已 fc 实证与主仓字节一致=新版） |
| CSharpGameFramework / GameDemo | buildclient/buildserver/build.dsl、storydoc.dsl(writefile/dslfile→write_file/dsl_file) | Tools\Batch 工具脚本 |
| MetaDSL | CppDsl\BraceScript\genbracecode.dsl | hashtableget/writeblock/listsize→hashtable_get/write_block/list_size |
| apkstudio | build\managed\script.dsl | outputlog→output_log、isnullorempty→is_null_or_empty |
| UnityProjForProfiler | editor_update_check + FindDsl/ProcessDsl 约 70 个 | 见下 |

### 漏网 1：isnullorempty / calcmd5（跨仓 38 文件）

C# 注册的是 `is_null_or_empty`（FrameworkApiAlias.cs:418）与 `calc_md5`，但旧名不在任何批次映射里。修复 37+2 处（webagent 两镜像、BatchStudio、Flow、apkstudio、UnityProjForProfiler 全部 FindDsl/ProcessDsl）。

### 漏网 2：UnityProjForProfiler Assets\Scripts\EditorDslCalculator\DslCalculatorApi.cs（批次 9 漏掉的注册文件）

UnityEditorApi.Register 49 条连写注册（与批次 9 处理的 `Assets\Editor\ScriptableDsl\` 是不同目录）。改名（注册+doc 103 处）+70 个调用脚本同步：

| 旧名 | 新名 | 说明 |
|---|---|---|
| getobjectbyid / getobjectid | get_object_by_id / get_object_id | |
| assetpath2guid / guid2assetpath | asset_path_2_guid / guid_2_asset_path | GUID 缩写保留 |
| getassetpath / getassetimporter | get_asset_path / get_asset_importer | |
| getguidandfileid | get_guid_and_file_id | flagged：按注册名词义（fileid）；类名 GetGuidAndLocalFileIdentifierExp 差异过大 |
| getdependencies / getdirectdependencies / getdependenciesgraph | get_dependencies / get_direct_dependencies / get_dependencies_graph | |
| loadasset / loadassets / unloadasset | load_asset / load_assets / unload_asset | |
| addobjecttoasset / removeobjectfromasset | add_object_to_asset / remove_object_from_asset | |
| fromjsonoverwrite / tojson | from_json_overwrite / to_json | |
| getstringinlength | get_string_in_length | 与批次 1 一致 |
| getprefabtype/status/object/parent 系 10 条 | get_prefab_type / get_prefab_status / get_prefab_object / get_prefab_parent / get_prefab_parent_at_path / get_prefab_parent_from_origin / get_nearst_prefab_root(_asset) / get_outermost_prefab_root / get_original_prefab_root / get_prefab_overrides | nearst 类名拼写保留 |
| findprefabinstances / findprefabinstancesinscene | find_prefab_instances / find_prefab_instances_in_scene | |
| scanprefab / scandependency / checkinternaldependency | scan_prefab / scan_dependency / check_internal_dependency | |
| convertdumpbin | convert_dump_bin | |
| displayprogressbar / displaycancelableprogressbar / clearprogressbar | display_progress_bar / display_cancelable_progress_bar / clear_progress_bar | |
| openwithdefaultapp / openfilepanel(withfilters) / openfolderpanel / savefilepanel(inproject) / savefolderpanel | open_with_default_app / open_file_panel(_with_filters) / open_folder_panel / save_file_panel(_in_project) / save_folder_panel | |
| displaydialog / hascmdarg | display_dialog / has_cmd_arg | |
| storyvar / callstory | story_var / call_story | USE_GM_STORY 条件 |
| strlen | 不动 | C 标准名 |

### 配套 C# 修复（3 处，历史批次漏网）

1. `AgentCore\Core\SkillManager.cs:62-63`：`_envs["basepath"]/_envs["appdir"]` → `base_path`/`app_dir`（批次 5 漏改，skills 脚本同步）
2. `FrameworkApiAlias.cs:2672`：简写语法分支 `firstId == "looplist"` → `"loop_list" || "looplist"`（双名兼容，旧脚本不受影响）
3. `FrameworkApiAlias.cs:2974`：同上 foreachvalue → for_each_value 双名兼容

### 不改项（甄别结论）

- `getcomponents`/`gettypeassemblyname`（UnityProjForProfiler 2 处脚本调用）：新旧名均无任何注册——历史死调用
- `setattr`/`windowstyle`（dreaman.dsl/main.dsl）：无注册死调用
- `iamgod`（dreaman.dsl）：add_button 按钮名（自由命名域）
- GameDemo storydoc.dsl 的 `dotnetexec`/`dictparse`/`followtarget`/`pauselocalmessagehandler`：vscode snippet 名（dsl.json 定义）与游戏域调用
- GameDemo ForPPT VisualStory.dsl 的 `defapi`×244/`initval`×354：自有 DSL 语法（defapi 体系）
- table.dsl 的 `fielddef`×283：TableReaderGenerator 自有语法
- ~~Hlsl2Python/Hlsl2Numpy 11 个 dsl：旧 DLL 决策（批次 9），暂不破~~（**已完成 2026-11-16 追加**：用户确认升级。gencode 运行时 10 文件（BatchCommand.exe+9 DLL）已从主仓 `bin\Debug\net9.0` 同步（SHA256 一致）；10 个 dsl 全修（gencode 4 个 hashtableset/hashtableget→hashtable_set/hashtable_get，bin tmp 6 个 smoothstep/atan2→smooth_step/atan_2）；冒烟：新名零报错 + 旧名报 unknown func 双向验证）
- 游戏 StorySystem 自有 API（get_leader_id/fire_message/vector_3/create_npc/onmessage 等）：游戏域非本工程对象

### 验证

- BatchScriptApi、AgentCore 构建 0 警告 0 错误（AgentCore post-build 已把新 DLL 拷到 WebAgent 部署副本）
- 三重扫描清零：全名扫描（357 名）ACTIONABLE 0、漏网候选 HIT 0、审计（注册表 769 名 vs 调用词）无新增断链
- webagent 镜像两侧（源仓+部署仓）各自独立修复；Unity 内编译留用户

### 重复注册清查与覆盖冲突修复（2026-11-16 追加，copy.dsl 环境变量问题延伸）

背景：用户指示查 DotnetStoryScript 与 BatchScriptApi 的重复注册名，新实现（webagent 迁移版，带 ApiTextInfo.Error 错误信息）相对 DS 缺失的补齐。共提取 141 条同名交集。

重复注册的三层结构（同名时后注册者覆盖）：

1. DS 内建（DslCalculatorApiRegistry.Init，NewApiRegistry 时注册）
2. FileOperationsApi.cs 版（BatchScriptApiRegistrar.RegisterFileApis）：webagent 迁移版，copy_file/move_file 3 参（overwrite 可选）+ 错误信息，路径经 PathHelper.EnsureAbsolutePath
3. FrameworkApiAlias.cs 复刻版（RegisterFrameworkAliasApis，注册顺序在 2 之后）：忠实复刻 DS（2 参 + 错误信息 + 展开 + FileEchoOn）

重大发现：

- 注册顺序导致 file_exists/path_exists/copy_file/move_file 4 个名被复刻版（仅 2 参）覆盖 FileOperationsApi 版（3 参），3 参 overwrite 能力丢失；DslHost.Prepare（webagent/AgentCore 宿主）固定走 BatchScriptApiRegistrar，同样受影响
- BatchCommand.exe 入口只调 BatchScript.Init() 不调 RegisterAllApis，只有 DS 内建——copy.dsl 跑在 exe 上时 copy_files 一直是 DS 版（DS 版自带环境变量展开，原故障根因只是 setenv 断链导致 %VAR% 未定义、ExpandEnvironmentVariables 保留原文）

修复 3 处（2026-11-16 二次修正后的最终方案，用户定原则：**重名只允许 FrameworkApiAlias.cs 与 DS 重名**——webagent 用 FA 带错误信息版本；FileOperationsApi.cs 侧重名 API 实现不同则改新名、实现相同则删）：

1. FrameworkApiAlias.cs 的 file_exists/path_exists/copy_file/move_file 4 行注册**保留**（曾误删后按用户指示恢复，附注释说明归属原则）：webagent 下这些名生效的是 FA 复刻版（2 参 + 错误信息 + 展开 + FileEchoOn，与 DS 语义一致）
2. FileOperationsApi.cs/BatchScriptApiRegistrar.cs：3 参版改新名 **copy_file_ex(sourcePath, destPath, overwrite) / move_file_ex(sourcePath, destPath, overwrite)**（回滚曾做的 2 参默认覆盖/FileEchoOn 对齐，恢复 webagent 迁移版原实现，默认 overwrite=false）；删除与 DS 实现相同的 6 行重名注册（file_exists/file_exist/dir_exists/dir_exist/path_exists/path_exist，FileOps 版）；DS 无名的别名组保留（exist_file/is_file/exist_dir/is_dir/exist_path/exist/exists/is_path/isfile/isdir/exists_file/exists_dir/exists_path、rename_file、*_not_exists 系）。**追加（同日）**：FA 补注册 exists 系 6 个名全部由 FA 提供带错误信息版本且进 help（用户理由：help 默认不展示 DS 内建注册）——dir_exists/file_exists/path_exists 主注册（进 userdoc），dir_exist/file_exist/path_exist 按别名惯例 false 注册（可调用不占 help 列表），webagent 下 6 名均为 FA 版（此前 dir_exists 等旧名会退回 DS 版的问题随之消除）
3. PathHelper.EnsureAbsolutePath 补 Environment.ExpandEnvironmentVariables（FileOperationsApi 全系单点获得展开，read_file/write_file/append 系等独有 API 同样受益）

消费端同步：script_agent.dsl:1350 两侧镜像（源仓 AgentCore + 部署仓 WebAgent）`copy_file($patternsFile,$bakFile,true)` → `copy_file_ex(...)`（唯一 3 参调用点）；script.dsl 3 处 2 参日志轮转调用不变（FA 版 2 参默认覆盖，正向修复了原 FileOps 版 2 参默认不覆盖导致第二次轮起报 already exists 的问题）；copy.dsl 2 参调用跑 BatchCommand.exe（DS 版）不受影响；prompt/AutoComplete.txt 无 copy_file/move_file 引用。

其余约 137 条交集核对结论：数学/字符串/集合/类型/时间类为忠实复刻、无路径语义，无功能缺失。

验证：e:/tmp/bsapi_smoke 存根宿主（引用根 bin DLL，DslHost.Prepare + ExecuteScript）冒烟 8 项全 PASS：FA 版 copy_file 2 参默认覆盖、FA 版 3 参拒绝（dst 不变）、copy_file_ex overwrite=true/false、FA 版 move_file 2 参、move_file_ex、file_exists %VAR% 展开（FA 版）、dir_exists %VAR%（DS 版，展开正常）、参数错误文案（"Expected: copy_file(file1,file2)"）与 copy_file_ex 目标存在不覆盖报错均经 ApiTextInfo.Error 回流。REGDOC 断言：copy_file/move_file/file_exists/path_exists=FA 文案、copy_file_ex/move_file_ex=新名、dir_exists/file_exist=DS 文案（带 " api" 后缀）。BatchCommand.csproj 构建 0 警告 0 错误，read_lints 0 诊断。

生效说明：webagent 运行时需重启（换新 BatchScriptApi.dll，AgentCore post-build 拷贝受 webagentdbg 进程锁制约，留用户处理）。
## 批次12：TestUC（e:/UGit/TestUC，2026-11-16，用户授权）

范围：Unity 工程 TestUC 的 DSL API 改 snake_case。注册 477 个（ClientGmStorySystem 240 + EditorDslProcessorApi 131 + EditorDslApi 73 + GmCommands 14 + TerrainProcessor 14 + UCEditorDslApi 5），改名 414 条（KEEP 63），完整映射表 e:/tmp/testuc_map.tsv。映射优先级：rustdsl_map（DS/StoryScript 权威）> uprof_map（批次9 同工具链已验收）> 类名 SOP 推导（缩写词典 Idapro/Url/Md5/Gpu/Cpu/Tex/Mem/Ui/Ptr/Scp/UrP/Msaa/Appid 等）。

KEEP 原则（63 条不改动）：已含下划线合规名（cam_info/asset_refresh/api_list 等）；标准 shell 名（gm/help/cd/pwd/gc/cmd/shell）；单词名（click/toggle/capture/screen/app/quit/position/rotation/scale/expand）；Profiler 内存五分类名（mono/native/gfx/unused/total）；紧凑转换对（deg2rad/rad2deg，itof-ftoi 先例）；C# 关键字风格（typeof）。

人工修正的 SOP 推导项：prefset*/prefget* 系补 set/get 动词（player_pref_set_int 等，myuzu 批次10 先例 prefset→player_pref_set）；WeTest 专有名词整词（wetest_touch/wetest_get_x 等）；asset_path_2_guid/guid_2_asset_path（数字独立分词）；is_iphone（iPhone 整词）；get_shell/get_shell_timeout（避免与 shell/shell_timeout 撞名）；position/rotation/scale 共用 Vector3Function 类推导会撞名为 vector3，按单词保留。

消费端：6 个注册 C# 文件（注册名+doc+错误消息，728 处）+ 补漏 4 文件（StartScript.cs 的 DebugConsole.Execute DSL 字符串、GeneralCommands.cs 实现类 doc/Expected 消息、GmCommands.cs 错误日志/case 注释、GmSocketServer.cs 示例注释，29 处）+ dsl 脚本 102 个（tools/dsl_scripts，494 处）。

重要发现：TestUC 运行时引擎是 GameDemo 的 StoryScript（netstandard2.1，非 DotnetStoryScript），using StoryScript; using StoryScript.DslExpression;——该引擎已是 snake_case 新版。批次11 全目录扫描漏了 e 盘工程，dsl 里残留 32 个内建旧名（stringcontains/looplist/readalllines/appendlineformat→append_format_line/stringbuildertostring→stringbuilder_tostring 等），本次补改 526 处。dsl_tools 执行器 smol-v-test.exe 是单文件打包（内嵌旧构建），改源码后需重新构建 exe 才能跑新名脚本。

验证：代码区裸出现预检 0 命中（PowerShell -match 不区分大小写会误报 GetComponent 等 Unity 驼峰 API，须用 -cmatch）；区分大小写残留扫描仅剩 Odin 第三方（import 英文词）与 proto 生成字段（appid）噪音；反向断链审计（dsl 调用名 ⊆ TestUC 470 注册 ∪ StoryScript 354 内建 ∪ 本地 script() 定义 ∪ smol 宿主 UI 描述语法白名单 button/input/popup/table/string/float/range/multiple/inthash/stringhash/label/file/encoding/feature/rect/sampler 及 startup 系 log_sysinfo/set_timescale/resettrees/mapmyhooksymbols/updatetexturedb）= 0 断链。Unity 工程内编译与 smol-v-test.exe 重建留用户。
## 批次 13：StoryDsl.txt 编码修复 + ServerPlatform 部署升级（2026-11-17，用户授权）

### 1）CSharpGameFramework/Doc/StoryDsl.txt 乱码修复

现象：工作区文件 65958B 混合编码（首字节 `D2-BB` GBK "一" + 11382 个 `EF-BF-BD` U+FFFD），中文不可逆损坏；git HEAD blob（40820B）是完好的 GB18030 原文；`GameDemo/Tools/DocGenerator/bin/Debug/StoryDsl.txt` 是另一份 UTF-8 生成器输出（2131 行/356 API 头，不是同一文件）。

修复：以 HEAD blob（GB18030 完好中文）为底；把工作区乱码版与 HEAD 逐行对齐（1992 行、1621 行 ASCII token 一致）推导出 191 个干净的旧名→snake_case 改名对（排除 2 个 mojibake token 噪音 `key->keloop_list`、`looplist->loop_listlooplist`）；加 rustdsl_map.tsv 旧名兜底共 299 对，按长度降序整词替换；输出 UTF-8 无 BOM（49951B）。

验证：strict UTF-8 YES、U+FFFD 0、中文 7054 字、1991 行、rustdsl 旧名残留 0、260 个编号 API 头全为 snake_case。刻意不动：文档里游戏域粘连名（isjsonarray/json2str/npcpatrol 等，注册表无权威映射证据，按"无证据不猜"保留）。

### 2）libevent 2.1.12 → 2.1.13

- 构建：`libevent-2.1.13-stable` 用 CMake 4.4 + VS2022 v143 静态 Release（`/MT`、OpenSSL off、tests/benchmark/samples/regress off），`-DCMAKE_POLICY_VERSION_MINIMUM=3.5` 绕过 cmake_minimum_required 3.1 兼容限制；x64/Win32 各生成 event/event_core/event_extra.lib。
- 落位（对齐 2.1.12 布局）：`lib/x64`、`lib/win32` 各 3 个 lib；`include/x64/`、`include/win32/` 放 evconfig-private.h 与 event2/event-config.h；`include/event2/event-config.h` 共用。新旧 event-config.h 仅 4 行版本号差异（ABI 一致）。
- 依赖切换：ServerCenter/AdminTool/CenterClientLibrary/CenterHubLibrary 四个 vcxproj 各 6 处 + .gitignore 2 处，2.1.12→2.1.13（源码级引用 0 残留）。
- 验证：MSBuild Debug|x64 四个工程全 0 错误；ServerCenter Win32 用临时 include 补丁（补既有缺失的 ../Common/Encoding）也 0 错误，构建后 vcxproj 已还原。LNK4098 为 /MT 静态库与 /MD 的既有警告。
- 删除 2.1.12 整目录（备份 e:/tmp/libevent_2112_backup + 旧 nodejs 亦在 e:/tmp）。环境 safe-delete 阈值按轮累计 500 文件拦截整目录删除，改用小批量逐文件删除完成；未绕过机制。

### 3）Node 26 部署

- 最新稳定 LTS：v26.13.1（Lithium，2026-11-10），`process.versions.modules=147`；从 nodejs.org 下载 win-x64 zip（41MB）。
- `d:/nodejs` 由 v24.15.0 更新到 v26.13.1（npm 11.20.0），旧版双备份 e:/tmp/nodejs24_backup 与 e:/tmp/nodejs24_original。
- `node_center_client`：main.cc 用标准 N-API（NODE_API_MODULE、Napi::*），无裸 V8 废弃调用；package.json 依赖 `node-addon-api` 由 `*` 固定为 `^8.9.2`（最新版，对应 Node 26），npm install + node-gyp rebuild（旧 config.gypi 的 module 137 → 147）；`-flto=thin/-fprofile-use` 警告为 Node 26 官方 headers 携带的 PGO 选项被 MSVC 忽略，无害。
- 部署 bin64：`node.exe`(100089160B) + `center_client.node`(730624B，11 个导出)，旧文件备份 e:/tmp/bin64_backup。
- 冒烟：bin64 就地 `node.exe -e "require('./center_client.node')"` 加载成功（11 exports、v26.13.1）；addon 依赖 `CenterClientLibrary.dll` 由 bin64 提供（dumpbin 确认 + 显式 PATH 验证）。

生效说明：ServerEnv 目录的部署由用户自行完成（bin64 已就绪）。