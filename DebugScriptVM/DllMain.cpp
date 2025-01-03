#include "DbgScpHook.h"
#include <mutex>
#include <string>
#include <vector>
#include <unordered_map>
#include <sstream>
#include <fstream>
#include <stdio.h>

#if PLATFORM_WIN || _MSC_VER
#include "windows.h"
#elif PLATFORM_ANDROID
#include <sys/types.h>
#include <sys/mman.h>
#include <unistd.h>
#elif UNITY_POSIX
#include <sys/types.h>
#include <sys/mman.h>
#include <unistd.h>
#include <pthread.h>
#endif

#if UNITY_APPLE
#include <execinfo.h>
#elif PLATFORM_ANDROID
#include "PlatformDependent/AndroidPlayer/Source/AndroidBacktrace.h"
#elif PLATFORM_SWITCH
#include "PlatformDependent/Switch/Source/Diagnostics/SwitchBacktrace.h"
#elif PLATFORM_LUMIN
#include "PlatformDependent/Lumin/Source/LuminBacktrace.h"
#elif PLATFORM_PLAYSTATION
#include "PlatformDependent/SonyCommon/Player/Native/PlayStationStackTrace.h"
#else
#define BACKTRACE_UNIMPLEMENTED 1
#endif

int64_t GetProcessId()
{
#if PLATFORM_WIN || _MSC_VER
    return static_cast<int64_t>(GetCurrentProcessId());
#elif UNITY_POSIX
    return static_cast<int64_t>(getpid());
#endif
    return 0;
}

int64_t GetThreadId()
{
#if PLATFORM_WIN || _MSC_VER
    return static_cast<int64_t>(GetCurrentThreadId());
#elif PLATFORM_ANDROID
    return static_cast<int64_t>(gettid());
#elif UNITY_POSIX
    return reinterpret_cast<int64_t>(pthread_self());
#endif
    return 0;
}

static const int c_max_log_file_num = 16;
static const int c_max_log_file_size = 1024 * 1024 * 1024;
static const std::streamsize c_log_buffer_size = 8 * 1024 * 1024;
static std::vector<char> g_SwapBuffer(c_log_buffer_size);
static std::stringstream g_LogBuffer{};
static std::recursive_mutex g_LogBufferMutex{};
static std::string g_LogFile[c_max_log_file_num] = {
    "dbgscp_log_0.txt","dbgscp_log_1.txt","dbgscp_log_2.txt","dbgscp_log_3.txt"
    "dbgscp_log_4.txt","dbgscp_log_5.txt","dbgscp_log_6.txt","dbgscp_log_7.txt",
    "dbgscp_log_8.txt","dbgscp_log_9.txt","dbgscp_log_10.txt","dbgscp_log_11.txt",
    "dbgscp_log_12.txt","dbgscp_log_13.txt","dbgscp_log_14.txt","dbgscp_log_15.txt"
};
static int g_LogIndex = 0;
static bool g_FirstLog = true;
static uint64_t g_LogSize = 0;

