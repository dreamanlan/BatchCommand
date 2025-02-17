using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Globalization;
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
        CALLEXTERN = 0,
        RET,
        JMP,
        JMPIF,
        JMPIFNOT,
        INC,
        INCFLT,
        INCV,
        INCVFLT,
        DEC,
        DECFLT,
        DECV,
        DECVFLT,
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
        INT2STR,
        FLT2STR,
        STR2INT,
        STR2FLT,
        CASTFLTINT,
        CASTSTRINT,
        CASTINTFLT,
        CASTINTSTR,
        ASINT,
        ASFLOAT,
        ASLONG,
        ASDOUBLE,
        ARGC,
        ARGV,
        ADDR,
        ADDRFLT,
        ADDRSTR,
        PTRGET,
        PTRGETFLT,
        PTRGETSTR,
        PTRSET,
        PTRSETFLT,
        PTRSETSTR,
        CASCADEPTR,
        STKIX,
        HOOKID,
        HOOKVER,
        FFIAUTO,
        FFIMANUAL,
        FFIMANUALSTACK,
        FFIMANUALDBL,
        FFIMANUALSTACKDBL,
        RESERVE5,
        RESERVE4,
        RESERVE3,
        RESERVE2,
        RESERVE1,
        CALLINTERN_FIRST = 100,
        CALLINTERN_LAST = 255,
        NUM
    }
    public sealed class DebugScriptCompiler
    {
        public string LoadApiDefine(string txt)
        {
            int apiId = 0;
            int externApiId = 0;
            m_Apis.Clear();
            m_Protos.Clear();

            Dsl.DslFile file = new Dsl.DslFile();
            var err = new StringBuilder();
            if (file.LoadFromString(txt, (msg) => { err.AppendLine(msg); })) {
                foreach (var dslInfo in file.DslInfos) {
                    var id = dslInfo.GetId();
                    if (id == "defapi" || id == "defexternapi") {
                        bool handled = false;
                        var fd = dslInfo as Dsl.FunctionData;
                        var call = fd;
                        bool existsOptions = false;
                        if (fd.IsHighOrder && fd.LowerOrderFunction.IsParenthesisParamClass() && fd.HaveStatement()) {
                            call = fd.LowerOrderFunction;
                            existsOptions = true;
                        }
                        if (null != call && !call.IsHighOrder && call.IsParenthesisParamClass()) {
                            int pnum = call.GetParamNum();
                            if (pnum == 4) {
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

                                bool isExternApi = id == "defexternapi";
                                var api = new ApiInfo { isExtern = isExternApi, ApiId = isExternApi ? externApiId : apiId, Name = name, Type = rty, Params = paramsEnum };

                                if (isExternApi)
                                    ++externApiId;
                                else
                                    ++apiId;

                                var pts = call.GetParam(3) as Dsl.FunctionData;
                                foreach (var p in pts.Params) {
                                    string pt = ParseType(p, err, out var ct);
                                    s_Type2Ids.TryGetValue(pt, out var ty);
                                    api.ParamTypes.Add(new ApiParamType { Type = ty, Count = ct });
                                }

                                if (existsOptions) {
                                    foreach (var syn in fd.Params) {
                                        var fcall = syn as Dsl.FunctionData;
                                        if (null != fcall) {
                                            string pname = fcall.GetId();
                                            if (pname == "minparamnum") {
                                                int.TryParse(fcall.GetParamId(0), out api.MinParamNum);
                                            }
                                        }
                                    }
                                }
                                if (api.MinParamNum == 0)
                                    api.MinParamNum = api.ParamTypes.Count;

                                m_Apis.Add(name, api);
                                handled = true;
                            }
                        }
                        if (!handled) {
                            err.AppendFormat("expect defapi/defexternapi(name,ret_type,params_type,[param_type,...]); code:{0}, line:{1}", dslInfo.ToScriptString(false), dslInfo.GetLine());
                            err.AppendLine();
                        }
                    }
                    else if (id == "defproto") {
                        bool handled = false;
                        var fd = dslInfo as Dsl.FunctionData;
                        var call = fd;
                        bool existsOptions = false;
                        if (fd.IsHighOrder && fd.LowerOrderFunction.IsParenthesisParamClass() && fd.HaveStatement()) {
                            call = fd.LowerOrderFunction;
                            existsOptions = true;
                        }
                        if (null != call && !call.IsHighOrder && call.IsParenthesisParamClass()) {
                            int pnum = call.GetParamNum();
                            if (pnum >= 4 && pnum <= 6) {
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

                                var proto = new ProtoInfo { Name = name, Type = rty, Params = paramsEnum };

                                if (pnum >= 4) {
                                    var ptsInt = call.GetParam(3) as Dsl.FunctionData;
                                    foreach (var p in ptsInt.Params) {
                                        s_Type2Ids.TryGetValue(p.GetId(), out var ty);
                                        proto.IntParams.Add(ty);
                                    }
                                }
                                if (pnum >= 5) {
                                    var ptsFloat = call.GetParam(4) as Dsl.FunctionData;
                                    foreach (var p in ptsFloat.Params) {
                                        s_Type2Ids.TryGetValue(p.GetId(), out var ty);
                                        proto.FloatParams.Add(ty);
                                    }
                                }
                                if (pnum >= 6) {
                                    var ptsStack = call.GetParam(5) as Dsl.FunctionData;
                                    foreach (var p in ptsStack.Params) {
                                        s_Type2Ids.TryGetValue(p.GetId(), out var ty);
                                        proto.StackParams.Add(ty);
                                    }
                                }

                                if (existsOptions) {
                                    foreach (var syn in fd.Params) {
                                        var fcall = syn as Dsl.FunctionData;
                                        if (null != fcall) {
                                            string pname = fcall.GetId();
                                            if (pname == "minstackparamnum") {
                                                int.TryParse(fcall.GetParamId(0), out proto.MinStackParamNum);
                                            }
                                            else if (pname == "manualstack") {
                                                proto.ManualStack = fcall.GetParamId(0) == "true";
                                            }
                                            else if (pname == "doublefloat") {
                                                proto.DoubleFloat = fcall.GetParamId(0) == "true";
                                            }
                                        }
                                    }
                                }
                                if (proto.MinStackParamNum == 0) {
                                    proto.MinStackParamNum = proto.StackParams.Count;
                                }
                                if (proto.ManualStack) {
                                    proto.MinStackParamNum = 2;
                                    proto.StackParams.Clear();
                                    proto.StackParams.Add(TypeEnum.Int);//addr
                                    proto.StackParams.Add(TypeEnum.Int);//size
                                }

                                m_Protos.Add(name, proto);
                                handled = true;
                            }
                        }
                        if (!handled) {
                            err.AppendFormat("expect defproto(name,ret_type,params_type,[int_param_type,...],[float_param_type,...],[stack_param_type,...]); code:{0}, line:{1}", dslInfo.ToScriptString(false), dslInfo.GetLine());
                            err.AppendLine();
                        }
                    }
                }
            }
            return err.ToString();
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
            foreach (var pair in m_IntConstInfos) {
                txt.AppendLine("{0}value:{1} type:{2} index:{3}", Literal.GetIndentString(indent), pair.Key, TypeEnum.Int, c_max_variable_table_size - 1 - pair.Value);
            }
            foreach (var pair in m_FltConstInfos) {
                txt.AppendLine("{0}value:{1} type:{2} index:{3}", Literal.GetIndentString(indent), pair.Key, TypeEnum.Float, c_max_variable_table_size - 1 - pair.Value);
            }
            foreach (var pair in m_StrConstInfos) {
                txt.AppendLine("{0}value:{1} type:{2} index:{3}", Literal.GetIndentString(indent), pair.Key, TypeEnum.String, c_max_variable_table_size - 1 - pair.Value);
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
                foreach (var pair2 in pair.Value) {
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
                    foreach (var name in hook.ShareWith) {
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
            var consts = new SortedDictionary<int, string>(comparer);
            CollectConstInfo(TypeEnum.Int, consts);
            foreach (var pair in consts) {
                var strv = pair.Value;
                TryParseLong(strv, out var v);
                CppDbgScpInterface.CppDbgScp_AllocConstInt(v);
            }
            consts.Clear();
            CollectConstInfo(TypeEnum.Float, consts);
            foreach (var pair in consts) {
                var strv = pair.Value;
                TryParseDouble(strv, out var v);
                CppDbgScpInterface.CppDbgScp_AllocConstFloat(v);
            }
            consts.Clear();
            CollectConstInfo(TypeEnum.String, consts);
            foreach (var pair in consts) {
                var strv = pair.Value;
                IntPtr str = Marshal.StringToHGlobalAnsi(strv);
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
                        TryParseLong(strVal, out var v);
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
                        TryParseDouble(strVal, out var v);
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

            foreach (var hook in m_Hooks) {
                IntPtr name = Marshal.StringToHGlobalAnsi(hook.Name);
                IntPtr enterPtr = Marshal.AllocHGlobal(hook.OnEnter.Count * sizeof(int));
                IntPtr exitPtr = Marshal.AllocHGlobal(hook.OnExit.Count * sizeof(int));

                Marshal.Copy(hook.OnEnter.ToArray(), 0, enterPtr, hook.OnEnter.Count);
                Marshal.Copy(hook.OnExit.ToArray(), 0, exitPtr, hook.OnExit.Count);

                int hookId = CppDbgScpInterface.CppDbgScp_AddHook(name, enterPtr, hook.OnEnter.Count, exitPtr, hook.OnExit.Count);
                LogSystem.Warn("hook:{0} id:{1}", hook.Name, hookId);

                foreach (var other in hook.ShareWith) {
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
                var consts = new SortedDictionary<int, string>(comparer);
                int constNum = CollectConstInfo(TypeEnum.Int, consts);

                var strDict = new Dictionary<string, int>();
                var strTable = new List<string>();

                //int consts num
                sw.Write(constNum);

                foreach (var pair in consts) {
                    var strv = pair.Value;
                    TryParseLong(strv, out var v);

                    //int const
                    sw.Write(v);
                }
                consts.Clear();
                constNum = CollectConstInfo(TypeEnum.Float, consts);

                //float consts num
                sw.Write(constNum);

                foreach (var pair in consts) {
                    var strv = pair.Value;
                    TryParseDouble(strv, out var v);

                    //float const
                    sw.Write(v);
                }
                consts.Clear();
                constNum = CollectConstInfo(TypeEnum.String, consts);

                //string consts num
                sw.Write(constNum);

                foreach (var pair in consts) {
                    var strv = pair.Value;

                    //string const
                    sw.Write(AddStringTable(strv, strDict, strTable));
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
                            TryParseLong(strVal, out var v);
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
                            TryParseDouble(strVal, out var v);
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
                    foreach (var v in hook.OnEnter) {
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
                foreach (var str in strTable) {
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

        private int CollectConstInfo(TypeEnum type, SortedDictionary<int, string> constInfos)
        {
            switch (type) {
                case TypeEnum.Int:
                    foreach (var pair in m_IntConstInfos) {
                        constInfos.Add(pair.Value, pair.Key);
                    }
                    break;
                case TypeEnum.Float:
                    foreach (var pair in m_FltConstInfos) {
                        constInfos.Add(pair.Value, pair.Key);
                    }
                    break;
                case TypeEnum.String:
                    foreach (var pair in m_StrConstInfos) {
                        constInfos.Add(pair.Value, pair.Key);
                    }
                    break;
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
                    case InsEnum.CALLEXTERN:
                        DumpCallExtern(txt, indent, codes, ref pos);
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
                        DumpIncDec(txt, indent, codes, ref pos, "INC");
                        break;
                    case InsEnum.INCFLT:
                        DumpIncDec(txt, indent, codes, ref pos, "INCFLT");
                        break;
                    case InsEnum.INCV:
                        DumpIncDecVal(txt, indent, codes, ref pos, "INCV");
                        break;
                    case InsEnum.INCVFLT:
                        DumpIncDecVal(txt, indent, codes, ref pos, "INCVFLT");
                        break;
                    case InsEnum.DEC:
                        DumpIncDec(txt, indent, codes, ref pos, "DEC");
                        break;
                    case InsEnum.DECFLT:
                        DumpIncDec(txt, indent, codes, ref pos, "DECFLT");
                        break;
                    case InsEnum.DECV:
                        DumpIncDecVal(txt, indent, codes, ref pos, "DECV");
                        break;
                    case InsEnum.DECVFLT:
                        DumpIncDecVal(txt, indent, codes, ref pos, "DECVFLT");
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
                    case InsEnum.INT2STR:
                        DumpUnary(txt, indent, codes, ref pos, "INT2STR");
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
                    case InsEnum.CASTFLTINT:
                        DumpUnary(txt, indent, codes, ref pos, "CASTFLTINT");
                        break;
                    case InsEnum.CASTSTRINT:
                        DumpUnary(txt, indent, codes, ref pos, "CASTSTRINT");
                        break;
                    case InsEnum.CASTINTFLT:
                        DumpUnary(txt, indent, codes, ref pos, "CASTINTFLT");
                        break;
                    case InsEnum.CASTINTSTR:
                        DumpUnary(txt, indent, codes, ref pos, "CASTINTSTR");
                        break;
                    case InsEnum.ASINT:
                        DumpUnary(txt, indent, codes, ref pos, "ASINT");
                        break;
                    case InsEnum.ASFLOAT:
                        DumpUnary(txt, indent, codes, ref pos, "ASFLOAT");
                        break;
                    case InsEnum.ASLONG:
                        DumpUnary(txt, indent, codes, ref pos, "ASLONG");
                        break;
                    case InsEnum.ASDOUBLE:
                        DumpUnary(txt, indent, codes, ref pos, "ASDOUBLE");
                        break;
                    case InsEnum.ARGC:
                        DumpArgc(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.ARGV:
                        DumpArgv(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.ADDR:
                        DumpAddr(txt, indent, codes, ref pos, "ADDR");
                        break;
                    case InsEnum.ADDRFLT:
                        DumpAddr(txt, indent, codes, ref pos, "ADDRFLT");
                        break;
                    case InsEnum.ADDRSTR:
                        DumpAddr(txt, indent, codes, ref pos, "ADDRSTR");
                        break;
                    case InsEnum.PTRGET:
                        DumpPtrGet(txt, indent, codes, ref pos, "PTRGET");
                        break;
                    case InsEnum.PTRGETFLT:
                        DumpPtrGet(txt, indent, codes, ref pos, "PTRGETFLT");
                        break;
                    case InsEnum.PTRGETSTR:
                        DumpPtrGet(txt, indent, codes, ref pos, "PTRGETSTR");
                        break;
                    case InsEnum.PTRSET:
                        DumpPtrSet(txt, indent, codes, ref pos, "PTRSET");
                        break;
                    case InsEnum.PTRSETFLT:
                        DumpPtrSet(txt, indent, codes, ref pos, "PTRSETFLT");
                        break;
                    case InsEnum.PTRSETSTR:
                        DumpPtrSet(txt, indent, codes, ref pos, "PTRSETSTR");
                        break;
                    case InsEnum.CASCADEPTR:
                        DumpCascadePtr(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.STKIX:
                        DumpStackIndex(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.HOOKID:
                        DumpHookId(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.HOOKVER:
                        DumpHookVer(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.FFIAUTO:
                        DumpFFIAuto(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.FFIMANUAL:
                        DumpFFIManual(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.FFIMANUALSTACK:
                        DumpFFIManualStack(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.FFIMANUALDBL:
                        DumpFFIManualDbl(txt, indent, codes, ref pos);
                        break;
                    case InsEnum.FFIMANUALSTACKDBL:
                        DumpFFIManualStackDbl(txt, indent, codes, ref pos);
                        break;
                    default:
                        if (op >= InsEnum.CALLINTERN_FIRST && op <= InsEnum.CALLINTERN_LAST) {
                            int apiIndex = (int)(op - InsEnum.CALLINTERN_FIRST);
                            if (apiIndex >= 0) {
                                DumpCallIntern(txt, indent, codes, ref pos, op);
                            }
                        }
                        break;
                }
            }
        }
        private void DumpCallIntern(StringBuilder txt, int indent, List<int> codes, ref int pos, InsEnum op)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.Append("{0}{1}: {2} = CALLINTERN {3}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), (int)(op - InsEnum.CALLINTERN_FIRST));
            for (int i = 0; i < argNum; i += 2) {
                ++pos;
                int operand = codes[pos];
                if (i < argNum) {
                    if (i == 0) {
                        txt.Append(' ');
                    }
                    else {
                        txt.Append(", ");
                    }
                    DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
                    txt.Append(BuildVar(isGlobal1, type1, index1));
                }
                if (i + 1 < argNum) {
                    txt.Append(", ");
                    DecodeOperand2(operand, out var isGlobal2, out var type2, out var index2);
                    txt.Append(BuildVar(isGlobal2, type2, index2));
                }
            }
            txt.AppendLine();
        }
        private void DumpCallExtern(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.Append("{0}{1}: {2} = CALLEXTERN", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index));
            for (int i = 0; i <= argNum; i += 2) {
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
        private void DumpIncDec(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: {2} = {3} {4}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), op, BuildVar(isGlobal1, type1, index1));
        }
        private void DumpIncDecVal(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            DecodeOperand2(operand, out var isGlobal2, out var type2, out var index2);
            txt.AppendLine("{0}{1}: {2} = {3} {4} {5}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), op, BuildVar(isGlobal1, type1, index1), BuildVar(isGlobal2, type2, index2));
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
        private void DumpAddr(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            ++pos;
            int operand = codes[pos];
            DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
            txt.AppendLine("{0}{1}: {2} = {3} {4}", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index), op, BuildVar(isGlobal1, type1, index1));
        }
        private void DumpPtrGet(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
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
        private void DumpPtrSet(StringBuilder txt, int indent, List<int> codes, ref int pos, string op)
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
        private void DumpCascadePtr(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.Append("{0}{1}: {2} = CASCADEPTR", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index));
            for (int i = 0; i < argNum; i += 2) {
                ++pos;
                int operand = codes[pos];
                if (i < argNum) {
                    if (i == 0) {
                        txt.Append(' ');
                    }
                    else {
                        txt.Append(", ");
                    }
                    DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
                    txt.Append(BuildVar(isGlobal1, type1, index1));
                }
                if (i + 1 < argNum) {
                    txt.Append(", ");
                    DecodeOperand2(operand, out var isGlobal2, out var type2, out var index2);
                    txt.Append(BuildVar(isGlobal2, type2, index2));
                }
            }
            txt.AppendLine();
        }
        private void DumpStackIndex(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.AppendLine("{0}{1}: {2} = STKIX", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index));
        }
        private void DumpHookId(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.AppendLine("{0}{1}: {2} = HOOKID", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index));
        }
        private void DumpHookVer(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.AppendLine("{0}{1}: {2} = HOOKVER", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index));
        }
        private void DumpFFIAuto(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.Append("{0}{1}: {2} = FFIAUTO", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index));
            for (int i = 0; i < argNum; i += 2) {
                ++pos;
                int operand = codes[pos];
                if (i < argNum) {
                    if (i == 0) {
                        txt.Append(' ');
                    }
                    else {
                        txt.Append(", ");
                    }
                    DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
                    txt.Append(BuildVar(isGlobal1, type1, index1));
                }
                if (i + 1 < argNum) {
                    txt.Append(", ");
                    DecodeOperand2(operand, out var isGlobal2, out var type2, out var index2);
                    txt.Append(BuildVar(isGlobal2, type2, index2));
                }
            }
            txt.AppendLine();
        }
        private void DumpFFIManual(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.Append("{0}{1}: {2} = FFIMANUAL", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index));
            ++pos;
            int operand0 = codes[pos];
            DecodeOperand1(operand0, out var num1);
            DecodeOperand2(operand0, out var num2);
            txt.Append(' ');
            txt.Append(num1);
            txt.Append(' ');
            txt.Append(num2);
            for (int i = 0; i < argNum; i += 2) {
                ++pos;
                int operand = codes[pos];
                if (i < argNum) {
                    if (i == 0) {
                        txt.Append(' ');
                    }
                    else {
                        txt.Append(", ");
                    }
                    DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
                    txt.Append(BuildVar(isGlobal1, type1, index1));
                }
                if (i + 1 < argNum) {
                    txt.Append(", ");
                    DecodeOperand2(operand, out var isGlobal2, out var type2, out var index2);
                    txt.Append(BuildVar(isGlobal2, type2, index2));
                }
            }
            txt.AppendLine();
        }
        private void DumpFFIManualStack(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.Append("{0}{1}: {2} = FFIMANUALSTACK", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index));
            ++pos;
            int operand0 = codes[pos];
            DecodeOperand1(operand0, out var num1);
            DecodeOperand2(operand0, out var num2);
            txt.Append(' ');
            txt.Append(num1);
            txt.Append(' ');
            txt.Append(num2);
            for (int i = 0; i < argNum; i += 2) {
                ++pos;
                int operand = codes[pos];
                if (i < argNum) {
                    if (i == 0) {
                        txt.Append(' ');
                    }
                    else {
                        txt.Append(", ");
                    }
                    DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
                    txt.Append(BuildVar(isGlobal1, type1, index1));
                }
                if (i + 1 < argNum) {
                    txt.Append(", ");
                    DecodeOperand2(operand, out var isGlobal2, out var type2, out var index2);
                    txt.Append(BuildVar(isGlobal2, type2, index2));
                }
            }
            txt.AppendLine();
        }
        private void DumpFFIManualDbl(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.Append("{0}{1}: {2} = FFIMANUALDBL", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index));
            ++pos;
            int operand0 = codes[pos];
            DecodeOperand1(operand0, out var num1);
            DecodeOperand2(operand0, out var num2);
            txt.Append(' ');
            txt.Append(num1);
            txt.Append(' ');
            txt.Append(num2);
            for (int i = 0; i < argNum; i += 2) {
                ++pos;
                int operand = codes[pos];
                if (i < argNum) {
                    if (i == 0) {
                        txt.Append(' ');
                    }
                    else {
                        txt.Append(", ");
                    }
                    DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
                    txt.Append(BuildVar(isGlobal1, type1, index1));
                }
                if (i + 1 < argNum) {
                    txt.Append(", ");
                    DecodeOperand2(operand, out var isGlobal2, out var type2, out var index2);
                    txt.Append(BuildVar(isGlobal2, type2, index2));
                }
            }
            txt.AppendLine();
        }
        private void DumpFFIManualStackDbl(StringBuilder txt, int indent, List<int> codes, ref int pos)
        {
            int ix = pos;
            int opcode = codes[pos];
            DecodeOpcode(opcode, out var ins, out var argNum, out var isGlobal, out var type, out var index);
            txt.Append("{0}{1}: {2} = FFIMANUALSTACKDBL", Literal.GetIndentString(indent), ix, BuildVar(isGlobal, type, index));
            ++pos;
            int operand0 = codes[pos];
            DecodeOperand1(operand0, out var num1);
            DecodeOperand2(operand0, out var num2);
            txt.Append(' ');
            txt.Append(num1);
            txt.Append(' ');
            txt.Append(num2);
            for (int i = 0; i < argNum; i += 2) {
                ++pos;
                int operand = codes[pos];
                if (i < argNum) {
                    if (i == 0) {
                        txt.Append(' ');
                    }
                    else {
                        txt.Append(", ");
                    }
                    DecodeOperand1(operand, out var isGlobal1, out var type1, out var index1);
                    txt.Append(BuildVar(isGlobal1, type1, index1));
                }
                if (i + 1 < argNum) {
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
                return "void";
            bool isConstOrTemp = index >= c_max_variable_table_size / 2;
            string name = isGlobal ? (isConstOrTemp ? "Const" : "Global") : (isConstOrTemp ? "Temp" : "Local");

            string val = string.Empty;
            if (isGlobal && isConstOrTemp) {
                switch (type) {
                    case TypeEnum.Int:
                        foreach (var pair in m_IntConstInfos) {
                            if (pair.Value == index) {
                                val = pair.Key;
                                break;
                            }
                        }
                        break;
                    case TypeEnum.Float:
                        foreach (var pair in m_FltConstInfos) {
                            if (pair.Value == index) {
                                val = pair.Key;
                                break;
                            }
                        }
                        break;
                    case TypeEnum.String:
                        foreach (var pair in m_StrConstInfos) {
                            if (pair.Value == index) {
                                val = pair.Key;
                                break;
                            }
                        }
                        break;
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

                            var info = new SemanticInfo { TargetOperation = TargetOperationEnum.VarAssign, TargetIsGlobal = vinfo.IsGlobal, TargetType = ty, TargetCount = count, TargetIndex = vinfo.Index };
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
                        for (int i = 1; i < head.GetParamNum(); ++i) {
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
                        bool existsElse = false;
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
                                existsElse = true;
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
                                    ConvertArgument(codes, TypeEnum.Int, ref info);
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
                        if (!existsElse) {
                            //fix jmpIfNot
                            int jmpTarget = codes.Count;
                            int offset = jmpTarget - jmpIfNot;
                            int opcode = codes[jmpIfNot];
                            codes[jmpIfNot] = opcode | (offset << 8);
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
                            ConvertArgument(codes, TypeEnum.Int, ref info);
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
                                ConvertArgument(codes, info1.ResultType, ref info2);
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
                                ConvertArgument(codes, info1.ResultType, ref info3);
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
                                if (id == "loopd") {
                                    if (incOne) {
                                        var rinfo = new SemanticInfo();
                                        TryGenDec(codes, new List<SemanticInfo> { info1 }, err, head, ref rinfo);
                                    }
                                    else {
                                        var info4 = new SemanticInfo();
                                        CompileExpression(incExp, codes, err, ref info4);
                                        if (null == info4.ResultValues) {
                                            var rinfo = new SemanticInfo();
                                            TryGenDec(codes, new List<SemanticInfo> { info1, info4 }, err, head, ref rinfo);
                                        }
                                        else {
                                            if (info4.ResultValues[0] == "1") {
                                                var rinfo = new SemanticInfo();
                                                TryGenDec(codes, new List<SemanticInfo> { info1 }, err, head, ref rinfo);
                                            }
                                            else {
                                                var rinfo = new SemanticInfo();
                                                TryGenDec(codes, new List<SemanticInfo> { info1, info4 }, err, head, ref rinfo);
                                            }
                                        }
                                    }
                                }
                                else {
                                    if (incOne) {
                                        var rinfo = new SemanticInfo();
                                        TryGenInc(codes, new List<SemanticInfo> { info1 }, err, head, ref rinfo);
                                    }
                                    else {
                                        var info4 = new SemanticInfo();
                                        CompileExpression(incExp, codes, err, ref info4);
                                        if (null == info4.ResultValues) {
                                            var rinfo = new SemanticInfo();
                                            TryGenInc(codes, new List<SemanticInfo> { info1, info4 }, err, head, ref rinfo);
                                        }
                                        else {
                                            if (info4.ResultValues[0] == "1") {
                                                var rinfo = new SemanticInfo();
                                                TryGenInc(codes, new List<SemanticInfo> { info1 }, err, head, ref rinfo);
                                            }
                                            else {
                                                var rinfo = new SemanticInfo();
                                                TryGenInc(codes, new List<SemanticInfo> { info1, info4 }, err, head, ref rinfo);
                                            }
                                        }
                                    }
                                }
                                lexicalInfo.ResetTempVars();

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
                                SemanticInfo info = new SemanticInfo { TargetOperation = TargetOperationEnum.VarAssign, TargetIsGlobal = vinfo.IsGlobal, TargetType = vinfo.Type, TargetCount = vinfo.Count, TargetIndex = vinfo.Index };
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

                                    var info = new SemanticInfo { TargetOperation = TargetOperationEnum.VarAssign, TargetIsGlobal = vinfo.IsGlobal, TargetType = ty, TargetCount = count, TargetIndex = vinfo.Index };
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
                            ConvertArgument(codes, TypeEnum.Int, ref info);
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
                            err.AppendFormat("Illegal if, if must has statements, code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
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
                            ConvertArgument(codes, TypeEnum.Int, ref info);
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
                            err.AppendFormat("Illegal while, while must has statements, code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
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
                                ConvertArgument(codes, info1.ResultType, ref info2);
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
                                ConvertArgument(codes, info1.ResultType, ref info3);
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
                                if (id == "loopd") {
                                    if (incOne) {
                                        var rinfo = new SemanticInfo();
                                        TryGenDec(codes, new List<SemanticInfo> { info1 }, err, head, ref rinfo);
                                    }
                                    else {
                                        var info4 = new SemanticInfo();
                                        CompileExpression(incExp, codes, err, ref info4);
                                        if (null == info4.ResultValues) {
                                            var rinfo = new SemanticInfo();
                                            TryGenDec(codes, new List<SemanticInfo> { info1, info4 }, err, head, ref rinfo);
                                        }
                                        else {
                                            if (info4.ResultValues[0] == "1") {
                                                var rinfo = new SemanticInfo();
                                                TryGenDec(codes, new List<SemanticInfo> { info1 }, err, head, ref rinfo);
                                            }
                                            else {
                                                var rinfo = new SemanticInfo();
                                                TryGenDec(codes, new List<SemanticInfo> { info1, info4 }, err, head, ref rinfo);
                                            }
                                        }
                                    }
                                }
                                else {
                                    if (incOne) {
                                        var rinfo = new SemanticInfo();
                                        TryGenInc(codes, new List<SemanticInfo> { info1 }, err, head, ref rinfo);
                                    }
                                    else {
                                        var info4 = new SemanticInfo();
                                        CompileExpression(incExp, codes, err, ref info4);
                                        if (null == info4.ResultValues) {
                                            var rinfo = new SemanticInfo();
                                            TryGenInc(codes, new List<SemanticInfo> { info1, info4 }, err, head, ref rinfo);
                                        }
                                        else {
                                            if (info4.ResultValues[0] == "1") {
                                                var rinfo = new SemanticInfo();
                                                TryGenInc(codes, new List<SemanticInfo> { info1 }, err, head, ref rinfo);
                                            }
                                            else {
                                                var rinfo = new SemanticInfo();
                                                TryGenInc(codes, new List<SemanticInfo> { info1, info4 }, err, head, ref rinfo);
                                            }
                                        }
                                    }
                                }
                                lexicalInfo.ResetTempVars();

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
                            err.AppendFormat("Illegal loop, loop must has statements, code:{0}, line:{1}", callData.ToScriptString(false), callData.GetLine());
                            err.AppendLine();
                        }
                    }
                    else if (id == "return") {
                        //return(exp);
                        if (callData.GetParamNum() == 1) {
                            var exp = callData.GetParam(0);
                            var info = new SemanticInfo { TargetType = TypeEnum.Int };
                            CompileExpression(exp, codes, err, ref info);
                            ConvertArgument(codes, TypeEnum.Int, ref info);
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
                            ConvertArgument(codes, TypeEnum.Int, ref info);
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
                            var sinfo1 = semanticInfo;
                            var tcodes1 = new List<int>();
                            CompileExpression(exp1, tcodes1, err, ref sinfo1);
                            var sinfo2 = semanticInfo;
                            var tcodes2 = new List<int>();
                            CompileExpression(exp2, tcodes2, err, ref sinfo2);
                            TryGenCondExp(codes, info, tcodes1, sinfo1, tcodes2, sinfo2, err, stmData, ref semanticInfo);
                            handled = true;
                        }
                    }
                    if (!handled) {
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
                            var param = callData.GetParam(0);
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
                                        TryGenMov(semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex + i, codes, sinfo, err, p);
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
                                    TryGenMov(false, TypeEnum.Int, tmpIndex + i, codes, sinfo, err, p);
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
                        if (op == "expect") {
                            if (!callData.IsHighOrder && callData.GetParamNum() == 2) {
                                string type = callData.GetParamId(0);
                                if (s_Type2Ids.TryGetValue(type, out var ty)) {
                                    semanticInfo.TargetType = ty;
                                }
                                var param = callData.GetParam(1);
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
                                var param = callData.GetParam(0);
                                var info = new SemanticInfo { TargetType = TypeEnum.Int };
                                CompileExpression(param, codes, err, ref info);
                                if (info.ResultType != TypeEnum.Int) {
                                    err.AppendFormat("struct(addr, exp), addr must be integer type, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                    err.AppendLine();
                                }
                                var exp = callData.GetParam(1);
                                TryGenStruct(exp, codes, info, err, ref semanticInfo);
                            }
                            else {
                                err.AppendFormat("expect 'struct(addr, exp)', code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                err.AppendLine();
                            }
                            return;
                        }
                        else if (op == "offset") {
                            if (!callData.IsHighOrder && callData.GetParamNum() == 1) {
                                var exp = callData.GetParam(0);
                                List<int> offsets = new List<int>();
                                if (CalcStructExpressionOffsets(exp, err, offsets, out var lastSize, out var struInfo, out var fieldIndexes)) {
                                    if (offsets.Count == 1) {
                                        int offset = offsets[0];
                                        semanticInfo.ResultType = TypeEnum.Int;
                                        semanticInfo.ResultValues = new List<string> { offset.ToString() };
                                        TryGenConstResult(codes, err, callData, ref semanticInfo);
                                    }
                                    else {
                                        err.AppendFormat("unable to calculate the offset of 'offset(struct_exp)', we can only calculate the first-level offset, i mean that you cannot calculate the member offset of a data or structure pointed to by a pointer, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                        err.AppendLine();
                                    }
                                }
                                else {
                                    // Error message is printed in CalcStructExpressionOffsets
                                }
                            }
                            else {
                                err.AppendFormat("expect 'offset(struct_exp)', code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                err.AppendLine();
                            }
                            return;
                        }
                        else if (op == "size") {
                            if (!callData.IsHighOrder && callData.GetParamNum() == 1) {
                                var struName = callData.GetParamId(0);
                                if(TryGetStruct(struName, out var struInfo)) {
                                    int size = struInfo.Size;
                                    semanticInfo.ResultType = TypeEnum.Int;
                                    semanticInfo.ResultValues = new List<string> { size.ToString() };
                                    TryGenConstResult(codes, err, callData, ref semanticInfo);
                                }
                                else {
                                    err.AppendFormat("unknown struct '{0}', code:{1}, line:{2}", struName, comp.ToScriptString(false), comp.GetLine());
                                    err.AppendLine();
                                }
                            }
                            else {
                                err.AppendFormat("expect 'size(struct_name)', code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                err.AppendLine();
                            }
                            return;
                        }
                        else if (op == "ffi") {
                            if (!callData.IsHighOrder && callData.GetParamNum() >= 2) {
                                var proto = callData.GetParam(0);
                                var protoInfo = ParseProto(proto, err);
                                for (int i = 1; i < num; ++i) {
                                    var param = callData.GetParam(i);
                                    var sinfo = new SemanticInfo { TargetType = TypeEnum.Int };
                                    Debug.Assert(null != protoInfo);
                                    sinfo.TargetType = protoInfo.GetParamType(i - 1, sinfo.TargetType);
                                    CompileExpression(param, codes, err, ref sinfo);
                                    if (i == 1) {
                                        if (sinfo.ResultType != TypeEnum.Int) {
                                            err.AppendFormat("ffi(proto, addr, ...), addr must be integer type, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                            err.AppendLine();
                                        }
                                    }
                                    sinfos.Add(sinfo);
                                }
                                TryGenFFI(protoInfo, codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else {
                                err.AppendFormat("expect 'ffi(proto, addr, ...)', code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
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
                                var sinfo1 = semanticInfo;
                                CompileExpression(exp1, codes, err, ref sinfo1);
                                var sinfo2 = semanticInfo;
                                var tcodes = new List<int>();
                                CompileExpression(exp2, tcodes, err, ref sinfo2);
                                var tinfo = semanticInfo;
                                TryGenNullishCoalescing(codes, sinfo1, tcodes, sinfo2, err, comp, ref semanticInfo);
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
                                var param2 = callData.GetParam(1);
                                var sinfo2 = new SemanticInfo();
                                var tcodes2 = new List<int>();
                                CompileExpression(param2, tcodes2, err, ref sinfo2);
                                if (null == sinfo1.ResultValues && null == sinfo2.ResultValues) {
                                    ConvertArgument(tcodes1, TypeEnum.Int, ref sinfo1);
                                    ConvertArgument(tcodes2, TypeEnum.Int, ref sinfo2);
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
                                    ConvertArgument(tcodes1, TypeEnum.Int, ref sinfo1);
                                    ConvertArgument(tcodes2, TypeEnum.Int, ref sinfo2);
                                    codes.AddRange(tcodes1);
                                    codes.AddRange(tcodes2);
                                    sinfos.Add(sinfo1);
                                    sinfos.Add(sinfo2);
                                }
                                else if (null == sinfo2.ResultValues) {
                                    ConvertArgument(tcodes1, TypeEnum.Int, ref sinfo1);
                                    ConvertArgument(tcodes2, TypeEnum.Int, ref sinfo2);
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
                                    sinfo.TargetType = apiInfo.GetParamType(i, sinfo.TargetType, out var ct);
                                    sinfo.TargetCount = ct;
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
                            if (op == "inc") {
                                TryGenInc(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "dec") {
                                TryGenDec(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "int2str") {
                                TryGenInt2Str(codes, sinfos, err, callData, ref semanticInfo);
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
                            else if (op == "castint") {
                                TryGenCastInt(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "castflt") {
                                TryGenCastFlt(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "caststr") {
                                TryGenCastStr(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "asint") {
                                TryGenAsInt(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "asfloat") {
                                TryGenAsFloat(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "aslong") {
                                TryGenAsLong(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "asdouble") {
                                TryGenAsDouble(codes, sinfos, err, callData, ref semanticInfo);
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
                            else if (op == "cascadeptr") {
                                TryGenCascadePtr(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "stkix") {
                                TryGenStackIndex(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "hookid") {
                                TryGenHookId(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else if (op == "hookver") {
                                TryGenHookVer(codes, sinfos, err, callData, ref semanticInfo);
                            }
                            else {
                                ApiInfo info;
                                if (!m_Apis.TryGetValue(op, out info)) {
                                    err.AppendFormat("Undefined api '{0}', code:{1}, line:{2}", op, callData.ToScriptString(false), callData.GetLine());
                                    err.AppendLine();
                                }
                                else {
                                    if (info.isExtern)
                                        TryGenCallExternApi(info, codes, sinfos, err, callData, ref semanticInfo);
                                    else
                                        TryGenCallInternApi(info, codes, sinfos, err, callData, ref semanticInfo);
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
                                TryGenConstResult(codes, err, comp, ref semanticInfo);
                                break;
                            case Dsl.ValueData.STRING_TOKEN:
                                semanticInfo.ResultType = TypeEnum.String;
                                semanticInfo.ResultValues = new List<string>();
                                semanticInfo.ResultValues.Add(val);
                                TryGenConstResult(codes, err, comp, ref semanticInfo);
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

        private ProtoInfo ParseProto(Dsl.ISyntaxComponent exp, StringBuilder err)
        {
            ProtoInfo proto = null;
            string id = exp.GetId();
            if (exp is Dsl.ValueData) {
                m_Protos.TryGetValue(id, out proto);
            }
            else {
                bool handled = false;
                if (id == "proto") {
                    var fd = exp as Dsl.FunctionData;
                    var call = fd;
                    bool existsOptions = false;
                    if (fd.IsHighOrder && fd.LowerOrderFunction.IsParenthesisParamClass() && fd.HaveStatement()) {
                        call = fd.LowerOrderFunction;
                        existsOptions = true;
                    }
                    if (null != call && !call.IsHighOrder && call.IsParenthesisParamClass()) {
                        int pnum = call.GetParamNum();
                        if (pnum >= 3 && pnum <= 5) {
                            var type = call.GetParamId(0);
                            var paramsStr = call.GetParamId(1);

                            s_Type2Ids.TryGetValue(type, out var rty);

                            ParamsEnum paramsEnum = ParamsEnum.NoParams;
                            if (paramsStr.Length > 0 && char.IsNumber(paramsStr[0])) {
                                int.TryParse(paramsStr, out var v);
                                paramsEnum = (ParamsEnum)v;
                            }
                            else {
                                Enum.TryParse<ParamsEnum>(paramsStr, true, out paramsEnum);
                            }

                            proto = new ProtoInfo { Name = "proto", Type = rty, Params = paramsEnum };

                            if (pnum >= 3) {
                                var ptsInt = call.GetParam(2) as Dsl.FunctionData;
                                foreach (var p in ptsInt.Params) {
                                    s_Type2Ids.TryGetValue(p.GetId(), out var ty);
                                    proto.IntParams.Add(ty);
                                }
                            }
                            if (pnum >= 4) {
                                var ptsFloat = call.GetParam(3) as Dsl.FunctionData;
                                foreach (var p in ptsFloat.Params) {
                                    s_Type2Ids.TryGetValue(p.GetId(), out var ty);
                                    proto.FloatParams.Add(ty);
                                }
                            }
                            if (pnum >= 5) {
                                var ptsStack = call.GetParam(4) as Dsl.FunctionData;
                                foreach (var p in ptsStack.Params) {
                                    s_Type2Ids.TryGetValue(p.GetId(), out var ty);
                                    proto.StackParams.Add(ty);
                                }
                            }

                            if (existsOptions) {
                                foreach (var syn in fd.Params) {
                                    var fcall = syn as Dsl.FunctionData;
                                    if (null != fcall) {
                                        string pname = fcall.GetId();
                                        if (pname == "minstackparamnum") {
                                            int.TryParse(fcall.GetParamId(0), out proto.MinStackParamNum);
                                        }
                                        else if (pname == "manualstack") {
                                            proto.ManualStack = fcall.GetParamId(0) == "true";
                                        }
                                        else if (pname == "doublefloat") {
                                            proto.DoubleFloat = fcall.GetParamId(0) == "true";
                                        }
                                    }
                                }
                            }
                            if (proto.MinStackParamNum == 0) {
                                proto.MinStackParamNum = proto.StackParams.Count;
                            }
                            if (proto.ManualStack) {
                                proto.MinStackParamNum = 2;
                                proto.StackParams.Clear();
                                proto.StackParams.Add(TypeEnum.Int);//addr
                                proto.StackParams.Add(TypeEnum.Int);//size
                            }

                            handled = true;
                        }
                    }
                }
                if (!handled) {
                    err.AppendFormat("expect proto(ret_type,params_type,[int_param_type,...],[float_param_type,...],[stack_param_type,...]); code:{0}, line:{1}", exp.ToScriptString(false), exp.GetLine());
                    err.AppendLine();
                }
            }
            return proto;
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
            foreach (var field in struInfo.Fields) {
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
                    if (funcData.IsHighOrder)
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
            if (m_StructInfos.TryGetValue(name, out struInfo)) {
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
                    else if (funcData.IsBracketParamClass() && num == 1) {
                        int.TryParse(funcData.GetParamId(0), out var index);
                        fieldIndexes.Add(index);

                        var field = struInfo.Fields[fieldIndexes[0]];
                        int arrNum = 1;
                        int addNum = 0;
                        for (int i = field.ArrayOrPtrs.Count - 1; i >= 0; --i) {
                            if (i + 1 < fieldIndexes.Count) {
                                int fix = fieldIndexes[i + 1];
                                addNum += fix * arrNum;
                                break;
                            }
                            else {
                                int arrSize = field.ArrayOrPtrs[i];
                                arrNum *= arrSize;
                            }
                        }
                        offsets[offsets.Count - 1] += field.Size * addNum;
                        lastSize = field.Size * addNum;

                        if(TryGetStruct(field.Type, out var newStruInfo)) {
                            struInfo = newStruInfo;
                        }
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
            if (CalcStructExpressionOffsets(exp, err, offsets, out var lastSize, out var struInfo, out var fieldIndexes)) {
                var opds = new List<SemanticInfo>();
                opds.Add(info);
                var cinfo0 = new SemanticInfo { ResultType = TypeEnum.Int, ResultCount = 0, ResultValues = new List<string> { lastSize.ToString() } };
                opds.Add(cinfo0);
                foreach (var offset in offsets) {
                    var cinfo = new SemanticInfo { ResultType = TypeEnum.Int, ResultCount = 0, ResultValues = new List<string> { offset.ToString() } };
                    opds.Add(cinfo);
                }
                TryGenCascadePtr(codes, opds, err, exp, ref semanticInfo);
            }
        }

        private void TryGenCondExp(List<int> codes, SemanticInfo info, List<int> tcodes1, SemanticInfo info1, List<int> tcodes2, SemanticInfo info2, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            bool cond = false;
            if (null != info.ResultValues) {
                if (info.ResultType == TypeEnum.Int || info.ResultType == TypeEnum.String) {
                    TryParseLong(info.ResultValues[0], out var v);
                    cond = v != 0;
                }
                else if (info.ResultType == TypeEnum.Float) {
                    TryParseDouble(info.ResultValues[0], out var v);
                    cond = v != 0;
                }
                if (cond) {
                    if (null != info1.ResultValues) {
                        semanticInfo.ResultType = info1.ResultType;
                        semanticInfo.ResultValues = info1.ResultValues;
                    }
                }
                else {
                    if (null != info2.ResultValues) {
                        semanticInfo.ResultType = info2.ResultType;
                        semanticInfo.ResultValues = info2.ResultValues;
                    }
                }
            }
            if (!TryGenConstResult(codes, err, comp, ref semanticInfo)) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (null != info.ResultValues) {
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
                }
                else {
                    if (null != info.ResultValues) {
                        if (cond) {
                            codes.AddRange(tcodes1);
                            if (TryGenConvert(codes, semanticInfo.TargetType, info1, out var newInfo)) {
                                semanticInfo = newInfo;
                            }
                        }
                        else {
                            codes.AddRange(tcodes2);
                            if (TryGenConvert(codes, semanticInfo.TargetType, info2, out var newInfo)) {
                                semanticInfo = newInfo;
                            }
                        }
                    }
                    else {
                        var tinfo = semanticInfo;
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
                        tinfo.TargetIsGlobal = false;
                        tinfo.TargetCount = 0;
                        tinfo.TargetIndex = tmpIndex;

                        tinfo.IsGlobal = false;
                        tinfo.ResultType = tinfo.TargetType;
                        tinfo.ResultCount = 0;
                        tinfo.ResultIndex = tmpIndex;

                        if (TryGenConvert(codes, TypeEnum.Int, info, out var newInfo)) {
                            info = newInfo;
                        }
                        //gen jmpifnot
                        int jmpIfNot = codes.Count;
                        codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                        codes.Add(EncodeOperand1(info.IsGlobal, info.ResultType, info.ResultIndex));

                        codes.AddRange(tcodes1);
                        if (!TryGenConvert(codes, tinfo, info1)) {
                            TryGenMov(tinfo.TargetIsGlobal, tinfo.TargetType, tinfo.TargetIndex, codes, info1, err, comp);
                        }

                        int jmp = codes.Count;
                        codes.Add(EncodeOpcode(InsEnum.JMP));

                        //fix jmpIfNot offset
                        int elseTarget = codes.Count;
                        int offset = elseTarget - jmpIfNot;
                        int opcode = codes[jmpIfNot];
                        codes[jmpIfNot] = opcode | EncodeOffset(offset);

                        codes.AddRange(tcodes2);
                        if (!TryGenConvert(codes, tinfo, info2)) {
                            TryGenMov(tinfo.TargetIsGlobal, tinfo.TargetType, tinfo.TargetIndex, codes, info2, err, comp);
                        }

                        //fix jmp offset
                        int exitTarget = codes.Count;
                        offset = exitTarget - jmp;
                        opcode = codes[jmp];
                        codes[jmp] = opcode | EncodeOffset(offset);

                        semanticInfo = tinfo;
                    }
                }
            }
        }
        private void TryGenNullishCoalescing(List<int> codes, SemanticInfo info, List<int> tcodes, SemanticInfo info2, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            bool cond = false;
            if (null != info.ResultValues) {
                if (info.ResultType == TypeEnum.Int || info.ResultType == TypeEnum.String) {
                    TryParseLong(info.ResultValues[0], out var v);
                    cond = v != 0;
                }
                else if (info.ResultType == TypeEnum.Float) {
                    TryParseDouble(info.ResultValues[0], out var v);
                    cond = v != 0;
                }
                if (cond) {
                    semanticInfo.ResultType = info.ResultType;
                    semanticInfo.ResultValues = info.ResultValues;
                }
                else {
                    if (null != info2.ResultValues) {
                        semanticInfo.ResultType = info2.ResultType;
                        semanticInfo.ResultValues = info2.ResultValues;
                    }
                }
            }
            if (!TryGenConstResult(codes, err, comp, ref semanticInfo)) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (null != info.ResultValues) {
                        if (cond) {
                            //handled
                        }
                        else {
                            codes.AddRange(tcodes);
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
                }
                else {
                    if (null != info.ResultValues) {
                        if (cond) {
                            //handled
                        }
                        else {
                            codes.AddRange(tcodes);
                            if (TryGenConvert(codes, semanticInfo.TargetType, info2, out var newInfo)) {
                                semanticInfo = newInfo;
                            }
                        }
                    }
                    else {
                        var tinfo = semanticInfo;
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
                        tinfo.TargetIsGlobal = false;
                        tinfo.TargetCount = 0;
                        tinfo.TargetIndex = tmpIndex;

                        tinfo.IsGlobal = false;
                        tinfo.ResultType = tinfo.TargetType;
                        tinfo.ResultCount = 0;
                        tinfo.ResultIndex = tmpIndex;

                        var cinfo = info;
                        if (TryGenConvert(codes, TypeEnum.Int, cinfo, out var newInfo)) {
                            cinfo = newInfo;
                        }
                        //gen jmpifnot
                        int jmpIfNot = codes.Count;
                        codes.Add(EncodeOpcode(InsEnum.JMPIFNOT));
                        codes.Add(EncodeOperand1(cinfo.IsGlobal, cinfo.ResultType, cinfo.ResultIndex));

                        if (!TryGenConvert(codes, tinfo, info)) {
                            TryGenMov(tinfo.TargetIsGlobal, tinfo.TargetType, tinfo.TargetIndex, codes, info, err, comp);
                        }

                        int jmp = codes.Count;
                        codes.Add(EncodeOpcode(InsEnum.JMP));

                        //fix jmpIfNot offset
                        int elseTarget = codes.Count;
                        int offset = elseTarget - jmpIfNot;
                        int opcode = codes[jmpIfNot];
                        codes[jmpIfNot] = opcode | EncodeOffset(offset);

                        codes.AddRange(tcodes);
                        if (!TryGenConvert(codes, tinfo, info2)) {
                            TryGenMov(tinfo.TargetIsGlobal, tinfo.TargetType, tinfo.TargetIndex, codes, info2, err, comp);
                        }

                        //fix jmp offset
                        int exitTarget = codes.Count;
                        offset = exitTarget - jmp;
                        opcode = codes[jmp];
                        codes[jmp] = opcode | EncodeOffset(offset);

                        semanticInfo = tinfo;
                    }
                }
            }
        }
        private void TryGenInc(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count < 1 || opds.Count > 2 || opds[0].ResultType != TypeEnum.Int && opds[0].ResultType != TypeEnum.Float) {
                err.AppendFormat("inc must has one or two int/float arguments, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
                return;
            }
            if (null != opds[0].ResultValues || opds[0].ResultCount > 0 || opds[0].ResultIndex >= c_max_variable_table_size / 2) {
                err.AppendFormat("inc's first argument must be a int/float variable, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
                return;
            }
            var info = opds[0];
            bool existsVal = opds.Count == 2;
            SemanticInfo info2 = new SemanticInfo();
            if (existsVal) {
                info2 = opds[1];
                ConvertArgument(codes, info.ResultType, ref info2);
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    //gen write result
                    SemanticInfo rinfo = new SemanticInfo();
                    if (semanticInfo.TargetType == info.ResultType) {
                        if (info.ResultType == TypeEnum.Int) {
                            if (opds.Count == 1)
                                codes.Add(EncodeOpcode(InsEnum.INC, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                            else
                                codes.Add(EncodeOpcode(InsEnum.INCV, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        }
                        else if (info.ResultType == TypeEnum.Float) {
                            if (opds.Count == 1)
                                codes.Add(EncodeOpcode(InsEnum.INCFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                            else
                                codes.Add(EncodeOpcode(InsEnum.INCVFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        }
                    }
                    else {
                        int tmpIndex = -1;
                        switch (info.ResultType) {
                            case TypeEnum.Int:
                                tmpIndex = CurBlock().AllocTempInt();
                                break;
                            case TypeEnum.Float:
                                tmpIndex = CurBlock().AllocTempFloat();
                                break;
                        }
                        if (tmpIndex >= 0) {
                            rinfo.IsGlobal = false;
                            rinfo.ResultType = semanticInfo.TargetType;
                            rinfo.ResultCount = 0;
                            rinfo.ResultIndex = tmpIndex;
                            rinfo.ResultValues = null;

                            if (info.ResultType == TypeEnum.Int) {
                                if (opds.Count == 1)
                                    codes.Add(EncodeOpcode(InsEnum.INC, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                                else
                                    codes.Add(EncodeOpcode(InsEnum.INCV, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                            }
                            else if (info.ResultType == TypeEnum.Float) {
                                if (opds.Count == 1)
                                    codes.Add(EncodeOpcode(InsEnum.INCFLT, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                                else
                                    codes.Add(EncodeOpcode(InsEnum.INCVFLT, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                            }
                        }
                    }
                    int opd1 = EncodeOperand1(info.IsGlobal, info.ResultType, info.ResultIndex);
                    int opd2 = 0;
                    if (existsVal) {
                        opd2 = EncodeOperand2(info2.IsGlobal, info2.ResultType, info2.ResultIndex);
                    }
                    codes.Add(opd1 | opd2);
                    if (semanticInfo.TargetType != info.ResultType) {
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
                switch (info.ResultType) {
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
                    semanticInfo.ResultType = info.ResultType;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;

                    //gen write result
                    if (info.ResultType == TypeEnum.Int) {
                        if (opds.Count == 1)
                            codes.Add(EncodeOpcode(InsEnum.INC, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                        else
                            codes.Add(EncodeOpcode(InsEnum.INCV, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    }
                    else if (info.ResultType == TypeEnum.Float) {
                        if (opds.Count == 1)
                            codes.Add(EncodeOpcode(InsEnum.INCFLT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                        else
                            codes.Add(EncodeOpcode(InsEnum.INCVFLT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    }
                    int opd1 = EncodeOperand1(info.IsGlobal, info.ResultType, info.ResultIndex);
                    int opd2 = 0;
                    if (existsVal) {
                        opd2 = EncodeOperand2(info2.IsGlobal, info2.ResultType, info2.ResultIndex);
                    }
                    codes.Add(opd1 | opd2);
                }
            }
        }
        private void TryGenDec(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count < 1 || opds.Count > 2 || opds[0].ResultType != TypeEnum.Int && opds[0].ResultType != TypeEnum.Float) {
                err.AppendFormat("dec must has one or two int/float arguments, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
                return;
            }
            if (null != opds[0].ResultValues || opds[0].ResultCount > 0 || opds[0].ResultIndex >= c_max_variable_table_size / 2) {
                err.AppendFormat("dec's first argument must be a int/float variable, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
                return;
            }
            var info = opds[0];
            bool existsVal = opds.Count == 2;
            SemanticInfo info2 = new SemanticInfo();
            if (existsVal) {
                info2 = opds[1];
                ConvertArgument(codes, info.ResultType, ref info2);
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    //gen write result
                    SemanticInfo rinfo = new SemanticInfo();
                    if (semanticInfo.TargetType == info.ResultType) {
                        if (info.ResultType == TypeEnum.Int) {
                            if (opds.Count == 1)
                                codes.Add(EncodeOpcode(InsEnum.DEC, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                            else
                                codes.Add(EncodeOpcode(InsEnum.DECV, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        }
                        else if (info.ResultType == TypeEnum.Float) {
                            if (opds.Count == 1)
                                codes.Add(EncodeOpcode(InsEnum.DECFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                            else
                                codes.Add(EncodeOpcode(InsEnum.DECVFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        }
                    }
                    else {
                        int tmpIndex = -1;
                        switch (info.ResultType) {
                            case TypeEnum.Int:
                                tmpIndex = CurBlock().AllocTempInt();
                                break;
                            case TypeEnum.Float:
                                tmpIndex = CurBlock().AllocTempFloat();
                                break;
                        }
                        if (tmpIndex >= 0) {
                            rinfo.IsGlobal = false;
                            rinfo.ResultType = semanticInfo.TargetType;
                            rinfo.ResultCount = 0;
                            rinfo.ResultIndex = tmpIndex;
                            rinfo.ResultValues = null;

                            if (info.ResultType == TypeEnum.Int) {
                                if (opds.Count == 1)
                                    codes.Add(EncodeOpcode(InsEnum.DEC, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                                else
                                    codes.Add(EncodeOpcode(InsEnum.DECV, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                            }
                            else if (info.ResultType == TypeEnum.Float) {
                                if (opds.Count == 1)
                                    codes.Add(EncodeOpcode(InsEnum.DECFLT, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                                else
                                    codes.Add(EncodeOpcode(InsEnum.DECVFLT, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                            }
                        }
                    }
                    int opd1 = EncodeOperand1(info.IsGlobal, info.ResultType, info.ResultIndex);
                    int opd2 = 0;
                    if (existsVal) {
                        opd2 = EncodeOperand2(info2.IsGlobal, info2.ResultType, info2.ResultIndex);
                    }
                    codes.Add(opd1 | opd2);
                    if (semanticInfo.TargetType != info.ResultType) {
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
                switch (info.ResultType) {
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
                    semanticInfo.ResultType = info.ResultType;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;

                    //gen write result
                    if (info.ResultType == TypeEnum.Int) {
                        if (opds.Count == 1)
                            codes.Add(EncodeOpcode(InsEnum.DEC, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                        else
                            codes.Add(EncodeOpcode(InsEnum.DECV, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    }
                    else if (info.ResultType == TypeEnum.Float) {
                        if (opds.Count == 1)
                            codes.Add(EncodeOpcode(InsEnum.DECFLT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                        else
                            codes.Add(EncodeOpcode(InsEnum.DECVFLT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    }
                    int opd1 = EncodeOperand1(info.IsGlobal, info.ResultType, info.ResultIndex);
                    int opd2 = 0;
                    if (existsVal) {
                        opd2 = EncodeOperand2(info2.IsGlobal, info2.ResultType, info2.ResultIndex);
                    }
                    codes.Add(opd1 | opd2);
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
                            err.AppendFormat("Can't assign array with different size, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
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
                                else if (semanticInfo.TargetType == TypeEnum.Float) {
                                    info.ResultIndex = CurBlock().AllocTempFloatArray(vinfo2.Count);
                                }
                                else if (semanticInfo.TargetType == TypeEnum.String) {
                                    info.ResultIndex = CurBlock().AllocTempStringArray(vinfo2.Count);
                                }

                                if (TryGenConvert(codes, info.ResultType, vinfo2, out var newInfo)) {
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
        private void TryGenArrGet(string id, List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1) {
                err.AppendFormat("arrget must has and only has one argument, code:{0}, line:{1}", id, comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
                return;
            }
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
                    ConvertArgument(codes, TypeEnum.Int, ref info2);
                    //gen write result
                    SemanticInfo rinfo = new SemanticInfo();
                    if (semanticInfo.TargetType == vinfo2.Type) {
                        if (vinfo2.Type == TypeEnum.Int)
                            codes.Add(EncodeOpcode(InsEnum.ARRGET, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        else if (vinfo2.Type == TypeEnum.Float)
                            codes.Add(EncodeOpcode(InsEnum.ARRGETFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        else if (vinfo2.Type == TypeEnum.String)
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

                            if (vinfo2.Type == TypeEnum.Int)
                                codes.Add(EncodeOpcode(InsEnum.ARRGET, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                            else if (vinfo2.Type == TypeEnum.Float)
                                codes.Add(EncodeOpcode(InsEnum.ARRGETFLT, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                            else if (vinfo2.Type == TypeEnum.String)
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
                    ConvertArgument(codes, TypeEnum.Int, ref info2);
                    //gen write result
                    if (vinfo.Type == TypeEnum.Int)
                        codes.Add(EncodeOpcode(InsEnum.ARRGET, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    else if (vinfo.Type == TypeEnum.Float)
                        codes.Add(EncodeOpcode(InsEnum.ARRGETFLT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    else if (vinfo.Type == TypeEnum.String)
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
            ConvertArgument(codes, TypeEnum.Int, ref info1);
            ConvertArgument(codes, vinfo.Type, ref info2);

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
            if (!TryGenConstResult(codes, err, comp, ref semanticInfo)) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        //gen write result
                        SemanticInfo info1 = new SemanticInfo();
                        SemanticInfo info2 = new SemanticInfo();
                        if (opds.Count > 0) {
                            info1 = opds[0];
                            ConvertArgument(codes, casts[0], ref info1);
                        }
                        if (opds.Count > 1) {
                            info2 = opds[1];
                            ConvertArgument(codes, casts[1], ref info2);
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
                        int opd1 = 0;
                        if (opds.Count > 0) {
                            opd1 = EncodeOperand1(info1.IsGlobal, info1.ResultType, info1.ResultIndex);
                        }
                        int opd2 = 0;
                        if (opds.Count > 1) {
                            opd2 = EncodeOperand2(info2.IsGlobal, info2.ResultType, info2.ResultIndex);
                        }
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

                        SemanticInfo info1 = new SemanticInfo();
                        SemanticInfo info2 = new SemanticInfo();
                        if (opds.Count > 0) {
                            info1 = opds[0];
                            ConvertArgument(codes, casts[0], ref info1);
                        }
                        if (opds.Count > 1) {
                            info2 = opds[1];
                            ConvertArgument(codes, casts[1], ref info2);
                        }
                        codes.Add(EncodeOpcode(newOp, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                        int opd1 = 0;
                        if (opds.Count > 0) {
                            opd1 = EncodeOperand1(info1.IsGlobal, info1.ResultType, info1.ResultIndex);
                        }
                        int opd2 = 0;
                        if (opds.Count > 1) {
                            opd2 = EncodeOperand2(info2.IsGlobal, info2.ResultType, info2.ResultIndex);
                        }
                        codes.Add(opd1 | opd2);
                    }
                }
            }
        }
        private void TryGenCallInternApi(ApiInfo api, List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            InsEnum op = InsEnum.CALLINTERN_FIRST + api.ApiId;
            CheckType(api, opds, err, comp);
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    for (int i = 0; i < opds.Count; ++i) {
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            ConvertArgument(codes, api, i, err, comp, ref opdInfo);
                            opds[i] = opdInfo;
                        }
                    }
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
                            codes.Add(EncodeOpcode(op, opds.Count, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                        }
                    }
                    else {
                        codes.Add(EncodeOpcode(op, opds.Count, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    }
                    for (int i = 0; i < opds.Count; i += 2) {
                        int opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        int opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
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

                    for (int i = 0; i < opds.Count; ++i) {
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            ConvertArgument(codes, api, i, err, comp, ref opdInfo);
                            opds[i] = opdInfo;
                        }
                    }
                    //gen api call
                    codes.Add(EncodeOpcode(op, opds.Count, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    for (int i = 0; i < opds.Count; i += 2) {
                        int opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        int opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        codes.Add(opd1 | opd2);
                    }
                }
            }
        }
        private void TryGenCallExternApi(ApiInfo api, List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            CheckType(api, opds, err, comp);
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    for (int i = 0; i < opds.Count; ++i) {
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            ConvertArgument(codes, api, i, err, comp, ref opdInfo);
                            opds[i] = opdInfo;
                        }
                    }
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
                            codes.Add(EncodeOpcode(InsEnum.CALLEXTERN, opds.Count, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                        }
                    }
                    else {
                        codes.Add(EncodeOpcode(InsEnum.CALLEXTERN, opds.Count, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    }
                    int opd1 = 0;
                    int opd2 = 0;
                    opd1 = EncodeOperand1(api.ApiId);
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                    }
                    codes.Add(opd1 | opd2);
                    for (int i = 1; i < opds.Count; i += 2) {
                        opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
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

                    for (int i = 0; i < opds.Count; ++i) {
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            ConvertArgument(codes, api, i, err, comp, ref opdInfo);
                            opds[i] = opdInfo;
                        }
                    }
                    //gen api call
                    codes.Add(EncodeOpcode(InsEnum.CALLEXTERN, opds.Count, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    int opd1 = 0;
                    int opd2 = 0;
                    opd1 = EncodeOperand1(api.ApiId);
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                    }
                    codes.Add(opd1 | opd2);
                    for (int i = 1; i < opds.Count; i += 2) {
                        opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        codes.Add(opd1 | opd2);
                    }
                }
            }
        }
        private void TryGenInt2Str(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Int) {
                err.AppendFormat("int2str must and only has one integer argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            semanticInfo.ResultType = TypeEnum.String;
            semanticInfo.ResultValues = TryCalcConst(InsEnum.INT2STR, opds);
            if (!TryGenConstResult(codes, err, comp, ref semanticInfo)) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        if (semanticInfo.TargetCount > 0) {
                            err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        else if (semanticInfo.TargetType != TypeEnum.String) {
                            err.AppendFormat("Can't assign string to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        //gen write result
                        codes.Add(EncodeOpcode(InsEnum.INT2STR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
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
                            opd1 = EncodeOperand1Helper(ref opdInfo);
                        }
                        codes.Add(opd1);
                    }
                }
            }
        }
        private void TryGenFlt2Str(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Float) {
                err.AppendFormat("flt2str must and only has one float argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            semanticInfo.ResultType = TypeEnum.String;
            semanticInfo.ResultValues = TryCalcConst(InsEnum.FLT2STR, opds);
            if (!TryGenConstResult(codes, err, comp, ref semanticInfo)) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        if (semanticInfo.TargetCount > 0) {
                            err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        else if (semanticInfo.TargetType != TypeEnum.String) {
                            err.AppendFormat("Can't assign string to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        //gen write result
                        codes.Add(EncodeOpcode(InsEnum.FLT2STR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
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
                            opd1 = EncodeOperand1Helper(ref opdInfo);
                        }
                        codes.Add(opd1);
                    }
                }
            }
        }
        private void TryGenStr2Int(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.String) {
                err.AppendFormat("str2int must and only has one string argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            semanticInfo.ResultType = TypeEnum.Int;
            semanticInfo.ResultValues = TryCalcConst(InsEnum.STR2INT, opds);
            if (!TryGenConstResult(codes, err, comp, ref semanticInfo)) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        if (semanticInfo.TargetCount > 0) {
                            err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        else if (semanticInfo.TargetType != TypeEnum.Int) {
                            err.AppendFormat("Can't assign int to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        //gen write result
                        codes.Add(EncodeOpcode(InsEnum.STR2INT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
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
                            opd1 = EncodeOperand1Helper(ref opdInfo);
                        }
                        codes.Add(opd1);
                    }
                }
            }
        }
        private void TryGenStr2Flt(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.String) {
                err.AppendFormat("str2flt must and only has one string argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            semanticInfo.ResultType = TypeEnum.Float;
            semanticInfo.ResultValues = TryCalcConst(InsEnum.STR2FLT, opds);
            if (!TryGenConstResult(codes, err, comp, ref semanticInfo)) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        if (semanticInfo.TargetCount > 0) {
                            err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        else if (semanticInfo.TargetType != TypeEnum.Float) {
                            err.AppendFormat("Can't assign float to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        //gen write result
                        codes.Add(EncodeOpcode(InsEnum.STR2FLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
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
                            opd1 = EncodeOperand1Helper(ref opdInfo);
                        }
                        codes.Add(opd1);
                    }
                }
            }
        }
        private void TryGenCastInt(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            InsEnum op = (opds[0].ResultType == TypeEnum.Float ? InsEnum.CASTFLTINT : InsEnum.CASTSTRINT);
            if (opds.Count != 1 || (opds[0].ResultType != TypeEnum.Float && opds[0].ResultType != TypeEnum.String)) {
                err.AppendFormat("castint must and only has one float/string argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            semanticInfo.ResultType = TypeEnum.Int;
            semanticInfo.ResultValues = TryCalcConst(op, opds);
            if (!TryGenConstResult(codes, err, comp, ref semanticInfo)) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        if (semanticInfo.TargetCount > 0) {
                            err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        else if (semanticInfo.TargetType != TypeEnum.Int) {
                            err.AppendFormat("Can't assign int to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        //gen write result
                        codes.Add(EncodeOpcode(op, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
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
                        codes.Add(EncodeOpcode(op, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
                        }
                        codes.Add(opd1);
                    }
                }
            }
        }
        private void TryGenCastFlt(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Int) {
                err.AppendFormat("castflt must and only has one integer argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            semanticInfo.ResultType = TypeEnum.Float;
            semanticInfo.ResultValues = TryCalcConst(InsEnum.CASTINTFLT, opds);
            if (!TryGenConstResult(codes, err, comp, ref semanticInfo)) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        if (semanticInfo.TargetCount > 0) {
                            err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        else if (semanticInfo.TargetType != TypeEnum.Float) {
                            err.AppendFormat("Can't assign float to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        //gen write result
                        codes.Add(EncodeOpcode(InsEnum.CASTINTFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
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
                        codes.Add(EncodeOpcode(InsEnum.CASTINTFLT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
                        }
                        codes.Add(opd1);
                    }
                }
            }
        }
        private void TryGenCastStr(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Int) {
                err.AppendFormat("caststr must and only has one integer argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            semanticInfo.ResultType = TypeEnum.String;
            semanticInfo.ResultValues = TryCalcConst(InsEnum.CASTINTSTR, opds);
            if (!TryGenConstResult(codes, err, comp, ref semanticInfo)) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        if (semanticInfo.TargetCount > 0) {
                            err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        else if (semanticInfo.TargetType != TypeEnum.String) {
                            err.AppendFormat("Can't assign string to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        //gen write result
                        codes.Add(EncodeOpcode(InsEnum.CASTINTSTR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
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
                        codes.Add(EncodeOpcode(InsEnum.CASTINTSTR, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
                        }
                        codes.Add(opd1);
                    }
                }
            }
        }
        private void TryGenAsInt(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Float) {
                err.AppendFormat("asint must and only has one float argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            semanticInfo.ResultType = TypeEnum.Int;
            semanticInfo.ResultValues = TryCalcConst(InsEnum.ASINT, opds);
            if (!TryGenConstResult(codes, err, comp, ref semanticInfo)) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        if (semanticInfo.TargetCount > 0) {
                            err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        else if (semanticInfo.TargetType != TypeEnum.Int) {
                            err.AppendFormat("Can't assign int to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        //gen write result
                        codes.Add(EncodeOpcode(InsEnum.ASINT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
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
                        codes.Add(EncodeOpcode(InsEnum.ASINT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
                        }
                        codes.Add(opd1);
                    }
                }
            }
        }
        private void TryGenAsFloat(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Int) {
                err.AppendFormat("asfloat must and only has one integer argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            semanticInfo.ResultType = TypeEnum.Float;
            semanticInfo.ResultValues = TryCalcConst(InsEnum.ASFLOAT, opds);
            if (!TryGenConstResult(codes, err, comp, ref semanticInfo)) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        if (semanticInfo.TargetCount > 0) {
                            err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        else if (semanticInfo.TargetType != TypeEnum.Float) {
                            err.AppendFormat("Can't assign float to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        //gen write result
                        codes.Add(EncodeOpcode(InsEnum.ASFLOAT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
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
                        codes.Add(EncodeOpcode(InsEnum.ASFLOAT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
                        }
                        codes.Add(opd1);
                    }
                }
            }
        }
        private void TryGenAsLong(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Float) {
                err.AppendFormat("aslong must and only has one float argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            semanticInfo.ResultType = TypeEnum.Int;
            semanticInfo.ResultValues = TryCalcConst(InsEnum.ASLONG, opds);
            if (!TryGenConstResult(codes, err, comp, ref semanticInfo)) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        if (semanticInfo.TargetCount > 0) {
                            err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        else if (semanticInfo.TargetType != TypeEnum.Int) {
                            err.AppendFormat("Can't assign int to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        //gen write result
                        codes.Add(EncodeOpcode(InsEnum.ASLONG, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
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
                        codes.Add(EncodeOpcode(InsEnum.ASLONG, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
                        }
                        codes.Add(opd1);
                    }
                }
            }
        }
        private void TryGenAsDouble(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1 || opds[0].ResultType != TypeEnum.Int) {
                err.AppendFormat("asdouble must and only has one integer argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            semanticInfo.ResultType = TypeEnum.Float;
            semanticInfo.ResultValues = TryCalcConst(InsEnum.ASDOUBLE, opds);
            if (!TryGenConstResult(codes, err, comp, ref semanticInfo)) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        if (semanticInfo.TargetCount > 0) {
                            err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        else if (semanticInfo.TargetType != TypeEnum.Float) {
                            err.AppendFormat("Can't assign float to {0} var, code:{1}, line:{2}", s_TypeNames[(int)semanticInfo.TargetType], comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        //gen write result
                        codes.Add(EncodeOpcode(InsEnum.ASDOUBLE, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
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
                        codes.Add(EncodeOpcode(InsEnum.ASDOUBLE, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                        int opd1 = 0;
                        if (opds.Count > 0) {
                            var opdInfo = opds[0];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
                        }
                        codes.Add(opd1);
                    }
                }
            }
        }
        private void TryGenArgc(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 0) {
                err.AppendFormat("argc must has zero arguments, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
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
                err.AppendFormat("arg must and only has one integer argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
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
                        opd1 = EncodeOperand1Helper(ref opdInfo);
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
                        opd1 = EncodeOperand1Helper(ref opdInfo);
                    }
                    codes.Add(opd1);
                }
            }
        }
        private void TryGenAddr(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 1) {
                err.AppendFormat("addr must and only has one argument, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
                return;
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
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
                            switch (opds[0].ResultType) {
                                case TypeEnum.Int:
                                    codes.Add(EncodeOpcode(InsEnum.ADDR, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                                    break;
                                case TypeEnum.Float:
                                    codes.Add(EncodeOpcode(InsEnum.ADDRFLT, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                                    break;
                                case TypeEnum.String:
                                    codes.Add(EncodeOpcode(InsEnum.ADDRSTR, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                                    break;
                                default:
                                    err.AppendFormat("addr's argument must be int/float/string type, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                    err.AppendLine();
                                    break;
                            }
                        }
                    }
                    else {
                        switch (opds[0].ResultType) {
                            case TypeEnum.Int:
                                codes.Add(EncodeOpcode(InsEnum.ADDR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                                break;
                            case TypeEnum.Float:
                                codes.Add(EncodeOpcode(InsEnum.ADDRFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                                break;
                            case TypeEnum.String:
                                codes.Add(EncodeOpcode(InsEnum.ADDRSTR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                                break;
                            default:
                                err.AppendFormat("addr's argument must be int/float/string type, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                                err.AppendLine();
                                break;
                        }
                    }
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        opd1 = EncodeOperand1Helper(ref opdInfo);
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
                    switch (opds[0].ResultType) {
                        case TypeEnum.Int:
                            codes.Add(EncodeOpcode(InsEnum.ADDR, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                            break;
                        case TypeEnum.Float:
                            codes.Add(EncodeOpcode(InsEnum.ADDRFLT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                            break;
                        case TypeEnum.String:
                            codes.Add(EncodeOpcode(InsEnum.ADDRSTR, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                            break;
                        default:
                            err.AppendFormat("addr's argument must be int/float/string type, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                            break;
                    }

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        opd1 = EncodeOperand1Helper(ref opdInfo);
                    }
                    codes.Add(opd1);
                }
            }
        }
        private void TryGenPtrGet(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 2 || opds[0].ResultType != TypeEnum.Int || opds[1].ResultType != TypeEnum.Int) {
                err.AppendFormat("ptrget must and only has two integer arguments, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
                return;
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                    //gen write result
                    switch (semanticInfo.TargetType) {
                        case TypeEnum.Int:
                            codes.Add(EncodeOpcode(InsEnum.PTRGET, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                            break;
                        case TypeEnum.Float:
                            codes.Add(EncodeOpcode(InsEnum.PTRGETFLT, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                            break;
                        case TypeEnum.String:
                            codes.Add(EncodeOpcode(InsEnum.PTRGETSTR, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                            break;
                        default:
                            err.AppendFormat("ptrget must be assigned to an int/float/string variable, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                            break;
                    }
                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        opd1 = EncodeOperand1Helper(ref opdInfo);
                    }
                    int opd2 = 0;
                    if (opds.Count > 1) {
                        var opdInfo = opds[1];
                        opd2 = EncodeOperand2Helper(ref opdInfo);
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
                TypeEnum retType = semanticInfo.TargetType;
                int tmpIndex = -1;
                switch (semanticInfo.TargetType) {
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
                        retType = TypeEnum.Int;
                        tmpIndex = CurBlock().AllocTempInt();
                        break;
                }
                if (tmpIndex >= 0) {
                    semanticInfo.IsGlobal = false;
                    semanticInfo.ResultType = retType;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;

                    switch (semanticInfo.TargetType) {
                        case TypeEnum.Int:
                            codes.Add(EncodeOpcode(InsEnum.PTRGET, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                            break;
                        case TypeEnum.Float:
                            codes.Add(EncodeOpcode(InsEnum.PTRGETFLT, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                            break;
                        case TypeEnum.String:
                            codes.Add(EncodeOpcode(InsEnum.PTRGETSTR, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                            break;
                        default:
                            err.AppendFormat("ptrget must be return int/float/string value, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                            break;
                    }

                    int opd1 = 0;
                    if (opds.Count > 0) {
                        var opdInfo = opds[0];
                        opd1 = EncodeOperand1Helper(ref opdInfo);
                    }
                    int opd2 = 0;
                    if (opds.Count > 1) {
                        var opdInfo = opds[1];
                        opd2 = EncodeOperand2Helper(ref opdInfo);
                    }
                    codes.Add(opd1 | opd2);
                }
            }
        }
        private void TryGenPtrSet(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 3 || opds[0].ResultType != TypeEnum.Int || opds[1].ResultType != TypeEnum.Int) {
                err.AppendFormat("ptrset must and only has three arguments and the first two parameters need to be integers, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
                return;
            }
            //gen ptrset
            if (opds.Count > 0) {
                int opcode = 0;
                var opdInfo = opds[0];
                ConvertArgument(ref opdInfo);
                switch (opds[2].TargetType) {
                    case TypeEnum.Int:
                        opcode = EncodeOpcode(InsEnum.PTRSET, opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        break;
                    case TypeEnum.Float:
                        opcode = EncodeOpcode(InsEnum.PTRSETFLT, opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        break;
                    case TypeEnum.String:
                        opcode = EncodeOpcode(InsEnum.PTRSETSTR, opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        break;
                    default:
                        err.AppendFormat("ptrset's third argument must be int/float/string type, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                        break;
                }
                codes.Add(opcode);
            }
            int opd1 = 0;
            if (opds.Count > 1) {
                var opdInfo = opds[1];
                opd1 = EncodeOperand1Helper(ref opdInfo);
            }
            int opd2 = 0;
            if (opds.Count > 2) {
                var opdInfo = opds[2];
                opd2 = EncodeOperand2Helper(ref opdInfo);
            }
            codes.Add(opd1 | opd2);
        }
        private void TryGenCascadePtr(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count < 3) {
                err.AppendFormat("cascadeptr requires at least 3 integer parameters, cascadeptr(addr, last_size, offset, ...), code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            foreach (var opd in opds) {
                if (opd.ResultType != TypeEnum.Int) {
                    err.AppendFormat("cascadeptr must only has integer arguments, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                    break;
                }
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assign api result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
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
                            codes.Add(EncodeOpcode(InsEnum.CASCADEPTR, opds.Count, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                        }
                    }
                    else {
                        codes.Add(EncodeOpcode(InsEnum.CASCADEPTR, opds.Count, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    }
                    for (int i = 0; i < opds.Count; i += 2) {
                        int opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
                        }
                        int opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            opd2 = EncodeOperand2Helper(ref opdInfo);
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
                    codes.Add(EncodeOpcode(InsEnum.CASCADEPTR, opds.Count, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    for (int i = 0; i < opds.Count; i += 2) {
                        int opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            opd1 = EncodeOperand1Helper(ref opdInfo);
                        }
                        int opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            opd2 = EncodeOperand2Helper(ref opdInfo);
                        }
                        codes.Add(opd1 | opd2);
                    }
                }
            }
        }
        private void TryGenStackIndex(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 0) {
                err.AppendFormat("stkix must has zero arguments, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
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
                            codes.Add(EncodeOpcode(InsEnum.STKIX, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                            TryGenConvert(codes, semanticInfo, rinfo);
                        }
                    }
                    else {
                        codes.Add(EncodeOpcode(InsEnum.STKIX, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
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
                    codes.Add(EncodeOpcode(InsEnum.STKIX, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                }
            }
        }
        private void TryGenHookId(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 0) {
                err.AppendFormat("hookid must has zero arguments, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
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
                            codes.Add(EncodeOpcode(InsEnum.HOOKID, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                            TryGenConvert(codes, semanticInfo, rinfo);
                        }
                    }
                    else {
                        codes.Add(EncodeOpcode(InsEnum.HOOKID, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
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
                    codes.Add(EncodeOpcode(InsEnum.HOOKID, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                }
            }
        }
        private void TryGenHookVer(List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            if (opds.Count != 0) {
                err.AppendFormat("hookver must has zero arguments, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    if (semanticInfo.TargetCount > 0) {
                        err.AppendFormat("Can't assign calc result to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
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
                            codes.Add(EncodeOpcode(InsEnum.HOOKVER, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                            TryGenConvert(codes, semanticInfo, rinfo);
                        }
                    }
                    else {
                        codes.Add(EncodeOpcode(InsEnum.HOOKVER, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
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
                    codes.Add(EncodeOpcode(InsEnum.HOOKVER, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                }
            }
        }
        private void TryGenFFI(ProtoInfo proto, List<int> codes, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            InsEnum op = CheckType(proto, opds, err, comp);
            if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                if (IsGlobalBlock()) {
                }
                else {
                    for (int i = 0; i < opds.Count; ++i) {
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            if (i == 0)
                                ConvertArgument(codes, TypeEnum.Int, ref opdInfo);
                            else
                                ConvertArgument(codes, proto, i - 1, ref opdInfo);
                            opds[i] = opdInfo;
                        }
                    }
                    //gen api call
                    var rinfo = semanticInfo;
                    if (semanticInfo.TargetType != proto.Type) {
                        int tmpIndex = -1;
                        switch (proto.Type) {
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
                            rinfo.ResultType = proto.Type;
                            rinfo.ResultCount = 0;
                            rinfo.ResultIndex = tmpIndex;
                            rinfo.ResultValues = null;
                            codes.Add(EncodeOpcode(op, opds.Count, rinfo.IsGlobal, rinfo.ResultType, rinfo.ResultIndex));
                        }
                    }
                    else {
                        codes.Add(EncodeOpcode(op, opds.Count, semanticInfo.TargetIsGlobal, semanticInfo.TargetType, semanticInfo.TargetIndex));
                    }
                    if (op != InsEnum.FFIAUTO) {
                        codes.Add(EncodeOperand1(proto.IntParams.Count) | EncodeOperand2(proto.FloatParams.Count));
                    }
                    for (int i = 0; i < opds.Count; i += 2) {
                        int opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        int opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        codes.Add(opd1 | opd2);
                    }
                    if (semanticInfo.TargetType != proto.Type) {
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
                switch (proto.Type) {
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
                    semanticInfo.ResultType = proto.Type;
                    semanticInfo.ResultCount = 0;
                    semanticInfo.ResultIndex = tmpIndex;
                    semanticInfo.ResultValues = null;

                    for (int i = 0; i < opds.Count; ++i) {
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            if (i == 0)
                                ConvertArgument(codes, TypeEnum.Int, ref opdInfo);
                            else
                                ConvertArgument(codes, proto, i - 1, ref opdInfo);
                            opds[i] = opdInfo;
                        }
                    }
                    //gen api call
                    codes.Add(EncodeOpcode(op, opds.Count, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));
                    if (op != InsEnum.FFIAUTO) {
                        codes.Add(EncodeOperand1(proto.IntParams.Count) | EncodeOperand2(proto.FloatParams.Count));
                    }
                    for (int i = 0; i < opds.Count; i += 2) {
                        int opd1 = 0;
                        if (i < opds.Count) {
                            var opdInfo = opds[i];
                            opd1 = EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        int opd2 = 0;
                        if (i + 1 < opds.Count) {
                            var opdInfo = opds[i + 1];
                            opd2 = EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
                        }
                        codes.Add(opd1 | opd2);
                    }
                }
            }
        }

        private void TryGenMov(bool isGlobal, TypeEnum type, int vindex, List<int> codes, SemanticInfo info, StringBuilder err, Dsl.ISyntaxComponent comp)
        {
            //gen mov
            ConvertArgument(ref info);
            if (type == info.ResultType) {
                if (type == TypeEnum.Int)
                    codes.Add(EncodeOpcode(InsEnum.MOV, isGlobal, type, vindex));
                else if (type == TypeEnum.Float)
                    codes.Add(EncodeOpcode(InsEnum.MOVFLT, isGlobal, type, vindex));
                else if (type == TypeEnum.String)
                    codes.Add(EncodeOpcode(InsEnum.MOVSTR, isGlobal, type, vindex));
                codes.Add(EncodeOperand1(info.IsGlobal, info.ResultType, info.ResultIndex));
            }
            else {
                TryGenConvert(codes, isGlobal, type, vindex, info);
            }
        }
        private bool TryGenConstResult(List<int> codes, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo semanticInfo)
        {
            bool ret = false;
            if (null != semanticInfo.ResultValues) {
                if (semanticInfo.TargetOperation == TargetOperationEnum.VarAssign) {
                    if (IsGlobalBlock()) {
                    }
                    else {
                        if (semanticInfo.TargetCount > 0) {
                            err.AppendFormat("Can't assign a value to a array, code:{0}, line:{1}", comp.ToScriptString(false), comp.GetLine());
                            err.AppendLine();
                        }
                        //gen write result
                        var info = semanticInfo;
                        ConvertArgument(codes, semanticInfo.TargetType, ref info);
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
                ret = true;
            }
            return ret;
        }
        private void ConvertArgument(ref SemanticInfo opdInfo)
        {
            if (null != opdInfo.ResultValues) {
                Debug.Assert(opdInfo.ResultCount == 0 && opdInfo.ResultValues.Count == 1);
                int index = AddConst(opdInfo.ResultType, opdInfo.ResultValues[0]);
                opdInfo.IsGlobal = true;
                opdInfo.ResultIndex = index;
                opdInfo.ResultValues = null;
            }
        }
        private void ConvertArgument(List<int> codes, TypeEnum paramType, ref SemanticInfo opdInfo)
        {
            ConvertArgument(ref opdInfo);
            if (paramType != opdInfo.ResultType) {
                if (TryGenConvert(codes, paramType, opdInfo, out var newInfo)) {
                    opdInfo = newInfo;
                }
            }
        }
        private void ConvertArgument(List<int> codes, ApiInfo api, int argIx, StringBuilder err, Dsl.ISyntaxComponent comp, ref SemanticInfo opdInfo)
        {
            var ptype = api.GetParamType(argIx, opdInfo.ResultType, out var ct);
            if (ct == 0) {
                ConvertArgument(codes, ptype, ref opdInfo);
            }
            else if (ptype != opdInfo.ResultType || ct != opdInfo.ResultCount) {
                err.AppendFormat("can't implicit cast array argument {0}, api '{1}', code:{2}, line:{3}", argIx, api.Name, comp, comp.GetLine());
            }
        }
        private void ConvertArgument(List<int> codes, ProtoInfo proto, int argIx, ref SemanticInfo opdInfo)
        {
            var ptype = proto.GetParamType(argIx, opdInfo.ResultType);
            ConvertArgument(codes, ptype, ref opdInfo);
        }
        private int EncodeOperand1Helper(ref SemanticInfo opdInfo)
        {
            ConvertArgument(ref opdInfo);
            return EncodeOperand1(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
        }
        private int EncodeOperand2Helper(ref SemanticInfo opdInfo)
        {
            ConvertArgument(ref opdInfo);
            return EncodeOperand2(opdInfo.IsGlobal, opdInfo.ResultType, opdInfo.ResultIndex);
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
            InsEnum op = DeduceConvert(type, opd);
            bool ret = false;
            if (op != InsEnum.NUM) {
                codes.Add(EncodeOpcode(op, isGlobal, type, index));
                int opd1 = EncodeOperand1Helper(ref opd);
                codes.Add(opd1);
                ret = true;
            }
            return ret;
        }
        private bool TryGenConvert(List<int> codes, TypeEnum type, VarInfo src, out SemanticInfo semanticInfo)
        {
            var opd = new SemanticInfo { IsGlobal = src.IsGlobal, ResultType = src.Type, ResultIndex = src.Index };
            return TryGenConvert(codes, type, opd, out semanticInfo);
        }
        private bool TryGenConvert(List<int> codes, TypeEnum type, SemanticInfo opd, out SemanticInfo semanticInfo)
        {
            InsEnum op = DeduceConvert(type, opd);
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
                    codes.Add(EncodeOpcode(op, semanticInfo.IsGlobal, semanticInfo.ResultType, semanticInfo.ResultIndex));

                    int opd1 = EncodeOperand1Helper(ref opd);
                    codes.Add(opd1);
                    ret = true;
                }
            }
            return ret;
        }
        private InsEnum DeduceConvert(TypeEnum type, SemanticInfo opd)
        {
            InsEnum op = InsEnum.NUM;
            if (opd.ResultType == TypeEnum.Int) {
                if (type == TypeEnum.Float) {
                    op = InsEnum.CASTINTFLT;
                }
                else if (type == TypeEnum.String) {
                    op = InsEnum.INT2STR;
                }
            }
            else if (opd.ResultType == TypeEnum.Float) {
                if (type == TypeEnum.Int) {
                    op = InsEnum.CASTFLTINT;
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
            return op;
        }
        private TypeEnum DeduceType(InsEnum op, List<SemanticInfo> opds, out List<TypeEnum> casts, out InsEnum newOp)
        {
            casts = new List<TypeEnum>();
            newOp = op;
            if (opds.Count == 1) {
                if (op == InsEnum.NOT || op == InsEnum.BITNOT) {
                    casts.Add(TypeEnum.Int);
                }
                else if (opds[0].ResultType == TypeEnum.String) {
                    //try to convert string to float for unary operator
                    casts.Add(TypeEnum.Float);
                }
                else {
                    casts.Add(opds[0].ResultType);
                }
                if (op == InsEnum.NOT || op == InsEnum.BITNOT)
                    return TypeEnum.Int;
                if (op == InsEnum.NEG && opds[0].ResultType != TypeEnum.Int) {
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
                        if (opds[0].ResultType == TypeEnum.String || opds[1].ResultType == TypeEnum.String) {
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
                        if (opds[0].ResultType == TypeEnum.String || opds[1].ResultType == TypeEnum.String) {
                            //try to convert string to float for binary operator
                            casts.Add(TypeEnum.Float);
                            casts.Add(TypeEnum.Float);
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
                if (type1 == TypeEnum.String || type2 == TypeEnum.String) {
                    if (op == InsEnum.ADD)
                        return TypeEnum.String;
                    else
                        return TypeEnum.Float;
                }
                else if (type1 == TypeEnum.Float || type2 == TypeEnum.Float) {
                    return TypeEnum.Float;
                }
                else {
                    return TypeEnum.Int;
                }
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
                            TryParseDouble(opd.ResultValues[0], out var val);
                            ret.Add((-val).ToString());
                        }
                        else {
                            TryParseLong(opd.ResultValues[0], out var val);
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
                            TryParseDouble(opd1.ResultValues[0], out var val1);
                            TryParseDouble(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 + val2).ToString());
                        }
                        else {
                            TryParseLong(opd1.ResultValues[0], out var val1);
                            TryParseLong(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 + val2).ToString());
                        }
                    }
                    break;
                case InsEnum.SUB:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
                            TryParseDouble(opd1.ResultValues[0], out var val1);
                            TryParseDouble(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 - val2).ToString());
                        }
                        else {
                            TryParseLong(opd1.ResultValues[0], out var val1);
                            TryParseLong(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 - val2).ToString());
                        }
                    }
                    break;
                case InsEnum.MUL:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
                            TryParseDouble(opd1.ResultValues[0], out var val1);
                            TryParseDouble(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 * val2).ToString());
                        }
                        else {
                            TryParseLong(opd1.ResultValues[0], out var val1);
                            TryParseLong(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 * val2).ToString());
                        }
                    }
                    break;
                case InsEnum.DIV:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
                            TryParseDouble(opd1.ResultValues[0], out var val1);
                            TryParseDouble(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 / val2).ToString());
                        }
                        else {
                            TryParseLong(opd1.ResultValues[0], out var val1);
                            TryParseLong(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 / val2).ToString());
                        }
                    }
                    break;
                case InsEnum.MOD:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
                            TryParseDouble(opd1.ResultValues[0], out var val1);
                            TryParseDouble(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 % val2).ToString());
                        }
                        else {
                            TryParseLong(opd1.ResultValues[0], out var val1);
                            TryParseLong(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 % val2).ToString());
                        }
                    }
                    break;
                case InsEnum.AND:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
                            TryParseDouble(opd1.ResultValues[0], out var val1);
                            TryParseDouble(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 != 0 && val2 != 0).ToString());
                        }
                        else {
                            TryParseLong(opd1.ResultValues[0], out var val1);
                            TryParseLong(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 != 0 && val2 != 0).ToString());
                        }
                    }
                    break;
                case InsEnum.OR:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
                            TryParseDouble(opd1.ResultValues[0], out var val1);
                            TryParseDouble(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 != 0 || val2 != 0).ToString());
                        }
                        else {
                            TryParseLong(opd1.ResultValues[0], out var val1);
                            TryParseLong(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 != 0 || val2 != 0).ToString());
                        }
                    }
                    break;
                case InsEnum.NOT:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        if (opd.ResultType == TypeEnum.Float) {
                            TryParseDouble(opd.ResultValues[0], out var val);
                            ret.Add((val == 0).ToString());
                        }
                        else {
                            TryParseLong(opd.ResultValues[0], out var val);
                            ret.Add((val == 0).ToString());
                        }
                    }
                    break;
                case InsEnum.GT:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
                            TryParseDouble(opd1.ResultValues[0], out var val1);
                            TryParseDouble(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 > val2 ? 1 : 0).ToString());
                        }
                        else {
                            TryParseLong(opd1.ResultValues[0], out var val1);
                            TryParseLong(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 > val2 ? 1 : 0).ToString());
                        }
                    }
                    break;
                case InsEnum.GE:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
                            TryParseDouble(opd1.ResultValues[0], out var val1);
                            TryParseDouble(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 >= val2 ? 1 : 0).ToString());
                        }
                        else {
                            TryParseLong(opd1.ResultValues[0], out var val1);
                            TryParseLong(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 >= val2 ? 1 : 0).ToString());
                        }
                    }
                    break;
                case InsEnum.EQ:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
                            TryParseDouble(opd1.ResultValues[0], out var val1);
                            TryParseDouble(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 == val2 ? 1 : 0).ToString());
                        }
                        else {
                            TryParseLong(opd1.ResultValues[0], out var val1);
                            TryParseLong(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 == val2 ? 1 : 0).ToString());
                        }
                    }
                    break;
                case InsEnum.NE:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
                            TryParseDouble(opd1.ResultValues[0], out var val1);
                            TryParseDouble(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 != val2 ? 1 : 0).ToString());
                        }
                        else {
                            TryParseLong(opd1.ResultValues[0], out var val1);
                            TryParseLong(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 != val2 ? 1 : 0).ToString());
                        }
                    }
                    break;
                case InsEnum.LE:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
                            TryParseDouble(opd1.ResultValues[0], out var val1);
                            TryParseDouble(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 <= val2 ? 1 : 0).ToString());
                        }
                        else {
                            TryParseLong(opd1.ResultValues[0], out var val1);
                            TryParseLong(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 <= val2 ? 1 : 0).ToString());
                        }
                    }
                    break;
                case InsEnum.LT:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        if (opd1.ResultType == TypeEnum.Float || opd2.ResultType == TypeEnum.Float) {
                            TryParseDouble(opd1.ResultValues[0], out var val1);
                            TryParseDouble(opd2.ResultValues[1], out var val2);
                            ret.Add((val1 < val2 ? 1 : 0).ToString());
                        }
                        else {
                            TryParseLong(opd1.ResultValues[0], out var val1);
                            TryParseLong(opd2.ResultValues[0], out var val2);
                            ret.Add((val1 < val2 ? 1 : 0).ToString());
                        }
                    }
                    break;
                case InsEnum.LSHIFT:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        TryParseLong(opd1.ResultValues[0], out var val1);
                        int.TryParse(opd2.ResultValues[0], out var val2);
                        ret.Add((val1 << val2).ToString());
                    }
                    break;
                case InsEnum.RSHIFT:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        TryParseLong(opd1.ResultValues[0], out var val1);
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
                        TryParseLong(opd1.ResultValues[0], out var val1);
                        TryParseLong(opd2.ResultValues[0], out var val2);
                        ret.Add((val1 & val2).ToString());
                    }
                    break;
                case InsEnum.BITOR:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        TryParseLong(opd1.ResultValues[0], out var val1);
                        TryParseLong(opd2.ResultValues[0], out var val2);
                        ret.Add((val1 | val2).ToString());
                    }
                    break;
                case InsEnum.BITXOR:
                    if (opds.Count == 2) {
                        var opd1 = opds[0];
                        var opd2 = opds[1];
                        TryParseLong(opd1.ResultValues[0], out var val1);
                        TryParseLong(opd2.ResultValues[0], out var val2);
                        ret.Add((val1 ^ val2).ToString());
                    }
                    break;
                case InsEnum.BITNOT:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        TryParseLong(opd.ResultValues[0], out var val);
                        ret.Add((~val).ToString());
                    }
                    break;
                case InsEnum.INT2STR:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        TryParseLong(opd.ResultValues[0], out var val);
                        ret.Add(val.ToString());
                    }
                    break;
                case InsEnum.FLT2STR:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        TryParseDouble(opd.ResultValues[0], out var val);
                        ret.Add(val.ToString());
                    }
                    break;
                case InsEnum.STR2INT:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        TryParseLong(opd.ResultValues[0], out var val);
                        ret.Add(val.ToString());
                    }
                    break;
                case InsEnum.STR2FLT:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        TryParseDouble(opd.ResultValues[0], out var val);
                        ret.Add(val.ToString());
                    }
                    break;
                case InsEnum.CASTFLTINT:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        TryParseDouble(opd.ResultValues[0], out var val);
                        ret.Add(((long)val).ToString());
                    }
                    break;
                case InsEnum.CASTSTRINT:
                    ret = null;
                    break;
                case InsEnum.CASTINTFLT:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        TryParseLong(opd.ResultValues[0], out var val);
                        ret.Add(((double)val).ToString());
                    }
                    break;
                case InsEnum.CASTINTSTR:
                    ret = null;
                    break;
                case InsEnum.ASINT:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        TryParseDouble(opd.ResultValues[0], out var val);
                        int v;
                        float fv = (float)val;
                        unsafe {
                            v = *(int*)&fv;
                        }
                        ret.Add(v.ToString());
                    }
                    break;
                case InsEnum.ASFLOAT:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        TryParseLong(opd.ResultValues[0], out var val);
                        float v;
                        int iv = (int)val;
                        unsafe {
                            v = *(float*)&iv;
                        }
                        ret.Add(v.ToString());
                    }
                    break;
                case InsEnum.ASLONG:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        TryParseDouble(opd.ResultValues[0], out var val);
                        long v;
                        unsafe {
                            v = *(long*)&val;
                        }
                        ret.Add(v.ToString());
                    }
                    break;
                case InsEnum.ASDOUBLE:
                    if (opds.Count == 1) {
                        var opd = opds[0];
                        TryParseLong(opd.ResultValues[0], out var val);
                        double v;
                        unsafe {
                            v = *(double*)&val;
                        }
                        ret.Add(v.ToString());
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
            if (opds.Count < api.MinParamNum) {
                err.AppendFormat("'{0}' must have at least {1} arguments, code:{2}, line:{3}", api.Name, api.MinParamNum, comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            for (int i = 0; i < opds.Count; ++i) {
                TypeEnum rtype = opds[i].ResultType;
                int rct = opds[i].ResultCount;
                TypeEnum type = api.GetParamType(i, rtype, out var ct);
                if (type != rtype) {
                    err.AppendFormat("'{0}' argument {1}'s type '{2}' dismatch '{3}', code:{4}, line:{5}", api.Name, i, rtype, type, comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                }
                else if (ct != rct) {
                    err.AppendFormat("'{0}' argument {1}'s count '{2}' dismatch '{3}', code:{4}, line:{5}", api.Name, i, rct, ct, comp.ToScriptString(false), comp.GetLine());
                    err.AppendLine();
                }
            }
        }
        private InsEnum CheckType(ProtoInfo proto, List<SemanticInfo> opds, StringBuilder err, Dsl.ISyntaxComponent comp)
        {
            int minParamNum = 1 + proto.IntParams.Count + proto.FloatParams.Count + proto.MinStackParamNum;
            if (opds.Count < minParamNum) {
                err.AppendFormat("ffi '{0}' must have at least {1} arguments, code:{2}, line:{3}", proto.Name, minParamNum, comp.ToScriptString(false), comp.GetLine());
                err.AppendLine();
            }
            for (int i = 0; i < opds.Count; ++i) {
                TypeEnum rtype = opds[i].ResultType;
                if (i == 0) {
                    //addr
                    if (TypeEnum.Int != rtype) {
                        err.AppendFormat("argument {0} addr's type '{1}' dismatch '{2}', code:{3}, line:{4}", i, rtype, TypeEnum.Int, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
                else {
                    TypeEnum type = proto.GetParamType(i - 1, rtype);
                    if (type != rtype) {
                        err.AppendFormat("'{0}' argument {1}'s type '{2}' dismatch '{3}', code:{4}, line:{5}", proto.Name, i, rtype, type, comp.ToScriptString(false), comp.GetLine());
                        err.AppendLine();
                    }
                }
            }
            InsEnum op = InsEnum.FFIMANUAL;
            if (proto.ManualStack) {
                if(proto.DoubleFloat)
                    op = InsEnum.FFIMANUALSTACKDBL;
                else
                    op = InsEnum.FFIMANUALSTACK;
            }
            else if (proto.DoubleFloat) {
                op = InsEnum.FFIMANUALDBL;
            }
            else if (proto.Params == ParamsEnum.NoParams && proto.FloatParams.Count == 0 && proto.StackParams.Count == 0) {
                op = InsEnum.FFIAUTO;
            }
            return op;
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
        private int EncodeOperand1(int num)
        {
            return (num & 0xffff);
        }
        private int EncodeOperand2(int num)
        {
            int operand = EncodeOperand1(num);
            return operand << 16;
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
        private void DecodeOperand1(int operand, out int num)
        {
            num = (operand & 0xffff);
        }
        private void DecodeOperand2(int operand, out int num)
        {
            operand >>= 16;
            DecodeOperand1(operand, out num);
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
            int index = -1;
            switch (type) {
                case TypeEnum.Int:
                    if (!m_IntConstInfos.TryGetValue(val, out index)) {
                        index = m_ConstIndexInfo.NextIntIndex--;
                        m_IntConstInfos.Add(val, index);
                    }
                    break;
                case TypeEnum.Float:
                    if (!m_FltConstInfos.TryGetValue(val, out index)) {
                        index = m_ConstIndexInfo.NextFloatIndex--;
                        m_FltConstInfos.Add(val, index);
                    }
                    break;
                case TypeEnum.String:
                    if (!m_StrConstInfos.TryGetValue(val, out index)) {
                        index = m_ConstIndexInfo.NextStringIndex--;
                        m_StrConstInfos.Add(val, index);
                    }
                    break;
            }
            return index;
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
            m_TempCodes.Clear();
            m_StructInfos.Clear();
            m_ConstIndexInfo.Reset();
            m_IntConstInfos.Clear();
            m_FltConstInfos.Clear();
            m_StrConstInfos.Clear();
            m_VarInfos.Clear();
            m_Hooks.Clear();
            m_HookParseStack.Clear();
            m_LexicalScopeStack.Clear();
            m_ToplevelLexicalScopeInfo = new LexicalScopeInfo();
            m_LastBlockId = 0;
            m_UniqueNumber = 0;
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
        public sealed class ApiParamType
        {
            public TypeEnum Type = TypeEnum.NotUse;
            public int Count = 0;
        }
        public sealed class ApiInfo
        {
            public bool isExtern = false;
            public int ApiId = 0;
            public string Name = string.Empty;
            public TypeEnum Type = TypeEnum.NotUse;
            public ParamsEnum Params = ParamsEnum.NoParams;
            public List<ApiParamType> ParamTypes = new List<ApiParamType>();
            public int MinParamNum = 0;

            public TypeEnum GetParamType(int ix, TypeEnum argType, out int count)
            {
                count = 0;
                TypeEnum paramType = TypeEnum.NotUse;
                if (ix < ParamTypes.Count) {
                    var pt = ParamTypes[ix];
                    paramType = pt.Type;
                    count = pt.Count;
                }
                else if (Params == ParamsEnum.LastType) {
                    if (ParamTypes.Count > 0) {
                        var pt = ParamTypes[ParamTypes.Count - 1];
                        paramType = pt.Type;
                        count = pt.Count;
                    }
                }
                else if (Params == ParamsEnum.Ints) {
                    paramType = TypeEnum.Int;
                }
                else if (Params == ParamsEnum.Floats) {
                    paramType = TypeEnum.Float;
                }
                else if (Params == ParamsEnum.Strings) {
                    paramType = TypeEnum.String;
                }
                else if (Params == ParamsEnum.Printf) {
                    paramType = argType;
                }
                return paramType;
            }
        }
        public sealed class ProtoInfo
        {
            public string Name = string.Empty;
            public TypeEnum Type = TypeEnum.NotUse;
            public ParamsEnum Params = ParamsEnum.NoParams;
            public List<TypeEnum> IntParams = new List<TypeEnum>();
            public List<TypeEnum> FloatParams = new List<TypeEnum>();
            public List<TypeEnum> StackParams = new List<TypeEnum>();
            public int MinStackParamNum = 0;
            public bool ManualStack = false;
            public bool DoubleFloat = false;

            public TypeEnum GetParamType(int ix, TypeEnum argType)
            {
                TypeEnum paramType = TypeEnum.NotUse;
                if (ix < IntParams.Count) {
                    paramType = IntParams[ix];
                }
                else if (ix < IntParams.Count + FloatParams.Count) {
                    paramType = FloatParams[ix - IntParams.Count];
                }
                else if (ix < IntParams.Count + FloatParams.Count + StackParams.Count) {
                    paramType = StackParams[ix - IntParams.Count - FloatParams.Count];
                }
                else if (Params == ParamsEnum.LastType) {
                    if (StackParams.Count > 0) {
                        paramType = StackParams[StackParams.Count - 1];
                    }
                }
                else if (Params == ParamsEnum.Ints) {
                    paramType = TypeEnum.Int;
                }
                else if (Params == ParamsEnum.Floats) {
                    paramType = TypeEnum.Float;
                }
                else if (Params == ParamsEnum.Strings) {
                    paramType = TypeEnum.String;
                }
                else if (Params == ParamsEnum.Printf) {
                    paramType = argType;
                }
                return paramType;
            }
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

            public void Reset()
            {
                NextIntIndex = c_max_variable_table_size - 1;
                NextFloatIndex = c_max_variable_table_size - 1;
                NextStringIndex = c_max_variable_table_size - 1;
            }
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
        private Dictionary<string, int> m_IntConstInfos = new Dictionary<string, int>();
        private Dictionary<string, int> m_FltConstInfos = new Dictionary<string, int>();
        private Dictionary<string, int> m_StrConstInfos = new Dictionary<string, int>();
        private Dictionary<string, Dictionary<int, VarInfo>> m_VarInfos = new Dictionary<string, Dictionary<int, VarInfo>>();
        public List<HookInfo> m_Hooks = new List<HookInfo>();

        private Stack<HookInfo> m_HookParseStack = new Stack<HookInfo>();
        private Stack<LexicalScopeInfo> m_LexicalScopeStack = new Stack<LexicalScopeInfo>();
        private LexicalScopeInfo m_ToplevelLexicalScopeInfo = new LexicalScopeInfo();
        private int m_LastBlockId = 0;
        private int m_UniqueNumber = 0;

        private Dictionary<string, ApiInfo> m_Apis = new Dictionary<string, ApiInfo>();
        private Dictionary<string, ProtoInfo> m_Protos = new Dictionary<string, ProtoInfo>();
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
            else if (type == TypeEnum.String) {
                return (int)TypeEnum.String << 13;
            }
            else {
                return (int)TypeEnum.NotUse << 13;
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

        private static bool TryParseLong(string str, out long val)
        {
            string type = string.Empty;
            double dval;
            bool r = TryParseNumeric(str, ref type, out val, out dval);
            if (type == "float") {
                val = (long)dval;
            }
            return r;
        }
        private static bool TryParseDouble(string str, out double val)
        {
            string type = string.Empty;
            long lval;
            bool r = TryParseNumeric(str, ref type, out lval, out val);
            if (type != "float") {
                val = lval;
            }
            return r;
        }
        private static bool TryParseNumeric(string str, ref string type, out long lval, out double dval)
        {
            bool ret = false;
            lval = 0;
            dval = 0;
            if (str.Length > 2 && str[0] == '0' && str[1] == 'x') {
                char c = str[str.Length - 1];
                if (c == 'u' || c == 'U') {
                    str = str.Substring(0, str.Length - 1);
                }
                if (ulong.TryParse(str.Substring(2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out var v)) {
                    str = v.ToString();
                }
                type = "uint";
            }
            else if (str.Length >= 2) {
                char c = str[str.Length - 1];
                if (c == 'u' || c == 'U') {
                    str = str.Substring(0, str.Length - 1);
                    type = "uint";
                }
                else if (c == 'f' || c == 'F') {
                    str = str.Substring(0, str.Length - 1);
                    c = str[str.Length - 1];
                    if (c == 'l' || c == 'L') {
                        str = str.Substring(0, str.Length - 1);
                    }
                    type = "float";
                }
            }
            if (type == "float" || str.IndexOfAny(s_FloatExponent) > 0) {
                if (double.TryParse(str, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out var v)) {
                    dval = v;
                    type = "float";
                    ret = true;
                }
            }
            else if (str.Length > 1 && str[0] == '0') {
                ulong v = Convert.ToUInt64(str, 8);
                lval = (long)v;
                type = "uint";
                ret = true;
            }
            else if (long.TryParse(str, out var lv)) {
                lval = lv;
                if (lv > int.MaxValue || type == "uint") {
                    type = "uint";
                }
                else {
                    type = "int";
                }
                ret = true;
            }
            else if (ulong.TryParse(str, out var uv)) {
                lval = (long)uv;
                type = "uint";
                ret = true;
            }
            return ret;
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
        private static char[] s_FloatExponent = new char[] { 'e', 'E', '.' };

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
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CppDbgScp_PauseVM();
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CppDbgScp_ResumeVM();

#pragma warning disable CA2101
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void CppDbgScp_Load([MarshalAs(UnmanagedType.LPStr)] string file);

        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static int Test1Export(int a, double b, [MarshalAs(UnmanagedType.LPStr)] string c);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static int Test2Export(int a, double b, [MarshalAs(UnmanagedType.LPStr)] string c);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void Test3Export(int a, double b, [MarshalAs(UnmanagedType.LPStr)] string c);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void Test4Export(int a, double b, [MarshalAs(UnmanagedType.LPStr)] string c);

        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static int TestMacro1Export(int a, double b, [MarshalAs(UnmanagedType.LPStr)] string c);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static int TestMacro2Export(int a, double b, [MarshalAs(UnmanagedType.LPStr)] string c);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void TestMacro3Export(int a, double b, [MarshalAs(UnmanagedType.LPStr)] string c);
        [DllImport("DebugScriptVM", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void TestMacro4Export(int a, double b, [MarshalAs(UnmanagedType.LPStr)] string c);
#pragma warning restore CA2101
    }
}
