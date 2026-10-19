using System;
using System.Collections.Generic;
using AgentCore.Core;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using static AgentCore.ScriptApi.AgentIdResolver;

namespace AgentCore.ScriptApi
{
    internal static class BacklogApiUtil
    {
        // Backlog has no create API: edit APIs lazily create an empty state.
        internal static BacklogTrackState GetOrCreateTrack(AgentInstance inst)
        {
            if (inst.BacklogTrack == null)
                inst.BacklogTrack = new BacklogTrackState();
            return inst.BacklogTrack;
        }

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

    // All edit APIs below persist docs/backlog.txt immediately after a successful edit.

    /// <summary>
    /// add_backlog_item(agentId, title[, state]) - append an item, returns its
    /// 0-based index. state (0=active 1=suspended) defaults to 1; done(2) is
    /// rejected. An active item demotes the previous active item to suspended.
    /// </summary>
    sealed class BacklogAddItemExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2 && operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("add_backlog_item requires (agentId, title[, state])");
                return BoxedValue.FromString("error: missing parameters");
            }
            int state = (int)BacklogItemStateEnum.Suspended;
            if (operands.Count > 2) {
                state = (int)operands[2].GetInt();
                if (state != (int)BacklogItemStateEnum.Active && state != (int)BacklogItemStateEnum.Suspended) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("add_backlog_item rejected: state must be 0(active)/1(suspended)");
                    return BoxedValue.FromString("error: invalid state");
                }
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = BacklogApiUtil.GetOrCreateTrack(inst);
            var it = new BacklogTrackState.BacklogItem();
            it.Title = operands[1].AsString ?? string.Empty;
            it.State = state;
            st.Items.Add(it);
            int idx = st.Items.Count - 1;
            if (state == (int)BacklogItemStateEnum.Active)
                BacklogTrackHelper.SetActive(st, idx);
            BacklogTrackHelper.PersistBacklog(inst);
            return BoxedValue.FromString(idx.ToString());
        }
    }

    /// <summary>
    /// modify_backlog_item(agentId, idx, title) - modify the item title.
    /// </summary>
    sealed class BacklogModifyItemExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("modify_backlog_item requires (agentId, idx, title)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = BacklogApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (idx < 0 || idx >= st.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("modify_backlog_item rejected: index out of range");
                return BoxedValue.FromString("error: index out of range");
            }
            st.Items[idx].Title = operands[2].AsString ?? string.Empty;
            BacklogTrackHelper.PersistBacklog(inst);
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// move_backlog_item(agentId, idx, newIdx) - move the item to newIdx
    /// (priority order); items in between shift by one. State is unchanged.
    /// </summary>
    sealed class BacklogMoveItemExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("move_backlog_item requires (agentId, idx, newIdx)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = BacklogApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            int newIdx = (int)operands[2].GetInt();
            if (idx < 0 || idx >= st.Items.Count || newIdx < 0 || newIdx >= st.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("move_backlog_item rejected: index out of range");
                return BoxedValue.FromString("error: index out of range");
            }
            if (idx != newIdx) {
                var it = st.Items[idx];
                st.Items.RemoveAt(idx);
                st.Items.Insert(newIdx, it);
            }
            BacklogTrackHelper.PersistBacklog(inst);
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// change_backlog_item_state(agentId, idx, state) - change the item state
    /// (0=active 1=suspended 2=done); setting active demotes the previous
    /// active item; setting done removes the item (and its clauses) from the
    /// list, later items are renumbered.
    /// </summary>
    sealed class BacklogChangeItemStateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("change_backlog_item_state requires (agentId, idx, state)");
                return BoxedValue.FromString("error: missing parameters");
            }
            int state = (int)operands[2].GetInt();
            if (!BacklogTrackHelper.IsValidState(state)) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("change_backlog_item_state rejected: state must be 0(active)/1(suspended)/2(done)");
                return BoxedValue.FromString("error: invalid state");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = BacklogApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (idx < 0 || idx >= st.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("change_backlog_item_state rejected: index out of range");
                return BoxedValue.FromString("error: index out of range");
            }
            if (state == (int)BacklogItemStateEnum.Active)
                BacklogTrackHelper.SetActive(st, idx);
            else if (state == (int)BacklogItemStateEnum.Done)
                st.Items.RemoveAt(idx);
            else
                st.Items[idx].State = state;
            BacklogTrackHelper.PersistBacklog(inst);
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// add_backlog_clause(agentId, itemIdx, content) - append a clause to the item,
    /// returns the 0-based clause index.
    /// </summary>
    sealed class BacklogAddClauseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("add_backlog_clause requires (agentId, itemIdx, content)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = BacklogApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (idx < 0 || idx >= st.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("add_backlog_clause rejected: item index out of range");
                return BoxedValue.FromString("error: index out of range");
            }
            var it = st.Items[idx];
            it.Clauses.Add(operands[2].AsString ?? string.Empty);
            BacklogTrackHelper.PersistBacklog(inst);
            return BoxedValue.FromString((it.Clauses.Count - 1).ToString());
        }
    }

    /// <summary>
    /// modify_backlog_clause(agentId, itemIdx, clauseIdx, content) - modify the clause.
    /// </summary>
    sealed class BacklogModifyClauseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 4) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("modify_backlog_clause requires (agentId, itemIdx, clauseIdx, content)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = BacklogApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (idx < 0 || idx >= st.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("modify_backlog_clause rejected: item index out of range");
                return BoxedValue.FromString("error: index out of range");
            }
            var it = st.Items[idx];
            int clauseIdx = (int)operands[2].GetInt();
            if (clauseIdx < 0 || clauseIdx >= it.Clauses.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("modify_backlog_clause rejected: clause index out of range");
                return BoxedValue.FromString("error: clause index out of range");
            }
            it.Clauses[clauseIdx] = operands[3].AsString ?? string.Empty;
            BacklogTrackHelper.PersistBacklog(inst);
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// delete_backlog_clauses(agentId, itemIdx, clauseIdx[, clauseIdx2, ...]) - delete
    /// one or more clauses of the item; later clauses are renumbered. All clause
    /// indexes are validated first; any out-of-range index rejects the whole
    /// call without deleting anything.
    /// </summary>
    sealed class BacklogDeleteClauseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_backlog_clauses requires (agentId, itemIdx, clauseIdx[, clauseIdx2, ...])");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = BacklogApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (idx < 0 || idx >= st.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_backlog_clauses rejected: item index out of range");
                return BoxedValue.FromString("error: index out of range");
            }
            var it = st.Items[idx];
            List<int> indexes;
            if (!BacklogApiUtil.CollectIndexes(operands, 2, it.Clauses.Count, "delete_backlog_clauses", "clause", out indexes))
                return BoxedValue.FromString("error: clause index out of range");
            foreach (int n in indexes)
                it.Clauses.RemoveAt(n);
            BacklogTrackHelper.PersistBacklog(inst);
            return BoxedValue.FromString("ok removed=" + indexes.Count + " left=" + it.Clauses.Count);
        }
    }

    /// <summary>
    /// clear_backlog_clauses(agentId, itemIdx) - remove all clauses of the item.
    /// </summary>
    sealed class BacklogClearClausesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("clear_backlog_clauses requires (agentId, itemIdx)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = BacklogApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (idx < 0 || idx >= st.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("clear_backlog_clauses rejected: item index out of range");
                return BoxedValue.FromString("error: index out of range");
            }
            st.Items[idx].Clauses.Clear();
            BacklogTrackHelper.PersistBacklog(inst);
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// view_active_backlog_item(agentId) - return the active item (with its
    /// index) in the serialized text format; empty string when no item is active.
    /// </summary>
    sealed class BacklogViewActiveItemExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("view_active_backlog_item requires (agentId)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.BacklogTrack == null)
                return BoxedValue.FromString(string.Empty);
            int activeIdx = BacklogTrackHelper.FindActive(inst.BacklogTrack);
            if (activeIdx < 0)
                return BoxedValue.FromString(string.Empty);
            var sb = new System.Text.StringBuilder();
            BacklogTrackHelper.AppendItemText(sb, inst.BacklogTrack.Items[activeIdx], activeIdx);
            return BoxedValue.FromString(sb.ToString());
        }
    }

    /// <summary>
    /// align_backlog(agentId) - regenerate the backlog text (active item first)
    /// into AgentInstance.Backlog, write docs/backlog.txt and return the text.
    /// </summary>
    sealed class BacklogAlignExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("align_backlog requires (agentId)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            BacklogApiUtil.GetOrCreateTrack(inst);
            string text = BacklogTrackHelper.PersistBacklog(inst);
            return BoxedValue.FromString(text);
        }
    }

    /// <summary>
    /// restore_backlog_state(agentId, backlog_text) - restore the in-memory
    /// backlog state from backlog.txt text; effective only when the memory
    /// state is empty (idempotent, safe to call on every boot). Unmarked
    /// lines are dropped.
    /// </summary>
    sealed class BacklogRestoreExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("restore_backlog_state requires (agentId, backlog_text)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.BacklogTrack != null)
                return BoxedValue.FromString("ok");
            inst.BacklogTrack = BacklogTrackHelper.ParseBacklogText(operands[1].AsString);
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// Registration entry for BacklogTrack APIs.
    /// </summary>
    public static class BacklogTrackApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("add_backlog_item",
                "add_backlog_item(agentId, title[, state]) - append a backlog item (state 0=active 1=suspended, default 1), returns its index",
                new ExpressionFactoryHelper<BacklogAddItemExp>());
            BatchCommand.BatchScript.Register("modify_backlog_item",
                "modify_backlog_item(agentId, idx, title) - modify the backlog item title at the index",
                new ExpressionFactoryHelper<BacklogModifyItemExp>());
            BatchCommand.BatchScript.Register("move_backlog_item",
                "move_backlog_item(agentId, idx, newIdx) - move the backlog item to the new index (priority order)",
                new ExpressionFactoryHelper<BacklogMoveItemExp>());
            BatchCommand.BatchScript.Register("change_backlog_item_state",
                "change_backlog_item_state(agentId, idx, state) - change the backlog item state (0=active 1=suspended 2=done, done removes the item)",
                new ExpressionFactoryHelper<BacklogChangeItemStateExp>());
            BatchCommand.BatchScript.Register("add_backlog_clause",
                "add_backlog_clause(agentId, itemIdx, content) - append a clause to the backlog item, returns its index",
                new ExpressionFactoryHelper<BacklogAddClauseExp>());
            BatchCommand.BatchScript.Register("modify_backlog_clause",
                "modify_backlog_clause(agentId, itemIdx, clauseIdx, content) - modify the clause of the backlog item",
                new ExpressionFactoryHelper<BacklogModifyClauseExp>());
            BatchCommand.BatchScript.Register("delete_backlog_clauses",
                "delete_backlog_clauses(agentId, itemIdx, clauseIdx[, clauseIdx2, ...]) - delete one or more clauses of the backlog item",
                new ExpressionFactoryHelper<BacklogDeleteClauseExp>());
            BatchCommand.BatchScript.Register("clear_backlog_clauses",
                "clear_backlog_clauses(agentId, itemIdx) - remove all clauses of the backlog item",
                new ExpressionFactoryHelper<BacklogClearClausesExp>());
            BatchCommand.BatchScript.Register("view_active_backlog_item",
                "view_active_backlog_item(agentId) - return the active backlog item in serialized text, empty if none",
                new ExpressionFactoryHelper<BacklogViewActiveItemExp>());
            BatchCommand.BatchScript.Register("align_backlog",
                "align_backlog(agentId) - write docs/backlog.txt from the in-memory backlog and return the text",
                new ExpressionFactoryHelper<BacklogAlignExp>());
            BatchCommand.BatchScript.Register("restore_backlog_state",
                "restore_backlog_state(agentId, backlog_text) - restore the backlog state from the given backlog text",
                new ExpressionFactoryHelper<BacklogRestoreExp>());
        }
    }
}
