using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using static GlslRewriter.ComputeGraph;
using static GlslRewriter.Program;
using static System.Reflection.Metadata.BlobBuilder;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;

namespace GlslRewriter
{
    public readonly ref struct ComputeSetting
    {
        public int MaxLevel { get; init; } = 0;
        public int MaxLength { get; init; } = 0;
        public bool UseMultilineComments { get; init; } = false;
        public bool VariableExpandedOnlyOnce { get; init; } = false;
        public bool WithValue { get; init; } = false;
        public bool DontCacheExpression { get; init; } = false;

        public ComputeSetting() { }
        public ComputeSetting(int maxLevel, int maxLength, bool useMultilineComments, bool variableExpandedOnlyOnce)
        {
            MaxLevel = maxLevel;
            MaxLength = maxLength;
            UseMultilineComments = useMultilineComments;
            VariableExpandedOnlyOnce = variableExpandedOnlyOnce;
        }
        public ComputeSetting(int maxLevel, int maxLength, bool useMultilineComments, bool variableExpandedOnlyOnce, bool withValue, bool dontCacheExpression)
        {
            MaxLevel = maxLevel;
            MaxLength = maxLength;
            UseMultilineComments = useMultilineComments;
            VariableExpandedOnlyOnce = variableExpandedOnlyOnce;
            WithValue = withValue;
            DontCacheExpression = dontCacheExpression;
        }
    }
    public ref struct ControlInfo
    {
        public bool ExistsContinue = false;
        public bool ExistsBreak = false;
        public bool ExistsDiscard = false;
        public bool ExistsReturn = false;

        public ControlInfo() { }
        public bool NeedBreakLoop()
        {
            return ExistsBreak || ExistsDiscard || ExistsReturn;
        }
        public bool NeedBreakBlock()
        {
            return ExistsContinue || ExistsBreak || ExistsDiscard || ExistsReturn;
        }
    }
    public class ComputeGraphNode
    {
        public ComputeGraphNode(FuncInfo? ownFunc, string type)
        {
            Id = s_NextId++;
            OwnFunc = ownFunc;
            Type = type;
        }
        public void AddPrev(ComputeGraphNode node)
        {
            PrevNodes.Add(node);
        }
        public void AddNext(ComputeGraphNode node)
        {
            NextNodes.Add(node);
        }
        public void AddOut(ComputeGraphNode node)
        {
            Debug.Assert(!OutNodes.Contains(node));
            OutNodes.Add(node);
        }

