using System;
using System.Collections.Generic;
using AgentCore.Core;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using static AgentCore.ScriptApi.AgentIdResolver;

namespace AgentCore.ScriptApi
{
    internal static class PatternApiUtil
    {
        // Pattern library has no create API: edit APIs lazily create an empty state.
        internal static PatternTrackState GetOrCreateTrack(AgentInstance inst)
        {
            if (inst.PatternTrack == null)
                inst.PatternTrack = new PatternTrackState();
            return inst.PatternTrack;
        }

        // Persist docs/patterns.txt and return "ok (total=.../30720 bytes, items=.../64)".
        internal static BoxedValue PersistAndOk(AgentInstance inst)
        {
            string text = PatternTrackHelper.PersistPatterns(inst);
            return BoxedValue.FromString("ok (" + PatternTrackHelper.GetStatsText(inst.PatternTrack!, text) + ")");
        }

        internal static bool CheckItemIndex(PatternTrackState st, int idx, string api)
        {
            if (idx < 0 || idx >= st.Items.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine(api + " rejected: item index out of range");
                return false;
            }
            return true;
        }

        internal static bool CheckClauseIndex(PatternTrackState.PatternItem it, int clauseIdx, string api)
        {
            if (clauseIdx < 0 || clauseIdx >= it.Clauses.Count) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine(api + " rejected: clause index out of range");
                return false;
            }
            return true;
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

    // All edit APIs below persist docs/patterns.txt immediately after a successful edit.

    /// <summary>
    /// add_pattern(agentId, title) - append a pattern, returns its 0-based index.
    /// Never rejected by the MaxItems(64) limit; over-limit is only reported
    /// by the stats text of other edit APIs and list_patterns.
    /// </summary>
    sealed class PatternAddItemExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("add_pattern requires (agentId, title)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            var it = new PatternTrackState.PatternItem();
            it.Title = operands[1].AsString ?? string.Empty;
            st.Items.Add(it);
            PatternTrackHelper.PersistPatterns(inst);
            return BoxedValue.FromString((st.Items.Count - 1).ToString());
        }
    }

