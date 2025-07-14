using System.Text.Json.Nodes;

namespace RayBlast.Composer; 

[Serializable]
public class LoopSet {
	public LoopSamplePoints[] loopSegments = Array.Empty<LoopSamplePoints>();
	public SkipSamplePoints[] skipSegments = Array.Empty<SkipSamplePoints>();
	public float[] channelLevels = Array.Empty<float>();

	public static LoopSet CreateFrom(JsonNode? token, int frequency) {
		var loopSet = new LoopSet();
		var loopSegmentsJsonArray = (JsonArray?)token?["loopSegments"];
		if(loopSegmentsJsonArray != null)
			loopSet.loopSegments = loopSegmentsJsonArray.Select(t => LoopSamplePoints.CreateFrom(t, frequency)).ToArray();
		else
			loopSet.loopSegments = Array.Empty<LoopSamplePoints>();
		var skipSegmentsJsonArray = (JsonArray?)token?["skipSegments"];
		if(skipSegmentsJsonArray != null)
			loopSet.skipSegments = skipSegmentsJsonArray.Select(t => SkipSamplePoints.CreateFrom(t, frequency)).ToArray();
		else
			loopSet.skipSegments = Array.Empty<SkipSamplePoints>();
		var channelLevelsJsonArray = (JsonArray?)token?["channelLevels"];
		if(channelLevelsJsonArray != null)
			loopSet.channelLevels = channelLevelsJsonArray.Select(t => (float?)t ?? 1f).ToArray();
		else
			loopSet.channelLevels = Array.Empty<float>();
		return loopSet;
	}
}