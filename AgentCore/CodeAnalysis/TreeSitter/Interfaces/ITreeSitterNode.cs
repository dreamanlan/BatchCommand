using System.Collections.Generic;

namespace AgentCore.CodeAnalysis.TreeSitter.Interfaces
{
    /// <summary>
    /// Unified interface for TreeSitter syntax nodes across different language parsers
    /// </summary>
    public interface ITreeSitterNode
    {
        /// <summary>
        /// Gets the type of this syntax node (e.g., "function_declaration", "class_specifier")
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the number of child nodes
        /// </summary>
        int ChildCount { get; }

        /// <summary>
        /// Gets a child node by index
        /// </summary>
        /// <param name="index">Zero-based index of the child</param>
        /// <returns>The child node, or null if index is out of range</returns>
        ITreeSitterNode? GetChild(int index);

        /// <summary>
        /// Gets the parent node
        /// </summary>
        ITreeSitterNode? Parent { get; }

        /// <summary>
        /// Gets the start byte offset in the source code
        /// </summary>
        int StartByte { get; }

        /// <summary>
        /// Gets the end byte offset in the source code
        /// </summary>
        int EndByte { get; }

        /// <summary>
        /// Gets the start position (row, column) in the source code
        /// </summary>
        (int Row, int Column) StartPoint { get; }

        /// <summary>
        /// Gets the end position (row, column) in the source code
        /// </summary>
        (int Row, int Column) EndPoint { get; }

        /// <summary>
        /// Gets the text content of this node from the source code
        /// </summary>
        /// <param name="sourceCode">The source code string</param>
        /// <returns>The text content of this node</returns>
        string GetText(string sourceCode);

        /// <summary>
        /// Gets all child nodes as an enumerable
        /// </summary>
        IEnumerable<ITreeSitterNode> Children { get; }

        /// <summary>
        /// Gets a child node by field name (e.g., "name", "parameters")
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <returns>The child node, or null if not found</returns>
        ITreeSitterNode? GetChildByFieldName(string fieldName);

        /// <summary>
        /// Gets whether this is a named node (not anonymous like punctuation)
        /// </summary>
        bool IsNamed { get; }

        /// <summary>
        /// Gets the native node object from the underlying parser
        /// This allows access to parser-specific features
        /// </summary>
        object NativeNode { get; }
    }
}
