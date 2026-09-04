using System.Collections.Generic;
using System.Text;

namespace AbstractAgent.Utils
{
    /// <summary>
    /// Template substitution utility for block strings with configurable delimiters.
    /// Supports parameter and environment variable replacement within delimiter pairs.
    /// </summary>
    public static class TemplateCode
    {
        // default delimiters: {%...%} for params/envs, no delimiters for comments
        private const char c_BeginFirst = '{';
        private const char c_BeginSecond = '%';
        private const char c_EndFirst = '%';
        private const char c_EndSecond = '}';
        private const char c_CommentBeginFirst = '\0';
        private const char c_CommentBeginSecond = '\0';
        private const char c_CommentEndFirst = '\0';
        private const char c_CommentEndSecond = '\0';

        /// <summary>
        /// Substitute parameters and environment variables in a block string.
        /// When beginChars/endChars are empty, uses default delimiters {%..%}.
        /// beginChars/endChars format: 2 chars = primary delimiter, 4 chars = primary+comment delimiters.
        /// </summary>
        public static string CalcBlockString(string block, Dictionary<string, string> args,
            Dictionary<string, string> envs, StringBuilder outputBuilder,
            StringBuilder paramAndEnvBuilder, string beginChars, string endChars)
        {
            char beginFirst = c_BeginFirst;
            char beginSecond = c_BeginSecond;
            char endFirst = c_EndFirst;
            char endSecond = c_EndSecond;
            char commentBeginFirst = c_CommentBeginFirst;
            char commentBeginSecond = c_CommentBeginSecond;
            char commentEndFirst = c_CommentEndFirst;
            char commentEndSecond = c_CommentEndSecond;

            if (beginChars.Length >= 2 && endChars.Length >= 2) {
                beginFirst = beginChars[0];
                beginSecond = beginChars[1];
                endFirst = endChars[0];
                endSecond = endChars[1];

                commentBeginFirst = '\0';
                commentBeginSecond = '\0';
                commentEndFirst = '\0';
                commentEndSecond = '\0';
            }
            if (beginChars.Length >= 4 && endChars.Length >= 4) {
                commentBeginFirst = beginChars[2];
                commentBeginSecond = beginChars[3];
                commentEndFirst = endChars[2];
                commentEndSecond = endChars[3];
            }

            return CalcBlockStringInternal(block, args, envs, outputBuilder, paramAndEnvBuilder
                , beginFirst, beginSecond, endFirst, endSecond
                , commentBeginFirst, commentBeginSecond, commentEndFirst, commentEndSecond);
        }

        private static string CalcBlockStringInternal(string block, Dictionary<string, string> args,
            Dictionary<string, string> envs, StringBuilder outputBuilder, StringBuilder paramAndEnvBuilder
            , char beginFirst, char beginSecond, char endFirst, char endSecond
            , char commentBeginFirst, char commentBeginSecond, char commentEndFirst, char commentEndSecond)
        {
            outputBuilder.Length = 0;
            for (int i = 0; i < block.Length; ++i) {
                char c = block[i];
                char nc = '\0';
                if (i + 1 < block.Length) {
                    nc = block[i + 1];
                }
                if (c == beginFirst && nc == beginSecond) {
                    ++i;
                    ++i;
                    ExtractBlockString(block, ref i, endFirst, endSecond, paramAndEnvBuilder);
                    ReplaceParamAndEnvs(args, envs, outputBuilder, paramAndEnvBuilder);
                }
                else if (c == commentBeginFirst && nc == commentBeginSecond) {
                    ++i;
                    ++i;
                    ExtractBlockString(block, ref i, commentEndFirst, commentEndSecond, paramAndEnvBuilder);
                }
                else {
                    outputBuilder.Append(c);
                }
            }
            return outputBuilder.ToString();
        }

        private static void ExtractBlockString(string block, ref int i, char endFirst, char endSecond, StringBuilder paramAndEnvBuilder)
        {
            paramAndEnvBuilder.Length = 0;
            for (int j = i; j < block.Length; ++j) {
                char c = block[j];
                char nc = '\0';
                if (j + 1 < block.Length) {
                    nc = block[j + 1];
                }
                if (c == endFirst && nc == endSecond) {
                    i = j + 1;
                    break;
                }
                else {
                    paramAndEnvBuilder.Append(c);
                }
            }
        }

        private static void ReplaceParamAndEnvs(Dictionary<string, string> args, Dictionary<string, string> envs, StringBuilder outputBuilder, StringBuilder paramAndEnvBuilder)
        {
            string key = paramAndEnvBuilder.ToString().Trim();
            if (args.TryGetValue(key, out var val)) {
                outputBuilder.Append(val);
            }
            else if (envs.TryGetValue(key, out var env)) {
                outputBuilder.Append(env);
            }
        }
    }
}
