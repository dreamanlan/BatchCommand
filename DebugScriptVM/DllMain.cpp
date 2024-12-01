#include <windows.h>
#include <string>
#include <vector>
#include <unordered_map>
#include <sstream>
#include <fstream>
#include "DbgScpHook.h"

static std::unordered_map<std::string, int64_t> g_Lib2Addresses{};

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved) {
    switch (fdwReason) {
    case DLL_PROCESS_ATTACH:
        break;
    case DLL_THREAD_ATTACH:
        break;
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

void TestFFI0(int a1, int a2, int a3, int a4, float f1, float f2, int64_t sv1, int64_t sv2)
{

}

static inline std::vector<std::string> string_split(const std::string& input, char delimiter, int max_fields) {
    std::istringstream input_stream(input);
    std::vector<std::string> tokens;
    std::string token;

    while (std::getline(input_stream, token, delimiter)) {
        if (token.length() > 0) {
            tokens.push_back(token);
        }
        if (tokens.size() >= max_fields) {
            break;
        }
    }

    return tokens;
}

static inline int64_t test_find_loaded_segments(int64_t pid, const std::string& so, const std::string& attr, bool incFirst, std::string& match, std::vector<std::string>& fields) {
    const char* txt =
        "72c9804000-72c9900000 r--p 00000000 fe:0c 31612032                       /vendor/lib64/hw/vulkan.adreno.so\n"
        "72c9900000-72c9ba1000 r-xp 000fc000 fe:0c 31612032                       /vendor/lib64/hw/vulkan.adreno.so\n"
        "72c9ba1000-72c9bb1000 r--p 0039d000 fe:0c 31612032                       /vendor/lib64/hw/vulkan.adreno.so\n"
        "72c9bb1000-72c9bd2000 rw-p 003ac000 fe:0c 31612032                       /vendor/lib64/hw/vulkan.adreno.so\n"
        "754670a000-754671e000 r--p 00000000 fe:09 19286849                       /system/lib64/libvulkan.so\n"
        "754671e000-7546737000 r-xp 00014000 fe:09 19286849                       /system/lib64/libvulkan.so\n"
        "7546737000-7546739000 r--p 0002d000 fe:09 19286849                       /system/lib64/libvulkan.so\n"
        "7546739000-754673a000 rw-p 0002e000 fe:09 19286849                       /system/lib64/libvulkan.so\n";
    std::istringstream input_stream(txt);

    std::string line;
    while (std::getline(input_stream, line)) {
        //72d2e1b000-72d2f17000 r--p 00000000 fe:0c 31612032                       /vendor/lib64/hw/vulkan.adreno.so
        if (line.find(so) != std::string::npos) {
            auto&& strs = string_split(line, ' ', 3);
            if (strs.size() >= 3) {
                if (strs[1] == attr) {
                    int64_t offset = std::stoll(strs[2], nullptr, 16);
                    if (incFirst || offset > 0) {
                        size_t pos = strs[0].find_first_of('-');
                        auto&& startStr = strs[0].substr(0, pos);
                        match = line;
                        fields.assign(strs.cbegin(), strs.cend());
                        return std::stoll(startStr, nullptr, 16);
                    }
                }
            }
        }
    }
    return 0;
}

static inline int64_t find_loaded_segments(int64_t pid, const std::string& so, const std::string& attr, bool incFirst, std::string& match, std::vector<std::string>& fields) {
    std::ostringstream path;
    path << "/proc/" << pid << "/maps";

    std::ifstream maps_file(path.str());
    if (!maps_file.is_open()) {
        //return 0;
        return test_find_loaded_segments(pid, so, attr, incFirst, match, fields);
    }

    std::string line;
    while (std::getline(maps_file, line)) {
        //72d2e1b000-72d2f17000 r--p 00000000 fe:0c 31612032                       /vendor/lib64/hw/vulkan.adreno.so
        if (line.find(so) != std::string::npos) {
            auto&& strs = string_split(line, ' ', 3);
            if (strs.size() >= 3) {
                if (strs[1] == attr) {
                    int64_t offset = std::stoll(strs[2], nullptr, 16);
                    if (incFirst || offset > 0) {
                        size_t pos = strs[0].find_first_of('-');
                        auto&& startStr = strs[0].substr(0, pos);
                        match = line;
                        fields.assign(strs.cbegin(), strs.cend());
                        return std::stoll(startStr, nullptr, 16);
                    }
                }
            }
        }
    }
    return 0;
}

enum class ExternApiEnum
{
    TestFFI = c_extern_api_start_id,
    LoadLib,
    GetProc,
    LoadLibAndGetProc,
    FreeLib,
    FreeLibByPath,
    GetPID,
    GetTID,
    FindSegment,
    Num
};

struct ExternApi
{
    static inline void TestFFI(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t ix = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        int64_t addr = 0;
        switch (ix) {
        case 0:
            addr = reinterpret_cast<int64_t>(TestFFI0);
            break;
        }
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, addr, stackBase, intLocals, intGlobals);
    }
    static inline void LoadLib(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        auto&& str = DebugScript::GetVarString(args[0].IsGlobal, args[0].Index, stackBase, strLocals, strGlobals);
        int64_t addr = 0;
        auto&& it = g_Lib2Addresses.find(str);
        if (it == g_Lib2Addresses.end()) {
            void* ptr = LoadLibraryA(str.c_str());
            addr = reinterpret_cast<int64_t>(ptr);
            if (ptr) {
                g_Lib2Addresses.insert(std::make_pair(str, addr));
            }
        }
        else {
            addr = it->second;
        }
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, addr, stackBase, intLocals, intGlobals);
    }
    static inline void GetProc(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t addr = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        auto&& str = DebugScript::GetVarString(args[1].IsGlobal, args[1].Index, stackBase, strLocals, strGlobals);
        auto&& ptr = GetProcAddress(reinterpret_cast<HMODULE>(addr), str.c_str());
        int64_t rval = reinterpret_cast<int64_t>(ptr);
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, rval, stackBase, intLocals, intGlobals);
    }
    static inline void LoadLibAndGetProc(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t rval = 0;
        int64_t addr = 0;
        auto&& lib = DebugScript::GetVarString(args[0].IsGlobal, args[0].Index, stackBase, strLocals, strGlobals);
        auto&& proc = DebugScript::GetVarString(args[1].IsGlobal, args[1].Index, stackBase, strLocals, strGlobals);
        auto&& it = g_Lib2Addresses.find(lib);
        if (it == g_Lib2Addresses.end()) {
            void* ptr = LoadLibraryA(lib.c_str());
            addr = reinterpret_cast<int64_t>(ptr);
            if (ptr) {
                g_Lib2Addresses.insert(std::make_pair(lib, addr));
            }
        }
        else {
            addr = it->second;
        }
        if (addr) {
            auto&& ptr = GetProcAddress(reinterpret_cast<HMODULE>(addr), proc.c_str());
            rval = reinterpret_cast<int64_t>(ptr);
        }
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, rval, stackBase, intLocals, intGlobals);
    }
    static inline void FreeLib(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t addr = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        FreeLibrary(reinterpret_cast<HMODULE>(addr));
        int64_t rval = 1;
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, rval, stackBase, intLocals, intGlobals);
    }
    static inline void FreeLibByPath(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t rval = 0;
        auto&& str = DebugScript::GetVarString(args[0].IsGlobal, args[0].Index, stackBase, strLocals, strGlobals);
        auto&& it = g_Lib2Addresses.find(str);
        if (it != g_Lib2Addresses.end()) {
            FreeLibrary(reinterpret_cast<HMODULE>(it->second));
            g_Lib2Addresses.erase(it);
            rval = 1;
        }
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, rval, stackBase, intLocals, intGlobals);
    }
    static inline void GetPID(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t rval = 0;
#ifdef WIN32
        rval = static_cast<int64_t>(GetCurrentProcessId());
#else
        rval = static_cast<int64_t>(getpid());
#endif
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, rval, stackBase, intLocals, intGlobals);
    }
    static inline void GetTID(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t rval = 0;
#ifdef WIN32
        rval = static_cast<int64_t>(GetCurrentThreadId());
#else
        //rval = static_cast<int64_t>(pthread_self());
        rval = static_cast<int64_t>(gettid());
#endif
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, rval, stackBase, intLocals, intGlobals);
    }
    static inline void FindSegment(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t pid = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        auto&& so = DebugScript::GetVarString(args[1].IsGlobal, args[1].Index, stackBase, strLocals, strGlobals);
        auto&& attr = DebugScript::GetVarString(args[2].IsGlobal, args[2].Index, stackBase, strLocals, strGlobals);
        bool incFirst = DebugScript::GetVarInt(args[3].IsGlobal, args[3].Index, stackBase, intLocals, intGlobals);
        std::string match;
        std::vector<std::string> fields;
        int64_t rval = find_loaded_segments(pid, so, attr, incFirst, match, fields);
        if (rval) {
            printf("find segment: '%s'\n", match.c_str());
            if (argNum > 4) {
                for (int ix = 0; ix < static_cast<int>(fields.size()); ++ix) {
                    DebugScript::SetVarString(args[4].IsGlobal, args[4].Index + ix, fields[ix], stackBase, strLocals, strGlobals);
                }
            }
        }
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, rval, stackBase, intLocals, intGlobals);
    }
};

