using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AgentCore.Core
{
    /// <summary>
    /// Static web server (HttpListener based, one instance per port), started
    /// and stopped from dsl (webserver_start_server / webserver_stop_server,
    /// see ScriptApi/WebServerApi.cs; mirrors the httpproxy family).
    ///
    /// Serves a document root over http (or https - the https listener needs
    /// the certificate bound to the port beforehand with
    ///   netsh http add sslcert ... (see ssl_selfsign_setup.ps1)
    /// ). Requests are answered as follows:
    ///
    ///   - OPTIONS: answered as a CORS preflight (204 + allow headers).
    ///   - GET/HEAD on a directory: the first default document found
    ///     (index.html, index.htm), 404 when none exists.
    ///   - GET/HEAD on a *.dsl file: the file is executed as a BatchScript
    ///     dsl page (full-file load, function definitions supported; the page
    ///     must define a main() entry, its return value becomes the response
    ///     body, see DslHost.ExecuteDslFileInWorker).
    ///   - GET/HEAD on any other file: served verbatim with a MIME content
    ///     type (If-Modified-Since/304 conditional requests and single byte
    ///     ranges supported); 404 when missing.
    ///
    /// Custom response headers are configured per port in dsl as an ordered
    /// rule list (webserver_start_server / webserver_set_headers):
    ///
    ///   [{"match": "*.html", "headers": {"Cross-Origin-Opener-Policy": "same-origin"}},
    ///    {"headers": {"X-Custom": "value"}}]
    ///
    /// 'match' is a glob (* and ?) over the url path; a missing match means
    /// every path. Rules apply in order; later rules override earlier ones on
    /// the same header name. A rule may override Content-Type.
    ///
    /// Exposure: the listener binds loopback only. Public pages are kept away
    /// by LNA: their preflight carries the PNA header and this server never
    /// answers Access-Control-Allow-Private-Network, so Chromium blocks the
    /// request before it reaches us (same model as HttpProxyServer).
    /// </summary>
    public sealed class WebServer : IDisposable
    {
        private HttpListener? _listener;
        private CancellationTokenSource? _cts;
        private int _port;
        private volatile bool _isRunning;
        private long _requestCount;
        private string _root = string.Empty;
        private List<HeaderRule> _headerRules = new();
        private readonly object _rulesLock = new object();

        /// <summary>Gets whether the server is currently running.</summary>
        public bool IsRunning => _isRunning;

        /// <summary>Gets the port the server is listening on.</summary>
        public int Port => _port;

        /// <summary>Total number of accepted requests since Start.</summary>
        public long RequestCount => Interlocked.Read(ref _requestCount);

        /// <summary>One custom-response-header rule: a glob over the url path plus the headers to add.</summary>
        public sealed class HeaderRule
        {
            public string Match = "*";
            public Dictionary<string, string> Headers = new(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Parses a header rule list from json. Accepts a single rule object
        /// or an array of rule objects:
        ///   {"match": "*.html", "headers": {"Name": "value"}}
        /// Throws ArgumentException with a readable message on bad input.
        /// </summary>
        public static List<HeaderRule> ParseHeaderRules(string json)
        {
            var rules = new List<HeaderRule>();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            void ParseRule(JsonElement el)
            {
                if (el.ValueKind != JsonValueKind.Object) {
                    throw new ArgumentException("each header rule must be a json object");
                }
                var rule = new HeaderRule();
                if (el.TryGetProperty("match", out var matchEl)) {
                    if (matchEl.ValueKind != JsonValueKind.String) {
                        throw new ArgumentException("'match' must be a string glob over the url path");
                    }
                    rule.Match = matchEl.GetString() ?? "*";
                }
                if (!el.TryGetProperty("headers", out var headersEl) || headersEl.ValueKind != JsonValueKind.Object) {
                    throw new ArgumentException("header rule must contain a 'headers' object");
                }
                foreach (var prop in headersEl.EnumerateObject()) {
                    if (prop.Value.ValueKind != JsonValueKind.String) {
                        throw new ArgumentException($"header value for '{prop.Name}' must be a string");
                    }
                    rule.Headers[prop.Name] = prop.Value.GetString() ?? string.Empty;
                }
                rules.Add(rule);
            }
            if (root.ValueKind == JsonValueKind.Array) {
                foreach (var el in root.EnumerateArray()) {
                    ParseRule(el);
                }
            }
            else {
                ParseRule(root);
            }
            if (rules.Count == 0) {
                throw new ArgumentException("header rules json is empty");
            }
            return rules;
        }

        /// <summary>Replaces the custom header rules (applied to every request from now on).</summary>
        public void SetHeaderRules(List<HeaderRule> rules)
        {
            lock (_rulesLock) {
                _headerRules = rules;
            }
        }

        /// <summary>
        /// Starts the server on the specified port (loopback only) serving
        /// the given document root. useHttps requires the certificate to be
        /// bound to the port beforehand (netsh http add sslcert, see
        /// ssl_selfsign_setup.ps1).
        /// </summary>
        public bool Start(int port, string root, bool useHttps)
        {
            if (_isRunning) {
                AgentCore.Instance.Logger.Warning("Web server is already running");
                return false;
            }
            if (useHttps && MetaDslExecutor.IsMac) {
                // HttpListener's managed implementation on macOS has no HTTPS
                // support; use a system reverse proxy (caddy/nginx) instead.
                AgentCore.Instance.Logger.Error("webserver https is Windows only (HttpListener on macOS has no HTTPS support)");
                return false;
            }
            string rootPath = Path.GetFullPath(root);
            if (!Directory.Exists(rootPath)) {
                AgentCore.Instance.Logger.Error($"Web server root does not exist: {rootPath}");
                return false;
            }
            try {
                _port = port;
                _root = rootPath;
                string scheme = useHttps ? "https" : "http";
                _listener = new HttpListener();
                _listener.Prefixes.Add($"{scheme}://localhost:{port}/");
                _listener.Prefixes.Add($"{scheme}://127.0.0.1:{port}/");
                _listener.Start();
                _isRunning = true;
                _cts = new CancellationTokenSource();
                _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
                AgentCore.Instance.Logger.Info($"Web server started on port {port} (root: {rootPath}, scheme: {scheme})");
                if (useHttps) {
                    AgentCore.Instance.Logger.Info("Web server https note: the certificate must be bound to the port with 'netsh http add sslcert' (see ssl_selfsign_setup.ps1)");
                }
                return true;
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error($"Failed to start web server: {ex.Message}\nStack: {ex.StackTrace}");
                _isRunning = false;
                return false;
            }
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) {
                return;
            }
            try {
                _isRunning = false;
                _cts?.Cancel();
                _listener?.Stop();
                _listener?.Close();
                AgentCore.Instance.Logger.Info("Web server stopped");
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error($"Error stopping web server: {ex.Message}\nStack: {ex.StackTrace}");
            }
        }

        private async Task AcceptLoopAsync(CancellationToken ct)
        {
            var listener = _listener!;
            while (_isRunning && !ct.IsCancellationRequested) {
                try {
                    if (!listener.IsListening) {
                        break;
                    }
                    var context = await listener.GetContextAsync();
                    Interlocked.Increment(ref _requestCount);
                    _ = Task.Run(() => HandleAsync(context));
                }
                catch (HttpListenerException) when (ct.IsCancellationRequested) {
                    break;
                }
                catch (Exception ex) {
                    AgentCore.Instance.Logger.Error($"Web server accept error: {ex.Message}\nStack: {ex.StackTrace}");
                }
            }
        }

        private async Task HandleAsync(HttpListenerContext context)
        {
            HttpListenerRequest req = context.Request;
            HttpListenerResponse resp = context.Response;
            string? origin = req.Headers["Origin"];
            try {
                if (req.HttpMethod == "OPTIONS") {
                    // CORS preflight (local page -> local server: no PNA
                    // involved; public pages are blocked by LNA before us).
                    resp.StatusCode = 204;
                    resp.AddHeader("Access-Control-Allow-Origin", string.IsNullOrEmpty(origin) ? "*" : origin);
                    resp.AddHeader("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
                    string? reqHeaders = req.Headers["Access-Control-Request-Headers"];
                    resp.AddHeader("Access-Control-Allow-Headers", string.IsNullOrEmpty(reqHeaders) ? "*" : reqHeaders);
                    resp.AddHeader("Access-Control-Max-Age", "86400");
                    resp.Close();
                    return;
                }
                if (req.HttpMethod != "GET" && req.HttpMethod != "HEAD") {
                    resp.StatusCode = 405;
                    resp.Close();
                    return;
                }

                string urlPath = Uri.UnescapeDataString(req.Url?.AbsolutePath ?? "/");
                // Map the url path under the root, rejecting traversal: the
                // containment check below is the real guard (GetFullPath
                // resolves any ".." before it).
                string rel = urlPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                string rootFull = _root;
                if (!rootFull.EndsWith(Path.DirectorySeparatorChar)) {
                    rootFull += Path.DirectorySeparatorChar;
                }
                string fullPath = Path.GetFullPath(Path.Combine(_root, rel));
                // Windows file systems are case insensitive; on case sensitive
                // systems (macOS/Linux) compare case sensitively so a same
                // spelled but differently cased sibling dir cannot sneak in.
                var pathComparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                if (!fullPath.StartsWith(rootFull, pathComparison)) {
                    resp.StatusCode = 403;
                    resp.Close();
                    return;
                }
                if (Directory.Exists(fullPath)) {
                    fullPath = ResolveDefaultDocument(fullPath);
                }

                // Default CORS echo, unless a matching rule provides its own
                // Access-Control-Allow-Origin (avoids duplicate headers).
                var customHeaders = CollectMatchingHeaders(urlPath);
                bool hasAcao = false;
                foreach (var name in customHeaders.Keys) {
                    if (name.Equals("Access-Control-Allow-Origin", StringComparison.OrdinalIgnoreCase)) {
                        hasAcao = true;
                        break;
                    }
                }
                if (!hasAcao) {
                    resp.AddHeader("Access-Control-Allow-Origin", string.IsNullOrEmpty(origin) ? "*" : origin);
                }
                foreach (var kv in customHeaders) {
                    if (kv.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)) {
                        resp.ContentType = kv.Value;
                    }
                    else {
                        try { resp.AddHeader(kv.Key, kv.Value); } catch { }
                    }
                }

                if (!File.Exists(fullPath)) {
                    resp.StatusCode = 404;
                    resp.ContentType = "text/plain; charset=utf-8";
                    byte[] notFound = Encoding.UTF8.GetBytes("404 not found: " + urlPath);
                    resp.ContentLength64 = notFound.Length;
                    if (req.HttpMethod != "HEAD") {
                        await resp.OutputStream.WriteAsync(notFound, 0, notFound.Length);
                    }
                    resp.Close();
                    return;
                }

                if (fullPath.EndsWith(".dsl", StringComparison.OrdinalIgnoreCase)) {
                    // Dsl page: full-file BatchScript execution (function
                    // definitions supported, main() entry), isolated on a
                    // dedicated interpreter thread
                    // (DslHost.ExecuteDslFileInWorker).
                    bool hasError;
                    string result = MetaDslExecutor.ExecuteDslFileInWorker(fullPath, out hasError);
                    byte[] data = Encoding.UTF8.GetBytes(result);
                    resp.StatusCode = 200;
                    if (string.IsNullOrEmpty(resp.ContentType)) {
                        resp.ContentType = "text/plain; charset=utf-8";
                    }
                    resp.ContentLength64 = data.Length;
                    if (req.HttpMethod != "HEAD") {
                        await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    }
                    resp.Close();
                    return;
                }

                // Plain static file: conditional requests + byte ranges.
                var fileInfo = new FileInfo(fullPath);
                long fileLength = fileInfo.Length;
                DateTime lastModifiedUtc = new DateTime(
                    fileInfo.LastWriteTimeUtc.Ticks - fileInfo.LastWriteTimeUtc.Ticks % TimeSpan.TicksPerSecond,
                    DateTimeKind.Utc);
                resp.AddHeader("Last-Modified", lastModifiedUtc.ToString("R"));
                string? ifModifiedSince = req.Headers["If-Modified-Since"];
                if (!string.IsNullOrEmpty(ifModifiedSince) &&
                    DateTime.TryParseExact(ifModifiedSince.Trim(), "R", CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime sinceUtc) &&
                    lastModifiedUtc <= sinceUtc) {
                    // Not modified since the client's cached copy.
                    resp.StatusCode = 304;
                    resp.Close();
                    return;
                }
                long rangeStart = 0, rangeEnd = fileLength - 1;
                bool unsatisfiable = false;
                bool isRange = req.HttpMethod == "GET" &&
                    TryParseSingleByteRange(req.Headers["Range"] ?? string.Empty, fileLength,
                        out rangeStart, out rangeEnd, out unsatisfiable);
                if (unsatisfiable) {
                    resp.StatusCode = 416;
                    resp.AddHeader("Content-Range", $"bytes */{fileLength}");
                    resp.Close();
                    return;
                }
                resp.StatusCode = 200;
                if (string.IsNullOrEmpty(resp.ContentType)) {
                    resp.ContentType = GetContentType(fullPath);
                }
                if (isRange) {
                    resp.StatusCode = 206;
                    resp.AddHeader("Content-Range", $"bytes {rangeStart}-{rangeEnd}/{fileLength}");
                    resp.ContentLength64 = rangeEnd - rangeStart + 1;
                }
                else {
                    resp.AddHeader("Accept-Ranges", "bytes");
                    resp.ContentLength64 = fileLength;
                }
                if (req.HttpMethod != "HEAD") {
                    using (var fs = File.OpenRead(fullPath)) {
                        if (isRange) {
                            fs.Seek(rangeStart, SeekOrigin.Begin);
                            long remaining = rangeEnd - rangeStart + 1;
                            var buffer = new byte[16 * 1024];
                            while (remaining > 0) {
                                int read = await fs.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, remaining));
                                if (read <= 0) {
                                    break;
                                }
                                await resp.OutputStream.WriteAsync(buffer, 0, read);
                                remaining -= read;
                            }
                        }
                        else {
                            await fs.CopyToAsync(resp.OutputStream);
                        }
                    }
                }
                resp.Close();
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error($"Web server error: {ex.Message}\nStack: {ex.StackTrace}");
                try {
                    resp.StatusCode = 500;
                    resp.ContentType = "text/plain; charset=utf-8";
                    byte[] err = Encoding.UTF8.GetBytes("web server error: " + ex.Message);
                    resp.ContentLength64 = err.Length;
                    resp.OutputStream.Write(err, 0, err.Length);
                }
                catch { }
            }
            finally {
                try { resp.Close(); } catch { }
            }
        }

        // Merges the headers of every rule matching the url path, in rule
        // order (later rules override earlier ones on the same name).
        private Dictionary<string, string> CollectMatchingHeaders(string urlPath)
        {
            var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            List<HeaderRule> rules;
            lock (_rulesLock) {
                rules = _headerRules;
            }
            foreach (var rule in rules) {
                if (GlobMatch(rule.Match, urlPath)) {
                    foreach (var kv in rule.Headers) {
                        merged[kv.Key] = kv.Value;
                    }
                }
            }
            return merged;
        }

        // Parses a single byte range ("bytes=start-end", "bytes=start-",
        // "bytes=-suffix"). Returns true when a satisfiable range was parsed
        // (start/end set, end clamped to the last byte); returns false with
        // unsatisfiable=true for a well formed but unsatisfiable range (416);
        // returns false with unsatisfiable=false when the header must be
        // ignored (absent, malformed or multi-range - RFC 9110 allows serving
        // the full body then).
        private static bool TryParseSingleByteRange(string header, long length, out long start, out long end, out bool unsatisfiable)
        {
            start = 0;
            end = 0;
            unsatisfiable = false;
            header = header.Trim();
            if (header.Length == 0 || !header.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase)) {
                return false;
            }
            string spec = header.Substring(6).Trim();
            if (spec.IndexOf(',') >= 0) {
                return false;  // multi-range: serve the full body
            }
            int dash = spec.IndexOf('-');
            if (dash < 0) {
                return false;
            }
            string first = spec.Substring(0, dash).Trim();
            string last = spec.Substring(dash + 1).Trim();
            if (first.Length == 0) {
                // Suffix range: the last N bytes.
                if (last.Length == 0 || !long.TryParse(last, out long n)) {
                    return false;
                }
                if (n <= 0) {
                    unsatisfiable = true;
                    return false;
                }
                start = Math.Max(0, length - n);
                end = length - 1;
            }
            else {
                if (!long.TryParse(first, out start) || start < 0) {
                    return false;
                }
                if (last.Length == 0) {
                    end = length - 1;
                }
                else if (!long.TryParse(last, out end) || end < start) {
                    return false;
                }
            }
            if (length == 0 || start >= length) {
                unsatisfiable = true;
                return false;
            }
            end = Math.Min(end, length - 1);
            return true;
        }

        // Directory default documents, tried in order.
        private static readonly string[] s_DefaultDocuments = { "index.html", "index.htm" };

        private static string ResolveDefaultDocument(string dir)
        {
            foreach (string name in s_DefaultDocuments) {
                string candidate = Path.Combine(dir, name);
                if (File.Exists(candidate)) {
                    return candidate;
                }
            }
            return Path.Combine(dir, s_DefaultDocuments[0]);
        }

        // Simple glob matcher ('*' = any run, '?' = any single char) over the
        // whole url path, case insensitive. Compiled regexes are cached per
        // pattern (rules are matched on every request).
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Regex> s_GlobCache = new();

        private static bool GlobMatch(string pattern, string path)
        {
            if (string.IsNullOrEmpty(pattern) || pattern == "*") {
                return true;
            }
            Regex regex = s_GlobCache.GetOrAdd(pattern, p => {
                var sb = new StringBuilder("^");
                foreach (char c in p) {
                    if (c == '*') {
                        sb.Append(".*");
                    }
                    else if (c == '?') {
                        sb.Append('.');
                    }
                    else {
                        sb.Append(Regex.Escape(c.ToString()));
                    }
                }
                sb.Append('$');
                return new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            });
            return regex.IsMatch(path);
        }

        // Process-wide MIME map shared by every WebServer instance; the
        // initial table covers the IANA-registered and de-facto standard
        // web types, dsl can extend or override entries with
        // webserver_set_mime (empty content type removes the entry).
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> s_MimeTypes = new(new Dictionary<string, string> {
            // text
            {".html", "text/html; charset=utf-8"},
            {".htm", "text/html; charset=utf-8"},
            {".xhtml", "application/xhtml+xml"},
            {".css", "text/css; charset=utf-8"},
            {".js", "text/javascript; charset=utf-8"},
            {".mjs", "text/javascript; charset=utf-8"},
            {".txt", "text/plain; charset=utf-8"},
            {".csv", "text/csv; charset=utf-8"},
            {".md", "text/markdown; charset=utf-8"},
            {".xml", "application/xml; charset=utf-8"},
            {".xsl", "application/xml; charset=utf-8"},
            {".ics", "text/calendar; charset=utf-8"},
            {".vcf", "text/vcard; charset=utf-8"},
            {".rtf", "application/rtf"},
            // json / structured
            {".json", "application/json; charset=utf-8"},
            {".map", "application/json; charset=utf-8"},
            {".jsonld", "application/ld+json"},
            {".ndjson", "application/x-ndjson"},
            {".webmanifest", "application/manifest+json"},
            {".yaml", "application/yaml; charset=utf-8"},
            {".yml", "application/yaml; charset=utf-8"},
            {".toml", "application/toml; charset=utf-8"},
            // feeds
            {".atom", "application/atom+xml; charset=utf-8"},
            {".rss", "application/rss+xml; charset=utf-8"},
            // images
            {".png", "image/png"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".gif", "image/gif"},
            {".svg", "image/svg+xml"},
            {".svgz", "image/svg+xml"},
            {".webp", "image/webp"},
            {".avif", "image/avif"},
            {".bmp", "image/bmp"},
            {".tiff", "image/tiff"},
            {".tif", "image/tiff"},
            {".heic", "image/heic"},
            {".ico", "image/x-icon"},
            // fonts
            {".woff", "font/woff"},
            {".woff2", "font/woff2"},
            {".ttf", "font/ttf"},
            {".otf", "font/otf"},
            {".eot", "application/vnd.ms-fontobject"},
            // audio
            {".mp3", "audio/mpeg"},
            {".wav", "audio/wav"},
            {".aac", "audio/aac"},
            {".flac", "audio/flac"},
            {".ogg", "audio/ogg"},
            {".oga", "audio/ogg"},
            {".opus", "audio/opus"},
            {".m4a", "audio/mp4"},
            {".mid", "audio/midi"},
            {".midi", "audio/midi"},
            {".m3u", "audio/mpegurl"},
            {".m3u8", "application/vnd.apple.mpegurl"},
            {".mpd", "application/dash+xml"},
            // video
            {".mp4", "video/mp4"},
            {".webm", "video/webm"},
            {".mov", "video/quicktime"},
            {".avi", "video/x-msvideo"},
            {".mpeg", "video/mpeg"},
            {".mpg", "video/mpeg"},
            {".ts", "video/mp2t"},
            // documents
            {".pdf", "application/pdf"},
            {".doc", "application/msword"},
            {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {".xls", "application/vnd.ms-excel"},
            {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            {".ppt", "application/vnd.ms-powerpoint"},
            {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
            // archives / packages
            {".zip", "application/zip"},
            {".gz", "application/gzip"},
            {".tar", "application/x-tar"},
            {".7z", "application/x-7z-compressed"},
            {".rar", "application/vnd.rar"},
            {".bz2", "application/x-bzip2"},
            {".xz", "application/x-xz"},
            {".jar", "application/java-archive"},
            {".apk", "application/vnd.android.package-archive"},
            {".wasm", "application/wasm"},
        });

        /// <summary>
        /// Adds or overrides a MIME mapping (process wide, every web server
        /// instance). The extension is normalized to a lowercase ".ext" form;
        /// an empty contentType removes the entry.
        /// </summary>
        public static void SetMimeType(string ext, string contentType)
        {
            string e = (ext ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(e)) {
                return;
            }
            if (!e.StartsWith('.')) {
                e = "." + e;
            }
            if (string.IsNullOrEmpty(contentType)) {
                s_MimeTypes.TryRemove(e, out _);
            }
            else {
                s_MimeTypes[e] = contentType;
            }
        }

        private static string GetContentType(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return s_MimeTypes.TryGetValue(ext, out string? type) ? type : "application/octet-stream";
        }

        /// <summary>
        /// Disposes the server resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }
}
