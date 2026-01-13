#include "HostCLR.h"
#include "processutils.h"
#include "commandworker.h"
#include "installworker.h"
#include "binarysettingswidget.h"
#include "mainwindow.h"

#include <QSettings>

#include <iostream>
#include <string>
#include <vector>

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

void add_log(const QString& msg)
{
    ProcessOutput::instance()->emitOutputLog(msg);
}

void printf_log(const char* fmt, ...)
{
    va_list vl;
    va_start(vl, fmt);
    char buffer[4097];
    int len = vsnprintf(buffer, sizeof(buffer) - 1, fmt, vl);
    buffer[len] = '\0';
    va_end(vl);

    ProcessOutput::instance()->emitOutputLog(buffer);
}

static void convert_separators_to_platform(std::string& pathName)
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

static load_assembly_and_get_function_pointer_fn load_assembly_and_get_function_pointer = nullptr;
// Function to initialize .NET Core runtime
int load_hostfxr()
{
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
        return -1;
    }
    printf("[native] hostfxr path: %s\n", hostfxr_path);
#endif

#ifdef _WIN32
    HMODULE hostfxr_lib = LoadLibraryW(hostfxr_path_w);
    if (!hostfxr_lib) {
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

#ifdef USE_SEPC_DOTNET
    // Initialize the .NET Core runtime
    hostfxr_initialize_parameters parameters{
        sizeof(hostfxr_initialize_parameters),
        L"./",
        L"../dotnet/Microsoft.NETCore.App/9.0.2"
    };

    hostfxr_handle cxt = nullptr;
    int rc = init_config_fptr(L"../managed/dotnetapp.runtimeconfig.json", &parameters, &cxt);
#else
    hostfxr_handle cxt = nullptr;
    int rc = init_config_fptr(L"../managed/dotnetapp.runtimeconfig.json", nullptr, &cxt);
#endif
    //int argc = 1;
    //const char_t* argv[] = { L"../managed/dotnetapp.dll" };
    //int rc = init_cmdline_fptr(argc, argv, &parameters, &cxt);
    if (rc != 0 || cxt == nullptr)
    {
        printf_log("Failed to initialize .NET Core runtime");
        return -4;
    }

    // Get the delegate for the runtime
    rc = get_delegate_fptr(cxt, hdt_load_assembly_and_get_function_pointer, reinterpret_cast<void**>(&load_assembly_and_get_function_pointer));
    if (rc != 0 || load_assembly_and_get_function_pointer == nullptr)
    {
        printf_log("Failed to get load_assembly_and_get_function_pointer");
        return -5;
    }

    //run_app_fptr(cxt);

    // Close the host context
    close_fptr(cxt);

    return 0;
}

// Function pointers to call dotnet methods
init_csharp_fn init_csharp_fptr = nullptr;
execute_dsl_fn execute_dsl_fptr = nullptr;
load_setting_fn load_setting_fptr = nullptr;
load_scheme_menu_fn load_scheme_menu_fptr = nullptr;
load_scheme_fn load_scheme_fptr = nullptr;
execute_command_fn execute_command_fptr = nullptr;
run_prog_fn run_prog_fptr = nullptr;
build_fn build_fptr = nullptr;
install_fn install_fptr = nullptr;

