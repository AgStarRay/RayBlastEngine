using System.Globalization;
using System.Numerics;
using System.Text;
using RayBlast;

//TODO: Split this class, make it actually partial
// ReSharper disable once ClassTooBig
public static partial class ExtensionMethods {
	private static readonly CultureInfo INVARIANT_CULTURE = CultureInfo.InvariantCulture;
	private static readonly CultureInfo CURRENT_CULTURE = CultureInfo.CurrentCulture;

	private static readonly char[] TRIM_NUMBERS = {
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
	};

	public static string InvariantString(this DateTime value) {
		return value.ToString(INVARIANT_CULTURE);
	}

	public static string InvariantString(this int value) {
		return value.ToString(INVARIANT_CULTURE);
	}

	public static string InvariantString(this uint value) {
		return value.ToString(INVARIANT_CULTURE);
	}

	public static string InvariantString(this short value) {
		return value.ToString(INVARIANT_CULTURE);
	}

	public static string InvariantString(this ushort value) {
		return value.ToString(INVARIANT_CULTURE);
	}

	public static string InvariantString(this byte value) {
		return value.ToString(INVARIANT_CULTURE);
	}

	public static string InvariantString(this sbyte value) {
		return value.ToString(INVARIANT_CULTURE);
	}

	public static string InvariantString(this long value) {
		return value.ToString(INVARIANT_CULTURE);
	}

	public static string InvariantString(this ulong value) {
		return value.ToString(INVARIANT_CULTURE);
	}

	public static string InvariantString(this float value) {
		return value.ToString(INVARIANT_CULTURE);
	}

	public static string InvariantString(this double value) {
		return value.ToString(INVARIANT_CULTURE);
	}

	public static string InvariantString(this byte value, string? format) {
		return value.ToString(format, INVARIANT_CULTURE);
	}

	public static string InvariantString(this int value, string? format) {
		return value.ToString(format, INVARIANT_CULTURE);
	}

	public static string InvariantString(this ulong value, string? format) {
		return value.ToString(format, INVARIANT_CULTURE);
	}

	public static string InvariantString(this float value, string? format) {
		return value.ToString(format, INVARIANT_CULTURE);
	}

	public static string InvariantString(this double value, string? format) {
		return value.ToString(format, INVARIANT_CULTURE);
	}

	public static string CultureString(this DateTime value) {
		return value.ToString(CURRENT_CULTURE);
	}

	public static string CultureString(this int value) {
		return value.ToString(CURRENT_CULTURE);
	}

	public static string CultureString(this uint value) {
		return value.ToString(CURRENT_CULTURE);
	}

	public static string CultureString(this short value) {
		return value.ToString(CURRENT_CULTURE);
	}

	public static string CultureString(this ushort value) {
		return value.ToString(CURRENT_CULTURE);
	}

	public static string CultureString(this byte value) {
		return value.ToString(CURRENT_CULTURE);
	}

	public static string CultureString(this sbyte value) {
		return value.ToString(CURRENT_CULTURE);
	}

	public static string CultureString(this long value) {
		return value.ToString(CURRENT_CULTURE);
	}

	public static string CultureString(this ulong value) {
		return value.ToString(CURRENT_CULTURE);
	}

	public static string CultureString(this float value) {
		return value.ToString(CURRENT_CULTURE);
	}

	public static string CultureString(this double value) {
		return value.ToString(CURRENT_CULTURE);
	}

	public static string CultureString(this int value, string? format) {
		return value.ToString(format, CURRENT_CULTURE);
	}

	public static string CultureString(this uint value, string? format) {
		return value.ToString(format, CURRENT_CULTURE);
	}

	public static string CultureString(this short value, string? format) {
		return value.ToString(format, CURRENT_CULTURE);
	}

	public static string CultureString(this ushort value, string? format) {
		return value.ToString(format, CURRENT_CULTURE);
	}

