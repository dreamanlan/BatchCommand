namespace AgentPlugin.Abstractions
{
    /// <summary>
    /// Abstraction for native browser interaction (NativeLog, JS execution, async callbacks).
    /// </summary>
    public interface INativeApi
    {
        void NativeLog(string msg);
        void JavascriptLog(string msg);
        void SendJavascriptCode(string code);
        void SendJavascriptCall(string func, string[] args);
        void EnqueueCefMessage(string msgName, string[] args);
        string GetStringInLength(string str, int len, int beginOrEndOrBeginEnd);
        string QuoteString(string? value);
        string StripQuotes(string? s);
    }
}
