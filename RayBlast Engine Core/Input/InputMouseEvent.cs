using System.Runtime.InteropServices;

namespace RayBlast;

[StructLayout(LayoutKind.Explicit)]
public struct InputMouseEvent(
    MouseCode mouseCode,
    bool down,
    float x,
    float y,
    double time) {
    [FieldOffset(0)]
    public byte state = (byte)(down ? 1 : 0);
    [FieldOffset(1)]
    public MouseCode mouseCode = mouseCode;
    [FieldOffset(4)]
    public float x = x;
    [FieldOffset(16)]
    public float y = y;
    [FieldOffset(8)]
    public double time = time;

    public readonly bool IsDown => (state & 1) != 0;

    public readonly override string ToString() {
        return IsDown ? $"{mouseCode} down @ {time}" : $"{mouseCode} up @ {time}";
    }
}
