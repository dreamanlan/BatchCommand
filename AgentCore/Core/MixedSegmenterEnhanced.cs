using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JiebaNet.Segmenter;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;

namespace CefDotnetApp.AgentCore.Core
{
    // MixedSegmenterEnhanced.cs
    // Mixed segmenter that supports optional injection of an existing WordSegmenterHybridTrie.
    // If an external WordSegmenterHybridTrie is provided, Latin/digit token segmentation will use it; otherwise
    // the class will build and use its internal trie-based segmenter as a fallback.

    public class MixedSegmenterEnhanced
    {
        // -------------------- External trie (injected) --------------------
        // If provided, this instance will be used for Latin/digit segmentation.
        private readonly WordSegmenterHybridTrie? _externalEnglishTrie;

        // -------------------- Internal Trie implementation (fallback) --------------------
        private class TempNode
        {
            public Dictionary<char, TempNode>? Children;
            public double Cost = double.NaN;
            public TempNode() { Children = null; Cost = double.NaN; }
            public TempNode GetOrAddChild(char c)
            {
                if (Children == null) Children = new Dictionary<char, TempNode>();
                if (!Children.TryGetValue(c, out TempNode? child)) {
                    child = new TempNode();
                    Children[c] = child;
                }
                return child;
            }
        }

        private class TrieNode
        {
            public TrieNode[]? ChildrenArray;
            public Dictionary<char, TrieNode>? ChildrenDict;
            public double Cost = double.NaN;
            public bool IsWord => !double.IsNaN(Cost);
            public bool TryGetChild(char c, out TrieNode? child, int arraySize)
            {
                if (ChildrenArray != null) {
                    int ci = c < arraySize ? c : -1;
                    if (ci >= 0) {
                        child = ChildrenArray[ci];
                        if (child != null) return true;
                    }
                }
                if (ChildrenDict != null && ChildrenDict.TryGetValue(c, out child)) return true;
                child = null;
                return false;
            }
        }

        private class MinHeapTopN
        {
            private List<(double count, string word)> _data;
            public int Capacity { get; }
            public int Count => _data.Count;
            public MinHeapTopN(int capacity)
            {
                Capacity = Math.Max(1, capacity);
                _data = new List<(double, string)>();
            }
            private static bool Less((double, string) a, (double, string) b) => a.Item1 < b.Item1 || (a.Item1 == b.Item1 && string.CompareOrdinal(a.Item2, b.Item2) < 0);
            private void Swap(int i, int j) { var t = _data[i]; _data[i] = _data[j]; _data[j] = t; }
            public (double count, string word) Peek() => _data[0];
            public void Push((double count, string word) item)
            {
                _data.Add(item);
                int i = _data.Count - 1;
                while (i > 0) {
                    int p = (i - 1) >> 1;
                    if (!Less(_data[i], _data[p])) break;
                    Swap(i, p);
                    i = p;
                }
                if (_data.Count > Capacity) Pop();
            }
            public (double count, string word) Pop()
            {
                int n = _data.Count;
                var root = _data[0];
                _data[0] = _data[n - 1];
                _data.RemoveAt(n - 1);
                n--;
                int i = 0;
                while (true) {
                    int l = 2 * i + 1, r = l + 1, smallest = i;
                    if (l < n && Less(_data[l], _data[smallest])) smallest = l;
                    if (r < n && Less(_data[r], _data[smallest])) smallest = r;
                    if (smallest == i) break;
                    Swap(i, smallest);
                    i = smallest;
                }
                return root;
            }
            public IEnumerable<(double count, string word)> Dump()
            {
                foreach (var item in _data) yield return item;
            }
        }

        // -------------------- Fields & configuration --------------------
        private readonly TrieNode _root;
        private readonly int _maxWordLen;
        private readonly double _totalCount;
        private readonly bool _caseSensitive;
        private readonly int _arraySize;
        private readonly int _minChildrenForArray;
        private readonly double _asciiFractionThreshold;

        private readonly JiebaSegmenter _jieba;
        private readonly MLContext _mlContext;
        private readonly ITransformer _mlTransformer;
        private readonly bool _preserveSeparators;

        private static readonly Regex TokenSplitRegex = new Regex(@"([\p{L}\p{N}]+)|([^\p{L}\p{N}]+)", RegexOptions.Compiled);
        private static readonly Regex HanLatinSplit = new Regex(@"(\p{IsCJKUnifiedIdeographs}+)|([A-Za-z0-9_']+)|([^\p{IsCJKUnifiedIdeographs}A-Za-z0-9_']+)", RegexOptions.Compiled);

