using System;
using System.IO;
using System.Diagnostics;

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
                for (int i = 0; i < args.Length; ++i) {
                    if (0 == string.Compare(args[i], "-src", true)) {
                        if (i < args.Length - 1) {
                            string arg = args[i + 1];
                            if (!arg.StartsWith("-")) {
                                scpFile = arg;
                                if (!File.Exists(scpFile)) {
                                    Console.WriteLine("file path not found ! {0}", scpFile);
                                }
                                ++i;
                            }
                        }
                    }
                    else if (0 == string.Compare(args[i], "-e", true)) {
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
                    }
                    else if (args[i][0] == '-') {
                        Console.WriteLine("unknown command option ! {0}", args[i]);
                    }
                    else if(string.IsNullOrEmpty(scpFile)) {
                        scpFile = args[i];
                        if (!File.Exists(scpFile)) {
                            Console.WriteLine("file path not found ! {0}", scpFile);
                        }
                        break;
                    }
                    else {
                        vargs.Add(args[i]);
                    }
                }
            }

            int exitCode = 0;
            try {
                var r = DslExpression.CalculatorValue.NullObject;
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
            } catch (Exception ex) {
                BatchScript.Log("exception:{0}\n{1}", ex.Message, ex.StackTrace);
                exitCode = -1;
            }
            BatchScript.RecycleCalculatorValueList(vargs);
            Environment.Exit(exitCode);
        }
        private static void PrintHelp()
        {
            Console.WriteLine("[usage]BatchCommand [-i] [-e script_string] [-src] [dsl_file arg1 arg2 ...]");
            Console.WriteLine(" [-i] interactive computing mode");
            Console.WriteLine(" [-e script_string] run script_string");
            Console.WriteLine(" [-src ] dsl_file source dsl file, -src can be omitted when file is the last argument");
        }
        private static void InteractiveComputing()
        {
            Console.WriteLine("Enter exit or quit to exit...");
            for (; ; ) {
                Console.Write(">");
                var line = Console.ReadLine();
                if (line == "exit" || line == "quit")
                    break;
                if (null != line) {
                    var r = BatchScript.EvalAndRun(line);
                    Console.Write("result:");
                    Console.WriteLine(r.ToString());
                }
            }
        }
    }
}
