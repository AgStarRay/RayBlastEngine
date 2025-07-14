namespace RayBlast;

[Flags]
public enum TextAlignmentFlags {
    HLeft = 0x1,
    HCenter = 0x2,
    HRight = 0x4,
    HJustified = 0x8,
    VTop = 0x100,
    VMiddle = 0x200,
    VBottom = 0x400,

    Left = HLeft | VMiddle,
    Center = HCenter | VMiddle,
    Right = HRight | VMiddle,
    TopLeft = HLeft | VTop,
    TopRight = HRight | VTop,
    BottomLeft = HLeft | VBottom,
    BottomRight = HRight | VBottom,
    Justified = HJustified | VMiddle
}
