using System;
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
            BatchCommand.BatchScript.Register("regex_match", "regex_match(str, regex_pattern, [ignoreCase])", new ExpressionFactoryHelper<RegexMatchExp>());
            BatchCommand.BatchScript.Register("regex_is_match", "regex_is_match(str, regex_pattern, [ignoreCase])", new ExpressionFactoryHelper<RegexMatchExp>());

            // Regex replacement APIs
            BatchCommand.BatchScript.Register("regex_replace", "regex_replace(str, regex_pattern, replacement, [ignoreCase])", new ExpressionFactoryHelper<RegexReplaceExp>());
            BatchCommand.BatchScript.Register("regex_replace_string", "regex_replace_string(str, regex_pattern, replacement, [ignoreCase])", new ExpressionFactoryHelper<RegexReplaceExp>());

            // Regex find all matches
            BatchCommand.BatchScript.Register("regex_find_all", "regex_find_all(str, regex_pattern, [ignoreCase])", new ExpressionFactoryHelper<RegexFindAllExp>());
            BatchCommand.BatchScript.Register("regex_matches", "regex_matches(str, regex_pattern, [ignoreCase])", new ExpressionFactoryHelper<RegexFindAllExp>());

            // File-based regex operations
            BatchCommand.BatchScript.Register("regex_replace_file", "regex_replace_file(path, regex_pattern, replacement, [ignoreCase])", new ExpressionFactoryHelper<RegexReplaceFileExp>());
            BatchCommand.BatchScript.Register("regex_search_file", "regex_search_file(path, regex_pattern, [ignoreCase])", new ExpressionFactoryHelper<RegexSearchFileExp>());
        }
    }

    /// <summary>
    /// Test if string matches regex pattern
    /// </summary>
    sealed class RegexMatchExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("Error: regex_match requires str and regex_pattern parameters");
                return BoxedValue.From(false);
            }

            try
            {
                string str = operands[0].AsString;
                string pattern = operands[1].AsString;
                bool ignoreCase = operands.Count > 2 ? operands[2].GetBool() : true;

                bool result = StringHelper.MatchesPattern(str, pattern, ignoreCase);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Error in regex_match: {ex.Message}");
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
            if (operands.Count < 3)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("Error: regex_replace requires str, regex_pattern, and replacement parameters");
                return BoxedValue.FromString("");
            }

            string str = operands[0].AsString;
            try
            {
                string pattern = operands[1].AsString;
                string replacement = operands[2].AsString;
                bool ignoreCase = operands.Count > 3 ? operands[3].GetBool() : true;

                if (!StringHelper.MatchesPattern(str, pattern, ignoreCase)) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Error: pattern not found: {pattern}");
                    return BoxedValue.From(false);
                }
                string result = StringHelper.ReplacePattern(str, pattern, replacement, ignoreCase);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Error in regex_replace: {ex.Message}");
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
            if (operands.Count < 2)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("Error: regex_find_all requires str and regex_pattern parameters");
                return BoxedValue.FromObject(new List<string>());
            }

            try
            {
                string str = operands[0].AsString;
                string pattern = operands[1].AsString;
                bool ignoreCase = operands.Count > 2 ? operands[2].GetBool() : true;

                var matches = StringHelper.FindAllMatches(str, pattern, ignoreCase);
                return BoxedValue.FromObject(matches);
            }
            catch (Exception ex)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Error in regex_find_all: {ex.Message}");
                return BoxedValue.FromObject(new List<string>());
            }
        }
    }

    /// <summary>
    /// Replace file content using regex pattern
    /// </summary>
    sealed class RegexReplaceFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("Error: regex_replace_file requires path, regex_pattern, and replacement parameters");
                return BoxedValue.From(false);
            }

            try
            {
                string path = operands[0].AsString;
                string pattern = operands[1].AsString;
                string replacement = operands[2].AsString;
                bool ignoreCase = operands.Count > 3 ? operands[3].GetBool() : true;

                if (!System.IO.File.Exists(path))
                {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Error: File not found: {path}");
                    return BoxedValue.From(false);
                }

                string content = System.IO.File.ReadAllText(path);
                if (!StringHelper.MatchesPattern(content, pattern, ignoreCase)) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Error: {path} pattern not found: {pattern}");
                    return BoxedValue.From(false);
                }
                string newContent = StringHelper.ReplacePattern(content, pattern, replacement, ignoreCase);
                System.IO.File.WriteAllText(path, newContent);
                return BoxedValue.From(true);
            }
            catch (Exception ex)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Error in regex_replace_file: {ex.Message}");
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
            if (operands.Count < 2)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("Error: regex_search_file requires path and regex_pattern parameters");
                return BoxedValue.FromObject(new List<string>());
            }

            try
            {
                string path = operands[0].AsString;
                string pattern = operands[1].AsString;
                bool ignoreCase = operands.Count > 2 ? operands[2].GetBool() : true;

                if (!System.IO.File.Exists(path))
                {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Error: File not found: {path}");
                    return BoxedValue.FromObject(new List<string>());
                }

                string content = System.IO.File.ReadAllText(path);
                var matches = StringHelper.FindAllMatches(content, pattern, ignoreCase);
                return BoxedValue.FromObject(matches);
            }
            catch (Exception ex)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Error in regex_search_file: {ex.Message}");
                return BoxedValue.FromObject(new List<string>());
            }
        }
    }
}
