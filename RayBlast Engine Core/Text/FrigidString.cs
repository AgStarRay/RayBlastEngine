using Cysharp.Text;

namespace RayBlast.Text;

public class FrigidString(Func<int> indicationFunction, Func<string> buildFunction) : FrigidText(indicationFunction) {
	private Utf16ValueStringBuilder internalUtf16Holder = ZString.CreateStringBuilder();
	private Utf8ValueStringBuilder internalUtf8Holder = ZString.CreateUtf8StringBuilder();

	public FrigidString(Func<string> buildFunction) : this(DefaultIndicationFunction, buildFunction) {
	}

	internal override Utf16ValueStringBuilder GetUtf16StringBuilder() {
		internalUtf16Holder.Clear();
		internalUtf16Holder.Append(buildFunction());
		return internalUtf16Holder;
	}

	internal override Utf8ValueStringBuilder GetUtf8StringBuilder() {
		internalUtf8Holder.Clear();
		internalUtf16Holder.Append(buildFunction());
		return internalUtf8Holder;
	}

	internal override string GetString() {
		return buildFunction();
	}

	public override void Dispose() {
		internalUtf16Holder.Dispose();
		internalUtf8Holder.Dispose();
	}
}
