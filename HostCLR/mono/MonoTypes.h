#pragma once

struct MonoException;
struct MonoAssembly;
struct MonoObject;
struct MonoClassField;
struct MonoClass;
struct MonoDomain;
struct MonoImage;
struct MonoType;
struct MonoMethodSignature;
struct MonoArray;
struct MonoThread;
struct MonoVTable;
struct MonoProperty;
struct MonoReflectionAssembly;
struct MonoReflectionMethod;
struct MonoReflectionField;
struct MonoAppDomain;
struct MonoCustomAttrInfo;
struct MonoDl;
struct MonoManagedMemorySnapshot;
struct MonoProfiler;
struct MonoMethod;
struct MonoTableInfo;
struct MonoGenericContext;

struct MonoDlFallbackHandler;
struct MonoMethodDesc;
typedef const void* gconstpointer;
typedef void* gpointer;
typedef int gboolean;
typedef unsigned char guint8;
typedef uint16_t guint16;
typedef unsigned int guint32;
typedef int gint32;
typedef uint64_t guint64;
typedef int64_t gint64;
typedef unsigned long gulong;
typedef unsigned char   guchar;
typedef uint16_t gunichar2;
struct MonoString
{
    void* monoObjectPart1;
    void* monoObjectPart2;
    gint32 length;
    gunichar2 firstCharacter;
};

struct MonoMethod
{
    uint16_t flags;
    uint16_t iflags;
};

struct GPtrArray
{
    gpointer *pdata;
    guint32 len;
};

typedef enum
{
    MONO_VERIFIER_MODE_OFF,
    MONO_VERIFIER_PE_ONLY,
    MONO_VERIFIER_MODE_VALID,
    MONO_VERIFIER_MODE_VERIFIABLE,
    MONO_VERIFIER_MODE_STRICT
} MiniVerifierMode;

typedef enum
{
    MONO_TYPE_NAME_FORMAT_IL,
    MONO_TYPE_NAME_FORMAT_REFLECTION,
    MONO_TYPE_NAME_FORMAT_FULL_NAME,
    MONO_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED
} MonoTypeNameFormat;

typedef enum
{
    MONO_GC_MODE_DISABLED = 0,
    MONO_GC_MODE_ENABLED = 1,
    MONO_GC_MODE_MANUAL = 2
}  MonoGCMode;

typedef struct
{
    const char *name;
    const char *culture;
    const char *hash_value;
    const uint8_t* public_key;
    // string of 16 hex chars + 1 NULL
    guchar public_key_token[17];
    guint32 hash_alg;
    guint32 hash_len;
    guint32 flags;
    uint16_t major, minor, build, revision;
    // only used and populated by newer Mono
    uint16_t arch;
    uint8_t without_version;
    uint8_t without_culture;
    uint8_t without_public_key_token;
} MonoAssemblyName;

typedef void GFuncRef (void*, void*);
typedef GFuncRef* GFunc;

typedef enum
{
    MONO_UNHANDLED_POLICY_LEGACY,
    MONO_UNHANDLED_POLICY_CURRENT
} MonoRuntimeUnhandledExceptionPolicy;

#if ENABLE_MONO_MEMORY_CALLBACKS
struct MonoMemoryCallbacks;
#endif

// mono/metadata/profiler.h
typedef enum
{
    MONO_PROFILER_CALL_INSTRUMENTATION_NONE = 0,
    MONO_PROFILER_CALL_INSTRUMENTATION_ENTER = 1 << 1,
    MONO_PROFILER_CALL_INSTRUMENTATION_ENTER_CONTEXT = 1 << 2,
    MONO_PROFILER_CALL_INSTRUMENTATION_LEAVE = 1 << 3,
    MONO_PROFILER_CALL_INSTRUMENTATION_LEAVE_CONTEXT = 1 << 4,
    MONO_PROFILER_CALL_INSTRUMENTATION_TAIL_CALL = 1 << 5,
    MONO_PROFILER_CALL_INSTRUMENTATION_EXCEPTION_LEAVE = 1 << 6,
} MonoProfilerCallInstrumentationFlags;

typedef enum
{
    MONO_PROFILER_CODE_BUFFER_METHOD = 0,
    MONO_PROFILER_CODE_BUFFER_METHOD_TRAMPOLINE = 1,
    MONO_PROFILER_CODE_BUFFER_UNBOX_TRAMPOLINE = 2,
    MONO_PROFILER_CODE_BUFFER_IMT_TRAMPOLINE = 3,
    MONO_PROFILER_CODE_BUFFER_GENERICS_TRAMPOLINE = 4,
    MONO_PROFILER_CODE_BUFFER_SPECIFIC_TRAMPOLINE = 5,
    MONO_PROFILER_CODE_BUFFER_HELPER = 6,
    MONO_PROFILER_CODE_BUFFER_MONITOR = 7,
    MONO_PROFILER_CODE_BUFFER_DELEGATE_INVOKE = 8,
    MONO_PROFILER_CODE_BUFFER_EXCEPTION_HANDLING = 9,
} MonoProfilerCodeBufferType;

struct MonoJitInfo
{
    MonoMethod* method;
    void* next_jit_code_hash;
    gpointer code_start;
    guint32 unwind_info;
    int code_size;
};

struct MonoDebugLineNumberEntry
{
    uint32_t il_offset;
    uint32_t native_offset;
};
struct MonoDebugMethodJitInfo
{
    gpointer code_start;
    uint32_t code_size;
    uint32_t prologue_end;
    uint32_t epilogue_begin;
    gpointer wrapper_addr;
    uint32_t num_line_numbers;
    MonoDebugLineNumberEntry *line_numbers;
};

struct MonoDebugSourceLocation
{
    char* source_file;
    uint32_t row;
    uint32_t column;
    uint32_t il_offset;
};

typedef enum
{
    /* the default is to always obey the breakpoint */
    MONO_BREAK_POLICY_ALWAYS,
    /* a nop is inserted instead of a breakpoint */
    MONO_BREAK_POLICY_NEVER,
    /* the breakpoint is executed only if the program has ben started under
     * the debugger (that is if a debugger was attached at the time the method
     * was compiled).
     */
    MONO_BREAK_POLICY_ON_DBG
} MonoBreakPolicy;

typedef MonoBreakPolicy (*MonoBreakPolicyFunc) (MonoMethod *method);

typedef struct
{
    MonoMethod *method;
    uint32_t il_offset;
    uint32_t counter;
    const char *file_name;
    uint32_t line;
    uint32_t column;
} MonoProfilerCoverageData;

struct MonoUnityCallstackFilter
{
    const char* name_space;
    const char* class_name;
    const char* method_name;
};

struct MonoUnityCallstackOptions
{
    const char *path_prefix_filter;
    int filter_count;
    const MonoUnityCallstackFilter *line_filters;
};


/*Keep in sync with MonoErrorInternal*/
typedef struct _MonoError
{
    unsigned short error_code;
    unsigned short hidden_0; /*DON'T TOUCH */

    void *hidden_1[12];  /*DON'T TOUCH */
} MonoError;
