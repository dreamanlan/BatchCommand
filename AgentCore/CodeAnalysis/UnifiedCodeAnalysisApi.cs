using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CefDotnetApp.AgentCore.Models;

namespace AgentCore.CodeAnalysis
{
    // Unified code analysis API (language-agnostic)
    public class UnifiedCodeAnalysisApi
    {
        private readonly Dictionary<ProgrammingLanguage, ICodeParser> _parsers;
        private readonly ParseCache _cache;

        // Cache entry for parsed files
        private class CacheEntry
        {
            public ParsedCodeFile ParsedFile { get; set; }
            public DateTime LastModified { get; set; }
            public string CodeHash { get; set; }
            public bool HasSyntaxErrors { get; set; }
            public List<string> SyntaxErrors { get; set; }
        }

        // Parse cache
        private class ParseCache
        {
            private readonly Dictionary<string, CacheEntry> _fileCache = new Dictionary<string, CacheEntry>();
            private readonly Dictionary<string, CacheEntry> _codeCache = new Dictionary<string, CacheEntry>();
            private const int MaxCodeCacheSize = 100;

            public bool TryGetFile(string filePath, DateTime lastModified, out CacheEntry entry)
            {
                if (_fileCache.TryGetValue(filePath, out entry))
                {
                    if (entry.LastModified == lastModified)
                    {
                        return true;
                    }
                    // File modified, remove old cache
                    _fileCache.Remove(filePath);
                }
                entry = null;
                return false;
            }

            public void SetFile(string filePath, DateTime lastModified, ParsedCodeFile parsedFile, bool hasErrors, List<string> errors)
            {
                _fileCache[filePath] = new CacheEntry
                {
                    ParsedFile = parsedFile,
                    LastModified = lastModified,
                    HasSyntaxErrors = hasErrors,
                    SyntaxErrors = errors ?? new List<string>()
                };
            }

            public bool TryGetCode(string codeHash, out CacheEntry entry)
            {
                return _codeCache.TryGetValue(codeHash, out entry);
            }

            public void SetCode(string codeHash, ParsedCodeFile parsedFile, bool hasErrors, List<string> errors)
            {
                // Limit cache size
                if (_codeCache.Count >= MaxCodeCacheSize)
                {
                    // Remove oldest entry (simple FIFO)
                    var firstKey = _codeCache.Keys.First();
                    _codeCache.Remove(firstKey);
                }

                _codeCache[codeHash] = new CacheEntry
                {
                    ParsedFile = parsedFile,
                    CodeHash = codeHash,
                    HasSyntaxErrors = hasErrors,
                    SyntaxErrors = errors ?? new List<string>()
                };
            }

            public void Clear()
            {
                _fileCache.Clear();
                _codeCache.Clear();
            }
        }

        // Compute hash for code string
        private static string ComputeCodeHash(string code)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(code);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // Generate cache key with hash and length to avoid collision
        private static string GenerateCodeCacheKey(string code)
        {
            var hash = ComputeCodeHash(code);
            var length = code.Length;
            return $"{hash}:{length}";
        }

        // Check syntax errors in Roslyn syntax tree
        private static (bool hasErrors, List<string> errors) CheckSyntaxErrors(Microsoft.CodeAnalysis.SyntaxTree tree)
        {
            var diagnostics = tree.GetDiagnostics();
            var errors = diagnostics
                .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
                .Select(d => $"{d.Location.GetLineSpan().StartLinePosition.Line + 1}:{d.Location.GetLineSpan().StartLinePosition.Character + 1} - {d.GetMessage()}")
                .ToList();

            return (errors.Count > 0, errors);
        }

        // Check if parsed file has syntax errors and return error message if any
        private static string CheckParsedFileSyntaxErrors(ParsedCodeFile parsed)
        {
            if (parsed.HasSyntaxErrors && parsed.SyntaxErrors != null && parsed.SyntaxErrors.Count > 0)
            {
                return $"Syntax errors found:\n{string.Join("\n", parsed.SyntaxErrors)}";
            }
            return null;
        }

        public UnifiedCodeAnalysisApi()
        {
            _parsers = new Dictionary<ProgrammingLanguage, ICodeParser>();
            _cache = new ParseCache();

            // Register TreeSitter parsers
            _parsers[ProgrammingLanguage.C] = new TreeSitterCParser();
            _parsers[ProgrammingLanguage.Cpp] = new TreeSitterCppParser();

            // Register Jint parser for JavaScript (pure C#, hot-reloadable)
            _parsers[ProgrammingLanguage.JavaScript] = new JintCodeParser();
        }

        public ParsedCodeFile ParseFile(string filePath)
        {
            var language = DetectLanguage(filePath);
            return ParseFile(filePath, language);
        }

        public ParsedCodeFile ParseFile(string filePath, ProgrammingLanguage language)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            // Check cache first (for all languages)
            var lastModified = File.GetLastWriteTime(filePath);
            if (_cache.TryGetFile(filePath, lastModified, out var cacheEntry))
            {
                return cacheEntry.ParsedFile;
            }

            ParsedCodeFile result;
            bool hasErrors = false;
            List<string> errors = new List<string>();

            if (language == ProgrammingLanguage.CSharp)
            {
                result = ParseCSharpFile(filePath);

                // Check for syntax errors
                var tree = RoslynParser.ParseFile(filePath);
                var (hasErr, errList) = CheckSyntaxErrors(tree);
                hasErrors = hasErr;
                errors = errList;

                // Set error info in result
                result.HasSyntaxErrors = hasErrors;
                result.SyntaxErrors = errors;
            }
            else if (_parsers.TryGetValue(language, out var parser))
            {
                result = parser.Parse(filePath);
                // For other languages, check if result has syntax errors
                hasErrors = result.HasSyntaxErrors;
                errors = result.SyntaxErrors ?? new List<string>();
            }
            else
            {
                throw new NotSupportedException($"Language {language} is not supported");
            }

            // Cache the result (for all languages)
            _cache.SetFile(filePath, lastModified, result, hasErrors, errors);

            return result;
        }

        public ParsedCodeFile ParseText(string code, ProgrammingLanguage language, string filePath = null)
        {
            // Check cache first (for all languages)
            var cacheKey = GenerateCodeCacheKey(code);
            if (_cache.TryGetCode(cacheKey, out var cacheEntry))
            {
                return cacheEntry.ParsedFile;
            }

            ParsedCodeFile result;
            bool hasErrors = false;
            List<string> errors = new List<string>();

            if (language == ProgrammingLanguage.CSharp)
            {
                result = ParseCSharpText(code, filePath);

                // Check for syntax errors
                var tree = RoslynParser.ParseCode(code, filePath ?? "<text>");
                var (hasErr, errList) = CheckSyntaxErrors(tree);
                hasErrors = hasErr;
                errors = errList;

                // Set error info in result
                result.HasSyntaxErrors = hasErrors;
                result.SyntaxErrors = errors;
            }
            else if (_parsers.TryGetValue(language, out var parser))
            {
                result = parser.ParseText(code, filePath);
                // For other languages, check if result has syntax errors
                hasErrors = result.HasSyntaxErrors;
                errors = result.SyntaxErrors ?? new List<string>();
            }
            else
            {
                throw new NotSupportedException($"Language {language} is not supported");
            }

            // Cache the result (for all languages)
            _cache.SetCode(cacheKey, result, hasErrors, errors);

            return result;
        }

        public List<FunctionInfo> FindFunctions(ParsedCodeFile parsed, string namePattern = null, bool ignoreCase = true)
        {
            if (parsed.Language == ProgrammingLanguage.CSharp)
            {
                return FindCSharpFunctions(parsed, namePattern, ignoreCase);
            }

            if (_parsers.TryGetValue(parsed.Language, out var parser))
            {
                return parser.FindFunctions(parsed, namePattern, ignoreCase);
            }

            return new List<FunctionInfo>();
        }

        public List<TypeInfo> FindTypes(ParsedCodeFile parsed, string namePattern = null, bool ignoreCase = true)
        {
            if (parsed.Language == ProgrammingLanguage.CSharp)
            {
                return FindCSharpTypes(parsed, namePattern, ignoreCase);
            }

            if (_parsers.TryGetValue(parsed.Language, out var parser))
            {
                return parser.FindTypes(parsed, namePattern, ignoreCase);
            }

            return new List<TypeInfo>();
        }

        public FunctionInfo FindFunction(ParsedCodeFile parsed, string functionName, bool ignoreCase = true)
        {
            if (parsed.Language == ProgrammingLanguage.CSharp)
            {
                // First, search in global functions
                foreach (var func in parsed.Functions)
                {
                    if (MatchesPattern(func.Name, functionName, ignoreCase))
                    {
                        return func;
                    }
                }

                // If not found, search in class methods, properties and constructors
                if (parsed.NativeSyntaxTree is RoslynParsedFile roslynFile)
                {
                    foreach (var cls in roslynFile.Classes)
                    {
                        foreach (var method in cls.Methods)
                        {
                            if (MatchesPattern(method.Name, functionName, ignoreCase))
                            {
                                return ConvertFromRoslynMethod(method);
                            }
                        }

                        foreach (var property in cls.Properties)
                        {
                            if (MatchesPattern(property.Name, functionName, ignoreCase))
                            {
                                return ConvertRoslynPropertyToFunctionInfo(property);
                            }
                        }

                        foreach (var constructor in cls.Constructors)
                        {
                            if (MatchesPattern(constructor.Name, functionName, ignoreCase))
                            {
                                return ConvertFromRoslynConstructor(constructor);
                            }
                        }
                    }
                }

                return null;
            }

            if (_parsers.TryGetValue(parsed.Language, out var parser))
            {
                return parser.FindFunction(parsed, functionName, ignoreCase);
            }

            return null;
        }

        public TypeInfo FindType(ParsedCodeFile parsed, string typeName, bool ignoreCase = true)
        {
            if (parsed.Language == ProgrammingLanguage.CSharp)
            {
                if (parsed.NativeSyntaxTree is RoslynParsedFile roslynFile)
                {
                    foreach (var cls in roslynFile.Classes)
                    {
                        if (MatchesPattern(cls.Name, typeName, ignoreCase))
                        {
                            return ConvertFromRoslynClass(cls);
                        }
                    }
                }
                return null;
            }

            if (_parsers.TryGetValue(parsed.Language, out var parser))
            {
                return parser.FindType(parsed, typeName, ignoreCase);
            }

            return null;
        }

