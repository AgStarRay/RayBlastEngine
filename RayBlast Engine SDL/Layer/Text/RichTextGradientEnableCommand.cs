using System.Runtime.InteropServices;

namespace RayBlast.Text;

public class RichTextGradientEnableCommand : IRichTextCommand {
    public int CharacterIndex { get; set; }
    public int EndIndex { get; set; }

    public void Reset() {
    }

    public void Edit(RichTextBatch batch) {
        if(batch.colors.Count < batch.subimages.Count * 4) {
            batch.colors.EnsureCapacity(batch.subimages.Count * 4);
            Span<ColorF> span = CollectionsMarshal.AsSpan(batch.colors);
            batch.colors.AddRange(span);
            batch.colors.AddRange(span);
            batch.colors.AddRange(span);
        }
        // if (batch.colors.Count > 0)
        // {
        //     ColorF firstColor = batch.colors[0];
        //     while (batch.colors.Count < batch.subimages.Count * 4) {
        //         batch.colors.Add(firstColor);
        //         batch.colors.Add(firstColor);
        //         batch.colors.Add(firstColor);
        //     }
        // }
    }
}
