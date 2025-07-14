using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL3;

namespace RayBlast;

public class RayBlastTextureHttp : RayBlastHttp {
	private readonly Uri uri;

	private RayBlastTextureHttp(Uri uri) {
		this.uri = uri;
	}

	public override bool IsDone => uri.IsFile || base.IsDone;
	public override Result State => uri.IsFile ? Result.Success : base.State;
	public override ulong DownloadedByteCount => uri.IsFile ? (ulong)new FileInfo(uri.LocalPath).Length : base.DownloadedByteCount;
	public override float DownloadProgress => uri.IsFile ? 1f : base.DownloadProgress;

	public Texture GetTexture() {
		if(uri.IsFile) {
			var fi = new FileInfo(uri.LocalPath);
			if(!fi.Exists)
				throw new FileNotFoundException(null, uri.LocalPath);
			Debug.LogDebug($"Load Image {uri.LocalPath}");
			IntPtr internalImage = Image.Load(uri.LocalPath);
			return new Texture(internalImage);
		}
		throw new NotImplementedException();
	}

	public static RayBlastTextureHttp CreateTextureGet(Uri uri) {
		return new RayBlastTextureHttp(uri);
	}
}
