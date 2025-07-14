namespace RayBlast;

public static class IdleTimings {
	public static double animationTime = 0f;

	public static double QuarterCosine => Math.Cos(animationTime * Math.PI * 8f);

	public static double HalfCosine => Math.Cos(animationTime * Math.PI * 4f);

	public static double WholeCosine => Math.Cos(animationTime * Math.PI * 2f);

	public static double DoubleCosine => Math.Cos(animationTime * Math.PI);

	public static double QuadrupleCosine => Math.Cos(animationTime * Math.PI / 2f);

	public static double OctupleCosine => Math.Cos(animationTime * Math.PI / 4f);

	public static void Update() {
		//TODO: Uncomment
		// animationTime += (BeatTracker.UnscaledBPM * Time.deltaTime / 240.0) % 8.0;
	}
}
