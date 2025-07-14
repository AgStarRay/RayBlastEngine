using Cysharp.Text;

namespace RayBlast.Text;

public class FrigidUtf16Builder(Func<int> indicationFunction, Action<Utf16ValueStringBuilder> buildFunction) : FrigidText(indicationFunction) {
	private Utf16ValueStringBuilder stringBuilder = ZString.CreateStringBuilder();
	private Utf8ValueStringBuilder internalHolder = ZString.CreateUtf8StringBuilder();

	public FrigidUtf16Builder(Action<Utf16ValueStringBuilder> buildFunction) : this(DefaultIndicationFunction, buildFunction) {
	}

	internal override Utf16ValueStringBuilder GetUtf16StringBuilder() {
		stringBuilder.Clear();
		buildFunction(stringBuilder);
		return stringBuilder;
	}

	internal override Utf8ValueStringBuilder GetUtf8StringBuilder() {
		stringBuilder.Clear();
		buildFunction(stringBuilder);
		internalHolder.Clear();
		internalHolder.Append(stringBuilder);
		return internalHolder;
	}

	internal override string GetString() {
		stringBuilder.Clear();
		buildFunction(stringBuilder);
		return stringBuilder.ToString();
	}

	public override void Dispose() {
		stringBuilder.Dispose();
		internalHolder.Dispose();
	}
}
