using System.Collections.Concurrent;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Text;
using SDL3;

namespace RayBlast;

//TODO: Record key press lengths and see if it can actually take advantage of 1000 Hz keyboards
public static partial class Input {
    //TODO: Push pending history to InputListeners rather than wait for them to poll
    internal static readonly Dictionary<Key, List<InputKeyEvent>> KEY_HISTORY = new();
    internal static readonly Dictionary<MouseCode, List<InputMouseEvent>> MOUSE_HISTORY = new();
    internal static readonly Dictionary<uint, JoystickHistory> JOYSTICK_HISTORY = new();
    internal static readonly Dictionary<uint, IntPtr> OPEN_JOYSTICKS = new();
    internal static readonly Dictionary<uint, IntPtr> OPEN_GAMEPADS = new();

    private static readonly List<Key> ALL_KEYS = [];
    private static readonly List<MouseCode> ALL_MOUSE_CODES = [];
    private static readonly Dictionary<Key, int> KEY_EVENT_INDEXES = new();
    private static readonly Dictionary<Key, bool> ALL_KEY_STATES = new();
    private static readonly Dictionary<Key, bool> ALL_KEY_PRESSES = new();
    private static readonly Dictionary<Key, bool> ALL_KEY_RELEASES = new();
    private static readonly Dictionary<MouseCode, int> MOUSE_EVENT_INDEXES = new();
    private static readonly Dictionary<MouseCode, bool> ALL_MOUSE_STATES = new();
    private static readonly Dictionary<MouseCode, bool> ALL_MOUSE_PRESSES = new();
    private static readonly Dictionary<MouseCode, bool> ALL_MOUSE_RELEASES = new();
    internal static readonly ConcurrentQueue<InputType> QUEUED_EVENTS = new();
    internal static readonly ConcurrentQueue<InputKeyEvent> QUEUED_KEY_EVENTS = new();
    internal static readonly ConcurrentQueue<InputMouseEvent> QUEUED_MOUSE_EVENTS = new();
    internal static readonly ConcurrentQueue<InputJoystickButtonEvent> QUEUED_JOYSTICK_BUTTON_EVENTS = new();
    internal static readonly ConcurrentQueue<InputJoystickAxisEvent> QUEUED_JOYSTICK_AXIS_EVENTS = new();
    internal static readonly ConcurrentQueue<InputJoystickHatEvent> QUEUED_JOYSTICK_HAT_EVENTS = new();

    public static int pollsPerformed;

    private static int pollSleepInterval = 1;
    private static long startTick;
    private static string inputStringOfThisFrame = "";
    internal static Vector2 capturedMousePosition;
    internal static Vector2 capturedMouseScrollAmount;

    public static event InputMouseEventHandler? OnMouseEvent;
    public static event InputKeyEventHandler? OnKeyboardEvent;
    public static event InputJoystickButtonEventHandler? OnJoystickButtonEvent;
    public static event InputJoystickAxisEventHandler? OnJoystickAxisEvent;
    public static event InputJoystickHatEventHandler? OnJoystickHatEvent;

    public static double PollRate => pollsPerformed / ((RayBlastEngine.WATCH.ElapsedTicks - startTick) / (double)TimeSpan.TicksPerSecond);

    public static int PollSleepInterval {
        get => pollSleepInterval;
        set {
            pollSleepInterval = value;
            pollsPerformed = 0;
            startTick = RayBlastEngine.WATCH.ElapsedTicks;
        }
    }

    public static bool IsCapsLockOn => (SDL.GetModState() & SDL.Keymod.Caps) != 0;
    public static ushort ModState => (ushort)SDL.GetModState();
    public static float ScrollAmount { get; internal set; }
    public static Vector2 MouseViewport { get; internal set; }
    public static Vector2 MousePosition { get; internal set; }
    public static bool MouseVisible { get; set; } = true;
    public static IEnumerable<uint> JoysticksConnected {
        get {
            RefreshJoysticks();
            return OPEN_JOYSTICKS.Keys;
        }
    }
    public static IEnumerable<uint> GamepadsConnected {
        get {
            RefreshJoysticks();
            return OPEN_GAMEPADS.Keys;
        }
    }

