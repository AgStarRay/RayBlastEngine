using System.Numerics;

namespace RayBlast;

public class TextureSubimage : IDisposable {
    public readonly Texture texture;
    public Vector4 rectangle;
    public Vector2 pivot;
    public float pixelsPerUnit;
    public uint extrude;

    public TextureSubimage(Texture texture, Vector4 rectangle) {
        this.texture = texture;
        rectangle.Y = texture.Height - rectangle.Y - rectangle.W;
        this.rectangle = rectangle;
        pivot = new Vector2(0.5f, 0.5f);
        pixelsPerUnit = 1f;
        extrude = 0;
    }

    public TextureSubimage(Texture texture, Vector4 rectangle,
                           Vector2 pivot, float pixelsPerUnit,
                           uint extrude) {
        this.texture = texture;
        rectangle.Y = texture.Height - rectangle.Y - rectangle.W;
        this.rectangle = rectangle;
        this.pivot = pivot;
        this.pixelsPerUnit = pixelsPerUnit;
        this.extrude = extrude;
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
    }
}
