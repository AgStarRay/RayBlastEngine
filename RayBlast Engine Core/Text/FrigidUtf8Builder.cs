using Cysharp.Text;

namespace RayBlast.Text;

public class FrigidUtf8Builder(Func<int> indicationFunction, Action<Utf8ValueStringBuilder> buildFunction) : FrigidText(indicationFunction) {
	private Utf8ValueStringBuilder stringBuilder = ZString.CreateUtf8StringBuilder();
	private Utf16ValueStringBuilder internalHolder = ZString.CreateStringBuilder();

	public FrigidUtf8Builder(Action<Utf8ValueStringBuilder> buildFunction) : this(DefaultIndicationFunction, buildFunction) {
	}

	internal override Utf16ValueStringBuilder GetUtf16StringBuilder() {
		stringBuilder.Clear();
		buildFunction(stringBuilder);
		internalHolder.Clear();
		internalHolder.Append(stringBuilder);
		return internalHolder;
	}

	internal override Utf8ValueStringBuilder GetUtf8StringBuilder() {
		stringBuilder.Clear();
		buildFunction(stringBuilder);
		return stringBuilder;
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
