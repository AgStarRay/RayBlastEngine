using System.Numerics;
using System.Runtime.CompilerServices;

namespace RayBlast;

public struct ColorF {
    public static readonly ColorF CLEAR = new();
    public static readonly ColorF BLACK = new(0f, 0f, 0f, 1f);
    public static readonly ColorF WHITE = new(1f, 1f, 1f, 1f);

    public float r;
    public float g;
    public float b;
    public float a;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public ColorF(float r, float g,
                  float b) {
        this.r = r;
        this.g = g;
        this.b = b;
        a = 1f;
    }

    // ReSharper disable once TooManyDependencies
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public ColorF(float r, float g,
                  float b, float a) {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static explicit operator ColorF(Vector3 a) {
        return new ColorF(a.X, a.Y, a.Z, 1f);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static explicit operator Vector3(ColorF a) {
        return new Vector3(a.r, a.g, a.b);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static explicit operator ColorF(Vector4 a) {
        return new ColorF(a.X, a.Y, a.Z, a.W);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static explicit operator Vector4(ColorF a) {
        return new Vector4(a.r, a.g, a.b, a.a);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static explicit operator Color32(ColorF a) {
        ColorF scaled = Clamp01(a) * byte.MaxValue;
        return new Color32((byte)scaled.r, (byte)scaled.g, (byte)scaled.b, (byte)scaled.a);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static explicit operator ColorF(Color32 a) {
        var scaled = new ColorF(a.r, a.g, a.b, a.a);
        return scaled * (1f / byte.MaxValue);
    }

    public static ColorF operator +(ColorF a, ColorF b) {
        return new ColorF(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);
    }

    public static ColorF operator *(ColorF a, float b) {
        return new ColorF(a.r * b, a.g * b, a.b * b, a.a * b);
    }

    public static ColorF operator *(ColorF a, ColorF b) {
        return new ColorF(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
    }

    public static bool operator ==(ColorF a, ColorF b) {
        return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    }

    public static bool operator !=(ColorF a, ColorF b) {
        return a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a;
    }

    public override bool Equals(object? obj) {
        return obj is ColorF other && Equals(other);
    }

    public readonly bool Equals(ColorF other) {
        return r == other.r && g == other.g && b == other.b && a == other.a;
    }

    public override int GetHashCode() {
        return HashCode.Combine(r, g, b, a);
    }

    public readonly ColorF WithAlpha(float newAlpha) {
        return new ColorF(r, g, b, newAlpha);
    }

    public readonly ColorF MultiplyBrightnessBy(float brightnessMultiplier) {
        return new ColorF(r * brightnessMultiplier, g * brightnessMultiplier, b * brightnessMultiplier, a);
    }

    public readonly ColorF MultiplySaturationBy(float saturationMultiplier) {
        float gray = Math.Max(Math.Max(r, g), b) * saturationMultiplier;
        return new ColorF(r * (1f - saturationMultiplier) + gray,
                          g * (1f - saturationMultiplier) + gray,
                          b * (1f - saturationMultiplier) + gray,
                          a);
    }

    public static ColorF Lerp(ColorF a, ColorF b,
                              float lerp) {
        lerp = Mathd.Clamp01(lerp);
        return a * (1f - lerp) + b * lerp;
    }

    public static float InverseLerp(ColorF a, ColorF b,
                                    ColorF value) {
        return Mathd.InverseLerp((Vector4)a, (Vector4)b, (Vector4)value);
    }

    public static float InverseLerpIgnoreAlpha(ColorF a, ColorF b,
                                               ColorF value) {
        return Mathd.InverseLerp((Vector3)a, (Vector3)b, (Vector3)value);
    }

    public readonly ColorF AddWhite(float whiteLerp) {
        return Lerp(this, WHITE, whiteLerp);
    }

    public readonly string ToHexString() {
        return (((int)Math.Round(Mathd.Clamp01(r) * byte.MaxValue) << 24)
              + ((int)Math.Round(Mathd.Clamp01(g) * byte.MaxValue) << 16)
              + ((int)Math.Round(Mathd.Clamp01(b) * byte.MaxValue) << 8)
              + (int)Math.Round(Mathd.Clamp01(a) * byte.MaxValue))
           .InvariantString("X8");
    }

    public readonly string ToHexShortString() {
        return (((int)Math.Round(Mathd.Clamp01(r) * 15) << 12)
              + ((int)Math.Round(Mathd.Clamp01(g) * 15) << 8)
              + ((int)Math.Round(Mathd.Clamp01(b) * 15) << 4)
              + (int)Math.Round(Mathd.Clamp01(a) * 15))
           .InvariantString("X4");
    }

    public readonly ColorF WithRGB(ColorF newColor) {
        return new ColorF(newColor.r, newColor.g, newColor.b, a);
    }

    public readonly ColorF WithRGB(float red,
                                   float green, float blue) {
        return new ColorF(red, green, blue, a);
    }

    public readonly ColorF WithRed(float red) {
        return new ColorF(red, g, b, a);
    }

    public readonly ColorF WithGreen(float green) {
        return new ColorF(r, green, b, a);
    }

    public readonly ColorF WithBlue(float blue) {
        return new ColorF(r, g, blue, a);
    }

    public static ColorF Clamp01(ColorF color) {
        return new ColorF(Math.Clamp(color.r, 0f, 1f), Math.Clamp(color.g, 0f, 1f), Math.Clamp(color.b, 0f, 1f), Math.Clamp(color.a, 0f, 1f));
    }
}
