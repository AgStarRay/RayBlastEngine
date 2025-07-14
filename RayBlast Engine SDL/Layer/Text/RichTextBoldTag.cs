namespace RayBlast.Text;

public class RichTextBoldTag : RichTextTag {
    private static readonly RichTextLayout[] LAYOUTS = [new(0, LayoutType.Style, BitConverter.Int32BitsToSingle((int)FontStyle.Bold))];

    public override string StartTag => "<b>";

    public override bool ProducesLayout => true;

    public override ReadOnlySpan<RichTextLayout> GetLayouts(ReadOnlySpan<char> substring) {
        return LAYOUTS;
    }
}
