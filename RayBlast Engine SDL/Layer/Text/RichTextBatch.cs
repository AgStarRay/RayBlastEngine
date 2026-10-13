using System.Numerics;

namespace RayBlast.Text;

public class RichTextBatch {
	public readonly List<TextureSubimage> subimages = new();
	public readonly List<Vector4> dest = new();
	public readonly List<ColorF> colors = new();

	private readonly BatchMode2D batch2D = new();

	public void Clear() {
		subimages.Clear();
		dest.Clear();
		colors.Clear();
	}

	public void Add(TextureSubimage subimage, Vector4 dest,
					ColorF color) {
		subimages.Add(subimage);
		this.dest.Add(dest);
		colors.Add(color);
	}
 
	internal void Render(SDFFontInstance fontInstance, Vector2 baseOffset) {
		// Span<Color32> atlasColors = stackalloc Color32[256];
		// for(int i = 0; i < 256; i++) {
		// 	atlasColors[i] = new Color32 {
		// 		r = (byte)i, g = (byte)i, b = (byte)i, a = (byte)i
		// 	};
		// }
		// fontInstance.sdfFont.atlas.SetColors(atlasColors);
		batch2D.StartBatch(fontInstance.sdfFont.atlas);
		TextureSubimage blankImage = fontInstance.sdfFont.atlasGlyphs[' '];
		if(colors.Count > subimages.Count) {
			var colorIndex = 0;
			for(var i = 0; i < subimages.Count; i++) {
				if(subimages[i] != blankImage) {
					batch2D.DrawSubimage(subimages[i], dest[i], baseOffset, Quaternion.Identity, colors[colorIndex++], colors[colorIndex++],
										 colors[colorIndex++], colors[colorIndex++]);
				}
			}
		}
		else {
			for(var i = 0; i < subimages.Count; i++) {
				if(subimages[i] != blankImage) {
					batch2D.DrawSubimage(subimages[i], dest[i], baseOffset, colors[i]);
				}
			}
		}
		batch2D.Render();
	}
}
