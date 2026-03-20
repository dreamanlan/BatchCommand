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

        public StdioTransport(string target, int timeoutMs = 60000)
        {
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
                StandardInputEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
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
                await _stdin.WriteLineAsync(jsonRpcRequest);
                await _stdin.FlushAsync();
                // Wrap ReadLineAsync with timeout to prevent indefinite blocking
                var readTask = _stdout.ReadLineAsync();
                using var delayCts = new CancellationTokenSource();
                var delayTask = Task.Delay(_sendTimeout, delayCts.Token);
                var completed = await Task.WhenAny(readTask, delayTask);
                if (completed == readTask)
                    delayCts.Cancel(); // cancel unused delay to avoid Task leak
                else
                    throw new TimeoutException($"StdioTransport: no response within {_sendTimeout.TotalSeconds}s");
                string? line = await readTask;
                return line ?? string.Empty;
            }
            finally
            {
                _lock.Release();
            }
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