	public static string CultureString(this byte value, string? format) {
		return value.ToString(format, CURRENT_CULTURE);
	}

	public static string CultureString(this sbyte value, string? format) {
		return value.ToString(format, CURRENT_CULTURE);
	}

	public static string CultureString(this long value, string? format) {
		return value.ToString(format, CURRENT_CULTURE);
	}

	public static string CultureString(this ulong value, string? format) {
		return value.ToString(format, CURRENT_CULTURE);
	}

	public static string CultureString(this float value, string? format) {
		return value.ToString(format, CURRENT_CULTURE);
	}

	public static string CultureString(this double value, string? format) {
		return value.ToString(format, CURRENT_CULTURE);
	}

	public static char Last(this string thisString) {
		if(thisString == null)
			throw new NullReferenceException("String cannot be null");
		if(thisString.Length == 0)
			throw new ArgumentException("String is empty");
		return thisString[thisString.Length - 1];
	}

	public static string SurroundingString(this string mainString, string substring) {
		int indexOfSubstring = mainString.IndexOf(substring, StringComparison.Ordinal);
		if(indexOfSubstring == -1)
			return mainString;
		return mainString.SurroundingString(indexOfSubstring, substring.Length);
	}

	public static string SurroundingString(this string mainString, int startingIndex,
										   int length) {
		if(startingIndex + length >= mainString.Length)
			return mainString.Substring(0, startingIndex);
		return $"{mainString.Substring(0, startingIndex)}{mainString.Substring(startingIndex + length)}";
	}

	public static string RemoveTags(this string str) {
		string newstr = str.RemoveColorTags();
		while(newstr.Contains("<b>")) {
			newstr = newstr.Replace("<b>", "");
		}
		while(newstr.Contains("</b>")) {
			newstr = newstr.Replace("</b>", "");
		}
		while(newstr.Contains("<i>")) {
			newstr = newstr.Replace("<i>", "");
		}
		while(newstr.Contains("</i>")) {
			newstr = newstr.Replace("</i>", "");
		}
		while(newstr.Contains("<u>")) {
			newstr = newstr.Replace("<u>", "");
		}
		while(newstr.Contains("</u>")) {
			newstr = newstr.Replace("</u>", "");
		}
		return newstr;
	}

	public static string RemoveColorTags(this string str) {
		string newstr = str;
		int cursor = newstr.IndexOf("<color=#", StringComparison.Ordinal);
		while(cursor >= 0) {
			string rightstr = newstr.Remove(0, cursor);
			int endCursor = rightstr.IndexOf(">", StringComparison.Ordinal) + 1;
			newstr = newstr.Remove(cursor, endCursor);
			cursor = newstr.IndexOf("<color=#", StringComparison.Ordinal);
		}
		while(newstr.Contains("</color>")) {
			newstr = newstr.Replace("</color>", "");
		}
		return newstr;
	}

	public static string ToTimeRemaining(this float timeLeft) {
		if(timeLeft == float.PositiveInfinity)
			return "Never";
		int days = (int)Math.Floor(timeLeft / 86400f);
		if(days > 99)
			return $"{days.InvariantString()}d";
		int hours = (int)Math.Floor(timeLeft / 3600f % 24f);
		int minutes = (int)Math.Floor(timeLeft / 60f % 60f);
		if(days > 9)
			return $"{days.CultureString()}d{hours.CultureString()}h{minutes.CultureString()}m";
		float seconds = timeLeft % 60f;
		if(days > 0)
			return $"{days.CultureString()}d{hours.CultureString()}h{minutes.CultureString()}m{Math.Floor(seconds).CultureString()}s";
		if(hours > 0)
			return $"{hours.CultureString()}h{minutes.CultureString()}m{Math.Floor(seconds).CultureString()}s";
		if(minutes > 0)
			return $"{minutes.CultureString()}m{seconds.CultureString("0.00")}s";
		return $"{seconds.CultureString("0.000")}s";
	}

