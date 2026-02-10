using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using Cysharp.Text;
using RayBlast.Text;
using SDL3;

namespace RayBlast;

public static unsafe class BatchMode3D {
	private static Texture? currentTexture;

	//TODO: Switch to arrays to avoid unnecessary capacity checks
	private static readonly List<float> XY = [];
	private static readonly List<SDL.FColor> COLORS = [];
	private static readonly List<float> UV = [];
	private static readonly List<int> INDICES = [];

	static BatchMode3D() {
	}

	public static void StartBatch(Texture texture) {
		UnmanagedManager.AssertMainThread();
		if(currentTexture != null)
			Debug.LogWarning("Batch aborted");
		currentTexture = texture;
		XY.Clear();
		COLORS.Clear();
		UV.Clear();
		INDICES.Clear();
	}


	public static void Render() {
		UnmanagedManager.AssertMainThread();
		if(currentTexture == null)
			throw new RayBlastEngineException("Batch not started");
		if(RayBlastEngine.renderer == IntPtr.Zero)
			throw new RayBlastEngineException("Renderer not initialized");
		bool success = SDL.RenderGeometryRaw(RayBlastEngine.renderer, currentTexture.internalTexture,
											 CollectionsMarshal.AsSpan(XY), 3 * sizeof(float),
											 CollectionsMarshal.AsSpan(COLORS), sizeof(SDL.FColor),
											 CollectionsMarshal.AsSpan(UV), 2 * sizeof(float), XY.Count / 3,
											 CollectionsMarshal.AsSpan(INDICES), INDICES.Count, sizeof(int));
		if(!success)
			Debug.LogError($"Failed to render 3D batch: {SDL.GetError()}", includeStackTrace: false);
		currentTexture = null;
	}

	public static void DrawRectangle(float x, float y,
									 float width, float height,
									 ColorF color) {
		UnmanagedManager.AssertMainThread();
		throw new NotImplementedException();
	}

	public static void DrawRectangleOutline(float x, float y,
											float width, float height,
											uint outlineSize, ColorF color) {
		if(outlineSize >= width / 2f || outlineSize >= height / 2f) {
			DrawRectangle(x, y, width, height, color);
		}
		else {
			UnmanagedManager.AssertMainThread();
			throw new NotImplementedException();
		}
	}

	public static void DrawTriangle(Vector3 a, Vector3 b,
									  Vector3 c, ColorF color) {
		UnmanagedManager.AssertMainThread();
		throw new NotImplementedException();
	}

	public static void DrawQuad(Vector3 origin, Quaternion rotation,
								  Vector2 size, ColorF color) {
		UnmanagedManager.AssertMainThread();
		Vector3 a = origin + Vector3.Transform(new Vector3(-size.X / 2f, size.Y / 2f, 0f), rotation);
		Vector3 b = origin + Vector3.Transform(new Vector3(size.X / 2f, size.Y / 2f, 0f), rotation);
		Vector3 c = origin + Vector3.Transform(new Vector3(-size.X / 2f, -size.Y / 2f, 0f), rotation);
		Vector3 d = origin + Vector3.Transform(new Vector3(size.X / 2f, -size.Y / 2f, 0f), rotation);
		throw new NotImplementedException();
	}

	public static void DrawRectangularPrism(Vector3 origin, Quaternion rotation,
											  Vector3 dimensions, ColorF color) {
		UnmanagedManager.AssertMainThread();
		Vector3 a = origin + Vector3.Transform(new Vector3(dimensions.X / 2f, -dimensions.Y / 2f, -dimensions.Z / 2f), rotation);
		Vector3 b = origin + Vector3.Transform(new Vector3(-dimensions.X / 2f, -dimensions.Y / 2f, -dimensions.Z / 2f), rotation);
		Vector3 c = origin + Vector3.Transform(new Vector3(dimensions.X / 2f, -dimensions.Y / 2f, dimensions.Z / 2f), rotation);
		Vector3 d = origin + Vector3.Transform(new Vector3(-dimensions.X / 2f, -dimensions.Y / 2f, dimensions.Z / 2f), rotation);
		Vector3 e = origin + Vector3.Transform(new Vector3(dimensions.X / 2f, dimensions.Y / 2f, -dimensions.Z / 2f), rotation);
		Vector3 f = origin + Vector3.Transform(new Vector3(-dimensions.X / 2f, dimensions.Y / 2f, -dimensions.Z / 2f), rotation);
		Vector3 g = origin + Vector3.Transform(new Vector3(-dimensions.X / 2f, dimensions.Y / 2f, dimensions.Z / 2f), rotation);
		Vector3 h = origin + Vector3.Transform(new Vector3(dimensions.X / 2f, dimensions.Y / 2f, dimensions.Z / 2f), rotation);
		Span<Vector3> array = stackalloc Vector3[14] {
			d, c, g, h, e, c, a, d, b, g,
			f, e, b, a
		};
		fixed(Vector3* points = array) {
			UnmanagedManager.AssertMainThread();
			throw new NotImplementedException();
		}
	}

	public static void DrawImage(int x, int y,
								 ColorF tint) {
		UnmanagedManager.AssertMainThread();
		throw new NotImplementedException();
	}

	public static void DrawSubimage(TextureSubimage subimage, Vector2 destination,
									ColorF tint) {
		DrawSubimage(subimage, subimage.rectangle with {
						 Z = destination.X, W = destination.Y
					 }, new Vector2(0.5f, 0.5f), Quaternion.Identity,
					 tint);
	}

	public static void DrawSubimage(TextureSubimage subimage, Vector4 destination,
									ColorF tint) {
		DrawSubimage(subimage, destination, new Vector2(0.5f, 0.5f), Quaternion.Identity, tint);
	}

	public static void DrawSubimage(TextureSubimage subimage, Vector4 destination,
									Vector2 origin, ColorF tint) {
		DrawSubimage(subimage, destination, origin, Quaternion.Identity, tint);
	}

	public static void DrawSubimage(TextureSubimage subimage, Vector4 destination,
									Vector2 origin, Quaternion rotation,
									ColorF tint) {
		origin *= new Vector2(destination.Z, destination.W);
		destination.X -= origin.X;
		destination.Y -= origin.Y;
		if(subimage.texture != currentTexture)
			throw new RayBlastEngineException("Mismatched texture for render batch");
		UnmanagedManager.AssertMainThread();
		throw new NotImplementedException();
	}

	public static void DrawLine(Vector3 start, Vector3 end,
								  ColorF color) {
		UnmanagedManager.AssertMainThread();
		throw new NotImplementedException();
	}
}
