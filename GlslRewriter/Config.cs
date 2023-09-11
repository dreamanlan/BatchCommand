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
                                    if (Config.ActiveConfig.SettingInfo.NeedUniformFtouVals) {
                                        sb.Append("// ");
                                        sb.Append(cols[0]);
                                        sb.Append(" = ");
                                        sb.Append("uvec4(");
                                        string vals = cols[1];
                                        var vstrs = vals.Split(",", StringSplitOptions.TrimEntries);
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
                                    if (Config.ActiveConfig.SettingInfo.NeedUniformUtofVals) {
                                        sb.Append("// ");
                                        sb.Append(cols[0]);
                                        sb.Append(" = ");
                                        sb.Append("vec4(");
                                        string vals = cols[1];
                                        var vstrs = vals.Split(",", StringSplitOptions.TrimEntries);
                                        for (int i = 0; i < vstrs.Length; ++i) {
                                            if (i > 0)
                                                sb.Append(", ");
                                            uint.TryParse(vstrs[i], out var v);
                                            sb.Append(Calculator.utof(v).ToString());
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
    }
    internal static class Config
    {
        internal static void LoadConfig(string cfgFilePath, string tmpDir)
        {
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
        internal static bool CalcFunc(string func, IList<string> args, ref string type, out string val)
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
                    val = string.Empty;
                    ret = true;
                }
            }
            else if(Calculator.CalcFunc(func, args, ref type, out val)) {
                ret = true;
            }
            else if (ActiveConfig.Calculators.TryGetValue(func, out var infos)) {
                foreach(var info in infos) {
                    bool match = true;
                    for (int i = 0; i < args.Count && i < info.Args.Count; ++i) {
                        if (args[i] == info.Args[i] || info.Args[i]=="*") {
                        }
                        else {
                            match = false;
                            break;
                        }
                    }
                    if (match && null != info.OnGetValue) {
                        val = info.OnGetValue(ref type);
                        ret = true;
                        break;
                    }
                }
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
                    else if (id == "vs_attr") {
                        ParseInOutAttr(id, cfgInfo, pfd);
                    }
                    else if (id == "ps_attr") {
                        ParseInOutAttr(id, cfgInfo, pfd);
                    }
                    else if (id == "uniform") {
                        ParseUniformImport(cfgInfo, pfd);
                    }
                    else if (id == "calculator") {
                        ParseCalculator(cfgInfo, pfd);
                    }
                }
            }
            s_ShaderConfigs[shaderType] = cfgInfo;
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
                        else if (vid == "comment_out_phi_var_definition_and_assignment") {
                            cfg.SettingInfo.CommentOutPhiVarDefinitionAndAssignment = true;
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
                            string vtype = "int";
                            string vstr = DoCalc(fd.GetParam(1), ref vtype);
                            cfg.SettingInfo.SettingVariables[key] = vstr;

                            if (key == "need_uniform_utof_vals") {
                                if (Calculator.TryParseBool(vstr, out var v)) {
                                    cfg.SettingInfo.NeedUniformUtofVals = v;
                                }
                            }
                            else if (key == "need_uniform_ftou_vals") {
                                if (Calculator.TryParseBool(vstr, out var v)) {
                                    cfg.SettingInfo.NeedUniformFtouVals = v;
                                }
                            }
                            else if (key == "def_max_level") {
                                if (int.TryParse(vstr, out var v) && v > 0) {
                                    cfg.SettingInfo.DefMaxLevel = v;
                                }
                            }
                            else if (key == "def_max_length") {
                                if (int.TryParse(vstr, out var v) && v > 0) {
                                    cfg.SettingInfo.DefMaxLength = v;
                                }
                            }
                            else if (key == "def_max_level_for_variable") {
                                if (int.TryParse(vstr, out var v) && v > 0) {
                                    SplitInfoForVariable.s_DefMaxLvl = v.ToString();
                                }
                            }
                            else if (key == "def_max_length_for_variable") {
                                if (int.TryParse(vstr, out var v) && v > 0) {
                                    SplitInfoForVariable.s_DefMaxLen = v.ToString();
                                }
                            }
                            else if (key == "def_multiline_for_variable") {
                                if (Calculator.TryParseBool(vstr, out var v)) {
                                    SplitInfoForVariable.s_DefMultiline = v.ToString();
                                }
                            }
                            else if (key == "def_expanded_only_once_for_variable") {
                                if (Calculator.TryParseBool(vstr, out var v)) {
                                    SplitInfoForVariable.s_DefExpandOnce = v.ToString();
                                }
                            }
                            else if (key == "compute_graph_nodes_capacity") {
                                if (int.TryParse(vstr, out var v)) {
                                    cfg.SettingInfo.ComputeGraphNodesCapacity = v;
                                }
                            }
                            else if(key== "shader_variables_capacity") {
                                if (int.TryParse(vstr, out var v)) {
                                    cfg.SettingInfo.ShaderVariablesCapacity = v;
                                }
                            }
                            else if(key== "string_buffer_capacity_surplus") {
                                if (int.TryParse(vstr, out var v)) {
                                    cfg.SettingInfo.StringBufferCapacitySurplus = v;
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
                        else if (fid == "invalidate_variable") {
                            ParseInvalidateVariable(cfg, fd);
                        }
                        else if (fid == "invalidate_object_member") {
                            ParseInvalidateObjectMember(cfg, fd);
                        }
                        else if (fid == "invalidate_array_element") {
                            ParseInvalidateArrayElement(cfg, fd);
                        }
                        else if (fid == "invalidate_object_array_member") {
                            ParseInvalidateObjectArrayMember(cfg, fd);
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
                    }
                }
            }
        }
        private static void ParseSplitVariableAssignment(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                string v1str = SplitInfoForVariable.s_DefMaxLvl;
                string v2str = SplitInfoForVariable.s_DefMaxLen;
                string v3str = SplitInfoForVariable.s_DefMultiline;
                string v4str = SplitInfoForVariable.s_DefExpandOnce;
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
                        string v1type = "int";
                        v1str = fd.GetParamNum() <= 1 ? SplitInfoForVariable.s_DefMaxLvl : DoCalc(fd.GetParam(1), ref v1type);
                        string v2type = "int";
                        v2str = fd.GetParamNum() <= 2 ? SplitInfoForVariable.s_DefMaxLen : DoCalc(fd.GetParam(2), ref v2type);
                        string v3type = "bool";
                        v3str = fd.GetParamNum() <= 3 ? SplitInfoForVariable.s_DefMultiline : DoCalc(fd.GetParam(3), ref v3type);
                        string v4type = "bool";
                        v4str = fd.GetParamNum() <= 4 ? SplitInfoForVariable.s_DefExpandOnce : DoCalc(fd.GetParam(4), ref v4type);
                    }
                    else if (sid == "skip") {
                        v1str = "-1";
                    }
                    cfg.SettingInfo.AddSplitOnVariable(key, v1str, v2str, v3str, v4str);
                }
            }
        }
        private static void ParseSplitObjectAssignment(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                string v1str = SplitInfoForVariable.s_DefMaxLvl;
                string v2str = SplitInfoForVariable.s_DefMaxLen;
                string v3str = SplitInfoForVariable.s_DefMultiline;
                string v4str = SplitInfoForVariable.s_DefExpandOnce;
                var cd = fp as Dsl.FunctionData;
                if (null != cd && cd.IsParenthesisParamClass()) {
                    var fd = cd;
                    cd = fd.GetParam(0) as Dsl.FunctionData;

                    string sid = fd.GetId();
                    if (sid == "set") {
                        string v1type = "int";
                        v1str = fd.GetParamNum() <= 1 ? SplitInfoForVariable.s_DefMaxLvl : DoCalc(fd.GetParam(1), ref v1type);
                        string v2type = "int";
                        v2str = fd.GetParamNum() <= 2 ? SplitInfoForVariable.s_DefMaxLen : DoCalc(fd.GetParam(2), ref v2type);
                        string v3type = "bool";
                        v3str = fd.GetParamNum() <= 3 ? SplitInfoForVariable.s_DefMultiline : DoCalc(fd.GetParam(3), ref v3type);
                        string v4type = "bool";
                        v4str = fd.GetParamNum() <= 4 ? SplitInfoForVariable.s_DefExpandOnce : DoCalc(fd.GetParam(4), ref v4type);
                    }
                    else if (sid == "skip") {
                        v1str = "-1";
                    }
                }
                if (null != cd && cd.IsPeriodParamClass() && int.TryParse(v1str, out var lvlForExp) && int.TryParse(v2str, out var lenForExp)
                && Calculator.TryParseBool(v3str, out var ml) && Calculator.TryParseBool(v4str, out var once)) {
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
                string v1str = SplitInfoForVariable.s_DefMaxLvl;
                string v2str = SplitInfoForVariable.s_DefMaxLen;
                string v3str = SplitInfoForVariable.s_DefMultiline;
                string v4str = SplitInfoForVariable.s_DefExpandOnce;
                var cd = fp as Dsl.FunctionData;
                if (null != cd && cd.IsParenthesisParamClass()) {
                    var fd = cd;
                    cd = fd.GetParam(0) as Dsl.FunctionData;

                    string sid = fd.GetId();
                    if (sid == "set") {
                        string v1type = "int";
                        v1str = fd.GetParamNum() <= 1 ? SplitInfoForVariable.s_DefMaxLvl : DoCalc(fd.GetParam(1), ref v1type);
                        string v2type = "int";
                        v2str = fd.GetParamNum() <= 2 ? SplitInfoForVariable.s_DefMaxLen : DoCalc(fd.GetParam(2), ref v2type);
                        string v3type = "bool";
                        v3str = fd.GetParamNum() <= 3 ? SplitInfoForVariable.s_DefMultiline : DoCalc(fd.GetParam(3), ref v3type);
                        string v4type = "bool";
                        v4str = fd.GetParamNum() <= 4 ? SplitInfoForVariable.s_DefExpandOnce : DoCalc(fd.GetParam(4), ref v4type);
                    }
                    else if (sid == "skip") {
                        v1str = "-1";
                    }
                }
                if (null != cd && cd.IsBracketParamClass() && int.TryParse(v1str, out var lvlForExp) && int.TryParse(v2str, out var lenForExp)
                && Calculator.TryParseBool(v3str, out var ml) && Calculator.TryParseBool(v4str, out var once)) {
                    string cid = cd.GetId();
                    string ixType = "int";
                    string ixStr = DoCalc(cd.GetParam(0), ref ixType);
                    if (int.TryParse(ixStr, out var index)) {
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
                string v1str = SplitInfoForVariable.s_DefMaxLvl;
                string v2str = SplitInfoForVariable.s_DefMaxLen;
                string v3str = SplitInfoForVariable.s_DefMultiline;
                string v4str = SplitInfoForVariable.s_DefExpandOnce;
                var cd = fp as Dsl.FunctionData;
                if (null != cd && cd.IsParenthesisParamClass()) {
                    var fd = cd;
                    cd = fd.GetParam(0) as Dsl.FunctionData;

                    string sid = fd.GetId();
                    if (sid == "set") {
                        string v1type = "int";
                        v1str = fd.GetParamNum() <= 1 ? SplitInfoForVariable.s_DefMaxLvl : DoCalc(fd.GetParam(1), ref v1type);
                        string v2type = "int";
                        v2str = fd.GetParamNum() <= 2 ? SplitInfoForVariable.s_DefMaxLen : DoCalc(fd.GetParam(2), ref v2type);
                        string v3type = "bool";
                        v3str = fd.GetParamNum() <= 3 ? SplitInfoForVariable.s_DefMultiline : DoCalc(fd.GetParam(3), ref v3type);
                        string v4type = "bool";
                        v4str = fd.GetParamNum() <= 4 ? SplitInfoForVariable.s_DefExpandOnce : DoCalc(fd.GetParam(4), ref v4type);
                    }
                    else if (sid == "skip") {
                        v1str = "-1";
                    }
                }
                if (null != cd && cd.IsPeriodParamClass() && cd.IsHighOrder && cd.LowerOrderFunction.IsBracketParamClass() && int.TryParse(v1str, out var lvlForExp)
                && int.TryParse(v2str, out var lenForExp) && Calculator.TryParseBool(v3str, out var ml) && Calculator.TryParseBool(v4str, out var once)) {
                    string cid = cd.LowerOrderFunction.GetId();
                    string ixType = "int";
                    string ixStr = DoCalc(cd.LowerOrderFunction.GetParam(0), ref ixType);
                    string member = cd.GetParamId(0);
                    if (int.TryParse(ixStr, out var index)) {
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

            string levelType = "int";
            string levelStr = callCfg.HaveStatement() || callCfg.GetParamNum() <= 0 ? SettingInfo.s_DefSplitLevel : DoCalc(callCfg.GetParam(0), ref levelType);
            if (int.TryParse(levelStr, out var splitLevel)) {
                cfg.SettingInfo.AutoSplitLevel = splitLevel;
            }

            foreach (var fp in dslCfg.Params) {
                var fd = fp as Dsl.FunctionData;
                if (null != fd && fd.IsParenthesisParamClass()) {
                    string sid = fd.GetId();
                    if (sid == "split_on") {
                        string v1type = "int";
                        string v1str = fd.GetParamNum() <= 0 ? string.Empty : DoCalc(fd.GetParam(0), ref v1type);
                        string v2type = "int";
                        string v2str = fd.GetParamNum() <= 1 ? SettingInfo.s_DefSplitOnLevel : DoCalc(fd.GetParam(1), ref v2type);
                        if(!string.IsNullOrEmpty(v1str) && int.TryParse(v2str, out var lvl)) {
                            cfg.SettingInfo.AutoSplitOnFuncs[v1str] = lvl;
                        }
                    }
                    else if (sid == "skip") {
                        string v1type = "int";
                        string v1str = fd.GetParamNum() <= 0 ? string.Empty : DoCalc(fd.GetParam(0), ref v1type);
                        if (!string.IsNullOrEmpty(v1str)) {
                            if (!cfg.SettingInfo.AutoSplitSkips.Contains(v1str)) {
                                cfg.SettingInfo.AutoSplitSkips.Add(v1str);
                            }
                        }
                    }
                }
            }
        }
        private static void ParseInvalidateVariable(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                string vname = fp.GetId();
                if (!cfg.SettingInfo.InvalidatedVariables.Contains(vname))
                    cfg.SettingInfo.InvalidatedVariables.Add(vname);
            }
        }
        private static void ParseInvalidateObjectMember(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var cd = fp as Dsl.FunctionData;
                if (null != cd && cd.IsPeriodParamClass()) {
                    string cid = cd.GetId();
                    string member = cd.GetParamId(0);
                    if (!cfg.SettingInfo.InvalidatedObjectMembers.TryGetValue(cid, out var memberHashSet)) {
                        memberHashSet = new HashSet<string>();
                        cfg.SettingInfo.InvalidatedObjectMembers.Add(cid, memberHashSet);
                    }
                    if (!memberHashSet.Contains(member))
                        memberHashSet.Add(member);
                }
            }
        }
        private static void ParseInvalidateArrayElement(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var cd = fp as Dsl.FunctionData;
                if (null != cd && cd.IsBracketParamClass()) {
                    string cid = cd.GetId();
                    string ixType = "int";
                    string ixStr = DoCalc(cd.GetParam(0), ref ixType);
                    if (int.TryParse(ixStr, out var ix)) {
                        if (!cfg.SettingInfo.InvalidatedArrayElements.TryGetValue(cid, out var ixHashSet)) {
                            ixHashSet = new HashSet<int>();
                            cfg.SettingInfo.InvalidatedArrayElements.Add(cid, ixHashSet);
                        }
                        if (!ixHashSet.Contains(ix))
                            ixHashSet.Add(ix);
                    }
                }
            }
        }
        private static void ParseInvalidateObjectArrayMember(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var cd = fp as Dsl.FunctionData;
                if (null != cd && cd.IsPeriodParamClass() && cd.IsHighOrder && cd.LowerOrderFunction.IsBracketParamClass()) {
                    string cid = cd.LowerOrderFunction.GetId();
                    string ixType = "int";
                    string ixStr = DoCalc(cd.LowerOrderFunction.GetParam(0), ref ixType);
                    string member = cd.GetParamId(0);
                    if (int.TryParse(ixStr, out var ix)) {
                        if (!cfg.SettingInfo.InvalidatedObjectArrayMembers.TryGetValue(cid, out var elemMemberList)) {
                            elemMemberList = new Dictionary<int, HashSet<string>>();
                            cfg.SettingInfo.InvalidatedObjectArrayMembers.Add(cid, elemMemberList);
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
        }
        private static void ParseVariableAssignment(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            foreach (var fp in dslCfg.Params) {
                var fd = fp as Dsl.FunctionData;
                if (null != fd){
                    string fid = fd.GetId();
                    if (fid == "=") {
                        string key = fd.GetParamId(0);
                        string valType = string.Empty;
                        string val = DoCalc(fd.GetParam(1), ref valType);

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
                        string valType = string.Empty;
                        string val = DoCalc(fd.GetParam(1), ref valType);
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
                        string valType = string.Empty;
                        string val = DoCalc(fd.GetParam(1), ref valType);
                        var vinfo = new ValueInfo { Type = valType, Value = val };

                        if (null != cd && cd.IsBracketParamClass()) {
                            string cid = cd.GetId();
                            string ixType = "int";
                            string ixStr = DoCalc(cd.GetParam(0), ref ixType);
                            if (int.TryParse(ixStr, out var index)) {
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
                        string valType = string.Empty;
                        string val = DoCalc(fd.GetParam(1), ref valType);
                        var vinfo = new ValueInfo { Type = valType, Value = val };


                        if (null != cd && cd.IsPeriodParamClass() && cd.IsHighOrder && cd.LowerOrderFunction.IsBracketParamClass()) {
                            string cid = cd.LowerOrderFunction.GetId();
                            string ixType = "int";
                            string ixStr = DoCalc(cd.LowerOrderFunction.GetParam(0), ref ixType);
                            string member = cd.GetParamId(0);
                            if (int.TryParse(ixStr, out var index)) {
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
        private static void ParseInOutAttr(string id, ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            var callCfg = dslCfg;
            if (dslCfg.IsHighOrder)
                callCfg = dslCfg.LowerOrderFunction;

            var info = cfg.InOutAttrInfo;
            if (id == "vs_attr") {
                string inAttr = callCfg.GetParamId(0).Trim();
                string outAttr = callCfg.GetParamId(1).Trim();
                string indexType = "int";
                string indexStr = DoCalc(callCfg.GetParam(2), ref indexType);
                if (int.TryParse(indexStr, out var ix)) {
                    info.InAttrImportFile = inAttr;
                    info.OutAttrImportFile = outAttr;
                    info.AttrIndex = ix;
                }
            }
            else if (id == "ps_attr") {
                string inAttr = callCfg.GetParamId(0).Trim();
                string indexType = "int";
                string indexStr = DoCalc(callCfg.GetParam(1), ref indexType);
                if (int.TryParse(indexStr, out var ix)) {
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
        private static void ParseUniformImport(ShaderConfig cfg, Dsl.FunctionData dslCfg)
        {
            var callCfg = dslCfg;
            if (dslCfg.IsHighOrder)
                callCfg = dslCfg.LowerOrderFunction;

            string file = callCfg.GetParamId(0).Trim();
            string type = callCfg.GetParamId(1).Trim();
            var info = new UniformImportInfo { File = file, Type = type };

            if (dslCfg.HaveStatement()) {
                foreach (var p in dslCfg.Params) {
                    var vd = p as Dsl.ValueData;
                    var fd = p as Dsl.FunctionData;
                    if (null != vd) {
                        string ixType = "int";
                        string ixStr = DoCalc(vd, ref ixType);
                        if (int.TryParse(ixStr, out var ix)) {
                            if (!info.UsedIndexes.Contains(ix))
                                info.UsedIndexes.Add(ix);
                        }
                    }
                    else if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "add") {
                            foreach (var fp in fd.Params) {
                                string ixType = "int";
                                string ixStr = DoCalc(fp, ref ixType);
                                if (int.TryParse(ixStr, out var ix)) {
                                    if (!info.UsedIndexes.Contains(ix))
                                        info.UsedIndexes.Add(ix);
                                }
                            }
                        }
                        else if (fid == "remove") {
                            foreach (var fp in fd.Params) {
                                string ixType = "int";
                                string ixStr = DoCalc(fp, ref ixType);
                                if (int.TryParse(ixStr, out var ix)) {
                                    info.UsedIndexes.Remove(ix);
                                }
                            }
                        }
                        else if (fid == "add_range") {
                            string ix1Type = "int";
                            string ix1Str = DoCalc(fd.GetParam(0), ref ix1Type);
                            string ix2Type = "int";
                            string ix2Str = DoCalc(fd.GetParam(1), ref ix2Type);
                            if (int.TryParse(ix1Str, out var ix1) && int.TryParse(ix2Str, out var ix2)) {
                                for (int i = ix1; i <= ix2; ++i) {
                                    if (!info.UsedIndexes.Contains(i))
                                        info.UsedIndexes.Add(i);
                                }
                            }
                        }
                        else if (fid == "remove_range") {
                            string ix1Type = "int";
                            string ix1Str = DoCalc(fd.GetParam(0), ref ix1Type);
                            string ix2Type = "int";
                            string ix2Str = DoCalc(fd.GetParam(1), ref ix2Type);
                            if (int.TryParse(ix1Str, out var ix1) && int.TryParse(ix2Str, out var ix2)) {
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
                    info.OnGetValue = (ref string type) => { return vd.GetId(); };
                }
                else if (null != fd) {
                    string fid = fd.GetId();
                    if (fid == "rand_color") {
                        info.OnGetValue = (ref string type) => {
                            type = "vec4";
                            var sb = new StringBuilder();
                            sb.Append("vec4");
                            sb.Append("(");
                            sb.Append(s_Random.NextDouble());
                            sb.Append(",");
                            sb.Append(s_Random.NextDouble());
                            sb.Append(",");
                            sb.Append(s_Random.NextDouble());
                            sb.Append(",");
                            sb.Append(s_Random.NextDouble());
                            sb.Append(")");
                            return sb.ToString();
                        };
                    }
                    else if (fid == "rand_uv") {
                        info.OnGetValue = (ref string type) => {
                            string argType = "int";
                            string argStr = DoCalc(fd.GetParam(0), ref argType);
                            int.TryParse(argStr, out var num);
                            if (num >= 2 && num <= 4) {
                                type = "vec" + num;
                                var sb = new StringBuilder();
                                sb.Append("vec");
                                sb.Append(num);
                                sb.Append("(");
                                sb.Append(s_Random.NextDouble());
                                for (int i = 0; i < num - 1; ++i) {
                                    sb.Append(",");
                                    sb.Append(s_Random.NextDouble());
                                }
                                sb.Append(")");
                                return sb.ToString();
                            }
                            else {
                                type = string.Empty;
                                return string.Empty;
                            }
                        };
                    }
                    else if (fid == "rand_size") {
                        info.OnGetValue = (ref string type) => {
                            List<int> list = new List<int>();
                            foreach (var arg in fd.Params) {
                                string argType = "int";
                                string argStr = DoCalc(arg, ref argType);
                                int.TryParse(argStr, out var v);
                                list.Add(v);
                            }
                            var sb = new StringBuilder();
                            sb.Append("vec");
                            sb.Append(list.Count);
                            sb.Append("(");
                            for (int i = 0; i < list.Count; ++i) {
                                if (i > 0)
                                    sb.Append(",");
                                sb.Append(s_Random.Next(list[i]));
                            }
                            sb.Append(")");
                            return sb.ToString();
                        };
                    }
                    else {
                        info.OnGetValue = (ref string type) => {
                            return DoCalc(fd, ref type);
                        };
                    }
                }

                if(!cfg.Calculators.TryGetValue(info.Func, out var infos)) {
                    infos = new List<CalculatorInfo>();
                    cfg.Calculators.Add(info.Func, infos);
                }
                infos.Add(info);
            }
        }
        private static string DoCalc(Dsl.ISyntaxComponent exp, ref string type)
        {
            var vd = exp as Dsl.ValueData;
            var fd = exp as Dsl.FunctionData;
            var stm = exp as Dsl.StatementData;
            if (null != vd) {
                string vstr = vd.GetId();
                if(ActiveConfig.SettingInfo.SettingVariables.TryGetValue(vstr, out var v)) {
                    vstr = v;
                }
                string val = Calculator.ReStringNumeric(vstr, ref type);
                return val;
            }
            else if (null != fd) {
                if (fd.IsPeriodParamClass()) {
                    if (fd.IsHighOrder) {
                        string objType = string.Empty;
                        string obj = DoCalc(fd.LowerOrderFunction, ref objType);
                        string m = fd.GetParamId(0);
                        if (Calculator.CalcMember(objType, obj, m, ref type, out var val))
                            return val;
                    }
                }
                else if (fd.IsBracketParamClass()) {
                    if (fd.IsHighOrder) {
                        string objType = string.Empty;
                        string obj = DoCalc(fd.LowerOrderFunction, ref objType);
                        string ixType = "int";
                        string m = DoCalc(fd.GetParam(0), ref ixType);
                        if (Calculator.CalcMember(objType, obj, m, ref type, out var val))
                            return val;
                    }
                }
                else if(fd.IsParenthesisParamClass())
                {
                    string func = fd.GetId();
                    List<string> args = new List<string>();
                    foreach(var p in fd.Params) {
                        string argType = string.Empty;
                        args.Add(DoCalc(p, ref argType));
                    }
                    if (Calculator.CalcFunc(func, args, ref type, out var val))
                        return val;
                }
                else if(fd.IsOperatorParamClass()) {
                    string op = fd.GetId();
                    if (fd.GetParamNum() == 2) {
                        var arg1 = fd.GetParam(0);
                        var arg2 = fd.GetParam(1);
                        string argType = string.Empty;
                        var a1 = DoCalc(arg1, ref argType);
                        var a2 = DoCalc(arg2, ref argType);
                        if (Calculator.CalcBinary(op, a1, a2, ref type, out var val))
                            return val;
                    }
                    else if (fd.GetParamNum() == 1) {
                        var arg1 = fd.GetParam(0);
                        string argType = string.Empty;
                        var a1 = DoCalc(arg1, ref argType);
                        if (Calculator.CalcUnary(op, a1, ref type, out var val))
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
                    string argType = "bool";
                    var vcond = DoCalc(cond, ref argType);
                    var vt = DoCalc(tval, ref type);
                    var vf = DoCalc(fval, ref type);
                    if (Calculator.CalcCondExp(vcond, vt, vf, ref type, out var val))
                        return val;
                }
            }
            return string.Empty;
        }

        internal class SplitInfoForVariable
        {
            internal int MaxLevel = 0;
            internal int MaxLength = 0;
            internal bool Multiline = false;
            internal bool ExpandedOnlyOnce = false;

            internal static string s_DefMaxLvl = "256";
            internal static string s_DefMaxLen = "102400";
            internal static string s_DefMultiline = "True";
            internal static string s_DefExpandOnce = "True";
        }
        internal class ValueInfo
        {
            internal string Type = string.Empty;
            internal string Value = string.Empty;
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
            internal bool CommentOutPhiVarDefinitionAndAssignment = false;
            internal int DefMaxLevel = 32;
            internal int DefMaxLength = 512;
            internal int ComputeGraphNodesCapacity = 10240;
            internal int ShaderVariablesCapacity = 1024;
            internal int StringBufferCapacitySurplus = 1024;

            internal int AutoSplitLevel = -1;
            internal Dictionary<string, int> AutoSplitOnFuncs = new Dictionary<string, int>();
            internal HashSet<string> AutoSplitSkips = new HashSet<string>();

            internal HashSet<string> SplitOnVariables = new HashSet<string>();
            internal Dictionary<string, SplitInfoForVariable> VariableSplitInfos = new Dictionary<string, SplitInfoForVariable>();
            internal Dictionary<string, Dictionary<string, SplitInfoForVariable>> ObjectSplitInfos = new Dictionary<string, Dictionary<string, SplitInfoForVariable>>();
            internal Dictionary<string, Dictionary<int, SplitInfoForVariable>> ArraySplitInfos = new Dictionary<string, Dictionary<int, SplitInfoForVariable>>();
            internal Dictionary<string, Dictionary<int, Dictionary<string, SplitInfoForVariable>>> ObjectArraySplitInfos = new Dictionary<string, Dictionary<int, Dictionary<string, SplitInfoForVariable>>>();

            internal HashSet<string> InvalidatedVariables = new HashSet<string>();
            internal Dictionary<string, HashSet<string>> InvalidatedObjectMembers = new Dictionary<string, HashSet<string>>();
            internal Dictionary<string, HashSet<int>> InvalidatedArrayElements = new Dictionary<string, HashSet<int>>();
            internal Dictionary<string, Dictionary<int, HashSet<string>>> InvalidatedObjectArrayMembers = new Dictionary<string, Dictionary<int, HashSet<string>>>();

            internal Dictionary<string, ValueInfo> VariableAssignments = new Dictionary<string, ValueInfo>();
            internal Dictionary<string, Dictionary<string, ValueInfo>> ObjectMemberAssignments = new Dictionary<string, Dictionary<string, ValueInfo>>();
            internal Dictionary<string, Dictionary<int, ValueInfo>> ArrayElementAssignments = new Dictionary<string, Dictionary<int, ValueInfo>>();
            internal Dictionary<string, Dictionary<int, Dictionary<string, ValueInfo>>> ObjectArrayMemberAssignments = new Dictionary<string, Dictionary<int, Dictionary<string, ValueInfo>>>();

            internal Dictionary<string, string> SettingVariables = new Dictionary<string, string>();
            internal SortedSet<string> AutoSplitAddedVariables = new SortedSet<string>();

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
                        AutoSplitAddedVariables.Add("\t" + vname + ",");
                    }

                    AddSplitOnVariable(vname, SplitInfoForVariable.s_DefMaxLvl, SplitInfoForVariable.s_DefMaxLen, SplitInfoForVariable.s_DefMultiline, SplitInfoForVariable.s_DefExpandOnce);
                }
            }
            internal void AddSplitOnVariable(string vname, string v1str, string v2str, string v3str, string v4str)
            {
                if (!string.IsNullOrEmpty(vname)) {
                    if (!SplitOnVariables.Contains(vname))
                        SplitOnVariables.Add(vname);

                    if (int.TryParse(v1str, out var lvlForExp) && int.TryParse(v2str, out var lenForExp)
                        && Calculator.TryParseBool(v3str, out var ml) && Calculator.TryParseBool(v4str, out var once)) {
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

            internal static string s_DefSplitLevel = "15";
            internal static string s_DefSplitOnLevel = "12";
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
        internal delegate string CalculatorValueDelegation(ref string type);
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
            internal InOutAttrInfo InOutAttrInfo = new InOutAttrInfo();
            internal List<UniformImportInfo> UniformImports = new List<UniformImportInfo>();
            internal Dictionary<string, List<CalculatorInfo>> Calculators = new Dictionary<string, List<CalculatorInfo>>();
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

        private static Dictionary<string, ShaderConfig> s_ShaderConfigs = new Dictionary<string, ShaderConfig>();
        private static Random s_Random = new Random();
    }
}
