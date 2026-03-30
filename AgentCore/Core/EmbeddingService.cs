using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;
using HFTokenizer = Tokenizers.DotNet.Tokenizer;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Provides ONNX-based text embedding inference, index management and semantic search.
    /// </summary>
    public class EmbeddingService : IDisposable
    {
        private InferenceSession? _session;
        private Tokenizer? _tokenizer;
        private HFTokenizer? _hfTokenizer;
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        // index: key -> (keyVec, docVec, text) normalized embedding vectors + original text
        private Dictionary<string, (float[] keyVec, float[] docVec, string text)> _index = new Dictionary<string, (float[], float[], string)>(StringComparer.Ordinal);

        // per-thread cache for ad-hoc Search candidates encoding
        private readonly ThreadLocal<(string cacheKey, List<(string key, float[] keyVec, float[] docVec)> encoded)> _adHocCache =
            new ThreadLocal<(string, List<(string, float[], float[])>)>(() => (string.Empty, new List<(string, float[], float[])>()));

        private bool _hasTokenTypeIds = false;
        private bool _disposed = false;

        /// <summary>
        /// Whether the embedding model is loaded and ready.
        /// </summary>
        public bool IsReady => _session != null && (_tokenizer != null || _hfTokenizer != null);

        /// <summary>
        /// Load ONNX model and tokenizer from the given paths.
        /// </summary>
        public void Load(string modelPath, string tokenizerPath)
        {
            _rwLock.EnterWriteLock();
            try {
                DisposeSession();
                var opts = new SessionOptions();
                opts.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                _session = new InferenceSession(modelPath, opts);
                string tokExt = Path.GetExtension(tokenizerPath).ToLowerInvariant();
                if (tokExt == ".model") {
                    using var stream = File.OpenRead(tokenizerPath);
                    _tokenizer = SentencePieceTokenizer.Create(stream, addBeginningOfSentence: false, addEndOfSentence: false);
                } else if (tokExt == ".json") {
                    // HFTokenizer (Tokenizers.DotNet) with cross-platform native runtime
                    _hfTokenizer = new HFTokenizer(vocabPath: tokenizerPath);
                } else {
                    _tokenizer = BertTokenizer.Create(tokenizerPath);
                }
                _hasTokenTypeIds = _session.InputMetadata.ContainsKey("token_type_ids");
                _index.Clear();
            }
            finally { _rwLock.ExitWriteLock(); }
        }

        /// <summary>
        /// Build (or rebuild) the semantic index from a collection of (key, text) pairs.
        /// </summary>
        public void BuildIndex(IEnumerable<(string key, string text)> items)
        {
            _rwLock.EnterWriteLock();
            try {
                if (_session == null || _tokenizer == null)
                    return;
                _index.Clear();
                foreach (var (key, text) in items) {
                    var keyVec = EncodeInternal(AgentFrameworkService.CleanStringData(key));
                    var docVec = EncodeInternal(AgentFrameworkService.CleanStringData(text));
                    if (keyVec != null && docVec != null)
                        _index[key] = (keyVec, docVec, text);
                }
            }
            finally { _rwLock.ExitWriteLock(); }
        }

        /// <summary>
        /// Encode a single text to a normalized embedding vector.
        /// Returns null if the service is not ready.
        /// </summary>
        public float[]? Encode(string text)
        {
            _rwLock.EnterReadLock();
            try {
                return EncodeInternal(text);
            }
            finally { _rwLock.ExitReadLock(); }
        }

        /// <summary>
        /// Semantic search over the current index.
        /// Each query in <paramref name="queries"/> returns top-N results independently;
        /// final result is the union sorted by score descending (highest score per key wins).
        /// Returns null if the service is not ready.
        /// </summary>
        public IList<(string key, string text, float score)>? Search(IList<string> queries, int topN)
        {
            _rwLock.EnterReadLock();
            try {
                if (_session == null || _tokenizer == null || _index.Count == 0)
                    return null;

                // union: key -> best score across all queries
                var best = new Dictionary<string, float>(StringComparer.Ordinal);

                foreach (var query in queries) {
                    var qVec = EncodeInternal(query);
                    if (qVec == null)
                        continue;

                    // collect top-N for this query, blend key(70%) + doc(30%) scores
                    var scores = new List<(string key, float score)>(_index.Count);
                    foreach (var pair in _index) {
                        float keyScore = CosineSimilarity(qVec, pair.Value.keyVec);
                        float docScore = CosineSimilarity(qVec, pair.Value.docVec);
                        float score = 0.7f * keyScore + 0.3f * docScore;
                        scores.Add((pair.Key, score));
                    }
                    scores.Sort((a, b) => b.score.CompareTo(a.score));

                    int take = Math.Min(topN, scores.Count);
                    for (int i = 0; i < take; i++) {
                        var (key, score) = scores[i];
                        if (!best.TryGetValue(key, out float prev) || score > prev)
                            best[key] = score;
                    }
                }

                var result = best.Select(p => (key: p.Key, text: _index[p.Key].text, score: p.Value))
                                 .OrderByDescending(p => p.score)
                                 .ToList();
                return result;
            }
            finally { _rwLock.ExitReadLock(); }
        }

        /// <summary>
        /// Semantic search over an ad-hoc set of (key, text) candidates (no index required).
        /// Each query returns top-N results independently; final result is the union.
        /// Returns null if the service is not ready.
        /// </summary>
        public IList<(string key, string text, float score)>? Search(
            IList<string> queries,
            IEnumerable<(string key, string text)> candidates,
            int topN)
        {
            _rwLock.EnterReadLock();
            try {
                if (_session == null || _tokenizer == null)
                    return null;

                // use per-thread cache to avoid re-encoding candidates when unchanged
                var candidateList = candidates as IList<(string key, string text)>
                    ?? candidates.ToList();
                string newCacheKey = BuildCacheKey(candidateList);
                var cache = _adHocCache.Value!;
                List<(string key, float[] keyVec, float[] docVec)> encoded;
                if (cache.cacheKey == newCacheKey && cache.encoded.Count > 0) {
                    encoded = cache.encoded;
                } else {
                    encoded = new List<(string key, float[] keyVec, float[] docVec)>(candidateList.Count);
                    foreach (var (key, text) in candidateList) {
                        var keyVec = EncodeInternal(AgentFrameworkService.CleanStringData(key));
                        var docVec = EncodeInternal(AgentFrameworkService.CleanStringData(text));
                        if (keyVec != null && docVec != null)
                            encoded.Add((key, keyVec, docVec));
                    }
                    _adHocCache.Value = (newCacheKey, encoded);
                }
                if (encoded.Count == 0)
                    return null;

                var best = new Dictionary<string, float>(StringComparer.Ordinal);

                foreach (var query in queries) {
                    var qVec = EncodeInternal(query);
                    if (qVec == null)
                        continue;

                    var scores = new List<(string key, float score)>(encoded.Count);
                    foreach (var (key, keyVec, docVec) in encoded) {
                        float keyScore = CosineSimilarity(qVec, keyVec);
                        float docScore = CosineSimilarity(qVec, docVec);
                        float score = 0.7f * keyScore + 0.3f * docScore;
                        scores.Add((key, score));
                    }
                    scores.Sort((a, b) => b.score.CompareTo(a.score));

                    int take = Math.Min(topN, scores.Count);
                    for (int i = 0; i < take; i++) {
                        var (key, score) = scores[i];
                        if (!best.TryGetValue(key, out float prev) || score > prev)
                            best[key] = score;
                    }
                }

                // build a lookup from key to text for the encoded candidates
                var textLookup = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var (key, text) in candidateList)
                    textLookup[key] = text;
                return best.Select(p => (key: p.Key, text: textLookup.TryGetValue(p.Key, out string? t) ? t : string.Empty, score: p.Value))
                           .OrderByDescending(p => p.score)
                           .ToList();
            }
            finally { _rwLock.ExitReadLock(); }
        }

        // ---- private helpers ----

        private static string BuildCacheKey(IList<(string key, string text)> items)
        {
            if (items.Count == 0)
                return string.Empty;
            string first = items[0].key;
            string last = items[items.Count - 1].key;
            return $"{items.Count}|{first}|{last}";
        }

        private float[]? EncodeInternal(string text)
        {
            if (_session == null || (_tokenizer == null && _hfTokenizer == null))
                return null;

            int seqLen;
            long[] inputIds;
            long[] attentionMask = Array.Empty<long>();
            long[] tokenTypeIds = Array.Empty<long>();

            if (_hfTokenizer != null) {
                var ids = _hfTokenizer.Encode(text);
                seqLen = ids.Length;
                if (seqLen == 0)
                    return null;
                if (seqLen > 512)
                    seqLen = 512;
                inputIds = new long[seqLen];
                attentionMask = new long[seqLen];
                tokenTypeIds = new long[seqLen];
                for (int i = 0; i < seqLen; i++) {
                    inputIds[i] = ids[i];
                    attentionMask[i] = 1;
                    tokenTypeIds[i] = 0;
                }
            } else {
                var encoding = _tokenizer!.EncodeToIds(text, considerPreTokenization: true, considerNormalization: true);
                seqLen = encoding.Count;
                if (seqLen == 0)
                    return null;
                if (seqLen > 512)
                    seqLen = 512;
                inputIds = new long[seqLen];
                attentionMask = new long[seqLen];
                tokenTypeIds = new long[seqLen];
                for (int i = 0; i < seqLen; i++) {
                    inputIds[i] = encoding[i];
                    attentionMask[i] = 1;
                    tokenTypeIds[i] = 0;
                }
            }

            int[] shape = new int[] { 1, seqLen };

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids",
                    new DenseTensor<long>(inputIds, shape)),
                NamedOnnxValue.CreateFromTensor("attention_mask",
                    new DenseTensor<long>(attentionMask, shape)),
            };
            if (_hasTokenTypeIds) {
                inputs.Add(NamedOnnxValue.CreateFromTensor("token_type_ids",
                    new DenseTensor<long>(tokenTypeIds, shape)));
            }

            using var outputs = _session.Run(inputs);
            // last_hidden_state: shape [1, seqLen, hiddenSize]
            var rawOutput = outputs.First(o => o.Name == "last_hidden_state");

            int hiddenSize;
            float[] pooled;

            // mean pooling over non-padding tokens, support both fp32 and fp16 models
            if (rawOutput.ElementType == TensorElementType.Float16) {
                var lastHidden = rawOutput.AsTensor<Float16>();
                hiddenSize = (int)lastHidden.Dimensions[2];
                pooled = new float[hiddenSize];
                for (int t = 0; t < seqLen; t++) {
                    if (attentionMask[t] == 0) continue;
                    for (int h = 0; h < hiddenSize; h++)
                        pooled[h] += (float)lastHidden[0, t, h];
                }
            } else {
                var lastHidden = rawOutput.AsTensor<float>();
                hiddenSize = (int)lastHidden.Dimensions[2];
                pooled = new float[hiddenSize];
                for (int t = 0; t < seqLen; t++) {
                    if (attentionMask[t] == 0) continue;
                    for (int h = 0; h < hiddenSize; h++)
                        pooled[h] += lastHidden[0, t, h];
                }
            }
            int validTokens = (int)attentionMask.Sum(x => x);
            if (validTokens > 0) {
                for (int h = 0; h < hiddenSize; h++)
                    pooled[h] /= validTokens;
            }

            // L2 normalize
            float norm = 0f;
            for (int h = 0; h < hiddenSize; h++)
                norm += pooled[h] * pooled[h];
            norm = (float)Math.Sqrt(norm);
            if (norm > 1e-9f) {
                for (int h = 0; h < hiddenSize; h++)
                    pooled[h] /= norm;
            }

            return pooled;
        }

        private static float CosineSimilarity(float[] a, float[] b)
        {
            // both vectors are already L2-normalized, so dot product == cosine similarity
            float dot = 0f;
            int len = Math.Min(a.Length, b.Length);
            for (int i = 0; i < len; i++)
                dot += a[i] * b[i];
            return dot;
        }

        private void DisposeSession()
        {
            _session?.Dispose();
            _session = null;
            _tokenizer = null;
            _hfTokenizer?.Dispose();
            _hfTokenizer = null;
        }

        public void Dispose()
        {
            if (!_disposed) {
                _rwLock.EnterWriteLock();
                try {
                    DisposeSession();
                    _index.Clear();
                }
                finally { _rwLock.ExitWriteLock(); }
                _adHocCache.Dispose();
                _rwLock.Dispose();
                _disposed = true;
            }
        }

        /// <summary>
        /// Strip punctuation and separator characters from a string to produce clean tokens for semantic search.
        /// </summary>
        public static string CleanStringData(string text)
        {
            return AgentFrameworkService.CleanStringData(text);
        }
    }
}
