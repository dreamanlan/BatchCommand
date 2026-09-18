using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AgentCore.Core
{
    /// <summary>
    /// Url matching filter set shared by HttpProxyServer and WebServer
    /// (see httpproxy_add_filter / webserver_add_filter).
    ///
    /// A filter = common url patterns (AND among them) plus an optional
    /// list of field clauses (OR between clauses, AND between the fields of
    /// one clause) - disjunctive normal form. Filters themselves are OR-ed:
    /// the first matching filter wins and its id is handed to the dsl
    /// callback.
    ///
    /// Pattern syntax: a pattern built only from plain characters
    /// (letters, digits and _ . : / ~ % + -) is matched as a case
    /// insensitive substring (the common fast path); anything else, or the
    /// "re:" prefix, is compiled as a case insensitive regex (compiled
    /// regexes are cached per pattern, like WebServer.GlobMatch).
    ///
    /// Clause fields: "host" (url host), "path" (url path), "method"
    /// (request method), "type" (response content type, response filters
    /// only). fields_json is a json object (one clause, AND of its fields)
    /// or an array of objects (OR between clauses).
    /// </summary>
    public sealed class UrlFilterEngine
    {
        public enum FilterDirection
        {
            Request = 1,
            Response = 2,
        }

        public sealed class Filter
        {
            public int Id;
            public FilterDirection Direction;
            public List<string> UrlPatterns = new();
            public List<Dictionary<string, string>> FieldClauses = new();
        }

        private static readonly HashSet<string> s_KnownFields = new(StringComparer.OrdinalIgnoreCase) {
            "host", "path", "method", "type",
        };

        private readonly List<Filter> _filters = new();
        private readonly object _lock = new object();
        private int _nextId;

        /// <summary>Gets whether no filter is registered (fast path for the servers).</summary>
        public bool IsEmpty
        {
            get { lock (_lock) { return _filters.Count == 0; } }
        }

        /// <summary>
        /// Parses a fields table from json: a single object (one clause, AND
        /// of its fields) or an array of objects (OR between clauses).
        /// Returns false with a readable message on bad input.
        /// </summary>
        public static bool TryParseFieldClauses(string json, out List<Dictionary<string, string>> clauses, out string error)
        {
            var parsed = new List<Dictionary<string, string>>();
            clauses = parsed;
            error = string.Empty;
            try {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                void ParseClause(JsonElement el)
                {
                    if (el.ValueKind != JsonValueKind.Object) {
                        throw new ArgumentException("each fields entry must be a json object");
                    }
                    var clause = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var prop in el.EnumerateObject()) {
                        if (!s_KnownFields.Contains(prop.Name)) {
                            throw new ArgumentException($"unknown filter field '{prop.Name}' (expected host/path/method/type)");
                        }
                        string? value = prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() : null;
                        if (string.IsNullOrEmpty(value)) {
                            throw new ArgumentException($"filter field '{prop.Name}' must be a non-empty string");
                        }
                        clause[prop.Name.ToLowerInvariant()] = value;
                    }
                    if (clause.Count > 0) {
                        parsed.Add(clause);
                    }
                }
                if (root.ValueKind == JsonValueKind.Array) {
                    foreach (var el in root.EnumerateArray()) {
                        ParseClause(el);
                    }
                }
                else {
                    ParseClause(root);
                }
                if (clauses.Count == 0) {
                    throw new ArgumentException("fields json is empty");
                }
                return true;
            }
            catch (Exception ex) {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Registers a filter (url patterns AND field clauses as parsed
        /// above) and returns its id (per engine, increasing).
        /// </summary>
        public int AddFilter(FilterDirection direction, List<string> urlPatterns, List<Dictionary<string, string>> fieldClauses)
        {
            lock (_lock) {
                int id = ++_nextId;
                _filters.Add(new Filter {
                    Id = id,
                    Direction = direction,
                    UrlPatterns = urlPatterns,
                    FieldClauses = fieldClauses,
                });
                return id;
            }
        }

        /// <summary>Removes the filter with the given id; true when found.</summary>
        public bool RemoveFilter(int id)
        {
            lock (_lock) {
                return _filters.RemoveAll(f => f.Id == id) > 0;
            }
        }

        /// <summary>Removes every filter.</summary>
        public void ClearFilters()
        {
            lock (_lock) {
                _filters.Clear();
            }
        }

        /// <summary>
        /// Finds the first request-direction filter matching the url
        /// (absolute target url for the proxy, request path for the web
        /// server) and method, or null.
        /// </summary>
        public Filter? MatchRequest(string url, string method)
        {
            return Match(FilterDirection.Request, url, method, null);
        }

        /// <summary>
        /// Finds the first response-direction filter matching the url,
        /// method and response content type, or null.
        /// </summary>
        public Filter? MatchResponse(string url, string method, string? contentType)
        {
            return Match(FilterDirection.Response, url, method, contentType);
        }

        private Filter? Match(FilterDirection direction, string url, string method, string? contentType)
        {
            Filter[] snapshot;
            lock (_lock) {
                if (_filters.Count == 0) {
                    return null;
                }
                snapshot = _filters.ToArray();
            }
            string host, path;
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri)) {
                host = uri.Host;
                path = uri.AbsolutePath;
            }
            else {
                host = string.Empty;
                path = url;
            }
            foreach (var f in snapshot) {
                if (f.Direction != direction) {
                    continue;
                }
                if (!UrlPatternsMatch(f, url)) {
                    continue;
                }
                if (FieldClausesMatch(f, host, path, method, contentType)) {
                    return f;
                }
            }
            return null;
        }

        private static bool UrlPatternsMatch(Filter f, string url)
        {
            foreach (var p in f.UrlPatterns) {
                if (!PatternMatch(p, url)) {
                    return false;
                }
            }
            return true;
        }

        private static bool FieldClausesMatch(Filter f, string host, string path, string method, string? contentType)
        {
            if (f.FieldClauses.Count == 0) {
                return true;
            }
            foreach (var clause in f.FieldClauses) {
                bool all = true;
                foreach (var kv in clause) {
                    string value = kv.Key switch {
                        "host" => host,
                        "path" => path,
                        "method" => method,
                        "type" => contentType ?? string.Empty,
                        _ => string.Empty,
                    };
                    if (!PatternMatch(kv.Value, value)) {
                        all = false;
                        break;
                    }
                }
                if (all) {
                    return true;
                }
            }
            return false;
        }

        // ---- pattern matching ----------------------------------------------

        private static readonly ConcurrentDictionary<string, Regex?> s_RegexCache = new();

        // Plain characters only (no regex metacharacter can appear): the
        // pattern degrades to a case insensitive substring match.
        private static bool IsPlainLiteral(string pattern)
        {
            if (pattern.Length == 0) {
                return false;
            }
            foreach (char c in pattern) {
                bool ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')
                    || c == '_' || c == '.' || c == ':' || c == '/' || c == '~' || c == '%' || c == '+' || c == '-';
                if (!ok) {
                    return false;
                }
            }
            return true;
        }

        private static Regex? CompileRegex(string pattern)
        {
            string p = pattern.StartsWith("re:", StringComparison.OrdinalIgnoreCase) ? pattern.Substring(3) : pattern;
            try {
                return new Regex(p, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            }
            catch {
                return null;  // invalid pattern: never matches
            }
        }

        private static bool PatternMatch(string pattern, string value)
        {
            if (pattern.StartsWith("re:", StringComparison.OrdinalIgnoreCase)) {
                Regex? re = s_RegexCache.GetOrAdd(pattern, CompileRegex);
                return re != null && re.IsMatch(value);
            }
            if (IsPlainLiteral(pattern)) {
                return value.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            Regex? re2 = s_RegexCache.GetOrAdd(pattern, CompileRegex);
            return re2 != null && re2.IsMatch(value);
        }
    }
}