static inline bool DbgScp_FlushLog_NoLock(const char* pstr, size_t len)
{
    bool r = false;
    if (g_LogIndex < c_max_log_file_num) {
        FILE* fp = fopen(g_LogFile[g_LogIndex].c_str(), g_FirstLog ? "wt" : "at");
        if (fp) {
            auto&& pos = g_LogBuffer.tellp();
            g_LogSize += pos;

            std::streampos curPos = 0;
            g_LogBuffer.seekg(curPos, std::ios::beg);
            while (curPos < pos) {
                std::streamsize size = pos - curPos;
                if (size > c_log_buffer_size) {
                    size = c_log_buffer_size;
                }
                g_LogBuffer.read(g_SwapBuffer.data(), size);
                fwrite(g_SwapBuffer.data(), 1, size, fp);
                curPos += size;
            }
            if (pstr) {
                fwrite(pstr, 1, len, fp);
                g_LogSize += len;
            }
            fclose(fp);
            g_LogBuffer.str("");
            g_FirstLog = false;
            r = true;

            if (g_LogSize >= c_max_log_file_size) {
                g_FirstLog = true;
                ++g_LogIndex;
                g_LogSize = 0;
            }
        }
    }
    return r;
}
static inline bool DbgScp_FlushLog()
{
    std::lock_guard<std::recursive_mutex> lock(g_LogBufferMutex);

    return DbgScp_FlushLog_NoLock(nullptr, 0);
}
static inline bool DbgScp_WriteLog(const std::string& str)
{
    std::lock_guard<std::recursive_mutex> lock(g_LogBufferMutex);

    bool r = true;
    auto&& pos = g_LogBuffer.tellp();
    if (pos + static_cast<std::streamoff>(str.length()) > c_log_buffer_size) {
        r = DbgScp_FlushLog_NoLock(str.c_str(), str.length());
    }
    else {
        g_LogBuffer << str;
    }
    return r;
}
static inline void DbgScp_LogCallstack(const char* prefix, const char* file, int line)
{
    const int c_buf_size = 1024 * 4 + 1;
    char buf[c_buf_size];
    snprintf(buf, c_buf_size, "%s%s:%d\n", prefix, file, line);
    DbgScp_WriteLog(buf);

#if !BACKTRACE_UNIMPLEMENTED
    const size_t kMaxDepth = 100;

    void* stackAddr[kMaxDepth];
    int stackDepth = (int)backtrace(stackAddr, kMaxDepth);
    char** stackSymbol = backtrace_symbols(stackAddr, stackDepth);

    for (int i = 1, n = stackDepth; i < n; ++i) {
        snprintf(buf, c_buf_size, " #%02d %p %s\n", i - 1, stackAddr[i], stackSymbol[i]);
        DbgScp_WriteLog(buf);
    }
    free(stackSymbol);
#endif
}

struct MemoryInfo
{
    int64_t addr;
    size_t size;
    size_t align;
    bool locking;
    bool tlsf;
    int64_t inst;
    int64_t pool;
    int64_t tid;
    bool is_main_thread;
};

static std::unordered_map<int64_t, MemoryInfo> g_MemoryInfos{};
static std::recursive_mutex g_MemoryInfoMutex{};

static inline bool DbgScp_AddMemoryInfo(int64_t addr, size_t size, size_t align, bool locking, bool tlsf, int64_t inst, int64_t pool)
{
    std::lock_guard<std::recursive_mutex> lock(g_MemoryInfoMutex);

    MemoryInfo mi{ addr, size, align, locking, tlsf, inst, pool, GetThreadId(), true/*CurrentThread::IsMainThread()*/ };
    auto&& r = g_MemoryInfos.insert(std::make_pair(addr, std::move(mi)));
    return r.second;
}
static inline bool DbgScp_RemoveMemoryInfo(int64_t addr)
{
    std::lock_guard<std::recursive_mutex> lock(g_MemoryInfoMutex);

    auto&& r = g_MemoryInfos.erase(addr);
    return r > 0;
}
static inline bool DbgScp_GetMemoryInfo(int64_t addr, size_t& size, size_t& align, bool& locking, bool& tlsf, int64_t& inst, int64_t& pool, int64_t& tid, bool& is_main_thread, int64_t& cur_tid, bool& cur_is_main_thread, const char*& alloc_name)
{
    std::lock_guard<std::recursive_mutex> lock(g_MemoryInfoMutex);

    bool r = false;
    auto&& it = g_MemoryInfos.find(addr);
    if (it != g_MemoryInfos.end()) {
        auto&& info = it->second;

        size = info.size;
        align = info.align;
        locking = info.locking;
        tlsf = info.tlsf;
        inst = info.inst;
        pool = info.pool;
        tid = info.tid;
        is_main_thread = info.is_main_thread;

        cur_tid = GetThreadId();
        cur_is_main_thread = true;/*CurrentThread::IsMainThread()*/;

#if ENABLE_MEMORY_MANAGER
        auto&& ptr = GetMemoryManagerPtr();
        if (ptr) {
            auto&& alloc = ptr->GetAllocatorContainingPtr(reinterpret_cast<void*>(addr));
            if (alloc) {
                alloc_name = alloc->GetName();
            }
            else {
                alloc_name = "[unknown]";
            }
        }
        else {
            alloc_name = "";
        }
#else
        alloc_name = "";
#endif

        r = true;
    }
    return r;
}