        public class MlFeaturizeOptions
        {
            public int WordNgramLength = 2;
            public int CharNgramLength = 0;
            public bool RemoveStopWords = false;
            public TextNormalizingEstimator.CaseMode CaseMode = TextNormalizingEstimator.CaseMode.Lower;
            public bool KeepPunctuations = false;
            public int MaximumNGramCount = 0;
        }

        // -------------------- Constructors --------------------

        public MixedSegmenterEnhanced(
            IEnumerable<string> freqLines,
            bool caseSensitive = false,
            int arraySize = 256,
            int minChildrenForArray = 8,
            double asciiFractionThreshold = 0.6,
            int topNToKeep = 0,
            bool preserveSeparators = false,
            MlFeaturizeOptions? mlOptions = null)
        {
            _externalEnglishTrie = null;
            _caseSensitive = caseSensitive;
            _arraySize = arraySize >= 2 ? arraySize : 256;
            _minChildrenForArray = Math.Max(1, minChildrenForArray);
            _asciiFractionThreshold = (asciiFractionThreshold >= 0 && asciiFractionThreshold <= 1) ? asciiFractionThreshold : 0.6;
            _preserveSeparators = preserveSeparators;

            Dictionary<string, double>? tempCounts = null;
            double total = 0;
            int maxLen = 0;

            if (topNToKeep > 0) {
                var heap = new MinHeapTopN(topNToKeep);
                foreach (var raw in freqLines ?? Enumerable.Empty<string>()) {
                    if (string.IsNullOrWhiteSpace(raw)) continue;
                    var parts = raw.Trim().Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) continue;
                    var word = parts[0];
                    if (!double.TryParse(parts[1], out double cnt)) continue;
                    if (cnt <= 0) continue;
                    string key = _caseSensitive ? word : word.ToLowerInvariant();
                    heap.Push((cnt, key));
                }
                tempCounts = new Dictionary<string, double>(_caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
                foreach (var it in heap.Dump()) {
                    if (tempCounts.TryGetValue(it.word, out double exist)) tempCounts[it.word] = exist + it.count;
                    else tempCounts[it.word] = it.count;
                    total += it.count;
                    if (it.word.Length > maxLen) maxLen = it.word.Length;
                }
            }
            else {
                tempCounts = new Dictionary<string, double>(_caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
                foreach (var raw in freqLines ?? Enumerable.Empty<string>()) {
                    if (string.IsNullOrWhiteSpace(raw)) continue;
                    var parts = raw.Trim().Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) continue;
                    var word = parts[0];
                    if (!double.TryParse(parts[1], out double cnt)) continue;
                    if (cnt <= 0) continue;
                    string key = _caseSensitive ? word : word.ToLowerInvariant();
                    if (tempCounts.TryGetValue(key, out double exist)) tempCounts[key] = exist + cnt;
                    else tempCounts[key] = cnt;
                    total += cnt;
                    if (key.Length > maxLen) maxLen = key.Length;
                }
            }

            _totalCount = Math.Max(total, 1.0);
            _maxWordLen = Math.Max(maxLen, 1);

            var tempRoot = new TempNode();
            double lnTotal = Math.Log(_totalCount);
            foreach (var kv in tempCounts) {
                string word = kv.Key;
                double cnt = kv.Value;
                double cost = -Math.Log(cnt) + lnTotal;
                var node = tempRoot;
                foreach (char ch in word) {
                    char nc = NormalizeChar(ch);
                    node = node.GetOrAddChild(nc);
                }
                node.Cost = cost;
            }

            _root = ConvertTempNode(tempRoot);
            _jieba = new JiebaSegmenter();
            _mlContext = new MLContext();
            var empty = new[] { new TextData { Text = "a" } };
            var dv = _mlContext.Data.LoadFromEnumerable(empty);
            var featurizeOpts = new TextFeaturizingEstimator.Options {
                CaseMode = mlOptions?.CaseMode ?? TextNormalizingEstimator.CaseMode.Lower,
                KeepPunctuations = mlOptions?.KeepPunctuations ?? false,
                StopWordsRemoverOptions = (mlOptions?.RemoveStopWords ?? false) ? new StopWordsRemovingEstimator.Options() : null,
                WordFeatureExtractor = new WordBagEstimator.Options { NgramLength = Math.Max(1, mlOptions?.WordNgramLength ?? 2), UseAllLengths = true, MaximumNgramsCount = new[] { mlOptions?.MaximumNGramCount > 0 ? mlOptions.MaximumNGramCount : 100000 } },
                CharFeatureExtractor = (mlOptions?.CharNgramLength > 0) ? new WordBagEstimator.Options { NgramLength = mlOptions.CharNgramLength, UseAllLengths = true } : null
            };
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", featurizeOpts, nameof(TextData.Text));
            _mlTransformer = pipeline.Fit(dv);
        }

