using System;
using System.IO;
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

        // Operation instances
        private FileOperations _fileOps;
        private DiffOperations _diffOps;
        private ClipboardOperations _clipboardOps;
        private LoggingAndDebugging _logger;
        private HttpClientOperations _httpClient;
        private TaskManagement _taskManager;
        private LLMInteraction _llmManager;
        private ProcessOperations _processOps;
        private ContextManagement _contextManager;
        private CodeAnalysis _codeAnalyzer;
        private TemplateEngine _templateEngine;
        private BrowserInteraction _browserOps;
        private AgentMessageHandler _messageHandler;
        private DotNetLib.NativeApi _nativeApi;

        // Public properties - return concrete types for full access
        public FileOperations FileOps => _fileOps;
        public DiffOperations DiffOps => _diffOps;
        public ClipboardOperations ClipboardOps => _clipboardOps;
        public LoggingAndDebugging Logger => _logger;
        public HttpClientOperations HttpClient => _httpClient;
        public TaskManagement TaskManager => _taskManager;
        public LLMInteraction LLMManager => _llmManager;
        public ProcessOperations ProcessOps => _processOps;
        public ContextManagement ContextManager => _contextManager;
        public CodeAnalysis CodeAnalyzer => _codeAnalyzer;
        public TemplateEngine TemplateEngine => _templateEngine;
        public BrowserInteraction BrowserOps => _browserOps;
        public AgentMessageHandler MessageHandler => _messageHandler;

        public static bool IsInitialized => _isInitialized;
        public static AgentCore Instance
        {
            get
            {
                if (!_isInitialized)
                    throw new InvalidOperationException("AgentCore not initialized. Call Initialize first.");
                return _instance!;
            }
        }

        private AgentCore(string basePath)
        {
            _basePath = basePath ?? Directory.GetCurrentDirectory();

            // Initialize all operation instances
            _logger = new LoggingAndDebugging();
            _fileOps = new FileOperations(basePath);
            _diffOps = new DiffOperations(basePath);
            _clipboardOps = new ClipboardOperations();
            _httpClient = new HttpClientOperations();
            _taskManager = new TaskManagement();
            _llmManager = new LLMInteraction();
            _processOps = new ProcessOperations();
            _contextManager = new ContextManagement();
            _codeAnalyzer = new CodeAnalysis(basePath);
            _templateEngine = new TemplateEngine(basePath);
            _browserOps = new BrowserInteraction(null, null);
            _messageHandler = new AgentMessageHandler(null, msg => _logger.Info(msg));
        }

        public static void Initialize(string basePath)
        {
            if (_isInitialized)
                return;

            lock (_lock)
            {
                if (_isInitialized)
                    return;

                _instance = new AgentCore(basePath);
                _isInitialized = true;
            }
        }

        public void SetNativeApi(DotNetLib.NativeApi nativeApi)
        {
            if (nativeApi == null)
            {
                _logger.Warning("nativeApi is null");
                return;
            }

            // Save the nativeApi reference
            _nativeApi = nativeApi;

            // Set native log action in LoggingAndDebugging
            Action<string> nativeLogAction = msg =>
            {
                try
                {
                    nativeApi.NativeLog($"[Native] {msg}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"[NativeLogError] {ex.Message}");
                }
            };
            _logger.SetNativeLogAction(nativeLogAction);

            // Set JavaScript execution actions in BrowserInteraction
            Action<string> executeJsAction = script =>
            {
                try
                {
                    nativeApi.ExecuteJavascript(script);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error executing JavaScript: {ex.Message}");
                }
            };

            Action<string, string> callJsAction = (funcName, arg) =>
            {
                try
                {
                    nativeApi.CallJavascript1(funcName, arg);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error calling JavaScript function: {ex.Message}");
                }
            };

            _browserOps.SetExecuteJsAction(executeJsAction);
            _browserOps.SetCallJsAction(callJsAction);

            // Set the SendToInject callback in AgentMessageHandler to send responses back to inject.js
            Action<string> sendToInject = json =>
            {
                try
                {
                    nativeApi.CallJavascript1("window.onAgentResponse", json);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error sending response to JavaScript: {ex.Message}");
                }
            };
            _messageHandler.SetSendToInjectCallback(sendToInject);

            _logger.Info("NativeApi set successfully");
        }

        public DotNetLib.NativeApi GetNativeApi()
        {
            return _nativeApi;
        }

        /// <summary>
        /// Trigger hot reload for a specific component
        /// This is called from MetaDSL scripts via hot_reload() function
        /// </summary>
        public void TriggerHotReload(string component)
        {
            _logger.Info($"Triggering hot reload for component: {component}");

            switch (component.ToLower())
            {
                case "agentcore":
                    TriggerAgentCoreHotReload();
                    break;
                case "script":
                    _logger.Info("Script.dsl hot reload scheduled (auto-reloaded on next execution)");
                    break;
                case "inject":
                    _logger.Info("inject.js hot reload scheduled (requires page refresh)");
                    break;
                case "all":
                    TriggerAgentCoreHotReload();
                    _logger.Info("Script.dsl and inject.js hot reload scheduled");
                    break;
                default:
                    _logger.Warning($"Unknown component for hot reload: {component}");
                    break;
            }
        }

        private void TriggerAgentCoreHotReload()
        {
            _logger.Info("Triggering AgentCore.dll hot reload...");

            try
            {
                // Send hot_reload command to inject.js
                // inject.js will use its CONFIG.hotReload configuration to get the paths
                // and then call cefQuery to trigger the C++ hot reload handler
                _browserOps.SendCommandToInject("hot_reload", new
                {
                    component = "agentcore"
                });

                _logger.Info("Hot reload command sent to inject.js");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error triggering AgentCore hot reload: {ex.Message}");
            }
        }

        public void OnReceiveJsMessage(string command, string parameters)
        {
            // HandleMessage expects JSON format, so we need to construct it
            // For now, just pass the parameters directly if it's already JSON
            // Otherwise, we might need to construct a proper JSON message
            _messageHandler.HandleMessage(parameters);
        }

        public void Shutdown()
        {
            // Cleanup resources if needed
            _isInitialized = false;
        }
    }
}
