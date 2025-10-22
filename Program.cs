﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using ScriptableFramework;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using System.Linq;
using System.Runtime.InteropServices;

namespace BatchCommand
{
    class MainClass
    {
        static void Main(string[] args)
        {
            bool interactiveComputing = false;
            var scpFile = string.Empty;
            var vargs = BatchScript.NewCalculatorValueList();
            string scpTxt = string.Empty;

            GlobalVariables.Instance.IsDevelopment = true;
            GlobalVariables.Instance.IsDebug = true;
            GlobalVariables.Instance.IsDevice = false;
            LogSystem.OnOutput = (GameLogType type, string msg) => {
                Console.WriteLine(msg);
            };

            BatchScript.Init();
            BatchScript.Register("compiledbgscp", "compiledbgscp(scpFile,struFile,apiFile) api", new ExpressionFactoryHelper<CompileDbgScpExp>());
            BatchScript.Register("uploaddbgscp", "uploaddbgscp() api", new ExpressionFactoryHelper<UploadDbgScpExp>());
            BatchScript.Register("savedbgscp", "savedbgscp(dataFile) api", new ExpressionFactoryHelper<SaveDbgScpExp>());
            BatchScript.Register("loaddbgscp", "loaddbgscp(dataFile) api", new ExpressionFactoryHelper<LoadDbgScpExp>());
            BatchScript.Register("dbgscpset", "dbgscpset(cmd,int_a,dbl_b,str_c) api", new ExpressionFactoryHelper<DbgScpSetExp>());
            BatchScript.Register("dbgscpget", "dbgscpget(cmd,int_a,dbl_b,str_c) api,return int", new ExpressionFactoryHelper<DbgScpGetExp>());
            BatchScript.Register("testdbgscp", "testdbgscp() api", new ExpressionFactoryHelper<TestDbgScpExp>());
            BatchScript.Register("addrs2lines", "addrs2lines(textBase,textSeg,dbg,file,outfile) api", new ExpressionFactoryHelper<Addrs2LinesExp>());

            if (args.Length == 0) {
                scpFile = "main.dsl";
                if (File.Exists(scpFile)) {
                    vargs.Clear();
                    vargs.Add(scpFile);
                }
                else {
                    PrintHelp();
                    scpFile = string.Empty;
                    interactiveComputing = true;
                }
            }
            else {
                bool isScpPart = false;
                for (int i = 0; i < args.Length; ++i) {
                    if (0 == string.Compare(args[i], "-e", true)) {
                        if (i < args.Length - 1) {
                            string arg = args[i + 1];
                            if (!arg.StartsWith("-")) {
                                scpTxt = arg;
                                ++i;
                            }
                        }
                    }
                    else if (0 == string.Compare(args[i], "-s", true)) {
                        if (i < args.Length - 1) {
                            string arg = args[i + 1];
                            if (!arg.StartsWith("-")) {
                                scpFile = arg;
                                isScpPart = true;
                                if (!File.Exists(scpFile)) {
                                    Console.WriteLine("file path not found ! {0}", scpFile);
                                }
                                vargs.Add(arg);
                                ++i;
                            }
                        }
                    }
                    else if (0 == string.Compare(args[i], "-i", true)) {
                        interactiveComputing = true;
                    }
                    else if (0 == string.Compare(args[i], "-h", true)) {
                        PrintHelp();
                        return;
                    }
                    else if (!isScpPart && args[i][0] == '-') {
                        Console.WriteLine("unknown command option ! {0}", args[i]);
                    }
                    else {
                        if (string.IsNullOrEmpty(scpFile)) {
                            scpFile = args[i];
                            isScpPart = true;
                            if (!File.Exists(scpFile)) {
                                Console.WriteLine("file path not found ! {0}", scpFile);
                            }
                        }
                        vargs.Add(args[i]);
                    }
                }
            }

            int exitCode = 0;
            try {
                var r = BoxedValue.NullObject;
                if (interactiveComputing) {
                    InteractiveComputing();
                }
                else if (!string.IsNullOrEmpty(scpTxt)) {
                    r = BatchScript.EvalAndRun(scpTxt);
                }
                else if (!string.IsNullOrEmpty(scpFile)) {
                    Stopwatch sw = Stopwatch.StartNew();
                    r = BatchScript.Run(scpFile, vargs);
                    sw.Stop();
                    long us = sw.ElapsedTicks*1000000 / Stopwatch.Frequency;
                    if (BatchScript.TimeStatisticOn)
                        Console.WriteLine("consume time: {0}us", us);
                }
                if (!r.IsNullObject) {
                    exitCode = r.GetInt();
                }
                Console.Out.Flush();
            } catch (Exception ex) {
                BatchScript.Log("exception:{0}\n{1}", ex.Message, ex.StackTrace);
                exitCode = -1;
            }
            BatchScript.RecycleCalculatorValueList(vargs);
            Environment.Exit(exitCode);
        }
        private static void PrintHelp()
        {
            Console.WriteLine("[usage]BatchCommand [-h] [-i] [-e script_string] [[-s] dsl_file arg1 arg2 ...]");
            Console.WriteLine(" [-h] show this help");
            Console.WriteLine(" [-i] interactive computing mode");
            Console.WriteLine(" [-e script_string] run script_string");
            Console.WriteLine(" [-s] dsl_file source dsl file");
            Console.WriteLine(" arg1 arg2 ... arguments to dsl file");
        }
        private static void InteractiveComputing()
        {
            string exeFullName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string exeDir = Path.GetDirectoryName(exeFullName);

            var emptyArgs = new List<string>();
            Console.WriteLine("Enter exit or quit to exit...");
            LineEditor.LoadOrRefresh(exeDir);
            for (; ; ) {
                string line = LineEditor.ReadLine();
                if (line == "exit" || line == "quit")
                    break;
                if (line == "help" || line.StartsWith("help ")) {
                    string filter = line.Substring(4).Trim();
                    foreach (var pair in BatchScript.ApiDocs) {
                        if (pair.Key.Contains(filter) || pair.Value.Contains(filter)) {
                            Console.WriteLine("[{0}]:{1}", pair.Key, pair.Value);
                        }
                    }
                }
                else {
                    var id = BatchScript.EvalAsFunc(line, emptyArgs);
                    var r = BatchScript.Call(id);
                    //var r = BatchScript.EvalAndRun(line);
                    Console.Write("result:");
                    Console.WriteLine(r.ToString());
                }
            }
        }
    }

