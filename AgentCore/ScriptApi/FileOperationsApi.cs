using System;
using AgentPlugin.Abstractions;
using System.Collections;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;
using CefDotnetApp.AgentCore.Utils;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // read_file(path) - read file content
    sealed class ReadFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: read_file(path)");
                return BoxedValue.NullObject;
            }

            try {
                string path = operands[0].AsString;
                string content = Core.AgentCore.Instance.FileOps.ReadFile(path);
                return BoxedValue.FromString(content ?? string.Empty);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"readfile error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // write_file(path, content) - write file content
    sealed class WriteFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: append_file(path, content)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                string content = operands[1].AsString;
                if (string.IsNullOrEmpty(content)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("You cannot append empty values 鈥嬧€媡o a file !!! To delete certain lines, use the 'delete_lines' function.");
                    return BoxedValue.From(false);
                }
                string ext = Path.GetExtension(path).ToLower();
                if (File.Exists(path) && ext != ".txt" && ext != ".md") {
                    var lineCount = File.ReadAllLines(path).Length;
                    var newLineCount = path.Split('\n').Length;
                    if (lineCount > newLineCount + Core.AgentCore.Instance.MaxLinesDeletedByWriteFile) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("You cannot significantly reduce code using 'write_file' !!! To delete certain lines, use the 'delete_lines' function.");
                        return BoxedValue.From(false);
                    }
                }
                bool result = Core.AgentCore.Instance.FileOps.WriteFile(path, content);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"writefile error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // append_file(path, content) - append to file
    sealed class AppendFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: write_file(path, content)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                string content = operands[1].AsString;
                if (string.IsNullOrEmpty(content)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("You cannot append empty values to a file !!!");
                    return BoxedValue.From(false);
                }
                bool result = Core.AgentCore.Instance.FileOps.AppendFile(path, content);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"appendfile error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // copy_file(sourcePath, destPath, overwrite) - copy file
    sealed class CopyFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: copy_file(sourcePath, destPath, overwrite)");
                return BoxedValue.From(false);
            }

            try {
                string sourcePath = operands[0].AsString;
                string destPath = operands[1].AsString;
                bool overwrite = operands.Count > 2 ? operands[2].GetBool() : false;
                bool result = Core.AgentCore.Instance.FileOps.CopyFile(sourcePath, destPath, overwrite);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"copyfile error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // move_file(sourcePath, destPath, overwrite) - move file
    sealed class MoveFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: move_file(sourcePath, destPath, overwrite)");
                return BoxedValue.From(false);
            }

            try {
                string sourcePath = operands[0].AsString;
                string destPath = operands[1].AsString;
                bool overwrite = operands.Count > 2 ? operands[2].GetBool() : false;
                bool result = Core.AgentCore.Instance.FileOps.MoveFile(sourcePath, destPath, overwrite);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"movefile error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // file_exists(path) - check if file exists
    sealed class FileExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: file_exists(path), aliased as file_exist");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                bool result = Core.AgentCore.Instance.FileOps.FileExists(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"fileexists error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // dir_exists(path) - check if directory exists
    sealed class DirExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: dir_exists(path), aliased as dir_exist");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                bool result = Core.AgentCore.Instance.FileOps.DirectoryExists(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"direxists error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // list_dir_info(path, glob_pattern, recursive) - list directory contents
    sealed class ListDirInfoExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_dir_info(path, glob_pattern, recursive)");
                return BoxedValue.NullObject;
            }

            try {
                string path = operands[0].AsString;
                string pattern = operands.Count > 1 ? operands[1].AsString : "*";
                bool recursive = operands.Count > 2 ? operands[2].GetBool() : false;

                var files = Core.AgentCore.Instance.FileOps.ListDirectory(path, pattern, recursive);
                var result = new List<BoxedValue>();

                foreach (var file in files) {
                    result.Add(BoxedValue.FromString(file.Path));
                }

                return BoxedValue.FromObject(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"listdir error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // search_log_file(log_file, search_regex[, context_lines_after, context_lines_before]) - search log file with regex pattern
    sealed class SearchLogFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: search_log_file(log_file, search_regex[, context_lines_after, context_lines_before]), aliased as grep_log_file");
                return BoxedValue.FromString("Expected: search_log_file(log_file, search_regex[, context_lines_after, context_lines_before])");
            }

            try {
                string logFile = operands[0].AsString;
                string searchRegex = operands[1].AsString;
                int contextLinesAfter = operands.Count > 2 ? operands[2].GetInt() : 5;
                int contextLinesBefore = operands.Count > 3 ? operands[3].GetInt() : 0;
                string result = Core.AgentCore.Instance.FileOps.SearchLogFile(logFile, searchRegex, contextLinesAfter, contextLinesBefore);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"search_log_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }

    // tail_log_file(log_file, lines) - get last N lines from log file
    sealed class TailLogFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: tail_log_file(log_file, lines)");
                return BoxedValue.FromString("Expected: tail_log_file(log_file, lines)");
            }

            try {
                string logFile = operands[0].AsString;
                int lines = operands.Count > 1 ? operands[1].GetInt() : 100;
                string result = Core.AgentCore.Instance.FileOps.TailLogFile(logFile, lines);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"tail_log_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }
}
