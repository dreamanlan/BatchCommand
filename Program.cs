using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using ScriptableFramework;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;

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
            BatchScript.Register("compiledbgscp", "compiledbgscp(scpFile, struFile, apiFile) api", new ExpressionFactoryHelper<CompileDbgScpExp>());
            BatchScript.Register("uploaddbgscp", "dumpdbgscp() api", new ExpressionFactoryHelper<UploadDbgScpExp>());
            BatchScript.Register("savedbgscp", "savedbgscp(dataFile) api", new ExpressionFactoryHelper<SaveDbgScpExp>());
            BatchScript.Register("loaddbgscp", "loaddbgscp(dataFile) api", new ExpressionFactoryHelper<LoadDbgScpExp>());
            BatchScript.Register("testdbgscp", "testdbgscp() api", new ExpressionFactoryHelper<TestDbgScpExp>());

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
            Console.WriteLine("[usage]BatchCommand [-h] [-i] [-e script_string] [dsl_file arg1 arg2 ...]");
            Console.WriteLine(" [-h] show this help");
            Console.WriteLine(" [-i] interactive computing mode");
            Console.WriteLine(" [-e script_string] run script_string");
            Console.WriteLine(" dsl_file source dsl file");
            Console.WriteLine(" arg1 arg2 ... arguments to dsl file");
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
                        foreach (var pair in BatchScript.ApiDocs) {
                            if (pair.Key.Contains(filter) || pair.Value.Contains(filter)) {
                                Console.WriteLine("[{0}]:{1}", pair.Key, pair.Value);
                            }
                        }
                    }
                    else {
                        var r = BatchScript.EvalAndRun(line);
                        Console.Write("result:");
                        Console.WriteLine(r.ToString());
                    }
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
    internal sealed class TestDbgScpExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            int a = CppDebugScript.CppDbgScpInterface.Test1(123, 123.456, "test1");
            LogSystem.Warn("retval a:{0}", a);
            int b = CppDebugScript.CppDbgScpInterface.Test2(1234, 1234.456, "test2");
            LogSystem.Warn("retval b:{0}", b);
            CppDebugScript.CppDbgScpInterface.Test3(12345, 12345.456, "test3");
            CppDebugScript.CppDbgScpInterface.Test4(123456, 123456.456, "test4");

            Console.WriteLine("--------");

            int ma = CppDebugScript.CppDbgScpInterface.TestMacro1(123, 123.456, "testmacro1");
            LogSystem.Warn("retval ma:{0}", ma);
            int mb = CppDebugScript.CppDbgScpInterface.TestMacro2(1234, 1234.456, "testmacro2");
            LogSystem.Warn("retval mb:{0}", mb);
            CppDebugScript.CppDbgScpInterface.TestMacro3(12345, 12345.456, "testmacro3");
            CppDebugScript.CppDbgScpInterface.TestMacro4(123456, 123456.456, "testmacro4");
            return BoxedValue.NullObject;
        }
    }
}
