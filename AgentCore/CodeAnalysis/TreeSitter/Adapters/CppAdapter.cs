using System;
using System.Collections.Generic;
using System.Reflection;
using AgentCore.CodeAnalysis.TreeSitter.Interfaces;

namespace AgentCore.CodeAnalysis.TreeSitter.Adapters
{
    /// <summary>
    /// Adapter for TreeSitterSharp.Cpp parser
    /// Wraps TreeSitterSharp.SyntaxNode to implement ITreeSitterNode
    /// </summary>
    public class CppNodeWrapper : ITreeSitterNode
    {
        private readonly dynamic _node;
        private readonly string _sourceCode;

        public CppNodeWrapper(dynamic node, string sourceCode)
        {
            _node = node ?? throw new ArgumentNullException(nameof(node));
            _sourceCode = sourceCode ?? string.Empty;
        }

        public string Type => _node.Type;

        public int ChildCount => _node.ChildCount;

        public ITreeSitterNode? GetChild(int index)
        {
            if (index < 0 || index >= ChildCount)
                return null;
            var child = _node.GetChild(index);
            return child != null ? new CppNodeWrapper(child, _sourceCode) : null;
        }

        public ITreeSitterNode? Parent
        {
            get
            {
                var parent = _node.Parent;
                return parent != null ? new CppNodeWrapper(parent, _sourceCode) : null;
            }
        }

        public int StartByte => _node.StartByte;

        public int EndByte => _node.EndByte;

        public (int Row, int Column) StartPoint => (_node.StartPoint.Row, _node.StartPoint.Column);

        public (int Row, int Column) EndPoint => (_node.EndPoint.Row, _node.EndPoint.Column);

        public string GetText(string sourceCode)
        {
            var code = sourceCode ?? _sourceCode;
            if (string.IsNullOrEmpty(code))
                return string.Empty;

            var start = StartByte;
            var end = EndByte;

            if (start < 0 || end > code.Length || start > end)
                return string.Empty;

            return code.Substring(start, end - start);
        }

        public IEnumerable<ITreeSitterNode> Children
        {
            get
            {
                for (int i = 0; i < ChildCount; i++)
                {
                    var child = GetChild(i);
                    if (child != null)
                        yield return child;
                }
            }
        }

        public ITreeSitterNode? GetChildByFieldName(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                return null;
            var child = _node.GetChildByFieldName(fieldName);
            return child != null ? new CppNodeWrapper(child, _sourceCode) : null;
        }

        public bool IsNamed => _node.IsNamed;

        public object NativeNode => _node;
    }

    /// <summary>
    /// Adapter for TreeSitterSharp.Cpp tree
    /// Wraps TreeSitterSharp.Tree to implement ITreeSitterTree
    /// </summary>
    public class CppTreeWrapper : ITreeSitterTree
    {
        private readonly dynamic _tree;
        private readonly string _sourceCode;

        public CppTreeWrapper(dynamic tree, string sourceCode)
        {
            _tree = tree ?? throw new ArgumentNullException(nameof(tree));
            _sourceCode = sourceCode ?? string.Empty;
        }

        public ITreeSitterNode RootNode => new CppNodeWrapper(_tree.RootNode, _sourceCode);

        public object NativeTree => _tree;
    }

    /// <summary>
    /// Adapter for TreeSitterSharp.Cpp parser
    /// Implements ITreeSitterParser for C++ language
    /// </summary>
    public class CppParserAdapter : ITreeSitterParser
    {
        private readonly dynamic _parser;

        public CppParserAdapter()
        {
            // Use TreeSitter base library Parser with dynamic language loading
            try
            {
                // Try to load the C++ language library
                var assembly = Assembly.Load("TreeSitterSharp.Cpp");

                // Search for any type that looks like a language class
                var types = assembly.GetTypes();
                Type? languageType = null;

                foreach (var type in types)
                {
                    // Look for types that might be the language class
                    if (type.Name.Contains("Language") || type.Name.Contains("Cpp"))
                    {
                        languageType = type;
                        break;
                    }
                }

                if (languageType == null)
                {
                    // Fallback: try to use TreeSitter base Parser
                    var parserType = Type.GetType("TreeSitter.Parser, TreeSitter");
                    if (parserType != null)
                    {
                        _parser = Activator.CreateInstance(parserType);
                    }
                    // Note: Without language-specific support, parsing will be limited
                    return;
                }

                // Try to get Instance property
                var instanceProperty = languageType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProperty != null)
                {
                    var language = instanceProperty.GetValue(null);
                    if (language != null)
                    {
                        // Create parser with language
                        var parserType = Type.GetType("TreeSitter.Parser, TreeSitter");
                        if (parserType != null)
                        {
                            _parser = Activator.CreateInstance(parserType);
                            // Try to set language if possible
                            var setLanguageMethod = _parser.GetType().GetMethod("SetLanguage");
                            if (setLanguageMethod != null)
                            {
                                setLanguageMethod.Invoke(_parser, new[] { language });
                            }
                        }
                        return;
                    }
                }



                // Final fallback
                var finalParserType = Type.GetType("TreeSitter.Parser, TreeSitter");
                if (finalParserType != null)
                {
                    _parser = Activator.CreateInstance(finalParserType);
                }
            }
            catch (Exception ex)
            {
                // Extract inner exception if it's a reflection exception
                var innerEx = ex.InnerException ?? ex;
                var detailedMessage = $"Failed to create C++ parser: {ex.GetType().Name}: {ex.Message}";
                if (innerEx != ex)
                {
                    detailedMessage += $" | Inner: {innerEx.GetType().Name}: {innerEx.Message}";
                }
                CefDotnetApp.AgentCore.Core.AgentCore.Instance.Logger.Error($"[CppParserAdapter] {detailedMessage}");
                CefDotnetApp.AgentCore.Core.AgentCore.Instance.Logger.Error($"[CppParserAdapter] Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException(detailedMessage, ex);
            }
        }

        public ITreeSitterTree Parse(string code)
        {
            if (string.IsNullOrEmpty(code))
                code = string.Empty;

            var tree = _parser.Parse(code);
            return new CppTreeWrapper(tree, code);
        }

        public string Language => "cpp";
    }
}
