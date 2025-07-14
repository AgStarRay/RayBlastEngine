using System.Runtime.InteropServices;

namespace RayBlast.Text;

internal static class RichTextParsing {
    private static RichTextLayout[] layouts = new RichTextLayout[1024];
    private static IRichTextCommand[] commands = new IRichTextCommand[2048];
    private static readonly List<char> OUT_TEXT = [];

    internal static ReadOnlySpan<RichTextLayout> ParseLayouts(ReadOnlySpan<char> text, ReadOnlySpan<RichTextTag> tags,
                                                              out ReadOnlySpan<char> parsedText) {
        var generatedLayouts = 0;
        OUT_TEXT.Clear();
        OUT_TEXT.AddRange(text);
        foreach(RichTextTag tag in tags) {
            if(!tag.ProducesLayout)
                continue;
            int index = CollectionsMarshal.AsSpan(OUT_TEXT).IndexOf(tag.StartTag);
            if(!string.IsNullOrEmpty(tag.EndTag)) {
                while(index != -1) {
                    int endIndex = CollectionsMarshal.AsSpan(OUT_TEXT)[(index + tag.StartTag.Length)..].IndexOf(tag.EndTag);
                    if(endIndex != -1) {
                        var safeToConsume = true;
                        for(var i = 0; i < generatedLayouts; i++) {
                            if(layouts[i].characterIndex >= index && layouts[i].characterIndex < endIndex) {
                                safeToConsume = false;
                                break;
                            }
                        }
                        if(safeToConsume) {
                            for(var i = 0; i < generatedLayouts; i++) {
                                if(layouts[i].characterIndex > index)
                                    layouts[i].characterIndex -= tag.StartTag.Length + endIndex + tag.EndTag.Length;
                            }
                            ReadOnlySpan<RichTextLayout> createdLayouts =
                                tag.GetLayouts(CollectionsMarshal.AsSpan(OUT_TEXT).Slice(index + tag.StartTag.Length, endIndex));
                            foreach(RichTextLayout newLayout in createdLayouts) {
                                if(generatedLayouts >= layouts.Length)
                                    Array.Resize(ref layouts, layouts.Length * 2);
                                RichTextLayout layout = newLayout;
                                layout.characterIndex = index;
                                layouts[generatedLayouts++] = layout;
                            }
                            OUT_TEXT.RemoveRange(index, tag.StartTag.Length + endIndex + tag.EndTag.Length);
                        }
                        else
                            index = endIndex + 1;
                        int newIndex = CollectionsMarshal.AsSpan(OUT_TEXT)[index..].IndexOf(tag.StartTag);
                        if(newIndex != -1)
                            index += newIndex;
                        else
                            index = -1;
                    }
                    else
                        index += tag.StartTag.Length;
                }
            }
            else {
                while(index != -1) {
                    var safeToConsume = true;
                    for(var i = 0; i < generatedLayouts; i++) {
                        if(layouts[i].characterIndex >= index && layouts[i].characterIndex < index + tag.StartTag.Length) {
                            safeToConsume = false;
                            break;
                        }
                    }
                    if(safeToConsume) {
                        for(var i = 0; i < generatedLayouts; i++) {
                            if(layouts[i].characterIndex > index)
                                layouts[i].characterIndex -= tag.StartTag.Length;
                        }
                        ReadOnlySpan<RichTextLayout> createdLayouts = tag.GetLayouts("");
                        foreach(RichTextLayout newLayout in createdLayouts) {
                            if(generatedLayouts >= layouts.Length)
                                Array.Resize(ref layouts, layouts.Length * 2);
                            RichTextLayout layout = newLayout;
                            layout.characterIndex = index;
                            layouts[generatedLayouts++] = layout;
                        }
                        OUT_TEXT.RemoveRange(index, tag.StartTag.Length);
                    }
                    else
                        index += tag.StartTag.Length;
                    int newIndex = CollectionsMarshal.AsSpan(OUT_TEXT)[index..].IndexOf(tag.StartTag);
                    if(newIndex != -1)
                        index += newIndex;
                    else
                        index = -1;
                }
            }
        }
        parsedText = CollectionsMarshal.AsSpan(OUT_TEXT);
        Span<RichTextLayout> newLayouts = layouts.AsSpan()[..generatedLayouts];
        newLayouts.Sort(static (layout1, layout2) => layout1.characterIndex.CompareTo(layout2.characterIndex));
        return newLayouts;
    }

