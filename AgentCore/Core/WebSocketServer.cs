using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ScriptableFramework;
using BatchCommand;
using BatchCommand.Utils;

namespace AgentCore.Core
{
    /// <summary>
    /// WebSocket server with dual queue message management
    /// </summary>
    public class WebSocketServer : IDisposable
    {
        private HttpListener _listener = null!;
        private ConcurrentQueue<(string message, WebSocket client)> _receiveQueue;
        private List<WebSocket> _clients;
        private CancellationTokenSource _cancellationTokenSource;
        private Thread? _tickThread;
        private readonly object _lockObj = new object();
        private bool _isRunning;
        private int _port;
        private DateTime _lastPingTime = DateTime.MinValue;
        private static readonly TimeSpan s_pingInterval = TimeSpan.FromSeconds(30);
        // Per-connection context round counter
        private readonly ConcurrentDictionary<WebSocket, int> _contextRounds = new();
        // Worker concurrency control
        private int _maxWorkerConcurrency = 16;
        private int _activeWorkers = 0;
        // Worker start time tracking for stuck detection
        private readonly ConcurrentDictionary<long, DateTime> _workerStartTimes = new();
        private long _workerIdCounter = 0;
        private DateTime _lastWorkerCheckTime = DateTime.UtcNow;
        private int _workerTimeoutSeconds = 600;
        private const int c_workerCheckIntervalSeconds = 30;
        private DateTime _lastWorkerStatusLogTime = DateTime.UtcNow;
        private const int c_workerStatusLogIntervalSeconds = 2;

        /// <summary>
        /// Gets whether the server is currently running
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Gets the port the server is listening on
        /// </summary>
        public int Port => _port;

        /// <summary>
        /// Gets or sets the maximum number of concurrent MetaDSL worker tasks
        /// </summary>
        public int MaxWorkerConcurrency
        {
            get => _maxWorkerConcurrency;
            set => _maxWorkerConcurrency = Math.Max(1, value);
        }

        /// <summary>
        /// Gets the number of active MetaDSL worker tasks
        /// </summary>
        public int ActiveWorkers => _activeWorkers;

        /// <summary>
        /// Agent id this server serves ("webagent", "hyarena", ...). Set by
        /// ws_start_server(port, agentId); empty = legacy port-keyed server.
        /// </summary>
        public string AgentId { get; set; } = string.Empty;

        // Per-connection agent id bindings (agent_bind envelope): raw MetaDSL
        // code on these connections resolves its AgentInstance by the bound id
        // instead of the server default. Used by the single-page adapters'
        // execution slots once everything is multiplexed on the relay port.
        private readonly System.Collections.Concurrent.ConcurrentDictionary<WebSocket, string> _clientAgentIds = new();

        /// <summary>
        /// Event fired when a client connects
        /// </summary>
        public event Action? OnClientConnected;

        /// <summary>
        /// Event fired when a client disconnects
        /// </summary>
        public event Action? OnClientDisconnected;

        /// <summary>
        /// Creates a new WebSocket server instance
        /// </summary>
        public WebSocketServer()
        {
            _receiveQueue = new ConcurrentQueue<(string, WebSocket)>();
            _clients = new List<WebSocket>();
            _cancellationTokenSource = new CancellationTokenSource();
            // Route connect/disconnect notifications to the agent main thread
            // so script_agent.dsl can react via on_ws_client_connected(port) /
            // on_ws_client_disconnected(port). _port is read at event time.
            OnClientConnected += () => MetaDslExecutor.EnqueueAgentPortEvent("on_ws_client_connected", _port);
            OnClientDisconnected += () => MetaDslExecutor.EnqueueAgentPortEvent("on_ws_client_disconnected", _port);
        }

        /// <summary>
        /// Starts the WebSocket server on the specified port
        /// </summary>
        public bool Start(int port)
        {
            if (_isRunning)
            {
                AgentCore.Instance.Logger.Warning("WebSocket server is already running");
                return false;
            }

            try
            {
                _port = port;
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://localhost:{port}/");
                // Also accept explicit loopback-IP clients (Host: 127.0.0.1:port)
                // alongside the "localhost" name - the browser relay wsclient may
                // use either form. The extra prefix needs no admin rights.
                _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
                _listener.Start();
                _isRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();

                // Start listener task
                Task.Run(() => AcceptConnectionsAsync(_cancellationTokenSource.Token));

                // Start queue processing tick in dedicated thread for stable timing
                _tickThread = new Thread(() => ProcessQueuesLoop(_cancellationTokenSource.Token))
                {
                    IsBackground = true,
                    Name = "WebSocketServerTick"
                };
                _tickThread.Start();

                AgentCore.Instance.Logger.Info($"WebSocket server started on port {port}");
                return true;
            }
            catch (Exception ex)
            {
                AgentCore.Instance.Logger.Error($"Failed to start WebSocket server: {ex.Message}\nStack: {ex.StackTrace}");
                _isRunning = false;
                return false;
            }
        }

