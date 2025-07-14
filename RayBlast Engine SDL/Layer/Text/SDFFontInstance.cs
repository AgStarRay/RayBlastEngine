namespace RayBlast.Text;

public struct SDFFontInstance(
    SDFFont sdfFont,
    ColorF outlineColor,
    float dilation = 0.5f,
    float outlineDilation = 0.25f) {
    public readonly SDFFont sdfFont = sdfFont;
    public readonly float dilation = dilation;
    public readonly ColorF outlineColor = outlineColor;
    public readonly float outlineDilation = outlineDilation;
}