        // -------------------- Helpers --------------------

        /// <summary>
        /// Alternate constructor that accepts an existing WordSegmenterHybridTrie instance.
        /// When provided, the injected trie will be used for Latin/digit segmentation.
        /// </summary>
        public MixedSegmenterEnhanced(
            WordSegmenterHybridTrie externalEnglishTrie,
            bool preserveSeparators = false,
            MlFeaturizeOptions? mlOptions = null)
        {
            if (externalEnglishTrie == null) throw new ArgumentNullException(nameof(externalEnglishTrie));
            _externalEnglishTrie = externalEnglishTrie;
            _caseSensitive = externalEnglishTrie._caseSensitive;
            _arraySize = externalEnglishTrie._arraySize;
            _minChildrenForArray = externalEnglishTrie._minChildrenForArray;
            _asciiFractionThreshold = externalEnglishTrie._asciiFractionThreshold;
            _preserveSeparators = preserveSeparators;

            _root = new TrieNode();
            _totalCount = 1.0;
            _maxWordLen = 1;

            _jieba = new JiebaSegmenter();
            _mlContext = new MLContext();
            var empty = new[] { new TextData { Text = "a" } };
            var dv = _mlContext.Data.LoadFromEnumerable(empty);
            var featurizeOpts = new TextFeaturizingEstimator.Options {
                CaseMode = mlOptions?.CaseMode ?? TextNormalizingEstimator.CaseMode.Lower,
                KeepPunctuations = mlOptions?.KeepPunctuations ?? false,
                StopWordsRemoverOptions = (mlOptions?.RemoveStopWords ?? false) ? new StopWordsRemovingEstimator.Options() : null,
                WordFeatureExtractor = new WordBagEstimator.Options { NgramLength = Math.Max(1, mlOptions?.WordNgramLength ?? 2), UseAllLengths = true, MaximumNgramsCount = new[] { mlOptions?.MaximumNGramCount > 0 ? mlOptions.MaximumNGramCount : 100000 } },
                CharFeatureExtractor = (mlOptions?.CharNgramLength > 0) ? new WordBagEstimator.Options { NgramLength = mlOptions.CharNgramLength, UseAllLengths = true } : null
            };
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", featurizeOpts, nameof(TextData.Text));
            _mlTransformer = pipeline.Fit(dv);
        }

        private char NormalizeChar(char c) => _caseSensitive ? c : char.ToLowerInvariant(c);

        private TrieNode ConvertTempNode(TempNode temp)
        {
            var node = new TrieNode();
            node.Cost = temp.Cost;
            if (temp.Children == null || temp.Children.Count == 0) return node;

            int childCount = temp.Children.Count;
            int asciiChildren = 0;
            foreach (var ch in temp.Children.Keys) if (ch < _arraySize) asciiChildren++;

            bool useArray = false;
            if (childCount >= _minChildrenForArray) useArray = true;
            else if (asciiChildren > 0 && ((double)asciiChildren / childCount) >= _asciiFractionThreshold) useArray = true;

            if (useArray) {
                node.ChildrenArray = new TrieNode[_arraySize];
                Dictionary<char, TrieNode>? dict = null;
                foreach (var kv in temp.Children) {
                    char c = kv.Key;
                    TrieNode childNode = ConvertTempNode(kv.Value);
                    if (c < _arraySize) node.ChildrenArray[c] = childNode;
                    else {
                        if (dict == null) dict = new Dictionary<char, TrieNode>();
                        dict[c] = childNode;
                    }
                }
                node.ChildrenDict = dict;
            }
            else {
                var dict = new Dictionary<char, TrieNode>();
                foreach (var kv in temp.Children) dict[kv.Key] = ConvertTempNode(kv.Value);
                node.ChildrenDict = dict;
            }
            return node;
        }

        private double UnknownCost(int len) => Math.Log(_totalCount) + 5.0 * len;

        // -------------------- Segmentation --------------------
        public List<string> SegmentToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return new List<string>();
            string working = _caseSensitive ? token : token.ToLowerInvariant();
            ReadOnlySpan<char> span = working.AsSpan();
            int n = span.Length;

            var cost = new double[n + 1];
            var prev = new int[n + 1];
            for (int i = 1; i <= n; i++) { cost[i] = double.PositiveInfinity; prev[i] = -1; }
            cost[0] = 0;

