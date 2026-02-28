using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CefDotnetApp.AgentCore.Core
{
    // Input schema for ML.NET pipeline
    internal class TfIdfDoc
    {
        public string Key { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    // Output schema: sparse TF-IDF feature vector
    internal class TfIdfDocSparse
    {
        public string Key { get; set; } = string.Empty;
        public VBuffer<float> Features { get; set; }
    }

    /// <summary>
    /// TF-IDF search service backed by ML.NET FeaturizeText + cosine similarity inverted index.
    /// Provides the same Search interface as BagOfWordsService and EmbeddingService.
    /// </summary>
    public class TfIdfService : IDisposable
    {
        private readonly object _lock = new object();
        private readonly MLContext _mlContext = new MLContext(seed: 1);

        // built-in index state
        private ITransformer? _transformer;
        private string[]? _indexKeys;
        private string[]? _indexTexts;
        // inverted index: featureIndex -> list of (docIndex, normalizedValue)
        private Dictionary<int, List<(int docIndex, float value)>>? _postings;

        // ad-hoc cache per thread
        private readonly ThreadLocal<(string cacheKey, ITransformer? transformer, string[] keys, string[] texts, Dictionary<int, List<(int, float)>> postings)> _adHocCache =
            new ThreadLocal<(string, ITransformer?, string[], string[], Dictionary<int, List<(int, float)>>)>(
                () => (string.Empty, null, Array.Empty<string>(), Array.Empty<string>(), new Dictionary<int, List<(int, float)>>()));

        private bool _disposed = false;

        /// <summary>
        /// Whether the built-in index has been built and is ready.
        /// </summary>
        public bool IsReady => true;

        /// <summary>
        /// Build (or rebuild) the TF-IDF index from a collection of (key, text) pairs.
        /// </summary>
        public void BuildIndex(IEnumerable<(string key, string text)> items)
        {
            var list = items.ToList();
            if (list.Count == 0)
                return;

            var (transformer, keys, texts, postings) = FitAndBuildPostings(list);

            lock (_lock) {
                _transformer = transformer;
                _indexKeys = keys;
                _indexTexts = texts;
                _postings = postings;
            }
        }

        /// <summary>
        /// TF-IDF cosine similarity search over the built-in index.
        /// Returns null if the index is not ready.
        /// </summary>
        public IList<(string key, string text, float score)>? Search(IList<string> queries, int topN)
        {
            ITransformer? transformer;
            string[]? keys;
            string[]? texts;
            Dictionary<int, List<(int, float)>>? postings;

            lock (_lock) {
                if (_transformer == null || _postings == null)
                    return null;
                transformer = _transformer;
                keys = _indexKeys!;
                texts = _indexTexts!;
                postings = _postings;
            }

            return RunSearch(transformer, keys, texts, postings, queries, topN);
        }

        /// <summary>
        /// TF-IDF cosine similarity search over an ad-hoc set of (key, text) candidates.
        /// Uses a per-thread cache to avoid rebuilding the index when candidates are unchanged.
        /// Returns null if candidates are empty.
        /// </summary>
        public IList<(string key, string text, float score)>? Search(
            IList<string> queries,
            IEnumerable<(string key, string text)> candidates,
            int topN)
        {
            var candidateList = candidates as IList<(string key, string text)>
                ?? candidates.ToList();
            if (candidateList.Count == 0)
                return null;

            string newCacheKey = BuildCacheKey(candidateList);
            var cache = _adHocCache.Value!;

            ITransformer transformer;
            string[] keys;
            string[] texts;
            Dictionary<int, List<(int, float)>> postings;

            if (cache.cacheKey == newCacheKey && cache.transformer != null) {
                transformer = cache.transformer;
                keys = cache.keys;
                texts = cache.texts;
                postings = cache.postings;
            } else {
                var result = FitAndBuildPostings(candidateList);
                transformer = result.transformer;
                keys = result.keys;
                texts = result.texts;
                postings = result.postings;
                _adHocCache.Value = (newCacheKey, transformer, keys, texts, postings);
            }

            return RunSearch(transformer, keys, texts, postings, queries, topN);
        }

        // ---- private helpers ----

        private (ITransformer transformer, string[] keys, string[] texts, Dictionary<int, List<(int, float)>> postings)
            FitAndBuildPostings(IList<(string key, string text)> items)
        {
            var docs = items.Select(i => new TfIdfDoc { Key = i.key, Text = i.text }).ToList();
            var dataView = _mlContext.Data.LoadFromEnumerable(docs);

            var pipeline = _mlContext.Transforms.Text.FeaturizeText(
                outputColumnName: "Features",
                inputColumnName: nameof(TfIdfDoc.Text));

            var transformer = pipeline.Fit(dataView);
            var transformed = transformer.Transform(dataView);

            var sparse = _mlContext.Data.CreateEnumerable<TfIdfDocSparse>(transformed, reuseRowObject: false).ToArray();

            int docCount = sparse.Length;
            var keys = items.Select(i => i.key).ToArray();
            var texts = items.Select(i => i.text).ToArray();

            // compute L2 norms
            var norms = new double[docCount];
            for (int i = 0; i < docCount; i++)
                norms[i] = ComputeL2Norm(sparse[i].Features);

            // build inverted index with normalized values
            var postings = new Dictionary<int, List<(int, float)>>();
            for (int i = 0; i < docCount; i++) {
                double norm = norms[i];
                if (norm <= 1e-12)
                    continue;
                AddToPostings(postings, sparse[i].Features, i, norm);
            }

            return (transformer, keys, texts, postings);
        }

        private IList<(string key, string text, float score)> RunSearch(
            ITransformer transformer,
            string[] keys,
            string[] texts,
            Dictionary<int, List<(int, float)>> postings,
            IList<string> queries,
            int topN)
        {
            // union: docIndex -> best score across all queries
            var best = new Dictionary<int, float>();

            foreach (var query in queries) {
                if (string.IsNullOrWhiteSpace(query))
                    continue;

                var qdocs = new[] { new TfIdfDoc { Key = "q", Text = query } };
                var qdv = _mlContext.Data.LoadFromEnumerable(qdocs);
                var qtrans = transformer.Transform(qdv);
                var qEnum = _mlContext.Data.CreateEnumerable<TfIdfDocSparse>(qtrans, reuseRowObject: false).ToArray();
                if (qEnum.Length == 0)
                    continue;

                var qvec = qEnum[0].Features;
                double qnorm = ComputeL2Norm(qvec);
                if (qnorm <= 1e-12)
                    continue;

                AccumulateScores(postings, qvec, qnorm, best);
            }

            return best
                .OrderByDescending(kv => kv.Value)
                .Take(topN)
                .Select(kv => (
                    key: kv.Key < keys.Length ? keys[kv.Key] : string.Empty,
                    text: kv.Key < texts.Length ? texts[kv.Key] : string.Empty,
                    score: kv.Value))
                .Where(r => !string.IsNullOrEmpty(r.key))
                .ToList();
        }

        private static double ComputeL2Norm(VBuffer<float> v)
        {
            double sumsq = 0;
            var vals = v.GetValues();
            for (int k = 0; k < vals.Length; k++)
                sumsq += (double)vals[k] * vals[k];
            return Math.Sqrt(sumsq);
        }

        private static void AddToPostings(
            Dictionary<int, List<(int, float)>> postings,
            VBuffer<float> v,
            int docIndex,
            double norm)
        {
            if (v.IsDense) {
                var vals = v.GetValues();
                for (int fi = 0; fi < vals.Length; fi++) {
                    float val = vals[fi];
                    if (Math.Abs(val) <= 1e-12f)
                        continue;
                    float normalized = (float)(val / norm);
                    if (!postings.TryGetValue(fi, out var list)) {
                        list = new List<(int, float)>();
                        postings[fi] = list;
                    }
                    list.Add((docIndex, normalized));
                }
            } else {
                var indices = v.GetIndices();
                var vals = v.GetValues();
                for (int k = 0; k < vals.Length; k++) {
                    int fi = indices[k];
                    float val = vals[k];
                    if (Math.Abs(val) <= 1e-12f)
                        continue;
                    float normalized = (float)(val / norm);
                    if (!postings.TryGetValue(fi, out var list)) {
                        list = new List<(int, float)>();
                        postings[fi] = list;
                    }
                    list.Add((docIndex, normalized));
                }
            }
        }

        private static void AccumulateScores(
            Dictionary<int, List<(int, float)>> postings,
            VBuffer<float> qvec,
            double qnorm,
            Dictionary<int, float> scoreMap)
        {
            if (qvec.IsDense) {
                var vals = qvec.GetValues();
                for (int fi = 0; fi < vals.Length; fi++) {
                    float qval = (float)(vals[fi] / qnorm);
                    if (Math.Abs(qval) <= 1e-12f)
                        continue;
                    if (!postings.TryGetValue(fi, out var plist))
                        continue;
                    foreach (var (docIndex, docVal) in plist) {
                        float contrib = qval * docVal;
                        if (scoreMap.TryGetValue(docIndex, out float cur))
                            scoreMap[docIndex] = cur + contrib;
                        else
                            scoreMap[docIndex] = contrib;
                    }
                }
            } else {
                var indices = qvec.GetIndices();
                var vals = qvec.GetValues();
                for (int k = 0; k < vals.Length; k++) {
                    int fi = indices[k];
                    float qval = (float)(vals[k] / qnorm);
                    if (Math.Abs(qval) <= 1e-12f)
                        continue;
                    if (!postings.TryGetValue(fi, out var plist))
                        continue;
                    foreach (var (docIndex, docVal) in plist) {
                        float contrib = qval * docVal;
                        if (scoreMap.TryGetValue(docIndex, out float cur))
                            scoreMap[docIndex] = cur + contrib;
                        else
                            scoreMap[docIndex] = contrib;
                    }
                }
            }
        }

        private static string BuildCacheKey(IList<(string key, string text)> items)
        {
            if (items.Count == 0)
                return string.Empty;
            string first = items[0].key;
            string last = items[items.Count - 1].key;
            return $"{items.Count}|{first}|{last}";
        }

        public void Dispose()
        {
            if (!_disposed) {
                _adHocCache.Dispose();
                _disposed = true;
            }
        }
    }
}
