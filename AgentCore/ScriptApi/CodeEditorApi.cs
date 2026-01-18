using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // replace_file(path, oldString, newString, allOccurrences) - replace string in file
    sealed class ReplaceFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
                return BoxedValue.From(false);

            try {
                string path = operands[0].AsString;
                string oldString = operands[1].AsString;
                string newString = operands[2].AsString;
                bool allOccurrences = operands.Count > 3 ? operands[3].GetBool() : false;

                if (string.IsNullOrEmpty(oldString)) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("The search string cannot be empty !!!");
                    return BoxedValue.From(false);
                }
                if (oldString.IndexOf('\n') >= 0) {
                    bool ret = Core.AgentCore.Instance.DiffOps.ReplaceFullLinesTextInFile(path, oldString, newString, allOccurrences);
                    if (ret) {
                        return BoxedValue.From(true);
                    }
                    else {
                        DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("Try using 'apply_diff' to replace multi-line text !!!");
                        return BoxedValue.From(false);
                    }
                }
                bool result = Core.AgentCore.Instance.FileOps.ReplaceInFile(path, oldString, newString, allOccurrences);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"replacefile error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // replace_range(path, startLine, endLine, newContent) - replace line range
    sealed class ReplaceRangeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 4)
                return BoxedValue.From(false);

            try {
                string path = operands[0].AsString;
                int startLine = operands[1].GetInt();
                int endLine = operands[2].GetInt();
                string newContent = operands[3].AsString;

                bool result = Core.AgentCore.Instance.FileOps.ReplaceLines(path, startLine, endLine, newContent);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"replacerange error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // insert_after(path, searchLiteralText, content, allOccurrences) - insert content after literal text
    sealed class InsertAfterExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
                return BoxedValue.From(false);

            try {
                string path = operands[0].AsString;
                string searchLiteralText = operands[1].AsString;
                string content = operands[2].AsString;
                bool allOccurrences = operands.Count > 3 ? operands[3].GetBool() : false;

                if (string.IsNullOrEmpty(searchLiteralText)) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("The search string cannot be empty !!!");
                    return BoxedValue.From(false);
                }
                if (searchLiteralText.IndexOf('\n') >= 0) {
                    bool ret = Core.AgentCore.Instance.DiffOps.InsertAfterFullLinesTextInFile(path, searchLiteralText, content, allOccurrences);
                    if (ret) {
                        return BoxedValue.From(true);
                    }
                    else {
                        DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("Try using 'apply_diff' to insert multi-line text !!!");
                        return BoxedValue.From(false);
                    }
                }
                bool result = Core.AgentCore.Instance.FileOps.InsertAfter(path, searchLiteralText, content, allOccurrences);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"insertafter error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // insert_before(path, searchLiteralText, content, allOccurrences) - insert content before literal text
    sealed class InsertBeforeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
                return BoxedValue.From(false);

            try {
                string path = operands[0].AsString;
                string searchLiteralText = operands[1].AsString;
                string content = operands[2].AsString;
                bool allOccurrences = operands.Count > 3 ? operands[3].GetBool() : false;

                if (string.IsNullOrEmpty(searchLiteralText)) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("The search string cannot be empty !!!");
                    return BoxedValue.From(false);
                }
                if (searchLiteralText.IndexOf('\n') >= 0) {
                    bool ret = Core.AgentCore.Instance.DiffOps.InsertBeforeFullLinesTextInFile(path, searchLiteralText, content, allOccurrences);
                    if (ret) {
                        return BoxedValue.From(true);
                    }
                    else {
                        DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("Try using 'apply_diff' to insert multi-line text !!!");
                        return BoxedValue.From(false);
                    }
                }
                bool result = Core.AgentCore.Instance.FileOps.InsertBefore(path, searchLiteralText, content, allOccurrences);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"insertbefore error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // delete_lines(path, startLine, endLine) - delete line range
    sealed class DeleteLinesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
                return BoxedValue.From(false);

            try {
                string path = operands[0].AsString;
                int startLine = operands[1].GetInt();
                int endLine = operands[2].GetInt();

                bool result = Core.AgentCore.Instance.FileOps.DeleteLines(path, startLine, endLine);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"deletelines error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // search_lines_in_file(path, regex_pattern) - search regex_pattern line number in file (auto fallback to substring if regex invalid)
    sealed class SearchLinesInFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

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
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"searchfile error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // read_lines(path, startLine, endLine) - read line range from file
    sealed class ReadLinesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

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
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"readlines error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // get_line_count(path) - get line count of file
    sealed class GetLineCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(0);

            try {
                string path = operands[0].AsString;
                int count = Core.AgentCore.Instance.FileOps.GetLineCount(path);
                return BoxedValue.From(count);
            }
            catch (Exception ex) {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"getlinecount error: {ex.Message}");
                return BoxedValue.From(0);
            }
        }
    }

    // search_file(path, regex_pattern[, context_lines_after, context_lines_before]) - search file with regex pattern and return matches with context
    sealed class SearchFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.FromString("Usage: search_file(path, regex_pattern[, context_lines_after, context_lines_before])");

            try {
                string path = operands[0].AsString;
                string pattern = operands[1].AsString;
                int contextLinesAfter = operands.Count > 2 ? operands[2].GetInt() : 5;
                int contextLinesBefore = operands.Count > 3 ? operands[3].GetInt() : 0;
                string result = Core.AgentCore.Instance.FileOps.SearchFile(path, pattern, contextLinesAfter, contextLinesBefore);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"search_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }
}
