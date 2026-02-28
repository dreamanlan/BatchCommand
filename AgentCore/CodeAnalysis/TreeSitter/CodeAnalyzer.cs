using System;
using System.Collections.Generic;
using System.Linq;
using AgentCore.CodeAnalysis.TreeSitter.Interfaces;

namespace AgentCore.CodeAnalysis.TreeSitter
{
    /// <summary>
    /// Code analyzer for extracting information from TreeSitter syntax trees
    /// </summary>
    public class CodeAnalyzer
    {
        /// <summary>
        /// Extract function information from a syntax tree
        /// </summary>
        public List<TreeSitterFunctionInfo> ExtractFunctions(ITreeSitterTree tree, string sourceCode, string language)
        {
            var functions = new List<TreeSitterFunctionInfo>();
            var functionNodeTypes = GetFunctionNodeTypes(language);

            TraverseTree(tree.RootNode, node =>
            {
                if (functionNodeTypes.Contains(node.Type))
                {
                    var funcInfo = ExtractFunctionInfo(node, sourceCode, language);
                    if (funcInfo != null)
                        functions.Add(funcInfo);
                }
            });

            return functions;
        }

        /// <summary>
        /// Extract class/struct information from a syntax tree
        /// </summary>
        public List<TreeSitterClassInfo> ExtractClasses(ITreeSitterTree tree, string sourceCode, string language)
        {
            var classes = new List<TreeSitterClassInfo>();
            var classNodeTypes = GetClassNodeTypes(language);

            TraverseTree(tree.RootNode, node =>
            {
                if (classNodeTypes.Contains(node.Type))
                {
                    var classInfo = ExtractClassInfo(node, sourceCode, language);
                    if (classInfo != null)
                        classes.Add(classInfo);
                }
            });

            return classes;
        }

        /// <summary>
        /// Find nodes by type
        /// </summary>
        public List<ITreeSitterNode> FindNodesByType(ITreeSitterTree tree, string nodeType)
        {
            var nodes = new List<ITreeSitterNode>();

            TraverseTree(tree.RootNode, node =>
            {
                if (node.Type == nodeType)
                    nodes.Add(node);
            });

            return nodes;
        }

        /// <summary>
        /// Traverse the syntax tree and apply action to each node
        /// </summary>
        public void TraverseTree(ITreeSitterNode node, Action<ITreeSitterNode> action)
        {
            if (node == null)
                return;

            action(node);

            foreach (var child in node.Children)
            {
                TraverseTree(child, action);
            }
        }

        private TreeSitterFunctionInfo? ExtractFunctionInfo(ITreeSitterNode node, string sourceCode, string language)
        {
            try
            {
                var name = ExtractFunctionName(node, sourceCode, language);
                if (string.IsNullOrEmpty(name))
                    return null;

                return new TreeSitterFunctionInfo
                {
                    Name = name,
                    StartLine = node.StartPoint.Row + 1,
                    EndLine = node.EndPoint.Row + 1,
                    StartColumn = node.StartPoint.Column,
                    EndColumn = node.EndPoint.Column,
                    Text = node.GetText(sourceCode)
                };
            }
            catch
            {
                return null;
            }
        }

        private TreeSitterClassInfo? ExtractClassInfo(ITreeSitterNode node, string sourceCode, string language)
        {
            try
            {
                var name = ExtractClassName(node, sourceCode, language);
                if (string.IsNullOrEmpty(name))
                    return null;

                return new TreeSitterClassInfo
                {
                    Name = name,
                    StartLine = node.StartPoint.Row + 1,
                    EndLine = node.EndPoint.Row + 1,
                    StartColumn = node.StartPoint.Column,
                    EndColumn = node.EndPoint.Column,
                    Text = node.GetText(sourceCode)
                };
            }
            catch
            {
                return null;
            }
        }

        private string? ExtractFunctionName(ITreeSitterNode node, string sourceCode, string language)
        {
            switch (language.ToLower())
            {
                case "c":
                    var nameNode = node.GetChildByFieldName("declarator");
                    if (nameNode != null)
                    {
                        var identifier = FindIdentifierInDeclarator(nameNode, sourceCode);
                        if (identifier != null)
                            return identifier.GetText(sourceCode);
                    }
                    break;

                case "cpp":
                    nameNode = node.GetChildByFieldName("declarator");
                    if (nameNode != null)
                    {
                        var identifier = FindIdentifierInDeclarator(nameNode, sourceCode);
                        if (identifier != null)
                            return identifier.GetText(sourceCode);
                    }
                    break;

                case "python":
                case "ruby":
                case "javascript":
                case "typescript":
                case "tsx":
                case "java":
                case "go":
                case "rust":
                case "swift":
                case "scala":
                case "php":
                case "c-sharp":
                case "haskell":
                case "julia":
                case "bash":
                    var fnName = node.GetChildByFieldName("name");
                    if (fnName != null)
                        return fnName.GetText(sourceCode);
                    break;
            }

            return null;
        }

