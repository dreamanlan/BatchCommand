using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using static AgentCore.ScriptApi.AgentIdResolver;

namespace AgentCore.ScriptApi
{
    /// <summary>
    /// Meeting track apis: agenda / action items / minutes for one meeting
    /// room, keyed by channelId (each group session of the meeting room page,
    /// agentId 'aichat'). The meeting room's AgentCore is only one of the
    /// participants' backends - other attendees may not be reachable via api
    /// at all, so decisions reach them only through chat messages; these
    /// apis purely persist the meeting's own state.
    /// Write convention (prompt level, not enforced): everyone may
    /// add_agenda_item / add_action_item, advancing the agenda and closing
    /// the meeting is the host PM's job (change_agenda_item_state /
    /// append_minutes / close_meeting).
    /// Files: ProjectDir/docs/meetings/&lt;channelId&gt;/{agenda,action_items,minutes}.txt
    /// (ProjectDir falls back to MeetingTrackHelper.DefaultProjectDir).
    /// </summary>
    internal static class MeetingTrackUtil
    {
        internal static Core.AgentInstance ResolveInstance(BoxedValue v)
        {
            return Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(v));
        }

        internal static string ResolveChannelId(BoxedValue v)
        {
            return (v.AsString ?? string.Empty).Trim();
        }

