using System;
using System.Collections.Generic;
using AgentPlugin.Abstractions;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // refresh_embedding()
    sealed class RefreshEmbeddingExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                string result = Core.AgentCore.Instance.RefreshEmbedding();
                return BoxedValue.FromString(result ?? string.Empty);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"RefreshEmbedding error: {ex.Message}");
                return BoxedValue.FromString($"[error] {ex.Message}");
            }
        }
    }

    // refresh_reranker()
    sealed class RefreshRerankExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                string result = Core.AgentCore.Instance.RefreshReranker();
                return BoxedValue.FromString(result ?? string.Empty);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"RefreshReranker error: {ex.Message}");
                return BoxedValue.FromString($"[error] {ex.Message}");
            }
        }
    }

    // refresh_skills()
    sealed class RefreshSkillsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                string result = Core.AgentCore.Instance.RefreshSkills(
                    System.IO.Path.Combine(Core.AgentCore.Instance.BasePath, "skills"));
                return BoxedValue.FromString(result ?? string.Empty);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"RefreshSkills error: {ex.Message}");
                return BoxedValue.FromString($"[error] {ex.Message}");
            }
        }
    }

    // call_skill(skill_name, arg1, arg2, ...)
    sealed class CallSkillExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: call_skill(skill_name, arg1, arg2, ...), aliased as callskill");
                return BoxedValue.FromString("[error] call_skill requires at least skill name");
            }

            try {
                string skillName = operands[0].AsString;
                var args = new List<string>();
                for (int i = 1; i < operands.Count; i++) {
                    args.Add(operands[i].ToString());
                }
                string result = Core.AgentCore.Instance.SkillMgr.CallSkill(skillName, args);
                return BoxedValue.FromString(result ?? string.Empty);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"CallSkill error: {ex.Message}");
                return BoxedValue.FromString($"[error] {ex.Message}");
            }
        }
    }

    // set_skill_env(key, value)
    sealed class SetSkillEnvExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: set_skill_env(key, value)");
                return BoxedValue.FromBool(false);
            }
            try {
                string key = operands[0].AsString;
                string value = operands[1].AsString;
                return BoxedValue.FromBool(Core.AgentCore.Instance.SkillMgr.SetEnv(key, value));
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"SetSkillEnv error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    // get_skill_env(key[, defval])
    sealed class GetSkillEnvExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_skill_env(key[, defval])");
                return BoxedValue.FromString(string.Empty);
            }
            try {
                string key = operands[0].AsString;
                string defval = operands.Count > 1 ? operands[1].AsString : string.Empty;
                return BoxedValue.FromString(Core.AgentCore.Instance.SkillMgr.GetEnv(key, defval));
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"GetSkillEnv error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    // delete_skill_env(key)
    sealed class DeleteSkillEnvExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: delete_skill_env(key)");
                return BoxedValue.FromBool(false);
            }
            try {
                string key = operands[0].AsString;
                return BoxedValue.FromBool(Core.AgentCore.Instance.SkillMgr.DeleteEnv(key));
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"DeleteSkillEnv error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    // clear_skill_envs([regexPattern])
    sealed class ClearSkillEnvsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count > 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: clear_skill_envs([regexPattern])");
                return BoxedValue.FromBool(false);
            }
            try {
                string? pattern = operands.Count > 0 ? operands[0].AsString : null;
                int removed = Core.AgentCore.Instance.SkillMgr.ClearEnvs(pattern);
                return BoxedValue.FromBool(removed >= 0);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"ClearSkillEnvs error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }
}
