using System;
using System.Collections.Generic;

using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
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
        private Action<string> _nativeLogAction;
        private Action<string> _jsLogAction;

        public LoggingAndDebugging(LogLevel minLogLevel = LogLevel.Info,
            Action<string> nativeLogAction = null, Action<string> jsLogAction = null)
        {
            _minLogLevel = minLogLevel;
            _nativeLogAction = nativeLogAction;
            _jsLogAction = jsLogAction;
        }

        public void Log(LogLevel level, string message, params object[] args)
        {
            if (level < _minLogLevel)
                return;

            try {
                string formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
                string logEntry = FormatLogEntry(level, formattedMessage);

                if (_nativeLogAction != null)
                {
                    _nativeLogAction.Invoke(logEntry);
                }
                else
                {
                    // Fallback to file logging when NativeApi is not yet set
                    // This allows logging during early initialization (e.g., NativeLibraryLoader)
                    WriteToFallbackLog(logEntry);
                }
            }
            catch (Exception ex) {
                string errorMsg = $"[LogError] Failed to log message: {ex.Message}";
                try {
                    if (_nativeLogAction != null)
                    {
                        _nativeLogAction.Invoke(errorMsg);
                    }
                    else
                    {
                        WriteToFallbackLog(errorMsg);
                    }
                }
                catch {
                    // Silently fail - we're in early initialization
                }
            }
        }

        private static readonly object _fallbackLogLock = new object();
        private void WriteToFallbackLog(string message)
        {
            try {
                // Write to a fallback log file in the current directory
                string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agentcore_early.log");
                lock (_fallbackLogLock)
                {
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

        public void Exception(Exception ex, string message = null)
        {
            string exMessage = message != null
                ? $"{message}: {ex.Message}\n{ex.StackTrace}"
                : $"{ex.Message}\n{ex.StackTrace}";

            Log(LogLevel.Error, exMessage);
        }

        public void NativeLog(string message, params object[] args)
        {
            try {
                string formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
                _nativeLogAction?.Invoke(formattedMessage);
            }
            catch (Exception ex) {
                _nativeLogAction?.Invoke($"[NativeLogError] {ex.Message}");
            }
        }

        public void JavascriptLog(string message, params object[] args)
        {
            try {
                string formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
                _jsLogAction?.Invoke(formattedMessage);
            }
            catch (Exception ex) {
                _nativeLogAction?.Invoke($"[JsLogError] {ex.Message}");
            }
        }

        private string FormatLogEntry(LogLevel level, string message)
        {
            return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level,-7}] {message}";
        }

        // Set or update the native log action
        public void SetNativeLogAction(Action<string> nativeLogAction)
        {
            var currentNativeLog = _nativeLogAction;
            _nativeLogAction = nativeLogAction;
        }

        // Set or update the JavaScript log action
        public void SetJsLogAction(Action<string> jsLogAction)
        {
            _jsLogAction = jsLogAction;
        }
    }
}
