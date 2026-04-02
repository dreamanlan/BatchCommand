using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AgentPlugin.Abstractions;

namespace CefDotnetApp.AgentCore.Core
{
    public enum HelpSearchType
    {
        BagOfWords = 0,
        TfIdf = 1,
        Embedding = 2
    }

    /// <summary>
    /// Central singleton class that manages all agent operations
    /// </summary>
    public class AgentCore
    {
        private static AgentCore? _instance;
        private static readonly object _lock = new object();
        private static bool _isInitialized = false;
        private static string _basePath = string.Empty;
        private static string _appDir = string.Empty;
        private static bool _isMac = false;

        // Operation instances
        private FileOperations _fileOps = null!;
        private DiffOperations _diffOps = null!;
        private ClipboardOperations _clipboardOps = null!;
        private LoggingAndDebugging _logger = null!;
        private HttpClientOperations _httpClient = null!;
        private ProcessOperations _processOps = null!;
        private DslContextManagement _dslContextManager = null!;
        private BrowserInteraction _browserOps = null!;
        private AgentBridge _agentBridge = null!;
        private INativeApi _nativeApi = null!;
        private SkillManager _skillMgr = null!;
        private EmbeddingService _embeddingService = null!;
        private RerankService _rerankService = null!;
        private SemanticIndex _semanticIndex = null!;
        private BagOfWordsService _bagOfWordsService = null!;
        private TfIdfService _tfIdfService = null!;
        private MixedSegmenterEnhanced _mixedSegmenter = null!;
        private BraveSearchService _braveSearch = null!;
        private SearXNGSearchService _searxngSearch = null!;
        private WebSearchRouter _webSearchRouter = null!;
        private HelpSearchType _helpSearchMode = HelpSearchType.BagOfWords;
        private bool _helpUseReranker = true;
        // Three-level dictionary: category -> group -> key -> encrypted_value
        // Used for agent environment data (e.g. mcp tokens, skill config)
        // Values are AES-encrypted at rest; decrypted only during ResolveEnvironmentValue calls.
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, string>>> _agentEnvironments = new();
        // Lock for ResolveEnvironmentValue to prevent concurrent Apply/Clear on process-level env vars
        private readonly object _envResolveLock = new object();

        // Public properties - return concrete types for full access
        public FileOperations FileOps => _fileOps;
        public DiffOperations DiffOps => _diffOps;
        public ClipboardOperations ClipboardOps => _clipboardOps;
        public LoggingAndDebugging Logger => _logger;
        public HttpClientOperations HttpClient => _httpClient;
        public ProcessOperations ProcessOps => _processOps;
        public DslContextManagement DslContextManager => _dslContextManager;
        public BrowserInteraction BrowserOps => _browserOps;
        public AgentBridge AgentBridge => _agentBridge;
        public SkillManager SkillMgr => _skillMgr;
        public EmbeddingService EmbeddingService => _embeddingService;
        public RerankService RerankService => _rerankService;
        public SemanticIndex SemanticIndex => _semanticIndex;
        public BagOfWordsService BagOfWordsService => _bagOfWordsService;
        public TfIdfService TfIdfService => _tfIdfService;
        public MixedSegmenterEnhanced MixedSegmenter => _mixedSegmenter;
        public BraveSearchService BraveSearch => _braveSearch;
        public SearXNGSearchService SearXNGSearch => _searxngSearch;
        public WebSearchRouter WebSearchRouter => _webSearchRouter;
        public HelpSearchType HelpSearchMode
        {
            get => _helpSearchMode;
            set => _helpSearchMode = value;
        }
        public bool HelpUseReranker
        {
            get => _helpUseReranker;
            set => _helpUseReranker = value;
        }
        public string BasePath => _basePath;

