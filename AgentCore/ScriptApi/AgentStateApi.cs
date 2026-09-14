using System.Collections.Generic;
using AgentCore.ScriptApi;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using static AgentCore.ScriptApi.AgentIdResolver;
using static AgentCore.ScriptApi.AgentPortResolver;

namespace AgentCore.ScriptApi
{
    /// <summary>
    /// Resolves the first operand of an agent_* api into an agent id string.
    /// Accepts an agent id string ("webagent", "hyarena", ...); a legacy int
    /// port argument is resolved through the port->agent registry and falls
    /// back to the number itself when no server was registered for it.
    /// </summary>
    internal static class AgentIdResolver
    {
        internal static string ResolveAgentId(BoxedValue v)
        {
            try {
                if (v.IsString) {
                    var s = v.AsString ?? string.Empty;
                    if (s.Length > 0)
                        return s;
                    return string.Empty;
                }
                if (!v.IsNullObject) {
                    // numeric (legacy port) argument
                    long n = v.GetInt();
                    if (n > 0 && n <= 65535) {
                        if (Core.AgentCore.Instance.TryResolveAgentId((int)n, out var id))
                            return id;
                        return n.ToString();
                    }
                }
            }
            catch {
            }
            return string.Empty;
        }
    }

    /// <summary>
    /// Resolves an agent_* api argument into a ws server port (for the few
    /// apis that operate on the server, e.g. worker concurrency). Accepts an
    /// int port or an agent id string ("webagent", ...); 0 when unresolvable.
    /// </summary>
    internal static class AgentPortResolver
    {
        internal static int ResolveServerPort(BoxedValue v)
        {
            try {
                if (v.IsString) {
                    var s = v.AsString ?? string.Empty;
                    if (int.TryParse(s, out int p))
                        return p;
                    if (Core.AgentCore.Instance.TryResolvePort(s, out int port))
                        return port;
                    return 0;
                }
                if (!v.IsNullObject) {
                    long n = v.GetInt();
                    if (n > 0 && n <= 65535)
                        return (int)n;
                }
            }
            catch {
            }
            return 0;
        }
    }

