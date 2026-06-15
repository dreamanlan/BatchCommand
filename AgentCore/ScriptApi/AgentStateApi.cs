using System.Collections.Generic;
using AgentPlugin.Abstractions;
using CefDotnetApp.AgentCore.ScriptApi;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace AgentCore.ScriptApi
{
    /// <summary>
    /// agent_set_project_dir(port, value) - set the current project directory
    /// </summary>
    sealed class AgentSetProjectDirExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_project_dir requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            inst.ProjectDir = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_project_dir(port) - get the current project directory
    /// </summary>
    sealed class AgentGetProjectDirExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_project_dir requires (port)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.FromString(inst.ProjectDir);
        }
    }

    /// <summary>
    /// agent_set_project_identity(port, value) - set the project identity string
    /// </summary>
    sealed class AgentSetProjectIdentityExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_project_identity requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            inst.ProjectIdentity = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_project_identity(port) - get the project identity string
    /// </summary>
    sealed class AgentGetProjectIdentityExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_project_identity requires (port)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.FromString(inst.ProjectIdentity);
        }
    }

    /// <summary>
    /// agent_set_system_prompt(port, value) - set the system prompt text
    /// </summary>
    sealed class AgentSetSystemPromptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_system_prompt requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            inst.SystemPrompt = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_system_prompt(port) - get the system prompt text
    /// </summary>
    sealed class AgentGetSystemPromptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_system_prompt requires (port)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.FromString(inst.SystemPrompt);
        }
    }

    /// <summary>
    /// agent_set_project_prompt(port, value) - set the project prompt text
    /// </summary>
    sealed class AgentSetProjectPromptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_project_prompt requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            inst.ProjectPrompt = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_project_prompt(port) - get the project prompt text
    /// </summary>
    sealed class AgentGetProjectPromptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_project_prompt requires (port)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.FromString(inst.ProjectPrompt);
        }
    }

    /// <summary>
    /// agent_set_plan(port, value) - set the current plan text
    /// </summary>
    sealed class AgentSetPlanExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_plan requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            inst.Plan = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_plan(port) - get the current plan text
    /// </summary>
    sealed class AgentGetPlanExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_plan requires (port)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.FromString(inst.Plan);
        }
    }

    /// <summary>
    /// agent_set_emphasize(port, value) - set the emphasize text
    /// </summary>
    sealed class AgentSetEmphasizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_emphasize requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            inst.Emphasize = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_emphasize(port) - get the emphasize text
    /// </summary>
    sealed class AgentGetEmphasizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_emphasize requires (port)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.FromString(inst.Emphasize);
        }
    }

    /// <summary>
    /// agent_set_todo(port, value) - set the todo text
    /// </summary>
    sealed class AgentSetToDoExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_todo requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            inst.ToDo = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_todo(port) - get the todo text
    /// </summary>
    sealed class AgentGetToDoExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_todo requires (port)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.FromString(inst.ToDo);
        }
    }

    /// <summary>
    /// agent_set_context(port, value) - set the context text
    /// </summary>
    sealed class AgentSetContextExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_context requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            inst.Context = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_context(port) - get the context text
    /// </summary>
    sealed class AgentGetContextExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_context requires (port)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.FromString(inst.Context);
        }
    }

    /// <summary>
    /// agent_set_history(port, value) - set the history text
    /// </summary>
    sealed class AgentSetHistoryExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_history requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            inst.History = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_history(port) - get the history text
    /// </summary>
    sealed class AgentGetHistoryExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_history requires (port)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("set_max_lines_deleted_by_write_file requires (value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.MaxLinesDeletedByWriteFile = operands[0].GetInt();
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
            return BoxedValue.From(CefDotnetApp.AgentCore.Core.AgentCore.Instance.MaxLinesDeletedByWriteFile);
        }
    }

    /// <summary>
    /// agent_set_max_result_size(port, value) - global setting
    /// </summary>
    sealed class AgentSetMaxResultSizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_max_result_size requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            inst.MaxResultSize = operands[1].GetInt();
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_max_result_size(port) - global setting
    /// </summary>
    sealed class AgentGetMaxResultSizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_max_result_size requires (port)");
                return BoxedValue.From(0);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.From(inst.MaxResultSize);
        }
    }

    /// <summary>
    /// agent_set_max_context_rounds(port, value)
    /// </summary>
    sealed class AgentSetMaxContextRoundsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_max_context_rounds requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            inst.MaxContextRounds = operands[1].GetInt();
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_max_context_rounds(port)
    /// </summary>
    sealed class AgentGetMaxContextRoundsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_max_context_rounds requires (port)");
                return BoxedValue.From(3);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.From(inst.MaxContextRounds);
        }
    }

    /// <summary>
    /// agent_set_cur_context_rounds(port, value)
    /// </summary>
    sealed class AgentSetCurContextRoundsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_cur_context_rounds requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            inst.CurContextRounds = operands[1].GetInt();
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_cur_context_rounds(port)
    /// </summary>
    sealed class AgentGetCurContextRoundsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_cur_context_rounds requires (port)");
                return BoxedValue.From(0);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.From(inst.CurContextRounds);
        }
    }

    /// <summary>
    /// agent_add_cur_context_rounds(port) - atomically increment by 1 mod MaxContextRounds, return new value
    /// </summary>
    sealed class AgentAddCurContextRoundsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_add_cur_context_rounds requires (port)");
                return BoxedValue.From(0);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.From(inst.AddCurContextRounds());
        }
    }

    /// <summary>
    /// agent_enable_context_injection(port, value) - enable/disable context injection in MetaDSL results
    /// </summary>
    sealed class AgentEnableContextInjectionExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_enable_context_injection requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            var val = operands[1].GetString();
            inst.ContextInjectionEnabled = val == "true" || val == "1" || val == "True";
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_is_context_injection_enabled(port) - check if context injection is enabled
    /// </summary>
    sealed class AgentIsContextInjectionEnabledExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_is_context_injection_enabled requires (port)");
                return BoxedValue.From(true);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.From(inst.ContextInjectionEnabled);
        }
    }

    /// <summary>
    /// agent_set_max_worker_concurrency(port, value)
    /// </summary>
    sealed class AgentSetMaxWorkerConcurrencyExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_max_worker_concurrency requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            int port = operands[0].GetInt();
            WebSocketServerManager.GetServer(port).MaxWorkerConcurrency = operands[1].GetInt();
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_max_worker_concurrency(port)
    /// </summary>
    sealed class AgentGetMaxWorkerConcurrencyExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_max_worker_concurrency requires (port)");
                return BoxedValue.From(16);
            }
            int port = operands[0].GetInt();
            return BoxedValue.From(WebSocketServerManager.GetServer(port).MaxWorkerConcurrency);
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("set_agent_environment requires (category, group, key, value)");
                return BoxedValue.FromBool(false);
            }
            string category = operands[0].AsString;
            string group = operands[1].AsString;
            string key = operands[2].AsString;
            string value = operands[3].ToString();
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.SetAgentEnvironment(category, group, key, value);
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_agent_environment_length(category, group, key)");
                return BoxedValue.From(-1);
            }
            string category = operands[0].AsString;
            string group = operands[1].AsString;
            string key = operands[2].AsString;
            int len = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetAgentEnvironmentLength(category, group, key);
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("apply_agent_environment requires (category, group)");
                return BoxedValue.FromBool(false);
            }
            string category = operands[0].AsString;
            string group = operands[1].AsString;
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.ApplyAgentEnvironment(category, group);
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("clear_agent_environment requires (category, group)");
                return BoxedValue.FromBool(false);
            }
            string category = operands[0].AsString;
            string group = operands[1].AsString;
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.ClearAgentEnvironment(category, group);
            return BoxedValue.FromBool(true);
        }
    }

    /// <summary>
    /// agent_set_inject_js_code(port, value) - set the inject JavaScript code text
    /// </summary>
    sealed class AgentSetInjectJsCodeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_inject_js_code requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            inst.InjectJsCode = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_inject_js_code(port) - get the inject JavaScript code text
    /// </summary>
    sealed class AgentGetInjectJsCodeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_inject_js_code requires (port)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.FromString(inst.InjectJsCode);
        }
    }

    /// <summary>
    /// agent_get_inject_js_code_size(port) - get the length of inject JavaScript code
    /// </summary>
    sealed class AgentGetInjectJsCodeSizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_inject_js_code_size requires (port)");
                return BoxedValue.From(0);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.From(inst.InjectJsCode.Length);
        }
    }

    /// <summary>
    /// agent_set_soul(port, value) - set the soul text
    /// </summary>
    sealed class AgentSetSoulExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_set_soul requires (port, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            inst.Soul = operands[1].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// agent_get_soul(port) - get the soul text
    /// </summary>
    sealed class AgentGetSoulExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("agent_get_soul requires (port)");
                return BoxedValue.FromString(string.Empty);
            }
            var inst = CefDotnetApp.AgentCore.Core.AgentCore.Instance.GetOrCreateInstance(operands[0].GetInt());
            return BoxedValue.FromString(inst.Soul);
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
            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_project_dir",
                "agent_set_project_dir(port, value) - set the current project directory",
                new ExpressionFactoryHelper<AgentSetProjectDirExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_project_dir",
                "agent_get_project_dir(port) - get the current project directory",
                new ExpressionFactoryHelper<AgentGetProjectDirExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_project_identity",
                "agent_set_project_identity(port, value) - set the project identity string",
                new ExpressionFactoryHelper<AgentSetProjectIdentityExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_project_identity",
                "agent_get_project_identity(port) - get the project identity string",
                new ExpressionFactoryHelper<AgentGetProjectIdentityExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_system_prompt",
                "agent_set_system_prompt(port, value) - set the system prompt text",
                false,
                new ExpressionFactoryHelper<AgentSetSystemPromptExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_system_prompt",
                "agent_get_system_prompt(port) - get the system prompt text",
                new ExpressionFactoryHelper<AgentGetSystemPromptExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_project_prompt",
                "agent_set_project_prompt(port, value) - set the project prompt text",
                false,
                new ExpressionFactoryHelper<AgentSetProjectPromptExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_project_prompt",
                "agent_get_project_prompt(port) - get the project prompt text",
                new ExpressionFactoryHelper<AgentGetProjectPromptExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_plan",
                "agent_set_plan(port, value) - set the current plan text",
                false,
                new ExpressionFactoryHelper<AgentSetPlanExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_plan",
                "agent_get_plan(port) - get the current plan text",
                new ExpressionFactoryHelper<AgentGetPlanExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_emphasize",
                "agent_set_emphasize(port, value) - set the emphasize text",
                false,
                new ExpressionFactoryHelper<AgentSetEmphasizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_emphasize",
                "agent_get_emphasize(port) - get the emphasize text",
                new ExpressionFactoryHelper<AgentGetEmphasizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_todo",
                "agent_set_todo(port, value) - set the todo text",
                false,
                new ExpressionFactoryHelper<AgentSetToDoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_todo",
                "agent_get_todo(port) - get the todo text",
                new ExpressionFactoryHelper<AgentGetToDoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_context",
                "agent_set_context(port, value) - set the context text",
                false,
                new ExpressionFactoryHelper<AgentSetContextExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_context",
                "agent_get_context(port) - get the context text",
                new ExpressionFactoryHelper<AgentGetContextExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_history",
                "agent_set_history(port, value) - set the history text",
                false,
                new ExpressionFactoryHelper<AgentSetHistoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_history",
                "agent_get_history(port) - get the history text",
                new ExpressionFactoryHelper<AgentGetHistoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_inject_js_code",
                "agent_set_inject_js_code(port, value) - set the inject JavaScript code text",
                false,
                new ExpressionFactoryHelper<AgentSetInjectJsCodeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_inject_js_code",
                "agent_get_inject_js_code(port) - get the inject JavaScript code text",
                false,
                new ExpressionFactoryHelper<AgentGetInjectJsCodeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_inject_js_code_size",
                "agent_get_inject_js_code_size(port) - get the length of inject JavaScript code",
                false,
                new ExpressionFactoryHelper<AgentGetInjectJsCodeSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_soul",
                "agent_set_soul(port, value) - set the soul text",
                false,
                new ExpressionFactoryHelper<AgentSetSoulExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_soul",
                "agent_get_soul(port) - get the soul text",
                new ExpressionFactoryHelper<AgentGetSoulExp>());

            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_max_context_rounds",
                "agent_set_max_context_rounds(port, value) - set how often to append context in WebSocket responses (0=every round)",
                false,
                new ExpressionFactoryHelper<AgentSetMaxContextRoundsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_max_context_rounds",
                "agent_get_max_context_rounds(port) - get the MaxContextRounds value",
                false,
                new ExpressionFactoryHelper<AgentGetMaxContextRoundsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_cur_context_rounds",
                "agent_set_cur_context_rounds(port, value) - set the CurContextRounds counter",
                false,
                new ExpressionFactoryHelper<AgentSetCurContextRoundsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_cur_context_rounds",
                "agent_get_cur_context_rounds(port) - get the CurContextRounds counter",
                false,
                new ExpressionFactoryHelper<AgentGetCurContextRoundsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_add_cur_context_rounds",
                "agent_add_cur_context_rounds(port) - atomically increment CurContextRounds by 1 mod MaxContextRounds, return new value",
                false,
                new ExpressionFactoryHelper<AgentAddCurContextRoundsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_enable_context_injection",
                "agent_enable_context_injection(port, value) - enable/disable context injection in MetaDSL results (true/false/1/0)",
                false,
                new ExpressionFactoryHelper<AgentEnableContextInjectionExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_is_context_injection_enabled",
                "agent_is_context_injection_enabled(port) - check if context injection is enabled, returns true/false",
                false,
                new ExpressionFactoryHelper<AgentIsContextInjectionEnabledExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_max_worker_concurrency",
                "agent_set_max_worker_concurrency(port, value) - set max concurrent MetaDSL worker tasks (min 1, default 16)",
                false,
                new ExpressionFactoryHelper<AgentSetMaxWorkerConcurrencyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_max_worker_concurrency",
                "agent_get_max_worker_concurrency(port) - get max concurrent MetaDSL worker tasks",
                false,
                new ExpressionFactoryHelper<AgentGetMaxWorkerConcurrencyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_set_max_result_size",
                "agent_set_max_result_size(port, value) - set the MaxResultSize (0=unlimited)",
                false,
                new ExpressionFactoryHelper<AgentSetMaxResultSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("agent_get_max_result_size",
                "agent_get_max_result_size(port) - get the MaxResultSize",
                false,
                new ExpressionFactoryHelper<AgentGetMaxResultSizeExp>());

            // Global APIs (no agent_ prefix, no port parameter)
            AgentFrameworkService.Instance.DslEngine!.Register("set_max_lines_deleted_by_write_file",
                "set_max_lines_deleted_by_write_file(value) - set the MaxLinesDeletedByWriteFile",
                false,
                new ExpressionFactoryHelper<SetMaxLinesDeletedByWriteFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_max_lines_deleted_by_write_file",
                "get_max_lines_deleted_by_write_file() - get the MaxLinesDeletedByWriteFile",
                false,
                new ExpressionFactoryHelper<GetMaxLinesDeletedByWriteFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_agent_environment",
                "set_agent_environment(category, group, key, value) - set agent environment value (three-level dict)",
                new ExpressionFactoryHelper<SetAgentEnvironmentExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_agent_environment_length",
                "get_agent_environment_length(category, group, key) - get encrypted string length, -1 if not found",
                new ExpressionFactoryHelper<GetAgentEnvironmentLengthExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("apply_agent_environment",
                "apply_agent_environment(category, group) - set process-level env vars from agent environment",
                new ExpressionFactoryHelper<ApplyAgentEnvironmentExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("clear_agent_environment",
                "clear_agent_environment(category, group) - clear process-level env vars set by apply_agent_environment",
                new ExpressionFactoryHelper<ClearAgentEnvironmentExp>());
        }
    }
}
