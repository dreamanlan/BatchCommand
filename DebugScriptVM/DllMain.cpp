#include <windows.h>
#include "DbgScpHook.h"

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
    FirstApi = c_extern_api_start_id,
    Num
};

ExternApiRetVal CppDbgScp_CallExternApi(int api, ExternApiArg args[], int32_t argNum)
{
    ExternApiRetVal retVal{ ExternApiTypeEnum::Int, 0 };
    retVal.IntVal = 0;
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
        using HookWrapType = HookWrap<decltype(Test1)>;
        thread_local static int32_t s_hook_id = -1;
        thread_local static uint32_t s_serial_num = 0;
        CheckFuncHook(__FUNCTION__, s_hook_id, s_serial_num);
        typename HookWrapType::RetType h_ret_val{};
        HookWrapType placeHolder(s_hook_id, h_ret_val, a, b, c);
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
        using HookWrapType = HookWrap<decltype(Test2)>;
        thread_local static int32_t s_hook_id = -1;
        thread_local static uint32_t s_serial_num = 0;
        CheckFuncHook(__FUNCTION__, s_hook_id, s_serial_num);
        typename HookWrapType::RetType h_ret_val{};
        do
        {
            HookWrapType placeHolder(s_hook_id, h_ret_val, a, b, c);
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
        using HookWrapType = HookWrap<decltype(Test3)>;
        static int32_t s_hook_id = -1;
        static uint32_t s_serial_num = 0;
        CheckFuncHook(__FUNCTION__, s_hook_id, s_serial_num);
        HookWrapType placeHolder(s_hook_id, a, b, c);
        if (placeHolder.IsBreak())
            return;
    }
    __declspec(dllexport) void Test4(int a, double b, const char* c)
    {
        auto f = [&]() {
            printf("a:%d b:%f c:%s\n", a, b, c);
            };
        using HookWrapType = HookWrap<decltype(Test4)>;
        static int32_t s_hook_id = -1;
        static uint32_t s_serial_num = 0;
        CheckFuncHook(__FUNCTION__, s_hook_id, s_serial_num);
        do
        {
            HookWrapType placeHolder(s_hook_id, a, b, c);
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
        DBGSCP_HOOK("TestMacro1", TestMacro1, a, b, c)

        return 0;
    }
    __declspec(dllexport) int TestMacro2(int a, double b, const char* c)
    {
        BEGIN_DBGSCP_HOOK()
            printf("a:%d b:%f c:%s\n", a, b, c);
            return 0;
        END_DBGSCP_HOOK("TestMacro2", TestMacro2, a, b, c)
    }
    __declspec(dllexport) void TestMacro3(int a, double b, const char* c)
    {
        DBGSCP_HOOK_VOID("TestMacro3", TestMacro3, a, b, c)
    }
    __declspec(dllexport) void TestMacro4(int a, double b, const char* c)
    {
        BEGIN_DBGSCP_HOOK_VOID()
            printf("a:%d b:%f c:%s\n", a, b, c);
        END_DBGSCP_HOOK_VOID("TestMacro4", TestMacro4, a, b, c)
    }
}
