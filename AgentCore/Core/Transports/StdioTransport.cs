using System;
using AgentPlugin.Abstractions;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// stdio transport: launches a local process and communicates via stdin/stdout.
    /// target = full command line, e.g. "npx -y @modelcontextprotocol/server-filesystem /path"
    /// </summary>
    internal class StdioTransport : IMcpTransport
    {
        private readonly string _command;
        private readonly string _arguments;
        private Process? _process;
        private StreamWriter? _stdin;
        private StreamReader? _stdout;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private readonly TimeSpan _sendTimeout;
        private readonly TimeSpan _lockTimeout;

        public bool IsConnected => _process != null && !_process.HasExited;

        private readonly bool _useLspFraming;

        public StdioTransport(string target, int timeoutMs = 60000, bool useLspFraming = false)
        {
            _useLspFraming = useLspFraming;
            _sendTimeout = TimeSpan.FromMilliseconds(timeoutMs);
            _lockTimeout = TimeSpan.FromMilliseconds(timeoutMs + 5000);
            // Expand environment variables before splitting command and arguments
            string expanded = Environment.ExpandEnvironmentVariables(target);
            int spaceIdx = expanded.IndexOf(' ');
            if (spaceIdx > 0)
            {
                _command = expanded.Substring(0, spaceIdx);
                _arguments = expanded.Substring(spaceIdx + 1);
            }
            else
            {
                _command = expanded;
                _arguments = string.Empty;
            }
        }

        public Task ConnectAsync()
        {
            // Note: StandardInputEncoding = Encoding.UTF8 (which has emitBOM=true)
            var psi = new ProcessStartInfo
            {
                FileName = _command,
                Arguments = _arguments,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardInputEncoding = new UTF8Encoding(false),
                StandardOutputEncoding = new UTF8Encoding(false),
            };
            _process = Process.Start(psi)
                ?? throw new InvalidOperationException($"Failed to start MCP process: {_command} {_arguments}");
            _stdin = _process.StandardInput;
            _stdout = _process.StandardOutput;
            // Drain stderr to prevent pipe buffer deadlock when child process writes to stderr
            _process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[MCP:stderr] {e.Data}");
            };
            _process.BeginErrorReadLine();
            try
            {
                AgentCore.Instance.Logger.Info($"[MCP:stdio] process started: pid={_process.Id}, cmd={_command}, args={_arguments}, lspFraming={_useLspFraming}");
            }
            catch { }
            return Task.CompletedTask;
        }

        public async Task<string> SendRequestAsync(string jsonRpcRequest)
        {
            if (_stdin == null || _stdout == null)
                throw new InvalidOperationException("StdioTransport not connected");

            if (!await _lock.WaitAsync(_lockTimeout))
                throw new TimeoutException("StdioTransport: timed out waiting for send lock");
            try
            {
                await WriteFramedAsync(jsonRpcRequest);
                string? msg = await ReadOneMessageAsync(_sendTimeout);
                return msg ?? string.Empty;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SendNotificationAsync(string jsonRpcNotification)
        {
            if (_stdin == null)
                throw new InvalidOperationException("StdioTransport not connected");

            if (!await _lock.WaitAsync(_lockTimeout))
                throw new TimeoutException("StdioTransport: timed out waiting for send lock");
            try
            {
                // Notification: write only, do NOT read stdout (no response expected)
                await WriteFramedAsync(jsonRpcNotification);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task WriteFramedAsync(string payload)
        {
            // Write raw bytes directly to BaseStream to avoid:
            // 1) Newline normalization by StreamWriter
            var stream = _stdin!.BaseStream;
            if (_useLspFraming)
            {
                var bodyBytes = Encoding.UTF8.GetBytes(payload);
                var headerBytes = Encoding.ASCII.GetBytes($"Content-Length: {bodyBytes.Length}\r\n\r\n");
                await stream.WriteAsync(headerBytes, 0, headerBytes.Length);
                await stream.WriteAsync(bodyBytes, 0, bodyBytes.Length);
            }
            else
            {
                // MCP stdio spec requires single LF (\n) as message delimiter, not CRLF.
                var bytes = Encoding.UTF8.GetBytes(payload + "\n");
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
            await stream.FlushAsync();
            try
            {
                var rawBytes = Encoding.UTF8.GetBytes(payload);
                int hexLen = Math.Min(16, rawBytes.Length);
                var sb = new StringBuilder(hexLen * 3);
                for (int i = 0; i < hexLen; i++)
                {
                    if (i > 0) sb.Append(' ');
                    sb.Append(rawBytes[i].ToString("X2"));
                }
                string preview = payload.Length > 2048 ? payload.Substring(0, 2048) + "...(truncated)" : payload;
                AgentCore.Instance.Logger.Info($"[MCP:stdio] WRITE {rawBytes.Length} bytes, framing={(_useLspFraming ? "lsp" : "ndjson")}, hex16=[{sb}], text={preview}");
            }
            catch { }
        }

        private async Task<string?> ReadOneMessageAsync(TimeSpan budget)
        {
            // First line read with bounded budget
            var readTask = _stdout!.ReadLineAsync();
            using var delayCts = new CancellationTokenSource();
            var delayTask = Task.Delay(budget, delayCts.Token);
            var completed = await Task.WhenAny(readTask, delayTask);
            if (completed == readTask)
                delayCts.Cancel();
            else
                throw new TimeoutException($"StdioTransport: no response within {_sendTimeout.TotalSeconds}s");
            string? firstLine = await readTask;
            try
            {
                string preview = firstLine == null
                    ? "<null>"
                    : (firstLine.Length > 2048 ? firstLine.Substring(0, 2048) + "...(truncated)" : firstLine);
                AgentCore.Instance.Logger.Info($"[MCP:stdio] READ line: {preview}");
            }
            catch { }
            if (firstLine == null)
                return null;
            // Auto-detect: if starts with Content-Length, parse LSP framing
            if (firstLine.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                int contentLength = int.Parse(firstLine.Substring("Content-Length:".Length).Trim());
                // Read until empty line (end of headers)
                while (true)
                {
                    var headerLine = await _stdout.ReadLineAsync();
                    if (string.IsNullOrEmpty(headerLine))
                        break;
                }
                // Read exact content length
                var buffer = new char[contentLength];
                int totalRead = 0;
                while (totalRead < contentLength)
                {
                    int read = await _stdout.ReadAsync(buffer, totalRead, contentLength - totalRead);
                    if (read == 0)
                        throw new IOException("StdioTransport: unexpected end of stream while reading LSP body");
                    totalRead += read;
                }
                string body = new string(buffer, 0, totalRead);
                try
                {
                    string preview = body.Length > 2048 ? body.Substring(0, 2048) + "...(truncated)" : body;
                    AgentCore.Instance.Logger.Info($"[MCP:stdio] READ body ({totalRead} chars): {preview}");
                }
                catch { }
                return body;
            }
            // Otherwise treat as NDJSON (the line itself is the JSON)
            return firstLine;
        }

        public void Disconnect()
        {
            try { _stdin?.Close(); } catch { }
            try { _process?.Kill(); } catch { }
            _process = null;
            _stdin = null;
            _stdout = null;
        }

        public void Dispose() => Disconnect();
    }
}
