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
	private static readonly string POLL_LOCK = "poll";

	private static readonly Stopwatch FRAME_WATCH = new();
	private static readonly List<WindowPane> WINDOW_PANES = [];

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
	internal static WindowPane currentWindowPane;

	public static bool VulkanSupported { get; internal set; }
	public static bool OpenGL11Supported { get; internal set; }
	public static bool OpenGL21Supported { get; internal set; }
	public static bool OpenGL33Supported { get; internal set; }
	public static bool OpenGL43Supported { get; internal set; }
	public static bool OpenGLES2Supported { get; internal set; }
	public static bool UsingVulkan { get; internal set; }
	public static bool UsingOpenGL { get; internal set; }
	public static DirectoryInfo CurrentDirectoryInfo { get; } = new(Environment.CurrentDirectory);
	public static DirectoryInfo LocalAppDataDirectoryInfo { get; private set; } =
		new(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
	public static DirectoryInfo RoamingAppDataDirectoryInfo { get; private set; } =
		new(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
	public static (bool, WindowPane) UserRequestedClose {
		get {
			(bool, WindowPane) returnValue = (Game.requestedClose, Game.closePane);
			Game.requestedClose = false;
			Game.closePane = new WindowPane();
			return returnValue;
		}
	}
	public static bool IsMouseInWindow => Input.mouseHoverPaneID == currentWindowPane.id;
	public static bool IsFocused => Input.focusedPaneID == currentWindowPane.id;

	public static string WindowTitle {
		get;
		set {
			field = value;
			if(currentWindowPane.window != IntPtr.Zero)
				SDL.SetWindowTitle(currentWindowPane.window, value);
		}
	} = "";

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
			ThreadCount = Environment.ProcessorCount;
			int coreCount = SDL.GetNumLogicalCPUCores();
			CPUCacheLine = SDL.GetCPUCacheLineSize();
			SystemMemory = SDL.GetSystemRAM();
			Debug.Log(
				$"System processor = {Debug.SystemProcessorName} ({ThreadCount} threads, {coreCount} logical cores, {CPUCacheLine} KB L1 cache)");
			Debug.Log($"System memory = {SystemMemory} MiB");
			ThreadPool.GetMinThreads(out int minThreads, out _);
			bool threadPoolSuccess = ThreadPool.SetMaxThreads(Math.Max(ThreadCount - 1, minThreads), 32767);
			if(!threadPoolSuccess)
				Debug.LogWarning("Failed to limit ThreadPool thread count");
			PlayerPreferences.Load();
			// if(!SDL.SetHint(SDL.Hints.VideoDriver, "x11"))
			// 	throw new RayBlastEngineException($"Failed to set video driver hint: {SDL.GetError()}");
			if(!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Gamepad | SDL.InitFlags.Haptic))
				throw new RayBlastEngineException($"SDL3 failed to initialize: {SDL.GetError()}");
			if(!TTF.Init())
				throw new RayBlastEngineException($"SDL_ttf failed to initialize: {SDL.GetError()}");
			preinitialized = true;
			Debug.Log($"Video driver name = {SDL.GetCurrentVideoDriver()}");
			Debug.Log("Preinitialization complete");
		}
		catch(Exception e) {
			SDL.ShowSimpleMessageBox(SDL.MessageBoxFlags.Error, "RayBlast Engine", e.ToString(), IntPtr.Zero);
			throw;
		}
	}

	public static int ThreadCount { get; private set; }
	public static int CPUCacheLine { get; private set; }
	public static int SystemMemory { get; private set; }

	public static void Initialize(int width = 0, int height = 0) {
		if(!preinitialized)
			Preinitialize();
		var windowCreated = false;
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
			int renderDriverCount = SDL.GetNumRenderDrivers();
			List<string?> driverNames = [];
			for(var i = 0; i < renderDriverCount; i++) {
				string? renderDriverName = SDL.GetRenderDriver(i);
				driverNames.Add(renderDriverName);
				switch(renderDriverName) {
				case "vulkan":
					VulkanSupported = true;
					break;
				case "opengl":
					OpenGL11Supported = true;
					break;
				case "opengles2":
					OpenGLES2Supported = true;
					break;
				}
			}
			Debug.Log($"Render drivers available: [{driverNames.Join(", ")}]");
			IntPtr window = IntPtr.Zero;
			#if RAYBLAST_DEBUG
			if(VulkanSupported && PseudoRNG.Bool) {
				#else
            if(VulkanSupported) {
				#endif
				Debug.Log("Using Vulkan...");
				window = SDL.CreateWindow(WindowTitle, width, height, SDL.WindowFlags.Vulkan | SDL.WindowFlags.Resizable);
				#if RAYBLAST_DEBUG
				if(window == IntPtr.Zero) {
					throw new RayBlastEngineException($"Failed to create Vulkan window: {SDL.GetError()}");
				}
				#endif
			}
			UsingVulkan = window != IntPtr.Zero;
			if(window == IntPtr.Zero) {
				#if RAYBLAST_DEBUG
				Debug.Log("Using OpenGL...");
				#else
                Debug.LogWarning("Vulkan unavailable, trying OpenGL...");
				#endif
				UsingVulkan = false;
				window = SDL.CreateWindow(WindowTitle, width, height, SDL.WindowFlags.OpenGL | SDL.WindowFlags.Resizable);
				if(window == IntPtr.Zero) {
					throw new RayBlastEngineException($"Failed to create window: {SDL.GetError()}");
				}
				UsingOpenGL = true;
			}
			windowCreated = true;
			IntPtr renderer = SDL.CreateRenderer(window, UsingVulkan ? "vulkan" : null);
			SDL.SetRenderDrawBlendMode(renderer, Graphics.BLEND_WITH_PREMULTIPLY_ALPHA);

			if(renderer == IntPtr.Zero) {
				throw new RayBlastEngineException($"Failed to create renderer: {SDL.GetError()}");
			}
			Debug.Log($"Renderer name = {SDL.GetRendererName(renderer)}");
			Debug.Log($"Window flags = {SDL.GetWindowFlags(window)}");
			SDL.SetRenderVSync(renderer, 1);
			uint windowID = SDL.GetWindowID(window);
			if(windowID == 0)
				throw new RayBlastEngineException($"Failed to get main window ID: {SDL.GetError()}");
			currentWindowPane = new WindowPane(windowID, window, renderer);
			WINDOW_PANES.Add(currentWindowPane);
			if(UsingVulkan)
				RayBlastCompute.Initialize();
			if(!PlayerPreferences.Loaded) {
				try {
					PlayerPreferences.Load();
				}
				catch(Exception e) {
					Debug.LogException(e);
				}
			}
			//TODO: Find out what consequences there are to turning on text input events
			Input.StartListeningToTextInput();
			Debug.Log("RayBlast Engine finished initialization");
		}
		catch(Exception e) {
			SDL.ShowSimpleMessageBox(SDL.MessageBoxFlags.Error, WindowTitle, e.ToString(), IntPtr.Zero);
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

	internal static void RecordLog(LogLevel msgType, ReadOnlySpan<char> logMessage,
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

	internal static void RecordLog(LogLevel msgType, Utf8ValueStringBuilder builder,
								   ReadOnlySpan<char> stackTrace) {
		Debug.HandleMessageReceived(builder, stackTrace, msgType);
		if(logStreamWriter != null) {
			lock(logStreamWriter) {
				using Utf16ValueStringBuilder buffer = ZString.CreateStringBuilder();
				buffer.Append(builder);
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
					foreach(WindowPane pane in WINDOW_PANES) {
						SDL.SetRenderVSync(pane.renderer, Graphics.VSyncCount);
					}
					Time.renderedFrameCount++;
					long renderStart = Stopwatch.GetTimestamp();
					SelectMainWindow();
					renderMethod();
					Time.accumulatedRenderTime += (Stopwatch.GetTimestamp() - renderStart) / (double)TimeSpan.TicksPerSecond;
				}
				catch(Exception e) {
					// Raylib.DrawText("RENDER FAILURE", 1, 1, 16, Color.Black);
					// Raylib.DrawText("RENDER FAILURE", 0, 0, 16, Color.White);
					Debug.LogException(e);
				}
				finally {
					foreach(WindowPane pane in WINDOW_PANES) {
						SDL.RenderPresent(pane.renderer);
					}
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
		lock(POLL_LOCK) {
			while(SDL.PollEvent(out SDL.Event sdlEvent)) {
				switch((SDL.EventType)sdlEvent.Type) {
				case SDL.EventType.WindowCloseRequested:
					Game.requestedClose = true;
					Game.closePane = new WindowPane();
					IntPtr windowPtr = SDL.GetWindowFromID(sdlEvent.Window.WindowID);
					foreach(WindowPane pane in WINDOW_PANES) {
						if(windowPtr == pane.window) {
							Game.closePane = pane;
							break;
						}
					}
					break;
				case SDL.EventType.JoystickAdded:
					Debug.LogDebug($"Joystick added {sdlEvent.JDevice.Which}: {Input.GetJoystickName(sdlEvent.JDevice.Which)}", false);
					break;
				case SDL.EventType.JoystickRemoved:
					Debug.LogDebug($"Joystick removed {sdlEvent.JDevice.Which}", false);
					Input.RemoveJoystick(sdlEvent.JDevice.Which);
					break;
				case SDL.EventType.JoystickButtonDown:
				case SDL.EventType.JoystickButtonUp:
					Input.QUEUED_EVENTS.Enqueue(InputType.JoystickButton);
					var newButtonEvent = new InputJoystickButtonEvent(sdlEvent.JButton.Which, sdlEvent.JButton.Button,
																	  sdlEvent.JButton.Down,
																	  sdlEvent.JButton.Timestamp / 1e9);
					Input.QUEUED_JOYSTICK_BUTTON_EVENTS.Enqueue(newButtonEvent);
					// Debug.LogDebug(newButtonEvent.ToString(), includeStackTrace: false);
					break;
				case SDL.EventType.JoystickAxisMotion:
					Input.QUEUED_EVENTS.Enqueue(InputType.JoystickAxis);
					var newAxisEvent = new InputJoystickAxisEvent(sdlEvent.JAxis.Which, sdlEvent.JAxis.Axis, sdlEvent.JAxis.Value,
																  sdlEvent.JAxis.Timestamp / 1e9);
					Input.QUEUED_JOYSTICK_AXIS_EVENTS.Enqueue(newAxisEvent);
					// Debug.LogDebug(newAxisEvent.ToString(), includeStackTrace: false);
					break;
				case SDL.EventType.JoystickHatMotion:
					Input.QUEUED_EVENTS.Enqueue(InputType.JoystickHat);
					var newHatEvent = new InputJoystickHatEvent(sdlEvent.JHat.Which, sdlEvent.JHat.Hat, (HatState)sdlEvent.JHat.Value,
																sdlEvent.Key.Timestamp / 1e9);
					Input.QUEUED_JOYSTICK_HAT_EVENTS.Enqueue(newHatEvent);
					// Debug.LogDebug(newHatEvent.ToString(), includeStackTrace: false);
					break;
				case SDL.EventType.KeyDown:
				case SDL.EventType.KeyUp:
					try {
						Key key = sdlEvent.Key.Key.ToRayBlast();
						if(key != Key.Null) {
							Input.QUEUED_EVENTS.Enqueue(InputType.Keyboard);
							var newKeyEvent = new InputKeyEvent(key, sdlEvent.Key.Down, sdlEvent.Key.Timestamp / 1e9, sdlEvent.Key.WindowID);
							Input.QUEUED_KEY_EVENTS.Enqueue(newKeyEvent);
							if(Input.listeningToTextEvents && sdlEvent.Key.Down) {
								switch(key) {
								case Key.Backspace:
									Input.CAPTURED_INPUT_STRING_BUILDER.Append('\b');
									break;
								case Key.Enter:
								case Key.KeypadEnter:
									Input.CAPTURED_INPUT_STRING_BUILDER.Append('\n');
									break;
								case Key.Tab:
									Input.CAPTURED_INPUT_STRING_BUILDER.Append('\t');
									break;
								case Key.Escape:
									Input.CAPTURED_INPUT_STRING_BUILDER.Append('\e');
									break;
								}
							}
						}
					}
					catch(ArgumentOutOfRangeException) {
						Debug.LogWarning($"RayBlast Engine received unknown keycode {sdlEvent.Key.Key}");
					}
					break;
				case SDL.EventType.MouseMotion:
					Input.CAPTURED_MOUSE_POSITIONS[sdlEvent.Motion.WindowID] = new Vector2(sdlEvent.Motion.X, sdlEvent.Motion.Y);
					break;
				case SDL.EventType.WindowMouseLeave:
					if(Input.mouseHoverPaneID == sdlEvent.Window.WindowID)
						Input.mouseHoverPaneID = 0;
					break;
				case SDL.EventType.WindowMouseEnter:
					Input.mouseHoverPaneID = sdlEvent.Window.WindowID;
					break;
				case SDL.EventType.WindowFocusLost:
					if(Input.focusedPaneID == sdlEvent.Window.WindowID)
						Input.focusedPaneID = 0;
					break;
				case SDL.EventType.WindowFocusGained:
					Input.focusedPaneID = sdlEvent.Window.WindowID;
					break;
				case SDL.EventType.MouseButtonDown:
				case SDL.EventType.MouseButtonUp:
					Input.CAPTURED_MOUSE_POSITIONS[sdlEvent.Button.WindowID] = new Vector2(sdlEvent.Button.X, sdlEvent.Button.Y);
					try {
						MouseCode mouseCode = Utils.SDLToRayBlastMouseCode(sdlEvent.Button.Button);
						if(mouseCode != MouseCode.Null) {
							Input.QUEUED_EVENTS.Enqueue(InputType.Mouse);
							var newMouseEvent = new InputMouseEvent(mouseCode, sdlEvent.Button.Down, sdlEvent.Button.X, sdlEvent.Button.Y,
																	sdlEvent.Button.Timestamp / 1e9, sdlEvent.Button.WindowID);
							Input.QUEUED_MOUSE_EVENTS.Enqueue(newMouseEvent);
						}
					}
					catch(ArgumentOutOfRangeException) {
						Debug.LogWarning($"RayBlast Engine received unknown mousecode {sdlEvent.Key.Key}");
					}
					break;
				case SDL.EventType.MouseWheel:
					if(Input.CAPTURED_MOUSE_SCROLL_AMOUNTS.ContainsKey(sdlEvent.Wheel.WindowID))
						Input.CAPTURED_MOUSE_SCROLL_AMOUNTS[sdlEvent.Wheel.WindowID] += new Vector2(sdlEvent.Wheel.X, sdlEvent.Wheel.Y);
					else
						Input.CAPTURED_MOUSE_SCROLL_AMOUNTS[sdlEvent.Wheel.WindowID] = new Vector2(sdlEvent.Wheel.X, sdlEvent.Wheel.Y);
					break;
				case SDL.EventType.TextInput:
					nint ptr = sdlEvent.Text.Text;
					if(ptr != 0) {
						byte* p = (byte*)ptr;
						int len = 0;
						while(p[len] != 0) {
							len++;
						}
						Input.CAPTURED_INPUT_STRING_BUILDER.Append(Encoding.UTF8.GetString(new ReadOnlySpan<byte>(p, len)));
					}
					break;
				}
			}
		}
	}

	//TODO: Add the ability to disable async update
	private static void GameUpdate() {
		try {
			long updateStart = Stopwatch.GetTimestamp();
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

	public static WindowPane CreateWindowPane(string name) {
		S32X2 resolution = Graphics.CurrentResolution;
		bool success = SDL.CreateWindowAndRenderer(name, resolution.X, resolution.Y, SDL.WindowFlags.OpenGL, out IntPtr window, out IntPtr renderer);
		if(!success)
			throw new RayBlastEngineException($"Failed to create window pane: {SDL.GetError()}");
		SDL.SetRenderDrawBlendMode(renderer, Graphics.BLEND_WITH_PREMULTIPLY_ALPHA);
		uint windowID = SDL.GetWindowID(window);
		if(windowID == 0)
			throw new RayBlastEngineException($"Failed to get window ID from window pane: {SDL.GetError()}");
		var windowPane = new WindowPane(windowID, window, renderer);
		WINDOW_PANES.Add(windowPane);
		return windowPane;
	}

	public static void DestroyWindowPane(WindowPane windowPane) {
		SDL.DestroyRenderer(windowPane.renderer);
		SDL.DestroyWindow(windowPane.window);
		WINDOW_PANES.Remove(windowPane);
	}

	public static void SelectMainWindow() {
		SelectWindowPane(WINDOW_PANES[0]);
	}

	public static void SelectWindowPane(WindowPane pane) {
		if(pane.window == IntPtr.Zero || pane.renderer == IntPtr.Zero)
			throw new RayBlastEngineException("Invalid window pane");
		currentWindowPane = pane;
	}

	public static void FocusWindow() {
		SDL.RaiseWindow(currentWindowPane.window);
	}
}
