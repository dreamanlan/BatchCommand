// Copyright Epic Games, Inc. All Rights Reserved.

#include "DbgScpHookHelper.h"
#include "DbgScpHook.h"

#include <stdint.h>
#include <string>
#include <array>

extern void EndFrameCaptureIfCapturing();
void FDbgScpHookHelper::EndFrameCaptureIfCapturing()
{
	::EndFrameCaptureIfCapturing();
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
	buffer[num]	= '\0';
	va_end(args);

	const char* info = buffer;
	DBGSCP_HOOK("DbgScpAssertWithStrKeyInfo", bool, tag, key, info, file, line, func);
	return true;
}