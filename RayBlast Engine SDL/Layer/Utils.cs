using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Cysharp.Text;

namespace RayBlast;

public static partial class Utils {
    private static readonly string[] SECOND_FORMATS = {
        "0", "0.0", "0.00", "0.000", "0.0000", "0.00000", "0.000000", "0.0000000", "0.00000000", "0.000000000",
        "0.0000000000", "0.00000000000", "0.000000000000", "0.0000000000000", "0.00000000000000", "0.000000000000000"
    };
    private static readonly string[] SECOND_TWO_DIGIT_FORMATS = {
        "00", "00.0", "00.00", "00.000", "00.0000", "00.00000", "00.000000", "00.0000000", "00.00000000", "00.000000000",
        "00.0000000000", "00.00000000000", "00.000000000000", "00.0000000000000", "00.00000000000000", "00.000000000000000"
    };
    private static readonly double[] SECOND_FACTORS = {
        Math.Pow(10.0, 0.0), Math.Pow(10.0, 1.0), Math.Pow(10.0, 2.0), Math.Pow(10.0, 3.0), Math.Pow(10.0, 4.0), Math.Pow(10.0, 5.0),
        Math.Pow(10.0, 6.0), Math.Pow(10.0, 7.0), Math.Pow(10.0, 8.0), Math.Pow(10.0, 9.0), Math.Pow(10.0, 10.0), Math.Pow(10.0, 11.0),
        Math.Pow(10.0, 12.0), Math.Pow(10.0, 13.0), Math.Pow(10.0, 14.0), Math.Pow(10.0, 15.0)
    };

    public static byte Build8Bits(bool bit0) {
        return (byte)(bit0 ? 1 : 0);
    }

    public static byte Build8Bits(bool bit0, bool bit1) {
        return (byte)((bit0 ? 1 : 0) | (bit1 ? 2 : 0));
    }

    public static byte Build8Bits(bool bit0, bool bit1,
                                  bool bit2) {
        return (byte)((bit0 ? 1 : 0) | (bit1 ? 2 : 0) | (bit2 ? 4 : 0));
    }

    public static byte Build8Bits(bool bit0, bool bit1,
                                  bool bit2, bool bit3) {
        return (byte)((bit0 ? 1 : 0) | (bit1 ? 2 : 0) | (bit2 ? 4 : 0) | (bit3 ? 8 : 0));
    }

    public static byte Build8Bits(bool bit0, bool bit1,
                                  bool bit2, bool bit3,
                                  bool bit4) {
        return (byte)((bit0 ? 1 : 0) | (bit1 ? 2 : 0) | (bit2 ? 4 : 0) | (bit3 ? 8 : 0) | (bit4 ? 16 : 0));
    }

    public static byte Build8Bits(bool bit0, bool bit1,
                                  bool bit2, bool bit3,
                                  bool bit4, bool bit5) {
        return (byte)((bit0 ? 1 : 0) | (bit1 ? 2 : 0) | (bit2 ? 4 : 0) | (bit3 ? 8 : 0) | (bit4 ? 16 : 0) | (bit5 ? 32 : 0));
    }

    public static byte Build8Bits(bool bit0, bool bit1,
                                  bool bit2, bool bit3,
                                  bool bit4, bool bit5,
                                  bool bit6) {
        return (byte)((bit0 ? 1 : 0) | (bit1 ? 2 : 0) | (bit2 ? 4 : 0) | (bit3 ? 8 : 0) | (bit4 ? 16 : 0) | (bit5 ? 32 : 0) | (bit6 ? 64 : 0));
    }

    public static byte Build8Bits(bool bit0, bool bit1,
                                  bool bit2, bool bit3,
                                  bool bit4, bool bit5,
                                  bool bit6, bool bit7) {
        return (byte)((bit0 ? 1 : 0) | (bit1 ? 2 : 0) | (bit2 ? 4 : 0) | (bit3 ? 8 : 0) | (bit4 ? 16 : 0) | (bit5 ? 32 : 0) | (bit6 ? 64 : 0)
                    | (bit7 ? 128 : 0));
    }

