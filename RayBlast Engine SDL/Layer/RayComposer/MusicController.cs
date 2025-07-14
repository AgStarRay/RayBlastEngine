using RayBlast.Composer;

namespace RayBlast;

public static class MusicController {
    private static bool musicFirstIndex;

    public static int Stage {
        get => BankPlayer.MAIN.stage;
        set => BankPlayer.MAIN.stage = value;
    }

    public static int StageCount => BankPlayer.MAIN.CurrentBank.stageLoops.Length;

    public static bool IsPlaying => BankPlayer.MAIN.IsPlaying;

    public static bool Looping {
        get => BankPlayer.MAIN.looping;
        set => BankPlayer.MAIN.looping = value;
    }

    public static MusicBank CurrentBank => BankPlayer.MAIN.CurrentBank;

    public static float Position {
        get => BankPlayer.MAIN.CurrentTime;
        set => BankPlayer.MAIN.CurrentTime = value % BankPlayer.MAIN.ClipLength;
    }

    public static float Length => BankPlayer.MAIN.ClipLength;

    public static float Pitch {
        get => BankPlayer.MAIN.pitch;
        set => BankPlayer.MAIN.pitch = value;
    }

    public static float Tempo {
        get => BankPlayer.MAIN.tempo;
        set => BankPlayer.MAIN.tempo = Math.Clamp(value, 0.5f, 2f);
    }
    public static float[] PublicChannelVolumes {
        get => BankPlayer.MAIN.publicChannelVolumes;
        set => BankPlayer.MAIN.publicChannelVolumes = value;
    }
    public static bool PlayingCustomMusic { get; set; }

    // public static int TotalMeasures => BeatTracker.Measure;
    //
    // public static int TotalBeats => BeatTracker.TotalBeats;
    //
    // public static int TotalTicks => BeatTracker.TotalTicks;

    public static void Update() {
        #if DEBUG
        if(Input.IsKeyDown(Key.F9)) {
            if(Input.IsKeyDown(Key.LeftShift)) {
                Stop();
                musicFirstIndex = false;
            }
            else
                musicFirstIndex = true;
        }
        if(Input.IsKeyDown(Key.F9)) {
            int playIndex = -1;
            for(var i = Key.Digit0; i <= Key.Digit9; i++) {
                if(Input.IsKeyPressed(i))
                    playIndex = i - Key.Digit0;
            }
            if(playIndex > -1) {
                musicFirstIndex = false;
                if(PlayingCustomMusic) {
                    Game.CustomBank ??= CurrentBank;
                    PlaySong(Game.CustomBank, playIndex);
                }
            }
        }
        if(Input.IsKeyReleased(Key.F9) && musicFirstIndex) {
            if(PlayingCustomMusic) {
                Game.CustomBank ??= CurrentBank;
                PlaySong(Game.CustomBank);
            }
        }
        if(Input.IsKeyPressed(Key.F10) && StageCount > 0)
            Stage = (Stage + 1) % StageCount;
        if(Input.IsKeyPressed(Key.F11))
            BankPlayer.MAIN.CurrentTime += 5f;
        #endif
        BankPlayer.MAIN.volume = Game.Settings.musicEnabled ? Game.Settings.MusicLevel : 0f;
    }

    public static void PlaySong(MusicBank musicBank, int startIndex = 0) {
        BankPlayer.MAIN.volume = Game.Settings.musicEnabled ? Game.Settings.MusicLevel : 0f;
        BankPlayer.MAIN.PlaySong(musicBank, startIndex);
        BankPlayer.MAIN.looping = true;
    }

    public static void PlaySong(SoundClip musicSample) {
        var bank = new MusicBank {
            channels = new[] {
                musicSample
            },
            title = "???"
        };
        PlaySong(bank);
    }

    public static void Stop() {
        BankPlayer.MAIN.Stop();
    }

    public static void Pause() {
        BankPlayer.MAIN.Pause();
    }

    public static void Unpause() {
        BankPlayer.MAIN.Unpause();
    }

    public static void Fadein(float fadeinTime = 0.5f) {
        var volumes = new float[BankPlayer.MAIN.publicChannelVolumes.Length];
        BankPlayer.MAIN.ChannelVolumes = volumes;
        for(var i = 0; i < volumes.Length; i++) {
            BankPlayer.MAIN.publicChannelVolumes[i] = 1f;
        }
        BankPlayer.MAIN.channelFadeTime = fadeinTime;
    }

    public static void Fadeout(float fadeoutTime = 0.5f) {
        for(var i = 0; i < BankPlayer.MAIN.publicChannelVolumes.Length; i++) {
            BankPlayer.MAIN.publicChannelVolumes[i] = 0f;
        }
        BankPlayer.MAIN.channelFadeTime = fadeoutTime;
    }
}
