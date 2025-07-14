using System.Numerics;
using System.Text;
using Cysharp.Text;

namespace RayBlast.Text;

//TODO: Add dynamic gradient tags with alpha channel support
public static class RichTextRendering {
    private static readonly UTF8Encoding UTF8_NO_BOM = new(false);
    private static readonly RichTextBatch BATCH = new();

    public static void DrawText(ReadOnlySpan<char> text, Vector2 renderPivot,
                                float fontSize, ColorF color,
                                SDFFontInstance fontInstance) {
        DrawText(text, renderPivot, fontSize, color, TextAlignmentFlags.Center, fontInstance, Array.Empty<RichTextLayout>(),
                 Array.Empty<IRichTextCommand>());
    }

    public static void DrawText(StringBuilder text, Vector2 renderPivot,
                                float fontSize, ColorF color,
                                TextAlignmentFlags alignment, SDFFontInstance fontInstance) {
        using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();
        StringBuilder.ChunkEnumerator chunks = text.GetChunks();
        foreach(ReadOnlyMemory<char> c in chunks) {
            builder.Append(c.Span);
        }
        ReadOnlySpan<char> readOnlySpan = builder.AsSpan();
        DrawText(readOnlySpan, renderPivot, fontSize, color, alignment, fontInstance, Array.Empty<RichTextLayout>(), Array.Empty<IRichTextCommand>());
    }

    public static void DrawText(ReadOnlySpan<char> span, Vector2 renderPivot,
                                float fontSize, ColorF color,
                                TextAlignmentFlags alignment, SDFFontInstance fontInstance) {
        DrawText(span, renderPivot, fontSize, color, alignment, fontInstance, Array.Empty<RichTextLayout>(), Array.Empty<IRichTextCommand>());
    }

    public static unsafe void DrawText(Utf8ValueStringBuilder builder, Vector2 renderPivot,
                                       float fontSize, ColorF color,
                                       TextAlignmentFlags alignment, SDFFontInstance fontInstance) {
        int charCount = UTF8_NO_BOM.GetCharCount(builder.AsSpan());
        Span<char> span = stackalloc char[Math.Min(charCount, 65535)];
        UTF8_NO_BOM.GetChars(builder.AsSpan(), span);
        DrawText(span, renderPivot, fontSize, color, alignment, fontInstance, Array.Empty<RichTextLayout>(), Array.Empty<IRichTextCommand>());
    }

    public static void DrawText(ReadOnlySpan<char> text, Vector2 renderPivot,
                                float fontSize, ColorF color,
                                SDFFontInstance fontInstance,
                                ReadOnlySpan<RichTextLayout> layouts, ReadOnlySpan<IRichTextCommand> commands) {
        DrawText(text, renderPivot, fontSize, color, TextAlignmentFlags.Center, fontInstance, layouts, commands);
    }

    public static void DrawText(StringBuilder text, Vector2 renderPivot,
                                float fontSize, ColorF color,
                                TextAlignmentFlags alignment, SDFFontInstance fontInstance,
                                ReadOnlySpan<RichTextLayout> layouts, ReadOnlySpan<IRichTextCommand> commands) {
        using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();
        StringBuilder.ChunkEnumerator chunks = text.GetChunks();
        foreach(ReadOnlyMemory<char> c in chunks) {
            builder.Append(c.Span);
        }
        ReadOnlySpan<char> readOnlySpan = builder.AsSpan();
        DrawText(readOnlySpan, renderPivot, fontSize, color, alignment, fontInstance, layouts, commands);
    }

    public static void DrawText(Utf8ValueStringBuilder builder, Vector2 renderPivot,
                                float fontSize, ColorF color,
                                TextAlignmentFlags alignment, SDFFontInstance fontInstance,
                                ReadOnlySpan<RichTextLayout> layouts, ReadOnlySpan<IRichTextCommand> commands) {
        int charCount = UTF8_NO_BOM.GetCharCount(builder.AsSpan());
        Span<char> span = stackalloc char[Math.Min(charCount, 65535)];
        UTF8_NO_BOM.GetChars(builder.AsSpan(), span);
        DrawText(span, renderPivot, fontSize, color, alignment, fontInstance, layouts, commands);
    }

