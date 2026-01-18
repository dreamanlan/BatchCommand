#include "path_utils.h"

#if defined(_WIN32)
  #include <windows.h>
#elif defined(__APPLE__)
  #include <mach-o/dyld.h>
  #include <limits.h>
#elif defined(__linux__)
  #include <unistd.h>
  #include <limits.h>
#else
  #error "Unsupported platform"
#endif

// Helper: extract directory from full path
static std::string GetDirFromPath(const std::string& path) {
    if (path.empty())
        return std::string();

    size_t pos = path.find_last_of("/\\");
    if (pos == std::string::npos)
        return path;

    return path.substr(0, pos);
}

std::string GetExePath() {
#if defined(_WIN32)
    // Get wide-char path first
    std::wstring wpath;
    wpath.resize(MAX_PATH);

    DWORD len = ::GetModuleFileNameW(nullptr, &wpath[0],
                                     static_cast<DWORD>(wpath.size()));
    if (len == 0)
        return std::string();

    // If buffer was too small, enlarge it and try again
    while (len == wpath.size()) {
        wpath.resize(wpath.size() * 2);
        len = ::GetModuleFileNameW(nullptr, &wpath[0],
                                   static_cast<DWORD>(wpath.size()));
        if (len == 0)
            return std::string();
    }
    wpath.resize(len);

    // Convert wide-char path to UTF-8
    int size = ::WideCharToMultiByte(CP_UTF8, 0, wpath.c_str(), -1,
                                     nullptr, 0, nullptr, nullptr);
    if (size <= 0)
        return std::string();

    std::string path(size - 1, '\0');
    ::WideCharToMultiByte(CP_UTF8, 0, wpath.c_str(), -1,
                          &path[0], size, nullptr, nullptr);
    return path;

#elif defined(__APPLE__)
    char buffer[PATH_MAX];
    uint32_t size = sizeof(buffer);

    // _NSGetExecutablePath may return a path with symbolic links
    if (_NSGetExecutablePath(buffer, &size) != 0) {
        // Buffer too small, PATH_MAX should be enough for most cases
        return std::string();
    }

    return std::string(buffer);

#elif defined(__linux__)
    char buffer[PATH_MAX];
    ssize_t len = ::readlink("/proc/self/exe", buffer, sizeof(buffer) - 1);
    if (len == -1)
        return std::string();

    buffer[len] = '\0';
    return std::string(buffer);
#endif
}

std::string GetExeDir() {
    return GetDirFromPath(GetExePath());
}