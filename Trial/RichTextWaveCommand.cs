using System.Numerics;
using RayBlast;
using RayBlast.Text;

public class RichTextWaveCommand : IRichTextCommand {
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
            double theta = Time.time * 3f + i * 0.2f;
            Vector4 newDest = batch.dest[i];
            newDest.X += (float)Math.Sin(theta) * 16f;
            newDest.Y += (float)Math.Cos(theta) * 16f;
            batch.dest[i] = newDest;
        }
    }
}
