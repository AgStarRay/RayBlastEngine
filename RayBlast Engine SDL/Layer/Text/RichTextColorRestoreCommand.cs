namespace RayBlast.Text;

public class RichTextColorRestoreCommand : IRichTextCommand {
    internal static readonly Stack<ColorF> PREVIOUS_COLORS = new();

    public int CharacterIndex { get; set; }
    public int EndIndex { get; set; }

    public void Reset() {
        PREVIOUS_COLORS.Clear();
    }

    public void Edit(RichTextBatch batch) {
        if(PREVIOUS_COLORS.Count > 0) {
            ColorF restoreColor = PREVIOUS_COLORS.Pop();
            for(int i = CharacterIndex; i < EndIndex; i++) {
                batch.colors[i] = restoreColor;
            }
        }
    }
}
