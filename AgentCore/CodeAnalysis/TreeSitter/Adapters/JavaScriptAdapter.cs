using System;
using System.Collections.Generic;
using System.Linq;
using AgentCore.CodeAnalysis.TreeSitter.Interfaces;

namespace AgentCore.CodeAnalysis.TreeSitter.Adapters
{
    /// <summary>
    /// Adapter for TreeSitter.JavaScript parser
    /// Wraps TreeSitter.Node to implement ITreeSitterNode
    /// WARNING: TreeSitterSharp.JavaScript is not available for Windows platform.
    /// This adapter only provides basic TreeSitter API without JavaScript language support.
    /// Parsing will fail without a language grammar loaded.
    /// </summary>
    public class JavaScriptNodeWrapper : ITreeSitterNode
    {
        private readonly dynamic _node;
        private readonly string _sourceCode;

        public JavaScriptNodeWrapper(dynamic node, string sourceCode)
        {
            _node = node ?? throw new ArgumentNullException(nameof(node));
            _sourceCode = sourceCode ?? string.Empty;
        }

        public string Type => _node.Type;

        public int ChildCount => (int)_node.ChildCount;

        public ITreeSitterNode? GetChild(int index)
        {
            if (index < 0 || index >= ChildCount)
                return null;
            var child = _node.GetChild((uint)index);
            return child != null ? new JavaScriptNodeWrapper(child, _sourceCode) : null;
        }

        public ITreeSitterNode? Parent
        {
            get
            {
                var parent = _node.Parent;
                return parent != null ? new JavaScriptNodeWrapper(parent, _sourceCode) : null;
            }
        }

        public int StartByte => (int)_node.StartByte;

        public int EndByte => (int)_node.EndByte;

        public (int Row, int Column) StartPoint
        {
            get
            {
                var point = _node.StartPoint;
                return ((int)point.Row, (int)point.Column);
            }
        }

        public (int Row, int Column) EndPoint
        {
            get
            {
                var point = _node.EndPoint;
                return ((int)point.Row, (int)point.Column);
            }
        }

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
            return child != null ? new JavaScriptNodeWrapper(child, _sourceCode) : null;
        }

        public bool IsNamed => _node.IsNamed;

        public object NativeNode => _node;
    }

    /// <summary>
    /// Adapter for TreeSitter.JavaScript tree
    /// Wraps TreeSitter.Tree to implement ITreeSitterTree
    /// WARNING: TreeSitterSharp.JavaScript is not available for Windows platform.
    /// </summary>
    public class JavaScriptTreeWrapper : ITreeSitterTree
    {
        private readonly dynamic _tree;
        private readonly string _sourceCode;

        public JavaScriptTreeWrapper(dynamic tree, string sourceCode)
        {
            _tree = tree ?? throw new ArgumentNullException(nameof(tree));
            _sourceCode = sourceCode ?? string.Empty;
        }

        public ITreeSitterNode RootNode
        {
            get
            {
                var root = _tree.RootNode;
                return new JavaScriptNodeWrapper(root, _sourceCode);
            }
        }

        public object NativeTree => _tree;
    }

    /// <summary>
    /// Adapter for TreeSitter.JavaScript parser
    /// Implements ITreeSitterParser for JavaScript language
    /// WARNING: TreeSitterSharp.JavaScript is not available for Windows platform.
    /// This parser will fail when Parse() is called because no JavaScript grammar is loaded.
    /// </summary>
    public class JavaScriptParserAdapter : ITreeSitterParser
    {
        public JavaScriptParserAdapter()
        {
            // NOTE: TreeSitterSharp.JavaScript package does not support Windows platform.
            // Cannot load JavaScript language grammar.
            // Parser object can be created, but Parse() will fail because no language is set.
        }

        public ITreeSitterTree Parse(string code)
        {
            // Cannot parse JavaScript code without language grammar
            throw new NotSupportedException(
                "JavaScript parser is not available on Windows platform. " +
                "TreeSitterSharp.JavaScript package is required but not supported on Windows. " +
                "Basic TreeSitter Node/Tree APIs are available if you have Tree/Node objects from other sources.");
        }

        public string Language => "javascript";
    }
}
