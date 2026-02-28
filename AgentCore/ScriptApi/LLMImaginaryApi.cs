using System;
using System.Collections;
using System.Text;
using CefDotnetApp.AgentCore.Utils;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using AgentPlugin.Abstractions;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    sealed class AppendExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: append(stringbuilder, val)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    var sb = operands[0].As<StringBuilder>();
                    var v = operands[1];
                    var tmp = new StringBuilder();
                    DslHelper.ConvertToString(v, tmp, 0, true);
                    sb.Append(tmp.ToString());
                    return BoxedValue.FromObject(sb);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"fromjson error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }
    sealed class AppendLineExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: append_line(stringbuilder, val)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    var sb = operands[0].As<StringBuilder>();
                    var v = operands[1];
                    var tmp = new StringBuilder();
                    DslHelper.ConvertToString(v, tmp, 0, true);
                    sb.AppendLine(tmp.ToString());
                    return BoxedValue.FromObject(sb);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"fromjson error: {ex.Message}");
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
            if (operands.Count < 2 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_index_of(str, substr[, start, count]), aliased as string_find|stringfind|index_of|indexof|string_indexof|stringindexof");
                return BoxedValue.NullObject;
            }

            {
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
            if (operands.Count < 2 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_last_index_of(str, substr[, start, count]), aliased as last_index_of|lastindexof|string_last_indexof|stringlastindexof");
                return BoxedValue.NullObject;
            }

            {
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
            if (operands.Count < 2 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_index_of_any(str, substr[, start, count]), aliased as index_of_any|indexofany|string_indexof_any|stringindexofany");
                return BoxedValue.NullObject;
            }

            {
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
            if (operands.Count < 2 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_last_index_of_any(str, substr[, start, count]), aliased as last_index_of_any|lastindexofany|string_last_indexof_any|stringlastindexofany");
                return BoxedValue.NullObject;
            }

            {
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
            foreach (var opd in operands) {
                sb.Append(opd.ToString());
            }
            return sb.ToString();
        }
    }
    internal sealed class SizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: length(str_or_list_or_dict), aliased as len|size|count");
                return (BoxedValue)(-1);
            }

            {
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
        }
    }
    internal sealed class CharToIntExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            BoxedValue r = BoxedValue.NullObject;
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: char_to_int(char_str)");
                return BoxedValue.NullObject;
            }

            {
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
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: int_to_char(int_ascii)");
                return BoxedValue.NullObject;
            }

            {
                int ascii = operands[0].GetInt();
                char c = (char)ascii;
                r = c.ToString();
            }
            return r;
        }
    }
}
