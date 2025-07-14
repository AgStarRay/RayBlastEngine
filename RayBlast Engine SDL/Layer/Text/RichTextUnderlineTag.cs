namespace RayBlast.Text;

public class RichTextUnderlineTag : RichTextTag {
    private static readonly RichTextLayout[] LAYOUTS = [new(0, LayoutType.Style, BitConverter.Int32BitsToSingle((int)FontStyle.Underline))];
    public override string StartTag => "<u>";

    public override bool ProducesLayout => true;

    public override ReadOnlySpan<RichTextLayout> GetLayouts(ReadOnlySpan<char> substring) {
        return LAYOUTS;
    }
}
