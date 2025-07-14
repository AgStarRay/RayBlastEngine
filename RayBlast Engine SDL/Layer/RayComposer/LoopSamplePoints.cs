using System.Text.Json.Nodes;

namespace RayBlast.Composer; 

[Serializable]
public struct LoopSamplePoints {
	public uint startSample;
	public uint endSample;

	public static LoopSamplePoints CreateFrom(JsonNode? token, int frequency) {
		JsonNode? startSampleJsonNode = token?["startSample"];
		JsonNode? endSampleJsonNode = token?["endSample"];
		var loopSamplePoints = new LoopSamplePoints();
		if(startSampleJsonNode != null)
			loopSamplePoints.startSample = (uint?)startSampleJsonNode ?? 0;
		else
			loopSamplePoints.startSample = (uint)(((double?)token?["start"] ?? 0.0) * frequency);
		if(endSampleJsonNode != null)
			loopSamplePoints.endSample = (uint?)endSampleJsonNode ?? 0;
		else
			loopSamplePoints.endSample = (uint)(((double?)token?["end"] ?? 0.0) * frequency);
		return loopSamplePoints;
	}
}