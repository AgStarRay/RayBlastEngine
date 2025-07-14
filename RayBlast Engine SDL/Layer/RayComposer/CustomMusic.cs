using System.Diagnostics;
using System.Text.Json;

namespace RayBlast.Composer;

//TODO: Fix some of the new exceptions with Debug.LogException
public static class CustomMusic {
    private static string[]? jsonFileNames = null;
    private static readonly Dictionary<string, MusicBank> LOADED_BANKS = new();
    private static readonly Dictionary<string, string> LOADING_BANK_JSON = new();
    private static readonly Dictionary<string, string> FILE_PATHS = new();
    private static readonly Dictionary<string, List<RayBlastSoundHttp>> CURRENT_REQUESTS = new();

    public const int DOWNLOAD_THRESHOLD = 1048576;

    public static string[] JsonNames {
        get {
            if(jsonFileNames == null)
                RefreshJsonNames();
            return jsonFileNames!;
        }
    }

    public static void Clear() {
        LOADED_BANKS.Clear();
        LOADING_BANK_JSON.Clear();
        FILE_PATHS.Clear();
        foreach(List<RayBlastSoundHttp> asyncList in CURRENT_REQUESTS.Values) {
            foreach(RayBlastSoundHttp operation in asyncList) {
                operation?.Dispose();
            }
        }
        CURRENT_REQUESTS.Clear();
        jsonFileNames = null;
    }

    public static void RefreshJsonNames() {
        try {
            var newNames = new List<string>();
            Directory.CreateDirectory($"{IO.GameDirectoryInfo.FullName}/Music");
            var di = new DirectoryInfo($"{IO.GameDirectoryInfo.FullName}/Music");
            foreach(FileInfo fi in di.GetFiles()) {
                if(fi.Extension == ".json")
                    newNames.Add(fi.Name.Substring(0, fi.Name.Length - 5));
            }
            jsonFileNames = newNames.ToArray();
        }
        catch(UnauthorizedAccessException e) {
            jsonFileNames = Array.Empty<string>();
            throw new RayBlastEngineException($"{e.Message}\n\nPlease check for a Music folder in the game directory, or create it if it doesn't exist.");
        }
    }

    public static void StartLoadingAllSongs() {
        var watch = Stopwatch.StartNew();
        foreach(string json in JsonNames) {
            if(!LOADED_BANKS.ContainsKey(json) && !LOADING_BANK_JSON.ContainsKey(json)) {
                try {
                    StartLoadingBankFromJsonName(json);
                }
                catch(JsonException e) {
                    Debug.LogError($"Error reading \"{json}\"", includeStackTrace: false);
                    Debug.LogException(e);
                }
            }
        }
        Debug.Log($"Started loading all custom music in {watch.Elapsed.ToString()}");
    }

    public static MusicBank? GetBankFromJsonName(string jsonFileName, bool waitForLoading = true) {
        if(!LOADED_BANKS.ContainsKey(jsonFileName)) {
            if(!LOADING_BANK_JSON.ContainsKey(jsonFileName))
                StartLoadingBankFromJsonName(jsonFileName);
            var channels = new List<SoundClip>();
            foreach(RayBlastSoundHttp request in CURRENT_REQUESTS[jsonFileName]) {
                while(true) {
                    switch(request.State) {
                    case RayBlastHttp.Result.ConnectionError:
                        throw new RayBlastEngineException($"Failed to retrieve {jsonFileName} files.");
                    case RayBlastHttp.Result.ProtocolError:
                        throw new RayBlastEngineException($"Failed to handle {jsonFileName} files.");
                    case RayBlastHttp.Result.DataProcessingError:
                        throw new RayBlastEngineException($"Failed to process {jsonFileName} files.");
                    }
                    if(request.IsDone) {
                        if(!request.LoadAsStreamable)
                            break;
                        if(request.DownloadedByteCount > 2048 && request.DownloadProgress > 0.01f)
                            break;
                    }
                    if(!waitForLoading)
                        return null;
                    Thread.Sleep(10);
                }
                try {
                    SoundClip clip = request.GetSound();
                    channels.Add(clip);
                }
                catch(InvalidOperationException e) {
                    Debug.LogException(e);
                    throw new RayBlastEngineException(
                        $"\"{request.URL}\" is not a valid audio file. Please use valid WAV and OGG audio types for your music banks.");
                }
                request.Dispose();
            }
            var bank = MusicBank.CreateFromJSON(LOADING_BANK_JSON[jsonFileName], channels[0].Frequency);
            foreach(SoundClip clip in channels) {
                clip.Name = bank.title;
            }
            CURRENT_REQUESTS[jsonFileName].Clear();
            bank.channels = channels.ToArray();
            bank.streaming = Game.Settings.streamMusic;
            LOADED_BANKS[jsonFileName] = bank;
        }
        return GetBankIfLoaded(jsonFileName);
    }

    public static MusicBank? GetBankIfLoaded(string jsonFileName) {
        return LOADED_BANKS.TryGetValue(jsonFileName, out MusicBank? value) ? value : null;
    }

    public static bool CheckIfBankFromJsonIsLoading(string jsonFileName) {
        return LOADING_BANK_JSON.ContainsKey(jsonFileName);
    }

    public static string? GetBankJson(string jsonFileName) {
        return LOADING_BANK_JSON.GetValueOrDefault(jsonFileName);
    }

    private static void StartLoadingBankFromJsonName(string jsonFileName) {
        var di = new DirectoryInfo($"{IO.GameDirectoryInfo.FullName}/Music");
        // var cacheDI = new DirectoryInfo($"{IO.GameDirectoryInfo.FullName}/Music/Cache");
        // FileInfo[] cacheFiles = [];
        // if(cacheDI.Exists)
        //     cacheFiles = cacheDI.GetFiles();
        string filePath = Path.Combine(di.FullName, $"{jsonFileName}.json");
        if(!File.Exists(filePath))
            throw new FileNotFoundException($"Cannot find JSON file: {filePath}");
        FILE_PATHS[jsonFileName] = filePath;
        CURRENT_REQUESTS[jsonFileName] = new List<RayBlastSoundHttp>();
        var channelFiles = new List<string>();
        foreach(FileInfo fi in di.GetFiles()) {
            if(fi.Name.StartsWith(jsonFileName, StringComparison.Ordinal)
            && fi.Extension != ".txt" && fi.Extension != ".json")
                channelFiles.Add(fi.FullName);
        }
        if(channelFiles.Count == 0) {
            throw new RayBlastEngineException(
                $"Cannot find any audio files that start with \"{jsonFileName}\". Please move out or delete the JSON file in the game's Music folder. If you meant to load this song, make sure there are audio files in the same folder. Safe ones to use are OGG and WAV. MP3s might work.");
        }
        channelFiles.Sort();
        foreach(string file in channelFiles) {
            var audioType = SoundFileType.WAV;
            if(file.EndsWith("ogg", StringComparison.Ordinal))
                audioType = SoundFileType.OGG;
            if(file.EndsWith("mp3", StringComparison.Ordinal)) {
                audioType = SoundFileType.MP3;
            }
            var uwr = RayBlastSoundHttp.CreateSoundGet(new Uri(file), audioType);
            uwr.LoadAsStreamable = Game.Settings.streamMusic;
            uwr.SendRequest();
            CURRENT_REQUESTS[jsonFileName].Add(uwr);
        }
        LOADING_BANK_JSON[jsonFileName] = File.ReadAllText(FILE_PATHS[jsonFileName]);
    }
}
