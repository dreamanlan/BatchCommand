using System;
using System.Diagnostics;

namespace BatchCommand
{
    class MainClass
    {
        static void Main(string[] args)
        {
            int exitCode = 0;
            try {
                BatchScript.Init();
                var r = DslExpression.CalculatorValue.NullObject;
                var vargs = BatchScript.NewCalculatorValueList();
                if (args.Length <= 0) {
                    r = BatchScript.Run(string.Empty, vargs);
                } else {
                    foreach(var arg in args) {
                        vargs.Add(arg);
                    }
                    Stopwatch sw = Stopwatch.StartNew();
                    r = BatchScript.Run(args[0], vargs);
                    sw.Stop();
                    long us = sw.ElapsedTicks*1000000 / Stopwatch.Frequency;
                    Console.WriteLine("consume time: {0}us", us);
                }
                BatchScript.RecycleCalculatorValueList(vargs);
                if (!r.IsNullObject) {
                    exitCode = r.GetInt();
                }
            } catch (Exception ex) {
                BatchScript.Log("exception:{0}\n{1}", ex.Message, ex.StackTrace);
                exitCode = -1;
            }
            Environment.Exit(exitCode);
        }
    }
}
