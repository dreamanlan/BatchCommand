namespace AgentCore.CodeAnalysis.TreeSitter.Interfaces
{
    /// <summary>
    /// Unified interface for TreeSitter syntax trees across different language parsers
    /// </summary>
    public interface ITreeSitterTree
    {
        /// <summary>
        /// Gets the root node of the syntax tree
        /// </summary>
        ITreeSitterNode RootNode { get; }

        /// <summary>
        /// Gets the native tree object from the underlying parser
        /// This allows access to parser-specific features
        /// </summary>
        object NativeTree { get; }
    }
}
