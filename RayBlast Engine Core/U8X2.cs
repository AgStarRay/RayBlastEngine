// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace RayBlast;

// ReSharper disable once InconsistentNaming
public record struct U8X2(byte X, byte Y) {
	public byte X = X;
	public byte Y = Y;

	public U8X2(byte x) : this(x, x) {
	}

	public static U8X2 operator +(U8X2 a, byte b) => new U8X2((byte)(a.X + b), (byte)(a.Y + b));
	public static U8X2 operator +(U8X2 a, U8X2 b) => new U8X2((byte)(a.X + b.X), (byte)(a.Y + b.Y));
	public static U8X2 operator -(U8X2 a, byte b) => new U8X2((byte)(a.X - b), (byte)(a.Y - b));
	public static U8X2 operator -(U8X2 a, U8X2 b) => new U8X2((byte)(a.X - b.X), (byte)(a.Y - b.Y));
	public static U8X2 operator *(U8X2 a, byte b) => new U8X2((byte)(a.X * b), (byte)(a.Y * b));
	public static U8X2 operator *(U8X2 a, U8X2 b) => new U8X2((byte)(a.X * b.X), (byte)(a.Y * b.Y));
	public static U8X2 operator /(U8X2 a, byte b) => new U8X2((byte)(a.X / b), (byte)(a.Y / b));
	[NonMIMD]
	public static U8X2 operator /(U8X2 a, U8X2 b) => new U8X2((byte)(a.X / b.X), (byte)(a.Y / b.Y));
	public static U8X2 operator %(U8X2 a, byte b) => new U8X2((byte)(a.X % b), (byte)(a.Y % b));
	[NonMIMD]
	public static U8X2 operator %(U8X2 a, U8X2 b) => new U8X2((byte)(a.X % b.X), (byte)(a.Y % b.Y));
	public static U8X2 operator &(U8X2 a, byte b) => new U8X2((byte)(a.X & b), (byte)(a.Y & b));
	public static U8X2 operator &(U8X2 a, U8X2 b) => new U8X2((byte)(a.X & b.X), (byte)(a.Y & b.Y));
	public static U8X2 operator |(U8X2 a, byte b) => new U8X2((byte)(a.X | b), (byte)(a.Y | b));
	public static U8X2 operator |(U8X2 a, U8X2 b) => new U8X2((byte)(a.X | b.X), (byte)(a.Y | b.Y));
	public static U8X2 operator ^(U8X2 a, byte b) => new U8X2((byte)(a.X ^ b), (byte)(a.Y ^ b));
	public static U8X2 operator ^(U8X2 a, U8X2 b) => new U8X2((byte)(a.X ^ b.X), (byte)(a.Y ^ b.Y));
	public static U8X2 operator <<(U8X2 a, int b) => new U8X2((byte)(a.X << b), (byte)(a.Y << b));
	public static U8X2 operator >>(U8X2 a, int b) => new U8X2((byte)(a.X >> b), (byte)(a.Y >> b));
}
