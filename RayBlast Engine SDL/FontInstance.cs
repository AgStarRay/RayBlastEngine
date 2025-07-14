namespace RayBlast.Text;

public struct FontInstance(
    Font font,
    ColorF outlineColor,
    float dilation = 0.5f,
    float outlineDilation = 0.25f) {
    public readonly Font font = font;
    public readonly float dilation = dilation;
    public readonly ColorF outlineColor = outlineColor;
    public readonly float outlineDilation = outlineDilation;
}
