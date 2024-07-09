using System;
using System.IO;
using System.Diagnostics;
using ScriptableFramework;

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

            BatchScript.Init();
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
}
