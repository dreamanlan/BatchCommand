using System.Collections.Generic;
using AgentPlugin.Abstractions;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace AgentCore.ScriptApi
{
    /// <summary>
    /// set_project_dir(value) - set the current project directory
    /// </summary>
    sealed class SetProjectDirExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("set_project_dir requires (value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.ProjectDir = operands[0].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// get_project_dir() - get the current project directory
    /// </summary>
    sealed class GetProjectDirExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromString(CefDotnetApp.AgentCore.Core.AgentCore.Instance.ProjectDir);
        }
    }

    /// <summary>
    /// set_system_prompt(value) - set the system prompt text
    /// </summary>
    sealed class SetSystemPromptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("set_system_prompt requires (value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.SystemPrompt = operands[0].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// get_system_prompt() - get the system prompt text
    /// </summary>
    sealed class GetSystemPromptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromString(CefDotnetApp.AgentCore.Core.AgentCore.Instance.SystemPrompt);
        }
    }

    /// <summary>
    /// set_project_prompt(value) - set the project prompt text
    /// </summary>
    sealed class SetProjectPromptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("set_project_prompt requires (value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.ProjectPrompt = operands[0].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// get_project_prompt() - get the project prompt text
    /// </summary>
    sealed class GetProjectPromptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromString(CefDotnetApp.AgentCore.Core.AgentCore.Instance.ProjectPrompt);
        }
    }

    /// <summary>
    /// set_plan(value) - set the current plan text
    /// </summary>
    sealed class SetPlanExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("set_plan requires (value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.Plan = operands[0].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// get_plan() - get the current plan text
    /// </summary>
    sealed class GetPlanExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromString(CefDotnetApp.AgentCore.Core.AgentCore.Instance.Plan);
        }
    }

    /// <summary>
    /// set_emphasize(value) - set the emphasize text
    /// </summary>
    sealed class SetEmphasizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("set_emphasize requires (value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.Emphasize = operands[0].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// get_emphasize() - get the emphasize text
    /// </summary>
    sealed class GetEmphasizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromString(CefDotnetApp.AgentCore.Core.AgentCore.Instance.Emphasize);
        }
    }

    /// <summary>
    /// set_todo(value) - set the todo text
    /// </summary>
    sealed class SetToDoExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("set_todo requires (value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.ToDo = operands[0].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// get_todo() - get the todo text
    /// </summary>
    sealed class GetToDoExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromString(CefDotnetApp.AgentCore.Core.AgentCore.Instance.ToDo);
        }
    }

    /// <summary>
    /// set_context(value) - set the context text
    /// </summary>
    sealed class SetContextExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("set_context requires (value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.Context = operands[0].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// get_context() - get the context text
    /// </summary>
    sealed class GetContextExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromString(CefDotnetApp.AgentCore.Core.AgentCore.Instance.Context);
        }
    }

    /// <summary>
    /// set_history(value) - set the history text
    /// </summary>
    sealed class SetHistoryExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("set_history requires (value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.History = operands[0].AsString;
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// get_history() - get the history text
    /// </summary>
    sealed class GetHistoryExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromString(CefDotnetApp.AgentCore.Core.AgentCore.Instance.History);
        }
    }

    /// <summary>
    /// set_max_lines_deleted_by_write_file(value)
    /// </summary>
    sealed class SetMaxLinesDeletedByWriteFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("set_max_lines_deleted_by_write_file requires (value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.MaxLinesDeletedByWriteFile = operands[0].GetInt();
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// get_max_lines_deleted_by_write_file()
    /// </summary>
    sealed class GetMaxLinesDeletedByWriteFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.From(CefDotnetApp.AgentCore.Core.AgentCore.Instance.MaxLinesDeletedByWriteFile);
        }
    }

    /// <summary>
    /// set_max_result_size(value)
    /// </summary>
    sealed class SetMaxResultSizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("set_max_result_size requires (value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.MaxResultSize = operands[0].GetInt();
            return BoxedValue.FromString("ok");
        }
    }

    /// <summary>
    /// get_max_result_size()
    /// </summary>
    sealed class GetMaxResultSizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.From(CefDotnetApp.AgentCore.Core.AgentCore.Instance.MaxResultSize);
        }
    }

    /// <summary>
    /// set_agent_environment(category, group, key, value)
    /// Sets a value in the three-level agent environment dictionary.
    /// Returns true.
    /// </summary>
    sealed class SetAgentEnvironmentExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("set_agent_environment requires (category, group, key, value)");
                return BoxedValue.FromBool(false);
            }
            string category = operands[0].AsString;
            string group = operands[1].AsString;
            string key = operands[2].AsString;
            string value = operands[3].AsString;
            CefDotnetApp.AgentCore.Core.AgentCore.Instance.SetAgentEnvironment(category, group, key, value);
            return BoxedValue.FromBool(true);
        }
    }

    /// <summary>
    /// get_agent_environment_length(category, group, key)
    /// Returns the length of the encrypted string, or -1 if not found.
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
    /// Registers all agent state DSL APIs
    /// </summary>
    public static class AgentStateApi
    {
        public static void RegisterApis()
        {
            AgentFrameworkService.Instance.DslEngine!.Register("set_project_dir",
                "set_project_dir(value) - set the current project directory",
                new ExpressionFactoryHelper<SetProjectDirExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_project_dir",
                "get_project_dir() - get the current project directory",
                new ExpressionFactoryHelper<GetProjectDirExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_system_prompt",
                "set_system_prompt(value) - set the system prompt text",
                false,
                new ExpressionFactoryHelper<SetSystemPromptExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_system_prompt",
                "get_system_prompt() - get the system prompt text",
                new ExpressionFactoryHelper<GetSystemPromptExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_project_prompt",
                "set_project_prompt(value) - set the project prompt text",
                false,
                new ExpressionFactoryHelper<SetProjectPromptExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_project_prompt",
                "get_project_prompt() - get the project prompt text",
                new ExpressionFactoryHelper<GetProjectPromptExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_plan",
                "set_plan(value) - set the current plan text",
                false,
                new ExpressionFactoryHelper<SetPlanExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_plan",
                "get_plan() - get the current plan text",
                new ExpressionFactoryHelper<GetPlanExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_emphasize",
                "set_emphasize(value) - set the emphasize text",
                false,
                new ExpressionFactoryHelper<SetEmphasizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_emphasize",
                "get_emphasize() - get the emphasize text",
                new ExpressionFactoryHelper<GetEmphasizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_todo",
                "set_todo(value) - set the todo text",
                false,
                new ExpressionFactoryHelper<SetToDoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_todo",
                "get_todo() - get the todo text",
                new ExpressionFactoryHelper<GetToDoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_context",
                "set_context(value) - set the context text",
                false,
                new ExpressionFactoryHelper<SetContextExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_context",
                "get_context() - get the context text",
                new ExpressionFactoryHelper<GetContextExp>());

            AgentFrameworkService.Instance.DslEngine!.Register("set_history",
                "set_history(value) - set the history text",
                false,
                new ExpressionFactoryHelper<SetHistoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_history",
                "get_history() - get the history text",
                new ExpressionFactoryHelper<GetHistoryExp>());

            AgentFrameworkService.Instance.DslEngine!.Register("set_max_lines_deleted_by_write_file",
                "set_max_lines_deleted_by_write_file(value) - set the MaxLinesDeletedByWriteFile",
                false,
                new ExpressionFactoryHelper<SetMaxLinesDeletedByWriteFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_max_lines_deleted_by_write_file",
                "get_max_lines_deleted_by_write_file() - get the MaxLinesDeletedByWriteFile",
                false,
                new ExpressionFactoryHelper<GetMaxLinesDeletedByWriteFileExp>());

            AgentFrameworkService.Instance.DslEngine!.Register("set_max_result_size",
                "set_max_result_size(value) - set the MaxResultSize (0=unlimited)",
                false,
                new ExpressionFactoryHelper<SetMaxResultSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_max_result_size",
                "get_max_result_size() - get the MaxResultSize",
                false,
                new ExpressionFactoryHelper<GetMaxResultSizeExp>());

            AgentFrameworkService.Instance.DslEngine!.Register("set_agent_environment",
                "set_agent_environment(category, group, key, value) - set agent environment value (three-level dict)",
                new ExpressionFactoryHelper<SetAgentEnvironmentExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_agent_environment_length",
                "get_agent_environment_length(category, group, key) - get encrypted string length, -1 if not found",
                new ExpressionFactoryHelper<GetAgentEnvironmentLengthExp>());
        }
    }
}
