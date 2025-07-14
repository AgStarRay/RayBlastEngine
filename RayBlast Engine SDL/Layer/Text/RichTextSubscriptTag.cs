namespace RayBlast.Text;

public class RichTextSubscriptTag : RichTextTag {
    private static readonly RichTextLayout[] LAYOUTS = [new(0, LayoutType.FontSize, 0.75f), new(0, LayoutType.VerticalOffset, -0.25f)];
    public override string StartTag => "<sub>";

    public override bool ProducesLayout => true;

    public override ReadOnlySpan<RichTextLayout> GetLayouts(ReadOnlySpan<char> substring) {
        return LAYOUTS;
    }
}
