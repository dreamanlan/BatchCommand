#pragma once

#include <stddef.h>
#include <stdint.h>

class ScriptingObjectPtr;
class ScriptingStringPtr;
class ScriptingArrayPtr;
class ScriptingExceptionPtr;
class ScriptingSystemTypeObjectPtr;

class ICallString;
class ScriptingArrayPtrReference;
class ScriptingStringPtrReference;
class ScriptingObjectPtrReference;

#define REFERENCE_TYPE(x) x

#include "MonoTypeSignatures.h"

#define SCRIPTING_TYPE_VOID           MONO_TYPE_VOID
#define SCRIPTING_TYPE_VALUETYPE      MONO_TYPE_VALUETYPE
#define SCRIPTING_TYPE_STRING         MONO_TYPE_STRING
#define SCRIPTING_TYPE_CLASS          MONO_TYPE_CLASS
#define SCRIPTING_TYPE_CHAR           MONO_TYPE_CHAR
#define SCRIPTING_TYPE_I              MONO_TYPE_I
#define SCRIPTING_TYPE_I1             MONO_TYPE_I1
#define SCRIPTING_TYPE_I2             MONO_TYPE_I2
#define SCRIPTING_TYPE_I4             MONO_TYPE_I4
#define SCRIPTING_TYPE_I8             MONO_TYPE_I8
#define SCRIPTING_TYPE_R4             MONO_TYPE_R4
#define SCRIPTING_TYPE_BOOLEAN        MONO_TYPE_BOOLEAN
#define SCRIPTING_TYPE_U1             MONO_TYPE_U1
#define SCRIPTING_TYPE_U2             MONO_TYPE_U2
#define SCRIPTING_TYPE_U4             MONO_TYPE_U4
#define SCRIPTING_TYPE_U8             MONO_TYPE_U8
#define SCRIPTING_TYPE_R8             MONO_TYPE_R8
#define SCRIPTING_TYPE_OBJECT         MONO_TYPE_OBJECT
#define SCRIPTING_TYPE_SZARRAY        MONO_TYPE_SZARRAY
#define SCRIPTING_TYPE_GENERICINST    MONO_TYPE_GENERICINST
#define SCRIPTING_TYPE_PTR              MONO_TYPE_PTR

struct MonoType;
struct MonoClass;
struct MonoAssembly;
struct MonoImage;
struct MonoArray;
struct MonoObject;
struct MonoString;
struct MonoMethod;
struct MonoProperty;
struct MonoDomain;
struct MonoThread;
struct MonoException;
struct MonoClassField;
struct MonoMethodSignature;
struct MonoCustomAttrInfo;

typedef REFERENCE_TYPE (MonoArray*)                ScriptingBackendNativeArrayPtr;
typedef REFERENCE_TYPE (MonoObject*)               ScriptingBackendNativeObjectPtr;
typedef REFERENCE_TYPE (MonoString*)               ScriptingBackendNativeStringPtr;
typedef REFERENCE_TYPE (MonoException*)            ScriptingBackendNativeExceptionPtr;
typedef REFERENCE_TYPE (MonoThread*)               ScriptingBackendNativeThreadPtr;
typedef MonoType*                                 ScriptingBackendNativeTypePtr;
typedef MonoAssembly*                             ScriptingBackendNativeAssemblyPtr;
typedef MonoImage*                                ScriptingBackendNativeImagePtr;
typedef MonoClass*                                ScriptingBackendNativeClassPtr;
typedef MonoDomain*                               ScriptingBackendNativeDomainPtr;
typedef MonoMethod*                               ScriptingBackendNativeMethodPtr;
typedef MonoClassField*                           ScriptingBackendNativeFieldPtr;
typedef MonoProperty*                             ScriptingBackendNativePropertyPtr;
typedef MonoCustomAttrInfo*                       ScriptingCustomAttrInfoPtr;
typedef ScriptingBackendNativeObjectPtr           ScriptingBackendNativeSystemTypeObjectPtr;
typedef MonoMethodSignature*                      ScriptingMethodSignaturePtr;
typedef void*                                     ScriptingParams;
typedef unsigned char                             ScriptingBool;
typedef ScriptingParams*                          ScriptingParamsPtr;
typedef ScriptingBackendNativeStringPtr           ICallStringHandle;
typedef uint32_t                                  ScriptingBackendNativeArrayLength;

typedef MonoObject* (*FastMonoMethod) (void* thiz, MonoException** ex);
typedef void (*mono_register_object_callback)(ScriptingBackendNativeObjectPtr* arr, int size, void* userdata);
typedef void*(*mono_liveness_reallocate_callback)(void* ptr, size_t size, void* state);

typedef mono_register_object_callback register_object_callback;


typedef ScriptingObjectPtr                       ICallType_Object_Local;
typedef ScriptingBackendNativeObjectPtr*         ICallType_Object_Argument_Out;
typedef ScriptingObjectPtrReference              ICallType_Object_Local_Out;
typedef ScriptingBackendNativeObjectPtr*         ICallType_Object_Argument_Ref;
typedef ScriptingObjectPtrReference              ICallType_Object_Local_Ref;

typedef ICallString                              ICallType_String_Local;
typedef ScriptingBackendNativeStringPtr*         ICallType_String_Argument_Out;
typedef ScriptingStringPtrReference              ICallType_String_Local_Out;
typedef ScriptingBackendNativeStringPtr*         ICallType_String_Argument_Ref;
typedef ScriptingStringPtrReference              ICallType_String_Local_Ref;

typedef ScriptingArrayPtr                        ICallType_Array_Local;
typedef ScriptingBackendNativeArrayPtr*          ICallType_Array_Argument_Out;
typedef ScriptingArrayPtrReference               ICallType_Array_Local_Out;
typedef ScriptingBackendNativeArrayPtr*          ICallType_Array_Argument_Ref;
typedef ScriptingArrayPtrReference               ICallType_Array_Local_Ref;

