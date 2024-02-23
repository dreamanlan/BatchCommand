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
using Microsoft.Win32;
using System.Globalization;

#pragma warning disable 8600,8601,8602,8603,8604,8618,8619,8620,8625,CA1416
namespace BatchCommand
{
    internal sealed class TimeStatisticOnExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 1) {
                BatchScript.TimeStatisticOn = operands[0].GetBool();
            }
            return BatchScript.TimeStatisticOn;
        }
    }

    internal sealed class FileEchoExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 1) {
                BatchScript.FileEchoOn = operands[0].GetBool();
            }
            return BatchScript.FileEchoOn;
        }
    }

    internal sealed class ListDirectoriesExp : SimpleExpressionBase
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

    internal sealed class ListFilesExp : SimpleExpressionBase
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

    internal sealed class ListAllDirectoriesExp : SimpleExpressionBase
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

    internal sealed class ListAllFilesExp : SimpleExpressionBase
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

    internal sealed class DirectoryExistExp : SimpleExpressionBase
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

    internal sealed class FileExistExp : SimpleExpressionBase
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

    internal sealed class CreateDirectoryExp : SimpleExpressionBase
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

    internal sealed class CopyDirectoryExp : SimpleExpressionBase
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

    internal sealed class MoveDirectoryExp : SimpleExpressionBase
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

    internal sealed class DeleteDirectoryExp : SimpleExpressionBase
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

    internal sealed class CopyFileExp : SimpleExpressionBase
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

    internal sealed class CopyFilesExp : SimpleExpressionBase
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

    internal sealed class MoveFileExp : SimpleExpressionBase
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

    internal sealed class DeleteFileExp : SimpleExpressionBase
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

    internal sealed class DeleteFilesExp : SimpleExpressionBase
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

    internal sealed class DeleteAllFilesExp : SimpleExpressionBase
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

    internal sealed class GetFileInfoExp : SimpleExpressionBase
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

    internal sealed class GetDirectoryInfoExp : SimpleExpressionBase
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

    internal sealed class GetDriveInfoExp : SimpleExpressionBase
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

    internal sealed class GetDrivesInfoExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var ret = DriveInfo.GetDrives();
            return CalculatorValue.FromObject(ret);
        }
    }

    internal sealed class GrepExp : SimpleExpressionBase
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

    internal sealed class SubstExp : SimpleExpressionBase
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
                    count = operands[3].GetInt();
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

    internal sealed class AwkExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var r = CalculatorValue.NullObject;
            if (operands.Count >= 2) {
                var lines = operands[0].As<IList<string>>();
                var script = operands[1].AsString;
                bool removeEmpties = true;
                if (operands.Count >= 3)
                    removeEmpties = operands[2].GetBool();
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
                    var scpId = BatchScript.EvalAsFunc(script, s_ArgNames);
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

    internal sealed class CommandExp : AbstractExpression
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
                var func = statementData.GetFunction(i);
                var vd = func.AsValue;
                if (null != vd) {
                    Load(vd);
                }
                else {
                    var fd = func.AsFunction;
                    Load(fd);
                }
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
                noWait = cfg.m_NoWait.Calc().GetBool();
            }
            BatchScript.ProcessStartOption option = new BatchScript.ProcessStartOption();
            if (null != cfg.m_UseShellExecute) {
                option.UseShellExecute = cfg.m_UseShellExecute.Calc().GetBool();
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
                option.LoadUserProfile = cfg.m_LoadUserProfile.Calc().GetBool();
            }
            if (null != cfg.m_WindowStyle) {
                var str = cfg.m_WindowStyle.Calc().AsString;
                ProcessWindowStyle style;
                if (Enum.TryParse(str, out style)) {
                    option.WindowStyle = style;
                }
            }
            if (null != cfg.m_NewWindow) {
                option.NewWindow = cfg.m_NewWindow.Calc().GetBool();
            }
            if (null != cfg.m_ErrorDialog) {
                option.ErrorDialog = cfg.m_ErrorDialog.Calc().GetBool();
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
                    int codePage = v.GetInt();
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
                    outputIx = cfg.m_OutputOptArg.Calc().GetInt();
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
                    errorIx = cfg.m_ErrorOptArg.Calc().GetInt();
                errorBuilder = new StringBuilder();
            }
            if (null != cfg.m_RedirectToConsole) {
                var v = cfg.m_RedirectToConsole.Calc();
                redirectToConsole = v.GetBool();
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
                    noWait = cfg.m_NoWait.Calc().GetBool();
                }
                BatchScript.ProcessStartOption option = new BatchScript.ProcessStartOption();
                if (null != cfg.m_UseShellExecute) {
                    option.UseShellExecute = cfg.m_UseShellExecute.Calc().GetBool();
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
                    option.LoadUserProfile = cfg.m_LoadUserProfile.Calc().GetBool();
                }
                if (null != cfg.m_WindowStyle) {
                    var str = cfg.m_WindowStyle.Calc().AsString;
                    ProcessWindowStyle style;
                    if (Enum.TryParse(str, out style)) {
                        option.WindowStyle = style;
                    }
                }
                if (null != cfg.m_NewWindow) {
                    option.NewWindow = cfg.m_NewWindow.Calc().GetBool();
                }
                if (null != cfg.m_ErrorDialog) {
                    option.ErrorDialog = cfg.m_ErrorDialog.Calc().GetBool();
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
                        int codePage = v.GetInt();
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
                        outputIx = cfg.m_OutputOptArg.Calc().GetInt();
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
                        errorIx = cfg.m_ErrorOptArg.Calc().GetInt();
                    errorBuilder = new StringBuilder();
                }
                if (null != cfg.m_RedirectToConsole) {
                    var v = cfg.m_RedirectToConsole.Calc();
                    redirectToConsole = v.GetBool();
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

    internal sealed class KillExp : SimpleExpressionBase
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
                    int pid = vObj.GetInt();
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

    internal sealed class KillMeExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            int ret = 0;
            Process p = Process.GetCurrentProcess();
            if (null != p) {
                ret = p.Id;
                int exitCode = 0;
                if (operands.Count >= 1) {
                    exitCode = operands[0].GetInt();
                }
                if (BatchScript.FileEchoOn) {
                    Console.WriteLine("killme {0}[pid:{1},session id:{2}] exit code:{3}", p.ProcessName, p.Id, p.SessionId, exitCode);
                }
                Environment.Exit(exitCode);
            }
            return ret;
        }
    }

    internal sealed class GetCurrentProcessIdExp : SimpleExpressionBase
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

    internal sealed class ListProcessesExp : SimpleExpressionBase
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

    internal sealed class GetScriptDirectoryExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return BatchScript.ScriptDirectory;
        }
    }

    internal sealed class PauseExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var info = Console.ReadKey(true);
            return (int)info.KeyChar;
        }
    }

    internal sealed class WaitExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var ret = CalculatorValue.NullObject;
            if (operands.Count >= 1) {
                var time = operands[0].GetInt();
                Thread.Sleep(time);
                ret = time;
            }
            return ret;
        }
    }

    internal sealed class WaitAllExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            var tasks = BatchScript.Tasks;
            int timeout = -1;
            if (operands.Count >= 1) {
                timeout = operands[0].GetInt();
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

    internal sealed class ClearExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            Console.Clear();
            return CalculatorValue.NullObject;
        }
    }

    internal sealed class WriteExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 1) {
                var obj = operands[0].GetObject();
                if (null != obj) {
                    var fmt = obj as string;
                    if (operands.Count > 1 && null != fmt) {
                        ArrayList arrayList = new ArrayList();
                        for (int i = 1; i < operands.Count; ++i) {
                            arrayList.Add(operands[i].GetObject());
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

    internal sealed class WriteBlockExp : AbstractExpression
    {
        protected override CalculatorValue DoCalc()
        {
            if (null != m_BeginChars && null != m_EndChars) {
                var c1 = m_BeginChars.Calc().ToString();
                var c2 = m_EndChars.Calc().ToString();
                if (c1.Length == 2 && c2.Length == 2) {
                    m_BeginFirst = c1[0];
                    m_BeginSecond = c1[1];
                    m_EndFirst = c2[0];
                    m_EndSecond = c2[1];
                }
            }
            Console.Write(BlockExp.CalcBlockString(m_Block, Calculator, m_OutputBuilder, m_TempBuilder, m_BeginFirst, m_BeginSecond, m_EndFirst, m_EndSecond));
            return CalculatorValue.NullObject;
        }
        protected override bool Load(Dsl.FunctionData funcData)
        {
            if (funcData.HaveExternScript()) {
                m_Block = funcData.GetParamId(0);
            }
            if (funcData.IsHighOrder) {
                var callData = funcData.LowerOrderFunction;
                if (callData.GetParamNum() == 2) {
                    m_BeginChars = Calculator.Load(callData.GetParam(0));
                    m_EndChars = Calculator.Load(callData.GetParam(1));
                }
            }
            return true;
        }

        private IExpression m_BeginChars = null;
        private IExpression m_EndChars = null;

        private string m_Block = String.Empty;
        private StringBuilder m_OutputBuilder = new StringBuilder();
        private StringBuilder m_TempBuilder = new StringBuilder();
        private char m_BeginFirst = BlockExp.c_BeginFirst;
        private char m_BeginSecond = BlockExp.c_BeginSecond;
        private char m_EndFirst = BlockExp.c_EndFirst;
        private char m_EndSecond = BlockExp.c_EndSecond;
    }

    internal sealed class BlockExp : AbstractExpression
    {
        protected override CalculatorValue DoCalc()
        {
            if (null != m_BeginChars && null != m_EndChars) {
                var c1 = m_BeginChars.Calc().ToString();
                var c2 = m_EndChars.Calc().ToString();
                if (c1.Length == 2 && c2.Length == 2) {
                    m_BeginFirst = c1[0];
                    m_BeginSecond = c1[1];
                    m_EndFirst = c2[0];
                    m_EndSecond = c2[1];
                }
            }
            return CalculatorValue.From(CalcBlockString(m_Block, Calculator, m_OutputBuilder, m_TempBuilder, m_BeginFirst, m_BeginSecond, m_EndFirst, m_EndSecond));
        }
        protected override bool Load(Dsl.FunctionData funcData)
        {
            if (funcData.HaveExternScript()) {
                m_Block = funcData.GetParamId(0);
            }
            if (funcData.IsHighOrder) {
                var callData = funcData.LowerOrderFunction;
                if (callData.GetParamNum() == 2) {
                    m_BeginChars = Calculator.Load(callData.GetParam(0));
                    m_EndChars = Calculator.Load(callData.GetParam(1));
                }
            }
            return true;
        }

        private IExpression m_BeginChars = null;
        private IExpression m_EndChars = null;

        private string m_Block = String.Empty;
        private StringBuilder m_OutputBuilder = new StringBuilder();
        private StringBuilder m_TempBuilder = new StringBuilder();
        private char m_BeginFirst = c_BeginFirst;
        private char m_BeginSecond = c_BeginSecond;
        private char m_EndFirst = c_EndFirst;
        private char m_EndSecond = c_EndSecond;

        internal static string CalcBlockString(string block, DslCalculator calculator, StringBuilder outputBuilder, StringBuilder tempBuilder, char beginFirst, char beginSecond, char endFirst, char endSecond)
        {
            outputBuilder.Length = 0;
            for (int i = 0; i < block.Length; ++i) {
                char c = block[i];
                char nc = '\0';
                if (i + 1 < block.Length) {
                    nc = block[i + 1];
                }
                if (c == beginFirst && nc == beginSecond) {
                    ++i;
                    ++i;
                    tempBuilder.Length = 0;
                    for (int j = i; j < block.Length; ++j) {
                        c = block[j];
                        nc = '\0';
                        if (j + 1 < block.Length) {
                            nc = block[j + 1];
                        }
                        if (c == endFirst && nc == endSecond) {
                            string varNameOrCode = tempBuilder.ToString().Trim();
                            CalculatorValue val;
                            if (calculator.TryGetVariable(varNameOrCode, out val)) {
                                outputBuilder.Append(val.ToString());
                            }
                            else {
                                val = BatchScript.EvalAndRun(varNameOrCode);
                                outputBuilder.Append(val.ToString());
                            }
                            i = j + 1;
                            break;
                        }
                        else {
                            tempBuilder.Append(c);
                        }
                    }
                }
                else {
                    outputBuilder.Append(c);
                }
            }
            return outputBuilder.ToString();
        }
        internal const char c_BeginFirst = '{';
        internal const char c_BeginSecond = '%';
        internal const char c_EndFirst = '%';
        internal const char c_EndSecond = '}';
    }

    internal sealed class ReadLineExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return Console.ReadLine();
        }
    }

    internal sealed class ReadExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            bool nodisplay = false;
            if (operands.Count >= 1) {
                nodisplay = operands[0].GetBool();
            }
            var info = Console.ReadKey(nodisplay);
            return info.KeyChar;
        }
    }

    internal sealed class BeepExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                if (operands.Count >= 2) {
                    int f = operands[0].GetInt();
                    int d = operands[1].GetInt();
                    Console.Beep(f, d);
                }
                else {
                    Console.Beep();
                }
            }
            return CalculatorValue.NullObject;
        }
    }

    internal sealed class GetTitleExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                return Console.Title;
            }
            return string.Empty;
        }
    }

    internal sealed class SetTitleExp : SimpleExpressionBase
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

    internal sealed class GetBufferWidthExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return Console.BufferWidth;
        }
    }

    internal sealed class GetBufferHeightExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return Console.BufferHeight;
        }
    }

    internal sealed class SetBufferSizeExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                if (operands.Count >= 2) {
                    int w = operands[0].GetInt();
                    int h = operands[1].GetInt();
                    Console.SetBufferSize(w, h);
                }
            }
            return CalculatorValue.NullObject;
        }
    }

    internal sealed class GetCursorLeftExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return Console.CursorLeft;
        }
    }

    internal sealed class GetCursorTopExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return Console.CursorTop;
        }
    }

    internal sealed class SetCursorPosExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 2) {
                int left = operands[0].GetInt();
                int top = operands[1].GetInt();
                Console.SetCursorPosition(left, top);
            }
            return CalculatorValue.NullObject;
        }
    }

    internal sealed class GetBgColorExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            //Enum.GetName(typeof(ConsoleColor), Console.BackgroundColor);
            return Console.BackgroundColor.ToString();
        }
    }

    internal sealed class SetBgColorExp : SimpleExpressionBase
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

    internal sealed class GetFgColorExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            //Enum.GetName(typeof(ConsoleColor), Console.ForegroundColor);
            return Console.ForegroundColor.ToString();
        }
    }

    internal sealed class SetFgColorExp : SimpleExpressionBase
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

    internal sealed class ResetColorExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            Console.ResetColor();
            return CalculatorValue.NullObject;
        }
    }

    internal sealed class SetEncodingExp : SimpleExpressionBase
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

    internal sealed class GetInputEncodingExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return CalculatorValue.FromObject(Console.InputEncoding);
        }
    }

    internal sealed class GetOutputEncodingExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return CalculatorValue.FromObject(Console.OutputEncoding);
        }
    }

    internal sealed class ConsoleExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return typeof(Console);
        }
    }

    internal sealed class EncodingExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return typeof(Encoding);
        }
    }

    internal sealed class EnvironmentExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            return typeof(Environment);
        }
    }

    internal sealed class ReadAllLinesExp : SimpleExpressionBase
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

    internal sealed class WriteAllLinesExp : SimpleExpressionBase
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

    internal sealed class ReadAllTextExp : SimpleExpressionBase
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

    internal sealed class WriteAllTextExp : SimpleExpressionBase
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

    internal sealed class WaitStartIntervalExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 1) {
                var v = operands[0].GetInt();
                BatchScript.CheckStartInterval = v;
            }
            return BatchScript.CheckStartInterval;
        }
    }

    internal sealed class RegReadExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 2) {
                string keyName = operands[0].AsString;
                string valName = operands[1].AsString;
                CalculatorValue defVal = CalculatorValue.NullObject;
                if (operands.Count >= 3)
                    defVal = operands[2];
                if (!string.IsNullOrEmpty(keyName) && !string.IsNullOrEmpty(valName)) {
                    object val = Registry.GetValue(keyName, valName, defVal.GetObject());
                    return CalculatorValue.FromObject(val);
                }
            }
            return CalculatorValue.NullObject;
        }
    }
    internal sealed class RegWriteExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 3) {
                string keyName = operands[0].AsString;
                string valName = operands[1].AsString;
                object val = operands[2].GetObject();
                CalculatorValue valKind = CalculatorValue.From((int)RegistryValueKind.Unknown);
                if (operands.Count >= 4)
                    valKind = operands[3];
                if (!string.IsNullOrEmpty(keyName) && !string.IsNullOrEmpty(valName)) {
                    Registry.SetValue(keyName, valName, val, (RegistryValueKind)valKind.GetInt());
                    return CalculatorValue.From(true);
                }
            }
            return CalculatorValue.From(false);
        }
        /*
        public enum RegistryValueKind
        {
            String = 1,
            ExpandString = 2,
            Binary = 3,
            DWord = 4,
            MultiString = 7,
            QWord = 11,
            Unknown = 0,
            [ComVisible(false)]
            None = -1
        }
        */
    }
    internal sealed class RegDeleteExp : SimpleExpressionBase
    {
        protected override CalculatorValue OnCalc(IList<CalculatorValue> operands)
        {
            if (operands.Count >= 1) {
                string keyName = operands[0].AsString;
                bool delVal = false;
                string valName = string.Empty;
                if (operands.Count >= 2) {
                    delVal = true;
                    valName = operands[1].AsString;
                }
                if (!string.IsNullOrEmpty(keyName) && !string.IsNullOrEmpty(valName)) {
                    var regKey = GetBaseKeyFromKeyName(keyName, out var subKeyName);
                    if (null != regKey) {
                        if (delVal)
                            regKey.DeleteValue(valName, false);
                        else
                            regKey.DeleteSubKeyTree(subKeyName, false);
                        return CalculatorValue.From(true);
                    }
                }
            }
            return CalculatorValue.From(false);
        }
        private static RegistryKey GetBaseKeyFromKeyName(string keyName, out string subKeyName)
        {
            if (keyName == null) {
                throw new ArgumentNullException("keyName");
            }
            int num = keyName.IndexOf('\\');
            string text = ((num == -1) ? keyName.ToUpper(CultureInfo.InvariantCulture) : keyName.Substring(0, num).ToUpper(CultureInfo.InvariantCulture));
            s_KeyMap.TryGetValue(text, out var regKey);
            if (num == -1 || num == keyName.Length) {
                subKeyName = string.Empty;
            }
            else {
                subKeyName = keyName.Substring(num + 1, keyName.Length - num - 1);
            }
            return regKey;
        }

        private static Dictionary<string, RegistryKey> s_KeyMap = new Dictionary<string, RegistryKey> {
        { "HKEY_CURRENT_USER", Registry.CurrentUser },
        {"HKEY_LOCAL_MACHINE", Registry.LocalMachine},
        {"HKEY_CLASSES_ROOT", Registry.ClassesRoot},
        {"HKEY_USERS", Registry.Users},
        {"HKEY_PERFORMANCE_DATA", Registry.PerformanceData},
        {"HKEY_CURRENT_CONFIG", Registry.CurrentConfig},
        //{"HKEY_DYN_DATA", Registry.DynData},
    };
    }

    internal sealed class BatchScript
    {
        internal static SortedList<string, string> ApiDocs
        {
            get { return s_Calculator.ApiDocs; }
        }
        internal static bool TimeStatisticOn
        {
            get { return s_TimeStatisticOn; }
            set { s_TimeStatisticOn = value; }
        }
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
            if (args.Length == 0)
                Console.WriteLine(fmt);
            else
                Console.WriteLine(fmt, args);
        }
        internal static void Log(object arg)
        {
            Console.WriteLine("{0}", arg);
        }
        internal static void Init()
        {
#if NET || NETSTANDARD
            var provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
#endif

            s_Calculator.OnLog = msg => { Log(msg); };
            s_Calculator.Init();

            //注册Gm命令
            s_Calculator.Register("timestat", "timestat(bool) or timestat() api", new ExpressionFactoryHelper<TimeStatisticOnExp>());
            s_Calculator.Register("fileecho", "fileecho(bool) or fileecho() api", new ExpressionFactoryHelper<FileEchoExp>());
            s_Calculator.Register("listdirs", "listdirs(dir,filter_list_or_str_1,filter_list_or_str_2,...) api", new ExpressionFactoryHelper<ListDirectoriesExp>());
            s_Calculator.Register("listfiles", "listfiles(dir,filter_list_or_str_1,filter_list_or_str_2,...) api", new ExpressionFactoryHelper<ListFilesExp>());
            s_Calculator.Register("listalldirs", "listalldirs(dir,filter_list_or_str_1,filter_list_or_str_2,...) api", new ExpressionFactoryHelper<ListAllDirectoriesExp>());
            s_Calculator.Register("listallfiles", "listallfiles(dir,filter_list_or_str_1,filter_list_or_str_2,...) api", new ExpressionFactoryHelper<ListAllFilesExp>());
            s_Calculator.Register("direxist", "direxist(dir) api", new ExpressionFactoryHelper<DirectoryExistExp>());
            s_Calculator.Register("fileexist", "fileexist(file) api", new ExpressionFactoryHelper<FileExistExp>());
            s_Calculator.Register("createdir", "createdir(dir) api", new ExpressionFactoryHelper<CreateDirectoryExp>());
            s_Calculator.Register("copydir", "copydir(dir1,dir2,filter_list_or_str_1,filter_list_or_str_2,...) api, include subdir", new ExpressionFactoryHelper<CopyDirectoryExp>());
            s_Calculator.Register("movedir", "movedir(dir1,dir2) api", new ExpressionFactoryHelper<MoveDirectoryExp>());
            s_Calculator.Register("deletedir", "deletedir(dir) api", new ExpressionFactoryHelper<DeleteDirectoryExp>());
            s_Calculator.Register("copyfile", "copyfile(file1,file2) api", new ExpressionFactoryHelper<CopyFileExp>());
            s_Calculator.Register("copyfiles", "copyfiles(dir1,dir2,filter_list_or_str_1,filter_list_or_str_2,...) api, dont include subdir", new ExpressionFactoryHelper<CopyFilesExp>());
            s_Calculator.Register("movefile", "movefile(file1,file2) api", new ExpressionFactoryHelper<MoveFileExp>());
            s_Calculator.Register("deletefile", "deletefile(file) api", new ExpressionFactoryHelper<DeleteFileExp>());
            s_Calculator.Register("deletefiles", "deletefiles(dir,filter_list_or_str_1,filter_list_or_str_2,...) api, dont include subdir", new ExpressionFactoryHelper<DeleteFilesExp>());
            s_Calculator.Register("deleteallfiles", "deleteallfiles(dir,filter_list_or_str_1,filter_list_or_str_2,...) api, include subdir", new ExpressionFactoryHelper<DeleteAllFilesExp>());
            s_Calculator.Register("getfileinfo", "getfileinfo(file) api", new ExpressionFactoryHelper<GetFileInfoExp>());
            s_Calculator.Register("getdirinfo", "getdirinfo(dir) api", new ExpressionFactoryHelper<GetDirectoryInfoExp>());
            s_Calculator.Register("getdriveinfo", "getdriveinfo(drive) api", new ExpressionFactoryHelper<GetDriveInfoExp>());
            s_Calculator.Register("getdrivesinfo", "getdrivesinfo() api", new ExpressionFactoryHelper<GetDrivesInfoExp>());
            s_Calculator.Register("grep", "grep(lines[,regex]) api", new ExpressionFactoryHelper<GrepExp>());
            s_Calculator.Register("subst", "subst(lines,regex,subst[,count]) api, count is the max count of per subst", new ExpressionFactoryHelper<SubstExp>());
            s_Calculator.Register("awk", "awk(lines,scp[,removeEmpties,sep1,sep2,...]) api", new ExpressionFactoryHelper<AwkExp>());
            s_Calculator.Register("process", "process(file,arg_str) or process(file,arg_str){[options;]} api", new ExpressionFactoryHelper<CommandExp>());
            s_Calculator.Register("command", "command{win{:cmd_str:};unix{:cmd_str:};common{:cmd_str:};[options;]} api", new ExpressionFactoryHelper<CommandExp>());
            s_Calculator.Register("kill", "kill(name_or_pid) api", new ExpressionFactoryHelper<KillExp>());
            s_Calculator.Register("killme", "killme([exit_code]) api", new ExpressionFactoryHelper<KillMeExp>());
            s_Calculator.Register("pid", "pid() api", new ExpressionFactoryHelper<GetCurrentProcessIdExp>());
            s_Calculator.Register("plist", "plist([filter]) api, return list", new ExpressionFactoryHelper<ListProcessesExp>());
            s_Calculator.Register("getscriptdir", "getscriptdir() api", new ExpressionFactoryHelper<GetScriptDirectoryExp>());
            s_Calculator.Register("pause", "pause() api", new ExpressionFactoryHelper<PauseExp>());
            s_Calculator.Register("wait", "wait(time) api", new ExpressionFactoryHelper<WaitExp>());
            s_Calculator.Register("waitall", "waitall([timeout]) api, wait all task to exit", new ExpressionFactoryHelper<WaitAllExp>());
            s_Calculator.Register("clear", "clear() api, clear console", new ExpressionFactoryHelper<ClearExp>());
            s_Calculator.Register("write", "write(fmt,arg1,arg2,....) api, Console.Write", new ExpressionFactoryHelper<WriteExp>());
            s_Calculator.Register("writeblock", "writeblock{:txt:} or writeblock(two_chars_begin,two_chars_end){:txt:} api, Console.Write with macro expand, def begin is {% end is %}", new ExpressionFactoryHelper<WriteBlockExp>());
            s_Calculator.Register("block", "block{:txt:} or block(two_chars_begin,two_chars_end){:txt:} api, macro expand, def begin is {% end is %}", new ExpressionFactoryHelper<BlockExp>());
            s_Calculator.Register("readline", "readline() api, Console.ReadLine", new ExpressionFactoryHelper<ReadLineExp>());
            s_Calculator.Register("read", "read([nodisplay]) api, Console.Read", new ExpressionFactoryHelper<ReadExp>());
            s_Calculator.Register("beep", "beep([frequence,duration]) api, Console.Beep, only on win32", new ExpressionFactoryHelper<BeepExp>());
            s_Calculator.Register("gettitle", "gettitle() api, Console.Title, only on win32", new ExpressionFactoryHelper<GetTitleExp>());
            s_Calculator.Register("settitle", "settitle(title) api", new ExpressionFactoryHelper<SetTitleExp>());
            s_Calculator.Register("getbufferwidth", "getbufferwidth() api", new ExpressionFactoryHelper<GetBufferWidthExp>());
            s_Calculator.Register("getbufferheight", "getbufferheight() api", new ExpressionFactoryHelper<GetBufferHeightExp>());
            s_Calculator.Register("setbuffersize", "setbuffersize(width,height) api", new ExpressionFactoryHelper<SetBufferSizeExp>());
            s_Calculator.Register("getcursorleft", "getcursorleft() api", new ExpressionFactoryHelper<GetCursorLeftExp>());
            s_Calculator.Register("getcursortop", "getcursortop() api", new ExpressionFactoryHelper<GetCursorTopExp>());
            s_Calculator.Register("setcursorpos", "setcursorpos(left,top) api", new ExpressionFactoryHelper<SetCursorPosExp>());
            s_Calculator.Register("getbgcolor", "getbgcolor() api, return str", new ExpressionFactoryHelper<GetBgColorExp>());
            s_Calculator.Register("setbgcolor", "setbgcolor(color_name) api, color:Black,DarkBlue,DarkGreen,DarkCyan,DarkRed,DarkMagenta,DarkYellow,Gray,DarkGray,Blue,Green,Cyan,Red,Magenta,Yellow,White", new ExpressionFactoryHelper<SetBgColorExp>());
            s_Calculator.Register("getfgcolor", "getfgcolor() api, return str", new ExpressionFactoryHelper<GetFgColorExp>());
            s_Calculator.Register("setfgcolor", "setfgcolor(color_name) api, color:Black,DarkBlue,DarkGreen,DarkCyan,DarkRed,DarkMagenta,DarkYellow,Gray,DarkGray,Blue,Green,Cyan,Red,Magenta,Yellow,White", new ExpressionFactoryHelper<SetFgColorExp>());
            s_Calculator.Register("resetcolor", "resetcolor() api", new ExpressionFactoryHelper<ResetColorExp>());
            s_Calculator.Register("setencoding", "setencoding([input[,output]]) api, def is UTF8", new ExpressionFactoryHelper<SetEncodingExp>());
            s_Calculator.Register("getinputencoding", "getinputencoding() api, return Encoding", new ExpressionFactoryHelper<GetInputEncodingExp>());
            s_Calculator.Register("getoutputencoding", "getoutputencoding() api, return Encoding", new ExpressionFactoryHelper<GetOutputEncodingExp>());
            s_Calculator.Register("console", "console() api, return typeof(Console)", new ExpressionFactoryHelper<ConsoleExp>());
            s_Calculator.Register("encoding", "encoding() api, return typeof(Encoding)", new ExpressionFactoryHelper<EncodingExp>());
            s_Calculator.Register("env", "env() api, return typeof(Environment)", new ExpressionFactoryHelper<EnvironmentExp>());
            s_Calculator.Register("readalllines", "readalllines(file[,encoding]) api", new ExpressionFactoryHelper<ReadAllLinesExp>());
            s_Calculator.Register("writealllines", "writealllines(file,lines[,encoding]) api", new ExpressionFactoryHelper<WriteAllLinesExp>());
            s_Calculator.Register("readalltext", "readalltext(file[,encoding]) api", new ExpressionFactoryHelper<ReadAllTextExp>());
            s_Calculator.Register("writealltext", "writealltext(file,txt[,encoding]) api", new ExpressionFactoryHelper<WriteAllTextExp>());
            s_Calculator.Register("waitstartinterval", "waitstartinterval(time) or waitstartinterval() api, used in Task.Wait for process/command", new ExpressionFactoryHelper<WaitStartIntervalExp>());
            s_Calculator.Register("regread", "regread(keyname,valname[,defval]) api, root:HKEY_CURRENT_USER|HKEY_LOCAL_MACHINE|HKEY_CLASSES_ROOT|HKEY_USERS|HKEY_PERFORMANCE_DATA|HKEY_CURRENT_CONFIG", new ExpressionFactoryHelper<RegReadExp>());
            s_Calculator.Register("regwrite", "regwrite(keyname,valname,val[,val_kind]) api, root:HKEY_CURRENT_USER|HKEY_LOCAL_MACHINE|HKEY_CLASSES_ROOT|HKEY_USERS|HKEY_PERFORMANCE_DATA|HKEY_CURRENT_CONFIG, val_kind:0-unk,1-str,2-exstr,3-bin,4-dword,7-multistr,11-qword", new ExpressionFactoryHelper<RegWriteExp>());
            s_Calculator.Register("regdelete", "regdelete(keyname[,valname]) api, root:HKEY_CURRENT_USER|HKEY_LOCAL_MACHINE|HKEY_CLASSES_ROOT|HKEY_USERS|HKEY_PERFORMANCE_DATA|HKEY_CURRENT_CONFIG", new ExpressionFactoryHelper<RegDeleteExp>());
        }
        internal static void Register(string name, string doc, IExpressionFactory factory)
        {
            s_Calculator.Register(name, doc, factory);
        }
        internal static void SetOnTryGetVariable(DslCalculator.TryGetVariableDelegation callback)
        {
            s_Calculator.OnTryGetVariable = callback;
        }
        internal static void SetOnTrySetVariable(DslCalculator.TrySetVariableDelegation callback)
        {
            s_Calculator.OnTrySetVariable = callback;
        }
        internal static void SetOnLoadFailback(DslCalculator.LoadFailbackDelegation callback)
        {
            s_Calculator.OnLoadFailback = callback;
        }
        internal static CalculatorValue Run(string scpFile, List<CalculatorValue> args)
        {
            var r = CalculatorValue.NullObject;
            bool redirect = true;
            var vargs = s_Calculator.NewCalculatorValueList();
            vargs.AddRange(args);
            while (redirect) {
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
        internal static CalculatorValue EvalAndRun(string code)
        {
            CalculatorValue r = CalculatorValue.EmptyString;
            var file = new Dsl.DslFile();
            if (file.LoadFromString(code, msg => { Log(msg); })) {
                r = EvalAndRun(file.DslInfos);
            }
            return r;
        }
        internal static CalculatorValue EvalAndRun(params ISyntaxComponent[] expressions)
        {
            IList<ISyntaxComponent> exps = expressions;
            return EvalAndRun(exps);
        }
        internal static CalculatorValue EvalAndRun(IList<ISyntaxComponent> expressions)
        {
            CalculatorValue r = CalculatorValue.EmptyString;
            List<IExpression> exps = new List<IExpression>();
            s_Calculator.LoadDsl(expressions, exps);
            r = s_Calculator.CalcInCurrentContext(exps);
            return r;
        }
        internal static string EvalAsFunc(string code, IList<string> argNames)
        {
            string id = System.Guid.NewGuid().ToString();
            string procCode = string.Format("script{{ {0}; }};", code);
            var file = new Dsl.DslFile();
            if (file.LoadFromString(procCode, msg => { Log(msg); })) {
                var func = file.DslInfos[0] as Dsl.FunctionData;
                Debug.Assert(null != func);
                s_Calculator.LoadDsl(id, argNames, func);
                return id;
            }
            return string.Empty;
        }
        internal static string EvalAsFunc(Dsl.FunctionData func, IList<string> argNames)
        {
            string id = System.Guid.NewGuid().ToString();
            Debug.Assert(null != func);
            s_Calculator.LoadDsl(id, argNames, func);
            return id;
        }
        internal static List<CalculatorValue> NewCalculatorValueList()
        {
            return s_Calculator.NewCalculatorValueList();
        }
        internal static void RecycleCalculatorValueList(List<CalculatorValue> list)
        {
            s_Calculator.RecycleCalculatorValueList(list);
        }
        internal static CalculatorValue Call(string func)
        {
            var args = NewCalculatorValueList();
            var r = Call(func, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static CalculatorValue Call(string func, CalculatorValue arg1)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            var r = Call(func, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static CalculatorValue Call(string func, CalculatorValue arg1, CalculatorValue arg2)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            args.Add(arg2);
            var r = Call(func, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static CalculatorValue Call(string func, CalculatorValue arg1, CalculatorValue arg2, CalculatorValue arg3)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            args.Add(arg2);
            args.Add(arg3);
            var r = Call(func, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static CalculatorValue Call(string func, CalculatorValue arg1, CalculatorValue arg2, CalculatorValue arg3, CalculatorValue arg4)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            args.Add(arg2);
            args.Add(arg3);
            args.Add(arg4);
            var r = Call(func, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static CalculatorValue Call(string func, CalculatorValue arg1, CalculatorValue arg2, CalculatorValue arg3, CalculatorValue arg4, CalculatorValue arg5)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            args.Add(arg2);
            args.Add(arg3);
            args.Add(arg4);
            args.Add(arg5);
            var r = Call(func, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static CalculatorValue Call(string func, List<CalculatorValue> args)
        {
            var r = s_Calculator.Calc(func, args);
            return r;
        }
        internal static Encoding GetEncoding(CalculatorValue v)
        {
            try {
                var name = v.AsString;
                if (null != name) {
                    return Encoding.GetEncoding(name);
                }
                else if (v.IsInteger) {
                    int codePage = v.GetInt();
                    return Encoding.GetEncoding(codePage);
                }
                else {
                    return Encoding.UTF8;
                }
            }
            catch {
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
                if (null != option.UserName) {
                    psi.UserName = option.UserName;
                }
                if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                    if (null != option.Domain) {
                        psi.Domain = option.Domain;
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
                }
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

        internal sealed class ProcessStartOption
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

        private static bool s_TimeStatisticOn = false;
        private static bool s_FileEchoOn = false;
        private static int s_CheckStartInterval = 500;
        private static string s_ScriptDirectory = string.Empty;
        private static List<Task<int>> s_Tasks = new List<Task<int>>();
        private static DslCalculator s_Calculator = new DslCalculator();
    }
}
#pragma warning restore 8600,8601,8602,8603,8604,8618,8619,8620,8625,CA1416
