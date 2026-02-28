using System;
using System.Collections.Generic;
using System.Linq;
using TreeSitter;
using AgentCore.CodeAnalysis.TreeSitter.Interfaces;

namespace AgentCore.CodeAnalysis.TreeSitter.Adapters
{
    /// <summary>
    /// Unified adapter for TreeSitter.DotNet package.
    /// Supports all languages provided by TreeSitter.DotNet through case-insensitive name matching.
    /// </summary>
    public class DotNetNodeWrapper : ITreeSitterNode
    {
        private readonly Node? _node;
        private readonly string _sourceCode;

        public DotNetNodeWrapper(Node? node, string sourceCode)
        {
            _node = node;
            _sourceCode = sourceCode ?? string.Empty;
        }

        /// <summary>
        /// Safely wrap a Node value into ITreeSitterNode.
        /// Returns null if the node is invalid (default struct or native pointer is null).
        /// </summary>
        private ITreeSitterNode? WrapNode(Node? node)
        {
            try
            {
                // For value-type Node, check if Type is accessible and non-null
                // Invalid/default nodes will throw or return null Type
                var type = node?.Type;
                if (type == null)
                    return null;
                return new DotNetNodeWrapper(node, _sourceCode);
            }
            catch
            {
                return null;
            }
        }

        public string Type => _node?.Type ?? string.Empty;

        public int ChildCount => _node?.Children.Count() ?? 0;

        public ITreeSitterNode? GetChild(int index)
        {
            if (index < 0 || _node == null)
                return null;
            try
            {
                var child = _node[(ushort)index];
                return WrapNode(child);
            }
            catch
            {
                return null;
            }
        }

        public ITreeSitterNode? Parent
        {
            get
            {
                if (_node == null)
                    return null;
                try
                {
                    var parent = _node.Parent;
                    return WrapNode(parent);
                }
                catch
                {
                    return null;
                }
            }
        }

        public int StartByte => _node?.StartIndex ?? 0;

        public int EndByte => _node?.EndIndex ?? 0;

        public (int Row, int Column) StartPoint => _node != null ? (_node.StartPosition.Row, _node.StartPosition.Column) : (0, 0);

        public (int Row, int Column) EndPoint => _node != null ? (_node.EndPosition.Row, _node.EndPosition.Column) : (0, 0);

        public string GetText(string sourceCode)
        {
            // Prefer Node.Text which is provided by TreeSitter.DotNet directly
            try
            {
                return _node?.Text ?? string.Empty;
            }
            catch
            {
                // Fallback to manual extraction
                var code = sourceCode ?? _sourceCode;
                if (string.IsNullOrEmpty(code))
                    return string.Empty;
                var start = StartByte;
                var end = EndByte;
                if (start < 0 || end > code.Length || start > end)
                    return string.Empty;
                return code.Substring(start, end - start);
            }
        }

        public IEnumerable<ITreeSitterNode> Children
        {
            get
            {
                if (_node == null)
                    yield break;
                foreach (var child in _node.Children)
                {
                    yield return new DotNetNodeWrapper(child, _sourceCode);
                }
            }
        }

        public ITreeSitterNode? GetChildByFieldName(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName) || _node == null)
                return null;
            try
            {
                var child = _node.GetChildForField(fieldName);
                return WrapNode(child);
            }
            catch
            {
                return null;
            }
        }

        public bool IsNamed => _node?.IsNamed ?? false;

        public object NativeNode => _node!;
    }

    /// <summary>
    /// Tree wrapper for TreeSitter.DotNet
    /// </summary>
    public class DotNetTreeWrapper : ITreeSitterTree
    {
        private readonly Tree _tree;
        private readonly string _sourceCode;

        public DotNetTreeWrapper(Tree tree, string sourceCode)
        {
            _tree = tree ?? throw new ArgumentNullException(nameof(tree));
            _sourceCode = sourceCode ?? string.Empty;
        }

        public ITreeSitterNode RootNode => new DotNetNodeWrapper(_tree.RootNode, _sourceCode);

        public object NativeTree => _tree;
    }

    /// <summary>
    /// Unified parser adapter for TreeSitter.DotNet.
    /// Supports all languages by passing a language name string (case-insensitive).
    /// </summary>
    public class DotNetParserAdapter : ITreeSitterParser
    {
        private readonly Parser _parser;
        private readonly string _language;

        // Map of normalized language names to TreeSitter.DotNet language ids
        private static readonly Dictionary<string, string> s_languageAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Primary names (matching DLL names)
            { "c", "c" },
            { "cpp", "cpp" },
            { "c++", "cpp" },
            { "csharp", "c-sharp" },
            { "c#", "c-sharp" },
            { "c-sharp", "c-sharp" },
            { "css", "css" },
            { "go", "go" },
            { "golang", "go" },
            { "html", "html" },
            { "java", "java" },
            { "javascript", "javascript" },
            { "js", "javascript" },
            { "json", "json" },
            { "python", "python" },
            { "py", "python" },
            { "ruby", "ruby" },
            { "rb", "ruby" },
            { "rust", "rust" },
            { "rs", "rust" },
            { "typescript", "typescript" },
            { "ts", "typescript" },
            { "tsx", "tsx" },
            { "swift", "swift" },
            { "scala", "scala" },
            { "php", "php" },
            { "bash", "bash" },
            { "shell", "bash" },
            { "sh", "bash" },
            { "haskell", "haskell" },
            { "hs", "haskell" },
            { "julia", "julia" },
            { "jl", "julia" },
            { "ocaml", "ocaml" },
            { "agda", "agda" },
            { "toml", "toml" },
            { "verilog", "verilog" },
            { "jsdoc", "jsdoc" },
            { "ql", "ql" },
            { "razor", "razor" },
            { "embedded-template", "embedded-template" },
        };

        /// <summary>
        /// Create a parser for the specified language.
        /// Language name is case-insensitive and supports common aliases.
        /// </summary>
        public DotNetParserAdapter(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
                throw new ArgumentException("Language name cannot be empty", nameof(language));

            // Resolve alias to actual TreeSitter language id
            string langId;
            if (!s_languageAliases.TryGetValue(language.Trim(), out langId!))
            {
                // If no alias found, try using the name directly (lowercase)
                langId = language.Trim().ToLowerInvariant();
            }

            _language = langId;

            try
            {
                var lang = new Language(langId);
                _parser = new Parser(lang);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create TreeSitter parser for language '{language}' (resolved to '{langId}'): {ex.Message}", ex);
            }
        }

        public ITreeSitterTree Parse(string code)
        {
            if (string.IsNullOrEmpty(code))
                code = string.Empty;
            var tree = _parser.Parse(code);
            return new DotNetTreeWrapper(tree!, code);
        }

        public string Language => _language;

        /// <summary>
        /// Get list of all supported language names (including aliases)
        /// </summary>
        public static IReadOnlyCollection<string> SupportedLanguages => s_languageAliases.Keys.ToList().AsReadOnly();

        /// <summary>
        /// Check if a language is supported
        /// </summary>
        public static bool IsLanguageSupported(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
                return false;
            if (s_languageAliases.ContainsKey(language.Trim()))
                return true;
            // Also try creating the language to check if the DLL exists
            try
            {
                var lang = new Language(language.Trim().ToLowerInvariant());
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
