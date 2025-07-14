namespace RayBlast;

public struct InputEvent {
	public Key key;
	public MouseCode mouseCode;
	public byte state;
	public double time;

	public bool IsDown => (state & 1) != 0;

	public override string ToString() {
		if(mouseCode != MouseCode.Null)
			return IsDown ? $"{mouseCode} down @ {time}" : $"{mouseCode} up @ {time}";
		return IsDown ? $"{key} down @ {time}" : $"{key} up @ {time}";
	}
}