// Native api
typedef void (*host_output_log_fn)(const char* msg);
typedef void (*host_show_progress_fn)(int percent, const char* msg);
typedef bool (*host_run_command_fn)(const char* cmd, const char* args, void* result);
typedef bool (*host_run_command_timeout_fn)(const char* cmd, const char* args, int timeout, void* result);
typedef int (*host_get_result_code_fn)(void* result);
typedef int (*host_get_error_count_fn)(void* result);
typedef int (*host_get_output_count_fn)(void* result);
typedef bool (*host_get_error_fn)(int index, char* path, int& path_size, void* result);
typedef bool (*host_get_output_fn)(int index, char* path, int& path_size, void* result);
typedef bool (*host_find_in_path_fn)(const char* filename, char* path, int& path_size);
typedef bool (*host_get_dsl_script_fn)(char* path, int& path_size);
typedef bool (*host_get_adb_exe_fn)(char* path, int& path_size);
typedef bool (*host_get_jadx_exe_fn)(char* path, int& path_size);
typedef bool (*host_get_java_exe_fn)(char* path, int& path_size);
typedef bool (*host_get_zipalign_exe_fn)(char* path, int& path_size);
typedef int (*host_get_java_heap_fn)();
typedef bool (*host_get_setting_path_fn)(const char* name, char* path, int& path_size);
typedef int (*host_get_setting_int_fn)(const char* name, int def_val);
typedef float (*host_get_setting_float_fn)(const char* name, float def_val);
typedef double (*host_get_setting_double_fn)(const char* name, double def_val);
typedef bool (*host_get_setting_string_fn)(const char* name, char* str, int& str_size);
typedef bool (*host_set_setting_int_fn)(const char* name, int val);
typedef bool (*host_set_setting_float_fn)(const char* name, float val);
typedef bool (*host_set_setting_double_fn)(const char* name, double val);
typedef bool (*host_set_setting_string_fn)(const char* name, const char* val);
//0--file 1--int 2--float 3--double 4--string
typedef bool (*host_add_setting_item_fn)(const char* name, const char* label, const char* tooltip, int type, const char* default_value, const char* ext, const char* link);
typedef bool (*host_add_scheme_menu_fn)(const char* path, const char* tooltip);
typedef bool (*host_add_button_fn)(const char* label, const char* tooltip, const char* cmd_type, const char* cmd_args);
typedef bool (*host_add_input_fn)(const char* label, const char* tooltip, const char* def_val, const char* cmd_type, const char* cmd_args);
typedef bool (*host_clear_console_fn)();
typedef bool (*host_show_windows_console_fn)();
typedef bool (*host_hide_windows_console_fn)();

typedef struct {
    host_output_log_fn OutputLog;
    host_show_progress_fn ShowProgress;
    host_run_command_fn RunCommand;
    host_run_command_timeout_fn RunCommandTimeout;
    host_get_result_code_fn GetResultCode;
    host_get_error_count_fn GetErrorCount;
    host_get_output_count_fn GetOutputCount;
    host_get_error_fn GetError;
    host_get_output_fn GetOutput;
    host_find_in_path_fn FindInPath;
    host_get_dsl_script_fn GetDslScript;
    host_get_adb_exe_fn GetAdbExe;
    host_get_jadx_exe_fn GetJadxExe;
    host_get_java_exe_fn GetJavaExe;
    host_get_zipalign_exe_fn GetZipAlignExe;
    host_get_java_heap_fn GetJavaHeap;
    host_get_setting_path_fn GetSettingPath;
    host_get_setting_int_fn GetSettingInt;
    host_get_setting_float_fn GetSettingFloat;
    host_get_setting_double_fn GetSettingDouble;
    host_get_setting_string_fn GetSettingString;
    host_set_setting_int_fn SetSettingInt;
    host_set_setting_float_fn SetSettingFloat;
    host_set_setting_double_fn SetSettingDouble;
    host_set_setting_string_fn SetSettingString;
    host_add_setting_item_fn AddSettingItem;
    host_add_scheme_menu_fn AddSchemeMenu;
    host_add_button_fn AddButton;
    host_add_input_fn AddInput;
    host_clear_console_fn ClearConsole;
    host_show_windows_console_fn ShowWindowsConsole;
    host_hide_windows_console_fn HideWindowsConsole;
} HostApi;

