using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Cross-encoder reranker service using an ONNX model (e.g. bge-reranker-base).
    /// Input: concatenated [query, doc] pair; output: logits[0,0] -> sigmoid score.
    /// </summary>
    public class RerankService : IDisposable
    {
        private InferenceSession? _session;
        private Tokenizer? _tokenizer;
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        private bool _hasTokenTypeIds = false;
        private bool _disposed = false;

        /// <summary>Whether the reranker model is loaded and ready.</summary>
        public bool IsReady => _session != null && _tokenizer != null;

        /// <summary>
        /// Load ONNX model and tokenizer. Supports both vocab.txt (BERT) and .model (SentencePiece).
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
                } else {
                    _tokenizer = BertTokenizer.Create(tokenizerPath);
                }
                _hasTokenTypeIds = _session.InputMetadata.ContainsKey("token_type_ids");
            }
            finally { _rwLock.ExitWriteLock(); }
        }

        /// <summary>
        /// Rerank candidates by scoring each (query, candidate) pair.
        /// Returns candidates sorted by rerank score descending, limited to topN.
        /// </summary>
        public IList<(string key, string text, float score)> Rerank(string query, IList<(string key, string text)> candidates, int topN)
        {
            _rwLock.EnterReadLock();
            try {
                if (_session == null || _tokenizer == null || candidates.Count == 0)
                    return Array.Empty<(string, string, float)>();

                var scored = new List<(string key, string text, float score)>(candidates.Count);
                foreach (var (key, text) in candidates) {
                    float score = ScorePair(query, text);
                    scored.Add((key, text, score));
                }
                scored.Sort((a, b) => b.score.CompareTo(a.score));
                return scored.Take(topN).ToList();
            }
            finally { _rwLock.ExitReadLock(); }
        }

        // ---- private helpers ----

        private float ScorePair(string query, string doc)
        {
            if (_session == null || _tokenizer == null)
                return 0f;

            // cross-encoder: encode "query [SEP] doc" as a single sequence
            // SentencePiece tokenizer does not add special tokens automatically,
            // so we concatenate with a separator token manually.
            string combined = query + " </s> " + doc;
            var encoding = _tokenizer.EncodeToIds(combined, considerPreTokenization: true, considerNormalization: true);
            int seqLen = encoding.Count;
            if (seqLen == 0)
                return 0f;
            if (seqLen > 512)
                seqLen = 512;

            long[] inputIds = new long[seqLen];
            long[] attentionMask = new long[seqLen];
            long[] tokenTypeIds = new long[seqLen];

            for (int i = 0; i < seqLen; i++) {
                inputIds[i] = encoding[i];
                attentionMask[i] = 1;
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
            var logitsTensor = outputs.First(o => o.Name == "logits").AsTensor<float>();
            float logit = logitsTensor[0, 0];
            // sigmoid
            return 1f / (1f + (float)Math.Exp(-logit));
        }

        private void DisposeSession()
        {
            _session?.Dispose();
            _session = null;
            _tokenizer = null;
        }

        public void Dispose()
        {
            if (!_disposed) {
                _rwLock.EnterWriteLock();
                try {
                    DisposeSession();
                }
                finally { _rwLock.ExitWriteLock(); }
                _rwLock.Dispose();
                _disposed = true;
            }
        }
    }
}
