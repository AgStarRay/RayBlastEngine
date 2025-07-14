using System.Numerics;
using System.Runtime.InteropServices;
using SDL3;

namespace RayBlast;

public static unsafe class BatchMode2D {
    private static Texture? currentTexture;

    private static float[] xy = new float[262144];
    private static SDL.FColor[] colors = new SDL.FColor[131072];
    private static float[] uv = new float[262144];
    private static int[] indices = new int[262144];
    private static int vertexCount;
    private static int indexCount;

    static BatchMode2D() {
    }

    public static void StartBatch(Texture texture) {
        UnmanagedManager.AssertMainThread();
        if(currentTexture != null)
            Debug.LogWarning("Batch aborted");
        currentTexture = texture;
        vertexCount = 0;
        indexCount = 0;
    }

    public static void Flush() {
        if(currentTexture != null && vertexCount > 0)
            Render();
    }

    public static void Render() {
        UnmanagedManager.AssertMainThread();
        if(currentTexture == null)
            throw new RayBlastEngineException("Batch not started");
        if(RayBlastEngine.renderer == IntPtr.Zero)
            throw new RayBlastEngineException("Renderer not initialized");
        if(currentTexture.internalTexture == IntPtr.Zero) {
            IntPtr intPtr = currentTexture.internalDataPtr;
            if(intPtr == IntPtr.Zero)
                throw new RayBlastEngineException("Attempted to render uninitialized texture");
            currentTexture.internalTexture = SDL.CreateTextureFromSurface(RayBlastEngine.renderer, intPtr);
            if(currentTexture.internalTexture == IntPtr.Zero)
                throw new RayBlastEngineException($"Failed to create texture from surface: {SDL.GetError()}");
            SDL.SetTextureBlendMode(currentTexture.internalTexture, Graphics.BLEND_WITH_PREMULTIPLY_ALPHA);
        }
        bool success = SDL.RenderGeometryRaw(RayBlastEngine.renderer, currentTexture.internalTexture,
                                             xy.AsSpan(), 2 * sizeof(float),
                                             colors.AsSpan(), sizeof(SDL.FColor),
                                             uv.AsSpan(), 2 * sizeof(float), vertexCount,
                                             indices.AsSpan(), indexCount, sizeof(int));
        if(!success)
            Debug.LogError($"Failed to render 2D batch: {SDL.GetError()}", false);
        currentTexture = null;
    }

    public static void DrawRectangle(float x, float y,
                                     float width, float height,
                                     ColorF color) {
        UnmanagedManager.AssertMainThread();
        throw new NotImplementedException();
    }

    public static void DrawRectangleOutline(float x, float y,
                                            float width, float height,
                                            uint outlineSize, ColorF color) {
        if(outlineSize >= width / 2f || outlineSize >= height / 2f) {
            DrawRectangle(x, y, width, height, color);
        }
        else {
            UnmanagedManager.AssertMainThread();
            throw new NotImplementedException();
        }
    }

    public static void DrawTriangle(Vector2 a, Vector2 b,
                                    Vector2 c, ColorF color) {
        UnmanagedManager.AssertMainThread();
        throw new NotImplementedException();
    }

    public static void DrawQuad(Vector2 origin, Quaternion rotation,
                                Vector2 size, ColorF color) {
        UnmanagedManager.AssertMainThread();
        Vector2 a = origin + Vector2.Transform(new Vector2(-size.X / 2f, size.Y / 2f), rotation);
        Vector2 b = origin + Vector2.Transform(new Vector2(size.X / 2f, size.Y / 2f), rotation);
        Vector2 c = origin + Vector2.Transform(new Vector2(-size.X / 2f, -size.Y / 2f), rotation);
        Vector2 d = origin + Vector2.Transform(new Vector2(size.X / 2f, -size.Y / 2f), rotation);
        throw new NotImplementedException();
    }

    public static void DrawImage(int x, int y,
                                 ColorF tint) {
        UnmanagedManager.AssertMainThread();
        throw new NotImplementedException();
    }

