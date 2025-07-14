// ReSharper disable FieldCanBeMadeReadOnly.Global

using System.Numerics;

namespace RayBlast;

// ReSharper disable once InconsistentNaming
public record struct S32X4(int X, int Y,
						   int Z, int W) {
	public int X = X;
	public int Y = Y;
	public int Z = Z;
	public int W = W;

	public S32X4(int x) : this(x, x, x, x) {
	}

	public static explicit operator Vector4(S32X4 a) {
		return new Vector4(a.X, a.Y, a.Z, a.W);
	}

	public static S32X4 operator +(S32X4 a, int b) => new S32X4(a.X + b, a.Y + b, a.Z + b, a.W + b);
	public static S32X4 operator +(S32X4 a, S32X4 b) => new S32X4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
	public static S32X4 operator -(S32X4 a, int b) => new S32X4(a.X - b, a.Y - b, a.Z - b, a.W - b);
	public static S32X4 operator -(S32X4 a, S32X4 b) => new S32X4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
	public static S32X4 operator *(S32X4 a, int b) => new S32X4(a.X * b, a.Y * b, a.Z * b, a.W * b);
	public static S32X4 operator *(S32X4 a, S32X4 b) => new S32X4(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
	public static S32X4 operator /(S32X4 a, int b) => new S32X4(a.X / b, a.Y / b, a.Z / b, a.W / b);

	[NonMIMD]
	public static S32X4 operator /(S32X4 a, S32X4 b) => new S32X4(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);

	public static S32X4 operator %(S32X4 a, int b) => new S32X4(a.X % b, a.Y % b, a.Z % b, a.W % b);

	[NonMIMD]
	public static S32X4 operator %(S32X4 a, S32X4 b) => new S32X4(a.X % b.X, a.Y % b.Y, a.Z % b.Z, a.W % b.W);

	public static S32X4 operator &(S32X4 a, int b) => new S32X4(a.X & b, a.Y & b, a.Z & b, a.W & b);
	public static S32X4 operator &(S32X4 a, S32X4 b) => new S32X4(a.X & b.X, a.Y & b.Y, a.Z & b.Z, a.W & b.W);
	public static S32X4 operator |(S32X4 a, int b) => new S32X4(a.X | b, a.Y | b, a.Z | b, a.W | b);
	public static S32X4 operator |(S32X4 a, S32X4 b) => new S32X4(a.X | b.X, a.Y | b.Y, a.Z | b.Z, a.W | b.W);
	public static S32X4 operator ^(S32X4 a, int b) => new S32X4(a.X ^ b, a.Y ^ b, a.Z ^ b, a.W ^ b);
	public static S32X4 operator ^(S32X4 a, S32X4 b) => new S32X4(a.X ^ b.X, a.Y ^ b.Y, a.Z ^ b.Z, a.W ^ b.W);
	public static S32X4 operator <<(S32X4 a, int b) => new S32X4(a.X << b, a.Y << b, a.Z << b, a.W << b);
	public static S32X4 operator >> (S32X4 a, int b) => new S32X4(a.X >> b, a.Y >> b, a.Z >> b, a.W >> b);

	public readonly override string ToString() {
		return $"{{{X}, {Y}, {Z}, {W}}}";
	}
}
