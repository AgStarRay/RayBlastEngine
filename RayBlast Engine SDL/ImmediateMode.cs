using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using Cysharp.Text;
using RayBlast.Text;
using SDL3;

namespace RayBlast;

public static unsafe class ImmediateMode {
    // private static readonly Shader SDF_SHADER;
    private static readonly int DILATION_LOC;
    private static readonly int OUTLINE_COLOR_LOC;
    private static readonly int OUTLINE_DILATION_LOC;

    private static RenderTexture? currentRenderTexture;
    private static IntPtr rendererTextEngine;

    static ImmediateMode() {
        Uri resourceUri = IO.CreateResourceUri("Shaders/sdf.frag");
        string fragmentCode = File.ReadAllText(resourceUri.LocalPath);
        // SDF_SHADER = Shader.Load(null, fragmentCode);
        // DILATION_LOC = SDF_SHADER.GetLocation("dilation");
        // OUTLINE_COLOR_LOC = SDF_SHADER.GetLocation("colOutline");
        // OUTLINE_DILATION_LOC = SDF_SHADER.GetLocation("outlineDilation");
    }

    public static void ClearBackground(Color32 color32) {
        UnmanagedManager.AssertMainThread();
        SDL.SetRenderDrawColor(RayBlastEngine.renderer, color32.r, color32.g, color32.b, color32.a);
        SDL.RenderClear(RayBlastEngine.renderer);
    }

    public static void Set3DMode(ImmediateCamera camera) {
        UnmanagedManager.AssertMainThread();
        // Near plane is at 0.01
        // Far plane is at 1000.0
        // Raylib.BeginMode3D(new Raylib_cs.Camera3D {
        //     Position = camera.position,
        //     Target = camera.target,
        //     Up = camera.up,
        //     FovY = camera.orthographic ? camera.nearPlane : camera.fieldOfView,
        //     Projection = camera.orthographic ? CameraProjection.Orthographic : CameraProjection.Perspective
        // });
        //TODO: Convert to Rlgl.SetClipPlanes when it's available
        S32X2 currentResolution = Graphics.CurrentResolution;
        //TODO_URGENT: Add 3D mode
    }

    public static void End3DMode() {
        UnmanagedManager.AssertMainThread();
        SDL.SetRenderTarget(RayBlastEngine.renderer, IntPtr.Zero);
        SDL.SetRenderDrawBlendMode(RayBlastEngine.renderer, Graphics.BLEND_WITH_PREMULTIPLY_ALPHA);
    }

    public static void BeginRenderTexture(RenderTexture renderTexture) {
        UnmanagedManager.AssertMainThread();
        if(currentRenderTexture != null)
            SDL.SetRenderTarget(RayBlastEngine.renderer, IntPtr.Zero);
        currentRenderTexture = renderTexture;
        SDL.SetRenderTarget(RayBlastEngine.renderer, renderTexture.Texture.internalTexture);
    }

    public static void EndRenderTexture() {
        UnmanagedManager.AssertMainThread();
        currentRenderTexture = null;
        SDL.SetRenderTarget(RayBlastEngine.renderer, IntPtr.Zero);
    }

    public static void DrawText(StringBuilder builderToCollapseIntoUTF8, S32X2 position,
                                int size, Color32 color32) {
        using Utf8ValueStringBuilder utf8 = ZString.CreateUtf8StringBuilder();
        foreach(ReadOnlyMemory<char> memory in builderToCollapseIntoUTF8.GetChunks()) {
            utf8.Append(memory.Span);
        }
        ReadOnlySpan<byte> readOnlySpan = utf8.AsSpan();
        fixed(byte* utf8Ptr = readOnlySpan) {
            DrawText((sbyte*)utf8Ptr, position, size, color32, TextAlignmentFlags.Center);
        }
    }

    public static void DrawText(StringBuilder builderToCollapseIntoUTF8, Vector2 position,
                                int size, Color32 color32,
                                FontInstance fontInstance) {
        using Utf8ValueStringBuilder utf8 = ZString.CreateUtf8StringBuilder();
        foreach(ReadOnlyMemory<char> memory in builderToCollapseIntoUTF8.GetChunks()) {
            utf8.Append(memory.Span);
        }
        utf8.Append('\0');
        ReadOnlySpan<byte> readOnlySpan = utf8.AsSpan();
        fixed(byte* utf8Ptr = readOnlySpan) {
            DrawText(utf8Ptr, position, size, color32, TextAlignmentFlags.Center, fontInstance);
        }
    }

