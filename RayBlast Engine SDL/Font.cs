using SDL3;

namespace RayBlast;

public class Font : IDisposable {
	internal readonly IntPtr fontPtr;
	internal IntPtr textPtr;
	public readonly float baseSize;

	public Font(Uri filePath, float ptSize = 48f) {
		UnmanagedManager.AssertMainThread();
		baseSize = ptSize;
		fontPtr = TTF.OpenFont(filePath.LocalPath, ptSize);
		if(fontPtr == IntPtr.Zero)
			throw new RayBlastEngineException($"Failed to load font: {SDL.GetError()}");
	}

	private void ReleaseUnmanagedResources() {
		SDL.Free(fontPtr);
	}

	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~Font() {
		ReleaseUnmanagedResources();
	}
}
