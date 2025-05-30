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

    printf("Obtained %d stack frames\n%s\n", std::count(trace.begin(), trace.end(), '\n'), trace.c_str());
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

void* load_library(const char* path)
{
#if defined(_MSC_VER)
    HMODULE h = ::LoadLibraryA(path);
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

static load_assembly_and_get_function_pointer_fn load_assembly_and_get_function_pointer = nullptr;
// Function to initialize .NET Core runtime
int load_hostfxr()
{
    const char* hostfxr_path = "hostfxr.dll";
    void* lib = load_library(hostfxr_path);
    if (!lib)
    {
        std::cerr << "Failed to load hostfxr.dll" << std::endl;
        return -1;
    }

    auto init_cmdline_fptr = reinterpret_cast<hostfxr_initialize_for_dotnet_command_line_fn>(get_export(lib, "hostfxr_initialize_for_dotnet_command_line"));
    auto run_app_fptr = reinterpret_cast<hostfxr_run_app_fn>(get_export(lib, "hostfxr_run_app"));
    auto init_config_fptr = reinterpret_cast<hostfxr_initialize_for_runtime_config_fn>(get_export(lib, "hostfxr_initialize_for_runtime_config"));
    auto get_delegate_fptr = reinterpret_cast<hostfxr_get_runtime_delegate_fn>(get_export(lib, "hostfxr_get_runtime_delegate"));
    auto close_fptr = reinterpret_cast<hostfxr_close_fn>(get_export(lib, "hostfxr_close"));

    if (!init_cmdline_fptr || !run_app_fptr || !init_config_fptr || !get_delegate_fptr || !close_fptr)
    {
        std::cerr << "Failed to get hostfxr functions" << std::endl;
        return -1;
    }

    // Initialize the .NET Core runtime
    hostfxr_initialize_parameters parameters{
        sizeof(hostfxr_initialize_parameters),
        L"./",
        L"./dotnet/Microsoft.NETCore.App/9.0.2"
    };

    hostfxr_handle cxt = nullptr;
    int rc = init_config_fptr(L"myapp.runtimeconfig.json", &parameters, &cxt);
    //int argc = 1;
    //const char_t* argv[] = { L"./publish/myapp.dll" };
    //int rc = init_cmdline_fptr(argc, argv, &parameters, &cxt);
    if (rc != 0 || cxt == nullptr)
    {
        std::cerr << "Failed to initialize .NET Core runtime" << std::endl;
        return -1;
    }

    // Get the delegate for the runtime
    rc = get_delegate_fptr(cxt, hdt_load_assembly_and_get_function_pointer, reinterpret_cast<void**>(&load_assembly_and_get_function_pointer));
    if (rc != 0 || load_assembly_and_get_function_pointer == nullptr)
    {
        std::cerr << "Failed to get load_assembly_and_get_function_pointer" << std::endl;
        return -1;
    }

    //run_app_fptr(cxt);

    // Close the host context
    close_fptr(cxt);

    return 0;
}

// Function to call .NET Core method
int call_dotnet_method()
{
    const char_t* dotnet_assembly_path = L"./publish/MyApp.dll";
    const char_t* dotnet_class_name = L"DotNetLib.Lib, MyApp";
    struct lib_args
    {
        const char_t* message;
        int number;
    };
    // Define the method signature
    typedef void (CORECLR_DELEGATE_CALLTYPE* custom_entry_fn)(lib_args args);
    typedef void (CORECLR_DELEGATE_CALLTYPE* hello2_fn)(const char_t* arg);

    component_entry_point_fn hello = nullptr;
    int rc = load_assembly_and_get_function_pointer(
        dotnet_assembly_path,
        dotnet_class_name,
        L"Hello",
        nullptr, // Delegate type (null for default)
        nullptr,
        (void**)&hello);
    _ASSERT(rc == 0 && hello != nullptr && "Failure: load_assembly_and_get_function_pointer()");

    // Call the .NET Core method
    lib_args args{ L"Hello from C++", -1 };
    hello(&args, sizeof(args));

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
    custom_entry(args);

    // UnmanagedCallersOnly
    rc = load_assembly_and_get_function_pointer(
        dotnet_assembly_path,
        dotnet_class_name,
        L"CustomEntryPointUnmanagedCallersOnly",
        UNMANAGEDCALLERSONLY_METHOD,
        nullptr,
        (void**)&custom_entry);
    _ASSERT(rc == 0 && custom_entry != nullptr && "Failure: load_assembly_and_get_function_pointer()");
    custom_entry(args);

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
    hello2(L"test test test");

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

    MonoDomain* domain;
    domain = mono_jit_init_version("mymono", "v4.0.30319");

    mono_config_parse(nullptr);

    mono_unity_set_embeddinghostname("mymono");

    mono_runtime_unhandled_exception_policy_set(MONO_UNHANDLED_POLICY_LEGACY);

    mono_add_internal_call("Api::GetInfo", reinterpret_cast<void*>(GetInfoApi));
    mono_add_internal_call("Api::LogString", reinterpret_cast<void*>(LogStringApi));

}

bool TestDotnetCore(const std::string& dataPath)
{
    MonoAssembly* assembly;

    const char* dllName = "MyApp.dll";
    const char* ns = "DotNetLib";
    const char* className = "Lib";

    MonoDomain* domain = mono_get_root_domain();
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

    MonoDomain* domain = mono_get_root_domain();
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
        load_hostfxr();
        call_dotnet_method();
    }
    else {
        bool testDotnetCore = false;
        if (testDotnetCore) {
            const char* dataPath = "./publish";
            std::vector<std::string> paths{ dataPath,
                "./MonoBleedingEdge/bin",
                std::string("./MonoBleedingEdge/lib/mono/") + GetMonoClasslibsProfile(),
                "./MonoBleedingEdge/lib"
            };
            LoadAndInitializeMono(paths, "./MonoBleedingEdge/etc", dataPath, "./MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll", 0, nullptr);
            TestDotnetCore(dataPath);
        }
        else {
            const char* dataPath = "./CSharpShaderCompiler";
            std::vector<std::string> paths{ dataPath,
                "./MonoBleedingEdge/bin",
                std::string("./MonoBleedingEdge/lib/mono/") + GetMonoClasslibsProfile(),
                "./MonoBleedingEdge/lib"
            };
            LoadAndInitializeMono(paths, "./MonoBleedingEdge/etc", dataPath, "./MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll", 0, nullptr);
            TestDotnetFramework(dataPath);
        }
        UnloadMono();
    }
}
