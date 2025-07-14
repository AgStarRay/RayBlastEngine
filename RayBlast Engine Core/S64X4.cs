// ReSharper disable FieldCanBeMadeReadOnly.Global

using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace RayBlast;

// ReSharper disable once InconsistentNaming
public record struct S64X4(long X, long Y,
						   long Z, long W) {
	public long X = X;
	public long Y = Y;
	public long Z = Z;
	public long W = W;

	public S64X4(long x) : this(x, x, x, x) {
	}

	public static explicit operator Vector4(S64X4 a) {
		return new Vector4(a.X, a.Y, a.Z, a.W);
	}

	public static S64X4 operator +(S64X4 a, long b) => new S64X4(a.X + b, a.Y + b, a.Z + b, a.W + b);
	public static S64X4 operator +(S64X4 a, S64X4 b) => new S64X4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
	public static S64X4 operator -(S64X4 a, long b) => new S64X4(a.X - b, a.Y - b, a.Z - b, a.W - b);
	public static S64X4 operator -(S64X4 a, S64X4 b) => new S64X4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
	public static S64X4 operator *(S64X4 a, long b) => new S64X4(a.X * b, a.Y * b, a.Z * b, a.W * b);
	public static S64X4 operator *(S64X4 a, S64X4 b) => new S64X4(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
	public static S64X4 operator /(S64X4 a, long b) => new S64X4(a.X / b, a.Y / b, a.Z / b, a.W / b);

	[NonMIMD]
	public static S64X4 operator /(S64X4 a, S64X4 b) => new S64X4(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);

	public static S64X4 operator %(S64X4 a, long b) => new S64X4(a.X % b, a.Y % b, a.Z % b, a.W % b);

	[NonMIMD]
	public static S64X4 operator %(S64X4 a, S64X4 b) => new S64X4(a.X % b.X, a.Y % b.Y, a.Z % b.Z, a.W % b.W);

	public static S64X4 operator &(S64X4 a, long b) => new S64X4(a.X & b, a.Y & b, a.Z & b, a.W & b);
	public static S64X4 operator &(S64X4 a, S64X4 b) => new S64X4(a.X & b.X, a.Y & b.Y, a.Z & b.Z, a.W & b.W);
	public static S64X4 operator |(S64X4 a, long b) => new S64X4(a.X | b, a.Y | b, a.Z | b, a.W | b);
	public static S64X4 operator |(S64X4 a, S64X4 b) => new S64X4(a.X | b.X, a.Y | b.Y, a.Z | b.Z, a.W | b.W);
	public static S64X4 operator ^(S64X4 a, long b) => new S64X4(a.X ^ b, a.Y ^ b, a.Z ^ b, a.W ^ b);
	public static S64X4 operator ^(S64X4 a, S64X4 b) => new S64X4(a.X ^ b.X, a.Y ^ b.Y, a.Z ^ b.Z, a.W ^ b.W);
	public static S64X4 operator <<(S64X4 a, int b) => new S64X4(a.X << b, a.Y << b, a.Z << b, a.W << b);
	public static S64X4 operator >> (S64X4 a, int b) => new S64X4(a.X >> b, a.Y >> b, a.Z >> b, a.W >> b);

	public readonly override string ToString() {
		return $"{{{X}, {Y}, {Z}, {W}}}";
	}
}