        // Fetch the in-memory state; on a miss (e.g. after an AgentCore
        // restart) auto-restore the room's latest unfinished meeting from
        // disk. Null + error line only when the room has no active meeting.
        internal static Core.MeetingTrackState? GetMeeting(Core.AgentInstance inst, string channelId, string api)
        {
            if (channelId.Length == 0) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine(api + " rejected: channelId is empty");
                return null;
            }
            if (!inst.MeetingTracks.TryGetValue(channelId, out var st)) {
                if (Core.MeetingTrackHelper.RestoreLatestMeeting(inst, channelId, out st)) {
                    inst.MeetingTracks[channelId] = st;
                    Core.MetaDslExecutor.AppendApiTipInfoLine("auto-restored the latest unfinished meeting in this room (dir " + st.DirName + ")");
                }
                else {
                    Core.MetaDslExecutor.AppendApiErrorInfoLine(api + " rejected: no active meeting in this room, open_meeting first");
                    return null;
                }
            }
            return st;
        }

        internal static bool ValidateItemState(int state, int max, string api)
        {
            if (state < 0 || state > max) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine(api + " rejected: invalid state " + state);
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// open_meeting(agentId, channelId, title) - create a meeting with an
    /// empty agenda for the room. Rejected while a meeting is in progress(1)
    /// in that room; planned(0)/done(2)/none allow a new one.
    /// </summary>
    sealed class MeetingOpenExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("open_meeting requires (agentId, channelId, title)");
                return BoxedValue.From(false);
            }
            var inst = MeetingTrackUtil.ResolveInstance(operands[0]);
            string channelId = MeetingTrackUtil.ResolveChannelId(operands[1]);
            if (channelId.Length == 0) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("open_meeting rejected: channelId is empty");
                return BoxedValue.From(false);
            }
            if (inst.MeetingTracks.TryGetValue(channelId, out var existing)
                && existing.MeetingState == (int)Core.MeetingStateEnum.InProgress) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("open_meeting rejected: a meeting is in progress in this room, close_meeting first");
                return BoxedValue.From(false);
            }
            var st = new Core.MeetingTrackState();
            st.Title = operands[2].AsString ?? string.Empty;
            st.MeetingState = (int)Core.MeetingStateEnum.Planned;
            st.DirName = Core.MeetingTrackHelper.BuildMeetingDirName(channelId, st.Title);
            inst.MeetingTracks[channelId] = st;
            Core.MeetingTrackHelper.PersistMeeting(inst, channelId, st);
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// add_agenda_item(agentId, channelId, content) - append an agenda item
    /// (any meeting state), returns the 0-based index, -1 on error.
    /// </summary>
    sealed class MeetingAgendaAddExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("add_agenda_item requires (agentId, channelId, content)");
                return BoxedValue.From(-1);
            }
            var inst = MeetingTrackUtil.ResolveInstance(operands[0]);
            string channelId = MeetingTrackUtil.ResolveChannelId(operands[1]);
            var st = MeetingTrackUtil.GetMeeting(inst, channelId, "add_agenda_item");
            if (st == null)
                return BoxedValue.From(-1);
            var it = new Core.MeetingTrackState.AgendaItem();
            it.Content = operands[2].AsString ?? string.Empty;
            st.AgendaItems.Add(it);
            Core.MeetingTrackHelper.PersistMeeting(inst, channelId, st);
            return BoxedValue.From(st.AgendaItems.Count - 1);
        }
    }

    /// <summary>
    /// change_agenda_item_state(agentId, channelId, idx, state[, note]) -
    /// change an agenda item's state (0=planned 1=discussing 2=skipped
    /// 3=done) and optionally append a timestamped note. Host PM's job.
    /// </summary>
    sealed class MeetingAgendaStateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 4 && operands.Count != 5) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("change_agenda_item_state requires (agentId, channelId, idx, state[, note])");
                return BoxedValue.From(false);
            }
            var inst = MeetingTrackUtil.ResolveInstance(operands[0]);
            string channelId = MeetingTrackUtil.ResolveChannelId(operands[1]);
            var st = MeetingTrackUtil.GetMeeting(inst, channelId, "change_agenda_item_state");
            if (st == null)
                return BoxedValue.From(false);
            int idx = (int)operands[2].GetInt();
            if (idx < 0 || idx >= st.AgendaItems.Count) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("change_agenda_item_state rejected: index out of range (" + idx + ")");
                return BoxedValue.From(false);
            }
            int state = (int)operands[3].GetInt();
            if (!MeetingTrackUtil.ValidateItemState(state, (int)Core.AgendaItemStateEnum.Done, "change_agenda_item_state"))
                return BoxedValue.From(false);
            st.AgendaItems[idx].State = state;
            if (operands.Count == 5) {
                string note = operands[4].AsString ?? string.Empty;
                if (note.Length > 0)
                    st.AgendaItems[idx].Notes.Add(Core.MeetingTrackHelper.TimestampNow() + " " + note);
            }
            Core.MeetingTrackHelper.PersistMeeting(inst, channelId, st);
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// append_agenda_note(agentId, channelId, note) - append a timestamped
    /// meeting-level note.
    /// </summary>
    sealed class MeetingAgendaNoteExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("append_agenda_note requires (agentId, channelId, note)");
                return BoxedValue.From(false);
            }
            var inst = MeetingTrackUtil.ResolveInstance(operands[0]);
            string channelId = MeetingTrackUtil.ResolveChannelId(operands[1]);
            var st = MeetingTrackUtil.GetMeeting(inst, channelId, "append_agenda_note");
            if (st == null)
                return BoxedValue.From(false);
            string note = operands[2].AsString ?? string.Empty;
            if (note.Length > 0)
                st.Notes.Add(Core.MeetingTrackHelper.TimestampNow() + " " + note);
            Core.MeetingTrackHelper.PersistMeeting(inst, channelId, st);
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// append_agenda_item_note(agentId, channelId, idx, note) - append a
    /// timestamped note to an agenda item without changing its state.
    /// </summary>
    sealed class MeetingAgendaItemNoteExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 4) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("append_agenda_item_note requires (agentId, channelId, idx, note)");
                return BoxedValue.From(false);
            }
            var inst = MeetingTrackUtil.ResolveInstance(operands[0]);
            string channelId = MeetingTrackUtil.ResolveChannelId(operands[1]);
            var st = MeetingTrackUtil.GetMeeting(inst, channelId, "append_agenda_item_note");
            if (st == null)
                return BoxedValue.From(false);
            int idx = (int)operands[2].GetInt();
            if (idx < 0 || idx >= st.AgendaItems.Count) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("append_agenda_item_note rejected: index out of range (" + idx + ")");
                return BoxedValue.From(false);
            }
            string note = operands[3].AsString ?? string.Empty;
            if (note.Length > 0)
                st.AgendaItems[idx].Notes.Add(Core.MeetingTrackHelper.TimestampNow() + " " + note);
            Core.MeetingTrackHelper.PersistMeeting(inst, channelId, st);
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// get_agenda(agentId, channelId) - agenda.txt text of the room's
    /// meeting, empty string on error (details in the api error info).
    /// </summary>
    sealed class MeetingGetAgendaExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("get_agenda requires (agentId, channelId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = MeetingTrackUtil.ResolveInstance(operands[0]);
            string channelId = MeetingTrackUtil.ResolveChannelId(operands[1]);
            var st = MeetingTrackUtil.GetMeeting(inst, channelId, "get_agenda");
            if (st == null)
                return BoxedValue.FromString(string.Empty);
            return BoxedValue.FromString(Core.MeetingTrackHelper.GenerateAgendaText(st));
        }
    }

    /// <summary>
    /// get_action_items(agentId, channelId) - action_items.txt text of the
    /// room's meeting, empty string on error.
    /// </summary>
    sealed class MeetingGetActionsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("get_action_items requires (agentId, channelId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = MeetingTrackUtil.ResolveInstance(operands[0]);
            string channelId = MeetingTrackUtil.ResolveChannelId(operands[1]);
            var st = MeetingTrackUtil.GetMeeting(inst, channelId, "get_action_items");
            if (st == null)
                return BoxedValue.FromString(string.Empty);
            return BoxedValue.FromString(Core.MeetingTrackHelper.GenerateActionItemsText(st));
        }
    }

    /// <summary>
    /// get_meeting(agentId, channelId) - combined view (agenda + action
    /// items + latest minutes) for prompt injection, empty string on error.
    /// </summary>
    sealed class MeetingGetViewExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("get_meeting requires (agentId, channelId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = MeetingTrackUtil.ResolveInstance(operands[0]);
            string channelId = MeetingTrackUtil.ResolveChannelId(operands[1]);
            var st = MeetingTrackUtil.GetMeeting(inst, channelId, "get_meeting");
            if (st == null)
                return BoxedValue.FromString(string.Empty);
            return BoxedValue.FromString(Core.MeetingTrackHelper.GenerateMeetingView(st));
        }
    }

    /// <summary>
    /// align_agenda(agentId, channelId) - derive the meeting state from the
    /// agenda items, regenerate the texts, persist and return agenda text.
    /// </summary>
    sealed class MeetingAlignExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("align_agenda requires (agentId, channelId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = MeetingTrackUtil.ResolveInstance(operands[0]);
            string channelId = MeetingTrackUtil.ResolveChannelId(operands[1]);
            var st = MeetingTrackUtil.GetMeeting(inst, channelId, "align_agenda");
            if (st == null)
                return BoxedValue.FromString(string.Empty);
            if (st.MeetingState != (int)Core.MeetingStateEnum.Done)
                st.MeetingState = Core.MeetingTrackHelper.DeriveMeetingState(st.MeetingState, st.AgendaItems);
            Core.MeetingTrackHelper.PersistMeeting(inst, channelId, st);
            return BoxedValue.FromString(Core.MeetingTrackHelper.GenerateAgendaText(st));
        }
    }

    /// <summary>
    /// restore_meeting_state(agentId, channelId) - explicitly reload the
    /// room's latest unfinished meeting from disk after an AgentCore
    /// restart. Usually unnecessary: get_* apis auto-restore on a miss. Only
    /// takes effect when the in-memory state is empty (same semantics as
    /// restore_plan_state); finished meetings stay archived, open a new one
    /// with open_meeting instead.
    /// </summary>
    sealed class MeetingRestoreExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("restore_meeting_state requires (agentId, channelId)");
                return BoxedValue.From(false);
            }
            var inst = MeetingTrackUtil.ResolveInstance(operands[0]);
            string channelId = MeetingTrackUtil.ResolveChannelId(operands[1]);
            if (channelId.Length == 0) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("restore_meeting_state rejected: channelId is empty");
                return BoxedValue.From(false);
            }
            if (inst.MeetingTracks.TryGetValue(channelId, out _)) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("restore_meeting_state skipped: in-memory state already exists");
                return BoxedValue.From(false);
            }
            if (!Core.MeetingTrackHelper.RestoreLatestMeeting(inst, channelId, out var st))
                return BoxedValue.From(false);
            inst.MeetingTracks[channelId] = st;
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// add_action_item(agentId, channelId, owner, task[, deadline]) -
    /// register a decision's action item, returns the 0-based index, -1 on
    /// error. Owner is a plain label: other attendees' agents are not
    /// reachable through this AgentCore, so the assignment reaches them via
    /// chat messages only (the PM should also state it in the conversation).
    /// </summary>
    sealed class MeetingActionAddExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 4 && operands.Count != 5) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("add_action_item requires (agentId, channelId, owner, task[, deadline])");
                return BoxedValue.From(-1);
            }
            var inst = MeetingTrackUtil.ResolveInstance(operands[0]);
            string channelId = MeetingTrackUtil.ResolveChannelId(operands[1]);
            var st = MeetingTrackUtil.GetMeeting(inst, channelId, "add_action_item");
            if (st == null)
                return BoxedValue.From(-1);
            var it = new Core.MeetingTrackState.ActionItem();
            it.Owner = operands[2].AsString ?? string.Empty;
            it.Task = operands[3].AsString ?? string.Empty;
            it.Deadline = operands.Count == 5 ? (operands[4].AsString ?? string.Empty) : string.Empty;
            st.ActionItems.Add(it);
            Core.MeetingTrackHelper.PersistMeeting(inst, channelId, st);
            return BoxedValue.From(st.ActionItems.Count - 1);
        }
    }

    /// <summary>
    /// append_minutes(agentId, channelId, text) - append a timestamped
    /// minutes entry (host PM's summary). Host PM's job.
    /// </summary>
    sealed class MeetingMinutesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("append_minutes requires (agentId, channelId, text)");
                return BoxedValue.From(false);
            }
            var inst = MeetingTrackUtil.ResolveInstance(operands[0]);
            string channelId = MeetingTrackUtil.ResolveChannelId(operands[1]);
            var st = MeetingTrackUtil.GetMeeting(inst, channelId, "append_minutes");
            if (st == null)
                return BoxedValue.From(false);
            string text = operands[2].AsString ?? string.Empty;
            if (text.Length > 0) {
                var entry = new Core.MeetingTrackState.MinutesEntry();
                entry.Timestamp = Core.MeetingTrackHelper.TimestampNow();
                entry.Text = text;
                st.Minutes.Add(entry);
            }
            Core.MeetingTrackHelper.PersistMeeting(inst, channelId, st);
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// close_meeting(agentId, channelId[, summary]) - mark the meeting done
    /// and optionally append the summary to the minutes. Host PM's job.
    /// </summary>
    sealed class MeetingCloseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2 && operands.Count != 3) {
                Core.MetaDslExecutor.AppendApiErrorInfoLine("close_meeting requires (agentId, channelId[, summary])");
                return BoxedValue.From(false);
            }
            var inst = MeetingTrackUtil.ResolveInstance(operands[0]);
            string channelId = MeetingTrackUtil.ResolveChannelId(operands[1]);
            var st = MeetingTrackUtil.GetMeeting(inst, channelId, "close_meeting");
            if (st == null)
                return BoxedValue.From(false);
            st.MeetingState = (int)Core.MeetingStateEnum.Done;
            if (operands.Count == 3) {
                string summary = operands[2].AsString ?? string.Empty;
                if (summary.Length > 0) {
                    var entry = new Core.MeetingTrackState.MinutesEntry();
                    entry.Timestamp = Core.MeetingTrackHelper.TimestampNow();
                    entry.Text = summary;
                    st.Minutes.Add(entry);
                }
            }
            Core.MeetingTrackHelper.PersistMeeting(inst, channelId, st);
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// Registration entry for MeetingTrack APIs (same pattern as
    /// PlanTrackApi.RegisterApis: the api file owns its registrations, the
    /// ScriptApiRegistrar only forwards with one line).
    /// </summary>
    public static class MeetingTrackApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("open_meeting",
                "open_meeting(agentId, channelId, title) - create a meeting with an empty agenda for the room, returns true/false",
                new ExpressionFactoryHelper<MeetingOpenExp>());
            BatchCommand.BatchScript.Register("add_agenda_item",
                "add_agenda_item(agentId, channelId, content) - add an agenda item, returns its index or -1 on error",
                new ExpressionFactoryHelper<MeetingAgendaAddExp>());
            BatchCommand.BatchScript.Register("change_agenda_item_state",
                "change_agenda_item_state(agentId, channelId, idx, state[, note]) - agenda item state: 0=planned 1=discussing 2=skipped 3=done, returns true/false",
                new ExpressionFactoryHelper<MeetingAgendaStateExp>());
            BatchCommand.BatchScript.Register("append_agenda_note",
                "append_agenda_note(agentId, channelId, note) - append a timestamped meeting-level note, returns true/false",
                new ExpressionFactoryHelper<MeetingAgendaNoteExp>());
            BatchCommand.BatchScript.Register("append_agenda_item_note",
                "append_agenda_item_note(agentId, channelId, idx, note) - append a timestamped note to an agenda item without changing state, returns true/false",
                new ExpressionFactoryHelper<MeetingAgendaItemNoteExp>());
            BatchCommand.BatchScript.Register("get_agenda",
                "get_agenda(agentId, channelId) - agenda text of the room's meeting",
                new ExpressionFactoryHelper<MeetingGetAgendaExp>());
            BatchCommand.BatchScript.Register("get_action_items",
                "get_action_items(agentId, channelId) - action items text of the room's meeting",
                new ExpressionFactoryHelper<MeetingGetActionsExp>());
            BatchCommand.BatchScript.Register("get_meeting",
                "get_meeting(agentId, channelId) - combined view (agenda + action items + latest minutes)",
                new ExpressionFactoryHelper<MeetingGetViewExp>());
            BatchCommand.BatchScript.Register("align_agenda",
                "align_agenda(agentId, channelId) - derive the meeting state from the agenda items and persist, returns the agenda text",
                new ExpressionFactoryHelper<MeetingAlignExp>());
            BatchCommand.BatchScript.Register("restore_meeting_state",
                "restore_meeting_state(agentId, channelId) - reload the room's meeting from disk (only when in-memory state is empty), returns true/false",
                new ExpressionFactoryHelper<MeetingRestoreExp>());
            BatchCommand.BatchScript.Register("add_action_item",
                "add_action_item(agentId, channelId, owner, task[, deadline]) - register a decision's action item, returns its index or -1 on error",
                new ExpressionFactoryHelper<MeetingActionAddExp>());
            BatchCommand.BatchScript.Register("append_minutes",
                "append_minutes(agentId, channelId, text) - append a timestamped minutes entry, returns true/false",
                new ExpressionFactoryHelper<MeetingMinutesExp>());
            BatchCommand.BatchScript.Register("close_meeting",
                "close_meeting(agentId, channelId[, summary]) - mark the meeting done, optionally append the summary to the minutes, returns true/false",
                new ExpressionFactoryHelper<MeetingCloseExp>());
        }
    }
}