    public static void DrawText(StringBuilder builderToCollapseIntoUTF8, Vector2 position,
                                int size, Color32 color32,
                                TextAlignmentFlags alignmentFlags, FontInstance fontInstance) {
        using Utf8ValueStringBuilder utf8 = ZString.CreateUtf8StringBuilder();
        foreach(ReadOnlyMemory<char> memory in builderToCollapseIntoUTF8.GetChunks()) {
            utf8.Append(memory.Span);
        }
        utf8.Append('\0');
        ReadOnlySpan<byte> readOnlySpan = utf8.AsSpan();
        fixed(byte* utf8Ptr = readOnlySpan) {
            DrawText(utf8Ptr, position, size, color32, alignmentFlags, fontInstance);
        }
    }

    public static void DrawText(Utf16ValueStringBuilder builderBeforeUTF8, S32X2 position,
                                int size, Color32 color32) {
        DrawText(builderBeforeUTF8.AsSpan(), position, size, color32);
    }

    public static void DrawText(ReadOnlySpan<char> spanBeforeUTF8, S32X2 position,
                                int size, Color32 color32) {
        using Utf8ValueStringBuilder utf8 = ZString.CreateUtf8StringBuilder();
        utf8.Append(spanBeforeUTF8);
        ReadOnlySpan<byte> readOnlySpan = utf8.AsSpan();
        fixed(byte* utf8Ptr = readOnlySpan) {
            DrawText((sbyte*)utf8Ptr, position, size, color32, TextAlignmentFlags.Center);
        }
    }

    public static void DrawText(Utf8ValueStringBuilder builder, S32X2 position,
                                int size, Color32 color32) {
        ReadOnlySpan<byte> readOnlySpan = builder.AsSpan();
        fixed(byte* utf8Ptr = readOnlySpan) {
            DrawText((sbyte*)utf8Ptr, position, size, color32, TextAlignmentFlags.Center);
        }
    }

    public static void DrawText(Utf8ValueStringBuilder builder, Vector2 position,
                                int size, Color32 color32,
                                FontInstance fontInstance) {
        builder.Append('\0');
        ReadOnlySpan<byte> readOnlySpan = builder.AsSpan();
        fixed(byte* utf8Ptr = readOnlySpan) {
            DrawText(utf8Ptr, position, size, color32, TextAlignmentFlags.Center, fontInstance);
        }
    }

    public static void DrawText(ReadOnlySpan<byte> span, S32X2 position,
                                int size, Color32 color32) {
        Span<byte> temp = stackalloc byte[span.Length + 1];
        span.CopyTo(temp);
        temp[^1] = 0;
        fixed(byte* utf8Ptr = temp) {
            DrawText((sbyte*)utf8Ptr, position, size, color32, TextAlignmentFlags.Center);
        }
    }

    public static void DrawText(ReadOnlySpan<byte> span, Vector2 position,
                                int size, Color32 color32,
                                FontInstance fontInstance) {
        Span<byte> temp = stackalloc byte[span.Length + 1];
        span.CopyTo(temp);
        temp[^1] = 0;
        fixed(byte* utf8Ptr = temp) {
            DrawText(utf8Ptr, position, size, color32, TextAlignmentFlags.Center, fontInstance);
        }
    }

    public static void DrawText(string text, S32X2 position,
                                int size, Color32 color32) {
        throw new NotImplementedException();
    }

    public static void DrawText(string text, Vector2 position,
                                int size, Color32 color32,
                                TextAlignmentFlags alignment, FontInstance fontInstance) {
        throw new NotImplementedException();
    }

    public static void DrawText(string text, Vector2 position,
                                int size, Color32 color32,
                                FontInstance fontInstance) {
        throw new NotImplementedException();
    }

    internal static void DrawText(sbyte* utf8Ptr, S32X2 position,
                                  int size, Color32 color32,
                                  TextAlignmentFlags alignment) {
        UnmanagedManager.AssertMainThread();
        UnmanagedManager.AssertProperPointer(utf8Ptr);
        throw new NotImplementedException();
    }

