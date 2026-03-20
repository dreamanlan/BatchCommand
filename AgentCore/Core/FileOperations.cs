using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CefDotnetApp.AgentCore.Models;
using CefDotnetApp.AgentCore.Utils;
using AgentPlugin.Abstractions;

namespace CefDotnetApp.AgentCore.Core
{
    public class FileOperations
    {
        private readonly string _basePath;
        private readonly string _appDir;
        private readonly bool _isMac;

        public FileOperations(string basePath, string appDir, bool isMac)
        {
            _basePath = basePath;
            _appDir = appDir;
            _isMac = isMac;
        }

        public string ReadFile(string path)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!File.Exists(fullPath)) {
                throw new IOException($"File not found: {path}");
            }

            return File.ReadAllText(fullPath, Encoding.UTF8);
        }

        public byte[] ReadFileBytes(string path)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!File.Exists(fullPath)) {
                throw new IOException($"File not found: {path}");
            }

            return File.ReadAllBytes(fullPath);
        }

        public string[] ReadFileLines(string path)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!File.Exists(fullPath)) {
                throw new IOException($"File not found: {path}");
            }

            return File.ReadAllLines(fullPath, Encoding.UTF8);
        }

        public bool WriteFile(string path, string content, bool createDirectory = true)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);

            if (createDirectory) {
                string? directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }

            if (!string.IsNullOrEmpty(content)) {
                File.WriteAllText(fullPath, content, Encoding.UTF8);
                return true;
            }
            return false;
        }

        public bool WriteFileBytes(string path, byte[] content, bool createDirectory = true)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);

            if (createDirectory) {
                string? directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }

            if (null != content && content.Length > 0) {
                File.WriteAllBytes(fullPath, content);
                return true;
            }
            return false;
        }

        public bool AppendFile(string path, string content)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!string.IsNullOrEmpty(content)) {
                File.AppendAllText(fullPath, content, Encoding.UTF8);
                return true;
            }
            return false;
        }

        public bool DeleteFile(string path)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (File.Exists(fullPath)) {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }

        public bool CopyFile(string sourcePath, string destPath, bool overwrite = false)
        {
            string fullSourcePath = PathHelper.EnsureAbsolutePath(sourcePath, _basePath);
            string fullDestPath = PathHelper.EnsureAbsolutePath(destPath, _basePath);

            if (!File.Exists(fullSourcePath))
                return false;

            string? destDirectory = Path.GetDirectoryName(fullDestPath);
            if (!string.IsNullOrEmpty(destDirectory) && !Directory.Exists(destDirectory))
                Directory.CreateDirectory(destDirectory);

            File.Copy(fullSourcePath, fullDestPath, overwrite);
            return true;
        }

        public bool MoveFile(string sourcePath, string destPath, bool overwrite = false)
        {
            string fullSourcePath = PathHelper.EnsureAbsolutePath(sourcePath, _basePath);
            string fullDestPath = PathHelper.EnsureAbsolutePath(destPath, _basePath);

            if (!File.Exists(fullSourcePath))
                return false;

            string? destDirectory = Path.GetDirectoryName(fullDestPath);
            if (!string.IsNullOrEmpty(destDirectory) && !Directory.Exists(destDirectory))
                Directory.CreateDirectory(destDirectory);

            if (overwrite && File.Exists(fullDestPath))
                File.Delete(fullDestPath);

            File.Move(fullSourcePath, fullDestPath);
            return true;
        }

        public bool FileExists(string path)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            return File.Exists(fullPath);
        }

        public bool DirectoryExists(string path)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            return Directory.Exists(fullPath);
        }

        public FileInfoModel GetFileInfo(string path)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            return new FileInfoModel(fullPath);
        }

        public List<FileInfoModel> ListDirectory(string path, string searchPattern = "*", bool recursive = false)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!Directory.Exists(fullPath))
                return new List<FileInfoModel>();

            var result = new List<FileInfoModel>();
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var files = Directory.GetFiles(fullPath, searchPattern, searchOption);
            foreach (var file in files) {
                result.Add(new FileInfoModel(file));
            }

            var directories = Directory.GetDirectories(fullPath, searchPattern, searchOption);
            foreach (var dir in directories) {
                result.Add(new FileInfoModel(dir));
            }

            return result;
        }

        public bool CreateDirectory(string path)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!Directory.Exists(fullPath)) {
                Directory.CreateDirectory(fullPath);
                return true;
            }
            return false;
        }

        public bool DeleteDirectory(string path, bool recursive = false)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (Directory.Exists(fullPath)) {
                Directory.Delete(fullPath, recursive);
                return true;
            }
            return false;
        }

        public List<string> FindFiles(string path, string pattern, bool recursive = true)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!Directory.Exists(fullPath))
                return new List<string>();

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.GetFiles(fullPath, pattern, searchOption).ToList();
        }

        // Code editing methods

        public bool ReplaceInFile(string path, string oldString, string newString, bool allOccurrences = false, bool exactMatch = false)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!File.Exists(fullPath)) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("replace_in_file: file not found");
                return false;
            }

            string content = File.ReadAllText(fullPath, Encoding.UTF8);

            // Level 1: Exact match (no modification to search string)
            if (allOccurrences) {
                if (content.Contains(oldString)) {
                    content = content.Replace(oldString, newString);
                    File.WriteAllText(fullPath, content, Encoding.UTF8);
                    return true;
                }
            }
            else {
                int index = content.IndexOf(oldString, StringComparison.Ordinal);
                if (index >= 0) {
                    content = content.Substring(0, index) + newString + content.Substring(index + oldString.Length);
                    File.WriteAllText(fullPath, content, Encoding.UTF8);
                    return true;
                }
            }
            if (exactMatch) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("replace_in_file: old string not found (exact match)");
                return false;
            }
            // Level 2: Trimmed match
            var trimedOldString = oldString.Trim();
            var trimedNewString = newString.Trim();
            if (allOccurrences) {
                if (content.Contains(trimedOldString)) {
                    content = content.Replace(trimedOldString, trimedNewString);
                    File.WriteAllText(fullPath, content, Encoding.UTF8);
                    return true;
                }
            }
            else {
                int index = content.IndexOf(trimedOldString, StringComparison.Ordinal);
                if (index >= 0) {
                    content = content.Substring(0, index) + trimedNewString + content.Substring(index + trimedOldString.Length);
                    File.WriteAllText(fullPath, content, Encoding.UTF8);
                    return true;
                }
            }
            // Level 3: Normalized whitespace matching (DiffOps fallback)
            var result = DiffOperations.ReplaceFullLinesText(content, oldString, newString, allOccurrences);
            if (result.Success) {
                File.WriteAllText(fullPath, result.ResultContent, Encoding.UTF8);
                return true;
            }
            AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("replace_in_file: old string not found");
            return false;
        }

        public bool ReplaceLines(string path, int startLine, int endLine, string newContent)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!File.Exists(fullPath)) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("replace_lines: file not found");
                return false;
            }

            string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);

            if (startLine < 1 || endLine > lines.Length || startLine > endLine) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("replace_lines: out of range");
                return false;
            }

            string[] newLines = newContent.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            string[] result = new string[lines.Length - (endLine - startLine + 1) + newLines.Length];

            int index = 0;
            for (int i = 0; i < startLine - 1; i++) {
                result[index++] = lines[i];
            }
            for (int i = 0; i < newLines.Length; i++) {
                result[index++] = newLines[i];
            }
            for (int i = endLine; i < lines.Length; i++) {
                result[index++] = lines[i];
            }

            File.WriteAllLines(fullPath, result, Encoding.UTF8);
            return true;
        }

        public bool InsertAfterText(string path, string searchLiteralText, string content, bool allOccurrences = false, bool exactMatch = false)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!File.Exists(fullPath)) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("insert_after: file not found");
                return false;
            }

            string fileContent = File.ReadAllText(fullPath, Encoding.UTF8);

            // Level 1: Exact match (no modification to search string)
            if (allOccurrences) {
                if (fileContent.Contains(searchLiteralText)) {
                    fileContent = fileContent.Replace(searchLiteralText, searchLiteralText + content);
                    File.WriteAllText(fullPath, fileContent, Encoding.UTF8);
                    return true;
                }
            }
            else {
                int index = fileContent.IndexOf(searchLiteralText, StringComparison.Ordinal);
                if (index >= 0) {
                    fileContent = fileContent.Substring(0, index + searchLiteralText.Length) + content + fileContent.Substring(index + searchLiteralText.Length);
                    File.WriteAllText(fullPath, fileContent, Encoding.UTF8);
                    return true;
                }
            }
            if (exactMatch) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("insert_after: search literal text not found (exact match)");
                return false;
            }
            // Level 2: Trimmed match
            var trimmedSearch = searchLiteralText.Trim();
            if (allOccurrences) {
                if (fileContent.Contains(trimmedSearch)) {
                    fileContent = fileContent.Replace(trimmedSearch, trimmedSearch + content);
                    File.WriteAllText(fullPath, fileContent, Encoding.UTF8);
                    return true;
                }
            }
            else {
                int index = fileContent.IndexOf(trimmedSearch, StringComparison.Ordinal);
                if (index >= 0) {
                    fileContent = fileContent.Substring(0, index + trimmedSearch.Length) + content + fileContent.Substring(index + trimmedSearch.Length);
                    File.WriteAllText(fullPath, fileContent, Encoding.UTF8);
                    return true;
                }
            }
            // Level 3: Normalized whitespace matching (DiffOps fallback)
            var result = DiffOperations.InsertAfterFullLinesText(fileContent, searchLiteralText, content, allOccurrences);
            if (result.Success) {
                File.WriteAllText(fullPath, result.ResultContent, Encoding.UTF8);
                return true;
            }
            AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("insert_after: search literal text not found");
            return false;
        }

        public bool InsertBeforeText(string path, string searchLiteralText, string content, bool allOccurrences = false, bool exactMatch = false)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!File.Exists(fullPath)) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("insert_before: file not found");
                return false;
            }

            string fileContent = File.ReadAllText(fullPath, Encoding.UTF8);

            // Level 1: Exact match (no modification to search string)
            if (allOccurrences) {
                if (fileContent.Contains(searchLiteralText)) {
                    fileContent = fileContent.Replace(searchLiteralText, content + searchLiteralText);
                    File.WriteAllText(fullPath, fileContent, Encoding.UTF8);
                    return true;
                }
            }
            else {
                int index = fileContent.IndexOf(searchLiteralText, StringComparison.Ordinal);
                if (index >= 0) {
                    fileContent = fileContent.Substring(0, index) + content + fileContent.Substring(index);
                    File.WriteAllText(fullPath, fileContent, Encoding.UTF8);
                    return true;
                }
            }
            if (exactMatch) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("insert_before: search literal text not found (exact match)");
                return false;
            }
            // Level 2: Trimmed match
            var trimmedSearch = searchLiteralText.Trim();
            if (allOccurrences) {
                if (fileContent.Contains(trimmedSearch)) {
                    fileContent = fileContent.Replace(trimmedSearch, content + trimmedSearch);
                    File.WriteAllText(fullPath, fileContent, Encoding.UTF8);
                    return true;
                }
            }
            else {
                int index = fileContent.IndexOf(trimmedSearch, StringComparison.Ordinal);
                if (index >= 0) {
                    fileContent = fileContent.Substring(0, index) + content + fileContent.Substring(index);
                    File.WriteAllText(fullPath, fileContent, Encoding.UTF8);
                    return true;
                }
            }
            // Level 3: Normalized whitespace matching (DiffOps fallback)
            var result = DiffOperations.InsertBeforeFullLinesText(fileContent, searchLiteralText, content, allOccurrences);
            if (result.Success) {
                File.WriteAllText(fullPath, result.ResultContent, Encoding.UTF8);
                return true;
            }
            AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("insert_before: search literal text not found");
            return false;
        }

        public bool DeleteLines(string path, int startLine, int endLine)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!File.Exists(fullPath)) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("delete_lines: file not found");
                return false;
            }

            string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);

            if (startLine < 1 || endLine > lines.Length || startLine > endLine) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("delete_lines: out of range");
                return false;
            }

            string[] result = new string[lines.Length - (endLine - startLine + 1)];
            int index = 0;
            for (int i = 0; i < lines.Length; i++) {
                if (i + 1 < startLine || i + 1 > endLine) {
                    result[index++] = lines[i];
                }
            }

            File.WriteAllLines(fullPath, result, Encoding.UTF8);
            return true;
        }

        public List<int> SearchLinesInFile(string path, string pattern, bool ignoreCase = true)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!File.Exists(fullPath))
                return new List<int>();

            var lines = File.ReadAllLines(fullPath, Encoding.UTF8);
            var result = new List<int>();

            for (int i = 0; i < lines.Length; i++) {
                if (MatchesPattern(lines[i], pattern, ignoreCase)) {
                    result.Add(i + 1);
                }
            }

            return result;
        }

        private bool MatchesPattern(string text, string pattern, bool ignoreCase = true)
        {
            try
            {
                var options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
                return Regex.IsMatch(text, pattern, options);
            }
            catch (ArgumentException)
            {
                var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                return text.IndexOf(pattern, comparison) >= 0;
            }
        }

        public string[] ReadLinesRange(string path, int startLine, int endLine)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!File.Exists(fullPath)) {
                throw new IOException($"File not found: {path}");
            }

            string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);

            if (startLine > endLine) {
                throw new IOException($"startLine:{startLine} > endLine:{endLine}");
            }
            if (startLine < 1) {
                startLine = 1;
            }
            if (endLine > lines.Length) {
                endLine = lines.Length;
            }

            string[] result = new string[endLine - startLine + 1];
            int index = 0;
            for (int i = startLine - 1; i < endLine; i++) {
                result[index++] = lines[i];
            }

            return result;
        }

        public int GetLineCount(string path)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!File.Exists(fullPath))
                return 0;

            string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);
            return lines.Length;
        }

        public string SearchFile(string path, string searchRegex, int contextLinesAfter = 5, int contextLinesBefore = 0)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (!File.Exists(fullPath))
                return $"File not found: {path}";

            var lines = File.ReadAllLines(fullPath, Encoding.UTF8);
            var matchedLineIndices = new HashSet<int>();
            var result = new StringBuilder();

            try {
                var regex = new Regex(searchRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                for (int i = 0; i < lines.Length; i++) {
                    if (regex.IsMatch(lines[i])) {
                        matchedLineIndices.Add(i);
                    }
                }
            }
            catch (ArgumentException) {
                // If regex is invalid, fall back to simple string search
                for (int i = 0; i < lines.Length; i++) {
                    if (lines[i].IndexOf(searchRegex, StringComparison.OrdinalIgnoreCase) >= 0) {
                        matchedLineIndices.Add(i);
                    }
                }
            }

            if (matchedLineIndices.Count == 0) {
                return $"No matches found for pattern: {searchRegex}";
            }

            // Build output with context lines
            var outputLines = new HashSet<int>();
            foreach (var matchIndex in matchedLineIndices) {
                int startLine = Math.Max(0, matchIndex - contextLinesBefore);
                int endLine = Math.Min(lines.Length - 1, matchIndex + contextLinesAfter);
                for (int i = startLine; i <= endLine; i++) {
                    outputLines.Add(i);
                }
            }

            var sortedLines = outputLines.OrderBy(x => x).ToList();
            int lastLine = -2;
            foreach (var lineIndex in sortedLines) {
                if (lineIndex > lastLine + 1) {
                    if (lastLine >= 0) {
                        result.AppendLine("--");
                    }
                }
                string prefix = matchedLineIndices.Contains(lineIndex) ? "* " : "  ";
                result.AppendLine($"{prefix}{lineIndex + 1}: {lines[lineIndex]}");
                lastLine = lineIndex;
            }

            return result.ToString();
        }

        public string SearchFiles(string path, string searchRegex, int contextLinesAfter = 5, int contextLinesBefore = 0, List<string>? filterAndNewExts = null)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
            if (File.Exists(fullPath)) {
                return SearchFile(path, searchRegex, contextLinesAfter, contextLinesBefore);
            }
            if (!Directory.Exists(fullPath)) {
                return $"Directory not found: {path}";
            }

            if (null == filterAndNewExts) {
                filterAndNewExts = new List<string> { "*.txt", "*.md", "*.jsonl", "*.json", "*.xml", "*.yaml", "*.csv", "*.htm", "*.html", "*.css",
                    "*.js", "*.py", "*.java", "*.c", "*.cpp", "*.h", "*.hpp", "*.m", "*.mm", "*.cxx", "*.hxx", "*.cc", "*.hh", "*.swift", "*.kt",
                    "*.cs", "*.vb", "*.php", "*.asp", "*.aspx", "*.jsp", "*.cgi", "*.pl", "*.rb", "*.lua", "*.go", "*.rs", "*.kts", "*.fs", "*.fsx",
                    "*.ts", "*.tsx", "*.jsx", "*.dsl", "*.sh", "*.bat", "*.cmd", "*.toml", "*.tml", "*.log"};
            }
            if (filterAndNewExts.Count <= 0) {
                filterAndNewExts.Add("*");
            }
            var sb = new StringBuilder();
            for (int i = 0; i < filterAndNewExts.Count; ++i) {
                string filter = filterAndNewExts[i];
                string[] files = Directory.GetFiles(path, filter, SearchOption.AllDirectories);
                foreach (string file in files) {
                    sb.AppendLine($"====== {file} ======");
                    var result = SearchFile(file, searchRegex, contextLinesAfter, contextLinesBefore);
                    sb.AppendLine(result);
                }
            }
            return sb.ToString();
        }

        public string SearchLogFile(string logFile, string searchRegex, int contextLinesAfter = 5, int contextLinesBefore = 0)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(logFile, _appDir);
            if (!File.Exists(fullPath))
                return $"Log file not found: {logFile}";

            var lines = File.ReadAllLines(fullPath, Encoding.UTF8);
            var matchedLineIndices = new HashSet<int>();
            var result = new StringBuilder();

            try {
                var regex = new Regex(searchRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                for (int i = 0; i < lines.Length; i++) {
                    if (regex.IsMatch(lines[i])) {
                        matchedLineIndices.Add(i);
                    }
                }
            }
            catch (ArgumentException) {
                // If regex is invalid, fall back to simple string search
                for (int i = 0; i < lines.Length; i++) {
                    if (lines[i].IndexOf(searchRegex, StringComparison.OrdinalIgnoreCase) >= 0) {
                        matchedLineIndices.Add(i);
                    }
                }
            }

            if (matchedLineIndices.Count == 0) {
                return $"No matches found for pattern: {searchRegex}";
            }

            // Build output with context lines
            var outputLines = new HashSet<int>();
            foreach (var matchIndex in matchedLineIndices) {
                int startLine = Math.Max(0, matchIndex - contextLinesBefore);
                int endLine = Math.Min(lines.Length - 1, matchIndex + contextLinesAfter);
                for (int i = startLine; i <= endLine; i++) {
                    outputLines.Add(i);
                }
            }

            var sortedLines = outputLines.OrderBy(x => x).ToList();
            int lastLine = -2;
            foreach (var lineIndex in sortedLines) {
                if (lineIndex > lastLine + 1) {
                    if (lastLine >= 0) {
                        result.AppendLine("--");
                    }
                }
                string prefix = matchedLineIndices.Contains(lineIndex) ? "* " : "  ";
                result.AppendLine($"{prefix}{lineIndex + 1}: {lines[lineIndex]}");
                lastLine = lineIndex;
            }

            return result.ToString();
        }

        public string TailLogFile(string logFile, int lines)
        {
            string fullPath = PathHelper.EnsureAbsolutePath(logFile, _appDir);
            if (!File.Exists(fullPath))
                return $"Log file not found: {logFile}";

            var allLines = File.ReadAllLines(fullPath, Encoding.UTF8);
            var lineCount = Math.Min(lines, allLines.Length);
            var recentLines = allLines.Skip(Math.Max(0, allLines.Length - lineCount)).ToArray();

            return string.Join("\n", recentLines);
        }
    }
}