        /// <summary>
        /// Stops the WebSocket server
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
                return;

            try
            {
                _isRunning = false;
                _cancellationTokenSource?.Cancel();

                // Wait for tick thread to finish
                if (_tickThread != null && _tickThread.IsAlive)
                {
                    if (!_tickThread.Join(TimeSpan.FromSeconds(2)))
                    {
                        AgentCore.Instance.Logger.Warning("Tick thread did not terminate gracefully");
                    }
                    _tickThread = null;
                }

                // Close all client connections
                lock (_lockObj)
                {
                    foreach (var client in _clients)
                    {
                        try
                        {
                            if (client.State == WebSocketState.Open)
                            {
                                client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down",
                                    CancellationToken.None).Wait(TimeSpan.FromSeconds(5));
                            }
                        }
                        catch { }
                    }
                    _clients.Clear();
                }

                _listener?.Stop();
                _listener?.Close();

                // Clear receive queue
                while (_receiveQueue.TryDequeue(out _)) { }

                AgentCore.Instance.Logger.Info("WebSocket server stopped");
            }
            catch (Exception ex)
            {
                AgentCore.Instance.Logger.Error($"Error stopping WebSocket server: {ex.Message}\nStack: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Checks whether an Origin header value comes from a local/trusted source.
        /// Only http://localhost, http://127.0.0.1, file:// and null (file:// sends Origin: null) are allowed.
        /// </summary>
        private static bool IsAllowedLocalOrigin(string? origin)
        {
            if (string.IsNullOrEmpty(origin))
                origin = "null";
            return string.Equals(origin, "null", StringComparison.OrdinalIgnoreCase)
                || origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase)
                || origin.StartsWith("http://127.0.0.1", StringComparison.OrdinalIgnoreCase)
                || origin.StartsWith("file://", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Accepts incoming WebSocket connections
        /// </summary>
        private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_listener == null || !_listener.IsListening)
                        break;

                    var context = await _listener.GetContextAsync();

                    // Handle Private Network Access (PNA) preflight - Chromium 130+ enforces this
                    // When a page from file:// or https:// connects to http://localhost, Chromium sends
                    // an OPTIONS preflight with Access-Control-Request-Private-Network: true
                    if (context.Request.HttpMethod == "OPTIONS" &&
                        context.Request.Headers["Access-Control-Request-Private-Network"] == "true")
                    {
                        string? origin = context.Request.Headers["Origin"];
                        if (IsAllowedLocalOrigin(origin))
                        {
                            AgentCore.Instance.Logger.Info($"PNA preflight accepted from origin: {origin}");
                            context.Response.StatusCode = 204;
                            context.Response.Headers.Add("Access-Control-Allow-Origin", origin ?? "null");
                            context.Response.Headers.Add("Access-Control-Allow-Private-Network", "true");
                            context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Upgrade, Connection, Sec-WebSocket-Key, Sec-WebSocket-Version");
                        }
                        else
                        {
                            AgentCore.Instance.Logger.Warning($"PNA preflight rejected from origin: {origin}");
                            context.Response.StatusCode = 403;
                        }
                        context.Response.Close();
                    }

                    else if (context.Request.IsWebSocketRequest)
                    {
                        // Validate Origin header - only allow localhost/known connections
                        string? origin = context.Request.Headers["Origin"];
                        if (!string.IsNullOrEmpty(origin) &&
                            !string.Equals(origin, "null", StringComparison.OrdinalIgnoreCase) && // file:// protocol sends Origin: null
                            !origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) &&
                            !origin.StartsWith("http://127.0.0.1", StringComparison.OrdinalIgnoreCase) &&
                            !origin.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                            !origin.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                        {
                            AgentCore.Instance.Logger.Warning($"WebSocket connection rejected from origin: {origin}");
                            context.Response.StatusCode = 403;
                            context.Response.Close();
                        }
                        else
                        {
                            _ = Task.Run(() => HandleWebSocketConnectionAsync(context, cancellationToken), cancellationToken);
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
                catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception ex)
                {
                    AgentCore.Instance.Logger.Error($"Error accepting connection: {ex.Message}\nStack: {ex.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Handles a single WebSocket connection
        /// </summary>
        private async Task HandleWebSocketConnectionAsync(HttpListenerContext context, CancellationToken cancellationToken)
        {
            WebSocket? webSocket = null;

            try
            {
                var wsContext = await context.AcceptWebSocketAsync(null);
                webSocket = wsContext.WebSocket;

                lock (_lockObj)
                {
                    _clients.Add(webSocket);
                }
                _contextRounds[webSocket] = 0;

                OnClientConnected?.Invoke();
                AgentCore.Instance.Logger.Info("WebSocket client connected");

                // Receive loop with support for large messages (up to 4MB)
                const int bufferSize = 4 * 1024 * 1024; // 4MB buffer
                var buffer = new byte[bufferSize];
                var messageBuilder = new MemoryStream();

                while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = await webSocket.ReceiveAsync(
                            new ArraySegment<byte>(buffer), cancellationToken);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            break;
                        }

                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            // Accumulate message fragments
                            messageBuilder.Write(buffer, 0, result.Count);

                            // Check if this is the end of the message
                            if (result.EndOfMessage)
                            {
                                var message = Encoding.UTF8.GetString(messageBuilder.GetBuffer(), 0, (int)messageBuilder.Length);
                                if (TryGetAgentEnvelope(message, out long envelopeId, out string? envelopeFunc, out List<BoxedValue>? envelopeArgs)) {
                                    if (envelopeFunc == "@bind") {
                                        // Bind this connection to an agent id for the raw
                                        // MetaDSL path (per-connection instance resolution).
                                        string bindAgentId = (envelopeArgs != null && envelopeArgs.Count > 0) ? (envelopeArgs[0].AsString ?? string.Empty) : string.Empty;
                                        if (!string.IsNullOrEmpty(bindAgentId)) {
                                            _clientAgentIds[webSocket] = bindAgentId;
                                            AgentCore.Instance.Logger.Info($"Client bound to agent '{bindAgentId}'");
                                        }
                                    }
                                    else {
                                        // Structured agent_call/agent_notify envelope: execute the
                                        // named dsl function on the agent main thread, replies go back
                                        // through this connection.
                                        var ctx = new DslContext(m => { _ = SendToClientAsync(webSocket, m); });
                                        MetaDslExecutor.EnqueueAgentCall(envelopeId, envelopeFunc!, envelopeArgs ?? new List<BoxedValue>(), ctx);
                                        AgentCore.Instance.Logger.Debug($"Agent envelope received from client, queued to main thread (func: {envelopeFunc}, id: {envelopeId})");
                                    }
                                }
                                else {
                                    // Raw MetaDSL code: existing worker pool path.
                                    _receiveQueue.Enqueue((message, webSocket));
                                    AgentCore.Instance.Logger.Debug($"Message received from client, queued (length: {message.Length})");
                                }
                                messageBuilder.SetLength(0);
                            }
                            else
                            {
                                AgentCore.Instance.Logger.Debug($"Received message fragment (accumulated: {messageBuilder.Length} bytes)");
                            }
                        }
                    }
                    catch (WebSocketException)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                AgentCore.Instance.Logger.Error($"WebSocket connection error: {ex.Message}\nStack: {ex.StackTrace}");
            }
            finally
            {
                if (webSocket != null)
                {
                    lock (_lockObj)
                    {
                        _clients.Remove(webSocket);
                    }
                    _contextRounds.TryRemove(webSocket, out _);
                    _clientAgentIds.TryRemove(webSocket, out _);

                    try
                    {
                        if (webSocket.State == WebSocketState.Open)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                "Connection closed", CancellationToken.None);
                        }
                    }
                    catch { }

                    webSocket.Dispose();
                }

                OnClientDisconnected?.Invoke();
                AgentCore.Instance.Logger.Info("WebSocket client disconnected");
            }
        }

