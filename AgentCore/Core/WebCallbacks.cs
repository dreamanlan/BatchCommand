using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using ScriptableFramework;

namespace AgentCore.Core
{
    /// <summary>
    /// Shared helpers for the httpproxy / webserver dsl web callbacks
    /// (see Core/UrlFilterEngine.cs and DslHost.ExecuteDslFileInWorker).
    ///
    /// The callbacks exchange C# objects, no json/base64 encoding: the
    /// request/response messages (web server pages and static filters: a
    /// headers dictionary / the HttpListenerRequest) and the body byte
    /// array are handed to the script as object references (readable
    /// through http_get_request_headers / http_get_response_headers and
    /// the bytes_to_* apis), and the script returns a Result object
    /// (new_web_result / set_web_result_* apis). Returning null (or an
    /// empty string) means "not modified, continue as-is"; a plain string
    /// return replaces the body with that text (utf-8). Invoke is fail-open:
    /// an error or a timeout never breaks the served request.
    ///
    /// Result fields: "status" (response status), "set_headers"/"del_headers"
    /// (name/value overrides), "body" (byte array replacement; only honoured
    /// when the body was buffered - SSE / binary / oversized or chunked
    /// bodies get header-only callbacks with an empty body argument),
    /// "follow_redirects" (proxy only, selects the manual-redirect client),
    /// "abort" (answer directly instead of forwarding / serving).
    /// </summary>
    internal static class WebCallbacks
    {
        public const int CallbackTimeoutMs = 10_000;
        public const int MaxCallbackBodyBytes = 8 * 1024 * 1024;

        // Worker pool convention: the filter callbacks run on worker 0 (they
        // are quick, mostly header munging - serial throughput is plenty and
        // a slow filter degrades through the callback timeout, not by
        // blocking the pool), the web server dsl pages run on worker 1 (slow
        // jobs must never occupy the filter worker). Both stay cache stable:
        // each worker keeps its script loaded without switching.
        public const int FilterWorkerIndex = 0;
        public const int PageWorkerIndex = 1;

        public sealed class Result
        {
            public int Status;
            public bool HasStatus;
            public Dictionary<string, string> SetHeaders = new(StringComparer.OrdinalIgnoreCase);
            public List<string> DelHeaders = new();
            public byte[] Body = Array.Empty<byte>();
            public bool HasBody;
            public bool FollowRedirects = true;
            public bool HasFollowRedirects;
            public bool Abort;
        }

        /// <summary>
        /// Invokes a filter callback; null means "no modification". Return
        /// handling: a Result object is returned as-is; a string is turned
        /// into a Result whose body is that text (utf-8); null (or an empty
        /// string), an error or a timeout all mean "not modified" (errors
        /// are logged).
        /// </summary>
        public static Result? Invoke(string dslPath, string func, params BoxedValue[] args)
        {
            if (string.IsNullOrEmpty(dslPath)) {
                return null;
            }
            try {
                bool hasError;
                var value = MetaDslExecutor.ExecuteDslFileInWorker(dslPath, func, args, CallbackTimeoutMs, FilterWorkerIndex, out hasError, out string error);
                if (hasError) {
                    AgentCore.Instance.Logger.Error("[filter] " + func + " failed: " + error);
                    return null;
                }
                if (value.IsNullObject) {
                    return null;
                }
                object? obj = value.GetObject();
                if (obj is Result r) {
                    return r;
                }
                if (obj is string s) {
                    // A string return replaces the body with that text
                    // (utf-8, verbatim - no trimming, the text IS the body);
                    // an empty string means no modification.
                    if (s.Length == 0) {
                        return null;
                    }
                    return new Result {
                        Body = Encoding.UTF8.GetBytes(s),
                        HasBody = true,
                    };
                }
                AgentCore.Instance.Logger.Error("[filter] " + func + " returned an unexpected type: " + obj?.GetType().Name);
                return null;
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error("[filter] " + func + " error: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Collects the headers of an upstream request (request + content
        /// headers) as a Dictionary&lt;string, List&lt;string&gt;&gt; - the same data
        /// the callbacks inspect via http_get_request_headers.
        /// </summary>
        public static Dictionary<string, List<string>> RequestHeadersToDict(HttpRequestMessage msg)
        {
            var dict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var h in msg.Headers) {
                AddHeaderValues(dict, h.Key, h.Value);
            }
            foreach (var h in msg.Content!.Headers) {
                AddHeaderValues(dict, h.Key, h.Value);
            }
            return dict;
        }

        /// <summary>
        /// Collects the headers of an upstream response (headers + content
        /// headers) as a Dictionary&lt;string, List&lt;string&gt;&gt; - the same data
        /// the callbacks inspect via http_get_response_headers.
        /// </summary>
        public static Dictionary<string, List<string>> ResponseHeadersToDict(HttpResponseMessage msg)
        {
            var dict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var h in msg.Headers) {
                AddHeaderValues(dict, h.Key, h.Value);
            }
            foreach (var h in msg.Content.Headers) {
                AddHeaderValues(dict, h.Key, h.Value);
            }
            return dict;
        }

        /// <summary>
        /// Collects the request line data (Method/Url) and headers of an
        /// HttpListenerRequest (the web server page / filter requests) as a
        /// Dictionary&lt;string, List&lt;string&gt;&gt;.
        /// </summary>
        public static Dictionary<string, List<string>> ListenerRequestToDict(HttpListenerRequest req)
        {
            var dict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase) {
                ["Method"] = new List<string> { req.HttpMethod },
                ["Url"] = new List<string> { req.Url?.ToString() ?? string.Empty },
            };
            foreach (string? key in req.Headers) {
                if (string.IsNullOrEmpty(key)) {
                    continue;
                }
                var list = new List<string>();
                foreach (string? v in req.Headers.GetValues(key) ?? Array.Empty<string?>()) {
                    if (v != null) {
                        list.Add(v);
                    }
                }
                if (list.Count > 0) {
                    dict[key] = list;
                }
            }
            return dict;
        }

