using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AgentCore.Core
{
    /// <summary>
    /// In-memory pattern library state for one agent (title + clauses, no state).
    /// Stored in AgentInstance.PatternTrack.
    /// </summary>
    public sealed class PatternTrackState
    {
        public readonly List<PatternItem> Items = new List<PatternItem>();

        public sealed class PatternItem
        {
            public string Title = string.Empty;
            public readonly List<string> Clauses = new List<string>();
        }
    }

    /// <summary>
    /// Text generation/parsing and persistence for PatternTrack.
    /// Format (same style as backlog.txt without the state field):
    /// -[3] pattern title
    ///     [0] clause text
    /// +    continuation text (plus followed by the first line's indentation)
    /// The item index is its position in the in-memory list (always contiguous).
    /// Items are written and parsed in index order; the serialized index is
    /// informational only. Lines matching neither form (and not a continuation)
    /// are dropped.
    /// </summary>
    internal static class PatternTrackHelper
    {
        // Soft limits (global, warn only); adjustable via set_patterns_max.
        internal static int MaxItems = 64;
        internal static int MaxBytes = 51200;
        private const int ClauseIndentation = 4;

        private static readonly Regex ItemLineRegex = new Regex(
            @"^-\[(\d+)\]\s*(.*)$", RegexOptions.Compiled);
        private static readonly Regex ClauseLineRegex = new Regex(
            @"^([ \t]+)\[(\d+)\]\s*(.*)$", RegexOptions.Compiled);

        private static string EncodeContinuationLines(string text, int indentation = 0)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n")
                .Replace("\n", Environment.NewLine + "+" + new string(' ', indentation));
        }

        internal static string GeneratePatternText(PatternTrackState st)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < st.Items.Count; i++)
                AppendItemText(sb, st.Items[i], i);
            return sb.ToString();
        }

        internal static void AppendItemText(System.Text.StringBuilder sb, PatternTrackState.PatternItem it, int idx)
        {
            sb.Append("-[").Append(idx).Append("] ");
            sb.AppendLine(EncodeContinuationLines(it.Title));
            for (int n = 0; n < it.Clauses.Count; n++)
                sb.Append(' ', ClauseIndentation).Append('[').Append(n).Append("] ").AppendLine(EncodeContinuationLines(it.Clauses[n], ClauseIndentation));
        }

        internal static int GetClauseBytes(PatternTrackState.PatternItem it, int idx)
        {
            var sb = new System.Text.StringBuilder();
            AppendItemText(sb, it, idx);
            return System.Text.Encoding.UTF8.GetByteCount(sb.ToString());
        }

        internal static bool IsOverBudget(PatternTrackState st, string text)
        {
            return System.Text.Encoding.UTF8.GetByteCount(text) > MaxBytes || st.Items.Count > MaxItems;
        }

        // Summary like "total=12345/MaxBytes bytes, items=40/MaxItems", with an over-budget hint.
        internal static string GetStatsText(PatternTrackState st, string text)
        {
            int bytes = System.Text.Encoding.UTF8.GetByteCount(text);
            string s = "total=" + bytes + "/" + MaxBytes + " bytes, items=" + st.Items.Count + "/" + MaxItems;
            if (bytes > MaxBytes || st.Items.Count > MaxItems)
                s += "; over budget, merge or condense patterns";
            return s;
        }

        // PersistPatterns: regenerate pattern text and write docs/patterns.txt
        // under inst.ProjectDir (UTF-8 no BOM, temp file + replace).
        internal static string PersistPatterns(AgentInstance inst)
        {
            if (inst.PatternTrack == null)
                return string.Empty;
            string text = GeneratePatternText(inst.PatternTrack);
            if (!string.IsNullOrEmpty(inst.ProjectDir)) {
                try {
                    string dir = System.IO.Path.Combine(inst.ProjectDir, "docs");
                    if (!System.IO.Directory.Exists(dir))
                        System.IO.Directory.CreateDirectory(dir);
                    string path = System.IO.Path.Combine(dir, "patterns.txt");
                    string tmp = path + ".tmp";
                    System.IO.File.WriteAllText(tmp, text, new System.Text.UTF8Encoding(false));
                    if (System.IO.File.Exists(path))
                        System.IO.File.Replace(tmp, path, null);
                    else
                        System.IO.File.Move(tmp, path);
                }
                catch (Exception ex) {
                    MetaDslExecutor.AppendApiErrorInfoLine("patterns.txt persist failed: " + ex.Message);
                }
            }
            return text;
        }

        // Always returns a state (possibly empty); unmarked lines are dropped.
        // Item/byte limits are soft (warn only) and not enforced here.
        internal static PatternTrackState ParsePatternText(string? text)
        {
            var st = new PatternTrackState();
            if (string.IsNullOrEmpty(text))
                return st;
            PatternTrackState.PatternItem? currentItem = null;
            List<string>? currentClauses = null;
            int currentIndentation = 0;
            string? line;
            using (var sr = new System.IO.StringReader(text)) {
                while ((line = sr.ReadLine()) != null) {
                    if (line.StartsWith("+", StringComparison.Ordinal)) {
                        if (currentItem == null)
                            continue;
                        int offset = 1;
                        while (offset < line.Length && offset <= currentIndentation
                            && (line[offset] == ' ' || line[offset] == '\t'))
                            offset++;
                        line = line.Substring(offset);
                        if (currentClauses != null)
                            currentClauses[currentClauses.Count - 1] += Environment.NewLine + line;
                        else
                            currentItem.Title += Environment.NewLine + line;
                        continue;
                    }
                    var im = ItemLineRegex.Match(line);
                    if (im.Success) {
                        currentItem = new PatternTrackState.PatternItem {
                            Title = im.Groups[2].Value.Trim()
                        };
                        currentClauses = null;
                        currentIndentation = 0;
                        st.Items.Add(currentItem);
                        continue;
                    }
                    if (currentItem == null)
                        continue;
                    var nm = ClauseLineRegex.Match(line);
                    if (nm.Success) {
                        currentClauses = currentItem.Clauses;
                        currentClauses.Add(nm.Groups[3].Value.Trim());
                        currentIndentation = nm.Groups[1].Value.Length;
                        continue;
                    }
                    // Ignore unmarked lines; only '+' introduces a continuation.
                }
            }
            return st;
        }
    }
}