        /// <summary>
        /// Tries to parse an incoming message as a structured agent envelope.
        /// Envelopes are JSON objects of the form
        ///   {"type":"agent_call","id":123,"func":"name","args":[...]} (reply expected)
        ///   {"type":"agent_notify","func":"name","args":[...]}          (no reply)
        /// Anything else (including MetaDSL code) returns false and takes the
        /// raw MetaDSL worker path. JSON objects inside args are passed to dsl
        /// as raw JSON strings.
        /// </summary>
        private static bool TryGetAgentEnvelope(string message, out long id, out string? func, out List<BoxedValue>? args)
        {
            id = 0;
            func = null;
            args = null;
            if (string.IsNullOrEmpty(message) || message[0] != '{') {
                return false;
            }
            try {
                using var doc = System.Text.Json.JsonDocument.Parse(message);
                var root = doc.RootElement;
                if (root.ValueKind != System.Text.Json.JsonValueKind.Object) {
                    return false;
                }
                if (!root.TryGetProperty("type", out var typeEl) || typeEl.ValueKind != System.Text.Json.JsonValueKind.String) {
                    return false;
                }
                string type = typeEl.GetString() ?? string.Empty;
                if (type == "agent_bind") {
                    // Connection-level instance binding for the raw MetaDSL
                    // path: {"type":"agent_bind","agentId":"venus"}. After this,
                    // raw code on this connection resolves its AgentInstance by
                    // the bound agent id instead of the server's default.
                    if (root.TryGetProperty("agentId", out var bindEl) && bindEl.ValueKind == System.Text.Json.JsonValueKind.String) {
                        var bindId = bindEl.GetString();
                        if (!string.IsNullOrEmpty(bindId)) {
                            func = "@bind";
                            args = new List<BoxedValue> { BoxedValue.FromString(bindId) };
                            return true;
                        }
                    }
                    return false;
                }
                if (type != "agent_call" && type != "agent_notify") {
                    return false;
                }
                if (!root.TryGetProperty("func", out var funcEl) || funcEl.ValueKind != System.Text.Json.JsonValueKind.String) {
                    return false;
                }
                func = funcEl.GetString();
                if (string.IsNullOrEmpty(func)) {
                    return false;
                }
                if (type == "agent_call" && root.TryGetProperty("id", out var idEl)) {
                    if (idEl.ValueKind == System.Text.Json.JsonValueKind.Number && idEl.TryGetInt64(out long longId)) {
                        id = longId;
                    }
                    else if (idEl.ValueKind == System.Text.Json.JsonValueKind.String && long.TryParse(idEl.GetString(), out long parsed)) {
                        id = parsed;
                    }
                }
                args = new List<BoxedValue>();
                if (root.TryGetProperty("args", out var argsEl) && argsEl.ValueKind == System.Text.Json.JsonValueKind.Array) {
                    foreach (var el in argsEl.EnumerateArray()) {
                        args.Add(BoxedValueFromJson(el));
                    }
                }
                return true;
            }
            catch (Exception) {
                // Not JSON or malformed: treat as raw MetaDSL code.
                return false;
            }
        }

