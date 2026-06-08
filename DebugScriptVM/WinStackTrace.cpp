// WinStackTrace.cpp
// Implementation of WalkWinStackTrace. Uses only Windows SDK (DbgHelp)
// and the C++ standard library. The whole platform-specific body is
// gated on _MSC_VER and _WIN32 / _WIN64 so this translation unit is
// empty (and cannot fail to compile) on every other platform / compiler.

#include "WinStackTrace.h"

#if defined(_MSC_VER) && (defined(_WIN32) || defined(_WIN64))

// Keep windows.h lean and avoid macro pollution from min/max.
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#ifndef NOMINMAX
#define NOMINMAX
#endif

#include <windows.h>
#include <dbghelp.h>

#include <cstdio>
#include <cstring>
#include <mutex>

#pragma comment(lib, "dbghelp.lib")

namespace
{
    // DbgHelp APIs are not thread safe; serialize all access.
    std::mutex& GetDbgHelpMutex()
    {
        static std::mutex s_Mutex;
        return s_Mutex;
    }

    bool& GetSymInitializedFlag()
    {
        static bool s_Initialized = false;
        return s_Initialized;
    }

    // Must be called with the DbgHelp mutex held.
    void EnsureSymInitialized(HANDLE hProcess)
    {
        bool& initialized = GetSymInitializedFlag();
        if (initialized)
            return;

        DWORD options = SymGetOptions();
        options |= SYMOPT_LOAD_LINES;
        options |= SYMOPT_UNDNAME;
        options |= SYMOPT_DEFERRED_LOADS;
        options |= SYMOPT_FAIL_CRITICAL_ERRORS;
        SymSetOptions(options);

        if (SymInitialize(hProcess, NULL, TRUE))
            initialized = true;
    }

    // Build a "0xADDRESS (module) function (at file:line)" style line.
    // Returns the number of bytes written into buf (excluding the null
    // terminator), or 0 on formatting failure.
    int FormatFrame(char* buf, size_t bufSize, HANDLE hProcess, DWORD64 pcAddress)
    {
        // Resolve symbol name.
        const size_t kMaxNameLen = MAX_SYM_NAME;
        char symbolStorage[sizeof(SYMBOL_INFO) + kMaxNameLen + 1];
        std::memset(symbolStorage, 0, sizeof(symbolStorage));
        SYMBOL_INFO* symbol = reinterpret_cast<SYMBOL_INFO*>(symbolStorage);
        symbol->SizeOfStruct = sizeof(SYMBOL_INFO);
        symbol->MaxNameLen = static_cast<ULONG>(kMaxNameLen);

        const char* funcName = "(<unknown>)";
        DWORD64 symDisp = 0;
        if (SymFromAddr(hProcess, pcAddress, &symDisp, symbol))
        {
            if (symbol->Name[0] != 0)
                funcName = symbol->Name;
        }

        // Resolve module name.
        const char* moduleName = "(<unknown>)";
        IMAGEHLP_MODULE64 moduleInfo;
        std::memset(&moduleInfo, 0, sizeof(moduleInfo));
        moduleInfo.SizeOfStruct = sizeof(IMAGEHLP_MODULE64);
        if (SymGetModuleInfo64(hProcess, pcAddress, &moduleInfo))
        {
            if (moduleInfo.ModuleName[0] != 0)
                moduleName = moduleInfo.ModuleName;
        }

        // Resolve file/line.
        IMAGEHLP_LINE64 line;
        std::memset(&line, 0, sizeof(line));
        line.SizeOfStruct = sizeof(IMAGEHLP_LINE64);
        DWORD lineDisp = 0;
        const bool hasLine = (SymGetLineFromAddr64(hProcess, pcAddress, &lineDisp, &line) != FALSE);

        int written = 0;
        if (hasLine && line.FileName != NULL)
        {
            written = _snprintf_s(
                buf, bufSize, _TRUNCATE,
                "0x%p (%s) %s (at %s:%lu)",
                reinterpret_cast<void*>(static_cast<uintptr_t>(pcAddress)),
                moduleName,
                funcName,
                line.FileName,
                static_cast<unsigned long>(line.LineNumber));
        }
        else
        {
            written = _snprintf_s(
                buf, bufSize, _TRUNCATE,
                "0x%p (%s) %s",
                reinterpret_cast<void*>(static_cast<uintptr_t>(pcAddress)),
                moduleName,
                funcName);
        }

        return written < 0 ? 0 : written;
    }

