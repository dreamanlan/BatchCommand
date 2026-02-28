using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AgentCore.CodeAnalysis.TreeSitter.Adapters;
using AgentCore.CodeAnalysis.TreeSitter.Interfaces;
using CefDotnetApp.AgentCore.Models;

namespace AgentCore.CodeAnalysis
{
    // Generic TreeSitter-based parser for multiple programming languages.
    // Uses LanguageProfile to adapt AST node types for different languages.
    public class GenericTreeSitterParser : ICodeParser
    {
        private readonly DotNetParserAdapter _parser;
        private readonly LanguageProfile _profile;

        public ProgrammingLanguage Language => _profile.Language;

        public GenericTreeSitterParser(LanguageProfile profile)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            try
            {
                _parser = new DotNetParserAdapter(profile.LanguageId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create parser for language '{profile.LanguageId}': {ex.Message}", ex);
            }
        }

        public ParsedCodeFile Parse(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var code = File.ReadAllText(filePath);
            return ParseText(code, filePath);
        }

        public ParsedCodeFile ParseText(string code, string? filePath = null)
        {
            var result = new ParsedCodeFile(filePath ?? "<memory>", _profile.Language);

            try
            {
                var tree = _parser.Parse(code);
                result.NativeSyntaxTree = tree;
                TraverseNode(tree.RootNode, code, result, null);
                return result;
            }
            catch (Exception ex)
            {
                result.HasSyntaxErrors = true;
                result.SyntaxErrors.Add($"Failed to parse {_profile.LanguageId} code: {ex.Message}");
                return result;
            }
        }

        private void TraverseNode(ITreeSitterNode node, string sourceCode, ParsedCodeFile result, TypeInfo? currentType)
        {
            if (node.Type == null)
                return;

            var nodeType = node.Type;

            // Check for function/method definitions
            if (_profile.FunctionNodeTypes.Contains(nodeType))
            {
                var funcInfo = ExtractFunctionInfo(node, sourceCode, result.FilePath);
                if (funcInfo != null)
                {
                    if (currentType != null)
                        currentType.Methods.Add(funcInfo);
                    else
                        result.Functions.Add(funcInfo);
                }
            }
            // Check for class/type definitions
            else if (_profile.TypeNodeTypes.Contains(nodeType))
            {
                var typeInfo = ExtractTypeInfo(node, sourceCode, result.FilePath);
                if (typeInfo != null)
                {
                    result.Types.Add(typeInfo);
                    // Traverse children within the type context
                    TraverseChildren(node, sourceCode, result, typeInfo);
                    return; // Already traversed children
                }
            }
            // Check for struct definitions
            else if (_profile.StructNodeTypes.Contains(nodeType))
            {
                var structInfo = ExtractTypeInfo(node, sourceCode, result.FilePath);
                if (structInfo != null)
                {
                    structInfo.Type = CodeElementType.Struct;
                    result.Structs.Add(structInfo);
                    TraverseChildren(node, sourceCode, result, structInfo);
                    return;
                }
            }
            // Check for enum definitions
            else if (_profile.EnumNodeTypes.Contains(nodeType))
            {
                var enumInfo = ExtractEnumInfo(node, sourceCode, result.FilePath);
                if (enumInfo != null)
                    result.Enums.Add(enumInfo);
            }
            // Check for import statements
            else if (_profile.ImportNodeTypes.Contains(nodeType))
            {
                var importText = node.GetText(sourceCode);
                if (!string.IsNullOrEmpty(importText))
                    result.Imports.Add(importText.Trim());
            }

            // Recursively traverse children
            TraverseChildren(node, sourceCode, result, currentType);
        }

        private void TraverseChildren(ITreeSitterNode node, string sourceCode, ParsedCodeFile result, TypeInfo? currentType)
        {
            foreach (var child in node.Children)
            {
                if (child.IsNamed)
                    TraverseNode(child, sourceCode, result, currentType);
            }
        }

        private FunctionInfo? ExtractFunctionInfo(ITreeSitterNode node, string sourceCode, string filePath)
        {
            try
            {
                // Most languages use "name" field for function name
                var nameNode = node.GetChildByFieldName("name");
                string funcName;

                if (nameNode != null)
                {
                    funcName = nameNode.GetText(sourceCode);
                }
                else
                {
                    // Fallback: find first identifier child
                    funcName = FindFirstChildText(node, "identifier", sourceCode);
                }

                if (string.IsNullOrEmpty(funcName))
                    return null;

                // Extract return type
                var returnType = ExtractReturnType(node, sourceCode);

                // Extract parameters
                var parameters = ExtractParameters(node, sourceCode);

                // Extract modifiers
                var modifiers = ExtractModifiers(node, sourceCode);

                return new FunctionInfo
                {
                    Name = funcName,
                    ReturnType = returnType,
                    Parameters = parameters,
                    Modifiers = modifiers,
                    FullText = node.GetText(sourceCode),
                    Language = _profile.Language,
                    Location = CreateLocation(node, filePath)
                };
            }
            catch
            {
                return null;
            }
        }

