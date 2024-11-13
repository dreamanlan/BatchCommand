using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using ScriptableFramework;

namespace CppDebugScript
{
    /// <summary>
    /// Array is alias name for continuous variable, Can only be used in init/mov/arrget/arrset, not in calc and API.
    /// The API can treat 64-bit integers as pointers, so it can do a lot of things.
    /// But the script itself can only operate on three types: long/double/string, without going through the API.
    /// </summary>
    public enum InsEnum : int
    {
        CALL = 0,
        RET,
        JMP,
        JMPIFNOT,
        INC,
        INCV,
        MOV,
        ARRGET,
        ARRSET,
        NEG,
        ADD,
        SUB,
        MUL,
        DIV,
        MOD,
        AND,
        OR,
        NOT,
        GT,
        GE,
        EQ,
        NE,
        LE,
        LT,
        LSHIFT,
        RSHIFT,
        URSHIFT,
        BITAND,
        BITOR,
        BITXOR,
        BITNOT,
        ARGC,
        ARGV,
        ADDR,
        PTRGET,
        PTRSET,
        JPTR,
        NUM
    }
    public sealed class DebugScriptCompiler
    {
        public string LoadApiDefine(string txt)
        {
            int apiId = 0;
            m_Apis.Clear();

            Dsl.DslFile file = new Dsl.DslFile();
            var err = new StringBuilder();
            if (file.LoadFromString(txt, (msg) => { err.AppendLine(msg); })) {
                foreach (var dslInfo in file.DslInfos) {
                    var id = dslInfo.GetId();
                    if (id == "defapi") {
                        var call = dslInfo as Dsl.FunctionData;
                        var func = dslInfo as Dsl.FunctionData;
                        if (null != func) {
                            call = func.ThisOrLowerOrderCall;
                        }
                        if (!call.IsValid())
                            continue;
                        var name = call.GetParamId(0);
                        var type = call.GetParamId(1);
                        var paramsStr = call.GetParamId(2);

                        ParamsEnum paramsEnum = ParamsEnum.NoParams;
                        if (paramsStr.Length > 0 && char.IsNumber(paramsStr[0])) {
                            int.TryParse(paramsStr, out var v);
                            paramsEnum = (ParamsEnum)v;
                        }
                        else {
                            Enum.TryParse<ParamsEnum>(paramsStr, true, out paramsEnum);
                        }

                        var api = new ApiInfo { ApiId = apiId, Name = name, Type = type, Params = paramsEnum };
                        ++apiId;

                        var pts = call.GetParam(3) as Dsl.FunctionData;
                        foreach (var p in pts.Params) {
                            api.ParamTypes.Add(p.GetId());
                        }
                        m_Apis.Add(name, api);
                    }
                }
            }
            return err.ToString();
        }
        public Dictionary<string, int> BuildApiIds()
        {
            var dict = new Dictionary<string, int>();
            foreach (var pair in m_Apis) {
                dict.Add(pair.Key, pair.Value.ApiId);
            }
            return dict;
        }
        public string LoadStructDefine(string txt)
        {
            m_PredefinedStructInfos.Clear();

            Dsl.DslFile file = new Dsl.DslFile();
            var err = new StringBuilder();
            if (file.LoadFromString(txt, (msg) => { err.AppendLine(msg); })) {
                foreach (var dslInfo in file.DslInfos) {
                    var id = dslInfo.GetId();
                    if (id == "struct") {
                        var callData = dslInfo as Dsl.FunctionData;
                        if (!callData.IsValid())
                            continue;
                        var struInfo = ParseStruct(callData, err);
                        if (null != struInfo) {
                            m_PredefinedStructInfos.Add(struInfo.Name, struInfo);
                        }
                    }
                }
            }
            return err.ToString();
        }

        public bool Compile(string txt, out string errInfo)
        {
            Reset();

            var err = new StringBuilder();
            var file = new Dsl.DslFile();
            file.onGetToken = (ref Dsl.Common.DslAction dslAction, ref Dsl.Common.DslToken dslToken, ref string tok, ref short val, ref int line) => {
                if (tok == "return") {
                    var oldCurTok = dslToken.getCurToken();
                    var oldLastTok = dslToken.getLastToken();
                    if (dslToken.PeekNextValidChar(0) == ';')
                        return false;
                    dslToken.setCurToken("<-");
                    dslToken.setLastToken(oldCurTok);
                    dslToken.enqueueToken(dslToken.getCurToken(), dslToken.getOperatorTokenValue(), line);
                    dslToken.setCurToken(oldCurTok);
                    dslToken.setLastToken(oldLastTok);
                    return true;
                }
                return false;
            };
            if (file.LoadFromString(txt, (msg) => { err.AppendLine(msg); })) {
                int count = file.DslInfos.Count;
                for (int i = 0; i < count; ++i) {
                    var call = file.DslInfos[i] as Dsl.FunctionData;
                    CompileToplevelSyntax(call, err);
                }
            }
            errInfo = err.ToString();
            return string.IsNullOrEmpty(errInfo);
        }
        public void DumpAsm(StringBuilder txt)
        {
            int indent = 0;
            txt.AppendLine("consts:");
            ++indent;
            foreach(var pair in m_ConstInfos) {
                var info = pair.Value;
                txt.AppendLine("{0}value:{1} type:{2} index:{3}", Literal.GetIndentString(indent), info.StrValue, info.Type, info.Index);
            }
            --indent;
            txt.AppendLine();
            txt.AppendLine("globals:");
            ++indent;
            foreach (var pair in m_VarInfos) {
                string name = pair.Key;
                if (pair.Value.TryGetValue(0, out var info)) {
                    txt.AppendLine("{0}name:{1} type:{2} count:{3} index:{4}", Literal.GetIndentString(indent), info.Name, info.Type, info.Count, info.Index);
                    ++indent;
                    if (null != info.InitValues) {
                        foreach (var v in info.InitValues) {
                            txt.AppendLine("{0}value:{1}", Literal.GetIndentString(indent), v);
                        }
                    }
                    --indent;
                }
            }
            --indent;
            txt.AppendLine();
            txt.AppendLine("locals:");
            ++indent;
            foreach (var pair in m_VarInfos) {
                string name = pair.Key;
                foreach(var pair2 in pair.Value) {
                    int block = pair2.Key;
                    if (block > 0) {
                        var info = pair2.Value;
                        txt.AppendLine("{0}name:{1} type:{2} count:{3} index:{4} block:{5}", Literal.GetIndentString(indent), info.Name, info.Type, info.Count, info.Index, block);
                    }
                }
            }
            --indent;
            txt.AppendLine();
            foreach (var hook in m_Hooks) {
                if (txt.Length > 0)
                    txt.AppendLine();
                txt.AppendLine("hook:{0}", hook.Name);
                if (hook.ShareWith.Count > 0) {
                    txt.Append("share with:");
                    foreach(var name in hook.ShareWith) {
                        txt.Append(' ');
                        txt.Append(name);
                    }
                    txt.AppendLine();
                }
                txt.AppendLine("{");
                ++indent;
                txt.AppendLine("{0}onenter {{", Literal.GetIndentString(indent));
                ++indent;
                DumpAsm(txt, indent, hook.OnEnter);
                --indent;
                txt.AppendLine("{0}}}", Literal.GetIndentString(indent));
                txt.AppendLine();
                txt.AppendLine("{0}onexit {{", Literal.GetIndentString(indent));
                ++indent;
                DumpAsm(txt, indent, hook.OnExit);
                --indent;
                txt.AppendLine("{0}}}", Literal.GetIndentString(indent));
                --indent;
                txt.AppendLine("}");
            }
        }
        public void UploadToCppVM()
        {
            CppDbgScpInterface.CppDbgScp_ResetVM();

            var consts = new SortedDictionary<int, ConstInfo>();
            CollectConstInfo("int", consts);
            foreach(var pair in consts) {
                var info = pair.Value;
                long.TryParse(info.StrValue, out var v);
                CppDbgScpInterface.CppDbgScp_AllocConstInt(v);
            }
            consts.Clear();
            CollectConstInfo("float", consts);
            foreach (var pair in consts) {
                var info = pair.Value;
                double.TryParse(info.StrValue, out var v);
                CppDbgScpInterface.CppDbgScp_AllocConstFloat(v);
            }
            consts.Clear();
            CollectConstInfo("string", consts);
            foreach (var pair in consts) {
                var info = pair.Value;
                var v = info.StrValue;
                IntPtr str = Marshal.StringToHGlobalAnsi(v);
                CppDbgScpInterface.CppDbgScp_AllocConstString(str);
            }

            var globals = new SortedDictionary<int, VarInfo>();
            CollectGlobalInfo("int", globals);
            foreach (var pair in globals) {
                var info = pair.Value;
                if (null == info.InitValues) {
                    CppDbgScpInterface.CppDbgScp_AllocGlobalInt(0);
                }
                else {
                    foreach (var strVal in info.InitValues) {
                        long.TryParse(strVal, out var v);
                        CppDbgScpInterface.CppDbgScp_AllocGlobalInt(v);
                    }
                }
            }
            globals.Clear();
            CollectGlobalInfo("float", globals);
            foreach (var pair in globals) {
                var info = pair.Value;
                if (null == info.InitValues) {
                    CppDbgScpInterface.CppDbgScp_AllocGlobalFloat(0);
                }
                else {
                    foreach (var strVal in info.InitValues) {
                        double.TryParse(strVal, out var v);
                        CppDbgScpInterface.CppDbgScp_AllocGlobalFloat(v);
                    }
                }
            }
            globals.Clear();
            CollectGlobalInfo("string", globals);
            foreach (var pair in globals) {
                var info = pair.Value;
                if (null == info.InitValues) {
                    IntPtr str = Marshal.StringToHGlobalAnsi(string.Empty);
                    CppDbgScpInterface.CppDbgScp_AllocGlobalString(str);
                }
                else {
                    foreach (var strVal in info.InitValues) {
                        IntPtr str = Marshal.StringToHGlobalAnsi(strVal);
                        CppDbgScpInterface.CppDbgScp_AllocGlobalString(str);
                    }
                }
            }

            foreach(var hook in m_Hooks) {
                IntPtr name = Marshal.StringToHGlobalAnsi(hook.Name); 
                IntPtr enterPtr = Marshal.AllocHGlobal(hook.OnEnter.Count * sizeof(int));
                IntPtr exitPtr = Marshal.AllocHGlobal(hook.OnExit.Count * sizeof(int));

                Marshal.Copy(hook.OnEnter.ToArray(), 0, enterPtr, hook.OnEnter.Count);
                Marshal.Copy(hook.OnExit.ToArray(), 0, exitPtr, hook.OnExit.Count);

                int hookId = CppDbgScpInterface.CppDbgScp_AddHook(name, enterPtr, hook.OnEnter.Count, exitPtr, hook.OnExit.Count);
                LogSystem.Warn("hook:{0} id:{1}", hook.Name, hookId);

                foreach(var other in hook.ShareWith) {
                    IntPtr otherFunc = Marshal.StringToHGlobalAnsi(other);
                    int rid = CppDbgScpInterface.CppDbgScp_ShareWith(hookId, otherFunc);
                    LogSystem.Warn("share with:{0} id:{1}", other, rid);
                }

                Marshal.FreeHGlobal(enterPtr);
                Marshal.FreeHGlobal(exitPtr);
            }

            CppDbgScpInterface.CppDbgScp_StartVM();
        }

        private void CollectConstInfo(string type, SortedDictionary<int, ConstInfo> constInfos)
        {
            foreach(var pair in m_ConstInfos) {
                var info = pair.Value;
                if(info.Type == type) {
                    constInfos.Add(info.Index, info);
                }
            }
        }
        private void CollectGlobalInfo(string type, SortedDictionary<int, VarInfo> varInfos)
        {
            foreach (var pair in m_VarInfos) {
                var dict = pair.Value;
                foreach (var pair2 in dict) {
                    int blockId = pair2.Key;
                    var info = pair2.Value;
                    if (blockId == 0 && info.Type == type) {
                        varInfos.Add(info.Index, info);
                    }
                }
            }
        }

