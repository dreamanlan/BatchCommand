using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Utils;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    /// <summary>
    /// Regular Expression Script APIs
    /// </summary>
    public static class RegexApi
    {
        public static void RegisterApis()
        {
            // Regex matching APIs
            AgentFrameworkService.Instance.DslEngine!.Register("regex_match", "regex_match(str, regex_pattern, [ignoreCase])", new ExpressionFactoryHelper<RegexMatchExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("regex_is_match", "regex_is_match(str, regex_pattern, [ignoreCase])", new ExpressionFactoryHelper<RegexMatchExp>());

            // Regex replacement APIs
            AgentFrameworkService.Instance.DslEngine!.Register("regex_replace", "regex_replace(str, regex_pattern, replacement, [ignoreCase])", new ExpressionFactoryHelper<RegexReplaceExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("regex_replace_string", "regex_replace_string(str, regex_pattern, replacement, [ignoreCase])", new ExpressionFactoryHelper<RegexReplaceExp>());

            // Regex find all matches
            AgentFrameworkService.Instance.DslEngine!.Register("regex_find_all", "regex_find_all(str, regex_pattern, [ignoreCase]) return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<RegexFindAllExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("regex_matches", "regex_matches(str, regex_pattern, [ignoreCase]) return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<RegexFindAllExp>());

            // File-based regex operations
            AgentFrameworkService.Instance.DslEngine!.Register("regex_replace_in_file", "regex_replace_in_file(path, regex_pattern, replacement, [ignoreCase])", new ExpressionFactoryHelper<RegexReplaceInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("regex_search_file", "regex_search_file(path, regex_pattern, [ignoreCase]) return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<RegexSearchFileExp>());
        }
    }

    /// <summary>
    /// Test if string matches regex pattern
    /// </summary>
    sealed class RegexMatchExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: regex_match(str, regex_pattern, [ignoreCase]), aliased as regex_is_match");
                return BoxedValue.From(false);
            }

            try {
                string str = operands[0].AsString;
                string pattern = operands[1].AsString;
                bool ignoreCase = operands.Count > 2 ? operands[2].GetBool() : true;

                bool result = StringHelper.MatchesPattern(str, pattern, ignoreCase);
                if (!result && File.Exists(str)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: regex_match(str, regex_pattern, [ignoreCase]), aliased as regex_is_match, str must be a string");
                }
                return BoxedValue.From(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error in regex_match: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// Replace string using regex pattern
    /// </summary>
    sealed class RegexReplaceExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: regex_replace(str, regex_pattern, replacement, [ignoreCase]), aliased as regex_replace_string");
                return BoxedValue.EmptyString;
            }

            string str = operands[0].AsString;
            try {
                string pattern = operands[1].AsString;
                string replacement = operands[2].AsString;
                bool ignoreCase = operands.Count > 3 ? operands[3].GetBool() : true;

                if (!StringHelper.MatchesPattern(str, pattern, ignoreCase)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error: pattern not found: {pattern}");
                    if (File.Exists(str)) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: regex_replace(str, regex_pattern, replacement, [ignoreCase]), aliased as regex_replace_string, str must be a string");
                    }
                    return BoxedValue.EmptyString;
                }
                string result = StringHelper.ReplacePattern(str, pattern, replacement, ignoreCase);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error in regex_replace: {ex.Message}");
                if (null != str) {
                    return str;
                }
                return BoxedValue.NullObject;
            }
        }
    }

    /// <summary>
    /// Find all matches of regex pattern in string
    /// </summary>
    sealed class RegexFindAllExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: regex_find_all(str, regex_pattern, [ignoreCase]), returns a list. Please use 'to_string' to convert it to a string, aliased as regex_matches.");
                return BoxedValue.FromObject(new List<string>());
            }

            try {
                string str = operands[0].AsString;
                string pattern = operands[1].AsString;
                bool ignoreCase = operands.Count > 2 ? operands[2].GetBool() : true;

                var matches = StringHelper.FindAllMatches(str, pattern, ignoreCase);
                if (matches.Count == 0 && File.Exists(str)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: regex_find_all(str, regex_pattern, [ignoreCase]), returns a list. Please use 'to_string' to convert it to a string, aliased as regex_matches, str must be a string");
                }
                return BoxedValue.FromObject(matches);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error in regex_find_all: {ex.Message}");
                return BoxedValue.FromObject(new List<string>());
            }
        }
    }

    /// <summary>
    /// Replace file content using regex pattern
    /// </summary>
    sealed class RegexReplaceInFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: regex_replace_in_file(path, regex_pattern, replacement, [ignoreCase])");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                string pattern = operands[1].AsString;
                string replacement = operands[2].AsString;
                bool ignoreCase = operands.Count > 3 ? operands[3].GetBool() : true;

                if (!System.IO.File.Exists(path)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error: File not found: {path}");
                    return BoxedValue.From(false);
                }

                string content = System.IO.File.ReadAllText(path);
                if (!StringHelper.MatchesPattern(content, pattern, ignoreCase)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error: {path} pattern not found: {pattern}");
                    return BoxedValue.From(false);
                }
                string newContent = StringHelper.ReplacePattern(content, pattern, replacement, ignoreCase);
                System.IO.File.WriteAllText(path, newContent);
                return BoxedValue.From(true);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error in regex_replace_in_file: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// Search file content using regex pattern and return all matches
    /// </summary>
    sealed class RegexSearchFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: regex_search_file(path, regex_pattern, [ignoreCase]) return List, use 'to_string' to convert to a string");
                return BoxedValue.FromObject(new List<string>());
            }

            try {
                string path = operands[0].AsString;
                string pattern = operands[1].AsString;
                bool ignoreCase = operands.Count > 2 ? operands[2].GetBool() : true;

                if (!System.IO.File.Exists(path)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error: File not found: {path}");
                    return BoxedValue.FromObject(new List<string>());
                }

                string content = System.IO.File.ReadAllText(path);
                var matches = StringHelper.FindAllMatches(content, pattern, ignoreCase);
                return BoxedValue.FromObject(matches);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error in regex_search_file: {ex.Message}");
                return BoxedValue.FromObject(new List<string>());
            }
        }
    }
}
