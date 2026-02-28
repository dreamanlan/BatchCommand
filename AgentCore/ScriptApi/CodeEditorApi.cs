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

    // insert_after(path, searchLiteralText, content[, allOccurrences[, exactMatch]]) - insert content after literal text
    sealed class InsertAfterExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 5) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: insert_after(path, searchLiteralText, content[, allOccurrences[, exactMatch]]), aliased as insert_file_after");
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
                bool result = Core.AgentCore.Instance.FileOps.InsertAfter(path, searchLiteralText, content, allOccurrences, exactMatch);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"insertafter error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // insert_before(path, searchLiteralText, content[, allOccurrences[, exactMatch]]) - insert content before literal text
    sealed class InsertBeforeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 5) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: insert_before(path, searchLiteralText, content[, allOccurrences[, exactMatch]]), aliased as insert_file_before");
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
                bool result = Core.AgentCore.Instance.FileOps.InsertBefore(path, searchLiteralText, content, allOccurrences, exactMatch);
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: delete_lines(path, startLine, endLine), aliased as delete_file_lines");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"searchfile error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: read_lines(path, startLine, endLine), aliased as read_file_lines");
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

    // search_file(path, regex_pattern[, context_lines_after, context_lines_before]) - search file with regex pattern and return matches with context
    sealed class SearchFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: search_file(path, regex_pattern[, context_lines_after, context_lines_before]), aliased as searchfile|grep_file|grepfile");
                return BoxedValue.FromString("Expected: search_file(path, regex_pattern[, context_lines_after, context_lines_before])");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"search_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }
}
