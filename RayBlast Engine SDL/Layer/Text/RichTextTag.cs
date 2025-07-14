namespace RayBlast.Text;

public abstract class RichTextTag {
    public abstract string StartTag { get; }
    public virtual string EndTag => "";
    public virtual bool ProducesCommand => false;
    public virtual bool ProducesLayout => false;

    public virtual void Reset() {
    }

    public virtual ReadOnlySpan<RichTextLayout> GetLayouts(ReadOnlySpan<char> substring) {
        throw new NotSupportedException();
    }

    public virtual IRichTextCommand GetCommand(ReadOnlySpan<char> substring) {
        throw new NotSupportedException();
    }
}