    internal static Span<IRichTextCommand> ParseCommands(ReadOnlySpan<char> text, ReadOnlySpan<RichTextTag> tags,
                                                         out ReadOnlySpan<char> parsedText) {
        var generatedCommands = 0;
        OUT_TEXT.Clear();
        OUT_TEXT.AddRange(text);
        foreach(RichTextTag tag in tags) {
            if(!tag.ProducesCommand)
                continue;
            int index = CollectionsMarshal.AsSpan(OUT_TEXT).IndexOf(tag.StartTag);
            while(index != -1) {
                if(generatedCommands >= commands.Length)
                    Array.Resize(ref commands, commands.Length * 2);
                if(!string.IsNullOrEmpty(tag.EndTag)) {
                    int endIndex = CollectionsMarshal.AsSpan(OUT_TEXT)[(index + tag.StartTag.Length)..].IndexOf(tag.EndTag);
                    if(endIndex != -1) {
                        var safeToConsume = true;
                        for(var i = 0; i < generatedCommands; i++) {
                            if(commands[i].CharacterIndex > index && commands[i].CharacterIndex < endIndex) {
                                safeToConsume = false;
                                break;
                            }
                        }
                        if(safeToConsume) {
                            for(var i = 0; i < generatedCommands; i++) {
                                if(commands[i].CharacterIndex > index)
                                    commands[i].CharacterIndex -= tag.StartTag.Length + endIndex + tag.EndTag.Length;
                                commands[i].EndIndex -= tag.StartTag.Length + endIndex + tag.EndTag.Length;
                            }
                            IRichTextCommand newCommand =
                                tag.GetCommand(CollectionsMarshal.AsSpan(OUT_TEXT).Slice(index + tag.StartTag.Length, endIndex));
                            commands[generatedCommands++] = newCommand;
                            OUT_TEXT.RemoveRange(index, tag.StartTag.Length + endIndex + tag.EndTag.Length);
                            newCommand.CharacterIndex = index;
                            newCommand.EndIndex = OUT_TEXT.Count;
                        }
                        else
                            index = endIndex + 1;
                        int newIndex = CollectionsMarshal.AsSpan(OUT_TEXT)[index..].IndexOf(tag.StartTag);
                        if(newIndex != -1)
                            index += newIndex;
                        else
                            index = -1;
                    }
                    else
                        index += tag.StartTag.Length;
                }
                else {
                    var safeToConsume = true;
                    for(var i = 0; i < generatedCommands; i++) {
                        if(commands[i].CharacterIndex > index && commands[i].CharacterIndex < index + tag.StartTag.Length) {
                            safeToConsume = false;
                            break;
                        }
                    }
                    if(safeToConsume) {
                        for(var i = 0; i < generatedCommands; i++) {
                            if(commands[i].CharacterIndex > index)
                                commands[i].CharacterIndex -= tag.StartTag.Length;
                            commands[i].EndIndex -= tag.StartTag.Length;
                        }
                        IRichTextCommand newCommand = tag.GetCommand("");
                        commands[generatedCommands++] = newCommand;
                        OUT_TEXT.RemoveRange(index, tag.StartTag.Length);
                        newCommand.CharacterIndex = index;
                        newCommand.EndIndex = OUT_TEXT.Count;
                    }
                    else
                        index += tag.StartTag.Length;
                    int newIndex = CollectionsMarshal.AsSpan(OUT_TEXT)[index..].IndexOf(tag.StartTag);
                    if(newIndex != -1)
                        index += newIndex;
                    else
                        index = -1;
                }
            }
        }
        parsedText = CollectionsMarshal.AsSpan(OUT_TEXT);
        Span<IRichTextCommand> newCommands = commands.AsSpan()[..generatedCommands];
        newCommands.Sort(static (cmd1, cmd2) => cmd1.CharacterIndex.CompareTo(cmd2.CharacterIndex));
        return newCommands;
    }
}