    public static byte Build8Bits(List<bool> bits) {
        if(bits.Count > 8)
            throw new OverflowException($"Cannot fit {bits.Count} states into 8 bits");
        byte compilation = 0;
        for(var i = 0; i < bits.Count; i++) {
            compilation += (byte)(bits[i] ? 1 << i : 0);
        }
        return compilation;
    }

    public static byte Build8Bits(params bool[] bits) {
        throw new OverflowException($"Cannot fit {bits.Length} states into 8 bits");
    }

    public static ushort Build16Bits(List<bool> bits) {
        if(bits.Count > 16)
            throw new OverflowException($"Cannot fit {bits.Count} states into 16 bits");
        ushort compilation = 0;
        for(var i = 0; i < bits.Count; i++) {
            compilation |= (ushort)(bits[i] ? 1 << i : 0);
        }
        return compilation;
    }

    public static ushort Build16Bits(params bool[] bits) {
        if(bits.Length > 16)
            throw new OverflowException($"Cannot fit {bits.Length} states into 16 bits");
        ushort compilation = 0;
        for(var i = 0; i < bits.Length; i++) {
            compilation |= (ushort)(bits[i] ? 1 << i : 0);
        }
        return compilation;
    }

    public static ushort Build16Bits(bool bit0, bool bit1,
                                     bool bit2, bool bit3,
                                     bool bit4, bool bit5,
                                     bool bit6, bool bit7,
                                     bool bit8) {
        return (ushort)((bit0 ? 1 : 0) | (bit1 ? 2 : 0) | (bit2 ? 4 : 0) | (bit3 ? 8 : 0) | (bit4 ? 16 : 0) | (bit5 ? 32 : 0) | (bit6 ? 64 : 0)
                      | (bit7 ? 128 : 0) | (bit8 ? 256 : 0));
    }

