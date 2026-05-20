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
            AgentFrameworkService.Instance.DslEngine!.Register("regex_find", "regex_find(str, regex_pattern, [ignoreCase])", false, new ExpressionFactoryHelper<RegexMatchExp>());

            // Regex replacement APIs
            AgentFrameworkService.Instance.DslEngine!.Register("regex_replace", "regex_replace(str, regex_pattern, replacement, [ignoreCase])", new ExpressionFactoryHelper<RegexReplaceExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("regex_replace_string", "regex_replace_string(str, regex_pattern, replacement, [ignoreCase])", new ExpressionFactoryHelper<RegexReplaceExp>());

            // Regex find all matches
            AgentFrameworkService.Instance.DslEngine!.Register("regex_find_all", "regex_find_all(str, regex_pattern, [ignoreCase]) return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<RegexFindAllExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("regex_matches", "regex_matches(str, regex_pattern, [ignoreCase]) return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<RegexFindAllExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("regex_find_first", "regex_find_first(str, regex_pattern, [ignoreCase]) return first match string or null", new ExpressionFactoryHelper<RegexFindFirstExp>());

            // File-based regex operations
            AgentFrameworkService.Instance.DslEngine!.Register("regex_replace_in_file", "regex_replace_in_file(path, regex_pattern, replacement, [ignoreCase])", new ExpressionFactoryHelper<RegexReplaceInFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("regex_search_file", "regex_search_file(path, regex_pattern, [ignoreCase]) return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<RegexSearchFileExp>());

            // Regex capture group APIs
            AgentFrameworkService.Instance.DslEngine!.Register("regex_group", "regex_group(str, regex_pattern, group_index, [ignoreCase]) return the specified capture group (1-based) of the first match, or null", new ExpressionFactoryHelper<RegexGroupExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("regex_group_all", "regex_group_all(str, regex_pattern, [ignoreCase]) return List<List<string>> of all matches with capture groups, use 'to_string' to convert to a string", new ExpressionFactoryHelper<RegexGroupAllExp>());

            // Regex escape / unescape
            AgentFrameworkService.Instance.DslEngine!.Register("regex_escape", "regex_escape(str) escape regex metacharacters, same as System.Text.RegularExpressions.Regex.Escape", new ExpressionFactoryHelper<RegexEscapeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("regex_unescape", "regex_unescape(str) unescape regex escape sequences, same as System.Text.RegularExpressions.Regex.Unescape", new ExpressionFactoryHelper<RegexUnescapeExp>());
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: regex_match(str, regex_pattern, [ignoreCase or 'i']), aliased as regex_is_match or regex_find");
                return BoxedValue.From(false);
            }

            try {
                string str = operands[0].AsString;
                string pattern = operands[1].AsString;
                bool ignoreCase = operands.Count > 2 ? operands[2].GetBool() || operands[2].ToString() == "i" : true;

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
                bool ignoreCase = operands.Count > 3 ? operands[3].GetBool() || operands[3].ToString() == "i" : true;

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
                bool ignoreCase = operands.Count > 2 ? operands[2].GetBool() || operands[2].ToString() == "i" : true;

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
    /// Find first match of regex pattern in string
    /// </summary>
    sealed class RegexFindFirstExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: regex_find_first(str, regex_pattern, [ignoreCase]) returns first match string or null");
                return BoxedValue.NullObject;
            }

            try {
                string str = operands[0].AsString;
                string pattern = operands[1].AsString;
                bool ignoreCase = operands.Count > 2 ? operands[2].GetBool() || operands[2].ToString() == "i" : true;

                string? first = StringHelper.FindFirstMatch(str, pattern, ignoreCase);
                if (first == null)
                    return BoxedValue.NullObject;
                return BoxedValue.FromString(first);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error in regex_find_first: {ex.Message}");
                return BoxedValue.NullObject;
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
                bool ignoreCase = operands.Count > 3 ? operands[3].GetBool() || operands[3].ToString() == "i" : true;

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
                // Preserve original BOM state when overwriting existing file.
                var writeEncoding = BomHelper.GetUtf8EncodingPreservingBom(path, defaultBom: false);
                System.IO.File.WriteAllText(path, newContent, writeEncoding);
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
                bool ignoreCase = operands.Count > 2 ? operands[2].GetBool() || operands[2].ToString() == "i" : true;

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

    /// <summary>
    /// Escape regex metacharacters in a string
    /// </summary>
    sealed class RegexEscapeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: regex_escape(str)");
                return BoxedValue.EmptyString;
            }

            try {
                string str = operands[0].AsString;
                string result = System.Text.RegularExpressions.Regex.Escape(str ?? string.Empty);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error in regex_escape: {ex.Message}");
                return BoxedValue.EmptyString;
            }
        }
    }

    /// <summary>
    /// Unescape regex escape sequences in a string
    /// </summary>
    sealed class RegexUnescapeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: regex_unescape(str)");
                return BoxedValue.EmptyString;
            }

            try {
                string str = operands[0].AsString;
                string result = System.Text.RegularExpressions.Regex.Unescape(str ?? string.Empty);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error in regex_unescape: {ex.Message}");
                return BoxedValue.EmptyString;
            }
        }
    }

    /// <summary>
    /// Return the specified capture group (1-based) of the first match, or null.
    /// group_index 0 = full match, 1+ = capture groups.
    /// </summary>
    sealed class RegexGroupExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: regex_group(str, regex_pattern, group_index, [ignoreCase]) group_index: 0=full match, 1+=capture groups");
                return BoxedValue.NullObject;
            }

            try {
                string str = operands[0].AsString;
                string pattern = operands[1].AsString;
                int groupIndex = (int)operands[2].GetLong();
                bool ignoreCase = operands.Count > 3 ? operands[3].GetBool() || operands[3].ToString() == "i" : true;

                string? result = StringHelper.FindGroup(str, pattern, groupIndex, ignoreCase);
                if (result == null)
                    return BoxedValue.NullObject;
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error in regex_group: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    /// <summary>
    /// Return all matches; each entry is [fullMatch, group1, group2, ...].
    /// </summary>
    sealed class RegexGroupAllExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: regex_group_all(str, regex_pattern, [ignoreCase]) returns List<List<string>>, use 'to_string' to convert to a string");
                return BoxedValue.FromObject(new List<List<string>>());
            }

            try {
                string str = operands[0].AsString;
                string pattern = operands[1].AsString;
                bool ignoreCase = operands.Count > 2 ? operands[2].GetBool() || operands[2].ToString() == "i" : true;

                var result = StringHelper.FindAllGroups(str, pattern, ignoreCase);
                return BoxedValue.FromObject(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error in regex_group_all: {ex.Message}");
                return BoxedValue.FromObject(new List<List<string>>());
            }
        }
    }
}
