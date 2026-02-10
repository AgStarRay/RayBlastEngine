using System.Collections;
using System.Collections.Concurrent;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace RayBlast;

internal class RayBlastSoundMixer : ISampleProvider {
    internal int realVoiceCount;
    internal int virtualVoiceCount;
    private readonly IList sources = ArrayList.Synchronized(new List<AudioVoice>());
    private float[] sourceBuffer = [];
    public int processedUpdates;

    //TODO: Use audio device frequency
    public WaveFormat WaveFormat { get; private set; } = WaveFormat.CreateIeeeFloatWaveFormat(DigitalSoundProcessing.OutputFrequency, 2);
    public int SourceCount => sources.Count;

    public void Reset(int newSampleRate) {
        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(newSampleRate, 2);
    }

    public void Play(AudioVoice voice) {
        if(sources.Count >= virtualVoiceCount)
            Debug.LogWarning("Too many sounds enqueued to possibly handle");
        else {
            //TODO_URGENT: Handle volume
            // stream.Volume = (float)RatioDecibelTransform(voice.volume);
            //TODO_URGENT: Handle pitch
            // Raylib.SetAudioStreamPitch(stream, voice.pitch * wave.SampleRate / outputFrequency);
            if(sources.Count >= virtualVoiceCount) {
                Debug.LogWarning($"Could not play {voice.source.Name}, hit virtual voice limit of {virtualVoiceCount}");
            }
            else if(!sources.Contains(voice)) {
                sources.Add(voice);
            }
        }
    }

    public void Stop(AudioVoice voice) {
        sources.Remove(voice);
    }

    public bool IsPlaying(AudioVoice voice) {
        return sources.Contains(voice);
    }

    public int Read(float[] buffer, int offset,
                    int count) {
        processedUpdates++;
        Time.dspTime += count / (double)(WaveFormat.SampleRate * 2);
        var val2 = 0;
        try {
            sourceBuffer = BufferHelpers.Ensure(sourceBuffer, count);
            for(int i1 = sources.Count - 1; i1 >= 0; --i1) {
                if(i1 >= sources.Count)
                    continue;
                AudioVoice voice = (AudioVoice?)sources[i1] ?? throw new NullReferenceException();
                if(voice.sampleProvider == null)
                    continue;
                if(voice.finalMixSampleProvider == null) {
                    ISampleProvider? newMix = voice.sampleProvider;
                    if(newMix.WaveFormat.Channels != WaveFormat.Channels) {
                        if(WaveFormat.Channels == 2 && newMix.WaveFormat.Channels == 1)
                            newMix = new MonoToStereoSampleProvider(newMix);
                    }
                    if(newMix.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                        Debug.LogWarning($"Invalid mix encoding {newMix.WaveFormat.Encoding}");
                    voice.finalMixSampleProvider = new RayBlastWdlResampler(newMix, WaveFormat.SampleRate);
                }
                RayBlastWdlResampler source = voice.finalMixSampleProvider;
                source.pitch = voice.pitch;
                int val1 = source.Read(sourceBuffer, 0, count);
                if(voice.volume > 0f && i1 < realVoiceCount) {
                    int num = offset;
                    for(var i2 = 0; i2 < val1; ++i2) {
                        if(i2 >= val2)
                            buffer[num++] = sourceBuffer[i2] * voice.volume;
                        else
                            buffer[num++] += sourceBuffer[i2] * voice.volume;
                    }
                    val2 = Math.Max(val1, val2);
                }
                if(val1 < count) {
                    sources.RemoveAt(i1);
                }
            }
        }
        catch(Exception e) {
            Debug.LogException(e);
        }
        if(val2 < count) {
            int num = offset + val2;
            while(num < offset + count) {
                buffer[num++] = 0.0f;
            }
        }
        return count;
    }

    public AudioVoice? GetSource(int index) {
        return index >= sources.Count ? null : (AudioVoice?)sources[index];
    }
}