void host_output_log(const char* msg)
{
    ProcessOutput::instance()->emitOutputLog(QString::fromUtf8(msg));
}
void host_show_progress(int percent, const char* msg)
{
    ProcessOutput::instance()->emitProgress(percent, QString::fromUtf8(msg));
}
bool host_run_command(const char* cmd, const char* args, void* result)
{
    if (!cmd || !args || !result) {
        return false;
    }
    ProcessResult* res = reinterpret_cast<ProcessResult*>(result);
    *res = ProcessUtils::runCommand(QString::fromUtf8(cmd), QString::fromUtf8(args).split(' '));
    return res->code == 0;
}
bool host_run_command_timeout(const char* cmd, const char* args, int timeout, void* result)
{
    if (!cmd || !args || !result) {
        return false;
    }
    ProcessResult* res = reinterpret_cast<ProcessResult*>(result);
    *res = ProcessUtils::runCommand(QString::fromUtf8(cmd), QString::fromUtf8(args).split(' '), timeout);
    return res->code == 0;
}
int host_get_result_code(void* result)
{
    if (!result) {
        return -1;
    }
    ProcessResult* res = reinterpret_cast<ProcessResult*>(result);
    return res->code;
}
int host_get_error_count(void* result)
{
    if (!result) {
        return 0;
    }
    ProcessResult* res = reinterpret_cast<ProcessResult*>(result);
    return res->error.size();
}
int host_get_output_count(void* result)
{
    if (!result) {
        return 0;
    }
    ProcessResult* res = reinterpret_cast<ProcessResult*>(result);
    return res->output.size();
}
bool host_get_error(int index, char* path, int& path_size, void* result)
{
    if (!path || !result) {
        return false;
    }
    ProcessResult* res = reinterpret_cast<ProcessResult*>(result);
    if (index < 0 || index >= res->error.size()) {
        path[0] = '\0';
        path_size = 0;
        return false;
    }
    std::string res_str = res->error[index].toStdString();
    size_t len = res_str.size();
    if (len > path_size) {
        len = path_size - 1;
    }
    memcpy(path, res_str.c_str(), len);
    path[len] = '\0';
    path_size = static_cast<int>(res_str.size());
    return true;
}
bool host_get_output(int index, char* path, int& path_size, void* result)
{
    if (!path || !result) {
        return false;
    }
    ProcessResult* res = reinterpret_cast<ProcessResult*>(result);
    if (index < 0 || index >= res->output.size()) {
        path[0] = '\0';
        path_size = 0;
        return false;
    }
    std::string res_str = res->output[index].toStdString();
    size_t len = res_str.size();
    if (len > path_size) {
        len = path_size - 1;
    }
    memcpy(path, res_str.c_str(), len);
    path[len] = '\0';
    path_size = static_cast<int>(res_str.size());
    return true;
}
bool host_find_in_path(const char* filename, char* path, int& path_size)
{
    if (!filename || !path) {
        return false;
    }
    QString res = ProcessUtils::findInPath(QString::fromUtf8(filename));
    if (res.isEmpty()) {
        path[0] = '\0';
        path_size = 0;
        return false;
    }
    std::string res_str = res.toStdString();
    convert_separators_to_platform(res_str);
    size_t len = res_str.size();
    if (len > path_size) {
        len = path_size - 1;
    }
    memcpy(path, res_str.c_str(), len);
    path[len] = '\0';
    path_size = static_cast<int>(res_str.size());
    return true;
}
bool host_get_dsl_script(char* path, int& path_size)
{
    if (!path) {
        return false;
    }
    QString res = ProcessUtils::dslScript();
    if (res.isEmpty()) {
        path[0] = '\0';
        path_size = 0;
        return false;
    }
    std::string res_str = res.toStdString();
    convert_separators_to_platform(res_str);
    size_t len = res_str.size();
    if (len > path_size) {
        len = path_size - 1;
    }
    memcpy(path, res_str.c_str(), len);
    path[len] = '\0';
    path_size = static_cast<int>(res_str.size());
    return true;
}
bool host_get_adb_exe(char* path, int& path_size)
{
    if (!path) {
        return false;
    }
    QString res = ProcessUtils::adbExe();
    if (res.isEmpty()) {
        path[0] = '\0';
        path_size = 0;
        return false;
    }
    std::string res_str = res.toStdString();
    convert_separators_to_platform(res_str);
    size_t len = res_str.size();
    if (len > path_size) {
        len = path_size - 1;
    }
    memcpy(path, res_str.c_str(), len);
    path[len] = '\0';
    path_size = static_cast<int>(res_str.size());
    return true;
}
bool host_get_jadx_exe(char* path, int& path_size)
{
    if (!path) {
        return false;
    }
    QString res = ProcessUtils::jadxExe();
    if (res.isEmpty()) {
        path[0] = '\0';
        path_size = 0;
        return false;
    }
    std::string res_str = res.toStdString();
    convert_separators_to_platform(res_str);
    size_t len = res_str.size();
    if (len > path_size) {
        len = path_size - 1;
    }
    memcpy(path, res_str.c_str(), len);
    path[len] = '\0';
    path_size = static_cast<int>(res_str.size());
    return true;
}
bool host_get_java_exe(char* path, int& path_size)
{
    if (!path) {
        return false;
    }
    QString res = ProcessUtils::javaExe();
    if (res.isEmpty()) {
        path[0] = '\0';
        path_size = 0;
        return false;
    }
    std::string res_str = res.toStdString();
    convert_separators_to_platform(res_str);
    size_t len = res_str.size();
    if (len > path_size) {
        len = path_size - 1;
    }
    memcpy(path, res_str.c_str(), len);
    path[len] = '\0';
    path_size = static_cast<int>(res_str.size());
    return true;
}
bool host_get_zipalign_exe(char* path, int& path_size)
{
    if (!path) {
        return false;
    }
    QString res = ProcessUtils::zipalignExe();
    if (res.isEmpty()) {
        path[0] = '\0';
        path_size = 0;
        return false;
    }
    std::string res_str = res.toStdString();
    convert_separators_to_platform(res_str);
    size_t len = res_str.size();
    if (len > path_size) {
        len = path_size - 1;
    }
    memcpy(path, res_str.c_str(), len);
    path[len] = '\0';
    path_size = static_cast<int>(res_str.size());
    return true;
}
int host_get_java_heap()
{
    return ProcessUtils::javaHeapSize();
}
bool host_get_setting_path(const char* name, char* path, int& path_size)
{
    if (!name || !path) {
        return false;
    }
    QSettings settings;
    QString res = settings.value(QString::fromUtf8(name), QVariant(QString::fromUtf8(path))).toString();
    if (res.isEmpty()) {
        path[0] = '\0';
        path_size = 0;
        return false;
    }
    std::string res_str = res.toStdString();
    convert_separators_to_platform(res_str);
    size_t len = res_str.size();
    if (len > path_size) {
        len = path_size - 1;
    }
    memcpy(path, res_str.c_str(), len);
    path[len] = '\0';
    path_size = static_cast<int>(res_str.size());
    return true;
}
int host_get_setting_int(const char* name, int def_val)
{
    if (!name) {
        return 0;
    }
    QSettings settings;
    return settings.value(QString::fromUtf8(name), def_val).toInt();
}
float host_get_setting_float(const char* name, float def_val)
{
    if (!name) {
        return 0;
    }
    QSettings settings;
    return settings.value(QString::fromUtf8(name), def_val).toFloat();
}
double host_get_setting_double(const char* name, double def_val)
{
    if (!name) {
        return 0;
    }
    QSettings settings;
    return settings.value(QString::fromUtf8(name), def_val).toDouble();
}
bool host_get_setting_string(const char* name, char* str, int& str_size)
{
    if (!name || !str) {
        return false;
    }
    QSettings settings;
    QString res = settings.value(QString::fromUtf8(name), QVariant(QString::fromUtf8(str))).toString();
    if (res.isEmpty()) {
        str[0] = '\0';
        str_size = 0;
        return false;
    }
    std::string res_str = res.toStdString();
    size_t len = res_str.size();
    if (len > str_size) {
        len = str_size - 1;
    }
    memcpy(str, res_str.c_str(), len);
    str[len] = '\0';
    str_size = static_cast<int>(res_str.size());
    return true;
}
bool host_set_setting_int(const char* name, int value)
{
    if (!name) {
        return false;
    }
    QSettings settings;
    settings.setValue(QString::fromUtf8(name), value);
    settings.sync();
    return true;
}
bool host_set_setting_float(const char* name, float value)
{
    if (!name) {
        return false;
    }
    QSettings settings;
    settings.setValue(QString::fromUtf8(name), value);
    settings.sync();
    return true;
}
bool host_set_setting_double(const char* name, double value)
{
    if (!name) {
        return false;
    }
    QSettings settings;
    settings.setValue(QString::fromUtf8(name), value);
    settings.sync();
    return true;
}
bool host_set_setting_string(const char* name, const char* value)
{
    if (!name || !value) {
        return false;
    }
    QSettings settings;
    settings.setValue(QString::fromUtf8(name), QString::fromUtf8(value));
    settings.sync();
    return true;
}
bool host_add_setting_item(const char* name, const char* label, const char* tooltip, int type, const char* default_value, const char* ext, const char* link)
{
    if (!name || !label || !tooltip || !default_value || !ext || !link) {
        return false;
    }
    BinarySettingsWidget* pWidget = BinarySettingsWidget::instance();
    if (!pWidget) {
        return false;
    }

    QMetaObject::invokeMethod(pWidget, &BinarySettingsWidget::addSettingItem, Qt::QueuedConnection, QString::fromUtf8(name), QString::fromUtf8(label), QString::fromUtf8(tooltip), type, QString::fromUtf8(default_value), QString::fromUtf8(ext), QString::fromUtf8(link));
    return true;
}
bool host_add_scheme_menu(const char* path, const char* tooltip)
{
    if (!path || !tooltip) {
        return false;
    }
    MainWindow* pWin = MainWindow::instance();
    if (!pWin) {
        return false;
    }

    QMetaObject::invokeMethod(pWin, &MainWindow::addSchemeMenu, Qt::QueuedConnection, QString::fromUtf8(path), QString::fromUtf8(tooltip));
    return true;
}
bool host_add_button(const char* label, const char* tooltip, const char* cmd_type, const char* cmd_args)
{
    if (!label || !tooltip || !cmd_type || !cmd_args) {
        return false;
    }
    MainWindow* pWin = MainWindow::instance();
    if (!pWin) {
        return false;
    }

    QMetaObject::invokeMethod(pWin, &MainWindow::addButton, Qt::QueuedConnection, QString::fromUtf8(label), QString::fromUtf8(tooltip), QString::fromUtf8(cmd_type), QString::fromUtf8(cmd_args));
    return true;
}
bool host_add_input(const char* label, const char* tooltip, const char* def_val, const char* cmd_type, const char* cmd_args)
{
    if (!label || !tooltip || !def_val || !cmd_type || !cmd_args) {
        return false;
    }
    MainWindow* pWin = MainWindow::instance();
    if (!pWin) {
        return false;
    }

    QMetaObject::invokeMethod(pWin, &MainWindow::addInput, Qt::QueuedConnection, QString::fromUtf8(label), QString::fromUtf8(tooltip), QString::fromUtf8(def_val), QString::fromUtf8(cmd_type), QString::fromUtf8(cmd_args));
    return true;
}
bool host_clear_console()
{
    MainWindow* pWin = MainWindow::instance();
    if (!pWin) {
        return false;
    }

    QMetaObject::invokeMethod(pWin, &MainWindow::clearConsole, Qt::QueuedConnection);
    return true;
}
bool host_show_windows_console()
{
    MainWindow* pWin = MainWindow::instance();
    if (!pWin) {
        return false;
    }

    QMetaObject::invokeMethod(pWin, &MainWindow::showWindowsConsole, Qt::QueuedConnection);
    return true;
}
bool host_hide_windows_console()
{
    MainWindow* pWin = MainWindow::instance();
    if (!pWin) {
        return false;
    }

    QMetaObject::invokeMethod(pWin, &MainWindow::hideWindowsConsole, Qt::QueuedConnection);
    return true;
}

