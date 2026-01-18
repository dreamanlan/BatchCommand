namespace AgentCore.CodeAnalysis.TreeSitter.Interfaces
{
    /// <summary>
    /// Unified interface for TreeSitter parsers across different languages
    /// </summary>
    public interface ITreeSitterParser
    {
        /// <summary>
        /// Parses source code and returns a syntax tree
        /// </summary>
        /// <param name="code">The source code to parse</param>
        /// <returns>The parsed syntax tree</returns>
        ITreeSitterTree Parse(string code);

        /// <summary>
        /// Gets the language name this parser supports
        /// </summary>
        string Language { get; }
    }
}
