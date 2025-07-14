using System.Numerics;
using Cysharp.Text;
using RayBlast.Text;

namespace RayBlast.Composer;

/// <summary>
/// Displays debug info for the music controller.
/// </summary>
public class MusicDebugInfo {
    /// <summary>
    /// Current state of the music debug panel.
    /// </summary>
    public DebugInfo debugInfo = DebugInfo.Hide;

    public void Render(SDFFontInstance fontInstance) {
        if(Input.IsKeyDown(Key.F3)) {
            debugInfo = debugInfo switch {
                DebugInfo.Hide => DebugInfo.Short,
                DebugInfo.Short => DebugInfo.Extended,
                _ => DebugInfo.Hide
            };
        }

        if(debugInfo != DebugInfo.Hide) {
            using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();
            switch(debugInfo) {
            case DebugInfo.Short:
                builder.Clear();
                builder.Append("♫ ");
                builder.Append(BankPlayer.MAIN.Artist);
                builder.Append(" - ");
                builder.Append(BankPlayer.MAIN.Title);
                builder.Append(" ♫\npos: ");
                builder.Append(Utils.TimeString(BankPlayer.MAIN.CurrentTime));
                builder.Append(" / ");
                builder.Append(Utils.TimeString(BankPlayer.MAIN.ClipLength));
                if(BankPlayer.MAIN.CurrentBank.stageLoops.Length > 1)
                    builder.Append(", stage ");
                builder.Append(BankPlayer.MAIN.stage.CultureString());
                builder.Append("\nbeat: ");
                builder.Append(BeatTracker.Measure.CultureString());
                builder.Append(":");
                builder.Append(BeatTracker.Beat.CultureString());
                builder.Append(":");
                builder.Append(BeatTracker.Tick.CultureString());
                builder.Append(" / ");
                builder.Append(BeatTracker.MaximumMeasure.CultureString());
                builder.Append(":");
                builder.Append(BeatTracker.FinalBeat.CultureString());
                builder.Append(":");
                builder.Append(BeatTracker.FinalTick.CultureString());
                builder.Append(", ");
                builder.Append(BeatTracker.BPM.CultureString("0.##"));
                builder.Append(" bpm");
                Vector2 sizeDelta = RichTextRendering.MeasureTextSizeDelta(builder.AsSpan(), 18, fontInstance.sdfFont);
                RichTextRendering.DrawText(builder.AsSpan(), new Vector2(0, Graphics.CurrentResolution.Y), 18, ColorF.WHITE,
                                           TextAlignmentFlags.BottomLeft, fontInstance);
                // debugTextFieldUGUI.enabled = true;
                // debugBack.enabled = true;
                break;
            case DebugInfo.Extended:
                builder.Clear();
                builder.Append("♫ ");
                builder.Append(BankPlayer.MAIN.Artist);
                builder.Append(" --- ");
                builder.Append(BankPlayer.MAIN.Album);
                builder.Append(" --- ");
                builder.Append(BankPlayer.MAIN.Title);
                builder.Append(" ♫\npos: ");
                builder.Append(Utils.TimeString(BankPlayer.MAIN.CurrentTime));
                builder.Append(" / ");
                builder.Append(Utils.TimeString(BankPlayer.MAIN.ClipLength));
                builder.Append(", stage ");
                builder.Append(BankPlayer.MAIN.stage.CultureString());
                builder.Append("\nbeat: ");
                builder.Append(BeatTracker.Measure.CultureString());
                builder.Append(":");
                builder.Append(BeatTracker.Beat.CultureString());
                builder.Append(":");
                builder.Append(BeatTracker.Tick.CultureString());
                builder.Append(" / ");
                builder.Append(BeatTracker.MaximumMeasure.CultureString());
                builder.Append(":");
                builder.Append(BeatTracker.FinalBeat.CultureString());
                builder.Append(":");
                builder.Append(BeatTracker.FinalTick.CultureString());
                builder.Append(", ");
                builder.Append(BeatTracker.BPM.CultureString("0.##"));
                builder.Append(" bpm, ");
                builder.Append(BeatTracker.CurrentSignature.CultureString());
                builder.Append(" ticks/measure\nnext jump from ");
                builder.Append(Utils.TimeString(BankPlayer.MAIN.NextJumpFrom));
                builder.Append(" to ");
                builder.Append(Utils.TimeString(BankPlayer.MAIN.NextJumpTo));
                builder.Append("\ndsp: ");
                builder.Append(Time.dspTime.CultureString("0.000"));
                // builder.Append(", jump dsp: ");
                // builder.Append(BankPlayer.MAIN.DSPNextTime.CultureString("0.000"));
                // if(BankPlayer.MAIN.looping) {
                //     builder.Append(", final dsp: ");
                //     builder.Append(BankPlayer.MAIN.FinalDSP.CultureString("0.000"));
                // }
                // else {
                //     builder.Append(", until ");
                //     builder.Append(BankPlayer.MAIN.PlayingUntil.CultureString("0.000"));
                // }
                builder.Append("\n ");
                sizeDelta = RichTextRendering.MeasureTextSizeDelta(builder.AsSpan(), 18, fontInstance.sdfFont);
                RichTextRendering.DrawText(builder.AsSpan(), new Vector2(0, Graphics.CurrentResolution.Y), 18, ColorF.WHITE,
                                           TextAlignmentFlags.BottomLeft, fontInstance);
                sizeDelta = Vector2.One * (12f + BeatTracker.WholeSawtooth * 5f);
                sizeDelta = Vector2.One * (7f + BeatTracker.HalfSawtooth * 5f);
                sizeDelta = Vector2.One * (2f + BeatTracker.QuarterSawtooth * 5f);

                // float barLength = 192f;
                // foreach(LoopSamplePoints lps in BankPlayer.MAIN.CurrentStage.loopSegments) {
                //     uint end = lps.endSample;
                //     if(end == 0f)
                //         end = (uint)BankPlayer.MAIN.SampleLength;
                //     var i = go.GetComponent<Image>();
                //     i.enabled = true;
                //     i.rectTransform.anchoredPosition = new Vector2(barLength * lps.startSample / BankPlayer.MAIN.ClipLength, 0f);
                //     i.rectTransform.sizeDelta =
                //         new Vector2(barLength * (end - lps.startSample) / BankPlayer.MAIN.ClipLength, i.rectTransform.sizeDelta.y);
                //     loopImages.Add(i);
                // }
                // foreach(SkipSamplePoints sps in BankPlayer.MAIN.CurrentStage.skipSegments) {
                //     var i = go.GetComponent<Image>();
                //     i.enabled = true;
                //     i.rectTransform.anchoredPosition = new Vector2(barLength * sps.fromSample / BankPlayer.MAIN.ClipLength, 0f);
                //     i.rectTransform.sizeDelta =
                //         new Vector2(barLength * (sps.toSample - sps.fromSample) / BankPlayer.MAIN.ClipLength, i.rectTransform.sizeDelta.y);
                //     loopImages.Add(i);
                // }
                //
                // barCover.rectTransform.anchoredPosition = new Vector2(192f, 0f);
                // barCover.rectTransform.sizeDelta = new Vector2((1f - BankPlayer.MAIN.CurrentTime / BankPlayer.MAIN.ClipLength) * barLength,
                //                                                barCover.rectTransform.sizeDelta.y);
                // debugTextFieldUGUI.enabled = true;
                // debugBack.enabled = true;
                // wholeCircle.enabled = true;
                // halfCircle.enabled = true;
                // quarterCircle.enabled = true;
                // barBack.enabled = true;
                // barCover.enabled = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(debugInfo));
            }
        }
    }

    /// <summary>
    /// States of the debug panel.
    /// </summary>
    public enum DebugInfo {
        Hide,
        Short,
        Extended
    }
}
