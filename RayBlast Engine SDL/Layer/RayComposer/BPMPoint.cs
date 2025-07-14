using System.Text.Json.Nodes;

namespace RayBlast.Composer; 

[Serializable]
public struct BPMPoint {
	public uint sample;
	public float bpm;

	public static BPMPoint CreateFrom(JsonNode? token, int frequency) {
		JsonNode? sampleJsonNode = token?["sample"];
		var bpmPoint = new BPMPoint {
			bpm = (float?)token?["bpm"] ?? 120f
		};
		if(sampleJsonNode != null)
			bpmPoint.sample = (uint?)sampleJsonNode ?? 0;
		else
			bpmPoint.sample = (uint)(((double?)token?["clipTime"] ?? 0.0) * frequency);
		return bpmPoint;
	}
}