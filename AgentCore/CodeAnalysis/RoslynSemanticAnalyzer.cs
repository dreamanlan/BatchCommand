using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace AgentCore.CodeAnalysis
{
    // Semantic analysis using Roslyn
    public class RoslynSemanticAnalyzer
    {
        private ProjectLoader? _projectLoader;
        private CSharpCompilation _compilation = null!;

        // Constructor for project-level analysis (Phase 2)
        public RoslynSemanticAnalyzer(ProjectLoader projectLoader)
        {
            _projectLoader = projectLoader;
            _compilation = projectLoader.Compilation!;
        }

        // Constructor for single-file analysis (Phase 1)
        public RoslynSemanticAnalyzer(string code, string fileName = "temp.cs")
        {
            var tree = CSharpSyntaxTree.ParseText(code, path: fileName);

            // Create minimal compilation for single file
            var references = GetDefaultReferences();
            _compilation = CSharpCompilation.Create(
                "TempAssembly",
                new[] { tree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
        }

        // Get default .NET references for single-file analysis
        private static List<MetadataReference> GetDefaultReferences()
        {
            var references = new List<MetadataReference>();
            var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location) ?? string.Empty;

            var coreAssemblies = new[] {
                "System.Runtime.dll",
                "System.Private.CoreLib.dll",
                "System.Console.dll",
                "System.Collections.dll",
                "System.Linq.dll",
                "netstandard.dll"
            };

            foreach (var assembly in coreAssemblies) {
                var path = Path.Combine(runtimeDir, assembly);
                if (File.Exists(path)) {
                    try {
                        references.Add(MetadataReference.CreateFromFile(path));
                    }
                    catch {
                        // Skip if can't load
                    }
                }
            }

            return references;
        }

        // Get semantic model for code
        public SemanticModel GetSemanticModel(string code)
        {
            var tree = _compilation.SyntaxTrees.FirstOrDefault(t => t.GetText().ToString() == code);
            if (tree == null) {
                tree = CSharpSyntaxTree.ParseText(code);
                _compilation = _compilation.AddSyntaxTrees(tree);
            }
            return _compilation.GetSemanticModel(tree!);
        }

        // Get semantic model for file (project-level)
        public SemanticModel GetSemanticModelForFile(string filePath)
        {
            if (_projectLoader == null) {
                throw new InvalidOperationException("Project loader not available. Use project-level constructor.");
            }
            return _projectLoader.GetSemanticModel(filePath);
        }

        // Get type info at specific position
        public RoslynTypeInfo GetTypeInfoAt(string code, int line, int column)
        {
            // Use existing tree from compilation or create new one
            var tree = _compilation.SyntaxTrees.FirstOrDefault();
            if (tree == null || tree.GetText().ToString() != code) {
                tree = CSharpSyntaxTree.ParseText(code);
                _compilation = _compilation.AddSyntaxTrees(tree);
            }

            var semanticModel = _compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            // Convert line/column to position (0-based)
            var position = tree.GetText().Lines[line - 1].Start + (column - 1);
            var node = root.FindToken(position).Parent;

            var typeInfo = semanticModel.GetTypeInfo(node!);

            return new RoslynTypeInfo {
                Type = typeInfo.Type?.ToDisplayString() ?? string.Empty,
                ConvertedType = typeInfo.ConvertedType?.ToDisplayString() ?? string.Empty,
                IsNullable = typeInfo.Type?.NullableAnnotation == NullableAnnotation.Annotated,
                Line = line,
                Column = column,
                NodeText = node?.ToString() ?? string.Empty
            };
        }

        // Get type info at position in file (project-level)
        public RoslynTypeInfo GetTypeInfoAtInFile(string filePath, int line, int column)
        {
            if (_projectLoader == null) {
                throw new InvalidOperationException("Project loader not available.");
            }

            var semanticModel = _projectLoader.GetSemanticModel(filePath);
            var tree = _projectLoader.GetSyntaxTree(filePath);
            var root = tree.GetRoot();

            // Convert line/column to position (0-based)
            var position = tree.GetText().Lines[line - 1].Start + (column - 1);
            var node = root.FindToken(position).Parent;

            var typeInfo = semanticModel.GetTypeInfo(node!);

            // If GetTypeInfo returns null, try GetDeclaredSymbol for declarations
            string? typeString = typeInfo.Type?.ToDisplayString();
            if (string.IsNullOrEmpty(typeString)) {
                var symbol = semanticModel.GetDeclaredSymbol(node!);
                if (symbol != null) {
                    typeString = (symbol as IFieldSymbol)?.Type?.ToDisplayString()
                        ?? (symbol as IPropertySymbol)?.Type?.ToDisplayString()
                        ?? (symbol as ILocalSymbol)?.Type?.ToDisplayString()
                        ?? (symbol as IParameterSymbol)?.Type?.ToDisplayString();
                }
            }

            return new RoslynTypeInfo {
                Type = typeString ?? string.Empty,
                ConvertedType = typeInfo.ConvertedType?.ToDisplayString() ?? string.Empty,
                IsNullable = typeInfo.Type?.NullableAnnotation == NullableAnnotation.Annotated,
                Line = line,
                Column = column
            };
        }

        // Get symbol info at specific position
        public RoslynSymbolInfo GetSymbolInfoAt(string code, int line, int column)
        {
            // Use existing tree from compilation or create new one
            var tree = _compilation.SyntaxTrees.FirstOrDefault();
            if (tree == null || tree.GetText().ToString() != code) {
                tree = CSharpSyntaxTree.ParseText(code);
                _compilation = _compilation.AddSyntaxTrees(tree);
            }

            var semanticModel = _compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            // Convert line/column to position (0-based)
            var position = tree.GetText().Lines[line - 1].Start + (column - 1);
            var token = root.FindToken(position);

            // Try to find the most specific node (identifier)
            var node = token.Parent;
            while (node != null && !(node is IdentifierNameSyntax || node is SimpleNameSyntax))
            {
                if (node is MemberDeclarationSyntax || node is StatementSyntax)
                    break;
                node = node.Parent;
            }

            // If we found an identifier, use it; otherwise use the token's parent
            if (node == null || (!(node is IdentifierNameSyntax || node is SimpleNameSyntax)))
            {
                node = token.Parent;
            }

            var symbolInfo = semanticModel.GetSymbolInfo(node!);
            var symbol = symbolInfo.Symbol;

            if (symbol == null) {
                // Try to get declared symbol if this is a declaration
                symbol = semanticModel.GetDeclaredSymbol(node!);
            }

            if (symbol == null) {
                return new RoslynSymbolInfo {
                    Name = token.Text,
                    Kind = "Unknown",
                    Line = line,
                    Column = column
                };
            }

            return new RoslynSymbolInfo {
                Name = symbol.Name,
                Kind = symbol.Kind.ToString(),
                Type = (symbol as IMethodSymbol)?.ReturnType?.ToDisplayString()
                    ?? (symbol as IPropertySymbol)?.Type?.ToDisplayString()
                    ?? (symbol as IFieldSymbol)?.Type?.ToDisplayString()
                    ?? (symbol as ILocalSymbol)?.Type?.ToDisplayString()
                    ?? (symbol as IParameterSymbol)?.Type?.ToDisplayString() ?? string.Empty,
                ContainingType = symbol.ContainingType?.ToDisplayString() ?? string.Empty,
                ContainingNamespace = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                IsStatic = symbol.IsStatic,
                IsAbstract = symbol.IsAbstract,
                IsVirtual = symbol.IsVirtual,
                IsOverride = symbol.IsOverride,
                Accessibility = symbol.DeclaredAccessibility.ToString(),
                Line = line,
                Column = column,
                DefinitionLocation = GetSymbolLocation(symbol)
            };
        }

        // Get symbol info at position in file (project-level)
        public RoslynSymbolInfo GetSymbolInfoAtInFile(string filePath, int line, int column)
        {
            if (_projectLoader == null) {
                throw new InvalidOperationException("Project loader not available.");
            }

            var symbol = _projectLoader.GetSymbolAt(filePath, line, column);

            if (symbol == null) {
                return new RoslynSymbolInfo {
                    Name = "Unknown",
                    Kind = "Unknown",
                    Line = line,
                    Column = column
                };
            }

            return new RoslynSymbolInfo {
                Name = symbol.Name,
                Kind = symbol.Kind.ToString(),
                Type = (symbol as IMethodSymbol)?.ReturnType?.ToDisplayString()
                    ?? (symbol as IPropertySymbol)?.Type?.ToDisplayString()
                    ?? (symbol as IFieldSymbol)?.Type?.ToDisplayString() ?? string.Empty,
                ContainingType = symbol.ContainingType?.ToDisplayString() ?? string.Empty,
                ContainingNamespace = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                IsStatic = symbol.IsStatic,
                IsAbstract = symbol.IsAbstract,
                IsVirtual = symbol.IsVirtual,
                IsOverride = symbol.IsOverride,
                Accessibility = symbol.DeclaredAccessibility.ToString(),
                Line = line,
                Column = column,
                DefinitionLocation = GetSymbolLocation(symbol)
            };
        }

        // Get location of symbol definition
        private string GetSymbolLocation(ISymbol symbol)
        {
            var location = symbol.Locations.FirstOrDefault();
            if (location == null || !location.IsInSource) {
                return "metadata";
            }

            var lineSpan = location.GetLineSpan();
            return $"{lineSpan.Path}:{lineSpan.StartLinePosition.Line + 1}:{lineSpan.StartLinePosition.Character + 1}";
        }

        // Get all diagnostics (errors and warnings)
        public List<RoslynDiagnostic> GetDiagnostics(string code)
        {
            // Use existing tree from compilation or create new one
            var tree = _compilation.SyntaxTrees.FirstOrDefault();
            if (tree == null || tree.GetText().ToString() != code) {
                tree = CSharpSyntaxTree.ParseText(code);
                _compilation = _compilation.AddSyntaxTrees(tree);
            }

            var diagnostics = _compilation.GetDiagnostics();

            return diagnostics.Select(d => new RoslynDiagnostic {
                Id = d.Id,
                Message = d.GetMessage(),
                Severity = d.Severity.ToString(),
                Line = d.Location.GetLineSpan().StartLinePosition.Line + 1,
                Column = d.Location.GetLineSpan().StartLinePosition.Character + 1,
                FilePath = d.Location.SourceTree?.FilePath ?? ""
            }).ToList();
        }

        // Get diagnostics for project
        public List<RoslynDiagnostic> GetProjectDiagnostics()
        {
            if (_projectLoader == null) {
                throw new InvalidOperationException("Project loader not available.");
            }

            var diagnostics = _projectLoader.GetDiagnostics();

            return diagnostics.Select(d => new RoslynDiagnostic {
                Id = d.Id,
                Message = d.GetMessage(),
                Severity = d.Severity.ToString(),
                Line = d.Location.GetLineSpan().StartLinePosition.Line + 1,
                Column = d.Location.GetLineSpan().StartLinePosition.Character + 1,
                FilePath = d.Location.SourceTree?.FilePath ?? ""
            }).ToList();
        }

        // Get diagnostics for specific file
        public List<RoslynDiagnostic> GetDiagnosticsForFile(string filePath)
        {
            if (_projectLoader == null) {
                throw new InvalidOperationException("Project loader not available.");
            }

            var diagnostics = _projectLoader.GetDiagnosticsForFile(filePath);

            return diagnostics.Select(d => new RoslynDiagnostic {
                Id = d.Id,
                Message = d.GetMessage(),
                Severity = d.Severity.ToString(),
                Line = d.Location.GetLineSpan().StartLinePosition.Line + 1,
                Column = d.Location.GetLineSpan().StartLinePosition.Character + 1,
                FilePath = d.Location.SourceTree?.FilePath ?? ""
            }).ToList();
        }

        // Find all references to a symbol in code
        public List<RoslynReference> FindReferencesInCode(string code, string symbolName)
        {
            // Use existing tree from compilation or create new one
            var tree = _compilation.SyntaxTrees.FirstOrDefault();
            if (tree == null || tree.GetText().ToString() != code) {
                tree = CSharpSyntaxTree.ParseText(code);
                _compilation = _compilation.AddSyntaxTrees(tree);
            }

            var semanticModel = _compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            var references = new List<RoslynReference>();

            // Find all identifier nodes matching the symbol name
            var identifiers = root.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(id => id.Identifier.Text == symbolName);

            foreach (var identifier in identifiers) {
                var symbolInfo = semanticModel.GetSymbolInfo(identifier);
                if (symbolInfo.Symbol != null) {
                    var lineSpan = tree.GetLineSpan(identifier.Span);
                    references.Add(new RoslynReference {
                        SymbolName = symbolName,
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        FilePath = tree.FilePath,
                        Context = identifier.Parent?.ToString() ?? string.Empty
                    });
                }
            }

            return references;
        }

        // Find all references to a symbol in project
        public List<RoslynReference> FindAllReferences(string symbolName)
        {
            if (_projectLoader == null) {
                throw new InvalidOperationException("Project loader not available.");
            }

            var references = new List<RoslynReference>();

            foreach (var tree in _compilation.SyntaxTrees) {
                var semanticModel = _compilation.GetSemanticModel(tree);
                var root = tree.GetRoot();

                var identifiers = root.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Where(id => id.Identifier.Text == symbolName);

                foreach (var identifier in identifiers) {
                    var symbolInfo = semanticModel.GetSymbolInfo(identifier);
                    if (symbolInfo.Symbol != null) {
                        var lineSpan = tree.GetLineSpan(identifier.Span);
                        references.Add(new RoslynReference {
                            SymbolName = symbolName,
                            Line = lineSpan.StartLinePosition.Line + 1,
                            Column = lineSpan.StartLinePosition.Character + 1,
                            FilePath = tree.FilePath,
                            Context = identifier.Parent?.ToString() ?? string.Empty
                        });
                    }
                }
            }

            return references;
        }

        // Get all types used in code
        public List<string> GetUsedTypes(string code)
        {
            // Use existing tree from compilation or create new one
            var tree = _compilation.SyntaxTrees.FirstOrDefault();
            if (tree == null || tree.GetText().ToString() != code) {
                tree = CSharpSyntaxTree.ParseText(code);
                _compilation = _compilation.AddSyntaxTrees(tree);
            }

            var semanticModel = _compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            var types = new HashSet<string>();

            // Find all type references
            var typeNodes = root.DescendantNodes()
                .Where(n => n is TypeSyntax || n is IdentifierNameSyntax);

            foreach (var node in typeNodes) {
                var typeInfo = semanticModel.GetTypeInfo(node);
                if (typeInfo.Type != null && typeInfo.Type.TypeKind != TypeKind.Error) {
                    types.Add(typeInfo.Type.ToDisplayString());
                }
            }

            return types.OrderBy(t => t).ToList();
        }

        // Get all symbols defined in code
        public List<RoslynSymbolInfo> GetDefinedSymbols(string code)
        {
            // Use existing tree from compilation or create new one
            var tree = _compilation.SyntaxTrees.FirstOrDefault();
            if (tree == null || tree.GetText().ToString() != code) {
                tree = CSharpSyntaxTree.ParseText(code);
                _compilation = _compilation.AddSyntaxTrees(tree);
            }

            var semanticModel = _compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            var symbols = new List<RoslynSymbolInfo>();

            // Find all declarations
            var declarations = root.DescendantNodes()
                .Where(n => n is ClassDeclarationSyntax
                    || n is MethodDeclarationSyntax
                    || n is PropertyDeclarationSyntax
                    || n is FieldDeclarationSyntax
                    || n is InterfaceDeclarationSyntax
                    || n is StructDeclarationSyntax
                    || n is EnumDeclarationSyntax);

            foreach (var decl in declarations) {
                var symbol = semanticModel.GetDeclaredSymbol(decl);
                if (symbol != null) {
                    var location = symbol.Locations.FirstOrDefault();
                    var lineSpan = location?.GetLineSpan();

                    symbols.Add(new RoslynSymbolInfo {
                        Name = symbol.Name,
                        Kind = symbol.Kind.ToString(),
                        Type = (symbol as IMethodSymbol)?.ReturnType?.ToDisplayString()
                            ?? (symbol as IPropertySymbol)?.Type?.ToDisplayString()
                            ?? (symbol as IFieldSymbol)?.Type?.ToDisplayString() ?? string.Empty,
                        ContainingType = symbol.ContainingType?.ToDisplayString() ?? string.Empty,
                        ContainingNamespace = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                        IsStatic = symbol.IsStatic,
                        IsAbstract = symbol.IsAbstract,
                        IsVirtual = symbol.IsVirtual,
                        IsOverride = symbol.IsOverride,
                        Accessibility = symbol.DeclaredAccessibility.ToString(),
                        Line = lineSpan?.StartLinePosition.Line + 1 ?? 0,
                        Column = lineSpan?.StartLinePosition.Character + 1 ?? 0
                    });
                }
            }

            return symbols;
        }

        // Check if code compiles without errors
        public bool CanCompile(string code)
        {
            var diagnostics = GetDiagnostics(code);
            return !diagnostics.Any(d => d.Severity == "Error");
        }

        // Get compilation errors only
        public List<RoslynDiagnostic> GetErrors(string code)
        {
            var diagnostics = GetDiagnostics(code);
            return diagnostics.Where(d => d.Severity == "Error").ToList();
        }

        // Get compilation warnings only
        public List<RoslynDiagnostic> GetWarnings(string code)
        {
            var diagnostics = GetDiagnostics(code);
            return diagnostics.Where(d => d.Severity == "Warning").ToList();
        }
    }

    // Type information result
    public class RoslynTypeInfo
    {
        public string Type { get; set; } = string.Empty;
        public string ConvertedType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string NodeText { get; set; } = string.Empty;
    }

    // Symbol information result
    public class RoslynSymbolInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string ContainingType { get; set; } = string.Empty;
        public string ContainingNamespace { get; set; } = string.Empty;
        public bool IsStatic { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsOverride { get; set; }
        public string Accessibility { get; set; } = string.Empty;
        public int Line { get; set; }
        public int Column { get; set; }
        public string DefinitionLocation { get; set; } = string.Empty;
    }

    // Diagnostic (error/warning) result
    public class RoslynDiagnostic
    {
        public string Id { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int Line { get; set; }
        public int Column { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }

    // Reference location result
    public class RoslynReference
    {
        public string SymbolName { get; set; } = string.Empty;
        public int Line { get; set; }
        public int Column { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
    }
}