    /// <summary>
    /// agent_set_project_dir(agentId, value) - set the current project directory
    /// </summary>
    sealed class AgentSetProjectDirExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_project_dir requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            inst.ProjectDir = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_project_dir(agentId) - get the current project directory
    /// </summary>
    sealed class AgentGetProjectDirExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_project_dir requires (agentId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.FromString(inst.ProjectDir);
        }
    }

    /// <summary>
    /// agent_set_project_identity(agentId, value) - set the project identity string
    /// </summary>
    sealed class AgentSetProjectIdentityExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_project_identity requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            inst.ProjectIdentity = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_project_identity(agentId) - get the project identity string
    /// </summary>
    sealed class AgentGetProjectIdentityExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_project_identity requires (agentId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.FromString(inst.ProjectIdentity);
        }
    }

    /// <summary>
    /// agent_set_foundation_prompt(agentId, value) - set the system prompt text
    /// </summary>
    sealed class AgentSetFoundationPromptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_foundation_prompt requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            inst.FoundationPrompt = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_foundation_prompt(agentId) - get the system prompt text
    /// </summary>
    sealed class AgentGetFoundationPromptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_foundation_prompt requires (agentId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.FromString(inst.FoundationPrompt);
        }
    }

    /// <summary>
    /// agent_set_project_prompt(agentId, value) - set the project prompt text
    /// </summary>
    sealed class AgentSetProjectPromptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_project_prompt requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            inst.ProjectPrompt = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_project_prompt(agentId) - get the project prompt text
    /// </summary>
    sealed class AgentGetProjectPromptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_project_prompt requires (agentId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.FromString(inst.ProjectPrompt);
        }
    }

    /// <summary>
    /// agent_set_emphasize(agentId, value) - set the emphasize text
    /// </summary>
    sealed class AgentSetEmphasizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_emphasize requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            inst.Emphasize = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_emphasize(agentId) - get the emphasize text
    /// </summary>
    sealed class AgentGetEmphasizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_emphasize requires (agentId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.FromString(inst.Emphasize);
        }
    }

    /// <summary>
    /// agent_set_soul(agentId, value) - set the soul text
    /// </summary>
    sealed class AgentSetSoulExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_soul requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            inst.Soul = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_soul(agentId) - get the soul text
    /// </summary>
    sealed class AgentGetSoulExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_soul requires (agentId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.FromString(inst.Soul);
        }
    }

    /// <summary>
    /// agent_set_backlog(agentId, value) - set the current requirement text
    /// </summary>
    sealed class AgentSetBacklogExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_backlog requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            inst.Backlog = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_backlog(agentId) - get the current requirement text
    /// </summary>
    sealed class AgentGetBacklogExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_backlog requires (agentId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.FromString(inst.Backlog);
        }
    }

    /// <summary>
    /// agent_set_plan(agentId, value) - set the plan text
    /// </summary>
    sealed class AgentSetPlanExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_plan requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            inst.Plan = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_plan(agentId) - get the plan text
    /// </summary>
    sealed class AgentGetPlanExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_plan requires (agentId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.FromString(inst.Plan);
        }
    }

    /// <summary>
    /// agent_set_context(agentId, value) - set the context text
    /// </summary>
    sealed class AgentSetContextExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_context requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            inst.Context = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_context(agentId) - get the context text
    /// </summary>
    sealed class AgentGetContextExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_context requires (agentId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.FromString(inst.Context);
        }
    }

    /// <summary>
    /// agent_set_history(agentId, value) - set the history text
    /// </summary>
    sealed class AgentSetHistoryExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_history requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            inst.History = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_history(agentId) - get the history text
    /// </summary>
    sealed class AgentGetHistoryExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_history requires (agentId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.FromString(inst.History);
        }
    }

    /// <summary>
    /// set_max_lines_deleted_by_write_file(value) - global setting
    /// </summary>
    sealed class SetMaxLinesDeletedByWriteFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("set_max_lines_deleted_by_write_file requires (value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            BatchCommand.Api.FrameworkApiAlias.MaxLinesDeletedByWriteFile = operands[0].GetInt();
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// get_max_lines_deleted_by_write_file() - global setting
    /// </summary>
    sealed class GetMaxLinesDeletedByWriteFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.From(BatchCommand.Api.FrameworkApiAlias.MaxLinesDeletedByWriteFile);
        }
    }

    /// <summary>
    /// agent_set_max_result_size(agentId, value) - global setting
    /// </summary>
    sealed class AgentSetMaxResultSizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_max_result_size requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            inst.MaxResultSize = operands[1].GetInt();
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_max_result_size(agentId) - global setting
    /// </summary>
    sealed class AgentGetMaxResultSizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_max_result_size requires (agentId)");
                return BoxedValue.From(0);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.From(inst.MaxResultSize);
        }
    }

    /// <summary>
    /// agent_set_max_context_rounds(agentId, value)
    /// </summary>
    sealed class AgentSetMaxContextRoundsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_max_context_rounds requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            inst.MaxContextRounds = operands[1].GetInt();
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_max_context_rounds(agentId)
    /// </summary>
    sealed class AgentGetMaxContextRoundsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_max_context_rounds requires (agentId)");
                return BoxedValue.From(3);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.From(inst.MaxContextRounds);
        }
    }

    /// <summary>
    /// agent_set_cur_context_rounds(agentId, value)
    /// </summary>
    sealed class AgentSetCurContextRoundsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_cur_context_rounds requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            inst.CurContextRounds = operands[1].GetInt();
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_cur_context_rounds(agentId)
    /// </summary>
    sealed class AgentGetCurContextRoundsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_cur_context_rounds requires (agentId)");
                return BoxedValue.From(0);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.From(inst.CurContextRounds);
        }
    }

    /// <summary>
    /// agent_add_cur_context_rounds(agentId) - atomically increment by 1 mod MaxContextRounds, return new value
    /// </summary>
    sealed class AgentAddCurContextRoundsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_add_cur_context_rounds requires (agentId)");
                return BoxedValue.From(0);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.From(inst.AddCurContextRounds());
        }
    }

    /// <summary>
    /// agent_enable_context_injection(agentId, value) - enable/disable context injection in MetaDSL results
    /// </summary>
    sealed class AgentEnableContextInjectionExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_enable_context_injection requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            var val = operands[1].GetString();
            inst.ContextInjectionEnabled = val == "true" || val == "1" || val == "True";
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_is_context_injection_enabled(agentId) - check if context injection is enabled
    /// </summary>
    sealed class AgentIsContextInjectionEnabledExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_is_context_injection_enabled requires (agentId)");
                return BoxedValue.From(true);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.From(inst.ContextInjectionEnabled);
        }
    }

    /// <summary>
    /// agent_set_max_worker_concurrency(agentId, value)
    /// </summary>
    sealed class AgentSetMaxWorkerConcurrencyExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_max_worker_concurrency requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            int port = ResolveServerPort(operands[0]);
            WebSocketServerManager.GetServer(port).MaxWorkerConcurrency = operands[1].GetInt();
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_max_worker_concurrency(agentId)
    /// </summary>
    sealed class AgentGetMaxWorkerConcurrencyExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_max_worker_concurrency requires (agentId)");
                return BoxedValue.From(16);
            }
            int port = ResolveServerPort(operands[0]);
            return BoxedValue.From(WebSocketServerManager.GetServer(port).MaxWorkerConcurrency);
        }
    }

    /// <summary>
    /// agent_get_active_workers(agentId) - get the number of currently active MetaDSL worker tasks
    /// </summary>
    sealed class AgentGetActiveWorkersExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_active_workers requires (agentId)");
                return BoxedValue.From(0);
            }
            int port = ResolveServerPort(operands[0]);
            return BoxedValue.From(WebSocketServerManager.GetServer(port).ActiveWorkers);
        }
    }

    /// <summary>
    /// set_agent_environment(category, group, key, value) - global, not per-instance
    /// </summary>
    sealed class SetAgentEnvironmentExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 4) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("set_agent_environment requires (category, group, key, value)");
                return BoxedValue.FromBool(false);
            }
            string category = operands[0].AsString;
            string group = operands[1].AsString;
            string key = operands[2].AsString;
            string value = operands[3].ToString();
            AgentCore.Core.AgentCore.Instance.SetAgentEnvironment(category, group, key, value);
            return BoxedValue.FromBool(true);
        }
    }

    /// <summary>
    /// get_agent_environment_length(category, group, key) - global
    /// </summary>
    sealed class GetAgentEnvironmentLengthExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: get_agent_environment_length(category, group, key)");
                return BoxedValue.From(-1);
            }
            string category = operands[0].AsString;
            string group = operands[1].AsString;
            string key = operands[2].AsString;
            int len = AgentCore.Core.AgentCore.Instance.GetAgentEnvironmentLength(category, group, key);
            return BoxedValue.From(len);
        }
    }

    /// <summary>
    /// apply_agent_environment(category, group) - global
    /// </summary>
    sealed class ApplyAgentEnvironmentExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("apply_agent_environment requires (category, group)");
                return BoxedValue.FromBool(false);
            }
            string category = operands[0].AsString;
            string group = operands[1].AsString;
            AgentCore.Core.AgentCore.Instance.ApplyAgentEnvironment(category, group);
            return BoxedValue.FromBool(true);
        }
    }

    /// <summary>
    /// clear_agent_environment(category, group) - global
    /// </summary>
    sealed class ClearAgentEnvironmentExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("clear_agent_environment requires (category, group)");
                return BoxedValue.FromBool(false);
            }
            string category = operands[0].AsString;
            string group = operands[1].AsString;
            AgentCore.Core.AgentCore.Instance.ClearAgentEnvironment(category, group);
            return BoxedValue.FromBool(true);
        }
    }

    /// <summary>
    /// agent_set_inject_js_code(agentId, value) - set the inject JavaScript code text
    /// </summary>
    sealed class AgentSetInjectJsCodeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_set_inject_js_code requires (agentId, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            inst.InjectJsCode = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_inject_js_code(agentId) - get the inject JavaScript code text
    /// </summary>
    sealed class AgentGetInjectJsCodeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_inject_js_code requires (agentId)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.FromString(inst.InjectJsCode);
        }
    }

    /// <summary>
    /// agent_get_inject_js_code_size(agentId) - get the length of inject JavaScript code
    /// </summary>
    sealed class AgentGetInjectJsCodeSizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("agent_get_inject_js_code_size requires (agentId)");
                return BoxedValue.From(0);
            }
            var inst = AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(ResolveAgentId(operands[0]));
            return BoxedValue.From(inst.InjectJsCode.Length);
        }
    }

    /// <summary>
    /// Registers all agent state DSL APIs
    /// </summary>
    public static class AgentStateApi
    {
        public static void RegisterApis()
        {
            // Instance-level APIs (with agent_ prefix, port parameter)
            BatchCommand.BatchScript.Register("agent_set_project_dir",
                "agent_set_project_dir(agentId, value) - set the current project directory",
                new ExpressionFactoryHelper<AgentSetProjectDirExp>());
            BatchCommand.BatchScript.Register("agent_get_project_dir",
                "agent_get_project_dir(agentId) - get the current project directory",
                new ExpressionFactoryHelper<AgentGetProjectDirExp>());
            BatchCommand.BatchScript.Register("agent_set_project_identity",
                "agent_set_project_identity(agentId, value) - set the project identity string",
                new ExpressionFactoryHelper<AgentSetProjectIdentityExp>());
            BatchCommand.BatchScript.Register("agent_get_project_identity",
                "agent_get_project_identity(agentId) - get the project identity string",
                new ExpressionFactoryHelper<AgentGetProjectIdentityExp>());
            BatchCommand.BatchScript.Register("agent_set_foundation_prompt",
                "agent_set_foundation_prompt(agentId, value) - set the system prompt text",
                false,
                new ExpressionFactoryHelper<AgentSetFoundationPromptExp>());
            BatchCommand.BatchScript.Register("agent_get_foundation_prompt",
                "agent_get_foundation_prompt(agentId) - get the system prompt text",
                new ExpressionFactoryHelper<AgentGetFoundationPromptExp>());
            BatchCommand.BatchScript.Register("agent_set_project_prompt",
                "agent_set_project_prompt(agentId, value) - set the project prompt text",
                false,
                new ExpressionFactoryHelper<AgentSetProjectPromptExp>());
            BatchCommand.BatchScript.Register("agent_get_project_prompt",
                "agent_get_project_prompt(agentId) - get the project prompt text",
                new ExpressionFactoryHelper<AgentGetProjectPromptExp>());
            BatchCommand.BatchScript.Register("agent_set_emphasize",
                "agent_set_emphasize(agentId, value) - set the emphasize text",
                false,
                new ExpressionFactoryHelper<AgentSetEmphasizeExp>());
            BatchCommand.BatchScript.Register("agent_get_emphasize",
                "agent_get_emphasize(agentId) - get the emphasize text",
                new ExpressionFactoryHelper<AgentGetEmphasizeExp>());
            BatchCommand.BatchScript.Register("agent_set_soul",
                "agent_set_soul(agentId, value) - set the soul text",
                false,
                new ExpressionFactoryHelper<AgentSetSoulExp>());
            BatchCommand.BatchScript.Register("agent_get_soul",
                "agent_get_soul(agentId) - get the soul text",
                new ExpressionFactoryHelper<AgentGetSoulExp>());
            BatchCommand.BatchScript.Register("agent_set_backlog",
                "agent_set_backlog(agentId, value) - set the current requirement text",
                false,
                new ExpressionFactoryHelper<AgentSetBacklogExp>());
            BatchCommand.BatchScript.Register("agent_get_backlog",
                "agent_get_backlog(agentId) - get the current requirement text",
                new ExpressionFactoryHelper<AgentGetBacklogExp>());
            BatchCommand.BatchScript.Register("agent_set_plan",
                "agent_set_plan(agentId, value) - set the plan text",
                false,
                new ExpressionFactoryHelper<AgentSetPlanExp>());
            BatchCommand.BatchScript.Register("agent_get_plan",
                "agent_get_plan(agentId) - get the plan text",
                new ExpressionFactoryHelper<AgentGetPlanExp>());
            BatchCommand.BatchScript.Register("agent_set_context",
                "agent_set_context(agentId, value) - set the context text",
                false,
                new ExpressionFactoryHelper<AgentSetContextExp>());
            BatchCommand.BatchScript.Register("agent_get_context",
                "agent_get_context(agentId) - get the context text",
                new ExpressionFactoryHelper<AgentGetContextExp>());
            BatchCommand.BatchScript.Register("agent_set_history",
                "agent_set_history(agentId, value) - set the history text",
                false,
                new ExpressionFactoryHelper<AgentSetHistoryExp>());
            BatchCommand.BatchScript.Register("agent_get_history",
                "agent_get_history(agentId) - get the history text",
                new ExpressionFactoryHelper<AgentGetHistoryExp>());
            BatchCommand.BatchScript.Register("agent_set_inject_js_code",
                "agent_set_inject_js_code(agentId, value) - set the inject JavaScript code text",
                false,
                new ExpressionFactoryHelper<AgentSetInjectJsCodeExp>());
            BatchCommand.BatchScript.Register("agent_get_inject_js_code",
                "agent_get_inject_js_code(agentId) - get the inject JavaScript code text",
                false,
                new ExpressionFactoryHelper<AgentGetInjectJsCodeExp>());
            BatchCommand.BatchScript.Register("agent_get_inject_js_code_size",
                "agent_get_inject_js_code_size(agentId) - get the length of inject JavaScript code",
                false,
                new ExpressionFactoryHelper<AgentGetInjectJsCodeSizeExp>());

            BatchCommand.BatchScript.Register("agent_set_max_context_rounds",
                "agent_set_max_context_rounds(agentId, value) - set how often to append context in WebSocket responses (0=every round)",
                false,
                new ExpressionFactoryHelper<AgentSetMaxContextRoundsExp>());
            BatchCommand.BatchScript.Register("agent_get_max_context_rounds",
                "agent_get_max_context_rounds(agentId) - get the MaxContextRounds value",
                false,
                new ExpressionFactoryHelper<AgentGetMaxContextRoundsExp>());
            BatchCommand.BatchScript.Register("agent_set_cur_context_rounds",
                "agent_set_cur_context_rounds(agentId, value) - set the CurContextRounds counter",
                false,
                new ExpressionFactoryHelper<AgentSetCurContextRoundsExp>());
            BatchCommand.BatchScript.Register("agent_get_cur_context_rounds",
                "agent_get_cur_context_rounds(agentId) - get the CurContextRounds counter",
                false,
                new ExpressionFactoryHelper<AgentGetCurContextRoundsExp>());
            BatchCommand.BatchScript.Register("agent_add_cur_context_rounds",
                "agent_add_cur_context_rounds(agentId) - atomically increment CurContextRounds by 1 mod MaxContextRounds, return new value",
                false,
                new ExpressionFactoryHelper<AgentAddCurContextRoundsExp>());
            BatchCommand.BatchScript.Register("agent_enable_context_injection",
                "agent_enable_context_injection(agentId, value) - enable/disable context injection in MetaDSL results (true/false/1/0)",
                false,
                new ExpressionFactoryHelper<AgentEnableContextInjectionExp>());
            BatchCommand.BatchScript.Register("agent_is_context_injection_enabled",
                "agent_is_context_injection_enabled(agentId) - check if context injection is enabled, returns true/false",
                false,
                new ExpressionFactoryHelper<AgentIsContextInjectionEnabledExp>());
            BatchCommand.BatchScript.Register("agent_set_max_worker_concurrency",
                "agent_set_max_worker_concurrency(agentId, value) - set max concurrent MetaDSL worker tasks (min 1, default 16)",
                false,
                new ExpressionFactoryHelper<AgentSetMaxWorkerConcurrencyExp>());
            BatchCommand.BatchScript.Register("agent_get_max_worker_concurrency",
                "agent_get_max_worker_concurrency(agentId) - get max concurrent MetaDSL worker tasks",
                false,
                new ExpressionFactoryHelper<AgentGetMaxWorkerConcurrencyExp>());
            BatchCommand.BatchScript.Register("agent_get_active_workers",
                "agent_get_active_workers(agentId) - get the number of currently active MetaDSL worker tasks",
                false,
                new ExpressionFactoryHelper<AgentGetActiveWorkersExp>());
            BatchCommand.BatchScript.Register("agent_set_max_result_size",
                "agent_set_max_result_size(agentId, value) - set the MaxResultSize (0=unlimited)",
                false,
                new ExpressionFactoryHelper<AgentSetMaxResultSizeExp>());
            BatchCommand.BatchScript.Register("agent_get_max_result_size",
                "agent_get_max_result_size(agentId) - get the MaxResultSize",
                false,
                new ExpressionFactoryHelper<AgentGetMaxResultSizeExp>());

            // Global APIs (no agent_ prefix, no port parameter)
            BatchCommand.BatchScript.Register("set_max_lines_deleted_by_write_file",
                "set_max_lines_deleted_by_write_file(value) - set the MaxLinesDeletedByWriteFile",
                false,
                new ExpressionFactoryHelper<SetMaxLinesDeletedByWriteFileExp>());
            BatchCommand.BatchScript.Register("get_max_lines_deleted_by_write_file",
                "get_max_lines_deleted_by_write_file() - get the MaxLinesDeletedByWriteFile",
                false,
                new ExpressionFactoryHelper<GetMaxLinesDeletedByWriteFileExp>());
            BatchCommand.BatchScript.Register("set_agent_environment",
                "set_agent_environment(category, group, key, value) - set agent environment value (three-level dict)",
                new ExpressionFactoryHelper<SetAgentEnvironmentExp>());
            BatchCommand.BatchScript.Register("get_agent_environment_length",
                "get_agent_environment_length(category, group, key) - get encrypted string length, -1 if not found",
                new ExpressionFactoryHelper<GetAgentEnvironmentLengthExp>());
            BatchCommand.BatchScript.Register("apply_agent_environment",
                "apply_agent_environment(category, group) - set process-level env vars from agent environment",
                new ExpressionFactoryHelper<ApplyAgentEnvironmentExp>());
            BatchCommand.BatchScript.Register("clear_agent_environment",
                "clear_agent_environment(category, group) - clear process-level env vars set by apply_agent_environment",
                new ExpressionFactoryHelper<ClearAgentEnvironmentExp>());
        }
    }
}
