// Copyright Epic Games, Inc. All Rights Reserved.

#include "DebugScript/DbgScpHookHelper.h"
#include "DebugScript/DbgScpHook.h"

#include <stdint.h>
#include <string>
#include <array>
#include <unordered_map>
#include <mutex>

static std::unordered_map<std::string, std::string> s_ShaderNameMap;
static std::mutex s_ShaderNameMutex;

static std::string DbgScpLookupOneShaderName(const char* hash)
{
	if (!hash || hash[0] == '\0')
		return hash ? hash : "";
	auto it = s_ShaderNameMap.find(hash);
	if (it != s_ShaderNameMap.end() && !it->second.empty())
		return std::string(hash) + "[" + it->second + "]";
	return hash;
}

static std::string DbgScpLookupShaderName(const char* hash)
{
	if (!hash || hash[0] == '\0')
		return hash ? hash : "";
	std::lock_guard<std::mutex> lock(s_ShaderNameMutex);
	std::string input(hash);
	size_t pos = input.find('+');
	if (pos == std::string::npos)
		return DbgScpLookupOneShaderName(hash);
	std::string result;
	size_t start = 0;
	while (start <= input.size())
	{
		size_t p = input.find('+', start);
		std::string part = (p == std::string::npos) ? input.substr(start) : input.substr(start, p - start);
		if (!result.empty())
			result += "+";
		result += DbgScpLookupOneShaderName(part.c_str());
		if (p == std::string::npos)
			break;
		start = p + 1;
	}
	return result;
}

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

void FDbgScpHookHelper::DbgScpBeforeGLSLToDeviceCompatibleGLSL(const char* glsl, const char* name, uint32_t typeEnum, const void* capabilities)
{
	DBGSCP_HOOK_VOID("DbgScpBeforeGLSLToDeviceCompatibleGLSL", glsl, name, typeEnum, capabilities);
}
void FDbgScpHookHelper::DbgScpAfterGLSLToDeviceCompatibleGLSL(const char* glsl, const char* name, uint32_t typeEnum, const void* capabilities, const char* newGlsl, bool isPlatformExtension)
{
    DBGSCP_HOOK_VOID("DbgScpAfterGLSLToDeviceCompatibleGLSL", glsl, name, typeEnum, capabilities, newGlsl, isPlatformExtension);
}

void FDbgScpHookHelper::DbgScpRegisterShaderName(const char* hash, const char* name)
{
	if (!hash || hash[0] == '\0' || !name || name[0] == '\0')
		return;
	{
		std::lock_guard<std::mutex> lock(s_ShaderNameMutex);
		auto result = s_ShaderNameMap.emplace(hash, name);
		if (!result.second)
			return; // already registered, skip
	}
	UE_LOG(LogTemp, Warning, TEXT("DbgScpRegisterShaderName hash:%s name:%s"), UTF8_TO_TCHAR(hash), UTF8_TO_TCHAR(name));
	DBGSCP_HOOK_VOID("DbgScpRegisterShaderName", hash, name);
}

bool FDbgScpHookHelper::DbgScpShouldUseVulkan(bool bVulkanAvailable, bool bVulkanDisabledCVar)
{
	BEGIN_DBGSCP_HOOK();
	return bVulkanAvailable && !bVulkanDisabledCVar;
	END_DBGSCP_HOOK("DbgScpShouldUseVulkan", bool, bVulkanAvailable, bVulkanDisabledCVar);
}

bool FDbgScpHookHelper::DbgScpShouldUseDesktopVulkan(bool bVulkanSM5Enabled, bool bVulkanSM5Disabled)
{
    BEGIN_DBGSCP_HOOK();
    return bVulkanSM5Enabled && !bVulkanSM5Disabled;
    END_DBGSCP_HOOK("DbgScpShouldUseDesktopVulkan", bool, bVulkanSM5Enabled, bVulkanSM5Disabled);
}

void FDbgScpHookHelper::DbgScpBindShaderBufferGLES(const char* shaderHash, int32_t bindSlot, int64_t bufferId, int64_t offset, int64_t size, const char* bufferType)
{
	std::string hashWithName = DbgScpLookupShaderName(shaderHash);
	const char* h = hashWithName.c_str();
	DBGSCP_HOOK_VOID("DbgScpBindShaderBufferGLES", h, bindSlot, bufferId, offset, size, bufferType);
}

void FDbgScpHookHelper::DbgScpBindShaderBufferVulkan(const char* shaderHash, int64_t bindSlot, int64_t bufferAddr, int64_t offset, int64_t size, const char* bufferType)
{
	std::string hashWithName = DbgScpLookupShaderName(shaderHash);
	const char* h = hashWithName.c_str();
	DBGSCP_HOOK_VOID("DbgScpBindShaderBufferVulkan", h, bindSlot, bufferAddr, offset, size, bufferType);
}

