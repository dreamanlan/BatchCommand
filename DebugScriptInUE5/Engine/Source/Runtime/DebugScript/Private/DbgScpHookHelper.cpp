// Copyright Epic Games, Inc. All Rights Reserved.

#include "DbgScpHookHelper.h"
#include "DbgScpHook.h"

#include <stdint.h>
#include <string>
#include <array>

bool FDbgScpHookHelper::DbgScpBreakpoint(const char* file, int line, const char* func, const char* tag)
{
	DBGSCP_HOOK("DbgScpBreakpoint", bool, tag, file, line, func);
	return true;
}

bool FDbgScpHookHelper::DbgScpBreakpointWithInfo(const char* file, int line, const char* func, const char* tag, const char* fmt, ...)
{
	const int c_buffer_capacity = 4097;
	char buffer[c_buffer_capacity];
	va_list args;
	va_start(args, fmt);
	int num = vsnprintf(buffer, c_buffer_capacity, fmt, args);
    // Guard against vsnprintf returning negative (error) or exceeding buffer size
    if (num < 0) {
        num = 0;
    } else if (num >= static_cast<int>(sizeof(buffer) - 1)) {
        num = static_cast<int>(sizeof(buffer) - 1);
    }
	buffer[num]	= '\0';
	va_end(args);

	const char* info = buffer;
	DBGSCP_HOOK("DbgScpBreakpointWithInfo", bool, tag, info, file, line, func);
	return true;
}

bool FDbgScpHookHelper::DbgScpBreakpointWithIntKey(const char* file, int line, const char* func, const char* tag, int64_t key)
{
	DBGSCP_HOOK("DbgScpBreakpointWithIntKey", bool, tag, key, file, line, func);
	return true;
}

bool FDbgScpHookHelper::DbgScpBreakpointWithIntKeyInfo(const char* file, int line, const char* func, const char* tag, int64_t key, const char* fmt, ...)
{
	const int c_buffer_capacity = 4097;
	char buffer[c_buffer_capacity];
	va_list args;
	va_start(args, fmt);
	int num = vsnprintf(buffer, c_buffer_capacity, fmt, args);
    // Guard against vsnprintf returning negative (error) or exceeding buffer size
    if (num < 0) {
        num = 0;
    } else if (num >= static_cast<int>(sizeof(buffer) - 1)) {
        num = static_cast<int>(sizeof(buffer) - 1);
    }
	buffer[num]	= '\0';
	va_end(args);

	const char* info = buffer;
	DBGSCP_HOOK("DbgScpBreakpointWithIntKeyInfo", bool, tag, key, info, file, line, func);
	return true;
}

bool FDbgScpHookHelper::DbgScpBreakpointWithStrKey(const char* file, int line, const char* func, const char* tag, const char* key)
{
	DBGSCP_HOOK("DbgScpBreakpointWithStrKey", bool, tag, key, file, line, func);
	return true;
}

bool FDbgScpHookHelper::DbgScpBreakpointWithStrKeyInfo(const char* file, int line, const char* func, const char* tag, const char* key, const char* fmt, ...)
{
	const int c_buffer_capacity = 4097;
	char buffer[c_buffer_capacity];
	va_list args;
	va_start(args, fmt);
	int num = vsnprintf(buffer, c_buffer_capacity, fmt, args);
    // Guard against vsnprintf returning negative (error) or exceeding buffer size
    if (num < 0) {
        num = 0;
    } else if (num >= static_cast<int>(sizeof(buffer) - 1)) {
        num = static_cast<int>(sizeof(buffer) - 1);
    }
	buffer[num]	= '\0';
	va_end(args);

	const char* info = buffer;
	DBGSCP_HOOK("DbgScpBreakpointWithStrKeyInfo", bool, tag, key, info, file, line, func);
	return true;
}

bool FDbgScpHookHelper::DbgScpAssert(const char* file, int line, const char* func, const char* tag, bool pred)
{
	if (pred)
		return true;
	DBGSCP_HOOK("DbgScpAssert", bool, tag, file, line, func);
	return true;
}

bool FDbgScpHookHelper::DbgScpAssertWithInfo(const char* file, int line, const char* func, const char* tag, bool pred, const char* fmt, ...)
{
	if (pred)
		return true;
	const int c_buffer_capacity = 4097;
	char buffer[c_buffer_capacity];
	va_list args;
	va_start(args, fmt);
	int num = vsnprintf(buffer, c_buffer_capacity, fmt, args);
    // Guard against vsnprintf returning negative (error) or exceeding buffer size
    if (num < 0) {
        num = 0;
    } else if (num >= static_cast<int>(sizeof(buffer) - 1)) {
        num = static_cast<int>(sizeof(buffer) - 1);
    }
	buffer[num]	= '\0';
	va_end(args);

	const char* info = buffer;
	DBGSCP_HOOK("DbgScpAssertWithInfo", bool, tag, info, file, line, func);
	return true;
}

bool FDbgScpHookHelper::DbgScpAssertWithIntKey(const char* file, int line, const char* func, const char* tag, int64_t key, bool pred)
{
	if (pred)
		return true;
	DBGSCP_HOOK("DbgScpAssertWithIntKey", bool, tag, key, file, line, func);
	return true;
}