typedef ScriptingExceptionPtr                    ICallType_Exception_Local;

typedef void*                                    ICallType_IntPtr_Argument;
typedef void*                                    ICallType_IntPtr_Return;
typedef void**                                   ICallType_IntPtr_Argument_Out;

typedef ScriptingBackendNativeObjectPtr          ICallType_StructMemberObject;

#define DefineOpaquePointerType(_backingType, _typeName) typedef _backingType _typeName;

DefineOpaquePointerType(ScriptingBackendNativeObjectPtr           , ICallType_Object_Return);
DefineOpaquePointerType(ScriptingBackendNativeStringPtr           , ICallType_String_Return);
DefineOpaquePointerType(ScriptingBackendNativeArrayPtr            , ICallType_Array_Return);
DefineOpaquePointerType(ScriptingBackendNativeExceptionPtr        , ICallType_Exception_Return);
DefineOpaquePointerType(ScriptingBackendNativeObjectPtr           , ICallType_SystemTypeObject_Return);

DefineOpaquePointerType(ScriptingBackendNativeObjectPtr           , ICallType_SystemTypeObject_Argument);
DefineOpaquePointerType(ScriptingBackendNativeArrayPtr            , ICallType_Array_Argument);
DefineOpaquePointerType(ScriptingBackendNativeObjectPtr           , ICallType_Object_Argument);
DefineOpaquePointerType(ScriptingBackendNativeStringPtr           , ICallType_String_Argument);
DefineOpaquePointerType(ScriptingBackendNativeObjectPtr           , ICallType_ReadOnlyUnityEngineObject_Argument);
DefineOpaquePointerType(ScriptingBackendNativeExceptionPtr        , ICallType_Exception_Argument);

#define ICallType_Array_Return_Generic(x) ICallType_Array_Return
#define ICallType_Array_Local_Generic(x) ICallType_Array_Local
#define ICallType_Array_Local_Out_Generic(x) ICallType_Array_Local_Out
#define ICallType_Array_Local_Ref_Generic(x) ICallType_Array_Local_Ref
#define ICallType_Array_Argument_Generic(x) ICallType_Array_Argument
#define ICallType_Array_Argument_Out_Generic(x) ICallType_Array_Argument_Out
#define ICallType_Array_Argument_Ref_Generic(x) ICallType_Array_Argument_Ref

struct ScriptingObjectNull
{
    ScriptingObjectNull() {}

    operator ScriptingBackendNativeObjectPtr() const { return ScriptingBackendNativeObjectPtr(); }
    operator ScriptingBackendNativeStringPtr() const { return ScriptingBackendNativeStringPtr(); }
    operator ScriptingBackendNativeArrayPtr() const { return ScriptingBackendNativeArrayPtr(); }
    operator ScriptingBackendNativeClassPtr() const { return NULL; }
    operator ScriptingBackendNativeExceptionPtr() const { return ScriptingBackendNativeExceptionPtr(); }
    operator ScriptingBackendNativeFieldPtr() const { return NULL; }
    operator ScriptingBackendNativeTypePtr() const { return NULL; }
    operator ScriptingBackendNativeDomainPtr() const { return NULL; }
    operator ScriptingBackendNativeImagePtr() const { return NULL; }
    operator ScriptingBackendNativeMethodPtr() const { return NULL; }
    operator ScriptingBackendNativeThreadPtr() const { return NULL; }
    operator ScriptingCustomAttrInfoPtr() const { return NULL; }

    bool operator==(const ScriptingBackendNativeObjectPtr& obj) const { return obj == ScriptingObjectNull(); }
    bool operator!=(const ScriptingBackendNativeObjectPtr& obj) const { return obj != ScriptingObjectNull(); }
};

extern const ScriptingObjectNull g_ScriptingObjectNull;
#define SCRIPTING_NULL g_ScriptingObjectNull

namespace ScriptingNativeBackend
{
    inline ScriptingBackendNativeObjectPtr ToObject(const ScriptingBackendNativeArrayPtr &obj) { return (ScriptingBackendNativeObjectPtr)obj; }
    inline ScriptingBackendNativeObjectPtr ToObject(const ScriptingBackendNativeExceptionPtr &obj) { return (ScriptingBackendNativeObjectPtr)obj; }
    inline ScriptingBackendNativeArrayPtr ToArray(const ScriptingBackendNativeObjectPtr &obj) { return (ScriptingBackendNativeArrayPtr)obj; }
    inline ScriptingBackendNativeThreadPtr ToThread(const ScriptingBackendNativeObjectPtr &obj) { return (ScriptingBackendNativeThreadPtr)obj; }
    inline ScriptingBackendNativeExceptionPtr ToException(const ScriptingBackendNativeObjectPtr &obj) { return (ScriptingBackendNativeExceptionPtr)obj; }
    inline ScriptingBackendNativeStringPtr ToString(const ScriptingBackendNativeObjectPtr &obj) { return (ScriptingBackendNativeStringPtr)obj; }
    inline ScriptingBackendNativeObjectPtr ToObject(const ScriptingBackendNativeStringPtr &obj) { return (ScriptingBackendNativeObjectPtr)obj; }
    inline ScriptingBackendNativeObjectPtr ToObject(void* obj) { return (ScriptingBackendNativeObjectPtr)obj; }
    inline void* ToPtr(ScriptingBackendNativeArrayPtr obj) { return obj; }
    inline void* ToPtr(ScriptingBackendNativeObjectPtr obj) { return obj; }
    inline void* ToPtr(ScriptingBackendNativeStringPtr obj) { return obj; }
}