    // Hard upper bound on the number of frames we will ever walk in a
    // single call, regardless of frameCount. Prevents runaway loops on
    // corrupted stacks.
    const int kMaxWalkFrames = 1024;
}

void WalkWinStackTrace(int startFrame, int frameCount, const WinStackTraceCallback& callback)
{
    if (!callback)
        return;
    if (frameCount == 0)
        return;
    if (startFrame < 0)
        startFrame = 0;

    CONTEXT context;
    std::memset(&context, 0, sizeof(context));
    context.ContextFlags = CONTEXT_FULL;
    RtlCaptureContext(&context);

    HANDLE hProcess = GetCurrentProcess();
    HANDLE hThread = GetCurrentThread();

    STACKFRAME64 frame;
    std::memset(&frame, 0, sizeof(frame));

    DWORD machineType = 0;
#if defined(_M_X64)
    machineType = IMAGE_FILE_MACHINE_AMD64;
    frame.AddrPC.Offset = context.Rip;
    frame.AddrPC.Mode = AddrModeFlat;
    frame.AddrFrame.Offset = context.Rbp;
    frame.AddrFrame.Mode = AddrModeFlat;
    frame.AddrStack.Offset = context.Rsp;
    frame.AddrStack.Mode = AddrModeFlat;
#elif defined(_M_IX86)
    machineType = IMAGE_FILE_MACHINE_I386;
    frame.AddrPC.Offset = context.Eip;
    frame.AddrPC.Mode = AddrModeFlat;
    frame.AddrFrame.Offset = context.Ebp;
    frame.AddrFrame.Mode = AddrModeFlat;
    frame.AddrStack.Offset = context.Esp;
    frame.AddrStack.Mode = AddrModeFlat;
#elif defined(_M_ARM64)
    machineType = IMAGE_FILE_MACHINE_ARM64;
    frame.AddrPC.Offset = context.Pc;
    frame.AddrPC.Mode = AddrModeFlat;
    frame.AddrFrame.Offset = context.Fp;
    frame.AddrFrame.Mode = AddrModeFlat;
    frame.AddrStack.Offset = context.Sp;
    frame.AddrStack.Mode = AddrModeFlat;
#else
    // Unknown architecture, do nothing.
    return;
#endif

    std::lock_guard<std::mutex> guard(GetDbgHelpMutex());
    EnsureSymInitialized(hProcess);

    // Always skip the very first frame, which is WalkWinStackTrace itself.
    const int internalSkip = 1;
    const int totalSkip = internalSkip + startFrame;

    int walked = 0;
    int skipped = 0;
    int delivered = 0;

    while (walked < kMaxWalkFrames)
    {
        if (!StackWalk64(
                machineType,
                hProcess,
                hThread,
                &frame,
                &context,
                NULL,
                SymFunctionTableAccess64,
                SymGetModuleBase64,
                NULL))
        {
            break;
        }

        ++walked;

        if (frame.AddrPC.Offset == 0)
            break;

        if (skipped < totalSkip)
        {
            ++skipped;
            continue;
        }

        char buffer[1024];
        buffer[0] = '\0';
        FormatFrame(buffer, sizeof(buffer), hProcess, frame.AddrPC.Offset);
        callback(buffer);

        ++delivered;
        if (frameCount > 0 && delivered >= frameCount)
            break;
    }
}

#else // !(_MSC_VER && (_WIN32 || _WIN64))

// No-op fallback. Keeps callers free of platform guards.
void WalkWinStackTrace(int /*startFrame*/, int /*frameCount*/, const WinStackTraceCallback& /*callback*/)
{
}

#endif