        private ParsedCodeFile ParseCSharpFile(string filePath)
        {
            var roslynParsed = RoslynParser.ParseFileComplete(filePath);
            var result = ConvertFromRoslyn(roslynParsed);

            // Add code metrics analysis
            if (roslynParsed.SyntaxTree != null)
            {
                var (totalLines, codeLines, commentLines) = CodeMetricsAnalyzer.AnalyzeFileLines(roslynParsed.SyntaxTree);
                result.TotalLines = totalLines;
                result.CodeLines = codeLines;
                result.CommentLines = commentLines;

                // Extract dependencies
                result.Dependencies = CodeMetricsAnalyzer.ExtractDependencies(roslynParsed.SyntaxTree);
            }

            return result;
        }

        private ParsedCodeFile ParseCSharpText(string code, string filePath)
        {
            var tree = RoslynParser.ParseCode(code, filePath ?? "<text>");
            var root = tree.GetRoot();
            var roslynParsed = new RoslynParsedFile(filePath ?? "<text>");

            // Extract usings
            var usingDirectives = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax>();
            foreach (var usingDir in usingDirectives) {
                roslynParsed.Usings.Add(usingDir.Name.ToString());
            }

            // Extract namespace
            var namespaceDecl = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax>().FirstOrDefault();
            if (namespaceDecl != null) {
                roslynParsed.Namespace = namespaceDecl.Name.ToString();
            }
            else {
                var fileScopedNamespace = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
                if (fileScopedNamespace != null) {
                    roslynParsed.Namespace = fileScopedNamespace.Name.ToString();
                }
            }

            // Extract classes
            roslynParsed.Classes = RoslynParser.ExtractClasses(tree);

            return ConvertFromRoslyn(roslynParsed);
        }

        private List<FunctionInfo> FindCSharpFunctions(ParsedCodeFile parsed, string namePattern, bool ignoreCase = true)
        {
            var result = new List<FunctionInfo>();

            if (parsed.NativeSyntaxTree is RoslynParsedFile roslynFile)
            {
                // Include global functions (if any)
                foreach (var func in parsed.Functions)
                {
                    if (string.IsNullOrEmpty(namePattern) || MatchesPattern(func.Name, namePattern, ignoreCase))
                    {
                        result.Add(func);
                    }
                }

                // Include all class methods
                foreach (var cls in roslynFile.Classes)
                {
                    foreach (var method in cls.Methods)
                    {
                        if (string.IsNullOrEmpty(namePattern) || MatchesPattern(method.Name, namePattern, ignoreCase))
                        {
                            result.Add(ConvertFromRoslynMethod(method));
                        }
                    }

                    // Include all class properties
                    foreach (var property in cls.Properties)
                    {
                        if (string.IsNullOrEmpty(namePattern) || MatchesPattern(property.Name, namePattern, ignoreCase))
                        {
                            result.Add(ConvertRoslynPropertyToFunctionInfo(property));
                        }
                    }

                    // Include all class constructors
                    foreach (var constructor in cls.Constructors)
                    {
                        if (string.IsNullOrEmpty(namePattern) || MatchesPattern(constructor.Name, namePattern, ignoreCase))
                        {
                            result.Add(ConvertFromRoslynConstructor(constructor));
                        }
                    }
                }
            }

            return result;
        }

        private List<TypeInfo> FindCSharpTypes(ParsedCodeFile parsed, string namePattern, bool ignoreCase = true)
        {
            var result = new List<TypeInfo>();

            if (parsed.NativeSyntaxTree is RoslynParsedFile roslynFile)
            {
                foreach (var cls in roslynFile.Classes)
                {
                    if (string.IsNullOrEmpty(namePattern) || MatchesPattern(cls.Name, namePattern, ignoreCase))
                    {
                        result.Add(ConvertFromRoslynClass(cls));
                    }
                }
            }

            return result;
        }

        private ParsedCodeFile ConvertFromRoslyn(RoslynParsedFile roslynFile)
        {
            var result = new ParsedCodeFile(roslynFile.FilePath, ProgrammingLanguage.CSharp)
            {
                Namespace = roslynFile.Namespace,
                NativeSyntaxTree = roslynFile
            };

            result.Imports.AddRange(roslynFile.Usings);

            foreach (var cls in roslynFile.Classes)
            {
                result.Types.Add(ConvertFromRoslynClass(cls));
            }

            foreach (var iface in roslynFile.Interfaces)
            {
                result.Interfaces.Add(ConvertFromRoslynInterface(iface));
            }

            foreach (var strct in roslynFile.Structs)
            {
                result.Structs.Add(ConvertFromRoslynStruct(strct));
            }

            foreach (var enm in roslynFile.Enums)
            {
                result.Enums.Add(ConvertFromRoslynEnum(enm));
            }

            return result;
        }

        private TypeInfo ConvertFromRoslynClass(RoslynClassInfo roslynClass)
        {
            var typeInfo = new TypeInfo
            {
                Name = roslynClass.Name,
                Namespace = roslynClass.Namespace,
                Type = CodeElementType.Class,
                Location = roslynClass.Location,
                Modifiers = roslynClass.Modifiers,
                Language = ProgrammingLanguage.CSharp,
                NativeObject = roslynClass,
                IsNested = roslynClass.IsNested,
                ParentTypeName = roslynClass.ParentTypeName,
                FullTypeName = roslynClass.FullTypeName
            };

            typeInfo.BaseTypes.AddRange(roslynClass.BaseTypes);

            foreach (var method in roslynClass.Methods)
            {
                typeInfo.Methods.Add(ConvertFromRoslynMethod(method));
            }

            foreach (var property in roslynClass.Properties)
            {
                typeInfo.Properties.Add(ConvertFromRoslynProperty(property));
            }

            foreach (var field in roslynClass.Fields)
            {
                typeInfo.Fields.Add(ConvertFromRoslynField(field));
            }

            foreach (var constructor in roslynClass.Constructors)
            {
                typeInfo.Constructors.Add(ConvertFromRoslynConstructor(constructor));
            }

            foreach (var evt in roslynClass.Events)
            {
                typeInfo.Events.Add(ConvertFromRoslynEvent(evt));
            }

            // Add code metrics if syntax node is available
            if (roslynClass.SyntaxNode != null)
            {
                typeInfo.DocumentationComment = CodeMetricsAnalyzer.ExtractDocumentationComment(roslynClass.SyntaxNode);
                typeInfo.LineCount = CodeMetricsAnalyzer.CalculateLineCount(roslynClass.SyntaxNode);
                typeInfo.MemberCount = CodeMetricsAnalyzer.CountMembers(roslynClass.SyntaxNode);
                typeInfo.IsAbstract = CodeMetricsAnalyzer.IsAbstract(roslynClass.SyntaxNode);
                typeInfo.IsSealed = CodeMetricsAnalyzer.IsSealed(roslynClass.SyntaxNode);
                typeInfo.IsPublic = CodeMetricsAnalyzer.IsPublicType(roslynClass.SyntaxNode);
            }

            return typeInfo;
        }

        private FunctionInfo ConvertFromRoslynMethod(RoslynMethodInfo roslynMethod)
        {
            var funcInfo = new FunctionInfo
            {
                Name = roslynMethod.Name,
                ReturnType = roslynMethod.ReturnType,
                Location = roslynMethod.Location,
                Modifiers = roslynMethod.Modifiers,
                FullText = roslynMethod.FullText,
                Language = ProgrammingLanguage.CSharp,
                NativeObject = roslynMethod
            };

            foreach (var param in roslynMethod.Parameters)
            {
                funcInfo.Parameters.Add(new ParameterInfo
                {
                    Name = param.Name,
                    Type = param.Type
                });
            }

            // Add code metrics if syntax node is available
            if (roslynMethod.SyntaxNode != null)
            {
                funcInfo.DocumentationComment = CodeMetricsAnalyzer.ExtractDocumentationComment(roslynMethod.SyntaxNode);
                funcInfo.LineCount = CodeMetricsAnalyzer.CalculateLineCount(roslynMethod.SyntaxNode);
                funcInfo.CyclomaticComplexity = CodeMetricsAnalyzer.CalculateCyclomaticComplexity(roslynMethod.SyntaxNode);
                funcInfo.IsAsync = CodeMetricsAnalyzer.IsAsync(roslynMethod.SyntaxNode);
                funcInfo.IsStatic = CodeMetricsAnalyzer.IsStatic(roslynMethod.SyntaxNode);
                funcInfo.IsPublic = CodeMetricsAnalyzer.IsPublic(roslynMethod.SyntaxNode);
            }

            return funcInfo;
        }

        private PropertyInfo ConvertFromRoslynProperty(RoslynPropertyInfo roslynProperty)
        {
            var propInfo = new PropertyInfo
            {
                Name = roslynProperty.Name,
                Type = roslynProperty.Type,
                Modifiers = roslynProperty.Modifiers,
                Location = roslynProperty.Location,
                HasGetter = roslynProperty.HasGetter,
                HasSetter = roslynProperty.HasSetter,
                IsAutoProperty = roslynProperty.IsAutoProperty,
                NativeObject = roslynProperty
            };

            if (roslynProperty.SyntaxNode != null)
            {
                propInfo.DocumentationComment = CodeMetricsAnalyzer.ExtractDocumentationComment(roslynProperty.SyntaxNode);
                propInfo.IsStatic = CodeMetricsAnalyzer.IsStatic(roslynProperty.SyntaxNode);
                propInfo.IsPublic = CodeMetricsAnalyzer.IsPublic(roslynProperty.SyntaxNode);
            }

            return propInfo;
        }

        private FunctionInfo ConvertPropertyToFunctionInfo(PropertyInfo property)
        {
            var funcInfo = new FunctionInfo
            {
                Name = property.Name,
                ReturnType = property.Type,
                Location = property.Location,
                Modifiers = property.Modifiers,
                Language = property.Language,
                NativeObject = property.NativeObject,
                DocumentationComment = property.DocumentationComment,
                IsStatic = property.IsStatic,
                IsPublic = property.IsPublic
            };

            return funcInfo;
        }

