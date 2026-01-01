#if TARGET_OS_OSX || TARGET_OS_IPHONE

#include "GpuCaptureMetalBridge.h"

struct GpuCaptureManager::MetalImpl
{
    void* implData = nullptr; // Opaque pointer to DebugScriptXcode::FMetalImplData
};

// Override Init for Metal backend on Apple platforms
bool GpuCaptureManager::Init(GpuCaptureBackend backend, const std::string& modulePath)
{
    if (backend == GpuCaptureBackend::MetalXcode)
    {
        Shutdown(); // cleanup any previous backend

        m_backend = backend;
        m_metal   = new MetalImpl();

        // Create the Metal implementation through the bridge API
        m_metal->implData = DebugScriptXcode::CreateMetalImpl();
        
        // You can choose to create device/queue here, or let external code pass them via StartFrameCapture
        // Example (optional):
        // id<MTLDevice> device = MTLCreateSystemDefaultDevice();
        // id<MTLCommandQueue> queue = [device newCommandQueue];
        // DebugScriptXcode::SetMetalDevice(m_metal->implData, (__bridge void*)device);
        // DebugScriptXcode::SetMetalCommandQueue(m_metal->implData, (__bridge void*)queue);

        m_available = true;
        (void)modulePath;
        return true;
    }

    // For non-Metal backend, RenderDoc Init will handle it in the .cpp
    return false;
}



void GpuCaptureManager::Shutdown()
{
	// Release Metal backend
	if (m_metal)
	{
		DebugScriptXcode::DestroyMetalImpl(static_cast<DebugScriptXcode::FMetalImplData*>(m_metal->implData));
		m_metal->implData = nullptr;
		delete m_metal;
		m_metal = nullptr;
	}


	// RenderDoc backend will be released in the RenderDoc .cpp file if used.
	if (m_rd)
	{
		// Here we just null the pointer; actual FreeLibrary/dlclose is in RenderDoc .cpp
		// It is safe because Shutdown() in RenderDoc file will be called when using that backend.
	}

	m_available = false;
	m_backend   = GpuCaptureBackend::None;
}


// TriggerCapture for Metal can be implemented as "capture next frame" if you like.
// Here we leave it empty by default and recommend using explicit Start/End.
void GpuCaptureManager::TriggerCapture()
{
    if (!m_available)
        return;

    if (m_backend == GpuCaptureBackend::MetalXcode)
    {
        // You can implement a "capture next frame" flag here.
        // For simplicity, we do nothing. External code should call
        // StartFrameCapture / EndFrameCapture explicitly.
    }
}

// Start a Metal capture using MTLCaptureManager.
// DeviceHandle is expected to be either:
// - id<MTLCommandQueue>, or
// - id<MTLDevice> (less recommended; queue is better).
bool GpuCaptureManager::StartFrameCapture(DeviceHandle device, WindowHandle window)
{
	if (!m_available || m_backend != GpuCaptureBackend::MetalXcode)
		return false;

	if (!m_metal->implData)
		return false;

	bool result = DebugScriptXcode::StartMetalCapture(
		static_cast<DebugScriptXcode::FMetalImplData*>(m_metal->implData),
		device
	);
	
	(void)window;
	return result;
}



bool GpuCaptureManager::EndFrameCapture(DeviceHandle device, WindowHandle window)
{
	if (!m_available || m_backend != GpuCaptureBackend::MetalXcode)
		return false;

	if (!m_metal->implData)
		return false;

	DebugScriptXcode::StopMetalCapture(static_cast<DebugScriptXcode::FMetalImplData*>(m_metal->implData));
	
	(void)device;
	(void)window;
	return true;
}



// RenderDoc-only options: no-op for Metal backend.
void GpuCaptureManager::SetRenderDocCaptureOptionU32(int opt, uint32_t val)
{
    (void)opt;
    (void)val;
}

void GpuCaptureManager::SetRenderDocCaptureOptionF32(int opt, float val)
{
    (void)opt;
    (void)val;
}

void GpuCaptureManager::SetRenderDocLogFilePathTemplate(const char* pathTemplate)
{
    (void)pathTemplate;
}

#endif // TARGET_OS_OSX || TARGET_OS_IPHONE