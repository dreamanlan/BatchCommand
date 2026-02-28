using System;
using AgentPlugin.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// WebSocket server with dual queue message management
    /// </summary>
    public class WebSocketServer : IDisposable
    {
        private HttpListener _listener = null!;
        private ConcurrentQueue<string> _receiveQueue;
        private List<WebSocket> _clients;
        private CancellationTokenSource _cancellationTokenSource;
        private Thread? _tickThread;
        private readonly object _lockObj = new object();
        private bool _isRunning;
        private int _port;
        private DateTime _lastPingTime = DateTime.MinValue;
        private static readonly TimeSpan s_pingInterval = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets whether the server is currently running
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Gets the port the server is listening on
        /// </summary>
        public int Port => _port;

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
            _receiveQueue = new ConcurrentQueue<string>();
            _clients = new List<WebSocket>();
            _cancellationTokenSource = new CancellationTokenSource();
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

                    if (context.Request.IsWebSocketRequest)
                    {
                        // Validate Origin header - only allow localhost/known connections
                        string? origin = context.Request.Headers["Origin"];
                        if (!string.IsNullOrEmpty(origin) &&
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
                                _receiveQueue.Enqueue(message);
                                AgentCore.Instance.Logger.Debug($"Message received from client, queued (length: {message.Length})");
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
        /// Processes receive queue on tick (runs in dedicated thread)
        /// Tick thread: dequeue from receive queue -> ExecuteMetaDSLInWorker -> broadcast result
        /// </summary>
        private void ProcessQueuesLoop(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Process receive queue - dequeue one message, execute MetaDSL, broadcast result directly
                    if (_receiveQueue.TryDequeue(out string? receivedMessage))
                    {
                        AgentCore.Instance.Logger.Info($"Processing message from queue: {receivedMessage.Substring(0, Math.Min(100, receivedMessage.Length))}...");
                        string result = ExecuteMetaDSLInWorker(receivedMessage);
                        if (!string.IsNullOrEmpty(result))
                        {
                            AgentCore.Instance.Logger.Info($"Broadcasting result (length: {result.Length}) to {GetClientCount()} client(s)");
                            // Fire and forget - WebSocket send is async non-blocking
                            _ = BroadcastMessageAsync(result);
                        }
                        else
                        {
                            AgentCore.Instance.Logger.Debug("MetaDSL execution returned empty result");
                        }
                    }

                    // Periodic ping to detect and clean up half-open connections
                    if (DateTime.UtcNow - _lastPingTime > s_pingInterval)
                    {
                        _lastPingTime = DateTime.UtcNow;
                        CleanupDeadClients();
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
        private string ExecuteMetaDSLInWorker(string message)
        {
            try {
                AgentCore.Instance.Logger.Debug($"Executing MetaDSL: {message}");
                string result = AgentFrameworkService.Instance.DslEngine!.ExecuteMetaDslScript(message);
                AgentCore.Instance.Logger.Debug($"MetaDSL execution completed, result length: {(result?.Length ?? 0)}");
                var sb = new StringBuilder();
                sb.AppendLine("MetaDSL {:");
                sb.AppendLine(message);
                sb.AppendLine(":};");
                sb.AppendLine("Result {:");
                sb.AppendLine(result);
                sb.AppendLine(":};");
                if (!string.IsNullOrEmpty(AgentCore.Instance.ToDo)) {
                    sb.AppendLine();
                    sb.AppendLine(AgentCore.Instance.ToDo);
                }
                if (!string.IsNullOrEmpty(AgentCore.Instance.History)) {
                    sb.AppendLine();
                    sb.AppendLine(AgentCore.Instance.History);
                }
                if (!string.IsNullOrEmpty(AgentCore.Instance.Context)) {
                    sb.AppendLine();
                    sb.AppendLine(AgentCore.Instance.Context);
                }
                else if (!string.IsNullOrEmpty(AgentCore.Instance.Plan)) {
                    sb.AppendLine();
                    sb.AppendLine(AgentCore.Instance.Plan);
                }
                if (!string.IsNullOrEmpty(AgentCore.Instance.Emphasize)) {
                    sb.AppendLine();
                    sb.AppendLine(AgentCore.Instance.Emphasize);
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
        /// Broadcasts a message to all connected clients
        /// Supports large messages by sending in chunks if needed
        /// </summary>
        private async Task BroadcastMessageAsync(string message)
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
            if (_receiveQueue.TryDequeue(out string? message))
            {
                return message;
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