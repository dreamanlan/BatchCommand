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

    public class LoggingAndDebugging : ILoggingAndDebugging
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

            try
            {
                string formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
                string logEntry = FormatLogEntry(level, formattedMessage);

                _nativeLogAction?.Invoke(logEntry);
            }
            catch (Exception ex)
            {
                _nativeLogAction?.Invoke($"[LogError] Failed to log message: {ex.Message}");
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
            try
            {
                string formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
                _nativeLogAction?.Invoke(formattedMessage);
            }
            catch (Exception ex)
            {
                _nativeLogAction?.Invoke($"[NativeLogError] {ex.Message}");
            }
        }

        public void JavascriptLog(string message, params object[] args)
        {
            try
            {
                string formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
                _jsLogAction?.Invoke(formattedMessage);
            }
            catch (Exception ex)
            {
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
