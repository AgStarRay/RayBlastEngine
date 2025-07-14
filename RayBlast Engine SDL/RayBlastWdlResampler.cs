using NAudio.Dsp;
using NAudio.Wave;

namespace RayBlast;

public class RayBlastWdlResampler : ISampleProvider {
  public float pitch = 1f;
  
  private readonly WdlResampler resampler;
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
  }

  /// <summary>Reads from this sample provider</summary>
  public int Read(float[] buffer, int offset, int count)
  {
    resampler.SetRates(source.WaveFormat.SampleRate * pitch, WaveFormat.SampleRate);
    int num1 = count / channels;
    int num2 = resampler.ResamplePrepare(num1, outFormat.Channels, out float[] inbuffer, out int inbufferOffset);
    int nsamples_in = source.Read(inbuffer, inbufferOffset, num2 * channels) / channels;
    return resampler.ResampleOut(buffer, offset, nsamples_in, num1, channels) * channels;
  }

  /// <summary>Output WaveFormat</summary>
  public WaveFormat WaveFormat => outFormat;
}
