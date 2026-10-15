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
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(m_AgentIdExp.Calc()));
            if (inst.PlanTrack != null && inst.PlanTrack.PlanState == (int)PlanStateEnum.InProgress) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("create_plan rejected: a plan is in progress, align or finish it first");
                return BoxedValue.FromString("error: plan in progress");
            }
            inst.PlanTrack = new PlanTrackState();
            inst.PlanTrack.Title = m_TitleExp.Calc().AsString ?? string.Empty;
            inst.PlanTrack.PlanState = (int)PlanStateEnum.Planned;
            foreach (var itemExp in m_ItemExps) {
                inst.PlanTrack.Items.Add(new PlanTrackState.PlanItem { State = (int)PlayItemStateEnum.Planned, Content = itemExp.Calc().AsString ?? string.Empty });
            }
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// add_plan_item(agentId, content) - append an item (planned state only),
    /// returns the 0-based index of the new item.
    /// </summary>
    sealed class PlanAddItemExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("add_plan_item requires (agentId, content)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("add_plan_item rejected: no plan, create_plan first");
                return BoxedValue.FromString("error: no plan");
            }
            var it = new PlanTrackState.PlanItem();
            it.Content = operands[1].AsString ?? string.Empty;
            inst.PlanTrack.Items.Add(it);
            if (inst.PlanTrack.PlanState == (int)PlanStateEnum.InProgress || inst.PlanTrack.PlanState == (int)PlanStateEnum.Done) {
                PlanTrackHelper.PersistPlan(inst);
            }
            return BoxedValue.FromString((inst.PlanTrack.Items.Count - 1).ToString());
        }
    }

    /// <summary>
    /// delete_plan_item(agentId, idx) - delete an item by 0-based index
    /// (planned state only); remaining items are renumbered implicitly.
    /// </summary>
    sealed class PlanDeleteItemExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_plan_item requires (agentId, idx)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_plan_item rejected: no plan");
                return BoxedValue.FromString("error: no plan");
            }
            if (inst.PlanTrack.PlanState != (int)PlanStateEnum.Planned) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_plan_item rejected: items frozen after start_plan");
                return BoxedValue.FromString("error: items frozen");
            }
            int idx = (int)operands[1].GetInt();
            if (idx < 0 || idx >= inst.PlanTrack.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_plan_item rejected: index out of range");
                return BoxedValue.FromString("error: index out of range");
            }
            inst.PlanTrack.Items.RemoveAt(idx);
            return BoxedValue.FromString("ok");
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
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("modify_plan_item rejected: no plan");
                return BoxedValue.FromString("error: no plan");
            }
            if (inst.PlanTrack.PlanState != (int)PlanStateEnum.Planned) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("modify_plan_item rejected: items frozen after start_plan");
                return BoxedValue.FromString("error: items frozen");
            }
            int idx = (int)operands[1].GetInt();
            if (idx < 0 || idx >= inst.PlanTrack.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("modify_plan_item rejected: index out of range");
                return BoxedValue.FromString("error: index out of range");
            }
            inst.PlanTrack.Items[idx].Content = operands[2].AsString ?? string.Empty;
            return BoxedValue.FromString("ok");
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
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("clear_plan rejected: no plan");
                return BoxedValue.FromString("error: no plan");
            }
            if (inst.PlanTrack.PlanState != (int)PlanStateEnum.Planned) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("clear_plan rejected: items frozen after start_plan");
                return BoxedValue.FromString("error: items frozen");
            }
            foreach (var it in inst.PlanTrack.Items) {
                it.State = (int)PlayItemStateEnum.Planned;
                it.Notes.Clear();
            }
            return BoxedValue.FromString("ok");
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
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("start_plan rejected: no plan, create_plan first");
                return BoxedValue.FromString("error: no plan");
            }
            if (inst.PlanTrack.PlanState != (int)PlanStateEnum.Planned) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("start_plan rejected: plan state is not planned(0)");
                return BoxedValue.FromString("error: plan not in planned state");
            }
            inst.PlanTrack.PlanState = (int)PlanStateEnum.InProgress;
            PlanTrackHelper.PersistPlan(inst);
            return BoxedValue.FromString("ok");
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
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("change_plan_item_state rejected: no plan");
                return BoxedValue.FromString("error: no plan");
            }
            int idx = (int)operands[1].GetInt();
            if (idx < 0 || idx >= inst.PlanTrack.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("change_plan_item_state rejected: index out of range");
                return BoxedValue.FromString("error: index out of range");
            }
            int state = (int)operands[2].GetInt();
            if (inst.PlanTrack.PlanState == (int)PlanStateEnum.Planned) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("change_plan_item_state must be called after start_plan");
                return BoxedValue.FromString("error: invalid state");
            }
            var it = inst.PlanTrack.Items[idx];
            it.State = state;
            string? note = operands.Count > 3 ? operands[3].AsString : null;
            if (!string.IsNullOrEmpty(note))
                it.Notes.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " " + note);
            PlanTrackHelper.PersistPlan(inst);
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// append_plan_note(agentId, note) - append a plan-level note with an
    /// automatic timestamp prefix; append-only (any plan state, no delete
    /// or modify API), persisted via PersistPlan only when the plan state
    /// is in progress(1) or done(2); state 0 keeps the note in memory only.
    /// </summary>
    sealed class PlanAppendNoteExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("append_plan_note requires (agentId, note)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string note = operands[1].AsString ?? string.Empty;
            if (note.Length == 0) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("append_plan_note rejected: note is empty");
                return BoxedValue.FromString("error: empty note");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("append_plan_note rejected: no plan, create_plan first");
                return BoxedValue.FromString("error: no plan");
            }
            inst.PlanTrack.Notes.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " " + note);
            if (inst.PlanTrack.PlanState == (int)PlanStateEnum.InProgress || inst.PlanTrack.PlanState == (int)PlanStateEnum.Done) {
                PlanTrackHelper.PersistPlan(inst);
            }
            return BoxedValue.FromString("ok");
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
                return BoxedValue.FromString("error: missing parameters");
            }
            int idx = (int)operands[1].GetInt();
            string note = operands[2].AsString ?? string.Empty;
            if (note.Length == 0) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("append_plan_item_note rejected: note is empty");
                return BoxedValue.FromString("error: empty note");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("append_plan_item_note rejected: no plan, create_plan first");
                return BoxedValue.FromString("error: no plan");
            }
            if (idx < 0 || idx >= inst.PlanTrack.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("append_plan_item_note rejected: item index out of range");
                return BoxedValue.FromString("error: invalid index");
            }
            var it = inst.PlanTrack.Items[idx];
            it.Notes.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " " + note);
            if (inst.PlanTrack.PlanState == (int)PlanStateEnum.InProgress || inst.PlanTrack.PlanState == (int)PlanStateEnum.Done) {
                PlanTrackHelper.PersistPlan(inst);
            }
            return BoxedValue.FromString("ok");
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
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("align_plan rejected: no plan");
                return BoxedValue.FromString("error: no plan");
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
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PlanTrack != null)
                return BoxedValue.FromString("ok");
            var st = PlanTrackHelper.ParsePlanText(operands[1].AsString);
            if (st == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("restore_plan_state rejected: no [PLAN] line found in text");
                return BoxedValue.FromString("error: no plan line");
            }
            inst.PlanTrack = st;
            return BoxedValue.FromString("ok");
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
                "create_plan(agentId, title) or create_plan(agentId, title){\"item1\";\"item2\";...} - Create a new plan track with a title and optionally add items.",
                new ExpressionFactoryHelper<PlanCreateExp>());
            BatchCommand.BatchScript.Register("add_plan_item",
                "add_plan_item(agentId, content) - add an item to the current plan",
                new ExpressionFactoryHelper<PlanAddItemExp>());
            BatchCommand.BatchScript.Register("delete_plan_item",
                "delete_plan_item(agentId, idx) - delete the plan item at the index",
                new ExpressionFactoryHelper<PlanDeleteItemExp>());
            BatchCommand.BatchScript.Register("modify_plan_item",
                "modify_plan_item(agentId, idx, content) - modify the plan item at the index",
                new ExpressionFactoryHelper<PlanModifyItemExp>());
            BatchCommand.BatchScript.Register("clear_plan",
                "clear_plan(agentId) - clear the current plan",
                new ExpressionFactoryHelper<PlanClearExp>());
            BatchCommand.BatchScript.Register("change_plan_item_state",
                "change_plan_item_state(agentId, idx, state[, note]) - change the state of the plan item at the index",
                new ExpressionFactoryHelper<PlanChangeItemStateExp>());
            BatchCommand.BatchScript.Register("start_plan",
                "start_plan(agentId) - start the current plan",
                new ExpressionFactoryHelper<PlanStartExp>());
            BatchCommand.BatchScript.Register("append_plan_note",
                "append_plan_note(agentId, note) - append a timestamped plan-level note (append-only)",
                new ExpressionFactoryHelper<PlanAppendNoteExp>());
            BatchCommand.BatchScript.Register("append_plan_item_note",
                "append_plan_item_note(agentId, itemIdx, note) - append a timestamped note to the item without changing state",
                new ExpressionFactoryHelper<PlanAppendItemNoteExp>());
            BatchCommand.BatchScript.Register("align_plan",
                "align_plan(agentId) - align the in-memory plan state with docs/plan.txt",
                new ExpressionFactoryHelper<PlanAlignExp>());
            BatchCommand.BatchScript.Register("restore_plan_state",
                "restore_plan_state(agentId, plan_text) - restore the plan state from the given plan text",
                new ExpressionFactoryHelper<PlanRestoreExp>());
        }
    }
}
