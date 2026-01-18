#include <iostream>
#include <string>
#include <vector>
#include "path_utils.h"

#if defined(_MSC_VER)
#include "windows.h"
// Ensure NTSTATUS and other NT types are available
#ifndef NTSTATUS
typedef LONG NTSTATUS;
#endif
typedef struct _UNICODE_STRING {
    USHORT Length;
    USHORT MaximumLength;
    PWSTR  Buffer;
} UNICODE_STRING, * PUNICODE_STRING;
typedef struct _MY_PROCESS_BASIC_INFORMATION {
    PVOID Reserved1;
    PVOID PebBaseAddress;
    PVOID Reserved2[2];
    ULONG_PTR UniqueProcessId;
    PVOID Reserved3;
} MY_PROCESS_BASIC_INFORMATION, * PMY_PROCESS_BASIC_INFORMATION;

// Helper function to check if memory is readable before ReadProcessMemory
bool IsMemoryReadable(HANDLE process, LPCVOID address) {
    MEMORY_BASIC_INFORMATION mbi;
    SIZE_T result = VirtualQueryEx(process, address, &mbi, sizeof(mbi));
    if (result == 0) {
        return false;
    }
    // Check if memory is committed and has read access
    return (mbi.State == MEM_COMMIT) &&
        (mbi.Protect & (PAGE_READONLY | PAGE_READWRITE | PAGE_EXECUTE_READ | PAGE_EXECUTE_READWRITE));
}

#include "coreclr/nethost.h"
#include "coreclr/coreclr_delegates.h"
#include "coreclr/hostfxr.h"

#pragma comment(lib, "coreclr/nethost.lib")

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

// Cross-platform character set conversion functions
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

enum LogSeverity {
    LOG_SEVERITY_ERROR,
    LOG_SEVERITY_WARNING,
    LOG_SEVERITY_INFO
};

void printf_log(LogSeverity severity, const char* fmt, ...)
{
    va_list vl;
    va_start(vl, fmt);
    char buffer[4097];
    int len = vsnprintf(buffer, sizeof(buffer) - 1, fmt, vl);
    buffer[len] = '\0';
    va_end(vl);

    if (severity == LOG_SEVERITY_ERROR) {
        printf("%s\n", buffer);
    }
    else {
        printf("%s\n", buffer);
    }

#if defined(_MSC_VER)
    // Convert UTF-8 to wide char for OutputDebugString
    std::wstring wbuffer = Utf8ToWstring(buffer);
    ::OutputDebugStringW(wbuffer.c_str());
#endif
}

[[maybe_unused]] static void convert_separators_to_platform(std::string& pathName)
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

#if defined(_MSC_VER)
#include <tlhelp32.h>
#include <psapi.h>
#pragma comment(lib, "psapi.lib")

#pragma pack(push, 8)
// Windows structures for reading command line
typedef NTSTATUS(NTAPI* PFN_NtQueryInformationProcess)(
    HANDLE ProcessHandle,
    DWORD ProcessInformationClass,
    PVOID ProcessInformation,
    DWORD ProcessInformationLength,
    PDWORD ReturnLength);

typedef struct _MY_RTL_USER_PROCESS_PARAMETERS {
    BYTE Reserved1[16];
    PVOID Reserved2[10];
    UNICODE_STRING ImagePathName;
    UNICODE_STRING CommandLine;
} MY_RTL_USER_PROCESS_PARAMETERS, * PMY_RTL_USER_PROCESS_PARAMETERS;

typedef struct _MY_PEB {
    BYTE Reserved1[2];
    BYTE BeingDebugged;
    BYTE Reserved2[1];
    PVOID Reserved3[2];
    PVOID Ldr;
    PMY_RTL_USER_PROCESS_PARAMETERS ProcessParameters;
    BYTE Reserved4[104];
    PVOID Reserved5[52];
    PVOID PostProcessInitRoutine;
    BYTE Reserved6[128];
    PVOID Reserved7[1];
    ULONG SessionId;
} MY_PEB, * PMY_PEB;
#pragma pack(pop)

#define ProcessBasicInformation 0

#elif defined(__linux__)
#include <dirent.h>
#include <unistd.h>
#include <sys/types.h>
#elif defined(__APPLE__)
#include <libproc.h>
#include <sys/sysctl.h>
#endif

