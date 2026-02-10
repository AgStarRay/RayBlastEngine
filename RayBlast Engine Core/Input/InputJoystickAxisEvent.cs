using System.Runtime.InteropServices;

namespace RayBlast;

[StructLayout(LayoutKind.Explicit)]
public struct InputJoystickAxisEvent(
    uint joystickID,
    short joystickAxis,
    float axisValue,
    double timestamp) {
    [FieldOffset(0)]
    public uint joystickID = joystickID;
    [FieldOffset(16)]
    public short joystickAxis = joystickAxis;
    [FieldOffset(4)]
    public float joystickAxisValue = axisValue;
    [FieldOffset(8)]
    public double time = timestamp;

    public readonly override string ToString() {
        return $"index {joystickID} axis {joystickAxis} = {joystickAxisValue} @ {time}";
    }
}
