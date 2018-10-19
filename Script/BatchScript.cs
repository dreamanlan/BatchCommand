﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using System.IO;
using Calculator;
using Dsl;

namespace BatchCommand
{
    internal class FileEchoExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            if (operands.Count >= 1) {
                BatchScript.FileEchoOn = (bool)Convert.ChangeType(operands[0], typeof(bool));
            }
            return BatchScript.FileEchoOn;
        }
    }

    internal class ListDirectoriesExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            object ret = null;
            if (operands.Count >= 1) {
                var baseDir = operands[0] as string;
                baseDir = Environment.ExpandEnvironmentVariables(baseDir);
                IList<string> filterList = new string[] { "*" };
                if (operands.Count >= 2) {
                    var strList = operands[1] as IList<string>;
                    if (null != strList && operands.Count == 2) {
                        filterList = strList;
                    } else {
                        var list = new List<string>();
                        for (int i = 1; i < operands.Count; ++i) {
                            var str = operands[i] as string;
                            if (null != str) {
                                list.Add(str);
                            }
                        }
                        filterList = list;
                    }
                }
                if (null != baseDir && Directory.Exists(baseDir)) {
                    var fullList = new List<string>();
                    foreach (var filter in filterList) {
                        var list = Directory.GetDirectories(baseDir, filter, SearchOption.AllDirectories);
                        fullList.AddRange(list);
                    }
                    ret = fullList;
                }
            }
            return ret;
        }
    }

    internal class ListFilesExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            object ret = null;
            if (operands.Count >= 1) {
                var baseDir = operands[0] as string;
                baseDir = Environment.ExpandEnvironmentVariables(baseDir);
                IList<string> filterList = new string[] { "*" };
                if (operands.Count >= 2) {
                    var strList = operands[1] as IList<string>;
                    if (null != strList && operands.Count == 2) {
                        filterList = strList;
                    } else {
                        var list = new List<string>();
                        for (int i = 1; i < operands.Count; ++i) {
                            var str = operands[i] as string;
                            if (null != str) {
                                list.Add(str);
                            }
                        }
                        filterList = list;
                    }
                }
                if (null != baseDir && Directory.Exists(baseDir)) {
                    var fullList = new List<string>();
                    foreach (var filter in filterList) {
                        var list = Directory.GetFiles(baseDir, filter, SearchOption.AllDirectories);
                        fullList.AddRange(list);
                    }
                    ret = fullList;
                }
            }
            return ret;
        }
    }

    internal class DirectoryExistExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            object ret = null;
            if (operands.Count >= 1) {
                var dir = operands[0] as string;
                dir = Environment.ExpandEnvironmentVariables(dir);
                ret = Directory.Exists(dir);
            }
            return ret;
        }
    }

    internal class FileExistExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            object ret = null;
            if (operands.Count >= 1) {
                var file = operands[0] as string;
                file = Environment.ExpandEnvironmentVariables(file);
                ret = File.Exists(file);
            }
            return ret;
        }
    }

    internal class CreateDirectoryExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            bool ret = false;
            if (operands.Count >= 1) {
                var dir = operands[0] as string;
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

    internal class CopyDirectoryExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            int ct = 0;
            if (operands.Count >= 2) {
                var dir1 = operands[0] as string;
                var dir2 = operands[1] as string;
                dir1 = Environment.ExpandEnvironmentVariables(dir1);
                dir2 = Environment.ExpandEnvironmentVariables(dir2);
                List<string> filterAndNewExts = new List<string>();
                for(int i = 2; i < operands.Count; ++i) {
                    var str = operands[i] as string;
                    if (null != str) {
                        filterAndNewExts.Add(str);
                    }
                }
                CopyFolder(dir1, dir2, filterAndNewExts, ref ct);
            }
            return ct;
        }
        private static void CopyFolder(string from, string to, IList<string> filterAndNewExts, ref int ct)
        {
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);
            // 子文件夹
            foreach (string sub in Directory.GetDirectories(from)) {
                CopyFolder(sub, Path.Combine(to, Path.GetFileName(sub)), filterAndNewExts, ref ct);
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
                        Console.WriteLine("copy file {0} -> {1}", file, targetFile);
                    }
                }
            }
        }
    }

    internal class MoveDirectoryExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            bool ret = false;
            if (operands.Count >= 2) {
                var dir1 = operands[0] as string;
                var dir2 = operands[1] as string;
                dir1 = Environment.ExpandEnvironmentVariables(dir1);
                dir2 = Environment.ExpandEnvironmentVariables(dir2);
                if (Directory.Exists(dir1)) {
                    if(Directory.Exists(dir2)){
                        Directory.Delete(dir2);
                    }
                    Directory.Move(dir1, dir2);
                    ret = true;
                    
                    if (BatchScript.FileEchoOn) {
                        Console.WriteLine("move directory {0} -> {1}", dir1, dir2);
                    }
                }
            }
            return ret;
        }
    }

    internal class DeleteDirectoryExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            bool ret = false;
            if (operands.Count >= 1) {
                var dir = operands[0] as string;
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

    internal class CopyFileExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            bool ret = false;
            if (operands.Count >= 2) {
                var file1 = operands[0] as string;
                var file2 = operands[1] as string;
                file1 = Environment.ExpandEnvironmentVariables(file1);
                file2 = Environment.ExpandEnvironmentVariables(file2);
                if (File.Exists(file1)) {
                    File.Copy(file1, file2, true);
                    ret = true;

                    if (BatchScript.FileEchoOn) {
                        Console.WriteLine("copy file {0} -> {1}", file1, file2);
                    }
                }
            }
            return ret;
        }
    }

    internal class MoveFileExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            bool ret = false;
            if (operands.Count >= 2) {
                var file1 = operands[0] as string;
                var file2 = operands[1] as string;
                file1 = Environment.ExpandEnvironmentVariables(file1);
                file2 = Environment.ExpandEnvironmentVariables(file2);
                if (File.Exists(file1)) {
                    if (File.Exists(file2)) {
                        File.Delete(file2);
                    }
                    File.Move(file1, file2);
                    ret = true;

                    if (BatchScript.FileEchoOn) {
                        Console.WriteLine("move file {0} -> {1}", file1, file2);
                    }
                }
            }
            return ret;
        }
    }

    internal class DeleteFileExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            bool ret = false;
            if (operands.Count >= 1) {
                var file = operands[0] as string;
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

    internal class DeleteFilesExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            int ct = 0;
            if (operands.Count >= 2) {
                var dir = operands[0] as string;
                var filter = operands[1] as string;
                dir = Environment.ExpandEnvironmentVariables(dir);
                foreach (string file in Directory.GetFiles(dir, filter, SearchOption.AllDirectories)) {
                    File.Delete(file);
                    ++ct;

                    if (BatchScript.FileEchoOn) {
                        Console.WriteLine("delete file {0}", file);
                    }
                }
            }
            return ct;
        }
    }

    internal class SetEnvironmentExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            object ret = null;
            if (operands.Count >= 2) {
                var key = operands[0] as string;
                var val = operands[1] as string;
                val = Environment.ExpandEnvironmentVariables(val);
                Environment.SetEnvironmentVariable(key, val);
            }
            return ret;
        }
    }

    internal class GetEnvironmentExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            object ret = string.Empty;
            if (operands.Count >= 1) {
                var key = operands[0] as string;
                return Environment.GetEnvironmentVariable(key);
            }
            return ret;
        }
    }

    internal class ExpandEnvironmentsExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            object ret = string.Empty;
            if (operands.Count >= 1) {
                var key = operands[0] as string;
                return Environment.ExpandEnvironmentVariables(key);
            }
            return ret;
        }
    }

    internal class OsExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Environment.OSVersion.VersionString;
        }
    }

    internal class OsPlatformExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Environment.OSVersion.Platform.ToString();
        }
    }

    internal class OsVersionExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Environment.OSVersion.Version.ToString();
        }
    }

    internal class CommandExp : AbstractExpression
    {
        public override object Calc()
        {
            int exitCode = 0;
            MemoryStream ims = null, oms = null;
            int ct = m_CommandConfigs.Count;
            for(int i = 0; i < ct; ++i) {
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
                    } else {
                        exitCode = ExecProcess(cfg, ims, oms);
                    }
                } finally {
                    if (null != ims) {
                        ims.Close();
                        ims.Dispose();
                        ims = null;
                    }
                }
            }
            return exitCode;
        }
        protected override bool Load(CallData callData)
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
                } else {
                    Console.WriteLine("[syntax error] {0} line:{1}", callData.ToScriptString(false), callData.GetLine());
                }
            } else if (id == "command") {
                int num = callData.GetParamNum();
                if (num > 0) {
                    Console.WriteLine("[syntax error] {0} line:{1}", callData.ToScriptString(false), callData.GetLine());
                }
            } else {
                Console.WriteLine("[syntax error] {0} line:{1}", callData.ToScriptString(false), callData.GetLine());
            }
            return true;
        }
        protected override bool Load(Dsl.FunctionData funcData)
        {
            var id = funcData.GetId();
            var callData = funcData.Call;
            Load(callData);
            if (funcData.HaveStatement()) {
                var cmd = m_CommandConfigs[m_CommandConfigs.Count - 1];
                for (int i = 0; i < funcData.GetStatementNum(); ++i) {
                    var comp = funcData.GetStatement(i);
                    var cd = comp as Dsl.CallData;
                    if (null != cd) {
                        int num = cd.GetParamNum();
                        if (num >= 1) {
                            string type = cd.GetId();
                            var exp = Calculator.Load(cd.GetParam(0));
                            if (type == "input") {
                                cmd.m_Input = exp;
                            } else if (type == "output") {
                                cmd.m_Output = exp;
                            } else if (type == "error") {
                                cmd.m_Error = exp;
                            } else if (type == "nowait") {
                                cmd.m_NoWait = exp;
                            } else if (type == "useshellexecute") {
                                cmd.m_UseShellExecute = exp;
                            } else if (type == "verb") {
                                cmd.m_Verb = exp;
                            } else if (type == "domain") {
                                cmd.m_Domain = exp;
                            } else if (type == "user") {
                                cmd.m_UserName = exp;
                            } else if (type == "password") {
                                cmd.m_Password = exp;
                            } else if (type == "passwordincleartext") {
                                cmd.m_PasswordInClearText = exp;
                            } else if (type == "loadprofile") {
                                cmd.m_LoadUserProfile = exp;
                            } else if (type == "windowstyle") {
                                cmd.m_WindowStyle = exp;
                            } else if (type == "newwindow") {
                                cmd.m_NewWindow = exp;
                            } else if (type == "errordialog") {
                                cmd.m_ErrorDialog = exp;
                            } else if (type == "workingdirectory") {
                                cmd.m_WorkingDirectory = exp;
                            } else if (type == "encoding") {
                                cmd.m_Encoding = exp;
                            } else {
                                Console.WriteLine("[syntax error] {0} line:{1}", cd.ToScriptString(false), cd.GetLine());
                            }
                        } else {
                            Console.WriteLine("[syntax error] {0} line:{1}", cd.ToScriptString(false), cd.GetLine());
                        }
                    } else {
                        var fd = comp as Dsl.FunctionData;
                        if (null != fd && fd.Call.GetParamNum()== 0 && fd.HaveExternScript()) {
                            string os = fd.GetId();
                            string txt = fd.GetExternScript();
                            cmd.m_Commands.Add(os, txt);
                        } else {
                            Console.WriteLine("[syntax error] {0} line:{1}", comp.ToScriptString(false), comp.GetLine());
                        }
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
        private int ExecProcess(CommandConfig cfg, Stream istream, Stream ostream)
        {
            string fileName = string.Empty;
            if (null != cfg.m_FileName) {
                fileName = cfg.m_FileName.Calc() as string;
            }
            string args = string.Empty;
            if (null != cfg.m_Argments) {
                args = cfg.m_Argments.Calc() as string;
            }
            bool noWait = false;
            if (null != cfg.m_NoWait) {
                noWait = (bool)Convert.ChangeType(cfg.m_NoWait.Calc(), typeof(bool));
            }
            BatchScript.ProcessStartOption option = new BatchScript.ProcessStartOption();
            if (null != cfg.m_UseShellExecute) {
                option.UseShellExecute = (bool)Convert.ChangeType(cfg.m_UseShellExecute.Calc(), typeof(bool));
            }
            if (null != cfg.m_Verb) {
                option.Verb = cfg.m_Verb.Calc() as string;
            }
            if (null != cfg.m_Domain) {
                option.Domain = cfg.m_Domain.Calc() as string;
            }
            if (null != cfg.m_UserName) {
                option.UserName = cfg.m_UserName.Calc() as string;
            }
            if (null != cfg.m_Password) {
                option.Password = cfg.m_Password.Calc() as string;
            }
            if (null != cfg.m_PasswordInClearText) {
                option.PasswordInClearText = cfg.m_PasswordInClearText.Calc() as string;
            }
            if (null != cfg.m_LoadUserProfile) {
                option.LoadUserProfile = (bool)Convert.ChangeType(cfg.m_LoadUserProfile.Calc(), typeof(bool));
            }
            if (null != cfg.m_WindowStyle) {
                var str = cfg.m_WindowStyle.Calc() as string;
                ProcessWindowStyle style;
                if (Enum.TryParse(str, out style)) {
                    option.WindowStyle = style;
                }
            }
            if (null != cfg.m_NewWindow) {
                option.NewWindow = (bool)Convert.ChangeType(cfg.m_NewWindow.Calc(), typeof(bool));
            }
            if (null != cfg.m_ErrorDialog) {
                option.ErrorDialog = (bool)Convert.ChangeType(cfg.m_ErrorDialog.Calc(), typeof(bool));
            }
            if (null != cfg.m_WorkingDirectory) {
                option.WorkingDirectory = cfg.m_WorkingDirectory.Calc() as string;
            }
            Encoding encoding = null;
            if (null != cfg.m_Encoding) {
                var v = cfg.m_Encoding.Calc();
                var name = v as string;
                if (!string.IsNullOrEmpty(name)) {
                    encoding = Encoding.GetEncoding(name);
                } else if (v is int) {
                    int codePage = (int)Convert.ChangeType(v, typeof(int));
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
                var str = v as string;
                if (!string.IsNullOrEmpty(str)) {
                    str = Environment.ExpandEnvironmentVariables(str);
                    input = File.ReadAllLines(str);
                } else {
                    try {
                        int vn = (int)Convert.ChangeType(v, typeof(int));
                        object val;
                        if (Calculator.Variables.TryGetValue(vn, out val)) {
                            var slist = new List<string>();
                            var list = val as IList;
                            foreach (var s in list) {
                                slist.Add(s.ToString());
                            }
                            input = slist;
                        }
                    } catch (Exception ex) {
                        Console.WriteLine("input {0} failed:{1}", v, ex.Message);
                    }
                }
            }
            StringBuilder outputBuilder = null;
            StringBuilder errorBuilder = null;
            object output = null;
            object error = null;
            if (null != cfg.m_Output) {
                var v = cfg.m_Output.Calc();
                var str = v as string;
                if (!string.IsNullOrEmpty(str)) {
                    str = Environment.ExpandEnvironmentVariables(str);
                    output = str;
                } else {
                    output = v;
                }
                outputBuilder = new StringBuilder();
            }
            if (null != cfg.m_Error) {
                var v = cfg.m_Error.Calc();
                var str = v as string;
                if (!string.IsNullOrEmpty(str)) {
                    str = Environment.ExpandEnvironmentVariables(str);
                    error = str;
                } else {
                    error = v;
                }
                errorBuilder = new StringBuilder();
            }

            int exitCode = BatchScript.NewProcess(noWait, fileName, args, option, istream, ostream, input, outputBuilder, errorBuilder, encoding);

            if (null != outputBuilder && null != output) {
                var file = output as string;
                if (null != file) {
                    File.WriteAllText(file, outputBuilder.ToString());
                } else {
                    try {
                        int v = (int)Convert.ChangeType(output, typeof(int));
                        Calculator.Variables[v] = outputBuilder.ToString();
                    } catch (Exception ex) {
                        Console.WriteLine("output {0} failed:{1}", output, ex.Message);
                    }
                }
            }
            if (null != errorBuilder && null != error) {
                var file = error as string;
                if (null != file) {
                    File.WriteAllText(file, errorBuilder.ToString());
                } else {
                    try {
                        int v = (int)Convert.ChangeType(error, typeof(int));
                        Calculator.Variables[v] = errorBuilder.ToString();
                    } catch (Exception ex) {
                        Console.WriteLine("error {0} failed:{1}", error, ex.Message);
                    }
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
                    noWait = (bool)Convert.ChangeType(cfg.m_NoWait.Calc(), typeof(bool));
                }
                BatchScript.ProcessStartOption option = new BatchScript.ProcessStartOption();
                if (null != cfg.m_UseShellExecute) {
                    option.UseShellExecute = (bool)Convert.ChangeType(cfg.m_UseShellExecute.Calc(), typeof(bool));
                }
                if (null != cfg.m_Verb) {
                    option.Verb = cfg.m_Verb.Calc() as string;
                }
                if (null != cfg.m_Domain) {
                    option.Domain = cfg.m_Domain.Calc() as string;
                }
                if (null != cfg.m_UserName) {
                    option.UserName = cfg.m_UserName.Calc() as string;
                }
                if (null != cfg.m_Password) {
                    option.Password = cfg.m_Password.Calc() as string;
                }
                if (null != cfg.m_PasswordInClearText) {
                    option.PasswordInClearText = cfg.m_PasswordInClearText.Calc() as string;
                }
                if (null != cfg.m_LoadUserProfile) {
                    option.LoadUserProfile = (bool)Convert.ChangeType(cfg.m_LoadUserProfile.Calc(), typeof(bool));
                }
                if (null != cfg.m_WindowStyle) {
                    var str = cfg.m_WindowStyle.Calc() as string;
                    ProcessWindowStyle style;
                    if (Enum.TryParse(str, out style)) {
                        option.WindowStyle = style;
                    }
                }
                if (null != cfg.m_NewWindow) {
                    option.NewWindow = (bool)Convert.ChangeType(cfg.m_NewWindow.Calc(), typeof(bool));
                }
                if (null != cfg.m_ErrorDialog) {
                    option.ErrorDialog = (bool)Convert.ChangeType(cfg.m_ErrorDialog.Calc(), typeof(bool));
                }
                if (null != cfg.m_WorkingDirectory) {
                    option.WorkingDirectory = cfg.m_WorkingDirectory.Calc() as string;
                }
                Encoding encoding = null;
                if (null != cfg.m_Encoding) {
                    var v = cfg.m_Encoding.Calc();
                    var name = v as string;
                    if (!string.IsNullOrEmpty(name)) {
                        encoding = Encoding.GetEncoding(name);
                    } else if (v is int) {
                        int codePage = (int)Convert.ChangeType(v, typeof(int));
                        encoding = Encoding.GetEncoding(codePage);
                    }
                }
                if (null == encoding) {
                    encoding = Encoding.UTF8;
                }
                IList<string> input = null;
                if (null != cfg.m_Input) {
                    var v = cfg.m_Input.Calc();
                    var str = v as string;
                    if (!string.IsNullOrEmpty(str)) {
                        str = Environment.ExpandEnvironmentVariables(str);
                        input = File.ReadAllLines(str);
                    } else {
                        try {
                            int vn = (int)Convert.ChangeType(v, typeof(int));
                            object val;
                            if (Calculator.Variables.TryGetValue(vn, out val)) {
                                var slist = new List<string>();
                                var list = val as IList;
                                foreach (var s in list) {
                                    slist.Add(s.ToString());
                                }
                                input = slist;
                            }
                        } catch (Exception ex) {
                            Console.WriteLine("input {0} failed:{1}", v, ex.Message);
                        }
                    }
                }
                StringBuilder outputBuilder = null;
                StringBuilder errorBuilder = null;
                object output = null;
                object error = null;
                if (null != cfg.m_Output) {
                    var v = cfg.m_Output.Calc();
                    var str = v as string;
                    if (!string.IsNullOrEmpty(str)) {
                        str = Environment.ExpandEnvironmentVariables(str);
                        output = str;
                    } else {
                        output = v;
                    }
                    outputBuilder = new StringBuilder();
                }
                if (null != cfg.m_Error) {
                    var v = cfg.m_Error.Calc();
                    var str = v as string;
                    if (!string.IsNullOrEmpty(str)) {
                        str = Environment.ExpandEnvironmentVariables(str);
                        error = str;
                    } else {
                        error = v;
                    }
                    errorBuilder = new StringBuilder();
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

                    exitCode = BatchScript.NewProcess(noWait, fileName, args, option, istream, ostream, input, outputBuilder, errorBuilder, encoding);
                    if (null!=outputBuilder && null != output) {
                        var file = output as string;
                        if (null != file) {
                            File.WriteAllText(file, outputBuilder.ToString());
                        } else {
                            try {
                                int v = (int)Convert.ChangeType(output, typeof(int));
                                Calculator.Variables[v] = outputBuilder.ToString();
                            } catch(Exception ex) {
                                Console.WriteLine("output {0} failed:{1}", output, ex.Message);
                            }
                        }
                    }
                    if(null!=errorBuilder && null != error) {
                        var file = error as string;
                        if (null != file) {
                            File.WriteAllText(file, errorBuilder.ToString());
                        } else {
                            try {
                                int v = (int)Convert.ChangeType(error, typeof(int));
                                Calculator.Variables[v] = errorBuilder.ToString();
                            } catch (Exception ex) {
                                Console.WriteLine("error {0} failed:{1}", error, ex.Message);
                            }
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
            internal IExpression m_Error = null;
        }

        private List<CommandConfig> m_CommandConfigs = new List<CommandConfig>();
    }

    internal class KillExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            object ret = 0;
            if (operands.Count >= 1) {
                var vObj = operands[0];
                var name = vObj as string;
                if (!string.IsNullOrEmpty(name)) {
                    var ps = Process.GetProcessesByName(name);
                    foreach (var p in ps) {
                        p.Kill();
                    }
                    ret = ps.Length;
                } else if (vObj is int) {
                    int pid = (int)Convert.ChangeType(vObj, typeof(int));
                    var p = Process.GetProcessById(pid);
                    if (null != p) {
                        p.Kill();
                        ret = 1;
                    }
                } else {

                }
            }
            return ret;
        }
    }

    internal class SetCurrentDirectoryExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            object ret = string.Empty;
            if (operands.Count >= 1) {
                var dir = operands[0] as string;
                Environment.CurrentDirectory = Environment.ExpandEnvironmentVariables(dir);
                ret = dir;
            }
            return ret;
        }
    }

    internal class GetCurrentDirectoryExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Environment.CurrentDirectory;
        }
    }

    internal class GetScriptDirectoryExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return BatchScript.ScriptDirectory;
        }
    }

    internal class EnvironmentsExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Environment.GetEnvironmentVariables();
        }
    }

    internal class CommandLineExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Environment.CommandLine;
        }
    }

    internal class CommandLineArgsExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Environment.GetCommandLineArgs();
        }
    }

    internal class PauseExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            var info = Console.ReadKey(true);
            return (int)info.KeyChar;
        }
    }

    internal class WaitExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            object ret = null;
            if (operands.Count >= 1) {
                var v = operands[0];
                if (null != v) {
                    int time = (int)Convert.ChangeType(v, typeof(int));
                    Thread.Sleep(time);
                    ret = time;
                }
            }
            return ret;
        }
    }

    internal class WaitAllExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            var tasks = BatchScript.Tasks;
            int timeout = -1;
            if (operands.Count >= 1) {
                timeout = (int)Convert.ChangeType(operands[0], typeof(int));
            }
            List<int> results = new List<int>();
            if (Task.WaitAll(tasks.ToArray(), timeout)) {
                foreach (var task in tasks) {
                    results.Add(task.Result);
                }
            }
            return results;
        }
    }

    internal class ClearExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            Console.Clear();
            return null;
        }
    }

    internal class WriteExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            if (operands.Count >= 1) {
                var obj = operands[0];
                if (null != obj) {
                    var fmt = obj as string;
                    if (operands.Count > 1 && null != fmt) {
                        ArrayList arrayList = new ArrayList();
                        for (int i = 1; i < operands.Count; ++i) {
                            arrayList.Add(operands[i]);
                        }
                        Console.Write(fmt, arrayList.ToArray());
                    } else {
                        Console.Write(obj);
                    }
                }
            }
            return null;
        }
    }

    internal class ReadLineExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Console.ReadLine();
        }
    }

    internal class ReadExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            bool nodisplay = false;
            if (operands.Count >= 1) {
                nodisplay = (bool)Convert.ChangeType(operands[0], typeof(bool));
            }
            var info = Console.ReadKey(nodisplay);
            return info.KeyChar;
        }
    }

    internal class BeepExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            if (operands.Count >= 2) {
                int f = (int)Convert.ChangeType(operands[0], typeof(int));
                int d = (int)Convert.ChangeType(operands[1], typeof(int));
                Console.Beep(f, d);
            } else {
                Console.Beep();
            }
            return null;
        }
    }

    internal class GetTitleExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Console.Title;
        }
    }

    internal class SetTitleExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            if (operands.Count >= 1) {
                var title = operands[0] as string;
                if (null != title) {
                    Console.Title = title;
                }
            }
            return null;
        }
    }

    internal class GetBufferWidthExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Console.BufferWidth;
        }
    }

    internal class GetBufferHeightExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Console.BufferHeight;
        }
    }

    internal class SetBufferSizeExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            if (operands.Count >= 2) {
                int w = (int)Convert.ChangeType(operands[0], typeof(int));
                int h = (int)Convert.ChangeType(operands[1], typeof(int));
                Console.SetBufferSize(w, h);
            }
            return null;
        }
    }

    internal class GetCursorLeftExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Console.CursorLeft;
        }
    }

    internal class GetCursorTopExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Console.CursorTop;
        }
    }

    internal class SetCursorPosExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            if (operands.Count >= 2) {
                int left = (int)Convert.ChangeType(operands[0], typeof(int));
                int top = (int)Convert.ChangeType(operands[1], typeof(int));
                Console.SetCursorPosition(left, top);
            }
            return null;
        }
    }

    internal class GetBgColorExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            //Enum.GetName(typeof(ConsoleColor), Console.BackgroundColor);
            return Console.BackgroundColor.ToString();
        }
    }

    internal class SetBgColorExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            if (operands.Count >= 1) {
                var color = operands[0] as string;
                if (!string.IsNullOrEmpty(color)) {
                    Console.BackgroundColor = (System.ConsoleColor)Enum.Parse(typeof(System.ConsoleColor), color);
                }
            }
            return null;
        }
    }

    internal class GetFgColorExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            //Enum.GetName(typeof(ConsoleColor), Console.ForegroundColor);
            return Console.ForegroundColor.ToString();
        }
    }

    internal class SetFgColorExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            if (operands.Count >= 1) {
                var color = operands[0] as string;
                if (!string.IsNullOrEmpty(color)) {
                    Console.ForegroundColor = (System.ConsoleColor)Enum.Parse(typeof(System.ConsoleColor), color);
                }
            }
            return null;
        }
    }

    internal class ResetColorExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            Console.ResetColor();
            return null;
        }
    }

    internal class SetEncodingExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            if (operands.Count >= 1) {
                var encoding1 = operands[0];
                var encoding2 = encoding1;
                if (operands.Count >= 2) {
                    encoding2 = operands[1];
                }
                if (null != encoding1 && null != encoding2) {
                    Console.InputEncoding = BatchScript.GetEncoding(encoding1);
                    Console.OutputEncoding = BatchScript.GetEncoding(encoding2);
                }
            } else {
                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;
            }
            return null;
        }
    }
    
    internal class GetInputEncodingExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Console.InputEncoding;
        }
    }

    internal class GetOutputEncodingExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return Console.OutputEncoding;
        }
    }

    internal class ConsoleExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return typeof(Console);
        }
    }

    internal class EncodingExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            return typeof(Encoding);
        }
    }

    internal class MakeStringExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            List<char> chars = new List<char>();
            for (int i = 0; i < operands.Count; ++i) {
                var v = operands[i];
                var str = v as string;
                if (null != str) {
                    char c = '\0';
                    if (str.Length > 0) {
                        c = str[0];
                    }
                    chars.Add(c);
                } else {
                    char c = (char)Convert.ChangeType(operands[i], typeof(char));
                    chars.Add(c);
                }
            }
            return new String(chars.ToArray());
        }
    }

    internal class ReadAllLinesExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            if (operands.Count >= 1) {
                string path = operands[0] as string;
                if (!string.IsNullOrEmpty(path)) {
                    path = Environment.ExpandEnvironmentVariables(path);
                    Encoding encoding = Encoding.UTF8;
                    if (operands.Count >= 2) {
                        var v = operands[1];
                        encoding = BatchScript.GetEncoding(v);
                    }
                    return File.ReadAllLines(path, encoding);
                }
            }
            return new string[0];
        }
    }

    internal class WriteAllLinesExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            if (operands.Count >= 2) {
                string path = operands[0] as string;
                var lines = operands[1] as IList;
                if (!string.IsNullOrEmpty(path) && null != lines) {
                    path = Environment.ExpandEnvironmentVariables(path);
                    Encoding encoding = Encoding.UTF8;
                    if (operands.Count >= 3) {
                        var v = operands[2];
                        encoding = BatchScript.GetEncoding(v);
                    }
                    var strs = new List<string>();
                    foreach(var line in lines) {
                        strs.Add(line.ToString());
                    }
                    File.WriteAllLines(path, strs, encoding);
                    return true;
                }
            }
            return false;
        }
    }

    internal class ReadAllTextExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            if (operands.Count >= 1) {
                string path = operands[0] as string;
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
            return null;
        }
    }

    internal class WriteAllTextExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            if (operands.Count >= 2) {
                string path = operands[0] as string;
                var text = operands[1] as string;
                if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(text)) {
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

    internal class WaitStartIntervalExp : Calculator.SimpleExpressionBase
    {
        protected override object OnCalc(IList<object> operands)
        {
            if (operands.Count >= 1) {
                var v = operands[0];
                if (null != v) {
                    BatchScript.CheckStartInterval = (int)Convert.ChangeType(v, typeof(int));
                }
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
        internal static void Init()
        {
            s_Calculator.Init();

            //注册Gm命令
            s_Calculator.Register("fileecho", new ExpressionFactoryHelper<FileEchoExp>());
            s_Calculator.Register("listdirs", new ExpressionFactoryHelper<ListDirectoriesExp>());
            s_Calculator.Register("listfiles", new ExpressionFactoryHelper<ListFilesExp>());
            s_Calculator.Register("direxist", new ExpressionFactoryHelper<DirectoryExistExp>());
            s_Calculator.Register("fileexist", new ExpressionFactoryHelper<FileExistExp>());
            s_Calculator.Register("createdir", new ExpressionFactoryHelper<CreateDirectoryExp>());
            s_Calculator.Register("copydir", new ExpressionFactoryHelper<CopyDirectoryExp>());
            s_Calculator.Register("movedir", new ExpressionFactoryHelper<MoveDirectoryExp>());
            s_Calculator.Register("deletedir", new ExpressionFactoryHelper<DeleteDirectoryExp>());
            s_Calculator.Register("copyfile", new ExpressionFactoryHelper<CopyFileExp>());
            s_Calculator.Register("movefile", new ExpressionFactoryHelper<MoveFileExp>());
            s_Calculator.Register("deletefile", new ExpressionFactoryHelper<DeleteFileExp>());
            s_Calculator.Register("deletefiles", new ExpressionFactoryHelper<DeleteFilesExp>());
            s_Calculator.Register("setenv", new ExpressionFactoryHelper<SetEnvironmentExp>());
            s_Calculator.Register("getenv", new ExpressionFactoryHelper<GetEnvironmentExp>());
            s_Calculator.Register("expand", new ExpressionFactoryHelper<ExpandEnvironmentsExp>());
            s_Calculator.Register("os", new ExpressionFactoryHelper<OsExp>());
            s_Calculator.Register("osplatform", new ExpressionFactoryHelper<OsPlatformExp>());
            s_Calculator.Register("osversion", new ExpressionFactoryHelper<OsVersionExp>());
            s_Calculator.Register("process", new ExpressionFactoryHelper<CommandExp>());
            s_Calculator.Register("command", new ExpressionFactoryHelper<CommandExp>());
            s_Calculator.Register("kill", new ExpressionFactoryHelper<KillExp>());
            s_Calculator.Register("cd", new ExpressionFactoryHelper<SetCurrentDirectoryExp>());
            s_Calculator.Register("pwd", new ExpressionFactoryHelper<GetCurrentDirectoryExp>());
            s_Calculator.Register("getscriptdir", new ExpressionFactoryHelper<GetScriptDirectoryExp>());
            s_Calculator.Register("envs", new ExpressionFactoryHelper<EnvironmentsExp>());
            s_Calculator.Register("cmdline", new ExpressionFactoryHelper<CommandLineExp>());
            s_Calculator.Register("cmdlineargs", new ExpressionFactoryHelper<CommandLineArgsExp>());
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
            s_Calculator.Register("makestring", new ExpressionFactoryHelper<MakeStringExp>());
            s_Calculator.Register("readalllines", new ExpressionFactoryHelper<ReadAllLinesExp>());
            s_Calculator.Register("writealllines", new ExpressionFactoryHelper<WriteAllLinesExp>());
            s_Calculator.Register("readalltext", new ExpressionFactoryHelper<ReadAllTextExp>());
            s_Calculator.Register("writealltext", new ExpressionFactoryHelper<WriteAllTextExp>());
            s_Calculator.Register("waitstartinterval", new ExpressionFactoryHelper<WaitStartIntervalExp>());
        }
        internal static object Run(string scpFile, string[] args)
        {
            object r = null;
            bool redirect = true;
            while (redirect) {
                if (string.IsNullOrEmpty(scpFile)) {
                    scpFile = "main.dsl";
                    args = new string[] { scpFile };
                }
                var sdir = Path.GetDirectoryName(scpFile);
                sdir = Path.Combine(Environment.CurrentDirectory, sdir);
                s_ScriptDirectory = sdir;
                s_Calculator.Cleanup();
                s_Calculator.Load(scpFile);
                Environment.SetEnvironmentVariable("scriptdir", s_ScriptDirectory);
                r = s_Calculator.Calc("main", args);
                if (s_Calculator.RunState == RunStateEnum.Redirect) {
                    s_Calculator.RunState = RunStateEnum.Normal;
                    var list = r as List<string>;
                    if (null == list || list.Count == 0) {
                        args = null;
                        scpFile = string.Empty;
                    } else {
                        args = list.ToArray();
                        scpFile = Environment.ExpandEnvironmentVariables(args[0]);
                    }
                } else {
                    redirect = false;
                }
            }
            return r;
        }
        internal static Encoding GetEncoding(object v)
        {
            var name = v as string;
            if (null != name) {
                return Encoding.GetEncoding(name);
            } else if (v is int) {
                int codePage = (int)Convert.ChangeType(v, typeof(int));
                return Encoding.GetEncoding(codePage);
            } else {
                return Encoding.UTF8;
            }
        }
        internal static int NewProcess(bool noWait, string fileName, string args, ProcessStartOption option, Stream istream, Stream ostream, IList<string> input, StringBuilder output, StringBuilder error, Encoding encoding)
        {
            if (noWait) {
                var task = Task.Run<int>(() => NewProcessTask(fileName, args, option, istream, ostream, input, output, error, encoding));
                s_Tasks.Add(task);
                while (task.Status == TaskStatus.Created || task.Status == TaskStatus.WaitingForActivation || task.Status == TaskStatus.WaitingToRun) {
                    Console.WriteLine("wait {0}[{1}] start", Path.GetFileName(fileName), task.Status);
                    task.Wait(s_CheckStartInterval);
                }
                return 0;
            } else {
                return NewProcessTask(fileName, args, option, istream, ostream, input, output, error, encoding);
            }
        }
        private static int NewProcessTask(string fileName, string args, ProcessStartOption option, Stream istream, Stream ostream, IList<string> input, StringBuilder output, StringBuilder error, Encoding encoding)
        {
            //考虑到跨平台兼容性，不使用特定进程环境变量
            try {
                var psi = new ProcessStartInfo();
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
                if (null != ostream || null != output) {
                    psi.RedirectStandardOutput = true;
                    psi.StandardOutputEncoding = encoding;
                }
                if (null != error) {
                    psi.RedirectStandardError = true;
                    psi.StandardErrorEncoding = encoding;
                }
                var p = Process.Start(psi);
                if (psi.RedirectStandardInput) {
                    if (null != istream) {
                        istream.Seek(0, SeekOrigin.Begin);
                        using(var sr = new StreamReader(istream, encoding, true, 1024, true)){
                            string line;
                            while((line = sr.ReadLine())!=null){
                                p.StandardInput.WriteLine(line);
                                p.StandardInput.Flush();
                            }
                        }
                        p.StandardInput.Close();
                    } else if (null != input) {
                        foreach (var line in input) {
                            p.StandardInput.WriteLine(line);
                            p.StandardInput.Flush();
                        }
                        p.StandardInput.Close();
                    }
                }
                p.WaitForExit();
                if (psi.RedirectStandardOutput) {
                    string txt = p.StandardOutput.ReadToEnd();
                    p.StandardOutput.Close();
                    if (null != ostream) {
                        ostream.Seek(0, SeekOrigin.Begin);
                        ostream.SetLength(0);
                        var bytes = encoding.GetBytes(txt);
                        ostream.Write(bytes, 0, bytes.Length);
                    }
                    if (null != output) {
                        output.Clear();
                        output.Append(txt);
                    }
                }
                if (psi.RedirectStandardError) {
                    string txt = p.StandardError.ReadToEnd();
                    p.StandardError.Close();
                    if (null != error) {
                        error.Clear();
                        error.Append(txt);
                    }
                }
                int r = p.ExitCode;
                p.Close();
                return r;
            } catch (Exception ex) {
                Console.WriteLine("process({0} {1}) exception:{2}", fileName, args, ex.Message);
                return -1;
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
