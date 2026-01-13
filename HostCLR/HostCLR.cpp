// HostCLR.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
#include <string>
#include <vector>
#include "mono/MiniMonoLoader.h"
#include "mono/MonoIncludes.h"

#if defined(_MSC_VER)
#include "windows.h"
#include "coreclr/nethost.h"
#include "coreclr/coreclr_delegates.h"
#include "coreclr/hostfxr.h"
#elif defined(__APPLE__)
#include <TargetConditionals.h>
#if TARGET_OS_OSX
#include <iostream>
#include <dlfcn.h>
#include "coreclr/nethost.h"
#include "coreclr/coreclr_delegates.h"
#include "coreclr/hostfxr.h"
#endif
#endif

#if PLATFORM_OSX || PLATFORM_LINUX
#include <dlfcn.h>
#include <signal.h>
#include <unistd.h>
#include <sys/stat.h>
#endif
#if PLATFORM_WIN
#include <signal.h>
#include <fileapi.h>
#include <io.h>
#endif
#if PLATFORM_OSX
#include <mach/task.h>
#elif PLATFORM_LINUX
#endif

#if PLATFORM_OSX
static void HandleSignal(int i, __siginfo* info, void* p);
#define SA_DISABLE    0x0004  // disable taking signals on alternate stack
#elif PLATFORM_LINUX
static void HandleSignal(int i, siginfo_t* info, void* p);
#elif PLATFORM_WIN && !defined(WIN_API_SUBSET)
static int HandleSignal(EXCEPTION_POINTERS* ep);
static void HandleSignal(int signal);
#endif

#if (PLATFORM_OSX || PLATFORM_LINUX) && !PLATFORM_APPLE_NONDESKTOP
void SetupSignalHandler(int signal)
{
    struct sigaction action;
    memset(&action, 0, sizeof(action));
    action.sa_sigaction = HandleSignal;
    action.sa_flags = SA_SIGINFO;
    if (sigaction(signal, &action, nullptr) < 0)
        printf("Error setting signal handler for signal %d\n", signal);
}

#endif

void SetupSignalHandlers()
{
#if PLATFORM_OSX || PLATFORM_LINUX
    SetupSignalHandler(SIGSEGV);
    SetupSignalHandler(SIGBUS);
    SetupSignalHandler(SIGFPE);
    SetupSignalHandler(SIGILL);
    SetupSignalHandler(SIGABRT);
    SetupSignalHandler(SIGTRAP);
#endif

#if PLATFORM_WIN && !defined(WIN_API_SUBSET)
    _set_abort_behavior(0, _WRITE_ABORT_MSG);
    signal(SIGABRT, HandleSignal);
    signal(SIGSEGV, HandleSignal);
#endif

#if PLATFORM_WIN || PLATFORM_OSX || PLATFORM_ANDROID || PLATFORM_LINUX
    mono_set_signal_chaining(1);

#if ENABLE_MONO && LOAD_MONO_DYNAMICALLY
    if (mono_set_crash_chaining != nullptr)
    {
        mono_set_crash_chaining(1);
    }
#endif
#endif
}

void MonoPrintfRedirect(const char* msg, va_list list)
{
    va_list args;
    va_copy(args, list);
    int len = vprintf(msg, args);
    va_end(args);
}

void MonoLoggingCallback(const char* log_domain, const char* log_level, const char* message, bool fatal, void* user_data)
{
    printf("%s : %s : %s\n", log_domain, log_level, message);

    if (fatal)
    {
        printf("Fatal log received from mono! Aborting!\n");
        abort();
    }
}

#if PLATFORM_LINUX
#define SKIP_FRAMES 4 // HandleSignal, mono_breakpoint_clean_code, mono_print_method_from_ip, funlockfile
#define MAX_FRAMES 256
static void HandleSignal(int i, siginfo_t* info, void* p)
{
    printf("Caught fatal signal - signo:%d code:%d errno:%d addr:%p\n",
        info->si_signo,
        info->si_code,
        info->si_errno,
        info->si_addr);

    PrintStackTraceLinux(SKIP_FRAMES, MAX_FRAMES);

    // Continue with the default handler for the signal in question
    signal(info->si_signo, SIG_DFL);
    kill(getpid(), info->si_signo);
}

