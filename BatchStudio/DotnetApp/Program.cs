
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.IO;
using ScriptableFramework;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using System.Security.Cryptography.X509Certificates;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("[csharp] Program.Main");
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct HostApi
{
    public IntPtr OutputLog;
    public IntPtr ShowProgress;
    public IntPtr RunCommand;
    public IntPtr RunCommandTimeout;
    public IntPtr GetResultCode;
    public IntPtr GetErrorCount;
    public IntPtr GetOutputCount;
    public IntPtr GetError;
    public IntPtr GetOutput;
    public IntPtr FindInPath;
    public IntPtr GetDslScript;
    public IntPtr GetAdbExe;
    public IntPtr GetJadxExe;
    public IntPtr GetJavaExe;
    public IntPtr GetZipAlignExe;
    public IntPtr GetJavaHeap;
    public IntPtr GetSettingPath;
    public IntPtr GetSettingInt;
    public IntPtr GetSettingFloat;
    public IntPtr GetSettingDouble;
    public IntPtr GetSettingString;
    public IntPtr AddSettingItem;
    public IntPtr AddSchemeMenu;
    public IntPtr AddButton;
    public IntPtr AddInput;
    public IntPtr ClearConsole;
    public IntPtr ShowWindowsConsole;
    public IntPtr HideWindowsConsole;
}

// delegate for native api
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostOutputLogDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string msg);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void HostShowProgressDelegation(int percent, [MarshalAs(UnmanagedType.LPUTF8Str)] string msg);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostRunCommandDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string cmd, [MarshalAs(UnmanagedType.LPUTF8Str)] string args, IntPtr result);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostRunCommandTimeoutDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string cmd, [MarshalAs(UnmanagedType.LPUTF8Str)] string args, int timeout, IntPtr result);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostGetResultCodeDelegation(IntPtr result);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostGetErrorCountDelegation(IntPtr result);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostGetOutputCountDelegation(IntPtr result);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostGetErrorDelegation(int index, StringBuilder path, ref int path_size, IntPtr result);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostGetOutputDelegation(int index, StringBuilder path, ref int path_size, IntPtr result);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostFindInPathDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string filename, StringBuilder path, ref int path_size);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostGetDslScriptDelegation(StringBuilder path, ref int path_size);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostGetAdbExeDelegation(StringBuilder path, ref int path_size);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostGetJadxExeDelegation(StringBuilder path, ref int path_size);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostGetJavaExeDelegation(StringBuilder path, ref int path_size);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostGetZipAlignExeDelegation(StringBuilder path, ref int path_size);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostGetJavaHeapDelegation();
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostGetSettingPathDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string name, StringBuilder path, ref int path_size);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int HostGetSettingIntDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string name, int def_val);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate float HostGetSettingFloatDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string name, float def_val);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate double HostGetSettingDoubleDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string name, double def_val);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostGetSettingStringDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string name, StringBuilder str, ref int str_size);
//0--file 1--int 2--float 3--double 4--string
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostAddSettingItemDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string name, [MarshalAs(UnmanagedType.LPUTF8Str)] string label, [MarshalAs(UnmanagedType.LPUTF8Str)] string tooltip, int type, [MarshalAs(UnmanagedType.LPUTF8Str)] string default_value, [MarshalAs(UnmanagedType.LPUTF8Str)] string ext, [MarshalAs(UnmanagedType.LPUTF8Str)] string link);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostAddSchemeMenuDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string path, [MarshalAs(UnmanagedType.LPUTF8Str)] string tooltip);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostAddButtonDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string label, [MarshalAs(UnmanagedType.LPUTF8Str)] string tooltip, [MarshalAs(UnmanagedType.LPUTF8Str)] string cmd_type, [MarshalAs(UnmanagedType.LPUTF8Str)] string cmd_args);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostAddInputDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string label, [MarshalAs(UnmanagedType.LPUTF8Str)] string tooltip, [MarshalAs(UnmanagedType.LPUTF8Str)] string def_val, [MarshalAs(UnmanagedType.LPUTF8Str)] string cmd_type, [MarshalAs(UnmanagedType.LPUTF8Str)] string cmd_args);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostClearConsoleDelegation();
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostShowWindowsConsoleDelegation();
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool HostHideWindowsConsoleDelegation();

enum SettingType
{
    ST_FILE = 0,
    ST_INT,
    ST_FLOAT,
    ST_DOUBLE,
    ST_STRING,
    ST_NUM
};

