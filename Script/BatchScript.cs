using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Dsl;
using DslExpression;

namespace BatchCommand
{
    internal class FileEchoExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 1) {
                BatchScript.FileEchoOn = operands[0].GetBool();
            }
            return BatchScript.FileEchoOn;
        }
    }

    internal class ListDirectoriesExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var ret = CalculatorValue.NullObject;
            if (operands.Count >= 1) {
                var baseDir = operands[0].AsString;
                baseDir = Environment.ExpandEnvironmentVariables(baseDir);
                IList<string> filterList = new string[] { "*" };
                if (operands.Count >= 2) {
                    var list = new List<string>();
                    for (int i = 1; i < operands.Count; ++i) {
                        var str = operands[i].AsString;
                        if (null != str) {
                            list.Add(str);
                        }
                        else {
                            var strList = operands[i].As<IList>();
                            if (null != strList) {
                                foreach (var strObj in strList) {
                                    var tempStr = strObj as string;
                                    if (null != tempStr)
                                        list.Add(tempStr);
                                }
                            }
                        }
                    }
                    filterList = list;
                }
                if (null != baseDir && Directory.Exists(baseDir)) {
                    var fullList = new List<string>();
                    foreach (var filter in filterList) {
                        var list = Directory.GetDirectories(baseDir, filter, SearchOption.TopDirectoryOnly);
                        fullList.AddRange(list);
                    }
                    ret = CalculatorValue.FromObject(fullList);
                }
            }
            return ret;
        }
    }

    internal class ListFilesExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var ret = CalculatorValue.NullObject;
            if (operands.Count >= 1) {
                var baseDir = operands[0].AsString;
                baseDir = Environment.ExpandEnvironmentVariables(baseDir);
                IList<string> filterList = new string[] { "*" };
                if (operands.Count >= 2) {
                    var list = new List<string>();
                    for (int i = 1; i < operands.Count; ++i) {
                        var str = operands[i].AsString;
                        if (null != str) {
                            list.Add(str);
                        }
                        else {
                            var strList = operands[i].As<IList>();
                            if (null != strList) {
                                foreach (var strObj in strList) {
                                    var tempStr = strObj as string;
                                    if (null != tempStr)
                                        list.Add(tempStr);
                                }
                            }
                        }
                    }
                    filterList = list;
                }
                if (null != baseDir && Directory.Exists(baseDir)) {
                    var fullList = new List<string>();
                    foreach (var filter in filterList) {
                        var list = Directory.GetFiles(baseDir, filter, SearchOption.TopDirectoryOnly);
                        fullList.AddRange(list);
                    }
                    ret = CalculatorValue.FromObject(fullList);
                }
            }
            return ret;
        }
    }

    internal class ListAllDirectoriesExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var ret = CalculatorValue.NullObject;
            if (operands.Count >= 1) {
                var baseDir = operands[0].AsString;
                baseDir = Environment.ExpandEnvironmentVariables(baseDir);
                IList<string> filterList = new string[] { "*" };
                if (operands.Count >= 2) {
                    var list = new List<string>();
                    for (int i = 1; i < operands.Count; ++i) {
                        var str = operands[i].AsString;
                        if (null != str) {
                            list.Add(str);
                        }
                        else {
                            var strList = operands[i].As<IList>();
                            if (null != strList) {
                                foreach (var strObj in strList) {
                                    var tempStr = strObj as string;
                                    if (null != tempStr)
                                        list.Add(tempStr);
                                }
                            }
                        }
                    }
                    filterList = list;
                }
                if (null != baseDir && Directory.Exists(baseDir)) {
                    var fullList = new List<string>();
                    foreach (var filter in filterList) {
                        var list = Directory.GetDirectories(baseDir, filter, SearchOption.AllDirectories);
                        fullList.AddRange(list);
                    }
                    ret = CalculatorValue.FromObject(fullList);
                }
            }
            return ret;
        }
    }

    internal class ListAllFilesExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var ret = CalculatorValue.NullObject;
            if (operands.Count >= 1) {
                var baseDir = operands[0].AsString;
                baseDir = Environment.ExpandEnvironmentVariables(baseDir);
                IList<string> filterList = new string[] { "*" };
                if (operands.Count >= 2) {
                    var list = new List<string>();
                    for (int i = 1; i < operands.Count; ++i) {
                        var str = operands[i].AsString;
                        if (null != str) {
                            list.Add(str);
                        }
                        else {
                            var strList = operands[i].As<IList>();
                            if (null != strList) {
                                foreach (var strObj in strList) {
                                    var tempStr = strObj as string;
                                    if (null != tempStr)
                                        list.Add(tempStr);
                                }
                            }
                        }
                    }
                    filterList = list;
                }
                if (null != baseDir && Directory.Exists(baseDir)) {
                    var fullList = new List<string>();
                    foreach (var filter in filterList) {
                        var list = Directory.GetFiles(baseDir, filter, SearchOption.AllDirectories);
                        fullList.AddRange(list);
                    }
                    ret = CalculatorValue.FromObject(fullList);
                }
            }
            return ret;
        }
    }

    internal class DirectoryExistExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var ret = CalculatorValue.NullObject;
            if (operands.Count >= 1) {
                var dir = operands[0].AsString;
                dir = Environment.ExpandEnvironmentVariables(dir);
                ret = Directory.Exists(dir);
            }
            return ret;
        }
    }

    internal class FileExistExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var ret = CalculatorValue.NullObject;
            if (operands.Count >= 1) {
                var file = operands[0].AsString;
                file = Environment.ExpandEnvironmentVariables(file);
                ret = File.Exists(file);
            }
            return ret;
        }
    }

    internal class CreateDirectoryExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            bool ret = false;
            if (operands.Count >= 1) {
                var dir = operands[0].AsString;
                dir = Environment.ExpandEnvironmentVariables(dir);
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                    ret = true;

                    if (BatchScript.FileEchoOn) {
                        Console.WriteLine("create directory {0}", dir);
                    }
                }
            }
            return ret;
        }
    }

    internal class CopyDirectoryExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            int ct = 0;
            if (operands.Count >= 2) {
                var dir1 = operands[0].AsString;
                var dir2 = operands[1].AsString;
                dir1 = Environment.ExpandEnvironmentVariables(dir1);
                dir2 = Environment.ExpandEnvironmentVariables(dir2);
                List<string> filterAndNewExts = new List<string>();
                for (int i = 2; i < operands.Count; ++i) {
                    var str = operands[i].AsString;
                    if (null != str) {
                        filterAndNewExts.Add(str);
                    }
                    else {
                        var strList = operands[i].As<IList>();
                        if (null != strList) {
                            foreach (var strObj in strList) {
                                var tempStr = strObj as string;
                                if (null != tempStr)
                                    filterAndNewExts.Add(tempStr);
                            }
                        }
                    }
                }
                if (filterAndNewExts.Count <= 0) {
                    filterAndNewExts.Add("*");
                }
                var targetRoot = Path.GetFullPath(dir2);
                if (Directory.Exists(dir1)) {
                    CopyFolder(targetRoot, dir1, dir2, filterAndNewExts, ref ct);
                }
            }
            return ct;
        }
        private static void CopyFolder(string targetRoot, string from, string to, IList<string> filterAndNewExts, ref int ct)
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
                CopyFolder(targetRoot, sub, Path.Combine(to, sName), filterAndNewExts, ref ct);
            }
            // 文件
            for (int i = 0; i < filterAndNewExts.Count; i += 2) {
                string filter = filterAndNewExts[i];
                string newExt = string.Empty;
                if (i + 1 < filterAndNewExts.Count) {
                    newExt = filterAndNewExts[i + 1];
                }
                foreach (string file in Directory.GetFiles(from, filter, SearchOption.TopDirectoryOnly)) {
                    string targetFile;
                    if (string.IsNullOrEmpty(newExt))
                        targetFile = Path.Combine(to, Path.GetFileName(file));
                    else
                        targetFile = Path.Combine(to, Path.ChangeExtension(Path.GetFileName(file), newExt));
                    File.Copy(file, targetFile, true);
                    ++ct;

                    if (BatchScript.FileEchoOn) {
                        Console.WriteLine("copy file {0} => {1}", file, targetFile);
                    }
                }
            }
        }
    }

    internal class MoveDirectoryExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            bool ret = false;
            if (operands.Count >= 2) {
                var dir1 = operands[0].AsString;
                var dir2 = operands[1].AsString;
                dir1 = Environment.ExpandEnvironmentVariables(dir1);
                dir2 = Environment.ExpandEnvironmentVariables(dir2);
                if (Directory.Exists(dir1)) {
                    if (Directory.Exists(dir2)) {
                        Directory.Delete(dir2);
                    }
                    Directory.Move(dir1, dir2);
                    ret = true;

                    if (BatchScript.FileEchoOn) {
                        Console.WriteLine("move directory {0} => {1}", dir1, dir2);
                    }
                }
            }
            return ret;
        }
    }

    internal class DeleteDirectoryExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            bool ret = false;
            if (operands.Count >= 1) {
                var dir = operands[0].AsString;
                dir = Environment.ExpandEnvironmentVariables(dir);
                if (Directory.Exists(dir)) {
                    Directory.Delete(dir, true);
                    ret = true;

                    if (BatchScript.FileEchoOn) {
                        Console.WriteLine("delete directory {0}", dir);
                    }
                }
            }
            return ret;
        }
    }

    internal class CopyFileExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            bool ret = false;
            if (operands.Count >= 2) {
                var file1 = operands[0].AsString;
                var file2 = operands[1].AsString;
                file1 = Environment.ExpandEnvironmentVariables(file1);
                file2 = Environment.ExpandEnvironmentVariables(file2);
                if (File.Exists(file1)) {
                    var dir = Path.GetDirectoryName(file2);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                        Directory.CreateDirectory(dir);
                    }
                    File.Copy(file1, file2, true);
                    ret = true;

                    if (BatchScript.FileEchoOn) {
                        Console.WriteLine("copy file {0} => {1}", file1, file2);
                    }
                }
            }
            return ret;
        }
    }

    internal class CopyFilesExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            int ct = 0;
            if (operands.Count >= 2) {
                var dir1 = operands[0].AsString;
                var dir2 = operands[1].AsString;
                dir1 = Environment.ExpandEnvironmentVariables(dir1);
                dir2 = Environment.ExpandEnvironmentVariables(dir2);
                List<string> filterAndNewExts = new List<string>();
                for (int i = 2; i < operands.Count; ++i) {
                    var str = operands[i].AsString;
                    if (null != str) {
                        filterAndNewExts.Add(str);
                    }
                    else {
                        var strList = operands[i].As<IList>();
                        if (null != strList) {
                            foreach (var strObj in strList) {
                                var tempStr = strObj as string;
                                if (null != tempStr)
                                    filterAndNewExts.Add(tempStr);
                            }
                        }
                    }
                }
                if (filterAndNewExts.Count <= 0) {
                    filterAndNewExts.Add("*");
                }
                if (Directory.Exists(dir1)) {
                    CopyFolder(dir1, dir2, filterAndNewExts, ref ct);
                }
            }
            return ct;
        }
        private static void CopyFolder(string from, string to, IList<string> filterAndNewExts, ref int ct)
        {
            if (!string.IsNullOrEmpty(to) && !Directory.Exists(to))
                Directory.CreateDirectory(to);
            // 文件
            for (int i = 0; i < filterAndNewExts.Count; i += 2) {
                string filter = filterAndNewExts[i];
                string newExt = string.Empty;
                if (i + 1 < filterAndNewExts.Count) {
                    newExt = filterAndNewExts[i + 1];
                }
                foreach (string file in Directory.GetFiles(from, filter, SearchOption.TopDirectoryOnly)) {
                    string targetFile;
                    if (string.IsNullOrEmpty(newExt))
                        targetFile = Path.Combine(to, Path.GetFileName(file));
                    else
                        targetFile = Path.Combine(to, Path.ChangeExtension(Path.GetFileName(file), newExt));
                    File.Copy(file, targetFile, true);
                    ++ct;

                    if (BatchScript.FileEchoOn) {
                        Console.WriteLine("copy file {0} => {1}", file, targetFile);
                    }
                }
            }
        }
    }

    internal class MoveFileExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            bool ret = false;
            if (operands.Count >= 2) {
                var file1 = operands[0].AsString;
                var file2 = operands[1].AsString;
                file1 = Environment.ExpandEnvironmentVariables(file1);
                file2 = Environment.ExpandEnvironmentVariables(file2);
                if (File.Exists(file1)) {
                    var dir = Path.GetDirectoryName(file2);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                        Directory.CreateDirectory(dir);
                    }
                    if (File.Exists(file2)) {
                        File.Delete(file2);
                    }
                    File.Move(file1, file2);
                    ret = true;

                    if (BatchScript.FileEchoOn) {
                        Console.WriteLine("move file {0} => {1}", file1, file2);
                    }
                }
            }
            return ret;
        }
    }

    internal class DeleteFileExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            bool ret = false;
            if (operands.Count >= 1) {
                var file = operands[0].AsString;
                file = Environment.ExpandEnvironmentVariables(file);
                if (File.Exists(file)) {
                    File.Delete(file);
                    ret = true;

                    if (BatchScript.FileEchoOn) {
                        Console.WriteLine("delete file {0}", file);
                    }
                }
            }
            return ret;
        }
    }

    internal class DeleteFilesExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            int ct = 0;
            if (operands.Count >= 1) {
                var dir = operands[0].AsString;
                List<string> filters = new List<string>();
                for (int i = 1; i < operands.Count; ++i) {
                    var str = operands[i].AsString;
                    if (null != str) {
                        filters.Add(str);
                    }
                    else {
                        var strList = operands[i].As<IList>();
                        if (null != strList) {
                            foreach (var strObj in strList) {
                                var tempStr = strObj as string;
                                if (null != tempStr)
                                    filters.Add(tempStr);
                            }
                        }
                    }
                }
                if (filters.Count <= 0) {
                    filters.Add("*");
                }
                dir = Environment.ExpandEnvironmentVariables(dir);
                if (Directory.Exists(dir)) {
                    foreach (var filter in filters) {
                        foreach (string file in Directory.GetFiles(dir, filter, SearchOption.TopDirectoryOnly)) {
                            File.Delete(file);
                            ++ct;

                            if (BatchScript.FileEchoOn) {
                                Console.WriteLine("delete file {0}", file);
                            }
                        }
                    }
                }
            }
            return ct;
        }
    }

    internal class DeleteAllFilesExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            int ct = 0;
            if (operands.Count >= 1) {
                var dir = operands[0].AsString;
                List<string> filters = new List<string>();
                for (int i = 1; i < operands.Count; ++i) {
                    var str = operands[i].AsString;
                    if (null != str) {
                        filters.Add(str);
                    }
                    else {
                        var strList = operands[i].As<IList>();
                        if (null != strList) {
                            foreach (var strObj in strList) {
                                var tempStr = strObj as string;
                                if (null != tempStr)
                                    filters.Add(tempStr);
                            }
                        }
                    }
                }
                if (filters.Count <= 0) {
                    filters.Add("*");
                }
                dir = Environment.ExpandEnvironmentVariables(dir);
                if (Directory.Exists(dir)) {
                    foreach (var filter in filters) {
                        foreach (string file in Directory.GetFiles(dir, filter, SearchOption.AllDirectories)) {
                            File.Delete(file);
                            ++ct;

                            if (BatchScript.FileEchoOn) {
                                Console.WriteLine("delete file {0}", file);
                            }
                        }
                    }
                }
            }
            return ct;
        }
    }

    internal class GetFileInfoExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var ret = CalculatorValue.NullObject;
            if (operands.Count >= 1) {
                var file = operands[0].AsString;
                file = Environment.ExpandEnvironmentVariables(file);
                if (File.Exists(file)) {
                    ret = CalculatorValue.FromObject(new FileInfo(file));
                }
            }
            return ret;
        }
    }

    internal class GetDirectoryInfoExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var ret = CalculatorValue.NullObject;
            if (operands.Count >= 1) {
                var file = operands[0].AsString;
                file = Environment.ExpandEnvironmentVariables(file);
                if (Directory.Exists(file)) {
                    ret = CalculatorValue.FromObject(new DirectoryInfo(file));
                }
            }
            return ret;
        }
    }

    internal class GetDriveInfoExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var ret = CalculatorValue.NullObject;
            if (operands.Count >= 1) {
                var drive = operands[0].AsString;
                ret = CalculatorValue.FromObject(new DriveInfo(drive));
            }
            return ret;
        }
    }

    internal class GetDrivesInfoExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var ret = DriveInfo.GetDrives();
            return CalculatorValue.FromObject(ret);
        }
    }

    internal class GrepExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            CalculatorValue r = CalculatorValue.NullObject;
            if (operands.Count >= 1) {
                var lines = operands[0].As<IList<string>>();
                Regex regex = null;
                if (operands.Count >= 2)
                    regex = new Regex(operands[1].AsString, RegexOptions.Compiled);
                var outLines = new List<string>();
                if (null != lines) {
                    int ct = lines.Count;
                    for (int i = 0; i < ct; ++i) {
                        string lineStr = lines[i];
                        if (null != regex) {
                            if (regex.IsMatch(lineStr)) {
                                outLines.Add(lineStr);
                            }
                        }
                        else {
                            outLines.Add(lineStr);
                        }
                    }
                    r = CalculatorValue.FromObject(outLines);
                }
            }
            return r;
        }
    }

    internal class SubstExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var r = CalculatorValue.NullObject;
            if (operands.Count >= 3) {
                var lines = operands[0].As<IList<string>>();
                Regex regex = new Regex(operands[1].AsString, RegexOptions.Compiled);
                string subst = operands[2].AsString;
                int count = -1;
                if (operands.Count >= 4)
                    count = operands[3].Get<int>();
                var outLines = new List<string>();
                if (null != lines && null != regex && null != subst) {
                    int ct = lines.Count;
                    for (int i = 0; i < ct; ++i) {
                        string lineStr = lines[i];
                        lineStr = regex.Replace(lineStr, subst, count);
                        outLines.Add(lineStr);
                    }
                    r = CalculatorValue.FromObject(outLines);
                }
            }
            return r;
        }
    }

    internal class AwkExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var r = CalculatorValue.NullObject;
            if (operands.Count >= 2) {
                var lines = operands[0].As<IList<string>>();
                var script = operands[1].AsString;
                bool removeEmpties = true;
                if (operands.Count >= 3)
                    removeEmpties = operands[2].Get<bool>();
                var sepList = new List<string> { " " };
                if (operands.Count >= 4) {
                    sepList.Clear();
                    for(int i = 3; i < operands.Count; ++i) {
                        var sep = operands[i].AsString;
                        if (!string.IsNullOrEmpty(sep)) {
                            sepList.Add(sep);
                        }
                    }
                }
                var outLines = new List<string>();
                if (null != lines && !string.IsNullOrEmpty(script)) {
                    var seps = sepList.ToArray();
                    var scpId = BatchScript.Eval(script, s_ArgNames);
                    var args = BatchScript.NewCalculatorValueList();
                    int ct = lines.Count;
                    for (int i = 0; i < ct; ++i) {
                        string lineStr = lines[i];
                        var fields = lineStr.Split(seps, removeEmpties ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
                        args.Clear();
                        args.Add(lineStr);
                        foreach (var field in fields) {
                            args.Add(field);
                        }
                        var o = BatchScript.Call(scpId, args);
                        BatchScript.RecycleCalculatorValueList(args);
                        var rlist = o.As<IList>();
                        if (null != rlist) {
                            foreach(var item in rlist) {
                                var str = item as string;
                                if (null != str) {
                                    outLines.Add(str);
                                }
                            }
                        }
                        else {
                            var str = o.AsString;
                            if (null != str) {
                                outLines.Add(str);
                            }
                        }
                    }
                    r = CalculatorValue.FromObject(outLines);
                }
            }
            return r;
        }

        private static string[] s_ArgNames = new string[] { "$0", "$1", "$2", "$3", "$4", "$5", "$6", "$7", "$8", "$9", "$10", "$11", "$12", "$13", "$14", "$15", "$16" };
    }

    internal class CommandExp : AbstractExpression
    {
        protected override CalculatorValue DoCalc()
        {
            int exitCode = 0;
            MemoryStream ims = null, oms = null;
            int ct = m_CommandConfigs.Count;
            for (int i = 0; i < ct; ++i) {
                try {
                    if (i > 0) {
                        ims = oms;
                        oms = null;
                    }
                    if (i < ct - 1) {
                        oms = new MemoryStream();
                    }
                    var cfg = m_CommandConfigs[i];
                    if (cfg.m_Commands.Count > 0) {
                        exitCode = ExecCommand(cfg, ims, oms);
                    }
                    else {
                        exitCode = ExecProcess(cfg, ims, oms);
                    }
                }
                finally {
                    if (null != ims) {
                        ims.Close();
                        ims.Dispose();
                        ims = null;
                    }
                }
            }
            return exitCode;
        }
        protected override bool Load(Dsl.FunctionData funcData)
        {
            var id = funcData.GetId();
            if (funcData.IsHighOrder) {
                var callData = funcData.LowerOrderFunction;
                LoadCall(callData);
            }
            else if (funcData.HaveParam()) {
                LoadCall(funcData);
            }
            else {
                var cmd = new CommandConfig();
                m_CommandConfigs.Add(cmd);
            }
            if (funcData.HaveStatement()) {
                var cmd = m_CommandConfigs[m_CommandConfigs.Count - 1];
                for (int i = 0; i < funcData.GetParamNum(); ++i) {
                    var comp = funcData.GetParam(i);
                    var cd = comp as Dsl.FunctionData;
                    if (null != cd) {
                        int num = cd.GetParamNum();
                        if (cd.HaveExternScript()) {
                            string os = cd.GetId();
                            string txt = cd.GetParamId(0);
                            cmd.m_Commands.Add(os, txt);
                        }
                        else if (num >= 2) {
                            string type = cd.GetId();
                            var exp = Calculator.Load(cd.GetParam(0));
                            var opt = Calculator.Load(cd.GetParam(1));
                            if (type == "output") {
                                cmd.m_Output = exp;
                                cmd.m_OutputOptArg = opt;
                            }
                            else if (type == "error") {
                                cmd.m_Error = exp;
                                cmd.m_ErrorOptArg = opt;
                            }
                            else {
                                BatchScript.Log("[syntax error] {0} line:{1}", cd.ToScriptString(false), cd.GetLine());
                            }
                        }
                        else if (num >= 1) {
                            string type = cd.GetId();
                            var exp = Calculator.Load(cd.GetParam(0));
                            if (type == "input") {
                                cmd.m_Input = exp;
                            }
                            else if (type == "output") {
                                cmd.m_Output = exp;
                            }
                            else if (type == "error") {
                                cmd.m_Error = exp;
                            }
                            else if (type == "redirecttoconsole") {
                                cmd.m_RedirectToConsole = exp;
                            }
                            else if (type == "nowait") {
                                cmd.m_NoWait = exp;
                            }
                            else if (type == "useshellexecute") {
                                cmd.m_UseShellExecute = exp;
                            }
                            else if (type == "verb") {
                                cmd.m_Verb = exp;
                            }
                            else if (type == "domain") {
                                cmd.m_Domain = exp;
                            }
                            else if (type == "user") {
                                cmd.m_UserName = exp;
                            }
                            else if (type == "password") {
                                cmd.m_Password = exp;
                            }
                            else if (type == "passwordincleartext") {
                                cmd.m_PasswordInClearText = exp;
                            }
                            else if (type == "loadprofile") {
                                cmd.m_LoadUserProfile = exp;
                            }
                            else if (type == "windowstyle") {
                                cmd.m_WindowStyle = exp;
                            }
                            else if (type == "newwindow") {
                                cmd.m_NewWindow = exp;
                            }
                            else if (type == "errordialog") {
                                cmd.m_ErrorDialog = exp;
                            }
                            else if (type == "workingdirectory") {
                                cmd.m_WorkingDirectory = exp;
                            }
                            else if (type == "encoding") {
                                cmd.m_Encoding = exp;
                            }
                            else {
                                BatchScript.Log("[syntax error] {0} line:{1}", cd.ToScriptString(false), cd.GetLine());
                            }
                        }
                        else {
                            BatchScript.Log("[syntax error] {0} line:{1}", cd.ToScriptString(false), cd.GetLine());
                        }
                    }
                    else {
                        BatchScript.Log("[syntax error] {0} line:{1}", comp.ToScriptString(false), comp.GetLine());
                    }
                }
            }
            return true;
        }
        protected override bool Load(Dsl.StatementData statementData)
        {
            for (int i = 0; i < statementData.GetFunctionNum(); ++i) {
                var funcData = statementData.GetFunction(i);
                Load(funcData);
            }
            return true;
        }

        private bool LoadCall(FunctionData callData)
        {
            var cmd = new CommandConfig();
            m_CommandConfigs.Add(cmd);

            var id = callData.GetId();
            if (id == "process") {
                int num = callData.GetParamNum();
                if (num > 0) {
                    var param0 = callData.GetParam(0);
                    var exp0 = Calculator.Load(param0);
                    cmd.m_FileName = exp0;

                    if (num > 1) {
                        var param1 = callData.GetParam(1);
                        var exp1 = Calculator.Load(param1);
                        cmd.m_Argments = exp1;
                    }
                }
                else {
                    BatchScript.Log("[syntax error] {0} line:{1}", callData.ToScriptString(false), callData.GetLine());
                }
            }
            else if (id == "command") {
                int num = callData.GetParamNum();
                if (num > 0) {
                    BatchScript.Log("[syntax error] {0} line:{1}", callData.ToScriptString(false), callData.GetLine());
                }
            }
            else {
                BatchScript.Log("[syntax error] {0} line:{1}", callData.ToScriptString(false), callData.GetLine());
            }
            return true;
        }
        private int ExecProcess(CommandConfig cfg, Stream istream, Stream ostream)
        {
            string fileName = string.Empty;
            if (null != cfg.m_FileName) {
                fileName = cfg.m_FileName.Calc().AsString;
            }
            string args = string.Empty;
            if (null != cfg.m_Argments) {
                args = cfg.m_Argments.Calc().AsString;
            }
            bool noWait = false;
            if (null != cfg.m_NoWait) {
                noWait = cfg.m_NoWait.Calc().Get<bool>();
            }
            BatchScript.ProcessStartOption option = new BatchScript.ProcessStartOption();
            if (null != cfg.m_UseShellExecute) {
                option.UseShellExecute = cfg.m_UseShellExecute.Calc().Get<bool>();
            }
            if (null != cfg.m_Verb) {
                option.Verb = cfg.m_Verb.Calc().AsString;
            }
            if (null != cfg.m_Domain) {
                option.Domain = cfg.m_Domain.Calc().AsString;
            }
            if (null != cfg.m_UserName) {
                option.UserName = cfg.m_UserName.Calc().AsString;
            }
            if (null != cfg.m_Password) {
                option.Password = cfg.m_Password.Calc().AsString;
            }
            if (null != cfg.m_PasswordInClearText) {
                option.PasswordInClearText = cfg.m_PasswordInClearText.Calc().AsString;
            }
            if (null != cfg.m_LoadUserProfile) {
                option.LoadUserProfile = cfg.m_LoadUserProfile.Calc().Get<bool>();
            }
            if (null != cfg.m_WindowStyle) {
                var str = cfg.m_WindowStyle.Calc().AsString;
                ProcessWindowStyle style;
                if (Enum.TryParse(str, out style)) {
                    option.WindowStyle = style;
                }
            }
            if (null != cfg.m_NewWindow) {
                option.NewWindow = cfg.m_NewWindow.Calc().Get<bool>();
            }
            if (null != cfg.m_ErrorDialog) {
                option.ErrorDialog = cfg.m_ErrorDialog.Calc().Get<bool>();
            }
            if (null != cfg.m_WorkingDirectory) {
                option.WorkingDirectory = cfg.m_WorkingDirectory.Calc().AsString;
            }
            Encoding encoding = null;
            if (null != cfg.m_Encoding) {
                var v = cfg.m_Encoding.Calc();
                var name = v.AsString;
                if (!string.IsNullOrEmpty(name)) {
                    encoding = Encoding.GetEncoding(name);
                }
                else if (v.IsInteger) {
                    int codePage = v.Get<int>();
                    encoding = Encoding.GetEncoding(codePage);
                }
            }
            if (null == encoding) {
                encoding = Encoding.UTF8;
            }

            fileName = Environment.ExpandEnvironmentVariables(fileName);
            args = Environment.ExpandEnvironmentVariables(args);

            IList<string> input = null;
            if (null != cfg.m_Input) {
                var v = cfg.m_Input.Calc();
                try {
                    var list = v.As<IList>();
                    if (null != list) {
                        var slist = new List<string>();
                        foreach (var s in list) {
                            slist.Add(s.ToString());
                        }
                        input = slist;
                    }
                    else {
                        var str = v.AsString;
                        if (!string.IsNullOrEmpty(str)) {
                            str = Environment.ExpandEnvironmentVariables(str);
                            input = File.ReadAllLines(str);
                        }
                    }
                }
                catch (Exception ex) {
                    BatchScript.Log("input {0} failed:{1}", v, ex.Message);
                }
            }
            bool redirectToConsole = BatchScript.FileEchoOn;
            StringBuilder outputBuilder = null;
            StringBuilder errorBuilder = null;
            var output = CalculatorValue.NullObject;
            int outputIx = -1;
            var error = CalculatorValue.NullObject;
            int errorIx = -1;
            if (null != cfg.m_Output) {
                var v = cfg.m_Output.Calc();
                var str = v.AsString;
                if (!string.IsNullOrEmpty(str)) {
                    str = Environment.ExpandEnvironmentVariables(str);
                    output = str;
                }
                else {
                    output = v;
                }
                if (null != cfg.m_OutputOptArg)
                    outputIx = cfg.m_OutputOptArg.Calc().Get<int>();
                outputBuilder = new StringBuilder();
            }
            if (null != cfg.m_Error) {
                var v = cfg.m_Error.Calc();
                var str = v.AsString;
                if (!string.IsNullOrEmpty(str)) {
                    str = Environment.ExpandEnvironmentVariables(str);
                    error = str;
                }
                else {
                    error = v;
                }
                if (null != cfg.m_ErrorOptArg)
                    errorIx = cfg.m_ErrorOptArg.Calc().Get<int>();
                errorBuilder = new StringBuilder();
            }
            if (null != cfg.m_RedirectToConsole) {
                var v = cfg.m_RedirectToConsole.Calc();
                redirectToConsole = v.Get<bool>();
            }
            int exitCode = BatchScript.NewProcess(noWait, fileName, args, option, istream, ostream, input, outputBuilder, errorBuilder, redirectToConsole, encoding);
            if (BatchScript.FileEchoOn) {
                Console.WriteLine("new process:{0} {1}, exit code:{2}", fileName, args, exitCode);
            }

            if (null != outputBuilder && !output.IsNullObject) {
                try {
                    var file = output.AsString;
                    if (!string.IsNullOrEmpty(file)) {
                        if (file[0] == '@' || file[0] == '$') {
                            Calculator.SetVariable(file, outputBuilder.ToString());
                        }
                        else {
                            File.WriteAllText(file, outputBuilder.ToString());
                        }
                    }
                    else if (outputIx >= 0) {
                        var list = output.As<IList>();
                        while (list.Count <= outputIx) {
                            list.Add(null);
                        }
                        list[outputIx] = outputBuilder.ToString();
                    }
                }
                catch (Exception ex) {
                    BatchScript.Log("output {0} failed:{1}", output, ex.Message);
                }
            }
            if (null != errorBuilder && !error.IsNullObject) {
                try {
                    var file = error.AsString;
                    if (!string.IsNullOrEmpty(file)) {
                        if (file[0] == '@' || file[0] == '$') {
                            Calculator.SetVariable(file, errorBuilder.ToString());
                        }
                        else {
                            File.WriteAllText(file, errorBuilder.ToString());
                        }
                    }
                    else if (errorIx >= 0) {
                        var list = error.As<IList>();
                        while (list.Count <= errorIx) {
                            list.Add(null);
                        }
                        list[errorIx] = errorBuilder.ToString();
                    }
                }
                catch (Exception ex) {
                    BatchScript.Log("error {0} failed:{1}", error, ex.Message);
                }
            }
            return exitCode;
        }
        private int ExecCommand(CommandConfig cfg, Stream istream, Stream ostream)
        {
            int exitCode = 0;
            string os = string.Empty;
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                os = "unix";
            else
                os = "win";
            string cmd;
            if (cfg.m_Commands.TryGetValue(os, out cmd) || cfg.m_Commands.TryGetValue("common", out cmd)) {
                bool noWait = false;
                if (null != cfg.m_NoWait) {
                    noWait = cfg.m_NoWait.Calc().Get<bool>();
                }
                BatchScript.ProcessStartOption option = new BatchScript.ProcessStartOption();
                if (null != cfg.m_UseShellExecute) {
                    option.UseShellExecute = cfg.m_UseShellExecute.Calc().Get<bool>();
                }
                if (null != cfg.m_Verb) {
                    option.Verb = cfg.m_Verb.Calc().AsString;
                }
                if (null != cfg.m_Domain) {
                    option.Domain = cfg.m_Domain.Calc().AsString;
                }
                if (null != cfg.m_UserName) {
                    option.UserName = cfg.m_UserName.Calc().AsString;
                }
                if (null != cfg.m_Password) {
                    option.Password = cfg.m_Password.Calc().AsString;
                }
                if (null != cfg.m_PasswordInClearText) {
                    option.PasswordInClearText = cfg.m_PasswordInClearText.Calc().AsString;
                }
                if (null != cfg.m_LoadUserProfile) {
                    option.LoadUserProfile = cfg.m_LoadUserProfile.Calc().Get<bool>();
                }
                if (null != cfg.m_WindowStyle) {
                    var str = cfg.m_WindowStyle.Calc().AsString;
                    ProcessWindowStyle style;
                    if (Enum.TryParse(str, out style)) {
                        option.WindowStyle = style;
                    }
                }
                if (null != cfg.m_NewWindow) {
                    option.NewWindow = cfg.m_NewWindow.Calc().Get<bool>();
                }
                if (null != cfg.m_ErrorDialog) {
                    option.ErrorDialog = cfg.m_ErrorDialog.Calc().Get<bool>();
                }
                if (null != cfg.m_WorkingDirectory) {
                    option.WorkingDirectory = cfg.m_WorkingDirectory.Calc().AsString;
                }
                Encoding encoding = null;
                if (null != cfg.m_Encoding) {
                    var v = cfg.m_Encoding.Calc();
                    var name = v.AsString;
                    if (!string.IsNullOrEmpty(name)) {
                        encoding = Encoding.GetEncoding(name);
                    }
                    else if (v.IsInteger) {
                        int codePage = v.Get<int>();
                        encoding = Encoding.GetEncoding(codePage);
                    }
                }
                if (null == encoding) {
                    encoding = Encoding.UTF8;
                }
                IList<string> input = null;
                if (null != cfg.m_Input) {
                    var v = cfg.m_Input.Calc();
                    try {
                        var list = v.As<IList>();
                        if (null != list) {
                            var slist = new List<string>();
                            foreach (var s in list) {
                                slist.Add(s.ToString());
                            }
                            input = slist;
                        }
                        else {
                            var str = v.AsString;
                            if (!string.IsNullOrEmpty(str)) {
                                str = Environment.ExpandEnvironmentVariables(str);
                                input = File.ReadAllLines(str);
                            }
                        }
                    }
                    catch (Exception ex) {
                        BatchScript.Log("input {0} failed:{1}", v, ex.Message);
                    }
                }
                bool redirectToConsole = BatchScript.FileEchoOn;
                StringBuilder outputBuilder = null;
                StringBuilder errorBuilder = null;
                var output = CalculatorValue.NullObject;
                int outputIx = -1;
                var error = CalculatorValue.NullObject;
                int errorIx = -1;
                if (null != cfg.m_Output) {
                    var v = cfg.m_Output.Calc();
                    var str = v.AsString;
                    if (!string.IsNullOrEmpty(str)) {
                        str = Environment.ExpandEnvironmentVariables(str);
                        output = str;
                    }
                    else {
                        output = v;
                    }
                    if (null != cfg.m_OutputOptArg)
                        outputIx = cfg.m_OutputOptArg.Calc().Get<int>();
                    outputBuilder = new StringBuilder();
                }
                if (null != cfg.m_Error) {
                    var v = cfg.m_Error.Calc();
                    var str = v.AsString;
                    if (!string.IsNullOrEmpty(str)) {
                        str = Environment.ExpandEnvironmentVariables(str);
                        error = str;
                    }
                    else {
                        error = v;
                    }
                    if (null != cfg.m_ErrorOptArg)
                        errorIx = cfg.m_ErrorOptArg.Calc().Get<int>();
                    errorBuilder = new StringBuilder();
                }
                if (null != cfg.m_RedirectToConsole) {
                    var v = cfg.m_RedirectToConsole.Calc();
                    redirectToConsole = v.Get<bool>();
                }

                cmd = cmd.Trim();
                var lines = cmd.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                string oneCmd = string.Join(" ", lines).Trim();
                if (!string.IsNullOrEmpty(oneCmd)) {
                    int split = oneCmd.IndexOfAny(new char[] { ' ', '\t' });
                    string fileName = oneCmd;
                    string args = string.Empty;
                    if (split > 0) {
                        fileName = oneCmd.Substring(0, split).Trim();
                        args = oneCmd.Substring(split).Trim();
                    }

                    fileName = Environment.ExpandEnvironmentVariables(fileName);
                    args = Environment.ExpandEnvironmentVariables(args);

                    exitCode = BatchScript.NewProcess(noWait, fileName, args, option, istream, ostream, input, outputBuilder, errorBuilder, redirectToConsole, encoding);
                    if (BatchScript.FileEchoOn) {
                        Console.WriteLine("new process:{0} {1}, exit code:{2}", fileName, args, exitCode);
                    }

                    if (null != outputBuilder && !output.IsNullObject) {
                        try {
                            var file = output.AsString;
                            if (!string.IsNullOrEmpty(file)) {
                                if (file[0] == '@' || file[0] == '$') {
                                    Calculator.SetVariable(file, outputBuilder.ToString());
                                }
                                else {
                                    File.WriteAllText(file, outputBuilder.ToString());
                                }
                            }
                            else if (outputIx >= 0) {
                                var list = output.As<IList>();
                                while (list.Count <= outputIx) {
                                    list.Add(null);
                                }
                                list[outputIx] = outputBuilder.ToString();
                            }
                        }
                        catch (Exception ex) {
                            BatchScript.Log("output {0} failed:{1}", output, ex.Message);
                        }
                    }
                    if (null != errorBuilder && !error.IsNullObject) {
                        try {
                            var file = error.AsString;
                            if (!string.IsNullOrEmpty(file)) {
                                if (file[0] == '@' || file[0] == '$') {
                                    Calculator.SetVariable(file, errorBuilder.ToString());
                                }
                                else {
                                    File.WriteAllText(file, errorBuilder.ToString());
                                }
                            }
                            else if (errorIx >= 0) {
                                var list = error.As<IList>();
                                while (list.Count <= errorIx) {
                                    list.Add(null);
                                }
                                list[errorIx] = errorBuilder.ToString();
                            }
                        }
                        catch (Exception ex) {
                            BatchScript.Log("error {0} failed:{1}", error, ex.Message);
                        }
                    }
                }
            }
            return exitCode;
        }

        private class CommandConfig
        {
            internal IExpression m_FileName = null;
            internal IExpression m_Argments = null;
            internal Dictionary<string, string> m_Commands = new Dictionary<string, string>();

            internal IExpression m_NoWait = null;
            internal IExpression m_UseShellExecute = null;
            internal IExpression m_Verb = null;
            internal IExpression m_Domain = null;
            internal IExpression m_UserName = null;
            internal IExpression m_Password = null;
            internal IExpression m_PasswordInClearText = null;
            internal IExpression m_LoadUserProfile = null;
            internal IExpression m_WindowStyle = null;
            internal IExpression m_NewWindow = null;
            internal IExpression m_ErrorDialog = null;
            internal IExpression m_WorkingDirectory = null;
            internal IExpression m_Encoding = null;
            internal IExpression m_Input = null;
            internal IExpression m_Output = null;
            internal IExpression m_OutputOptArg = null;
            internal IExpression m_Error = null;
            internal IExpression m_ErrorOptArg = null;
            internal IExpression m_RedirectToConsole = null;
        }

        private List<CommandConfig> m_CommandConfigs = new List<CommandConfig>();
    }

    internal class KillExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            int ret = 0;
            if (operands.Count >= 1) {
                int myselfId = 0;
                var myself = Process.GetCurrentProcess();
                if (null != myself) {
                    myselfId = myself.Id;
                }
                var vObj = operands[0];
                var name = vObj.AsString;
                if (!string.IsNullOrEmpty(name)) {
                    int ct = 0;
                    var ps = Process.GetProcessesByName(name);
                    foreach (var p in ps) {
                        if (p.Id != myselfId) {
                            if (BatchScript.FileEchoOn) {
                                Console.WriteLine("kill {0}[pid:{1},session id:{2}]", p.ProcessName, p.Id, p.SessionId);
                            }
                            p.Kill();
                            ++ct;
                        }
                    }
                    ret = ct;
                }
                else if (vObj.IsInteger) {
                    int pid = vObj.Get<int>();
                    var p = Process.GetProcessById(pid);
                    if (null != p && p.Id != myselfId) {
                        if (BatchScript.FileEchoOn) {
                            Console.WriteLine("kill {0}[pid:{1},session id:{2}]", p.ProcessName, p.Id, p.SessionId);
                        }
                        p.Kill();
                        ret = 1;
                    }
                }
                else {

                }
            }
            return CalculatorValue.From(ret);
        }
    }

    internal class KillMeExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            int ret = 0;
            Process p = Process.GetCurrentProcess();
            if (null != p) {
                ret = p.Id;
                int exitCode = 0;
                if (operands.Count >= 1) {
                    exitCode = operands[0].Get<int>();
                }
                if (BatchScript.FileEchoOn) {
                    Console.WriteLine("killme {0}[pid:{1},session id:{2}] exit code:{3}", p.ProcessName, p.Id, p.SessionId, exitCode);
                }
                Environment.Exit(exitCode);
            }
            return ret;
        }
    }

    internal class GetCurrentProcessIdExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            int ret = 0;
            Process p = Process.GetCurrentProcess();
            if (null != p) {
                ret = p.Id;
            }
            return ret;
        }
    }

    internal class ListProcessesExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            IList<Process> ret = null;
            Process[] ps = Process.GetProcesses();
            string filter = null;
            if (operands.Count >= 1) {
                filter = operands[0].AsString;
            }
            if (null == filter)
                filter = string.Empty;
            if (!string.IsNullOrEmpty(filter)) {
                var list = new List<Process>();
                foreach (var p in ps) {
                    try {
                        if (!p.HasExited) {
                            if (p.ProcessName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) {
                                list.Add(p);
                            }
                        }
                    }
                    catch {
                    }
                }
                ret = list;
            }
            else {
                ret = ps;
            }
            return CalculatorValue.FromObject(ret);
        }
    }

    internal class GetScriptDirectoryExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return BatchScript.ScriptDirectory;
        }
    }

    internal class PauseExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var info = Console.ReadKey(true);
            return (int)info.KeyChar;
        }
    }

    internal class WaitExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var ret = CalculatorValue.NullObject;
            if (operands.Count >= 1) {
                var time = operands[0].Get<int>();
                Thread.Sleep(time);
                ret = time;
            }
            return ret;
        }
    }

    internal class WaitAllExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var tasks = BatchScript.Tasks;
            int timeout = -1;
            if (operands.Count >= 1) {
                timeout = operands[0].Get<int>();
            }
            List<int> results = new List<int>();
            if (Task.WaitAll(tasks.ToArray(), timeout)) {
                foreach (var task in tasks) {
                    results.Add(task.Result);
                }
            }
            return CalculatorValue.FromObject(results);
        }
    }

    internal class ClearExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            Console.Clear();
            return CalculatorValue.NullObject;
        }
    }

    internal class WriteExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 1) {
                var obj = operands[0].Get<object>();
                if (null != obj) {
                    var fmt = obj as string;
                    if (operands.Count > 1 && null != fmt) {
                        ArrayList arrayList = new ArrayList();
                        for (int i = 1; i < operands.Count; ++i) {
                            arrayList.Add(operands[i].Get<object>());
                        }
                        Console.Write(fmt, arrayList.ToArray());
                    }
                    else {
                        Console.Write(obj);
                    }
                }
            }
            return CalculatorValue.NullObject;
        }
    }

    internal class ReadLineExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return Console.ReadLine();
        }
    }

    internal class ReadExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            bool nodisplay = false;
            if (operands.Count >= 1) {
                nodisplay = operands[0].Get<bool>();
            }
            var info = Console.ReadKey(nodisplay);
            return info.KeyChar;
        }
    }

    internal class BeepExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 2) {
                int f = operands[0].Get<int>();
                int d = operands[1].Get<int>();
                Console.Beep(f, d);
            }
            else {
                Console.Beep();
            }
            return CalculatorValue.NullObject;
        }
    }

    internal class GetTitleExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return Console.Title;
        }
    }

    internal class SetTitleExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 1) {
                var title = operands[0].AsString;
                if (null != title) {
                    Console.Title = title;
                }
            }
            return CalculatorValue.NullObject;
        }
    }

    internal class GetBufferWidthExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return Console.BufferWidth;
        }
    }

    internal class GetBufferHeightExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return Console.BufferHeight;
        }
    }

    internal class SetBufferSizeExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 2) {
                int w = operands[0].Get<int>();
                int h = operands[1].Get<int>();
                Console.SetBufferSize(w, h);
            }
            return CalculatorValue.NullObject;
        }
    }

    internal class GetCursorLeftExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return Console.CursorLeft;
        }
    }

    internal class GetCursorTopExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return Console.CursorTop;
        }
    }

    internal class SetCursorPosExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 2) {
                int left = operands[0].Get<int>();
                int top = operands[1].Get<int>();
                Console.SetCursorPosition(left, top);
            }
            return CalculatorValue.NullObject;
        }
    }

    internal class GetBgColorExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            //Enum.GetName(typeof(ConsoleColor), Console.BackgroundColor);
            return Console.BackgroundColor.ToString();
        }
    }

    internal class SetBgColorExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 1) {
                var color = operands[0].AsString;
                if (!string.IsNullOrEmpty(color)) {
                    Console.BackgroundColor = (System.ConsoleColor)Enum.Parse(typeof(System.ConsoleColor), color);
                }
            }
            return CalculatorValue.NullObject;
        }
    }

    internal class GetFgColorExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            //Enum.GetName(typeof(ConsoleColor), Console.ForegroundColor);
            return Console.ForegroundColor.ToString();
        }
    }

    internal class SetFgColorExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 1) {
                var color = operands[0].AsString;
                if (!string.IsNullOrEmpty(color)) {
                    Console.ForegroundColor = (System.ConsoleColor)Enum.Parse(typeof(System.ConsoleColor), color);
                }
            }
            return CalculatorValue.NullObject;
        }
    }

    internal class ResetColorExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            Console.ResetColor();
            return CalculatorValue.NullObject;
        }
    }

    internal class SetEncodingExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 1) {
                var encoding1 = operands[0];
                var encoding2 = encoding1;
                if (operands.Count >= 2) {
                    encoding2 = operands[1];
                }
                Console.InputEncoding = BatchScript.GetEncoding(encoding1);
                Console.OutputEncoding = BatchScript.GetEncoding(encoding2);
            }
            else {
                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;
            }
            return CalculatorValue.NullObject;
        }
    }

    internal class GetInputEncodingExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return CalculatorValue.FromObject(Console.InputEncoding);
        }
    }

    internal class GetOutputEncodingExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return CalculatorValue.FromObject(Console.OutputEncoding);
        }
    }

    internal class ConsoleExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return typeof(Console);
        }
    }

    internal class EncodingExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return typeof(Encoding);
        }
    }

    internal class EnvironmentExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return typeof(Environment);
        }
    }

    internal class ReadAllLinesExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 1) {
                string path = operands[0].AsString;
                if (!string.IsNullOrEmpty(path)) {
                    path = Environment.ExpandEnvironmentVariables(path);
                    Encoding encoding = Encoding.UTF8;
                    if (operands.Count >= 2) {
                        var v = operands[1];
                        encoding = BatchScript.GetEncoding(v);
                    }
                    return CalculatorValue.FromObject(File.ReadAllLines(path, encoding));
                }
            }
            return CalculatorValue.FromObject(new string[0]);
        }
    }

    internal class WriteAllLinesExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 2) {
                string path = operands[0].AsString;
                var lines = operands[1].As<IList>();
                if (!string.IsNullOrEmpty(path) && null != lines) {
                    path = Environment.ExpandEnvironmentVariables(path);
                    Encoding encoding = Encoding.UTF8;
                    if (operands.Count >= 3) {
                        var v = operands[2];
                        encoding = BatchScript.GetEncoding(v);
                    }
                    var strs = new List<string>();
                    foreach (var line in lines) {
                        strs.Add(line.ToString());
                    }
                    File.WriteAllLines(path, strs, encoding);
                    return true;
                }
            }
            return false;
        }
    }

    internal class ReadAllTextExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 1) {
                string path = operands[0].AsString;
                if (!string.IsNullOrEmpty(path)) {
                    path = Environment.ExpandEnvironmentVariables(path);
                    Encoding encoding = Encoding.UTF8;
                    if (operands.Count >= 2) {
                        var v = operands[1];
                        encoding = BatchScript.GetEncoding(v);
                    }
                    return File.ReadAllText(path, encoding);
                }
            }
            return CalculatorValue.NullObject;
        }
    }

    internal class WriteAllTextExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 2) {
                string path = operands[0].AsString;
                var text = operands[1].AsString;
                if (!string.IsNullOrEmpty(path) && null != text) {
                    path = Environment.ExpandEnvironmentVariables(path);
                    Encoding encoding = Encoding.UTF8;
                    if (operands.Count >= 3) {
                        var v = operands[2];
                        encoding = BatchScript.GetEncoding(v);
                    }
                    File.WriteAllText(path, text, encoding);
                    return true;
                }
            }
            return false;
        }
    }

    internal class WaitStartIntervalExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 1) {
                var v = operands[0].Get<int>();
                BatchScript.CheckStartInterval = v;
            }
            return BatchScript.CheckStartInterval;
        }
    }

    internal class BatchScript
    {
        internal static bool FileEchoOn
        {
            get { return s_FileEchoOn; }
            set { s_FileEchoOn = value; }
        }
        internal static int CheckStartInterval
        {
            get { return s_CheckStartInterval; }
            set { s_CheckStartInterval = value; }
        }
        internal static string ScriptDirectory
        {
            get { return s_ScriptDirectory; }
        }
        internal static List<Task<int>> Tasks
        {
            get { return s_Tasks; }
        }
        internal static void Log(string fmt, params object[] args)
        {
            Console.WriteLine(fmt, args);
        }
        internal static void Log(object arg)
        {
            Console.WriteLine("{0}", arg);
        }
        internal static void Init()
        {
            s_Calculator.OnLog = msg => { Log(msg); };
            s_Calculator.Init();

            //注册Gm命令
            s_Calculator.Register("fileecho", new ExpressionFactoryHelper<FileEchoExp>());
            s_Calculator.Register("listdirs", new ExpressionFactoryHelper<ListDirectoriesExp>());
            s_Calculator.Register("listfiles", new ExpressionFactoryHelper<ListFilesExp>());
            s_Calculator.Register("listalldirs", new ExpressionFactoryHelper<ListAllDirectoriesExp>());
            s_Calculator.Register("listallfiles", new ExpressionFactoryHelper<ListAllFilesExp>());
            s_Calculator.Register("direxist", new ExpressionFactoryHelper<DirectoryExistExp>());
            s_Calculator.Register("fileexist", new ExpressionFactoryHelper<FileExistExp>());
            s_Calculator.Register("createdir", new ExpressionFactoryHelper<CreateDirectoryExp>());
            s_Calculator.Register("copydir", new ExpressionFactoryHelper<CopyDirectoryExp>());
            s_Calculator.Register("movedir", new ExpressionFactoryHelper<MoveDirectoryExp>());
            s_Calculator.Register("deletedir", new ExpressionFactoryHelper<DeleteDirectoryExp>());
            s_Calculator.Register("copyfile", new ExpressionFactoryHelper<CopyFileExp>());
            s_Calculator.Register("copyfiles", new ExpressionFactoryHelper<CopyFilesExp>());
            s_Calculator.Register("movefile", new ExpressionFactoryHelper<MoveFileExp>());
            s_Calculator.Register("deletefile", new ExpressionFactoryHelper<DeleteFileExp>());
            s_Calculator.Register("deletefiles", new ExpressionFactoryHelper<DeleteFilesExp>());
            s_Calculator.Register("deleteallfiles", new ExpressionFactoryHelper<DeleteAllFilesExp>());
            s_Calculator.Register("getfileinfo", new ExpressionFactoryHelper<GetFileInfoExp>());
            s_Calculator.Register("getdirinfo", new ExpressionFactoryHelper<GetDirectoryInfoExp>());
            s_Calculator.Register("getdriveinfo", new ExpressionFactoryHelper<GetDriveInfoExp>());
            s_Calculator.Register("getdrivesinfo", new ExpressionFactoryHelper<GetDrivesInfoExp>());
            s_Calculator.Register("grep", new ExpressionFactoryHelper<GrepExp>());
            s_Calculator.Register("subst", new ExpressionFactoryHelper<SubstExp>());
            s_Calculator.Register("awk", new ExpressionFactoryHelper<AwkExp>());
            s_Calculator.Register("process", new ExpressionFactoryHelper<CommandExp>());
            s_Calculator.Register("command", new ExpressionFactoryHelper<CommandExp>());
            s_Calculator.Register("kill", new ExpressionFactoryHelper<KillExp>());
            s_Calculator.Register("killme", new ExpressionFactoryHelper<KillMeExp>());
            s_Calculator.Register("pid", new ExpressionFactoryHelper<GetCurrentProcessIdExp>());
            s_Calculator.Register("plist", new ExpressionFactoryHelper<ListProcessesExp>());
            s_Calculator.Register("getscriptdir", new ExpressionFactoryHelper<GetScriptDirectoryExp>());
            s_Calculator.Register("pause", new ExpressionFactoryHelper<PauseExp>());
            s_Calculator.Register("wait", new ExpressionFactoryHelper<WaitExp>());
            s_Calculator.Register("waitall", new ExpressionFactoryHelper<WaitAllExp>());
            s_Calculator.Register("clear", new ExpressionFactoryHelper<ClearExp>());
            s_Calculator.Register("write", new ExpressionFactoryHelper<WriteExp>());
            s_Calculator.Register("readline", new ExpressionFactoryHelper<ReadLineExp>());
            s_Calculator.Register("read", new ExpressionFactoryHelper<ReadExp>());
            s_Calculator.Register("beep", new ExpressionFactoryHelper<BeepExp>());
            s_Calculator.Register("gettitle", new ExpressionFactoryHelper<GetTitleExp>());
            s_Calculator.Register("settitle", new ExpressionFactoryHelper<SetTitleExp>());
            s_Calculator.Register("getbufferwidth", new ExpressionFactoryHelper<GetBufferWidthExp>());
            s_Calculator.Register("getbufferheight", new ExpressionFactoryHelper<GetBufferHeightExp>());
            s_Calculator.Register("setbuffersize", new ExpressionFactoryHelper<SetBufferSizeExp>());
            s_Calculator.Register("getcursorleft", new ExpressionFactoryHelper<GetCursorLeftExp>());
            s_Calculator.Register("getcursortop", new ExpressionFactoryHelper<GetCursorTopExp>());
            s_Calculator.Register("setcursorpos", new ExpressionFactoryHelper<SetCursorPosExp>());
            s_Calculator.Register("getbgcolor", new ExpressionFactoryHelper<GetBgColorExp>());
            s_Calculator.Register("setbgcolor", new ExpressionFactoryHelper<SetBgColorExp>());
            s_Calculator.Register("getfgcolor", new ExpressionFactoryHelper<GetFgColorExp>());
            s_Calculator.Register("setfgcolor", new ExpressionFactoryHelper<SetFgColorExp>());
            s_Calculator.Register("resetcolor", new ExpressionFactoryHelper<ResetColorExp>());
            s_Calculator.Register("setencoding", new ExpressionFactoryHelper<SetEncodingExp>());
            s_Calculator.Register("getinputencoding", new ExpressionFactoryHelper<GetInputEncodingExp>());
            s_Calculator.Register("getoutputencoding", new ExpressionFactoryHelper<GetOutputEncodingExp>());
            s_Calculator.Register("console", new ExpressionFactoryHelper<ConsoleExp>());
            s_Calculator.Register("encoding", new ExpressionFactoryHelper<EncodingExp>());
            s_Calculator.Register("env", new ExpressionFactoryHelper<EnvironmentExp>());
            s_Calculator.Register("readalllines", new ExpressionFactoryHelper<ReadAllLinesExp>());
            s_Calculator.Register("writealllines", new ExpressionFactoryHelper<WriteAllLinesExp>());
            s_Calculator.Register("readalltext", new ExpressionFactoryHelper<ReadAllTextExp>());
            s_Calculator.Register("writealltext", new ExpressionFactoryHelper<WriteAllTextExp>());
            s_Calculator.Register("waitstartinterval", new ExpressionFactoryHelper<WaitStartIntervalExp>());
        }
        internal static void Register(string name, IExpressionFactory factory)
        {
            s_Calculator.Register(name, factory);
        }
        internal static CalculatorValue Run(string scpFile, List<CalculatorValue> args)
        {
            var r = CalculatorValue.NullObject;
            bool redirect = true;
            var vargs = s_Calculator.NewCalculatorValueList();
            vargs.AddRange(args);
            while (redirect) {
                if (string.IsNullOrEmpty(scpFile)) {
                    scpFile = "main.dsl";
                    vargs.Clear();
                    vargs.Add(scpFile);
                }
                var sdir = Path.GetDirectoryName(scpFile);
                sdir = Path.Combine(Environment.CurrentDirectory, sdir);
                s_ScriptDirectory = sdir;
                s_Calculator.Clear();
                s_Calculator.LoadDsl(scpFile);
                Environment.SetEnvironmentVariable("scriptdir", s_ScriptDirectory);
                r = s_Calculator.Calc("main", vargs);
                if (s_Calculator.RunState == RunStateEnum.Redirect) {
                    s_Calculator.RunState = RunStateEnum.Normal;
                    var list = r.As<IList>();
                    if (null == list || list.Count == 0) {
                        vargs.Clear();
                        scpFile = string.Empty;
                    }
                    else {
                        vargs.Clear();
                        foreach(var o in list) {
                            vargs.Add(CalculatorValue.FromObject(o));
                        }
                        scpFile = Environment.ExpandEnvironmentVariables(vargs[0].AsString);
                    }
                }
                else {
                    redirect = false;
                }
            }
            s_Calculator.RecycleCalculatorValueList(vargs);
            return r;
        }
        internal static string Eval(string code, IList<string> argNames)
        {
            string id = System.Guid.NewGuid().ToString();
            string procCode = string.Format("script{{ {0}; }};", code);
            var file = new Dsl.DslFile();
            if (file.LoadFromString(procCode, id, msg => { Log(msg); })) {
                var func = file.DslInfos[0] as Dsl.FunctionData;
                s_Calculator.LoadDsl(id, argNames, func);
                return id;
            }
            return null;
        }
        internal static List<CalculatorValue> NewCalculatorValueList()
        {
            return s_Calculator.NewCalculatorValueList();
        }
        internal static void RecycleCalculatorValueList(List<CalculatorValue> list)
        {
            s_Calculator.RecycleCalculatorValueList(list);
        }
        internal static CalculatorValue Call(string scpFile)
        {
            var args = NewCalculatorValueList();
            var r = Call(scpFile, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static CalculatorValue Call(string scpFile, CalculatorValue arg1)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            var r = Call(scpFile, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static CalculatorValue Call(string scpFile, CalculatorValue arg1, CalculatorValue arg2)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            args.Add(arg2);
            var r = Call(scpFile, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static CalculatorValue Call(string scpFile, CalculatorValue arg1, CalculatorValue arg2, CalculatorValue arg3)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            args.Add(arg2);
            args.Add(arg3);
            var r = Call(scpFile, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static CalculatorValue Call(string scpFile, CalculatorValue arg1, CalculatorValue arg2, CalculatorValue arg3, CalculatorValue arg4)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            args.Add(arg2);
            args.Add(arg3);
            args.Add(arg4);
            var r = Call(scpFile, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static CalculatorValue Call(string scpFile, CalculatorValue arg1, CalculatorValue arg2, CalculatorValue arg3, CalculatorValue arg4, CalculatorValue arg5)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            args.Add(arg2);
            args.Add(arg3);
            args.Add(arg4);
            args.Add(arg5);
            var r = Call(scpFile, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static CalculatorValue Call(string proc, List<CalculatorValue> args)
        {
            var r = s_Calculator.Calc(proc, args);
            return r;
        }
        internal static Encoding GetEncoding(CalculatorValue v)
        {
            var name = v.AsString;
            if (null != name) {
                return Encoding.GetEncoding(name);
            }
            else if (v.IsInteger) {
                int codePage = v.Get<int>();
                return Encoding.GetEncoding(codePage);
            }
            else {
                return Encoding.UTF8;
            }
        }
        internal static int NewProcess(bool noWait, string fileName, string args, ProcessStartOption option, Stream istream, Stream ostream, IList<string> input, StringBuilder output, StringBuilder error, bool redirectToConsole, Encoding encoding)
        {
            if (noWait) {
                var task = Task.Run<int>(() => NewProcessTask(fileName, args, option, istream, ostream, input, output, error, redirectToConsole, encoding));
                s_Tasks.Add(task);
                while (task.Status == TaskStatus.Created || task.Status == TaskStatus.WaitingForActivation || task.Status == TaskStatus.WaitingToRun) {
                    Console.WriteLine("wait {0}[{1}] start", Path.GetFileName(fileName), task.Status);
                    task.Wait(s_CheckStartInterval);
                }
                return 0;
            }
            else {
                return NewProcessTask(fileName, args, option, istream, ostream, input, output, error, redirectToConsole, encoding);
            }
        }
        private static int NewProcessTask(string fileName, string args, ProcessStartOption option, Stream istream, Stream ostream, IList<string> input, StringBuilder output, StringBuilder error, bool redirectToConsole, Encoding encoding)
        {
            //考虑到跨平台兼容性，不使用特定进程环境变量
            try {
                Process p = new Process();
                var psi = p.StartInfo;
                psi.FileName = fileName;
                psi.Arguments = args;
                psi.UseShellExecute = option.UseShellExecute;
                if (null != option.Verb) {
                    psi.Verb = option.Verb;
                }
                if (null != option.Domain) {
                    psi.Domain = option.Domain;
                }
                if (null != option.UserName) {
                    psi.UserName = option.UserName;
                }
                if (null != option.Password) {
                    unsafe {
                        fixed (char* pchar = option.Password.ToCharArray()) {
                            psi.Password = new System.Security.SecureString(pchar, option.Password.Length);
                        }
                    }
                }
                if (null != option.PasswordInClearText) {
                    psi.PasswordInClearText = option.PasswordInClearText;
                }
                psi.LoadUserProfile = option.LoadUserProfile;
                psi.WindowStyle = option.WindowStyle;
                psi.CreateNoWindow = !option.NewWindow;
                psi.ErrorDialog = option.ErrorDialog;
                psi.WorkingDirectory = option.WorkingDirectory;

                if (null != istream || null != input) {
                    psi.RedirectStandardInput = true;
                }
                if (null != ostream || null != output || redirectToConsole) {
                    psi.RedirectStandardOutput = true;
                    psi.StandardOutputEncoding = encoding;
                    var tempStringBuilder = new StringBuilder();
                    p.OutputDataReceived += (sender, e) => OnOutputDataReceived(sender, e, ostream, output, redirectToConsole, encoding, tempStringBuilder);
                }
                if (null != error || redirectToConsole) {
                    psi.RedirectStandardError = true;
                    psi.StandardErrorEncoding = encoding;
                    var tempStringBuilder = new StringBuilder();
                    p.ErrorDataReceived += (sender, e) => OnErrorDataReceived(sender, e, ostream, error, redirectToConsole, encoding, tempStringBuilder);
                }
                if (p.Start()) {
                    if (psi.RedirectStandardInput) {
                        if (null != istream) {
                            istream.Seek(0, SeekOrigin.Begin);
                            using (var sr = new StreamReader(istream, encoding, true, 1024, true)) {
                                string line;
                                while ((line = sr.ReadLine()) != null) {
                                    p.StandardInput.WriteLine(line);
                                    p.StandardInput.Flush();
                                }
                            }
                            p.StandardInput.Close();
                        }
                        else if (null != input) {
                            foreach (var line in input) {
                                p.StandardInput.WriteLine(line);
                                p.StandardInput.Flush();
                            }
                            p.StandardInput.Close();
                        }
                    }
                    if (null != ostream) {
                        ostream.Seek(0, SeekOrigin.Begin);
                        ostream.SetLength(0);
                    }
                    if (psi.RedirectStandardOutput)
                        p.BeginOutputReadLine();
                    if (psi.RedirectStandardError)
                        p.BeginErrorReadLine();
                    p.WaitForExit();
                    if (psi.RedirectStandardOutput) {
                        p.CancelOutputRead();
                    }
                    if (psi.RedirectStandardError) {
                        p.CancelErrorRead();
                    }
                    int r = p.ExitCode;
                    p.Close();
                    return r;
                }
                else {
                    Console.WriteLine("process({0} {1}) failed.", fileName, args);
                    return -1;
                }
            }
            catch (Exception ex) {
                BatchScript.Log("process({0} {1}) exception:{2} stack:{3}", fileName, args, ex.Message, ex.StackTrace);
                while (null != ex.InnerException) {
                    ex = ex.InnerException;
                    BatchScript.Log("\t=> exception:{0} stack:{1}", ex.Message, ex.StackTrace);
                }
                return -1;
            }
        }

        private static void OnOutputDataReceived(object sender, DataReceivedEventArgs e, Stream ostream, StringBuilder output, bool redirectToConsole, Encoding encoding, StringBuilder temp)
        {
            var p = sender as Process;
            if (p.StartInfo.RedirectStandardOutput) {
                temp.Length = 0;
                temp.AppendLine(e.Data);
                var txt = temp.ToString();
                if (null != ostream) {
                    var bytes = encoding.GetBytes(txt);
                    ostream.Write(bytes, 0, bytes.Length);
                }
                if (null != output) {
                    output.Append(txt);
                }
                if (redirectToConsole) {
                    Console.Write(txt);
                }
            }
        }

        private static void OnErrorDataReceived(object sender, DataReceivedEventArgs e, Stream ostream, StringBuilder error, bool redirectToConsole, Encoding encoding, StringBuilder temp)
        {
            var p = sender as Process;
            if (p.StartInfo.RedirectStandardError) {
                temp.Length = 0;
                temp.AppendLine(e.Data);
                var txt = temp.ToString();
                if (null != ostream) {
                    var bytes = encoding.GetBytes(txt);
                    ostream.Write(bytes, 0, bytes.Length);
                }
                if (null != error) {
                    error.Append(txt);
                }
                if (redirectToConsole) {
                    Console.Write(txt);
                }
            }
        }

        internal class ProcessStartOption
        {
            internal bool UseShellExecute = false;
            internal string Verb = null;
            internal string Domain = null;
            internal string UserName = null;
            internal string Password = null;
            internal string PasswordInClearText = null;
            internal bool LoadUserProfile = false;
            internal ProcessWindowStyle WindowStyle = ProcessWindowStyle.Normal;
            internal bool NewWindow = false;
            internal bool ErrorDialog = false;
            internal string WorkingDirectory = Environment.CurrentDirectory;
        }

        private static bool s_FileEchoOn = false;
        private static int s_CheckStartInterval = 500;
        private static string s_ScriptDirectory = string.Empty;
        private static List<Task<int>> s_Tasks = new List<Task<int>>();
        private static DslCalculator s_Calculator = new DslCalculator();
    }
}
