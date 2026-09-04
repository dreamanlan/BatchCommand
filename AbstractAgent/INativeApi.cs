using ScriptableFramework;

namespace AbstractAgent
{
    /// <summary>
    /// Abstraction for native browser interaction (NativeLog, JS execution, async callbacks).
    /// </summary>
    public interface INativeApi
    {
        void NativeLog(string msg);
        void JavascriptLog(string msg);
        void SendJavascriptCode(string code);
        void SendJavascriptCall(string func, IList<BoxedValue> args);
        void EnqueueCefMessage(string msgName, IList<BoxedValue> args);
        string GetStringInLength(string str, int len, int beginOrEndOrBeginEnd);
        string QuoteString(string? value);
        System.Collections.Generic.IEnumerable<string> GetHelpDocs();

        string StripQuotes(string? s);
    }
}
