using System.Numerics;
using System.Runtime.CompilerServices;
using Cysharp.Text;

namespace RayBlast;

public static class FastRNG {
    private static ulong state;

    static FastRNG() {
        ulong p = (ulong)DateTime.UtcNow.Ticks + 0x9e3779b97f4a7c15;
        ulong z = p;
        z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;
        z = (z ^ (z >> 27)) * 0x94d049bb133111eb;
        state = z ^ (z >> 31);
    }

    public static bool Bool {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get => (Mutate() & 1) != 0;
    }

    public static byte Byte {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get => (byte)Exclusive(256);
    }
    public static int Integer {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get => (int)Mutate();
    }

    public static int Sign {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get => Bool ? 1 : -1;
    }

    public static float Float {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get => Mutate() * 5.42101086242752E-20f;
    }

    public static double Double {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get => Mutate() * 5.42101086242752E-20;
    }

    public static Vector2 Vector2 {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get {
            ulong result = Mutate();
            return new Vector2((int)result * 4.65661287307739E-10f, result * 1.0842021724855E-19f);
        }
    }

    public static Vector2 UnsignedVector2 {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get {
            ulong result = Mutate();
            return new Vector2((uint)result * 2.3283064365387E-10f, result * 5.42101086242752E-20f);
        }
    }

    public static Vector3 Vector3 {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get {
            ulong resultA = Mutate();
            ulong resultB = Mutate();
            return new Vector3((int)resultA * 4.65661287307739E-10f, resultA * 1.0842021724855E-19f, resultB * 1.0842021724855E-19f);
        }
    }

    public static Vector3 UnsignedVector3 {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get {
            ulong resultA = Mutate();
            ulong resultB = Mutate();
            return new Vector3((uint)resultA * 2.3283064365387E-10f, resultA * 5.42101086242752E-20f, resultB * 5.42101086242752E-20f);
        }
    }

    public static Vector4 Vector4 {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get {
            ulong resultA = Mutate();
            ulong resultB = Mutate();
            return new Vector4((int)resultA * 4.65661287307739E-10f, resultA * 1.0842021724855E-19f,
                               (int)resultB * 4.65661287307739E-10f, resultB * 1.0842021724855E-19f);
        }
    }

    public static Vector4 UnsignedVector4 {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get {
            ulong resultA = Mutate();
            ulong resultB = Mutate();
            return new Vector4((uint)resultA * 2.3283064365387E-10f, resultA * 5.42101086242752E-20f,
                               (uint)resultB * 2.3283064365387E-10f, resultB * 5.42101086242752E-20f);
        }
    }

    public static ColorF Color {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get {
            ulong resultA = Mutate();
            ulong resultB = Mutate();
            return new ColorF((uint)resultA * 2.3283064365387E-10f, resultA * 5.42101086242752E-20f, resultB * 5.42101086242752E-20f);
        }
    }

    public static ColorF ColorA {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get {
            ulong resultA = Mutate();
            ulong resultB = Mutate();
            return new ColorF((uint)resultA * 2.3283064365387E-10f, resultA * 5.42101086242752E-20f,
                              (uint)resultB * 2.3283064365387E-10f, resultB * 5.42101086242752E-20f);
        }
    }

    public static Vector3 OnXZCircle {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        get {
            double direction = Mutate() * (5.42101086242752E-20 * Math.PI * 2.0);
            return new Vector3((float)Math.Cos(direction), 0f, (float)Math.Sin(direction));
        }
    }

    public static Vector3 OnSphereSurface {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        get {
            double longitude = Mutate() * (5.42101086242752E-20 * Math.PI * 2.0);
            double latitude = Mutate() * (5.42101086242752E-20 * Math.PI * 2.0);
            //TODO_AFTER: Check if RNG is distributed proper
            return new Vector3((float)(Math.Cos(longitude) * Math.Cos(latitude)),
                               (float)Math.Sin(latitude),
                               (float)(Math.Sin(longitude) * Math.Cos(latitude)));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static bool Chance(double ratio) {
        return Double < ratio;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int Inclusive(int max) {
        return (int)(Mutate() % ((ulong)max + 1));
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int Inclusive(int min, int max) {
        return (int)(Mutate() % ((ulong)max + 1 - (ulong)min) + (ulong)min);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int Exclusive(int max) {
        return (int)(Mutate() % (ulong)max);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int Exclusive(int min, int max) {
        return (int)(Mutate() % ((ulong)max - (ulong)min) + (ulong)min);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static float Range(float max) {
        return (float)(Double * max);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static float Range(float min, float max) {
        return (float)(Double * (max - min) + min);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static double Range(double max) {
        return Double * max;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static double Range(double min, double max) {
        return Double * (max - min) + min;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static string CapsString(int length) {
        using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();
        for(var i = 0; i < length; i++) {
            builder.Append((char)('A' + Exclusive(26)));
        }
        return builder.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static TEnum ChooseEnum<TEnum>() where TEnum : Enum {
        Array values = Enum.GetValues(typeof(TEnum));
        return (TEnum)values.GetValue(Exclusive(values.Length))!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static ulong Mutate() {
        return state = BitOperations.RotateLeft(state + 0xb5ad4eceda1ce2a9, 33);
    }
}
