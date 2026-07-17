using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;
using CefDotnetApp.AgentCore.Models;
using CefDotnetApp.AgentCore.Utils;
using System.Text;
using System.Collections;
using J2N;
using J2N.Collections.Generic.Extensions;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // Clipboard Operations
    sealed class GetClipboardExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                string text = Core.AgentCore.Instance.ClipboardOps.GetText();
                return BoxedValue.FromString(text);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"getclipboard error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    sealed class SetClipboardExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: set_clipboard(text)");
                return BoxedValue.From(false);
            }

            {
                try {
                    string text = operands[0].AsString;
                    bool result = Core.AgentCore.Instance.ClipboardOps.SetText(text);
                    return BoxedValue.From(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"setclipboard error: {ex.Message}");
                }
            }
            return BoxedValue.From(false);
        }
    }

    // Logging Operations
    sealed class LogInfoExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: log_info(fmt, ...)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string fmt = operands[0].AsString;
                    var args = new object[operands.Count - 1];
                    for (int i = 1; i < operands.Count; i++) {
                        args[i - 1] = operands[i].GetObject();
                    }
                    Core.AgentCore.Instance.Logger.Info(fmt, args);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"loginfo error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class LogErrorExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: log_error(fmt, ...)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string fmt = operands[0].AsString;
                    var args = new object[operands.Count - 1];
                    for (int i = 1; i < operands.Count; i++) {
                        args[i - 1] = operands[i].GetObject();
                    }
                    Core.AgentCore.Instance.Logger.Error(fmt, args);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"logerror error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class LogWarningExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: log_warning(fmt, ...)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string fmt = operands[0].AsString;
                    var args = new object[operands.Count - 1];
                    for (int i = 1; i < operands.Count; i++) {
                        args[i - 1] = operands[i].GetObject();
                    }
                    Core.AgentCore.Instance.Logger.Warning(fmt, args);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"logwarning error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    // HTTP Operations
    static class HttpHeaderHelper
    {
        public static Dictionary<string, string>? ExtractStringHeaders(BoxedValue bv)
        {
            var obj = bv.GetObject();
            if (obj is IDictionary<BoxedValue, BoxedValue> bvdict) {
                var d = new Dictionary<string, string>();
                foreach (var kv in bvdict) {
                    d[kv.Key.ToString()] = kv.Value.ToString();
                }
                return d.Count > 0 ? d : null;
            }
            if (obj is IDictionary<string, object?> sdict) {
                var d = new Dictionary<string, string>();
                foreach (var kv in sdict) {
                    d[kv.Key] = kv.Value?.ToString() ?? string.Empty;
                }
                return d.Count > 0 ? d : null;
            }
            return null;
        }
    }

    sealed class HttpGetExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: http_get(url) or http_get(url, headers)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string url = operands[0].AsString;
                    Dictionary<string, string>? headers = null;
                    if (operands.Count > 1) {
                        headers = HttpHeaderHelper.ExtractStringHeaders(operands[1]);
                        if (headers == null) {
                            AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: http_get(url, headers), headers must be hashtable");
                            return BoxedValue.NullObject;
                        }
                    }
                    string result = Core.AgentCore.Instance.HttpClient.Get(url, headers);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"httpget error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class HttpPostExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: http_post(url, content) or http_post(url, content, contentType) or http_post(url, content, contentType, headers)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string url = operands[0].AsString;
                    string content = operands[1].AsString;
                    string contentType = "application/json";
                    if (operands.Count > 2) {
                        if (operands[2].IsString) {
                            contentType = operands[2].ToString();
                        }
                        else {
                            AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: http_post(url, content, contentType) or http_post(url, content, contentType, headers), contentType must be string");
                            return BoxedValue.NullObject;
                        }
                    }
                    Dictionary<string, string>? headers = null;
                    if (operands.Count > 3) {
                        headers = HttpHeaderHelper.ExtractStringHeaders(operands[3]);
                        if (headers == null) {
                            AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: http_post(url, content, contentType, headers), headers must be hashtable");
                            return BoxedValue.NullObject;
                        }
                    }
                    string result = Core.AgentCore.Instance.HttpClient.Post(url, content, contentType, headers);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"httppost error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class DownloadFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: download_file(url, savePath) or download_file(url, savePath, headers)");
                return BoxedValue.From(false);
            }

            {
                try {
                    string url = operands[0].AsString;
                    string savePath = operands[1].AsString;
                    Dictionary<string, string>? headers = null;
                    if (operands.Count > 2) {
                        headers = HttpHeaderHelper.ExtractStringHeaders(operands[2]);
                        if (headers == null) {
                            AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: download_file(url, savePath, headers), headers must be hashtable");
                            return BoxedValue.NullObject;
                        }
                    }
                    bool result = Core.AgentCore.Instance.HttpClient.DownloadFile(url, savePath, headers);
                    return BoxedValue.From(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"downloadfile error: {ex.Message}");
                }
            }
            return BoxedValue.From(false);
        }
    }

    sealed class SetHttpUserAgentExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: set_http_user_agent(user_agent)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string ua = operands[0].AsString;
            Core.AgentCore.Instance.HttpClient.SetUserAgent(ua);
            return BoxedValue.FromString("ok");
        }
    }

    sealed class GetHttpUserAgentExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromString(Core.AgentCore.Instance.HttpClient.GetUserAgent());
        }
    }

    // JSON Operations
    sealed class ToJsonExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: to_json(obj, prettyPrint)");
                return BoxedValue.FromString("null");
            }

            {
                try {
                    object? obj = DslHelper.GetValueFromBoxedValue(operands[0]);
                    if (null == obj) {
                        return BoxedValue.FromString("null");
                    }
                    bool prettyPrint = operands.Count > 1 ? operands[1].GetBool() : false;
                    string json = JsonHelper.ToJson(obj, prettyPrint);
                    return BoxedValue.FromString(json);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"tojson error: {ex.Message}");
                }
            }
            return BoxedValue.FromString("null");
        }
    }

    sealed class FromJsonExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: from_json(json)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string json = operands[0].AsString;
                    object? obj = JsonHelper.FromJson(json);
                    return DslHelper.GetBoxedValueFromValue(obj);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"fromjson error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class JsonEscapeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: json_escape(str[, bool_add_quotes])");
                return BoxedValue.FromString(string.Empty);
            }

            {
                try {
                    string s = operands[0].AsString ?? string.Empty;
                    bool addQuotes = operands.Count > 1 ? operands[1].GetBool() : false;
                    string esc = JsonHelper.EscapeJsonString(s, addQuotes);
                    return BoxedValue.FromString(esc);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"json_escape error: {ex.Message}");
                }
            }
            return BoxedValue.FromString(string.Empty);
        }
    }

    sealed class JsonUnescapeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: json_unescape(str)");
                return BoxedValue.FromString(string.Empty);
            }

            {
                try {
                    string s = operands[0].AsString ?? string.Empty;
                    string un = JsonHelper.UnescapeJsonString(s);
                    return BoxedValue.FromString(un);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"json_unescape error: {ex.Message}");
                }
            }
            return BoxedValue.FromString(string.Empty);
        }
    }


    sealed class NewObjectExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                // Create a dictionary to hold the key-value pairs
                var dict = new Dictionary<string, object?>();

                // Parse key-value pairs from operands
                // Format: key1, value1, key2, value2, ...
                for (int i = 0; i + 1 < operands.Count; i += 2) {
                    string key = operands[i].AsString;
                    object? value = DslHelper.GetValueFromBoxedValue(operands[i + 1]);
                    dict[key] = value;
                }

                return BoxedValue.FromObject(dict);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"newobject error: {ex.Message}");
                return BoxedValue.FromObject(new Dictionary<string, object>());
            }
        }
    }

    sealed class ToStringExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: to_string(val)");
                return (BoxedValue)string.Empty;
            }

            {
                try {
                    var v = operands[0];
                    var sb = new StringBuilder();
                    DslHelper.ConvertToString(v, sb, 0, true);
                    return sb.ToString();
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"fromjson error: {ex.Message}");
                }
            }
            return string.Empty;
        }
    }

    sealed class StringLengthExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_length(val), aliased as stringlength|strlen");
                return (BoxedValue)(-1);
            }

            {
                string str = operands[0].AsString;
                if (!string.IsNullOrEmpty(str)) {
                    return BoxedValue.From(str.Length);
                }
            }
            return -1;
        }
    }

    sealed class StringStartsWithExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_starts_with(str, substr), aliased as starts_with");
                return BoxedValue.From(false);
            }

            {
                try {
                    string str = operands[0].AsString;
                    string sub = operands[1].AsString;
                    if (str == null) {
                        return BoxedValue.From(false);
                    }
                    if (sub == null) {
                        return BoxedValue.From(false);
                    }
                    return BoxedValue.From(str.StartsWith(sub));
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"string_starts_with error: {ex.Message}");
                }
            }
            return BoxedValue.From(false);
        }
    }

    sealed class StringEndsWithExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_ends_with(str, substr), aliased as ends_with");
                return BoxedValue.From(false);
            }

            {
                try {
                    string str = operands[0].AsString;
                    string sub = operands[1].AsString;
                    if (str == null) {
                        return BoxedValue.From(false);
                    }
                    if (sub == null) {
                        return BoxedValue.From(false);
                    }
                    return BoxedValue.From(str.EndsWith(sub));
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"string_ends_with error: {ex.Message}");
                }
            }
            return BoxedValue.From(false);
        }
    }

    sealed class ExtractTagsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var empty = new List<IList<string>>();
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: extract_tags(txt, tag_name[, max_num]), aliased as extracttags");
                return BoxedValue.FromObject(empty);
            }

            try {
                string txt = operands[0].AsString;
                string tagName = operands[1].AsString;
                int maxNum = 0;
                if (operands.Count == 3) {
                    maxNum = operands[2].GetInt();
                }
                if (maxNum < 0) {
                    maxNum = 0;
                }
                if (string.IsNullOrEmpty(txt) || string.IsNullOrEmpty(tagName)) {
                    return BoxedValue.FromObject(empty);
                }

                var result = new List<IList<string>>();
                string escaped = System.Text.RegularExpressions.Regex.Escape(tagName);
                // Match three XML forms: <tag>content</tag>, <tag></tag>, <tag/>
                // Non-greedy, case-sensitive, tolerate whitespace around self-closing slash
                string pattern = "<" + escaped + @"\s*/\s*>|<" + escaped + ">(.*?)</" + escaped + ">";
                var re = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Singleline);
                foreach (System.Text.RegularExpressions.Match m in re.Matches(txt)) {
                    if (maxNum > 0 && result.Count >= maxNum) break;
                    var g = m.Groups[1];
                    if (!g.Success || g.Value.Length == 0) {
                        result.Add(new List<string>());
                    }
                    else {
                        var parts = g.Value.Split('|');
                        result.Add(new List<string>(parts));
                    }
                }
                return BoxedValue.FromObject(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"extract_tags error: {ex.Message}");
            }
            return BoxedValue.FromObject(empty);
        }
    }

    sealed class ExtractTagCodesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var empty = new List<BoxedValue>();
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: extract_tag_codes(txt, tag_name[, max_num]), aliased as extracttagcodes");
                return BoxedValue.FromObject(empty);
            }

            try {
                string txt = operands[0].AsString;
                string tagName = operands[1].AsString;
                int maxNum = 0;
                if (operands.Count == 3) {
                    maxNum = operands[2].GetInt();
                }
                if (maxNum < 0) {
                    maxNum = 0;
                }
                if (string.IsNullOrEmpty(txt) || string.IsNullOrEmpty(tagName)) {
                    return BoxedValue.FromObject(empty);
                }

                var result = new List<BoxedValue>();
                string escaped = System.Text.RegularExpressions.Regex.Escape(tagName);
                // Match three XML forms: <tag>content</tag>, <tag></tag>, <tag/>
                // Non-greedy, case-sensitive, tolerate whitespace around self-closing slash
                string pattern = "<" + escaped + @"\s*/\s*>|<" + escaped + ">(.*?)</" + escaped + ">";
                var re = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Singleline);
                foreach (System.Text.RegularExpressions.Match m in re.Matches(txt)) {
                    if (maxNum > 0 && result.Count >= maxNum) break;
                    var g = m.Groups[1];
                    result.Add(BoxedValue.FromString(g.Success ? g.Value : string.Empty));
                }
                return BoxedValue.FromObject(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"extract_tag_codes error: {ex.Message}");
            }
            return BoxedValue.FromObject(empty);
        }
    }

    // HTML/URL Encode Operations
    sealed class HtmlEncodeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: html_encode(html_str)");
                return BoxedValue.FromString(string.Empty);
            }

            try {
                string s = operands[0].AsString ?? string.Empty;
                return BoxedValue.FromString(System.Net.WebUtility.HtmlEncode(s));
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"html_encode error: {ex.Message}");
            }
            return BoxedValue.FromString(string.Empty);
        }
    }

    sealed class HtmlDecodeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: html_decode(encoded_html_str)");
                return BoxedValue.FromString(string.Empty);
            }

            try {
                string s = operands[0].AsString ?? string.Empty;
                return BoxedValue.FromString(System.Net.WebUtility.HtmlDecode(s));
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"html_decode error: {ex.Message}");
            }
            return BoxedValue.FromString(string.Empty);
        }
    }

    sealed class UrlEncodeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: url_encode(url_str)");
                return BoxedValue.FromString(string.Empty);
            }

            try {
                string s = operands[0].AsString ?? string.Empty;
                return BoxedValue.FromString(System.Net.WebUtility.UrlEncode(s));
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"url_encode error: {ex.Message}");
            }
            return BoxedValue.FromString(string.Empty);
        }
    }

    sealed class UrlDecodeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: url_decode(encoded_url_str)");
                return BoxedValue.FromString(string.Empty);
            }

            try {
                string s = operands[0].AsString ?? string.Empty;
                return BoxedValue.FromString(System.Net.WebUtility.UrlDecode(s));
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"url_decode error: {ex.Message}");
            }
            return BoxedValue.FromString(string.Empty);
        }
    }

    sealed class ToPrettyStringExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: to_pretty_string(val)");
                return (BoxedValue)string.Empty;
            }

            try {
                string? input = null;
                var obj = operands[0].GetObject();
                if (null == obj) {
                    return BoxedValue.FromString(string.Empty);
                }
                if (obj is string s) {
                    input = s;
                }
                else {
                    var sb0 = new StringBuilder();
                    DslHelper.ConvertToString(operands[0], sb0, 0, true);
                    input = sb0.ToString();
                }
                if (string.IsNullOrEmpty(input)) {
                    return BoxedValue.FromString(string.Empty);
                }
                return BoxedValue.FromString(UnescapeLiteral(input));
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"to_pretty_string error: {ex.Message}");
            }
            return BoxedValue.FromString(string.Empty);
        }

        private static string UnescapeLiteral(string input)
        {
            var sb = new StringBuilder(input.Length);
            int i = 0;
            int n = input.Length;
            while (i < n) {
                char c = input[i];
                if (c != '\\') {
                    sb.Append(c);
                    i++;
                    continue;
                }
                if (i + 1 >= n) {
                    // trailing backslash, keep as-is
                    sb.Append(c);
                    i++;
                    continue;
                }
                char next = input[i + 1];
                switch (next) {
                    case '\\': sb.Append('\\'); i += 2; break;
                    case '"': sb.Append('"'); i += 2; break;
                    case '\'': sb.Append('\''); i += 2; break;
                    case 'n': sb.Append('\n'); i += 2; break;
                    case 'r': sb.Append('\r'); i += 2; break;
                    case 't': sb.Append('\t'); i += 2; break;
                    case 'b': sb.Append('\b'); i += 2; break;
                    case 'f': sb.Append('\f'); i += 2; break;
                    case 'v': sb.Append('\v'); i += 2; break;
                    case 'a': sb.Append('\a'); i += 2; break;
                    case '0': sb.Append('\0'); i += 2; break;
                    case 'u': {
                            // \uXXXX - require exactly 4 hex digits
                            if (i + 5 < n && IsHex(input[i + 2]) && IsHex(input[i + 3]) && IsHex(input[i + 4]) && IsHex(input[i + 5])) {
                                int code = (HexVal(input[i + 2]) << 12) | (HexVal(input[i + 3]) << 8) | (HexVal(input[i + 4]) << 4) | HexVal(input[i + 5]);
                                sb.Append((char)code);
                                i += 6;
                            }
                            else {
                                // invalid, keep literal
                                sb.Append(c);
                                sb.Append(next);
                                i += 2;
                            }
                            break;
                        }
                    case 'x': {
                            // \xH{1..4} - greedy 1 to 4 hex digits
                            int j = i + 2;
                            int hexCount = 0;
                            int code = 0;
                            while (j < n && hexCount < 4 && IsHex(input[j])) {
                                code = (code << 4) | HexVal(input[j]);
                                j++;
                                hexCount++;
                            }
                            if (hexCount > 0) {
                                sb.Append((char)code);
                                i = j;
                            }
                            else {
                                // no hex digit, keep literal
                                sb.Append(c);
                                sb.Append(next);
                                i += 2;
                            }
                            break;
                        }
                    default:
                        // unknown escape, keep literal
                        sb.Append(c);
                        sb.Append(next);
                        i += 2;
                        break;
                }
            }
            return sb.ToString();
        }

        private static bool IsHex(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }

        private static int HexVal(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            return c - 'A' + 10;
        }
    }
    internal sealed class ListContainsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_contains(list,val)");
                return BoxedValue.NullObject;
            }
            bool r = false;
            if (operands.Count >= 2) {
                var list = operands[0].As<System.Collections.IList>();
                var val = operands[1];
                if (null != list && list is List<BoxedValue> bvList) {
                    r = bvList.Contains(val);
                }
                else if (null != list) {
                    r = list.Contains(val.GetObject());
                }
            }
            return BoxedValue.FromBool(r);
        }
    }
    internal sealed class HashtableContainsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hashtable_contains(hash,val)");
                return BoxedValue.NullObject;
            }
            bool r = false;
            if (operands.Count >= 2) {
                var list = operands[0].As<System.Collections.IDictionary>();
                var val = operands[1];
                if (null != list && list is IDictionary<BoxedValue, BoxedValue> bvList) {
                    r = bvList.ContainsKey(val);
                }
                else if (null != list) {
                    r = list.Contains(val.GetObject());
                }
            }
            return BoxedValue.FromBool(r);
        }
    }
    internal sealed class StringBuilderLengthExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_builder_length(sb), aliased as stringbuilder_length or stringbuilderlength");
                return BoxedValue.NullObject;
            }
            int r = -1;
            if (operands.Count >= 1) {
                var sb = operands[0].As<StringBuilder>();
                if (null != sb) {
                    r = sb.Length;
                }
            }
            return BoxedValue.From(r);
        }
    }
    internal sealed class CharCodeAtExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2)
                throw new Exception("Expected: char_code_at(str,index), aliased as charcodeat");
            BoxedValue r = BoxedValue.NullObject;
            var str = operands[0].AsString;
            int index = operands[1].GetInt();
            if (null != str) {
                int len = str.Length;
                if (index >= 0 && index < len) {
                    r = BoxedValue.From((int)str[index]);
                }
            }
            return r;
        }
    }
    internal sealed class ArrayToListExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3)
                throw new Exception("Expected: array_to_list(array[,start,count]), aliased as arraytolist");
            BoxedValue r = BoxedValue.NullObject;
            if (operands.Count >= 1) {
                var array = operands[0].As<Array>();
                if (null != array) {
                    int start = 0;
                    int len = array.Length;
                    if (operands.Count >= 2) {
                        start = operands[1].GetInt();
                        if (start < 0) {
                            start = 0;
                        }
                        else if (start >= len) {
                            start = len - 1;
                        }
                        len -= start;
                    }
                    if (operands.Count >= 3) {
                        len = operands[2].GetInt();
                        if (len < 0) {
                            len = 0;
                        }
                        else if (len > array.Length - start) {
                            len = array.Length - start;
                        }
                    }
                    if (array is byte[] bytes) {
                        r = BoxedValue.FromObject(ArrayToList(bytes, start, len));
                    }
                    else if (array is sbyte[] sbytes) {
                        r = BoxedValue.FromObject(ArrayToList(sbytes, start, len));
                    }
                    else if (array is char[] chars) {
                        r = BoxedValue.FromObject(ArrayToList(chars, start, len));
                    }
                    else if (array is short[] shorts) {
                        r = BoxedValue.FromObject(ArrayToList(shorts, start, len));
                    }
                    else if (array is ushort[] ushorts) {
                        r = BoxedValue.FromObject(ArrayToList(ushorts, start, len));
                    }
                    else if (array is int[] ints) {
                        r = BoxedValue.FromObject(ArrayToList(ints, start, len));
                    }
                    else if (array is uint[] uints) {
                        r = BoxedValue.FromObject(ArrayToList(uints, start, len));
                    }
                    else if (array is long[] longs) {
                        r = BoxedValue.FromObject(ArrayToList(longs, start, len));
                    }
                    else if (array is ulong[] ulongs) {
                        r = BoxedValue.FromObject(ArrayToList(ulongs, start, len));
                    }
                    else if (array is decimal[] decimals) {
                        r = BoxedValue.FromObject(ArrayToList(decimals, start, len));
                    }
                    else if (array is float[] floats) {
                        r = BoxedValue.FromObject(ArrayToList(floats, start, len));
                    }
                    else if (array is double[] doubles) {
                        r = BoxedValue.FromObject(ArrayToList(doubles, start, len));
                    }
                    else if (array is DateTime[] datetimes) {
                        r = BoxedValue.FromObject(ArrayToList(datetimes, start, len));
                    }
                    else if (array is BoxedValue[] boxedvals) {
                        r = BoxedValue.FromObject(ArrayToList(boxedvals, start, len));
                    }
                    else if (array is object[] objects) {
                        r = BoxedValue.FromObject(ArrayToList(objects, start, len));
                    }
                }
            }
            return r;
        }
        internal static List<T> ArrayToList<T>(T[] array, int start, int len)
        {
            if (len == array.Length) {
                return new List<T>(array);
            }
            else {
                var list = new List<T>(len);
                list.AddRange(array.AsSpan(start, len));
                return list;
            }
        }
    }
    internal sealed class ListToArrayExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3)
                throw new Exception("Expected: list_to_array(list[,start,count]), aliased as listtoarray");
            BoxedValue r = BoxedValue.NullObject;
            if (operands.Count >= 1) {
                var list = operands[0].As<IList>();
                if (null != list) {
                    int start = 0;
                    int count = list.Count;
                    if (operands.Count >= 2) {
                        start = operands[1].GetInt();
                        if (start < 0) {
                            start = 0;
                        }
                        else if (start >= count) {
                            start = count - 1;
                        }
                        count -= start;
                    }
                    if (operands.Count >= 3) {
                        count = operands[2].GetInt();
                        if (count < 0) {
                            count = 0;
                        }
                        else if (count > list.Count - start) {
                            count = list.Count - start;
                        }
                    }
                    if (list is IList<byte> bytes) {
                        r = BoxedValue.FromObject(ListToArray(bytes, start, count));
                    }
                    else if (list is IList<sbyte> sbytes) {
                        r = BoxedValue.FromObject(ListToArray(sbytes, start, count));
                    }
                    else if (list is IList<char> chars) {
                        r = BoxedValue.FromObject(ListToArray(chars, start, count));
                    }
                    else if (list is IList<short> shorts) {
                        r = BoxedValue.FromObject(ListToArray(shorts, start, count));
                    }
                    else if (list is IList<ushort> ushorts) {
                        r = BoxedValue.FromObject(ListToArray(ushorts, start, count));
                    }
                    else if (list is IList<int> ints) {
                        r = BoxedValue.FromObject(ListToArray(ints, start, count));
                    }
                    else if (list is IList<uint> uints) {
                        r = BoxedValue.FromObject(ListToArray(uints, start, count));
                    }
                    else if (list is IList<long> longs) {
                        r = BoxedValue.FromObject(ListToArray(longs, start, count));
                    }
                    else if (list is IList<ulong> ulongs) {
                        r = BoxedValue.FromObject(ListToArray(ulongs, start, count));
                    }
                    else if (list is IList<decimal> decimals) {
                        r = BoxedValue.FromObject(ListToArray(decimals, start, count));
                    }
                    else if (list is IList<float> floats) {
                        r = BoxedValue.FromObject(ListToArray(floats, start, count));
                    }
                    else if (list is IList<double> doubles) {
                        r = BoxedValue.FromObject(ListToArray(doubles, start, count));
                    }
                    else if (list is IList<DateTime> datetimes) {
                        r = BoxedValue.FromObject(ListToArray(datetimes, start, count));
                    }
                    else if (list is IList<BoxedValue> boxedvals) {
                        r = BoxedValue.FromObject(ListToArray(boxedvals, start, count));
                    }
                    else if (list is IList<object> objects) {
                        r = BoxedValue.FromObject(ListToArray(objects, start, count));
                    }
                }
            }
            return r;
        }
        internal static T[] ListToArray<T>(IList<T> list, int start, int count)
        {
            if (count == list.Count) {
                return list.ToArray();
            }
            else {
                var array = new T[count];
                Array.Copy(list.ToArray(), start, array, 0, count);
                return array;
            }
        }
    }
    internal sealed class SubListExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3)
                throw new Exception("Expected: sublist(list[,start,count]) (alias: list_slice)");
            BoxedValue r = BoxedValue.NullObject;
            if (operands.Count >= 1) {
                var list = operands[0].As<IList>();
                if (null != list) {
                    int start = 0;
                    int count = list.Count;
                    if (operands.Count >= 2) {
                        start = operands[1].GetInt();
                        if (start < 0) {
                            start = 0;
                        }
                        else if (start >= count) {
                            start = count - 1;
                        }
                        count -= start;
                    }
                    if (operands.Count >= 3) {
                        count = operands[2].GetInt();
                        if (count < 0) {
                            count = 0;
                        }
                        else if (count > list.Count - start) {
                            count = list.Count - start;
                        }
                    }
                    if (list is IList<byte> bytes) {
                        r = BoxedValue.FromObject(SubList(bytes, start, count));
                    }
                    else if (list is IList<sbyte> sbytes) {
                        r = BoxedValue.FromObject(SubList(sbytes, start, count));
                    }
                    else if (list is IList<char> chars) {
                        r = BoxedValue.FromObject(SubList(chars, start, count));
                    }
                    else if (list is IList<short> shorts) {
                        r = BoxedValue.FromObject(SubList(shorts, start, count));
                    }
                    else if (list is IList<ushort> ushorts) {
                        r = BoxedValue.FromObject(SubList(ushorts, start, count));
                    }
                    else if (list is IList<int> ints) {
                        r = BoxedValue.FromObject(SubList(ints, start, count));
                    }
                    else if (list is IList<uint> uints) {
                        r = BoxedValue.FromObject(SubList(uints, start, count));
                    }
                    else if (list is IList<long> longs) {
                        r = BoxedValue.FromObject(SubList(longs, start, count));
                    }
                    else if (list is IList<ulong> ulongs) {
                        r = BoxedValue.FromObject(SubList(ulongs, start, count));
                    }
                    else if (list is IList<decimal> decimals) {
                        r = BoxedValue.FromObject(SubList(decimals, start, count));
                    }
                    else if (list is IList<float> floats) {
                        r = BoxedValue.FromObject(SubList(floats, start, count));
                    }
                    else if (list is IList<double> doubles) {
                        r = BoxedValue.FromObject(SubList(doubles, start, count));
                    }
                    else if (list is IList<DateTime> datetimes) {
                        r = BoxedValue.FromObject(SubList(datetimes, start, count));
                    }
                    else if (list is IList<BoxedValue> boxedvals) {
                        r = BoxedValue.FromObject(SubList(boxedvals, start, count));
                    }
                    else if (list is IList<object> objects) {
                        r = BoxedValue.FromObject(SubList(objects, start, count));
                    }
                }
            }
            return r;
        }
        internal static IList<T> SubList<T>(IList<T> list, int start, int count)
        {
            if (count == list.Count) {
                return new List<T>(list);
            }
            else {
                var r = new List<T>(count);
                for (int i = 0; i < count; ++i) {
                    r.Add(list[start + i]);
                }
                return r;
            }
        }
    }
    // subarray(array[, start, count]) - slice array, returns array of same element type
    internal sealed class SubArrayExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 3)
                throw new Exception("Expected: subarray(array[,start,count]) (alias: array_slice)");
            BoxedValue r = BoxedValue.NullObject;
            var arr = operands[0].As<Array>();
            if (null != arr) {
                int start = 0;
                int count = arr.Length;
                if (operands.Count >= 2) {
                    start = operands[1].GetInt();
                    if (start < 0) {
                        start = 0;
                    }
                    else if (start >= count) {
                        start = count - 1;
                    }
                    count -= start;
                }
                if (operands.Count >= 3) {
                    count = operands[2].GetInt();
                    if (count < 0) {
                        count = 0;
                    }
                    else if (count > arr.Length - start) {
                        count = arr.Length - start;
                    }
                }
                var elementType = arr.GetType().GetElementType();
                if (null != elementType) {
                    var result = Array.CreateInstance(elementType, count);
                    Array.Copy(arr, start, result, 0, count);
                    r = BoxedValue.FromObject(result);
                }
            }
            return r;
        }
    }
    // bytes_to_string(bytes[, encoding]) - convert byte array to string, default UTF-8
    internal sealed class BytesToStringExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2)
                throw new Exception("Expected: bytes_to_string(bytes[, encoding]) (alias: bytestostring)");
            byte[]? bytes = operands[0].As<byte[]>();
            if (null == bytes) {
                var list = operands[0].As<IList<byte>>();
                if (null != list) {
                    bytes = new byte[list.Count];
                    list.CopyTo(bytes, 0);
                }
            }
            if (null == bytes)
                return BoxedValue.FromString(string.Empty);
            Encoding encoding = operands.Count > 1 ? (GetEncoding(operands[1]) ?? Encoding.UTF8) : Encoding.UTF8;
            return BoxedValue.FromString(encoding.GetString(bytes));
        }
    }
    // string_to_bytes(str[, encoding]) - convert string to byte array, default UTF-8
    internal sealed class StringToBytesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2)
                throw new Exception("Expected: string_to_bytes(str[, encoding]) (alias: stringtobytes)");
            string s = operands[0].AsString ?? string.Empty;
            Encoding encoding = operands.Count > 1 ? (GetEncoding(operands[1]) ?? Encoding.UTF8) : Encoding.UTF8;
            return BoxedValue.FromObject(encoding.GetBytes(s));
        }
    }
    // string_first_lines(str, n) - return the first n lines of a string
    internal sealed class StringFirstLinesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2)
                throw new Exception("Expected: string_first_lines(str, n) (alias: stringfirstlines)");
            string s = operands[0].AsString ?? string.Empty;
            int n = operands[1].GetInt();
            if (n <= 0)
                return BoxedValue.FromString(string.Empty);
            var lines = s.Split('\n');
            if (n >= lines.Length)
                return BoxedValue.FromString(s);
            var sb = new StringBuilder();
            for (int i = 0; i < n; ++i) {
                if (i > 0) sb.Append('\n');
                sb.Append(lines[i]);
            }
            return BoxedValue.FromString(sb.ToString());
        }
    }
    // string_replace_with_count(str, oldValue, newValue, count) - replace first N occurrences
    internal sealed class StringReplaceWithCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 4)
                throw new Exception("Expected: string_replace_with_count(str, oldValue, newValue, count) (alias: stringreplacewithcount)");
            string s = operands[0].AsString ?? string.Empty;
            string oldValue = operands[1].AsString ?? string.Empty;
            string newValue = operands[2].AsString ?? string.Empty;
            int count = operands[3].GetInt();
            if (string.IsNullOrEmpty(oldValue) || count <= 0)
                return BoxedValue.FromString(s);
            var sb = new StringBuilder();
            int pos = 0;
            int replaced = 0;
            while (replaced < count) {
                int idx = s.IndexOf(oldValue, pos, StringComparison.Ordinal);
                if (idx < 0) break;
                sb.Append(s, pos, idx - pos);
                sb.Append(newValue);
                pos = idx + oldValue.Length;
                ++replaced;
            }
            if (pos < s.Length) sb.Append(s, pos, s.Length - pos);
            return BoxedValue.FromString(sb.ToString());
        }
    }
}
