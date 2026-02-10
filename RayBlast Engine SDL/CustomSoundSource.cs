using NAudio.Wave;

namespace RayBlast;

public abstract class CustomSoundSource(int sampleRate, int channels) : ISoundSource, ISampleProvider {
    public abstract int Read(float[] buffer, int offset, int count);
    
    public WaveFormat WaveFormat => new(sampleRate, channels);
    
    ISampleProvider ISoundSource.SampleProvider => this;
    public string Name { get; protected init; } = "<unnamed>";
    public int Frequency => sampleRate;
}
