namespace RayBlast.Text;

public class RichTextColorTag : RichTextTag {
    private static readonly Dictionary<char, int> HEX_VALUES = new() {
        {
            '0', 0
        }, {
            '1', 1
        }, {
            '2', 2
        }, {
            '3', 3
        }, {
            '4', 4
        }, {
            '5', 5
        }, {
            '6', 6
        }, {
            '7', 7
        }, {
            '8', 8
        }, {
            '9', 9
        }, {
            'a', 10
        }, {
            'b', 11
        }, {
            'c', 12
        }, {
            'd', 13
        }, {
            'e', 14
        }, {
            'f', 15
        }, {
            'A', 10
        }, {
            'B', 11
        }, {
            'C', 12
        }, {
            'D', 13
        }, {
            'E', 14
        }, {
            'F', 15
        }
    };

    public override string StartTag => "<color=";
    public override string EndTag => ">";
    public override bool ProducesCommand => true;

    public override IRichTextCommand GetCommand(ReadOnlySpan<char> substring) {
        ColorF newColor = ColorF.WHITE;
        if(substring[0] == '#') {
            var validHex = true;
            foreach(char c in substring[1..]) {
                if(c is (< '0' or > '9') and (< 'a' or > 'f') and (< 'A' or > 'F')) {
                    validHex = false;
                    break;
                }
            }
            if(validHex) {
                newColor = substring.Length switch {
                    9 => new ColorF(HEX_VALUES[substring[1]] * (16f / 255f) + HEX_VALUES[substring[2]] / 255f,
                                    HEX_VALUES[substring[3]] * (16f / 255f) + HEX_VALUES[substring[4]] / 255f,
                                    HEX_VALUES[substring[5]] * (16f / 255f) + HEX_VALUES[substring[6]] / 255f,
                                    HEX_VALUES[substring[7]] * (16f / 255f) + HEX_VALUES[substring[8]] / 255f),
                    7 => new ColorF(HEX_VALUES[substring[1]] * (16f / 255f) + HEX_VALUES[substring[2]] / 255f,
                                    HEX_VALUES[substring[3]] * (16f / 255f) + HEX_VALUES[substring[4]] / 255f,
                                    HEX_VALUES[substring[5]] * (16f / 255f) + HEX_VALUES[substring[6]] / 255f, 1f),
                    5 => new ColorF(HEX_VALUES[substring[1]] / 15f, HEX_VALUES[substring[2]] / 15f,
                                    HEX_VALUES[substring[3]] / 15f, HEX_VALUES[substring[4]] / 15f),
                    4 => new ColorF(HEX_VALUES[substring[1]] / 15f, HEX_VALUES[substring[2]] / 15f, HEX_VALUES[substring[3]] / 15f, 1f),
                    _ => newColor
                };
            }
        }
        else {
            if(substring[0] == '"' && substring[^1] == '"') {
                substring = substring[1..^1];
            }
            Span<char> lower = stackalloc char[substring.Length];
            substring.ToLowerInvariant(lower);
            newColor = lower switch {
                "red" => new ColorF(1f, 0f, 0f),
                "orange" => new ColorF(1f, 0.5f, 0f),
                "yellow" => new ColorF(1f, 1f, 0f),
                "lime" => new ColorF(0.5f, 1f, 0f),
                "green" => new ColorF(0f, 1f, 0f),
                "mint" => new ColorF(0f, 1f, 0.5f),
                "cyan" => new ColorF(0f, 1f, 1f),
                "iceblue" => new ColorF(0f, 0.5f, 1f),
                "blue" => new ColorF(0f, 0f, 1f),
                "purple" => new ColorF(0.5f, 0f, 1f),
                "magenta" => new ColorF(1f, 0f, 1f),
                "rose" => new ColorF(1f, 0f, 0.5f),
                "white" => new ColorF(1f, 1f, 1f),
                "gray" => new ColorF(0.5f, 0.5f, 0.5f),
                "black" => new ColorF(0f, 0f, 0f),
                "brown" => new ColorF(0.5f, 0.25f, 0f),
                "pink" => new ColorF(1f, 0.5f, 0.75f),
                _ => newColor
            };
        }
        return new RichTextColorCommand(newColor);
    }
}
