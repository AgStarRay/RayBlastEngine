using System.Text.Json.Nodes;

namespace RayBlast.Composer; 

[Serializable]
public struct SkipSamplePoints {
	public uint fromSample;
	public uint toSample;

	public static SkipSamplePoints CreateFrom(JsonNode? token, int frequency) {
		JsonNode? fromSampleJsonNode = token?["fromSample"];
		JsonNode? toSampleJsonNode = token?["toSample"];
		var loopSamplePoints = new SkipSamplePoints();
		if(fromSampleJsonNode != null)
			loopSamplePoints.fromSample = (uint?)fromSampleJsonNode ?? 0;
		else
			loopSamplePoints.fromSample = (uint)(((double?)token?["from"] ?? 0.0) * frequency);
		if(toSampleJsonNode != null)
			loopSamplePoints.toSample = (uint?)toSampleJsonNode ?? 0;
		else
			loopSamplePoints.toSample = (uint)(((double?)token?["to"] ?? 0.0) * frequency);
		return loopSamplePoints;
	}
}