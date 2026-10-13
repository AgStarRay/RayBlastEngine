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

    public void Flush() {
        resampler.Reset();
    }

    public int Read(Span<float> buffer) {
        int count = buffer.Length;
        resampler.SetRates(source.WaveFormat.SampleRate * pitch, WaveFormat.SampleRate);
        int num1 = count / channels;
        int num2 = resampler.ResamplePrepare(num1, outFormat.Channels, out Span<float> inbuffer);
        int inSamples = source.Read(inbuffer[..(num2 * channels)]) / channels;
        int resampleOut = resampler.ResampleOut(buffer, inSamples, num1, channels) * channels;
        //TODO: Check fftFrameSize and osamp parameters
        shifter?.PitchShift(1f, count, 2048L, 10L, WaveFormat.SampleRate, buffer);
        return resampleOut;
    }

    //public int Read(Span<float> buffer) {
    //     resampler.SetRates(source.WaveFormat.SampleRate * pitch, WaveFormat.SampleRate);
    //   int num1 = buffer.Length / channels;
    //   int num2 = resampler.ResamplePrepare(num1, outFormat.Channels, out float[] inbuffer, out int inbufferOffset);
    //   int inSamples = source.Read(inbuffer, inbufferOffset, num2 * channels) / channels;
    //   int resampleOut = resampler.ResampleOut(buffer, inSamples, num1, channels) * channels;
    //   shifter?.PitchShift(1f, buffer.Length, WaveFormat.SampleRate, buffer);
    //return resampleOut;
    //}

    public WaveFormat WaveFormat => outFormat;
}