    internal static void Initialize() {
        Key[] allKeys = Enum.GetValues<Key>();
        ALL_KEYS.EnsureCapacity(allKeys.Length - 1);
        foreach(Key i in allKeys) {
            if(i != 0) {
                ALL_KEYS.Add(i);
                KEY_HISTORY[i] = new List<InputKeyEvent>(256) {
                    new(i, false, 0.0)
                };
                KEY_EVENT_INDEXES[i] = 1;
                ALL_KEY_STATES[i] = false;
                ALL_KEY_PRESSES[i] = false;
                ALL_KEY_RELEASES[i] = false;
            }
        }
        MouseCode[] allMouseCodes = Enum.GetValues<MouseCode>();
        ALL_MOUSE_CODES.EnsureCapacity(allMouseCodes.Length - 1);
        foreach(MouseCode i in allMouseCodes) {
            if(i != 0) {
                ALL_MOUSE_CODES.Add(i);
                MOUSE_HISTORY[i] = new List<InputMouseEvent>(256) {
                    new(i, false, 0f, 0f, 0.0)
                };
                MOUSE_EVENT_INDEXES[i] = 1;
                ALL_MOUSE_STATES[i] = false;
                ALL_MOUSE_PRESSES[i] = false;
                ALL_MOUSE_RELEASES[i] = false;
            }
        }
    }

    internal static void RemoveJoystick(uint joystickID) {
        OPEN_GAMEPADS.Remove(joystickID);
        OPEN_JOYSTICKS.Remove(joystickID);
        //TODO: Remove dangling lists
    }

    internal static void UpdateFrame() {
        PollEvents();
        ScrollAmount = capturedMouseScrollAmount.Y;
        MousePosition = capturedMousePosition;
        MouseViewport = new Vector2(capturedMousePosition.X / Graphics.CurrentResolution.X, capturedMousePosition.Y / Graphics.CurrentResolution.Y);
        capturedMouseScrollAmount = Vector2.Zero;
        foreach(Key k in ALL_KEYS) {
            int lastIndex = KEY_EVENT_INDEXES[k];
            var pressed = false;
            var released = false;
            List<InputKeyEvent> inputEvents = KEY_HISTORY[k];
            for(int i = lastIndex; i < inputEvents.Count; i++) {
                if(inputEvents[i].IsDown)
                    pressed = true;
                else
                    released = true;
            }
            ALL_KEY_STATES[k] = inputEvents.Last().IsDown;
            ALL_KEY_PRESSES[k] = pressed;
            ALL_KEY_RELEASES[k] = released;
            KEY_EVENT_INDEXES[k] = inputEvents.Count;
        }
        foreach(MouseCode m in ALL_MOUSE_CODES) {
            int lastIndex = MOUSE_EVENT_INDEXES[m];
            var pressed = false;
            var released = false;
            List<InputMouseEvent> inputEvents = MOUSE_HISTORY[m];
            for(int i = lastIndex; i < inputEvents.Count; i++) {
                if(inputEvents[i].IsDown)
                    pressed = true;
                else
                    released = true;
            }
            ALL_MOUSE_STATES[m] = inputEvents.Last().IsDown;
            ALL_MOUSE_PRESSES[m] = pressed;
            ALL_MOUSE_RELEASES[m] = released;
            MOUSE_EVENT_INDEXES[m] = inputEvents.Count;
        }
        foreach(uint joystickID in OPEN_JOYSTICKS.Keys) {
            if(!JOYSTICK_HISTORY.TryGetValue(joystickID, out JoystickHistory? history))
                JOYSTICK_HISTORY[joystickID] = history = new JoystickHistory();
            history.FlushButtonStates();
        }

        using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();
        //TODO_URGENT: Create inputStringOfThisFrame
        // PollEvents_ConcatenateCharacters:
        // int charPressed = Raylib.GetCharPressed();
        // if(charPressed != 0) {
        // 	builder.Append((char)charPressed);
        // 	goto PollEvents_ConcatenateCharacters;
        // }
        // if(IsKeyPressed(Key.Backspace))
        // 	builder.Append('\b');
        // if(Raylib.IsKeyPressedRepeat(KeyboardKey.Backspace))
        // 	builder.Append('\b');
        // if(IsKeyPressed(Key.Enter))
        // 	builder.Append('\n');
        // if(Raylib.IsKeyPressedRepeat(KeyboardKey.Enter))
        // 	builder.Append('\n');
        // if(IsKeyPressed(Key.KeypadEnter))
        // 	builder.Append('\n');
        // if(Raylib.IsKeyPressedRepeat(KeyboardKey.KpEnter))
        // 	builder.Append('\n');
        inputStringOfThisFrame = builder.ToString();
    }

