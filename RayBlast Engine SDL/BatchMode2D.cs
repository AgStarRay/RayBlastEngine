using System.Numerics;
using System.Runtime.InteropServices;
using SDL3;

namespace RayBlast;

public unsafe class BatchMode2D {
    public bool isStatic = false;
    private Texture? currentTexture;

    private float[] xy = new float[8192];
    private ColorF[] colors = new ColorF[4096];
    private float[] uv = new float[8192];
    private int[] indices = new int[8192];
    private int vertexCount;
    private int indexCount;

    public void StartBatch(Texture texture) {
        if(currentTexture != null)
            Debug.LogWarning("Batch aborted");
        currentTexture = texture;
        vertexCount = 0;
        indexCount = 0;
    }

    public void Flush() {
        if(currentTexture != null && vertexCount > 0)
            Render();
    }

    public void Render() {
        UnmanagedManager.AssertMainThread();
        if(vertexCount > 0) {
            if(currentTexture == null)
                throw new RayBlastEngineException("Batch not started");
            if(RayBlastEngine.currentWindowPane.renderer == IntPtr.Zero)
                throw new RayBlastEngineException("Renderer not initialized");
            currentTexture.PerformUpdate();
            bool success = SDL.RenderGeometryRaw(RayBlastEngine.currentWindowPane.renderer, currentTexture.internalTexture,
                                                 xy, 2 * sizeof(float),
                                                 MemoryMarshal.Cast<ColorF, SDL.FColor>(colors.AsSpan()), sizeof(SDL.FColor),
                                                 uv, 2 * sizeof(float), vertexCount,
                                                 indices, indexCount, sizeof(int));
            if(!success)
                Debug.LogError($"Failed to render 2D batch: {SDL.GetError()}", false);
        }
        if(!isStatic)
            Toss();
    }

    public void Toss() {
        currentTexture = null;
        vertexCount = 0;
        indexCount = 0;
    }

    public void DrawRectangle(float x, float y,
                              float width, float height,
                              ColorF color) {
        throw new NotImplementedException();
    }

    public void DrawRectangleOutline(float x, float y,
                                     float width, float height,
                                     uint outlineSize, ColorF color) {
        if(outlineSize >= width / 2f || outlineSize >= height / 2f) {
            DrawRectangle(x, y, width, height, color);
        }
        else {
            throw new NotImplementedException();
        }
    }

    public void DrawTriangle(Vector2 a, Vector2 b,
                             Vector2 c, ColorF color) {
        throw new NotImplementedException();
    }

    public void DrawQuad(Vector2 origin, Quaternion rotation,
                         Vector2 size, ColorF color) {
        Vector2 a = origin + Vector2.Transform(new Vector2(-size.X / 2f, size.Y / 2f), rotation);
        Vector2 b = origin + Vector2.Transform(new Vector2(size.X / 2f, size.Y / 2f), rotation);
        Vector2 c = origin + Vector2.Transform(new Vector2(-size.X / 2f, -size.Y / 2f), rotation);
        Vector2 d = origin + Vector2.Transform(new Vector2(size.X / 2f, -size.Y / 2f), rotation);
        throw new NotImplementedException();
    }

    public void DrawImage(int x, int y,
                          ColorF tint) {
        throw new NotImplementedException();
    }

    public void DrawSubimage(TextureSubimage subimage, Vector2 destination,
                             ColorF tint) {
        if(subimage.texture != currentTexture)
            throw new RayBlastEngineException("Mismatched texture for render batch");
        DrawSubimage(subimage.rectangle, subimage.rectangle with {
                         X = destination.X, Y = destination.Y
                     }, new Vector2(0.5f, 0.5f), Quaternion.Identity,
                     tint);
    }

    public void DrawSubimage(TextureSubimage subimage, Vector4 destination,
                             ColorF tint) {
        DrawSubimage(subimage, destination, new Vector2(0.5f, 0.5f), Quaternion.Identity, tint);
    }

    public void DrawSubimage(TextureSubimage subimage, Vector4 destination,
                             Vector2 origin, ColorF tint) {
        DrawSubimage(subimage, destination, origin, Quaternion.Identity, tint);
    }

    public void DrawSubimage(TextureSubimage subimage, Vector4 destination,
                             Vector2 origin, Quaternion rotation,
                             ColorF tint) {
        if(subimage.texture != currentTexture)
            throw new RayBlastEngineException("Mismatched texture for render batch");
        DrawSubimage(subimage.rectangle, destination, origin, rotation, tint);
    }

