using System;
using System.Collections.Generic;
using ScriptableFramework;
using BatchCommand;
using BatchCommand.Utils;

namespace AgentCore.Core
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public class LoggingAndDebugging
    {
        private readonly LogLevel _minLogLevel;

        public LoggingAndDebugging(LogLevel minLogLevel = LogLevel.Info,
            Action<string>? nativeLogAction = null, Action<string>? jsLogAction = null)
        {
            _minLogLevel = minLogLevel;
        }

        public void Log(LogLevel level, string message, params object[] args)
        {
            if (level < _minLogLevel)
                return;

            try {
                string formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
                string logEntry = FormatLogEntry(level, formattedMessage);

                WriteToLog(logEntry);
            }
            catch (Exception ex) {
                string errorMsg = $"[LogError] Failed to log message: {ex.Message}";
                try {
                    WriteToLog(errorMsg);
                }
                catch {
                    // Silently fail - we're in early initialization
                }
            }
        }

        private static readonly object _logLock = new object();
        private void WriteToLog(string message)
        {
            try {
                Console.WriteLine(message);
                // Write to a fallback log file in the current directory
                string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agentcore.log");
                lock (_logLock) {
                    System.IO.File.AppendAllText(logPath, message + Environment.NewLine);
                }
            }
            catch {
                // Silently fail - we're in early initialization
            }
        }

        public void Trace(string message, params object[] args)
        {
            Log(LogLevel.Trace, message, args);
        }

        public void Debug(string message, params object[] args)
        {
            Log(LogLevel.Debug, message, args);
        }

        public void Info(string message, params object[] args)
        {
            Log(LogLevel.Info, message, args);
        }

        public void Warning(string message, params object[] args)
        {
            Log(LogLevel.Warning, message, args);
        }

        public void Error(string message, params object[] args)
        {
            Log(LogLevel.Error, message, args);
        }

        public void Fatal(string message, params object[] args)
        {
            Log(LogLevel.Fatal, message, args);
        }

        public void Exception(Exception ex, string? message = null)
        {
            string exMessage = message != null
                ? $"{message}: {ex.Message}\n{ex.StackTrace}"
                : $"{ex.Message}\n{ex.StackTrace}";

            Log(LogLevel.Error, exMessage);
        }

        private string FormatLogEntry(LogLevel level, string message)
        {
            return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level,-7}] {message}";
        }
    }
}