    public static void PollEvents() {
        //TODO_URGENT: Sort by time
        while(QUEUED_EVENTS.TryDequeue(out InputType type)) {
            switch(type) {
            case InputType.Mouse:
                QUEUED_MOUSE_EVENTS.TryDequeue(out InputMouseEvent mouseInput);
                if(MOUSE_HISTORY.TryGetValue(mouseInput.mouseCode, out List<InputMouseEvent>? mouseHistory))
                    mouseHistory.Add(mouseInput);
                try {
                    OnMouseEvent?.Invoke(mouseInput);
                }
                catch(Exception e) {
                    Debug.LogException(e);
                }
                break;
            case InputType.Keyboard:
                QUEUED_KEY_EVENTS.TryDequeue(out InputKeyEvent keyInput);
                if(KEY_HISTORY.TryGetValue(keyInput.key, out List<InputKeyEvent>? keyHistory))
                    keyHistory.Add(keyInput);
                else
                    throw new RayBlastEngineException($"No key history for {keyInput.key.ToString()}");
                try {
                    OnKeyboardEvent?.Invoke(keyInput);
                }
                catch(Exception e) {
                    Debug.LogException(e);
                }
                break;
            case InputType.JoystickAxis:
                QUEUED_JOYSTICK_AXIS_EVENTS.TryDequeue(out InputJoystickAxisEvent axisInput);
                if(!JOYSTICK_HISTORY.TryGetValue(axisInput.joystickID, out JoystickHistory? history))
                    JOYSTICK_HISTORY[axisInput.joystickID] = history = new JoystickHistory();
                history.Add(axisInput);
                try {
                    OnJoystickAxisEvent?.Invoke(axisInput);
                }
                catch(Exception e) {
                    Debug.LogException(e);
                }
                break;
            case InputType.JoystickButton:
                QUEUED_JOYSTICK_BUTTON_EVENTS.TryDequeue(out InputJoystickButtonEvent buttonInput);
                if(!JOYSTICK_HISTORY.TryGetValue(buttonInput.joystickID, out history))
                    JOYSTICK_HISTORY[buttonInput.joystickID] = history = new JoystickHistory();
                history.Add(buttonInput);
                try {
                    OnJoystickButtonEvent?.Invoke(buttonInput);
                }
                catch(Exception e) {
                    Debug.LogException(e);
                }
                break;
            case InputType.JoystickHat:
                QUEUED_JOYSTICK_HAT_EVENTS.TryDequeue(out InputJoystickHatEvent hatInput);
                if(!JOYSTICK_HISTORY.TryGetValue(hatInput.joystickID, out history))
                    JOYSTICK_HISTORY[hatInput.joystickID] = history = new JoystickHistory();
                history.Add(hatInput);
                try {
                    OnJoystickHatEvent?.Invoke(hatInput);
                }
                catch(Exception e) {
                    Debug.LogException(e);
                }
                break;
            default:
                throw new RayBlastEngineException($"No case for {type.ToString()}");
            }
        }
    }

    public static bool IsKeyDown(Key key) {
        if(key == Key.Null)
            return false;
        return ALL_KEY_STATES[key];
    }

    public static bool IsKeyPressed(Key key) {
        if(key == Key.Null)
            return false;
        return ALL_KEY_PRESSES[key];
    }

    public static bool IsKeyReleased(Key key) {
        if(key == Key.Null)
            return false;
        return ALL_KEY_RELEASES[key];
    }

    public static bool IsMouseButtonDown(MouseCode mouseCode) {
        if(mouseCode == MouseCode.Null)
            return false;
        return ALL_MOUSE_STATES[mouseCode];
    }