	public static string ToTimeRemaining(this double timeLeft) {
		if(timeLeft == double.PositiveInfinity)
			return "Never";
		int days = (int)Math.Floor((timeLeft + 1.0) / 86400.0);
		if(days > 99)
			return $"{days.CultureString()}d";
		int hours = (int)Math.Floor((timeLeft + 1.0) / 3600.0 % 24.0);
		int minutes = (int)Math.Floor((timeLeft + 1.0) / 60.0 % 60.0);
		if(days > 9)
			return $"{days.CultureString()}d{hours.CultureString()}h{minutes.CultureString()}m";
		double seconds = (timeLeft + 1.0) % 60.0;
		if(days > 0)
			return $"{days.CultureString()}d{hours.CultureString()}h{minutes.CultureString()}m{Math.Floor(seconds).CultureString()}s";
		if(hours > 0)
			return $"{hours.CultureString()}h{minutes.CultureString()}m{Math.Floor(seconds).CultureString()}s";
		if(minutes > 0)
			return $"{minutes.CultureString()}m{seconds.CultureString("0.00")}s";
		return $"{seconds.CultureString("0.000")}s";
	}

	/// <summary>
	/// Converts a hexadecimal string representation to a decimal string. 0x prefix is optional.
	/// </summary>
	/// <param name="hexadecimalString">Number string in hexadecimal style</param>
	/// <returns>Number string in decimal style</returns>
	/// <exception cref="System.FormatException"/>
	public static string ConvertFromHexToDecimal(this string hexadecimalString) {
		if(hexadecimalString.Length > 2 && hexadecimalString.ToLower().StartsWith("0x", StringComparison.Ordinal))
			return Convert.ToInt64(hexadecimalString.Substring(2), 16).ToString(CURRENT_CULTURE);
		return Convert.ToInt64(hexadecimalString, 16).ToString(CURRENT_CULTURE);
	}

	/// <summary>
	/// Converts a binary string representation to a decimal string. 0b prefix is optional.
	/// </summary>
	/// <param name="binaryString">Number string in binary style</param>
	/// <returns>Number string in decimal style</returns>
	/// <exception cref="System.FormatException"/>
	public static string ConvertFromBinaryToDecimal(this string binaryString) {
		if(binaryString.Length > 2 && binaryString.ToLower().StartsWith("0b", StringComparison.Ordinal))
			return Convert.ToInt64(binaryString.Substring(2), 2).ToString(CURRENT_CULTURE);
		return Convert.ToInt64(binaryString, 2).ToString(CURRENT_CULTURE);
	}

	/// <summary>
	/// Converts an octal string representation to a decimal string. 0o prefix is optional.
	/// </summary>
	/// <param name="octalString">Number string in octal style</param>
	/// <returns>Number string in decimal style</returns>
	/// <exception cref="System.FormatException"/>
	public static string ConvertFromOctalToDecimal(this string octalString) {
		if(octalString.Length > 2 && octalString.ToLower().StartsWith("0o", StringComparison.Ordinal))
			return Convert.ToInt64(octalString.Substring(2), 8).ToString(CURRENT_CULTURE);
		return Convert.ToInt64(octalString, 8).ToString(CURRENT_CULTURE);
	}

	/// <summary>
	/// Converts a decimal number to a hexadecimal string representation.
	/// </summary>
	/// <param name="decimalNumber">Number</param>
	/// <returns>Number string in hexadecimal style</returns>
	public static string ToHex(this int decimalNumber) {
		return Convert.ToString(decimalNumber, 16);
	}

	public static string ToBinary(this int decimalNumber) {
		return Convert.ToString(decimalNumber, 2);
	}

	/// <summary>
	/// Converts a decimal number to a hexadecimal string representation.
	/// </summary>
	/// <param name="decimalNumber">Number</param>
	/// <returns>Number string in hexadecimal style</returns>
	public static string ToHex(this long decimalNumber) {
		return Convert.ToString(decimalNumber, 16);
	}

