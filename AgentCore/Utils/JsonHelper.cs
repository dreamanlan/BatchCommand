using System;
using LitJson;

namespace CefDotnetApp.AgentCore.Utils
{
    public static class JsonHelper
    {
        public static string ToJson(object obj, bool prettyPrint = false)
        {
            if (obj == null)
                return "null";

            JsonWriter writer = new JsonWriter();
            writer.PrettyPrint = prettyPrint;
            JsonMapper.ToJson(obj, writer);
            return writer.ToString();
        }

        public static T? FromJson<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json))
                return default;

            return JsonMapper.ToObject<T>(json);
        }

        public static object? FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonMapper.ToObject(json);
        }

        public static bool TryParseJson(string json, out JsonData? result)
        {
            result = default;
            if (string.IsNullOrEmpty(json))
                return false;

            try {
                result = JsonMapper.ToObject(json);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public static bool IsValidJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return false;

            try {
                JsonMapper.ToObject(json);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public static string EscapeJsonString(string s, bool addQuotes = false)
        {
            if (s == null)
                return addQuotes ? "null" : string.Empty;

            var sb = new System.Text.StringBuilder(s.Length + 2);
            if (addQuotes)
                sb.Append('"');
            for (int i = 0; i < s.Length; i++) {
                char c = s[i];
                switch (c) {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20 || c == 0x7F) {
                            sb.Append("\\u");
                            sb.Append(((int)c).ToString("x4"));
                        }
                        else {
                            sb.Append(c);
                        }
                        break;
                }
            }
            if (addQuotes)
                sb.Append('"');
            return sb.ToString();
        }

        public static string UnescapeJsonString(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s ?? string.Empty;

            int start = 0;
            int end = s.Length;
            if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"') {
                start = 1;
                end = s.Length - 1;
            }

            var sb = new System.Text.StringBuilder(end - start);
            for (int i = start; i < end; i++) {
                char c = s[i];
                if (c != '\\') {
                    sb.Append(c);
                    continue;
                }
                if (i + 1 >= end)
                    throw new FormatException("Invalid escape at end of string");
                char esc = s[++i];
                switch (esc) {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (i + 4 >= end)
                            throw new FormatException("Invalid \\u escape: not enough chars");
                        string hex = s.Substring(i + 1, 4);
                        sb.Append((char)Convert.ToInt32(hex, 16));
                        i += 4;
                        break;
                    default:
                        throw new FormatException("Unknown escape: \\" + esc);
                }
            }
            return sb.ToString();
        }

    }
}
