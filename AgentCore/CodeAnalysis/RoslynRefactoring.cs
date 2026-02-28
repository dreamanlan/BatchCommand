using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Rename;

namespace AgentCore.CodeAnalysis
{
    // Code refactoring operations using Roslyn
    public class RoslynRefactoring
    {
        private ProjectLoader _projectLoader;
        private CSharpCompilation? _compilation;

        // Constructor for project-level refactoring
        public RoslynRefactoring(ProjectLoader projectLoader)
        {
            _projectLoader = projectLoader;
            _compilation = projectLoader.Compilation;
        }

        // Rename symbol across project
        public RoslynRefactoringResult RenameSymbol(string filePath, int line, int column, string newName)
        {
            try {
                var symbol = _projectLoader.GetSymbolAt(filePath, line, column);
                if (symbol == null) {
                    return new RoslynRefactoringResult {
                        Success = false,
                        Message = "Symbol not found at specified location"
                    };
                }

                // Find all references
                var references = new List<Location>();
                foreach (var tree in _compilation!.SyntaxTrees) {
                    var semanticModel = _compilation.GetSemanticModel(tree);
                    var root = tree.GetRoot();

                    var identifiers = root.DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Where(id => id.Identifier.Text == symbol.Name);

                    foreach (var identifier in identifiers) {
                        var symbolInfo = semanticModel.GetSymbolInfo(identifier);
                        if (SymbolEqualityComparer.Default.Equals(symbolInfo.Symbol, symbol)) {
                            references.Add(identifier.GetLocation());
                        }
                    }
                }

                // Also check declaration
                foreach (var location in symbol.Locations) {
                    if (location.IsInSource) {
                        references.Add(location);
                    }
                }

                // Build changes
                var changes = new List<RoslynFileChange>();
                var groupedByFile = references.GroupBy(r => r.SourceTree!.FilePath);

                foreach (var group in groupedByFile) {
                    var fileChanges = new List<RoslynTextChange>();
                    foreach (var location in group) {
                        var lineSpan = location.GetLineSpan();
                        fileChanges.Add(new RoslynTextChange {
                            Line = lineSpan.StartLinePosition.Line + 1,
                            Column = lineSpan.StartLinePosition.Character + 1,
                            OldText = symbol.Name,
                            NewText = newName
                        });
                    }

                    changes.Add(new RoslynFileChange {
                        FilePath = group.Key,
                        Changes = fileChanges
                    });
                }

                return new RoslynRefactoringResult {
                    Success = true,
                    Message = $"Found {references.Count} references to rename",
                    FileChanges = changes
                };
            }
            catch (Exception ex) {
                return new RoslynRefactoringResult {
                    Success = false,
                    Message = $"Rename failed: {ex.Message}"
                };
            }
        }