int TerminateMonitoredProcess(const char* cmd_line_key) {
    int terminated_count = 0;
#if defined(_MSC_VER)
    // Windows implementation
    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (snapshot == INVALID_HANDLE_VALUE) {
        printf_log(LOG_SEVERITY_ERROR, "TerminateMonitoredProcess: Failed to create process snapshot");
        return -1;
    }

    PROCESSENTRY32 pe32;
    pe32.dwSize = sizeof(PROCESSENTRY32);

    if (!Process32First(snapshot, &pe32)) {
        printf_log(LOG_SEVERITY_ERROR, "TerminateMonitoredProcess: Failed to get first process");
        CloseHandle(snapshot);
        return -1;
    }

    DWORD current_pid = GetCurrentProcessId();

    // Dynamically load NtQueryInformationProcess for reading command line
    HMODULE ntdll = GetModuleHandleW(L"ntdll.dll");
    PFN_NtQueryInformationProcess pNtQueryInformationProcess = nullptr;
    if (ntdll) {
        pNtQueryInformationProcess = reinterpret_cast<PFN_NtQueryInformationProcess>(
            GetProcAddress(ntdll, "NtQueryInformationProcess"));
    }

    // Loop through processes
    do {
        // Skip current process
        if (pe32.th32ProcessID == current_pid) {
            continue;
        }

        // Open process with required access rights
        HANDLE process = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ | PROCESS_TERMINATE,
            FALSE, pe32.th32ProcessID);
        if (!process) {
            continue;
        }

        // Try to read command line from PEB
        bool is_target = false;

        if (pNtQueryInformationProcess) {
            MY_PROCESS_BASIC_INFORMATION pbi;
            DWORD return_length = 0;

            // Query process basic information to get PEB address
            NTSTATUS status = pNtQueryInformationProcess(process, ProcessBasicInformation,
                &pbi, sizeof(pbi), &return_length);

            if (status >= 0 && pbi.PebBaseAddress) {
                // Check if PEB memory is readable before reading
                if (!IsMemoryReadable(process, pbi.PebBaseAddress)) {
                    printf_log(LOG_SEVERITY_WARNING,
                        "TerminateMonitoredProcess: PEB memory not readable for PID=%d",
                        pe32.th32ProcessID);
                    CloseHandle(process);
                    continue;
                }

                // Read PEB from target process
                MY_PEB peb;
                SIZE_T bytes_read = 0;

                if (ReadProcessMemory(process, pbi.PebBaseAddress, &peb, sizeof(peb), &bytes_read)) {
                    if (peb.ProcessParameters) {
                        // Check if ProcessParameters memory is readable before reading
                        if (!IsMemoryReadable(process, peb.ProcessParameters)) {
                            printf_log(LOG_SEVERITY_WARNING,
                                "TerminateMonitoredProcess: ProcessParameters memory not readable for PID=%d",
                                pe32.th32ProcessID);
                            CloseHandle(process);
                            continue;
                        }

                        // Read process parameters
                        MY_RTL_USER_PROCESS_PARAMETERS params;
                        if (ReadProcessMemory(process, peb.ProcessParameters, &params,
                            sizeof(params), &bytes_read)) {
                            // Check if CommandLine.Buffer memory is readable before reading
                            if (!IsMemoryReadable(process, params.CommandLine.Buffer)) {
                                printf_log(LOG_SEVERITY_WARNING,
                                    "TerminateMonitoredProcess: CommandLine.Buffer memory not readable for PID=%d",
                                    pe32.th32ProcessID);
                                CloseHandle(process);
                                continue;
                            }

                            // Read command line string
                            WCHAR* cmd_line = (WCHAR*)malloc(params.CommandLine.MaximumLength + sizeof(WCHAR));
                            if (cmd_line) {
                                if (ReadProcessMemory(process, params.CommandLine.Buffer,
                                    cmd_line, params.CommandLine.Length, &bytes_read)) {
                                    // Null-terminate the string
                                    cmd_line[params.CommandLine.Length / sizeof(WCHAR)] = L'\0';

                                    // Convert to narrow string for easier comparison
                                    int cmd_line_len = WideCharToMultiByte(CP_UTF8, 0, cmd_line, -1,
                                        NULL, 0, NULL, NULL);
                                    if (cmd_line_len > 0) {
                                        char* cmd_line_utf8 = (char*)malloc(cmd_line_len);
                                        if (cmd_line_utf8) {
                                            WideCharToMultiByte(CP_UTF8, 0, cmd_line, -1,
                                                cmd_line_utf8, cmd_line_len, NULL, NULL);

                                            // Check if command line contains --type=monitored
                                            if (strstr(cmd_line_utf8, cmd_line_key)) {
                                                is_target = true;
                                                printf_log(LOG_SEVERITY_INFO,
                                                    "TerminateMonitoredProcess: Found monitored process PID=%d",
                                                    pe32.th32ProcessID);
                                            }

                                            free(cmd_line_utf8);
                                        }
                                    }
                                }
                                free(cmd_line);
                            }
                        }
                    }
                }
            }
        }

        // Terminate monitored process
        if (is_target) {
            if (TerminateProcess(process, 0)) {
                terminated_count++;
                printf_log(LOG_SEVERITY_INFO,
                    "TerminateMonitoredProcess: Terminated monitored process PID=%d",
                    pe32.th32ProcessID);
            }
            else {
                printf_log(LOG_SEVERITY_WARNING,
                    "TerminateMonitoredProcess: Failed to terminate PID=%d, error=%lu",
                    pe32.th32ProcessID, GetLastError());
            }
        }

        CloseHandle(process);

    }
    while (Process32Next(snapshot, &pe32));

    CloseHandle(snapshot);

