using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TreeSitterSharp;
using TreeSitterSharp.C;
using CefDotnetApp.AgentCore.Core;
using CefDotnetApp.AgentCore.Models;

namespace AgentCore.CodeAnalysis
{
    // TreeSitter-based C parser
    public class TreeSitterCParser : ICodeParser
    {
        private readonly CParser _parser;
        private string _sourceCode;

        public ProgrammingLanguage Language => ProgrammingLanguage.C;

        public TreeSitterCParser()
        {
            try
            {
                // Create C parser using the correct API
                _parser = new CParser();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create C parser: {ex.GetType().Name}: {ex.Message}", ex);
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
            var result = new ParsedCodeFile(filePath ?? "<memory>", ProgrammingLanguage.C);

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
                result.SyntaxErrors.Add($"Failed to parse C code: {ex.Message}");
                return result;
            }
        }

        private void ExtractSymbols(CSyntaxTree tree, ParsedCodeFile result)
        {
            // Get the root node and traverse the tree to find functions and types
            var root = tree.Root;
            TraverseNode(root, result);
        }

        private void TraverseNode(CSyntaxNode node, ParsedCodeFile result)
        {
            if (node == null)
            {
                return;
            }

            var nodeType = node.NodeType;

            // Check for function definitions
            if (nodeType == "function_definition")
            {
                var funcInfo = ExtractFunctionInfo(node, result.FilePath);
                if (funcInfo != null)
                {
                    result.Functions.Add(funcInfo);
                }
            }
            // Check for struct/union/enum definitions
            else if (nodeType == "struct_specifier" || nodeType == "union_specifier" || nodeType == "enum_specifier")
            {
                var typeInfo = ExtractTypeInfo(node, result.FilePath);
                if (typeInfo != null)
                {
                    result.Types.Add(typeInfo);
                }
            }

            // Recursively traverse children
            foreach (var child in node.NamedChildren)
            {
                TraverseNode(child, result);
            }
        }

        private FunctionInfo ExtractFunctionInfo(CSyntaxNode node, string filePath)
        {
            try
            {
                // Find the function declarator to get the name
                var declarator = FindChildByType(node, "function_declarator");
                if (declarator == null)
                {
                    return null;
                }

                var identifier = FindChildByType(declarator, "identifier");
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

        private string ExtractReturnType(CSyntaxNode functionNode)
        {
            try
            {
                // Look for type nodes (primitive_type, type_identifier, etc.)
                foreach (var child in functionNode.NamedChildren)
                {
                    var nodeType = child.NodeType;
                    if (nodeType == "primitive_type" || nodeType == "type_identifier" ||
                        nodeType == "sized_type_specifier" || nodeType == "struct_specifier" ||
                        nodeType == "union_specifier" || nodeType == "enum_specifier")
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

        private List<ParameterInfo> ExtractParameters(CSyntaxNode declaratorNode)
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

        private ParameterInfo ExtractParameter(CSyntaxNode paramNode)
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
                        nodeType == "sized_type_specifier" || nodeType == "struct_specifier" ||
                        nodeType == "union_specifier" || nodeType == "enum_specifier")
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

        private TypeInfo ExtractTypeInfo(CSyntaxNode node, string filePath)
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

        private List<FieldInfo> ExtractFields(CSyntaxNode structNode, string filePath)
        {
            var fields = new List<FieldInfo>();

            try
            {
                // Find field_declaration_list
                var fieldList = FindChildByType(structNode, "field_declaration_list");
                if (fieldList == null)
                {
                    return fields;
                }

                // Iterate through field_declaration nodes
                foreach (var child in fieldList.NamedChildren)
                {
                    if (child.NodeType == "field_declaration")
                    {
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

        private FieldInfo ExtractField(CSyntaxNode fieldNode, string filePath)
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
                        nodeType == "sized_type_specifier" || nodeType == "struct_specifier" ||
                        nodeType == "union_specifier" || nodeType == "enum_specifier")
                    {
                        fieldType = GetNodeText(child);
                    }
                    // Name nodes (field_identifier)
                    else if (nodeType == "field_identifier")
                    {
                        fieldName = GetNodeText(child);
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

        private CSyntaxNode FindChildByType(CSyntaxNode node, string type)
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

        private string GetNodeText(CSyntaxNode node)
        {
            if (node == null || string.IsNullOrEmpty(_sourceCode))
            {
                return string.Empty;
            }

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
            return new List<FunctionInfo>();
        }

        public List<TypeInfo> FindTypes(ParsedCodeFile parsed, string namePattern = null, bool ignoreCase = true)
        {
            return new List<TypeInfo>();
        }

        public FunctionInfo FindFunction(ParsedCodeFile parsed, string functionName, bool ignoreCase = true)
        {
            return null;
        }

        public TypeInfo FindType(ParsedCodeFile parsed, string typeName, bool ignoreCase = true)
        {
            return null;
        }
    }
}
