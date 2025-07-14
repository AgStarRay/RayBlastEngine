using System.Numerics;

namespace RayBlast;

public record struct S32X2(int X, int Y) {
    public S32X2(int x) : this(x, x) {
    }

    public static explicit operator Vector2(S32X2 a) {
        return new Vector2(a.X, a.Y);
    }

    public static S32X2 operator +(S32X2 a, int b) {
        return new S32X2(a.X + b, a.Y + b);
    }

    public static S32X2 operator +(S32X2 a, S32X2 b) {
        return new S32X2(a.X + b.X, a.Y + b.Y);
    }

    public static S32X2 operator -(S32X2 a, int b) {
        return new S32X2(a.X - b, a.Y - b);
    }

    public static S32X2 operator -(S32X2 a, S32X2 b) {
        return new S32X2(a.X - b.X, a.Y - b.Y);
    }

    public static S32X2 operator *(S32X2 a, int b) {
        return new S32X2(a.X * b, a.Y * b);
    }

    public static S32X2 operator *(S32X2 a, S32X2 b) {
        return new S32X2(a.X * b.X, a.Y * b.Y);
    }

    public static S32X2 operator /(S32X2 a, int b) {
        return new S32X2(a.X / b, a.Y / b);
    }

    [NonMIMD]
    public static S32X2 operator /(S32X2 a, S32X2 b) {
        return new S32X2(a.X / b.X, a.Y / b.Y);
    }

    public static S32X2 operator %(S32X2 a, int b) {
        return new S32X2(a.X % b, a.Y % b);
    }

    [NonMIMD]
    public static S32X2 operator %(S32X2 a, S32X2 b) {
        return new S32X2(a.X % b.X, a.Y % b.Y);
    }

    public static S32X2 operator &(S32X2 a, int b) {
        return new S32X2(a.X & b, a.Y & b);
    }

    public static S32X2 operator &(S32X2 a, S32X2 b) {
        return new S32X2(a.X & b.X, a.Y & b.Y);
    }

    public static S32X2 operator |(S32X2 a, int b) {
        return new S32X2(a.X | b, a.Y | b);
    }

    public static S32X2 operator |(S32X2 a, S32X2 b) {
        return new S32X2(a.X | b.X, a.Y | b.Y);
    }

    public static S32X2 operator ^(S32X2 a, int b) {
        return new S32X2(a.X ^ b, a.Y ^ b);
    }

    public static S32X2 operator ^(S32X2 a, S32X2 b) {
        return new S32X2(a.X ^ b.X, a.Y ^ b.Y);
    }

    public static S32X2 operator <<(S32X2 a, int b) {
        return new S32X2(a.X << b, a.Y << b);
    }

    public static S32X2 operator >> (S32X2 a, int b) {
        return new S32X2(a.X >> b, a.Y >> b);
    }

    public readonly override string ToString() {
        return $"{{{X}, {Y}}}";
    }
}
