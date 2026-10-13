using System.Runtime.InteropServices;

namespace RayBlast;

[StructLayout(LayoutKind.Explicit)]
public struct InputKeyEvent(
    Key key,
    bool down,
    double timestamp,
    uint windowID) {
    [FieldOffset(0)]
    public byte state = (byte)(down ? 1 : 0);
    [FieldOffset(2)]
    public Key key = key;
    [FieldOffset(4)]
    public uint windowID = windowID;
    [FieldOffset(8)]
    public double time = timestamp;

    public readonly bool IsDown => (state & 1) != 0;

    public readonly override string ToString() {
        return IsDown ? $"{key} down @ {time}" : $"{key} up @ {time}";
    }
}