#undef SKIP_FRAMES
#undef MAX_FRAMES
//endif PLATFORM_LINUX

#elif PLATFORM_OSX
static void HandleSignal(int i, __siginfo* info, void* p)
{
    PrintStackTraceOSX(p);
    OnSignalReceived();

    // Continue with the default handler for the signal in question
    signal(info->si_signo, SIG_DFL);
    kill(getpid(), info->si_signo);
}

//endif PLATFORM_OSX

#elif PLATFORM_WIN
static int __cdecl HandleSignal(EXCEPTION_POINTERS* ep)
{
    return EXCEPTION_EXECUTE_HANDLER;
}

static void __cdecl HandleSignal(int signal)
{
    std::string signame;

    const int kAbortSignalSkipFrames = 6;
    const int kSegfaultSkipFrames = 11;

    int skipFrames = 0;

    switch (signal)
    {
    case SIGABRT:
        signame = "SIGABRT";
        skipFrames = kAbortSignalSkipFrames;
        break;
    case SIGSEGV:
        signame = "SIGSEGV";
        skipFrames = kSegfaultSkipFrames;
        break;
    default:
        signame = std::to_string(signal);
    }

    printf("Received signal %s\n", signame.c_str());
    std::string trace;// = GetStacktrace(skipFrames);

    printf("Obtained %lld stack frames\n%s\n", std::count(trace.begin(), trace.end(), '\n'), trace.c_str());
}

#endif //PLATFORM_WIN

void CleanupMono()
{
#if DEBUGMODE
    printf("Cleanup mono\n");
#endif

    mono_unity_jit_cleanup(mono_get_root_domain());

#if PLATFORM_OSX
    struct sigaction sa;
    sa.sa_sigaction = nullptr;
    sigemptyset(&sa.sa_mask);
    sa.sa_flags = SA_DISABLE;
    sigaction(SIGSEGV, &sa, nullptr);

    sa.sa_sigaction = nullptr;
    sigemptyset(&sa.sa_mask);
    sa.sa_flags = SA_DISABLE;
    sigaction(SIGABRT, &sa, nullptr);
#elif PLATFORM_LINUX
    struct sigaction sa;
    sa.sa_sigaction = nullptr;
    sigemptyset(&sa.sa_mask);
    sigaction(SIGSEGV, &sa, nullptr);

    sa.sa_sigaction = nullptr;
    sigemptyset(&sa.sa_mask);
    sigaction(SIGABRT, &sa, nullptr);
#endif
}

MonoMethod* FindMonoMethod(MonoClass* klass, const char* name, int argsCount, MonoType** argTypes)
{
    if (!name)
        return nullptr;

    void* iterator = nullptr;
    while (MonoMethod* monoMethod = mono_class_get_methods(klass, &iterator))
    {
        if (!monoMethod)
            return nullptr;

        const char* methodName = mono_method_get_name(monoMethod);
        if ((methodName[0] != name[0]) || strcmp(methodName, name) != 0)
            continue;

        if (argsCount < 0)
            return monoMethod;

        MonoMethodSignature* sig = mono_method_signature(monoMethod);
        if (!sig) {
            printf("Error looking up signature for %s.%s\n", mono_class_get_name(klass), methodName);
        }
        if (argsCount != mono_signature_get_param_count(sig))
            continue;

        if (argsCount > 0 && argTypes != nullptr)
        {
            MonoType* argType;
            void* iter = nullptr;
            bool didAllArgumentsMatch = true;
            for (int i = 0; i < argsCount && didAllArgumentsMatch; ++i)
            {
                argType = mono_signature_get_params(sig, &iter);
                didAllArgumentsMatch = didAllArgumentsMatch && (argType != nullptr) && mono_metadata_type_equal(argTypes[i], argType);
            }
            if (!didAllArgumentsMatch)
                continue;
        }

        if (!unity_mono_method_is_inflated(monoMethod) && unity_mono_method_is_generic(monoMethod))
            continue;
    }

    MonoClass* parent = mono_class_get_parent(klass);
    if (!parent)
        return nullptr;
    return FindMonoMethod(parent, name, argsCount, argTypes);
}

