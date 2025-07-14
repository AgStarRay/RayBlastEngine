using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Cysharp.Text;
using RayBlast.Composer;
using SDL3;

namespace RayBlast;

//TODO: Add multi-channel looping music support with another version of RayBlast.Composer
//TODO: Add support for forward, deferred, and ray march lighting
//TODO: Add text parsing string type that internally uses Utf16StringBuilder
//TODO: Use a render textures that can be applied to both frame buffers during spinning
//TODO: Use TryFormat instead of InvariantString
//TODO: Try other ways to make logging faster
public static unsafe class RayBlastEngine {
    internal static readonly Stopwatch WATCH = new();
    internal static readonly Stopwatch LOG_WATCH = new();

    private static readonly Stopwatch FRAME_WATCH = new();

    public static string windowTitle = "";
    public static string rendererName = "";
    public static string glslName = "";
    // internal static GlVersion glVersion;

    private static byte canContinue = 1;
    private static FileStream? logFileStream;
    private static StreamWriter? logWriter;
    private static RayBlastLogStreamWriter? logStreamWriter;
    private static RayBlastMethod? updateMethod;
    private static RayBlastMethod? renderMethod;
    internal static bool windowIsAvailable;
    private static bool preinitialized;
    private static float targetFPS;
    internal static IntPtr window;
    internal static IntPtr renderer;

