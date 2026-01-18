using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TreeSitterSharp;
using TreeSitterSharp.Cpp;
using CefDotnetApp.AgentCore.Models;

namespace AgentCore.CodeAnalysis
{
    // TreeSitter-based C++ parser
    public class TreeSitterCppParser : ICodeParser
    {
        private readonly CppParser _parser;
        private string _sourceCode;

        public ProgrammingLanguage Language => ProgrammingLanguage.Cpp;

        public TreeSitterCppParser()
        {
            try
            {
                // Create C++ parser using the correct API
                _parser = new CppParser();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create C++ parser: {ex.GetType().Name}: {ex.Message}", ex);
            }
        }

        public ParsedCodeFile Parse(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var code = File.ReadAllText(filePath);
            return ParseText(code, filePath);
        }

        public ParsedCodeFile ParseText(string code, string filePath = null)
        {
            var result = new ParsedCodeFile(filePath ?? "<memory>", ProgrammingLanguage.Cpp);

            try
            {
                // Store source code for text extraction
                _sourceCode = code;

                // Parse the code - convert string to UTF8 bytes
                var codeBytes = System.Text.Encoding.UTF8.GetBytes(code);
                var tree = _parser.Parse(codeBytes.AsSpan());

                result.NativeSyntaxTree = tree;

                // Extract functions and types from the tree
                ExtractSymbols(tree, result);

                return result;
            }
            catch (Exception ex)
            {
                result.HasSyntaxErrors = true;
                result.SyntaxErrors.Add($"Failed to parse C++ code: {ex.Message}");
                return result;
            }
        }

        private void ExtractSymbols(CppSyntaxTree tree, ParsedCodeFile result)
        {
            try
            {
                // Get the root node
                var root = tree.Root;

                // Traverse the tree to find functions and types
                TraverseNode(root, result);
            }
            catch (Exception ex)
            {
                // Log but don't fail - we can still return partial results
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Warning: Failed to extract symbols: {ex.Message}");
            }
        }

        private string _currentNamespace = string.Empty;

        private void TraverseNode(CppSyntaxNode node, ParsedCodeFile result)
        {
            if (node == null)
            {
                return;
            }

            try
            {
                var nodeType = node.NodeType;

                // Check for namespace definition
                if (nodeType == "namespace_definition")
                {
                    var nsIdentifier = FindChildByType(node, "namespace_identifier");
                    if (nsIdentifier != null)
                    {
                        var nsName = GetNodeText(nsIdentifier);
                        var previousNamespace = _currentNamespace;
                        _currentNamespace = string.IsNullOrEmpty(_currentNamespace)
                            ? nsName
                            : $"{_currentNamespace}::{nsName}";

                        // Traverse children within this namespace
                        foreach (var child in node.NamedChildren)
                        {
                            TraverseNode(child, result);
                        }

                        // Restore previous namespace
                        _currentNamespace = previousNamespace;
                        return; // Don't traverse children again
                    }
                }
                // Check for template declarations
                else if (nodeType == "template_declaration")
                {
                    // Extract template parameters
                    var templateParams = ExtractTemplateParameters(node);

                    // Find the actual declaration (function or class)
                    foreach (var child in node.NamedChildren)
                    {
                        var childType = child.NodeType;
                        if (childType == "function_definition")
                        {
                            var funcInfo = ExtractFunctionInfo(child, result.FilePath);
                            if (funcInfo != null)
                            {
                                funcInfo.Namespace = _currentNamespace;
                                funcInfo.TemplateParameters = templateParams;
                                result.Functions.Add(funcInfo);
                            }
                        }
                        else if (childType == "class_specifier" || childType == "struct_specifier")
                        {
                            var typeInfo = ExtractTypeInfo(child, result.FilePath);
                            if (typeInfo != null)
                            {
                                typeInfo.Namespace = _currentNamespace;
                                typeInfo.TemplateParameters = templateParams;
                                result.Types.Add(typeInfo);
                            }
                        }
                    }
                    return; // Don't traverse children again
                }
                // Check for function definitions (including member functions)
                else if (nodeType == "function_definition")
                {
                    var funcInfo = ExtractFunctionInfo(node, result.FilePath);
                    if (funcInfo != null)
                    {
                        funcInfo.Namespace = _currentNamespace;
                        result.Functions.Add(funcInfo);
                    }
                }
                // Check for class/struct/union/enum definitions
                else if (nodeType == "class_specifier" || nodeType == "struct_specifier" ||
                         nodeType == "union_specifier" || nodeType == "enum_specifier")
                {
                    var typeInfo = ExtractTypeInfo(node, result.FilePath);
                    if (typeInfo != null)
                    {
                        typeInfo.Namespace = _currentNamespace;
                        result.Types.Add(typeInfo);
                    }
                }
                // Check for field_declaration which may contain function_definition (member functions)
                else if (nodeType == "field_declaration")
                {
                    // Look for function_definition inside field_declaration
                    var funcDef = FindChildByType(node, "function_definition");
                    if (funcDef != null)
                    {
                        var funcInfo = ExtractFunctionInfo(funcDef, result.FilePath);
                        if (funcInfo != null)
                        {
                            funcInfo.Namespace = _currentNamespace;
                            result.Functions.Add(funcInfo);
                        }
                    }
                }

                // Recursively traverse children
                foreach (var child in node.NamedChildren)
                {
                    TraverseNode(child, result);
                }
            }
            catch
            {
                // Skip problematic nodes
            }
        }