#elif defined(__linux__)
    // Linux implementation
    DIR* proc_dir = opendir("/proc");
    if (!proc_dir) {
        printf_log(LOG_SEVERITY_ERROR, "TerminateMonitoredProcess: Failed to open /proc");
        return -1;
    }

    pid_t current_pid = getpid();

    struct dirent* entry;
    while ((entry = readdir(proc_dir)) != NULL) {
        // Skip non-numeric directories
        if (!isdigit(entry->d_name[0])) {
            continue;
        }

        pid_t pid = atoi(entry->d_name);

        // Skip current process
        if (pid == current_pid) {
            continue;
        }

        // Check if it's a child process of current process
        char status_path[256];
        snprintf(status_path, sizeof(status_path), "/proc/%d/status", pid);

        FILE* status_file = fopen(status_path, "r");
        if (!status_file) {
            continue;
        }

        // Read status to find PPid
        char status_line[256];
        pid_t parent_pid = 0;
        while (fgets(status_line, sizeof(status_line), status_file)) {
            if (strncmp(status_line, "PPid:", 5) == 0) {
                sscanf(status_line, "PPid:\t%d", &parent_pid);
                break;
            }
        }
        fclose(status_file);

        // Only process child processes of current process
        if (parent_pid != current_pid) {
            continue;
        }

        // Read cmdline
        char cmdline_path[256];
        snprintf(cmdline_path, sizeof(cmdline_path), "/proc/%d/cmdline", pid);

        FILE* cmdline_file = fopen(cmdline_path, "r");
        if (!cmdline_file) {
            continue;
        }

        char cmdline[8192];
        size_t bytes_read = fread(cmdline, 1, sizeof(cmdline) - 1, cmdline_file);
        cmdline[bytes_read] = '\0';
        fclose(cmdline_file);

        // Check if it's a monitored process
        if (strstr(cmdline, cmd_line_key)) {
            printf_log(LOG_SEVERITY_INFO, "TerminateMonitoredProcess: Found monitored process PID=%d", pid);

            // Terminate the process
            if (kill(pid, SIGTERM) == 0) {
                terminated_count++;
                printf_log(LOG_SEVERITY_INFO, "TerminateMonitoredProcess: Terminated monitored process PID=%d", pid);
            }
            else {
                printf_log(LOG_SEVERITY_WARNING, "TerminateMonitoredProcess: Failed to terminate PID=%d", pid);
            }
        }
    }

    closedir(proc_dir);

