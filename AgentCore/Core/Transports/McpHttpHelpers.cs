using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AgentCore.Core
{
    /// <summary>
    /// Shared helpers for MCP HTTP transports.
    /// </summary>
    internal static class McpHttpHelpers
    {
        /// <summary>
        /// Extracts the JSON payload from an HTTP response body: parses SSE
        /// frames (aggregating data: lines) when the content type is
        /// text/event-stream, otherwise returns the body as-is.
        /// </summary>
        public static string ExtractSseJson(HttpResponseMessage response, string body)
        {
            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (!string.Equals(mediaType, "text/event-stream", StringComparison.OrdinalIgnoreCase))
                return body;
            // Parse SSE frames: aggregate all lines starting with "data:" (strip prefix, join by newline)
            var sb = new StringBuilder();
            using var reader = new System.IO.StringReader(body);
            string? line;
            while ((line = reader.ReadLine()) != null) {
                if (line.StartsWith("data:", StringComparison.Ordinal)) {
                    string payload = line.Substring(5);
                    if (payload.StartsWith(" ", StringComparison.Ordinal))
                        payload = payload.Substring(1);
                    if (sb.Length > 0) sb.Append('\n');
                    sb.Append(payload);
                }
            }
            return sb.Length > 0 ? sb.ToString() : body;
        }

        /// <summary>
        /// Encodes a value for the Mcp-Name header (2026-07-28): plain ASCII
        /// when header-safe, otherwise the base64 sentinel form
        /// "=?base64?{Base64Utf8}?=" (values already matching the sentinel
        /// pattern are themselves encoded).
        /// </summary>
        public static string EncodeMcpName(string value)
        {
            bool asciiSafe = !value.StartsWith("=?base64?", StringComparison.Ordinal);
            if (asciiSafe) {
                foreach (char c in value) {
                    if (c < 0x21 || c > 0x7E) {
                        asciiSafe = false;
                        break;
                    }
                }
            }
            if (asciiSafe)
                return value;
            return "=?base64?" + Convert.ToBase64String(Encoding.UTF8.GetBytes(value)) + "?=";
        }

        /// <summary>
        /// Reads the method and the name/uri parameter from a JSON-RPC request
        /// body, for mirroring into the Mcp-Method / Mcp-Name headers.
        /// </summary>
        public static void ReadMethodAndName(string json, out string? method, out string? name)
        {
            method = null;
            name = null;
            try {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                    return;
                if (root.TryGetProperty("method", out var m) && m.ValueKind == JsonValueKind.String)
                    method = m.GetString();
                if (root.TryGetProperty("params", out var prm) && prm.ValueKind == JsonValueKind.Object) {
                    if (prm.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String)
                        name = n.GetString();
                    else if (prm.TryGetProperty("uri", out var u) && u.ValueKind == JsonValueKind.String)
                        name = u.GetString();
                }
            }
            catch {
                // Header mirroring must not change transport behavior.
            }
        }
    }
}
