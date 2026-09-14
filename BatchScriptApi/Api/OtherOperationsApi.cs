using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using BatchCommand.Utils;

namespace BatchCommand.Api
{
    // Generic (host independent) utility apis moved from AgentCore/ScriptApi/OtherOperationsApi.cs:
    // clipboard, json, string, collection and byte conversion helpers. The network/http and
    // logger apis stayed in AgentCore (they are bound to AgentCore services).

    // Clipboard Operations (TextCopy, see Utils/ClipboardOperations.cs).
    // Uses a private static instance so these also work in hosts that never
    // create a DslHost (e.g. the console BatchCommand.exe).
    public sealed class GetClipboardExp : SimpleExpressionBase
    {
        private static readonly BatchCommand.Utils.ClipboardOperations s_Ops = new BatchCommand.Utils.ClipboardOperations();

        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                string text = s_Ops.GetText();
                return BoxedValue.FromString(text);
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"getclipboard error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    public sealed class SetClipboardExp : SimpleExpressionBase
    {
        private static readonly BatchCommand.Utils.ClipboardOperations s_Ops = new BatchCommand.Utils.ClipboardOperations();

        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: set_clipboard(text)");
                return BoxedValue.From(false);
            }

            {
                try {
                    string text = operands[0].AsString;
                    bool result = s_Ops.SetText(text);
                    return BoxedValue.From(result);
                }
                catch (Exception ex) {
                    ApiErrorInfo.AppendLine($"setclipboard error: {ex.Message}");
                }
            }
            return BoxedValue.From(false);
        }
    }
    // JSON Operations
    public sealed class ToJsonExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                ApiErrorInfo.AppendLine("Expected: to_json(obj, prettyPrint)");
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
                    ApiErrorInfo.AppendLine($"tojson error: {ex.Message}");
                }
            }
            return BoxedValue.FromString("null");
        }
    }

    public sealed class FromJsonExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: from_json(json)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string json = operands[0].AsString;
                    object? obj = JsonHelper.FromJson(json);
                    return DslHelper.GetBoxedValueFromValue(obj);
                }
                catch (Exception ex) {
                    ApiErrorInfo.AppendLine($"fromjson error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    public sealed class JsonEscapeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                ApiErrorInfo.AppendLine("Expected: json_escape(str[, bool_add_quotes])");
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
                    ApiErrorInfo.AppendLine($"json_escape error: {ex.Message}");
                }
            }
            return BoxedValue.FromString(string.Empty);
        }
    }

    public sealed class JsonUnescapeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: json_unescape(str)");
                return BoxedValue.FromString(string.Empty);
            }

            {
                try {
                    string s = operands[0].AsString ?? string.Empty;
                    string un = JsonHelper.UnescapeJsonString(s);
                    return BoxedValue.FromString(un);
                }
                catch (Exception ex) {
                    ApiErrorInfo.AppendLine($"json_unescape error: {ex.Message}");
                }
            }
            return BoxedValue.FromString(string.Empty);
        }
    }


    public sealed class NewObjectExp : SimpleExpressionBase
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
                ApiErrorInfo.AppendLine($"newobject error: {ex.Message}");
                return BoxedValue.FromObject(new Dictionary<string, object>());
            }
        }
    }

    public sealed class ToStringExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: to_string(val)");
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
                    ApiErrorInfo.AppendLine($"fromjson error: {ex.Message}");
                }
            }
            return string.Empty;
        }
    }

    public sealed class StringLengthExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: string_length(val), aliased as stringlength|strlen");
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

    public sealed class StringStartsWithExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                ApiErrorInfo.AppendLine("Expected: string_starts_with(str, substr), aliased as starts_with");
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
                    ApiErrorInfo.AppendLine($"string_starts_with error: {ex.Message}");
                }
            }
            return BoxedValue.From(false);
        }
    }

    public sealed class StringEndsWithExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                ApiErrorInfo.AppendLine("Expected: string_ends_with(str, substr), aliased as ends_with");
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
                    ApiErrorInfo.AppendLine($"string_ends_with error: {ex.Message}");
                }
            }
            return BoxedValue.From(false);
        }
    }

    public sealed class ExtractTagsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var empty = new List<IList<string>>();
            if (operands.Count < 2 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: extract_tags(txt, tag_name[, max_num]), aliased as extracttags");
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
                ApiErrorInfo.AppendLine($"extract_tags error: {ex.Message}");
            }
            return BoxedValue.FromObject(empty);
        }
    }

    public sealed class ExtractTagCodesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var empty = new List<BoxedValue>();
            if (operands.Count < 2 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: extract_tag_codes(txt, tag_name[, max_num]), aliased as extracttagcodes");
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
                ApiErrorInfo.AppendLine($"extract_tag_codes error: {ex.Message}");
            }
            return BoxedValue.FromObject(empty);
        }
    }

    // HTML/URL Encode Operations
    public sealed class HtmlEncodeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: html_encode(html_str)");
                return BoxedValue.FromString(string.Empty);
            }

            try {
                string s = operands[0].AsString ?? string.Empty;
                return BoxedValue.FromString(System.Net.WebUtility.HtmlEncode(s));
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"html_encode error: {ex.Message}");
            }
            return BoxedValue.FromString(string.Empty);
        }
    }

    public sealed class HtmlDecodeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: html_decode(encoded_html_str)");
                return BoxedValue.FromString(string.Empty);
            }

            try {
                string s = operands[0].AsString ?? string.Empty;
                return BoxedValue.FromString(System.Net.WebUtility.HtmlDecode(s));
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"html_decode error: {ex.Message}");
            }
            return BoxedValue.FromString(string.Empty);
        }
    }

    public sealed class UrlEncodeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: url_encode(url_str)");
                return BoxedValue.FromString(string.Empty);
            }

            try {
                string s = operands[0].AsString ?? string.Empty;
                return BoxedValue.FromString(System.Net.WebUtility.UrlEncode(s));
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"url_encode error: {ex.Message}");
            }
            return BoxedValue.FromString(string.Empty);
        }
    }

    public sealed class UrlDecodeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: url_decode(encoded_url_str)");
                return BoxedValue.FromString(string.Empty);
            }

            try {
                string s = operands[0].AsString ?? string.Empty;
                return BoxedValue.FromString(System.Net.WebUtility.UrlDecode(s));
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"url_decode error: {ex.Message}");
            }
            return BoxedValue.FromString(string.Empty);
        }
    }

    public sealed class ToPrettyStringExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: to_pretty_string(val)");
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
                ApiErrorInfo.AppendLine($"to_pretty_string error: {ex.Message}");
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
    public sealed class ListContainsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                ApiErrorInfo.AppendLine("Expected: list_contains(list,val)");
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
    public sealed class HashtableContainsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                ApiErrorInfo.AppendLine("Expected: hashtable_contains(hash,val)");
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
    public sealed class StringBuilderLengthExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: string_builder_length(sb), aliased as stringbuilder_length or stringbuilderlength");
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
    public sealed class CharCodeAtExp : SimpleExpressionBase
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
    public sealed class ArrayToListExp : SimpleExpressionBase
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
    public sealed class ListToArrayExp : SimpleExpressionBase
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
    public sealed class SubListExp : SimpleExpressionBase
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
    public sealed class SubArrayExp : SimpleExpressionBase
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
    public sealed class BytesToStringExp : SimpleExpressionBase
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
    public sealed class StringToBytesExp : SimpleExpressionBase
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
    public sealed class StringFirstLinesExp : SimpleExpressionBase
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
    // string_replace_with_count(str, oldValue, newValue, count[, skipCount]) - skip first skipCount matches, then replace next N occurrences
    public sealed class StringReplaceWithCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 4 || operands.Count > 5)
                throw new Exception("Expected: string_replace_with_count(str, oldValue, newValue, count[, skipCount]) (alias: stringreplacewithcount)");
            string s = operands[0].AsString ?? string.Empty;
            string oldValue = operands[1].AsString ?? string.Empty;
            string newValue = operands[2].AsString ?? string.Empty;
            int count = operands[3].GetInt();
            int skipCount = operands.Count > 4 ? operands[4].GetInt() : 0;
            if (skipCount < 0) skipCount = 0;
            if (string.IsNullOrEmpty(oldValue) || count <= 0)
                return BoxedValue.FromString(s);
            int pos = 0;
            int skipped = 0;
            while (skipped < skipCount) {
                int idx = s.IndexOf(oldValue, pos, StringComparison.Ordinal);
                if (idx < 0) return BoxedValue.FromString(s);
                pos = idx + oldValue.Length;
                ++skipped;
            }
            var sb = new StringBuilder();
            sb.Append(s, 0, pos);
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