// Function to call .NET Core method
int load_dotnet_method()
{
    const char_t* dotnet_assembly_path = L"../managed/DotnetApp.dll";
    const char_t* dotnet_class_name = L"DotNetLib.Lib, DotnetApp";

    // native api
    HostApi api;
    api.OutputLog = &host_output_log;
    api.ShowProgress = &host_show_progress;
    api.RunCommand = &host_run_command;
    api.RunCommandTimeout = &host_run_command_timeout;
    api.GetResultCode = &host_get_result_code;
    api.GetErrorCount = &host_get_error_count;
    api.GetOutputCount = &host_get_output_count;
    api.GetError = &host_get_error;
    api.GetOutput = &host_get_output;
    api.FindInPath = &host_find_in_path;
    api.GetDslScript = &host_get_dsl_script;
    api.GetAdbExe = &host_get_adb_exe;
    api.GetJadxExe = &host_get_jadx_exe;
    api.GetJavaExe = &host_get_java_exe;
    api.GetZipAlignExe = &host_get_zipalign_exe;
    api.GetJavaHeap = &host_get_java_heap;
    api.GetSettingPath = &host_get_setting_path;
    api.GetSettingInt = &host_get_setting_int;
    api.GetSettingFloat = &host_get_setting_float;
    api.GetSettingDouble = &host_get_setting_double;
    api.GetSettingString = &host_get_setting_string;
    api.SetSettingInt = &host_set_setting_int;
    api.SetSettingFloat = &host_set_setting_float;
    api.SetSettingDouble = &host_set_setting_double;
    api.SetSettingString = &host_set_setting_string;
    api.AddSettingItem = &host_add_setting_item;
    api.AddSchemeMenu = &host_add_scheme_menu;
    api.AddButton = &host_add_button;
    api.AddInput = &host_add_input;
    api.ClearConsole = &host_clear_console;
    api.ShowWindowsConsole = &host_show_windows_console;
    api.HideWindowsConsole = &host_hide_windows_console;

    // For UNMANAGEDCALLERSONLY_METHOD, this must be int (or other directly copyable type), not bool.
    typedef int (CORECLR_DELEGATE_CALLTYPE* register_api_fn)(void* arg);
    register_api_fn register_api = nullptr;
    int rc = load_assembly_and_get_function_pointer(
        dotnet_assembly_path,
        dotnet_class_name,
        L"RegisterApi",
        UNMANAGEDCALLERSONLY_METHOD,
        nullptr,
        (void**)&register_api);
    if (rc || !register_api) {
        printf_log("Failure: load register_api");
    }

    if (register_api) {
        int result = register_api(&api);
        printf_log("register_api returned: %d", result);
    }

    // dotnet methods
    rc = load_assembly_and_get_function_pointer(
    dotnet_assembly_path,
    dotnet_class_name,
    L"Init",
    L"DotNetLib.Lib+InitDelegation, DotnetApp", // Delegate type
    nullptr,
    (void**)&init_csharp_fptr);
    if (rc || !init_csharp_fptr) {
        printf_log("Failure: load init_csharp");
    }

    rc = load_assembly_and_get_function_pointer(
    dotnet_assembly_path,
    dotnet_class_name,
    L"ExecuteDsl",
    L"DotNetLib.Lib+ExecuteDslDelegation, DotnetApp", // Delegate type
    nullptr,
    (void**)&execute_dsl_fptr);
    if (rc || !execute_dsl_fptr) {
        printf_log("Failure: load execute_dsl");
    }

    rc = load_assembly_and_get_function_pointer(
    dotnet_assembly_path,
    dotnet_class_name,
    L"LoadSetting",
    L"DotNetLib.Lib+LoadSettingDelegation, DotnetApp", // Delegate type
    nullptr,
    (void**)&load_setting_fptr);
    if (rc || !load_setting_fptr) {
        printf_log("Failure: load load_setting");
    }

    rc = load_assembly_and_get_function_pointer(
    dotnet_assembly_path,
    dotnet_class_name,
    L"LoadSchemeMenu",
    L"DotNetLib.Lib+LoadSchemeMenuDelegation, DotnetApp", // Delegate type
    nullptr,
    (void**)&load_scheme_menu_fptr);
    if (rc || !load_scheme_menu_fptr) {
        printf_log("Failure: load load_scheme_menu");
    }

    rc = load_assembly_and_get_function_pointer(
    dotnet_assembly_path,
    dotnet_class_name,
    L"LoadScheme",
    L"DotNetLib.Lib+LoadSchemeDelegation, DotnetApp", // Delegate type
    nullptr,
    (void**)&load_scheme_fptr);
    if (rc || !load_scheme_fptr) {
        printf_log("Failure: load load_scheme");
    }

    rc = load_assembly_and_get_function_pointer(
    dotnet_assembly_path,
    dotnet_class_name,
    L"ExecuteCommand",
    L"DotNetLib.Lib+ExecuteCommandDelegation, DotnetApp", // Delegate type
    nullptr,
    (void**)&execute_command_fptr);
    if (rc || !execute_command_fptr) {
        printf_log("Failure: load execute_command");
    }

    rc = load_assembly_and_get_function_pointer(
    dotnet_assembly_path,
    dotnet_class_name,
    L"RunProg",
    L"DotNetLib.Lib+RunProgDelegation, DotnetApp", // Delegate type
    nullptr,
    (void**)&run_prog_fptr);
    if (rc || !run_prog_fptr) {
        printf_log("Failure: load run_prog");
    }

    rc = load_assembly_and_get_function_pointer(
    dotnet_assembly_path,
    dotnet_class_name,
    L"Build",
    L"DotNetLib.Lib+BuildDelegation, DotnetApp", // Delegate type
    nullptr,
    (void**)&build_fptr);
    if (rc || !build_fptr) {
        printf_log("Failure: load build");
    }

    rc = load_assembly_and_get_function_pointer(
    dotnet_assembly_path,
    dotnet_class_name,
    L"Install",
    L"DotNetLib.Lib+InstallDelegation, DotnetApp", // Delegate type
    nullptr,
    (void**)&install_fptr);
    if (rc || !install_fptr) {
        printf_log("Failure: load install");
    }

    return 0;
}
