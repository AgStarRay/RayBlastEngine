namespace RayBlast;

public struct QualitySettings {
	public string name;
	public bool useMsaa4x;
	public TextureFilterLevel filterLevel;
}

public enum TextureFilterLevel {
	Point = 0,
	Bilinear,
	Trilinear,
	Anisotropic4X,
	Anisotropic8X,
	Anisotropic16X,
}
