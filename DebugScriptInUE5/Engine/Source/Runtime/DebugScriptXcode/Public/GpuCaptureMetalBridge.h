#pragma once

#ifdef __OBJC__
@class MTLDevice;
@class MTLCommandQueue;
#else
// Forward declarations for C++ compilation
typedef struct objc_object MTLDevice;
typedef struct objc_object MTLCommandQueue;
#endif

namespace DebugScriptXcode
{

/**
 * Metal capture implementation data structure
 * Opaque pointer type to hide Objective-C details from C++ code
 */
struct FMetalImplData;

/**
 * Create a new Metal implementation instance
 * @return Opaque pointer to the implementation data, or nullptr on failure
 */
FMetalImplData* CreateMetalImpl();

/**
 * Destroy a Metal implementation instance and release all resources
 * @param Impl The implementation data pointer to destroy
 */
void DestroyMetalImpl(FMetalImplData* Impl);

/**
 * Start a Metal GPU capture using MTLCaptureManager
 * @param Impl The implementation data pointer
 * @param DeviceHandle The device or command queue handle (id<MTLDevice> or id<MTLCommandQueue>)
 * @return true if capture started successfully, false otherwise
 */
bool StartMetalCapture(FMetalImplData* Impl, void* DeviceHandle);

/**
 * Stop the currently active Metal capture
 * @param Impl The implementation data pointer
 */
void StopMetalCapture(FMetalImplData* Impl);

/**
 * Check if a Metal capture is currently active
 * @param Impl The implementation data pointer
 * @return true if capturing, false otherwise
 */
bool IsCapturing(const FMetalImplData* Impl);

/**
 * Set the default Metal device for capture
 * @param Impl The implementation data pointer
 * @param Device The MTLDevice pointer
 */
void SetMetalDevice(FMetalImplData* Impl, void* Device);

/**
 * Set the default Metal command queue for capture
 * @param Impl The implementation data pointer
 * @param Queue The MTLCommandQueue pointer
 */
void SetMetalCommandQueue(FMetalImplData* Impl, void* Queue);

} // namespace DebugScriptXcode
