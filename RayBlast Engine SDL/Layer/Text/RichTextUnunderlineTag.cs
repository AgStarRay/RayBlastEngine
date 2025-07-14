namespace RayBlast.Text;

public class RichTextUnunderlineTag : RichTextTag {
    private static readonly RichTextLayout[] LAYOUTS = [new(0, LayoutType.Style, BitConverter.Int32BitsToSingle((int)FontStyle.Underline), 1f)];
    public override string StartTag => "</u>";

    public override bool ProducesLayout => true;

    public override ReadOnlySpan<RichTextLayout> GetLayouts(ReadOnlySpan<char> substring) {
        return LAYOUTS;
    }
}
