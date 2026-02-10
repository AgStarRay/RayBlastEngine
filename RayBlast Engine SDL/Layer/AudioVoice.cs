using System.Runtime.InteropServices;
using NAudio.Wave;

namespace RayBlast;

public class AudioVoice : IDisposable {
    public float Time {
        get {
            if(sampleProvider != null)
                return SamplePosition / (float)(finalMixSampleProvider ?? sampleProvider).WaveFormat.SampleRate;
            return 0f;
        }
        set {
            if(sampleProvider != null)
                SamplePosition = (int)(value * (finalMixSampleProvider ?? sampleProvider).WaveFormat.SampleRate);
        }
    }
    public ISoundSource source;
    public bool IsPlaying => DigitalSoundProcessing.IsPlaying(this);
    public float volume;
    public float pitch;
    //TODO: Use these values
    public bool looping;
    public uint skipFrom;
    public uint skipTo;
    public int SamplePosition {
        get {
            if(sampleProvider != null)
                return (int)(playbackStream?.Position ?? 0 / (finalMixSampleProvider ?? sampleProvider).WaveFormat.BlockAlign);
            return 0;
        }
        set {
            if(sampleProvider != null)
                playbackStream?.Position = value * (finalMixSampleProvider ?? sampleProvider).WaveFormat.BlockAlign;
        }
    }
    public double dspEnter;
    public double dspExit;
    public byte priority = 128;

    internal int endFrames;
    internal long lastUpdate;
    internal WaveStream? playbackStream;
    internal ISampleProvider? sampleProvider;
    internal RayBlastWdlResampler? finalMixSampleProvider;
    internal MemoryStream? memoryStream;
    internal bool disposed;

    public AudioVoice() : this(SoundClip.SILENCE) {
    }

    public AudioVoice(SoundClip source) {
        this.source = source;
        pitch = 1f;
        volume = 1f;
        skipFrom = 0;
    }

    public void Play() {
        DigitalSoundProcessing.Stop(this);
        SamplePosition = 0;
        DigitalSoundProcessing.Play(this);
    }

    public void Play(int startingSample) {
        DigitalSoundProcessing.Stop(this);
        SamplePosition = startingSample;
        DigitalSoundProcessing.Play(this);
    }

    //TODO: Use NAudio's offset capability
    public void PlayScheduled(double dspNextTime) {
        DigitalSoundProcessing.Play(this);
        dspEnter = dspNextTime;
    }

    public void Pause() {
        DigitalSoundProcessing.Stop(this);
    }

    public void Resume() {
        DigitalSoundProcessing.Play(this);
    }

    public void Stop() {
        DigitalSoundProcessing.Stop(this);
        SamplePosition = 0;
    }

    public void SetScheduledStartTime(double newDspTime) {
        dspEnter = newDspTime;
    }

    public void SetScheduledEndTime(double newDspTime) {
        dspExit = newDspTime;
    }

    private void ReleaseUnmanagedResources() {
        DigitalSoundProcessing.Stop(this);
        if(!disposed && memoryStream != null) {
            disposed = true;
            finalMixSampleProvider = null;
            if(playbackStream != null)
                UnmanagedManager.EnqueueUnloadStream(playbackStream);
            UnmanagedManager.EnqueueUnloadStream(memoryStream);
        }
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~AudioVoice() {
        ReleaseUnmanagedResources();
    }
}
