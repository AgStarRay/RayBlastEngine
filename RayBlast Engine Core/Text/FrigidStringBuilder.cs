using System.Text;
using Cysharp.Text;

namespace RayBlast.Text;

public class FrigidStringBuilder(Func<int> indicationFunction, Action<StringBuilder> buildFunction) : FrigidText(indicationFunction) {
	private readonly StringBuilder stringBuilder = new();
	private Utf16ValueStringBuilder internalUtf16Holder = ZString.CreateStringBuilder();
	private Utf8ValueStringBuilder internalUtf8Holder = ZString.CreateUtf8StringBuilder();

	public FrigidStringBuilder(Action<StringBuilder> buildFunction) : this(DefaultIndicationFunction, buildFunction) {
	}

	internal override Utf16ValueStringBuilder GetUtf16StringBuilder() {
		stringBuilder.Clear();
		buildFunction(stringBuilder);
		internalUtf16Holder.Clear();
		internalUtf16Holder.Append(stringBuilder);
		return internalUtf16Holder;
	}

	internal override Utf8ValueStringBuilder GetUtf8StringBuilder() {
		stringBuilder.Clear();
		buildFunction(stringBuilder);
		internalUtf8Holder.Clear();
		internalUtf8Holder.Append(stringBuilder);
		return internalUtf8Holder;
	}

	internal override string GetString() {
		stringBuilder.Clear();
		buildFunction(stringBuilder);
		return stringBuilder.ToString();
	}

	public override void Dispose() {
		internalUtf16Holder.Dispose();
		internalUtf8Holder.Dispose();
	}
}
