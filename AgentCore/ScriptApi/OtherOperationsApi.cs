using System;
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
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"getclipboard error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    sealed class SetClipboardExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                try {
                    string text = operands[0].AsString;
                    bool result = Core.AgentCore.Instance.ClipboardOps.SetText(text);
                    return BoxedValue.From(result);
                }
                catch (Exception ex) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"setclipboard error: {ex.Message}");
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
            if (operands.Count >= 1) {
                try {
                    string fmt = operands[0].AsString;
                    var args = new object[operands.Count - 1];
                    for (int i = 1; i < operands.Count; i++) {
                        args[i - 1] = operands[i].GetObject();
                    }
                    Core.AgentCore.Instance.Logger.Info(fmt, args);
                }
                catch (Exception ex) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"loginfo error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class LogErrorExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                try {
                    string fmt = operands[0].AsString;
                    var args = new object[operands.Count - 1];
                    for (int i = 1; i < operands.Count; i++) {
                        args[i - 1] = operands[i].GetObject();
                    }
                    Core.AgentCore.Instance.Logger.Error(fmt, args);
                }
                catch (Exception ex) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"logerror error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class LogWarningExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                try {
                    string fmt = operands[0].AsString;
                    var args = new object[operands.Count - 1];
                    for (int i = 1; i < operands.Count; i++) {
                        args[i - 1] = operands[i].GetObject();
                    }
                    Core.AgentCore.Instance.Logger.Warning(fmt, args);
                }
                catch (Exception ex) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"logwarning error: {ex.Message}");
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
            if (operands.Count >= 1) {
                try {
                    string url = operands[0].AsString;
                    string result = Core.AgentCore.Instance.HttpClient.Get(url);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"httpget error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class HttpPostExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 2) {
                try {
                    string url = operands[0].AsString;
                    string content = operands[1].AsString;
                    string contentType = operands.Count > 2 ? operands[2].AsString : "application/json";
                    string result = Core.AgentCore.Instance.HttpClient.Post(url, content, contentType);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"httppost error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class DownloadFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 2) {
                try {
                    string url = operands[0].AsString;
                    string savePath = operands[1].AsString;
                    bool result = Core.AgentCore.Instance.HttpClient.DownloadFile(url, savePath);
                    return BoxedValue.From(result);
                }
                catch (Exception ex) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"downloadfile error: {ex.Message}");
                }
            }
            return BoxedValue.From(false);
        }
    }

    // JSON Operations
    sealed class ToJsonExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                try {
                    object obj = operands[0].GetObject();
                    if (obj is IDictionary<BoxedValue, BoxedValue> dict) {
                        obj = DslHelper.GetDictionaryFromBoxedValue(dict);
                    }
                    bool prettyPrint = operands.Count > 1 ? operands[1].GetBool() : false;
                    string json = JsonHelper.ToJson(obj, prettyPrint);
                    return BoxedValue.FromString(json);
                }
                catch (Exception ex) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"tojson error: {ex.Message}");
                }
            }
            return BoxedValue.FromString("null");
        }
    }

    sealed class FromJsonExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                try {
                    string json = operands[0].AsString;
                    object obj = JsonHelper.FromJson(json);
                    return BoxedValue.FromObject(obj);
                }
                catch (Exception ex) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"fromjson error: {ex.Message}");
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
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"newobject error: {ex.Message}");
                return BoxedValue.FromObject(new Dictionary<string, object>());
            }
        }
    }

    sealed class ToStringExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                try {
                    var v = operands[0];
                    var sb = new StringBuilder();
                    DslHelper.ConvertToString(v, sb, 0, true);
                    return sb.ToString();
                }
                catch (Exception ex) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"fromjson error: {ex.Message}");
                }
            }
            return string.Empty;
        }
    }

    sealed class StringLengthExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                string str = operands[0].AsString;
                return BoxedValue.From(str.Length);
            }
            return -1;
        }
    }
}
