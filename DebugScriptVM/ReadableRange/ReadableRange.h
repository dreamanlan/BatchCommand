// ReadableRange.h
#pragma once
#include <stdint.h>
#include <stddef.h>

typedef struct {
    void*  start;  // readable start address (inclusive)
    size_t size;   // readable size in bytes
} ReadableRange;

// Max size of user-defined extra data (you can adjust).
#define CED_MAX_USER_DATA_SIZE (64 * 1024)

typedef struct CED_CrashContext {
    uint64_t start_addr;
    uint64_t size;
} CED_CrashContext;
// Full dump data container.
typedef struct CED_CrashDump {
    CED_CrashContext context;
    uint64_t user_data_size;
    uint8_t user_data[CED_MAX_USER_DATA_SIZE];
} CED_CrashDump;

// Try to compute a readable range around `addr` with ±half_range bytes.
// Returns 1 on success, 0 on failure.
int GetReadableRangeAround(void* addr,
                           size_t half_range,
                           ReadableRange* out_range);

uint64_t CED_GetTimestamp();
uint64_t CED_GetThreadId();
int CED_Platform_WriteDumpFile(const char* path, const CED_CrashDump* dump);
void CED_InitWithDir(const char* dump_dir, const char* base_name);
void CED_OnCrash(void* ptr);
