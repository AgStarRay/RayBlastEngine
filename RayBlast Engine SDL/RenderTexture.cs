using System.Runtime.InteropServices;
using SDL3;

namespace RayBlast;

public class RenderTexture : IDisposable {
    public RenderTexture(ushort width, ushort height) {
        UnmanagedManager.AssertMainThread();
        Debug.LogDebug($"Load RenderTexture {width}x{height}");
        Texture = new Texture(willGenerateImage: false, willGenerateTexture: true, width, height);
        Texture.internalTexture = SDL.CreateTexture(RayBlastEngine.currentWindowPane.renderer, SDL.PixelFormat.ARGB8888, SDL.TextureAccess.Target,
                                                    width, width);
        SDL.SetRenderTarget(RayBlastEngine.currentWindowPane.renderer, Texture.internalTexture);
        SDL.GetRenderDrawColor(RayBlastEngine.currentWindowPane.renderer, out byte r, out byte g, out byte b, out byte a);
        SDL.SetRenderDrawColor(RayBlastEngine.currentWindowPane.renderer, 0, 0, 0, 0);
        SDL.RenderClear(RayBlastEngine.currentWindowPane.renderer);
        SDL.SetRenderDrawColor(RayBlastEngine.currentWindowPane.renderer, r, g, b, a);
        SDL.SetTextureBlendMode(Texture.internalTexture, SDL.BlendMode.BlendPremultiplied);
    }

    public Texture Texture { get; }
    public int Width => Texture.Width;
    public int Height => Texture.Height;

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