    public static bool IsMouseButtonPressed(MouseCode mouseCode) {
        if(mouseCode == MouseCode.Null)
            return false;
        return ALL_MOUSE_PRESSES[mouseCode];
    }

    public static bool IsMouseButtonReleased(MouseCode mouseCode) {
        if(mouseCode == MouseCode.Null)
            return false;
        return ALL_MOUSE_RELEASES[mouseCode];
    }

    public static string GetInputStringOfThisFrame() {
        return inputStringOfThisFrame;
    }

    public static bool MouseIsInside(Vector4 rect) {
        Vector2 mouseViewport = MousePosition;
        return mouseViewport.X >= rect.X && mouseViewport.Y >= rect.Y && mouseViewport.X < rect.X + rect.Z && mouseViewport.Y < rect.Y + rect.W;
    }

    private static void RefreshJoysticks() {
        uint[]? ids = SDL.GetGamepads(out _);
        if(ids == null) {
            Debug.LogError($"Could not get connected gamepads: {SDL.GetError()}");
        }
        else {
            foreach(uint id in ids.Where(static id => !OPEN_GAMEPADS.ContainsKey(id))) {
                IntPtr gamepadPtr = SDL.OpenGamepad(id);
                if(gamepadPtr == IntPtr.Zero)
                    throw new RayBlastEngineException($"Failed to open gamepad {id}: {SDL.GetError()}");
                OPEN_GAMEPADS[id] = gamepadPtr;
            }
        }
        ids = SDL.GetJoysticks(out _);
        if(ids == null) {
            Debug.LogError($"Could not get connected joysticks: {SDL.GetError()}");
        }
        else {
            foreach(uint id in ids.Where(static id => !OPEN_JOYSTICKS.ContainsKey(id))) {
                IntPtr joystickPtr = SDL.OpenJoystick(id);
                if(joystickPtr == IntPtr.Zero)
                    throw new RayBlastEngineException($"Failed to open joystick {id}: {SDL.GetError()}");
                OPEN_JOYSTICKS[id] = joystickPtr;
                JOYSTICK_HISTORY[id] = new JoystickHistory();
            }
        }
    }

    public static int GetJoystickAxisCount(uint joystickID) {
        if(OPEN_JOYSTICKS.TryGetValue(joystickID, out IntPtr joystickPtr)) {
            int buttonCount = SDL.GetNumJoystickAxes(joystickPtr);
            if(buttonCount < -1)
                Debug.LogError($"Failed to get axis count of joystick ID {joystickID}: {SDL.GetError()}");
            return buttonCount;
        }
        return 0;
    }

    public static int GetJoystickButtonCount(uint joystickID) {
        if(OPEN_JOYSTICKS.TryGetValue(joystickID, out IntPtr joystickPtr)) {
            int buttonCount = SDL.GetNumJoystickButtons(joystickPtr);
            if(buttonCount < -1)
                Debug.LogError($"Failed to get button count of joystick ID {joystickID}: {SDL.GetError()}");
            return buttonCount;
        }
        return 0;
    }

    public static int GetJoystickHatCount(uint joystickID) {
        if(OPEN_JOYSTICKS.TryGetValue(joystickID, out IntPtr joystickPtr)) {
            int buttonCount = SDL.GetNumJoystickHats(joystickPtr);
            if(buttonCount < -1)
                Debug.LogError($"Failed to get hat count of joystick ID {joystickID}: {SDL.GetError()}");
            return buttonCount;
        }
        return 0;
    }

    public static short GetJoystickRawAxisValue(uint joystickID, int axis) {
        if(OPEN_JOYSTICKS.TryGetValue(joystickID, out IntPtr joystickPtr))
            return SDL.GetJoystickAxis(joystickPtr, axis);
        return 0;
    }

    public static float GetJoystickAxisValue(uint joystickID, int axis,
                                             float deadzone = 0.05f, float limit = 0.95f) {
        if(OPEN_JOYSTICKS.TryGetValue(joystickID, out IntPtr joystickPtr)) {
            short axisValue = SDL.GetJoystickAxis(joystickPtr, axis);
            if(axisValue < 0)
                return -Mathd.InverseLerp(deadzone, limit, -axisValue / 32767f);
            return Mathd.InverseLerp(deadzone, limit, axisValue / 32767f);
        }
        return 0f;
    }