    public static void DrawSubimage(TextureSubimage subimage, Vector2 destination,
                                    ColorF tint) {
        DrawSubimage(subimage, subimage.rectangle with {
                         X = destination.X, Y = destination.Y
                     }, new Vector2(0.5f, 0.5f), Quaternion.Identity,
                     tint);
    }

    public static void DrawSubimage(TextureSubimage subimage, Vector4 destination,
                                    ColorF tint) {
        DrawSubimage(subimage, destination, new Vector2(0.5f, 0.5f), Quaternion.Identity, tint);
    }

    public static void DrawSubimage(TextureSubimage subimage, Vector4 destination,
                                    Vector2 origin, ColorF tint) {
        DrawSubimage(subimage, destination, origin, Quaternion.Identity, tint);
    }

    public static void DrawSubimage(TextureSubimage subimage, Vector4 destination,
                                    Vector2 origin, Quaternion rotation,
                                    ColorF tint) {
        origin *= new Vector2(destination.Z, destination.W);
        if(subimage.texture != currentTexture)
            throw new RayBlastEngineException("Mismatched texture for render batch");
        EnsureVertices(4);
        EnsureIndices(6);
        int xyCount = vertexCount * 2;
        int uvIndex = vertexCount * 2;
        indices[indexCount++] = vertexCount;
        indices[indexCount++] = vertexCount + 1;
        indices[indexCount++] = vertexCount + 2;
        indices[indexCount++] = vertexCount + 1;
        indices[indexCount++] = vertexCount + 3;
        indices[indexCount++] = vertexCount + 2;
        Vector4 subimageRectangle = subimage.rectangle;
        uv[uvIndex++] = subimageRectangle.X / currentTexture.Width;
        uv[uvIndex++] = subimageRectangle.Y / currentTexture.Height;
        uv[uvIndex++] = (subimageRectangle.X + subimageRectangle.Z) / currentTexture.Width;
        uv[uvIndex++] = subimageRectangle.Y / currentTexture.Height;
        uv[uvIndex++] = subimageRectangle.X / currentTexture.Width;
        uv[uvIndex++] = (subimageRectangle.Y + subimageRectangle.W) / currentTexture.Height;
        uv[uvIndex++] = (subimageRectangle.X + subimageRectangle.Z) / currentTexture.Width;
        uv[uvIndex] = (subimageRectangle.Y + subimageRectangle.W) / currentTexture.Height;
        SDL.FColor fColor = tint.ToSDL();
        colors[vertexCount++] = fColor;
        colors[vertexCount++] = fColor;
        colors[vertexCount++] = fColor;
        colors[vertexCount++] = fColor;
        if(rotation == Quaternion.Identity) {
            destination.X -= origin.X;
            destination.Y -= origin.Y;
            xy[xyCount++] = destination.X;
            xy[xyCount++] = destination.Y;
            xy[xyCount++] = destination.X + destination.Z;
            xy[xyCount++] = destination.Y;
            xy[xyCount++] = destination.X;
            xy[xyCount++] = destination.Y + destination.W;
            xy[xyCount++] = destination.X + destination.Z;
            xy[xyCount] = destination.Y + destination.W;
        }
        else {
            Vector2 a = Vector2.Transform(-origin, rotation);
            Vector2 b = Vector2.Transform(new Vector2(destination.Z - origin.X, -origin.Y), rotation);
            Vector2 c = Vector2.Transform(new Vector2(-origin.X, destination.W - origin.Y), rotation);
            Vector2 d = Vector2.Transform(new Vector2(destination.Z - origin.X, destination.W - origin.Y), rotation);
            xy[xyCount++] = destination.X + a.X;
            xy[xyCount++] = destination.Y + a.Y;
            xy[xyCount++] = destination.X + b.X;
            xy[xyCount++] = destination.Y + b.Y;
            xy[xyCount++] = destination.X + c.X;
            xy[xyCount++] = destination.Y + c.Y;
            xy[xyCount++] = destination.X + d.X;
            xy[xyCount] = destination.Y + d.Y;
        }
    }

