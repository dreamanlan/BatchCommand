using System;
using System.Collections;
using System.Text;
using CefDotnetApp.AgentCore.Utils;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Utils;
using DotNetLib;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    sealed class AppendExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 2) {
                try {
                    var sb = operands[0].As<StringBuilder>();
                    var v = operands[1];
                    var tmp = new StringBuilder();
                    DslHelper.ConvertToString(v, tmp, 0, true);
                    sb.Append(tmp.ToString());
                    return BoxedValue.FromObject(sb);
                }
                catch (Exception ex) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"fromjson error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }
    sealed class AppendLineExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 2) {
                try {
                    var sb = operands[0].As<StringBuilder>();
                    var v = operands[1];
                    var tmp = new StringBuilder();
                    DslHelper.ConvertToString(v, tmp, 0, true);
                    sb.AppendLine(tmp.ToString());
                    return BoxedValue.FromObject(sb);
                }
                catch (Exception ex) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"fromjson error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }
    internal sealed class StringIndexOfExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            BoxedValue r = BoxedValue.NullObject;
            if (operands.Count >= 2) {
                string srcstr = operands[0].GetString();
                string str = operands[1].GetString();
                if (srcstr != null && str != null) {
                    int start = 0;
                    int ct = srcstr.Length;
                    if (operands.Count >= 3) {
                        start = operands[2].GetInt();
                    }
                    if (operands.Count >= 4) {
                        ct = operands[3].GetInt();
                    }
                    if (ct > srcstr.Length - start) {
                        ct = srcstr.Length - start;
                    }
                    r = srcstr.IndexOf(str, start, ct);
                }
            }
            return r;
        }
    }
    internal sealed class StringLastIndexOfExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            BoxedValue r = BoxedValue.NullObject;
            if (operands.Count >= 2) {
                string srcstr = operands[0].GetString();
                string str = operands[1].GetString();
                if (srcstr != null && str != null) {
                    int start = srcstr.Length - 1;
                    int ct = srcstr.Length;
                    if (operands.Count >= 3) {
                        start = operands[2].GetInt();
                    }
                    if (operands.Count >= 4) {
                        ct = operands[3].GetInt();
                    }
                    if (ct > start + 1) {
                        ct = start + 1;
                    }
                    r = srcstr.LastIndexOf(str, start, ct);
                }
            }
            return r;
        }
    }
    internal sealed class StringIndexOfAnyExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            BoxedValue r = BoxedValue.NullObject;
            if (operands.Count >= 2) {
                string srcstr = operands[0].GetString();
                IList seps = operands[1].As<IList>();
                if (srcstr != null && seps != null) {
                    char[] cs = new char[seps.Count];
                    for (int i = 0; i < seps.Count; i++) {
                        var _sep = seps[i];
                        if (null != _sep) {
                            string? sep = _sep.ToString();
                            if (sep?.Length > 0) {
                                cs[i] = sep[0];
                            }
                            else {
                                cs[i] = '\0';
                            }
                        }
                    }
                    int start = 0;
                    int ct = srcstr.Length;
                    if (operands.Count >= 3) {
                        start = operands[2].GetInt();
                    }
                    if (operands.Count >= 4) {
                        ct = operands[3].GetInt();
                    }
                    if (ct > srcstr.Length - start) {
                        ct = srcstr.Length - start;
                    }
                    r = srcstr.IndexOfAny(cs, start, ct);
                }
            }
            return r;
        }
    }
    internal sealed class StringLastIndexOfAnyExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            BoxedValue r = BoxedValue.NullObject;
            if (operands.Count >= 2) {
                string srcstr = operands[0].GetString();
                IList seps = operands[1].As<IList>();
                if (srcstr != null && seps != null) {
                    char[] cs = new char[seps.Count];
                    for (int i = 0; i < seps.Count; i++) {
                        var _sep = seps[i];
                        if (null != _sep) {
                            string? sep = _sep.ToString();
                            if (sep?.Length > 0) {
                                cs[i] = sep[0];
                            }
                            else {
                                cs[i] = '\0';
                            }
                        }
                    }
                    int start = srcstr.Length - 1;
                    int ct = srcstr.Length;
                    if (operands.Count >= 3) {
                        start = operands[2].GetInt();
                    }
                    if (operands.Count >= 4) {
                        ct = operands[3].GetInt();
                    }
                    if (ct > start + 1) {
                        ct = start + 1;
                    }
                    r = srcstr.LastIndexOfAny(cs, start, ct);
                }
            }
            return r;
        }
    }
    internal sealed class StringConcatExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var sb = new StringBuilder();
            foreach(var opd in operands) {
                sb.Append(opd.ToString());
            }
            return sb.ToString();
        }
    }
    internal sealed class SizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                string str = operands[0].AsString;
                if (null != str) {
                    return BoxedValue.From(str.Length);
                }
                else {
                    System.Collections.IList list = operands[0].As<System.Collections.IList>();
                    if (null != list) {
                        return BoxedValue.From(list.Count);
                    }
                    else {
                        System.Collections.IDictionary dict = operands[0].As<System.Collections.IDictionary>();
                        if (null != dict) {
                            return BoxedValue.From(dict.Count);
                        }
                        else {
                            return 0;
                        }
                    }
                }
            }
            return -1;
        }
    }
    internal sealed class CharToIntExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            BoxedValue r = BoxedValue.NullObject;
            if (operands.Count >= 1) {
                string str = operands[0].AsString;
                if (!string.IsNullOrEmpty(str)) {
                    char c = str[0];
                    r = BoxedValue.From((int)c);
                }
            }
            return r;
        }
    }
    internal sealed class IntToCharExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            BoxedValue r = BoxedValue.NullObject;
            if (operands.Count >= 1) {
                int ascii = operands[0].GetInt();
                char c = (char)ascii;
                r = c.ToString();
            }
            return r;
        }
    }
    internal sealed class StringReplaceExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            BoxedValue r = BoxedValue.NullObject;
            if (operands.Count >= 3) {
                string str = operands[0].AsString;
                string key = operands[1].AsString;
                string val = operands[2].AsString;
                if (null != str && null != key && null != val) {
                    if (string.IsNullOrEmpty(key)) {
                        DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("The substr cannot be empty !!!");
                        return BoxedValue.From(str);
                    }
                    if (key.IndexOf('\n') >= 0) {
                        var result = DiffOperations.ReplaceFullLinesText(str, key, val);
                        if (result.Success) {
                            return BoxedValue.From(result.ResultContent);
                        }
                        else {
                            DotNetLib.NativeApi.AppendApiErrorInfoFormatLine(result.Error + "\nTry to use 'apply_diff' to replace multi-line text !!!");
                            return BoxedValue.From(str);
                        }
                    }
                    var trimedKey = key.Trim();
                    var trimedVal = val.Trim();
                    if (!str.Contains(trimedKey)) {
                        NativeApi.AppendApiErrorInfoFormatLine("replace_string: substr not found");
                    }
                    r = str.Replace(trimedKey, trimedVal);
                }
                else {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("replace_string: str or substr or replace is null !!!");
                    r = str;
                }
            }
            return r;
        }
    }
}
