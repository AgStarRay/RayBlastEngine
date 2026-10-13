using System.Runtime.InteropServices;

namespace RayBlast;

[StructLayout(LayoutKind.Explicit)]
public struct InputMouseEvent(
    MouseCode mouseCode,
    bool down,
    float x,
    float y,
    double time,
    uint windowID) {
    [FieldOffset(0)]
    public byte state = (byte)(down ? 1 : 0);
    [FieldOffset(1)]
    public MouseCode mouseCode = mouseCode;
    [FieldOffset(4)]
    public uint windowID = windowID;
    [FieldOffset(8)]
    public double time = time;
    [FieldOffset(16)]
    public float x = x;
    [FieldOffset(20)]
    public float y = y;

    public readonly bool IsDown => (state & 1) != 0;

    public readonly override string ToString() {
        return IsDown ? $"{mouseCode} down @ {time} on window {windowID}" : $"{mouseCode} up @ {time} on window {windowID}";
    }
}
