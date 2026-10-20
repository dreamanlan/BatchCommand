using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AgentCore.Core;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using static AgentCore.ScriptApi.AgentIdResolver;

namespace AgentCore.ScriptApi
{
    internal static class PlanNoteUtil
    {
        // Validate every index in operands[start..] against [0, count), drop
        // duplicates, and return them in descending order so RemoveAt calls do
        // not shift each other. Any out-of-range index fails the whole call.
        internal static bool CollectIndexes(IList<BoxedValue> operands, int start, int count, string api, string what, out List<int> indexes)
        {
            indexes = new List<int>();
            var seen = new HashSet<int>();
            for (int i = start; i < operands.Count; i++) {
                int idx = (int)operands[i].GetInt();
                if (idx < 0 || idx >= count) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine(api + " rejected: " + what + " index out of range (" + idx + ")");
                    indexes.Clear();
                    return false;
                }
                if (seen.Add(idx))
                    indexes.Add(idx);
            }
            indexes.Sort();
            indexes.Reverse();
            return true;
        }
    }

    /// <summary>
    /// create_plan(agentId, title) - create a new plan and make it current.
    /// High-order style: create_plan(agentId, title){"item1";"item2";...} adds initial items (state 0=planned).
    /// Rejected while a plan is in progress(1); planned(0)/done(2)/none allow a new plan.
    /// </summary>
    sealed class PlanCreateExp : AbstractExpression
    {
        private IExpression? m_AgentIdExp;
        private IExpression? m_TitleExp;
        private readonly List<IExpression> m_ItemExps = new List<IExpression>();

        protected override bool Load(Dsl.FunctionData funcData)
        {
            var callData = funcData.IsHighOrder ? funcData.LowerOrderFunction : funcData;
            if (null == callData || callData.GetParamNum() != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("create_plan requires (agentId, title)");
                return false;
            }
            m_AgentIdExp = Calculator.Load(callData.GetParam(0));
            m_TitleExp = Calculator.Load(callData.GetParam(1));
            if (funcData.IsHighOrder) {
                for (int ix = 0; ix < funcData.GetParamNum(); ++ix) {
                    m_ItemExps.Add(Calculator.Load(funcData.GetParam(ix)));
                }
            }
            return true;
        }

        protected override BoxedValue DoCalc()
        {
            if (null == m_AgentIdExp || null == m_TitleExp) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("create_plan requires (agentId, title)");
                return BoxedValue.From(false);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(m_AgentIdExp.Calc()));
            if (inst.PlanTrack != null && inst.PlanTrack.PlanState == (int)PlanStateEnum.InProgress) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("create_plan rejected: a plan is in progress, align or finish it first");
                return BoxedValue.From(false);
            }
            inst.PlanTrack = new PlanTrackState();
            inst.PlanTrack.Title = m_TitleExp.Calc().AsString ?? string.Empty;
            inst.PlanTrack.PlanState = (int)PlanStateEnum.Planned;
            foreach (var itemExp in m_ItemExps) {
                inst.PlanTrack.Items.Add(new PlanTrackState.PlanItem { State = (int)PlayItemStateEnum.Planned, Content = itemExp.Calc().AsString ?? string.Empty });
            }
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// add_plan_item(agentId, content) - append an item (planned state only),
    /// returns the 0-based index of the new item (int), -1 on error (details
    /// in the api error info).
    /// </summary>
    sealed class PlanAddItemExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("add_plan_item requires (agentId, content)");
                return BoxedValue.From(-1);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("add_plan_item rejected: no plan, create_plan first");
                return BoxedValue.From(-1);
            }
            var it = new PlanTrackState.PlanItem();
            it.Content = operands[1].AsString ?? string.Empty;
            inst.PlanTrack.Items.Add(it);
            if (inst.PlanTrack.PlanState == (int)PlanStateEnum.InProgress || inst.PlanTrack.PlanState == (int)PlanStateEnum.Done) {
                PlanTrackHelper.PersistPlan(inst);
            }
            return BoxedValue.From(inst.PlanTrack.Items.Count - 1);
        }
    }

    /// <summary>
    /// delete_plan_items(agentId, idx[, idx2, ...]) - delete one or more items
    /// by 0-based index (planned state only); remaining items are renumbered
    /// implicitly. All indexes are validated first; any out-of-range index
    /// rejects the whole call without deleting anything.
    /// </summary>
    sealed class PlanDeleteItemsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_plan_items requires (agentId, idx[, idx2, ...])");
                return BoxedValue.From(-1);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_plan_items rejected: no plan");
                return BoxedValue.From(-1);
            }
            if (inst.PlanTrack.PlanState != (int)PlanStateEnum.Planned) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_plan_items rejected: items frozen after start_plan");
                return BoxedValue.From(-1);
            }
            List<int> indexes;
            if (!PlanNoteUtil.CollectIndexes(operands, 1, inst.PlanTrack.Items.Count, "delete_plan_items", "item", out indexes))
                return BoxedValue.From(-1);
            foreach (int idx in indexes)
                inst.PlanTrack.Items.RemoveAt(idx);
            return BoxedValue.From(indexes.Count);
        }
    }

    /// <summary>
    /// modify_plan_item(agentId, idx, content) - modify item content (planned state only).
    /// </summary>
    sealed class PlanModifyItemExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("modify_plan_item requires (agentId, idx, content)");
                return BoxedValue.From(false);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("modify_plan_item rejected: no plan");
                return BoxedValue.From(false);
            }
            if (inst.PlanTrack.PlanState != (int)PlanStateEnum.Planned) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("modify_plan_item rejected: items frozen after start_plan");
                return BoxedValue.From(false);
            }
            int idx = (int)operands[1].GetInt();
            if (idx < 0 || idx >= inst.PlanTrack.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("modify_plan_item rejected: index out of range");
                return BoxedValue.From(false);
            }
            inst.PlanTrack.Items[idx].Content = operands[2].AsString ?? string.Empty;
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// clear_plan(agentId) - reset every item state to 0 and drop records,
    /// keep item contents (planned state only).
    /// </summary>
    sealed class PlanClearExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("clear_plan requires (agentId)");
                return BoxedValue.From(false);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("clear_plan rejected: no plan");
                return BoxedValue.From(false);
            }
            if (inst.PlanTrack.PlanState != (int)PlanStateEnum.Planned) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("clear_plan rejected: items frozen after start_plan");
                return BoxedValue.From(false);
            }
            foreach (var it in inst.PlanTrack.Items) {
                it.State = (int)PlayItemStateEnum.Planned;
                it.Notes.Clear();
            }
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// start_plan(agentId) - move the plan from planned(0) to in progress(1).
    /// After start, items freeze: only change_plan_item_state may operate.
    /// </summary>
    sealed class PlanStartExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("start_plan requires (agentId)");
                return BoxedValue.From(false);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("start_plan rejected: no plan, create_plan first");
                return BoxedValue.From(false);
            }
            if (inst.PlanTrack.PlanState != (int)PlanStateEnum.Planned) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("start_plan rejected: plan state is not planned(0)");
                return BoxedValue.From(false);
            }
            inst.PlanTrack.PlanState = (int)PlanStateEnum.InProgress;
            PlanTrackHelper.PersistPlan(inst);
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// change_plan_item_state(agentId, idx, state[, note]) - change one item
    /// state (0=planned 1=in progress 2=abandoned 3=done); usable in any plan
    /// state, transitions are reversible. The note text is stored as an
    /// incremental note with a timestamp prefix added automatically.
    /// </summary>
    sealed class PlanChangeItemStateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3 && operands.Count != 4) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("change_plan_item_state requires (agentId, idx, state[, note])");
                return BoxedValue.From(false);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("change_plan_item_state rejected: no plan");
                return BoxedValue.From(false);
            }
            int idx = (int)operands[1].GetInt();
            if (idx < 0 || idx >= inst.PlanTrack.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("change_plan_item_state rejected: index out of range");
                return BoxedValue.From(false);
            }
            int state = (int)operands[2].GetInt();
            if (inst.PlanTrack.PlanState == (int)PlanStateEnum.Planned) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("change_plan_item_state must be called after start_plan");
                return BoxedValue.From(false);
            }
            var it = inst.PlanTrack.Items[idx];
            it.State = state;
            string? note = operands.Count > 3 ? operands[3].AsString : null;
            if (!string.IsNullOrEmpty(note))
                it.Notes.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " " + note);
            PlanTrackHelper.PersistPlan(inst);
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// append_plan_note(agentId, note) - append a plan-level note with an
    /// automatic timestamp prefix; usable in any plan state (no modify API,
    /// notes are removed via trim_plan_notes/delete_plan_notes), persisted via
    /// PersistPlan only when the plan state is in progress(1) or done(2);
    /// state 0 keeps the note in memory only.
    /// </summary>
    sealed class PlanAppendNoteExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("append_plan_note requires (agentId, note)");
                return BoxedValue.From(false);
            }
            string note = operands[1].AsString ?? string.Empty;
            if (note.Length == 0) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("append_plan_note rejected: note is empty");
                return BoxedValue.From(false);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("append_plan_note rejected: no plan, create_plan first");
                return BoxedValue.From(false);
            }
            inst.PlanTrack.Notes.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " " + note);
            if (inst.PlanTrack.PlanState == (int)PlanStateEnum.InProgress || inst.PlanTrack.PlanState == (int)PlanStateEnum.Done) {
                PlanTrackHelper.PersistPlan(inst);
            }
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// append_plan_item_note(agentId, itemIdx, note) - append a timestamped
    /// note to the specified item's Notes without changing its state;
    /// usable in any plan state, persisted via PersistPlan.
    /// </summary>
    sealed class PlanAppendItemNoteExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("append_plan_item_note requires (agentId, itemIdx, note)");
                return BoxedValue.From(false);
            }
            int idx = (int)operands[1].GetInt();
            string note = operands[2].AsString ?? string.Empty;
            if (note.Length == 0) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("append_plan_item_note rejected: note is empty");
                return BoxedValue.From(false);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("append_plan_item_note rejected: no plan, create_plan first");
                return BoxedValue.From(false);
            }
            if (idx < 0 || idx >= inst.PlanTrack.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("append_plan_item_note rejected: item index out of range");
                return BoxedValue.From(false);
            }
            var it = inst.PlanTrack.Items[idx];
            it.Notes.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " " + note);
            if (inst.PlanTrack.PlanState == (int)PlanStateEnum.InProgress || inst.PlanTrack.PlanState == (int)PlanStateEnum.Done) {
                PlanTrackHelper.PersistPlan(inst);
            }
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// trim_plan_notes(agentId, keepCount) - keep only the newest keepCount
    /// plan-level notes (the list is chronological) and drop the older ones;
    /// keepCount 0 clears all. Any plan state, persisted like append_plan_note.
    /// </summary>
    sealed class PlanTrimNotesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("trim_plan_notes requires (agentId, keepCount)");
                return BoxedValue.From(-1);
            }
            int keep = (int)operands[1].GetInt();
            if (keep < 0) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("trim_plan_notes rejected: keepCount must be >= 0");
                return BoxedValue.From(-1);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("trim_plan_notes rejected: no plan");
                return BoxedValue.From(-1);
            }
            var notes = inst.PlanTrack.Notes;
            int removed = 0;
            if (keep < notes.Count) {
                removed = notes.Count - keep;
                notes.RemoveRange(0, removed);
            }
            if (inst.PlanTrack.PlanState == (int)PlanStateEnum.InProgress || inst.PlanTrack.PlanState == (int)PlanStateEnum.Done) {
                PlanTrackHelper.PersistPlan(inst);
            }
            return BoxedValue.From(removed);
        }
    }

    /// <summary>
    /// trim_plan_item_notes(agentId, itemIdx, keepCount) - keep only the newest
    /// keepCount notes of the item and drop the older ones; keepCount 0 clears
    /// all. Any plan state, persisted like append_plan_item_note.
    /// </summary>
    sealed class PlanTrimItemNotesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("trim_plan_item_notes requires (agentId, itemIdx, keepCount)");
                return BoxedValue.From(-1);
            }
            int idx = (int)operands[1].GetInt();
            int keep = (int)operands[2].GetInt();
            if (keep < 0) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("trim_plan_item_notes rejected: keepCount must be >= 0");
                return BoxedValue.From(-1);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("trim_plan_item_notes rejected: no plan");
                return BoxedValue.From(-1);
            }
            if (idx < 0 || idx >= inst.PlanTrack.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("trim_plan_item_notes rejected: item index out of range");
                return BoxedValue.From(-1);
            }
            var notes = inst.PlanTrack.Items[idx].Notes;
            int removed = 0;
            if (keep < notes.Count) {
                removed = notes.Count - keep;
                notes.RemoveRange(0, removed);
            }
            if (inst.PlanTrack.PlanState == (int)PlanStateEnum.InProgress || inst.PlanTrack.PlanState == (int)PlanStateEnum.Done) {
                PlanTrackHelper.PersistPlan(inst);
            }
            return BoxedValue.From(removed);
        }
    }

    /// <summary>
    /// delete_plan_notes(agentId, idx[, idx2, ...]) - delete plan-level notes by
    /// 0-based index; all indexes are validated first. Any plan state,
    /// persisted like append_plan_note.
    /// </summary>
    sealed class PlanDeleteNotesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_plan_notes requires (agentId, idx[, idx2, ...])");
                return BoxedValue.From(-1);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_plan_notes rejected: no plan");
                return BoxedValue.From(-1);
            }
            var notes = inst.PlanTrack.Notes;
            List<int> indexes;
            if (!PlanNoteUtil.CollectIndexes(operands, 1, notes.Count, "delete_plan_notes", "note", out indexes))
                return BoxedValue.From(-1);
            foreach (int n in indexes)
                notes.RemoveAt(n);
            if (inst.PlanTrack.PlanState == (int)PlanStateEnum.InProgress || inst.PlanTrack.PlanState == (int)PlanStateEnum.Done) {
                PlanTrackHelper.PersistPlan(inst);
            }
            return BoxedValue.From(indexes.Count);
        }
    }

    /// <summary>
    /// delete_plan_item_notes(agentId, itemIdx, idx[, idx2, ...]) - delete notes
    /// of the item by 0-based index; all indexes are validated first. Any plan
    /// state, persisted like append_plan_item_note.
    /// </summary>
    sealed class PlanDeleteItemNotesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_plan_item_notes requires (agentId, itemIdx, idx[, idx2, ...])");
                return BoxedValue.From(-1);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_plan_item_notes rejected: no plan");
                return BoxedValue.From(-1);
            }
            int idx = (int)operands[1].GetInt();
            if (idx < 0 || idx >= inst.PlanTrack.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_plan_item_notes rejected: item index out of range");
                return BoxedValue.From(-1);
            }
            var notes = inst.PlanTrack.Items[idx].Notes;
            List<int> indexes;
            if (!PlanNoteUtil.CollectIndexes(operands, 2, notes.Count, "delete_plan_item_notes", "note", out indexes))
                return BoxedValue.From(-1);
            foreach (int n in indexes)
                notes.RemoveAt(n);
            if (inst.PlanTrack.PlanState == (int)PlanStateEnum.InProgress || inst.PlanTrack.PlanState == (int)PlanStateEnum.Done) {
                PlanTrackHelper.PersistPlan(inst);
            }
            return BoxedValue.From(indexes.Count);
        }
    }

    /// <summary>
    /// align_plan(agentId) - derive the plan state from item states, regenerate
    /// the plan.txt text into AgentInstance.Plan and return the text.
    /// </summary>
    sealed class PlanAlignExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("align_plan requires (agentId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("align_plan rejected: no plan");
                return BoxedValue.FromString(string.Empty);
            }
            inst.PlanTrack.PlanState = PlanTrackHelper.DerivePlanState(inst.PlanTrack.PlanState, inst.PlanTrack.Items);
            string text = PlanTrackHelper.PersistPlan(inst);
            return BoxedValue.FromString(text);
        }
    }

    /// <summary>
    /// restore_plan_state(agentId, plan_text) - restore the in-memory plan
    /// state from plan.txt text; effective only when the memory state is
    /// empty (idempotent, safe to call on every boot).
    /// </summary>
    sealed class PlanRestoreExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("restore_plan_state requires (agentId, plan_text)");
                return BoxedValue.From(false);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack != null)
                return BoxedValue.From(true);
            var st = PlanTrackHelper.ParsePlanText(operands[1].AsString);
            if (st == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("restore_plan_state rejected: no [PLAN] line found in text");
                return BoxedValue.From(false);
            }
            inst.PlanTrack = st;
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// Registration entry for PlanTrack APIs (moved from AgentStateApi).
    /// </summary>
    public static class PlanTrackApi
    {
        public static void RegisterApis()
        {
            // PlanTrack APIs
            BatchCommand.BatchScript.Register("create_plan",
                "create_plan(agentId, title) or create_plan(agentId, title){\"item1\";\"item2\";...} - Create a new plan track with a title and optionally add items, returns true/false",
                new ExpressionFactoryHelper<PlanCreateExp>());
            BatchCommand.BatchScript.Register("add_plan_item",
                "add_plan_item(agentId, content) - add an item to the current plan, returns its index or -1 on error",
                new ExpressionFactoryHelper<PlanAddItemExp>());
            BatchCommand.BatchScript.Register("delete_plan_items",
                "delete_plan_items(agentId, idx[, idx2, ...]) - delete one or more plan items by index (planned state only), returns the removed count or -1 on error",
                new ExpressionFactoryHelper<PlanDeleteItemsExp>());
            BatchCommand.BatchScript.Register("modify_plan_item",
                "modify_plan_item(agentId, idx, content) - modify the plan item at the index, returns true/false",
                new ExpressionFactoryHelper<PlanModifyItemExp>());
            BatchCommand.BatchScript.Register("clear_plan",
                "clear_plan(agentId) - clear the current plan, returns true/false",
                new ExpressionFactoryHelper<PlanClearExp>());
            BatchCommand.BatchScript.Register("change_plan_item_state",
                "change_plan_item_state(agentId, idx, state[, note]) - change the state of the plan item at the index, returns true/false",
                new ExpressionFactoryHelper<PlanChangeItemStateExp>());
            BatchCommand.BatchScript.Register("start_plan",
                "start_plan(agentId) - start the current plan, returns true/false",
                new ExpressionFactoryHelper<PlanStartExp>());
            BatchCommand.BatchScript.Register("append_plan_note",
                "append_plan_note(agentId, note) - append a timestamped plan-level note (append-only), returns true/false",
                new ExpressionFactoryHelper<PlanAppendNoteExp>());
            BatchCommand.BatchScript.Register("append_plan_item_note",
                "append_plan_item_note(agentId, itemIdx, note) - append a timestamped note to the item without changing state, returns true/false",
                new ExpressionFactoryHelper<PlanAppendItemNoteExp>());
            BatchCommand.BatchScript.Register("trim_plan_notes",
                "trim_plan_notes(agentId, keepCount) - keep only the newest keepCount plan-level notes, drop the older ones (0 clears all), returns the removed count or -1 on error",
                new ExpressionFactoryHelper<PlanTrimNotesExp>());
            BatchCommand.BatchScript.Register("trim_plan_item_notes",
                "trim_plan_item_notes(agentId, itemIdx, keepCount) - keep only the newest keepCount notes of the item (0 clears all), returns the removed count or -1 on error",
                new ExpressionFactoryHelper<PlanTrimItemNotesExp>());
            BatchCommand.BatchScript.Register("delete_plan_notes",
                "delete_plan_notes(agentId, idx[, idx2, ...]) - delete plan-level notes by index, returns the removed count or -1 on error",
                new ExpressionFactoryHelper<PlanDeleteNotesExp>());
            BatchCommand.BatchScript.Register("delete_plan_item_notes",
                "delete_plan_item_notes(agentId, itemIdx, idx[, idx2, ...]) - delete notes of the plan item by index, returns the removed count or -1 on error",
                new ExpressionFactoryHelper<PlanDeleteItemNotesExp>());
            BatchCommand.BatchScript.Register("align_plan",
                "align_plan(agentId) - align the in-memory plan state with docs/plan.txt",
                new ExpressionFactoryHelper<PlanAlignExp>());
            BatchCommand.BatchScript.Register("restore_plan_state",
                "restore_plan_state(agentId, plan_text) - restore the plan state from the given plan text, returns true/false",
                new ExpressionFactoryHelper<PlanRestoreExp>());
        }
    }
}
