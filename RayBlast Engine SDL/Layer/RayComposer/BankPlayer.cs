// Add pitch filters, zero-source compatibility, parameter exceptions, and play-compile compatibility
namespace RayBlast.Composer;

public class BankPlayer {
    private const float PITCH_SPEED = 20f;
    // Raise this value to reduce the power of the low pass filter
    private const float LOWER_PASS_BASE = 250f;
    // Lower this value to reduce the power of the high pass filter
    private const float UPPER_PASS_BASE = 1000f;
    // Raise this value to give speed change more of a punch
    private const float MAX_SLOW_Q = 2f;

    public static readonly BankPlayer MAIN = new();
    private static readonly LoopSet BLANK_STAGE = new() {
        loopSegments = Array.Empty<LoopSamplePoints>(), skipSegments = Array.Empty<SkipSamplePoints>(), channelLevels = Array.Empty<float>()
    };
    // private static AudioMixerGroup pitchBendMixer;

    public float volume = 0.86f;
    public float pitch = 1f;
    public float tempo = 1f;
    public bool linkWithTimeScale = true;
    public float[] publicChannelVolumes = Array.Empty<float>();
    public int stage = 0;
    public float channelFadeTime = 0.5f;
    public bool channelFadingIsUnscaled = true;
    public bool usePitchBendEffects = true;
    public bool looping = true;

    private readonly List<AudioVoice> sources = new();
    private float trackedPosition;
    private float[] targetChannelVolumes = Array.Empty<float>();
    private float[] channelVolumes = Array.Empty<float>();
    private float channelPitch = 1f;
    private bool supposedToBePlaying = false;
    private float artistTimer = 0f;

    public MusicBank CurrentBank { get; private set; } = MusicBank.BLANK;

    public string Title => CurrentBank.title;

    public string Artist {
        get {
            int artistCount = CurrentBank.artists.Length;
            return CurrentBank.artists[(int)Math.Floor(artistTimer / 2.5f) % (artistCount + 1) % artistCount];
        }
    }

    public string Album => CurrentBank.album;

    public float BeatOffset => CurrentBank?.beatOffset ?? 0f;

    public LoopSet CurrentStage {
        get {
            if(CurrentBank.stageLoops.Length == 0)
                return BLANK_STAGE;
            if(stage < 0 || stage >= CurrentBank.stageLoops.Length)
                return CurrentBank.stageLoops[0];
            return CurrentBank.stageLoops[stage];
        }
    }

    public BPMPoint[] BPMPoints => CurrentBank.bpmPoints;

    public SignaturePoint[] SignaturePoints => CurrentBank.signaturePoints;

    public float MainBPM => CurrentBank.mainBPM;

    public float CurrentTime {
        get => sources.Count > 0 ? sources[0].Time : 0f;
        set {
            value = Math.Max(value % ClipLength, 0f);
            foreach(AudioVoice s in sources) {
                s.Time = value;
            }
        }
    }

    public int CurrentSample {
        get => sources.Count > 0 ? sources[0].SamplePosition : 0;
        set {
            value = Math.Max(value % SampleLength, 0);
            foreach(AudioVoice s in sources) {
                s.Time = value;
            }
        }
    }

    public float[] ChannelVolumes {
        get => channelVolumes;
        set {
            channelVolumes = value;
            for(var i = 0; i < channelVolumes.Length; i++) {
                sources[i].volume = value[i] * volume;
            }
        }
    }

    public float Speed => sources.Count > 0 ? sources[0].pitch : 1f;

    public float ActualPitch {
        get {
            if(sources.Count == 0)
                return 1f;
            return sources[0].pitch / Math.Clamp(tempo, 0.5f, 2f);
        }
    }

    //TODO: Re-implement
    // public float ClipLength => sources.Count > 0 ? sources[0].source.Length : 0f;
    // public int SampleLength => sources.Count > 0 ? sources[0].source.SampleCount : 0;
    public float ClipLength => 0f;
    public int SampleLength => 0;

