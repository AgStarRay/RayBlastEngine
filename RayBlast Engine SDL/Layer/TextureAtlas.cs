using System.Numerics;

namespace RayBlast; 

public class TextureAtlas {
    public readonly Texture texture;
    public readonly List<Vector4> rectangles;

    public TextureAtlas(Texture texture, List<Vector4> rectangles) {
        this.texture = texture;
        this.rectangles = rectangles;
    }
}