    public static void DrawText(StringBuilder text, Vector2 renderPivot,
                                float fontSize, ColorF color,
                                TextAlignmentFlags alignment, SDFFontInstance fontInstance,
                                ReadOnlySpan<RichTextTag> tags) {
        using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();
        StringBuilder.ChunkEnumerator chunks = text.GetChunks();
        foreach(ReadOnlyMemory<char> c in chunks) {
            builder.Append(c.Span);
        }
        ReadOnlySpan<char> readOnlySpan = builder.AsSpan();
        DrawText(readOnlySpan, renderPivot, fontSize, color, alignment, fontInstance, tags);
    }

    public static void DrawText(ReadOnlySpan<char> chars, Vector2 renderPivot,
                                float fontSize, ColorF color,
                                TextAlignmentFlags alignment, SDFFontInstance fontInstance,
                                ReadOnlySpan<RichTextTag> tags) {
        ReadOnlySpan<RichTextLayout> layouts = RichTextParsing.ParseLayouts(chars, tags, out ReadOnlySpan<char> parsedText);
        ReadOnlySpan<IRichTextCommand> commands = RichTextParsing.ParseCommands(parsedText, tags, out ReadOnlySpan<char> doubleParsedText);
        DrawText(doubleParsedText, renderPivot, fontSize, color, alignment, fontInstance, layouts, commands);
    }

    public static void DrawText(ReadOnlySpan<char> chars, Vector2 renderPivot,
                                float fontSize, ColorF color,
                                SDFFontInstance fontInstance, ReadOnlySpan<RichTextTag> tags) {
        ReadOnlySpan<RichTextLayout> layouts = RichTextParsing.ParseLayouts(chars, tags, out ReadOnlySpan<char> parsedText);
        ReadOnlySpan<IRichTextCommand> commands = RichTextParsing.ParseCommands(parsedText, tags, out ReadOnlySpan<char> doubleParsedText);
        DrawText(doubleParsedText, renderPivot, fontSize, color, TextAlignmentFlags.Center, fontInstance, layouts, commands);
    }

    public static void DrawText(ReadOnlySpan<char> chars, Vector2 renderPivot,
                                float fontSize, ColorF color,
                                TextAlignmentFlags alignment, SDFFontInstance fontInstance,
                                ReadOnlySpan<RichTextLayout> layouts, ReadOnlySpan<IRichTextCommand> commands) {
        UnmanagedManager.AssertMainThread();
        fontInstance.sdfFont.ReadyAtlas();
        Vector2 currentPosition = renderPivot;
        float sizeMultiplier = fontSize / fontInstance.sdfFont.font.baseSize;
        var lineCount = 0;
        var lineStartIndex = 0;
        var layoutIndex = 0;
        var alignmentOrigin = new Vector2(0f, 0.5f);
        if((alignment & TextAlignmentFlags.VTop) != 0)
            alignmentOrigin.Y = 0f;
        if((alignment & TextAlignmentFlags.VBottom) != 0)
            alignmentOrigin.Y = 1f;
        TextureSubimage blankImage = fontInstance.sdfFont.atlasGlyphs[' '];
        BATCH.Clear();
        for(var i = 0; i < chars.Length; i++) {
            while(layoutIndex < layouts.Length && i >= layouts[layoutIndex].characterIndex) {
                RichTextLayout layout = layouts[layoutIndex];
                switch(layout.type) {
                case LayoutType.FontSize:
                    if(layout.parameterB > 0f)
                        sizeMultiplier = layout.parameterA / fontInstance.sdfFont.font.baseSize;
                    else
                        sizeMultiplier *= layout.parameterA;
                    break;
                }
                layoutIndex++;
            }
            char c = chars[i];
            switch(c) {
            case '\n':
                CompleteLine();
                BATCH.Add(blankImage, new Vector4(), new ColorF());
                lineCount++;
                currentPosition = renderPivot with {
                    Y = currentPosition.Y + fontSize
                };
                break;
            case '\f':
            case '\r':
                BATCH.Add(blankImage, new Vector4(), new ColorF());
                break;
            case '\t':
                BATCH.Add(blankImage, new Vector4(), new ColorF());
                //TODO: Implement
                break;
            default:
                if(fontInstance.sdfFont.atlasGlyphs.TryGetValue(c, out TextureSubimage? subimage)
                || fontInstance.sdfFont.atlasGlyphs.TryGetValue('□', out subimage)
                || fontInstance.sdfFont.atlasGlyphs.TryGetValue('�', out subimage)
                || fontInstance.sdfFont.atlasGlyphs.TryGetValue('?', out subimage)) {
                    var dest = new Vector4(currentPosition.X, currentPosition.Y,
                                           subimage.rectangle.Z * sizeMultiplier, subimage.rectangle.W * sizeMultiplier);
                    BATCH.Add(subimage, dest, color);
                    currentPosition += new Vector2(subimage.rectangle.Z * sizeMultiplier, 0f);
                }
                else
                    Debug.LogWarning($"No glyph subimage for {c}");
                break;
            }
        }
        CompleteLine();
        var vOffset = 0f;
        if((alignment & TextAlignmentFlags.VMiddle) != 0)
            vOffset = -lineCount * fontSize / 2f;
        if((alignment & TextAlignmentFlags.VBottom) != 0)
            vOffset = -lineCount * fontSize;
        for(var i = 0; i < BATCH.dest.Count; i++) {
            Vector4 newDest = BATCH.dest[i];
            newDest.Y += vOffset;
            BATCH.dest[i] = newDest;
        }
        // ReSharper disable once ForCanBeConvertedToForeach
        for(var i = 0; i < commands.Length; i++) {
            IRichTextCommand cmd = commands[i];
            cmd.Reset();
        }
        // ReSharper disable once ForCanBeConvertedToForeach
        for(var i = 0; i < commands.Length; i++) {
            IRichTextCommand cmd = commands[i];
            cmd.Edit(BATCH);
        }
        BATCH.Render(fontInstance, alignmentOrigin);
        return;

        void CompleteLine() {
            float lineWidth = currentPosition.X - renderPivot.X;
            var offset = 0f;
            if((alignment & TextAlignmentFlags.HCenter) != 0)
                offset = -lineWidth / 2f;
            if((alignment & TextAlignmentFlags.HRight) != 0)
                offset = -lineWidth;
            for(int i = lineStartIndex; i < BATCH.dest.Count; ++i) {
                Vector4 newDest = BATCH.dest[i];
                newDest.X += offset;
                BATCH.dest[i] = newDest;
            }
            lineStartIndex = BATCH.dest.Count;
        }
    }