    public static void DrawSubimage(TextureSubimage subimage, Vector4 destination,
                                    Vector2 origin, Quaternion rotation,
                                    ColorF colA, ColorF colB,
                                    ColorF colC, ColorF colD) {
        origin *= new Vector2(destination.Z, destination.W);
        if(subimage.texture != currentTexture)
            throw new RayBlastEngineException("Mismatched texture for render batch");
        EnsureVertices(4);
        EnsureIndices(6);
        int xyCount = vertexCount * 2;
        int uvIndex = vertexCount * 2;
        indices[indexCount++] = vertexCount;
        indices[indexCount++] = vertexCount + 1;
        indices[indexCount++] = vertexCount + 2;
        indices[indexCount++] = vertexCount + 1;
        indices[indexCount++] = vertexCount + 3;
        indices[indexCount++] = vertexCount + 2;
        Vector4 subimageRectangle = subimage.rectangle;
        uv[uvIndex++] = subimageRectangle.X / currentTexture.Width;
        uv[uvIndex++] = subimageRectangle.Y / currentTexture.Height;
        uv[uvIndex++] = (subimageRectangle.X + subimageRectangle.Z) / currentTexture.Width;
        uv[uvIndex++] = subimageRectangle.Y / currentTexture.Height;
        uv[uvIndex++] = subimageRectangle.X / currentTexture.Width;
        uv[uvIndex++] = (subimageRectangle.Y + subimageRectangle.W) / currentTexture.Height;
        uv[uvIndex++] = (subimageRectangle.X + subimageRectangle.Z) / currentTexture.Width;
        uv[uvIndex] = (subimageRectangle.Y + subimageRectangle.W) / currentTexture.Height;
        colors[vertexCount++] = colA.ToSDL();
        colors[vertexCount++] = colB.ToSDL();
        colors[vertexCount++] = colC.ToSDL();
        colors[vertexCount++] = colD.ToSDL();
        if(rotation == Quaternion.Identity) {
            destination.X -= origin.X;
            destination.Y -= origin.Y;
            xy[xyCount++] = destination.X;
            xy[xyCount++] = destination.Y;
            xy[xyCount++] = destination.X + destination.Z;
            xy[xyCount++] = destination.Y;
            xy[xyCount++] = destination.X;
            xy[xyCount++] = destination.Y + destination.W;
            xy[xyCount++] = destination.X + destination.Z;
            xy[xyCount] = destination.Y + destination.W;
        }
        else {
            Vector2 a = Vector2.Transform(-origin, rotation);
            Vector2 b = Vector2.Transform(new Vector2(destination.Z - origin.X, -origin.Y), rotation);
            Vector2 c = Vector2.Transform(new Vector2(-origin.X, destination.W - origin.Y), rotation);
            Vector2 d = Vector2.Transform(new Vector2(destination.Z - origin.X, destination.W - origin.Y), rotation);
            xy[xyCount++] = destination.X + a.X;
            xy[xyCount++] = destination.Y + a.Y;
            xy[xyCount++] = destination.X + b.X;
            xy[xyCount++] = destination.Y + b.Y;
            xy[xyCount++] = destination.X + c.X;
            xy[xyCount++] = destination.Y + c.Y;
            xy[xyCount++] = destination.X + d.X;
            xy[xyCount] = destination.Y + d.Y;
        }
    }

    public static void DrawLine(Vector2 start, Vector2 end,
                                ColorF color) {
        UnmanagedManager.AssertMainThread();
        throw new NotImplementedException();
    }

    private static void EnsureVertices(int newVertices) {
        if(vertexCount + newVertices > colors.Length) {
            Array.Resize(ref xy, xy.Length * 2);
            Array.Resize(ref colors, colors.Length * 2);
            Array.Resize(ref uv, uv.Length * 2);
        }
    }

    private static void EnsureIndices(int newIndices) {
        if(indexCount + newIndices > indices.Length) {
            Array.Resize(ref indices, indices.Length * 2);
        }
    }
}