        private FunctionInfo ExtractFunctionInfo(CppSyntaxNode node, string filePath)
        {
            try
            {
                // Find the function declarator to get the name
                var declarator = FindChildByType(node, "function_declarator");
                if (declarator == null)
                {
                    return null;
                }

                // Try to find identifier (for regular functions) or field_identifier (for member functions)
                var identifier = FindChildByType(declarator, "identifier");
                if (identifier == null)
                {
                    identifier = FindChildByType(declarator, "field_identifier");
                }

                if (identifier == null)
                {
                    return null;
                }

                var funcName = GetNodeText(identifier);
                if (string.IsNullOrEmpty(funcName))
                {
                    return null;
                }

                // Extract return type
                var returnType = ExtractReturnType(node);

                // Extract parameters
                var parameters = ExtractParameters(declarator);

                return new FunctionInfo
                {
                    Name = funcName,
                    ReturnType = returnType,
                    Parameters = parameters,
                    Location = new CodeLocation(
                        filePath,
                        (int)node.StartPoint.Row + 1,
                        (int)node.EndPoint.Row + 1,
                        (int)node.StartPoint.Column,
                        (int)node.EndPoint.Column)
                };
            }
            catch
            {
                return null;
            }
        }

        private string ExtractReturnType(CppSyntaxNode functionNode)
        {
            try
            {
                // Look for type nodes (primitive_type, type_identifier, etc.)
                foreach (var child in functionNode.NamedChildren)
                {
                    var nodeType = child.NodeType;
                    if (nodeType == "primitive_type" || nodeType == "type_identifier" ||
                        nodeType == "sized_type_specifier" || nodeType == "class_specifier" ||
                        nodeType == "struct_specifier" || nodeType == "union_specifier" ||
                        nodeType == "enum_specifier" || nodeType == "template_type")
                    {
                        return GetNodeText(child);
                    }
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private List<ParameterInfo> ExtractParameters(CppSyntaxNode declaratorNode)
        {
            var parameters = new List<ParameterInfo>();

            try
            {
                // Find parameter_list
                var paramList = FindChildByType(declaratorNode, "parameter_list");
                if (paramList == null)
                {
                    return parameters;
                }

                // Iterate through parameter_declaration nodes
                foreach (var child in paramList.NamedChildren)
                {
                    if (child.NodeType == "parameter_declaration")
                    {
                        var param = ExtractParameter(child);
                        if (param != null)
                        {
                            parameters.Add(param);
                        }
                    }
                }
            }
            catch
            {
                // Return empty list on error
            }

            return parameters;
        }

        private ParameterInfo ExtractParameter(CppSyntaxNode paramNode)
        {
            try
            {
                string paramType = string.Empty;
                string paramName = string.Empty;

                // Extract type and name from parameter_declaration
                foreach (var child in paramNode.NamedChildren)
                {
                    var nodeType = child.NodeType;

                    // Type nodes
                    if (nodeType == "primitive_type" || nodeType == "type_identifier" ||
                        nodeType == "sized_type_specifier" || nodeType == "class_specifier" ||
                        nodeType == "struct_specifier" || nodeType == "union_specifier" ||
                        nodeType == "enum_specifier" || nodeType == "template_type")
                    {
                        paramType = GetNodeText(child);
                    }
                    // Name nodes
                    else if (nodeType == "identifier")
                    {
                        paramName = GetNodeText(child);
                    }
                    // Pointer declarator
                    else if (nodeType == "pointer_declarator")
                    {
                        var id = FindChildByType(child, "identifier");
                        if (id != null)
                        {
                            paramName = GetNodeText(id);
                            paramType += "*"; // Add pointer indicator
                        }
                    }
                    // Reference declarator
                    else if (nodeType == "reference_declarator")
                    {
                        var id = FindChildByType(child, "identifier");
                        if (id != null)
                        {
                            paramName = GetNodeText(id);
                            paramType += "&"; // Add reference indicator
                        }
                    }
                }

                if (!string.IsNullOrEmpty(paramType))
                {
                    return new ParameterInfo
                    {
                        Type = paramType,
                        Name = paramName
                    };
                }
            }
            catch
            {
                // Return null on error
            }

            return null;
        }

        private List<TemplateParameterInfo> ExtractTemplateParameters(CppSyntaxNode templateNode)
        {
            var templateParams = new List<TemplateParameterInfo>();

            try
            {
                // Find template_parameter_list
                var paramList = FindChildByType(templateNode, "template_parameter_list");
                if (paramList == null)
                {
                    return templateParams;
                }

                // Iterate through template parameters
                foreach (var child in paramList.NamedChildren)
                {
                    var nodeType = child.NodeType;

                    // Type parameter (typename T or class T)
                    if (nodeType == "type_parameter_declaration")
                    {
                        string paramType = "typename"; // default
                        string paramName = string.Empty;
                        string defaultValue = string.Empty;

                        foreach (var subChild in child.NamedChildren)
                        {
                            var subType = subChild.NodeType;
                            if (subType == "type_identifier")
                            {
                                paramName = GetNodeText(subChild);
                            }
                            else if (subType == "type_descriptor")
                            {
                                defaultValue = GetNodeText(subChild);
                            }
                        }

                        // Check if it's "class" or "typename"
                        var text = GetNodeText(child);
                        if (text.StartsWith("class "))
                        {
                            paramType = "class";
                        }

                        templateParams.Add(new TemplateParameterInfo
                        {
                            Type = paramType,
                            Name = paramName,
                            DefaultValue = defaultValue
                        });
                    }
                    // Non-type parameter (int N, size_t Size, etc.)
                    else if (nodeType == "parameter_declaration")
                    {
                        string paramType = string.Empty;
                        string paramName = string.Empty;
                        string defaultValue = string.Empty;

                        foreach (var subChild in child.NamedChildren)
                        {
                            var subType = subChild.NodeType;
                            if (subType == "primitive_type" || subType == "type_identifier")
                            {
                                paramType = GetNodeText(subChild);
                            }
                            else if (subType == "identifier")
                            {
                                paramName = GetNodeText(subChild);
                            }
                            else if (subType == "number_literal")
                            {
                                defaultValue = GetNodeText(subChild);
                            }
                        }

                        templateParams.Add(new TemplateParameterInfo
                        {
                            Type = paramType,
                            Name = paramName,
                            DefaultValue = defaultValue
                        });
                    }
                }
            }
            catch
            {
                // Return empty list on error
            }

            return templateParams;
        }

        private TypeInfo ExtractTypeInfo(CppSyntaxNode node, string filePath)
        {
            try
            {
                var identifier = FindChildByType(node, "type_identifier");
                if (identifier == null)
                {
                    return null;
                }

                var typeName = GetNodeText(identifier);
                if (string.IsNullOrEmpty(typeName))
                {
                    return null;
                }

                // Extract fields from field_declaration_list
                var fields = ExtractFields(node, filePath);

                return new TypeInfo
                {
                    Name = typeName,
                    Fields = fields,
                    Location = new CodeLocation(
                        filePath,
                        (int)node.StartPoint.Row + 1,
                        (int)node.EndPoint.Row + 1,
                        (int)node.StartPoint.Column,
                        (int)node.EndPoint.Column)
                };
            }
            catch
            {
                return null;
            }
        }

        private List<FieldInfo> ExtractFields(CppSyntaxNode classNode, string filePath)
        {
            var fields = new List<FieldInfo>();

            try
            {
                // Find field_declaration_list
                var fieldList = FindChildByType(classNode, "field_declaration_list");
                if (fieldList == null)
                {
                    return fields;
                }

                // Iterate through field_declaration nodes
                foreach (var child in fieldList.NamedChildren)
                {
                    if (child.NodeType == "field_declaration")
                    {
                        // Skip if it's a function definition (member function)
                        var funcDef = FindChildByType(child, "function_definition");
                        if (funcDef != null)
                        {
                            continue; // Skip member functions
                        }

                        var field = ExtractField(child, filePath);
                        if (field != null)
                        {
                            fields.Add(field);
                        }
                    }
                }
            }
            catch
            {
                // Return empty list on error
            }

            return fields;
        }

        private FieldInfo ExtractField(CppSyntaxNode fieldNode, string filePath)
        {
            try
            {
                string fieldType = string.Empty;
                string fieldName = string.Empty;

                // Extract type and name from field_declaration
                foreach (var child in fieldNode.NamedChildren)
                {
                    var nodeType = child.NodeType;

                    // Type nodes
                    if (nodeType == "primitive_type" || nodeType == "type_identifier" ||
                        nodeType == "sized_type_specifier" || nodeType == "class_specifier" ||
                        nodeType == "struct_specifier" || nodeType == "union_specifier" ||
                        nodeType == "enum_specifier" || nodeType == "template_type")
                    {
                        fieldType = GetNodeText(child);
                    }
                    // Name nodes (field_identifier)
                    else if (nodeType == "field_identifier")
                    {
                        fieldName = GetNodeText(child);
                    }
                    // Field declarator (may contain pointer/reference)
                    else if (nodeType == "field_declarator")
                    {
                        var id = FindChildByType(child, "field_identifier");
                        if (id != null)
                        {
                            fieldName = GetNodeText(id);
                        }
                    }
                    // Pointer declarator
                    else if (nodeType == "pointer_declarator")
                    {
                        var id = FindChildByType(child, "field_identifier");
                        if (id != null)
                        {
                            fieldName = GetNodeText(id);
                            fieldType += "*"; // Add pointer indicator
                        }
                    }
                    // Reference declarator
                    else if (nodeType == "reference_declarator")
                    {
                        var id = FindChildByType(child, "field_identifier");
                        if (id != null)
                        {
                            fieldName = GetNodeText(id);
                            fieldType += "&"; // Add reference indicator
                        }
                    }
                }

                if (!string.IsNullOrEmpty(fieldType) && !string.IsNullOrEmpty(fieldName))
                {
                    return new FieldInfo
                    {
                        Type = fieldType,
                        Name = fieldName,
                        Location = new CodeLocation(
                            filePath,
                            (int)fieldNode.StartPoint.Row + 1,
                            (int)fieldNode.EndPoint.Row + 1,
                            (int)fieldNode.StartPoint.Column,
                            (int)fieldNode.EndPoint.Column)
                    };
                }
            }
            catch
            {
                // Return null on error
            }

            return null;
        }

        private CppSyntaxNode FindChildByType(CppSyntaxNode node, string type)
        {
            if (node == null)
            {
                return null;
            }

            foreach (var child in node.NamedChildren)
            {
                if (child.NodeType == type)
                {
                    return child;
                }
            }

            return null;
        }

        private string GetNodeText(CppSyntaxNode node)
        {
            if (node == null || string.IsNullOrEmpty(_sourceCode))
                return string.Empty;

            try
            {
                int start = (int)node.StartByte;
                int end = (int)node.EndByte;

                if (start >= 0 && end <= _sourceCode.Length && start < end)
                {
                    return _sourceCode.Substring(start, end - start);
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public List<FunctionInfo> FindFunctions(ParsedCodeFile parsed, string namePattern = null, bool ignoreCase = true)
        {
            if (parsed == null || parsed.Functions == null)
            {
                return new List<FunctionInfo>();
            }

            var result = new List<FunctionInfo>();

            // Include global functions
            if (string.IsNullOrEmpty(namePattern))
            {
                result.AddRange(parsed.Functions);
            }
            else
            {
                result.AddRange(parsed.Functions.Where(f => MatchesPattern(f.Name, namePattern, ignoreCase)));
            }

            // Include class methods and constructors from types
            if (parsed.Types != null)
            {
                foreach (var type in parsed.Types)
                {
                    if (type.Methods != null)
                    {
                        foreach (var method in type.Methods)
                        {
                            if (string.IsNullOrEmpty(namePattern) || MatchesPattern(method.Name, namePattern, ignoreCase))
                            {
                                result.Add(method);
                            }
                        }
                    }

                    if (type.Constructors != null)
                    {
                        foreach (var constructor in type.Constructors)
                        {
                            if (string.IsNullOrEmpty(namePattern) || MatchesPattern(constructor.Name, namePattern, ignoreCase))
                            {
                                result.Add(constructor);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public List<TypeInfo> FindTypes(ParsedCodeFile parsed, string namePattern = null, bool ignoreCase = true)
        {
            if (parsed == null || parsed.Types == null)
            {
                return new List<TypeInfo>();
            }

            if (string.IsNullOrEmpty(namePattern))
            {
                return parsed.Types;
            }

            return parsed.Types.Where(t => MatchesPattern(t.Name, namePattern, ignoreCase)).ToList();
        }

        public FunctionInfo FindFunction(ParsedCodeFile parsed, string functionName, bool ignoreCase = true)
        {
            if (parsed == null || parsed.Functions == null)
            {
                return null;
            }

            // First, search in global functions
            foreach (var func in parsed.Functions)
            {
                if (MatchesPattern(func.Name, functionName, ignoreCase))
                {
                    return func;
                }
            }

            // If not found, search in class methods and constructors
            if (parsed.Types != null)
            {
                foreach (var type in parsed.Types)
                {
                    if (type.Methods != null)
                    {
                        foreach (var method in type.Methods)
                        {
                            if (MatchesPattern(method.Name, functionName, ignoreCase))
                            {
                                return method;
                            }
                        }
                    }

                    if (type.Constructors != null)
                    {
                        foreach (var constructor in type.Constructors)
                        {
                            if (MatchesPattern(constructor.Name, functionName, ignoreCase))
                            {
                                return constructor;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public TypeInfo FindType(ParsedCodeFile parsed, string typeName, bool ignoreCase = true)
        {
            if (parsed == null || parsed.Types == null)
            {
                return null;
            }

            return parsed.Types.FirstOrDefault(t => MatchesPattern(t.Name, typeName, ignoreCase));
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
        }    }
}
