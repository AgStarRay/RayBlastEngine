namespace RayBlast.Text;

public class RichTextUnitalicTag : RichTextTag {
    private static readonly RichTextLayout[] LAYOUTS = [new(0, LayoutType.Style, BitConverter.Int32BitsToSingle((int)FontStyle.Italics), 1f)];
    public override string StartTag => "</i>";

    public override bool ProducesLayout => true;

    public override ReadOnlySpan<RichTextLayout> GetLayouts(ReadOnlySpan<char> substring) {
        return LAYOUTS;
    }
}