    /// <summary>
    /// Produces a boxed object that contains a number representation of the string in the smallest possible size.
    /// It prefers integers over floating-point.
    /// </summary>
    /// <param name="str">String representing a number</param>
    /// <param name="result">Boxed number</param>
    /// <returns>True if successfully parsed, false otherwise</returns>
    public static bool TryParseSmallestNumber(string str, out object? result) {
        if(byte.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte sByte)) {
            result = sByte;
            return true;
        }
        if(sbyte.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out sbyte sSByte)) {
            result = sSByte;
            return true;
        }
        if(short.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out short sShort)) {
            result = sShort;
            return true;
        }
        if(ushort.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort sUShort)) {
            result = sUShort;
            return true;
        }
        if(int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int sInt)) {
            result = sInt;
            return true;
        }
        if(uint.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint sUInt)) {
            result = sUInt;
            return true;
        }
        if(float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float sFloat)) {
            result = sFloat;
            return true;
        }
        if(long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out long sLong)) {
            result = sLong;
            return true;
        }
        if(ulong.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong sULong)) {
            result = sULong;
            return true;
        }
        if(double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double sDouble)) {
            result = sDouble;
            return true;
        }
        result = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion EulerDegreesToQuaternion(float x, float y,
                                                      float z) {
        return Quaternion.CreateFromYawPitchRoll((float)(y * Math.PI / 180.0), (float)(x * Math.PI / 180.0), (float)(z * Math.PI / 180.0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion EulerRadiansToQuaternion(float x, float y,
                                                      float z) {
        return Quaternion.CreateFromYawPitchRoll(y, x, z);
    }

    public static Quaternion EulerDegreesToQuaternion(Vector3 vector) {
        vector *= (float)(Math.PI / 180.0);
        return Quaternion.CreateFromYawPitchRoll(vector.Y, vector.X, vector.Z);
    }

    public static Quaternion EulerRadiansToQuaternion(Vector3 vector) {
        return Quaternion.CreateFromYawPitchRoll(vector.Y, vector.X, vector.Z);
    }

    public static int LeastCommonMultiple(int a, int b) {
        return a * b / GreatestCommonDivisor(a, b);
    }

    public static int GreatestCommonDivisor(int a, int b) {
        return b == 0 ? a : GreatestCommonDivisor(b, a % b);
    }

    public static string ParseText(string input, bool useSprites = false) {
        //TODO: Implement
        return input;
    }

    public static string TimeString(double time, bool forceHours = false,
                                    bool forceMinutes = true, int decimalDigits = 3) {
        Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();
        builder.AppendTimeString(time, forceHours, forceMinutes, decimalDigits);
        var timeString = builder.ToString();
        builder.Dispose();
        return timeString;
    }

    public static void AppendTimeString(this ref Utf16ValueStringBuilder builder, double time,
                                        bool forceHours = false,
                                        bool forceMinutes = true, int decimalDigits = 3) {
        if(double.IsInfinity(time)) {
            builder.Append("Infinity");
            return;
        }
        switch(time) {
        case double.NaN:
            builder.Append("N/A");
            return;
        case < 0.0:
            builder.Append("T-");
            time = -time;
            break;
        }
        double secondsFactor = SECOND_FACTORS[decimalDigits];
        time += 0.5 / secondsFactor;
        var hour = (int)Math.Floor(time / 3600.0);
        var minute = (int)(Math.Floor(time / 60.0) % 60);
        double second = Math.Floor(time * secondsFactor) / secondsFactor % 60.0;
        bool displayHours = forceHours || hour > 0;
        bool displayMinutes = forceMinutes || minute > 0;
        if(displayHours) {
            builder.Append(hour.CultureString());
            builder.Append(':');
            if(minute < 10)
                builder.Append('0');
            displayMinutes = true;
        }
        if(displayMinutes) {
            builder.Append(minute.CultureString());
            builder.Append(':');
        }
        builder.Append(second.CultureString((displayMinutes ? SECOND_TWO_DIGIT_FORMATS : SECOND_FORMATS)[decimalDigits]));
    }

    public static void AppendTimeString(this StringBuilder builder, double time,
                                        bool forceHours = false,
                                        bool forceMinutes = true, int decimalDigits = 3) {
        if(double.IsInfinity(time)) {
            builder.Append("Infinity");
            return;
        }
        switch(time) {
        case double.NaN:
            builder.Append("N/A");
            return;
        case < 0.0:
            builder.Append("T-");
            time = -time;
            break;
        }
        double secondsFactor = SECOND_FACTORS[decimalDigits];
        time += 0.5 / secondsFactor;
        var hour = (int)Math.Floor(time / 3600.0);
        var minute = (int)(Math.Floor(time / 60.0) % 60);
        double second = Math.Floor(time * secondsFactor) / secondsFactor % 60.0;
        bool displayHours = forceHours || hour > 0;
        bool displayMinutes = forceMinutes || minute > 0;
        if(displayHours) {
            builder.Append(hour.CultureString());
            builder.Append(':');
            if(minute < 10)
                builder.Append('0');
            displayMinutes = true;
        }
        if(displayMinutes) {
            builder.Append(minute.CultureString());
            builder.Append(':');
        }
        builder.Append(second.CultureString((displayMinutes ? SECOND_TWO_DIGIT_FORMATS : SECOND_FORMATS)[decimalDigits]));
    }

    public static T[] FullSetOf<T>() where T : Enum {
        return (T[])Enum.GetValues(typeof(T));
    }

    public static double PointDirection(Vector3 from, Vector3 to) {
        return PointDirection(new Vector2(from.X, from.Z), new Vector2(to.X, to.Z));
    }

    public static double PointDirection(Vector2 from, Vector2 to) {
        Vector2 difference = to - from;
        return PointDirection(difference);
    }

    public static double PointDirection(S32X2 difference) {
        return PointDirection((Vector2)difference);
    }

    public static double PointDirection(S32X2 from, S32X2 to) {
        return PointDirection((Vector2)from, (Vector2)to);
    }

    public static double PointDirection(Vector2 difference) {
        double angleDifference = Math.Acos(Vector2.Dot(new Vector2(1f, 0f), difference / difference.Length()));
        if(difference.Y <= 0f)
            return angleDifference;
        return -angleDifference;
    }

    public static bool RectContains(Vector4 rect, Vector2 point) {
        return point.X >= rect.X && point.X < rect.X + rect.Z && point.Y >= rect.Y && point.Y < rect.Y + rect.W;
    }
}
