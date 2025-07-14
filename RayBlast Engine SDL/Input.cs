using System.Collections.Concurrent;
using System.ComponentModel;
using System.Numerics;
using Cysharp.Text;
using SDL3;

namespace RayBlast;

//TODO: Record key press lengths and see if it can actually take advantage of 1000 Hz keyboards
public static class Input {
    internal static readonly Dictionary<Key, List<InputEvent>> HISTORY = new();
    internal static readonly Dictionary<MouseCode, List<InputEvent>> MOUSE_HISTORY = new();

    private static readonly List<Key> ALL_KEYS = new();
    private static readonly List<MouseCode> ALL_MOUSE_CODES = new();
    private static readonly Dictionary<Key, InputListener> ALL_FRAME_LISTENERS = new();
    private static readonly Dictionary<MouseCode, int> MOUSE_EVENT_INDEXES = new();
    private static readonly Dictionary<MouseCode, bool> ALL_MOUSE_STATES = new();
    private static readonly Dictionary<MouseCode, bool> ALL_MOUSE_PRESSES = new();
    private static readonly Dictionary<MouseCode, bool> ALL_MOUSE_RELEASES = new();
    internal static readonly ConcurrentQueue<InputEvent> QUEUED_EVENTS = new();

    public static int pollsPerformed;

    private static int pollSleepInterval = 1;
    private static long startTick;
    private static string inputStringOfThisFrame = "";
    internal static Vector2 capturedMousePosition;
    internal static Vector2 capturedMouseScrollAmount;

    public static event InputEventHandler? OnEvent;

    public static double PollRate => pollsPerformed / ((RayBlastEngine.WATCH.ElapsedTicks - startTick) / (double)TimeSpan.TicksPerSecond);

    public static int PollSleepInterval {
        get => pollSleepInterval;
        set {
            pollSleepInterval = value;
            pollsPerformed = 0;
            startTick = RayBlastEngine.WATCH.ElapsedTicks;
        }
    }

    public static float ScrollAmount { get; internal set; }
    public static Vector2 MouseViewport { get; internal set; }
    public static Vector2 MousePosition { get; internal set; }
    public static bool MouseVisible { get; set; } = true;

    internal static void Initialize() {
        //TODO: Add joystick/gamepad support
        // uint[]? joystickIDs = SDL.GetJoysticks(out _);
        // if(joystickIDs != null) {
        //     foreach(uint id in joystickIDs) {
        //         SDL.OpenJoystick(id);
        //     }
        // }
        Key[] allKeys = Enum.GetValues<Key>();
        ALL_KEYS.EnsureCapacity(allKeys.Length - 1);
        foreach(Key i in allKeys) {
            if(i != 0) {
                ALL_KEYS.Add(i);
                HISTORY[i] = new List<InputEvent>(256) {
                    new() {
                        key = i
                    }
                };
                var inputListener = new InputListener(i.ToString(), i);
                inputListener.AutoUpdateEachFrame = false;
                ALL_FRAME_LISTENERS[i] = inputListener;
            }
        }
        MouseCode[] allMouseCodes = Enum.GetValues<MouseCode>();
        ALL_MOUSE_CODES.EnsureCapacity(allMouseCodes.Length - 1);
        foreach(MouseCode i in allMouseCodes) {
            if(i != 0) {
                ALL_MOUSE_CODES.Add(i);
                MOUSE_HISTORY[i] = new List<InputEvent>(256) {
                    new() {
                        mouseCode = i
                    }
                };
                MOUSE_EVENT_INDEXES[i] = 1;
                ALL_MOUSE_STATES[i] = false;
                ALL_MOUSE_PRESSES[i] = false;
                ALL_MOUSE_RELEASES[i] = false;
            }
        }
    }

    internal static void UpdateFrame() {
        PollEvents();
        ScrollAmount = capturedMouseScrollAmount.Y;
        MousePosition = capturedMousePosition;
        MouseViewport = new Vector2(capturedMousePosition.X / Graphics.CurrentResolution.X, capturedMousePosition.Y / Graphics.CurrentResolution.Y);
        capturedMouseScrollAmount = Vector2.Zero;
        foreach(Key i in ALL_KEYS) {
            ALL_FRAME_LISTENERS[i].Update();
            ALL_FRAME_LISTENERS[i].FixedUpdate();
        }
        foreach(MouseCode m in ALL_MOUSE_CODES) {
            int lastIndex = MOUSE_EVENT_INDEXES[m];
            bool pressed = false;
            bool released = false;
            List<InputEvent> inputEvents = MOUSE_HISTORY[m];
            for(int i = lastIndex; i < inputEvents.Count; i++) {
                if(inputEvents[i].IsDown) {
                    pressed = true;
                }
                else {
                    released = true;
                }
            }
            ALL_MOUSE_STATES[m] = inputEvents.Last().IsDown;
            ALL_MOUSE_PRESSES[m] = pressed;
            ALL_MOUSE_RELEASES[m] = released;
            MOUSE_EVENT_INDEXES[m] = inputEvents.Count;
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
        while(QUEUED_EVENTS.TryDequeue(out InputEvent input)) {
            if(input.mouseCode != MouseCode.Null) {
                if(MOUSE_HISTORY.TryGetValue(input.mouseCode, out List<InputEvent>? history)) {
                    history.Add(input);
                    try {
                        OnEvent?.Invoke(input);
                    }
                    catch(Exception e) {
                        Debug.LogException(e);
                    }
                }
            }
            else if(HISTORY.TryGetValue(input.key, out List<InputEvent>? history)) {
                history.Add(input);
                try {
                    OnEvent?.Invoke(input);
                }
                catch(Exception e) {
                    Debug.LogException(e);
                }
            }
            else
                throw new RayBlastEngineException($"No key history for {input.key.ToString()}");
        }
    }

    public static bool IsKeyDown(Key key) {
        if(key == Key.Null)
            return false;
        return ALL_FRAME_LISTENERS[key].Held;
    }

    public static bool IsKeyPressed(Key key) {
        if(key == Key.Null)
            return false;
        return ALL_FRAME_LISTENERS[key].Pressed;
    }

    public static bool IsKeyReleased(Key key) {
        if(key == Key.Null)
            return false;
        return ALL_FRAME_LISTENERS[key].Released;
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

    public static float GetGamepadAxisValue(int joystickIndex, GamepadAxis axis) {
        throw new NotImplementedException();
    }
}

public delegate void InputEventHandler(InputEvent inputEvent);
