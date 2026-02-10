using SDL3;

namespace RayBlast;

public sealed class GamepadTranslator {
    public uint joystickID;

    public readonly List<GamepadBindingTranslation> translations = [];

    internal void Add(GamepadBindingTranslation translation) {
        translations.Add(translation);
    }

    public string GetButtonLabel(GamepadButton button) {
        foreach(GamepadBindingTranslation translation in translations) {
            if(translation.gamepadButton == button)
                return translation.buttonLabel ?? button.ToString();
        }
        return button.ToString();
    }

    public bool IsButtonDown(GamepadButton button) {
        foreach(GamepadBindingTranslation translation in translations) {
            if((translation.translationType & GamepadBindingTranslation.TranslationType.GamepadButton) != 0 && translation.gamepadButton == button) {
                switch(translation.translationType & GamepadBindingTranslation.TranslationType.JoystickMask) {
                case GamepadBindingTranslation.TranslationType.JoystickAxis:
                    if(translation.joystickMin < 0f || translation.joystickMax < 0f)
                        return Input.GetJoystickRawAxisValue(joystickID, translation.joystickAxis) < translation.joystickMin;
                    return Input.GetJoystickRawAxisValue(joystickID, translation.joystickAxis) > translation.joystickMin;
                case GamepadBindingTranslation.TranslationType.JoystickButton:
                    return Input.IsJoystickButtonDown(joystickID, translation.joystickButton);
                case GamepadBindingTranslation.TranslationType.JoystickHat:
                    throw new NotImplementedException();
                default:
                    throw new RayBlastEngineException($"Cannot translate gamepad button {button} to joystick mapping");
                }
            }
        }
        return false;
    }

    public bool IsButtonPressed(GamepadButton button) {
        foreach(GamepadBindingTranslation translation in translations) {
            if((translation.translationType & GamepadBindingTranslation.TranslationType.GamepadButton) != 0 && translation.gamepadButton == button) {
                switch(translation.translationType & GamepadBindingTranslation.TranslationType.JoystickMask) {
                case GamepadBindingTranslation.TranslationType.JoystickAxis:
                    throw new NotImplementedException();
                case GamepadBindingTranslation.TranslationType.JoystickButton:
                    return Input.IsJoystickButtonPressed(joystickID, translation.joystickButton);
                case GamepadBindingTranslation.TranslationType.JoystickHat:
                    throw new NotImplementedException();
                default:
                    throw new RayBlastEngineException($"Cannot translate gamepad button {button} to joystick mapping");
                }
            }
        }
        return false;
    }
 
    public bool IsButtonReleased(GamepadButton button) {
        foreach(GamepadBindingTranslation translation in translations) {
            if((translation.translationType & GamepadBindingTranslation.TranslationType.GamepadButton) != 0 && translation.gamepadButton == button) {
                switch(translation.translationType & GamepadBindingTranslation.TranslationType.JoystickMask) {
                case GamepadBindingTranslation.TranslationType.JoystickAxis:
                    throw new NotImplementedException();
                case GamepadBindingTranslation.TranslationType.JoystickButton:
                    return Input.IsJoystickButtonReleased(joystickID, translation.joystickButton);
                case GamepadBindingTranslation.TranslationType.JoystickHat:
                    throw new NotImplementedException();
                default:
                    throw new RayBlastEngineException($"Cannot translate gamepad button {button} to joystick mapping");
                }
            }
        }
        return false;
    }

    public int GetRawAxisValue(GamepadAxis axis) {
        foreach(GamepadBindingTranslation translation in translations) {
            if((translation.translationType & GamepadBindingTranslation.TranslationType.GamepadAxis) != 0 && translation.gamepadAxis == axis) {
                switch(translation.translationType & GamepadBindingTranslation.TranslationType.JoystickMask) {
                case GamepadBindingTranslation.TranslationType.JoystickAxis:
                    return Input.GetJoystickRawAxisValue(joystickID, translation.joystickAxis);
                case GamepadBindingTranslation.TranslationType.JoystickButton:
                    return Input.IsJoystickButtonDown(joystickID, translation.joystickButton) ? translation.gamepadMax : 0;
                case GamepadBindingTranslation.TranslationType.JoystickHat:
                    throw new NotImplementedException();
                default:
                    throw new RayBlastEngineException($"Cannot translate gamepad axis {axis} to joystick mapping");
                }
            }
        }
        return 0;
    }

    public HatState GetHatState(int hat) {
        foreach(GamepadBindingTranslation translation in translations) {
            if((translation.translationType & GamepadBindingTranslation.TranslationType.GamepadHat) != 0 && translation.joystickHat == hat) {
                switch(translation.translationType & GamepadBindingTranslation.TranslationType.JoystickMask) {
                case GamepadBindingTranslation.TranslationType.JoystickHat:
                    return Input.GetJoystickRawHatValue(joystickID, hat);
                case GamepadBindingTranslation.TranslationType.JoystickAxis:
                case GamepadBindingTranslation.TranslationType.JoystickButton:
                    throw new NotImplementedException();
                default:
                    throw new RayBlastEngineException($"Cannot translate gamepad hat {hat} to joystick mapping");
                }
            }
        }
        return 0;
    }

    public string GetLabelForJoystickAxis(short currentIndex, short currentValue) {
        foreach(GamepadBindingTranslation t in translations) {
            if((t.translationType & GamepadBindingTranslation.TranslationType.JoystickMask)
            == GamepadBindingTranslation.TranslationType.JoystickAxis && t.joystickAxis == currentIndex) {
                return GetGamepadLabel(t, currentValue);
            }
        }
        return "???";
    }

    public string GetLabelForJoystickButton(short currentIndex) {
        foreach(GamepadBindingTranslation t in translations) {
            if((t.translationType & GamepadBindingTranslation.TranslationType.JoystickMask)
            == GamepadBindingTranslation.TranslationType.JoystickButton && t.joystickButton == currentIndex) {
                return GetGamepadLabel(t, 1);
            }
        }
        return "???";
    }

    public string GetLabelForJoystickHat(short currentIndex, HatState currentValue) {
        foreach(GamepadBindingTranslation t in translations) {
            if((t.translationType & GamepadBindingTranslation.TranslationType.JoystickMask)
            == GamepadBindingTranslation.TranslationType.JoystickHat && t.joystickHat == currentIndex) {
                return GetGamepadLabel(t, (short)currentValue);
            }
        }
        return "???";
    }

    private static string GetGamepadLabel(GamepadBindingTranslation translation, short value) {
        switch(translation.translationType & GamepadBindingTranslation.TranslationType.GamepadMask) {
        case GamepadBindingTranslation.TranslationType.GamepadAxis:
            return value switch {
                > 0 => $"{translation.gamepadAxis.ToString()}+",
                < 0 => $"{translation.gamepadAxis.ToString()}-",
                _ => translation.gamepadAxis.ToString()
            };
        case GamepadBindingTranslation.TranslationType.GamepadButton:
            return translation.buttonLabel ?? translation.gamepadButton.ToString();
        case GamepadBindingTranslation.TranslationType.GamepadHat:
            return ((HatState)translation.joystickHatMask).ToString();
        default:
            return translation.ToString();
        }
    }
}
