using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TreeSitter;
using CefDotnetApp.AgentCore.Core;
using CefDotnetApp.AgentCore.Models;

namespace AgentCore.CodeAnalysis
{
    // TreeSitter-based C parser
    public class TreeSitterCParser : ICodeParser
    {
        private readonly Parser _parser;
        private string _sourceCode = string.Empty;

        public ProgrammingLanguage Language => ProgrammingLanguage.C;

        public TreeSitterCParser()
        {
            try
            {
                // Create C parser using TreeSitter.DotNet
                _parser = new Parser(new Language("c"));
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

        public ParsedCodeFile ParseText(string code, string? filePath = null)
        {
            var result = new ParsedCodeFile(filePath ?? "<memory>", ProgrammingLanguage.C);

            try
            {
                // Store source code for text extraction
                _sourceCode = code;

                // Parse the code directly as string
                var tree = _parser.Parse(code);
                if (tree == null)
                {
                    result.HasSyntaxErrors = true;
                    result.SyntaxErrors.Add("Failed to parse C code: parser returned null");
                    return result;
                }

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

        private void ExtractSymbols(Tree tree, ParsedCodeFile result)
        {
            // Get the root node and traverse the tree to find functions and types
            var root = tree.RootNode;
            TraverseNode(root, result);
        }

        private void TraverseNode(Node node, ParsedCodeFile result)
        {
            if (node.Type == null)
            {
                return;
            }

            var nodeType = node.Type;

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

        private FunctionInfo? ExtractFunctionInfo(Node node, string filePath)
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
                        node.StartPosition.Row + 1,
                        node.EndPosition.Row + 1,
                        node.StartPosition.Column,
                        node.EndPosition.Column)
                };
            }
            catch
            {
                return null;
            }
        }

        private string ExtractReturnType(Node functionNode)
        {
            try
            {
                foreach (var child in functionNode.NamedChildren)
                {
                    var nodeType = child.Type;
                    if (nodeType == "primitive_type" || nodeType == "type_identifier" ||
                        nodeType == "sized_type_specifier" || nodeType == "struct_specifier" ||
                        nodeType == "union_specifier" || nodeType == "enum_specifier")
                    {
                        return child.Text;
                    }
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private List<ParameterInfo> ExtractParameters(Node declaratorNode)
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
                    if (child.Type == "parameter_declaration")
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

        private ParameterInfo? ExtractParameter(Node paramNode)
        {
            try
            {
                string paramType = string.Empty;
                string paramName = string.Empty;

                foreach (var child in paramNode.NamedChildren)
                {
                    var nodeType = child.Type;

                    // Type nodes
                    if (nodeType == "primitive_type" || nodeType == "type_identifier" ||
                        nodeType == "sized_type_specifier" || nodeType == "struct_specifier" ||
                        nodeType == "union_specifier" || nodeType == "enum_specifier")
                    {
                        paramType = GetNodeText(child);
                    }
                    else if (nodeType == "identifier")
                    {
                        paramName = child.Text;
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

        private TypeInfo? ExtractTypeInfo(Node node, string filePath)
        {
            try
            {
                var identifier = FindChildByType(node, "type_identifier");
                if (identifier == null)
                {
                    return null;
                }

                var typeName = identifier.Text;
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
                        node.StartPosition.Row + 1,
                        node.EndPosition.Row + 1,
                        node.StartPosition.Column,
                        node.EndPosition.Column)
                };
            }
            catch
            {
                return null;
            }
        }

        private List<FieldInfo> ExtractFields(Node structNode, string filePath)
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
                    if (child.Type == "field_declaration")
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

        private FieldInfo? ExtractField(Node fieldNode, string filePath)
        {
            try
            {
                string fieldType = string.Empty;
                string fieldName = string.Empty;

                foreach (var child in fieldNode.NamedChildren)
                {
                    var nodeType = child.Type;

                    // Type nodes
                    if (nodeType == "primitive_type" || nodeType == "type_identifier" ||
                        nodeType == "sized_type_specifier" || nodeType == "struct_specifier" ||
                        nodeType == "union_specifier" || nodeType == "enum_specifier")
                    {
                        fieldType = GetNodeText(child);
                    }
                    else if (nodeType == "field_identifier")
                    {
                        fieldName = child.Text;
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
                            fieldNode.StartPosition.Row + 1,
                            fieldNode.EndPosition.Row + 1,
                            fieldNode.StartPosition.Column,
                            fieldNode.EndPosition.Column)
                    };
                }
            }
            catch
            {
                // Return null on error
            }

            return null;
        }

        private Node? FindChildByType(Node node, string type)
        {
            if (node.Type == null)
            {
                return null;
            }

            foreach (var child in node.NamedChildren)
            {
                if (child.Type == type)
                {
                    return child;
                }
            }

            return null;
        }

        private string GetNodeText(Node node)
        {
            try
            {
                return node.Text;
            }
            catch
            {
                return string.Empty;
            }
        }

        public List<FunctionInfo> FindFunctions(ParsedCodeFile parsed, string? namePattern = null, bool ignoreCase = true)
        {
            if (parsed == null || parsed.Functions == null)
                return new List<FunctionInfo>();

            if (string.IsNullOrEmpty(namePattern))
                return new List<FunctionInfo>(parsed.Functions);

            return parsed.Functions.Where(f => MatchesPattern(f.Name, namePattern, ignoreCase)).ToList();
        }

        public List<TypeInfo> FindTypes(ParsedCodeFile parsed, string? namePattern = null, bool ignoreCase = true)
        {
            if (parsed == null || parsed.Types == null)
                return new List<TypeInfo>();

            if (string.IsNullOrEmpty(namePattern))
                return new List<TypeInfo>(parsed.Types);

            return parsed.Types.Where(t => MatchesPattern(t.Name, namePattern, ignoreCase)).ToList();
        }

        public FunctionInfo? FindFunction(ParsedCodeFile parsed, string functionName, bool ignoreCase = true)
        {
            if (parsed == null || parsed.Functions == null)
                return null;

            return parsed.Functions.FirstOrDefault(f => MatchesPattern(f.Name, functionName, ignoreCase));
        }

        public TypeInfo? FindType(ParsedCodeFile parsed, string typeName, bool ignoreCase = true)
        {
            if (parsed == null || parsed.Types == null)
                return null;

            return parsed.Types.FirstOrDefault(t => MatchesPattern(t.Name, typeName, ignoreCase));
        }

        private static bool MatchesPattern(string name, string pattern, bool ignoreCase = true)
        {
            try
            {
                var options = ignoreCase ? System.Text.RegularExpressions.RegexOptions.IgnoreCase
                                         : System.Text.RegularExpressions.RegexOptions.None;
                return System.Text.RegularExpressions.Regex.IsMatch(name, pattern, options);
            }
            catch (ArgumentException)
            {
                var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                return name.IndexOf(pattern, comparison) >= 0;
            }
        }
    }
}
