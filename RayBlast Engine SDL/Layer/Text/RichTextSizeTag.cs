namespace RayBlast.Text;

public class RichTextSizeTag : RichTextTag {
    private static readonly RichTextLayout[] ARRAY = new RichTextLayout[1];

    public override string StartTag => "<size=";
    public override string EndTag => ">";

    public override bool ProducesLayout => true;

    public override ReadOnlySpan<RichTextLayout> GetLayouts(ReadOnlySpan<char> substring) {
        if(substring[^1] == '%')
            ARRAY[0] = new RichTextLayout(0, LayoutType.FontSize, float.TryParse(substring[..^1], out float result) ? result / 100f : 1f, 0f);
        else
            ARRAY[0] = new RichTextLayout(0, LayoutType.FontSize, float.TryParse(substring, out float result2) ? result2 : 1f, 1f);
        return ARRAY;
    }
}
