#include "GpuCaptureManager.h"

#if PLATFORM_MAC || PLATFORM_IOS || PLATFORM_TVOS
#include "GpuCaptureManager_Metal.inl"
#else
#include "GpuCaptureManager_RenderDoc.inl"
#endif
