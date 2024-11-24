#pragma once
#include <stdint.h>
#include <string>

class DebugScriptGlobal
{
public:
    static void Reset();
    static void AllocConstInt(int64_t val);
    static void AllocConstFloat(double val);
    static void AllocConstString(const char* val);
    static void AllocGlobalInt(int64_t val);
    static void AllocGlobalFloat(double val);
    static void AllocGlobalString(const char* val);
    static int32_t AddHook(const char* name, int32_t* onEnter, int32_t onEnterSize, int32_t* onExit, int32_t onExitSize);
    static int32_t ShareWith(int32_t hookId, const char* other);
    static void Start();
    static void Pause();
    static void Resume();
    static void Load(const char* file);
};

class DebugScriptVM
{
public:
    static int32_t FindHook(const char* name);
    static void Reset();
    static bool CanRun();
    static bool RunHookOnEnter(int32_t id, int32_t argc, int64_t argv[]);
    static bool RunHookOnExit(int32_t id, int32_t argc, int64_t argv[]);
};

extern uint32_t g_DebugScriptSerialNum;
extern bool g_DebugScriptStarted;

static const int c_extern_api_start_id = 100;
enum class ExternApiTypeEnum
{
    NotUse = 0,
    Int,
    Float,
    String
};
struct ExternApiArg
{
    ExternApiTypeEnum Type;
    union
    {
        int64_t IntVal;
        double FloatVal;
        const char* StringVal;
    };
};
struct ExternApiRetVal
{
    ExternApiTypeEnum Type;
    union
    {
        int64_t IntVal;
        double FloatVal;
    };
    std::string StringVal{};
};

extern ExternApiRetVal CppDbgScp_CallExternApi(int api, ExternApiArg args[], int32_t argNum);
