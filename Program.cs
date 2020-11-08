using System;

namespace BatchCommand
{
    class MainClass
    {
        static void Main(string[] args)
        {
            int exitCode = 0;
            try {
                BatchScript.Init();
                object r = null;
                var vargs = BatchScript.NewCalculatorValueList();
                if (args.Length <= 0) {
                    r = BatchScript.Run(string.Empty, vargs);
                } else {
                    foreach(var arg in args) {
                        vargs.Add(arg);
                    }
                    r = BatchScript.Run(args[0], vargs);
                }
                BatchScript.RecycleCalculatorValueList(vargs);
                if (null != r) {
                    exitCode = (int)Convert.ChangeType(r, typeof(int));
                }
            } catch (Exception ex) {
                Console.WriteLine("exception:{0}\n{1}", ex.Message, ex.StackTrace);
                exitCode = -1;
            }
            Environment.Exit(exitCode);
        }
    }
}
