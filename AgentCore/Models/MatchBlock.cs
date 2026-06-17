using System;

namespace CefDotnetApp.AgentCore.Models
{
    /// <summary>
    /// Represents a contiguous block of matched lines (with context) from a file search.
    /// Each block carries its source file path so flattened multi-file results can be filtered
    /// or grouped via LINQ such as $$.FilePath, $$.StartLine, $$.MatchedCount.
    /// </summary>
    public class MatchBlock
    {
        public string FilePath { get; set; } = string.Empty;
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public int MatchedCount { get; set; }
        public string Text { get; set; } = string.Empty;

        public MatchBlock()
        {
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
