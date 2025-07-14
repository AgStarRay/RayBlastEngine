using System.Drawing;

namespace RayBlast.Text;

public class RichTextColorCommand(ColorF newColor) : IRichTextCommand {
    private static ColorF currentColor;

    public ColorF newColor = newColor;

    public int CharacterIndex { get; set; }
    public int EndIndex { get; set; }

    public void Reset() {
        currentColor = ColorF.WHITE;
    }

    public void Edit(RichTextBatch batch) {
        RichTextColorRestoreCommand.PREVIOUS_COLORS.Push(currentColor);
        currentColor = newColor;
        for(int i = CharacterIndex; i < EndIndex; i++) {
            batch.colors[i] = newColor;
        }
    }
}
