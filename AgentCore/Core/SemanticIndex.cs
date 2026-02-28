using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hnsw;
using Hnsw.RamStorage;
using Microsoft.Data.Sqlite;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// A single search/retrieval result item.
    /// </summary>
    public class SearchResultItem
    {
        public string Id = string.Empty;
        public string Content = string.Empty;
        public string Metadata = string.Empty;
        public double Score;
        public long CreatedAt;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Content);
            sb.Append(' ');
            sb.Append(Metadata);
            sb.Append(' ');
            sb.Append('[');
            sb.Append(Id);
            sb.Append("]");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Persistent semantic index backed by SQLite (content storage) and HnswLite RAM index (ANN search).
    /// Each collection is loaded in the background; writes during loading are queued and applied after ready.
    /// </summary>
    public class SemanticIndex : IDisposable
    {
        private enum CollectionState { NotLoaded, Loading, Ready, Failed }

        private class PendingAdd
        {
            public Guid Guid;
            public List<float> Vector = new List<float>();
        }

        private class CollectionIndex
        {
            public string Name = string.Empty;
            public HnswIndex? Graph;
            // HNSW Guid -> record id (same value, Guid.ToString("N"))
            public ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
            public TaskCompletionSource<bool> ReadyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            public CollectionState State = CollectionState.NotLoaded;
            public List<PendingAdd> PendingAdds = new List<PendingAdd>();
            public readonly object PendingLock = new object();
        }

        private readonly string _dbPath;
        private readonly Dictionary<string, CollectionIndex> _collections = new Dictionary<string, CollectionIndex>(StringComparer.Ordinal);
        private readonly object _collectionsLock = new object();
        private RerankService? _rerankService;
        private bool _disposed = false;

        /// <summary>Fired when a collection finishes background loading.</summary>
        public event Action<string>? OnCollectionReady;

        /// <summary>Set an optional reranker to apply after HNSW+BM25 retrieval.</summary>
        public void SetRerankService(RerankService? reranker) => _rerankService = reranker;

        public SemanticIndex(string dbPath)
        {
            _dbPath = dbPath;
            EnsureSchema();
        }

        // ---- Public API ----

        /// <summary>
        /// Start background loading of a collection. Returns immediately.
        /// </summary>
        public void InitCollection(string collection)
        {
            var col = GetOrCreateCollection(collection);
            lock (col.PendingLock) {
                if (col.State != CollectionState.NotLoaded)
                    return;
                col.State = CollectionState.Loading;
            }
            Task.Run(() => LoadCollectionAsync(col));
        }

        /// <summary>
        /// Wait until the collection is ready (background load complete).
        /// </summary>
        public Task WaitReadyAsync(string collection, CancellationToken ct = default)
        {
            var col = GetOrCreateCollection(collection);
            if (col.State == CollectionState.Ready)
                return Task.CompletedTask;
            return col.ReadyTcs.Task.WaitAsync(ct);
        }

        public bool IsReady(string collection)
        {
            lock (_collectionsLock) {
                return _collections.TryGetValue(collection, out var col) && col.State == CollectionState.Ready;
            }
        }

        /// <summary>
        /// Add a record. Always writes to SQLite immediately.
        /// If the HNSW graph is ready, also inserts into the graph; otherwise queues for later.
        /// Returns the new record id.
        /// </summary>
        public string Add(string collection, string content, float[] vector, string? metadata = null)
        {
            var guid = Guid.NewGuid();
            string id = guid.ToString("N");
            long createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // 1. persist to SQLite
            using var conn = OpenConnection();
            using var txn = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = txn;
            cmd.CommandText = "INSERT INTO semantic_records(id,collection,content,metadata,vector,created_at) VALUES(@id,@col,@content,@meta,@vec,@ts)";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@col", collection);
            cmd.Parameters.AddWithValue("@content", content);
            cmd.Parameters.AddWithValue("@meta", (object?)metadata ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@vec", VectorToBytes(vector));
            cmd.Parameters.AddWithValue("@ts", createdAt);
            cmd.ExecuteNonQuery();
            using var ftsCmd = conn.CreateCommand();
            ftsCmd.Transaction = txn;
            ftsCmd.CommandText = "INSERT INTO semantic_fts(id,content,collection) VALUES(@id,@content,@col)";
            ftsCmd.Parameters.AddWithValue("@id", id);
            ftsCmd.Parameters.AddWithValue("@content", content);
            ftsCmd.Parameters.AddWithValue("@col", collection);
            ftsCmd.ExecuteNonQuery();
            txn.Commit();

            // 2. insert into HNSW graph or queue
            var col = GetOrCreateCollection(collection);
            bool queued = false;
            lock (col.PendingLock) {
                if (col.State != CollectionState.Ready) {
                    col.PendingAdds.Add(new PendingAdd { Guid = guid, Vector = vector.ToList() });
                    queued = true;
                }
            }
            if (!queued) {
                col.Lock.EnterWriteLock();
                try {
                    col.Graph!.AddAsync(guid, vector.ToList()).GetAwaiter().GetResult();
                }
                finally {
                    col.Lock.ExitWriteLock();
                }
            }
            return id;
        }

        /// <summary>
        /// Search the collection. Waits (synchronously) up to timeoutMs for the collection to be ready.
        /// Returns JSON array of {id, content, metadata, score}.
        /// </summary>
        public string SemanticSearch(string collection, float[] queryVector, string query = "", int topN = 5, int timeoutMs = 5000)
        {
            var items = SemanticSearchCore(collection, queryVector, query, topN, timeoutMs);
            return ResultsToJson(items);
        }

        /// <summary>
        /// Core semantic search returning structured results.
        /// </summary>
        public List<SearchResultItem> SemanticSearchCore(string collection, float[] queryVector, string query = "", int topN = 5, int timeoutMs = 5000)
        {
            return SemanticSearchCoreInternal(collection, queryVector, query, topN, timeoutMs, 0, 0);
        }

        /// <summary>
        /// Semantic search within a time range. startTime/endTime are unix timestamps. endTime=0 means now.
        /// </summary>
        public List<SearchResultItem> SemanticSearchBetweenCore(string collection, float[] queryVector, string query, int topN, long startTime, long endTime, int timeoutMs = 5000)
        {
            if (endTime <= 0) endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return SemanticSearchCoreInternal(collection, queryVector, query, topN, timeoutMs, startTime, endTime);
        }

        private List<SearchResultItem> SemanticSearchCoreInternal(string collection, float[] queryVector, string query, int topN, int timeoutMs, long startTime, long endTime)
        {
            var col = GetOrCreateCollection(collection);
            if (col.State != CollectionState.Ready) {
                col.ReadyTcs.Task.Wait(timeoutMs);
                if (col.State != CollectionState.Ready)
                    return new List<SearchResultItem>();
            }

            IEnumerable<VectorResult> hits;
            col.Lock.EnterReadLock();
            try {
                if (col.Graph == null)
                    return new List<SearchResultItem>();
                hits = col.Graph.GetTopKAsync(queryVector.ToList(), topN).GetAwaiter().GetResult();
            }
            finally {
                col.Lock.ExitReadLock();
            }

            if (!hits.Any())
                return new List<SearchResultItem>();

            // expand recall pool when reranker is available
            bool hasReranker = _rerankService != null && _rerankService.IsReady && !string.IsNullOrWhiteSpace(query);
            int recallN = hasReranker ? topN * 10 : topN;

            // build candidate id list
            var candidates = hits.Select(h => new {
                Id = h.GUID.ToString("N"),
                VectorScore = 1.0 - (double)h.Distance  // cosine distance -> similarity
            }).ToList();

            // fetch BM25 scores from FTS5
            var bm25Map = new Dictionary<string, double>(StringComparer.Ordinal);
            using var conn = OpenConnection();
            string ftsQuery = BuildFtsQuery(query);
            if (!string.IsNullOrEmpty(ftsQuery)) {
                var idList = string.Join(",", candidates.Select(c => $"'{c.Id}'"));
                using var ftsCmd = conn.CreateCommand();
                ftsCmd.CommandText = $"SELECT id, bm25(semantic_fts) FROM semantic_fts WHERE collection=@col AND content MATCH @q AND id IN ({idList})";
                ftsCmd.Parameters.AddWithValue("@col", collection);
                ftsCmd.Parameters.AddWithValue("@q", ftsQuery);
                using var ftsReader = ftsCmd.ExecuteReader();
                while (ftsReader.Read()) {
                    string fid = ftsReader.GetString(0);
                    double raw = ftsReader.GetDouble(1); // negative in SQLite FTS5
                    bm25Map[fid] = 1.0 / (1.0 + Math.Abs(raw)); // normalize to (0,1]
                }
            }

            // hybrid scoring: 0.6 * vector + 0.4 * bm25
            var hybridRanked = candidates
                .Select(c => new {
                    c.Id,
                    HybridScore = 0.6 * c.VectorScore + 0.4 * (bm25Map.TryGetValue(c.Id, out var b) ? b : 0.0)
                })
                .OrderByDescending(c => c.HybridScore)
                .Take(recallN)
                .ToList();

            // fetch content for reranker or final output
            bool hasTimeFilter = startTime > 0 && endTime > 0;
            var idContentMap = new Dictionary<string, (string content, string meta, long createdAt)>(StringComparer.Ordinal);
            foreach (var item in hybridRanked) {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT content,metadata,created_at FROM semantic_records WHERE id=@id";
                cmd.Parameters.AddWithValue("@id", item.Id);
                using var reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    long ts = reader.GetInt64(2);
                    if (hasTimeFilter && (ts < startTime || ts > endTime))
                        continue;
                    idContentMap[item.Id] = (reader.GetString(0), reader.IsDBNull(1) ? "" : reader.GetString(1), ts);
                }
            }

            // rerank if available
            IList<(string id, double score)> finalRanked;
            if (hasReranker) {
                var rerankCandidates = hybridRanked
                    .Where(c => idContentMap.ContainsKey(c.Id))
                    .Select(c => (c.Id, idContentMap[c.Id].content))
                    .ToList();
                var rerankResults = _rerankService!.Rerank(query, rerankCandidates, topN);
                finalRanked = rerankResults.Select(r => (r.key, (double)r.score)).ToList();
            } else {
                finalRanked = hybridRanked
                    .Where(c => idContentMap.ContainsKey(c.Id))
                    .Select(c => (c.Id, c.HybridScore))
                    .ToList();
            }

            var results = new List<SearchResultItem>();
            foreach (var (id, score) in finalRanked) {
                if (!idContentMap.TryGetValue(id, out var rec)) continue;
                results.Add(new SearchResultItem {
                    Id = id,
                    Content = rec.content,
                    Metadata = rec.meta,
                    Score = score,
                    CreatedAt = rec.createdAt
                });
            }
            return results;
        }

        /// <summary>Delete all records in a collection and reset its index.</summary>
        public void DeleteCollection(string collection)
        {
            using var conn = OpenConnection();
            using var txn = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = txn;
            cmd.CommandText = "DELETE FROM semantic_records WHERE collection=@col";
            cmd.Parameters.AddWithValue("@col", collection);
            cmd.ExecuteNonQuery();
            using var ftsCmd = conn.CreateCommand();
            ftsCmd.Transaction = txn;
            ftsCmd.CommandText = "DELETE FROM semantic_fts WHERE collection=@col";
            ftsCmd.Parameters.AddWithValue("@col", collection);
            ftsCmd.ExecuteNonQuery();
            txn.Commit();

            lock (_collectionsLock) {
                if (_collections.TryGetValue(collection, out var col)) {
                    col.Lock.EnterWriteLock();
                    try {
                        col.Graph = null;
                        col.State = CollectionState.NotLoaded;
                        col.ReadyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    }
                    finally {
                        col.Lock.ExitWriteLock();
                    }
                }
            }
        }

        /// <summary>Return the number of records in a collection.</summary>
        public int Count(string collection)
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM semantic_records WHERE collection=@col";
            cmd.Parameters.AddWithValue("@col", collection);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Return the most recent N records in a collection ordered by created_at ASC (oldest first).
        /// Does not require the collection to be initialized. Returns JSON array of {id, content, metadata}.
        /// </summary>
        public string GetRecent(string collection, int topN = 20)
        {
            var items = GetRecentCore(collection, topN);
            return ResultsToJson(items, includeScore: false);
        }

        /// <summary>
        /// Core method: return the most recent N records as a List, ordered by created_at ASC (oldest first).
        /// </summary>
        public List<SearchResultItem> GetRecentCore(string collection, int topN = 20)
        {
            var result = new List<SearchResultItem>();
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id, content, metadata, created_at FROM (
    SELECT id, content, metadata, created_at FROM semantic_records WHERE collection=@col ORDER BY created_at DESC LIMIT @n
) ORDER BY created_at ASC";
            cmd.Parameters.AddWithValue("@col", collection);
            cmd.Parameters.AddWithValue("@n", topN);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) {
                result.Add(new SearchResultItem {
                    Id = reader.GetString(0),
                    Content = reader.GetString(1),
                    Metadata = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    CreatedAt = reader.GetInt64(3)
                });
            }
            return result;
        }

        /// <summary>
        /// Return the most recent N records' content as a List&lt;string&gt;, ordered by created_at ASC (oldest first).
        /// </summary>
        public List<string> GetRecentContentList(string collection, int topN = 20)
        {
            return GetRecentCore(collection, topN).Select(r => r.Content).ToList();
        }

        // ---- Private helpers ----

        private async Task LoadCollectionAsync(CollectionIndex col)
        {
            try {
                int dim = 0;
                var ids = new List<Guid>();
                var vectors = new List<List<float>>();

                using (var conn = OpenConnection()) {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT id,vector FROM semantic_records WHERE collection=@col ORDER BY created_at";
                    cmd.Parameters.AddWithValue("@col", col.Name);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read()) {
                        var v = BytesToVector((byte[])reader[1]);
                        if (dim == 0) dim = v.Length;
                        ids.Add(Guid.Parse(reader.GetString(0)));
                        vectors.Add(v.ToList());
                    }
                }

                if (dim == 0) dim = 768; // default embedding dimension

                var graph = new HnswIndex(dim, new RamHnswStorage(), new RamHnswLayerStorage());
                graph.DistanceFunction = new CosineDistance();
                graph.M = 16;
                graph.EfConstruction = 200;

                if (ids.Count > 0) {
                    var batch = new Dictionary<Guid, List<float>>();
                    for (int i = 0; i < ids.Count; i++)
                        batch[ids[i]] = vectors[i];
                    await graph.AddNodesAsync(batch);
                }

                col.Lock.EnterWriteLock();
                try {
                    col.Graph = graph;
                }
                finally {
                    col.Lock.ExitWriteLock();
                }

                // apply pending adds
                List<PendingAdd> pending;
                lock (col.PendingLock) {
                    col.State = CollectionState.Ready;
                    pending = new List<PendingAdd>(col.PendingAdds);
                    col.PendingAdds.Clear();
                }

                if (pending.Count > 0) {
                    col.Lock.EnterWriteLock();
                    try {
                        var batch = new Dictionary<Guid, List<float>>();
                        foreach (var p in pending)
                            batch[p.Guid] = p.Vector;
                        await graph.AddNodesAsync(batch);
                    }
                    finally {
                        col.Lock.ExitWriteLock();
                    }
                }

                col.ReadyTcs.TrySetResult(true);
                OnCollectionReady?.Invoke(col.Name);
            }
            catch (Exception ex) {
                lock (col.PendingLock) {
                    col.State = CollectionState.Failed;
                }
                col.ReadyTcs.TrySetException(ex);
            }
        }

        private CollectionIndex GetOrCreateCollection(string collection)
        {
            lock (_collectionsLock) {
                if (!_collections.TryGetValue(collection, out var col)) {
                    col = new CollectionIndex { Name = collection };
                    _collections[collection] = col;
                }
                return col;
            }
        }

        private void EnsureSchema()
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS semantic_records (
    id TEXT PRIMARY KEY,
    collection TEXT NOT NULL,
    content TEXT NOT NULL,
    metadata TEXT,
    vector BLOB NOT NULL,
    created_at INTEGER NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_collection ON semantic_records(collection);
CREATE VIRTUAL TABLE IF NOT EXISTS semantic_fts USING fts5(id UNINDEXED, content, collection UNINDEXED);";
            cmd.ExecuteNonQuery();
        }

        private SqliteConnection OpenConnection()
        {
            var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                // WAL mode allows concurrent reads while writing; busy_timeout avoids immediate SQLITE_BUSY errors
                cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;";
                cmd.ExecuteNonQuery();
            }
            return conn;
        }

        private static byte[] VectorToBytes(float[] v)
        {
            var bytes = new byte[v.Length * 4];
            Buffer.BlockCopy(v, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static float[] BytesToVector(byte[] b)
        {
            var v = new float[b.Length / 4];
            Buffer.BlockCopy(b, 0, v, 0, b.Length);
            return v;
        }

        /// <summary>
        /// Build a FTS5 MATCH query string from the raw text query.
        /// Splits on whitespace, escapes special chars, joins with OR.
        /// </summary>
        private static string BuildFtsQuery(string rawQuery)
        {
            if (string.IsNullOrWhiteSpace(rawQuery))
                return string.Empty;
            var tokens = rawQuery.Split(new char[] { ' ', '\t', '\n', '\r', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                return string.Empty;
            // FTS5: escape double-quotes by doubling them, wrap each token in quotes
            var parts = tokens.Select(t => '"' + t.Replace("\"", "\"\"") + '"');
            return string.Join(" OR ", parts);
        }

        /// <summary>
        /// Pure BM25-based keyword search on FTS5 index. Does not require HNSW graph or embedding model.
        /// Returns JSON array of {id, content, metadata, score}.
        /// </summary>
        public string KeywordSearch(string collection, string query, int topN = 5)
        {
            var items = KeywordSearchCore(collection, query, topN);
            return ResultsToJson(items);
        }

        /// <summary>
        /// Core keyword search returning structured results.
        /// </summary>
        public List<SearchResultItem> KeywordSearchCore(string collection, string query, int topN = 5)
        {
            return KeywordSearchCoreInternal(collection, query, topN, 0, 0);
        }

        /// <summary>
        /// Keyword search within a time range. startTime/endTime are unix timestamps. endTime=0 means now.
        /// </summary>
        public List<SearchResultItem> KeywordSearchBetweenCore(string collection, string query, int topN, long startTime, long endTime)
        {
            if (endTime <= 0) endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return KeywordSearchCoreInternal(collection, query, topN, startTime, endTime);
        }

        private List<SearchResultItem> KeywordSearchCoreInternal(string collection, string query, int topN, long startTime, long endTime)
        {
            string ftsQuery = BuildFtsQuery(query);
            if (string.IsNullOrEmpty(ftsQuery))
                return new List<SearchResultItem>();

            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT f.id, bm25(semantic_fts) AS score
FROM semantic_fts f
WHERE f.collection=@col AND f.content MATCH @q
ORDER BY score
LIMIT @n";
            cmd.Parameters.AddWithValue("@col", collection);
            cmd.Parameters.AddWithValue("@q", ftsQuery);
            cmd.Parameters.AddWithValue("@n", topN);

            bool hasTimeFilter = startTime > 0 && endTime > 0;
            var idScores = new List<(string id, double score)>();
            using (var reader = cmd.ExecuteReader()) {
                while (reader.Read()) {
                    string id = reader.GetString(0);
                    double raw = reader.GetDouble(1);
                    double normalized = 1.0 / (1.0 + Math.Abs(raw));
                    idScores.Add((id, normalized));
                }
            }

            if (idScores.Count == 0)
                return new List<SearchResultItem>();

            var results = new List<SearchResultItem>();
            foreach (var (id, score) in idScores) {
                using var recCmd = conn.CreateCommand();
                recCmd.CommandText = "SELECT content,metadata,created_at FROM semantic_records WHERE id=@id";
                recCmd.Parameters.AddWithValue("@id", id);
                using var recReader = recCmd.ExecuteReader();
                if (recReader.Read()) {
                    long ts = recReader.GetInt64(2);
                    if (hasTimeFilter && (ts < startTime || ts > endTime))
                        continue;
                    results.Add(new SearchResultItem {
                        Id = id,
                        Content = recReader.GetString(0),
                        Metadata = recReader.IsDBNull(1) ? "" : recReader.GetString(1),
                        Score = score,
                        CreatedAt = ts
                    });
                }
            }
            return results;
        }

        private static string EscapeJson(string s)
        {
            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
        }

        /// <summary>
        /// Convert a list of SearchResultItem to JSON array string.
        /// </summary>
        public static string ResultsToJson(List<SearchResultItem> items, bool includeScore = true)
        {
            var sb = new StringBuilder("[");
            bool first = true;
            foreach (var item in items) {
                if (!first) sb.Append(',');
                first = false;
                if (includeScore)
                    sb.Append($"{{\"id\":{EscapeJson(item.Id)},\"content\":{EscapeJson(item.Content)},\"metadata\":{EscapeJson(item.Metadata)},\"score\":{item.Score:F4}}}");
                else
                    sb.Append($"{{\"id\":{EscapeJson(item.Id)},\"content\":{EscapeJson(item.Content)},\"metadata\":{EscapeJson(item.Metadata)}}}");
            }
            sb.Append(']');
            return sb.ToString();
        }

        /// <summary>
        /// Parse a time string in "yyyyMMdd hhmmss" or "yyyyMMdd" format to unix timestamp (UTC).
        /// </summary>
        public static long ParseTimeToUnix(string timeStr)
        {
            timeStr = timeStr.Trim();
            DateTime dt;
            if (timeStr.Length <= 8)
                dt = DateTime.ParseExact(timeStr, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            else
                dt = DateTime.ParseExact(timeStr, "yyyyMMdd HHmmss", System.Globalization.CultureInfo.InvariantCulture);
            return new DateTimeOffset(dt, TimeSpan.Zero).ToUnixTimeSeconds();
        }

        public void Dispose()
        {
            if (!_disposed) {
                _disposed = true;
                lock (_collectionsLock) {
                    foreach (var col in _collections.Values)
                        col.Lock.Dispose();
                    _collections.Clear();
                }
            }
        }
    }
}
