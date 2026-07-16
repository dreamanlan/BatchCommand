using System;
using System.Text;
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
    // read_file(path[, encoding]) - read file content
    sealed class ReadFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: read_file(path[, encoding])");
                return BoxedValue.NullObject;
            }

            try {
                string path = operands[0].AsString;
                Encoding? encoding = operands.Count > 1 ? GetEncoding(operands[1]) : null;
                string content = Core.AgentCore.Instance.FileOps.ReadFile(path, encoding);
                return BoxedValue.FromString(content ?? string.Empty);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"read_file error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // write_file(path, content[, encoding]) - write file content
    sealed class WriteFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: write_file(path, content[, encoding])");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                string content = operands[1].AsString;
                Encoding? encoding = operands.Count > 2 ? GetEncoding(operands[2]) : null;
                if (string.IsNullOrEmpty(content)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("You cannot write empty values to a file !!! To delete certain lines, use the 'delete_lines' function.");
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
                bool result = Core.AgentCore.Instance.FileOps.WriteFile(path, content, true, encoding);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"write_file error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // write_file_no_bom(path, content) - write file content without BOM
    sealed class WriteFileNoBomExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: write_file_no_bom(path, content)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                string content = operands[1].AsString;
                if (string.IsNullOrEmpty(content)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("You cannot write empty content to a file !!!");
                    return BoxedValue.From(false);
                }
                string ext = Path.GetExtension(path).ToLower();
                if (File.Exists(path) && ext != ".txt" && ext != ".md") {
                    var lineCount = File.ReadAllLines(path).Length;
                    var newLineCount = content.Split('\n').Length;
                    if (lineCount > newLineCount + Core.AgentCore.Instance.MaxLinesDeletedByWriteFile) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("You cannot significantly reduce code using 'write_file_no_bom' !!! To delete certain lines, use the 'delete_lines' function.");
                        return BoxedValue.From(false);
                    }
                }
                bool result = Core.AgentCore.Instance.FileOps.WriteFileNoBom(path, content);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"write_file_no_bom error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // read_file_bytes(path) - read file as byte array
    sealed class ReadFileBytesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: read_file_bytes(path)");
                return BoxedValue.NullObject;
            }

            try {
                string path = operands[0].AsString;
                byte[] content = Core.AgentCore.Instance.FileOps.ReadFileBytes(path);
                return BoxedValue.FromObject(content);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"read_file_bytes error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // write_file_bytes(path, bytes) - write byte array to file
    sealed class WriteFileBytesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: write_file_bytes(path, bytes)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                var obj = operands[1].GetObject();
                if (obj is byte[] bytes) {
                    bool result = Core.AgentCore.Instance.FileOps.WriteFileBytes(path, bytes);
                    return BoxedValue.From(result);
                }
                else if (obj is IList<byte> list) {
                    bool result = Core.AgentCore.Instance.FileOps.WriteFileBytes(path, list.ToArray());
                    return BoxedValue.From(result);
                }
                else {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("write_file_bytes: second argument must be a byte array or a byte list");
                    return BoxedValue.From(false);
                }
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"write_file_bytes error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // hex_to_bytes(hexString) - convert hex string to byte array
    sealed class HexToBytesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hex_to_bytes(hexString), aliased as hex_string_to_bytes");
                return BoxedValue.NullObject;
            }

            try {
                string hex = operands[0].AsString;
                // Remove all whitespace (spaces, newlines, tabs)
                hex = hex.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
                if (hex.Length % 2 != 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("hex_to_bytes: hex string length must be even");
                    return BoxedValue.NullObject;
                }
                byte[] bytes = new byte[hex.Length / 2];
                for (int i = 0; i < bytes.Length; i++) {
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                return BoxedValue.FromObject(bytes);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"hex_to_bytes error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // bytes_to_hex(bytes[, bytesPerLine]) - convert byte array to formatted hex string
    sealed class BytesToHexExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: bytes_to_hex(bytes[, bytesPerLine]), aliased as bytes_to_hex_string");
                return BoxedValue.NullObject;
            }

            try {
                var obj = operands[0].GetObject();
                if (obj is not byte[] bytes) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("bytes_to_hex: first argument must be a byte array");
                    return BoxedValue.NullObject;
                }
                int bytesPerLine = operands.Count > 1 ? operands[1].GetInt() : 32;
                if (bytesPerLine <= 0) bytesPerLine = 32;
                var sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) {
                    if (i > 0 && i % bytesPerLine == 0) {
                        sb.AppendLine();
                    }
                    if (i % bytesPerLine != 0) {
                        sb.Append(' ');
                    }
                    sb.Append(bytes[i].ToString("X2"));
                }
                return BoxedValue.FromString(sb.ToString());
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"bytes_to_hex error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // append_file(path, content[, encoding]) - append to file
    sealed class AppendFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: append_file(path, content[, encoding])");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                string content = operands[1].AsString;
                Encoding? encoding = operands.Count > 2 ? GetEncoding(operands[2]) : null;
                if (string.IsNullOrEmpty(content)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("You cannot append empty values to a file !!!");
                    return BoxedValue.From(false);
                }
                bool result = Core.AgentCore.Instance.FileOps.AppendFile(path, content, encoding);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"append_file error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"copy_file error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"move_file error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"file_exists error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // file_not_exists(path) - check if file not exists
    sealed class FileNotExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: file_not_exists(path), aliased as file_not_exist");
                return BoxedValue.From(true);
            }

            try {
                string path = operands[0].AsString;
                bool result = !Core.AgentCore.Instance.FileOps.FileExists(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"file_not_exists error: {ex.Message}");
                return BoxedValue.From(true);
            }
        }
    }

    // file_has_bom(path) - check if file has UTF-8 BOM
    sealed class FileHasBomExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: file_has_bom(path)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                bool result = Core.AgentCore.Instance.FileOps.FileHasBom(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"file_has_bom error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // file_add_bom(path) - add UTF-8 BOM to file (skip if already has BOM)
    sealed class FileAddBomExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: file_add_bom(path)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                bool result = Core.AgentCore.Instance.FileOps.FileAddBom(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"file_add_bom error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // file_remove_bom(path) - remove UTF-8 BOM from file (skip if no BOM)
    sealed class FileRemoveBomExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: file_remove_bom(path)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                bool result = Core.AgentCore.Instance.FileOps.FileRemoveBom(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"file_remove_bom error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"dir_exists error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // dir_not_exists(path) - check if directory exists
    sealed class DirNotExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: dir_not_exists(path), aliased as dir_not_exist");
                return BoxedValue.From(true);
            }

            try {
                string path = operands[0].AsString;
                bool result = !Core.AgentCore.Instance.FileOps.DirectoryExists(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"dir_not_exists error: {ex.Message}");
                return BoxedValue.From(true);
            }
        }
    }

    // path_exists(path) - check if file or directory exists
    sealed class PathExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: path_exists(path), aliased as path_exist");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                return BoxedValue.FromBool(Core.AgentCore.Instance.FileOps.DirectoryExists(path) || Core.AgentCore.Instance.FileOps.FileExists(path));
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"path_exists error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // path_not_exists(path) - check if file or directory exists
    sealed class PathNotExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: path_not_exists(path), aliased as path_not_exist");
                return BoxedValue.From(true);
            }

            try {
                string path = operands[0].AsString;
                return BoxedValue.FromBool(!(Core.AgentCore.Instance.FileOps.DirectoryExists(path) || Core.AgentCore.Instance.FileOps.FileExists(path)));
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"path_not_exists error: {ex.Message}");
                return BoxedValue.From(true);
            }
        }
    }

    // list_dir_info(path[, glob_pattern, recursive]) - list directory contents
    sealed class ListDirInfoExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_dir_info(path[, glob_pattern, recursive])");
                return BoxedValue.NullObject;
            }

            try {
                string path = operands[0].AsString;
                string pattern = operands.Count > 1 ? operands[1].AsString : "*";
                bool recursive = operands.Count > 2 ? operands[2].GetBool() : false;

                var files = Core.AgentCore.Instance.FileOps.ListDirectory(path, pattern, recursive);
                var result = new List<BoxedValue>();

                foreach (var file in files) {
                    result.Add(BoxedValue.FromString(file.ToString()));
                }

                return BoxedValue.FromObject(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"list_dir_info error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    sealed class EnsureDirectoryExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: ensure_directory(dir), aliased as ensure_dir");
                return BoxedValue.NullObject;
            }
            bool ret = false;
            if (operands.Count >= 1) {
                var dir = operands[0].AsString;
                dir = Environment.ExpandEnvironmentVariables(dir);
                AgentCore.Core.AgentCore.Instance.FileOps.EnsureDirectory(dir);
            }
            return ret;
        }
    }
    sealed class RemoveDirectoryExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: remove_directory(dir), aliased as remove_dir");
                return BoxedValue.NullObject;
            }
            bool ret = false;
            if (operands.Count >= 1) {
                var dir = operands[0].AsString;
                dir = Environment.ExpandEnvironmentVariables(dir);
                AgentCore.Core.AgentCore.Instance.FileOps.RemoveDirectory(dir);
            }
            return ret;
        }
    }

    sealed class FindFilesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: find_files(path,glob_pattern[,recursive])");
                return BoxedValue.NullObject;
            }
            string dir = operands[0].AsString;
            string globPattern = operands[1].AsString;
            bool recursive = operands.Count > 2 ? operands[2].GetBool() : true;
            return BoxedValue.FromObject(Core.AgentCore.Instance.FileOps.FindFiles(dir, globPattern, recursive));
        }
    }

    sealed class SearchFilesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: search_files(glob_pattern,path)");
                return BoxedValue.NullObject;
            }
            string globPattern = operands[0].AsString;
            string dir = operands[1].AsString;
            return BoxedValue.FromObject(Core.AgentCore.Instance.FileOps.FindFiles(dir, globPattern));
        }
    }

    // search_log_file(log_file, search_regex[, context_lines_after, context_lines_before, encoding]) - search log file with regex pattern
    sealed class SearchLogFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 5) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: search_log_file(log_file, search_regex[, context_lines_after, context_lines_before, encoding]), aliased as grep_log_file");
                return BoxedValue.FromString("Expected: search_log_file(log_file, search_regex[, context_lines_after, context_lines_before, encoding])");
            }

            try {
                string logFile = operands[0].AsString;
                string searchRegex = operands[1].AsString;
                int contextLinesAfter = operands.Count > 2 ? operands[2].GetInt() : 5;
                int contextLinesBefore = operands.Count > 3 ? operands[3].GetInt() : 0;
                Encoding? encoding = operands.Count > 4 ? GetEncoding(operands[4]) : null;
                string result = Core.AgentCore.Instance.FileOps.SearchLogFile(logFile, searchRegex, contextLinesAfter, contextLinesBefore, encoding);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"search_log_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }

    // search_log_file_as_list(log_file, search_regex[, context_lines_after, context_lines_before, encoding]) - return list of match blocks
    sealed class SearchLogFileAsListExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 5) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: search_log_file_as_list(log_file, search_regex[, context_lines_after, context_lines_before, encoding]), aliased as grep_log_file_as_list");
                return BoxedValue.FromObject(new List<object>());
            }

            try {
                string logFile = operands[0].AsString;
                string searchRegex = operands[1].AsString;
                int contextLinesAfter = operands.Count > 2 ? operands[2].GetInt() : 5;
                int contextLinesBefore = operands.Count > 3 ? operands[3].GetInt() : 0;
                Encoding? encoding = operands.Count > 4 ? GetEncoding(operands[4]) : null;
                var blocks = Core.AgentCore.Instance.FileOps.SearchLogFileAsList(logFile, searchRegex, contextLinesAfter, contextLinesBefore, encoding);
                var result = new List<object>();
                foreach (var b in blocks) {
                    result.Add(b);
                }
                return BoxedValue.FromObject(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"search_log_file_as_list error: {ex.Message}");
                return BoxedValue.FromObject(new List<object>());
            }
        }
    }

    // tail_log_file(log_file, lines[, encoding]) - get last N lines from log file
    sealed class TailLogFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: tail_log_file(log_file, lines[, encoding])");
                return BoxedValue.FromString("Expected: tail_log_file(log_file, lines[, encoding])");
            }

            try {
                string logFile = operands[0].AsString;
                int lines = operands.Count > 1 ? operands[1].GetInt() : 100;
                Encoding? encoding = operands.Count > 2 ? GetEncoding(operands[2]) : null;
                string result = Core.AgentCore.Instance.FileOps.TailLogFile(logFile, lines, encoding);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"tail_log_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }

    // head_log_file(log_file, lines[, encoding]) - get first N lines from log file
    sealed class HeadLogFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: head_log_file(log_file, lines[, encoding])");
                return BoxedValue.FromString("Expected: head_log_file(log_file, lines[, encoding])");
            }

            try {
                string logFile = operands[0].AsString;
                int lines = operands.Count > 1 ? operands[1].GetInt() : 100;
                Encoding? encoding = operands.Count > 2 ? GetEncoding(operands[2]) : null;
                string result = Core.AgentCore.Instance.FileOps.HeadLogFile(logFile, lines, encoding);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"head_log_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }

    // tail_file(file, lines[, encoding]) - get last N lines from file
    sealed class TailFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: tail_file(file, lines[, encoding])");
                return BoxedValue.FromString("Expected: tail_file(file, lines[, encoding])");
            }

            try {
                string file = operands[0].AsString;
                int lines = operands.Count > 1 ? operands[1].GetInt() : 100;
                Encoding? encoding = operands.Count > 2 ? GetEncoding(operands[2]) : null;
                string result = Core.AgentCore.Instance.FileOps.TailFile(file, lines, encoding);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"tail_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }

    // head_file(file, lines[, encoding]) - get first N lines from file
    sealed class HeadFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: head_file(file, lines[, encoding])");
                return BoxedValue.FromString("Expected: head_file(file, lines[, encoding])");
            }

            try {
                string file = operands[0].AsString;
                int lines = operands.Count > 1 ? operands[1].GetInt() : 100;
                Encoding? encoding = operands.Count > 2 ? GetEncoding(operands[2]) : null;
                string result = Core.AgentCore.Instance.FileOps.HeadFile(file, lines, encoding);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"head_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }
    // csv_read(path[, encoding]) - read CSV file into List<List<string>>. minimal RFC4180 subset: comma delimiter, double-quoted fields, "" escape. no cross-line quoted fields.
    sealed class CsvReadExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: csv_read(path[, encoding])");
                return BoxedValue.NullObject;
            }
            try {
                string path = operands[0].AsString;
                Encoding? encoding = operands.Count > 1 ? GetEncoding(operands[1]) : null;
                string content = Core.AgentCore.Instance.FileOps.ReadFile(path, encoding) ?? string.Empty;
                var rows = new List<List<string>>();
                var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (var line in lines) {
                    if (line.Length == 0)
                        continue;
                    var row = new List<string>();
                    var field = new StringBuilder();
                    bool inQuote = false;
                    for (int i = 0; i < line.Length; ++i) {
                        char c = line[i];
                        if (inQuote) {
                            if (c == '"') {
                                if (i + 1 < line.Length && line[i + 1] == '"') {
                                    field.Append('"');
                                    ++i;
                                }
                                else {
                                    inQuote = false;
                                }
                            }
                            else {
                                field.Append(c);
                            }
                        }
                        else {
                            if (c == ',') {
                                row.Add(field.ToString());
                                field.Clear();
                            }
                            else if (c == '"' && field.Length == 0) {
                                inQuote = true;
                            }
                            else {
                                field.Append(c);
                            }
                        }
                    }
                    row.Add(field.ToString());
                    rows.Add(row);
                }
                return BoxedValue.FromObject(rows);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"csv_read error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // read_file_line_range(path, startLine, endLine[, encoding]) - read line range as concatenated string, preserving original newline style
    sealed class ReadFileLineRangeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: read_file_line_range(path, startLine, endLine[, encoding])");
                return BoxedValue.FromString(string.Empty);
            }
            try {
                string path = operands[0].AsString;
                int startLine = operands[1].GetInt();
                int endLine = operands[2].GetInt();
                Encoding? encoding = operands.Count > 3 ? GetEncoding(operands[3]) : null;
                string content = Core.AgentCore.Instance.FileOps.ReadFile(path, encoding) ?? string.Empty;
                string newline = "\n";
                if (content.Contains("\r\n")) newline = "\r\n";
                else if (content.Contains("\r")) newline = "\r";
                var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                if (startLine < 1) startLine = 1;
                if (endLine > lines.Length) endLine = lines.Length;
                if (startLine > endLine) return BoxedValue.FromString(string.Empty);
                var sb = new StringBuilder();
                for (int i = startLine - 1; i < endLine; ++i) {
                    if (i > startLine - 1) sb.Append(newline);
                    sb.Append(lines[i]);
                }
                return BoxedValue.FromString(sb.ToString());
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"read_file_line_range error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

}