        private static void AddHeaderValues(Dictionary<string, List<string>> dict, string name, IEnumerable<string?> values)
        {
            if (!dict.TryGetValue(name, out var list)) {
                list = new List<string>();
                dict[name] = list;
            }
            foreach (string? v in values) {
                if (v != null) {
                    list.Add(v);
                }
            }
        }

        /// <summary>
        /// True for content types whose body may be buffered and handed to
        /// a response callback (text-ish types; SSE is excluded - it must
        /// keep streaming chunk by chunk).
        /// </summary>
        public static bool IsModifiableType(string? contentType)
        {
            if (string.IsNullOrEmpty(contentType)) {
                return false;
            }
            string t = contentType.Split(';')[0].Trim().ToLowerInvariant();
            if (t == "text/event-stream") {
                return false;
            }
            return t.StartsWith("text/", StringComparison.Ordinal)
                || t.Contains("json", StringComparison.Ordinal)
                || t.Contains("xml", StringComparison.Ordinal)
                || t.Contains("javascript", StringComparison.Ordinal)
                || t.Contains("ecmascript", StringComparison.Ordinal)
                || t.Contains("svg", StringComparison.Ordinal);
        }

        /// <summary>
        /// Applies status / del_headers / set_headers of a callback result
        /// to an HttpListenerResponse before the body is written. Content-Type
        /// goes through resp.ContentType; Content-Length is ignored (the body
        /// writer owns it).
        /// </summary>
        public static void ApplyToResponse(HttpListenerResponse resp, Result? mod)
        {
            if (mod == null) {
                return;
            }
            if (mod.HasStatus && mod.Status >= 100 && mod.Status <= 599) {
                resp.StatusCode = mod.Status;
            }
            foreach (string name in mod.DelHeaders) {
                if (name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }
                try { resp.Headers.Remove(name); } catch { }
            }
            foreach (var kv in mod.SetHeaders) {
                if (kv.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)) {
                    resp.ContentType = kv.Value;
                }
                else if (kv.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)) {
                    // ignored: the body writer owns the length
                }
                else {
                    try { resp.Headers.Remove(kv.Key); resp.AddHeader(kv.Key, kv.Value); } catch { }
                }
            }
        }

        /// <summary>
        /// Answers a request directly from an "abort" callback result
        /// (status default 403, optional headers/body) instead of
        /// forwarding / serving. ACAO is always echoed so the page can read
        /// the response.
        /// </summary>
        public static void WriteAbortResponse(HttpListenerResponse resp, string? origin, Result mod, int defaultStatus)
        {
            string contentType = "text/plain; charset=utf-8";
            if (mod.SetHeaders.TryGetValue("Content-Type", out string? ct)) {
                contentType = ct;
            }
            resp.StatusCode = mod.HasStatus ? mod.Status : defaultStatus;
            resp.ContentType = contentType;
            resp.AddHeader("Access-Control-Allow-Origin", string.IsNullOrEmpty(origin) ? "*" : origin);
            ApplyToResponse(resp, mod);
            byte[] data = mod.HasBody && mod.Body != null ? mod.Body : Array.Empty<byte>();
            resp.ContentLength64 = data.Length;
            if (data.Length > 0) {
                resp.OutputStream.Write(data, 0, data.Length);
            }
            resp.Close();
        }
    }
}
