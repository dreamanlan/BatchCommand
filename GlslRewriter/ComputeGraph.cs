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
using DslExpression;

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
            m_HaveValue = false;
            m_CachedValue.SetNullObject();
        }
        public void ResetExpression()
        {
            CachedExpression = string.Empty;
            ExpressionIndent = 0;
        }
        public DslExpression.BoxedValue GetValue()
        {
            HashSet<ComputeGraphNode> visits = new HashSet<ComputeGraphNode>(Config.ActiveConfig.SettingInfo.ComputeGraphNodesCapacity);
            var cinfo = new ControlInfo();
            var val = CalcValue(visits, ref cinfo);
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
                CachedExpression = sb.ToString();
                ExpressionIndent = 0;
                result = CachedExpression;
            }
            return result;
        }

        public void DoCalc()
        {
            HashSet<ComputeGraphNode> visits = new HashSet<ComputeGraphNode>(Config.ActiveConfig.SettingInfo.ComputeGraphNodesCapacity);
            var cinfo = new ControlInfo();
            CalcValue(visits, ref cinfo);
        }
        public DslExpression.BoxedValue CalcValue(HashSet<ComputeGraphNode> visits, ref ControlInfo cinfo)
        {
            if (!m_HaveValue) {
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
            return m_CachedValue;
        }
        public void GenerateExpression(StringBuilder sb, int indent, int curLevel, ref int mergeMaxLevel, out int curMaxLevel, in ComputeSetting setting, Dictionary<string, int> usedVars, HashSet<ComputeGraphNode> visits, ComputeGraphNode? fromNode)
        {
            curMaxLevel = curLevel;
            if (setting.DontCacheExpression || setting.VariableExpandedOnlyOnce || string.IsNullOrEmpty(CachedExpression)) {
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
                if (indent == ExpressionIndent) {
                    sb.Append(CachedExpression);
                }
                else {
                    var lines = CachedExpression.Split(s_LineSplits, StringSplitOptions.RemoveEmptyEntries);
                    if (indent > ExpressionIndent) {
                        foreach (var line in lines) {
                            sb.AppendLine();
                            sb.Append(Literal.GetIndentString(indent - ExpressionIndent));
                            sb.Append(line);
                        }
                    }
                    else {
                        foreach (var line in lines) {
                            sb.AppendLine();
                            sb.Append(line.Substring(ExpressionIndent - indent));
                        }
                    }
                }
            }
            else {
                sb.Append(CachedExpression);
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

        protected DslExpression.BoxedValue CachedValue
        {
            get { return m_CachedValue; }
            set {
                m_CachedValue = value;
                m_HaveValue = true;
            }
        }
        protected string CachedExpression { get; set; } = string.Empty;
        protected int ExpressionIndent { get; set; } = 0;

        private bool m_HaveValue = false;
        private DslExpression.BoxedValue m_CachedValue = DslExpression.BoxedValue.NullObject;

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
        public ComputeGraphConstNode(FuncInfo? ownFunc, string type, string srcString, DslExpression.BoxedValue val) : base(ownFunc, type)
        {
            SourceString = srcString;
            Value = val;
        }
        protected override void PrintFieldInfo(int indent)
        {
            base.PrintFieldInfo(indent);

            PrintIndent(indent);
            Console.Write("Value:");
            Console.WriteLine(Value.ToString());
        }
        protected override void TryCalcValue(HashSet<ComputeGraphNode> visits, ref ControlInfo cinfo)
        {
            CachedValue = Value;
        }
        protected override void TryGenerateExpression(StringBuilder sb, int indent, int curLevel, ref int curMaxLevel, in ComputeSetting setting, Dictionary<string, int> usedVars, HashSet<ComputeGraphNode> visits, ComputeGraphNode? fromNode)
        {
            sb.Append(SourceString);
        }

        public string SourceString = string.Empty;
        public DslExpression.BoxedValue Value = DslExpression.BoxedValue.NullObject;
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
                CachedValue = val;
            }
            else {
                if (PrevNodes.Count > 0) {
                    //Fetch the last assignment(multiple assignments only occur in branch scenarios with phi variable assignments),
                    //which facilitates correct calculation after commenting out non-executed if statements during code analysis.
                    CachedValue = PrevNodes[PrevNodes.Count - 1].CalcValue(visits, ref cinfo);
                }
                else {
                    CachedValue = DslExpression.BoxedValue.From(VarName);
                }
            }
        }
        protected override void TryGenerateExpression(StringBuilder sb, int indent, int curLevel, ref int curMaxLevel, in ComputeSetting setting, Dictionary<string, int> usedVars, HashSet<ComputeGraphNode> visits, ComputeGraphNode? fromNode)
        {
            bool withValue = false;
            if (setting.WithValue) {
                if (fromNode is not ComputeGraphCalcNode) {
                    //This situation should not occur for variable nodes.
                    withValue = true;
                }
                else if (fromNode is ComputeGraphCalcNode calcNode && calcNode.Operator != "[]" && calcNode.Operator != ".") {
                    //Only annotate the computed values on the leaf nodes of the computation graph for array or member access.
                    if (calcNode.Operator == "utof" || calcNode.Operator == "uintBitsToFloat" || calcNode.Operator == "ftou" || calcNode.Operator == "floatBitsToUint") {
                        //Annotate the value on the type conversion function call when using a uniform with type casting.
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
                        sb.Append(CachedValue.ToString());
                        sb.Append("}");
                    }
                }
                else {
                    if (setting.VariableExpandedOnlyOnce && usedVars.TryGetValue(VarName, out var varMaxLevel)) {
                        if (Config.ActiveConfig.SettingInfo.AutoSplitLevel >= 0 && Config.ActiveConfig.SettingInfo.AutoSplitLevelForRepeatedExpression <= varMaxLevel - curLevel) {
                            Config.ActiveConfig.SettingInfo.AutoSplitAddVariable(VarName, Type);
                        }
                        if (withValue) {
                            sb.Append("{");
                        }
                        sb.Append(VarName);
                        if (withValue) {
                            sb.Append(" : ");
                            sb.Append(CachedValue.ToString());
                            sb.Append("}");
                        }
                    }
                    else {
                        if (curLevel < setting.MaxLevel) {
                            //Fetch the last assignment (multiple assignments only occur in branch scenarios with phi variable assignments),
                            //which facilitates correct calculation after commenting out non-executed if statements during code analysis.
                            int start = sb.Length;
                            int tmpMaxLevel = curLevel;
                            //When the assignment expression is merged into the expression used by the variable, record the fromNode as the node of the expression used.
                            var from = fromNode;
                            if (null == from)
                                from = this;
                            //The variable assignment expression does not increase the level.
                            PrevNodes[PrevNodes.Count - 1].GenerateExpression(sb, indent, curLevel, ref tmpMaxLevel, out varMaxLevel, setting, usedVars, visits, from);
                            if (Config.ActiveConfig.SettingInfo.AutoSplitLevel >= 0) {
                                if (IsSplitOn(PrevNodes[PrevNodes.Count - 1], varMaxLevel - curLevel)) {
                                    Config.ActiveConfig.SettingInfo.AutoSplitAddVariable(VarName, Type);
                                    tmpMaxLevel = curLevel;
                                }
                                else if (Config.ActiveConfig.SettingInfo.AutoSplitLevel <= varMaxLevel - curLevel) {
                                    Config.ActiveConfig.SettingInfo.AutoSplitAddVariable(VarName, Type);
                                    tmpMaxLevel = curLevel;
                                }
                            }
                            usedVars[VarName] = tmpMaxLevel;
                            if (curMaxLevel < tmpMaxLevel)
                                curMaxLevel = tmpMaxLevel;
                            int end = sb.Length;
                            if (!setting.VariableExpandedOnlyOnce && !setting.DontCacheExpression) {
                                if (end > start) {
                                    CachedExpression = sb.ToString(start, end - start);
                                    ExpressionIndent = indent;
                                }
                            }
                        }
                        else {
                            //Here, there is no need to split the variable. It should be possible to eliminate it through other splitting methods with multiple iterations.
                            //Splitting here might introduce a relatively large number of simple expressions.
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
                    //Uninitialized local variables, which are generally phi variables, are added to the list of variables for the split expression to record variable usage.
                    Config.ActiveConfig.SettingInfo.AutoSplitAddVariable(VarName, Type);
                }
                sb.Append(VarName);
                if (withValue) {
                    sb.Append(" : ");
                    sb.Append(CachedValue.ToString());
                    sb.Append("}");
                }
            }
        }

        private bool IsSplitOn(ComputeGraphNode prevNode, int deltaLevel)
        {
            bool ret = false;
            if(prevNode is ComputeGraphCalcNode calcNode && calcNode.Operator=="=" && prevNode.PrevNodes[0] is ComputeGraphCalcNode calcNode2) {
                if (Config.ActiveConfig.SettingInfo.AutoSplitOnFuncs.TryGetValue(calcNode2.Operator, out var lvl) && lvl <= deltaLevel) {
                    ret = true;
                }
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
                var args = new List<DslExpression.BoxedValue>();
                foreach (var p in PrevNodes) {
                    args.Add(p.CalcValue(visits, ref cinfo));
                }
                if (Config.CalcFunc(Operator, args, GetResultType(), ArgTypeConversion, out var val)) {
                    CachedValue = val;
                }
            }
            else if (Operator == ".") {
                bool handled = false;
                if (PrevNodes[1] is ComputeGraphConstNode constNode) {
                    if (PrevNodes[0] is ComputeGraphVarNode varNode) {
                        //var.member
                        if(VariableTable.ObjectGetValue(varNode, constNode.Value, out var val)) {
                            CachedValue = val;
                            handled = true;
                        }
                    }
                    else if (PrevNodes[0] is ComputeGraphCalcNode calcNode) {
                        if (calcNode.Operator == "[]" && calcNode.PrevNodes[0] is ComputeGraphVarNode vnode2 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode2) {
                            //var[ix].member, Here, ix should not have a situation where it is converted from a floating-point number to an integer.
                            //If encountered, it should be because an integer parameter was configured as a floating-point number.
                            //Reconfigure and regenerate the code.
                            if (VariableTable.ObjectArrayGetValue(vnode2, cnode2.Value, constNode.Value, out var val)) {
                                CachedValue = val;
                                handled = true;
                            }
                        }
                    }
                }
                if (!handled) {
                    var objVal = PrevNodes[0].CalcValue(visits, ref cinfo);
                    var member = PrevNodes[1].CalcValue(visits, ref cinfo);
                    if (Calculator.CalcMember(objVal, member, GetResultType(), ArgTypeConversion, out var val, out var supported)) {
                        CachedValue = val;
                    }
                    else if (objVal.IsObject && !supported) {
                        Console.WriteLine("type {0}'s member '{1}' is unsupported, please support it.", !objVal.IsNullObject ? objVal.ObjectVal.GetType().FullName : "null", member);
                    }
                }
            }
            else if (Operator == "[]") {
                bool handled = false;
                if (PrevNodes[0] is ComputeGraphVarNode varNode) {
                    //var[ix], Here, ix should not have a situation where it is converted from a floating-point number to an integer.
                    //If encountered, it should be because an integer parameter was configured as a floating-point number.
                    //Reconfigure and regenerate the code.
                    if (PrevNodes[1] is ComputeGraphConstNode constNode) {
                        if (VariableTable.ArrayGetValue(varNode, constNode.Value, out var val)) {
                            CachedValue = val;
                            handled = true;
                        }
                    }
                    else {
                        var ix = PrevNodes[1].CalcValue(visits, ref cinfo);
                        if (VariableTable.ArrayGetValue(varNode, ix, out var val)) {
                            CachedValue = val;
                            handled = true;
                        }
                    }
                }
                if (!handled) {
                    var objVal = PrevNodes[0].CalcValue(visits, ref cinfo);
                    var ix = PrevNodes[1].CalcValue(visits, ref cinfo);
                    if (Calculator.CalcMember(objVal, ix, GetResultType(), ArgTypeConversion, out var val, out var supported)) {
                        CachedValue = val;
                    }
                    else if (objVal.IsObject && !supported) {
                        Console.WriteLine("type {0}'s subscript is unsupported, please support it.", !objVal.IsNullObject ? objVal.ObjectVal.GetType().FullName : "null");
                    }
                }
            }
            else if (Operator == "=") {
                bool cachedValueAssigned = false;
                if (NextNodes[0] is ComputeGraphVarNode vnode) {
                    //var = exp
                    //Phi variables have multiple branch assignments, which are skipped during calculation on the computation graph.
                    if (!Program.IsPhiVar(vnode.VarName)) {
                        if (PrevNodes[0] is ComputeGraphVarNode vnode2) {
                            VariableTable.AssignValue(vnode, vnode2, ArgTypeConversion, 0);
                            if (Program.IsPhiVar(vnode2.VarName)) {
                                //Here, it is possible that the phi variable is being used for the first time, as the phi variable does not have a value assigned on the computation graph.
                                //Here, we provide an opportunity to cache the value once.
                                vnode2.CalcValue(visits, ref cinfo);
                            }
                        }
                        else {
                            VariableTable.AssignValue(vnode, PrevNodes[0].CalcValue(visits, ref cinfo), ArgTypeConversion, 0);
                        }
                        if(VariableTable.GetVarValue(vnode.VarName, vnode.Type, out var v)) {
                            CachedValue = v;
                            cachedValueAssigned = true;
                        }
                    }
                }
                else if (NextNodes[0] is ComputeGraphCalcNode calcNode) {
                    if (calcNode.Operator=="." && calcNode.PrevNodes[0] is ComputeGraphVarNode vnode2 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode2) {
                        //var.member = exp, The types of exp and var.member may not be consistent here, and they will be dealt with when encountered later.
                        //(This should be rare because the conversions are usually applied to input parameters, and there are generally no writing situations,
                        //while output parameters are usually floating-point numbers.)
                        VariableTable.ObjectAssignValue(vnode2, cnode2.Value, PrevNodes[0].CalcValue(visits, ref cinfo));
                    }
                    else if (calcNode.Operator == "[]" && calcNode.PrevNodes[0] is ComputeGraphVarNode vnode3 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode3) {
                        //var[ix] = exp, The types of exp and var.member may not be consistent here, and they will be dealt with when encountered later.
                        //(This should be rare because the conversions are usually applied to input parameters, and there are generally no writing situations,
                        //while output parameters are usually floating-point numbers.)
                        //In addition, there should not be a case where ix changes from a floating-point number to an integer. If this happens,
                        //it indicates that a parameter that was originally an integer has been set as a floating-point number.
                        //The parameter should be reconfigured and the code should be regenerated.
                        VariableTable.ArrayAssignValue(vnode3, cnode3.Value, PrevNodes[0].CalcValue(visits, ref cinfo));
                    }
                    else if (calcNode.Operator == "." && calcNode.PrevNodes[0] is ComputeGraphCalcNode calcNode2 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode4) {
                        if (calcNode2.Operator == "[]" && calcNode2.PrevNodes[0] is ComputeGraphVarNode vnode5 && calcNode2.PrevNodes[1] is ComputeGraphConstNode cnode5) {
                            //var[ix].member = exp, The types of exp and var[ix].member may not be consistent here, and they will be dealt with when encountered later.
                            //(This should be rare because the conversions are usually applied to input parameters, and there are generally no writing situations,
                            //while output parameters are usually floating-point numbers.)
                            //In addition, there should not be a case where ix changes from a floating-point number to an integer. If this happens,
                            //it indicates that a parameter that was originally an integer has been set as a floating-point number.
                            //The parameter should be reconfigured and the code should be regenerated.
                            //
                            //Currently, we have not encountered a situation where the execution reaches this point.
                            VariableTable.ObjectArrayAssignValue(vnode5, cnode5.Value, cnode4.Value, PrevNodes[0].CalcValue(visits, ref cinfo));
                        }
                    }
                }
                if(!cachedValueAssigned) {
                    CachedValue = PrevNodes[0].CalcValue(visits, ref cinfo);
                }
            }
            else if (PrevNodes.Count == 3) {
                var cond = PrevNodes[0].CalcValue(visits, ref cinfo);
                var opd1 = PrevNodes[1].CalcValue(visits, ref cinfo);
                var opd2 = PrevNodes[2].CalcValue(visits, ref cinfo);
                if (Calculator.CalcCondExp(cond, opd1, opd2, GetResultType(), ArgTypeConversion, out var val, out var supported)) {
                    CachedValue = val;
                }
                else if (!supported) {
                    Console.WriteLine("condition expression is unsupported, please support it.");
                }
            }
            else if (PrevNodes.Count == 2) {
                var opd1 = PrevNodes[0].CalcValue(visits, ref cinfo);
                var opd2 = PrevNodes[1].CalcValue(visits, ref cinfo);
                if (Calculator.CalcBinary(Operator, opd1, opd2, GetResultType(), ArgTypeConversion, out var val, out var supported)) {
                    CachedValue = val;
                }
                else if (!supported) {
                    Console.WriteLine("binary operator '{0}' is unsupported, please support it.", Operator);
                }
            }
            else {
                var opd = PrevNodes[0].CalcValue(visits, ref cinfo);
                if (Calculator.CalcUnary(Operator, opd, GetResultType(), ArgTypeConversion, out var val, out var supported)) {
                    CachedValue = val;
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
                bool handled = true;
                if (Operator == "fma") {
                    //fma(a,b,c) => a*b+c
                    //We do not consider operator precedence when outputting, and add parentheses to each operation.
                    sb.Append("((");
                    PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(" * ");
                    PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(") + ");
                    PrevNodes[2].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(")");
                }
                else if (Operator == "min" && PrevNodes[0] is ComputeGraphCalcNode cnode && cnode.Operator == "max") {
                    //opengl:clamp returns the value of x constrained to the range minVal to maxVal. The returned value is computed as min(max(x, minVal), maxVal).
                    //min(max(v,a),b) => clamp(v,a,b)
                    int v1 = CalcVarLike(cnode.PrevNodes[0]);
                    int v2 = CalcVarLike(cnode.PrevNodes[1]);
                    int ix0 = v1 >= v2 ? 0 : 1;
                    int ix1 = 1 - ix0;
                    sb.Append("clamp");
                    sb.Append("(");
                    cnode.PrevNodes[ix0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode);
                    sb.Append(", ");
                    cnode.PrevNodes[ix1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode);
                    sb.Append(", ");
                    PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(")");
                }
                else if (Operator == "min" && PrevNodes[0] is ComputeGraphVarNode vnode && vnode.PrevNodes.Count > 0 && vnode.PrevNodes[vnode.PrevNodes.Count - 1] is ComputeGraphCalcNode cnode3 && cnode3.Operator=="="
                    && cnode3.PrevNodes[0] is ComputeGraphCalcNode cnode31 && cnode31.Operator == "max") {
                    //opengl:clamp returns the value of x constrained to the range minVal to maxVal. The returned value is computed as min(max(x, minVal), maxVal).
                    //min({vname = max(v,a)},b) => clamp(v,a,b)
                    int v1 = CalcVarLike(cnode31.PrevNodes[0]);
                    int v2 = CalcVarLike(cnode31.PrevNodes[1]);
                    int ix0 = v1 >= v2 ? 0 : 1;
                    int ix1 = 1 - ix0;
                    sb.Append("clamp");
                    sb.Append("(");
                    cnode31.PrevNodes[ix0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode31);
                    sb.Append(", ");
                    cnode31.PrevNodes[ix1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode31);
                    sb.Append(", ");
                    PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(")");
                }
                else if (Operator == "min" && PrevNodes[1] is ComputeGraphCalcNode cnode5 && cnode5.Operator == "max") {
                    //opengl:clamp returns the value of x constrained to the range minVal to maxVal. The returned value is computed as min(max(x, minVal), maxVal).
                    //And, the parameters of "min" are commutative, so the following equation holds as well.
                    //min(b,max(v,a)) => clamp(v,a,b)
                    int v1 = CalcVarLike(cnode5.PrevNodes[0]);
                    int v2 = CalcVarLike(cnode5.PrevNodes[1]);
                    int ix0 = v1 >= v2 ? 0 : 1;
                    int ix1 = 1 - ix0;
                    sb.Append("clamp");
                    sb.Append("(");
                    cnode5.PrevNodes[ix0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode5);
                    sb.Append(", ");
                    cnode5.PrevNodes[ix1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode5);
                    sb.Append(", ");
                    PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(")");
                }
                else if (Operator == "min" && PrevNodes[1] is ComputeGraphVarNode vnode3 && vnode3.PrevNodes.Count > 0 && vnode3.PrevNodes[vnode3.PrevNodes.Count - 1] is ComputeGraphCalcNode cnode7 && cnode7.Operator == "="
                    && cnode7.PrevNodes[0] is ComputeGraphCalcNode cnode71 && cnode71.Operator == "max") {
                    //opengl:clamp returns the value of x constrained to the range minVal to maxVal. The returned value is computed as min(max(x, minVal), maxVal).
                    //And, the parameters of "min" are commutative, so the following equation holds as well.
                    //min(b,{vname = max(v,a)}) => clamp(v,a,b)
                    int v1 = CalcVarLike(cnode71.PrevNodes[0]);
                    int v2 = CalcVarLike(cnode71.PrevNodes[1]);
                    int ix0 = v1 >= v2 ? 0 : 1;
                    int ix1 = 1 - ix0;
                    sb.Append("clamp");
                    sb.Append("(");
                    cnode71.PrevNodes[ix0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode71);
                    sb.Append(", ");
                    cnode71.PrevNodes[ix1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode71);
                    sb.Append(", ");
                    PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(")");
                }
                else if (Operator == "max" && PrevNodes[0] is ComputeGraphCalcNode cnode2 && cnode2.Operator == "min") {
                    //The following equation only holds when a<=b.
                    //max(min(v,b),a) => clamp(v,a,b)
                    bool r1 = TryGetConstNumeric(cnode2.PrevNodes[0], out var v1);
                    bool r2 = TryGetConstNumeric(cnode2.PrevNodes[1], out var v2);
                    bool r3 = TryGetConstNumeric(PrevNodes[1], out var v3);
                    bool firstIsVar = r2 && r3 && v3 <= v2;
                    bool secondIsVar = r1 && r3 && v3 <= v1;
                    if (firstIsVar || secondIsVar) {
                        int ix0 = firstIsVar ? 0 : 1;
                        int ix1 = 1 - ix0;
                        sb.Append("clamp");
                        sb.Append("(");
                        cnode2.PrevNodes[ix0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode2);
                        sb.Append(", ");
                        PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                        sb.Append(", ");
                        cnode2.PrevNodes[ix1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode2);
                        sb.Append(")");
                    }
                    else {
                        handled = false;
                    }
                }
                else if (Operator == "max" && PrevNodes[0] is ComputeGraphVarNode vnode2 && vnode2.PrevNodes.Count > 0 && vnode2.PrevNodes[vnode2.PrevNodes.Count - 1] is ComputeGraphCalcNode cnode4 && cnode4.Operator == "="
                    && cnode4.PrevNodes[0] is ComputeGraphCalcNode cnode41 && cnode41.Operator == "min") {
                    //The following equation only holds when a<=b.
                    //max({vname = min(v,b)},a) => clamp(v,a,b)
                    bool r1 = TryGetConstNumeric(cnode41.PrevNodes[0], out var v1);
                    bool r2 = TryGetConstNumeric(cnode41.PrevNodes[1], out var v2);
                    bool r3 = TryGetConstNumeric(PrevNodes[1], out var v3);
                    bool firstIsVar = r2 && r3 && v3 <= v2;
                    bool secondIsVar = r1 && r3 && v3 <= v1;
                    if (firstIsVar || secondIsVar) {
                        int ix0 = firstIsVar ? 0 : 1;
                        int ix1 = 1 - ix0;
                        sb.Append("clamp");
                        sb.Append("(");
                        cnode41.PrevNodes[ix0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode41);
                        sb.Append(", ");
                        PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                        sb.Append(", ");
                        cnode41.PrevNodes[ix1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode41);
                        sb.Append(")");
                    }
                    else {
                        handled = false;
                    }
                }
                else if (Operator == "max" && PrevNodes[1] is ComputeGraphCalcNode cnode6 && cnode6.Operator == "min") {
                    //The following equation only holds when a<=b.
                    //max(a,min(v,b)) => clamp(v,a,b)
                    bool r1 = TryGetConstNumeric(PrevNodes[0], out var v1);
                    bool r2 = TryGetConstNumeric(cnode6.PrevNodes[0], out var v2);
                    bool r3 = TryGetConstNumeric(cnode6.PrevNodes[1], out var v3);
                    bool firstIsVar = r1 && r3 && v1 <= v3;
                    bool secondIsVar = r1 && r2 && v1 <= v2;
                    if (firstIsVar || secondIsVar) {
                        int ix0 = firstIsVar ? 0 : 1;
                        int ix1 = 1 - ix0;
                        sb.Append("clamp");
                        sb.Append("(");
                        cnode6.PrevNodes[ix0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode6);
                        sb.Append(", ");
                        PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                        sb.Append(", ");
                        cnode6.PrevNodes[ix1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode6);
                        sb.Append(")");
                    }
                    else {
                        handled = false;
                    }
                }
                else if (Operator == "max" && PrevNodes[1] is ComputeGraphVarNode vnode4 && vnode4.PrevNodes.Count > 0 && vnode4.PrevNodes[vnode4.PrevNodes.Count - 1] is ComputeGraphCalcNode cnode8 && cnode8.Operator == "="
                    && cnode8.PrevNodes[0] is ComputeGraphCalcNode cnode81 && cnode81.Operator == "min") {
                    //The following equation only holds when a<=b.
                    //max(a,{vname = min(v,b)}) => clamp(v,a,b)
                    bool r1 = TryGetConstNumeric(PrevNodes[0], out var v1);
                    bool r2 = TryGetConstNumeric(cnode81.PrevNodes[0], out var v2);
                    bool r3 = TryGetConstNumeric(cnode81.PrevNodes[1], out var v3);
                    bool firstIsVar = r1 && r3 && v1 <= v3;
                    bool secondIsVar = r1 && r2 && v1 <= v2;
                    if (firstIsVar || secondIsVar) {
                        int ix0 = firstIsVar ? 0 : 1;
                        int ix1 = 1 - ix0;
                        sb.Append("clamp");
                        sb.Append("(");
                        cnode81.PrevNodes[ix0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode81);
                        sb.Append(", ");
                        PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                        sb.Append(", ");
                        cnode81.PrevNodes[ix1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, cnode81);
                        sb.Append(")");
                    }
                    else {
                        handled = false;
                    }
                }
                else if(Operator == "for") {
                    sb.Append("for");
                    sb.Append("(");
                    int i = 0;
                    for (; i < PrevNodes.Count; ++i) {
                        var p = PrevNodes[i];
                        switch (i) {
                            case 0: {
                                    var varnode = p as ComputeGraphVarNode;
                                    if (null != varnode) {
                                        Config.ActiveConfig.SettingInfo.AutoSplitAddVariable(varnode.VarName, varnode.Type);
                                        sb.Append(GetIndentString(indent));
                                        sb.Append(varnode.VarName);
                                        sb.Append(" = ");
                                        p.PrevNodes[0].GenerateExpression(sb, 0, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                                    }
                                    else if (p is not ComputeGraphConstNode) {
                                        // Only the case where the first expression of the "for" statement is a single assignment or empty is supported.
                                        Debug.Assert(false);
                                    }
                                }
                                break;
                            case 1: {
                                    sb.Append("; ");
                                    p.GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                                }
                                break;
                            case 2: {
                                    if (p is ComputeGraphConstNode) {
                                        //empty statement
                                    }
                                    else if (p.OutNodes.Count == 1) {
                                        var varnode = p.OutNodes[0] as ComputeGraphVarNode;
                                        Debug.Assert(null != varnode);
                                        sb.Append("; ");
                                        sb.Append(GetIndentString(indent));
                                        sb.Append(varnode.VarName);
                                        sb.Append(" = ");
                                        p.GenerateExpression(sb, 0, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                                    }
                                    else {
                                        //Only the case where the third expression of the "for" statement is a single assignment or empty is supported.
                                        Debug.Assert(false);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    sb.Append(")");
                }
                else {
                    handled = false;
                }
                if(!handled) {
                    bool withValue = false;
                    if(setting.WithValue && (Operator == "utof" || Operator== "uintBitsToFloat"
                        || Operator == "ftou" || Operator == "floatBitsToUint")){
                        //The "uniform" that uses type conversion should mark the calculated value on the conversion function call.
                        withValue = true;
                        sb.Append("{");
                    }
                    if (!TryGenerateWithFunctionReplacement(false, Operator, sb, indent, curLevel, ref curMaxLevel, setting, usedVars, visits, this)) {
                        if (ArgTypeConversion.TryGetValue(0, out var action)) {
                            if (action == c_action_remove_func) {
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
                            }
                        }
                        else {
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
                        }
                    }
                    if (withValue) {
                        sb.Append(" : ");
                        sb.Append(CachedValue.ToString());
                        sb.Append("}");
                    }
                }
            }
            else if (Operator == ".") {
                bool withValue = false;
                if (setting.WithValue) {
                    if (fromNode is not ComputeGraphCalcNode) {
                        // If the parent node is not a calculation node (constant or variable), then the current member access node is already a member access leaf node,
                        // and the calculated value should be marked on this node.
                        withValue = true;
                        sb.Append("{");
                    }
                    else if(fromNode is ComputeGraphCalcNode calcNode && calcNode.Operator != "[]" && calcNode.Operator != ".") {
                        //Only annotate the leaf nodes of the calculation nodes for array or member access with the calculated value.
                        if (calcNode.Operator == "utof" || calcNode.Operator == "uintBitsToFloat" ||
                            calcNode.Operator == "ftou" || calcNode.Operator == "floatBitsToUint") {
                            //Annotate the values on the type conversion function call when using uniforms with type conversion.
                        }
                        else {
                            withValue = true;
                            sb.Append("{");
                        }
                    }
                }
                bool handled = false;
                if (PrevNodes[0] is not ComputeGraphCalcNode) {
                    handled = TryGenerateWithFunctionReplacement(false, ".", sb, indent, curLevel, ref curMaxLevel, setting, usedVars, visits, this);
                }
                else if (PrevNodes[0] is ComputeGraphCalcNode calcNode && calcNode.Operator == "[]") {
                    handled = TryGenerateWithFunctionReplacement(false, "[].", sb, indent, curLevel, ref curMaxLevel, setting, usedVars, visits, this);
                }
                if (!handled) {
                    PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(".");
                    PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                }
                if (withValue) {
                    sb.Append(" : ");
                    sb.Append(CachedValue.ToString());
                    sb.Append("}");
                }
            }
            else if (Operator == "[]") {
                bool withValue = false;
                if (setting.WithValue) {
                    if (fromNode is not ComputeGraphCalcNode) {
                        //If the parent node is not a computation node (constant or variable), then the current member access node is already an array element access leaf node,
                        //and the calculated value should be annotated on this node.
                        withValue = true;
                        sb.Append("{");
                    }
                    else if (fromNode is ComputeGraphCalcNode calcNode && calcNode.Operator != "[]" && calcNode.Operator != ".") {
                        //Only annotate the calculated value on the leaf nodes of the computation nodes for array or member access.
                        if (calcNode.Operator == "utof" || calcNode.Operator == "uintBitsToFloat" ||
                            calcNode.Operator == "ftou" || calcNode.Operator == "floatBitsToUint") {
                            //Annotate the values on the type conversion function call when using uniforms with type conversion.
                        }
                        else {
                            withValue = true;
                            sb.Append("{");
                        }
                    }
                }
                bool handled = false;
                if (PrevNodes[0] is not ComputeGraphCalcNode) {
                    handled = TryGenerateWithFunctionReplacement(false, "[]", sb, indent, curLevel, ref curMaxLevel, setting, usedVars, visits, this);
                }
                if (!handled) {
                    PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append("[");
                    PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append("]");
                }
                if (withValue) {
                    sb.Append(" : ");
                    sb.Append(CachedValue.ToString());
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
                //When assigning an expression to a variable, record the fromNode as the expression node used.
                var from = fromNode;
                if (setting.UseMultilineComments || null == fromNode)
                    from = this;
                int action = GenConversionBefore(sb, 0);
                //Do not increase the level when assigning a statement.
                PrevNodes[0].GenerateExpression(sb, indent + 1, curLevel, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, from);
                GenConversionAfter(sb, action);
                if (setting.UseMultilineComments) {
                    sb.AppendLine();
                    sb.Append(Literal.GetIndentString(indent));
                    sb.Append("}");
                }
            }
            else if (PrevNodes.Count == 3) {
                if (!TryGenerateWithFunctionReplacement(false, "?:", sb, indent, curLevel, ref curMaxLevel, setting, usedVars, visits, this)) {
                    sb.Append("(");
                    PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(" ? ");
                    PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(" : ");
                    PrevNodes[2].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    sb.Append(")");
                }
            }
            else if (PrevNodes.Count == 2) {
                if (!TryGenerateWithFunctionReplacement(true, Operator, sb, indent, curLevel, ref curMaxLevel, setting, usedVars, visits, this)) {
                    sb.Append("(");
                    int action0 = GenConversionBefore(sb, 0);
                    PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    GenConversionAfter(sb, action0);
                    sb.Append(" ");
                    sb.Append(Operator);
                    sb.Append(" ");
                    int action1 = GenConversionBefore(sb, 1);
                    PrevNodes[1].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    GenConversionAfter(sb, action1);
                    sb.Append(")");
                }
            }
            else {
                if (!TryGenerateWithFunctionReplacement(true, Operator, sb, indent, curLevel, ref curMaxLevel, setting, usedVars, visits, this)) {
                    sb.Append("(");
                    sb.Append(Operator);
                    sb.Append(" ");
                    int action = GenConversionBefore(sb, 0);
                    PrevNodes[0].GenerateExpression(sb, indent, curLevel + 1, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                    GenConversionAfter(sb, action);
                    sb.Append(")");
                }
            }
        }

        private bool TryGenerateWithFunctionReplacement(bool useArgTypeConversion, string key, StringBuilder sb, int indent, int curLevel, ref int curMaxLevel, in ComputeSetting setting, Dictionary<string, int> usedVars, HashSet<ComputeGraphNode> visits, ComputeGraphNode? fromNode)
        {
            bool ret = false;
            if(Program.s_DoReplacement && Config.ActiveConfig.FunctionReplacements.TryGetValue(key, out var repList)) {
                foreach (var repInfo in repList) {
                    bool match = true;
                    for (int ix = 0; ix < repInfo.Args.Count; ++ix) {
                        string str = repInfo.Args[ix];
                        Debug.Assert(null != repInfo.ArgGetter);
                        int curLvl = curLevel;
                        var node = repInfo.ArgGetter(this, ix, ref curLvl);
                        if(node is ComputeGraphVarNode vnode) {
                            if (str != "*" && str != vnode.VarName && str != vnode.GetValue().ToString()) {
                                match = false;
                                break;
                            }
                        }
                        else if(node is ComputeGraphConstNode cnode) {
                            if (str != "*" && str != cnode.Value) {
                                match = false;
                                break;
                            }
                        }
                        else if(node is ComputeGraphCalcNode calcNode) {
                            if (str != "*" && str != calcNode.GetValue().ToString()) {
                                match = false;
                                break;
                            }
                        }
                    }
                    if (match) {
                        var dslFunc = repInfo.Replacement;
                        if (null != dslFunc) {
                            ret = true;

                            string dslFuncId = dslFunc.GetId();
                            string repTag = key + "=>" + dslFuncId;

                            if (dslFunc is Dsl.ValueData) {
                                //Since the visits record is used for duplicate checking, if we actually use the parameters, we cannot use this method of ignoring the results to calculate the depth of the expression.
                                //Therefore, this general method can only be used in the case of a single value. For expressions that do not use parameters, you can use the @join function and the @skip_and_lvlup function to handle them.
                                //This section uses the method of generating parameter expressions and discarding them to calculate maxLevel,
                                //in order to keep the depth of the expression consistent in the case of replacement and non - replacement(for maintaining consistency between GLSL and HLSL code).
                                for (int argIx = 0; argIx < PrevNodes.Count; ++argIx) {
                                    var node = PrevNodes[argIx];
                                    int curLvl;
                                    if (null != repInfo.ArgGetter) {
                                        //The level configuration here may be inconsistent with the original calculation.
                                        //When encountering inconsistencies in variable splitting between GLSL and HLSL,
                                        //pay particular attention to this part.
                                        curLvl = curLevel;
                                        var p = repInfo.ArgGetter(this, argIx, ref curLvl);
                                        s_IgnoredContent.Length = 0;
                                        int subMaxLevel;
                                        p.GenerateExpression(s_IgnoredContent, 0, curLvl, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                                    }
                                    else {
                                        curLvl = curLevel + 1;
                                        int subMaxLevel;
                                        node.GenerateExpression(s_IgnoredContent, 0, curLvl, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                                    }
                                }
                            }
                            TransformSyntax(useArgTypeConversion, repTag, repInfo, dslFunc, sb, curLevel, ref curMaxLevel, setting, usedVars, visits, fromNode);
                        }
                        break;
                    }
                }
            }
            return ret;
        }
        private void TransformSyntax(bool useArgTypeConversion, string repTag, Config.FunctionReplacement repInfo, Dsl.ISyntaxComponent syntax, StringBuilder sb, int curLevel, ref int curMaxLevel, in ComputeSetting setting, Dictionary<string, int> usedVars, HashSet<ComputeGraphNode> visits, ComputeGraphNode? fromNode)
        {
            var valData = syntax as Dsl.ValueData;
            if (null != valData) {
                if (valData.IsString()) {
                    sb.Append('"');
                    sb.Append(valData.GetId());
                    sb.Append('"');
                }
                else {
                    sb.Append(valData.GetId());
                }
            }
            else {
                var funcData = syntax as Dsl.FunctionData;
                if (null != funcData) {
                    string id = funcData.GetId();
                    if (id == "@arg_and_lvlup") {
                        string ixStr = funcData.GetParamId(0);
                        string lvlStr = funcData.GetParamNum() > 1 ? funcData.GetParamId(1) : "0";
                        if (int.TryParse(ixStr, out var argIx) && int.TryParse(lvlStr, out var lvl)) {
                            Debug.Assert(null != repInfo.ArgGetter);
                            int curLvl = curLevel;
                            var p = repInfo.ArgGetter(this, argIx, ref curLvl);
                            if (lvl != 0) {
                                curLvl = curLevel + lvl;
                            }
                            int action = GenConversionBefore(sb, argIx);
                            int subMaxLevel;
                            p.GenerateExpression(sb, 0, curLvl, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                            GenConversionAfter(sb, action);
                        }
                        else {
                            Console.WriteLine("function_replacement: {0}, @arg_and_lvlup's argument must be integer !", repTag);
                        }
                    }
                    else if (id == "@skip_and_lvlup") {
                        string ixStr = funcData.GetParamId(0);
                        string lvlStr = funcData.GetParamNum() > 1 ? funcData.GetParamId(1) : "0";
                        if (int.TryParse(ixStr, out var argIx) && int.TryParse(lvlStr, out var lvl)) {
                            Debug.Assert(null != repInfo.ArgGetter);
                            int curLvl = curLevel;
                            var p = repInfo.ArgGetter(this, argIx, ref curLvl);
                            if (lvl != 0) {
                                curLvl = curLevel + lvl;
                            }
                            s_IgnoredContent.Length = 0;
                            int subMaxLevel;
                            p.GenerateExpression(s_IgnoredContent, 0, curLvl, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                        }
                        else {
                            Console.WriteLine("function_replacement: {0}, @skip_and_lvlup's argument must be integer !", repTag);
                        }
                    }
                    else if (id == "@join") {
                        foreach (var p in funcData.Params) {
                            TransformSyntax(useArgTypeConversion, repTag, repInfo, p, sb, curLevel, ref curMaxLevel, setting, usedVars, visits, fromNode);
                        }
                    }
                    else if (id == "@arg") {
                        string ixStr = funcData.GetParamId(0);
                        if (int.TryParse(ixStr, out var argIx)) {
                            Debug.Assert(null != repInfo.ArgGetter);
                            int curLvl = curLevel;
                            var p = repInfo.ArgGetter(this, argIx, ref curLvl);
                            int action = GenConversionBefore(sb, argIx);
                            int subMaxLevel;
                            p.GenerateExpression(sb, 0, curLevel, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                            GenConversionAfter(sb, action);
                        }
                        else {
                            Console.WriteLine("function_replacement: {0}, @arg's argument must be integer !", repTag);
                        }
                    }
                    else if (id == "@skip") {
                        string ixStr = funcData.GetParamId(0);
                        if (int.TryParse(ixStr, out var argIx)) {
                            Debug.Assert(null != repInfo.ArgGetter);
                            int curLvl = curLevel;
                            var p = repInfo.ArgGetter(this, argIx, ref curLvl);
                            s_IgnoredContent.Length = 0;
                            int subMaxLevel;
                            p.GenerateExpression(s_IgnoredContent, 0, curLevel, ref curMaxLevel, out subMaxLevel, setting, usedVars, visits, this);
                        }
                        else {
                            Console.WriteLine("function_replacement: {0}, @skip's argument must be integer !", repTag);
                        }
                    }
                    else if (funcData.IsOperatorParamClass()) {
                        int num = funcData.GetParamNum();
                        if (num == 1) {
                            var p = funcData.GetParam(0);
                            sb.Append(id);
                            sb.Append(" ");
                            TransformSyntax(useArgTypeConversion, repTag, repInfo, p, sb, curLevel + 1, ref curMaxLevel, setting, usedVars, visits, fromNode);
                        }
                        else if (num == 2) {
                            var p1 = funcData.GetParam(0);
                            var p2 = funcData.GetParam(1);
                            TransformSyntax(useArgTypeConversion, repTag, repInfo, p1, sb, curLevel + 1, ref curMaxLevel, setting, usedVars, visits, fromNode);
                            sb.Append(" ");
                            sb.Append(id);
                            sb.Append(" ");
                            TransformSyntax(useArgTypeConversion, repTag, repInfo, p2, sb, curLevel + 1, ref curMaxLevel, setting, usedVars, visits, fromNode);
                        }
                        else {
                            Console.WriteLine("function_replacement: {0}, invalid argument num for operator {1} !", repTag, id);
                        }
                    }
                    else {
                        sb.Append(id);
                        switch (funcData.GetParamClassUnmasked()) {
                            case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PERIOD:
                                sb.Append(".");
                                break;
                            case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET:
                                sb.Append("[");
                                break;
                            case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PARENTHESIS:
                                sb.Append("(");
                                break;
                            default:
                                Console.WriteLine("function_replacement: {0}, invalid parenthesis !", repTag);
                                break;
                        }
                        string prestr = string.Empty;
                        foreach (var p in funcData.Params) {
                            sb.Append(prestr);
                            TransformSyntax(useArgTypeConversion, repTag, repInfo, p, sb, curLevel + 1, ref curMaxLevel, setting, usedVars, visits, fromNode);
                            if (string.IsNullOrEmpty(prestr))
                                prestr = ", ";
                        }
                        switch (funcData.GetParamClassUnmasked()) {
                            case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PERIOD:
                                break;
                            case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET:
                                sb.Append("]");
                                break;
                            case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PARENTHESIS:
                                sb.Append(")");
                                break;
                        }
                    }
                }
                else {
                    var stmData = syntax as Dsl.StatementData;
                    if (null != stmData) {
                        var f1 = stmData.First.AsFunction;
                        var f2 = stmData.Second.AsFunction;
                        if (null != f1 && null != f2 && f1.GetId() == "?" && f1.IsHighOrder && f1.IsTernaryOperatorParamClass()
                            && f1.LowerOrderFunction.GetParamNum() == 1 && f1.GetParamNum() == 1
                            && f2.GetId() == ":" && f2.IsTernaryOperatorParamClass() && f2.GetParamNum() == 1) {
                            var condExp = f1.LowerOrderFunction.GetParam(0);
                            var trueExp = f1.GetParam(0);
                            var falseExp = f2.GetParam(0);
                            TransformSyntax(useArgTypeConversion, repTag, repInfo, condExp, sb, curLevel + 1, ref curMaxLevel, setting, usedVars, visits, fromNode);
                            sb.Append(" ? ");
                            TransformSyntax(useArgTypeConversion, repTag, repInfo, trueExp, sb, curLevel + 1, ref curMaxLevel, setting, usedVars, visits, fromNode);
                            sb.Append(" : ");
                            TransformSyntax(useArgTypeConversion, repTag, repInfo, falseExp, sb, curLevel + 1, ref curMaxLevel, setting, usedVars, visits, fromNode);
                        }
                        else {
                            Console.WriteLine("function_replacement: {0}, invalid condition expression !", repTag);
                        }
                    }
                }
            }
        }
        private void AppendAssignLeft(StringBuilder sb, ComputeGraphNode leftNode)
        {
            if (leftNode is ComputeGraphVarNode vnode) {
                sb.Append(vnode.VarName);
            }
            else if (leftNode is ComputeGraphConstNode cnode) {
                sb.Append(cnode.Value.ToString());
            }
            else if (leftNode is ComputeGraphCalcNode calcNode) {
                if (calcNode.Operator == "." && calcNode.PrevNodes[0] is ComputeGraphVarNode vnode2 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode2) {
                    sb.Append(vnode2.VarName);
                    sb.Append(".");
                    sb.Append(cnode2.Value.ToString());
                }
                else if (calcNode.Operator == "[]" && calcNode.PrevNodes[0] is ComputeGraphVarNode vnode3 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode3) {
                    sb.Append(vnode3.VarName);
                    sb.Append("[");
                    sb.Append(cnode3.Value.ToString());
                    sb.Append("]");
                }
                else if (calcNode.Operator == "." && calcNode.PrevNodes[0] is ComputeGraphCalcNode calcNode2 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode4) {
                    if (calcNode2.Operator == "[]" && calcNode2.PrevNodes[0] is ComputeGraphVarNode vnode5 && calcNode2.PrevNodes[1] is ComputeGraphConstNode cnode5) {
                        sb.Append(vnode5.VarName);
                        sb.Append("[");
                        sb.Append(cnode5.Value.ToString());
                        sb.Append("].");
                        sb.Append(cnode4.Value.ToString());
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
        private int CalcVarLike(ComputeGraphNode node)
        {
            int val = 0;
            if(node is ComputeGraphVarNode) {
                if (node.PrevNodes.Count > 0)
                    val = CalcVarLike(node.PrevNodes[node.PrevNodes.Count - 1]);
                else
                    val = int.MaxValue;
            }
            else if(node is ComputeGraphCalcNode cnode) {
                if (cnode.Operator == ".") {
                    val = 10000;
                    if (cnode.PrevNodes[0] is ComputeGraphVarNode vnode) {
                        int v = CalcVarLike(vnode);
                        if (v == int.MaxValue)
                            val = v;
                    }
                }
                else if (cnode.Operator == "[]") {
                    val = 5000;
                    if (cnode.PrevNodes[0] is ComputeGraphVarNode vnode) {
                        int v = CalcVarLike(vnode);
                        if (v == int.MaxValue)
                            val = v;
                    }
                }
                else if (cnode.Operator == "=") {
                    val = CalcVarLike(node.PrevNodes[0]);
                }
                else {
                    val = int.MinValue;
                    foreach(var n in node.PrevNodes) {
                        int v = CalcVarLike(n);
                        if (val < v)
                            val = v;
                        if(val==int.MaxValue)
                            break;
                    }
                }
            }
            else if(node is ComputeGraphConstNode) {
                val = int.MinValue;
            }
            return val;
        }
        private bool TryGetConstNumeric(ComputeGraphNode node, out double val)
        {
            val = 0;
            bool ret = false;
            if (node is ComputeGraphVarNode) {
                if (node.PrevNodes.Count > 0)
                    ret = TryGetConstNumeric(node.PrevNodes[node.PrevNodes.Count - 1], out val);
            }
            else if (node is ComputeGraphCalcNode cnode) {
                if (cnode.Operator == "=") {
                    ret = TryGetConstNumeric(node.PrevNodes[0], out val);
                }
            }
            else if (node is ComputeGraphConstNode vnode) {
                if (vnode.Value.IsNumber || vnode.Value.IsInteger) {
                    val = vnode.Value.GetDouble();
                    ret = true;
                }
            }
            return ret;
        }
        private string GetResultType()
        {
            return GetResultType(this);
        }
        private int GenConversionBefore(StringBuilder sb, int argIndex)
        {
            int action;
            if (ArgTypeConversion.TryGetValue(0, out action)) {
                switch (action) {
                    case c_action_add_ftoi:
                        sb.Append("ftoi(");
                        break;
                    case c_action_add_ftou:
                        sb.Append("ftou(");
                        break;
                    case c_action_add_itof:
                        sb.Append("itof(");
                        break;
                    case c_action_add_utof:
                        sb.Append("utof(");
                        break;
                }
            }
            return action;
        }
        private void GenConversionAfter(StringBuilder sb, int action)
        {
            switch (action) {
                case c_action_add_ftoi:
                    sb.Append(")");
                    break;
                case c_action_add_ftou:
                    sb.Append(")");
                    break;
                case c_action_add_itof:
                    sb.Append(")");
                    break;
                case c_action_add_utof:
                    sb.Append(")");
                    break;
            }
        }

        public string Operator = string.Empty;

        private Dictionary<int, int> ArgTypeConversion = new Dictionary<int, int>();

        public static string GetResultType(ComputeGraphNode curNode)
        {
            foreach (var node in curNode.NextNodes) {
                if (node is ComputeGraphVarNode vnode) {
                    return vnode.Type;
                }
                else if (node is ComputeGraphCalcNode cnode && cnode.Operator == "=") {
                    return GetResultType(cnode);
                }
            }
            return string.Empty;
        }
        public static ComputeGraphNode GetArgForMember(ComputeGraphNode expNode, int ix, ref int curLevel)
        {
            switch (ix) {
                case 0:
                    curLevel = curLevel + 1;
                    return expNode.PrevNodes[0];
                case 1:
                    curLevel = curLevel + 1;
                    return expNode.PrevNodes[1];
                default:
                    Debug.Assert(false);
                    return expNode;
            }
        }
        public static ComputeGraphNode GetArgForArray(ComputeGraphNode expNode, int ix, ref int curLevel)
        {
            switch (ix) {
                case 0:
                    curLevel = curLevel + 1;
                    return expNode.PrevNodes[0];
                case 1:
                    curLevel = curLevel + 1;
                    return expNode.PrevNodes[1];
                default:
                    Debug.Assert(false);
                    return expNode;
            }
        }
        public static ComputeGraphNode GetArgForArrayMember(ComputeGraphNode expNode, int ix, ref int curLevel)
        {
            switch (ix) {
                case 0:
                    curLevel = curLevel + 2;
                    return expNode.PrevNodes[0].PrevNodes[0];
                case 1:
                    curLevel = curLevel + 2;
                    return expNode.PrevNodes[0].PrevNodes[1];
                case 2:
                    curLevel = curLevel + 1;
                    return expNode.PrevNodes[1];
                default:
                    Debug.Assert(false);
                    return expNode;
            }
        }
        public static ComputeGraphNode GetArgForFuncOrOper(ComputeGraphNode expNode, int ix, ref int curLevel)
        {
            if (ix >= 0 && ix < expNode.PrevNodes.Count) {
                curLevel = curLevel + 1;
                return expNode.PrevNodes[ix];
            }
            else {
                Debug.Assert(false);
                return expNode;
            }
        }

        public const int c_action_nothing = 0;
        public const int c_action_remove_func = 1;
        public const int c_action_add_ftoi = 2;
        public const int c_action_add_itof = 3;
        public const int c_action_add_ftou = 4;
        public const int c_action_add_utof = 5;

        private static StringBuilder s_IgnoredContent = new StringBuilder();
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
                    if(Calculator.TryGetBool(cond, out var val) && val) {
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
                    if (!Calculator.TryGetBool(cond, out var val) || !val) {
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
                    if (!Calculator.TryGetBool(cond, out var val) || !val) {
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
                    if (!Calculator.TryGetBool(cond, out var val) || !val) {
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
