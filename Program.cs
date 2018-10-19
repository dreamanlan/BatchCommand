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
                if (args.Length <= 0) {
                    r = BatchScript.Run(string.Empty, args);
                } else {
                    r = BatchScript.Run(args[0], args);
                }
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
