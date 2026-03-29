using AgentPlugin.Abstractions;
using System;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.ScriptApi;
using AgentCore.ScriptApi;

namespace CefDotnetApp.AgentCore
{
    /// <summary>
    /// Registers all Script APIs to the DSL script engine
    /// </summary>
    public static class ScriptApiRegistrar
    {
        /// <summary>
        /// Register all agent-related Script APIs
        /// </summary>
        public static void RegisterAllApis()
        {
            // File Operations
            AgentFrameworkService.Instance.DslEngine!.Register("read_file", "read_file(path)", new ExpressionFactoryHelper<ReadFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("write_file", "write_file(path, content)", new ExpressionFactoryHelper<WriteFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("append_file", "append_file(path, content)", new ExpressionFactoryHelper<AppendFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("copy_file", "copy_file(sourcePath, destPath, overwrite)", new ExpressionFactoryHelper<CopyFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("move_file", "move_file(sourcePath, destPath, overwrite)", new ExpressionFactoryHelper<MoveFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_exists", "file_exists(path)", new ExpressionFactoryHelper<FileExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_exist", "file_exist(path)", false, new ExpressionFactoryHelper<FileExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("dir_exists", "dir_exists(path)", new ExpressionFactoryHelper<DirExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("dir_exist", "dir_exist(path)", false, new ExpressionFactoryHelper<DirExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_dir_info", "list_dir_info(path, glob_pattern, recursive) return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<ListDirInfoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_size", "get_file_size(path)", new ExpressionFactoryHelper<GetFileSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_size", "file_size(path)", false, new ExpressionFactoryHelper<GetFileSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_log_file", "search_log_file(log_file, search_regex[, context_lines_after, context_lines_before])", new ExpressionFactoryHelper<SearchLogFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grep_log_file", "grep_log_file(log_file, search_regex[, context_lines_after, context_lines_before])", new ExpressionFactoryHelper<SearchLogFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("tail_log_file", "tail_log_file(log_file, lines)", new ExpressionFactoryHelper<TailLogFileExp>());

            // Code Editing Operations
            AgentFrameworkService.Instance.DslEngine!.Register("replace_in_file", "replace_in_file(path, oldString, newString[, replaceAll[, exactMatch]])", new ExpressionFactoryHelper<ReplaceInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("multi_replace", "multi_replace(path, editsJson) editsJson=[{\"old_string\":\"...\",\"new_string\":\"...\",\"replace_all\":false,\"exact_match\":false},...]", new ExpressionFactoryHelper<MultiReplaceExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("replace_range", "replace_range(path, startLine, endLine, newContent)", new ExpressionFactoryHelper<ReplaceRangeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("insert_after_text", "insert_after_text(path, searchLiteralText, content[, allOccurrences[, exactMatch]])", new ExpressionFactoryHelper<InsertAfterTextExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("insert_before_text", "insert_before_text(path, searchLiteralText, content[, allOccurrences[, exactMatch]])", new ExpressionFactoryHelper<InsertBeforeTextExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("insert_after", "insert_after(path, line, insert_content) - insert content after specified line number", new ExpressionFactoryHelper<InsertAfterLineExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("insert_before", "insert_before(path, line, insert_content) - insert content before specified line number", new ExpressionFactoryHelper<InsertBeforeLineExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("insert_after_line", "insert_after_line(path, line, insert_content) - insert content after specified line number", new ExpressionFactoryHelper<InsertAfterLineExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("insert_before_line", "insert_before_line(path, line, insert_content) - insert content before specified line number", new ExpressionFactoryHelper<InsertBeforeLineExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("delete_lines", "delete_lines(path, startLine, endLine)", new ExpressionFactoryHelper<DeleteLinesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_lines", "search_lines(path, regex_pattern, [ignoreCase]) return List, use 'to_string' to convert to a string. auto fallback to substring if regex invalid", new ExpressionFactoryHelper<SearchLinesInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_lines_in_file", "search_lines_in_file(path, regex_pattern, [ignoreCase]) return List, use 'to_string' to convert to a string. auto fallback to substring if regex invalid", false, new ExpressionFactoryHelper<SearchLinesInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("read_lines", "read_lines(path, startLine, endLine) return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<ReadLinesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("read_file_lines", "read_file_lines(path, startLine, endLine) return List, use 'to_string' to convert to a string", false, new ExpressionFactoryHelper<ReadLinesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_line_count", "get_line_count(path)", new ExpressionFactoryHelper<GetLineCountExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_line_count", "get_file_line_count(path)", false, new ExpressionFactoryHelper<GetLineCountExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("line_count", "line_count(path)", new ExpressionFactoryHelper<GetLineCountExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_line_count", "file_line_count(path)", false, new ExpressionFactoryHelper<GetLineCountExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_in_file", "search_in_file(path, regex_pattern[, context_lines_after, context_lines_before])", new ExpressionFactoryHelper<SearchInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grep_file", "grep_file(path, regex_pattern[, context_lines_after, context_lines_before])", new ExpressionFactoryHelper<SearchInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grepfile", "grepfile(path, regex_pattern[, context_lines_after, context_lines_before])", false, new ExpressionFactoryHelper<SearchInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_in_files", "search_in_files(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...])", new ExpressionFactoryHelper<SearchInFilesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grep_files", "grep_files(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...])", new ExpressionFactoryHelper<SearchInFilesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grepfiles", "grepfiles(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...])", false, new ExpressionFactoryHelper<SearchInFilesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("count_lines", "count_lines(path[, startLine, endLine]) - display lines with line number, indent info, and content", new ExpressionFactoryHelper<CountLinesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("countlines", "countlines(path[, startLine, endLine]) - display lines with line number, indent info, and content", false, new ExpressionFactoryHelper<CountLinesExp>());

            // Diff Operations
            AgentFrameworkService.Instance.DslEngine!.Register("apply_unified_diff", "apply_unified_diff(targetPath, diffPathOrContent[, isContent[, exactMatch]]) return Object(success/error/linesAdded/linesRemoved), use 'to_string' to convert to a string", false, new ExpressionFactoryHelper<ApplyDiffExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("apply_diff", "apply_diff(targetPath, diffPathOrContent[, isContent[, exactMatch]]) return Object(success/error/linesAdded/linesRemoved), use 'to_string' to convert to a string", new ExpressionFactoryHelper<ApplyDiffExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("applydiff", "applydiff(targetPath, diffPathOrContent[, isContent[, exactMatch]]) return Object(success/error/linesAdded/linesRemoved), use 'to_string' to convert to a string", false, new ExpressionFactoryHelper<ApplyDiffExp>());

            // Clipboard Operations
            AgentFrameworkService.Instance.DslEngine!.Register("get_clipboard", "get_clipboard()", new ExpressionFactoryHelper<GetClipboardExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_clipboard", "set_clipboard(text)", new ExpressionFactoryHelper<SetClipboardExp>());

            // Logging Operations
            AgentFrameworkService.Instance.DslEngine!.Register("log_info", "log_info(fmt, ...)", new ExpressionFactoryHelper<LogInfoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("log_error", "log_error(fmt, ...)", new ExpressionFactoryHelper<LogErrorExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("log_warning", "log_warning(fmt, ...)", new ExpressionFactoryHelper<LogWarningExp>());

            // HTTP Operations
            AgentFrameworkService.Instance.DslEngine!.Register("fetch", "fetch(url)", new ExpressionFactoryHelper<HttpGetExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("http_get", "http_get(url)", new ExpressionFactoryHelper<HttpGetExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("http_post", "http_post(url, content, contentType)", new ExpressionFactoryHelper<HttpPostExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("download_file", "download_file(url, savePath)", new ExpressionFactoryHelper<DownloadFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_http_user_agent", "set_http_user_agent(user_agent) - set User-Agent header for http_get/http_post/download_file", new ExpressionFactoryHelper<SetHttpUserAgentExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_http_user_agent", "get_http_user_agent() - get current User-Agent header string", new ExpressionFactoryHelper<GetHttpUserAgentExp>());

            // JSON Operations
            AgentFrameworkService.Instance.DslEngine!.Register("to_json", "to_json(obj, prettyPrint)", new ExpressionFactoryHelper<ToJsonExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("from_json", "from_json(json)", new ExpressionFactoryHelper<FromJsonExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("new_object", "new_object(key1, value1, key2, value2, ...)", new ExpressionFactoryHelper<NewObjectExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("to_string", "to_string(val)", new ExpressionFactoryHelper<ToStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_length", "string_length(str)", new ExpressionFactoryHelper<StringLengthExp>());

            // Process Operations
            AgentFrameworkService.Instance.DslEngine!.Register("execute_script", "execute_script([language, workingDir, timeout_def_30000ms, cmd_and_args]) return Object(success/exitCode/output/error/executionTime)[bindings($a,$b,...)delimiter(begin_chars,end_chars)]{: script_code :};, use 'to_string' to convert to a string", new ExpressionFactoryHelper<ExecuteScriptExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("execute_script_async", "execute_script_async(callbackTag[, language, workingDir, timeout_def_30000ms, cmd_and_args]) return Object(success/exitCode/output/error/executionTime)[bindings($a,$b,...)delimiter(begin_chars,end_chars)]{: script_code :}; - async exec, result via command_callback", new ExpressionFactoryHelper<ExecuteScriptAsyncExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("execute_command", "execute_command(command[, arguments, workingDir, timeout_def_30000ms]) return Object(success/exitCode/output/error/executionTime)[bindings($a,$b,...)delimiter(begin_chars,end_chars)], use 'to_string' to convert to a string", new ExpressionFactoryHelper<ExecuteCommandExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("execute_command_async", "execute_command_async(callbackTag, command[, arguments, workingDir, timeout_def_30000ms])[bindings($a,$b,...)delimiter(begin_chars,end_chars)] - async exec, result via command_callback", new ExpressionFactoryHelper<ExecuteCommandAsyncExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("start_process", "start_process(processId, command[, arguments, workingDir])[params($a,$b,...)delimiter(begin_chars,end_chars)]", new ExpressionFactoryHelper<StartProcessExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stop_process", "stop_process(processId[, timeout_def_5000ms])", new ExpressionFactoryHelper<StopProcessExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("is_process_running", "is_process_running(processId)", new ExpressionFactoryHelper<IsProcessRunningExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("write_process_input", "write_process_input(processId, input)", new ExpressionFactoryHelper<WriteProcessInputExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("read_process_output", "read_process_output(processId)", new ExpressionFactoryHelper<ReadProcessOutputExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("read_process_error", "read_process_error(processId)", new ExpressionFactoryHelper<ReadProcessErrorExp>());

            // DSL Context Management Operations
            AgentFrameworkService.Instance.DslEngine!.Register("set_context_var", "set_context_var(key, value[, scope])", new ExpressionFactoryHelper<SetContextVarExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_context_var", "get_context_var(key[, scope])", new ExpressionFactoryHelper<GetContextVarExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("remove_context_var", "remove_context_var(key[, scope])", new ExpressionFactoryHelper<RemoveContextVarExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("clear_context_vars", "clear_context_vars([scope])", new ExpressionFactoryHelper<ClearContextVarsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("clear_all_context_var", "clear_all_context_var()", new ExpressionFactoryHelper<ClearAllContextVarsExp>());

            // Browser Interaction Operations
            AgentFrameworkService.Instance.DslEngine!.Register("build_query_selector", "build_query_selector(selector)", new ExpressionFactoryHelper<BuildQuerySelectorExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("build_click_element", "build_click_element(selector)", new ExpressionFactoryHelper<BuildClickElementExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("build_set_value", "build_set_value(selector, value)", new ExpressionFactoryHelper<BuildSetValueExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("build_get_value", "build_get_value(selector)", new ExpressionFactoryHelper<BuildGetValueExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("build_get_text", "build_get_text(selector)", new ExpressionFactoryHelper<BuildGetTextExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("build_set_innerhtml", "build_set_innerhtml(selector, html)", new ExpressionFactoryHelper<BuildSetInnerHTMLExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("build_wait_for_element", "build_wait_for_element(selector[, timeout_def_5000ms])", new ExpressionFactoryHelper<BuildWaitForElementExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("build_scroll_to_element", "build_scroll_to_element(selector)", new ExpressionFactoryHelper<BuildScrollToElementExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("build_is_visible", "build_is_visible(selector)", new ExpressionFactoryHelper<BuildIsVisibleExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("build_add_class", "build_add_class(selector, className)", new ExpressionFactoryHelper<BuildAddClassExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("build_remove_class", "build_remove_class(selector, className)", new ExpressionFactoryHelper<BuildRemoveClassExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("build_set_style", "build_set_style(selector, property, value)", new ExpressionFactoryHelper<BuildSetStyleExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("build_inject_css", "build_inject_css(css)", new ExpressionFactoryHelper<BuildInjectCSSExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("build_navigate_to", "build_navigate_to(url)", new ExpressionFactoryHelper<BuildNavigateToExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("send_js_code", "send_js_code(jscode)", new ExpressionFactoryHelper<SendJsCodeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("send_js_call", "send_js_call(jsfunc, arg1, arg2, ...)", new ExpressionFactoryHelper<SendJsCallExp>());

            // Agent Command Operations
            AgentFrameworkService.Instance.DslEngine!.Register("parse_agent_command", "parse_agent_command(jsonData)", new ExpressionFactoryHelper<ParseAgentCommandExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("parse_agent_notification", "parse_agent_notification(jsonData)", new ExpressionFactoryHelper<ParseAgentNotificationExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_message_param", "get_message_param(paramsObj, key)", new ExpressionFactoryHelper<GetMessageParamExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("send_command_to_inject", "send_command_to_inject(command, paramsJson)", new ExpressionFactoryHelper<SendCommandToInjectExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("build_agent_response", "build_agent_response(messageId, success, data, error)", new ExpressionFactoryHelper<BuildAgentResponseExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("send_response_to_inject", "send_response_to_inject(responseJson)", new ExpressionFactoryHelper<SendResponseToInjectExp>());

            AgentFrameworkService.Instance.DslEngine!.Register("hot_reload", "hot_reload()", new ExpressionFactoryHelper<HotReloadExp>());

            AgentFrameworkService.Instance.DslEngine!.Register("call_skill", "call_skill(skill_name, arg1, arg2, ...)", new ExpressionFactoryHelper<CallSkillExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("refresh_skills", "refresh_skills()", new ExpressionFactoryHelper<RefreshSkillsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("refresh_embedding", "refresh_embedding()", new ExpressionFactoryHelper<RefreshEmbeddingExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("refresh_reranker", "refresh_reranker()", new ExpressionFactoryHelper<RefreshRerankExp>());

            // Skill Environment Operations
            AgentFrameworkService.Instance.DslEngine!.Register("set_skill_env", "set_skill_env(key, value) - set skill environment variable", new ExpressionFactoryHelper<SetSkillEnvExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_skill_env", "get_skill_env(key[, defval]) - get skill environment variable", new ExpressionFactoryHelper<GetSkillEnvExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("delete_skill_env", "delete_skill_env(key) - delete skill environment variable", new ExpressionFactoryHelper<DeleteSkillEnvExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("clear_skill_envs", "clear_skill_envs([regexPattern]) - clear skill environment variables", new ExpressionFactoryHelper<ClearSkillEnvsExp>());

            AgentFrameworkService.Instance.DslEngine!.Register("append", "append(stringbuilder, val)", new ExpressionFactoryHelper<AppendExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("append_line", "append_line(stringbuilder, val)", new ExpressionFactoryHelper<AppendLineExp>());

            AgentFrameworkService.Instance.DslEngine!.Register("string_index_of", "string_index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_last_index_of", "string_last_index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_index_of_any", "string_index_of_any(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_last_index_of_any", "string_last_index_of_any(str, substr[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_concat", "string_concat(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("concat_string", "concat_string(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("char_to_int", "char_to_int(char_str)", new ExpressionFactoryHelper<CharToIntExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("int_to_char", "int_to_char(int_ascii)", new ExpressionFactoryHelper<IntToCharExp>());

            // alias for LLM Imaginary
            AgentFrameworkService.Instance.DslEngine!.Register("string_find", "string_find(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stringfind", "stringfind(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("index_of", "index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("last_index_of", "last_index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("index_of_any", "index_of_any(str, char_list[, start, count])", new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("last_index_of_any", "last_index_of_any(str, char_list[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("indexof", "indexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("lastindexof", "lastindexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("indexofany", "indexofany(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("lastindexofany", "lastindexofany(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_indexof", "string_indexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_last_indexof", "string_last_indexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_indexof_any", "string_indexof_any(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_last_indexof_any", "string_last_indexof_any(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stringindexof", "stringindexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stringlastindexof", "stringlastindexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stringindexofany", "stringindexofany(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stringlastindexofany", "stringlastindexofany(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stringlength", "stringlength(val)", false, new ExpressionFactoryHelper<StringLengthExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("length", "length(str) or length(list) or length(dict)", new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("len", "len(str) or len(list) or len(dict)", new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("size", "size(str) or size(list) or size(dict)", new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("count", "count(str) or count(list) or count(dict)", new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_concat", "string_concat(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_append", "string_append(str1, str2, ...)", false, new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("concat", "concat(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("strcat", "strcat(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("strlen", "strlen(val)", new ExpressionFactoryHelper<StringLengthExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("mkdir", "mkdir(path)", new ExpressionFactoryHelper<FrameworkApiAlias.CreateDirectoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hotreload", "hotreload()", false, new ExpressionFactoryHelper<HotReloadExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("callskill", "callskill(skill_name, arg1, arg2, ...)", false, new ExpressionFactoryHelper<CallSkillExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("refreshskills", "refreshskills()", false, new ExpressionFactoryHelper<RefreshSkillsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("refreshembedding", "refreshembedding()", false, new ExpressionFactoryHelper<RefreshEmbeddingExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("refreshreranker", "refreshreranker()", false, new ExpressionFactoryHelper<RefreshRerankExp>());

            // Unified Multi-Language Code Analysis Operations
            UnifiedCodeAnalysisScriptApi.RegisterApis();

            // WebSocket API
            WebSocketApi.RegisterApis();

            // TreeSitter API Explorer
            TreeSitterExplorerApi.RegisterApis();

            FrameworkApiAlias.RegisterApis();

            // Regular Expression API
            RegexApi.RegisterApis();

            // Semantic Index API
            SemanticApi.RegisterApis();

            // Segment / Tokenize API
            SegmentApi.RegisterApis();

            // LLM Client API (OpenAI / Claude / AutoMetaDSL / Ollama)
            LlmApi.RegisterApis();

            // MCP Client API
            McpApi.RegisterApis();

            // Agent State API
            AgentStateApi.RegisterApis();

            // Web Search API (Brave + SearXNG)
            WebSearchApi.RegisterApis();

            // Cross-Platform File Search API
            FindFileApi.RegisterApis();

            // Everything Search API (Windows advanced features)
            EverythingApi.RegisterApis();
        }
    }
}

