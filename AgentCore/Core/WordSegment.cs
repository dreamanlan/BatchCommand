using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// High-performance word segmenter backed by HybridTrie.
    /// - Uses HybridTrie.BuildFromFreqLines which parallelizes aggregation and build.
    /// - Provides Save/Load wrapper to persist optimized trie to file.
    /// - Segmentation logic remains the same and uses optimized TrieNode for traversal.
    /// </summary>
    public class WordSegmenterHybridTrie
    {
        private readonly HybridTrie.TrieNode _root;
        private readonly int _maxWordLen;
        private readonly double _totalCount;
        internal readonly bool _caseSensitive;

        // Configurable parameters (kept for compatibility)
        internal readonly int _arraySize; // e.g. 256
        internal readonly int _minChildrenForArray; // min child count to consider allocating array
        internal readonly double _asciiFractionThreshold; // fraction of children that are ascii to trigger array

        private static readonly Regex _tokenSplitRegex = new Regex(@"([\p{L}\p{N}]+)|([^\p{L}\p{N}]+)", RegexOptions.Compiled);

        /// <summary>
        /// Construct from frequency file path (will build trie with parallel aggregation).
        /// </summary>
        public WordSegmenterHybridTrie(
            string freqFilePath,
            bool caseSensitive = false,
            int arraySize = 256,
            int minChildrenForArray = 8,
            double asciiFractionThreshold = 0.6)
            : this(File.ReadLines(freqFilePath), caseSensitive, arraySize, minChildrenForArray, asciiFractionThreshold)
        {
        }

        /// <summary>
        /// Construct from enumerable of "word count" lines (parallelized build).
        /// </summary>
        public WordSegmenterHybridTrie(
            IEnumerable<string> freqLines,
            bool caseSensitive = false,
            int arraySize = 256,
            int minChildrenForArray = 8,
            double asciiFractionThreshold = 0.6)
        {
            _caseSensitive = caseSensitive;
            _arraySize = arraySize >= 2 ? arraySize : 256;
            _minChildrenForArray = Math.Max(1, minChildrenForArray);
            _asciiFractionThreshold = (asciiFractionThreshold >= 0 && asciiFractionThreshold <= 1) ? asciiFractionThreshold : 0.6;

            // Use HybridTrie.BuildFromFreqLines (parallel aggregation + partitioned build)
            var hybrid = HybridTrie.BuildFromFreqLines(freqLines, _caseSensitive, _arraySize, _minChildrenForArray, _asciiFractionThreshold);

            _root = hybrid.Root;
            _totalCount = hybrid.TotalCount;
            _maxWordLen = hybrid.MaxWordLen;
        }

        /// <summary>
        /// Load a serialized HybridTrie from file and create a WordSegmenterHybridTrie instance.
        /// </summary>
        public static WordSegmenterHybridTrie LoadFromTrieFile(string path, bool caseSensitive = false, int arraySize = 256, int minChildrenForArray = 8, double asciiFractionThreshold = 0.6)
        {
            var hybrid = HybridTrie.LoadFromFile(path, arraySize, minChildrenForArray, asciiFractionThreshold);
            return new WordSegmenterHybridTrie(hybrid, caseSensitive, arraySize, minChildrenForArray, asciiFractionThreshold);
        }

        // Internal constructor from already-built HybridTrie
        internal WordSegmenterHybridTrie(HybridTrie hybrid, bool caseSensitive, int arraySize, int minChildrenForArray, double asciiFractionThreshold)
        {
            _root = hybrid.Root;
            _totalCount = hybrid.TotalCount;
            _maxWordLen = hybrid.MaxWordLen;
            _caseSensitive = caseSensitive;
            _arraySize = arraySize;
            _minChildrenForArray = minChildrenForArray;
            _asciiFractionThreshold = asciiFractionThreshold;
        }

        /// <summary>
        /// Persist current trie to file (binary).
        /// Note: this serializes an equivalent temp-style tree then re-optimizes on load.
        /// </summary>
        public void SaveTrieToFile(string path)
        {
            var hybrid = new HybridTrie {
                Root = _root,
                TotalCount = _totalCount,
                MaxWordLen = _maxWordLen,
                ArraySize = _arraySize,
                MinChildrenForArray = _minChildrenForArray,
                AsciiFractionThreshold = _asciiFractionThreshold
            };
            hybrid.SaveToFile(path);
        }

        private char NormalizeChar(char c) => _caseSensitive ? c : char.ToLowerInvariant(c);

        private double UnknownCost(int len) => Math.Log(_totalCount) + 5.0 * len;

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
                    if (!node.TryGetChild(c, out HybridTrie.TrieNode? next, _arraySize)) break;
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

        public List<string> SegmentTextPreserveSeparators(string text)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(text)) return result;

            var matches = _tokenSplitRegex.Matches(text);
            foreach (System.Text.RegularExpressions.Match m in matches) {
                if (m.Groups[1].Success) {
                    var token = m.Groups[1].Value;
                    var segs = SegmentToken(token);
                    result.AddRange(segs);
                }
                else if (m.Groups[2].Success) {
                    result.Add(m.Groups[2].Value);
                }
            }
            return result;
        }

        public static string JoinSegments(IEnumerable<string> segments)
        {
            return string.Join(" ", segments.Where(s => !string.IsNullOrEmpty(s)));
        }
    }
}