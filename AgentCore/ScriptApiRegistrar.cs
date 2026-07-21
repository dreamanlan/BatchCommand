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
            AgentFrameworkService.Instance.DslEngine!.Register("read_file", "read_file(path[, encoding])", new ExpressionFactoryHelper<ReadFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("csv_read", "csv_read(path[, encoding]) - read CSV file into List<List<string>>, comma delimiter, double-quoted fields with \"\" escape, no cross-line quotes", new ExpressionFactoryHelper<CsvReadExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("write_file", "write_file(path, content[, encoding])", new ExpressionFactoryHelper<WriteFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("write_file_no_bom", "write_file_no_bom(path, content) - write UTF-8 file without BOM header", new ExpressionFactoryHelper<WriteFileNoBomExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("read_file_bytes", "read_file_bytes(path) - read file as byte array", new ExpressionFactoryHelper<ReadFileBytesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("write_file_bytes", "write_file_bytes(path, bytes) - write byte array to file", new ExpressionFactoryHelper<WriteFileBytesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hex_to_bytes", "hex_to_bytes(hexString) - convert hex string to byte array", new ExpressionFactoryHelper<HexToBytesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hex_string_to_bytes", "hex_string_to_bytes(hexString) - convert hex string to byte array", false, new ExpressionFactoryHelper<HexToBytesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("bytes_to_hex", "bytes_to_hex(bytes[, bytesPerLine]) - convert byte array to hex string, default 32 bytes per line", new ExpressionFactoryHelper<BytesToHexExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("bytes_to_hex_string", "bytes_to_hex_string(bytes[, bytesPerLine]) - convert byte array to hex string, default 32 bytes per line", false, new ExpressionFactoryHelper<BytesToHexExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("bytes_to_string", "bytes_to_string(bytes[, encoding]) - convert byte array to string, default UTF-8", new ExpressionFactoryHelper<BytesToStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("bytestostring", "bytestostring(bytes[, encoding]) - convert byte array to string, default UTF-8", false, new ExpressionFactoryHelper<BytesToStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_to_bytes", "string_to_bytes(str[, encoding]) - convert string to byte array, default UTF-8", new ExpressionFactoryHelper<StringToBytesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stringtobytes", "stringtobytes(str[, encoding]) - convert string to byte array, default UTF-8", false, new ExpressionFactoryHelper<StringToBytesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("base64_to_bytes", "base64_to_bytes(base64String) - convert base64 string to byte array", new ExpressionFactoryHelper<Base64ToBytesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("base64_string_to_bytes", "base64_string_to_bytes(base64String) - convert base64 string to byte array", false, new ExpressionFactoryHelper<Base64ToBytesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("bytes_to_base64", "bytes_to_base64(bytes) - convert byte array to base64 string", new ExpressionFactoryHelper<BytesToBase64Exp>());
            AgentFrameworkService.Instance.DslEngine!.Register("bytes_to_base64_string", "bytes_to_base64_string(bytes) - convert byte array to base64 string", false, new ExpressionFactoryHelper<BytesToBase64Exp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_first_lines", "string_first_lines(str, n) - return the first n lines of a string", new ExpressionFactoryHelper<StringFirstLinesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_replace_with_count", "string_replace_with_count(str, oldValue, newValue, count) - replace first N occurrences", new ExpressionFactoryHelper<StringReplaceWithCountExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("append_file", "append_file(path, content[, encoding])", new ExpressionFactoryHelper<AppendFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("append_text", "append_text(path, content)", false, new ExpressionFactoryHelper<AppendFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("append_text_to_file", "append_text_to_file(path, content)", false, new ExpressionFactoryHelper<AppendFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("append_all_text", "append_all_text(path, content)", false, new ExpressionFactoryHelper<AppendFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("copy_file", "copy_file(sourcePath, destPath, overwrite)", new ExpressionFactoryHelper<CopyFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("move_file", "move_file(sourcePath, destPath, overwrite)", new ExpressionFactoryHelper<MoveFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("rename_file", "rename_file(source, destination)", new ExpressionFactoryHelper<MoveFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_exists", "file_exists(path)", new ExpressionFactoryHelper<FileExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_exist", "file_exist(path)", false, new ExpressionFactoryHelper<FileExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("exist_file", "exist_file(path)", false, new ExpressionFactoryHelper<FileExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("exists_file", "exists_file(path)", false, new ExpressionFactoryHelper<FileExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("is_file", "is_file(path)", false, new ExpressionFactoryHelper<FileExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_not_exists", "file_not_exists(path)", new ExpressionFactoryHelper<FileNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_not_exist", "file_not_exist(path)", false, new ExpressionFactoryHelper<FileNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("not_exist_file", "not_exist_file(path)", false, new ExpressionFactoryHelper<FileNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("not_exists_file", "not_exists_file(path)", false, new ExpressionFactoryHelper<FileNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_has_bom", "file_has_bom(path) - check whether file has UTF-8 BOM", new ExpressionFactoryHelper<FileHasBomExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_add_bom", "file_add_bom(path) - add UTF-8 BOM to file (skip if already has BOM)", new ExpressionFactoryHelper<FileAddBomExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_remove_bom", "file_remove_bom(path) - remove UTF-8 BOM from file (skip if no BOM)", new ExpressionFactoryHelper<FileRemoveBomExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("dir_exists", "dir_exists(path)", new ExpressionFactoryHelper<DirExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("dir_exist", "dir_exist(path)", false, new ExpressionFactoryHelper<DirExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("exist_dir", "exist_dir(path)", false, new ExpressionFactoryHelper<DirExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("exists_dir", "exists_dir(path)", false, new ExpressionFactoryHelper<DirExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("is_dir", "is_dir(path)", false, new ExpressionFactoryHelper<DirExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("dir_not_exists", "dir_not_exists(path)", new ExpressionFactoryHelper<DirNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("dir_not_exist", "dir_not_exist(path)", false, new ExpressionFactoryHelper<DirNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("not_exist_dir", "not_exist_dir(path)", false, new ExpressionFactoryHelper<DirNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("not_exists_dir", "not_exists_dir(path)", false, new ExpressionFactoryHelper<DirNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("directory_not_exists", "directory_not_exists(path)", new ExpressionFactoryHelper<DirNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("directory_not_exist", "directory_not_exist(path)", false, new ExpressionFactoryHelper<DirNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("not_exist_directory", "not_exist_directory(path)", false, new ExpressionFactoryHelper<DirNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("not_exists_directory", "not_exists_directory(path)", false, new ExpressionFactoryHelper<DirNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("path_exists", "path_exists(path)", new ExpressionFactoryHelper<PathExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("path_exist", "path_exist(path)", false, new ExpressionFactoryHelper<PathExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("exist_path", "exist_path(path)", false, new ExpressionFactoryHelper<PathExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("exists_path", "exists_path(path)", false, new ExpressionFactoryHelper<PathExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("exist", "exist(path)", false, new ExpressionFactoryHelper<PathExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("exists", "exists(path)", false, new ExpressionFactoryHelper<PathExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("is_path", "is_path(path)", false, new ExpressionFactoryHelper<PathExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("path_not_exists", "path_not_exists(path)", new ExpressionFactoryHelper<PathNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("path_not_exist", "path_not_exist(path)", false, new ExpressionFactoryHelper<PathNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("not_exist_path", "not_exist_path(path)", false, new ExpressionFactoryHelper<PathNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("not_exists_path", "not_exists_path(path)", false, new ExpressionFactoryHelper<PathNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("not_exist", "not_exist(path)", false, new ExpressionFactoryHelper<PathNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("not_exists", "not_exists(path)", false, new ExpressionFactoryHelper<PathNotExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_dir_info", "list_dir_info(path[, glob_pattern, recursive]) return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<ListDirInfoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_files", "find_files(path, glob_pattern[, recursive]) return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindFilesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_files", "search_files(glob_pattern, path) return List, use 'to_string' to convert to a string", false, new ExpressionFactoryHelper<SearchFilesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_size", "get_file_size(path)", new ExpressionFactoryHelper<GetFileSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_size", "file_size(path)", false, new ExpressionFactoryHelper<GetFileSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_length", "get_file_length(path)", false, new ExpressionFactoryHelper<GetFileSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_length", "file_length(path)", false, new ExpressionFactoryHelper<GetFileSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_last_write_time", "file_last_write_time(path)", new ExpressionFactoryHelper<GetFileLastWriteTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_last_write_time", "get_file_last_write_time(path)", false, new ExpressionFactoryHelper<GetFileLastWriteTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_write_time", "get_file_write_time(path)", false, new ExpressionFactoryHelper<GetFileLastWriteTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_last_access_time", "file_last_access_time(path)", new ExpressionFactoryHelper<GetFileLastAccessTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_last_access_time", "get_file_last_access_time(path)", false, new ExpressionFactoryHelper<GetFileLastAccessTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_access_time", "get_file_access_time(path)", false, new ExpressionFactoryHelper<GetFileLastAccessTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_create_time", "file_create_time(path)", new ExpressionFactoryHelper<GetFileCreateTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_create_time", "get_file_create_time(path)", false, new ExpressionFactoryHelper<GetFileCreateTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_file_attributes", "set_file_attributes(path, int_attrs)", new ExpressionFactoryHelper<SetFileAttributesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_file_attribute", "set_file_attribute(path, int_attrs)", false, new ExpressionFactoryHelper<SetFileAttributesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_attributes", "get_file_attributes(path)", new ExpressionFactoryHelper<GetFileAttributesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_attribute", "get_file_attribute(path)", false, new ExpressionFactoryHelper<GetFileAttributesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_attributes", "file_attributes(path)", false, new ExpressionFactoryHelper<GetFileAttributesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_attribute", "file_attribute(path)", false, new ExpressionFactoryHelper<GetFileAttributesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_file_last_write_time", "set_file_last_write_time(path, date_time)", new ExpressionFactoryHelper<SetFileLastWriteTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_file_write_time", "set_file_write_time(path, date_time)", false, new ExpressionFactoryHelper<SetFileLastWriteTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_file_last_access_time", "set_file_last_access_time(path, date_time)", new ExpressionFactoryHelper<SetFileLastAccessTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_file_access_time", "set_file_access_time(path, date_time)", false, new ExpressionFactoryHelper<SetFileLastAccessTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_file_create_time", "set_file_create_time(path, date_time)", new ExpressionFactoryHelper<SetFileCreateTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_diff_time_days", "get_diff_time_days(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalDays truncated to int", new ExpressionFactoryHelper<GetDiffTimeDaysExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_diff_time_seconds", "get_diff_time_seconds(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalSeconds truncated to int", new ExpressionFactoryHelper<GetDiffTimeSecondsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_diff_time_ms", "get_diff_time_ms(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalMilliseconds truncated to long", new ExpressionFactoryHelper<GetDiffTimeMsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("calc_diff_time_days", "calc_diff_time_days(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalDays truncated to int", false, new ExpressionFactoryHelper<GetDiffTimeDaysExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("calc_diff_time_seconds", "calc_diff_time_seconds(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalSeconds truncated to int", false, new ExpressionFactoryHelper<GetDiffTimeSecondsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("calc_diff_time_ms", "calc_diff_time_ms(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalMilliseconds truncated to long", false, new ExpressionFactoryHelper<GetDiffTimeMsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_log_file", "search_log_file(log_file, search_regex[, context_lines_after, context_lines_before, encoding])", new ExpressionFactoryHelper<SearchLogFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grep_log_file", "grep_log_file(log_file, search_regex[, context_lines_after, context_lines_before, encoding])", new ExpressionFactoryHelper<SearchLogFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_log_file_as_list", "search_log_file_as_list(log_file, search_regex[, context_lines_after, context_lines_before, encoding]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, supports LINQ such as .where($$.MatchedCount > 1)), use 'to_string' to convert to a string", new ExpressionFactoryHelper<SearchLogFileAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grep_log_file_as_list", "grep_log_file_as_list(log_file, search_regex[, context_lines_after, context_lines_before, encoding]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, supports LINQ such as .where($$.MatchedCount > 1)), use 'to_string' to convert to a string", new ExpressionFactoryHelper<SearchLogFileAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("tail_log_file", "tail_log_file(log_file, lines[, encoding])", new ExpressionFactoryHelper<TailLogFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("head_log_file", "head_log_file(log_file, lines[, encoding])", new ExpressionFactoryHelper<HeadLogFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("tail_file", "tail_file(file, lines[, encoding])", new ExpressionFactoryHelper<TailFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("head_file", "head_file(file, lines[, encoding])", new ExpressionFactoryHelper<HeadFileExp>());

            // Code Editing Operations
            AgentFrameworkService.Instance.DslEngine!.Register("replace_in_file", "replace_in_file(path, oldString, newString[, replaceAll[, exactMatch[, encoding]]])", new ExpressionFactoryHelper<ReplaceInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("replaceinfile", "replaceinfile(path, oldString, newString[, replaceAll[, exactMatch[, encoding]]])", false, new ExpressionFactoryHelper<ReplaceInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_replace_in_file", "string_replace_in_file(path, oldString, newString[, replaceAll[, exactMatch[, encoding]]])", false, new ExpressionFactoryHelper<ReplaceInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_replace_file", "string_replace_file(path, oldString, newString[, replaceAll[, exactMatch[, encoding]]])", false, new ExpressionFactoryHelper<ReplaceInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("multi_replace", "multi_replace(path, editsJson[, encoding]) editsJson=[{\"old_string\":\"...\",\"new_string\":\"...\",\"replace_all\":false,\"exact_match\":false},...]", new ExpressionFactoryHelper<MultiReplaceExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("replace_range", "replace_range(path, startLine, endLine, newContent[, encoding])", new ExpressionFactoryHelper<ReplaceRangeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("insert_after_text", "insert_after_text(path, searchLiteralText, content[, allOccurrences[, exactMatch[, encoding]]])", new ExpressionFactoryHelper<InsertAfterTextExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("insert_before_text", "insert_before_text(path, searchLiteralText, content[, allOccurrences[, exactMatch[, encoding]]])", new ExpressionFactoryHelper<InsertBeforeTextExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("insert_after", "insert_after(path, line, insert_content[, encoding]) - insert content after specified line number", new ExpressionFactoryHelper<InsertAfterLineExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("insert_before", "insert_before(path, line, insert_content[, encoding]) - insert content before specified line number", new ExpressionFactoryHelper<InsertBeforeLineExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("insert_after_line", "insert_after_line(path, line, insert_content[, encoding]) - insert content after specified line number", new ExpressionFactoryHelper<InsertAfterLineExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("insert_before_line", "insert_before_line(path, line, insert_content[, encoding]) - insert content before specified line number", new ExpressionFactoryHelper<InsertBeforeLineExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("delete_lines", "delete_lines(path, startLine, endLine[, encoding])", new ExpressionFactoryHelper<DeleteLinesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_lines", "search_lines(path, regex_pattern, [ignoreCase[, encoding]]) return List, use 'to_string' to convert to a string. auto fallback to substring if regex invalid", new ExpressionFactoryHelper<SearchLinesInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_lines_in_file", "search_lines_in_file(path, regex_pattern, [ignoreCase[, encoding]]) return List, use 'to_string' to convert to a string. auto fallback to substring if regex invalid", false, new ExpressionFactoryHelper<SearchLinesInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("read_lines", "read_lines(path, startLine, endLine[, encoding]) return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<ReadLinesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("read_file_lines", "read_file_lines(path, startLine, endLine[, encoding]) return List, use 'to_string' to convert to a string", false, new ExpressionFactoryHelper<ReadLinesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("readlines", "readlines(path, startLine, endLine[, encoding]) return List, use 'to_string' to convert to a string", false, new ExpressionFactoryHelper<ReadLinesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("read_line_range", "read_line_range(path, startLine, endLine[, encoding]) - read line range as concatenated string, preserving original newline style", new ExpressionFactoryHelper<ReadFileLineRangeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("read_file_line_range", "read_file_line_range(path, startLine, endLine[, encoding]) - read line range as concatenated string, preserving original newline style", new ExpressionFactoryHelper<ReadFileLineRangeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("readlinerange", "readlinerange(path, startLine, endLine[, encoding]) - read line range as concatenated string, preserving original newline style", new ExpressionFactoryHelper<ReadFileLineRangeExp>());

            AgentFrameworkService.Instance.DslEngine!.Register("get_line_count", "get_line_count(path[, encoding])", new ExpressionFactoryHelper<GetLineCountExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_line_count", "get_file_line_count(path[, encoding])", false, new ExpressionFactoryHelper<GetLineCountExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("line_count", "line_count(path[, encoding])", new ExpressionFactoryHelper<GetLineCountExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_line_count", "file_line_count(path[, encoding])", false, new ExpressionFactoryHelper<GetLineCountExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("count_lines", "count_lines(path[, encoding])", false, new ExpressionFactoryHelper<GetLineCountExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_in_file", "search_in_file(path, regex_pattern[, context_lines_after, context_lines_before, encoding])", new ExpressionFactoryHelper<SearchInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grep_file", "grep_file(path, regex_pattern[, context_lines_after, context_lines_before, encoding])", new ExpressionFactoryHelper<SearchInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grepfile", "grepfile(path, regex_pattern[, context_lines_after, context_lines_before, encoding])", false, new ExpressionFactoryHelper<SearchInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_in_file_as_list", "search_in_file_as_list(path, regex_pattern[, context_lines_after, context_lines_before, encoding]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, supports LINQ such as .where($$.MatchedCount > 1)), use 'to_string' to convert to a string", new ExpressionFactoryHelper<SearchInFileAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grep_file_as_list", "grep_file_as_list(path, regex_pattern[, context_lines_after, context_lines_before, encoding]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, supports LINQ such as .where($$.MatchedCount > 1)), use 'to_string' to convert to a string", new ExpressionFactoryHelper<SearchInFileAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grepfileaslist", "grepfileaslist(path, regex_pattern[, context_lines_after, context_lines_before, encoding]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, supports LINQ such as .where($$.MatchedCount > 1)), use 'to_string' to convert to a string", false, new ExpressionFactoryHelper<SearchInFileAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_in_files", "search_in_files(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...])", new ExpressionFactoryHelper<SearchInFilesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grep_files", "grep_files(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...])", new ExpressionFactoryHelper<SearchInFilesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grepfiles", "grepfiles(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...])", false, new ExpressionFactoryHelper<SearchInFilesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_in_files_as_list", "search_in_files_as_list(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, flattened across files, supports LINQ such as .where($$.FilePath.EndsWith(\"cs\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<SearchInFilesAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grep_dir", "grep_dir(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...])", false, new ExpressionFactoryHelper<SearchInFilesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grep_files_as_list", "grep_files_as_list(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, flattened across files, supports LINQ such as .where($$.FilePath.EndsWith(\"cs\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<SearchInFilesAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("grepfilesaslist", "grepfilesaslist(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, flattened across files, supports LINQ such as .where($$.FilePath.EndsWith(\"cs\"))), use 'to_string' to convert to a string", false, new ExpressionFactoryHelper<SearchInFilesAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_in_files_with_encoding", "search_in_files_with_encoding(path, regex_pattern, encoding[, context_lines_after, context_lines_before, filter_list_or_str_1, ...])", new ExpressionFactoryHelper<SearchInFilesWithEncodingExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("search_in_files_as_list_with_encoding", "search_in_files_as_list_with_encoding(path, regex_pattern, encoding[, context_lines_after, context_lines_before, filter_list_or_str_1, ...]) return List of MatchBlock(FilePath/StartLine/EndLine/MatchedCount/Text fields, flattened across files, supports LINQ such as .where($$.FilePath.EndsWith(\"cs\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<SearchInFilesAsListWithEncodingExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("count_file_indentations", "count_file_indentations(path[, startLine, endLine, encoding]) - display lines with line number, indent info, and content", new ExpressionFactoryHelper<CountFileIndentationsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("count_indentations", "count_indentations(path[, startLine, endLine, encoding]) - display lines with line number, indent info, and content", false, new ExpressionFactoryHelper<CountFileIndentationsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("countindentations", "countindentations(path[, startLine, endLine, encoding]) - display lines with line number, indent info, and content", false, new ExpressionFactoryHelper<CountFileIndentationsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("count_file_indentation", "count_file_indentation(path)", false, new ExpressionFactoryHelper<CountFileIndentationsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("count_indentation", "count_indentation(path)", false, new ExpressionFactoryHelper<CountFileIndentationsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("countindentation", "countindentation(path)", false, new ExpressionFactoryHelper<CountFileIndentationsExp>());

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
            AgentFrameworkService.Instance.DslEngine!.Register("fetch", "fetch(url) or fetch(url, headers)", new ExpressionFactoryHelper<HttpGetExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("http_get", "http_get(url) or http_get(url, headers)", new ExpressionFactoryHelper<HttpGetExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("http_post", "http_post(url, content) or http_post(url, content, contentType) or http_post(url, content, contentType, headers)", new ExpressionFactoryHelper<HttpPostExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("download_file", "download_file(url, savePath) or download_file(url, savePath, headers)", new ExpressionFactoryHelper<DownloadFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_http_user_agent", "set_http_user_agent(user_agent) - set User-Agent header for http_get/http_post/download_file", new ExpressionFactoryHelper<SetHttpUserAgentExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_http_user_agent", "get_http_user_agent() - get current User-Agent header string", new ExpressionFactoryHelper<GetHttpUserAgentExp>());

            // JSON Operations
            AgentFrameworkService.Instance.DslEngine!.Register("to_json", "to_json(obj, prettyPrint)", new ExpressionFactoryHelper<ToJsonExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("array_to_json", "array_to_json(array)", false, new ExpressionFactoryHelper<ToJsonExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_to_json", "list_to_json(list)", false, new ExpressionFactoryHelper<ToJsonExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_to_json", "hashtable_to_json(hashtable)", false, new ExpressionFactoryHelper<ToJsonExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("from_json", "from_json(json)", new ExpressionFactoryHelper<FromJsonExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("json_escape", "json_escape(str[, bool_add_quotes])", new ExpressionFactoryHelper<JsonEscapeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("escape_json", "escape_json(str[, bool_add_quotes])", false, new ExpressionFactoryHelper<JsonEscapeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("escape_json_string", "escape_json_string(str[, bool_add_quotes])", false, new ExpressionFactoryHelper<JsonEscapeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("escape_json_str", "escape_json_str(str[, bool_add_quotes])", false, new ExpressionFactoryHelper<JsonEscapeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("json_unescape", "json_unescape(str)", new ExpressionFactoryHelper<JsonUnescapeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("unescape_json", "unescape_json(str)", false, new ExpressionFactoryHelper<JsonUnescapeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("unescape_json_string", "unescape_json_string(str)", false, new ExpressionFactoryHelper<JsonUnescapeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("unescape_json_str", "unescape_json_str(str)", false, new ExpressionFactoryHelper<JsonUnescapeExp>());

            AgentFrameworkService.Instance.DslEngine!.Register("new_object", "new_object(key1, value1, key2, value2, ...)", new ExpressionFactoryHelper<NewObjectExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("array_to_string", "array_to_string(array)", false, new ExpressionFactoryHelper<ToStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_to_string", "list_to_string(list)", false, new ExpressionFactoryHelper<ToStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_to_string", "hashtable_to_string(hashtable)", false, new ExpressionFactoryHelper<ToStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("to_string", "to_string(val)", new ExpressionFactoryHelper<ToStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("tostring", "tostring(val)", new ExpressionFactoryHelper<ToStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("to_str", "to_str(val)", false, new ExpressionFactoryHelper<ToStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("tostr", "tostr(val)", false, new ExpressionFactoryHelper<ToStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("to_pretty_string", "to_pretty_string(val) - convert to a more readable form by unescaping escape sequences (\\n \\r \\t \\\" \\\\ \\uXXXX \\xH...) into their actual characters", new ExpressionFactoryHelper<ToPrettyStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("toprettystring", "toprettystring(val) - convert to a more readable form by unescaping escape sequences (\\n \\r \\t \\\" \\\\ \\uXXXX \\xH...) into their actual characters", new ExpressionFactoryHelper<ToPrettyStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("to_pretty_str", "to_pretty_str(val) - convert to a more readable form by unescaping escape sequences (\\n \\r \\t \\\" \\\\ \\uXXXX \\xH...) into their actual characters", false, new ExpressionFactoryHelper<ToPrettyStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("toprettystr", "toprettystr(val) - convert to a more readable form by unescaping escape sequences (\\n \\r \\t \\\" \\\\ \\uXXXX \\xH...) into their actual characters", false, new ExpressionFactoryHelper<ToPrettyStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_length", "string_length(str)", new ExpressionFactoryHelper<StringLengthExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_length", "str_length(str)", false, new ExpressionFactoryHelper<StringLengthExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_starts_with", "string_starts_with(str, substr)", new ExpressionFactoryHelper<StringStartsWithExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_starts_with", "str_starts_with(str, substr)", false, new ExpressionFactoryHelper<StringStartsWithExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_ends_with", "string_ends_with(str, substr)", new ExpressionFactoryHelper<StringEndsWithExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_ends_with", "str_ends_with(str, substr)", false, new ExpressionFactoryHelper<StringEndsWithExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("extract_tags", "extract_tags(txt, tag_name[, max_num]) - extract XML-style tag contents like <a>x|y</a>/<a></a>/<a/>, content split by '|', case-sensitive, non-greedy. Returns IList<IList<string>>. max_num<=0 means no limit.", new ExpressionFactoryHelper<ExtractTagsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("extracttags", "extracttags(txt, tag_name[, max_num])", false, new ExpressionFactoryHelper<ExtractTagsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("extract_tag_codes", "extract_tag_codes(txt, tag_name[, max_num]) - extract XML-style tag contents like <a>x</a>/<a></a>/<a/>, case-sensitive, non-greedy. Returns List<BoxedValue> of raw string content per match. max_num<=0 means no limit.", new ExpressionFactoryHelper<ExtractTagCodesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("extracttagcodes", "extracttagcodes(txt, tag_name[, max_num])", false, new ExpressionFactoryHelper<ExtractTagCodesExp>());

            // HTML/URL Encode Operations
            AgentFrameworkService.Instance.DslEngine!.Register("html_encode", "html_encode(html_str)", new ExpressionFactoryHelper<HtmlEncodeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("html_decode", "html_decode(encoded_html_str)", new ExpressionFactoryHelper<HtmlDecodeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("url_encode", "url_encode(url_str)", new ExpressionFactoryHelper<UrlEncodeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("url_decode", "url_decode(encoded_url_str)", new ExpressionFactoryHelper<UrlDecodeExp>());

            // Process Operations
            AgentFrameworkService.Instance.DslEngine!.Register("execute_script", "execute_script([language, workingDir, timeout_def_30000ms, cmd_and_args])[bindings($a,$b,...)delimiter(begin_template_code_chars,end_template_code_chars)]{: script_code :}; return Object(success/exitCode/output/error/executionTime), use 'to_string' to convert to a string. The default template code delimiters are \"{%{{{#\" and \"%}}}#}\"; specifically, {% %} and {{ }} serve as the template code brackets, while {# #} denote template code comments.", new ExpressionFactoryHelper<ExecuteScriptExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("execute_script_callback", "execute_script_callback('command_callback'[, language, workingDir, timeout_def_30000ms, cmd_and_args])[bindings($a,$b,...)delimiter(begin_template_code_chars,end_template_code_chars)]{: script_code :}; - async exec, result via command_callback. The default template code delimiters are \"{%{{{#\" and \"%}}}#}\"; specifically, {% %} and {{ }} serve as the template code brackets, while {# #} denote template code comments.", new ExpressionFactoryHelper<ExecuteScriptCallbackExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("execute_command", "execute_command(command[, arguments, workingDir, timeout_def_30000ms])[bindings($a,$b,...)delimiter(begin_chars,end_chars)] return Object(success/exitCode/output/error/executionTime), use 'to_string' to convert to a string", new ExpressionFactoryHelper<ExecuteCommandExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("execute_command_callback", "execute_command_callback('command_callback', command[, arguments, workingDir, timeout_def_30000ms])[bindings($a,$b,...)delimiter(begin_chars,end_chars)] - async exec, result via command_callback", new ExpressionFactoryHelper<ExecuteCommandCallbackExp>());
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
            AgentFrameworkService.Instance.DslEngine!.Register("append_string", "append_string(stringbuilder, val)", false, new ExpressionFactoryHelper<AppendExp>());

            AgentFrameworkService.Instance.DslEngine!.Register("string_index_of", "string_index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_last_index_of", "string_last_index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_index_of_any", "string_index_of_any(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_last_index_of_any", "string_last_index_of_any(str, substr[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_index_of", "str_index_of(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_last_index_of", "str_last_index_of(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_index_of_any", "str_index_of_any(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_last_index_of_any", "str_last_index_of_any(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_concat", "string_concat(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_concat", "str_concat(str1, str2, ...)", false, new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("concat_string", "concat_string(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("concat_str", "concat_str(str1, str2, ...)", false, new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("char_to_int", "char_to_int(char_str)", new ExpressionFactoryHelper<CharToIntExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("int_to_char", "int_to_char(int_ascii)", new ExpressionFactoryHelper<IntToCharExp>());

            // alias for LLM Imaginary
            AgentFrameworkService.Instance.DslEngine!.Register("string_find", "string_find(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stringfind", "stringfind(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_string", "find_string(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("findstring", "findstring(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_in_string", "find_in_string(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_find", "str_find(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("strfind", "strfind(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_str", "find_str(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("findstr", "findstr(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_in_str", "find_in_str(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("index_of", "index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_index_of", "find_index_of(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("last_index_of", "last_index_of(str, substr[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_last_index_of", "find_last_index_of(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("index_of_any", "index_of_any(str, char_list[, start, count])", new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_index_of_any", "find_index_of_any(str, char_list[, start, count])", new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("last_index_of_any", "last_index_of_any(str, char_list[, start, count])", new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_last_index_of_any", "find_last_index_of_any(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
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
            AgentFrameworkService.Instance.DslEngine!.Register("str_indexof", "str_indexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_last_indexof", "str_last_indexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_indexof_any", "str_indexof_any(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_last_indexof_any", "str_last_indexof_any(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("strindexof", "strindexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("strlastindexof", "strlastindexof(str, substr[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("strindexofany", "strindexofany(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("strlastindexofany", "strlastindexofany(str, char_list[, start, count])", false, new ExpressionFactoryHelper<StringLastIndexOfAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("strlength", "strlength(val)", false, new ExpressionFactoryHelper<StringLengthExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("length", "length(str) or length(list) or length(hashtable)", new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("len", "len(str) or len(list) or len(hashtable)", new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("size", "size(str) or size(list) or size(hashtable)", new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("count", "count(str) or count(list) or count(hashtable)", new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("size_of", "size_of(str) or size_of(list) or size_of(hashtable)", false, new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_count", "get_count(str) or get_count(list) or get_count(hashtable)", false, new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_size", "get_size(str) or get_size(list) or get_size(hashtable)", false, new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_length", "get_length(str) or get_length(list) or get_length(hashtable)", false, new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("array_size", "array_size(list)", false, new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("array_count", "array_count(list)", false, new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("array_length", "array_length(list)", false, new ExpressionFactoryHelper<SizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_contains", "list_contains(list,val)", new ExpressionFactoryHelper<ListContainsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_contains", "hashtable_contains(hash,val)", new ExpressionFactoryHelper<HashtableContainsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_builder_length", "string_builder_length(sb)", new ExpressionFactoryHelper<StringBuilderLengthExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_contains_key", "hashtable_contains_key(hash,key)", false, new ExpressionFactoryHelper<HashtableContainsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stringbuilder_length", "stringbuilder_length(sb)", false, new ExpressionFactoryHelper<StringBuilderLengthExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stringbuilderlength", "stringbuilderlength(sb)", false, new ExpressionFactoryHelper<StringBuilderLengthExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("char_code_at", "char_code_at(str, index)", new ExpressionFactoryHelper<CharCodeAtExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("charcodeat", "charcodeat(str, index)", false, new ExpressionFactoryHelper<CharCodeAtExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("array_to_list", "array_to_list(array[, index, count])", new ExpressionFactoryHelper<ArrayToListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("arraytolist", "arraytolist(array[, index, count])", false, new ExpressionFactoryHelper<ArrayToListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_to_array", "list_to_array(list[, index, count])", new ExpressionFactoryHelper<ListToArrayExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("listtoarray", "listtoarray(list[, index, count])", false, new ExpressionFactoryHelper<ListToArrayExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("sublist", "sublist(list[, index, count])", new ExpressionFactoryHelper<SubListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_slice", "list_slice(list[, index, count])", false, new ExpressionFactoryHelper<SubListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_get_range", "list_get_range(list[, index, count])", false, new ExpressionFactoryHelper<SubListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("subarray", "subarray(array[, index, count])", new ExpressionFactoryHelper<SubArrayExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("array_slice", "array_slice(array[, index, count])", false, new ExpressionFactoryHelper<SubArrayExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("array_get_range", "array_get_range(array[, index, count])", false, new ExpressionFactoryHelper<SubArrayExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_append", "string_append(str1, str2, ...)", false, new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_append", "str_append(str1, str2, ...)", false, new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("concat", "concat(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("strcat", "strcat(str1, str2, ...)", new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_cat", "str_cat(str1, str2, ...)", false, new ExpressionFactoryHelper<StringConcatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("strlen", "strlen(val)", new ExpressionFactoryHelper<StringLengthExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_len", "str_len(val)", false, new ExpressionFactoryHelper<StringLengthExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("starts_with", "starts_with(str, substr)", new ExpressionFactoryHelper<StringStartsWithExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("ends_with", "ends_with(str, substr)", new ExpressionFactoryHelper<StringEndsWithExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("ensure_dir", "ensure_dir(path)", new ExpressionFactoryHelper<EnsureDirectoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("ensure_directory_exists", "ensure_directory_exists(path)", false, new ExpressionFactoryHelper<EnsureDirectoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("ensure_directory", "ensure_directory(path)", new ExpressionFactoryHelper<EnsureDirectoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("remove_dir", "remove_dir(path)", new ExpressionFactoryHelper<RemoveDirectoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("remove_directory", "remove_directory(path)", new ExpressionFactoryHelper<RemoveDirectoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hotreload", "hotreload()", false, new ExpressionFactoryHelper<HotReloadExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("callskill", "callskill(skill_name, arg1, arg2, ...)", false, new ExpressionFactoryHelper<CallSkillExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("refreshskills", "refreshskills()", false, new ExpressionFactoryHelper<RefreshSkillsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("refreshembedding", "refreshembedding()", false, new ExpressionFactoryHelper<RefreshEmbeddingExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("refreshreranker", "refreshreranker()", false, new ExpressionFactoryHelper<RefreshRerankExp>());

            AgentFrameworkService.Instance.DslEngine!.Register("name_contains", "name_contains(str,str_or_list_1,str_or_list_2,...)", false, new ExpressionFactoryHelper<FrameworkApiAlias.StringContainsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("name_not_contains", "name_not_contains(str,str_or_list_1,str_or_list_2,...)", false, new ExpressionFactoryHelper<FrameworkApiAlias.StringNotContainsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("name_contains_any", "name_contains_any(str,str_or_list_1,str_or_list_2,...)", false, new ExpressionFactoryHelper<FrameworkApiAlias.StringContainsAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("name_not_contains_any", "name_not_contains_any(str,str_or_list_1,str_or_list_2,...)", false, new ExpressionFactoryHelper<FrameworkApiAlias.StringNotContainsAnyExp>());

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