// Cross-platform character set conversion functions
extern std::string WideStringToUtf8(const wchar_t* wstr);
extern std::wstring Utf8ToWstring(const char* str);
extern bool WideToUtf8ToBuffer(const wchar_t* wstr, char* buf, int bufSize);
extern bool MultiByteToWideBuffer(const char* src, wchar_t* buf, int bufSize, unsigned int codePage = 65001);  // CP_UTF8 = 65001

std::string WideStringToUtf8(const wchar_t* wstr)
{
    if (!wstr) return std::string();

#if defined(_MSC_VER)
    int size_needed = ::WideCharToMultiByte(CP_UTF8, 0, wstr, -1, nullptr, 0, nullptr, nullptr);
    if (size_needed == 0) {
        return std::string();
    }

    std::string result;
    result.resize(size_needed - 1, '\0');
    ::WideCharToMultiByte(CP_UTF8, 0, wstr, -1, &result[0], size_needed, nullptr, nullptr);
    return result;
#else
    // On non-Windows platforms, use standard library conversion
    std::mbstate_t state = std::mbstate_t();
    std::size_t len = std::wcsrtombs(nullptr, &wstr, 0, &state);
    if (len == static_cast<std::size_t>(-1)) {
        return std::string();
    }
    std::string result(len, '\0');
    std::wcsrtombs(&result[0], &wstr, len, &state);
    return result;
#endif
}

std::wstring Utf8ToWstring(const char* str)
{
    if (!str) return std::wstring();

#if defined(_MSC_VER)
    int size_needed = ::MultiByteToWideChar(CP_UTF8, 0, str, -1, nullptr, 0);
    if (size_needed == 0) {
        return std::wstring();
    }

    std::wstring wstr;
    wstr.resize(size_needed, '\0');
    ::MultiByteToWideChar(CP_UTF8, 0, str, -1, &wstr[0], size_needed);

    if (!wstr.empty()) wstr.pop_back();
    return wstr;
#else
    // On non-Windows platforms, use standard library conversion
    std::mbstate_t state = std::mbstate_t();
    const char* ptr = str;
    std::size_t len = std::mbsrtowcs(nullptr, &ptr, 0, &state);
    if (len == static_cast<std::size_t>(-1)) {
        return std::wstring();
    }
    std::wstring result(len, L'\0');
    ptr = str;
    std::mbsrtowcs(&result[0], &ptr, len, &state);
    return result;
#endif
}

bool WideToUtf8ToBuffer(const wchar_t* wstr, char* buf, int bufSize)
{
    if (!wstr || !buf || bufSize <= 0) return false;

#if defined(_MSC_VER)
    int needed = ::WideCharToMultiByte(CP_UTF8, 0, wstr, -1, nullptr, 0, nullptr, nullptr);
    if (needed == 0 || needed > bufSize) return false;
    ::WideCharToMultiByte(CP_UTF8, 0, wstr, -1, buf, needed, nullptr, nullptr);
    return true;
#else
    std::mbstate_t state = std::mbstate_t();
    std::size_t len = std::wcsrtombs(nullptr, &wstr, 0, &state);
    if (len == static_cast<std::size_t>(-1) || static_cast<int>(len) >= bufSize) {
        return false;
    }
    std::wcsrtombs(buf, &wstr, bufSize, &state);
    return true;
#endif
}

bool MultiByteToWideBuffer(const char* src, wchar_t* buf, int bufSize, unsigned int codePage)
{
    if (!src || !buf || bufSize <= 0) return false;

#if defined(_MSC_VER)
    int needed = ::MultiByteToWideChar(codePage, 0, src, -1, nullptr, 0);
    if (needed == 0 || needed > bufSize) return false;
    int ret = ::MultiByteToWideChar(codePage, 0, src, -1, buf, bufSize);
    return ret != 0;
#else
    // On non-Windows platforms, ignore codePage and use standard conversion
    std::mbstate_t state = std::mbstate_t();
    const char* ptr = src;
    std::size_t len = std::mbsrtowcs(nullptr, &ptr, 0, &state);
    if (len == static_cast<std::size_t>(-1) || static_cast<int>(len) >= bufSize) {
        return false;
    }
    ptr = src;
    std::mbsrtowcs(buf, &ptr, bufSize, &state);
    return true;
#endif
}