        // Agent state properties - readable/writable by DSL scripts
        public string ProjectDir { get; set; } = string.Empty;
        public string ProjectIdentity { get; set; } = string.Empty;
        public string SystemPrompt { get; set; } = string.Empty;
        public string ProjectPrompt { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string Emphasize { get; set; } = string.Empty;
        public string ToDo { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public string History { get; set; } = string.Empty;
        public string InjectJsCode { get; set; } = string.Empty;
        public string Soul { get; set; } = string.Empty;

        public int MaxLinesDeletedByWriteFile { get; set; } = 20;
        public int MaxResultSize { get; set; } = 0;
        // How often to append context info in WebSocket responses (0 = every round)
        public int MaxContextRounds { get; set; } = 3;
        // Current context round counter (atomic via Interlocked)
        private int _curContextRounds = 0;
        public int CurContextRounds
        {
            get => System.Threading.Interlocked.CompareExchange(ref _curContextRounds, 0, 0);
            set => System.Threading.Interlocked.Exchange(ref _curContextRounds, value);
        }
        /// <summary>
        /// Atomically increments CurContextRounds by 1, modulo MaxContextRounds.
        /// When MaxContextRounds is 0, always returns 0.
        /// Returns the new value after the operation.
        /// </summary>
        public int AddCurContextRounds()
        {
            int max = MaxContextRounds;
            if (max <= 0)
                return 0;
            int oldVal, newVal;
            do {
                oldVal = _curContextRounds;
                newVal = (oldVal + 1) % max;
            } while (System.Threading.Interlocked.CompareExchange(ref _curContextRounds, newVal, oldVal) != oldVal);
            return newVal;
        }

        /// <summary>
        /// Sets a value in the three-level agent environment dictionary.
        /// Value is AES-encrypted before storage.
        /// Used for storing secrets/config (e.g. mcp tokens, skill settings).
        /// </summary>
        public void SetAgentEnvironment(string category, string group, string key, string value)
        {
            var groups = _agentEnvironments.GetOrAdd(category, _ => new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>());
            var kvs = groups.GetOrAdd(group, _ => new ConcurrentDictionary<string, string>());
            kvs[key] = AesCrypto.Encrypt(value);
        }

        /// <summary>
        /// Gets the key-value dictionary for a specific category+group.
        /// Returns null if category or group does not exist.
        /// </summary>
        public ConcurrentDictionary<string, string>? GetAgentEnvironmentGroup(string category, string group)
        {
            if (_agentEnvironments.TryGetValue(category, out var groups))
            {
                if (groups.TryGetValue(group, out var kvs))
                    return kvs;
            }
            return null;
        }

        /// <summary>
        /// Gets the length of the encrypted string for a specific environment key.
        /// Returns -1 if the key does not exist.
        /// </summary>
        public int GetAgentEnvironmentLength(string category, string group, string key)
        {
            var kvs = GetAgentEnvironmentGroup(category, group);
            if (kvs == null) return -1;
            if (kvs.TryGetValue(key, out var encrypted))
                return encrypted.Length;
            return -1;
        }

        /// <summary>
        /// Sets process-level environment variables from the specified category+group.
        /// Values are decrypted before being set as env vars.
        /// Call ClearAgentEnvironment after use to remove them.
        /// NOTE: Callers should prefer ResolveEnvironmentValue/ResolveEnvironmentValues
        /// which handle Apply+Expand+Clear atomically with proper locking.
        /// </summary>
        public void ApplyAgentEnvironment(string category, string group)
        {
            var kvs = GetAgentEnvironmentGroup(category, group);
            if (kvs == null) return;
            foreach (var kvp in kvs)
            {
                Environment.SetEnvironmentVariable(kvp.Key, AesCrypto.Decrypt(kvp.Value), EnvironmentVariableTarget.Process);
            }
        }

        /// <summary>
        /// Clears process-level environment variables that were set by ApplyAgentEnvironment.
        /// </summary>
        public void ClearAgentEnvironment(string category, string group)
        {
            var kvs = GetAgentEnvironmentGroup(category, group);
            if (kvs == null) return;
            foreach (var kvp in kvs)
            {
                Environment.SetEnvironmentVariable(kvp.Key, null, EnvironmentVariableTarget.Process);
            }
        }

        /// <summary>
        /// Resolves a single template string by decrypting env vars, expanding %var% placeholders,
        /// and immediately clearing the process env vars.
        /// Thread-safe: uses lock to prevent concurrent Apply/Clear conflicts.
        /// </summary>
        public string ResolveEnvironmentValue(string category, string group, string template)
        {
            if (string.IsNullOrEmpty(template) || !template.Contains('%'))
                return template;
            lock (_envResolveLock)
            {
                ApplyAgentEnvironment(category, group);
                try
                {
                    return Environment.ExpandEnvironmentVariables(template);
                }
                finally
                {
                    ClearAgentEnvironment(category, group);
                }
            }
        }

        /// <summary>
        /// Resolves multiple template strings in one Apply/Clear cycle.
        /// More efficient than calling ResolveEnvironmentValue for each template.
        /// Thread-safe: uses lock to prevent concurrent Apply/Clear conflicts.
        /// </summary>
        public string[] ResolveEnvironmentValues(string category, string group, params string[] templates)
        {
            if (templates == null || templates.Length == 0)
                return Array.Empty<string>();
            lock (_envResolveLock)
            {
                ApplyAgentEnvironment(category, group);
                try
                {
                    var results = new string[templates.Length];
                    for (int i = 0; i < templates.Length; i++)
                    {
                        results[i] = (templates[i] != null && templates[i].Contains('%'))
                            ? Environment.ExpandEnvironmentVariables(templates[i])
                            : templates[i] ?? string.Empty;
                    }
                    return results;
                }
                finally
                {
                    ClearAgentEnvironment(category, group);
                }
            }
        }

        public static bool IsInitialized => _isInitialized;
        public static AgentCore Instance
        {
            get {
                if (!_isInitialized)
                    throw new InvalidOperationException("AgentCore not initialized. Call Initialize first.");
                return _instance!;
            }
        }

        private AgentCore(string? basePath, string? appDir, bool isMac)
        {
            _basePath = basePath ?? Directory.GetCurrentDirectory();
            _appDir = appDir ?? Directory.GetCurrentDirectory();
            _isMac = isMac;

            // Initialize all operation instances
            _logger = new LoggingAndDebugging();
            _fileOps = new FileOperations(_basePath, _appDir, isMac);
            _diffOps = new DiffOperations(_basePath, _appDir, isMac);
            _clipboardOps = new ClipboardOperations();
            _httpClient = new HttpClientOperations();
            _processOps = new ProcessOperations();
            _dslContextManager = new DslContextManagement();
            _browserOps = new BrowserInteraction(null, null);
            _agentBridge = new AgentBridge(null, msg => _logger.Info(msg));
            _skillMgr = new SkillManager(_processOps, _basePath, _appDir, isMac);
            _embeddingService = new EmbeddingService();
            _rerankService = new RerankService();
            // Initialize mixed segmenter with empty freq lines; jieba handles Chinese, internal trie handles Latin
            string freqPath = System.IO.Path.Combine(_basePath, "onnx", "help_freq.txt");
            _mixedSegmenter = new MixedSegmenterEnhanced(new WordSegmenterHybridTrie(freqPath));
            _bagOfWordsService = new BagOfWordsService(_mixedSegmenter);
            _tfIdfService = new TfIdfService();
            string dbPath = System.IO.Path.Combine(_basePath, "onnx", "semantic.db");
            _semanticIndex = new SemanticIndex(dbPath);
            _semanticIndex.SetSegmenter(_mixedSegmenter);
            _braveSearch = new BraveSearchService(_httpClient);
            _searxngSearch = new SearXNGSearchService(_httpClient);
            _webSearchRouter = new WebSearchRouter(_braveSearch, _searxngSearch);
        }

        public static void Initialize(string basePath, string appDir, bool isMac)
        {
            if (_isInitialized)
                return;

            lock (_lock) {
                if (_isInitialized)
                    return;

                // Initialize native library loader for TreeSitter
                try {
                    NativeLibraryLoader.Initialize(basePath);
                }
                catch (Exception ex) {
                    // At this point, Logger instance is not yet available, use Console as fallback
                    System.Console.WriteLine($"[AgentCore] Warning: Failed to initialize NativeLibraryLoader: {ex.Message}");
                }

                // Initialize AES crypto key for in-memory secret encryption
                AesCrypto.Initialize();

                _instance = new AgentCore(basePath, appDir, isMac);
                _isInitialized = true;

                JiebaNet.Segmenter.ConfigManager.ConfigFileBaseDir = Path.Combine(basePath, "managed", "Resources");

                // try to load embedding model if available
                try {
                    _instance.RefreshEmbedding();
                }
                catch (Exception ex) {
                    System.Console.WriteLine($"[AgentCore] Warning: Failed to load embedding model: {ex.Message}");
                }

                // try to load reranker model if available
                try {
                    _instance.RefreshReranker();
                }
                catch (Exception ex) {
                    System.Console.WriteLine($"[AgentCore] Warning: Failed to load reranker model: {ex.Message}");
                }

                if (null != _instance.EmbeddingService && _instance.EmbeddingService.IsReady) {
                    _instance.SemanticIndex.InitCollection("metadsl_history");
                }
            }
        }

        public string RefreshSkills(string skillsDir)
        {
            _skillMgr.RefreshSkills(skillsDir);
            RebuildEmbeddingIndex();
            _logger.Info($"Skills refreshed from {skillsDir}");
            return "ok";
        }

        public string RefreshEmbedding()
        {
            try {
                //string modelPath = System.IO.Path.Combine(_basePath, "onnx", "nomic_embed_text_v1_5.onnx");
                //string tokenizerPath = System.IO.Path.Combine(_basePath, "onnx", "nomic_embed_text_v1_5_vocab.txt");
                string modelPath = System.IO.Path.Combine(_basePath, "onnx", "paraphrase_multilingual_MiniLM_L12_v2.onnx");
                string tokenizerPath = System.IO.Path.Combine(_basePath, "onnx", "paraphrase_multilingual_MiniLM_L12_v2_sentencepiece.bpe.model");
                if (!System.IO.File.Exists(modelPath) || !System.IO.File.Exists(tokenizerPath)) {
                    _logger.Warning($"[EmbeddingService] model or tokenizer not found in {_basePath}/onnx/");
                    return "[error] model or tokenizer not found";
                }
                _embeddingService.Load(modelPath, tokenizerPath);
                RebuildEmbeddingIndex();
                _logger.Info("[EmbeddingService] reloaded");
                return "ok";
            }
            catch (Exception ex) {
                _logger.Error($"[EmbeddingService] RefreshEmbedding failed: {ex.Message}");
                return $"[error] {ex.Message}";
            }
        }

        private void RebuildEmbeddingIndex()
        {
            var items = _skillMgr.Skills.Values
                .Select(s => (s.Key, s.Document ?? string.Empty))
                .ToList();
            if (_embeddingService.IsReady) {
                _embeddingService.BuildIndex(items);
            }
            if (_bagOfWordsService.IsReady) {
                _bagOfWordsService.BuildIndex(items);
            }
            if (_tfIdfService.IsReady) {
                _tfIdfService.BuildIndex(items);
            }
        }

        public IList<(string key, string text, float score)>? SemanticSearch(
            IList<string> queries,
            IEnumerable<(string key, string text)> candidates,
            int topN)
        {
            bool useReranker = _helpUseReranker && _rerankService.IsReady;
            int recallN = useReranker ? topN * 3 : topN;
            var candidateList = candidates as IList<(string key, string text)> ?? candidates.ToList();
            IList<(string key, string text, float score)>? results = null;
            if (_helpSearchMode == HelpSearchType.TfIdf) {
                if (_tfIdfService.IsReady) {
                    results = _tfIdfService.Search(queries, candidateList, recallN);
                }
            }
            else if (_helpSearchMode == HelpSearchType.Embedding) {
                if (_embeddingService.IsReady) {
                    results = _embeddingService.Search(queries, candidateList, recallN);
                }
                if (results == null && _bagOfWordsService.IsReady) {
                    results = _bagOfWordsService.Search(queries, candidateList, recallN);
                }
            }
            else if (_bagOfWordsService.IsReady) {
                results = _bagOfWordsService.Search(queries, candidateList, recallN);
            }
            if (results == null || results.Count == 0 || !useReranker || queries.Count == 0) {
                return results;
            }
            var rerankCandidates = new List<(string key, string text)>(results.Count);
            foreach (var (key, text, _) in results) {
                rerankCandidates.Add((key, text));
            }
            return _rerankService.Rerank(queries[0], rerankCandidates, topN);
        }

        public void BuildSkillDocs()
        {
            _skillMgr.BuildSkillDocs();
            RebuildEmbeddingIndex();
        }

        public string GetSkillHelp(IList<System.Text.RegularExpressions.Regex> keyRegexes)
        {
            RerankService? reranker = _helpUseReranker ? _rerankService : null;
            if (_helpSearchMode == HelpSearchType.TfIdf) {
                return _skillMgr.GetSkillHelp(keyRegexes, null, reranker, null, _tfIdfService);
            }
            if (_helpSearchMode == HelpSearchType.Embedding) {
                return _skillMgr.GetSkillHelp(keyRegexes, _embeddingService, reranker, _bagOfWordsService);
            }
            return _skillMgr.GetSkillHelp(keyRegexes, null, reranker, _bagOfWordsService);
        }

        public string RefreshReranker()
        {
            try {
                string modelPath = System.IO.Path.Combine(_basePath, "onnx", "bge_reranker_base.onnx");
                string tokenizerPath = System.IO.Path.Combine(_basePath, "onnx", "bge_reranker_base_sentencepiece.bpe.model");
                if (!System.IO.File.Exists(modelPath) || !System.IO.File.Exists(tokenizerPath)) {
                    _logger.Warning($"[RerankService] model or tokenizer not found in {_basePath}/onnx/");
                    return "[error] model or tokenizer not found";
                }
                _rerankService.Load(modelPath, tokenizerPath);
                _semanticIndex.SetRerankService(_rerankService);
                _logger.Info("[RerankService] reloaded");
                return "ok";
            }
            catch (Exception ex) {
                _logger.Error($"[RerankService] RefreshReranker failed: {ex.Message}");
                return $"[error] {ex.Message}";
            }
        }

        public void SetNativeApi(INativeApi nativeApi)
        {
            if (nativeApi == null) {
                _logger.Warning("nativeApi is null");
                return;
            }

            // Save the nativeApi reference
            _nativeApi = nativeApi;

            // Set native log action in LoggingAndDebugging
            Action<string> nativeLogAction = msg => {
                try {
                    nativeApi.NativeLog($"[Native] {msg}");
                }
                catch (Exception ex) {
                    _logger.Error($"[NativeLogError] {ex.Message}");
                }
            };
            _logger.SetNativeLogAction(nativeLogAction);

            // Set JavaScript execution actions in BrowserInteraction
            Action<string> executeJsAction = script => {
                try {
                    nativeApi.SendJavascriptCode(script);
                }
                catch (Exception ex) {
                    _logger.Error($"Error executing JavaScript: {ex.Message}");
                }
            };

            Action<string, string[]> callJsAction = (funcName, args) => {
                try {
                    nativeApi.SendJavascriptCall(funcName, args);
                }
                catch (Exception ex) {
                    _logger.Error($"Error calling JavaScript function: {ex.Message}");
                }
            };

            _browserOps.SetSendJsCodeAction(executeJsAction);
            _browserOps.SetSendJsCallAction(callJsAction);

            // Set the SendJsCall callback in AgentBridge to send commands to inject.js
            _agentBridge.SetSendJsCallAction(callJsAction);

            // Flush cached logs from NativeLibraryLoader
            try {
                NativeLibraryLoader.FlushLogsToLogger();
            }
            catch (Exception ex) {
                _logger.Warning($"[AgentCore] Warning: Failed to flush NativeLibraryLoader logs: {ex.Message}");
            }
            _logger.Info("NativeApi set successfully");
        }

        public INativeApi GetNativeApi()
        {
            return _nativeApi;
        }

        public void TriggerHotReload()
        {
            _logger.Info("Triggering AgentCore.dll hot reload...");

            try {
                _agentBridge.SendCommandToInject("hot_reload", new Dictionary<string, object>
                {
                    { "component", "agentcore" }
                });

                _logger.Info("Hot reload command sent to inject.js");
            }
            catch (Exception ex) {
                _logger.Error($"Error triggering AgentCore hot reload: {ex.Message}");
            }
        }

        public void Shutdown()
        {
            // Dispose ONNX sessions, SQLite connections and other native resources
            try { _embeddingService?.Dispose(); } catch { }
            try { _rerankService?.Dispose(); } catch { }
            try { _semanticIndex?.Dispose(); } catch { }
            try { _bagOfWordsService?.Dispose(); } catch { }
            _isInitialized = false;
        }
    }
}
