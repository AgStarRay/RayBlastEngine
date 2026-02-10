namespace RayBlast;

[Flags]
public enum HatState : byte {
    Neutral = 0x00,
    Up = 0x01,
    Right = 0x02,
    Down = 0x04,
    Left = 0x08,
    UpLeft = Up | Left,
    UpRight = Up | Right,
    DownLeft = Down | Left,
    DownRight = Down | Right
}