void* load_library(const char* path)
{
#if defined(_MSC_VER)
    //HMODULE h = ::LoadLibraryA(path);
    //return reinterpret_cast<void*>(h);
    wchar_t wpath[1025];
    MultiByteToWideBuffer(path, wpath, 1024);
    HMODULE h = LoadLibraryW(wpath);
    return reinterpret_cast<void*>(h);
#else
    return dlopen(path, RTLD_LAZY | RTLD_LOCAL);
#endif
}

void* get_export(void* h, const char* name)
{
#if defined(_MSC_VER)
    return reinterpret_cast<void*>(::GetProcAddress(reinterpret_cast<HMODULE>(h), name));
#else
    return dlsym(h, name);
#endif
}

void free_library(void* h)
{
#if defined(_MSC_VER)
    ::FreeLibrary(reinterpret_cast<HMODULE>(h));
#else
    dlclose(h);
#endif
}

void host_output_log(const char* msg);
void printf_log(const char* fmt, ...)
{
    va_list vl;
    va_start(vl, fmt);
    char buffer[4097];
    int len = vsnprintf(buffer, sizeof(buffer) - 1, fmt, vl);
    buffer[len] = '\0';
    va_end(vl);

    host_output_log(buffer);

#if defined(_MSC_VER)
    // Convert UTF-8 to wide char for OutputDebugString
    std::wstring wbuffer = Utf8ToWstring(buffer);
    ::OutputDebugStringW(wbuffer.c_str());
#endif
}

typedef int (*host_test_fn)(int a, float b, const char* c);
typedef void (*host_output_log_fn)(const char* msg);
typedef bool (*host_run_command_fn)(const char* cmd, const char* args, void* result);
typedef bool (*host_run_command_timeout_fn)(const char* cmd, const char* args, int timeout, void* result);
typedef bool (*host_find_in_path_fn)(const char* filename, char* path, int& path_size);
typedef bool (*host_get_adb_exe_fn)(char* path, int& path_size);
typedef bool (*host_get_apktool_jar_fn)(char* path, int& path_size);
typedef bool (*host_get_jadx_exe_fn)(char* path, int& path_size);
typedef bool (*host_get_java_exe_fn)(char* path, int& path_size);
typedef bool (*host_get_uberapksigner_jar_fn)(char* path, int& path_size);
typedef bool (*host_get_zipalign_exe_fn)(char* path, int& path_size);
typedef int (*host_get_java_heap_fn)();
typedef void (*host_emit_decompile_failed_fn)(void* worker);
typedef void (*host_emit_recompile_failed_fn)(void* worker);
typedef void (*host_emit_sign_failed_fn)(void* worker);
typedef void (*host_emit_install_failed_fn)(void* worker);

typedef struct {
    host_test_fn test;
    host_output_log_fn OutputLog;
    host_run_command_fn RunCommand;
    host_run_command_timeout_fn RunCommandTimeout;
    host_find_in_path_fn FindInPath;
    host_get_adb_exe_fn GetAdbExe;
} HostApi;

int host_test(int a, float b, const char* c) {
    printf("native host_test called: %d, %f, %s\n", a, b, c);
    return static_cast<int>(a * b);
}
void host_output_log(const char* msg)
{
}
bool host_run_command(const char* cmd, const char* args, void* result)
{
    if (!cmd || !args || !result) {
        return false;
    }
    return true;
}
bool host_run_command_timeout(const char* cmd, const char* args, int timeout, void* result)
{
    if (!cmd || !args || !result) {
        return false;
    }
    return true;
}
bool host_find_in_path(const char* filename, char* path, int& path_size)
{
    if (!filename || !path) {
        return false;
    }
    return true;
}
bool host_get_adb_exe(char* path, int& path_size)
{
    if (!path) {
        return false;
    }
    return true;
}

#define USE_SPEC_DOTNET

