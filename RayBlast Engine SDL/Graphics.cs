using SDL3;

namespace RayBlast;

public static class Graphics {
    // internal static TextureFilter textureFilter = TextureFilter.Bilinear;

    internal static readonly SDL.BlendMode BLEND_WITH_PREMULTIPLY_ALPHA = SDL.ComposeCustomBlendMode(
        SDL.BlendFactor.SrcAlpha, SDL.BlendFactor.OneMinusSrcAlpha, SDL.BlendOperation.Add,
        SDL.BlendFactor.One, SDL.BlendFactor.OneMinusSrcAlpha, SDL.BlendOperation.Add);

    private static QualitySettings qualitySettings;
    private static int qualityLevel = -1;
    private static readonly List<QualitySettings> QUALITY_PRESETS = [
        new() {
            name = "Base", filterLevel = TextureFilterLevel.Bilinear, useMsaa4x = true
        }
    ];
    private static bool usingMsaa4x;

    public static int QualityLevel {
        get => qualityLevel;
        set {
            if(qualityLevel != value) {
                qualityLevel = value;
                if(qualityLevel >= 0) {
                    qualitySettings = QUALITY_PRESETS[qualityLevel];
                    Debug.LogDebug($"Using quality level {qualityLevel}: {qualitySettings.name}");
                    UseNewQualitySettings();
                }
            }
        }
    }
    public static int MaxQualityLevel => QUALITY_PRESETS.Count - 1;
    public static string[] QualityLevelNames { get; private set; } = ["Base"];
    public static QualitySettings QualitySettings {
        get => qualitySettings;
        set {
            qualitySettings = value;
            qualityLevel = -1;
            UseNewQualitySettings();
        }
    }

    public static int VSyncCount { get; set; } = 1;
    public static int MaxQueuedFrames { get; set; } = 1;
    public static FullscreenMode FullscreenMode { get; set; } = FullscreenMode.Windowed;
    public static S32X2 CurrentResolution {
        get {
            if(RayBlastEngine.window == IntPtr.Zero)
                throw new RayBlastEngineException("Window not initialized");
            if(!SDL.GetWindowSize(RayBlastEngine.window, out int w, out int h))
                throw new RayBlastEngineException($"Could not get window size: {SDL.GetError()}");
            return new S32X2(w, h);
        }
    }
    public static Resolution CurrentDeviceResolution {
        get {
            uint displayID;
            if(RayBlastEngine.window != IntPtr.Zero)
                displayID = SDL.GetDisplayForWindow(RayBlastEngine.window);
            else
                displayID = SDL.GetPrimaryDisplay();
            if(displayID == 0)
                throw new RayBlastEngineException("Cannot get display ID");
            SDL.DisplayMode currentDisplayMode =
                SDL.GetCurrentDisplayMode(displayID) ?? throw new RayBlastEngineException("Cannot get current display mode");
            return new Resolution {
                width = currentDisplayMode.W,
                height = currentDisplayMode.H,
                refreshRateNumerator = (uint)currentDisplayMode.RefreshRateNumerator,
                refreshRateDenominator = (uint)currentDisplayMode.RefreshRateDenominator
            };
        }
    }

    public static List<Resolution> GetAllResolutions() {
        uint displayID;
        if(RayBlastEngine.window != IntPtr.Zero)
            displayID = SDL.GetDisplayForWindow(RayBlastEngine.window);
        else
            displayID = SDL.GetPrimaryDisplay();
        if(displayID == 0)
            throw new RayBlastEngineException("Cannot get display ID");
        SDL.DisplayMode[] fullscreenDisplayModes = SDL.GetFullscreenDisplayModes(displayID, out _)
                                                ?? throw new RayBlastEngineException("Unable to get all fullscreen display modes");
        var resolutions = new List<Resolution>();
        foreach(SDL.DisplayMode mode in fullscreenDisplayModes) {
            resolutions.Add(new Resolution {
                width = mode.W,
                height = mode.H,
                refreshRateNumerator = (uint)mode.RefreshRateNumerator,
                refreshRateDenominator = (uint)mode.RefreshRateDenominator
            });
        }
        return resolutions;
    }

    public static void SetResolution(int width, int height,
                                     FullscreenMode fullscreenMode) {
        if(RayBlastEngine.window == IntPtr.Zero)
            throw new RayBlastEngineException("Window not initialized");
        //TODO_URGENT: Check if certain SDL functions are called out of order
        SDL.SetWindowSize(RayBlastEngine.window, width, height);
        switch(fullscreenMode) {
        case FullscreenMode.Windowed:
            if(SDL.GetWindowFullscreenMode(RayBlastEngine.window) != null)
                SDL.SetWindowFullscreen(RayBlastEngine.window, false);
            Resolution currentDeviceResolution = CurrentDeviceResolution;
            SDL.SetWindowPosition(RayBlastEngine.window, (int)((currentDeviceResolution.width - width) / 2f),
                                  (int)((CurrentDeviceResolution.height - height) / 2f));
            SDL.SetWindowBordered(RayBlastEngine.window, true);
            break;
        case FullscreenMode.BorderlessFullscreen:
            SDL.SetWindowPosition(RayBlastEngine.window, 0, 0);
            SDL.SetWindowBordered(RayBlastEngine.window, false);
            break;
        case FullscreenMode.ExclusiveFullScreen:
            SDL.SetWindowBordered(RayBlastEngine.window, true);
            if(SDL.GetWindowFullscreenMode(RayBlastEngine.window) == null)
                SDL.SetWindowFullscreen(RayBlastEngine.window, true);
            break;
        }
        SDL.SyncWindow(RayBlastEngine.window);
    }

    internal static void SetConfigFlags() {
        //TODO_URGENT: Handle quality settings before the window is created
    }

    private static void UseNewQualitySettings() {
        //TODO_URGENT: Handle quality level initialization
    }

    public static void SetQualityPresets(IEnumerable<QualitySettings> presets) {
        QUALITY_PRESETS.Clear();
        QUALITY_PRESETS.AddRange(presets);
        QualityLevelNames = QUALITY_PRESETS.Select(static q => q.name).ToArray();
        if(qualityLevel > MaxQualityLevel)
            QualityLevel = MaxQualityLevel;
    }

    public record Resolution {
        public int width;
        public int height;
        public uint refreshRateNumerator;
        public uint refreshRateDenominator;
    }
}
