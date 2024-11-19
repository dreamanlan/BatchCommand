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
using DotnetStoryScript.CommonFunctions;

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
        JMPIF,
        JMPIFNOT,
        INC,
        INCFLT,
        INCV,
        INCVFLT,
        MOV,
        MOVFLT,
        MOVSTR,
        ARRGET,
        ARRGETFLT,
        ARRGETSTR,
        ARRSET,
        ARRSETFLT,
        ARRSETSTR,
        NEG,
        NEGFLT,
        ADD,
        ADDFLT,
        ADDSTR,
        SUB,
        SUBFLT,
        MUL,
        MULFLT,
        DIV,
        DIVFLT,
        MOD,
        MODFLT,
        AND,
        OR,
        NOT,
        GT,
        GTFLT,
        GTSTR,
        GE,
        GEFLT,
        GESTR,
        EQ,
        EQFLT,
        EQSTR,
        NE,
        NEFLT,
        NESTR,
        LE,
        LEFLT,
        LESTR,
        LT,
        LTFLT,
        LTSTR,
        LSHIFT,
        RSHIFT,
        URSHIFT,
        BITAND,
        BITOR,
        BITXOR,
        BITNOT,
        INT2FLT,
        INT2STR,
        FLT2INT,
        FLT2STR,
        STR2INT,
        STR2FLT,
        ITOF,
        FTOI,
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

                        s_Type2Ids.TryGetValue(type, out var rty);

                        ParamsEnum paramsEnum = ParamsEnum.NoParams;
                        if (paramsStr.Length > 0 && char.IsNumber(paramsStr[0])) {
                            int.TryParse(paramsStr, out var v);
                            paramsEnum = (ParamsEnum)v;
                        }
                        else {
                            Enum.TryParse<ParamsEnum>(paramsStr, true, out paramsEnum);
                        }

                        var api = new ApiInfo { ApiId = apiId, Name = name, Type = rty, Params = paramsEnum };
                        ++apiId;

                        var pts = call.GetParam(3) as Dsl.FunctionData;
                        foreach (var p in pts.Params) {
                            s_Type2Ids.TryGetValue(p.GetId(), out var ty);
                            api.ParamTypes.Add(ty);
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
                            CalcStructOffsetAndSize(struInfo);
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
                txt.AppendLine("{0}value:{1} type:{2} index:{3}", Literal.GetIndentString(indent), info.StrValue, info.Type, c_max_variable_table_size - 1 - info.Index);
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

            var comparer = new ReverseComparer<int>(Comparer<int>.Default);
            var consts = new SortedDictionary<int, ConstInfo>(comparer);
            CollectConstInfo(TypeEnum.Int, consts);
            foreach(var pair in consts) {
                var info = pair.Value;
                long.TryParse(info.StrValue, out var v);
                CppDbgScpInterface.CppDbgScp_AllocConstInt(v);
            }
            consts.Clear();
            CollectConstInfo(TypeEnum.Float, consts);
            foreach (var pair in consts) {
                var info = pair.Value;
                double.TryParse(info.StrValue, out var v);
                CppDbgScpInterface.CppDbgScp_AllocConstFloat(v);
            }
            consts.Clear();
            CollectConstInfo(TypeEnum.String, consts);
            foreach (var pair in consts) {
                var info = pair.Value;
                var v = info.StrValue;
                IntPtr str = Marshal.StringToHGlobalAnsi(v);
                CppDbgScpInterface.CppDbgScp_AllocConstString(str);
            }

            var globals = new SortedDictionary<int, VarInfo>();
            CollectGlobalInfo(TypeEnum.Int, globals);
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
            CollectGlobalInfo(TypeEnum.Float, globals);
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
            CollectGlobalInfo(TypeEnum.String, globals);
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
        public void SaveByteCode(string file)
        {
            //tag:DSBC 0x43425344
            using (var sw = new BinaryWriter(new FileStream(file, FileMode.Create))) {
                //tag
                sw.Write(0x43425344);

                //str table offset
                sw.Write(0);

                var comparer = new ReverseComparer<int>(Comparer<int>.Default);
                var consts = new SortedDictionary<int, ConstInfo>(comparer);
                int constNum = CollectConstInfo(TypeEnum.Int, consts);

                var strDict = new Dictionary<string, int>();
                var strTable = new List<string>();

                //int consts num
                sw.Write(constNum);

                foreach (var pair in consts) {
                    var info = pair.Value;
                    long.TryParse(info.StrValue, out var v);

                    //int const
                    sw.Write(v);
                }
                consts.Clear();
                constNum = CollectConstInfo(TypeEnum.Float, consts);

                //float consts num
                sw.Write(constNum);

                foreach (var pair in consts) {
                    var info = pair.Value;
                    double.TryParse(info.StrValue, out var v);

                    //float const
                    sw.Write(v);
                }
                consts.Clear();
                constNum = CollectConstInfo(TypeEnum.String, consts);

                //string consts num
                sw.Write(constNum);

                foreach (var pair in consts) {
                    var info = pair.Value;
                    var v = info.StrValue;

                    //string const
                    sw.Write(AddStringTable(v, strDict, strTable));
                }

                var globals = new SortedDictionary<int, VarInfo>();
                int globalNum = CollectGlobalInfo(TypeEnum.Int, globals);

                //int globals num
                sw.Write(globalNum);

                foreach (var pair in globals) {
                    var info = pair.Value;
                    if (null == info.InitValues) {
                        sw.Write((long)0);
                    }
                    else {
                        foreach (var strVal in info.InitValues) {
                            long.TryParse(strVal, out var v);
                            sw.Write(v);
                        }
                    }
                }
                globals.Clear();
                globalNum = CollectGlobalInfo(TypeEnum.Float, globals);

                //float globals num
                sw.Write(globalNum);

                foreach (var pair in globals) {
                    var info = pair.Value;
                    if (null == info.InitValues) {
                        sw.Write((double)0.0);
                    }
                    else {
                        foreach (var strVal in info.InitValues) {
                            double.TryParse(strVal, out var v);
                            sw.Write(v);
                        }
                    }
                }
                globals.Clear();
                globalNum = CollectGlobalInfo(TypeEnum.String, globals);

                //string globals num
                sw.Write(globalNum);

                foreach (var pair in globals) {
                    var info = pair.Value;
                    if (null == info.InitValues) {
                        sw.Write(AddStringTable(string.Empty, strDict, strTable));
                    }
                    else {
                        foreach (var strVal in info.InitValues) {
                            sw.Write(AddStringTable(strVal, strDict, strTable));
                        }
                    }
                }

                sw.Write(m_Hooks.Count);
                foreach (var hook in m_Hooks) {
                    sw.Write(AddStringTable(hook.Name, strDict, strTable));
                    sw.Write(hook.OnEnter.Count);
                    foreach(var v in hook.OnEnter) {
                        sw.Write(v);
                    }
                    sw.Write(hook.OnExit.Count);
                    foreach (var v in hook.OnExit) {
                        sw.Write(v);
                    }
                    sw.Write(hook.ShareWith.Count);
                    foreach (var other in hook.ShareWith) {
                        sw.Write(AddStringTable(other, strDict, strTable));
                    }
                }

                int strTableOffset = (int)sw.BaseStream.Position;
                sw.Write(strTable.Count);
                foreach(var str in strTable) {
                    sw.Write(str);
                }

                sw.Seek(sizeof(int), SeekOrigin.Begin);
                sw.Write(strTableOffset);

                sw.Close();
            }
        }
        public void LoadByteCode(string file)
        {
            CppDbgScpInterface.CppDbgScp_Load(file);
        }

        private int CollectConstInfo(TypeEnum type, SortedDictionary<int, ConstInfo> constInfos)
        {
            foreach(var pair in m_ConstInfos) {
                var info = pair.Value;
                if(info.Type == type) {
                    constInfos.Add(info.Index, info);
                }
            }
            return constInfos.Count;
        }
        private int CollectGlobalInfo(TypeEnum type, SortedDictionary<int, VarInfo> varInfos)
        {
            int ct = 0;
            foreach (var pair in m_VarInfos) {
                var dict = pair.Value;
                foreach (var pair2 in dict) {
                    int blockId = pair2.Key;
                    var info = pair2.Value;
                    if (blockId == 0 && info.Type == type) {
                        varInfos.Add(info.Index, info);
                        if (info.Count > 0)
                            ct += info.Count;
                        else
                            ++ct;
                    }
                }
            }
            return ct;
        }
        private int AddStringTable(string str, Dictionary<string, int> dict, List<string> table)
        {
            if (!dict.TryGetValue(str, out var ix)) {
                ix = table.Count;
                table.Add(str);
                dict.Add(str, ix);
            }
            return ix;
        }
        
        private void DumpAsm(StringBuilder txt, int indent, List<int> codes)
        {
            for (int pos = 0; pos < codes.Count; ++pos) {
                int opcode = codes[pos];
                InsEnum op = DecodeInsEnum(opcode);
                switch (op) {
                    case InsEnum.CALL:
                        DumpCall(txt, indent, codes, ref pos, "CALL");
                        break;
                    case InsEnum.RET:
                        DumpRet(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.JMP:
                        DumpJmp(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.JMPIF:
                        DumpJmpIf(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.JMPIFNOT:
                        DumpJmpIfNot(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.INC:
                        DumpInc(txt, indent, codes, ref pos, "INC");
                        break;
                    case InsEnum.INCFLT:
                        DumpInc(txt, indent, codes, ref pos, "INCFLT");
                        break;
                    case InsEnum.INCV:
                        DumpIncVal(txt, indent, codes, ref pos, "INCV");
                        break;
                    case InsEnum.INCVFLT:
                        DumpIncVal(txt, indent, codes, ref pos, "INCVFLT");
                        break;
                    case InsEnum.MOV:
                        DumpMov(txt, indent, codes, ref pos, "MOV");
                        break;
                    case InsEnum.MOVFLT:
                        DumpMov(txt, indent, codes, ref pos, "MOVFLT");
                        break;
                    case InsEnum.MOVSTR:
                        DumpMov(txt, indent, codes, ref pos, "MOVSTR");
                        break;
                    case InsEnum.ARRGET:
                        DumpArrGet(txt, indent, codes, ref pos, "ARRGET");
                        break;
                    case InsEnum.ARRGETFLT:
                        DumpArrGet(txt, indent, codes, ref pos, "ARRGETFLT");
                        break;
                    case InsEnum.ARRGETSTR:
                        DumpArrGet(txt, indent, codes, ref pos, "ARRGETSTR");
                        break;
                    case InsEnum.ARRSET:
                        DumpArrSet(txt, indent, codes, ref pos, "ARRSET");
                        break;
                    case InsEnum.ARRSETFLT:
                        DumpArrSet(txt, indent, codes, ref pos, "ARRSETFLT");
                        break;
                    case InsEnum.ARRSETSTR:
                        DumpArrSet(txt, indent, codes, ref pos, "ARRSETSTR");
                        break;
                    case InsEnum.NEG:
                        DumpUnary(txt, indent, codes, ref pos, "NEG");
                        break;
                    case InsEnum.NEGFLT:
                        DumpUnary(txt, indent, codes, ref pos, "NEGFLT");
                        break;
                    case InsEnum.ADD:
                        DumpBinary(txt, indent, codes, ref pos, "ADD");
                        break;
                    case InsEnum.ADDFLT:
                        DumpBinary(txt, indent, codes, ref pos, "ADDFLT");
                        break;
                    case InsEnum.ADDSTR:
                        DumpBinary(txt, indent, codes, ref pos, "ADDSTR");
                        break;
                    case InsEnum.SUB:
                        DumpBinary(txt, indent, codes, ref pos, "SUB");
                        break;
                    case InsEnum.SUBFLT:
                        DumpBinary(txt, indent, codes, ref pos, "SUBFLT");
                        break;
                    case InsEnum.MUL:
                        DumpBinary(txt, indent, codes, ref pos, "MUL");
                        break;
                    case InsEnum.MULFLT:
                        DumpBinary(txt, indent, codes, ref pos, "MULFLT");
                        break;
                    case InsEnum.DIV:
                        DumpBinary(txt, indent, codes, ref pos, "DIV");
                        break;
                    case InsEnum.DIVFLT:
                        DumpBinary(txt, indent, codes, ref pos, "DIVFLT");
                        break;
                    case InsEnum.MOD:
                        DumpBinary(txt, indent, codes, ref pos, "MOD");
                        break;
                    case InsEnum.MODFLT:
                        DumpBinary(txt, indent, codes, ref pos, "MODFLT");
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
                    case InsEnum.GTFLT:
                        DumpBinary(txt, indent, codes, ref pos, "GTFLT");
                        break;
                    case InsEnum.GTSTR:
                        DumpBinary(txt, indent, codes, ref pos, "GTSTR");
                        break;
                    case InsEnum.GE:
                        DumpBinary(txt, indent, codes, ref pos, "GE");
                        break;
                    case InsEnum.GEFLT:
                        DumpBinary(txt, indent, codes, ref pos, "GEFLT");
                        break;
                    case InsEnum.GESTR:
                        DumpBinary(txt, indent, codes, ref pos, "GESTR");
                        break;
                    case InsEnum.EQ:
                        DumpBinary(txt, indent, codes, ref pos, "EQ");
                        break;
                    case InsEnum.EQFLT:
                        DumpBinary(txt, indent, codes, ref pos, "EQFLT");
                        break;
                    case InsEnum.EQSTR:
                        DumpBinary(txt, indent, codes, ref pos, "EQSTR");
                        break;
                    case InsEnum.NE:
                        DumpBinary(txt, indent, codes, ref pos, "NE");
                        break;
                    case InsEnum.NEFLT:
                        DumpBinary(txt, indent, codes, ref pos, "NEFLT");
                        break;
                    case InsEnum.NESTR:
                        DumpBinary(txt, indent, codes, ref pos, "NESTR");
                        break;
                    case InsEnum.LE:
                        DumpBinary(txt, indent, codes, ref pos, "LE");
                        break;
                    case InsEnum.LEFLT:
                        DumpBinary(txt, indent, codes, ref pos, "LEFLT");
                        break;
                    case InsEnum.LESTR:
                        DumpBinary(txt, indent, codes, ref pos, "LESTR");
                        break;
                    case InsEnum.LT:
                        DumpBinary(txt, indent, codes, ref pos, "LT");
                        break;
                    case InsEnum.LTFLT:
                        DumpBinary(txt, indent, codes, ref pos, "LTFLT");
                        break;
                    case InsEnum.LTSTR:
                        DumpBinary(txt, indent, codes, ref pos, "LTSTR");
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
                    case InsEnum.INT2FLT:
                        DumpUnary(txt, indent, codes, ref pos, "INT2FLT");
                        break;
                    case InsEnum.INT2STR:
                        DumpUnary(txt, indent, codes, ref pos, "INT2STR");
                        break;
                    case InsEnum.FLT2INT:
                        DumpUnary(txt, indent, codes, ref pos, "FLT2INT");
                        break;
                    case InsEnum.FLT2STR:
                        DumpUnary(txt, indent, codes, ref pos, "FLT2STR");
                        break;
                    case InsEnum.STR2INT:
                        DumpUnary(txt, indent, codes, ref pos, "STR2INT");
                        break;
                    case InsEnum.STR2FLT:
                        DumpUnary(txt, indent, codes, ref pos, "STR2FLT");
                        break;
                    case InsEnum.ITOF:
                        DumpIToF(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.FTOI:
                        DumpFToI(txt, indent, codes, ref pos);
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
        private void DumpCall(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.Append("{0}{1}: {2} = {3}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), op);
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
                        DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
                        txt.Append(BuildVar(isGlobal1, type1, index1));
                    }
                }
                if (i + 1 <= argNum) {
                    if (i == 0)
                        txt.Append(' ');
                    else
                        txt.Append(", ");
                    DecodeOperand2(operand, out var isGlobal2, out var type2, out var index2);
                    txt.Append(BuildVar(isGlobal2, type2, index2));
                }
            }
            txt.AppendLine();
        }
        private void DumpRet(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.AppendLine("{0}{1}: RET {2}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index));
        }
        private void DumpJmp(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var offset);
            txt.AppendLine("{0}{1}: JMP {2}", Literal.GetIndentString(indent), ix, offset);
        }
        private void DumpJmpIf(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var offset);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: JMPIF {2}, {3}", Literal.GetIndentString(indent), ix, offset, BuildVar(isGlobal1, type1, index1));
        }
        private void DumpJmpIfNot(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var offset);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: JMPIFNOT {2}, {3}", Literal.GetIndentString(indent), ix, offset, BuildVar(isGlobal1, type1, index1));
        }
        private void DumpInc(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.AppendLine("{0}{1}: {2} = {3} {2}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), op);
        }
        private void DumpIncVal(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: {2} = {4} {2} {3}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), BuildVar(isGlobal1, type1, index1), op);
        }
        private void DumpMov(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: {2} {3}, {4}", Literal.GetIndentString(indent), ix, op, BuildVar(isGlobal, type, index), BuildVar(isGlobal1, type1, index1));
        }
        private void DumpArrGet(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            DecodeOperand2(operand, out var isGlobal2, out var type2, out var index2);
            txt.AppendLine("{0}{1}: {2} = {3} {4}, {5}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), op, BuildVar(isGlobal1, type1, index1), BuildVar(isGlobal2, type2, index2));
        }
        private void DumpArrSet(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            DecodeOperand2(operand, out var isGlobal2, out var type2, out var index2);
            txt.AppendLine("{0}{1}: {2} {3}, {4}, {5}", Literal.GetIndentString(indent), ix, op, BuildVar(isGlobal, type, index), BuildVar(isGlobal1, type1, index1), BuildVar(isGlobal2, type2, index2));
        }
        private void DumpUnary(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: {2} = {3} {4}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), op, BuildVar(isGlobal1, type1, index1));
        }
        private void DumpBinary(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            DecodeOperand2(operand, out var isGlobal2, out var type2, out var index2);
            txt.AppendLine("{0}{1}: {2} = {3} {4}, {5}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), op, BuildVar(isGlobal1, type1, index1), BuildVar(isGlobal2, type2, index2));
        }
        private void DumpIToF(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: {2} = ITOF {3}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), BuildVar(isGlobal1, type1, index1));
        }
        private void DumpFToI(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: {2} = FTOI {3}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), BuildVar(isGlobal1, type1, index1));
        }
        private void DumpArgc(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.AppendLine("{0}{1}: {2} = ARGC", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index));
        }
        private void DumpArgv(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: {2} = ARGV {3}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), BuildVar(isGlobal1, type1, index1));
        }
        private void DumpAddr(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: {2} = ADDR {3}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), BuildVar(isGlobal1, type1, index1));
        }
        private void DumpPtrGet(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            DecodeOperand2(operand, out var isGlobal2, out var type2, out var index2);
            txt.AppendLine("{0}{1}: {2} = PTRGET {3}, {4}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), BuildVar(isGlobal1, type1, index1), BuildVar(isGlobal2, type2, index2));
        }
        private void DumpPtrSet(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            DecodeOperand2(operand, out var isGlobal2, out var type2, out var index2);
            txt.AppendLine("{0}{1}: PTRSET {2}, {3}, {4}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), BuildVar(isGlobal1, type1, index1), BuildVar(isGlobal2, type2, index2));
        }
        private void DumpJaggedPtr(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.Append("{0}{1}: {2} = JPTR", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index));
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
                    DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
                    txt.Append(BuildVar(isGlobal1, type1, index1));
                }
                if (i + 1 <= argNum) {
                    txt.Append(", ");
                    DecodeOperand2(operand, out var isGlobal2, out var type2, out var index2);
                    txt.Append(BuildVar(isGlobal2, type2, index2));
                }
            }
            txt.AppendLine();
        }
        private string BuildVar(bool isGlobal, TypeEnum type, int index)
        {
            if (type == TypeEnum.NotUse)
                return string.Empty;
            bool isConstOrTemp = index >= c_max_variable_table_size / 2;
            string name = isGlobal ? (isConstOrTemp ? "Const" : "Global") : (isConstOrTemp ? "Temp" : "Local");

            string val = string.Empty;
            if (isGlobal && isConstOrTemp) {
                foreach (var pair in m_ConstInfos) {
                    if (pair.Value.Type == type && pair.Value.Index == index) {
                        val = pair.Value.StrValue;
                        break;
                    }
                }
            }
            int cindex = isConstOrTemp ? c_max_variable_table_size - 1 - index : index;
            return string.Format("{0}[{1}]:{2}({3})", name, cindex, val, s_TypeNames[(int)type]);
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
                        s_Type2Ids.TryGetValue(type, out var ty);
                        if (IsGlobalVar(name)) {
                            var vinfo = new VarInfo { Name = name, Type = ty, Count = count };
                            AddVar(vinfo);

                            var info = new SemanticInfo { TargetOperation = TargetOperationEnum.VarAssign, TargetType = ty, TargetCount = count, TargetIndex = vinfo.Index };
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
                    s_Type2Ids.TryGetValue(type, out var ty);
                    if (IsGlobalVar(name)) {
                        var vinfo = new VarInfo { Name = name, Type = ty, InitValues = null };
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
                        CalcStructOffsetAndSize(struInfo);
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
                                    var info = new SemanticInfo { TargetType = TypeEnum.Int };
                                    CompileExpression(exp, codes, err, ref info);
                                    if (null == info.ResultValues) {
                                        if (TryGenConvert(codes, TypeEnum.Int, info, out var newInfo)) {
                                            info = newInfo;
                                        }
                                    }
                                    else {
                                        int index = AddConst(info.ResultType, info.ResultValues[0]);
                                        info.ResultIndex = index;
                                        info.IsGlobal = true;
                                        if (TryGenConvert(codes, TypeEnum.Int, info, out var newInfo)) {
                                            info = newInfo;
                                        }
                                    }
                                    //gen jmpifnot
                                    jmpIfNot = codes.Count;
                                    codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                                    codes.Add(EncodeOperand1(info.IsGlobal, info.ResultType, info.ResultIndex));
                                    CurBlock().ResetTempVars();
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
                            codes[jmp] = opcode | EncodeOffset(offset);
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
                            var info = new SemanticInfo { TargetType = TypeEnum.Int };
                            int loopContinue = codes.Count;
                            CompileExpression(exp, codes, err, ref info);
                            if (null == info.ResultValues) {
                                if (TryGenConvert(codes, TypeEnum.Int, info, out var newInfo)) {
                                    info = newInfo;
                                }
                            }
                            else {
                                int index = AddConst(info.ResultType, info.ResultValues[0]);
                                info.ResultIndex = index;
                                info.IsGlobal = true;
                                if (TryGenConvert(codes, TypeEnum.Int, info, out var newInfo)) {
                                    info = newInfo;
                                }
                            }
                            //gen jmpifnot
                            int jmpIfNot = codes.Count;
                            codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                            codes.Add(EncodeOperand1(info.IsGlobal, info.ResultType, info.ResultIndex));
                            CurBlock().ResetTempVars();
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
                                var info1 = new SemanticInfo();
                                CompileExpression(v, codes, err, ref info1);
                                var info2 = new SemanticInfo();
                                CompileExpression(beginExp, codes, err, ref info2);
                                if (null == info2.ResultValues) {
                                    if (TryGenConvert(codes, info1.ResultType, info2, out var newInfo)) {
                                        info2 = newInfo;
                                    }
                                }
                                else {
                                    int index = AddConst(info2.ResultType, info2.ResultValues[0]);
                                    info2.IsGlobal = true;
                                    info2.ResultIndex = index;
                                    if (TryGenConvert(codes, info1.ResultType, info2, out var newInfo)) {
                                        info2 = newInfo;
                                    }
                                }
                                //gen assignment
                                if (info1.ResultType == TypeEnum.Int) {
                                    codes.Add(EncodeOpcode(InsEnum.MOV, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                }
                                else if (info1.ResultType == TypeEnum.Float) {
                                    codes.Add(EncodeOpcode(InsEnum.MOVFLT, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                }
                                else {
                                    err.AppendFormat("Illegal loop, first argument must be int/float type, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                    err.AppendLine();
                                }
                                codes.Add(EncodeOperand1(info2.IsGlobal, info2.ResultType, info2.ResultIndex));
                                int loopContinue = codes.Count;
                                var info3 = new SemanticInfo();
                                CompileExpression(endExp, codes, err, ref info3);
                                if (null == info3.ResultValues) {
                                    if (TryGenConvert(codes, info1.ResultType, info3, out var newInfo)) {
                                        info3 = newInfo;
                                    }
                                }
                                else {
                                    int index = AddConst(info3.ResultType, info3.ResultValues[0]);
                                    info3.ResultIndex = index;
                                    info3.IsGlobal = true;
                                    if (TryGenConvert(codes, info1.ResultType, info3, out var newInfo)) {
                                        info3 = newInfo;
                                    }
                                }
                                //gen less equal than/great equal than
                                int tempIx = lexicalInfo.AllocTempInt();
                                if (info1.ResultType == TypeEnum.Int) {
                                    if (id == "loopd")
                                        codes.Add(EncodeOpcode(InsEnum.GE, false, TypeEnum.Int, tempIx));
                                    else
                                        codes.Add(EncodeOpcode(InsEnum.LE, false, TypeEnum.Int, tempIx));
                                }
                                else if (info1.ResultType == TypeEnum.Float) {
                                    if (id == "loopd")
                                        codes.Add(EncodeOpcode(InsEnum.GEFLT, false, TypeEnum.Int, tempIx));
                                    else
                                        codes.Add(EncodeOpcode(InsEnum.LEFLT, false, TypeEnum.Int, tempIx));
                                }
                                int opd1 = EncodeOperand1(info1.IsGlobal, info1.ResultType, info1.ResultIndex);
                                int opd2 = EncodeOperand2(info3.IsGlobal, info3.ResultType, info3.ResultIndex);
                                codes.Add(opd1 | opd2);
                                //gen jmpifnot
                                int jmpIfNot = codes.Count;
                                codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                                codes.Add(EncodeOperand1(false, TypeEnum.Int, tempIx));

                                lexicalInfo.ResetTempVars();

                                var breakFixes = new List<int>();
                                breakFixes.Add(jmpIfNot);
                                lexicalInfo.LoopContinue = loopContinue;
                                lexicalInfo.LoopBreakFixes = breakFixes;
                                CompileSyntaxInHook(funcCall, codes, err);

                                //gen inc/incv
                                if (incOne) {
                                    if (info1.ResultType == TypeEnum.Int) {
                                        codes.Add(EncodeOpcode(InsEnum.INC, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                    }
                                    else if (info1.ResultType == TypeEnum.Float) {
                                        codes.Add(EncodeOpcode(InsEnum.INCFLT, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                    }
                                }
                                else {
                                    var info4 = new SemanticInfo();
                                    CompileExpression(incExp, codes, err, ref info4);
                                    if (null == info4.ResultValues) {
                                        if (TryGenConvert(codes, info1.ResultType, info4, out var newInfo)) {
                                            info4 = newInfo;
                                        }
                                        if (info1.ResultType == TypeEnum.Int) {
                                            codes.Add(EncodeOpcode(InsEnum.INCV, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                        }
                                        else if (info1.ResultType == TypeEnum.Float) {
                                            codes.Add(EncodeOpcode(InsEnum.INCFLT, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                        }
                                        codes.Add(EncodeOperand1(info4.IsGlobal, info4.ResultType, info4.ResultIndex));
                                    }
                                    else {
                                        if (info4.ResultValues[0] == "1") {
                                            if (info1.ResultType == TypeEnum.Int) {
                                                codes.Add(EncodeOpcode(InsEnum.INC, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                            }
                                            else if (info1.ResultType == TypeEnum.Float) {
                                                codes.Add(EncodeOpcode(InsEnum.INCFLT, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                            }
                                        }
                                        else {
                                            int index = AddConst(info4.ResultType, info4.ResultValues[0]);
                                            info4.ResultIndex = index;
                                            info4.IsGlobal = true;
                                            if (TryGenConvert(codes, info1.ResultType, info4, out var newInfo)) {
                                                info4 = newInfo;
                                            }
                                            if (info1.ResultType == TypeEnum.Int) {
                                                codes.Add(EncodeOpcode(InsEnum.INCV, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                            }
                                            else if (info1.ResultType == TypeEnum.Float) {
                                                codes.Add(EncodeOpcode(InsEnum.INCFLT, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                            }
                                            codes.Add(EncodeOperand1(info4.IsGlobal, info4.ResultType, info4.ResultIndex));
                                        }
                                    }
                                    lexicalInfo.ResetTempVars();
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
                                SemanticInfo info = new SemanticInfo { TargetOperation = TargetOperationEnum.VarAssign, TargetType = vinfo.Type, TargetCount = vinfo.Count, TargetIndex = vinfo.Index };
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
                                    var info1 = new SemanticInfo { TargetType = TypeEnum.Int };
                                    CompileExpression(ixExp, codes, err, ref info1);
                                    var info2 = new SemanticInfo { TargetType = vinfo.Type, TargetCount = vinfo.Count, TargetIndex = vinfo.Index };
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
                                s_Type2Ids.TryGetValue(type, out var ty);
                                if (IsLocalVar(name)) {
                                    var vinfo = new VarInfo { Name = name, Type = ty, Count = count };
                                    AddVar(vinfo);

                                    var info = new SemanticInfo { TargetOperation = TargetOperationEnum.VarAssign, TargetType = ty, TargetCount = count, TargetIndex = vinfo.Index};
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
                        s_Type2Ids.TryGetValue(type, out var ty);
                        if (IsLocalVar(name)) {
                            var vinfo = new VarInfo { Name = name, Type = ty, Count = count, InitValues = null };
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
                            var info = new SemanticInfo { TargetType = TypeEnum.Int };
                            CompileExpression(exp, codes, err, ref info);
                            if (null == info.ResultValues) {
                                if (TryGenConvert(codes, TypeEnum.Int, info, out var newInfo)) {
                                    info = newInfo;
                                }
                            }
                            else {
                                int index = AddConst(info.ResultType, info.ResultValues[0]);
                                info.ResultIndex = index;
                                info.IsGlobal = true;
                                if (TryGenConvert(codes, TypeEnum.Int, info, out var newInfo)) {
                                    info = newInfo;
                                }
                            }
                            //gen jmpifnot
                            int jmpIfNot = codes.Count;
                            codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                            codes.Add(EncodeOperand1(info.IsGlobal, info.ResultType, info.ResultIndex));
                            CurBlock().ResetTempVars();
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
                            codes[jmpIfNot] = opcode | EncodeOffset(offset);
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
                            var info = new SemanticInfo { TargetType = TypeEnum.Int };
                            int loopContinue = codes.Count;
                            CompileExpression(exp, codes, err, ref info);
                            if (null == info.ResultValues) {
                                if (TryGenConvert(codes, TypeEnum.Int, info, out var newInfo)) {
                                    info = newInfo;
                                }
                            }
                            else {
                                int index = AddConst(info.ResultType, info.ResultValues[0]);
                                info.ResultIndex = index;
                                info.IsGlobal = true;
                                if (TryGenConvert(codes, TypeEnum.Int, info, out var newInfo)) {
                                    info = newInfo;
                                }
                            }
                            //gen jmpifnot
                            int jmpIfNot = codes.Count;
                            codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                            codes.Add(EncodeOperand1(info.IsGlobal, info.ResultType, info.ResultIndex));
                            CurBlock().ResetTempVars();
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
                                var info1 = new SemanticInfo();
                                CompileExpression(v, codes, err, ref info1);
                                var info2 = new SemanticInfo();
                                CompileExpression(beginExp, codes, err, ref info2);
                                if (null == info2.ResultValues) {
                                    if (TryGenConvert(codes, info1.ResultType, info2, out var newInfo)) {
                                        info2 = newInfo;
                                    }
                                }
                                else {
                                    int index = AddConst(info2.ResultType, info2.ResultValues[0]);
                                    info2.IsGlobal = true;
                                    info2.ResultIndex = index;
                                    if (TryGenConvert(codes, info1.ResultType, info2, out var newInfo)) {
                                        info2 = newInfo;
                                    }
                                }
                                //gen assignment
                                if (info1.ResultType == TypeEnum.Int) {
                                    codes.Add(EncodeOpcode(InsEnum.MOV, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                }
                                else if (info1.ResultType == TypeEnum.Float) {
                                    codes.Add(EncodeOpcode(InsEnum.MOVFLT, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                }
                                else {
                                    err.AppendFormat("Illegal loop, first argument must be int/float type, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                    err.AppendLine();
                                }
                                codes.Add(EncodeOperand1(info2.IsGlobal, info2.ResultType, info2.ResultIndex));
                                int loopContinue = codes.Count;
                                var info3 = new SemanticInfo();
                                CompileExpression(endExp, codes, err, ref info3);
                                if (null == info3.ResultValues) {
                                    if (TryGenConvert(codes, info1.ResultType, info3, out var newInfo)) {
                                        info3 = newInfo;
                                    }
                                }
                                else {
                                    int index = AddConst(info3.ResultType, info3.ResultValues[0]);
                                    info3.ResultIndex = index;
                                    info3.IsGlobal = true;
                                    if (TryGenConvert(codes, info1.ResultType, info3, out var newInfo)) {
                                        info3 = newInfo;
                                    }
                                }
                                //gen less equal than/great equal than
                                int tempIx = lexicalInfo.AllocTempInt();
                                if (info1.ResultType == TypeEnum.Int) {
                                    if (id == "loopd")
                                        codes.Add(EncodeOpcode(InsEnum.GE, false, TypeEnum.Int, tempIx));
                                    else
                                        codes.Add(EncodeOpcode(InsEnum.LE, false, TypeEnum.Int, tempIx));
                                }
                                else if (info1.ResultType == TypeEnum.Float) {
                                    if (id == "loopd")
                                        codes.Add(EncodeOpcode(InsEnum.GEFLT, false, TypeEnum.Int, tempIx));
                                    else
                                        codes.Add(EncodeOpcode(InsEnum.LEFLT, false, TypeEnum.Int, tempIx));
                                }
                                int opd1 = EncodeOperand1(info1.IsGlobal, info1.ResultType, info1.ResultIndex);
                                int opd2 = EncodeOperand2(info3.IsGlobal, info3.ResultType, info3.ResultIndex);
                                codes.Add(opd1 | opd2);
                                //gen jmpifnot
                                int jmpIfNot = codes.Count;
                                codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                                codes.Add(EncodeOperand1(false, TypeEnum.Int, tempIx));

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
                                    if (info1.ResultType == TypeEnum.Int) {
                                        codes.Add(EncodeOpcode(InsEnum.INC, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                    }
                                    else if(info1.ResultType == TypeEnum.Float) {
                                        codes.Add(EncodeOpcode(InsEnum.INCFLT, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                    }
                                }
                                else {
                                    var info4 = new SemanticInfo();
                                    CompileExpression(incExp, codes, err, ref info4);
                                    if (null == info4.ResultValues) {
                                        if (TryGenConvert(codes, info1.ResultType, info4, out var newInfo)) {
                                            info4 = newInfo;
                                        }
                                        if (info1.ResultType == TypeEnum.Int) {
                                            codes.Add(EncodeOpcode(InsEnum.INCV, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                        }
                                        else if (info1.ResultType == TypeEnum.Float) {
                                            codes.Add(EncodeOpcode(InsEnum.INCFLT, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                        }
                                        codes.Add(EncodeOperand1(info4.IsGlobal, info4.ResultType, info4.ResultIndex));
                                    }
                                    else {
                                        if (info4.ResultValues[0] == "1") {
                                            if (info1.ResultType == TypeEnum.Int) {
                                                codes.Add(EncodeOpcode(InsEnum.INC, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                            }
                                            else if (info1.ResultType == TypeEnum.Float) {
                                                codes.Add(EncodeOpcode(InsEnum.INCFLT, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                            }
                                        }
                                        else {
                                            int index = AddConst(info4.ResultType, info4.ResultValues[0]);
                                            info4.ResultIndex = index;
                                            info4.IsGlobal = true;
                                            if (TryGenConvert(codes, info1.ResultType, info4, out var newInfo)) {
                                                info4 = newInfo;
                                            }
                                            if (info1.ResultType == TypeEnum.Int) {
                                                codes.Add(EncodeOpcode(InsEnum.INCV, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                            }
                                            else if (info1.ResultType == TypeEnum.Float) {
                                                codes.Add(EncodeOpcode(InsEnum.INCFLT, info1.IsGlobal, info1.ResultType, info1.ResultIndex));
                                            }
                                            codes.Add(EncodeOperand1(info4.IsGlobal, info4.ResultType, info4.ResultIndex));
                                        }
                                    }
                                    lexicalInfo.ResetTempVars();
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
                            var info = new SemanticInfo { TargetType = TypeEnum.Int };
                            CompileExpression(exp, codes, err, ref info);
                            if (null == info.ResultValues) {
                                if (TryGenConvert(codes, TypeEnum.Int, info, out var newInfo)) {
                                    info = newInfo;
                                }
                            }
                            else {
                                int index = AddConst(info.ResultType, info.ResultValues[0]);
                                info.ResultIndex = index;
                                info.IsGlobal = true;
                                if (TryGenConvert(codes, TypeEnum.Int, info, out var newInfo)) {
                                    info = newInfo;
                                }
                            }
                            CurBlock().ResetTempVars();
                            //gen ret
                            codes.Add(EncodeOpcode(InsEnum.RET, info.IsGlobal, info.ResultType, info.ResultIndex));
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
                            var info = new SemanticInfo { TargetType = TypeEnum.Int };
                            CompileExpression(exp, codes, err, ref info);
                            if (null == info.ResultValues) {
                                if (TryGenConvert(codes, TypeEnum.Int, info, out var newInfo)) {
                                    info = newInfo;
                                }
                            }
                            else {
                                int index = AddConst(info.ResultType, info.ResultValues[0]);
                                info.ResultIndex = index;
                                info.IsGlobal = true;
                                if (TryGenConvert(codes, TypeEnum.Int, info, out var newInfo)) {
                                    info = newInfo;
                                }
                            }
                            CurBlock().ResetTempVars();
                            //gen ret
                            codes.Add(EncodeOpcode(InsEnum.RET, info.IsGlobal, info.ResultType, info.ResultIndex));
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
            var stmData = comp as Dsl.StatementData;
            if (null != stmData) {
                string id = stmData.GetId();
                if (id == "?") {
                    //cond ? exp1 : exp2 <= ?(cond){exp1;}:{exp2;}
                    bool handled = false;
                    if (stmData.GetFunctionNum() == 2) {
                        var ifPart = stmData.First.AsFunction;
                        var elsePart = stmData.Second.AsFunction;
                        if (null != ifPart && null != elsePart && ifPart.IsHighOrder && ifPart.LowerOrderFunction.GetParamNum() == 1 && ifPart.GetParamNum() == 1 && !elsePart.IsHighOrder && elsePart.GetParamNum() == 1) {
                            var ifExp = ifPart.LowerOrderFunction.GetParam(0);
                            var exp1 = ifPart.GetParam(0);
                            var exp2 = elsePart.GetParam(0);
                            var info = new SemanticInfo { TargetType = TypeEnum.Int };
                            CompileExpression(ifExp, codes, err, ref info);
                            var tinfo = semanticInfo;
                            if (semanticInfo.TargetOperation != TargetOperationEnum.VarAssign) {
                                if (tinfo.TargetType == TypeEnum.NotUse)
                                    tinfo.TargetType = TypeEnum.Int;
                                int tmpIndex = -1;
                                switch (tinfo.TargetType) {
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
                                Debug.Assert(tmpIndex >= 0);
                                tinfo.TargetOperation = TargetOperationEnum.VarAssign;
                                tinfo.TargetIsGlobal = false;
                                tinfo.TargetCount = 0;
                                tinfo.TargetIndex = tmpIndex;

                                tinfo.ResultType = tinfo.TargetType;
                                tinfo.ResultCount = 0;
                                tinfo.ResultIndex = tmpIndex;
                            }
                            semanticInfo = tinfo;
                            var sinfo1 = tinfo;
                            var tcodes1 = new List<int>();
                            CompileExpression(exp1, tcodes1, err, ref sinfo1);
                            var sinfo2 = tinfo;
                            var tcodes2 = new List<int>();
                            CompileExpression(exp2, tcodes2, err, ref sinfo2);
                            if (null != info.ResultValues) {
                                bool cond = false;
                                if (info.ResultType == TypeEnum.Int || info.ResultType == TypeEnum.String) {
                                    long.TryParse(info.ResultValues[0], out var v);
                                    cond = v != 0;
                                }
                                else if (info.ResultType == TypeEnum.Float) {
                                    double.TryParse(info.ResultValues[0], out var v);
                                    cond = v != 0;
                                }
                                if (cond) {
                                    codes.AddRange(tcodes1);
                                }
                                else {
                                    codes.AddRange(tcodes2);
                                }
                            }
                            else {
                                if (TryGenConvert(codes, TypeEnum.Int, info, out var newInfo)) {
                                    info = newInfo;
                                }
                                //gen jmpifnot
                                int jmpIfNot = codes.Count;
                                codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                                codes.Add(EncodeOperand1(info.IsGlobal, info.ResultType, info.ResultIndex));

                                codes.AddRange(tcodes1);
                                int jmp = codes.Count;
                                codes.Add(EncodeOpcode(InsEnum.JMP));

                                //fix jmpIfNot offset
                                int elseTarget = codes.Count;
                                int offset = elseTarget - jmpIfNot;
                                int opcode = codes[jmpIfNot];
                                codes[jmpIfNot] = opcode | EncodeOffset(offset);

                                codes.AddRange(tcodes2);

                                //fix jmp offset
                                int exitTarget = codes.Count;
                                offset = exitTarget - jmp;
                                opcode = codes[jmp];
                                codes[jmp] = opcode | EncodeOffset(offset);
                            }
                            CurBlock().ResetTempVars();
                            handled = true;
                        }
                    }
                    if(!handled) {
                        err.AppendFormat("Illegal condition expression, expect 'cond ? exp1 : exp2', code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else {
                    err.AppendFormat("Unknown expression syntax, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                }
            }
            else {
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
                                    for (int i = 0; i < callData.GetParamNum() && i < semanticInfo.TargetCount; ++i) {
                                        var p = callData.GetParam(i);
                                        var sinfo = new SemanticInfo { TargetType = semanticInfo.TargetType };
                                        CompileExpression(p, codes, err, ref sinfo);
                                        TryGenArrMov(semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex + i, codes, sinfo, err, p);
                                    }
                                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                                    semanticInfo.ResultType = semanticInfo.TargetType;
                                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                                    semanticInfo.ResultValues = null;
                                }
                            }
                            else {
                                int tmpIndex = -1;
                                TypeEnum type = TypeEnum.NotUse;
                                for (int i = 0; i < callData.GetParamNum(); ++i) {
                                    var p = callData.GetParam(i);
                                    var sinfo = new SemanticInfo { TargetType = semanticInfo.TargetType };
                                    CompileExpression(p, codes, err, ref sinfo);
                                    if (i == 0) {
                                        type = sinfo.TargetType;
                                        if (type == TypeEnum.Int) {
                                            tmpIndex = CurBlock().AllocTempIntArray(callData.GetParamNum());
                                        }
                                        else if (type == TypeEnum.Float) {
                                            tmpIndex = CurBlock().AllocTempFloatArray(callData.GetParamNum());
                                        }
                                        else if (type == TypeEnum.String) {
                                            tmpIndex = CurBlock().AllocTempFloatArray(callData.GetParamNum());
                                        }
                                    }
                                    TryGenArrMov(false, TypeEnum.Int, tmpIndex + i, codes, sinfo, err, p);
                                }
                                semanticInfo.IsGlobal = false;
                                semanticInfo.ResultType = type;
                                semanticInfo.ResultCount = callData.GetParamNum();
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
                        ApiInfo apiInfo;
                        bool apiExists = m_Apis.TryGetValue(op, out apiInfo);
                        int num = callData.GetParamNum();
                        var sinfos = new List<SemanticInfo>();
                        if(op == "expect") {
                            if (!callData.IsHighOrder && callData.GetParamNum() == 2) {
                                string type = callData.GetParamId(0);
                                if (s_Type2Ids.TryGetValue(type, out var ty)) {
                                    semanticInfo.TargetType = ty;
                                }
                                Dsl.ISyntaxComponent param = callData.GetParam(1);
                                CompileExpression(param, codes, err, ref semanticInfo);
                            }
                            else {
                                err.AppendFormat("expect 'expect(type, exp)', code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                err.AppendLine();
                            }
                            return;
                        }
                        else if (op == "struct") {
                            if (!callData.IsHighOrder && callData.GetParamNum() == 2) {
                                Dsl.ISyntaxComponent param = callData.GetParam(0);
                                var info = new SemanticInfo { TargetType = TypeEnum.Int };
                                CompileExpression(param, codes, err, ref info);
                                if (info.ResultType != TypeEnum.Int) {
                                    err.AppendFormat("struct(addr, exp), addr must be integer type, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                    err.AppendLine();
                                }
                                var exp = callData.GetParam(1);
                                TryGenStruct(exp, codes, info, err, ref semanticInfo);
                                CurBlock().ResetTempVars();
                            }
                            else {
                                err.AppendFormat("expect 'struct(addr, exp)', code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                err.AppendLine();
                            }
                            return;
                        }
                        else if (callData.IsOperatorParamClass() && op == "??") {
                            //exp1 ?? exp2
                            bool handled = false;
                            if (num == 2) {
                                var exp1 = callData.GetParam(0);
                                var exp2 = callData.GetParam(1);
                                var tinfo = semanticInfo;
                                if (semanticInfo.TargetOperation != TargetOperationEnum.VarAssign) {
                                    if (tinfo.TargetType == TypeEnum.NotUse)
                                        tinfo.TargetType = TypeEnum.Int;
                                    int tmpIndex = -1;
                                    switch (tinfo.TargetType) {
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
                                    Debug.Assert(tmpIndex >= 0);
                                    tinfo.TargetOperation = TargetOperationEnum.VarAssign;
                                    tinfo.TargetIsGlobal = false;
                                    tinfo.TargetCount = 0;
                                    tinfo.TargetIndex = tmpIndex;

                                    tinfo.ResultType = tinfo.TargetType;
                                    tinfo.ResultCount = 0;
                                    tinfo.ResultIndex = tmpIndex;
                                }
                                semanticInfo = tinfo;
                                var sinfo1 = tinfo;
                                CompileExpression(exp1, codes, err, ref sinfo1);
                                var sinfo2 = tinfo;
                                var tcodes = new List<int>();
                                CompileExpression(exp2, tcodes, err, ref sinfo2);
                                if (null != sinfo1.ResultValues) {
                                    bool cond = false;
                                    if (sinfo1.ResultType == TypeEnum.Int || sinfo1.ResultType == TypeEnum.String) {
                                        long.TryParse(sinfo1.ResultValues[0], out var v);
                                        cond = v != 0;
                                    }
                                    else if (sinfo1.ResultType == TypeEnum.Float) {
                                        double.TryParse(sinfo1.ResultValues[0], out var v);
                                        cond = v != 0;
                                    }
                                    if (!cond) {
                                        codes.AddRange(tcodes);
                                    }
                                }
                                else {
                                    if (TryGenConvert(codes, TypeEnum.Int, sinfo1, out var newInfo)) {
                                        sinfo1 = newInfo;
                                    }
                                    //gen jmpifnot
                                    int jmpIfNot = codes.Count;
                                    codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                                    codes.Add(EncodeOperand1(sinfo1.IsGlobal, sinfo1.ResultType, sinfo1.ResultIndex));

                                    int jmp = codes.Count;
                                    codes.Add(EncodeOpcode(InsEnum.JMP));

                                    //fix jmpIfNot offset
                                    int elseTarget = codes.Count;
                                    int offset = elseTarget - jmpIfNot;
                                    int opcode = codes[jmpIfNot];
                                    codes[jmpIfNot] = opcode | EncodeOffset(offset);

                                    codes.AddRange(tcodes);

                                    //fix jmp offset
                                    int exitTarget = codes.Count;
                                    offset = exitTarget - jmp;
                                    opcode = codes[jmp];
                                    codes[jmp] = opcode | EncodeOffset(offset);
                                }
                                CurBlock().ResetTempVars();
                                handled = true;
                            }
                            if (!handled) {
                                err.AppendFormat("Illegal condition expression, expect 'exp1 ?? exp2', code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                err.AppendLine();
                            }
                            return;
                        }
                        else if (callData.IsOperatorParamClass() && (op == "&&" || op == "||")) {
                            //Short Circuit
                            if (num == 2) {
                                var param1 = callData.GetParam(0);
                                var sinfo1 = new SemanticInfo();
                                var tcodes1 = new List<int>();
                                CompileExpression(param1, tcodes1, err, ref sinfo1);
                                var param2 = callData.GetParam(0);
                                var sinfo2 = new SemanticInfo();
                                var tcodes2 = new List<int>();
                                CompileExpression(param2, tcodes2, err, ref sinfo2);
                                if (null == sinfo1.ResultValues && null == sinfo2.ResultValues) {
                                    if (TryGenConvert(tcodes1, TypeEnum.Int, sinfo1, out var newInfo1)) {
                                        sinfo1 = newInfo1;
                                    }
                                    if (TryGenConvert(tcodes2, TypeEnum.Int, sinfo2, out var newInfo2)) {
                                        sinfo2 = newInfo2;
                                    }
                                    codes.AddRange(tcodes1);
                                    int jmp = codes.Count;
                                    if (op == "&&") {
                                        codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                                    }
                                    else {
                                        codes.Add(EncodeOpcode(InsEnum.JMPIF));
                                    }
                                    codes.Add(EncodeOperand1(sinfo1.IsGlobal, sinfo1.ResultType, sinfo1.ResultIndex));
                                    codes.AddRange(tcodes2);
                                    sinfos.Add(sinfo1);
                                    sinfos.Add(sinfo2);

                                    int target = codes.Count;
                                    int offset = target - jmp;
                                    int opcode = codes[jmp];
                                    codes[jmp] = opcode | EncodeOffset(offset);
                                }
                                else if (null == sinfo1.ResultValues) {
                                    if (TryGenConvert(tcodes1, TypeEnum.Int, sinfo1, out var newInfo1)) {
                                        sinfo1 = newInfo1;
                                    }
                                    int index = AddConst(sinfo2.ResultType, sinfo2.ResultValues[0]);
                                    sinfo2.ResultIndex = index;
                                    sinfo2.IsGlobal = true;
                                    if (TryGenConvert(tcodes2, TypeEnum.Int, sinfo2, out var newInfo2)) {
                                        sinfo2 = newInfo2;
                                    }
                                    codes.AddRange(tcodes1);
                                    codes.AddRange(tcodes2);
                                    sinfos.Add(sinfo1);
                                    sinfos.Add(sinfo2);
                                }
                                else if (null == sinfo2.ResultValues) {
                                    int index = AddConst(sinfo1.ResultType, sinfo1.ResultValues[0]);
                                    sinfo1.ResultIndex = index;
                                    sinfo1.IsGlobal = true;
                                    if (TryGenConvert(tcodes1, TypeEnum.Int, sinfo1, out var newInfo1)) {
                                        sinfo1 = newInfo1;
                                    }
                                    if (TryGenConvert(tcodes2, TypeEnum.Int, sinfo2, out var newInfo2)) {
                                        sinfo2 = newInfo2;
                                    }
                                    codes.AddRange(tcodes1);
                                    int jmp = codes.Count;
                                    if (op == "&&") {
                                        codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                                    }
                                    else {
                                        codes.Add(EncodeOpcode(InsEnum.JMPIF));
                                    }
                                    int opd = EncodeOperand1(sinfo1.IsGlobal, sinfo1.ResultType, sinfo1.ResultIndex);
                                    codes.Add(opd);
                                    codes.AddRange(tcodes2);
                                    sinfos.Add(sinfo1);
                                    sinfos.Add(sinfo2);

                                    int target = codes.Count;
                                    int offset = target - jmp;
                                    int opcode = codes[jmp];
                                    codes[jmp] = opcode | EncodeOffset(offset);
                                }
                                else {
                                    codes.AddRange(tcodes1);
                                    codes.AddRange(tcodes2);
                                    sinfos.Add(sinfo1);
                                    sinfos.Add(sinfo2);
                                }
                            }
                            else {
                                err.AppendFormat("operator '&&' or '||' must have two arguments, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                err.AppendLine();
                            }
                        }
                        else {
                            for (int i = 0; i < num; ++i) {
                                var param = callData.GetParam(i);
                                var sinfo = new SemanticInfo { TargetType = TypeEnum.Int };
                                if (apiExists) {
                                    Debug.Assert(null != apiInfo);
                                    if (i < apiInfo.ParamTypes.Count) {
                                        sinfo.TargetType = apiInfo.ParamTypes[i];
                                    }
                                    else if (apiInfo.Params == ParamsEnum.Ints) {
                                        sinfo.TargetType = TypeEnum.Int;
                                    }
                                    else if (apiInfo.Params == ParamsEnum.Floats) {
                                        sinfo.TargetType = TypeEnum.Float;
                                    }
                                    else if (apiInfo.Params == ParamsEnum.Strings) {
                                        sinfo.TargetType = TypeEnum.String;
                                    }
                                }
                                CompileExpression(param, codes, err, ref sinfo);
                                sinfos.Add(sinfo);
                            }
                        }
                        if (callData.IsOperatorParamClass()) {
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
                            if (op == "int2flt") {
                                TryGenInt2Flt(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "int2str") {
                                TryGenInt2Str(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "flt2int") {
                                TryGenFlt2Int(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "flt2str") {
                                TryGenFlt2Str(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "str2int") {
                                TryGenStr2Int(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "str2flt") {
                                TryGenStr2Flt(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "itof") {
                                TryGenIToF(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "ftoi") {
                                TryGenFToI(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "argc") {
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
                                    semanticInfo.ResultType = TypeEnum.Float;
                                else
                                    semanticInfo.ResultType = TypeEnum.Int;
                                semanticInfo.ResultValues = new List<string>();
                                semanticInfo.ResultValues.Add(val);
                                TryGenConst(val, codes, err, comp, ref semanticInfo);
                                break;
                            case Dsl.ValueData.STRING_TOKEN:
                                semanticInfo.ResultType = TypeEnum.String;
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
                            if (!s_FieldTypeNames.Contains(ftype) && !TryGetStruct(ftype, out var stru)) {
                                err.AppendFormat("Unknown field type '{0}' must be defined first than '{1}', code:{2}, line:{3}", ftype, name, p.ToScriptString(false), p.GetLine());
                                err.AppendLine();
                            }
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
        private void CalcStructOffsetAndSize(StructInfo struInfo)
        {
            int size = 0;
            foreach(var field in struInfo.Fields) {
                string type = field.Type;
                field.Offset = size;
                if (field.ArrayOrPtrs.Count > 0 && field.ArrayOrPtrs[0] <= 0) {
                    field.Size = sizeof(long);
                }
                else if (type == "int8" || type == "uint8" || type == "char" || type == "byte") {
                    field.Size = sizeof(byte);
                }
                else if (type == "int16" || type == "uint16" || type == "short" || type == "ushort") {
                    field.Size = sizeof(short);
                }
                else if (type == "int32" || type == "uint32" || type == "int" || type == "uint") {
                    field.Size = sizeof(int);
                }
                else if (type == "int64" || type == "uint64" || type == "long" || type == "ulong") {
                    field.Size = sizeof(long);
                }
                else if (type == "float") {
                    field.Size = sizeof(float);
                }
                else if (type == "double") {
                    field.Size = sizeof(double);
                }
                else {
                    if (TryGetStruct(field.Type, out var refStruInfo)) {
                        field.Size = refStruInfo.Size;
                    }
                    else {
                        field.Size = 0;
                    }
                }
                int count = 1;
                if (field.ArrayOrPtrs.Count > 0) {
                    foreach (var v in field.ArrayOrPtrs) {
                        if (v > 0)
                            count *= v;
                    }
                }
                field.TotalSize = field.Size * count;
                size += field.TotalSize;
            }
            struInfo.Size = size;
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
                    string type = ParseFieldType(funcData.GetParam(0), err, arrOrPtrs);
                    if (arrOrPtrs.Count > 0) {
                        err.AppendFormat("Pointer in struct can only pointer to struct or base type, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    arrOrPtrs.Add(0);
                    return type;
                }
                else if (null != funcData && funcData.IsBracketParamClass() && funcData.GetParamNum() == 1) {
                    string type;
                    if(funcData.IsHighOrder)
                        type = ParseFieldType(funcData.LowerOrderFunction, err, arrOrPtrs);
                    else
                        type = funcData.GetId();
                    int.TryParse(funcData.GetParamId(0), out var count);
                    arrOrPtrs.Add(count);
                    return type;
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
        private bool TryGetStruct(string name, out StructInfo struInfo)
        {
            if(m_StructInfos.TryGetValue(name, out struInfo)) {
                return true;
            }
            else if (m_PredefinedStructInfos.TryGetValue(name, out struInfo)) {
                return true;
            }
            return false;
        }
        private bool CalcStructExpressionOffsets(Dsl.ISyntaxComponent exp, StringBuilder err, List<int> offsets, out int lastSize, out StructInfo struInfo, out List<int> fieldIndexes)
        {
            bool success = true;
            lastSize = 0;
            struInfo = null;
            fieldIndexes = null;
            var valData = exp as Dsl.ValueData;
            if (null != valData) {
                string name = valData.GetId();
                TryGetStruct(name, out struInfo);

                offsets.Add(0);
                lastSize = sizeof(long);
            }
            else {
                var funcData = exp as Dsl.FunctionData;
                if (null != funcData) {
                    int num = funcData.GetParamNum();
                    if (funcData.IsHighOrder) {
                        success = success && CalcStructExpressionOffsets(funcData.LowerOrderFunction, err, offsets, out lastSize, out struInfo, out fieldIndexes);
                    }
                    else if (funcData.IsMemberParamClass()) {
                        string name = funcData.GetId();
                        TryGetStruct(name, out struInfo);

                        offsets.Add(0);
                    }
                    if (funcData.IsMemberParamClass()) {
                        string member = funcData.GetParamId(0);
                        if (null != struInfo) {
                            int fieldIndex = struInfo.Fields.FindIndex(fi => fi.Name == member);
                            if (fieldIndex >= 0) {
                                fieldIndexes = new List<int>();
                                fieldIndexes.Add(fieldIndex);

                                var field = struInfo.Fields[fieldIndex];
                                offsets[offsets.Count - 1] += field.Offset;
                                lastSize = field.TotalSize;
                            }
                            else {
                                err.AppendFormat("Can't find the member '{0}' from the struct '{1}', code:{2}, line:{3}", member, struInfo.Name, exp.ToScriptString(false), exp.GetLine());
                                err.AppendLine();
                                success = false;
                            }
                        }
                        else {
                            err.AppendFormat("Can't resolve the struct for the member '{0}', code:{1}, line:{2}", member, exp.ToScriptString(false), exp.GetLine());
                            err.AppendLine();
                            success = false;
                        }
                    }
                    else if (funcData.IsParenthesisParamClass() && num == 1 && funcData.GetId() == "ptr") {
                        success = success && CalcStructExpressionOffsets(funcData.GetParam(0), err, offsets, out lastSize, out struInfo, out fieldIndexes);

                        var field = struInfo.Fields[fieldIndexes[0]];
                        TryGetStruct(field.Type, out struInfo);
                        fieldIndexes = null;

                        offsets.Add(0);
                        lastSize = field.TotalSize;
                    }
                    else if(funcData.IsBracketParamClass() && num == 1) {
                        int.TryParse(funcData.GetParamId(0), out var index);
                        fieldIndexes.Add(index);

                        var field = struInfo.Fields[fieldIndexes[0]];
                        int arrNum = 1;
                        int addNum = 0;
                        for(int i = field.ArrayOrPtrs.Count - 1; i >= 0; --i) {
                            int arrSize = field.ArrayOrPtrs[i];
                            arrNum *= arrSize;
                            if (i < fieldIndexes.Count) {
                                int fix = fieldIndexes[i];
                                addNum += fix * arrNum;
                                break;
                            }
                        }
                        offsets[offsets.Count - 1] += field.Size * addNum;
                        lastSize = field.Size * addNum;
                    }
                    else {
                        err.AppendFormat("Struct exp must be 'ptr(exp)' or 'exp[ix]' or 'exp.field', and exp is a recursive struct exp, code:{0}, line:{1}", exp.ToScriptString(false), exp.GetLine());
                        err.AppendLine();
                        success = false;
                    }
                }
                else {
                    err.AppendFormat("Unknown syntax in struct exp, code:{0}, line:{1}", exp.ToScriptString(false), exp.GetLine());
                    err.AppendLine();
                    success = false;
                }
            }
            return success;
        }

        private void TryGenStruct(Dsl.ISyntaxComponent exp, List<int> codes, SemanticInfo info, StringBuilder err, ref SemanticInfo semanticInfo)
        {
            List<int> offsets = new List<int>();
            if(CalcStructExpressionOffsets(exp, err, offsets, out var lastSize, out var struInfo, out var fieldIndexes)) {
                var opds = new List<SemanticInfo>();
                opds.Add(info);
                var cinfo0 = new SemanticInfo { ResultType = TypeEnum.Int, ResultCount = 0, ResultValues = new List<string> { lastSize.ToString() } };
                opds.Add(cinfo0);
                foreach (var offset in offsets) {
                    var cinfo = new SemanticInfo { ResultType = TypeEnum.Int, ResultCount = 0, ResultValues = new List<string> { offset.ToString() } };
                    opds.Add(cinfo);
                }
                TryGenJaggedPtr(codes, opds, err, exp, ref semanticInfo);
            }
        }

        private void TryGenConst(string val, List<int> codes, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assignment a value to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    var info = semanticInfo;
                    int index = AddConst(info.ResultType, val);
                    info.ResultIndex = index;
                    info.IsGlobal = true;
                    if(TryGenConvert(codes, semanticInfo.TargetType, info, out var newInfo)) {
                        info = newInfo;
                    }
                    if (semanticInfo.TargetType == TypeEnum.Int)
                        codes.Add(EncodeOpcode(InsEnum.MOV, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    else if (semanticInfo.TargetType == TypeEnum.Float)
                        codes.Add(EncodeOpcode(InsEnum.MOVFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    else if (semanticInfo.TargetType == TypeEnum.String)
                        codes.Add(EncodeOpcode(InsEnum.MOVSTR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    codes.Add(EncodeOperand1(info.IsGlobal, info.ResultType, info.ResultIndex));

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
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
                    var vinfo2 = GetVarInfo(id);
                    if (null == vinfo2) {
                        err.AppendFormat("Undefined var '{0}', code:{1}, line:{2}", id, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                        return;
                    }
                    if (semanticInfo.TargetCount > 0 || vinfo2.Count > 0) {
                        if (semanticInfo.TargetCount != vinfo2.Count) {
                            err.AppendFormat("Can't assignment array with different size, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        for (int i = 0; i < semanticInfo.TargetCount && i < vinfo2.Count; ++i) {
                            //gen write result
                            if (semanticInfo.TargetType == vinfo2.Type) {
                                if (semanticInfo.TargetType == TypeEnum.Int)
                                    codes.Add(EncodeOpcode(InsEnum.MOV, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex + i));
                                else if (semanticInfo.TargetType == TypeEnum.Float)
                                    codes.Add(EncodeOpcode(InsEnum.MOVFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex + i));
                                else if (semanticInfo.TargetType == TypeEnum.String)
                                    codes.Add(EncodeOpcode(InsEnum.MOVSTR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex + i));
                                codes.Add(EncodeOperand1(vinfo2.IsGlobal, vinfo2.Type, vinfo2.Index + i));
                            }
                            else {
                                var info = new SemanticInfo { IsGlobal = false, ResultType = semanticInfo.TargetType };
                                if (semanticInfo.TargetType == TypeEnum.Int) {
                                    info.ResultIndex = CurBlock().AllocTempIntArray(vinfo2.Count);
                                }
                                else if(semanticInfo.TargetType == TypeEnum.Float) {
                                    info.ResultIndex = CurBlock().AllocTempFloatArray(vinfo2.Count);
                                }
                                else if(semanticInfo.TargetType == TypeEnum.String) {
                                    info.ResultIndex = CurBlock().AllocTempStringArray(vinfo2.Count);
                                }

                                if(TryGenConvert(codes, info.ResultType, vinfo2, out var newInfo)) {
                                    if (semanticInfo.TargetType == TypeEnum.Int)
                                        codes.Add(EncodeOpcode(InsEnum.MOV, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex + i));
                                    else if (semanticInfo.TargetType == TypeEnum.Float)
                                        codes.Add(EncodeOpcode(InsEnum.MOVFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex + i));
                                    else if (semanticInfo.TargetType == TypeEnum.String)
                                        codes.Add(EncodeOpcode(InsEnum.MOVSTR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex + i));
                                    codes.Add(EncodeOperand1(newInfo.IsGlobal, newInfo.ResultType, newInfo.ResultIndex + i));
                                }
                            }
                        }
                    }
                    else {
                        //gen write result
                        if (semanticInfo.TargetType == vinfo2.Type) {
                            if (semanticInfo.TargetType == TypeEnum.Int)
                                codes.Add(EncodeOpcode(InsEnum.MOV, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                            else if (semanticInfo.TargetType == TypeEnum.Float)
                                codes.Add(EncodeOpcode(InsEnum.MOVFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                            else if (semanticInfo.TargetType == TypeEnum.String)
                                codes.Add(EncodeOpcode(InsEnum.MOVSTR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                            codes.Add(EncodeOperand1(vinfo2.IsGlobal, vinfo2.Type, vinfo2.Index));
                        }
                        else {
                            var info = new SemanticInfo { IsGlobal = false, ResultType = semanticInfo.TargetType };
                            if (semanticInfo.TargetType == TypeEnum.Int) {
                                info.ResultIndex = CurBlock().AllocTempInt();
                            }
                            else if (semanticInfo.TargetType == TypeEnum.Float) {
                                info.ResultIndex = CurBlock().AllocTempFloat();
                            }
                            else if (semanticInfo.TargetType == TypeEnum.String) {
                                info.ResultIndex = CurBlock().AllocTempString();
                            }

                            if (TryGenConvert(codes, info.ResultType, vinfo2, out var newInfo)) {
                                if (semanticInfo.TargetType == TypeEnum.Int)
                                    codes.Add(EncodeOpcode(InsEnum.MOV, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                                else if (semanticInfo.TargetType == TypeEnum.Float)
                                    codes.Add(EncodeOpcode(InsEnum.MOVFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                                else if (semanticInfo.TargetType == TypeEnum.String)
                                    codes.Add(EncodeOpcode(InsEnum.MOVSTR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                                codes.Add(EncodeOperand1(newInfo.IsGlobal, newInfo.ResultType, newInfo.ResultIndex));
                            }
                        }
                    }

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
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
                semanticInfo.ResultType = vinfo.Type;
                semanticInfo.ResultCount = vinfo.Count;
                semanticInfo.ResultIndex = vinfo.Index;
                semanticInfo.ResultValues = null;
            }
        }
        private void TryGenArrMov(bool isGlobal, TypeEnum type, int vindex, List<int> codes, SemanticInfo info, StringBuilder err, Dsl.ISyntaxComponent comp)
        {
            //gen mov
            if (null == info.ResultValues) {
                if (TryGenConvert(codes, type, info, out var newInfo)) {
                    info = newInfo;
                }
            }
            else {
                int index = AddConst(info.ResultType, info.ResultValues[0]);
                info.ResultIndex = index;
                info.IsGlobal = true;
                if (TryGenConvert(codes, type, info, out var newInfo)) {
                    info = newInfo;
                }
            }

            if (type == TypeEnum.Int)
                codes.Add(EncodeOpcode(InsEnum.MOV, isGlobal, type, vindex));
            else if (type == TypeEnum.Float)
                codes.Add(EncodeOpcode(InsEnum.MOVFLT, isGlobal, type, vindex));
            else if (type == TypeEnum.String)
                codes.Add(EncodeOpcode(InsEnum.MOVSTR, isGlobal, type, vindex));
            codes.Add(EncodeOperand1(info.IsGlobal, info.ResultType, info.ResultIndex));
        }
        private void TryGenArrGet(string id, List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            Debug.Assert(opds.Count == 1);
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    var vinfo2 = GetVarInfo(id);
                    if (null == vinfo2) {
                        err.AppendFormat("Undefined var '{0}', code:{1}, line:{2}", id, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                        return;
                    }

                    var info2 = opds[0];
                    if (null == info2.ResultValues) {
                        if (TryGenConvert(codes, TypeEnum.Int, info2, out var newInfo2)) {
                            info2 = newInfo2;
                        }
                    }
                    else {
                        int index = AddConst(info2.ResultType, info2.ResultValues[0]);
                        info2.ResultIndex = index;
                        info2.IsGlobal = true;
                        if (TryGenConvert(codes, TypeEnum.Int, info2, out var newInfo)) {
                            info2 = newInfo;
                        }
                    }
                    //gen write result
                    SemanticInfo rinfo = new SemanticInfo();
                    if (semanticInfo.TargetType == vinfo2.Type) {
                        if (semanticInfo.TargetType == TypeEnum.Int)
                            codes.Add(EncodeOpcode(InsEnum.ARRGET, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        else if (semanticInfo.TargetType == TypeEnum.Float)
                            codes.Add(EncodeOpcode(InsEnum.ARRGETFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        else if (semanticInfo.TargetType == TypeEnum.String)
                            codes.Add(EncodeOpcode(InsEnum.ARRGETSTR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    }
                    else {
                        int tmpIndex = -1;
                        switch (vinfo2.Type) {
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
                            rinfo.IsGlobal = false;
                            rinfo.ResultType = semanticInfo.TargetType;
                            rinfo.ResultCount = 0;
                            rinfo.ResultIndex = tmpIndex;
                            rinfo.ResultValues = null;

                            if (rinfo.ResultType == TypeEnum.Int)
                                codes.Add(EncodeOpcode(InsEnum.ARRGET, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                            else if (rinfo.ResultType == TypeEnum.Float)
                                codes.Add(EncodeOpcode(InsEnum.ARRGETFLT, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                            else if (rinfo.ResultType == TypeEnum.String)
                                codes.Add(EncodeOpcode(InsEnum.ARRGETSTR, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                        }
                    }
                    int opd1 = EncodeOperand1(vinfo2.IsGlobal, vinfo2.Type, vinfo2.Index);
                    int opd2 = EncodeOperand2(info2.IsGlobal, info2.ResultType, info2.ResultIndex);
                    codes.Add(opd1 | opd2);
                    if (semanticInfo.TargetType != vinfo2.Type) {
                        TryGenConvert(codes, semanticInfo, rinfo);
                    }

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
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
                int tmpIndex = -1;
                switch (vinfo.Type) {
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
                    semanticInfo.ResultType = vinfo.Type;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;

                    var info2 = opds[0];
                    if (null == info2.ResultValues) {
                        if (TryGenConvert(codes, TypeEnum.Int, info2, out var newInfo2)) {
                            info2 = newInfo2;
                        }
                    }
                    else {
                        int index = AddConst(info2.ResultType, info2.ResultValues[0]);
                        info2.ResultIndex = index;
                        info2.IsGlobal = true;
                        if (TryGenConvert(codes, TypeEnum.Int, info2, out var newInfo)) {
                            info2 = newInfo;
                        }
                    }
                    //gen write result
                    if (semanticInfo.ResultType == TypeEnum.Int)
                        codes.Add(EncodeOpcode(InsEnum.ARRGET, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    else if (semanticInfo.ResultType == TypeEnum.Float)
                        codes.Add(EncodeOpcode(InsEnum.ARRGETFLT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    else if (semanticInfo.ResultType == TypeEnum.String)
                        codes.Add(EncodeOpcode(InsEnum.ARRGETSTR, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    int opd1 = EncodeOperand1(vinfo.IsGlobal, vinfo.Type, vinfo.Index);
                    int opd2 = EncodeOperand2(info2.IsGlobal, info2.ResultType, info2.ResultIndex);
                    codes.Add(opd1 | opd2);
                }
            }
        }
        private void TryGenArrSet(VarInfo vinfo, List<int> codes, SemanticInfo info1, SemanticInfo info2, StringBuilder err, Dsl.ISyntaxComponent comp)
        {
            //gen arrset
            if (null == info1.ResultValues) {
                if (TryGenConvert(codes, TypeEnum.Int, info1, out var newInfo1)) {
                    info1 = newInfo1;
                }
            }
            else {
                int index = AddConst(info1.ResultType, info1.ResultValues[0]);
                info1.ResultIndex = index;
                info1.IsGlobal = true;
                if (TryGenConvert(codes, TypeEnum.Int, info1, out var newInfo)) {
                    info1 = newInfo;
                }
            }
            if (null == info2.ResultValues) {
                if (TryGenConvert(codes, vinfo.Type, info2, out var newInfo2)) {
                    info2 = newInfo2;
                }
            }
            else {
                int index = AddConst(info2.ResultType, info2.ResultValues[0]);
                info2.ResultIndex = index;
                info2.IsGlobal = true;
                if (TryGenConvert(codes, vinfo.Type, info2, out var newInfo)) {
                    info2 = newInfo;
                }
            }

            if (vinfo.Type == TypeEnum.Int)
                codes.Add(EncodeOpcode(InsEnum.ARRSET, vinfo.IsGlobal, vinfo.Type, vinfo.Index));
            else if (vinfo.Type == TypeEnum.Float)
                codes.Add(EncodeOpcode(InsEnum.ARRSETFLT, vinfo.IsGlobal, vinfo.Type, vinfo.Index));
            else if (vinfo.Type == TypeEnum.String)
                codes.Add(EncodeOpcode(InsEnum.ARRSETSTR, vinfo.IsGlobal, vinfo.Type, vinfo.Index));
            int opd1 = EncodeOperand1(info1.IsGlobal, info1.ResultType, info1.ResultIndex);
            int opd2 = EncodeOperand2(info2.IsGlobal, info2.ResultType, info2.ResultIndex);
            codes.Add(opd1 | opd2);
        }
        private void TryGenExpression(InsEnum op, List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            CheckType(op, opds, err, comp);
            semanticInfo.ResultType = DeduceType(op, opds, out var casts, out var newOp);
            semanticInfo.ResultValues = TryCalcConst(op, opds);
            if (null != semanticInfo.ResultValues) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        //gen write result
                        int index = AddConst(semanticInfo.ResultType, semanticInfo.ResultValues[0]);
                        var info = semanticInfo;
                        info.ResultIndex = index;
                        info.IsGlobal = true;
                        if(TryGenConvert(codes, semanticInfo.TargetType, info, out var newInfo)) {
                            info = newInfo;
                        }
                        if (semanticInfo.TargetType == TypeEnum.Int)
                            codes.Add(EncodeOpcode(InsEnum.MOV, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        else if (semanticInfo.TargetType == TypeEnum.Float)
                            codes.Add(EncodeOpcode(InsEnum.MOVFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        else if (semanticInfo.TargetType == TypeEnum.String)
                            codes.Add(EncodeOpcode(InsEnum.MOVSTR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        codes.Add(EncodeOperand1(info.IsGlobal, info.ResultType, info.ResultIndex));

                        semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                        semanticInfo.ResultType = semanticInfo.TargetType;
                        semanticInfo.ResultCount = semanticInfo.TargetCount;
                        semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    }
                }
            }
            else {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        //gen write result
                        SemanticInfo info1 = new SemanticInfo(), info2 = new SemanticInfo();
                        if (opds.Count > 0) {
                            info1 = opds[0];
                        }
                        if (opds.Count > 1) {
                            info2 = opds[1];
                        }
                        if (TryGenConvert(codes, casts[0], info1, out var newInfo1)) {
                            info1 = newInfo1;
                        }
                        if (TryGenConvert(codes, casts[1], info2, out var newInfo2)) {
                            info2 = newInfo2;
                        }

                        var rinfo = semanticInfo;
                        if (semanticInfo.TargetType == semanticInfo.ResultType) {
                            codes.Add(EncodeOpcode(newOp, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        }
                        else {
                            int tmpIndex = -1;
                            switch (rinfo.ResultType) {
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
                                rinfo.IsGlobal = false;
                                rinfo.ResultCount = 0;
                                rinfo.ResultIndex = tmpIndex;
                                rinfo.ResultValues = null;
                                codes.Add(EncodeOpcode(newOp, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                            }
                        }
                        int opd1 = EncodeOperand1(info1.IsGlobal, info1.ResultType, info1.ResultIndex);
                        int opd2 = EncodeOperand2(info2.IsGlobal, info2.ResultType, info2.ResultIndex);
                        codes.Add(opd1 | opd2);
                        if (semanticInfo.TargetType != semanticInfo.ResultType) {
                            TryGenConvert(codes, semanticInfo, rinfo);
                        }

                        semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                        semanticInfo.ResultType = semanticInfo.TargetType;
                        semanticInfo.ResultCount = semanticInfo.TargetCount;
                        semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                        semanticInfo.ResultValues = null;
                    }
                }
                else {
                    int tmpIndex = -1;
                    switch (semanticInfo.ResultType) {
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
                        semanticInfo.ResultCount = 0;
                        semanticInfo.ResultIndex = tmpIndex;
                        semanticInfo.ResultValues = null;

                        SemanticInfo info1 = new SemanticInfo(), info2 = new SemanticInfo();
                        if (opds.Count > 0) {
                            info1 = opds[0];
                        }
                        if (opds.Count > 1) {
                            info2 = opds[1];
                        }
                        if (TryGenConvert(codes, casts[0], info1, out var newInfo1)) {
                            info1 = newInfo1;
                        }
                        if (TryGenConvert(codes, casts[1], info2, out var newInfo2)) {
                            info2 = newInfo2;
                        }
                        codes.Add(EncodeOpcode(newOp, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                        int opd1 = EncodeOperand1(info1.IsGlobal, info1.ResultType, info1.ResultIndex);
                        int opd2 = EncodeOperand2(info2.IsGlobal, info2.ResultType, info2.ResultIndex);
                        codes.Add(opd1 | opd2);
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
                    //gen api call
                    var rinfo = semanticInfo;
                    if (semanticInfo.TargetType != api.Type) {
                        int tmpIndex = -1;
                        switch (api.Type) {
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
                            rinfo.IsGlobal = false;
                            rinfo.ResultType = api.Type;
                            rinfo.ResultCount = 0;
                            rinfo.ResultIndex = tmpIndex;
                            rinfo.ResultValues = null;
                            codes.Add(EncodeOpcode(InsEnum.CALL, opds.Count, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                        }
                    }
                    else {
                        codes.Add(EncodeOpcode(InsEnum.CALL, opds.Count, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    }
                    int opd1 = 0;
                    int opd2 = 0;
                    opd1 = EncodeOperand1(api.ApiId);
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd2 = EncodeConstOperand2(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1 | opd2);
                    for (int i = 1; i < opds.Count; i += 2) {
                        opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            if (null == opdInfo.ResultValues) {
                                opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                            }
                        }
                        opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            if (null == opdInfo.ResultValues) {
                                opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd2 = EncodeConstOperand2(opdInfo.ResultType, index);
                            }
                        }
                        codes.Add(opd1 | opd2);
                    }
                    if (semanticInfo.TargetType != api.Type) {
                        TryGenConvert(codes, semanticInfo, rinfo);
                    }

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = -1;
                switch (api.Type) {
                    case TypeEnum.Int:
                        tmpIndex = CurBlock().AllocTempInt();
                        break;
                    case TypeEnum.Float:
                        tmpIndex = CurBlock().AllocTempFloat();
                        break;
                    case TypeEnum.String:
                        tmpIndex = CurBlock().AllocTempString();
                        break;
                    default:
                        tmpIndex = 0;
                        break;
                }
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = api.Type;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    //gen api call
                    codes.Add(EncodeOpcode(InsEnum.CALL, opds.Count, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    int opd1 = 0;
                    int opd2 = 0;
                    opd1 = EncodeOperand1(api.ApiId);
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd2 = EncodeConstOperand2(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1 | opd2);
                    for (int i = 1; i < opds.Count; i += 2) {
                        opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            if (null == opdInfo.ResultValues) {
                                opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                            }
                        }
                        opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            if (null == opdInfo.ResultValues) {
                                opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd2 = EncodeConstOperand2(opdInfo.ResultType, index);
                            }
                        }
                        codes.Add(opd1 | opd2);
                    }
                }
            }
        }
        private void TryGenInt2Flt(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Int) {
                err.AppendFormat("int2flt must and only have one integer argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    else if (semanticInfo.TargetType != TypeEnum.Float) {
                        err.AppendFormat("Can't assignment float to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.INT2FLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = TypeEnum.Float;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.INT2FLT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);
                }
            }
        }
        private void TryGenInt2Str(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Int) {
                err.AppendFormat("int2str must and only have one integer argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    else if (semanticInfo.TargetType != TypeEnum.String) {
                        err.AppendFormat("Can't assignment string to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.INT2STR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = TypeEnum.String;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.INT2STR, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);
                }
            }
        }
        private void TryGenFlt2Int(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Float) {
                err.AppendFormat("flt2int must and only have one float argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    else if (semanticInfo.TargetType != TypeEnum.Int) {
                        err.AppendFormat("Can't assignment int to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.FLT2INT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = TypeEnum.Int;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.FLT2INT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);
                }
            }
        }
        private void TryGenFlt2Str(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Float) {
                err.AppendFormat("flt2str must and only have one float argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    else if (semanticInfo.TargetType != TypeEnum.String) {
                        err.AppendFormat("Can't assignment string to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.FLT2STR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = TypeEnum.String;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.FLT2STR, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);
                }
            }
        }
        private void TryGenStr2Int(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.String) {
                err.AppendFormat("str2int must and only have one string argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    else if (semanticInfo.TargetType != TypeEnum.Int) {
                        err.AppendFormat("Can't assignment int to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.STR2INT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = TypeEnum.Int;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.STR2INT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);
                }
            }
        }
        private void TryGenStr2Flt(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.String) {
                err.AppendFormat("str2flt must and only have one string argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    else if (semanticInfo.TargetType != TypeEnum.Float) {
                        err.AppendFormat("Can't assignment float to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.STR2FLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = TypeEnum.Float;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.STR2FLT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);
                }
            }
        }
        private void TryGenIToF(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Int) {
                err.AppendFormat("itof must and only have one integer argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    else if (semanticInfo.TargetType != TypeEnum.Float) {
                        err.AppendFormat("Can't assignment float to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.ITOF, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = TypeEnum.Float;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.ITOF, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);
                }
            }
        }
        private void TryGenFToI(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Float) {
                err.AppendFormat("ftoi must and only have one float argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    else if (semanticInfo.TargetType != TypeEnum.Int) {
                        err.AppendFormat("Can't assignment int to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.FTOI, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = TypeEnum.Int;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.FTOI, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);
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
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    var rinfo = semanticInfo;
                    if (semanticInfo.TargetType != TypeEnum.Int) {
                        int tmpIndex = CurBlock().AllocTempInt();
                        if (tmpIndex >= 0) {
                            rinfo.IsGlobal = false;
                            rinfo.ResultType = TypeEnum.Int;
                            rinfo.ResultCount = 0;
                            rinfo.ResultIndex = tmpIndex;
                            rinfo.ResultValues = null;
                            codes.Add(EncodeOpcode(InsEnum.ARGC, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                            TryGenConvert(codes, semanticInfo, rinfo);
                        }
                    }
                    else {
                        codes.Add(EncodeOpcode(InsEnum.ARGC, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    }

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = TypeEnum.Int;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.ARGC, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                }
            }
        }
        private void TryGenArgv(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Int) {
                err.AppendFormat("arg must and only have one integer argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    var rinfo = semanticInfo;
                    if (semanticInfo.TargetType != TypeEnum.Int) {
                        int tmpIndex = CurBlock().AllocTempInt();
                        if (tmpIndex >= 0) {
                            rinfo.IsGlobal = false;
                            rinfo.ResultType = TypeEnum.Int;
                            rinfo.ResultCount = 0;
                            rinfo.ResultIndex = tmpIndex;
                            rinfo.ResultValues = null;
                            codes.Add(EncodeOpcode(InsEnum.ARGV, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                        }
                    }
                    else {
                        codes.Add(EncodeOpcode(InsEnum.ARGV, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    }
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);
                    if (semanticInfo.TargetType != TypeEnum.Int) {
                        TryGenConvert(codes, semanticInfo, rinfo);
                    }

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = TypeEnum.Int;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.ARGV, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);
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
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    var rinfo = semanticInfo;
                    if (semanticInfo.TargetType != TypeEnum.Int) {
                        int tmpIndex = CurBlock().AllocTempInt();
                        if (tmpIndex >= 0) {
                            rinfo.IsGlobal = false;
                            rinfo.ResultType = TypeEnum.Int;
                            rinfo.ResultCount = 0;
                            rinfo.ResultIndex = tmpIndex;
                            rinfo.ResultValues = null;
                            codes.Add(EncodeOpcode(InsEnum.ADDR, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                        }
                    }
                    else {
                        codes.Add(EncodeOpcode(InsEnum.ADDR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    }
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);
                    if (semanticInfo.TargetType != TypeEnum.Int) {
                        TryGenConvert(codes, semanticInfo, rinfo);
                    }

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = TypeEnum.Int;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.ADDR, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1);
                }
            }
        }
        private void TryGenPtrGet(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 2 || opds[0].ResultType != TypeEnum.Int || opds[1].ResultType != TypeEnum.Int) {
                err.AppendFormat("ptrget must and only have two integer arguments, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
                return;
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assignment calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.PTRGET, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    int opd2 = 0;
                    if (opds.Count > 1) {
                        var opdInfo = opds[1];
                        if (null == opdInfo.ResultValues) {
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd2 = EncodeConstOperand2(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1 | opd2);

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = TypeEnum.Int;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.PTRGET, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        if (null == opdInfo.ResultValues) {
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                        }
                    }
                    int opd2 = 0;
                    if (opds.Count > 1) {
                        var opdInfo = opds[1];
                        if (null == opdInfo.ResultValues) {
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        else {
                            int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                            opd2 = EncodeConstOperand2(opdInfo.ResultType, index);
                        }
                    }
                    codes.Add(opd1 | opd2);
                }
            }
        }
        private void TryGenPtrSet(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 3 || opds[0].ResultType != TypeEnum.Int || opds[1].ResultType != TypeEnum.Int) {
                err.AppendFormat("ptrset must and only have three arguments and the first two parameters need to be integers, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
                return;
            }
            //gen ptrset
            if (opds.Count > 0) {
                int opcode = 0;
                var opdInfo = opds[0];
                if (null == opdInfo.ResultValues) {
                    opcode = EncodeOpcode(InsEnum.PTRSET, opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                }
                else {
                    int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                    opcode = EncodeOpcode(InsEnum.PTRSET, true, opdInfo.ResultType, index);
                }
                codes.Add(opcode);
            }
            int opd1 = 0;
            if (opds.Count > 1) {
                var opdInfo = opds[1];
                if (null == opdInfo.ResultValues) {
                    opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                }
                else {
                    int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                    opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                }
            }
            int opd2 = 0;
            if (opds.Count > 2) {
                var opdInfo = opds[2];
                if (null == opdInfo.ResultValues) {
                    opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                }
                else {
                    int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                    opd2 = EncodeConstOperand2(opdInfo.ResultType, index);
                }
            }
            codes.Add(opd1 | opd2);
        }
        private void TryGenJaggedPtr(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count < 3) {
                err.AppendFormat("jptr requires at least 3 integer parameters, jptr(addr, last_size, offset, ...), code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            foreach(var opd in opds) {
                if (opd.ResultType != TypeEnum.Int) {
                    err.AppendFormat("jptr must only have integer arguments, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                    break;
                }
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assignment api result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    var rinfo = semanticInfo;
                    if (semanticInfo.TargetType != TypeEnum.Int) {
                        int tmpIndex = CurBlock().AllocTempInt();
                        if (tmpIndex >= 0) {
                            rinfo.IsGlobal = false;
                            rinfo.ResultType = TypeEnum.Int;
                            rinfo.ResultCount = 0;
                            rinfo.ResultIndex = tmpIndex;
                            rinfo.ResultValues = null;
                            codes.Add(EncodeOpcode(InsEnum.JPTR, opds.Count, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                        }
                    }
                    else {
                        codes.Add(EncodeOpcode(InsEnum.JPTR, opds.Count, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    }
                    for (int i = 0; i < opds.Count; i += 2) {
                        int opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            if (null == opdInfo.ResultValues) {
                                opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                            }
                        }
                        int opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            if (null == opdInfo.ResultValues) {
                                opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd2 = EncodeConstOperand2(opdInfo.ResultType, index);
                            }
                        }
                        codes.Add(opd1 | opd2);
                    }
                    if (semanticInfo.TargetType != TypeEnum.Int) {
                        TryGenConvert(codes, semanticInfo, rinfo);
                    }

                    semanticInfo.IsGlobal = semanticInfo.TargetIsGlobal;
                    semanticInfo.ResultType = semanticInfo.TargetType;
                    semanticInfo.ResultCount = semanticInfo.TargetCount;
                    semanticInfo.ResultIndex = semanticInfo.TargetIndex;
                    semanticInfo.ResultValues = null;
                }
            }
            else {
                int tmpIndex = CurBlock().AllocTempInt();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = TypeEnum.Int;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    //gen write result
                    codes.Add(EncodeOpcode(InsEnum.JPTR, opds.Count, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    for (int i = 0; i < opds.Count; i += 2) {
                        int opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            if (null == opdInfo.ResultValues) {
                                opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                            }
                        }
                        int opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            if (null == opdInfo.ResultValues) {
                                opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                            }
                            else {
                                int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                                opd2 = EncodeConstOperand2(opdInfo.ResultType, index);
                            }
                        }
                        codes.Add(opd1 | opd2);
                    }
                }
            }
        }

        private bool TryGenConvert(List<int> codes, VarInfo target, SemanticInfo opd)
        {
            return TryGenConvert(codes, target.IsGlobal, target.Type, target.Index, opd);
        }
        private bool TryGenConvert(List<int> codes, SemanticInfo target, SemanticInfo opd)
        {
            return TryGenConvert(codes, target.TargetIsGlobal, target.TargetType, target.TargetIndex, opd);
        }
        private bool TryGenConvert(List<int> codes, bool isGlobal, TypeEnum type, int index, SemanticInfo opd)
        {
            InsEnum op = InsEnum.NUM;
            if (opd.ResultType == TypeEnum.Int) {
                if (type == TypeEnum.Float) {
                    op = InsEnum.INT2FLT;
                }
                else if (type == TypeEnum.String) {
                    op = InsEnum.INT2STR;
                }
            }
            else if (opd.ResultType == TypeEnum.Float) {
                if (type == TypeEnum.Int) {
                    op = InsEnum.FLT2INT;
                }
                else if (type == TypeEnum.String) {
                    op = InsEnum.FLT2STR;
                }
            }
            else if (opd.ResultType == TypeEnum.String) {
                if (type == TypeEnum.Int) {
                    op = InsEnum.STR2INT;
                }
                else if (type == TypeEnum.Float) {
                    op = InsEnum.STR2FLT;
                }
            }
            bool ret = false;
            if (op != InsEnum.NUM) {
                codes.Add(EncodeOpcode(InsEnum.INT2FLT, isGlobal, type, index));
                int opd1 = 0;
                var opdInfo = opd;
                if (null == opdInfo.ResultValues) {
                    opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                }
                else {
                    int cindex = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                    opd1 = EncodeConstOperand1(opdInfo.ResultType, cindex);
                }
                codes.Add(opd1);
            }
            return ret;
        }
        private bool TryGenConvert(List<int> codes, TypeEnum type, VarInfo src, out SemanticInfo semanticInfo)
        {
            var opd = new SemanticInfo { IsGlobal = src.IsGlobal, ResultType = src.Type, ResultIndex = src.Index };
            return TryGenConvert(codes, type, src, out semanticInfo);
        }
        private bool TryGenConvert(List<int> codes, TypeEnum type, SemanticInfo opd, out SemanticInfo semanticInfo)
        {
            InsEnum op = InsEnum.NUM;
            if (opd.ResultType == TypeEnum.Int) {
                if (type == TypeEnum.Float) {
                    op = InsEnum.INT2FLT;
                }
                else if (type == TypeEnum.String) {
                    op = InsEnum.INT2STR;
                }
            }
            else if (opd.ResultType == TypeEnum.Float) {
                if (type == TypeEnum.Int) {
                    op = InsEnum.FLT2INT;
                }
                else if (type == TypeEnum.String) {
                    op = InsEnum.FLT2STR;
                }
            }
            else if (opd.ResultType == TypeEnum.String) {
                if (type == TypeEnum.Int) {
                    op = InsEnum.STR2INT;
                }
                else if (type == TypeEnum.String) {
                    op = InsEnum.STR2FLT;
                }
            }
            bool ret = false;
            semanticInfo = new SemanticInfo();
            if (op != InsEnum.NUM) {
                int tmpIndex = -1;
                if (type == TypeEnum.Int)
                    tmpIndex = CurBlock().AllocTempInt();
                else if (type == TypeEnum.Float)
                    tmpIndex = CurBlock().AllocTempFloat();
                else if (type == TypeEnum.String)
                    tmpIndex = CurBlock().AllocTempString();
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = type;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;
                    codes.Add(EncodeOpcode(InsEnum.FTOI, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = 0;
                        var opdInfo = opd;
                    if (null == opdInfo.ResultValues) {
                        opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                    }
                    else {
                        int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                        opd1 = EncodeConstOperand1(opdInfo.ResultType, index);
                    }
                    codes.Add(opd1);
                }
            }
            return ret;
        }
        private TypeEnum DeduceType(InsEnum op, List<SemanticInfo> opds, out List<TypeEnum> casts, out InsEnum newOp)
        {
            casts = new List<TypeEnum>();
            newOp = op;
            if (opds.Count == 1) {
                if(op == InsEnum.NOT || op == InsEnum.BITNOT) {
                    casts.Add(TypeEnum.Int);
                }
                else if (opds[0].ResultType == TypeEnum.String && op != InsEnum.STR2INT && op != InsEnum.STR2FLT) {
                    casts.Add(TypeEnum.Float);
                }
                else {
                    casts.Add(opds[0].ResultType);
                }
                if (op == InsEnum.NOT || op == InsEnum.BITNOT || op == InsEnum.FLT2INT || op == InsEnum.STR2INT)
                    return TypeEnum.Int;
                else if(op == InsEnum.INT2FLT || op == InsEnum.STR2FLT)
                    return TypeEnum.Float;
                else if (op == InsEnum.INT2STR || op == InsEnum.FLT2STR)
                    return TypeEnum.String;
                if (op == InsEnum.NEG && opds[0].ResultType == TypeEnum.Float) {
                    newOp = InsEnum.NEGFLT;
                }
                return opds[0].ResultType;
            }
            else if (opds.Count == 2) {
                switch (op) {
                    case InsEnum.ADD:
                        if (opds[0].ResultType == TypeEnum.String || opds[1].ResultType == TypeEnum.String) {
                            newOp = InsEnum.ADDSTR;
                        }
                        else if (opds[0].ResultType == TypeEnum.Float || opds[1].ResultType == TypeEnum.Float) {
                            newOp = InsEnum.ADDFLT;
                        }
                        break;
                    case InsEnum.SUB:
                        if (opds[0].ResultType == TypeEnum.Float || opds[1].ResultType == TypeEnum.Float) {
                            newOp = InsEnum.SUBFLT;
                        }
                        break;
                    case InsEnum.MUL:
                        if (opds[0].ResultType == TypeEnum.Float || opds[1].ResultType == TypeEnum.Float) {
                            newOp = InsEnum.MULFLT;
                        }
                        break;
                    case InsEnum.DIV:
                        if (opds[0].ResultType == TypeEnum.Float || opds[1].ResultType == TypeEnum.Float) {
                            newOp = InsEnum.DIVFLT;
                        }
                        break;
                    case InsEnum.MOD:
                        if (opds[0].ResultType == TypeEnum.Float || opds[1].ResultType == TypeEnum.Float) {
                            newOp = InsEnum.MODFLT;
                        }
                        break;
                    case InsEnum.GT:
                        if (opds[0].ResultType == TypeEnum.String || opds[1].ResultType == TypeEnum.String) {
                            newOp = InsEnum.GTSTR;
                        }
                        else if (opds[0].ResultType == TypeEnum.Float || opds[1].ResultType == TypeEnum.Float) {
                            newOp = InsEnum.GTFLT;
                        }
                        break;
                    case InsEnum.GE:
                        if (opds[0].ResultType == TypeEnum.String || opds[1].ResultType == TypeEnum.String) {
                            newOp = InsEnum.GESTR;
                        }
                        else if (opds[0].ResultType == TypeEnum.Float || opds[1].ResultType == TypeEnum.Float) {
                            newOp = InsEnum.GEFLT;
                        }
                        break;
                    case InsEnum.EQ:
                        if (opds[0].ResultType == TypeEnum.String || opds[1].ResultType == TypeEnum.String) {
                            newOp = InsEnum.EQSTR;
                        }
                        else if (opds[0].ResultType == TypeEnum.Float || opds[1].ResultType == TypeEnum.Float) {
                            newOp = InsEnum.EQFLT;
                        }
                        break;
                    case InsEnum.NE:
                        if (opds[0].ResultType == TypeEnum.String || opds[1].ResultType == TypeEnum.String) {
                            newOp = InsEnum.NESTR;
                        }
                        else if (opds[0].ResultType == TypeEnum.Float || opds[1].ResultType == TypeEnum.Float) {
                            newOp = InsEnum.NEFLT;
                        }
                        break;
                    case InsEnum.LE:
                        if (opds[0].ResultType == TypeEnum.String || opds[1].ResultType == TypeEnum.String) {
                            newOp = InsEnum.LESTR;
                        }
                        else if (opds[0].ResultType == TypeEnum.Float || opds[1].ResultType == TypeEnum.Float) {
                            newOp = InsEnum.LEFLT;
                        }
                        break;
                    case InsEnum.LT:
                        if (opds[0].ResultType == TypeEnum.String || opds[1].ResultType == TypeEnum.String) {
                            newOp = InsEnum.LTSTR;
                        }
                        else if (opds[0].ResultType == TypeEnum.Float || opds[1].ResultType == TypeEnum.Float) {
                            newOp = InsEnum.LTFLT;
                        }
                        break;
                }
                switch (op) {
                    case InsEnum.ADD:
                        if (opds[0].ResultType==TypeEnum.String || opds[1].ResultType == TypeEnum.String) {
                            casts.Add(TypeEnum.String);
                            casts.Add(TypeEnum.String);
                        }
                        else if (opds[0].ResultType == TypeEnum.Float || opds[1].ResultType == TypeEnum.Float) {
                            casts.Add(TypeEnum.Float);
                            casts.Add(TypeEnum.Float);
                        }
                        else {
                            casts.Add(TypeEnum.Int);
                            casts.Add(TypeEnum.Int);
                        }
                        break;
                    case InsEnum.AND:
                    case InsEnum.OR:
                    case InsEnum.LSHIFT:
                    case InsEnum.RSHIFT:
                    case InsEnum.URSHIFT:
                    case InsEnum.BITAND:
                    case InsEnum.BITOR:
                    case InsEnum.BITXOR:
                        casts.Add(TypeEnum.Int);
                        casts.Add(TypeEnum.Int);
                        break;
                    default:
                        casts.Add(opds[0].ResultType);
                        casts.Add(opds[1].ResultType);
                        break;
                }
                switch (op) {
                    case InsEnum.AND:
                    case InsEnum.OR:
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
                        return TypeEnum.Int;
                }
                TypeEnum type1 = opds[0].ResultType;
                TypeEnum type2 = opds[1].ResultType;
                if (type1 == TypeEnum.String || type2 == TypeEnum.String)
                    return TypeEnum.String;
                else if (type1 == TypeEnum.Float || type2 == TypeEnum.Float)
                    return TypeEnum.Float;
                else
                    return TypeEnum.Int;
            }
            else if (opds.Count > 0) {
                return opds[0].ResultType;
            }
            return TypeEnum.NotUse;
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
                        if (opd.ResultType == TypeEnum.String) {
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
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
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
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
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
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
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
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
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
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
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
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
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
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
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
                        if (opd.ResultType == TypeEnum.Float) {
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
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
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
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
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
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
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
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
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
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
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
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
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
                case InsEnum.INT2FLT:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        long.TryParse(opd.ResultValues[0], out var val);
                        ret.Add(((double)val).ToString());
                    }
                    break;
                case InsEnum.INT2STR:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        long.TryParse(opd.ResultValues[0], out var val);
                        ret.Add(val.ToString());
                    }
                    break;
                case InsEnum.FLT2INT:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        double.TryParse(opd.ResultValues[0], out var val);
                        ret.Add(((long)val).ToString());
                    }
                    break;
                case InsEnum.FLT2STR:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        double.TryParse(opd.ResultValues[0], out var val);
                        ret.Add(val.ToString());
                    }
                    break;
                case InsEnum.STR2INT:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        long.TryParse(opd.ResultValues[0], out var val);
                        ret.Add(val.ToString());
                    }
                    break;
                case InsEnum.STR2FLT:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        double.TryParse(opd.ResultValues[0], out var val);
                        ret.Add(val.ToString());
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
                TypeEnum type = opds[0].ResultType;
                if (type == TypeEnum.String && op != InsEnum.NOT) {
                    err.AppendFormat("Can't calc on string, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                }
            }
            else if (opds.Count == 2) {
                TypeEnum type1 = opds[0].ResultType;
                TypeEnum type2 = opds[1].ResultType;
                if ((op == InsEnum.ADD || op == InsEnum.AND || op == InsEnum.OR || (op >= InsEnum.GE && op <= InsEnum.LT)) && type1 == TypeEnum.String && type2 == TypeEnum.String) {
                }
                else if (type1 == TypeEnum.String || type2 == TypeEnum.String) {
                    err.AppendFormat("Can't calc on string, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                }
                if (type1 == TypeEnum.Float || type2 == TypeEnum.Float) {
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
                TypeEnum rtype = opds[i].ResultType;
                TypeEnum type;
                if (i < api.ParamTypes.Count) {
                    type = api.ParamTypes[i];
                    if (type != rtype) {
                        err.AppendFormat("'{0}' argument {1}'s type '{2}' dismatch '{3}', code:{4}, line:{5}", api.Name, i, rtype, type, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else if (api.Params == ParamsEnum.Ints) {
                    type = TypeEnum.Int;
                    if (type != rtype) {
                        err.AppendFormat("'{0}' argument {1}'s type '{2}' dismatch '{3}', code:{4}, line:{5}", api.Name, i, rtype, type, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else if (api.Params == ParamsEnum.Floats) {
                    type = TypeEnum.Float;
                    if (type != rtype) {
                        err.AppendFormat("'{0}' argument {1}'s type '{2}' dismatch '{3}', code:{4}, line:{5}", api.Name, i, rtype, type, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else if (api.Params == ParamsEnum.Strings) {
                    type = TypeEnum.String;
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
        private int EncodeOpcode(InsEnum opc, bool isGlobal, TypeEnum type, int index)
        {
            int opcode = (int)opc;
            int operand = EncodeOperand2(isGlobal, type, index);
            opcode |= operand;
            return opcode;
        }
        private int EncodeOpcode(InsEnum opc, int argNum, bool isGlobal, TypeEnum type, int index)
        {
            int opcode = (int)opc;
            opcode |= (argNum << 8);
            int operand = EncodeOperand2(isGlobal, type, index);
            opcode |= operand;
            return opcode;
        }
        private int EncodeOperand1(int apiIndex)
        {
            return (apiIndex & 0xffff);
        }
        private int EncodeOperand1(bool isGlobal, TypeEnum type, int index)
        {
            int operand = Global2Tag(isGlobal) | Type2Tag(type) | index;
            return operand;
        }
        private int EncodeOperand2(bool isGlobal, TypeEnum type, int index)
        {
            int operand = EncodeOperand1(isGlobal, type, index);
            return operand << 16;
        }
        private int EncodeConstOperand1(TypeEnum type, int index)
        {
            return EncodeOperand1(true, type, index);
        }
        private int EncodeConstOperand2(TypeEnum type, int index)
        {
            return EncodeOperand2(true, type, index);
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
        private void DecodeOpcode(int opcode, out InsEnum ins, out int argNum, out bool isGlobal, out TypeEnum type, out int index)
        {
            ins = (InsEnum)(opcode & 0xff);
            argNum = ((opcode & 0xff00) >> 8);
            int operand = opcode >> 16;
            DecodeOperand1(operand, out isGlobal, out type, out index);
        }
        private void DecodeOperand1(int operand, out int apiIndex)
        {
            apiIndex = (operand & 0xffff);
        }
        private void DecodeOperand1(int operand, out bool isGlobal, out TypeEnum type, out int index)
        {
            int localOrGlobal = ((operand & 0x8000) >> 15);
            int ty = ((operand & 0x6000) >> 13);
            index = (operand & 0x1fff);
            isGlobal = localOrGlobal != 0;
            type = (TypeEnum)ty;
        }
        private void DecodeOperand2(int operand, out bool isGlobal, out TypeEnum type, out int index)
        {
            operand >>= 16;
            DecodeOperand1(operand, out isGlobal, out type, out index);
        }

        //program structure
        private int AddConst(TypeEnum type, string val)
        {
            if (!m_ConstInfos.TryGetValue(val, out var info)) {
                info = new ConstInfo { StrValue = val, Type = type };
                switch (type) {
                    case TypeEnum.Int:
                        info.Index = m_ConstIndexInfo.NextIntIndex--;
                        break;
                    case TypeEnum.Float:
                        info.Index = m_ConstIndexInfo.NextFloatIndex--;
                        break;
                    case TypeEnum.String:
                        info.Index = m_ConstIndexInfo.NextStringIndex--;
                        break;
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
            switch (varInfo.Type) {
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
            public TypeEnum Type = TypeEnum.NotUse;
            public ParamsEnum Params = ParamsEnum.NoParams;
            public List<TypeEnum> ParamTypes = new List<TypeEnum>();
        }

        public sealed class FieldInfo
        {
            public string Name = string.Empty;
            public string Type = string.Empty;
            public int Offset = 0;
            public int Size = 0;
            public int TotalSize = 0;
            public List<int> ArrayOrPtrs = new List<int>(); // <=0 -- pointer, >0 -- array size
        }
        public sealed class StructInfo
        {
            public string Name = string.Empty;
            public int Size = 0;
            public List<FieldInfo> Fields = new List<FieldInfo>();
        }

        public sealed class ConstInfo
        {
            public string StrValue = string.Empty;
            public TypeEnum Type = TypeEnum.NotUse;
            public int Index = 0;
        }
        public sealed class VarInfo
        {
            public string Name = string.Empty;
            public TypeEnum Type = TypeEnum.NotUse;
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
            public int NextIntIndex = c_max_variable_table_size - 1;
            public int NextFloatIndex = c_max_variable_table_size - 1;
            public int NextStringIndex = c_max_variable_table_size - 1;
        }
        public sealed class LexicalScopeInfo
        {
            public bool IsBranch = false;
            public bool IsLoop = false;
            public int BlockId = 0;
            public int NextIntVarIndex = 0;
            public int NextFloatVarIndex = 0;
            public int NextStringVarIndex = 0;

            public int NextTempIntVarIndex = c_max_variable_table_size - 1;
            public int NextTempFloatVarIndex = c_max_variable_table_size - 1;
            public int NextTempStringVarIndex = c_max_variable_table_size - 1;

            public int LoopContinue = 0;
            public List<int> LoopBreakFixes = null;

            public void ResetTempVars()
            {
                NextTempIntVarIndex = c_max_variable_table_size - 1;
                NextTempFloatVarIndex = c_max_variable_table_size - 1;
                NextTempStringVarIndex = c_max_variable_table_size - 1;
            }
            public int AllocTempInt()
            {
                return NextTempIntVarIndex--;
            }
            public int AllocTempFloat()
            {
                return NextTempFloatVarIndex--;
            }
            public int AllocTempString()
            {
                return NextTempStringVarIndex--;
            }
            public int AllocTempIntArray(int ct)
            {
                NextTempIntVarIndex -= ct - 1;
                return NextTempIntVarIndex--;
            }
            public int AllocTempFloatArray(int ct)
            {
                NextTempFloatVarIndex -= ct - 1;
                return NextTempFloatVarIndex--;
            }
            public int AllocTempStringArray(int ct)
            {
                NextTempStringVarIndex -= ct - 1;
                return NextTempStringVarIndex--;
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
            public bool TargetIsGlobal;
            public TypeEnum TargetType;
            public int TargetCount;
            public int TargetIndex;

            public bool IsGlobal;
            public TypeEnum ResultType;
            public int ResultCount;
            public int ResultIndex;
            public List<string> ResultValues;

            public void Reset()
            {
                TargetOperation = TargetOperationEnum.TypeInfo;
                TargetIsGlobal = false;
                TargetType = TypeEnum.NotUse;
                TargetCount = 0;
                TargetIndex = 0;

                IsGlobal = false;
                ResultType = TypeEnum.NotUse;
                ResultCount = 0;
                ResultIndex = 0;
                ResultValues = null;
            }
            public void CopyResultFrom(SemanticInfo other)
            {
                IsGlobal = other.IsGlobal;
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

        public enum LocalGlobalEnum
        {
            Local = 0,
            Global
        }
        public enum TypeEnum
        {
            NotUse = 0,
            Int,
            Float,
            String
        }

        private static int Global2Tag(bool isGlobal)
        {
            //bit 15: 0--local 1--global
            if (isGlobal)
                return (int)LocalGlobalEnum.Global << 15;
            else
                return (int)LocalGlobalEnum.Local << 15;
        }
        private static int Type2Tag(TypeEnum type)
        {
            //bit 13、14: 0--not use 1--int 2--float 3--string
            if (type == TypeEnum.Int) {
                return (int)TypeEnum.Int << 13;
            }
            else if (type == TypeEnum.Float) {
                return (int)TypeEnum.Float << 13;
            }
            else {
                return (int)TypeEnum.String << 13;
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
        private static HashSet<string> s_FieldTypeNames = new HashSet<string> {
            "int8",
            "uint8",
            "char",
            "byte",
            "int16",
            "uint16",
            "short",
            "ushort",
            "int32",
            "uint32",
            "int",
            "uint",
            "int64",
            "uint64",
            "long",
            "ulong",
            "float",
            "double"
        };

        private const int c_abs_offset_mask = 0x7fffffff;
        private const int c_offset_backward_flag = unchecked((int)0x80000000);
        private const int c_max_variable_table_size = 8192;

        public static DebugScriptCompiler Instance
        {
            get {
                return s_Instance;
            }
        }
        private static DebugScriptCompiler s_Instance = new DebugScriptCompiler();
    }
    public class ReverseComparer<TKey> : IComparer<TKey>
    {
        private readonly IComparer<TKey> _baseComparer;

        public ReverseComparer(IComparer<TKey> baseComparer)
        {
            _baseComparer = baseComparer ?? Comparer<TKey>.Default;
        }

        public int Compare(TKey x, TKey y)
        {
            return _baseComparer.Compare(y, x);
        }
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
        internal extern static void CppDbgScp_Load([MarshalAs(UnmanagedType.LPStr)] string file);

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
