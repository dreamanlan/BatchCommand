using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
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
        private FileOperations _fileOps;
        private DiffOperations _diffOps;
        private ClipboardOperations _clipboardOps;
        private LoggingAndDebugging _logger;
        private HttpClientOperations _httpClient;
        private ProcessOperations _processOps;
        private ContextManagement _contextManager;
        private BrowserInteraction _browserOps;
        private AgentBridge _agentBridge;
        private DotNetLib.NativeApi _nativeApi;

        // Public properties - return concrete types for full access
        public FileOperations FileOps => _fileOps;
        public DiffOperations DiffOps => _diffOps;
        public ClipboardOperations ClipboardOps => _clipboardOps;
        public LoggingAndDebugging Logger => _logger;
        public HttpClientOperations HttpClient => _httpClient;
        public ProcessOperations ProcessOps => _processOps;
        public ContextManagement ContextManager => _contextManager;
        public BrowserInteraction BrowserOps => _browserOps;
        public AgentBridge AgentBridge => _agentBridge;

        public static bool IsInitialized => _isInitialized;
        public static AgentCore Instance
        {
            get {
                if (!_isInitialized)
                    throw new InvalidOperationException("AgentCore not initialized. Call Initialize first.");
                return _instance!;
            }
        }

        private AgentCore(string basePath, string appDir, bool isMac)
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
            _contextManager = new ContextManagement();
            _browserOps = new BrowserInteraction(null, null);
            _agentBridge = new AgentBridge(null, msg => _logger.Info(msg));
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
                    NativeLibraryLoader.Initialize();
                }
                catch (Exception ex) {
                    // At this point, Logger instance is not yet available, use Console as fallback
                    System.Console.WriteLine($"[AgentCore] Warning: Failed to initialize NativeLibraryLoader: {ex.Message}");
                }

                _instance = new AgentCore(basePath, appDir, isMac);
                _isInitialized = true;
            }
        }

        public void SetNativeApi(DotNetLib.NativeApi nativeApi)
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

            _browserOps.SetExecuteJsAction(executeJsAction);
            _browserOps.SetCallJsAction(callJsAction);

            // Set the ExecuteJsAction callback in AgentBridge to send commands to inject.js
            _agentBridge.SetExecuteJsAction(callJsAction);

            // Flush cached logs from NativeLibraryLoader
            try {
                NativeLibraryLoader.FlushLogsToLogger();
            }
            catch (Exception ex) {
                _logger.Warning($"[AgentCore] Warning: Failed to flush NativeLibraryLoader logs: {ex.Message}");
            }
            _logger.Info("NativeApi set successfully");
        }

        public DotNetLib.NativeApi GetNativeApi()
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
            // Cleanup resources if needed
            _isInitialized = false;
        }
    }
}