        // Extract method from code block
        public RoslynRefactoringResult ExtractMethod(string filePath, int startLine, int endLine, string methodName)
        {
            try {
                var tree = _projectLoader.GetSyntaxTree(filePath);
                var root = tree.GetRoot();
                var semanticModel = _projectLoader.GetSemanticModel(filePath);

                // Find statements in range
                var startPos = tree.GetText().Lines[startLine - 1].Start;
                var endPos = tree.GetText().Lines[endLine - 1].End;
                var span = Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(startPos, endPos);

                var statements = root.DescendantNodes(span)
                    .OfType<StatementSyntax>()
                    .Where(s => span.Contains(s.Span))
                    .ToList();

                if (statements.Count == 0) {
                    return new RoslynRefactoringResult {
                        Success = false,
                        Message = "No statements found in specified range"
                    };
                }

                // Analyze data flow
                var dataFlow = semanticModel.AnalyzeDataFlow(statements.First(), statements.Last());
                if (dataFlow == null) return new RoslynRefactoringResult { Success = false, Message = "Error: Could not analyze data flow" };

                // Get parameters (variables used but not declared in selection)
                var parameters = dataFlow.DataFlowsIn
                    .Where(s => s.Kind == SymbolKind.Local || s.Kind == SymbolKind.Parameter)
                    .Select(s => $"{(s as ILocalSymbol)?.Type?.ToDisplayString() ?? (s as IParameterSymbol)?.Type?.ToDisplayString() ?? "object"} {s.Name}")
                    .ToList();

                // Get return type (variables assigned in selection and used after)
                var returnVars = dataFlow.DataFlowsOut
                    .Where(s => s.Kind == SymbolKind.Local)
                    .ToList();

                string returnType = "void";
                string returnStatement = "";
                if (returnVars.Count == 1) {
                    returnType = (returnVars[0] as ILocalSymbol)?.Type?.ToDisplayString() ?? "object";
                    returnStatement = $"\nreturn {returnVars[0].Name};";
                }
                else if (returnVars.Count > 1) {
                    returnType = $"({string.Join(", ", returnVars.Select(v => (v as ILocalSymbol)?.Type?.ToDisplayString()))})";
                    returnStatement = $"\nreturn ({string.Join(", ", returnVars.Select(v => v.Name))});";
                }

                // Build new method
                var extractedCode = string.Join("\n", statements.Select(s => s.ToString()));
                var newMethod = $@"
private {returnType} {methodName}({string.Join(", ", parameters)})
{{
{extractedCode}{returnStatement}
}}";

                // Build method call
                var methodCall = returnVars.Count > 0
                    ? $"var result = {methodName}({string.Join(", ", dataFlow.DataFlowsIn.Select(s => s.Name))});"
                    : $"{methodName}({string.Join(", ", dataFlow.DataFlowsIn.Select(s => s.Name))});";

                return new RoslynRefactoringResult {
                    Success = true,
                    Message = "Method extracted successfully",
                    GeneratedCode = newMethod,
                    ReplacementCode = methodCall
                };
            }
            catch (Exception ex) {
                return new RoslynRefactoringResult {
                    Success = false,
                    Message = $"Extract method failed: {ex.Message}"
                };
            }
        }

        // Implement interface for a class
        public RoslynRefactoringResult ImplementInterface(string filePath, string className, string interfaceName)
        {
            try {
                var tree = _projectLoader.GetSyntaxTree(filePath);
                var root = tree.GetRoot();
                var semanticModel = _projectLoader.GetSemanticModel(filePath);

                // Find class
                var classDecl = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => c.Identifier.Text == className);

                if (classDecl == null) {
                    return new RoslynRefactoringResult {
                        Success = false,
                        Message = $"Class {className} not found"
                    };
                }

                // Find interface symbol
                var interfaceSymbol = _compilation!.GetTypeByMetadataName(interfaceName);
                if (interfaceSymbol == null) {
                    // Try to find in current compilation
                    interfaceSymbol = _compilation.GetSymbolsWithName(interfaceName, SymbolFilter.Type)
                        .OfType<INamedTypeSymbol>()
                        .FirstOrDefault(s => s.TypeKind == TypeKind.Interface);
                }

                if (interfaceSymbol == null) {
                    return new RoslynRefactoringResult {
                        Success = false,
                        Message = $"Interface {interfaceName} not found"
                    };
                }

                // Get interface members
                var members = interfaceSymbol.GetMembers()
                    .Where(m => m.Kind == SymbolKind.Method || m.Kind == SymbolKind.Property)
                    .ToList();

                // Generate implementations
                var implementations = new List<string>();

                foreach (var member in members) {
                    if (member is IMethodSymbol method && method.MethodKind == MethodKind.Ordinary) {
                        var parameters = string.Join(", ", method.Parameters.Select(p =>
                            $"{p.Type.ToDisplayString()} {p.Name}"));

                        implementations.Add($@"
public {method.ReturnType.ToDisplayString()} {method.Name}({parameters})
{{
    throw new NotImplementedException();
}}");
                    }
                    else if (member is IPropertySymbol property) {
                        var getter = property.GetMethod != null ? " get; " : "";
                        var setter = property.SetMethod != null ? " set; " : "";

                        implementations.Add($@"
public {property.Type.ToDisplayString()} {property.Name} {{ {getter}{setter}}}");
                    }
                }

                var generatedCode = string.Join("\n", implementations);

                return new RoslynRefactoringResult {
                    Success = true,
                    Message = $"Generated {implementations.Count} member implementations",
                    GeneratedCode = generatedCode
                };
            }
            catch (Exception ex) {
                return new RoslynRefactoringResult {
                    Success = false,
                    Message = $"Implement interface failed: {ex.Message}"
                };
            }
        }

        // Generate constructor for class
        public RoslynRefactoringResult GenerateConstructor(string filePath, string className, List<string> fieldNames)
        {
            try {
                var tree = _projectLoader.GetSyntaxTree(filePath);
                var root = tree.GetRoot();
                var semanticModel = _projectLoader.GetSemanticModel(filePath);

                // Find class
                var classDecl = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => c.Identifier.Text == className);

                if (classDecl == null) {
                    return new RoslynRefactoringResult {
                        Success = false,
                        Message = $"Class {className} not found"
                    };
                }

                // Get class symbol
                var classSymbol = semanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                if (classSymbol == null) {
                    return new RoslynRefactoringResult {
                        Success = false,
                        Message = "Could not get class symbol"
                    };
                }

                // Find fields
                var fields = new List<IFieldSymbol>();
                foreach (var fieldName in fieldNames) {
                    var field = classSymbol.GetMembers(fieldName)
                        .OfType<IFieldSymbol>()
                        .FirstOrDefault();

                    if (field != null) {
                        fields.Add(field);
                    }
                }

                if (fields.Count == 0) {
                    return new RoslynRefactoringResult {
                        Success = false,
                        Message = "No fields found"
                    };
                }

                // Generate constructor
                var parameters = string.Join(", ", fields.Select(f =>
                    $"{f.Type.ToDisplayString()} {ToCamelCase(f.Name)}"));

                var assignments = string.Join("\n    ", fields.Select(f =>
                    $"this.{f.Name} = {ToCamelCase(f.Name)};"));

                var constructor = $@"
public {className}({parameters})
{{
    {assignments}
}}";

                return new RoslynRefactoringResult {
                    Success = true,
                    Message = "Constructor generated successfully",
                    GeneratedCode = constructor
                };
            }
            catch (Exception ex) {
                return new RoslynRefactoringResult {
                    Success = false,
                    Message = $"Generate constructor failed: {ex.Message}"
                };
            }
        }

