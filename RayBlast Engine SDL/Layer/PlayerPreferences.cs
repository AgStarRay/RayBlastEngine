namespace RayBlast;

public static class PlayerPreferences {
	private static Dictionary<string, int>? allInts = null;
	private static Dictionary<string, float>? allFloats = null;
	private static Dictionary<string, double>? allDoubles = null;
	private static Dictionary<string, string?>? allStrings = null;
	internal static bool hasChanges = false;
	public static bool Loaded { get; private set; }

	internal static void Load() {
		allInts = new Dictionary<string, int>();
		allFloats = new Dictionary<string, float>();
		allDoubles = new Dictionary<string, double>();
		allStrings = new Dictionary<string, string?>();
		if(File.Exists($"{IO.PrivateDirectoryInfo.FullName}/playerprefs")) {
			byte[] bytes = File.ReadAllBytes($"{IO.PrivateDirectoryInfo.FullName}/playerprefs");
			using MemoryStream memoryStream = new MemoryStream(bytes);
			using BinaryReader reader = new BinaryReader(memoryStream);
			if(memoryStream.Position < bytes.Length)
				reader.ReadByte();
			while(memoryStream.Position < bytes.Length) {
				string key = reader.ReadString();
				byte code = reader.ReadByte();
				switch(code) {
				case 1:
					allInts[key] = reader.ReadInt32();
					break;
				case 2:
					allFloats[key] = reader.ReadSingle();
					break;
				case 3:
					allDoubles[key] = reader.ReadDouble();
					break;
				case 4:
					allStrings[key] = reader.ReadString();
					break;
				default:
					throw new RayBlastEngineException($"Unknown pref code {code}");
				}
			}
		}
		Loaded = true;
	}

	internal static void Save() {
		if(!Loaded)
			throw new RayBlastEngineException("PlayerPreferences were not loaded");
		hasChanges = false;
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(memoryStream);
		writer.Write((byte)0);
		foreach(KeyValuePair<string,int> kvp in allInts!) {
			writer.Write(kvp.Key);
			writer.Write((byte)1);
			writer.Write(kvp.Value);
		}
		foreach(KeyValuePair<string,float> kvp in allFloats!) {
			writer.Write(kvp.Key);
			writer.Write((byte)2);
			writer.Write(kvp.Value);
		}
		foreach(KeyValuePair<string,double> kvp in allDoubles!) {
			writer.Write(kvp.Key);
			writer.Write((byte)3);
			writer.Write(kvp.Value);
		}
		foreach(KeyValuePair<string,string?> kvp in allStrings!) {
			if(kvp.Value != null) {
				writer.Write(kvp.Key);
				writer.Write((byte)4);
				writer.Write(kvp.Value);
			}
		}
		File.WriteAllBytes($"{IO.PrivateDirectoryInfo.FullName}/playerprefs", memoryStream.ToArray());
	}

	public static int GetInt(string keyName, int defaultValue) {
		if(allInts == null)
			throw new RayBlastEngineException("RayBlast engine not initialized");
		return allInts.GetValueOrDefault(keyName, defaultValue);
	}

	public static float GetFloat(string keyName, float defaultValue) {
		if(allFloats == null)
			throw new RayBlastEngineException("RayBlast engine not initialized");
		return allFloats.GetValueOrDefault(keyName, defaultValue);
	}

	public static double GetDouble(string keyName, double defaultValue) {
		if(allDoubles == null)
			throw new RayBlastEngineException("RayBlast engine not initialized");
		return allDoubles.GetValueOrDefault(keyName, defaultValue);
	}

	public static string? GetString(string keyName) {
		if(allStrings == null)
			throw new RayBlastEngineException("RayBlast engine not initialized");
		return allStrings.GetValueOrDefault(keyName);
	}

	public static string GetString(string keyName, string defaultValue) {
		if(allStrings == null)
			throw new RayBlastEngineException("RayBlast engine not initialized");
		if(allStrings.TryGetValue(keyName, out string? result) && result != null)
			return result;
		return defaultValue;
	}

	public static void SetInt(string keyName, int value) {
		if(allInts == null)
			throw new RayBlastEngineException("RayBlast engine not initialized");
		allInts[keyName] = value;
		hasChanges = true;
	}

	public static void SetFloat(string keyName, float value) {
		if(allFloats == null)
			throw new RayBlastEngineException("RayBlast engine not initialized");
		allFloats[keyName] = value;
		hasChanges = true;
	}

	public static void SetDouble(string keyName, double value) {
		if(allDoubles == null)
			throw new RayBlastEngineException("RayBlast engine not initialized");
		allDoubles[keyName] = value;
		hasChanges = true;
	}

	public static void SetString(string keyName, string? value) {
		if(allStrings == null)
			throw new RayBlastEngineException("RayBlast engine not initialized");
		allStrings[keyName] = value;
		hasChanges = true;
	}
}