#elif defined(__APPLE__)
    // macOS implementation
    pid_t current_pid = getpid();
    int num_pids = proc_listpids(PROC_ALL_PIDS, 0, NULL, 0);

    if (num_pids <= 0) {
        printf_log(LOG_SEVERITY_ERROR, "TerminateMonitoredProcess: Failed to get process list");
        return -1;
    }

    std::vector<pid_t> pids(num_pids);
    proc_listpids(PROC_ALL_PIDS, 0, pids.data(), sizeof(pid_t) * num_pids);

    for (int i = 0; i < num_pids; i++) {
        if (pids[i] <= 0 || pids[i] == current_pid) {
            continue;
        }

        // Get process information to check parent PID
        struct kinfo_proc proc_info;
        size_t proc_info_size = sizeof(proc_info);
        int mib[4] = { CTL_KERN, KERN_PROC, KERN_PROC_PID, pids[i] };

        if (sysctl(mib, 4, &proc_info, &proc_info_size, NULL, 0) != 0) {
            continue;
        }

        // Only process child processes of current process
        if (proc_info.kp_eproc.e_ppid != current_pid) {
            continue;
        }

        // Get process arguments
        char args_buffer[MAXPATHLEN * 4];
        int mib[3] = { CTL_KERN, KERN_PROCARGS2, pids[i] };
        size_t args_size = sizeof(args_buffer);

        if (sysctl(mib, 3, args_buffer, &args_size, NULL, 0) != 0) {
            continue;
        }

        // Parse arguments
        int argc;
        memcpy(&argc, args_buffer, sizeof(argc));

        char* args_end = args_buffer + args_size;
        char* exe_path = args_buffer + sizeof(argc);
        char* current_arg = exe_path + strlen(exe_path) + 1;

        // Skip to first argument
        while (current_arg < args_end && *current_arg == '\0') {
            current_arg++;
        }

        // Check all arguments for --type=monitored
        bool is_target = false;
        char* arg = current_arg;
        for (int j = 0; j < argc && arg < args_end; j++) {
            if (strstr(arg, cmd_line_key)) {
                is_target = true;
                break;
            }

            // Move to next argument
            arg += strlen(arg) + 1;
            while (arg < args_end && *arg == '\0') {
                arg++;
            }
        }

        if (is_target) {
            printf_log(LOG_SEVERITY_INFO, "TerminateMonitoredProcess: Found monitored process PID=%d", pids[i]);

            // Terminate the process
            if (kill(pids[i], SIGTERM) == 0) {
                terminated_count++;
                printf_log(LOG_SEVERITY_INFO, "TerminateMonitoredProcess: Terminated monitored process PID=%d", pids[i]);
            }
            else {
                printf_log(LOG_SEVERITY_WARNING, "TerminateMonitoredProcess: Failed to terminate PID=%d", pids[i]);
            }
        }
    }
#else
    printf_log(LOG_SEVERITY_ERROR, "TerminateMonitoredProcess: Not implemented for this platform");
    return -1;
#endif

    printf_log(LOG_SEVERITY_INFO, "TerminateMonitoredProcess: Terminated %d monitored process(es)", terminated_count);
    return terminated_count;
}

