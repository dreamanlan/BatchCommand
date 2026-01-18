#pragma once
#include "coreclr/coreclr_delegates.h"

enum SettingType {
    ST_FILE = 0,
    ST_INT,
    ST_FLOAT,
    ST_DOUBLE,
    ST_STRING,
    ST_NUM
};

class QString;
extern void add_log(const QString& msg);
extern void printf_log(const char* fmt, ...);
extern int load_hostfxr(int& out_rc);
extern int load_dotnet_method(int& out_rc);

typedef void (CORECLR_DELEGATE_CALLTYPE* init_csharp_fn)(const char* base_path, void* result);
typedef void (CORECLR_DELEGATE_CALLTYPE* execute_dsl_fn)(const char* dsl, const char* selInTree, const char* selInList, void* result);
typedef int (CORECLR_DELEGATE_CALLTYPE* load_setting_fn)(void* result);
typedef int (CORECLR_DELEGATE_CALLTYPE* load_scheme_menu_fn)(void* result);
typedef int (CORECLR_DELEGATE_CALLTYPE* load_scheme_fn)(const char* path, void* result);
typedef int (CORECLR_DELEGATE_CALLTYPE* execute_command_fn)(const char* cmd_type, const char* cmd_args, const char* selInTree, const char* selInList, void* worker, void* result);
typedef int (CORECLR_DELEGATE_CALLTYPE* run_prog_fn)(const char* selInTree, const char* selInList, void* worker, void* result);
typedef int (CORECLR_DELEGATE_CALLTYPE* build_fn)(const char* selInTree, const char* selInList, void* worker, void* result);
typedef int (CORECLR_DELEGATE_CALLTYPE* install_fn)(const char* selInTree, const char* selInList, void* worker, void* result);

extern init_csharp_fn init_csharp_fptr;
extern execute_dsl_fn execute_dsl_fptr;
extern load_setting_fn load_setting_fptr;
extern load_scheme_menu_fn load_scheme_menu_fptr;
extern load_scheme_fn load_scheme_fptr;
extern execute_command_fn execute_command_fptr;
extern run_prog_fn run_prog_fptr;
extern build_fn build_fptr;
extern install_fn install_fptr;