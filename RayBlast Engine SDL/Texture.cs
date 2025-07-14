using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL3;

namespace RayBlast;

public class Texture : IDisposable {
    // internal RenderTexture2D internalRenderTexture;
    // internal Image internalData;
    // internal Texture2D internalTexture;
    //TODO: Use
    internal bool imageUpdated = true;
    private readonly bool generatedImage;
    private readonly bool generatedTexture;
    internal readonly SDL.Surface internalData;
    internal readonly IntPtr internalDataPtr;
    internal IntPtr internalTexture;

    public Texture(int width, int height) {
        internalDataPtr = SDL.CreateSurface(width, height, SDL.PixelFormat.RGBA8888);
        internalData = Marshal.PtrToStructure<SDL.Surface>(internalDataPtr);
        generatedImage = true;
        generatedTexture = true;
        Width = width;
        Height = height;
    }

    internal Texture(bool willGenerateImage, bool willGenerateTexture,
                     int width, int height) {
        generatedImage = willGenerateImage;
        generatedTexture = willGenerateTexture;
        Width = width;
        Height = height;
    }

    internal Texture(IntPtr internalDataPtr) {
        this.internalDataPtr = internalDataPtr;
        internalData = Marshal.PtrToStructure<SDL.Surface>(internalDataPtr);
        Width = internalData.Width;
        Height = internalData.Height;
        generatedTexture = true;
    }

    // internal Texture(Texture2D internalTexture) {
    //     UnmanagedManager.AssertMainThread();
    //     this.internalTexture = internalTexture;
    //     Debug.LogDebug($"Load ImageFromTexture {internalTexture.Id}");
    //     internalData = Raylib.LoadImageFromTexture(internalTexture);
    //     generatedImage = true;
    // }

    // internal Texture(RenderTexture2D internalRenderTexture) {
    //     this.internalRenderTexture = internalRenderTexture;
    // }

    public int Width { get; }
    public int Height { get; }
    //TODO: Implement
    public bool UsePixelFilter { get; set; }
    //TODO: Implement
    public bool UseClamping { get; set; }

    public void SetPixel(int x, int y,
                         ColorF color) {
        SetPixel(x, y, (Color32)color);
    }

    public void SetPixel(int x, int y,
                         Color32 color) {
        if(internalData.Pixels == IntPtr.Zero)
            throw new RayBlastEngineException("Texture is read-only or not initialized with a surface");
        if(x < 0)
            throw new RayBlastEngineException("X coordinate less than 0");
        if(y < 0)
            throw new RayBlastEngineException("Y coordinate less than 0");
        if(x >= internalData.Width)
            throw new RayBlastEngineException($"X coordinate {x} outside of image bounds {internalData.Width}");
        if(y >= internalData.Height)
            throw new RayBlastEngineException($"Y coordinate {y} outside of image bounds {internalData.Height}");
        SDL.WriteSurfacePixel(internalDataPtr, x, y, color.r, color.g, color.b, color.a);
    }

    public unsafe void SetPixels(int x, int y,
                                 int width, int height,
                                 ColorF[] colors) {
        if(internalData.Pixels == IntPtr.Zero)
            throw new RayBlastEngineException("Texture is read-only or not initialized with a surface");
        if(x < 0)
            throw new RayBlastEngineException("X coordinate less than 0");
        if(y < 0)
            throw new RayBlastEngineException("Y coordinate less than 0");
        if(x + width > internalData.Width)
            throw new RayBlastEngineException($"X {x} plus width {width} outside of image bounds {internalData.Width}");
        if(y + height > internalData.Height)
            throw new RayBlastEngineException($"Y {y} plus height {height} outside of image bounds {internalData.Height}");
        var colorData = (Color32*)((byte*)internalData.Pixels - 4);
        for(var i = 0; i < height; i++) {
            int destOffset = (i + y) * width + x;
            int srcOffset = i * width;
            for(var j = 0; j < width; j++) {
                colorData[destOffset++] = (Color32)colors[srcOffset++ % colors.Length];
            }
        }
    }

    //TODO_AFTER: Find a way to copy a block of data
    public unsafe void SetPixels(int x, int y,
                                 int width, int height,
                                 Color32[] colors) {
        if(internalData.Pixels == IntPtr.Zero)
            throw new RayBlastEngineException("Texture is read-only or not initialized with a surface");
        if(x < 0)
            throw new RayBlastEngineException("X coordinate less than 0");
        if(y < 0)
            throw new RayBlastEngineException("Y coordinate less than 0");
        if(x + width > internalData.Width)
            throw new RayBlastEngineException($"X {x} plus width {width} outside of image bounds {internalData.Width}");
        if(y + height > internalData.Height)
            throw new RayBlastEngineException($"Y {y} plus height {height} outside of image bounds {internalData.Height}");
        var colorData = (Color32*)((byte*)internalData.Pixels - 4);
        for(var i = 0; i < height; i++) {
            int destOffset = (i + y) * width + x;
            int srcOffset = i * width;
            for(var j = 0; j < width; j++) {
                colorData[destOffset++] = colors[srcOffset++ % colors.Length];
            }
        }
    }

    public void Clear() {
        if(!SDL.ClearSurface(internalDataPtr, 1f, 1f, 1f, 0f))
            throw new RayBlastEngineException($"Failed to clear surface: {SDL.GetError()}");
        imageUpdated = true;
    }

    public void Apply() {
        imageUpdated = true;
    }

    private void ReleaseUnmanagedResources() {
        //TODO_URGENT: Dispose the surface
        // if(generatedTexture && Raylib.IsTextureValid(internalTexture))
        //     UnmanagedManager.EnqueueUnloadStream(internalTexture);
        // if(generatedImage && Raylib.IsImageValid(internalData))
        //     Raylib.UnloadImage(internalData);
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Texture() {
        ReleaseUnmanagedResources();
    }
}
