using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GlslRewriter.ComputeGraph;
using static GlslRewriter.Program;

namespace GlslRewriter
{
    internal class ComputeGraphNode
    {
        internal ComputeGraphNode(FuncInfo? ownFunc, string type)
        {
            Id = s_NextId++;
            OwnFunc = ownFunc;
            Type = type;
        }
        internal void AddPrev(ComputeGraphNode node)
        {
            PrevNodes.Add(node);
        }
        internal void AddNext(ComputeGraphNode node)
        {
            NextNodes.Add(node);
        }
        internal void AddOut(ComputeGraphNode node)
        {
            Debug.Assert(!OutNodes.Contains(node));
            OutNodes.Add(node);
        }

        internal void VisitPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
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
        internal void VisitNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
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

        internal void VisitAllPrev(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
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
        internal void VisitAllNext(HashSet<ComputeGraphNode> visits, VisitDelegation visitorCallback)
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

        internal void Print(FuncInfo? ownFunc, HashSet<ComputeGraphNode> prevVisits, HashSet<ComputeGraphNode> nextVisits, int indent)
        {
            PrintInfo(indent);
            PrintPrev(ownFunc, prevVisits, indent);
            PrintNext(ownFunc, nextVisits, indent);
        }
        internal void PrintPrev(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
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
        internal void PrintNext(FuncInfo? ownFunc, HashSet<ComputeGraphNode> visits, int indent)
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
        internal void PrintIndent(int indent)
        {
            Console.Write(Literal.GetSpaceString(indent));
        }
        internal void PrintInfo(int indent)
        {
            PrintIndent(indent);
            Console.WriteLine("[Node:{0}, {1}]", Id, GetType().Name);
            PrintFieldInfo(indent + 1);
        }
        internal virtual void PrintFieldInfo(int indent)
        {
            PrintIndent(indent);
            Console.Write("Type:");
            Console.WriteLine(Type);
        }

        internal string GetValue()
        {
            return CalcValue(0);
        }
        internal string GetExpression()
        {
            return CalcExpression(0);
        }

        internal string CalcValue(int curLevel)
        {
            TryGenerateValue(curLevel);
            return null != m_Value ? m_Value : string.Empty;
        }
        internal string CalcExpression(int curLevel)
        {
            TryGenerateString(curLevel);
            return null != m_Expression ? m_Expression : string.Empty;
        }

        protected virtual void TryGenerateValue(int curLevel)
        {
        }
        protected virtual void TryGenerateString(int curLevel)
        {
        }

        internal uint Id { get; init; }
        internal FuncInfo? OwnFunc = null;
        internal string Type = string.Empty;
        internal List<ComputeGraphNode> PrevNodes = new List<ComputeGraphNode>();
        internal List<ComputeGraphNode> NextNodes = new List<ComputeGraphNode>();
        internal List<ComputeGraphNode> OutNodes = new List<ComputeGraphNode>();

        protected string? m_Value = null;
        protected string? m_Expression = null;

        internal static void ResetStatic()
        {
            s_NextId = 1;
        }
        private static uint s_NextId = 1;
    }
    internal class ComputeGraphConstNode : ComputeGraphNode
    {
        internal ComputeGraphConstNode(FuncInfo? ownFunc, string type, string val) : base(ownFunc, type)
        {
            Value = val;
        }
        internal override void PrintFieldInfo(int indent)
        {
            base.PrintFieldInfo(indent);

            PrintIndent(indent);
            Console.Write("Value:");
            Console.WriteLine(Value);
        }
        protected override void TryGenerateValue(int curLevel)
        {
            if (null == m_Value) {
                m_Value = Value;
            }
        }
        protected override void TryGenerateString(int curLevel)
        {
            if (null == m_Expression) {
                m_Expression = Value;
            }
        }

        internal string Value = string.Empty;

    }
    internal class ComputeGraphVarNode : ComputeGraphNode
    {
        internal ComputeGraphVarNode(FuncInfo? ownFunc, string type, string name) : base(ownFunc, type)
        {
            VarName = name;
        }
        internal override void PrintFieldInfo(int indent)
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
        protected override void TryGenerateValue(int curLevel)
        {
            if (null == m_Value) {
                m_Value = VarName;
            }
        }
        protected override void TryGenerateString(int curLevel)
        {
            if (curLevel < 16) {
                if (PrevNodes.Count > 0) {
                    m_Expression = PrevNodes[0].CalcExpression(curLevel + 1);
                }
                else {
                    m_Expression = VarName;
                }
            }
            else {
                m_Expression = VarName;
            }
        }

        internal string VarName = string.Empty;
        internal bool IsInOut = false;
        internal bool IsOut = false;
        internal bool IsParam = false;

    }
    internal class ComputeGraphCalcNode : ComputeGraphNode
    {
        internal ComputeGraphCalcNode(FuncInfo? ownFunc, string type, string op) : base(ownFunc, type)
        {
            Operator = op;
        }
        internal override void PrintFieldInfo(int indent)
        {
            base.PrintFieldInfo(indent);

            PrintIndent(indent);
            Console.Write("Operator:");
            Console.WriteLine(Operator);
        }
        protected override void TryGenerateValue(int curLevel)
        {
            if (null == m_Value) {
                m_Value = string.Empty;
            }
        }
        protected override void TryGenerateString(int curLevel)
        {
            StringBuilder sb = new StringBuilder();
            if (Operator.Length > 0 && (char.IsLetter(Operator[0]) || Operator[0] == '_')) {
                sb.Append(Operator);
                sb.Append("(");
                bool first = true;
                foreach (var p in PrevNodes) {
                    if (first)
                        first = false;
                    else
                        sb.Append(", ");
                    sb.Append(p.CalcExpression(curLevel + 1));
                }
                sb.Append(")");
            }
            else if (Operator == ".") {
                sb.Append(PrevNodes[0].CalcExpression(curLevel + 1));
                sb.Append(".");
                sb.Append(PrevNodes[1].CalcExpression(curLevel + 1));
            }
            else if (Operator == "[]") {
                sb.Append(PrevNodes[0].CalcExpression(curLevel + 1));
                sb.Append("[");
                sb.Append(PrevNodes[1].CalcExpression(curLevel + 1));
                sb.Append("]");
            }
            else if (Operator == "=") {
                sb.Append(PrevNodes[0].CalcExpression(curLevel + 1));
            }
            else if (PrevNodes.Count == 3) {
                sb.Append("(");
                sb.Append(PrevNodes[0].CalcExpression(curLevel + 1));
                sb.Append(" ? ");
                sb.Append(PrevNodes[1].CalcExpression(curLevel + 1));
                sb.Append(" : ");
                sb.Append(PrevNodes[2].CalcExpression(curLevel + 1));
                sb.Append(")");
            }
            else if (PrevNodes.Count == 2) {
                sb.Append("(");
                sb.Append(PrevNodes[0].CalcExpression(curLevel + 1));
                sb.Append(" ");
                sb.Append(Operator);
                sb.Append(" ");
                sb.Append(PrevNodes[1].CalcExpression(curLevel + 1));
                sb.Append(")");
            }
            else {
                sb.Append("(");
                sb.Append(Operator);
                sb.Append(" ");
                sb.Append(PrevNodes[0].CalcExpression(curLevel + 1));
                sb.Append(")");
            }
            m_Expression = sb.ToString();
        }

        internal string Operator = string.Empty;

    }
    internal class ComputeGraph
    {
        internal delegate bool VisitDelegation(ComputeGraphNode node);

        internal List<ComputeGraphNode> RootNodes = new List<ComputeGraphNode>();
        internal Dictionary<string, ComputeGraphVarNode> VarNodes = new Dictionary<string, ComputeGraphVarNode>();

        internal void Reset(FuncInfo? ownFunc)
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
        internal void VisitPrev(FuncInfo? ownFunc, string vname, VisitDelegation visitorCallback)
        {
            if (VarNodes.TryGetValue(vname, out var node)) {
                var visits = new HashSet<ComputeGraphNode>();
                node.VisitPrev(ownFunc, visits, visitorCallback);
            }
        }
        internal void VisitNext(FuncInfo? ownFunc, string vname, VisitDelegation visitorCallback)
        {
            if (VarNodes.TryGetValue(vname, out var node)) {
                var visits = new HashSet<ComputeGraphNode>();
                node.VisitNext(ownFunc, visits, visitorCallback);
            }
        }
        internal void VisitPrev(FuncInfo? ownFunc, ComputeGraphNode node, VisitDelegation visitorCallback)
        {
            var visits = new HashSet<ComputeGraphNode>();
            node.VisitPrev(ownFunc, visits, visitorCallback);
        }
        internal void VisitNext(FuncInfo? ownFunc, ComputeGraphNode node, VisitDelegation visitorCallback)
        {
            var visits = new HashSet<ComputeGraphNode>();
            node.VisitNext(ownFunc, visits, visitorCallback);
        }
        internal void VisitAllPrev(string vname, VisitDelegation visitorCallback)
        {
            if (VarNodes.TryGetValue(vname, out var node)) {
                var visits = new HashSet<ComputeGraphNode>();
                node.VisitAllPrev(visits, visitorCallback);
            }
        }
        internal void VisitAllNext(string vname, VisitDelegation visitorCallback)
        {
            if (VarNodes.TryGetValue(vname, out var node)) {
                var visits = new HashSet<ComputeGraphNode>();
                node.VisitAllNext(visits, visitorCallback);
            }
        }
        internal void VisitAllPrev(ComputeGraphNode node, VisitDelegation visitorCallback)
        {
            var visits = new HashSet<ComputeGraphNode>();
            node.VisitAllPrev(visits, visitorCallback);
        }
        internal void VisitAllNext(ComputeGraphNode node, VisitDelegation visitorCallback)
        {
            var visits = new HashSet<ComputeGraphNode>();
            node.VisitAllNext(visits, visitorCallback);
        }
        internal void Print(FuncInfo? ownFunc)
        {
            var prevVisits = new HashSet<ComputeGraphNode>();
            var nextVisits = new HashSet<ComputeGraphNode>();
            foreach (var pair in VarNodes) {
                var node = pair.Value;
                node.Print(ownFunc, prevVisits, nextVisits, 1);
            }
        }

        internal static void ResetStatic()
        {
            ComputeGraphNode.ResetStatic();
        }
    }
}