        private string? ExtractClassName(ITreeSitterNode node, string sourceCode, string language)
        {
            switch (language.ToLower())
            {
                case "cpp":
                case "c-sharp":
                case "java":
                case "python":
                case "ruby":
                case "javascript":
                case "typescript":
                case "tsx":
                case "go":
                case "rust":
                case "swift":
                case "scala":
                case "php":
                case "haskell":
                case "julia":
                    var nameNode = node.GetChildByFieldName("name");
                    if (nameNode != null)
                        return nameNode.GetText(sourceCode);
                    break;
            }

            return null;
        }

        private ITreeSitterNode? FindIdentifierInDeclarator(ITreeSitterNode declarator, string sourceCode)
        {
            if (declarator.Type == "identifier")
                return declarator;

            foreach (var child in declarator.Children)
            {
                if (child.Type == "identifier")
                    return child;

                var result = FindIdentifierInDeclarator(child, sourceCode);
                if (result != null)
                    return result;
            }

            return null;
        }

        private HashSet<string> GetFunctionNodeTypes(string language)
        {
            switch (language.ToLower())
            {
                case "c":
                    return new HashSet<string> { "function_definition" };
                case "cpp":
                    return new HashSet<string> { "function_definition" };
                case "python":
                    return new HashSet<string> { "function_definition" };
                case "javascript":
                    return new HashSet<string>
                    {
                        "function_declaration",
                        "function_expression",
                        "arrow_function",
                        "method_definition"
                    };
                case "typescript":
                case "tsx":
                    return new HashSet<string>
                    {
                        "function_declaration",
                        "function_expression",
                        "arrow_function",
                        "method_definition"
                    };
                case "java":
                    return new HashSet<string> { "method_declaration", "constructor_declaration" };
                case "c-sharp":
                    return new HashSet<string> { "method_declaration", "constructor_declaration" };
                case "go":
                    return new HashSet<string> { "function_declaration", "method_declaration" };
                case "rust":
                    return new HashSet<string> { "function_item" };
                case "ruby":
                    return new HashSet<string> { "method", "singleton_method" };
                case "php":
                    return new HashSet<string> { "function_definition", "method_declaration" };
                case "swift":
                    return new HashSet<string> { "function_declaration" };
                case "scala":
                    return new HashSet<string> { "function_definition", "function_declaration" };
                case "bash":
                    return new HashSet<string> { "function_definition" };
                case "haskell":
                    return new HashSet<string> { "function" };
                case "julia":
                    return new HashSet<string> { "function_definition", "short_function_definition" };
                default:
                    // Generic fallback for unknown languages
                    return new HashSet<string>
                    {
                        "function_definition",
                        "function_declaration",
                        "method_declaration",
                        "method_definition"
                    };
            }
        }

        private HashSet<string> GetClassNodeTypes(string language)
        {
            switch (language.ToLower())
            {
                case "c":
                    return new HashSet<string> { "struct_specifier" };
                case "cpp":
                    return new HashSet<string> { "class_specifier", "struct_specifier" };
                case "python":
                    return new HashSet<string> { "class_definition" };
                case "javascript":
                    return new HashSet<string> { "class_declaration" };
                case "typescript":
                case "tsx":
                    return new HashSet<string> { "class_declaration", "interface_declaration" };
                case "java":
                    return new HashSet<string> { "class_declaration", "interface_declaration", "enum_declaration" };
                case "c-sharp":
                    return new HashSet<string> { "class_declaration", "interface_declaration", "struct_declaration", "enum_declaration" };
                case "go":
                    return new HashSet<string> { "type_declaration" };
                case "rust":
                    return new HashSet<string> { "struct_item", "enum_item", "impl_item", "trait_item" };
                case "ruby":
                    return new HashSet<string> { "class", "module" };
                case "php":
                    return new HashSet<string> { "class_declaration", "interface_declaration" };
                case "swift":
                    return new HashSet<string> { "class_declaration", "struct_declaration", "protocol_declaration" };
                case "scala":
                    return new HashSet<string> { "class_definition", "object_definition", "trait_definition" };
                case "haskell":
                    return new HashSet<string> { "data_declaration", "newtype_declaration", "type_class_declaration" };
                case "julia":
                    return new HashSet<string> { "struct_definition", "abstract_definition" };
                default:
                    // Generic fallback for unknown languages
                    return new HashSet<string>
                    {
                        "class_definition",
                        "class_declaration",
                        "struct_specifier",
                        "interface_declaration"
                    };
            }
        }
    }

    /// <summary>
    /// Function information extracted from syntax tree
    /// </summary>
    public class TreeSitterFunctionInfo
    {
        public string Name { get; set; } = string.Empty;
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// Class information extracted from syntax tree
    /// </summary>
    public class TreeSitterClassInfo
    {
        public string Name { get; set; } = string.Empty;
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
