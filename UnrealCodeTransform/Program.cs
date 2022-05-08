// See https://aka.ms/new-console-template for more information
using System.Text;

namespace UnrealCodeTransform
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2) {
                Console.WriteLine("[Usage]UnrealCodeTransform srcDir destDir");
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
                    if (filter == "*.cs") {
                        File.Copy(file, targetFile, true);
                    }
                    else {
                        var lines = File.ReadAllLines(file);
                        for (int ii = 0; ii < lines.Length; ++ii) {
                            var line = lines[ii];
                            var trimLine = line.TrimStart();
                            if (IsMatch(trimLine)) {
                                var pureLine = trimLine.TrimEnd();
                                if (pureLine[pureLine.Length - 1] == ')') {
                                    lines[ii] = line.Substring(0, line.Length - trimLine.Length) + "//" + trimLine;
                                }
                                else {
                                    s_MatchStack.Clear();
                                    int pos;
                                    if (FetchLine(trimLine, 0, out pos)) {
                                        lines[ii] = line.Substring(0, line.Length - trimLine.Length) + "//" + trimLine;
                                    }
                                    else {
                                        int k = ii + 1;
                                        for (; k < lines.Length; ++k) {
                                            var kline = lines[k];
                                            if(FetchLine(kline, 0, out pos)) {
                                                lines[ii] = line.Substring(0, line.Length - trimLine.Length) + "/*" + trimLine;
                                                lines[k] = kline.Insert(pos + 1, "*/");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        File.WriteAllLines(targetFile, lines);
                    }

                    ++ct;
                    Console.SetCursorPosition(1, 1);
                    Console.Write("copy {0}/{1} file {2} => {3}", ct, total, file, targetFile);
                    Console.WriteLine(s_Spaces);
                }
            }
        }
        private static bool IsMatch(string trimLine)
        {
            bool ret = false;
            foreach (string s in s_FilterNames) {
                if (trimLine.IndexOf(s) == 0) {
                    ret = true;
                    break;
                }
            }
            return ret;
        }
        private static void ParseLines(string[] lines, string file)
        {
            for (int ii = 0; ii < lines.Length; ++ii) {
                var line = lines[ii];
                var trimLine = line.TrimStart();
                if (IsMatch(trimLine)) {
                    var pureLine = trimLine.TrimEnd();
                    if (pureLine[pureLine.Length - 1] == ')') {
                        if (s_DslFile.LoadCppFromString(pureLine, "line", msg => s_ErrorWriter?.WriteLine(msg))) {
                            var pcom = s_DslFile.DslInfos[0];
                            string id = pcom.GetId();
                            if (s_FilterNames.Contains(id)) {
                                lines[ii] = line.Substring(0, line.Length - trimLine.Length) + "//" + trimLine;
                            }
                        }
                        else {
                            s_ErrorWriter?.WriteLine("{0} can't parsed {1} line: {2} !", file, ii, trimLine);
                            s_ErrorWriter?.Flush();
                            Console.WriteLine("{0} can't parsed {1} line: {2} !", file, ii, trimLine);
                        }
                    }
                    else {
                        s_MultiLineBuffer.Clear();
                        s_MultiLineBuffer.AppendLine(trimLine);
                        int k = ii + 1;
                        for (; k < lines.Length; ++k) {
                            var kline = lines[k];
                            var kTrimLine = line.TrimEnd();
                            s_MultiLineBuffer.AppendLine(kTrimLine);
                            if (kTrimLine[kTrimLine.Length - 1] == ')') {
                                var mline = s_MultiLineBuffer.ToString();
                                if (s_DslFile.LoadCppFromString(mline, "mline", msg => s_ErrorWriter?.WriteLine(msg))) {
                                    var pcom = s_DslFile.DslInfos[0];
                                    string id = pcom.GetId();
                                    if (s_FilterNames.Contains(id)) {
                                        lines[ii] = line.Substring(0, line.Length - trimLine.Length) + "/*" + trimLine;
                                        lines[k] = kTrimLine + "*/";
                                    }
                                }
                                else {
                                    s_ErrorWriter?.WriteLine("{0} can't parsed {1}-{2} lines: {3} !", file, ii, k, mline);
                                    s_ErrorWriter?.Flush();
                                    Console.WriteLine("{0} can't parsed {1}-{2} lines: {3} !", file, ii, k, mline);
                                }
                                ii = k;
                                break;
                            }
                        }
                    }
                }
            }
        }
        private static bool FetchLine(string line, int start, out int pos)
        {
            pos = -1;
            int k = start;
            if (s_InCommentBlock) {
                for (int i = 0; i < line.Length; ++i) {
                    char ch = line[i];
                    if (ch == '*' && i < line.Length - 1 && line[i + 1] == '/') {
                        s_InCommentBlock = false;
                        k = i + 2;
                    }
                }
                if (s_InCommentBlock)
                    return false;
            }
            for (int i = k; i < line.Length; ++i) {
                char c = line[i];
                if (c == '\\') {
                    ++i;
                }
                else if (c == '(') {
                    s_MatchStack.Push(c);
                }
                else if (c == ')') {
                    if (s_MatchStack.Count > 0 && s_MatchStack.Peek() == c) {
                        s_MatchStack.Pop();
                    }
                    if (s_MatchStack.Count == 0) {
                        pos = i;
                        return true;
                    }
                }
                else if (c == '"') {
                    for (int j = i + 1; j < line.Length; ++j) {
                        char ch = line[j];
                        if (ch == '\\') {
                            ++j;
                        }
                        else if (ch == '"') {
                            i = j;
                            break;
                        }
                    }
                }
                else {
                    char nc = i < line.Length - 1 ? line[i + 1] : '\0';
                    if (c == '/' && nc == '*') {
                        s_InCommentBlock = true;
                        for (int j = i + 2; j < line.Length; ++j) {
                            char ch = line[j];
                            if (ch == '*' && j < line.Length - 1 && line[j + 1] == '/') {
                                s_InCommentBlock = false;
                                i = j + 1;
                            }
                        }
                        if (s_InCommentBlock)
                            return false;
                    }
                    else if (c == '/' && nc == '/') {
                        return false;
                    }
                    else {
                        char lc = i > 0 ? line[i - 1] : '\0';
                        if (c == '\'' && (!char.IsDigit(lc) || !char.IsDigit(nc))) {
                            for (int j = i + 1; j < line.Length; ++j) {
                                char ch = line[j];
                                if (ch == '\\') {
                                    ++j;
                                }
                                else if (ch == '\'') {
                                    i = j;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static StreamWriter? s_ErrorWriter = null;
        private static StringBuilder s_MultiLineBuffer = new StringBuilder();
        private static Dsl.DslFile s_DslFile = new Dsl.DslFile();
        private static string[] s_Filters = new string[] { "*.cs", "*.h", "*.hpp", "*.hxx", "*.hh", "*.inl", "*.c", "*.cpp", "*.cxx", "*.cc" };
        private static HashSet<string> s_FilterNames = new HashSet<string> { "UCLASS", "UENUM", "USTRUCT", "UINTERFACE", "UPROPERTY", "UFUNCTION" };
        private static string s_Spaces = string.Empty.PadRight(1024);
        private static Stack<char> s_MatchStack = new Stack<char>();
        private static bool s_InCommentBlock = false;
    }
}