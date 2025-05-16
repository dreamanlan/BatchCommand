﻿using System.Text;
using System.Diagnostics;
using static GlslRewriter.Program;
using Dsl;
using System;
using System.Xml.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel.Design;
using System.Security;
using static GlslRewriter.Config;
using System.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ScriptableFramework;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;

namespace GlslRewriter
{
    // Currently mainly used to convert decompiled GLSL (from spirv-cross or yuzu) into an approximation of SSA form and merge expressions.
    // Expression merging (using a computation graph for generation) is only used to generate comments, making the code easier to understand.
    // May not be suitable for general GLSL code.
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) {
                PrintHelp();
                return;
            }
            else {
                string srcFilePath = string.Empty;
                string outFilePath = string.Empty;
                string cfgFilePath = string.Empty;
                bool typeByExt = true;
                for (int i = 0; i < args.Length; ++i) {
                    if (0 == string.Compare(args[i], "-out", true)) {
                        if (i < args.Length - 1) {
                            string arg = args[i + 1];
                            if (!arg.StartsWith("-")) {
                                outFilePath = arg;
                                ++i;
                            }
                        }
                    }
                    else if (0 == string.Compare(args[i], "-src", true)) {
                        if (i < args.Length - 1) {
                            string arg = args[i + 1];
                            if (!arg.StartsWith("-")) {
                                srcFilePath = arg;
                                if (!File.Exists(srcFilePath)) {
                                    Console.WriteLine("file path not found ! {0}", srcFilePath);
                                }
                                ++i;
                            }
                        }
                    }
                    else if (0 == string.Compare(args[i], "-cfg", true)) {
                        if (i < args.Length - 1) {
                            string arg = args[i + 1];
                            if (!arg.StartsWith("-")) {
                                cfgFilePath = arg;
                                if (!File.Exists(cfgFilePath)) {
                                    Console.WriteLine("file path not found ! {0}", cfgFilePath);
                                }
                                ++i;
                            }
                        }
                    }
                    else if (0 == string.Compare(args[i], "-argcfg", true)) {
                        if (i < args.Length - 1) {
                            string arg = args[i + 1];
                            if (!arg.StartsWith("-")) {
                                if(int.TryParse(arg, out var ix)) {
                                    Config.ActiveArgCfgId = ix;
                                }
                                else {
                                    Console.WriteLine("argcfg must be integer ! {0}", arg);
                                }
                                ++i;
                            }
                        }
                    }
                    else if (0 == string.Compare(args[i], "-vs", true)) {
                        s_IsVsShader = true;
                        s_IsPsShader = false;
                        s_IsCsShader = false;

                        typeByExt = false;
                    }
                    else if (0 == string.Compare(args[i], "-ps", true)) {
                        s_IsPsShader = true;
                        s_IsVsShader = false;
                        s_IsCsShader = false;

                        typeByExt = false;
                    }
                    else if (0 == string.Compare(args[i], "-cs", true)) {
                        s_IsCsShader = true;
                        s_IsVsShader = false;
                        s_IsPsShader = false;

                        typeByExt = false;
                    }
                    else if (0 == string.Compare(args[i], "-i", true)) {
                        s_InteractiveComputing = true;
                    }
                    else if (0 == string.Compare(args[i], "-r", true)) {
                        s_DoReplacement = true;
                    }
                    else if (0 == string.Compare(args[i], "-ssa", true)) {
                        s_SSA = true;
                    }
                    else if (0 == string.Compare(args[i], "-nossa", true)) {
                        s_SSA = false;
                    }
                    else if (0 == string.Compare(args[i], "-h", true)) {
                        PrintHelp();
                        return;
                    }
                    else if (args[i][0] == '-') {
                        Console.WriteLine("unknown command option ! {0}", args[i]);
                    }
                    else {
                        srcFilePath = args[i];
                        if (!File.Exists(srcFilePath)) {
                            Console.WriteLine("file path not found ! {0}", srcFilePath);
                        }
                        break;
                    }
                }

                string oldCurDir = Environment.CurrentDirectory;
                string exeFullName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string? exeDir = Path.GetDirectoryName(exeFullName);
                Debug.Assert(null != exeDir);
                Environment.CurrentDirectory = exeDir;
                Console.WriteLine("curdir {0} change to exedir {1}", oldCurDir, exeDir);

                try {
                    string? workDir = Path.GetDirectoryName(srcFilePath);
                    Debug.Assert(null != workDir);
                    string tmpDir = Path.Combine(workDir, "tmp");
                    if (!Directory.Exists(tmpDir)) {
                        Directory.CreateDirectory(tmpDir);
                    }

                    string srcFileName = Path.GetFileName(srcFilePath);
                    string srcFileNameWithoutExt = Path.GetFileNameWithoutExtension(srcFileName);
                    string srcExt = Path.GetExtension(srcFilePath);

                    if (typeByExt) {
                        if (s_VertExts.Contains(srcExt)) {
                            s_IsVsShader = true;
                            s_IsPsShader = false;
                            s_IsCsShader = false;
                        }
                        else if (s_FragExts.Contains(srcExt)) {
                            s_IsVsShader = false;
                            s_IsPsShader = true;
                            s_IsCsShader = false;
                        }
                        else if (s_CompExts.Contains(srcExt)) {
                            s_IsVsShader = false;
                            s_IsPsShader = false;
                            s_IsCsShader = true;
                        }
                    }

                    //Initialize the batch script before loading the Config. The script interpreter may be used in the Config.
                    InitBatchScript();

                    if (string.IsNullOrEmpty(cfgFilePath)) {
                        cfgFilePath = Path.Combine(workDir, srcFileNameWithoutExt + "_args.dsl");
                    }
                    if (File.Exists(cfgFilePath)) {
                        Config.LoadConfig(cfgFilePath, tmpDir);
                    }

                    if (string.IsNullOrEmpty(outFilePath)) {
                        if (srcExt == ".txt")
                            outFilePath = Path.Combine(workDir, srcFileNameWithoutExt + "_" + srcExt.Substring(1) + ".glsl");
                        else
                            outFilePath = Path.Combine(workDir, srcFileNameWithoutExt + "_" + srcExt.Substring(1) + ".txt");
                    }

                    s_SrcFileForDSL = Path.Combine(tmpDir, srcFileName);

                    string outFileName = Path.GetFileName(outFilePath);
                    string studyFilePath = Path.Combine(tmpDir, outFileName);

                    if (s_InteractiveComputing) {
                        Config.ActiveConfig.SettingInfo.GenerateExpressionList = false;
                    }

                    Transform(srcFilePath, studyFilePath, outFilePath);

                    if (Config.ActiveConfig.SettingInfo.PrintGraph) {
                        s_GlobalComputeGraph.Print(null);
                        if (s_FuncInfos.TryGetValue("main", out var funcInfo)) {
                            var cg = funcInfo.FuncComputeGraph;
                            cg.Print(funcInfo);
                        }
                    }
                    Console.Out.Flush();
                }
                catch (Exception ex) {
                    Console.WriteLine("{0}", ex.Message);
                    Console.WriteLine("[Stack]:");
                    Console.WriteLine("{0}", ex.StackTrace);
                }
                finally {
                    Environment.CurrentDirectory = oldCurDir;
                }
            }
        }
        static void PrintHelp()
        {
            Console.WriteLine("[usage]GlslRewriter [-out outfile] [-cfg cfg_dsl_file] [-argcfg id_in_cfg] [-vs] [-ps] [-cs] [-i] [-r] [-src] glsl_file");
            Console.WriteLine(" [-out outfile] output file path and name, default is [glsl_file_name]_[glsl_file_ext].txt");
            Console.WriteLine(" [-cfg cfg_dsl_file] config file path and name, default is [glsl_file_name]_cfg.dsl");
            Console.WriteLine(" [-argcfg id_in_config] arg config id in config file, default is 0");
            Console.WriteLine(" [-vs] glsl_file is vertex shader [-ps] glsl_file is pixel shader (default)");
            Console.WriteLine(" [-cs] glsl_file is compute shader");
            Console.WriteLine(" [-i] interactive computing mode, don't write outfile");
            Console.WriteLine(" [-r] do string replacement");
            Console.WriteLine(" [-ssa] transform to SSA (default)");
            Console.WriteLine(" [-nossa] don't transform to SSA");
            Console.WriteLine(" [-src] glsl_file source glsl file, -src can be omitted when file is the last argument");
        }

        private static void Transform(string srcFile, string studyFile, string outFile)
        {
            if (!s_InteractiveComputing)
                File.Delete(studyFile);
            var glslFileLines = File.ReadAllLines(srcFile);
            var hlslMergeDatas = new Dictionary<string, Dictionary<string, RenderDocImporter.HlslExportInfo>>();
            foreach(var hlslMerge in Config.ActiveArgConfig.HlslMergeImports) {
                var datas = RenderDocImporter.ReadHlslMergeInfo(hlslMerge.File);
                hlslMergeDatas.Add(hlslMerge.File, datas);
            }
            var glslLines = CompletionAndSkipPP(glslFileLines, hlslMergeDatas, out var glslGlobalCode);
            string glslTxt = ConvertCondExpAndSkipComments(glslLines);
            if (!s_InteractiveComputing)
                File.WriteAllText(s_SrcFileForDSL, glslTxt);

            var file = new Dsl.DslFile();
            file.onGetToken = (ref Dsl.Common.DslAction dslAction, ref Dsl.Common.DslToken dslToken, ref string tok, ref short val, ref int line) => {
                if (tok == "return") {
                    var oldCurTok = dslToken.getCurToken();
                    var oldLastTok = dslToken.getLastToken();
                    if (dslToken.PeekNextValidChar(0) == ';')
                        return false;
                    dslToken.setCurToken("<-");
                    dslToken.setLastToken(oldCurTok);
                    dslToken.enqueueToken(dslToken.getCurToken(), dslToken.getOperatorTokenValue(), line);
                    dslToken.setCurToken(oldCurTok);
                    dslToken.setLastToken(oldLastTok);
                    return true;
                }
                return false;
            };
            file.onBeforeAddFunction = (ref Dsl.Common.DslAction dslAction, Dsl.StatementData statement) => {
                string sid = statement.GetId();
                var func = statement.Last.AsFunction;
                if (null != func) {
                    if (func.HaveStatement()) {
                        if (string.IsNullOrEmpty(sid) || sid == "for" || sid == "while" || sid == "else" || sid == "switch" || (func.IsHighOrder && func.LowerOrderFunction.IsParenthesesParamClass())) {
                            //End the current statement and start a new empty statement.
                            dslAction.endStatement();
                            dslAction.beginStatement();
                            return true;
                        }
                    }
                    else {
                        if (sid == "do") {
                            //End the current statement and start a new empty statement.
                            dslAction.endStatement();
                            dslAction.beginStatement();
                            return true;
                        }
                    }
                }
                return false;
            };
            file.onAddFunction = (ref Dsl.Common.DslAction dslAction, Dsl.StatementData statement, Dsl.FunctionData function) => {
                //Do not change the program structure here. At this point, the "function" is still an empty function, and the real function information has not been filled in.
                //The difference between this and "onBeforeAddFunction" is that the "function" is constructed and added to the function table of the current statement.
                return false;
            };
            file.onBeforeEndStatement = (ref Dsl.Common.DslAction dslAction, Dsl.StatementData statement) => {
                //The statement can be split here.
                return false;
            };
            file.onEndStatement = (ref Dsl.Common.DslAction dslAction, ref Dsl.StatementData statement) => {
                //The entire statement can be replaced here, but do not modify other parts of the program structure. The difference between this and "onBeforeEndStatement" is that
                //the statement has been popped from the stack at this point, and the simplified statement will be added to the upper-level syntax unit in the future.
                return false;
            };
            file.onBeforeBuildOperator = (ref Dsl.Common.DslAction dslAction, Dsl.Common.OperatorCategoryEnum category, string op, Dsl.StatementData statement) => {
                //The statement can be split here.
                string sid = statement.GetId();
                var func = statement.Last.AsFunction;
                if (null != func) {
                    if (sid == "if") {
                        statement.Functions.Remove(func);
                        dslAction.endStatement();
                        dslAction.beginStatement();
                        var stm = dslAction.getCurStatement();
                        stm.AddFunction(func);
                        return true;
                    }
                }
                return false;
            };
            file.onBuildOperator = (ref Dsl.Common.DslAction dslAction, Dsl.Common.OperatorCategoryEnum category, string op, ref Dsl.StatementData statement) => {
                //The statement can be replaced here, but do not modify other syntax structures.
                return false;
            };
            file.onSetFunctionId = (ref Dsl.Common.DslAction dslAction, string name, Dsl.StatementData statement, Dsl.FunctionData function) => {
                //The statement can be split here.
                string sid = statement.GetId();
                var func = statement.Last.AsFunction;
                if (null != func) {
                    if (sid == "if" && name != "else") {
                        statement.Functions.Remove(func);
                        dslAction.endStatement();
                        dslAction.beginStatement();
                        var stm = dslAction.getCurStatement();
                        stm.AddFunction(func);
                        return true;
                    }
                    else if (name == "struct" || name == "if" || name == "switch" || name == "for" || name == "do" || name == "while") {
                        statement.Functions.Remove(func);
                        dslAction.endStatement();
                        dslAction.beginStatement();
                        var stm = dslAction.getCurStatement();
                        stm.AddFunction(func);
                        return true;
                    }
                }
                return false;
            };
            file.onBeforeBuildHighOrder = (ref Dsl.Common.DslAction dslAction, Dsl.StatementData statement, Dsl.FunctionData function) => {
                //The statement can be split here.
                return false;
            };
            file.onBuildHighOrder = (ref Dsl.Common.DslAction dslAction, Dsl.StatementData statement, Dsl.FunctionData function) => {
                //The statement can be split here.
                return false;
            };

            int maxIterations = Config.ActiveConfig.SettingInfo.MaxIterations;
            for (int iterIx = 0; iterIx < maxIterations; ++iterIx) {
                Console.WriteLine("iteration:{0}...", iterIx);
                int varCountBefore = Config.ActiveConfig.SettingInfo.AutoSplitAddedVariables.Count;
                if (file.LoadFromString(glslTxt, msg => { Console.WriteLine(msg); })) {
                    //The syntax of GLSL is a valid DSL grammar, but with a different semantic structure. We attempt to process the GLSL syntax using the representation of a DSL.
                    Transform(file);
                    int varCountAfter = Config.ActiveConfig.SettingInfo.AutoSplitAddedVariables.Count;
                    Console.WriteLine("iteration:{0}, split variable count:{1} => {2}.", iterIx, varCountBefore, varCountAfter);
                    if (Config.ActiveConfig.SettingInfo.AutoSplitLevel < 0 || varCountBefore >= varCountAfter || iterIx == maxIterations - 1) {
                        if (!s_InteractiveComputing)
                            file.Save(studyFile);
                        break;
                    }
                    else if (varCountBefore < varCountAfter) {
                        file.DslInfos.Clear();
                        Reset();
                    }
                }
                else {
                    Environment.Exit(-1);
                }
            }

            if (s_InteractiveComputing) {
                InteractiveComputing();
            }
            else {
                var lineList = new List<string>();
                if (RenderDocImporter.s_VertexStructInits.Count > 0) {
                    lineList.AddRange(RenderDocImporter.s_VertexStructInits.Values);
                    lineList.Add(string.Empty);
                }
                if (RenderDocImporter.s_VertexAttrInits.Count > 0) {
                    foreach (var pair in RenderDocImporter.s_VertexAttrInits) {
                        lineList.Add("// " + pair.Key);
                        lineList.AddRange(pair.Value.Values);
                        lineList.Add(string.Empty);
                    }
                }
                if (RenderDocImporter.s_UniformInits.Count > 0) {
                    lineList.AddRange(RenderDocImporter.s_UniformInits);
                    lineList.Add(string.Empty);
                }
                if (RenderDocImporter.s_UniformRawInits.Count > 0) {
                    lineList.AddRange(RenderDocImporter.s_UniformRawInits);
                    lineList.Add(string.Empty);
                }
                if (RenderDocImporter.s_UniformUtofOrFtouVals.Count > 0) {
                    lineList.AddRange(RenderDocImporter.s_UniformUtofOrFtouVals);
                    lineList.Add(string.Empty);
                }
                var argConfig = Config.ActiveArgConfig;
                if (argConfig.SSBOImports.Count > 0) {
                    foreach (var ssboImp in argConfig.SSBOImports) {
                        var ssbos = RenderDocImporter.GenerateSSBO(ssboImp.Attr, ssboImp.AttrArrayLeft, ssboImp.Type, ssboImp.File);
                        lineList.AddRange(ssbos);
                    }
                    lineList.Add(string.Empty);
                }
                if (argConfig.VAOImports.Count > 0) {
                    foreach (var vaoImp in argConfig.VAOImports) {
                        var vaos = RenderDocImporter.GenerateVAO(vaoImp.Attr, vaoImp.AttrArrayLeft, vaoImp.Type, vaoImp.UsedIndexes, vaoImp.File, hlslMergeDatas);
                        lineList.AddRange(vaos);
                    }
                    lineList.Add(string.Empty);
                }
                var lines = File.ReadAllLines(studyFile);
                lineList.AddRange(lines);
                if (Config.ActiveConfig.SettingInfo.AutoSplitLevel >= 0 && Config.ActiveConfig.SettingInfo.AutoSplitAddedVariables.Count > 0) {
                    lineList.Add("/*split_variable_assignment{");
                    lineList.AddRange(Config.ActiveConfig.SettingInfo.AutoSplitAddedVariables);
                    lineList.Add("}*/");
                }
                File.WriteAllLines(studyFile, lineList.ToArray());

                if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                    if (Config.ActiveConfig.SettingInfo.RemoveDuplicateExpression && s_DuplicateVars.Count > 0) {
                        bool removeComments = false;
                        for (int ix = s_ExpressionList.Count - 1; ix >= 0; --ix) {
                            var tuple = s_ExpressionList[ix];
                            var line = tuple.Item1;
                            var vname = tuple.Item2;
                            var blockId = tuple.Item3;
                            if (vname == c_CommentTag && removeComments) {
                                s_ExpressionList.RemoveAt(ix);
                            }
                            else if (s_DuplicateVars.TryGetValue(blockId, out var dupvars)) {
                                if (!string.IsNullOrEmpty(vname) && dupvars.ContainsKey(vname)) {
                                    s_ExpressionList.RemoveAt(ix);
                                    removeComments = true;
                                }
                                else {
                                    string s = tuple.Item1;
                                    foreach (var pair in dupvars) {
                                        var reptuple = pair.Value;
                                        s = reptuple.Item2.Replace(s, reptuple.Item1);
                                    }
                                    s_ExpressionList[ix] = Tuple.Create(s, vname, blockId);
                                    removeComments = false;
                                }
                            }
                        }
                        foreach (var pair0 in s_DuplicateVars) {
                            foreach (var pair1 in pair0.Value) {
                                var vname = pair1.Key;
                                Config.ActiveConfig.SettingInfo.UsedVariables.Remove(vname);
                            }
                        }
                    }
                    var outBuilder = s_ExpressionBuilder;
                    outBuilder.Length = 0;
                    bool isForHlsl = false;
                    if (s_DoReplacement && Config.ActiveConfig.SettingInfo.ForHlslShader) {
                        isForHlsl = true;
                    }
                    if (isForHlsl) {
                        if (Config.ActiveConfig.CodeBlocks.TryGetValue("hlsl_global", out var gcode)) {
                            outBuilder.AppendLine(gcode);
                        }
                        if (Config.ActiveConfig.ShaderType == "vs") {
                            outBuilder.AppendLine("void vert_from_glsl(appdata v, vaodata i, inout v2f o)");
                            outBuilder.AppendLine("{");
                        }
                        else {
                            outBuilder.AppendLine("void frag_from_glsl(v2f i, inout float4 col)");
                            outBuilder.AppendLine("{");
                        }
                        if (Config.ActiveConfig.CodeBlocks.TryGetValue("hlsl_prologue", out var pcode)) {
                            outBuilder.AppendLine(pcode);
                        }
                    }
                    else {
                        if (Config.ActiveConfig.CodeBlocks.TryGetValue("glsl_global", out var gcode)) {
                            outBuilder.AppendLine(gcode);
                        }
                        else {
                            outBuilder.AppendLine(glslGlobalCode);
                        }
                        outBuilder.AppendLine("void main()");
                        outBuilder.AppendLine("{");
                        if (Config.ActiveConfig.CodeBlocks.TryGetValue("glsl_prologue", out var pcode)) {
                            outBuilder.AppendLine(pcode);
                        }
                    }
                    if (RenderDocImporter.s_UniformUtofOrFtouVals.Count > 0) {
                        foreach (var line in RenderDocImporter.s_UniformUtofOrFtouVals) {
                            outBuilder.Append("\t");
                            outBuilder.AppendLine(line);
                        }
                        outBuilder.AppendLine();
                    }
                    foreach (var pair in Config.ActiveConfig.SettingInfo.UsedVariables) {
                        if (string.IsNullOrEmpty(pair.Value)) {
                            Console.WriteLine("variable '{0}' isn't used, please remove it from Config !", pair.Key);
                        }
                        else {
                            outBuilder.Append("\t");
                            outBuilder.Append(pair.Value);
                            outBuilder.Append(" ");
                            outBuilder.Append(pair.Key);
                            outBuilder.AppendLine(";");
                        }
                    }
                    foreach (var tuple in s_ExpressionList) {
                        var line = tuple.Item1;
                        outBuilder.AppendLine(line);
                    }
                    if (isForHlsl) {
                        if (Config.ActiveConfig.CodeBlocks.TryGetValue("hlsl_epilogue", out var ecode)) {
                            outBuilder.AppendLine(ecode);
                        }
                    }
                    else {
                        if (Config.ActiveConfig.CodeBlocks.TryGetValue("glsl_epilogue", out var ecode)) {
                            outBuilder.AppendLine(ecode);
                        }
                    }
                    outBuilder.AppendLine("}");
                    string txt = outBuilder.ToString();
                    txt = Config.TryReplaceString(txt);
                    File.WriteAllText(outFile, txt);
                }
            }
        }
        private static List<string> CompletionAndSkipPP(IList<string> glslLines, Dictionary<string, Dictionary<string, RenderDocImporter.HlslExportInfo>> hlslMergeDatas, out string globalCode)
        {
            var lines = new List<string>();
            bool attrImported = false;
            bool needFindLBrace = false;
            if (s_IsPsShader) {
                lines.Add("vec4 gl_FragCoord;");
            }
            bool findMain = false;
            var globalBuilder = s_ExpressionBuilder;
            globalBuilder.Length = 0;
            foreach (var line in glslLines) {
                if (line.TrimStart().StartsWith("#")) {
                    if (!findMain)
                        globalBuilder.AppendLine(line);
                    continue;
                }
                lines.Add(line);
                if (needFindLBrace) {
                    int ix = line.IndexOf('{');
                    if (ix >= 0) {
                        needFindLBrace = false;
                        attrImported = true;
                        lines.AddRange(GetImportAttrs(hlslMergeDatas));
                    }
                }
                else if (!attrImported) {
                    int ix1 = line.IndexOf("void");
                    int ix2 = line.IndexOf("main", ix1 >= 0 ? ix1 : 0);
                    int ix3 = line.IndexOf("(", ix2 >= 0 ? ix2 : 0);
                    int ix4 = line.IndexOf(")", ix3 >= 0 ? ix3 : 0);
                    int ix5 = line.IndexOf('{', ix4 >= 0 ? ix4 : 0);
                    if (ix1 >= 0 && ix2 >= 0 && ix3 >= 0) {
                        bool match1 = false;
                        for (int i = ix1 + "void".Length; i < ix2 && i < line.Length; ++i) {
                            char ch = line[i];
                            if (ch == ' ' || ch == '\t') {
                                match1 = true;
                            }
                            else {
                                match1 = false;
                                break;
                            }
                        }
                        bool match2 = true;
                        for (int i = ix2 + "main".Length; i < ix3 && i < line.Length; ++i) {
                            char ch = line[i];
                            if (ch == ' ' || ch == '\t') {
                            }
                            else {
                                match2 = false;
                                break;
                            }
                        }
                        if (match1 && match2) {
                            findMain = true;
                            if (ix5 > ix4 && ix4 > ix3) {
                                attrImported = true;
                                lines.AddRange(GetImportAttrs(hlslMergeDatas));
                            }
                            else if (ix5 < 0) {
                                needFindLBrace = true;
                            }
                        }
                    }
                }
                if(!findMain)
                    globalBuilder.AppendLine(line);
            }
            globalCode = globalBuilder.ToString();
            return lines;
        }
        private static List<string> GetImportAttrs(Dictionary<string, Dictionary<string, RenderDocImporter.HlslExportInfo>> hlslMergeDatas)
        {
            List<string> stms = new List<string>();
            var cfg = Config.ActiveArgConfig;
            var attrCfg = cfg.InOutAttrInfo;
            if (!string.IsNullOrEmpty(attrCfg.InAttrImportFile)) {
                if (File.Exists(attrCfg.InAttrImportFile)) {
                    var lines = RenderDocImporter.GenenerateVsInOutAttr("float", attrCfg.AttrIndex, attrCfg.InAttrMap, attrCfg.InAttrImportFile, hlslMergeDatas, out var outPos);
                    stms.AddRange(lines);
                    //Insert parameter initialization
                    if (s_IsPsShader) {
                        int w = Config.CommonCfg.ViewportWidth;
                        int h = Config.CommonCfg.ViewportHeight;
                        float x = outPos.X * 0.5f + 0.5f;
                        float y = outPos.Y * 0.5f + 0.5f;
                        float z = outPos.Z * 0.5f + 0.5f;
                        stms.Insert(0, string.Format("gl_FragCoord = vec4({0}, {1}, {2}, {3});", x * (w - 1), y * (h - 1), z, outPos.W));
                    }
                }
                else {
                    Console.WriteLine("Can't find file {0}", attrCfg.InAttrImportFile);
                }
            }
            if (!string.IsNullOrEmpty(attrCfg.OutAttrImportFile)) {
                if (File.Exists(attrCfg.OutAttrImportFile)) {
                    var lines = RenderDocImporter.GenenerateVsInOutAttr("float", attrCfg.AttrIndex, attrCfg.OutAttrMap, attrCfg.OutAttrImportFile, hlslMergeDatas, out var outPos);
                    stms.AddRange(lines);
                }
                else {
                    Console.WriteLine("Can't find file {0}", attrCfg.OutAttrImportFile);
                }
            }
            foreach (var uniform in cfg.UniformImports) {
                if (File.Exists(uniform.File)) {
                    var lines = RenderDocImporter.GenerateUniform(uniform.Type, uniform.UsedIndexes, uniform.File, hlslMergeDatas);
                    stms.AddRange(lines);
                }
                else {
                    Console.WriteLine("Can't find file {0}", uniform.File);
                }
            }
            return stms;
        }
        private static string ConvertCondExpAndSkipComments(IList<string> glslLines)
        {
            //In C language, the ternary operator (?:) and the assignment operator (=) have the same precedence, which is different from MetaDSL.
            //It is possible that an assignment expression may appear within a conditional expression. Therefore,
            //we need to enclose these expressions in parentheses before proceeding with the DSL parsing.
            var sb = new StringBuilder();
            bool inCommentBlock = false;
            for (int ix = 0; ix < glslLines.Count; ++ix) {
                var line = glslLines[ix];
                int k = 0;
                if (inCommentBlock) {
                    for (int i = 0; i < line.Length; ++i) {
                        char ch = line[i];
                        if (ch == '*' && i < line.Length - 1 && line[i + 1] == '/') {
                            inCommentBlock = false;
                            k = i + 2;
                            break;
                        }
                    }
                    if (inCommentBlock)
                        continue;
                }
                var rline = line.Substring(k);
                if (rline.StartsWith("#line")) {
                    //ignore
                }
                else {
                    for (int i = k; i < line.Length; ++i) {
                        char c = line[i];
                        if (c == '\\') {
                            ++i;
                            sb.Append(c);
                            sb.Append(line[i]);
                        }
                        else if (s_CondExpStack.Count > 0 && (c == '(' || c == '[' || c == '{')) {
                            s_CondExpStack.Peek().IncParenthesesCount();
                            sb.Append(c);
                        }
                        else if (s_CondExpStack.Count > 0 && (c == ')' || c == ']' || c == '}')) {
                            if (s_CondExpStack.Peek().MaybeCompletePart(CondExpEnum.Colon)) {
                                s_CondExpStack.Pop();
                                sb.Append(')');
                            }
                            if (s_CondExpStack.Count > 0) {
                                s_CondExpStack.Peek().DecParenthesesCount();
                            }
                            sb.Append(c);
                        }
                        else if (c == '?') {
                            s_CondExpStack.Push(new CondExpInfo(CondExpEnum.Question));
                            sb.Append(c);
                            sb.Append(' ');
                            sb.Append('(');
                        }
                        else if (c == ':') {
                            while (s_CondExpStack.Count > 0 && s_CondExpStack.Peek().MaybeCompletePart(CondExpEnum.Colon)) {
                                s_CondExpStack.Pop();
                                sb.Append(')');
                            }
                            if (s_CondExpStack.Count > 0 && s_CondExpStack.Peek().MaybeCompletePart(CondExpEnum.Question)) {
                                s_CondExpStack.Pop();
                                s_CondExpStack.Push(new CondExpInfo(CondExpEnum.Colon));
                                sb.Append(')');
                                sb.Append(' ');
                                sb.Append(c);
                                sb.Append(' ');
                                sb.Append('(');
                            }
                            else {
                                sb.Append(c);
                            }
                        }
                        else if (c == ',' || c == ';') {
                            while (s_CondExpStack.Count > 0 && s_CondExpStack.Peek().MaybeCompletePart(CondExpEnum.Colon)) {
                                s_CondExpStack.Pop();
                                sb.Append(')');
                            }
                            sb.Append(c);
                        }
                        else if (c == '"') {
                            sb.Append(c);
                            for (int j = i + 1; j < line.Length; ++j) {
                                char ch = line[j];
                                sb.Append(ch);
                                if (ch == '\\') {
                                    ++j;
                                    sb.Append(line[j]);
                                }
                                else if (ch == '"') {
                                    i = j;
                                    break;
                                }
                            }
                        }
                        else {
                            char nc = i < line.Length - 1 ? line[i + 1] : '\0';
                            if (c == '/' && nc == '*') {
                                inCommentBlock = true;
                                for (int j = i + 2; j < line.Length; ++j) {
                                    char ch = line[j];
                                    if (ch == '*' && j < line.Length - 1 && line[j + 1] == '/') {
                                        inCommentBlock = false;
                                        i = j + 1;
                                        break;
                                    }
                                }
                                if (inCommentBlock)
                                    break;
                            }
                            else if (c == '/' && nc == '/') {
                                break;
                            }
                            else {
                                sb.Append(c);
                                char lc = i > 0 ? line[i - 1] : '\0';
                                if (c == '\'' && (!char.IsDigit(lc) || !char.IsDigit(nc))) {
                                    for (int j = i + 1; j < line.Length; ++j) {
                                        char ch = line[j];
                                        if (ch == '\\') {
                                            ++j;
                                            sb.Append(ch);
                                            sb.Append(line[j]);
                                        }
                                        else if (ch == '\'') {
                                            sb.Append(ch);
                                            i = j;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
        private static void Transform(Dsl.DslFile file)
        {
            foreach (var dsl in file.DslInfos) {
                TransformToplevelSyntax(dsl);
            }
        }
        private static void InitBatchScript()
        {
            BatchCommand.BatchScript.Init();
            BatchCommand.BatchScript.Register("shader", "shader(code1,code2,...) api", new ExpressionFactoryHelper<ShaderExp>());
            BatchCommand.BatchScript.Register("add_svar", "add_svar(name,val[,dim_for_array]) api", new ExpressionFactoryHelper<AddShaderVarExp>());
            BatchCommand.BatchScript.Register("set_svar", "set_svar(var_code,val) api", new ExpressionFactoryHelper<SetShaderVarExp>());
            BatchCommand.BatchScript.Register("add_unsvar", "add_unsvar(var_code_1,var_code_2,...) api", new ExpressionFactoryHelper<AddUnassignableShaderVarExp>());
            BatchCommand.BatchScript.Register("import_inout", "import_inout(index) api", new ExpressionFactoryHelper<ImportInOutExp>());
            BatchCommand.BatchScript.Register("recalc", "recalc([bool_full]) api", new ExpressionFactoryHelper<ReCalcExp>());
            BatchCommand.BatchScript.Register("rand_color", "rand_color() api", new ExpressionFactoryHelper<RandColorExp>());
            BatchCommand.BatchScript.Register("rand_uv", "rand_uv(dim) api", new ExpressionFactoryHelper<RandUVExp>());
            BatchCommand.BatchScript.Register("rand_size", "rand_size(max_v) or rand_size(max_x,max_y) or rand_size(max_x,max_y,max_z) or rand_size(max_x,max_y,max_z,max_w) api", new ExpressionFactoryHelper<RandSizeExp>());
            BatchCommand.BatchScript.SetOnTryGetVariable(VariableTable.TryGetVariable);
            BatchCommand.BatchScript.SetOnTrySetVariable(VariableTable.TrySetVariable);
        }
        private static void InteractiveComputing()
        {
            Console.WriteLine("Enter exit or quit to exit...");
            for (; ; ) {
                Console.Write(">");
                var line = Console.ReadLine();
                if (null != line) {
                    if (line == "exit" || line == "quit")
                        break;
                    if (line == "help" || line.StartsWith("help ")) {
                        string filter = line.Substring(4).Trim();
                        foreach (var pair in BatchCommand.BatchScript.ApiDocs) {
                            if (pair.Key.Contains(filter) || pair.Value.Contains(filter)) {
                                Console.WriteLine("[{0}]:{1}", pair.Key, pair.Value);
                            }
                        }
                    }
                    else {
                        var r = BatchCommand.BatchScript.EvalAndRun(line);
                        Console.Write("result:");
                        if (r.IsNumber) {
                            if (r.Type == BoxedValue.c_DoubleType) {
                                double v = r.GetDouble();
                                Console.WriteLine("{0}", v);
                            }
                            else {
                                double v = r.GetFloat();
                                Console.WriteLine("{0}", v);
                            }
                        }
                        else {
                            Console.WriteLine(r.ToString());
                        }
                    }
                }
            }
        }
        internal static void ResetValueDependsVar(string vname)
        {
            if (s_FuncInfos.TryGetValue("main", out var fi)) {
                var cg = fi.FuncComputeGraph;
                if (null != cg) {
                    cg.ResetValueDependsVar(fi, vname);
                }
            }
        }
        internal static bool ReCalc(bool full)
        {
            bool ret = false;
            if(s_FuncInfos.TryGetValue("main", out var fi)) {
                if (full) {
                    var cg = fi.FuncComputeGraph;
                    if (null != cg) {
                        cg.ResetValue(fi);
                    }
                }
                var cgf = fi.ComputeGraphFunc;
                if (null != cgf) {
                    cgf.DoCalc();
                    ret = true;
                }
            }
            return ret;
        }

        //Top-level syntax processing
        private static void TransformToplevelSyntax(Dsl.ISyntaxComponent syntax)
        {
            var valData = syntax as Dsl.ValueData;
            var funcData = syntax as Dsl.FunctionData;
            var stmData = syntax as Dsl.StatementData;
            if (null != valData) {
                TransformToplevelValue(valData);
            }
            else if (null != funcData) {
                TransformToplevelFunction(funcData);
            }
            else if (null != stmData) {
                TransformToplevelStatement(stmData, 0, new List<Dsl.ValueOrFunctionData>());
            }
        }
        private static void TransformToplevelStatement(Dsl.StatementData stmData, int startFuncIx, List<Dsl.ValueOrFunctionData> modifiers)
        {
            var semanticInfo = new SemanticInfo();
            string id = stmData.GetFunctionId(startFuncIx);
            if (id == "out") {
                var funcNum = stmData.GetFunctionNum();
                if (startFuncIx + 1 < funcNum) {
                    var func = stmData.GetFunction(startFuncIx + 1).AsFunction;
                    if (null != func) {
                        TransformOutBlock(func);
                    }
                    else {
                        TransformVar(stmData, startFuncIx, true, ref semanticInfo);
                    }
                }
            }
            else if (id == "in") {
                //In a compute shader, the syntax for describing the number of threads ends with the `in` keyword, rather than treating the last part as a variable.
                if (startFuncIx < stmData.GetFunctionNum() - 1) {
                    TransformVar(stmData, startFuncIx, true, ref semanticInfo);
                }
            }
            else if (id == "flat") {
                TransformVar(stmData, startFuncIx, true, ref semanticInfo);
            }
            else if (id == "layout") {
                modifiers.Add(stmData.GetFunction(startFuncIx));
                TransformToplevelStatement(stmData, startFuncIx + 1, modifiers);
            }
            else if (id == "struct") {
                var funcNum = stmData.GetFunctionNum();
                if (startFuncIx + 1 < funcNum) {
                    var func = stmData.GetFunction(startFuncIx + 1).AsFunction;
                    if (null != func) {
                        var namePart = startFuncIx + 2 < funcNum ? stmData.GetFunction(startFuncIx + 2) : (Dsl.ValueOrFunctionData?)null;
                        TransformStruct(func, namePart);
                    }
                }
            }
            else if (id == "buffer") {
                var funcNum = stmData.GetFunctionNum();
                if (startFuncIx + 1 < funcNum) {
                    var func = stmData.GetFunction(startFuncIx + 1).AsFunction;
                    if (null != func) {
                        var namePart = startFuncIx + 2 < funcNum ? stmData.GetFunction(startFuncIx + 2) : (Dsl.ValueOrFunctionData?)null;
                        TransformBuffer(func, string.Empty, namePart);
                    }
                }
            }
            else if (id == "uniform") {
                var funcNum = stmData.GetFunctionNum();
                if (startFuncIx + 1 < funcNum) {
                    var func = stmData.GetFunction(startFuncIx + 1).AsFunction;
                    if (null != func) {
                        var namePart = startFuncIx + 2 < funcNum ? stmData.GetFunction(startFuncIx + 2) : (Dsl.ValueOrFunctionData?)null;
                        TransformUniformBlock(func, namePart);
                    }
                    else {
                        TransformVar(stmData, startFuncIx, true, ref semanticInfo);
                    }
                }
            }
            else {
                //In the DSL syntax, semicolons are used to separate statements. However, in GLSL, function definitions do not have a terminating semicolon. During DSL parsing,
                //function definitions and subsequent function definitions or struct/buffer/variable definitions may be combined into a single large statement.
                //Here, they need to be separated and analyzed individually (to ensure the correctness of the output, the overall representation cannot be modified).
                int index = 0;
                bool handled = false;
                while (index < stmData.GetFunctionNum()) {
                    int startIndex = index;
                    string fid = stmData.GetFunctionId(startIndex);
                    if (fid == "layout" || fid == "struct" || fid == "uniform" || fid == "out" || fid == "in") {
                        TransformToplevelStatement(stmData, startIndex, new List<ValueOrFunctionData>());
                        handled = true;
                        break;
                    }
                    else {
                        bool existsFunc = ParseStatement(stmData, ref index, out var f, out var fmodifiers);
                        if (existsFunc) {
                            Debug.Assert(null != f);
                            TransformFuncDef(f, fmodifiers);
                            handled = true;
                        }
                        else {
                            handled = false;
                            break;
                        }
                    }
                }
                if (!handled) {
                    TransformVar(stmData, index, true, ref semanticInfo);
                }
            }
        }
        private static void TransformToplevelFunction(Dsl.FunctionData funcData)
        {
            var semanticInfo = new SemanticInfo();
            string funcId = funcData.GetId();
            if (funcId == "=") {
                var p = funcData.GetParam(0);
                var vd = p as Dsl.ValueData;
                var fd = p as Dsl.FunctionData;
                var sd = p as Dsl.StatementData;
                if (null != vd) {
                    TransformVar(vd, ref semanticInfo);
                }
                else if (null != fd) {
                    TransformGeneralCall(fd, ref semanticInfo);
                }
                else if (null != sd) {
                    //In the DSL syntax, semicolons are used to separate statements.However, in GLSL, function definitions do not have a terminating semicolon.
                    //During DSL parsing, function definitions are parsed until the syntax part to the left of the assignment statement.Here, the function definition
                    //and the left part of the assignment statement need to be separated in order to correctly analyze the function prototype and identify
                    //variable definitions in the assignment statement(without changing the overall representation, otherwise the syntax may be incorrect during output).
                    Dsl.StatementData? left, right;
                    if (SplitToplevelStatementsInExpression(sd, out left, out right)) {
                        Debug.Assert(null != left && null != right);
                        string id = left.GetId();
                        int index = 0;
                        while (index < left.GetFunctionNum()) {
                            bool existsFunc = ParseStatement(left, ref index, out var f, out var modifiers);
                            if (existsFunc) {
                                Debug.Assert(null != f);
                                TransformFuncDef(f, modifiers);
                            }
                            else {
                                break;
                            }
                        }
                        if (index < left.GetFunctionNum()) {
                            Debug.Assert(false);
                        }
                        //When splitting, left is the end of a compound statement. At this point, left should have been processed completely.
                        TransformVar(right, 0, true, sd, ref semanticInfo);
                    }
                    else {
                        TransformVar(sd, 0, true, ref semanticInfo);
                    }
                }
                var v = funcData.GetParam(1);
                TransformExpression(v, ref semanticInfo);
            }
            else {
                TransformGeneralFunction(funcData, ref semanticInfo);
            }
        }
        private static void TransformToplevelValue(Dsl.ValueData valData)
        {
            var semanticInfo = new SemanticInfo();
            TransformVar(valData, ref semanticInfo);
        }

        private static void TransformStruct(Dsl.FunctionData structFunc, Dsl.ValueOrFunctionData? varNamePart)
        {
            string name = structFunc.GetId();
            var struInfo = new StructInfo();
            struInfo.Name = name;
            VarInfo? last = null;
            foreach (var p in structFunc.Params) {
                var stm = p as Dsl.StatementData;
                if (null != stm) {
                    var varInfo = ParseVarInfo(stm);
                    struInfo.Fields.Add(varInfo);
                    last = varInfo;
                }
                else if (null != last) {
                    var f = p as Dsl.FunctionData;
                    if (null != f) {
                        var varInfo = new VarInfo();
                        varInfo.CopyFrom(last);
                        varInfo.Name = f.GetId();
                        string arrTag = BuildTypeWithTypeArgs(f).Substring(varInfo.Name.Length);
                        varInfo.Type = varInfo.Type + arrTag;
                        struInfo.Fields.Add(varInfo);
                    }
                    else {
                        var varInfo = new VarInfo();
                        varInfo.CopyFrom(last);
                        varInfo.Name = p.GetId();
                        struInfo.Fields.Add(varInfo);
                    }
                }
            }
            if (null != varNamePart) {
                var vinfo = new VarInfo();
                var fd = varNamePart.AsFunction;
                if (null != fd) {
                    List<string> arrTags = new List<string>();
                    vinfo.Name = BuildTypeWithArrTags(fd, arrTags);
                    var sb = new StringBuilder();
                    sb.Append(struInfo.Name);
                    for (int ix = arrTags.Count - 1; ix >= 0; --ix) {
                        sb.Append(arrTags[ix]);
                    }
                    vinfo.Type = sb.ToString();
                }
                else {
                    vinfo.Name = varNamePart.GetId();
                    vinfo.Type = struInfo.Name;
                }
                AddVar(vinfo);
            }
            else {
                var vinfo = new VarInfo();
                vinfo.Type = struInfo.Name;
                SetLastVarType(vinfo);
            }
            if (s_StructInfos.ContainsKey(struInfo.Name)) {
                Console.WriteLine("duplicated glsl struct define '{0}', line: {1}", struInfo.Name, structFunc.GetLine());
            }
            else {
                s_StructInfos.Add(struInfo.Name, struInfo);
            }
        }
        private static void TransformBuffer(Dsl.FunctionData info, string layout, Dsl.ValueOrFunctionData? varNamePart)
        {
            var cbufInfo = new BufferInfo();
            cbufInfo.Name = info.GetId();
            cbufInfo.Layout = layout;
            if (null != varNamePart) {
                Debug.Assert(varNamePart.IsValue);
                cbufInfo.instName = varNamePart.GetId();
            }
            foreach (var p in info.Params) {
                var stm = p as Dsl.StatementData;
                if (null != stm) {
                    var varInfo = ParseVarInfo(stm);
                    cbufInfo.Variables.Add(varInfo);

                    AddVar(varInfo);
                }
            }
            if (s_BufferInfos.ContainsKey(cbufInfo.Name)) {
                Console.WriteLine("duplicated glsl buffer define '{0}', line: {1}", cbufInfo.Name, info.GetLine());
            }
            else {
                s_BufferInfos.Add(cbufInfo.Name, cbufInfo);
            }
        }
        private static void TransformFuncDef(Dsl.FunctionData func, List<string> modifiers)
        {
            var semanticInfo = new SemanticInfo();
            Debug.Assert(func.IsHighOrder);
            var paramsPart = func.LowerOrderFunction;
            var funcInfo = new FuncInfo();
            string funcName = paramsPart.GetId();
            funcInfo.Name = funcName;
            PushFuncInfo(funcInfo);
            PushBlock(false, false);

            var retInfo = new VarInfo();
            if (modifiers.Count > 0) {
                retInfo.Name = funcName;
                retInfo.Type = modifiers[modifiers.Count - 1];
                modifiers.RemoveAt(modifiers.Count - 1);
                retInfo.Modifiers = modifiers;
            }
            SetRetInfo(retInfo);

            List<string> argTypes = new List<string>();
            for (int ix = 0; ix < paramsPart.GetParamNum(); ++ix) {
                var pcomp = paramsPart.GetParam(ix);
                var pFunc = pcomp as Dsl.FunctionData;
                var pStm = pcomp as Dsl.StatementData;
                Dsl.ISyntaxComponent? pDef = null;
                if (null != pFunc && pFunc.GetId() == "=") {
                    pcomp = pFunc.GetParam(0);
                    pStm = pcomp as Dsl.StatementData;
                    pDef = pFunc.GetParam(1);
                    TransformExpression(pDef, ref semanticInfo);
                }
                if (null != pStm) {
                    var paramInfo = ParseVarInfo(pStm);
                    paramInfo.DefaultValue = pDef;
                    AddParamInfo(paramInfo);
                    argTypes.Add(paramInfo.Type);
                }
            }
            bool first = false;
            string signature = GetFullTypeFuncSig(funcName, argTypes);
            funcInfo.Signature = signature;
            if (!s_FuncInfos.ContainsKey(signature)) {
                s_FuncInfos.Add(signature, funcInfo);
                first = true;
            }
            if (first) {
                AddFuncToComputeGraph(funcInfo);

                if (!s_FuncOverloads.TryGetValue(funcName, out var overloads)) {
                    overloads = new HashSet<string>();
                    s_FuncOverloads.Add(funcName, overloads);
                }
                if (!overloads.Contains(signature)) {
                    overloads.Add(signature);
                }
            }
            TransformFunctionStatements(func);
            PopBlock();
            PopFuncInfo();
        }
        private static void TransformUniformBlock(Dsl.FunctionData func, Dsl.ValueOrFunctionData? varNamePart)
        {
            if (null != varNamePart) {
                var val = varNamePart.AsValue;
                var f = varNamePart.AsFunction;
                if (null != val) {
                    var semanticInfo = new SemanticInfo();
                    TransformVar(val, ref semanticInfo);
                }
                else if (null != f) {
                    var semanticInfo = new SemanticInfo();
                    TransformVar(f.Name, ref semanticInfo);
                }
            }
            else {
                var stmData = func.GetParam(0) as Dsl.StatementData;
                if (null != stmData) {
                    var semanticInfo = new SemanticInfo();
                    TransformVar(stmData, 0, true, stmData, ref semanticInfo);
                }
            }
        }
        private static void TransformOutBlock(Dsl.FunctionData func)
        {
            if (func.GetId() == "gl_PerVertex") {
                var stmData = func.GetParam(0) as Dsl.StatementData;
                if (null != stmData) {
                    var semanticInfo = new SemanticInfo();
                    TransformVar(stmData, 0, true, stmData, ref semanticInfo);
                }
            }
        }

        //Non-top-level statement syntax handling.
        private static bool TransformStatement(Dsl.ISyntaxComponent syntax, out List<Dsl.ISyntaxComponent>? insertBeforeOuter, out List<Dsl.ISyntaxComponent>? insertAfterOuter)
        {
            insertBeforeOuter = null;
            insertAfterOuter = null;
            var semanticInfo = new SemanticInfo();

            var valData = syntax as Dsl.ValueData;
            var funcData = syntax as Dsl.FunctionData;
            var stmData = syntax as Dsl.StatementData;
            if (null != valData) {
                string valId = valData.GetId();
                if (valId == "return") {
                    TransformReturn(valData, out insertBeforeOuter, out insertAfterOuter);
                }
                else if (valId == "discard") {
                    TransformDiscard(valData, out insertBeforeOuter, out insertAfterOuter);
                }
                else if (valId == "break") {
                    TransformBreak(valData, out insertBeforeOuter, out insertAfterOuter);
                }
                else if (valId == "continue") {
                    TransformContinue(valData, out insertBeforeOuter, out insertAfterOuter);
                }
                else {
                    TransformVar(valData, ref semanticInfo);
                }
            }
            else if (null != funcData) {
                string funcId = funcData.GetId();
                if (funcId == "=") {
                    var p = funcData.GetParam(0);
                    var vd = p as Dsl.ValueData;
                    var fd = p as Dsl.FunctionData;
                    var sd = p as Dsl.StatementData;
                    var tempVarSi = new SemanticInfo();
                    if (null != vd) {
                        TransformAssignLeft(vd, ref tempVarSi);
                    }
                    else if (null != fd) {
                        TransformGeneralCall(fd, ref tempVarSi);
                    }
                    else if (null != sd) {
                        TransformAssignLeft(sd, 0, false, ref tempVarSi);
                    }
                    var tempValSi = new SemanticInfo();
                    var v = funcData.GetParam(1);
                    TransformExpression(v, ref tempValSi);
                    HandleVarAliasInfoUpdate();

                    var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), tempVarSi.ResultType, "=");

                    var vgn = tempVarSi.GraphNode;
                    var agn = tempValSi.GraphNode;
                    Debug.Assert(null != vgn);
                    Debug.Assert(null != agn);

                    cgcn.AddPrev(agn);
                    agn.AddNext(cgcn);

                    cgcn.AddNext(vgn);
                    vgn.AddPrev(cgcn);

                    AddComputeGraphRootNode(cgcn);

                    //Calculations always need to be executed, but the output may be skipped based on the configuration.
                    cgcn.DoCalc();
                    if (Config.CalcSettingForVariable(p, out var isVariableSetting, out var markValue, out var markExp, out var maxLvlForExp, out var maxLenForExp, out var multiline, out var expandedOnlyOnce)) {
                        GenerateValueAndExpression(funcData, p, cgcn, isVariableSetting, markValue, markExp, true, maxLvlForExp, maxLenForExp, multiline, expandedOnlyOnce, out var val);
                    }
                }
                else if (funcId.Length >= 2 && funcId[funcId.Length - 1] == '=' && funcId != "==" && funcId != "!=" && funcId != "<=" && funcId != ">=") {
                    //convert to var = var op val
                    var p1 = funcData.GetParam(0);
                    var p22 = funcData.GetParam(1);
                    funcData.Name.SetId("=");

                    var p2 = new Dsl.FunctionData();
                    p2.Name = new Dsl.ValueData(funcId.Substring(0, funcId.Length - 1), AbstractSyntaxComponent.ID_TOKEN);
                    p2.SetOperatorParamClass();
                    var p21 = Dsl.Utility.CloneDsl(p1);
                    p2.AddParam(p21);
                    p2.AddParam(p22);
                    funcData.Params[1] = p2;

                    return TransformStatement(funcData, out insertBeforeOuter, out insertAfterOuter);
                }
                else if (funcId == "++" || funcId == "--") {
                    //convert to var = var + 1
                    var p1 = funcData.GetParam(0);
                    funcData.Name.SetId("=");

                    var p2 = new Dsl.FunctionData();
                    p2.Name = new Dsl.ValueData(funcId.Substring(0, 1), AbstractSyntaxComponent.ID_TOKEN);
                    p2.SetOperatorParamClass();
                    var p21 = Dsl.Utility.CloneDsl(p1);
                    p2.AddParam(p21);
                    p2.AddParam(new Dsl.ValueData("1", AbstractSyntaxComponent.NUM_TOKEN));
                    funcData.AddParam(p2);

                    return TransformStatement(funcData, out insertBeforeOuter, out insertAfterOuter);
                }
                else if (funcId == "<-") {
                    var p = funcData.GetParam(0);
                    var v = funcData.GetParam(1);
                    var fd = p as Dsl.FunctionData;
                    TransformExpression(v, ref semanticInfo);
                    if (null != fd) {
                        funcData.LowerOrderFunction = fd;
                    }
                    else {
                        funcData.Name.SetId("return");
                    }
                    funcData.SetParenthesesParamClass();
                    funcData.Params.Clear();
                    funcData.AddParam(v);

                    TransformReturn(funcData, semanticInfo.GraphNode, out insertBeforeOuter, out insertAfterOuter);
                }
                else if (funcId == "if") {
                    TransformIfFunction(funcData, out insertBeforeOuter, out insertAfterOuter);
                }
                else if (funcId == "switch") {
                    Console.WriteLine("Can't rewrite GLSL including switch !");

                    TransformSwitchFunction(funcData, out insertBeforeOuter, out insertAfterOuter);
                }
                else if (funcId == "for") {
                    TransformForFunction(funcData, out insertBeforeOuter, out insertAfterOuter);
                }
                else if (funcId == "while") {
                    TransformWhileFunction(funcData, out insertBeforeOuter, out insertAfterOuter);
                }
                else {
                    TransformGeneralFunction(funcData, ref semanticInfo);
                }
            }
            else if (null != stmData) {
                var firstValOrFunc = stmData.GetFunction(0);
                string funcId = firstValOrFunc.GetId();
                if (funcId == "if") {
                    TransformIfStatement(stmData, out insertBeforeOuter, out insertAfterOuter);
                }
                else if (funcId == "do") {
                    TransformDoWhileStatement(stmData, out insertBeforeOuter, out insertAfterOuter);
                }
                else if (funcId == "?") {
                    Debug.Assert(false);
                    TransformConditionStatement(stmData, ref semanticInfo);
                }
                else {
                    TransformVar(stmData, 0, false, ref semanticInfo);
                }
            }
            return null != insertBeforeOuter || null != insertAfterOuter;
        }

        //Expression syntax processing, expressions always have a return value.
        private static void TransformExpression(Dsl.ISyntaxComponent syntax, ref SemanticInfo semanticInfo)
        {
            string resultType = TypeInference(syntax);
            semanticInfo.ResultType = resultType;

            var valData = syntax as Dsl.ValueData;
            var funcData = syntax as Dsl.FunctionData;
            var stmData = syntax as Dsl.StatementData;
            if (null != valData) {
                TransformValueExpression(valData, ref semanticInfo);
            }
            else if (null != funcData) {
                TransformFunctionExpression(funcData, ref semanticInfo);
            }
            else if (null != stmData) {
                TransformStatementExpression(stmData, ref semanticInfo);
            }
        }
        private static void TransformStatementExpression(Dsl.StatementData stmData, ref SemanticInfo semanticInfo)
        {
            var firstValOrFunc = stmData.GetFunction(0);
            string funcId = firstValOrFunc.GetId();
            if (funcId == "?") {
                TransformConditionStatement(stmData, ref semanticInfo);
            }
            else {
                TransformVar(stmData, 0, false, ref semanticInfo);
            }
        }
        private static void TransformFunctionExpression(Dsl.FunctionData funcData, ref SemanticInfo semanticInfo)
        {
            string funcId = funcData.GetId();
            if (funcId == "=") {
                var p = funcData.GetParam(0);
                var vd = p as Dsl.ValueData;
                var fd = p as Dsl.FunctionData;
                var sd = p as Dsl.StatementData;
                var tempVarSi = new SemanticInfo();
                if (null != vd) {
                    TransformAssignLeft(vd, ref tempVarSi);
                }
                else if (null != fd) {
                    TransformGeneralCall(fd, ref tempVarSi);
                }
                else if (null != sd) {
                    TransformAssignLeft(sd, 0, false, ref tempVarSi);
                }
                var tempValSi = new SemanticInfo();
                var v = funcData.GetParam(1);
                TransformExpression(v, ref tempValSi);
                HandleVarAliasInfoUpdate();

                var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), tempVarSi.ResultType, "=");

                var vgn = tempVarSi.GraphNode;
                var agn = tempValSi.GraphNode;
                Debug.Assert(null != vgn);
                Debug.Assert(null != agn);

                cgcn.AddPrev(agn);
                agn.AddNext(cgcn);

                cgcn.AddNext(vgn);
                vgn.AddPrev(cgcn);

                semanticInfo.GraphNode = vgn;

                if (!string.IsNullOrEmpty(tempVarSi.ResultType) && !string.IsNullOrEmpty(tempValSi.ResultType) && tempVarSi.ResultType != tempValSi.ResultType) {
                    Console.WriteLine("type '{0}' assigned to type '{1}', line {2} dsl:{3}", tempValSi.ResultType, tempVarSi.ResultType, funcData.GetLine(), funcData.ToScriptString(false, Dsl.DelimiterInfo.Default));
                }
            }
            else {
                TransformGeneralFunction(funcData, ref semanticInfo);
            }
        }
        private static void TransformValueExpression(Dsl.ValueData valData, ref SemanticInfo semanticInfo)
        {
            TransformVar(valData, ref semanticInfo);
        }

        private static void TransformGeneralFunction(Dsl.FunctionData funcData, ref SemanticInfo semanticInfo)
        {
            if (funcData.HaveStatement()) {
                if (funcData.IsHighOrder) {
                    var lowerFunc = funcData.LowerOrderFunction;
                    TransformGeneralFunction(lowerFunc, ref semanticInfo);
                }

                PushBlock(false, false);

                var block = new ComputeGraphBlock(CurFuncInfo());
                AddComputeGraphRootNode(block);
                SetCurBlock(block);

                TransformFunctionStatements(funcData);
                PopBlock();
            }
            else {
                TransformGeneralCall(funcData, ref semanticInfo);
            }
        }
        private static void TransformGeneralCall(Dsl.FunctionData call, ref SemanticInfo semanticInfo)
        {
            int paramNum = call.GetParamNum();
            int paramClass = call.GetParamClassUnmasked();
            if (!call.HaveId()) {
                switch (paramClass) {
                    case (int)Dsl.ParamClassEnum.PARAM_CLASS_PARENTHESES: {
                            if (paramNum == 1) {
                                var pp = call.GetParam(0);
                                var innerCall = pp as Dsl.FunctionData;
                                if (null != innerCall) {
                                    if (!innerCall.HaveId() && innerCall.IsParenthesesParamClass()) {
                                        call.ClearParams();
                                        call.Params.AddRange(innerCall.Params);
                                        TransformGeneralCall(call, ref semanticInfo);
                                        return;
                                    }
                                }
                                TransformExpression(pp, ref semanticInfo);
                            }
                            else {
                                for (int ix = 0; ix < paramNum; ++ix) {
                                    var syntax = call.GetParam(ix);
                                    TransformExpression(syntax, ref semanticInfo);
                                }
                            }
                        }
                        break;
                    case (int)Dsl.ParamClassEnum.PARAM_CLASS_BRACKET: {

                        }
                        break;
                }
                return;
            }
            if (call.IsHighOrder) {
                var lowerFunc = call.LowerOrderFunction;
                if (lowerFunc.IsBracketParamClass() && call.IsParenthesesParamClass()) {
                    //todo: array init
                    return;
                }
            }
            if (paramClass == (int)Dsl.ParamClassEnum.PARAM_CLASS_PARENTHESES) {
                string id = call.GetId();
                List<string> argTypes = new List<string>();
                var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), string.Empty, id);
                for (int ix = 0; ix < paramNum; ++ix) {
                    var syntax = call.GetParam(ix);
                    if (syntax.IsValid()) {
                        var tsemanticInfo = new SemanticInfo();
                        TransformExpression(syntax, ref tsemanticInfo);

                        var agn = tsemanticInfo.GraphNode;
                        Debug.Assert(null != agn);
                        argTypes.Add(agn.Type);

                        cgcn.AddPrev(agn);
                        agn.AddNext(cgcn);
                    }
                }
                cgcn.Type = FunctionTypeInference(id, argTypes, call, out var finfo);
                semanticInfo.GraphNode = cgcn;
            }
            else if (paramClass == (int)Dsl.ParamClassEnum.PARAM_CLASS_OPERATOR) {
                string id = call.GetId();
                List<string> argTypes = new List<string>();
                var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), string.Empty, id);
                for (int ix = 0; ix < paramNum; ++ix) {
                    var syntax = call.GetParam(ix);
                    var tsemanticInfo = new SemanticInfo();
                    TransformExpression(syntax, ref tsemanticInfo);

                    var agn = tsemanticInfo.GraphNode;
                    Debug.Assert(null != agn);
                    argTypes.Add(agn.Type);

                    cgcn.AddPrev(agn);
                    agn.AddNext(cgcn);
                }
                if (argTypes.Count == 1)
                    cgcn.Type = OperatorTypeInference(id, argTypes[0]);
                else if (argTypes.Count == 2)
                    cgcn.Type = OperatorTypeInference(id, argTypes[1]);
                semanticInfo.GraphNode = cgcn;
            }
            else if (paramClass == (int)Dsl.ParamClassEnum.PARAM_CLASS_PERIOD) {
                var tsemanticInfo = new SemanticInfo();
                if (call.IsHighOrder) {
                    TransformFunctionExpression(call.LowerOrderFunction, ref tsemanticInfo);
                }
                else {
                    TransformValueExpression(call.Name, ref tsemanticInfo);
                }
                var agn1 = tsemanticInfo.GraphNode;
                Debug.Assert(null != agn1);

                string m = call.GetParamId(0);
                var agn2 = new ComputeGraphConstNode(CurFuncInfo(), "string", m, BoxedValue.From(m));

                var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), "float", ".");

                cgcn.AddPrev(agn1);
                agn1.AddNext(cgcn);

                cgcn.AddPrev(agn2);
                agn2.AddNext(cgcn);

                cgcn.Type = MemberTypeInference(".", agn1.Type, string.Empty, m);
                semanticInfo.GraphNode = cgcn;
            }
            else if (paramClass == (int)Dsl.ParamClassEnum.PARAM_CLASS_BRACKET) {
                var tsemanticInfo = new SemanticInfo();
                if (call.IsHighOrder) {
                    TransformFunctionExpression(call.LowerOrderFunction, ref tsemanticInfo);
                }
                else {
                    TransformValueExpression(call.Name, ref tsemanticInfo);
                }
                var agn1 = tsemanticInfo.GraphNode;
                Debug.Assert(null != agn1);

                var tsemanticInfo2 = new SemanticInfo();
                TransformExpression(call.GetParam(0), ref tsemanticInfo2);
                var agn2 = tsemanticInfo2.GraphNode;
                Debug.Assert(null != agn2);

                var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), string.Empty, "[]");

                cgcn.AddPrev(agn1);
                agn1.AddNext(cgcn);

                cgcn.AddPrev(agn2);
                agn2.AddNext(cgcn);

                cgcn.Type = MemberTypeInference("[]", agn1.Type, string.Empty, agn2.Type);
                semanticInfo.GraphNode = cgcn;
            }
        }
        private static void TransformFunctionStatements(Dsl.FunctionData funcData)
        {
            for (int stmIx = 0; stmIx < funcData.GetParamNum(); ++stmIx) {
                Dsl.ISyntaxComponent? syntax = null;
                for (; ; ) {
                    //Remove consecutive semicolons.
                    syntax = funcData.GetParam(stmIx);
                    if (syntax.IsValid()) {
                        break;
                    }
                    else {
                        funcData.Params.Remove(syntax);
                        if (stmIx < funcData.GetParamNum()) {
                            syntax = funcData.GetParam(stmIx);
                        }
                        else {
                            syntax = null;
                            break;
                        }
                    }
                }
                //Process the statement.
                if (stmIx < funcData.GetParamNum() && null != syntax) {
                    if (TransformStatement(syntax, out var insertBefore, out var insertAfter)) {
                        if (null != insertBefore) {
                            foreach (var isyntax in insertBefore) {
                                funcData.Params.Insert(stmIx, isyntax);
                                ++stmIx;
                            }
                        }
                        if (null != insertAfter) {
                            foreach (var isyntax in insertAfter) {
                                funcData.Params.Insert(stmIx + 1, isyntax);
                                ++stmIx;
                            }
                        }
                    }
                }
            }
        }

        //SSA (Static Single Assignment) processing for if statements and loop statements.
        private static bool TransformIfFunction(Dsl.FunctionData ifFunc, out List<Dsl.ISyntaxComponent>? insertBeforeOuter, out List<Dsl.ISyntaxComponent>? insertAfterOuter)
        {
            var semanticInfo = new SemanticInfo();
            insertBeforeOuter = null;
            insertAfterOuter = null;
            if (ifFunc.IsHighOrder) {
                int expListCt = s_ExpressionList.Count;
                var lowerFunc = ifFunc.LowerOrderFunction;

                var ifNode = new ComputeGraphIfStatement(CurFuncInfo());
                AddComputeGraphRootNode(ifNode);
                BoxedValue ifVal = BoxedValue.NullObject;

                TransformGeneralCall(lowerFunc, ref semanticInfo);
                if (null != semanticInfo.GraphNode) {
                    ifNode.AddCondition(semanticInfo.GraphNode);
                    GenerateValueAndExpressionForCondition(lowerFunc, semanticInfo.GraphNode, true, true, true, false, out ifVal);
                }
                if (ifFunc.HaveStatement()) {
                    var suffix = c_PhiTagSeparator + GenUniqueNumber();
                    var oldAliasInfos = CloneVarAliasInfos(CurVarAliasInfos());
                    HashSet<string> setVars = new HashSet<string>();
                    if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                        s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "{", string.Empty, CurBlockId()));
                        ++s_Indent;
                    }
                    PushBlock(true, false);

                    var block = new ComputeGraphBlock(CurFuncInfo());
                    ifNode.AddBlock(block);
                    SetCurBlock(block);

                    SetCurBlockPhiSuffix(suffix);
                    TransformFunctionStatements(ifFunc);
                    foreach (var vname in CurSetVars()) {
                        if (!setVars.Contains(vname))
                            setVars.Add(vname);
                        if (s_SSA) {
                            var assignFunc = BuildPhiVarAliasAssignment(vname, suffix, CurVarAliasInfos(), true);
                            AddPhiVarAssignExpression(assignFunc, ifVal.IsBoolean && ifVal.GetBool());
                            ifFunc.AddParam(assignFunc);
                        }
                    }
                    PopBlock();
                    if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                        --s_Indent;
                        s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "}", string.Empty, CurBlockId()));
                    }
                    if (s_SSA) {
                        // 1. Before the start of a single-branch if statement, use phi variable aliases to temporarily store the values of variables that
                        // may be assigned within the if branch (these phi variables will not be used within the if statement, but will be used
                        // by the code after the if statement).
                        // 2. Update the aliases of each variable to the phi variable aliases.
                        insertBeforeOuter = new List<ISyntaxComponent>();
                        Debug.Assert(null != insertBeforeOuter);
                        foreach (var vname in setVars) {
                            var assignFunc = BuildPhiVarAliasAssignment(vname, suffix, oldAliasInfos, false);
                            AddPhiVarAssignExpression(assignFunc, ifVal.IsBoolean && !ifVal.GetBool(), ref expListCt);
                            insertBeforeOuter.Add(assignFunc);

                            var curAliasInfos = CurVarAliasInfos();
                            if (curAliasInfos.TryGetValue(vname, out var varAliasInfo)) {
                                varAliasInfo.AliasSuffix = suffix;
                                TryAddComputeGraphVarNode(vname, suffix);
                            }
                        }
                    }
                }
            }
            return insertBeforeOuter != null || insertAfterOuter != null;
        }
        private static bool TransformIfStatement(Dsl.StatementData ifStatement, out List<Dsl.ISyntaxComponent>? insertBeforeOuter, out List<Dsl.ISyntaxComponent>? insertAfterOuter)
        {
            insertBeforeOuter = null;
            insertAfterOuter = null;

            var ifNode = new ComputeGraphIfStatement(CurFuncInfo());
            AddComputeGraphRootNode(ifNode);

            var suffix = c_PhiTagSeparator + GenUniqueNumber();
            var oldAliasInfos = CloneVarAliasInfos(CurVarAliasInfos());
            HashSet<string> setVars = new HashSet<string>();
            int expListCt = s_ExpressionList.Count;
            bool useBranch = false;
            foreach (var valOrFunc in ifStatement.Functions) {
                var f = valOrFunc.AsFunction;
                if (null != f) {
                    BoxedValue ifVal = BoxedValue.NullObject;
                    if (f.IsHighOrder) {
                        var semanticInfo = new SemanticInfo();
                        TransformGeneralCall(f.LowerOrderFunction, ref semanticInfo);
                        if (null != semanticInfo.GraphNode) {
                            ifNode.AddCondition(semanticInfo.GraphNode);
                            if (GenerateValueAndExpressionForCondition(f.LowerOrderFunction, semanticInfo.GraphNode, true, true, true, false, out ifVal))
                                useBranch = true;
                        }
                    }
                    else if (valOrFunc == ifStatement.Last) {
                        useBranch = true;
                    }
                    if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                        s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "{", string.Empty, CurBlockId()));
                        ++s_Indent;
                    }
                    PushBlock(true, false);

                    var block = new ComputeGraphBlock(CurFuncInfo());
                    ifNode.AddBlock(block);
                    SetCurBlock(block);

                    SetCurBlockPhiSuffix(suffix);
                    TransformFunctionStatements(f);
                    //Each branch handles the variables it has assigned, while also consolidating the variable names.
                    foreach (var vname in CurSetVars()) {
                        if (!setVars.Contains(vname))
                            setVars.Add(vname);
                        if (s_SSA) {
                            var assignFunc = BuildPhiVarAliasAssignment(vname, suffix, CurVarAliasInfos(), true);
                            AddPhiVarAssignExpression(assignFunc, ifVal.IsBoolean && ifVal.GetBool());
                            f.AddParam(assignFunc);
                        }
                    }
                    PopBlock();
                    if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                        --s_Indent;
                        s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "}", string.Empty, CurBlockId()));
                    }
                }
            }
            if (s_SSA) {
                // 1. Before the start of a multi-branch if statement, use phi variable aliases to temporarily store the values of variables that may be assigned within each branch
                // (these phi variables will not be used within the if statement, but will be used by the code after the if statement).
                // 2. Update the aliases of each variable to the phi variable aliases.
                insertBeforeOuter = new List<ISyntaxComponent>();
                Debug.Assert(null != insertBeforeOuter);
                foreach (var vname in setVars) {
                    var assignFunc = BuildPhiVarAliasAssignment(vname, suffix, oldAliasInfos, false);
                    AddPhiVarAssignExpression(assignFunc, !useBranch, ref expListCt);
                    insertBeforeOuter.Add(assignFunc);

                    var curAliasInfos = CurVarAliasInfos();
                    if (curAliasInfos.TryGetValue(vname, out var varAliasInfo)) {
                        varAliasInfo.AliasSuffix = suffix;
                        TryAddComputeGraphVarNode(vname, suffix);
                    }
                }
            }
            return insertBeforeOuter != null || insertAfterOuter != null;
        }
        private static bool TransformSwitchFunction(Dsl.FunctionData ifFunc, out List<Dsl.ISyntaxComponent>? insertBeforeOuter, out List<Dsl.ISyntaxComponent>? insertAfterOuter)
        {
            //I won't support it for now. It seems that decompiled shaders generally do not use switch statements
            insertBeforeOuter = null;
            insertAfterOuter = null;
            return insertBeforeOuter != null || insertAfterOuter != null;
        }
        private static bool TransformForFunction(Dsl.FunctionData forFunc, out List<Dsl.ISyntaxComponent>? insertBeforeOuter, out List<Dsl.ISyntaxComponent>? insertAfterOuter)
        {
            insertBeforeOuter = null;
            insertAfterOuter = null;
            var semanticInfo = new SemanticInfo();
            if (forFunc.IsHighOrder) {
                int expListCt = s_ExpressionList.Count;
                var lowerFunc = forFunc.LowerOrderFunction;

                var forNode = new ComputeGraphForStatement(CurFuncInfo());
                AddComputeGraphRootNode(forNode);
                BoxedValue forVal = BoxedValue.NullObject;

                TransformForHeader(lowerFunc, ref semanticInfo);
                if (null != semanticInfo.GraphNode) {
                    forNode.ForFunc = semanticInfo.GraphNode;
                    GenerateValueAndExpressionForCondition(lowerFunc, semanticInfo.GraphNode, true, true, true, false, out forVal);
                }
                if (forFunc.HaveStatement()) {
                    var suffix = c_PhiTagSeparator + GenUniqueNumber();
                    var oldAliasInfos = CloneVarAliasInfos(CurVarAliasInfos());
                    if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                        s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "{", string.Empty, CurBlockId()));
                        ++s_Indent;
                    }
                    PushBlock(true, true);

                    var block = new ComputeGraphBlock(CurFuncInfo());
                    forNode.Block = block;
                    SetCurBlock(block);

                    SetCurBlockPhiSuffix(suffix);
                    TransformFunctionStatements(forFunc);
                    ProcessLoopPhiVarAlias(suffix, oldAliasInfos, forFunc, expListCt, forVal.IsBoolean && forVal.GetBool(), out var newRefVarValDataOuter, out insertBeforeOuter);
                    PopBlock();
                    if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                        --s_Indent;
                        s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "}", string.Empty, CurBlockId()));
                    }
                    if (null != newRefVarValDataOuter) {
                        foreach (var pair in newRefVarValDataOuter) {
                            foreach (var vd in pair.Value) {
                                TryAddUndeterminedVarAlias(pair.Key, vd);
                            }
                        }
                    }
                }
            }
            return insertBeforeOuter != null || insertAfterOuter != null;
        }
        private static bool TransformWhileFunction(Dsl.FunctionData whileFunc, out List<Dsl.ISyntaxComponent>? insertBeforeOuter, out List<Dsl.ISyntaxComponent>? insertAfterOuter)
        {
            insertBeforeOuter = null;
            insertAfterOuter = null;
            var semanticInfo = new SemanticInfo();
            if (whileFunc.IsHighOrder) {
                int expListCt = s_ExpressionList.Count;
                var lowerFunc = whileFunc.LowerOrderFunction;

                var whileNode = new ComputeGraphWhileStatement(CurFuncInfo());
                AddComputeGraphRootNode(whileNode);
                BoxedValue whileVal = BoxedValue.NullObject;

                TransformGeneralCall(lowerFunc, ref semanticInfo);
                if (null != semanticInfo.GraphNode) {
                    whileNode.WhileFunc = semanticInfo.GraphNode;
                    GenerateValueAndExpressionForCondition(lowerFunc, semanticInfo.GraphNode, true, true, true, false, out whileVal);
                }
                if (whileFunc.HaveStatement()) {
                    var suffix = c_PhiTagSeparator + GenUniqueNumber();
                    var oldAliasInfos = CloneVarAliasInfos(CurVarAliasInfos());
                    if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                        s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "{", string.Empty, CurBlockId()));
                        ++s_Indent;
                    }
                    PushBlock(true, true);

                    var block = new ComputeGraphBlock(CurFuncInfo());
                    whileNode.Block = block;
                    SetCurBlock(block);

                    SetCurBlockPhiSuffix(suffix);
                    TransformFunctionStatements(whileFunc);
                    ProcessLoopPhiVarAlias(suffix, oldAliasInfos, whileFunc, expListCt, whileVal.IsBoolean && whileVal.GetBool(), out var newRefVarValDataOuter, out insertBeforeOuter);
                    PopBlock();
                    if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                        --s_Indent;
                        s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "}", string.Empty, CurBlockId()));
                    }
                    if (null != newRefVarValDataOuter) {
                        foreach (var pair in newRefVarValDataOuter) {
                            foreach (var vd in pair.Value) {
                                TryAddUndeterminedVarAlias(pair.Key, vd);
                            }
                        }
                    }
                }
            }
            return insertBeforeOuter != null || insertAfterOuter != null;
        }
        private static bool TransformDoWhileStatement(Dsl.StatementData stm, out List<Dsl.ISyntaxComponent>? insertBeforeOuter, out List<Dsl.ISyntaxComponent>? insertAfterOuter)
        {
            insertBeforeOuter = null;
            insertAfterOuter = null;
            var semanticInfo = new SemanticInfo();
            var doFunc = stm.First.AsFunction;
            var whileFunc = stm.Second.AsFunction;
            Debug.Assert(null != doFunc);
            Debug.Assert(null != whileFunc);

            var doWhileNode = new ComputeGraphDoWhileStatement(CurFuncInfo());
            AddComputeGraphRootNode(doWhileNode);

            if (doFunc.HaveStatement()) {
                int expListCt = s_ExpressionList.Count;
                var suffix = c_PhiTagSeparator + GenUniqueNumber();
                var oldAliasInfos = CloneVarAliasInfos(CurVarAliasInfos());
                if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                    s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "do {", string.Empty, CurBlockId()));
                    ++s_Indent;
                }
                PushBlock(true, true);

                var block = new ComputeGraphBlock(CurFuncInfo());
                doWhileNode.Block = block;
                SetCurBlock(block);

                SetCurBlockPhiSuffix(suffix);
                TransformFunctionStatements(doFunc);
                ProcessLoopPhiVarAlias(suffix, oldAliasInfos, doFunc, expListCt, true, out var newRefVarValDataOuter, out insertBeforeOuter);
                PopBlock();
                if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                    --s_Indent;
                    s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "}", string.Empty, CurBlockId()));
                }
                if (null != newRefVarValDataOuter) {
                    foreach (var pair in newRefVarValDataOuter) {
                        foreach (var vd in pair.Value) {
                            TryAddUndeterminedVarAlias(pair.Key, vd);
                        }
                    }
                }
            }
            TransformGeneralCall(whileFunc, ref semanticInfo);
            if (null != semanticInfo.GraphNode) {
                doWhileNode.WhileFunc = semanticInfo.GraphNode;
                GenerateValueAndExpressionForCondition(whileFunc, semanticInfo.GraphNode, true, true, true, true, out var whileVal);
            }
            return insertBeforeOuter != null || insertAfterOuter != null;
        }
        private static bool TransformBreak(Dsl.ValueData valData, out List<Dsl.ISyntaxComponent>? insertBeforeOuter, out List<Dsl.ISyntaxComponent>? insertAfterOuter)
        {
            var breakNode = new ComputeGraphBreakNode(CurFuncInfo(), "break");
            AddComputeGraphRootNode(breakNode);

            if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "break;", string.Empty, CurBlockId()));
            }
            insertAfterOuter = null;
            var suffix = GetCurLoopBlockPhiSuffix();
            insertBeforeOuter = new List<ISyntaxComponent>();
            Debug.Assert(null != insertBeforeOuter);
            foreach (var vname in CurLoopSetVars()) {
                var assignFunc = BuildPhiVarAliasAssignment(vname, suffix, CurVarAliasInfos(), false);
                insertBeforeOuter.Add(assignFunc);
            }
            return insertBeforeOuter != null || insertAfterOuter != null;
        }
        private static bool TransformContinue(Dsl.ValueData valData, out List<Dsl.ISyntaxComponent>? insertBeforeOuter, out List<Dsl.ISyntaxComponent>? insertAfterOuter)
        {
            var breakNode = new ComputeGraphBreakNode(CurFuncInfo(), "continue");
            AddComputeGraphRootNode(breakNode);

            if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "continue;", string.Empty, CurBlockId()));
            }
            insertAfterOuter = null;
            var suffix = GetCurLoopBlockPhiSuffix();
            insertBeforeOuter = new List<ISyntaxComponent>();
            Debug.Assert(null != insertBeforeOuter);
            foreach (var vname in CurLoopSetVars()) {
                var assignFunc = BuildPhiVarAliasAssignment(vname, suffix, CurVarAliasInfos(), false);
                insertBeforeOuter.Add(assignFunc);
            }
            return insertBeforeOuter != null || insertAfterOuter != null;
        }
        private static bool TransformDiscard(Dsl.ValueData valData, out List<Dsl.ISyntaxComponent>? insertBeforeOuter, out List<Dsl.ISyntaxComponent>? insertAfterOuter)
        {
            var breakNode = new ComputeGraphBreakNode(CurFuncInfo(), "discard");
            AddComputeGraphRootNode(breakNode);

            insertBeforeOuter = null;
            insertAfterOuter = null;
            if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "discard;", string.Empty, CurBlockId()));
            }
            return insertBeforeOuter != null || insertAfterOuter != null;
        }
        private static bool TransformReturn(Dsl.ValueData valData, out List<Dsl.ISyntaxComponent>? insertBeforeOuter, out List<Dsl.ISyntaxComponent>? insertAfterOuter)
        {
            var breakNode = new ComputeGraphBreakNode(CurFuncInfo(), "return");
            AddComputeGraphRootNode(breakNode);

            insertBeforeOuter = null;
            insertAfterOuter = null;
            if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "return;", string.Empty, CurBlockId()));
            }
            return insertBeforeOuter != null || insertAfterOuter != null;
        }
        private static bool TransformReturn(Dsl.FunctionData funcData, ComputeGraphNode? expression, out List<Dsl.ISyntaxComponent>? insertBeforeOuter, out List<Dsl.ISyntaxComponent>? insertAfterOuter)
        {
            var breakNode = new ComputeGraphBreakNode(CurFuncInfo(), "return");
            AddComputeGraphRootNode(breakNode);
            breakNode.Expression = expression;

            insertBeforeOuter = null;
            insertAfterOuter = null;
            if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                string expStr = string.Empty;
                if (null != expression) {
                    int defMaxLvl = Config.ActiveConfig.SettingInfo.DefMaxLevel;
                    int defMaxLen = Config.ActiveConfig.SettingInfo.DefMaxLength;
                    expStr = expression.GetExpression(new ComputeSetting(defMaxLvl, defMaxLen, false, false, false, true));
                }
                s_ExpressionList.Add(Tuple.Create(Literal.GetIndentString(s_Indent) + "return " + expStr, string.Empty, CurBlockId()));
            }
            return insertBeforeOuter != null || insertAfterOuter != null;
        }
        private static void TransformConditionStatement(Dsl.StatementData stmData, ref SemanticInfo semanticInfo)
        {
            var tfunc = stmData.GetFunction(0).AsFunction;
            var ffunc = stmData.GetFunction(1).AsFunction;
            Debug.Assert(null != tfunc && null != ffunc);
            var cond = tfunc.LowerOrderFunction.GetParam(0);
            var tval = tfunc.GetParam(0);
            var fval = ffunc.GetParam(0);

            var tsemanticInfo1 = new SemanticInfo();
            var tsemanticInfo2 = new SemanticInfo();
            var tsemanticInfo3 = new SemanticInfo();
            TransformExpression(cond, ref tsemanticInfo1);
            TransformExpression(tval, ref tsemanticInfo2);
            TransformExpression(fval, ref tsemanticInfo3);

            var agn1 = tsemanticInfo1.GraphNode;
            var agn2 = tsemanticInfo2.GraphNode;
            var agn3 = tsemanticInfo3.GraphNode;

            Debug.Assert(null != agn1);
            Debug.Assert(null != agn2);
            Debug.Assert(null != agn3);

            var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), tsemanticInfo2.ResultType, "?:");

            cgcn.AddPrev(agn1);
            agn1.AddNext(cgcn);
            cgcn.AddPrev(agn2);
            agn2.AddNext(cgcn);
            cgcn.AddPrev(agn3);
            agn3.AddNext(cgcn);

            semanticInfo.GraphNode = cgcn;
            semanticInfo.ResultType = cgcn.Type;
        }

        //Processing the "for" part of a "for" statement
        private static void TransformForHeader(Dsl.FunctionData call, ref SemanticInfo semanticInfo)
        {
            int paramNum = call.GetParamNum();
            string id = call.GetId();
            List<string> argTypes = new List<string>();
            var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), string.Empty, id);
            for (int ix = 0; ix < paramNum; ++ix) {
                var syntax = call.GetParam(ix);
                if (syntax.IsValid()) {
                    var tsemanticInfo = new SemanticInfo();
                    if (ix == 1) {
                        TransformExpression(syntax, ref tsemanticInfo);
                    }
                    else {
                        TransformStatementOfForHeader(syntax, ref tsemanticInfo);
                    }

                    var agn = tsemanticInfo.GraphNode;
                    Debug.Assert(null != agn);
                    argTypes.Add(agn.Type);

                    cgcn.AddPrev(agn);
                    agn.AddNext(cgcn);
                }
                else {
                    var agn = new ComputeGraphConstNode(CurFuncInfo(), "string", string.Empty, BoxedValue.EmptyString);
                    cgcn.AddPrev(agn);
                    agn.AddNext(cgcn);
                }
            }
            cgcn.Type = FunctionTypeInference(id, argTypes, call, out var finfo);
            semanticInfo.GraphNode = cgcn;
        }
        private static void TransformStatementOfForHeader(Dsl.ISyntaxComponent syntax, ref SemanticInfo semanticInfo)
        {
            var valData = syntax as Dsl.ValueData;
            var funcData = syntax as Dsl.FunctionData;
            var stmData = syntax as Dsl.StatementData;
            if (null != valData) {
                TransformVar(valData, ref semanticInfo);
            }
            else if (null != funcData) {
                string funcId = funcData.GetId();
                if (funcId == "=") {
                    var p = funcData.GetParam(0);
                    var vd = p as Dsl.ValueData;
                    var fd = p as Dsl.FunctionData;
                    var sd = p as Dsl.StatementData;
                    var tempVarSi = new SemanticInfo();
                    if (null != vd) {
                        TransformAssignLeft(vd, ref tempVarSi);
                    }
                    else if (null != fd) {
                        TransformGeneralCall(fd, ref tempVarSi);
                    }
                    else if (null != sd) {
                        TransformAssignLeft(sd, 0, false, ref tempVarSi);
                    }
                    var tempValSi = new SemanticInfo();
                    var v = funcData.GetParam(1);
                    TransformExpression(v, ref tempValSi);
                    HandleVarAliasInfoUpdate();

                    var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), tempVarSi.ResultType, "=");

                    var vgn = tempVarSi.GraphNode;
                    var agn = tempValSi.GraphNode;
                    Debug.Assert(null != vgn);
                    Debug.Assert(null != agn);

                    cgcn.AddPrev(agn);
                    agn.AddNext(cgcn);

                    cgcn.AddNext(vgn);
                    vgn.AddPrev(cgcn);

                    semanticInfo.GraphNode = vgn;
                }
                else if (funcId.Length >= 2 && funcId[funcId.Length - 1] == '=' && funcId != "==" && funcId != "!=" && funcId != "<=" && funcId != ">=") {
                    //convert to var = var op val
                    var p1 = funcData.GetParam(0);
                    var p22 = funcData.GetParam(1);
                    funcData.Name.SetId("=");

                    var p2 = new Dsl.FunctionData();
                    p2.Name = new Dsl.ValueData(funcId.Substring(0, funcId.Length - 1), AbstractSyntaxComponent.ID_TOKEN);
                    p2.SetOperatorParamClass();
                    var p21 = Dsl.Utility.CloneDsl(p1);
                    p2.AddParam(p21);
                    p2.AddParam(p22);
                    funcData.Params[1] = p2;

                    var tempVarSi = new SemanticInfo();
                    TransformExpression(p1, ref tempVarSi);

                    var tempValSi = new SemanticInfo();
                    TransformExpression(p2, ref tempValSi);

                    var cgn = tempValSi.GraphNode;
                    if (null != cgn) {
                        if (null != tempVarSi.GraphNode)
                            cgn.AddOut(tempVarSi.GraphNode);

                        semanticInfo.GraphNode = cgn;
                    }
                }
                else if (funcId == "++" || funcId == "--") {
                    //convert to var = var + 1
                    var p1 = funcData.GetParam(0);
                    funcData.Name.SetId("=");

                    var p2 = new Dsl.FunctionData();
                    p2.Name = new Dsl.ValueData(funcId.Substring(0, 1), AbstractSyntaxComponent.ID_TOKEN);
                    p2.SetOperatorParamClass();
                    var p21 = Dsl.Utility.CloneDsl(p1);
                    p2.AddParam(p21);
                    p2.AddParam(new Dsl.ValueData("1", AbstractSyntaxComponent.NUM_TOKEN));
                    funcData.AddParam(p2);

                    var tempVarSi = new SemanticInfo();
                    TransformExpression(p1, ref tempVarSi);

                    var tempValSi = new SemanticInfo();
                    TransformExpression(p2, ref tempValSi);

                    var cgn = tempValSi.GraphNode;
                    if (null != cgn) {
                        if (null != tempVarSi.GraphNode)
                            cgn.AddOut(tempVarSi.GraphNode);

                        semanticInfo.GraphNode = cgn;
                    }
                }
                else {
                    TransformGeneralFunction(funcData, ref semanticInfo);
                }
            }
            else if (null != stmData) {
                var firstValOrFunc = stmData.GetFunction(0);
                string funcId = firstValOrFunc.GetId();
                if (funcId == "?") {
                    Debug.Assert(false);
                    TransformConditionStatement(stmData, ref semanticInfo);
                }
                else {
                    TransformVar(stmData, 0, false, ref semanticInfo);
                }
            }
        }

        private static void ProcessLoopPhiVarAlias(string phiSuffix, Dictionary<string, VarAliasInfo> oldAliasInfos, Dsl.FunctionData loopFunc, int expListCountBeforeLoop, bool isLoopTrue, out Dictionary<string, List<Dsl.ValueData>>? newRefVarValDataOuter, out List<Dsl.ISyntaxComponent>? insertBeforeOuter)
        {
            newRefVarValDataOuter = new Dictionary<string, List<ValueData>>();
            insertBeforeOuter = new List<ISyntaxComponent>();
            if (!s_SSA)
                return;
            Debug.Assert(null != insertBeforeOuter);
            var undeterminedAliasTable = CurUndeterminedVarAliasTable();
            foreach (var vname in CurSetVars()) {
                //Handling unresolved aliases within the loop
                string phiVarName = vname + phiSuffix;
                if (undeterminedAliasTable.TryGetValue(vname, out var info)) {
                    foreach (var valData in info.ValueDatas) {
                        valData.SetId(phiVarName);
                    }
                    undeterminedAliasTable.Remove(vname);
                }

                //Adding phi variable assignments before the end of the loop
                var assignFunc = BuildPhiVarAliasAssignment(vname, phiSuffix, CurVarAliasInfos(), true);
                loopFunc.AddParam(assignFunc);
                AddPhiVarAssignExpression(assignFunc, isLoopTrue);

                //Adding phi variable assignments before the loop body can potentially introduce unresolved aliases for outer loops (phi variables always compute their values before the loop).
                var assignFunc2 = BuildPhiVarAliasAssignment(vname, phiSuffix, oldAliasInfos, false, out var refVarData);
                AddPhiVarAssignExpression(assignFunc2, true, ref expListCountBeforeLoop);
                if (newRefVarValDataOuter.TryGetValue(vname, out var list)) {
                    if (list.Contains(refVarData))
                        list.Add(refVarData);
                }
                else {
                    list = new List<ValueData>();
                    list.Add(refVarData);
                    newRefVarValDataOuter.Add(vname, list);
                }
                insertBeforeOuter.Add(assignFunc2);
            }
        }
        private static Dsl.FunctionData BuildPhiVarAliasAssignment(string vname, string phiSuffix, Dictionary<string, VarAliasInfo> aliasInfos, bool updateVarAliasInfo)
        {
            return BuildPhiVarAliasAssignment(vname, phiSuffix, aliasInfos, updateVarAliasInfo, out var valData);
        }
        private static Dsl.FunctionData BuildPhiVarAliasAssignment(string vname, string phiSuffix, Dictionary<string, VarAliasInfo> aliasInfos, bool updateVarAliasInfo, out Dsl.ValueData refVarValDataOuter)
        {
            //Note: Phi variable assignments do not generate computation nodes because phi variables themselves are not in SSA form. We break the dependencies before and
            //after the phi variable in the computation graph. When generating phi variable assignment expressions, we attempt to assign a value to the phi variable based
            //on the condition expression's value. Alternatively, we can manually specify a value through variable_assignment (this is mainly used in loop scenarios,
            //as automatic assignment is usually correct for if statements).
            var assignFunc = new Dsl.FunctionData();
            assignFunc.Name = new Dsl.ValueData("=", Dsl.AbstractSyntaxComponent.ID_TOKEN);
            assignFunc.SetOperatorParamClass();
            assignFunc.SetSemiColonSeparator();
            var phiVarName = vname + phiSuffix;
            assignFunc.AddParam(new Dsl.ValueData(phiVarName, Dsl.AbstractSyntaxComponent.ID_TOKEN));
            if (aliasInfos.TryGetValue(vname, out var aliasInfo)) {
                refVarValDataOuter = new Dsl.ValueData(vname + aliasInfo.AliasSuffix, Dsl.AbstractSyntaxComponent.ID_TOKEN);
                if (updateVarAliasInfo) {
                    aliasInfo.AliasSuffix = phiSuffix;
                    TryAddComputeGraphVarNode(vname, phiSuffix);
                }
            }
            else {
                refVarValDataOuter = new Dsl.ValueData(vname, Dsl.AbstractSyntaxComponent.ID_TOKEN);
            }
            assignFunc.AddParam(refVarValDataOuter);

            //To mark the variables that require expression splitting, we need to generate code multiple times to determine the phi variables used.
            //However, this approach helps avoid generating unnecessary assignment statements.
            if (Config.ActiveConfig.SettingInfo.UsedVariables.ContainsKey(phiVarName)) {
                string nvname = refVarValDataOuter.GetId();
                //Since our marking is not recursive, code that has already undergone SSA processing cannot be processed again as input. Otherwise,
                //this check would discard assignment expressions for variables used solely for phi variable assignments. Additionally,
                //in code that has already undergone SSA processing, variable definitions and assignments are separate.
                //Processing it again as input would result in these variables being renamed again.
                //We can avoid this confusion by using the command-line parameter "-nossa".
                if (!IsPhiVar(nvname)) {
                    Config.ActiveConfig.SettingInfo.AutoSplitAddVariable(nvname, string.Empty);
                }
            }
            return assignFunc;
        }
        private static void AddPhiVarAssignExpression(Dsl.FunctionData assignFunc, bool useThisValue)
        {
            int index = -1;
            AddPhiVarAssignExpression(assignFunc, useThisValue, ref index);
        }
        private static void AddPhiVarAssignExpression(Dsl.FunctionData assignFunc, bool useThisValue, ref int index)
        {
            if (Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                var key = assignFunc.GetParamId(0);
                var val = assignFunc.GetParamId(1);
                if (useThisValue) {
                    var vnode = FindComputeGraphVarNode(val);
                    if (null != vnode) {
                        Config.ActiveConfig.SettingInfo.AddAssignment(key, vnode.Type, vnode.GetValue());
                    }
                }
                //Generate assignment statements for the phi variables used
                if (Config.ActiveConfig.SettingInfo.UsedVariables.ContainsKey(key)) {
                    var line = s_ExpressionBuilder;
                    line.Length = 0;
                    line.Append(Literal.GetIndentString(s_Indent));
                    line.Append(key);
                    line.Append(" = ");
                    line.Append(val);
                    line.Append(";");
                    var lineStr = line.ToString();
                    if (index >= 0) {
                        s_ExpressionList.Insert(index, Tuple.Create(lineStr, key, CurBlockId()));
                        ++index;
                    }
                    else {
                        s_ExpressionList.Add(Tuple.Create(lineStr, key, CurBlockId()));
                    }
                }
            }
        }
        private static bool GenerateValueAndExpressionForCondition(Dsl.FunctionData funcData, ComputeGraphNode cgn, bool isVariableSetting, bool markValue, bool markExp, bool addSemiColon, out BoxedValue val)
        {
            bool ret = false;
            val = BoxedValue.NullObject;
            int lvlForExp = 1;
            var v1str = SplitInfoForVariable.s_DefMaxLen;
            var v2str = SplitInfoForVariable.s_DefMultiline;
            var v3str = SplitInfoForVariable.s_DefExpandOnce;
            if (Calculator.TryGetInt(v1str, out var lenForExp) && Calculator.TryGetBool(v2str, out var ml) && Calculator.TryGetBool(v3str, out var once)) {
                ret = GenerateValueAndExpression(funcData, null, cgn, isVariableSetting, markValue, markExp, addSemiColon, lvlForExp, lenForExp, ml, once, out val);
            }
            return ret;
        }
        private static bool GenerateValueAndExpression(Dsl.FunctionData funcData, Dsl.ISyntaxComponent? leftAssignDsl, ComputeGraphNode cgn, bool isVariableSetting, bool markValue, bool markExp, bool addSemiColon, int maxLvlForExp, int maxLenForExp, bool multiline, bool expandedOnlyOnce, out BoxedValue val)
        {
            int defMaxLvl = Config.ActiveConfig.SettingInfo.DefMaxLevel;
            int defMaxLen = Config.ActiveConfig.SettingInfo.DefMaxLength;
            val = cgn.GetValue();
            string valStr = markValue ? val.ToString() : string.Empty;
            string expWithVal = markValue && markExp ? cgn.GetExpression(new ComputeSetting(defMaxLvl, defMaxLen, false, false, true, true)) : string.Empty;
            string exp = markExp ? cgn.GetExpression(new ComputeSetting(maxLvlForExp, maxLenForExp, multiline, expandedOnlyOnce)) : string.Empty;
            string singleLineExp = exp;
            if (multiline) {
                singleLineExp = markExp ? cgn.GetExpression(new ComputeSetting(defMaxLvl, defMaxLen, false, false, false, true)) : string.Empty;

                if (!string.IsNullOrEmpty(valStr) || markExp) {
                    var sb = new StringBuilder(valStr.Length + exp.Length + 128);
                    sb.Append("/* ");
                    if (!string.IsNullOrEmpty(valStr))
                        sb.Append(valStr);
                    if (!string.IsNullOrEmpty(valStr) && markExp)
                        sb.Append("  <=>  ");
                    if (markExp) {
                        if (markValue) {
                            sb.AppendLine(expWithVal);
                            sb.AppendLine("<=>");
                        }
                        sb.AppendLine(singleLineExp);
                        sb.Append("<=>");
                        sb.Append(exp);
                    }
                    sb.AppendLine();
                    sb.Append("*/");
                    funcData.Comments.Add(sb.ToString());
                }
            }
            else {
                if (!string.IsNullOrEmpty(valStr) || markExp) {
                    var sb = new StringBuilder(valStr.Length + exp.Length + 128);
                    sb.Append("// ");
                    if (!string.IsNullOrEmpty(valStr))
                        sb.Append(valStr);
                    if (!string.IsNullOrEmpty(valStr) && markExp)
                        sb.Append("  <=>  ");
                    if (markExp) {
                        if (markValue)
                            sb.AppendLine(expWithVal);
                        sb.Append("// ");
                        sb.Append(exp);
                    }
                    funcData.Comments.Add(sb.ToString());
                }
            }

            if (markExp && isVariableSetting && Config.ActiveConfig.SettingInfo.GenerateExpressionList) {
                var line = s_ExpressionBuilder;
                if (!string.IsNullOrEmpty(valStr) || markValue) {
                    line.Length = 0;
                    line.Append(Literal.GetIndentString(s_Indent));
                    line.Append("// ");
                    if (!string.IsNullOrEmpty(valStr)) {
                        line.Append(valStr);
                    }
                    if (!string.IsNullOrEmpty(valStr) && markValue)
                        line.Append("  <=>  ");
                    if (markValue) {
                        line.Append(expWithVal);
                    }
                    s_ExpressionList.Add(Tuple.Create(line.ToString(), c_CommentTag, CurBlockId()));
                }

                if (null != leftAssignDsl) {
                    string varExp = Config.CalcVariableExpression(leftAssignDsl, out var vname);

                    line.Length = 0;
                    line.Append(Literal.GetIndentString(s_Indent));
                    line.Append(varExp);
                    line.Append(" = ");
                    line.Append(singleLineExp);
                    if (addSemiColon)
                        line.Append(";");
                    if (singleLineExp.IndexOf(' ') > 0) {
                        int blockId = CurBlockId();
                        //Only check multi-parameter functions and operator expressions to avoid mistaking constants or "utof/ftou" as duplicate expressions.
                        if (!s_Expression2VarList.TryGetValue(blockId, out var exp2vars)) {
                            exp2vars = new Dictionary<string, List<Tuple<string, string>>>();
                            s_Expression2VarList.Add(blockId, exp2vars);
                        }
                        if(!s_DuplicateVars.TryGetValue(blockId, out var dupvars)) {
                            dupvars = new Dictionary<string, Tuple<string, Regex>>();
                            s_DuplicateVars.Add(blockId, dupvars);
                        }
                        if (exp2vars.TryGetValue(singleLineExp, out var varlist)) {
                            line.Append(" // maybe duplicate expression on the right side of the assignment, vars:{0}", string.Join('|', varlist));
                            var tuple = Tuple.Create(varExp, vname);
                            if (varlist.Contains(tuple)) {
                                //
                            }
                            else {
                                varlist.Add(tuple);
                            }
                            string firstVar = varlist[0].Item2;
                            if (!string.IsNullOrEmpty(firstVar) && !string.IsNullOrEmpty(vname)) {
                                var regex = new Regex(@"\b" + vname + @"\b", RegexOptions.Singleline | RegexOptions.Compiled);
                                dupvars.Add(vname, Tuple.Create(firstVar, regex));
                            }
                        }
                        else {
                            varlist = new List<Tuple<string, string>>();
                            varlist.Add(Tuple.Create(varExp, vname));
                            exp2vars.Add(singleLineExp, varlist);
                        }
                    }
                    s_ExpressionList.Add(Tuple.Create(line.ToString(), vname, CurBlockId()));

                    if (s_SSA && !string.IsNullOrEmpty(vname)) {
                        int len = vname.LastIndexOf(c_AliasSeparator);
                        if (len > 0) {
                            string oriVarName = vname.Substring(0, len);
                            var vinfo = GetVarInfo(oriVarName);
                            if (null != vinfo && null != vinfo.OwnFunc) {
                                string type = vinfo.Type;
                                if (vinfo.Modifiers.Contains("precise"))
                                    type = "precise " + type;
                                Config.ActiveConfig.SettingInfo.AddUsedVariable(vname, type);
                            }
                        }
                    }
                }
                else {
                    line.Length = 0;
                    line.Append(Literal.GetIndentString(s_Indent));
                    line.Append(singleLineExp);
                    if (addSemiColon)
                        line.Append(";");
                    s_ExpressionList.Add(Tuple.Create(line.ToString(), string.Empty, CurBlockId()));
                }
            }
            return !val.IsNullObject;
        }

        //SSA handling of variable.
        private static void TransformVar(Dsl.ValueData valData, ref SemanticInfo semanticInfo)
        {
            //If a variable does not appear on the left side of an assignment statement, it may still be a variable definition (when defining multiple variables of the same type).
            if (valData.IsId()) {
                string vid = valData.GetId();
                if (vid != "true" && vid != "false" && vid.IndexOf(' ') < 0) {
                    var vinfo = GetVarInfo(vid);
                    if (null == vinfo) {
                        var lastVarInfo = GetLastVarType();
                        if (null != lastVarInfo) {
                            var varInfo = new VarInfo();
                            varInfo.CopyFrom(lastVarInfo);
                            varInfo.Name = vid;
                            AddVar(varInfo);

                            var cgvn = FindComputeGraphVarNode(varInfo.Name);
                            if (null == cgvn) {
                                Console.WriteLine("[Error]: variable '{0}' undefined, line {1} in file {2}.", varInfo.Name, valData.GetLine(), s_SrcFileForDSL);
                            }
                            semanticInfo.GraphNode = cgvn;
                            semanticInfo.ResultType = varInfo.Type;
                        }
                        else {
                            Console.WriteLine("[Error]: variable '{0}' undefined, line {1} in file {2}.", vid, valData.GetLine(), s_SrcFileForDSL);
                        }
                    }
                    else {
                        if (s_SSA && null != vinfo.OwnFunc) {
                            if (CurVarAliasInfos().TryGetValue(vid, out var info)) {
                                valData.SetId(vid + info.AliasSuffix);
                            }
                            //If the referenced variable is not assigned in the current loop, add it to the unresolved alias table
                            //(this variable may be assigned later in the loop, and if it is assigned, the alias needs to be modified to the phi variable alias).
                            TryAddUndeterminedVarAlias(vid, valData);
                        }
                        var cgvn = FindComputeGraphVarNode(valData.GetId());
                        if (null == cgvn) {
                            Console.WriteLine("[Error]: variable '{0}' uninitialized, line {1} in file {2}.", valData.GetId(), valData.GetLine(), s_SrcFileForDSL);
                        }
                        semanticInfo.GraphNode = cgvn;
                        semanticInfo.ResultType = vinfo.Type;
                    }
                }
                else {
                    string str = valData.GetId();
                    bool v = Calculator.TryParseBool(str, out var bval);
                    var cgcn = new ComputeGraphConstNode(CurFuncInfo(), "bool", str, v ? BoxedValue.From(bval) : BoxedValue.NullObject);
                    semanticInfo.GraphNode = cgcn;
                    semanticInfo.ResultType = cgcn.Type;
                }
            }
            else {
                string type = string.Empty;
                string strVal = valData.GetId();
                if (!Calculator.TryParseNumeric(strVal, ref type, out var numVal)) {
                    type = "string";
                    numVal = BoxedValue.From(strVal);
                }
                var cgcn = new ComputeGraphConstNode(CurFuncInfo(), type, strVal, numVal);
                semanticInfo.GraphNode = cgcn;
                semanticInfo.ResultType = cgcn.Type;
            }
        }
        private static void TransformVar(Dsl.StatementData stmData, int index, bool toplevel, ref SemanticInfo semanticInfo)
        {
            //Uninitialized variable definition.
            TransformVar(stmData, index, toplevel, null, ref semanticInfo);
        }
        private static void TransformVar(Dsl.StatementData stmData, int index, bool toplevel, Dsl.StatementData? oriStmData, ref SemanticInfo semanticInfo)
        {
            //Uninitialized variable definition.
            var lastId = stmData.Last.GetId();
            var last = stmData.Last.AsFunction;
            if (null == last || (last.IsBracketParamClass() && !last.HaveStatement())) {
                bool isType = true;
                bool needAdjustArrTag = false;
                int funcNum = stmData.GetFunctionNum();
                for (int ix = index; ix < funcNum; ++ix) {
                    var valOrFunc = stmData.GetFunction(ix);
                    var val = valOrFunc.AsValue;
                    var func = valOrFunc.AsFunction;
                    if (null == val) {
                        if (ix < funcNum - 2) {
                            isType = false;
                            break;
                        }
                        else {
                            if (func.IsBracketParamClass()) {
                                if (ix == funcNum - 2) {
                                    needAdjustArrTag = true;
                                }
                            }
                            else {
                                isType = false;
                                break;
                            }
                        }
                    }
                }
                if (isType) {
                    VarInfo vinfo = ParseVarInfo(stmData);
                    AddVar(vinfo);

                    var cgvn = FindComputeGraphVarNode(vinfo.Name);
                    semanticInfo.GraphNode = cgvn;
                    semanticInfo.ResultType = vinfo.Type;

                    if (needAdjustArrTag) {
                        var typeFunc = stmData.GetFunction(funcNum - 2).AsFunction;
                        var nameValOrFunc = stmData.GetFunction(funcNum - 1);
                        var nameVal = nameValOrFunc.AsValue;
                        var nameFunc = nameValOrFunc.AsFunction;
                        if (null != nameVal) {
                            nameFunc = new Dsl.FunctionData();
                            nameFunc.Name = nameVal;
                            stmData.SetFunction(funcNum - 1, nameFunc);
                        }
                        else {
                            Debug.Assert(null != nameFunc);
                            while (nameFunc.IsHighOrder) {
                                nameFunc = nameFunc.LowerOrderFunction;
                            }
                        }
                        Debug.Assert(null != nameFunc);
                        while (null != typeFunc) {
                            var newFunc = new Dsl.FunctionData();
                            newFunc.Name = nameFunc.Name;
                            newFunc.SetParamClass(typeFunc.GetParamClass());
                            newFunc.Params = typeFunc.Params;
                            newFunc.SetSeparator(typeFunc.GetSeparator());
                            nameFunc.LowerOrderFunction = newFunc;
                            nameFunc = newFunc;
                            if (typeFunc.IsHighOrder) {
                                typeFunc = typeFunc.LowerOrderFunction;
                            }
                            else {
                                stmData.SetFunction(funcNum - 2, typeFunc.Name);
                                typeFunc = null;
                            }
                        }
                    }
                }
            }
        }

        //SSA handling of variable assignment.
        private static void TransformAssignLeft(Dsl.ValueData valData, ref SemanticInfo semanticInfo)
        {
            // The variable appears on the left side of the assignment statement, which may be a variable definition (when defining multiple variables of the same type) or an assignment.
            if (valData.IsId()) {
                string vid = valData.GetId();
                if (vid != "true" && vid != "false" && vid.IndexOf(' ') < 0) {
                    var vinfo = GetVarInfo(vid);
                    if (null == vinfo) {
                        var lastVarInfo = GetLastVarType();
                        if (null != lastVarInfo) {
                            var varInfo = new VarInfo();
                            varInfo.CopyFrom(lastVarInfo);
                            varInfo.Name = vid;
                            AddVar(varInfo);

                            var cgvn = FindComputeGraphVarNode(vid);
                            semanticInfo.GraphNode = cgvn;
                            semanticInfo.ResultType = varInfo.Type;
                        }
                        else {
                            Debug.Assert(false);
                        }
                    }
                    else {
                        if (s_SSA && null != vinfo.OwnFunc) {
                            if (TryGetVarAliasIndex(vid, out var aliasIndex)) {
                                string nid = vid + c_AliasSeparator + (aliasIndex + 1).ToString();
                                valData.SetId(nid);
                            }
                            else {
                                string nid = vid + c_AliasSeparator + aliasIndex.ToString();
                                valData.SetId(nid);
                            }
                            s_VarAliasInfoUpdateQueue.Enqueue(vid);
                        }

                        if (s_SSA) {
                            var cgvn = new ComputeGraphVarNode(vinfo.OwnFunc, Config.TryReplaceType(vinfo.Name, vinfo.Type), valData.GetId());
                            AddComputeGraphVarNode(cgvn);
                            semanticInfo.GraphNode = cgvn;
                            semanticInfo.ResultType = vinfo.Type;
                        }
                        else {
                            //In code like SSA form, variable definition and assignment are separate. During assignment, the variable is already in the computation graph.
                            var cgvn = FindComputeGraphVarNode(valData.GetId());
                            semanticInfo.GraphNode = cgvn;
                            semanticInfo.ResultType = vinfo.Type;
                        }

                        var setVars = CurSetVars();
                        if (!setVars.Contains(vid)) {
                            setVars.Add(vid);
                        }
                    }
                }
                else {
                    Debug.Assert(false);
                }
            }
            else {
                Debug.Assert(false);
            }
        }
        private static void TransformAssignLeft(Dsl.StatementData stmData, int index, bool toplevel, ref SemanticInfo semanticInfo)
        {
            //Variable definition with assigning initial value.
            TransformAssignLeft(stmData, index, toplevel, null, ref semanticInfo);
        }
        private static void TransformAssignLeft(Dsl.StatementData stmData, int index, bool toplevel, Dsl.StatementData? oriStmData, ref SemanticInfo semanticInfo)
        {
            //Variable definition with assigning initial value.
            var lastId = stmData.Last.GetId();
            var last = stmData.Last.AsFunction;
            if (null == last || (last.IsBracketParamClass() && !last.HaveStatement())) {
                bool isType = true;
                bool needAdjustArrTag = false;
                int funcNum = stmData.GetFunctionNum();
                for (int ix = index; ix < funcNum; ++ix) {
                    var valOrFunc = stmData.GetFunction(ix);
                    var val = valOrFunc.AsValue;
                    var func = valOrFunc.AsFunction;
                    if (null == val) {
                        if (ix < funcNum - 2) {
                            isType = false;
                            break;
                        }
                        else {
                            if (func.IsBracketParamClass()) {
                                if (ix == funcNum - 2) {
                                    needAdjustArrTag = true;
                                }
                            }
                            else {
                                isType = false;
                                break;
                            }
                        }
                    }
                }
                if (isType) {
                    VarInfo vinfo = ParseVarInfo(stmData);
                    AddVar(vinfo);

                    var cgvn = FindComputeGraphVarNode(vinfo.Name);
                    semanticInfo.GraphNode = cgvn;
                    semanticInfo.ResultType = vinfo.Type;

                    if (needAdjustArrTag) {
                        var typeFunc = stmData.GetFunction(funcNum - 2).AsFunction;
                        var nameValOrFunc = stmData.GetFunction(funcNum - 1);
                        var nameVal = nameValOrFunc.AsValue;
                        var nameFunc = nameValOrFunc.AsFunction;
                        if (null != nameVal) {
                            nameFunc = new Dsl.FunctionData();
                            nameFunc.Name = nameVal;
                            stmData.SetFunction(funcNum - 1, nameFunc);
                        }
                        else {
                            Debug.Assert(null != nameFunc);
                            while (nameFunc.IsHighOrder) {
                                nameFunc = nameFunc.LowerOrderFunction;
                            }
                        }
                        Debug.Assert(null != nameFunc);
                        while (null != typeFunc) {
                            var newFunc = new Dsl.FunctionData();
                            newFunc.Name = nameFunc.Name;
                            newFunc.SetParamClass(typeFunc.GetParamClass());
                            newFunc.Params = typeFunc.Params;
                            newFunc.SetSeparator(typeFunc.GetSeparator());
                            nameFunc.LowerOrderFunction = newFunc;
                            nameFunc = newFunc;
                            if (typeFunc.IsHighOrder) {
                                typeFunc = typeFunc.LowerOrderFunction;
                            }
                            else {
                                stmData.SetFunction(funcNum - 2, typeFunc.Name);
                                typeFunc = null;
                            }
                        }
                    }
                }
            }
        }
        private static void HandleVarAliasInfoUpdate()
        {
            while (s_VarAliasInfoUpdateQueue.Count > 0) {
                string vid = s_VarAliasInfoUpdateQueue.Dequeue();
                int aliasIndex = IncVarAliasIndex(vid);
                if (CurVarAliasInfos().TryGetValue(vid, out var info)) {
                    info.AliasSuffix = c_AliasSeparator + aliasIndex.ToString();
                }
                else {
                    info = new VarAliasInfo { AliasSuffix = c_AliasSeparator + aliasIndex.ToString() };
                    CurVarAliasInfos().Add(vid, info);
                }
            }
        }
        private static void TryAddComputeGraphVarNode(string vname, string phiSuffix)
        {
            var vinfo = GetVarInfo(vname);
            Debug.Assert(null != vinfo);
            string phiVarAlias = vname + phiSuffix;
            var node = FindComputeGraphVarNode(phiVarAlias);
            if (null == node) {
                var cgvn = new ComputeGraphVarNode(vinfo.OwnFunc, Config.TryReplaceType(vinfo.Name, vinfo.Type), phiVarAlias);
                AddComputeGraphVarNode(cgvn);

                if (null != vinfo.OwnFunc) {
                    //Only phi variables that are already in the usage list will have their types recorded.
                    if (Config.ActiveConfig.SettingInfo.UsedVariables.ContainsKey(phiVarAlias)) {
                        string type = vinfo.Type;
                        if (vinfo.Modifiers.Contains("precise"))
                            type = "precise " + type;
                        Config.ActiveConfig.SettingInfo.SetUsedVariableType(phiVarAlias, type);
                    }
                }
            }
        }

        //Imprecise syntax handling, this part may require continuous improvement.
        private static bool ParseStatement(Dsl.StatementData stmData, ref int index, out Dsl.FunctionData? f, out List<string> modifiers)
        {
            bool ret = false;
            f = null;
            modifiers = new List<string>();
            for (int ix = index; ix < stmData.GetFunctionNum(); ++ix) {
                var func = stmData.GetFunction(ix);
                string id = func.GetId();
                var f_ = func.AsFunction;
                if (null != f_ && f_.HaveStatement()) {
                    f = f_;
                    ret = true;
                    index = ix + 1;
                    break;
                }
                else {
                    modifiers.Add(id);
                }
            }
            return ret;
        }
        private static bool SplitToplevelStatementsInExpression(Dsl.StatementData expParam, out Dsl.StatementData? left, out Dsl.StatementData? right)
        {
            left = null;
            right = null;
            int funcNum = expParam.GetFunctionNum();
            var lastFunc = expParam.Last.AsFunction;
            if (null != lastFunc && lastFunc.IsHighOrder) {
                //Statement curly braces followed by parentheses expression
                var innerFunc = lastFunc;
                while (innerFunc.IsHighOrder && innerFunc.HaveParam())
                    innerFunc = innerFunc.LowerOrderFunction;
                if (null != innerFunc && innerFunc.HaveStatement()) {
                    left = new Dsl.StatementData();
                    right = new Dsl.StatementData();
                    for (int i = 0; i < funcNum - 1; ++i) {
                        left.AddFunction(expParam.GetFunction(i));
                    }
                    left.AddFunction(innerFunc);
                    var func = new Dsl.FunctionData();
                    right.AddFunction(func);
                    var f = lastFunc;
                    while (f != innerFunc) {
                        func.Params.AddRange(f.Params);
                        f = f.LowerOrderFunction;
                        if (f != innerFunc) {
                            func.LowerOrderFunction = new Dsl.FunctionData();
                            func = func.LowerOrderFunction;
                        }
                    }
                    return true;
                }
            }
            for (int ix = funcNum - 2; ix >= 0; --ix) {
                var func = expParam.GetFunction(ix);
                string id = func.GetId();
                var f_ = func.AsFunction;
                if (null != f_) {
                    if (ix < funcNum - 1 && f_.HaveStatement()) {
                        left = new Dsl.StatementData();
                        right = new Dsl.StatementData();
                        for (int i = 0; i <= ix; ++i) {
                            left.AddFunction(expParam.GetFunction(i));
                        }
                        for (int i = ix + 1; i < funcNum; ++i) {
                            right.AddFunction(expParam.GetFunction(i));
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private static VarInfo ParseVarInfo(Dsl.StatementData varStm)
        {
            var lastId = varStm.Last.GetId();

            var nameInfoFunc = varStm.Last.AsFunction;
            string arrTag = string.Empty;
            if (null != nameInfoFunc) {
                arrTag = BuildTypeWithTypeArgs(nameInfoFunc).Substring(lastId.Length);
            }

            VarInfo vinfo = new VarInfo();
            int funcNum = varStm.GetFunctionNum();
            for (int ix = 0; ix < funcNum - 1; ++ix) {
                var valOrFunc = varStm.GetFunction(ix);
                string id = valOrFunc.GetId();
                if (ix == funcNum - 2) {
                    var func = valOrFunc.AsFunction;
                    if (null != func)
                        vinfo.Type = BuildTypeWithTypeArgs(func) + arrTag;
                    else
                        vinfo.Type = id + arrTag;
                }
                else {
                    if (id == "inout")
                        vinfo.IsInOut = true;
                    else if (id == "out")
                        vinfo.IsOut = true;

                    vinfo.Modifiers.Add(id);
                }
            }
            vinfo.Name = lastId;
            return vinfo;
        }
        private static string BuildTypeWithTypeArgs(Dsl.FunctionData func)
        {
            var sb = new StringBuilder();
            if (func.IsBracketParamClass()) {
                var arrTags = new List<string>();
                string baseType = BuildTypeWithArrTags(func, arrTags);
                sb.Append(baseType);
                for (int ix = arrTags.Count - 1; ix >= 0; --ix) {
                    sb.Append(arrTags[ix]);
                }
            }
            else {
                if (func.IsHighOrder) {
                    sb.Append(BuildTypeWithTypeArgs(func.LowerOrderFunction));
                }
                else {
                    sb.Append(func.GetId());
                }
                foreach (var p in func.Params) {
                    sb.Append('|');
                    sb.Append(DslToNameString(p));
                }
            }
            return sb.ToString();
        }
        private static string BuildTypeWithArrTags(Dsl.FunctionData func, List<string> arrTags)
        {
            string ret = string.Empty;
            if (func.IsBracketParamClass()) {
                if (func.IsHighOrder) {
                    ret = BuildTypeWithArrTags(func.LowerOrderFunction, arrTags);
                }
                else {
                    ret = func.GetId();
                }
                string arrTag = "_x";
                if (func.GetParamNum() > 0) {
                    arrTag += func.GetParamId(0);
                }
                arrTags.Add(arrTag);
            }
            else {
                ret = BuildTypeWithTypeArgs(func);
            }
            return ret;
        }
        private static string DslToNameString(Dsl.ISyntaxComponent syntax)
        {
            var valData = syntax as Dsl.ValueData;
            if (null != valData)
                return valData.GetId();
            else {
                var funcData = syntax as Dsl.FunctionData;
                if (null != funcData) {
                    var sb = new StringBuilder();
                    if (funcData.IsHighOrder) {
                        sb.Append(DslToNameString(funcData.LowerOrderFunction));
                    }
                    else {
                        sb.Append(funcData.GetId());
                    }
                    switch (funcData.GetParamClassUnmasked()) {
                        case (int)Dsl.ParamClassEnum.PARAM_CLASS_PERIOD:
                            sb.Append(".");
                            break;
                        case (int)Dsl.ParamClassEnum.PARAM_CLASS_BRACKET:
                            sb.Append("_x");
                            break;
                        default:
                            if (funcData.GetParamNum() > 0)
                                sb.Append("_");
                            break;
                    }
                    foreach (var p in funcData.Params) {
                        sb.Append(DslToNameString(p));
                    }
                    return sb.ToString();
                }
                else {
                    var stmData = syntax as Dsl.StatementData;
                    if (null != stmData) {
                        var sb = new StringBuilder();
                        for (int ix = 0; ix < stmData.GetFunctionNum(); ++ix) {
                            if (ix > 0)
                                sb.Append("__");
                            var func = stmData.GetFunction(ix);
                            sb.Append(DslToNameString(func));
                        }
                        return sb.ToString();
                    }
                    else {
                        return string.Empty;
                    }
                }
            }
        }

        // Expression Type Inference
        private static string TypeInference(Dsl.ISyntaxComponent syntax)
        {
            var valData = syntax as Dsl.ValueData;
            var funcData = syntax as Dsl.FunctionData;
            var stmData = syntax as Dsl.StatementData;
            if (null != valData) {
                var varInfo = GetVarInfo(valData.GetId());
                if (null != varInfo) {
                    return varInfo.Type;
                }
                else {
                    int idType = valData.GetIdType();
                    string val = valData.GetId();
                    switch (idType) {
                        case Dsl.ValueData.NUM_TOKEN:
                            if (val.Contains('.'))
                                return "float";
                            else
                                return "int";
                        default:
                            if (val == "true" || val == "false")
                                return "bool";
                            return string.Empty;
                    }
                }
            }
            else if (null != funcData) {
                string funcName = funcData.GetId();
                if (funcName == "vec2" || funcName == "vec3" || funcName == "vec4" ||
                    funcName == "ivec2" || funcName == "ivec3" || funcName == "ivec4" ||
                    funcName == "uvec2" || funcName == "uvec3" || funcName == "uvec4" ||
                    funcName == "bvec2" || funcName == "bvec3" || funcName == "bvec4") {
                    return funcName;
                }
                else if (funcName == "float" || funcName == "double" || funcName == "int" ||
                    funcName == "uint" || funcName == "bool" ||
                    funcName == "mat2" || funcName == "mat3" || funcName == "mat4" ||
                    funcName == "dmat2" || funcName == "dmat3" || funcName == "dmat4") {
                    return funcName;
                }
                else if (funcName == "texture") {
                    return "vec4";
                }
                switch (funcData.GetParamClassUnmasked()) {
                    case (int)Dsl.ParamClassEnum.PARAM_CLASS_OPERATOR: {
                            int pnum = funcData.GetParamNum();
                            if (pnum == 1) {
                                string op = funcData.GetId();
                                string p1 = TypeInference(funcData.GetParam(0));
                                return OperatorTypeInference(op, p1);
                            }
                            else {
                                string op = funcData.GetId();
                                string p1 = TypeInference(funcData.GetParam(0));
                                string p2 = TypeInference(funcData.GetParam(1));
                                return OperatorTypeInference(op, p1, p2);
                            }
                        }
                    case (int)Dsl.ParamClassEnum.PARAM_CLASS_PERIOD: {
                            if (funcData.IsHighOrder) {
                                string objType = TypeInference(funcData.LowerOrderFunction);
                                string mname = funcData.GetParamId(0);
                                return MemberTypeInference(".", objType, string.Empty, mname);
                            }
                            else {
                                var vinfo = GetVarInfo(funcName);
                                if (null != vinfo) {
                                    string objType = vinfo.Type;
                                    string mname = funcData.GetParamId(0);
                                    return MemberTypeInference(".", objType, string.Empty, mname);
                                }
                                else {
                                    string objType = funcName;
                                    string mname = funcData.GetParamId(0);
                                    return MemberTypeInference(".", objType, string.Empty, mname);
                                }
                            }
                        }
                    case (int)Dsl.ParamClassEnum.PARAM_CLASS_BRACKET: {
                            if (funcData.IsHighOrder) {
                                string objType = TypeInference(funcData.LowerOrderFunction);
                                string mname = funcData.GetParamId(0);
                                return MemberTypeInference("[]", objType, string.Empty, mname);
                            }
                            else {
                                var vinfo = GetVarInfo(funcName);
                                if (null != vinfo) {
                                    string objType = vinfo.Type;
                                    string mname = funcData.GetParamId(0);
                                    return MemberTypeInference("[]", objType, string.Empty, mname);
                                }
                                else {
                                    string objType = funcName;
                                    string mname = funcData.GetParamId(0);
                                    return MemberTypeInference("[]", objType, string.Empty, mname);
                                }
                            }
                        }
                    case (int)Dsl.ParamClassEnum.PARAM_CLASS_PARENTHESES: {
                            List<string> argTypes = new List<string>();
                            foreach (var p in funcData.Params) {
                                string type = TypeInference(p);
                                argTypes.Add(type);
                            }
                            if (string.IsNullOrEmpty(funcName)) {
                                return (argTypes.Count > 0 ? argTypes[argTypes.Count - 1] : string.Empty);
                            }
                            else {
                                return FunctionTypeInference(funcName, argTypes, syntax, out FuncInfo? funcInfo);
                            }
                        }
                }
            }
            else if (null != stmData) {
                if (stmData.GetId() == "?") {
                    var tfunc = stmData.First.AsFunction;
                    var ffunc = stmData.Second.AsFunction;
                    Debug.Assert(null != tfunc && null != ffunc);
                    var texp = tfunc.GetParam(0);
                    var fexp = ffunc.GetParam(0);
                    var t1 = TypeInference(texp);
                    var t2 = TypeInference(fexp);
                    if (!string.IsNullOrEmpty(t1))
                        return t1;
                    else if (!string.IsNullOrEmpty(t2))
                        return t2;
                }
            }
            return string.Empty;
        }

        private static string OperatorTypeInference(string op, string opd)
        {
            string resultType = string.Empty;
            if (op == "+" || op == "-" || op == "~")
                resultType = opd;
            else if (op == "!")
                resultType = "bool";
            return resultType;
        }
        private static string OperatorTypeInference(string op, string opd1, string opd2)
        {
            string resultType = string.Empty;
            if (op == "*") {
                if (opd1 == opd2) {
                    resultType = opd1;
                }
                else {
                    return GetMatmulType(opd1, opd2);
                }
            }
            else if (op == "+" || op == "-" || op == "/" || op == "%") {
                if (opd1.StartsWith("mat") || opd1.StartsWith("dmat"))
                    resultType = opd1;
                else if (opd2.StartsWith("mat") || opd2.StartsWith("dmat"))
                    resultType = opd2;
                else if (opd1.StartsWith("vec") || opd1.StartsWith("dvec"))
                    resultType = opd1;
                else if (opd2.StartsWith("vec") || opd2.StartsWith("dvec"))
                    resultType = opd2;
                else
                    resultType = opd1.Length >= opd2.Length ? opd1 : opd2;
            }
            else if (op == "&&" || op == "||" || op == ">=" || op == "==" || op == "!=" || op == "<=" || op == ">" || op == "<") {
                if (opd1.StartsWith("vec") || opd1.StartsWith("dvec"))
                    resultType = "bool" + GetTypeSuffix(opd1);
                else if (opd2.StartsWith("vec") || opd2.StartsWith("dvec"))
                    resultType = "bool" + GetTypeSuffix(opd2);
                else
                    resultType = "bool";
            }
            else if (op == "&" || op == "|" || op == "^" || op == "<<" || op == ">>") {
                if (opd1.StartsWith("vec") || opd1.StartsWith("dvec"))
                    resultType = "int" + GetTypeSuffix(opd1);
                else if (opd2.StartsWith("vec") || opd2.StartsWith("dvec"))
                    resultType = "int" + GetTypeSuffix(opd2);
                else
                    resultType = "int";
            }
            return resultType;
        }
        private static string FunctionTypeInference(string func, IList<string> args, Dsl.ISyntaxComponent syntax, out FuncInfo? funcInfo)
        {
            funcInfo = null;
            string callSig = func + "_" + string.Join("_", args);
            if (s_FuncOverloads.TryGetValue(func, out var overloads)) {
                foreach (var sig in overloads) {
                    if (sig.StartsWith(callSig)) {
                        if (s_FuncInfos.TryGetValue(sig, out var tmpInfo)) {
                            if (args.Count == tmpInfo.Params.Count && sig == callSig || args.Count < tmpInfo.Params.Count && null != tmpInfo.Params[args.Count].DefaultValue) {
                                funcInfo = tmpInfo;
                                return null == funcInfo.RetInfo ? "void" : funcInfo.RetInfo.Type;
                            }
                        }
                    }
                }
                //find nearst match
                int curScore = -1;
                foreach (var sig in overloads) {
                    if (s_FuncInfos.TryGetValue(sig, out var tmpInfo)) {
                        if (IsArgsMatch(args, tmpInfo, out int newScore) && curScore < newScore) {
                            curScore = newScore;
                            funcInfo = tmpInfo;
                        }
                    }
                }
                if (null != funcInfo) {
                    return null == funcInfo.RetInfo ? "void" : funcInfo.RetInfo.Type;
                }
            }
            else {
                //built-in function
                if (s_BuiltInFuncs.TryGetValue(func, out var resultType)) {
                    if (func.StartsWith("texture")) {
                        foreach (var arg in args) {
                            if (arg == "vec3" || arg == "vec4") {
                                Console.WriteLine("func '{0}' use {1} argument, line {2} dsl {3}", func, arg, syntax.GetLine(), syntax.ToScriptString(false, Dsl.DelimiterInfo.Default));
                                break;
                            }
                        }
                    }
                    string ret = GetFuncResultType(resultType, func, args, args);
                    return ret;
                }
                else if (func.StartsWith("glsl_")) {
                    string oriName = func.Substring("glsl_".Length);
                    if (oriName.EndsWith("_ctor"))
                        oriName = oriName.Substring(0, oriName.Length - "_ctor".Length);
                    if (s_BuiltInFuncs.TryGetValue(oriName, out var resultType2)) {
                        string ret = GetFuncResultType(resultType2, oriName, args, args);
                        return ret;
                    }
                    else if (s_StructInfos.TryGetValue(oriName, out var struInfo)) {
                        string ret = oriName;
                        return ret;
                    }
                    else {
                        string ret = oriName;
                        return ret;
                    }
                }
            }
            return string.Empty;
        }
        private static string MemberTypeInference(string op, string objType, string resultType, string memberOrType)
        {
            if (op == ".") {
                if (s_StructInfos.TryGetValue(objType, out var info)) {
                    foreach (var field in info.Fields) {
                        if (field.Name == memberOrType) {
                            resultType = field.Type;
                            break;
                        }
                    }
                }
                else {
                    string baseType = GetTypeRemoveSuffix(objType);
                    string suffix = GetTypeSuffix(objType);
                    if (string.IsNullOrEmpty(resultType)) {
                        if (memberOrType.Length == 1) {
                            string stype = VectorMatrixTypeToScalarType(objType);
                            resultType = stype;
                        }
                        else if (baseType == "mat") {
                            int ct = GetMemberCount(memberOrType);
                            if (ct == 1)
                                resultType = "float";
                            else
                                resultType = "vec" + ct.ToString();
                        }
                        else if (baseType == "dmat") {
                            int ct = GetMemberCount(memberOrType);
                            if (ct == 1)
                                resultType = "double";
                            else
                                resultType = "dvec" + ct.ToString();
                        }
                        else {
                            resultType = baseType + memberOrType.Length.ToString();
                        }
                    }
                }
            }
            else if (op == "[]") {
                string baseType = GetTypeRemoveSuffix(objType, out var suffix, out var arrNums);
                if (arrNums.Count > 0)
                    resultType = baseType + suffix;
                else if (suffix.Length > 0)
                    resultType = baseType;
                else
                    resultType = objType;
            }
            return resultType;
        }
        private static string GetMaxType(params string[] argTypes)
        {
            IList<string> ats = argTypes;
            return GetMaxType(ats);
        }
        private static string GetMaxType(IList<string> argTypes)
        {
            string mat = string.Empty;
            string vec = string.Empty;
            string maxTy = string.Empty;
            string ty0 = argTypes[0];
            if (ty0.StartsWith("mat") || ty0.StartsWith("dmat"))
                mat = ty0;
            else if (ty0.StartsWith("vec") || ty0.StartsWith("dvec"))
                vec = ty0;
            else
                maxTy = ty0;
            for (int i = 1; i < argTypes.Count; ++i) {
                string ty = argTypes[i];
                if (string.IsNullOrEmpty(mat) && (ty.StartsWith("mat") || ty.StartsWith("dmat")))
                    mat = ty;
                else if (string.IsNullOrEmpty(vec) && (ty.StartsWith("vec") || ty.StartsWith("dvec")))
                    vec = ty;
                else
                    maxTy = maxTy.Length >= ty.Length ? maxTy : ty;
            }
            if (!string.IsNullOrEmpty(mat))
                return mat;
            else if (!string.IsNullOrEmpty(vec))
                return vec;
            else
                return maxTy;
        }
        private static string GetMatmulType(string oriTypeA, string oriTypeB)
        {
            string ret;
            string bt1 = GetTypeRemoveSuffix(oriTypeA);
            string s1 = GetTypeSuffix(oriTypeA);
            string bt2 = GetTypeRemoveSuffix(oriTypeB);
            string s2 = GetTypeSuffix(oriTypeB);
            if (s1.Length == 0 && s2.Length == 0)
                ret = GetMaxType(oriTypeA, oriTypeB);
            else if (s1.Length == 0)
                ret = oriTypeB;
            else if (s2.Length == 0)
                ret = oriTypeA;
            else if (s1.Length == 1 && s2.Length == 0)
                ret = oriTypeA;
            else if (s1.Length == 0 && s2.Length == 1)
                ret = oriTypeB;
            else if (bt1 == "vec" && bt2 == "vec")
                return "float";
            else if (bt1 == "dvec" && bt2 == "dvec")
                return "double";
            else if (bt1 == "vec" || bt1 == "dvec")
                return oriTypeA;
            else if (bt2 == "vec" || bt2 == "dvec")
                return oriTypeB;
            else if (s1.Length == 1 && s2.Length == 1) {
                return GetMaxType(oriTypeA, oriTypeB);
            }
            else {
                string mt = GetMaxType(bt1, bt2);
                ret = mt + s1[0] + "x" + s2[2];
            }
            return ret;
        }
        private static bool IsArgsMatch(IList<string> args, FuncInfo funcInfo, out int score)
        {
            bool ret = false;
            score = 0;
            if (args.Count <= funcInfo.Params.Count) {
                ret = true;
                for (int ix = 0; ix < args.Count; ++ix) {
                    int argScore;
                    if (!IsTypeMatch(args[ix], funcInfo.Params[ix].Type, out argScore)) {
                        ret = false;
                        break;
                    }
                    score += argScore;
                }
                if (ret && args.Count < funcInfo.Params.Count) {
                    if (null == funcInfo.Params[args.Count].DefaultValue)
                        ret = false;
                }
            }
            return ret;
        }
        private static bool IsTypeMatch(string argType, string paramType, out int score)
        {
            bool ret = false;
            score = 0;
            if (argType == paramType) {
                score = 2;
                ret = true;
            }
            else if ((argType == "bool" || argType == "int" || argType == "uint" || argType == "float" || argType == "double")
                && (paramType == "bool" || paramType == "int" || paramType == "uint" || paramType == "float" || paramType == "double")) {
                score = 1;
                ret = true;
            }
            else if ((argType == "bvec2" || argType == "ivec2" || argType == "uvec3" || argType == "vec2" || argType == "dvec2")
                && (paramType == "bvec2" || paramType == "ivec2" || paramType == "uvec2" || paramType == "vec2" || paramType == "dvec2")) {
                score = 1;
                ret = true;
            }
            else if ((argType == "bvec3" || argType == "ivec3" || argType == "uvec3" || argType == "vec3" || argType == "dvec3")
                && (paramType == "bvec3" || paramType == "ivec3" || paramType == "uvec3" || paramType == "vec3" || paramType == "dvec3")) {
                score = 1;
                ret = true;
            }
            else if ((argType == "bvec4" || argType == "ivec4" || argType == "uvec4" || argType == "vec4" || argType == "dvec4")
                && (paramType == "bvec4" || paramType == "ivec4" || paramType == "uvec4" || paramType == "vec4" || paramType == "dvec4")) {
                score = 1;
                ret = true;
            }
            else if ((argType == "mat2" || argType == "dmat2") || (paramType == "mat2" || paramType == "dmat2")) {
                score = 1;
                ret = true;
            }
            else if ((argType == "mat3" || argType == "dmat3") || (paramType == "mat3" || paramType == "dmat3")) {
                score = 1;
                ret = true;
            }
            else if ((argType == "mat4" || argType == "dmat4") || (paramType == "mat4" || paramType == "dmat4")) {
                score = 1;
                ret = true;
            }
            else if (argType == "bool" || argType == "bvec2" || argType == "bvec3" || argType == "bvec4") {
                ret = true;
            }
            else if (argType == "int" || argType == "ivec2" || argType == "ivec3" || argType == "ivec4") {
                ret = true;
            }
            else if (argType == "uint" || argType == "uvec2" || argType == "uvec3" || argType == "uvec4") {
                ret = true;
            }
            else if (argType == "float" || argType == "vec2" || argType == "vec3" || argType == "vec4") {
                ret = true;
            }
            else if (argType == "double" || argType == "dvec2" || argType == "dvec3" || argType == "dvec4") {
                ret = true;
            }
            return ret;
        }

        //function signature and type encoding
        private static string GetFullTypeFuncSig(string funcName, IList<string> argTypes)
        {
            var sb = new StringBuilder();
            sb.Append(funcName);
            foreach (var t in argTypes) {
                sb.Append("_");
                sb.Append(t);
            }
            return sb.ToString();
        }
        private static string GetFuncResultType(string resultTypeTag, string funcOrObjType, IList<string> args, IList<string> oriArgs)
        {
            string ret;
            if (resultTypeTag == "@@")
                ret = funcOrObjType;
            else if (resultTypeTag == "@0-$0")
                ret = GetTypeRemoveSuffix(args[0]);
            else if (resultTypeTag == "@0")
                ret = args[0];
            else if (resultTypeTag == "@m") {
                ret = GetMaxType(oriArgs);
            }
            else if (!resultTypeTag.Contains('@') && !resultTypeTag.Contains('$'))
                ret = resultTypeTag;
            else {
                string rt = resultTypeTag.Replace("@@", funcOrObjType).Replace("@0-$0", GetTypeRemoveSuffix(args[0])).Replace("@0", args[0]).Replace("$0", GetTypeSuffix(args[0])).Replace("$R0", GetTypeSuffixReverse(args[0]));
                if (args.Count > 1) {
                    rt = rt.Replace("@1-$1", GetTypeRemoveSuffix(args[1])).Replace("@1", args[1]).Replace("$1", GetTypeSuffix(args[1])).Replace("$R1", GetTypeSuffixReverse(args[1]));
                }
                if (rt.Contains("m")) {
                    string mt = GetMaxType(oriArgs);
                    rt = rt.Replace("@m-$m", GetTypeRemoveSuffix(mt)).Replace("@m", mt).Replace("$m", GetTypeSuffix(mt)).Replace("$Rm", GetTypeSuffixReverse(mt));
                }
                ret = rt;
            }
            return ret;
        }
        private static int GetMemberCount(string member)
        {
            int ct = 0;
            foreach (char c in member) {
                if (c == '_')
                    ++ct;
            }
            return ct;
        }

        internal static string GetTypeRemoveSuffix(string type)
        {
            return GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
        }
        internal static string GetTypeRemoveSuffix(string type, out string suffix, out IList<int> arrNums)
        {
            type = GetTypeRemoveArrTag(type, out arrNums);
            if (type.Length >= 3) {
                char last = type[type.Length - 1];
                if (last == '2' || last == '3' || last == '4') {
                    string last3 = type.Substring(type.Length - 3);
                    if (last3 == "2x2" || last3 == "3x3" || last3 == "4x4") {
                        suffix = last3;
                        return type.Substring(0, type.Length - 3);
                    }
                    else if (last3 == "2x3" || last3 == "3x2" || last3 == "3x4" || last3 == "4x3" || last3 == "2x4" || last3 == "4x2") {
                        suffix = last3;
                        return type.Substring(0, type.Length - 3);
                    }
                    else {
                        suffix = last.ToString();
                        return type.Substring(0, type.Length - 1);
                    }
                }
            }
            suffix = string.Empty;
            return type;
        }
        internal static string GetTypeSuffix(string type)
        {
            return GetTypeSuffix(type, out var arrNums);
        }
        internal static string GetTypeSuffix(string type, out IList<int> arrNums)
        {
            type = GetTypeRemoveArrTag(type, out arrNums);
            if (type.Length >= 3) {
                char last = type[type.Length - 1];
                if (last == '2' || last == '3' || last == '4') {
                    string last3 = type.Substring(type.Length - 3);
                    if (last3 == "2x2" || last3 == "3x3" || last3 == "4x4") {
                        return last3;
                    }
                    else if (last3 == "2x3" || last3 == "3x2" || last3 == "3x4" || last3 == "4x3" || last3 == "2x4" || last3 == "4x2") {
                        return last3;
                    }
                    else {
                        return last.ToString();
                    }
                }
            }
            return string.Empty;
        }
        internal static string GetTypeSuffixReverse(string type)
        {
            return GetTypeSuffixReverse(type, out var arrNums);
        }
        internal static string GetTypeSuffixReverse(string type, out IList<int> arrNums)
        {
            type = GetTypeRemoveArrTag(type, out arrNums);
            if (type.Length >= 3) {
                char last = type[type.Length - 1];
                if (last == '2' || last == '3' || last == '4') {
                    string last3 = type.Substring(type.Length - 3);
                    if (last3 == "2x2" || last3 == "3x3" || last3 == "4x4") {
                        return last3;
                    }
                    else if (last3 == "2x3" || last3 == "3x2" || last3 == "3x4" || last3 == "4x3" || last3 == "2x4" || last3 == "4x2") {
                        return new string(new char[] { last3[2], last3[1], last3[0] });
                    }
                    else {
                        return last.ToString();
                    }
                }
            }
            return string.Empty;
        }
        internal static string GetTypeRemoveArrTag(string type, out IList<int> arrNums)
        {
            var list = new List<int>();
            var r = GetTypeRemoveArrTagRecursively(type, list);
            arrNums = list;
            return r;
        }
        internal static string GetTypeRemoveArrTagRecursively(string type, List<int> arrNums)
        {
            int st = type.LastIndexOf("_x");
            if (st > 0) {
                string arrNumStr = type.Substring(st + 2);
                if (int.TryParse(arrNumStr, out int arrNum)) {
                    arrNums.Add(arrNum);
                    type = GetTypeRemoveArrTagRecursively(type.Substring(0, st), arrNums);
                }
            }
            return type;
        }
        internal static string VectorMatrixTypeToScalarType(string vm)
        {
            if (vm.StartsWith("vec"))
                return "float";
            else if (vm.StartsWith("ivec"))
                return "int";
            else if (vm.StartsWith("uvec"))
                return "uint";
            else if (vm.StartsWith("dvec"))
                return "double";
            else if (vm.StartsWith("bvec"))
                return "bool";
            else if (vm.StartsWith("mat"))
                return "float";
            else if (vm.StartsWith("dmat"))
                return "double";
            else
                return GetTypeRemoveSuffix(vm);
        }
        internal static bool IsPhiVar(string vname)
        {
            return vname.Contains(c_PhiTagSeparator);
        }

        private static Dictionary<string, VarAliasInfo> CloneVarAliasInfos(Dictionary<string, VarAliasInfo> infos)
        {
            var newInfos = new Dictionary<string, VarAliasInfo>();
            foreach (var pair in infos) {
                newInfos.Add(pair.Key, pair.Value.Clone());
            }
            return newInfos;
        }
        private static void AddFuncToComputeGraph(FuncInfo funcInfo)
        {
            var graph = funcInfo.FuncComputeGraph;
            foreach (var p in funcInfo.Params) {
                var vgn = new ComputeGraphVarNode(funcInfo, p.Type, p.Name);
                vgn.IsInOut = p.IsInOut;
                vgn.IsOut = p.IsOut;
                vgn.IsParam = true;

                if (graph.VarNodes.TryGetValue(vgn.VarName, out var node)) {
                    node.Type = vgn.Type;
                    node.IsOut = vgn.IsOut;
                    node.IsInOut = vgn.IsInOut;
                }
                else {
                    graph.VarNodes.Add(vgn.VarName, vgn);
                }
            }
            graph.RootNodes.Add(funcInfo.ComputeGraphFunc);
            funcInfo.ComputeGraphFunc.OwnFunc = funcInfo;
            funcInfo.ComputeGraphFunc.Block = new ComputeGraphBlock(funcInfo);
            SetCurBlock(funcInfo.ComputeGraphFunc.Block);
        }

        //compute graph
        private static ComputeGraph CurComputeGraph()
        {
            var curFunc = CurFuncInfo();
            if (null != curFunc)
                return curFunc.FuncComputeGraph;
            else
                return s_GlobalComputeGraph;
        }
        private static void AddComputeGraphRootNode(ComputeGraphNode gn)
        {
            var block = CurBlock();
            if (null != block) {
                block.AddChild(gn);
            }
            else {
                var cg = CurComputeGraph();
                cg.RootNodes.Add(gn);
            }
        }
        private static void AddComputeGraphVarNode(ComputeGraphVarNode cgvn)
        {
            var cg = CurComputeGraph();
            if (cg.VarNodes.TryGetValue(cgvn.VarName, out var node)) {
                node.Type = cgvn.Type;
                node.IsOut = cgvn.IsOut;
                node.IsInOut = cgvn.IsInOut;
            }
            else {
                cg.VarNodes.Add(cgvn.VarName, cgvn);
            }

            string name = cgvn.VarName;
            string type = cgvn.Type;
            VariableTable.AllocVar(name, type);
        }
        private static ComputeGraphVarNode? FindComputeGraphVarNode(string name)
        {
            ComputeGraphVarNode? ret = null;
            var cg = CurComputeGraph();
            if (cg.VarNodes.TryGetValue(name, out var node)) {
                ret = node;
            }
            else if (s_GlobalComputeGraph != cg) {
                if (s_GlobalComputeGraph.VarNodes.TryGetValue(name, out var gnode))
                    ret = gnode;
            }
            return ret;
        }

        //program structure
        private static void AddVar(VarInfo varInfo)
        {
            if (!s_VarInfos.TryGetValue(varInfo.Name, out var varInfos)) {
                varInfos = new Dictionary<int, VarInfo>();
                s_VarInfos.Add(varInfo.Name, varInfos);
            }
            varInfo.OwnFunc = CurFuncInfo();
            varInfos[CurBlockId()] = varInfo;
            SetLastVarType(varInfo);

            var cgvn = new ComputeGraphVarNode(varInfo.OwnFunc, Config.TryReplaceType(varInfo.Name, varInfo.Type), varInfo.Name);
            AddComputeGraphVarNode(cgvn);

            //Only variables that are already in the list of used variables are recorded with their types.
            if (Config.ActiveConfig.SettingInfo.UsedVariables.ContainsKey(varInfo.Name)) {
                string type = varInfo.Type;
                if (varInfo.Modifiers.Contains("precise"))
                    type = "precise " + type;
                Config.ActiveConfig.SettingInfo.SetUsedVariableType(varInfo.Name, type);
            }
        }
        private static FuncInfo? CurFuncInfo()
        {
            FuncInfo? curFuncInfo = null;
            if (s_FuncParseStack.Count > 0) {
                curFuncInfo = s_FuncParseStack.Peek();
            }
            return curFuncInfo;
        }
        private static void PushFuncInfo(FuncInfo funcInfo)
        {
            s_FuncParseStack.Push(funcInfo);
        }
        private static void PopFuncInfo()
        {
            s_FuncParseStack.Pop();
        }
        private static void AddParamInfo(VarInfo varInfo)
        {
            var funcInfo = CurFuncInfo();
            if (null != funcInfo) {
                funcInfo.Params.Add(varInfo);
                if (varInfo.IsInOut || varInfo.IsOut) {
                    funcInfo.HasInOutOrOutParams = true;
                    funcInfo.InOutOrOutParams.Add(varInfo);
                }
            }
        }
        private static void SetRetInfo(VarInfo varInfo)
        {
            var funcInfo = CurFuncInfo();
            if (null != funcInfo) {
                funcInfo.RetInfo = varInfo;
            }
        }
        private static void PushBlock(bool isBranch, bool isLoop)
        {
            ++s_LastBlockId;
            Dictionary<string, VarAliasInfo>? varAliasInfo = null;
            if (s_LexicalScopeStack.Count > 0) {
                var curInfo = s_LexicalScopeStack.Peek();
                Debug.Assert(null != curInfo.VarAliasInfos);
                varAliasInfo = CloneVarAliasInfos(curInfo.VarAliasInfos);
            }
            else {
                Debug.Assert(null != s_ToplevelLexicalScopeInfo.VarAliasInfos);
                varAliasInfo = CloneVarAliasInfos(s_ToplevelLexicalScopeInfo.VarAliasInfos);
            }
            s_LexicalScopeStack.Push(new LexicalScopeInfo { IsBranch = isBranch, IsLoop = isLoop, BlockId = s_LastBlockId, LastVarType = null, VarAliasInfos = varAliasInfo });
        }
        private static void PopBlock()
        {
            var lastInfo = s_LexicalScopeStack.Pop();
            if (s_LexicalScopeStack.Count > 0) {
                var curInfo = s_LexicalScopeStack.Peek();
                var setVars = curInfo.SetVars;
                foreach (var key in lastInfo.SetVars) {
                    if (!setVars.Contains(key)) {
                        setVars.Add(key);
                    }
                }
                var undeterminedVarAliasTable = curInfo.UndeterminedVarAliasTable;
                foreach (var pair in lastInfo.UndeterminedVarAliasTable) {
                    if (undeterminedVarAliasTable.TryGetValue(pair.Key, out var info)) {
                        info.ValueDatas.AddRange(pair.Value.ValueDatas);
                    }
                    else {
                        undeterminedVarAliasTable.Add(pair.Key, pair.Value);
                    }
                }
                //If it is not a branch block, the alias information is inherited by the upper block.
                if (!lastInfo.IsBranch) {
                    var varAliasInfos = curInfo.VarAliasInfos;
                    Debug.Assert(null != varAliasInfos);
                    Debug.Assert(null != lastInfo.VarAliasInfos);
                    foreach (var pair in lastInfo.VarAliasInfos) {
                        varAliasInfos[pair.Key] = pair.Value;
                    }
                }
            }
            else {
                var setVars = s_ToplevelLexicalScopeInfo.SetVars;
                foreach (var key in lastInfo.SetVars) {
                    if (!setVars.Contains(key)) {
                        setVars.Add(key);
                    }
                }
                var undeterminedVarAliasTable = s_ToplevelLexicalScopeInfo.UndeterminedVarAliasTable;
                foreach (var pair in lastInfo.UndeterminedVarAliasTable) {
                    if (undeterminedVarAliasTable.TryGetValue(pair.Key, out var info)) {
                        info.ValueDatas.AddRange(pair.Value.ValueDatas);
                    }
                    else {
                        undeterminedVarAliasTable.Add(pair.Key, pair.Value);
                    }
                }
            }
        }
        private static int CurBlockId()
        {
            if (s_LexicalScopeStack.Count > 0) {
                return s_LexicalScopeStack.Peek().BlockId;
            }
            return s_ToplevelLexicalScopeInfo.BlockId;
        }
        private static void SetCurBlock(ComputeGraphBlock block)
        {
            if (s_LexicalScopeStack.Count > 0) {
                s_LexicalScopeStack.Peek().Block = block;
            }
        }
        private static ComputeGraphBlock? CurBlock()
        {
            if (s_LexicalScopeStack.Count > 0) {
                return s_LexicalScopeStack.Peek().Block;
            }
            return s_ToplevelLexicalScopeInfo.Block;
        }
        private static HashSet<string> CurSetVars()
        {
            if (s_LexicalScopeStack.Count > 0) {
                return s_LexicalScopeStack.Peek().SetVars;
            }
            return s_ToplevelLexicalScopeInfo.SetVars;
        }
        private static HashSet<string> CurLoopSetVars()
        {
            if (s_LexicalScopeStack.Count > 0) {
                HashSet<string> result = new HashSet<string>();
                foreach (var info in s_LexicalScopeStack) {
                    foreach (var key in info.SetVars) {
                        if (!result.Contains(key))
                            result.Add(key);
                    }
                    if (info.IsLoop)
                        break;
                }
                return result;
            }
            return s_ToplevelLexicalScopeInfo.SetVars;
        }
        private static Dictionary<string, UndeterminedVarAlias> CurUndeterminedVarAliasTable()
        {
            if (s_LexicalScopeStack.Count > 0) {
                return s_LexicalScopeStack.Peek().UndeterminedVarAliasTable;
            }
            return s_ToplevelLexicalScopeInfo.UndeterminedVarAliasTable;
        }
        private static Dictionary<string, VarAliasInfo> CurVarAliasInfos()
        {
            if (s_LexicalScopeStack.Count > 0) {
                var infos = s_LexicalScopeStack.Peek().VarAliasInfos;
                Debug.Assert(null != infos);
                return infos;
            }
            var toplevelInfos = s_ToplevelLexicalScopeInfo.VarAliasInfos;
            Debug.Assert(null != toplevelInfos);
            return toplevelInfos;
        }
        private static void TryAddUndeterminedVarAlias(string vname, Dsl.ValueData valData)
        {
            var vinfo = GetVarInfo(vname);
            if (null != vinfo && null == vinfo.OwnFunc) {
                //Global variables do not need to be renamed.
                return;
            }
            var curLoopSetVars = CurLoopSetVars();
            if (!curLoopSetVars.Contains(vname)) {
                var curUndeterminedVarAliasTable = CurUndeterminedVarAliasTable();
                if (curUndeterminedVarAliasTable.TryGetValue(vname, out var info)) {
                    if (!info.ValueDatas.Contains(valData))
                        info.ValueDatas.Add(valData);
                }
                else {
                    info = new UndeterminedVarAlias();
                    info.ValueDatas.Add(valData);
                    curUndeterminedVarAliasTable.Add(vname, info);
                }
            }
        }
        private static void SetCurBlockPhiSuffix(string suffix)
        {
            if (s_LexicalScopeStack.Count > 0) {
                s_LexicalScopeStack.Peek().PhiSuffix = suffix;
            }
            else {
                s_ToplevelLexicalScopeInfo.PhiSuffix = suffix;
            }
        }
        private static string GetCurBlockPhiSuffix()
        {
            if (s_LexicalScopeStack.Count > 0) {
                return s_LexicalScopeStack.Peek().PhiSuffix;
            }
            else {
                return s_ToplevelLexicalScopeInfo.PhiSuffix;
            }
        }
        private static string GetCurLoopBlockPhiSuffix()
        {
            if (s_LexicalScopeStack.Count > 0) {
                foreach (var info in s_LexicalScopeStack) {
                    if (info.IsLoop)
                        return info.PhiSuffix;
                }
            }
            return string.Empty;
        }
        private static void SetLastVarType(VarInfo info)
        {
            if (s_LexicalScopeStack.Count > 0) {
                s_LexicalScopeStack.Peek().LastVarType = info;
            }
            else {
                s_ToplevelLexicalScopeInfo.LastVarType = info;
            }
        }
        private static VarInfo? GetLastVarType()
        {
            if (s_LexicalScopeStack.Count > 0) {
                return s_LexicalScopeStack.Peek().LastVarType;
            }
            else {
                return s_ToplevelLexicalScopeInfo.LastVarType;
            }
        }
        private static VarInfo? GetVarInfo(string name)
        {
            VarInfo? varInfo = null;
            if (s_VarInfos.TryGetValue(name, out var varInfos)) {
                bool find = false;
                foreach (var scopeInfo in s_LexicalScopeStack) {
                    if (varInfos.TryGetValue(scopeInfo.BlockId, out varInfo)) {
                        find = true;
                        break;
                    }
                }
                if (!find) {
                    find = varInfos.TryGetValue(0, out varInfo);
                }
            }
            if (null == varInfo) {
                var curFunc = CurFuncInfo();
                if (null != curFunc) {
                    foreach (var p in curFunc.Params) {
                        if (name == p.Name) {
                            varInfo = p;
                            break;
                        }
                    }
                }
            }
            return varInfo;
        }

        private static bool TryGetVarAliasIndex(string name, out int index)
        {
            bool ret;
            if (s_VarAliasIndexes.TryGetValue(name, out var ix)) {
                index = ix.Index;
                ret = true;
            }
            else {
                index = 0;
                ret = false;
            }
            return ret;
        }
        private static int IncVarAliasIndex(string name)
        {
            if (s_VarAliasIndexes.TryGetValue(name, out var ix)) {
                ++ix.Index;
            }
            else {
                ix = new VarAliasIndex { Index = 0 };
                s_VarAliasIndexes.Add(name, ix);
            }
            return ix.Index;
        }

        internal static int GenUniqueNumber()
        {
            return ++s_UniqueNumber;
        }
        internal static string GetIndentString(int indent)
        {
            const string c_IndentString = "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t";
            return c_IndentString.Substring(0, indent);
        }
        internal static void Reset()
        {
            ComputeGraph.ResetStatic();
            s_GlobalComputeGraph = new ComputeGraph();
            s_ExpressionBuilder.Length = 0;
            s_ExpressionList.Clear();
            s_Expression2VarList.Clear();
            s_DuplicateVars.Clear();
            s_Indent = 1;

            s_CondExpStack.Clear();
            s_StructInfos.Clear();
            s_BufferInfos.Clear();
            s_ArrayInits.Clear();

            s_VarAliasInfoUpdateQueue.Clear();
            s_VarAliasIndexes.Clear();

            s_VarInfos.Clear();
            s_LexicalScopeStack.Clear();
            s_ToplevelLexicalScopeInfo = new LexicalScopeInfo { VarAliasInfos = new Dictionary<string, VarAliasInfo>() };
            s_LastBlockId = 0;
            s_UniqueNumber = 0;

            s_FuncInfos.Clear();
            s_FuncOverloads.Clear();
            s_FuncParseStack.Clear();
        }

        public sealed class VarInfo
        {
            public string Name = string.Empty;
            public string Type = string.Empty;
            public bool IsInOut = false;
            public bool IsOut = false;
            public List<string> Modifiers = new List<string>();
            public Dsl.ISyntaxComponent? DefaultValue = null;
            public FuncInfo? OwnFunc = null;

            public void CopyFrom(VarInfo other)
            {
                Name = other.Name;
                Type = other.Type;
                IsInOut = other.IsInOut;
                IsOut = other.IsOut;
                Modifiers.AddRange(other.Modifiers);
                DefaultValue = other.DefaultValue;
            }
            public string CalcTypeString()
            {
                return (Modifiers.Count > 0 ? string.Join(' ', Modifiers) + " " : string.Empty) + Type;
            }
        }
        public sealed class FuncInfo
        {
            public string Name = string.Empty;
            public string Signature = string.Empty;
            public bool HasInOutOrOutParams = false;
            public List<VarInfo> Params = new List<VarInfo>();
            public VarInfo? RetInfo = null;
            public List<VarInfo> InOutOrOutParams = new List<VarInfo>();

            public ComputeGraph FuncComputeGraph = new ComputeGraph();
            public ComputeGraphFunction ComputeGraphFunc = new ComputeGraphFunction(null);

            public bool IsVoid()
            {
                return null == RetInfo || RetInfo.Type == "void";
            }
        }
        public sealed class StructInfo
        {
            public string Name = string.Empty;
            public List<VarInfo> Fields = new List<VarInfo>();
        }
        public sealed class BufferInfo
        {
            public string Name = string.Empty;
            public string Layout = string.Empty;
            public string instName = string.Empty;
            public List<VarInfo> Variables = new List<VarInfo>();
        }
        public sealed class ArrayInitInfo
        {
            public string Type = string.Empty;
            public int Size = 0;
        }
        public enum CondExpEnum
        {
            Question = 0,
            Colon,
        }
        public sealed class CondExpInfo
        {
            public CondExpInfo(CondExpEnum part)
            {
                m_CondExpPart = part;
                m_ParenthesesCount = 0;
            }
            public void IncParenthesesCount()
            {
                ++m_ParenthesesCount;
            }
            public void DecParenthesesCount()
            {
                --m_ParenthesesCount;
            }
            public bool MaybeCompletePart(CondExpEnum part)
            {
                return m_CondExpPart == part && m_ParenthesesCount == 0;
            }

            private CondExpEnum m_CondExpPart;
            private int m_ParenthesesCount;
        }

        public sealed class VarSyntaxInfo
        {
            public List<Dsl.ValueData> Vars = new List<Dsl.ValueData>();
        }
        public sealed class VarAliasInfo
        {
            public string AliasSuffix = string.Empty;

            public VarAliasInfo Clone()
            {
                return new VarAliasInfo { AliasSuffix = AliasSuffix };
            }
        }
        private sealed class VarAliasIndex
        {
            internal int Index = 0;
        }
        public sealed class UndeterminedVarAlias
        {
            public List<Dsl.ValueData> ValueDatas = new List<ValueData>();
        }
        public sealed class LexicalScopeInfo
        {
            public bool IsBranch = false;
            public bool IsLoop = false;
            public int BlockId = 0;
            public ComputeGraphBlock? Block = null;
            public string PhiSuffix = string.Empty;
            public VarInfo? LastVarType = null;
            public HashSet<string> SetVars = new HashSet<string>();
            public Dictionary<string, UndeterminedVarAlias> UndeterminedVarAliasTable = new Dictionary<string, UndeterminedVarAlias>();

            public Dictionary<string, VarAliasInfo>? VarAliasInfos = null;
        }
        public struct SemanticInfo
        {
            public string ResultType = string.Empty;

            public ComputeGraphNode? GraphNode = null;

            public SemanticInfo()
            {
                Reset();
            }
            public void Reset()
            {
                ResultType = string.Empty;
                GraphNode = null;
            }
            public void CopyResultFrom(SemanticInfo other)
            {
                ResultType = other.ResultType;
                GraphNode = other.GraphNode;
            }
        }

        internal static ComputeGraph s_GlobalComputeGraph = new ComputeGraph();
        internal static StringBuilder s_ExpressionBuilder = new StringBuilder();
        internal static List<Tuple<string, string, int>> s_ExpressionList = new List<Tuple<string, string, int>>();
        internal static Dictionary<int, Dictionary<string, List<Tuple<string, string>>>> s_Expression2VarList = new Dictionary<int, Dictionary<string, List<Tuple<string, string>>>>();
        internal static Dictionary<int, Dictionary<string, Tuple<string, Regex>>> s_DuplicateVars = new Dictionary<int, Dictionary<string, Tuple<string, Regex>>>();
        internal static int s_Indent = 1;

        private static Stack<CondExpInfo> s_CondExpStack = new Stack<CondExpInfo>();
        private static SortedDictionary<string, StructInfo> s_StructInfos = new SortedDictionary<string, StructInfo>();
        private static SortedDictionary<string, BufferInfo> s_BufferInfos = new SortedDictionary<string, BufferInfo>();
        private static SortedDictionary<string, ArrayInitInfo> s_ArrayInits = new SortedDictionary<string, ArrayInitInfo>();

        //The assignment of aliases to variables on the left side of an assignment statement needs to be performed after the current statement is processed
        //(there may be variables with the same name on the right side of the statement, and the alias of the referenced variable should be the alias
        //determined before assignment at this time).
        //This queue is used for this purpose.
        private static Queue<string> s_VarAliasInfoUpdateQueue = new Queue<string>();
        private static Dictionary<string, VarAliasIndex> s_VarAliasIndexes = new Dictionary<string, VarAliasIndex>();

        private static Dictionary<string, Dictionary<int, VarInfo>> s_VarInfos = new Dictionary<string, Dictionary<int, VarInfo>>();
        private static Stack<LexicalScopeInfo> s_LexicalScopeStack = new Stack<LexicalScopeInfo>();
        private static LexicalScopeInfo s_ToplevelLexicalScopeInfo = new LexicalScopeInfo { VarAliasInfos = new Dictionary<string, VarAliasInfo>() };
        private static int s_LastBlockId = 0;
        private static int s_UniqueNumber = 0;

        private static Dictionary<string, FuncInfo> s_FuncInfos = new Dictionary<string, FuncInfo>();
        private static Dictionary<string, HashSet<string>> s_FuncOverloads = new Dictionary<string, HashSet<string>>();
        private static Stack<FuncInfo> s_FuncParseStack = new Stack<FuncInfo>();

        internal static string s_SrcFileForDSL = string.Empty;
        internal static bool s_IsVsShader = false;
        internal static bool s_IsPsShader = true;
        internal static bool s_IsCsShader = false;
        internal static bool s_InteractiveComputing = false;
        internal static bool s_DoReplacement = false;
        internal static bool s_SSA = true;
        internal const string c_PhiTagSeparator = "_phi_";
        internal const string c_AliasSeparator = "_";
        internal const string c_CommentTag = "@comment";

        private static char[] s_eOrE = new char[] { 'e', 'E' };
        private static HashSet<string> s_VertExts = new HashSet<string> { ".vert", ".vs", ".vsh" };
        private static HashSet<string> s_FragExts = new HashSet<string> { ".frag", ".fs", ".fsh" };
        private static HashSet<string> s_CompExts = new HashSet<string> { ".comp", ".cs", ".csh" };
        private static HashSet<string> s_GeomExts = new HashSet<string> { ".geom", ".gs", ".gsh" };
        private static HashSet<string> s_TessCtrlExts = new HashSet<string> { ".tesc", ".tcs" };
        private static HashSet<string> s_TessEvalExts = new HashSet<string> { ".tese", ".tes" };

        private static Dictionary<string, string> s_BuiltInFuncs = new Dictionary<string, string> {
            { "float", "@@" },
            { "vec2", "@@" },
            { "vec3", "@@" },
            { "vec4", "@@" },
            { "double", "@@" },
            { "dvec2", "@@" },
            { "dvec3", "@@" },
            { "dvec4", "@@" },
            { "uint", "@@" },
            { "uvec2", "@@" },
            { "uvec3", "@@" },
            { "uvec4", "@@" },
            { "int", "@@" },
            { "ivec2", "@@" },
            { "ivec3", "@@" },
            { "ivec4", "@@" },
            { "bool", "@@" },
            { "bvec2", "@@" },
            { "bvec3", "@@" },
            { "bvec4", "@@" },
            { "mat2", "@@" },
            { "mat3", "@@" },
            { "mat4", "@@" },
            { "dmat2", "@@" },
            { "dmat3", "@@" },
            { "dmat4", "@@" },
            { "radians", "@0" },
            { "degrees", "@0" },
            { "sin", "@0" },
            { "cos", "@0" },
            { "tan", "@0" },
            { "asin", "@0" },
            { "acos", "@0" },
            { "atan", "@0" },
            { "sinh", "@0" },
            { "cosh", "@0" },
            { "tanh", "@0" },
            { "asinh", "@0" },
            { "acosh", "@0" },
            { "atanh", "@0" },
            { "pow", "@0" },
            { "exp", "@0" },
            { "log", "@0" },
            { "exp2", "@0" },
            { "log2", "@0" },
            { "sqrt", "@0" },
            { "inversesqrt", "@0" },
            { "abs", "@0" },
            { "sign", "@0" },
            { "floor", "@0" },
            { "trunc", "@0" },
            { "round", "@0" },
            { "roundEven", "@0" },
            { "ceil", "@0" },
            { "fract", "@0" },
            { "mod", "@0" },
            { "modf", "@0" },
            { "min", "@0" },
            { "max", "@0" },
            { "clamp", "@m" },
            { "mix", "@m" },
            { "step", "@m" },
            { "smoothstep", "@m" },
            { "isnan", "bool" },
            { "isinf", "bool" },
            { "floatBitsToInt", "int" },
            { "floatBitsToUint", "uint" },
            { "intBitsToFloat", "float" },
            { "uintBitsToFloat", "float" },
            { "fma", "float" },
            { "frexp", "float" },
            { "ldexp", "float" },
            { "packUnorm2x16", "uint" },
            { "packSnorm2x16", "uint" },
            { "packUnorm4x8", "uint" },
            { "packSnorm4x8", "uint" },
            { "unpackUnorm2x16", "vec2" },
            { "unpackSnorm2x16", "vec2" },
            { "unpackUnorm4x8", "vec4" },
            { "unpackSnorm4x8", "vec4" },
            { "packHalf2x16", "uint" },
            { "unpackHalf2x16", "vec2" },
            { "length", "float" },
            { "distance", "float" },
            { "dot", "float" },
            { "cross", "vec3" },
            { "normalize", "@0" },
            { "faceforward", "float" },
            { "reflect", "float" },
            { "refract", "float" },
            { "matrixCompMult", "@0" },
            { "outerProduct", "mat$1x$0" },
            { "transpose", "mat$R0" },
            { "determinant", "float" },
            { "inverse", "@0" },
            { "lessThan", "bvec" },
            { "lessThanEqual", "bvec" },
            { "greaterThan", "bvec" },
            { "greaterThanEqual", "bvec" },
            { "equal", "bvec" },
            { "notEqual", "bvec" },
            { "any", "bool" },
            { "all", "bool"},
            { "not", "bvec"},
            { "uaddCarry", "@0" },
            { "usubBorrow", "@0" },
            { "umulExtended", "void" },
            { "imulExtended", "void" },
            { "bitfieldExtrace", "@0" },
            { "bitfieldInsert", "@0" },
            { "bitfieldReverse", "@0" },
            { "bitCount", "@0" },
            { "findLSB", "@0" },
            { "findMSB", "@0" },
            { "textureSize", "ivec$0" },
            { "texture", "vec$0" },
            { "textureProj", "vec$0" },
            { "textureLod", "vec$0" },
            { "textureOffset", "vec$0" },
            { "texelFetch", "vec$0" },
            { "texelFetchOffset", "vec$0" },
            { "textureProjOffset", "vec$0" },
            { "textureLodOffset", "vec$0" },
            { "textureProjLod", "vec$0" },
            { "textureProjLodOffset", "vec$0" },
            { "textureGrad", "vec$0" },
            { "textureGradOffset", "vec$0" },
            { "textureProjGrad", "vec$0" },
            { "textureProjGradOffset", "vec$0" },
            { "textureGather", "vec$0" },
            { "textureGatherOffset", "vec$0" },
            { "atomicCounterIncrement", "uint" },
            { "atomicCounterDecrement", "uint" },
            { "atomicCounter", "uint" },
            { "atomicAdd", "@0" },
            { "atomicMin", "@0" },
            { "atomicMax", "@0" },
            { "atomicAnd", "@0" },
            { "atomicOr", "@0" },
            { "atomicXor", "@0" },
            { "atomicExchange", "@0" },
            { "aotmicCompSwap", "@0" },
            { "imageSize", "@0" },
            { "imageLoad", "vec4" },
            { "imageStore", "void" },
            { "imageAtomicAdd", "uint" },
            { "imageAtomicMin", "@0" },
            { "imageAtomicMax", "@0" },
            { "imageAtomicAnd", "@0" },
            { "imageAtomicOr", "@0" },
            { "imageAtomicXor", "@0" },
            { "imageAtomicExchange", "@0" },
            { "imageAtomicCompSwap", "@0" },
            { "dFdx", "@0" },
            { "dFdy", "@0" },
            { "fwidth", "@0" },
            { "interpolateAtCentroid", "@0" },
            { "interpolateAtSample", "@0" },
            { "interpolateAtOffset", "@0" },
            { "barrier", "void" },
            { "memoryBarrier", "void" },
            { "memoryBarrierAtomicCounter", "void" },
            { "memoryBarrierBuffer", "void" },
            { "memoryBarrierShared", "void" },
            { "memoryBarrierImage", "void" },
            { "groupMemoryBarrier", "void" },
            { "EmitVertex", "void" },
            { "EndPrimitive", "void" },
            { "subpassLoad", "vec4" },
            { "if", "bool" },
            { "while", "bool" },
            { "for", "bool" },
        };
    }
}