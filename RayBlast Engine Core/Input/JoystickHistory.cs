namespace RayBlast;

public class JoystickHistory {
    private readonly List<List<InputJoystickAxisEvent>> axisHistory = [];
    private readonly List<List<InputJoystickButtonEvent>> buttonHistory = [];
    private readonly List<List<InputJoystickHatEvent>> hatHistory = [];
    private readonly List<int> buttonEventIndexes = [];
    private readonly List<bool> buttonStates = [];
    private readonly List<bool> buttonPresses = [];
    private readonly List<bool> buttonReleases = [];

    public void FlushButtonStates() {
        for(int i = 0; i < buttonEventIndexes.Count; i++) {
            int lastIndex = buttonEventIndexes[i];
            List<InputJoystickButtonEvent> buttonEvents = buttonHistory[i];
            var pressed = false;
            var released = false;
            for(int j = lastIndex; j < buttonEvents.Count; j++) {
                if(buttonEvents[j].IsDown)
                    pressed = true;
                else
                    released = true;
            }
            buttonStates[i] = buttonEvents.Last().IsDown;
            buttonPresses[i] = pressed;
            buttonReleases[i] = released;
            buttonEventIndexes[i] = buttonEvents.Count;
        }
    }

    public void Add(InputJoystickAxisEvent axisInput) {
        while(axisHistory.Count <= axisInput.joystickAxis) {
            axisHistory.Add([]);
        }
        axisHistory[axisInput.joystickAxis].Add(axisInput);
    }

    public void Add(InputJoystickButtonEvent buttonInput) {
        EnsureButtonCount(buttonInput.joystickButton + 1);
        buttonHistory[buttonInput.joystickButton].Add(buttonInput);
    }

    private void EnsureButtonCount(int buttonCount) {
        while(buttonHistory.Count < buttonCount) {
            buttonHistory.Add([
                new InputJoystickButtonEvent()
            ]);
            buttonEventIndexes.Add(1);
            buttonStates.Add(false);
            buttonPresses.Add(false);
            buttonReleases.Add(false);
        }
    }

    public void Add(InputJoystickHatEvent hatInput) {
        while(hatHistory.Count <= hatInput.joystickHat) {
            hatHistory.Add([]);
        }
        hatHistory[hatInput.joystickHat].Add(hatInput);
    }

    public bool GetButtonState(int buttonIndex) {
        return buttonStates.Count > buttonIndex && buttonStates[buttonIndex];
    }

    public bool GetButtonPress(int buttonIndex) {
        return buttonPresses.Count > buttonIndex && buttonPresses[buttonIndex];
    }

    public bool GetButtonRelease(int buttonIndex) {
        return buttonReleases.Count > buttonIndex && buttonReleases[buttonIndex];
    }
}
