#if defined(__APPLE__) && TARGET_OS_IPHONE

#include "ReadableRange.h"
#include <mach/mach.h>
#include <mach/mach_vm.h>
#include <unistd.h>
#include <string.h>
#include <time.h>
#include <fcntl.h>
#include <pthread.h>

int GetReadableRangeAround(void* addr,
                           size_t half_range,
                           ReadableRange* out_range) {
    if (!addr || !out_range) return 0;

    mach_vm_address_t target = (mach_vm_address_t)addr;
    mach_vm_size_t    size   = 0;
    mach_port_t       object_name = MACH_PORT_NULL;
    vm_region_basic_info_data_64_t info;
    mach_msg_type_number_t         info_count = VM_REGION_BASIC_INFO_COUNT_64;

    mach_vm_address_t region_addr = target;
    kern_return_t kr = mach_vm_region(
        mach_task_self(),
        &region_addr,
        &size,
        VM_REGION_BASIC_INFO_64,
        (vm_region_info_t)&info,
        &info_count,
        &object_name
    );
    if (kr != KERN_SUCCESS) {
        return 0;
    }

    // region_addr is the base of the region
    uintptr_t region_start = (uintptr_t)region_addr;
    uintptr_t region_end   = region_start + (uintptr_t)size;

    uintptr_t t = (uintptr_t)addr;
    if (t < region_start || t >= region_end) {
        return 0;
    }

    // Check readability: info.protection has VM_PROT_READ, VM_PROT_WRITE, etc.
    if (!(info.protection & VM_PROT_READ)) {
        return 0;
    }

    uintptr_t want_start = (t > half_range) ? (t - half_range) : 0;
    uintptr_t want_end   = t + half_range;

    uintptr_t real_start = (want_start > region_start) ? want_start : region_start;
    uintptr_t real_end   = (want_end   < region_end)   ? want_end   : region_end;

    if (real_end <= real_start) {
        return 0;
    }

    out_range->start = (void*)real_start;
    out_range->size  = (size_t)(real_end - real_start);
    return 1;
}

// Timestamp (seconds).
uint64_t CED_GetTimestamp() {
    struct timespec ts;
    if (clock_gettime(CLOCK_REALTIME, &ts) == 0) {
        return (uint64_t)ts.tv_sec;
    }
    return 0;
}

// Thread ID
uint64_t CED_GetThreadId() {
    return (uint64_t)pthread_self();
}

// Write dump file (called from signal handler, keep it async-signal-safe).
int CED_Platform_WriteDumpFile(const char* path, const CED_CrashDump* dump) {
    if (!path || !dump) return 0;
    int fd = open(path, O_CREAT | O_WRONLY | O_TRUNC, 0644);
    if (fd < 0) return 0;

    ssize_t w1 = write(fd, &dump->context, sizeof(CED_CrashContext));
    if (w1 != (ssize_t)sizeof(CED_CrashContext)) {
        close(fd);
        return 0;
    }

    ssize_t w2 = write(fd, &dump->user_data_size, sizeof(uint32_t));
    if (w2 != (ssize_t)sizeof(uint32_t)) {
        close(fd);
        return 0;
    }

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

#endif