using System.Collections.Generic;

namespace RayBlast.Composer;

public static class BeatTracker {
    private static double lastDSP = 0.0;
    private static float maximumMeasures = 0f;
    private static int totalTicks = 0;
    private static int maximumTicks = 0;
    private static float bpm = 120f;
    private static float unscaledBPM = 120f;
    private static float currentSongBPM = 120f;
    private static int[] tickStarts = new int[0];
    private static int currentStartTick = 0;
    private static int currentStartMeasure = 1;

    public static int Measure {
        get {
            if(Time.dspTime != lastDSP)
                UpdateBeats();
            return currentStartMeasure + Mathd.FloorToInt((totalTicks - currentStartTick - 1) / (float)CurrentSignature);
        }
    }

    public static float MeasureDelta {
        get {
            if(Time.dspTime != lastDSP)
                UpdateBeats();
            return (float)(bpm * Time.unscaledDeltaTime / (CurrentSignature * 15f));
        }
    }

    public static int TotalBeats => Mathd.CeilToInt(TotalTicks / 4f);

    public static int TotalTicks {
        get {
            if(Time.dspTime != lastDSP)
                UpdateBeats();
            return totalTicks;
        }
    }

    public static int Beat {
        get {
            float beatsFromStart = (TotalTicks - currentStartTick - 1) / 4f;
            return Mathd.FloorToInt((beatsFromStart + CurrentSignature / 4f) % (CurrentSignature / 4f) + 1);
        }
    }

    public static int Tick => (TotalTicks - currentStartTick + CurrentSignature - 1) % CurrentSignature % 4 + 1;

    public static int MaximumMeasure {
        get {
            var rate = 16;
            var startingTick = 0;
            var startingMeasure = 1;
            SignaturePoint[] points = BankPlayer.MAIN.SignaturePoints;
            if(tickStarts.Length > 0 && points.Length > 0) {
                rate = points[points.Length - 1].ticks;
                startingMeasure = points[points.Length - 1].measureNumber;
                startingTick = tickStarts[tickStarts.Length - 1];
            }
            return startingMeasure + Mathd.CeilToInt((MaximumTicks - startingTick) / (float)rate) - 1;
        }
    }

    public static int MaximumBeats => Mathd.CeilToInt(MaximumTicks / 4f);

    public static int MaximumTicks => maximumTicks;

    public static int FinalBeat {
        get {
            var rate = 16;
            var startingTick = 0;
            SignaturePoint[] points = BankPlayer.MAIN.SignaturePoints;
            if(tickStarts.Length > 0 && points.Length > 0) {
                rate = points[points.Length - 1].ticks;
                startingTick = tickStarts[tickStarts.Length - 1];
            }
            float beatsFromStart = (MaximumTicks - startingTick - 1) / 4f;
            return Mathd.FloorToInt((beatsFromStart + rate / 4f) % (rate / 4f) + 1);
        }
    }

    public static int FinalTick {
        get {
            var rate = 16;
            var startingTick = 0;
            SignaturePoint[] points = BankPlayer.MAIN.SignaturePoints;
            if(tickStarts.Length > 0 && points.Length > 0) {
                rate = points[points.Length - 1].ticks;
                startingTick = tickStarts[tickStarts.Length - 1];
            }
            return (MaximumTicks - startingTick + rate - 1) % rate % 4 + 1;
        }
    }

    public static float WholeSawtooth {
        get {
            if(Time.dspTime != lastDSP)
                UpdateBeats();
            return 1f - TotalMeasureFloat % 1f;
        }
    }

    public static float HalfSawtooth {
        get {
            if(Time.dspTime != lastDSP)
                UpdateBeats();
            return 1f - TotalMeasureFloat * 2f % 1f;
        }
    }

    public static float QuarterSawtooth {
        get {
            if(Time.dspTime != lastDSP)
                UpdateBeats();
            return 1f - TotalMeasureFloat * 4f % 1f;
        }
    }

    public static float EighthSawtooth {
        get {
            if(Time.dspTime != lastDSP)
                UpdateBeats();
            return 1f - TotalMeasureFloat * 8f % 1f;
        }
    }

    public static float WholeCosine => (float)Math.Cos(WholeSawtooth * 2f * Math.PI);

    public static float HalfCosine => (float)Math.Cos(HalfSawtooth * 2f * Math.PI);

