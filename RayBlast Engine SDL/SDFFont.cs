using System.Diagnostics;
using System.Numerics;
using System.Text;
using SDL3;

namespace RayBlast.Text;

public class SDFFont : IDisposable {
    private const string ALL_CHARACTERS =
        "□�☺☻♥♦♣♠•◘○◙♂♀♪♫☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼ !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~⌂ÇüéâäàåçêëèïîìÄÅÉæÆôöòûùÿÖÜ¢£¥₧ƒáíóúñÑªº¿⌐¬½¼¡«»░▒▓│┤╡╢╖╕╣║╗╝╛┐└┴┬├─┼╞╟╚╔╩╦╠═╬╧╨╤╥╙╘╒╓╫╪┘┌█▄▌▐▀αßΓπΣσµτΦΘΩδ∞φε∩≡±≥≤⌠⌡÷≈°∙·√ⁿ²■ ™©";
    private static readonly Dictionary<char, int> ALL_CHARACTER_CODEPOINTS = new();
    private static readonly SDL.Color[] PALETTE_COLORS = new SDL.Color[256];
    private static int GLYPH_INDEX_0 = ALL_CHARACTERS.IndexOf('0');

    private readonly int numberMonospaceAmount;
    public readonly Font font;
    internal readonly Texture atlas;
    internal Dictionary<char, TextureSubimage> atlasGlyphs = new();
    private bool atlasReady = false;
    private readonly Dictionary<char, Texture> pendingRasterGlyphs = new();

    static SDFFont() {
        for(int i = 0; i < ALL_CHARACTERS.Length; i++) {
            int codepoint = char.ConvertToUtf32(ALL_CHARACTERS, i);
            string codepointStr = char.ConvertFromUtf32(codepoint);
            if(codepointStr.Length == 1)
                ALL_CHARACTER_CODEPOINTS[codepointStr[0]] = codepoint;
            else
                Debug.LogWarning($"Surrogate pair detected: {codepointStr}", includeStackTrace: false);
        }
        for(int i = 0; i < PALETTE_COLORS.Length; i++) {
            PALETTE_COLORS[i] = new SDL.Color {
                R = (byte)i, G = (byte)i, B = (byte)i, A = (byte)i
            };
        }
    }

    private SDFFont(Font font, int numberMonospaceAmount) {
        this.font = font;
        this.numberMonospaceAmount = numberMonospaceAmount;
        IntPtr atlasPtr = SDL.CreateSurface(1024, 1024, SDL.PixelFormat.Index8);
        atlas = new Texture(atlasPtr);
        IntPtr atlasPalettePtr = SDL.CreateSurfacePalette(atlasPtr);
        if(!SDL.SetPaletteColors(atlasPalettePtr, PALETTE_COLORS, 0, PALETTE_COLORS.Length))
            Debug.LogError($"Failed to set atlas palette: {SDL.GetError()}");
    }

