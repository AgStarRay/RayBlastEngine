using System;
using System.ComponentModel;
using System.Numerics;
using AVXPerlinNoise;

namespace RayBlast;

//TODO_AFTER: Obsolete more incorrect usages
public static class Mathd {
    public const double Infinity = double.PositiveInfinity;
    public const double NegativeInfinity = double.NegativeInfinity;
    public const double Deg2Rad = Math.PI / 180.0;
    public const double Rad2Deg = 180.0 / Math.PI;
    public const double Epsilon = double.Epsilon;

    private static readonly Perlin PERLIN = new();

    public static double Min(double a, double b,
                             double c) {
        if(a < c) {
            if(a < b)
                return a;
            return b;
        }
        if(b < c)
            return b;
        return c;
    }

    public static double Min(Span<double> values) {
        int length = values.Length;
        if(length == 0)
            return 0.0;
        double num = values[0];
        for(var index = 1; index < length; ++index) {
            if(values[index] < num)
                num = values[index];
        }
        return num;
    }

    public static long Min(long a, long b,
                           long c) {
        if(a < c) {
            if(a < b)
                return a;
            return b;
        }
        if(b < c)
            return b;
        return c;
    }

    public static long Min(Span<long> values) {
        int length = values.Length;
        if(length == 0)
            return 0;
        long num = values[0];
        for(var index = 1; index < length; ++index) {
            if(values[index] < num)
                num = values[index];
        }
        return num;
    }

    public static byte Min(byte a, byte b,
                           byte c) {
        if(a < c) {
            if(a < b)
                return a;
            return b;
        }
        if(b < c)
            return b;
        return c;
    }

    public static byte Min(Span<byte> values) {
        int length = values.Length;
        if(length == 0)
            return 0;
        byte num = values[0];
        for(var index = 1; index < length; ++index) {
            if(values[index] < num)
                num = values[index];
        }
        return num;
    }

    public static double Max(Span<double> values) {
        int length = values.Length;
        if(length == 0)
            return 0d;
        double num = values[0];
        for(var index = 1; index < length; ++index) {
            if(values[index] > num)
                num = values[index];
        }
        return num;
    }

    public static double Max(double a, double b,
                             double c) {
        if(a > c) {
            if(a > b)
                return a;
            return b;
        }
        if(b > c)
            return b;
        return c;
    }

    public static long Max(Span<long> values) {
        int length = values.Length;
        if(length == 0)
            return 0;
        long num = values[0];
        for(var index = 1; index < length; ++index) {
            if(values[index] > num)
                num = values[index];
        }
        return num;
    }

    public static double Max(long a, long b,
                             long c) {
        if(a > c) {
            if(a > b)
                return a;
            return b;
        }
        if(b > c)
            return b;
        return c;
    }

    public static byte Max(Span<byte> values) {
        int length = values.Length;
        if(length == 0)
            return 0;
        byte num = values[0];
        for(var index = 1; index < length; ++index) {
            if(values[index] > num)
                num = values[index];
        }
        return num;
    }

    public static double Max(byte a, byte b,
                             byte c) {
        if(a > c) {
            if(a > b)
                return a;
            return b;
        }
        if(b > c)
            return b;
        return c;
    }

    //TODO_AFTER: Delete
    public static int CeilToInt(double d) {
        return (int)Math.Ceiling(d);
    }

    //TODO_AFTER: Delete
    public static int FloorToInt(double d) {
        return (int)Math.Floor(d);
    }

    //TODO_AFTER: Delete
    public static int RoundToInt(double d) {
        return (int)Math.Round(d);
    }

    //TODO_AFTER: Delete
    public static long RoundToLong(double d) {
        return (long)Math.Round(d);
    }

    public static double Clamp(double value, double min,
                               double max) {
        if(value < min)
            return min;
        if(value > max)
            return max;
        return value;
    }

    public static int Clamp(int value, int min,
                            int max) {
        if(value < min)
            return min;
        if(value > max)
            return max;
        return value;
    }

    public static uint Clamp(uint value, uint min,
                             uint max) {
        if(value < min)
            return min;
        if(value > max)
            return max;
        return value;
    }

    public static long Clamp(long value, long min,
                             long max) {
        if(value < min)
            return min;
        if(value > max)
            return max;
        return value;
    }

    public static ulong Clamp(ulong value, ulong min,
                              ulong max) {
        if(value < min)
            return min;
        if(value > max)
            return max;
        return value;
    }

    public static byte Clamp(byte value, byte min,
                             byte max) {
        if(value < min)
            return min;
        if(value > max)
            return max;
        return value;
    }

