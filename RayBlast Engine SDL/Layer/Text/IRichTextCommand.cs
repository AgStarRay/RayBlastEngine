namespace RayBlast.Text;

public interface IRichTextCommand {
    public int CharacterIndex { get; set; }
    public int EndIndex { get; set; }

    void Reset();
    void Edit(RichTextBatch batch);
}
