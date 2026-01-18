#pragma once
#include <string>

// Returns full path of the current executable, UTF-8 encoded.
// Returns empty string on failure.
std::string GetExePath();

// Returns directory part of the current executable path, UTF-8 encoded.
// Returns empty string on failure.
std::string GetExeDir();