    public void DrawSubimage(TextureSubimage subimage, Vector4 destination,
                             Vector2 origin, Quaternion rotation,
                             ColorF colA, ColorF colB,
                             ColorF colC, ColorF colD) {
        if(subimage.texture != currentTexture)
            throw new RayBlastEngineException("Mismatched texture for render batch");
        DrawSubimage(subimage.rectangle, destination, origin, rotation, colA, colB, colC, colD);
    }

    public void DrawSubimage(Vector4 subimageRectangle, Vector4 destination,
                             Vector2 origin, Quaternion rotation,
                             ColorF tint) {
        if(currentTexture == null)
            throw new RayBlastEngineException("Batch not started");
        EnsureVertices(4);
        EnsureIndices(6);
        origin *= new Vector2(destination.Z, destination.W);
        int xyCount = vertexCount * 2;
        int uvIndex = vertexCount * 2;
        indices[indexCount++] = vertexCount;
        indices[indexCount++] = vertexCount + 1;
        indices[indexCount++] = vertexCount + 2;
        indices[indexCount++] = vertexCount + 1;
        indices[indexCount++] = vertexCount + 3;
        indices[indexCount++] = vertexCount + 2;
        uv[uvIndex++] = subimageRectangle.X / currentTexture.Width;
        uv[uvIndex++] = subimageRectangle.Y / currentTexture.Height;
        uv[uvIndex++] = (subimageRectangle.X + subimageRectangle.Z) / currentTexture.Width;
        uv[uvIndex++] = subimageRectangle.Y / currentTexture.Height;
        uv[uvIndex++] = subimageRectangle.X / currentTexture.Width;
        uv[uvIndex++] = (subimageRectangle.Y + subimageRectangle.W) / currentTexture.Height;
        uv[uvIndex++] = (subimageRectangle.X + subimageRectangle.Z) / currentTexture.Width;
        uv[uvIndex] = (subimageRectangle.Y + subimageRectangle.W) / currentTexture.Height;
        colors[vertexCount++] = tint;
        colors[vertexCount++] = tint;
        colors[vertexCount++] = tint;
        colors[vertexCount++] = tint;
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

    public void DrawSubimage(Vector4 subimageRectangle, Vector4 destination,
                             Vector2 origin, Quaternion rotation,
                             ColorF colA, ColorF colB,
                             ColorF colC, ColorF colD) {
        if(currentTexture == null)
            throw new RayBlastEngineException("Batch not started");
        EnsureVertices(4);
        EnsureIndices(6);
        origin *= new Vector2(destination.Z, destination.W);
        int xyCount = vertexCount * 2;
        int uvIndex = vertexCount * 2;
        indices[indexCount++] = vertexCount;
        indices[indexCount++] = vertexCount + 1;
        indices[indexCount++] = vertexCount + 2;
        indices[indexCount++] = vertexCount + 1;
        indices[indexCount++] = vertexCount + 3;
        indices[indexCount++] = vertexCount + 2;
        uv[uvIndex++] = subimageRectangle.X / currentTexture.Width;
        uv[uvIndex++] = subimageRectangle.Y / currentTexture.Height;
        uv[uvIndex++] = (subimageRectangle.X + subimageRectangle.Z) / currentTexture.Width;
        uv[uvIndex++] = subimageRectangle.Y / currentTexture.Height;
        uv[uvIndex++] = subimageRectangle.X / currentTexture.Width;
        uv[uvIndex++] = (subimageRectangle.Y + subimageRectangle.W) / currentTexture.Height;
        uv[uvIndex++] = (subimageRectangle.X + subimageRectangle.Z) / currentTexture.Width;
        uv[uvIndex] = (subimageRectangle.Y + subimageRectangle.W) / currentTexture.Height;
        colors[vertexCount++] = colA;
        colors[vertexCount++] = colB;
        colors[vertexCount++] = colC;
        colors[vertexCount++] = colD;
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

    public void DrawLine(Vector2 start, Vector2 end,
                         ColorF color) {
        throw new NotImplementedException();
    }

    private void EnsureVertices(int newVertices) {
        if(vertexCount + newVertices > colors.Length) {
            Array.Resize(ref xy, xy.Length * 2);
            Array.Resize(ref colors, colors.Length * 2);
            Array.Resize(ref uv, uv.Length * 2);
        }
    }

    private void EnsureIndices(int newIndices) {
        if(indexCount + newIndices > indices.Length) {
            Array.Resize(ref indices, indices.Length * 2);
        }
    }
}
