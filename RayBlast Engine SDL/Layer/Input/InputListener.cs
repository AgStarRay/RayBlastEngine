using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RayBlast;

[Serializable]
public class InputListener : IDisposable {
    private static readonly List<InputListener> LISTENERS = new();

    public string name;
    public string displayName;
    public string label = "?";
    public List<Key> keys = new();
    [JsonIgnore]
    public int joystickID = -1;
    public List<int> axisIndexes = new();
    public List<float> analogThresholds = new();
    public float repeatDelay = 0.25f;
    public float repeatRate = 10f;
    public float levelTwoDelay = 2f;
    public float repeatRateTwo = 20f;

    private double repeatTimeLeft = 0.3;
    private double levelTwoTimeLeft = 2.05;
    private double unpausedRepeatTimeLeft = 0.3;
    private double unpausedLevelTwoTimeLeft = 2.05;

    private readonly List<int> lastUnpausedEventIndexes = new();
    private readonly List<int> lastEventIndexes = new();

    [JsonIgnore]
    public bool Pressed { get; private set; }
    [JsonIgnore]
    public bool Held { get; private set; }
    [JsonIgnore]
    public bool Released { get; private set; }
    [JsonIgnore]
    public int Signals { get; private set; }
    [JsonIgnore]
    public bool UnpausedPressed { get; private set; }
    [JsonIgnore]
    public bool UnpausedHeld { get; private set; }
    [JsonIgnore]
    public bool UnpausedReleased { get; private set; }
    [JsonIgnore]
    public int UnpausedSignals { get; private set; }

    [JsonIgnore]
    public bool IsReady {
        get {
            if(keys.Count > 0)
                return true;
            if(axisIndexes.Count > 0 && analogThresholds.Count > 0)
                return true;
            return false;
        }
    }

    [JsonIgnore]
    public float[] AxisValues {
        get {
            var values = new float[Math.Min(axisIndexes.Count, analogThresholds.Count)];
            for(var i = 0; i < values.Length; i++) {
                if(axisIndexes[i] >= 0 && axisIndexes[i] < 10) {
                    if(joystickID > -1)
                        //TODO_AFTER: Read analog value
                        values[i] = Input.GetJoystickAxisValue((uint)joystickID, i);
                }
            }
            return values;
        }
    }

    public InputListener(string listenerName) {
        name = listenerName;
        displayName = listenerName;
        LISTENERS.Add(this);
        if(LISTENERS.Count > 999)
            Debug.LogWarning("Too many input listeners", false);
    }

    public InputListener(string listenerName, Key key) {
        name = listenerName;
        displayName = listenerName;
        keys = new List<Key> {
            key
        };
        LISTENERS.Add(this);
        if(LISTENERS.Count > 999)
            Debug.LogWarning("Too many input listeners", false);
    }

    public InputListener(string listenerName, params Key[] keys) {
        name = listenerName;
        displayName = listenerName;
        this.keys = new List<Key>(keys);
        LISTENERS.Add(this);
        if(LISTENERS.Count > 999)
            Debug.LogWarning("Too many input listeners", false);
    }

    public bool AutoUpdateEachFrame {
        get => LISTENERS.Contains(this);
        set {
            if(value) {
                if(!LISTENERS.Contains(this)) {
                    LISTENERS.Add(this);
                    if(LISTENERS.Count > 999)
                        Debug.LogWarning("Too many input listeners", false);
                }
            }
            else {
                LISTENERS.Remove(this);
            }
        }
    }

    public static void UpdateAll() {
        foreach(InputListener l in LISTENERS) {
            l.Update();
        }
    }

    public static void FixedUpdateAll() {
        if(Time.timeScale > 0.0) {
            // Debug.Log("Update inputs at " + (1 / Time.fixedDeltaTime) + " fps", includeStackTrace: false);
            foreach(InputListener l in LISTENERS) {
                l.FixedUpdate();
            }
        }
    }