        // Add using directive to file
        public RoslynRefactoringResult AddUsing(string filePath, string namespaceName)
        {
            try {
                var tree = _projectLoader.GetSyntaxTree(filePath);
                var root = tree.GetRoot();

                // Check if using already exists
                var existingUsings = root.DescendantNodes()
                    .OfType<UsingDirectiveSyntax>()
                    .Select(u => u.Name?.ToString() ?? string.Empty)
                    .ToList();

                if (existingUsings.Contains(namespaceName)) {
                    return new RoslynRefactoringResult {
                        Success = false,
                        Message = $"Using {namespaceName} already exists"
                    };
                }

                // Generate using directive
                var usingDirective = $"using {namespaceName};";

                return new RoslynRefactoringResult {
                    Success = true,
                    Message = "Using directive generated",
                    GeneratedCode = usingDirective
                };
            }
            catch (Exception ex) {
                return new RoslynRefactoringResult {
                    Success = false,
                    Message = $"Add using failed: {ex.Message}"
                };
            }
        }

        // Helper: Convert to camelCase
        private string ToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            // Remove leading underscore if present
            if (name.StartsWith("_")) {
                name = name.Substring(1);
            }

            return char.ToLower(name[0]) + name.Substring(1);
        }
    }

    // Refactoring result
    public class RoslynRefactoringResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string GeneratedCode { get; set; } = string.Empty;
        public string ReplacementCode { get; set; } = string.Empty;
        public List<RoslynFileChange> FileChanges { get; set; } = new List<RoslynFileChange>();
    }

    // File change for refactoring
    public class RoslynFileChange
    {
        public string FilePath { get; set; } = string.Empty;
        public List<RoslynTextChange> Changes { get; set; } = new List<RoslynTextChange>();
    }

    // Text change for refactoring
    public class RoslynTextChange
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string OldText { get; set; } = string.Empty;
        public string NewText { get; set; } = string.Empty;
    }
}
