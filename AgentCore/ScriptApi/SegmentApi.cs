using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AgentPlugin.Abstractions;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // tokenize(text) - segment mixed Chinese/English text into a list of tokens
    sealed class TokenizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: tokenize(text)");
                return BoxedValue.FromObject(new List<BoxedValue>());
            }

            {
                try {
                    string text = operands[0].AsString;
                    var tokens = Core.AgentCore.Instance.MixedSegmenter.Segment(text);
                    var list = new List<BoxedValue>(tokens.Count);
                    foreach (var t in tokens)
                        list.Add(BoxedValue.FromString(t));
                    return BoxedValue.FromObject(list);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"tokenize error: {ex.Message}");
                }
            }
            return BoxedValue.FromObject(new List<BoxedValue>());
        }
    }

    // set_help_semantic_search(type) - set help search mode: 0=BagOfWords, 1=TfIdf, 2=Embedding
    sealed class SetHelpSemanticSearchExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: set_help_semantic_search(type)");
                return BoxedValue.FromString("[error] missing argument");
            }

            {
                int type = operands[0].GetInt();
                Core.AgentCore.Instance.HelpSearchMode = (Core.HelpSearchType)type;
                return BoxedValue.FromString("ok");
            }
        }
    }

    // set_help_reranker(enable) - enable or disable reranker for help search: 1=enable, 0=disable
    sealed class SetHelpRerankerExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: set_help_reranker(enable)");
                return BoxedValue.FromString("[error] missing argument");
            }

            {
                Core.AgentCore.Instance.HelpUseReranker = operands[0].GetInt() != 0;
                return BoxedValue.FromString("ok");
            }
        }
    }

    // sethelp_debug(enable) - append help search diagnostics when enabled
    sealed class SetHelpDebugExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: sethelp_debug(enable)");
                return BoxedValue.FromString("[error] missing argument");
            }

            Core.AgentCore.Instance.HelpDebug = operands[0].GetInt() != 0;
            return BoxedValue.FromString("ok");
        }
    }

    // update_help_freq(file)
    sealed class UpdateHelpFreqExp : SimpleExpressionBase
    {
        private static readonly Regex TokenRegex = new Regex(@"[A-Za-z][A-Za-z0-9_]*|[0-9]+", RegexOptions.Compiled);
        private static readonly Regex IdentifierBoundaryRegex = new Regex(
            @"_+|(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|(?<=[A-Za-z])(?=\d)|(?<=\d)(?=[A-Za-z])",
            RegexOptions.Compiled);
        private static readonly HashSet<string> TechnicalTerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "api", "bm25", "cef", "dsl", "freq", "fts", "fts5", "idf", "mcp", "onnx", "tfidf", "trie"
            };

        private static List<string> SplitIdentifierBoundaries(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return new List<string>();
            if (TechnicalTerms.Contains(token))
                return new List<string> { token.ToLowerInvariant() };

            return IdentifierBoundaryRegex.Split(token)
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => part.ToLowerInvariant())
                .ToList();
        }

        private static List<string>? SplitConcatenatedWord(string token, HashSet<string> vocabulary)
        {
            string word = token.ToLowerInvariant();
            if (word.Length < 8 || !word.All(char.IsLetter))
                return null;

            int length = word.Length;
            var bestParts = new int[length + 1];
            var previous = new int[length + 1];
            Array.Fill(bestParts, int.MaxValue);
            Array.Fill(previous, -1);
            bestParts[0] = 0;

            for (int start = 0; start < length; ++start) {
                if (bestParts[start] == int.MaxValue)
                    continue;
                for (int end = start + 3; end <= length; ++end) {
                    if (start == 0 && end == length)
                        continue;
                    string part = word.Substring(start, end - start);
                    if (!vocabulary.Contains(part))
                        continue;
                    int partCount = bestParts[start] + 1;
                    if (partCount < bestParts[end]) {
                        bestParts[end] = partCount;
                        previous[end] = start;
                    }
                }
            }

            if (bestParts[length] < 2 || bestParts[length] == int.MaxValue)
                return null;

            var parts = new List<string>();
            for (int end = length; end > 0;) {
                int start = previous[end];
                if (start < 0)
                    return null;
                parts.Add(word.Substring(start, end - start));
                end = start;
            }
            parts.Reverse();
            return parts;
        }

        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: update_help_freq(file)");
                return BoxedValue.FromString("[error] update_help_freq requires an output file path");
            }

            try {
                string file_path = operands[0].AsString;
                if (string.IsNullOrWhiteSpace(file_path))
                    return BoxedValue.FromString("[error] output file path is empty");

                var documents = new List<List<string>>();
                var vocabulary = new HashSet<string>(TechnicalTerms, StringComparer.OrdinalIgnoreCase);
                string englishBaseWordsPath = Path.Combine(
                    Core.AgentCore.Instance.BasePath,
                    "onnx",
                    "englishbasewords.txt");
                if (File.Exists(englishBaseWordsPath)) {
                    foreach (string word in File.ReadLines(englishBaseWordsPath)) {
                        string trimmedWord = word.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmedWord))
                            vocabulary.Add(trimmedWord);
                    }
                }

                void AddDocument(string? text)
                {
                    if (string.IsNullOrWhiteSpace(text))
                        return;

                    var tokens = new List<string>();
                    foreach (Match match in TokenRegex.Matches(text)) {
                        var parts = SplitIdentifierBoundaries(match.Value);
                        if (parts.Count == 0)
                            continue;
                        tokens.AddRange(parts);
                        foreach (string part in parts)
                            vocabulary.Add(part);
                    }
                    if (tokens.Count > 0)
                        documents.Add(tokens);
                }

                var native_api = AgentFrameworkService.Instance.NativeApi;
                if (native_api == null)
                    return BoxedValue.FromString("[error] native API is unavailable");

                foreach (string document in native_api.GetHelpDocs())
                    AddDocument(document);
                foreach (var pair in Core.AgentCore.Instance.SkillMgr.Skills) {
                    AddDocument(pair.Key + "\n" + pair.Value.Document);
                }

                var termFrequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var documentFrequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var document in documents) {
                    var documentTerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string token in document) {
                        var parts = SplitConcatenatedWord(token, vocabulary);
                        var finalTerms = parts ?? new List<string> { token };
                        foreach (string term in finalTerms) {
                            termFrequencies.TryGetValue(term, out int tf);
                            termFrequencies[term] = tf + 1;
                            documentTerms.Add(term);
                        }
                    }
                    foreach (string term in documentTerms) {
                        documentFrequencies.TryGetValue(term, out int df);
                        documentFrequencies[term] = df + 1;
                    }
                }

                int documentCount = Math.Max(documents.Count, 1);
                var frequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var pair in termFrequencies) {
                    documentFrequencies.TryGetValue(pair.Key, out int df);
                    double idf = Math.Log(1.0 + (documentCount - df + 0.5) / (df + 0.5));
                    int weightedFrequency = Math.Max(1, (int)Math.Round(pair.Value * (1.0 + idf), MidpointRounding.AwayFromZero));
                    frequencies[pair.Key] = weightedFrequency;
                }

                string? directory = Path.GetDirectoryName(file_path);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                var lines = frequencies
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => string.Format("{0} {1}", pair.Key, pair.Value));
                File.WriteAllLines(file_path, lines, new UTF8Encoding(false));
                return BoxedValue.FromString($"ok: wrote {frequencies.Count} tokens to {file_path}");
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"UpdateHelpFreq error: {ex.Message}");
                return BoxedValue.FromString($"[error] {ex.Message}");
            }
        }
    }

    public static class SegmentApi
    {
        public static void RegisterApis()
        {
            AgentFrameworkService.Instance.DslEngine!.Register("tokenize", "tokenize(text) => list", new ExpressionFactoryHelper<TokenizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("update_help_freq", "update_help_freq(file) - rebuild help token frequency file from API and loaded Skill documents", new ExpressionFactoryHelper<UpdateHelpFreqExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("updatehelpfreq", "updatehelpfreq(file) - rebuild help token frequency file from API and loaded Skill documents", false, new ExpressionFactoryHelper<UpdateHelpFreqExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_help_semantic_search", "set_help_semantic_search(type) - 0=BagOfWords,1=TfIdf,2=Embedding", new ExpressionFactoryHelper<SetHelpSemanticSearchExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_help_reranker", "set_help_reranker(enable) - 1=enable,0=disable", new ExpressionFactoryHelper<SetHelpRerankerExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("sethelp_debug", "sethelp_debug(enable) - 1=enable,0=disable help search diagnostics", new ExpressionFactoryHelper<SetHelpDebugExp>());
        }
    }
}
