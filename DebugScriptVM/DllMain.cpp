#include <windows.h>
#include <string>
#include <vector>
#include <unordered_map>
#include <sstream>
#include <fstream>
#include "DbgScpHook.h"

#if PLATFORM_WIN
#include "windows.h"
#elif PLATFORM_ANDROID
#include <sys/types.h>
#include <unistd.h>
#include <sys/mman.h>
#elif UNITY_POSIX
#include <pthread.h>
#endif

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

void DbgScp_TestVoid(int a, double b, const char* c)
{
    BEGIN_DBGSCP_HOOK_VOID()

        printf("DbgScp_TestVoid a:%d b:%f c:%s\n", a, b, c);

    END_DBGSCP_HOOK_VOID("DbgScp_TestVoid", a, b, c)
}
int DbgScp_TestInt(int a, double b, const char* c)
{
    BEGIN_DBGSCP_HOOK()

        printf("DbgScp_TestInt a:%d b:%f c:%s\n", a, b, c);
        return 0;

    END_DBGSCP_HOOK("DbgScp_TestInt", int, a, b, c)
}

int TestFFI0(int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, float f1, float f2, int64_t sv1, int64_t sv2)
{
    printf("%d %d %d %d %d %d %d %d %f %f %lld %lld\n", a1, a2, a3, a4, a5, a6, a7, a8, f1, f2, sv1, sv2);
    return 0;
}
int64_t TestFFI1(int64_t a1, int64_t a2, int64_t a3, int64_t a4, int64_t a5, int64_t a6, int64_t a7, int64_t a8, double f1, double f2, int64_t sv1, int64_t sv2)
{
    printf("%lld %lld %lld %lld %lld %lld %lld %lld %f %f %lld %lld\n", a1, a2, a3, a4, a5, a6, a7, a8, f1, f2, sv1, sv2);
    return 1;
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

static inline size_t GetPagetSize()
{
#if PLATFORM_WIN
    SYSTEM_INFO sysInfo;
    GetSystemInfo(&sysInfo);
    return static_cast<size_t>(sysInfo.dwPageSize);
#elif UNITY_POSIX
    return static_cast<size_t>(getpagesize());
#else
    return 4096;
#endif
}
static inline int64_t AlignToPageSize(int64_t addr, size_t pageSize)
{
    return static_cast<int64_t>(static_cast<size_t>(addr) & ~(pageSize - 1));
}
static inline size_t RoundToPageSize(size_t size, size_t pageSize)
{
    return (size + pageSize - 1) & ~(pageSize - 1);
}
static inline void SetMemoryProtect(int64_t addr, size_t size, size_t pageSize, int32_t quickflag, int32_t rawflag)
{
    addr = AlignToPageSize(addr, pageSize);
    size = RoundToPageSize(size, pageSize);
    /*
#define PAGE_NOACCESS           0x01
#define PAGE_READONLY           0x02
#define PAGE_READWRITE          0x04
#define PAGE_WRITECOPY          0x08
#define PAGE_EXECUTE            0x10
#define PAGE_EXECUTE_READ       0x20
#define PAGE_EXECUTE_READWRITE  0x40
#define PAGE_EXECUTE_WRITECOPY  0x80
#define PAGE_GUARD             0x100
#define PAGE_NOCACHE           0x200
#define PAGE_WRITECOMBINE      0x400
    */
    /*
#define PROT_READ 0x1
#define PROT_WRITE 0x2
#define PROT_EXEC 0x4
#define PROT_SEM 0x8
#define PROT_NONE 0x0
#define PROT_GROWSDOWN 0x01000000
#define PROT_GROWSUP 0x02000000
    */
#if PLATFORM_WIN || _WIN32 || _WIN64
    DWORD flag = rawflag;
    if (quickflag >= 0) {
        switch (quickflag) {
        case 0:
            flag = PAGE_NOACCESS;
            break;
        case 1:
            flag = PAGE_READONLY;
            break;
        case 2:
            flag = PAGE_READWRITE;
            break;
        case 3:
            flag = PAGE_EXECUTE_READWRITE;
            break;
        }
    }
    DWORD oldProtect;
    VirtualProtect(reinterpret_cast<void*>(addr), size, flag, &oldProtect);
#elif UNITY_POSIX
    int flag = rawflag;
    if (quickflag >= 0) {
        switch (quickflag) {
        case 0:
            flag = PROT_NONE;
            break;
        case 1:
            flag = PROT_READ;
            break;
        case 2:
            flag = PROT_READ | PROT_WRITE;
            break;
        case 3:
            flag = PROT_READ | PROT_WRITE | PROT_EXEC;
            break;
        }
    }
    mprotect(reinterpret_cast<char*>(addr), size, flag);
#endif
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
    MemoryProtect,
    GetDeviceModel,
    GetGpu,
    GetGpuVer,
    CheckMemory,
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
        case 1:
            addr = reinterpret_cast<int64_t>(TestFFI1);
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
#ifdef PLATFORM_WIN || WIN32
        rval = static_cast<int64_t>(GetCurrentProcessId());
#elif UNITY_POSIX
        rval = static_cast<int64_t>(getpid());
#endif
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, rval, stackBase, intLocals, intGlobals);
    }
    static inline void GetTID(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t rval = 0;
#ifdef PLATFORM_WIN || WIN32
        rval = static_cast<int64_t>(GetCurrentThreadId());
#elif PLATFORM_ANDROID
        return static_cast<int64_t>(gettid());
#elif UNITY_POSIX
        return static_cast<int64_t>(pthread_self());
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
    static inline void MemoryProtect(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t addr = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        size_t size = static_cast<size_t>(DebugScript::GetVarInt(args[1].IsGlobal, args[1].Index, stackBase, intLocals, intGlobals));
        int quickflag = static_cast<int>(DebugScript::GetVarInt(args[2].IsGlobal, args[2].Index, stackBase, intLocals, intGlobals));
        int rawflag = 0;
        if (argNum > 3) {
            rawflag = static_cast<int>(DebugScript::GetVarInt(args[3].IsGlobal, args[3].Index, stackBase, intLocals, intGlobals));
        }
        size_t pageSize = GetPagetSize();
        SetMemoryProtect(addr, size, pageSize, quickflag, rawflag);
    }
    static inline void GetDeviceModel(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        const char* name = "[device_model]";
        DebugScript::SetVarString(retVal.IsGlobal, retVal.Index, name, stackBase, strLocals, strGlobals);
    }
    static inline void GetGpu(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        const char* name = "[gpu]";
        DebugScript::SetVarString(retVal.IsGlobal, retVal.Index, name, stackBase, strLocals, strGlobals);
    }
    static inline void GetGpuVer(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        const char* name = "[gpu_ver]";
        DebugScript::SetVarString(retVal.IsGlobal, retVal.Index, name, stackBase, strLocals, strGlobals);
    }
    static inline void CheckMemory(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        //unity memory check
#if ENABLE_MEM_PROFILER
        int64_t memId = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        auto&& id = static_cast<MemLabelIdentifier>(memId);
        ValidateAllocatorIntegrity(MemLabelId(id));
#endif
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
    case ExternApiEnum::MemoryProtect:
        ExternApi::MemoryProtect(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::GetDeviceModel:
        ExternApi::GetDeviceModel(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::GetGpu:
        ExternApi::GetGpu(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::GetGpuVer:
        ExternApi::GetGpuVer(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::CheckMemory:
        ExternApi::CheckMemory(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
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
        bool retry{};
        int h_ret_val{};
        do
        {
            auto&& placeHolder = CreateHookWrap(retry, s_hook_id, h_ret_val, a, b, c);
            if (placeHolder.IsBreak()) {
                return h_ret_val;
            }
            else
            {
                h_ret_val = f();
            }
        } while (false);
        if (retry) {
            h_ret_val = f();
        }
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
        bool retry{};
        do
        {
            auto&& placeHolder = CreateHookWrap(retry, s_hook_id, a, b, c);
            if (placeHolder.IsBreak()) {
                return;
            }
            else
            {
                f();
            }
        } while (false);
        if (retry) {
            f();
        }
    }

    __declspec(dllexport) int TestMacro1(int a, double b, const char* c)
    {
        DBGSCP_HOOK("TestMacro1", int, a, b, c)

        return DbgScp_TestInt(a, b, c);;
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
        DbgScp_TestVoid(a, b, c);
    }
    __declspec(dllexport) void TestMacro4(int a, double b, const char* c)
    {
        BEGIN_DBGSCP_HOOK_VOID()
            printf("a:%d b:%f c:%s\n", a, b, c);
        END_DBGSCP_HOOK_VOID("TestMacro4", a, b, c)
    }
}