static load_assembly_and_get_function_pointer_fn load_assembly_and_get_function_pointer = nullptr;
// Function to initialize .NET Core runtime
int load_hostfxr(int& out_rc)
{
    out_rc = 0;
#ifdef USE_SPEC_DOTNET
    // Load hostfxr.dll and use dotnet framework in specific directory
    const char* hostfxr_path = "hostfxr.dll";
    void* hostfxr_lib = load_library(hostfxr_path);
    if (!hostfxr_lib)
    {
        printf_log("Failed to load hostfxr.dll");
        return -2;
    }
#else
#ifdef _WIN32
    wchar_t hostfxr_path_w[1024];
    size_t sz = sizeof(hostfxr_path_w) / sizeof(wchar_t);
    int rc0 = get_hostfxr_path(hostfxr_path_w, &sz, nullptr);
    if (rc0 != 0) {
        printf_log("get_hostfxr_path failed: %d", rc0);
        out_rc = rc0;
        return -1;
    }
    char path[1025];
    WideToUtf8ToBuffer(hostfxr_path_w, path, 1024);
    printf_log("[native] hostfxr path: %s\n", path);
#else
    char hostfxr_path[PATH_MAX];
    size_t sz = sizeof(hostfxr_path);
    int rc0 = get_hostfxr_path(hostfxr_path, &sz, nullptr);
    if (rc0 != 0) {
        printf_log("get_hostfxr_path failed: %d", rc0);
        out_rc = rc0;
        return -1;
    }
    printf("[native] hostfxr path: %s\n", hostfxr_path);
#endif

#ifdef _WIN32
    HMODULE hostfxr_lib = LoadLibraryW(hostfxr_path_w);
    if (!hostfxr_lib) {
        out_rc = static_cast<int>(::GetLastError());
        printf_log("LoadLibraryW failed\n");
        return -2;
    }
#else
    void* hostfxr_lib = load_library(hostfxr_path);
    if (!hostfxr_lib) {
        return -2;
    }
#endif

#endif

    auto init_cmdline_fptr = reinterpret_cast<hostfxr_initialize_for_dotnet_command_line_fn>(get_export(hostfxr_lib, "hostfxr_initialize_for_dotnet_command_line"));
    auto run_app_fptr = reinterpret_cast<hostfxr_run_app_fn>(get_export(hostfxr_lib, "hostfxr_run_app"));
    auto init_config_fptr = reinterpret_cast<hostfxr_initialize_for_runtime_config_fn>(get_export(hostfxr_lib, "hostfxr_initialize_for_runtime_config"));
    auto get_delegate_fptr = reinterpret_cast<hostfxr_get_runtime_delegate_fn>(get_export(hostfxr_lib, "hostfxr_get_runtime_delegate"));
    auto close_fptr = reinterpret_cast<hostfxr_close_fn>(get_export(hostfxr_lib, "hostfxr_close"));

    if (!init_cmdline_fptr || !run_app_fptr || !init_config_fptr || !get_delegate_fptr || !close_fptr)
    {
        printf_log("Failed to get hostfxr functions");
        return -3;
    }

#ifdef USE_SPEC_DOTNET
    // Initialize the .NET Core runtime
    hostfxr_initialize_parameters parameters{
        sizeof(hostfxr_initialize_parameters),
        L"./",
        L"./dotnet/Microsoft.NETCore.App/9.0.2"
    };

    hostfxr_handle cxt = nullptr;
    int rc = init_config_fptr(L"./Managed/myapp.runtimeconfig.json", &parameters, &cxt);
#else
    hostfxr_handle cxt = nullptr;
    int rc = init_config_fptr(L"./Managed/myapp.runtimeconfig.json", nullptr, &cxt);
#endif

    //int argc = 1;
    //const char_t* argv[] = { L"./publish/myapp.dll" };
    //int rc = init_cmdline_fptr(argc, argv, &parameters, &cxt);
    if (rc != 0 || cxt == nullptr)
    {
        printf_log("Failed to initialize .NET Core runtime: %d", rc);
        out_rc = rc;
        return -4;
    }

    // Get the delegate for the runtime
    rc = get_delegate_fptr(cxt, hdt_load_assembly_and_get_function_pointer, reinterpret_cast<void**>(&load_assembly_and_get_function_pointer));
    if (rc != 0 || load_assembly_and_get_function_pointer == nullptr)
    {
        printf_log("Failed to get load_assembly_and_get_function_pointer: %d", rc);
        out_rc = rc;
        return -5;
    }

    //run_app_fptr(cxt);

    // Close the host context
    close_fptr(cxt);

    return 0;
}

