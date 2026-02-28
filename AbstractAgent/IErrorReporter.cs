using System.Text;

namespace AgentPlugin.Abstractions
{
    /// <summary>
    /// Abstraction for API error reporting (replaces NativeApi static error methods).
    /// </summary>
    public interface IErrorReporter
    {
        void ClearApiErrorInfo();
        void AppendApiErrorInfo(string msg);
        void AppendApiErrorInfoLine(string msg);
        void AppendApiErrorInfoFormat(string fmt, params object[] args);
        void AppendApiErrorInfoFormatLine(string fmt, params object[] args);
        bool HasApiErrorInfo { get; }
        string GetApiErrorInfo();
    }
}