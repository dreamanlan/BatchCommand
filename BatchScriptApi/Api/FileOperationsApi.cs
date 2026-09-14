using System;
using System.Text;

using System.Collections;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using BatchCommand.Utils;

namespace BatchCommand.Api
{
    // read_file(path[, encoding]) - read file content
    public sealed class ReadFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                ApiErrorInfo.AppendLine("Expected: read_file(path[, encoding])");
                return BoxedValue.NullObject;
            }

            try {
                string path = operands[0].AsString;
                Encoding? encoding = operands.Count > 1 ? BatchCommand.Utils.BomHelper.GetEncoding(operands[1], path) : null;
                string content = DslHost.Current!.FileOps.ReadFile(path, encoding);
                return BoxedValue.FromString(content ?? string.Empty);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"read_file error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // write_file(path, content[, encoding]) - write file content; encoding supports -bom/-no-bom/-nobom suffixes
    public sealed class WriteFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: write_file(path, content[, encoding])");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                string fullPath = BatchCommand.Utils.PathHelper.EnsureAbsolutePath(path, DslHost.Current!.BasePath);
                string content = operands[1].AsString;
                Encoding? encoding = operands.Count > 2 ? BatchCommand.Utils.BomHelper.GetEncodingForWrite(operands[2], fullPath) : null;
                if (string.IsNullOrEmpty(content)) {
                    ApiErrorInfo.AppendLine("You cannot write empty values to a file !!! To delete certain lines, use the 'delete_lines' function.");
                    return BoxedValue.From(false);
                }
                string ext = Path.GetExtension(path).ToLower();
                if (File.Exists(path) && ext != ".txt" && ext != ".md") {
                    var lineCount = SafeFileReader.ReadAllLines(path).Length;
                    var newLineCount = path.Split('\n').Length;
                    if (lineCount > newLineCount + BatchCommand.Api.FrameworkApiAlias.MaxLinesDeletedByWriteFile) {
                        ApiErrorInfo.AppendLine("You cannot significantly reduce code using 'write_file' !!! To delete certain lines, use the 'delete_lines' function.");
                        return BoxedValue.From(false);
                    }
                }
                bool result = DslHost.Current!.FileOps.WriteFile(path, content, true, encoding);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"write_file error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // write_file_no_bom(path, content) - write file content without BOM
    public sealed class WriteFileNoBomExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                ApiErrorInfo.AppendLine("Expected: write_file_no_bom(path, content)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                string content = operands[1].AsString;
                if (string.IsNullOrEmpty(content)) {
                    ApiErrorInfo.AppendLine("You cannot write empty content to a file !!!");
                    return BoxedValue.From(false);
                }
                string ext = Path.GetExtension(path).ToLower();
                if (File.Exists(path) && ext != ".txt" && ext != ".md") {
                    var lineCount = SafeFileReader.ReadAllLines(path).Length;
                    var newLineCount = content.Split('\n').Length;
                    if (lineCount > newLineCount + BatchCommand.Api.FrameworkApiAlias.MaxLinesDeletedByWriteFile) {
                        ApiErrorInfo.AppendLine("You cannot significantly reduce code using 'write_file_no_bom' !!! To delete certain lines, use the 'delete_lines' function.");
                        return BoxedValue.From(false);
                    }
                }
                bool result = DslHost.Current!.FileOps.WriteFileNoBom(path, content);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"write_file_no_bom error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // read_file_bytes(path) - read file as byte array
    public sealed class ReadFileBytesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: read_file_bytes(path)");
                return BoxedValue.NullObject;
            }

            try {
                string path = operands[0].AsString;
                byte[] content = DslHost.Current!.FileOps.ReadFileBytes(path);
                return BoxedValue.FromObject(content);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"read_file_bytes error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // write_file_bytes(path, bytes) - write byte array to file
    public sealed class WriteFileBytesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                ApiErrorInfo.AppendLine("Expected: write_file_bytes(path, bytes)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                var obj = operands[1].GetObject();
                if (obj is byte[] bytes) {
                    bool result = DslHost.Current!.FileOps.WriteFileBytes(path, bytes);
                    return BoxedValue.From(result);
                }
                else if (obj is IList<byte> list) {
                    bool result = DslHost.Current!.FileOps.WriteFileBytes(path, list.ToArray());
                    return BoxedValue.From(result);
                }
                else {
                    ApiErrorInfo.AppendLine("write_file_bytes: second argument must be a byte array or a byte list");
                    return BoxedValue.From(false);
                }
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"write_file_bytes error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // hex_to_bytes(hexString) - convert hex string to byte array
    public sealed class HexToBytesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: hex_to_bytes(hexString), aliased as hex_string_to_bytes");
                return BoxedValue.NullObject;
            }

            try {
                string hex = operands[0].AsString;
                // Remove all whitespace (spaces, newlines, tabs)
                hex = hex.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
                if (hex.Length % 2 != 0) {
                    ApiErrorInfo.AppendLine("hex_to_bytes: hex string length must be even");
                    return BoxedValue.NullObject;
                }
                byte[] bytes = new byte[hex.Length / 2];
                for (int i = 0; i < bytes.Length; i++) {
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                return BoxedValue.FromObject(bytes);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"hex_to_bytes error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // bytes_to_hex(bytes[, bytesPerLine]) - convert byte array to formatted hex string
    public sealed class BytesToHexExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                ApiErrorInfo.AppendLine("Expected: bytes_to_hex(bytes[, bytesPerLine]), aliased as bytes_to_hex_string");
                return BoxedValue.NullObject;
            }

            try {
                var obj = operands[0].GetObject();
                if (obj is not byte[] bytes) {
                    ApiErrorInfo.AppendLine("bytes_to_hex: first argument must be a byte array");
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
                ApiErrorInfo.AppendLine($"bytes_to_hex error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // base64_to_bytes(base64String) - convert base64 string to byte array
    public sealed class Base64ToBytesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: base64_to_bytes(base64String)");
                return BoxedValue.NullObject;
            }

            try {
                string base64 = operands[0].AsString;
                byte[] bytes = Convert.FromBase64String(base64);
                return BoxedValue.FromObject(bytes);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"base64_to_bytes error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // bytes_to_base64(bytes) - convert byte array to base64 string
    public sealed class BytesToBase64Exp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: bytes_to_base64(bytes)");
                return BoxedValue.NullObject;
            }

            try {
                var obj = operands[0].GetObject();
                byte[]? bytes = obj switch {
                    byte[] b => b,
                    IList<byte> list => list.ToArray(),
                    _ => null
                };
                if (bytes == null) {
                    ApiErrorInfo.AppendLine("bytes_to_base64: first argument must be a byte array");
                    return BoxedValue.NullObject;
                }
                return BoxedValue.FromString(Convert.ToBase64String(bytes));
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"bytes_to_base64 error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // append_file(path, content[, encoding]) - append to file; encoding supports -bom/-no-bom/-nobom suffixes
    public sealed class AppendFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: append_file(path, content[, encoding])");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                string fullPath = BatchCommand.Utils.PathHelper.EnsureAbsolutePath(path, DslHost.Current!.BasePath);
                string content = operands[1].AsString;
                Encoding? encoding = operands.Count > 2 ? BatchCommand.Utils.BomHelper.GetEncodingForWrite(operands[2], fullPath) : null;
                if (string.IsNullOrEmpty(content)) {
                    ApiErrorInfo.AppendLine("You cannot append empty values to a file !!!");
                    return BoxedValue.From(false);
                }
                bool result = DslHost.Current!.FileOps.AppendFile(path, content, encoding);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"append_file error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // copy_file(sourcePath, destPath, overwrite) - copy file
    public sealed class CopyFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: copy_file(sourcePath, destPath, overwrite)");
                return BoxedValue.From(false);
            }

            try {
                string sourcePath = operands[0].AsString;
                string destPath = operands[1].AsString;
                bool overwrite = operands.Count > 2 ? operands[2].GetBool() : false;
                bool result = DslHost.Current!.FileOps.CopyFile(sourcePath, destPath, overwrite);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"copy_file error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // move_file(sourcePath, destPath, overwrite) - move file
    public sealed class MoveFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: move_file(sourcePath, destPath, overwrite)");
                return BoxedValue.From(false);
            }

            try {
                string sourcePath = operands[0].AsString;
                string destPath = operands[1].AsString;
                bool overwrite = operands.Count > 2 ? operands[2].GetBool() : false;
                bool result = DslHost.Current!.FileOps.MoveFile(sourcePath, destPath, overwrite);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"move_file error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // file_exists(path) - check if file exists
    public sealed class FileExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: file_exists(path), aliased as file_exist");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                bool result = DslHost.Current!.FileOps.FileExists(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"file_exists error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // file_not_exists(path) - check if file not exists
    public sealed class FileNotExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: file_not_exists(path), aliased as file_not_exist");
                return BoxedValue.From(true);
            }

            try {
                string path = operands[0].AsString;
                bool result = !DslHost.Current!.FileOps.FileExists(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"file_not_exists error: {ex.Message}");
                return BoxedValue.From(true);
            }
        }
    }

    // file_has_bom(path) - check if file has BOM
    public sealed class FileHasBomExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: file_has_bom(path)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                bool result = DslHost.Current!.FileOps.FileHasBom(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"file_has_bom error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // file_add_bom(path) - add BOM to file (skip if already has BOM)
    public sealed class FileAddBomExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: file_add_bom(path)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                bool result = DslHost.Current!.FileOps.FileAddBom(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"file_add_bom error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // file_remove_bom(path) - remove BOM from file (skip if no BOM)
    public sealed class FileRemoveBomExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: file_remove_bom(path)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                bool result = DslHost.Current!.FileOps.FileRemoveBom(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"file_remove_bom error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // dir_exists(path) - check if directory exists
    public sealed class DirExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: dir_exists(path), aliased as dir_exist");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                bool result = DslHost.Current!.FileOps.DirectoryExists(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"dir_exists error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // dir_not_exists(path) - check if directory exists
    public sealed class DirNotExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: dir_not_exists(path), aliased as dir_not_exist");
                return BoxedValue.From(true);
            }

            try {
                string path = operands[0].AsString;
                bool result = !DslHost.Current!.FileOps.DirectoryExists(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"dir_not_exists error: {ex.Message}");
                return BoxedValue.From(true);
            }
        }
    }

    // path_exists(path) - check if file or directory exists
    public sealed class PathExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: path_exists(path), aliased as path_exist");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                return BoxedValue.FromBool(DslHost.Current!.FileOps.DirectoryExists(path) || DslHost.Current!.FileOps.FileExists(path));
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"path_exists error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // path_not_exists(path) - check if file or directory exists
    public sealed class PathNotExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: path_not_exists(path), aliased as path_not_exist");
                return BoxedValue.From(true);
            }

            try {
                string path = operands[0].AsString;
                return BoxedValue.FromBool(!(DslHost.Current!.FileOps.DirectoryExists(path) || DslHost.Current!.FileOps.FileExists(path)));
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"path_not_exists error: {ex.Message}");
                return BoxedValue.From(true);
            }
        }
    }

    // list_dir_info(path[, glob_pattern, recursive]) - list directory contents
    public sealed class ListDirInfoExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: list_dir_info(path[, glob_pattern, recursive])");
                return BoxedValue.NullObject;
            }

            try {
                string path = operands[0].AsString;
                string pattern = operands.Count > 1 ? operands[1].AsString : "*";
                bool recursive = operands.Count > 2 ? operands[2].GetBool() : false;

                var files = DslHost.Current!.FileOps.ListDirectory(path, pattern, recursive);
                var result = new List<BoxedValue>();

                foreach (var file in files) {
                    result.Add(BoxedValue.FromString(file.ToString()));
                }

                return BoxedValue.FromObject(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"list_dir_info error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    public sealed class EnsureDirectoryExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: ensure_directory(dir), aliased as ensure_dir");
                return BoxedValue.NullObject;
            }
            bool ret = false;
            if (operands.Count >= 1) {
                var dir = operands[0].AsString;
                dir = Environment.ExpandEnvironmentVariables(dir);
                DslHost.Current!.FileOps.EnsureDirectory(dir);
            }
            return ret;
        }
    }
    public sealed class RemoveDirectoryExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: remove_directory(dir), aliased as remove_dir");
                return BoxedValue.NullObject;
            }
            bool ret = false;
            if (operands.Count >= 1) {
                var dir = operands[0].AsString;
                dir = Environment.ExpandEnvironmentVariables(dir);
                DslHost.Current!.FileOps.RemoveDirectory(dir);
            }
            return ret;
        }
    }

    public sealed class FindFilesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: find_files(path,glob_pattern[,recursive])");
                return BoxedValue.NullObject;
            }
            string dir = operands[0].AsString;
            string globPattern = operands[1].AsString;
            bool recursive = operands.Count > 2 ? operands[2].GetBool() : true;
            return BoxedValue.FromObject(DslHost.Current!.FileOps.FindFiles(dir, globPattern, recursive));
        }
    }

    public sealed class SearchFilesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                ApiErrorInfo.AppendLine("Expected: search_files(glob_pattern,path)");
                return BoxedValue.NullObject;
            }
            string globPattern = operands[0].AsString;
            string dir = operands[1].AsString;
            return BoxedValue.FromObject(DslHost.Current!.FileOps.FindFiles(dir, globPattern));
        }
    }

    // search_log_file(log_file, search_regex[, context_lines_after, context_lines_before, encoding]) - search log file with regex pattern
    public sealed class SearchLogFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 5) {
                ApiErrorInfo.AppendLine("Expected: search_log_file(log_file, search_regex[, context_lines_after, context_lines_before, encoding]), aliased as grep_log_file");
                return BoxedValue.FromString("Expected: search_log_file(log_file, search_regex[, context_lines_after, context_lines_before, encoding])");
            }

            try {
                string logFile = operands[0].AsString;
                string searchRegex = operands[1].AsString;
                int contextLinesAfter = operands.Count > 2 ? operands[2].GetInt() : 5;
                int contextLinesBefore = operands.Count > 3 ? operands[3].GetInt() : 0;
                Encoding? encoding = operands.Count > 4 ? BatchCommand.Utils.BomHelper.GetEncoding(operands[4], logFile) : null;
                string result = DslHost.Current!.FileOps.SearchLogFile(logFile, searchRegex, contextLinesAfter, contextLinesBefore, encoding);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"search_log_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }

    // search_log_file_as_list(log_file, search_regex[, context_lines_after, context_lines_before, encoding]) - return list of match blocks
    public sealed class SearchLogFileAsListExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 5) {
                ApiErrorInfo.AppendLine("Expected: search_log_file_as_list(log_file, search_regex[, context_lines_after, context_lines_before, encoding]), aliased as grep_log_file_as_list");
                return BoxedValue.FromObject(new List<object>());
            }

            try {
                string logFile = operands[0].AsString;
                string searchRegex = operands[1].AsString;
                int contextLinesAfter = operands.Count > 2 ? operands[2].GetInt() : 5;
                int contextLinesBefore = operands.Count > 3 ? operands[3].GetInt() : 0;
                Encoding? encoding = operands.Count > 4 ? BatchCommand.Utils.BomHelper.GetEncoding(operands[4], logFile) : null;
                var blocks = DslHost.Current!.FileOps.SearchLogFileAsList(logFile, searchRegex, contextLinesAfter, contextLinesBefore, encoding);
                var result = new List<object>();
                foreach (var b in blocks) {
                    result.Add(b);
                }
                return BoxedValue.FromObject(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"search_log_file_as_list error: {ex.Message}");
                return BoxedValue.FromObject(new List<object>());
            }
        }
    }

    // tail_log_file(log_file, lines[, encoding]) - get last N lines from log file
    public sealed class TailLogFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: tail_log_file(log_file, lines[, encoding])");
                return BoxedValue.FromString("Expected: tail_log_file(log_file, lines[, encoding])");
            }

            try {
                string logFile = operands[0].AsString;
                int lines = operands.Count > 1 ? operands[1].GetInt() : 100;
                Encoding? encoding = operands.Count > 2 ? BatchCommand.Utils.BomHelper.GetEncoding(operands[2], logFile) : null;
                string result = DslHost.Current!.FileOps.TailLogFile(logFile, lines, encoding);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"tail_log_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }

    // head_log_file(log_file, lines[, encoding]) - get first N lines from log file
    public sealed class HeadLogFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: head_log_file(log_file, lines[, encoding])");
                return BoxedValue.FromString("Expected: head_log_file(log_file, lines[, encoding])");
            }

            try {
                string logFile = operands[0].AsString;
                int lines = operands.Count > 1 ? operands[1].GetInt() : 100;
                Encoding? encoding = operands.Count > 2 ? BatchCommand.Utils.BomHelper.GetEncoding(operands[2], logFile) : null;
                string result = DslHost.Current!.FileOps.HeadLogFile(logFile, lines, encoding);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"head_log_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }

    // tail_file(file, lines[, encoding]) - get last N lines from file
    public sealed class TailFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: tail_file(file, lines[, encoding])");
                return BoxedValue.FromString("Expected: tail_file(file, lines[, encoding])");
            }

            try {
                string file = operands[0].AsString;
                int lines = operands.Count > 1 ? operands[1].GetInt() : 100;
                Encoding? encoding = operands.Count > 2 ? BatchCommand.Utils.BomHelper.GetEncoding(operands[2], file) : null;
                string result = DslHost.Current!.FileOps.TailFile(file, lines, encoding);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"tail_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }

    // head_file(file, lines[, encoding]) - get first N lines from file
    public sealed class HeadFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: head_file(file, lines[, encoding])");
                return BoxedValue.FromString("Expected: head_file(file, lines[, encoding])");
            }

            try {
                string file = operands[0].AsString;
                int lines = operands.Count > 1 ? operands[1].GetInt() : 100;
                Encoding? encoding = operands.Count > 2 ? BatchCommand.Utils.BomHelper.GetEncoding(operands[2], file) : null;
                string result = DslHost.Current!.FileOps.HeadFile(file, lines, encoding);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"head_file error: {ex.Message}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }
    // csv_read(path[, encoding]) - read CSV file into List<List<string>>. minimal RFC4180 subset: comma delimiter, double-quoted fields, "" escape. no cross-line quoted fields.
    public sealed class CsvReadExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                ApiErrorInfo.AppendLine("Expected: csv_read(path[, encoding])");
                return BoxedValue.NullObject;
            }
            try {
                string path = operands[0].AsString;
                Encoding? encoding = operands.Count > 1 ? BatchCommand.Utils.BomHelper.GetEncoding(operands[1], path) : null;
                string content = DslHost.Current!.FileOps.ReadFile(path, encoding) ?? string.Empty;
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
                ApiErrorInfo.AppendLine($"csv_read error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // read_file_line_range(path, startLine, endLine[, encoding]) - read line range as concatenated string, preserving original newline style
    public sealed class ReadFileLineRangeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 4) {
                ApiErrorInfo.AppendLine("Expected: read_file_line_range(path, startLine, endLine[, encoding])");
                return BoxedValue.FromString(string.Empty);
            }
            try {
                string path = operands[0].AsString;
                int startLine = operands[1].GetInt();
                int endLine = operands[2].GetInt();
                Encoding? encoding = operands.Count > 3 ? BatchCommand.Utils.BomHelper.GetEncoding(operands[3], path) : null;
                string content = DslHost.Current!.FileOps.ReadFile(path, encoding) ?? string.Empty;
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
                ApiErrorInfo.AppendLine($"read_file_line_range error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

}
