namespace RayBlast;

public interface IFontAtlas {
    internal Texture Atlas { get; }
    internal Dictionary<char, TextureSubimage> AtlasGlyphs { get; }
}