    /// <summary>
    /// modify_pattern(agentId, idx, title) - modify the pattern title.
    /// </summary>
    sealed class PatternModifyItemExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("modify_pattern requires (agentId, idx, title)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "modify_pattern"))
                return BoxedValue.FromString("error: index out of range");
            st.Items[idx].Title = operands[2].AsString ?? string.Empty;
            return PatternApiUtil.PersistAndOk(inst);
        }
    }

    /// <summary>
    /// delete_patterns(agentId, idx[, idx2, ...]) - delete one or more patterns
    /// (with their clauses); later patterns are renumbered. All indexes are
    /// validated first; any out-of-range index rejects the whole call.
    /// </summary>
    sealed class PatternDeleteItemExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_patterns requires (agentId, idx[, idx2, ...])");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            List<int> indexes;
            if (!PatternApiUtil.CollectIndexes(operands, 1, st.Items.Count, "delete_patterns", "pattern", out indexes))
                return BoxedValue.FromString("error: index out of range");
            foreach (int idx in indexes)
                st.Items.RemoveAt(idx);
            string text = PatternTrackHelper.PersistPatterns(inst);
            return BoxedValue.FromString("ok removed=" + indexes.Count + " (" + PatternTrackHelper.GetStatsText(inst.PatternTrack!, text) + ")");
        }
    }

    /// <summary>
    /// move_pattern(agentId, idx, newIdx) - move the pattern to newIdx;
    /// patterns in between shift by one.
    /// </summary>
    sealed class PatternMoveItemExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("move_pattern requires (agentId, idx, newIdx)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            int newIdx = (int)operands[2].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "move_pattern") || !PatternApiUtil.CheckItemIndex(st, newIdx, "move_pattern"))
                return BoxedValue.FromString("error: index out of range");
            if (idx != newIdx) {
                var it = st.Items[idx];
                st.Items.RemoveAt(idx);
                st.Items.Insert(newIdx, it);
            }
            return PatternApiUtil.PersistAndOk(inst);
        }
    }

    /// <summary>
    /// merge_patterns(agentId, srcIdx, dstIdx) - append the clauses of src to dst
    /// (in order), then delete src. Returns the new index of dst (dst - 1 when
    /// src is before dst).
    /// </summary>
    sealed class PatternMergeItemsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("merge_patterns requires (agentId, srcIdx, dstIdx)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int srcIdx = (int)operands[1].GetInt();
            int dstIdx = (int)operands[2].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, srcIdx, "merge_patterns") || !PatternApiUtil.CheckItemIndex(st, dstIdx, "merge_patterns"))
                return BoxedValue.FromString("error: index out of range");
            if (srcIdx == dstIdx) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("merge_patterns rejected: srcIdx equals dstIdx");
                return BoxedValue.FromString("error: same index");
            }
            st.Items[dstIdx].Clauses.AddRange(st.Items[srcIdx].Clauses);
            st.Items.RemoveAt(srcIdx);
            int newDstIdx = srcIdx < dstIdx ? dstIdx - 1 : dstIdx;
            PatternTrackHelper.PersistPatterns(inst);
            return BoxedValue.FromString(newDstIdx.ToString());
        }
    }

    /// <summary>
    /// set_pattern_clauses(agentId, idx, clause1, clause2, ...) - replace all clauses of
    /// the pattern with the given clauses (no clause means clear).
    /// </summary>
    sealed class PatternSetClausesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("set_pattern_clauses requires (agentId, idx, clause1, clause2, ...)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "set_pattern_clauses"))
                return BoxedValue.FromString("error: index out of range");
            var it = st.Items[idx];
            it.Clauses.Clear();
            for (int i = 2; i < operands.Count; i++)
                it.Clauses.Add(operands[i].AsString ?? string.Empty);
            return PatternApiUtil.PersistAndOk(inst);
        }
    }

    /// <summary>
    /// add_pattern_clause(agentId, idx, content) - append a clause to the pattern,
    /// returns the 0-based clause index.
    /// </summary>
    sealed class PatternAddClauseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("add_pattern_clause requires (agentId, idx, content)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "add_pattern_clause"))
                return BoxedValue.FromString("error: index out of range");
            var it = st.Items[idx];
            it.Clauses.Add(operands[2].AsString ?? string.Empty);
            PatternTrackHelper.PersistPatterns(inst);
            return BoxedValue.FromString((it.Clauses.Count - 1).ToString());
        }
    }

    /// <summary>
    /// modify_pattern_clause(agentId, idx, clauseIdx, content) - modify the clause.
    /// </summary>
    sealed class PatternModifyClauseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 4) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("modify_pattern_clause requires (agentId, idx, clauseIdx, content)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "modify_pattern_clause"))
                return BoxedValue.FromString("error: index out of range");
            var it = st.Items[idx];
            int clauseIdx = (int)operands[2].GetInt();
            if (!PatternApiUtil.CheckClauseIndex(it, clauseIdx, "modify_pattern_clause"))
                return BoxedValue.FromString("error: clause index out of range");
            it.Clauses[clauseIdx] = operands[3].AsString ?? string.Empty;
            return PatternApiUtil.PersistAndOk(inst);
        }
    }

    /// <summary>
    /// delete_pattern_clauses(agentId, idx, clauseIdx[, clauseIdx2, ...]) - delete one
    /// or more clauses of the pattern; later clauses are renumbered. All clause
    /// indexes are validated first.
    /// </summary>
    sealed class PatternDeleteClauseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("delete_pattern_clauses requires (agentId, idx, clauseIdx[, clauseIdx2, ...])");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "delete_pattern_clauses"))
                return BoxedValue.FromString("error: index out of range");
            var it = st.Items[idx];
            List<int> indexes;
            if (!PatternApiUtil.CollectIndexes(operands, 2, it.Clauses.Count, "delete_pattern_clauses", "clause", out indexes))
                return BoxedValue.FromString("error: clause index out of range");
            foreach (int n in indexes)
                it.Clauses.RemoveAt(n);
            string text = PatternTrackHelper.PersistPatterns(inst);
            return BoxedValue.FromString("ok removed=" + indexes.Count + " (" + PatternTrackHelper.GetStatsText(inst.PatternTrack!, text) + ")");
        }
    }

    /// <summary>
    /// clear_pattern_clauses(agentId, idx) - remove all clauses of the pattern.
    /// </summary>
    sealed class PatternClearClausesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("clear_pattern_clauses requires (agentId, idx)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "clear_pattern_clauses"))
                return BoxedValue.FromString("error: index out of range");
            st.Items[idx].Clauses.Clear();
            return PatternApiUtil.PersistAndOk(inst);
        }
    }

    /// <summary>
    /// list_patterns(agentId) - overview lines "[idx] title (clauses=N, bytes=B)"
    /// followed by a stats line "total=.../30720 bytes, items=.../64".
    /// </summary>
    sealed class PatternListExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("list_patterns requires (agentId)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = inst.PatternTrack ?? new PatternTrackState();
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < st.Items.Count; i++) {
                var it = st.Items[i];
                string title = it.Title.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
                sb.Append('[').Append(i).Append("] ").Append(title)
                    .Append(" (clauses=").Append(it.Clauses.Count)
                    .Append(", bytes=").Append(PatternTrackHelper.GetClauseBytes(it, i)).AppendLine(")");
            }
            sb.Append(PatternTrackHelper.GetStatsText(st, PatternTrackHelper.GeneratePatternText(st)));
            return BoxedValue.FromString(sb.ToString());
        }
    }

    /// <summary>
    /// view_pattern(agentId, idx) - return the pattern (with its clauses) in the
    /// serialized text format.
    /// </summary>
    sealed class PatternViewItemExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("view_pattern requires (agentId, idx)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = inst.PatternTrack ?? new PatternTrackState();
            int idx = (int)operands[1].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "view_pattern"))
                return BoxedValue.FromString("error: index out of range");
            var sb = new System.Text.StringBuilder();
            PatternTrackHelper.AppendItemText(sb, st.Items[idx], idx);
            return BoxedValue.FromString(sb.ToString());
        }
    }

    /// <summary>
    /// align_patterns(agentId) - write docs/patterns.txt from the in-memory
    /// pattern library and return the text.
    /// </summary>
    sealed class PatternAlignExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("align_patterns requires (agentId)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            PatternApiUtil.GetOrCreateTrack(inst);
            string text = PatternTrackHelper.PersistPatterns(inst);
            return BoxedValue.FromString(text);
        }
    }

    /// <summary>
    /// restore_patterns_state(agentId, patterns_text) - restore the in-memory
    /// pattern library from patterns.txt text; effective only when the memory
    /// state is empty (idempotent, safe to call on every boot). Unmarked lines
    /// are dropped.
    /// </summary>
    sealed class PatternRestoreExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("restore_patterns_state requires (agentId, patterns_text)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PatternTrack != null)
                return BoxedValue.FromString("ok");
            inst.PatternTrack = PatternTrackHelper.ParsePatternText(operands[1].AsString);
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// Registration entry for PatternTrack APIs.
    /// </summary>
    public static class PatternTrackApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("add_pattern",
                "add_pattern(agentId, title) - append a pattern (soft limit 64, only warned), returns its index",
                new ExpressionFactoryHelper<PatternAddItemExp>());
            BatchCommand.BatchScript.Register("modify_pattern",
                "modify_pattern(agentId, idx, title) - modify the pattern title at the index",
                new ExpressionFactoryHelper<PatternModifyItemExp>());
            BatchCommand.BatchScript.Register("delete_patterns",
                "delete_patterns(agentId, idx[, idx2, ...]) - delete one or more patterns by index, later patterns are renumbered",
                new ExpressionFactoryHelper<PatternDeleteItemExp>());
            BatchCommand.BatchScript.Register("move_pattern",
                "move_pattern(agentId, idx, newIdx) - move the pattern to the new index",
                new ExpressionFactoryHelper<PatternMoveItemExp>());
            BatchCommand.BatchScript.Register("merge_patterns",
                "merge_patterns(agentId, srcIdx, dstIdx) - append clauses of src to dst then delete src, returns the new dst index",
                new ExpressionFactoryHelper<PatternMergeItemsExp>());
            BatchCommand.BatchScript.Register("set_pattern_clauses",
                "set_pattern_clauses(agentId, idx, clause1, clause2, ...) - replace all clauses of the pattern",
                new ExpressionFactoryHelper<PatternSetClausesExp>());
            BatchCommand.BatchScript.Register("add_pattern_clause",
                "add_pattern_clause(agentId, idx, content) - append a clause to the pattern, returns its index",
                new ExpressionFactoryHelper<PatternAddClauseExp>());
            BatchCommand.BatchScript.Register("modify_pattern_clause",
                "modify_pattern_clause(agentId, idx, clauseIdx, content) - modify the clause of the pattern",
                new ExpressionFactoryHelper<PatternModifyClauseExp>());
            BatchCommand.BatchScript.Register("delete_pattern_clauses",
                "delete_pattern_clauses(agentId, idx, clauseIdx[, clauseIdx2, ...]) - delete one or more clauses of the pattern",
                new ExpressionFactoryHelper<PatternDeleteClauseExp>());
            BatchCommand.BatchScript.Register("clear_pattern_clauses",
                "clear_pattern_clauses(agentId, idx) - remove all clauses of the pattern",
                new ExpressionFactoryHelper<PatternClearClausesExp>());
            BatchCommand.BatchScript.Register("list_patterns",
                "list_patterns(agentId) - list pattern titles with clause count/bytes and the total size/count stats",
                new ExpressionFactoryHelper<PatternListExp>());
            BatchCommand.BatchScript.Register("view_pattern",
                "view_pattern(agentId, idx) - return the pattern with its clauses in serialized text",
                new ExpressionFactoryHelper<PatternViewItemExp>());
            BatchCommand.BatchScript.Register("align_patterns",
                "align_patterns(agentId) - write docs/patterns.txt from the in-memory pattern library and return the text",
                new ExpressionFactoryHelper<PatternAlignExp>());
            BatchCommand.BatchScript.Register("restore_patterns_state",
                "restore_patterns_state(agentId, patterns_text) - restore the pattern library from the given patterns text",
                new ExpressionFactoryHelper<PatternRestoreExp>());
        }
    }
}
