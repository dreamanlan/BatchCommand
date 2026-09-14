#include "path_utils.h"

#if defined(_WIN32)
  #include <windows.h>
#elif defined(__APPLE__)
  #include <mach-o/dyld.h>
  #include <cstdlib>
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
static std::string GetLastNameFromPath(const std::string& path) {
    if (path.empty())
        return std::string();

    size_t pos = path.find_last_of("/\\");
    if (pos == std::string::npos)
        return path;

    return path.substr(pos + 1);
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

std::string GetExeLastDirName() {
    return GetLastNameFromPath(GetExeDir());
}

std::string GetMacAppDirPath() {
#if defined(__APPLE__)
    std::string path = GetExePath();
    if (path.empty())
        return std::string();

    // Traverse up the directory tree to find .app bundle
    while (!path.empty()) {
        size_t pos = path.find_last_of("/\\");
        if (pos == std::string::npos)
            break;

        path = path.substr(0, pos);

        // Check if current path ends with .app
        if (path.length() > 4 && path.substr(path.length() - 4) == ".app") {
            return path;
        }
    }

    return std::string();
#else
    // On non-macOS platforms, return empty string
    return std::string();
#endif
}

std::string GetMacAppSupportDir() {
#if defined(__APPLE__)
    const char* home = getenv("HOME");
    if (!home || home[0] == '\0')
        return std::string();

    // Derive app name from the outermost (main) .app bundle name
    // (e.g. "webagent.app" -> "webagent")
    // Use GetMacMainAppDirName to ensure Helper processes also use the main app's name
    std::string appName = GetMacMainAppDirName();
    if (appName.length() > 4 && appName.substr(appName.length() - 4) == ".app") {
        appName = appName.substr(0, appName.length() - 4);
    }
    if (appName.empty()) {
        appName = "webagent";  // fallback
    }

    std::string dir = std::string(home) + "/Library/Application Support/" + appName + "/";
    return dir;
#else
    return std::string();
#endif
}

std::string GetMacMainAppDirPath() {
#if defined(__APPLE__)
    std::string path = GetExePath();
    if (path.empty())
        return std::string();

    // Find the outermost .app bundle by traversing up the directory tree
    // and remembering the last .app we found.
    // For Helper: .../webagent.app/Contents/Frameworks/webagent Helper.app/Contents/MacOS/helper
    //   -> finds webagent Helper.app first, then webagent.app (outermost)
    // For Browser: .../webagent.app/Contents/MacOS/webagent
    //   -> finds webagent.app (only one)
    std::string outermost_app;
    while (!path.empty()) {
        size_t pos = path.find_last_of("/\\");
        if (pos == std::string::npos)
            break;

        path = path.substr(0, pos);

        // Check if current path ends with .app
        if (path.length() > 4 && path.substr(path.length() - 4) == ".app") {
            outermost_app = path;
            // Don't break - keep looking for an outer .app
        }
    }

    return outermost_app;
#else
    return std::string();
#endif
}

std::string GetMacMainAppDirName() {
    std::string appPath = GetMacMainAppDirPath();
    if (appPath.empty())
        return std::string();

    return GetLastNameFromPath(appPath);
}

std::string GetMacAppDirName() {
    std::string appPath = GetMacAppDirPath();
    if (appPath.empty())
        return std::string();

    return GetLastNameFromPath(appPath);
}