    public static bool IsJoystickButtonDown(uint joystickID, int buttonIndex) {
        if(JOYSTICK_HISTORY.TryGetValue(joystickID, out JoystickHistory? history))
            return history.GetButtonState(buttonIndex);
        return false;
    }

    public static bool IsJoystickButtonPressed(uint joystickID, int buttonIndex) {
        if(JOYSTICK_HISTORY.TryGetValue(joystickID, out JoystickHistory? history))
            return history.GetButtonPress(buttonIndex);
        return false;
    }

    public static bool IsJoystickButtonReleased(uint joystickID, int buttonIndex) {
        if(JOYSTICK_HISTORY.TryGetValue(joystickID, out JoystickHistory? history))
            return history.GetButtonRelease(buttonIndex);
        return false;
    }

    public static HatState GetJoystickRawHatValue(uint joystickID, int hat) {
        if(OPEN_JOYSTICKS.TryGetValue(joystickID, out IntPtr joystickPtr))
            return SDL.GetJoystickHat(joystickPtr, hat).ToRayBlast();
        return 0;
    }

    [LibraryImport("SDL3", EntryPoint = "SDL_GetGamepadBindings")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial IntPtr SDL_GetGamepadBindings_Hack(IntPtr gamepad, out int count);

    public static GamepadTranslator GetGamepadTranslator(uint gamepadID) {
        RefreshJoysticks();
        IntPtr gamepadPtr = SDL.GetGamepadFromID(gamepadID);
        if(gamepadPtr == IntPtr.Zero)
            throw new RayBlastEngineException($"Failed to get gamepad from ID {gamepadID}: {SDL.GetError()}");
        //TODO: Cache translator for gamepad ID
        IntPtr bindingsPtr = SDL_GetGamepadBindings_Hack(gamepadPtr, out int bindingsCount);
        SDL.GamepadBinding[]? bindings = SDL.PointerToStructureArray<SDL.GamepadBinding>(bindingsPtr, bindingsCount);
        if(bindings == null)
            throw new RayBlastEngineException($"Failed to get bindings for gamepad {gamepadID}: {SDL.GetError()}");
        var translator = new GamepadTranslator();
        IntPtr joystickPtr = SDL.GetGamepadJoystick(gamepadPtr);
        translator.joystickID = SDL.GetJoystickID(joystickPtr);
        foreach(SDL.GamepadBinding binding in bindings) {
            var translation = new GamepadBindingTranslation();
            translator.Add(translation);
            switch(binding.InputType) {
            case SDL.GamepadBindingType.Axis:
                SDL.GamepadBinding.InputData.AxisInfo inputAxis = binding.Input.Axis;
                translation.joystickAxis = inputAxis.Axis;
                translation.joystickMin = inputAxis.AxisMin;
                translation.joystickMax = inputAxis.AxisMax;
                break;
            case SDL.GamepadBindingType.Button:
                translation.joystickButton = binding.Input.Button;
                break;
            case SDL.GamepadBindingType.Hat:
                SDL.GamepadBinding.InputData.HatInfo inputHat = binding.Input.Hat;
                translation.joystickHat = inputHat.Hat;
                translation.joystickHatMask = inputHat.HatMask;
                translation.translationType = GamepadBindingTranslation.TranslationType.HatToHat;
                break;
            default:
                throw new RayBlastEngineException($"Gamepad {gamepadID} had unrecognized input binding type {binding.InputType}");
            }
            //TODO: Add DPad button mappings from the four bindings produced
            if(binding.InputType != SDL.GamepadBindingType.Hat) {
                switch(binding.OutputType) {
                case SDL.GamepadBindingType.Axis:
                    translation.translationType = binding.InputType switch {
                        SDL.GamepadBindingType.None =>
                            throw new RayBlastEngineException($"Gamepad translation fail from {binding.InputType} to axis"),
                        SDL.GamepadBindingType.Button => GamepadBindingTranslation.TranslationType.JoystickButtonToGamepadAxis,
                        SDL.GamepadBindingType.Axis => GamepadBindingTranslation.TranslationType.AxisToAxis,
                        SDL.GamepadBindingType.Hat => GamepadBindingTranslation.TranslationType.JoystickHatToGamepadAxis,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    translation.gamepadAxis = binding.Output.Axis.Axis.ToRayBlast();
                    translation.gamepadMin = binding.Output.Axis.AxisMin;
                    translation.gamepadMax = binding.Output.Axis.AxisMax;
                    break;
                case SDL.GamepadBindingType.Button:
                    translation.translationType = binding.InputType switch {
                        SDL.GamepadBindingType.None => throw new RayBlastEngineException(
                                                           $"Gamepad translation fail from {binding.InputType} to button"),
                        SDL.GamepadBindingType.Button => GamepadBindingTranslation.TranslationType.ButtonToButton,
                        SDL.GamepadBindingType.Axis => GamepadBindingTranslation.TranslationType.JoystickAxisToGamepadButton,
                        SDL.GamepadBindingType.Hat => GamepadBindingTranslation.TranslationType.JoystickHatToGamepadButton,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    translation.gamepadButton = binding.Output.Button.ToRayBlast();
                    SDL.GamepadButtonLabel label = SDL.GetGamepadButtonLabel(gamepadPtr, binding.Output.Button);
                    if(label != SDL.GamepadButtonLabel.Unknown)
                        translation.buttonLabel = label.ToString();
                    break;
                case SDL.GamepadBindingType.Hat:
                    translation.translationType = binding.InputType switch {
                        SDL.GamepadBindingType.None => throw new RayBlastEngineException($"Gamepad translation fail from {binding.InputType} to hat"),
                        SDL.GamepadBindingType.Button => GamepadBindingTranslation.TranslationType.JoystickButtonToGamepadHat,
                        SDL.GamepadBindingType.Axis => GamepadBindingTranslation.TranslationType.JoystickAxisToGamepadHat,
                        SDL.GamepadBindingType.Hat => GamepadBindingTranslation.TranslationType.HatToHat,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    break;
                default:
                    throw new RayBlastEngineException($"Gamepad {gamepadID} had unrecognized output binding type {binding.OutputType}");
                }
            }
        }
        return translator;
    }

    public static (ushort vendor, ushort product, ushort version, ushort crc16) GetJoystickGUID(uint id) {
        RefreshJoysticks();
        if(OPEN_JOYSTICKS.TryGetValue(id, out IntPtr ptr)) {
            SDL.GUID guid = SDL.GetJoystickGUID(ptr);
            SDL.GetJoystickGUIDInfo(guid, out short vendor, out short product, out short version, out short crc16);
            if(vendor == 0 && product == 0 && version == 0 && crc16 == 0)
                Debug.LogError($"Failed to get GUID for joystick {id}: {SDL.GetError()}");
            return ((ushort, ushort, ushort, ushort))(vendor, product, version, crc16);
        }
        return (0, 0, 0, 0);
    }

    public static ulong GetJoystickBaseGUID(uint id) {
        (ushort vendor, ushort product, ushort version, _) = GetJoystickGUID(id);
        return ((ulong)vendor << 48) | ((ulong)product << 32) | ((ulong)version << 16);
    }

    public static string? GetJoystickName(uint id) {
        RefreshJoysticks();
        return OPEN_JOYSTICKS.TryGetValue(id, out IntPtr ptr) ? SDL.GetJoystickName(ptr) : "<not connected>";
    }

    public static string GetJoystickPath(uint id) {
        RefreshJoysticks();
        if(OPEN_JOYSTICKS.TryGetValue(id, out IntPtr ptr)) {
            return SDL.GetJoystickPath(ptr) ?? "<null>";
        }
        return "<not connected>";
    }
}

public delegate void InputMouseEventHandler(InputMouseEvent inputEvent);

public delegate void InputKeyEventHandler(InputKeyEvent inputEvent);

public delegate void InputJoystickButtonEventHandler(InputJoystickButtonEvent inputEvent);

public delegate void InputJoystickAxisEventHandler(InputJoystickAxisEvent inputEvent);

public delegate void InputJoystickHatEventHandler(InputJoystickHatEvent inputEvent);
