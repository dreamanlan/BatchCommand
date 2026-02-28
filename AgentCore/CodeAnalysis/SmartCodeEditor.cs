using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace AgentCore.CodeAnalysis
{
    /// <summary>
    /// Smart code editor for self-modification using Roslyn
    /// Enables agent to add, replace, and modify its own code
    /// </summary>
    public class SmartCodeEditor
    {
        /// <summary>
        /// Add a new method to an existing class
        /// </summary>
        /// <param name="filePath">Path to the C# file</param>
        /// <param name="className">Name of the class to add method to</param>
        /// <param name="methodCode">Complete method code including modifiers, return type, name, parameters, and body</param>
        /// <returns>True if successful</returns>
        public static bool AddMethodToClass(string filePath, string className, string methodCode)
        {
            try {
                if (!File.Exists(filePath)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] File not found: {filePath}");
                    return false;
                }

                // Parse the file
                string originalCode = File.ReadAllText(filePath);
                var tree = CSharpSyntaxTree.ParseText(originalCode, path: filePath);
                var root = (CompilationUnitSyntax)tree.GetRoot();

                // Find the target class
                var classDecl = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => c.Identifier.Text == className);

                if (classDecl == null) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Class '{className}' not found in {filePath}");
                    return false;
                }

                // Parse the method code
                var methodTree = CSharpSyntaxTree.ParseText($"class Temp {{ {methodCode} }}");
                var methodRoot = (CompilationUnitSyntax)methodTree.GetRoot();
                var tempClass = methodRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
                var newMethod = tempClass.Members.First();

                // Add the method to the class
                var updatedClass = classDecl.AddMembers(newMethod);

                // Replace the old class with the updated one
                var newRoot = root.ReplaceNode(classDecl, updatedClass);

                // Format the code
                var workspace = new AdhocWorkspace();
                var formattedRoot = Formatter.Format(newRoot, workspace);

                // Write back to file
                File.WriteAllText(filePath, formattedRoot.ToFullString(), Encoding.UTF8);

                return true;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Error adding method: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Replace an existing method in a class
        /// </summary>
        /// <param name="filePath">Path to the C# file</param>
        /// <param name="className">Name of the class containing the method</param>
        /// <param name="methodName">Name of the method to replace</param>
        /// <param name="newMethodCode">New method code</param>
        /// <returns>True if successful</returns>
        public static bool ReplaceMethod(string filePath, string className, string methodName, string newMethodCode)
        {
            try {
                if (!File.Exists(filePath)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] File not found: {filePath}");
                    return false;
                }

                // Parse the file
                string originalCode = File.ReadAllText(filePath);
                var tree = CSharpSyntaxTree.ParseText(originalCode, path: filePath);
                var root = (CompilationUnitSyntax)tree.GetRoot();

                // Find the target class
                var classDecl = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => c.Identifier.Text == className);

                if (classDecl == null) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Class '{className}' not found in {filePath}");
                    return false;
                }

                // Find the target method
                var oldMethod = classDecl.Members
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(m => m.Identifier.Text == methodName);

                if (oldMethod == null) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Method '{methodName}' not found in class '{className}'");
                    return false;
                }

                // Parse the new method code
                var methodTree = CSharpSyntaxTree.ParseText($"class Temp {{ {newMethodCode} }}");
                var methodRoot = (CompilationUnitSyntax)methodTree.GetRoot();
                var tempClass = methodRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
                var newMethod = tempClass.Members.First() as MethodDeclarationSyntax;

                if (newMethod == null) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Failed to parse new method code");
                    return false;
                }

                // Replace the method
                var updatedClass = classDecl.ReplaceNode(oldMethod, newMethod);
                var newRoot = root.ReplaceNode(classDecl, updatedClass);

                // Format the code
                var workspace = new AdhocWorkspace();
                var formattedRoot = Formatter.Format(newRoot, workspace);

                // Write back to file
                File.WriteAllText(filePath, formattedRoot.ToFullString(), Encoding.UTF8);

                return true;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Error replacing method: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Insert a method after another method in a class
        /// </summary>
        /// <param name="filePath">Path to the C# file</param>
        /// <param name="className">Name of the class</param>
        /// <param name="afterMethodName">Name of the method to insert after</param>
        /// <param name="newMethodCode">New method code to insert</param>
        /// <returns>True if successful</returns>
        public static bool InsertMethodAfter(string filePath, string className, string afterMethodName, string newMethodCode)
        {
            try {
                if (!File.Exists(filePath)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] File not found: {filePath}");
                    return false;
                }

                // Parse the file
                string originalCode = File.ReadAllText(filePath);
                var tree = CSharpSyntaxTree.ParseText(originalCode, path: filePath);
                var root = (CompilationUnitSyntax)tree.GetRoot();

                // Find the target class
                var classDecl = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => c.Identifier.Text == className);

                if (classDecl == null) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Class '{className}' not found in {filePath}");
                    return false;
                }

                // Find the reference method
                var afterMethod = classDecl.Members
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(m => m.Identifier.Text == afterMethodName);

                if (afterMethod == null) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Method '{afterMethodName}' not found in class '{className}'");
                    return false;
                }

                // Parse the new method code
                var methodTree = CSharpSyntaxTree.ParseText($"class Temp {{ {newMethodCode} }}");
                var methodRoot = (CompilationUnitSyntax)methodTree.GetRoot();
                var tempClass = methodRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
                var newMethod = tempClass.Members.First();

                // Find the index of the reference method
                var members = classDecl.Members.ToList();
                int index = members.IndexOf(afterMethod);

                // Insert the new method after the reference method
                members.Insert(index + 1, newMethod);

                // Create updated class with new members
                var updatedClass = classDecl.WithMembers(SyntaxFactory.List(members));
                var newRoot = root.ReplaceNode(classDecl, updatedClass);

                // Format the code
                var workspace = new AdhocWorkspace();
                var formattedRoot = Formatter.Format(newRoot, workspace);

                // Write back to file
                File.WriteAllText(filePath, formattedRoot.ToFullString(), Encoding.UTF8);

                return true;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Error inserting method: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete a method from a class
        /// </summary>
        /// <param name="filePath">Path to the C# file</param>
        /// <param name="className">Name of the class</param>
        /// <param name="methodName">Name of the method to delete</param>
        /// <returns>True if successful</returns>
        public static bool DeleteMethod(string filePath, string className, string methodName)
        {
            try {
                if (!File.Exists(filePath)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] File not found: {filePath}");
                    return false;
                }

                // Parse the file
                string originalCode = File.ReadAllText(filePath);
                var tree = CSharpSyntaxTree.ParseText(originalCode, path: filePath);
                var root = (CompilationUnitSyntax)tree.GetRoot();

                // Find the target class
                var classDecl = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => c.Identifier.Text == className);

                if (classDecl == null) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Class '{className}' not found in {filePath}");
                    return false;
                }

                // Find the target method
                var methodToDelete = classDecl.Members
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(m => m.Identifier.Text == methodName);

                if (methodToDelete == null) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Method '{methodName}' not found in class '{className}'");
                    return false;
                }

                // Remove the method
                var updatedClass = classDecl.RemoveNode(methodToDelete!, SyntaxRemoveOptions.KeepNoTrivia);
                var newRoot = root.ReplaceNode(classDecl, updatedClass!);

                // Format the code
                var workspace = new AdhocWorkspace();
                var formattedRoot = Formatter.Format(newRoot, workspace);

                // Write back to file
                File.WriteAllText(filePath, formattedRoot.ToFullString(), Encoding.UTF8);

                return true;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Error deleting method: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Add a new class to a file
        /// </summary>
        /// <param name="filePath">Path to the C# file</param>
        /// <param name="classCode">Complete class code</param>
        /// <returns>True if successful</returns>
        public static bool AddClassToFile(string filePath, string classCode)
        {
            try {
                if (!File.Exists(filePath)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] File not found: {filePath}");
                    return false;
                }

                // Parse the file
                string originalCode = File.ReadAllText(filePath);
                var tree = CSharpSyntaxTree.ParseText(originalCode, path: filePath);
                var root = (CompilationUnitSyntax)tree.GetRoot();

                // Parse the new class code
                var classTree = CSharpSyntaxTree.ParseText(classCode);
                var classRoot = (CompilationUnitSyntax)classTree.GetRoot();
                var newClass = classRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

                if (newClass == null) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Failed to parse class code");
                    return false;
                }

                // Find the namespace to add the class to
                var namespaceDecl = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                if (namespaceDecl != null) {
                    var updatedNamespace = namespaceDecl.AddMembers(newClass);
                    var newRoot = root.ReplaceNode(namespaceDecl, updatedNamespace);

                    // Format and write
                    var workspace = new AdhocWorkspace();
                    var formattedRoot = Formatter.Format(newRoot, workspace);
                    File.WriteAllText(filePath, formattedRoot.ToFullString(), Encoding.UTF8);
                }
                else {
                    // Check for file-scoped namespace
                    var fileScopedNamespace = root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
                    if (fileScopedNamespace != null) {
                        var updatedNamespace = fileScopedNamespace.AddMembers(newClass);
                        var newRoot = root.ReplaceNode(fileScopedNamespace, updatedNamespace);

                        // Format and write
                        var workspace = new AdhocWorkspace();
                        var formattedRoot = Formatter.Format(newRoot, workspace);
                        File.WriteAllText(filePath, formattedRoot.ToFullString(), Encoding.UTF8);
                    }
                    else {
                        // No namespace, add to root
                        var newRoot = root.AddMembers(newClass);

                        // Format and write
                        var workspace = new AdhocWorkspace();
                        var formattedRoot = Formatter.Format(newRoot, workspace);
                        File.WriteAllText(filePath, formattedRoot.ToFullString(), Encoding.UTF8);
                    }
                }

                return true;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Error adding class: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create a new C# file with the given code
        /// </summary>
        /// <param name="filePath">Path for the new file</param>
        /// <param name="code">Complete C# code</param>
        /// <returns>True if successful</returns>
        public static bool CreateNewFile(string filePath, string code)
        {
            try {
                // Ensure directory exists
                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
                    Directory.CreateDirectory(directory);
                }

                // Parse and format the code
                var tree = CSharpSyntaxTree.ParseText(code);
                var root = (CompilationUnitSyntax)tree.GetRoot();

                var workspace = new AdhocWorkspace();
                var formattedRoot = Formatter.Format(root, workspace);

                // Write to file
                File.WriteAllText(filePath, formattedRoot.ToFullString(), Encoding.UTF8);

                return true;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Error creating file: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Verify that code compiles without errors
        /// </summary>
        /// <param name="code">C# code to verify</param>
        /// <returns>True if code compiles successfully</returns>
        public static bool VerifyCodeCompiles(string code)
        {
            try {
                var tree = CSharpSyntaxTree.ParseText(code);

                // Add comprehensive references
                var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location) ?? string.Empty;
                var references = new List<MetadataReference>
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Collections.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll"))
                };

                var compilation = CSharpCompilation.Create(
                    "TempAssembly",
                    syntaxTrees: new[] { tree },
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                var diagnostics = compilation.GetDiagnostics();
                var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

                if (errors.Any()) {
                    var logger = CefDotnetApp.AgentCore.Core.AgentCore.Instance.Logger;
                    logger.Warning($"[SmartCodeEditor] Code has {errors.Count} compilation errors:");
                    foreach (var error in errors.Take(5)) {
                        logger.Warning($"  {error.GetMessage()}");
                    }
                    return false;
                }

                return true;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[SmartCodeEditor] Error verifying code: {ex.Message}");
                return false;
            }
        }
    }
}
