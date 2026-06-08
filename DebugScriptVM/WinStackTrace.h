// WinStackTrace.h
// Standalone Windows stack trace walker. Depends only on the Windows SDK
// and the C++ standard library. On non-MSVC / non-Windows platforms the
// function compiles to a no-op so callers do not need to guard the call
// site with platform macros.
#pragma once

#include <functional>

// Callback type. Receives a textual description of one stack frame.
// The pointer is owned by the walker and is only valid for the duration
// of the call; copy the string if you need to keep it.
// Lambdas are supported (std::function wraps them).
typedef std::function<void(const char*)> WinStackTraceCallback;

// Walk the current thread's stack from top (innermost) to bottom (outermost).
//
// startFrame  Number of frames to skip from the top before invoking the
//             callback. WalkWinStackTrace itself and its internal helpers
//             are always skipped before this count is applied, so passing
//             0 means "start at the caller of WalkWinStackTrace".
// frameCount  Maximum number of frames to deliver to the callback.
//             A value <= 0 means "no limit": walk until the bottom of the
//             stack is reached.
// callback    Invoked once per frame. May be empty / null, in which case
//             the function returns immediately.
//
// Notes:
//  - On non-MSVC / non-Windows builds this function does nothing.
//  - DbgHelp APIs are serialized internally with a mutex, so the function
//    is safe to call from multiple threads, but is not lock-free.
void WalkWinStackTrace(int startFrame, int frameCount, const WinStackTraceCallback& callback);
