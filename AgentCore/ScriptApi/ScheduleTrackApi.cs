using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using static AgentCore.ScriptApi.AgentIdResolver;

namespace AgentCore.ScriptApi
{
    /// <summary>
    /// Meeting schedule apis: recurring meeting times, mainly maintained by
    /// the host PM (or the user via PM private chat). One global list across
    /// all rooms; each item points at the room (channelId) where the meeting
    /// takes place. The meeting room page polls get_schedule and reminds the
    /// PM when a scheduled time is reached. File:
    /// ProjectDir/docs/meeting_schedule.txt.
    /// </summary>
    internal static class ScheduleTrackUtil
    {
        // Lazy-load the schedule: in-memory state when present, disk otherwise.
        internal static Core.ScheduleTrackState GetSchedule(Core.AgentInstance inst)
        {
            if (inst.ScheduleTrack == null)
                inst.ScheduleTrack = Core.ScheduleTrackHelper.LoadSchedule(inst);
            return inst.ScheduleTrack;
        }
    }

    /// <summary>
    /// add_schedule_item(agentId, channelId, title, repeat, weekday, time) -
    /// register a recurring meeting. repeat: "daily" or "weekly"; weekday:
    /// 1=Mon ... 7=Sun (weekly only); time: "HH:mm". Returns the 0-based
    /// index, -1 on error.
    /// </summary>
    sealed class ScheduleAddExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(DslCalculator calculator, IList<BoxedValue> operands)
        {
            if (operands.Count != 6) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("add_schedule_item requires (agentId, channelId, title, repeat, weekday, time)");
                return BoxedValue.From(-1);
            }
            var inst = Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            string channelId = (operands[1].AsString ?? string.Empty).Trim();
            string title = operands[2].AsString ?? string.Empty;
            string repeat = (operands[3].AsString ?? string.Empty).Trim().ToLowerInvariant();
            int weekday = (int)operands[4].GetInt();
            string time = (operands[5].AsString ?? string.Empty).Trim();
            if (channelId.Length == 0) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("add_schedule_item rejected: channelId is empty");
                return BoxedValue.From(-1);
            }
            if (!Core.ScheduleTrackHelper.IsValidRepeat(repeat)) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("add_schedule_item rejected: repeat must be \"daily\" or \"weekly\"");
                return BoxedValue.From(-1);
            }
            if (repeat == "weekly" && (weekday < 1 || weekday > 7)) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("add_schedule_item rejected: weekday must be 1-7 (1=Mon ... 7=Sun) for weekly");
                return BoxedValue.From(-1);
            }
            if (!Core.ScheduleTrackHelper.IsValidTime(time)) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("add_schedule_item rejected: time must be \"HH:mm\" (24h)");
                return BoxedValue.From(-1);
            }
            var st = ScheduleTrackUtil.GetSchedule(inst);
            var it = new Core.ScheduleTrackState.ScheduleItem();
            it.ChannelId = channelId;
            it.Title = title;
            it.Repeat = repeat;
            it.Weekday = weekday;
            it.Time = time;
            st.Items.Add(it);
            Core.ScheduleTrackHelper.PersistSchedule(inst, st);
            return BoxedValue.From(st.Items.Count - 1);
        }
    }

    /// <summary>
    /// delete_schedule_items(agentId, idx[, idx2, ...]) - delete schedule
    /// entries by 0-based index. All indexes are validated first; any
    /// out-of-range index rejects the whole call. Returns the removed count,
    /// -1 on error.
    /// </summary>
    sealed class ScheduleDeleteExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(DslCalculator calculator, IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_schedule_items requires (agentId, idx[, idx2, ...])");
                return BoxedValue.From(-1);
            }
            var inst = Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = ScheduleTrackUtil.GetSchedule(inst);
            var indexes = new List<int>();
            var seen = new HashSet<int>();
            for (int i = 1; i < operands.Count; i++) {
                int idx = (int)operands[i].GetInt();
                if (idx < 0 || idx >= st.Items.Count) {
                    Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_schedule_items rejected: index out of range (" + idx + ")");
                    return BoxedValue.From(-1);
                }
                if (seen.Add(idx))
                    indexes.Add(idx);
            }
            indexes.Sort();
            indexes.Reverse();
            foreach (int idx in indexes)
                st.Items.RemoveAt(idx);
            Core.ScheduleTrackHelper.PersistSchedule(inst, st);
            return BoxedValue.From(indexes.Count);
        }
    }

    /// <summary>
    /// set_schedule_item_enabled(agentId, idx, enabled) - pause/resume a
    /// schedule entry without deleting it. Returns true/false.
    /// </summary>
    sealed class ScheduleSetEnabledExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(DslCalculator calculator, IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("set_schedule_item_enabled requires (agentId, idx, enabled)");
                return BoxedValue.From(false);
            }
            var inst = Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = ScheduleTrackUtil.GetSchedule(inst);
            int idx = (int)operands[1].GetInt();
            if (idx < 0 || idx >= st.Items.Count) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("set_schedule_item_enabled rejected: index out of range (" + idx + ")");
                return BoxedValue.From(false);
            }
            st.Items[idx].Enabled = operands[2].GetBool();
            Core.ScheduleTrackHelper.PersistSchedule(inst, st);
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// get_schedule(agentId) - the schedule text (also machine-parseable by
    /// the meeting room page). Empty string on error.
    /// </summary>
    sealed class ScheduleGetExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(DslCalculator calculator, IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("get_schedule requires (agentId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = ScheduleTrackUtil.GetSchedule(inst);
            return BoxedValue.FromString(Core.ScheduleTrackHelper.GenerateScheduleText(st));
        }
    }

    /// <summary>
    /// Registration entry for schedule apis (same pattern as
    /// MeetingTrackApi.RegisterApis).
    /// </summary>
    public static class ScheduleTrackApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("add_schedule_item",
                "add_schedule_item(agentId, channelId, title, repeat, weekday, time) - register a recurring meeting; repeat: \"daily\"/\"weekly\", weekday: 1=Mon...7=Sun (weekly only), time: \"HH:mm\", returns its index or -1 on error",
                new ExpressionFactoryHelper<ScheduleAddExp>());
            BatchCommand.BatchScript.Register("delete_schedule_items",
                "delete_schedule_items(agentId, idx[, idx2, ...]) - delete schedule entries by index, returns the removed count or -1 on error",
                new ExpressionFactoryHelper<ScheduleDeleteExp>());
            BatchCommand.BatchScript.Register("set_schedule_item_enabled",
                "set_schedule_item_enabled(agentId, idx, enabled) - pause/resume a schedule entry, returns true/false",
                new ExpressionFactoryHelper<ScheduleSetEnabledExp>());
            BatchCommand.BatchScript.Register("get_schedule",
                "get_schedule(agentId) - the meeting schedule text",
                new ExpressionFactoryHelper<ScheduleGetExp>());
        }
    }
}