int CountMonitoredProcess(const char* cmd_line_key) {
    int monitor_count = 0;

#if defined(_MSC_VER)
    // Windows implementation
    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (snapshot == INVALID_HANDLE_VALUE) {
        printf_log(LOG_SEVERITY_ERROR, "CountMonitoredProcess: Failed to create process snapshot");
        return -1;
    }

    PROCESSENTRY32W pe32;
    pe32.dwSize = sizeof(PROCESSENTRY32W);

    if (!Process32FirstW(snapshot, &pe32)) {
        CloseHandle(snapshot);
        printf_log(LOG_SEVERITY_ERROR, "CountMonitoredProcess: Failed to get first process");
        return -1;
    }

    DWORD current_pid = GetCurrentProcessId();

    // Dynamically load NtQueryInformationProcess for reading command line
    HMODULE ntdll = GetModuleHandleW(L"ntdll.dll");
    PFN_NtQueryInformationProcess pNtQueryInformationProcess = nullptr;
    if (ntdll) {
        pNtQueryInformationProcess = reinterpret_cast<PFN_NtQueryInformationProcess>(
            GetProcAddress(ntdll, "NtQueryInformationProcess"));
    }

    do {
        if (pe32.th32ProcessID == current_pid) {
            continue;
        }

        // Only process child processes of current process
        if (pe32.th32ParentProcessID != current_pid) {
            continue;
        }

        HANDLE process = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, pe32.th32ProcessID);
        if (!process) {
            continue;
        }

        bool is_target = false;

        if (pNtQueryInformationProcess) {
            MY_PROCESS_BASIC_INFORMATION pbi;

            pbi.Reserved1 = nullptr;
            pbi.PebBaseAddress = nullptr;
            pbi.Reserved2[0] = nullptr;
            pbi.Reserved2[1] = nullptr;
            pbi.UniqueProcessId = 0;
            pbi.Reserved3 = nullptr;

            NTSTATUS status = pNtQueryInformationProcess(process, ProcessBasicInformation,
                &pbi, sizeof(pbi), nullptr);
            if (status == 0 && pbi.PebBaseAddress) {
                // Check if PEB memory is readable before reading
                if (!IsMemoryReadable(process, pbi.PebBaseAddress)) {
                    printf_log(LOG_SEVERITY_WARNING,
                        "CountMonitoredProcess: PEB memory not readable for PID=%d",
                        pe32.th32ProcessID);
                    CloseHandle(process);
                    continue;
                }

                MY_PEB peb;
                SIZE_T bytes_read;
                if (ReadProcessMemory(process, pbi.PebBaseAddress, &peb, sizeof(peb), &bytes_read)) {
                    // Check if ProcessParameters memory is readable before reading
                    if (!IsMemoryReadable(process, peb.ProcessParameters)) {
                        printf_log(LOG_SEVERITY_WARNING,
                            "CountMonitoredProcess: ProcessParameters memory not readable for PID=%d",
                            pe32.th32ProcessID);
                        CloseHandle(process);
                        continue;
                    }

                    MY_RTL_USER_PROCESS_PARAMETERS params;
                    if (ReadProcessMemory(process, peb.ProcessParameters, &params, sizeof(params), &bytes_read)) {
                        // Check if CommandLine.Buffer memory is readable before reading
                        if (!IsMemoryReadable(process, params.CommandLine.Buffer)) {
                            printf_log(LOG_SEVERITY_WARNING,
                                "CountMonitoredProcess: CommandLine.Buffer memory not readable for PID=%d",
                                pe32.th32ProcessID);
                            CloseHandle(process);
                            continue;
                        }

                        // Read command line string
                        WCHAR* cmd_line = (WCHAR*)malloc(params.CommandLine.MaximumLength + sizeof(WCHAR));
                        if (cmd_line) {
                            if (ReadProcessMemory(process, params.CommandLine.Buffer,
                                cmd_line, params.CommandLine.Length, &bytes_read)) {
                                // Null-terminate the string
                                cmd_line[params.CommandLine.Length / sizeof(WCHAR)] = L'\0';

                                // Convert to narrow string for easier comparison
                                int cmd_line_len = WideCharToMultiByte(CP_UTF8, 0, cmd_line, -1,
                                    NULL, 0, NULL, NULL);
                                if (cmd_line_len > 0) {
                                    char* cmd_line_utf8 = (char*)malloc(cmd_line_len);
                                    if (cmd_line_utf8) {
                                        WideCharToMultiByte(CP_UTF8, 0, cmd_line, -1,
                                            cmd_line_utf8, cmd_line_len, NULL, NULL);

                                        // Check if command line contains --type=monitored
                                        if (strstr(cmd_line_utf8, cmd_line_key)) {
                                            is_target = true;
                                            printf_log(LOG_SEVERITY_INFO,
                                                "CountMonitoredProcess: Found monitored process PID=%d",
                                                pe32.th32ProcessID);
                                        }

                                        free(cmd_line_utf8);
                                    }
                                }
                            }
                            free(cmd_line);
                        }
                    }
                }
            }
        }

        if (is_target) {
            monitor_count++;
            printf_log(LOG_SEVERITY_INFO, "CountMonitoredProcess: Found monitored process PID=%d", pe32.th32ProcessID);
        }

        CloseHandle(process);

    }
    while (Process32NextW(snapshot, &pe32));

    CloseHandle(snapshot);