	/// <summary>
	/// Converts a decimal number to a binary string representation.
	/// </summary>
	/// <param name="decimalNumber">Number</param>
	/// <returns>Number string in binary style</returns>
	public static string ToBinary(this long decimalNumber) {
		return Convert.ToString(decimalNumber, 2);
	}

	/// <summary>
	/// Converts a decimal number to an octal string representation.
	/// </summary>
	/// <param name="decimalNumber">Number</param>
	/// <returns>Number string in octal style</returns>
	public static string ToOctal(this long decimalNumber) {
		return Convert.ToString(decimalNumber, 8);
	}

	public static string ToBinary(this uint decimalNumber) {
		return Convert.ToString(decimalNumber, 2);
	}

	public static string ToBinary(this ulong decimalNumber) {
		return Convert.ToString((long)decimalNumber, 2);
	}

	public static bool BitIsSet(this byte thisByte, int bitIndex) {
		if(bitIndex is < 0 or >= 8)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of a byte");
		return (thisByte & (1 << bitIndex)) != 0;
	}

	public static byte WithBit(this byte thisByte, int bitIndex) {
		if(bitIndex is < 0 or >= 8)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of a byte");
		return (byte)(thisByte | (1 << bitIndex));
	}

	public static byte WithoutBit(this byte thisByte, int bitIndex) {
		if(bitIndex is < 0 or >= 8)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of a byte");
		return (byte)(thisByte & ~(1 << bitIndex));
	}

	public static bool BitIsSet(this short thisShort, int bitIndex) {
		if(bitIndex is < 0 or >= 16)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of a short");
		return (thisShort & (1 << bitIndex)) != 0;
	}

	public static short WithBit(this short thisShort, int bitIndex) {
		if(bitIndex is < 0 or >= 16)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of a short");
		return (short)(thisShort | (short)(1 << bitIndex));
	}

	public static short WithoutBit(this short thisShort, int bitIndex) {
		if(bitIndex is < 0 or >= 16)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of a short");
		return (short)(thisShort & ~(1 << bitIndex));
	}

	public static bool BitIsSet(this ushort thisShort, int bitIndex) {
		if(bitIndex is < 0 or >= 16)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of a ushort");
		return (thisShort & (1 << bitIndex)) != 0;
	}

	public static ushort WithBit(this ushort thisShort, int bitIndex) {
		if(bitIndex is < 0 or >= 16)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of a ushort");
		return (ushort)(thisShort | (ushort)(1 << bitIndex));
	}

	public static ushort WithoutBit(this ushort thisShort, int bitIndex) {
		if(bitIndex is < 0 or >= 16)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of a ushort");
		return (ushort)(thisShort & ~(1 << bitIndex));
	}

	public static bool BitIsSet(this int thisInt, int bitIndex) {
		if(bitIndex is < 0 or >= 32)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of an Int32");
		return (thisInt & (1 << bitIndex)) != 0;
	}

	public static int WithBit(this int thisInt, int bitIndex) {
		if(bitIndex is < 0 or >= 32)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of an Int32");
		return thisInt | (1 << bitIndex);
	}

	public static int WithoutBit(this int thisInt, int bitIndex) {
		if(bitIndex is < 0 or >= 32)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of an Int32");
		return thisInt & ~(1 << bitIndex);
	}

	public static bool BitIsSet(this long thisLong, int bitIndex) {
		if(bitIndex is < 0 or >= 64)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of an Int64");
		return (thisLong & (1L << bitIndex)) != 0;
	}

	public static long WithBit(this long thisLong, int bitIndex) {
		if(bitIndex is < 0 or >= 64)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of an Int64");
		return thisLong | (1L << bitIndex);
	}

	public static long WithoutBit(this long thisLong, int bitIndex) {
		if(bitIndex is < 0 or >= 64)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of an Int64");
		return thisLong & ~(1L << bitIndex);
	}