        /// <summary>
        /// Converts a JSON element to a BoxedValue: strings/numbers/bools map
        /// directly, arrays become List&lt;BoxedValue&gt;, and objects are passed
        /// through as raw JSON text.
        /// </summary>
        private static BoxedValue BoxedValueFromJson(System.Text.Json.JsonElement el)
        {
            switch (el.ValueKind) {
                case System.Text.Json.JsonValueKind.String:
                    return BoxedValue.FromString(el.GetString());
                case System.Text.Json.JsonValueKind.True:
                    return BoxedValue.From(true);
                case System.Text.Json.JsonValueKind.False:
                    return BoxedValue.From(false);
                case System.Text.Json.JsonValueKind.Null:
                    return BoxedValue.NullObject;
                case System.Text.Json.JsonValueKind.Number:
                    if (el.TryGetInt64(out long longVal)) {
                        return BoxedValue.From(longVal);
                    }
                    return BoxedValue.From(el.GetDouble());
                case System.Text.Json.JsonValueKind.Array: {
                        var list = new List<BoxedValue>();
                        foreach (var item in el.EnumerateArray()) {
                            list.Add(BoxedValueFromJson(item));
                        }
                        return BoxedValue.FromObject(list);
                    }
                default:
                    // Object: hand the raw JSON text to dsl.
                    return BoxedValue.FromString(el.GetRawText());
            }
        }