#elif defined(__linux__)
    // Linux implementation
    DIR* proc_dir = opendir("/proc");
    if (!proc_dir) {
        printf_log(LOG_SEVERITY_ERROR, "CountMonitoredProcess: Failed to open /proc");
        return -1;
    }

    pid_t current_pid = getpid();

    struct dirent* entry;
    while ((entry = readdir(proc_dir)) != NULL) {
        if (!isdigit(entry->d_name[0])) {
            continue;
        }

        pid_t pid = atoi(entry->d_name);

        if (pid == current_pid) {
            continue;
        }

        // Check if it's a child process of current process
        char status_path[256];
        snprintf(status_path, sizeof(status_path), "/proc/%d/status", pid);

        FILE* status_file = fopen(status_path, "r");
        if (!status_file) {
            continue;
        }

        char status_line[256];
        pid_t parent_pid = 0;
        while (fgets(status_line, sizeof(status_line), status_file)) {
            if (strncmp(status_line, "PPid:", 5) == 0) {
                sscanf(status_line, "PPid:\t%d", &parent_pid);
                break;
            }
        }
        fclose(status_file);

        // Only process child processes of current process
        if (parent_pid != current_pid) {
            continue;
        }

        char cmdline_path[256];
        snprintf(cmdline_path, sizeof(cmdline_path), "/proc/%d/cmdline", pid);

        FILE* cmdline_file = fopen(cmdline_path, "r");
        if (!cmdline_file) {
            continue;
        }

        char cmdline[8192];
        size_t bytes_read = fread(cmdline, 1, sizeof(cmdline) - 1, cmdline_file);
        cmdline[bytes_read] = '\0';
        fclose(cmdline_file);

        if (strstr(cmdline, cmd_line_key)) {
            monitor_count++;
            printf_log(LOG_SEVERITY_INFO, "CountMonitoredProcess: Found monitored process PID=%d", pid);
        }
    }

    closedir(proc_dir);

#elif defined(__APPLE__)
    // macOS implementation
    pid_t current_pid = getpid();
    int num_pids = proc_listpids(PROC_ALL_PIDS, 0, NULL, 0);

    if (num_pids <= 0) {
        printf_log(LOG_SEVERITY_ERROR, "CountMonitoredProcess: Failed to get process list");
        return -1;
    }

    std::vector<pid_t> pids(num_pids);
    proc_listpids(PROC_ALL_PIDS, 0, pids.data(), sizeof(pid_t) * num_pids);

    for (int i = 0; i < num_pids; i++) {
        if (pids[i] <= 0 || pids[i] == current_pid) {
            continue;
        }

        // Get process information to check parent PID
        struct kinfo_proc proc_info;
        size_t proc_info_size = sizeof(proc_info);
        int mib[4] = { CTL_KERN, KERN_PROC, KERN_PROC_PID, pids[i] };

        if (sysctl(mib, 4, &proc_info, &proc_info_size, NULL, 0) != 0) {
            continue;
        }

        // Only process child processes of current process
        if (proc_info.kp_eproc.e_ppid != current_pid) {
            continue;
        }

        char args_buffer[MAXPATHLEN * 4];
        int mib_args[3] = { CTL_KERN, KERN_PROCARGS2, pids[i] };
        size_t args_size = sizeof(args_buffer);

        if (sysctl(mib_args, 3, args_buffer, &args_size, NULL, 0) != 0) {
            continue;
        }

        int argc;
        memcpy(&argc, args_buffer, sizeof(argc));

        char* args_end = args_buffer + args_size;
        char* exe_path = args_buffer + sizeof(argc);
        char* current_arg = exe_path + strlen(exe_path) + 1;

        while (current_arg < args_end && *current_arg == '\0') {
            current_arg++;
        }

        bool is_target = false;
        char* arg = current_arg;
        for (int j = 0; j < argc && arg < args_end; j++) {
            if (strstr(arg, cmd_line_key)) {
                is_target = true;
                break;
            }
            arg += strlen(arg) + 1;
        }

        if (is_target) {
            monitor_count++;
            printf_log(LOG_SEVERITY_INFO, "CountMonitoredProcess: Found monitored process PID=%d", pids[i]);
        }
    }