    // public double TimeToNextJump => (DSPNextTime - AudioSettings.dspTime);

    public double PlayingUntil { get; private set; } = double.PositiveInfinity;

    public double NextJumpFrom { get; private set; }

    public double NextJumpTo { get; private set; }

    public bool Paused { get; private set; }

    public bool IsPlaying {
        get {
            if(supposedToBePlaying)
                return true;
            return sources.Count > 0 && sources[0].IsPlaying;
        }
    }
    public int ClipFrequency => sources.Count > 0 ? sources[0].source.Frequency : 8000;

    public void Update() {
        // if(pitchBendMixer == null) {
        // 	pitchBendMixer = Resources.Load<AudioMixerGroup>("Pitch Bend Mixer");
        // 	if(pitchBendMixer == null)
        // 		throw new NullReferenceException(
        // 			"Pitch Bend Mixer.mixer is not in the Resources folder. This is required for tempo changing.");
        // }

        // Adjust volume of each channel
        var fadeDeltaTime = 1.0;
        if(channelFadeTime > 0f) {
            fadeDeltaTime /= channelFadeTime;
            if(channelFadingIsUnscaled)
                fadeDeltaTime *= Time.unscaledDeltaTime;
            else
                fadeDeltaTime *= Time.deltaTime;
        }
        float[] newLevels = CurrentStage.channelLevels;
        for(var i = 0; i < sources.Count; i++) {
            if(i < newLevels.Length)
                targetChannelVolumes[i] = newLevels[i];
            if(targetChannelVolumes.Length < channelVolumes.Length) {
                targetChannelVolumes = new float[channelVolumes.Length];
                for(var j = 0; j < targetChannelVolumes.Length; j++) {
                    targetChannelVolumes[j] = 1f;
                }
            }
            if(publicChannelVolumes.Length < channelVolumes.Length) {
                publicChannelVolumes = new float[channelVolumes.Length];
                for(var j = 0; j < publicChannelVolumes.Length; j++) {
                    publicChannelVolumes[j] = 1f;
                }
            }
            float targetChannelVolume = targetChannelVolumes[i] * publicChannelVolumes[i];
            channelVolumes[i] = Math.Abs(targetChannelVolume - channelVolumes[i]) <= fadeDeltaTime
                                    ? targetChannelVolume
                                    : (float)(channelVolumes[i] + Math.Sign(targetChannelVolume - channelVolumes[i]) * fadeDeltaTime);
            sources[i].volume = channelVolumes[i] * volume;
        }

        if(Time.unscaledDeltaTime > 0f) {
            float newPitch = pitch;
            if(linkWithTimeScale && Time.timeScale > 0f)
                newPitch *= (float)Time.timeScale;
            float newTempo = Math.Clamp(tempo, 0.5f, 2f);

            newPitch = Math.Clamp(newPitch, 0.25f, 4f) * newTempo;
            channelPitch = (float)((channelPitch * newTempo - newPitch) * Math.Pow(0.5f, Time.unscaledDeltaTime * PITCH_SPEED) + newPitch);
            if(Math.Abs(channelPitch - newPitch) < 0.00001f)
                channelPitch = newPitch;
            // pitchBendMixer.audioMixer.SetFloat("PitchBend", 1f / newTempo);
            foreach(AudioVoice s in sources) {
                s.pitch = channelPitch;
            }
            // if(newTempo != 1f)
            // 	s.outputAudioMixerGroup = pitchBendMixer;
            // else
            // 	s.outputAudioMixerGroup = null;
            channelPitch /= newTempo;
        }
        artistTimer += (float)Time.unscaledDeltaTime;

        UpdatePitchBendEffects();
        UpdateNextJump();
    }

