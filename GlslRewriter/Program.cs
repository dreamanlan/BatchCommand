using System.Text;
using System.Diagnostics;
using static GlslRewriter.Program;
using Dsl;

namespace GlslRewriter
{
    //目前主要用于将反编译（spirv-cross或yuzu）出来的glsl转换为SSA形式与表达式合并，表达式合并（借助计算图来生成）只用于生成注释，方便理解代码
    //可能不适合用于通常的glsl代码
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2) {
                Console.WriteLine("[usage]GlslRewriter input.glsl output.glsl");
                return;
            }
            Transform(args[0], args[1]);
        }

        private static void Transform(string srcFile, string outFile)
        {
            File.Delete(outFile);
            var glslFileLines = File.ReadAllLines(srcFile);
            var glslLines = RemovePreprocessLines(glslFileLines);
            string glslTxt = PreprocessCondExp(glslLines);

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
            file.onBeforeAddFunction = (ref Dsl.Common.DslAction dslAction, Dsl.StatementData statement) => {
                string sid = statement.GetId();
                var func = statement.Last.AsFunction;
                if (null != func) {
                    if (func.HaveStatement()) {
                        if (string.IsNullOrEmpty(sid) || sid == "for" || sid == "while" || sid == "else" || sid == "switch") {
                            //结束当前语句并开始一个新的空语句
                            dslAction.endStatement();
                            dslAction.beginStatement();
                            return true;
                        }
                    }
                    else {
                        if (sid == "do") {
                            //结束当前语句并开始一个新的空语句
                            dslAction.endStatement();
                            dslAction.beginStatement();
                            return true;
                        }
                    }
                }
                return false;
            };
            file.onAddFunction = (ref Dsl.Common.DslAction dslAction, Dsl.StatementData statement, Dsl.FunctionData function) => {
                //这里不要改变程序结构，此时function还是一个空函数，真正的函数信息还没有填充，这里与onBeforeAddFunction的区别是此时构建了function并添加到了当前语句的函数表中
                return false;
            };
            file.onBeforeEndStatement = (ref Dsl.Common.DslAction dslAction, Dsl.StatementData statement) => {
                //这里可拆分语句
                return false;
            };
            file.onEndStatement = (ref Dsl.Common.DslAction dslAction, ref Dsl.StatementData statement) => {
                //这里可替换整个语句，但不要修改程序其它部分结构，这里与onBeforeEndStatement的区别是此时语句已经从栈里弹出，后续将化简再加入上层语法单位
                return false;
            };
            file.onBeforeBuildOperator = (ref Dsl.Common.DslAction dslAction, string op, Dsl.StatementData statement) => {
                //这里拆分语句
                string sid = statement.GetId();
                var func = statement.Last.AsFunction;
                if (null != func) {
                    if (sid == "if") {
                        statement.Functions.Remove(func);
                        dslAction.endStatement();
                        dslAction.beginStatement();
                        var stm = dslAction.getCurStatement();
                        stm.AddFunction(func);
                        return true;
                    }
                }
                return false;
            };
            file.onBuildOperator = (ref Dsl.Common.DslAction dslAction, string op, ref Dsl.StatementData statement) => {
                //这里可替换语句，不要修改其它语法结构
                return false;
            };
            file.onSetFunctionId = (ref Dsl.Common.DslAction dslAction, string name, Dsl.StatementData statement, Dsl.FunctionData function) => {
                //这里可拆分语句
                string sid = statement.GetId();
                var func = statement.Last.AsFunction;
                if (null != func) {
                    if (sid == "if" && name != "else") {
                        statement.Functions.Remove(func);
                        dslAction.endStatement();
                        dslAction.beginStatement();
                        var stm = dslAction.getCurStatement();
                        stm.AddFunction(func);
                        return true;
                    }
                    else if (name == "struct") {
                        statement.Functions.Remove(func);
                        dslAction.endStatement();
                        dslAction.beginStatement();
                        var stm = dslAction.getCurStatement();
                        stm.AddFunction(func);
                        return true;
                    }
                }
                return false;
            };
            file.onSetMemberId = (ref Dsl.Common.DslAction dslAction, string name, Dsl.StatementData statement, Dsl.FunctionData function) => {
                //这里可拆分语句
                return false;
            };
            file.onBeforeBuildHighOrder = (ref Dsl.Common.DslAction dslAction, Dsl.StatementData statement, Dsl.FunctionData function) => {
                //这里可拆分语句
                return false;
            };
            file.onBuildHighOrder = (ref Dsl.Common.DslAction dslAction, Dsl.StatementData statement, Dsl.FunctionData function) => {
                //这里可拆分语句
                return false;
            };
            if (file.LoadFromString(glslTxt, msg => { Console.WriteLine(msg); })) {
                //glsl的语法是一个合法的dsl语法，但语义结构不同，我们尝试用dsl的表示来处理glsl语法
                Transform(file);
                file.Save(outFile);
            }
            else {
                Environment.Exit(-1);
            }

            var lineList = new List<string>();
            var lines = File.ReadAllLines(outFile);
            lineList.AddRange(lines);
            File.WriteAllLines(outFile, lineList.ToArray());
        }
        private static List<string> RemovePreprocessLines(IList<string> glslLines)
        {
            var lines = new List<string>();
            foreach(var line in glslLines) {
                if (line.TrimStart().StartsWith("#"))
                    continue;
                lines.Add(line);
            }
            return lines;
        }
        private static string PreprocessCondExp(IList<string> glslLines)
        {
            //c语言的?:操作符与赋值操作是相同优先级，这与MetaDSL里不一样，有可能条件表达式里会出现赋值表达式，我们需要把这些表达式括起来再进行dsl解析
            var sb = new StringBuilder();
            bool inCommentBlock = false;
            for (int ix = 0; ix < glslLines.Count; ++ix) {
                var line = glslLines[ix];
                int k = 0;
                if (inCommentBlock) {
                    for (int i = 0; i < line.Length; ++i) {
                        char ch = line[i];
                        if (ch == '*' && i < line.Length - 1 && line[i + 1] == '/') {
                            inCommentBlock = false;
                            k = i + 2;
                            break;
                        }
                    }
                    if (inCommentBlock)
                        continue;
                }
                var rline = line.Substring(k);
                if (rline.StartsWith("#line")) {
                    //ignore
                }
                else {
                    for (int i = k; i < line.Length; ++i) {
                        char c = line[i];
                        if (c == '\\') {
                            ++i;
                            sb.Append(c);
                            sb.Append(line[i]);
                        }
                        else if (s_CondExpStack.Count > 0 && (c == '(' || c == '[' || c == '{')) {
                            s_CondExpStack.Peek().IncParenthesisCount();
                            sb.Append(c);
                        }
                        else if (s_CondExpStack.Count > 0 && (c == ')' || c == ']' || c == '}')) {
                            if (s_CondExpStack.Peek().MaybeCompletePart(CondExpEnum.Colon)) {
                                s_CondExpStack.Pop();
                                sb.Append(')');
                            }
                            if (s_CondExpStack.Count > 0) {
                                s_CondExpStack.Peek().DecParenthesisCount();
                            }
                            sb.Append(c);
                        }
                        else if (c == '?') {
                            s_CondExpStack.Push(new CondExpInfo(CondExpEnum.Question));
                            sb.Append(c);
                            sb.Append(' ');
                            sb.Append('(');
                        }
                        else if (c == ':') {
                            while (s_CondExpStack.Count > 0 && s_CondExpStack.Peek().MaybeCompletePart(CondExpEnum.Colon)) {
                                s_CondExpStack.Pop();
                                sb.Append(')');
                            }
                            if (s_CondExpStack.Count > 0 && s_CondExpStack.Peek().MaybeCompletePart(CondExpEnum.Question)) {
                                s_CondExpStack.Pop();
                                s_CondExpStack.Push(new CondExpInfo(CondExpEnum.Colon));
                                sb.Append(')');
                                sb.Append(' ');
                                sb.Append(c);
                                sb.Append(' ');
                                sb.Append('(');
                            }
                            else {
                                sb.Append(c);
                            }
                        }
                        else if (c == ',' || c == ';') {
                            while (s_CondExpStack.Count > 0 && s_CondExpStack.Peek().MaybeCompletePart(CondExpEnum.Colon)) {
                                s_CondExpStack.Pop();
                                sb.Append(')');
                            }
                            sb.Append(c);
                        }
                        else if (c == '"') {
                            sb.Append(c);
                            for (int j = i + 1; j < line.Length; ++j) {
                                char ch = line[j];
                                sb.Append(ch);
                                if (ch == '\\') {
                                    ++j;
                                    sb.Append(line[j]);
                                }
                                else if (ch == '"') {
                                    i = j;
                                    break;
                                }
                            }
                        }
                        else {
                            char nc = i < line.Length - 1 ? line[i + 1] : '\0';
                            if (c == '/' && nc == '*') {
                                inCommentBlock = true;
                                for (int j = i + 2; j < line.Length; ++j) {
                                    char ch = line[j];
                                    if (ch == '*' && j < line.Length - 1 && line[j + 1] == '/') {
                                        inCommentBlock = false;
                                        i = j + 1;
                                        break;
                                    }
                                }
                                if (inCommentBlock)
                                    break;
                            }
                            else if (c == '/' && nc == '/') {
                                break;
                            }
                            else {
                                sb.Append(c);
                                char lc = i > 0 ? line[i - 1] : '\0';
                                if (c == '\'' && (!char.IsDigit(lc) || !char.IsDigit(nc))) {
                                    for (int j = i + 1; j < line.Length; ++j) {
                                        char ch = line[j];
                                        if (ch == '\\') {
                                            ++j;
                                            sb.Append(ch);
                                            sb.Append(line[j]);
                                        }
                                        else if (ch == '\'') {
                                            sb.Append(ch);
                                            i = j;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
        private static void Transform(Dsl.DslFile file)
        {
            var semanticInfo = new SemanticInfo();
            foreach (var dsl in file.DslInfos) {
                TransformToplevelSyntax(dsl, ref semanticInfo);
            }
        }

        //顶层语法处理
        private static void TransformToplevelSyntax(Dsl.ISyntaxComponent syntax, ref SemanticInfo semanticInfo)
        {
            var valData = syntax as Dsl.ValueData;
            var funcData = syntax as Dsl.FunctionData;
            var stmData = syntax as Dsl.StatementData;
            if (null != valData) {
                TransformToplevelValue(valData, ref semanticInfo);
            }
            else if (null != funcData) {
                TransformToplevelFunction(funcData, ref semanticInfo);
            }
            else if (null != stmData) {
                TransformToplevelStatement(stmData, 0, new List<Dsl.ValueOrFunctionData>(), ref semanticInfo);
            }
        }
        private static void TransformToplevelStatement(Dsl.StatementData stmData, int startFuncIx, List<Dsl.ValueOrFunctionData> modifiers, ref SemanticInfo semanticInfo)
        {
            string id = stmData.GetFunctionId(startFuncIx);
            if (id == "out") {
                var funcNum = stmData.GetFunctionNum();
                if (startFuncIx + 1 < funcNum) {
                    var func = stmData.GetFunction(startFuncIx + 1).AsFunction;
                    if (null != func) {
                        TransformOutFunc(func);
                    }
                    else {
                        TransformVar(stmData, startFuncIx, true, ref semanticInfo);
                    }
                }
            }
            else if (id == "in") {
                TransformVar(stmData, startFuncIx, true, ref semanticInfo);
            }
            else if (id == "layout") {
                modifiers.Add(stmData.GetFunction(startFuncIx));
                TransformToplevelStatement(stmData, startFuncIx + 1, modifiers, ref semanticInfo);
            }
            else if (id == "struct") {
                var funcNum = stmData.GetFunctionNum();
                if (startFuncIx + 1 < funcNum) {
                    var func = stmData.GetFunction(startFuncIx + 1).AsFunction;
                    if (null != func) {
                        var namePart = startFuncIx + 2 < funcNum ? stmData.GetFunction(startFuncIx + 2) : (Dsl.ValueOrFunctionData?)null;
                        TransformStruct(func, namePart);
                    }
                }
            }
            else if (id == "buffer") {
                var funcNum = stmData.GetFunctionNum();
                if (startFuncIx + 1 < funcNum) {
                    var func = stmData.GetFunction(startFuncIx + 1).AsFunction;
                    if (null != func) {
                        var namePart = startFuncIx + 2 < funcNum ? stmData.GetFunction(startFuncIx + 2) : (Dsl.ValueOrFunctionData?)null;
                        TransformBuffer(func, string.Empty, namePart);
                    }
                }
            }
            else if (id == "uniform") {
                var funcNum = stmData.GetFunctionNum();
                if (startFuncIx + 1 < funcNum) {
                    var func = stmData.GetFunction(startFuncIx + 1).AsFunction;
                    if (null != func) {
                        var namePart = startFuncIx + 2 < funcNum ? stmData.GetFunction(startFuncIx + 2) : (Dsl.ValueOrFunctionData?)null;
                        TransformUniformFunc(func, namePart);
                    }
                    else {
                        TransformVar(stmData, startFuncIx, true, ref semanticInfo);
                    }
                }
            }
            else {
                //在dsl语法里，分号分隔各语句，glsl里的函数定义结尾不加分隔符，dsl解析时会将函数定义与后面的函数定义或struct/buffer/变量定义
                //连接在一起构成一个大语句，这里需要分别拆出来分析（为保证输出时的正确性，不能修改整体表示）
                int index = 0;
                bool handled = false;
                while (index < stmData.GetFunctionNum()) {
                    int startIndex = index;
                    string fid = stmData.GetFunctionId(startIndex);
                    if (fid == "layout" || fid == "struct" || fid == "uniform" || fid == "out" || fid == "in") {
                        TransformToplevelStatement(stmData, startIndex, new List<ValueOrFunctionData>(), ref semanticInfo);
                        handled = true;
                        break;
                    }
                    else {
                        bool existsFunc = ParseStatement(stmData, ref index, out var f, out var fmodifiers);
                        if (existsFunc) {
                            Debug.Assert(null != f);
                            TransformFuncDef(f, fmodifiers);
                            handled = true;
                        }
                        else {
                            handled = false;
                            break;
                        }
                    }
                }
                if (!handled) {
                    TransformVar(stmData, index, true, ref semanticInfo);
                }
            }
        }
        private static void TransformToplevelFunction(Dsl.FunctionData funcData, ref SemanticInfo semanticInfo)
        {
            if (funcData.GetId() == "=") {
                var p = funcData.GetParam(0);
                var vd = p as Dsl.ValueData;
                var fd = p as Dsl.FunctionData;
                var sd = p as Dsl.StatementData;
                if (null != vd) {
                    TransformVar(vd, ref semanticInfo);
                }
                else if (null != fd) {
                    TransformGeneralCall(fd, ref semanticInfo);
                }
                else if (null != sd) {
                    //在dsl语法里，分号分隔各语句，glsl里的函数定义结尾不加分隔符，dsl解析时会将函数定义解析到赋值语句左边的语法部分
                    //，这里需要把函数定义与赋值语句的左边部分拆分，以正确分析函数原型与识别赋值语句里的变量定义（但不能改变整体的表示
                    //，否则输出时语法可能会不正确）
                    Dsl.StatementData? left, right;
                    if (SplitToplevelStatementsInExpression(sd, out left, out right)) {
                        Debug.Assert(null != left && null != right);
                        string id = left.GetId();
                        int index = 0;
                        while (index < left.GetFunctionNum()) {
                            bool existsFunc = ParseStatement(left, ref index, out var f, out var modifiers);
                            if (existsFunc) {
                                Debug.Assert(null != f);
                                TransformFuncDef(f, modifiers);
                            }
                            else {
                                break;
                            }
                        }
                        if (index < left.GetFunctionNum()) {
                            Debug.Assert(false);
                        }
                        //拆分时left是一个复合语句结尾，到这里left应该已经处理完成了
                        TransformVar(right, 0, true, sd, ref semanticInfo);
                    }
                    else {
                        TransformVar(sd, 0, true, ref semanticInfo);
                    }
                }
                var v = funcData.GetParam(1);
                TransformSyntax(v, false, ref semanticInfo, out var addStm);
            }
            else {
                TransformGeneralFunction(funcData, ref semanticInfo);
            }
        }
        private static void TransformToplevelValue(Dsl.ValueData valData, ref SemanticInfo semanticInfo)
        {
            TransformVar(valData, ref semanticInfo);
        }

        private static void TransformStruct(Dsl.FunctionData structFunc, Dsl.ValueOrFunctionData? varNamePart)
        {
            string name = structFunc.GetId();
            var struInfo = new StructInfo();
            struInfo.Name = name;
            VarInfo? last = null;
            foreach (var p in structFunc.Params) {
                var stm = p as Dsl.StatementData;
                if (null != stm) {
                    var varInfo = ParseVarInfo(stm);
                    struInfo.Fields.Add(varInfo);
                    last = varInfo;
                }
                else if (null != last) {
                    var f = p as Dsl.FunctionData;
                    if (null != f) {
                        var varInfo = new VarInfo();
                        varInfo.CopyFrom(last);
                        varInfo.Name = f.GetId();
                        string arrTag = BuildTypeWithTypeArgs(f).Substring(varInfo.Name.Length);
                        varInfo.Type = varInfo.Type + arrTag;
                        struInfo.Fields.Add(varInfo);
                    }
                    else {
                        var varInfo = new VarInfo();
                        varInfo.CopyFrom(last);
                        varInfo.Name = p.GetId();
                        struInfo.Fields.Add(varInfo);
                    }
                }
            }
            if (null != varNamePart) {
                var vinfo = new VarInfo();
                var fd = varNamePart.AsFunction;
                if (null != fd) {
                    List<string> arrTags = new List<string>();
                    vinfo.Name = BuildTypeWithArrTags(fd, arrTags);
                    var sb = new StringBuilder();
                    sb.Append(struInfo.Name);
                    for (int ix = arrTags.Count - 1; ix >= 0; --ix) {
                        sb.Append(arrTags[ix]);
                    }
                    vinfo.Type = sb.ToString();
                }
                else {
                    vinfo.Name = varNamePart.GetId();
                    vinfo.Type = struInfo.Name;
                }
                AddVar(vinfo);
            }
            else {
                var vinfo = new VarInfo();
                vinfo.Type = struInfo.Name;
                SetLastVarType(vinfo);
            }
            if (s_StructInfos.ContainsKey(struInfo.Name)) {
                Console.WriteLine("duplicated glsl struct define '{0}', line: {1}", struInfo.Name, structFunc.GetLine());
            }
            else {
                s_StructInfos.Add(struInfo.Name, struInfo);
            }
        }
        private static void TransformBuffer(Dsl.FunctionData info, string layout, Dsl.ValueOrFunctionData? varNamePart)
        {
            var cbufInfo = new BufferInfo();
            cbufInfo.Name = info.GetId();
            cbufInfo.Layout = layout;
            if (null != varNamePart) {
                Debug.Assert(varNamePart.IsValue);
                cbufInfo.instName = varNamePart.GetId();
            }
            foreach (var p in info.Params) {
                var stm = p as Dsl.StatementData;
                if (null != stm) {
                    var varInfo = ParseVarInfo(stm);
                    cbufInfo.Variables.Add(varInfo);

                    AddVar(varInfo);
                }
            }
            if (s_BufferInfos.ContainsKey(cbufInfo.Name)) {
                Console.WriteLine("duplicated glsl buffer define '{0}', line: {1}", cbufInfo.Name, info.GetLine());
            }
            else {
                s_BufferInfos.Add(cbufInfo.Name, cbufInfo);
            }
        }
        private static void TransformFuncDef(Dsl.FunctionData func, List<string> modifiers)
        {
            var semanticInfo = new SemanticInfo();
            Debug.Assert(func.IsHighOrder);
            var paramsPart = func.LowerOrderFunction;
            var funcInfo = new FuncInfo();
            string funcName = paramsPart.GetId();
            funcInfo.Name = funcName;
            PushFuncInfo(funcInfo);
            PushBlock();

            var retInfo = new VarInfo();
            if (modifiers.Count > 0) {
                retInfo.Name = funcName;
                retInfo.Type = modifiers[modifiers.Count - 1];
                modifiers.RemoveAt(modifiers.Count - 1);
                retInfo.Modifiers = modifiers;
            }
            SetRetInfo(retInfo);

            List<string> argTypes = new List<string>();
            for (int ix = 0; ix < paramsPart.GetParamNum(); ++ix) {
                var pcomp = paramsPart.GetParam(ix);
                var pFunc = pcomp as Dsl.FunctionData;
                var pStm = pcomp as Dsl.StatementData;
                Dsl.ISyntaxComponent? pDef = null;
                if (null != pFunc && pFunc.GetId() == "=") {
                    pcomp = pFunc.GetParam(0);
                    pStm = pcomp as Dsl.StatementData;
                    pDef = pFunc.GetParam(1);
                    TransformSyntax(pDef, false, ref semanticInfo, out var addStm);
                }
                if (null != pStm) {
                    var paramInfo = ParseVarInfo(pStm);
                    paramInfo.DefaultValue = pDef;
                    AddParamInfo(paramInfo);
                    argTypes.Add(paramInfo.Type);
                }
            }
            bool first = false;
            string signature = GetFullTypeFuncSig(funcName, argTypes);
            funcInfo.Signature = signature;
            if (!s_FuncInfos.ContainsKey(signature)) {
                s_FuncInfos.Add(signature, funcInfo);
                first = true;
            }
            if (first) {
                AddFuncParamsToComputeGraph(funcInfo);

                if (!s_FuncOverloads.TryGetValue(funcName, out var overloads)) {
                    overloads = new HashSet<string>();
                    s_FuncOverloads.Add(funcName, overloads);
                }
                if (!overloads.Contains(signature)) {
                    overloads.Add(signature);
                }
            }
            for (int stmIx = 0; stmIx < func.GetParamNum(); ++stmIx) {
                Dsl.ISyntaxComponent? syntax = null;
                for (; ; ) {
                    //去掉连续分号
                    syntax = func.GetParam(stmIx);
                    if (syntax.IsValid()) {
                        break;
                    }
                    else {
                        func.Params.Remove(syntax);
                        if (stmIx < func.GetParamNum()) {
                            syntax = func.GetParam(stmIx);
                        }
                        else {
                            syntax = null;
                            break;
                        }
                    }
                }
                //处理语句
                if (stmIx < func.GetParamNum() && null != syntax && TransformSyntax(syntax, true, ref semanticInfo, out var addStm)) {
                    func.Params.Insert(stmIx + 1, addStm);
                    ++stmIx;
                }
            }
            PopBlock();
            PopFuncInfo();
        }
        private static void TransformUniformFunc(Dsl.FunctionData func, Dsl.ValueOrFunctionData? varNamePart)
        {
            if (null != varNamePart) {
                var val = varNamePart.AsValue;
                var f = varNamePart.AsFunction;
                if (null != val) {
                    var semanticInfo = new SemanticInfo();
                    TransformVar(val, ref semanticInfo);
                }
                else if (null != f) {
                    var semanticInfo = new SemanticInfo();
                    TransformVar(f.Name, ref semanticInfo);
                }
            }
            else {
                var stmData = func.GetParam(0) as Dsl.StatementData;
                if (null != stmData) {
                    var semanticInfo = new SemanticInfo();
                    TransformVar(stmData, 0, true, stmData, ref semanticInfo);
                }
            }
        }
        private static void TransformOutFunc(Dsl.FunctionData func)
        {
            if(func.GetId()== "gl_PerVertex") {
                var stmData = func.GetParam(0) as Dsl.StatementData;
                if (null != stmData) {
                    var semanticInfo = new SemanticInfo();
                    TransformVar(stmData, 0, true, stmData, ref semanticInfo);
                }
            }
        }

        //非顶层语法处理
        private static bool TransformSyntax(Dsl.ISyntaxComponent syntax, bool isStatement, ref SemanticInfo semanticInfo, out Dsl.ISyntaxComponent? addStm)
        {
            addStm = null;

            var valData = syntax as Dsl.ValueData;
            var funcData = syntax as Dsl.FunctionData;
            var stmData = syntax as Dsl.StatementData;
            if (null != valData) {
                TransformValue(valData, ref semanticInfo, out addStm);
            }
            else if (null != funcData) {
                TransformFunction(funcData, ref semanticInfo, out addStm);
            }
            else if (null != stmData) {
                TransformStatement(stmData, ref semanticInfo, out addStm);
            }
            return null != addStm;
        }
        private static bool TransformStatement(Dsl.StatementData stmData, ref SemanticInfo semanticInfo, out Dsl.ISyntaxComponent? addStm)
        {
            addStm = null;
            //DSL需要逗号分隔语句，否则多个语句会连在一起，这里进行拆分。
            int funcIndex = 0;
            while (funcIndex < stmData.GetFunctionNum()) {
                bool existsFunc = ParseStatement(stmData, ref funcIndex, out var f, out var modifiers);
                if (existsFunc) {
                    Debug.Assert(null != f);
                    TransformGeneralFunction(f, ref semanticInfo);
                }
                else {
                    break;
                }
            }
            //注：现在通过MetaDSL语义行为回调来处理拆分，理论上上面的代码都是直接break到这里（可能只有非复合语句的语句块需要上面代码处理）
            var firstValOrFunc = stmData.GetFunction(funcIndex);
            string funcId = firstValOrFunc.GetId();
            if (funcId == "return") {
                var retFunc = firstValOrFunc.AsFunction;
                if (null != retFunc) {
                    TransformGeneralFunction(retFunc, ref semanticInfo);
                }
                else {
                    for (int ix = funcIndex + 1; ix < stmData.GetFunctionNum(); ++ix) {
                        var fd = stmData.GetFunction(ix).AsFunction;
                        if (null != fd)
                            TransformGeneralFunction(fd, ref semanticInfo);
                    }
                }
            }
            else if (funcId == "if") {
                if (funcIndex == 0) {
                    if (TransformIfStatement(stmData, out var addFunc)) {
                        addStm = addFunc;
                    }
                }
                else {
                    Debug.Assert(false);
                }
            }
            else if (funcId == "switch") {
                //todo
                for (int ix = funcIndex; ix < stmData.GetFunctionNum(); ++ix) {
                    var fd = stmData.GetFunction(ix).AsFunction;
                    if (null != fd)
                        TransformGeneralFunction(fd, ref semanticInfo);
                }
            }
            else if (funcId == "while" || funcId == "for" || funcId == "do") {
                Console.WriteLine("Can't rewrite GLSL including loop !");

                for (int ix = funcIndex; ix < stmData.GetFunctionNum(); ++ix) {
                    var fd = stmData.GetFunction(ix).AsFunction;
                    if (null != fd)
                        TransformGeneralFunction(fd, ref semanticInfo);
                }
            }
            else if (funcId == "?") {
                TransformConditionStatement(stmData, funcIndex, ref semanticInfo);
            }
            else {
                TransformVar(stmData, funcIndex, false, ref semanticInfo);
            }
            return null != addStm;
        }
        private static bool TransformFunction(Dsl.FunctionData funcData, ref SemanticInfo semanticInfo, out Dsl.ISyntaxComponent? addStm)
        {
            addStm = null;
            string funcId = funcData.GetId();
            if (funcId == "=") {
                var p = funcData.GetParam(0);
                var vd = p as Dsl.ValueData;
                var fd = p as Dsl.FunctionData;
                var sd = p as Dsl.StatementData;
                var tempVarSi = new SemanticInfo();
                if (null != vd) {
                    TransformAssignLeft(vd, ref tempVarSi);
                }
                else if (null != fd) {
                    TransformGeneralCall(fd, ref tempVarSi);
                }
                else if (null != sd) {
                    //在dsl语法里，分号分隔各语句，glsl里的复合语句不加分隔符，dsl解析时会将赋值语句前面的复合语句解析到赋值语句左边
                    //的语法部分，这里需要把复合语句与赋值语句的左边部分拆分，以正确识别赋值语句里的变量定义（但不能改变整体的表示，否
                    //则输出到时语法可能会不正确）
                    //注：通过MetalDSL语义行为回调处理，可以在赋值语句解析前将复合语句拆分，这里处理无法在回调里处理的部分，精确语法应
                    //优先在回调里处理，可能只有非复合语句的语句块会走到这里
                    Dsl.StatementData? left, right;
                    if (SplitStatementsInExpression(sd, out left, out right)) {
                        Debug.Assert(null != left && null != right);
                        string id = left.GetId();
                        int funcIndex = 0;
                        while (funcIndex < left.GetFunctionNum()) {
                            bool existsFunc = ParseStatement(left, ref funcIndex, out var f, out var modifiers);
                            if (existsFunc) {
                                Debug.Assert(null != f);
                                TransformGeneralFunction(f, ref semanticInfo);
                            }
                            else {
                                break;
                            }
                        }
                        if (funcIndex < left.GetFunctionNum()) {
                            Debug.Assert(false);
                        }
                        TransformAssignLeft(right, 0, false, sd, ref tempVarSi);
                    }
                    else {
                        TransformAssignLeft(sd, 0, false, ref tempVarSi);
                    }
                }
                var tempValSi = new SemanticInfo();
                var v = funcData.GetParam(1);
                TransformSyntax(v, false, ref tempValSi, out var caddStm);
                HandleVarAliasInfoUpdate();

                var tempSi = new SemanticInfo();
                var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), tempVarSi.ResultType, "=");

                var vgn = tempVarSi.GraphNode;
                var agn = tempValSi.GraphNode;
                Debug.Assert(null != vgn);
                Debug.Assert(null != agn);

                cgcn.AddPrev(agn);
                agn.AddNext(cgcn);

                cgcn.AddNext(vgn);
                vgn.AddPrev(cgcn);

                AddComputeGraphRootNode(cgcn);

                string exp = cgcn.GetExpression();
                funcData.LastComments.Add("//  "+exp);
            }
            else if (funcId == "<-") {
                var p = funcData.GetParam(0);
                var v = funcData.GetParam(1);
                var vd = p as Dsl.ValueData;
                var fd = p as Dsl.FunctionData;
                var sd = p as Dsl.StatementData;
                if (null != sd) {
                    //在dsl语法里，分号分隔各语句，glsl里的复合语句不加分隔符，dsl解析时会将赋值语句前面的复合语句解析到return语句左边
                    //的语法部分，这里需要把复合语句与return语句的左边部分拆分，以正确处理return语句，在解析时已经把return语句改为<-表达式了,
                    //这里需要改为return函数形式
                    //注：通过MetalDSL语义行为回调处理，可以在赋值语句解析前将复合语句拆分，这里处理无法在回调里处理的部分，精确语法应
                    //优先在回调里处理，可能只有非复合语句的语句块会走到这里
                    Dsl.StatementData? left, right;
                    if (SplitStatementsInExpression(sd, out left, out right)) {
                        Debug.Assert(null != left && null != right);
                        string id = left.GetId();
                        int funcIndex = 0;
                        while (funcIndex < left.GetFunctionNum()) {
                            bool existsFunc = ParseStatement(left, ref funcIndex, out var f, out var modifiers);
                            if (existsFunc) {
                                Debug.Assert(null != f);
                                TransformGeneralFunction(f, ref semanticInfo);
                            }
                            else {
                                break;
                            }
                        }
                        if (funcIndex < left.GetFunctionNum()) {
                            Debug.Assert(false);
                        }
                    }
                    else {
                        Debug.Assert(false);
                    }
                }
                var vfd = v as Dsl.FunctionData;
                if (null != vfd && vfd.GetId() == "*") {
                    string lhs = vfd.GetParamId(0);
                    string rhs = vfd.GetParamId(1);
                }
                TransformSyntax(v, false, ref semanticInfo, out var caddStm);
                if (null != fd) {
                    funcData.LowerOrderFunction = fd;
                }
                else {
                    funcData.Name.SetId("return");
                }
                funcData.SetParamClass((int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PARENTHESIS);
                funcData.Params.Clear();
                funcData.AddParam(v);
            }
            else if (funcId == "if") {
                if (TransformIfFunction(funcData, out var addFunc)) {
                    addStm = addFunc;
                }
            }
            else {
                TransformGeneralFunction(funcData, ref semanticInfo);
            }
            return null != addStm;
        }
        private static bool TransformValue(Dsl.ValueData valData, ref SemanticInfo semanticInfo, out Dsl.ISyntaxComponent? addStm)
        {
            addStm = null;
            TransformVar(valData, ref semanticInfo);
            return null != addStm;
        }

        private static void TransformGeneralFunction(Dsl.FunctionData funcData, ref SemanticInfo semanticInfo)
        {
            if (null != funcData) {
                if (funcData.IsHighOrder) {
                    var lowerFunc = funcData.LowerOrderFunction;
                    if (lowerFunc.GetParamClassUnmasked() == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET && funcData.GetParamClassUnmasked() == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PARENTHESIS) {
                        //array init
                        TransformGeneralFunction(funcData, ref semanticInfo);
                        return;
                    }
                    if (funcData.HaveStatement()) {
                        TransformGeneralFunction(lowerFunc, ref semanticInfo);
                        PushBlock();
                        for (int stmIx = 0; stmIx < funcData.GetParamNum(); ++stmIx) {
                            var syntax = funcData.GetParam(stmIx);
                            if (TransformSyntax(syntax, true, ref semanticInfo, out var addStm)) {
                                funcData.Params.Insert(stmIx + 1, addStm);
                                ++stmIx;
                            }
                        }
                        PopBlock();
                    }
                    else {
                        TransformGeneralCall(funcData, ref semanticInfo);
                    }
                }
                else if (funcData.HaveStatement()) {
                    PushBlock();
                    for (int stmIx = 0; stmIx < funcData.GetParamNum(); ++stmIx) {
                        Dsl.ISyntaxComponent? syntax = null;
                        for (; ; ) {
                            //去掉连续分号
                            syntax = funcData.GetParam(stmIx);
                            if (syntax.IsValid()) {
                                break;
                            }
                            else {
                                funcData.Params.Remove(syntax);
                                if (stmIx < funcData.GetParamNum()) {
                                    syntax = funcData.GetParam(stmIx);
                                }
                                else {
                                    syntax = null;
                                    break;
                                }
                            }
                        }
                        //处理语句
                        if (stmIx < funcData.GetParamNum() && null != syntax && TransformSyntax(syntax, true, ref semanticInfo, out var addStm)) {
                            funcData.Params.Insert(stmIx + 1, addStm);
                            ++stmIx;
                        }
                    }
                    PopBlock();
                }
                else {
                    TransformGeneralCall(funcData, ref semanticInfo);
                }
            }
        }
        private static void TransformGeneralCall(Dsl.FunctionData call, ref SemanticInfo semanticInfo)
        {
            int paramNum = call.GetParamNum();
            int paramClass = call.GetParamClassUnmasked();
            if (!call.HaveId()) {
                switch (paramClass) {
                    case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PARENTHESIS: {
                            if(paramNum == 1) {
                                var pp = call.GetParam(0);
                                var innerCall = pp as Dsl.FunctionData;
                                if (null != innerCall) {
                                    if (!innerCall.HaveId() && innerCall.GetParamClassUnmasked() == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PARENTHESIS) {
                                        call.ClearParams();
                                        call.Params.AddRange(innerCall.Params);
                                        TransformGeneralCall(call, ref semanticInfo);
                                        return;
                                    }
                                }
                                TransformSyntax(pp, false, ref semanticInfo, out var addStm);
                            }
                            else {
                                for (int ix = 0; ix < paramNum; ++ix) {
                                    var syntax = call.GetParam(ix);
                                    if (TransformSyntax(syntax, false, ref semanticInfo, out var addStm)) {
                                        call.Params.Insert(ix + 1, addStm);
                                        ++ix;
                                    }
                                }
                            }
                        }
                        break;
                    case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET: {

                        }
                        break;
                }
                return;
            }
            if (paramClass == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PARENTHESIS) {
                string id = call.GetId();
                var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), string.Empty, id);
                for (int ix = 0; ix < paramNum; ++ix) {
                    var syntax = call.GetParam(ix);
                    var tsemanticInfo = new SemanticInfo();
                    if (TransformSyntax(syntax, false, ref tsemanticInfo, out var addStm)) {
                        call.Params.Insert(ix + 1, addStm);
                        ++ix;
                    }

                    var agn = tsemanticInfo.GraphNode;
                    Debug.Assert(null != agn);

                    cgcn.AddPrev(agn);
                    agn.AddNext(cgcn);
                }
                semanticInfo.GraphNode = cgcn;
            }
            else if (paramClass == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_OPERATOR) {
                string id = call.GetId();
                var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), string.Empty, id);
                for (int ix = 0; ix < paramNum; ++ix) {
                    var syntax = call.GetParam(ix);
                    var tsemanticInfo = new SemanticInfo();
                    if (TransformSyntax(syntax, false, ref tsemanticInfo, out var addStm)) {
                        call.Params.Insert(ix + 1, addStm);
                        ++ix;
                    }

                    var agn = tsemanticInfo.GraphNode;
                    Debug.Assert(null != agn);

                    cgcn.AddPrev(agn);
                    agn.AddNext(cgcn);
                }
                semanticInfo.GraphNode = cgcn;
            }
            else if (paramClass == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PERIOD) {
                var tsemanticInfo = new SemanticInfo();
                if (call.IsHighOrder) {
                    TransformFunction(call.LowerOrderFunction, ref tsemanticInfo, out var addStm);
                }
                else {
                    TransformValue(call.Name, ref tsemanticInfo, out var addStm);
                }
                var agn1 = tsemanticInfo.GraphNode;
                Debug.Assert(null != agn1);

                string m = call.GetParamId(0);
                var agn2 = new ComputeGraphConstNode(CurFuncInfo(), string.Empty, m);

                var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), string.Empty, ".");

                cgcn.AddPrev(agn1);
                agn1.AddNext(cgcn);

                cgcn.AddPrev(agn2);
                agn2.AddNext(cgcn);

                semanticInfo.GraphNode = cgcn;
            }
            else if (paramClass == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET) {
                var tsemanticInfo = new SemanticInfo();
                if (call.IsHighOrder) {
                    TransformFunction(call.LowerOrderFunction, ref tsemanticInfo, out var addStm);
                }
                else {
                    TransformValue(call.Name, ref tsemanticInfo, out var addStm);
                }
                var agn1 = tsemanticInfo.GraphNode;
                Debug.Assert(null != agn1);

                var tsemanticInfo2 = new SemanticInfo();
                TransformSyntax(call.GetParam(0), false, ref tsemanticInfo2, out var addStm2);
                var agn2 = tsemanticInfo2.GraphNode;
                Debug.Assert(null != agn2);

                var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), string.Empty, "[]");

                cgcn.AddPrev(agn1);
                agn1.AddNext(cgcn);

                cgcn.AddPrev(agn2);
                agn2.AddNext(cgcn);

                semanticInfo.GraphNode = cgcn;
            }
        }

        //if语句的SSA处理
        private static bool TransformIfFunction(Dsl.FunctionData ifFunc, out Dsl.FunctionData? addFunc)
        {
            var semanticInfo = new SemanticInfo();
            addFunc = null;
            if (ifFunc.IsHighOrder) {
                var lowerFunc = ifFunc.LowerOrderFunction;
                TransformGeneralFunction(lowerFunc, ref semanticInfo);
                if (ifFunc.HaveStatement()) {
                    var suffix = "_phi_" + GenUniqueNumber();
                    var oldAliasInfos = CloneVarAliasInfos(CurVarAliasInfos());
                    PushBlock();
                    for (int stmIx = 0; stmIx < ifFunc.GetParamNum(); ++stmIx) {
                        var syntax = ifFunc.GetParam(stmIx);
                        if (TransformSyntax(syntax, true, ref semanticInfo, out var addStm)) {
                            ifFunc.Params.Insert(stmIx + 1, addStm);
                            ++stmIx;
                        }
                    }
                    addFunc = new Dsl.FunctionData();
                    addFunc.Name = new Dsl.ValueData("else", Dsl.AbstractSyntaxComponent.ID_TOKEN);
                    addFunc.SetParamClass((int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_STATEMENT);
                    foreach(var vname in CurSetVars()) {
                        var assignFunc = new Dsl.FunctionData();
                        assignFunc.Name = new Dsl.ValueData("=", Dsl.AbstractSyntaxComponent.ID_TOKEN);
                        assignFunc.SetParamClass((int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_OPERATOR);
                        assignFunc.SetSeparator(Dsl.AbstractSyntaxComponent.SEPARATOR_SEMICOLON);
                        assignFunc.AddParam(new Dsl.ValueData(vname + suffix, Dsl.AbstractSyntaxComponent.ID_TOKEN));
                        if(CurVarAliasInfos().TryGetValue(vname, out var aliasInfo)) {
                            assignFunc.AddParam(new Dsl.ValueData(vname + aliasInfo.AliasSuffix, Dsl.AbstractSyntaxComponent.ID_TOKEN));
                            aliasInfo.AliasSuffix = suffix;
                        }
                        else {
                            Debug.Assert(false);
                        }
                        ifFunc.AddParam(assignFunc);

                        var assignFunc2 = new Dsl.FunctionData();
                        assignFunc2.Name = new Dsl.ValueData("=", Dsl.AbstractSyntaxComponent.ID_TOKEN);
                        assignFunc2.SetParamClass((int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_OPERATOR);
                        assignFunc2.SetSeparator(Dsl.AbstractSyntaxComponent.SEPARATOR_SEMICOLON);
                        assignFunc2.AddParam(new Dsl.ValueData(vname + suffix, Dsl.AbstractSyntaxComponent.ID_TOKEN));
                        if (oldAliasInfos.TryGetValue(vname, out var aliasInfo2)) {
                            assignFunc2.AddParam(new Dsl.ValueData(vname + aliasInfo2.AliasSuffix, Dsl.AbstractSyntaxComponent.ID_TOKEN));
                        }
                        else {
                            assignFunc2.AddParam(new Dsl.ValueData(vname, Dsl.AbstractSyntaxComponent.ID_TOKEN));
                        }
                        addFunc.AddParam(assignFunc2);
                    }
                    PopBlock();
                }
            }
            return addFunc != null;
        }
        private static bool TransformIfStatement(Dsl.StatementData ifStatement, out Dsl.FunctionData? addFunc)
        {
            addFunc = null;
            return addFunc != null;
        }
        private static void TransformForFunction(Dsl.FunctionData forFunc)
        {
            //支持循环后的可能调整
            //0、所有变量的语法引用记录到一个字典里，供后续修订别名使用
            //1、phi变量的值变为phi函数运算的结果，同时赋值放到语句块外（从而是SSA形式）
            //2、别名信息记录引入的位置（可能记录词法范围ID即可），词法范围信息上记一个入口phi变量信息，供分析中添加，在退出词法范围时实际
            //修改语法
            //3、在进入语句块时备份一份所有变量别名信息，用于确定离开语句块后的phi函数参数
            //4、在进入语句块时为块内使用的每一个变量引入一个phi变量，在实际变量使用时根据当前变量的别名引入层次添加到2中入口phi变量信息里，
            //如果是在里层循环跨多层循环使用循环外变量，则各层循环的入口phi变量信息都需要增加相应phi变量，最终使用最里层的phi变量作为新名称
            //5、在离开语句块时修订进入语句块时的phi变量对应的phi函数参数，加入赋值变量的最后别名，在离开语句块后为块内赋值过的每一个变量
            //引入一个phi变量
            //6、所有插入的phi变量赋值语句都保留语法引用与上级语法，全部处理完后，如果phi函数参数只有一个的，删除phi变量与赋值，所有对该
            //phi变量的引用修订别名为其唯一参数
        }
        private static void TransformWhileFunction(Dsl.FunctionData forFunc)
        {

        }
        private static void TransformDoWhileStatement(Dsl.StatementData forFunc)
        {

        }

        private static void TransformConditionStatement(Dsl.StatementData stmData, int index, ref SemanticInfo semanticInfo)
        {
            var tfunc = stmData.GetFunction(index).AsFunction;
            var ffunc = stmData.GetFunction(index + 1).AsFunction;
            Debug.Assert(null != tfunc && null != ffunc);
            var cond = tfunc.LowerOrderFunction.GetParam(0);
            var tval = tfunc.GetParam(0);
            var fval = ffunc.GetParam(0);

            var tsemanticInfo1 = new SemanticInfo();
            var tsemanticInfo2 = new SemanticInfo();
            var tsemanticInfo3 = new SemanticInfo();
            TransformSyntax(cond, false, ref tsemanticInfo1, out var addStm1);
            TransformSyntax(tval, false, ref tsemanticInfo2, out var addStm2);
            TransformSyntax(fval, false, ref tsemanticInfo3, out var addStm3);

            var agn1 = tsemanticInfo1.GraphNode;
            var agn2 = tsemanticInfo2.GraphNode;
            var agn3 = tsemanticInfo3.GraphNode;

            Debug.Assert(null != agn1);
            Debug.Assert(null != agn2);
            Debug.Assert(null != agn3);

            var cgcn = new ComputeGraphCalcNode(CurFuncInfo(), tsemanticInfo2.ResultType, "?:");

            cgcn.AddPrev(agn1);
            agn1.AddNext(cgcn);
            cgcn.AddPrev(agn2);
            agn2.AddNext(cgcn);
            cgcn.AddPrev(agn3);
            agn3.AddNext(cgcn);

            semanticInfo.GraphNode = cgcn;
        }

        //变量的SSA处理
        private static void TransformVar(Dsl.ValueData valData, ref SemanticInfo semanticInfo)
        {
            //变量没有出现在赋值语句左边，仍然可能是变量定义（定义相同类型的多个变量时）
            if (valData.IsId()) {
                string vid = valData.GetId();
                if (vid != "true" && vid != "false" && vid.IndexOf(' ') < 0) {
                    var vinfo = GetVarInfo(vid);
                    if (null == vinfo) {
                        var lastVarInfo = GetLastVarType();
                        if (null != lastVarInfo) {
                            var varInfo = new VarInfo();
                            varInfo.CopyFrom(lastVarInfo);
                            varInfo.Name = vid;
                            AddVar(varInfo);
                        }
                    }
                    else {
                        if (CurVarAliasInfos().TryGetValue(vid, out var info)) {
                            valData.SetId(vid + info.AliasSuffix);
                        }
                        var cgvn = FindComputeGraphVarNode(valData.GetId());
                        semanticInfo.GraphNode = cgvn;
                    }
                }
                else {
                    var cgcn = new ComputeGraphConstNode(CurFuncInfo(), string.Empty, valData.GetId());
                    semanticInfo.GraphNode = cgcn;
                }
            }
            else {
                var cgcn = new ComputeGraphConstNode(CurFuncInfo(), string.Empty, valData.GetId());
                semanticInfo.GraphNode = cgcn;
            }
        }
        private static void TransformVar(Dsl.StatementData stmData, int index, bool toplevel, ref SemanticInfo semanticInfo)
        {
            //未初始化的变量定义
            TransformVar(stmData, index, toplevel, null, ref semanticInfo);
        }
        private static void TransformVar(Dsl.StatementData stmData, int index, bool toplevel, Dsl.StatementData? oriStmData, ref SemanticInfo semanticInfo)
        {
            //未初始化的变量定义
            var lastId = stmData.Last.GetId();
            var last = stmData.Last.AsFunction;
            if (null == last || (last.GetParamClassUnmasked() == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET && !last.HaveStatement())) {
                bool isType = true;
                bool needAdjustArrTag = false;
                int funcNum = stmData.GetFunctionNum();
                for (int ix = index; ix < funcNum; ++ix) {
                    var valOrFunc = stmData.GetFunction(ix);
                    var val = valOrFunc.AsValue;
                    var func = valOrFunc.AsFunction;
                    if (null == val) {
                        if (ix < funcNum - 2) {
                            isType = false;
                            break;
                        }
                        else {
                            if (func.GetParamClassUnmasked() == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET) {
                                if (ix == funcNum - 2) {
                                    needAdjustArrTag = true;
                                }
                            }
                            else {
                                isType = false;
                                break;
                            }
                        }
                    }
                }
                if (isType) {
                    VarInfo vinfo = ParseVarInfo(stmData);
                    AddVar(vinfo);
                    if (needAdjustArrTag) {
                        var typeFunc = stmData.GetFunction(funcNum - 2).AsFunction;
                        var nameValOrFunc = stmData.GetFunction(funcNum - 1);
                        var nameVal = nameValOrFunc.AsValue;
                        var nameFunc = nameValOrFunc.AsFunction;
                        if (null != nameVal) {
                            nameFunc = new Dsl.FunctionData();
                            nameFunc.Name = nameVal;
                            stmData.SetFunction(funcNum - 1, nameFunc);
                        }
                        else {
                            Debug.Assert(null != nameFunc);
                            while (nameFunc.IsHighOrder) {
                                nameFunc = nameFunc.LowerOrderFunction;
                            }
                        }
                        Debug.Assert(null != nameFunc);
                        while (null != typeFunc) {
                            var newFunc = new Dsl.FunctionData();
                            newFunc.Name = nameFunc.Name;
                            newFunc.SetParamClass(typeFunc.GetParamClass());
                            newFunc.Params = typeFunc.Params;
                            newFunc.SetSeparator(typeFunc.GetSeparator());
                            nameFunc.LowerOrderFunction = newFunc;
                            nameFunc = newFunc;
                            if (typeFunc.IsHighOrder) {
                                typeFunc = typeFunc.LowerOrderFunction;
                            }
                            else {
                                stmData.SetFunction(funcNum - 2, typeFunc.Name);
                                typeFunc = null;
                            }
                        }
                    }
                }
            }
        }

        //变量赋值的SSA处理
        private static void TransformAssignLeft(Dsl.ValueData valData, ref SemanticInfo semanticInfo)
        {
            //变量出现在赋值语句左边，有可能是变量定义（定义相同类型的多个变量时）或赋值
            if (valData.IsId()) {
                string vid = valData.GetId();
                if (vid != "true" && vid != "false" && vid.IndexOf(' ') < 0) {
                    var vinfo = GetVarInfo(vid);
                    if (null == vinfo) {
                        var lastVarInfo = GetLastVarType();
                        if (null != lastVarInfo) {
                            var varInfo = new VarInfo();
                            varInfo.CopyFrom(lastVarInfo);
                            varInfo.Name = vid;
                            AddVar(varInfo);

                            var cgvn = FindComputeGraphVarNode(vid);
                            semanticInfo.GraphNode = cgvn;
                        }
                        else {
                            Debug.Assert(false);
                        }
                    }
                    else {
                        if(CurVarAliasInfos().TryGetValue(vid, out var info)) {
                            string nid = vid + "_" + (info.AliasIndex + 1).ToString();
                            valData.SetId(nid);
                        }
                        else {
                            valData.SetId(vid + "_0");
                        }

                        var cgvn = new ComputeGraphVarNode(CurFuncInfo(), vinfo.Type, valData.GetId());
                        AddComputeGraphVarNode(cgvn);
                        semanticInfo.GraphNode = cgvn;

                        s_VarAliasInfoUpdateQueue.Enqueue(vid);
                        var setVars = CurSetVars();
                        if (!setVars.Contains(vid)) {
                            setVars.Add(vid);
                        }
                    }
                }
                else {
                    Debug.Assert(false);
                }
            }
            else {
                Debug.Assert(false);
            }
        }
        private static void TransformAssignLeft(Dsl.StatementData stmData, int index, bool toplevel, ref SemanticInfo semanticInfo)
        {
            //赋初始值的变量定义
            TransformAssignLeft(stmData, index, toplevel, null, ref semanticInfo);
        }
        private static void TransformAssignLeft(Dsl.StatementData stmData, int index, bool toplevel, Dsl.StatementData? oriStmData, ref SemanticInfo semanticInfo)
        {
            //赋初始值的变量定义
            var lastId = stmData.Last.GetId();
            var last = stmData.Last.AsFunction;
            if (null == last || (last.GetParamClassUnmasked() == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET && !last.HaveStatement())) {
                bool isType = true;
                bool needAdjustArrTag = false;
                int funcNum = stmData.GetFunctionNum();
                for (int ix = index; ix < funcNum; ++ix) {
                    var valOrFunc = stmData.GetFunction(ix);
                    var val = valOrFunc.AsValue;
                    var func = valOrFunc.AsFunction;
                    if (null == val) {
                        if (ix < funcNum - 2) {
                            isType = false;
                            break;
                        }
                        else {
                            if (func.GetParamClassUnmasked() == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET) {
                                if (ix == funcNum - 2) {
                                    needAdjustArrTag = true;
                                }
                            }
                            else {
                                isType = false;
                                break;
                            }
                        }
                    }
                }
                if (isType) {
                    VarInfo vinfo = ParseVarInfo(stmData);
                    AddVar(vinfo);

                    var cgvn = FindComputeGraphVarNode(vinfo.Name);
                    semanticInfo.GraphNode = cgvn;

                    if (needAdjustArrTag) {
                        var typeFunc = stmData.GetFunction(funcNum - 2).AsFunction;
                        var nameValOrFunc = stmData.GetFunction(funcNum - 1);
                        var nameVal = nameValOrFunc.AsValue;
                        var nameFunc = nameValOrFunc.AsFunction;
                        if (null != nameVal) {
                            nameFunc = new Dsl.FunctionData();
                            nameFunc.Name = nameVal;
                            stmData.SetFunction(funcNum - 1, nameFunc);
                        }
                        else {
                            Debug.Assert(null != nameFunc);
                            while (nameFunc.IsHighOrder) {
                                nameFunc = nameFunc.LowerOrderFunction;
                            }
                        }
                        Debug.Assert(null != nameFunc);
                        while (null != typeFunc) {
                            var newFunc = new Dsl.FunctionData();
                            newFunc.Name = nameFunc.Name;
                            newFunc.SetParamClass(typeFunc.GetParamClass());
                            newFunc.Params = typeFunc.Params;
                            newFunc.SetSeparator(typeFunc.GetSeparator());
                            nameFunc.LowerOrderFunction = newFunc;
                            nameFunc = newFunc;
                            if (typeFunc.IsHighOrder) {
                                typeFunc = typeFunc.LowerOrderFunction;
                            }
                            else {
                                stmData.SetFunction(funcNum - 2, typeFunc.Name);
                                typeFunc = null;
                            }
                        }
                    }
                }
            }
        }
        private static void HandleVarAliasInfoUpdate()
        {
            while (s_VarAliasInfoUpdateQueue.Count > 0) {
                string vid = s_VarAliasInfoUpdateQueue.Dequeue();
                if (CurVarAliasInfos().TryGetValue(vid, out var info)) {
                    ++info.AliasIndex;
                    info.AliasSuffix = "_" + info.AliasIndex.ToString();
                }
                else {
                    info = new VarAliasInfo { AliasIndex = 0, AliasSuffix = "_0" };
                    CurVarAliasInfos().Add(vid, info);
                }
            }
        }

        //非精确语法处理，这部分可能需要持续改进
        private static bool ParseStatement(Dsl.StatementData stmData, ref int index, out Dsl.FunctionData? f, out List<string> modifiers)
        {
            bool ret = false;
            f = null;
            modifiers = new List<string>();
            for (int ix = index; ix < stmData.GetFunctionNum(); ++ix) {
                var func = stmData.GetFunction(ix);
                string id = func.GetId();
                var f_ = func.AsFunction;
                if (null != f_ && f_.HaveStatement()) {
                    f = f_;
                    ret = true;
                    index = ix + 1;
                    break;
                }
                else {
                    modifiers.Add(id);
                }
            }
            return ret;
        }
        private static bool SplitToplevelStatementsInExpression(Dsl.StatementData expParam, out Dsl.StatementData? left, out Dsl.StatementData? right)
        {
            left = null;
            right = null;
            int funcNum = expParam.GetFunctionNum();
            var lastFunc = expParam.Last.AsFunction;
            if (null != lastFunc && lastFunc.IsHighOrder) {
                //语句大括号后接圆括号表达式
                var innerFunc = lastFunc;
                while (innerFunc.IsHighOrder && innerFunc.HaveParam())
                    innerFunc = innerFunc.LowerOrderFunction;
                if (null != innerFunc && innerFunc.HaveStatement()) {
                    left = new Dsl.StatementData();
                    right = new Dsl.StatementData();
                    for (int i = 0; i < funcNum - 1; ++i) {
                        left.AddFunction(expParam.GetFunction(i));
                    }
                    left.AddFunction(innerFunc);
                    var func = new Dsl.FunctionData();
                    right.AddFunction(func);
                    var f = lastFunc;
                    while (f != innerFunc) {
                        func.Params.AddRange(f.Params);
                        f = f.LowerOrderFunction;
                        if (f != innerFunc) {
                            func.LowerOrderFunction = new Dsl.FunctionData();
                            func = func.LowerOrderFunction;
                        }
                    }
                    return true;
                }
            }
            for (int ix = funcNum - 2; ix >= 0; --ix) {
                var func = expParam.GetFunction(ix);
                string id = func.GetId();
                var f_ = func.AsFunction;
                if (null != f_) {
                    if (ix < funcNum - 1 && f_.HaveStatement()) {
                        left = new Dsl.StatementData();
                        right = new Dsl.StatementData();
                        for (int i = 0; i <= ix; ++i) {
                            left.AddFunction(expParam.GetFunction(i));
                        }
                        for (int i = ix + 1; i < funcNum; ++i) {
                            right.AddFunction(expParam.GetFunction(i));
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        private static bool SplitStatementsInExpression(Dsl.StatementData expParam, out Dsl.StatementData? left, out Dsl.StatementData? right)
        {
            left = null;
            right = null;
            int funcNum = expParam.GetFunctionNum();
            var lastFunc = expParam.Last.AsFunction;
            if (null != lastFunc && lastFunc.IsHighOrder) {
                //语句大括号后接圆括号表达式
                var innerFunc = lastFunc;
                while (innerFunc.IsHighOrder && innerFunc.HaveParam())
                    innerFunc = innerFunc.LowerOrderFunction;
                if (null != innerFunc && innerFunc.HaveStatement()) {
                    left = new Dsl.StatementData();
                    right = new Dsl.StatementData();
                    for (int i = 0; i < funcNum - 1; ++i) {
                        left.AddFunction(expParam.GetFunction(i));
                    }
                    left.AddFunction(innerFunc);
                    var func = new Dsl.FunctionData();
                    right.AddFunction(func);
                    var f = lastFunc;
                    while (f != innerFunc) {
                        func.Params.AddRange(f.Params);
                        f = f.LowerOrderFunction;
                        if (f != innerFunc) {
                            func.LowerOrderFunction = new Dsl.FunctionData();
                            func = func.LowerOrderFunction;
                        }
                    }
                    return true;
                }
            }
            for (int ix = funcNum - 2; ix >= 0; --ix) {
                var func = expParam.GetFunction(ix);
                string id = func.GetId();
                var f_ = func.AsFunction;
                if (null != f_) {
                    if (ix < funcNum - 1 && f_.HaveStatement()) {
                        left = new Dsl.StatementData();
                        right = new Dsl.StatementData();
                        for (int i = 0; i <= ix; ++i) {
                            left.AddFunction(expParam.GetFunction(i));
                        }
                        for (int i = ix + 1; i < funcNum; ++i) {
                            right.AddFunction(expParam.GetFunction(i));
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private static string TypeInference(Dsl.ISyntaxComponent syntax)
        {
            var valData = syntax as Dsl.ValueData;
            var funcData = syntax as Dsl.FunctionData;
            var stmData = syntax as Dsl.StatementData;
            if (null != valData) {
                var varInfo = GetVarInfo(valData.GetId());
                if (null != varInfo) {
                    return varInfo.Type;
                }
                else {
                    int idType = valData.GetIdType();
                    string val = valData.GetId();
                    switch (idType) {
                        case Dsl.ValueData.NUM_TOKEN:
                            if (val.Contains('.'))
                                return "float";
                            else
                                return "int";
                        default:
                            if (val == "true" || val == "false")
                                return "bool";
                            return string.Empty;
                    }
                }
            }
            else if (null != funcData) {
                string funcName = funcData.GetId();
                if (funcName == "vec2" || funcName == "vec3" || funcName == "vec4" ||
                    funcName == "ivec2" || funcName == "ivec3" || funcName == "ivec4" ||
                    funcName == "uvec2" || funcName == "uvec3" || funcName == "uvec4" ||
                    funcName == "bvec2" || funcName == "bvec3" || funcName == "bvec4") {
                    return funcName;
                }
                else if (funcName == "float" || funcName == "double" || funcName == "int" ||
                    funcName == "uint" || funcName == "bool" ||
                    funcName == "mat2" || funcName == "mat3" || funcName == "mat4" ||
                    funcName == "dmat2" || funcName == "dmat3" || funcName == "dmat4") {
                    return funcName;
                }
                else if (funcName == "texture") {
                    return "vec4";
                }
                switch (funcData.GetParamClassUnmasked()) {
                    case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_OPERATOR: {
                            int pnum = funcData.GetParamNum();
                            if (pnum == 1) {
                                string op = funcData.GetId();
                                string p1 = TypeInference(funcData.GetParam(0));
                                return OperatorTypeInference(op, p1);
                            }
                            else {
                                string op = funcData.GetId();
                                string p1 = TypeInference(funcData.GetParam(0));
                                string p2 = TypeInference(funcData.GetParam(1));
                                return OperatorTypeInference(op, p1, p2);
                            }
                        }
                    case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PERIOD: {
                            if (funcData.IsHighOrder) {
                                string objType = TypeInference(funcData.LowerOrderFunction);
                                string mname = funcData.GetParamId(0);
                                return MemberTypeInference(".", objType, string.Empty, mname);
                            }
                            else {
                                var vinfo = GetVarInfo(funcName);
                                if (null != vinfo) {
                                    string objType = vinfo.Type;
                                    string mname = funcData.GetParamId(0);
                                    return MemberTypeInference(".", objType, string.Empty, mname);
                                }
                                else {
                                    string objType = funcName;
                                    string mname = funcData.GetParamId(0);
                                    return MemberTypeInference(".", objType, string.Empty, mname);
                                }
                            }
                        }
                    case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET: {
                            if (funcData.IsHighOrder) {
                                string objType = TypeInference(funcData.LowerOrderFunction);
                                string mname = funcData.GetParamId(0);
                                return MemberTypeInference("[]", objType, string.Empty, mname);
                            }
                            else {
                                var vinfo = GetVarInfo(funcName);
                                if (null != vinfo) {
                                    string objType = vinfo.Type;
                                    string mname = funcData.GetParamId(0);
                                    return MemberTypeInference("[]", objType, string.Empty, mname);
                                }
                                else {
                                    string objType = funcName;
                                    string mname = funcData.GetParamId(0);
                                    return MemberTypeInference("[]", objType, string.Empty, mname);
                                }
                            }
                        }
                    case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PARENTHESIS: {
                            List<string> argTypes = new List<string>();
                            foreach (var p in funcData.Params) {
                                string type = TypeInference(p);
                                argTypes.Add(type);
                            }
                            if (string.IsNullOrEmpty(funcName)) {
                                return (argTypes.Count > 0 ? argTypes[argTypes.Count - 1] : string.Empty);
                            }
                            else {
                                return FunctionTypeInference(funcName, argTypes, out FuncInfo? funcInfo);
                            }
                        }
                }
            }
            else if (null != stmData) {
                if (stmData.GetId() == "?") {
                    var tfunc = stmData.First.AsFunction;
                    var ffunc = stmData.Second.AsFunction;
                    Debug.Assert(null != tfunc && null != ffunc);
                    var texp = tfunc.GetParam(0);
                    var fexp = ffunc.GetParam(0);
                    var t1 = TypeInference(texp);
                    var t2 = TypeInference(fexp);
                    if (!string.IsNullOrEmpty(t1))
                        return t1;
                    else if (!string.IsNullOrEmpty(t2))
                        return t2;
                }
            }
            return string.Empty;
        }

        private static string OperatorTypeInference(string op, string opd)
        {
            string resultType = string.Empty;
            if (op == "+" || op == "-" || op == "~")
                resultType = opd;
            else if (op == "!")
                resultType = "bool";
            return resultType;
        }
        private static string OperatorTypeInference(string op, string opd1, string opd2)
        {
            string resultType = string.Empty;
            if (op == "*") {
                if (opd1 == opd2) {
                    resultType = opd1;
                }
                else {
                    return GetMatmulType(opd1, opd2);
                }
            }
            else if (op == "+" || op == "-" || op == "/" || op == "%") {
                if (opd1.StartsWith("mat") || opd1.StartsWith("dmat"))
                    resultType = opd1;
                else if (opd2.StartsWith("mat") || opd2.StartsWith("dmat"))
                    resultType = opd2;
                else if (opd1.StartsWith("vec") || opd1.StartsWith("dvec"))
                    resultType = opd1;
                else if (opd2.StartsWith("vec") || opd2.StartsWith("dvec"))
                    resultType = opd2;
                else
                    resultType = opd1.Length >= opd2.Length ? opd1 : opd2;
            }
            else if (op == "&&" || op == "||" || op == ">=" || op == "==" || op == "!=" || op == "<=" || op == ">" || op == "<") {
                if (opd1.StartsWith("vec") || opd1.StartsWith("dvec"))
                    resultType = "bool" + GetTypeSuffix(opd1);
                else if (opd2.StartsWith("vec") || opd2.StartsWith("dvec"))
                    resultType = "bool" + GetTypeSuffix(opd2);
                else
                    resultType = "bool";
            }
            else if (op == "&" || op == "|" || op == "^" || op == "<<" || op == ">>") {
                if (opd1.StartsWith("vec") || opd1.StartsWith("dvec"))
                    resultType = "int" + GetTypeSuffix(opd1);
                else if (opd2.StartsWith("vec") || opd2.StartsWith("dvec"))
                    resultType = "int" + GetTypeSuffix(opd2);
                else
                    resultType = "int";
            }
            return resultType;
        }
        private static string FunctionTypeInference(string func, IList<string> args, out FuncInfo? funcInfo)
        {
            funcInfo = null;
            string callSig = func + "_" + string.Join("_", args);
            if (s_FuncOverloads.TryGetValue(func, out var overloads)) {
                foreach (var sig in overloads) {
                    if (sig.StartsWith(callSig)) {
                        if (s_FuncInfos.TryGetValue(sig, out var tmpInfo)) {
                            if (args.Count == tmpInfo.Params.Count && sig == callSig || args.Count < tmpInfo.Params.Count && null != tmpInfo.Params[args.Count].DefaultValue) {
                                funcInfo = tmpInfo;
                                return null == funcInfo.RetInfo ? "void" : funcInfo.RetInfo.Type;
                            }
                        }
                    }
                }
                //find nearst match
                int curScore = -1;
                foreach (var sig in overloads) {
                    if (s_FuncInfos.TryGetValue(sig, out var tmpInfo)) {
                        if (IsArgsMatch(args, tmpInfo, out int newScore) && curScore < newScore) {
                            curScore = newScore;
                            funcInfo = tmpInfo;
                        }
                    }
                }
                if (null != funcInfo) {
                    return null == funcInfo.RetInfo ? "void" : funcInfo.RetInfo.Type;
                }
            }
            else {
                //built-in function
                if (s_BuiltInFuncs.TryGetValue(func, out var resultType)) {
                    string ret = GetFuncResultType(resultType, func, args, args);
                    return ret;
                }
                else if (func.StartsWith("glsl_")) {
                    string oriName = func.Substring("glsl_".Length);
                    if (oriName.EndsWith("_ctor"))
                        oriName = oriName.Substring(0, oriName.Length - "_ctor".Length);
                    if (s_BuiltInFuncs.TryGetValue(oriName, out var resultType2)) {
                        string ret = GetFuncResultType(resultType2, oriName, args, args);
                        return ret;
                    }
                    else if (s_StructInfos.TryGetValue(oriName, out var struInfo)) {
                        string ret = oriName;
                        return ret;
                    }
                    else {
                        string ret = oriName;
                        return ret;
                    }
                }
            }
            return string.Empty;
        }
        private static string MemberTypeInference(string op, string objType, string resultType, string memberOrType)
        {
            if (op == ".") {
                if (s_StructInfos.TryGetValue(objType, out var info)) {
                    foreach (var field in info.Fields) {
                        if (field.Name == memberOrType) {
                            resultType = field.Type;
                            break;
                        }
                    }
                }
                else {
                    string baseType = GetTypeRemoveSuffix(objType);
                    string suffix = GetTypeSuffix(objType);
                    if (string.IsNullOrEmpty(resultType)) {
                        if (memberOrType.Length == 1) {
                            string stype = VectorMatrixTypeToScalarType(objType);
                            resultType = stype;
                        }
                        else if (baseType == "mat") {
                            int ct = GetMemberCount(memberOrType);
                            if (ct == 1)
                                resultType = "float";
                            else
                                resultType = "vec" + ct.ToString();
                        }
                        else if (baseType == "dmat") {
                            int ct = GetMemberCount(memberOrType);
                            if (ct == 1)
                                resultType = "double";
                            else
                                resultType = "dvec" + ct.ToString();
                        }
                        else {
                            resultType = baseType + memberOrType.Length.ToString();
                        }
                    }
                }
            }
            else if (op == "[]") {
                resultType = objType;
            }
            return resultType;
        }
        private static string GetMaxType(params string[] argTypes)
        {
            IList<string> ats = argTypes;
            return GetMaxType(ats);
        }
        private static string GetMaxType(IList<string> argTypes)
        {
            string mat = string.Empty;
            string vec = string.Empty;
            string maxTy = string.Empty;
            string ty0 = argTypes[0];
            if (ty0.StartsWith("mat") || ty0.StartsWith("dmat"))
                mat = ty0;
            else if (ty0.StartsWith("vec") || ty0.StartsWith("dvec"))
                vec = ty0;
            else
                maxTy = ty0;
            for (int i = 1; i < argTypes.Count; ++i) {
                string ty = argTypes[i];
                if (string.IsNullOrEmpty(mat) && (ty.StartsWith("mat") || ty.StartsWith("dmat")))
                    mat = ty;
                else if (string.IsNullOrEmpty(vec) && (ty.StartsWith("vec") || ty.StartsWith("dvec")))
                    vec = ty;
                else
                    maxTy = maxTy.Length >= ty.Length ? maxTy : ty;
            }
            if (!string.IsNullOrEmpty(mat))
                return mat;
            else if (!string.IsNullOrEmpty(vec))
                return vec;
            else
                return maxTy;
        }
        private static string GetMatmulType(string oriTypeA, string oriTypeB)
        {
            string ret;
            string bt1 = GetTypeRemoveSuffix(oriTypeA);
            string s1 = GetTypeSuffix(oriTypeA);
            string bt2 = GetTypeRemoveSuffix(oriTypeB);
            string s2 = GetTypeSuffix(oriTypeB);
            if (s1.Length == 0 && s2.Length == 0)
                ret = GetMaxType(oriTypeA, oriTypeB);
            else if (s1.Length == 0)
                ret = oriTypeB;
            else if (s2.Length == 0)
                ret = oriTypeA;
            else if (s1.Length == 1 && s2.Length == 0)
                ret = oriTypeA;
            else if (s1.Length == 0 && s2.Length == 1)
                ret = oriTypeB;
            else if (bt1 == "vec" && bt2 == "vec")
                return "float";
            else if (bt1 == "dvec" && bt2 == "dvec")
                return "double";
            else if (bt1 == "vec" || bt1 == "dvec")
                return oriTypeA;
            else if (bt2 == "vec" || bt2 == "dvec")
                return oriTypeB;
            else if (s1.Length == 1 && s2.Length == 1) {
                return GetMaxType(oriTypeA, oriTypeB);
            }
            else {
                string mt = GetMaxType(bt1, bt2);
                ret = mt + s1[0] + "x" + s2[2];
            }
            return ret;
        }

        private static VarInfo ParseVarInfo(Dsl.StatementData varStm)
        {
            var lastId = varStm.Last.GetId();

            var nameInfoFunc = varStm.Last.AsFunction;
            string arrTag = string.Empty;
            if (null != nameInfoFunc) {
                arrTag = BuildTypeWithTypeArgs(nameInfoFunc).Substring(lastId.Length);
            }

            VarInfo vinfo = new VarInfo();
            int funcNum = varStm.GetFunctionNum();
            for (int ix = 0; ix < funcNum - 1; ++ix) {
                var valOrFunc = varStm.GetFunction(ix);
                string id = valOrFunc.GetId();
                if (ix == funcNum - 2) {
                    var func = valOrFunc.AsFunction;
                    if (null != func)
                        vinfo.Type = BuildTypeWithTypeArgs(func) + arrTag;
                    else
                        vinfo.Type = id + arrTag;
                }
                else {
                    if (id == "inout")
                        vinfo.IsInOut = true;
                    else if (id == "out")
                        vinfo.IsOut = true;

                    vinfo.Modifiers.Add(id);
                }
            }
            vinfo.Name = lastId;
            return vinfo;
        }

        private static string BuildTypeWithTypeArgs(Dsl.FunctionData func)
        {
            var sb = new StringBuilder();
            if (func.GetParamClassUnmasked() == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET) {
                var arrTags = new List<string>();
                string baseType = BuildTypeWithArrTags(func, arrTags);
                sb.Append(baseType);
                for (int ix = arrTags.Count - 1; ix >= 0; --ix) {
                    sb.Append(arrTags[ix]);
                }
            }
            else {
                if (func.IsHighOrder) {
                    sb.Append(BuildTypeWithTypeArgs(func.LowerOrderFunction));
                }
                else {
                    sb.Append(func.GetId());
                }
                foreach (var p in func.Params) {
                    sb.Append('|');
                    sb.Append(DslToNameString(p));
                }
            }
            return sb.ToString();
        }
        private static string BuildTypeWithArrTags(Dsl.FunctionData func, List<string> arrTags)
        {
            string ret = string.Empty;
            if (func.GetParamClassUnmasked() == (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET) {
                if (func.IsHighOrder) {
                    ret = BuildTypeWithArrTags(func.LowerOrderFunction, arrTags);
                }
                else {
                    ret = func.GetId();
                }
                string arrTag = "_x";
                if (func.GetParamNum() > 0) {
                    arrTag += func.GetParamId(0);
                }
                arrTags.Add(arrTag);
            }
            else {
                ret = BuildTypeWithTypeArgs(func);
            }
            return ret;
        }
        private static string DslToNameString(Dsl.ISyntaxComponent syntax)
        {
            var valData = syntax as Dsl.ValueData;
            if (null != valData)
                return valData.GetId();
            else {
                var funcData = syntax as Dsl.FunctionData;
                if (null != funcData) {
                    var sb = new StringBuilder();
                    if (funcData.IsHighOrder) {
                        sb.Append(DslToNameString(funcData.LowerOrderFunction));
                    }
                    else {
                        sb.Append(funcData.GetId());
                    }
                    switch (funcData.GetParamClassUnmasked()) {
                        case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_PERIOD:
                            sb.Append(".");
                            break;
                        case (int)Dsl.FunctionData.ParamClassEnum.PARAM_CLASS_BRACKET:
                            sb.Append("_x");
                            break;
                        default:
                            if (funcData.GetParamNum() > 0)
                                sb.Append("_");
                            break;
                    }
                    foreach (var p in funcData.Params) {
                        sb.Append(DslToNameString(p));
                    }
                    return sb.ToString();
                }
                else {
                    var stmData = syntax as Dsl.StatementData;
                    if (null != stmData) {
                        var sb = new StringBuilder();
                        for (int ix = 0; ix < stmData.GetFunctionNum(); ++ix) {
                            if (ix > 0)
                                sb.Append("__");
                            var func = stmData.GetFunction(ix);
                            sb.Append(DslToNameString(func));
                        }
                        return sb.ToString();
                    }
                    else {
                        return string.Empty;
                    }
                }
            }
        }

        private static string GetFullTypeFuncSig(string funcName, IList<string> argTypes)
        {
            var sb = new StringBuilder();
            sb.Append(funcName);
            foreach (var t in argTypes) {
                sb.Append("_");
                sb.Append(t);
            }
            return sb.ToString();
        }
        private static string GetFuncResultType(string resultTypeTag, string funcOrObjType, IList<string> args, IList<string> oriArgs)
        {
            string ret;
            if (resultTypeTag == "@@")
                ret = funcOrObjType;
            else if (resultTypeTag == "@0-$0")
                ret = GetTypeRemoveSuffix(args[0]);
            else if (resultTypeTag == "@0")
                ret = args[0];
            else if (resultTypeTag == "@m") {
                ret = GetMaxType(oriArgs);
            }
            else if (!resultTypeTag.Contains('@') && !resultTypeTag.Contains('$'))
                ret = resultTypeTag;
            else {
                string rt = resultTypeTag.Replace("@@", funcOrObjType).Replace("@0-$0", GetTypeRemoveSuffix(args[0])).Replace("@0", args[0]).Replace("$0", GetTypeSuffix(args[0])).Replace("$R0", GetTypeSuffixReverse(args[0]));
                if (args.Count > 1) {
                    rt = rt.Replace("@1-$1", GetTypeRemoveSuffix(args[1])).Replace("@1", args[1]).Replace("$1", GetTypeSuffix(args[1])).Replace("$R1", GetTypeSuffixReverse(args[1]));
                }
                if (rt.Contains("m")) {
                    string mt = GetMaxType(oriArgs);
                    rt = rt.Replace("@m-$m", GetTypeRemoveSuffix(mt)).Replace("@m", mt).Replace("$m", GetTypeSuffix(mt)).Replace("$Rm", GetTypeSuffixReverse(mt));
                }
                ret = rt;
            }
            return ret;
        }
        private static int GetMemberCount(string member)
        {
            int ct = 0;
            foreach (char c in member) {
                if (c == '_')
                    ++ct;
            }
            return ct;
        }

        private static bool IsArgsMatch(IList<string> args, FuncInfo funcInfo, out int score)
        {
            bool ret = false;
            score = 0;
            if (args.Count <= funcInfo.Params.Count) {
                ret = true;
                for (int ix = 0; ix < args.Count; ++ix) {
                    int argScore;
                    if (!IsTypeMatch(args[ix], funcInfo.Params[ix].Type, out argScore)) {
                        ret = false;
                        break;
                    }
                    score += argScore;
                }
                if (ret && args.Count < funcInfo.Params.Count) {
                    if (null == funcInfo.Params[args.Count].DefaultValue)
                        ret = false;
                }
            }
            return ret;
        }
        private static bool IsTypeMatch(string argType, string paramType, out int score)
        {
            bool ret = false;
            score = 0;
            if (argType == paramType) {
                score = 2;
                ret = true;
            }
            else if ((argType == "bool" || argType == "int" || argType == "uint" || argType == "float" || argType == "double")
                && (paramType == "bool" || paramType == "int" || paramType == "uint" || paramType == "float" || paramType == "double")) {
                score = 1;
                ret = true;
            }
            else if ((argType == "bvec2" || argType == "ivec2" || argType == "uvec3" || argType == "vec2" || argType == "dvec2")
                && (paramType == "bvec2" || paramType == "ivec2" || paramType == "uvec2" || paramType == "vec2" || paramType == "dvec2")) {
                score = 1;
                ret = true;
            }
            else if ((argType == "bvec3" || argType == "ivec3" || argType == "uvec3" || argType == "vec3" || argType == "dvec3")
                && (paramType == "bvec3" || paramType == "ivec3" || paramType == "uvec3" || paramType == "vec3" || paramType == "dvec3")) {
                score = 1;
                ret = true;
            }
            else if ((argType == "bvec4" || argType == "ivec4" || argType == "uvec4" || argType == "vec4" || argType == "dvec4")
                && (paramType == "bvec4" || paramType == "ivec4" || paramType == "uvec4" || paramType == "vec4" || paramType == "dvec4")) {
                score = 1;
                ret = true;
            }
            else if ((argType == "mat2" || argType == "dmat2") || (paramType == "mat2" || paramType == "dmat2")) {
                score = 1;
                ret = true;
            }
            else if ((argType == "mat3" || argType == "dmat3") || (paramType == "mat3" || paramType == "dmat3")) {
                score = 1;
                ret = true;
            }
            else if ((argType == "mat4" || argType == "dmat4") || (paramType == "mat4" || paramType == "dmat4")) {
                score = 1;
                ret = true;
            }
            else if (argType == "bool" || argType == "bvec2" || argType == "bvec3" || argType == "bvec4") {
                ret = true;
            }
            else if (argType == "int" || argType == "ivec2" || argType == "ivec3" || argType == "ivec4") {
                ret = true;
            }
            else if (argType == "uint" || argType == "uvec2" || argType == "uvec3" || argType == "uvec4") {
                ret = true;
            }
            else if (argType == "float" || argType == "vec2" || argType == "vec3" || argType == "vec4") {
                ret = true;
            }
            else if (argType == "double" || argType == "dvec2" || argType == "dvec3" || argType == "dvec4") {
                ret = true;
            }
            return ret;
        }
        private static string GetTypeRemoveSuffix(string type)
        {
            type = GetTypeRemoveArrTag(type, out var arrNums);
            if (type.Length >= 3) {
                char last = type[type.Length - 1];
                if (last == '2' || last == '3' || last == '4') {
                    string last3 = type.Substring(type.Length - 3);
                    if (last3 == "2x2" || last3 == "3x3" || last3 == "4x4")
                        return type.Substring(0, type.Length - 3);
                    else if (last3 == "2x3" || last3 == "3x2" || last3 == "3x4" || last3 == "4x3" || last3 == "2x4" || last3 == "4x2")
                        return type.Substring(0, type.Length - 3);
                    else
                        return type.Substring(0, type.Length - 1);
                }
            }
            return type;
        }
        private static string GetTypeSuffix(string type)
        {
            return GetTypeSuffix(type, out var arrNums);
        }
        private static string GetTypeSuffix(string type, out IList<int> arrNums)
        {
            type = GetTypeRemoveArrTag(type, out arrNums);
            if (type.Length >= 3) {
                char last = type[type.Length - 1];
                if (last == '2' || last == '3' || last == '4') {
                    string last3 = type.Substring(type.Length - 3);
                    if (last3 == "2x2" || last3 == "3x3" || last3 == "4x4") {
                        return last3;
                    }
                    else if (last3 == "2x3" || last3 == "3x2" || last3 == "3x4" || last3 == "4x3" || last3 == "2x4" || last3 == "4x2") {
                        return last3;
                    }
                    else {
                        return last.ToString();
                    }
                }
            }
            return string.Empty;
        }
        private static string GetTypeSuffixReverse(string type)
        {
            return GetTypeSuffixReverse(type, out var arrNums);
        }
        private static string GetTypeSuffixReverse(string type, out IList<int> arrNums)
        {
            type = GetTypeRemoveArrTag(type, out arrNums);
            if (type.Length >= 3) {
                char last = type[type.Length - 1];
                if (last == '2' || last == '3' || last == '4') {
                    string last3 = type.Substring(type.Length - 3);
                    if (last3 == "2x2" || last3 == "3x3" || last3 == "4x4") {
                        return last3;
                    }
                    else if (last3 == "2x3" || last3 == "3x2" || last3 == "3x4" || last3 == "4x3" || last3 == "2x4" || last3 == "4x2") {
                        return new string(new char[] { last3[2], last3[1], last3[0] });
                    }
                    else {
                        return last.ToString();
                    }
                }
            }
            return string.Empty;
        }
        private static string GetTypeRemoveArrTag(string type, out IList<int> arrNums)
        {
            var list = new List<int>();
            var r = GetTypeRemoveArrTagRecursively(type, list);
            arrNums = list;
            return r;
        }
        private static string GetTypeRemoveArrTagRecursively(string type, List<int> arrNums)
        {
            int st = type.LastIndexOf("_x");
            if (st > 0) {
                string arrNumStr = type.Substring(st + 2);
                if (int.TryParse(arrNumStr, out int arrNum)) {
                    arrNums.Add(arrNum);
                    type = GetTypeRemoveArrTagRecursively(type.Substring(0, st), arrNums);
                }
            }
            return type;
        }
        private static string VectorMatrixTypeToScalarType(string vm)
        {
            if (vm.StartsWith("vec"))
                return "float";
            else if (vm.StartsWith("ivec"))
                return "int";
            else if (vm.StartsWith("uvec"))
                return "uint";
            else if (vm.StartsWith("dvec"))
                return "double";
            else if (vm.StartsWith("bvec"))
                return "bool";
            else if (vm.StartsWith("mat"))
                return "float";
            else if (vm.StartsWith("dmat"))
                return "double";
            else
                return GetTypeRemoveSuffix(vm);
        }

        private static Dictionary<string, VarAliasInfo> CloneVarAliasInfos(Dictionary<string, VarAliasInfo> infos)
        {
            var newInfos = new Dictionary<string, VarAliasInfo>();
            foreach(var pair in infos) {
                newInfos.Add(pair.Key, pair.Value.Clone());
            }
            return newInfos;
        }
        private static void AddFuncParamsToComputeGraph(FuncInfo funcInfo)
        {
            var graph = funcInfo.FuncComputeGraph;
            foreach (var p in funcInfo.Params) {
                var vgn = new ComputeGraphVarNode(funcInfo, p.Type, p.Name);
                vgn.IsInOut = p.IsInOut;
                vgn.IsOut = p.IsOut;
                vgn.IsParam = true;

                if (graph.VarNodes.TryGetValue(vgn.VarName, out var node)) {
                    node.Type = vgn.Type;
                    node.IsOut = vgn.IsOut;
                    node.IsInOut = vgn.IsInOut;
                }
                else {
                    graph.VarNodes.Add(vgn.VarName, vgn);
                }
            }
        }

        private static ComputeGraph CurComputeGraph()
        {
            var curFunc = CurFuncInfo();
            if (null != curFunc)
                return curFunc.FuncComputeGraph;
            else
                return s_GlobalComputeGraph;
        }
        private static void AddComputeGraphRootNode(ComputeGraphNode gn)
        {
            var cg = CurComputeGraph();
            cg.RootNodes.Add(gn);
        }
        private static void AddComputeGraphVarNode(ComputeGraphVarNode cgvn)
        {
            var cg = CurComputeGraph();
            if (cg.VarNodes.TryGetValue(cgvn.VarName, out var node)) {
                node.Type = cgvn.Type;
                node.IsOut = cgvn.IsOut;
                node.IsInOut = cgvn.IsInOut;
            }
            else {
                cg.VarNodes.Add(cgvn.VarName, cgvn);
            }
        }
        private static ComputeGraphVarNode? FindComputeGraphVarNode(string name)
        {
            ComputeGraphVarNode? ret = null;
            var cg = CurComputeGraph();
            if (cg.VarNodes.TryGetValue(name, out var node)) {
                ret = node;
            }
            else if (s_GlobalComputeGraph != cg) {
                if (s_GlobalComputeGraph.VarNodes.TryGetValue(name, out var gnode))
                    ret = gnode;
            }
            return ret;
        }

        private static void AddVar(VarInfo varInfo)
        {
            if (!s_VarInfos.TryGetValue(varInfo.Name, out var varInfos)) {
                varInfos = new Dictionary<int, VarInfo>();
                s_VarInfos.Add(varInfo.Name, varInfos);
            }
            varInfos[CurBlockId()] = varInfo;
            SetLastVarType(varInfo);

            var cgvn = new ComputeGraphVarNode(CurFuncInfo(), varInfo.Type, varInfo.Name);
            AddComputeGraphVarNode(cgvn);
        }
        private static FuncInfo? CurFuncInfo()
        {
            FuncInfo? curFuncInfo = null;
            if (s_FuncParseStack.Count > 0) {
                curFuncInfo = s_FuncParseStack.Peek();
            }
            return curFuncInfo;
        }
        private static void PushFuncInfo(FuncInfo funcInfo)
        {
            s_FuncParseStack.Push(funcInfo);
        }
        private static void PopFuncInfo()
        {
            s_FuncParseStack.Pop();
        }
        private static void AddParamInfo(VarInfo varInfo)
        {
            var funcInfo = CurFuncInfo();
            if (null != funcInfo) {
                funcInfo.Params.Add(varInfo);
                if (varInfo.IsInOut || varInfo.IsOut) {
                    funcInfo.HasInOutOrOutParams = true;
                    funcInfo.InOutOrOutParams.Add(varInfo);
                }
            }
        }
        private static void SetRetInfo(VarInfo varInfo)
        {
            var funcInfo = CurFuncInfo();
            if (null != funcInfo) {
                funcInfo.RetInfo = varInfo;
            }
        }
        private static void PushBlock()
        {
            ++s_LastBlockId;
            Dictionary<string, VarAliasInfo>? varAliasInfo = null;
            if(s_LexicalScopeStack.Count > 0) {
                var curInfo = s_LexicalScopeStack.Peek();
                Debug.Assert(null != curInfo.VarAliasInfos);
                varAliasInfo = CloneVarAliasInfos(curInfo.VarAliasInfos);
            }
            else {
                Debug.Assert(null != s_ToplevelLexicalScopeInfo.VarAliasInfos);
                varAliasInfo = CloneVarAliasInfos(s_ToplevelLexicalScopeInfo.VarAliasInfos);
            }
            s_LexicalScopeStack.Push(new LexicalScopeInfo { BlockId = s_LastBlockId, LastVarType = null, VarAliasInfos = varAliasInfo });
        }
        private static void PopBlock()
        {
            var lastInfo = s_LexicalScopeStack.Pop();
            if (s_LexicalScopeStack.Count > 0) {
                var curInfo = s_LexicalScopeStack.Peek();
                var setVars = curInfo.SetVars;
                foreach (var key in lastInfo.SetVars) {
                    if (!setVars.Contains(key)) {
                        setVars.Add(key);
                    }
                }
            }
            else {
                var setVars = s_ToplevelLexicalScopeInfo.SetVars;
                foreach (var key in lastInfo.SetVars) {
                    if (!setVars.Contains(key)) {
                        setVars.Add(key);
                    }
                }
            }
        }
        private static int CurBlockId()
        {
            if (s_LexicalScopeStack.Count > 0) {
                return s_LexicalScopeStack.Peek().BlockId;
            }
            return s_ToplevelLexicalScopeInfo.BlockId;
        }
        private static HashSet<string> CurSetVars()
        {
            if (s_LexicalScopeStack.Count > 0) {
                return s_LexicalScopeStack.Peek().SetVars;
            }
            return s_ToplevelLexicalScopeInfo.SetVars;
        }
        private static Dictionary<string, PhiVarInfo> CurPhiVarInfos()
        {
            if (s_LexicalScopeStack.Count > 0) {
                return s_LexicalScopeStack.Peek().PhiVarInfos;
            }
            return s_ToplevelLexicalScopeInfo.PhiVarInfos;
        }
        private static Dictionary<string, VarAliasInfo> CurVarAliasInfos()
        {
            if (s_LexicalScopeStack.Count > 0) {
                var infos = s_LexicalScopeStack.Peek().VarAliasInfos;
                Debug.Assert(null != infos);
                return infos;
            }
            var toplevelInfos = s_ToplevelLexicalScopeInfo.VarAliasInfos;
            Debug.Assert(null != toplevelInfos);
            return toplevelInfos;
        }
        private static void SetLastVarType(VarInfo info)
        {
            if (s_LexicalScopeStack.Count > 0) {
                s_LexicalScopeStack.Peek().LastVarType = info;
            }
            else {
                s_ToplevelLexicalScopeInfo.LastVarType = info;
            }
        }
        private static VarInfo? GetLastVarType()
        {
            if (s_LexicalScopeStack.Count > 0) {
                return s_LexicalScopeStack.Peek().LastVarType;
            }
            else {
                return s_ToplevelLexicalScopeInfo.LastVarType;
            }
        }
        private static VarInfo? GetVarInfo(string name)
        {
            VarInfo? varInfo = null;
            if (s_VarInfos.TryGetValue(name, out var varInfos)) {
                bool find = false;
                foreach (var scopeInfo in s_LexicalScopeStack) {
                    if (varInfos.TryGetValue(scopeInfo.BlockId, out varInfo)) {
                        find = true;
                        break;
                    }
                }
                if (!find) {
                    find = varInfos.TryGetValue(0, out varInfo);
                }
            }
            if (null == varInfo) {
                var curFunc = CurFuncInfo();
                if (null != curFunc) {
                    foreach (var p in curFunc.Params) {
                        if (name == p.Name) {
                            varInfo = p;
                            break;
                        }
                    }
                }
            }
            return varInfo;
        }

        internal static int GenUniqueNumber()
        {
            return ++s_UniqueNumber;
        }
        internal static string GetIndentString(int indent)
        {
            const string c_IndentString = "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t";
            return c_IndentString.Substring(0, indent);
        }

        internal sealed class VarInfo
        {
            internal string Name = string.Empty;
            internal string Type = string.Empty;
            internal bool IsInOut = false;
            internal bool IsOut = false;
            internal List<string> Modifiers = new List<string>();
            internal Dsl.ISyntaxComponent? DefaultValue = null;

            internal void CopyFrom(VarInfo other)
            {
                Name = other.Name;
                Type = other.Type;
                IsInOut = other.IsInOut;
                IsOut = other.IsOut;
                Modifiers.AddRange(other.Modifiers);
                DefaultValue = other.DefaultValue;
            }
            internal string CalcTypeString()
            {
                return (Modifiers.Count > 0 ? string.Join(' ', Modifiers) + " " : string.Empty) + Type;
            }
        }
        internal sealed class FuncInfo
        {
            internal string Name = string.Empty;
            internal string Signature = string.Empty;
            internal bool HasInOutOrOutParams = false;
            internal List<VarInfo> Params = new List<VarInfo>();
            internal VarInfo? RetInfo = null;
            internal List<VarInfo> InOutOrOutParams = new List<VarInfo>();

            internal ComputeGraph FuncComputeGraph = new ComputeGraph();

            internal bool IsVoid()
            {
                return null == RetInfo || RetInfo.Type == "void";
            }
        }
        internal sealed class StructInfo
        {
            internal string Name = string.Empty;
            internal List<VarInfo> Fields = new List<VarInfo>();
        }
        internal sealed class BufferInfo
        {
            internal string Name = string.Empty;
            internal string Layout = string.Empty;
            internal string instName = string.Empty;
            internal List<VarInfo> Variables = new List<VarInfo>();
        }
        internal sealed class ArrayInitInfo
        {
            internal string Type = string.Empty;
            internal int Size = 0;
        }
        internal enum CondExpEnum
        {
            Question = 0,
            Colon,
        }
        internal sealed class CondExpInfo
        {
            internal CondExpInfo(CondExpEnum part)
            {
                m_CondExpPart = part;
                m_ParenthesisCount = 0;
            }
            internal void IncParenthesisCount()
            {
                ++m_ParenthesisCount;
            }
            internal void DecParenthesisCount()
            {
                --m_ParenthesisCount;
            }
            internal bool MaybeCompletePart(CondExpEnum part)
            {
                return m_CondExpPart == part && m_ParenthesisCount == 0;
            }

            private CondExpEnum m_CondExpPart;
            private int m_ParenthesisCount;
        }

        internal sealed class VarSyntaxInfo
        {
            internal List<Dsl.ValueData> Vars = new List<Dsl.ValueData>();
        }
        internal sealed class VarAliasInfo
        {
            internal int AliasIndex = 0;
            internal string AliasSuffix = string.Empty;
            internal int BlockId = -1;

            internal VarAliasInfo Clone()
            {
                return new VarAliasInfo { AliasIndex = AliasIndex, AliasSuffix = AliasSuffix, BlockId = BlockId };
            }
        }
        internal sealed class PhiVarInfo
        {
            internal string PhiVarName = string.Empty;
            internal List<string> PhiArgs = new List<string>();
            internal Dsl.ISyntaxComponent? ParentStatement = null;
            internal int ParamIndex = -1;
        }
        internal sealed class LexicalScopeInfo
        {
            internal int BlockId = 0;
            internal VarInfo? LastVarType = null;
            internal HashSet<string> SetVars = new HashSet<string>();
            internal Dictionary<string, PhiVarInfo> PhiVarInfos = new Dictionary<string, PhiVarInfo>();

            internal Dictionary<string, VarAliasInfo>? VarAliasInfos = null;
        }
        internal struct SemanticInfo
        {
            internal string ResultType = string.Empty;
            internal bool IsVarValRef = false;
            internal string NameOrConst = string.Empty;

            internal ComputeGraphNode? GraphNode = null;

            public SemanticInfo()
            {
                Reset();
            }
            internal void Reset()
            {
                ResultType = string.Empty;
                IsVarValRef = false;
                NameOrConst = string.Empty;
                GraphNode = null;
            }
            internal void CopyResultFrom(SemanticInfo other)
            {
                ResultType = other.ResultType;
                IsVarValRef = other.IsVarValRef;
                NameOrConst = other.NameOrConst;

                GraphNode = other.GraphNode;
            }
        }

        private static Stack<CondExpInfo> s_CondExpStack = new Stack<CondExpInfo>();
        private static SortedDictionary<string, StructInfo> s_StructInfos = new SortedDictionary<string, StructInfo>();
        private static SortedDictionary<string, BufferInfo> s_BufferInfos = new SortedDictionary<string, BufferInfo>();
        private static SortedDictionary<string, ArrayInitInfo> s_ArrayInits = new SortedDictionary<string, ArrayInitInfo>();

        private static Queue<string> s_VarAliasInfoUpdateQueue = new Queue<string>();
        private static Dictionary<string, VarSyntaxInfo> s_VarSyntaxInfos = new Dictionary<string, VarSyntaxInfo>();
        private static List<PhiVarInfo> s_PhiVarInfos = new List<PhiVarInfo>();

        internal static ComputeGraph s_GlobalComputeGraph = new ComputeGraph();
        private static Dictionary<string, Dictionary<int, VarInfo>> s_VarInfos = new Dictionary<string, Dictionary<int, VarInfo>>();
        private static Stack<LexicalScopeInfo> s_LexicalScopeStack = new Stack<LexicalScopeInfo>();
        private static LexicalScopeInfo s_ToplevelLexicalScopeInfo = new LexicalScopeInfo { VarAliasInfos = new Dictionary<string, VarAliasInfo>() };
        private static int s_LastBlockId = 0;
        private static int s_UniqueNumber = 0;

        private static Dictionary<string, FuncInfo> s_FuncInfos = new Dictionary<string, FuncInfo>();
        private static Dictionary<string, HashSet<string>> s_FuncOverloads = new Dictionary<string, HashSet<string>>();
        private static Stack<FuncInfo> s_FuncParseStack = new Stack<FuncInfo>();

        private static char[] s_eOrE = new char[] { 'e', 'E' };

        private static Dictionary<string, string> s_BuiltInFuncs = new Dictionary<string, string> {
            { "float", "@@" },
            { "vec2", "@@" },
            { "vec3", "@@" },
            { "vec4", "@@" },
            { "double", "@@" },
            { "dvec2", "@@" },
            { "dvec3", "@@" },
            { "dvec4", "@@" },
            { "uint", "@@" },
            { "uvec2", "@@" },
            { "uvec3", "@@" },
            { "uvec4", "@@" },
            { "int", "@@" },
            { "ivec2", "@@" },
            { "ivec3", "@@" },
            { "ivec4", "@@" },
            { "bool", "@@" },
            { "bvec2", "@@" },
            { "bvec3", "@@" },
            { "bvec4", "@@" },
            { "mat2", "@@" },
            { "mat3", "@@" },
            { "mat4", "@@" },
            { "dmat2", "@@" },
            { "dmat3", "@@" },
            { "dmat4", "@@" },
            { "radians", "@0" },
            { "degrees", "@0" },
            { "sin", "@0" },
            { "cos", "@0" },
            { "tan", "@0" },
            { "asin", "@0" },
            { "acos", "@0" },
            { "atan", "@0" },
            { "sinh", "@0" },
            { "cosh", "@0" },
            { "tanh", "@0" },
            { "asinh", "@0" },
            { "acosh", "@0" },
            { "atanh", "@0" },
            { "pow", "@0" },
            { "exp", "@0" },
            { "log", "@0" },
            { "exp2", "@0" },
            { "log2", "@0" },
            { "sqrt", "@0" },
            { "inversesqrt", "@0" },
            { "abs", "@0" },
            { "sign", "@0" },
            { "floor", "@0" },
            { "trunc", "@0" },
            { "round", "@0" },
            { "roundEven", "@0" },
            { "ceil", "@0" },
            { "fract", "@0" },
            { "mod", "@0" },
            { "modf", "@0" },
            { "min", "@0" },
            { "max", "@0" },
            { "clamp", "@m" },
            { "mix", "@m" },
            { "step", "@m" },
            { "smoothstep", "@m" },
            { "isnan", "bool" },
            { "isinf", "bool" },
            { "floatBitsToInt", "int" },
            { "floatBitsToUint", "uint" },
            { "intBitsToFloat", "float" },
            { "uintBitsToFloat", "float" },
            { "fma", "float" },
            { "frexp", "float" },
            { "ldexp", "float" },
            { "packUnorm2x16", "uint" },
            { "packSnorm2x16", "uint" },
            { "packUnorm4x8", "uint" },
            { "packSnorm4x8", "uint" },
            { "unpackUnorm2x16", "vec2" },
            { "unpackSnorm2x16", "vec2" },
            { "unpackUnorm4x8", "vec4" },
            { "unpackSnorm4x8", "vec4" },
            { "packHalf2x16", "uint" },
            { "unpackHalf2x16", "vec2" },
            { "length", "float" },
            { "distance", "float" },
            { "dot", "float" },
            { "cross", "vec3" },
            { "normalize", "@0" },
            { "faceforward", "float" },
            { "reflect", "float" },
            { "refract", "float" },
            { "matrixCompMult", "@0" },
            { "outerProduct", "mat$1x$0" },
            { "transpose", "mat$R0" },
            { "determinant", "float" },
            { "inverse", "@0" },
            { "lessThan", "bvec" },
            { "lessThanEqual", "bvec" },
            { "greaterThan", "bvec" },
            { "greaterThanEqual", "bvec" },
            { "equal", "bvec" },
            { "notEqual", "bvec" },
            { "any", "bool" },
            { "all", "bool"},
            { "not", "bvec"},
            { "uaddCarry", "@0" },
            { "usubBorrow", "@0" },
            { "umulExtended", "void" },
            { "imulExtended", "void" },
            { "bitfieldExtrace", "@0" },
            { "bitfieldInsert", "@0" },
            { "bitfieldReverse", "@0" },
            { "bitCount", "@0" },
            { "findLSB", "@0" },
            { "findMSB", "@0" },
            { "textureSize", "ivec$0" },
            { "texture", "vec$0" },
            { "textureProj", "vec$0" },
            { "textureLod", "vec$0" },
            { "textureOffset", "vec$0" },
            { "texelFetch", "vec$0" },
            { "texelFetchOffset", "vec$0" },
            { "textureProjOffset", "vec$0" },
            { "textureLodOffset", "vec$0" },
            { "textureProjLod", "vec$0" },
            { "textureProjLodOffset", "vec$0" },
            { "textureGrad", "vec$0" },
            { "textureGradOffset", "vec$0" },
            { "textureProjGrad", "vec$0" },
            { "textureProjGradOffset", "vec$0" },
            { "textureGather", "vec$0" },
            { "textureGatherOffset", "vec$0" },
            { "atomicCounterIncrement", "uint" },
            { "atomicCounterDecrement", "uint" },
            { "atomicCounter", "uint" },
            { "atomicAdd", "@0" },
            { "atomicMin", "@0" },
            { "atomicMax", "@0" },
            { "atomicAnd", "@0" },
            { "atomicOr", "@0" },
            { "atomicXor", "@0" },
            { "atomicExchange", "@0" },
            { "aotmicCompSwap", "@0" },
            { "imageSize", "@0" },
            { "imageLoad", "vec4" },
            { "imageStore", "void" },
            { "imageAtomicAdd", "uint" },
            { "imageAtomicMin", "@0" },
            { "imageAtomicMax", "@0" },
            { "imageAtomicAnd", "@0" },
            { "imageAtomicOr", "@0" },
            { "imageAtomicXor", "@0" },
            { "imageAtomicExchange", "@0" },
            { "imageAtomicCompSwap", "@0" },
            { "dFdx", "@0" },
            { "dFdy", "@0" },
            { "fwidth", "@0" },
            { "interpolateAtCentroid", "@0" },
            { "interpolateAtSample", "@0" },
            { "interpolateAtOffset", "@0" },
            { "barrier", "void" },
            { "memoryBarrier", "void" },
            { "memoryBarrierAtomicCounter", "void" },
            { "memoryBarrierBuffer", "void" },
            { "memoryBarrierShared", "void" },
            { "memoryBarrierImage", "void" },
            { "groupMemoryBarrier", "void" },
            { "EmitVertex", "void" },
            { "EndPrimitive", "void" },
            { "subpassLoad", "vec4" },
        };
    }
}