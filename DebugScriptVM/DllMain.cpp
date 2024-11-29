#include <windows.h>
#include <string>
#include <unordered_map>
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

enum class ExternApiEnum
{
    LoadLib = c_extern_api_start_id,
    GetProc,
    LoadLibAndGetProc,
    FreeLib,
    FreeLibByPath,
    Num
};

ExternApiRetVal CppDbgScp_CallExternApi(int api, ExternApiArg args[], int32_t argNum)
{
    ExternApiRetVal retVal{ ExternApiTypeEnum::Int, 0 };
    retVal.IntVal = 0;
    switch (static_cast<ExternApiEnum>(api)) {
    case ExternApiEnum::LoadLib: {
        if (argNum == 1 && args[0].Type == ExternApiTypeEnum::String) {
            if (args[0].StringVal) {
                auto&& pStr = args[0].StringVal;
                int64_t addr = 0;
                auto&& it = g_Lib2Addresses.find(pStr);
                if (it == g_Lib2Addresses.end()) {
                    void* ptr = LoadLibraryA(pStr);
                    addr = reinterpret_cast<int64_t>(ptr);
                    if (ptr) {
                        g_Lib2Addresses.insert(std::make_pair(pStr, addr));
                    }
                }
                else {
                    addr = it->second;
                }
                retVal.IntVal = addr;
            }
        }
    }break;
    case ExternApiEnum::GetProc: {
        if (argNum == 2 && args[0].Type == ExternApiTypeEnum::Int && args[1].Type == ExternApiTypeEnum::String) {
            if (args[0].IntVal && args[1].StringVal) {
                int64_t addr = args[0].IntVal;
                auto&& pStr = args[1].StringVal;
                auto&& ptr = GetProcAddress(reinterpret_cast<HMODULE>(addr), pStr);
                retVal.IntVal = reinterpret_cast<int64_t>(ptr);
            }
        }
    }break;
    case ExternApiEnum::LoadLibAndGetProc: {
        if (argNum == 2 && args[0].Type == ExternApiTypeEnum::String && args[1].Type == ExternApiTypeEnum::String) {
            if (args[0].StringVal && args[1].StringVal) {
                auto&& pLibStr = args[0].StringVal;
                auto&& pProcStr = args[1].StringVal;
                int64_t addr = 0;
                auto&& it = g_Lib2Addresses.find(pLibStr);
                if (it == g_Lib2Addresses.end()) {
                    void* ptr = LoadLibraryA(pLibStr);
                    addr = reinterpret_cast<int64_t>(ptr);
                    if (ptr) {
                        g_Lib2Addresses.insert(std::make_pair(pLibStr, addr));
                    }
                }
                else {
                    addr = it->second;
                }
                if (addr) {
                    auto&& ptr = GetProcAddress(reinterpret_cast<HMODULE>(addr), pProcStr);
                    retVal.IntVal = reinterpret_cast<int64_t>(ptr);
                }
            }
        }
    }break;
    case ExternApiEnum::FreeLib: {
        if (argNum == 1 && args[0].Type == ExternApiTypeEnum::Int) {
            if (args[0].IntVal) {
                int64_t addr = args[0].IntVal;
                FreeLibrary(reinterpret_cast<HMODULE>(addr));
                retVal.IntVal = 1;
            }
        }
    }break;
    case ExternApiEnum::FreeLibByPath: {
        if (argNum == 1 && args[0].Type == ExternApiTypeEnum::String) {
            if (args[0].StringVal) {
                auto&& pStr = args[0].StringVal;
                auto&& it = g_Lib2Addresses.find(pStr);
                if (it != g_Lib2Addresses.end()) {
                    FreeLibrary(reinterpret_cast<HMODULE>(it->second));
                    g_Lib2Addresses.erase(it);
                    retVal.IntVal = 1;
                }
            }
        }
    }break;
    default:
        break;
    }
    return retVal;
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
