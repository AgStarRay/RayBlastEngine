using NAudio.Wave;

namespace RayBlast;

public abstract class CustomSoundSource(int sampleRate, int channels) : ISoundSource, ISampleProvider {
    public abstract int Read(Span<float> buffer);

    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);

    ISampleProvider ISoundSource.SampleProvider => this;
    public string Name { get; protected init; } = "<unnamed>";
    public int Frequency => sampleRate;
}
