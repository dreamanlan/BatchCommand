using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AgentCore.Core
{
    public enum PlanStateEnum
    {
        Planned = 0,
        InProgress,
        Done
    }
    public enum PlayItemStateEnum
    {
        Planned = 0,
        InProgress,
        Abandoned,
        Done
    }
    /// <summary>
    /// In-memory plan tracking state for one agent.
    /// Plan state: 0=planned 1=in progress 2=done.
    /// Item state: 0=planned 1=in progress 2=abandoned 3=done.
    /// Stored in AgentInstance.PlanTrack.
    /// </summary>
    public sealed class PlanTrackState
    {
        public string Title = string.Empty;
        public int PlanState;
        public readonly List<string> Notes = new List<string>();
        public readonly List<PlanItem> Items = new List<PlanItem>();

        public sealed class PlanItem
        {
            public int State;
            public string Content = string.Empty;
            public readonly List<string> Notes = new List<string>();
        }
    }

    /// <summary>
    /// State derivation and plan.txt text generation/parsing for PlanTrack.
    /// Format (parsed line by line with regexes):
    /// # [PLAN] state=1 title
    /// +[0] plan-level note text
    /// -[0] state=0 item content
    ///     [0] 2026-10-13 12:00 record text
    /// =continuation text (one leading '=' is added on save and removed on read)
    /// </summary>
    internal static class PlanTrackHelper
    {
        private static readonly Regex PlanLineRegex = new Regex(
            @"^#\s*\[PLAN\]\s*state=(\d+)\s*(.*)$", RegexOptions.Compiled);
        private static readonly Regex ItemLineRegex = new Regex(
            @"^-\[(\d+)\]\s*state=(\d+)\s+(.*)$", RegexOptions.Compiled);
        private static readonly Regex NoteLineRegex = new Regex(
            @"^\+\[(\d+)\]\s+(.*)$", RegexOptions.Compiled);
        private static readonly Regex RecordLineRegex = new Regex(
            @"^\s+\[(\d+)\]\s+(.*)$", RegexOptions.Compiled);

        internal static int DerivePlanState(int current, List<PlanTrackState.PlanItem> items)
        {
            if (items == null || items.Count == 0)
                return current;
            bool anyInProgress = false;
            bool allClosed = true;
            bool allPlanned = true;
            foreach (var it in items) {
                if (it.State == (int)PlayItemStateEnum.InProgress)
                    anyInProgress = true;
                if (it.State != (int)PlayItemStateEnum.Abandoned && it.State != (int)PlayItemStateEnum.Done)
                    allClosed = false;
                if (it.State != (int)PlayItemStateEnum.Planned)
                    allPlanned = false;
            }
            if (anyInProgress)
                return (int)PlanStateEnum.InProgress;
            if (allClosed)
                return (int)PlanStateEnum.Done;
            if (allPlanned)
                return (int)PlanStateEnum.Planned;
            return (int)PlanStateEnum.InProgress;
        }

        private static string EncodeContinuationLines(string text)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n")
                .Replace("\n", Environment.NewLine + "=");
        }

        internal static string GeneratePlanText(PlanTrackState st)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("# [PLAN] state=").Append(st.PlanState).Append(' ');
            sb.AppendLine(EncodeContinuationLines(st.Title.Length > 0 ? st.Title : "(untitled)"));
            for (int n = 0; n < st.Notes.Count; n++)
                sb.Append("+[").Append(n).Append("] ").AppendLine(EncodeContinuationLines(st.Notes[n]));
            for (int i = 0; i < st.Items.Count; i++) {
                var it = st.Items[i];
                sb.Append("-[").Append(i).Append("] state=").Append(it.State).Append(' ');
                sb.AppendLine(EncodeContinuationLines(it.Content));
                for (int r = 0; r < it.Notes.Count; r++)
                    sb.Append("    [").Append(r).Append("] ").AppendLine(EncodeContinuationLines(it.Notes[r]));
            }
            return sb.ToString();
        }

        // PersistPlan: regenerate plan text into AgentInstance.Plan and write
        // docs/plan.txt under inst.ProjectDir (UTF-8 no BOM, temp file + replace).
        // Does NOT derive the plan state; callers derive when needed (align_plan).
        internal static string PersistPlan(AgentInstance inst)
        {
            if (inst.PlanTrack == null)
                return string.Empty;
            string text = GeneratePlanText(inst.PlanTrack);
            inst.Plan = text;
            if (!string.IsNullOrEmpty(inst.ProjectDir)) {
                try {
                    string dir = System.IO.Path.Combine(inst.ProjectDir, "docs");
                    if (!System.IO.Directory.Exists(dir))
                        System.IO.Directory.CreateDirectory(dir);
                    string path = System.IO.Path.Combine(dir, "plan.txt");
                    string tmp = path + ".tmp";
                    System.IO.File.WriteAllText(tmp, text, new System.Text.UTF8Encoding(false));
                    if (System.IO.File.Exists(path))
                        System.IO.File.Replace(tmp, path, null);
                    else
                        System.IO.File.Move(tmp, path);
                }
                catch (Exception ex) {
                    MetaDslExecutor.AppendApiErrorInfoLine("plan.txt persist failed: " + ex.Message);
                }
            }
            return text;
        }

        // Returns null when no [PLAN] line is found.
        internal static PlanTrackState? ParsePlanText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            PlanTrackState? st = null;
            PlanTrackState.PlanItem? currentItem = null;
            List<string>? currentNotes = null;
            string? line;
            using (var sr = new System.IO.StringReader(text)) {
                while ((line = sr.ReadLine()) != null) {
                    // Decode explicit continuations before matching any structure.
                    if (line.StartsWith("=", StringComparison.Ordinal)) {
                        if (st == null)
                            continue;
                        line = line.Substring(1);
                        if (currentNotes != null)
                            currentNotes[currentNotes.Count - 1] += Environment.NewLine + line;
                        else if (currentItem != null)
                            currentItem.Content += Environment.NewLine + line;
                        else
                            st.Title += Environment.NewLine + line;
                        continue;
                    }
                    var m = PlanLineRegex.Match(line);
                    if (m.Success) {
                        st = new PlanTrackState();
                        currentItem = null;
                        currentNotes = null;
                        int ps;
                        if (!int.TryParse(m.Groups[1].Value, out ps))
                            ps = (int)PlanStateEnum.Planned;
                        st.PlanState = ps;
                        st.Title = m.Groups[2].Value.Trim();
                        continue;
                    }
                    if (st == null)
                        continue;
                    var nm = NoteLineRegex.Match(line);
                    if (nm.Success) {
                        st.Notes.Add(nm.Groups[2].Value.Trim());
                        currentItem = null;
                        currentNotes = st.Notes;
                        continue;
                    }
                    var im = ItemLineRegex.Match(line);
                    if (im.Success) {
                        int its;
                        if (!int.TryParse(im.Groups[2].Value, out its))
                            its = (int)PlayItemStateEnum.Planned;
                        currentItem = new PlanTrackState.PlanItem {
                            State = its,
                            Content = im.Groups[3].Value.Trim()
                        };
                        st.Items.Add(currentItem);
                        currentNotes = null;
                        continue;
                    }
                    var rm = RecordLineRegex.Match(line);
                    if (rm.Success && st.Items.Count > 0) {
                        currentItem = st.Items[st.Items.Count - 1];
                        currentNotes = currentItem.Notes;
                        currentNotes.Add(rm.Groups[2].Value.Trim());
                        continue;
                    }
                    // Unmarked lines continue the last recognized record verbatim.
                    if (currentNotes != null)
                        currentNotes[currentNotes.Count - 1] += Environment.NewLine + line;
                    else if (currentItem != null)
                        currentItem.Content += Environment.NewLine + line;
                    else
                        st.Title += Environment.NewLine + line;
                }
            }
            return st;
        }
    }
}
