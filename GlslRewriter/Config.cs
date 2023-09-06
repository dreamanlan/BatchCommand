using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
        internal static bool CalcMaxLevel(Dsl.ISyntaxComponent varDsl, out int maxLevelForVal, out int maxLevelForExp)
        {
            var settingInfo = ActiveConfig.SettingInfo;
            maxLevelForVal = settingInfo.DefMaxLevelForValue;
            maxLevelForExp = settingInfo.DefMaxLevelForExpression;

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
                if(settingInfo.VariableMaxLevelInfos.TryGetValue(vname, out var lvlInfo)) {
                    maxLevelForVal = lvlInfo.MaxLevelForValue;
                    maxLevelForExp = lvlInfo.MaxLevelForExpression;
                }
            }
            else if (null != fd) {
                if(fd.IsPeriodParamClass() && fd.IsHighOrder && fd.LowerOrderFunction.IsBracketParamClass()) {
                    string cid = fd.LowerOrderFunction.GetId();
                    string ixStr = fd.LowerOrderFunction.GetParamId(0);
                    string member = fd.GetParamId(0);
                    if(int.TryParse(ixStr, out var index) && settingInfo.ObjectArrayMaxLevelInfos.TryGetValue(cid, out var objArrLvlInfo)
                        && objArrLvlInfo.TryGetValue(index, out var objLvlInfo)
                        && objLvlInfo.TryGetValue(member, out var lvlInfo)) {
                        maxLevelForVal = lvlInfo.MaxLevelForValue;
                        maxLevelForExp = lvlInfo.MaxLevelForExpression;
                    }
                }
                else if (fd.IsPeriodParamClass()) {
                    string cid = fd.GetId();
                    string member = fd.GetParamId(0);
                    if(settingInfo.ObjectMaxLevelInfos.TryGetValue(cid, out var objLvlInfo) && objLvlInfo.TryGetValue(member, out var lvlInfo)) {
                        maxLevelForVal = lvlInfo.MaxLevelForValue;
                        maxLevelForExp = lvlInfo.MaxLevelForExpression;
                    }
                }
                else if (fd.IsBracketParamClass()) {
                    string cid = fd.GetId();
                    string ixStr = fd.GetParamId(0);
                    if (int.TryParse(ixStr, out var index) && settingInfo.ArrayMaxLevelInfos.TryGetValue(cid, out var arrLvlInfo) && arrLvlInfo.TryGetValue(index, out var lvlInfo)) {
                        maxLevelForVal = lvlInfo.MaxLevelForValue;
                        maxLevelForExp = lvlInfo.MaxLevelForExpression;
                    }
                }
            }
            return maxLevelForVal >= 0 && maxLevelForExp >= 0;
        }
        internal static bool CalcFunc(string func, IList<string> args, ref string type, out string val)
        {
            bool ret = false;
            if(Calculator.CalcFunc(func, args, ref type, out val)) {
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
                        if (vid == "def_skip_all_comments") {
                            cfg.SettingInfo.DefMaxLevelForValue = -1;
                            cfg.SettingInfo.DefMaxLevelForExpression = -1;
                        }
                    }
                    else if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "=") {
                            string key = fd.GetParamId(0);
                            string vstr = fd.GetParamId(1);
                            if (key == "def_max_level_for_value") {
                                if (int.TryParse(vstr, out var v)) {
                                    cfg.SettingInfo.DefMaxLevelForValue = v;
                                }
                            }
                            else if (key == "def_max_level_for_expression") {
                                if (int.TryParse(vstr, out var v)) {
                                    cfg.SettingInfo.DefMaxLevelForExpression = v;
                                }
                            }
                        }
                        else if (fid == "set_variable_max_level") {
                            string key = fd.GetParamId(0);
                            string v1str = fd.GetParamId(1);
                            string v2str = fd.GetParamId(2);
                            if(int.TryParse(v1str, out var lvlForVal) && int.TryParse(v2str, out var lvlForExp)) {
                                if(!cfg.SettingInfo.VariableMaxLevelInfos.TryGetValue(key, out var lvlInfo)) {
                                    lvlInfo = new MaxLevelInfo();
                                    cfg.SettingInfo.VariableMaxLevelInfos.Add(key, lvlInfo);
                                }
                                lvlInfo.MaxLevelForValue = lvlForVal;
                                lvlInfo.MaxLevelForExpression = lvlForExp;
                            }
                        }
                        else if (fid == "set_object_max_level") {
                            var cd = fd.GetParam(0) as Dsl.FunctionData;
                            string v1str = fd.GetParamId(1);
                            string v2str = fd.GetParamId(2);
                            if (null != cd && cd.IsPeriodParamClass() && int.TryParse(v1str, out var lvlForVal) && int.TryParse(v2str, out var lvlForExp)) {
                                string cid = cd.GetId();
                                string member = cd.GetParamId(0);
                                if (!cfg.SettingInfo.ObjectMaxLevelInfos.TryGetValue(cid, out var objLvlInfo)) {
                                    objLvlInfo = new Dictionary<string, MaxLevelInfo>();
                                    cfg.SettingInfo.ObjectMaxLevelInfos.Add(cid, objLvlInfo);
                                }
                                if(!objLvlInfo.TryGetValue(member, out var lvlInfo)) {
                                    lvlInfo = new MaxLevelInfo();
                                    objLvlInfo.Add(member, lvlInfo);
                                }
                                lvlInfo.MaxLevelForValue = lvlForVal;
                                lvlInfo.MaxLevelForExpression = lvlForExp;
                            }
                        }
                        else if (fid == "set_array_max_level") {
                            var cd = fd.GetParam(0) as Dsl.FunctionData;
                            string v1str = fd.GetParamId(1);
                            string v2str = fd.GetParamId(2);
                            if (null != cd && cd.IsBracketParamClass() && int.TryParse(v1str, out var lvlForVal) && int.TryParse(v2str, out var lvlForExp)) {
                                string cid = cd.GetId();
                                string ixStr = cd.GetParamId(0);
                                if (int.TryParse(ixStr, out var index)) {
                                    if (!cfg.SettingInfo.ArrayMaxLevelInfos.TryGetValue(cid, out var arrLvlInfo)) {
                                        arrLvlInfo = new Dictionary<int, MaxLevelInfo>();
                                        cfg.SettingInfo.ArrayMaxLevelInfos.Add(cid, arrLvlInfo);
                                    }
                                    if (!arrLvlInfo.TryGetValue(index, out var lvlInfo)) {
                                        lvlInfo = new MaxLevelInfo();
                                        arrLvlInfo.Add(index, lvlInfo);
                                    }
                                    lvlInfo.MaxLevelForValue = lvlForVal;
                                    lvlInfo.MaxLevelForExpression = lvlForExp;
                                }
                            }
                        }
                        else if (fid == "set_object_array_max_level") {
                            var cd = fd.GetParam(0) as Dsl.FunctionData;
                            string v1str = fd.GetParamId(1);
                            string v2str = fd.GetParamId(2);
                            if (null != cd && cd.IsPeriodParamClass() && cd.IsHighOrder && cd.LowerOrderFunction.IsBracketParamClass() && int.TryParse(v1str, out var lvlForVal) && int.TryParse(v2str, out var lvlForExp)) {
                                string cid = cd.LowerOrderFunction.GetId();
                                string ixStr = cd.LowerOrderFunction.GetParamId(0);
                                string member = cd.GetParamId(0);
                                if (int.TryParse(ixStr, out var index)) {
                                    if (!cfg.SettingInfo.ObjectArrayMaxLevelInfos.TryGetValue(cid, out var objArrLvlInfo)) {
                                        objArrLvlInfo = new Dictionary<int, Dictionary<string, MaxLevelInfo>>();
                                        cfg.SettingInfo.ObjectArrayMaxLevelInfos.Add(cid, objArrLvlInfo);
                                    }
                                    if (!objArrLvlInfo.TryGetValue(index, out var objLvlInfo)) {
                                        objLvlInfo = new Dictionary<string, MaxLevelInfo>();
                                        objArrLvlInfo.Add(index, objLvlInfo);
                                    }
                                    if (!objLvlInfo.TryGetValue(member, out var lvlInfo)) {
                                        lvlInfo = new MaxLevelInfo();
                                        objLvlInfo.Add(member, lvlInfo);
                                    }
                                    lvlInfo.MaxLevelForValue = lvlForVal;
                                    lvlInfo.MaxLevelForExpression = lvlForExp;
                                }
                            }
                        }
                        else if (fid == "skip_variable_comment") {
                            foreach (var fp in fd.Params) {
                                string vname = fp.GetId();
                                if (!cfg.SettingInfo.VariableMaxLevelInfos.TryGetValue(vname, out var lvlInfo)) {
                                    lvlInfo = new MaxLevelInfo();
                                    cfg.SettingInfo.VariableMaxLevelInfos.Add(vname, lvlInfo);
                                }
                                lvlInfo.MaxLevelForValue = -1;
                                lvlInfo.MaxLevelForExpression = -1;
                            }
                        }
                        else if (fid == "skip_object_comment") {
                            foreach (var fp in fd.Params) {
                                var cd = fp as Dsl.FunctionData;
                                if (null != cd && cd.IsPeriodParamClass()) {
                                    string cid = cd.GetId();
                                    string member = cd.GetParamId(0);
                                    if (!cfg.SettingInfo.ObjectMaxLevelInfos.TryGetValue(cid, out var objLvlInfo)) {
                                        objLvlInfo = new Dictionary<string, MaxLevelInfo>();
                                        cfg.SettingInfo.ObjectMaxLevelInfos.Add(cid, objLvlInfo);
                                    }
                                    if (!objLvlInfo.TryGetValue(member, out var lvlInfo)) {
                                        lvlInfo = new MaxLevelInfo();
                                        objLvlInfo.Add(member, lvlInfo);
                                    }
                                    lvlInfo.MaxLevelForValue = -1;
                                    lvlInfo.MaxLevelForExpression = -1;
                                }
                            }
                        }
                        else if (fid == "skip_array_comment") {
                            foreach (var fp in fd.Params) {
                                var cd = fp as Dsl.FunctionData;
                                if (null != cd && cd.IsBracketParamClass()) {
                                    string cid = cd.GetId();
                                    string ixStr = cd.GetParamId(0);
                                    if (int.TryParse(ixStr, out var index)) {
                                        if (!cfg.SettingInfo.ArrayMaxLevelInfos.TryGetValue(cid, out var arrLvlInfo)) {
                                            arrLvlInfo = new Dictionary<int, MaxLevelInfo>();
                                            cfg.SettingInfo.ArrayMaxLevelInfos.Add(cid, arrLvlInfo);
                                        }
                                        if (!arrLvlInfo.TryGetValue(index, out var lvlInfo)) {
                                            lvlInfo = new MaxLevelInfo();
                                            arrLvlInfo.Add(index, lvlInfo);
                                        }
                                        lvlInfo.MaxLevelForValue = -1;
                                        lvlInfo.MaxLevelForExpression = -1;
                                    }
                                }
                            }
                        }
                        else if (fid == "skip_object_array_comment") {
                            foreach (var fp in fd.Params) {
                                var cd = fp as Dsl.FunctionData;
                                if (null != cd && cd.IsPeriodParamClass() && cd.IsHighOrder && cd.LowerOrderFunction.IsBracketParamClass()) {
                                    string cid = cd.LowerOrderFunction.GetId();
                                    string ixStr = cd.LowerOrderFunction.GetParamId(0);
                                    string member = cd.GetParamId(0);
                                    if (int.TryParse(ixStr, out var index)) {
                                        if (!cfg.SettingInfo.ObjectArrayMaxLevelInfos.TryGetValue(cid, out var objArrLvlInfo)) {
                                            objArrLvlInfo = new Dictionary<int, Dictionary<string, MaxLevelInfo>>();
                                            cfg.SettingInfo.ObjectArrayMaxLevelInfos.Add(cid, objArrLvlInfo);
                                        }
                                        if (!objArrLvlInfo.TryGetValue(index, out var objLvlInfo)) {
                                            objLvlInfo = new Dictionary<string, MaxLevelInfo>();
                                            objArrLvlInfo.Add(index, objLvlInfo);
                                        }
                                        if (!objLvlInfo.TryGetValue(member, out var lvlInfo)) {
                                            lvlInfo = new MaxLevelInfo();
                                            objLvlInfo.Add(member, lvlInfo);
                                        }
                                        lvlInfo.MaxLevelForValue = -1;
                                        lvlInfo.MaxLevelForExpression = -1;
                                    }
                                }
                            }
                        }
                        else if (fid == "invalid_variable") {
                            foreach (var fp in fd.Params) {
                                string vname = fp.GetId();
                                if (!cfg.SettingInfo.InvalidVariables.Contains(vname))
                                    cfg.SettingInfo.InvalidVariables.Add(vname);
                            }
                        }
                        else if (fid == "invalid_object_member") {
                            foreach (var fp in fd.Params) {
                                var cd = fp as Dsl.FunctionData;
                                if (null != cd && cd.IsPeriodParamClass()) {
                                    string cid = cd.GetId();
                                    string member = cd.GetParamId(0);
                                    if (!cfg.SettingInfo.InvalidObjectMembers.TryGetValue(cid, out var memberHashSet)) {
                                        memberHashSet = new HashSet<string>();
                                        cfg.SettingInfo.InvalidObjectMembers.Add(cid, memberHashSet);
                                    }
                                    if (!memberHashSet.Contains(member))
                                        memberHashSet.Add(member);
                                }
                            }
                        }
                        else if (fid == "invalid_array_element") {
                            foreach (var fp in fd.Params) {
                                var cd = fp as Dsl.FunctionData;
                                if (null != cd && cd.IsBracketParamClass()) {
                                    string cid = cd.GetId();
                                    string ixStr = cd.GetParamId(0);
                                    if (int.TryParse(ixStr, out var ix)) {
                                        if (!cfg.SettingInfo.InvalidArrayElements.TryGetValue(cid, out var ixHashSet)) {
                                            ixHashSet = new HashSet<int>();
                                            cfg.SettingInfo.InvalidArrayElements.Add(cid, ixHashSet);
                                        }
                                        if (!ixHashSet.Contains(ix))
                                            ixHashSet.Add(ix);
                                    }
                                }
                            }
                        }
                        else if (fid == "invalid_object_array_member") {
                            foreach (var fp in fd.Params) {
                                var cd = fp as Dsl.FunctionData;
                                if (null != cd && cd.IsPeriodParamClass() && cd.IsHighOrder && cd.LowerOrderFunction.IsBracketParamClass()) {
                                    string cid = cd.LowerOrderFunction.GetId();
                                    string ixStr = cd.LowerOrderFunction.GetParamId(0);
                                    string member = cd.GetParamId(0);
                                    if (int.TryParse(ixStr, out var ix)) {
                                        if (!cfg.SettingInfo.InvalidObjectArrayMembers.TryGetValue(cid, out var elemMemberList)) {
                                            elemMemberList = new Dictionary<int, HashSet<string>>();
                                            cfg.SettingInfo.InvalidObjectArrayMembers.Add(cid, elemMemberList);
                                        }
                                        if(!elemMemberList.TryGetValue(ix, out var memberHashSet)) {
                                            memberHashSet = new HashSet<string>();
                                            elemMemberList.Add(ix, memberHashSet);
                                        }
                                        if (!memberHashSet.Contains(member))
                                            memberHashSet.Add(member);
                                    }
                                }
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
                string indexStr = callCfg.GetParamId(2).Trim();
                if (int.TryParse(indexStr, out var ix)) {
                    info.InAttrImportFile = inAttr;
                    info.OutAttrImportFile = outAttr;
                    info.AttrIndex = ix;
                }
            }
            else if (id == "ps_attr") {
                string inAttr = callCfg.GetParamId(0).Trim();
                string indexStr = callCfg.GetParamId(1).Trim();
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
                        string ixStr = vd.GetId();
                        if (int.TryParse(ixStr, out var ix)) {
                            if (!info.UsedIndexes.Contains(ix))
                                info.UsedIndexes.Add(ix);
                        }
                    }
                    else if (null != fd) {
                        string fid = fd.GetId();
                        if (fid == "add") {
                            foreach (var fp in fd.Params) {
                                string ixStr = fp.GetId();
                                if (int.TryParse(ixStr, out var ix)) {
                                    if (!info.UsedIndexes.Contains(ix))
                                        info.UsedIndexes.Add(ix);
                                }
                            }
                        }
                        else if (fid == "remove") {
                            foreach (var fp in fd.Params) {
                                string ixStr = fp.GetId();
                                if (int.TryParse(ixStr, out var ix)) {
                                    info.UsedIndexes.Remove(ix);
                                }
                            }
                        }
                        else if (fid == "add_range") {
                            string ix1Str = fd.GetParamId(0);
                            string ix2Str = fd.GetParamId(1);
                            if (int.TryParse(ix1Str, out var ix1) && int.TryParse(ix2Str, out var ix2)) {
                                for (int i = ix1; i <= ix2; ++i) {
                                    if (!info.UsedIndexes.Contains(i))
                                        info.UsedIndexes.Add(i);
                                }
                            }
                        }
                        else if (fid == "remove_range") {
                            string ix1Str = fd.GetParamId(0);
                            string ix2Str = fd.GetParamId(1);
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
                return vd.GetId();
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

        internal class MaxLevelInfo
        {
            internal int MaxLevelForValue = 0;
            internal int MaxLevelForExpression = 0;
        }
        internal class SettingInfo
        {
            internal int DefMaxLevelForValue = 16;
            internal int DefMaxLevelForExpression = 2048;
            internal Dictionary<string, MaxLevelInfo> VariableMaxLevelInfos = new Dictionary<string, MaxLevelInfo>();
            internal Dictionary<string, Dictionary<string, MaxLevelInfo>> ObjectMaxLevelInfos = new Dictionary<string, Dictionary<string, MaxLevelInfo>>();
            internal Dictionary<string, Dictionary<int, MaxLevelInfo>> ArrayMaxLevelInfos = new Dictionary<string, Dictionary<int, MaxLevelInfo>>();
            internal Dictionary<string, Dictionary<int, Dictionary<string, MaxLevelInfo>>> ObjectArrayMaxLevelInfos = new Dictionary<string, Dictionary<int, Dictionary<string, MaxLevelInfo>>>();
            internal HashSet<string> InvalidVariables = new HashSet<string>();
            internal Dictionary<string, HashSet<string>> InvalidObjectMembers = new Dictionary<string, HashSet<string>>();
            internal Dictionary<string, HashSet<int>> InvalidArrayElements = new Dictionary<string, HashSet<int>>();
            internal Dictionary<string, Dictionary<int, HashSet<string>>> InvalidObjectArrayMembers = new Dictionary<string, Dictionary<int, HashSet<string>>>();
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