// Function to call .NET Core method
int call_dotnet_method()
{
    const char_t* dotnet_assembly_path = L"./Managed/MyApp.dll";
    const char_t* dotnet_class_name = L"DotNetLib.Lib, MyApp";

    // native api
    HostApi api;
    api.test = &host_test;
    api.OutputLog = &host_output_log;
    api.RunCommand = &host_run_command;
    api.RunCommandTimeout = &host_run_command_timeout;
    api.FindInPath = &host_find_in_path;
    api.GetAdbExe = &host_get_adb_exe;

    // For UNMANAGEDCALLERSONLY_METHOD, this must be int (or other directly copyable type), not bool.
    typedef int (CORECLR_DELEGATE_CALLTYPE* register_api_fn)(void* arg);
    register_api_fn entry = nullptr;
    int rc = load_assembly_and_get_function_pointer(
        dotnet_assembly_path,
        dotnet_class_name,
        L"RegisterApi",
        UNMANAGEDCALLERSONLY_METHOD,
        nullptr,
        (void**)&entry);
    _ASSERT(rc == 0 && entry != nullptr && "Failure: load_assembly_and_get_function_pointer()");

    if (entry) {
        int result = entry(&api);
        printf("managed returned: %d\n", result);
    }

    // Call the .NET Core method, test StringBuilder
    typedef int (CORECLR_DELEGATE_CALLTYPE* get_zipalign_args_fn)(const char* folder, char* args, int& args_size);
    get_zipalign_args_fn get_zipalign_args = nullptr;

    rc = load_assembly_and_get_function_pointer(
        dotnet_assembly_path,
        dotnet_class_name,
        L"GetZipAlignArgs",
        L"DotNetLib.Lib+GetZipAlignArgsDelegation, MyApp", // Delegate type
        nullptr,
        (void**)&get_zipalign_args);
    _ASSERT(rc == 0 && get_zipalign_args != nullptr && "Failure: load get_zipalign_args");

    if (get_zipalign_args) {
        char buffer[1025];
        int args_size = 1024;
        int r = get_zipalign_args("test", buffer, args_size);
        buffer[args_size] = 0;
        printf("args: %s ret: %d\n", buffer, r);
    }

    // Call the .NET Core method
    struct lib_args
    {
        const char_t* message;
        int number;
    };
    // Define the method signature
    typedef void (CORECLR_DELEGATE_CALLTYPE* custom_entry_fn)(lib_args args);
    typedef void (CORECLR_DELEGATE_CALLTYPE* hello2_fn)(const char_t* arg);

    component_entry_point_fn hello = nullptr;
    rc = load_assembly_and_get_function_pointer(
        dotnet_assembly_path,
        dotnet_class_name,
        L"Hello",
        nullptr, // Delegate type (null for default)
        nullptr,
        (void**)&hello);
    _ASSERT(rc == 0 && hello != nullptr && "Failure: load_assembly_and_get_function_pointer()");

    lib_args args{ L"Hello from C++", -1 };
    if (hello) {
        hello(&args, sizeof(args));
    }

    custom_entry_fn custom_entry = nullptr;
    rc = load_assembly_and_get_function_pointer(
        dotnet_assembly_path,
        dotnet_class_name,
        L"CustomEntryPoint",
        L"DotNetLib.Lib+CustomEntryPointDelegate, MyApp",
        nullptr, // Reserved for future use
        reinterpret_cast<void**>(&custom_entry) // Output function pointer
    );
    _ASSERT(rc == 0 && custom_entry != nullptr && "Failure: load_assembly_and_get_function_pointer()");

    if (custom_entry) {
        custom_entry(args);
    }

    // UnmanagedCallersOnly
    rc = load_assembly_and_get_function_pointer(
        dotnet_assembly_path,
        dotnet_class_name,
        L"CustomEntryPointUnmanagedCallersOnly",
        UNMANAGEDCALLERSONLY_METHOD,
        nullptr,
        (void**)&custom_entry);
    _ASSERT(rc == 0 && custom_entry != nullptr && "Failure: load_assembly_and_get_function_pointer()");

    if (custom_entry) {
        custom_entry(args);
    }

    // UnmanagedCallersOnly
    hello2_fn hello2 = nullptr;
    rc = load_assembly_and_get_function_pointer(
        dotnet_assembly_path,
        dotnet_class_name,
        L"Hello2",
        UNMANAGEDCALLERSONLY_METHOD,
        nullptr,
        (void**)&hello2);
    _ASSERT(rc == 0 && hello2 != nullptr && "Failure: load_assembly_and_get_function_pointer()");

    if (hello2) {
        hello2(L"test test test");
    }
    return 0;
}

