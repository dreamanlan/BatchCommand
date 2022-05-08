// See https://aka.ms/new-console-template for more information
using System.Text;
using System.Text.RegularExpressions;

namespace CppLua2Dsl
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2) {
                Console.WriteLine("[Usage]CppLua2Dsl srcDir destDir");
                return;
            }
            using (s_ErrorWriter = new StreamWriter("error.log", false)) {
                string srcDir = args[0];
                string destDir = args[1];
                CopyDir(srcDir, destDir);
                s_ErrorWriter?.Close();
            }
        }
        private static void CopyDir(string srcDir, string destDir)
        {
            var filters = s_Filters;
            var targetRoot = Path.GetFullPath(destDir);
            if (Directory.Exists(srcDir)) {
                int total = 0;
                CountFiles(srcDir, filters, ref total);
                int ct = 0;
                CopyFolder(targetRoot, srcDir, destDir, filters, total, ref ct);
            }
        }
        private static void CountFiles(string dir, IList<string> filters, ref int ct)
        {
            // 子文件夹
            foreach (string sub in Directory.GetDirectories(dir)) {
                CountFiles(sub, filters, ref ct);
            }
            // 文件
            for (int i = 0; i < filters.Count; ++i) {
                string filter = filters[i];
                var files = Directory.GetFiles(dir, filter, SearchOption.TopDirectoryOnly);
                ct += files.Length;

                if (files.Length > 0) {
                    Console.SetCursorPosition(1, 1);
                    Console.Write("count {0} files {1}", ct, files[files.Length - 1]);
                    Console.WriteLine(s_Spaces);
                }
            }
        }
        private static void CopyFolder(string targetRoot, string from, string to, IList<string> filters, int total, ref int ct)
        {
            if (!string.IsNullOrEmpty(to) && !Directory.Exists(to))
                Directory.CreateDirectory(to);
            // 子文件夹
            foreach (string sub in Directory.GetDirectories(from)) {
                var srcPath = Path.GetFullPath(sub);
                if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) {
                    if (srcPath.IndexOf(targetRoot) == 0)
                        continue;
                }
                else {
                    if (srcPath.IndexOf(targetRoot, StringComparison.CurrentCultureIgnoreCase) == 0)
                        continue;
                }
                var sName = Path.GetFileName(sub);
                CopyFolder(targetRoot, sub, Path.Combine(to, sName), filters, total, ref ct);
            }
            // 文件
            for (int i = 0; i < filters.Count; ++i) {
                string filter = filters[i];
                foreach (string file in Directory.GetFiles(from, filter, SearchOption.TopDirectoryOnly)) {
                    Console.SetCursorPosition(1, 30);

                    string targetFile = Path.Combine(to, Path.GetFileName(file));
                    if (filter == "*.lua") {
                        try {
                            if (s_DslFile.LoadLua(file, msg => s_ErrorWriter?.WriteLine(msg))) {
                                s_DslFile.Save(targetFile);
                            }
                            else {
                                s_ErrorWriter?.WriteLine("{0} can't parsed !", file);
                                s_ErrorWriter?.Flush();
                                Console.Write("{0} can't parsed !", file);
                                Console.WriteLine(s_Spaces);
                            }
                        }
                        catch {
                            s_ErrorWriter?.WriteLine("{0} can't parsed !", file);
                            s_ErrorWriter?.Flush();
                            Console.Write("{0} can't parsed !", file);
                            Console.WriteLine(s_Spaces);
                        }
                    }
                    else {
                        try {
                            string txt = File.ReadAllText(file);
                            if (s_CppTemplStart.IsMatch(txt) && s_CppTemplEnd.IsMatch(txt)) {
                                s_ErrorWriter?.WriteLine("skip template file {0}.", file);
                                s_ErrorWriter?.Flush();
                                Console.Write("skip template file {0}.", file);
                                Console.WriteLine(s_Spaces);
                            }
                            else if (IsObjectC(txt)) {
                                s_ErrorWriter?.WriteLine("skip Object C file {0}.", file);
                                s_ErrorWriter?.Flush();
                                Console.Write("skip Object C file {0}.", file);
                                Console.WriteLine(s_Spaces);
                            }
                            else {
                                if (s_DslFile.LoadCppFromString(txt, file, msg => s_ErrorWriter?.WriteLine(msg))) {
                                    s_DslFile.Save(targetFile);
                                }
                                else {
                                    s_ErrorWriter?.WriteLine("{0} can't parsed !", file);
                                    s_ErrorWriter?.Flush();
                                    Console.Write("{0} can't parsed !", file);
                                    Console.WriteLine(s_Spaces);
                                }
                            }
                        }
                        catch {
                            s_ErrorWriter?.WriteLine("{0} can't parsed !", file);
                            s_ErrorWriter?.Flush();
                            Console.Write("{0} can't parsed !", file);
                            Console.WriteLine(s_Spaces);
                        }
                    }

                    ++ct;
                    Console.SetCursorPosition(1, 1);
                    Console.Write("copy {0}/{1} file {2} => {3}", ct, total, file, targetFile);
                    Console.WriteLine(s_Spaces);
                }
            }
        }
        private static bool IsObjectC(string txt)
        {
            s_LineBuffer.Length = 0;
            for (int i = 0; i < txt.Length; ++i) {
                char c = txt[i];
                if (c == '\r' || c == '\n') {
                    bool endMatch = false;
                    bool first = true;
                    for (int ii = s_LineBuffer.Length - 1; ii >= 0; --ii) {
                        char ch = s_LineBuffer[ii];
                        if (char.IsWhiteSpace(ch)) {
                        }
                        else if (first) {
                            if (ch == ';') {
                                first = false;
                                endMatch = true;
                            }
                            else {
                                endMatch = false;
                                break;
                            }
                        }
                        else {
                            if (ch != ']') {
                                endMatch = false;
                            }
                            break;
                        }
                    }
                    if (endMatch) {
                        first = false;
                        for (int ii = 0; ii < s_LineBuffer.Length; ++ii) {
                            char ch = s_LineBuffer[ii];
                            if (char.IsWhiteSpace(ch)) {
                            }
                            else if (ch == '[') {
                                if (first)
                                    return true;
                                first = true;
                            }
                            else {
                                break;
                            }
                        }
                        first = false;
                        bool second = false;
                        for (int ii = 0; ii < s_LineBuffer.Length; ++ii) {
                            char ch = s_LineBuffer[ii];
                            if (ch == '=') {
                                first = true;
                            }
                            else if (first) {
                                if (char.IsWhiteSpace(ch)) {

                                }
                                else if (ch == '[') {
                                    if(second)
                                        return true;
                                    second = true;
                                }
                                else {
                                    break;
                                }
                            }
                        }
                    }
                    s_LineBuffer.Length = 0;
                }
                else {
                    s_LineBuffer.Append(c);
                }
            }
            return false;
        }

        private static StreamWriter? s_ErrorWriter = null;
        private static StringBuilder s_LineBuffer = new StringBuilder();
        private static Dsl.DslFile s_DslFile = new Dsl.DslFile();
        private static string[] s_Filters = new string[] { "*.lua", "*.h", "*.hpp", "*.hxx", "*.hh", "*.inl", "*.c", "*.cpp", "*.cxx", "*.cc" };
        private static string s_Spaces = string.Empty.PadRight(1024);
        private static Regex s_CppTemplStart = new Regex(@"^\s*{%", RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex s_CppTemplEnd = new Regex(@"%}\s*$", RegexOptions.Compiled | RegexOptions.Multiline);
    }
}