	public static bool BitIsSet(this ulong thisLong, int bitIndex) {
		if(bitIndex is < 0 or >= 64)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of a UInt64");
		return (thisLong & ((ulong)1 << bitIndex)) != 0;
	}

	public static ulong WithBit(this ulong thisLong, int bitIndex) {
		if(bitIndex is < 0 or >= 64)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of a UInt64");
		return thisLong | ((ulong)1 << bitIndex);
	}

	public static ulong WithoutBit(this ulong thisLong, int bitIndex) {
		if(bitIndex is < 0 or >= 64)
			throw new ArgumentOutOfRangeException($"Bit index {bitIndex} is out of range of a UInt64");
		return thisLong & ~((ulong)1 << bitIndex);
	}

	public static bool[] ToBoolArray(this int thisInt) {
		bool[] array = new bool[32];
		for(int i = 0; i < 32; i++) {
			array[i] = (thisInt & (1 << i)) > 0;
		}
		return array;
	}

	public static string ToTitleCase(this string thisString) {
		TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
		return textInfo.ToTitleCase(thisString.ToLower());
	}

	public static string Join<T>(this IEnumerable<T> thisGroup, string separator) {
		return string.Join(separator, thisGroup);
	}

	public static string Join<T>(this IEnumerable<T> thisGroup, char separator) {
		return string.Join(separator, thisGroup);
	}

	public static string JoinWithEnding<T>(this IEnumerable<T> thisGroup, string separator,
										   string ending) {
		List<T> list = thisGroup.ToList();
		switch(list.Count) {
		case 0:
			return "";
		case 1:
			return list[0]?.ToString() ?? "null";
		case 2:
			separator = "";
			break;
		}
		string result = string.Join(separator, list);
		int insertionIndex = result.Length - (list.Last()?.ToString() ?? "null").Length;
		return result.Insert(insertionIndex, ending);
	}

	public static bool IsNumber(this char character) {
		return character is >= '0' and <= '9';
	}

	public static string TrimEndNumbers(this string original) {
		return original.TrimEnd(TRIM_NUMBERS);
	}

	public static void Write(this BinaryWriter writer, List<bool> boolArray) {
		int x = 0;
		byte compilation = 0;
		foreach(bool b in boolArray) {
			if(x >= 8) {
				writer.Write(compilation);
			}
			compilation += (byte)(b ? 1 << x++ : 0);
		}
		writer.Write(compilation);
	}

	public static void ReadBits(this BinaryReader reader, int bitCount,
								IList<bool> list) {
		list.Clear();
		for(int i = 0; i < bitCount; i += 8) {
			byte data = reader.ReadByte();
			for(int j = 0; j < 8; j++) {
				if(i + j < bitCount)
					list.Add((data & (1 << j)) > 0);
			}
		}
	}

	public static StringBuilder AppendLine(this StringBuilder builder, char value) {
		return builder.Append(value).Append(Environment.NewLine);
	}

	public static StringBuilder AppendLine(this StringBuilder builder, bool value) {
		return builder.Append(value).Append(Environment.NewLine);
	}

	public static StringBuilder AppendLine(this StringBuilder builder, object value) {
		return builder.Append(value).Append(Environment.NewLine);
	}

	public static StringBuilder BinaryString(this ICollection<bool> boolArray) {
		var builder = new StringBuilder(boolArray.Count);
		foreach(bool b in boolArray) {
			builder.Append(b ? "1" : "0");
		}
		return builder;
	}

	public static int GetComparisonHash(this object obj) {
		Type t = obj.GetType();
		if(t.IsPrimitive || t == typeof(string) || t == typeof(decimal))
			return obj.GetHashCode();
		return 0;
	}

	public static Vector2 Normalized(this Vector2 vector) {
		return vector / vector.Length();
	}

	public static Vector3 Normalized(this Vector3 vector) {
		return vector / vector.Length();
	}
}
