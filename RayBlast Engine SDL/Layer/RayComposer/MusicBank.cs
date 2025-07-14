using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace RayBlast.Composer; 

public class MusicBank {
	public static readonly MusicBank BLANK = new MusicBank();

	public string title = "No name";
	public string[] artists = {
		"Unknown"
	};
	public string album = "None";
	public float mainBPM = 120f;
	public float beatOffset = 0f;
	[JsonIgnore]
	public SoundClip[] channels = Array.Empty<SoundClip>();
	public BPMPoint[] bpmPoints = Array.Empty<BPMPoint>();
	public SignaturePoint[] signaturePoints = Array.Empty<SignaturePoint>();
	public LoopSet[] stageLoops = Array.Empty<LoopSet>();
	public uint[] startSamples = Array.Empty<uint>();
	public float channelFadeTime = 0.5f;

	[JsonIgnore]
	public bool streaming = false;

	public static MusicBank CreateFromJSON(string json, int frequency) {
		return CreateFrom(JsonNode.Parse(json)?.AsObject() ?? new JsonObject(), frequency);
	}

	public static MusicBank CreateFrom(JsonObject JsonObject, int frequency) {
		var bank = new MusicBank {
			title = (string?)JsonObject["title"] ?? "<no title>",
			album = (string?)JsonObject["album"] ?? "<no album>",
			mainBPM = (float?)JsonObject["mainBPM"] ?? 120f,
			beatOffset = (float?)JsonObject["beatOffset"] ?? 0f,
			channelFadeTime = (float?)JsonObject["channelFadeTime"] ?? 0.5f
		};
		var artistsJsonArray = (JsonArray?)JsonObject["artists"];
		if(artistsJsonArray != null)
			bank.artists = artistsJsonArray.Select(token => token?.ToString() ?? "???").ToArray();
		else
			bank.artists = Array.Empty<string>();
		var bpmPointsJsonArray = (JsonArray?)JsonObject["bpmPoints"];
		if(bpmPointsJsonArray != null)
			bank.bpmPoints = bpmPointsJsonArray.Select(token => BPMPoint.CreateFrom(token, frequency)).ToArray();
		else
			bank.bpmPoints = Array.Empty<BPMPoint>();
		var signaturePointsJsonArray = (JsonArray?)JsonObject["signaturePoints"];
		if(signaturePointsJsonArray != null)
			bank.signaturePoints = signaturePointsJsonArray.Select(SignaturePoint.CreateFrom).ToArray();
		else
			bank.signaturePoints = Array.Empty<SignaturePoint>();
		var stageLoopsJsonArray = (JsonArray?)JsonObject["stageLoops"];
		if(stageLoopsJsonArray != null)
			bank.stageLoops = stageLoopsJsonArray.Select(token => LoopSet.CreateFrom(token, frequency)).ToArray();
		else
			bank.stageLoops = Array.Empty<LoopSet>();
		var startSamplesJsonArray = (JsonArray?)JsonObject["startSamples"];
		if(startSamplesJsonArray != null)
			bank.startSamples = startSamplesJsonArray.Select(token => (uint?)token ?? 0).ToArray();
		else {
			var startPointsJsonArray = (JsonArray?)JsonObject["startPoints"];
			if(startPointsJsonArray != null)
				bank.startSamples = startPointsJsonArray.Select(token => (uint)(((double?)token ?? 0.0) * frequency)).ToArray();
			else
				bank.startSamples = Array.Empty<uint>();
		}
		return bank;
	}
}