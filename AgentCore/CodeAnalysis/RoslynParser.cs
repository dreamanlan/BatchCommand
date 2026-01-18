using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CefDotnetApp.AgentCore.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AgentCore.CodeAnalysis
{
    // Core Roslyn-based C# code parser
    public class RoslynParser
    {
        // Parse a C# file and return syntax tree
        public static SyntaxTree ParseFile(string filePath)
        {
            if (!File.Exists(filePath)) {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            string code = File.ReadAllText(filePath);
            return CSharpSyntaxTree.ParseText(code, path: filePath);
        }

        // Parse C# code from string
        public static SyntaxTree ParseCode(string code, string fileName = "")
        {
            return CSharpSyntaxTree.ParseText(code, path: fileName);
        }

        // Extract all classes from a syntax tree
        public static List<RoslynClassInfo> ExtractClasses(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            var classes = new List<RoslynClassInfo>();

            var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDecl in classDeclarations) {
                var classInfo = new RoslynClassInfo {
                    Name = classDecl.Identifier.Text,
                    Modifiers = string.Join(" ", classDecl.Modifiers.Select(m => m.Text)),
                    Location = GetLocation(tree, classDecl),
                    SyntaxNode = classDecl
                };

                // Extract nested type information
                BuildNestedTypeInfo(classDecl, out bool isNested, out string parentTypeName, out string fullTypeName);
                classInfo.IsNested = isNested;
                classInfo.ParentTypeName = parentTypeName;
                classInfo.FullTypeName = fullTypeName;

                // Extract namespace
                var namespaceDecl = classDecl.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                if (namespaceDecl != null) {
                    classInfo.Namespace = namespaceDecl.Name.ToString();
                }
                else {
                    // Check for file-scoped namespace (C# 10+)
                    var fileScopedNamespace = classDecl.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
                    if (fileScopedNamespace != null) {
                        classInfo.Namespace = fileScopedNamespace.Name.ToString();
                    }
                }

                // Extract base types
                if (classDecl.BaseList != null) {
                    foreach (var baseType in classDecl.BaseList.Types) {
                        classInfo.BaseTypes.Add(baseType.Type.ToString());
                    }
                }

                // Extract methods
                classInfo.Methods = ExtractMethods(tree, classDecl);

                // Extract properties
                classInfo.Properties = ExtractProperties(tree, classDecl);

                // Extract fields
                classInfo.Fields = ExtractFields(tree, classDecl);

                // Extract constructors
                classInfo.Constructors = ExtractConstructors(tree, classDecl);

                // Extract events
                classInfo.Events = ExtractEvents(tree, classDecl);

                classes.Add(classInfo);
            }

            return classes;
        }

        // Extract methods from a type declaration (class, struct, or interface)
        public static List<RoslynMethodInfo> ExtractMethods(SyntaxTree tree, TypeDeclarationSyntax typeDecl)
        {
            var methods = new List<RoslynMethodInfo>();

            var methodDeclarations = typeDecl.Members.OfType<MethodDeclarationSyntax>();

            foreach (var methodDecl in methodDeclarations) {
                var methodInfo = new RoslynMethodInfo {
                    Name = methodDecl.Identifier.Text,
                    ReturnType = methodDecl.ReturnType.ToString(),
                    Modifiers = string.Join(" ", methodDecl.Modifiers.Select(m => m.Text)),
                    Location = GetLocation(tree, methodDecl),
                    FullText = methodDecl.ToString(),
                    SyntaxNode = methodDecl
                };

                // Extract parameters
                foreach (var param in methodDecl.ParameterList.Parameters) {
                    methodInfo.Parameters.Add(new RoslynParameterInfo {
                        Name = param.Identifier.Text,
                        Type = param.Type?.ToString() ?? "var"
                    });
                }

                methods.Add(methodInfo);
            }

            return methods;
        }

        // Extract properties from a type declaration
        public static List<RoslynPropertyInfo> ExtractProperties(SyntaxTree tree, TypeDeclarationSyntax typeDecl)
        {
            var properties = new List<RoslynPropertyInfo>();

            var propertyDeclarations = typeDecl.Members.OfType<PropertyDeclarationSyntax>();

            foreach (var propDecl in propertyDeclarations) {
                var propInfo = new RoslynPropertyInfo {
                    Name = propDecl.Identifier.Text,
                    Type = propDecl.Type.ToString(),
                    Modifiers = string.Join(" ", propDecl.Modifiers.Select(m => m.Text)),
                    Location = GetLocation(tree, propDecl),
                    SyntaxNode = propDecl
                };

                // Check for getter and setter
                if (propDecl.AccessorList != null) {
                    foreach (var accessor in propDecl.AccessorList.Accessors) {
                        if (accessor.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration) {
                            propInfo.HasGetter = true;
                        }
                        else if (accessor.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.SetAccessorDeclaration) {
                            propInfo.HasSetter = true;
                        }
                    }

                    // Check if it's an auto-property (no body in accessors)
                    propInfo.IsAutoProperty = propDecl.AccessorList.Accessors.All(a => a.Body == null && a.ExpressionBody == null);
                }
                else if (propDecl.ExpressionBody != null) {
                    // Expression-bodied property (e.g., public int X => 5;)
                    propInfo.HasGetter = true;
                    propInfo.IsAutoProperty = false;
                }

                properties.Add(propInfo);
            }

            return properties;
        }

        // Extract fields from a type declaration
        public static List<RoslynFieldInfo> ExtractFields(SyntaxTree tree, TypeDeclarationSyntax typeDecl)
        {
            var fields = new List<RoslynFieldInfo>();

            var fieldDeclarations = typeDecl.Members.OfType<FieldDeclarationSyntax>();

            foreach (var fieldDecl in fieldDeclarations) {
                var modifiers = string.Join(" ", fieldDecl.Modifiers.Select(m => m.Text));
                var isReadonly = fieldDecl.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ReadOnlyKeyword));
                var isConst = fieldDecl.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ConstKeyword));

                // A field declaration can have multiple variables (e.g., int x, y, z;)
                foreach (var variable in fieldDecl.Declaration.Variables) {
                    var fieldInfo = new RoslynFieldInfo {
                        Name = variable.Identifier.Text,
                        Type = fieldDecl.Declaration.Type.ToString(),
                        Modifiers = modifiers,
                        Location = GetLocation(tree, fieldDecl),
                        IsReadonly = isReadonly,
                        IsConst = isConst,
                        SyntaxNode = fieldDecl
                    };

                    fields.Add(fieldInfo);
                }
            }

            return fields;
        }

        // Extract constructors from a type declaration
        public static List<RoslynConstructorInfo> ExtractConstructors(SyntaxTree tree, TypeDeclarationSyntax typeDecl)
        {
            var constructors = new List<RoslynConstructorInfo>();

            var constructorDeclarations = typeDecl.Members.OfType<ConstructorDeclarationSyntax>();

            foreach (var ctorDecl in constructorDeclarations) {
                var ctorInfo = new RoslynConstructorInfo {
                    Name = ctorDecl.Identifier.Text,
                    Modifiers = string.Join(" ", ctorDecl.Modifiers.Select(m => m.Text)),
                    Location = GetLocation(tree, ctorDecl),
                    FullText = ctorDecl.ToString(),
                    SyntaxNode = ctorDecl
                };

                // Extract parameters
                foreach (var param in ctorDecl.ParameterList.Parameters) {
                    ctorInfo.Parameters.Add(new RoslynParameterInfo {
                        Name = param.Identifier.Text,
                        Type = param.Type?.ToString() ?? "var"
                    });
                }

                constructors.Add(ctorInfo);
            }

            return constructors;
        }

        // Extract events from a type declaration
        public static List<RoslynEventInfo> ExtractEvents(SyntaxTree tree, TypeDeclarationSyntax typeDecl)
        {
            var events = new List<RoslynEventInfo>();

            var eventDeclarations = typeDecl.Members.OfType<EventDeclarationSyntax>();

            foreach (var eventDecl in eventDeclarations) {
                var eventInfo = new RoslynEventInfo {
                    Name = eventDecl.Identifier.Text,
                    Type = eventDecl.Type.ToString(),
                    Modifiers = string.Join(" ", eventDecl.Modifiers.Select(m => m.Text)),
                    Location = GetLocation(tree, eventDecl),
                    SyntaxNode = eventDecl
                };

                events.Add(eventInfo);
            }

            return events;
        }

        // Find a specific class by name
        public static RoslynClassInfo FindClass(SyntaxTree tree, string className)
        {
            var classes = ExtractClasses(tree);
            return classes.FirstOrDefault(c => c.Name == className);
        }

        // Find a specific method in a class
        public static RoslynMethodInfo FindMethod(RoslynClassInfo classInfo, string methodName)
        {
            return classInfo?.Methods.FirstOrDefault(m => m.Name == methodName);
        }

        // Extract all interfaces from a syntax tree
        public static List<RoslynInterfaceInfo> ExtractInterfaces(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            var interfaces = new List<RoslynInterfaceInfo>();

            var interfaceDeclarations = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();

            foreach (var interfaceDecl in interfaceDeclarations) {
                var interfaceInfo = new RoslynInterfaceInfo {
                    Name = interfaceDecl.Identifier.Text,
                    Modifiers = string.Join(" ", interfaceDecl.Modifiers.Select(m => m.Text)),
                    Location = GetLocation(tree, interfaceDecl),
                    SyntaxNode = interfaceDecl
                };

                // Extract nested type information
                BuildNestedTypeInfo(interfaceDecl, out bool isNested, out string parentTypeName, out string fullTypeName);
                interfaceInfo.IsNested = isNested;
                interfaceInfo.ParentTypeName = parentTypeName;
                interfaceInfo.FullTypeName = fullTypeName;

                // Extract namespace
                var namespaceDecl = interfaceDecl.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                if (namespaceDecl != null) {
                    interfaceInfo.Namespace = namespaceDecl.Name.ToString();
                }
                else {
                    var fileScopedNamespace = interfaceDecl.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
                    if (fileScopedNamespace != null) {
                        interfaceInfo.Namespace = fileScopedNamespace.Name.ToString();
                    }
                }

                // Extract base interfaces
                if (interfaceDecl.BaseList != null) {
                    foreach (var baseType in interfaceDecl.BaseList.Types) {
                        interfaceInfo.BaseInterfaces.Add(baseType.Type.ToString());
                    }
                }

                // Extract methods (interface methods don't have bodies)
                interfaceInfo.Methods = ExtractMethods(tree, interfaceDecl);

                // Extract properties
                interfaceInfo.Properties = ExtractProperties(tree, interfaceDecl);

                interfaces.Add(interfaceInfo);
            }

            return interfaces;
        }

        // Extract all structs from a syntax tree
        public static List<RoslynStructInfo> ExtractStructs(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            var structs = new List<RoslynStructInfo>();

            var structDeclarations = root.DescendantNodes().OfType<StructDeclarationSyntax>();

            foreach (var structDecl in structDeclarations) {
                var structInfo = new RoslynStructInfo {
                    Name = structDecl.Identifier.Text,
                    Modifiers = string.Join(" ", structDecl.Modifiers.Select(m => m.Text)),
                    Location = GetLocation(tree, structDecl),
                    SyntaxNode = structDecl
                };

                // Extract nested type information
                BuildNestedTypeInfo(structDecl, out bool isNested, out string parentTypeName, out string fullTypeName);
                structInfo.IsNested = isNested;
                structInfo.ParentTypeName = parentTypeName;
                structInfo.FullTypeName = fullTypeName;

                // Extract namespace
                var namespaceDecl = structDecl.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                if (namespaceDecl != null) {
                    structInfo.Namespace = namespaceDecl.Name.ToString();
                }
                else {
                    var fileScopedNamespace = structDecl.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
                    if (fileScopedNamespace != null) {
                        structInfo.Namespace = fileScopedNamespace.Name.ToString();
                    }
                }

                // Extract members
                structInfo.Methods = ExtractMethods(tree, structDecl);
                structInfo.Properties = ExtractProperties(tree, structDecl);
                structInfo.Fields = ExtractFields(tree, structDecl);
                structInfo.Constructors = ExtractConstructors(tree, structDecl);

                structs.Add(structInfo);
            }

            return structs;
        }

        // Extract all enums from a syntax tree
        public static List<RoslynEnumInfo> ExtractEnums(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            var enums = new List<RoslynEnumInfo>();

            var enumDeclarations = root.DescendantNodes().OfType<EnumDeclarationSyntax>();

            foreach (var enumDecl in enumDeclarations) {
                var enumInfo = new RoslynEnumInfo {
                    Name = enumDecl.Identifier.Text,
                    Modifiers = string.Join(" ", enumDecl.Modifiers.Select(m => m.Text)),
                    Location = GetLocation(tree, enumDecl),
                    SyntaxNode = enumDecl
                };

                // Extract nested type information
                BuildNestedTypeInfo(enumDecl, out bool isNested, out string parentTypeName, out string fullTypeName);
                enumInfo.IsNested = isNested;
                enumInfo.ParentTypeName = parentTypeName;
                enumInfo.FullTypeName = fullTypeName;

                // Extract namespace
                var namespaceDecl = enumDecl.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                if (namespaceDecl != null) {
                    enumInfo.Namespace = namespaceDecl.Name.ToString();
                }
                else {
                    var fileScopedNamespace = enumDecl.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
                    if (fileScopedNamespace != null) {
                        enumInfo.Namespace = fileScopedNamespace.Name.ToString();
                    }
                }

                // Extract enum members
                foreach (var member in enumDecl.Members) {
                    var memberInfo = new RoslynEnumMemberInfo {
                        Name = member.Identifier.Text,
                        Value = member.EqualsValue?.Value.ToString()
                    };
                    enumInfo.Members.Add(memberInfo);
                }

                enums.Add(enumInfo);
            }

            return enums;
        }

        // Parse entire file and extract all information
        public static RoslynParsedFile ParseFileComplete(string filePath)
        {
            var tree = ParseFile(filePath);
            var root = tree.GetRoot();
            var parsedFile = new RoslynParsedFile(filePath);
            parsedFile.SyntaxTree = tree;

            // Extract usings
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
            foreach (var usingDir in usingDirectives) {
                parsedFile.Usings.Add(usingDir.Name.ToString());
            }

            // Extract namespace
            var namespaceDecl = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            if (namespaceDecl != null) {
                parsedFile.Namespace = namespaceDecl.Name.ToString();
            }
            else {
                var fileScopedNamespace = root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
                if (fileScopedNamespace != null) {
                    parsedFile.Namespace = fileScopedNamespace.Name.ToString();
                }
            }

            // Extract classes
            parsedFile.Classes = ExtractClasses(tree);

            // Extract interfaces
            parsedFile.Interfaces = ExtractInterfaces(tree);

            // Extract structs
            parsedFile.Structs = ExtractStructs(tree);

            // Extract enums
            parsedFile.Enums = ExtractEnums(tree);

            return parsedFile;
        }

        // Get location information from a syntax node
        private static CodeLocation GetLocation(SyntaxTree tree, SyntaxNode node)
        {
            var span = node.Span;
            var lineSpan = tree.GetLineSpan(span);

            return new CodeLocation(
                tree.FilePath,
                lineSpan.StartLinePosition.Line + 1,  // Convert to 1-based
                lineSpan.EndLinePosition.Line + 1,
                lineSpan.StartLinePosition.Character + 1,
                lineSpan.EndLinePosition.Character + 1
            );
        }

        // Build full type name including parent types for nested types
        private static void BuildNestedTypeInfo(TypeDeclarationSyntax typeDecl, out bool isNested, out string parentTypeName, out string fullTypeName)
        {
            var typeNames = new List<string>();
            typeNames.Add(typeDecl.Identifier.Text);

            // Walk up the parent types
            var currentNode = typeDecl.Parent;
            TypeDeclarationSyntax parentType = null;

            while (currentNode != null)
            {
                if (currentNode is ClassDeclarationSyntax classDecl)
                {
                    typeNames.Insert(0, classDecl.Identifier.Text);
                    if (parentType == null) parentType = classDecl;
                }
                else if (currentNode is StructDeclarationSyntax structDecl)
                {
                    typeNames.Insert(0, structDecl.Identifier.Text);
                    if (parentType == null) parentType = structDecl;
                }
                else if (currentNode is InterfaceDeclarationSyntax interfaceDecl)
                {
                    typeNames.Insert(0, interfaceDecl.Identifier.Text);
                    if (parentType == null) parentType = interfaceDecl;
                }
                else if (currentNode is NamespaceDeclarationSyntax || currentNode is FileScopedNamespaceDeclarationSyntax)
                {
                    // Stop at namespace boundary
                    break;
                }

                currentNode = currentNode.Parent;
            }

            isNested = typeNames.Count > 1;
            parentTypeName = parentType?.Identifier.Text;
            fullTypeName = string.Join(".", typeNames);
        }

        // Build full type name including parent types for nested enums
        private static void BuildNestedTypeInfo(EnumDeclarationSyntax enumDecl, out bool isNested, out string parentTypeName, out string fullTypeName)
        {
            var typeNames = new List<string>();
            typeNames.Add(enumDecl.Identifier.Text);

            // Walk up the parent types
            var currentNode = enumDecl.Parent;
            SyntaxNode parentType = null;

            while (currentNode != null)
            {
                if (currentNode is ClassDeclarationSyntax classDecl)
                {
                    typeNames.Insert(0, classDecl.Identifier.Text);
                    if (parentType == null) parentType = classDecl;
                }
                else if (currentNode is StructDeclarationSyntax structDecl)
                {
                    typeNames.Insert(0, structDecl.Identifier.Text);
                    if (parentType == null) parentType = structDecl;
                }
                else if (currentNode is InterfaceDeclarationSyntax interfaceDecl)
                {
                    typeNames.Insert(0, interfaceDecl.Identifier.Text);
                    if (parentType == null) parentType = interfaceDecl;
                }
                else if (currentNode is NamespaceDeclarationSyntax || currentNode is FileScopedNamespaceDeclarationSyntax)
                {
                    // Stop at namespace boundary
                    break;
                }

                currentNode = currentNode.Parent;
            }

            isNested = typeNames.Count > 1;
            parentTypeName = parentType switch
            {
                ClassDeclarationSyntax c => c.Identifier.Text,
                StructDeclarationSyntax s => s.Identifier.Text,
                InterfaceDeclarationSyntax i => i.Identifier.Text,
                _ => null
            };
            fullTypeName = string.Join(".", typeNames);
        }

        // Find all C# files in a directory
        public static List<string> FindCSharpFiles(string directory, bool recursive = true)
        {
            if (!Directory.Exists(directory)) {
                throw new DirectoryNotFoundException($"Directory not found: {directory}");
            }

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.GetFiles(directory, "*.cs", searchOption).ToList();
        }

        // Find file containing a specific class
        public static string FindFileContainingClass(string directory, string className)
        {
            var files = FindCSharpFiles(directory);

            foreach (var file in files) {
                try {
                    var tree = ParseFile(file);
                    var classes = ExtractClasses(tree);
                    if (classes.Any(c => c.Name == className)) {
                        return file;
                    }
                }
                catch {
                    // Skip files that can't be parsed
                    continue;
                }
            }

            return null;
        }
    }
}