        private FunctionInfo ConvertRoslynPropertyToFunctionInfo(RoslynPropertyInfo roslynProperty)
        {
            var funcInfo = new FunctionInfo
            {
                Name = roslynProperty.Name,
                ReturnType = roslynProperty.Type,
                Location = roslynProperty.Location,
                Modifiers = roslynProperty.Modifiers,
                Language = ProgrammingLanguage.CSharp,
                NativeObject = roslynProperty
            };

            if (roslynProperty.SyntaxNode != null)
            {
                funcInfo.DocumentationComment = CodeMetricsAnalyzer.ExtractDocumentationComment(roslynProperty.SyntaxNode);
                funcInfo.IsStatic = CodeMetricsAnalyzer.IsStatic(roslynProperty.SyntaxNode);
                funcInfo.IsPublic = CodeMetricsAnalyzer.IsPublic(roslynProperty.SyntaxNode);
            }

            return funcInfo;
        }

        private FieldInfo ConvertFromRoslynField(RoslynFieldInfo roslynField)
        {
            var fieldInfo = new FieldInfo
            {
                Name = roslynField.Name,
                Type = roslynField.Type,
                Modifiers = roslynField.Modifiers,
                Location = roslynField.Location,
                IsReadonly = roslynField.IsReadonly,
                IsConst = roslynField.IsConst,
                NativeObject = roslynField
            };

            if (roslynField.SyntaxNode != null)
            {
                fieldInfo.DocumentationComment = CodeMetricsAnalyzer.ExtractDocumentationComment(roslynField.SyntaxNode);
                fieldInfo.IsStatic = CodeMetricsAnalyzer.IsStatic(roslynField.SyntaxNode);
                fieldInfo.IsPublic = CodeMetricsAnalyzer.IsPublic(roslynField.SyntaxNode);
            }

            return fieldInfo;
        }

        private FunctionInfo ConvertFromRoslynConstructor(RoslynConstructorInfo roslynConstructor)
        {
            var funcInfo = new FunctionInfo
            {
                Name = roslynConstructor.Name,
                ReturnType = "void",
                Location = roslynConstructor.Location,
                Modifiers = roslynConstructor.Modifiers,
                FullText = roslynConstructor.FullText,
                Language = ProgrammingLanguage.CSharp,
                NativeObject = roslynConstructor
            };

            foreach (var param in roslynConstructor.Parameters)
            {
                funcInfo.Parameters.Add(new ParameterInfo
                {
                    Name = param.Name,
                    Type = param.Type
                });
            }

            if (roslynConstructor.SyntaxNode != null)
            {
                funcInfo.DocumentationComment = CodeMetricsAnalyzer.ExtractDocumentationComment(roslynConstructor.SyntaxNode);
                funcInfo.LineCount = CodeMetricsAnalyzer.CalculateLineCount(roslynConstructor.SyntaxNode);
                funcInfo.IsPublic = CodeMetricsAnalyzer.IsPublic(roslynConstructor.SyntaxNode);
            }

            return funcInfo;
        }

        private EventInfo ConvertFromRoslynEvent(RoslynEventInfo roslynEvent)
        {
            var eventInfo = new EventInfo
            {
                Name = roslynEvent.Name,
                Type = roslynEvent.Type,
                Modifiers = roslynEvent.Modifiers,
                Location = roslynEvent.Location,
                NativeObject = roslynEvent
            };

            if (roslynEvent.SyntaxNode != null)
            {
                eventInfo.DocumentationComment = CodeMetricsAnalyzer.ExtractDocumentationComment(roslynEvent.SyntaxNode);
                eventInfo.IsStatic = CodeMetricsAnalyzer.IsStatic(roslynEvent.SyntaxNode);
                eventInfo.IsPublic = CodeMetricsAnalyzer.IsPublic(roslynEvent.SyntaxNode);
            }

            return eventInfo;
        }

        private TypeInfo ConvertFromRoslynInterface(RoslynInterfaceInfo roslynInterface)
        {
            var typeInfo = new TypeInfo
            {
                Name = roslynInterface.Name,
                Namespace = roslynInterface.Namespace,
                Type = CodeElementType.Interface,
                Location = roslynInterface.Location,
                Modifiers = roslynInterface.Modifiers,
                Language = ProgrammingLanguage.CSharp,
                NativeObject = roslynInterface,
                IsNested = roslynInterface.IsNested,
                ParentTypeName = roslynInterface.ParentTypeName,
                FullTypeName = roslynInterface.FullTypeName
            };

            typeInfo.Interfaces.AddRange(roslynInterface.BaseInterfaces);

            foreach (var method in roslynInterface.Methods)
            {
                typeInfo.Methods.Add(ConvertFromRoslynMethod(method));
            }

            foreach (var property in roslynInterface.Properties)
            {
                typeInfo.Properties.Add(ConvertFromRoslynProperty(property));
            }

            if (roslynInterface.SyntaxNode != null)
            {
                typeInfo.DocumentationComment = CodeMetricsAnalyzer.ExtractDocumentationComment(roslynInterface.SyntaxNode);
                typeInfo.LineCount = CodeMetricsAnalyzer.CalculateLineCount(roslynInterface.SyntaxNode);
                typeInfo.IsPublic = CodeMetricsAnalyzer.IsPublicType(roslynInterface.SyntaxNode);
            }

            return typeInfo;
        }

        private TypeInfo ConvertFromRoslynStruct(RoslynStructInfo roslynStruct)
        {
            var typeInfo = new TypeInfo
            {
                Name = roslynStruct.Name,
                Namespace = roslynStruct.Namespace,
                Type = CodeElementType.Struct,
                Location = roslynStruct.Location,
                Modifiers = roslynStruct.Modifiers,
                Language = ProgrammingLanguage.CSharp,
                NativeObject = roslynStruct,
                IsNested = roslynStruct.IsNested,
                ParentTypeName = roslynStruct.ParentTypeName,
                FullTypeName = roslynStruct.FullTypeName
            };

            foreach (var method in roslynStruct.Methods)
            {
                typeInfo.Methods.Add(ConvertFromRoslynMethod(method));
            }

            foreach (var property in roslynStruct.Properties)
            {
                typeInfo.Properties.Add(ConvertFromRoslynProperty(property));
            }

            foreach (var field in roslynStruct.Fields)
            {
                typeInfo.Fields.Add(ConvertFromRoslynField(field));
            }

            foreach (var constructor in roslynStruct.Constructors)
            {
                typeInfo.Constructors.Add(ConvertFromRoslynConstructor(constructor));
            }

            if (roslynStruct.SyntaxNode != null)
            {
                typeInfo.DocumentationComment = CodeMetricsAnalyzer.ExtractDocumentationComment(roslynStruct.SyntaxNode);
                typeInfo.LineCount = CodeMetricsAnalyzer.CalculateLineCount(roslynStruct.SyntaxNode);
                typeInfo.IsPublic = CodeMetricsAnalyzer.IsPublicType(roslynStruct.SyntaxNode);
            }

            return typeInfo;
        }

        private EnumInfo ConvertFromRoslynEnum(RoslynEnumInfo roslynEnum)
        {
            var enumInfo = new EnumInfo
            {
                Name = roslynEnum.Name,
                Namespace = roslynEnum.Namespace,
                Modifiers = roslynEnum.Modifiers,
                Location = roslynEnum.Location,
                NativeObject = roslynEnum,
                IsNested = roslynEnum.IsNested,
                ParentTypeName = roslynEnum.ParentTypeName,
                FullTypeName = roslynEnum.FullTypeName
            };

            foreach (var member in roslynEnum.Members)
            {
                enumInfo.Members.Add(new EnumMemberInfo
                {
                    Name = member.Name,
                    Value = member.Value
                });
            }

            if (roslynEnum.SyntaxNode != null)
            {
                enumInfo.DocumentationComment = CodeMetricsAnalyzer.ExtractDocumentationComment(roslynEnum.SyntaxNode);
                enumInfo.IsPublic = CodeMetricsAnalyzer.IsPublicType(roslynEnum.SyntaxNode);
            }

            return enumInfo;
        }

        private ProgrammingLanguage DetectLanguage(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();

            return ext switch
            {
                ".cs" => ProgrammingLanguage.CSharp,
                ".js" => ProgrammingLanguage.JavaScript,
                ".c" => ProgrammingLanguage.C,
                ".h" => ProgrammingLanguage.C,
                ".cpp" => ProgrammingLanguage.Cpp,
                ".cc" => ProgrammingLanguage.Cpp,
                ".cxx" => ProgrammingLanguage.Cpp,
                ".hpp" => ProgrammingLanguage.Cpp,
                ".hh" => ProgrammingLanguage.Cpp,
                ".hxx" => ProgrammingLanguage.Cpp,
                _ => throw new NotSupportedException($"File extension {ext} is not supported")
            };
        }

        // ========================================
        // LLM-Friendly Formatting APIs
        // ========================================