static void LogStringApi(MonoString* info)
{
    const char* str = mono_string_to_utf8(info);
    std::cout << str << std::endl;
    mono_free(str);
}
static MonoString* GetInfoApi(void)
{
    return mono_string_new(mono_domain_get(), "Hello from mono !");
}

void ConvertSeparatorsToPlatform(std::string& pathName)
{
#if _MSC_VER
    std::string::iterator it = pathName.begin(), itEnd = pathName.end();
    while (it != itEnd)
    {
        if (*it == '/')
            *it = '\\';
        ++it;
    }
#endif
}
static std::string BuildMonoPath(const std::vector<std::string>& monoPaths)
{
    const char separator = '\0';

    std::string monoPath;
    for (size_t i = 0; i != monoPaths.size(); i++)
    {
        if (i != 0)
            monoPath.push_back(separator);
        monoPath.append(monoPaths[i]);
    }

    //Append two null terminators when there are no more paths
    monoPath.push_back(separator);
    monoPath.push_back(separator);

    // Mono uses platform separators
    ConvertSeparatorsToPlatform(monoPath);

    return monoPath;
}

static MonoDomain* g_MonoDomain = nullptr;

bool LoadAndInitializeMono(const std::vector<std::string>& monoPaths, const std::string& monoConfigPath, const std::string& dataPath, const std::string& monoDll, int argc, const char** argv)
{
    if (!LoadMono(monoDll))
        return false;

    const char* emptyarg = "";
    if (argv == nullptr)
        argv = &emptyarg;

    mono_unity_set_vprintf_func(MonoPrintfRedirect);
    mono_trace_set_log_handler(MonoLoggingCallback, nullptr);

    // Mono works with paths that uses platform separators
    std::string assemblyDir = monoPaths[0];
    std::string configDir = monoConfigPath;
    ConvertSeparatorsToPlatform(assemblyDir);
    ConvertSeparatorsToPlatform(configDir);
    mono_set_dirs(assemblyDir.c_str(), configDir.c_str());
    mono_set_assemblies_path_null_separated(BuildMonoPath(monoPaths).c_str());

    SetupSignalHandlers();

    int opt = mono_parse_default_optimizations(nullptr);
    mono_set_defaults(0, opt);

    mono_unity_runtime_set_main_args(argc, argv);
    mono_unity_set_data_dir(dataPath.c_str());

    g_MonoDomain = mono_jit_init_version("mymono", "v4.0.30319");

    mono_config_parse(nullptr);

    mono_unity_set_embeddinghostname("mymono");

    mono_runtime_unhandled_exception_policy_set(MONO_UNHANDLED_POLICY_LEGACY);

    mono_add_internal_call("Api::GetInfo", reinterpret_cast<void*>(GetInfoApi));
    mono_add_internal_call("Api::LogString", reinterpret_cast<void*>(LogStringApi));

}

struct MonoAutoAttachDetach
{
    MonoAutoAttachDetach() : m_Thread(nullptr)
    {
        m_NeedAttached = mono_domain_get() == nullptr;
        if (m_NeedAttached) {
            m_Thread = mono_thread_attach(g_MonoDomain);
        }
    }
    ~MonoAutoAttachDetach()
    {
        if (m_NeedAttached && m_Thread) {
            mono_thread_detach(m_Thread);
        }
    }
private:
    bool m_NeedAttached;
    MonoThread* m_Thread;
};