    internal static void DrawText(byte* utf8Ptr, Vector2 position,
                                  int size, Color32 color32,
                                  TextAlignmentFlags alignment, FontInstance fontInstance) {
        UnmanagedManager.AssertMainThread();
        Font fontTtf = fontInstance.font;
        TTF.SetFontSize(fontTtf.fontPtr, size);
        if(fontTtf.textPtr == IntPtr.Zero) {
            if(rendererTextEngine == IntPtr.Zero)
                rendererTextEngine = TTF.CreateRendererTextEngine(RayBlastEngine.renderer);
            fontTtf.textPtr = TTF.CreateText(rendererTextEngine, fontTtf.fontPtr, "", UIntPtr.Zero);
        }
        SetTextString(fontTtf.textPtr, utf8Ptr, 0);
        TTF.GetTextSize(fontTtf.textPtr, out int w, out int h);
        var renderSize = new Vector2(w, h);
        if((alignment & TextAlignmentFlags.HRight) != 0)
            position.X -= renderSize.X;
        else if((alignment & TextAlignmentFlags.HCenter) != 0)
            position.X -= renderSize.X / 2f;
        if((alignment & TextAlignmentFlags.VBottom) != 0)
            position.Y -= renderSize.Y;
        else if((alignment & TextAlignmentFlags.VMiddle) != 0)
            position.Y -= renderSize.Y / 2f;
        TTF.SetFontWrapAlignment(fontTtf.fontPtr, (alignment & (TextAlignmentFlags)15) switch {
            TextAlignmentFlags.HLeft => TTF.HorizontalAlignment.Left,
            TextAlignmentFlags.HRight => TTF.HorizontalAlignment.Right,
            _ => TTF.HorizontalAlignment.Center
        });
        TTF.SetTextColor(fontTtf.textPtr, color32.r, color32.g, color32.b, color32.a);
        TTF.DrawRendererText(fontTtf.textPtr, position.X, position.Y);
    }

    public static Vector2 MeasureTextSizeDelta(string? text, int fontSize,
                                               Font? font = null) {
        if(string.IsNullOrWhiteSpace(text))
            return Vector2.Zero;
        if(font == null)
            throw new NullReferenceException(nameof(font));
        TTF.SetFontSize(font.fontPtr, fontSize);
        if(font.textPtr == IntPtr.Zero) {
            if(rendererTextEngine == IntPtr.Zero)
                rendererTextEngine = TTF.CreateRendererTextEngine(RayBlastEngine.renderer);
            font.textPtr = TTF.CreateText(rendererTextEngine, font.fontPtr, "", UIntPtr.Zero);
        }
        TTF.SetTextString(font.textPtr, text, 0);
        TTF.GetTextSize(font.textPtr, out int w, out int h);
        return new Vector2(w, h);
    }

    public static Vector2 MeasureTextSizeDelta(byte* utf8Ptr, int fontSize,
                                               Font? font = null) {
        if(font == null)
            throw new NullReferenceException(nameof(font));
        TTF.SetFontSize(font.fontPtr, fontSize);
        if(font.textPtr == IntPtr.Zero) {
            if(rendererTextEngine == IntPtr.Zero)
                rendererTextEngine = TTF.CreateRendererTextEngine(RayBlastEngine.renderer);
            font.textPtr = TTF.CreateText(rendererTextEngine, font.fontPtr, "", UIntPtr.Zero);
        }
        SetTextString(font.textPtr, utf8Ptr, 0);
        TTF.GetTextSize(font.textPtr, out int w, out int h);
        return new Vector2(w, h);
    }