        // A. Overview APIs (File Structure)
        public string FormatFileStructure(string filePath, ProgrammingLanguage language)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            return FormatStructureOverview(parsed);
        }

        public string FormatCodeStructure(string code, ProgrammingLanguage language, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            return FormatStructureOverview(parsed);
        }

        // B1. Function Search APIs
        public string FindFunctions(string filePath, ProgrammingLanguage language, string namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var functions = FindFunctions(parsed, namePattern);
            return FormatFunctionList(functions, filePath);
        }

        public string FindFunctionsInCode(string code, ProgrammingLanguage language, string namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var functions = FindFunctions(parsed, namePattern);
            return FormatFunctionList(functions, fileName);
        }

        public string FindFunction(string filePath, ProgrammingLanguage language, string functionName)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var function = FindFunction(parsed, functionName);
            return function != null ? FormatFunctionDetail(function, filePath) : $"Function '{functionName}' not found";
        }

        public string FindFunctionInCode(string code, ProgrammingLanguage language, string functionName, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var function = FindFunction(parsed, functionName);
            return function != null ? FormatFunctionDetail(function, fileName) : $"Function '{functionName}' not found";
        }

        // B2. Type Search APIs
        public string FindTypes(string filePath, ProgrammingLanguage language, string namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var types = FindTypes(parsed, namePattern);
            return FormatTypeList(types, filePath);
        }

        public string FindTypesInCode(string code, ProgrammingLanguage language, string namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var types = FindTypes(parsed, namePattern);
            return FormatTypeList(types, fileName);
        }

        public string FindType(string filePath, ProgrammingLanguage language, string typeName)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var type = FindType(parsed, typeName);
            return type != null ? FormatTypeDetail(type, filePath) : $"Type '{typeName}' not found";
        }

        public string FindTypeInCode(string code, ProgrammingLanguage language, string typeName, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var type = FindType(parsed, typeName);
            return type != null ? FormatTypeDetail(type, fileName) : $"Type '{typeName}' not found";
        }

        // B3. Variable Search APIs
        public string FindVariables(string filePath, ProgrammingLanguage language, string namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            return FormatVariables(parsed, namePattern, filePath);
        }

        public string FindVariablesInCode(string code, ProgrammingLanguage language, string namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            return FormatVariables(parsed, namePattern, fileName);
        }

        public string FindVariable(string filePath, ProgrammingLanguage language, string variableName)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            return FormatVariable(parsed, variableName, filePath);
        }

        public string FindVariableInCode(string code, ProgrammingLanguage language, string variableName, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            return FormatVariable(parsed, variableName, fileName);
        }

        public string FindGlobalVariables(string filePath, ProgrammingLanguage language, string namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            return FormatGlobalVariables(parsed, namePattern, filePath);
        }

        public string FindGlobalVariablesInCode(string code, ProgrammingLanguage language, string namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            return FormatGlobalVariables(parsed, namePattern, fileName);
        }

        public string FindLocalVariables(string filePath, ProgrammingLanguage language, string namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            return FormatLocalVariables(parsed, namePattern, filePath);
        }

        public string FindLocalVariablesInCode(string code, ProgrammingLanguage language, string namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            return FormatLocalVariables(parsed, namePattern, fileName);
        }

        public string FindParameters(string filePath, ProgrammingLanguage language, string namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            return FormatParameters(parsed, namePattern, filePath);
        }

        public string FindParametersInCode(string code, ProgrammingLanguage language, string namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            return FormatParameters(parsed, namePattern, fileName);
        }

        // B4. Class Member Search APIs
        public string FindClassMembers(string filePath, ProgrammingLanguage language, string className)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);
            if (matchingTypes.Count == 0)
                return $"No classes matching pattern '{className}' found";

            var sb = new StringBuilder();
            foreach (var type in matchingTypes)
            {
                if (matchingTypes.Count > 1)
                    sb.AppendLine($"--- Class: {type.Name} ---").AppendLine();
                sb.Append(FormatClassMembers(type, filePath));
                if (matchingTypes.Count > 1)
                    sb.AppendLine();
            }
            return sb.ToString();
        }

        public string FindClassMembersInCode(string code, ProgrammingLanguage language, string className, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);
            if (matchingTypes.Count == 0)
                return $"No classes matching pattern '{className}' found";

            var sb = new StringBuilder();
            foreach (var type in matchingTypes)
            {
                if (matchingTypes.Count > 1)
                    sb.AppendLine($"--- Class: {type.Name} ---").AppendLine();
                sb.Append(FormatClassMembers(type, fileName));
                if (matchingTypes.Count > 1)
                    sb.AppendLine();
            }
            return sb.ToString();
        }

        public string FindFields(string filePath, ProgrammingLanguage language, string className, string namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);
            if (matchingTypes.Count == 0)
                return $"No classes matching pattern '{className}' found";

            var sb = new StringBuilder();
            int totalCount = 0;
            foreach (var type in matchingTypes)
            {
                var fields = string.IsNullOrEmpty(namePattern)
                    ? type.Fields
                    : type.Fields.Where(f => MatchesPattern(f.Name, namePattern)).ToList();

                if (fields.Count > 0)
                {
                    if (totalCount > 0)
                        sb.AppendLine();
                    if (matchingTypes.Count > 1)
                        sb.AppendLine($"--- Class: {type.Name} ---").AppendLine();
                    sb.Append(FormatFields(type, namePattern, filePath));
                    totalCount += fields.Count;
                }
            }

            return totalCount > 0 ? sb.ToString() : $"No fields found matching pattern '{namePattern ?? "*"}' in classes matching '{className}'";
        }

        public string FindFieldsInCode(string code, ProgrammingLanguage language, string className, string namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);
            if (matchingTypes.Count == 0)
                return $"No classes matching pattern '{className}' found";

            var sb = new StringBuilder();
            int totalCount = 0;
            foreach (var type in matchingTypes)
            {
                var fields = string.IsNullOrEmpty(namePattern)
                    ? type.Fields
                    : type.Fields.Where(f => MatchesPattern(f.Name, namePattern)).ToList();

                if (fields.Count > 0)
                {
                    if (totalCount > 0)
                        sb.AppendLine();
                    if (matchingTypes.Count > 1)
                        sb.AppendLine($"--- Class: {type.Name} ---").AppendLine();
                    sb.Append(FormatFields(type, namePattern, fileName));
                    totalCount += fields.Count;
                }
            }

            return totalCount > 0 ? sb.ToString() : $"No fields found matching pattern '{namePattern ?? "*"}' in classes matching '{className}'";
        }

        public string FindField(string filePath, ProgrammingLanguage language, string className, string fieldName)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);

            foreach (var type in matchingTypes)
            {
                var field = type.Fields.FirstOrDefault(f => MatchesPattern(f.Name, fieldName));
                if (field != null)
                    return FormatFieldDetail(field, type.Name, filePath);
            }

            return $"No field matching pattern '{fieldName}' found in classes matching '{className}'";
        }

        public string FindFieldInCode(string code, ProgrammingLanguage language, string className, string fieldName, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);

            foreach (var type in matchingTypes)
            {
                var field = type.Fields.FirstOrDefault(f => MatchesPattern(f.Name, fieldName));
                if (field != null)
                    return FormatFieldDetail(field, type.Name, fileName);
            }

            return $"No field matching pattern '{fieldName}' found in classes matching '{className}'";
        }

        public string FindProperties(string filePath, ProgrammingLanguage language, string className, string namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);
            if (matchingTypes.Count == 0)
                return $"No classes matching pattern '{className}' found";

            var sb = new StringBuilder();
            int totalCount = 0;
            foreach (var type in matchingTypes)
            {
                var properties = string.IsNullOrEmpty(namePattern)
                    ? type.Properties
                    : type.Properties.Where(p => MatchesPattern(p.Name, namePattern)).ToList();

                if (properties.Count > 0)
                {
                    if (totalCount > 0)
                        sb.AppendLine();
                    if (matchingTypes.Count > 1)
                        sb.AppendLine($"--- Class: {type.Name} ---").AppendLine();
                    sb.Append(FormatProperties(type, namePattern, filePath));
                    totalCount += properties.Count;
                }
            }

            return totalCount > 0 ? sb.ToString() : $"No properties found matching pattern '{namePattern ?? "*"}' in classes matching '{className}'";
        }

        public string FindPropertiesInCode(string code, ProgrammingLanguage language, string className, string namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);
            if (matchingTypes.Count == 0)
                return $"No classes matching pattern '{className}' found";

            var sb = new StringBuilder();
            int totalCount = 0;
            foreach (var type in matchingTypes)
            {
                var properties = string.IsNullOrEmpty(namePattern)
                    ? type.Properties
                    : type.Properties.Where(p => MatchesPattern(p.Name, namePattern)).ToList();

                if (properties.Count > 0)
                {
                    if (totalCount > 0)
                        sb.AppendLine();
                    if (matchingTypes.Count > 1)
                        sb.AppendLine($"--- Class: {type.Name} ---").AppendLine();
                    sb.Append(FormatProperties(type, namePattern, fileName));
                    totalCount += properties.Count;
                }
            }

            return totalCount > 0 ? sb.ToString() : $"No properties found matching pattern '{namePattern ?? "*"}' in classes matching '{className}'";
        }

        public string FindProperty(string filePath, ProgrammingLanguage language, string className, string propertyName)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);

            foreach (var type in matchingTypes)
            {
                var property = type.Properties.FirstOrDefault(p => MatchesPattern(p.Name, propertyName));
                if (property != null)
                    return FormatPropertyDetail(property, type.Name, filePath);
            }

            return $"No property matching pattern '{propertyName}' found in classes matching '{className}'";
        }

        public string FindPropertyInCode(string code, ProgrammingLanguage language, string className, string propertyName, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);

            foreach (var type in matchingTypes)
            {
                var property = type.Properties.FirstOrDefault(p => MatchesPattern(p.Name, propertyName));
                if (property != null)
                    return FormatPropertyDetail(property, type.Name, fileName);
            }

            return $"No property matching pattern '{propertyName}' found in classes matching '{className}'";
        }

        public string FindMethods(string filePath, ProgrammingLanguage language, string className, string namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);
            if (matchingTypes.Count == 0)
                return $"No classes matching pattern '{className}' found";

            var sb = new StringBuilder();
            int totalCount = 0;
            foreach (var type in matchingTypes)
            {
                var methods = string.IsNullOrEmpty(namePattern)
                    ? type.Methods
                    : type.Methods.Where(m => MatchesPattern(m.Name, namePattern)).ToList();

                if (methods.Count > 0)
                {
                    if (totalCount > 0)
                        sb.AppendLine();
                    if (matchingTypes.Count > 1)
                        sb.AppendLine($"--- Class: {type.Name} ---").AppendLine();
                    sb.Append(FormatMethods(type, namePattern, filePath));
                    totalCount += methods.Count;
                }
            }

            return totalCount > 0 ? sb.ToString() : $"No methods found matching pattern '{namePattern ?? "*"}' in classes matching '{className}'";
        }

        public string FindMethodsInCode(string code, ProgrammingLanguage language, string className, string namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);
            if (matchingTypes.Count == 0)
                return $"No classes matching pattern '{className}' found";

            var sb = new StringBuilder();
            int totalCount = 0;
            foreach (var type in matchingTypes)
            {
                var methods = string.IsNullOrEmpty(namePattern)
                    ? type.Methods
                    : type.Methods.Where(m => MatchesPattern(m.Name, namePattern)).ToList();

                if (methods.Count > 0)
                {
                    if (totalCount > 0)
                        sb.AppendLine();
                    if (matchingTypes.Count > 1)
                        sb.AppendLine($"--- Class: {type.Name} ---").AppendLine();
                    sb.Append(FormatMethods(type, namePattern, fileName));
                    totalCount += methods.Count;
                }
            }

            return totalCount > 0 ? sb.ToString() : $"No methods found matching pattern '{namePattern ?? "*"}' in classes matching '{className}'";
        }

        public string FindMethod(string filePath, ProgrammingLanguage language, string className, string methodName)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);

            foreach (var type in matchingTypes)
            {
                var method = type.Methods.FirstOrDefault(m => MatchesPattern(m.Name, methodName));
                if (method != null)
                    return FormatMethodDetail(method, type.Name, filePath);
            }

            return $"No method matching pattern '{methodName}' found in classes matching '{className}'";
        }

        public string FindMethodInCode(string code, ProgrammingLanguage language, string className, string methodName, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);

            foreach (var type in matchingTypes)
            {
                var method = type.Methods.FirstOrDefault(m => MatchesPattern(m.Name, methodName));
                if (method != null)
                    return FormatMethodDetail(method, type.Name, fileName);
            }

            return $"No method matching pattern '{methodName}' found in classes matching '{className}'";
        }

        // E. Event Search APIs
        public string FindEvents(string filePath, ProgrammingLanguage language, string className, string namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);
            if (matchingTypes.Count == 0)
                return $"No classes matching pattern '{className}' found";

            var sb = new StringBuilder();
            int totalCount = 0;
            foreach (var type in matchingTypes)
            {
                var events = string.IsNullOrEmpty(namePattern)
                    ? type.Events
                    : type.Events.Where(e => MatchesPattern(e.Name, namePattern)).ToList();

                if (events.Count > 0)
                {
                    if (totalCount > 0)
                        sb.AppendLine();
                    if (matchingTypes.Count > 1)
                        sb.AppendLine($"--- Class: {type.Name} ---").AppendLine();
                    sb.Append(FormatEvents(type, namePattern, filePath));
                    totalCount += events.Count;
                }
            }

            return totalCount > 0 ? sb.ToString() : $"No events found matching pattern '{namePattern ?? "*"}' in classes matching '{className}'";
        }

        public string FindEventsInCode(string code, ProgrammingLanguage language, string className, string namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);
            if (matchingTypes.Count == 0)
                return $"No classes matching pattern '{className}' found";

            var sb = new StringBuilder();
            int totalCount = 0;
            foreach (var type in matchingTypes)
            {
                var events = string.IsNullOrEmpty(namePattern)
                    ? type.Events
                    : type.Events.Where(e => MatchesPattern(e.Name, namePattern)).ToList();

                if (events.Count > 0)
                {
                    if (totalCount > 0)
                        sb.AppendLine();
                    if (matchingTypes.Count > 1)
                        sb.AppendLine($"--- Class: {type.Name} ---").AppendLine();
                    sb.Append(FormatEvents(type, namePattern, fileName));
                    totalCount += events.Count;
                }
            }

            return totalCount > 0 ? sb.ToString() : $"No events found matching pattern '{namePattern ?? "*"}' in classes matching '{className}'";
        }

        public string FindEvent(string filePath, ProgrammingLanguage language, string className, string eventName)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);

            foreach (var type in matchingTypes)
            {
                var evt = type.Events.FirstOrDefault(e => MatchesPattern(e.Name, eventName));
                if (evt != null)
                    return FormatEventDetail(evt, type.Name, filePath);
            }

            return $"No event matching pattern '{eventName}' found in classes matching '{className}'";
        }

        public string FindEventInCode(string code, ProgrammingLanguage language, string className, string eventName, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);

            foreach (var type in matchingTypes)
            {
                var evt = type.Events.FirstOrDefault(e => MatchesPattern(e.Name, eventName));
                if (evt != null)
                    return FormatEventDetail(evt, type.Name, fileName);
            }

            return $"No event matching pattern '{eventName}' found in classes matching '{className}'";
        }

        // Constructor Search APIs
        public string FindConstructors(string filePath, ProgrammingLanguage language, string className)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);
            if (matchingTypes.Count == 0)
                return $"No classes matching pattern '{className}' found";

            var sb = new StringBuilder();
            int totalCount = 0;
            foreach (var type in matchingTypes)
            {
                if (type.Constructors.Count > 0)
                {
                    if (totalCount > 0)
                        sb.AppendLine();
                    if (matchingTypes.Count > 1)
                        sb.AppendLine($"--- Class: {type.Name} ---").AppendLine();
                    sb.Append(FormatConstructors(type, filePath));
                    totalCount += type.Constructors.Count;
                }
            }

            return totalCount > 0 ? sb.ToString() : $"No constructors found in classes matching '{className}'";
        }

        public string FindConstructorsInCode(string code, ProgrammingLanguage language, string className, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var matchingTypes = FindAllMatchingTypes(parsed, className);
            if (matchingTypes.Count == 0)
                return $"No classes matching pattern '{className}' found";

            var sb = new StringBuilder();
            int totalCount = 0;
            foreach (var type in matchingTypes)
            {
                if (type.Constructors.Count > 0)
                {
                    if (totalCount > 0)
                        sb.AppendLine();
                    if (matchingTypes.Count > 1)
                        sb.AppendLine($"--- Class: {type.Name} ---").AppendLine();
                    sb.Append(FormatConstructors(type, fileName));
                    totalCount += type.Constructors.Count;
                }
            }

            return totalCount > 0 ? sb.ToString() : $"No constructors found in classes matching '{className}'";
        }

        // F. Interface Search APIs
        public string FindInterfaces(string filePath, ProgrammingLanguage language, string namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var interfaces = string.IsNullOrEmpty(namePattern)
                ? parsed.Interfaces
                : parsed.Interfaces.Where(i => MatchesPattern(i.Name, namePattern)).ToList();
            return FormatInterfaceList(interfaces, filePath);
        }

        public string FindInterfacesInCode(string code, ProgrammingLanguage language, string namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var interfaces = string.IsNullOrEmpty(namePattern)
                ? parsed.Interfaces
                : parsed.Interfaces.Where(i => MatchesPattern(i.Name, namePattern)).ToList();
            return FormatInterfaceList(interfaces, fileName);
        }

        public string FindInterface(string filePath, ProgrammingLanguage language, string interfaceName)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var iface = parsed.Interfaces.FirstOrDefault(i => MatchesPattern(i.Name, interfaceName));
            return iface != null ? FormatInterfaceDetail(iface, filePath) : $"Interface matching pattern '{interfaceName}' not found";
        }

        public string FindInterfaceInCode(string code, ProgrammingLanguage language, string interfaceName, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var iface = parsed.Interfaces.FirstOrDefault(i => MatchesPattern(i.Name, interfaceName));
            return iface != null ? FormatInterfaceDetail(iface, fileName) : $"Interface matching pattern '{interfaceName}' not found";
        }

        // G. Struct Search APIs
        public string FindStructs(string filePath, ProgrammingLanguage language, string namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var structs = string.IsNullOrEmpty(namePattern)
                ? parsed.Structs
                : parsed.Structs.Where(s => MatchesPattern(s.Name, namePattern)).ToList();
            return FormatStructList(structs, filePath);
        }

        public string FindStructsInCode(string code, ProgrammingLanguage language, string namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var structs = string.IsNullOrEmpty(namePattern)
                ? parsed.Structs
                : parsed.Structs.Where(s => MatchesPattern(s.Name, namePattern)).ToList();
            return FormatStructList(structs, fileName);
        }

        public string FindStruct(string filePath, ProgrammingLanguage language, string structName)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var structType = parsed.Structs.FirstOrDefault(s => MatchesPattern(s.Name, structName));
            return structType != null ? FormatStructDetail(structType, filePath) : $"Struct matching pattern '{structName}' not found";
        }

        public string FindStructInCode(string code, ProgrammingLanguage language, string structName, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var structType = parsed.Structs.FirstOrDefault(s => MatchesPattern(s.Name, structName));
            return structType != null ? FormatStructDetail(structType, fileName) : $"Struct matching pattern '{structName}' not found";
        }

        // H. Enum Search APIs
        public string FindEnums(string filePath, ProgrammingLanguage language, string namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var enums = string.IsNullOrEmpty(namePattern)
                ? parsed.Enums
                : parsed.Enums.Where(e => MatchesPattern(e.Name, namePattern)).ToList();
            return FormatEnumList(enums, filePath);
        }

        public string FindEnumsInCode(string code, ProgrammingLanguage language, string namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var enums = string.IsNullOrEmpty(namePattern)
                ? parsed.Enums
                : parsed.Enums.Where(e => MatchesPattern(e.Name, namePattern)).ToList();
            return FormatEnumList(enums, fileName);
        }

        public string FindEnum(string filePath, ProgrammingLanguage language, string enumName)
        {
            var parsed = ParseFile(filePath, language);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var enumType = parsed.Enums.FirstOrDefault(e => MatchesPattern(e.Name, enumName));
            return enumType != null ? FormatEnumDetail(enumType, filePath) : $"Enum matching pattern '{enumName}' not found";
        }

        public string FindEnumInCode(string code, ProgrammingLanguage language, string enumName, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            var errorMsg = CheckParsedFileSyntaxErrors(parsed);
            if (errorMsg != null) return errorMsg;

            var enumType = parsed.Enums.FirstOrDefault(e => MatchesPattern(e.Name, enumName));
            return enumType != null ? FormatEnumDetail(enumType, fileName) : $"Enum matching pattern '{enumName}' not found";
        }

        // ========================================
        // Formatting Helper Methods
        // ========================================

        private string FormatStructureOverview(ParsedCodeFile parsed)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"File: {parsed.FilePath} ({parsed.Language})");
            sb.AppendLine($"Total Lines: {parsed.TotalLines}");
            sb.AppendLine();

            if (parsed.Types.Count > 0)
            {
                sb.AppendLine($"Classes: {parsed.Types.Count}");
                foreach (var type in parsed.Types)
                {
                    sb.AppendLine($"  - {type.Name} {FormatLocation(type.Location)}");
                    if (type.IsNested)
                    {
                        sb.AppendLine($"    Nested: Yes");
                        sb.AppendLine($"    Parent Type: {type.ParentTypeName}");
                        sb.AppendLine($"    Full Type Name: {type.FullTypeName}");
                    }

                    // Show fields in the class
                    if (type.Fields.Count > 0)
                    {
                        sb.AppendLine($"    Fields:");
                        foreach (var field in type.Fields)
                        {
                            sb.AppendLine($"      {FormatLocation(field.Location)} {field.Name}: {field.Type}{(string.IsNullOrEmpty(field.Modifiers) ? "" : $" ({field.Modifiers})")}");
                        }
                    }

                    // Show properties in the class
                    if (type.Properties.Count > 0)
                    {
                        sb.AppendLine($"    Properties:");
                        foreach (var prop in type.Properties)
                        {
                            var accessors = new List<string>();
                            if (prop.HasGetter) accessors.Add("get");
                            if (prop.HasSetter) accessors.Add("set");
                            var accessorStr = accessors.Count > 0 ? $" {{{string.Join("; ", accessors)}}}" : "";
                            sb.AppendLine($"      {FormatLocation(prop.Location)} {prop.Name}: {prop.Type}{accessorStr}{(string.IsNullOrEmpty(prop.Modifiers) ? "" : $" ({prop.Modifiers})")}");
                        }
                    }

                    // Show events in the class
                    if (type.Events.Count > 0)
                    {
                        sb.AppendLine($"    Events:");
                        foreach (var evt in type.Events)
                        {
                            sb.AppendLine($"      {FormatLocation(evt.Location)} {evt.Name}: {evt.Type}{(string.IsNullOrEmpty(evt.Modifiers) ? "" : $" ({evt.Modifiers})")}");
                        }
                    }

                    // Show constructors in the class
                    if (type.Constructors.Count > 0)
                    {
                        sb.AppendLine($"    Constructors:");
                        foreach (var ctor in type.Constructors)
                        {
                            sb.AppendLine($"      {FormatLocation(ctor.Location)} {FormatFunctionSignature(ctor)}{(string.IsNullOrEmpty(ctor.Modifiers) ? "" : $" ({ctor.Modifiers})")}");
                        }
                    }

                    // Show methods in the class
                    if (type.Methods.Count > 0)
                    {
                        sb.AppendLine($"    Methods:");
                        foreach (var method in type.Methods)
                        {
                            sb.AppendLine($"      {FormatLocation(method.Location)} {FormatFunctionSignature(method)}{(string.IsNullOrEmpty(method.Modifiers) ? "" : $" ({method.Modifiers})")}");
                        }
                    }
                }
                sb.AppendLine();
            }

            if (parsed.Functions.Count > 0)
            {
                // Collect all method locations from classes to avoid duplicates
                var methodLocations = new HashSet<string>();
                foreach (var type in parsed.Types)
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.Location != null)
                        {
                            methodLocations.Add($"{method.Location.StartLine}:{method.Location.StartColumn}");
                        }
                    }
                    foreach (var ctor in type.Constructors)
                    {
                        if (ctor.Location != null)
                        {
                            methodLocations.Add($"{ctor.Location.StartLine}:{ctor.Location.StartColumn}");
                        }
                    }
                }

                // Filter out functions that are class methods
                var standaloneFunctions = parsed.Functions.Where(func =>
                {
                    if (func.Location == null) return true;
                    var locationKey = $"{func.Location.StartLine}:{func.Location.StartColumn}";
                    return !methodLocations.Contains(locationKey);
                }).ToList();

                if (standaloneFunctions.Count > 0)
                {
                    sb.AppendLine($"Functions: {standaloneFunctions.Count}");
                    foreach (var func in standaloneFunctions)
                    {
                        sb.AppendLine($"  - {func.Name} {FormatLocation(func.Location)}");
                    }
                    sb.AppendLine();
                }
            }

            if (parsed.Interfaces.Count > 0)
            {
                sb.AppendLine($"Interfaces: {parsed.Interfaces.Count}");
                foreach (var iface in parsed.Interfaces)
                {
                    sb.AppendLine($"  - {iface.Name} {FormatLocation(iface.Location)}");
                    if (iface.Methods.Count > 0)
                    {
                        sb.AppendLine($"    Methods:");
                        foreach (var method in iface.Methods)
                        {
                            sb.AppendLine($"      {FormatLocation(method.Location)} {FormatFunctionSignature(method)}");
                        }
                    }
                }
                sb.AppendLine();
            }

            if (parsed.Structs.Count > 0)
            {
                sb.AppendLine($"Structs: {parsed.Structs.Count}");
                foreach (var structType in parsed.Structs)
                {
                    sb.AppendLine($"  - {structType.Name} {FormatLocation(structType.Location)}");
                    if (structType.Fields.Count > 0)
                    {
                        sb.AppendLine($"    Fields:");
                        foreach (var field in structType.Fields)
                        {
                            sb.AppendLine($"      {FormatLocation(field.Location)} {field.Name}: {field.Type}");
                        }
                    }
                }
                sb.AppendLine();
            }

            if (parsed.Enums.Count > 0)
            {
                sb.AppendLine($"Enums: {parsed.Enums.Count}");
                foreach (var enumType in parsed.Enums)
                {
                    sb.AppendLine($"  - {enumType.Name} {FormatLocation(enumType.Location)}");
                    if (enumType.Members.Count > 0)
                    {
                        sb.AppendLine($"    Members: {string.Join(", ", enumType.Members.Select(m => m.Name))}");
                    }
                }
                sb.AppendLine();
            }

            if (parsed.Imports.Count > 0)
            {
                sb.AppendLine($"Imports: {parsed.Imports.Count}");
                foreach (var import in parsed.Imports)
                {
                    sb.AppendLine($"  - {import}");
                }
                sb.AppendLine();
            }

            if (parsed.Dependencies.Count > 0)
            {
                sb.AppendLine($"Dependencies: {parsed.Dependencies.Count}");
                foreach (var dep in parsed.Dependencies)
                {
                    sb.AppendLine($"  - {dep.Name} ({dep.Type})");
                }
            }

            return sb.ToString();
        }

        private string FormatFunctionList(List<FunctionInfo> functions, string filePath)
        {
            if (functions.Count == 0)
                return "No functions found";

            var sb = new StringBuilder();
            sb.AppendLine($"=== Functions Found: {functions.Count} ===");
            sb.AppendLine();

            foreach (var func in functions)
            {
                sb.AppendLine($"Function: {func.Name}");
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {FormatLocation(func.Location)}");
                sb.AppendLine($"Signature: {FormatFunctionSignature(func)}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string FormatFunctionDetail(FunctionInfo func, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Function: {func.Name}");
            sb.AppendLine($"File: {filePath}");
            sb.AppendLine($"Location: {FormatLocation(func.Location)}");
            sb.AppendLine($"Signature: {FormatFunctionSignature(func)}");

            if (func.Parameters.Count > 0)
            {
                sb.AppendLine("Parameters:");
                foreach (var param in func.Parameters)
                {
                    sb.AppendLine($"  - {param.Type} {param.Name}");
                }
            }

            sb.AppendLine($"Return Type: {func.ReturnType}");
            if (!string.IsNullOrEmpty(func.Modifiers))
                sb.AppendLine($"Modifiers: {func.Modifiers}");

            return sb.ToString();
        }

        private string FormatTypeList(List<TypeInfo> types, string filePath)
        {
            if (types.Count == 0)
                return "No types found";

            var sb = new StringBuilder();
            sb.AppendLine($"=== Types Found: {types.Count} ===");
            sb.AppendLine();

            foreach (var type in types)
            {
                sb.AppendLine($"{type.Type}: {type.Name}");
                if (type.IsNested)
                {
                    sb.AppendLine($"  Nested: Yes");
                    sb.AppendLine($"  Parent Type: {type.ParentTypeName}");
                    sb.AppendLine($"  Full Type Name: {type.FullTypeName}");
                }
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {FormatLocation(type.Location)}");
                if (type.BaseTypes.Count > 0)
                    sb.AppendLine($"Base Types: {string.Join(", ", type.BaseTypes)}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string FormatTypeDetail(TypeInfo type, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{type.Type}: {type.Name}");
            if (type.IsNested)
            {
                sb.AppendLine($"Nested: Yes");
                sb.AppendLine($"Parent Type: {type.ParentTypeName}");
                sb.AppendLine($"Full Type Name: {type.FullTypeName}");
            }
            sb.AppendLine($"File: {filePath}");
            sb.AppendLine($"Location: {FormatLocation(type.Location)}");

            if (type.BaseTypes.Count > 0)
                sb.AppendLine($"Base Types: {string.Join(", ", type.BaseTypes)}");

            if (type.Methods.Count > 0)
            {
                sb.AppendLine($"Methods: {type.Methods.Count}");
                foreach (var method in type.Methods)
                {
                    sb.AppendLine($"  - {FormatFunctionSignature(method)} {FormatLocation(method.Location)}");
                }
            }

            if (type.Properties.Count > 0)
            {
                sb.AppendLine($"Properties: {type.Properties.Count}");
                foreach (var prop in type.Properties)
                {
                    sb.AppendLine($"  - {prop.Name}: {prop.Type} {FormatLocation(prop.Location)}");
                }
            }

            if (type.Fields.Count > 0)
            {
                sb.AppendLine($"Fields: {type.Fields.Count}");
                foreach (var field in type.Fields)
                {
                    sb.AppendLine($"  - {field.Name}: {field.Type} {FormatLocation(field.Location)}");
                }
            }

            return sb.ToString();
        }

        private string FormatVariables(ParsedCodeFile parsed, string namePattern, string filePath)
        {
            var sb = new StringBuilder();
            var count = 0;

            // Collect all variables from types
            foreach (var type in parsed.Types)
            {
                foreach (var field in type.Fields)
                {
                    if (string.IsNullOrEmpty(namePattern) || MatchesPattern(field.Name, namePattern))
                    {
                        if (count == 0) sb.AppendLine("=== Variables Found ===").AppendLine();
                        sb.AppendLine($"Variable: {field.Name}");
                        sb.AppendLine($"File: {filePath}");
                        sb.AppendLine($"Location: {FormatLocation(field.Location)}");
                        sb.AppendLine($"Type: {field.Type}");
                        sb.AppendLine($"Scope: Field (in {type.Type.ToString().ToLower()} {type.Name})");
                        if (!string.IsNullOrEmpty(field.Modifiers))
                            sb.AppendLine($"Modifiers: {field.Modifiers}");
                        sb.AppendLine();
                        count++;
                    }
                }
            }

            return count > 0 ? sb.ToString() : "No variables found";
        }

        private string FormatVariable(ParsedCodeFile parsed, string variableName, string filePath)
        {
            // Search in type fields
            foreach (var type in parsed.Types)
            {
                var field = type.Fields.FirstOrDefault(f => f.Name == variableName);
                if (field != null)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Variable: {field.Name}");
                    sb.AppendLine($"File: {filePath}");
                    sb.AppendLine($"Location: {FormatLocation(field.Location)}");
                    sb.AppendLine($"Type: {field.Type}");
                    sb.AppendLine($"Scope: Field (in {type.Type.ToString().ToLower()} {type.Name})");
                    if (!string.IsNullOrEmpty(field.Modifiers))
                        sb.AppendLine($"Modifiers: {field.Modifiers}");
                    return sb.ToString();
                }
            }

            return $"Variable '{variableName}' not found";
        }

        private string FormatGlobalVariables(ParsedCodeFile parsed, string namePattern, string filePath)
        {
            var sb = new StringBuilder();
            var count = 0;

            foreach (var type in parsed.Types)
            {
                foreach (var field in type.Fields)
                {
                    if (field.IsStatic && (string.IsNullOrEmpty(namePattern) || MatchesPattern(field.Name, namePattern)))
                    {
                        if (count == 0) sb.AppendLine("=== Global Variables Found ===").AppendLine();
                        sb.AppendLine($"Variable: {field.Name}");
                        sb.AppendLine($"File: {filePath}");
                        sb.AppendLine($"Location: {FormatLocation(field.Location)}");
                        sb.AppendLine($"Type: {field.Type}");
                        sb.AppendLine($"Scope: Global (static field in {type.Name})");
                        if (!string.IsNullOrEmpty(field.Modifiers))
                            sb.AppendLine($"Modifiers: {field.Modifiers}");
                        sb.AppendLine();
                        count++;
                    }
                }
            }

            return count > 0 ? sb.ToString() : "No global variables found";
        }

        private string FormatLocalVariables(ParsedCodeFile parsed, string namePattern, string filePath)
        {
            return "Local variable extraction not yet implemented for this language";
        }

        private string FormatParameters(ParsedCodeFile parsed, string namePattern, string filePath)
        {
            var sb = new StringBuilder();
            var count = 0;

            foreach (var func in parsed.Functions)
            {
                foreach (var param in func.Parameters)
                {
                    if (string.IsNullOrEmpty(namePattern) || MatchesPattern(param.Name, namePattern))
                    {
                        if (count == 0) sb.AppendLine("=== Parameters Found ===").AppendLine();
                        sb.AppendLine($"Parameter: {param.Name}");
                        sb.AppendLine($"File: {filePath}");
                        sb.AppendLine($"Type: {param.Type}");
                        sb.AppendLine($"Function: {func.Name}");
                        sb.AppendLine();
                        count++;
                    }
                }
            }

            foreach (var type in parsed.Types)
            {
                foreach (var method in type.Methods)
                {
                    foreach (var param in method.Parameters)
                    {
                        if (string.IsNullOrEmpty(namePattern) || MatchesPattern(param.Name, namePattern))
                        {
                            if (count == 0) sb.AppendLine("=== Parameters Found ===").AppendLine();
                            sb.AppendLine($"Parameter: {param.Name}");
                            sb.AppendLine($"File: {filePath}");
                            sb.AppendLine($"Type: {param.Type}");
                            sb.AppendLine($"Method: {type.Name}.{method.Name}");
                            sb.AppendLine();
                            count++;
                        }
                    }
                }
            }

            return count > 0 ? sb.ToString() : "No parameters found";
        }

        private string FormatClassMembers(TypeInfo type, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== Class Members: {type.Name} ===");
            sb.AppendLine($"File: {filePath}");
            sb.AppendLine();

            if (type.Fields.Count > 0)
            {
                sb.AppendLine($"Fields: {type.Fields.Count}");
                foreach (var field in type.Fields)
                {
                    sb.AppendLine($"  {FormatLocation(field.Location)} {field.Name}: {field.Type} ({field.Modifiers})");
                }
                sb.AppendLine();
            }

            if (type.Properties.Count > 0)
            {
                sb.AppendLine($"Properties: {type.Properties.Count}");
                foreach (var prop in type.Properties)
                {
                    var accessors = new List<string>();
                    if (prop.HasGetter) accessors.Add("get");
                    if (prop.HasSetter) accessors.Add("set");
                    sb.AppendLine($"  {FormatLocation(prop.Location)} {prop.Name}: {prop.Type} ({prop.Modifiers}) {{ {string.Join(", ", accessors)} }}");
                }
                sb.AppendLine();
            }

            if (type.Methods.Count > 0)
            {
                sb.AppendLine($"Methods: {type.Methods.Count}");
                foreach (var method in type.Methods)
                {
                    sb.AppendLine($"  {FormatLocation(method.Location)} {FormatFunctionSignature(method)} ({method.Modifiers})");
                }
            }

            return sb.ToString();
        }

        private string FormatFields(TypeInfo type, string namePattern, string filePath)
        {
            var fields = string.IsNullOrEmpty(namePattern)
                ? type.Fields
                : type.Fields.Where(f => MatchesPattern(f.Name, namePattern)).ToList();

            if (fields.Count == 0)
                return $"No fields found in class '{type.Name}'";

            var sb = new StringBuilder();
            sb.AppendLine($"=== Fields in class {type.Name}: {fields.Count} ===");
            sb.AppendLine();

            foreach (var field in fields)
            {
                sb.AppendLine($"Field: {field.Name}");
                sb.AppendLine($"Class: {type.Name}");
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {FormatLocation(field.Location)}");
                sb.AppendLine($"Type: {field.Type}");
                if (!string.IsNullOrEmpty(field.Modifiers))
                    sb.AppendLine($"Modifiers: {field.Modifiers}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string FormatFieldDetail(FieldInfo field, string className, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Field: {field.Name}");
            sb.AppendLine($"Class: {className}");
            sb.AppendLine($"File: {filePath}");
            sb.AppendLine($"Location: {FormatLocation(field.Location)}");
            sb.AppendLine($"Type: {field.Type}");
            if (!string.IsNullOrEmpty(field.Modifiers))
                sb.AppendLine($"Modifiers: {field.Modifiers}");
            return sb.ToString();
        }

        private string FormatProperties(TypeInfo type, string namePattern, string filePath)
        {
            var properties = string.IsNullOrEmpty(namePattern)
                ? type.Properties
                : type.Properties.Where(p => MatchesPattern(p.Name, namePattern)).ToList();

            if (properties.Count == 0)
                return $"No properties found in class '{type.Name}'";

            var sb = new StringBuilder();
            sb.AppendLine($"=== Properties in class {type.Name}: {properties.Count} ===");
            sb.AppendLine();

            foreach (var prop in properties)
            {
                sb.AppendLine($"Property: {prop.Name}");
                sb.AppendLine($"Class: {type.Name}");
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {FormatLocation(prop.Location)}");
                sb.AppendLine($"Type: {prop.Type}");
                if (!string.IsNullOrEmpty(prop.Modifiers))
                    sb.AppendLine($"Modifiers: {prop.Modifiers}");
                var accessors = new List<string>();
                if (prop.HasGetter) accessors.Add("get");
                if (prop.HasSetter) accessors.Add("set");
                if (accessors.Count > 0)
                    sb.AppendLine($"Accessors: {string.Join(", ", accessors)}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string FormatPropertyDetail(PropertyInfo prop, string className, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Property: {prop.Name}");
            sb.AppendLine($"Class: {className}");
            sb.AppendLine($"File: {filePath}");
            sb.AppendLine($"Location: {FormatLocation(prop.Location)}");
            sb.AppendLine($"Type: {prop.Type}");
            if (!string.IsNullOrEmpty(prop.Modifiers))
                sb.AppendLine($"Modifiers: {prop.Modifiers}");
            var accessors = new List<string>();
            if (prop.HasGetter) accessors.Add("get");
            if (prop.HasSetter) accessors.Add("set");
            if (accessors.Count > 0)
                sb.AppendLine($"Accessors: {string.Join(", ", accessors)}");
            return sb.ToString();
        }

        private string FormatMethods(TypeInfo type, string namePattern, string filePath)
        {
            var methods = string.IsNullOrEmpty(namePattern)
                ? type.Methods
                : type.Methods.Where(m => MatchesPattern(m.Name, namePattern)).ToList();

            if (methods.Count == 0)
                return $"No methods found in class '{type.Name}'";

            var sb = new StringBuilder();
            sb.AppendLine($"=== Methods in class {type.Name}: {methods.Count} ===");
            sb.AppendLine();

            foreach (var method in methods)
            {
                sb.AppendLine($"Method: {method.Name}");
                sb.AppendLine($"Class: {type.Name}");
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {FormatLocation(method.Location)}");
                sb.AppendLine($"Signature: {FormatFunctionSignature(method)}");
                if (!string.IsNullOrEmpty(method.Modifiers))
                    sb.AppendLine($"Modifiers: {method.Modifiers}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string FormatMethodDetail(FunctionInfo method, string className, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Method: {method.Name}");
            sb.AppendLine($"Class: {className}");
            sb.AppendLine($"File: {filePath}");
            sb.AppendLine($"Location: {FormatLocation(method.Location)}");
            sb.AppendLine($"Signature: {FormatFunctionSignature(method)}");

            if (method.Parameters.Count > 0)
            {
                sb.AppendLine("Parameters:");
                foreach (var param in method.Parameters)
                {
                    sb.AppendLine($"  - {param.Type} {param.Name}");
                }
            }

            sb.AppendLine($"Return Type: {method.ReturnType}");
            if (!string.IsNullOrEmpty(method.Modifiers))
                sb.AppendLine($"Modifiers: {method.Modifiers}");

            return sb.ToString();
        }

        private string FormatEvents(TypeInfo type, string namePattern, string filePath)
        {
            var events = string.IsNullOrEmpty(namePattern)
                ? type.Events
                : type.Events.Where(e => MatchesPattern(e.Name, namePattern)).ToList();

            if (events.Count == 0)
                return $"No events found in class '{type.Name}'";

            var sb = new StringBuilder();
            sb.AppendLine($"=== Events in class {type.Name}: {events.Count} ===");
            sb.AppendLine();

            foreach (var evt in events)
            {
                sb.AppendLine($"Event: {evt.Name}");
                sb.AppendLine($"Class: {type.Name}");
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {FormatLocation(evt.Location)}");
                sb.AppendLine($"Type: {evt.Type}");
                if (!string.IsNullOrEmpty(evt.Modifiers))
                    sb.AppendLine($"Modifiers: {evt.Modifiers}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string FormatEventDetail(EventInfo evt, string className, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Event: {evt.Name}");
            sb.AppendLine($"Class: {className}");
            sb.AppendLine($"File: {filePath}");
            sb.AppendLine($"Location: {FormatLocation(evt.Location)}");
            sb.AppendLine($"Type: {evt.Type}");
            if (!string.IsNullOrEmpty(evt.Modifiers))
                sb.AppendLine($"Modifiers: {evt.Modifiers}");
            return sb.ToString();
        }

        private string FormatConstructors(TypeInfo type, string filePath)
        {
            if (type.Constructors.Count == 0)
                return $"No constructors found in class '{type.Name}'";

            var sb = new StringBuilder();
            sb.AppendLine($"=== Constructors in class {type.Name}: {type.Constructors.Count} ===");
            sb.AppendLine();

            foreach (var ctor in type.Constructors)
            {
                sb.AppendLine($"Constructor: {type.Name}");
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {FormatLocation(ctor.Location)}");
                sb.AppendLine($"Signature: {FormatFunctionSignature(ctor)}");

                if (ctor.Parameters.Count > 0)
                {
                    sb.AppendLine("Parameters:");
                    foreach (var param in ctor.Parameters)
                    {
                        sb.AppendLine($"  - {param.Type} {param.Name}");
                    }
                }

                if (!string.IsNullOrEmpty(ctor.Modifiers))
                    sb.AppendLine($"Modifiers: {ctor.Modifiers}");

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string FormatInterfaceList(List<TypeInfo> interfaces, string filePath)
        {
            if (interfaces.Count == 0)
                return "No interfaces found";

            var sb = new StringBuilder();
            sb.AppendLine($"=== Interfaces Found: {interfaces.Count} ===");
            sb.AppendLine();

            foreach (var iface in interfaces)
            {
                sb.AppendLine($"Interface: {iface.Name}");
                if (iface.IsNested)
                {
                    sb.AppendLine($"  Nested: Yes");
                    sb.AppendLine($"  Parent Type: {iface.ParentTypeName}");
                    sb.AppendLine($"  Full Type Name: {iface.FullTypeName}");
                }
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {FormatLocation(iface.Location)}");
                if (iface.BaseTypes.Count > 0)
                    sb.AppendLine($"Base Interfaces: {string.Join(", ", iface.BaseTypes)}");
                sb.AppendLine($"Methods: {iface.Methods.Count}");
                sb.AppendLine($"Properties: {iface.Properties.Count}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string FormatInterfaceDetail(TypeInfo iface, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Interface: {iface.Name}");
            if (iface.IsNested)
            {
                sb.AppendLine($"Nested: Yes");
                sb.AppendLine($"Parent Type: {iface.ParentTypeName}");
                sb.AppendLine($"Full Type Name: {iface.FullTypeName}");
            }
            sb.AppendLine($"File: {filePath}");
            sb.AppendLine($"Location: {FormatLocation(iface.Location)}");

            if (iface.BaseTypes.Count > 0)
                sb.AppendLine($"Base Interfaces: {string.Join(", ", iface.BaseTypes)}");

            if (iface.Methods.Count > 0)
            {
                sb.AppendLine($"Methods: {iface.Methods.Count}");
                foreach (var method in iface.Methods)
                {
                    sb.AppendLine($"  - {FormatFunctionSignature(method)} {FormatLocation(method.Location)}");
                }
            }

            if (iface.Properties.Count > 0)
            {
                sb.AppendLine($"Properties: {iface.Properties.Count}");
                foreach (var prop in iface.Properties)
                {
                    var accessors = new List<string>();
                    if (prop.HasGetter) accessors.Add("get");
                    if (prop.HasSetter) accessors.Add("set");
                    var accessorStr = accessors.Count > 0 ? $" {{{string.Join("; ", accessors)}}}" : "";
                    sb.AppendLine($"  - {prop.Name}: {prop.Type}{accessorStr} {FormatLocation(prop.Location)}");
                }
            }

            return sb.ToString();
        }

        private string FormatStructList(List<TypeInfo> structs, string filePath)
        {
            if (structs.Count == 0)
                return "No structs found";

            var sb = new StringBuilder();
            sb.AppendLine($"=== Structs Found: {structs.Count} ===");
            sb.AppendLine();

            foreach (var structType in structs)
            {
                sb.AppendLine($"Struct: {structType.Name}");
                if (structType.IsNested)
                {
                    sb.AppendLine($"  Nested: Yes");
                    sb.AppendLine($"  Parent Type: {structType.ParentTypeName}");
                    sb.AppendLine($"  Full Type Name: {structType.FullTypeName}");
                }
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {FormatLocation(structType.Location)}");
                sb.AppendLine($"Fields: {structType.Fields.Count}");
                sb.AppendLine($"Methods: {structType.Methods.Count}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string FormatStructDetail(TypeInfo structType, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Struct: {structType.Name}");
            if (structType.IsNested)
            {
                sb.AppendLine($"Nested: Yes");
                sb.AppendLine($"Parent Type: {structType.ParentTypeName}");
                sb.AppendLine($"Full Type Name: {structType.FullTypeName}");
            }
            sb.AppendLine($"File: {filePath}");
            sb.AppendLine($"Location: {FormatLocation(structType.Location)}");

            if (structType.Fields.Count > 0)
            {
                sb.AppendLine($"Fields: {structType.Fields.Count}");
                foreach (var field in structType.Fields)
                {
                    sb.AppendLine($"  - {field.Name}: {field.Type} {FormatLocation(field.Location)}");
                }
            }

            if (structType.Methods.Count > 0)
            {
                sb.AppendLine($"Methods: {structType.Methods.Count}");
                foreach (var method in structType.Methods)
                {
                    sb.AppendLine($"  - {FormatFunctionSignature(method)} {FormatLocation(method.Location)}");
                }
            }

            return sb.ToString();
        }

        private string FormatEnumList(List<EnumInfo> enums, string filePath)
        {
            if (enums.Count == 0)
                return "No enums found";

            var sb = new StringBuilder();
            sb.AppendLine($"=== Enums Found: {enums.Count} ===");
            sb.AppendLine();

            foreach (var enumType in enums)
            {
                sb.AppendLine($"Enum: {enumType.Name}");
                if (enumType.IsNested)
                {
                    sb.AppendLine($"  Nested: Yes");
                    sb.AppendLine($"  Parent Type: {enumType.ParentTypeName}");
                    sb.AppendLine($"  Full Type Name: {enumType.FullTypeName}");
                }
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {FormatLocation(enumType.Location)}");
                sb.AppendLine($"Members: {enumType.Members.Count}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string FormatEnumDetail(EnumInfo enumType, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Enum: {enumType.Name}");
            if (enumType.IsNested)
            {
                sb.AppendLine($"Nested: Yes");
                sb.AppendLine($"Parent Type: {enumType.ParentTypeName}");
                sb.AppendLine($"Full Type Name: {enumType.FullTypeName}");
            }
            sb.AppendLine($"File: {filePath}");
            sb.AppendLine($"Location: {FormatLocation(enumType.Location)}");

            if (enumType.Members.Count > 0)
            {
                sb.AppendLine($"Members: {enumType.Members.Count}");
                foreach (var member in enumType.Members)
                {
                    sb.AppendLine($"  - {member.Name} = {member.Value}");
                }
            }

            return sb.ToString();
        }

        private string FormatLocation(CodeLocation location)
        {
            if (location == null)
                return "[unknown]";

            int lineCount = location.EndLine - location.StartLine + 1;
            return $"[{location.StartLine}:{location.StartColumn}-{location.EndLine}:{location.EndColumn}] ({lineCount} line{(lineCount != 1 ? "s" : "")})";
        }

        private string FormatFunctionSignature(FunctionInfo func)
        {
            var paramStr = string.Join(", ", func.Parameters.Select(p => $"{p.Type} {p.Name}"));
            return $"{func.Name}({paramStr}) → {func.ReturnType}";
        }

        private List<TypeInfo> FindAllMatchingTypes(ParsedCodeFile parsed, string classNamePattern)
        {
            var matchingTypes = new List<TypeInfo>();

            // Handle C# Roslyn - prefer native syntax tree
            if (parsed.Language == ProgrammingLanguage.CSharp && parsed.NativeSyntaxTree is RoslynParsedFile roslynFile)
            {
                foreach (var cls in roslynFile.Classes)
                {
                    if (MatchesPattern(cls.Name, classNamePattern))
                    {
                        matchingTypes.Add(ConvertFromRoslynClass(cls));
                    }
                }
                return matchingTypes;
            }

            // Handle all languages including C# (fallback) - use Types collection
            if (parsed.Types != null && parsed.Types.Count > 0)
            {
                foreach (var type in parsed.Types)
                {
                    if (MatchesPattern(type.Name, classNamePattern))
                    {
                        matchingTypes.Add(type);
                    }
                }
            }

            return matchingTypes;
        }

        private bool MatchesPattern(string name, string pattern, bool ignoreCase = true)
        {
            try
            {
                var options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
                return Regex.IsMatch(name, pattern, options);
            }
            catch (ArgumentException)
            {
                var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                return name.IndexOf(pattern, comparison) >= 0;
            }
        }
    }
}
