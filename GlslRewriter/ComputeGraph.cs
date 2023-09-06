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

namespace GlslRewriter
{
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
        public void PrintPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            visits.Add(this);
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
        public void PrintNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
        {
            visits.Add(this);
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
        public virtual void PrintFieldInfo(int indent)
        {
            PrintIndent(indent);
            Console.Write("Type:");
            Console.WriteLine(Type);
        }

        public string GetValue(int maxLevel)
        {
            string val = CalcValue();
            if (string.IsNullOrEmpty(val)) {
                var sb = new StringBuilder();
                GenerateValue(sb, 0, maxLevel);
                val = sb.ToString();
            }
            return val;
        }
        public string GetExpression(int maxLevel)
        {
            var sb = new StringBuilder();
            GenerateExpression(sb, 0, maxLevel);
            m_Expression = sb.ToString();
            return m_Expression;
        }

        public string CalcValue()
        {
            if (null == m_Value) {
                TryCalcValue();
            }
            return null != m_Value ? m_Value : string.Empty;
        }
        public void GenerateValue(StringBuilder sb, int curLevel, int maxLevel)
        {
            if (string.IsNullOrEmpty(m_Value)) {
                TryGenerateValue(sb, curLevel, maxLevel);
            }
            else {
                sb.Append(m_Value);
            }
        }
        public void GenerateExpression(StringBuilder sb, int curLevel, int maxLevel)
        {
            if (string.IsNullOrEmpty(m_Expression)) {
                TryGenerateExpression(sb, curLevel, maxLevel);
            }
            else {
                sb.Append(m_Expression);
            }
        }

        protected virtual void TryCalcValue()
        {
        }
        protected virtual void TryGenerateValue(StringBuilder sb, int curLevel, int maxLevel)
        {
        }
        protected virtual void TryGenerateExpression(StringBuilder sb, int curLevel, int maxLevel)
        {
        }

        public uint Id { get; init; }
        public FuncInfo? OwnFunc = null;
        public string Type = string.Empty;
        public List<ComputeGraphNode> PrevNodes = new List<ComputeGraphNode>();
        public List<ComputeGraphNode> NextNodes = new List<ComputeGraphNode>();
        public List<ComputeGraphNode> OutNodes = new List<ComputeGraphNode>();

        protected string? m_Value = null;
        protected string? m_Expression = null;

        public static void ResetStatic()
        {
            s_NextId = 1;
        }
        private static uint s_NextId = 1;
    }
    public class ComputeGraphConstNode : ComputeGraphNode
    {
        public ComputeGraphConstNode(FuncInfo? ownFunc, string type, string val) : base(ownFunc, type)
        {
            Value = val;
        }
        public override void PrintFieldInfo(int indent)
        {
            base.PrintFieldInfo(indent);

            PrintIndent(indent);
            Console.Write("Value:");
            Console.WriteLine(Value);
        }
        protected override void TryCalcValue()
        {
            string val = Value;
            val = Calculator.ReStringNumeric(val);
            m_Value = val;
        }
        protected override void TryGenerateValue(StringBuilder sb, int curLevel, int maxLevel)
        {
            sb.Append(Value);
        }
        protected override void TryGenerateExpression(StringBuilder sb, int curLevel, int maxLevel)
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
        public override void PrintFieldInfo(int indent)
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
        protected override void TryCalcValue()
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
                    m_Value = PrevNodes[PrevNodes.Count - 1].CalcValue();
                }
                else {
                    m_Value = string.Empty;
                }
            }
        }
        protected override void TryGenerateValue(StringBuilder sb, int curLevel, int maxLevel)
        {
            if (curLevel < maxLevel) {
                if (PrevNodes.Count > 0) {
                    //取最后一次赋值（多次赋值仅出现在分支情形的phi变量赋值），方便代码分析中注释掉不执行的if语句后进行正确计算
                    PrevNodes[PrevNodes.Count - 1].GenerateValue(sb, curLevel + 1, maxLevel);
                }
                else {
                    sb.Append(VarName);
                }
            }
            else {
                sb.Append(VarName);
            }
        }
        protected override void TryGenerateExpression(StringBuilder sb, int curLevel, int maxLevel)
        {
            if (curLevel < maxLevel) {
                if (PrevNodes.Count > 0) {
                    //取最后一次赋值（多次赋值仅出现在分支情形的phi变量赋值），方便代码分析中注释掉不执行的if语句后进行正确计算
                    PrevNodes[PrevNodes.Count - 1].GenerateExpression(sb, curLevel + 1, maxLevel);
                }
                else {
                    sb.Append(VarName);
                }
            }
            else {
                sb.Append(VarName);
            }
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
        public override void PrintFieldInfo(int indent)
        {
            base.PrintFieldInfo(indent);

            PrintIndent(indent);
            Console.Write("Operator:");
            Console.WriteLine(Operator);
        }
        protected override void TryCalcValue()
        {
            if (Operator.Length > 0 && (char.IsLetter(Operator[0]) || Operator[0] == '_')) {
                var args = new List<string>();
                foreach (var p in PrevNodes) {
                    args.Add(p.CalcValue());
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
                        if(VariableTable.ObjectGetValue(varNode, constNode.Value, out var val)) {
                            m_Value = val;
                            handled = true;
                        }
                    }
                    else if (PrevNodes[0] is ComputeGraphCalcNode calcNode) {
                        if (calcNode.Operator == "[]" && calcNode.PrevNodes[0] is ComputeGraphVarNode vnode2 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode2) {
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
                    string objVal = PrevNodes[0].CalcValue();
                    string member = PrevNodes[1].CalcValue();
                    if (Calculator.CalcMember(objType, objVal, member, ref type, out var val)) {
                        m_Value = val;
                    }
                }
            }
            else if (Operator == "[]") {
                bool handled = false;
                if (PrevNodes[1] is ComputeGraphConstNode constNode) {
                    if (PrevNodes[0] is ComputeGraphVarNode varNode) {
                        if(VariableTable.ArrayGetValue(varNode, constNode.Value, out var val)) {
                            m_Value = val;
                            handled = true;
                        }
                    }
                }
                if (!handled) {
                    string objType = PrevNodes[0].Type;
                    string type = Type;
                    string objVal = PrevNodes[0].CalcValue();
                    string ix = PrevNodes[1].CalcValue();
                    if (Calculator.CalcMember(objType, objVal, ix, ref type, out var val)) {
                        m_Value = val;
                    }
                }
            }
            else if (Operator == "=") {
                if (NextNodes[0] is ComputeGraphVarNode vnode) {
                    if (PrevNodes[0] is ComputeGraphVarNode vnode2) {
                        VariableTable.AssignValue(vnode, vnode2);
                    }
                    else if (PrevNodes[0] is ComputeGraphCalcNode calcNode2) {
                        VariableTable.AssignValue(vnode, calcNode2);
                    }
                    else {
                        VariableTable.AssignValue(vnode, PrevNodes[0].CalcValue());
                    }
                }
                else if (NextNodes[0] is ComputeGraphCalcNode calcNode) {
                    if (calcNode.Operator=="." && calcNode.PrevNodes[0] is ComputeGraphVarNode vnode2 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode2) {
                        VariableTable.ObjectAssignValue(vnode2, cnode2.Value, PrevNodes[0].CalcValue());
                    }
                    else if (calcNode.Operator == "[]" && calcNode.PrevNodes[0] is ComputeGraphVarNode vnode3 && calcNode.PrevNodes[1] is ComputeGraphConstNode cnode3) {
                        if (PrevNodes[0] is ComputeGraphVarNode varNode) {
                            VariableTable.ArrayAssignValue(vnode3, cnode3.Value, varNode);
                        }
                        else if (PrevNodes[0] is ComputeGraphCalcNode calcNode2) {
                            VariableTable.ArrayAssignValue(vnode3, cnode3.Value, calcNode2);
                        }
                        else {
                            VariableTable.ArrayAssignValue(vnode3, cnode3.Value, PrevNodes[0].CalcValue());
                        }
                    }
                }
                m_Value = PrevNodes[0].CalcValue();
            }
            else if (PrevNodes.Count == 3) {
                string type = Type;
                string cond = PrevNodes[0].CalcValue();
                string opd1 = PrevNodes[1].CalcValue();
                string opd2 = PrevNodes[2].CalcValue();
                if (Calculator.CalcCondExp(cond, opd1, opd2, ref type, out var val)) {
                    m_Value = val;
                }
            }
            else if (PrevNodes.Count == 2) {
                string type = Type;
                string opd1 = PrevNodes[0].CalcValue();
                string opd2 = PrevNodes[1].CalcValue();
                if (Calculator.CalcBinary(Operator, opd1, opd2, ref type, out var val)) {
                    m_Value = val;
                }
            }
            else {
                string type = Type;
                string opd = PrevNodes[0].CalcValue();
                if (Calculator.CalcUnary(Operator, opd, ref type, out var val)) {
                    m_Value = val;
                }
            }
        }
        protected override void TryGenerateValue(StringBuilder sb, int curLevel, int maxLevel)
        {
            if (Operator.Length > 0 && (char.IsLetter(Operator[0]) || Operator[0] == '_')) {
                sb.Append(Operator);
                sb.Append("(");
                bool first = true;
                foreach (var p in PrevNodes) {
                    if (first)
                        first = false;
                    else
                        sb.Append(",");
                    p.GenerateValue(sb, curLevel + 1, maxLevel);
                }
                sb.Append(")");
            }
            else if (Operator == ".") {
                PrevNodes[0].GenerateValue(sb, curLevel + 1, maxLevel);
                sb.Append(".");
                PrevNodes[1].GenerateValue(sb, curLevel + 1, maxLevel);
            }
            else if (Operator == "[]") {
                PrevNodes[0].GenerateValue(sb, curLevel + 1, maxLevel);
                sb.Append("[");
                PrevNodes[1].GenerateValue(sb, curLevel + 1, maxLevel);
                sb.Append("]");
            }
            else if (Operator == "=") {
                PrevNodes[0].GenerateValue(sb, curLevel + 1, maxLevel);
            }
            else if (PrevNodes.Count == 3) {
                sb.Append("(");
                PrevNodes[0].GenerateValue(sb, curLevel + 1, maxLevel);
                sb.Append(" ? ");
                PrevNodes[1].GenerateValue(sb, curLevel + 1, maxLevel);
                sb.Append(" : ");
                PrevNodes[2].GenerateValue(sb, curLevel + 1, maxLevel);
                sb.Append(")");
            }
            else if (PrevNodes.Count == 2) {
                sb.Append("(");
                PrevNodes[0].GenerateValue(sb, curLevel + 1, maxLevel);
                sb.Append(" ");
                sb.Append(Operator);
                sb.Append(" ");
                PrevNodes[1].GenerateValue(sb, curLevel + 1, maxLevel);
                sb.Append(")");
            }
            else {
                sb.Append("(");
                sb.Append(Operator);
                sb.Append(" ");
                PrevNodes[0].GenerateValue(sb, curLevel + 1, maxLevel);
                sb.Append(")");
            }
        }
        protected override void TryGenerateExpression(StringBuilder sb, int curLevel, int maxLevel)
        {
            if (Operator.Length > 0 && (char.IsLetter(Operator[0]) || Operator[0] == '_')) {
                sb.Append(Operator);
                sb.Append("(");
                bool first = true;
                foreach (var p in PrevNodes) {
                    if (first)
                        first = false;
                    else
                        sb.Append(", ");
                    p.GenerateExpression(sb, curLevel + 1, maxLevel);
                }
                sb.Append(")");
            }
            else if (Operator == ".") {
                PrevNodes[0].GenerateExpression(sb, curLevel + 1, maxLevel);
                sb.Append(".");
                PrevNodes[1].GenerateExpression(sb, curLevel + 1, maxLevel);
            }
            else if (Operator == "[]") {
                PrevNodes[0].GenerateExpression(sb, curLevel + 1, maxLevel);
                sb.Append("[");
                PrevNodes[1].GenerateExpression(sb, curLevel + 1, maxLevel);
                sb.Append("]");
            }
            else if (Operator == "=") {
                PrevNodes[0].GenerateExpression(sb, curLevel + 1, maxLevel);
            }
            else if (PrevNodes.Count == 3) {
                sb.Append("(");
                PrevNodes[0].GenerateExpression(sb, curLevel + 1, maxLevel);
                sb.Append(" ? ");
                PrevNodes[1].GenerateExpression(sb, curLevel + 1, maxLevel);
                sb.Append(" : ");
                PrevNodes[2].GenerateExpression(sb, curLevel + 1, maxLevel);
                sb.Append(")");
            }
            else if (PrevNodes.Count == 2) {
                sb.Append("(");
                PrevNodes[0].GenerateExpression(sb, curLevel + 1, maxLevel);
                sb.Append(" ");
                sb.Append(Operator);
                sb.Append(" ");
                PrevNodes[1].GenerateExpression(sb, curLevel + 1, maxLevel);
                sb.Append(")");
            }
            else {
                sb.Append("(");
                sb.Append(Operator);
                sb.Append(" ");
                PrevNodes[0].GenerateExpression(sb, curLevel + 1, maxLevel);
                sb.Append(")");
            }
        }

        public string Operator = string.Empty;

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
            var nextVisits = new HashSet<ComputeGraphNode>();
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
            foreach (var pair in VarNodes) {
                var node = pair.Value;
                node.Print(ownFunc, prevVisits, nextVisits, 1);
            }
        }

        public static void ResetStatic()
        {
            ComputeGraphNode.ResetStatic();
        }
    }
}