bool TestDotnetCore(const std::string& dataPath)
{
    MonoAssembly* assembly;

    const char* dllName = "MyApp.dll";
    const char* ns = "DotNetLib";
    const char* className = "Lib";

    MonoDomain* domain = g_MonoDomain;
    assembly = mono_domain_assembly_open(domain, (dataPath + "/" + dllName).c_str());
    if (!assembly) {
        printf("mono: failed to open assembly %s\n", dllName);
        return false;
    }

    //If the dll uses dotnet core SDK, we can call Program.Main in the dll in embedded mode without terminating the process
    const char* args_main[] = { dllName };
    mono_jit_exec(domain, assembly, 1, args_main);

    int exitCode = mono_environment_exitcode_get();
    printf("mono: Program.Main return %d\n", exitCode);

    MonoImage* image;
    image = mono_assembly_get_image(assembly);
    if (!image) {
        printf("mono: failed to get image for %s\n", dllName);
        return false;
    }
    MonoClass* pClass = mono_class_from_name(image, ns, className);
    if (!pClass) {
        printf("mono: failed to get class for %s.%s\n", ns, className);
        return false;
    }
    MonoMethod* pMethod = FindMonoMethod(pClass, "HelloMono", -1, nullptr);
    if (!pMethod) {
        printf("mono: failed to get method for HelloMono\n");
        return false;
    }
    int cmd = 0;
    MonoString* pString = mono_string_new(domain, "HostMono");
    MonoString* pRefString = mono_string_new(domain, "test test test");
    void* addr = nullptr;
    void* args[4];
    args[0] = &cmd;
    args[1] = pString;
    args[2] = &pRefString;
    args[3] = &addr;
    MonoObject* pObject = mono_runtime_invoke(pMethod, nullptr, (void**)args, nullptr);
    if (!pObject) {
        printf("mono: failed to invoke method for HelloMono\n");
        return false;
    }
    const char* pStr = mono_string_to_utf8(pRefString);
    std::string newArgVal = pStr;
    mono_free(pStr);
    int val = *reinterpret_cast<int*>(mono_object_unbox(pObject));
    printf("mono: result = %d, %s\n", val, newArgVal.c_str());
    return true;
}

bool TestDotnetFramework(const std::string& dataPath)
{
    MonoAssembly* assembly;

    const char* dllName = "CSharpShaderCompiler.dll";
    const char* ns = "CSharpShaderCompiler";
    const char* className = "Entry";

    MonoDomain* domain = g_MonoDomain;
    assembly = mono_domain_assembly_open(domain, (dataPath + "/" + dllName).c_str());
    if (!assembly) {
        printf("mono: failed to open assembly %s\n", dllName);
        return false;
    }

    MonoImage* image;
    image = mono_assembly_get_image(assembly);
    if (!image) {
        printf("mono: failed to get image for %s\n", dllName);
        return false;
    }
    MonoClass* pClass = mono_class_from_name(image, ns, className);
    if (!pClass) {
        printf("mono: failed to get class for %s.%s\n", ns, className);
        return false;
    }
    MonoMethod* pMethod = FindMonoMethod(pClass, "Log", -1, nullptr);
    if (!pMethod) {
        printf("mono: failed to get method for Log\n");
        return false;
    }
    auto* title = mono_string_new(domain, "HostMono");
    auto* info = mono_string_new(domain, "test test test");
    // Primitive values ​​and pointers must be passed by reference, if the mono type pointer has the ref modifier
    // then the address of the pointer is passed, otherwise the pointer is passed as an argument.
    void* args[2];
    args[0] = title;
    args[1] = info;
    mono_runtime_invoke(pMethod, nullptr, (void**)args, nullptr);
    return true;
}

static const char* GetMonoClasslibsProfile()
{
#if PLATFORM_WIN
    return "unityjit-win32";
#elif PLATFORM_OSX
    return "unityjit-macos";
#elif PLATFORM_LINUX
    return "unityjit-linux";
#else
    return "unityjit";
#endif
}

int main(int argc, const char* argv[])
{
    std::cout << "HostCLR started.\n";

    bool use_coreclr = argc > 1 ? _stricmp(argv[1], "coreclr") == 0 : false;
    if (use_coreclr) {
        int rc = 0;
        load_hostfxr(rc);
        call_dotnet_method();
    }
    else {
        const char* dataPath = "./Managed";
        std::vector<std::string> paths{ dataPath,
            std::string("./MonoBleedingEdge/lib/mono/") + GetMonoClasslibsProfile(),
            "./MonoBleedingEdge/lib",
            "./MonoBleedingEdge/bin"
        };
        LoadAndInitializeMono(paths, "./MonoBleedingEdge/etc", dataPath, "./MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll", 0, nullptr);

        bool testDotnetCore = false;
        if (testDotnetCore) {
            TestDotnetCore(dataPath);
        }
        else {
            TestDotnetFramework(dataPath);
        }
        UnloadMono();
    }
}