    public void Update() {
        if(lastUnpausedEventIndexes.Count != keys.Count) {
            if(keys.Count > 32) {
                Debug.LogWarning($"Too many keys in InputListener, {keys.Count}/32");
                keys.RemoveRange(32, keys.Count - 32);
            }
            lastUnpausedEventIndexes.Clear();
            foreach(Key k in keys) {
                lastUnpausedEventIndexes.Add(Input.KEY_HISTORY[k].Count);
            }
        }

        if(lastUnpausedEventIndexes.Count == 1) {
            var pressed = false;
            var released = false;
            bool currentState = UnpausedHeld;
            List<InputKeyEvent> history = Input.KEY_HISTORY[keys[0]];
            for(int i = lastUnpausedEventIndexes[0]; i < history.Count; i++) {
                bool newState = history[i].IsDown;
                if(newState != currentState) {
                    if(newState)
                        pressed = true;
                    else
                        released = true;
                    if(pressed && released)
                        break;
                    currentState = newState;
                }
            }
            lastUnpausedEventIndexes[0] = history.Count;
            UnpausedPressed = pressed;
            UnpausedReleased = released;
            UnpausedHeld = history.Last().IsDown;
        }
        else {
            var currentStates = 0;
            var history = new List<(int, InputKeyEvent)>();
            for(var i = 0; i < keys.Count; i++) {
                Key key = keys[i];
                List<InputKeyEvent> keyHistory = Input.KEY_HISTORY[key];
                int startIndex = lastUnpausedEventIndexes[i];
                if(keyHistory[startIndex - 1].IsDown)
                    currentStates |= 1 << i;
                for(int j = startIndex; j < keyHistory.Count; j++) {
                    InputKeyEvent e = keyHistory[j];
                    history.Add((i, e));
                }
                lastUnpausedEventIndexes[i] = keyHistory.Count;
            }
            if(history.Count > 0)
                history.Sort((a, b) => a.Item2.time.CompareTo(b.Item2.time));

            var pressed = false;
            var released = false;
            foreach((int, InputKeyEvent) p in history) {
                int newStates;
                if(p.Item2.IsDown)
                    newStates = currentStates.WithBit(p.Item1);
                else
                    newStates = currentStates.WithoutBit(p.Item1);
                if(currentStates != 0 != (newStates != 0)) {
                    if(newStates != 0)
                        pressed = true;
                    else
                        released = true;
                }
                currentStates = newStates;
            }
            UnpausedPressed = pressed;
            UnpausedReleased = released;
            UnpausedHeld = currentStates != 0;
        }

        // for(int i = 0; i < axisIndexes.Count && i < analogThresholds.Count; i++)
        // 	if(analogThresholds[i] != 0f) {
        // 		float value = AxisValues[i];
        // 		if(Math.Abs(value) >= Math.Abs(analogThresholds[i]) && Math.Sign(value) == Math.Sign(analogThresholds[i]))
        // 			return true;
        // 	}
        UnpausedSignals = 0;
        if(UnpausedPressed) {
            unpausedRepeatTimeLeft = repeatDelay;
            unpausedLevelTwoTimeLeft = levelTwoDelay;
            UnpausedSignals++;
        }
        if(UnpausedHeld && repeatRate >= 0f)
            ProgressUnpausedSignalGenerator();
    }

    public void FixedUpdate() {
        if(lastEventIndexes.Count != keys.Count) {
            if(keys.Count > 32) {
                Debug.LogWarning($"Too many keys in InputListener, {keys.Count}/32");
                keys.RemoveRange(32, keys.Count - 32);
            }
            lastEventIndexes.Clear();
            foreach(Key k in keys) {
                lastEventIndexes.Add(Input.KEY_HISTORY[k].Count);
            }
        }

        if(lastEventIndexes.Count == 1) {
            var pressed = false;
            var released = false;
            bool currentState = Held;
            List<InputKeyEvent> history = Input.KEY_HISTORY[keys[0]];
            for(int i = lastEventIndexes[0]; i < history.Count; i++) {
                bool newState = history[i].IsDown;
                if(newState != currentState) {
                    if(newState)
                        pressed = true;
                    else
                        released = true;
                    if(pressed && released)
                        break;
                    currentState = newState;
                }
            }
            lastEventIndexes[0] = history.Count;
            Pressed = pressed;
            Released = released;
            Held = history.Last().IsDown;
        }
        else {
            var currentStates = 0;
            var history = new List<(int, InputKeyEvent)>();
            for(var i = 0; i < keys.Count; i++) {
                Key key = keys[i];
                List<InputKeyEvent> keyHistory = Input.KEY_HISTORY[key];
                int startIndex = lastEventIndexes[i];
                if(keyHistory[startIndex - 1].IsDown)
                    currentStates |= 1 << i;
                for(int j = startIndex; j < keyHistory.Count; j++) {
                    InputKeyEvent e = keyHistory[j];
                    history.Add((i, e));
                }
                lastEventIndexes[i] = keyHistory.Count;
            }
            if(history.Count > 0)
                history.Sort((a, b) => a.Item2.time.CompareTo(b.Item2.time));

            var pressed = false;
            var released = false;
            foreach((int, InputKeyEvent) p in history) {
                int newStates;
                if(p.Item2.IsDown)
                    newStates = currentStates.WithBit(p.Item1);
                else
                    newStates = currentStates.WithoutBit(p.Item1);
                if(currentStates != 0 != (newStates != 0)) {
                    if(newStates != 0)
                        pressed = true;
                    else
                        released = true;
                }
                currentStates = newStates;
            }
            Pressed = pressed;
            Released = released;
            Held = currentStates != 0;
        }

        // for(int i = 0; i < axisIndexes.Count && i < analogThresholds.Count; i++)
        // 	if(analogThresholds[i] != 0f) {
        // 		float value = AxisValues[i];
        // 		if(Math.Abs(value) >= Math.Abs(analogThresholds[i]) && Math.Sign(value) == Math.Sign(analogThresholds[i]))
        // 			return true;
        // 	}
        Signals = 0;
        if(Pressed) {
            repeatTimeLeft = repeatDelay;
            levelTwoTimeLeft = levelTwoDelay;
            Signals++;
        }
        if(Held && repeatRate >= 0f)
            ProgressSignalGenerator();
    }