        /// <summary>
        /// Processes receive queue on tick (runs in dedicated thread)
        /// Tick thread: dequeue from receive queue -> dispatch to worker tasks via thread pool
        /// </summary>
        private void ProcessQueuesLoop(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Dispatch messages to worker tasks when concurrency slots are available
                    while (_activeWorkers < _maxWorkerConcurrency
                        && _receiveQueue.TryDequeue(out var received))
                    {
                        Interlocked.Increment(ref _activeWorkers);
                        long workerId = Interlocked.Increment(ref _workerIdCounter);
                        _workerStartTimes[workerId] = DateTime.UtcNow;
                        var msg = received;
                        Task.Run(() =>
                        {
                            try
                            {
                                AgentCore.Instance.Logger.Info($"Worker processing message (active: {_activeWorkers}/{_maxWorkerConcurrency}): {msg.message.Substring(0, Math.Min(100, msg.message.Length))}...");

                                // Determine whether to append context for this round.
                                // Instance is keyed by agent id: per-connection agent_bind
                                // first, then the server default, then the port number.
                                string instKey = _clientAgentIds.TryGetValue(msg.client, out var boundAgentId)
                                    ? boundAgentId
                                    : (string.IsNullOrEmpty(AgentId) ? _port.ToString() : AgentId);
                                var inst = AgentCore.Instance.GetInstance(instKey);
                                int maxRounds = inst?.MaxContextRounds ?? 3;
                                int rounds = _contextRounds.AddOrUpdate(msg.client, 0, (_, old) => old + 1);
                                bool appendContext = (inst?.ContextInjectionEnabled ?? true) && ((maxRounds <= 1) || (rounds % maxRounds == 0));
                                // Clearing the User Context Rounds when using MetaDSL code
                                if (inst != null) inst.CurContextRounds = 0;

                                // Bind async callback delivery to the originating connection
                                MetaDslExecutor.PushContext(new DslContext(message => { _ = SendToClientAsync(msg.client, message); }));
                                string result;
                                try {
                                    result = ExecuteMetaDSLInWorker(msg.message, appendContext, inst);
                                }
                                finally {
                                    MetaDslExecutor.PopContext();
                                }
                                if (!string.IsNullOrEmpty(result))
                                {
                                    AgentCore.Instance.Logger.Info($"Sending result (length: {result.Length}) to originator client, appendContext={appendContext}");
                                    _ = SendToClientAsync(msg.client, result);
                                }
                                else
                                {
                                    AgentCore.Instance.Logger.Debug("MetaDSL execution returned empty result");
                                }
                            }
                            catch (Exception ex)
                            {
                                AgentCore.Instance.Logger.Error($"Worker error: {ex.Message}\nStack: {ex.StackTrace}");
                            }
                            finally
                            {
                                _workerStartTimes.TryRemove(workerId, out _);
                                Interlocked.Decrement(ref _activeWorkers);
                            }
                        }, cancellationToken);
                    }

                    // Periodic ping to detect and clean up half-open connections
                    if (DateTime.UtcNow - _lastPingTime > s_pingInterval)
                    {
                        _lastPingTime = DateTime.UtcNow;
                        CleanupDeadClients();
                    }

                    // Check for stuck workers
                    if ((int)(DateTime.UtcNow - _lastWorkerCheckTime).TotalSeconds >= c_workerCheckIntervalSeconds)
                    {
                        _lastWorkerCheckTime = DateTime.UtcNow;
                        CheckStuckWorkers();
                    }

                    // Periodic worker status log (every 2 seconds, only when workers are running)
                    if ((int)(DateTime.UtcNow - _lastWorkerStatusLogTime).TotalSeconds >= c_workerStatusLogIntervalSeconds)
                    {
                        _lastWorkerStatusLogTime = DateTime.UtcNow;
                        if (_workerStartTimes.Count > 0)
                        {
                            AgentCore.Instance.Logger.Info($"Worker status: {GetWorkerStatus()}");
                        }
                    }

                    // Small delay to prevent CPU spinning (100ms = 10 ticks per second)
                    Thread.Sleep(100);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    AgentCore.Instance.Logger.Error($"Error processing queues: {ex.Message}\nStack: {ex.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Executes MetaDSL code in worker thread (called by Tick thread)
        /// This is where the agent logic processes incoming messages
        /// </summary>
        private string ExecuteMetaDSLInWorker(string message, bool appendContext, AgentInstance? inst)
        {
            try {
                AgentCore.Instance.Logger.Debug($"Executing MetaDSL: {message}");
                string result = MetaDslExecutor.ExecuteMetaDslScript(message, (null != inst ? inst.MaxResultSize : 0), out var hasError);
                AgentCore.Instance.Logger.Debug($"MetaDSL execution completed, result length: {(result?.Length ?? 0)}");
                var sb = new StringBuilder();
                if (appendContext) {
                    if (inst != null) {
                        if (!string.IsNullOrEmpty(inst.Emphasize)) {
                            sb.AppendLine(inst.Emphasize);
                            sb.AppendLine();
                        }
                        if (!string.IsNullOrEmpty(inst.Soul)) {
                            sb.AppendLine(inst.Soul);
                            sb.AppendLine();
                        }
                        if (!string.IsNullOrEmpty(inst.Plan)) {
                            sb.AppendLine(inst.Plan);
                            sb.AppendLine();
                        }
                        if (!string.IsNullOrEmpty(inst.Context)) {
                            sb.AppendLine(inst.Context);
                            sb.AppendLine();
                        }
                        if (!string.IsNullOrEmpty(inst.History)) {
                            sb.AppendLine(inst.History);
                            sb.AppendLine();
                        }
                    }
                }
                sb.AppendLine("MetaDSL <{:>");
                sb.AppendLine(message);
                sb.AppendLine("<:}>;");
                sb.AppendLine("Result <{:>");
                sb.AppendLine(result);
                if (hasError) {
                    sb.AppendLine("<:}>");
                    sb.AppendLine("HasError;");
                }
                else {
                    sb.AppendLine("<:}>;");
                }

                // Archiving to the semantic index is a side channel: the MetaDSL code
                // has already run at this point. A failure here (e.g. SQLITE_BUSY when
                // several agent processes write concurrently) must NOT discard the
                // result, otherwise the caller sees an empty string and never sends it
                // back to the client.
                try {
                    var record = sb.ToString();
                    var embedding = Core.AgentCore.Instance.EmbeddingService;
                    if (embedding.IsReady) {
                        if (AgentCore.Instance.SemanticIndex.IsReady("metadsl_history")) {
                            float[]? vector = embedding.Encode(record);
                            if (null != vector) {
                                AgentCore.Instance.SemanticIndex.Add("metadsl_history", record, vector, string.Format("{{source: \"metadsl\", date: \"{0}\"}}", DateTime.Now.ToString()));
                            }
                        }
                        else {
                            AgentCore.Instance.SemanticIndex.InitCollection("metadsl_history");
                        }
                    }
                    else {
                        AgentCore.Instance.Logger.Error($"EmbeddingService not ready !");
                    }
                }
                catch (Exception ex) {
                    AgentCore.Instance.Logger.Error($"Failed to archive MetaDSL history (result is still returned): {ex.Message}\nStack: {ex.StackTrace}");
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                AgentCore.Instance.Logger.Error($"Error executing MetaDSL: {ex.Message}\nStack: {ex.StackTrace}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Sends a message to a specific client (the originator of a request).
        /// Supports large messages by sending in chunks if needed.
        /// internal: also used by the agent main thread to deliver
        /// agent_result replies and broadcasts.
        /// </summary>
        internal async Task SendToClientAsync(WebSocket client, string message)
        {
            if (client == null || string.IsNullOrEmpty(message))
                return;

            var buffer = Encoding.UTF8.GetBytes(message);
            const int chunkSize = 4 * 1024 * 1024; // 4MB chunks

            try
            {
                if (client.State == WebSocketState.Open)
                {
                    int offset = 0;
                    while (offset < buffer.Length)
                    {
                        int count = Math.Min(chunkSize, buffer.Length - offset);
                        bool endOfMessage = (offset + count) >= buffer.Length;

                        var segment = new ArraySegment<byte>(buffer, offset, count);
                        await client.SendAsync(segment, WebSocketMessageType.Text,
                            endOfMessage, CancellationToken.None);

                        offset += count;
                    }

                    AgentCore.Instance.Logger.Debug($"Message sent to originator client successfully (length: {buffer.Length})");
                }
                else
                {
                    AgentCore.Instance.Logger.Debug($"Skipped originator client with state: {client.State}");
                }
            }
            catch (Exception ex)
            {
                AgentCore.Instance.Logger.Debug($"Failed to send to originator client: {ex.Message}");
            }
        }

        /// <summary>
        /// Broadcasts a message to all connected clients
        /// Supports large messages by sending in chunks if needed
        /// internal: wrapped by WebSocketServerManager.BroadcastAll for
        /// server-initiated pushes without a request context
        /// </summary>
        internal async Task BroadcastMessageAsync(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            var buffer = Encoding.UTF8.GetBytes(message);
            const int chunkSize = 4 * 1024 * 1024; // 4MB chunks

            List<WebSocket> clientsCopy;
            lock (_lockObj)
            {
                clientsCopy = new List<WebSocket>(_clients);
            }

            foreach (var client in clientsCopy)
            {
                try
                {
                    if (client.State == WebSocketState.Open)
                    {
                        // Send message in chunks if it's larger than chunk size
                        int offset = 0;
                        while (offset < buffer.Length)
                        {
                            int count = Math.Min(chunkSize, buffer.Length - offset);
                            bool endOfMessage = (offset + count) >= buffer.Length;

                            var segment = new ArraySegment<byte>(buffer, offset, count);
                            await client.SendAsync(segment, WebSocketMessageType.Text,
                                endOfMessage, CancellationToken.None);

                            offset += count;
                        }

                        AgentCore.Instance.Logger.Debug($"Message sent to client successfully (length: {buffer.Length})");
                    }
                    else
                    {
                        AgentCore.Instance.Logger.Debug($"Skipped client with state: {client.State}");
                    }
                }
                catch (Exception ex)
                {
                    AgentCore.Instance.Logger.Debug($"Failed to send to client: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Removes dead/closed WebSocket connections from the client list.
        /// </summary>
        private void CleanupDeadClients()
        {
            List<WebSocket>? deadClients = null;
            lock (_lockObj)
            {
                for (int i = _clients.Count - 1; i >= 0; i--)
                {
                    var client = _clients[i];
                    if (client.State != WebSocketState.Open && client.State != WebSocketState.Connecting)
                    {
                        deadClients ??= new List<WebSocket>();
                        deadClients.Add(client);
                        _clients.RemoveAt(i);
                    }
                }
            }
            if (deadClients != null)
            {
                foreach (var client in deadClients)
                {
                    try { client.Dispose(); } catch { }
                }
                AgentCore.Instance.Logger.Debug($"Cleaned up {deadClients.Count} dead WebSocket client(s)");
            }
        }

        /// <summary>
        /// Checks for stuck workers and logs warnings.
        /// </summary>
        private void CheckStuckWorkers()
        {
            var now = DateTime.UtcNow;
            foreach (var kv in _workerStartTimes)
            {
                int duration = (int)(now - kv.Value).TotalSeconds;
                if (duration > _workerTimeoutSeconds)
                {
                    AgentCore.Instance.Logger.Warning($"WebSocket worker {kv.Key} has been running for {duration}s (limit {_workerTimeoutSeconds}s), may be stuck");
                }
            }
        }

        /// <summary>
        /// Returns a status string of all active workers.
        /// </summary>
        public string GetWorkerStatus()
        {
            var now = DateTime.UtcNow;
            var sb = new StringBuilder();
            int stuck = 0;
            foreach (var kv in _workerStartTimes)
            {
                int duration = (int)(now - kv.Value).TotalSeconds;
                sb.AppendLine($"worker {kv.Key}: {duration}s");
                if (duration > _workerTimeoutSeconds) stuck++;
            }
            sb.AppendLine($"total: {_workerStartTimes.Count}, active: {_activeWorkers}/{_maxWorkerConcurrency}, stuck(>{_workerTimeoutSeconds}s): {stuck}");
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Gets or sets the worker timeout in seconds.
        /// </summary>
        public int WorkerTimeoutSeconds
        {
            get => _workerTimeoutSeconds;
            set => _workerTimeoutSeconds = Math.Max(60, value);
        }

        /// <summary>
        /// Gets the number of connected clients
        /// </summary>
        public int GetClientCount()
        {
            lock (_lockObj)
            {
                return _clients.Count;
            }
        }

        /// <summary>
        /// Dequeues a message from the receive queue
        /// </summary>
        /// <returns>Message or null if queue is empty</returns>
        public string? DequeueMessage()
        {
            if (_receiveQueue.TryDequeue(out var item))
            {
                return item.message;
            }
            return null;
        }

        /// <summary>
        /// Gets the count of messages in receive queue
        /// </summary>
        public int GetReceiveQueueCount()
        {
            return _receiveQueue.Count;
        }

        /// <summary>
        /// Disposes the server resources
        /// </summary>
        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }
    }
}