bool FDbgScpHookHelper::DbgScpAssertWithIntKeyInfo(const char* file, int line, const char* func, const char* tag, int64_t key, bool pred, const char* fmt, ...)
{
	if (pred)
		return true;
	const int c_buffer_capacity = 4097;
	char buffer[c_buffer_capacity];
	va_list args;
	va_start(args, fmt);
	int num = vsnprintf(buffer, c_buffer_capacity, fmt, args);
    // Guard against vsnprintf returning negative (error) or exceeding buffer size
    if (num < 0) {
        num = 0;
    } else if (num >= static_cast<int>(sizeof(buffer) - 1)) {
        num = static_cast<int>(sizeof(buffer) - 1);
    }
	buffer[num]	= '\0';
	va_end(args);

	const char* info = buffer;
	DBGSCP_HOOK("DbgScpAssertWithIntKeyInfo", bool, tag, key, info, file, line, func);
	return true;
}

bool FDbgScpHookHelper::DbgScpAssertWithStrKey(const char* file, int line, const char* func, const char* tag, const char* key, bool pred)
{
	if (pred)
		return true;
	DBGSCP_HOOK("DbgScpAssertWithStrKey", bool, tag, key, file, line, func);
	return true;
}

bool FDbgScpHookHelper::DbgScpAssertWithStrKeyInfo(const char* file, int line, const char* func, const char* tag, const char* key, bool pred, const char* fmt, ...)
{
	if (pred)
		return true;
	const int c_buffer_capacity = 4097;
	char buffer[c_buffer_capacity];
	va_list args;
	va_start(args, fmt);
	int num = vsnprintf(buffer, c_buffer_capacity, fmt, args);
    // Guard against vsnprintf returning negative (error) or exceeding buffer size
    if (num < 0) {
        num = 0;
    } else if (num >= static_cast<int>(sizeof(buffer) - 1)) {
        num = static_cast<int>(sizeof(buffer) - 1);
    }
	buffer[num]	= '\0';
	va_end(args);

	const char* info = buffer;
	DBGSCP_HOOK("DbgScpAssertWithStrKeyInfo", bool, tag, key, info, file, line, func);
	return true;
}

void FDbgScpHookHelper::DbgScpBindShaderBufferGLES(const char* shaderHash, int32_t bindSlot, int64_t bufferId, int64_t offset, int64_t size, const char* bufferType)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderBufferGLES", shaderHash, bindSlot, bufferId, offset, size, bufferType);
}

void FDbgScpHookHelper::DbgScpBindShaderBufferVulkan(const char* shaderHash, int64_t bindSlot, int64_t bufferAddr, int64_t offset, int64_t size, const char* bufferType)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderBufferVulkan", shaderHash, bindSlot, bufferAddr, offset, size, bufferType);
}

void FDbgScpHookHelper::DbgScpBindShaderBufferD3D12(const char* shaderHash, int64_t bindSlot, int64_t bufferAddr, int64_t offset, int64_t size, const char* bufferType)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderBufferD3D12", shaderHash, bindSlot, bufferAddr, offset, size, bufferType);
}

void FDbgScpHookHelper::DbgScpBindShaderBufferD3D11(const char* shaderHash, int32_t bindSlot, int64_t bufferAddr, int64_t offset, int64_t size, const char* bufferType)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderBufferD3D11", shaderHash, bindSlot, bufferAddr, offset, size, bufferType);
}

void FDbgScpHookHelper::DbgScpBindShaderBufferMetal(const char* shaderHash, int64_t bindSlot, int64_t bufferAddr, int64_t offset, int64_t size, const char* bufferType)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderBufferMetal", shaderHash, bindSlot, bufferAddr, offset, size, bufferType);
}

void FDbgScpHookHelper::DbgScpBindShaderTextureGLES(const char* shaderHash, int32_t bindSlot, int64_t textureAddr)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderTextureGLES", shaderHash, bindSlot, textureAddr);
}

void FDbgScpHookHelper::DbgScpBindShaderTextureVulkan(const char* shaderHash, int64_t bindSlot, int64_t textureAddr)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderTextureVulkan", shaderHash, bindSlot, textureAddr);
}

void FDbgScpHookHelper::DbgScpBindShaderTextureD3D12(const char* shaderHash, int64_t bindSlot, int64_t textureAddr)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderTextureD3D12", shaderHash, bindSlot, textureAddr);
}

void FDbgScpHookHelper::DbgScpBindShaderTextureD3D11(const char* shaderHash, int32_t bindSlot, int64_t textureAddr)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderTextureD3D11", shaderHash, bindSlot, textureAddr);
}

void FDbgScpHookHelper::DbgScpBindShaderTextureMetal(const char* shaderHash, int64_t bindSlot, int64_t textureAddr)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderTextureMetal", shaderHash, bindSlot, textureAddr);
}

void FDbgScpHookHelper::DbgScpBindShaderSamplerGLES(const char* shaderHash, int32_t bindSlot, int64_t samplerAddr)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderSamplerGLES", shaderHash, bindSlot, samplerAddr);
}

void FDbgScpHookHelper::DbgScpBindShaderSamplerVulkan(const char* shaderHash, int64_t bindSlot, int64_t samplerAddr)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderSamplerVulkan", shaderHash, bindSlot, samplerAddr);
}

void FDbgScpHookHelper::DbgScpBindShaderSamplerD3D12(const char* shaderHash, int64_t bindSlot, int64_t samplerAddr)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderSamplerD3D12", shaderHash, bindSlot, samplerAddr);
}

void FDbgScpHookHelper::DbgScpBindShaderSamplerD3D11(const char* shaderHash, int32_t bindSlot, int64_t samplerAddr)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderSamplerD3D11", shaderHash, bindSlot, samplerAddr);
}

void FDbgScpHookHelper::DbgScpBindShaderSamplerMetal(const char* shaderHash, int64_t bindSlot, int64_t samplerAddr)
{
	DBGSCP_HOOK_VOID("DbgScpBindShaderSamplerMetal", shaderHash, bindSlot, samplerAddr);
}