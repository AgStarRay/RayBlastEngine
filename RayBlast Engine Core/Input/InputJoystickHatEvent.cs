using System.Runtime.InteropServices;

namespace RayBlast;

[StructLayout(LayoutKind.Explicit)]
public struct InputJoystickHatEvent(
    uint joystickID,
    short joystickHat,
    HatState hatValue,
    double timestamp) {
    [FieldOffset(0)]
    public uint joystickID = joystickID;
    [FieldOffset(4)]
    public short joystickHat = joystickHat;
    [FieldOffset(6)]
    public HatState joystickHatValue = hatValue;
    [FieldOffset(8)]
    public double time = timestamp;

    public readonly override string ToString() {
        return $"index {joystickID} hat {joystickHat} = {joystickHatValue} @ {time}";
    }
}
