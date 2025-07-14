namespace RayBlast;

public partial struct Color32 {
	public static readonly Color32 BLACK = new Color32(0, 0, 0);
	public static readonly Color32 RED = new Color32(255, 0, 0);
	public static readonly Color32 YELLOW = new Color32(255, 255, 0);
	public static readonly Color32 GREEN = new Color32(0, 255, 0);
	public static readonly Color32 CYAN = new Color32(0, 255, 255);
	public static readonly Color32 BLUE = new Color32(0, 0, 255);
	public static readonly Color32 MAGENTA = new Color32(255, 0, 255);
	public static readonly Color32 WHITE = new Color32(255, 255, 255);
	
	public byte r;
	public byte g;
	public byte b;
	public byte a;

	public Color32(int r, int g,
				 int b, int a = 255) {
		this.r = (byte)r;
		this.g = (byte)g;
		this.b = (byte)b;
		this.a = (byte)a;
	}

	public Color32(byte r, byte g,
				 byte b, byte a = 255) {
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public static Color32 operator *(Color32 a, Color32 b) {
		return new Color32(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);
	}

	public static Color32 operator -(Color32 a, Color32 b) {
		return new Color32(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a);
	}
}
