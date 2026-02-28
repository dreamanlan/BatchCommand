using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// BM25-based bag-of-words search service backed by Lucene.Net (in-memory index).
    /// Provides the same Search interface as EmbeddingService.
    /// Optionally accepts a MixedSegmenterEnhanced for Chinese/English tokenization.
    /// </summary>
    public class BagOfWordsService : IDisposable
    {
        private const LuceneVersion LUCENE_VER = LuceneVersion.LUCENE_48;
        private const string FIELD_KEY = "key";
        private const string FIELD_CONTENT = "content";
        private const string FIELD_ORIGINAL = "original";

        private readonly object _lock = new object();
        private readonly MixedSegmenterEnhanced? _segmenter;

        // built-in index (from BuildIndex)
        private RAMDirectory? _indexDir;
        private DirectoryReader? _indexReader;
        private IndexSearcher? _indexSearcher;

        // ad-hoc cache: avoid rebuilding index for same candidate set
        private readonly ThreadLocal<(string cacheKey, RAMDirectory dir, DirectoryReader reader, IndexSearcher searcher)> _adHocCache =
            new ThreadLocal<(string, RAMDirectory, DirectoryReader, IndexSearcher)>(
                () => (string.Empty, null!, null!, null!));

        private bool _disposed = false;

        public bool IsReady => true;

        /// <summary>
        /// Create a BagOfWordsService without a word segmenter (uses Lucene StandardAnalyzer).
        /// </summary>
        public BagOfWordsService() : this(null) { }

        /// <summary>
        /// Create a BagOfWordsService with an optional mixed segmenter for Chinese/English tokenization.
        /// </summary>
        public BagOfWordsService(MixedSegmenterEnhanced? segmenter)
        {
            _segmenter = segmenter;
        }

        /// <summary>
        /// Build (or rebuild) the in-memory BM25 index from a collection of (key, text) pairs.
        /// </summary>
        public void BuildIndex(IEnumerable<(string key, string text)> items)
        {
            lock (_lock) {
                CloseIndex();
                var dir = new RAMDirectory();
                WriteToDirectory(dir, items);
                _indexDir = dir;
                _indexReader = DirectoryReader.Open(dir);
                _indexSearcher = new IndexSearcher(_indexReader) { Similarity = new BM25Similarity() };
            }
        }

        /// <summary>
        /// BM25 search over the current built-in index.
        /// Returns null if the index is not ready.
        /// </summary>
        public IList<(string key, string text, float score)>? Search(IList<string> queries, int topN)
        {
            lock (_lock) {
                if (_indexSearcher == null)
                    return null;
                return RunSearch(_indexSearcher, queries, topN);
            }
        }

        /// <summary>
        /// BM25 search over an ad-hoc set of (key, text) candidates (no built-in index required).
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
            IndexSearcher searcher;

            if (cache.cacheKey == newCacheKey && cache.searcher != null) {
                searcher = cache.searcher;
            } else {
                // dispose old cached resources
                try { cache.reader?.Dispose(); } catch { }
                try { cache.dir?.Dispose(); } catch { }

                var dir = new RAMDirectory();
                WriteToDirectory(dir, candidateList);
                var reader = DirectoryReader.Open(dir);
                searcher = new IndexSearcher(reader) { Similarity = new BM25Similarity() };
                _adHocCache.Value = (newCacheKey, dir, reader, searcher);
            }

            return RunSearch(searcher, queries, topN);
        }

        // ---- private helpers ----

        private IList<(string key, string text, float score)> RunSearch(
            IndexSearcher searcher,
            IList<string> queries,
            int topN)
        {
            // union: key -> best score across all queries
            var best = new Dictionary<string, float>(StringComparer.Ordinal);
            var textMap = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var query in queries) {
                var bq = BuildBooleanQuery(query);
                if (bq == null)
                    continue;

                var hits = searcher.Search(bq, topN * queries.Count + topN);
                foreach (var hit in hits.ScoreDocs) {
                    var doc = searcher.Doc(hit.Doc);
                    string key = doc.Get(FIELD_KEY);
                    string text = doc.Get(FIELD_ORIGINAL) ?? doc.Get(FIELD_CONTENT);
                    float score = hit.Score;
                    if (!best.TryGetValue(key, out float prev) || score > prev)
                        best[key] = score;
                    if (!textMap.ContainsKey(key))
                        textMap[key] = text;
                }
            }

            return best
                .Select(p => (key: p.Key, text: textMap.TryGetValue(p.Key, out string? t) ? t : string.Empty, score: p.Value))
                .OrderByDescending(p => p.score)
                .Take(topN)
                .ToList();
        }

        private BooleanQuery? BuildBooleanQuery(string queryText)
        {
            var tokens = Tokenize(queryText);
            if (tokens.Count == 0)
                return null;

            var bq = new BooleanQuery();
            foreach (var token in tokens) {
                var tq = new TermQuery(new Term(FIELD_CONTENT, token));
                bq.Add(tq, Occur.SHOULD);
            }
            return bq;
        }

        /// <summary>
        /// Tokenize the provided text. If a MixedSegmenterEnhanced is present use it.
        /// Otherwise use Lucene's StandardAnalyzer to produce the same tokens as indexing.
        /// Returned tokens are normalized to lower-case and deduplicated.
        /// </summary>
        private List<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            if (_segmenter != null) {
                // use mixed segmenter for Chinese/English-aware tokenization
                return _segmenter.Segment(text)
                    .Select(s => s.ToLowerInvariant())
                    .Where(s => s.Length > 0)
                    .Distinct()
                    .ToList();
            } else {
                // use the same analyzer as indexing to ensure token consistency
                return AnalyzeWithStandardAnalyzer(text)
                    .Select(s => s.ToLowerInvariant())
                    .Where(s => s.Length > 0)
                    .Distinct()
                    .ToList();
            }
        }

        /// <summary>
        /// Analyze text using Lucene StandardAnalyzer and return token terms.
        /// This ensures query-time tokenization matches index-time tokenization when no segmenter is provided.
        /// </summary>
        private List<string> AnalyzeWithStandardAnalyzer(string text)
        {
            var tokens = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
                return tokens;

            using var analyzer = new StandardAnalyzer(LUCENE_VER);
            using var reader = new StringReader(text);
            using var ts = analyzer.GetTokenStream(FIELD_CONTENT, reader);
            var termAttr = ts.GetAttribute<ICharTermAttribute>();
            ts.Reset();
            while (ts.IncrementToken()) {
                var term = termAttr.ToString();
                if (!string.IsNullOrEmpty(term))
                    tokens.Add(term);
            }
            ts.End();
            return tokens;
        }

        /// <summary>
        /// Write items into given RAMDirectory. If a MixedSegmenterEnhanced is set, pre-tokenize
        /// content with the segmenter and index using WhitespaceAnalyzer on the tokenized string.
        /// Otherwise use StandardAnalyzer on raw text.
        /// This ensures index-time tokenization matches query-time tokenization.
        /// </summary>
        private void WriteToDirectory(RAMDirectory dir, IEnumerable<(string key, string text)> items)
        {
            if (_segmenter != null) {
                // When segmenter is provided, pre-tokenize each document and index tokenized string
                // with WhitespaceAnalyzer so that tokens match segmenter tokens.
                using var analyzer = new WhitespaceAnalyzer(LUCENE_VER);
                var config = new IndexWriterConfig(LUCENE_VER, analyzer) {
                    Similarity = new BM25Similarity()
                };
                using var writer = new IndexWriter(dir, config);
                foreach (var (key, text) in items) {
                    var doc = new Document();
                    doc.Add(new StringField(FIELD_KEY, key, Field.Store.YES));
                    // pre-tokenize with segmenter and join tokens with single space (lower-cased)
                    var tokens = _segmenter.Segment(text)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.ToLowerInvariant());
                    var tokenizedText = string.Join(" ", tokens);
                    // index tokenizedText (whitespace analyzer will treat each token as term)
                    doc.Add(new TextField(FIELD_CONTENT, tokenizedText, Field.Store.NO));
                    // store original text for retrieval
                    doc.Add(new StoredField(FIELD_ORIGINAL, text));
                    writer.AddDocument(doc);
                }
                writer.Commit();
            } else {
                // No segmenter: use StandardAnalyzer on raw text (unchanged behavior)
                using var analyzer = new StandardAnalyzer(LUCENE_VER);
                var config = new IndexWriterConfig(LUCENE_VER, analyzer) {
                    Similarity = new BM25Similarity()
                };
                using var writer = new IndexWriter(dir, config);
                foreach (var (key, text) in items) {
                    var doc = new Document();
                    doc.Add(new StringField(FIELD_KEY, key, Field.Store.YES));
                    // store original text for retrieval; index tokenized content via StandardAnalyzer
                    doc.Add(new TextField(FIELD_CONTENT, text, Field.Store.YES));
                    writer.AddDocument(doc);
                }
                writer.Commit();
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

        private void CloseIndex()
        {
            try { _indexReader?.Dispose(); } catch { }
            try { _indexDir?.Dispose(); } catch { }
            _indexReader = null;
            _indexSearcher = null;
            _indexDir = null;
        }

        public void Dispose()
        {
            if (!_disposed) {
                lock (_lock) {
                    CloseIndex();
                }
                var cache = _adHocCache.Value!;
                try { cache.reader?.Dispose(); } catch { }
                try { cache.dir?.Dispose(); } catch { }
                _adHocCache.Dispose();
                _disposed = true;
            }
        }
    }
}