void CppDbgScp_CallExternApi(int api, int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
{
    switch (static_cast<ExternApiEnum>(api)) {
    case ExternApiEnum::TestFFI:
        ExternApi::TestFFI(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::LoadLib:
        ExternApi::LoadLib(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::GetProc:
        ExternApi::GetProc(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::LoadLibAndGetProc:
        ExternApi::LoadLibAndGetProc(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::FreeLib:
        ExternApi::FreeLib(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::FreeLibByPath:
        ExternApi::FreeLibByPath(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::GetPID:
        ExternApi::GetPID(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::GetTID:
        ExternApi::GetTID(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::FindSegment:
        ExternApi::FindSegment(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    default:
        break;
    }
}

extern "C" {
    __declspec(dllexport) void CppDbgScp_ResetVM()
    {
        DebugScriptGlobal::Reset();
    }
    __declspec(dllexport) void CppDbgScp_AllocConstInt(int64_t val)
    {
        DebugScriptGlobal::AllocConstInt(val);
    }
    __declspec(dllexport) void CppDbgScp_AllocConstFloat(double val)
    {
        DebugScriptGlobal::AllocConstFloat(val);
    }
    __declspec(dllexport) void CppDbgScp_AllocConstString(const char* val)
    {
        DebugScriptGlobal::AllocConstString(val);
    }
    __declspec(dllexport) void CppDbgScp_AllocGlobalInt(int64_t val)
    {
        DebugScriptGlobal::AllocGlobalInt(val);
    }
    __declspec(dllexport) void CppDbgScp_AllocGlobalFloat(double val)
    {
        DebugScriptGlobal::AllocGlobalFloat(val);
    }
    __declspec(dllexport) void CppDbgScp_AllocGlobalString(const char* val)
    {
        DebugScriptGlobal::AllocGlobalString(val);
    }
    __declspec(dllexport) int32_t CppDbgScp_AddHook(const char* name, int32_t* enterCodes, int enterCodeNum, int32_t* exitCodes, int exitCodeNum)
    {
        return DebugScriptGlobal::AddHook(name, enterCodes, enterCodeNum, exitCodes, exitCodeNum);
    }
    __declspec(dllexport) int32_t CppDbgScp_ShareWith(int32_t hookId, const char* other)
    {
        return DebugScriptGlobal::ShareWith(hookId, other);
    }
    __declspec(dllexport) void CppDbgScp_StartVM()
    {
        DebugScriptGlobal::Start();
    }
    __declspec(dllexport) void CppDbgScp_PauseVM()
    {
        DebugScriptGlobal::Pause();
    }
    __declspec(dllexport) void CppDbgScp_ResumeVM()
    {
        DebugScriptGlobal::Resume();
    }

    __declspec(dllexport) void CppDbgScp_Load(const char* file)
    {
        DebugScriptGlobal::Reset();
        DebugScriptGlobal::Load(file);
        DebugScriptGlobal::Start();
    }

    __declspec(dllexport) int Test1(int a, double b, const char* c)
    {
        thread_local static int32_t s_hook_id = -1;
        thread_local static uint32_t s_serial_num = 0;
        CheckFuncHook(__FUNCTION__, s_hook_id, s_serial_num);
        int h_ret_val{};
        auto&& placeHolder = CreateHookWrap(s_hook_id, h_ret_val, a, b, c);
        if (placeHolder.IsBreak())
            return h_ret_val;

        return 0;

    }
    __declspec(dllexport) int Test2(int a, double b, const char* c)
    {
        auto f = [&]() {
            printf("a:%d b:%f c:%s\n", a, b, c);
            return 0;
            };
        thread_local static int32_t s_hook_id = -1;
        thread_local static uint32_t s_serial_num = 0;
        CheckFuncHook(__FUNCTION__, s_hook_id, s_serial_num);
        int h_ret_val{};
        do
        {
            auto&& placeHolder = CreateHookWrap(s_hook_id, h_ret_val, a, b, c);
            if (placeHolder.IsBreak()) {
                return h_ret_val;
            }
            else
            {
                h_ret_val = f();
            }
        } while (false);
        return h_ret_val;
    }
    __declspec(dllexport) void Test3(int a, double b, const char* c)
    {
        static int32_t s_hook_id = -1;
        static uint32_t s_serial_num = 0;
        CheckFuncHook(__FUNCTION__, s_hook_id, s_serial_num);
        auto&& placeHolder = CreateHookWrap(s_hook_id, a, b, c);
        if (placeHolder.IsBreak())
            return;
    }
    __declspec(dllexport) void Test4(int a, double b, const char* c)
    {
        auto f = [&]() {
            printf("a:%d b:%f c:%s\n", a, b, c);
            };
        static int32_t s_hook_id = -1;
        static uint32_t s_serial_num = 0;
        CheckFuncHook(__FUNCTION__, s_hook_id, s_serial_num);
        do
        {
            auto&& placeHolder = CreateHookWrap(s_hook_id, a, b, c);
            if (placeHolder.IsBreak()) {
                return;
            }
            else
            {
                f();
            }
        } while (false);
    }

    __declspec(dllexport) int TestMacro1(int a, double b, const char* c)
    {
        DBGSCP_HOOK("TestMacro1", int, a, b, c)

        return 0;
    }
    __declspec(dllexport) int TestMacro2(int a, double b, const char* c)
    {
        BEGIN_DBGSCP_HOOK()
            printf("a:%d b:%f c:%s\n", a, b, c);
            return 0;
        END_DBGSCP_HOOK("TestMacro2", int, a, b, c)
    }
    __declspec(dllexport) void TestMacro3(int a, double b, const char* c)
    {
        DBGSCP_HOOK_VOID("TestMacro3", a, b, c)
    }
    __declspec(dllexport) void TestMacro4(int a, double b, const char* c)
    {
        BEGIN_DBGSCP_HOOK_VOID()
            printf("a:%d b:%f c:%s\n", a, b, c);
        END_DBGSCP_HOOK_VOID("TestMacro4", a, b, c)
    }
}
