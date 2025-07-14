using System.Buffers;
using System.Numerics;
using Cysharp.Text;
using RayBlast;
using RayBlast.Text;

public static class Program {
    private const string TEST_STRING = "Allocation-free rich text rendering with effects starting at different positions";
    private static readonly TextureSubimage[] SKIN = new TextureSubimage[45];
    private static readonly List<Vector2> POSITIONS = new(131072);
    private static readonly List<Vector2> VELOCITIES = new(131072);
    private static readonly RichTextLayout[] TEXT_LAYOUTS = [
        new(15, LayoutType.FontSize, 0.5f),
        new(64, LayoutType.FontSize, 2f)
    ];
    private static readonly RichTextRainbowCommand RICH_TEXT_RAINBOW_COMMAND =
        new() {
            CharacterIndex = 5
        };
    private static readonly RichTextWaveCommand RICH_TEXT_WAVE_COMMAND =
        new() {
            CharacterIndex = 10, EndIndex = 99
        };
    private static readonly IRichTextCommand[] WAVE_COMMANDS = [
        RICH_TEXT_WAVE_COMMAND,
        RICH_TEXT_RAINBOW_COMMAND
    ];
    private static readonly StandardFormat DOUBLE_DECIMAL_FORMAT = StandardFormat.Parse("N2");
    
    private static Texture texture = null!;
    private static string renderingMode = "Z";
    private static int renderLimit = 500;
    private static SDFFontInstance fontOutline;

    public static void Main() {
        RayBlastEngine.windowTitle = "Trial";
        RayBlastEngine.Preinitialize();
        if(Graphics.CurrentDeviceResolution.width > 2000 && Graphics.CurrentDeviceResolution.height > 1100)
            RayBlastEngine.Initialize(1920, 1080);
        else
            RayBlastEngine.Initialize(1280, 720);
        var sdfFont = SDFFont.Create(RayBlastEngine.CreateResourceUri("Clear Sans.ttf"), 48f, 15);
        fontOutline = new SDFFontInstance(sdfFont, ColorF.BLACK, 0.5f, 0.8f);
        AudioSettings.ResetDSP(16, 16, AudioSettings.SpeakerMode.Stereo);
        texture = RayBlastTextureHttp.CreateTextureGet(RayBlastEngine.CreateResourceUri("skinPyramid.png")).GetTexture();
        for(int i = 0; i < 3; i++) {
            for(int j = 0; j < 15; j++) {
                SKIN[i * 15 + j] = new TextureSubimage(texture, new Vector4(j * 32, i * 32, 32, 32));
            }
        }
        Graphics.VSyncCount = 0;
        RayBlastEngine.Run(Update, Render);
    }

    private static void Update() {
        if(RayBlastEngine.UserRequestedClose) {
            RayBlastEngine.StopApplication();
        }
        if(Input.IsKeyPressed(Key.Down)) {
            renderLimit = (int)Math.Ceiling(renderLimit * 0.66 + 1);
        }
        if(Input.IsKeyPressed(Key.Up)) {
            renderLimit = (int)Math.Ceiling(renderLimit * 1.33 + 1);
        }
        if(Input.IsKeyPressed(Key.Z))
            renderingMode = "Z";
        if(Input.IsKeyPressed(Key.X))
            renderingMode = "X";
        if(Input.IsKeyPressed(Key.C))
            renderingMode = "C";
        if(Input.IsKeyPressed(Key.V))
            renderingMode = "V";
        while(POSITIONS.Count < renderLimit) {
            POSITIONS.Add(FastRNG.UnsignedVector2 * new Vector2(1280, 720));
        }
        while(VELOCITIES.Count < renderLimit) {
            VELOCITIES.Add(FastRNG.Vector2 * new Vector2(240, 240));
        }
        for(int i = 0; i < renderLimit; i++) {
            Vector2 velocity = VELOCITIES[i];
            Vector2 position = POSITIONS[i];
            position += velocity * (float)Time.deltaTime;
            POSITIONS[i] = position;
            if((velocity.X > 0f && position.X > 1280f) || (velocity.X < 0f && position.X < 0f))
                VELOCITIES[i] *= new Vector2(-1f, 1f);
            if((velocity.Y > 0f && position.Y > 720f) || (velocity.Y < 0f && position.Y < 0f))
                VELOCITIES[i] *= new Vector2(1f, -1f);
        }
    }

    private static void Render() {
        ImmediateMode.End3DMode();
        ImmediateMode.ClearBackground(new Color32(39, 47, 55));
        if(POSITIONS.Count >= renderLimit) {
            switch(renderingMode) {
            case "C":
                BatchMode2D.StartBatch(texture);
                for(int i = 0; i < renderLimit; i++) {
                    var rotation = Quaternion.CreateFromYawPitchRoll((float)Time.unscaledTime, (float)Time.unscaledTime,
                                                                             (float)Time.unscaledTime);
                    BatchMode2D.DrawSubimage(SKIN[i % SKIN.Length], new Vector4(POSITIONS[i].X, POSITIONS[i].Y, 64f, 64f), new Vector2(0.5f, 0.5f),
                                             rotation, ColorF.WHITE);
                }
                BatchMode2D.Render();
                break;
            case "X":
                BatchMode2D.StartBatch(texture);
                for(int i = 0; i < renderLimit; i++) {
                    BatchMode2D.DrawSubimage(SKIN[i % SKIN.Length], POSITIONS[i], ColorF.WHITE);
                }
                BatchMode2D.Render();
                break;
            default:
                for(int i = 0; i < renderLimit; i++) {
                    ImmediateMode.DrawSubimage(SKIN[i % SKIN.Length], POSITIONS[i], Color32.WHITE);
                }
                break;
            }
        }
        using Utf8ValueStringBuilder builder = ZString.CreateUtf8StringBuilder();
        builder.AppendLine(TEST_STRING);
        builder.AppendLine();
        builder.Append("FPS: ");
        builder.Append(Time.FPS, DOUBLE_DECIMAL_FORMAT);
        builder.AppendLine();
        RICH_TEXT_WAVE_COMMAND.EndIndex = builder.Length;
        builder.Append("U: ");
        builder.Append(Time.AverageUpdateTime * 1000.0, DOUBLE_DECIMAL_FORMAT);
        builder.Append(" ms, R: ");
        builder.Append(Time.AverageRenderTime * 1000.0, DOUBLE_DECIMAL_FORMAT);
        builder.Append(" ms, O: ");
        builder.Append(Time.AverageOverheadTime * 1000.0, DOUBLE_DECIMAL_FORMAT);
        builder.AppendLine(" ms");
        builder.Append(renderLimit);
        builder.Append(" sprites ");
        builder.AppendLine(renderingMode);
        RichTextRendering.DrawText(builder, new Vector2(Graphics.CurrentResolution.X / 2f, Graphics.CurrentResolution.Y * 0.8f), 48f, ColorF.WHITE,
                                   TextAlignmentFlags.Center, fontOutline,
                                   TEXT_LAYOUTS, WAVE_COMMANDS);
        #if RAYBLAST_DEBUG
        RayBlastEngine.AddDebugGraphics();
        #endif
    }
}
