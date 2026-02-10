using System.Numerics;
using System.Runtime.CompilerServices;

namespace RayBlast;

public struct ColorH {
    public static readonly ColorH CLEAR = new();
    public static readonly ColorH BLACK = new((Half)0f, (Half)0f, (Half)0f, (Half)1f);
    public static readonly ColorH WHITE = new((Half)1f, (Half)1f, (Half)1f, (Half)1f);

    public Half r;
    public Half g;
    public Half b;
    public Half a;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public ColorH(Half r, Half g,
                  Half b) {
        this.r = r;
        this.g = g;
        this.b = b;
        a = (Half)1f;
    }

    // ReSharper disable once TooManyDependencies
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public ColorH(Half r, Half g,
                  Half b, Half a) {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public ColorH(float r, float g,
                  float b) {
        this.r = (Half)r;
        this.g = (Half)g;
        this.b = (Half)b;
        a = (Half)1f;
    }

    // ReSharper disable once TooManyDependencies
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public ColorH(float r, float g,
                  float b, float a) {
        this.r = (Half)r;
        this.g = (Half)g;
        this.b = (Half)b;
        this.a = (Half)a;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static explicit operator ColorH(Vector3 a) {
        return new ColorH((Half)a.X, (Half)a.Y, (Half)a.Z, (Half)1f);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static explicit operator Vector3(ColorH a) {
        return new Vector3((float)a.r, (float)a.g, (float)a.b);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static explicit operator ColorH(Vector4 a) {
        return new ColorH((Half)a.X, (Half)a.Y, (Half)a.Z, (Half)a.W);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static explicit operator Vector4(ColorH a) {
        return new Vector4((float)a.r, (float)a.g, (float)a.b, (float)a.a);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static explicit operator Color32(ColorH a) {
        ColorH scaled = Clamp01(a) * byte.MaxValue;
        return new Color32((byte)scaled.r, (byte)scaled.g, (byte)scaled.b, (byte)scaled.a);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static explicit operator ColorH(Color32 a) {
        var scaled = new ColorH(a.r, a.g, a.b, (Half)a.a);
        return scaled * (1f / byte.MaxValue);
    }

    public static ColorH operator +(ColorH a, ColorH b) {
        return new ColorH(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);
    }

    public static ColorH operator *(ColorH a, float b) {
        return new ColorH((float)a.r * b, (float)a.g * b, (float)a.b * b, (float)a.a * b);
    }

    public static ColorH operator *(ColorH a, ColorH b) {
        return new ColorH(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
    }

    public static bool operator ==(ColorH a, ColorH b) {
        return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    }

    public static bool operator !=(ColorH a, ColorH b) {
        return a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a;
    }

    public override bool Equals(object? obj) {
        return obj is ColorH other && Equals(other);
    }

    public readonly bool Equals(ColorH other) {
        return r == other.r && g == other.g && b == other.b && a == other.a;
    }

    public override int GetHashCode() {
        return HashCode.Combine(r, g, b, a);
    }

    public readonly ColorH WithAlpha(Half newAlpha) {
        return new ColorH(r, g, b, newAlpha);
    }

    public readonly ColorH MultiplyBrightnessBy(float brightnessMultiplier) {
        return new ColorH((Half)((float)r * brightnessMultiplier), (Half)((float)g * brightnessMultiplier), (Half)((float)b * brightnessMultiplier),
                          a);
    }

    public readonly ColorH MultiplySaturationBy(float saturationMultiplier) {
        float gray = Math.Max(Math.Max((float)r, (float)g), (float)b) * saturationMultiplier;
        return new ColorH((Half)((float)r * (1f - saturationMultiplier) + gray),
                          (Half)((float)g * (1f - saturationMultiplier) + gray),
                          (Half)((float)b * (1f - saturationMultiplier) + gray),
                          a);
    }

    public static ColorH Lerp(ColorH a, ColorH b,
                              float lerp) {
        lerp = Mathd.Clamp01(lerp);
        return a * (1f - lerp) + b * lerp;
    }

    public static float InverseLerp(ColorH a, ColorH b,
                                    ColorH value) {
        return Mathd.InverseLerp((Vector4)a, (Vector4)b, (Vector4)value);
    }

    public static float InverseLerpIgnoreAlpha(ColorH a, ColorH b,
                                               ColorH value) {
        return Mathd.InverseLerp((Vector3)a, (Vector3)b, (Vector3)value);
    }

    public readonly ColorH AddWhite(float whiteLerp) {
        return Lerp(this, WHITE, whiteLerp);
    }

    public readonly string ToHexString() {
        return (((int)Math.Round(Mathd.Clamp01((float)r) * byte.MaxValue) << 24)
              + ((int)Math.Round(Mathd.Clamp01((float)g) * byte.MaxValue) << 16)
              + ((int)Math.Round(Mathd.Clamp01((float)b) * byte.MaxValue) << 8)
              + (int)Math.Round(Mathd.Clamp01((float)a) * byte.MaxValue))
           .InvariantString("X8");
    }

    public readonly string ToHexShortString() {
        return (((int)Math.Round(Mathd.Clamp01((float)r) * 15) << 12)
              + ((int)Math.Round(Mathd.Clamp01((float)g) * 15) << 8)
              + ((int)Math.Round(Mathd.Clamp01((float)b) * 15) << 4)
              + (int)Math.Round(Mathd.Clamp01((float)a) * 15))
           .InvariantString("X4");
    }

    public readonly ColorH WithRGB(ColorH newColor) {
        return new ColorH(newColor.r, newColor.g, newColor.b, a);
    }

    public readonly ColorH WithRGB(Half red,
                                   Half green, Half blue) {
        return new ColorH(red, green, blue, a);
    }

    public readonly ColorH WithRed(Half red) {
        return new ColorH(red, g, b, a);
    }

    public readonly ColorH WithGreen(Half green) {
        return new ColorH(r, green, b, a);
    }

    public readonly ColorH WithBlue(Half blue) {
        return new ColorH(r, g, blue, a);
    }

    public static ColorH Clamp01(ColorH color) {
        return new ColorH(Math.Clamp((float)color.r, 0f, 1f), Math.Clamp((float)color.g, 0f, 1f), Math.Clamp((float)color.b, 0f, 1f),
                          Math.Clamp((float)color.a, 0f, 1f));
    }
}
