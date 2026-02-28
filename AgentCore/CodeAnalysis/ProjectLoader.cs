using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AgentCore.CodeAnalysis
{
    // Project loader for building Roslyn Compilation from .csproj files
    public class ProjectLoader
    {
        private string? _projectPath;
        private string? _projectDirectory;
        private XDocument? _projectXml;
        private CSharpCompilation? _compilation;
        private Dictionary<string, SyntaxTree> _syntaxTrees = new Dictionary<string, SyntaxTree>();

        public string? ProjectPath => _projectPath;
        public string? ProjectDirectory => _projectDirectory;
        public CSharpCompilation? Compilation => _compilation;
        public IReadOnlyDictionary<string, SyntaxTree> SyntaxTrees => _syntaxTrees;

        // Load project from .csproj file
        public static ProjectLoader LoadProject(string csprojPath)
        {
            if (!File.Exists(csprojPath)) {
                throw new FileNotFoundException($"Project file not found: {csprojPath}");
            }

            var loader = new ProjectLoader();
            loader._projectPath = Path.GetFullPath(csprojPath);
            loader._projectDirectory = Path.GetDirectoryName(loader._projectPath);

            try {
                loader._projectXml = XDocument.Load(csprojPath);
                loader.BuildCompilation();
                return loader;
            }
            catch (Exception ex) {
                throw new Exception($"Failed to load project: {ex.Message}", ex);
            }
        }

        // Build Compilation from project
        private void BuildCompilation()
        {
            // Get project name
            string projectName = Path.GetFileNameWithoutExtension(_projectPath) ?? string.Empty;

            // Parse all C# files in project
            var sourceFiles = GetSourceFiles();
            foreach (var file in sourceFiles) {
                try {
                    var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file), path: file);
                    _syntaxTrees[file] = tree;
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Warning: Failed to parse {file}: {ex.Message}");
                }
            }

            // Get compilation options
            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true,
                optimizationLevel: OptimizationLevel.Debug
            );

            // Get metadata references
            var references = GetMetadataReferences();

            // Create compilation
            _compilation = CSharpCompilation.Create(
                projectName,
                _syntaxTrees.Values,
                references,
                options
            );
        }

        // Get all C# source files in project
        private List<string> GetSourceFiles()
        {
            var files = new List<string>();

            // Get explicitly included files from .csproj
            var compileItems = _projectXml?.Descendants()
                .Where(e => e.Name.LocalName == "Compile")
                .Select(e => e.Attribute("Include")?.Value)
                .Where(v => v != null) ?? Enumerable.Empty<string?>();

            foreach (var item in compileItems) {
                var fullPath = Path.Combine(_projectDirectory ?? string.Empty, item ?? string.Empty);
                if (File.Exists(fullPath)) {
                    files.Add(fullPath);
                }
            }

            // If no explicit Compile items, use default glob pattern (SDK-style projects)
            if (files.Count == 0) {
            files = Directory.GetFiles(_projectDirectory ?? string.Empty, "*.cs", SearchOption.AllDirectories)
                    .Where(f => !f.Contains("\\obj\\") && !f.Contains("\\bin\\"))
                    .ToList();
            }

            return files;
        }

        // Get metadata references (assemblies)
        private List<MetadataReference> GetMetadataReferences()
        {
            var references = new List<MetadataReference>();

            // Add default .NET references
            AddDefaultReferences(references);

            // Add project references from .csproj
            AddProjectReferences(references);

            // Add package references (NuGet)
            AddPackageReferences(references);

            return references;
        }

        // Add default .NET framework references
        private void AddDefaultReferences(List<MetadataReference> references)
        {
            // Get runtime directory
            var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location) ?? string.Empty;

            // Core assemblies
            var coreAssemblies = new[] {
                "System.Runtime.dll",
                "System.Private.CoreLib.dll",
                "System.Console.dll",
                "System.Collections.dll",
                "System.Linq.dll",
                "System.Linq.Expressions.dll",
                "netstandard.dll",
                "mscorlib.dll"
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

            // Add all assemblies from runtime directory (fallback)
            if (references.Count == 0) {
                foreach (var dll in Directory.GetFiles(runtimeDir, "*.dll")) {
                    try {
                        references.Add(MetadataReference.CreateFromFile(dll));
                    }
                    catch {
                        // Skip if can't load
                    }
                }
            }
        }

        // Add project references
        private void AddProjectReferences(List<MetadataReference> references)
        {
            var projectRefs = _projectXml?.Descendants()
                .Where(e => e.Name.LocalName == "ProjectReference")
                .Select(e => e.Attribute("Include")?.Value)
                .Where(v => v != null) ?? Enumerable.Empty<string?>();

            foreach (var projRef in projectRefs) {
                var refPath = Path.Combine(_projectDirectory ?? string.Empty, projRef ?? string.Empty);
                if (File.Exists(refPath)) {
                    try {
                        // Try to find compiled DLL
                        var refDir = Path.GetDirectoryName(refPath) ?? string.Empty;
                        var refName = Path.GetFileNameWithoutExtension(refPath);
                        var dllPath = Path.Combine(refDir, "bin", "Debug", "net8.0", $"{refName}.dll");

                        if (File.Exists(dllPath)) {
                            references.Add(MetadataReference.CreateFromFile(dllPath));
                        }
                        else {
                            // Try other common paths
                            dllPath = Path.Combine(refDir, "bin", "Release", "net8.0", $"{refName}.dll");
                            if (File.Exists(dllPath)) {
                                references.Add(MetadataReference.CreateFromFile(dllPath));
                            }
                        }
                    }
                    catch {
                        // Skip if can't load
                    }
                }
            }
        }

        // Add NuGet package references
        private void AddPackageReferences(List<MetadataReference> references)
        {
            var packageRefQuery = _projectXml?.Descendants()
                .Where(e => e.Name.LocalName == "PackageReference")
                .Select(e => new {
                    Name = e.Attribute("Include")?.Value,
                    Version = e.Attribute("Version")?.Value ?? e.Element(XName.Get("Version", e.Name.NamespaceName))?.Value
                })
                .Where(p => p.Name != null)
                .ToList();
            var packageRefs = packageRefQuery ?? new List<object>().Select(o => new { Name = (string?)null, Version = (string?)null }).ToList();

            // Try to find NuGet packages in common locations
            var nugetPaths = new[] {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages"),
                Path.Combine(_projectDirectory ?? string.Empty, "packages")
            };

            foreach (var package in packageRefs) {
                foreach (var nugetPath in nugetPaths) {
                    if (!Directory.Exists(nugetPath)) continue;

                    var packageDir = Path.Combine(nugetPath, package.Name?.ToLower() ?? string.Empty);
                    if (!Directory.Exists(packageDir)) continue;

                    // Find version directory
                    string? versionDir = null;
                    if (package.Version != null) {
                        versionDir = Path.Combine(packageDir, package.Version);
                    }
                    else {
                        // Use latest version
                        var versions = Directory.GetDirectories(packageDir);
                        if (versions.Length > 0) {
                            versionDir = versions.OrderByDescending(v => v).First();
                        }
                    }

                    if (versionDir != null && Directory.Exists(versionDir)) {
                        // Find DLLs in lib folder
                        var libDir = Path.Combine(versionDir, "lib");
                        if (Directory.Exists(libDir)) {
                            var dlls = Directory.GetFiles(libDir, "*.dll", SearchOption.AllDirectories);
                            foreach (var dll in dlls) {
                                try {
                                    references.Add(MetadataReference.CreateFromFile(dll));
                                }
                                catch {
                                    // Skip if can't load
                                }
                            }
                        }
                    }
                }
            }
        }

        // Get semantic model for a file
        public SemanticModel GetSemanticModel(string filePath)
        {
            // Normalize path to handle both / and \ separators
            filePath = Path.GetFullPath(filePath.Replace('/', Path.DirectorySeparatorChar));

            if (!_syntaxTrees.ContainsKey(filePath)) {
                throw new FileNotFoundException($"File not in project: {filePath}");
            }

            return _compilation!.GetSemanticModel(_syntaxTrees[filePath]);
        }

        // Get syntax tree for a file (with path normalization)
        public SyntaxTree GetSyntaxTree(string filePath)
        {
            // Normalize path to handle both / and \ separators
            filePath = Path.GetFullPath(filePath.Replace('/', Path.DirectorySeparatorChar));

            if (!_syntaxTrees.ContainsKey(filePath)) {
                throw new FileNotFoundException($"File not in project: {filePath}");
            }

            return _syntaxTrees[filePath];
        }

        // Get all diagnostics for the project
        public IEnumerable<Diagnostic> GetDiagnostics()
        {
            return _compilation!.GetDiagnostics();
        }

        // Get diagnostics for a specific file
        public IEnumerable<Diagnostic> GetDiagnosticsForFile(string filePath)
        {
            // Normalize path to handle both / and \ separators
            filePath = Path.GetFullPath(filePath.Replace('/', Path.DirectorySeparatorChar));

            if (!_syntaxTrees.ContainsKey(filePath)) {
                return Enumerable.Empty<Diagnostic>();
            }

            var tree = _syntaxTrees[filePath];
            return _compilation!.GetDiagnostics().Where(d => d.Location.SourceTree == tree);
        }

        // Reload a file (after modification)
        public void ReloadFile(string filePath)
        {
            // Normalize path to handle both / and \ separators
            filePath = Path.GetFullPath(filePath.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(filePath)) {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var newTree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath), path: filePath);

            if (_syntaxTrees.ContainsKey(filePath)) {
                var oldTree = _syntaxTrees[filePath];
                _compilation = _compilation!.ReplaceSyntaxTree(oldTree, newTree);
                _syntaxTrees[filePath] = newTree;
            }
            else {
                _compilation = _compilation!.AddSyntaxTrees(newTree);
                _syntaxTrees[filePath] = newTree;
            }
        }

        // Get symbol at position
        public ISymbol? GetSymbolAt(string filePath, int line, int column)
        {
            // Normalize path to handle both / and \ separators
            filePath = Path.GetFullPath(filePath.Replace('/', Path.DirectorySeparatorChar));

            var semanticModel = GetSemanticModel(filePath);
            var tree = _syntaxTrees[filePath];
            var root = tree.GetRoot();

            // Convert line/column to position (0-based)
            var position = tree.GetText().Lines[line - 1].Start + (column - 1);

            var node = root.FindToken(position).Parent;
            return semanticModel.GetSymbolInfo(node!).Symbol;
        }

        // Get type info at position
        public Microsoft.CodeAnalysis.TypeInfo GetTypeInfoAt(string filePath, int line, int column)
        {
            // Normalize path to handle both / and \ separators
            filePath = Path.GetFullPath(filePath.Replace('/', Path.DirectorySeparatorChar));

            var semanticModel = GetSemanticModel(filePath);
            var tree = _syntaxTrees[filePath];
            var root = tree.GetRoot();

            // Convert line/column to position (0-based)
            var position = tree.GetText().Lines[line - 1].Start + (column - 1);

            var node = root.FindToken(position).Parent;
            return semanticModel.GetTypeInfo(node!);
        }
    }
}