    internal sealed class CompileDbgScpExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            string scpFile = "hook.txt";
            string struFile = "struct.txt";
            string apiFile = "api.txt";
            if (operands.Count > 0) {
                scpFile = operands[0].AsString;
            }
            if (operands.Count > 1) {
                struFile = operands[1].AsString;
            }
            if (operands.Count > 2) {
                apiFile = operands[2].AsString;
            }
            if (!string.IsNullOrEmpty(apiFile) && !string.IsNullOrEmpty(struFile) && !string.IsNullOrEmpty(scpFile)) {
                string txt = File.ReadAllText(apiFile);
                string err = CppDebugScript.DebugScriptCompiler.Instance.LoadApiDefine(txt);
                if (string.IsNullOrEmpty(err)) {
                    LogSystem.Warn("Load API from {0} finished.", apiFile);
                }
                else {
                    LogSystem.Warn("Load API from {0} failed:{1}", apiFile, err);
                }

                txt = File.ReadAllText(struFile);
                err = CppDebugScript.DebugScriptCompiler.Instance.LoadStructDefine(txt);
                if (string.IsNullOrEmpty(err)) {
                    LogSystem.Warn("Load Struct from {0} finished.", struFile);
                }
                else {
                    LogSystem.Warn("Load Struct from {0} failed:{1}", struFile, err);
                }

                txt = File.ReadAllText(scpFile);
                if (CppDebugScript.DebugScriptCompiler.Instance.Compile(txt, out err)) {
                    LogSystem.Warn("Compile {0} finished.", scpFile);
                    var sb = new StringBuilder();
                    try {
                        CppDebugScript.DebugScriptCompiler.Instance.DumpAsm(sb);
                    }
                    catch (Exception e) {
                        sb.AppendLine();
                        sb.AppendLine();
                        sb.AppendLine("========[Exception]========");
                        sb.AppendLine(e.ToString());
                    }
                    LogSystem.Warn("[ASM]:\n{0}", sb.ToString());
                }
                else {
                    LogSystem.Warn("Compile DebugScript from {0} failed:{1}", scpFile, err);
                }
            }
            return BoxedValue.NullObject;
        }
    }
    internal sealed class UploadDbgScpExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            CppDebugScript.DebugScriptCompiler.Instance.UploadToCppVM();
            return BoxedValue.NullObject;
        }
    }
    internal sealed class SaveDbgScpExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            string file = "bytecode.dat";
            if (operands.Count > 0) {
                file = operands[0].AsString;
            }
            CppDebugScript.DebugScriptCompiler.Instance.SaveByteCode(file);
            return BoxedValue.NullObject;
        }
    }
    internal sealed class LoadDbgScpExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            string file = "bytecode.dat";
            if (operands.Count > 0) {
                file = operands[0].AsString;
            }
            CppDebugScript.DebugScriptCompiler.Instance.LoadByteCode(file);
            return BoxedValue.NullObject;
        }
    }
    internal sealed class DbgScpSetExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            int cmd = 0;
            int a = 123;
            double b = 123.456;
            string c = "test_dbgscp_set";
            if(operands.Count > 0) {
                cmd = operands[0].GetInt();
            }
            if (operands.Count > 1) {
                a = operands[1].GetInt();
            }
            if (operands.Count > 2) {
                b = operands[2].GetDouble();
            }
            if (operands.Count > 3) {
                c = operands[3].GetString();
            }
            CppDebugScript.CppDbgScpInterface.DbgScp_Set_Export(cmd, a, b, c);
            return BoxedValue.NullObject;
        }
    }
    internal sealed class DbgScpGetExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            int cmd = 0;
            int a = 123;
            double b = 123.456;
            string c = "test_dbgscp_get";
            if (operands.Count > 0) {
                cmd = operands[0].GetInt();
            }
            if (operands.Count > 1) {
                a = operands[1].GetInt();
            }
            if (operands.Count > 2) {
                b = operands[2].GetDouble();
            }
            if (operands.Count > 3) {
                c = operands[3].GetString();
            }
            return CppDebugScript.CppDbgScpInterface.DbgScp_Get_Export(cmd, a, b, c);
        }
    }
    internal sealed class TestDbgScpExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            int a = CppDebugScript.CppDbgScpInterface.Test1_Export(123, 123.456, "test1");
            LogSystem.Warn("retval a:{0}", a);
            int b = CppDebugScript.CppDbgScpInterface.Test2_Export(1234, 1234.456, "test2");
            LogSystem.Warn("retval b:{0}", b);
            CppDebugScript.CppDbgScpInterface.Test3_Export(12345, 12345.456, "test3");
            CppDebugScript.CppDbgScpInterface.Test4_Export(123456, 123456.456, "test4");

            Console.WriteLine("--------");

            int ma = CppDebugScript.CppDbgScpInterface.TestMacro1_Export(123, 123.456, "testmacro1");
            LogSystem.Warn("retval ma:{0}", ma);
            int mb = CppDebugScript.CppDbgScpInterface.TestMacro2_Export(1234, 1234.456, "testmacro2");
            LogSystem.Warn("retval mb:{0}", mb);
            CppDebugScript.CppDbgScpInterface.TestMacro3_Export(12345, 12345.456, "testmacro3");
            CppDebugScript.CppDbgScpInterface.TestMacro4_Export(123456, 123456.456, "testmacro4");
            return BoxedValue.NullObject;
        }
    }
    internal sealed class Addrs2LinesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            ulong textBase = 0x72a4d6a000;
            ulong textSeg = 0x55a000;
            string dbg = "E:\\AndroidPlayerDebug\\rel\\Symbols\\arm64-v8a\\libunity.dbg.so";
            string file = "C:\\sdk_full\\platform-tools\\temp.txt";
            string ofile = file;
            if (operands.Count > 0) {
                textBase = operands[0].GetULong();
            }
            if (operands.Count > 1) {
                textSeg = operands[1].GetULong();
            }
            if (operands.Count > 2) {
                dbg = operands[2].GetString();
            }
            if (operands.Count > 3) {
                file = operands[3].GetString();
                ofile = file;
            }
            if(operands.Count > 4) {
                ofile = operands[4].GetString();
            }
            int ct = 0;
            if (File.Exists(file)) {
                var addrHashSet = new HashSet<ulong>();
                var dict = new Dictionary<int, ulong>();

                var lines = File.ReadAllLines(file);
                for (int ix = 0; ix < lines.Length; ++ix) {
                    string line = lines[ix];
                    var fs = line.Split(s_WhiteSpaces, StringSplitOptions.RemoveEmptyEntries);
                    string strAddr = string.Empty;
                    string so = string.Empty;
                    if (fs.Length > 10 && fs[10].Contains("libunity.so") && fs[8].StartsWith("0x")) {
                        strAddr = fs[8];
                        so = fs[10];
                    }
                    else if(fs.Length>3 && fs[3].Contains("libunity.so") && fs[1].StartsWith("0x")) {
                        strAddr = fs[1];
                        so = fs[3];
                    }
                    if(!string.IsNullOrEmpty(strAddr) && !string.IsNullOrEmpty(so)) {
                        int si = line.IndexOf(so);
                        lines[ix] = line.Substring(0, si + so.Length);
                        if (ulong.TryParse(strAddr.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out var addr)) {
                            ulong offset = addr - textBase + textSeg;
                            addrHashSet.Add(offset);
                            dict.Add(ix, offset);
                        }
                    }
                }

                var addrs = addrHashSet.ToArray();
                var symDict = new Dictionary<ulong, string>();

                var coption = new ProcessStartOption();
                for (int ix = 0; ix < addrs.Length; ix += c_BatchCount) {
                    var inputs = new List<string>();
                    for (int i = ix; i < ix + c_BatchCount && i < addrs.Length; ++i) {
                        inputs.Add(addrs[i].ToString("x"));
                    }

                    if (OperatingSystem.IsWindows()) {
                        var output = new StringBuilder();
                        var error = new StringBuilder();
                        int cr = ProcessHelper.RunProcess("addr2line", " -f -C -e " + dbg, coption, 5000, null, null, inputs, output, error, false, false, Encoding.UTF8);
                        if (cr == 0) {
                            string results = output.ToString();
                            var syms = results.Split(s_Seps, StringSplitOptions.RemoveEmptyEntries);
                            for(int i = 0; i < syms.Length - 1; i += 2) {
                                ulong offset = addrs[ix + i / 2];
                                string sym = syms[i] + " " + syms[i + 1];
                                symDict.TryAdd(offset, sym);
                            }
                            ct += inputs.Count;

                            var pos = Console.GetCursorPosition();
                            Console.Write(ct + "/" + addrs.Length);
                            Console.SetCursorPosition(pos.Left, pos.Top);
                        }
                        else {
                            Console.WriteLine("run addr2line failed, exit code:{0}", cr);
                        }
                    }
                }

                for (int ix = 0; ix < lines.Length; ++ix) {
                    string line = lines[ix];
                    if(dict.TryGetValue(ix, out var offset) && symDict.TryGetValue(offset, out var sym)) {
                        lines[ix] = line + " " + sym;
                    }
                }
                File.WriteAllLines(ofile, lines);
            }
            else {
                Console.WriteLine("Can't find '{0}' !", file);
            }
            return ct;
        }

        private static char[] s_WhiteSpaces = new char[] { ' ', '\t' };
        private static char[] s_Seps = new char[] { '\r', '\n' };
        private const int c_BatchCount = 500;
    }
}