        private TypeInfo? ExtractTypeInfo(ITreeSitterNode node, string sourceCode, string filePath)
        {
            try
            {
                var nameNode = node.GetChildByFieldName("name");
                string typeName;

                if (nameNode != null)
                {
                    typeName = nameNode.GetText(sourceCode);
                }
                else
                {
                    typeName = FindFirstChildText(node, "type_identifier", sourceCode);
                    if (string.IsNullOrEmpty(typeName))
                        typeName = FindFirstChildText(node, "identifier", sourceCode);
                }

                if (string.IsNullOrEmpty(typeName))
                    return null;

                // Extract base types / superclass
                var baseTypes = ExtractBaseTypes(node, sourceCode);

                return new TypeInfo
                {
                    Name = typeName,
                    Type = CodeElementType.Class,
                    BaseTypes = baseTypes,
                    Language = _profile.Language,
                    Location = CreateLocation(node, filePath)
                };
            }
            catch
            {
                return null;
            }
        }

        private EnumInfo? ExtractEnumInfo(ITreeSitterNode node, string sourceCode, string filePath)
        {
            try
            {
                var nameNode = node.GetChildByFieldName("name");
                string enumName;

                if (nameNode != null)
                {
                    enumName = nameNode.GetText(sourceCode);
                }
                else
                {
                    enumName = FindFirstChildText(node, "type_identifier", sourceCode);
                    if (string.IsNullOrEmpty(enumName))
                        enumName = FindFirstChildText(node, "identifier", sourceCode);
                }

                if (string.IsNullOrEmpty(enumName))
                    return null;

                var enumInfo = new EnumInfo
                {
                    Name = enumName,
                    Location = CreateLocation(node, filePath)
                };

                // Try to extract enum members from body
                var bodyNode = node.GetChildByFieldName("body");
                if (bodyNode != null)
                {
                    foreach (var child in bodyNode.Children)
                    {
                        if (child.IsNamed)
                        {
                            var memberName = child.GetChildByFieldName("name");
                            if (memberName != null)
                            {
                                var valueNode = child.GetChildByFieldName("value");
                                enumInfo.Members.Add(new EnumMemberInfo
                                {
                                    Name = memberName.GetText(sourceCode),
                                    Value = valueNode?.GetText(sourceCode)
                                });
                            }
                        }
                    }
                }

                return enumInfo;
            }
            catch
            {
                return null;
            }
        }

