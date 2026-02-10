using NAudio.Dsp;
using NAudio.Wave;

namespace RayBlast;

public class RayBlastWdlResampler : ISampleProvider {
    public float pitch = 1f;

    private readonly WdlResampler resampler;
    private SmbPitchShifter? shifter;
    private readonly WaveFormat outFormat;
    private readonly ISampleProvider source;
    private readonly int channels;

    public RayBlastWdlResampler(ISampleProvider source, int newSampleRate) {
        channels = source.WaveFormat.Channels;
        outFormat = WaveFormat.CreateIeeeFloatWaveFormat(newSampleRate, channels);
        this.source = source;
        resampler = new WdlResampler();
        resampler.SetMode(true, 2, false);
        resampler.SetFilterParms();
        resampler.SetFeedMode(false);
        resampler.SetRates(source.WaveFormat.SampleRate, newSampleRate);
        //SmbPitchShifter
    }

    public int Read(float[] buffer, int offset,
                    int count) {
        resampler.SetRates(source.WaveFormat.SampleRate * pitch, WaveFormat.SampleRate);
        int num1 = count / channels;
        int num2 = resampler.ResamplePrepare(num1, outFormat.Channels, out float[] inbuffer, out int inbufferOffset);
        int inSamples = source.Read(inbuffer, inbufferOffset, num2 * channels) / channels;
        int resampleOut = resampler.ResampleOut(buffer, offset, inSamples, num1, channels) * channels;
        shifter?.PitchShift(1f, count, WaveFormat.SampleRate, buffer);
        return resampleOut;
    }

    public WaveFormat WaveFormat => outFormat;
}
