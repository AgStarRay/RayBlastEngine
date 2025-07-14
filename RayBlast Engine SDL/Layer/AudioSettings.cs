namespace RayBlast;

public static class AudioSettings {
    public static int RealVoiceCount { get; private set; }
    public static int VirtualVoiceCount { get; private set; }
    public static SpeakerMode CurrentSpeakerMode { get; private set; }

    public static void ResetDSP(int realVoices, int virtualVoices,
                                SpeakerMode speakerMode) {
        RealVoiceCount = realVoices;
        VirtualVoiceCount = virtualVoices;
        CurrentSpeakerMode = speakerMode;
        DigitalSoundProcessing.pendingReset = true;
    }

    public enum SpeakerMode {
        Undefined,
        Mono,
        Stereo,
        Quad,
        Surround,
        Mode5point1,
        Mode7point1,
        Prologic
    }
}
