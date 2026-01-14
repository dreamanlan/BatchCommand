#if defined(_WIN32)

#include "ReadableRange.h"
#include <windows.h>
#include <string.h>

int GetReadableRangeAround(void* addr,
                           size_t half_range,
                           ReadableRange* out_range) {
    if (!addr || !out_range) return 0;

    MEMORY_BASIC_INFORMATION mbi;
    SIZE_T r = VirtualQuery(addr, &mbi, sizeof(mbi));
    if (r != sizeof(mbi)) {
        return 0;
    }

    DWORD protect = mbi.Protect;
    if (protect == PAGE_NOACCESS || protect == PAGE_EXECUTE) {
        return 0;
    }
    if (protect & PAGE_GUARD) {
        return 0;
    }

    uintptr_t region_start = (uintptr_t)mbi.BaseAddress;
    uintptr_t region_end   = region_start + mbi.RegionSize;

    uintptr_t target = (uintptr_t)addr;
    if (target < region_start || target >= region_end) {
        // Theoretically shouldn't happen, but check anyway.
        return 0;
    }

    uintptr_t want_start = (target > half_range) ? (target - half_range) : 0;
    uintptr_t want_end   = target + half_range;

    uintptr_t real_start = (want_start > region_start) ? want_start : region_start;
    uintptr_t real_end   = (want_end   < region_end)   ? want_end   : region_end;

    if (real_end <= real_start) {
        return 0;
    }

    out_range->start = (void*)real_start;
    out_range->size  = (size_t)(real_end - real_start);
    return 1;
}

// Timestamp (seconds from Unix epoch).
uint64_t CED_GetTimestamp() {
    FILETIME ft;
    GetSystemTimeAsFileTime(&ft);
    ULARGE_INTEGER uli;
    uli.LowPart  = ft.dwLowDateTime;
    uli.HighPart = ft.dwHighDateTime;
    // Windows file time: 100-ns intervals since January 1, 1601 (UTC)
    uint64_t fileTime = uli.QuadPart;
    uint64_t seconds  = fileTime / 10000000ULL; // to seconds
    // Convert to Unix epoch (1970)
    const uint64_t EPOCH_DIFF = 11644473600ULL;
    if (seconds > EPOCH_DIFF) {
        seconds -= EPOCH_DIFF;
    } else {
        seconds = 0;
    }
    return seconds;
}

uint64_t CED_GetThreadId() {
    return (uint64_t)GetCurrentThreadId();
}

// Write dump file (not in async signal context, can use Win32 API).
int CED_Platform_WriteDumpFile(const char* path, const CED_CrashDump* dump) {
    if (!path || !dump) return 0;

    HANDLE hFile = CreateFileA(
        path,
        GENERIC_WRITE,
        FILE_SHARE_READ,
        NULL,
        CREATE_ALWAYS,
        FILE_ATTRIBUTE_NORMAL,
        NULL
    );
    if (hFile == INVALID_HANDLE_VALUE) {
        return 0;
    }

    DWORD bytesWritten = 0;
    BOOL ok = WriteFile(hFile, &dump->context, sizeof(CED_CrashContext), &bytesWritten, NULL);
    if (!ok || bytesWritten != sizeof(CED_CrashContext)) {
        CloseHandle(hFile);
        return 0;
    }

    ok = WriteFile(hFile, &dump->user_data_size, sizeof(uint32_t), &bytesWritten, NULL);
    if (!ok || bytesWritten != sizeof(uint32_t)) {
        CloseHandle(hFile);
        return 0;
    }

    if (dump->user_data_size > 0) {
        ok = WriteFile(hFile, dump->user_data, dump->user_data_size, &bytesWritten, NULL);
        if (!ok || bytesWritten != dump->user_data_size) {
            CloseHandle(hFile);
            return 0;
        }
    }

    CloseHandle(hFile);
    return 1;
}

#endif