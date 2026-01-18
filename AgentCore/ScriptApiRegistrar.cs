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
            BatchCommand.BatchScript.Register("read_file", "read_file(path)", new ExpressionFactoryHelper<ReadFileExp>());
            BatchCommand.BatchScript.Register("write_file", "write_file(path, content)", new ExpressionFactoryHelper<WriteFileExp>());
            BatchCommand.BatchScript.Register("append_file", "append_file(path, content)", new ExpressionFactoryHelper<AppendFileExp>());
            BatchCommand.BatchScript.Register("copy_file", "copy_file(sourcePath, destPath, overwrite)", new ExpressionFactoryHelper<CopyFileExp>());
            BatchCommand.BatchScript.Register("move_file", "move_file(sourcePath, destPath, overwrite)", new ExpressionFactoryHelper<MoveFileExp>());
            BatchCommand.BatchScript.Register("file_exists", "file_exists(path)", new ExpressionFactoryHelper<FileExistsExp>());
            BatchCommand.BatchScript.Register("dir_exists", "dir_exists(path)", new ExpressionFactoryHelper<DirExistsExp>());
            BatchCommand.BatchScript.Register("list_dir_info", "list_dir_info(path, glob_pattern, recursive)", new ExpressionFactoryHelper<ListDirInfoExp>());
            BatchCommand.BatchScript.Register("get_file_size", "get_file_size(path)", new ExpressionFactoryHelper<GetFileSizeExp>());
            BatchCommand.BatchScript.Register("file_size", "file_size(path)", new ExpressionFactoryHelper<GetFileSizeExp>());
            BatchCommand.BatchScript.Register("search_log_file", "search_log_file(log_file, search_regex[, context_lines_after, context_lines_before])", new ExpressionFactoryHelper<SearchLogFileExp>());
            BatchCommand.BatchScript.Register("grep_log_file", "grep_log_file(log_file, search_regex[, context_lines_after, context_lines_before])", new ExpressionFactoryHelper<SearchLogFileExp>());
            BatchCommand.BatchScript.Register("tail_log_file", "tail_log_file(log_file, lines)", new ExpressionFactoryHelper<TailLogFileExp>());

            // Code Editing Operations
            BatchCommand.BatchScript.Register("replace_file", "replace_file(path, oldString, newString, allOccurrences)", new ExpressionFactoryHelper<ReplaceFileExp>());
            BatchCommand.BatchScript.Register("replace_range", "replace_range(path, startLine, endLine, newContent)", new ExpressionFactoryHelper<ReplaceRangeExp>());
            BatchCommand.BatchScript.Register("replace_file_range", "replace_file_range(path, startLine, endLine, newContent)", new ExpressionFactoryHelper<ReplaceRangeExp>());
            BatchCommand.BatchScript.Register("insert_after", "insert_after(path, searchLiteralText, content, allOccurrences)", new ExpressionFactoryHelper<InsertAfterExp>());
            BatchCommand.BatchScript.Register("insert_file_after", "insert_file_after(path, searchLiteralText, content, allOccurrences)", new ExpressionFactoryHelper<InsertAfterExp>());
            BatchCommand.BatchScript.Register("insert_before", "insert_before(path, searchLiteralText, content, allOccurrences)", new ExpressionFactoryHelper<InsertBeforeExp>());
            BatchCommand.BatchScript.Register("insert_file_before", "insert_file_before(path, searchLiteralText, content, allOccurrences)", new ExpressionFactoryHelper<InsertBeforeExp>());
            BatchCommand.BatchScript.Register("delete_lines", "delete_lines(path, startLine, endLine)", new ExpressionFactoryHelper<DeleteLinesExp>());
            BatchCommand.BatchScript.Register("delete_file_lines", "delete_file_lines(path, startLine, endLine)", new ExpressionFactoryHelper<DeleteLinesExp>());
            BatchCommand.BatchScript.Register("search_lines", "search_lines(path, regex_pattern, [ignoreCase]) - auto fallback to substring if regex invalid", new ExpressionFactoryHelper<SearchLinesInFileExp>());
            BatchCommand.BatchScript.Register("search_lines_in_file", "search_lines_in_file(path, regex_pattern, [ignoreCase]) - auto fallback to substring if regex invalid", new ExpressionFactoryHelper<SearchLinesInFileExp>());
            BatchCommand.BatchScript.Register("read_lines", "read_lines(path, startLine, endLine)", new ExpressionFactoryHelper<ReadLinesExp>());
            BatchCommand.BatchScript.Register("read_file_lines", "read_file_lines(path, startLine, endLine)", new ExpressionFactoryHelper<ReadLinesExp>());
            BatchCommand.BatchScript.Register("get_line_count", "get_line_count(path)", new ExpressionFactoryHelper<GetLineCountExp>());
            BatchCommand.BatchScript.Register("get_file_line_count", "get_file_line_count(path)", new ExpressionFactoryHelper<GetLineCountExp>());
            BatchCommand.BatchScript.Register("line_count", "line_count(path)", new ExpressionFactoryHelper<GetLineCountExp>());
            BatchCommand.BatchScript.Register("file_line_count", "file_line_count(path)", new ExpressionFactoryHelper<GetLineCountExp>());
            BatchCommand.BatchScript.Register("search_file", "search_file(path, regex_pattern[, context_lines_after, context_lines_before])", new ExpressionFactoryHelper<SearchFileExp>());
            BatchCommand.BatchScript.Register("grep_file", "grep_file(path, regex_pattern[, context_lines_after, context_lines_before])", new ExpressionFactoryHelper<SearchFileExp>());

            // Diff Operations
            BatchCommand.BatchScript.Register("apply_unified_diff", "apply_unified_diff(targetPath, diffPathOrContent[, isContent])", new ExpressionFactoryHelper<ApplyDiffExp>());
            BatchCommand.BatchScript.Register("apply_diff", "apply_diff(targetPath, diffPathOrContent[, isContent])", new ExpressionFactoryHelper<ApplyDiffExp>());
            BatchCommand.BatchScript.Register("apply_unified_diff_full", "apply_unified_diff_full(targetPath, diffPath)", new ExpressionFactoryHelper<ApplyDiffFullExp>());
            BatchCommand.BatchScript.Register("apply_diff_full", "apply_diff_full(targetPath, diffPath)", new ExpressionFactoryHelper<ApplyDiffFullExp>());
            BatchCommand.BatchScript.Register("apply_unified_diff_libgit2", "apply_unified_diff_libgit2(targetPath, diffContent, createBackup)", new ExpressionFactoryHelper<ApplyDiffLibGit2Exp>());
            BatchCommand.BatchScript.Register("apply_diff_libgit2", "apply_diff_libgit2(targetPath, diffContent, createBackup)", new ExpressionFactoryHelper<ApplyDiffLibGit2Exp>());

            // Clipboard Operations
            BatchCommand.BatchScript.Register("get_clipboard", "get_clipboard()", new ExpressionFactoryHelper<GetClipboardExp>());
            BatchCommand.BatchScript.Register("set_clipboard", "set_clipboard(text)", new ExpressionFactoryHelper<SetClipboardExp>());

            // Logging Operations
            BatchCommand.BatchScript.Register("log_info", "log_info(fmt, ...)", new ExpressionFactoryHelper<LogInfoExp>());
            BatchCommand.BatchScript.Register("log_error", "log_error(fmt, ...)", new ExpressionFactoryHelper<LogErrorExp>());
            BatchCommand.BatchScript.Register("log_warning", "log_warning(fmt, ...)", new ExpressionFactoryHelper<LogWarningExp>());

            // HTTP Operations
            BatchCommand.BatchScript.Register("http_get", "http_get(url)", new ExpressionFactoryHelper<HttpGetExp>());
            BatchCommand.BatchScript.Register("http_post", "http_post(url, content, contentType)", new ExpressionFactoryHelper<HttpPostExp>());
            BatchCommand.BatchScript.Register("download_file", "download_file(url, savePath)", new ExpressionFactoryHelper<DownloadFileExp>());

            // JSON Operations
            BatchCommand.BatchScript.Register("to_json", "to_json(obj, prettyPrint)", new ExpressionFactoryHelper<ToJsonExp>());
            BatchCommand.BatchScript.Register("from_json", "from_json(json)", new ExpressionFactoryHelper<FromJsonExp>());
            BatchCommand.BatchScript.Register("new_object", "new_object(key1, value1, key2, value2, ...)", new ExpressionFactoryHelper<NewObjectExp>());
            BatchCommand.BatchScript.Register("to_string", "to_string(val)", new ExpressionFactoryHelper<ToStringExp>());
            BatchCommand.BatchScript.Register("string_length", "string_length(val)", new ExpressionFactoryHelper<StringLengthExp>());

            // Process Operations
            BatchCommand.BatchScript.Register("execute_command", "execute_command(command, arguments, workingDir, timeout)", new ExpressionFactoryHelper<ExecuteCommandExp>());
            BatchCommand.BatchScript.Register("start_process", "start_process(processId, command, arguments, workingDir)", new ExpressionFactoryHelper<StartProcessExp>());
            BatchCommand.BatchScript.Register("stop_process", "stop_process(processId, timeout)", new ExpressionFactoryHelper<StopProcessExp>());
            BatchCommand.BatchScript.Register("is_process_running", "is_process_running(processId)", new ExpressionFactoryHelper<IsProcessRunningExp>());
            BatchCommand.BatchScript.Register("write_process_input", "write_process_input(processId, input)", new ExpressionFactoryHelper<WriteProcessInputExp>());
            BatchCommand.BatchScript.Register("read_process_output", "read_process_output(processId)", new ExpressionFactoryHelper<ReadProcessOutputExp>());
            BatchCommand.BatchScript.Register("read_process_error", "read_process_error(processId)", new ExpressionFactoryHelper<ReadProcessErrorExp>());

            // Context Management Operations
            BatchCommand.BatchScript.Register("set_context_var", "set_context_var(key, value, scope)", new ExpressionFactoryHelper<SetContextVarExp>());
            BatchCommand.BatchScript.Register("get_context_var", "get_context_var(key, scope)", new ExpressionFactoryHelper<GetContextVarExp>());

            // Browser Interaction Operations
            BatchCommand.BatchScript.Register("build_query_selector", "build_query_selector(selector)", new ExpressionFactoryHelper<BuildQuerySelectorExp>());
            BatchCommand.BatchScript.Register("build_click_element", "build_click_element(selector)", new ExpressionFactoryHelper<BuildClickElementExp>());
            BatchCommand.BatchScript.Register("build_set_value", "build_set_value(selector, value)", new ExpressionFactoryHelper<BuildSetValueExp>());
            BatchCommand.BatchScript.Register("build_get_value", "build_get_value(selector)", new ExpressionFactoryHelper<BuildGetValueExp>());
            BatchCommand.BatchScript.Register("build_get_text", "build_get_text(selector)", new ExpressionFactoryHelper<BuildGetTextExp>());
            BatchCommand.BatchScript.Register("build_set_innerhtml", "build_set_innerhtml(selector, html)", new ExpressionFactoryHelper<BuildSetInnerHTMLExp>());
            BatchCommand.BatchScript.Register("build_wait_for_element", "build_wait_for_element(selector, timeout)", new ExpressionFactoryHelper<BuildWaitForElementExp>());
            BatchCommand.BatchScript.Register("build_scroll_to_element", "build_scroll_to_element(selector)", new ExpressionFactoryHelper<BuildScrollToElementExp>());
            BatchCommand.BatchScript.Register("build_is_visible", "build_is_visible(selector)", new ExpressionFactoryHelper<BuildIsVisibleExp>());
            BatchCommand.BatchScript.Register("build_add_class", "build_add_class(selector, className)", new ExpressionFactoryHelper<BuildAddClassExp>());
            BatchCommand.BatchScript.Register("build_remove_class", "build_remove_class(selector, className)", new ExpressionFactoryHelper<BuildRemoveClassExp>());
            BatchCommand.BatchScript.Register("build_set_style", "build_set_style(selector, property, value)", new ExpressionFactoryHelper<BuildSetStyleExp>());
            BatchCommand.BatchScript.Register("build_inject_css", "build_inject_css(css)", new ExpressionFactoryHelper<BuildInjectCSSExp>());
            BatchCommand.BatchScript.Register("build_navigate_to", "build_navigate_to(url)", new ExpressionFactoryHelper<BuildNavigateToExp>());

            // Agent Command Operations
            BatchCommand.BatchScript.Register("send_command_to_inject", "send_command_to_inject(command, paramsJson)", new ExpressionFactoryHelper<SendCommandToInjectExp>());
            BatchCommand.BatchScript.Register("send_response_to_inject", "send_response_to_inject(responseJson)", new ExpressionFactoryHelper<SendResponseToInjectExp>());
            BatchCommand.BatchScript.Register("build_agent_response", "build_agent_response(messageId, success, data, error)", new ExpressionFactoryHelper<BuildAgentResponseExp>());
            BatchCommand.BatchScript.Register("parse_agent_command", "parse_agent_command(jsonData)", new ExpressionFactoryHelper<ParseAgentCommandExp>());
            BatchCommand.BatchScript.Register("get_message_param", "get_message_param(paramsObj, key)", new ExpressionFactoryHelper<GetMessageParamExp>());

            BatchCommand.BatchScript.Register("hot_reload", "hot_reload()", new ExpressionFactoryHelper<HotReloadExp>());

            BatchCommand.BatchScript.Register("append", "append(stringbuilder, val)", new ExpressionFactoryHelper<AppendExp>());
            BatchCommand.BatchScript.Register("append_line", "append_line(stringbuilder, val)", new ExpressionFactoryHelper<AppendLineExp>());

            BatchCommand.BatchScript.Register("string_index_of", "string_index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("string_last_index_of", "string_last_index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfExp>());
            BatchCommand.BatchScript.Register("string_index_of_any", "string_index_of_any(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("string_last_index_of_any", "string_last_index_of_any(str, substr[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("string_concat", "string_concat(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            BatchCommand.BatchScript.Register("concat_string", "concat_string(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            BatchCommand.BatchScript.Register("char_to_int", "char_to_int(char_str)", new ExpressionFactoryHelper<CharToIntExp>());
            BatchCommand.BatchScript.Register("int_to_char", "int_to_char(int_ascii)", new ExpressionFactoryHelper<IntToCharExp>());

            // alias for LLM Imaginary
            BatchCommand.BatchScript.Register("replace_string", "replace_string(str, substr, replace)", new ExpressionFactoryHelper<StringReplaceExp>());
            BatchCommand.BatchScript.Register("replace", "replace(str, substr, replace)", new ExpressionFactoryHelper<StringReplaceExp>());
            BatchCommand.BatchScript.Register("string_find", "string_find(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("stringfind", "stringfind(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("index_of", "index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("last_index_of", "last_index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfExp>());
            BatchCommand.BatchScript.Register("index_of_any", "index_of_any(str, char_list[, start, count])", new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("last_index_of_any", "last_index_of_any(str, char_list[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("indexof", "indexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("lastindexof", "lastindexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfExp>());
            BatchCommand.BatchScript.Register("indexofany", "indexofany(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("lastindexofany", "lastindexofany(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("string_indexof", "string_indexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("string_last_indexof", "string_last_indexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfExp>());
            BatchCommand.BatchScript.Register("string_indexof_any", "string_indexof_any(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("string_last_indexof_any", "string_last_indexof_any(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("stringindexof", "stringindexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("stringlastindexof", "stringlastindexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfExp>());
            BatchCommand.BatchScript.Register("stringindexofany", "stringindexofany(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("stringlastindexofany", "stringlastindexofany(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("stringlength", "stringlength(val)", false, new ExpressionFactoryHelper<StringLengthExp>());
            BatchCommand.BatchScript.Register("length", "length(str) or length(list) or length(dict)", new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("len", "len(str) or len(list) or len(dict)", new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("size", "size(str) or size(list) or size(dict)", new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("count", "count(str) or count(list) or count(dict)", new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("concat", "concat(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            BatchCommand.BatchScript.Register("strcat", "strcat(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            BatchCommand.BatchScript.Register("strlen", "strlen(val)", new ExpressionFactoryHelper<StringLengthExp>());
            BatchCommand.BatchScript.Register("mkdir", "mkdir(path)", new ExpressionFactoryHelper<FrameworkApiAlias.CreateDirectoryExp>());

            // Unified Multi-Language Code Analysis Operations
            UnifiedCodeAnalysisScriptApi.RegisterApis();

            // WebSocket API
            WebSocketApi.RegisterApis();

            // TreeSitter API Explorer
            TreeSitterExplorerApi.RegisterApis();

            FrameworkApiAlias.RegisterApis();

            // Regular Expression API
            RegexApi.RegisterApis();
        }
    }
}

