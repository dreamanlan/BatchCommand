// ReadableRangeAndroid.cpp
// Note: this code is NOT async-signal-safe. Use only in normal thread context.

#if defined(__ANDROID__)

#include "ReadableRange.h"

#include <stdio.h>
#include <string.h>
#include <inttypes.h>
#include <errno.h>
#include <unistd.h>
#include <time.h>
#include <fcntl.h>
#include <sys/syscall.h>

typedef struct {
    uintptr_t start;
    uintptr_t end;
    int       readable;
    int       writable;
    int       executable;
} VmArea;

// Parse a single line of /proc/self/maps into VmArea.
// Example line:
// 00400000-0040b000 r-xp 00000000 fc:01 1234       /system/bin/app_process32
static int ParseMapsLine(const char* line, VmArea* out_vma) {
    if (!line || !out_vma) return 0;

    // Buffer for flags, path etc.
    char perms[5] = {0};
    // We only care about start, end, perms.
    // Format: start-end perms offset dev inode pathname...
    unsigned long start = 0;
    unsigned long end   = 0;

    int scanned = sscanf(line, "%lx-%lx %4s", &start, &end, perms);
    if (scanned != 3) {
        return 0;
    }

    out_vma->start = (uintptr_t)start;
    out_vma->end   = (uintptr_t)end;
    out_vma->readable   = (perms[0] == 'r');
    out_vma->writable   = (perms[1] == 'w');
    out_vma->executable = (perms[2] == 'x');
    return 1;
}

// Main function: find readable region that covers addr and intersect with ±half_range.
int GetReadableRangeAround(void* addr,
                           size_t half_range,
                           ReadableRange* out_range) {
    if (!addr || !out_range) return 0;

    uintptr_t target = (uintptr_t)addr;

    FILE* fp = fopen("/proc/self/maps", "r");
    if (!fp) {
        return 0;
    }

    char line[512];
    VmArea vma;
    int   found = 0;

    while (fgets(line, sizeof(line), fp)) {
        if (!ParseMapsLine(line, &vma)) {
            continue;
        }
        if (!vma.readable) {
            continue; // only care about readable mappings
        }
        if (target < vma.start || target >= vma.end) {
            continue; // this VMA does not cover addr
        }

        // We found a readable VMA that contains target.
        // Now intersect [target-half_range, target+half_range] with [vma.start, vma.end).
        uintptr_t want_start = (target > half_range) ? (target - half_range) : 0;
        uintptr_t want_end   = target + half_range;

        // Clip by VMA bounds
        uintptr_t real_start = (want_start > vma.start) ? want_start : vma.start;
        uintptr_t real_end   = (want_end   < vma.end)   ? want_end   : vma.end;

        if (real_end > real_start) {
            out_range->start = (void*)real_start;
            out_range->size  = (size_t)(real_end - real_start);
            found = 1;
        }
        break;
    }

    fclose(fp);
    return found;
}

// Get epoch time (seconds).
uint64_t CED_GetTimestamp() {
    struct timespec ts;
    if (clock_gettime(CLOCK_REALTIME, &ts) == 0) {
        return (uint64_t)ts.tv_sec;
    }
    return 0;
}

// Get Linux thread ID.
uint64_t CED_GetThreadId() {
    return (uint64_t)syscall(SYS_gettid);
}

// Write dump file using async-signal-safe APIs.
int CED_Platform_WriteDumpFile(const char* path, const CED_CrashDump* dump) {
    if (!path || !dump) return 0;

    int fd = open(path, O_CREAT | O_WRONLY | O_TRUNC, 0644);
    if (fd < 0) return 0;

    // Write context
    ssize_t w1 = write(fd, &dump->context, sizeof(CED_CrashContext));
    if (w1 != (ssize_t)sizeof(CED_CrashContext)) {
        close(fd);
        return 0;
    }

    // Write size
    ssize_t w2 = write(fd, &dump->user_data_size, sizeof(uint32_t));
    if (w2 != (ssize_t)sizeof(uint32_t)) {
        close(fd);
        return 0;
    }

    // Write user data
    if (dump->user_data_size > 0) {
        ssize_t w3 = write(fd, dump->user_data, dump->user_data_size);
        if (w3 != (ssize_t)dump->user_data_size) {
            close(fd);
            return 0;
        }
    }

    close(fd);
    return 1;
}

#endif // __ANDROID__