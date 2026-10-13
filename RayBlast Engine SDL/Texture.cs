using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL3;

namespace RayBlast;

public class Texture : IDisposable {
    // internal RenderTexture2D internalRenderTexture;
    // internal Image internalData;
    // internal Texture2D internalTexture;
    internal bool imageUpdated = true;
    private readonly bool generatedImage;
    private readonly bool generatedTexture;
    private readonly bool usesPalette;
    internal readonly SDL.Surface internalData;
    internal readonly IntPtr internalDataPtr;
    internal IntPtr internalTexture;
    internal readonly IntPtr palettePtr;

    public Texture(int width, int height) {
        internalDataPtr = SDL.CreateSurface(width, height, SDL.PixelFormat.ABGR8888);
        if(internalDataPtr == IntPtr.Zero)
            throw new RayBlastEngineException($"Failed to create texture surface: {SDL.GetError()}");
        internalData = Marshal.PtrToStructure<SDL.Surface>(internalDataPtr);
        generatedImage = true;
        generatedTexture = true;
        usesPalette = false;
        Width = width;
        Height = height;
    }

    public Texture(int width, int height,
                   ReadOnlySpan<Color32> palette) {
        internalDataPtr = SDL.CreateSurface(width, height, SDL.PixelFormat.Index8);
        if(internalDataPtr == IntPtr.Zero)
            throw new RayBlastEngineException($"Failed to create texture surface: {SDL.GetError()}");
        internalData = Marshal.PtrToStructure<SDL.Surface>(internalDataPtr);
        generatedImage = true;
        generatedTexture = true;
        usesPalette = true;
        palettePtr = SDL.CreateSurfacePalette(internalDataPtr);
        if(palettePtr == IntPtr.Zero)
            throw new RayBlastEngineException($"Failed to create texture palette: {SDL.GetError()}");
        SetColors(palette);
        Width = width;
        Height = height;
    }

    internal Texture(bool willGenerateImage, bool willGenerateTexture,
                     int width, int height) {
        generatedImage = willGenerateImage;
        generatedTexture = willGenerateTexture;
        usesPalette = false;
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
    public bool UsePixelFilter {
        get;
        set {
            field = value;
            if(internalTexture != IntPtr.Zero) {
                UnmanagedManager.AssertMainThread();
                SDL.SetTextureScaleMode(internalTexture, UsePixelFilter ? SDL.ScaleMode.Nearest : SDL.ScaleMode.Linear);
            }
        }
    }
    //TODO: Implement
    public bool UseClamping { get; set; }

    public void SetColors(ReadOnlySpan<Color32> palette) {
        UnmanagedManager.AssertMainThread();
        if(!usesPalette)
            throw new RayBlastEngineException("Texture does not support a palette");
        SDL.Color[] sdlColors = ArrayPool<SDL.Color>.Shared.Rent(palette.Length);
        try {
            for(var i = 0; i < palette.Length; i++) {
                sdlColors[i] = palette[i].ToSDL();
            }
            bool success = SDL.SetPaletteColors(palettePtr, sdlColors, 0, palette.Length);
            if(!success)
                throw new RayBlastEngineException($"Failed to set palette colors: {SDL.GetError()}");
        }
        finally {
            ArrayPool<SDL.Color>.Shared.Return(sdlColors);
        }
    }

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
        bool success = SDL.WriteSurfacePixel(internalDataPtr, x, y, color.r, color.g, color.b, color.a);
        if(!success)
            throw new RayBlastEngineException($"Failed to set pixel: {SDL.GetError()}");
    }