        private void DumpAsm(StringBuilder txt, int indent, List<int> codes)
        {
            for (int pos = 0; pos < codes.Count; ++pos) {
                int opcode = codes[pos];
                InsEnum op = DecodeInsEnum(opcode);
                switch (op) {
                    case InsEnum.CALL:
                        DumpCall(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.RET:
                        DumpRet(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.JMP:
                        DumpJmp(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.JMPIFNOT:
                        DumpJmpIfNot(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.INC:
                        DumpInc(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.INCV:
                        DumpIncVal(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.MOV:
                        DumpMov(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.ARRGET:
                        DumpArrGet(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.ARRSET:
                        DumpArrSet(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.ADD:
                        DumpBinary(txt, indent, codes, ref pos, "ADD");
                        break;
                    case InsEnum.SUB:
                        DumpBinary(txt, indent, codes, ref pos, "SUB");
                        break;
                    case InsEnum.MUL:
                        DumpBinary(txt, indent, codes, ref pos, "MUL");
                        break;
                    case InsEnum.DIV:
                        DumpBinary(txt, indent, codes, ref pos, "DIV");
                        break;
                    case InsEnum.MOD:
                        DumpBinary(txt, indent, codes, ref pos, "MOD");
                        break;
                    case InsEnum.AND:
                        DumpBinary(txt, indent, codes, ref pos, "AND");
                        break;
                    case InsEnum.OR:
                        DumpBinary(txt, indent, codes, ref pos, "OR");
                        break;
                    case InsEnum.NOT:
                        DumpUnary(txt, indent, codes, ref pos, "NOT");
                        break;
                    case InsEnum.GT:
                        DumpBinary(txt, indent, codes, ref pos, "GT");
                        break;
                    case InsEnum.GE:
                        DumpBinary(txt, indent, codes, ref pos, "GE");
                        break;
                    case InsEnum.EQ:
                        DumpBinary(txt, indent, codes, ref pos, "EQ");
                        break;
                    case InsEnum.NE:
                        DumpBinary(txt, indent, codes, ref pos, "NE");
                        break;
                    case InsEnum.LE:
                        DumpBinary(txt, indent, codes, ref pos, "LE");
                        break;
                    case InsEnum.LT:
                        DumpBinary(txt, indent, codes, ref pos, "LT");
                        break;
                    case InsEnum.LSHIFT:
                        DumpBinary(txt, indent, codes, ref pos, "LSHIFT");
                        break;
                    case InsEnum.RSHIFT:
                        DumpBinary(txt, indent, codes, ref pos, "RSHIFT");
                        break;
                    case InsEnum.URSHIFT:
                        DumpBinary(txt, indent, codes, ref pos, "URSHIFT");
                        break;
                    case InsEnum.BITAND:
                        DumpBinary(txt, indent, codes, ref pos, "BITAND");
                        break;
                    case InsEnum.BITOR:
                        DumpBinary(txt, indent, codes, ref pos, "BITOR");
                        break;
                    case InsEnum.BITXOR:
                        DumpBinary(txt, indent, codes, ref pos, "BITXOR");
                        break;
                    case InsEnum.BITNOT:
                        DumpUnary(txt, indent, codes, ref pos, "BITNOT");
                        break;
                    case InsEnum.ARGC:
                        DumpArgc(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.ARGV:
                        DumpArgv(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.ADDR:
                        DumpAddr(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.PTRGET:
                        DumpPtrGet(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.PTRSET:
                        DumpPtrSet(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.JPTR:
                        DumpJaggedPtr(txt, indent, codes, ref pos);
                        break;
                }
            }
        }
        private void DumpCall(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            txt.Append("{0}{1}: {2} = CALL", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index));
            for (int i = 0; i < argNum + 1; i += 2) {
                ++pos;
                int operand = codes[pos];
                if (i <= argNum) {
                    if (i == 0) {
                        txt.Append(' ');
                        DecodeOperand1(operand, out var index1);
                        txt.Append(index1);
                    }
                    else {
                        txt.Append(", ");
                        DecodeOperand1(operand, out var isGlobal1, out var isConstOrTemp1, out var type1, out var index1);
                        txt.Append(BuildVar(isGlobal1, isConstOrTemp1, type1, index1));
                    }
                }
                if (i + 1 <= argNum) {
                    if (i == 0)
                        txt.Append(' ');
                    else
                        txt.Append(", ");
                    DecodeOperand2(operand, out var isGlobal2, out var isConstOrTemp2, out var type2, out var index2);
                    txt.Append(BuildVar(isGlobal2, isConstOrTemp2, type2, index2));
                }
            }
            txt.AppendLine();
        }
        private void DumpRet(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            txt.AppendLine("{0}{1}: RET {2}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index));
        }
        private void DumpJmp(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var offset);
            txt.AppendLine("{0}{1}: JMP {2}", Literal.GetIndentString(indent), ix, offset);
        }
        private void DumpJmpIfNot(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var offset);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var isConstOrTemp1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: JMPIFNOT {2}, {3}", Literal.GetIndentString(indent), ix, offset, BuildVar(isGlobal1, isConstOrTemp1, type1, index1));
        }
        private void DumpInc(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            txt.AppendLine("{0}{1}: {2} = INC {2}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index));
        }
        private void DumpIncVal(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var isConstOrTemp1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: {2} = INCV {2} {3}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index), BuildVar(isGlobal1, isConstOrTemp1, type1, index1));
        }
        private void DumpMov(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var isConstOrTemp1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: MOV {2}, {3}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index), BuildVar(isGlobal1, isConstOrTemp1, type1, index1));
        }
        private void DumpArrGet(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var isConstOrTemp1, out var type1, out var index1);
            DecodeOperand2(operand, out var isGlobal2, out var isConstOrTemp2, out var type2, out var index2);
            txt.AppendLine("{0}{1}: {2} = ARRGET {3}, {4}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index), BuildVar(isGlobal1, isConstOrTemp1, type1, index1), BuildVar(isGlobal2, isConstOrTemp2, type2, index2));
        }
        private void DumpArrSet(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var isConstOrTemp1, out var type1, out var index1);
            DecodeOperand2(operand, out var isGlobal2, out var isConstOrTemp2, out var type2, out var index2);
            txt.AppendLine("{0}{1}: ARRSET {2}, {3}, {4}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index), BuildVar(isGlobal1, isConstOrTemp1, type1, index1), BuildVar(isGlobal2, isConstOrTemp2, type2, index2));
        }
        private void DumpUnary(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var isConstOrTemp1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: {2} = {3} {4}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index), op, BuildVar(isGlobal1, isConstOrTemp1, type1, index1));
        }
        private void DumpBinary(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var isConstOrTemp1, out var type1, out var index1);
            DecodeOperand2(operand, out var isGlobal2, out var isConstOrTemp2, out var type2, out var index2);
            txt.AppendLine("{0}{1}: {2} = {3} {4}, {5}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index), op, BuildVar(isGlobal1, isConstOrTemp1, type1, index1), BuildVar(isGlobal2, isConstOrTemp2, type2, index2));
        }
        private void DumpArgc(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            txt.AppendLine("{0}{1}: {2} = ARGC", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index));
        }
        private void DumpArgv(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var isConstOrTemp1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: {2} = ARGV {3}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index), BuildVar(isGlobal1, isConstOrTemp1, type1, index1));
        }
        private void DumpAddr(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var isConstOrTemp1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: {2} = ADDR {3}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index), BuildVar(isGlobal1, isConstOrTemp1, type1, index1));
        }
        private void DumpPtrGet(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var isConstOrTemp1, out var type1, out var index1);
            DecodeOperand2(operand, out var isGlobal2, out var isConstOrTemp2, out var type2, out var index2);
            txt.AppendLine("{0}{1}: {2} = PTRGET {3}, {4}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index), BuildVar(isGlobal1, isConstOrTemp1, type1, index1), BuildVar(isGlobal2, isConstOrTemp2, type2, index2));
        }
        private void DumpPtrSet(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var isConstOrTemp1, out var type1, out var index1);
            DecodeOperand2(operand, out var isGlobal2, out var isConstOrTemp2, out var type2, out var index2);
            txt.AppendLine("{0}{1}: PTRSET {2}, {3}, {4}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index), BuildVar(isGlobal1, isConstOrTemp1, type1, index1), BuildVar(isGlobal2, isConstOrTemp2, type2, index2));
        }
        private void DumpJaggedPtr(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var isConstOrTemp, out var type, out var index);
            txt.Append("{0}{1}: {2} = JPTR", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, isConstOrTemp, type, index));
            for (int i = 0; i < argNum + 1; i += 2) {
                ++pos;
                int operand = codes[pos];
                if (i <= argNum) {
                    if (i == 0) {
                        txt.Append(' ');
                    }
                    else {
                        txt.Append(", ");
                    }
                    DecodeOperand1(operand, out var isGlobal1, out var isConstOrTemp1, out var type1, out var index1);
                    txt.Append(BuildVar(isGlobal1, isConstOrTemp1, type1, index1));
                }
                if (i + 1 <= argNum) {
                    txt.Append(", ");
                    DecodeOperand2(operand, out var isGlobal2, out var isConstOrTemp2, out var type2, out var index2);
                    txt.Append(BuildVar(isGlobal2, isConstOrTemp2, type2, index2));
                }
            }
            txt.AppendLine();
        }
        private string BuildVar(bool isGlobal, bool isConstOrTemp, string type, int index)
        {
            if (string.IsNullOrEmpty(type))
                return string.Empty;
            string name = isConstOrTemp ? (isGlobal ? "Const" : "Temp") : (isGlobal ? "Global" : "Local");

            string val = string.Empty;
            if (isGlobal && isConstOrTemp) {
                foreach (var pair in m_ConstInfos) {
                    if (pair.Value.Type == type && pair.Value.Index == index) {
                        val = pair.Value.StrValue;
                        break;
                    }
                }
            }
            return string.Format("{0}[{1}]:{2}({3})", name, index, val, type);
        }

