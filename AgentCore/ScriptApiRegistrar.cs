using System;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using BatchCommand;
using BatchCommand.Utils;
using AgentCore.ScriptApi;

namespace AgentCore
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
            // File operations, file size/time, diff, json/string/collection utility
            // and regex apis are generic: they moved to BatchCommand.Api (registered
            // by BatchScriptApiRegistrar via DslHost.Prepare, see
            // BatchScriptApi/Api/{FileOperationsApi,FileSizeApi,DiffApi,OtherOperationsApi,RegexApi}.cs).

            // AgentCore worker pool convention (DslHost dsl file execution
            // pool): reserve worker 0 for the httpproxy / webserver filter
            // callbacks and worker 1 for the web server dsl pages (see
            // Core/WebCallbacks.cs); scripts calling
            // execute_dsl_file_in_worker start at this index by default.
            // Idempotent: RegisterAllApis runs per prepared thread.
            BatchCommand.Api.DslHost.MinDslFileWorkerCount = 2;

            // Code Editing Operations
            BatchCommand.BatchScript.Register("replace_in_file", "replace_in_file(path, oldString, newString[, replaceAll[, exactMatch[, encoding]]]), exactMatch=true requires exact whitespace; false (default) falls back to trimmed and normalized-whitespace matching; encoding supports -bom/-no-bom/-nobom suffixes", new ExpressionFactoryHelper<ReplaceInFileExp>());
            BatchCommand.BatchScript.Register("replaceinfile", "replaceinfile(path, oldString, newString[, replaceAll[, exactMatch[, encoding]]]), exactMatch=true requires exact whitespace; false (default) falls back to trimmed and normalized-whitespace matching; encoding supports -bom/-no-bom/-nobom suffixes", false, new ExpressionFactoryHelper<ReplaceInFileExp>());
            BatchCommand.BatchScript.Register("string_replace_in_file", "string_replace_in_file(path, oldString, newString[, replaceAll[, exactMatch[, encoding]]]), exactMatch=true requires exact whitespace; false (default) falls back to trimmed and normalized-whitespace matching; encoding supports -bom/-no-bom/-nobom suffixes", false, new ExpressionFactoryHelper<ReplaceInFileExp>());
            BatchCommand.BatchScript.Register("string_replace_file", "string_replace_file(path, oldString, newString[, replaceAll[, exactMatch[, encoding]]]), exactMatch=true requires exact whitespace; false (default) falls back to trimmed and normalized-whitespace matching; encoding supports -bom/-no-bom/-nobom suffixes", false, new ExpressionFactoryHelper<ReplaceInFileExp>());
            BatchCommand.BatchScript.Register("replace_in_file_with_count", "replace_in_file_with_count(path, oldString, newString, count[, skipCount[, exactMatch[, encoding]]]) - skip first skipCount matches, then replace next N literal occurrences in file, exactMatch=true requires exact whitespace; false (default) allows trimmed and normalized-whitespace fallback only when skipCount=0 and count=1; encoding supports -bom/-no-bom/-nobom suffixes", new ExpressionFactoryHelper<ReplaceInFileWithCountExp>());
            BatchCommand.BatchScript.Register("string_replace_in_file_with_count", "string_replace_in_file_with_count(path, oldString, newString, count[, skipCount[, exactMatch[, encoding]]]) - skip first skipCount matches, then replace next N literal occurrences in file, exactMatch=true requires exact whitespace; false (default) allows trimmed and normalized-whitespace fallback only when skipCount=0 and count=1; encoding supports -bom/-no-bom/-nobom suffixes", false, new ExpressionFactoryHelper<ReplaceInFileWithCountExp>());
            BatchCommand.BatchScript.Register("multi_replace", "multi_replace(path, editsJson[, encoding]) editsJson=[{\"old_string\":\"...\",\"new_string\":\"...\",\"replace_all\":false,\"exact_match\":false},...], string or list of hashtables, exact_match=true requires exact whitespace; false (default) falls back to trimmed and normalized-whitespace matching; encoding supports -bom/-no-bom/-nobom suffixes", new ExpressionFactoryHelper<MultiReplaceExp>());
            BatchCommand.BatchScript.Register("replace_range", "replace_range(path, startLine, endLine, newContent[, encoding]) - encoding supports -bom/-no-bom/-nobom suffixes", new ExpressionFactoryHelper<ReplaceRangeExp>());
            BatchCommand.BatchScript.Register("insert_after_text", "insert_after_text(path, searchLiteralText, content[, allOccurrences[, exactMatch[, encoding]]]), exactMatch=true requires exact whitespace; false (default) falls back to trimmed and normalized-whitespace matching; encoding supports -bom/-no-bom/-nobom suffixes", new ExpressionFactoryHelper<InsertAfterTextExp>());
            BatchCommand.BatchScript.Register("insert_before_text", "insert_before_text(path, searchLiteralText, content[, allOccurrences[, exactMatch[, encoding]]]), exactMatch=true requires exact whitespace; false (default) falls back to trimmed and normalized-whitespace matching; encoding supports -bom/-no-bom/-nobom suffixes", new ExpressionFactoryHelper<InsertBeforeTextExp>());
            BatchCommand.BatchScript.Register("insert_after", "insert_after(path, line, insert_content[, encoding]) - insert content after specified line number; encoding supports -bom/-no-bom/-nobom suffixes", new ExpressionFactoryHelper<InsertAfterLineExp>());
            BatchCommand.BatchScript.Register("insert_before", "insert_before(path, line, insert_content[, encoding]) - insert content before specified line number; encoding supports -bom/-no-bom/-nobom suffixes", new ExpressionFactoryHelper<InsertBeforeLineExp>());
            BatchCommand.BatchScript.Register("insert_after_line", "insert_after_line(path, line, insert_content[, encoding]) - insert content after specified line number; encoding supports -bom/-no-bom/-nobom suffixes", new ExpressionFactoryHelper<InsertAfterLineExp>());
            BatchCommand.BatchScript.Register("insert_before_line", "insert_before_line(path, line, insert_content[, encoding]) - insert content before specified line number; encoding supports -bom/-no-bom/-nobom suffixes", new ExpressionFactoryHelper<InsertBeforeLineExp>());
            BatchCommand.BatchScript.Register("delete_lines", "delete_lines(path, startLine, endLine[, encoding]) - encoding supports -bom/-no-bom/-nobom suffixes", new ExpressionFactoryHelper<DeleteLinesExp>());
            BatchCommand.BatchScript.Register("search_lines", "search_lines(path, regex_pattern, [ignoreCase[, encoding]]) return List, use 'to_string' to convert to a string. auto fallback to substring if regex invalid", new ExpressionFactoryHelper<SearchLinesInFileExp>());
            BatchCommand.BatchScript.Register("search_lines_in_file", "search_lines_in_file(path, regex_pattern, [ignoreCase[, encoding]]) return List, use 'to_string' to convert to a string. auto fallback to substring if regex invalid", false, new ExpressionFactoryHelper<SearchLinesInFileExp>());
            BatchCommand.BatchScript.Register("read_lines", "read_lines(path, startLine, endLine[, encoding]) return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<ReadLinesExp>());
            BatchCommand.BatchScript.Register("read_file_lines", "read_file_lines(path, startLine, endLine[, encoding]) return List, use 'to_string' to convert to a string", false, new ExpressionFactoryHelper<ReadLinesExp>());
            BatchCommand.BatchScript.Register("readlines", "readlines(path, startLine, endLine[, encoding]) return List, use 'to_string' to convert to a string", false, new ExpressionFactoryHelper<ReadLinesExp>());

            BatchCommand.BatchScript.Register("get_line_count", "get_line_count(path[, encoding])", new ExpressionFactoryHelper<GetLineCountExp>());
            BatchCommand.BatchScript.Register("get_file_line_count", "get_file_line_count(path[, encoding])", false, new ExpressionFactoryHelper<GetLineCountExp>());
            BatchCommand.BatchScript.Register("line_count", "line_count(path[, encoding])", new ExpressionFactoryHelper<GetLineCountExp>());
            BatchCommand.BatchScript.Register("file_line_count", "file_line_count(path[, encoding])", false, new ExpressionFactoryHelper<GetLineCountExp>());
            BatchCommand.BatchScript.Register("count_lines", "count_lines(path[, encoding])", false, new ExpressionFactoryHelper<GetLineCountExp>());
            BatchCommand.BatchScript.Register("search_in_file", "search_in_file(path, regex_pattern[, context_lines_after, context_lines_before, encoding])", new ExpressionFactoryHelper<SearchInFileExp>());
            BatchCommand.BatchScript.Register("grep_file", "grep_file(path, regex_pattern[, context_lines_after, context_lines_before, encoding])", new ExpressionFactoryHelper<SearchInFileExp>());
            BatchCommand.BatchScript.Register("grepfile", "grepfile(path, regex_pattern[, context_lines_after, context_lines_before, encoding])", false, new ExpressionFactoryHelper<SearchInFileExp>());
            BatchCommand.BatchScript.Register("search_in_file_as_list", "search_in_file_as_list(path, regex_pattern[, context_lines_after, context_lines_before, encoding]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, supports LINQ such as .where($$.MatchedCount > 1)), use 'to_string' to convert to a string", new ExpressionFactoryHelper<SearchInFileAsListExp>());
            BatchCommand.BatchScript.Register("grep_file_as_list", "grep_file_as_list(path, regex_pattern[, context_lines_after, context_lines_before, encoding]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, supports LINQ such as .where($$.MatchedCount > 1)), use 'to_string' to convert to a string", new ExpressionFactoryHelper<SearchInFileAsListExp>());
            BatchCommand.BatchScript.Register("grepfileaslist", "grepfileaslist(path, regex_pattern[, context_lines_after, context_lines_before, encoding]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, supports LINQ such as .where($$.MatchedCount > 1)), use 'to_string' to convert to a string", false, new ExpressionFactoryHelper<SearchInFileAsListExp>());
            BatchCommand.BatchScript.Register("search_in_files", "search_in_files(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...])", new ExpressionFactoryHelper<SearchInFilesExp>());
            BatchCommand.BatchScript.Register("grep_files", "grep_files(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...])", new ExpressionFactoryHelper<SearchInFilesExp>());
            BatchCommand.BatchScript.Register("grepfiles", "grepfiles(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...])", false, new ExpressionFactoryHelper<SearchInFilesExp>());
            BatchCommand.BatchScript.Register("search_in_files_as_list", "search_in_files_as_list(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, flattened across files, supports LINQ such as .where($$.FilePath.EndsWith(\"cs\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<SearchInFilesAsListExp>());
            BatchCommand.BatchScript.Register("grep_dir", "grep_dir(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...])", false, new ExpressionFactoryHelper<SearchInFilesExp>());
            BatchCommand.BatchScript.Register("grep_files_as_list", "grep_files_as_list(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, flattened across files, supports LINQ such as .where($$.FilePath.EndsWith(\"cs\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<SearchInFilesAsListExp>());
            BatchCommand.BatchScript.Register("grepfilesaslist", "grepfilesaslist(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, flattened across files, supports LINQ such as .where($$.FilePath.EndsWith(\"cs\"))), use 'to_string' to convert to a string", false, new ExpressionFactoryHelper<SearchInFilesAsListExp>());
            BatchCommand.BatchScript.Register("search_in_files_with_encoding", "search_in_files_with_encoding(path, regex_pattern, encoding[, context_lines_after, context_lines_before, filter_list_or_str_1, ...])", new ExpressionFactoryHelper<SearchInFilesWithEncodingExp>());
            BatchCommand.BatchScript.Register("search_in_files_as_list_with_encoding", "search_in_files_as_list_with_encoding(path, regex_pattern, encoding[, context_lines_after, context_lines_before, filter_list_or_str_1, ...]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, flattened across files, supports LINQ such as .where($$.FilePath.EndsWith(\"cs\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<SearchInFilesAsListWithEncodingExp>());
            BatchCommand.BatchScript.Register("count_file_indentations", "count_file_indentations(path[, startLine, endLine, encoding]) - display lines with line number, indent info, and content", new ExpressionFactoryHelper<CountFileIndentationsExp>());
            BatchCommand.BatchScript.Register("count_indentations", "count_indentations(path[, startLine, endLine, encoding]) - display lines with line number, indent info, and content", false, new ExpressionFactoryHelper<CountFileIndentationsExp>());
            BatchCommand.BatchScript.Register("countindentations", "countindentations(path[, startLine, endLine, encoding]) - display lines with line number, indent info, and content", false, new ExpressionFactoryHelper<CountFileIndentationsExp>());
            BatchCommand.BatchScript.Register("count_file_indentation", "count_file_indentation(path[, startLine, endLine, encoding])", false, new ExpressionFactoryHelper<CountFileIndentationsExp>());
            BatchCommand.BatchScript.Register("count_indentation", "count_indentation(path[, startLine, endLine, encoding])", false, new ExpressionFactoryHelper<CountFileIndentationsExp>());
            BatchCommand.BatchScript.Register("countindentation", "countindentation(path[, startLine, endLine, encoding])", false, new ExpressionFactoryHelper<CountFileIndentationsExp>());

            // Clipboard apis (TextCopy) are generic: they moved to BatchCommand.Api
            // (see BatchScriptApi/Api/OtherOperationsApi.cs).

            // Logging Operations
            BatchCommand.BatchScript.Register("log_info", "log_info(fmt, ...)", new ExpressionFactoryHelper<LogInfoExp>());
            BatchCommand.BatchScript.Register("log_error", "log_error(fmt, ...)", new ExpressionFactoryHelper<LogErrorExp>());
            BatchCommand.BatchScript.Register("log_warning", "log_warning(fmt, ...)", new ExpressionFactoryHelper<LogWarningExp>());

            // HTTP Operations: http_* / download_file moved to BatchScriptApi (Api/HttpApi.cs).

            // Process Operations

            // Global context var apis (set/get/remove/clear_context_var) moved to
            // BatchCommand.Api (Api/DslContextApi.cs, global store on DslHost).

            // DSL Context Management Operations (per agent instance, keyed by port)
            BatchCommand.BatchScript.Register("agent_set_context_var", "agent_set_context_var(port, key, value)", new ExpressionFactoryHelper<AgentSetContextVarExp>());
            BatchCommand.BatchScript.Register("agent_get_context_var", "agent_get_context_var(port, key)", new ExpressionFactoryHelper<AgentGetContextVarExp>());
            BatchCommand.BatchScript.Register("agent_remove_context_var", "agent_remove_context_var(port, key)", new ExpressionFactoryHelper<AgentRemoveContextVarExp>());
            BatchCommand.BatchScript.Register("agent_clear_context_vars", "agent_clear_context_vars(port)", new ExpressionFactoryHelper<AgentClearContextVarsExp>());

            // Browser Interaction Operations
            BatchCommand.BatchScript.Register("build_query_selector", "build_query_selector(selector)", new ExpressionFactoryHelper<BuildQuerySelectorExp>());
            BatchCommand.BatchScript.Register("build_click_element", "build_click_element(selector)", new ExpressionFactoryHelper<BuildClickElementExp>());
            BatchCommand.BatchScript.Register("build_set_value", "build_set_value(selector, value)", new ExpressionFactoryHelper<BuildSetValueExp>());
            BatchCommand.BatchScript.Register("build_get_value", "build_get_value(selector)", new ExpressionFactoryHelper<BuildGetValueExp>());
            BatchCommand.BatchScript.Register("build_get_text", "build_get_text(selector)", new ExpressionFactoryHelper<BuildGetTextExp>());
            BatchCommand.BatchScript.Register("build_set_innerhtml", "build_set_innerhtml(selector, html)", new ExpressionFactoryHelper<BuildSetInnerHTMLExp>());
            BatchCommand.BatchScript.Register("build_wait_for_element", "build_wait_for_element(selector[, timeout_def_5000ms])", new ExpressionFactoryHelper<BuildWaitForElementExp>());
            BatchCommand.BatchScript.Register("build_scroll_to_element", "build_scroll_to_element(selector)", new ExpressionFactoryHelper<BuildScrollToElementExp>());
            BatchCommand.BatchScript.Register("build_is_visible", "build_is_visible(selector)", new ExpressionFactoryHelper<BuildIsVisibleExp>());
            BatchCommand.BatchScript.Register("build_add_class", "build_add_class(selector, className)", new ExpressionFactoryHelper<BuildAddClassExp>());
            BatchCommand.BatchScript.Register("build_remove_class", "build_remove_class(selector, className)", new ExpressionFactoryHelper<BuildRemoveClassExp>());
            BatchCommand.BatchScript.Register("build_set_style", "build_set_style(selector, property, value)", new ExpressionFactoryHelper<BuildSetStyleExp>());
            BatchCommand.BatchScript.Register("build_inject_css", "build_inject_css(css)", new ExpressionFactoryHelper<BuildInjectCSSExp>());
            BatchCommand.BatchScript.Register("build_navigate_to", "build_navigate_to(url)", new ExpressionFactoryHelper<BuildNavigateToExp>());
            BatchCommand.BatchScript.Register("send_js_code", "send_js_code(jscode)", new ExpressionFactoryHelper<SendJsCodeExp>());
            BatchCommand.BatchScript.Register("send_js_call", "send_js_call(jsfunc, arg1, arg2, ...)", new ExpressionFactoryHelper<SendJsCallExp>());

            // Agent Command Operations
            BatchCommand.BatchScript.Register("parse_agent_command", "parse_agent_command(jsonData)", new ExpressionFactoryHelper<ParseAgentCommandExp>());
            BatchCommand.BatchScript.Register("parse_agent_notification", "parse_agent_notification(jsonData)", new ExpressionFactoryHelper<ParseAgentNotificationExp>());
            BatchCommand.BatchScript.Register("get_message_param", "get_message_param(paramsObj, key)", new ExpressionFactoryHelper<GetMessageParamExp>());
            BatchCommand.BatchScript.Register("send_command_to_inject", "send_command_to_inject(command, paramsJson)", new ExpressionFactoryHelper<SendCommandToInjectExp>());
            BatchCommand.BatchScript.Register("build_agent_response", "build_agent_response(messageId, success, data, error)", new ExpressionFactoryHelper<BuildAgentResponseExp>());
            BatchCommand.BatchScript.Register("send_response_to_inject", "send_response_to_inject(responseJson)", new ExpressionFactoryHelper<SendResponseToInjectExp>());

            BatchCommand.BatchScript.Register("hot_reload", "hot_reload()", new ExpressionFactoryHelper<HotReloadExp>());
            BatchCommand.BatchScript.Register("restart_page", "restart_page() - restart the page: close the window, terminate the renderers and reopen it (no dll update)", new ExpressionFactoryHelper<RestartPageExp>());

            BatchCommand.BatchScript.Register("call_skill", "call_skill(skill_name, arg1, arg2, ...)", new ExpressionFactoryHelper<CallSkillExp>());
            BatchCommand.BatchScript.Register("refresh_skills", "refresh_skills()", new ExpressionFactoryHelper<RefreshSkillsExp>());
            BatchCommand.BatchScript.Register("refresh_embedding", "refresh_embedding()", new ExpressionFactoryHelper<RefreshEmbeddingExp>());
            BatchCommand.BatchScript.Register("refresh_reranker", "refresh_reranker()", new ExpressionFactoryHelper<RefreshRerankExp>());

            // Skill Environment Operations
            BatchCommand.BatchScript.Register("set_skill_env", "set_skill_env(key, value) - set skill environment variable", new ExpressionFactoryHelper<SetSkillEnvExp>());
            BatchCommand.BatchScript.Register("get_skill_env", "get_skill_env(key[, defval]) - get skill environment variable", new ExpressionFactoryHelper<GetSkillEnvExp>());
            BatchCommand.BatchScript.Register("delete_skill_env", "delete_skill_env(key) - delete skill environment variable", new ExpressionFactoryHelper<DeleteSkillEnvExp>());
            BatchCommand.BatchScript.Register("clear_skill_envs", "clear_skill_envs([regexPattern]) - clear skill environment variables", new ExpressionFactoryHelper<ClearSkillEnvsExp>());

            BatchCommand.BatchScript.Register("append", "append(stringbuilder, val)", new ExpressionFactoryHelper<AppendExp>());
            BatchCommand.BatchScript.Register("append_string", "append_string(stringbuilder, val)", false, new ExpressionFactoryHelper<AppendExp>());

            BatchCommand.BatchScript.Register("string_index_of", "string_index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("string_last_index_of", "string_last_index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfExp>());
            BatchCommand.BatchScript.Register("string_index_of_any", "string_index_of_any(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("string_last_index_of_any", "string_last_index_of_any(str, substr[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("str_index_of", "str_index_of(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("str_last_index_of", "str_last_index_of(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfExp>());
            BatchCommand.BatchScript.Register("str_index_of_any", "str_index_of_any(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("str_last_index_of_any", "str_last_index_of_any(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("string_concat", "string_concat(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            BatchCommand.BatchScript.Register("str_concat", "str_concat(str1, str2, ...)", false, new ExpressionFactoryHelper<StringConcatExp>());
            BatchCommand.BatchScript.Register("concat_string", "concat_string(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            BatchCommand.BatchScript.Register("concat_str", "concat_str(str1, str2, ...)", false, new ExpressionFactoryHelper<StringConcatExp>());
            BatchCommand.BatchScript.Register("char_to_int", "char_to_int(char_str)", new ExpressionFactoryHelper<CharToIntExp>());
            BatchCommand.BatchScript.Register("int_to_char", "int_to_char(int_ascii)", new ExpressionFactoryHelper<IntToCharExp>());

            // alias for LLM Imaginary
            BatchCommand.BatchScript.Register("string_find", "string_find(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("stringfind", "stringfind(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("find_string", "find_string(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("findstring", "findstring(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("find_in_string", "find_in_string(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("str_find", "str_find(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("strfind", "strfind(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("find_str", "find_str(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("findstr", "findstr(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("find_in_str", "find_in_str(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("index_of", "index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("find_index_of", "find_index_of(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("last_index_of", "last_index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfExp>());
            BatchCommand.BatchScript.Register("find_last_index_of", "find_last_index_of(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfExp>());
            BatchCommand.BatchScript.Register("index_of_any", "index_of_any(str, char_list[, start, count])", new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("find_index_of_any", "find_index_of_any(str, char_list[, start, count])", new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("last_index_of_any", "last_index_of_any(str, char_list[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("find_last_index_of_any", "find_last_index_of_any(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
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
            BatchCommand.BatchScript.Register("str_indexof", "str_indexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("str_last_indexof", "str_last_indexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfExp>());
            BatchCommand.BatchScript.Register("str_indexof_any", "str_indexof_any(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("str_last_indexof_any", "str_last_indexof_any(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("strindexof", "strindexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            BatchCommand.BatchScript.Register("strlastindexof", "strlastindexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfExp>());
            BatchCommand.BatchScript.Register("strindexofany", "strindexofany(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("strlastindexofany", "strlastindexofany(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            BatchCommand.BatchScript.Register("length", "length(str) or length(list) or length(hashtable)", new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("len", "len(str) or len(list) or len(hashtable)", new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("size", "size(str) or size(list) or size(hashtable)", new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("count", "count(str) or count(list) or count(hashtable)", new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("size_of", "size_of(str) or size_of(list) or size_of(hashtable)", false, new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("get_count", "get_count(str) or get_count(list) or get_count(hashtable)", false, new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("get_size", "get_size(str) or get_size(list) or get_size(hashtable)", false, new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("get_length", "get_length(str) or get_length(list) or get_length(hashtable)", false, new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("array_size", "array_size(list)", false, new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("array_count", "array_count(list)", false, new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("array_length", "array_length(list)", false, new ExpressionFactoryHelper<SizeExp>());
            BatchCommand.BatchScript.Register("string_append", "string_append(str1, str2, ...)", false, new ExpressionFactoryHelper<StringConcatExp>());
            BatchCommand.BatchScript.Register("str_append", "str_append(str1, str2, ...)", false, new ExpressionFactoryHelper<StringConcatExp>());
            BatchCommand.BatchScript.Register("concat", "concat(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            BatchCommand.BatchScript.Register("strcat", "strcat(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            BatchCommand.BatchScript.Register("str_cat", "str_cat(str1, str2, ...)", false, new ExpressionFactoryHelper<StringConcatExp>());
            BatchCommand.BatchScript.Register("hotreload", "hotreload()", false, new ExpressionFactoryHelper<HotReloadExp>());
            BatchCommand.BatchScript.Register("callskill", "callskill(skill_name, arg1, arg2, ...)", false, new ExpressionFactoryHelper<CallSkillExp>());
            BatchCommand.BatchScript.Register("refreshskills", "refreshskills()", false, new ExpressionFactoryHelper<RefreshSkillsExp>());
            BatchCommand.BatchScript.Register("refreshembedding", "refreshembedding()", false, new ExpressionFactoryHelper<RefreshEmbeddingExp>());
            BatchCommand.BatchScript.Register("refreshreranker", "refreshreranker()", false, new ExpressionFactoryHelper<RefreshRerankExp>());

            BatchCommand.BatchScript.Register("name_contains", "name_contains(str,str_or_list_1,str_or_list_2,...)", false, new ExpressionFactoryHelper<BatchCommand.Api.FrameworkApiAlias.StringContainsExp>());
            BatchCommand.BatchScript.Register("name_not_contains", "name_not_contains(str,str_or_list_1,str_or_list_2,...)", false, new ExpressionFactoryHelper<BatchCommand.Api.FrameworkApiAlias.StringNotContainsExp>());
            BatchCommand.BatchScript.Register("name_contains_any", "name_contains_any(str,str_or_list_1,str_or_list_2,...)", false, new ExpressionFactoryHelper<BatchCommand.Api.FrameworkApiAlias.StringContainsAnyExp>());
            BatchCommand.BatchScript.Register("name_not_contains_any", "name_not_contains_any(str,str_or_list_1,str_or_list_2,...)", false, new ExpressionFactoryHelper<BatchCommand.Api.FrameworkApiAlias.StringNotContainsAnyExp>());

            // Unified Multi-Language Code Analysis Operations
            UnifiedCodeAnalysisScriptApi.RegisterApis();

            // WebSocket API
            WebSocketApi.RegisterApis();

            // HTTP Proxy API (local CORS reverse proxy)
            HttpProxyApi.RegisterApis();

            // Static Web Server API (per-port document root, header rules, dsl pages)
            WebServerApi.RegisterApis();

            // Web result object APIs (httpproxy / webserver filter callbacks,
            // web server dsl pages)
            WebApi.RegisterApis();

            // Admin process launch (elevated, UAC)
            AdminProcessApi.RegisterApis();

            // TreeSitter API Explorer
            TreeSitterExplorerApi.RegisterApis();

            BatchCommand.Api.FrameworkApiAlias.RegisterApis();

            // Semantic Index API
            SemanticApi.RegisterApis();

            // Segment / Tokenize API
            SegmentApi.RegisterApis();

            // LLM Client API (OpenAI / Claude / AutoMetaDSL / Ollama)
            LlmApi.RegisterApis();

            // MCP Client API
            McpApi.RegisterApis();

            // HTTP Auth Server API (OAuth loopback redirect capture)
            HttpAuthServerApi.RegisterApis();

            // Playwright Browser Automation API
            PlaywrightApi.RegisterApis();

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