#else
    printf_log(LOG_SEVERITY_ERROR, "CountMonitoredProcess: Unsupported platform");
    return -1;
#endif

    printf_log(LOG_SEVERITY_INFO, "CountMonitoredProcess: Found %d monitored process(es)", monitor_count);
    return monitor_count;
}

const char_t* dotnet_runtime_Config = L"./managed/BatchCmdDsl.runtimeconfig.json";
const char_t* dotnet_assembly_path = L"./managed/BatchCmdDsl.dll";
const char_t* dotnet_class_name = L"Program, BatchCmdDsl";
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
        printf_log(LOG_SEVERITY_ERROR, "Failed to load hostfxr.dll");
        return -2;
    }
#else
#ifdef _WIN32
    wchar_t hostfxr_path_w[1024];
    size_t sz = sizeof(hostfxr_path_w) / sizeof(wchar_t);
    int rc0 = get_hostfxr_path(hostfxr_path_w, &sz, nullptr);
    if (rc0 != 0) {
        printf_log(LOG_SEVERITY_ERROR, "get_hostfxr_path failed: %d (0x%x)", rc0, rc0);
        out_rc = rc0;
        return -1;
    }
    char path[1025];
    WideToUtf8ToBuffer(hostfxr_path_w, path, 1024);
    printf_log(LOG_SEVERITY_INFO, "[native] hostfxr path: %s\n", path);
#else
    char hostfxr_path[PATH_MAX];
    size_t sz = sizeof(hostfxr_path);
    int rc0 = get_hostfxr_path(hostfxr_path, &sz, nullptr);
    if (rc0 != 0) {
        printf_log(LOG_SEVERITY_ERROR, "get_hostfxr_path failed: %d (0x%x)", rc0, rc0);
        out_rc = rc0;
        return -1;
    }
    printf("[native] hostfxr path: %s\n", hostfxr_path);
#endif

#ifdef _WIN32
    HMODULE hostfxr_lib = LoadLibraryW(hostfxr_path_w);
    if (!hostfxr_lib) {
        out_rc = static_cast<int>(::GetLastError());
        printf_log(LOG_SEVERITY_ERROR, "LoadLibraryW failed\n");
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
        printf_log(LOG_SEVERITY_ERROR, "Failed to get hostfxr functions");
        return -3;
    }

    int argc = 1;
    const char_t* argv[] = { dotnet_assembly_path };
#ifdef USE_SPEC_DOTNET
    // Initialize the .NET Core runtime
    hostfxr_initialize_parameters parameters{
        sizeof(hostfxr_initialize_parameters),
        L"./",
        L"./dotnet/Microsoft.NETCore.App/9.0.2"
    };

    hostfxr_handle cxt = nullptr;
    int rc = init_config_fptr(dotnet_runtime_Config, &parameters, &cxt);
    //int rc = init_cmdline_fptr(argc, argv, &parameters, &cxt);
#else
    hostfxr_handle cxt = nullptr;
    int rc = init_config_fptr(dotnet_runtime_Config, nullptr, &cxt);
    //int rc = init_cmdline_fptr(argc, argv, nullptr, &cxt);
#endif

    if (rc != 0 || cxt == nullptr)
    {
        printf_log(LOG_SEVERITY_ERROR, "Failed to initialize .NET Core runtime: %d (0x%x)", rc, rc);
        out_rc = rc;
        return -4;
    }

    // Get the delegate for the runtime
    rc = get_delegate_fptr(cxt, hdt_load_assembly_and_get_function_pointer, reinterpret_cast<void**>(&load_assembly_and_get_function_pointer));
    if (rc != 0 || load_assembly_and_get_function_pointer == nullptr)
    {
        printf_log(LOG_SEVERITY_ERROR, "Failed to get load_assembly_and_get_function_pointer: %d (0x%x)", rc, rc);
        out_rc = rc;
        return -5;
    }

    // Before using `hostfxr_run_app`, we must initialize the .NET Core runtime using `hostfxr_initialize_for_dotnet_command_line`
    // instead of `hostfxr_initialize_for_runtime_config`.
    //rc = run_app_fptr(cxt);
    if (rc != 0)
    {
        printf_log(LOG_SEVERITY_ERROR, "Failed to call hostfxr_run_app: %d (0x%x)", rc, rc);
        out_rc = rc;
        return -6;
    }

    // Close the host context
    close_fptr(cxt);

    return 0;
}

