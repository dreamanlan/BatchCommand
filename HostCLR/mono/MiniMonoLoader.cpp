#include <string>
#include <vector>
#include <unordered_map>

#include "MiniMonoLoader.h"
#include "MonoIncludes.h"

void* load_library(const char* path);
void* get_export(void* h, const char* name);
void free_library(void* h);
void CleanupMono();

static std::unordered_map<std::string, void*> g_Lib2Addresses{};

static inline void* LoadDynamicLibrary(const std::string& str)
{
    void* ptr = 0;
    auto&& it = g_Lib2Addresses.find(str);
    if (it == g_Lib2Addresses.end()) {
        ptr = load_library(str.c_str());
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
    return get_export(module, str.c_str());
}
static inline void* LoadAndLookupSymbol(const std::string& lib, const std::string& proc)
{
    void* ptr = 0;
    auto&& it = g_Lib2Addresses.find(lib);
    if (it == g_Lib2Addresses.end()) {
        ptr = load_library(lib.c_str());
        if (ptr) {
            g_Lib2Addresses.insert(std::make_pair(lib, ptr));
        }
    }
    else {
        ptr = it->second;
    }
    if (ptr) {
        auto&& p = get_export(ptr, proc.c_str());
        return p;
    }
    return nullptr;
}
static inline void UnloadDynamicLibrary(void* module)
{
    free_library(module);
}
static inline bool UnloadDynamicLibrary(const std::string& str)
{
    bool r = false;
    auto&& it = g_Lib2Addresses.find(str);
    if (it != g_Lib2Addresses.end()) {
        free_library(it->second);
        g_Lib2Addresses.erase(it);
        r = true;
    }
    return r;
}

// define a ProcPtr type for each API
#define DO_API(r, n, p)   typedef r (*fp##n##Type) p;
#if !defined(_MSC_VER) // visual studio supports __declspec(noreturn) only in definition/declaration (not in typedef or function pointer)
    #define DO_API_NO_RETURN(r, n, p)   typedef DOES_NOT_RETURN r (*fp##n##Type) p;
#endif
#include "MonoFunctions.h"

// declare storage for each API's function pointers
#define DO_API(r, n, p)   fp##n##Type n = NULL;
#include "MonoFunctions.h"

static std::string g_MonoModulePath;
static void * g_MonoModule = NULL;

bool LoadMono(const std::string& libraryPath)
{
    g_MonoModule = LoadDynamicLibrary(libraryPath);

    if (!g_MonoModule)
    {
        printf("Unable to load mono library from %s\n", libraryPath.c_str());
        return false;
    }

    g_MonoModulePath = libraryPath;

    // Search for the functions we want by name.
    bool funcsOK = true;
#define DO_API(r, n, p)   n = (fp##n##Type) LookupSymbol(g_MonoModule, #n); if( !n ) { funcsOK = false; printf("mono: function " #n " not found\n"); }
#define DO_API_OPTIONAL(r, n, p) n = (fp##n##Type) LookupSymbol(g_MonoModule, #n);
#include "MonoFunctions.h"

    if (!funcsOK)
    {
        printf("mono: function lookup failed\n");
        UnloadDynamicLibrary(g_MonoModule);
        g_MonoModule = NULL;
        return false;
    }

    return true;
}

void UnloadMono()
{
    CleanupMono();

    // dissociate function pointers
    #define DO_API(r, n, p)   n = NULL;
    #include "MonoFunctions.h"

    if (g_MonoModule)
    {
        UnloadDynamicLibrary(g_MonoModule);
        g_MonoModule = NULL;
        g_MonoModulePath.clear();
    }
}

const char* GetMonoModulePath()
{
    return g_MonoModulePath.c_str();
}