        private void CompileToplevelSyntax(Dsl.ISyntaxComponent comp, StringBuilder err)
        {
            var callData = comp as Dsl.FunctionData;
            if (null != callData) {
                string id = callData.GetId();
                if (id == "=") {//init global
                    var param1 = callData.GetParam(0) as Dsl.FunctionData;
                    if (null != param1 && param1.GetId() == ":") {
                        //var : type = init_val
                        var param2 = callData.GetParam(1);
                        string name = param1.GetParamId(0);
                        string type = ParseType(param1.GetParam(1), err, out var count);
                        if (IsGlobalVar(name)) {
                            var vinfo = new VarInfo { Name = name, Type = type, Count = count };
                            AddVar(vinfo);

                            var info = new SemanticInfo { TargetOperation = TargetOperationEnum.VarAssign, TargetType = type, TargetCount = count, TargetName = name };
                            CompileExpression(param2, m_TempCodes, err, ref info);
                            CurBlock().ResetTempVars();

                            if (null == info.ResultValues) {
                                err.AppendFormat("Illegal operator=, global variable can only be initialized with constant, code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                                err.AppendLine();
                            }
                            else {
                                vinfo.InitValues = info.ResultValues.ToArray();
                            }
                        }
                        else {
                            err.AppendFormat("Illegal operator=, global variable must start with '@', code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                            err.AppendLine();
                        }
                    }
                    else {
                        err.AppendFormat("Illegal operator=, left operand must be 'name : type', code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                        err.AppendLine();
                    }
                }
                else if (id == ":") {
                    //var : type
                    string name = callData.GetParamId(0);
                    string type = callData.GetParamId(1);
                    if (IsGlobalVar(name)) {
                        var vinfo = new VarInfo { Name = name, Type = type, InitValues = null };
                        AddVar(vinfo);
                    }
                    else {
                        err.AppendFormat("Illegal operator=, global variable must start with '@', code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                        err.AppendLine();
                    }
                }
                else if (id == "struct") {
                    var struInfo = ParseStruct(callData, err);
                    if (null != struInfo) {
                        m_StructInfos.Add(struInfo.Name, struInfo);
                    }
                }
                else if (id == "hook") {
                    if (callData.IsHighOrder) {
                        var head = callData.LowerOrderFunction;
                        Debug.Assert(null != head);
                        string name = head.GetParamId(0);
                        var hookInfo = new HookInfo { Name = name };
                        for(int i = 1; i < head.GetParamNum(); ++i) {
                            hookInfo.ShareWith.Add(head.GetParamId(i));
                        }
                        PushHookInfo(hookInfo);
                        PushBlock(false, false);
                        if (callData.HaveStatement()) {
                            foreach (var p in callData.Params) {
                                var funcData = p as Dsl.FunctionData;
                                if (null != funcData) {
                                    string bid = funcData.GetId();
                                    if (bid == "onenter") {
                                        foreach (var f in funcData.Params) {
                                            CompileSyntaxInHook(f, hookInfo.OnEnter, err);
                                        }
                                    }
                                    else if (bid == "onexit") {
                                        foreach (var f in funcData.Params) {
                                            CompileSyntaxInHook(f, hookInfo.OnExit, err);
                                        }
                                    }
                                    else {
                                        err.AppendFormat("Hook syntax error, hook definition can only include onenter block or/and onexit block, code:{0}, line:{1}", p.ToScriptString(false), p.GetLine());
                                        err.AppendLine();
                                    }
                                }
                                else {
                                    err.AppendFormat("Hook syntax error, hook definition can only include onenter block or/and onexit block, code:{0}, line:{1}", p.ToScriptString(false), p.GetLine());
                                    err.AppendLine();
                                }
                            }
                        }
                        else {
                            err.AppendFormat("Hook syntax error, hook definition must include onenter block or onexit block, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        PopBlock();
                        PopHookInfo();
                        m_Hooks.Add(hookInfo);
                    }
                }
                else {
                    err.AppendFormat("Unknown toplevel syntax, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                }
            }
            else {
                err.AppendFormat("Unknown toplevel syntax, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
        }
        private void CompileSyntaxInHook(Dsl.ISyntaxComponent comp, List<int> codes, StringBuilder err)
        {
            var stmData = comp as Dsl.StatementData;
            if (null != stmData) {
                string id = stmData.GetId();
                if (id == "if") {
                    //if(exp) func_call;
                    //if(exp) func_call elif(exp) func_call else func_call;
                    //if(exp){...}elif(exp){...}else{...};
                    if (stmData.GetFunctionNum() >= 2) {
                        int jmpIfNot = 0;
                        List<int> exitFixes = new List<int>();
                        for (int i = 0; i < stmData.GetFunctionNum(); ++i) {
                            if (i > 0) {
                                //fix jmpIfNot
                                int jmpTarget = codes.Count;
                                int offset = jmpTarget - jmpIfNot;
                                int opcode = codes[jmpIfNot];
                                codes[jmpIfNot] = opcode | (offset << 8);
                            }
                            var f = stmData.GetFunction(i);
                            string fid = f.GetId();
                            if (fid == "else") {
                                //block
                                PushBlock(true, false);
                                var callData = f.AsFunction;
                                if (null != callData) {
                                    foreach (var p in callData.Params) {
                                        CompileSyntaxInHook(p, codes, err);
                                    }
                                }
                                else {
                                    ++i;
                                    if (i < stmData.GetFunctionNum()) {
                                        var p = stmData.GetFunction(i);
                                        CompileSyntaxInHook(p, codes, err);
                                    }
                                    else {
                                        err.AppendFormat("Illegal if, expect if(exp) func_call/if(exp) func_call elif(exp) func_call else func_call/if(exp){...}elif(exp){...}else{...}, code:{0}, line:{1}", f.ToScriptString(false), f.GetLine());
                                        err.AppendLine();
                                    }
                                }
                                PopBlock();
                                break;
                            }
                            else if (fid == "if" || fid == "elif" || fid == "elseif") {
                                var callData = f.AsFunction;
                                if (null != callData) {
                                    Dsl.ISyntaxComponent exp;
                                    if (callData.IsHighOrder) {
                                        exp = callData.LowerOrderFunction.GetParam(0);
                                    }
                                    else {
                                        exp = callData.GetParam(0);
                                    }
                                    var info = new SemanticInfo { TargetType = "int" };
                                    CompileExpression(exp, codes, err, ref info);
                                    CurBlock().ResetTempVars();
                                    //gen jmpifnot
                                    jmpIfNot = codes.Count;
                                    codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                                    if (null == info.ResultValues) {
                                        codes.Add(EncodeOperand1(info.IsGlobal, info.IsConstOrTemp, info.ResultType, info.ResultIndex));
                                    }
                                    else {
                                        int rindex = AddConst(info.ResultType, info.ResultValues[0]);
                                        codes.Add(EncodeConstOperand1(info.ResultType, rindex));
                                    }
                                    //block
                                    PushBlock(true, false);
                                    if (callData.IsHighOrder) {
                                        foreach (var p in callData.Params) {
                                            CompileSyntaxInHook(p, codes, err);
                                        }
                                    }
                                    else {
                                        ++i;
                                        if (i < stmData.GetFunctionNum()) {
                                            var p = stmData.GetFunction(i);
                                            CompileSyntaxInHook(p, codes, err);
                                        }
                                        else {
                                            err.AppendFormat("Illegal if, expect if(exp) func_call/if(exp) func_call elif(exp) func_call else func_call/if(exp){...}elif(exp){...}else{...}, code:{0}, line:{1}", f.ToScriptString(false), f.GetLine());
                                            err.AppendLine();
                                        }
                                    }
                                    if (i < stmData.GetFunctionNum() - 1) {
                                        //gen jmp
                                        int jmp = codes.Count;
                                        codes.Add(EncodeOpcode(InsEnum.JMP));
                                        exitFixes.Add(jmp);
                                    }
                                    PopBlock();
                                }
                                else {
                                    err.AppendFormat("Illegal if, expect if(exp) func_call/if(exp) func_call elif(exp) func_call else func_call/if(exp){...}elif(exp){...}else{...}, code:{0}, line:{1}", f.ToScriptString(false), f.GetLine());
                                    err.AppendLine();
                                }
                            }
                            else {
                                err.AppendFormat("Illegal if, expect if(exp) func_call/if(exp) func_call elif(exp) func_call else func_call/if(exp){...}elif(exp){...}else{...}, code:{0}, line:{1}", f.ToScriptString(false), f.GetLine());
                                err.AppendLine();
                            }
                        }
                        //fix jmp offset
                        int exitTarget = codes.Count;
                        foreach (var jmp in exitFixes) {
                            int offset = exitTarget - jmp;
                            int opcode = codes[jmp];
                            codes[jmp] = opcode | (offset << 8);
                        }
                    }
                    else {
                        err.AppendFormat("Illegal if, expect if(exp) func_call/if(exp) func_call elif(exp) func_call else func_call/if(exp){...}elif(exp){...}else{...}, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else if (id == "while") {
                    //while(exp) func_call;
                    if (stmData.GetFunctionNum() == 2) {
                        var head = stmData.First.AsFunction;
                        var funcCall = stmData.Second;
                        if (null != head) {
                            var exp = head.GetParam(0);
                            var info = new SemanticInfo { TargetType = "int" };
                            int loopContinue = codes.Count;
                            CompileExpression(exp, codes, err, ref info);
                            CurBlock().ResetTempVars();
                            //gen jmpifnot
                            int jmpIfNot = codes.Count;
                            codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                            if (null == info.ResultValues) {
                                codes.Add(EncodeOperand1(info.IsGlobal, info.IsConstOrTemp, info.ResultType, info.ResultIndex));
                            }
                            else {
                                int rindex = AddConst(info.ResultType, info.ResultValues[0]);
                                codes.Add(EncodeConstOperand1(info.ResultType, rindex));
                            }
                            //block
                            PushBlock(false, true);
                            var breakFixes = new List<int>();
                            breakFixes.Add(jmpIfNot);
                            var lexicalInfo = CurBlock();
                            lexicalInfo.LoopContinue = loopContinue;
                            lexicalInfo.LoopBreakFixes = breakFixes;
                            CompileSyntaxInHook(funcCall, codes, err);
                            PopBlock();
                            //gen jmp
                            int jmp = codes.Count;
                            codes.Add(EncodeOpcode(InsEnum.JMP, loopContinue - jmp));
                            //fix jmp offset
                            int jmpTarget = codes.Count;
                            foreach (int fix in breakFixes) {
                                int offset = jmpTarget - fix;
                                int opcode = codes[fix];
                                codes[fix] = opcode | EncodeOffset(offset);
                            }
                        }
                        else {
                            err.AppendFormat("Illegal while, expect while(exp) func_call, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                    }
                    else {
                        err.AppendFormat("Illegal while, expect while(exp) func_call, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else if (id == "loop" || id == "loopi" || id == "loopd") {
                    //loop(var,begin,end[,inc]) func_call;
                    if (stmData.GetFunctionNum() == 2) {
                        var head = stmData.First.AsFunction;
                        var funcCall = stmData.Second;
                        if (null != head) {
                            int paramNum = head.GetParamNum();
                            if (paramNum == 3 || paramNum == 4) {
                                //block
                                PushBlock(false, true);
                                var lexicalInfo = CurBlock();
                                var v = head.GetParam(0);
                                var beginExp = head.GetParam(1);
                                var endExp = head.GetParam(2);
                                Dsl.ISyntaxComponent incExp = null;
                                bool incOne = paramNum == 3;
                                if (paramNum == 4) {
                                    incExp = head.GetParam(3);
                                }
                                var info1 = new SemanticInfo { TargetType = "int" };
                                CompileExpression(v, codes, err, ref info1);
                                var info2 = new SemanticInfo { TargetType = "int" };
                                CompileExpression(beginExp, codes, err, ref info2);
                                //gen assignment
                                codes.Add(EncodeOpcode(InsEnum.MOV, info1.IsGlobal, info1.IsConstOrTemp, info1.ResultType, info1.ResultIndex));
                                if (null == info2.ResultValues) {
                                    codes.Add(EncodeOperand1(info2.IsGlobal, info2.IsConstOrTemp, info2.ResultType, info2.ResultIndex));
                                }
                                else {
                                    int rindex = AddConst(info2.ResultType, info2.ResultValues[0]);
                                    codes.Add(EncodeConstOperand1(info2.ResultType, rindex));
                                }
                                int loopContinue = codes.Count;
                                var info3 = new SemanticInfo { TargetType = "int" };
                                CompileExpression(endExp, codes, err, ref info3);
                                //gen less equal than/great equal than
                                int tempIx = lexicalInfo.AllocTempInt();
                                if (id == "loopd")
                                    codes.Add(EncodeOpcode(InsEnum.GE, false, true, "int", tempIx));
                                else
                                    codes.Add(EncodeOpcode(InsEnum.LE, false, true, "int", tempIx));
                                int opd1 = EncodeOperand1(info1.IsGlobal, info1.IsConstOrTemp, info1.ResultType, info1.ResultIndex);
                                int opd2;
                                if (null == info3.ResultValues) {
                                    opd2 = EncodeOperand2(info3.IsGlobal, info3.IsConstOrTemp, info3.ResultType, info3.ResultIndex);
                                }
                                else {
                                    int rindex = AddConst(info3.ResultType, info3.ResultValues[0]);
                                    opd2 = EncodeConstOperand2(info3.ResultType, rindex);
                                }
                                codes.Add(opd1 | opd2);
                                //gen jmpifnot
                                int jmpIfNot = codes.Count;
                                codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                                codes.Add(EncodeOperand1(false, true, "int", tempIx));

                                lexicalInfo.ResetTempVars();

                                var breakFixes = new List<int>();
                                breakFixes.Add(jmpIfNot);
                                lexicalInfo.LoopContinue = loopContinue;
                                lexicalInfo.LoopBreakFixes = breakFixes;
                                CompileSyntaxInHook(funcCall, codes, err);

                                //gen inc/incv
                                if (incOne) {
                                    codes.Add(EncodeOpcode(InsEnum.INC, info1.IsGlobal, info1.IsConstOrTemp, info1.ResultType, info1.ResultIndex));
                                }
                                else {
                                    var info4 = new SemanticInfo { TargetType = "int" };
                                    CompileExpression(incExp, codes, err, ref info4);
                                    lexicalInfo.ResetTempVars();

                                    if (null == info4.ResultValues) {
                                        codes.Add(EncodeOpcode(InsEnum.INCV, info1.IsGlobal, info2.IsConstOrTemp, info1.ResultType, info1.ResultIndex));
                                        codes.Add(EncodeOperand1(info4.IsGlobal, info4.IsConstOrTemp, info4.ResultType, info4.ResultIndex));
                                    }
                                    else {
                                        if (info4.ResultValues[0] == "1") {
                                            codes.Add(EncodeOpcode(InsEnum.INC, info1.IsGlobal, info1.IsConstOrTemp, info1.ResultType, info1.ResultIndex));
                                        }
                                        else {
                                            codes.Add(EncodeOpcode(InsEnum.INCV, info1.IsGlobal, info1.IsConstOrTemp, info1.ResultType, info1.ResultIndex));
                                            int rindex = AddConst(info4.ResultType, info4.ResultValues[0]);
                                            codes.Add(EncodeConstOperand1(info4.ResultType, rindex));
                                        }
                                    }
                                }

                                PopBlock();
                                //gen jmp
                                int jmp = codes.Count;
                                codes.Add(EncodeOpcode(InsEnum.JMP, loopContinue - jmp));
                                //fix jmp offset
                                int jmpTarget = codes.Count;
                                foreach (int fix in breakFixes) {
                                    int offset = jmpTarget - fix;
                                    int opcode = codes[fix];
                                    codes[fix] = opcode | EncodeOffset(offset);
                                }
                            }
                            else {
                                err.AppendFormat("Illegal loop, expect loop(var,begin,end[,inc]) func_call, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                err.AppendLine();
                            }
                        }
                        else {
                            err.AppendFormat("Illegal loop, expect loop(var,begin,end[,inc]) func_call, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                    }
                    else {
                        err.AppendFormat("Illegal loop, expect loop(var,begin,end[,inc]) func_call, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else {
                    err.AppendFormat("Unknown syntax in hook, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                }
            }
            else {
                var callData = comp as Dsl.FunctionData;
                if (null != callData) {
                    string id = callData.GetId();
                    if (id == "=") {//assignment
                        var p1 = callData.GetParam(0);
                        var v1 = p1 as Dsl.ValueData;
                        if (null != v1) {
                            //var = val
                            string name = v1.GetId();
                            var vinfo = GetVarInfo(name);
                            if (null == vinfo) {
                                err.AppendFormat("Illegal operator=, use undefined variable, code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                                err.AppendLine();
                            }
                            else {
                                var v2 = callData.GetParam(1);
                                SemanticInfo info = new SemanticInfo { TargetOperation = TargetOperationEnum.VarAssign, TargetType = vinfo.Type, TargetCount = vinfo.Count, TargetName = vinfo.Name };
                                CompileExpression(v2, codes, err, ref info);
                                CurBlock().ResetTempVars();
                            }
                        }
                        else {
                            var param1 = p1 as Dsl.FunctionData;
                            if (null != param1 && param1.IsBracketParamClass()) {
                                //arr[ix] = val
                                var param2 = callData.GetParam(1);
                                string name = param1.GetId();
                                var ixExp = param1.GetParam(0);
                                var vinfo = GetVarInfo(name);
                                if (null == vinfo) {
                                    err.AppendFormat("Illegal operator=, use undefined variable, code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                                    err.AppendLine();
                                }
                                else {
                                    var info1 = new SemanticInfo { TargetType = "int" };
                                    CompileExpression(ixExp, codes, err, ref info1);
                                    var info2 = new SemanticInfo { TargetType = vinfo.Type, TargetCount = vinfo.Count, TargetName = vinfo.Name };
                                    CompileExpression(param2, codes, err, ref info2);
                                    CurBlock().ResetTempVars();

                                    TryGenArrSet(vinfo, codes, info1, info2, err, callData);
                                }
                            }
                            else if (null != param1 && param1.GetId() == ":") {
                                //var : type = val
                                var param2 = callData.GetParam(1);
                                string name = param1.GetParamId(0);
                                string type = ParseType(param1.GetParam(1), err, out var count);
                                if (IsLocalVar(name)) {
                                    var vinfo = new VarInfo { Name = name, Type = type, Count = count };
                                    AddVar(vinfo);

                                    var info = new SemanticInfo { TargetOperation = TargetOperationEnum.VarAssign, TargetType = type, TargetCount = count, TargetName = name };
                                    CompileExpression(param2, codes, err, ref info);
                                    CurBlock().ResetTempVars();
                                }
                                else {
                                    err.AppendFormat("Illegal operator=, local variable must start with '$', code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                                    err.AppendLine();
                                }
                            }
                            else {
                                err.AppendFormat("Illegal operator=, left operand must be 'name : type', code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                                err.AppendLine();
                            }
                        }
                    }
                    else if (id == ":") {
                        // var : type
                        string name = callData.GetParamId(0);
                        string type = ParseType(callData.GetParam(1), err, out var count);
                        if (IsLocalVar(name)) {
                            var vinfo = new VarInfo { Name = name, Type = type, Count = count, InitValues = null };
                            AddVar(vinfo);
                        }
                        else {
                            err.AppendFormat("Illegal operator=, local variable must start with '$', code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                            err.AppendLine();
                        }
                    }
                    else if (id == "if") {
                        //if(exp){statements;};
                        if (callData.IsHighOrder) {
                            var head = callData.LowerOrderFunction;
                            var exp = head.GetParam(0);
                            var info = new SemanticInfo { TargetType = "int" };
                            CompileExpression(exp, codes, err, ref info);
                            CurBlock().ResetTempVars();
                            //gen jmpifnot
                            int jmpIfNot = codes.Count;
                            codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                            if (null == info.ResultValues) {
                                codes.Add(EncodeOperand1(info.IsGlobal, info.IsConstOrTemp, info.ResultType, info.ResultIndex));
                            }
                            else {
                                int rindex = AddConst(info.ResultType, info.ResultValues[0]);
                                codes.Add(EncodeConstOperand1(info.ResultType, rindex));
                            }
                            //block
                            PushBlock(true, false);
                            foreach (var p in callData.Params) {
                                CompileSyntaxInHook(p, codes, err);
                            }
                            PopBlock();
                            //fix jmp offset
                            int jmpTarget = codes.Count;
                            int offset = jmpTarget - jmpIfNot;
                            int opcode = codes[jmpIfNot];
                            codes[jmpIfNot] = opcode | (offset << 8);
                        }
                        else {
                            err.AppendFormat("Illegal if, if must have statements, code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                            err.AppendLine();
                        }
                    }
                    else if (id == "while") {
                        //while(exp){statements;};
                        if (callData.IsHighOrder) {
                            var head = callData.LowerOrderFunction;
                            var exp = head.GetParam(0);
                            var info = new SemanticInfo { TargetType = "int" };
                            int loopContinue = codes.Count;
                            CompileExpression(exp, codes, err, ref info);
                            CurBlock().ResetTempVars();
                            //gen jmpifnot
                            int jmpIfNot = codes.Count;
                            codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                            if (null == info.ResultValues) {
                                codes.Add(EncodeOperand1(info.IsGlobal, info.IsConstOrTemp, info.ResultType, info.ResultIndex));
                            }
                            else {
                                int rindex = AddConst(info.ResultType, info.ResultValues[0]);
                                codes.Add(EncodeConstOperand1(info.ResultType, rindex));
                            }
                            //block
                            PushBlock(false, true);
                            var breakFixes = new List<int>();
                            breakFixes.Add(jmpIfNot);
                            var lexicalInfo = CurBlock();
                            lexicalInfo.LoopContinue = loopContinue;
                            lexicalInfo.LoopBreakFixes = breakFixes;
                            foreach (var p in callData.Params) {
                                CompileSyntaxInHook(p, codes, err);
                            }
                            PopBlock();
                            //gen jmp
                            int jmp = codes.Count;
                            codes.Add(EncodeOpcode(InsEnum.JMP, loopContinue - jmp));
                            //fix jmp offset
                            int jmpTarget = codes.Count;
                            foreach (int fix in breakFixes) {
                                int offset = jmpTarget - fix;
                                int opcode = codes[fix];
                                codes[fix] = opcode | EncodeOffset(offset);
                            }
                        }
                        else {
                            err.AppendFormat("Illegal while, while must have statements, code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                            err.AppendLine();
                        }
                    }
                    else if (id == "loop" || id == "loopi" || id == "loopd") {
                        //loop(var,begin,end[,inc]){statements;};
                        if (callData.IsHighOrder) {
                            var head = callData.LowerOrderFunction;
                            int paramNum = head.GetParamNum();
                            if (paramNum == 3 || paramNum == 4) {
                                //block
                                PushBlock(false, true);
                                var lexicalInfo = CurBlock();
                                var v = head.GetParam(0);
                                var beginExp = head.GetParam(1);
                                var endExp = head.GetParam(2);
                                Dsl.ISyntaxComponent incExp = null;
                                bool incOne = paramNum == 3;
                                if (paramNum == 4) {
                                    incExp = head.GetParam(3);
                                }
                                var info1 = new SemanticInfo { TargetType = "int" };
                                CompileExpression(v, codes, err, ref info1);
                                var info2 = new SemanticInfo { TargetType = "int" };
                                CompileExpression(beginExp, codes, err, ref info2);
                                //gen assignment
                                codes.Add(EncodeOpcode(InsEnum.MOV, info1.IsGlobal, info1.IsConstOrTemp, info1.ResultType, info1.ResultIndex));
                                if (null == info2.ResultValues) {
                                    codes.Add(EncodeOperand1(info2.IsGlobal, info2.IsConstOrTemp, info2.ResultType, info2.ResultIndex));
                                }
                                else {
                                    int rindex = AddConst(info2.ResultType, info2.ResultValues[0]);
                                    codes.Add(EncodeConstOperand1(info2.ResultType, rindex));
                                }
                                int loopContinue = codes.Count;
                                var info3 = new SemanticInfo { TargetType = "int" };
                                CompileExpression(endExp, codes, err, ref info3);
                                //gen less equal than/great equal than
                                int tempIx = lexicalInfo.AllocTempInt();
                                if (id == "loopd")
                                    codes.Add(EncodeOpcode(InsEnum.GE, false, true, "int", tempIx));
                                else
                                    codes.Add(EncodeOpcode(InsEnum.LE, false, true, "int", tempIx));
                                int opd1 = EncodeOperand1(info1.IsGlobal, info1.IsConstOrTemp, info1.ResultType, info1.ResultIndex);
                                int opd2;
                                if (null == info3.ResultValues) {
                                    opd2 = EncodeOperand2(info3.IsGlobal, info3.IsConstOrTemp, info3.ResultType, info3.ResultIndex);
                                }
                                else {
                                    int rindex = AddConst(info3.ResultType, info3.ResultValues[0]);
                                    opd2 = EncodeConstOperand2(info3.ResultType, rindex);
                                }
                                codes.Add(opd1 | opd2);
                                //gen jmpifnot
                                int jmpIfNot = codes.Count;
                                codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                                codes.Add(EncodeOperand1(false, true, "int", tempIx));

                                lexicalInfo.ResetTempVars();

                                var breakFixes = new List<int>();
                                breakFixes.Add(jmpIfNot);
                                lexicalInfo.LoopContinue = loopContinue;
                                lexicalInfo.LoopBreakFixes = breakFixes;
                                foreach (var p in callData.Params) {
                                    CompileSyntaxInHook(p, codes, err);
                                }

                                //gen inc/incv
                                if (incOne) {
                                    codes.Add(EncodeOpcode(InsEnum.INC, info1.IsGlobal, info1.IsConstOrTemp, info1.ResultType, info1.ResultIndex));
                                }
                                else {
                                    var info4 = new SemanticInfo { TargetType = "int" };
                                    CompileExpression(incExp, codes, err, ref info4);
                                    lexicalInfo.ResetTempVars();

                                    if (null == info4.ResultValues) {
                                        codes.Add(EncodeOpcode(InsEnum.INCV, info1.IsGlobal, info1.IsConstOrTemp, info1.ResultType, info1.ResultIndex));
                                        codes.Add(EncodeOperand1(info4.IsGlobal, info4.IsConstOrTemp, info4.ResultType, info4.ResultIndex));
                                    }
                                    else {
                                        if (info4.ResultValues[0] == "1") {
                                            codes.Add(EncodeOpcode(InsEnum.INC, info1.IsGlobal, info1.IsConstOrTemp, info1.ResultType, info1.ResultIndex));
                                        }
                                        else {
                                            codes.Add(EncodeOpcode(InsEnum.INCV, info1.IsGlobal, info1.IsConstOrTemp, info1.ResultType, info1.ResultIndex));
                                            int rindex = AddConst(info4.ResultType, info4.ResultValues[0]);
                                            codes.Add(EncodeConstOperand1(info4.ResultType, rindex));
                                        }
                                    }
                                }

                                PopBlock();
                                //gen jmp
                                int jmp = codes.Count;
                                codes.Add(EncodeOpcode(InsEnum.JMP, loopContinue - jmp));
                                //fix jmp offset
                                int jmpTarget = codes.Count;
                                foreach (int fix in breakFixes) {
                                    int offset = jmpTarget - fix;
                                    int opcode = codes[fix];
                                    codes[fix] = opcode | EncodeOffset(offset);
                                }
                            }
                            else {
                                err.AppendFormat("Illegal loop, expect loop(var,begin,end[,inc]){...}, code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                                err.AppendLine();
                            }
                        }
                        else {
                            err.AppendFormat("Illegal loop, loop must have statements, code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                            err.AppendLine();
                        }
                    }
                    else if (id == "return") {
                        //return(exp);
                        if (callData.GetParamNum() == 1) {
                            var exp = callData.GetParam(0);
                            var info = new SemanticInfo { TargetType = "int" };
                            CompileExpression(exp, codes, err, ref info);
                            CurBlock().ResetTempVars();
                            //gen ret
                            if (null == info.ResultValues) {
                                codes.Add(EncodeOpcode(InsEnum.RET, info.IsGlobal, info.IsConstOrTemp, info.ResultType, info.ResultIndex));
                            }
                            else {
                                int rindex = AddConst(info.ResultType, info.ResultValues[0]);
                                codes.Add(EncodeOpcode(InsEnum.RET, true, true, info.ResultType, rindex));
                            }
                        }
                        else {
                            err.AppendFormat("Illegal return, expect return(exp), code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                            err.AppendLine();
                        }
                    }
                    else if (id == "<-") {
                        //return exp;
                        if (callData.GetParamNum() == 2) {
                            var exp = callData.GetParam(1);
                            var info = new SemanticInfo { TargetType = "int" };
                            CompileExpression(exp, codes, err, ref info);
                            CurBlock().ResetTempVars();
                            //gen ret
                            if (null == info.ResultValues) {
                                codes.Add(EncodeOpcode(InsEnum.RET, info.IsGlobal, info.IsConstOrTemp, info.ResultType, info.ResultIndex));
                            }
                            else {
                                int rindex = AddConst(info.ResultType, info.ResultValues[0]);
                                codes.Add(EncodeOpcode(InsEnum.RET, true, true, info.ResultType, rindex));
                            }
                        }
                        else {
                            err.AppendFormat("Illegal return, expect return exp, code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                            err.AppendLine();
                        }
                    }
                    else if (callData.IsParenthesisParamClass()) {
                        var info = new SemanticInfo { };
                        CompileExpression(callData, codes, err, ref info);
                        CurBlock().ResetTempVars();
                    }
                    else {
                        err.AppendFormat("Unknown syntax in hook, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else {
                    var valData = comp as Dsl.ValueData;
                    if (null != valData) {
                        string id = valData.GetId();
                        if (id == "continue") {
                            int loopContinue = CurLoopContinuePoint();
                            if (loopContinue >= 0) {
                                int curpos = codes.Count;
                                codes.Add(EncodeOpcode(InsEnum.JMP, loopContinue - curpos));
                            }
                            else {
                                err.AppendFormat("Illegal continue, must be in a loop, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                err.AppendLine();
                            }
                        }
                        else if (id == "break") {
                            var lexicalInfo = CurLoopBlock();
                            if (null != lexicalInfo) {
                                int curpos = codes.Count;
                                codes.Add(EncodeOpcode(InsEnum.JMP));
                                lexicalInfo.LoopBreakFixes.Add(curpos);
                            }
                            else {
                                err.AppendFormat("Illegal break, must be in a loop, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                err.AppendLine();
                            }
                        }
                        else if (id == "return") {
                            err.AppendFormat("Illegal return, must return integer in hook, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        else {
                            err.AppendFormat("Unknown syntax in hook, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                    }
                    else {
                        err.AppendFormat("Unknown syntax in hook, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
            }
        }
        private void CompileExpression(Dsl.ISyntaxComponent comp, List<int> codes, StringBuilder err, ref SemanticInfo semanticInfo)
        {
            var callData = comp as Dsl.FunctionData;
            if (null != callData) {
                if (!callData.HaveId()) {
                    int paramNum = callData.GetParamNum();
                    if (paramNum == 1 && callData.IsParenthesisParamClass()) {
                        Dsl.ISyntaxComponent param = callData.GetParam(0);
                        CompileExpression(param, codes, err, ref semanticInfo);
                    }
                    else if (paramNum >= 1 && callData.IsBracketParamClass()) {
                        if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                            if (IsGlobalBlock()) {
                                semanticInfo.ResultValues = new List<string>();
                                for (int i = 0; i < callData.GetParamNum() && i < semanticInfo.TargetCount; ++i) {
                                    var p = callData.GetParam(i);
                                    var sinfo = new SemanticInfo { TargetType = semanticInfo.TargetType };
                                    CompileExpression(p, codes, err, ref sinfo);
                                    if (null == sinfo.ResultValues) {
                                        err.AppendFormat("Illegal global init syntax, [%d] must be const value, code:{0}, line:{1}", i, comp.ToScriptString(false), comp.GetLine());
                                        err.AppendLine();
                                    }
                                    else {
                                        semanticInfo.ResultValues.Add(sinfo.ResultValues[0]);
                                    }
                                }
                            }
                            else {
                                var vinfo = GetVarInfo(semanticInfo.TargetName);
                                for (int i = 0; i < callData.GetParamNum() && i < semanticInfo.TargetCount; ++i) {
                                    var p = callData.GetParam(i);
                                    var sinfo = new SemanticInfo { TargetType = semanticInfo.TargetType };
                                    CompileExpression(p, codes, err, ref sinfo);
                                    TryGenArrMov(vinfo.IsGlobal, false, vinfo.Type, vinfo.Index + i, codes, sinfo, err, p);
                                }
                                semanticInfo.IsGlobal = vinfo.IsGlobal;
                                semanticInfo.IsConstOrTemp = false;
                                semanticInfo.ResultType = vinfo.Type;
                                semanticInfo.ResultCount = vinfo.Count;
                                semanticInfo.ResultIndex = vinfo.Index;
                                semanticInfo.ResultValues = null;
                            }
                        }
                        else {
                            int tmpIndex = CurBlock().AllocTempIntArray(callData.GetParamNum());
                            string type = string.Empty;
                            for (int i = 0; i < callData.GetParamNum(); ++i) {
                                var p = callData.GetParam(i);
                                var sinfo = new SemanticInfo { TargetType = semanticInfo.TargetType };
                                CompileExpression(p, codes, err, ref sinfo);
                                TryGenArrMov(false, true, "int", tmpIndex + i, codes, sinfo, err, p);
                            }
                            semanticInfo.IsGlobal = false;
                            semanticInfo.IsConstOrTemp = true;
                            semanticInfo.ResultType = type;
                            semanticInfo.ResultCount = semanticInfo.TargetCount;
                            semanticInfo.ResultIndex = tmpIndex;
                            semanticInfo.ResultValues = null;
                        }
                    }
                    else {
                        err.AppendFormat("Unknown expression syntax, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else {
                    string op = callData.GetId();
                    int num = callData.GetParamNum();
                    var sinfos = new List<SemanticInfo>();
                    for (int i = 0; i < num; ++i) {
                        Dsl.ISyntaxComponent param = callData.GetParam(i);
                        var sinfo = new SemanticInfo();
                        CompileExpression(param, codes, err, ref sinfo);
                        sinfos.Add(sinfo);
                    }
                    if (callData.GetParamClass() == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_OPERATOR) {
                        if (num == 2 && op != "!" && op != "~" || num == 1 && (op == "+" || op == "-" || op == "!" || op == "~")) {
                            if (op == "+") {
                                if (num == 2) {
                                    TryGenExpression(InsEnum.ADD, codes, sinfos, err, callData, ref semanticInfo);
                                }
                            }
                            else if (op == "-") {
                                if (num == 2) {
                                    TryGenExpression(InsEnum.SUB, codes, sinfos, err, callData, ref semanticInfo);
                                }
                                else {
                                    TryGenExpression(InsEnum.NEG, codes, sinfos, err, callData, ref semanticInfo);
                                }
                            }
                            else if (op == "*") {
                                TryGenExpression(InsEnum.MUL, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "/") {
                                TryGenExpression(InsEnum.DIV, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "%") {
                                TryGenExpression(InsEnum.MOD, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "&&") {
                                TryGenExpression(InsEnum.AND, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "||") {
                                TryGenExpression(InsEnum.OR, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "!") {
                                TryGenExpression(InsEnum.NOT, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == ">") {
                                TryGenExpression(InsEnum.GT, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == ">=") {
                                TryGenExpression(InsEnum.GE, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "==") {
                                TryGenExpression(InsEnum.EQ, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "!=") {
                                TryGenExpression(InsEnum.NE, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "<=") {
                                TryGenExpression(InsEnum.LE, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "<") {
                                TryGenExpression(InsEnum.LT, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "<<") {
                                TryGenExpression(InsEnum.LSHIFT, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == ">>") {
                                TryGenExpression(InsEnum.RSHIFT, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == ">>>") {
                                TryGenExpression(InsEnum.URSHIFT, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "&") {
                                TryGenExpression(InsEnum.BITAND, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "|") {
                                TryGenExpression(InsEnum.BITOR, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "^") {
                                TryGenExpression(InsEnum.BITXOR, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "~") {
                                TryGenExpression(InsEnum.BITNOT, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else {
                                err.AppendFormat("operator '{0}' illegal, code:{1}, line:{2}", op, callData.ToScriptString(false), callData.GetLine());
                                err.AppendLine();
                            }
                        }
                        else {
                            err.AppendFormat("operator '{0}' arg num {1} illegal, code:{2}, line:{3}", op, num, callData.ToScriptString(false), callData.GetLine());
                            err.AppendLine();
                        }
                    }
                    else if (callData.IsBracketParamClass() && num == 1) {
                        TryGenArrGet(op, codes, sinfos, err, callData, ref semanticInfo);
                    }
                    else if (callData.IsParenthesisParamClass()) {
                        if (op == "argc") {
                            TryGenArgc(codes, sinfos, err, callData, ref semanticInfo);
                        }
                        else if (op == "argv") {
                            TryGenArgv(codes, sinfos, err, callData, ref semanticInfo);
                        }
                        else if (op == "addr") {
                            TryGenAddr(codes, sinfos, err, callData, ref semanticInfo);
                        }
                        else if (op == "ptrget") {
                            TryGenPtrGet(codes, sinfos, err, callData, ref semanticInfo);
                        }
                        else if (op == "ptrset") {
                            TryGenPtrSet(codes, sinfos, err, callData, ref semanticInfo);
                        }
                        else if (op == "jptr") {
                            TryGenJaggedPtr(codes, sinfos, err, callData, ref semanticInfo);
                        }
                        else {
                            ApiInfo info;
                            if (!m_Apis.TryGetValue(op, out info)) {
                                err.AppendFormat("Undefined api '{0}', code:{1}, line:{2}", op, callData.ToScriptString(false), callData.GetLine());
                                err.AppendLine();
                            }
                            else {
                                TryGenCallApi(info, codes, sinfos, err, callData, ref semanticInfo);
                            }
                        }
                    }
                    else {
                        err.AppendFormat("Unknown expression syntax, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
            }
            else {
                Dsl.ValueData valueData = comp as Dsl.ValueData;
                if (null != valueData) {
                    string val = valueData.GetId();
                    switch (valueData.GetIdType()) {
                        case Dsl.ValueData.NUM_TOKEN:
                            if (val.Contains('.'))
                                semanticInfo.ResultType = "float";
                            else
                                semanticInfo.ResultType = "int";
                            semanticInfo.ResultValues = new List<string>();
                            semanticInfo.ResultValues.Add(val);
                            TryGenConst(val, codes, err, comp, ref semanticInfo);
                            break;
                        case Dsl.ValueData.STRING_TOKEN:
                            semanticInfo.ResultType = "string";
                            semanticInfo.ResultValues = new List<string>();
                            semanticInfo.ResultValues.Add(val);
                            TryGenConst(val, codes, err, comp, ref semanticInfo);
                            break;
                        case Dsl.ValueData.ID_TOKEN:
                            TryGenVar(val, codes, err, comp, ref semanticInfo);
                            break;
                    }
                }
                else {
                    err.AppendFormat("Unknown expression syntax, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                }
            }
        }

        private StructInfo ParseStruct(Dsl.FunctionData callData, StringBuilder err)
        {
            if (callData.IsHighOrder) {
                var head = callData.LowerOrderFunction;
                Debug.Assert(null != head);
                string name = head.GetParamId(0);
                var struInfo = new StructInfo { Name = name };
                if (callData.HaveStatement()) {
                    foreach (var p in callData.Params) {
                        var fieldData = p as Dsl.FunctionData;
                        if (null != fieldData) {
                            var fname = fieldData.GetParamId(0);
                            var arrOrPtrs = new List<int>();
                            var ftype = ParseFieldType(fieldData.GetParam(1), err, arrOrPtrs);
                            var finfo = new FieldInfo { Name = fname, Type = ftype, ArrayOrPtrs = arrOrPtrs };
                            struInfo.Fields.Add(finfo);
                        }
                        else {
                            err.AppendFormat("Struct syntax error, field must be 'name : type', code:{0}, line:{1}", p.ToScriptString(false), p.GetLine());
                            err.AppendLine();
                        }
                    }
                }
                else {
                    err.AppendFormat("Struct syntax error, struct definition must include fields, code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                    err.AppendLine();
                }
                return struInfo;
            }
            return null;
        }
        private string ParseFieldType(Dsl.ISyntaxComponent comp, StringBuilder err, List<int> arrOrPtrs)
        {
            var valData = comp as Dsl.ValueData;
            if (null != valData) {
                return valData.GetId();
            }
            else {
                var funcData = comp as Dsl.FunctionData;
                if (null != funcData && funcData.IsParenthesisParamClass() && funcData.GetParamNum() == 1 && !funcData.IsHighOrder && funcData.GetId() == "ptr") {
                    arrOrPtrs.Add(0);
                    return ParseFieldType(funcData.GetParam(0), err, arrOrPtrs);
                }
                else if (null != funcData && funcData.IsBracketParamClass() && funcData.GetParamNum() == 1) {
                    int.TryParse(funcData.GetParamId(0), out var count);
                    arrOrPtrs.Add(count);
                    if(funcData.IsHighOrder)
                        return ParseFieldType(funcData.LowerOrderFunction, err, arrOrPtrs);
                    else
                        return funcData.GetId();
                }
                else {
                    err.AppendFormat("Unknown syntax in type, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                }
            }
            return string.Empty;
        }
        private string ParseType(Dsl.ISyntaxComponent comp, StringBuilder err, out int count)
        {
            count = 0;
            var valData = comp as Dsl.ValueData;
            if (null != valData) {
                return valData.GetId();
            }
            else {
                var funcData = comp as Dsl.FunctionData;
                if (null != funcData && funcData.IsBracketParamClass() && funcData.GetParamNum() == 1) {
                    int.TryParse(funcData.GetParamId(0), out count);
                    return funcData.GetId();
                }
                else {
                    err.AppendFormat("Unknown syntax in type, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                }
            }
            return string.Empty;
        }

        private void TryGenConst(string val, List<int> codes, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    var vinfo = GetVarInfo(semanticInfo.TargetName);
                    if (vinfo.Count > 0) {
                        err.AppendFormat("Can't assignment a value to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.MOV, vinfo.IsGlobal, false, vinfo.Type, vinfo.Index));
                    int rindex = AddConst(semanticInfo.ResultType, val);
                    int opd = EncodeConstOperand1(semanticInfo.ResultType, rindex);
                    codes.Add(opd);

                    semanticInfo.IsGlobal = vinfo.IsGlobal;
                    semanticInfo.IsConstOrTemp = false;
                    semanticInfo.ResultType = vinfo.Type;
                    semanticInfo.ResultCount = vinfo.Count;
                    semanticInfo.ResultIndex = vinfo.Index;
                    semanticInfo.ResultValues = null;
                }
            }
        }
        private void TryGenVar(string id, List<int> codes, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    var vinfo = GetVarInfo(semanticInfo.TargetName);
                    var vinfo2 = GetVarInfo(id);
                    if (null == vinfo) {
                        err.AppendFormat("Undefined var '{0}', code:{1}, line:{2}", semanticInfo.TargetName, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                        return;
                    }
                    if (null == vinfo2) {
                        err.AppendFormat("Undefined var '{0}', code:{1}, line:{2}", id, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                        return;
                    }
                    if (vinfo.Count > 0 || vinfo2.Count > 0) {
                        if (vinfo.Count != vinfo2.Count) {
                            err.AppendFormat("Can't assignment array with different size, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        for (int i = 0; i < vinfo.Count && i < vinfo2.Count; ++i) {
                            //gen write result
                            codes.Add(EncodeOpcode(InsEnum.MOV, vinfo.IsGlobal, false, vinfo.Type, vinfo.Index + i));
                            int opd = EncodeOperand1(vinfo2.IsGlobal, false, vinfo2.Type, vinfo2.Index + i);
                            codes.Add(opd);
                        }
                    }
                    else {
                        //gen write result
                        codes.Add(EncodeOpcode(InsEnum.MOV, vinfo.IsGlobal, false, vinfo.Type, vinfo.Index));
                        int opd = EncodeOperand1(vinfo2.IsGlobal, false, vinfo2.Type, vinfo2.Index);
                        codes.Add(opd);
                    }

                    semanticInfo.IsGlobal = vinfo.IsGlobal;
                    semanticInfo.IsConstOrTemp = false;
                    semanticInfo.ResultType = vinfo.Type;
                    semanticInfo.ResultCount = vinfo.Count;
                    semanticInfo.ResultIndex = vinfo.Index;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                var vinfo = GetVarInfo(id);
                if (null == vinfo) {
                    err.AppendFormat("Undefined var '{0}', code:{1}, line:{2}", id, comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                    return;
                }

                semanticInfo.IsGlobal = vinfo.IsGlobal;
                semanticInfo.IsConstOrTemp = false;
                semanticInfo.ResultType = vinfo.Type;
                semanticInfo.ResultCount = vinfo.Count;
                semanticInfo.ResultIndex = vinfo.Index;
                semanticInfo.ResultValues = null;
            }
        }
        private void TryGenArrMov(bool isGlobal, bool isConstOrTemp, string type, int vindex, List<int> codes, SemanticInfo info, StringBuilder err, Dsl.ISyntaxComponent comp)
        {
            //gen mov
            int opd1;
            codes.Add(EncodeOpcode(InsEnum.MOV, isGlobal, isConstOrTemp, type, vindex));
            if (null == info.ResultValues) {
                opd1 = EncodeOperand1(info.IsGlobal, info.IsConstOrTemp, info.ResultType, info.ResultIndex);
            }
            else {
                int rindex = AddConst(info.ResultType, info.ResultValues[0]);
                opd1 = EncodeConstOperand1(info.ResultType, rindex);
            }
            codes.Add(opd1);
        }
        private void TryGenArrGet(string id, List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            Debug.Assert(opds.Count == 1);
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    var vinfo = GetVarInfo(semanticInfo.TargetName);
                    var vinfo2 = GetVarInfo(id);
                    if (null == vinfo) {
                        err.AppendFormat("Undefined var '{0}', code:{1}, line:{2}", semanticInfo.TargetName, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                        return;
                    }
                    if (null == vinfo2) {
                        err.AppendFormat("Undefined var '{0}', code:{1}, line:{2}", id, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                        return;
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.ARRGET, vinfo.IsGlobal, false, vinfo.Type, vinfo.Index));
                    int opd1 = EncodeOperand1(vinfo2.IsGlobal, false, vinfo2.Type, vinfo2.Index);
                    var opd2Info = opds[0];
                    int opd2;
                    if (null == opd2Info.ResultValues) {
                        opd2 = EncodeOperand2(opd2Info.IsGlobal, opd2Info.IsConstOrTemp, opd2Info.ResultType, opd2Info.ResultIndex);
                    }
                    else {
                        int rindex = AddConst(opd2Info.ResultType, opd2Info.ResultValues[0]);
                        opd2 = EncodeConstOperand2(opd2Info.ResultType, rindex);
                    }
                    codes.Add(opd1 | opd2);

                    semanticInfo.IsGlobal = vinfo.IsGlobal;
                    semanticInfo.IsConstOrTemp = false;
                    semanticInfo.ResultType = vinfo.Type;
                    semanticInfo.ResultCount = vinfo.Count;
                    semanticInfo.ResultIndex = vinfo.Index;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                var vinfo = GetVarInfo(id);
                if (null == vinfo) {
                    err.AppendFormat("Undefined var '{0}', code:{1}, line:{2}", id, comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                    return;
                }
                if (s_Type2Ids.TryGetValue(vinfo.Type, out var ty)) {
                    int tmpIndex = -1;
                    switch (ty) {
                        case TypeEnum.Int:
                            tmpIndex = CurBlock().AllocTempInt();
                            break;
                        case TypeEnum.Float:
                            tmpIndex = CurBlock().AllocTempFloat();
                            break;
                        case TypeEnum.String:
                            tmpIndex = CurBlock().AllocTempString();
                            break;
                    }
                    if (tmpIndex >= 0) {
                        semanticInfo.IsGlobal = false;
                        semanticInfo.IsConstOrTemp = true;
                        semanticInfo.ResultType = vinfo.Type;
                        semanticInfo.ResultCount = 0;
                        semanticInfo.ResultIndex = tmpIndex;
                        semanticInfo.ResultValues = null;
                        //gen write result
                        codes.Add(EncodeOpcode(InsEnum.ARRGET, semanticInfo.IsGlobal, semanticInfo.IsConstOrTemp, semanticInfo.ResultType, semanticInfo.ResultIndex));
                        int opd1 = EncodeOperand1(vinfo.IsGlobal, false, vinfo.Type, vinfo.Index);
                        var opd2Info = opds[0];
                        int opd2;
                        if (null == opd2Info.ResultValues) {
                            opd2 = EncodeOperand2(opd2Info.IsGlobal, opd2Info.IsConstOrTemp, opd2Info.ResultType, opd2Info.ResultIndex);
                        }
                        else {
                            int rindex = AddConst(opd2Info.ResultType, opd2Info.ResultValues[0]);
                            opd2 = EncodeConstOperand2(opd2Info.ResultType, rindex);
                        }
                        codes.Add(opd1 | opd2);
                    }
                }
            }
        }
        private void TryGenArrSet(VarInfo vinfo, List<int> codes, SemanticInfo info1, SemanticInfo info2, StringBuilder err, Dsl.ISyntaxComponent comp)
        {
            //gen arrset
            int opd1, opd2;
            codes.Add(EncodeOpcode(InsEnum.ARRSET, vinfo.IsGlobal, false, vinfo.Type, vinfo.Index));
            if (null == info1.ResultValues) {
                opd1 = EncodeOperand1(info1.IsGlobal, info1.IsConstOrTemp, info1.ResultType, info1.ResultIndex);
            }
            else {
                int rindex = AddConst(info1.ResultType, info1.ResultValues[0]);
                opd1 = EncodeConstOperand1(info1.ResultType, rindex);
            }
            if (null == info2.ResultValues) {
                opd2 = EncodeOperand2(info2.IsGlobal, info2.IsConstOrTemp, info2.ResultType, info2.ResultIndex);
            }
            else {
                int rindex = AddConst(info2.ResultType, info2.ResultValues[0]);
                opd2 = EncodeConstOperand2(info2.ResultType, rindex);
            }
            codes.Add(opd1 | opd2);
        }
        private void TryGenExpression(InsEnum op, List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            CheckType(op, opds, err, comp);
            semanticInfo.ResultType = DeduceType(op, opds);
            semanticInfo.ResultValues = TryCalcConst(op, opds);
            if (null != semanticInfo.ResultValues) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        var vinfo = GetVarInfo(semanticInfo.TargetName);
                        if (vinfo.Count > 0) {
                            err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        //gen write result
                        codes.Add(EncodeOpcode(InsEnum.MOV, vinfo.IsGlobal, false, vinfo.Type, vinfo.Index));
                        int rindex = AddConst(semanticInfo.ResultType, semanticInfo.ResultValues[0]);
                        int opd = EncodeConstOperand1(semanticInfo.ResultType, rindex);
                        codes.Add(opd);

                        semanticInfo.IsGlobal = vinfo.IsGlobal;
                        semanticInfo.IsConstOrTemp = false;
                        semanticInfo.ResultType = vinfo.Type;
                        semanticInfo.ResultCount = vinfo.Count;
                        semanticInfo.ResultIndex = vinfo.Index;
                    }
                }
            }
            else {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        var vinfo = GetVarInfo(semanticInfo.TargetName);
                        if (vinfo.Count > 0) {
                            err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        //gen write result
                        codes.Add(EncodeOpcode(op, vinfo.IsGlobal, false, vinfo.Type, vinfo.Index));
                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            if (null == opdInfo.ResultValues) {
                                opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd1 = EncodeConstOperand1(opdInfo.ResultType, rindex);
                            }
                        }
                        int opd2 = 0;
                        if (opds.Count > 1) {
                            var opdInfo = opds[1];
                            if (null == opdInfo.ResultValues) {
                                opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                            }
                        }
                        codes.Add(opd1 | opd2);

                        semanticInfo.IsGlobal = vinfo.IsGlobal;
                        semanticInfo.IsConstOrTemp = false;
                        semanticInfo.ResultType = vinfo.Type;
                        semanticInfo.ResultCount = vinfo.Count;
                        semanticInfo.ResultIndex = vinfo.Index;
                        semanticInfo.ResultValues = null;
                    }
                }
                else {
                    if (s_Type2Ids.TryGetValue(semanticInfo.ResultType, out var ty)) {
                        int tmpIndex = -1;
                        switch (ty) {
                            case TypeEnum.Int:
                                tmpIndex = CurBlock().AllocTempInt();
                                break;
                            case TypeEnum.Float:
                                tmpIndex = CurBlock().AllocTempFloat();
                                break;
                            case TypeEnum.String:
                                tmpIndex = CurBlock().AllocTempString();
                                break;
                        }
                        if (tmpIndex >= 0) {
                            semanticInfo.IsGlobal = false;
                            semanticInfo.IsConstOrTemp = true;
                            semanticInfo.ResultCount = 0;
                            semanticInfo.ResultIndex = tmpIndex;
                            semanticInfo.ResultValues = null;
                            codes.Add(EncodeOpcode(op, semanticInfo.IsGlobal, semanticInfo.IsConstOrTemp, semanticInfo.ResultType, semanticInfo.ResultIndex));

                            int opd1 = 0;
                            if (opds.Count > 0) {
                                var opdInfo = opds[0];
                                if (null == opdInfo.ResultValues) {
                                    opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                                }
                                else {
                                    int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                    opd1 = EncodeConstOperand1(opdInfo.ResultType, rindex);
                                }
                            }
                            int opd2 = 0;
                            if (opds.Count > 1) {
                                var opdInfo = opds[1];
                                if (null == opdInfo.ResultValues) {
                                    opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                                }
                                else {
                                    int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                    opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                                }
                            }
                            codes.Add(opd1 | opd2);
                        }
                    }
                }
            }
        }
        private void TryGenCallApi(ApiInfo api, List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            CheckType(api, opds, err, comp);

            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    var vinfo = GetVarInfo(semanticInfo.TargetName);
                    if (vinfo.Count > 0) {
                        err.AppendFormat("Can't assignment api result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen api call
                    codes.Add(EncodeOpcode(InsEnum.CALL, opds.Count, vinfo.IsGlobal, false, vinfo.Type, vinfo.Index));
                    int opd1 = 0;
                    int opd2 = 0;
                    opd1 = EncodeOperand1(api.ApiId);
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                        }
                    }
                    codes.Add(opd1 | opd2);
                    for (int i = 1; i < opds.Count; i += 2) {
                        opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            if (null == opdInfo.ResultValues) {
                                opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd1 = EncodeConstOperand1(opdInfo.ResultType, rindex);
                            }
                        }
                        opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            if (null == opdInfo.ResultValues) {
                                opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                            }
                        }
                        codes.Add(opd1 | opd2);
                    }

                    semanticInfo.IsGlobal = vinfo.IsGlobal;
                    semanticInfo.IsConstOrTemp = false;
                    semanticInfo.ResultType = vinfo.Type;
                    semanticInfo.ResultCount = vinfo.Count;
                    semanticInfo.ResultIndex = vinfo.Index;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                if (s_Type2Ids.TryGetValue(api.Type, out var ty)) {
                    int tmpIndex = -1;
                    switch (ty) {
                        case TypeEnum.Int:
                            tmpIndex = CurBlock().AllocTempInt();
                            break;
                        case TypeEnum.Float:
                            tmpIndex = CurBlock().AllocTempFloat();
                            break;
                        case TypeEnum.String:
                            tmpIndex = CurBlock().AllocTempString();
                            break;
                    }
                    if (tmpIndex >= 0) {
                        semanticInfo.IsGlobal = false;
                        semanticInfo.IsConstOrTemp = true;
                        semanticInfo.ResultType = api.Type;
                        semanticInfo.ResultCount = 0;
                        semanticInfo.ResultIndex = tmpIndex;
                        semanticInfo.ResultValues = null;
                        //gen api call
                        codes.Add(EncodeOpcode(InsEnum.CALL, opds.Count, semanticInfo.IsGlobal, semanticInfo.IsConstOrTemp, semanticInfo.ResultType, semanticInfo.ResultIndex));
                        int opd1 = 0;
                        int opd2 = 0;
                        opd1 = EncodeOperand1(api.ApiId);
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            if (null == opdInfo.ResultValues) {
                                opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                            }
                        }
                        codes.Add(opd1 | opd2);
                        for (int i = 1; i < opds.Count; i += 2) {
                            opd1 = 0;
                            if (i < opds.Count) {
                                var opdInfo = opds[i];
                                if (null == opdInfo.ResultValues) {
                                    opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                                }
                                else {
                                    int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                    opd1 = EncodeConstOperand1(opdInfo.ResultType, rindex);
                                }
                            }
                            opd2 = 0;
                            if (i + 1 < opds.Count) {
                                var opdInfo = opds[i + 1];
                                if (null == opdInfo.ResultValues) {
                                    opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                                }
                                else {
                                    int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                    opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                                }
                            }
                            codes.Add(opd1 | opd2);
                        }
                    }
                }
            }
        }
        private void TryGenArgc(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 0) {
                err.AppendFormat("argc must have zero arguments, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    var vinfo = GetVarInfo(semanticInfo.TargetName);
                    if (vinfo.Count > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.ARGC, vinfo.IsGlobal, false, vinfo.Type, vinfo.Index));

                    semanticInfo.IsGlobal = vinfo.IsGlobal;
                    semanticInfo.IsConstOrTemp = false;
                    semanticInfo.ResultType = vinfo.Type;
                    semanticInfo.ResultCount = vinfo.Count;
                    semanticInfo.ResultIndex = vinfo.Index;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.IsConstOrTemp = true;
                    semanticInfo.ResultType = "int";
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.ARGC, semanticInfo.IsGlobal, semanticInfo.IsConstOrTemp, semanticInfo.ResultType, semanticInfo.ResultIndex));
                }
            }
        }
        private void TryGenArgv(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != "int") {
                err.AppendFormat("arg must and only have one integer argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    var vinfo = GetVarInfo(semanticInfo.TargetName);
                    if (vinfo.Count > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.ARGV, vinfo.IsGlobal, false, vinfo.Type, vinfo.Index));
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, rindex);
                        }
                    }
                    int opd2 = 0;
                    if (opds.Count > 1) {
                        var opdInfo = opds[1];
                        if (null == opdInfo.ResultValues) {
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                        }
                    }
                    codes.Add(opd1 | opd2);

                    semanticInfo.IsGlobal = vinfo.IsGlobal;
                    semanticInfo.IsConstOrTemp = false;
                    semanticInfo.ResultType = vinfo.Type;
                    semanticInfo.ResultCount = vinfo.Count;
                    semanticInfo.ResultIndex = vinfo.Index;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.IsConstOrTemp = true;
                    semanticInfo.ResultType = "int";
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.ARGV, semanticInfo.IsGlobal, semanticInfo.IsConstOrTemp, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, rindex);
                        }
                    }
                    int opd2 = 0;
                    if (opds.Count > 1) {
                        var opdInfo = opds[1];
                        if (null == opdInfo.ResultValues) {
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                        }
                    }
                    codes.Add(opd1 | opd2);
                }
            }
        }
        private void TryGenAddr(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1) {
                err.AppendFormat("addr must and only have one argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
                return;
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    var vinfo = GetVarInfo(semanticInfo.TargetName);
                    if (vinfo.Count > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.ADDR, vinfo.IsGlobal, false, vinfo.Type, vinfo.Index));
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, rindex);
                        }
                    }
                    int opd2 = 0;
                    if (opds.Count > 1) {
                        var opdInfo = opds[1];
                        if (null == opdInfo.ResultValues) {
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                        }
                    }
                    codes.Add(opd1 | opd2);

                    semanticInfo.IsGlobal = vinfo.IsGlobal;
                    semanticInfo.IsConstOrTemp = false;
                    semanticInfo.ResultType = vinfo.Type;
                    semanticInfo.ResultCount = vinfo.Count;
                    semanticInfo.ResultIndex = vinfo.Index;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.IsConstOrTemp = true;
                    semanticInfo.ResultType = "int";
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.ADDR, semanticInfo.IsGlobal, semanticInfo.IsConstOrTemp, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, rindex);
                        }
                    }
                    int opd2 = 0;
                    if (opds.Count > 1) {
                        var opdInfo = opds[1];
                        if (null == opdInfo.ResultValues) {
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                        }
                    }
                    codes.Add(opd1 | opd2);
                }
            }
        }
        private void TryGenPtrGet(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 2 || opds[0].ResultType != "int" || opds[1].ResultType != "int") {
                err.AppendFormat("ptrget must and only have two integer arguments, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
                return;
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    var vinfo = GetVarInfo(semanticInfo.TargetName);
                    if (vinfo.Count > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.PTRGET, vinfo.IsGlobal, false, vinfo.Type, vinfo.Index));
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, rindex);
                        }
                    }
                    int opd2 = 0;
                    if (opds.Count > 1) {
                        var opdInfo = opds[1];
                        if (null == opdInfo.ResultValues) {
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                        }
                    }
                    codes.Add(opd1 | opd2);

                    semanticInfo.IsGlobal = vinfo.IsGlobal;
                    semanticInfo.IsConstOrTemp = false;
                    semanticInfo.ResultType = vinfo.Type;
                    semanticInfo.ResultCount = vinfo.Count;
                    semanticInfo.ResultIndex = vinfo.Index;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.IsConstOrTemp = true;
                    semanticInfo.ResultType = "int";
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.PTRGET, semanticInfo.IsGlobal, semanticInfo.IsConstOrTemp, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, rindex);
                        }
                    }
                    int opd2 = 0;
                    if (opds.Count > 1) {
                        var opdInfo = opds[1];
                        if (null == opdInfo.ResultValues) {
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                        }
                    }
                    codes.Add(opd1 | opd2);
                }
            }
        }
        private void TryGenPtrSet(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 3 || opds[0].ResultType != "int" || opds[1].ResultType != "int") {
                err.AppendFormat("ptrset must and only have three arguments and the first two parameters need to be integers, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
                return;
            }
            //gen ptrset
            if (opds.Count > 0) {
                int opcode = 0;
                var opdInfo = opds[0];
                if (null == opdInfo.ResultValues) {
                    opcode = EncodeOpcode(InsEnum.PTRSET, opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                }
                else {
                    int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                    opcode = EncodeOpcode(InsEnum.PTRSET, true, true, opdInfo.ResultType, rindex);
                }
                codes.Add(opcode);
            }
            int opd1 = 0;
            if (opds.Count > 1) {
                var opdInfo = opds[1];
                if (null == opdInfo.ResultValues) {
                    opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                }
                else {
                    int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                    opd1 = EncodeConstOperand1(opdInfo.ResultType, rindex);
                }
            }
            int opd2 = 0;
            if (opds.Count > 2) {
                var opdInfo = opds[2];
                if (null == opdInfo.ResultValues) {
                    opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                }
                else {
                    int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                    opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                }
            }
            codes.Add(opd1 | opd2);
        }
        private void TryGenJaggedPtr(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            foreach(var opd in opds) {
                if (opd.ResultType != "int") {
                    err.AppendFormat("jptr must only have integer arguments, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                    break;
                }
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    var vinfo = GetVarInfo(semanticInfo.TargetName);
                    if (vinfo.Count > 0) {
                        err.AppendFormat("Can't assignment api result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.JPTR, opds.Count, vinfo.IsGlobal, false, vinfo.Type, vinfo.Index));
                    for (int i = 0; i < opds.Count; i += 2) {
                        int opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            if (null == opdInfo.ResultValues) {
                                opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd1 = EncodeConstOperand1(opdInfo.ResultType, rindex);
                            }
                        }
                        int opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            if (null == opdInfo.ResultValues) {
                                opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                            }
                        }
                        codes.Add(opd1 | opd2);
                    }

                    semanticInfo.IsGlobal = vinfo.IsGlobal;
                    semanticInfo.IsConstOrTemp = false;
                    semanticInfo.ResultType = vinfo.Type;
                    semanticInfo.ResultCount = vinfo.Count;
                    semanticInfo.ResultIndex = vinfo.Index;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.IsConstOrTemp = true;
                    semanticInfo.ResultType = "int";
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.JPTR, opds.Count, semanticInfo.IsGlobal, semanticInfo.IsConstOrTemp, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    for (int i = 0; i < opds.Count; i += 2) {
                        int opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            if (null == opdInfo.ResultValues) {
                                opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd1 = EncodeConstOperand1(opdInfo.ResultType, rindex);
                            }
                        }
                        int opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            if (null == opdInfo.ResultValues) {
                                opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.IsConstOrTemp, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int rindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd2 = EncodeConstOperand2(opdInfo.ResultType, rindex);
                            }
                        }
                        codes.Add(opd1 | opd2);
                    }
                }
            }
        }

        private string DeduceType(InsEnum op, List<SemanticInfo> opds)
        {
            if (opds.Count == 1) {
                if (op == InsEnum.NOT || op == InsEnum.BITXOR)
                    return "int";
                return opds[0].ResultType;
            }
            else if (opds.Count == 2) {
                switch (op) {
                    case InsEnum.AND:
                    case InsEnum.OR:
                    case InsEnum.NOT:
                    case InsEnum.GT:
                    case InsEnum.GE:
                    case InsEnum.EQ:
                    case InsEnum.NE:
                    case InsEnum.LE:
                    case InsEnum.LT:
                    case InsEnum.LSHIFT:
                    case InsEnum.RSHIFT:
                    case InsEnum.URSHIFT:
                    case InsEnum.BITAND:
                    case InsEnum.BITOR:
                    case InsEnum.BITXOR:
                    case InsEnum.BITNOT:
                        return "int";
                }
                string type1 = opds[0].ResultType;
                string type2 = opds[1].ResultType;
                if (type1 == "string" || type2 == "string")
                    return "string";
                else if (type1 == "float" || type2 == "float")
                    return "float";
                else
                    return "int";
            }
            else if (opds.Count > 0) {
                return opds[0].ResultType;
            }
            return string.Empty;
        }
        private List<string> TryCalcConst(InsEnum op, List<SemanticInfo> opds)
        {
            foreach (var opd in opds) {
                if (null == opd.ResultValues)
                    return null;
            }
            List<string> ret = new List<string>();
            switch (op) {
                case InsEnum.NEG:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        if (opd.ResultType == "float") {
                            double.TryParse(opd.ResultValues[0], out var val);
                            ret.Add((-val).ToString());
                        }
                        else {
                            long.TryParse(opd.ResultValues[0], out var val);
                            ret.Add((-val).ToString());
                        }
                    }
                    break;
                case InsEnum.ADD:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        ret.AddRange(opd.ResultValues);
                    }
                    else if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == "float" || opd2.ResultType == "float") {
                            double.TryParse(opd1.ResultValues[0], out var val1);
                            double.TryParse(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 + val2).ToString());
                        }
                        else {
                            long.TryParse(opd1.ResultValues[0], out var val1);
                            long.TryParse(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 + val2).ToString());
                        }
                    }
                    break;
                case InsEnum.SUB:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == "float" || opd2.ResultType == "float") {
                            double.TryParse(opd1.ResultValues[0], out var val1);
                            double.TryParse(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 - val2).ToString());
                        }
                        else {
                            long.TryParse(opd1.ResultValues[0], out var val1);
                            long.TryParse(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 - val2).ToString());
                        }
                    }
                    break;
                case InsEnum.MUL:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == "float" || opd2.ResultType == "float") {
                            double.TryParse(opd1.ResultValues[0], out var val1);
                            double.TryParse(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 * val2).ToString());
                        }
                        else {
                            long.TryParse(opd1.ResultValues[0], out var val1);
                            long.TryParse(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 * val2).ToString());
                        }
                    }
                    break;
                case InsEnum.DIV:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == "float" || opd2.ResultType == "float") {
                            double.TryParse(opd1.ResultValues[0], out var val1);
                            double.TryParse(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 / val2).ToString());
                        }
                        else {
                            long.TryParse(opd1.ResultValues[0], out var val1);
                            long.TryParse(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 / val2).ToString());
                        }
                    }
                    break;
                case InsEnum.MOD:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == "float" || opd2.ResultType == "float") {
                            double.TryParse(opd1.ResultValues[0], out var val1);
                            double.TryParse(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 % val2).ToString());
                        }
                        else {
                            long.TryParse(opd1.ResultValues[0], out var val1);
                            long.TryParse(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 % val2).ToString());
                        }
                    }
                    break;
                case InsEnum.AND:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == "float" || opd2.ResultType == "float") {
                            double.TryParse(opd1.ResultValues[0], out var val1);
                            double.TryParse(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 != 0 && val2 != 0).ToString());
                        }
                        else {
                            long.TryParse(opd1.ResultValues[0], out var val1);
                            long.TryParse(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 != 0 && val2 != 0).ToString());
                        }
                    }
                    break;
                case InsEnum.OR:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == "float" || opd2.ResultType == "float") {
                            double.TryParse(opd1.ResultValues[0], out var val1);
                            double.TryParse(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 != 0 || val2 != 0).ToString());
                        }
                        else {
                            long.TryParse(opd1.ResultValues[0], out var val1);
                            long.TryParse(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 != 0 || val2 != 0).ToString());
                        }
                    }
                    break;
                case InsEnum.NOT:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        if (opd.ResultType == "float") {
                            double.TryParse(opd.ResultValues[0], out var val);
                            ret.Add((val == 0).ToString());
                        }
                        else {
                            long.TryParse(opd.ResultValues[0], out var val);
                            ret.Add((val == 0).ToString());
                        }
                    }
                    break;
                case InsEnum.GT:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == "float" || opd2.ResultType == "float") {
                            double.TryParse(opd1.ResultValues[0], out var val1);
                            double.TryParse(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 > val2 ? 1 : 0).ToString());
                        }
                        else {
                            long.TryParse(opd1.ResultValues[0], out var val1);
                            long.TryParse(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 > val2 ? 1 : 0).ToString());
                        }
                    }
                    break;
                case InsEnum.GE:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == "float" || opd2.ResultType == "float") {
                            double.TryParse(opd1.ResultValues[0], out var val1);
                            double.TryParse(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 >= val2 ? 1 : 0).ToString());
                        }
                        else {
                            long.TryParse(opd1.ResultValues[0], out var val1);
                            long.TryParse(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 >= val2 ? 1 : 0).ToString());
                        }
                    }
                    break;
                case InsEnum.EQ:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == "float" || opd2.ResultType == "float") {
                            double.TryParse(opd1.ResultValues[0], out var val1);
                            double.TryParse(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 == val2 ? 1 : 0).ToString());
                        }
                        else {
                            long.TryParse(opd1.ResultValues[0], out var val1);
                            long.TryParse(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 == val2 ? 1 : 0).ToString());
                        }
                    }
                    break;
                case InsEnum.NE:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == "float" || opd2.ResultType == "float") {
                            double.TryParse(opd1.ResultValues[0], out var val1);
                            double.TryParse(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 != val2 ? 1 : 0).ToString());
                        }
                        else {
                            long.TryParse(opd1.ResultValues[0], out var val1);
                            long.TryParse(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 != val2 ? 1 : 0).ToString());
                        }
                    }
                    break;
                case InsEnum.LE:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == "float" || opd2.ResultType == "float") {
                            double.TryParse(opd1.ResultValues[0], out var val1);
                            double.TryParse(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 <= val2 ? 1 : 0).ToString());
                        }
                        else {
                            long.TryParse(opd1.ResultValues[0], out var val1);
                            long.TryParse(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 <= val2 ? 1 : 0).ToString());
                        }
                    }
                    break;
                case InsEnum.LT:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == "float" || opd2.ResultType == "float") {
                            double.TryParse(opd1.ResultValues[0], out var val1);
                            double.TryParse(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 < val2 ? 1 : 0).ToString());
                        }
                        else {
                            long.TryParse(opd1.ResultValues[0], out var val1);
                            long.TryParse(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 < val2 ? 1 : 0).ToString());
                        }
                    }
                    break;
                case InsEnum.LSHIFT:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        long.TryParse(opd1.ResultValues[0], out var val1);
                        int.TryParse(opd2.ResultValues[0], out var val2);
                        ret.Add((val1 << val2).ToString());
                    }
                    break;
                case InsEnum.RSHIFT:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        long.TryParse(opd1.ResultValues[0], out var val1);
                        int.TryParse(opd2.ResultValues[0], out var val2);
                        ret.Add((val1 >> val2).ToString());
                    }
                    break;
                case InsEnum.URSHIFT:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        ulong.TryParse(opd1.ResultValues[0], out var val1);
                        int.TryParse(opd2.ResultValues[0], out var val2);
                        ret.Add(unchecked((long)(val1 >> val2)).ToString());
                    }
                    break;
                case InsEnum.BITAND:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        long.TryParse(opd1.ResultValues[0], out var val1);
                        long.TryParse(opd2.ResultValues[0], out var val2);
                        ret.Add((val1 & val2).ToString());
                    }
                    break;
                case InsEnum.BITOR:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        long.TryParse(opd1.ResultValues[0], out var val1);
                        long.TryParse(opd2.ResultValues[0], out var val2);
                        ret.Add((val1 | val2).ToString());
                    }
                    break;
                case InsEnum.BITXOR:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        long.TryParse(opd1.ResultValues[0], out var val1);
                        long.TryParse(opd2.ResultValues[0], out var val2);
                        ret.Add((val1 ^ val2).ToString());
                    }
                    break;
                case InsEnum.BITNOT:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        long.TryParse(opd.ResultValues[0], out var val);
                        ret.Add((~val).ToString());
                    }
                    break;
            }
            return ret;
        }
        private void CheckType(InsEnum op, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp)
        {
            foreach (var opd in opds) {
                if (opd.ResultCount > 0) {
                    err.AppendFormat("Can't calc on array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                    return;
                }
            }
            if (opds.Count == 1) {
                string type = opds[0].ResultType;
                if (type == "string" && op != InsEnum.NOT) {
                    err.AppendFormat("Can't calc on string, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                }
            }
            else if (opds.Count == 2) {
                string type1 = opds[0].ResultType;
                string type2 = opds[1].ResultType;
                if ((op == InsEnum.ADD || op == InsEnum.AND || op == InsEnum.OR || (op >= InsEnum.GE && op <= InsEnum.LT)) && type1 == "string" && type2 == "string") {
                }
                else if (type1 == "string" || type2 == "string") {
                    err.AppendFormat("Can't calc on string, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                }
                if (type1 == "float" || type2 == "float") {
                    switch (op) {
                        case InsEnum.LSHIFT:
                        case InsEnum.RSHIFT:
                        case InsEnum.URSHIFT:
                        case InsEnum.BITAND:
                        case InsEnum.BITOR:
                        case InsEnum.BITXOR:
                        case InsEnum.BITNOT:
                            err.AppendFormat("Can't calc on float, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                            break;
                    }
                }
            }
        }
        private void CheckType(ApiInfo api, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp)
        {
            for (int i = 0; i < opds.Count; ++i) {
                string rtype = opds[i].ResultType;
                string type;
                if (i < api.ParamTypes.Count) {
                    type = api.ParamTypes[i];
                    if (type != rtype) {
                        err.AppendFormat("'{0}' argument {1}'s type '{2}' dismatch '{3}', code:{4}, line:{5}", api.Name, i, rtype, type, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else if (api.Params == ParamsEnum.Ints) {
                    type = "int";
                    if (type != rtype) {
                        err.AppendFormat("'{0}' argument {1}'s type '{2}' dismatch '{3}', code:{4}, line:{5}", api.Name, i, rtype, type, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else if (api.Params == ParamsEnum.Floats) {
                    type = "float";
                    if (type != rtype) {
                        err.AppendFormat("'{0}' argument {1}'s type '{2}' dismatch '{3}', code:{4}, line:{5}", api.Name, i, rtype, type, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else if (api.Params == ParamsEnum.Strings) {
                    type = "string";
                    if (type != rtype) {
                        err.AppendFormat("'{0}' argument {1}'s type '{2}' dismatch '{3}', code:{4}, line:{5}", api.Name, i, rtype, type, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else if (api.Params == ParamsEnum.LastType && api.ParamTypes.Count > 0) {
                    type = api.ParamTypes[api.ParamTypes.Count - 1];
                    if (type != rtype) {
                        err.AppendFormat("'{0}' argument {1}'s type '{2}' dismatch '{3}', code:{4}, line:{5}", api.Name, i, rtype, type, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else {
                    break;
                }
            }
        }

        private int EncodeOpcode(InsEnum opc)
        {
            int opcode = (int)opc;
            return opcode;
        }
        private int EncodeOpcode(InsEnum opc, int offset)
        {
            int opcode = (int)opc;
            offset = EncodeOffset(offset);
            return opcode | offset;
        }
        private int EncodeOpcode(InsEnum opc, bool isGlobal, bool isConstOrTemp, string type, int index)
        {
            int opcode = (int)opc;
            int operand = EncodeOperand2(isGlobal, isConstOrTemp, type, index);
            opcode |= operand;
            return opcode;
        }
        private int EncodeOpcode(InsEnum opc, int argNum, bool isGlobal, bool isConstOrTemp, string type, int index)
        {
            int opcode = (int)opc;
            opcode |= (argNum << 8);
            int operand = EncodeOperand2(isGlobal, isConstOrTemp, type, index);
            opcode |= operand;
            return opcode;
        }
        private int EncodeOperand1(int apiIndex)
        {
            return (apiIndex & 0xffff);
        }
        private int EncodeOperand1(bool isGlobal, bool isConstOrTemp, string type, int index)
        {
            int operand = ConstOrTemp2Tag(isConstOrTemp) | Global2Tag(isGlobal) | Type2Tag(type) | index;
            return operand;
        }
        private int EncodeOperand2(bool isGlobal, bool isConstOrTemp, string type, int index)
        {
            int operand = EncodeOperand1(isGlobal, isConstOrTemp, type, index);
            return operand << 16;
        }
        private int EncodeConstOperand1(string type, int index)
        {
            return EncodeOperand1(true, true, type, index);
        }
        private int EncodeConstOperand2(string type, int index)
        {
            return EncodeOperand2(true, true, type, index);
        }
        private InsEnum DecodeInsEnum(int opcode)
        {
            return (InsEnum)(opcode & 0xff);
        }
        private void DecodeOpcode(int opcode, out InsEnum ins, out int offset)
        {
            ins = (InsEnum)(opcode & 0xff);
            offset = DecodeOffset(opcode);
        }
        private void DecodeOpcode(int opcode, out InsEnum ins, out int argNum, out bool isGlobal, out bool isConstOrTemp, out string type, out int index)
        {
            ins = (InsEnum)(opcode & 0xff);
            argNum = ((opcode & 0xff00) >> 8);
            int operand = opcode >> 16;
            DecodeOperand1(operand, out isGlobal, out isConstOrTemp, out type, out index);
        }
        private void DecodeOperand1(int operand, out int apiIndex)
        {
            apiIndex = (operand & 0xffff);
        }
        private void DecodeOperand1(int operand, out bool isGlobal, out bool isConstOrTemp, out string type, out int index)
        {
            int varOrConst = ((operand & 0x8000) >> 15);
            int localOrGlobal = ((operand & 0x4000) >> 14);
            int ty = ((operand & 0x3000) >> 12);
            index = (operand & 0xfff);
            isGlobal = localOrGlobal != 0;
            isConstOrTemp = varOrConst != 0;
            type = s_TypeNames[ty];
        }
        private void DecodeOperand2(int operand, out bool isGlobal, out bool isConstOrTemp, out string type, out int index)
        {
            operand >>= 16;
            DecodeOperand1(operand, out isGlobal, out isConstOrTemp, out type, out index);
        }

        //program structure
        private int AddConst(string type, string val)
        {
            if (!m_ConstInfos.TryGetValue(val, out var info)) {
                info = new ConstInfo { StrValue = val, Type = type };
                if (s_Type2Ids.TryGetValue(type, out var typeid)) {
                    switch (typeid) {
                        case TypeEnum.Int:
                            info.Index = m_ConstIndexInfo.NextIntIndex++;
                            break;
                        case TypeEnum.Float:
                            info.Index = m_ConstIndexInfo.NextFloatIndex++;
                            break;
                        case TypeEnum.String:
                            info.Index = m_ConstIndexInfo.NextStringIndex++;
                            break;
                    }
                }
                m_ConstInfos.Add(val, info);
            }
            return info.Index;
        }
        private int AddVar(VarInfo varInfo)
        {
            if (!m_VarInfos.TryGetValue(varInfo.Name, out var varInfos)) {
                varInfos = new Dictionary<int, VarInfo>();
                m_VarInfos.Add(varInfo.Name, varInfos);
            }
            varInfo.OwnHook = CurHookInfo();
            varInfos[CurBlockId()] = varInfo;

            var curBlock = CurBlock();
            Debug.Assert(null != curBlock);
            if (s_Type2Ids.TryGetValue(varInfo.Type, out var typeid)) {
                switch (typeid) {
                    case TypeEnum.Int:
                        varInfo.Index = curBlock.NextIntVarIndex++;
                        if (varInfo.Count > 0) {
                            curBlock.NextIntVarIndex += varInfo.Count - 1;
                        }
                        break;
                    case TypeEnum.Float:
                        varInfo.Index = curBlock.NextFloatVarIndex++;
                        if (varInfo.Count > 0) {
                            curBlock.NextFloatVarIndex += varInfo.Count - 1;
                        }
                        break;
                    case TypeEnum.String:
                        varInfo.Index = curBlock.NextStringVarIndex++;
                        if (varInfo.Count > 0) {
                            curBlock.NextStringVarIndex += varInfo.Count - 1;
                        }
                        break;
                }
            }
            return varInfo.Index;
        }
        private HookInfo CurHookInfo()
        {
            HookInfo curHookInfo = null;
            if (m_HookParseStack.Count > 0) {
                curHookInfo = m_HookParseStack.Peek();
            }
            return curHookInfo;
        }
        private void PushHookInfo(HookInfo hookInfo)
        {
            m_HookParseStack.Push(hookInfo);
        }
        private void PopHookInfo()
        {
            m_HookParseStack.Pop();
        }
        private void PushBlock(bool isBranch, bool isLoop)
        {
            ++m_LastBlockId;
            var lexicalInfo = new LexicalScopeInfo { IsBranch = isBranch, IsLoop = isLoop, BlockId = m_LastBlockId };
            if (m_LexicalScopeStack.Count > 0) {
                var curInfo = m_LexicalScopeStack.Peek();
                lexicalInfo.NextIntVarIndex = curInfo.NextIntVarIndex;
                lexicalInfo.NextFloatVarIndex = curInfo.NextFloatVarIndex;
                lexicalInfo.NextStringVarIndex = curInfo.NextStringVarIndex;
            }
            m_LexicalScopeStack.Push(lexicalInfo);
        }
        private void PopBlock()
        {
            var lastInfo = m_LexicalScopeStack.Pop();
        }
        private LexicalScopeInfo CurBlock()
        {
            if (m_LexicalScopeStack.Count > 0) {
                return m_LexicalScopeStack.Peek();
            }
            return m_ToplevelLexicalScopeInfo;
        }
        private bool IsGlobalBlock()
        {
            return m_LexicalScopeStack.Count == 0;
        }
        private int CurBlockId()
        {
            return CurBlock().BlockId;
        }
        private LexicalScopeInfo CurBranchBlock()
        {
            foreach (var info in m_LexicalScopeStack) {
                if (info.IsBranch)
                    return info;
            }
            return null;
        }
        private LexicalScopeInfo CurLoopBlock()
        {
            foreach (var info in m_LexicalScopeStack) {
                if (info.IsLoop)
                    return info;
            }
            return null;
        }
        private int CurLoopContinuePoint()
        {
            var info = CurLoopBlock();
            if (info != null) {
                return info.LoopContinue;
            }
            return -1;
        }
        private VarInfo GetVarInfo(string name)
        {
            VarInfo varInfo = null;
            if (m_VarInfos.TryGetValue(name, out var varInfos)) {
                bool find = false;
                foreach (var scopeInfo in m_LexicalScopeStack) {
                    if (varInfos.TryGetValue(scopeInfo.BlockId, out varInfo)) {
                        find = true;
                        break;
                    }
                }
                if (!find) {
                    find = varInfos.TryGetValue(0, out varInfo);
                }
            }
            return varInfo;
        }
        private int GenUniqueNumber()
        {
            return ++m_UniqueNumber;
        }
        private void Reset()
        {
            m_StructInfos.Clear();
            m_ConstInfos.Clear();
            m_VarInfos.Clear();
            m_LexicalScopeStack.Clear();
            m_ToplevelLexicalScopeInfo = new LexicalScopeInfo();
            m_LastBlockId = 0;
            m_UniqueNumber = 0;

            m_HookParseStack.Clear();
            m_Hooks.Clear();
        }

        public enum ParamsEnum
        {
            NoParams = 0,
            Printf,
            Ints,
            Floats,
            Strings,
            LastType,
            Num
        }
        public sealed class ApiInfo
        {
            public int ApiId = 0;
            public string Name = string.Empty;
            public string Type = string.Empty;
            public ParamsEnum Params = ParamsEnum.NoParams;
            public List<string> ParamTypes = new List<string>();
        }

        public sealed class FieldInfo
        {
            public string Name = string.Empty;
            public string Type = string.Empty;
            public List<int> ArrayOrPtrs = new List<int>(); // <=0 -- pointer, >0 -- array size
            public int Offset = 0;
        }
        public sealed class StructInfo
        {
            public string Name = string.Empty;
            public List<FieldInfo> Fields = new List<FieldInfo>();
        }

        public sealed class ConstInfo
        {
            public string StrValue = string.Empty;
            public string Type = string.Empty;
            public int Index = 0;
        }
        public sealed class VarInfo
        {
            public string Name = string.Empty;
            public string Type = string.Empty;
            public int Count = 0;
            public int Index = 0;
            public string[] InitValues = null;
            public HookInfo OwnHook = null;

            public void CopyFrom(VarInfo other)
            {
                Name = other.Name;
                Type = other.Type;
            }
            public bool IsGlobal
            {
                get { return IsGlobalVar(Name); }
            }
        }
        public sealed class HookInfo
        {
            public string Name = string.Empty;
            public SortedSet<string> ShareWith = new SortedSet<string>();
            public List<int> OnEnter = new List<int>();
            public List<int> OnExit = new List<int>();
        }

        public sealed class ConstIndexInfo
        {
            public int NextIntIndex = 0;
            public int NextFloatIndex = 0;
            public int NextStringIndex = 0;
        }
        public sealed class LexicalScopeInfo
        {
            public bool IsBranch = false;
            public bool IsLoop = false;
            public int BlockId = 0;
            public int NextIntVarIndex = 0;
            public int NextFloatVarIndex = 0;
            public int NextStringVarIndex = 0;

            public int NextTempIntVarIndex = 0;
            public int NextTempFloatVarIndex = 0;
            public int NextTempStringVarIndex = 0;

            public int LoopContinue = 0;
            public List<int> LoopBreakFixes = null;

            public void ResetTempVars()
            {
                NextTempIntVarIndex = 0;
                NextTempFloatVarIndex = 0;
                NextTempStringVarIndex = 0;
            }
            public int AllocTempInt()
            {
                return NextTempIntVarIndex++;
            }
            public int AllocTempFloat()
            {
                return NextTempFloatVarIndex++;
            }
            public int AllocTempString()
            {
                return NextTempStringVarIndex++;
            }
            public int AllocTempIntArray(int ct)
            {
                NextTempIntVarIndex += ct - 1;
                return NextTempIntVarIndex++;
            }
            public int AllocTempFloatArray(int ct)
            {
                NextTempFloatVarIndex += ct - 1;
                return NextTempFloatVarIndex++;
            }
            public int AllocTempStringArray(int ct)
            {
                NextTempStringVarIndex += ct - 1;
                return NextTempStringVarIndex++;
            }
        }
        public enum TargetOperationEnum
        {
            TypeInfo = 0,
            VarAssign,
            Num
        }
        public struct SemanticInfo
        {
            public TargetOperationEnum TargetOperation;
            public string TargetType;
            public int TargetCount;
            public string TargetName;

            public bool IsGlobal;
            public bool IsConstOrTemp;
            public string ResultType;
            public int ResultCount;
            public int ResultIndex;
            public List<string> ResultValues;

            public void Reset()
            {
                TargetOperation = TargetOperationEnum.TypeInfo;
                TargetType = string.Empty;
                TargetCount = 0;
                TargetName = string.Empty;

                IsGlobal = false;
                IsConstOrTemp = false;
                ResultType = string.Empty;
                ResultCount = 0;
                ResultIndex = 0;
                ResultValues = null;
            }
            public void CopyResultFrom(SemanticInfo other)
            {
                IsGlobal = other.IsGlobal;
                IsConstOrTemp = other.IsConstOrTemp;
                ResultType = other.ResultType;
                ResultCount = other.ResultCount;
                ResultIndex = other.ResultIndex;
                ResultValues = other.ResultValues;
            }
        }

        private List<int> m_TempCodes = new List<int>();
        private ConstIndexInfo m_ConstIndexInfo = new ConstIndexInfo();
        private Dictionary<string, ConstInfo> m_ConstInfos = new Dictionary<string, ConstInfo>();
        private Dictionary<string, Dictionary<int, VarInfo>> m_VarInfos = new Dictionary<string, Dictionary<int, VarInfo>>();
        public List<HookInfo> m_Hooks = new List<HookInfo>();

        private Stack<HookInfo> m_HookParseStack = new Stack<HookInfo>();
        private Stack<LexicalScopeInfo> m_LexicalScopeStack = new Stack<LexicalScopeInfo>();
        private LexicalScopeInfo m_ToplevelLexicalScopeInfo = new LexicalScopeInfo();
        private int m_LastBlockId = 0;
        private int m_UniqueNumber = 0;

        private Dictionary<string, ApiInfo> m_Apis = new Dictionary<string, ApiInfo>();
        private Dictionary<string, StructInfo> m_PredefinedStructInfos = new Dictionary<string, StructInfo>();
        private Dictionary<string, StructInfo> m_StructInfos = new Dictionary<string, StructInfo>();

        private enum VarConstEnum
        {
            Var = 0,
            ConstOrTemp
        }
        private enum LocalGlobalEnum
        {
            Local = 0,
            Global
        }
        private enum TypeEnum
        {
            NotUse = 0,
            Int,
            Float,
            String
        }

        private static int ConstOrTemp2Tag(bool isConstOrTemp)
        {
            //bit 15: 0--var 1--const or temp
            if (isConstOrTemp)
                return (int)VarConstEnum.ConstOrTemp << 15;
            else
                return (int)VarConstEnum.Var << 15;
        }
        private static int Global2Tag(bool isGlobal)
        {
            //bit 14: 0--local 1--global
            if (isGlobal)
                return (int)LocalGlobalEnum.Global << 14;
            else
                return (int)LocalGlobalEnum.Local << 14;
        }
        private static int Type2Tag(string type)
        {
            //bit 12、13: 0--not use 1--int 2--float 3--string
            if (type == "int") {
                return (int)TypeEnum.Int << 12;
            }
            else if (type == "float") {
                return (int)TypeEnum.Float << 12;
            }
            else {
                return (int)TypeEnum.String << 12;
            }
        }
        private static int EncodeOffset(int offset)
        {
            bool back = offset < 0 ? true : false;
            if (back)
                offset = -offset;
            offset <<= 8;
            if (back)
                offset |= c_offset_backward_flag;
            return offset;
        }
        private static int DecodeOffset(int offset)
        {
            bool back = (offset & c_offset_backward_flag) != 0;
            offset = ((offset & c_abs_offset_mask) >> 8);
            if (back)
                offset = -offset;
            return offset;
        }
        private static bool IsGlobalVar(string name)
        {
            if (!string.IsNullOrEmpty(name) && name.Length > 0 && name[0] == '@')
                return true;
            return false;
        }
        private static bool IsLocalVar(string name)
        {
            if (!string.IsNullOrEmpty(name) && name.Length > 0 && name[0] == '$')
                return true;
            return false;
        }

        private static Dictionary<string, TypeEnum> s_Type2Ids = new Dictionary<string, TypeEnum>() {
            { string.Empty, TypeEnum.NotUse },
            { "int", TypeEnum.Int },
            { "float", TypeEnum.Float },
            { "string", TypeEnum.String }
        };
        private static string[] s_TypeNames = new[] { string.Empty, "int", "float", "string" };

        private const int c_abs_offset_mask = 0x7fffffff;
        private const int c_offset_backward_flag = unchecked((int)0x80000000);

        public static DebugScriptCompiler Instance
        {
            get {
                return s_Instance;
            }
        }
        private static DebugScriptCompiler s_Instance = new DebugScriptCompiler();
    }
    internal static class Literal
    {
        internal static string GetIndentString(int indent)
        {
            PrepareIndent(indent);
            return s_IndentString.Substring(0, indent);
        }
        internal static string GetSpaceString(int indent)
        {
            PrepareSpace(indent * c_IndentSpaceString.Length);
            return s_SpaceString.Substring(0, indent * c_IndentSpaceString.Length);
        }

        private static void PrepareIndent(int indent)
        {
            if (s_IndentString.Length < indent) {
                int len = c_InitCount;
                while (len < indent) {
                    len *= 2;
                }
                s_IndentString = new string('\t', indent);
            }
        }
        private static void PrepareSpace(int indent)
        {
            if (s_SpaceString.Length < indent) {
                int len = c_InitCount;
                while (len < indent) {
                    len *= 2;
                }
                while (s_SpaceBuilder.Length < len) {
                    s_SpaceBuilder.Append(c_IndentSpaceString);
                }
                s_SpaceString = s_SpaceBuilder.ToString();
            }
        }

        private static string s_IndentString = string.Empty;
        private static string s_SpaceString = string.Empty;
        private static StringBuilder s_SpaceBuilder = new StringBuilder();

        private const int c_InitCount = 256;
        private const string c_IndentSpaceString = "| ";
    }
    internal static class StringBuilderExtension
    {
        public static void Append(this StringBuilder sb, string fmt, params object[] args)
        {
            if (args.Length == 0) {
                sb.Append(fmt);
            }
            else {
                sb.AppendFormat(fmt, args);
            }
        }
        public static void AppendLine(this StringBuilder sb, string fmt, params object[] args)
        {
            if (args.Length == 0) {
                sb.AppendLine(fmt);
            }
            else {
                sb.AppendFormat(fmt, args);
                sb.AppendLine();
            }
        }
    }
#pragma warning disable CA1060
    internal static class CppDbgScpInterface
#pragma warning restore CA1060
    {
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CppDbgScp_ResetVM();
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CppDbgScp_AllocConstInt(long val);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CppDbgScp_AllocConstFloat(double val);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CppDbgScp_AllocConstString(IntPtr val);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CppDbgScp_AllocGlobalInt(long val);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CppDbgScp_AllocGlobalFloat(double val);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CppDbgScp_AllocGlobalString(IntPtr val);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static int CppDbgScp_AddHook(IntPtr name, IntPtr enterCodes, int enterCodeNum, IntPtr exitCodes, int exitCodeNum);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static int CppDbgScp_ShareWith(int hookId, IntPtr other);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CppDbgScp_StartVM();

#pragma warning disable CA2101
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static int Test1(int a, double b, [MarshalAs(UnmanagedType.LPStr)]string c);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static int Test2(int a, double b, [MarshalAs(UnmanagedType.LPStr)] string c);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void Test3(int a, double b, [MarshalAs(UnmanagedType.LPStr)] string c);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void Test4(int a, double b, [MarshalAs(UnmanagedType.LPStr)] string c);
#pragma warning restore CA2101
    }
}