namespace DotNetLib
{
    sealed class OutpuLogExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            string fmt = string.Empty;
            var al = new System.Collections.ArrayList();
            for (int ix = 0; ix < operands.Count; ix++) {
                BoxedValue v = operands[ix];
                if (ix == 0) {
                    fmt = v.AsString;
                }
                else {
                    al.Add(v.GetObject());
                }
            }
            string str = string.Format(fmt, al.ToArray());
            Lib.LogNoLock(str);
            return str;
        }
    }
    sealed class WriteFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            bool r = false;
            if (operands.Count >= 2) {
                string path = operands[0].GetString();
                string txt = operands[1].GetString();
                path = Path.Combine(Lib.BasePath, path);
                string? dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(path, txt);
            }
            return r;
        }
    }
    sealed class ClearScriptConsoleExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            bool r = false;
            Lib.ClearScriptConsole();
            return r;
        }
    }
    sealed class LogScriptConsoleExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            bool r = false;
            Lib.LogScriptConsole();
            return r;
        }
    }

    internal sealed class BuildDbgScpExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            string scpFile = "hook.txt";
            string struFile = "struct.txt";
            string apiFile = "api.txt";
            string datFile = "bytecode.dat";
            if (operands.Count > 0) {
                string path = operands[0].AsString;
                scpFile = Path.Combine(path, scpFile);
                struFile = Path.Combine(path, struFile);
                apiFile = Path.Combine(path, apiFile);
                datFile = Path.Combine(path, datFile);
            }
            if (operands.Count > 1) {
                scpFile = operands[1].AsString;
            }
            if (operands.Count > 2) {
                struFile = operands[2].AsString;
            }
            if (operands.Count > 3) {
                apiFile = operands[3].AsString;
            }
            if (operands.Count > 4) {
                datFile = operands[4].AsString;
            }
            if (!string.IsNullOrEmpty(apiFile) && !string.IsNullOrEmpty(struFile) && !string.IsNullOrEmpty(scpFile) && !string.IsNullOrEmpty(datFile)) {
                string txt = File.ReadAllText(apiFile);
                string err = CppDebugScript.DebugScriptCompiler.Instance.LoadApiDefine(txt);
                if (string.IsNullOrEmpty(err)) {
                    Lib.LogNoLock(string.Format("Load API from {0} finished.", apiFile));
                }
                else {
                    Lib.LogNoLock(string.Format("Load API from {0} failed:{1}", apiFile, err));
                }

                txt = File.ReadAllText(struFile);
                err = CppDebugScript.DebugScriptCompiler.Instance.LoadStructDefine(txt);
                if (string.IsNullOrEmpty(err)) {
                    Lib.LogNoLock(string.Format("Load Struct from {0} finished.", struFile));
                }
                else {
                    Lib.LogNoLock(string.Format("Load Struct from {0} failed:{1}", struFile, err));
                }

                txt = File.ReadAllText(scpFile);
                if (CppDebugScript.DebugScriptCompiler.Instance.Compile(txt, out err)) {
                    Lib.LogNoLock(string.Format("Compile {0} finished.", scpFile));
                    var sb = new StringBuilder();
                    try {
                        CppDebugScript.DebugScriptCompiler.Instance.DumpAsm(sb);
                    }
                    catch (Exception e) {
                        sb.AppendLine();
                        sb.AppendLine();
                        sb.AppendLine("========[Exception]========");
                        sb.AppendLine(e.ToString());
                    }
                    Lib.LogNoLock(string.Format("[ASM]:\n{0}", sb.ToString()));
                }
                else {
                    Lib.LogNoLock(string.Format("Compile DebugScript from {0} failed:{1}", scpFile, err));
                }

                CppDebugScript.DebugScriptCompiler.Instance.SaveByteCode(datFile);
                Lib.LogNoLock(string.Format("Save to {0} finished.", datFile));

                CppDebugScript.CppDbgScpInterface.CppDbgScp_Load(datFile);

                Lib.LogNoLock("--------test--------");
                int a = CppDebugScript.CppDbgScpInterface.Test1Export(123, 123.456, "test1");
                Lib.LogNoLock(string.Format("retval a:{0}", a));
                int b = CppDebugScript.CppDbgScpInterface.Test2Export(1234, 1234.456, "test2");
                Lib.LogNoLock(string.Format("retval b:{0}", b));
                CppDebugScript.CppDbgScpInterface.Test3Export(12345, 12345.456, "test3");
                CppDebugScript.CppDbgScpInterface.Test4Export(123456, 123456.456, "test4");

                Lib.LogNoLock("--------------------");

                int ma = CppDebugScript.CppDbgScpInterface.TestMacro1Export(123, 123.456, "testmacro1");
                Lib.LogNoLock(string.Format("retval ma:{0}", ma));
                int mb = CppDebugScript.CppDbgScpInterface.TestMacro2Export(1234, 1234.456, "testmacro2");
                Lib.LogNoLock(string.Format("retval mb:{0}", mb));
                CppDebugScript.CppDbgScpInterface.TestMacro3Export(12345, 12345.456, "testmacro3");
                CppDebugScript.CppDbgScpInterface.TestMacro4Export(123456, 123456.456, "testmacro4");

                Lib.LogNoLock("--------------------");
            }
            return BoxedValue.NullObject;
        }
    }
    public class NativeApi
    {
        public NativeApi(IntPtr apis)
        {
            HostApi hostApi = Marshal.PtrToStructure<HostApi>(apis);
            m_OutputLogApi = Marshal.GetDelegateForFunctionPointer<HostOutputLogDelegation>(hostApi.OutputLog);
            m_ShowProgressApi = Marshal.GetDelegateForFunctionPointer<HostShowProgressDelegation>(hostApi.ShowProgress);
            m_RunCommandApi = Marshal.GetDelegateForFunctionPointer<HostRunCommandDelegation>(hostApi.RunCommand);
            m_RunCommandTimeoutApi = Marshal.GetDelegateForFunctionPointer<HostRunCommandTimeoutDelegation>(hostApi.RunCommandTimeout);
            m_GetResultCodeApi = Marshal.GetDelegateForFunctionPointer<HostGetResultCodeDelegation>(hostApi.GetResultCode);
            m_GetErrorCountApi = Marshal.GetDelegateForFunctionPointer<HostGetErrorCountDelegation>(hostApi.GetErrorCount);
            m_GetOutputCountApi = Marshal.GetDelegateForFunctionPointer<HostGetOutputCountDelegation>(hostApi.GetOutputCount);
            m_GetErrorApi = Marshal.GetDelegateForFunctionPointer<HostGetErrorDelegation>(hostApi.GetError);
            m_GetOutputApi = Marshal.GetDelegateForFunctionPointer<HostGetOutputDelegation>(hostApi.GetOutput);
            m_FindInPathApi = Marshal.GetDelegateForFunctionPointer<HostFindInPathDelegation>(hostApi.FindInPath);
            m_GetDslScriptApi = Marshal.GetDelegateForFunctionPointer<HostGetDslScriptDelegation>(hostApi.GetDslScript);
            m_GetAdbExeApi = Marshal.GetDelegateForFunctionPointer<HostGetAdbExeDelegation>(hostApi.GetAdbExe);
            m_GetJadxExeApi = Marshal.GetDelegateForFunctionPointer<HostGetJadxExeDelegation>(hostApi.GetJadxExe);
            m_GetJavaExeApi = Marshal.GetDelegateForFunctionPointer<HostGetJavaExeDelegation>(hostApi.GetJavaExe);
            m_GetZipAlignExeApi = Marshal.GetDelegateForFunctionPointer<HostGetZipAlignExeDelegation>(hostApi.GetZipAlignExe);
            m_GetJavaHeapApi = Marshal.GetDelegateForFunctionPointer<HostGetJavaHeapDelegation>(hostApi.GetJavaHeap);
            m_GetSettingPathApi = Marshal.GetDelegateForFunctionPointer<HostGetSettingPathDelegation>(hostApi.GetSettingPath);
            m_GetSettingIntApi = Marshal.GetDelegateForFunctionPointer<HostGetSettingIntDelegation>(hostApi.GetSettingInt);
            m_GetSettingFloatApi = Marshal.GetDelegateForFunctionPointer<HostGetSettingFloatDelegation>(hostApi.GetSettingFloat);
            m_GetSettingDoubleApi = Marshal.GetDelegateForFunctionPointer<HostGetSettingDoubleDelegation>(hostApi.GetSettingDouble);
            m_GetSettingStringApi = Marshal.GetDelegateForFunctionPointer<HostGetSettingStringDelegation>(hostApi.GetSettingString);
            m_AddSettingItemApi = Marshal.GetDelegateForFunctionPointer<HostAddSettingItemDelegation>(hostApi.AddSettingItem);
            m_AddSchemeMenuApi = Marshal.GetDelegateForFunctionPointer<HostAddSchemeMenuDelegation>(hostApi.AddSchemeMenu);
            m_AddButtonApi = Marshal.GetDelegateForFunctionPointer<HostAddButtonDelegation>(hostApi.AddButton);
            m_AddInputApi = Marshal.GetDelegateForFunctionPointer<HostAddInputDelegation>(hostApi.AddInput);
            m_ClearConsoleApi = Marshal.GetDelegateForFunctionPointer<HostClearConsoleDelegation>(hostApi.ClearConsole);
            m_ShowWindowsConsoleApi = Marshal.GetDelegateForFunctionPointer<HostShowWindowsConsoleDelegation>(hostApi.ShowWindowsConsole);
            m_HideWindowsConsoleApi = Marshal.GetDelegateForFunctionPointer<HostHideWindowsConsoleDelegation>(hostApi.HideWindowsConsole);
        }

        public IntPtr Worker { get => m_Worker; set => m_Worker = value; }
        public IntPtr Result { get => m_Result; set => m_Result = value; }

        public void OutputLog(string msg)
        {
            if (m_OutputLogApi == null) {
                return;
            }
            m_OutputLogApi.Invoke(msg);
        }
        public void ShowProgress(int percent, string msg)
        {
            if (m_ShowProgressApi == null) {
                return;
            }
            m_ShowProgressApi.Invoke(percent, msg);
        }
        public bool RunCommand(string cmd, string args)
        {
            if (m_RunCommandApi == null) {
                return false;
            }
            return m_RunCommandApi.Invoke(cmd, args, m_Result);
        }
        public bool RunCommandTimeout(string cmd, string args, int timeout)
        {
            if (m_RunCommandTimeoutApi == null) {
                return false;
            }
            return m_RunCommandTimeoutApi.Invoke(cmd, args, timeout, m_Result);
        }
        public int GetResultCode()
        {
            if (m_GetResultCodeApi == null) {
                return 0;
            }
            return m_GetResultCodeApi.Invoke(m_Result);
        }
        public int GetErrorCount()
        {
            if (m_GetErrorCountApi == null) {
                return 0;
            }
            return m_GetErrorCountApi.Invoke(m_Result);
        }
        public int GetOutputCount()
        {
            if (m_GetOutputCountApi == null) {
                return 0;
            }
            return m_GetOutputCountApi.Invoke(m_Result);
        }
        public string GetError(int index)
        {
            if (m_GetErrorApi == null) {
                return string.Empty;
            }
            var sb = new StringBuilder(c_max_info_length + 1);
            int len = sb.Capacity;
            if (m_GetErrorApi.Invoke(index, sb, ref len, m_Result)) {
                return sb.ToString();
            }
            return string.Empty;
        }
        public string GetOutput(int index)
        {
            if (m_GetOutputApi == null) {
                return string.Empty;
            }
            var sb = new StringBuilder(c_max_info_length + 1);
            int len = sb.Capacity;
            if (m_GetOutputApi.Invoke(index, sb, ref len, m_Result)) {
                return sb.ToString();
            }
            return string.Empty;
        }
        public string FindInPath(string fileName)
        {
            if (m_FindInPathApi == null) {
                return string.Empty;
            }
            var sb = new StringBuilder(c_max_path_length + 1);
            int len = sb.Capacity;
            if (m_FindInPathApi.Invoke(fileName, sb, ref len)) {
                return sb.ToString();
            }
            return string.Empty;
        }
        public string GetDslScript()
        {
            if (m_GetDslScriptApi == null) {
                return string.Empty;
            }
            var sb = new StringBuilder(c_max_path_length + 1);
            int len = sb.Capacity;
            if (m_GetDslScriptApi.Invoke(sb, ref len)) {
                return sb.ToString();
            }
            return string.Empty;
        }
        public string GetAdbExe()
        {
            if (m_GetAdbExeApi == null) {
                return string.Empty;
            }
            var sb = new StringBuilder(c_max_path_length + 1);
            int len = sb.Capacity;
            if (m_GetAdbExeApi.Invoke(sb, ref len)) {
                return sb.ToString();
            }
            return string.Empty;
        }
        public string GetJadxExe()
        {
            if (m_GetJadxExeApi == null) {
                return string.Empty;
            }
            var sb = new StringBuilder(c_max_path_length + 1);
            int len = sb.Capacity;
            if (m_GetJadxExeApi.Invoke(sb, ref len)) {
                return sb.ToString();
            }
            return string.Empty;
        }
        public string GetJavaExe()
        {
            if (m_GetJavaExeApi == null) {
                return string.Empty;
            }
            var sb = new StringBuilder(c_max_path_length + 1);
            int len = sb.Capacity;
            if (m_GetJavaExeApi.Invoke(sb, ref len)) {
                return sb.ToString();
            }
            return string.Empty;
        }
        public string GetZipAlignExe()
        {
            if (m_GetZipAlignExeApi == null) {
                return string.Empty;
            }
            var sb = new StringBuilder(c_max_path_length + 1);
            int len = sb.Capacity;
            if (m_GetZipAlignExeApi.Invoke(sb, ref len)) {
                return sb.ToString();
            }
            return string.Empty;
        }
        public int GetJavaHeap()
        {
            if (m_GetJavaHeapApi == null) {
                return 256;
            }
            return m_GetJavaHeapApi.Invoke();
        }
        public string GetSettingPath(string name, string def_val)
        {
            if (m_GetSettingPathApi == null) {
                return string.Empty;
            }
            var sb = new StringBuilder(c_max_path_length + 1);
            if (!string.IsNullOrEmpty(def_val)) {
                sb.Append(def_val);
            }
            int len = sb.Capacity;
            if (m_GetSettingPathApi.Invoke(name, sb, ref len)) {
                return sb.ToString();
            }
            return string.Empty;
        }
        public int GetSettingInt(string name, int def_val)
        {
            if (m_GetSettingIntApi == null) {
                return 0;
            }
            return m_GetSettingIntApi.Invoke(name, def_val);
        }
        public float GetSettingFloat(string name, float def_val)
        {
            if (m_GetSettingFloatApi == null) {
                return 0.0f;
            }
            return m_GetSettingFloatApi.Invoke(name, def_val);
        }
        public double GetSettingDouble(string name, double def_val)
        {
            if (m_GetSettingDoubleApi == null) {
                return 0.0;
            }
            return m_GetSettingDoubleApi.Invoke(name, def_val);
        }
        public string GetSettingString(string name, string def_val)
        {
            if (m_GetSettingStringApi == null) {
                return string.Empty;
            }
            var sb = new StringBuilder(c_max_info_length + 1);
            if (!string.IsNullOrEmpty(def_val)) {
                sb.Append(def_val);
            }
            int len = sb.Capacity;
            if (m_GetSettingStringApi.Invoke(name, sb, ref len)) {
                return sb.ToString();
            }
            return string.Empty;
        }
        public bool AddSettingItem(string name, string label, string tooltip, int type, string default_value, string ext, string link)
        {
            if (m_AddSettingItemApi == null) {
                return false;
            }
            return m_AddSettingItemApi.Invoke(name, label, tooltip, type, default_value, ext, link);
        }
        public bool AddSchemeMenu(string path, string tooltip)
        {
            if (m_AddSchemeMenuApi == null) {
                return false;
            }
            return m_AddSchemeMenuApi.Invoke(path, tooltip);
        }
        public bool AddButton(string label, string tooltip, string cmd_type, string cmd_args)
        {
            if (m_AddButtonApi == null) {
                return false;
            }
            return m_AddButtonApi.Invoke(label, tooltip, cmd_type, cmd_args);
        }
        public bool AddInput(string label, string tooltip, string def_val, string cmd_type, string cmd_args)
        {
            if (m_AddInputApi == null) {
                return false;
            }
            return m_AddInputApi.Invoke(label, tooltip, def_val, cmd_type, cmd_args);
        }
        public bool ClearConsole()
        {
            if (m_ClearConsoleApi == null) {
                return false;
            }
            return m_ClearConsoleApi.Invoke();
        }
        public bool ShowWindowsConsole()
        {
            if (m_ShowWindowsConsoleApi == null) {
                return false;
            }
            return m_ShowWindowsConsoleApi.Invoke();
        }
        public bool HideWindowsConsole()
        {
            if (m_HideWindowsConsoleApi == null) {
                return false;
            }
            return m_HideWindowsConsoleApi.Invoke();
        }
        public void LoadSchemeMenu()
        {
            m_Schemes.Clear();
            string path = Path.Combine(Lib.BasePath, "../schemes");
            var files = Directory.GetFiles(path, "*.dsl", SearchOption.TopDirectoryOnly);
            foreach (var file in files) {
                var dslFile = new Dsl.DslFile();
                dslFile.Load(file, msg => { Lib.LogNoLock(msg); });
                foreach (var func in dslFile.DslInfos) {
                    if (func is Dsl.FunctionData fd && fd.GetId() == "scheme" && fd.IsHighOrder) {
                        var call = fd.LowerOrderFunction;
                        m_Schemes.Add(call.GetParamId(0), fd);
                    }
                }
            }
            foreach (var pair in m_Schemes) {
                string key = pair.Key;
                string tooltip = string.Empty;
                var call = pair.Value.LowerOrderFunction;
                if (call.GetParamNum() > 1) {
                    tooltip = call.GetParamId(1);
                }
                AddSchemeMenu(key, tooltip);
            }
        }
        public void LoadScheme(string path)
        {
            if (m_Schemes.TryGetValue(path, out var fd)) {
                foreach (var p in fd.Params) {
                    if(p is Dsl.FunctionData func) {
                        string fid = func.GetId();
                        if (fid == "add_button") {
                            AddButton(func.GetParamId(0), func.GetParamId(1), func.GetParamId(2), func.GetParamId(3));
                        }
                        else if (fid == "add_input") {
                            AddInput(func.GetParamId(0), func.GetParamId(1), func.GetParamId(2), func.GetParamId(3), func.GetParamId(4));
                        }
                    }
                }
            }
        }

        private SortedDictionary<string, Dsl.FunctionData> m_Schemes = new SortedDictionary<string, Dsl.FunctionData>();

        private HostOutputLogDelegation? m_OutputLogApi;
        private HostShowProgressDelegation? m_ShowProgressApi;
        private HostRunCommandDelegation? m_RunCommandApi;
        private HostRunCommandTimeoutDelegation? m_RunCommandTimeoutApi;
        private HostGetResultCodeDelegation? m_GetResultCodeApi;
        private HostGetErrorCountDelegation? m_GetErrorCountApi;
        private HostGetOutputCountDelegation? m_GetOutputCountApi;
        private HostGetErrorDelegation? m_GetErrorApi;
        private HostGetOutputDelegation? m_GetOutputApi;
        private HostFindInPathDelegation? m_FindInPathApi;
        private HostGetDslScriptDelegation? m_GetDslScriptApi;
        private HostGetAdbExeDelegation? m_GetAdbExeApi;
        private HostGetJadxExeDelegation? m_GetJadxExeApi;
        private HostGetJavaExeDelegation? m_GetJavaExeApi;
        private HostGetZipAlignExeDelegation? m_GetZipAlignExeApi;
        private HostGetJavaHeapDelegation? m_GetJavaHeapApi;
        private HostGetSettingPathDelegation? m_GetSettingPathApi;
        private HostGetSettingIntDelegation? m_GetSettingIntApi;
        private HostGetSettingFloatDelegation? m_GetSettingFloatApi;
        private HostGetSettingDoubleDelegation? m_GetSettingDoubleApi;
        private HostGetSettingStringDelegation? m_GetSettingStringApi;
        private HostAddSettingItemDelegation? m_AddSettingItemApi;
        private HostAddSchemeMenuDelegation? m_AddSchemeMenuApi;
        private HostAddButtonDelegation? m_AddButtonApi;
        private HostAddInputDelegation? m_AddInputApi;
        private HostClearConsoleDelegation? m_ClearConsoleApi;
        private HostShowWindowsConsoleDelegation? m_ShowWindowsConsoleApi;
        private HostHideWindowsConsoleDelegation? m_HideWindowsConsoleApi;

        private IntPtr m_Worker = IntPtr.Zero;
        private IntPtr m_Result = IntPtr.Zero;

        private const int c_max_path_length = 1024;
        private const int c_max_info_length = 4096;
    }
    public static class Lib
    {
        [UnmanagedCallersOnly]
        public static int RegisterApi(IntPtr apis)
        {
            s_NativeApi = new NativeApi(apis);

            return 0;
        }

        public delegate void InitDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string path, IntPtr result);
        public delegate void ExecuteDslDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string dsl, [MarshalAs(UnmanagedType.LPUTF8Str)] string selInTree, [MarshalAs(UnmanagedType.LPUTF8Str)] string selInList, IntPtr result);
        public delegate int LoadSettingDelegation(IntPtr result);
        public delegate int LoadSchemeMenuDelegation(IntPtr result);
        public delegate int LoadSchemeDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string path, IntPtr result);
        public delegate int ExecuteCommandDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string cmdType, [MarshalAs(UnmanagedType.LPUTF8Str)] string cmdArgs, [MarshalAs(UnmanagedType.LPUTF8Str)] string selInTree, [MarshalAs(UnmanagedType.LPUTF8Str)] string selInList, IntPtr worker, IntPtr result);
        public delegate int BuildDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string selInTree, [MarshalAs(UnmanagedType.LPUTF8Str)] string selInList, IntPtr worker, IntPtr result);
        public delegate int InstallDelegation([MarshalAs(UnmanagedType.LPUTF8Str)] string selInTree, [MarshalAs(UnmanagedType.LPUTF8Str)] string selInList, IntPtr worker, IntPtr result);

        public static void Init(string path, IntPtr result)
        {
            s_MainThreadId = Thread.CurrentThread.ManagedThreadId;

            LogNoLock("[csharp] Init BasePath: " + path);
            s_BasePath = path;
            Console.SetOut(s_StringWriter);
            Console.SetError(s_StringWriter);

            try {
                LogNoLock(string.Format("[csharp] Call dsl init"));

                if (null != s_NativeApi) {
                    s_NativeApi.Worker = IntPtr.Zero;
                    s_NativeApi.Result = result;

                    TryLoadDSL();
                    BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                    BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                    BoxedValue r = BatchCommand.BatchScript.Call("init");
                    if (!r.IsNullObject) {
                        LogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                    }
                }
            }
            catch (Exception e) {
                LogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
            }
        }

        public static void ExecuteDsl(string dsl, string selInTree, string selInList, IntPtr result)
        {
            lock (s_Lock) {
                try {
                    LogNoLock(string.Format("[csharp] Call csharp ExecuteDsl, dsl:{0} selInTree:{1} selInList:{2}", dsl, selInTree, selInList));

                    if (null != s_NativeApi) {
                        s_NativeApi.Worker = IntPtr.Zero;
                        s_NativeApi.Result = result;

                        TryLoadDSL();
                        BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                        BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                        BatchCommand.BatchScript.SetGlobalVariable("selInTree", BoxedValue.FromString(selInTree));
                        BatchCommand.BatchScript.SetGlobalVariable("selInList", BoxedValue.FromString(selInList));
                        var r = BatchCommand.BatchScript.EvalAndRun(dsl);
                        if (!r.IsNullObject) {
                            LogNoLock(string.Format("[csharp] result:{0}", r.ToString()));
                        }
                    }
                }
                catch (Exception e) {
                    LogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
                }
            }
        }
        // -1 -- failed 0 -- nothing was done 1 -- finished
        public static int LoadSetting(IntPtr result)
        {
            lock (s_Lock) {
                try {
                    LogNoLock(string.Format("[csharp] Call dsl loadsetting"));

                    if (null != s_NativeApi) {
                        s_NativeApi.Worker = IntPtr.Zero;
                        s_NativeApi.Result = result;

                        TryLoadDSL();
                        BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                        BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                        BoxedValue r = BatchCommand.BatchScript.Call("loadsetting");
                        if (r.IsInteger) {
                            return r.GetInt();
                        }
                    }
                }
                catch (Exception e) {
                    LogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
                }
                return 0;
            }
        }
        public static int LoadSchemeMenu(IntPtr result)
        {
            lock (s_Lock) {
                try {
                    LogNoLock(string.Format("[csharp] Call dsl loadschememenu"));

                    if (null != s_NativeApi) {
                        s_NativeApi.Worker = IntPtr.Zero;
                        s_NativeApi.Result = result;

                        TryLoadDSL();
                        BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                        BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                        BoxedValue r = BatchCommand.BatchScript.Call("loadschememenu");
                        if (r.IsInteger) {
                            return r.GetInt();
                        }
                    }
                }
                catch (Exception e) {
                    LogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
                }
                return 0;
            }
        }
        public static int LoadScheme(string path, IntPtr result)
        {
            lock (s_Lock) {
                try {
                    LogNoLock(string.Format("[csharp] Call dsl loadscheme, path:{0}", path));

                    if (null != s_NativeApi) {
                        s_NativeApi.Worker = IntPtr.Zero;
                        s_NativeApi.Result = result;

                        TryLoadDSL();
                        BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                        BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                        var args = BatchCommand.BatchScript.NewCalculatorValueList();
                        args.Add(BoxedValue.FromString(path));
                        BoxedValue r = BatchCommand.BatchScript.Call("loadscheme", args);
                        BatchCommand.BatchScript.RecycleCalculatorValueList(args);
                        if (r.IsInteger) {
                            return r.GetInt();
                        }
                    }
                }
                catch (Exception e) {
                    LogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
                }
                return 0;
            }
        }
        public static int ExecuteCommand(string cmdType, string cmdArgs, string selInTree, string selInList, IntPtr worker, IntPtr result)
        {
            lock (s_Lock) {
                try {
                    LogNoLock(string.Format("[csharp] Call dsl executecommand, cmd type:{0} cmd args:{1} selInTree:{2} selInList:{3}", cmdType, cmdArgs, selInTree, selInList));

                    if (null != s_NativeApi) {
                        s_NativeApi.Worker = worker;
                        s_NativeApi.Result = result;

                        TryLoadDSL();
                        BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                        BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                        BatchCommand.BatchScript.SetGlobalVariable("selInTree", BoxedValue.FromString(selInTree));
                        BatchCommand.BatchScript.SetGlobalVariable("selInList", BoxedValue.FromString(selInList));
                        var args = BatchCommand.BatchScript.NewCalculatorValueList();
                        args.Add(BoxedValue.FromString(cmdType));
                        args.Add(BoxedValue.FromString(cmdArgs));
                        BoxedValue r = BatchCommand.BatchScript.Call("executecommand", args);
                        BatchCommand.BatchScript.RecycleCalculatorValueList(args);
                        if (r.IsInteger) {
                            return r.GetInt();
                        }
                    }
                }
                catch (Exception e) {
                    LogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
                }
                return 0;
            }
        }
        public static int Build(string selInTree, string selInList, IntPtr worker, IntPtr result)
        {
            lock (s_Lock) {
                try {
                    LogNoLock(string.Format("[csharp] Call dsl build, selInTree:{0} selInList:{1}", selInTree, selInList));

                    if (null != s_NativeApi) {
                        s_NativeApi.Worker = worker;
                        s_NativeApi.Result = result;

                        TryLoadDSL();
                        BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                        BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                        BatchCommand.BatchScript.SetGlobalVariable("selInTree", BoxedValue.FromString(selInTree));
                        BatchCommand.BatchScript.SetGlobalVariable("selInList", BoxedValue.FromString(selInList));
                        BoxedValue r = BatchCommand.BatchScript.Call("build");
                        if (r.IsInteger) {
                            return r.GetInt();
                        }
                    }
                }
                catch (Exception e) {
                    LogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
                }
                return 0;
            }
        }
        public static int Install(string selInTree, string selInList, IntPtr worker, IntPtr result)
        {
            lock (s_Lock) {
                try {
                    LogNoLock(string.Format("[csharp] Call dsl install, selInTree:{0} selInList:{1}", selInTree, selInList));

                    if (null != s_NativeApi) {
                        s_NativeApi.Worker = worker;
                        s_NativeApi.Result = result;

                        TryLoadDSL();
                        BatchCommand.BatchScript.SetGlobalVariable("nativeapi", BoxedValue.FromObject(s_NativeApi));
                        BatchCommand.BatchScript.SetGlobalVariable("basepath", BoxedValue.FromString(s_BasePath));
                        BatchCommand.BatchScript.SetGlobalVariable("selInTree", BoxedValue.FromString(selInTree));
                        BatchCommand.BatchScript.SetGlobalVariable("selInList", BoxedValue.FromString(selInList));
                        BoxedValue r = BatchCommand.BatchScript.Call("install");
                        if (r.IsInteger) {
                            return r.GetInt();
                        }
                    }
                }
                catch (Exception e) {
                    LogNoLock("[csharp] Exception:" + e.Message + "\n" + e.StackTrace);
                }
                return 0;
            }
        }

        public static string BasePath
        {
            get {
                return s_BasePath;
            }
        }
        public static void LogNoLock(string msg)
        {
            if (null != s_NativeApi) {
                bool isMainThread = Thread.CurrentThread.ManagedThreadId == s_MainThreadId;
                string txt = string.Format("thread:{0} {1}{2}: {3}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name, isMainThread ? "(main)" : string.Empty, msg);
                //Console.WriteLine(txt);
                var lines = txt.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines) {
                    s_NativeApi.OutputLog(line);
                }
            }
        }
        public static void ClearScriptConsole()
        {
            s_StringBuilder.Clear();
        }
        public static void LogScriptConsole()
        {
            LogNoLock(s_StringBuilder.ToString());
        }
        private static void TryLoadDSL()
        {
            PrepareBatchScript();
            string? path = s_NativeApi?.GetDslScript();
            if (string.IsNullOrEmpty(path)) {
                path = Path.Combine(s_BasePath, "../managed/Script.dsl");
            }
            else if (!Path.IsPathRooted(path)) {
                path = Path.Combine(s_BasePath, path);
            }
            var fi = new FileInfo(path);
            if (fi.Exists) {
                if (fi.LastWriteTime != s_DslScriptTime || s_DslScriptPath != path) {
                    s_DslScriptTime = fi.LastWriteTime;
                    s_DslScriptPath = path;
                    BatchCommand.BatchScript.Load(fi.FullName);

                    LogNoLock("[csharp] Load dsl script: " + fi.FullName);
                }
            }
            else {
                LogNoLock("[csharp] Can't find dsl script: " + fi.FullName);
            }
        }
        private static void RegisterBatchScriptApi()
        {
            BatchCommand.BatchScript.Register("outputlog", "outputlog(fmt, ...)", new ExpressionFactoryHelper<OutpuLogExp>());
            BatchCommand.BatchScript.Register("writefile", "writefile(path, txt)", new ExpressionFactoryHelper<WriteFileExp>());
            BatchCommand.BatchScript.Register("clrscpcon", "clrscpcon()", new ExpressionFactoryHelper<ClearScriptConsoleExp>());
            BatchCommand.BatchScript.Register("logscpcon", "logscpcon()", new ExpressionFactoryHelper<LogScriptConsoleExp>());
            BatchCommand.BatchScript.Register("builddbgscp", "builddbgscp(path,scpFile,struFile,apiFile,datFile) api", new ExpressionFactoryHelper<BuildDbgScpExp>());
        }
        private static void PrepareBatchScript()
        {
            if (!s_BatchScriptInited) {
                BatchCommand.BatchScript.Init();
                RegisterBatchScriptApi();
                s_BatchScriptInited = true;
            }
        }

        private static bool s_BatchScriptInited = false;
        private static string s_BasePath = string.Empty;
        private static string s_DslScriptPath = string.Empty;
        private static DateTime s_DslScriptTime = DateTime.Now;
        private static int s_MainThreadId = 0;
        private static object s_Lock = new object();

        private static StringBuilder s_StringBuilder = new StringBuilder();
        private static StringWriter s_StringWriter = new StringWriter(s_StringBuilder);
        private static NativeApi? s_NativeApi;
    }
}