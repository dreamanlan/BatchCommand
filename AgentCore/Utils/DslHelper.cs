using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ScriptableFramework;

namespace CefDotnetApp.AgentCore.Utils
{
    public static class DslHelper
    {
        public static void ConvertToString(BoxedValue val, StringBuilder sb, int indent, bool firstIndent)
        {
            if (indent > 0 && firstIndent) {
                AppendIndent(sb, indent);
            }
            if (!val.IsObject) {
                sb.Append(val.ToString());
            }
            else if (val.IsNullObject) {
                sb.Append("null");
            }
            else {
                var obj = val.GetObject();
                if (null != obj) {
                    if (obj is LitJson.JsonData jsonData) {
                        val = GetBoxedValueFromJsonValue(jsonData);
                        ConvertToString(val, sb, indent, firstIndent);
                    }
                    else if (obj is IList<string?> strlist) {
                        val = GetBoxedValueFromValue(strlist);
                        ConvertToString(val, sb, indent, firstIndent);
                    }
                    else if (obj is IDictionary<string, string?> strdict) {
                        val = GetBoxedValueFromValue(strdict);
                        ConvertToString(val, sb, indent, firstIndent);
                    }
                    else if (obj is IList<object?> list) {
                        val = GetBoxedValueFromValue(list);
                        ConvertToString(val, sb, indent, firstIndent);
                    }
                    else if (obj is IDictionary<string, object?> dict) {
                        val = GetBoxedValueFromValue(dict);
                        ConvertToString(val, sb, indent, firstIndent);
                    }
                    else if (obj is IList<BoxedValue> bvlist) {
                        sb.Append('[');
                        sb.AppendLine();
                        ++indent;
                        foreach(var bv in bvlist) {
                            ConvertToString(bv, sb, indent, true);
                            sb.AppendLine();
                        }
                        --indent;
                        AppendIndent(sb, indent);
                        sb.Append(']');
                        sb.AppendLine();
                    }
                    else if (obj is IDictionary<BoxedValue, BoxedValue> bvdict) {
                        sb.Append('{');
                        sb.AppendLine();
                        ++indent;
                        foreach (var pair in bvdict) {
                            var k = pair.Key;
                            var v = pair.Value;
                            AppendIndent(sb, indent);
                            sb.Append(k.ToString());
                            sb.Append(" : ");
                            ConvertToString(v, sb, indent, false);
                            sb.AppendLine();
                        }
                        --indent;
                        AppendIndent(sb, indent);
                        sb.Append('}');
                        sb.AppendLine();
                    }
                    else {
                        sb.Append(obj.ToString());
                    }
                }
            }
        }
        public static object? GetValueFromBoxedValue(BoxedValue boxed)
        {
            if (boxed.IsNullObject) {
                return null;
            }
            if (boxed.IsBoolean) {
                return boxed.GetBool();
            }
            if (boxed.IsInteger) {
                if (boxed.Type == BoxedValue.c_LongType)
                    return boxed.GetLong();
                else
                    return boxed.GetInt();
            }
            if (boxed.IsNumber) {
                if (boxed.Type == BoxedValue.c_DoubleType)
                    return boxed.GetDouble();
                else
                    return boxed.GetFloat();
            }
            if (boxed.IsString) {
                return boxed.AsString;
            }
            var obj = boxed.GetObject();
            if (null != obj) {
                if (obj is IList<BoxedValue> list) {
                    obj = GetListFromBoxedValue(list);
                }
                else if (obj is IDictionary<BoxedValue, BoxedValue> dict) {
                    obj = GetDictionaryFromBoxedValue(dict);
                }
            }
            return obj;
        }
        public static BoxedValue GetBoxedValueFromValue(object? value)
        {
            if (value is bool boolValue) {
                return BoxedValue.From(boolValue);
            }
            else if (value is int intValue) {
                return BoxedValue.From(intValue);
            }
            else if (value is long longValue) {
                return BoxedValue.From(longValue);
            }
            else if (value is float fltValue) {
                return BoxedValue.From(fltValue);
            }
            else if (value is double dblValue) {
                return BoxedValue.From(dblValue);
            }
            else if (value is string strValue) {
                return BoxedValue.FromString(strValue);
            }
            else if (value is System.Text.Json.JsonElement jsonElem) {
                switch (jsonElem.ValueKind) {
                    case System.Text.Json.JsonValueKind.Undefined:
                    case System.Text.Json.JsonValueKind.Null:
                        return BoxedValue.NullObject;
                    case System.Text.Json.JsonValueKind.Array:
                        return BoxedValue.FromObject(GetBoxedValueFromJsonList(jsonElem));
                    case System.Text.Json.JsonValueKind.Object:
                        return BoxedValue.FromObject(GetBoxedValueFromJsonDictionary(jsonElem));
                    case System.Text.Json.JsonValueKind.Number: {
                            var str = jsonElem.GetRawText();
                            if (TryParseNumeric(str, out var v)) {
                                return v;
                            }
                            else {
                                return BoxedValue.FromString(jsonElem.ToString());
                            }
                        }
                    case System.Text.Json.JsonValueKind.True:
                        return BoxedValue.From(true);
                    case System.Text.Json.JsonValueKind.False:
                        return BoxedValue.From(false);
                    case System.Text.Json.JsonValueKind.String:
                    default:
                        return BoxedValue.FromString(jsonElem.ToString());
                }
            }
            else if (value is LitJson.JsonData jsonData) {
                return GetBoxedValueFromJsonValue(jsonData);
            }
            else {
                if (null != value) {
                    if (value is IList<string?> strlist) {
                        return BoxedValue.FromObject(GetBoxedValueFromList(strlist));
                    }
                    else if (value is IDictionary<string, string?> strdict) {
                        return BoxedValue.FromObject(GetBoxedValueFromDictionary(strdict));
                    }
                    else if (value is IList<object?> list) {
                        return BoxedValue.FromObject(GetBoxedValueFromList(list));
                    }
                    else if (value is IDictionary<string, object?> dict) {
                        return BoxedValue.FromObject(GetBoxedValueFromDictionary(dict));
                    }
                }
                return BoxedValue.FromObject(value);
            }
        }
        public static IList<object?> GetListFromBoxedValue(IList<BoxedValue> list)
        {
            var newList = new List<object?>();
            foreach (var bv in list) {
                newList.Add(GetValueFromBoxedValue(bv));
            }
            return newList;
        }
        public static IDictionary<string, object?> GetDictionaryFromBoxedValue(IDictionary<BoxedValue, BoxedValue> dict)
        {
            var newDict = new Dictionary<string, object?>();
            foreach (var pair in dict) {
                var k = pair.Key;
                var v = GetValueFromBoxedValue(pair.Value);
                newDict.Add(k, v);
            }
            return newDict;
        }
        public static IList<BoxedValue> GetBoxedValueFromList(IList<object?> list)
        {
            var newList = new List<BoxedValue>();
            foreach (var v in list) {
                newList.Add(GetBoxedValueFromValue(v));
            }
            return newList;
        }
        public static IList<BoxedValue> GetBoxedValueFromList(IList<string?> list)
        {
            var newList = new List<BoxedValue>();
            foreach (var v in list) {
                newList.Add(GetBoxedValueFromValue(v));
            }
            return newList;
        }
        public static IDictionary<BoxedValue, BoxedValue> GetBoxedValueFromDictionary(IDictionary<string, object?> dict)
        {
            var newDict = new Dictionary<BoxedValue, BoxedValue>();
            foreach (var pair in dict) {
                var k = GetBoxedValueFromValue(pair.Key);
                var v = GetBoxedValueFromValue(pair.Value);
                newDict.Add(k, v);
            }
            return newDict;
        }
        public static IDictionary<BoxedValue, BoxedValue> GetBoxedValueFromDictionary(IDictionary<string, string?> dict)
        {
            var newDict = new Dictionary<BoxedValue, BoxedValue>();
            foreach (var pair in dict) {
                var k = GetBoxedValueFromValue(pair.Key);
                var v = GetBoxedValueFromValue(pair.Value);
                newDict.Add(k, v);
            }
            return newDict;
        }
        public static LitJson.JsonData? GetJsonValueFromBoxedValue(BoxedValue boxed)
        {
            if (boxed.IsNullObject) {
                return null;
            }
            if (boxed.IsBoolean) {
                return boxed.GetBool();
            }
            if (boxed.IsInteger) {
                if (boxed.Type == BoxedValue.c_LongType)
                    return boxed.GetLong();
                else
                    return boxed.GetInt();
            }
            if (boxed.IsNumber) {
                if (boxed.Type == BoxedValue.c_DoubleType)
                    return boxed.GetDouble();
                else
                    return boxed.GetFloat();
            }
            if (boxed.IsString) {
                return boxed.AsString;
            }
            var obj = boxed.GetObject();
            if (null != obj) {
                if (obj is IList<BoxedValue> list) {
                    return GetJsonListFromBoxedValue(list);
                }
                else if (obj is IDictionary<BoxedValue, BoxedValue> dict) {
                    return GetJsonDictionaryFromBoxedValue(dict);
                }
            }
            return new LitJson.JsonData(obj);
        }
        public static BoxedValue GetBoxedValueFromJsonValue(LitJson.JsonData? value)
        {
            if (null == value) {
                return BoxedValue.NullObject;
            }
            else if (value.IsString) {
                return BoxedValue.FromString((string)value);
            }
            else if (value.IsInt) {
                return BoxedValue.From((int)value);
            }
            else if (value.IsLong) {
                return BoxedValue.From((long)value);
            }
            else if (value.IsDouble) {
                return BoxedValue.From((double)value);
            }
            else if (value.IsBoolean) {
                return BoxedValue.FromBool((bool)value);
            }
            else if (value.IsArray) {
                return BoxedValue.FromObject(GetBoxedValueFromJsonList(value));
            }
            else if (value.IsObject) {
                return BoxedValue.FromObject(GetBoxedValueFromJsonDictionary(value));
            }
            else {
                return BoxedValue.FromObject(value);
            }
        }
        public static LitJson.JsonData GetJsonListFromBoxedValue(IList<BoxedValue> list)
        {
            var newList = new LitJson.JsonData();
            foreach (var bv in list) {
                newList.Add(GetJsonValueFromBoxedValue(bv));
            }
            return newList;
        }
        public static LitJson.JsonData GetJsonDictionaryFromBoxedValue(IDictionary<BoxedValue, BoxedValue> dict)
        {
            var newDict = new LitJson.JsonData();
            foreach (var pair in dict) {
                var k = GetJsonValueFromBoxedValue(pair.Key);
                var v = GetJsonValueFromBoxedValue(pair.Value);
                Debug.Assert(k != null);
                ((IDictionary)newDict).Add(k, v);
            }
            return newDict;
        }
        public static IList<BoxedValue> GetBoxedValueFromJsonList(LitJson.JsonData list)
        {
            var newList = new List<BoxedValue>();
            foreach (var v in list) {
                newList.Add(GetBoxedValueFromJsonValue((LitJson.JsonData)v));
            }
            return newList;
        }
        public static IDictionary<BoxedValue, BoxedValue> GetBoxedValueFromJsonDictionary(LitJson.JsonData dict)
        {
            var newDict = new Dictionary<BoxedValue, BoxedValue>();
            foreach (var key in dict.Keys) {
                var k = BoxedValue.FromString(key);
                var v = GetBoxedValueFromJsonValue(dict[key]);
                newDict.Add(k, v);
            }
            return newDict;
        }
        public static IList<BoxedValue> GetBoxedValueFromJsonList(System.Text.Json.JsonElement list)
        {
            var newList = new List<BoxedValue>();
            foreach (var v in list.EnumerateArray()) {
                newList.Add(GetBoxedValueFromValue(v));
            }
            return newList;
        }
        public static IDictionary<BoxedValue, BoxedValue> GetBoxedValueFromJsonDictionary(System.Text.Json.JsonElement dict)
        {
            var newDict = new Dictionary<BoxedValue, BoxedValue>();
            foreach (var prop in dict.EnumerateObject()) {
                var k = BoxedValue.FromString(prop.Name);
                var v = GetBoxedValueFromValue(prop.Value);
                newDict.Add(k, v);
            }
            return newDict;
        }
        public static bool TryParseNumeric(string str, out BoxedValue val)
        {
            string type = string.Empty;
            return TryParseNumeric(str, ref type, out val);
        }
        public static bool TryParseNumeric(string str, ref string type, out BoxedValue val)
        {
            bool ret = false;
            val = BoxedValue.NullObject;
            if (str.Length > 2 && str[0] == '0' && str[1] == 'x') {
                char c = str[str.Length - 1];
                if (c == 'u' || c == 'U') {
                    str = str.Substring(0, str.Length - 1);
                }
                if (ulong.TryParse(str.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.CurrentInfo, out var v)) {
                    str = v.ToString();
                }
                type = "uint";
            }
            else if (str.Length >= 2) {
                char c = str[str.Length - 1];
                if (c == 'u' || c == 'U') {
                    str = str.Substring(0, str.Length - 1);
                    type = "uint";
                }
                else if (c == 'f' || c == 'F') {
                    str = str.Substring(0, str.Length - 1);
                    c = str[str.Length - 1];
                    if (c == 'l' || c == 'L') {
                        str = str.Substring(0, str.Length - 1);
                    }
                    type = "float";
                }
            }
            if (type == "float" || type != "uint" && str.IndexOfAny(s_FloatExponent) > 0) {
                if (double.TryParse(str, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.CurrentInfo, out var v)) {
                    val.Set((float)v);
                    type = "float";
                    ret = true;
                }
            }
            else if (str.Length > 1 && str[0] == '0') {
                ulong v = Convert.ToUInt64(str, 8);
                val.Set((uint)v);
                type = "uint";
                ret = true;
            }
            else if (long.TryParse(str, out var lv)) {
                if (lv > int.MaxValue || type == "uint") {
                    val.Set((uint)lv);
                    type = "uint";
                }
                else {
                    val.Set((int)lv);
                    type = "int";
                }
                ret = true;
            }
            else if (ulong.TryParse(str, out var uv)) {
                val.Set(uv);
                type = "uint";
                ret = true;
            }
            if (!ret && TryParseBool(str, out var bv)) {
                val.Set(bv);
                type = "bool";
                ret = true;
            }
            return ret;
        }
        public static bool TryParseBool(string v, out bool val)
        {
            if (bool.TryParse(v, out val)) {
                return true;
            }
            else if (int.TryParse(v, out var ival)) {
                val = ival != 0;
                return true;
            }
            else if (v == "true") {
                val = true;
                return true;
            }
            else if (v == "false") {
                val = false;
                return true;
            }
            return false;
        }
        public static void AppendIndent(StringBuilder sb, int indent)
        {
            sb.Append(s_IndentString.Substring(0, indent * 4));
        }
        public static string s_IndentString = "                                                                                                                                                                                                                                                                ";
        public static char[] s_FloatExponent = new char[] { 'e', 'E', '.' };
    }
}
