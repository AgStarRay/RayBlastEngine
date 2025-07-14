using SDL3;

namespace RayBlast;

public static partial class Utils {
	public static string GetClipboardText() {
		return SDL.GetClipboardText();
	}

	public static void SetClipboardText(string text) {
		SDL.SetClipboardText(text);
	}

	public static void CaptureExceptions(Action methodCall) {
		try {
			methodCall();
		}
		catch(Exception e) {
			Debug.LogException(e);
		}
	}

	internal static MouseCode SDLToRayBlastMouseCode(byte sdlMouseCode) {
		return sdlMouseCode switch {
			1 => MouseCode.Left,
			2 => MouseCode.Middle,
			3 => MouseCode.Right,
			//TODO_URGENT: Check if correct
			4 => MouseCode.Forward,
			5 => MouseCode.Back,
			_ => throw new ArgumentOutOfRangeException(nameof(sdlMouseCode), sdlMouseCode, null)
		};
	}

	internal static byte RayBlastToSDLMouseCode(MouseCode mouseCode) {
		return mouseCode switch {
			MouseCode.Null => 0,
			MouseCode.Left => 1,
			MouseCode.Right => 3,
			MouseCode.Middle => 2,
			//TODO_URGENT: Check if correct
			MouseCode.Back => 5,
			MouseCode.Forward => 4,
			_ => throw new ArgumentOutOfRangeException(nameof(mouseCode), mouseCode, null)
		};
	}
}