extern "C" void LogCallStackOnDuplicateFreeMemory(const char* prefix, void* ptr)
{
    DBGSCP_HOOK_VOID("TlsfDuplicateFreeMemory", prefix, ptr)
}
extern "C" void LogOnTlsfAssert()
{
    DBGSCP_HOOK_VOID("LogOnTlsfAssert")
}
extern "C" void LogOnTlsfMemory(void* ptr)
{
    DBGSCP_HOOK_VOID("LogOnTlsfMemory", ptr)
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

static std::unordered_map<std::string, void*> g_Lib2Addresses{};

static inline void* LoadDynamicLibrary(const std::string& str)
{
	void* ptr = 0;
    auto&& it = g_Lib2Addresses.find(str);
    if (it == g_Lib2Addresses.end()) {
        ptr = LoadLibraryA(str.c_str());
        if (ptr) {
            g_Lib2Addresses.insert(std::make_pair(str, ptr));
        }
    }
    else {
        ptr = it->second;
    }
	return ptr;
}
static inline void* LookupSymbol(void* module, const std::string& str)
{
	return GetProcAddress(reinterpret_cast<HMODULE>(module), str.c_str());
}
static inline void* LoadAndLookupSymbol(const std::string& lib, const std::string& proc)
{
	void* ptr = 0;
    auto&& it = g_Lib2Addresses.find(lib);
    if (it == g_Lib2Addresses.end()) {
        ptr = LoadLibraryA(lib.c_str());
        if (ptr) {
            g_Lib2Addresses.insert(std::make_pair(lib, ptr));
        }
    }
    else {
        ptr = it->second;
    }
    if (ptr) {
        auto&& p = GetProcAddress(reinterpret_cast<HMODULE>(ptr), proc.c_str());
		return p;
    }
	return nullptr;
}
static inline void UnloadDynamicLibrary(void* module)
{
	FreeLibrary(reinterpret_cast<HMODULE>(module));
}
static inline bool UnloadDynamicLibrary(const std::string& str)
{
	bool r = false;
    auto&& it = g_Lib2Addresses.find(str);
    if (it != g_Lib2Addresses.end()) {
        FreeLibrary(reinterpret_cast<HMODULE>(it->second));
        g_Lib2Addresses.erase(it);
		r = true;
    }
	return r;
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

    std::string proc_file = path.str();
    std::ifstream maps_file(proc_file);
    if (!maps_file.is_open()) {
        printf("[seg]:can't open '%s'\n", proc_file.c_str());
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
            else {
                printf("[seg]:unexpected format: '%s'\n", line.c_str());
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
#if PLATFORM_WIN || _MSC_VER
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
    UnityAlloc,
    UnityRealloc,
    UnityDealloc,
    WriteLog,
    FlushLog,
    LogCallStack,
    AddMemoryInfo,
    RemoveMemoryInfo,
    GetMemoryInfo,
    IsMainThread,
    ThreadSleep,
    ThreadYield,
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
        void* ptr = LoadDynamicLibrary(str);
        int64_t addr = reinterpret_cast<int64_t>(ptr);
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, addr, stackBase, intLocals, intGlobals);
    }
    static inline void GetProc(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t addr = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        auto&& str = DebugScript::GetVarString(args[1].IsGlobal, args[1].Index, stackBase, strLocals, strGlobals);
        void* ptr = LookupSymbol(reinterpret_cast<void*>(addr), str);
        int64_t rval = reinterpret_cast<int64_t>(ptr);
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, rval, stackBase, intLocals, intGlobals);
    }
    static inline void LoadLibAndGetProc(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t rval = 0;
        auto&& lib = DebugScript::GetVarString(args[0].IsGlobal, args[0].Index, stackBase, strLocals, strGlobals);
        auto&& proc = DebugScript::GetVarString(args[1].IsGlobal, args[1].Index, stackBase, strLocals, strGlobals);
        void* ptr = LoadAndLookupSymbol(lib, proc);
        rval = reinterpret_cast<int64_t>(ptr);
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, rval, stackBase, intLocals, intGlobals);
    }
    static inline void FreeLib(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t addr = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        UnloadDynamicLibrary(reinterpret_cast<void*>(addr));
        int64_t rval = 1;
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, rval, stackBase, intLocals, intGlobals);
    }
    static inline void FreeLibByPath(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        auto&& str = DebugScript::GetVarString(args[0].IsGlobal, args[0].Index, stackBase, strLocals, strGlobals);
        UnloadDynamicLibrary(str);
        int64_t rval = 1;
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, rval, stackBase, intLocals, intGlobals);
    }
    static inline void GetPID(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t rval = GetProcessId();
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, rval, stackBase, intLocals, intGlobals);
    }
    static inline void GetTID(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t rval = GetThreadId();
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
        int64_t memId = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
#if ENABLE_MEMORY_MANAGER
        auto&& id = static_cast<MemLabelIdentifier>(memId);
        //We must also define the USE_MEMORY_DEBUGGING in the unity version. We cannot simply define the ENABLE_MEM_PROFILER because the compile error.
        GetMemoryManager().GetAllocator(MemLabelId{ id })->CheckIntegrity();
#endif
    }
    static inline void UnityAlloc(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t size = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        int64_t align = DebugScript::GetVarInt(args[1].IsGlobal, args[1].Index, stackBase, intLocals, intGlobals);
        int64_t label = DebugScript::GetVarInt(args[2].IsGlobal, args[2].Index, stackBase, intLocals, intGlobals);
        int64_t option = DebugScript::GetVarInt(args[3].IsGlobal, args[3].Index, stackBase, intLocals, intGlobals);
        int64_t addr = 0;
#if ENABLE_MEMORY_MANAGER
        auto&& ptr = GetMemoryManagerPtr();
        if (ptr) {
            auto&& id = static_cast<MemLabelIdentifier>(label);
            addr = reinterpret_cast<int64_t>(ptr->Allocate(static_cast<size_t>(size), static_cast<size_t>(align), MemLabelId{ id }, static_cast<AllocateOptions>(option), __FILE__, __LINE__));
        }
#endif
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, addr, stackBase, intLocals, intGlobals);
    }
    static inline void UnityRealloc(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t oldAddr = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        int64_t size = DebugScript::GetVarInt(args[1].IsGlobal, args[1].Index, stackBase, intLocals, intGlobals);
        int64_t align = DebugScript::GetVarInt(args[2].IsGlobal, args[2].Index, stackBase, intLocals, intGlobals);
        int64_t label = DebugScript::GetVarInt(args[3].IsGlobal, args[3].Index, stackBase, intLocals, intGlobals);
        int64_t option = DebugScript::GetVarInt(args[4].IsGlobal, args[4].Index, stackBase, intLocals, intGlobals);
        int64_t addr = 0;
#if ENABLE_MEMORY_MANAGER
        auto&& ptr = GetMemoryManagerPtr();
        if (ptr) {
            auto&& id = static_cast<MemLabelIdentifier>(label);
            addr = reinterpret_cast<int64_t>(ptr->Reallocate(reinterpret_cast<void*>(oldAddr), static_cast<size_t>(size), static_cast<size_t>(align), MemLabelId{ id }, static_cast<AllocateOptions>(option), __FILE__, __LINE__));
        }
#endif
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, addr, stackBase, intLocals, intGlobals);
    }
    static inline void UnityDealloc(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t addr = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        int64_t label = 0;
        if (argNum > 1) {
            label = DebugScript::GetVarInt(args[1].IsGlobal, args[1].Index, stackBase, intLocals, intGlobals);
        }
#if ENABLE_MEMORY_MANAGER
        auto&& ptr = GetMemoryManagerPtr();
        if (ptr) {
            if (label > 0) {
                auto&& id = static_cast<MemLabelIdentifier>(label);
                ptr->Deallocate(reinterpret_cast<void*>(addr), MemLabelId{ id }, __FILE__, __LINE__);
            }
            else {
                ptr->Deallocate(reinterpret_cast<void*>(addr), __FILE__, __LINE__);
            }
        }
#endif
    }
    static inline void WriteLog(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        const std::string& str = DebugScript::GetVarString(args[0].IsGlobal, args[0].Index, stackBase, strLocals, strGlobals);
        bool r = DbgScp_WriteLog(str);
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, r ? 1 : 0, stackBase, intLocals, intGlobals);
    }
    static inline void FlushLog(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        bool r = DbgScp_FlushLog();
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, r ? 1 : 0, stackBase, intLocals, intGlobals);
    }
    static inline void LogCallStack(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        const std::string& str = DebugScript::GetVarString(args[0].IsGlobal, args[0].Index, stackBase, strLocals, strGlobals);
        DbgScp_LogCallstack(str.c_str(), __FILE__, __LINE__);
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, 1, stackBase, intLocals, intGlobals);
    }
    static inline void AddMemoryInfo(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t addr = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        int64_t size = DebugScript::GetVarInt(args[1].IsGlobal, args[1].Index, stackBase, intLocals, intGlobals);
        int64_t align = DebugScript::GetVarInt(args[2].IsGlobal, args[2].Index, stackBase, intLocals, intGlobals);
        int64_t locking = DebugScript::GetVarInt(args[3].IsGlobal, args[3].Index, stackBase, intLocals, intGlobals);
        int64_t tlsf = DebugScript::GetVarInt(args[4].IsGlobal, args[4].Index, stackBase, intLocals, intGlobals);
        int64_t inst = DebugScript::GetVarInt(args[5].IsGlobal, args[5].Index, stackBase, intLocals, intGlobals);
        int64_t pool = DebugScript::GetVarInt(args[6].IsGlobal, args[6].Index, stackBase, intLocals, intGlobals);
        bool r = DbgScp_AddMemoryInfo(addr, size, align, locking, tlsf, inst, pool);
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, r ? 1 : 0, stackBase, intLocals, intGlobals);
    }
    static inline void RemoveMemoryInfo(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t addr = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        bool r = DbgScp_RemoveMemoryInfo(addr);
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, r ? 1 : 0, stackBase, intLocals, intGlobals);
    }
    static inline void GetMemoryInfo(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t addr = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        size_t size = 0, align = 0;
        bool locking = false, tlsf = false, is_main_thread = false, cur_is_main_thread = false;
        int64_t inst = 0, pool = 0, tid = 0, cur_tid = 0;
        const char* alloc_name = "";
        bool r = DbgScp_GetMemoryInfo(addr, size, align, locking, tlsf, inst, pool, tid, is_main_thread, cur_tid, cur_is_main_thread, alloc_name);
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, r ? 1 : 0, stackBase, intLocals, intGlobals);
        if (argNum > 1) {
            DebugScript::SetVarInt(args[1].IsGlobal, args[1].Index, size, stackBase, intLocals, intGlobals);
        }
        if (argNum > 2) {
            DebugScript::SetVarInt(args[2].IsGlobal, args[2].Index, align, stackBase, intLocals, intGlobals);
        }
        if (argNum > 3) {
            DebugScript::SetVarInt(args[3].IsGlobal, args[3].Index, locking ? 1 : 0, stackBase, intLocals, intGlobals);
        }
        if (argNum > 4) {
            DebugScript::SetVarInt(args[4].IsGlobal, args[4].Index, tlsf ? 1 : 0, stackBase, intLocals, intGlobals);
        }
        if (argNum > 5) {
            DebugScript::SetVarInt(args[5].IsGlobal, args[5].Index, inst, stackBase, intLocals, intGlobals);
        }
        if (argNum > 6) {
            DebugScript::SetVarInt(args[6].IsGlobal, args[6].Index, pool, stackBase, intLocals, intGlobals);
        }
        if (argNum > 7) {
            DebugScript::SetVarInt(args[7].IsGlobal, args[7].Index, tid, stackBase, intLocals, intGlobals);
        }
        if (argNum > 8) {
            DebugScript::SetVarInt(args[8].IsGlobal, args[8].Index, is_main_thread ? 1 : 0, stackBase, intLocals, intGlobals);
        }
        if (argNum > 9) {
            DebugScript::SetVarInt(args[9].IsGlobal, args[9].Index, cur_tid, stackBase, intLocals, intGlobals);
        }
        if (argNum > 10) {
            DebugScript::SetVarInt(args[10].IsGlobal, args[10].Index, cur_is_main_thread ? 1 : 0, stackBase, intLocals, intGlobals);
        }
        if (argNum > 11) {
            DebugScript::SetVarString(args[11].IsGlobal, args[11].Index, alloc_name, stackBase, strLocals, strGlobals);
        }
    }
    static inline void IsMainThread(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        bool r = true;//CurrentThread::IsMainThread();
        DebugScript::SetVarInt(retVal.IsGlobal, retVal.Index, r ? 1 : 0, stackBase, intLocals, intGlobals);
    }
    static inline void ThreadSleep(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
        int64_t ms = DebugScript::GetVarInt(args[0].IsGlobal, args[0].Index, stackBase, intLocals, intGlobals);
        //CurrentThread::SleepForSeconds(ms / 1000.0);
    }
    static inline void ThreadYield(int32_t stackBase, DebugScript::IntLocals& intLocals, DebugScript::FloatLocals& fltLocals, DebugScript::StringLocals& strLocals, DebugScript::IntGlobals& intGlobals, DebugScript::FloatGlobals& fltGlobals, DebugScript::StringGlobals& strGlobals, const ExternApiArgOrRetVal args[], int32_t argNum, const ExternApiArgOrRetVal& retVal)
    {
#ifdef Yield
#define old_yield Yield
#undef Yield
        //CurrentThread::Yield();
#ifdef old_yield
#define Yield old_yield
#endif
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
    case ExternApiEnum::UnityAlloc:
        ExternApi::UnityAlloc(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::UnityRealloc:
        ExternApi::UnityRealloc(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::UnityDealloc:
        ExternApi::UnityDealloc(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::WriteLog:
        ExternApi::WriteLog(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::FlushLog:
        ExternApi::FlushLog(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::LogCallStack:
        ExternApi::LogCallStack(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::AddMemoryInfo:
        ExternApi::AddMemoryInfo(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::RemoveMemoryInfo:
        ExternApi::RemoveMemoryInfo(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::GetMemoryInfo:
        ExternApi::GetMemoryInfo(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::IsMainThread:
        ExternApi::IsMainThread(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::ThreadSleep:
        ExternApi::ThreadSleep(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    case ExternApiEnum::ThreadYield:
        ExternApi::ThreadYield(stackBase, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, args, argNum, retVal);
        break;
    default:
        break;
    }
}

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
