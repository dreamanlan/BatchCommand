namespace CefDotnetApp.Interfaces
{
    // Marker interfaces for operation classes
    // These allow IAgentCore to reference operation types without circular dependency

    public interface IFileOperations { }
    public interface IDiffOperations { }
    public interface IClipboardOperations { }
    public interface ILoggingAndDebugging
    {
        void Info(string message, params object[] args);
        void Error(string message, params object[] args);
        void Warning(string message, params object[] args);
        void Debug(string message, params object[] args);
        void Trace(string message, params object[] args);
        void Fatal(string message, params object[] args);
    }
    public interface IHttpClientOperations { }
    public interface ITaskManagement { }
    public interface ILLMInteraction { }
    public interface IProcessOperations { }
    public interface IContextManagement { }
    public interface ICodeAnalysis { }
    public interface ITemplateEngine { }
    public interface IBrowserInteraction { }
    public interface IAgentMessageHandler { }
}