    private void ProgressSignalGenerator() {
        if(repeatRate == 0f || float.IsInfinity(repeatRate))
            return;
        float repeatPeriod = 1f / repeatRate;
        repeatTimeLeft -= Time.unscaledDeltaTime;
        levelTwoTimeLeft -= Time.unscaledDeltaTime;
        if(repeatTimeLeft <= -repeatPeriod) {
            var signalsGenerated = (int)Math.Floor(-repeatTimeLeft * repeatRate);
            Signals += signalsGenerated;
            if(levelTwoTimeLeft < 0f) {
                float repeatPeriodTwo = 1f / repeatRateTwo;
                repeatTimeLeft += repeatPeriodTwo * signalsGenerated;
            }
            else {
                repeatTimeLeft += repeatPeriod * signalsGenerated;
            }
        }
    }

    private void ProgressUnpausedSignalGenerator() {
        if(repeatRate == 0f || float.IsInfinity(repeatRate))
            return;
        float repeatPeriod = 1f / repeatRate;
        unpausedRepeatTimeLeft -= Time.unscaledDeltaTime;
        unpausedLevelTwoTimeLeft -= Time.unscaledDeltaTime;
        if(unpausedRepeatTimeLeft <= -repeatPeriod) {
            var signalsGenerated = (int)Math.Floor(-unpausedRepeatTimeLeft * repeatRate);
            UnpausedSignals += signalsGenerated;
            if(unpausedLevelTwoTimeLeft < 0f) {
                float repeatPeriodTwo = 1f / repeatRateTwo;
                unpausedRepeatTimeLeft += repeatPeriodTwo * signalsGenerated;
            }
            else {
                unpausedRepeatTimeLeft += repeatPeriod * signalsGenerated;
            }
        }
    }

    public void InjectKeyEvent(InputKeyEvent e) {
        
    }

    public void Dispose() {
        LISTENERS.Remove(this);
    }
}

// using Raylib_cs;
//
// namespace RayBlast;
//
// public class InputListener {
// 	public KeyboardKey key;
//
// 	internal int lastTrackedIndex;
// 	internal byte state;
//
// 	public bool IsDown => (state & 1) != 0;
// 	public bool IsPressed => (state & 2) != 0;
// 	public bool IsReleased => (state & 4) != 0;
//
// 	public InputListener(string listenerName) {
// 		name = listenerName;
// 		displayName = listenerName;
// 		listeners.Add(this);
// 		if(listeners.Count > 999)
// 			Debug.LogWarning("Too many input listeners", includeStackTrace: false);
// 	}
//
// 	public InputListener(string listenerName, Key key) {
// 		name = listenerName;
// 		displayName = listenerName;
// 		RayBlast.Keys = new List<Key> {
// 			key
// 		};
// 		listeners.Add(this);
// 	}
//
// 	public InputListener(string listenerName, params Key[] keys) {
// 		name = listenerName;
// 		displayName = listenerName;
// 		RayBlast.Keys = new List<Key>(keys);
// 		listeners.Add(this);
// 	}
// }
