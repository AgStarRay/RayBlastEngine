using System.Runtime.InteropServices;
using SDL3;

namespace RayBlast;

public class RenderTexture : IDisposable {
    public RenderTexture(ushort width, ushort height) {
        UnmanagedManager.AssertMainThread();
        Width = width;
        Height = height;
        Debug.LogDebug($"Load RenderTexture {width}x{height}");
        Texture = new Texture(willGenerateImage: false, willGenerateTexture: true, width, height);
        Texture.internalTexture = SDL.CreateTexture(RayBlastEngine.renderer, SDL.PixelFormat.RGBA8888, SDL.TextureAccess.Target,
                                                    width, width);
        SDL.SetRenderTarget(RayBlastEngine.renderer, Texture.internalTexture);
        SDL.GetRenderDrawColor(RayBlastEngine.renderer, out byte r, out byte g, out byte b, out byte a);
        SDL.SetRenderDrawColor(RayBlastEngine.renderer, 0, 0, 0, 0);
        SDL.RenderClear(RayBlastEngine.renderer);
        SDL.SetRenderDrawColor(RayBlastEngine.renderer, r, g, b, a);
        SDL.SetTextureBlendMode(Texture.internalTexture, SDL.BlendMode.BlendPremultiplied);
    }

    public Texture Texture { get; }
    public int Width { get; }
    public int Height { get; }

    private void ReleaseUnmanagedResources() {
        Texture.Dispose();
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~RenderTexture() {
        ReleaseUnmanagedResources();
    }
}