    public static SDFFont Create(Uri filePath, float ptSize = 48f,
                                 int numberMonospaceAmount = int.MinValue) {
        long startTimestamp = Stopwatch.GetTimestamp();
        using var font = new Font(filePath, ptSize);
        if(!TTF.SetFontSDF(font.fontPtr, enabled: true))
            throw new RayBlastEngineException($"Font could not be initialized with SDF: {SDL.GetError()}");
        var sdfFont = new SDFFont(font, numberMonospaceAmount);
        var remainingGlyphs = new List<char>();
        try {
            foreach((char key, int codepoint) in ALL_CHARACTER_CODEPOINTS) {
                if(TTF.FontHasGlyph(font.fontPtr, (uint)codepoint)) {
                    IntPtr image = TTF.RenderGlyphShaded(font.fontPtr, (uint)codepoint, new SDL.Color {
                        R = 255, G = 255, B = 255, A = 255
                    }, new SDL.Color {
                        R = 0, G = 0, B = 0, A = 0
                    });
                    if(image != IntPtr.Zero) {
                        var texture = new Texture(image);
                        sdfFont.pendingRasterGlyphs[key] = texture;
                        remainingGlyphs.Add(key);
                    }
                    else if(codepoint != 0)
                        Debug.LogError($"Failed to render glyph {codepoint} ({key}): {SDL.GetError()}", includeStackTrace: false);
                }
                // else
                //     Debug.LogWarning($"Font does not have glyph {codepoint} ({key})", includeStackTrace: false);
            }
            var minSize = new Vector2(sdfFont.atlas.Width, sdfFont.atlas.Height);
            var maxSize = new Vector2(0, 0);
            foreach(KeyValuePair<char, Texture> kvp in sdfFont.pendingRasterGlyphs) {
                minSize = new Vector2(Math.Min(minSize.X, kvp.Value.Width), Math.Min(minSize.Y, kvp.Value.Height));
                maxSize = new Vector2(Math.Max(maxSize.X, kvp.Value.Width), Math.Max(maxSize.Y, kvp.Value.Height));
            }
            Debug.Log($"Glyph size range: {minSize} to {maxSize}\nTotal glyphs to render: {remainingGlyphs.Count}");
            int y = 1;
            int separation = (int)maxSize.Y + 1;
            int x = 1;
            while(remainingGlyphs.Count > 0) {
                int previousX = x;
                foreach(char key in remainingGlyphs) {
                    Texture g = sdfFont.pendingRasterGlyphs[key];
                    if(x + g.Width <= sdfFont.atlas.Width - 1) {
                        sdfFont.atlasGlyphs[key] = new TextureSubimage(sdfFont.atlas, new Vector4(x, y, g.Width, g.Height));
                        x += g.Width + 1;
                        remainingGlyphs.Remove(key);
                        break;
                    }
                }
                if(x == previousX) {
                    x = 1;
                    y += separation;
                    if(y > sdfFont.atlas.Height - separation)
                        throw new RayBlastEngineException("Ran out of room to render all the glyphs");
                }
            }
        }
        catch {
            sdfFont.Dispose();
            throw;
        }
        Debug.Log($"Font SDF atlas generated in {(Stopwatch.GetTimestamp() - startTimestamp) / (double)TimeSpan.TicksPerSecond}");
        if(Environment.CurrentManagedThreadId == UnmanagedManager.mainThreadId)
            sdfFont.ReadyAtlas();
        return sdfFont;
    }

    internal unsafe void ReadyAtlas() {
        if(atlasReady)
            return;
        UnmanagedManager.AssertMainThread();
        long startTimestamp = Stopwatch.GetTimestamp();
        atlasReady = true;
        foreach(KeyValuePair<char, Texture> kvp in pendingRasterGlyphs) {
            TextureSubimage targetSubimage = atlasGlyphs[kvp.Key];
            var destRect = new SDL.Rect {
                X = (int)targetSubimage.rectangle.X, Y = (int)targetSubimage.rectangle.Y, W = kvp.Value.Width, H = kvp.Value.Height
            };
            bool successfulBlit = SDL.BlitSurface(kvp.Value.internalDataPtr, IntPtr.Zero, atlas.internalDataPtr, (IntPtr)(&destRect));
            if(!successfulBlit)
                throw new RayBlastEngineException($"Failed to blit: {SDL.GetError()}");
            kvp.Value.Dispose();
        }
        pendingRasterGlyphs.Clear();
        Debug.Log($"Font SDF atlas readied in {(Stopwatch.GetTimestamp() - startTimestamp) / (double)TimeSpan.TicksPerSecond}");
    }

    private void ReleaseUnmanagedResources() {
        font.Dispose();
        atlas.Dispose();
        foreach(KeyValuePair<char, Texture> kvp in pendingRasterGlyphs) {
            kvp.Value.Dispose();
        }
    }

    protected virtual void Dispose(bool disposing) {
        ReleaseUnmanagedResources();
        if(disposing) {
            font.Dispose();
            atlas.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SDFFont() {
        Dispose(false);
    }
}
