using NAudio.Wave;

namespace RayBlast;

//https://markheath.net/post/30-days-naudio-docs
//TODO: Add timestretch support
//TODO: If realVoiceLimit is hit, trade out virtual voices with higher priority
//TODO: If attempting to play a sound over the virtualVoiceLimit, stop a sound with lower priority
public static class DigitalSoundProcessing {
    private static WaveOutEvent? waveOut;
    private static readonly RayBlastSoundMixer MIXER = new();
    internal static bool pendingReset;
    public static int ProcessedUpdates => MIXER.processedUpdates;

    internal static void Initialize() {
        pendingReset = false;
        MIXER.realVoiceCount = AudioSettings.RealVoiceCount;
        MIXER.virtualVoiceCount = AudioSettings.VirtualVoiceCount;
        Shutdown();

        Debug.LogDebug("Load audio output device", false);
        waveOut = new WaveOutEvent();
        waveOut.DesiredLatency = 66;
        MIXER.Reset(44100);
        waveOut.Init(MIXER);
        waveOut.Play();
    }

    internal static void Play(AudioVoice voice) {
        if(voice.disposed)
            return;
        if(voice.memoryStream != null) {
            UnmanagedManager.EnqueueUnloadStream(voice.playbackStream!);
            UnmanagedManager.EnqueueUnloadStream(voice.memoryStream);
        }
        if(voice.clip.audioData != null) {
            voice.memoryStream = new MemoryStream(voice.clip.audioData);
            voice.playbackStream = new WaveFileReader(voice.memoryStream);
            voice.sampleProvider = voice.playbackStream.ToSampleProvider();
        }
        else if(voice.clip.stream != null) {
            voice.playbackStream = voice.clip.stream;
            voice.sampleProvider = voice.clip.sampleProvider;
        }
        else
            throw new RayBlastEngineException($"{voice.clip.Name} is missing stream or sample data");
        voice.finalMixSampleProvider = null;
        MIXER.Play(voice);
    }


    internal static void Stop(AudioVoice voice) {
        MIXER.Stop(voice);
    }

    public static double RatioDecibelTransform(double volume) {
        return volume switch {
            <= 0.0 => 0.0,
            >= 1.0 => 1.0,
            // _ => 0.316227766016838 * (Math.Pow(3.16227766016838, volume) + volume - 1.0)
            _ => 0.1 * (Math.Pow(10.0, volume) + volume - 1.0)
        };
    }

    public static double LinearToDecibel(double linear) {
        return 20.0 * Math.Log10(linear / 10.0) + 20.0;
    }

    internal static bool IsPlaying(AudioVoice voice) {
        return MIXER.IsPlaying(voice);
    }

    public static void AddDebugGraphics(int anchorX, int anchorY) {
        int trackedY = anchorY;
        for(int i = 0; i < MIXER.realVoiceCount; i++) {
            if(i % 8 == 0) {
                trackedY -= 8;
            }
            int sourceX = anchorX - 8 * (8 - i % 8);
            ImmediateMode.DrawRectangleOutline(sourceX - 1, trackedY, 9, 9, 1, Color32.WHITE);
            ImmediateMode.DrawRectangle(sourceX, trackedY + 1, 7, 4, GetDebugSourceColor(i));
            ImmediateMode.DrawRectangle(sourceX, trackedY + 5, 7, 3, GetDebugStreamColor(i));
        }
        for(int i = MIXER.realVoiceCount; i < MIXER.virtualVoiceCount; i++) {
            if(i % 8 == 0) {
                trackedY -= 4;
            }
            int sourceX = anchorX - 8 * (8 - i % 8);
            ImmediateMode.DrawRectangle(sourceX, trackedY, 7, 3, GetDebugSourceColor(i));
        }
    }

    private static Color32 GetDebugStreamColor(int index) {
        if(index >= MIXER.SourceCount) {
            return Color32.BLACK;
        }
        AudioVoice? voice = MIXER.GetSource(index);
        return voice?.IsPlaying ?? false ? Color32.GREEN : Color32.BLUE;
    }

    private static Color32 GetDebugSourceColor(int index) {
        if(index >= MIXER.SourceCount) {
            return Color32.BLACK;
        }
        AudioVoice? voice = MIXER.GetSource(index);
        if(voice?.IsPlaying ?? false) {
            int voiceProgress = (int)(voice.samplePosition * 255L / Math.Max(voice.clip.SampleCount, 256L));
            // bool behind = index < sources.Length
            // 		   && RayBlastEngine.WATCH.ElapsedTicks - voice.lastUpdate > TimeSpan.TicksPerSecond * 2 / INTERVAL_DENOMINATOR;
            return new Color32(0, 255 - voiceProgress / 3, voiceProgress);
        }
        return Color32.BLUE;
    }

    public static void Shutdown() {
        waveOut?.Dispose();
    }
}