    public static bool OpenGL11Supported { get; internal set; }
    public static bool OpenGL21Supported { get; internal set; }
    public static bool OpenGL33Supported { get; internal set; }
    public static bool OpenGL43Supported { get; internal set; }
    public static bool OpenGLES2Supported { get; internal set; }
    public static DirectoryInfo CurrentDirectoryInfo { get; } = new(Environment.CurrentDirectory);
    public static DirectoryInfo LocalAppDataDirectoryInfo { get; private set; } =
        new(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
    public static DirectoryInfo RoamingAppDataDirectoryInfo { get; private set; } =
        new(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
    public static bool UserRequestedClose {
        get {
            bool requestedClose = Game.requestedClose;
            Game.requestedClose = false;
            return requestedClose;
        }
    }
    public static bool IsFocused { get; private set; } = true;

    public static double RealtimeSinceStartup => WATCH.Elapsed.TotalSeconds;

    public delegate void RayBlastMethod();

    public static void Preinitialize() {
        try {
            LOG_WATCH.Restart();
            var entryAssembly = Assembly.GetCallingAssembly();
            string companyName = entryAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company
                              ?? throw new NullReferenceException($"No company specified for entry assembly {entryAssembly.FullName}");
            AssemblyName assemblyName = entryAssembly.GetName();
            string localDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                companyName,
                                                assemblyName.Name
                                             ?? throw new ArgumentNullException($"No assembly name specified for {entryAssembly.FullName}"));
            string roamingDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                  companyName,
                                                  assemblyName.Name
                                               ?? throw new ArgumentNullException($"No assembly name specified for {entryAssembly.FullName}"));
            LocalAppDataDirectoryInfo = Directory.CreateDirectory(localDataPath);
            RoamingAppDataDirectoryInfo = Directory.CreateDirectory(roamingDataPath);
            logFileStream = new FileStream($"{localDataPath}/rayblast.log", FileMode.Create, FileAccess.Write);
            logWriter = new StreamWriter(logFileStream) {
                AutoFlush = false
            };
            var newWriter = new RayBlastLogStreamWriter(logWriter);
            lock(newWriter) {
                logStreamWriter = newWriter;
                Console.SetOut(logStreamWriter);
                Console.SetError(logStreamWriter);
            }

            Debug.Log("Preinitializing RayBlast Engine");
            UnmanagedManager.mainThreadId = Environment.CurrentManagedThreadId;
            Version? version = assemblyName.Version;
            Game.Version = version?.ToString() ?? "???";
            Debug.Log($"Game version = {Game.Version}");
            Debug.Log($"Processor brand name = {Debug.SystemProcessorName}");
            PlayerPreferences.Load();
            //TODO: Disable SDL3 audio
            if(!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Audio | SDL.InitFlags.Gamepad | SDL.InitFlags.Haptic)) {
                throw new RayBlastEngineException($"SDL3 failed to initialize: {SDL.GetError()}");
            }
            if(!TTF.Init()) {
                throw new RayBlastEngineException($"SDL_ttf failed to initialize: {SDL.GetError()}");
            }
            preinitialized = true;
        }
        catch(Exception e) {
            Debug.LogException(e);
        }
    }

    public static void Initialize(int width = 0, int height = 0) {
        if(!preinitialized)
            Preinitialize();
        bool windowCreated = false;
        try {
            Debug.Log("Initializing RayBlast Engine");
            if(width == 0 || height == 0) {
                if(Graphics.CurrentDeviceResolution.width < 1920 || Graphics.CurrentDeviceResolution.height < 1120) {
                    width = 1280;
                    height = 720;
                }
                else {
                    width = 1920;
                    height = 1080;
                }
            }
            Graphics.SetConfigFlags();
            window = SDL.CreateWindow(windowTitle, width, height, SDL.WindowFlags.OpenGL);
            if(window == IntPtr.Zero) {
                throw new RayBlastEngineException($"Failed to create window: {SDL.GetError()}");
            }
            windowCreated = true;
            renderer = SDL.CreateRenderer(window, null);

            if(renderer == IntPtr.Zero) {
                throw new RayBlastEngineException($"Failed to create OpenGL renderer: {SDL.GetError()}");
            }
            SDL.SetRenderVSync(renderer, 1);
            if(!PlayerPreferences.Loaded) {
                try {
                    PlayerPreferences.Load();
                }
                catch(Exception e) {
                    Debug.LogException(e);
                }
            }
            Debug.Log("RayBlast Engine finished initialization", false);
        }
        catch(Exception e) {
            throw;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [
        typeof(System.Runtime.CompilerServices.CallConvCdecl)
    ])]
    private static unsafe void OnLogInitializing(int msgType, sbyte* text,
                                                 sbyte* args) {
        throw new NotImplementedException();
    }

    [UnmanagedCallersOnly(CallConvs = [
        typeof(System.Runtime.CompilerServices.CallConvCdecl)
    ])]
    private static unsafe void OnLog(int msgType, sbyte* text,
                                     sbyte* args) {
        throw new NotImplementedException();
    }

    internal static void RecordLog(LogLevel msgType, string logMessage,
                                   ReadOnlySpan<char> stackTrace) {
        Debug.HandleMessageReceived(logMessage, stackTrace, msgType);
        if(logStreamWriter != null) {
            lock(logStreamWriter) {
                logStreamWriter.WriteLogLevel(msgType);
                if(!Game.IsRunning) {
                    Span<char> span = stackalloc char[16];
                    #if DEBUG
                    logStreamWriter.Write('T');
                    LOG_WATCH.ElapsedTicks.TryFormat(span, out int charsWritten);
                    logStreamWriter.Write(span[..charsWritten]);
                    #else
					logStreamWriter.Write('I');
					LOG_WATCH.ElapsedMilliseconds.TryFormat(span, out int charsWritten);
					logStreamWriter.Write(span[..charsWritten]);
                    #endif
                }
                else {
                    #if DEBUG
                    Span<char> span = stackalloc char[12];
                    WATCH.Elapsed.TotalSeconds.TryFormat(span, out int charsWritten, "0.000000");
                    logStreamWriter.Write(span[..charsWritten]);
                    #else
					Span<char> span = stackalloc char[8];
					WATCH.Elapsed.TotalSeconds.TryFormat(span, out int charsWritten, "0.000");
					logStreamWriter.Write(span[..charsWritten]);
                    #endif
                }
                logStreamWriter.Write(' ');
                logStreamWriter.Write(logMessage);
                if(stackTrace.Length > 0) {
                    logStreamWriter.WriteLine();
                    logStreamWriter.Write(stackTrace);
                }
                logStreamWriter.ResetLogLevel();
                logStreamWriter.WriteLine();
            }
        }
    }

    internal static void RecordLog(LogLevel msgType, StringBuilder logMessage,
                                   ReadOnlySpan<char> stackTrace) {
        Debug.HandleMessageReceived(logMessage, stackTrace, msgType);
        if(logStreamWriter != null) {
            lock(logStreamWriter) {
                logStreamWriter.WriteLogLevel(msgType);
                #if DEBUG
                Span<char> span = stackalloc char[12];
                WATCH.Elapsed.TotalSeconds.TryFormat(span, out int charsWritten, "0.000000");
                logStreamWriter.Write(span[..charsWritten]);
                #else
				Span<char> span = stackalloc char[8];
				WATCH.Elapsed.TotalSeconds.TryFormat(span, out int charsWritten, "0.000");
				logStreamWriter.Write(span[..charsWritten]);
                #endif
                logStreamWriter.Write(' ');
                logStreamWriter.Write(logMessage);
                if(stackTrace.Length > 0) {
                    logStreamWriter.WriteLine();
                    logStreamWriter.Write(stackTrace);
                }
                logStreamWriter.ResetLogLevel();
                logStreamWriter.WriteLine();
            }
        }
    }

    internal static void RecordLog(LogLevel msgType, ReadOnlySpan<char> logMessage,
                                   ReadOnlySpan<char> stackTrace) {
        Debug.HandleMessageReceived(logMessage, stackTrace, msgType);
        if(logStreamWriter != null) {
            lock(logStreamWriter) {
                logStreamWriter.WriteLogLevel(msgType);
                #if DEBUG
                Span<char> span = stackalloc char[12];
                WATCH.Elapsed.TotalSeconds.TryFormat(span, out int charsWritten, "0.000000");
                logStreamWriter.Write(span[..charsWritten]);
                #else
				Span<char> span = stackalloc char[8];
				WATCH.Elapsed.TotalSeconds.TryFormat(span, out int charsWritten, "0.000");
				logStreamWriter.Write(span[..charsWritten]);
                #endif
                logStreamWriter.Write(' ');
                logStreamWriter.Write(logMessage);
                if(stackTrace.Length > 0) {
                    logStreamWriter.WriteLine();
                    logStreamWriter.Write(stackTrace);
                }
                logStreamWriter.ResetLogLevel();
                logStreamWriter.WriteLine();
            }
        }
    }

    internal static void RecordLog(LogLevel msgType, Utf8ValueStringBuilder builder,
                                   ReadOnlySpan<char> stackTrace) {
        Debug.HandleMessageReceived(builder, stackTrace, msgType);
        if(logStreamWriter != null) {
            lock(logStreamWriter) {
                using Utf16ValueStringBuilder buffer = ZString.CreateStringBuilder();
                buffer.Append(builder);
                logStreamWriter.WriteLogLevel(msgType);
                #if DEBUG
                Span<char> span = stackalloc char[12];
                WATCH.Elapsed.TotalSeconds.TryFormat(span, out int charsWritten, "0.000000");
                logStreamWriter.Write(span[..charsWritten]);
                #else
				Span<char> span = stackalloc char[8];
				WATCH.Elapsed.TotalSeconds.TryFormat(span, out int charsWritten, "0.000");
				logStreamWriter.Write(span[..charsWritten]);
                #endif
                logStreamWriter.Write(' ');
                logStreamWriter.Write(buffer.AsSpan());
                if(stackTrace.Length > 0) {
                    logStreamWriter.WriteLine();
                    logStreamWriter.Write(stackTrace);
                }
                logStreamWriter.ResetLogLevel();
                logStreamWriter.WriteLine();
            }
        }
    }

    public static void Run(RayBlastMethod update, RayBlastMethod render) {
        if(logStreamWriter == null)
            throw new RayBlastEngineException("Run was called before Initialize");
        WATCH.Restart();
        updateMethod = update;
        renderMethod = render;
        UnmanagedManager.mainThreadId = Environment.CurrentManagedThreadId;
        // if(Graphics.VSyncCount > 0)
        //     Raylib.SetWindowState(ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
        // else
        //     Raylib.SetWindowState(ConfigFlags.ResizableWindow);
        Game.TargetFrameRate = 0;
        canContinue = 1;
        Input.Initialize();
        Game.IsRunning = true;
        try {
            lock(logStreamWriter) {
                logStreamWriter.criticalErrors = false;
            }
            while(canContinue > 0) {
                UnmanagedManager.RecoverResources();
                if(PlayerPreferences.hasChanges) {
                    try {
                        PlayerPreferences.Save();
                    }
                    catch(Exception e) {
                        Debug.LogException(e);
                    }
                }
                // if(Graphics.VSyncCount > 0)
                //     Raylib.SetWindowState(ConfigFlags.VSyncHint);
                // else
                //     Raylib.ClearWindowState(ConfigFlags.VSyncHint);
                lock(logStreamWriter) {
                    logStreamWriter.FlushNow();
                }
                //TODO: Allow skipping render calls with a softcoded target framerate
                try {
                    Time.renderedFrameCount++;
                    SDL.SetRenderVSync(renderer, Graphics.VSyncCount);
                    var renderStart = Stopwatch.GetTimestamp();
                    renderMethod();
                    Time.accumulatedRenderTime += (Stopwatch.GetTimestamp() - renderStart) / (double)TimeSpan.TicksPerSecond;
                }
                catch(Exception e) {
                    // Raylib.DrawText("RENDER FAILURE", 1, 1, 16, Color.Black);
                    // Raylib.DrawText("RENDER FAILURE", 0, 0, 16, Color.White);
                    Debug.LogException(e);
                }
                finally {
                    SDL.RenderPresent(renderer);
                }
                FRAME_WATCH.Restart();
                Time.frameCount++;
                double newTime = RealtimeSinceStartup;
                Time.unscaledDeltaTime = newTime - Time.unscaledTime;
                Time.unscaledTime = newTime;
                Time.time += Time.deltaTime;
                Time.deltaTime = Time.unscaledDeltaTime * Time.timeScale;
                BankPlayer.MAIN.Update();
                PollEvents();
                if(Input.MouseVisible)
                    SDL.ShowCursor();
                else
                    SDL.HideCursor();
                Input.UpdateFrame();
                SoundEffect2D.UpdateAll();
                //TODO: Calculate target deltaTime somewhere
                // float frameTime = Math.Max(Raylib.GetFrameTime(), 0.01f);
                // Game.requestedClose |= Raylib.WindowShouldClose();
                if(DigitalSoundProcessing.pendingReset)
                    DigitalSoundProcessing.Initialize();
                GameUpdate();
            }
            Game.IsRunning = false;
            DigitalSoundProcessing.Shutdown();
        }
        catch(Exception e) {
            lock(logStreamWriter) {
                logStreamWriter.criticalErrors = true;
                logStreamWriter.FlushNow();
            }
            Game.IsRunning = false;
            Debug.LogError("UNHANDLED ERROR!!", false);
            Debug.LogException(e);
            if(windowIsAvailable) {
                windowIsAvailable = false;
                try {
                    DigitalSoundProcessing.Shutdown();
                    // Raylib.CloseAudioDevice();
                    string[] errorLines = e.ToString().Split('\n');
                    // for(int i = 0; i < 2; i++) {
                    //     Raylib.BeginDrawing();
                    //     Raylib.ClearBackground(new Raylib_cs.Color(0, 0, 0, 128));
                    //     Raylib.DrawText("UNHANDLED FATAL ERROR, press Ctrl+C to copy", 5, 5, 20, new Raylib_cs.Color(255, 255, 255));
                    //     int y = 30;
                    //     foreach(string line in errorLines) {
                    //         Raylib.DrawText(line.TrimEnd(), 5, y, 20, new Raylib_cs.Color(255, 255, 255));
                    //         y += 20;
                    //     }
                    //     Raylib.EndDrawing();
                    // }
                    // Raylib.SetWindowState(ConfigFlags.VSyncHint);
                    // while(!Raylib.WindowShouldClose()) {
                    //     if((Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl))
                    //     && Raylib.IsKeyPressed(KeyboardKey.C)) {
                    //         Raylib.SetClipboardText(e.ToString());
                    //         Debug.Log("Copied", includeStackTrace: false);
                    //     }
                    //     Raylib.BeginDrawing();
                    //     Raylib.EndDrawing();
                    // }
                }
                catch(Exception e2) {
                    Debug.LogException(e2);
                }
            }
            throw;
        }
        finally {
            // Raylib.CloseWindow();
            lock(logStreamWriter) {
                RayBlastLogStreamWriter streamWriter = logStreamWriter;
                logStreamWriter = null;
                streamWriter.Write("RayBlast Engine has shut down");
                streamWriter.FlushNow();
                Thread.Sleep(10);
                streamWriter.Close();
            }
        }
    }

    public static void PollEvents() {
        while(SDL.PollEvent(out SDL.Event sdlEvent)) {
            switch((SDL.EventType)sdlEvent.Type) {
            case SDL.EventType.WindowCloseRequested:
                Game.requestedClose = true;
                break;
            case SDL.EventType.JoystickButtonDown:
                Debug.LogDebug($"JButton {sdlEvent.JButton.Which}");
                break;
            case SDL.EventType.JoystickAxisMotion:
                Debug.LogDebug($"JAxis {sdlEvent.JAxis.Which}");
                break;
            case SDL.EventType.KeyDown:
            case SDL.EventType.KeyUp:
                try {
                    Key key = sdlEvent.Key.Key.ToRayBlast();
                    if(key != Key.Null) {
                        Input.QUEUED_EVENTS.Enqueue(new InputEvent {
                            key = key, state = (byte)(sdlEvent.Key.Down ? 1 : 0), time = sdlEvent.Key.Timestamp / 1e9
                        });
                    }
                }
                catch(ArgumentOutOfRangeException) {
                    Debug.LogWarning($"RayBlast Engine received unknown keycode {sdlEvent.Key.Key}");
                }
                break;
            case SDL.EventType.MouseMotion:
                Input.capturedMousePosition = new Vector2(sdlEvent.Motion.X, sdlEvent.Motion.Y);
                break;
            case SDL.EventType.MouseButtonDown:
            case SDL.EventType.MouseButtonUp:
                Input.capturedMousePosition = new Vector2(sdlEvent.Button.X, sdlEvent.Button.Y);
                try {
                    MouseCode mouseCode = Utils.SDLToRayBlastMouseCode(sdlEvent.Button.Button);
                    if(mouseCode != MouseCode.Null) {
                        Input.QUEUED_EVENTS.Enqueue(new InputEvent {
                            mouseCode = mouseCode, state = (byte)(sdlEvent.Button.Down ? 1 : 0), time = sdlEvent.Button.Timestamp / 1e9
                        });
                    }
                }
                catch(ArgumentOutOfRangeException) {
                    Debug.LogWarning($"RayBlast Engine received unknown mousecode {sdlEvent.Key.Key}");
                }
                break;
            case SDL.EventType.MouseWheel:
                Input.capturedMouseScrollAmount += new Vector2(sdlEvent.Wheel.X, sdlEvent.Wheel.Y);
                Input.capturedMousePosition = new Vector2(sdlEvent.Wheel.X, sdlEvent.Wheel.Y);
                break;
            case SDL.EventType.WindowFocusLost:
                IsFocused = false;
                break;
            case SDL.EventType.WindowFocusGained:
                IsFocused = true;
                break;
            }
        }
    }

    //TODO: Add the ability to disable async update
    private static void GameUpdate() {
        try {
            var updateStart = Stopwatch.GetTimestamp();
            updateMethod?.Invoke();
            Time.accumulatedUpdateTime += (Stopwatch.GetTimestamp() - updateStart) / (double)TimeSpan.TicksPerSecond;
        }
        catch(Exception e) {
            Debug.LogException(e);
        }
    }

    public static void StopApplication() {
        canContinue = 0;
    }

    public static Uri CreateResourceUri(string resourceUri) {
        return new Uri(Path.Combine(Environment.CurrentDirectory, "Resources", resourceUri));
    }

    public static void AddDebugGraphics() {
        S32X2 currentResolution = Graphics.CurrentResolution;
        DigitalSoundProcessing.AddDebugGraphics(currentResolution.X - 4, currentResolution.Y - 4);
    }

    public static int WebGLUpdate() {
        if(!Game.IsRunning)
            return 0;
        if(logStreamWriter == null) {
            Debug.LogError("Update was called before Initialize");
            return 1;
        }
        UnmanagedManager.mainThreadId = Environment.CurrentManagedThreadId;
        canContinue = 1;
        try {
            lock(logStreamWriter) {
                logStreamWriter.criticalErrors = false;
            }
            UnmanagedManager.RecoverResources();
            if(PlayerPreferences.hasChanges) {
                try {
                    PlayerPreferences.Save();
                }
                catch(Exception e) {
                    Debug.LogException(e);
                }
            }
            // if(Graphics.VSyncCount > 0)
            //     Raylib.SetWindowState(ConfigFlags.VSyncHint);
            // else
            //     Raylib.ClearWindowState(ConfigFlags.VSyncHint);
            lock(logStreamWriter) {
                logStreamWriter.FlushNow();
            }
            //TODO: Allow skipping render calls with a softcoded target framerate
            // Raylib.BeginDrawing();
            try {
                Time.renderedFrameCount++;
                renderMethod?.Invoke();
            }
            catch(Exception e) {
                // Raylib.DrawText("RENDER FAILURE", 1, 1, 16, Color.Black);
                // Raylib.DrawText("RENDER FAILURE", 0, 0, 16, Color.White);
                Debug.LogException(e);
            }
            FRAME_WATCH.Restart();
            Time.frameCount++;
            double newTime = RealtimeSinceStartup;
            Time.unscaledDeltaTime = newTime - Time.unscaledTime;
            Time.unscaledTime = newTime;
            Time.time += Time.deltaTime;
            Time.deltaTime = Time.unscaledDeltaTime * Time.timeScale;
            BankPlayer.MAIN.Update();
            Input.UpdateFrame();
            SoundEffect2D.UpdateAll();
            //TODO: Calculate target deltaTime somewhere
            // float frameTime = Math.Max(Raylib.GetFrameTime(), 0.01f);
            // Game.requestedClose |= Raylib.WindowShouldClose();
            if(DigitalSoundProcessing.pendingReset)
                DigitalSoundProcessing.Initialize();
            GameUpdate();
            return 0;
        }
        catch(Exception e) {
            lock(logStreamWriter) {
                logStreamWriter.criticalErrors = true;
                logStreamWriter.FlushNow();
            }
            Game.IsRunning = false;
            Debug.LogError("UNHANDLED ERROR!!", false);
            Debug.LogException(e);
            if(windowIsAvailable) {
                windowIsAvailable = false;
                try {
                    DigitalSoundProcessing.Shutdown();
                    // Raylib.CloseAudioDevice();
                    string[] errorLines = e.ToString().Split('\n');
                    // for(int i = 0; i < 2; i++) {
                    //     Raylib.BeginDrawing();
                    //     Raylib.ClearBackground(new Raylib_cs.Color(0, 0, 0, 128));
                    //     Raylib.DrawText("UNHANDLED FATAL ERROR", 5, 5, 20, new Raylib_cs.Color(255, 255, 255));
                    //     int y = 30;
                    //     foreach(string line in errorLines) {
                    //         Raylib.DrawText(line.TrimEnd(), 5, y, 20, new Raylib_cs.Color(255, 255, 255));
                    //         y += 20;
                    //     }
                    //     Raylib.EndDrawing();
                    // }
                }
                catch(Exception e2) {
                    Debug.LogException(e2);
                }
            }
            lock(logStreamWriter) {
                RayBlastLogStreamWriter streamWriter = logStreamWriter;
                logStreamWriter = null;
                streamWriter.Write("RayBlast Engine has shut down");
                streamWriter.FlushNow();
                Thread.Sleep(10);
                streamWriter.Close();
            }
            return 1;
        }
    }

    public static void SetTargetFPS(float fps) {
        targetFPS = fps;
    }
}