            for (int start = 0; start < n; start++) {
                if (double.IsPositiveInfinity(cost[start])) continue;
                var node = _root;
                int limit = Math.Min(n, start + _maxWordLen);
                for (int end = start; end < limit; end++) {
                    char c = span[end];
                    if (!node.TryGetChild(c, out TrieNode? next, _arraySize)) break;
                    node = next!;
                    if (node.IsWord) {
                        double cand = cost[start] + node.Cost;
                        if (cand < cost[end + 1]) {
                            cost[end + 1] = cand;
                            prev[end + 1] = start;
                        }
                    }
                }
                {
                    int end = start;
                    double cand = cost[start] + UnknownCost(1);
                    if (cand < cost[end + 1]) {
                        cost[end + 1] = cand;
                        prev[end + 1] = start;
                    }
                }
            }

            var segments = new List<string>();
            int idx = n;
            if (double.IsPositiveInfinity(cost[n])) {
                for (int i = 0; i < n; i++) segments.Add(span.Slice(i, 1).ToString());
                return segments;
            }
            while (idx > 0) {
                int p = prev[idx];
                if (p < 0) p = idx - 1;
                int len = idx - p;
                segments.Add(working.Substring(p, len));
                idx = p;
            }
            segments.Reverse();
            return segments;
        }

        public List<string> Segment(string text)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(text)) return result;

            var matches = TokenSplitRegex.Matches(text);
            foreach (Match m in matches) {
                if (m.Groups[1].Success) {
                    var run = m.Groups[1].Value;
                    var parts = HanLatinSplit.Matches(run);
                    foreach (Match pm in parts) {
                        if (pm.Groups[1].Success) {
                            var han = pm.Groups[1].Value;
                            var segs = _jieba.Cut(han, cutAll: false);
                            foreach (var s in segs) if (!string.IsNullOrWhiteSpace(s)) result.Add(s);
                        }
                        else if (pm.Groups[2].Success) {
                            var latin = pm.Groups[2].Value;
                            List<string>? segs = null;
                            if (_externalEnglishTrie != null) {
                                try {
                                    var extResult = _externalEnglishTrie.SegmentToken(latin);
                                    if (extResult != null) segs = extResult;
                                }
                                catch {
                                    segs = null;
                                }
                            }
                            if (segs == null) {
                                segs = SegmentToken(latin);
                            }
                            foreach (var s in segs) if (!string.IsNullOrWhiteSpace(s)) result.Add(s);
                        }
                        else {
                            var oth = pm.Value;
                            if (_preserveSeparators) result.Add(oth);
                        }
                    }
                }
                else if (m.Groups[2].Success) {
                    if (_preserveSeparators) result.Add(m.Groups[2].Value);
                }
            }
            return result;
        }

        public static string TokensToText(IEnumerable<string> tokens) => string.Join(" ", tokens.Where(s => !string.IsNullOrWhiteSpace(s)));

        public float[] FeaturizeTokensToVector(IEnumerable<string> tokens)
        {
            if (_mlContext == null || _mlTransformer == null) throw new InvalidOperationException("ML transformer not initialized.");
            string text = TokensToText(tokens);
            var sample = new[] { new TextData { Text = text } };
            var dv = _mlContext.Data.LoadFromEnumerable(sample);
            var transformed = _mlTransformer.Transform(dv);
            var featuresColumn = transformed.Schema["Features"];
            using var cursor = transformed.GetRowCursor(new[] { featuresColumn });
            var getter = cursor.GetGetter<VBuffer<float>>(featuresColumn);
            VBuffer<float> v = default;
            if (cursor.MoveNext()) {
                getter(ref v);
                var dense = new float[v.Length];
                v.CopyTo(dense);
                return dense;
            }
            return Array.Empty<float>();
        }

        public float[][] FeaturizeBatch(IEnumerable<IEnumerable<string>> tokenCollection)
        {
            if (_mlContext == null || _mlTransformer == null) throw new InvalidOperationException("ML transformer not initialized.");
            var samples = tokenCollection.Select(t => new TextData { Text = TokensToText(t) }).ToList();
            var dv = _mlContext.Data.LoadFromEnumerable(samples);
            var transformed = _mlTransformer.Transform(dv);

            var featuresColumn = transformed.Schema["Features"];
            var result = new List<float[]>();
            using var cursor = transformed.GetRowCursor(new[] { featuresColumn });
            var getter = cursor.GetGetter<VBuffer<float>>(featuresColumn);
            VBuffer<float> v = default;
            while (cursor.MoveNext()) {
                getter(ref v);
                var dense = new float[v.Length];
                v.CopyTo(dense);
                result.Add(dense);
            }
            return result.ToArray();
        }

        private class TextData { public string Text { get; set; } = string.Empty; }
    }
}