    public static sbyte Clamp(sbyte value, sbyte min,
                              sbyte max) {
        if(value < min)
            return min;
        if(value > max)
            return max;
        return value;
    }

    public static short Clamp(short value, short min,
                              short max) {
        if(value < min)
            return min;
        if(value > max)
            return max;
        return value;
    }

    public static ushort Clamp(ushort value, ushort min,
                               ushort max) {
        if(value < min)
            return min;
        if(value > max)
            return max;
        return value;
    }

    public static float Clamp01(float value) {
        return value switch {
            < 0f => 0f,
            > 1f => 1f,
            _ => value
        };
    }

    public static double Clamp01(double value) {
        return value switch {
            < 0.0 => 0.0,
            > 1.0 => 1.0,
            _ => value
        };
    }

    public static double Median(double a, double b,
                                double c) {
        double x = a - b;
        if(x * (b - c) > 0)
            return b;
        if(x * (a - c) > 0)
            return c;
        return a;
    }

    public static int Median(int a, int b,
                             int c) {
        int x = a - b;
        if(x * (b - c) > 0)
            return b;
        if(x * (a - c) > 0)
            return c;
        return a;
    }

    public static uint Median(uint a, uint b,
                              uint c) {
        var x = (int)(a - b);
        if(x * ((long)b - c) > 0)
            return b;
        if(x * ((long)a - c) > 0)
            return c;
        return a;
    }

    public static long Median(long a, long b,
                              long c) {
        long x = a - b;
        if(x * (b - c) > 0)
            return b;
        if(x * (a - c) > 0)
            return c;
        return a;
    }

    public static ulong Median(ulong a, ulong b,
                               ulong c) {
        var x = (long)(a - b);
        if(x * (long)(b - c) > 0)
            return b;
        if(x * (long)(a - c) > 0)
            return c;
        return a;
    }

    public static byte Median(byte a, byte b,
                              byte c) {
        int x = a - b;
        if(x * (b - c) > 0)
            return b;
        if(x * (a - c) > 0)
            return c;
        return a;
    }

    public static sbyte Median(sbyte a, sbyte b,
                               sbyte c) {
        int x = a - b;
        if(x * (b - c) > 0)
            return b;
        if(x * (a - c) > 0)
            return c;
        return a;
    }

    public static short Median(short a, short b,
                               short c) {
        int x = a - b;
        if(x * (b - c) > 0)
            return b;
        if(x * (a - c) > 0)
            return c;
        return a;
    }

    public static ushort Median(ushort a, ushort b,
                                ushort c) {
        int x = a - b;
        if(x * (b - c) > 0)
            return b;
        if(x * (a - c) > 0)
            return c;
        return a;
    }

    public static float Lerp(float from, float to,
                             float t) {
        return from + (to - from) * Clamp01(t);
    }

    public static double Lerp(double from, double to,
                              double t) {
        return from + (to - from) * Clamp01(t);
    }

    public static double LerpAngle(double a, double b,
                                   double t) {
        double num = Repeat(b - a, 360d);
        if(num > 180.0d)
            num -= 360d;
        return a + num * Clamp01(t);
    }

    public static double MoveTowards(double current, double target,
                                     double maxDelta) {
        if(Math.Abs(target - current) <= maxDelta)
            return target;
        return current + Math.Sign(target - current) * maxDelta;
    }

    public static double MoveTowardsAngle(double current, double target,
                                          double maxDelta) {
        target = current + DeltaAngle(current, target);
        return MoveTowards(current, target, maxDelta);
    }

    public static double SmoothStep(double from, double to,
                                    double t) {
        t = Clamp01(t);
        t = -2.0 * t * t * t + 3.0 * t * t;
        return to * t + from * (1.0 - t);
    }

    public static double Gamma(double value, double absmax,
                               double gamma) {
        bool flag = value < 0.0;
        double num1 = Math.Abs(value);
        if(num1 > absmax) {
            if(flag)
                return -num1;
            return num1;
        }
        double num2 = Math.Pow(num1 / absmax, gamma) * absmax;
        if(flag)
            return -num2;
        return num2;
    }

    public static bool Approximately(double a, double b) {
        return Math.Abs(b - a) < Math.Max(1E-06d * Math.Max(Math.Abs(a), Math.Abs(b)), 1.121039E-44d);
    }