    public static float QuarterCosine => (float)Math.Cos(QuarterSawtooth * 2f * Math.PI);

    public static float EighthCosine => (float)Math.Cos(EighthSawtooth * 2f * Math.PI);

    public static float WholeTriangle => Math.Abs(WholeSawtooth * 2f - 1f);

    public static float HalfTriangle => Math.Abs(HalfSawtooth * 2f - 1f);

    public static float QuarterTriangle => Math.Abs(QuarterSawtooth * 2f - 1f);

    public static float EighthTriangle => Math.Abs(EighthSawtooth * 2f - 1f);

    public static float TotalMeasureFloat { get; private set; }

    public static int CurrentSignature { get; private set; } = 16;

    public static float BPM {
        get {
            if(Time.dspTime != lastDSP)
                UpdateBeats();
            return bpm;
        }
    }

    public static float UnscaledBPM {
        get {
            if(Time.dspTime != lastDSP)
                UpdateBeats();
            return unscaledBPM;
        }
    }

    public static float CurrentSongBPM {
        get {
            if(Time.dspTime != lastDSP)
                UpdateBeats();
            return currentSongBPM;
        }
    }

    private static void UpdateBeats() {
        lastDSP = Time.dspTime;
        BPMPoint[] points = BankPlayer.MAIN.BPMPoints;
        bpm = BankPlayer.MAIN.MainBPM;
        TotalMeasureFloat = 0f;
        if(BankPlayer.MAIN.BeatOffset > 0f)
            TotalMeasureFloat -= 1f - BankPlayer.MAIN.BeatOffset;
        maximumMeasures = TotalMeasureFloat;
        uint trackedPosition = 0;
        float trackedBPM = bpm;
        var passedLastPoint = true;
        float denominator = 240f * BankPlayer.MAIN.ClipFrequency;
        foreach(BPMPoint p in points) {
            if(p.sample < BankPlayer.MAIN.CurrentSample) {
                TotalMeasureFloat += (p.sample - trackedPosition) * trackedBPM / denominator;
                bpm = p.bpm;
            }
            else {
                if(passedLastPoint)
                    TotalMeasureFloat += (BankPlayer.MAIN.CurrentSample - trackedPosition) * trackedBPM / denominator;
                passedLastPoint = false;
            }
            maximumMeasures += (p.sample - trackedPosition) * trackedBPM / denominator;
            trackedPosition = p.sample;
            trackedBPM = p.bpm;
        }
        if(passedLastPoint)
            TotalMeasureFloat += (BankPlayer.MAIN.CurrentSample - trackedPosition) * trackedBPM / denominator;
        maximumMeasures += (BankPlayer.MAIN.SampleLength - trackedPosition) * trackedBPM / denominator;
        totalTicks = Mathd.FloorToInt(1 + TotalMeasureFloat * 16f);
        maximumTicks = Mathd.FloorToInt(1.03125f + maximumMeasures * 16f);
        var signatureIndex = 0;
        while(tickStarts.Length - 1 > signatureIndex && totalTicks > tickStarts[signatureIndex + 1]) {
            signatureIndex++;
        }
        if(tickStarts.Length > 0 && BankPlayer.MAIN.SignaturePoints.Length > 0) {
            SignaturePoint point = BankPlayer.MAIN.SignaturePoints[signatureIndex];
            currentStartTick = tickStarts[signatureIndex];
            currentStartMeasure = point.measureNumber;
            CurrentSignature = point.ticks;
        }
        else {
            currentStartTick = 0;
            currentStartMeasure = 1;
            CurrentSignature = 16;
        }
        currentSongBPM = bpm;
        bpm *= BankPlayer.MAIN.Speed;
        if(Time.timeScale != 0f)
            unscaledBPM = (float)(bpm / Time.timeScale);
        if(BankPlayer.MAIN.Paused) {
            bpm = 0f;
            unscaledBPM = 0f;
        }
    }

    public static void InitializeSignatureChanges() {
        var tickPoints = new List<int>();
        var currentRate = 16;
        var currentTick = 0;
        var currentMeasure = 1;
        foreach(SignaturePoint p in BankPlayer.MAIN.SignaturePoints) {
            currentTick += (p.measureNumber - currentMeasure) * currentRate;
            tickPoints.Add(currentTick);
            currentMeasure = p.measureNumber;
            currentRate = p.ticks;
        }
        tickStarts = tickPoints.ToArray();
    }
}
