using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlslRewriter
{
    internal static class RenderDocImporter
    {
        internal static List<string> GenenerateVsInOutAttr(string type, int index, string csv_path)
        {
            var results = new List<string>();
            var sb = new StringBuilder();
            var lines = File.ReadAllLines(csv_path);
            if (lines.Length >= 2) {
                var header = lines[0];
                var colNames = header.Split(",");
                if (colNames.Length >= 2 && colNames[0].Trim() == "VTX" && colNames[1].Trim() == "IDX") {
                    int ix = index + 1;
                    if (ix >= 0 && ix < lines.Length) {
                        var line = lines[ix];
                        var cols = SplitCsvLine(line);
                        Debug.Assert(colNames.Length == cols.Count);
                        for (int i = 2; i < colNames.Length && i < cols.Count; ++i) {
                            sb.Append(colNames[i].Trim());
                            sb.Append(" = ");
                            sb.Append(type);
                            sb.Append("(");
                            sb.Append(cols[i].Trim());
                            sb.Append(");");

                            results.Add(sb.ToString());
                            sb.Length = 0;
                        }
                    }
                }
            }
            return results;
        }
        internal static List<string> GenerateUniform(string type, string csv_path)
        {
            var results = new List<string>();
            var sb = new StringBuilder();
            var lines = File.ReadAllLines(csv_path);
            if (lines.Length >= 2) {
                var header = lines[0];
                var colNames = header.Split(",");
                if (colNames.Length >= 2 && colNames[0].Trim() == "Name" && colNames[1].Trim() == "Value") {
                    for (int ix = 1; ix < lines.Length; ++ix) {
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
        }
        private static void AddShaderConfig(string shaderType, Dsl.FunctionData shaderCfg)
        {
            var cfgInfo = new ShaderConfig();
            cfgInfo.ShaderType = shaderType;
            foreach (var p in shaderCfg.Params) {
                string id = p.GetId();
                var pfd = p as Dsl.FunctionData;
                if (null != pfd) {
                    if (id == "uniform") {
                        string file = pfd.GetParamId(0).Trim();
                        string type = pfd.GetParamId(1).Trim();
                        cfgInfo.UniformImports.Add(new UniformImportInfo { File = file, Type = type });
                    }
                    else if (id == "vsattr") {
                        string inAttr = pfd.GetParamId(0).Trim();
                        string outAttr = pfd.GetParamId(1).Trim();
                        string indexStr = pfd.GetParamId(2).Trim();
                        int.TryParse(indexStr, out var ix);
                        cfgInfo.InAttrImportFile = inAttr;
                        cfgInfo.OutAttrImportFile = outAttr;
                        cfgInfo.AttrIndex = ix;
                    }
                    else if (id == "psattr") {
                        string inAttr = pfd.GetParamId(0).Trim();
                        string indexStr = pfd.GetParamId(1).Trim();
                        int.TryParse(indexStr, out var ix);
                        cfgInfo.InAttrImportFile = inAttr;
                        cfgInfo.AttrIndex = ix;
                    }
                }
            }
            s_ShaderConfigs[shaderType] = cfgInfo;
        }

        internal class UniformImportInfo
        {
            internal string File = string.Empty;
            internal string Type = string.Empty;
        }
        internal class ShaderConfig
        {
            internal string ShaderType = string.Empty;
            internal string InAttrImportFile = string.Empty;
            internal string OutAttrImportFile = string.Empty;
            internal int AttrIndex = 0;
            internal List<UniformImportInfo> UniformImports = new List<UniformImportInfo>();
        }

        internal static Dictionary<string, ShaderConfig> s_ShaderConfigs = new Dictionary<string, ShaderConfig>();
    }
}
