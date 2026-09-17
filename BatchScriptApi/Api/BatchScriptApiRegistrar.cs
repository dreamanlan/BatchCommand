using DotnetStoryScript.DslExpression;

namespace BatchCommand.Api
{
    /// <summary>
    /// Registers the apis implemented in the BatchScriptApi project to the dsl
    /// engine (BatchCommand.BatchScript.Register). Mirrors AgentCore's
    /// ScriptApiRegistrar: hosts call RegisterAllApis() once per thread (done
    /// automatically by DslHost.Prepare), or pick the individual groups.
    ///
    /// Register order matters when a host re-registers the same name with its
    /// own richer implementation (e.g. AgentCore's ScriptApiRegistrar runs
    /// after this and its registrations win).
    /// </summary>
    public static class BatchScriptApiRegistrar
    {
        /// <summary>
        /// Register everything the BatchScriptApi project provides: BatchScript
        /// related apis (Api/BatchScriptApi.cs), shared utility apis
        /// (Api/SharedApis.cs), file/size/diff/regex/other utility apis moved
        /// from AgentCore (Api/FileOperationsApi.cs, Api/FileSizeApi.cs,
        /// Api/DiffApi.cs, Api/OtherOperationsApi.cs, Api/RegexApi.cs) and the
        /// DotnetStoryScript api aliases (Api/FrameworkApiAlias.cs).
        /// </summary>
        public static void RegisterAllApis()
        {
            RegisterBatchScriptApis();
            RegisterSharedApis();
            RegisterFileApis();
            RegisterDiffApis();
            RegisterOtherUtilApis();
            RegisterClipboardApis();
            RegisterDslContextApis();
            RegisterRegexApis();
            RegisterFrameworkAliasApis();
            RegisterWsClientApis();
            RegisterHttpApis();
            RegisterProcessApis();
        }

        /// <summary>Websocket client api set (WebSocketClientApi.cs): wsclient_open/send/close/... + handle_wsclient_queue, shared by every host.</summary>
        public static void RegisterWsClientApis()
        {
            Api.WebSocketClientApi.RegisterApis();
        }

        /// <summary>HTTP api set (HttpApi.cs): http_get/post/... + *_callback async variants + download_file, shared by every host.</summary>
        public static void RegisterHttpApis()
        {
            Api.HttpApi.RegisterApis();
        }

        /// <summary>Process / script execution api set (ProcessApi.cs): execute_script/command, start/stop_process (child, graceful), launch/count/kill_process (OS-level), process io, shared by every host.</summary>
        public static void RegisterProcessApis()
        {
            Api.ProcessApi.RegisterApis();
        }

        /// <summary>BatchScript related apis (BatchScriptApi.cs): import, redirectcall, executemetadsl, the script task pool, nativelog, quote/strip, help, ...</summary>
        public static void RegisterBatchScriptApis()
        {
            // Basic framework APIs
            BatchScript.Register("import", "import(dsl_file,...)", false, new ExpressionFactoryHelper<ImportExp>());
            BatchScript.Register("redirectcall", "redirectcall(func_name) or redirectcall(func_name,args) or redirectcall(func_name, args, ...)", false, new ExpressionFactoryHelper<RedirectCallExp>());
            BatchScript.Register("executemetadsl", "executemetadsl(dsl_code), return (bool, result)", false, new ExpressionFactoryHelper<ExecuteMetaDslExp>());
            BatchScript.Register("execute_metadsl", "execute_metadsl(dsl_code), return (bool, result)", new ExpressionFactoryHelper<ExecuteMetaDslExp>());
            BatchScript.Register("call_metadsl_task", "call_metadsl_task(task_index,func_name,arg1,arg2,...) - queue a dsl func to worker thread task_index, an index at or past the default num raises the default to task_index+1 (an index more than 10 past it is rejected as a typo), for slow non-ui work such as sqlite writes, fire-and-forget, returns bool", new ExpressionFactoryHelper<CallMetaDslTaskExp>());
            BatchScript.Register("set_metadsl_task_num", "set_metadsl_task_num(num) - set the default worker thread num of call_metadsl_task, extra workers retire after being idle, returns the live num", new ExpressionFactoryHelper<SetMetaDslTaskNumExp>());
            BatchScript.Register("get_metadsl_task_num", "get_metadsl_task_num() - returns the live worker thread num of call_metadsl_task", new ExpressionFactoryHelper<GetMetaDslTaskNumExp>());
            BatchScript.Register("nativelog", "nativelog(fmt, ...)", new ExpressionFactoryHelper<NativeLogExp>());
            BatchScript.Register("quotestring", "quotestring(str)", false, new ExpressionFactoryHelper<QuoteStringExp>());
            BatchScript.Register("quote_string", "quote_string(str)", new ExpressionFactoryHelper<QuoteStringExp>());
            BatchScript.Register("stripquotes", "stripquotes(str)", false, new ExpressionFactoryHelper<StripQuotesExp>());
            BatchScript.Register("strip_quotes", "strip_quotes(str)", new ExpressionFactoryHelper<StripQuotesExp>());
            BatchScript.Register("trygetrawswitch", "trygetrawswitch(str)", false, new ExpressionFactoryHelper<TryGetRawCommandLineSwitchExp>());
            BatchScript.Register("try_get_raw_switch", "try_get_raw_switch(str)", new ExpressionFactoryHelper<TryGetRawCommandLineSwitchExp>());
            BatchScript.Register("getdotnetinfo", "getdotnetinfo()", false, new ExpressionFactoryHelper<GetDotnetInfoExp>());
            BatchScript.Register("get_dotnet_info", "get_dotnet_info()", false, new ExpressionFactoryHelper<GetDotnetInfoExp>());
            BatchScript.Register("get_string_in_length", "get_string_in_length(str,len[,begin0_end1_or_beginend2])", new ExpressionFactoryHelper<GetStringInLengthExp>());
            BatchScript.Register("help", "help(pattern, ...), agent api help", new ExpressionFactoryHelper<HelpExp>());
            BatchScript.Register("helpall", "helpall(pattern, ...), agent and framework api help", new ExpressionFactoryHelper<HelpAllExp>());
        }

