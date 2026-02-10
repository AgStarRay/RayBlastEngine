namespace RayBlast;

public class GamepadBindingTranslation {
    public TranslationType translationType;
    public GamepadAxis gamepadAxis = GamepadAxis.Null;
    public GamepadButton gamepadButton = GamepadButton.Null;
    public int gamepadMin;
    public int gamepadMax;
    public int joystickAxis;
    public int joystickMin;
    public int joystickMax;
    public int joystickButton;
    public int joystickHat;
    public int joystickHatMask;
    public string? buttonLabel;

    public bool IsAxis => translationType.HasFlag(TranslationType.GamepadAxis);
    public bool IsButton => translationType.HasFlag(TranslationType.GamepadButton);
    public bool IsHat => translationType.HasFlag(TranslationType.GamepadHat);

    public override string ToString() {
        switch(translationType) {
        case TranslationType.AxisToAxis:
            return $"jaxis {joystickAxis} to gaxis {gamepadAxis}";
        case TranslationType.JoystickAxisToGamepadButton:
            return buttonLabel != null
                       ? $"jaxis {joystickAxis} to gbutton {gamepadButton} \"{buttonLabel}\""
                       : $"jaxis {joystickAxis} to gbutton {gamepadButton}";
        case TranslationType.JoystickAxisToGamepadHat:
            return $"jaxis {joystickAxis} to ghat {joystickHat}";
        case TranslationType.JoystickButtonToGamepadAxis:
            return $"jbutton {joystickButton} to gaxis {gamepadAxis}";
        case TranslationType.ButtonToButton:
            return buttonLabel != null
                       ? $"jbutton {joystickButton} to gbutton {gamepadButton} \"{buttonLabel}\""
                       : $"jbutton {joystickButton} to gbutton {gamepadButton}";
        case TranslationType.JoystickButtonToGamepadHat:
            return $"jbutton {joystickButton} to ghat {joystickHat}";
        case TranslationType.JoystickHatToGamepadAxis:
            return $"jhat {joystickHat} to gaxis {gamepadAxis}";
        case TranslationType.JoystickHatToGamepadButton:
            return buttonLabel != null
                       ? $"jhat {joystickHat} to gbutton {gamepadButton} \"{buttonLabel}\""
                       : $"jhat {joystickHat} to gbutton {gamepadButton}";
        case TranslationType.HatToHat:
            return $"jhat {joystickHat} to ghat {joystickHat}";
        default:
            return translationType.ToString();
        }
    }

    [Flags]
    public enum TranslationType {
        Invalid = 0,
        JoystickAxis = 1 << 0,
        JoystickButton = 1 << 1,
        JoystickHat = 1 << 2,
        JoystickMask = JoystickAxis | JoystickButton | JoystickHat,
        GamepadAxis = 1 << 3,
        GamepadButton = 1 << 4,
        GamepadHat = 1 << 5,
        GamepadMask = GamepadAxis | GamepadButton | GamepadHat,

        AxisToAxis = JoystickAxis | GamepadAxis,
        JoystickAxisToGamepadButton = JoystickAxis | GamepadButton,
        JoystickAxisToGamepadHat = JoystickAxis | GamepadHat,
        JoystickButtonToGamepadAxis = JoystickButton | GamepadAxis,
        ButtonToButton = JoystickButton | GamepadButton,
        JoystickButtonToGamepadHat = JoystickButton | GamepadHat,
        JoystickHatToGamepadAxis = JoystickHat | GamepadAxis,
        JoystickHatToGamepadButton = JoystickHat | GamepadButton,
        HatToHat = JoystickHat | GamepadHat
    }
}
