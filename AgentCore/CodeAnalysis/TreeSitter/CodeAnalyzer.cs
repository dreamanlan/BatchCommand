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
                case "cpp":
                    var declarator = node.GetChildByFieldName("declarator");
                    if (declarator != null)
                    {
                        var identifier = FindIdentifierInDeclarator(declarator, sourceCode);
                        if (identifier != null)
                            return identifier.GetText(sourceCode);
                    }
                    break;

                case "javascript":
                    var nameNode = node.GetChildByFieldName("name");
                    if (nameNode != null)
                        return nameNode.GetText(sourceCode);
                    break;
            }

            return null;
        }

        private string? ExtractClassName(ITreeSitterNode node, string sourceCode, string language)
        {
            switch (language.ToLower())
            {
                case "cpp":
                    var nameNode = node.GetChildByFieldName("name");
                    if (nameNode != null)
                        return nameNode.GetText(sourceCode);
                    break;

                case "javascript":
                    nameNode = node.GetChildByFieldName("name");
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

                case "javascript":
                    return new HashSet<string>
                    {
                        "function_declaration",
                        "function_expression",
                        "arrow_function",
                        "method_definition"
                    };

                default:
                    return new HashSet<string>();
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

                case "javascript":
                    return new HashSet<string> { "class_declaration" };

                default:
                    return new HashSet<string>();
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
