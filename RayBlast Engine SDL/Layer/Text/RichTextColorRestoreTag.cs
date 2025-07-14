namespace RayBlast.Text;

public class RichTextColorRestoreTag : RichTextTag {
    public override string StartTag => "</color>";

    public override bool ProducesCommand => true;

    public override void Reset() {
    }

    public override IRichTextCommand GetCommand(ReadOnlySpan<char> substring) {
        return new RichTextColorRestoreCommand();
    }
}