    public void PlaySong(MusicBank musicBank, float[] channelLevels,
                         int startIndex = 0) {
        Paused = false;
        supposedToBePlaying = true;
        foreach(AudioVoice source in sources) {
            source.Stop();
        }
        sources.Clear();

        CurrentBank = musicBank;
        channelFadeTime = CurrentBank.channelFadeTime;
        targetChannelVolumes = new float[channelLevels.Length];
        publicChannelVolumes = new float[channelLevels.Length];
        channelVolumes = new float[channelLevels.Length];
        for(var i = 0; i < channelLevels.Length; i++) {
            targetChannelVolumes[i] = channelLevels[i];
            publicChannelVolumes[i] = 1f;
            channelVolumes[i] = channelLevels[i];
        }

        trackedPosition = 0f;
        if(startIndex < musicBank.startSamples.Length)
            trackedPosition = CurrentBank.startSamples[startIndex] / (float)CurrentBank.channels[0].Frequency;
        for(var i = 0; i < CurrentBank.channels.Length; i++) {
            sources.Add(new AudioVoice(CurrentBank.channels[i]));
            sources[i].Time = trackedPosition;
            sources[i].volume = targetChannelVolumes[i] * volume;
        }
        foreach(AudioVoice s in sources) {
            s.Play();
        }
        UpdateNextJump();
        // if(!musicBank.streaming && sources[0].clip.loadType != AudioClipLoadType.Streaming) {
        // 	foreach(AudioSource s in nextSources) {
        // 		s.PlayScheduled(DSPNextTime);
        // 	}
        // }
        artistTimer = 0f;
        // BeatTracker.InitializeSignatureChanges();
    }

    public void PlaySong(MusicBank musicBank, int startIndex = 0) {
        var channels = new float[musicBank.channels.Length];
        for(var i = 0; i < channels.Length; i++) {
            channels[i] = 1f;
        }
        if(stage < musicBank.stageLoops.Length) {
            for(var i = 0; i < channels.Length && i < musicBank.stageLoops[stage].channelLevels.Length; i++) {
                channels[i] = musicBank.stageLoops[stage].channelLevels[i];
            }
        }
        PlaySong(musicBank, channels, startIndex);
    }

    public void Pause() {
        Paused = true;
        supposedToBePlaying = false;
        foreach(AudioVoice s in sources) {
            s.Pause();
        }
    }

    public void Unpause() {
        Paused = false;
        supposedToBePlaying = true;
        foreach(AudioVoice s in sources) {
            s.Play();
        }
    }

    public void Stop() {
        Paused = false;
        supposedToBePlaying = false;
        foreach(AudioVoice s in sources) {
            s.Stop();
            s.Time = 0f;
        }
    }

    private void UpdatePitchBendEffects() {
        float newPitch = pitch;
        if(linkWithTimeScale && Time.timeScale > 0f)
            newPitch *= (float)Time.timeScale;
        float newTempo = Math.Clamp(tempo, 0.5f, 2f);

        newPitch = Math.Clamp(newPitch, 0.25f, 4f) * newTempo;
        float pitchDifference = newPitch - Speed;
        var lowCutoff = 22_000f;
        var lowResonanceQ = 1f;
        if(pitchDifference > 0f) {
            lowCutoff = LOWER_PASS_BASE / Math.Abs(pitchDifference / newPitch);
            lowResonanceQ = 1f + Math.Min(Math.Abs(pitchDifference) * 5f, 1f) * MAX_SLOW_Q;
        }
        // foreach(AudioSource s in sources) {
        // 	AudioLowPassFilter lowPassFilter = s.GetComponent<AudioLowPassFilter>();
        // 	lowPassFilter.enabled = usePitchBendEffects;
        // 	lowPassFilter.cutoffFrequency = lowCutoff;
        // 	lowPassFilter.lowpassResonanceQ = lowResonanceQ;
        // }
        // foreach(AudioSource s in nextSources) {
        // 	AudioLowPassFilter lowPassFilter = s.GetComponent<AudioLowPassFilter>();
        // 	lowPassFilter.enabled = usePitchBendEffects;
        // 	lowPassFilter.cutoffFrequency = lowCutoff;
        // 	lowPassFilter.lowpassResonanceQ = lowResonanceQ;
        // }
    }

