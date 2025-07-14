using RayBlast;
using RayBlast.Text;

public class RichTextRainbowCommand : IRichTextCommand {
    public int CharacterIndex {
        get;
        set;
    }
    public int EndIndex {
        get;
        set;
    }

    public void Reset() {
    }

    public void Edit(RichTextBatch batch) {
        int endIndex = EndIndex;
        if(endIndex < CharacterIndex)
            endIndex = batch.colors.Count;
        for(int i = CharacterIndex; i < endIndex; i++) {
            double theta = Time.time * 2f + i * 0.1f;
            var newColor = new ColorF((float)(0.75 + Math.Sin(theta) * 0.25f),
                                      (float)(0.75 + Math.Sin(theta + Math.PI * 2 / 3) * 0.25f),
                                      (float)(0.75 + Math.Sin(theta + Math.PI * 4 / 3) * 0.25f));
            batch.colors[i] = newColor;
        }
    }
}
