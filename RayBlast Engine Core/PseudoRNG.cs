using System.Numerics;

namespace RayBlast {
	public static class PseudoRNG {
		public static readonly RNG INSTANCE = new RNG();
		
		public static bool Bool => INSTANCE.Bool;
		public static int Sign => INSTANCE.Sign;
		public static byte Byte => INSTANCE.Byte;
		public static int Integer => INSTANCE.Integer;
		public static float Float => INSTANCE.Float;
		public static double Double => INSTANCE.Double;
		public static Vector2 Vector2 => INSTANCE.Vector2;
		public static Vector2 UnsignedVector2 => INSTANCE.UnsignedVector2;
		public static Vector3 Vector3 => INSTANCE.Vector3;
		public static Vector3 UnsignedVector3 => INSTANCE.UnsignedVector3;
		public static Vector4 Vector4 => INSTANCE.Vector4;
		public static ColorF Color => INSTANCE.Color;
		public static ColorF ColorA => INSTANCE.ColorA;
		public static Vector3 OnXZCircle => INSTANCE.OnXZCircle;
		public static Vector3 OnSphereSurface => INSTANCE.OnSphereSurface;

		public static bool Chance(double ratio) {
			return INSTANCE.Chance(ratio);
		}
		
		public static int Inclusive(int max) {
			return INSTANCE.Inclusive(max);
		}
		
		public static int Inclusive(int min, int max) {
			return INSTANCE.Inclusive(min, max);
		}
		
		public static int Exclusive(int max) {
			return INSTANCE.Exclusive(max);
		}
		
		public static int Exclusive(int min, int max) {
			return INSTANCE.Exclusive(min, max);
		}
		
		public static float Range(float max) {
			return INSTANCE.Range(max);
		}
		
		public static float Range(float min, float max) {
			return INSTANCE.Range(min, max);
		}
		
		public static double Range(double max) {
			return INSTANCE.Range(max);
		}
		
		public static double Range(double min, double max) {
			return INSTANCE.Range(min, max);
		}
		
		public static void ResetWithSeed(int seed) {
			INSTANCE.state = (ulong)seed;
		}
	}
}
