using System.Diagnostics;
using RayBlast.Composer;

namespace RayBlast;

public static class Game {
    internal static bool requestedClose = false;

    #if DEBUG
	//TODO_AFTER: Persist across builds
	public static string customSongToPlay = "";
	public static bool randomCustomSong = false;
    #endif
    public static GameSettings Settings { get; } = new();

    public static bool PlayingCustomMusic {
        get {
            #if DEBUG
				return customSongToPlay != "";
            #else
            return false;
            #endif
        }
    }

    public static MusicBank? CustomBank { get; set; }

    public static bool TimeCapped => Time.deltaTime >= Time.maximumDeltaTime;

    //TODO_AFTER: Add VR support
    public static bool InVR => false;

    public static bool IsFocused => RayBlastEngine.IsFocused;
    private static int targetFrameRate;
    public static bool IsRunning { get; internal set; }
    public static int TargetFrameRate {
        get => targetFrameRate;
        set {
            targetFrameRate = value;
            RayBlastEngine.SetTargetFPS(value);
        }
    }
    public static string Version { get; internal set; } = "0.0.0.0";

    public static void OpenURL(string url) {
        if(url.StartsWith("http://", StringComparison.Ordinal) || url.StartsWith("https://", StringComparison.Ordinal)) {
            Process.Start(url);
        }
        else {
            Process.Start(new ProcessStartInfo {
                FileName = url,
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }

    public static void Quit() {
        requestedClose = true;
    }

    public class GameSettings {
        public bool soundEnabled = true;
        public bool musicEnabled = true;
        public bool streamMusic = false;

        private float soundLevel = 1f;
        private float musicLevel = 0.86f;

        public float SoundLevel {
            get => soundEnabled ? soundLevel : 0f;
            set => soundLevel = value;
        }

        public float MusicLevel {
            get => musicEnabled ? musicLevel : 0f;
            set => musicLevel = value;
        }
    }
}
