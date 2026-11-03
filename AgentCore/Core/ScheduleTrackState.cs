using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AgentCore.Core
{
    /// <summary>
    /// In-memory meeting schedule for one agent (the meeting room page,
    /// agentId 'aichat'): recurring meeting times the PM gets reminded of.
    /// One global list across all rooms; each item points at the room
    /// (channelId) where the meeting takes place.
    /// </summary>
    public sealed class ScheduleTrackState
    {
        public readonly List<ScheduleItem> Items = new List<ScheduleItem>();

        public sealed class ScheduleItem
        {
            public bool Enabled = true;
            public string ChannelId = string.Empty;
            public string Title = string.Empty;
            public string Repeat = "weekly";   // daily | weekly
            public int Weekday = 1;            // 1=Mon ... 7=Sun (weekly only)
            public string Time = "09:00";      // HH:mm
        }
    }

    /// <summary>
    /// meeting_schedule.txt text generation/parsing for ScheduleTrack.
    /// File: ProjectDir/docs/meeting_schedule.txt (UTF-8 no BOM, temp file
    /// + replace; ProjectDir falls back like the meeting dirs).
    /// Format (line by line with regexes):
    /// # [SCHEDULE]
    /// -[0] enabled=1 repeat=weekly weekday=3 time=14:00 channel=group_xxx | 周会
    /// </summary>
    internal static class ScheduleTrackHelper
    {
        private static readonly Regex ItemLineRegex = new Regex(
            @"^-\[(\d+)\]\s+enabled=(\d+)\s+repeat=(\S+)\s+weekday=(\d+)\s+time=(\S+)\s+channel=(\S+)\s*\|\s*(.*)$", RegexOptions.Compiled);

        internal static bool IsValidRepeat(string repeat)
        {
            return repeat == "daily" || repeat == "weekly";
        }

        internal static bool IsValidTime(string time)
        {
            if (string.IsNullOrEmpty(time) || time.Length != 5 || time[2] != ':')
                return false;
            if (!int.TryParse(time.Substring(0, 2), out int h) || !int.TryParse(time.Substring(3, 2), out int m))
                return false;
            return h >= 0 && h <= 23 && m >= 0 && m <= 59;
        }

        internal static string GenerateScheduleText(ScheduleTrackState st)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("# [SCHEDULE]");
            for (int i = 0; i < st.Items.Count; i++) {
                var it = st.Items[i];
                sb.Append("-[").Append(i).Append("] enabled=").Append(it.Enabled ? 1 : 0)
                    .Append(" repeat=").Append(it.Repeat)
                    .Append(" weekday=").Append(it.Weekday)
                    .Append(" time=").Append(it.Time)
                    .Append(" channel=").Append(it.ChannelId)
                    .Append(" | ").AppendLine(it.Title);
            }
            return sb.ToString();
        }

        // Returns null when no [SCHEDULE] line is found.
        internal static ScheduleTrackState? ParseScheduleText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            ScheduleTrackState? st = null;
            string? line;
            using (var sr = new System.IO.StringReader(text)) {
                while ((line = sr.ReadLine()) != null) {
                    if (st == null) {
                        if (line.StartsWith("# [SCHEDULE]", StringComparison.Ordinal))
                            st = new ScheduleTrackState();
                        continue;
                    }
                    var m = ItemLineRegex.Match(line);
                    if (m.Success) {
                        var it = new ScheduleTrackState.ScheduleItem();
                        it.Enabled = m.Groups[2].Value != "0";
                        it.Repeat = m.Groups[3].Value;
                        int.TryParse(m.Groups[4].Value, out int wd);
                        it.Weekday = wd;
                        it.Time = m.Groups[5].Value;
                        it.ChannelId = m.Groups[6].Value;
                        it.Title = m.Groups[7].Value.Trim();
                        st.Items.Add(it);
                    }
                }
            }
            return st;
        }

        // Load from ProjectDir/docs/meeting_schedule.txt; a missing file is
        // an empty schedule (not an error). Does NOT mutate inst.ProjectDir.
        internal static ScheduleTrackState LoadSchedule(AgentInstance inst)
        {
            var st = new ScheduleTrackState();
            try {
                string projectDir = string.IsNullOrEmpty(inst.ProjectDir) ? MeetingTrackHelper.DefaultProjectDir : inst.ProjectDir;
                string path = System.IO.Path.Combine(projectDir, "docs", "meeting_schedule.txt");
                if (!System.IO.File.Exists(path))
                    return st;
                var parsed = ParseScheduleText(System.IO.File.ReadAllText(path));
                if (parsed != null)
                    return parsed;
            }
            catch (Exception ex) {
                MetaDslExecutor.AppendApiErrorInfoLine("meeting schedule load failed: " + ex.Message);
            }
            return st;
        }

        // Persist to the same file (UTF-8 no BOM, temp file + replace).
        internal static void PersistSchedule(AgentInstance inst, ScheduleTrackState st)
        {
            try {
                string projectDir = string.IsNullOrEmpty(inst.ProjectDir) ? MeetingTrackHelper.DefaultProjectDir : inst.ProjectDir;
                string dir = System.IO.Path.Combine(projectDir, "docs");
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                string path = System.IO.Path.Combine(dir, "meeting_schedule.txt");
                string tmp = path + ".tmp";
                System.IO.File.WriteAllText(tmp, GenerateScheduleText(st), new System.Text.UTF8Encoding(false));
                if (System.IO.File.Exists(path))
                    System.IO.File.Replace(tmp, path, null);
                else
                    System.IO.File.Move(tmp, path);
            }
            catch (Exception ex) {
                MetaDslExecutor.AppendApiErrorInfoLine("meeting schedule persist failed: " + ex.Message);
            }
        }
    }
}