    public unsafe void SetPixels(int x, int y,
                                 int width, int height,
                                 ColorF[] colors) {
        if(internalData.Pixels == IntPtr.Zero)
            throw new RayBlastEngineException("Texture is read-only or not initialized with a surface");
        if(colors.Length == 0)
            throw new RayBlastEngineException("Colors array is empty");
        if(x < 0)
            throw new RayBlastEngineException("X coordinate less than 0");
        if(y < 0)
            throw new RayBlastEngineException("Y coordinate less than 0");
        if(x + width > internalData.Width)
            throw new RayBlastEngineException($"X {x} plus width {width} outside of image bounds {internalData.Width}");
        if(y + height > internalData.Height)
            throw new RayBlastEngineException($"Y {y} plus height {height} outside of image bounds {internalData.Height}");
        var colorData = (Color32*)(byte*)internalData.Pixels;
        int srcOffset = 0;
        for(var i = 0; i < height; i++) {
            int destOffset = (i + y) * width + x;
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

    public void CopyTo(Texture destinationTexture) {
        if(internalData.Pixels == IntPtr.Zero)
            throw new RayBlastEngineException("Source texture is read-only or not initialized with a surface");
        if(destinationTexture.internalData.Pixels == IntPtr.Zero)
            throw new RayBlastEngineException("Destination texture is read-only or not initialized with a surface");
        // if(internalData.Width != destinationTexture.internalData.Width || internalData.Height != destinationTexture.internalData.Height)
        //     throw new RayBlastEngineException("Source texture dimensions do not match destination texture dimensions");
        if(internalData.Pitch != destinationTexture.internalData.Pitch)
            throw new RayBlastEngineException("Source texture formatting is incompatible with destination texture formatting");
        var srcRect = new SDL.Rect {
            W = internalData.Width, H = internalData.Height
        };
        var destRect = new SDL.Rect {
            W = destinationTexture.internalData.Width, H = destinationTexture.internalData.Height
        };
        bool success = SDL.BlitSurface(internalDataPtr, srcRect, destinationTexture.internalDataPtr, destRect);
        if(!success)
            throw new RayBlastEngineException($"Failed to blit texture data: {SDL.GetError()}");
    }

    public void Clear() {
        if(!SDL.ClearSurface(internalDataPtr, 1f, 1f, 1f, 0f))
            throw new RayBlastEngineException($"Failed to clear surface: {SDL.GetError()}");
        imageUpdated = true;
    }

    public void Apply() {
        imageUpdated = true;
    }

    internal void PerformUpdate() {
        if(internalTexture == IntPtr.Zero) {
            if(internalDataPtr == IntPtr.Zero)
                throw new RayBlastEngineException("Attempted to render uninitialized texture");
            //TODO: Check if the texture was loaded in the currentWindowPane
            IntPtr texture = SDL.CreateTextureFromSurface(RayBlastEngine.currentWindowPane.renderer, internalDataPtr);
            if(texture == IntPtr.Zero)
                throw new RayBlastEngineException($"Failed to create texture from surface: {SDL.GetError()}");
            internalTexture = texture;
            if(usesPalette)
                SDL.SetTexturePalette(internalTexture, palettePtr);
            SDL.SetTextureBlendMode(internalTexture, Graphics.BLEND_WITH_PREMULTIPLY_ALPHA);
            SDL.SetTextureScaleMode(internalTexture, UsePixelFilter ? SDL.ScaleMode.PixelArt : SDL.ScaleMode.Linear);
        }
        if(imageUpdated && internalDataPtr != IntPtr.Zero && internalData.Pitch > 0) {
            bool success = SDL.UpdateTexture(internalTexture, new SDL.Rect {
                W = Width, H = Height
            }, internalData.Pixels, internalData.Pitch);
            if(!success)
                throw new RayBlastEngineException($"Failed to update texture: {SDL.GetError()}");
            imageUpdated = false;
        }
    }

    public void ReadFrom(RenderTexture source, Vector4 sourceRect,
                         S32X2 destination) {
        throw new NotImplementedException();
        // UnityEngine.RenderTexture.active = source.internalTexture;
        // internalTexture?.ReadPixels(new Rect(sourceRect.X, sourceRect.Y, sourceRect.Z, sourceRect.W), destination.X, destination.Y);
        // internalTexture?.Apply();
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
