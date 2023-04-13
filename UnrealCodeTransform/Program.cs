// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

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
                bool isTest = false;
                if (isTest) {
                    string file = @"D:\UGit\UndoBase.h";
                    string targetFile = @"D:\test.h";
                    string filter = "*.h";
                    CopyFile(file, targetFile, filter, "pp.txt");
                }
                else {
                    string srcDir = args[0];
                    string destDir = args[1];
                    CopyDir(srcDir, destDir);
                }
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
                    CopyFile(file, targetFile, filter, string.Empty);

                    ++ct;
                    Console.SetCursorPosition(1, 1);
                    Console.Write("copy {0}/{1} file {2} => {3}", ct, total, file, targetFile);
                    Console.WriteLine(s_Spaces);
                }
            }
        }
        private static void CopyFile(string file, string targetFile, string filter, string ppfile)
        {
            if (s_CopyFileTypes.Contains(filter)) {
                File.Copy(file, targetFile, true);
            }
            else {
                string[]? lines = null;
                try {
                    string txt = File.ReadAllText(file);
                    if (s_CppTemplStart.IsMatch(txt) && s_CppTemplEnd.IsMatch(txt)) {
                        s_ErrorWriter?.WriteLine("skip template file {0}.", file);
                        s_ErrorWriter?.Flush();
                        Console.Write("skip template file {0}.", file);
                        Console.WriteLine(s_Spaces);
                        lines = File.ReadAllLines(file);
                    }
                    /*
                    else if (IsObjectC(txt)) {
                        s_ErrorWriter?.WriteLine("skip Object C file {0}.", file);
                        s_ErrorWriter?.Flush();
                        Console.Write("skip Object C file {0}.", file);
                        Console.WriteLine(s_Spaces);
                        lines = File.ReadAllLines(file);
                    }
                    */
                    else {
                        string gppTxt;
                        txt = Preprocess(txt, file, out gppTxt);
                        if (!string.IsNullOrEmpty(ppfile)) {
                            File.WriteAllText(ppfile, gppTxt);
                        }
                        int ct = txt.Length;
                        if (ct >= 1 && txt[ct - 1] == '\n') {
                            if (ct >= 2 && txt[ct - 2] == '\r')
                                txt = txt.Substring(0, ct - 2);
                            else
                                txt = txt.Substring(0, ct - 1);
                        }
                        lines = txt.Split(new char[] { '\n' }, StringSplitOptions.None);
                    }
                }
                catch (Exception ex) {
                    s_ErrorWriter?.WriteLine("{0} can't parsed {1}\n{2} !", file, ex.Message, ex.StackTrace);
                    s_ErrorWriter?.Flush();
                    Console.Write("{0} can't parsed {1} !", file, ex.Message);
                    Console.WriteLine(s_Spaces);
                }

                if (null != lines) {
                    for (int ii = 0; ii < lines.Length; ++ii) {
                        var line = lines[ii].TrimEnd();
                        lines[ii] = line;
                        var trimLine = line.TrimStart();
                        if (IsMatch(trimLine)) {
                            if (trimLine[trimLine.Length - 1] == ')') {
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
                                        var kline = lines[k].TrimEnd();
                                        if (FetchLine(kline, 0, out pos)) {
                                            lines[ii] = line.Substring(0, line.Length - trimLine.Length) + "/*" + trimLine;
                                            lines[k] = kline.Insert(pos + 1, "*/");
                                            ii = k;
                                            break;
                                        }
                                        else {
                                            lines[k] = kline;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    File.WriteAllLines(targetFile, lines);
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
                        if (s_DslFile.LoadCppFromString(pureLine, msg => s_ErrorWriter?.WriteLine(msg))) {
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
                                if (s_DslFile.LoadCppFromString(mline, msg => s_ErrorWriter?.WriteLine(msg))) {
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
                        break;
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
                    if (s_MatchStack.Count > 0 && s_MatchStack.Peek() == '(') {
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
                                break;
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
                                    if (second)
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
        private static string Preprocess(string txt, string file, out string gppTxt)
        {
            var sb = new StringBuilder();
            Dsl.DslFile dslFile = new Dsl.DslFile();
            if (dslFile.LoadGppFromString(txt, msg => s_ErrorWriter?.WriteLine("{0} preprocess file:{1}", msg, file), "={:=", "=:}=", out gppTxt)) {
                //遍历并提取代码
                foreach (var info in dslFile.DslInfos) {
                    HandleSyntax(sb, info, false, file);
                }
            }
            return sb.ToString();
        }
        private static void HandleSyntax(StringBuilder sb, Dsl.ISyntaxComponent syntax, bool commentOut, string file)
        {
            var func = syntax as Dsl.FunctionData;
            if (null != func) {
                HandleFunction(sb, func, commentOut, file);
            }
            else {
                var statement = syntax as Dsl.StatementData;
                if (null != statement) {
                    HandleStatement(sb, statement, commentOut, file);
                }
                else {
                    throw new Exception("exception: " + syntax.ToString());
                }
            }
        }
        private static void HandleCall(StringBuilder sb, Dsl.FunctionData call, string file)
        {
            var id = call.GetId();
            if (id.Length >= 2 && id[0] == '@' && id[1] == '@')
                id = "#" + id.Substring(2);
            sb.Append(id);
            foreach(var p in call.Params) {
                sb.Append(' ');
                var vd = p as Dsl.ValueData;
                if (null != vd && vd.GetIdType() == Dsl.ValueData.STRING_TOKEN) {
                    sb.Append(vd.GetId());
                }
                else {
                    sb.Append(p.ToScriptString(false));
                }
            }
        }
        private static void HandleFunction(StringBuilder sb, Dsl.FunctionData func, bool commentOut, string file)
        {
            string id = func.GetId();
            if (id.StartsWith("@@if")) {
                sb.Append("//");
                HandleCall(sb, func.LowerOrderFunction, file);
                sb.AppendLine();
                foreach (var syntax in func.Params) {
                    HandleSyntax(sb, syntax, commentOut, file);
                }
            }
            else if (id.StartsWith("@@el")) {
                sb.Append("//");
                HandleCall(sb, func.LowerOrderFunction, file);
                sb.AppendLine();
                foreach (var syntax in func.Params) {
                    HandleSyntax(sb, syntax, true, file);
                }
            }
            else if (id == "@@code") {
                if (commentOut) {
                    string code = func.GetParamId(0);
                    var lines = code.Split(new char[] { '\n' }, StringSplitOptions.None);
                    foreach (var line in lines) {
                        if (line != lines[0])
                            sb.AppendLine();
                        sb.Append("//");
                        sb.Append(line.TrimEnd());
                    }
                }
                else {
                    sb.Append(func.GetParamId(0));
                }
                //sb.AppendLine();
            }
            else if (id == "@@define") {
                string code = string.Empty;
                if (func.GetParamNum() > 0) {
                    var p = func.GetParam(0);
                    var pvd = p as Dsl.ValueData;
                    if (null != pvd) {
                        code = pvd.GetId();
                    }
                    else {
                        var psd = p as Dsl.StatementData;
                        if (null != psd) {
                            var psb = new StringBuilder();
                            for (int ix = 0; ix < psd.GetFunctionNum(); ++ix) {
                                var pf = psd.GetFunction(ix);
                                if (ix > 0)
                                    psb.Append(" ");
                                psb.Append(pf.GetId());
                            }
                            code = psb.ToString();
                        }
                        else {
                            code = p.ToScriptString(false);
                            s_ErrorWriter?.WriteLine("file:{0} #define {1}", file, code);
                        }
                    }
                }
                else {
                    s_ErrorWriter?.WriteLine("file:{0} #define", file);
                }
                if (commentOut) {
                    sb.Append("//#define ");
                    var lines = code.Split(new char[] { '\n' }, StringSplitOptions.None);
                    for (int i = 0; i < lines.Length; i++) {
                        var line = lines[i];
                        if (line.Length > 0 && line[line.Length - 1] == '\r')
                            line = line.Substring(0, line.Length - 1);
                        sb.Append("//");
                        if (i < lines.Length - 1 && line.Length > 0 && line[line.Length - 1] != '\\')
                            sb.Append(line + "\\");
                        else
                            sb.Append(line);
                        sb.AppendLine();
                    }
                }
                else {
                    sb.Append("#define ");
                    var lines = code.Split(new char[] { '\n' }, StringSplitOptions.None);
                    for (int i = 0; i < lines.Length; i++) {
                        var line = lines[i];
                        if (line.Length > 0 && line[line.Length - 1] == '\r')
                            line = line.Substring(0, line.Length - 1);
                        if (i < lines.Length - 1 && line.Length > 0 && line[line.Length - 1] != '\\')
                            sb.Append(line + "\\");
                        else
                            sb.Append(line);
                        sb.AppendLine();
                    }
                }
                //sb.AppendLine();
            }
            else if (id.StartsWith("@@include") || id.StartsWith("@@undef")) {
                if (commentOut) {
                    sb.Append("//");
                }
                HandleCall(sb, func, file);
                sb.AppendLine();
            }
            else if (id.StartsWith("@@pragma")) {
                HandleCall(sb, func, file);
                sb.AppendLine();
            }
            else {
                sb.Append("//");
                HandleCall(sb, func, file);
                sb.AppendLine();
            }
        }
        private static void HandleStatement(StringBuilder sb, Dsl.StatementData statement, bool commentOut, string file)
        {
            foreach (var vf in statement.Functions) {
                var func = vf.AsFunction;
                if (null != func) {
                    HandleFunction(sb, func, commentOut, file);
                }
                else {
                    //error
                }
            }
        }

        private static StreamWriter? s_ErrorWriter = null;
        private static StringBuilder s_MultiLineBuffer = new StringBuilder();
        private static Dsl.DslFile s_DslFile = new Dsl.DslFile();
        private static string[] s_Filters = new string[] { "*.lua", "*.cs", "*.h", "*.hpp", "*.hxx", "*.hh", "*.inl", "*.c", "*.cpp", "*.cxx", "*.cc", "*.m", "*.mm" };
        private static string[] s_CopyFileTypes = new string[] { "*.lua", "*.cs" };
        private static HashSet<string> s_FilterNames = new HashSet<string> { "UCLASS", "UENUM", "USTRUCT", "UINTERFACE", "UPROPERTY", "UFUNCTION" };
        private static string s_Spaces = string.Empty.PadRight(1024);
        private static Stack<char> s_MatchStack = new Stack<char>();
        private static bool s_InCommentBlock = false;

        private static StringBuilder s_LineBuffer = new StringBuilder();
        private static Regex s_CppTemplStart = new Regex(@"^\s*{%", RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex s_CppTemplEnd = new Regex(@"%}\s*$", RegexOptions.Compiled | RegexOptions.Multiline);
    }
}