void FDbgScpHookHelper::DbgScpBindShaderBufferD3D12(const char* shaderHash, int64_t bindSlot, int64_t bufferAddr, int64_t offset, int64_t size, const char* bufferType)
{
	std::string hashWithName = DbgScpLookupShaderName(shaderHash);
	const char* h = hashWithName.c_str();
	DBGSCP_HOOK_VOID("DbgScpBindShaderBufferD3D12", h, bindSlot, bufferAddr, offset, size, bufferType);
}

void FDbgScpHookHelper::DbgScpBindShaderBufferMetal(const char* shaderHash, int64_t bindSlot, int64_t bufferAddr, int64_t offset, int64_t size, const char* bufferType)
{
	std::string hashWithName = DbgScpLookupShaderName(shaderHash);
	const char* h = hashWithName.c_str();
	DBGSCP_HOOK_VOID("DbgScpBindShaderBufferMetal", h, bindSlot, bufferAddr, offset, size, bufferType);
}

void FDbgScpHookHelper::DbgScpSetShaderParameterGLES(const char* shaderHash, int32_t shaderFreq, int32_t bindIndex, int64_t resourceAddr, int32_t byteOffset, int32_t byteSize, const char* paramType)
{
DBGSCP_HOOK_VOID("DbgScpSetShaderParameterGLES", shaderHash, shaderFreq, bindIndex, resourceAddr, byteOffset, byteSize, paramType);
}

void FDbgScpHookHelper::DbgScpSetupDrawGLES(const char* shaderHash, int64_t frame, int64_t drawIdx)
{
	std::string hashWithName = DbgScpLookupShaderName(shaderHash);
	const char* h = hashWithName.c_str();
	DBGSCP_HOOK_VOID("DbgScpSetupDrawGLES", h, frame, drawIdx);
}

void FDbgScpHookHelper::DbgScpSetupDispatchGLES(const char* shaderHash, int64_t frame, int64_t dispatchIdx)
{
	std::string hashWithName = DbgScpLookupShaderName(shaderHash);
	const char* h = hashWithName.c_str();
	DBGSCP_HOOK_VOID("DbgScpSetupDispatchGLES", h, frame, dispatchIdx);
}

void FDbgScpHookHelper::DbgScpSetupDrawVulkan(const char* shaderHash, int64_t frame, int64_t drawIdx)
{
	std::string hashWithName = DbgScpLookupShaderName(shaderHash);
	const char* h = hashWithName.c_str();
	DBGSCP_HOOK_VOID("DbgScpSetupDrawVulkan", h, frame, drawIdx);
}

void FDbgScpHookHelper::DbgScpSetupDispatchDrawVulkan(const char* shaderHash, int64_t frame, int64_t drawIdx)
{
	std::string hashWithName = DbgScpLookupShaderName(shaderHash);
	const char* h = hashWithName.c_str();
	DBGSCP_HOOK_VOID("DbgScpSetupDispatchDrawVulkan", h, frame, drawIdx);
}

void FDbgScpHookHelper::DbgScpSetupDispatchVulkan(const char* shaderHash, int64_t frame, int64_t dispatchIdx)
{
	std::string hashWithName = DbgScpLookupShaderName(shaderHash);
	const char* h = hashWithName.c_str();
	DBGSCP_HOOK_VOID("DbgScpSetupDispatchVulkan", h, frame, dispatchIdx);
}

void FDbgScpHookHelper::DbgScpSetupDrawD3D12(const char* shaderHash, int64_t frame, int64_t drawIdx)
{
	std::string hashWithName = DbgScpLookupShaderName(shaderHash);
	const char* h = hashWithName.c_str();
	DBGSCP_HOOK_VOID("DbgScpSetupDrawD3D12", h, frame, drawIdx);
}

void FDbgScpHookHelper::DbgScpSetupDispatchDrawD3D12(const char* shaderHash, int64_t frame, int64_t drawIdx)
{
	std::string hashWithName = DbgScpLookupShaderName(shaderHash);
	const char* h = hashWithName.c_str();
	DBGSCP_HOOK_VOID("DbgScpSetupDispatchDrawD3D12", h, frame, drawIdx);
}

void FDbgScpHookHelper::DbgScpSetupDispatchD3D12(const char* shaderHash, int64_t frame, int64_t dispatchIdx)
{
	std::string hashWithName = DbgScpLookupShaderName(shaderHash);
	const char* h = hashWithName.c_str();
	DBGSCP_HOOK_VOID("DbgScpSetupDispatchD3D12", h, frame, dispatchIdx);
}

void FDbgScpHookHelper::DbgScpSetupDrawMetal(const char* shaderHash, int64_t frame, int64_t drawIdx)
{
	std::string hashWithName = DbgScpLookupShaderName(shaderHash);
	const char* h = hashWithName.c_str();
	DBGSCP_HOOK_VOID("DbgScpSetupDrawMetal", h, frame, drawIdx);
}

void FDbgScpHookHelper::DbgScpSetupDispatchMetal(const char* shaderHash, int64_t frame, int64_t dispatchIdx)
{
	std::string hashWithName = DbgScpLookupShaderName(shaderHash);
	const char* h = hashWithName.c_str();
	DBGSCP_HOOK_VOID("DbgScpSetupDispatchMetal", h, frame, dispatchIdx);
}
