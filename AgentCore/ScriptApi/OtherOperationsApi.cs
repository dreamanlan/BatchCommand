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
    sealed class HttpGetExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: fetch(url), aliased as http_get");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string url = operands[0].AsString;
                    string result = Core.AgentCore.Instance.HttpClient.Get(url);
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
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: http_post(url, content, contentType)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string url = operands[0].AsString;
                    string content = operands[1].AsString;
                    string contentType = operands.Count > 2 ? operands[2].AsString : "application/json";
                    string result = Core.AgentCore.Instance.HttpClient.Post(url, content, contentType);
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
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: download_file(url, savePath)");
                return BoxedValue.From(false);
            }

            {
                try {
                    string url = operands[0].AsString;
                    string savePath = operands[1].AsString;
                    bool result = Core.AgentCore.Instance.HttpClient.DownloadFile(url, savePath);
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
}
