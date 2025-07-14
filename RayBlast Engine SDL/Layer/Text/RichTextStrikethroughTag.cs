namespace RayBlast.Text;

public class RichTextStrikethroughTag : RichTextTag {
    private static readonly RichTextLayout[] LAYOUTS = [new(0, LayoutType.Style, BitConverter.Int32BitsToSingle((int)FontStyle.Strikethrough))];
    public override string StartTag => "<s>";

    public override bool ProducesLayout => true;

    public override ReadOnlySpan<RichTextLayout> GetLayouts(ReadOnlySpan<char> substring) {
        return LAYOUTS;
    }
}