    public static Vector2 MeasureTextSizeDelta(ReadOnlySpan<char> text, int fontSize,
                                               SDFFont sdfFont) {
        return MeasureTextSizeDelta(text, fontSize, sdfFont, Array.Empty<RichTextLayout>());
    }

    public static Vector2 MeasureTextSizeDelta(ReadOnlySpan<char> text, int fontSize,
                                               SDFFont sdfFont, ReadOnlySpan<RichTextTag> tags) {
        ReadOnlySpan<RichTextLayout> layouts = RichTextParsing.ParseLayouts(text, tags, out ReadOnlySpan<char> parsedText);
        return MeasureTextSizeDelta(parsedText, fontSize, sdfFont, layouts);
    }

    public static Vector2 MeasureTextSizeDelta(ReadOnlySpan<char> text, int fontSize,
                                               SDFFont sdfFont, ReadOnlySpan<RichTextLayout> layouts) {
        //TODO_URGENT: Use layouts to correctly measure text size
        Vector2 size = Vector2.Zero;
        Vector2 currentLineSize = Vector2.Zero;
        Vector2 firstLineSize = Vector2.Zero;
        float lineSeparation = fontSize;
        var lineCount = 0;
        float sizeMultiplier = fontSize / sdfFont.font.baseSize;
        foreach(char c in text) {
            if(c == '\n') {
                size.X = Math.Max(size.X, currentLineSize.X);
                if(lineCount == 0)
                    firstLineSize = currentLineSize;
                lineCount++;
                currentLineSize = Vector2.Zero;
            }
            else if(sdfFont.atlasGlyphs.TryGetValue(c, out TextureSubimage? subimage)) {
                currentLineSize.X += subimage.rectangle.Z * sizeMultiplier;
                currentLineSize.Y = Math.Max(currentLineSize.Y, subimage.rectangle.W * sizeMultiplier);
            }
        }
        size = new Vector2(Math.Max(size.X, currentLineSize.X), Math.Max(firstLineSize.Y, currentLineSize.Y) + lineCount * lineSeparation);
        return size;
    }
}
