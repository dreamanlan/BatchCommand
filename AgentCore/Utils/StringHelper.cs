using System;
using System.Text;
using System.Text.RegularExpressions;

namespace CefDotnetApp.AgentCore.Utils
{
    public static class StringHelper
    {
        public static bool IsNullOrEmpty(string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace(string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static string Truncate(string str, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
                return str;

            return str.Substring(0, maxLength - suffix.Length) + suffix;
        }

        public static string EscapeString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return str.Replace("\\", "\\\\")
                      .Replace("\"", "\\\"")
                      .Replace("\n", "\\n")
                      .Replace("\r", "\\r")
                      .Replace("\t", "\\t");
        }

        public static string UnescapeString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return str.Replace("\\\\", "\\")
                      .Replace("\\\"", "\"")
                      .Replace("\\n", "\n")
                      .Replace("\\r", "\r")
                      .Replace("\\t", "\t");
        }

        public static string[] SplitLines(string str)
        {
            if (string.IsNullOrEmpty(str))
                return Array.Empty<string>();

            return str.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        public static string JoinLines(string[] lines, string separator = "\n")
        {
            if (lines == null || lines.Length == 0)
                return string.Empty;

            return string.Join(separator, lines);
        }

        public static bool MatchesPattern(string str, string pattern, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(pattern))
                return false;

            try
            {
                RegexOptions options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
                return Regex.IsMatch(str, pattern, options);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string ReplacePattern(string str, string pattern, string replacement, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(pattern))
                return str;

            try
            {
                RegexOptions options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
                return Regex.Replace(str, pattern, replacement, options);
            }
            catch (Exception)
            {
                return str;
            }
        }

        public static string Indent(string str, int spaces)
        {
            if (string.IsNullOrEmpty(str) || spaces <= 0)
                return str;

            string indent = new string(' ', spaces);
            string[] lines = SplitLines(str);

            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                    lines[i] = indent + lines[i];
            }

            return JoinLines(lines);
        }

        public static string RemoveIndent(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            string[] lines = SplitLines(str);
            int minIndent = int.MaxValue;

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                int indent = 0;
                foreach (char c in line)
                {
                    if (c == ' ' || c == '\t')
                        indent++;
                    else
                        break;
                }

                minIndent = Math.Min(minIndent, indent);
            }

            if (minIndent == int.MaxValue || minIndent == 0)
                return str;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length >= minIndent)
                    lines[i] = lines[i].Substring(minIndent);
            }

            return JoinLines(lines);
        }
    }
}
