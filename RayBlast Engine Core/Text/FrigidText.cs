using Cysharp.Text;

namespace RayBlast.Text;

/// <summary>
/// Optional text container to prevent unnecessary expensive string creation.
/// If a rendering component has this as a property, use it with an indication function to lower rendering cost.
/// </summary>
/// <param name="indicationFunction">Function that generates a number; if a different number is generated, a different string is expected</param>
public abstract class FrigidText(Func<int> indicationFunction) : IDisposable {
	protected static int DefaultIndicationFunction() {
		return RNG.TimeSeed;
	}

	private int previousIndication;

	public int ValueIndicator => indicationFunction();

	public bool CheckForNewText() {
		int newIndication = ValueIndicator;
		if(newIndication != previousIndication) {
			previousIndication = newIndication;
			return true;
		}
		return false;
	}

	internal abstract Utf16ValueStringBuilder GetUtf16StringBuilder();
	internal abstract Utf8ValueStringBuilder GetUtf8StringBuilder();
	internal abstract string GetString();
	public abstract void Dispose();
}
