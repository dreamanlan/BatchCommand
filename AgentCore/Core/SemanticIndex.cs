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
        private MixedSegmenterEnhanced? _segmenter;
        private bool _disposed = false;

        // Configurable hybrid scoring weights (vector vs bm25)
        private double _vectorWeight = 0.6;
        private double _bm25Weight = 0.4;
        // HNSW recall multiplier: actual recall = topN * multiplier
        private int _hnswRecallMultiplier = 5;
        // FTS search scope: "all" (default), "content", "metadata"
        private string _searchScope = "content";
        // Detected FTS schema version at runtime
        private int _schemaVersion = 1;
        // Current schema version for new databases
        private const int CurrentSchemaVersion = 2;

        /// <summary>Fired when a collection finishes background loading.</summary>
        public event Action<string>? OnCollectionReady;

        /// <summary>Set an optional reranker to apply after HNSW+BM25 retrieval.</summary>
        public void SetRerankService(RerankService? reranker) => _rerankService = reranker;

        /// <summary>Set an optional segmenter for improved Chinese/mixed-language FTS5 tokenization.</summary>
        public void SetSegmenter(MixedSegmenterEnhanced? segmenter) => _segmenter = segmenter;

        /// <summary>Set hybrid scoring weights (must sum to 1.0). Default: vector=0.6, bm25=0.4.</summary>
        public void SetHybridWeights(double vectorWeight, double bm25Weight)
        {
            double sum = vectorWeight + bm25Weight;
            if (sum <= 0) return;
            _vectorWeight = vectorWeight / sum;
            _bm25Weight = bm25Weight / sum;
        }

        /// <summary>Set HNSW recall multiplier. Actual HNSW recall = topN * multiplier. Default: 5.</summary>
        public void SetHnswRecallMultiplier(int multiplier)
        {
            _hnswRecallMultiplier = Math.Max(1, multiplier);
        }

        /// <summary>Set FTS search scope: "all" (default), "content", or "metadata".</summary>
        public void SetSearchScope(string scope)
        {
            scope = (scope ?? "all").Trim().ToLowerInvariant();
            if (scope != "content" && scope != "metadata" && scope != "all")
                scope = "all";
            // If schema is V1 (no metadata column), metadata scope falls back to all
            if (_schemaVersion < 2 && scope == "metadata")
                scope = "all";
            _searchScope = scope;
        }

        /// <summary>Get current FTS search scope.</summary>
        public string GetSearchScope() => _searchScope;

        /// <summary>Get detected FTS schema version.</summary>
        public int GetSchemaVersion() => _schemaVersion;

        public SemanticIndex(string dbPath)
        {
            _dbPath = dbPath;
            EnsureSchema();
            _schemaVersion = DetectSchemaVersion();
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
            if (_schemaVersion >= 2) {
                ftsCmd.CommandText = "INSERT INTO semantic_fts(id,content,metadata,collection) VALUES(@id,@content,@meta,@col)";
                ftsCmd.Parameters.AddWithValue("@meta", SegmentForFts(metadata ?? ""));
            } else {
                ftsCmd.CommandText = "INSERT INTO semantic_fts(id,content,collection) VALUES(@id,@content,@col)";
            }
            ftsCmd.Parameters.AddWithValue("@id", id);
            // Use segmenter for FTS5 content if available, so BM25 works well with Chinese
            string ftsContent = SegmentForFts(content);
            ftsCmd.Parameters.AddWithValue("@content", ftsContent);
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

        // Incremental recall parameters for between-time search
        private const int BetweenRecallStart = 1000;
        private const int BetweenRecallStep = 1000;
        private const int BetweenRecallCap = 100_000;

        private List<SearchResultItem> SemanticSearchCoreInternal(string collection, float[] queryVector, string query, int topN, int timeoutMs, long startTime, long endTime)
        {
            var col = GetOrCreateCollection(collection);
            if (col.State != CollectionState.Ready) {
                col.ReadyTcs.Task.Wait(timeoutMs);
                if (col.State != CollectionState.Ready)
                    return new List<SearchResultItem>();
            }

            // Expand HNSW recall pool for better coverage
            bool hasReranker = _rerankService != null && _rerankService.IsReady && !string.IsNullOrWhiteSpace(query);
            int hnswRecallN = topN * _hnswRecallMultiplier;
            int finalRecallN = hasReranker ? topN * 10 : topN;

            bool hasTimeFilter = startTime > 0 && endTime > 0;
            if (hasTimeFilter) {
                return SemanticSearchBetweenInternal(col, collection, queryVector, query, topN, finalRecallN, hasReranker, startTime, endTime);
            }

            IEnumerable<VectorResult> hits;
            col.Lock.EnterReadLock();
            try {
                if (col.Graph == null)
                    return new List<SearchResultItem>();
                hits = col.Graph.GetTopKAsync(queryVector.ToList(), hnswRecallN).GetAwaiter().GetResult();
            }
            finally {
                col.Lock.ExitReadLock();
            }

            // build candidate map from HNSW results
            var candidateMap = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var h in hits) {
                string hid = h.GUID.ToString("N");
                candidateMap[hid] = 1.0 - (double)h.Distance; // cosine distance -> similarity
            }

            // fetch BM25 scores from FTS5 + independent BM25 recall channel
            var bm25Map = new Dictionary<string, double>(StringComparer.Ordinal);
            using var conn = OpenConnection();
            string ftsQuery = BuildFtsQuery(query);
            if (!string.IsNullOrEmpty(ftsQuery)) {
                // Score BM25 within HNSW candidates
                if (candidateMap.Count > 0) {
                    var idList = string.Join(",", candidateMap.Keys.Select(id => $"'{id}'"));
                    using var ftsCmd = conn.CreateCommand();
                    ftsCmd.CommandText = $"SELECT id, {GetBm25Expression()} FROM semantic_fts WHERE collection=@col AND semantic_fts MATCH @q AND id IN ({idList})";
                    ftsCmd.Parameters.AddWithValue("@col", collection);
                    ftsCmd.Parameters.AddWithValue("@q", ftsQuery);
                    using var ftsReader = ftsCmd.ExecuteReader();
                    while (ftsReader.Read()) {
                        string fid = ftsReader.GetString(0);
                        double raw = ftsReader.GetDouble(1); // negative in SQLite FTS5
                        bm25Map[fid] = 1.0 / (1.0 + Math.Abs(raw)); // normalize to (0,1]
                    }
                }
                // Independent BM25 recall: find additional candidates not in HNSW results
                using var bm25RecallCmd = conn.CreateCommand();
                bm25RecallCmd.CommandText = $"SELECT id, {GetBm25Expression()} AS score\nFROM semantic_fts\nWHERE collection=@col AND semantic_fts MATCH @q\nORDER BY score\nLIMIT @n";
                bm25RecallCmd.Parameters.AddWithValue("@col", collection);
                bm25RecallCmd.Parameters.AddWithValue("@q", ftsQuery);
                bm25RecallCmd.Parameters.AddWithValue("@n", hnswRecallN);
                using var bm25Reader = bm25RecallCmd.ExecuteReader();
                while (bm25Reader.Read()) {
                    string bid = bm25Reader.GetString(0);
                    double raw = bm25Reader.GetDouble(1);
                    double normalized = 1.0 / (1.0 + Math.Abs(raw));
                    if (!bm25Map.ContainsKey(bid))
                        bm25Map[bid] = normalized;
                    // Add BM25-only candidates to the candidate map (no vector score)
                    if (!candidateMap.ContainsKey(bid))
                        candidateMap[bid] = 0.0;
                }
            }

            if (candidateMap.Count == 0)
                return new List<SearchResultItem>();

            // hybrid scoring with configurable weights
            var hybridRanked = candidateMap
                .Select(c => new {
                    Id = c.Key,
                    HybridScore = _vectorWeight * c.Value + _bm25Weight * (bm25Map.TryGetValue(c.Key, out var b) ? b : 0.0)
                })
                .OrderByDescending(c => c.HybridScore)
                .Take(finalRecallN)
                .ToList();

            // fetch content for reranker or final output
            var idContentMap = new Dictionary<string, (string content, string meta, long createdAt)>(StringComparer.Ordinal);
            foreach (var item in hybridRanked) {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT content,metadata,created_at FROM semantic_records WHERE id=@id";
                cmd.Parameters.AddWithValue("@id", item.Id);
                using var reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    long ts = reader.GetInt64(2);
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
CREATE VIRTUAL TABLE IF NOT EXISTS semantic_fts USING fts5(id UNINDEXED, content, metadata, collection UNINDEXED);
CREATE TABLE IF NOT EXISTS semantic_schema_version (id INTEGER PRIMARY KEY, version INTEGER NOT NULL);";
            cmd.ExecuteNonQuery();
            // Only set current version for truly new databases.
            // For legacy databases (have records but no version row), insert version 1
            // so that MigrateFtsSchema() will run the proper migration.
            using var chkCmd = conn.CreateCommand();
            chkCmd.CommandText = "SELECT COUNT(*) FROM semantic_schema_version WHERE id=1";
            long hasVer = (long)chkCmd.ExecuteScalar()!;
            if (hasVer == 0) {
                using var cntCmd = conn.CreateCommand();
                cntCmd.CommandText = "SELECT COUNT(*) FROM semantic_records";
                long recCount = (long)cntCmd.ExecuteScalar()!;
                int verToInsert = recCount > 0 ? 1 : CurrentSchemaVersion;
                using var verCmd = conn.CreateCommand();
                verCmd.CommandText = "INSERT INTO semantic_schema_version(id, version) VALUES(1, @ver)";
                verCmd.Parameters.AddWithValue("@ver", verToInsert);
                verCmd.ExecuteNonQuery();
            }
        }

        /// <summary>Detect the current FTS schema version from the database.</summary>
        private int DetectSchemaVersion()
        {
            try {
                using var conn = OpenConnection();
                // Check if version table exists
                using var chkCmd = conn.CreateCommand();
                chkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='semantic_schema_version'";
                var result = chkCmd.ExecuteScalar();
                if (result == null)
                    return 1; // No version table means V1 (legacy)
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT version FROM semantic_schema_version WHERE id=1";
                var ver = cmd.ExecuteScalar();
                return ver != null ? Convert.ToInt32(ver) : 1;
            }
            catch {
                return 1;
            }
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
        /// Segment content for FTS5 storage. Uses MixedSegmenter if available for better
        /// Chinese tokenization; otherwise returns original content.
        /// </summary>
        private string SegmentForFts(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return content;
            if (_segmenter == null)
                return content;
            try {
                var tokens = _segmenter.Segment(content);
                if (tokens.Count == 0)
                    return content;
                return string.Join(" ", tokens);
            }
            catch {
                return content;
            }
        }

        /// <summary>
        /// Build a FTS5 MATCH query string from the raw text query.
        /// Uses MixedSegmenter for Chinese/mixed-language tokenization if available;
        /// otherwise falls back to whitespace/punctuation splitting.
        /// </summary>
        private string BuildFtsQuery(string rawQuery, bool useScope = false)
        {
            if (string.IsNullOrWhiteSpace(rawQuery))
                return string.Empty;
            IEnumerable<string> tokens;
            if (_segmenter != null) {
                try {
                    var segmented = _segmenter.Segment(rawQuery);
                    tokens = segmented.Where(t => !string.IsNullOrWhiteSpace(t));
                }
                catch {
                    tokens = rawQuery.Split(new char[] { ' ', '\t', '\n', '\r', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            else {
                tokens = rawQuery.Split(new char[] { ' ', '\t', '\n', '\r', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            }
            var tokenList = tokens.ToList();
            if (tokenList.Count == 0)
                return string.Empty;
            // FTS5: escape double-quotes by doubling them, wrap each token in quotes
            var parts = tokenList.Select(t => '"' + t.Replace("\"", "\"\"") + '"');
            string joined = string.Join(" OR ", parts);
            // Apply column scope for V2 schema (only when useScope=true, i.e. keyword_search)
            if (useScope && _schemaVersion >= 2) {
                if (_searchScope == "content")
                    return "content : (" + joined + ")";
                else if (_searchScope == "metadata")
                    return "metadata : (" + joined + ")";
            }
            return joined;
        }

        /// <summary>
        /// Get the bm25() SQL expression adjusted for current search scope.
        /// V2 schema has 2 indexed columns: content(0), metadata(1).
        /// </summary>
        private string GetBm25Expression(bool useScope = false)
        {
            if (_schemaVersion < 2)
                return "bm25(semantic_fts)";
            if (useScope) {
                if (_searchScope == "content")
                    return "bm25(semantic_fts, 1.0, 0.0)";
                if (_searchScope == "metadata")
                    return "bm25(semantic_fts, 0.0, 1.0)";
            }
            return "bm25(semantic_fts, 1.0, 1.0)";
        }

        /// <summary>
        /// Rebuild the FTS5 index for a collection using the current segmenter.
        /// Call this after changing the segmenter to re-tokenize all stored content.
        /// </summary>
        public int RebuildFtsIndex(string collection)
        {
            int count = 0;
            using var conn = OpenConnection();
            // Delete existing FTS entries for this collection
            using (var delCmd = conn.CreateCommand()) {
                delCmd.CommandText = "DELETE FROM semantic_fts WHERE collection=@col";
                delCmd.Parameters.AddWithValue("@col", collection);
                delCmd.ExecuteNonQuery();
            }
            // Re-insert with segmented content (and metadata for V2)
            using var selCmd = conn.CreateCommand();
            selCmd.CommandText = "SELECT id, content, metadata FROM semantic_records WHERE collection=@col";
            selCmd.Parameters.AddWithValue("@col", collection);
            using var reader = selCmd.ExecuteReader();
            var rows = new List<(string id, string content, string metadata)>();
            while (reader.Read()) {
                rows.Add((reader.GetString(0), reader.GetString(1), reader.IsDBNull(2) ? "" : reader.GetString(2)));
            }
            reader.Close();
            using var txn = conn.BeginTransaction();
            foreach (var (id, content, metadata) in rows) {
                using var insCmd = conn.CreateCommand();
                insCmd.Transaction = txn;
                if (_schemaVersion >= 2) {
                    insCmd.CommandText = "INSERT INTO semantic_fts(id,content,metadata,collection) VALUES(@id,@content,@meta,@col)";
                    insCmd.Parameters.AddWithValue("@meta", SegmentForFts(metadata));
                } else {
                    insCmd.CommandText = "INSERT INTO semantic_fts(id,content,collection) VALUES(@id,@content,@col)";
                }
                insCmd.Parameters.AddWithValue("@id", id);
                insCmd.Parameters.AddWithValue("@content", SegmentForFts(content));
                insCmd.Parameters.AddWithValue("@col", collection);
                insCmd.ExecuteNonQuery();
                count++;
            }
            txn.Commit();
            return count;
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

        // ---- sqlite_* direct LIKE-based search on semantic_records (no FTS, no scoring) ----

        /// <summary>
        /// Direct LIKE-based search on semantic_records table.
        /// Tokens are AND'd; token order has no semantic meaning.
        /// Result order: created_at DESC, take first topN.
        /// </summary>
        public List<SearchResultItem> SqliteSearchCore(string collection, string query, int topN)
        {
            return SqliteLikeSearchInternal(collection, query, topN, 0, 0);
        }

        /// <summary>
        /// Direct LIKE-based search on semantic_records within a time range.
        /// endTime=0 means now. Result order: created_at DESC, take first topN.
        /// </summary>
        public List<SearchResultItem> SqliteSearchBetweenCore(string collection, string query, int topN, long startTime, long endTime)
        {
            if (endTime <= 0) endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return SqliteLikeSearchInternal(collection, query, topN, startTime, endTime);
        }

        /// <summary>Tokenize query using same segmenter logic as BuildFtsQuery, cap at 64 tokens.</summary>
        private List<string> TokenizeForLike(string rawQuery)
        {
            if (string.IsNullOrWhiteSpace(rawQuery))
                return new List<string>();
            // Split by any Unicode whitespace (no segmenter, no punctuation splitting).
            var tokens = rawQuery.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            var list = tokens.Where(t => !string.IsNullOrEmpty(t)).Take(64).ToList();
            return list;
        }

        /// <summary>Escape LIKE special chars (\, %, _) using backslash as ESCAPE char, wrap with % % for contains match.</summary>
        private static string EscapeLikePattern(string token)
        {
            string escaped = token.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            return "%" + escaped + "%";
        }

        private List<SearchResultItem> SqliteLikeSearchInternal(string collection, string query, int topN, long startTime, long endTime)
        {
            var tokens = TokenizeForLike(query);
            if (tokens.Count == 0)
                return new List<SearchResultItem>();

            bool hasTimeFilter = startTime > 0 && endTime > 0;
            // Determine columns to match based on current _searchScope.
            // V1 schema has no metadata column in semantic_records? Actually metadata is in semantic_records always; _searchScope is an FTS concept.
            // For sqlite_* we honor _searchScope: content -> only content; metadata -> only metadata; all -> (content OR metadata).
            string scope = _searchScope;
            if (scope != "content" && scope != "metadata")
                scope = "all";

            var sb = new StringBuilder();
            sb.Append("SELECT id, content, metadata, created_at FROM semantic_records WHERE collection=@col");
            if (hasTimeFilter) {
                sb.Append(" AND created_at BETWEEN @s AND @e");
            }
            for (int i = 0; i < tokens.Count; i++) {
                sb.Append(" AND (");
                if (scope == "content") {
                    sb.Append("content LIKE @t").Append(i).Append(" ESCAPE '\\'");
                }
                else if (scope == "metadata") {
                    sb.Append("metadata LIKE @t").Append(i).Append(" ESCAPE '\\'");
                }
                else {
                    sb.Append("content LIKE @t").Append(i).Append(" ESCAPE '\\'");
                    sb.Append(" OR metadata LIKE @t").Append(i).Append(" ESCAPE '\\'");
                }
                sb.Append(")");
            }
            sb.Append(" ORDER BY created_at DESC LIMIT @n");

            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sb.ToString();
            cmd.Parameters.AddWithValue("@col", collection);
            if (hasTimeFilter) {
                cmd.Parameters.AddWithValue("@s", startTime);
                cmd.Parameters.AddWithValue("@e", endTime);
            }
            for (int i = 0; i < tokens.Count; i++) {
                cmd.Parameters.AddWithValue("@t" + i, EscapeLikePattern(tokens[i]));
            }
            cmd.Parameters.AddWithValue("@n", topN);

            var results = new List<SearchResultItem>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) {
                results.Add(new SearchResultItem {
                    Id = reader.GetString(0),
                    Content = reader.GetString(1),
                    Metadata = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Score = 0,
                    CreatedAt = reader.GetInt64(3)
                });
            }
            return results;
        }

        private List<SearchResultItem> KeywordSearchCoreInternal(string collection, string query, int topN, long startTime, long endTime)
        {
            string ftsQuery = BuildFtsQuery(query, useScope: true);
            if (string.IsNullOrEmpty(ftsQuery))
                return new List<SearchResultItem>();

            bool hasTimeFilter = startTime > 0 && endTime > 0;
            if (hasTimeFilter) {
                return KeywordSearchBetweenInternal(collection, ftsQuery, topN, startTime, endTime);
            }

            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT f.id, {GetBm25Expression(useScope: true)} AS score\nFROM semantic_fts f\nWHERE f.collection=@col AND semantic_fts MATCH @q\nORDER BY score\nLIMIT @n";
            cmd.Parameters.AddWithValue("@col", collection);
            cmd.Parameters.AddWithValue("@q", ftsQuery);
            cmd.Parameters.AddWithValue("@n", topN);

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

        // ---- Between-time search helpers ----

        /// <summary>
        /// Semantic+BM25 hybrid search within a time range.
        /// Strategy: fetch candidate id set by time range first, then do incremental HNSW+BM25 recall
        /// until the intersection with the candidate set exceeds topN, or the recall cap is reached
        /// (in which case fall back to table-scan on candidates).
        /// </summary>
        private List<SearchResultItem> SemanticSearchBetweenInternal(CollectionIndex col, string collection, float[] queryVector, string query, int topN, int finalRecallN, bool hasReranker, long startTime, long endTime)
        {
            // 1. Load candidate ids filtered by time range
            HashSet<string> candIds = LoadCandidateIdsByTime(collection, startTime, endTime);
            if (candIds.Count == 0)
                return new List<SearchResultItem>();

            string ftsQuery = BuildFtsQuery(query);

            // 2. Incremental recall loop: grow recallN until intersection exceeds topN, or cap is hit
            List<(string id, double hybridScore)>? orderedHits = null;
            int recallN = BetweenRecallStart;
            while (orderedHits == null) {
                if (recallN > BetweenRecallCap) {
                    orderedHits = ScanHybridOnCandidates(col, collection, candIds, queryVector, ftsQuery);
                    break;
                }
                var hybridOrdered = RecallHybridOnce(col, collection, queryVector, ftsQuery, recallN);
                if (hybridOrdered == null)
                    return new List<SearchResultItem>();
                int hit = 0;
                foreach (var h in hybridOrdered) {
                    if (candIds.Contains(h.id)) hit++;
                }
                if (hit > topN || hybridOrdered.Count < recallN) {
                    // Either enough hits, or the recall is already exhausted (no more results available)
                    orderedHits = hybridOrdered;
                    break;
                }
                recallN += BetweenRecallStep;
            }

            // 3. Intersect with candidate set, preserve hybrid score order, take finalRecallN
            var intersected = new List<(string id, double score)>();
            foreach (var h in orderedHits) {
                if (candIds.Contains(h.id)) {
                    intersected.Add(h);
                    if (intersected.Count >= finalRecallN)
                        break;
                }
            }
            if (intersected.Count == 0)
                return new List<SearchResultItem>();

            // 4. Load content for the intersected ids
            var idContentMap = LoadContentsByIds(intersected.Select(x => x.id));

            // 5. Rerank or use hybrid score directly
            IList<(string id, double score)> finalRanked;
            if (hasReranker) {
                var rerankCandidates = intersected
                    .Where(c => idContentMap.ContainsKey(c.id))
                    .Select(c => (c.id, idContentMap[c.id].content))
                    .ToList();
                var rerankResults = _rerankService!.Rerank(query, rerankCandidates, topN);
                finalRanked = rerankResults.Select(r => (r.key, (double)r.score)).ToList();
            } else {
                finalRanked = intersected
                    .Where(c => idContentMap.ContainsKey(c.id))
                    .Take(topN)
                    .Select(c => (c.id, c.score))
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

        /// <summary>
        /// Keyword-only (BM25) search within a time range using the same incremental strategy.
        /// </summary>
        private List<SearchResultItem> KeywordSearchBetweenInternal(string collection, string ftsQuery, int topN, long startTime, long endTime)
        {
            HashSet<string> candIds = LoadCandidateIdsByTime(collection, startTime, endTime);
            if (candIds.Count == 0)
                return new List<SearchResultItem>();

            List<(string id, double score)>? orderedHits = null;
            int recallN = BetweenRecallStart;
            while (orderedHits == null) {
                if (recallN > BetweenRecallCap) {
                    orderedHits = ScanBm25OnCandidates(collection, candIds, ftsQuery);
                    break;
                }
                var bm25Ordered = RecallBm25Once(collection, ftsQuery, recallN);
                int hit = 0;
                foreach (var h in bm25Ordered) {
                    if (candIds.Contains(h.id)) hit++;
                }
                if (hit > topN || bm25Ordered.Count < recallN) {
                    orderedHits = bm25Ordered;
                    break;
                }
                recallN += BetweenRecallStep;
            }

            var intersected = new List<(string id, double score)>();
            foreach (var h in orderedHits) {
                if (candIds.Contains(h.id)) {
                    intersected.Add(h);
                    if (intersected.Count >= topN)
                        break;
                }
            }
            if (intersected.Count == 0)
                return new List<SearchResultItem>();

            var idContentMap = LoadContentsByIds(intersected.Select(x => x.id));
            var results = new List<SearchResultItem>();
            foreach (var (id, score) in intersected) {
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

        /// <summary>Load all record ids in a collection whose created_at falls in [startTime, endTime].</summary>
        private HashSet<string> LoadCandidateIdsByTime(string collection, long startTime, long endTime)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id FROM semantic_records WHERE collection=@col AND created_at BETWEEN @s AND @e";
            cmd.Parameters.AddWithValue("@col", collection);
            cmd.Parameters.AddWithValue("@s", startTime);
            cmd.Parameters.AddWithValue("@e", endTime);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                set.Add(reader.GetString(0));
            return set;
        }

        /// <summary>Load content/metadata/created_at for the given ids.</summary>
        private Dictionary<string, (string content, string meta, long createdAt)> LoadContentsByIds(IEnumerable<string> ids)
        {
            var map = new Dictionary<string, (string content, string meta, long createdAt)>(StringComparer.Ordinal);
            using var conn = OpenConnection();
            foreach (var id in ids) {
                if (map.ContainsKey(id)) continue;
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT content,metadata,created_at FROM semantic_records WHERE id=@id";
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    map[id] = (reader.GetString(0), reader.IsDBNull(1) ? "" : reader.GetString(1), reader.GetInt64(2));
                }
            }
            return map;
        }

        /// <summary>
        /// Single HNSW+BM25 hybrid recall pass returning up to recallN ordered candidates.
        /// Returns null if the graph is not ready.
        /// </summary>
        private List<(string id, double hybridScore)>? RecallHybridOnce(CollectionIndex col, string collection, float[] queryVector, string ftsQuery, int recallN)
        {
            IEnumerable<VectorResult> hits;
            col.Lock.EnterReadLock();
            try {
                if (col.Graph == null)
                    return null;
                hits = col.Graph.GetTopKAsync(queryVector.ToList(), recallN).GetAwaiter().GetResult();
            }
            finally {
                col.Lock.ExitReadLock();
            }

            var candidateMap = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var h in hits) {
                string hid = h.GUID.ToString("N");
                candidateMap[hid] = 1.0 - (double)h.Distance;
            }

            var bm25Map = new Dictionary<string, double>(StringComparer.Ordinal);
            if (!string.IsNullOrEmpty(ftsQuery)) {
                using var conn = OpenConnection();
                if (candidateMap.Count > 0) {
                    var idList = string.Join(",", candidateMap.Keys.Select(id => $"'{id}'"));
                    using var ftsCmd = conn.CreateCommand();
                    ftsCmd.CommandText = $"SELECT id, {GetBm25Expression()} FROM semantic_fts WHERE collection=@col AND semantic_fts MATCH @q AND id IN ({idList})";
                    ftsCmd.Parameters.AddWithValue("@col", collection);
                    ftsCmd.Parameters.AddWithValue("@q", ftsQuery);
                    using var ftsReader = ftsCmd.ExecuteReader();
                    while (ftsReader.Read()) {
                        string fid = ftsReader.GetString(0);
                        double raw = ftsReader.GetDouble(1);
                        bm25Map[fid] = 1.0 / (1.0 + Math.Abs(raw));
                    }
                }
                using var bm25RecallCmd = conn.CreateCommand();
                bm25RecallCmd.CommandText = $"SELECT id, {GetBm25Expression()} AS score\nFROM semantic_fts\nWHERE collection=@col AND semantic_fts MATCH @q\nORDER BY score\nLIMIT @n";
                bm25RecallCmd.Parameters.AddWithValue("@col", collection);
                bm25RecallCmd.Parameters.AddWithValue("@q", ftsQuery);
                bm25RecallCmd.Parameters.AddWithValue("@n", recallN);
                using var bm25Reader = bm25RecallCmd.ExecuteReader();
                while (bm25Reader.Read()) {
                    string bid = bm25Reader.GetString(0);
                    double raw = bm25Reader.GetDouble(1);
                    double normalized = 1.0 / (1.0 + Math.Abs(raw));
                    if (!bm25Map.ContainsKey(bid))
                        bm25Map[bid] = normalized;
                    if (!candidateMap.ContainsKey(bid))
                        candidateMap[bid] = 0.0;
                }
            }

            return candidateMap
                .Select(c => (c.Key, _vectorWeight * c.Value + _bm25Weight * (bm25Map.TryGetValue(c.Key, out var b) ? b : 0.0)))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }

        /// <summary>Single BM25 recall pass returning up to recallN ordered candidates.</summary>
        private List<(string id, double score)> RecallBm25Once(string collection, string ftsQuery, int recallN)
        {
            var result = new List<(string id, double score)>();
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT f.id, {GetBm25Expression(useScope: true)} AS score\nFROM semantic_fts f\nWHERE f.collection=@col AND semantic_fts MATCH @q\nORDER BY score\nLIMIT @n";
            cmd.Parameters.AddWithValue("@col", collection);
            cmd.Parameters.AddWithValue("@q", ftsQuery);
            cmd.Parameters.AddWithValue("@n", recallN);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) {
                string id = reader.GetString(0);
                double raw = reader.GetDouble(1);
                double normalized = 1.0 / (1.0 + Math.Abs(raw));
                result.Add((id, normalized));
            }
            return result;
        }

        /// <summary>
        /// Fallback: table-scan hybrid scoring over candidate ids.
        /// Used when incremental recall fails to collect enough hits within the cap.
        /// </summary>
        private List<(string id, double hybridScore)> ScanHybridOnCandidates(CollectionIndex col, string collection, HashSet<string> candIds, float[] queryVector, string ftsQuery)
        {
            using var conn = OpenConnection();

            // 1. Populate a connection-scoped temp table with candidate ids
            using (var ddlCmd = conn.CreateCommand()) {
                ddlCmd.CommandText = "CREATE TEMP TABLE IF NOT EXISTS tmp_cand_ids(id TEXT PRIMARY KEY); DELETE FROM tmp_cand_ids;";
                ddlCmd.ExecuteNonQuery();
            }
            using (var txn = conn.BeginTransaction()) {
                using var insCmd = conn.CreateCommand();
                insCmd.Transaction = txn;
                insCmd.CommandText = "INSERT INTO tmp_cand_ids(id) VALUES(@id)";
                var p = insCmd.CreateParameter();
                p.ParameterName = "@id";
                insCmd.Parameters.Add(p);
                foreach (var id in candIds) {
                    p.Value = id;
                    insCmd.ExecuteNonQuery();
                }
                txn.Commit();
            }

            // 2. Vector similarity: read (id, vector) for candidate rows, compute cosine in parallel
            var records = new List<(string id, byte[] vec)>(candIds.Count);
            using (var vecCmd = conn.CreateCommand()) {
                vecCmd.CommandText = "SELECT r.id, r.vector FROM semantic_records r JOIN tmp_cand_ids t ON r.id = t.id WHERE r.collection=@col";
                vecCmd.Parameters.AddWithValue("@col", collection);
                using var reader = vecCmd.ExecuteReader();
                while (reader.Read()) {
                    records.Add((reader.GetString(0), (byte[])reader[1]));
                }
            }

            var vectorScores = new System.Collections.Concurrent.ConcurrentDictionary<string, double>(StringComparer.Ordinal);
            double qNorm = VectorNorm(queryVector);
            Parallel.ForEach(records, r => {
                var v = BytesToVector(r.vec);
                double sim = CosineSimilarity(queryVector, v, qNorm);
                vectorScores[r.id] = sim;
            });

            // 3. BM25 scan on candidates
            var bm25Map = new Dictionary<string, double>(StringComparer.Ordinal);
            if (!string.IsNullOrEmpty(ftsQuery)) {
                using var bm25Cmd = conn.CreateCommand();
                bm25Cmd.CommandText = $"SELECT f.id, {GetBm25Expression()} FROM semantic_fts f JOIN tmp_cand_ids t ON f.id = t.id WHERE f.collection=@col AND semantic_fts MATCH @q";
                bm25Cmd.Parameters.AddWithValue("@col", collection);
                bm25Cmd.Parameters.AddWithValue("@q", ftsQuery);
                using var reader = bm25Cmd.ExecuteReader();
                while (reader.Read()) {
                    string bid = reader.GetString(0);
                    double raw = reader.GetDouble(1);
                    bm25Map[bid] = 1.0 / (1.0 + Math.Abs(raw));
                }
            }

            // 4. Cleanup temp table
            using (var dropCmd = conn.CreateCommand()) {
                dropCmd.CommandText = "DROP TABLE IF EXISTS tmp_cand_ids";
                dropCmd.ExecuteNonQuery();
            }

            // 5. Merge and rank
            var allIds = new HashSet<string>(vectorScores.Keys, StringComparer.Ordinal);
            foreach (var k in bm25Map.Keys) allIds.Add(k);
            return allIds
                .Select(id => (id, _vectorWeight * (vectorScores.TryGetValue(id, out var v) ? v : 0.0)
                                 + _bm25Weight * (bm25Map.TryGetValue(id, out var b) ? b : 0.0)))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }

        /// <summary>Fallback: table-scan BM25 scoring over candidate ids.</summary>
        private List<(string id, double score)> ScanBm25OnCandidates(string collection, HashSet<string> candIds, string ftsQuery)
        {
            var result = new List<(string id, double score)>();
            if (string.IsNullOrEmpty(ftsQuery))
                return result;
            using var conn = OpenConnection();
            using (var ddlCmd = conn.CreateCommand()) {
                ddlCmd.CommandText = "CREATE TEMP TABLE IF NOT EXISTS tmp_cand_ids(id TEXT PRIMARY KEY); DELETE FROM tmp_cand_ids;";
                ddlCmd.ExecuteNonQuery();
            }
            using (var txn = conn.BeginTransaction()) {
                using var insCmd = conn.CreateCommand();
                insCmd.Transaction = txn;
                insCmd.CommandText = "INSERT INTO tmp_cand_ids(id) VALUES(@id)";
                var p = insCmd.CreateParameter();
                p.ParameterName = "@id";
                insCmd.Parameters.Add(p);
                foreach (var id in candIds) {
                    p.Value = id;
                    insCmd.ExecuteNonQuery();
                }
                txn.Commit();
            }
            using (var bm25Cmd = conn.CreateCommand()) {
                bm25Cmd.CommandText = $"SELECT f.id, {GetBm25Expression(useScope: true)} AS score FROM semantic_fts f JOIN tmp_cand_ids t ON f.id = t.id WHERE f.collection=@col AND semantic_fts MATCH @q ORDER BY score";
                bm25Cmd.Parameters.AddWithValue("@col", collection);
                bm25Cmd.Parameters.AddWithValue("@q", ftsQuery);
                using var reader = bm25Cmd.ExecuteReader();
                while (reader.Read()) {
                    string id = reader.GetString(0);
                    double raw = reader.GetDouble(1);
                    double normalized = 1.0 / (1.0 + Math.Abs(raw));
                    result.Add((id, normalized));
                }
            }
            using (var dropCmd = conn.CreateCommand()) {
                dropCmd.CommandText = "DROP TABLE IF EXISTS tmp_cand_ids";
                dropCmd.ExecuteNonQuery();
            }
            return result;
        }

        /// <summary>Compute L2 norm of a vector.</summary>
        private static double VectorNorm(float[] v)
        {
            double sum = 0.0;
            for (int i = 0; i < v.Length; i++) sum += (double)v[i] * v[i];
            return Math.Sqrt(sum);
        }

        /// <summary>Cosine similarity in range [-1, 1]. Returns 0 if either vector is zero.</summary>
        private static double CosineSimilarity(float[] a, float[] b, double aNorm)
        {
            if (a.Length != b.Length) return 0.0;
            double dot = 0.0;
            double bSum = 0.0;
            for (int i = 0; i < a.Length; i++) {
                double x = a[i];
                double y = b[i];
                dot += x * y;
                bSum += y * y;
            }
            double bNorm = Math.Sqrt(bSum);
            if (aNorm <= 0.0 || bNorm <= 0.0) return 0.0;
            return dot / (aNorm * bNorm);
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
                    sb.Append($"{{\"content\":{EscapeJson(item.Content)},\"metadata\":{EscapeJson(item.Metadata)},\"score\":{item.Score:F4}}}");
                else
                    sb.Append($"{{\"content\":{EscapeJson(item.Content)},\"metadata\":{EscapeJson(item.Metadata)}}}");
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
            DateTime tmp;
            DateTime dt;
            if (timeStr.Length <= 8 && DateTime.TryParseExact(timeStr, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tmp))
                dt = tmp;
            else if (timeStr.Length <= 10 && DateTime.TryParseExact(timeStr, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tmp))
                dt = tmp;
            else if (timeStr.Length <= 15 && DateTime.TryParseExact(timeStr, "yyyyMMdd HHmmss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tmp))
                dt = tmp;
            else if (timeStr.Length <= 19 && DateTime.TryParseExact(timeStr, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tmp))
                dt = tmp;
            else if (DateTime.TryParseExact(timeStr, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tmp))
                dt = tmp;
            else if (DateTime.TryParse(timeStr, out tmp))
                dt = tmp;
            else
                dt = DateTime.Now;
            return new DateTimeOffset(dt, TimeSpan.Zero).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Migrate FTS schema to the latest version. Detects current version and applies
        /// incremental migrations. Returns a report string.
        /// </summary>
        public string MigrateFtsSchema()
        {
            int currentVer = DetectSchemaVersion();
            if (currentVer >= CurrentSchemaVersion)
                return $"Already at version {currentVer}, no migration needed.";
            var sb = new StringBuilder();
            sb.AppendLine($"Current FTS schema version: {currentVer}");
            // Backup database before migration
            try {
                string backupPath = BackupDatabase();
                sb.AppendLine($"Database backed up to: {backupPath}");
            }
            catch (Exception ex) {
                sb.AppendLine($"Warning: backup failed ({ex.Message}), proceeding with migration.");
            }
            if (currentVer < 2) {
                MigrateV1ToV2();
                sb.AppendLine("Migrated V1 -> V2: added metadata column to FTS5 table.");
            }
            // Future migrations go here: if (currentVer < 3) { MigrateV2ToV3(); ... }
            _schemaVersion = DetectSchemaVersion();
            sb.AppendLine($"Migration complete. Current version: {_schemaVersion}");
            return sb.ToString();
        }

        /// <summary>
        /// Backup the database using VACUUM INTO for consistency.
        /// Returns the backup file path. If backupPath is null, generates a timestamped path.
        /// </summary>
        public string BackupDatabase(string? backupPath = null)
        {
            if (string.IsNullOrWhiteSpace(backupPath))
                backupPath = _dbPath + $".bak_{DateTime.Now:yyyyMMdd_HHmmss}";
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "VACUUM INTO @path";
            cmd.Parameters.AddWithValue("@path", backupPath);
            cmd.ExecuteNonQuery();
            return backupPath;
        }

        /// <summary>
        /// Restore the database from a backup file. Replaces the current database,
        /// clears all in-memory HNSW indexes, and re-initializes schema detection.
        /// </summary>
        public string RestoreDatabase(string backupPath)
        {
            if (!File.Exists(backupPath))
                return $"[error] Backup file not found: {backupPath}";
            var sb = new StringBuilder();
            // 1. Reset all in-memory collection states
            lock (_collectionsLock) {
                foreach (var col in _collections.Values) {
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
                _collections.Clear();
            }
            sb.AppendLine("In-memory indexes cleared.");
            // 2. Replace database file (delete WAL/SHM first to avoid stale state)
            string walPath = _dbPath + "-wal";
            string shmPath = _dbPath + "-shm";
            try { if (File.Exists(walPath)) File.Delete(walPath); } catch { }
            try { if (File.Exists(shmPath)) File.Delete(shmPath); } catch { }
            File.Copy(backupPath, _dbPath, overwrite: true);
            sb.AppendLine($"Database restored from: {backupPath}");
            // 3. Re-detect schema version
            _schemaVersion = DetectSchemaVersion();
            sb.AppendLine($"Schema version: {_schemaVersion}");
            return sb.ToString();
        }

        /// <summary>
        /// Execute a non-query SQL statement (INSERT/UPDATE/DELETE/DDL).
        /// Returns the number of affected rows.
        /// </summary>
        public int ExecuteSql(string sql)
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Execute a query SQL statement (SELECT). Returns JSON array of row objects.
        /// Each row is a JSON object with column names as keys.
        /// </summary>
        public string QuerySql(string sql)
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            using var reader = cmd.ExecuteReader();
            var sb = new StringBuilder("[");
            bool firstRow = true;
            while (reader.Read()) {
                if (!firstRow) sb.Append(',');
                firstRow = false;
                sb.Append('{');
                bool firstCol = true;
                for (int i = 0; i < reader.FieldCount; i++) {
                    if (!firstCol) sb.Append(',');
                    firstCol = false;
                    string colName = reader.GetName(i);
                    sb.Append(EscapeJson(colName));
                    sb.Append(':');
                    if (reader.IsDBNull(i)) {
                        sb.Append("null");
                    } else {
                        var fieldType = reader.GetFieldType(i);
                        if (fieldType == typeof(long) || fieldType == typeof(int)) {
                            sb.Append(reader.GetValue(i).ToString());
                        } else if (fieldType == typeof(double) || fieldType == typeof(float)) {
                            sb.Append(Convert.ToDouble(reader.GetValue(i)).ToString("G"));
                        } else {
                            sb.Append(EscapeJson(reader.GetValue(i).ToString() ?? ""));
                        }
                    }
                }
                sb.Append('}');
            }
            sb.Append(']');
            return sb.ToString();
        }

        private void MigrateV1ToV2()
        {
            using var conn = OpenConnection();
            using var txn = conn.BeginTransaction();
            // Drop old FTS table
            using (var dropCmd = conn.CreateCommand()) {
                dropCmd.Transaction = txn;
                dropCmd.CommandText = "DROP TABLE IF EXISTS semantic_fts";
                dropCmd.ExecuteNonQuery();
            }
            // Create new FTS table with metadata column
            using (var createCmd = conn.CreateCommand()) {
                createCmd.Transaction = txn;
                createCmd.CommandText = "CREATE VIRTUAL TABLE semantic_fts USING fts5(id UNINDEXED, content, metadata, collection UNINDEXED)";
                createCmd.ExecuteNonQuery();
            }
            // Re-populate from semantic_records (all collections)
            using var selCmd = conn.CreateCommand();
            selCmd.Transaction = txn;
            selCmd.CommandText = "SELECT id, collection, content, metadata FROM semantic_records";
            using var reader = selCmd.ExecuteReader();
            var rows = new List<(string id, string collection, string content, string metadata)>();
            while (reader.Read()) {
                rows.Add((
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.IsDBNull(3) ? "" : reader.GetString(3)
                ));
            }
            reader.Close();
            foreach (var (id, collection, content, metadata) in rows) {
                using var insCmd = conn.CreateCommand();
                insCmd.Transaction = txn;
                insCmd.CommandText = "INSERT INTO semantic_fts(id,content,metadata,collection) VALUES(@id,@content,@meta,@col)";
                insCmd.Parameters.AddWithValue("@id", id);
                insCmd.Parameters.AddWithValue("@content", SegmentForFts(content));
                insCmd.Parameters.AddWithValue("@meta", SegmentForFts(metadata));
                insCmd.Parameters.AddWithValue("@col", collection);
                insCmd.ExecuteNonQuery();
            }
            // Create version table if not exists and update version
            using (var verTableCmd = conn.CreateCommand()) {
                verTableCmd.Transaction = txn;
                verTableCmd.CommandText = "CREATE TABLE IF NOT EXISTS semantic_schema_version (id INTEGER PRIMARY KEY, version INTEGER NOT NULL)";
                verTableCmd.ExecuteNonQuery();
            }
            using (var verCmd = conn.CreateCommand()) {
                verCmd.Transaction = txn;
                verCmd.CommandText = "INSERT OR REPLACE INTO semantic_schema_version(id, version) VALUES(1, 2)";
                verCmd.ExecuteNonQuery();
            }
            txn.Commit();
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