typedef void (*host_native_log_fn)(const char* msg);
typedef int (*host_terminate_process_fn)(const char* key);
typedef int (*host_count_process_fn)(const char* key);

typedef struct {
    host_native_log_fn NativeLog;
    host_terminate_process_fn TerminateProcess;
    host_count_process_fn CountProcess;
} HostApi;

void host_native_log(const char* msg)
{
    printf("%s\n", msg);

#if defined(_MSC_VER)
    // Convert UTF-8 to wide char for OutputDebugString
    std::wstring wbuffer = Utf8ToWstring(msg);
    ::OutputDebugStringW(wbuffer.c_str());
#endif
}
int host_terminate_process(const char* key)
{
    if (!key) {
        return -1;
    }
    return TerminateMonitoredProcess(key);
}
int host_count_process(const char* key)
{
    if (!key) {
        return -1;
    }
    return CountMonitoredProcess(key);
}

// Function to call .NET Core method
int call_dotnet_method(int& rc)
{
    // native api
    HostApi api;
    api.NativeLog = &host_native_log;
    api.TerminateProcess = &host_terminate_process;
    api.CountProcess = &host_count_process;

    typedef int (CORECLR_DELEGATE_CALLTYPE* register_api_fn)(void* arg);
    register_api_fn register_entry = nullptr;
    rc = load_assembly_and_get_function_pointer(
        dotnet_assembly_path,
        dotnet_class_name,
        L"RegisterApi",
        UNMANAGEDCALLERSONLY_METHOD,
        nullptr,
        (void**)&register_entry);
    if (rc || !register_entry) {
        printf_log(LOG_SEVERITY_ERROR, "Failure: load register_entry");
        return -1;
    }

    if (register_entry) {
        int result = register_entry(&api);
        printf("RegisterApi returned: %d\n", result);
    }

    typedef int (CORECLR_DELEGATE_CALLTYPE* init_fn)(const char* cmd_line, const char* path);
    init_fn init_entry = nullptr;
    rc = load_assembly_and_get_function_pointer(
        dotnet_assembly_path,
        dotnet_class_name,
        L"Init",
        L"Program+InitDelegation, BatchCmdDsl",
        nullptr, // Reserved for future use
        reinterpret_cast<void**>(&init_entry) // Output function pointer
    );
    if (rc || !init_entry) {
        printf_log(LOG_SEVERITY_ERROR, "Failure: load init_entry");
        return -1;
    }

    if (init_entry) {
        const wchar_t* raw_command_line_w = ::GetCommandLineW();
        std::string raw_command_line_utf8 = WideStringToUtf8(raw_command_line_w);
        std::string exeDir = GetExeDir();
        int result = init_entry(raw_command_line_utf8.c_str(), exeDir.c_str());
        printf("Init returned: %d\n", result);
    }

    typedef int (CORECLR_DELEGATE_CALLTYPE* loop_fn)();
    loop_fn loop_entry = nullptr;
    rc = load_assembly_and_get_function_pointer(
        dotnet_assembly_path,
        dotnet_class_name,
        L"Loop",
        L"Program+LoopDelegation, BatchCmdDsl",
        nullptr, // Reserved for future use
        reinterpret_cast<void**>(&loop_entry) // Output function pointer
    );
    if (rc || !loop_entry) {
        printf_log(LOG_SEVERITY_ERROR, "Failure: load loop_entry");
        return -1;
    }

    if (loop_entry) {
        int result = loop_entry();
        printf("Loop returned: %d\n", result);
    }

    return 0;
}

int main(int argc, const char* argv[])
{
    std::cout << "DonetHost started.\n";

    int rc = 0;
    int r = load_hostfxr(rc);
    if (r != 0)
    {
        printf("load_hostfxr failed: %d (%d [0x%x])\n", r, rc, rc);
        return -1;
    }
    r = call_dotnet_method(rc);
    if (r != 0)
    {
        printf("call_dotnet_method failed: %d (%d [0x%x])\n", r, rc, rc);
        return -1;
    }
    return 0;
}
