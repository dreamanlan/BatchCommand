#pragma once
#include <string>

// Returns full path of the current executable, UTF-8 encoded.
// Returns empty string on failure.
std::string GetExePath();

// Returns directory part of the current executable path, UTF-8 encoded.
// Returns empty string on failure.
std::string GetExeDir();

// Returns last directory name of the current executable path, UTF-8 encoded.
// Returns empty string on failure.
std::string GetExeLastDirName();

// Returns full path of the nearest .app directory on macOS, UTF-8 encoded.
// For Helper processes, this returns the Helper's .app path.
// Returns empty string on failure or on non-macOS platforms.
std::string GetMacAppDirPath();

// Returns full path of the outermost (main) .app directory on macOS.
// For Helper processes inside Frameworks/, this returns the main app's .app path
// (e.g. webagent.app instead of webagent Helper.app).
// Returns empty string on failure or on non-macOS platforms.
std::string GetMacMainAppDirPath();

// Returns the outermost (main) .app directory name on macOS.
// Returns empty string on failure.
std::string GetMacMainAppDirName();

// Returns ~/Library/Application Support/<appname>/ on macOS, where <appname>
// is derived from the .app bundle name. Returns empty string on failure or
// on non-macOS platforms.
std::string GetMacAppSupportDir();

// Returns the nearest app directory name of the current executable path, UTF-8 encoded.
// Returns empty string on failure.
std::string GetMacAppDirName();