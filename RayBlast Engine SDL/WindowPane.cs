namespace RayBlast;

public readonly record struct WindowPane(
    uint id,
    IntPtr window,
    IntPtr renderer) {
    public readonly uint id = id;
    internal readonly IntPtr window = window;
    internal readonly IntPtr renderer = renderer;
    public bool Visible => window != IntPtr.Zero && renderer != IntPtr.Zero;
}
