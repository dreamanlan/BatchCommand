using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AgentCore.Core
{
    public enum BacklogItemStateEnum
    {
        Active = 0,
        Suspended,
        Done
    }
    /// <summary>
    /// In-memory backlog tracking state for one agent.
    /// Item state: 0=active 1=suspended 2=done (done is dropped on align).
    /// At most one item is active at any time.
    /// Stored in AgentInstance.BacklogTrack.
    /// </summary>
    public sealed class BacklogTrackState
    {
        public readonly List<BacklogItem> Items = new List<BacklogItem>();

        public sealed class BacklogItem
        {
            public int State = (int)BacklogItemStateEnum.Suspended;
            public string Title = string.Empty;
            public readonly List<string> Clauses = new List<string>();
        }
    }

    /// <summary>
    /// Text generation/parsing and persistence for BacklogTrack.
    /// Format (same style as plan.txt, parsed line by line with regexes):
    /// -[3] state=0 item title
    ///     [0] clause text
    /// +    continuation text (plus followed by the first line's indentation)
    /// The item index is its position in the in-memory list (always contiguous,
    /// done items are removed from the list immediately). The active item is
    /// written first but keeps its index; other items follow in index order.
    /// Parsing re-inserts the active item at its own index.
    /// Lines matching neither form (and not a continuation) are dropped.
    /// </summary>
    internal static class BacklogTrackHelper
    {
        private const int ClauseIndentation = 4;
        // Soft limit of one clause in UTF-8 bytes (global, warn only); adjustable
        // via set_backlog_clause_max_size.
        internal static int MaxClauseBytes = 600;

        private static readonly Regex ItemLineRegex = new Regex(
            @"^-\[(\d+)\]\s*state=(\d+)\s*(.*)$", RegexOptions.Compiled);
        private static readonly Regex ClauseLineRegex = new Regex(
            @"^([ \t]+)\[(\d+)\]\s*(.*)$", RegexOptions.Compiled);

        internal static bool IsValidState(int state)
        {
            return state >= (int)BacklogItemStateEnum.Active && state <= (int)BacklogItemStateEnum.Done;
        }

        // Returns the index of the active item, or -1 when none.
        internal static int FindActive(BacklogTrackState st)
        {
            for (int i = 0; i < st.Items.Count; i++) {
                if (st.Items[i].State == (int)BacklogItemStateEnum.Active)
                    return i;
            }
            return -1;
        }

        // Make the item at idx active and demote any other active item to suspended.
        internal static void SetActive(BacklogTrackState st, int idx)
        {
            for (int i = 0; i < st.Items.Count; i++) {
                if (i != idx && st.Items[i].State == (int)BacklogItemStateEnum.Active)
                    st.Items[i].State = (int)BacklogItemStateEnum.Suspended;
            }
            st.Items[idx].State = (int)BacklogItemStateEnum.Active;
        }

        // Over-length clauses are still stored; a non-error notice is appended to
        // the api error info so the model sees it wherever the call sits.
        internal static void WarnLongClause(string api, string clause)
        {
            int bytes = System.Text.Encoding.UTF8.GetByteCount(clause);
            if (bytes > MaxClauseBytes) {
                MetaDslExecutor.AppendApiErrorInfoLine(api + " succeeded (notice, not an error, do not retry): clause bytes="
                    + bytes + " > " + MaxClauseBytes
                    + "; a clause should be one short requirement constraint, put progress/evidence into plan notes and shorten it with modify_backlog_clause");
            }
        }

        private static string EncodeContinuationLines(string text, int indentation = 0)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n")
                .Replace("\n", Environment.NewLine + "+" + new string(' ', indentation));
        }

        // Active item first, then the others in index order.
        internal static string GenerateBacklogText(BacklogTrackState st)
        {
            var sb = new System.Text.StringBuilder();
            int activeIdx = FindActive(st);
            if (activeIdx >= 0)
                AppendItemText(sb, st.Items[activeIdx], activeIdx);
            for (int i = 0; i < st.Items.Count; i++) {
                if (i == activeIdx)
                    continue;
                AppendItemText(sb, st.Items[i], i);
            }
            return sb.ToString();
        }

        internal static void AppendItemText(System.Text.StringBuilder sb, BacklogTrackState.BacklogItem it, int idx)
        {
            sb.Append("-[").Append(idx).Append("] state=").Append(it.State).Append(' ');
            sb.AppendLine(EncodeContinuationLines(it.Title));
            for (int n = 0; n < it.Clauses.Count; n++)
                sb.Append(' ', ClauseIndentation).Append('[').Append(n).Append("] ").AppendLine(EncodeContinuationLines(it.Clauses[n], ClauseIndentation));
        }

        // PersistBacklog: regenerate backlog text into AgentInstance.Backlog and write
        // docs/backlog.txt under inst.ProjectDir (UTF-8 no BOM, temp file + replace).
        internal static string PersistBacklog(AgentInstance inst)
        {
            if (inst.BacklogTrack == null)
                return string.Empty;
            string text = GenerateBacklogText(inst.BacklogTrack);
            inst.Backlog = text;
            if (!string.IsNullOrEmpty(inst.ProjectDir)) {
                try {
                    string dir = System.IO.Path.Combine(inst.ProjectDir, "docs");
                    if (!System.IO.Directory.Exists(dir))
                        System.IO.Directory.CreateDirectory(dir);
                    string path = System.IO.Path.Combine(dir, "backlog.txt");
                    string tmp = path + ".tmp";
                    System.IO.File.WriteAllText(tmp, text, new System.Text.UTF8Encoding(false));
                    if (System.IO.File.Exists(path))
                        System.IO.File.Replace(tmp, path, null);
                    else
                        System.IO.File.Move(tmp, path);
                }
                catch (Exception ex) {
                    MetaDslExecutor.AppendApiErrorInfoLine("backlog.txt persist failed: " + ex.Message);
                }
            }
            return text;
        }

        // Always returns a state (possibly empty); unmarked lines and done(2)
        // items are dropped. The single-active rule is enforced: only the first
        // active item stays active. The active item is written first, so it is
        // held back and inserted before the first later item whose serialized
        // index is greater than its own (or appended at the end).
        internal static BacklogTrackState ParseBacklogText(string? text)
        {
            var st = new BacklogTrackState();
            if (string.IsNullOrEmpty(text))
                return st;
            BacklogTrackState.BacklogItem? pendingActive = null;
            int pendingActiveIdx = 0;
            BacklogTrackState.BacklogItem? currentItem = null;
            List<string>? currentClauses = null;
            int currentIndentation = 0;
            bool hasActive = false;
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
                        int idx;
                        if (!int.TryParse(im.Groups[1].Value, out idx))
                            idx = int.MaxValue;
                        int state;
                        if (!int.TryParse(im.Groups[2].Value, out state) || !IsValidState(state))
                            state = (int)BacklogItemStateEnum.Suspended;
                        if (state == (int)BacklogItemStateEnum.Active) {
                            if (hasActive)
                                state = (int)BacklogItemStateEnum.Suspended;
                            hasActive = true;
                        }
                        // Done items are parsed (to absorb their clauses) but not kept.
                        currentItem = new BacklogTrackState.BacklogItem {
                            State = state,
                            Title = im.Groups[3].Value.Trim()
                        };
                        currentClauses = null;
                        currentIndentation = 0;
                        if (state == (int)BacklogItemStateEnum.Done)
                            continue;
                        if (state == (int)BacklogItemStateEnum.Active) {
                            pendingActive = currentItem;
                            pendingActiveIdx = idx;
                            continue;
                        }
                        if (pendingActive != null && idx > pendingActiveIdx) {
                            st.Items.Add(pendingActive);
                            pendingActive = null;
                        }
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
            if (pendingActive != null)
                st.Items.Add(pendingActive);
            return st;
        }
    }
}
