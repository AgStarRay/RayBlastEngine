using System.Runtime.InteropServices;

namespace RayBlast;

[StructLayout(LayoutKind.Explicit)]
public struct InputJoystickButtonEvent(
    uint joystickID,
    short joystickButton,
    bool down,
    double timestamp) {
    [FieldOffset(0)]
    public uint joystickID = joystickID;
    [FieldOffset(4)]
    public byte state = (byte)(down ? 1 : 0);
    [FieldOffset(6)]
    public short joystickButton = joystickButton;
    [FieldOffset(8)]
    public double time = timestamp;

    public readonly bool IsDown => (state & 1) != 0;

    public readonly override string ToString() {
        return $"index {joystickID} button {joystickButton} {(IsDown ? "down" : "up")} @ {time}";
    }
}
