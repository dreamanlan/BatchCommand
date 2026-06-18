using System.Text.RegularExpressions;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Helper that removes "thinking" / "reasoning" segments from LLM
    /// response content so that callers only see the user-facing answer.
    ///
    /// Targets three common in-band tag styles emitted by reasoning models
    /// (qwen3, gpt-oss, deepseek-r1, glm, etc. running locally or via gateways):
    ///   &lt;think&gt; ... &lt;/think&gt;
    ///   &lt;thinking&gt; ... &lt;/thinking&gt;
    ///   &lt;reasoning&gt; ... &lt;/reasoning&gt;
    ///
    /// Out-of-band reasoning fields (reasoning_content, message.thinking, etc.)
    /// are handled in each provider directly by simply not concatenating them
    /// into the final reply.
    /// </summary>
    internal static class ThinkingFilter
    {
        // Singleline lets "." match newlines; IgnoreCase covers <Think>, <THINKING>, etc.
        private static readonly Regex s_thinkRe = new Regex(
            @"<\s*think\s*>[\s\S]*?<\s*/\s*think\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex s_thinkingRe = new Regex(
            @"<\s*thinking\s*>[\s\S]*?<\s*/\s*thinking\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex s_reasoningRe = new Regex(
            @"<\s*reasoning\s*>[\s\S]*?<\s*/\s*reasoning\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Defensively clean up an unmatched dangling open-tag tail
        // (e.g. content ends with "...<think>partial" because the upstream
        //  truncated the response). Drop everything from the open tag on.
        private static readonly Regex s_danglingOpenRe = new Regex(
            @"<\s*(think|thinking|reasoning)\s*>[\s\S]*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Strip all thinking / reasoning segments from the given content.
        /// Returns the cleaned answer text. Returns the original string
        /// unchanged when input is null or empty.
        /// </summary>
        public static string StripThink(string content)
        {
            if (string.IsNullOrEmpty(content)) return content;
            string s = s_thinkRe.Replace(content, "");
            s = s_thinkingRe.Replace(s, "");
            s = s_reasoningRe.Replace(s, "");
            s = s_danglingOpenRe.Replace(s, "");
            return s.Trim();
        }
    }
}
