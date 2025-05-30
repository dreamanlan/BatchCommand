#pragma once

#include "MonoTypes.h"
#include "MonoTypeSignatures.h"
#include "MonoNativeTypes.h"

#ifndef __cplusplus
#error somewhat unexpected
#endif


#pragma once
#include "MonoTypes.h"
#include "MonoNativeTypes.h"

#if BUILDING_COREMODULE
#define EXPORT_COREMODULE __declspec(dllexport)
#else
#define EXPORT_COREMODULE __declspec(dllimport)
#endif

extern "C"
{

#define mono_string_chars(s) ((gunichar2*)&((s)->firstCharacter))
#define mono_string_length(s) ((s)->length)

#define DO_API(r, n, p)   extern EXPORT_COREMODULE r (*n) p;

#if !defined(_MSC_VER) // visual studio supports __declspec(noreturn) only in definition/declaration (not in typedef or function pointer)
    #define DO_API_NO_RETURN(r, n, p) extern EXPORT_COREMODULE DOES_NOT_RETURN r (*n) p;
#endif
#include "MonoFunctions.h"

}