        private string ExtractReturnType(ITreeSitterNode node, string sourceCode)
        {
            try
            {
                // Try "return_type" field (used by many languages)
                var returnTypeNode = node.GetChildByFieldName("return_type");
                if (returnTypeNode != null)
                    return returnTypeNode.GetText(sourceCode);

                // Try "type" field (some languages use this)
                var typeNode = node.GetChildByFieldName("type");
                if (typeNode != null)
                    return typeNode.GetText(sourceCode);

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private List<ParameterInfo> ExtractParameters(ITreeSitterNode node, string sourceCode)
        {
            var parameters = new List<ParameterInfo>();
            try
            {
                // Try "parameters" field first
                var paramListNode = node.GetChildByFieldName("parameters");

                // If not found, search children for parameter list node types
                if (paramListNode == null)
                {
                    foreach (var child in node.Children)
                    {
                        if (child.IsNamed && _profile.ParameterListNodeTypes.Contains(child.Type))
                        {
                            paramListNode = child;
                            break;
                        }
                    }
                }

                if (paramListNode == null)
                    return parameters;

                foreach (var child in paramListNode.Children)
                {
                    if (!child.IsNamed)
                        continue;

                    if (_profile.ParameterNodeTypes.Contains(child.Type))
                    {
                        var param = ExtractSingleParameter(child, sourceCode);
                        if (param != null)
                            parameters.Add(param);
                    }
                }
            }
            catch
            {
                // Return whatever we have
            }
            return parameters;
        }

        private ParameterInfo? ExtractSingleParameter(ITreeSitterNode paramNode, string sourceCode)
        {
            try
            {
                string paramName = string.Empty;
                string paramType = string.Empty;

                // Try field-based extraction
                var nameNode = paramNode.GetChildByFieldName("name");
                if (nameNode != null)
                    paramName = nameNode.GetText(sourceCode);

                var typeNode = paramNode.GetChildByFieldName("type");
                if (typeNode != null)
                    paramType = typeNode.GetText(sourceCode);

                // If name not found via field, check if the node itself is an identifier (Python)
                if (string.IsNullOrEmpty(paramName) && paramNode.Type == "identifier")
                {
                    paramName = paramNode.GetText(sourceCode);
                }

                // Fallback: search children for identifiers and type nodes
                if (string.IsNullOrEmpty(paramName))
                {
                    foreach (var child in paramNode.Children)
                    {
                        if (!child.IsNamed) continue;
                        if (child.Type == "identifier" && string.IsNullOrEmpty(paramName))
                            paramName = child.GetText(sourceCode);
                        else if ((child.Type == "type_identifier" || child.Type == "primitive_type" ||
                                  child.Type == "predefined_type" || child.Type == "generic_type") &&
                                 string.IsNullOrEmpty(paramType))
                            paramType = child.GetText(sourceCode);
                    }
                }

                if (!string.IsNullOrEmpty(paramName))
                {
                    return new ParameterInfo
                    {
                        Name = paramName,
                        Type = paramType
                    };
                }
            }
            catch
            {
                // Ignore
            }
            return null;
        }

        private string? ExtractModifiers(ITreeSitterNode node, string sourceCode)
        {
            try
            {
                var modifiers = new List<string>();
                foreach (var child in node.Children)
                {
                    if (child.Type == "modifiers" || child.Type == "modifier" ||
                        child.Type == "visibility_modifier" || child.Type == "access_specifier")
                    {
                        modifiers.Add(child.GetText(sourceCode));
                    }
                }
                return modifiers.Count > 0 ? string.Join(" ", modifiers) : null;
            }
            catch
            {
                return null;
            }
        }

        private List<string> ExtractBaseTypes(ITreeSitterNode node, string sourceCode)
        {
            var baseTypes = new List<string>();
            try
            {
                // Try "superclass" field (Python, Ruby)
                var superNode = node.GetChildByFieldName("superclass");
                if (superNode != null)
                {
                    baseTypes.Add(superNode.GetText(sourceCode));
                    return baseTypes;
                }

                // Search for inheritance-related nodes
                foreach (var child in node.Children)
                {
                    if (!child.IsNamed) continue;
                    if (child.Type == "class_heritage" || child.Type == "superclass" ||
                        child.Type == "super_interfaces" || child.Type == "superinterfaces" ||
                        child.Type == "extends_type_clause" || child.Type == "type_list")
                    {
                        baseTypes.Add(child.GetText(sourceCode));
                    }
                }
            }
            catch
            {
                // Ignore
            }
            return baseTypes;
        }

        private string FindFirstChildText(ITreeSitterNode node, string childType, string sourceCode)
        {
            foreach (var child in node.Children)
            {
                if (child.IsNamed && child.Type == childType)
                    return child.GetText(sourceCode);
            }
            return string.Empty;
        }

        private CodeLocation CreateLocation(ITreeSitterNode node, string filePath)
        {
            return new CodeLocation(
                filePath,
                node.StartPoint.Row + 1,
                node.EndPoint.Row + 1,
                node.StartPoint.Column,
                node.EndPoint.Column);
        }

        // ICodeParser search methods

        public List<FunctionInfo> FindFunctions(ParsedCodeFile parsed, string? namePattern = null, bool ignoreCase = true)
        {
            var allFunctions = new List<FunctionInfo>(parsed.Functions);
            foreach (var type in parsed.Types)
                allFunctions.AddRange(type.Methods);
            foreach (var strct in parsed.Structs)
                allFunctions.AddRange(strct.Methods);

            if (string.IsNullOrEmpty(namePattern))
                return allFunctions;

            return allFunctions.Where(f => MatchesPattern(f.Name, namePattern, ignoreCase)).ToList();
        }

        public List<TypeInfo> FindTypes(ParsedCodeFile parsed, string? namePattern = null, bool ignoreCase = true)
        {
            var allTypes = new List<TypeInfo>(parsed.Types);
            allTypes.AddRange(parsed.Structs);

            if (string.IsNullOrEmpty(namePattern))
                return allTypes;

            return allTypes.Where(t => MatchesPattern(t.Name, namePattern, ignoreCase)).ToList();
        }

        public FunctionInfo? FindFunction(ParsedCodeFile parsed, string functionName, bool ignoreCase = true)
        {
            // Search standalone functions first
            var func = parsed.Functions.FirstOrDefault(f => MatchesPattern(f.Name, functionName, ignoreCase));
            if (func != null) return func;

            // Search type methods
            foreach (var type in parsed.Types)
            {
                func = type.Methods.FirstOrDefault(m => MatchesPattern(m.Name, functionName, ignoreCase));
                if (func != null) return func;
            }
            foreach (var strct in parsed.Structs)
            {
                func = strct.Methods.FirstOrDefault(m => MatchesPattern(m.Name, functionName, ignoreCase));
                if (func != null) return func;
            }

            return null;
        }

        public TypeInfo? FindType(ParsedCodeFile parsed, string typeName, bool ignoreCase = true)
        {
            var type = parsed.Types.FirstOrDefault(t => MatchesPattern(t.Name, typeName, ignoreCase));
            if (type != null) return type;

            return parsed.Structs.FirstOrDefault(s => MatchesPattern(s.Name, typeName, ignoreCase));
        }

        private static bool MatchesPattern(string name, string pattern, bool ignoreCase = true)
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
