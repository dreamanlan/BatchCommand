// Copyright Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"

#include <stdint.h>
#include <string>
#include <array>

class CORE_API FDbgScpHookHelper
{
public://common hook functions
    static bool DbgScpBreakpoint(const char* file, int line, const char* func, const char* tag);
	static bool DbgScpBreakpointWithInfo(const char* file, int line, const char* func, const char* tag, const char* fmt, ...);
	static bool DbgScpBreakpointWithIntKey(const char* file, int line, const char* func, const char* tag, int64_t key);
	static bool DbgScpBreakpointWithIntKeyInfo(const char* file, int line, const char* func, const char* tag, int64_t key, const char* fmt, ...);
	static bool DbgScpBreakpointWithStrKey(const char* file, int line, const char* func, const char* tag, const char* key);
	static bool DbgScpBreakpointWithStrKeyInfo(const char* file, int line, const char* func, const char* tag, const char* key, const char* fmt, ...);
	static bool DbgScpAssert(const char* file, int line, const char* func, const char* tag, bool pred);
	static bool DbgScpAssertWithInfo(const char* file, int line, const char* func, const char* tag, bool pred, const char* fmt, ...);
	static bool DbgScpAssertWithIntKey(const char* file, int line, const char* func, const char* tag, int64_t key, bool pred);
	static bool DbgScpAssertWithIntKeyInfo(const char* file, int line, const char* func, const char* tag, int64_t key, bool pred, const char* fmt, ...);
	static bool DbgScpAssertWithStrKey(const char* file, int line, const char* func, const char* tag, const char* key, bool pred);
	static bool DbgScpAssertWithStrKeyInfo(const char* file, int line, const char* func, const char* tag, const char* key, bool pred, const char* fmt, ...);
public://shader info
	static void DbgScpBeforeGLSLToDeviceCompatibleGLSL(const char* glsl, const char* name, uint32_t typeEnum, const void* capabilities);
	static void DbgScpAfterGLSLToDeviceCompatibleGLSL(const char* glsl, const char* name, uint32_t typeEnum, const void* capabilities, const char* newGlsl, bool isPlatformExtension);
	static void DbgScpRegisterShaderName(const char* hash, const char* name);
public://special hook functions
	static bool DbgScpShouldUseVulkan(bool bVulkanAvailable, bool bVulkanDisabledCVar);
	static bool DbgScpShouldUseDesktopVulkan(bool bVulkanSM5Enabled, bool bVulkanSM5Disabled);
	static void DbgScpBindShaderBufferGLES(const char* shaderHash, int32_t bindSlot, int64_t bufferId, int64_t offset, int64_t size, const char* bufferType);
	static void DbgScpBindShaderBufferVulkan(const char* shaderHash, int64_t bindSlot, int64_t bufferAddr, int64_t offset, int64_t size, const char* bufferType);
	static void DbgScpBindShaderBufferD3D12(const char* shaderHash, int64_t bindSlot, int64_t bufferAddr, int64_t offset, int64_t size, const char* bufferType);
	static void DbgScpBindShaderBufferMetal(const char* shaderHash, int64_t bindSlot, int64_t bufferAddr, int64_t offset, int64_t size, const char* bufferType);
	static void DbgScpSetShaderParameterGLES(const char* shaderHash, int32_t shaderFreq, int32_t bindIndex, int64_t resourceAddr, int32_t byteOffset, int32_t byteSize, const char* paramType);
	static void DbgScpSetupDrawGLES        (const char* shaderHash, int64_t frame, int64_t drawIdx);
	static void DbgScpSetupDispatchGLES        (const char* shaderHash, int64_t frame, int64_t dispatchIdx);
	static void DbgScpSetupDrawVulkan          (const char* shaderHash, int64_t frame, int64_t drawIdx);
	static void DbgScpSetupDispatchDrawVulkan  (const char* shaderHash, int64_t frame, int64_t drawIdx);
	static void DbgScpSetupDispatchVulkan      (const char* shaderHash, int64_t frame, int64_t dispatchIdx);
	static void DbgScpSetupDrawD3D12           (const char* shaderHash, int64_t frame, int64_t drawIdx);
	static void DbgScpSetupDispatchDrawD3D12   (const char* shaderHash, int64_t frame, int64_t drawIdx);
	static void DbgScpSetupDispatchD3D12       (const char* shaderHash, int64_t frame, int64_t dispatchIdx);
	static void DbgScpSetupDrawMetal           (const char* shaderHash, int64_t frame, int64_t drawIdx);
	static void DbgScpSetupDispatchMetal       (const char* shaderHash, int64_t frame, int64_t dispatchIdx);
};
