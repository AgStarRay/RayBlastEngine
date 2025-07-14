namespace RayBlast.Text;

public struct RichTextLayout {
    public int characterIndex;
    public LayoutType type;
    public float parameterA;
    public float parameterB;

    public RichTextLayout(int characterIndex, LayoutType type) {
        this.characterIndex = characterIndex;
        this.type = type;
        parameterA = float.NaN;
        parameterB = float.NaN;
    }

    public RichTextLayout(int characterIndex, LayoutType type,
                          float parameter) {
        this.characterIndex = characterIndex;
        this.type = type;
        parameterA = parameter;
        parameterB = float.NaN;
    }

    public RichTextLayout(int characterIndex, LayoutType type,
                          float parameterA, float parameterB) {
        this.characterIndex = characterIndex;
        this.type = type;
        this.parameterA = parameterA;
        this.parameterB = parameterB;
    }
}
