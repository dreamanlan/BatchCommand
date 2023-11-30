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

namespace GlslRewriter
{
    internal static class RenderDocImporter
    {
        internal static List<string> GenenerateVsInOutAttr(string type, int index, Dictionary<string,string> attrMap, string csv_path)
        {
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
                                if (attrMap.TryGetValue(names[0], out var newName)) {
                                    if (string.IsNullOrEmpty(newName)) {
                                        continue;
                                    }
                                    names[0] = newName;
                                }
                                sb.Append(names[0]);
                                sb.Append(".");
                                sb.Append(names[1]);
                                sb.Append(" = ");
                                sb.Append(type);
                                sb.Append("(");
                                sb.Append(cols[i].Trim());
                                sb.Append(");");

                                results.Add(sb.ToString());
                                sb.Length = 0;
                            }
                            else {
                                Debug.Assert(false);
                            }
                        }
                    }

                    sb.Append("var vertexes = new[] {");
                    s_VertexStructInits.Add(sb.ToString());
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
                                }
                                else if (i > 2)
                                    sb.Append(", ");
                                sb.Append(cols[i].Trim());
                                sb.Append("f");
                            }
                            sb.Append("))");
                            s_VertexStructInits.Add(sb.ToString());
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
                                            attrs = new List<string>();
                                            s_VertexAttrInits.Add(prevNames[0], attrs);
                                            attrs.Add("var " + prevNames[0] + " = new[] {");
                                            first = true;
                                        }
                                        attrs.Add(sb.ToString());
                                        ++j;
                                        sb.Length = 0;
                                        sb.Append("\t");
                                    }
                                    if (first)
                                        first = false;
                                    else
                                        sb.Append(", ");
                                    sb.Append("new Vector4(");
                                }
                                else if (i > 2)
                                    sb.Append(", ");
                                sb.Append(cols[i].Trim());
                                sb.Append("f");
                            }
                            sb.Append(")");
                            var lastNames = colNames[colNames.Length - 1].Split(".");
                            if (!s_VertexAttrInits.TryGetValue(lastNames[0], out var attrs2)) {
                                attrs2 = new List<string>();
                                s_VertexAttrInits.Add(lastNames[0], attrs2);
                                attrs2.Add("var " + lastNames[0] + " = new[] {");
                            }
                            attrs2.Add(sb.ToString());
                            sb.Length = 0;
                        }
                    }

                    const string c_EndStr = "};";

                    sb.Append(c_EndStr);
                    s_VertexStructInits.Add(sb.ToString());
                    sb.Length = 0;

                    foreach(var pair in s_VertexAttrInits) {
                        var list = pair.Value;
                        if (list.Count > 0 && list[list.Count - 1] != c_EndStr)
                            pair.Value.Add(c_EndStr);
                    }
                }
            }
            return results;
        }
        internal static List<string> GenerateUniform(string type, HashSet<int> indexes, string csv_path)
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
                            if (firstCol.Length > 0 && firstCol[firstCol.Length - 1] == ']') {
                                sb.Append(cols[0]);
                                sb.Append(" = ");
                                sb.Append(type);
                                sb.Append("(");
                                sb.Append(cols[1]);
                                sb.Append(");");

                                results.Add(sb.ToString());
                                sb.Length = 0;

                                if (type == "vec4") {
                                    string vals = cols[1];
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
                                else {
                                    string vals = cols[1];
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
                            }
                        }
                    }
                }
            }
            return results;
        }
        internal static List<string> GenerateVAO(string attr, string attrArrayLeft, string type, HashSet<int> indexes, string csv_path)
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
                        if (indexes.Count == 0 || indexes.Contains(ix - 1)) {
                            var line = lines[ix];
                            var cols = SplitCsvLine(line);
                            Debug.Assert(colNames.Length == cols.Count);
                            sb.Append("\t");
                            if (results.Count > start) {
                                sb.Append(", ");
                            }
                            if (type == "float") {
                                sb.Append(cols[1]);
                                sb.Append("f");
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
                        }
                        else if (type == "uint") {
                            sb.Append(cols[1]);
                            sb.Append("u");
                        }
                        else if (type == "float") {
                            sb.Append(cols[1]);
                            sb.Append("f");
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

        internal static List<string> s_UniformUtofOrFtouVals = new List<string>();
        internal static List<string> s_VertexStructInits = new List<string>();
        internal static SortedDictionary<string, List<string>> s_VertexAttrInits = new SortedDictionary<string, List<string>>();
        internal static List<string> s_UniformInits = new List<string>();
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
                        if (id == "vs") {
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
            else if(Program.s_IsPsShader) {
                s_ShaderConfigs.TryGetValue("ps", out s_ActiveConfig);
            }
            else if(Program.s_IsCsShader) {
                s_ShaderConfigs.TryGetValue("cs", out s_ActiveConfig);
            }
        }
        internal static string ReplaceString(string txt)
        {
            string ret = txt;
            if (Config.ActiveConfig.StringReplacements.Count > 0) {
                foreach(var info in Config.ActiveConfig.StringReplacements) {
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
                if(settingInfo.VariableSplitInfos.TryGetValue(vname, out var lvlInfo)) {
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
                if(fd.IsPeriodParamClass() && fd.IsHighOrder && fd.LowerOrderFunction.IsBracketParamClass()) {
                    string cid = fd.LowerOrderFunction.GetId();
                    string ixStr = fd.LowerOrderFunction.GetParamId(0);
                    string member = fd.GetParamId(0);
                    if(int.TryParse(ixStr, out var index) && settingInfo.ObjectArraySplitInfos.TryGetValue(cid, out var objArrLvlInfo)
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
                    if(settingInfo.ObjectSplitInfos.TryGetValue(cid, out var objLvlInfo) && objLvlInfo.TryGetValue(member, out var lvlInfo)) {
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
        internal static bool CalcFunc(string func, List<DslExpression.CalculatorValue> args, out DslExpression.CalculatorValue val)
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
            else if(Calculator.CalcFunc(func, args, out val, out var supported)) {
                ret = true;
            }
            else if (ActiveConfig.Calculators.TryGetValue(func, out var infos)) {
                foreach(var info in infos) {
                    bool match = true;
                    for (int i = 0; i < args.Count && i < info.Args.Count; ++i) {
                        if (args[i].ToString() == info.Args[i] || info.Args[i]=="*") {
                        }
                        else {
                            match = false;
                            break;
                        }
                    }
                    if (match && null != info.OnGetValue) {
                        val = info.OnGetValue(args);
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

        private static void AddShaderConfig(string shaderType, Dsl.FunctionData dslCfg)
        {
            var cfgInfo = new ShaderConfig();
            cfgInfo.ShaderType = shaderType;
            foreach (var p in dslCfg.Params) {
                string id = p.GetId();
                var pfd = p as Dsl.FunctionData;
                if (null != pfd) {
                    if (id == "setting") {
                        ParseSetting(cfgInfo, pfd);
                    }
                    else if (id == "shader_arg") {
                        ParseShaderArg(cfgInfo, pfd);
                    }
                    else if (id == "string_replacement") {
                        ParseStringReplacement(cfgInfo, pfd);
                    }
                    else if (id == "calculator") {
                        ParseCalculator(cfgInfo, pfd);
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

                string key = !callCfg.IsParenthesisParamClass() || callCfg.GetParamNum() <= 0 ? "global" : callCfg.GetParamId(0).Trim();

                if (dslCfg.HaveExternScript()) {
                    string code = dslCfg.GetParamId(0);

                    cfgInfo.CodeBlocks[key] = code;
                }
            }
            else {
                Console.WriteLine("[Error]: {0}_code_block must be defined after the {0} config !", shaderType);
            }
        }
        private static void ParseSetting(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            if (dslCfg.HaveStatement()) {
                foreach (var p in dslCfg.Params) {
                    var vd = p as Dsl.ValueData;
                    var fd = p as Dsl.FunctionData;
                    if (null != vd) {
                        string vid = vd.GetId();
                        if (vid == "debug_mode") {
                            cfg.SettingInfo.DebugMode = true;
                        }
                        else if (vid == "print_graph") {
                            cfg.SettingInfo.PrintGraph = true;
                        }
                        else if (vid == "generate_expression_list") {
                            cfg.SettingInfo.GenerateExpressionList = true;
                        }
                        else if (vid == "def_multiline") {
                            cfg.SettingInfo.DefMultiline = true;
                        }
                        else if (vid == "def_expanded_only_once") {
                            cfg.SettingInfo.DefExpandedOnlyOnce = true;
                        }
                        else if (vid == "def_skip_value") {
                            cfg.SettingInfo.DefSkipValue = true;
                        }
                        else if (vid == "def_skip_expression") {
                            cfg.SettingInfo.DefSkipExpression = true;
                        }
                    }
                    else if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "=") {
                            string key = fd.GetParamId(0);
                            var val = DoCalc(fd.GetParam(1));
                            cfg.SettingInfo.SettingVariables[key] = val;

                            if (key == "need_uniform_utof_vals") {
                                if (Calculator.TryGetBool(val, out var v)) {
                                    cfg.SettingInfo.NeedUniformUtofVals = v;
                                }
                            }
                            else if (key == "need_uniform_ftou_vals") {
                                if (Calculator.TryGetBool(val, out var v)) {
                                    cfg.SettingInfo.NeedUniformFtouVals = v;
                                }
                            }
                            else if (key == "def_max_level") {
                                if (Calculator.TryGetInt(val, out var v) && v > 0) {
                                    cfg.SettingInfo.DefMaxLevel = v;
                                }
                            }
                            else if (key == "def_max_length") {
                                if (Calculator.TryGetInt(val, out var v) && v > 0) {
                                    cfg.SettingInfo.DefMaxLength = v;
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
                                    cfg.SettingInfo.ComputeGraphNodesCapacity = v;
                                }
                            }
                            else if(key== "shader_variables_capacity") {
                                if (Calculator.TryGetInt(val, out var v)) {
                                    cfg.SettingInfo.ShaderVariablesCapacity = v;
                                }
                            }
                            else if(key== "string_buffer_capacity_surplus") {
                                if (Calculator.TryGetInt(val, out var v)) {
                                    cfg.SettingInfo.StringBufferCapacitySurplus = v;
                                }
                            }
                            else if (key == "max_iterations") {
                                if (Calculator.TryGetInt(val, out var v)) {
                                    cfg.SettingInfo.MaxIterations = v;
                                }
                            }
                            else if (key == "max_loop") {
                                if (Calculator.TryGetInt(val, out var v)) {
                                    cfg.SettingInfo.MaxLoop = v;
                                }
                            }
                        }
                        else if (fid == "split_variable_assignment") {
                            ParseSplitVariableAssignment(cfg, fd);
                        }
                        else if (fid == "split_object_assignment") {
                            ParseSplitObjectAssignment(cfg, fd);
                        }
                        else if (fid == "split_array_assignment") {
                            ParseSplitArrayAssignment(cfg, fd);
                        }
                        else if (fid == "split_object_array_assignment") {
                            ParseSplitObjectArrayAssignment(cfg, fd);
                        }
                        else if (fid == "auto_split") {
                            ParseAutoSplit(cfg, fd);
                        }
                        else if (fid == "unassignable_variable") {
                            ParseUnassignableVariable(cfg, fd);
                        }
                        else if (fid == "unassignable_object_member") {
                            ParseUnassignableObjectMember(cfg, fd);
                        }
                        else if (fid == "unassignable_array_element") {
                            ParseUnassignableArrayElement(cfg, fd);
                        }
                        else if (fid == "unassignable_object_array_member") {
                            ParseUnassignableObjectArrayMember(cfg, fd);
                        }
                        else if (fid == "variable_assignment") {
                            ParseVariableAssignment(cfg, fd);
                        }
                        else if (fid == "object_member_assignment") {
                            ParseObjectMemberAssignment(cfg, fd);
                        }
                        else if (fid == "array_element_assignment") {
                            ParseArrayElementAssignment(cfg, fd);
                        }
                        else if (fid == "object_array_member_assignment") {
                            ParseObjectArrayMemberAssignment(cfg, fd);
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
        private static void ParseSplitVariableAssignment(ShaderConfig cfg, Dsl.FunctionData dslCfg)
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
                    cfg.SettingInfo.AddSplitOnVariable(key);
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
                    cfg.SettingInfo.AddSplitOnVariable(key, v1val, v2val, v3val, v4val);
                }
            }
        }
        private static void ParseSplitObjectAssignment(ShaderConfig cfg, Dsl.FunctionData dslCfg)
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
                    if (!cfg.SettingInfo.ObjectSplitInfos.TryGetValue(cid, out var objLvlInfo)) {
                        objLvlInfo = new Dictionary<string, SplitInfoForVariable>();
                        cfg.SettingInfo.ObjectSplitInfos.Add(cid, objLvlInfo);
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
        private static void ParseSplitArrayAssignment(ShaderConfig cfg, Dsl.FunctionData dslCfg)
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
                        if (!cfg.SettingInfo.ArraySplitInfos.TryGetValue(cid, out var arrLvlInfo)) {
                            arrLvlInfo = new Dictionary<int, SplitInfoForVariable>();
                            cfg.SettingInfo.ArraySplitInfos.Add(cid, arrLvlInfo);
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
        private static void ParseSplitObjectArrayAssignment(ShaderConfig cfg, Dsl.FunctionData dslCfg)
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
                        if (!cfg.SettingInfo.ObjectArraySplitInfos.TryGetValue(cid, out var objArrLvlInfo)) {
                            objArrLvlInfo = new Dictionary<int, Dictionary<string, SplitInfoForVariable>>();
                            cfg.SettingInfo.ObjectArraySplitInfos.Add(cid, objArrLvlInfo);
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
        private static void ParseAutoSplit(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            var callCfg = dslCfg;
            if (dslCfg.IsHighOrder)
                callCfg = dslCfg.LowerOrderFunction;

            var levelVal = !callCfg.IsParenthesisParamClass() || callCfg.GetParamNum() <= 0 ? SettingInfo.s_DefSplitLevel : DoCalc(callCfg.GetParam(0));
            if (Calculator.TryGetInt(levelVal, out var splitLevel)) {
                cfg.SettingInfo.AutoSplitLevel = splitLevel;
            }

            foreach (var fp in dslCfg.Params) {
                var fd = fp as Dsl.FunctionData;
                if (null != fd) {
                    string sid = fd.GetId();
                    if (sid == "=") {
                        string key = fd.GetParamId(0);
                        var val = DoCalc(fd.GetParam(1));
                        if (key == "split_level_for_repeat_expression") {
                            if (Calculator.TryGetInt(val, out var v) && v > 0) {
                                cfg.SettingInfo.AutoSplitLevelForRepeatExpression = v;
                            }
                        }
                    }
                    else if (fd.IsParenthesisParamClass()) {
                        if (sid == "split_on") {
                            var v1val = fd.GetParamNum() <= 0 ? DslExpression.CalculatorValue.EmptyString : DoCalc(fd.GetParam(0));
                            var v2val = fd.GetParamNum() <= 1 ? SettingInfo.s_DefSplitOnLevel : DoCalc(fd.GetParam(1));
                            if (Calculator.TryGetString(v1val, out var v1str) && !string.IsNullOrEmpty(v1str) && Calculator.TryGetInt(v2val, out var lvl)) {
                                cfg.SettingInfo.AutoSplitOnFuncs[v1str] = lvl;
                            }
                        }
                        else if (sid == "skip") {
                            var v1val = fd.GetParamNum() <= 0 ? DslExpression.CalculatorValue.EmptyString : DoCalc(fd.GetParam(0));
                            if (Calculator.TryGetString(v1val, out var v1str) && !string.IsNullOrEmpty(v1str)) {
                                if (!cfg.SettingInfo.AutoSplitSkips.Contains(v1str)) {
                                    cfg.SettingInfo.AutoSplitSkips.Add(v1str);
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void ParseVariableAssignment(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var fd = fp as Dsl.FunctionData;
                if (null != fd){
                    string fid = fd.GetId();
                    if (fid == "=") {
                        string key = fd.GetParamId(0);
                        var val = DoCalc(fd.GetParam(1));
                        string valType = Calculator.GetValueType(val);

                        cfg.SettingInfo.VariableAssignments[key] = new ValueInfo { Type = valType, Value = val };
                    }
                }
            }
        }
        private static void ParseObjectMemberAssignment(ShaderConfig cfg, Dsl.FunctionData dslCfg)
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
                            if (!cfg.SettingInfo.ObjectMemberAssignments.TryGetValue(cid, out var members)) {
                                members = new Dictionary<string, ValueInfo>();
                                cfg.SettingInfo.ObjectMemberAssignments.Add(cid, members);
                            }
                            members[member] = vinfo;
                        }
                    }
                }
            }
        }
        private static void ParseArrayElementAssignment(ShaderConfig cfg, Dsl.FunctionData dslCfg)
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
                                if (!cfg.SettingInfo.ArrayElementAssignments.TryGetValue(cid, out var elements)) {
                                    elements = new Dictionary<int, ValueInfo>();
                                    cfg.SettingInfo.ArrayElementAssignments.Add(cid, elements);
                                }
                                elements[index] = vinfo;
                            }
                        }
                    }
                }
            }
        }
        private static void ParseObjectArrayMemberAssignment(ShaderConfig cfg, Dsl.FunctionData dslCfg)
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
                                if (!cfg.SettingInfo.ObjectArrayMemberAssignments.TryGetValue(cid, out var objects)) {
                                    objects = new Dictionary<int, Dictionary<string, ValueInfo>>();
                                    cfg.SettingInfo.ObjectArrayMemberAssignments.Add(cid, objects);
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
        private static void ParseUnassignableVariable(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                AddUnassignableVariable(cfg, fp as Dsl.ValueData);
            }
        }
        private static void ParseUnassignableObjectMember(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var cd = fp as Dsl.FunctionData;
                AddUnassignableObjectMember(cfg, cd);
            }
        }
        private static void ParseUnassignableArrayElement(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var cd = fp as Dsl.FunctionData;
                AddUnassignableArrayElement(cfg, cd);
            }
        }
        private static void ParseUnassignableObjectArrayMember(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var cd = fp as Dsl.FunctionData;
                AddUnassignableObjectArrayMember(cfg, cd);
            }
        }
        private static void ParseShaderArg(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            if (dslCfg.HaveStatement()) {
                int cid = 0;
                if (dslCfg.IsHighOrder) {
                    var callCfg = dslCfg.LowerOrderFunction;
                    var cidStr = callCfg.GetParamId(0).Trim();
                    int.TryParse(cidStr, out cid);
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
        private static void ParseRedirect(ShaderConfig cfg, ShaderArgConfig argCfg, Dsl.FunctionData dslCfg)
        {
            Dsl.FunctionData callCfg;
            if (dslCfg.HaveParam()) {
                callCfg = dslCfg;

                int num = callCfg.GetParamNum();
                string baseCfg = callCfg.GetParamId(0).Trim();
                string newDir = callCfg.GetParamId(1).Trim();
                string newDirVAO = newDir;
                if (num > 2) {
                    newDirVAO = callCfg.GetParamId(2).Trim();
                }

                if (int.TryParse(baseCfg, out var baseCfgId)) {
                    if(cfg.ArgConfigs.TryGetValue(baseCfgId, out var baseCfgInfo)) {
                        argCfg.InOutAttrInfo.InAttrImportFile = ChangeDir(baseCfgInfo.InOutAttrInfo.InAttrImportFile, newDir);
                        argCfg.InOutAttrInfo.OutAttrImportFile = ChangeDir(baseCfgInfo.InOutAttrInfo.OutAttrImportFile, newDir);
                        argCfg.InOutAttrInfo.AttrIndex = baseCfgInfo.InOutAttrInfo.AttrIndex;
                        foreach(var pair in baseCfgInfo.InOutAttrInfo.InAttrMap) {
                            argCfg.InOutAttrInfo.InAttrMap.Add(pair.Key, pair.Value);
                        }
                        foreach (var pair in baseCfgInfo.InOutAttrInfo.OutAttrMap) {
                            argCfg.InOutAttrInfo.OutAttrMap.Add(pair.Key, pair.Value);
                        }
                        foreach(var uniform in baseCfgInfo.UniformImports) {
                            var newCfg = new UniformImportInfo();
                            newCfg.File = ChangeDir(uniform.File, newDir);
                            newCfg.Type = uniform.Type;
                            foreach(var ix in uniform.UsedIndexes) {
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
                        foreach(var vao in baseCfgInfo.VAOImports) {
                            var newCfg = new VAOImportInfo();
                            newCfg.File = ChangeDir(vao.File, newDirVAO);
                            newCfg.Type = vao.Type;
                            newCfg.Attr = vao.Attr;
                            newCfg.AttrArrayLeft = vao.AttrArrayLeft;
                            foreach(var ix in vao.UsedIndexes) {
                                newCfg.UsedIndexes.Add(ix);
                            }
                            argCfg.VAOImports.Add(newCfg);
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

        private static void ParseStringReplacement(ShaderConfig cfg, Dsl.FunctionData dslCfg)
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
                            cfg.StringReplacements.Add(info);
                        }
                        else if (fid == "regex") {
                            var src = fd.GetParamId(0);
                            var dst = fd.GetParamId(1);
                            var regex = new Regex(src, RegexOptions.Compiled);
                            var info = new StringReplacement { Src = src, Dst = dst, SrcRegex = regex };
                            cfg.StringReplacements.Add(info);
                        }
                    }
                }
            }
        }
        private static void ParseCalculator(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            if (dslCfg.HaveStatement()) {
                foreach (var p in dslCfg.Params) {
                    var fd = p as Dsl.FunctionData;
                    if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "=") {
                            var func = fd.GetParam(0) as Dsl.FunctionData;
                            var val = fd.GetParam(1);
                            if (null != func) {
                                ParseCalculatorInfo(cfg, func, val);
                            }
                        }
                    }
                }
            }
        }
        private static void ParseCalculatorInfo(ShaderConfig cfg, Dsl.FunctionData func, Dsl.ISyntaxComponent val)
        {
            if (func.IsHighOrder) {

            }
            else {
                var info = new CalculatorInfo();
                info.Func = func.GetId();
                foreach(var p in func.Params) {
                    info.Args.Add(p.GetId());
                }
                var vd = val as Dsl.ValueData;
                var fd = val as Dsl.FunctionData;
                if (null != vd) {
                    info.OnGetValue = args => { return vd.GetId(); };
                }
                else if (null != fd) {
                    if (fd.GetId() == "func" && fd.IsHighOrder && fd.HaveStatement()) {
                        //这种形式用于直接调dsl脚本，构造一个函数
                        var callData = fd.LowerOrderFunction;
                        List<string> argNames = new List<string>();
                        foreach (var p in callData.Params) {
                            argNames.Add(p.GetId());
                        }
                        string funcId = BatchScript.EvalAsFunc(func, argNames);
                        info.OnGetValue = args => {
                            return BatchScript.Call(funcId, args);
                        };
                    }
                    else {
                        info.OnGetValue = args => {
                            return DoCalc(fd);
                        };
                    }
                }

                if (!cfg.Calculators.TryGetValue(info.Func, out var infos)) {
                    infos = new List<CalculatorInfo>();
                    cfg.Calculators.Add(info.Func, infos);
                }
                infos.Add(info);
            }
        }
        internal static DslExpression.CalculatorValue DoCalc(Dsl.ISyntaxComponent exp)
        {
            bool supported = false;
            var vd = exp as Dsl.ValueData;
            var fd = exp as Dsl.FunctionData;
            var stm = exp as Dsl.StatementData;
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
                        if (Calculator.CalcMember(obj, m, out var val, out supported))
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
                        if (Calculator.CalcMember(obj, m, out var val, out supported))
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
                else if(fd.IsParenthesisParamClass())
                {
                    string func = fd.GetId();
                    var args = new List<DslExpression.CalculatorValue>();
                    foreach(var p in fd.Params) {
                        args.Add(DoCalc(p));
                    }
                    if (Calculator.CalcFunc(func, args, out var val, out supported))
                        return val;
                }
                else if(fd.IsOperatorParamClass()) {
                    string op = fd.GetId();
                    if (fd.GetParamNum() == 2) {
                        var arg1 = fd.GetParam(0);
                        var arg2 = fd.GetParam(1);
                        var a1 = DoCalc(arg1);
                        var a2 = DoCalc(arg2);
                        if (Calculator.CalcBinary(op, a1, a2, out var val, out supported))
                            return val;
                    }
                    else if (fd.GetParamNum() == 1) {
                        var arg1 = fd.GetParam(0);
                        var a1 = DoCalc(arg1);
                        if (Calculator.CalcUnary(op, a1, out var val, out supported))
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
                    if (Calculator.CalcCondExp(vcond, vt, vf, out var val, out supported))
                        return val;
                }
            }
            if (!supported) {
                var r = BatchCommand.BatchScript.EvalAndRun(exp);
                return r;
            }
            return DslExpression.CalculatorValue.NullObject;
        }

        internal static void AddUnassignableVariable(ShaderConfig cfg, Dsl.ValueData? vd)
        {
            if (null != vd) {
                string vname = vd.GetId();
                if (!cfg.SettingInfo.UnassignableVariables.Contains(vname))
                    cfg.SettingInfo.UnassignableVariables.Add(vname);
            }
        }
        internal static void AddUnassignableObjectMember(ShaderConfig cfg, Dsl.FunctionData? cd)
        {
            if (null != cd && cd.IsPeriodParamClass()) {
                string cid = cd.GetId();
                string member = cd.GetParamId(0);
                if (!cfg.SettingInfo.UnassignableObjectMembers.TryGetValue(cid, out var memberHashSet)) {
                    memberHashSet = new HashSet<string>();
                    cfg.SettingInfo.UnassignableObjectMembers.Add(cid, memberHashSet);
                }
                if (!memberHashSet.Contains(member))
                    memberHashSet.Add(member);
            }
        }
        internal static void AddUnassignableArrayElement(ShaderConfig cfg, Dsl.FunctionData? cd)
        {
            if (null != cd && cd.IsBracketParamClass()) {
                string cid = cd.GetId();
                var ixVal = DoCalc(cd.GetParam(0));
                if (Calculator.TryGetInt(ixVal, out var ix)) {
                    if (!cfg.SettingInfo.UnassignableArrayElements.TryGetValue(cid, out var ixHashSet)) {
                        ixHashSet = new HashSet<int>();
                        cfg.SettingInfo.UnassignableArrayElements.Add(cid, ixHashSet);
                    }
                    if (!ixHashSet.Contains(ix))
                        ixHashSet.Add(ix);
                }
            }
        }
        internal static void AddUnassignableObjectArrayMember(ShaderConfig cfg, Dsl.FunctionData? cd)
        {
            if (null != cd && cd.IsPeriodParamClass() && cd.IsHighOrder && cd.LowerOrderFunction.IsBracketParamClass()) {
                string cid = cd.LowerOrderFunction.GetId();
                var ixVal = DoCalc(cd.LowerOrderFunction.GetParam(0));
                string member = cd.GetParamId(0);
                if (Calculator.TryGetInt(ixVal, out var ix)) {
                    if (!cfg.SettingInfo.UnassignableObjectArrayMembers.TryGetValue(cid, out var elemMemberList)) {
                        elemMemberList = new Dictionary<int, HashSet<string>>();
                        cfg.SettingInfo.UnassignableObjectArrayMembers.Add(cid, elemMemberList);
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
            internal bool NeedUniformUtofVals = true;
            internal bool NeedUniformFtouVals = false;
            internal bool DefMultiline = false;
            internal bool DefExpandedOnlyOnce = false;
            internal bool DefSkipValue = false;
            internal bool DefSkipExpression = false;
            internal bool GenerateExpressionList = false;
            internal int DefMaxLevel = 32;
            internal int DefMaxLength = 1024;
            internal int ComputeGraphNodesCapacity = 10240;
            internal int ShaderVariablesCapacity = 1024;
            internal int StringBufferCapacitySurplus = 1024;
            internal int MaxIterations = 32;
            internal int MaxLoop = 256;

            internal int AutoSplitLevel = -1;
            internal int AutoSplitLevelForRepeatExpression = 6;
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

            internal void AutoSplitAddVariable(string vname)
            {
                if (!string.IsNullOrEmpty(vname)) {
                    if (AutoSplitSkips.Contains(vname))
                        return;
                    AddSplitOnVariable(vname);
                }
            }
            internal void AddSplitOnVariable(string vname)
            {
                if (!string.IsNullOrEmpty(vname)) {
                    if (!SplitOnVariables.Contains(vname)) {
                        AddUsedVariable(vname, string.Empty);
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

            internal static DslExpression.CalculatorValue s_DefSplitLevel = 15;
            internal static DslExpression.CalculatorValue s_DefSplitOnLevel = 9;
        }
        internal class InOutAttrInfo
        {
            internal string InAttrImportFile = string.Empty;
            internal string OutAttrImportFile = string.Empty;
            internal int AttrIndex = 0;
            internal Dictionary<string,string> InAttrMap = new Dictionary<string,string>();
            internal Dictionary<string, string> OutAttrMap = new Dictionary<string, string>();
        }
        internal class UniformImportInfo
        {
            internal string File = string.Empty;
            internal string Type = string.Empty;
            internal HashSet<int> UsedIndexes = new HashSet<int>();
        }
        internal class VAOImportInfo
        {
            internal string AttrArrayLeft = string.Empty;
            internal string Type = string.Empty;
            internal string File = string.Empty;
            internal string Attr = string.Empty;
            internal HashSet<int> UsedIndexes = new HashSet<int>();
        }
        internal class SSBOImportInfo
        {
            internal string AttrArrayLeft = string.Empty;
            internal string Type = string.Empty;
            internal string File = string.Empty;
            internal string Attr = string.Empty;
        }
        internal class ShaderArgConfig
        {
            internal int Id = 0;
            internal InOutAttrInfo InOutAttrInfo = new InOutAttrInfo();
            internal List<UniformImportInfo> UniformImports = new List<UniformImportInfo>();
            internal List<VAOImportInfo> VAOImports = new List<VAOImportInfo>();
            internal List<SSBOImportInfo> SSBOImports = new List<SSBOImportInfo>();

            internal static ShaderArgConfig s_Empty = new ShaderArgConfig();
        }
        internal class StringReplacement
        {
            internal string Src = string.Empty;
            internal string Dst = string.Empty;
            internal Regex? SrcRegex = null;
        }
        internal delegate DslExpression.CalculatorValue CalculatorValueDelegation(List<DslExpression.CalculatorValue> args);
        internal class CalculatorInfo
        {
            internal string Func = string.Empty;
            internal List<string> Args = new List<string>();
            internal CalculatorValueDelegation? OnGetValue;
        }
        internal class ShaderConfig
        {
            internal string ShaderType = string.Empty;
            internal SettingInfo SettingInfo = new SettingInfo();
            internal Dictionary<int, ShaderArgConfig> ArgConfigs = new Dictionary<int, ShaderArgConfig>();
            internal List<StringReplacement> StringReplacements = new List<StringReplacement>();
            internal Dictionary<string, List<CalculatorInfo>> Calculators = new Dictionary<string, List<CalculatorInfo>>();
            internal Dictionary<string, string> CodeBlocks = new Dictionary<string, string>();
        }

        internal static ShaderConfig ActiveConfig
        {
            get {
                if(null == s_ActiveConfig) {
                    s_ActiveConfig = new ShaderConfig();
                }
                return s_ActiveConfig;
            }
        }
        private static ShaderConfig? s_ActiveConfig = null;
        internal static ShaderArgConfig ActiveArgConfig
        {
            get {
                if(ActiveConfig.ArgConfigs.TryGetValue(ActiveArgCfgId, out var config)) {
                    return config;
                }
                return ShaderArgConfig.s_Empty;
            }
        }
        internal static int ActiveArgCfgId { get; set; } = 0;

        private static Dictionary<string, ShaderConfig> s_ShaderConfigs = new Dictionary<string, ShaderConfig>();
        internal static Random s_Random = new Random();
    }

    internal sealed class ShaderExp : DslExpression.AbstractExpression
    {
        protected override DslExpression.CalculatorValue DoCalc()
        {
            DslExpression.CalculatorValue val = DslExpression.CalculatorValue.NullObject;
            foreach(var p in m_DslArgs) {
                val = Config.DoCalc(p);
            }
            return val;
        }
        protected override bool Load(Dsl.FunctionData funcData)
        {
            if (funcData.IsHighOrder) {
                Load(funcData.LowerOrderFunction);
            }
            foreach(var p in funcData.Params) {
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
            if(funcData.IsParenthesisParamClass() && !funcData.IsHighOrder) {
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
                    Config.AddUnassignableVariable(Config.ActiveConfig, vd);
                    ret = true;
                }
                else if (null != fd) {
                    if (fd.IsPeriodParamClass() && fd.IsHighOrder && fd.LowerOrderFunction.IsBracketParamClass()) {
                        Config.AddUnassignableObjectArrayMember(Config.ActiveConfig, fd);
                        ret = true;
                    }
                    else if (fd.IsPeriodParamClass()) {
                        Config.AddUnassignableObjectMember(Config.ActiveConfig, fd);
                        ret = true;
                    }
                    else if (fd.IsBracketParamClass()) {
                        Config.AddUnassignableArrayElement(ActiveConfig, fd);
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
                foreach(var v in operands) {
                    list.Add(v.GetInt());
                }
                switch(list.Count) {
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
