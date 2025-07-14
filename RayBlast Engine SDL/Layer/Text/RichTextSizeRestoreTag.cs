namespace RayBlast.Text;

public class RichTextSizeRestoreTag : RichTextTag {
    internal static readonly Stack<(float, bool)> PREVIOUS_SIZES = new();
    private static readonly RichTextLayout[] ARRAY = new RichTextLayout[1];

    public override string StartTag => "</size>";

    public override bool ProducesLayout => true;

    public override void Reset() {
        PREVIOUS_SIZES.Clear();
    }

    public override ReadOnlySpan<RichTextLayout> GetLayouts(ReadOnlySpan<char> substring) {
        if(PREVIOUS_SIZES.Count > 0) {
            (float, bool) restoreSize = PREVIOUS_SIZES.Pop();
            ARRAY[0] = new RichTextLayout(0, LayoutType.FontSize, 1f / restoreSize.Item1, restoreSize.Item2 ? 1f : 0f);
        }
        else
            ARRAY[0] = new RichTextLayout(0, LayoutType.FontSize, 1f);
        return ARRAY;
    }
}
