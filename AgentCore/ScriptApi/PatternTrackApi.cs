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

        // Persist docs/patterns.txt; when the library is over budget, append a
        // non-error notice with the stats to the api error info.
        internal static void PersistAndNotice(AgentInstance inst, string api)
        {
            string text = PatternTrackHelper.PersistPatterns(inst);
            var st = inst.PatternTrack!;
            if (PatternTrackHelper.IsOverBudget(st, text))
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine(api + " succeeded (notice, not an error, do not retry): " + PatternTrackHelper.GetStatsText(st, text));
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
    /// add_pattern(agentId, title) - append a pattern, returns its 0-based index
    /// (int), -1 on error (details in the api error info).
    /// Never rejected by the MaxItems limit; over-limit is only reported
    /// by the stats text of other edit APIs and list_patterns.
    /// </summary>
    sealed class PatternAddItemExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("add_pattern requires (agentId, title)");
                return BoxedValue.From(-1);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            var it = new PatternTrackState.PatternItem();
            it.Title = operands[1].AsString ?? string.Empty;
            st.Items.Add(it);
            PatternTrackHelper.PersistPatterns(inst);
            return BoxedValue.From(st.Items.Count - 1);
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
                return BoxedValue.From(false);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "modify_pattern"))
                return BoxedValue.From(false);
            st.Items[idx].Title = operands[2].AsString ?? string.Empty;
            PatternApiUtil.PersistAndNotice(inst, "modify_pattern");
            return BoxedValue.From(true);
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
                return BoxedValue.From(-1);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            List<int> indexes;
            if (!PatternApiUtil.CollectIndexes(operands, 1, st.Items.Count, "delete_patterns", "pattern", out indexes))
                return BoxedValue.From(-1);
            foreach (int idx in indexes)
                st.Items.RemoveAt(idx);
            PatternApiUtil.PersistAndNotice(inst, "delete_patterns");
            return BoxedValue.From(indexes.Count);
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
                return BoxedValue.From(false);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            int newIdx = (int)operands[2].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "move_pattern") || !PatternApiUtil.CheckItemIndex(st, newIdx, "move_pattern"))
                return BoxedValue.From(false);
            if (idx != newIdx) {
                var it = st.Items[idx];
                st.Items.RemoveAt(idx);
                st.Items.Insert(newIdx, it);
            }
            PatternApiUtil.PersistAndNotice(inst, "move_pattern");
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// merge_patterns(agentId, srcIdx, dstIdx) - append the clauses of src to dst
    /// (in order), then delete src. Returns the new index of dst (int, dst - 1
    /// when src is before dst), -1 on error (details in the api error info).
    /// </summary>
    sealed class PatternMergeItemsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("merge_patterns requires (agentId, srcIdx, dstIdx)");
                return BoxedValue.From(-1);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int srcIdx = (int)operands[1].GetInt();
            int dstIdx = (int)operands[2].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, srcIdx, "merge_patterns") || !PatternApiUtil.CheckItemIndex(st, dstIdx, "merge_patterns"))
                return BoxedValue.From(-1);
            if (srcIdx == dstIdx) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("merge_patterns rejected: srcIdx equals dstIdx");
                return BoxedValue.From(-1);
            }
            st.Items[dstIdx].Clauses.AddRange(st.Items[srcIdx].Clauses);
            st.Items.RemoveAt(srcIdx);
            int newDstIdx = srcIdx < dstIdx ? dstIdx - 1 : dstIdx;
            PatternTrackHelper.PersistPatterns(inst);
            return BoxedValue.From(newDstIdx);
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
                return BoxedValue.From(false);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "set_pattern_clauses"))
                return BoxedValue.From(false);
            var it = st.Items[idx];
            it.Clauses.Clear();
            for (int i = 2; i < operands.Count; i++)
                it.Clauses.Add(operands[i].AsString ?? string.Empty);
            PatternApiUtil.PersistAndNotice(inst, "set_pattern_clauses");
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// add_pattern_clause(agentId, idx, content) - append a clause to the pattern,
    /// returns the 0-based clause index (int), -1 on error (details in the api
    /// error info).
    /// </summary>
    sealed class PatternAddClauseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("add_pattern_clause requires (agentId, idx, content)");
                return BoxedValue.From(-1);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "add_pattern_clause"))
                return BoxedValue.From(-1);
            var it = st.Items[idx];
            it.Clauses.Add(operands[2].AsString ?? string.Empty);
            PatternTrackHelper.PersistPatterns(inst);
            return BoxedValue.From(it.Clauses.Count - 1);
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
                return BoxedValue.From(false);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "modify_pattern_clause"))
                return BoxedValue.From(false);
            var it = st.Items[idx];
            int clauseIdx = (int)operands[2].GetInt();
            if (!PatternApiUtil.CheckClauseIndex(it, clauseIdx, "modify_pattern_clause"))
                return BoxedValue.From(false);
            it.Clauses[clauseIdx] = operands[3].AsString ?? string.Empty;
            PatternApiUtil.PersistAndNotice(inst, "modify_pattern_clause");
            return BoxedValue.From(true);
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
                return BoxedValue.From(-1);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "delete_pattern_clauses"))
                return BoxedValue.From(-1);
            var it = st.Items[idx];
            List<int> indexes;
            if (!PatternApiUtil.CollectIndexes(operands, 2, it.Clauses.Count, "delete_pattern_clauses", "clause", out indexes))
                return BoxedValue.From(-1);
            foreach (int n in indexes)
                it.Clauses.RemoveAt(n);
            PatternApiUtil.PersistAndNotice(inst, "delete_pattern_clauses");
            return BoxedValue.From(indexes.Count);
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
                return BoxedValue.From(false);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = PatternApiUtil.GetOrCreateTrack(inst);
            int idx = (int)operands[1].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "clear_pattern_clauses"))
                return BoxedValue.From(false);
            st.Items[idx].Clauses.Clear();
            PatternApiUtil.PersistAndNotice(inst, "clear_pattern_clauses");
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// list_patterns(agentId) - overview lines "[idx] title (clauses=N, bytes=B)"
    /// followed by a stats line "total=.../MaxBytes bytes, items=.../MaxItems".
    /// </summary>
    sealed class PatternListExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("list_patterns requires (agentId)");
                return BoxedValue.FromString(string.Empty);
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
                return BoxedValue.FromString(string.Empty);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var st = inst.PatternTrack ?? new PatternTrackState();
            int idx = (int)operands[1].GetInt();
            if (!PatternApiUtil.CheckItemIndex(st, idx, "view_pattern"))
                return BoxedValue.FromString(string.Empty);
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
                return BoxedValue.FromString(string.Empty);
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
                return BoxedValue.From(false);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            if (inst.PatternTrack != null)
                return BoxedValue.From(true);
            inst.PatternTrack = PatternTrackHelper.ParsePatternText(operands[1].AsString);
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// set_patterns_max(maxCount, maxSize) - global setting: soft limits of the
    /// pattern library (item count and total bytes), both must be positive.
    /// Not exposed in the user API doc (for script_agent.dsl only).
    /// </summary>
    sealed class PatternSetMaxExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("set_patterns_max requires (maxCount, maxSize)");
                return BoxedValue.From(false);
            }
            int maxCount = (int)operands[0].GetInt();
            int maxSize = (int)operands[1].GetInt();
            if (maxCount <= 0 || maxSize <= 0) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("set_patterns_max rejected: maxCount and maxSize must be positive");
                return BoxedValue.From(false);
            }
            PatternTrackHelper.MaxItems = maxCount;
            PatternTrackHelper.MaxBytes = maxSize;
            return BoxedValue.From(true);
        }
    }

    /// <summary>
    /// get_patterns_max_count() - global setting: soft limit of the pattern item count.
    /// </summary>
    sealed class PatternGetMaxCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.From(PatternTrackHelper.MaxItems);
        }
    }

    /// <summary>
    /// get_patterns_max_size() - global setting: soft limit of the pattern library total bytes.
    /// </summary>
    sealed class PatternGetMaxSizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.From(PatternTrackHelper.MaxBytes);
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
                "add_pattern(agentId, title) - append a pattern (soft count limit, only warned), returns its index or -1 on error",
                new ExpressionFactoryHelper<PatternAddItemExp>());
            BatchCommand.BatchScript.Register("modify_pattern",
                "modify_pattern(agentId, idx, title) - modify the pattern title at the index, returns true/false",
                new ExpressionFactoryHelper<PatternModifyItemExp>());
            BatchCommand.BatchScript.Register("delete_patterns",
                "delete_patterns(agentId, idx[, idx2, ...]) - delete one or more patterns by index, later patterns are renumbered, returns the removed count or -1 on error",
                new ExpressionFactoryHelper<PatternDeleteItemExp>());
            BatchCommand.BatchScript.Register("move_pattern",
                "move_pattern(agentId, idx, newIdx) - move the pattern to the new index, returns true/false",
                new ExpressionFactoryHelper<PatternMoveItemExp>());
            BatchCommand.BatchScript.Register("merge_patterns",
                "merge_patterns(agentId, srcIdx, dstIdx) - append clauses of src to dst then delete src, returns the new dst index or -1 on error",
                new ExpressionFactoryHelper<PatternMergeItemsExp>());
            BatchCommand.BatchScript.Register("set_pattern_clauses",
                "set_pattern_clauses(agentId, idx, clause1, clause2, ...) - replace all clauses of the pattern, returns true/false",
                new ExpressionFactoryHelper<PatternSetClausesExp>());
            BatchCommand.BatchScript.Register("add_pattern_clause",
                "add_pattern_clause(agentId, idx, content) - append a clause to the pattern, returns its index or -1 on error",
                new ExpressionFactoryHelper<PatternAddClauseExp>());
            BatchCommand.BatchScript.Register("modify_pattern_clause",
                "modify_pattern_clause(agentId, idx, clauseIdx, content) - modify the clause of the pattern, returns true/false",
                new ExpressionFactoryHelper<PatternModifyClauseExp>());
            BatchCommand.BatchScript.Register("delete_pattern_clauses",
                "delete_pattern_clauses(agentId, idx, clauseIdx[, clauseIdx2, ...]) - delete one or more clauses of the pattern, returns the removed count or -1 on error",
                new ExpressionFactoryHelper<PatternDeleteClauseExp>());
            BatchCommand.BatchScript.Register("clear_pattern_clauses",
                "clear_pattern_clauses(agentId, idx) - remove all clauses of the pattern, returns true/false",
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
                "restore_patterns_state(agentId, patterns_text) - restore the pattern library from the given patterns text, returns true/false",
                new ExpressionFactoryHelper<PatternRestoreExp>());
            BatchCommand.BatchScript.Register("set_patterns_max",
                "set_patterns_max(maxCount, maxSize) - set the soft limits of the pattern library (item count, total bytes), returns true/false",
                false,
                new ExpressionFactoryHelper<PatternSetMaxExp>());
            BatchCommand.BatchScript.Register("get_patterns_max_count",
                "get_patterns_max_count() - get the soft limit of the pattern item count",
                false,
                new ExpressionFactoryHelper<PatternGetMaxCountExp>());
            BatchCommand.BatchScript.Register("get_patterns_max_size",
                "get_patterns_max_size() - get the soft limit of the pattern library total bytes",
                false,
                new ExpressionFactoryHelper<PatternGetMaxSizeExp>());
        }
    }
}