    [DllImport("SDL3_ttf", EntryPoint = "TTF_SetTextString", ExactSpelling = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static extern sbyte SetTextString(nint textNative, byte* stringNative,
                                              ulong lengthNative);

    public static void DrawPixels(ReadOnlySpan<Vector2> points, Color32 color32) {
        UnmanagedManager.AssertMainThread();
        SDL.SetRenderDrawColor(RayBlastEngine.renderer, color32.r, color32.g, color32.b, color32.a);
        //TODO: Figure out how to eliminate allocation
        var temp = new SDL.FPoint[points.Length];
        MemoryMarshal.Cast<Vector2, SDL.FPoint>(points).CopyTo(temp);
        SDL.RenderPoints(RayBlastEngine.renderer, temp, points.Length);
    }

    public static void DrawRectangle(float x, float y,
                                     float width, float height,
                                     Color32 color32) {
        UnmanagedManager.AssertMainThread();
        SDL.SetRenderDrawColor(RayBlastEngine.renderer, color32.r, color32.g, color32.b, color32.a);
        SDL.RenderFillRect(RayBlastEngine.renderer, new SDL.FRect {
            X = x, Y = y, W = width, H = height
        });
    }

    public static void DrawRectangleOutline(float x, float y,
                                            float width, float height,
                                            uint outlineSize, Color32 color32) {
        if(outlineSize >= width / 2f || outlineSize >= height / 2f) {
            DrawRectangle(x, y, width, height, color32);
        }
        else {
            UnmanagedManager.AssertMainThread();
            SDL.SetRenderDrawColor(RayBlastEngine.renderer, color32.r, color32.g, color32.b, color32.a);
            SDL.RenderFillRect(RayBlastEngine.renderer, new SDL.FRect {
                X = x, Y = y, W = outlineSize, H = height
            });
            SDL.RenderFillRect(RayBlastEngine.renderer, new SDL.FRect {
                X = x, Y = y, W = width, H = outlineSize
            });
            SDL.RenderFillRect(RayBlastEngine.renderer, new SDL.FRect {
                X = x + width - outlineSize, Y = y, W = outlineSize, H = height
            });
            SDL.RenderFillRect(RayBlastEngine.renderer, new SDL.FRect {
                X = x, Y = y + height - outlineSize, W = width, H = outlineSize
            });
        }
    }

    public static void Draw3DTriangle(Vector3 a, Vector3 b,
                                      Vector3 c, Color32 color32) {
        UnmanagedManager.AssertMainThread();
        //TODO_URGENT: Render triangle
    }

    public static void Draw3DQuad(Vector3 origin, Quaternion rotation,
                                  Vector2 size, Color32 color32) {
        UnmanagedManager.AssertMainThread();
        Vector3 a = origin + Vector3.Transform(new Vector3(-size.X / 2f, size.Y / 2f, 0f), rotation);
        Vector3 b = origin + Vector3.Transform(new Vector3(size.X / 2f, size.Y / 2f, 0f), rotation);
        Vector3 c = origin + Vector3.Transform(new Vector3(-size.X / 2f, -size.Y / 2f, 0f), rotation);
        Vector3 d = origin + Vector3.Transform(new Vector3(size.X / 2f, -size.Y / 2f, 0f), rotation);
        //TODO_URGENT: Render triangles
    }

    public static void Draw3DRectangularPrism(Vector3 origin, Quaternion rotation,
                                              Vector3 dimensions, Color32 color32) {
        UnmanagedManager.AssertMainThread();
        Vector3 a = origin + Vector3.Transform(new Vector3(dimensions.X / 2f, -dimensions.Y / 2f, -dimensions.Z / 2f), rotation);
        Vector3 b = origin + Vector3.Transform(new Vector3(-dimensions.X / 2f, -dimensions.Y / 2f, -dimensions.Z / 2f), rotation);
        Vector3 c = origin + Vector3.Transform(new Vector3(dimensions.X / 2f, -dimensions.Y / 2f, dimensions.Z / 2f), rotation);
        Vector3 d = origin + Vector3.Transform(new Vector3(-dimensions.X / 2f, -dimensions.Y / 2f, dimensions.Z / 2f), rotation);
        Vector3 e = origin + Vector3.Transform(new Vector3(dimensions.X / 2f, dimensions.Y / 2f, -dimensions.Z / 2f), rotation);
        Vector3 f = origin + Vector3.Transform(new Vector3(-dimensions.X / 2f, dimensions.Y / 2f, -dimensions.Z / 2f), rotation);
        Vector3 g = origin + Vector3.Transform(new Vector3(-dimensions.X / 2f, dimensions.Y / 2f, dimensions.Z / 2f), rotation);
        Vector3 h = origin + Vector3.Transform(new Vector3(dimensions.X / 2f, dimensions.Y / 2f, dimensions.Z / 2f), rotation);
        Span<Vector3> array = stackalloc Vector3[14] {
            d, c, g, h, e, c, a, d, b, g,
            f, e, b, a
        };
        fixed(Vector3* points = array) {
            //TODO_URGENT: Render triangles
        }
    }

    public static void DrawImage(Texture texture, int x,
                                 int y, Color32 tint) {
        UnmanagedManager.AssertMainThread();
        if(RayBlastEngine.renderer == IntPtr.Zero)
            throw new RayBlastEngineException("Renderer not initialized");
        if(texture.internalTexture == IntPtr.Zero) {
            IntPtr intPtr = texture.internalDataPtr;
            if(intPtr == IntPtr.Zero)
                throw new RayBlastEngineException("Attempted to render uninitialized texture");
            texture.internalTexture = SDL.CreateTextureFromSurface(RayBlastEngine.renderer, intPtr);
            if(texture.internalTexture == IntPtr.Zero)
                throw new RayBlastEngineException($"Failed to create texture from surface: {SDL.GetError()}");
            SDL.SetTextureBlendMode(texture.internalTexture, Graphics.BLEND_WITH_PREMULTIPLY_ALPHA);
        }
        SDL.SetTextureColorMod(texture.internalTexture, tint.r, tint.g, tint.b);
        SDL.SetTextureAlphaMod(texture.internalTexture, tint.a);
        var srcRect = new SDL.FRect {
            X = 0, Y = 0, W = texture.Width, H = texture.Height
        };
        var destRect = new SDL.FRect {
            X = x, Y = y, W = texture.Width, H = texture.Height
        };
        SDL.RenderTexture(RayBlastEngine.renderer, texture.internalTexture, srcRect, destRect);
    }

    public static void DrawSubimage(TextureSubimage subimage, Vector2 destination,
                                    Color32 tint) {
        DrawSubimage(subimage, subimage.rectangle with {
                         X = destination.X, Y = destination.Y
                     }, new Vector2(0.5f, 0.5f), 0f,
                     tint);
    }

    public static void DrawSubimage(TextureSubimage subimage, Vector4 destination,
                                    Color32 tint) {
        DrawSubimage(subimage, destination, new Vector2(0.5f, 0.5f), 0f, tint);
    }

    public static void DrawSubimage(TextureSubimage subimage, Vector4 destination,
                                    Vector2 origin, Color32 tint) {
        DrawSubimage(subimage, destination, origin, 0f, tint);
    }

    public static void DrawSubimage(TextureSubimage subimage, Vector4 destination,
                                    Vector2 origin, float rotation,
                                    Color32 tint) {
        origin *= new Vector2(destination.Z, destination.W);
        destination.X -= origin.X;
        destination.Y -= origin.Y;
        UnmanagedManager.AssertMainThread();
        if(RayBlastEngine.renderer == IntPtr.Zero)
            throw new RayBlastEngineException("Renderer not initialized");
        if(subimage.texture.internalTexture == IntPtr.Zero) {
            IntPtr intPtr = subimage.texture.internalDataPtr;
            if(intPtr == IntPtr.Zero)
                throw new RayBlastEngineException("Attempted to render uninitialized texture");
            IntPtr texture = SDL.CreateTextureFromSurface(RayBlastEngine.renderer, intPtr);
            if(texture == IntPtr.Zero)
                throw new RayBlastEngineException($"Failed to create texture from surface: {SDL.GetError()}");
            subimage.texture.internalTexture = texture;
            SDL.SetTextureBlendMode(subimage.texture.internalTexture, Graphics.BLEND_WITH_PREMULTIPLY_ALPHA);
        }
        SDL.SetTextureColorMod(subimage.texture.internalTexture, tint.r, tint.g, tint.b);
        SDL.SetTextureAlphaMod(subimage.texture.internalTexture, tint.a);
        var srcRect = new SDL.FRect {
            X = subimage.rectangle.X, Y = subimage.rectangle.Y, W = subimage.rectangle.Z, H = subimage.rectangle.W
        };
        var destRect = new SDL.FRect {
            X = destination.X, Y = destination.Y, W = destination.Z, H = destination.W
        };
        SDL.RenderTextureRotated(RayBlastEngine.renderer, subimage.texture.internalTexture, srcRect, destRect, rotation, (IntPtr)(&origin),
                                 SDL.FlipMode.None);
    }

    public static void BeginShaderMode(Shader shader) {
        UnmanagedManager.AssertMainThread();
        throw new NotImplementedException();
    }

    public static void EndShaderMode() {
        UnmanagedManager.AssertMainThread();
        throw new NotImplementedException();
    }

    public static void Draw3DLine(Vector3 start, Vector3 end,
                                  ColorF color) {
        throw new NotImplementedException();
    }

    // [DllImport("raylib", EntryPoint = "rlSetClipPlanes", CallingConvention = CallingConvention.Cdecl)]
    // public static extern void SetClipPlanes(
    //     double near,
    //     double far
    // );
}
