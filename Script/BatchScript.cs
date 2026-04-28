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
    internal sealed class StartAsyncTaskExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                throw new Exception("Expected: startasynctask(func_name, args...) api");
            string funcName = operands[0].AsString;
            var args = BatchScript.NewCalculatorValueList();
            for (int i = 1; i < operands.Count; i++) {
                args.Add(operands[i]);
            }
            int handle = BatchScript.StartAsyncTask(funcName, args);
            BatchScript.RecycleCalculatorValueList(args);
            return BoxedValue.From(handle);
        }
    }
    internal sealed class TickAsyncTasksExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count > 0)
                throw new Exception("Expected: tickasynctasks() api");
            int activeCount = BatchScript.TickAsyncTasks();
            return BoxedValue.From(activeCount);
        }
    }
    internal sealed class StopAsyncTaskExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1)
                throw new Exception("Expected: stopasynctask(handle) api");
            int handle = operands[0].GetInt();
            bool removed = BatchScript.StopAsyncTask(handle);
            return BoxedValue.FromBool(removed);
        }
    }
    internal sealed class StopCompletedAsyncTasksExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count > 0)
                throw new Exception("Expected: stopcompletedasynctasks() api");
            int removedCount = BatchScript.StopCompletedAsyncTasks();
            return BoxedValue.From(removedCount);
        }
    }
    internal sealed class GetAsyncTaskResultExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1)
                throw new Exception("Expected: getasynctaskresult(handle) api");
            int handle = operands[0].GetInt();
            bool res = BatchScript.TryGetAsyncTaskResult(handle, out bool isCompleted, out BoxedValue value);
            return Tuple.Create(BoxedValue.FromBool(res), BoxedValue.FromBool(isCompleted), value);
        }
    }
    internal sealed class DebuggerLaunchExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0)
                throw new Exception("Expected: debuggerlaunch() api");
            bool r = false;
            if (!Debugger.IsAttached) {
                r = Debugger.Launch();
            }
            return BoxedValue.FromBool(r);
        }
    }
    internal sealed class DebuggerBreakExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0)
                throw new Exception("Expected: debuggerbreak() api");
            Debugger.Break();
            return BoxedValue.FromBool(Debugger.IsAttached);
        }
    }
    internal sealed class GetStringInLengthExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                throw new Exception("Expected: getstringinlength(str, len[, begin_or_end_or_beginend])");
            }
            string str = operands[0].AsString;
            int len = operands[1].GetInt();
            int beginOrEndOrBeginEnd = operands.Count > 2 ? operands[2].GetInt() : 0;
            if (!string.IsNullOrEmpty(str)) {
                if (str.Length <= len) {
                    return BoxedValue.FromString(str);
                }
                switch (beginOrEndOrBeginEnd) {
                    case 1:
                        return BoxedValue.From("..." + str.Substring(str.Length - len, len));
                    case 2:
                        return BoxedValue.From(str.Substring(0, len / 2) + "..." + str.Substring(str.Length - len / 2, len / 2));
                    case 0:
                    default:
                        return BoxedValue.From(str.Substring(0, len) + "...");
                }
            }
            return BoxedValue.EmptyString;
        }
    }
    internal sealed class CloneExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1)
                throw new Exception("Expected: clone(v)");
            if (operands.Count >= 1) {
                var val = operands[0];
                return Clone(val);
            }
            return BoxedValue.NullObject;
        }
        private BoxedValue Clone(BoxedValue val)
        {
            if (val.IsNullObject) {
                return BoxedValue.NullObject;
            }
            else if (val.IsString) {
                var str = val.GetString();
                return string.Concat(str);
            }
            else if (val.IsObject) {
                var obj = val.GetObject();
                if (obj is ICloneable cloneable) {
                    return BoxedValue.FromObject(cloneable.Clone());
                }
                else if (obj is IList list) {
                    var newList = new List<BoxedValue>();
                    foreach (var item in list) {
                        newList.Add(BoxedValue.FromObject(item));
                    }
                    return BoxedValue.FromObject(newList);
                }
                else if (obj is IDictionary dict) {
                    var newDict = new Dictionary<BoxedValue, BoxedValue>();
                    foreach (var key in dict.Keys) {
                        var v = dict[key];
                        newDict.Add(BoxedValue.FromObject(key), BoxedValue.FromObject(v));
                    }
                    return BoxedValue.FromObject(newDict);
                }
                else if (obj is Queue<BoxedValue> queue) {
                    var newQueue = new Queue<BoxedValue>(queue);
                    return BoxedValue.FromObject(newQueue);
                }
                else if (obj is Stack<BoxedValue> stack) {
                    var newStack = new Stack<BoxedValue>(stack.ToArray());
                    return BoxedValue.FromObject(newStack);
                }
                else if (obj is Tuple<BoxedValue> t1) {
                    return BoxedValue.From(Tuple.Create(t1.Item1));
                }
                else if (obj is Tuple<BoxedValue, BoxedValue> t2) {
                    return BoxedValue.From(Tuple.Create(t2.Item1, t2.Item2));
                }
                else if (obj is Tuple<BoxedValue, BoxedValue, BoxedValue> t3) {
                    return BoxedValue.From(Tuple.Create(t3.Item1, t3.Item2, t3.Item3));
                }
                else if (obj is Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue> t4) {
                    return BoxedValue.From(Tuple.Create(t4.Item1, t4.Item2, t4.Item3, t4.Item4));
                }
                else if (obj is Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue> t5) {
                    return BoxedValue.From(Tuple.Create(t5.Item1, t5.Item2, t5.Item3, t5.Item4, t5.Item5));
                }
                else if (obj is Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue> t6) {
                    return BoxedValue.From(Tuple.Create(t6.Item1, t6.Item2, t6.Item3, t6.Item4, t6.Item5, t6.Item6));
                }
                else if (obj is Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue> t7) {
                    return BoxedValue.From(Tuple.Create(t7.Item1, t7.Item2, t7.Item3, t7.Item4, t7.Item5, t7.Item6, t7.Item7));
                }
                else if (obj is Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, Tuple<BoxedValue>> t8) {
                    return BoxedValue.From(Tuple.Create(t8.Item1, t8.Item2, t8.Item3, t8.Item4, t8.Item5, t8.Item6, t8.Item7, Tuple.Create(Clone(t8.Rest.Item1))));
                }
            }
            return val;
        }
    }

    internal sealed class TimeStatisticOnExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count > 1)
                throw new Exception("Expected: timestat(bool) or timestat() api");
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
            if (operands.Count < 2 || operands.Count > 4)
                throw new Exception("Expected: grep(lines,regex[,context_lines_after,context_lines_before]) api");
            BoxedValue r = BoxedValue.EmptyString;
            if (operands.Count >= 1) {
                var lines = operands[0].As<IList<string>>();
                var regex = operands[1].AsString;
                int contextLinesAfter = operands.Count > 2 ? operands[2].GetInt() : 5;
                int contextLinesBefore = operands.Count > 3 ? operands[3].GetInt() : 0;
                if (null != lines) {
                    string result = GrepLines(lines, regex, contextLinesAfter, contextLinesBefore);
                    r = BoxedValue.FromObject(result);
                }
            }
            return r;
        }

        public string GrepLines(IList<string> lines, string searchRegex, int contextLinesAfter = 5, int contextLinesBefore = 0)
        {
            var matchedLineIndices = new HashSet<int>();
            var result = new StringBuilder();

            try {
                var regex = new Regex(searchRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                for (int i = 0; i < lines.Count; i++) {
                    if (regex.IsMatch(lines[i])) {
                        matchedLineIndices.Add(i);
                    }
                }
            }
            catch (ArgumentException) {
                // If regex is invalid, fall back to simple string search
                for (int i = 0; i < lines.Count; i++) {
                    if (lines[i].IndexOf(searchRegex, StringComparison.OrdinalIgnoreCase) >= 0) {
                        matchedLineIndices.Add(i);
                    }
                }
            }

            if (matchedLineIndices.Count == 0) {
                return $"No matches found for pattern: {searchRegex}";
            }

            // Build output with context lines
            var outputLines = new SortedSet<int>();
            foreach (var matchIndex in matchedLineIndices) {
                int startLine = Math.Max(0, matchIndex - contextLinesBefore);
                int endLine = Math.Min(lines.Count - 1, matchIndex + contextLinesAfter);
                for (int i = startLine; i <= endLine; i++) {
                    outputLines.Add(i);
                }
            }

            var sortedLines = new List<int>(outputLines);
            int lastLine = -2;
            foreach (var lineIndex in sortedLines) {
                if (lineIndex > lastLine + 1) {
                    if (lastLine >= 0) {
                        result.AppendLine("--");
                    }
                }
                string prefix = matchedLineIndices.Contains(lineIndex) ? "* " : "  ";
                result.AppendLine($"{prefix}{lineIndex + 1}: {lines[lineIndex]}");
                lastLine = lineIndex;
            }

            return result.ToString();
        }
    }

    internal sealed class SubstExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 4)
                throw new Exception("Expected: subst(lines,regex,subst[,count]) api, count is the max count of per subst");
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
            if (operands.Count < 2)
                throw new Exception("Expected: awk(lines,scp[,removeEmpties,sep1,sep2,...]) api");
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
                    BatchScript.RecycleCalculatorValueList(args);
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
            if (operands.Count != 0)
                throw new Exception("Expected: getscriptdir() api");
            return BatchScript.ScriptDirectory;
        }
    }

    internal sealed class PauseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0)
                throw new Exception("Expected: pause() api");
            var info = Console.ReadKey(true);
            return (int)info.KeyChar;
        }
    }

    internal sealed class ReadLineExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count > 2)
                throw new Exception("Expected: readline() api, Console.ReadLine");
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
            if (operands.Count > 1)
                throw new Exception("Expected: read([nodisplay]) api, Console.Read");
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
            if (operands.Count != 0)
                throw new Exception("Expected: clear() api, clear console");
            Console.Clear();
            return BoxedValue.NullObject;
        }
    }

    internal sealed class WriteExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                throw new Exception("Expected: write(fmt,arg1,arg2,....) api, Console.Write");
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
                if (c1.Length >= 2 && c2.Length >= 2) {
                    m_BeginFirst = c1[0];
                    m_BeginSecond = c1[1];
                    m_EndFirst = c2[0];
                    m_EndSecond = c2[1];

                    m_BeginFirst2 = m_BeginFirst;
                    m_BeginSecond2 = m_BeginSecond;
                    m_EndFirst2 = m_EndFirst;
                    m_EndSecond2 = m_EndSecond;

                    m_CommentBeginFirst = '\0';
                    m_CommentBeginSecond = '\0';
                    m_CommentEndFirst = '\0';
                    m_CommentEndSecond = '\0';
                }
                if (c1.Length >= 4 && c2.Length >= 4) {
                    m_BeginFirst2 = c1[2];
                    m_BeginSecond2 = c1[3];
                    m_EndFirst2 = c2[2];
                    m_EndSecond2 = c2[3];
                }
                if (c1.Length >= 6 && c2.Length >= 6) {
                    m_CommentBeginFirst = c1[4];
                    m_CommentBeginSecond = c1[5];
                    m_CommentEndFirst = c2[4];
                    m_CommentEndSecond = c2[5];
                }
            }
            Console.Write(BlockExp.CalcBlockString(m_Block, Calculator, m_OutputBuilder, m_TempBuilder
                , m_BeginFirst, m_BeginSecond, m_EndFirst, m_EndSecond
                , m_BeginFirst2, m_BeginSecond2, m_EndFirst2, m_EndSecond2
                , m_CommentBeginFirst, m_CommentBeginSecond, m_CommentEndFirst, m_CommentEndSecond));
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
        private char m_BeginFirst2 = BlockExp.c_BeginFirst2;
        private char m_BeginSecond2 = BlockExp.c_BeginSecond2;
        private char m_EndFirst2 = BlockExp.c_EndFirst2;
        private char m_EndSecond2 = BlockExp.c_EndSecond2;
        private char m_CommentBeginFirst = BlockExp.c_CommentBeginFirst;
        private char m_CommentBeginSecond = BlockExp.c_CommentBeginSecond;
        private char m_CommentEndFirst = BlockExp.c_CommentEndFirst;
        private char m_CommentEndSecond = BlockExp.c_CommentEndSecond;
    }

    internal sealed class BlockExp : AbstractExpression
    {
        protected override BoxedValue DoCalc()
        {
            if (null != m_BeginChars && null != m_EndChars) {
                var c1 = m_BeginChars.Calc().ToString();
                var c2 = m_EndChars.Calc().ToString();
                if (c1.Length >= 2 && c2.Length >= 2) {
                    m_BeginFirst = c1[0];
                    m_BeginSecond = c1[1];
                    m_EndFirst = c2[0];
                    m_EndSecond = c2[1];

                    m_BeginFirst2 = m_BeginFirst;
                    m_BeginSecond2 = m_BeginSecond;
                    m_EndFirst2 = m_EndFirst;
                    m_EndSecond2 = m_EndSecond;

                    m_CommentBeginFirst = '\0';
                    m_CommentBeginSecond = '\0';
                    m_CommentEndFirst = '\0';
                    m_CommentEndSecond = '\0';
                }
                if (c1.Length >= 4 && c2.Length >= 4) {
                    m_BeginFirst2 = c1[2];
                    m_BeginSecond2 = c1[3];
                    m_EndFirst2 = c2[2];
                    m_EndSecond2 = c2[3];
                }
                if (c1.Length >= 6 && c2.Length >= 6) {
                    m_CommentBeginFirst = c1[4];
                    m_CommentBeginSecond = c1[5];
                    m_CommentEndFirst = c2[4];
                    m_CommentEndSecond = c2[5];
                }
            }
            return BoxedValue.From(CalcBlockString(m_Block, Calculator, m_OutputBuilder, m_TempBuilder
                , m_BeginFirst, m_BeginSecond, m_EndFirst, m_EndSecond
                , m_BeginFirst2, m_BeginSecond2, m_EndFirst2, m_EndSecond2
                , m_CommentBeginFirst, m_CommentBeginSecond, m_CommentEndFirst, m_CommentEndSecond));
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
        private char m_BeginFirst2 = c_BeginFirst2;
        private char m_BeginSecond2 = c_BeginSecond2;
        private char m_EndFirst2 = c_EndFirst2;
        private char m_EndSecond2 = c_EndSecond2;
        private char m_CommentBeginFirst = c_CommentBeginFirst;
        private char m_CommentBeginSecond = c_CommentBeginSecond;
        private char m_CommentEndFirst = c_CommentEndFirst;
        private char m_CommentEndSecond = c_CommentEndSecond;

        internal static string CalcBlockString(string block, DslCalculator calculator, StringBuilder outputBuilder, StringBuilder tempBuilder
            , char beginFirst, char beginSecond, char endFirst, char endSecond
            , char beginFirst2, char beginSecond2, char endFirst2, char endSecond2
            , char commentBeginFirst, char commentBeginSecond, char commentEndFirst, char commentEndSecond)
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
                    ExtractBlockString(block, ref i, endFirst, endSecond, tempBuilder);
                    BoxedValue val = CalcBlockString(calculator, tempBuilder);
                    outputBuilder.Append(val.ToString());
                }
                else if (c == beginFirst2 && nc == beginSecond2) {
                    ++i;
                    ++i;
                    ExtractBlockString(block, ref i, endFirst2, endSecond2, tempBuilder);
                    BoxedValue val = CalcBlockString(calculator, tempBuilder);
                    outputBuilder.Append(val.ToString());
                }
                else if (c == commentBeginFirst && nc == commentBeginSecond) {
                    ++i;
                    ++i;
                    ExtractBlockString(block, ref i, commentEndFirst, commentEndSecond, tempBuilder);
                }
                else {
                    outputBuilder.Append(c);
                }
            }
            return outputBuilder.ToString();
        }
        internal static void ExtractBlockString(string block, ref int i, char endFirst, char endSecond, StringBuilder tempBuilder)
        {
            tempBuilder.Length = 0;
            for (int j = i; j < block.Length; ++j) {
                char c = block[j];
                char nc = '\0';
                if (j + 1 < block.Length) {
                    nc = block[j + 1];
                }
                if (c == endFirst && nc == endSecond) {
                    i = j + 1;
                    break;
                }
                else {
                    tempBuilder.Append(c);
                }
            }
        }
        internal static BoxedValue CalcBlockString(DslCalculator calculator, StringBuilder tempBuilder)
        {
            string varNameOrCode = tempBuilder.ToString().Trim();
            BoxedValue val;
            if (!calculator.TryGetVariable(varNameOrCode, out val)) {
                val = BatchScript.EvalAndRun(varNameOrCode);
            }
            return val;
        }
        internal const char c_BeginFirst = '{';
        internal const char c_BeginSecond = '%';
        internal const char c_EndFirst = '%';
        internal const char c_EndSecond = '}';
        internal const char c_BeginFirst2 = '{';
        internal const char c_BeginSecond2 = '{';
        internal const char c_EndFirst2 = '}';
        internal const char c_EndSecond2 = '}';
        internal const char c_CommentBeginFirst = '{';
        internal const char c_CommentBeginSecond = '#';
        internal const char c_CommentEndFirst = '#';
        internal const char c_CommentEndSecond = '}';
    }

    internal sealed class BeepExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count > 2)
                throw new Exception("Expected: beep([frequence,duration]) api, Console.Beep, only on win32");
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
            if (operands.Count != 0)
                throw new Exception("Expected: gettitle() api, Console.Title, only on win32");
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
            if (operands.Count != 1)
                throw new Exception("Expected: settitle(title) api");
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
            if (operands.Count != 0)
                throw new Exception("Expected: getbufferwidth() api");
            return Console.BufferWidth;
        }
    }

    internal sealed class GetBufferHeightExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0)
                throw new Exception("Expected: getbufferheight() api");
            return Console.BufferHeight;
        }
    }

    internal sealed class SetBufferSizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2)
                throw new Exception("Expected: setbuffersize(width,height) api");
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
            if (operands.Count != 0)
                throw new Exception("Expected: getcursorleft() api");
            return Console.CursorLeft;
        }
    }

    internal sealed class GetCursorTopExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0)
                throw new Exception("Expected: getcursortop() api");
            return Console.CursorTop;
        }
    }

    internal sealed class SetCursorPosExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2)
                throw new Exception("Expected: setcursorpos(left,top) api");
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
            if (operands.Count != 0)
                throw new Exception("Expected: getbgcolor() api, return str");
            //Enum.GetName(typeof(ConsoleColor), Console.BackgroundColor);
            return Console.BackgroundColor.ToString();
        }
    }

    internal sealed class SetBgColorExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1)
                throw new Exception("Expected: setbgcolor(color_name) api, color:Black,DarkBlue,DarkGreen,DarkCyan,DarkRed,DarkMagenta,DarkYellow,Gray,DarkGray,Blue,Green,Cyan,Red,Magenta,Yellow,White");
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
            if (operands.Count != 0)
                throw new Exception("Expected: getfgcolor() api, return str");
            //Enum.GetName(typeof(ConsoleColor), Console.ForegroundColor);
            return Console.ForegroundColor.ToString();
        }
    }

    internal sealed class SetFgColorExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1)
                throw new Exception("Expected: setfgcolor(color_name) api, color:Black,DarkBlue,DarkGreen,DarkCyan,DarkRed,DarkMagenta,DarkYellow,Gray,DarkGray,Blue,Green,Cyan,Red,Magenta,Yellow,White");
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
            if (operands.Count != 0)
                throw new Exception("Expected: resetcolor() api");
            Console.ResetColor();
            return BoxedValue.NullObject;
        }
    }

    internal sealed class SetEncodingExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count > 2)
                throw new Exception("Expected: setencoding([input[,output]]) api, def is UTF8");
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
            if (operands.Count != 0)
                throw new Exception("Expected: getinputencoding() api, return Encoding");
            return BoxedValue.FromObject(Console.InputEncoding);
        }
    }

    internal sealed class GetOutputEncodingExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0)
                throw new Exception("Expected: getoutputencoding() api, return Encoding");
            return BoxedValue.FromObject(Console.OutputEncoding);
        }
    }

    internal sealed class ConsoleExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0)
                throw new Exception("Expected: console() api, return typeof(Console)");
            return typeof(Console);
        }
    }

    internal sealed class EncodingExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0)
                throw new Exception("Expected: encoding() api, return typeof(Encoding)");
            return typeof(Encoding);
        }
    }

    internal sealed class EnvironmentExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0)
                throw new Exception("Expected: env() api, return typeof(Environment)");
            return typeof(Environment);
        }
    }

    internal sealed class GetClipboardExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0)
                throw new Exception("Expected: getclipboard() api");
            return TextCopy.ClipboardService.GetText();
        }
    }

    internal sealed class SetClipboardExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1)
                throw new Exception("Expected: setclipboard(txt) api");
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
            if (operands.Count < 2 || operands.Count > 3)
                throw new Exception("Expected: regread(keyname,valname[,defval]) api, root:HKEY_CURRENT_USER|HKEY_LOCAL_MACHINE|HKEY_CLASSES_ROOT|HKEY_USERS|HKEY_PERFORMANCE_DATA|HKEY_CURRENT_CONFIG");
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
            if (operands.Count < 3 || operands.Count > 4)
                throw new Exception("Expected: regwrite(keyname,valname,val[,val_kind]) api, root:HKEY_CURRENT_USER|HKEY_LOCAL_MACHINE|HKEY_CLASSES_ROOT|HKEY_USERS|HKEY_PERFORMANCE_DATA|HKEY_CURRENT_CONFIG, val_kind:0-unk,1-str,2-exstr,3-bin,4-dword,7-multistr,11-qword");
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
            if (operands.Count < 1 || operands.Count > 2)
                throw new Exception("Expected: regdelete(keyname[,valname]) api, root:HKEY_CURRENT_USER|HKEY_LOCAL_MACHINE|HKEY_CLASSES_ROOT|HKEY_USERS|HKEY_PERFORMANCE_DATA|HKEY_CURRENT_CONFIG");
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
            SetCursorPosition(left, top);
        }
        private static void MoveCursorLeft()
        {
            (int left, int top) = GetCursorPosition();
            int pos = Cursor2Pos(left, top);
            if (pos > 1) {
                (left, top) = Pos2Cursor(pos - 1);
                SetCursorPosition(left, top);
            }
        }
        private static void MoveCursorRight()
        {
            int maxPos = s_LineBuilder.Length + 1;
            (int left, int top) = GetCursorPosition();
            int pos = Cursor2Pos(left, top);
            if (pos < maxPos) {
                (left, top) = Pos2Cursor(pos + 1);
                SetCursorPosition(left, top);
            }
        }
        private static void DeleteLeftChar()
        {
            var sb = s_LineBuilder;
            (int left, int top) = GetCursorPosition();
            int pos = Cursor2Pos(left, top);
            if (pos >= 2 && pos - 2 < sb.Length) {
                ClearLine();
                sb.Remove(pos - 2, 1);
                RefreshLine();
                (left, top) = Pos2Cursor(pos - 1);
                SetCursorPosition(left, top);
            }
            else {
                SetCursorPosition(1, top);
            }
        }
        private static void DeleteRightChar()
        {
            var sb = s_LineBuilder;
            (int left, int top) = GetCursorPosition();
            int pos = Cursor2Pos(left, top);
            if (pos >= 1 && pos - 1 < sb.Length) {
                ClearLine();
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
            ClearLine();
            if (RefreshLine(s_AutoCompletions)) {
                (left, top) = Pos2Cursor(pos + 1);
                SetCursorPosition(left, top);
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
            ClearLine();
            RefreshLine();
            (left, top) = Pos2Cursor(pos + str.Length);
            SetCursorPosition(left, top);
        }
        private static void RefreshLine()
        {
            var sb = s_LineBuilder;
            (int left, int top) = GetCursorPosition();
            SetCursorPosition(0, s_StartTop);
            Console.Write(">");
            if (sb.Length > 0) {
                WriteContent();
                SetCursorPosition(left, top);
            }
        }
        private static bool RefreshLine(ConcurrentDictionary<string, string> autoCompletions)
        {
            var sb = s_LineBuilder;
            (int left, int top) = GetCursorPosition();
            SetCursorPosition(0, s_StartTop);
            Console.Write(">");
            if (sb.Length > 0) {
                var str = sb.ToString();
                if (autoCompletions.TryGetValue(str, out var val)) {
                    sb.Clear();
                    sb.Append(val);
                    WriteContent();
                    (left, top) = Pos2Cursor(val.Length + 1);
                    SetCursorPosition(left, top);
                    return false;
                }
                else {
                    WriteContent();
                    SetCursorPosition(left, top);
                }
            }
            return true;
        }
        private static void ClearLine()
        {
            var sb = s_LineBuilder;
            (_, int ttop) = Pos2Cursor(sb.Length + 1);
            for (int t = s_StartTop; t <= ttop; ++t) {
                SetCursorPosition(0, t);
                Console.Write(EmptyLine);
                if (t < ttop || (sb.Length + 1) % MaxLineCharNum == 0) {
                    Console.Write(' ');
                }
            }
            SetCursorPosition(0, s_StartTop);
        }
        private static void WriteContent()
        {
            var sb = s_LineBuilder;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                (_, int ttop) = Pos2Cursor(sb.Length + 1);
                int pos = 0;
                for (int t = s_StartTop; t <= ttop; ++t) {
                    int num;
                    if (t == s_StartTop) {
                        num = Math.Min(MaxLineCharNum - 1, sb.Length - pos);
                        SetCursorPosition(1, t);
                    }
                    else {
                        num = Math.Min(MaxLineCharNum, sb.Length - pos);
                        SetCursorPosition(0, t);
                    }
                    Console.Write(sb.ToString(pos, num));
                    if (t < ttop) {
                        Console.WriteLine();
                    }
                    pos += num;
                }
            }
            else {
                Console.Write(sb.ToString());
            }
        }
        private static (int left, int top) GetCursorPosition()
        {
            int left = Console.CursorLeft;
            int top = Console.CursorTop;
            return (left, top);
        }
        private static void SetCursorPosition(int left, int top)
        {
            try {
                Console.SetCursorPosition(left, top);
            }
            catch (Exception ex) {
                Console.WriteLine("left:{0} top:{1} start top:{2} exception:{3} stack:{4}", left, top, s_StartTop, ex.Message, ex.StackTrace);
            }
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
                if (s_MaxLineCharNum != Console.BufferWidth) {
                    s_MaxLineCharNum = Console.BufferWidth;
                }
                return s_MaxLineCharNum;
            }
        }
        private static string EmptyLine
        {
            get {
                int reserved = 1;
                if (MaxLineCharNum - reserved != s_EmptyLine.Length) {
                    s_EmptyLine = new string(' ', MaxLineCharNum - reserved);
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

    public sealed class BatchScript
    {
        public static bool TimeStatisticOn
        {
            get { return s_TimeStatisticOn; }
            set { s_TimeStatisticOn = value; }
        }
        public static string ScriptDirectory
        {
            get {
                if (null == s_ScriptDirectory) {
                    s_ScriptDirectory = string.Empty;
                }
                return s_ScriptDirectory;
            }
        }
        public static bool HasDslErrors
        {
            get { return DslErrorInfo.Length > 0; }
        }
        public static DslCalculator Calculator
        {
            get {
                if (null == s_Calculator) {
                    s_Calculator = new DslCalculator();
                }
                return s_Calculator;
            }
        }
        public static Dictionary<int, Tuple<Stack<IEnumerator>, AsyncCalcResult, AsyncTaskRuntimeContext>> AsyncTasks
        {
            get {
                if (s_AsyncTasks == null)
                    s_AsyncTasks = new Dictionary<int, Tuple<Stack<IEnumerator>, AsyncCalcResult, AsyncTaskRuntimeContext>>();
                return s_AsyncTasks;
            }
        }
        public static List<string> EmptyStringList
        {
            get {
                if (null == s_EmptyStringList) {
                    s_EmptyStringList = new List<string>();
                }
                return s_EmptyStringList;
            }
        }
        public static List<BoxedValue> EmptyBoxedValueList
        {
            get {
                if (null == s_EmptyBoxedValueList) {
                    s_EmptyBoxedValueList = new List<BoxedValue>();
                }
                return s_EmptyBoxedValueList;
            }
        }
        public static StringBuilder DslErrorInfo
        {
            get {
                if (null == s_DslErrorInfo) {
                    s_DslErrorInfo = new StringBuilder();
                }
                return s_DslErrorInfo;
            }
        }
        public static SortedList<string, string> UserApiDocs
        {
            get {
                if (null == s_UserApiDocs) {
                    s_UserApiDocs = new SortedList<string, string>();
                }
                return s_UserApiDocs;
            }
        }
        public static SortedList<string, string> ApiDocs
        {
            get { return Calculator.ApiDocs; }
        }

        public static void ClearDslErrors()
        {
            DslErrorInfo.Clear();
        }
        public static string GetDslErrors()
        {
            return DslErrorInfo.ToString();
        }
        public static void Log(string fmt, params object[] args)
        {
            if (args.Length == 0)
                Console.WriteLine(fmt);
            else
                Console.WriteLine(fmt, args);
        }
        public static void Log(object arg)
        {
            Console.WriteLine("{0}", arg);
        }
        public static void Init()
        {
#if NET || NETSTANDARD
            var provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
#endif
            DslErrorInfo.Clear();
            Calculator.OnLog = msg => { OnDslError(msg); };
            Calculator.NewApiRegistry();

            //register Gm Command
            Calculator.Register("startasynctask", "startasynctask(func_name,arg1,arg2,...) api, start an async script function and return a handle", new ExpressionFactoryHelper<StartAsyncTaskExp>());
            Calculator.Register("tickasynctasks", "tickasynctasks() api, tick all async tasks, return active task count", new ExpressionFactoryHelper<TickAsyncTasksExp>());
            Calculator.Register("stopasynctask", "stopasynctask(handle) api, stop and remove an async task by handle", new ExpressionFactoryHelper<StopAsyncTaskExp>());
            Calculator.Register("stopcompletedasynctasks", "stopcompletedasynctasks() api, remove all completed async tasks and return removed count", new ExpressionFactoryHelper<StopCompletedAsyncTasksExp>());
            Calculator.Register("getasynctaskresult", "getasynctaskresult(handle) api, get the result of a completed async task, tuple (success, completed, result)", new ExpressionFactoryHelper<GetAsyncTaskResultExp>());
            Calculator.Register("debuggerlaunch", "debuggerlaunch() api", new ExpressionFactoryHelper<DebuggerLaunchExp>());
            Calculator.Register("debuggerbreak", "debuggerbreak() api", new ExpressionFactoryHelper<DebuggerBreakExp>());
            Calculator.Register("getstringinlength", "getstringinlength(str,len[,begin_end_or_beginend])", new ExpressionFactoryHelper<GetStringInLengthExp>());
            Calculator.Register("clone", "clone(v)", new ExpressionFactoryHelper<CloneExp>());
            Calculator.Register("timestat", "timestat(bool) or timestat() api", new ExpressionFactoryHelper<TimeStatisticOnExp>());
            Calculator.Register("grep", "grep(lines,regex[,context_lines_after,context_lines_before]) api", new ExpressionFactoryHelper<GrepExp>());
            Calculator.Register("subst", "subst(lines,regex,subst[,count]) api, count is the max count of per subst", new ExpressionFactoryHelper<SubstExp>());
            Calculator.Register("awk", "awk(lines,scp[,removeEmpties,sep1,sep2,...]) api", new ExpressionFactoryHelper<AwkExp>());
            Calculator.Register("getscriptdir", "getscriptdir() api", new ExpressionFactoryHelper<GetScriptDirectoryExp>());
            Calculator.Register("pause", "pause() api", new ExpressionFactoryHelper<PauseExp>());
            Calculator.Register("clear", "clear() api, clear console", new ExpressionFactoryHelper<ClearExp>());
            Calculator.Register("write", "write(fmt,arg1,arg2,....) api, Console.Write", new ExpressionFactoryHelper<WriteExp>());
            Calculator.Register("writeblock", "writeblock{:txt:} or writeblock(two_chars_begin,two_chars_end){:txt:} api, Console.Write with macro expand, def begin is {% end is %}", new ExpressionFactoryHelper<WriteBlockExp>());
            Calculator.Register("block", "block{:txt:} or block(two_chars_begin,two_chars_end){:txt:} api, macro expand, def begin is {% end is %}", new ExpressionFactoryHelper<BlockExp>());
            Calculator.Register("readline", "readline() api, Console.ReadLine", new ExpressionFactoryHelper<ReadLineExp>());
            Calculator.Register("read", "read([nodisplay]) api, Console.Read", new ExpressionFactoryHelper<ReadExp>());
            Calculator.Register("beep", "beep([frequence,duration]) api, Console.Beep, only on win32", new ExpressionFactoryHelper<BeepExp>());
            Calculator.Register("gettitle", "gettitle() api, Console.Title, only on win32", new ExpressionFactoryHelper<GetTitleExp>());
            Calculator.Register("settitle", "settitle(title) api", new ExpressionFactoryHelper<SetTitleExp>());
            Calculator.Register("getbufferwidth", "getbufferwidth() api", new ExpressionFactoryHelper<GetBufferWidthExp>());
            Calculator.Register("getbufferheight", "getbufferheight() api", new ExpressionFactoryHelper<GetBufferHeightExp>());
            Calculator.Register("setbuffersize", "setbuffersize(width,height) api", new ExpressionFactoryHelper<SetBufferSizeExp>());
            Calculator.Register("getcursorleft", "getcursorleft() api", new ExpressionFactoryHelper<GetCursorLeftExp>());
            Calculator.Register("getcursortop", "getcursortop() api", new ExpressionFactoryHelper<GetCursorTopExp>());
            Calculator.Register("setcursorpos", "setcursorpos(left,top) api", new ExpressionFactoryHelper<SetCursorPosExp>());
            Calculator.Register("getbgcolor", "getbgcolor() api, return str", new ExpressionFactoryHelper<GetBgColorExp>());
            Calculator.Register("setbgcolor", "setbgcolor(color_name) api, color:Black,DarkBlue,DarkGreen,DarkCyan,DarkRed,DarkMagenta,DarkYellow,Gray,DarkGray,Blue,Green,Cyan,Red,Magenta,Yellow,White", new ExpressionFactoryHelper<SetBgColorExp>());
            Calculator.Register("getfgcolor", "getfgcolor() api, return str", new ExpressionFactoryHelper<GetFgColorExp>());
            Calculator.Register("setfgcolor", "setfgcolor(color_name) api, color:Black,DarkBlue,DarkGreen,DarkCyan,DarkRed,DarkMagenta,DarkYellow,Gray,DarkGray,Blue,Green,Cyan,Red,Magenta,Yellow,White", new ExpressionFactoryHelper<SetFgColorExp>());
            Calculator.Register("resetcolor", "resetcolor() api", new ExpressionFactoryHelper<ResetColorExp>());
            Calculator.Register("setencoding", "setencoding([input[,output]]) api, def is UTF8", new ExpressionFactoryHelper<SetEncodingExp>());
            Calculator.Register("getinputencoding", "getinputencoding() api, return Encoding", new ExpressionFactoryHelper<GetInputEncodingExp>());
            Calculator.Register("getoutputencoding", "getoutputencoding() api, return Encoding", new ExpressionFactoryHelper<GetOutputEncodingExp>());
            Calculator.Register("console", "console() api, return typeof(Console)", new ExpressionFactoryHelper<ConsoleExp>());
            Calculator.Register("encoding", "encoding() api, return typeof(Encoding)", new ExpressionFactoryHelper<EncodingExp>());
            Calculator.Register("env", "env() api, return typeof(Environment)", new ExpressionFactoryHelper<EnvironmentExp>());
            Calculator.Register("getclipboard", "getclipboard() api", new ExpressionFactoryHelper<GetClipboardExp>());
            Calculator.Register("setclipboard", "setclipboard(txt) api", new ExpressionFactoryHelper<SetClipboardExp>());
            Calculator.Register("regread", "regread(keyname,valname[,defval]) api, root:HKEY_CURRENT_USER|HKEY_LOCAL_MACHINE|HKEY_CLASSES_ROOT|HKEY_USERS|HKEY_PERFORMANCE_DATA|HKEY_CURRENT_CONFIG", new ExpressionFactoryHelper<RegReadExp>());
            Calculator.Register("regwrite", "regwrite(keyname,valname,val[,val_kind]) api, root:HKEY_CURRENT_USER|HKEY_LOCAL_MACHINE|HKEY_CLASSES_ROOT|HKEY_USERS|HKEY_PERFORMANCE_DATA|HKEY_CURRENT_CONFIG, val_kind:0-unk,1-str,2-exstr,3-bin,4-dword,7-multistr,11-qword", new ExpressionFactoryHelper<RegWriteExp>());
            Calculator.Register("regdelete", "regdelete(keyname[,valname]) api, root:HKEY_CURRENT_USER|HKEY_LOCAL_MACHINE|HKEY_CLASSES_ROOT|HKEY_USERS|HKEY_PERFORMANCE_DATA|HKEY_CURRENT_CONFIG", new ExpressionFactoryHelper<RegDeleteExp>());
        }
        public static void Register(string name, string doc, IExpressionFactory factory)
        {
            Register(name, doc, true, factory);
        }
        public static void Register(string name, string doc, bool addToUserApiDoc, IExpressionFactory factory)
        {
            Calculator.Register(name, doc, factory);
            if (addToUserApiDoc) {
                AddUserApiDoc(name, doc);
            }
        }
        public static void ClearUserApiDocs()
        {
            UserApiDocs.Clear();
        }
        public static void AddUserApiDoc(string name, string doc)
        {
            var userApiDocs = UserApiDocs;
            if (userApiDocs.ContainsKey(name)) {
                userApiDocs[name] = doc;
            }
            else {
                userApiDocs.Add(name, doc);
            }
        }
        public static void SetOnTryGetVariable(DslCalculator.TryGetVariableDelegation callback)
        {
            Calculator.OnTryGetVariable = callback;
        }
        public static void SetOnTrySetVariable(DslCalculator.TrySetVariableDelegation callback)
        {
            Calculator.OnTrySetVariable = callback;
        }
        public static void SetOnLoadFailback(DslCalculator.LoadFailbackDelegation callback)
        {
            Calculator.OnLoadFailback = callback;
        }
        public static void Clear()
        {
            Calculator.Clear();
        }
        public static void ClearGlobalVariables()
        {
            Calculator.ClearGlobalVariables();
        }
        public static bool TryGetGlobalVariable(string v, out BoxedValue result)
        {
            return Calculator.TryGetGlobalVariable(v, out result);
        }
        public static BoxedValue GetGlobalVariable(string v)
        {
            TryGetGlobalVariable(v, out var result);
            return result;
        }
        public static void SetGlobalVariable(string v, BoxedValue val)
        {
            Calculator.SetGlobalVariable(v, val);
        }
        public static void Load(string scpFile)
        {
            var sdir = Path.GetDirectoryName(scpFile);
            sdir = Path.Combine(Environment.CurrentDirectory, sdir);
            s_ScriptDirectory = sdir;
            Calculator.Clear();
            DslErrorInfo.Clear();
            LoadDslHelper(scpFile);
            Environment.SetEnvironmentVariable("scriptdir", ScriptDirectory);
        }
        public static void LoadImportFiles(params string[] scpFiles)
        {
            LoadImportFiles((IList<string>)scpFiles);
        }
        public static void LoadImportFiles(IList<string> scpFiles)
        {
            DslErrorInfo.Clear();
            foreach (var scpFile in scpFiles) {
                LoadDslHelper(scpFile);
            }
        }
        public static void LoadFunc(string funcName, string code, IList<string> paramNames, bool update)
        {
            if (update || Calculator.TryGetFuncInfo(funcName, out var funcInfo)) {
                string procCode = string.Format("script{{ {0}; }};", code);
                var file = new Dsl.DslFile();
                file.SetStringDelimiter("[[", "]]");
                ScriptableDslHelper.ForDslCalculator.SetCallbacks(file);
                if (file.LoadFromString(procCode, msg => { OnDslError(msg); })) {
                    var func = file.DslInfos[0] as Dsl.FunctionData;
                    Debug.Assert(null != func);
                    Calculator.LoadDsl(funcName, paramNames, func);
                }
            }
        }
        public static BoxedValue Run(string scpFile)
        {
            return Run(scpFile, EmptyStringList, EmptyBoxedValueList);
        }
        public static BoxedValue Run(string scpFile, List<string> includes)
        {
            return Run(scpFile, includes, EmptyBoxedValueList);
        }
        public static BoxedValue Run(string scpFile, List<BoxedValue> args)
        {
            return Run(scpFile, EmptyStringList, args);
        }
        public static BoxedValue Run(string scpFile, List<string> includes, List<BoxedValue> args)
        {
            var r = BoxedValue.NullObject;
            bool redirect = true;
            var vargs = Calculator.NewCalculatorValueList();
            vargs.AddRange(args);
            while (redirect) {
                Load(scpFile);
                LoadImportFiles(includes);
                r = Calculator.Calc("main", vargs);
                if (Calculator.RunState == RunStateEnum.Redirect) {
                    Calculator.RunState = RunStateEnum.Normal;
                    var list = r.As<IList>();
                    if (null == list || list.Count == 0) {
                        vargs.Clear();
                        scpFile = string.Empty;
                    }
                    else {
                        vargs.Clear();
                        foreach (var o in list) {
                            vargs.Add(BoxedValue.FromObject(o));
                        }
                        scpFile = Environment.ExpandEnvironmentVariables(vargs[0].AsString);
                    }
                }
                else {
                    redirect = false;
                }
            }
            Calculator.RecycleCalculatorValueList(vargs);
            return r;
        }
        public static BoxedValue EvalAndRun(string code)
        {
            //If local variables are used, the code must run within the function context.
            BoxedValue r = BoxedValue.EmptyString;
            var file = new Dsl.DslFile();
            file.SetStringDelimiter("[[", "]]");
            ScriptableDslHelper.ForDslCalculator.SetCallbacks(file);
            if (file.LoadFromString(code, msg => { OnDslError(msg); })) {
                r = EvalAndRun(file.DslInfos);
            }
            return r;
        }
        public static BoxedValue EvalAndRun(params ISyntaxComponent[] expressions)
        {
            //If local variables are used, the code must run within the function context.
            IList<ISyntaxComponent> exps = expressions;
            return EvalAndRun(exps);
        }
        public static BoxedValue EvalAndRun(IList<ISyntaxComponent> expressions)
        {
            //If local variables are used, the code must run within the function context.
            BoxedValue r = BoxedValue.EmptyString;
            List<IExpression> exps = new List<IExpression>();
            Calculator.LoadDsl(expressions, exps);
            try {
                r = Calculator.CalcInCurrentContext(exps);
            }
            catch {
                //For annotation purposes only
                Log("If local variables are used, the code must run within the function context.");
            }
            return r;
        }
        public static string EvalAsFunc(string code, IList<string> paramNames)
        {
            string id = System.Guid.NewGuid().ToString();
            string procCode = string.Format("script{{ {0}; }};", code);
            var file = new Dsl.DslFile();
            file.SetStringDelimiter("[[", "]]");
            ScriptableDslHelper.ForDslCalculator.SetCallbacks(file);
            if (file.LoadFromString(procCode, msg => { OnDslError(msg); })) {
                var func = file.DslInfos[0] as Dsl.FunctionData;
                Debug.Assert(null != func);
                Calculator.LoadDsl(id, paramNames, func);
                return id;
            }
            return string.Empty;
        }
        public static string EvalAsFunc(Dsl.FunctionData func, IList<string> paramNames)
        {
            string id = System.Guid.NewGuid().ToString();
            Debug.Assert(null != func);
            Calculator.LoadDsl(id, paramNames, func);
            return id;
        }
        public static List<BoxedValue> NewCalculatorValueList()
        {
            return Calculator.NewCalculatorValueList();
        }
        public static void RecycleCalculatorValueList(List<BoxedValue> list)
        {
            Calculator.RecycleCalculatorValueList(list);
        }
        public static BoxedValue Call(string func)
        {
            var args = NewCalculatorValueList();
            var r = Call(func, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        public static BoxedValue Call(string func, BoxedValue arg1)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            var r = Call(func, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        public static BoxedValue Call(string func, BoxedValue arg1, BoxedValue arg2)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            args.Add(arg2);
            var r = Call(func, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        public static BoxedValue Call(string func, BoxedValue arg1, BoxedValue arg2, BoxedValue arg3)
        {
            var args = NewCalculatorValueList();
            args.Add(arg1);
            args.Add(arg2);
            args.Add(arg3);
            var r = Call(func, args);
            RecycleCalculatorValueList(args);
            return r;
        }
        public static BoxedValue Call(string func, BoxedValue arg1, BoxedValue arg2, BoxedValue arg3, BoxedValue arg4)
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
        public static BoxedValue Call(string func, BoxedValue arg1, BoxedValue arg2, BoxedValue arg3, BoxedValue arg4, BoxedValue arg5)
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
        public static BoxedValue Call(string func, List<BoxedValue> args)
        {
            var r = Calculator.Calc(func, args);
            return r;
        }
        public static Encoding GetEncoding(BoxedValue v)
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
        public static int StartAsyncTask(string func, List<BoxedValue> args)
        {
            var asyncResult = new AsyncCalcResult();
            var enumerator = Calculator.CalcAsync(func, args, asyncResult);
            if (enumerator == null) {
                return -1;
            }
            var tasks = AsyncTasks;
            int handle = NextAsyncTaskId();
            var runtimeCtx = Calculator.CreateAsyncContext();
            var stack = new Stack<IEnumerator>();
                stack.Push(enumerator);
                tasks[handle] = new Tuple<Stack<IEnumerator>, AsyncCalcResult, AsyncTaskRuntimeContext>(stack, asyncResult, runtimeCtx);
            return handle;
        }
        public static int TickAsyncTasks()
        {
            var tasks = AsyncTasks;
            if (tasks.Count == 0)
                return 0;
            var keys = AsyncTaskTickKeys;
            keys.Clear();
            foreach (var key in tasks.Keys) {
                keys.Add(key);
            }
            int activeCount = 0;
            for (int i = 0; i < keys.Count; i++) {
                int key = keys[i];
                if (tasks.TryGetValue(key, out var task)) {
                    if (task.Item2.IsCompleted) {
                        continue;
                    }
                    Calculator.SetAsyncContext(task.Item3);
                    var stack = task.Item1;
                    while (stack.Count > 0) {
                        var top = stack.Peek();
                        bool hasMore = top.MoveNext();
                        if (hasMore) {
                            if (top.Current is IEnumerator subEnum) {
                                stack.Push(subEnum);
                                continue;
                            }
                            break;
                        }
                        else {
                            stack.Pop();
                        }
                    }
                    Calculator.SaveAsyncContext(task.Item3);
                    if (stack.Count == 0) {
                        task.Item2.IsCompleted = true;
                    }
                    else {
                        activeCount++;
                    }
                }
            }
            return activeCount;
        }
        public static bool StopAsyncTask(int handle)
        {
            var tasks = AsyncTasks;
            bool removed = tasks.Remove(handle);
            return removed;
        }
        public static int StopCompletedAsyncTasks()
        {
            var tasks = BatchScript.AsyncTasks;
            var keys = BatchScript.AsyncTaskTickKeys;
            keys.Clear();
            foreach (var key in tasks.Keys) {
                keys.Add(key);
            }
            int removedCount = 0;
            for (int i = 0; i < keys.Count; i++) {
                int key = keys[i];
                if (tasks.TryGetValue(key, out var task)) {
                    if (task.Item2.IsCompleted) {
                        tasks.Remove(key);
                        removedCount++;
                    }
                }
            }
            return removedCount;
        }
        public static bool TryGetAsyncTaskResult(int handle, out bool isCompleted, out BoxedValue result)
        {
            var tasks = BatchScript.AsyncTasks;
            if (tasks.TryGetValue(handle, out var task)) {
                isCompleted = task.Item2.IsCompleted;
                result = task.Item2.Value;
                return true;
            }
            isCompleted = false;
            result = BoxedValue.NullObject;
            return false;
        }
        internal static List<int> AsyncTaskTickKeys
        {
            get {
                if (s_AsyncTaskTickKeys == null)
                    s_AsyncTaskTickKeys = new List<int>();
                return s_AsyncTaskTickKeys;
            }
        }
        internal static int NextAsyncTaskId()
        {
            return ++s_AsyncTaskIdSeed;
        }
        private static void LoadDslHelper(string file)
        {
            DslFile dslFile = new DslFile();
            dslFile.SetStringDelimiter("[[", "]]");
            ScriptableDslHelper.ForDslCalculator.SetCallbacks(dslFile);
            if (!dslFile.Load(file, OnDslError)) {
                return;
            }

            foreach (ISyntaxComponent dslInfo in dslFile.DslInfos) {
                Calculator.LoadDsl(dslInfo);
            }
        }
        private static void OnDslError(string err)
        {
            DslErrorInfo.AppendLine(err);
            Log(err);
        }

        [ThreadStatic]
        private static bool s_TimeStatisticOn;
        [ThreadStatic]
        private static string s_ScriptDirectory;
        [ThreadStatic]
        private static DslCalculator s_Calculator;

        [ThreadStatic]
        private static List<string> s_EmptyStringList;
        [ThreadStatic]
        private static List<BoxedValue> s_EmptyBoxedValueList;
        [ThreadStatic]
        private static StringBuilder s_DslErrorInfo;
        [ThreadStatic]
        private static SortedList<string, string> s_UserApiDocs;
        [ThreadStatic]
        private static Dictionary<int, Tuple<Stack<IEnumerator>, AsyncCalcResult, AsyncTaskRuntimeContext>> s_AsyncTasks;
        [ThreadStatic]
        private static int s_AsyncTaskIdSeed;
        [ThreadStatic]
        private static List<int> s_AsyncTaskTickKeys;

    }
}
#pragma warning restore 8600,8601,8602,8603,8604,8618,8619,8620,8625,CA1416
