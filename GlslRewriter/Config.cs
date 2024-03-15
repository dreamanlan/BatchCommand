using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static GlslRewriter.Config;
using System.Xml.Linq;
using BatchCommand;
using System.Collections;
using DslExpression;
using System.Transactions;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;
using Dsl;
using System.Numerics;

namespace GlslRewriter
{
    internal static class RenderDocImporter
    {
        internal static Dictionary<string, HlslExportInfo> ReadHlslMergeInfo(string hlslDataFile)
        {
            var results = new Dictionary<string, HlslExportInfo>();
            var lines = File.ReadAllLines(hlslDataFile);
            if (lines.Length >= 2) {
                var header = lines[0];
                var colNames = header.Split(",", StringSplitOptions.TrimEntries);
                if (colNames.Length >= 4 && colNames[0] == "Name" && colNames[1] == "Value" && colNames[3] == "Type") {
                    for (int ix = 1; ix < lines.Length; ++ix) {
                        var line = lines[ix];
                        var cols = SplitCsvLine(line);
                        Debug.Assert(colNames.Length == cols.Count);

                        var info = new HlslExportInfo { Name = cols[0], Value = cols[1], Type = cols[3] };
                        results[info.Name] = info;
                    }
                }
            }
            return results;
        }
        internal static List<string> GenenerateVsInOutAttr(string type, int index, Dictionary<string, string> attrMap, string csv_path, Dictionary<string, Dictionary<string, HlslExportInfo>> hlslMergeDatas, out Vector4 outPos)
        {
            outPos = Vector4.Zero;
            var results = new List<string>();
            var sb = new StringBuilder();
            var lines = File.ReadAllLines(csv_path);
            if (lines.Length >= 2) {
                var header = lines[0];
                var colNames = header.Split(",", StringSplitOptions.TrimEntries);
                if (colNames.Length >= 2 && colNames[0] == "VTX" && colNames[1] == "IDX") {
                    int ix = index + 1;
                    if (ix >= 0 && ix < lines.Length) {
                        var line = lines[ix];
                        var cols = SplitCsvLine(line);
                        Debug.Assert(colNames.Length == cols.Count);
                        for (int i = 2; i < colNames.Length && i < cols.Count; ++i) {
                            var names = colNames[i].Split(".");
                            if (names.Length == 2) {
                                bool skip = false;
                                if (attrMap.TryGetValue(names[0], out var newName)) {
                                    if (string.IsNullOrEmpty(newName)) {
                                        skip = true;
                                    }
                                    else {
                                        names[0] = newName;
                                    }
                                }
                                string vstr;
                                if (Config.TryMergeHlslAttr(names[0], names[1], hlslMergeDatas, out var attrData)) {
                                    vstr = attrData.Trim();
                                }
                                else {
                                    vstr = cols[i].Trim();
                                }
                                if (names[0] == "gl_Position") {
                                    if (names[1] == "x")
                                        outPos.X = float.Parse(vstr);
                                    else if (names[1] == "y")
                                        outPos.Y = float.Parse(vstr);
                                    else if (names[1] == "z")
                                        outPos.Z = float.Parse(vstr);
                                    else if (names[1] == "w")
                                        outPos.W = float.Parse(vstr);
                                }
                                if (!skip) {
                                    sb.Append(names[0]);
                                    sb.Append(".");
                                    sb.Append(names[1]);
                                    sb.Append(" = ");
                                    sb.Append(type);
                                    sb.Append("(");
                                    sb.Append(vstr);
                                    sb.Append(");");

                                    results.Add(sb.ToString());
                                    sb.Length = 0;
                                }
                            }
                            else {
                                Debug.Assert(false);
                            }
                        }
                    }

                    int startIdx = s_VertexStructInits.Count;
                    sb.Append("var vertexes = new[] {");
                    s_VertexStructInits.Add(startIdx, sb.ToString());
                    sb.Length = 0;

                    int start = s_VertexStructInits.Count;
                    bool first = true;
                    var idxHashset = new HashSet<int>();
                    for (ix = 1; ix < lines.Length; ++ix) {
                        var line = lines[ix];
                        var cols = SplitCsvLine(line);
                        Debug.Assert(colNames.Length == cols.Count);
                        if (int.TryParse(cols[1], out var idx) && !idxHashset.Contains(idx)) {
                            idxHashset.Add(idx);
                            sb.Append("\t");
                            if (s_VertexStructInits.Count > start) {
                                sb.Append(", ");
                            }
                            sb.Append("new VertexData(");
                            for (int i = 2; i < colNames.Length && i < cols.Count; ++i) {
                                var names = colNames[i].Split(".");
                                if (names.Length == 2 && names[1] == "x") {
                                    if (i > 2)
                                        sb.Append("), ");
                                    sb.Append("new Vector4(");
                                    if (Config.TryMergeHlslAttr(names[0], string.Empty, hlslMergeDatas, out var attrData)) {
                                        var vals = attrData.Split(",", StringSplitOptions.RemoveEmptyEntries);
                                        for (int aix = 0; aix < vals.Length; ++aix) {
                                            if (aix > 0)
                                                sb.Append(", ");
                                            sb.Append(vals[aix].Trim());
                                            sb.Append("f");
                                        }
                                        i += 3;
                                        continue;
                                    }
                                }
                                else if (i > 2)
                                    sb.Append(", ");
                                sb.Append(cols[i].Trim());
                                sb.Append("f");
                            }
                            sb.Append("))");
                            s_VertexStructInits.Add(startIdx + idx + 1, sb.ToString());
                            sb.Length = 0;

                            int j = 0;
                            sb.Append("\t");
                            for (int i = 2; i < colNames.Length && i < cols.Count; ++i) {
                                var names = colNames[i].Split(".");
                                if (names.Length == 2 && names[1] == "x") {
                                    if (i > 2) {
                                        sb.Append(")");
                                        var prevNames = colNames[i - 1].Split(".");
                                        if (!s_VertexAttrInits.TryGetValue(prevNames[0], out var attrs)) {
                                            attrs = new SortedList<int, string>();
                                            s_VertexAttrInits.Add(prevNames[0], attrs);
                                            attrs.Add(0, "var " + prevNames[0] + " = new[] {");
                                            first = true;
                                        }
                                        attrs.Add(idx + 1, sb.ToString());
                                        ++j;
                                        sb.Length = 0;
                                        sb.Append("\t");
                                    }
                                    if (first)
                                        first = false;
                                    else
                                        sb.Append(", ");
                                    sb.Append("new Vector4(");
                                    if (Config.TryMergeHlslAttr(names[0], string.Empty, hlslMergeDatas, out var attrData)) {
                                        var vals = attrData.Split(",", StringSplitOptions.RemoveEmptyEntries);
                                        for (int aix = 0; aix < vals.Length; ++aix) {
                                            if (aix > 0)
                                                sb.Append(", ");
                                            sb.Append(vals[aix].Trim());
                                            sb.Append("f");
                                        }
                                        i += 3;
                                        continue;
                                    }
                                }
                                else if (i > 2)
                                    sb.Append(", ");
                                sb.Append(cols[i].Trim());
                                sb.Append("f");
                            }
                            sb.Append(")");
                            var lastNames = colNames[colNames.Length - 1].Split(".");
                            if (!s_VertexAttrInits.TryGetValue(lastNames[0], out var attrs2)) {
                                attrs2 = new SortedList<int, string>();
                                s_VertexAttrInits.Add(lastNames[0], attrs2);
                                attrs2.Add(0, "var " + lastNames[0] + " = new[] {");
                            }
                            attrs2.Add(idx + 1, sb.ToString());
                            sb.Length = 0;
                        }
                    }

                    const string c_EndStr = "};";

                    sb.Append(c_EndStr);
                    s_VertexStructInits.Add(s_VertexStructInits.Count, sb.ToString());
                    sb.Length = 0;

                    foreach (var pair in s_VertexAttrInits) {
                        var list = pair.Value;
                        if (list.Count > 0 && list[list.Count - 1] != c_EndStr)
                            pair.Value.Add(pair.Value.Count, c_EndStr);
                    }
                }
            }
            return results;
        }
        internal static List<string> GenerateUniform(string type, HashSet<int> indexes, string csv_path, Dictionary<string, Dictionary<string, HlslExportInfo>> hlslMergeDatas)
        {
            var results = new List<string>();
            var sb = new StringBuilder();
            var lines = File.ReadAllLines(csv_path);
            if (lines.Length >= 2) {
                var header = lines[0];
                var colNames = header.Split(",", StringSplitOptions.TrimEntries);
                if (colNames.Length >= 2 && colNames[0] == "Name" && colNames[1] == "Value") {
                    for (int ix = 2; ix < lines.Length; ++ix) {
                        if (indexes.Count == 0 || indexes.Contains(ix - 2)) {
                            var line = lines[ix];
                            var cols = SplitCsvLine(line);
                            Debug.Assert(colNames.Length == cols.Count);
                            var firstCol = cols[0];
                            int si = firstCol.IndexOf('[');
                            var vname = firstCol.Substring(0, si).Trim();
                            int ei = firstCol.IndexOf("]");
                            var ixStr = firstCol.Substring(si + 1, ei - si - 1);
                            int.TryParse(ixStr, out var index);
                            if (firstCol.Length > 0 && firstCol[firstCol.Length - 1] == ']') {
                                sb.Append(cols[0]);
                                sb.Append(" = ");
                                bool hasHlslData = Config.TryMergeHlslData(vname, index, hlslMergeDatas, out var hlslData);
                                string vals = cols[1];
                                string dataType = type;
                                if (hasHlslData) {
                                    vals = hlslData.Value;
                                    dataType = hlslData.Type == "float4" ? "vec4" : "uvec4";
                                }
                                if (Program.s_DoReplacement && Config.ActiveConfig.TypeReplacements.TryGetValue(vname, out var newType)) {
                                    var vstrs = vals.Split(",", StringSplitOptions.TrimEntries);
                                    if (dataType == "vec4" && newType == "uint") {
                                        sb.Append("uvec4(");
                                        for (int i = 0; i < vstrs.Length; ++i) {
                                            if (i > 0)
                                                sb.Append(", ");
                                            float.TryParse(vstrs[i], out var v);
                                            sb.Append(Calculator.ftou(v).ToString());
                                        }
                                        sb.Append(");");
                                    }
                                    else if (dataType == "uvec4" && newType == "float") {
                                        sb.Append("vec4(");
                                        for (int i = 0; i < vstrs.Length; ++i) {
                                            if (i > 0)
                                                sb.Append(", ");
                                            uint.TryParse(vstrs[i], out var v);
                                            sb.Append(Calculator.FloatToString(Calculator.utof(v)));
                                        }
                                        sb.Append(");");
                                    }
                                }
                                else if (hasHlslData) {
                                    if ((type == "vec4" && hlslData.Type == "float4")
                                        || (type == "uvec4" && hlslData.Type == "uint4")) {
                                        sb.Append(type);
                                        sb.Append("(");
                                        sb.Append(vals);
                                        sb.Append(");");
                                    }
                                    else if (hlslData.Type == "float4") {
                                        sb.Append("uvec4(");
                                        var vstrs = vals.Split(",", StringSplitOptions.TrimEntries);
                                        for (int i = 0; i < vstrs.Length; ++i) {
                                            if (i > 0)
                                                sb.Append(", ");
                                            float.TryParse(vstrs[i], out var v);
                                            sb.Append(Calculator.ftou(v).ToString());
                                        }
                                        sb.Append(");");
                                    }
                                    else {
                                        sb.Append("vec4(");
                                        var vstrs = vals.Split(",", StringSplitOptions.TrimEntries);
                                        for (int i = 0; i < vstrs.Length; ++i) {
                                            if (i > 0)
                                                sb.Append(", ");
                                            uint.TryParse(vstrs[i], out var v);
                                            sb.Append(Calculator.FloatToString(Calculator.utof(v)));
                                        }
                                        sb.Append(");");
                                    }
                                }
                                else {
                                    sb.Append(type);
                                    sb.Append("(");
                                    sb.Append(vals);
                                    sb.Append(");");
                                }

                                results.Add(sb.ToString());
                                sb.Length = 0;

                                if (dataType == "vec4") {
                                    var vstrs = vals.Split(",", StringSplitOptions.TrimEntries);
                                    sb.Append("// ");
                                    sb.Append(cols[0]);
                                    sb.Append(" = ");
                                    sb.Append("new Vector4(");
                                    for (int i = 0; i < vstrs.Length; ++i) {
                                        if (i > 0)
                                            sb.Append(", ");
                                        float.TryParse(vstrs[i], out var v);
                                        sb.Append(Calculator.FloatToString(v));
                                        sb.Append("f");
                                    }
                                    sb.Append(");");
                                    s_UniformInits.Add(sb.ToString());
                                    sb.Length = 0;

                                    if (type == "uvec4") {
                                        sb.Append("// ");
                                        sb.Append(cols[0]);
                                        sb.Append(" = ");
                                        for (int i = 0; i < vstrs.Length; ++i) {
                                            sb.Append(", ");
                                            uint.TryParse(vstrs[i], out var v);
                                            sb.Append(v.ToString());
                                            sb.Append("u");
                                        }
                                        s_UniformRawInits.Add(sb.ToString());
                                        sb.Length = 0;

                                        if (Config.ActiveConfig.SettingInfo.NeedUniformUtofVals) {
                                            sb.Append("// ");
                                            sb.Append(cols[0]);
                                            sb.Append(" = ");
                                            sb.Append("vec4(");
                                            for (int i = 0; i < vstrs.Length; ++i) {
                                                if (i > 0)
                                                    sb.Append(", ");
                                                float.TryParse(vstrs[i], out var v);
                                                sb.Append(Calculator.FloatToString(v));
                                            }
                                            sb.Append(");");
                                            s_UniformUtofOrFtouVals.Add(sb.ToString());
                                            sb.Length = 0;
                                        }
                                    }
                                    else {
                                        if (Config.ActiveConfig.SettingInfo.NeedUniformFtouVals) {
                                            sb.Append("// ");
                                            sb.Append(cols[0]);
                                            sb.Append(" = ");
                                            sb.Append("uvec4(");
                                            for (int i = 0; i < vstrs.Length; ++i) {
                                                if (i > 0)
                                                    sb.Append(", ");
                                                float.TryParse(vstrs[i], out var v);
                                                sb.Append(Calculator.ftou(v).ToString());
                                            }
                                            sb.Append(");");
                                            s_UniformUtofOrFtouVals.Add(sb.ToString());
                                            sb.Length = 0;
                                        }
                                    }
                                }
                                else {
                                    var vstrs = vals.Split(",", StringSplitOptions.TrimEntries);
                                    sb.Append("// ");
                                    sb.Append(cols[0]);
                                    sb.Append(" = ");
                                    sb.Append("new Vector4(");
                                    for (int i = 0; i < vstrs.Length; ++i) {
                                        if (i > 0)
                                            sb.Append(", ");
                                        uint.TryParse(vstrs[i], out var v);
                                        sb.Append(Calculator.FloatToString(Calculator.utof(v)));
                                        sb.Append("f");
                                    }
                                    sb.Append(");");
                                    s_UniformInits.Add(sb.ToString());
                                    sb.Length = 0;

                                    if (type == "uvec4") {
                                        sb.Append("// ");
                                        sb.Append(cols[0]);
                                        sb.Append(" = ");
                                        for (int i = 0; i < vstrs.Length; ++i) {
                                            sb.Append(", ");
                                            uint.TryParse(vstrs[i], out var v);
                                            sb.Append(v.ToString());
                                            sb.Append("u");
                                        }
                                        s_UniformRawInits.Add(sb.ToString());
                                        sb.Length = 0;

                                        if (Config.ActiveConfig.SettingInfo.NeedUniformUtofVals) {
                                            sb.Append("// ");
                                            sb.Append(cols[0]);
                                            sb.Append(" = ");
                                            sb.Append("vec4(");
                                            for (int i = 0; i < vstrs.Length; ++i) {
                                                if (i > 0)
                                                    sb.Append(", ");
                                                uint.TryParse(vstrs[i], out var v);
                                                sb.Append(Calculator.FloatToString(Calculator.utof(v)));
                                            }
                                            sb.Append(");");
                                            s_UniformUtofOrFtouVals.Add(sb.ToString());
                                            sb.Length = 0;
                                        }
                                    }
                                    else {
                                        if (Config.ActiveConfig.SettingInfo.NeedUniformFtouVals) {
                                            sb.Append("// ");
                                            sb.Append(cols[0]);
                                            sb.Append(" = ");
                                            sb.Append("uvec4(");
                                            for (int i = 0; i < vstrs.Length; ++i) {
                                                if (i > 0)
                                                    sb.Append(", ");
                                                uint.TryParse(vstrs[i], out var v);
                                                sb.Append(v.ToString());
                                            }
                                            sb.Append(");");
                                            s_UniformUtofOrFtouVals.Add(sb.ToString());
                                            sb.Length = 0;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return results;
        }
        internal static List<string> GenerateVAO(string attr, string attrArrayLeft, string type, HashSet<int> indexes, string csv_path, Dictionary<string, Dictionary<string, HlslExportInfo>> hlslMergeDatas)
        {
            var results = new List<string>();
            var sb = new StringBuilder();
            var lines = File.ReadAllLines(csv_path);
            if (lines.Length >= 2) {
                var header = lines[0];
                var colNames = header.Split(",", StringSplitOptions.TrimEntries);
                if (colNames.Length >= 2 && colNames[0] == "Element") {
                    string[] realColNames;
                    if (colNames.Length > 5) {
                        int colNum = (colNames.Length - 1) / 4;
                        realColNames = new string[colNum];
                        for (int ix = 0; ix < colNum; ++ix) {
                            var colName = colNames[1 + ix * 4];
                            int si = colName.LastIndexOf('.');
                            if (si > 0) {
                                colName = colName.Substring(0, si);
                            }
                            realColNames[ix] = colName;
                        }
                    }
                    else {
                        realColNames = new string[1];
                        var colName = colNames[1];
                        int si = colName.LastIndexOf('.');
                        if (si > 0) {
                            colName = colName.Substring(0, si);
                        }
                        realColNames[0] = colName;
                    }
                    if (!string.IsNullOrEmpty(attr))
                        sb.AppendLine(attr);
                    sb.Append(attrArrayLeft);
                    sb.Append(" = ");
                    sb.Append("new[] {");
                    results.Add(sb.ToString());
                    sb.Length = 0;
                    int start = results.Count;
                    for (int ix = 1; ix < lines.Length; ++ix) {
                        if (indexes.Count == 0 || indexes.Contains(ix - 1)) {
                            var line = lines[ix];
                            var cols = SplitCsvLine(line);
                            Debug.Assert(colNames.Length == cols.Count);
                            sb.Append("\t");
                            if (results.Count > start) {
                                sb.Append(", ");
                            }
                            if (type == "float") {
                                if (Config.TryMergeHlslData(colNames[1], ix - 1, hlslMergeDatas, out var hlslData)) {
                                    if (hlslData.Type == "float") {
                                        sb.Append(hlslData.Value);
                                        sb.Append("f");
                                    }
                                    else {
                                        uint.TryParse(hlslData.Value, out var uval);
                                        float fval = Calculator.utof(uval);
                                        sb.Append(fval);
                                        sb.Append("u");
                                    }
                                }
                                else {
                                    sb.Append(cols[1]);
                                    sb.Append("f");
                                }
                            }
                            else {
                                int colNum = (colNames.Length - 1) / 4;
                                if (colNum > 1) {
                                    sb.Append("new[] {");
                                    for (int index = 1; index < cols.Count; ++index) {
                                        if ((index - 1) % 4 == 0) {
                                            if (index > 1)
                                                sb.Append("), ");
                                            sb.Append("new Vector4(");
                                            var colName = realColNames[(index - 1) / 4];
                                            if (Config.TryMergeHlslData(colName, ix - 1, hlslMergeDatas, out var hlslData)) {
                                                //VAO目前类型都是float，就不检查类型了
                                                var vals = hlslData.Value.Split(",", StringSplitOptions.TrimEntries);
                                                foreach (var v in vals) {
                                                    sb.Append(v);
                                                    sb.Append("f");
                                                }
                                                index += 3;
                                                continue;
                                            }
                                        }
                                        else if (index > 1)
                                            sb.Append(", ");
                                        sb.Append(cols[index]);
                                        sb.Append("f");
                                    }
                                    sb.Append(") }");
                                }
                                else {
                                    sb.Append("new Vector4(");
                                    var colName = realColNames[0];
                                    if (Config.TryMergeHlslData(colName, ix - 1, hlslMergeDatas, out var hlslData)) {
                                        //VAO目前类型都是float，就不检查类型了
                                        var vals = hlslData.Value.Split(",", StringSplitOptions.TrimEntries);
                                        foreach (var v in vals) {
                                            sb.Append(v);
                                            sb.Append("f");
                                        }
                                    }
                                    else {
                                        for (int index = 1; index < cols.Count; ++index) {
                                            if (index > 1)
                                                sb.Append(", ");
                                            sb.Append(cols[index]);
                                            sb.Append("f");
                                        }
                                    }
                                    sb.Append(")");
                                }
                            }
                            results.Add(sb.ToString());
                            sb.Length = 0;
                        }
                    }
                    sb.Append("};");
                    results.Add(sb.ToString());
                    sb.Length = 0;
                }
            }
            return results;
        }
        internal static List<string> GenerateSSBO(string attr, string attrArrayLeft, string type, string csv_path)
        {
            var results = new List<string>();
            var sb = new StringBuilder();
            var lines = File.ReadAllLines(csv_path);
            if (lines.Length >= 2) {
                var header = lines[0];
                var colNames = header.Split(",", StringSplitOptions.TrimEntries);
                if (colNames.Length >= 2 && colNames[0] == "Element") {
                    if (!string.IsNullOrEmpty(attr))
                        sb.AppendLine(attr);
                    sb.Append(attrArrayLeft);
                    sb.Append(" = ");
                    sb.Append("new[] {");
                    results.Add(sb.ToString());
                    sb.Length = 0;
                    int start = results.Count;
                    for (int ix = 1; ix < lines.Length; ++ix) {
                        var line = lines[ix];
                        var cols = SplitCsvLine(line);
                        Debug.Assert(colNames.Length == cols.Count);
                        sb.Append("\t");
                        if (results.Count > start) {
                            sb.Append(", ");
                        }
                        if (type == "int") {
                            sb.Append(cols[1]);
                            sb.Append(" //");
                            if (int.TryParse(cols[1], out var ival)) {
                                sb.Append(Calculator.itof(ival));
                            }
                        }
                        else if (type == "uint") {
                            sb.Append(cols[1]);
                            sb.Append("u");
                            sb.Append(" //");
                            if (uint.TryParse(cols[1], out var uval)) {
                                sb.Append(Calculator.utof(uval));
                            }
                        }
                        else if (type == "float") {
                            sb.Append(cols[1]);
                            sb.Append("f");
                            sb.Append(" //");
                            if (float.TryParse(cols[1], out var fval)) {
                                sb.Append(Calculator.ftou(fval));
                            }
                        }
                        else {
                            int colNum = (colNames.Length - 1) / 4;
                            if (colNum > 1) {
                                sb.Append("new[] {");
                                for (int index = 1; index < cols.Count; ++index) {
                                    if ((index - 1) % 4 == 0) {
                                        if (index > 1)
                                            sb.Append("), ");
                                        sb.Append("new Vector4(");
                                    }
                                    else if (index > 1)
                                        sb.Append(", ");
                                    sb.Append(cols[index]);
                                    sb.Append("f");
                                }
                                sb.Append(") }");
                                sb.Append(" //");
                                for (int index = 1; index < cols.Count; ++index) {
                                    if ((index - 1) % 4 == 0) {
                                        if (index > 1)
                                            sb.Append(" | ");
                                    }
                                    else if (index > 1)
                                        sb.Append(", ");
                                    float.TryParse(cols[index], out var fval);
                                    sb.Append(Calculator.ftou(fval));
                                }
                                for (int index = 1; index < cols.Count; ++index) {
                                    if (index > 1)
                                        sb.Append(", ");
                                    float.TryParse(cols[index], out var fval);
                                    sb.Append(Calculator.ftou(fval));
                                }
                            }
                            else {
                                sb.Append("new Vector4(");
                                for (int index = 1; index < cols.Count; ++index) {
                                    if (index > 1)
                                        sb.Append(", ");
                                    sb.Append(cols[index]);
                                    sb.Append("f");
                                }
                                sb.Append(")");
                                sb.Append(" //");
                                for (int index = 1; index < cols.Count; ++index) {
                                    if (index > 1)
                                        sb.Append(", ");
                                    float.TryParse(cols[index], out var fval);
                                    sb.Append(Calculator.ftou(fval));
                                }
                            }
                        }
                        results.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    sb.Append("};");
                    results.Add(sb.ToString());
                    sb.Length = 0;
                }
            }
            return results;
        }
        internal static bool ImportVsInOutData(string type, int index, Dictionary<string, string> attrMap, string csv_path)
        {
            bool ret = false;
            var lines = File.ReadAllLines(csv_path);
            if (lines.Length >= 2) {
                var header = lines[0];
                var colNames = header.Split(",", StringSplitOptions.TrimEntries);
                if (colNames.Length >= 2 && colNames[0] == "VTX" && colNames[1] == "IDX") {
                    int ix = index + 1;
                    if (ix >= 0 && ix < lines.Length) {
                        var line = lines[ix];
                        var cols = SplitCsvLine(line);
                        Debug.Assert(colNames.Length == cols.Count);
                        ret = true;
                        for (int i = 2; i < colNames.Length && i < cols.Count; ++i) {
                            var names = colNames[i].Split(".");
                            if (names.Length == 2) {
                                if (attrMap.TryGetValue(names[0], out var newName)) {
                                    if (string.IsNullOrEmpty(newName)) {
                                        continue;
                                    }
                                    names[0] = newName;
                                }
                                DslExpression.CalculatorValue val;
                                if (type == "float") {
                                    if (float.TryParse(cols[i], out var v)) {
                                        val = DslExpression.CalculatorValue.From(v);
                                    }
                                    else {
                                        val = DslExpression.CalculatorValue.From(0.0f);
                                    }
                                }
                                else if (type == "uint") {
                                    if (uint.TryParse(cols[i], out var v)) {
                                        val = DslExpression.CalculatorValue.From(v);
                                    }
                                    else {
                                        val = DslExpression.CalculatorValue.From(0u);
                                    }
                                }
                                else {
                                    if (int.TryParse(cols[i], out var v)) {
                                        val = DslExpression.CalculatorValue.From(v);
                                    }
                                    else {
                                        val = DslExpression.CalculatorValue.From(0);
                                    }
                                }
                                ret = VariableTable.TrySetObject(names[0], names[1], ref val) && ret;
                            }
                            else {
                                Debug.Assert(false);
                            }
                        }
                    }
                }
            }
            return ret;
        }
        private static List<string> SplitCsvLine(string line)
        {
            var cols = new List<string>();
            var sb = new StringBuilder();
            for (int i = 0; i < line.Length; ++i) {
                char c = line[i];
                if (c == ',') {
                    cols.Add(sb.ToString().Trim());
                    sb.Length = 0;
                }
                else if (c == '"') {
                    for (int j = i + 1; j < line.Length; ++j) {
                        char ch = line[j];
                        if (ch == '"') {
                            i = j;
                            break;
                        }
                        else {
                            sb.Append(ch);
                        }
                    }
                }
                else {
                    sb.Append(c);
                }
            }
            cols.Add(sb.ToString().Trim());
            return cols;
        }

        internal class HlslExportInfo
        {
            internal string Name = string.Empty;
            internal string Value = string.Empty;
            internal string Type = string.Empty;

            internal static HlslExportInfo s_EmptyInfo = new HlslExportInfo();
        }

        internal static List<string> s_UniformUtofOrFtouVals = new List<string>();
        internal static SortedList<int, string> s_VertexStructInits = new SortedList<int, string>();
        internal static SortedDictionary<string, SortedList<int, string>> s_VertexAttrInits = new SortedDictionary<string, SortedList<int, string>>();
        internal static List<string> s_UniformInits = new List<string>();
        internal static List<string> s_UniformRawInits = new List<string>();
    }
    internal static class Config
    {
        internal static void Reset()
        {
            s_ShaderConfigs.Clear();
            s_ActiveConfig = null;
        }
        internal static void LoadConfig(string cfgFilePath, string tmpDir)
        {
            Reset();
            var cfgFile = new Dsl.DslFile();
            if (cfgFile.Load(cfgFilePath, msg => { Console.WriteLine(msg); })) {
                foreach (var cfg in cfgFile.DslInfos) {
                    string id = cfg.GetId();
                    var fd = cfg as Dsl.FunctionData;
                    if (null != fd) {
                        if (id == "common") {
                            ParseCommonConfig(fd);
                        }
                        else if (id == "vs") {
                            AddShaderConfig(id, fd);
                        }
                        else if (id == "ps") {
                            AddShaderConfig(id, fd);
                        }
                        else if (id == "cs") {
                            AddShaderConfig(id, fd);
                        }
                        else if (id == "vs_code_block") {
                            ParseCodeBlock("vs", fd);
                        }
                        else if (id == "ps_code_block") {
                            ParseCodeBlock("ps", fd);
                        }
                        else if (id == "cs_code_block") {
                            ParseCodeBlock("cs", fd);
                        }
                    }
                }
            }

            if (Program.s_IsVsShader) {
                s_ShaderConfigs.TryGetValue("vs", out s_ActiveConfig);
            }
            else if (Program.s_IsPsShader) {
                s_ShaderConfigs.TryGetValue("ps", out s_ActiveConfig);
            }
            else if (Program.s_IsCsShader) {
                s_ShaderConfigs.TryGetValue("cs", out s_ActiveConfig);
            }
        }
        internal static bool TryMergeHlslData(string name, int index, Dictionary<string, Dictionary<string, RenderDocImporter.HlslExportInfo>> hlslFileDatas, out RenderDocImporter.HlslExportInfo hlslData)
        {
            hlslData = RenderDocImporter.HlslExportInfo.s_EmptyInfo;
            foreach (var hlslMerge in ActiveArgConfig.HlslMergeImports) {
                if (hlslFileDatas.TryGetValue(hlslMerge.File, out var hlslDatas)) {
                    foreach (var repInfo in hlslMerge.NameReplacements) {
                        if (("*" == repInfo.Item1 || name == repInfo.Item1) && ("*" == repInfo.Item2 || (int.TryParse(repInfo.Item2, out var v2) && index == v2))) {
                            var sb = new StringBuilder();
                            TransformNameReplacement(sb, repInfo.Item3, repInfo.Item4, name, index);
                            var newKeys = sb.ToString().Split("|", StringSplitOptions.RemoveEmptyEntries);
                            if (newKeys.Length == 1) {
                                if (hlslDatas.TryGetValue(newKeys[0], out var data)) {
                                    hlslData = data;
                                    return true;
                                }
                            }
                            else if (newKeys.Length > 1) {
                                string type = string.Empty;
                                sb.Length = 0;
                                for (int ix = 0; ix < newKeys.Length; ++ix) {
                                    var newKey = newKeys[ix];
                                    if (hlslDatas.TryGetValue(newKey, out var data)) {
                                        type = data.Type;
                                        if (ix > 0)
                                            sb.Append(",");
                                        sb.Append(data.Value);
                                    }
                                }
                                hlslData = new RenderDocImporter.HlslExportInfo { Name = repInfo.Item1, Value = sb.ToString(), Type = type + newKeys.Length.ToString() };
                                return true;
                            }
                            break;
                        }
                    }
                }
            }
            return false;
        }
        internal static bool TryMergeHlslAttr(string attr, string member, Dictionary<string, Dictionary<string, RenderDocImporter.HlslExportInfo>> hlslFileDatas, out string attrData)
        {
            attrData = string.Empty;
            foreach (var hlslMerge in ActiveArgConfig.HlslMergeImports) {
                if (hlslFileDatas.TryGetValue(hlslMerge.File, out var hlslDatas)) {
                    if (hlslMerge.AttrHlslMap.TryGetValue(attr, out var vname)) {
                        if(hlslDatas.TryGetValue(vname, out var info)) {
                            if (string.IsNullOrEmpty(member)) {
                                attrData = info.Value;
                                return true;
                            }
                            else {
                                var vals = info.Value.Split(",", StringSplitOptions.RemoveEmptyEntries);
                                if (member == "x" && vals.Length > 0) {
                                    attrData = vals[0];
                                    return true;
                                }
                                else if (member == "y" && vals.Length > 1) {
                                    attrData = vals[1];
                                    return true;
                                }
                                else if (member == "z" && vals.Length > 2) {
                                    attrData = vals[2];
                                    return true;
                                }
                                else if (member == "w" && vals.Length > 3) {
                                    attrData = vals[3];
                                    return true;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            return false;
        }
        internal static string TryReplaceType(string key, string type)
        {
            string ret = type;
            if (Program.s_DoReplacement && Config.ActiveConfig.TypeReplacements.TryGetValue(key, out var newType)) {
                string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
                if (arrNums.Count > 0) {
                    if (suffix.Length > 0) {
                        if (baseType == "vec") {
                            if (newType == "uint")
                                ret = type.Replace("vec", "uvec");
                            else if (newType == "int")
                                ret = type.Replace("vec", "ivec");
                        }
                        else if (baseType == "uvec") {
                            if (newType == "float")
                                ret = type.Replace("uvec", "vec");
                        }
                        else if (baseType == "ivec") {
                            if (newType == "float")
                                ret = type.Replace("ivec", "vec");
                        }
                    }
                    else {
                        ret = type.Replace(baseType, newType);
                    }
                }
                else {
                    if (suffix.Length > 0) {
                        if (baseType == "vec") {
                            if (newType == "uint")
                                ret = "uvec" + suffix;
                            else if (newType == "int")
                                ret = "ivec" + suffix;
                        }
                        else if (baseType == "uvec") {
                            if (newType == "float")
                                ret = "vec" + suffix;
                        }
                        else if (baseType == "ivec") {
                            if (newType == "float")
                                ret = "vec" + suffix;
                        }
                    }
                    else {
                        ret = newType;
                    }
                }
            }
            return ret;
        }
        internal static string TryReplaceString(string txt)
        {
            string ret = txt;
            if (Program.s_DoReplacement && Config.ActiveConfig.StringReplacements.Count > 0) {
                foreach (var info in Config.ActiveConfig.StringReplacements) {
                    if (null != info.SrcRegex) {
                        ret = info.SrcRegex.Replace(ret, info.Dst);
                    }
                    else {
                        ret = ret.Replace(info.Src, info.Dst);
                    }
                }
            }
            return ret;
        }
        internal static bool CalcSettingForVariable(Dsl.ISyntaxComponent varDsl, out bool isVariableSetting, out bool markValue, out bool markExpression, out int maxLevelForExp, out int maxLengthForExp, out bool multiline, out bool expandedOnlyOnce)
        {
            var settingInfo = ActiveConfig.SettingInfo;
            bool skipValue = ActiveConfig.SettingInfo.DefSkipValue;
            bool skipExpression = ActiveConfig.SettingInfo.DefSkipExpression;
            maxLevelForExp = settingInfo.DefMaxLevel;
            maxLengthForExp = settingInfo.DefMaxLength;
            multiline = settingInfo.DefMultiline;
            expandedOnlyOnce = settingInfo.DefExpandedOnlyOnce;
            isVariableSetting = false;

            markValue = !skipValue;
            markExpression = !skipExpression;

            var vd = varDsl as Dsl.ValueData;
            var fd = varDsl as Dsl.FunctionData;
            var sd = varDsl as Dsl.StatementData;
            if (null != sd) {
                fd = sd.Last.AsFunction;
                if (null == fd) {
                    vd = sd.Last.AsValue;
                }
            }
            if (null != vd) {
                string vname = vd.GetId();
                if (settingInfo.VariableSplitInfos.TryGetValue(vname, out var lvlInfo)) {
                    maxLevelForExp = lvlInfo.MaxLevel;
                    maxLengthForExp = lvlInfo.MaxLength;
                    multiline = lvlInfo.Multiline;
                    expandedOnlyOnce = lvlInfo.ExpandedOnlyOnce;

                    isVariableSetting = true;
                    markExpression = lvlInfo.MaxLevel >= 0;
                }
                if (Program.IsPhiVar(vname)) {
                    markValue = false;
                }
            }
            else if (null != fd) {
                if (fd.IsPeriodParamClass() && fd.IsHighOrder && fd.LowerOrderFunction.IsBracketParamClass()) {
                    string cid = fd.LowerOrderFunction.GetId();
                    string ixStr = fd.LowerOrderFunction.GetParamId(0);
                    string member = fd.GetParamId(0);
                    if (int.TryParse(ixStr, out var index) && settingInfo.ObjectArraySplitInfos.TryGetValue(cid, out var objArrLvlInfo)
                        && objArrLvlInfo.TryGetValue(index, out var objLvlInfo)
                        && objLvlInfo.TryGetValue(member, out var lvlInfo)) {
                        maxLevelForExp = lvlInfo.MaxLevel;
                        maxLengthForExp = lvlInfo.MaxLength;
                        multiline = lvlInfo.Multiline;
                        expandedOnlyOnce = lvlInfo.ExpandedOnlyOnce;

                        isVariableSetting = true;
                        markExpression = lvlInfo.MaxLevel >= 0;
                    }
                }
                else if (fd.IsPeriodParamClass()) {
                    string cid = fd.GetId();
                    string member = fd.GetParamId(0);
                    if (settingInfo.ObjectSplitInfos.TryGetValue(cid, out var objLvlInfo) && objLvlInfo.TryGetValue(member, out var lvlInfo)) {
                        maxLevelForExp = lvlInfo.MaxLevel;
                        maxLengthForExp = lvlInfo.MaxLength;
                        multiline = lvlInfo.Multiline;
                        expandedOnlyOnce = lvlInfo.ExpandedOnlyOnce;

                        isVariableSetting = true;
                        markExpression = lvlInfo.MaxLevel >= 0;
                    }
                }
                else if (fd.IsBracketParamClass()) {
                    string cid = fd.GetId();
                    string ixStr = fd.GetParamId(0);
                    if (int.TryParse(ixStr, out var index) && settingInfo.ArraySplitInfos.TryGetValue(cid, out var arrLvlInfo) && arrLvlInfo.TryGetValue(index, out var lvlInfo)) {
                        maxLevelForExp = lvlInfo.MaxLevel;
                        maxLengthForExp = lvlInfo.MaxLength;
                        multiline = lvlInfo.Multiline;
                        expandedOnlyOnce = lvlInfo.ExpandedOnlyOnce;

                        isVariableSetting = true;
                        markExpression = lvlInfo.MaxLevel >= 0;
                    }
                }
            }
            return markValue || markExpression;
        }
        internal static string CalcVariableExpression(Dsl.ISyntaxComponent varDsl, out string varName)
        {
            varName = string.Empty;

            var sb = new StringBuilder();
            var vd = varDsl as Dsl.ValueData;
            var fd = varDsl as Dsl.FunctionData;
            var sd = varDsl as Dsl.StatementData;
            if (null != sd) {
                fd = sd.Last.AsFunction;
                if (null == fd) {
                    vd = sd.Last.AsValue;
                }
            }
            if (null != vd) {
                string vname = vd.GetId();
                sb.Append(vname);

                varName = vname;
            }
            else if (null != fd) {
                if (fd.IsPeriodParamClass() && fd.IsHighOrder && fd.LowerOrderFunction.IsBracketParamClass()) {
                    string cid = fd.LowerOrderFunction.GetId();
                    string ixStr = fd.LowerOrderFunction.GetParamId(0);
                    string member = fd.GetParamId(0);
                    sb.Append(cid);
                    sb.Append("[");
                    sb.Append(ixStr);
                    sb.Append("].");
                    sb.Append(member);
                }
                else if (fd.IsPeriodParamClass()) {
                    string cid = fd.GetId();
                    string member = fd.GetParamId(0);
                    sb.Append(cid);
                    sb.Append(".");
                    sb.Append(member);
                }
                else if (fd.IsBracketParamClass()) {
                    string cid = fd.GetId();
                    string ixStr = fd.GetParamId(0);
                    sb.Append(cid);
                    sb.Append("[");
                    sb.Append(ixStr);
                    sb.Append("]");
                }
            }
            return sb.ToString();
        }
        internal static bool CalcFunc(string func, List<DslExpression.CalculatorValue> args, string resultType, Dictionary<int, int> argTypeConversion, out DslExpression.CalculatorValue val)
        {
            bool ret = false;
            if (func == "if" || func == "while") {
                val = args[0];
                ret = true;
            }
            else if (func == "for") {
                if (args.Count > 1) {
                    val = args[1];
                    ret = true;
                }
                else if (args.Count == 1) {
                    val = args[0];
                    ret = true;
                }
                else {
                    val = DslExpression.CalculatorValue.NullObject;
                    ret = true;
                }
            }
            else if (Calculator.CalcFunc(func, args, resultType, argTypeConversion, out val, out var supported)) {
                ret = true;
            }
            else if (ActiveConfig.Calculators.TryGetValue(func, out var infos)) {
                foreach (var info in infos) {
                    bool match = true;
                    for (int i = 0; i < args.Count && i < info.Args.Count; ++i) {
                        if (args[i].ToString() == info.Args[i] || info.Args[i] == "*") {
                        }
                        else {
                            match = false;
                            break;
                        }
                    }
                    if (match && null != info.OnGetValue) {
                        val = info.OnGetValue(args, resultType, argTypeConversion);
                        ret = true;
                        break;
                    }
                }
            }
            else if (!supported) {
                Console.WriteLine("api '{0}' is unsupported, please support it.", func);
            }
            return ret;
        }

        private static void ParseCommonConfig(Dsl.FunctionData dslCfg)
        {
            if (s_ShaderConfigs.Count > 0) {
                Console.WriteLine("[Error]: common must be the first config !");
            }
            else if (dslCfg.HaveStatement()) {
                foreach (var p in dslCfg.Params) {
                    var vd = p as Dsl.ValueData;
                    var fd = p as Dsl.FunctionData;
                    if (null != vd) {
                        string vid = vd.GetId();
                    }
                    else if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "=") {
                            string key = fd.GetParamId(0);
                            var val = DoCalc(fd.GetParam(1));
                        }
                        else if (fid == "viewport") {
                            var w = DoCalc(fd.GetParam(0));
                            var h = DoCalc(fd.GetParam(1));
                            if (Calculator.TryGetInt(w, out var iw) && Calculator.TryGetInt(h, out var ih)) {
                                CommonCfg.ViewportWidth = iw;
                                CommonCfg.ViewportHeight = ih;
                            }
                        }
                        else if (fid == "setting") {
                            ParseSetting(CommonCfg.SettingInfo, fd);
                        }
                        else if (fid == "type_replacement") {
                            ParseTypeReplacement(CommonCfg.TypeReplacements, fd);
                        }
                        else if (fid == "function_replacement") {
                            ParseFunctionReplacement(CommonCfg.FunctionReplacements, fd);
                        }
                        else if (fid == "string_replacement") {
                            ParseStringReplacement(CommonCfg.StringReplacements, fd);
                        }
                        else if (fid == "calculator") {
                            ParseCalculator(CommonCfg.Calculators, fd);
                        }
                    }
                }
            }
        }
        private static void AddShaderConfig(string shaderType, Dsl.FunctionData dslCfg)
        {
            s_CurMaxShaderArgId = -1;

            var cfgInfo = new ShaderConfig();
            cfgInfo.CopyFrom(CommonCfg);
            cfgInfo.ShaderType = shaderType;
            foreach (var p in dslCfg.Params) {
                string id = p.GetId();
                var pfd = p as Dsl.FunctionData;
                if (null != pfd) {
                    if (id == "setting") {
                        ParseSetting(cfgInfo.SettingInfo, pfd);
                    }
                    else if (id == "shader_arg") {
                        ParseShaderArg(cfgInfo, pfd);
                    }
                    else if (id == "type_replacement") {
                        ParseTypeReplacement(cfgInfo.TypeReplacements, pfd);
                    }
                    else if (id == "function_replacement") {
                        ParseFunctionReplacement(cfgInfo.FunctionReplacements, pfd);
                    }
                    else if (id == "string_replacement") {
                        ParseStringReplacement(cfgInfo.StringReplacements, pfd);
                    }
                    else if (id == "calculator") {
                        ParseCalculator(cfgInfo.Calculators, pfd);
                    }
                }
            }
            s_ShaderConfigs[shaderType] = cfgInfo;
        }
        private static void ParseCodeBlock(string shaderType, Dsl.FunctionData dslCfg)
        {
            if (s_ShaderConfigs.TryGetValue(shaderType, out var cfgInfo)) {
                var callCfg = dslCfg;
                if (dslCfg.IsHighOrder)
                    callCfg = dslCfg.LowerOrderFunction;

                string key = !callCfg.IsParenthesisParamClass() || callCfg.GetParamNum() <= 0 ? "glsl_global" : callCfg.GetParamId(0).Trim();

                if (dslCfg.HaveExternScript()) {
                    string code = dslCfg.GetParamId(0);

                    cfgInfo.CodeBlocks[key] = code;
                }
            }
            else {
                Console.WriteLine("[Error]: {0}_code_block must be defined after the {0} config !", shaderType);
            }
        }
        private static void ParseSetting(SettingInfo settingInfo, Dsl.FunctionData dslCfg)
        {
            if (dslCfg.HaveStatement()) {
                foreach (var p in dslCfg.Params) {
                    var vd = p as Dsl.ValueData;
                    var fd = p as Dsl.FunctionData;
                    if (null != vd) {
                        string vid = vd.GetId();
                        if (vid == "debug_mode") {
                            settingInfo.DebugMode = true;
                        }
                        else if (vid == "print_graph") {
                            settingInfo.PrintGraph = true;
                        }
                        else if (vid == "for_hlsl_shader") {
                            settingInfo.ForHlslShader = true;
                        }
                        else if (vid == "generate_expression_list") {
                            settingInfo.GenerateExpressionList = true;
                        }
                        else if (vid == "remove_duplicate_expression") {
                            settingInfo.RemoveDuplicateExpression = true;
                        }
                        else if (vid == "def_multiline") {
                            settingInfo.DefMultiline = true;
                        }
                        else if (vid == "def_expanded_only_once") {
                            settingInfo.DefExpandedOnlyOnce = true;
                        }
                        else if (vid == "def_skip_value") {
                            settingInfo.DefSkipValue = true;
                        }
                        else if (vid == "def_skip_expression") {
                            settingInfo.DefSkipExpression = true;
                        }
                    }
                    else if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "=") {
                            string key = fd.GetParamId(0);
                            var val = DoCalc(fd.GetParam(1));
                            settingInfo.SettingVariables[key] = val;

                            if (key == "need_uniform_utof_vals") {
                                if (Calculator.TryGetBool(val, out var v)) {
                                    settingInfo.NeedUniformUtofVals = v;
                                }
                            }
                            else if (key == "need_uniform_ftou_vals") {
                                if (Calculator.TryGetBool(val, out var v)) {
                                    settingInfo.NeedUniformFtouVals = v;
                                }
                            }
                            else if (key == "def_max_level") {
                                if (Calculator.TryGetInt(val, out var v) && v > 0) {
                                    settingInfo.DefMaxLevel = v;
                                }
                            }
                            else if (key == "def_max_length") {
                                if (Calculator.TryGetInt(val, out var v) && v > 0) {
                                    settingInfo.DefMaxLength = v;
                                }
                            }
                            else if (key == "def_max_level_for_variable") {
                                if (Calculator.TryGetInt(val, out var v) && v > 0) {
                                    SplitInfoForVariable.s_DefMaxLvl = v;
                                }
                            }
                            else if (key == "def_max_length_for_variable") {
                                if (Calculator.TryGetInt(val, out var v) && v > 0) {
                                    SplitInfoForVariable.s_DefMaxLen = v;
                                }
                            }
                            else if (key == "def_multiline_for_variable") {
                                if (Calculator.TryGetBool(val, out var v)) {
                                    SplitInfoForVariable.s_DefMultiline = v;
                                }
                            }
                            else if (key == "def_expanded_only_once_for_variable") {
                                if (Calculator.TryGetBool(val, out var v)) {
                                    SplitInfoForVariable.s_DefExpandOnce = v;
                                }
                            }
                            else if (key == "compute_graph_nodes_capacity") {
                                if (Calculator.TryGetInt(val, out var v)) {
                                    settingInfo.ComputeGraphNodesCapacity = v;
                                }
                            }
                            else if (key == "shader_variables_capacity") {
                                if (Calculator.TryGetInt(val, out var v)) {
                                    settingInfo.ShaderVariablesCapacity = v;
                                }
                            }
                            else if (key == "string_buffer_capacity_surplus") {
                                if (Calculator.TryGetInt(val, out var v)) {
                                    settingInfo.StringBufferCapacitySurplus = v;
                                }
                            }
                            else if (key == "max_iterations") {
                                if (Calculator.TryGetInt(val, out var v)) {
                                    settingInfo.MaxIterations = v;
                                }
                            }
                            else if (key == "max_loop") {
                                if (Calculator.TryGetInt(val, out var v)) {
                                    settingInfo.MaxLoop = v;
                                }
                            }
                        }
                        else if (fid == "split_variable_assignment") {
                            ParseSplitVariableAssignment(settingInfo, fd);
                        }
                        else if (fid == "split_object_assignment") {
                            ParseSplitObjectAssignment(settingInfo, fd);
                        }
                        else if (fid == "split_array_assignment") {
                            ParseSplitArrayAssignment(settingInfo, fd);
                        }
                        else if (fid == "split_object_array_assignment") {
                            ParseSplitObjectArrayAssignment(settingInfo, fd);
                        }
                        else if (fid == "auto_split") {
                            ParseAutoSplit(settingInfo, fd);
                        }
                        else if (fid == "unassignable_variable") {
                            ParseUnassignableVariable(settingInfo, fd);
                        }
                        else if (fid == "unassignable_object_member") {
                            ParseUnassignableObjectMember(settingInfo, fd);
                        }
                        else if (fid == "unassignable_array_element") {
                            ParseUnassignableArrayElement(settingInfo, fd);
                        }
                        else if (fid == "unassignable_object_array_member") {
                            ParseUnassignableObjectArrayMember(settingInfo, fd);
                        }
                        else if (fid == "variable_assignment") {
                            ParseVariableAssignment(settingInfo, fd);
                        }
                        else if (fid == "object_member_assignment") {
                            ParseObjectMemberAssignment(settingInfo, fd);
                        }
                        else if (fid == "array_element_assignment") {
                            ParseArrayElementAssignment(settingInfo, fd);
                        }
                        else if (fid == "object_array_member_assignment") {
                            ParseObjectArrayMemberAssignment(settingInfo, fd);
                        }
                        else if (fid == "add_utof") {
                            var val = DoCalc(fd.GetParam(0));
                            if (Calculator.TryGetUInt(val, out var uval)) {
                                var fvstr = Calculator.FloatToString(Calculator.utof(uval));
                                RenderDocImporter.s_UniformUtofOrFtouVals.Add("// " + val + " = " + fvstr + "f;");
                            }
                        }
                        else if (fid == "add_ftou") {
                            var val = DoCalc(fd.GetParam(0));
                            if (Calculator.TryGetFloat(val, out var fval)) {
                                var uvstr = Calculator.ftou(fval).ToString();
                                RenderDocImporter.s_UniformUtofOrFtouVals.Add("// " + val + " = " + uvstr + "u;");
                            }
                        }
                    }
                }
            }
        }
        private static void ParseSplitVariableAssignment(SettingInfo settingInfo, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var v1val = SplitInfoForVariable.s_DefMaxLvl;
                var v2val = SplitInfoForVariable.s_DefMaxLen;
                var v3val = SplitInfoForVariable.s_DefMultiline;
                var v4val = SplitInfoForVariable.s_DefExpandOnce;
                var vd = fp as Dsl.ValueData;
                var fd = fp as Dsl.FunctionData;
                if (null != vd) {
                    string key = vd.GetId();
                    settingInfo.AddSplitOnVariable(key, string.Empty);
                }
                else if (null != fd && fd.IsParenthesisParamClass()) {
                    string key = fd.GetParamId(0);

                    string sid = fd.GetId();
                    if (sid == "set") {
                        v1val = fd.GetParamNum() <= 1 ? SplitInfoForVariable.s_DefMaxLvl : DoCalc(fd.GetParam(1));
                        v2val = fd.GetParamNum() <= 2 ? SplitInfoForVariable.s_DefMaxLen : DoCalc(fd.GetParam(2));
                        v3val = fd.GetParamNum() <= 3 ? SplitInfoForVariable.s_DefMultiline : DoCalc(fd.GetParam(3));
                        v4val = fd.GetParamNum() <= 4 ? SplitInfoForVariable.s_DefExpandOnce : DoCalc(fd.GetParam(4));
                    }
                    else if (sid == "skip") {
                        v1val = "-1";
                    }
                    settingInfo.AddSplitOnVariable(key, v1val, v2val, v3val, v4val);
                }
            }
        }
        private static void ParseSplitObjectAssignment(SettingInfo settingInfo, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var v1val = SplitInfoForVariable.s_DefMaxLvl;
                var v2val = SplitInfoForVariable.s_DefMaxLen;
                var v3val = SplitInfoForVariable.s_DefMultiline;
                var v4val = SplitInfoForVariable.s_DefExpandOnce;
                var cd = fp as Dsl.FunctionData;
                if (null != cd && cd.IsParenthesisParamClass()) {
                    var fd = cd;
                    cd = fd.GetParam(0) as Dsl.FunctionData;

                    string sid = fd.GetId();
                    if (sid == "set") {
                        v1val = fd.GetParamNum() <= 1 ? SplitInfoForVariable.s_DefMaxLvl : DoCalc(fd.GetParam(1));
                        v2val = fd.GetParamNum() <= 2 ? SplitInfoForVariable.s_DefMaxLen : DoCalc(fd.GetParam(2));
                        v3val = fd.GetParamNum() <= 3 ? SplitInfoForVariable.s_DefMultiline : DoCalc(fd.GetParam(3));
                        v4val = fd.GetParamNum() <= 4 ? SplitInfoForVariable.s_DefExpandOnce : DoCalc(fd.GetParam(4));
                    }
                    else if (sid == "skip") {
                        v1val = "-1";
                    }
                }
                if (null != cd && cd.IsPeriodParamClass() && Calculator.TryGetInt(v1val, out var lvlForExp) && Calculator.TryGetInt(v2val, out var lenForExp)
                && Calculator.TryGetBool(v3val, out var ml) && Calculator.TryGetBool(v4val, out var once)) {
                    string cid = cd.GetId();
                    string member = cd.GetParamId(0);
                    if (!settingInfo.ObjectSplitInfos.TryGetValue(cid, out var objLvlInfo)) {
                        objLvlInfo = new Dictionary<string, SplitInfoForVariable>();
                        settingInfo.ObjectSplitInfos.Add(cid, objLvlInfo);
                    }
                    if (!objLvlInfo.TryGetValue(member, out var lvlInfo)) {
                        lvlInfo = new SplitInfoForVariable();
                        objLvlInfo.Add(member, lvlInfo);
                    }
                    lvlInfo.MaxLevel = lvlForExp;
                    lvlInfo.MaxLength = lenForExp;
                    lvlInfo.Multiline = ml;
                    lvlInfo.ExpandedOnlyOnce = once;
                }
            }
        }
        private static void ParseSplitArrayAssignment(SettingInfo settingInfo, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var v1val = SplitInfoForVariable.s_DefMaxLvl;
                var v2val = SplitInfoForVariable.s_DefMaxLen;
                var v3val = SplitInfoForVariable.s_DefMultiline;
                var v4val = SplitInfoForVariable.s_DefExpandOnce;
                var cd = fp as Dsl.FunctionData;
                if (null != cd && cd.IsParenthesisParamClass()) {
                    var fd = cd;
                    cd = fd.GetParam(0) as Dsl.FunctionData;

                    string sid = fd.GetId();
                    if (sid == "set") {
                        v1val = fd.GetParamNum() <= 1 ? SplitInfoForVariable.s_DefMaxLvl : DoCalc(fd.GetParam(1));
                        v2val = fd.GetParamNum() <= 2 ? SplitInfoForVariable.s_DefMaxLen : DoCalc(fd.GetParam(2));
                        v3val = fd.GetParamNum() <= 3 ? SplitInfoForVariable.s_DefMultiline : DoCalc(fd.GetParam(3));
                        v4val = fd.GetParamNum() <= 4 ? SplitInfoForVariable.s_DefExpandOnce : DoCalc(fd.GetParam(4));
                    }
                    else if (sid == "skip") {
                        v1val = "-1";
                    }
                }
                if (null != cd && cd.IsBracketParamClass() && Calculator.TryGetInt(v1val, out var lvlForExp) && Calculator.TryGetInt(v2val, out var lenForExp)
                && Calculator.TryGetBool(v3val, out var ml) && Calculator.TryGetBool(v4val, out var once)) {
                    string cid = cd.GetId();
                    var ixVal = DoCalc(cd.GetParam(0));
                    if (Calculator.TryGetInt(ixVal, out var index)) {
                        if (!settingInfo.ArraySplitInfos.TryGetValue(cid, out var arrLvlInfo)) {
                            arrLvlInfo = new Dictionary<int, SplitInfoForVariable>();
                            settingInfo.ArraySplitInfos.Add(cid, arrLvlInfo);
                        }
                        if (!arrLvlInfo.TryGetValue(index, out var lvlInfo)) {
                            lvlInfo = new SplitInfoForVariable();
                            arrLvlInfo.Add(index, lvlInfo);
                        }
                        lvlInfo.MaxLevel = lvlForExp;
                        lvlInfo.MaxLength = lenForExp;
                        lvlInfo.Multiline = ml;
                        lvlInfo.ExpandedOnlyOnce = once;
                    }
                }
            }
        }
        private static void ParseSplitObjectArrayAssignment(SettingInfo settingInfo, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var v1val = SplitInfoForVariable.s_DefMaxLvl;
                var v2val = SplitInfoForVariable.s_DefMaxLen;
                var v3val = SplitInfoForVariable.s_DefMultiline;
                var v4val = SplitInfoForVariable.s_DefExpandOnce;
                var cd = fp as Dsl.FunctionData;
                if (null != cd && cd.IsParenthesisParamClass()) {
                    var fd = cd;
                    cd = fd.GetParam(0) as Dsl.FunctionData;

                    string sid = fd.GetId();
                    if (sid == "set") {
                        v1val = fd.GetParamNum() <= 1 ? SplitInfoForVariable.s_DefMaxLvl : DoCalc(fd.GetParam(1));
                        v2val = fd.GetParamNum() <= 2 ? SplitInfoForVariable.s_DefMaxLen : DoCalc(fd.GetParam(2));
                        v3val = fd.GetParamNum() <= 3 ? SplitInfoForVariable.s_DefMultiline : DoCalc(fd.GetParam(3));
                        v4val = fd.GetParamNum() <= 4 ? SplitInfoForVariable.s_DefExpandOnce : DoCalc(fd.GetParam(4));
                    }
                    else if (sid == "skip") {
                        v1val = "-1";
                    }
                }
                if (null != cd && cd.IsPeriodParamClass() && cd.IsHighOrder && cd.LowerOrderFunction.IsBracketParamClass() && Calculator.TryGetInt(v1val, out var lvlForExp)
                && Calculator.TryGetInt(v2val, out var lenForExp) && Calculator.TryGetBool(v3val, out var ml) && Calculator.TryGetBool(v4val, out var once)) {
                    string cid = cd.LowerOrderFunction.GetId();
                    var ixVal = DoCalc(cd.LowerOrderFunction.GetParam(0));
                    string member = cd.GetParamId(0);
                    if (int.TryParse(ixVal, out var index)) {
                        if (!settingInfo.ObjectArraySplitInfos.TryGetValue(cid, out var objArrLvlInfo)) {
                            objArrLvlInfo = new Dictionary<int, Dictionary<string, SplitInfoForVariable>>();
                            settingInfo.ObjectArraySplitInfos.Add(cid, objArrLvlInfo);
                        }
                        if (!objArrLvlInfo.TryGetValue(index, out var objLvlInfo)) {
                            objLvlInfo = new Dictionary<string, SplitInfoForVariable>();
                            objArrLvlInfo.Add(index, objLvlInfo);
                        }
                        if (!objLvlInfo.TryGetValue(member, out var lvlInfo)) {
                            lvlInfo = new SplitInfoForVariable();
                            objLvlInfo.Add(member, lvlInfo);
                        }
                        lvlInfo.MaxLevel = lvlForExp;
                        lvlInfo.MaxLength = lenForExp;
                        lvlInfo.Multiline = ml;
                        lvlInfo.ExpandedOnlyOnce = once;
                    }
                }
            }
        }
        private static void ParseAutoSplit(SettingInfo settingInfo, Dsl.FunctionData dslCfg)
        {
            var callCfg = dslCfg;
            if (dslCfg.IsHighOrder)
                callCfg = dslCfg.LowerOrderFunction;

            var levelVal = !callCfg.IsParenthesisParamClass() || callCfg.GetParamNum() <= 0 ? SettingInfo.s_DefSplitLevel : DoCalc(callCfg.GetParam(0));
            if (Calculator.TryGetInt(levelVal, out var splitLevel)) {
                settingInfo.AutoSplitLevel = splitLevel;
            }

            foreach (var fp in dslCfg.Params) {
                var fd = fp as Dsl.FunctionData;
                if (null != fd) {
                    string sid = fd.GetId();
                    if (sid == "=") {
                        string key = fd.GetParamId(0);
                        var val = DoCalc(fd.GetParam(1));
                        if (key == "split_level_for_repeated_expression") {
                            if (Calculator.TryGetInt(val, out var v) && v > 0) {
                                settingInfo.AutoSplitLevelForRepeatedExpression = v;
                            }
                        }
                    }
                    else if (fd.IsParenthesisParamClass()) {
                        if (sid == "split_on") {
                            var v1val = fd.GetParamNum() <= 0 ? DslExpression.CalculatorValue.EmptyString : DoCalc(fd.GetParam(0));
                            var v2val = fd.GetParamNum() <= 1 ? SettingInfo.s_DefSplitOnLevel : DoCalc(fd.GetParam(1));
                            if (Calculator.TryGetString(v1val, out var v1str) && !string.IsNullOrEmpty(v1str) && Calculator.TryGetInt(v2val, out var lvl)) {
                                settingInfo.AutoSplitOnFuncs[v1str] = lvl;
                            }
                        }
                        else if (sid == "skip") {
                            var v1val = fd.GetParamNum() <= 0 ? DslExpression.CalculatorValue.EmptyString : DoCalc(fd.GetParam(0));
                            if (Calculator.TryGetString(v1val, out var v1str) && !string.IsNullOrEmpty(v1str)) {
                                if (!settingInfo.AutoSplitSkips.Contains(v1str)) {
                                    settingInfo.AutoSplitSkips.Add(v1str);
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void ParseVariableAssignment(SettingInfo settingInfo, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var fd = fp as Dsl.FunctionData;
                if (null != fd) {
                    string fid = fd.GetId();
                    if (fid == "=") {
                        string key = fd.GetParamId(0);
                        var val = DoCalc(fd.GetParam(1));
                        string valType = Calculator.GetValueType(val);

                        settingInfo.VariableAssignments[key] = new ValueInfo { Type = valType, Value = val };
                    }
                }
            }
        }
        private static void ParseObjectMemberAssignment(SettingInfo settingInfo, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var fd = fp as Dsl.FunctionData;
                if (null != fd) {
                    string fid = fd.GetId();
                    if (fid == "=") {
                        var cd = fd.GetParam(0) as Dsl.FunctionData;
                        var val = DoCalc(fd.GetParam(1));
                        string valType = Calculator.GetValueType(val);
                        var vinfo = new ValueInfo { Type = valType, Value = val };

                        if (null != cd && cd.IsPeriodParamClass()) {
                            string cid = cd.GetId();
                            string member = cd.GetParamId(0);
                            if (!settingInfo.ObjectMemberAssignments.TryGetValue(cid, out var members)) {
                                members = new Dictionary<string, ValueInfo>();
                                settingInfo.ObjectMemberAssignments.Add(cid, members);
                            }
                            members[member] = vinfo;
                        }
                    }
                }
            }
        }
        private static void ParseArrayElementAssignment(SettingInfo settingInfo, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var fd = fp as Dsl.FunctionData;
                if (null != fd) {
                    string fid = fd.GetId();
                    if (fid == "=") {
                        var cd = fd.GetParam(0) as Dsl.FunctionData;
                        var val = DoCalc(fd.GetParam(1));
                        string valType = Calculator.GetValueType(val);
                        var vinfo = new ValueInfo { Type = valType, Value = val };

                        if (null != cd && cd.IsBracketParamClass()) {
                            string cid = cd.GetId();
                            var ixVal = DoCalc(cd.GetParam(0));
                            if (Calculator.TryGetInt(ixVal, out var index)) {
                                if (!settingInfo.ArrayElementAssignments.TryGetValue(cid, out var elements)) {
                                    elements = new Dictionary<int, ValueInfo>();
                                    settingInfo.ArrayElementAssignments.Add(cid, elements);
                                }
                                elements[index] = vinfo;
                            }
                        }
                    }
                }
            }
        }
        private static void ParseObjectArrayMemberAssignment(SettingInfo settingInfo, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var fd = fp as Dsl.FunctionData;
                if (null != fd) {
                    string fid = fd.GetId();
                    if (fid == "=") {
                        var cd = fd.GetParam(0) as Dsl.FunctionData;
                        var val = DoCalc(fd.GetParam(1));
                        string valType = Calculator.GetValueType(val);
                        var vinfo = new ValueInfo { Type = valType, Value = val };


                        if (null != cd && cd.IsPeriodParamClass() && cd.IsHighOrder && cd.LowerOrderFunction.IsBracketParamClass()) {
                            string cid = cd.LowerOrderFunction.GetId();
                            var ixVal = DoCalc(cd.LowerOrderFunction.GetParam(0));
                            string member = cd.GetParamId(0);
                            if (Calculator.TryGetInt(ixVal, out var index)) {
                                if (!settingInfo.ObjectArrayMemberAssignments.TryGetValue(cid, out var objects)) {
                                    objects = new Dictionary<int, Dictionary<string, ValueInfo>>();
                                    settingInfo.ObjectArrayMemberAssignments.Add(cid, objects);
                                }
                                if (!objects.TryGetValue(index, out var objInfo)) {
                                    objInfo = new Dictionary<string, ValueInfo>();
                                    objects.Add(index, objInfo);
                                }
                                objInfo[member] = vinfo;
                            }
                        }
                    }
                }
            }
        }
        private static void ParseUnassignableVariable(SettingInfo settingInfo, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                AddUnassignableVariable(settingInfo, fp as Dsl.ValueData);
            }
        }
        private static void ParseUnassignableObjectMember(SettingInfo settingInfo, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var cd = fp as Dsl.FunctionData;
                AddUnassignableObjectMember(settingInfo, cd);
            }
        }
        private static void ParseUnassignableArrayElement(SettingInfo settingInfo, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var cd = fp as Dsl.FunctionData;
                AddUnassignableArrayElement(settingInfo, cd);
            }
        }
        private static void ParseUnassignableObjectArrayMember(SettingInfo settingInfo, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var cd = fp as Dsl.FunctionData;
                AddUnassignableObjectArrayMember(settingInfo, cd);
            }
        }
        private static void ParseShaderArg(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            if (dslCfg.HaveStatement()) {
                int cid;
                if (dslCfg.IsHighOrder) {
                    var callCfg = dslCfg.LowerOrderFunction;
                    var cidStr = callCfg.GetParamId(0).Trim();
                    int.TryParse(cidStr, out cid);
                    if (s_CurMaxShaderArgId < cid)
                        s_CurMaxShaderArgId = cid;
                }
                else {
                    ++s_CurMaxShaderArgId;
                    cid = s_CurMaxShaderArgId;
                }

                var info = new ShaderArgConfig();

                foreach (var p in dslCfg.Params) {
                    string fid = p.GetId();
                    var fd = p as Dsl.FunctionData;
                    if (null != fd) {
                        if (fid == "vs_attr") {
                            ParseInOutAttr(fid, info, fd);
                        }
                        else if (fid == "ps_attr") {
                            ParseInOutAttr(fid, info, fd);
                        }
                        else if (fid == "uniform") {
                            ParseUniformImport(info, fd);
                        }
                        else if (fid == "ssbo_attr") {
                            ParseSSBOImport(info, fd);
                        }
                        else if (fid == "vao_attr") {
                            ParseVAOImport(info, fd);
                        }
                        else if (fid == "hlsl_merge") {
                            ParseHlslMergeImport(info, fd);
                        }
                        else if (fid == "redirect") {
                            ParseRedirect(cfg, info, fd);
                        }
                    }
                }

                cfg.ArgConfigs.Add(cid, info);
            }
        }
        private static void ParseInOutAttr(string id, ShaderArgConfig cfg, Dsl.FunctionData dslCfg)
        {
            Dsl.FunctionData callCfg;
            if (dslCfg.HaveParam()) {
                callCfg = dslCfg;
            }
            else if (dslCfg.HaveStatement() && dslCfg.IsHighOrder) {
                callCfg = dslCfg.LowerOrderFunction;
            }
            else {
                return;
            }

            var info = cfg.InOutAttrInfo;
            if (id == "vs_attr") {
                string inAttr = callCfg.GetParamId(0).Trim();
                string outAttr = callCfg.GetParamId(1).Trim();
                var indexVal = DoCalc(callCfg.GetParam(2));
                if (Calculator.TryGetInt(indexVal, out var ix)) {
                    info.InAttrImportFile = inAttr;
                    info.OutAttrImportFile = outAttr;
                    info.AttrIndex = ix;
                }
            }
            else if (id == "ps_attr") {
                string inAttr = callCfg.GetParamId(0).Trim();
                var indexVal = DoCalc(callCfg.GetParam(1));
                if (Calculator.TryGetInt(indexVal, out var ix)) {
                    info.InAttrImportFile = inAttr;
                    info.AttrIndex = ix;
                }
            }

            if (dslCfg.HaveStatement()) {
                foreach (var p in dslCfg.Params) {
                    var fd = p as Dsl.FunctionData;
                    if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "map_in_attr") {
                            string oldAttr = fd.GetParamId(0);
                            string newAttr = fd.GetParamId(1);
                            info.InAttrMap[oldAttr] = newAttr;
                        }
                        else if (fid == "map_out_attr") {
                            string oldAttr = fd.GetParamId(0);
                            string newAttr = fd.GetParamId(1);
                            info.OutAttrMap[oldAttr] = newAttr;
                        }
                        else if (fid == "remove_in_attr") {
                            string oldAttr = fd.GetParamId(0);
                            info.InAttrMap[oldAttr] = string.Empty;
                        }
                        else if (fid == "remove_out_attr") {
                            string oldAttr = fd.GetParamId(0);
                            info.OutAttrMap[oldAttr] = string.Empty;
                        }
                    }
                }
            }
        }
        private static void ParseUniformImport(ShaderArgConfig cfg, Dsl.FunctionData dslCfg)
        {
            Dsl.FunctionData callCfg;
            if (dslCfg.HaveParam()) {
                callCfg = dslCfg;
            }
            else if (dslCfg.HaveStatement() && dslCfg.IsHighOrder) {
                callCfg = dslCfg.LowerOrderFunction;
            }
            else {
                return;
            }

            string file = callCfg.GetParamId(0).Trim();
            string type = callCfg.GetParamId(1).Trim();
            var info = new UniformImportInfo { File = file, Type = type };

            if (dslCfg.HaveStatement()) {
                foreach (var p in dslCfg.Params) {
                    var vd = p as Dsl.ValueData;
                    var fd = p as Dsl.FunctionData;
                    if (null != vd) {
                        var ixVal = DoCalc(vd);
                        if (Calculator.TryGetInt(ixVal, out var ix)) {
                            if (!info.UsedIndexes.Contains(ix))
                                info.UsedIndexes.Add(ix);
                        }
                    }
                    else if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "add") {
                            foreach (var fp in fd.Params) {
                                var ixVal = DoCalc(fp);
                                if (Calculator.TryGetInt(ixVal, out var ix)) {
                                    if (!info.UsedIndexes.Contains(ix))
                                        info.UsedIndexes.Add(ix);
                                }
                            }
                        }
                        else if (fid == "remove") {
                            foreach (var fp in fd.Params) {
                                var ixVal = DoCalc(fp);
                                if (Calculator.TryGetInt(ixVal, out var ix)) {
                                    info.UsedIndexes.Remove(ix);
                                }
                            }
                        }
                        else if (fid == "add_range") {
                            var ix1Val = DoCalc(fd.GetParam(0));
                            var ix2Val = DoCalc(fd.GetParam(1));
                            if (Calculator.TryGetInt(ix1Val, out var ix1) && Calculator.TryGetInt(ix2Val, out var ix2)) {
                                for (int i = ix1; i <= ix2; ++i) {
                                    if (!info.UsedIndexes.Contains(i))
                                        info.UsedIndexes.Add(i);
                                }
                            }
                        }
                        else if (fid == "remove_range") {
                            var ix1Val = DoCalc(fd.GetParam(0));
                            var ix2Val = DoCalc(fd.GetParam(1));
                            if (Calculator.TryGetInt(ix1Val, out var ix1) && Calculator.TryGetInt(ix2Val, out var ix2)) {
                                for (int i = ix1; i <= ix2; ++i) {
                                    info.UsedIndexes.Remove(i);
                                }
                            }
                        }
                    }
                }
            }

            cfg.UniformImports.Add(info);

        }
        private static void ParseSSBOImport(ShaderArgConfig cfg, Dsl.FunctionData dslCfg)
        {
            Dsl.FunctionData callCfg;
            if (dslCfg.HaveParam()) {
                callCfg = dslCfg;
            }
            else if (dslCfg.HaveStatement() && dslCfg.IsHighOrder) {
                callCfg = dslCfg.LowerOrderFunction;
            }
            else {
                return;
            }

            int num = callCfg.GetParamNum();
            var csArrayLeft = callCfg.GetParamId(0).Trim();
            string type = callCfg.GetParamId(1).Trim();
            string file = callCfg.GetParamId(2).Trim();
            string attr = string.Empty;
            if (num > 3) {
                attr = callCfg.GetParamId(3).Trim();
            }

            var info = new SSBOImportInfo { AttrArrayLeft = csArrayLeft, Type = type, File = file, Attr = attr };

            cfg.SSBOImports.Add(info);
        }
        private static void ParseVAOImport(ShaderArgConfig cfg, Dsl.FunctionData dslCfg)
        {
            Dsl.FunctionData callCfg;
            if (dslCfg.HaveParam()) {
                callCfg = dslCfg;
            }
            else if (dslCfg.HaveStatement() && dslCfg.IsHighOrder) {
                callCfg = dslCfg.LowerOrderFunction;
            }
            else {
                return;
            }

            int num = callCfg.GetParamNum();
            var csArrayLeft = callCfg.GetParamId(0).Trim();
            string type = callCfg.GetParamId(1).Trim();
            string file = callCfg.GetParamId(2).Trim();
            string attr = string.Empty;
            if (num > 3) {
                attr = callCfg.GetParamId(3).Trim();
            }

            var info = new VAOImportInfo { AttrArrayLeft = csArrayLeft, Type = type, File = file, Attr = attr };

            if (dslCfg.HaveStatement()) {
                foreach (var p in dslCfg.Params) {
                    var vd = p as Dsl.ValueData;
                    var fd = p as Dsl.FunctionData;
                    if (null != vd) {
                        var ixVal = DoCalc(vd);
                        if (Calculator.TryGetInt(ixVal, out var ix)) {
                            if (!info.UsedIndexes.Contains(ix))
                                info.UsedIndexes.Add(ix);
                        }
                    }
                    else if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "add") {
                            foreach (var fp in fd.Params) {
                                var ixVal = DoCalc(fp);
                                if (Calculator.TryGetInt(ixVal, out var ix)) {
                                    if (!info.UsedIndexes.Contains(ix))
                                        info.UsedIndexes.Add(ix);
                                }
                            }
                        }
                        else if (fid == "remove") {
                            foreach (var fp in fd.Params) {
                                var ixVal = DoCalc(fp);
                                if (Calculator.TryGetInt(ixVal, out var ix)) {
                                    info.UsedIndexes.Remove(ix);
                                }
                            }
                        }
                        else if (fid == "add_range") {
                            var ix1Val = DoCalc(fd.GetParam(0));
                            var ix2Val = DoCalc(fd.GetParam(1));
                            if (Calculator.TryGetInt(ix1Val, out var ix1) && Calculator.TryGetInt(ix2Val, out var ix2)) {
                                for (int i = ix1; i <= ix2; ++i) {
                                    if (!info.UsedIndexes.Contains(i))
                                        info.UsedIndexes.Add(i);
                                }
                            }
                        }
                        else if (fid == "remove_range") {
                            var ix1Val = DoCalc(fd.GetParam(0));
                            var ix2Val = DoCalc(fd.GetParam(1));
                            if (Calculator.TryGetInt(ix1Val, out var ix1) && Calculator.TryGetInt(ix2Val, out var ix2)) {
                                for (int i = ix1; i <= ix2; ++i) {
                                    info.UsedIndexes.Remove(i);
                                }
                            }
                        }
                    }
                }
            }

            cfg.VAOImports.Add(info);
        }
        private static void ParseHlslMergeImport(ShaderArgConfig cfg, Dsl.FunctionData dslCfg)
        {
            Dsl.FunctionData callCfg;
            if (dslCfg.HaveParam()) {
                callCfg = dslCfg;
            }
            else if (dslCfg.HaveStatement() && dslCfg.IsHighOrder) {
                callCfg = dslCfg.LowerOrderFunction;
            }
            else {
                return;
            }

            string file = callCfg.GetParamId(0).Trim();
            var info = new HlslMergeImportInfo { File = file };

            if (dslCfg.HaveStatement()) {
                foreach (var p in dslCfg.Params) {
                    var fd = p as Dsl.FunctionData;
                    var sd = p as Dsl.StatementData;
                    if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "name_replacement") {
                            var vname = fd.GetParamId(0);
                            var ixStr = fd.GetParamId(1);
                            var repName = fd.GetParam(2);
                            info.NameReplacements.Add(Tuple.Create(vname, ixStr, vname + "|" + ixStr, repName));
                        }
                        else if (fid == "attr_hlsl_map") {
                            string attr = fd.GetParamId(0);
                            string vname = fd.GetParamId(1);
                            info.AttrHlslMap[attr] = vname;
                        }
                    }
                    else if (null != sd) {
                        var firstFunc = sd.First.AsFunction;
                        var secondFunc = sd.Second.AsFunction;
                        if (null != firstFunc && null != secondFunc) {
                            string fid = firstFunc.GetId();
                            string sid = secondFunc.GetId();
                            if (fid == "name_replacement") {
                                if (sid == "for") {
                                    string stStr = secondFunc.GetParamId(0);
                                    string edStr = secondFunc.GetParamId(1);
                                    string incStr = secondFunc.GetParamNum() > 2 ? secondFunc.GetParamId(2) : "1";
                                    if(int.TryParse(stStr, out var st) &&
                                        int.TryParse(edStr, out var ed) &&
                                        int.TryParse(incStr, out var inc)) {
                                        if (st <= ed && inc > 0) {
                                            for(int ix = st; ix <= ed; ix += inc) {
                                                var vname = firstFunc.GetParamId(0);
                                                var ixStr = firstFunc.GetParamId(1);
                                                var repName = firstFunc.GetParam(2);
                                                var strIx = ix.ToString();
                                                info.NameReplacements.Add(Tuple.Create(vname.Replace("$iter", strIx), ixStr.Replace("$iter", strIx), vname + "|" + ixStr, repName));
                                            }
                                        }
                                        else if (st > ed && inc < 0) {
                                            for (int ix = st; ix >= ed; ix += inc) {
                                                var vname = firstFunc.GetParamId(0);
                                                var ixStr = firstFunc.GetParamId(1);
                                                var repName = firstFunc.GetParam(2);
                                                var strIx = ix.ToString();
                                                info.NameReplacements.Add(Tuple.Create(vname.Replace("$iter", strIx), ixStr.Replace("$iter", strIx), vname + "|" + ixStr, repName));
                                            }
                                        }
                                        else {
                                            Console.WriteLine("name_replacement(vname, ix, rep)for(start, end, inc), arg of for is illegal, line:{0} !", sd.GetLine());
                                        }
                                    }
                                    else {
                                        Console.WriteLine("name_replacement(vname, ix, rep)for(start, end, inc), arg of for must be integer, line:{0} !", sd.GetLine());
                                    }
                                }
                                else if (sid == "foreach") {
                                    foreach(var it in secondFunc.Params) {
                                        var vname = firstFunc.GetParamId(0);
                                        var ixStr = firstFunc.GetParamId(1);
                                        var repName = firstFunc.GetParam(2);
                                        var strIx = it.GetId();
                                        info.NameReplacements.Add(Tuple.Create(vname.Replace("$iter", strIx), ixStr.Replace("$iter", strIx), vname + "|" + ixStr, repName));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            cfg.HlslMergeImports.Add(info);
        }
        private static void ParseRedirect(ShaderConfig cfg, ShaderArgConfig argCfg, Dsl.FunctionData dslCfg)
        {
            Dsl.FunctionData callCfg;
            if (dslCfg.HaveParam()) {
                callCfg = dslCfg;

                int num = callCfg.GetParamNum();
                string baseCfg = callCfg.GetParamId(0).Trim();
                string newDir = callCfg.GetParamId(1).Trim();
                string newDirVAO = newDir;
                string newDirHlsl = newDir;
                if (num > 2) {
                    newDirVAO = callCfg.GetParamId(2).Trim();
                }
                if (num > 3) {
                    newDirHlsl = callCfg.GetParamId(3).Trim();
                }

                if (int.TryParse(baseCfg, out var baseCfgId)) {
                    if (cfg.ArgConfigs.TryGetValue(baseCfgId, out var baseCfgInfo)) {
                        argCfg.InOutAttrInfo.InAttrImportFile = ChangeDir(baseCfgInfo.InOutAttrInfo.InAttrImportFile, newDir);
                        argCfg.InOutAttrInfo.OutAttrImportFile = ChangeDir(baseCfgInfo.InOutAttrInfo.OutAttrImportFile, newDir);
                        argCfg.InOutAttrInfo.AttrIndex = baseCfgInfo.InOutAttrInfo.AttrIndex;
                        foreach (var pair in baseCfgInfo.InOutAttrInfo.InAttrMap) {
                            argCfg.InOutAttrInfo.InAttrMap.Add(pair.Key, pair.Value);
                        }
                        foreach (var pair in baseCfgInfo.InOutAttrInfo.OutAttrMap) {
                            argCfg.InOutAttrInfo.OutAttrMap.Add(pair.Key, pair.Value);
                        }
                        foreach (var uniform in baseCfgInfo.UniformImports) {
                            var newCfg = new UniformImportInfo();
                            newCfg.File = ChangeDir(uniform.File, newDir);
                            newCfg.Type = uniform.Type;
                            foreach (var ix in uniform.UsedIndexes) {
                                newCfg.UsedIndexes.Add(ix);
                            }
                            argCfg.UniformImports.Add(newCfg);
                        }
                        foreach (var ssbo in baseCfgInfo.SSBOImports) {
                            var newCfg = new SSBOImportInfo();
                            newCfg.File = ChangeDir(ssbo.File, newDir);
                            newCfg.Type = ssbo.Type;
                            newCfg.Attr = ssbo.Attr;
                            newCfg.AttrArrayLeft = ssbo.AttrArrayLeft;
                            argCfg.SSBOImports.Add(newCfg);
                        }
                        foreach (var vao in baseCfgInfo.VAOImports) {
                            var newCfg = new VAOImportInfo();
                            newCfg.File = ChangeDir(vao.File, newDirVAO);
                            newCfg.Type = vao.Type;
                            newCfg.Attr = vao.Attr;
                            newCfg.AttrArrayLeft = vao.AttrArrayLeft;
                            foreach (var ix in vao.UsedIndexes) {
                                newCfg.UsedIndexes.Add(ix);
                            }
                            argCfg.VAOImports.Add(newCfg);
                        }
                        foreach (var hlslMerge in baseCfgInfo.HlslMergeImports) {
                            var newCfg = new HlslMergeImportInfo();
                            newCfg.File = ChangeDir(hlslMerge.File, newDirHlsl);
                            foreach (var repInfo in hlslMerge.NameReplacements) {
                                newCfg.NameReplacements.Add(repInfo);
                            }
                            foreach (var pair in hlslMerge.AttrHlslMap) {
                                newCfg.AttrHlslMap.Add(pair.Key, pair.Value);
                            }
                            argCfg.HlslMergeImports.Add(newCfg);
                        }
                    }
                    else {
                        Console.WriteLine("base shader arg '{0}' can't be found.", baseCfgId);
                    }
                }
            }
        }
        private static string ChangeDir(string path, string newDir)
        {
            string ret = path;
            if (!string.IsNullOrEmpty(path)) {
                string fn = Path.GetFileName(path);
                ret = Path.Combine(newDir, fn);
            }
            return ret;
        }

        private static void ParseTypeReplacement(Dictionary<string, string> typeReplacements, Dsl.FunctionData dslCfg)
        {
            if (dslCfg.HaveStatement()) {
                foreach (var p in dslCfg.Params) {
                    var fd = p as Dsl.FunctionData;
                    if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "=") {
                            var vname = fd.GetParamId(0);
                            var newType = fd.GetParamId(1);
                            typeReplacements.Add(vname, newType);
                        }
                    }
                }
            }
        }
        private static void ParseFunctionReplacement(Dictionary<string, List<FunctionReplacement>> functionReplacements, Dsl.FunctionData dslCfg)
        {
            if (dslCfg.HaveStatement()) {
                foreach (var p in dslCfg.Params) {
                    var fd = p as Dsl.FunctionData;
                    var sd = p as Dsl.StatementData;
                    if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "=") {
                            var funcMatch = fd.GetParam(0);
                            var dstFunc = fd.GetParam(1);
                            ParseFunctionReplacementInfo(functionReplacements, funcMatch, dstFunc, string.Empty);
                        }
                    }
                    else if (null != sd) {
                        var firstFunc = sd.First.AsFunction;
                        var secondFunc = sd.Second.AsFunction;
                        if (null != firstFunc && null != secondFunc) {
                            string fid = firstFunc.GetId();
                            string sid = secondFunc.GetId();
                            if (fid == "replacement") {
                                if (sid == "for") {
                                    string stStr = secondFunc.GetParamId(0);
                                    string edStr = secondFunc.GetParamId(1);
                                    string incStr = secondFunc.GetParamNum() > 2 ? secondFunc.GetParamId(2) : "1";
                                    if (int.TryParse(stStr, out var st) &&
                                        int.TryParse(edStr, out var ed) &&
                                        int.TryParse(incStr, out var inc)) {
                                        if (st <= ed && inc > 0) {
                                            for (int ix = st; ix <= ed; ix += inc) {
                                                var funcMatch = firstFunc.GetParam(0);
                                                var dstFunc = firstFunc.GetParam(1);
                                                var strIx = ix.ToString();
                                                ParseFunctionReplacementInfo(functionReplacements, funcMatch, dstFunc, strIx);
                                            }
                                        }
                                        else if (st > ed && inc < 0) {
                                            for (int ix = st; ix >= ed; ix += inc) {
                                                var funcMatch = firstFunc.GetParam(0);
                                                var dstFunc = firstFunc.GetParam(1);
                                                var strIx = ix.ToString();
                                                ParseFunctionReplacementInfo(functionReplacements, funcMatch, dstFunc, strIx);
                                            }
                                        }
                                        else {
                                            Console.WriteLine("function replacement(vname, ix, rep)for(start, end, inc), arg of for is illegal, line:{0} !", sd.GetLine());
                                        }
                                    }
                                    else {
                                        Console.WriteLine("function replacement(vname, ix, rep)for(start, end, inc), arg of for must be integer, line:{0} !", sd.GetLine());
                                    }
                                }
                                else if (sid == "foreach") {
                                    foreach (var it in secondFunc.Params) {
                                        var funcMatch = firstFunc.GetParam(0);
                                        var dstFunc = firstFunc.GetParam(1);
                                        var strIx = it.GetId();
                                        ParseFunctionReplacementInfo(functionReplacements, funcMatch, dstFunc, strIx);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void ParseFunctionReplacementInfo(Dictionary<string, List<FunctionReplacement>> functionReplacements, Dsl.ISyntaxComponent funcMatch, Dsl.ISyntaxComponent dstFunc, string iter)
        {
            var info = new FunctionReplacement();
            ParseFunctionMatchInfo(info, funcMatch, "function_replacement", iter);
            info.Replacement = dstFunc;

            if (!functionReplacements.TryGetValue(info.FuncOrOper, out var infos)) {
                infos = new List<FunctionReplacement>();
                functionReplacements.Add(info.FuncOrOper, infos);
            }
            infos.Add(info);
        }
        private static void ParseStringReplacement(List<StringReplacement> stringReplacements, Dsl.FunctionData dslCfg)
        {
            if (dslCfg.HaveStatement()) {
                foreach (var p in dslCfg.Params) {
                    var fd = p as Dsl.FunctionData;
                    if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "string") {
                            var src = fd.GetParamId(0);
                            var dst = fd.GetParamId(1);
                            var info = new StringReplacement { Src = src, Dst = dst };
                            stringReplacements.Add(info);
                        }
                        else if (fid == "regex") {
                            var src = fd.GetParamId(0);
                            var dst = fd.GetParamId(1);
                            var regex = new Regex(src, RegexOptions.Compiled);
                            var info = new StringReplacement { Src = src, Dst = dst, SrcRegex = regex };
                            stringReplacements.Add(info);
                        }
                    }
                }
            }
        }
        private static void ParseCalculator(Dictionary<string, List<CalculatorInfo>> calculators, Dsl.FunctionData dslCfg)
        {
            if (dslCfg.HaveStatement()) {
                foreach (var p in dslCfg.Params) {
                    var fd = p as Dsl.FunctionData;
                    var sd = p as Dsl.StatementData;
                    if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "=") {
                            var funcMatch = fd.GetParam(0);
                            var val = fd.GetParam(1);
                            ParseCalculatorInfo(calculators, funcMatch, val, string.Empty);
                        }
                    }
                    else if (null != sd) {
                        var firstFunc = sd.First.AsFunction;
                        var secondFunc = sd.Second.AsFunction;
                        if (null != firstFunc && null != secondFunc) {
                            string fid = firstFunc.GetId();
                            string sid = secondFunc.GetId();
                            if (fid == "replacement") {
                                if (sid == "for") {
                                    string stStr = secondFunc.GetParamId(0);
                                    string edStr = secondFunc.GetParamId(1);
                                    string incStr = secondFunc.GetParamNum() > 2 ? secondFunc.GetParamId(2) : "1";
                                    if (int.TryParse(stStr, out var st) &&
                                        int.TryParse(edStr, out var ed) &&
                                        int.TryParse(incStr, out var inc)) {
                                        if (st <= ed && inc > 0) {
                                            for (int ix = st; ix <= ed; ix += inc) {
                                                var funcMatch = firstFunc.GetParam(0);
                                                var val = firstFunc.GetParam(1);
                                                var strIx = ix.ToString();
                                                ParseCalculatorInfo(calculators, funcMatch, val, strIx);
                                            }
                                        }
                                        else if (st > ed && inc < 0) {
                                            for (int ix = st; ix >= ed; ix += inc) {
                                                var funcMatch = firstFunc.GetParam(0);
                                                var val = firstFunc.GetParam(1);
                                                var strIx = ix.ToString();
                                                ParseCalculatorInfo(calculators, funcMatch, val, strIx);
                                            }
                                        }
                                        else {
                                            Console.WriteLine("calculator replacement(vname, ix, rep)for(start, end, inc), arg of for is illegal, line:{0} !", sd.GetLine());
                                        }
                                    }
                                    else {
                                        Console.WriteLine("calculator replacement(vname, ix, rep)for(start, end, inc), arg of for must be integer, line:{0} !", sd.GetLine());
                                    }
                                }
                                else if (sid == "foreach") {
                                    foreach (var it in secondFunc.Params) {
                                        var funcMatch = firstFunc.GetParam(0);
                                        var val = firstFunc.GetParam(1);
                                        var strIx = it.GetId();
                                        ParseCalculatorInfo(calculators, funcMatch, val, strIx);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void ParseCalculatorInfo(Dictionary<string, List<CalculatorInfo>> calculators, Dsl.ISyntaxComponent funcMatch, Dsl.ISyntaxComponent val, string iter)
        {
            var info = new CalculatorInfo();
            ParseFunctionMatchInfo(info, funcMatch, "calculator", iter);
            var vd = val as Dsl.ValueData;
            var fd = val as Dsl.FunctionData;
            if (null != vd) {
                info.OnGetValue = (args, resultType, argTypeConversion) => { return vd.GetId(); };
            }
            else if (null != fd) {
                if (fd.GetId() == "func" && fd.IsHighOrder && fd.HaveStatement()) {
                    //这种形式用于直接调dsl脚本，构造一个函数
                    var callData = fd.LowerOrderFunction;
                    List<string> argNames = new List<string>();
                    foreach (var p in callData.Params) {
                        argNames.Add(p.GetId());
                    }
                    string funcId = BatchScript.EvalAsFunc(fd, argNames);
                    info.OnGetValue = (args, resultType, argTypeConversion) => {
                        return BatchScript.Call(funcId, CalculatorValue.FromObject(args), resultType, CalculatorValue.FromObject(argTypeConversion));
                    };
                }
                else {
                    info.OnGetValue = (args, resultType, argTypeConversion) => {
                        return DoCalc(fd, resultType);
                    };
                }
            }

            if (!calculators.TryGetValue(info.FuncOrOper, out var infos)) {
                infos = new List<CalculatorInfo>();
                calculators.Add(info.FuncOrOper, infos);
            }
            infos.Add(info);
        }
        private static void ParseFunctionMatchInfo(FunctionMatchInfo info, Dsl.ISyntaxComponent funcMatch, string tag, string iter)
        {
            var val = funcMatch as Dsl.ValueData;
            if (null != val) {
                info.FuncOrOper = val.GetId();
                info.ArgGetter = ComputeGraphCalcNode.GetArgForFuncOrOper;
            }
            else {
                var func = funcMatch as Dsl.FunctionData;
                if (null != func) {
                    if (!func.HaveId() && func.IsParenthesisParamClass()) {
                        if (func.GetParamNum() == 1) {
                            ParseFunctionMatchInfo(info, func.GetParam(0), tag, iter);
                        }
                        else {
                            Console.WriteLine("{0}: () left hand, () have and only have one argument !", tag);
                        }
                    }
                    else {
                        if (func.IsPeriodParamClass()) {
                            if (func.IsHighOrder && func.LowerOrderFunction.IsBracketParamClass()) {
                                var bracketObj = func.LowerOrderFunction;
                                string cid = ConvertMatchString(bracketObj.IsHighOrder ? bracketObj.LowerOrderFunction.GetParamId(0) : bracketObj.GetId(), iter);
                                string ixStr = GetParamMatchString(bracketObj.GetParam(0), iter);
                                string member = GetParamMatchString(func.GetParam(0), iter);

                                info.FuncOrOper = "[].";
                                info.ArgGetter = ComputeGraphCalcNode.GetArgForArrayMember;
                                info.Args.Add(cid);
                                info.Args.Add(ixStr);
                                info.Args.Add(member);
                            }
                            else {
                                string cid = ConvertMatchString(func.IsHighOrder ? func.LowerOrderFunction.GetParamId(0) : func.GetId(), iter);
                                string member = GetParamMatchString(func.GetParam(0), iter);

                                info.FuncOrOper = ".";
                                info.ArgGetter = ComputeGraphCalcNode.GetArgForMember;
                                info.Args.Add(cid);
                                info.Args.Add(member);
                            }
                        }
                        else if (func.IsBracketParamClass()) {
                            string cid = ConvertMatchString(func.IsHighOrder ? func.LowerOrderFunction.GetParamId(0) : func.GetId(), iter);
                            string ixStr = GetParamMatchString(func.GetParam(0), iter);

                            info.FuncOrOper = "[]";
                            info.ArgGetter = ComputeGraphCalcNode.GetArgForArray;
                            info.Args.Add(cid);
                            info.Args.Add(ixStr);
                        }
                        else if (func.IsOperatorParamClass()) {
                            string op = func.GetId();
                            int num = func.GetParamNum();
                            if (num == 1) {
                                var p = func.GetParam(0);
                                var pfd = p as Dsl.FunctionData;
                                info.FuncOrOper = op;
                                info.ArgGetter = ComputeGraphCalcNode.GetArgForFuncOrOper;
                                info.Args.Add(GetParamMatchString(p, iter));
                            }
                            else if (num == 2) {
                                var p1 = func.GetParam(0);
                                var p2 = func.GetParam(1);

                                info.FuncOrOper = op;
                                info.ArgGetter = ComputeGraphCalcNode.GetArgForFuncOrOper;
                                info.Args.Add(GetParamMatchString(p1, iter));
                                info.Args.Add(GetParamMatchString(p2, iter));
                            }
                            else {
                                Console.WriteLine("{0}: operator {1} left hand, invalid argument number !", tag, op);
                            }
                        }
                        else if (func.IsParenthesisParamClass()) {
                            info.FuncOrOper = func.GetId();
                            info.ArgGetter = ComputeGraphCalcNode.GetArgForFuncOrOper;
                            foreach (var p in func.Params) {
                                info.Args.Add(GetParamMatchString(p, iter));
                            }
                        }
                        else {
                            Console.WriteLine("{0}: {1} left hand, invalid parenthesis !", tag, func.GetId());
                        }
                    }
                }
                else {
                    var stm = funcMatch as Dsl.StatementData;
                    if (null != stm) {
                        var f1 = stm.First.AsFunction;
                        var f2 = stm.Second.AsFunction;
                        if (null != f1 && null != f2 && f1.GetId() == "?" && f1.IsHighOrder && f1.IsTernaryOperatorParamClass()
                            && f1.LowerOrderFunction.GetParamNum() == 1 && f1.GetParamNum() == 1
                            && f2.GetId() == ":" && f2.IsTernaryOperatorParamClass() && f2.GetParamNum() == 1) {
                            var condExp = f1.LowerOrderFunction.GetParam(0);
                            var trueExp = f1.GetParam(0);
                            var falseExp = f2.GetParam(0);

                            info.FuncOrOper = "?:";
                            info.ArgGetter = ComputeGraphCalcNode.GetArgForFuncOrOper;
                            info.Args.Add(GetParamMatchString(condExp, iter));
                            info.Args.Add(GetParamMatchString(trueExp, iter));
                            info.Args.Add(GetParamMatchString(falseExp, iter));
                        }
                        else {
                            Console.WriteLine("{0}: {1}, invalid condition expression !", tag, info.FuncOrOper);
                        }
                    }
                }
            }
        }
        private static string GetParamMatchString(Dsl.ISyntaxComponent p, string iter)
        {
            var func = p as Dsl.FunctionData;
            if (null != func) {
                if (!func.HaveId() && func.GetParamNum() == 1) {
                    return GetParamMatchString(func.GetParam(0), iter);
                }
            }
            return ConvertMatchString(p.GetId(), iter);
        }
        private static string ConvertMatchString(string str, string iter)
        {
            if (str == "$any")
                str = "*";
            else if (str == "$iter")
                str = iter;
            return str;
        }
        private static void TransformNameReplacement(StringBuilder sb, string repTag, Dsl.ISyntaxComponent syntax, string name, int index)
        {
            var valData = syntax as Dsl.ValueData;
            if (null != valData) {
                sb.Append(valData.GetId());
            }
            else {
                var funcData = syntax as Dsl.FunctionData;
                if (null != funcData) {
                    string id = funcData.GetId();
                    if (id == "@arg") {
                        string ixStr = funcData.GetParamId(0);
                        if (int.TryParse(ixStr, out var argIx)) {
                            if (argIx == 0)
                                sb.Append(name);
                            else
                                sb.Append(index);
                        }
                        else {
                            Console.WriteLine("name_replacement: {0}, @arg's argument must be integer !", repTag);
                        }
                    }
                    else if (id == "@join") {
                        foreach (var p in funcData.Params) {
                            TransformNameReplacement(sb, repTag, p, name, index);
                        }
                    }
                    else if (id == "@repeat") {
                        var p = funcData.GetParam(0);
                        string numStr = funcData.GetParamId(1);
                        if (int.TryParse(numStr, out var num)) {
                            for (int ix = 0; ix < num; ++ix) {
                                if (ix > 0)
                                    sb.Append("|");
                                TransformNameReplacement(sb, repTag, p, name, index);
                            }
                        }
                    }
                    else if (id == "@list") {
                        for (int ix = 0; ix < funcData.GetParamNum(); ++ix) {
                            if (ix > 0)
                                sb.Append("|");
                            var p = funcData.GetParam(ix);
                            TransformNameReplacement(sb, repTag, p, name, index);
                        }
                    }
                    else {
                        sb.Append(id);
                        switch (funcData.GetParamClassUnmasked()) {
                            case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PERIOD:
                                sb.Append(".");
                                break;
                            case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET:
                                sb.Append("[");
                                break;
                            case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PARENTHESIS:
                                sb.Append("(");
                                break;
                            default:
                                Console.WriteLine("function_replacement: {0}, invalid parenthesis !", repTag);
                                break;
                        }
                        string prestr = string.Empty;
                        foreach (var p in funcData.Params) {
                            sb.Append(prestr);
                            TransformNameReplacement(sb, repTag, p, name, index);
                            if (string.IsNullOrEmpty(prestr))
                                prestr = ", ";
                        }
                        switch (funcData.GetParamClassUnmasked()) {
                            case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PERIOD:
                                break;
                            case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET:
                                sb.Append("]");
                                break;
                            case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PARENTHESIS:
                                sb.Append(")");
                                break;
                        }
                    }
                }
            }
        }

        internal static DslExpression.CalculatorValue DoCalc(Dsl.ISyntaxComponent exp)
        {
            return DoCalc(exp, string.Empty);
        }
        internal static DslExpression.CalculatorValue DoCalc(Dsl.ISyntaxComponent exp, string resultType)
        {
            bool supported = false;
            var vd = exp as Dsl.ValueData;
            var fd = exp as Dsl.FunctionData;
            var stm = exp as Dsl.StatementData;
            s_ArgTypeConversion.Clear();
            if (null != vd) {
                string vstr = vd.GetId();
                if (ActiveConfig.SettingInfo.SettingVariables.TryGetValue(vstr, out var v)) {
                    vstr = v;
                }
                else if (VariableTable.TryGetVarType(vstr, out var varType, out var isArray)) {
                    if (VariableTable.GetVarValue(vstr, varType, out var v2))
                        vstr = v2;
                }
                if (Calculator.TryParseNumeric(vstr, out var val))
                    return val;
                else if (!string.IsNullOrEmpty(vstr))
                    return DslExpression.CalculatorValue.From(vstr);
            }
            else if (null != fd) {
                if (fd.IsPeriodParamClass()) {
                    if (fd.IsHighOrder) {
                        var obj = DoCalc(fd.LowerOrderFunction);
                        var m = fd.GetParamId(0);
                        if (Calculator.CalcMember(obj, m, resultType, s_ArgTypeConversion, out var val, out supported))
                            return val;
                    }
                    else {
                        string vname = fd.GetId();
                        string m = fd.GetParamId(0);
                        if (VariableTable.TryGetVarType(vname, out var objType, out var isArray)) {
                            if (VariableTable.ObjectGetValue(vname, objType, m, out var val))
                                return val;
                        }
                    }
                }
                else if (fd.IsBracketParamClass()) {
                    if (fd.IsHighOrder) {
                        var obj = DoCalc(fd.LowerOrderFunction);
                        var m = DoCalc(fd.GetParam(0));
                        if (Calculator.CalcMember(obj, m, resultType, s_ArgTypeConversion, out var val, out supported))
                            return val;
                    }
                    else {
                        string vname = fd.GetId();
                        var m = DoCalc(fd.GetParam(0));
                        if (VariableTable.TryGetVarType(vname, out var objType, out var isArray)) {
                            if (isArray) {
                                objType = objType + "_x1024";
                                if (VariableTable.ArrayGetValue(vname, objType, m, out var val))
                                    return val;
                            }
                            else {
                                if (VariableTable.ObjectGetValue(vname, objType, m, out var val))
                                    return val;
                            }
                        }
                    }
                }
                else if (fd.IsParenthesisParamClass()) {
                    string func = fd.GetId();
                    var args = new List<DslExpression.CalculatorValue>();
                    foreach (var p in fd.Params) {
                        args.Add(DoCalc(p));
                    }
                    if (Calculator.CalcFunc(func, args, resultType, s_ArgTypeConversion, out var val, out supported))
                        return val;
                }
                else if (fd.IsOperatorParamClass()) {
                    string op = fd.GetId();
                    if (fd.GetParamNum() == 2) {
                        var arg1 = fd.GetParam(0);
                        var arg2 = fd.GetParam(1);
                        var a1 = DoCalc(arg1);
                        var a2 = DoCalc(arg2);
                        if (Calculator.CalcBinary(op, a1, a2, resultType, s_ArgTypeConversion, out var val, out supported))
                            return val;
                    }
                    else if (fd.GetParamNum() == 1) {
                        var arg1 = fd.GetParam(0);
                        var a1 = DoCalc(arg1);
                        if (Calculator.CalcUnary(op, a1, resultType, s_ArgTypeConversion, out var val, out supported))
                            return val;
                    }
                }
            }
            else if (null != stm) {
                var tfun = stm.First.AsFunction;
                var ffun = stm.Second.AsFunction;
                if (stm.GetFunctionNum() == 2 && null != tfun && null != ffun && tfun.IsTernaryOperatorParamClass()) {
                    var cond = tfun.LowerOrderFunction.GetParam(0);
                    var tval = tfun.GetParam(0);
                    var fval = ffun.GetParam(0);
                    var vcond = DoCalc(cond);
                    var vt = DoCalc(tval);
                    var vf = DoCalc(fval);
                    if (Calculator.CalcCondExp(vcond, vt, vf, resultType, s_ArgTypeConversion, out var val, out supported))
                        return val;
                }
            }
            if (!supported) {
                var r = BatchCommand.BatchScript.EvalAndRun(exp);
                return r;
            }
            return DslExpression.CalculatorValue.NullObject;
        }

        internal static void AddUnassignableVariable(SettingInfo settingInfo, Dsl.ValueData? vd)
        {
            if (null != vd) {
                string vname = vd.GetId();
                if (!settingInfo.UnassignableVariables.Contains(vname))
                    settingInfo.UnassignableVariables.Add(vname);
            }
        }
        internal static void AddUnassignableObjectMember(SettingInfo settingInfo, Dsl.FunctionData? cd)
        {
            if (null != cd && cd.IsPeriodParamClass()) {
                string cid = cd.GetId();
                string member = cd.GetParamId(0);
                if (!settingInfo.UnassignableObjectMembers.TryGetValue(cid, out var memberHashSet)) {
                    memberHashSet = new HashSet<string>();
                    settingInfo.UnassignableObjectMembers.Add(cid, memberHashSet);
                }
                if (!memberHashSet.Contains(member))
                    memberHashSet.Add(member);
            }
        }
        internal static void AddUnassignableArrayElement(SettingInfo settingInfo, Dsl.FunctionData? cd)
        {
            if (null != cd && cd.IsBracketParamClass()) {
                string cid = cd.GetId();
                var ixVal = DoCalc(cd.GetParam(0));
                if (Calculator.TryGetInt(ixVal, out var ix)) {
                    if (!settingInfo.UnassignableArrayElements.TryGetValue(cid, out var ixHashSet)) {
                        ixHashSet = new HashSet<int>();
                        settingInfo.UnassignableArrayElements.Add(cid, ixHashSet);
                    }
                    if (!ixHashSet.Contains(ix))
                        ixHashSet.Add(ix);
                }
            }
        }
        internal static void AddUnassignableObjectArrayMember(SettingInfo settingInfo, Dsl.FunctionData? cd)
        {
            if (null != cd && cd.IsPeriodParamClass() && cd.IsHighOrder && cd.LowerOrderFunction.IsBracketParamClass()) {
                string cid = cd.LowerOrderFunction.GetId();
                var ixVal = DoCalc(cd.LowerOrderFunction.GetParam(0));
                string member = cd.GetParamId(0);
                if (Calculator.TryGetInt(ixVal, out var ix)) {
                    if (!settingInfo.UnassignableObjectArrayMembers.TryGetValue(cid, out var elemMemberList)) {
                        elemMemberList = new Dictionary<int, HashSet<string>>();
                        settingInfo.UnassignableObjectArrayMembers.Add(cid, elemMemberList);
                    }
                    if (!elemMemberList.TryGetValue(ix, out var memberHashSet)) {
                        memberHashSet = new HashSet<string>();
                        elemMemberList.Add(ix, memberHashSet);
                    }
                    if (!memberHashSet.Contains(member))
                        memberHashSet.Add(member);
                }
            }
        }

        internal class SplitInfoForVariable
        {
            internal int MaxLevel = 0;
            internal int MaxLength = 0;
            internal bool Multiline = false;
            internal bool ExpandedOnlyOnce = false;

            internal static DslExpression.CalculatorValue s_DefMaxLvl = 256;
            internal static DslExpression.CalculatorValue s_DefMaxLen = 102400;
            internal static DslExpression.CalculatorValue s_DefMultiline = true;
            internal static DslExpression.CalculatorValue s_DefExpandOnce = true;
        }
        internal class ValueInfo
        {
            internal string Type = string.Empty;
            internal DslExpression.CalculatorValue Value = DslExpression.CalculatorValue.NullObject;
        }
        internal class SettingInfo
        {
            internal bool DebugMode = false;
            internal bool PrintGraph = false;
            internal bool ForHlslShader = false;
            internal bool NeedUniformUtofVals = true;
            internal bool NeedUniformFtouVals = false;
            internal bool DefMultiline = false;
            internal bool DefExpandedOnlyOnce = false;
            internal bool DefSkipValue = false;
            internal bool DefSkipExpression = false;
            internal bool GenerateExpressionList = false;
            internal bool RemoveDuplicateExpression = false;
            internal int DefMaxLevel = 32;
            internal int DefMaxLength = 1024;
            internal int ComputeGraphNodesCapacity = 10240;
            internal int ShaderVariablesCapacity = 1024;
            internal int StringBufferCapacitySurplus = 1024;
            internal int MaxIterations = 32;
            internal int MaxLoop = 256;

            internal int AutoSplitLevel = -1;
            internal int AutoSplitLevelForRepeatedExpression = 2;
            internal Dictionary<string, int> AutoSplitOnFuncs = new Dictionary<string, int>();
            internal HashSet<string> AutoSplitSkips = new HashSet<string>();

            internal HashSet<string> SplitOnVariables = new HashSet<string>();
            internal Dictionary<string, SplitInfoForVariable> VariableSplitInfos = new Dictionary<string, SplitInfoForVariable>();
            internal Dictionary<string, Dictionary<string, SplitInfoForVariable>> ObjectSplitInfos = new Dictionary<string, Dictionary<string, SplitInfoForVariable>>();
            internal Dictionary<string, Dictionary<int, SplitInfoForVariable>> ArraySplitInfos = new Dictionary<string, Dictionary<int, SplitInfoForVariable>>();
            internal Dictionary<string, Dictionary<int, Dictionary<string, SplitInfoForVariable>>> ObjectArraySplitInfos = new Dictionary<string, Dictionary<int, Dictionary<string, SplitInfoForVariable>>>();

            internal Dictionary<string, ValueInfo> VariableAssignments = new Dictionary<string, ValueInfo>();
            internal Dictionary<string, Dictionary<string, ValueInfo>> ObjectMemberAssignments = new Dictionary<string, Dictionary<string, ValueInfo>>();
            internal Dictionary<string, Dictionary<int, ValueInfo>> ArrayElementAssignments = new Dictionary<string, Dictionary<int, ValueInfo>>();
            internal Dictionary<string, Dictionary<int, Dictionary<string, ValueInfo>>> ObjectArrayMemberAssignments = new Dictionary<string, Dictionary<int, Dictionary<string, ValueInfo>>>();

            internal HashSet<string> UnassignableVariables = new HashSet<string>();
            internal Dictionary<string, HashSet<string>> UnassignableObjectMembers = new Dictionary<string, HashSet<string>>();
            internal Dictionary<string, HashSet<int>> UnassignableArrayElements = new Dictionary<string, HashSet<int>>();
            internal Dictionary<string, Dictionary<int, HashSet<string>>> UnassignableObjectArrayMembers = new Dictionary<string, Dictionary<int, HashSet<string>>>();

            internal Dictionary<string, DslExpression.CalculatorValue> SettingVariables = new Dictionary<string, DslExpression.CalculatorValue>();
            internal SortedSet<string> AutoSplitAddedVariables = new SortedSet<string>();
            internal SortedList<string, string> UsedVariables = new SortedList<string, string>();

            internal void CopyFrom(SettingInfo other)
            {
                DebugMode = other.DebugMode;
                PrintGraph = other.PrintGraph;
                ForHlslShader = other.ForHlslShader;
                NeedUniformUtofVals = other.NeedUniformUtofVals;
                NeedUniformFtouVals = other.NeedUniformFtouVals;
                DefMultiline = other.DefMultiline;
                DefExpandedOnlyOnce = other.DefExpandedOnlyOnce;
                DefSkipValue = other.DefSkipValue;
                DefSkipExpression = other.DefSkipExpression;
                GenerateExpressionList = other.GenerateExpressionList;
                RemoveDuplicateExpression = other.RemoveDuplicateExpression;
                DefMaxLevel = other.DefMaxLevel;
                DefMaxLength = other.DefMaxLength;
                ComputeGraphNodesCapacity = other.ComputeGraphNodesCapacity;
                ShaderVariablesCapacity = other.ShaderVariablesCapacity;
                StringBufferCapacitySurplus = other.StringBufferCapacitySurplus;
                MaxIterations = other.MaxIterations;
                MaxLoop = other.MaxLoop;

                AutoSplitLevel = other.AutoSplitLevel;
                AutoSplitLevelForRepeatedExpression = other.AutoSplitLevelForRepeatedExpression;

                AutoSplitOnFuncs.Clear();
                foreach(var pair in other.AutoSplitOnFuncs) {
                    AutoSplitOnFuncs.Add(pair.Key, pair.Value);
                }
                AutoSplitSkips.Clear();
                foreach(var key in other.AutoSplitSkips) {
                    AutoSplitSkips.Add(key);
                }

                SplitOnVariables.Clear();
                foreach (var key in other.SplitOnVariables) {
                    SplitOnVariables.Add(key);
                }
                VariableSplitInfos.Clear();
                foreach(var pair in other.VariableSplitInfos) {
                    VariableSplitInfos.Add(pair.Key, pair.Value);
                }
                ObjectSplitInfos.Clear();
                foreach(var pair in other.ObjectSplitInfos) {
                    var dict = new Dictionary<string, SplitInfoForVariable>();
                    ObjectSplitInfos.Add(pair.Key, dict);
                    foreach (var spair in pair.Value) {
                        dict.Add(spair.Key, spair.Value);
                    }
                }
                ArraySplitInfos.Clear();
                foreach (var pair in other.ArraySplitInfos) {
                    var dict = new Dictionary<int, SplitInfoForVariable>();
                    ArraySplitInfos.Add(pair.Key, dict);
                    foreach (var spair in pair.Value) {
                        dict.Add(spair.Key, spair.Value);
                    }
                }
                ObjectArraySplitInfos.Clear();
                foreach (var pair in other.ObjectArraySplitInfos) {
                    var dict = new Dictionary<int, Dictionary<string, SplitInfoForVariable>>();
                    ObjectArraySplitInfos.Add(pair.Key, dict);
                    foreach (var spair in pair.Value) {
                        var sdict = new Dictionary<string, SplitInfoForVariable>();
                        dict.Add(spair.Key, sdict);
                        foreach(var tpair in spair.Value) {
                            sdict.Add(tpair.Key, tpair.Value);
                        }
                    }
                }

                VariableAssignments.Clear();
                foreach (var pair in other.VariableAssignments) {
                    VariableAssignments.Add(pair.Key, pair.Value);
                }
                ObjectMemberAssignments.Clear();
                foreach (var pair in other.ObjectMemberAssignments) {
                    var dict = new Dictionary<string, ValueInfo>();
                    ObjectMemberAssignments.Add(pair.Key, dict);
                    foreach (var spair in pair.Value) {
                        dict.Add(spair.Key, spair.Value);
                    }
                }
                ArrayElementAssignments.Clear();
                foreach (var pair in other.ArrayElementAssignments) {
                    var dict = new Dictionary<int, ValueInfo>();
                    ArrayElementAssignments.Add(pair.Key, dict);
                    foreach (var spair in pair.Value) {
                        dict.Add(spair.Key, spair.Value);
                    }
                }
                ObjectArrayMemberAssignments.Clear();
                foreach (var pair in other.ObjectArrayMemberAssignments) {
                    var dict = new Dictionary<int, Dictionary<string, ValueInfo>>();
                    ObjectArrayMemberAssignments.Add(pair.Key, dict);
                    foreach (var spair in pair.Value) {
                        var sdict = new Dictionary<string, ValueInfo>();
                        dict.Add(spair.Key, sdict);
                        foreach (var tpair in spair.Value) {
                            sdict.Add(tpair.Key, tpair.Value);
                        }
                    }
                }

                UnassignableVariables.Clear();
                foreach(var key in other.UnassignableVariables) {
                    UnassignableVariables.Add(key);
                }
                UnassignableObjectMembers.Clear();
                foreach (var pair in other.UnassignableObjectMembers) {
                    var hash = new HashSet<string>();
                    UnassignableObjectMembers.Add(pair.Key, hash);
                    foreach (var skey in pair.Value) {
                        hash.Add(skey);
                    }
                }
                UnassignableArrayElements.Clear();
                foreach (var pair in other.UnassignableArrayElements) {
                    var hash = new HashSet<int>();
                    UnassignableArrayElements.Add(pair.Key, hash);
                    foreach (var skey in pair.Value) {
                        hash.Add(skey);
                    }
                }
                UnassignableObjectArrayMembers.Clear();
                foreach (var pair in other.UnassignableObjectArrayMembers) {
                    var dict = new Dictionary<int, HashSet<string>>();
                    UnassignableObjectArrayMembers.Add(pair.Key, dict);
                    foreach (var spair in pair.Value) {
                        var hash = new HashSet<string>();
                        dict.Add(spair.Key, hash);
                        foreach (var tkey in spair.Value) {
                            hash.Add(tkey);
                        }
                    }
                }

                SettingVariables.Clear();
                foreach(var pair in other.SettingVariables) {
                    SettingVariables.Add(pair.Key, pair.Value);
                }
                AutoSplitAddedVariables.Clear();
                foreach(var key in other.AutoSplitAddedVariables) {
                    AutoSplitAddedVariables.Add(key);
                }
                UsedVariables.Clear();
                foreach (var pair in other.UsedVariables) {
                    UsedVariables.Add(pair.Key, pair.Value);
                }
            }
            internal void AutoSplitAddVariable(string vname, string type)
            {
                if (!string.IsNullOrEmpty(vname)) {
                    if (AutoSplitSkips.Contains(vname))
                        return;
                    AddSplitOnVariable(vname, type);
                }
            }
            internal void AddSplitOnVariable(string vname, string type)
            {
                if (!string.IsNullOrEmpty(vname)) {
                    if (!SplitOnVariables.Contains(vname)) {
                        AddUsedVariable(vname, type);
                        AutoSplitAddedVariables.Add("\t" + vname + ",");
                    }

                    AddSplitOnVariable(vname, SplitInfoForVariable.s_DefMaxLvl, SplitInfoForVariable.s_DefMaxLen, SplitInfoForVariable.s_DefMultiline, SplitInfoForVariable.s_DefExpandOnce);
                }
            }
            internal void AddSplitOnVariable(string vname, DslExpression.CalculatorValue v1val, DslExpression.CalculatorValue v2val, DslExpression.CalculatorValue v3val, DslExpression.CalculatorValue v4val)
            {
                if (!string.IsNullOrEmpty(vname)) {
                    if (!SplitOnVariables.Contains(vname))
                        SplitOnVariables.Add(vname);

                    if (Calculator.TryGetInt(v1val, out var lvlForExp) && Calculator.TryGetInt(v2val, out var lenForExp)
                        && Calculator.TryGetBool(v3val, out var ml) && Calculator.TryGetBool(v4val, out var once)) {
                        if (!VariableSplitInfos.TryGetValue(vname, out var lvlInfo)) {
                            lvlInfo = new SplitInfoForVariable();
                            VariableSplitInfos.Add(vname, lvlInfo);
                        }
                        lvlInfo.MaxLevel = lvlForExp;
                        lvlInfo.MaxLength = lenForExp;
                        lvlInfo.Multiline = ml;
                        lvlInfo.ExpandedOnlyOnce = once;
                    }
                }
            }
            internal void AddUsedVariable(string vname, string type)
            {
                if (UsedVariables.TryGetValue(vname, out var curType)) {
                    if (string.IsNullOrEmpty(curType) && !string.IsNullOrEmpty(type))
                        UsedVariables[vname] = type;
                }
                else {
                    UsedVariables.Add(vname, type);
                }
            }
            internal void RemoveUsedVariable(string vname)
            {
                UsedVariables.Remove(vname);
            }
            internal void SetUsedVariableType(string vname, string type)
            {
                if (UsedVariables.TryGetValue(vname, out var curType)) {
                    if (string.IsNullOrEmpty(curType) && !string.IsNullOrEmpty(type))
                        UsedVariables[vname] = type;
                }
            }
            internal void AddAssignment(string vname, string type, DslExpression.CalculatorValue val)
            {
                VariableAssignments[vname] = new ValueInfo { Type = type, Value = val };
            }

            internal static DslExpression.CalculatorValue s_DefSplitLevel = 5;
            internal static DslExpression.CalculatorValue s_DefSplitOnLevel = 1;
        }
        internal class InOutAttrInfo
        {
            internal string InAttrImportFile = string.Empty;
            internal string OutAttrImportFile = string.Empty;
            internal int AttrIndex = 0;
            internal Dictionary<string, string> InAttrMap = new Dictionary<string, string>();
            internal Dictionary<string, string> OutAttrMap = new Dictionary<string, string>();
        }
        internal class UniformImportInfo
        {
            internal string File = string.Empty;
            internal string Type = string.Empty;
            internal HashSet<int> UsedIndexes = new HashSet<int>();
        }
        internal class SSBOImportInfo
        {
            internal string AttrArrayLeft = string.Empty;
            internal string Type = string.Empty;
            internal string File = string.Empty;
            internal string Attr = string.Empty;
        }
        internal class VAOImportInfo
        {
            internal string AttrArrayLeft = string.Empty;
            internal string Type = string.Empty;
            internal string File = string.Empty;
            internal string Attr = string.Empty;
            internal HashSet<int> UsedIndexes = new HashSet<int>();
        }
        internal class HlslMergeImportInfo
        {
            internal string File = string.Empty;
            internal Dictionary<string, string> AttrHlslMap = new Dictionary<string, string>();
            internal List<Tuple<string, string, string, Dsl.ISyntaxComponent>> NameReplacements = new List<Tuple<string, string, string, ISyntaxComponent>>();
        }
        internal class ShaderArgConfig
        {
            internal int Id = 0;
            internal InOutAttrInfo InOutAttrInfo = new InOutAttrInfo();
            internal List<UniformImportInfo> UniformImports = new List<UniformImportInfo>();
            internal List<VAOImportInfo> VAOImports = new List<VAOImportInfo>();
            internal List<SSBOImportInfo> SSBOImports = new List<SSBOImportInfo>();
            internal List<HlslMergeImportInfo> HlslMergeImports = new List<HlslMergeImportInfo>();

            internal static ShaderArgConfig s_Empty = new ShaderArgConfig();
        }
        internal class FunctionMatchInfo
        {
            internal delegate ComputeGraphNode GetArgDelegation(ComputeGraphNode expNode, int index, ref int curLevel);

            internal string FuncOrOper = string.Empty;
            internal GetArgDelegation? ArgGetter = null;
            internal List<string> Args = new List<string>();
        }
        internal class FunctionReplacement : FunctionMatchInfo
        {
            internal Dsl.ISyntaxComponent Replacement = Dsl.AbstractSyntaxComponent.NullSyntax;
        }
        internal class StringReplacement
        {
            internal string Src = string.Empty;
            internal string Dst = string.Empty;
            internal Regex? SrcRegex = null;
        }
        internal delegate DslExpression.CalculatorValue CalculatorValueDelegation(List<DslExpression.CalculatorValue> args, string resultType, Dictionary<int, int> argTypeConversion);
        internal class CalculatorInfo : FunctionMatchInfo
        {
            internal CalculatorValueDelegation? OnGetValue;
        }
        internal class ShaderConfig
        {
            internal string ShaderType = string.Empty;
            internal SettingInfo SettingInfo = new SettingInfo();
            internal Dictionary<int, ShaderArgConfig> ArgConfigs = new Dictionary<int, ShaderArgConfig>();
            internal Dictionary<string, string> TypeReplacements = new Dictionary<string, string>();
            internal Dictionary<string, List<FunctionReplacement>> FunctionReplacements = new Dictionary<string, List<FunctionReplacement>>();
            internal List<StringReplacement> StringReplacements = new List<StringReplacement>();
            internal Dictionary<string, List<CalculatorInfo>> Calculators = new Dictionary<string, List<CalculatorInfo>>();
            internal Dictionary<string, string> CodeBlocks = new Dictionary<string, string>();

            internal void CopyFrom(CommonConfig other)
            {
                SettingInfo.CopyFrom(other.SettingInfo);

                TypeReplacements.Clear();
                foreach(var pair in other.TypeReplacements) {
                    TypeReplacements.Add(pair.Key, pair.Value);
                }
                FunctionReplacements.Clear();
                foreach(var pair in other.FunctionReplacements) {
                    var list = new List<FunctionReplacement>(pair.Value);
                    FunctionReplacements.Add(pair.Key, list);
                }
                StringReplacements.Clear();
                StringReplacements.AddRange(other.StringReplacements);
                Calculators.Clear();
                foreach (var pair in other.Calculators) {
                    var list = new List<CalculatorInfo>(pair.Value);
                    Calculators.Add(pair.Key, list);
                }
                CodeBlocks.Clear();
                foreach(var pair in other.CodeBlocks) {
                    CodeBlocks.Add(pair.Key, pair.Value);
                }
            }
        }
        internal class CommonConfig
        {
            internal int ViewportWidth = 256;
            internal int ViewportHeight = 256;

            internal SettingInfo SettingInfo = new SettingInfo();
            internal Dictionary<string, string> TypeReplacements = new Dictionary<string, string>();
            internal Dictionary<string, List<FunctionReplacement>> FunctionReplacements = new Dictionary<string, List<FunctionReplacement>>();
            internal List<StringReplacement> StringReplacements = new List<StringReplacement>();
            internal Dictionary<string, List<CalculatorInfo>> Calculators = new Dictionary<string, List<CalculatorInfo>>();
            internal Dictionary<string, string> CodeBlocks = new Dictionary<string, string>();
        }

        internal static CommonConfig CommonCfg { get; } = new CommonConfig();
        internal static ShaderConfig ActiveConfig
        {
            get {
                if (null == s_ActiveConfig) {
                    s_ActiveConfig = new ShaderConfig();
                }
                return s_ActiveConfig;
            }
        }
        private static ShaderConfig? s_ActiveConfig = null;
        internal static ShaderArgConfig ActiveArgConfig
        {
            get {
                if (ActiveConfig.ArgConfigs.TryGetValue(ActiveArgCfgId, out var config)) {
                    return config;
                }
                return ShaderArgConfig.s_Empty;
            }
        }
        internal static int ActiveArgCfgId { get; set; } = 0;

        private static Dictionary<string, ShaderConfig> s_ShaderConfigs = new Dictionary<string, ShaderConfig>();
        private static int s_CurMaxShaderArgId = -1;
        internal static Random s_Random = new Random();
        internal static Dictionary<int, int> s_ArgTypeConversion = new Dictionary<int, int>();
    }

    internal sealed class ShaderExp : DslExpression.AbstractExpression
    {
        protected override DslExpression.CalculatorValue DoCalc()
        {
            DslExpression.CalculatorValue val = DslExpression.CalculatorValue.NullObject;
            foreach (var p in m_DslArgs) {
                val = Config.DoCalc(p);
            }
            return val;
        }
        protected override bool Load(Dsl.FunctionData funcData)
        {
            if (funcData.IsHighOrder) {
                Load(funcData.LowerOrderFunction);
            }
            foreach (var p in funcData.Params) {
                m_DslArgs.Add(p);
            }
            return true;
        }

        private List<Dsl.ISyntaxComponent> m_DslArgs = new List<Dsl.ISyntaxComponent>();
    }
    internal sealed class AddShaderVarExp : DslExpression.AbstractExpression
    {
        protected override DslExpression.CalculatorValue DoCalc()
        {
            int ct = 1;
            if (null != m_CountExp)
                ct = m_CountExp.Calc();
            string type = m_Type;
            if (ct > 1)
                type = type + "_x" + ct.ToString();
            VariableTable.AllocVar(m_Name, type);
            return DslExpression.CalculatorValue.From(true);
        }
        protected override bool Load(Dsl.FunctionData funcData)
        {
            if (funcData.IsParenthesisParamClass() && !funcData.IsHighOrder) {
                m_Name = funcData.GetParamId(0);
                m_Type = funcData.GetParamId(1);
                if (funcData.GetParamNum() > 2)
                    m_CountExp = Calculator.Load(funcData.GetParam(2));
            }
            return true;
        }

        private string m_Name = string.Empty;
        private string m_Type = string.Empty;
        private DslExpression.IExpression? m_CountExp = null;
    }
    internal sealed class SetShaderVarExp : DslExpression.AbstractExpression
    {
        protected override DslExpression.CalculatorValue DoCalc()
        {
            bool ret = false;
            Debug.Assert(null != m_ValExp);
            var val = m_ValExp.Calc();
            int ix = -1;
            switch (m_VarType) {
                case VarTypeEnum.Var:
                    ret = SetVar(ref val);
                    break;
                case VarTypeEnum.Obj:
                    ret = SetObj(ref val);
                    break;
                case VarTypeEnum.Array:
                    Debug.Assert(null != m_IndexExp);
                    ix = m_IndexExp.Calc();
                    ret = SetArray(ix, ref val);
                    break;
                case VarTypeEnum.ObjArray:
                    Debug.Assert(null != m_IndexExp);
                    ix = m_IndexExp.Calc();
                    ret = SetObjArray(ix, ref val);
                    break;
            }
            return DslExpression.CalculatorValue.From(ret);
        }
        protected override bool Load(Dsl.FunctionData funcData)
        {
            if (funcData.IsParenthesisParamClass() && !funcData.IsHighOrder) {
                var varDsl = funcData.GetParam(0);

                var vd = varDsl as Dsl.ValueData;
                var fd = varDsl as Dsl.FunctionData;
                var sd = varDsl as Dsl.StatementData;
                if (null != sd) {
                    fd = sd.Last.AsFunction;
                    if (null == fd) {
                        vd = sd.Last.AsValue;
                    }
                }
                if (null != vd) {
                    m_VarType = VarTypeEnum.Var;
                    m_VarName = vd.GetId();
                }
                else if (null != fd) {
                    if (fd.IsPeriodParamClass() && fd.IsHighOrder && fd.LowerOrderFunction.IsBracketParamClass()) {
                        m_VarType = VarTypeEnum.ObjArray;
                        m_VarName = fd.LowerOrderFunction.GetId();
                        m_IndexExp = Calculator.Load(fd.LowerOrderFunction.GetParam(0));
                        m_Member = fd.GetParamId(0);
                    }
                    else if (fd.IsPeriodParamClass()) {
                        m_VarType = VarTypeEnum.Obj;
                        m_VarName = fd.GetId();
                        m_Member = fd.GetParamId(0);
                    }
                    else if (fd.IsBracketParamClass()) {
                        m_VarType = VarTypeEnum.Array;
                        m_VarName = fd.GetId();
                        m_IndexExp = Calculator.Load(fd.GetParam(0));
                    }
                }

                m_ValExp = Calculator.Load(funcData.GetParam(1));
            }
            return true;
        }
        private bool SetVar(ref DslExpression.CalculatorValue val)
        {
            bool ret = VariableTable.TrySetVariable(m_VarName, ref val);
            return ret;
        }
        private bool SetObj(ref DslExpression.CalculatorValue val)
        {
            bool ret = VariableTable.TrySetObject(m_VarName, m_Member, ref val);
            return ret;
        }
        private bool SetArray(int ix, ref DslExpression.CalculatorValue val)
        {
            bool ret = VariableTable.TrySetArray(m_VarName, ix, ref val);
            return ret;
        }
        private bool SetObjArray(int ix, ref DslExpression.CalculatorValue val)
        {
            bool ret = VariableTable.TrySetObjArray(m_VarName, ix, m_Member, ref val);
            return ret;
        }

        enum VarTypeEnum
        {
            Var = 0,
            Obj,
            Array,
            ObjArray
        }

        private VarTypeEnum m_VarType = VarTypeEnum.Var;
        private string m_VarName = string.Empty;
        private IExpression? m_IndexExp = null;
        private string m_Member = string.Empty;
        private IExpression? m_ValExp = null;
    }
    internal sealed class AddUnassignableShaderVarExp : DslExpression.AbstractExpression
    {
        protected override DslExpression.CalculatorValue DoCalc()
        {
            bool ret = false;
            foreach (var varDsl in m_DslArgs) {
                var vd = varDsl as Dsl.ValueData;
                var fd = varDsl as Dsl.FunctionData;
                var sd = varDsl as Dsl.StatementData;
                if (null != sd) {
                    fd = sd.Last.AsFunction;
                    if (null == fd) {
                        vd = sd.Last.AsValue;
                    }
                }
                if (null != vd) {
                    Config.AddUnassignableVariable(Config.ActiveConfig.SettingInfo, vd);
                    ret = true;
                }
                else if (null != fd) {
                    if (fd.IsPeriodParamClass() && fd.IsHighOrder && fd.LowerOrderFunction.IsBracketParamClass()) {
                        Config.AddUnassignableObjectArrayMember(Config.ActiveConfig.SettingInfo, fd);
                        ret = true;
                    }
                    else if (fd.IsPeriodParamClass()) {
                        Config.AddUnassignableObjectMember(Config.ActiveConfig.SettingInfo, fd);
                        ret = true;
                    }
                    else if (fd.IsBracketParamClass()) {
                        Config.AddUnassignableArrayElement(ActiveConfig.SettingInfo, fd);
                        ret = true;
                    }
                }
            }
            return DslExpression.CalculatorValue.From(ret);
        }
        protected override bool Load(Dsl.FunctionData funcData)
        {
            if (funcData.IsHighOrder) {
                Load(funcData.LowerOrderFunction);
            }
            foreach (var p in funcData.Params) {
                m_DslArgs.Add(p);
            }
            return true;
        }

        private List<Dsl.ISyntaxComponent> m_DslArgs = new List<Dsl.ISyntaxComponent>();
    }
    internal sealed class ImportInOutExp : DslExpression.SimpleExpressionBase
    {
        protected override DslExpression.CalculatorValue OnCalc(IList<DslExpression.CalculatorValue> operands)
        {
            bool ret = false;
            if (operands.Count > 0) {
                bool ret1 = true;
                bool ret2 = true;
                int ix = operands[0].GetInt();
                var cfg = Config.ActiveArgConfig;
                var attrCfg = cfg.InOutAttrInfo;
                if (!string.IsNullOrEmpty(attrCfg.InAttrImportFile)) {
                    if (File.Exists(attrCfg.InAttrImportFile)) {
                        ret1 = RenderDocImporter.ImportVsInOutData("float", ix, attrCfg.InAttrMap, attrCfg.InAttrImportFile);
                    }
                    else {
                        ret1 = false;
                        Console.WriteLine("Can't find file {0}", attrCfg.InAttrImportFile);
                    }
                }
                if (!string.IsNullOrEmpty(attrCfg.OutAttrImportFile)) {
                    if (File.Exists(attrCfg.OutAttrImportFile)) {
                        ret2 = RenderDocImporter.ImportVsInOutData("float", ix, attrCfg.OutAttrMap, attrCfg.OutAttrImportFile);
                    }
                    else {
                        ret2 = false;
                        Console.WriteLine("Can't find file {0}", attrCfg.OutAttrImportFile);
                    }
                }
                ret = ret1 && ret2;
            }
            return DslExpression.CalculatorValue.From(ret);
        }
    }
    internal sealed class ReCalcExp : DslExpression.SimpleExpressionBase
    {
        protected override DslExpression.CalculatorValue OnCalc(IList<DslExpression.CalculatorValue> operands)
        {
            bool full = false;
            if (operands.Count > 0) {
                full = operands[0].GetBool();
            }
            var ret = Program.ReCalc(full);
            return DslExpression.CalculatorValue.From(ret);
        }
    }
    internal sealed class RandColorExp : DslExpression.SimpleExpressionBase
    {
        protected override DslExpression.CalculatorValue OnCalc(IList<DslExpression.CalculatorValue> operands)
        {
            var color = new Float4 {
                x = Config.s_Random.NextSingle(),
                y = Config.s_Random.NextSingle(),
                z = Config.s_Random.NextSingle(),
                w = Config.s_Random.NextSingle()
            };
            return DslExpression.CalculatorValue.FromObject(color);
        }
    }
    internal sealed class RandUVExp : DslExpression.SimpleExpressionBase
    {
        protected override DslExpression.CalculatorValue OnCalc(IList<DslExpression.CalculatorValue> operands)
        {
            var uv = DslExpression.CalculatorValue.NullObject;
            if (operands.Count > 0) {
                int num = operands[0].GetInt();
                switch (num) {
                    case 2:
                        uv = DslExpression.CalculatorValue.FromObject(new Float2 {
                            x = Config.s_Random.NextSingle(),
                            y = Config.s_Random.NextSingle()
                        });
                        break;
                    case 3:
                        uv = DslExpression.CalculatorValue.FromObject(new Float3 {
                            x = Config.s_Random.NextSingle(),
                            y = Config.s_Random.NextSingle(),
                            z = Config.s_Random.NextSingle()
                        });
                        break;
                    case 4:
                        uv = DslExpression.CalculatorValue.FromObject(new Float4 {
                            x = Config.s_Random.NextSingle(),
                            y = Config.s_Random.NextSingle(),
                            z = Config.s_Random.NextSingle(),
                            w = Config.s_Random.NextSingle()
                        });
                        break;
                }
            }
            return uv;
        }
    }
    internal sealed class RandSizeExp : DslExpression.SimpleExpressionBase
    {
        protected override DslExpression.CalculatorValue OnCalc(IList<DslExpression.CalculatorValue> operands)
        {
            var size = DslExpression.CalculatorValue.NullObject;
            if (operands.Count > 0) {
                var list = new List<int>();
                foreach (var v in operands) {
                    list.Add(v.GetInt());
                }
                switch (list.Count) {
                    case 1:
                        size = DslExpression.CalculatorValue.From(Config.s_Random.Next(list[0]));
                        break;
                    case 2:
                        size = DslExpression.CalculatorValue.FromObject(new Float2 {
                            x = Config.s_Random.Next(list[0]),
                            y = Config.s_Random.Next(list[1])
                        });
                        break;
                    case 3:
                        size = DslExpression.CalculatorValue.FromObject(new Float3 {
                            x = Config.s_Random.Next(list[0]),
                            y = Config.s_Random.Next(list[1]),
                            z = Config.s_Random.Next(list[2])
                        });
                        break;
                    case 4:
                        size = DslExpression.CalculatorValue.FromObject(new Float4 {
                            x = Config.s_Random.Next(list[0]),
                            y = Config.s_Random.Next(list[1]),
                            z = Config.s_Random.Next(list[2]),
                            w = Config.s_Random.Next(list[3])
                        });
                        break;
                }
            }
            return size;
        }
    }
}
