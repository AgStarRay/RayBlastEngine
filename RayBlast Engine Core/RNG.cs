using System.Numerics;
using System.Runtime.CompilerServices;
using Cysharp.Text;

namespace RayBlast; 

public class RNG {
	public static int TimeSeed => (int)DateTime.UtcNow.Ticks;

	private const ulong SAMPLE_MULTIPLIER = 0x7facc0f7a00541bd;
	private const ulong SAMPLE_ADDITIVE = 0xb5ad4eceda1ce2a9;

	public ulong state;

	public RNG() {
		state = (ulong)DateTime.UtcNow.Ticks;
	}

	public RNG(int seed) {
		state = (ulong)seed;
	}

	public RNG(ulong seed) {
		state = seed;
	}

	public bool Bool {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get => (Mutate() & 1) != 0;
	}

	public byte Byte {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get => (byte)Exclusive(256);
	}
	public int Integer {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get => (int)Mutate();
	}

	public int Sign {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get => Bool ? 1 : -1;
	}

	public float Float {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get => Mutate() * 5.42101086242752E-20f;
	}

	public double Double {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get => Mutate() * 5.42101086242752E-20;
	}

	public Vector2 Vector2 {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get => new(((long)Mutate() * 1.0842021724855E-19f), ((long)Mutate() * 1.0842021724855E-19f));
	}

	public Vector2 UnsignedVector2 {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get => new(Mutate() * 5.42101086242752E-20f, Mutate() * 5.42101086242752E-20f);
	}

	public Vector3 Vector3 {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get => new(((long)Mutate() * 1.0842021724855E-19f), ((long)Mutate() * 1.0842021724855E-19f), ((long)Mutate() * 1.0842021724855E-19f));
	}

	public Vector3 UnsignedVector3 {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get => new(Mutate() * 5.42101086242752E-20f, Mutate() * 5.42101086242752E-20f, Mutate() * 5.42101086242752E-20f);
	}

	public Vector4 Vector4 {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get =>
			new(((long)Mutate() * 1.0842021724855E-19f), ((long)Mutate() * 1.0842021724855E-19f), ((long)Mutate() * 1.0842021724855E-19f),
				((long)Mutate() * 1.0842021724855E-19f));
	}

	public Vector4 UnsignedVector4 {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get =>
			new(Mutate() * 5.42101086242752E-20f, Mutate() * 5.42101086242752E-20f, Mutate() * 5.42101086242752E-20f,
				Mutate() * 5.42101086242752E-20f);
	}

	public ColorF Color {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get => new(Float, Float, Float);
	}

	public ColorF ColorA {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get => new(Float, Float, Float, Float);
	}

	public Vector3 OnXZCircle {
		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		get {
			double direction = Mutate() * (5.42101086242752E-20 * Math.PI * 2.0);
			return new Vector3((float)Math.Cos(direction), 0f, (float)Math.Sin(direction));
		}
	}

	public Vector3 OnSphereSurface {
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
	public bool Chance(double ratio) {
		return Double < ratio;
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public int Inclusive(int max) {
		return (int)(Mutate() % ((ulong)max + 1));
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public int Inclusive(int min, int max) {
		return (int)(Mutate() % ((ulong)max + 1 - (ulong)min) + (ulong)min);
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public int Exclusive(int max) {
		return (int)(Mutate() % (ulong)max);
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public int Exclusive(int min, int max) {
		return (int)(Mutate() % ((ulong)max - (ulong)min) + (ulong)min);
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public float Range(float max) {
		return (float)(Double * max);
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public float Range(float min, float max) {
		return (float)(Double * (max - min) + min);
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public double Range(double max) {
		return (Double * max);
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public double Range(double min, double max) {
		return (Double * (max - min) + min);
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public string CapsString(int length) {
		using var builder = ZString.CreateStringBuilder();
		for(int i = 0; i < length; i++) {
			builder.Append((char)('A' + Exclusive(26)));
		}
		return builder.ToString();
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public TEnum ChooseEnum<TEnum>() where TEnum : Enum {
		Array values = Enum.GetValues(typeof(TEnum));
		return (TEnum)values.GetValue(Exclusive(values.Length))!;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public ulong Mutate() {
		return state = BitOperations.RotateLeft(state * SAMPLE_MULTIPLIER + SAMPLE_ADDITIVE, 16);
	}
}