    private void UpdateNextJump() {
        if(sources.Count == 0)
            return;
        double dsp = Time.dspTime;
        NextJumpFrom = float.PositiveInfinity;
        if(looping)
            NextJumpFrom = CurrentBank.channels[0].length;
        NextJumpTo = 0f;
        LoopSet currentStage = CurrentStage;
        int sampleRate = CurrentBank.channels[0].Frequency;
        var loopFromPoint = (uint)CurrentBank.channels[0].SampleCount;
        uint loopToPoint = 0;
        if(CurrentBank.stageLoops.Length > 0) {
            if(looping) {
                foreach(LoopSamplePoints lp in currentStage.loopSegments) {
                    uint endSample = lp.endSample;
                    float end = endSample / (float)sampleRate;
                    if(endSample == 0) {
                        endSample = (uint)CurrentBank.channels[0].SampleCount;
                        end = CurrentBank.channels[0].length;
                    }
                    if(end < NextJumpFrom + 0.0005 && end > trackedPosition) {
                        NextJumpFrom = Math.Min(end, CurrentBank.channels[0].length);
                        NextJumpTo = Math.Min(lp.startSample / (float)sampleRate, CurrentBank.channels[0].length - 0.01f);
                        loopFromPoint = endSample;
                        loopToPoint = lp.startSample;
                    }
                }
            }
            foreach(SkipSamplePoints sp in currentStage.skipSegments) {
                float from = sp.fromSample / (float)sampleRate;
                if(from < NextJumpFrom + 0.0005f && from > trackedPosition) {
                    NextJumpFrom = Math.Min(from, CurrentBank.channels[0].length);
                    NextJumpTo = Math.Min(sp.toSample / (float)sampleRate, CurrentBank.channels[0].length - 0.01f);
                    loopFromPoint = sp.fromSample;
                    loopToPoint = sp.toSample;
                }
            }
        }

        if(IsPlaying) {
            if(looping)
                PlayingUntil = double.PositiveInfinity;
            else if(supposedToBePlaying)
                PlayingUntil = dsp + (CurrentBank.channels[0].length - sources[0].Time) / Speed - 0.5;
        }
        if(supposedToBePlaying) {
            if(dsp > PlayingUntil)
                supposedToBePlaying = false;
            else if(!sources[0].IsPlaying)
                Unpause();
        }
        foreach(AudioVoice s in sources) {
            s.looping = looping;
            s.skipFrom = loopFromPoint;
            s.skipTo = loopToPoint;
        }
        trackedPosition = sources[0].Time;
    }

    private void SecondsToMeasureBPM(float time, out float measure,
                                     out float bpm) {
        var currentMeasure = 0.0;
        var currentTime = 0.0;
        var currentBPM = 120f;
        for(var i = 0; i <= CurrentBank.bpmPoints.Length; i++) {
            int frequency = CurrentBank.channels[0].Frequency;
            BPMPoint bpmPoint = CurrentBank.bpmPoints[i];
            var canCalculate = true;
            // If not after the last point
            if(i < CurrentBank.bpmPoints.Length)
                // If after the current point
            {
                if(time >= bpmPoint.sample / (double)frequency) {
                    // Calculate how many measures pass between points
                    currentMeasure += (bpmPoint.sample / (double)frequency - currentTime) * currentBPM / 240f;
                    currentTime = bpmPoint.sample / (double)frequency;
                    currentBPM = bpmPoint.bpm;
                    // Do not calculate between current point and current clip time
                    canCalculate = false;
                }
            }
            // If after the last point or before the current point
            if(canCalculate) {
                // Calculate how many measures have passed since the latest point
                currentMeasure += (time - currentTime) * currentBPM / 240f;
                break;
            }
        }
        measure = (float)currentMeasure;
        bpm = currentBPM;
    }
}
