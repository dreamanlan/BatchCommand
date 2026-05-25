// Copyright Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"

#include <stdint.h>
#include <string>
#include <array>

class DEBUGSCRIPT_API FDbgScpHookHelper
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
public://special hook functions
	static void DbgScpBindShaderBufferGLES(const char* shaderHash, int32_t bindSlot, int64_t bufferId, int64_t offset, int64_t size, const char* bufferType);
	static void DbgScpBindShaderBufferVulkan(const char* shaderHash, int64_t bindSlot, int64_t bufferAddr, int64_t offset, int64_t size, const char* bufferType);
	static void DbgScpBindShaderBufferD3D12(const char* shaderHash, int64_t bindSlot, int64_t bufferAddr, int64_t offset, int64_t size, const char* bufferType);
	static void DbgScpBindShaderBufferD3D11(const char* shaderHash, int32_t bindSlot, int64_t bufferAddr, int64_t offset, int64_t size, const char* bufferType);
	static void DbgScpBindShaderBufferMetal(const char* shaderHash, int64_t bindSlot, int64_t bufferAddr, int64_t offset, int64_t size, const char* bufferType);
	static void DbgScpBindShaderTextureGLES(const char* shaderHash, int32_t bindSlot, int64_t textureAddr);
	static void DbgScpBindShaderTextureVulkan(const char* shaderHash, int64_t bindSlot, int64_t textureAddr);
	static void DbgScpBindShaderTextureD3D12(const char* shaderHash, int64_t bindSlot, int64_t textureAddr);
	static void DbgScpBindShaderTextureD3D11(const char* shaderHash, int32_t bindSlot, int64_t textureAddr);
	static void DbgScpBindShaderTextureMetal(const char* shaderHash, int64_t bindSlot, int64_t textureAddr);
	static void DbgScpBindShaderSamplerGLES(const char* shaderHash, int32_t bindSlot, int64_t samplerAddr);
	static void DbgScpBindShaderSamplerVulkan(const char* shaderHash, int64_t bindSlot, int64_t samplerAddr);
	static void DbgScpBindShaderSamplerD3D12(const char* shaderHash, int64_t bindSlot, int64_t samplerAddr);
	static void DbgScpBindShaderSamplerD3D11(const char* shaderHash, int32_t bindSlot, int64_t samplerAddr);
	static void DbgScpBindShaderSamplerMetal(const char* shaderHash, int64_t bindSlot, int64_t samplerAddr);
};
