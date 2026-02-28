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
        void EnqueueLlmCallback(string providerId, string tag, string topic, string reply);
        void EnqueueMcpCallback(string serverId, string callbackTag, string result);
        string GetStringInLength(string str, int len, bool getTheEnd);
        string QuoteString(string? value);
        string StripQuotes(string? s);
    }
}
