#include "GpuCaptureMetalBridge.h"
#include <Metal/Metal.h>
#include <Metal/MTLCaptureManager.h>
#include <TargetConditionals.h>

#if TARGET_OS_OSX || TARGET_OS_IPHONE

namespace DebugScriptXcode
{

/**
 * Internal implementation structure that holds Objective-C objects
 * This is private to the .mm file, so C++ code doesn't see Objective-C types
 */
struct FMetalImplData
{
    id<MTLDevice>       device    = nil;
    id<MTLCommandQueue> queue     = nil;
    bool                capturing = false;
};

FMetalImplData* CreateMetalImpl()
{
    return new FMetalImplData();
}

void DestroyMetalImpl(FMetalImplData* Impl)
{
    if (Impl)
    {
        Impl->device    = nil;
        Impl->queue     = nil;
        Impl->capturing = false;
        delete Impl;
    }
}

bool StartMetalCapture(FMetalImplData* Impl, void* DeviceHandle)
{
    if (!Impl)
        return false;

    id<MTLDevice> dev = nil;
    id<MTLCommandQueue> q = nil;

    if (DeviceHandle)
    {
        // Assume device is a command queue.
        // If you actually pass a device, you can adjust this logic.
        q = (__bridge id<MTLCommandQueue>)DeviceHandle;
    }

    if (!q && Impl->queue)
        q = Impl->queue;

    if (!dev && Impl->device)
        dev = Impl->device;

    if (!dev && q)
        dev = q.device;

    if (!dev && !q)
        return false;

    MTLCaptureManager* mgr = [MTLCaptureManager sharedCaptureManager];

    MTLCaptureDescriptor* desc = [[MTLCaptureDescriptor alloc] init];
    if (q)
        desc.captureObject = q;   // capture a specific queue
    else
        desc.captureObject = dev; // fallback to device

    NSError* error = nil;
    BOOL ok = [mgr startCaptureWithDescriptor:desc error:&error];
    if (!ok)
    {
        NSLog(@"[GpuCaptureManager] Failed to start Metal capture: %@", error);
        return false;
    }

    Impl->capturing = true;
    return true;
}

void StopMetalCapture(FMetalImplData* Impl)
{
    if (!Impl || !Impl->capturing)
        return;

    MTLCaptureManager* mgr = [MTLCaptureManager sharedCaptureManager];
    [mgr stopCapture];

    Impl->capturing = false;
}

bool IsCapturing(const FMetalImplData* Impl)
{
    return Impl ? Impl->capturing : false;
}

void SetMetalDevice(FMetalImplData* Impl, void* Device)
{
    if (Impl)
    {
        Impl->device = (__bridge id<MTLDevice>)Device;
    }
}

void SetMetalCommandQueue(FMetalImplData* Impl, void* Queue)
{
    if (Impl)
    {
        Impl->queue = (__bridge id<MTLCommandQueue>)Queue;
    }
}

} // namespace DebugScriptXcode

#else // TARGET_OS_OSX || TARGET_OS_IPHONE

// Stub implementations for non-Apple platforms
namespace DebugScriptXcode
{

struct FMetalImplData
{
    bool capturing = false;
};

FMetalImplData* CreateMetalImpl() { return nullptr; }
void DestroyMetalImpl(FMetalImplData* Impl) {}
bool StartMetalCapture(FMetalImplData* Impl, void* DeviceHandle) { return false; }
void StopMetalCapture(FMetalImplData* Impl) {}
bool IsCapturing(const FMetalImplData* Impl) { return false; }
void SetMetalDevice(FMetalImplData* Impl, void* Device) {}
void SetMetalCommandQueue(FMetalImplData* Impl, void* Queue) {}

} // namespace DebugScriptXcode

#endif // TARGET_OS_OSX || TARGET_OS_IPHONE
