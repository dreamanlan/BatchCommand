using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // replace_in_file(path, oldString, newString[, replaceAll[, exactMatch]]) - replace string in file
    sealed class ReplaceInFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 5) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: replace_in_file(path, oldString, newString[, replaceAll[, exactMatch]])");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                string oldString = operands[1].AsString;
                string newString = operands[2].AsString;
                bool replaceAll = operands.Count > 3 ? operands[3].GetBool() : false;
                bool exactMatch = operands.Count > 4 ? operands[4].GetBool() : false;

                if (string.IsNullOrEmpty(oldString)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("replace_in_file: oldString cannot be empty");
                    return BoxedValue.From(false);
                }
                bool result = Core.AgentCore.Instance.FileOps.ReplaceInFile(path, oldString, newString, replaceAll, exactMatch);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"replace_in_file error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // multi_replace(path, editsJson) - perform multiple replacements sequentially
    // editsJson: [{"old_string":"...","new_string":"...","replace_all":false}, ...]
    sealed class MultiReplaceExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine(
                    "Expected: multi_replace(path, editsJson) editsJson=[{\"old_string\":\"...\",\"new_string\":\"...\",\"replace_all\":false,\"exact_match\":false},...]");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                string editsJson = operands[1].AsString;

                var edits = LitJson.JsonMapper.ToObject<LitJson.JsonData>(editsJson);
                if (edits == null || !edits.IsArray || edits.Count == 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("multi_replace: editsJson must be a non-empty JSON array");
                    return BoxedValue.From(false);
                }

                string fullPath = CefDotnetApp.AgentCore.Utils.PathHelper.EnsureAbsolutePath(path, Core.AgentCore.Instance.BasePath);
                if (!System.IO.File.Exists(fullPath)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"multi_replace: file not found: {path}");
                    return BoxedValue.From(false);
                }

                string content = System.IO.File.ReadAllText(fullPath, System.Text.Encoding.UTF8);

                for (int i = 0; i < edits.Count; i++) {
                    var edit = edits[i];
                    if (!edit.Keys.Contains("old_string") || !edit.Keys.Contains("new_string")) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"multi_replace: edit[{i}] missing old_string or new_string");
                        return BoxedValue.From(false);
                    }

                    string oldString = (string)edit["old_string"];
                    string newString = (string)edit["new_string"];
                    bool replaceAll = edit.Keys.Contains("replace_all") ? (bool)edit["replace_all"] : false;
                    bool exactMatch = edit.Keys.Contains("exact_match") ? (bool)edit["exact_match"] : false;

                    if (string.IsNullOrEmpty(oldString)) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"multi_replace: edit[{i}] old_string cannot be empty");
                        return BoxedValue.From(false);
                    }

                    // Level 1: Exact match
                    if (replaceAll) {
                        if (content.Contains(oldString)) {
                            content = content.Replace(oldString, newString);
                            continue;
                        }
                    }
                    else {
                        int idx = content.IndexOf(oldString, StringComparison.Ordinal);
                        if (idx >= 0) {
                            content = content.Substring(0, idx) + newString + content.Substring(idx + oldString.Length);
                            continue;
                        }
                    }
                    if (exactMatch) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"multi_replace: edit[{i}] old_string not found (exact match)");
                        return BoxedValue.From(false);
                    }
                    // Level 2: Trimmed match
                    var trimmedOld = oldString.Trim();
                    var trimmedNew = newString.Trim();
                    if (replaceAll) {
                        if (content.Contains(trimmedOld)) {
                            content = content.Replace(trimmedOld, trimmedNew);
                            continue;
                        }
                    }
                    else {
                        int idx2 = content.IndexOf(trimmedOld, StringComparison.Ordinal);
                        if (idx2 >= 0) {
                            content = content.Substring(0, idx2) + trimmedNew + content.Substring(idx2 + trimmedOld.Length);
                            continue;
                        }
                    }
                    // Level 3: Normalized whitespace matching (DiffOps fallback)
                    var normResult = CefDotnetApp.AgentCore.Core.DiffOperations.ReplaceFullLinesText(content, oldString, newString, replaceAll);
                    if (normResult.Success) {
                        content = normResult.ResultContent;
                        continue;
                    }
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"multi_replace: edit[{i}] old_string not found");
                    return BoxedValue.From(false);
                }

                System.IO.File.WriteAllText(fullPath, content, System.Text.Encoding.UTF8);
                return BoxedValue.From(true);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"multi_replace error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // replace_range(path, startLine, endLine, newContent) - replace line range
    sealed class ReplaceRangeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: replace_range(path, startLine, endLine, newContent)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                int startLine = operands[1].GetInt();
                int endLine = operands[2].GetInt();
                string newContent = operands[3].AsString;

                bool result = Core.AgentCore.Instance.FileOps.ReplaceLines(path, startLine, endLine, newContent);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"replacerange error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // insert_after_text(path, searchLiteralText, content[, allOccurrences[, exactMatch]]) - insert content after literal text
    sealed class InsertAfterTextExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 5) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: insert_after_text(path, searchLiteralText, content[, allOccurrences[, exactMatch]])");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                string searchLiteralText = operands[1].AsString;
                string content = operands[2].AsString;
                bool allOccurrences = operands.Count > 3 ? operands[3].GetBool() : false;
                bool exactMatch = operands.Count > 4 ? operands[4].GetBool() : false;

                if (string.IsNullOrEmpty(searchLiteralText)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("The search string cannot be empty !!!");
                    return BoxedValue.From(false);
                }
                bool result = Core.AgentCore.Instance.FileOps.InsertAfterText(path, searchLiteralText, content, allOccurrences, exactMatch);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"insertafter error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // insert_before_text(path, searchLiteralText, content[, allOccurrences[, exactMatch]]) - insert content before literal text
    sealed class InsertBeforeTextExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 5) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: insert_before_text(path, searchLiteralText, content[, allOccurrences[, exactMatch]])");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                string searchLiteralText = operands[1].AsString;
                string content = operands[2].AsString;
                bool allOccurrences = operands.Count > 3 ? operands[3].GetBool() : false;
                bool exactMatch = operands.Count > 4 ? operands[4].GetBool() : false;

                if (string.IsNullOrEmpty(searchLiteralText)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("The search string cannot be empty !!!");
                    return BoxedValue.From(false);
                }
                bool result = Core.AgentCore.Instance.FileOps.InsertBeforeText(path, searchLiteralText, content, allOccurrences, exactMatch);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"insertbefore error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // delete_lines(path, startLine, endLine) - delete line range
    sealed class DeleteLinesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: delete_lines(path, startLine, endLine)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                int startLine = operands[1].GetInt();
                int endLine = operands[2].GetInt();

                bool result = Core.AgentCore.Instance.FileOps.DeleteLines(path, startLine, endLine);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"deletelines error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // search_lines_in_file(path, regex_pattern) - search regex_pattern line number in file (auto fallback to substring if regex invalid)
    sealed class SearchLinesInFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: search_lines(path, regex_pattern, [ignoreCase]), aliased as search_lines_in_file");
                return BoxedValue.NullObject;
            }

            try {
                string path = operands[0].AsString;
                string pattern = operands[1].AsString;
                bool ignoreCase = operands.Count > 2 ? operands[2].GetBool() : true;

                var lines = Core.AgentCore.Instance.FileOps.SearchLinesInFile(path, pattern, ignoreCase);
                var result = new List<object>();

                foreach (var line in lines) {
                    result.Add(line);
                }

                return BoxedValue.FromObject(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"search_lines error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // read_lines(path, startLine, endLine) - read line range from file
    sealed class ReadLinesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: read_lines(path, startLine, endLine)");
                return BoxedValue.NullObject;
            }

            try {
                string path = operands[0].AsString;
                int startLine = operands.Count > 1 ? operands[1].GetInt() : 1;
                int endLine = operands.Count > 2 ? operands[2].GetInt() : -1;

                if (endLine == -1) {
                    // Read all lines
                    string[] lines = Core.AgentCore.Instance.FileOps.ReadFileLines(path);
                    var result = new List<object>();
                    foreach (var line in lines) {
                        result.Add(line);
                    }
                    return BoxedValue.FromObject(result);
                }
                else {
                    string[] lines = Core.AgentCore.Instance.FileOps.ReadLinesRange(path, startLine, endLine);
                    var result = new List<object>();
                    foreach (var line in lines) {
                        result.Add(line);
                    }
                    return BoxedValue.FromObject(result);
                }
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"readlines error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // get_line_count(path) - get line count of file
    sealed class GetLineCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_line_count(path), aliased as get_file_line_count|line_count|file_line_count");
                return BoxedValue.From(0);
            }

            try {
                string path = operands[0].AsString;
                int count = Core.AgentCore.Instance.FileOps.GetLineCount(path);
                return BoxedValue.From(count);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"getlinecount error: {ex.Message}");
                return BoxedValue.From(0);
            }
        }
    }

    // search_in_file(path, regex_pattern[, context_lines_after, context_lines_before]) - search file with regex pattern and return matches with context
    sealed class SearchInFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: search_in_file(path, regex_pattern[, context_lines_after, context_lines_before]), aliased as grep_file|grepfile");
                return BoxedValue.FromString("Expected: search_in_file(path, regex_pattern[, context_lines_after, context_lines_before])");
            }

            try {
                string path = operands[0].AsString;
                string pattern = operands[1].AsString;
                int contextLinesAfter = operands.Count > 2 ? operands[2].GetInt() : 5;
                int contextLinesBefore = operands.Count > 3 ? operands[3].GetInt() : 0;
                string result = Core.AgentCore.Instance.FileOps.SearchFile(path, pattern, contextLinesAfter, contextLinesBefore);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"search_in_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }

    // search_in_files(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...]) - search files with regex pattern and return matches with context
    sealed class SearchInFilesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: search_in_files(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...]), aliased as grep_files|grepfiles");
                return BoxedValue.FromString("Expected: search_in_files(path, regex_pattern[, context_lines_after, context_lines_before, filter_list_or_str_1, ...])");
            }

            try {
                string path = operands[0].AsString;
                string pattern = operands[1].AsString;
                int contextLinesAfter = operands.Count > 2 ? operands[2].GetInt() : 5;
                int contextLinesBefore = operands.Count > 3 ? operands[3].GetInt() : 0;
                List<string>? filterAndNewExts = null;
                if (operands.Count > 4) {
                    filterAndNewExts = new List<string>();
                    for (int i = 4; i < operands.Count; i++) {
                        string str = operands[i].AsString;
                        if (str != null) {
                            filterAndNewExts.Add(str);
                            continue;
                        }
                        var strList = operands[i].As<System.Collections.IList>();
                        if (strList == null) {
                            continue;
                        }
                        foreach (object strObj in strList) {
                            if (strObj is string tempStr) {
                                filterAndNewExts.Add(tempStr);
                            }
                        }
                    }
                }
                string result = Core.AgentCore.Instance.FileOps.SearchFiles(path, pattern, contextLinesAfter, contextLinesBefore, filterAndNewExts);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"search_in_files error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }

    // count_lines(path[, startLine, endLine]) - display file lines with line number, indent info, and left-aligned content
    sealed class CountLinesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: count_lines(path[, startLine, endLine])");
                return BoxedValue.FromString("Expected: count_lines(path[, startLine, endLine])");
            }

            try {
                string path = operands[0].AsString;
                string fullPath = CefDotnetApp.AgentCore.Utils.PathHelper.EnsureAbsolutePath(path, Core.AgentCore.Instance.BasePath);
                if (!System.IO.File.Exists(fullPath)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"count_lines: file not found: {path}");
                    return BoxedValue.FromString($"Error: file not found: {path}");
                }

                string[] allLines = System.IO.File.ReadAllLines(fullPath, System.Text.Encoding.UTF8);
                int totalLines = allLines.Length;
                if (totalLines == 0) {
                    return BoxedValue.FromString("(empty file, 0 lines)");
                }

                int startLine = operands.Count > 1 ? operands[1].GetInt() : 1;
                int endLine = operands.Count > 2 ? operands[2].GetInt() : totalLines;

                // Clamp to file boundary
                if (startLine < 1) startLine = 1;
                if (endLine > totalLines) endLine = totalLines;
                if (startLine > totalLines) startLine = totalLines;
                if (endLine < 1) endLine = 1;
                if (startLine > endLine) {
                    return BoxedValue.FromString($"(no lines in range {startLine}-{endLine}, file has {totalLines} lines)");
                }

                // First pass: compute indent info strings and find max prefix width
                int count = endLine - startLine + 1;
                string[] indentInfos = new string[count];
                int maxPrefixLen = 0;

                for (int i = 0; i < count; i++) {
                    int lineNum = startLine + i;
                    string line = allLines[lineNum - 1];
                    string indentInfo = BuildIndentInfo(line);
                    indentInfos[i] = indentInfo;
                    // Prefix format: "lineNum: indentInfo" or "lineNum:" if no indent
                    string prefix = indentInfo.Length > 0
                        ? $"{lineNum}: {indentInfo}"
                        : $"{lineNum}:";
                    if (prefix.Length > maxPrefixLen)
                        maxPrefixLen = prefix.Length;
                }

                // Second pass: build output with fixed-width prefix
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"[{path}] lines {startLine}-{endLine} of {totalLines}");

                for (int i = 0; i < count; i++) {
                    int lineNum = startLine + i;
                    string line = allLines[lineNum - 1];
                    string indentInfo = indentInfos[i];
                    string prefix = indentInfo.Length > 0
                        ? $"{lineNum}: {indentInfo}"
                        : $"{lineNum}:";
                    sb.Append(prefix.PadRight(maxPrefixLen));
                    sb.Append('\t');
                    sb.AppendLine(line);
                }

                return BoxedValue.FromString(sb.ToString());
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"count_lines error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Build indent info string from leading whitespace.
        /// Counts consecutive segments of tabs and spaces.
        /// E.g. "\t\t   " => " 2 tab  3 spc", "    \t" => " 4 spc  1 tab", "\t" => " 1 tab", "  " => " 2 spc", "" => ""
        /// </summary>
        private static string BuildIndentInfo(string line)
        {
            if (line.Length == 0) return "";

            var sb = new System.Text.StringBuilder();
            int pos = 0;

            while (pos < line.Length) {
                char ch = line[pos];
                if (ch == '\t') {
                    int segStart = pos;
                    while (pos < line.Length && line[pos] == '\t') pos++;
                    if (sb.Length > 0) sb.Append(' ');
                    sb.Append($"{pos - segStart,2} tab");
                }
                else if (ch == ' ') {
                    int segStart = pos;
                    while (pos < line.Length && line[pos] == ' ') pos++;
                    if (sb.Length > 0) sb.Append(' ');
                    sb.Append($"{pos - segStart,2} spc");
                }
                else {
                    break;
                }
            }

            return sb.ToString();
        }
    }

    // insert_after_line(path, line, insert_content) - insert content after specified line number (content starts on a new line)
    sealed class InsertAfterLineExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: insert_after_line(path, line, insert_content), aliased as insert_after");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                int line = operands[1].GetInt();
                string insertContent = operands[2].AsString;

                string fullPath = CefDotnetApp.AgentCore.Utils.PathHelper.EnsureAbsolutePath(path, Core.AgentCore.Instance.BasePath);
                if (!System.IO.File.Exists(fullPath)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"insert_after_line: file not found: {path}");
                    return BoxedValue.From(false);
                }

                var lines = new List<string>(System.IO.File.ReadAllLines(fullPath, System.Text.Encoding.UTF8));
                if (line < 1 || line > lines.Count) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"insert_after_line: line {line} out of range (1-{lines.Count})");
                    return BoxedValue.From(false);
                }

                // Split insert_content by newlines and insert after the specified line
                string[] newLines = insertContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                lines.InsertRange(line, newLines);

                // Detect original line ending
                string originalContent = System.IO.File.ReadAllText(fullPath, System.Text.Encoding.UTF8);
                string lineEnding = originalContent.Contains("\r\n") ? "\r\n" : "\n";
                System.IO.File.WriteAllText(fullPath, string.Join(lineEnding, lines) + (originalContent.EndsWith("\n") ? lineEnding : ""), System.Text.Encoding.UTF8);
                return BoxedValue.From(true);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"insert_after_line error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // insert_before_line(path, line, insert_content) - insert content before specified line number (content starts on a new line)
    sealed class InsertBeforeLineExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: insert_before_line(path, line, insert_content), aliased as insert_before");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                int line = operands[1].GetInt();
                string insertContent = operands[2].AsString;

                string fullPath = CefDotnetApp.AgentCore.Utils.PathHelper.EnsureAbsolutePath(path, Core.AgentCore.Instance.BasePath);
                if (!System.IO.File.Exists(fullPath)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"insert_before_line: file not found: {path}");
                    return BoxedValue.From(false);
                }

                var lines = new List<string>(System.IO.File.ReadAllLines(fullPath, System.Text.Encoding.UTF8));
                if (line < 1 || line > lines.Count) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"insert_before_line: line {line} out of range (1-{lines.Count})");
                    return BoxedValue.From(false);
                }

                // Split insert_content by newlines and insert before the specified line
                string[] newLines = insertContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                lines.InsertRange(line - 1, newLines);

                // Detect original line ending
                string originalContent = System.IO.File.ReadAllText(fullPath, System.Text.Encoding.UTF8);
                string lineEnding = originalContent.Contains("\r\n") ? "\r\n" : "\n";
                System.IO.File.WriteAllText(fullPath, string.Join(lineEnding, lines) + (originalContent.EndsWith("\n") ? lineEnding : ""), System.Text.Encoding.UTF8);
                return BoxedValue.From(true);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"insert_before_line error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
}
