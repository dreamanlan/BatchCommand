using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AgentCore.Core
{
    public enum MeetingStateEnum
    {
        Planned = 0,
        InProgress,
        Done
    }
    public enum AgendaItemStateEnum
    {
        Planned = 0,
        Discussing,
        Skipped,
        Done
    }
    public enum ActionItemStateEnum
    {
        Open = 0,
        InProgress,
        Done
    }
    /// <summary>
    /// In-memory meeting tracking state for one meeting room (channelId).
    /// Meeting state: 0=planned 1=in progress 2=done.
    /// Agenda item state: 0=planned 1=discussing 2=skipped 3=done.
    /// Action item state: 0=open 1=in progress 2=done.
    /// Stored in AgentInstance.MeetingTracks keyed by channelId (per group
    /// session of the meeting room page, agentId 'aichat').
    /// </summary>
    public sealed class MeetingTrackState
    {
        public string Title = string.Empty;
        public int MeetingState;
        public readonly List<string> Notes = new List<string>();
        public readonly List<AgendaItem> AgendaItems = new List<AgendaItem>();
        public readonly List<ActionItem> ActionItems = new List<ActionItem>();
        public readonly List<MinutesEntry> Minutes = new List<MinutesEntry>();

        public sealed class AgendaItem
        {
            public int State;
            public string Content = string.Empty;
            public readonly List<string> Notes = new List<string>();
        }

        public sealed class ActionItem
        {
            public int State;
            public string Owner = string.Empty;
            public string Task = string.Empty;
            public string Deadline = string.Empty;
            public readonly List<string> Notes = new List<string>();
        }

        public sealed class MinutesEntry
        {
            public string Timestamp = string.Empty;
            public string Text = string.Empty;
        }
    }

    /// <summary>
    /// State derivation and meetings/&lt;channelId&gt; text generation/parsing
    /// for MeetingTrack. Files under ProjectDir/docs/meetings/&lt;channelId&gt;/:
    /// agenda.txt, action_items.txt, minutes.txt (all UTF-8 no BOM,
    /// temp file + replace, same write pattern as PlanTrackHelper.PersistPlan).
    /// agenda.txt format (line by line with regexes):
    /// # [MEETING] state=1 title
    ///     [0] meeting-level note text
    /// -[0] state=0 item content
    ///     [0] 2026-10-13 12:00 note text
    /// +    continuation text (plus followed by the first line's indentation)
    /// action_items.txt format:
    /// # [ACTIONS]
    /// -[0] state=0 owner=someone deadline=2026-11-05 | task text
    ///     [0] note text
    /// minutes.txt format:
    /// # [MINUTES]
    /// [2026-11-03 15:32] minutes text
    /// +    continuation text
    /// </summary>
    internal static class MeetingTrackHelper
    {
        // Fallback project dir for agents that never went through the
        // project config init (the meeting room registers as 'aichat'):
        // meeting files live under this docs tree.
        internal const string DefaultProjectDir = "D:/AiClaw";

        private static readonly Regex MeetingLineRegex = new Regex(
            @"^#\s*\[MEETING\]\s*state=(\d+)\s*(.*)$", RegexOptions.Compiled);
        private static readonly Regex ItemLineRegex = new Regex(
            @"^-\[(\d+)\]\s*state=(\d+)\s+(.*)$", RegexOptions.Compiled);
        private static readonly Regex ActionLineRegex = new Regex(
            @"^-\[(\d+)\]\s*state=(\d+)\s+owner=(.*?)\s+deadline=(\S+)\s*\|\s*(.*)$", RegexOptions.Compiled);
        private static readonly Regex NoteLineRegex = new Regex(
            @"^([ \t]+)\[(\d+)\]\s+(.*)$", RegexOptions.Compiled);
        private static readonly Regex MinutesLineRegex = new Regex(
            @"^\[([^\]]+)\]\s*(.*)$", RegexOptions.Compiled);

        internal static string SanitizeChannelId(string channelId)
        {
            // Keep file-name-safe characters only; everything else becomes '_'.
            var sb = new System.Text.StringBuilder();
            foreach (char c in channelId) {
                sb.Append(char.IsLetterOrDigit(c) || c == '_' || c == '-' ? c : '_');
            }
            string s = sb.ToString();
            if (string.IsNullOrEmpty(s))
                s = "default";
            return s;
        }

        // Meeting docs dir: <ProjectDir or default>/docs/meetings/<channelId>.
        // Does NOT mutate inst.ProjectDir - the fallback is per-call only.
        internal static string GetMeetingDir(AgentInstance inst, string channelId)
        {
            string projectDir = string.IsNullOrEmpty(inst.ProjectDir) ? DefaultProjectDir : inst.ProjectDir;
            return System.IO.Path.Combine(projectDir, "docs", "meetings", SanitizeChannelId(channelId));
        }

        internal static int DeriveMeetingState(int current, List<MeetingTrackState.AgendaItem> items)
        {
            if (items == null || items.Count == 0)
                return current;
            bool anyDiscussing = false;
            bool allClosed = true;
            bool allPlanned = true;
            foreach (var it in items) {
                if (it.State == (int)AgendaItemStateEnum.Discussing)
                    anyDiscussing = true;
                if (it.State != (int)AgendaItemStateEnum.Skipped && it.State != (int)AgendaItemStateEnum.Done)
                    allClosed = false;
                if (it.State != (int)AgendaItemStateEnum.Planned)
                    allPlanned = false;
            }
            if (anyDiscussing)
                return (int)MeetingStateEnum.InProgress;
            if (allClosed)
                return (int)MeetingStateEnum.Done;
            if (allPlanned)
                return (int)MeetingStateEnum.Planned;
            return (int)MeetingStateEnum.InProgress;
        }

        internal static string TimestampNow()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        private static string EncodeContinuationLines(string text, int indentation = 0)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n")
                .Replace("\n", Environment.NewLine + "+" + new string(' ', indentation));
        }

        internal static string GenerateAgendaText(MeetingTrackState st)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("# [MEETING] state=").Append(st.MeetingState).Append(' ');
            sb.AppendLine(EncodeContinuationLines(st.Title.Length > 0 ? st.Title : "(untitled)"));
            for (int n = 0; n < st.Notes.Count; n++)
                sb.Append("    [").Append(n).Append("] ").AppendLine(EncodeContinuationLines(st.Notes[n], 4));
            for (int i = 0; i < st.AgendaItems.Count; i++) {
                var it = st.AgendaItems[i];
                sb.Append("-[").Append(i).Append("] state=").Append(it.State).Append(' ');
                sb.AppendLine(EncodeContinuationLines(it.Content));
                for (int r = 0; r < it.Notes.Count; r++)
                    sb.Append("    [").Append(r).Append("] ").AppendLine(EncodeContinuationLines(it.Notes[r], 4));
            }
            return sb.ToString();
        }

        // Returns null when no [MEETING] line is found.
        internal static MeetingTrackState? ParseAgendaText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            MeetingTrackState? st = null;
            MeetingTrackState.AgendaItem? currentItem = null;
            List<string>? currentNotes = null;
            int currentIndentation = 0;
            string? line;
            using (var sr = new System.IO.StringReader(text)) {
                while ((line = sr.ReadLine()) != null) {
                    // Decode explicit continuations before matching any structure.
                    if (line.StartsWith("+", StringComparison.Ordinal)) {
                        if (st == null)
                            continue;
                        int offset = 1;
                        while (offset < line.Length && offset <= currentIndentation
                            && (line[offset] == ' ' || line[offset] == '\t'))
                            offset++;
                        line = line.Substring(offset);
                        if (currentNotes != null)
                            currentNotes[currentNotes.Count - 1] += Environment.NewLine + line;
                        else if (currentItem != null)
                            currentItem.Content += Environment.NewLine + line;
                        else
                            st.Title += Environment.NewLine + line;
                        continue;
                    }
                    var m = MeetingLineRegex.Match(line);
                    if (m.Success) {
                        st = new MeetingTrackState();
                        currentItem = null;
                        currentNotes = null;
                        currentIndentation = 0;
                        int ms;
                        if (!int.TryParse(m.Groups[1].Value, out ms))
                            ms = (int)MeetingStateEnum.Planned;
                        st.MeetingState = ms;
                        st.Title = m.Groups[2].Value.Trim();
                        continue;
                    }
                    if (st == null)
                        continue;
                    var im = ItemLineRegex.Match(line);
                    if (im.Success) {
                        int state;
                        if (!int.TryParse(im.Groups[2].Value, out state))
                            state = (int)AgendaItemStateEnum.Planned;
                        currentItem = new MeetingTrackState.AgendaItem();
                        currentItem.State = state;
                        currentItem.Content = im.Groups[3].Value.Trim();
                        st.AgendaItems.Add(currentItem);
                        currentNotes = currentItem.Notes;
                        currentIndentation = 0;
                        continue;
                    }
                    var nm = NoteLineRegex.Match(line);
                    if (nm.Success) {
                        currentIndentation = nm.Groups[1].Value.Length;
                        string noteText = nm.Groups[3].Value;
                        if (currentNotes != null)
                            currentNotes.Add(noteText);
                        else
                            st.Notes.Add(noteText);
                    }
                }
            }
            return st;
        }

        internal static string GenerateActionItemsText(MeetingTrackState st)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("# [ACTIONS]");
            for (int i = 0; i < st.ActionItems.Count; i++) {
                var it = st.ActionItems[i];
                sb.Append("-[").Append(i).Append("] state=").Append(it.State)
                    .Append(" owner=").Append(it.Owner.Length > 0 ? it.Owner : "-")
                    .Append(" deadline=").Append(it.Deadline.Length > 0 ? it.Deadline : "-")
                    .Append(" | ").AppendLine(EncodeContinuationLines(it.Task));
                for (int r = 0; r < it.Notes.Count; r++)
                    sb.Append("    [").Append(r).Append("] ").AppendLine(EncodeContinuationLines(it.Notes[r], 4));
            }
            return sb.ToString();
        }

        // Appends parsed action items into st (missing/malformed file = no-op).
        internal static void ParseActionItemsText(MeetingTrackState st, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            MeetingTrackState.ActionItem? currentItem = null;
            List<string>? currentNotes = null;
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
                        if (currentNotes != null)
                            currentNotes[currentNotes.Count - 1] += Environment.NewLine + line.Substring(offset);
                        else
                            currentItem.Task += Environment.NewLine + line.Substring(offset);
                        continue;
                    }
                    var am = ActionLineRegex.Match(line);
                    if (am.Success) {
                        int state;
                        if (!int.TryParse(am.Groups[2].Value, out state))
                            state = (int)ActionItemStateEnum.Open;
                        currentItem = new MeetingTrackState.ActionItem();
                        currentItem.State = state;
                        currentItem.Owner = am.Groups[3].Value.Trim();
                        string deadline = am.Groups[4].Value.Trim();
                        currentItem.Deadline = deadline == "-" ? string.Empty : deadline;
                        currentItem.Task = am.Groups[5].Value.Trim();
                        st.ActionItems.Add(currentItem);
                        currentNotes = currentItem.Notes;
                        currentIndentation = 0;
                        continue;
                    }
                    var nm = NoteLineRegex.Match(line);
                    if (nm.Success && currentNotes != null) {
                        currentIndentation = nm.Groups[1].Value.Length;
                        currentNotes.Add(nm.Groups[3].Value);
                    }
                }
            }
        }

        internal static string GenerateMinutesText(MeetingTrackState st)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("# [MINUTES]");
            foreach (var entry in st.Minutes)
                sb.Append('[').Append(entry.Timestamp).Append("] ")
                    .AppendLine(EncodeContinuationLines(entry.Text));
            return sb.ToString();
        }

        // Appends parsed minutes entries into st.
        internal static void ParseMinutesText(MeetingTrackState st, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            MeetingTrackState.MinutesEntry? currentEntry = null;
            int currentIndentation = 0;
            string? line;
            using (var sr = new System.IO.StringReader(text)) {
                while ((line = sr.ReadLine()) != null) {
                    if (line.StartsWith("+", StringComparison.Ordinal)) {
                        if (currentEntry == null)
                            continue;
                        int offset = 1;
                        while (offset < line.Length && offset <= currentIndentation
                            && (line[offset] == ' ' || line[offset] == '\t'))
                            offset++;
                        currentEntry.Text += Environment.NewLine + line.Substring(offset);
                        continue;
                    }
                    var mm = MinutesLineRegex.Match(line);
                    if (mm.Success) {
                        currentEntry = new MeetingTrackState.MinutesEntry();
                        currentEntry.Timestamp = mm.Groups[1].Value.Trim();
                        currentEntry.Text = mm.Groups[2].Value;
                        st.Minutes.Add(currentEntry);
                        currentIndentation = 0;
                    }
                }
            }
        }

        // Combined read-only view for prompt injection (agenda + action items
        // + latest minutes entries).
        internal static string GenerateMeetingView(MeetingTrackState st, int maxMinutes = 5)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("会议状态: ").Append(st.MeetingState)
                .Append(" (0=待开 1=进行中 2=已散会)，主题: ")
                .AppendLine(st.Title.Length > 0 ? st.Title : "(untitled)");
            sb.AppendLine("【议程】");
            if (st.AgendaItems.Count == 0) {
                sb.AppendLine("(无议程)");
            }
            else {
                for (int i = 0; i < st.AgendaItems.Count; i++) {
                    var it = st.AgendaItems[i];
                    sb.Append("-[").Append(i).Append("] state=").Append(it.State)
                        .Append(" (0=待议 1=讨论中 2=跳过 3=完成) ")
                        .AppendLine(it.Content);
                }
            }
            sb.AppendLine("【行动项】");
            if (st.ActionItems.Count == 0) {
                sb.AppendLine("(无行动项)");
            }
            else {
                for (int i = 0; i < st.ActionItems.Count; i++) {
                    var it = st.ActionItems[i];
                    sb.Append("-[").Append(i).Append("] state=").Append(it.State)
                        .Append(" (0=待办 1=进行中 2=完成) 负责人: ")
                        .Append(it.Owner.Length > 0 ? it.Owner : "-");
                    if (it.Deadline.Length > 0)
                        sb.Append(" 期限: ").Append(it.Deadline);
                    sb.Append(" 任务: ").AppendLine(it.Task);
                }
            }
            sb.AppendLine("【最近纪要】");
            if (st.Minutes.Count == 0) {
                sb.AppendLine("(无纪要)");
            }
            else {
                int start = Math.Max(0, st.Minutes.Count - maxMinutes);
                for (int i = start; i < st.Minutes.Count; i++)
                    sb.Append('[').Append(st.Minutes[i].Timestamp).Append("] ")
                        .AppendLine(st.Minutes[i].Text);
            }
            return sb.ToString();
        }

        // Write agenda.txt / action_items.txt / minutes.txt under the meeting
        // dir (UTF-8 no BOM, temp file + replace). Missing ProjectDir falls
        // back to DefaultProjectDir without mutating the instance.
        internal static void PersistMeeting(AgentInstance inst, string channelId, MeetingTrackState st)
        {
            try {
                string dir = GetMeetingDir(inst, channelId);
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                WriteFileAtomic(System.IO.Path.Combine(dir, "agenda.txt"), GenerateAgendaText(st));
                WriteFileAtomic(System.IO.Path.Combine(dir, "action_items.txt"), GenerateActionItemsText(st));
                WriteFileAtomic(System.IO.Path.Combine(dir, "minutes.txt"), GenerateMinutesText(st));
            }
            catch (Exception ex) {
                MetaDslExecutor.AppendApiErrorInfoLine("meeting persist failed: " + ex.Message);
            }
        }

        // Read the three files back into a fresh state. Returns false when
        // no agenda.txt exists (meeting never opened / never persisted).
        internal static bool RestoreMeeting(AgentInstance inst, string channelId, out MeetingTrackState st)
        {
            st = new MeetingTrackState();
            try {
                string dir = GetMeetingDir(inst, channelId);
                string agendaPath = System.IO.Path.Combine(dir, "agenda.txt");
                if (!System.IO.File.Exists(agendaPath))
                    return false;
                var parsed = ParseAgendaText(System.IO.File.ReadAllText(agendaPath));
                if (parsed == null)
                    return false;
                st = parsed;
                string actionsPath = System.IO.Path.Combine(dir, "action_items.txt");
                if (System.IO.File.Exists(actionsPath))
                    ParseActionItemsText(st, System.IO.File.ReadAllText(actionsPath));
                string minutesPath = System.IO.Path.Combine(dir, "minutes.txt");
                if (System.IO.File.Exists(minutesPath))
                    ParseMinutesText(st, System.IO.File.ReadAllText(minutesPath));
                return true;
            }
            catch (Exception ex) {
                MetaDslExecutor.AppendApiErrorInfoLine("meeting restore failed: " + ex.Message);
                return false;
            }
        }

        private static void WriteFileAtomic(string path, string text)
        {
            string tmp = path + ".tmp";
            System.IO.File.WriteAllText(tmp, text, new System.Text.UTF8Encoding(false));
            if (System.IO.File.Exists(path))
                System.IO.File.Replace(tmp, path, null);
            else
                System.IO.File.Move(tmp, path);
        }
    }
}
