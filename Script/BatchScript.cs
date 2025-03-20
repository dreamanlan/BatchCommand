using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;
using Microsoft.Win32;
using Dsl;
using ScriptableFramework;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;

#pragma warning disable 8600,8601,8602,8603,8604,8618,8619,8620,8625,CA1416
namespace BatchCommand
{
    internal sealed class TimeStatisticOnExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                BatchScript.TimeStatisticOn = operands[0].GetBool();
            }
            return BatchScript.TimeStatisticOn;
        }
    }

    internal sealed class GrepExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            BoxedValue r = BoxedValue.NullObject;
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
                    r = BoxedValue.FromObject(outLines);
                }
            }
            return r;
        }
    }

    internal sealed class SubstExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var r = BoxedValue.NullObject;
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
                    r = BoxedValue.FromObject(outLines);
                }
            }
            return r;
        }
    }

    internal sealed class AwkExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var r = BoxedValue.NullObject;
            if (operands.Count >= 2) {
                var lines = operands[0].As<IList<string>>();
                var script = operands[1].AsString;
                bool removeEmpties = true;
                if (operands.Count >= 3)
                    removeEmpties = operands[2].GetBool();
                var sepList = new List<string> { " " };
                if (operands.Count >= 4) {
                    sepList.Clear();
                    for (int i = 3; i < operands.Count; ++i) {
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
                            foreach (var item in rlist) {
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
                    r = BoxedValue.FromObject(outLines);
                }
            }
            return r;
        }

        private static string[] s_ArgNames = new string[] { "$0", "$1", "$2", "$3", "$4", "$5", "$6", "$7", "$8", "$9", "$10", "$11", "$12", "$13", "$14", "$15", "$16" };
    }

    internal sealed class GetScriptDirectoryExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BatchScript.ScriptDirectory;
        }
    }

    internal sealed class PauseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            var info = Console.ReadKey(true);
            return (int)info.KeyChar;
        }
    }

    internal sealed class ReadLineExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                string dir = operands[0].AsString;
                if (string.IsNullOrEmpty(dir)) {
                    dir = BatchScript.ScriptDirectory;
                }
                bool force = false;
                if (operands.Count >= 2) {
                    force = operands[1].GetBool();
                }
                LineEditor.LoadOrRefresh(dir, force);
                return LineEditor.ReadLine();
            }
            else {
                return Console.ReadLine();
            }
        }
    }

    internal sealed class ReadExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            bool nodisplay = false;
            if (operands.Count >= 1) {
                nodisplay = operands[0].GetBool();
            }
            var info = Console.ReadKey(nodisplay);
            return info.KeyChar;
        }
    }

    internal sealed class ClearExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            Console.Clear();
            return BoxedValue.NullObject;
        }
    }

    internal sealed class WriteExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
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
            return BoxedValue.NullObject;
        }
    }

    internal sealed class WriteBlockExp : AbstractExpression
    {
        protected override BoxedValue DoCalc()
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
            return BoxedValue.NullObject;
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
        protected override BoxedValue DoCalc()
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
            return BoxedValue.From(CalcBlockString(m_Block, Calculator, m_OutputBuilder, m_TempBuilder, m_BeginFirst, m_BeginSecond, m_EndFirst, m_EndSecond));
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
                            BoxedValue val;
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

    internal sealed class BeepExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
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
            return BoxedValue.NullObject;
        }
    }

    internal sealed class GetTitleExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                return Console.Title;
            }
            return string.Empty;
        }
    }

    internal sealed class SetTitleExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                var title = operands[0].AsString;
                if (null != title) {
                    Console.Title = title;
                }
            }
            return BoxedValue.NullObject;
        }
    }

    internal sealed class GetBufferWidthExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return Console.BufferWidth;
        }
    }

    internal sealed class GetBufferHeightExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return Console.BufferHeight;
        }
    }

    internal sealed class SetBufferSizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                if (operands.Count >= 2) {
                    int w = operands[0].GetInt();
                    int h = operands[1].GetInt();
                    Console.SetBufferSize(w, h);
                }
            }
            return BoxedValue.NullObject;
        }
    }

    internal sealed class GetCursorLeftExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return Console.CursorLeft;
        }
    }

    internal sealed class GetCursorTopExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return Console.CursorTop;
        }
    }

    internal sealed class SetCursorPosExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 2) {
                int left = operands[0].GetInt();
                int top = operands[1].GetInt();
                Console.SetCursorPosition(left, top);
            }
            return BoxedValue.NullObject;
        }
    }

    internal sealed class GetBgColorExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            //Enum.GetName(typeof(ConsoleColor), Console.BackgroundColor);
            return Console.BackgroundColor.ToString();
        }
    }

    internal sealed class SetBgColorExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                var color = operands[0].AsString;
                if (!string.IsNullOrEmpty(color)) {
                    Console.BackgroundColor = (System.ConsoleColor)Enum.Parse(typeof(System.ConsoleColor), color);
                }
            }
            return BoxedValue.NullObject;
        }
    }

    internal sealed class GetFgColorExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            //Enum.GetName(typeof(ConsoleColor), Console.ForegroundColor);
            return Console.ForegroundColor.ToString();
        }
    }

    internal sealed class SetFgColorExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                var color = operands[0].AsString;
                if (!string.IsNullOrEmpty(color)) {
                    Console.ForegroundColor = (System.ConsoleColor)Enum.Parse(typeof(System.ConsoleColor), color);
                }
            }
            return BoxedValue.NullObject;
        }
    }

    internal sealed class ResetColorExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            Console.ResetColor();
            return BoxedValue.NullObject;
        }
    }

    internal sealed class SetEncodingExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
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
            return BoxedValue.NullObject;
        }
    }

    internal sealed class GetInputEncodingExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromObject(Console.InputEncoding);
        }
    }

    internal sealed class GetOutputEncodingExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromObject(Console.OutputEncoding);
        }
    }

    internal sealed class ConsoleExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return typeof(Console);
        }
    }

    internal sealed class EncodingExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return typeof(Encoding);
        }
    }

    internal sealed class EnvironmentExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return typeof(Environment);
        }
    }

    internal sealed class GetClipboardExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return TextCopy.ClipboardService.GetText();
        }
    }

    internal sealed class SetClipboardExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count > 0) {
                string txt = operands[0].AsString;
                TextCopy.ClipboardService.SetText(txt);
                return BoxedValue.From(true);
            }
            return BoxedValue.From(false);
        }
    }

    internal sealed class RegReadExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 2) {
                string keyName = operands[0].AsString;
                string valName = operands[1].AsString;
                BoxedValue defVal = BoxedValue.NullObject;
                if (operands.Count >= 3)
                    defVal = operands[2];
                if (!string.IsNullOrEmpty(keyName) && !string.IsNullOrEmpty(valName)) {
                    object val = Registry.GetValue(keyName, valName, defVal.GetObject());
                    return BoxedValue.FromObject(val);
                }
            }
            return BoxedValue.NullObject;
        }
    }
    internal sealed class RegWriteExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 3) {
                string keyName = operands[0].AsString;
                string valName = operands[1].AsString;
                object val = operands[2].GetObject();
                BoxedValue valKind = BoxedValue.From((int)RegistryValueKind.Unknown);
                if (operands.Count >= 4)
                    valKind = operands[3];
                if (!string.IsNullOrEmpty(keyName) && !string.IsNullOrEmpty(valName)) {
                    Registry.SetValue(keyName, valName, val, (RegistryValueKind)valKind.GetInt());
                    return BoxedValue.From(true);
                }
            }
            return BoxedValue.From(false);
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
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
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
                        return BoxedValue.From(true);
                    }
                }
            }
            return BoxedValue.From(false);
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

        private static Dictionary<string, RegistryKey> s_KeyMap = new Dictionary<string, RegistryKey>
        {
            { "HKEY_CURRENT_USER", Registry.CurrentUser },
            {"HKEY_LOCAL_MACHINE", Registry.LocalMachine},
            {"HKEY_CLASSES_ROOT", Registry.ClassesRoot},
            {"HKEY_USERS", Registry.Users},
            {"HKEY_PERFORMANCE_DATA", Registry.PerformanceData},
            {"HKEY_CURRENT_CONFIG", Registry.CurrentConfig},
            //{"HKEY_DYN_DATA", Registry.DynData},
        };
    }

    internal static class LineEditor
    {
        internal static void LoadOrRefresh(string cfgDir)
        {
            LoadOrRefresh(cfgDir, false);
        }
        internal static void LoadOrRefresh(string cfgDir, bool force)
        {
            if (s_CfgDir != cfgDir || force) {
                s_CfgDir = cfgDir;
                LoadAutoComplete(cfgDir);
            }
        }
        internal static string ReadLine()
        {
            s_StartTop = Console.CursorTop;
            s_LineBuilder.Clear();
            RefreshLine();
            for (; ; ) {
                if (Console.KeyAvailable) {
                    var keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.UpArrow) {
                        if (s_HistoryIndex < s_Histories.Count) {
                            var str = s_Histories[s_HistoryIndex];
                            s_HistoryIndex = (s_HistoryIndex - 1 + s_Histories.Count) % s_Histories.Count;
                            ClearLine();
                            s_LineBuilder.Clear();
                            s_LineBuilder.Append(str);
                            int pos = str.Length + 1;
                            MoveCursor(pos);
                            RefreshLine();
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.DownArrow) {
                        if (s_HistoryIndex < s_Histories.Count) {
                            var str = s_Histories[s_HistoryIndex];
                            s_HistoryIndex = (s_HistoryIndex + 1) % s_Histories.Count;
                            ClearLine();
                            s_LineBuilder.Clear();
                            s_LineBuilder.Append(str);
                            int pos = str.Length + 1;
                            MoveCursor(pos);
                            RefreshLine();
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.LeftArrow) {
                        MoveCursorLeft();
                    }
                    else if (keyInfo.Key == ConsoleKey.RightArrow) {
                        MoveCursorRight();
                    }
                    else if (keyInfo.Key == ConsoleKey.Home) {
                        MoveCursor(1);
                    }
                    else if (keyInfo.Key == ConsoleKey.End) {
                        MoveCursor(s_LineBuilder.Length + 1);
                    }
                    else if (keyInfo.Key == ConsoleKey.Backspace) {
                        DeleteLeftChar();
                    }
                    else if (keyInfo.Key == ConsoleKey.Delete) {
                        DeleteRightChar();
                    }
                    else if ((keyInfo.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control && keyInfo.Key == ConsoleKey.V) {
                        string str = TextCopy.ClipboardService.GetText();
                        InsertStr(str);
                    }
                    else if (keyInfo.KeyChar == '\t' || !char.IsControl(keyInfo.KeyChar)) {
                        InsertChar(keyInfo.KeyChar);
                    }
                    else if (keyInfo.Key == ConsoleKey.Enter) {
                        string line = s_LineBuilder.ToString();
                        if (s_Histories.Count == 0 || line != s_Histories[s_Histories.Count - 1]) {
                            s_Histories.Add(line);
                            s_HistoryIndex = s_Histories.Count - 1;
                        }
                        while (s_Histories.Count > c_MaxHistoryCount) {
                            s_Histories.RemoveAt(0);
                        }

                        MoveCursor(line.Length + 1);
                        Console.WriteLine();
                        break;
                    }
                }
            }
            return s_LineBuilder.ToString();
        }

        private static void LoadAutoComplete(string fileDir)
        {
            s_AutoCompletions.Clear();
            string autoCompleteFile = Path.Combine(fileDir, "AutoComplete.txt");
            if (File.Exists(autoCompleteFile)) {
                var lines = File.ReadAllLines(autoCompleteFile);
                foreach (var line in lines) {
                    int si = line.IndexOf("=>");
                    if (si > 0) {
                        string key = line.Substring(0, si);
                        string val = line.Substring(si + 2);
                        s_AutoCompletions.TryAdd(key, val);
                    }
                }
            }
        }
        private static void MoveCursor(int pos)
        {
            (int left, int top) = Pos2Cursor(pos);
            Console.SetCursorPosition(left, top);
        }
        private static void MoveCursorLeft()
        {
            (int left, int top) = GetCursorPosition();
            int pos = Cursor2Pos(left, top);
            if (pos > 1) {
                (left, top) = Pos2Cursor(pos - 1);
                Console.SetCursorPosition(left, top);
            }
        }
        private static void MoveCursorRight()
        {
            int maxPos = s_LineBuilder.Length + 1;
            (int left, int top) = GetCursorPosition();
            int pos = Cursor2Pos(left, top);
            if (pos < maxPos) {
                (left, top) = Pos2Cursor(pos + 1);
                Console.SetCursorPosition(left, top);
            }
        }
        private static void DeleteLeftChar()
        {
            var sb = s_LineBuilder;
            (int left, int top) = GetCursorPosition();
            int pos = Cursor2Pos(left, top);
            if (pos >= 2 && pos - 2 < sb.Length) {
                sb.Remove(pos - 2, 1);
                RefreshLine();
                (left, top) = Pos2Cursor(pos - 1);
                Console.SetCursorPosition(left, top);
            }
            else {
                Console.SetCursorPosition(1, top);
            }
        }
        private static void DeleteRightChar()
        {
            var sb = s_LineBuilder;
            (int left, int top) = GetCursorPosition();
            int pos = Cursor2Pos(left, top);
            if (pos >= 1 && pos - 1 < sb.Length) {
                sb.Remove(pos - 1, 1);
                RefreshLine();
            }
        }
        private static void InsertChar(char c)
        {
            var sb = s_LineBuilder;
            (int left, int top) = GetCursorPosition();
            int pos = Cursor2Pos(left, top);
            if (pos >= 1) {
                sb.Insert(pos - 1, c);
            }
            if (RefreshLine(s_AutoCompletions)) {
                (left, top) = Pos2Cursor(pos + 1);
                Console.SetCursorPosition(left, top);
            }
        }
        private static void InsertStr(string str)
        {
            var sb = s_LineBuilder;
            (int left, int top) = GetCursorPosition();
            int pos = Cursor2Pos(left, top);
            if (pos >= 1) {
                sb.Insert(pos - 1, str);
            }
            RefreshLine();
            (left, top) = Pos2Cursor(pos + str.Length);
            Console.SetCursorPosition(left, top);
        }
        private static void RefreshLine()
        {
            var sb = s_LineBuilder;
            (int left, int top) = GetCursorPosition();
            (_, int ttop) = Pos2Cursor(sb.Length + 1);
            for (int t = s_StartTop; t <= ttop; ++t) {
                Console.SetCursorPosition(0, t);
                Console.Write(EmptyLine);
            }
            Console.SetCursorPosition(0, s_StartTop);
            Console.Write(">");
            if (sb.Length > 0) {
                var str = sb.ToString();
                Console.Write(str);
                Console.SetCursorPosition(left, top);
            }
        }
        private static bool RefreshLine(ConcurrentDictionary<string, string> autoCompletions)
        {
            var sb = s_LineBuilder;
            (int left, int top) = GetCursorPosition();
            (_, int ttop) = Pos2Cursor(sb.Length + 1);
            for (int t = s_StartTop; t <= ttop; ++t) {
                Console.SetCursorPosition(0, t);
                Console.Write(EmptyLine);
            }
            Console.SetCursorPosition(0, s_StartTop);
            Console.Write(">");
            if (sb.Length > 0) {
                var str = sb.ToString();
                if (autoCompletions.TryGetValue(str, out var val)) {
                    sb.Clear();
                    sb.Append(val);
                    Console.Write(val);
                    (left, top) = Pos2Cursor(val.Length + 1);
                    Console.SetCursorPosition(left, top);
                    return false;
                }
                else {
                    Console.Write(str);
                    Console.SetCursorPosition(left, top);
                }
            }
            return true;
        }
        private static void ClearLine()
        {
            var sb = s_LineBuilder;
            (_, int ttop) = Pos2Cursor(sb.Length + 1);
            for (int t = s_StartTop; t <= ttop; ++t) {
                Console.SetCursorPosition(0, t);
                Console.Write(EmptyLine);
            }
            Console.SetCursorPosition(0, s_StartTop);
        }
        private static (int left, int top) GetCursorPosition()
        {
            int left = Console.CursorLeft;
            int top = Console.CursorTop;
            return (left, top);
        }
        private static int Cursor2Pos(int left, int top)
        {
            int pos = (top - s_StartTop) * MaxLineCharNum + left;
            return pos;
        }
        private static (int left, int top) Pos2Cursor(int pos)
        {
            int dt = pos / MaxLineCharNum;
            int left = pos % MaxLineCharNum;
            int top = s_StartTop + dt;
            return (left, top);
        }
        private static int MaxLineCharNum
        {
            get {
                int reserved = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 0 : 1;
                if (s_MaxLineCharNum != Console.BufferWidth - reserved) {
                    s_MaxLineCharNum = Console.BufferWidth - reserved;
                }
                return s_MaxLineCharNum;
            }
        }
        private static string EmptyLine
        {
            get {
                if (MaxLineCharNum != s_EmptyLine.Length) {
                    s_EmptyLine = new string(' ', s_MaxLineCharNum);
                }
                return s_EmptyLine;
            }
        }

        private static string s_CfgDir = string.Empty;
        private static StringBuilder s_LineBuilder = new StringBuilder();
        private static ConcurrentDictionary<string, string> s_AutoCompletions = new ConcurrentDictionary<string, string>();
        private static List<string> s_Histories = new List<string>();
        private static int s_HistoryIndex = 0;
        private static string s_EmptyLine = string.Empty;
        private static int s_MaxLineCharNum = 0;
        private static int s_StartTop = 0;
        private const int c_MaxHistoryCount = 1024;
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
        internal static string ScriptDirectory
        {
            get { return s_ScriptDirectory; }
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

            //register Gm Command
            s_Calculator.Register("timestat", "timestat(bool) or timestat() api", new ExpressionFactoryHelper<TimeStatisticOnExp>());
            s_Calculator.Register("grep", "grep(lines[,regex]) api", new ExpressionFactoryHelper<GrepExp>());
            s_Calculator.Register("subst", "subst(lines,regex,subst[,count]) api, count is the max count of per subst", new ExpressionFactoryHelper<SubstExp>());
            s_Calculator.Register("awk", "awk(lines,scp[,removeEmpties,sep1,sep2,...]) api", new ExpressionFactoryHelper<AwkExp>());
            s_Calculator.Register("getscriptdir", "getscriptdir() api", new ExpressionFactoryHelper<GetScriptDirectoryExp>());
            s_Calculator.Register("pause", "pause() api", new ExpressionFactoryHelper<PauseExp>());
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
            s_Calculator.Register("getclipboard", "getclipboard() api", new ExpressionFactoryHelper<GetClipboardExp>());
            s_Calculator.Register("setclipboard", "setclipboard(txt) api", new ExpressionFactoryHelper<SetClipboardExp>());
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
        internal static BoxedValue Run(string scpFile, List<BoxedValue> args)
        {
            var r = BoxedValue.NullObject;
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
                            vargs.Add(BoxedValue.FromObject(o));
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
        internal static BoxedValue EvalAndRun(string code)
        {
            BoxedValue r = BoxedValue.EmptyString;
            var file = new Dsl.DslFile();
            if (file.LoadFromString(code, msg => { Log(msg); })) {
                r = EvalAndRun(file.DslInfos);
            }
            return r;
        }
        internal static BoxedValue EvalAndRun(params ISyntaxComponent[] expressions)
        {
            IList<ISyntaxComponent> exps = expressions;
            return EvalAndRun(exps);
        }
        internal static BoxedValue EvalAndRun(IList<ISyntaxComponent> expressions)
        {
            BoxedValue r = BoxedValue.EmptyString;
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
        internal static List<BoxedValue> NewCalculatorValueList()
        {
            return s_Calculator.NewCalculatorValueList();
        }
        internal static void RecycleCalculatorValueList(List<BoxedValue> list)
        {
            s_Calculator.RecycleCalculatorValueList(list);
        }
        internal static BoxedValue Call(string func)
        {
            var args = NewCalculatorValueList();
            var r = Call(func, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static BoxedValue Call(string func, BoxedValue arg1)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            var r = Call(func, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static BoxedValue Call(string func, BoxedValue arg1, BoxedValue arg2)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            args.Add(arg2);
            var r = Call(func, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static BoxedValue Call(string func, BoxedValue arg1, BoxedValue arg2, BoxedValue arg3)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            args.Add(arg2);
            args.Add(arg3);
            var r = Call(func, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        internal static BoxedValue Call(string func, BoxedValue arg1, BoxedValue arg2, BoxedValue arg3, BoxedValue arg4)
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
        internal static BoxedValue Call(string func, BoxedValue arg1, BoxedValue arg2, BoxedValue arg3, BoxedValue arg4, BoxedValue arg5)
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
        internal static BoxedValue Call(string func, List<BoxedValue> args)
        {
            var r = s_Calculator.Calc(func, args);
            return r;
        }
        internal static Encoding GetEncoding(BoxedValue v)
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

        private static bool s_TimeStatisticOn = false;
        private static string s_ScriptDirectory = string.Empty;
        private static DslCalculator s_Calculator = new DslCalculator();
    }
}
#pragma warning restore 8600,8601,8602,8603,8604,8618,8619,8620,8625,CA1416
