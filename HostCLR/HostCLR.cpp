// HostCLR.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
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

int main()
{
    std::cout << "Hello World!\n";
    load_hostfxr();
    call_dotnet_method();
}
