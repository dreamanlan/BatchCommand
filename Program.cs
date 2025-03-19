using System;
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
            BatchScript.Register("uploaddbgscp", "dumpdbgscp() api", new ExpressionFactoryHelper<UploadDbgScpExp>());
            BatchScript.Register("savedbgscp", "savedbgscp(dataFile) api", new ExpressionFactoryHelper<SaveDbgScpExp>());
            BatchScript.Register("loaddbgscp", "loaddbgscp(dataFile) api", new ExpressionFactoryHelper<LoadDbgScpExp>());
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
            string exeFullName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string exeDir = Path.GetDirectoryName(exeFullName);
            var autoCompletions = LoadAutoComplete(exeDir);

            var lineBuilder = new StringBuilder();
            Console.WriteLine("Enter exit or quit to exit...");
            RefreshLine(lineBuilder);
            for (; ; ) {
                if (Console.KeyAvailable) {
                    var keyInfo = Console.ReadKey();
                    if (keyInfo.Key == ConsoleKey.UpArrow) {
                        if (s_HistoryIndex < s_Histories.Count) {
                            var str = s_Histories[s_HistoryIndex];
                            s_HistoryIndex = (s_HistoryIndex - 1 + s_Histories.Count) % s_Histories.Count;
                            lineBuilder.Clear();
                            lineBuilder.Append(str);
                            int pos = str.Length + 1;
                            MoveCursor(pos);
                            RefreshLine(lineBuilder);
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.DownArrow) {
                        if (s_HistoryIndex < s_Histories.Count) {
                            var str = s_Histories[s_HistoryIndex];
                            s_HistoryIndex = (s_HistoryIndex + 1) % s_Histories.Count;
                            lineBuilder.Clear();
                            lineBuilder.Append(str);
                            int pos = str.Length + 1;
                            MoveCursor(pos);
                            RefreshLine(lineBuilder);
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.LeftArrow) {
                        MoveCursorLeft();
                    }
                    else if (keyInfo.Key == ConsoleKey.RightArrow) {
                        MoveCursorRight(lineBuilder.Length + 1);
                    }
                    else if (keyInfo.Key == ConsoleKey.Home) {
                        MoveCursor(1);
                    }
                    else if (keyInfo.Key == ConsoleKey.End) {
                        MoveCursor(lineBuilder.Length + 1);
                    }
                    else if (keyInfo.Key == ConsoleKey.Backspace) {
                        DeleteLeftChar(lineBuilder);
                    }
                    else if (keyInfo.Key == ConsoleKey.Delete) {
                        DeleteRightChar(lineBuilder);
                    }
                    else if((keyInfo.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control && keyInfo.Key == ConsoleKey.V) {
                        string str = TextCopy.ClipboardService.GetText();
                        InsertStr(lineBuilder, str);
                    }
                    else if (!char.IsControl(keyInfo.KeyChar)) {
                        InsertChar(lineBuilder, keyInfo.KeyChar, autoCompletions);
                    }
                    else if (keyInfo.Key == ConsoleKey.Enter) {
                        string line = lineBuilder.ToString();
                        lineBuilder.Clear();
                        s_Histories.Add(line);
                        s_HistoryIndex = s_Histories.Count - 1;
                        while (s_Histories.Count > c_MaxHistoryCount) {
                            s_Histories.RemoveAt(0);
                        }

                        if (line == "exit" || line == "quit")
                            break;
                        if (line == "help" || line.StartsWith("help ")) {
                            Console.WriteLine();
                            string filter = line.Substring(4).Trim();
                            foreach (var pair in BatchScript.ApiDocs) {
                                if (pair.Key.Contains(filter) || pair.Value.Contains(filter)) {
                                    Console.WriteLine("[{0}]:{1}", pair.Key, pair.Value);
                                }
                            }
                        }
                        else {
                            Console.WriteLine();
                            var r = BatchScript.EvalAndRun(line);
                            Console.Write("result:");
                            Console.WriteLine(r.ToString());
                        }
                        RefreshLine(lineBuilder);
                    }
                }
            }
        }
        private static Dictionary<string, string> LoadAutoComplete(string fileDir)
        {
            var autoCompletions = new Dictionary<string, string>();
            string autoCompleteFile = Path.Combine(fileDir, "AutoComplete.txt");
            if (File.Exists(autoCompleteFile)) {
                var lines = File.ReadAllLines(autoCompleteFile);
                foreach (var line in lines) {
                    int si = line.IndexOf("=>");
                    if (si > 0) {
                        string key = line.Substring(0, si);
                        string val = line.Substring(si + 2);
                        autoCompletions.TryAdd(key, val);
                    }
                }
            }
            return autoCompletions;
        }
        private static void MoveCursor(int pos)
        {
            (int left, int top) = Console.GetCursorPosition();
            Console.SetCursorPosition(pos, top);
        }
        private static void MoveCursorLeft()
        {
            (int left, int top) = Console.GetCursorPosition();
            if (left > 1)
                Console.SetCursorPosition(left - 1, top);
        }
        private static void MoveCursorRight(int maxPos)
        {
            (int left, int top) = Console.GetCursorPosition();
            if (left < maxPos)
                Console.SetCursorPosition(left + 1, top);
        }
        private static void DeleteLeftChar(StringBuilder sb)
        {
            (int left, int top) = Console.GetCursorPosition();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                if (left >= 1 && left - 1 < sb.Length) {
                    sb.Remove(left - 1, 1);
                    RefreshLine(sb);
                    Console.SetCursorPosition(left, top);
                }
                else {
                    Console.SetCursorPosition(1, top);
                }
            }
            else {
                if (left >= 2 && left - 2 < sb.Length) {
                    sb.Remove(left - 2, 1);
                    RefreshLine(sb);
                    Console.SetCursorPosition(left - 1, top);
                }
                else {
                    Console.SetCursorPosition(1, top);
                }
            }
        }
        private static void DeleteRightChar(StringBuilder sb)
        {
            (int left, int top) = Console.GetCursorPosition();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                if (left >= 1 && left < sb.Length) {
                    sb.Remove(left - 1, 1);
                    RefreshLine(sb);
                }
            }
            else {
                if (left >= 1 && left < sb.Length) {
                    sb.Remove(left - 1, 1);
                    RefreshLine(sb);
                }
            }
        }
        private static void InsertChar(StringBuilder sb, char c, Dictionary<string,string> autoCompletions)
        {
            (int left, int top) = Console.GetCursorPosition();
            if (left >= 2) {
                sb.Insert(left - 2, c);
            }
            if (RefreshLine(sb, autoCompletions)) {
                Console.SetCursorPosition(left, top);
            }
        }
        private static void InsertStr(StringBuilder sb, string str)
        {
            (int left, int top) = Console.GetCursorPosition();
            if (left >= 2) {
                sb.Insert(left - 2, str);
            }
            RefreshLine(sb);
            Console.SetCursorPosition(left, top);
        }
        private static void RefreshLine(StringBuilder sb)
        {
            (int left, int top) = Console.GetCursorPosition();
            Console.SetCursorPosition(0, top);
            Console.Write(EmptyLine);
            Console.SetCursorPosition(0, top);
            Console.Write(">");
            if (sb.Length > 0) {
                var str = sb.ToString();
                Console.Write(str);
                Console.SetCursorPosition(left, top);
            }
        }
        private static bool RefreshLine(StringBuilder sb, Dictionary<string, string> autoCompletions)
        {
            (int left, int top) = Console.GetCursorPosition();
            Console.SetCursorPosition(0, top);
            Console.Write(EmptyLine);
            Console.SetCursorPosition(0, top);
            Console.Write(">");
            if (sb.Length > 0) {
                var str = sb.ToString();
                if (autoCompletions.TryGetValue(str, out var val)) {
                    sb.Clear();
                    sb.Append(val);
                    Console.Write(val);
                    Console.SetCursorPosition(val.Length + 1, top);
                    return false;
                }
                else {
                    Console.Write(str);
                    Console.SetCursorPosition(left, top);
                }
            }
            return true;
        }
        private static void ClearLine()
        {
            (int left, int top) = Console.GetCursorPosition();
            Console.SetCursorPosition(0, top);
            Console.Write(EmptyLine);
            Console.SetCursorPosition(0, top);
        }
        private static string EmptyLine
        {
            get {
                if (s_MaxCharCount < Console.BufferWidth) {
                    s_MaxCharCount = Console.BufferWidth;
                    s_EmptyLine = new string(' ', s_MaxCharCount);
                }
                return s_EmptyLine;
            }
        }

        private static List<string> s_Histories = new List<string>();
        private static int s_HistoryIndex = 0;
        private static string s_EmptyLine = string.Empty;
        private static int s_MaxCharCount = 0;
        private const int c_MaxHistoryCount = 1024;
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
            int a = CppDebugScript.CppDbgScpInterface.Test1Export(123, 123.456, "test1");
            LogSystem.Warn("retval a:{0}", a);
            int b = CppDebugScript.CppDbgScpInterface.Test2Export(1234, 1234.456, "test2");
            LogSystem.Warn("retval b:{0}", b);
            CppDebugScript.CppDbgScpInterface.Test3Export(12345, 12345.456, "test3");
            CppDebugScript.CppDbgScpInterface.Test4Export(123456, 123456.456, "test4");

            Console.WriteLine("--------");

            int ma = CppDebugScript.CppDbgScpInterface.TestMacro1Export(123, 123.456, "testmacro1");
            LogSystem.Warn("retval ma:{0}", ma);
            int mb = CppDebugScript.CppDbgScpInterface.TestMacro2Export(1234, 1234.456, "testmacro2");
            LogSystem.Warn("retval mb:{0}", mb);
            CppDebugScript.CppDbgScpInterface.TestMacro3Export(12345, 12345.456, "testmacro3");
            CppDebugScript.CppDbgScpInterface.TestMacro4Export(123456, 123456.456, "testmacro4");
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
