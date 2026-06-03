#include "GpuCaptureManager.h"

#if PLATFORM_MAC || PLATFORM_IOS || PLATFORM_TVOS
#include "GpuCaptureManager_Metal.inl"
#elif defined(__OHOS__)
#include "GpuCaptureManager_HuaweiSquid.inl"
#else
#include "GpuCaptureManager_RenderDoc.inl"
#endif
