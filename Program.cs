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
                var r = DslExpression.CalculatorValue.NullObject;
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
                if (!r.IsNullObject) {
                    exitCode = r.Get<int>();
                }
            } catch (Exception ex) {
                BatchScript.Log("exception:{0}\n{1}", ex.Message, ex.StackTrace);
                exitCode = -1;
            }
            Environment.Exit(exitCode);
        }
    }
}