        public void VisitPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            visits.Add(this);
            bool cont0 = visitorCallback(this);
            if (!cont0)
                return;
            VisitChildPrev(ownFunc, visits, visitorCallback);
            foreach (var node in PrevNodes) {
                if (visits.Contains(node)) {
                    bool cont = visitorCallback(this);
                    if (!cont)
                        return;
                }
                else if (ownFunc == node.OwnFunc) {
                    node.VisitPrev(ownFunc, visits, visitorCallback);
                }
            }
        }
        public void VisitNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            visits.Add(this);
            bool cont0 = visitorCallback(this);
            if (!cont0)
                return;
            VisitChildNext(ownFunc, visits, visitorCallback);
            foreach (var node in NextNodes) {
                if (visits.Contains(node)) {
                    bool cont = visitorCallback(this);
                    if (!cont)
                        return;
                }
                else if (ownFunc == node.OwnFunc) {
                    node.VisitNext(ownFunc, visits, visitorCallback);
                }
            }
            foreach (var node in OutNodes) {
                if (visits.Contains(node)) {
                    bool cont = visitorCallback(this);
                    if (!cont)
                        return;
                }
                else if (ownFunc == node.OwnFunc) {
                    node.VisitNext(ownFunc, visits, visitorCallback);
                }
            }
        }
        public void VisitAllPrev(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            visits.Add(this);
            bool cont0 = visitorCallback(this);
            if (!cont0)
                return;
            VisitChildAllPrev(visits, visitorCallback);
            foreach (var node in PrevNodes) {
                if (visits.Contains(node)) {
                    bool cont = visitorCallback(this);
                    if (!cont)
                        return;
                }
                else {
                    node.VisitAllPrev(visits, visitorCallback);
                }
            }
        }
        public void VisitAllNext(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            visits.Add(this);
            bool cont0 = visitorCallback(this);
            if (!cont0)
                return;
            VisitChildAllNext(visits, visitorCallback);
            foreach (var node in NextNodes) {
                if (visits.Contains(node)) {
                    bool cont = visitorCallback(this);
                    if (!cont)
                        return;
                }
                else {
                    node.VisitAllNext(visits, visitorCallback);
                }
            }
            foreach (var node in OutNodes) {
                if (visits.Contains(node)) {
                    bool cont = visitorCallback(this);
                    if (!cont)
                        return;
                }
                else {
                    node.VisitAllNext(visits, visitorCallback);
                }
            }
        }

        public void Print(FuncInfo? ownFunc, HashSet<ComputeGraphNode> prevVisits, HashSet<ComputeGraphNode> nextVisits, int indent)
        {
            PrintInfo(indent);
            PrintPrev(ownFunc, prevVisits, indent);
            PrintNext(ownFunc, nextVisits, indent);
        }
        public void PrintIndent(int indent)
        {
            Console.Write(Literal.GetSpaceString(indent));
        }
        public void PrintInfo(int indent)
        {
            PrintIndent(indent);
            Console.WriteLine("[Node:{0}, {1}]", Id, GetType().Name);
            PrintFieldInfo(indent + 1);
        }

        public void ResetValue()
        {
            m_Value = null;
        }
        public void ResetExpression()
        {
            m_Expression = string.Empty;
            m_ExpressionIndent = 0;
        }
        public string GetValue()
        {
            HashSet<ComputeGraphNode> visits = new HashSet<ComputeGraphNode>(Config.ActiveConfig.SettingInfo.ComputeGraphNodesCapacity);
            var cinfo = new ControlInfo();
            string val = CalcValue(visits, ref cinfo);
            return val;
        }
        public string GetExpression(in ComputeSetting setting)
        {
            Dictionary<string, int> usedVars = new Dictionary<string, int>(Config.ActiveConfig.SettingInfo.ShaderVariablesCapacity);
            var sb = new StringBuilder(setting.MaxLength + Config.ActiveConfig.SettingInfo.StringBufferCapacitySurplus);
            HashSet<ComputeGraphNode> visits = new HashSet<ComputeGraphNode>(Config.ActiveConfig.SettingInfo.ComputeGraphNodesCapacity);
            int curMaxLevel = 0;
            GenerateExpression(sb, 0, 0, ref curMaxLevel, out var subMaxLevel, setting, usedVars, visits, null);
            string result;
            if (setting.DontCacheExpression) {
                result = sb.ToString();
            }
            else {
                m_Expression = sb.ToString();
                m_ExpressionIndent = 0;
                result = m_Expression;
            }
            return result;
        }

        public void DoCalc()
        {
            HashSet<ComputeGraphNode> visits = new HashSet<ComputeGraphNode>(Config.ActiveConfig.SettingInfo.ComputeGraphNodesCapacity);
            var cinfo = new ControlInfo();
            CalcValue(visits, ref cinfo);
        }
        public string CalcValue(HashSet<ComputeGraphNode> visits, ref ControlInfo cinfo)
        {
            if (null == m_Value) {
                if (Config.ActiveConfig.SettingInfo.DebugMode) {
                    if (visits.Contains(this)) {
                        Debug.Assert(false);
                    }
                    else {
                        visits.Add(this);
                    }
                }
                else {
                    Debug.Assert(visits.Count == 0);
                }
                TryCalcValue(visits, ref cinfo);
                visits.Remove(this);
            }
            return null != m_Value ? m_Value : string.Empty;
        }
        public void GenerateExpression(StringBuilder sb, int indent, int curLevel, ref int mergeMaxLevel, out int curMaxLevel, in ComputeSetting setting, Dictionary<string, int> usedVars, HashSet<ComputeGraphNode> visits, ComputeGraphNode? fromNode)
        {
            curMaxLevel = curLevel;
            if (setting.DontCacheExpression || setting.VariableExpandedOnlyOnce || string.IsNullOrEmpty(m_Expression)) {
                if (Config.ActiveConfig.SettingInfo.DebugMode) {
                    if (visits.Contains(this)) {
                        Debug.Assert(false);
                    }
                    else {
                        visits.Add(this);
                    }
                }
                else {
                    Debug.Assert(visits.Count == 0);
                }
                if (sb.Length > setting.MaxLength) {
                    sb.Append("...");
                }
                else {
                    TryGenerateExpression(sb, indent, curLevel, ref curMaxLevel, setting, usedVars, visits, fromNode);
                }
                visits.Remove(this);
            }
            else if (setting.UseMultilineComments) {
                if (indent == m_ExpressionIndent) {
                    sb.Append(m_Expression);
                }
                else {
                    var lines = m_Expression.Split(s_LineSplits, StringSplitOptions.RemoveEmptyEntries);
                    if (indent > m_ExpressionIndent) {
                        foreach (var line in lines) {
                            sb.AppendLine();
                            sb.Append(Literal.GetIndentString(indent - m_ExpressionIndent));
                            sb.Append(line);
                        }
                    }
                    else {
                        foreach (var line in lines) {
                            sb.AppendLine();
                            sb.Append(line.Substring(m_ExpressionIndent - indent));
                        }
                    }
                }
            }
            else {
                sb.Append(m_Expression);
            }
            if(mergeMaxLevel < curMaxLevel)
                mergeMaxLevel = curMaxLevel;
        }

        internal void PrintPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            visits.Add(this);
            PrintChildPrev(ownFunc, visits, indent);
            if (PrevNodes.Count > 0) {
                PrintIndent(indent + 1);
                Console.WriteLine("[PrevNodes]");
                foreach (var node in PrevNodes) {
                    node.PrintInfo(indent + 2);
                    if (!visits.Contains(node)) {
                        if (ownFunc == node.OwnFunc) {
                            node.PrintPrev(ownFunc, visits, indent + 2);
                        }
                    }
                }
            }
        }
        internal void PrintNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            visits.Add(this);
            PrintChildNext(ownFunc, visits, indent);
            if (NextNodes.Count > 0) {
                PrintIndent(indent + 1);
                Console.WriteLine("[NextNodes]");
                foreach (var node in NextNodes) {
                    node.PrintInfo(indent + 2);
                    if (!visits.Contains(node)) {
                        if (ownFunc == node.OwnFunc) {
                            node.PrintNext(ownFunc, visits, indent + 2);
                        }
                    }
                }
            }
            if (OutNodes.Count > 0) {
                PrintIndent(indent + 1);
                Console.WriteLine("[OutNodes]");
                foreach (var node in OutNodes) {
                    node.PrintInfo(indent + 2);
                    if (!visits.Contains(node)) {
                        if (ownFunc == node.OwnFunc) {
                            node.PrintNext(ownFunc, visits, indent + 2);
                        }
                    }
                }
            }
        }

        protected virtual void VisitChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
        }
        protected virtual void VisitChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
        }
        protected virtual void VisitChildAllPrev(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
        }
        protected virtual void VisitChildAllNext(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
        }

        protected virtual void PrintChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
        }
        protected virtual void PrintChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
        }
        protected virtual void PrintFieldInfo(int indent)
        {
            if (!string.IsNullOrEmpty(Type)) {
                PrintIndent(indent);
                Console.Write("Type:");
                Console.WriteLine(Type);
            }
        }

        protected virtual void TryCalcValue(HashSet<ComputeGraphNode> visits, ref ControlInfo cinfo)
        {
        }
        protected virtual void TryGenerateExpression(StringBuilder sb, int indent, int curLevel, ref int curMaxLevel, in ComputeSetting setting, Dictionary<string, int> usedVars, HashSet<ComputeGraphNode> visits, ComputeGraphNode? fromNode)
        {
        }

        public uint Id { get; init; }
        public FuncInfo? OwnFunc = null;
        public string Type = string.Empty;
        public List<ComputeGraphNode> PrevNodes = new List<ComputeGraphNode>();
        public List<ComputeGraphNode> NextNodes = new List<ComputeGraphNode>();
        public List<ComputeGraphNode> OutNodes = new List<ComputeGraphNode>();

        protected string? m_Value = null;
        protected string m_Expression = string.Empty;
        protected int m_ExpressionIndent = 0;

        protected static void VisitChildPrevHelper(ComputeGraphNode? node, FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            if (null != node) {
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.VisitPrev(ownFunc, visits, visitorCallback);
                    }
                }
            }
        }
        protected static void VisitChildNextHelper(ComputeGraphNode? node, FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            if (null != node) {
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.VisitNext(ownFunc, visits, visitorCallback);
                    }
                }
            }
        }
        protected static void VisitChildAllPrevHelper(ComputeGraphNode? node, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            if (null != node) {
                if (!visits.Contains(node)) {
                    node.VisitAllPrev(visits, visitorCallback);
                }
            }
        }
        protected static void VisitChildAllNextHelper(ComputeGraphNode? node, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            if (null != node) {
                if (!visits.Contains(node)) {
                    node.VisitAllNext(visits, visitorCallback);
                }
            }
        }

        protected static void VisitChildPrevHelper<T>(IList<T> nodes, FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback) where T : ComputeGraphNode
        {
            foreach(var node in nodes) {
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.VisitPrev(ownFunc, visits, visitorCallback);
                    }
                }
            }
        }
        protected static void VisitChildNextHelper<T>(IList<T> nodes, FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback) where T : ComputeGraphNode
        {
            foreach (var node in nodes) {
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.VisitNext(ownFunc, visits, visitorCallback);
                    }
                }
            }
        }
        protected static void VisitChildAllPrevHelper<T>(IList<T> nodes, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback) where T : ComputeGraphNode
        {
            foreach (var node in nodes) {
                if (!visits.Contains(node)) {
                    node.VisitAllPrev(visits, visitorCallback);
                }
            }
        }
        protected static void VisitChildAllNextHelper<T>(IList<T> nodes, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback) where T : ComputeGraphNode
        {
            foreach (var node in nodes) {
                if (!visits.Contains(node)) {
                    node.VisitAllNext(visits, visitorCallback);
                }
            }
        }

        public static void ResetStatic()
        {
            s_NextId = 1;
        }
        private static uint s_NextId = 1;
        private static char[] s_LineSplits = new char[] { '\r', '\n' };
    }
    public class ComputeGraphConstNode : ComputeGraphNode
    {
        public ComputeGraphConstNode(FuncInfo? ownFunc, string type, string val) : base(ownFunc, type)
        {
            Value = val;
        }
        protected override void PrintFieldInfo(int indent)
        {
            base.PrintFieldInfo(indent);

            PrintIndent(indent);
            Console.Write("Value:");
            Console.WriteLine(Value);
        }
        protected override void TryCalcValue(HashSet<ComputeGraphNode> visits, ref ControlInfo cinfo)
        {
            string val = Value;
            val = Calculator.ReStringNumeric(val);
            m_Value = val;
        }
        protected override void TryGenerateExpression(StringBuilder sb, int indent, int curLevel, ref int curMaxLevel, in ComputeSetting setting, Dictionary<string, int> usedVars, HashSet<ComputeGraphNode> visits, ComputeGraphNode? fromNode)
        {
            sb.Append(Value);
        }

        public string Value = string.Empty;
    }
    public class ComputeGraphVarNode : ComputeGraphNode
    {
        public ComputeGraphVarNode(FuncInfo? ownFunc, string type, string name) : base(ownFunc, type)
        {
            VarName = name;
        }
        protected override void PrintFieldInfo(int indent)
        {
            base.PrintFieldInfo(indent);

            PrintIndent(indent);
            Console.Write("VarName:");
            Console.WriteLine(VarName);

            PrintIndent(indent);
            Console.Write("IsInOut:");
            Console.WriteLine(IsInOut);

            PrintIndent(indent);
            Console.Write("IsOut:");
            Console.WriteLine(IsOut);
        }
        protected override void TryCalcValue(HashSet<ComputeGraphNode> visits, ref ControlInfo cinfo)
        {
            if (VariableTable.GetVarValue(VarName, Type, out var val)) {
                if (string.IsNullOrEmpty(val)) {
                    m_Value = "{complex}";
                }
                else {
                    m_Value = val;
                }
            }
            else {
                if (PrevNodes.Count > 0) {
                    //取最后一次赋值（多次赋值仅出现在分支情形的phi变量赋值），方便代码分析中注释掉不执行的if语句后进行正确计算
                    m_Value = PrevNodes[PrevNodes.Count - 1].CalcValue(visits, ref cinfo);
                }
                else {
                    m_Value = VarName;
                }
            }
        }
        protected override void TryGenerateExpression(StringBuilder sb, int indent, int curLevel, ref int curMaxLevel, in ComputeSetting setting, Dictionary<string, int> usedVars, HashSet<ComputeGraphNode> visits, ComputeGraphNode? fromNode)
        {
            bool withValue = false;
            if (setting.WithValue) {
                if (fromNode is not ComputeGraphCalcNode) {
                    //这种情形对变量结点应该不会出现
                    withValue = true;
                }
                else if (fromNode is ComputeGraphCalcNode calcNode && calcNode.Operator != "[]" && calcNode.Operator != ".") {
                    //只对数组或成员访问计算结点的叶子结点标注计算值
                    if (calcNode.Operator == "utof" || calcNode.Operator == "uintBitsToFloat" || calcNode.Operator == "ftou" || calcNode.Operator == "floatBitsToUint") {
                        //使用类型转换的uniform，值标注在类型转换函数调用上
                    }
                    else {
                        withValue = true;
                    }
                }
            }
            if (null != OwnFunc && PrevNodes.Count > 0) {
                if (Config.ActiveConfig.SettingInfo.SplitOnVariables.Contains(VarName)) {
                    if (withValue) {
                        sb.Append("{");
                    }
                    sb.Append(VarName);
                    if (withValue) {
                        sb.Append(" : ");
                        sb.Append(m_Value);
                        sb.Append("}");
                    }
                }
                else {
                    if (setting.VariableExpandedOnlyOnce && usedVars.TryGetValue(VarName, out var varMaxLevel)) {
                        if (Config.ActiveConfig.SettingInfo.AutoSplitLevel >= 0 && Config.ActiveConfig.SettingInfo.AutoSplitLevelForRepeatExpression <= varMaxLevel - curLevel) {
                            Config.ActiveConfig.SettingInfo.AutoSplitAddVariable(VarName);
                        }
                        if (withValue) {
                            sb.Append("{");
                        }
                        sb.Append(VarName);
                        if (withValue) {
                            sb.Append(" : ");
                            sb.Append(m_Value);
                            sb.Append("}");
                        }
                    }
                    else {
                        if (curLevel < setting.MaxLevel) {
                            //取最后一次赋值（多次赋值仅出现在分支情形的phi变量赋值），方便代码分析中注释掉不执行的if语句后进行正确计算
                            int start = sb.Length;
                            int tmpMaxLevel = curLevel;
                            PrevNodes[PrevNodes.Count - 1].GenerateExpression(sb, indent, curLevel + 1, ref tmpMaxLevel, out varMaxLevel, setting, usedVars, visits, this);
                            if (Config.ActiveConfig.SettingInfo.AutoSplitLevel >= 0) {
                                if (IsSplitOn(PrevNodes[PrevNodes.Count - 1], varMaxLevel - curLevel)) {
                                    Config.ActiveConfig.SettingInfo.AutoSplitAddVariable(VarName);
                                    tmpMaxLevel = curLevel;
                                }
                                else if (Config.ActiveConfig.SettingInfo.AutoSplitLevel <= varMaxLevel - curLevel) {
                                    Config.ActiveConfig.SettingInfo.AutoSplitAddVariable(VarName);
                                    tmpMaxLevel = curLevel;
                                }
                            }
                            usedVars[VarName] = tmpMaxLevel;
                            if (curMaxLevel < tmpMaxLevel)
                                curMaxLevel = tmpMaxLevel;
                            int end = sb.Length;
                            if (!setting.VariableExpandedOnlyOnce && !setting.DontCacheExpression) {
                                if (end > start) {
                                    m_Expression = sb.ToString(start, end - start);
                                    m_ExpressionIndent = indent;
                                }
                            }
                        }
                        else {
                            //这里不用拆分变量，通过其它拆分多次迭代应该就可以消除，如果在这里也拆分可能会引入比较多的简单表达式
                            sb.Append(VarName + "...");
                        }
                    }
                }
            }
            else {
                if (withValue) {
                    sb.Append("{");
                }
                if (null != OwnFunc) {
                    //未赋值的局部变量，这里一般是phi变量，添加到拆分表达式变量列表记录变量使用
                    Config.ActiveConfig.SettingInfo.AutoSplitAddVariable(VarName);
                }
                sb.Append(VarName);
                if (withValue) {
                    sb.Append(" : ");
                    sb.Append(m_Value);
                    sb.Append("}");
                }
            }
        }

        private bool IsSplitOn(ComputeGraphNode prevNode, int deltaLevel)
        {
            bool ret = false;
            if(prevNode is ComputeGraphCalcNode calcNode && calcNode.Operator=="=" && prevNode.PrevNodes[0] is ComputeGraphCalcNode calcNode2 &&
                Config.ActiveConfig.SettingInfo.AutoSplitOnFuncs.TryGetValue(calcNode2.Operator, out var lvl) && lvl<=deltaLevel) {
                ret = true;
            }
            return ret;
        }

        public string VarName = string.Empty;
        public bool IsInOut = false;
        public bool IsOut = false;
        public bool IsParam = false;

    }
    public class ComputeGraphCalcNode : ComputeGraphNode
    {
        public ComputeGraphCalcNode(FuncInfo? ownFunc, string type, string op) : base(ownFunc, type)
        {
            Operator = op;
        }
        protected override void PrintFieldInfo(int indent)
        {
            base.PrintFieldInfo(indent);

            PrintIndent(indent);
            Console.Write("Operator:");
            Console.WriteLine(Operator);
        }
        protected override void TryCalcValue(HashSet<ComputeGraphNode> visits, ref ControlInfo cinfo)
        {
            if (Operator.Length > 0 && (char.IsLetter(Operator[0]) || Operator[0] == '_')) {
                var args = new List<string>();
                foreach (var p in PrevNodes) {
                    args.Add(p.CalcValue(visits, ref cinfo));
                }
                string type = Type;
                if (Config.CalcFunc(Operator, args, ref type, out var val)) {
                    m_Value = val;
                }
            }
            else if (Operator == ".") {
                bool handled = false;
                if (PrevNodes[1] is ComputeGraphConstNode constNode) {
                    if (PrevNodes[0] is ComputeGraphVarNode varNode) {
                        //var.member
                        if(VariableTable.ObjectGetValue(varNode, constNode.Value, out var val)) {
                            m_Value = val;
                            handled = true;
                        }
                    }
                    else if (PrevNodes[0] is ComputeGraphCalcNode calcNode) {
                        if (calcNode.Operator == "[]" && calcNode.PrevNodes[0] is ComputeGraphVarNode vnode2 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode2) {
                            //var[ix].member
                            if(VariableTable.ObjectArrayGetValue(vnode2, cnode2.Value, constNode.Value, out var val)) {
                                m_Value = val;
                                handled = true;
                            }
                        }
                    }
                }
                if (!handled) {
                    string objType = PrevNodes[0].Type;
                    string type = Type;
                    string objVal = PrevNodes[0].CalcValue(visits, ref cinfo);
                    string member = PrevNodes[1].CalcValue(visits, ref cinfo);
                    if (Calculator.CalcMember(objType, objVal, member, ref type, out var val, out var supported)) {
                        m_Value = val;
                    }
                    else if (!supported) {
                        Console.WriteLine("member '{0}' is unsupported, please support it.", member);
                    }
                }
            }
            else if (Operator == "[]") {
                bool handled = false;
                if (PrevNodes[0] is ComputeGraphVarNode varNode) {
                    //var[ix]
                    if (PrevNodes[1] is ComputeGraphConstNode constNode) {
                        if (VariableTable.ArrayGetValue(varNode, constNode.Value, out var val)) {
                            m_Value = val;
                            handled = true;
                        }
                    }
                    else {
                        string ix = PrevNodes[1].CalcValue(visits, ref cinfo);
                        if (VariableTable.ArrayGetValue(varNode, ix, out var val)) {
                            m_Value = val;
                            handled = true;
                        }
                    }
                }
                if (!handled) {
                    string objType = PrevNodes[0].Type;
                    string type = Type;
                    string objVal = PrevNodes[0].CalcValue(visits, ref cinfo);
                    string ix = PrevNodes[1].CalcValue(visits, ref cinfo);
                    if (Calculator.CalcMember(objType, objVal, ix, ref type, out var val, out var supported)) {
                        m_Value = val;
                    }
                    else if (!supported) {
                        Console.WriteLine("type {0}'s subscript is unsupported, please support it.", type);
                    }
                }
            }
            else if (Operator == "=") {
                if (NextNodes[0] is ComputeGraphVarNode vnode) {
                    //var = exp
                    if (PrevNodes[0] is ComputeGraphVarNode vnode2) {
                        VariableTable.AssignValue(vnode, vnode2);
                    }
                    else if (PrevNodes[0] is ComputeGraphCalcNode calcNode2) {
                        VariableTable.AssignValue(vnode, calcNode2, visits, ref cinfo);
                    }
                    else {
                        VariableTable.AssignValue(vnode, PrevNodes[0].CalcValue(visits, ref cinfo));
                    }
                }
                else if (NextNodes[0] is ComputeGraphCalcNode calcNode) {
                    if (calcNode.Operator=="." && calcNode.PrevNodes[0] is ComputeGraphVarNode vnode2 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode2) {
                        //var.member = exp
                        VariableTable.ObjectAssignValue(vnode2, cnode2.Value, PrevNodes[0].CalcValue(visits, ref cinfo));
                    }
                    else if (calcNode.Operator == "[]" && calcNode.PrevNodes[0] is ComputeGraphVarNode vnode3 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode3) {
                        //var[ix] = exp
                        if (PrevNodes[0] is ComputeGraphVarNode varNode) {
                            VariableTable.ArrayAssignValue(vnode3, cnode3.Value, varNode);
                        }
                        else if (PrevNodes[0] is ComputeGraphCalcNode calcNode2) {
                            VariableTable.ArrayAssignValue(vnode3, cnode3.Value, calcNode2, visits, ref cinfo);
                        }
                        else {
                            VariableTable.ArrayAssignValue(vnode3, cnode3.Value, PrevNodes[0].CalcValue(visits, ref cinfo));
                        }
                    }
                    else if (calcNode.Operator == "." && calcNode.PrevNodes[0] is ComputeGraphCalcNode calcNode2 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode4) {
                        if (calcNode2.Operator == "[]" && calcNode2.PrevNodes[0] is ComputeGraphVarNode vnode5 && calcNode2.PrevNodes[1] is ComputeGraphConstNode cnode5) {
                            //var[ix].member = exp
                            //暂未遇到
                        }
                    }
                }
                m_Value = PrevNodes[0].CalcValue(visits, ref cinfo);
            }
            else if (PrevNodes.Count == 3) {
                string type = Type;
                string cond = PrevNodes[0].CalcValue(visits, ref cinfo);
                string opd1 = PrevNodes[1].CalcValue(visits, ref cinfo);
                string opd2 = PrevNodes[2].CalcValue(visits, ref cinfo);
                if (Calculator.CalcCondExp(cond, opd1, opd2, ref type, out var val, out var supported)) {
                    m_Value = val;
                }
                else if (!supported) {
                    Console.WriteLine("condition expression is unsupported, please support it.");
                }
            }
            else if (PrevNodes.Count == 2) {
                string type = Type;
                string opd1 = PrevNodes[0].CalcValue(visits, ref cinfo);
                string opd2 = PrevNodes[1].CalcValue(visits, ref cinfo);
                if (Calculator.CalcBinary(Operator, opd1, opd2, ref type, out var val, out var supported)) {
                    m_Value = val;
                }
                else if (!supported) {
                    Console.WriteLine("binary operator '{0}' is unsupported, please support it.", Operator);
                }
            }
            else {
                string type = Type;
                string opd = PrevNodes[0].CalcValue(visits, ref cinfo);
                if (Calculator.CalcUnary(Operator, opd, ref type, out var val, out var supported)) {
                    m_Value = val;
                }
                else if (!supported) {
                    Console.WriteLine("unary operator '{0}' is unsupported, please support it.", Operator);
                }
            }
        }
        protected override void TryGenerateExpression(StringBuilder sb, int indent, int curLevel, ref int curMaxLevel, in ComputeSetting setting, Dictionary<string, int> usedVars, HashSet<ComputeGraphNode> visits, ComputeGraphNode? fromNode)
        {
            int subMaxLevel = 0;
            if (Operator.Length > 0 && (char.IsLetter(Operator[0]) || Operator[0] == '_')) {
                if (Operator == "fma") {
                    //fma(a,b,c) => a*b+c
                    sb.Append("(");
                    PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(" * ");
                    PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(" + ");
                    PrevNodes[2].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(")");
                }
                else if (Operator == "min" && PrevNodes[0] is ComputeGraphCalcNode cnode && cnode.Operator == "max") {
                    //min(max(v,a),b) => clamp(v,a,b)
                    sb.Append("clamp");
                    sb.Append("(");
                    cnode.PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode);
                    sb.Append(", ");
                    cnode.PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode);
                    sb.Append(", ");
                    PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(")");
                }
                else if (Operator == "max" && PrevNodes[0] is ComputeGraphCalcNode cnode2 && cnode2.Operator == "min") {
                    //max(min(v,b),a) => clamp(v,a,b)
                    sb.Append("clamp");
                    sb.Append("(");
                    cnode2.PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode2);
                    sb.Append(", ");
                    PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(", ");
                    cnode2.PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode2);
                    sb.Append(")");
                }
                else if (Operator == "min" && PrevNodes[0] is ComputeGraphVarNode vnode && vnode.PrevNodes[vnode.PrevNodes.Count - 1] is ComputeGraphCalcNode cnode3 && cnode3.Operator=="="
                    && cnode3.PrevNodes[0] is ComputeGraphCalcNode cnode31 && cnode31.Operator == "max") {
                    //min({vname = max(v,a)},b) => clamp(v,a,b)
                    sb.Append("clamp");
                    sb.Append("(");
                    cnode31.PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode31);
                    sb.Append(",  ");
                    cnode31.PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode31);
                    sb.Append(", ");
                    PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(")");
                }
                else if (Operator == "max" && PrevNodes[0] is ComputeGraphVarNode vnode2 && vnode2.PrevNodes[vnode2.PrevNodes.Count - 1] is ComputeGraphCalcNode cnode4 && cnode4.Operator == "="
                    && cnode4.PrevNodes[0] is ComputeGraphCalcNode cnode41 && cnode41.Operator == "min") {
                    //max({vname = min(v,b)},a) => clamp(v,a,b)
                    sb.Append("clamp");
                    sb.Append("(");
                    cnode41.PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode41);
                    sb.Append(", ");
                    PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(", ");
                    cnode41.PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode41);
                    sb.Append(")");
                }
                else if(Operator == "for") {
                    sb.Append("for");
                    sb.Append("(");
                    int i = 0;
                    for(; i < PrevNodes.Count; ++i) {
                        var p = PrevNodes[i];
                        if (i > 0)
                            sb.Append("; ");
                        p.GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    }
                    for (; i < 3; ++i) {
                        if (i > 0)
                            sb.Append("; ");
                    }
                    sb.Append(")");
                }
                else {
                    bool withValue = false;
                    if(setting.WithValue && (Operator == "utof" || Operator== "uintBitsToFloat"
                        || Operator == "ftou" || Operator == "floatBitsToUint")){
                        //使用类型转换的uniform，在转换函数调用上标注计算值
                        withValue = true;
                        sb.Append("{");
                    }
                    sb.Append(Operator);
                    sb.Append("(");
                    bool first = true;
                    foreach (var p in PrevNodes) {
                        if (first)
                            first = false;
                        else
                            sb.Append(", ");
                        p.GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    }
                    sb.Append(")");
                    if (withValue) {
                        sb.Append(" : ");
                        sb.Append(m_Value);
                        sb.Append("}");
                    }
                }
            }
            else if (Operator == ".") {
                bool withValue = false;
                if (setting.WithValue) {
                    if (fromNode is not ComputeGraphCalcNode) {
                        //如果父结点不是计算结点（常量或变量），则当前成员访问结点已经是成员访问叶子结点，在此结点上标注计算值
                        withValue = true;
                        sb.Append("{");
                    }
                    else if(fromNode is ComputeGraphCalcNode calcNode && calcNode.Operator != "[]" && calcNode.Operator != ".") {
                        //只对数组或成员访问计算结点的叶子结点标注计算值
                        if (calcNode.Operator == "utof" || calcNode.Operator == "uintBitsToFloat" ||
                            calcNode.Operator == "ftou" || calcNode.Operator == "floatBitsToUint") {
                            //使用类型转换的uniform，值标注在类型转换函数调用上
                        }
                        else {
                            withValue = true;
                            sb.Append("{");
                        }
                    }
                }
                PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                sb.Append(".");
                PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                if (withValue) {
                    sb.Append(" : ");
                    sb.Append(m_Value);
                    sb.Append("}");
                }
            }
            else if (Operator == "[]") {
                bool withValue = false;
                if (setting.WithValue) {
                    if (fromNode is not ComputeGraphCalcNode) {
                        //如果父结点不是计算结点（常量或变量），则当前成员访问结点已经是数组元素访问叶子结点，在此结点上标注计算值
                        withValue = true;
                        sb.Append("{");
                    }
                    else if (fromNode is ComputeGraphCalcNode calcNode && calcNode.Operator != "[]" && calcNode.Operator != ".") {
                        //只对数组或成员访问计算结点的叶子结点标注计算值
                        if (calcNode.Operator == "utof" || calcNode.Operator == "uintBitsToFloat" ||
                            calcNode.Operator == "ftou" || calcNode.Operator == "floatBitsToUint") {
                            //使用类型转换的uniform，值标注在类型转换函数调用上
                        }
                        else {
                            withValue = true;
                            sb.Append("{");
                        }
                    }
                }
                PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                sb.Append("[");
                PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                sb.Append("]");
                if (withValue) {
                    sb.Append(" : ");
                    sb.Append(m_Value);
                    sb.Append("}");
                }
            }
            else if (Operator == "=") {
                if (setting.UseMultilineComments) {
                    sb.AppendLine();
                    sb.Append(Literal.GetIndentString(indent));
                    sb.Append("{");
                    AppendAssignLeft(sb, NextNodes[0]);
                    sb.Append(" = ");
                }
                PrevNodes[0].GenerateExpression(sb, indent + 1, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                if (setting.UseMultilineComments) {
                    sb.AppendLine();
                    sb.Append(Literal.GetIndentString(indent));
                    sb.Append("}");
                }
            }
            else if (PrevNodes.Count == 3) {
                sb.Append("(");
                PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                sb.Append(" ? ");
                PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                sb.Append(" : ");
                PrevNodes[2].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                sb.Append(")");
            }
            else if (PrevNodes.Count == 2) {
                sb.Append("(");
                PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                sb.Append(" ");
                sb.Append(Operator);
                sb.Append(" ");
                PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                sb.Append(")");
            }
            else {
                sb.Append("(");
                sb.Append(Operator);
                sb.Append(" ");
                PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                sb.Append(")");
            }
        }

        private void AppendAssignLeft(StringBuilder sb, ComputeGraphNode leftNode)
        {
            if (leftNode is ComputeGraphVarNode vnode) {
                sb.Append(vnode.VarName);
            }
            else if (leftNode is ComputeGraphConstNode cnode) {
                sb.Append(cnode.Value);
            }
            else if (leftNode is ComputeGraphCalcNode calcNode) {
                if (calcNode.Operator == "." && calcNode.PrevNodes[0] is ComputeGraphVarNode vnode2 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode2) {
                    sb.Append(vnode2.VarName);
                    sb.Append(".");
                    sb.Append(cnode2.Value);
                }
                else if (calcNode.Operator == "[]" && calcNode.PrevNodes[0] is ComputeGraphVarNode vnode3 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode3) {
                    sb.Append(vnode3.VarName);
                    sb.Append("[");
                    sb.Append(cnode3.Value);
                    sb.Append("]");
                }
                else if (calcNode.Operator == "." && calcNode.PrevNodes[0] is ComputeGraphCalcNode calcNode2 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode4) {
                    if (calcNode2.Operator == "[]" && calcNode2.PrevNodes[0] is ComputeGraphVarNode vnode5 && calcNode2.PrevNodes[1] is ComputeGraphConstNode cnode5) {
                        sb.Append(vnode5.VarName);
                        sb.Append("[");
                        sb.Append(cnode5.Value);
                        sb.Append("].");
                        sb.Append(cnode4.Value);
                    }
                    else {
                        sb.Append("{complex}");
                    }
                }
                else {
                    sb.Append("{complex}");
                }
            }
            else {
                Debug.Assert(false);
            }
        }

        public string Operator = string.Empty;

    }
    public class ComputeGraphBreakNode : ComputeGraphNode
    {
        public ComputeGraphBreakNode(FuncInfo? ownFunc, string keyword) : base(ownFunc, string.Empty)
        {
            Keyword = keyword;
        }
        protected override void VisitChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildPrevHelper(Expression, ownFunc, visits, visitorCallback);
        }
        protected override void VisitChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildNextHelper(Expression, ownFunc, visits, visitorCallback);
        }
        protected override void VisitChildAllPrev(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildAllPrevHelper(Expression, visits, visitorCallback);
        }
        protected override void VisitChildAllNext(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildAllNextHelper(Expression, visits, visitorCallback);
        }
        protected override void PrintChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            if (null != Expression) {
                var node = Expression;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintPrev(ownFunc, visits, indent + 1);
                    }
                }
            }
        }
        protected override void PrintChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            if (null != Expression) {
                var node = Expression;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintNext(ownFunc, visits, indent + 1);
                    }
                }
            }
        }
        protected override void PrintFieldInfo(int indent)
        {
            base.PrintFieldInfo(indent);

            PrintIndent(indent);
            Console.WriteLine(Keyword);
        }
        protected override void TryCalcValue(HashSet<ComputeGraphNode> visits, ref ControlInfo cinfo)
        {
            if (Keyword == "break")
                cinfo.ExistsBreak = true;
            else if (Keyword == "continue")
                cinfo.ExistsContinue = true;
            else if (Keyword == "discard")
                cinfo.ExistsDiscard = true;
            else if (Keyword == "return")
                cinfo.ExistsReturn = true;
        }

        public string Keyword = string.Empty;
        public ComputeGraphNode? Expression = null;
    }
    public class ComputeGraphBlock : ComputeGraphNode
    {
        public ComputeGraphBlock(FuncInfo? ownFunc) : base(ownFunc, string.Empty)
        {
        }
        public void AddChild(ComputeGraphNode node)
        {
            ChildNodes.Add(node);
        }
        protected override void VisitChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildPrevHelper(ChildNodes, ownFunc, visits, visitorCallback);
        }
        protected override void VisitChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildNextHelper(ChildNodes, ownFunc, visits, visitorCallback);
        }
        protected override void VisitChildAllPrev(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildAllPrevHelper(ChildNodes, visits, visitorCallback);
        }
        protected override void VisitChildAllNext(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildAllNextHelper(ChildNodes, visits, visitorCallback);
        }
        protected override void PrintChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            if (ChildNodes.Count > 0) {
                foreach (var node in ChildNodes) {
                    node.PrintInfo(indent + 1);
                    if (!visits.Contains(node)) {
                        if (ownFunc == node.OwnFunc) {
                            node.PrintPrev(ownFunc, visits, indent + 1);
                        }
                    }
                }
            }
        }
        protected override void PrintChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            if (ChildNodes.Count > 0) {
                foreach (var node in ChildNodes) {
                    node.PrintInfo(indent + 1);
                    if (!visits.Contains(node)) {
                        if (ownFunc == node.OwnFunc) {
                            node.PrintNext(ownFunc, visits, indent + 1);
                        }
                    }
                }
            }
        }
        protected override void TryCalcValue(HashSet<ComputeGraphNode> visits, ref ControlInfo cinfo)
        {
            foreach (var node in ChildNodes) {
                node.CalcValue(visits, ref cinfo);
                if (cinfo.NeedBreakBlock()) {
                    cinfo.ExistsContinue = false;
                    break;
                }
            }
        }

        public List<ComputeGraphNode> ChildNodes = new List<ComputeGraphNode>();
    }
    public class ComputeGraphIfStatement : ComputeGraphNode
    {
        public ComputeGraphIfStatement(FuncInfo? ownFunc) : base(ownFunc, string.Empty)
        {
        }
        public void AddCondition(ComputeGraphNode node)
        {
            Conditions.Add(node);
        }
        public void AddBlock(ComputeGraphBlock block)
        {
            Blocks.Add(block);
        }
        protected override void VisitChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildPrevHelper(Conditions, ownFunc, visits, visitorCallback);
            VisitChildPrevHelper(Blocks, ownFunc, visits, visitorCallback);
        }
        protected override void VisitChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildNextHelper(Conditions, ownFunc, visits, visitorCallback);
            VisitChildNextHelper(Blocks, ownFunc, visits, visitorCallback);
        }
        protected override void VisitChildAllPrev(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildAllPrevHelper(Conditions, visits, visitorCallback);
            VisitChildAllPrevHelper(Blocks, visits, visitorCallback);
        }
        protected override void VisitChildAllNext(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildAllNextHelper(Conditions, visits, visitorCallback);
            VisitChildAllNextHelper(Blocks, visits, visitorCallback);
        }
        protected override void PrintChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            for (int ix = 0; ix < Conditions.Count || ix < Blocks.Count; ++ix) {
                if (ix < Conditions.Count) {
                    var node = Conditions[ix];
                    node.PrintInfo(indent + 1);
                    if (!visits.Contains(node)) {
                        if (ownFunc == node.OwnFunc) {
                            node.PrintPrev(ownFunc, visits, indent + 1);
                        }
                    }
                }
                if (ix < Blocks.Count) {
                    var node = Blocks[ix];
                    node.PrintInfo(indent + 1);
                    if (!visits.Contains(node)) {
                        if (ownFunc == node.OwnFunc) {
                            node.PrintPrev(ownFunc, visits, indent + 1);
                        }
                    }
                }
            }
        }
        protected override void PrintChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            for (int ix = 0; ix < Conditions.Count || ix < Blocks.Count; ++ix) {
                if (ix < Conditions.Count) {
                    var node = Conditions[ix];
                    node.PrintInfo(indent + 1);
                    if (!visits.Contains(node)) {
                        if (ownFunc == node.OwnFunc) {
                            node.PrintNext(ownFunc, visits, indent + 1);
                        }
                    }
                }
                if (ix < Blocks.Count) {
                    var node = Blocks[ix];
                    node.PrintInfo(indent + 1);
                    if (!visits.Contains(node)) {
                        if (ownFunc == node.OwnFunc) {
                            node.PrintNext(ownFunc, visits, indent + 1);
                        }
                    }
                }
            }
        }
        protected override void TryCalcValue(HashSet<ComputeGraphNode> visits, ref ControlInfo cinfo)
        {
            for (int ix = 0; ix < Conditions.Count || ix < Blocks.Count; ++ix) {
                if (ix < Conditions.Count && ix < Blocks.Count) {
                    var cond = Conditions[ix].CalcValue(visits, ref cinfo);
                    if(Calculator.TryParseBool(cond, out var val) && val) {
                        Blocks[ix].CalcValue(visits, ref cinfo);
                        break;
                    }
                }
                else if (ix < Blocks.Count) {
                    Blocks[ix].CalcValue(visits, ref cinfo);
                    break;
                }
            }
        }

        public List<ComputeGraphNode> Conditions = new List<ComputeGraphNode>();
        public List<ComputeGraphBlock> Blocks = new List<ComputeGraphBlock>();
    }
    public class ComputeGraphForStatement : ComputeGraphNode
    {
        public ComputeGraphForStatement(FuncInfo? ownFunc) : base(ownFunc, string.Empty)
        {
        }
        protected override void VisitChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildPrevHelper(ForFunc, ownFunc, visits, visitorCallback);
            VisitChildPrevHelper(Block, ownFunc, visits, visitorCallback);
        }
        protected override void VisitChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildNextHelper(ForFunc, ownFunc, visits, visitorCallback);
            VisitChildNextHelper(Block, ownFunc, visits, visitorCallback);
        }
        protected override void VisitChildAllPrev(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildAllPrevHelper(ForFunc, visits, visitorCallback);
            VisitChildAllPrevHelper(Block, visits, visitorCallback);
        }
        protected override void VisitChildAllNext(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildAllNextHelper(ForFunc, visits, visitorCallback);
            VisitChildAllNextHelper(Block, visits, visitorCallback);
        }
        protected override void PrintChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            PrintIndent(indent);
            if (null != ForFunc) {
                var node = ForFunc;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintPrev(ownFunc, visits, indent + 1);
                    }
                }
            }
            if (null != Block) {
                var node = Block;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintPrev(ownFunc, visits, indent + 1);
                    }
                }
            }
        }
        protected override void PrintChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            if (null != ForFunc) {
                var node = ForFunc;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintNext(ownFunc, visits, indent + 1);
                    }
                }
            }
            if (null != Block) {
                var node = Block;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintNext(ownFunc, visits, indent + 1);
                    }
                }
            }
        }
        protected override void TryCalcValue(HashSet<ComputeGraphNode> visits, ref ControlInfo cinfo)
        {
            var initVisits = new HashSet<ComputeGraphNode>(visits);
            int condNodeIndex = 0;
            ComputeGraphNode? condNode = null;
            if (null != ForFunc) {
                foreach(var prev in ForFunc.PrevNodes) {
                    if (prev.Type == "bool") {
                        condNode = prev;
                        break;
                    }
                    else {
                        prev.CalcValue(visits, ref cinfo);
                    }
                    ++condNodeIndex;
                }

            }
            for (int ct = 0; ct < Config.ActiveConfig.SettingInfo.MaxLoop; ++ct) {
                visits.Clear();
                foreach (var v in initVisits) {
                    visits.Add(v);
                }
                if (null != condNode) {
                    var cond = condNode.CalcValue(visits, ref cinfo);
                    if (!Calculator.TryParseBool(cond, out var val) || !val) {
                        break;
                    }
                }
                if (null != Block) {
                    Block.CalcValue(visits, ref cinfo);
                    if (cinfo.NeedBreakLoop()) {
                        cinfo.ExistsBreak = false;
                        break;
                    }
                }
                if (null != ForFunc) {
                    for (int ix = condNodeIndex + 1; ix < ForFunc.PrevNodes.Count; ++ix) {
                        var prev = ForFunc.PrevNodes[ix];
                        prev.CalcValue(visits, ref cinfo);
                    }
                }
            }
        }

        public ComputeGraphNode? ForFunc = null;
        public ComputeGraphBlock? Block = null;
    }
    public class ComputeGraphWhileStatement : ComputeGraphNode
    {
        public ComputeGraphWhileStatement(FuncInfo? ownFunc) : base(ownFunc, string.Empty)
        {
        }
        protected override void VisitChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildPrevHelper(WhileFunc, ownFunc, visits, visitorCallback);
            VisitChildPrevHelper(Block, ownFunc, visits, visitorCallback);
        }
        protected override void VisitChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildNextHelper(WhileFunc, ownFunc, visits, visitorCallback);
            VisitChildNextHelper(Block, ownFunc, visits, visitorCallback);
        }
        protected override void VisitChildAllPrev(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildAllPrevHelper(WhileFunc, visits, visitorCallback);
            VisitChildAllPrevHelper(Block, visits, visitorCallback);
        }
        protected override void VisitChildAllNext(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildAllNextHelper(WhileFunc, visits, visitorCallback);
            VisitChildAllNextHelper(Block, visits, visitorCallback);
        }
        protected override void PrintChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            if (null != WhileFunc) {
                var node = WhileFunc;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintPrev(ownFunc, visits, indent + 1);
                    }
                }
            }
            if (null != Block) {
                var node = Block;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintPrev(ownFunc, visits, indent + 1);
                    }
                }
            }
        }
        protected override void PrintChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            if (null != WhileFunc) {
                var node = WhileFunc;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintNext(ownFunc, visits, indent + 1);
                    }
                }
            }
            if (null != Block) {
                var node = Block;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintNext(ownFunc, visits, indent + 1);
                    }
                }
            }
        }
        protected override void TryCalcValue(HashSet<ComputeGraphNode> visits, ref ControlInfo cinfo)
        {
            var initVisits = new HashSet<ComputeGraphNode>(visits);
            for (int ct = 0; ct < Config.ActiveConfig.SettingInfo.MaxLoop; ++ct) {
                visits.Clear();
                foreach (var v in initVisits) {
                    visits.Add(v);
                }
                if (null != WhileFunc) {
                    var cond = WhileFunc.CalcValue(visits, ref cinfo);
                    if (!Calculator.TryParseBool(cond, out var val) || !val) {
                        break;
                    }
                }
                if (null != Block) {
                    Block.CalcValue(visits, ref cinfo);
                    if (cinfo.NeedBreakLoop()) {
                        cinfo.ExistsBreak = false;
                        break;
                    }
                }
            }
        }

        public ComputeGraphNode? WhileFunc = null;
        public ComputeGraphBlock? Block = null;
    }
    public class ComputeGraphDoWhileStatement : ComputeGraphNode
    {
        public ComputeGraphDoWhileStatement(FuncInfo? ownFunc) : base(ownFunc, string.Empty)
        {
        }
        protected override void VisitChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildPrevHelper(Block, ownFunc, visits, visitorCallback);
            VisitChildPrevHelper(WhileFunc, ownFunc, visits, visitorCallback);
        }
        protected override void VisitChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildNextHelper(Block, ownFunc, visits, visitorCallback);
            VisitChildNextHelper(WhileFunc, ownFunc, visits, visitorCallback);
        }
        protected override void VisitChildAllPrev(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildAllPrevHelper(Block, visits, visitorCallback);
            VisitChildAllPrevHelper(WhileFunc, visits, visitorCallback);
        }
        protected override void VisitChildAllNext(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildAllNextHelper(Block, visits, visitorCallback);
            VisitChildAllNextHelper(WhileFunc, visits, visitorCallback);
        }
        protected override void PrintChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            if (null != Block) {
                var node = Block;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintPrev(ownFunc, visits, indent + 1);
                    }
                }
            }
            if (null != WhileFunc) {
                var node = WhileFunc;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintPrev(ownFunc, visits, indent + 1);
                    }
                }
            }
        }
        protected override void PrintChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            if (null != Block) {
                var node = Block;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintNext(ownFunc, visits, indent + 1);
                    }
                }
            }
            if (null != WhileFunc) {
                var node = WhileFunc;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintNext(ownFunc, visits, indent + 1);
                    }
                }
            }
        }
        protected override void TryCalcValue(HashSet<ComputeGraphNode> visits, ref ControlInfo cinfo)
        {
            var initVisits = new HashSet<ComputeGraphNode>(visits);
            for (int ct = 0; ct < Config.ActiveConfig.SettingInfo.MaxLoop; ++ct) {
                visits.Clear();
                foreach (var v in initVisits) {
                    visits.Add(v);
                }
                if (null != Block) {
                    Block.CalcValue(visits, ref cinfo);
                    if (cinfo.NeedBreakLoop()) {
                        cinfo.ExistsBreak = false;
                        break;
                    }
                }
                if (null != WhileFunc) {
                    var cond = WhileFunc.CalcValue(visits, ref cinfo);
                    if (!Calculator.TryParseBool(cond, out var val) || !val) {
                        break;
                    }
                }
            }
        }

        public ComputeGraphBlock? Block = null;
        public ComputeGraphNode? WhileFunc = null;
    }
    public class ComputeGraphFunction : ComputeGraphNode
    {
        public ComputeGraphFunction(FuncInfo? ownFunc) : base(ownFunc, string.Empty)
        {
        }
        protected override void VisitChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildPrevHelper(Block, ownFunc, visits, visitorCallback);
        }
        protected override void VisitChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildNextHelper(Block, ownFunc, visits, visitorCallback);
        }
        protected override void VisitChildAllPrev(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildAllPrevHelper(Block, visits, visitorCallback);
        }
        protected override void VisitChildAllNext(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
        {
            VisitChildAllNextHelper(Block, visits, visitorCallback);
        }
        protected override void PrintChildPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            if (null != Block) {
                var node = Block;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintPrev(ownFunc, visits, indent + 1);
                    }
                }
            }
        }
        protected override void PrintChildNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            if (null != Block) {
                var node = Block;
                node.PrintInfo(indent + 1);
                if (!visits.Contains(node)) {
                    if (ownFunc == node.OwnFunc) {
                        node.PrintNext(ownFunc, visits, indent + 1);
                    }
                }
            }
        }
        protected override void PrintFieldInfo(int indent)
        {
            base.PrintFieldInfo(indent);

            if (null != OwnFunc) {
                PrintIndent(indent);
                Console.Write("[function:");
                Console.Write(OwnFunc.Name);
                Console.WriteLine("]");

                if (!OwnFunc.IsVoid()) {
                    Debug.Assert(null != OwnFunc.RetInfo);
                    PrintIndent(indent);
                    Console.Write("ret:");
                    Console.WriteLine(OwnFunc.RetInfo.Type);
                }
                foreach(var p in OwnFunc.Params) {
                    PrintIndent(indent);
                    Console.Write("param:");
                    Console.Write(p.Type);
                    Console.Write(" ");
                    Console.WriteLine(p.Name);
                }
            }
        }
        protected override void TryCalcValue(HashSet<ComputeGraphNode> visits, ref ControlInfo cinfo)
        {
            if (null != Block) {
                Block.CalcValue(visits, ref cinfo);
            }
        }

        public ComputeGraphBlock? Block = null;
    }
    public class ComputeGraph
    {
        public delegate bool VisitDelegation(ComputeGraphNode node);

        public List<ComputeGraphNode> RootNodes = new List<ComputeGraphNode>();
        public Dictionary<string, ComputeGraphVarNode> VarNodes = new Dictionary<string, ComputeGraphVarNode>();

        public void Reset(FuncInfo? ownFunc)
        {
            var nodes = new HashSet<ComputeGraphNode>();
            var prevVisits = new HashSet<ComputeGraphNode>();
            foreach (var node in RootNodes) {
                node.VisitPrev(ownFunc, prevVisits, node => {
                    if (ownFunc != node.OwnFunc) {
                        if (!nodes.Contains(node))
                            nodes.Add(node);
                    }
                    return true;
                });
            }
            var tnodes = new HashSet<ComputeGraphNode>();
            foreach (var node in nodes) {
                tnodes.Clear();
                foreach (var tnode in node.PrevNodes) {
                    if (ownFunc == tnode.OwnFunc) {
                        tnodes.Add(tnode);
                    }
                }
                foreach (var tnode in tnodes) {
                    node.PrevNodes.RemoveAll(n => n == tnode);
                }
                tnodes.Clear();
                foreach (var tnode in node.NextNodes) {
                    if (ownFunc == tnode.OwnFunc) {
                        tnodes.Add(tnode);
                    }
                }
                foreach (var tnode in tnodes) {
                    node.NextNodes.RemoveAll(n => n == tnode);
                }
                tnodes.Clear();
                foreach (var tnode in node.OutNodes) {
                    if (ownFunc == tnode.OwnFunc) {
                        tnodes.Add(tnode);
                    }
                }
                foreach (var tnode in tnodes) {
                    node.OutNodes.RemoveAll(n => n == tnode);
                }
            }
            RootNodes.Clear();
            VarNodes.Clear();
        }
        public void ResetValue(FuncInfo? ownFunc)
        {
            var nodes = new HashSet<ComputeGraphNode>();
            var prevVisits = new HashSet<ComputeGraphNode>();
            foreach (var node in RootNodes) {
                node.VisitPrev(ownFunc, prevVisits, node => {
                    if (ownFunc == node.OwnFunc) {
                        node.ResetValue();
                    }
                    return true;
                });
            }
            foreach(var pair in VarNodes) {
                var node = pair.Value;
                node.ResetValue();
            }
        }
        public void ResetExpression(FuncInfo? ownFunc)
        {
            var nodes = new HashSet<ComputeGraphNode>();
            var prevVisits = new HashSet<ComputeGraphNode>();
            foreach (var node in RootNodes) {
                node.VisitPrev(ownFunc, prevVisits, node => {
                    if (ownFunc == node.OwnFunc) {
                        node.ResetExpression();
                    }
                    return true;
                });
            }
            foreach (var pair in VarNodes) {
                var node = pair.Value;
                node.ResetExpression();
            }
        }

        public void ResetValueDependsVar(FuncInfo? ownFunc, string vname)
        {
            if (VarNodes.TryGetValue(vname, out var node)) {
                var visits = new HashSet<ComputeGraphNode>();
                node.VisitNext(ownFunc, visits, node => {
                    if (ownFunc == node.OwnFunc) {
                        node.ResetValue();
                    }
                    return true;
                });
            }
        }

        public void VisitPrev(FuncInfo? ownFunc, string vname, VisitDelegation visitorCallback)
        {
            if (VarNodes.TryGetValue(vname, out var node)) {
                var visits = new HashSet<ComputeGraphNode>();
                node.VisitPrev(ownFunc, visits, visitorCallback);
            }
        }
        public void VisitNext(FuncInfo? ownFunc, string vname, VisitDelegation visitorCallback)
        {
            if (VarNodes.TryGetValue(vname, out var node)) {
                var visits = new HashSet<ComputeGraphNode>();
                node.VisitNext(ownFunc, visits, visitorCallback);
            }
        }
        public void VisitPrev(FuncInfo? ownFunc, ComputeGraphNode node, VisitDelegation visitorCallback)
        {
            var visits = new HashSet<ComputeGraphNode>();
            node.VisitPrev(ownFunc, visits, visitorCallback);
        }
        public void VisitNext(FuncInfo? ownFunc, ComputeGraphNode node, VisitDelegation visitorCallback)
        {
            var visits = new HashSet<ComputeGraphNode>();
            node.VisitNext(ownFunc, visits, visitorCallback);
        }
        public void VisitAllPrev(string vname, VisitDelegation visitorCallback)
        {
            if (VarNodes.TryGetValue(vname, out var node)) {
                var visits = new HashSet<ComputeGraphNode>();
                node.VisitAllPrev(visits, visitorCallback);
            }
        }
        public void VisitAllNext(string vname, VisitDelegation visitorCallback)
        {
            if (VarNodes.TryGetValue(vname, out var node)) {
                var visits = new HashSet<ComputeGraphNode>();
                node.VisitAllNext(visits, visitorCallback);
            }
        }
        public void VisitAllPrev(ComputeGraphNode node, VisitDelegation visitorCallback)
        {
            var visits = new HashSet<ComputeGraphNode>();
            node.VisitAllPrev(visits, visitorCallback);
        }
        public void VisitAllNext(ComputeGraphNode node, VisitDelegation visitorCallback)
        {
            var visits = new HashSet<ComputeGraphNode>();
            node.VisitAllNext(visits, visitorCallback);
        }

        public void Print(FuncInfo? ownFunc)
        {
            var prevVisits = new HashSet<ComputeGraphNode>();
            var nextVisits = new HashSet<ComputeGraphNode>();
            foreach (var node in RootNodes) {
                node.Print(ownFunc, prevVisits, nextVisits, 1);
            }
        }

        public static void ResetStatic()
        {
            ComputeGraphNode.ResetStatic();
        }
    }
}
