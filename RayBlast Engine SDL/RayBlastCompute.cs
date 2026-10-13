using System.Runtime.InteropServices;
using SDL3;

namespace RayBlast;

public class RayBlastCompute {
    private static IntPtr gpuDevice;

    private readonly IntPtr computePipeline;
    private readonly SDL.GPUStorageTextureReadWriteBinding[] storageTextureBindings = [];
    private readonly SDL.GPUStorageBufferReadWriteBinding[] storageBufferBindings = [];
    private readonly IntPtr[] storageTextures = [];
    private readonly IntPtr[] storageBuffers = [];

    internal static void Initialize() {
        //TODO: Implement Direct3D 12
        //TODO: Lower Vulkan requirements where not needed
        if(!RayBlastEngine.UsingVulkan)
            throw new RayBlastEngineException("Vulkan support not currently active");
        Debug.Log("Initializing Vulkan compute module");
        #if RAYBLAST_DEBUG
        gpuDevice = SDL.CreateGPUDevice(SDL.GPUShaderFormat.SPIRV, true, "vulkan");
        #else
        gpuDevice = SDL.CreateGPUDevice(SDL.GPUShaderFormat.SPIRV, debugMode: false, "vulkan");
        #endif
        if(gpuDevice == IntPtr.Zero)
            throw new RayBlastEngineException($"Could not initialize compute module: {SDL.GetError()}");
        Debug.Log("Compute module loaded");
    }

    public RayBlastCompute(SDL.GPUComputePipelineCreateInfo createInfo) {
        if(!RayBlastEngine.UsingVulkan)
            throw new RayBlastEngineException("Vulkan support not currently active");
        computePipeline = SDL.CreateGPUComputePipeline(gpuDevice, createInfo);
        if(computePipeline == IntPtr.Zero)
            throw new RayBlastEngineException($"Could not create compute pipeline: {SDL.GetError()}");
    }

    public void Run(uint groupCountX, uint groupCountY,
                    uint groupCountZ) {
        if(!RayBlastEngine.UsingVulkan)
            throw new RayBlastEngineException("Vulkan support not currently active");
        IntPtr commandBuffer = SDL.AcquireGPUCommandBuffer(gpuDevice);
        if(commandBuffer == IntPtr.Zero)
            throw new RayBlastEngineException($"Could not acquire GPU command buffer: {SDL.GetError()}");
        IntPtr computePass = SDL.BeginGPUComputePass(commandBuffer, storageTextureBindings, (uint)storageTextureBindings.Length,
                                                     storageBufferBindings, (uint)storageBufferBindings.Length);
        if(computePass == IntPtr.Zero)
            throw new RayBlastEngineException($"Could not begin compute pass: {SDL.GetError()}");
        SDL.BindGPUComputePipeline(computePass, computePipeline);
        uint firstSlot = 0;
        SDL.BindGPUComputeStorageTextures(computePass, firstSlot, storageTextures, (uint)storageTextures.Length);
        SDL.BindGPUComputeStorageBuffers(computePass, firstSlot, storageBuffers, (uint)storageBuffers.Length);
        SDL.EndGPUComputePass(commandBuffer);
        SDL.DispatchGPUCompute(computePass, groupCountX, groupCountY, groupCountZ);
        IntPtr fencePtr = SDL.SubmitGPUCommandBufferAndAcquireFence(commandBuffer);
        if(fencePtr == IntPtr.Zero)
            throw new RayBlastEngineException($"Submit GPU command buffer failed: {SDL.GetError()}");
        //TODO_AFTER: Get rid of allocation
        bool success = SDL.WaitForGPUFences(gpuDevice, true, [fencePtr], 1);
        if(!success)
            throw new RayBlastEngineException($"GPU fence failure: {SDL.GetError()}");
        SDL.ReleaseGPUFence(gpuDevice, fencePtr);
    }
}