        /// <summary>
        /// Shared utility apis (SharedApis.cs). string_contains*/combine_path/
        /// new_string_builder/string_builder_tostring are registered by
        /// FrameworkApiAlias.RegisterApis (the AgentCore-copied single
        /// implementation, under all alias names); to_json by
        /// RegisterOtherUtilApis.
        /// </summary>
        public static void RegisterSharedApis()
        {
            BatchScript.Register("append_line", "append_line(stringbuilder, val)", new ExpressionFactoryHelper<AppendLineExp>());
        }

        /// <summary>
        /// File operations (moved from AgentCore ScriptApi/FileOperationsApi.cs;
        /// the FileOperations service itself lives in Utils/FileOperations.cs,
        /// owned by DslHost.FileOps).
        /// </summary>
        public static void RegisterFileApis()
        {
            BatchScript.Register("read_file", "read_file(path[, encoding])", new ExpressionFactoryHelper<ReadFileExp>());
            BatchScript.Register("csv_read", "csv_read(path[, encoding]) - read CSV file into List<List<string>>, comma delimiter, double-quoted fields with \"\" escape, no cross-line quotes", new ExpressionFactoryHelper<CsvReadExp>());
            BatchScript.Register("write_file", "write_file(path, content[, encoding]) - encoding supports -bom/-no-bom/-nobom suffixes", new ExpressionFactoryHelper<WriteFileExp>());
            BatchScript.Register("write_file_no_bom", "write_file_no_bom(path, content) - Write the file without a BOM, preserving the encoding of the target file (UTF-8/UTF-16/UTF-32) if it exists; otherwise, use UTF-8", new ExpressionFactoryHelper<WriteFileNoBomExp>());
            BatchScript.Register("read_file_bytes", "read_file_bytes(path) - read file as byte array", new ExpressionFactoryHelper<ReadFileBytesExp>());
            BatchScript.Register("write_file_bytes", "write_file_bytes(path, bytes) - write byte array to file", new ExpressionFactoryHelper<WriteFileBytesExp>());
            BatchScript.Register("hex_to_bytes", "hex_to_bytes(hexString) - convert hex string to byte array", new ExpressionFactoryHelper<HexToBytesExp>());
            BatchScript.Register("hex_string_to_bytes", "hex_string_to_bytes(hexString) - convert hex string to byte array", false, new ExpressionFactoryHelper<HexToBytesExp>());
            BatchScript.Register("bytes_to_hex", "bytes_to_hex(bytes[, bytesPerLine]) - convert byte array to hex string, default 32 bytes per line", new ExpressionFactoryHelper<BytesToHexExp>());
            BatchScript.Register("bytes_to_hex_string", "bytes_to_hex_string(bytes[, bytesPerLine]) - convert byte array to hex string, default 32 bytes per line", false, new ExpressionFactoryHelper<BytesToHexExp>());
            BatchScript.Register("bytes_to_string", "bytes_to_string(bytes[, encoding]) - convert byte array to string, default UTF-8", new ExpressionFactoryHelper<BytesToStringExp>());
            BatchScript.Register("bytestostring", "bytestostring(bytes[, encoding]) - convert byte array to string, default UTF-8", false, new ExpressionFactoryHelper<BytesToStringExp>());
            BatchScript.Register("string_to_bytes", "string_to_bytes(str[, encoding]) - convert string to byte array, default UTF-8", new ExpressionFactoryHelper<StringToBytesExp>());
            BatchScript.Register("stringtobytes", "stringtobytes(str[, encoding]) - convert string to byte array, default UTF-8", false, new ExpressionFactoryHelper<StringToBytesExp>());
            BatchScript.Register("base64_to_bytes", "base64_to_bytes(base64String) - convert base64 string to byte array", new ExpressionFactoryHelper<Base64ToBytesExp>());
            BatchScript.Register("base64_string_to_bytes", "base64_string_to_bytes(base64String) - convert base64 string to byte array", false, new ExpressionFactoryHelper<Base64ToBytesExp>());
            BatchScript.Register("bytes_to_base64", "bytes_to_base64(bytes) - convert byte array to base64 string", new ExpressionFactoryHelper<BytesToBase64Exp>());
            BatchScript.Register("bytes_to_base64_string", "bytes_to_base64_string(bytes) - convert byte array to base64 string", false, new ExpressionFactoryHelper<BytesToBase64Exp>());
            BatchScript.Register("string_first_lines", "string_first_lines(str, n) - return the first n lines of a string", new ExpressionFactoryHelper<StringFirstLinesExp>());
            BatchScript.Register("string_replace_with_count", "string_replace_with_count(str, oldValue, newValue, count[, skipCount]) - skip first skipCount matches, then replace next N occurrences", new ExpressionFactoryHelper<StringReplaceWithCountExp>());
            BatchScript.Register("append_file", "append_file(path, content[, encoding]) - encoding supports -bom/-no-bom/-nobom suffixes", new ExpressionFactoryHelper<AppendFileExp>());
            BatchScript.Register("append_text", "append_text(path, content[, encoding])", false, new ExpressionFactoryHelper<AppendFileExp>());
            BatchScript.Register("append_text_to_file", "append_text_to_file(path, content[, encoding])", false, new ExpressionFactoryHelper<AppendFileExp>());
            BatchScript.Register("append_line_to_file", "append_line_to_file(path, content[, encoding])", false, new ExpressionFactoryHelper<AppendFileExp>());
            BatchScript.Register("append_all_text", "append_all_text(path, content[, encoding])", false, new ExpressionFactoryHelper<AppendFileExp>());
            BatchScript.Register("copy_file", "copy_file(sourcePath, destPath, overwrite)", new ExpressionFactoryHelper<CopyFileExp>());
            BatchScript.Register("move_file", "move_file(sourcePath, destPath, overwrite)", new ExpressionFactoryHelper<MoveFileExp>());
            BatchScript.Register("rename_file", "rename_file(source, destination)", new ExpressionFactoryHelper<MoveFileExp>());
            BatchScript.Register("file_exists", "file_exists(path)", new ExpressionFactoryHelper<FileExistsExp>());
            BatchScript.Register("file_exist", "file_exist(path)", false, new ExpressionFactoryHelper<FileExistsExp>());
            BatchScript.Register("exist_file", "exist_file(path)", false, new ExpressionFactoryHelper<FileExistsExp>());
            BatchScript.Register("exists_file", "exists_file(path)", false, new ExpressionFactoryHelper<FileExistsExp>());
            BatchScript.Register("is_file", "is_file(path)", false, new ExpressionFactoryHelper<FileExistsExp>());
            BatchScript.Register("isfile", "isfile(path)", false, new ExpressionFactoryHelper<FileExistsExp>());
            BatchScript.Register("file_not_exists", "file_not_exists(path)", new ExpressionFactoryHelper<FileNotExistsExp>());
            BatchScript.Register("file_not_exist", "file_not_exist(path)", false, new ExpressionFactoryHelper<FileNotExistsExp>());
            BatchScript.Register("not_exist_file", "not_exist_file(path)", false, new ExpressionFactoryHelper<FileNotExistsExp>());
            BatchScript.Register("not_exists_file", "not_exists_file(path)", false, new ExpressionFactoryHelper<FileNotExistsExp>());
            BatchScript.Register("file_has_bom", "file_has_bom(path) - check whether file has BOM", new ExpressionFactoryHelper<FileHasBomExp>());
            BatchScript.Register("file_add_bom", "file_add_bom(path) - add BOM to file (skip if already has BOM)", new ExpressionFactoryHelper<FileAddBomExp>());
            BatchScript.Register("file_remove_bom", "file_remove_bom(path) - remove BOM from file (skip if no BOM)", new ExpressionFactoryHelper<FileRemoveBomExp>());
            BatchScript.Register("dir_exists", "dir_exists(path)", new ExpressionFactoryHelper<DirExistsExp>());
            BatchScript.Register("dir_exist", "dir_exist(path)", false, new ExpressionFactoryHelper<DirExistsExp>());
            BatchScript.Register("exist_dir", "exist_dir(path)", false, new ExpressionFactoryHelper<DirExistsExp>());
            BatchScript.Register("exists_dir", "exists_dir(path)", false, new ExpressionFactoryHelper<DirExistsExp>());
            BatchScript.Register("is_dir", "is_dir(path)", false, new ExpressionFactoryHelper<DirExistsExp>());
            BatchScript.Register("isdir", "isdir(path)", false, new ExpressionFactoryHelper<DirExistsExp>());
            BatchScript.Register("dir_not_exists", "dir_not_exists(path)", new ExpressionFactoryHelper<DirNotExistsExp>());
            BatchScript.Register("dir_not_exist", "dir_not_exist(path)", false, new ExpressionFactoryHelper<DirNotExistsExp>());
            BatchScript.Register("not_exist_dir", "not_exist_dir(path)", false, new ExpressionFactoryHelper<DirNotExistsExp>());
            BatchScript.Register("not_exists_dir", "not_exists_dir(path)", false, new ExpressionFactoryHelper<DirNotExistsExp>());
            BatchScript.Register("directory_not_exists", "directory_not_exists(path)", new ExpressionFactoryHelper<DirNotExistsExp>());
            BatchScript.Register("directory_not_exist", "directory_not_exist(path)", false, new ExpressionFactoryHelper<DirNotExistsExp>());
            BatchScript.Register("not_exist_directory", "not_exist_directory(path)", false, new ExpressionFactoryHelper<DirNotExistsExp>());
            BatchScript.Register("not_exists_directory", "not_exists_directory(path)", false, new ExpressionFactoryHelper<DirNotExistsExp>());
            BatchScript.Register("path_exists", "path_exists(path)", new ExpressionFactoryHelper<PathExistsExp>());
            BatchScript.Register("path_exist", "path_exist(path)", false, new ExpressionFactoryHelper<PathExistsExp>());
            BatchScript.Register("exist_path", "exist_path(path)", false, new ExpressionFactoryHelper<PathExistsExp>());
            BatchScript.Register("exists_path", "exists_path(path)", false, new ExpressionFactoryHelper<PathExistsExp>());
            BatchScript.Register("exist", "exist(path)", false, new ExpressionFactoryHelper<PathExistsExp>());
            BatchScript.Register("exists", "exists(path)", false, new ExpressionFactoryHelper<PathExistsExp>());
            BatchScript.Register("is_path", "is_path(path)", false, new ExpressionFactoryHelper<PathExistsExp>());
            BatchScript.Register("ispath", "ispath(path)", false, new ExpressionFactoryHelper<PathExistsExp>());
            BatchScript.Register("path_not_exists", "path_not_exists(path)", new ExpressionFactoryHelper<PathNotExistsExp>());
            BatchScript.Register("path_not_exist", "path_not_exist(path)", false, new ExpressionFactoryHelper<PathNotExistsExp>());
            BatchScript.Register("not_exist_path", "not_exist_path(path)", false, new ExpressionFactoryHelper<PathNotExistsExp>());
            BatchScript.Register("not_exists_path", "not_exists_path(path)", false, new ExpressionFactoryHelper<PathNotExistsExp>());
            BatchScript.Register("not_exist", "not_exist(path)", false, new ExpressionFactoryHelper<PathNotExistsExp>());
            BatchScript.Register("not_exists", "not_exists(path)", false, new ExpressionFactoryHelper<PathNotExistsExp>());
            BatchScript.Register("list_dir_info", "list_dir_info(path[, glob_pattern, recursive]) return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<ListDirInfoExp>());
            BatchScript.Register("find_files", "find_files(path, glob_pattern[, recursive]) return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindFilesExp>());
            BatchScript.Register("search_files", "search_files(glob_pattern, path) return List, use 'to_string' to convert to a string", false, new ExpressionFactoryHelper<SearchFilesExp>());
            BatchScript.Register("get_file_size", "get_file_size(path)", new ExpressionFactoryHelper<GetFileSizeExp>());
            BatchScript.Register("file_size", "file_size(path)", false, new ExpressionFactoryHelper<GetFileSizeExp>());
            BatchScript.Register("get_file_length", "get_file_length(path)", false, new ExpressionFactoryHelper<GetFileSizeExp>());
            BatchScript.Register("file_length", "file_length(path)", false, new ExpressionFactoryHelper<GetFileSizeExp>());
            BatchScript.Register("file_last_write_time", "file_last_write_time(path)", new ExpressionFactoryHelper<GetFileLastWriteTimeExp>());
            BatchScript.Register("get_file_last_write_time", "get_file_last_write_time(path)", false, new ExpressionFactoryHelper<GetFileLastWriteTimeExp>());
            BatchScript.Register("get_file_write_time", "get_file_write_time(path)", false, new ExpressionFactoryHelper<GetFileLastWriteTimeExp>());
            BatchScript.Register("file_last_access_time", "file_last_access_time(path)", new ExpressionFactoryHelper<GetFileLastAccessTimeExp>());
            BatchScript.Register("get_file_last_access_time", "get_file_last_access_time(path)", false, new ExpressionFactoryHelper<GetFileLastAccessTimeExp>());
            BatchScript.Register("get_file_access_time", "get_file_access_time(path)", false, new ExpressionFactoryHelper<GetFileLastAccessTimeExp>());
            BatchScript.Register("file_create_time", "file_create_time(path)", new ExpressionFactoryHelper<GetFileCreateTimeExp>());
            BatchScript.Register("get_file_create_time", "get_file_create_time(path)", false, new ExpressionFactoryHelper<GetFileCreateTimeExp>());
            BatchScript.Register("set_file_attributes", "set_file_attributes(path, int_attrs)", new ExpressionFactoryHelper<SetFileAttributesExp>());
            BatchScript.Register("set_file_attribute", "set_file_attribute(path, int_attrs)", false, new ExpressionFactoryHelper<SetFileAttributesExp>());
            BatchScript.Register("get_file_attributes", "get_file_attributes(path)", new ExpressionFactoryHelper<GetFileAttributesExp>());
            BatchScript.Register("get_file_attribute", "get_file_attribute(path)", false, new ExpressionFactoryHelper<GetFileAttributesExp>());
            BatchScript.Register("file_attributes", "file_attributes(path)", false, new ExpressionFactoryHelper<GetFileAttributesExp>());
            BatchScript.Register("file_attribute", "file_attribute(path)", false, new ExpressionFactoryHelper<GetFileAttributesExp>());
            BatchScript.Register("set_file_last_write_time", "set_file_last_write_time(path, date_time)", new ExpressionFactoryHelper<SetFileLastWriteTimeExp>());
            BatchScript.Register("set_file_write_time", "set_file_write_time(path, date_time)", false, new ExpressionFactoryHelper<SetFileLastWriteTimeExp>());
            BatchScript.Register("set_file_last_access_time", "set_file_last_access_time(path, date_time)", new ExpressionFactoryHelper<SetFileLastAccessTimeExp>());
            BatchScript.Register("set_file_access_time", "set_file_access_time(path, date_time)", false, new ExpressionFactoryHelper<SetFileLastAccessTimeExp>());
            BatchScript.Register("set_file_create_time", "set_file_create_time(path, date_time)", new ExpressionFactoryHelper<SetFileCreateTimeExp>());
            BatchScript.Register("get_diff_time_days", "get_diff_time_days(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalDays truncated to int", new ExpressionFactoryHelper<GetDiffTimeDaysExp>());
            BatchScript.Register("get_diff_time_seconds", "get_diff_time_seconds(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalSeconds truncated to int", new ExpressionFactoryHelper<GetDiffTimeSecondsExp>());
            BatchScript.Register("get_diff_time_ms", "get_diff_time_ms(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalMilliseconds truncated to long", new ExpressionFactoryHelper<GetDiffTimeMsExp>());
            BatchScript.Register("calc_diff_time_days", "calc_diff_time_days(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalDays truncated to int", false, new ExpressionFactoryHelper<GetDiffTimeDaysExp>());
            BatchScript.Register("calc_diff_time_seconds", "calc_diff_time_seconds(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalSeconds truncated to int", false, new ExpressionFactoryHelper<GetDiffTimeSecondsExp>());
            BatchScript.Register("calc_diff_time_ms", "calc_diff_time_ms(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalMilliseconds truncated to long", false, new ExpressionFactoryHelper<GetDiffTimeMsExp>());
            BatchScript.Register("search_log_file", "search_log_file(log_file, search_regex[, context_lines_after, context_lines_before, encoding])", new ExpressionFactoryHelper<SearchLogFileExp>());
            BatchScript.Register("grep_log_file", "grep_log_file(log_file, search_regex[, context_lines_after, context_lines_before, encoding])", new ExpressionFactoryHelper<SearchLogFileExp>());
            BatchScript.Register("search_log_file_as_list", "search_log_file_as_list(log_file, search_regex[, context_lines_after, context_lines_before, encoding]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, supports LINQ such as .where($$.MatchedCount > 1)), use 'to_string' to convert to a string", new ExpressionFactoryHelper<SearchLogFileAsListExp>());
            BatchScript.Register("grep_log_file_as_list", "grep_log_file_as_list(log_file, search_regex[, context_lines_after, context_lines_before, encoding]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, supports LINQ such as .where($$.MatchedCount > 1)), use 'to_string' to convert to a string", new ExpressionFactoryHelper<SearchLogFileAsListExp>());
            BatchScript.Register("tail_log_file", "tail_log_file(log_file, lines[, encoding])", new ExpressionFactoryHelper<TailLogFileExp>());
            BatchScript.Register("head_log_file", "head_log_file(log_file, lines[, encoding])", new ExpressionFactoryHelper<HeadLogFileExp>());
            BatchScript.Register("tail_file", "tail_file(file, lines[, encoding])", new ExpressionFactoryHelper<TailFileExp>());
            BatchScript.Register("head_file", "head_file(file, lines[, encoding])", new ExpressionFactoryHelper<HeadFileExp>());
            BatchScript.Register("read_line_range", "read_line_range(path, startLine, endLine[, encoding]) - read line range as concatenated string, preserving original newline style", new ExpressionFactoryHelper<ReadFileLineRangeExp>());
            BatchScript.Register("read_file_line_range", "read_file_line_range(path, startLine, endLine[, encoding]) - read line range as concatenated string, preserving original newline style", new ExpressionFactoryHelper<ReadFileLineRangeExp>());
            BatchScript.Register("readlinerange", "readlinerange(path, startLine, endLine[, encoding]) - read line range as concatenated string, preserving original newline style", new ExpressionFactoryHelper<ReadFileLineRangeExp>());
            BatchScript.Register("ensure_dir", "ensure_dir(path)", new ExpressionFactoryHelper<EnsureDirectoryExp>());
            BatchScript.Register("ensure_directory_exists", "ensure_directory_exists(path)", false, new ExpressionFactoryHelper<EnsureDirectoryExp>());
            BatchScript.Register("ensure_directory", "ensure_directory(path)", new ExpressionFactoryHelper<EnsureDirectoryExp>());
            BatchScript.Register("remove_dir", "remove_dir(path)", new ExpressionFactoryHelper<RemoveDirectoryExp>());
            BatchScript.Register("remove_directory", "remove_directory(path)", new ExpressionFactoryHelper<RemoveDirectoryExp>());
        }

        /// <summary>Diff operations (moved from AgentCore ScriptApi/DiffApi.cs).</summary>
        public static void RegisterDiffApis()
        {
            BatchScript.Register("apply_unified_diff", "apply_unified_diff(targetPath, diffPathOrContent[, isContent[, exactMatch]]) return Object(success/error/linesAdded/linesRemoved), exactMatch=true requires exact whitespace; false (default) falls back to trimmed and normalized-whitespace matching", false, new ExpressionFactoryHelper<ApplyDiffExp>());
            BatchScript.Register("apply_diff", "apply_diff(targetPath, diffPathOrContent[, isContent[, exactMatch]]) return Object(success/error/linesAdded/linesRemoved), exactMatch=true requires exact whitespace; false (default) falls back to trimmed and normalized-whitespace matching", new ExpressionFactoryHelper<ApplyDiffExp>());
            BatchScript.Register("applydiff", "applydiff(targetPath, diffPathOrContent[, isContent[, exactMatch]]) return Object(success/error/linesAdded/linesRemoved), exactMatch=true requires exact whitespace; false (default) falls back to trimmed and normalized-whitespace matching", false, new ExpressionFactoryHelper<ApplyDiffExp>());
        }

        /// <summary>
        /// Generic json/string/collection utility apis (moved from AgentCore
        /// ScriptApi/OtherOperationsApi.cs; the network/http, clipboard and
        /// logger apis stayed in AgentCore).
        /// </summary>
        public static void RegisterOtherUtilApis()
        {
            // JSON Operations
            BatchScript.Register("to_json", "to_json(obj, prettyPrint)", new ExpressionFactoryHelper<ToJsonExp>());
            BatchScript.Register("array_to_json", "array_to_json(array)", false, new ExpressionFactoryHelper<ToJsonExp>());
            BatchScript.Register("list_to_json", "list_to_json(list)", false, new ExpressionFactoryHelper<ToJsonExp>());
            BatchScript.Register("hashtable_to_json", "hashtable_to_json(hashtable)", false, new ExpressionFactoryHelper<ToJsonExp>());
            BatchScript.Register("from_json", "from_json(json)", new ExpressionFactoryHelper<FromJsonExp>());
            BatchScript.Register("json_escape", "json_escape(str[, bool_add_quotes])", new ExpressionFactoryHelper<JsonEscapeExp>());
            BatchScript.Register("escape_json", "escape_json(str[, bool_add_quotes])", false, new ExpressionFactoryHelper<JsonEscapeExp>());
            BatchScript.Register("escape_json_string", "escape_json_string(str[, bool_add_quotes])", false, new ExpressionFactoryHelper<JsonEscapeExp>());
            BatchScript.Register("escape_json_str", "escape_json_str(str[, bool_add_quotes])", false, new ExpressionFactoryHelper<JsonEscapeExp>());
            BatchScript.Register("json_unescape", "json_unescape(str)", new ExpressionFactoryHelper<JsonUnescapeExp>());
            BatchScript.Register("unescape_json", "unescape_json(str)", false, new ExpressionFactoryHelper<JsonUnescapeExp>());
            BatchScript.Register("unescape_json_string", "unescape_json_string(str)", false, new ExpressionFactoryHelper<JsonUnescapeExp>());
            BatchScript.Register("unescape_json_str", "unescape_json_str(str)", false, new ExpressionFactoryHelper<JsonUnescapeExp>());

            BatchScript.Register("new_object", "new_object(key1, value1, key2, value2, ...)", new ExpressionFactoryHelper<NewObjectExp>());
            BatchScript.Register("array_to_string", "array_to_string(array)", false, new ExpressionFactoryHelper<ToStringExp>());
            BatchScript.Register("list_to_string", "list_to_string(list)", false, new ExpressionFactoryHelper<ToStringExp>());
            BatchScript.Register("hashtable_to_string", "hashtable_to_string(hashtable)", false, new ExpressionFactoryHelper<ToStringExp>());
            BatchScript.Register("to_string", "to_string(val)", new ExpressionFactoryHelper<ToStringExp>());
            BatchScript.Register("tostring", "tostring(val)", new ExpressionFactoryHelper<ToStringExp>());
            BatchScript.Register("to_str", "to_str(val)", false, new ExpressionFactoryHelper<ToStringExp>());
            BatchScript.Register("tostr", "tostr(val)", false, new ExpressionFactoryHelper<ToStringExp>());
            BatchScript.Register("to_pretty_string", "to_pretty_string(val) - convert to a more readable form by unescaping escape sequences (\\n \\r \\t \\\" \\\\ \\uXXXX \\xH...) into their actual characters", new ExpressionFactoryHelper<ToPrettyStringExp>());
            BatchScript.Register("toprettystring", "toprettystring(val) - convert to a more readable form by unescaping escape sequences (\\n \\r \\t \\\" \\\\ \\uXXXX \\xH...) into their actual characters", new ExpressionFactoryHelper<ToPrettyStringExp>());
            BatchScript.Register("to_pretty_str", "to_pretty_str(val) - convert to a more readable form by unescaping escape sequences (\\n \\r \\t \\\" \\\\ \\uXXXX \\xH...) into their actual characters", false, new ExpressionFactoryHelper<ToPrettyStringExp>());
            BatchScript.Register("toprettystr", "toprettystr(val) - convert to a more readable form by unescaping escape sequences (\\n \\r \\t \\\" \\\\ \\uXXXX \\xH...) into their actual characters", false, new ExpressionFactoryHelper<ToPrettyStringExp>());
            BatchScript.Register("string_length", "string_length(str)", new ExpressionFactoryHelper<StringLengthExp>());
            BatchScript.Register("str_length", "str_length(str)", false, new ExpressionFactoryHelper<StringLengthExp>());
            BatchScript.Register("string_starts_with", "string_starts_with(str, substr)", new ExpressionFactoryHelper<StringStartsWithExp>());
            BatchScript.Register("str_starts_with", "str_starts_with(str, substr)", false, new ExpressionFactoryHelper<StringStartsWithExp>());
            BatchScript.Register("string_ends_with", "string_ends_with(str, substr)", new ExpressionFactoryHelper<StringEndsWithExp>());
            BatchScript.Register("str_ends_with", "str_ends_with(str, substr)", false, new ExpressionFactoryHelper<StringEndsWithExp>());
            BatchScript.Register("extract_tags", "extract_tags(txt, tag_name[, max_num]) - extract XML-style tag contents like <a>x|y</a>/<a></a>/<a/>, content split by '|', case-sensitive, non-greedy. Returns IList<IList<string>>. max_num<=0 means no limit.", new ExpressionFactoryHelper<ExtractTagsExp>());
            BatchScript.Register("extracttags", "extracttags(txt, tag_name[, max_num])", false, new ExpressionFactoryHelper<ExtractTagsExp>());
            BatchScript.Register("extract_tag_codes", "extract_tag_codes(txt, tag_name[, max_num]) - extract XML-style tag contents like <a>x</a>/<a></a>/<a/>, case-sensitive, non-greedy. Returns List<BoxedValue> of raw string content per match. max_num<=0 means no limit.", new ExpressionFactoryHelper<ExtractTagCodesExp>());
            BatchScript.Register("extracttagcodes", "extracttagcodes(txt, tag_name[, max_num])", false, new ExpressionFactoryHelper<ExtractTagCodesExp>());

            // HTML/URL Encode Operations
            BatchScript.Register("html_encode", "html_encode(html_str)", new ExpressionFactoryHelper<HtmlEncodeExp>());
            BatchScript.Register("html_decode", "html_decode(encoded_html_str)", new ExpressionFactoryHelper<HtmlDecodeExp>());
            BatchScript.Register("url_encode", "url_encode(url_str)", new ExpressionFactoryHelper<UrlEncodeExp>());
            BatchScript.Register("url_decode", "url_decode(encoded_url_str)", new ExpressionFactoryHelper<UrlDecodeExp>());

            // collection utilities
            BatchScript.Register("list_contains", "list_contains(list,val)", new ExpressionFactoryHelper<ListContainsExp>());
            BatchScript.Register("hashtable_contains", "hashtable_contains(hash,val)", new ExpressionFactoryHelper<HashtableContainsExp>());
            BatchScript.Register("string_builder_length", "string_builder_length(sb)", new ExpressionFactoryHelper<StringBuilderLengthExp>());
            BatchScript.Register("hashtable_contains_key", "hashtable_contains_key(hash,key)", false, new ExpressionFactoryHelper<HashtableContainsExp>());
            BatchScript.Register("stringbuilder_length", "stringbuilder_length(sb)", false, new ExpressionFactoryHelper<StringBuilderLengthExp>());
            BatchScript.Register("stringbuilderlength", "stringbuilderlength(sb)", false, new ExpressionFactoryHelper<StringBuilderLengthExp>());
            BatchScript.Register("char_code_at", "char_code_at(str, index)", new ExpressionFactoryHelper<CharCodeAtExp>());
            BatchScript.Register("charcodeat", "charcodeat(str, index)", false, new ExpressionFactoryHelper<CharCodeAtExp>());
            BatchScript.Register("array_to_list", "array_to_list(array[, index, count])", new ExpressionFactoryHelper<ArrayToListExp>());
            BatchScript.Register("arraytolist", "arraytolist(array[, index, count])", false, new ExpressionFactoryHelper<ArrayToListExp>());
            BatchScript.Register("list_to_array", "list_to_array(list[, index, count])", new ExpressionFactoryHelper<ListToArrayExp>());
            BatchScript.Register("listtoarray", "listtoarray(list[, index, count])", false, new ExpressionFactoryHelper<ListToArrayExp>());
            BatchScript.Register("sublist", "sublist(list[, index, count])", new ExpressionFactoryHelper<SubListExp>());
            BatchScript.Register("list_slice", "list_slice(list[, index, count])", false, new ExpressionFactoryHelper<SubListExp>());
            BatchScript.Register("list_get_range", "list_get_range(list[, index, count])", false, new ExpressionFactoryHelper<SubListExp>());
            BatchScript.Register("subarray", "subarray(array[, index, count])", new ExpressionFactoryHelper<SubArrayExp>());
            BatchScript.Register("array_slice", "array_slice(array[, index, count])", false, new ExpressionFactoryHelper<SubArrayExp>());
            BatchScript.Register("array_get_range", "array_get_range(array[, index, count])", false, new ExpressionFactoryHelper<SubArrayExp>());

            // string length aliases
            BatchScript.Register("stringlength", "stringlength(val)", false, new ExpressionFactoryHelper<StringLengthExp>());
            BatchScript.Register("strlength", "strlength(val)", false, new ExpressionFactoryHelper<StringLengthExp>());
            BatchScript.Register("strlen", "strlen(val)", new ExpressionFactoryHelper<StringLengthExp>());
            BatchScript.Register("str_len", "str_len(val)", false, new ExpressionFactoryHelper<StringLengthExp>());
            BatchScript.Register("starts_with", "starts_with(str, substr)", new ExpressionFactoryHelper<StringStartsWithExp>());
            BatchScript.Register("ends_with", "ends_with(str, substr)", new ExpressionFactoryHelper<StringEndsWithExp>());
        }

        /// <summary>Clipboard apis (TextCopy, moved from AgentCore OtherOperationsApi.cs).</summary>
        public static void RegisterClipboardApis()
        {
            BatchScript.Register("get_clipboard", "get_clipboard()", new ExpressionFactoryHelper<GetClipboardExp>());
            BatchScript.Register("set_clipboard", "set_clipboard(text)", new ExpressionFactoryHelper<SetClipboardExp>());
        }

        /// <summary>
        /// Global context variable apis (moved from AgentCore DslContextApi.cs);
        /// they operate on the global store owned by DslHost. Per-instance
        /// variants (agent_*) stay in AgentCore (AgentStateApi on AgentInstance).
        /// </summary>
        public static void RegisterDslContextApis()
        {
            BatchScript.Register("set_context_var", "set_context_var(key, value)", new ExpressionFactoryHelper<SetContextVarExp>());
            BatchScript.Register("get_context_var", "get_context_var(key)", new ExpressionFactoryHelper<GetContextVarExp>());
            BatchScript.Register("remove_context_var", "remove_context_var(key)", new ExpressionFactoryHelper<RemoveContextVarExp>());
            BatchScript.Register("clear_context_vars", "clear_context_vars()", new ExpressionFactoryHelper<ClearContextVarsExp>());
        }

        /// <summary>Regex apis (moved from AgentCore ScriptApi/RegexApi.cs).</summary>
        public static void RegisterRegexApis()
        {
            RegexApi.RegisterApis();
        }

        /// <summary>Aliases over the DotnetStoryScript.dll built-in apis (FrameworkApiAlias.cs).</summary>
        public static void RegisterFrameworkAliasApis()
        {
            FrameworkApiAlias.RegisterApis();
        }
    }
}
