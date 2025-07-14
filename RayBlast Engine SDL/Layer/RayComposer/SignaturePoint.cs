using System.Text.Json.Nodes;

namespace RayBlast.Composer; 

[Serializable]
public struct SignaturePoint {
	public int measureNumber;
	public int ticks;

	public SignaturePoint(int rate = 16) {
		measureNumber = 0;
		ticks = rate;
	}

	public static SignaturePoint CreateFrom(JsonNode? token) {
		return new SignaturePoint {
			measureNumber = (int?)token?["measureNumber"] ?? 1,
			ticks = (int?)token?["ticks"] ?? 16
		};
	}
}