    public static double SmoothDamp(double current, double target,
                                    ref double currentVelocity, double smoothTime,
                                    double maxSpeed) {
        double deltaTime = Time.deltaTime;
        return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    public static double SmoothDamp(double current, double target,
                                    ref double currentVelocity, double smoothTime) {
        double deltaTime = Time.deltaTime;
        double maxSpeed = double.PositiveInfinity;
        return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    public static double SmoothDamp(double current, double target,
                                    ref double currentVelocity, double smoothTime,
                                    double maxSpeed, double deltaTime) {
        smoothTime = Math.Max(0.0001d, smoothTime);
        double num1 = 2d / smoothTime;
        double num2 = num1 * deltaTime;
        double num3 = 1.0d / (1.0d + num2 + 0.479999989271164d * num2 * num2 + 0.234999999403954d * num2 * num2 * num2);
        double num4 = current - target;
        double num5 = target;
        double max = maxSpeed * smoothTime;
        double num6 = Clamp(num4, -max, max);
        target = current - num6;
        double num7 = (currentVelocity + num1 * num6) * deltaTime;
        currentVelocity = (currentVelocity - num1 * num7) * num3;
        double num8 = target + (num6 + num7) * num3;
        if(num5 - current > 0.0 == num8 > num5) {
            num8 = num5;
            currentVelocity = (num8 - num5) / deltaTime;
        }
        return num8;
    }

    public static double SmoothDampAngle(double current, double target,
                                         ref double currentVelocity, double smoothTime,
                                         double maxSpeed) {
        double deltaTime = Time.deltaTime;
        return SmoothDampAngle(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    public static double SmoothDampAngle(double current, double target,
                                         ref double currentVelocity, double smoothTime) {
        double deltaTime = Time.deltaTime;
        double maxSpeed = double.PositiveInfinity;
        return SmoothDampAngle(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    public static double SmoothDampAngle(double current, double target,
                                         ref double currentVelocity, double smoothTime,
                                         double maxSpeed, double deltaTime) {
        target = current + DeltaAngle(current, target);
        return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    public static Vector3 SmoothDamp(Vector3 current,
                                     Vector3 target,
                                     ref Vector3 currentVelocity,
                                     float smoothTime,
                                     float maxSpeed) {
        double deltaTime = Time.deltaTime;
        return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    public static Vector3 SmoothDamp(Vector3 current,
                                     Vector3 target,
                                     ref Vector3 currentVelocity,
                                     float smoothTime) {
        double deltaTime = Time.deltaTime;
        float maxSpeed = float.PositiveInfinity;
        return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    public static Vector3 SmoothDamp(Vector3 current,
                                     Vector3 target,
                                     ref Vector3 currentVelocity,
                                     float smoothTime,
                                     float maxSpeed,
                                     double deltaTime) {
        smoothTime = Math.Max(0.0001f, smoothTime);
        float num1 = 2f / smoothTime;
        double num2 = num1 * deltaTime;
        var num3 = (float)(1.0 / (1.0 + num2 + 0.47999998927116394 * num2 * num2 + 0.23499999940395355 * num2 * num2 * num2));
        float num4 = current.X - target.X;
        float num5 = current.Y - target.Y;
        float num6 = current.Z - target.Z;
        Vector3 vector3 = target;
        float num7 = maxSpeed * smoothTime;
        float num8 = num7 * num7;
        float d = num4 * num4 + num5 * num5 + num6 * num6;
        if(d > num8) {
            var num9 = (float)Math.Sqrt(d);
            num4 = num4 / num9 * num7;
            num5 = num5 / num9 * num7;
            num6 = num6 / num9 * num7;
        }
        target.X = current.X - num4;
        target.Y = current.Y - num5;
        target.Z = current.Z - num6;
        double num10 = (currentVelocity.X + num1 * num4) * deltaTime;
        double num11 = (currentVelocity.Y + num1 * num5) * deltaTime;
        double num12 = (currentVelocity.Z + num1 * num6) * deltaTime;
        currentVelocity.X = (float)((currentVelocity.X - num1 * num10) * num3);
        currentVelocity.Y = (float)((currentVelocity.Y - num1 * num11) * num3);
        currentVelocity.Z = (float)((currentVelocity.Z - num1 * num12) * num3);
        double x = target.X + (num4 + num10) * num3;
        double y = target.Y + (num5 + num11) * num3;
        double z = target.Z + (num6 + num12) * num3;
        float num13 = vector3.X - current.X;
        float num14 = vector3.Y - current.Y;
        float num15 = vector3.Z - current.Z;
        double num16 = x - vector3.X;
        double num17 = y - vector3.Y;
        double num18 = z - vector3.Z;
        if(num13 * num16 + num14 * num17 + num15 * num18 > 0.0) {
            x = vector3.X;
            y = vector3.Y;
            z = vector3.Z;
            currentVelocity.X = (float)((x - vector3.X) / deltaTime);
            currentVelocity.Y = (float)((y - vector3.Y) / deltaTime);
            currentVelocity.Z = (float)((z - vector3.Z) / deltaTime);
        }
        return new Vector3((float)x, (float)y, (float)z);
    }

    public static double Repeat(double t, double length) {
        return t - Math.Floor(t / length) * length;
    }

    public static double PingPong(double t, double length) {
        t = Repeat(t, length * 2d);
        return length - Math.Abs(t - length);
    }

    public static float InverseLerp(float a, float b,
                                    float value) {
        return a != b ? Clamp01((value - a) / (b - a)) : 0f;
    }

    public static double InverseLerp(double a, double b,
                                     double value) {
        return a != b ? Clamp01((value - a) / (b - a)) : 0.0;
    }

    public static double DeltaAngle(double current, double target) {
        double num = Repeat(target - current, 360.0);
        if(num > 180.0)
            num -= 360.0;
        return num;
    }

    public static float InverseLerp(Vector4 a, Vector4 b,
                                    Vector4 value) {
        Vector4 ab = b - a;
        Vector4 av = value - a;
        return Vector4.Dot(av, ab) / Vector4.Dot(ab, ab);
    }

    public static float InverseLerp(Vector3 a, Vector3 b,
                                    Vector3 value) {
        Vector3 ab = b - a;
        Vector3 av = value - a;
        return Vector3.Dot(av, ab) / Vector3.Dot(ab, ab);
    }

    // internal static bool LineIntersection(Vector2d p1, Vector2d p2,
    // 									  Vector2d p3, Vector2d p4,
    // 									  ref Vector2d result) {
    // 	double num1 = p2.x - p1.x;
    // 	double num2 = p2.y - p1.y;
    // 	double num3 = p4.x - p3.x;
    // 	double num4 = p4.y - p3.y;
    // 	double num5 = num1 * num4 - num2 * num3;
    // 	if(num5 == 0.0d)
    // 		return false;
    // 	double num6 = p3.x - p1.x;
    // 	double num7 = p3.y - p1.y;
    // 	double num8 = (num6 * num4 - num7 * num3) / num5;
    // 	result = new Vector2d(p1.x + num8 * num1, p1.y + num8 * num2);
    // 	return true;
    // }
    //
    // internal static bool LineSegmentIntersection(Vector2d p1, Vector2d p2,
    // 											 Vector2d p3, Vector2d p4,
    // 											 ref Vector2d result) {
    // 	double num1 = p2.x - p1.x;
    // 	double num2 = p2.y - p1.y;
    // 	double num3 = p4.x - p3.x;
    // 	double num4 = p4.y - p3.y;
    // 	double num5 = (num1 * num4 - num2 * num3);
    // 	if(num5 == 0.0)
    // 		return false;
    // 	double num6 = p3.x - p1.x;
    // 	double num7 = p3.y - p1.y;
    // 	double num8 = (num6 * num4 - num7 * num3) / num5;
    // 	if(num8 is < 0.0 or > 1.0)
    // 		return false;
    // 	double num9 = (num6 * num2 - num7 * num1) / num5;
    // 	if(num9 is < 0.0 or > 1.0)
    // 		return false;
    // 	result = new Vector2d(p1.x + num8 * num1, p1.y + num8 * num2);
    // 	return true;
    // }

    public static double PerlinNoise(double x, double y) {
        //TODO_URGENT: Check if works correctly
        return PERLIN.perlin(x, y, 0.0);
    }

    public static double PointInsideEllipsoid(Vector3 point, Vector3 ellipsoidCenter,
                                              Vector3 ellipsoidRadii, Quaternion ellipsoidRotation,
                                              out Vector3 closestEdgePoint) {
        Quaternion inverseRotation = Quaternion.Inverse(ellipsoidRotation);
        Vector3 localPoint = Vector3.Transform(point - ellipsoidCenter, inverseRotation);
        Vector3 localPointScaled = localPoint / ellipsoidRadii;
        Vector3 closestPointLocal = localPointScaled.Normalized();
        closestPointLocal *= ellipsoidRadii;
        closestEdgePoint = Vector3.Transform(closestPointLocal, ellipsoidRotation) + ellipsoidCenter;
        return localPointScaled.LengthSquared();
    }
}
