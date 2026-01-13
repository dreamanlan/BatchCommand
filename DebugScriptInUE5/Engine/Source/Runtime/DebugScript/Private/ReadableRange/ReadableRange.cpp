#include "ReadableRange.h"
#include <string.h>

// Global configuration
static char g_cedDumpDir[512] = { 0 };
static char g_cedDumpBase[128] = { 0 };

// Internal string copy.
static void CED_StrCopySafe(char* dst, size_t dstSize, const char* src) {
    if (!dst || dstSize == 0) return;
    if (!src) {
        dst[0] = '\0';
        return;
    }
    size_t i = 0;
    for (; i + 1 < dstSize && src[i] != '\0'; ++i) {
        dst[i] = src[i];
    }
    dst[i] = '\0';
}

// Build a dump file path from dir/base/timestamp/thread/seq.
// This function must be async-signal-safe when called from crash handler.
static void CED_BuildDumpFilePath(char* outPath, size_t outSize, uint32_t seq) {
    if (!outPath || outSize == 0) return;
    outPath[0] = '\0';
    size_t pos = 0;

    auto append_char = [&](char c) {
        if (pos + 1 < outSize) {
            outPath[pos++] = c;
            outPath[pos] = '\0';
        }
        };
    auto append_str = [&](const char* s) {
        if (!s) return;
        while (*s && pos + 1 < outSize) {
            outPath[pos++] = *s++;
        }
        outPath[pos] = '\0';
        };
    auto append_uint64 = [&](uint64_t v) {
        char tmp[32];
        int len = 0;
        if (v == 0) {
            tmp[len++] = '0';
        }
        else {
            while (v > 0 && len < (int)sizeof(tmp)) {
                tmp[len++] = (char)('0' + (v % 10));
                v /= 10;
            }
        }
        // reverse write
        for (int i = len - 1; i >= 0 && pos + 1 < outSize; --i) {
            outPath[pos++] = tmp[i];
        }
        outPath[pos] = '\0';
        };
    auto append_uint32 = [&](uint32_t v) {
        append_uint64((uint64_t)v);
        };

    uint64_t timestamp = CED_GetTimestamp();
    uint64_t thread_id = CED_GetThreadId();

    append_str(g_cedDumpDir);
    append_char('/');
    append_str(g_cedDumpBase);
    append_char('_');
    append_uint64(timestamp);
    append_char('_');
    append_uint64(thread_id);
    append_char('_');
    append_uint32(seq);
    append_str(".bin");
}

void CED_InitWithDir(const char* dump_dir, const char* base_name) {
    CED_StrCopySafe(g_cedDumpDir, sizeof(g_cedDumpDir), dump_dir);
    CED_StrCopySafe(g_cedDumpBase, sizeof(g_cedDumpBase), base_name);
}

bool CED_OnCrash(void* ptr) {
    if (!ptr)
        return false;
    if (g_cedDumpDir[0] == '\0' || g_cedDumpBase[0] == '\0') {
        // Not configured, do nothing.
        return false;
    }

    ReadableRange range;
    if (!GetReadableRangeAround(ptr, CED_MAX_USER_DATA_SIZE/2, &range)) {
        return false;
    }

    CED_CrashDump dump;
    memset(&dump, 0, sizeof(dump));
    dump.context.start_addr = reinterpret_cast<uint64_t>(range.start);
    dump.context.size = range.size;
    dump.user_data_size = range.size > CED_MAX_USER_DATA_SIZE ? CED_MAX_USER_DATA_SIZE : range.size;
    memcpy(dump.user_data, range.start, dump.user_data_size);

    // Simple sequence counter (not thread-safe, but we are crashing anyway).
    static volatile uint32_t s_seq = 0;
    uint32_t seq = ++s_seq;

    char path[512];
    CED_BuildDumpFilePath(path, sizeof(path), seq);

    return 0 != CED_Platform_WriteDumpFile(path, &dump);
}