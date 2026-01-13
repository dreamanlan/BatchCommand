using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CefDotnetApp.AgentCore.Models;
using CefDotnetApp.AgentCore.Utils;
using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
    public class FileOperations : IFileOperations
    {
        private readonly string _basePath;

        public FileOperations(string basePath)
        {
            _basePath = basePath ?? Directory.GetCurrentDirectory();
        }

        public string ReadFile(string path)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return null;

                return File.ReadAllText(fullPath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to read file: {path}", ex);
            }
        }

        public byte[] ReadFileBytes(string path)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return null;

                return File.ReadAllBytes(fullPath);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to read file bytes: {path}", ex);
            }
        }

        public string[] ReadFileLines(string path)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return null;

                return File.ReadAllLines(fullPath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to read file lines: {path}", ex);
            }
        }

        public bool WriteFile(string path, string content, bool createDirectory = true)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);

                if (createDirectory)
                {
                    string directory = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                }

                File.WriteAllText(fullPath, content, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to write file: {path}", ex);
            }
        }

        public bool WriteFileBytes(string path, byte[] content, bool createDirectory = true)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);

                if (createDirectory)
                {
                    string directory = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                }

                File.WriteAllBytes(fullPath, content);
                return true;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to write file bytes: {path}", ex);
            }
        }

        public bool AppendFile(string path, string content)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                File.AppendAllText(fullPath, content, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to append file: {path}", ex);
            }
        }

        public bool DeleteFile(string path)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to delete file: {path}", ex);
            }
        }

        public bool CopyFile(string sourcePath, string destPath, bool overwrite = false)
        {
            try
            {
                string fullSourcePath = PathHelper.EnsureAbsolutePath(sourcePath, _basePath);
                string fullDestPath = PathHelper.EnsureAbsolutePath(destPath, _basePath);

                if (!File.Exists(fullSourcePath))
                    return false;

                string destDirectory = Path.GetDirectoryName(fullDestPath);
                if (!string.IsNullOrEmpty(destDirectory) && !Directory.Exists(destDirectory))
                    Directory.CreateDirectory(destDirectory);

                File.Copy(fullSourcePath, fullDestPath, overwrite);
                return true;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to copy file from {sourcePath} to {destPath}", ex);
            }
        }

        public bool MoveFile(string sourcePath, string destPath, bool overwrite = false)
        {
            try
            {
                string fullSourcePath = PathHelper.EnsureAbsolutePath(sourcePath, _basePath);
                string fullDestPath = PathHelper.EnsureAbsolutePath(destPath, _basePath);

                if (!File.Exists(fullSourcePath))
                    return false;

                string destDirectory = Path.GetDirectoryName(fullDestPath);
                if (!string.IsNullOrEmpty(destDirectory) && !Directory.Exists(destDirectory))
                    Directory.CreateDirectory(destDirectory);

                if (overwrite && File.Exists(fullDestPath))
                    File.Delete(fullDestPath);

                File.Move(fullSourcePath, fullDestPath);
                return true;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to move file from {sourcePath} to {destPath}", ex);
            }
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
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!Directory.Exists(fullPath))
                    return new List<FileInfoModel>();

                var result = new List<FileInfoModel>();
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                var files = Directory.GetFiles(fullPath, searchPattern, searchOption);
                foreach (var file in files)
                {
                    result.Add(new FileInfoModel(file));
                }

                var directories = Directory.GetDirectories(fullPath, searchPattern, searchOption);
                foreach (var dir in directories)
                {
                    result.Add(new FileInfoModel(dir));
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to list directory: {path}", ex);
            }
        }

        public bool CreateDirectory(string path)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to create directory: {path}", ex);
            }
        }

        public bool DeleteDirectory(string path, bool recursive = false)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, recursive);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to delete directory: {path}", ex);
            }
        }

        public List<string> FindFiles(string path, string pattern, bool recursive = true)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!Directory.Exists(fullPath))
                    return new List<string>();

                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                return Directory.GetFiles(fullPath, pattern, searchOption).ToList();
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to find files in: {path}", ex);
            }
        }

        // Code editing methods

        public bool ReplaceInFile(string path, string oldString, string newString, bool allOccurrences = false)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return false;

                string content = File.ReadAllText(fullPath, Encoding.UTF8);

                if (allOccurrences)
                {
                    content = content.Replace(oldString, newString);
                }
                else
                {
                    int index = content.IndexOf(oldString, StringComparison.Ordinal);
                    if (index >= 0)
                    {
                        content = content.Substring(0, index) + newString + content.Substring(index + oldString.Length);
                    }
                    else
                    {
                        return false;
                    }
                }

                File.WriteAllText(fullPath, content, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to replace in file: {path}", ex);
            }
        }

        public bool ReplaceLines(string path, int startLine, int endLine, string newContent)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return false;

                string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);

                if (startLine < 1 || endLine > lines.Length || startLine > endLine)
                    return false;

                string[] newLines = newContent.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                string[] result = new string[lines.Length - (endLine - startLine + 1) + newLines.Length];

                int index = 0;
                for (int i = 0; i < startLine - 1; i++)
                {
                    result[index++] = lines[i];
                }
                for (int i = 0; i < newLines.Length; i++)
                {
                    result[index++] = newLines[i];
                }
                for (int i = endLine; i < lines.Length; i++)
                {
                    result[index++] = lines[i];
                }

                File.WriteAllLines(fullPath, result, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to replace lines in file: {path}", ex);
            }
        }

        public bool InsertAfter(string path, string searchPattern, string content, bool allOccurrences = false)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return false;

                string fileContent = File.ReadAllText(fullPath, Encoding.UTF8);

                if (allOccurrences)
                {
                    fileContent = fileContent.Replace(searchPattern, searchPattern + content);
                }
                else
                {
                    int index = fileContent.IndexOf(searchPattern, StringComparison.Ordinal);
                    if (index >= 0)
                    {
                        fileContent = fileContent.Substring(0, index + searchPattern.Length) + content + fileContent.Substring(index + searchPattern.Length);
                    }
                    else
                    {
                        return false;
                    }
                }

                File.WriteAllText(fullPath, fileContent, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to insert after pattern in file: {path}", ex);
            }
        }

        public bool InsertBefore(string path, string searchPattern, string content, bool allOccurrences = false)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return false;

                string fileContent = File.ReadAllText(fullPath, Encoding.UTF8);

                if (allOccurrences)
                {
                    fileContent = fileContent.Replace(searchPattern, content + searchPattern);
                }
                else
                {
                    int index = fileContent.IndexOf(searchPattern, StringComparison.Ordinal);
                    if (index >= 0)
                    {
                        fileContent = fileContent.Substring(0, index) + content + fileContent.Substring(index);
                    }
                    else
                    {
                        return false;
                    }
                }

                File.WriteAllText(fullPath, fileContent, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to insert before pattern in file: {path}", ex);
            }
        }

        public bool DeleteLines(string path, int startLine, int endLine)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return false;

                string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);

                if (startLine < 1 || endLine > lines.Length || startLine > endLine)
                    return false;

                string[] result = new string[lines.Length - (endLine - startLine + 1)];
                int index = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i + 1 < startLine || i + 1 > endLine)
                    {
                        result[index++] = lines[i];
                    }
                }

                File.WriteAllLines(fullPath, result, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to delete lines in file: {path}", ex);
            }
        }

        public List<int> SearchInFile(string path, string pattern, bool useRegex = false)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return new List<int>();

                var lines = File.ReadAllLines(fullPath, Encoding.UTF8);
                var result = new List<int>();

                if (useRegex)
                {
                    var regex = new Regex(pattern, RegexOptions.Multiline);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (regex.IsMatch(lines[i]))
                        {
                            result.Add(i + 1);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains(pattern))
                        {
                            result.Add(i + 1);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to search in file: {path}", ex);
            }
        }

        public string[] ReadLinesRange(string path, int startLine, int endLine)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return null;

                string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);

                if (startLine < 1 || endLine > lines.Length || startLine > endLine)
                    return null;

                string[] result = new string[endLine - startLine + 1];
                int index = 0;
                for (int i = startLine - 1; i < endLine; i++)
                {
                    result[index++] = lines[i];
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to read lines from file: {path}", ex);
            }
        }

        public int GetLineCount(string path)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return 0;

                string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);
                return lines.Length;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to get line count of file: {path}", ex);
            }
        }

        // Advanced code analysis methods

        public CodeSymbolLocation FindSymbolDefinition(string path, string symbolName, string symbolType = "")
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return null;

                string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);
                string pattern = symbolName.Replace(" ", "\\s+");

                string[] typePatterns;
                if (string.IsNullOrEmpty(symbolType))
                {
                    typePatterns = new[] {
                        $"class\\s+{pattern}",
                        $"interface\\s+{pattern}",
                        $"struct\\s+{pattern}",
                        $"enum\\s+{pattern}",
                        $@"\b(?:public|private|protected|internal)\s+(?:static\s+)?(?:virtual\s+)?(?:override\s+)?(?:async\s+)?\w+\s+{pattern}\s*\(",
                        $@"\bvoid\s+{pattern}\s*\(",
                        $@"\b\w+\s+{pattern}\s*\(",
                        $"var\\s+{pattern}\\s*=",
                        $@"\w+\s+{pattern}\\s*=",
                        $@"\b{pattern}\s*="
                    };
                }
                else
                {
                    switch (symbolType.ToLower())
                    {
                        case "class":
                            typePatterns = new[] { $"class\\s+{pattern}" };
                            break;
                        case "interface":
                            typePatterns = new[] { $"interface\\s+{pattern}" };
                            break;
                        case "struct":
                            typePatterns = new[] { $"struct\\s+{pattern}" };
                            break;
                        case "enum":
                            typePatterns = new[] { $"enum\\s+{pattern}" };
                            break;
                        case "function":
                        case "method":
                            typePatterns = new[] {
                                $@"\b(?:public|private|protected|internal)\s+(?:static\s+)?(?:virtual\s+)?(?:override\s+)?(?:async\s+)?\w+\s+{pattern}\s*\(",
                                $@"\bvoid\s+{pattern}\s*\(",
                                $@"\b\w+\s+{pattern}\s*\("
                            };
                            break;
                        case "variable":
                        case "field":
                            typePatterns = new[] {
                                $"var\\s+{pattern}\\s*=",
                                $@"\b\w+\s+{pattern}\\s*="
                            };
                            break;
                        default:
                            typePatterns = new[] {
                                $@"\b{symbolType}\s+{pattern}",
                                $@"\b{pattern}\s*="
                            };
                            break;
                    }
                }

                for (int i = 0; i < lines.Length; i++)
                {
                    foreach (var typePattern in typePatterns)
                    {
                        var regex = new Regex(typePattern, RegexOptions.Multiline);
                        if (regex.IsMatch(lines[i]))
                        {
                        var match = regex.Match(lines[i]);
                        int column = match.Index + 1;
                        return new CodeSymbolLocation
                        {
                            LineNumber = i + 1,
                            Column = column,
                            Length = symbolName.Length,
                            Context = lines[i].Trim(),
                            SymbolName = symbolName
                        };
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to find symbol definition: {path}", ex);
            }

            return (CodeSymbolLocation)null;
        }

        public List<CodeSymbolLocation> FindSymbolUsages(string path, string symbolName)        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return new List<CodeSymbolLocation>();

                string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);
                var result = new List<CodeSymbolLocation>();

                var pattern = new Regex($@"\b{Regex.Escape(symbolName)}\b", RegexOptions.Multiline);

                for (int i = 0; i < lines.Length; i++)
                {
                    var matches = pattern.Matches(lines[i]);
                    foreach (Match match in matches)
                    {
                        var line = lines[i];

                        // Filter out definition lines (class/interface/struct declarations)
                        if (Regex.IsMatch(line, $@"\b(?:class|interface|struct|enum)\s+{Regex.Escape(symbolName)}\b"))
                            continue;

                        // Filter out type declarations
                        if (Regex.IsMatch(line, $@"\b{Regex.Escape(symbolName)}\s*\("))
                        {
                            // Check if it's a function definition
                            if (Regex.IsMatch(line, $@"\b(?:public|private|protected|internal|static|virtual|override|async)\s+\w+\s+{Regex.Escape(symbolName)}\s*\("))
                                continue;
                        }

                        result.Add(new CodeSymbolLocation
                        {
                            LineNumber = i + 1,
                            Column = match.Index + 1,
                            Length = match.Length,
                            Context = line.Trim(),
                            SymbolName = symbolName
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to find symbol usages: {path}", ex);
            }
        }

        public List<CodeFunctionInfo> GetAllFunctions(string path)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return new List<CodeFunctionInfo>();

                string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);
                var result = new List<CodeFunctionInfo>();

                var methodPattern = new Regex(
                    @"^(?:\s*)?(?<modifiers>(?:public|private|protected|internal|static|virtual|override|async|unsafe)\s+)*(?<returntype>\w+(?:<[^>]+>)?)\s+(?<name>\w+)\s*\(",
                    RegexOptions.Multiline);

                var voidPattern = new Regex(
                    @"^(?:\s*)?(?<modifiers>(?:public|private|protected|internal|static|virtual|override|async|unsafe)\s+)*void\s+(?<name>\w+)\s*\(",
                    RegexOptions.Multiline);

                for (int i = 0; i < lines.Length; i++)
                {
                    Match match = methodPattern.Match(lines[i]);
                    if (!match.Success)
                        match = voidPattern.Match(lines[i]);

                    if (match.Success)
                    {
                        string modifiers = match.Groups["modifiers"].Value.Trim();
                        string returnType = match.Groups["returntype"]?.Value ?? "void";
                        string name = match.Groups["name"].Value;

                        var functionInfo = new CodeFunctionInfo
                        {
                            Name = name,
                            ReturnType = returnType,
                            Modifiers = string.IsNullOrEmpty(modifiers) ? "public" : modifiers,
                            StartLine = i + 1,
                            StartColumn = match.Groups["name"].Index + 1
                        };

                        // Find end of function
                        functionInfo.EndLine = FindBlockEnd(lines, i + 1);

                        result.Add(functionInfo);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to get all functions: {path}", ex);
            }
        }

        public List<CodeClassInfo> GetAllClasses(string path)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return new List<CodeClassInfo>();

                string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);
                var result = new List<CodeClassInfo>();

                for (int i = 0; i < lines.Length; i++)
                {
                    var classMatch = Regex.Match(lines[i], @"^\s*class\s+(\w+)");
                    var interfaceMatch = Regex.Match(lines[i], @"^\s*interface\s+(\w+)");
                    var structMatch = Regex.Match(lines[i], @"^\s*struct\s+(\w+)");

                    Match match = classMatch.Success ? classMatch : (interfaceMatch.Success ? interfaceMatch : structMatch);

                    if (match.Success)
                    {
                        var classInfo = new CodeClassInfo
                        {
                            Name = match.Groups[1].Value,
                            Type = classMatch.Success ? "class" : (interfaceMatch.Success ? "interface" : "struct"),
                            StartLine = i + 1,
                            StartColumn = match.Groups[1].Index + 1
                        };

                        classInfo.EndLine = FindBlockEnd(lines, i + 1);

                        result.Add(classInfo);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to get all classes: {path}", ex);
            }
        }

        public CodeFunctionRange FindFunctionRange(string path, string functionName)
        {
            try
            {
                var functions = GetAllFunctions(path);
                var function = functions.FirstOrDefault(f => f.Name == functionName);

                if (function == null)
                    return null;

                return new CodeFunctionRange
                {
                    Name = function.Name,
                    StartLine = function.StartLine,
                    EndLine = function.EndLine,
                    ReturnType = function.ReturnType,
                    Modifiers = function.Modifiers
                };
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to find function range: {path}", ex);
            }
        }

        public CodeClassRange FindClassRange(string path, string className)
        {
            try
            {
                var classes = GetAllClasses(path);
                var classInfo = classes.FirstOrDefault(c => c.Name == className);

                if (classInfo == null)
                    return null;

                return new CodeClassRange
                {
                    Name = classInfo.Name,
                    Type = classInfo.Type,
                    StartLine = classInfo.StartLine,
                    EndLine = classInfo.EndLine
                };
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to find class range: {path}", ex);
            }
        }

        public List<string> GetImportStatements(string path)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return new List<string>();

                string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);
                var result = new List<string>();

                var importPattern = new Regex(@"^\s*using\s+(.+?);", RegexOptions.Multiline);

                for (int i = 0; i < lines.Length; i++)
                {
                    var match = importPattern.Match(lines[i]);
                    if (match.Success)
                    {
                        result.Add(lines[i].Trim());
                    }
                    else if (i > 100) // Usually imports are at the beginning
                    {
                        break;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to get import statements: {path}", ex);
            }
        }

        public bool AddImportStatement(string path, string importStatement)
        {
            try
            {
                string fullPath = PathHelper.EnsureAbsolutePath(path, _basePath);
                if (!File.Exists(fullPath))
                    return false;

                var existingImports = GetImportStatements(path);
                string importLine = importStatement.TrimEnd();

                if (!importLine.EndsWith(";"))
                    importLine += ";";

                // Check if import already exists
                foreach (var imp in existingImports)
                {
                    if (imp.Equals(importLine, StringComparison.OrdinalIgnoreCase))
                        return true; // Already exists
                }

                string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);

                // Find the last using statement
                int lastUsingLine = -1;
                var importPattern = new Regex(@"^\s*using\s+", RegexOptions.Multiline);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (importPattern.IsMatch(lines[i]))
                        lastUsingLine = i;
                }

                // Insert after the last using statement
                var result = lines.ToList();
                if (lastUsingLine >= 0)
                {
                    result.Insert(lastUsingLine + 1, importLine);
                }
                else
                {
                    result.Insert(0, importLine);
                }

                File.WriteAllLines(fullPath, result, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to add import statement: {path}", ex);
            }
        }

        private int FindBlockEnd(string[] lines, int startLine)
        {
            int braceCount = 0;
            bool inBlock = false;

            for (int i = startLine - 1; i < lines.Length; i++)
            {
                string line = lines[i];

                // Count braces
                for (int j = 0; j < line.Length; j++)
                {
                    if (line[j] == '{')
                    {
                        braceCount++;
                        inBlock = true;
                    }
                    else if (line[j] == '}')
                    {
                        braceCount--;
                        if (inBlock && braceCount == 0)
                        {
                            return i + 1;
                        }
                    }
                }
            }

            // If block end not found, return the line after start
            return startLine;
        }
    }

    // Supporting classes for advanced code editing

    public class CodeSymbolLocation
    {
        public int LineNumber { get; set; }
        public int Column { get; set; }
        public int Length { get; set; }
        public string Context { get; set; }
        public string SymbolName { get; set; }
    }

    public class CodeFunctionInfo
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public string Modifiers { get; set; }
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
    }

    public class CodeClassInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
    }

    public class CodeFunctionRange
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public string Modifiers { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
    }

    public class CodeClassRange
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
    }
}
