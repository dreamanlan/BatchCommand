using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
    public class CodeAnalysis : ICodeAnalysis
    {
        private readonly string _basePath;

        public CodeAnalysis(string basePath)
        {
            _basePath = basePath ?? Directory.GetCurrentDirectory();
        }

        public List<string> FindSymbol(string symbol, string directory, string filePattern = "*.*", bool recursive = true)
        {
            var results = new List<string>();
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            try
            {
                var files = Directory.GetFiles(directory, filePattern, searchOption);
                foreach (var file in files)
                {
                    try
                    {
                        var content = File.ReadAllText(file, Encoding.UTF8);
                        if (content.Contains(symbol))
                        {
                            results.Add(file);
                        }
                    }
                    catch
                    {
                        // Skip files that cannot be read
                    }
                }
            }
            catch
            {
                // Skip directories that cannot be accessed
            }

            return results;
        }

        public List<CodeMatch> SearchCode(string pattern, string directory, string filePattern = "*.*", bool useRegex = false, bool recursive = true)
        {
            var results = new List<CodeMatch>();
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            try
            {
                var files = Directory.GetFiles(directory, filePattern, searchOption);
                foreach (var file in files)
                {
                    try
                    {
                        var lines = File.ReadAllLines(file, Encoding.UTF8);
                        for (int i = 0; i < lines.Length; i++)
                        {
                            bool matched = false;
                            if (useRegex)
                            {
                                matched = Regex.IsMatch(lines[i], pattern);
                            }
                            else
                            {
                                matched = lines[i].Contains(pattern);
                            }

                            if (matched)
                            {
                                results.Add(new CodeMatch
                                {
                                    FilePath = file,
                                    LineNumber = i + 1,
                                    LineContent = lines[i],
                                    Pattern = pattern
                                });
                            }
                        }
                    }
                    catch
                    {
                        // Skip files that cannot be read
                    }
                }
            }
            catch
            {
                // Skip directories that cannot be accessed
            }

            return results;
        }

        public List<FunctionInfo> ExtractFunctions(string filePath, string language = "csharp")
        {
            var functions = new List<FunctionInfo>();

            try
            {
                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

                if (language.ToLower() == "csharp")
                {
                    var functionPattern = @"^\s*(public|private|protected|internal|static|\s)+(async\s+)?(void|int|string|bool|double|float|long|Task|Task<[^>]+>|[A-Z][a-zA-Z0-9_<>,\s]*)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\(([^)]*)\)";
                    ExtractFunctionsWithPattern(filePath, lines, functionPattern, functions);
                }
                else if (language.ToLower() == "javascript" || language.ToLower() == "typescript")
                {
                    var functionPattern = @"^\s*(export\s+)?(async\s+)?function\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\(([^)]*)\)|^\s*(const|let|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(async\s+)?\(([^)]*)\)\s*=>";
                    ExtractFunctionsWithPattern(filePath, lines, functionPattern, functions);
                }
                else if (language.ToLower() == "python")
                {
                    var functionPattern = @"^\s*def\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\(([^)]*)\)";
                    ExtractFunctionsWithPattern(filePath, lines, functionPattern, functions);
                }
            }
            catch
            {
                // Return empty list if file cannot be read
            }

            return functions;
        }

        private void ExtractFunctionsWithPattern(string filePath, string[] lines, string pattern, List<FunctionInfo> functions)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var match = Regex.Match(lines[i], pattern);
                if (match.Success)
                {
                    var functionInfo = new FunctionInfo
                    {
                        FilePath = filePath,
                        LineNumber = i + 1,
                        Name = ExtractFunctionName(match),
                        Signature = lines[i].Trim()
                    };

                    functions.Add(functionInfo);
                }
            }
        }

        private string ExtractFunctionName(Match match)
        {
            for (int i = match.Groups.Count - 1; i >= 0; i--)
            {
                if (!string.IsNullOrWhiteSpace(match.Groups[i].Value) &&
                    Regex.IsMatch(match.Groups[i].Value, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                {
                    return match.Groups[i].Value;
                }
            }
            return "Unknown";
        }

        public List<ClassInfo> ExtractClasses(string filePath, string language = "csharp")
        {
            var classes = new List<ClassInfo>();

            try
            {
                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

                if (language.ToLower() == "csharp")
                {
                    var classPattern = @"^\s*(public|private|protected|internal|\s)+(static\s+)?(class|interface|struct|enum)\s+([a-zA-Z_][a-zA-Z0-9_<>,\s]*)";
                    ExtractClassesWithPattern(filePath, lines, classPattern, classes);
                }
                else if (language.ToLower() == "javascript" || language.ToLower() == "typescript")
                {
                    var classPattern = @"^\s*(export\s+)?(class|interface)\s+([a-zA-Z_][a-zA-Z0-9_]*)";
                    ExtractClassesWithPattern(filePath, lines, classPattern, classes);
                }
                else if (language.ToLower() == "python")
                {
                    var classPattern = @"^\s*class\s+([a-zA-Z_][a-zA-Z0-9_]*)";
                    ExtractClassesWithPattern(filePath, lines, classPattern, classes);
                }
            }
            catch
            {
                // Return empty list if file cannot be read
            }

            return classes;
        }

        private void ExtractClassesWithPattern(string filePath, string[] lines, string pattern, List<ClassInfo> classes)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var match = Regex.Match(lines[i], pattern);
                if (match.Success)
                {
                    var classInfo = new ClassInfo
                    {
                        FilePath = filePath,
                        LineNumber = i + 1,
                        Name = ExtractClassName(match),
                        Signature = lines[i].Trim()
                    };

                    classes.Add(classInfo);
                }
            }
        }

        private string ExtractClassName(Match match)
        {
            for (int i = match.Groups.Count - 1; i >= 0; i--)
            {
                if (!string.IsNullOrWhiteSpace(match.Groups[i].Value) &&
                    Regex.IsMatch(match.Groups[i].Value, @"^[a-zA-Z_][a-zA-Z0-9_<>,\s]*$"))
                {
                    return match.Groups[i].Value.Trim();
                }
            }
            return "Unknown";
        }

        public List<string> ExtractImports(string filePath, string language = "csharp")
        {
            var imports = new List<string>();

            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);

                if (language.ToLower() == "csharp")
                {
                    var importPattern = @"^\s*using\s+([a-zA-Z_][a-zA-Z0-9_.]*)\s*;";
                    ExtractImportsWithPattern(lines, importPattern, imports);
                }
                else if (language.ToLower() == "javascript" || language.ToLower() == "typescript")
                {
                    var importPattern = @"^\s*import\s+.*\s+from\s+['""]([^'""]+)['""]";
                    ExtractImportsWithPattern(lines, importPattern, imports);
                }
                else if (language.ToLower() == "python")
                {
                    var importPattern = @"^\s*(import|from)\s+([a-zA-Z_][a-zA-Z0-9_.]*)";
                    ExtractImportsWithPattern(lines, importPattern, imports);
                }
            }
            catch
            {
                // Return empty list if file cannot be read
            }

            return imports;
        }

        private void ExtractImportsWithPattern(string[] lines, string pattern, List<string> imports)
        {
            foreach (var line in lines)
            {
                var match = Regex.Match(line, pattern);
                if (match.Success && match.Groups.Count > 1)
                {
                    var importName = match.Groups[match.Groups.Count - 1].Value;
                    if (!string.IsNullOrWhiteSpace(importName))
                    {
                        imports.Add(importName);
                    }
                }
            }
        }

        public CodeStatistics AnalyzeFile(string filePath)
        {
            var stats = new CodeStatistics { FilePath = filePath };

            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                stats.TotalLines = lines.Length;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed))
                    {
                        stats.BlankLines++;
                    }
                    else if (trimmed.StartsWith("//") || trimmed.StartsWith("#") ||
                             trimmed.StartsWith("/*") || trimmed.StartsWith("*"))
                    {
                        stats.CommentLines++;
                    }
                    else
                    {
                        stats.CodeLines++;
                    }
                }
            }
            catch
            {
                // Return empty stats if file cannot be read
            }

            return stats;
        }

        public Dictionary<string, CodeStatistics> AnalyzeDirectory(string directory, string filePattern = "*.*", bool recursive = true)
        {
            var results = new Dictionary<string, CodeStatistics>();
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            try
            {
                var files = Directory.GetFiles(directory, filePattern, searchOption);
                foreach (var file in files)
                {
                    var stats = AnalyzeFile(file);
                    results[file] = stats;
                }
            }
            catch
            {
                // Return empty results if directory cannot be accessed
            }

            return results;
        }
    }

    public class CodeMatch
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string LineContent { get; set; }
        public string Pattern { get; set; }
    }

    public class FunctionInfo
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string Name { get; set; }
        public string Signature { get; set; }
    }

    public class ClassInfo
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string Name { get; set; }
        public string Signature { get; set; }
    }

    public class CodeStatistics
    {
        public string FilePath { get; set; }
        public int TotalLines { get; set; }
        public int CodeLines { get; set; }
        public int CommentLines { get; set; }
        public int BlankLines { get; set; }
    }
}
