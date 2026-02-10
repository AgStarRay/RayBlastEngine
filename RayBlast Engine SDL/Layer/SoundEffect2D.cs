using RayBlast;

/// <summary>
/// AudioVoice that plays a sound once and automatically disposes itself
/// </summary>
public class SoundEffect2D {
	private static readonly List<SoundEffect2D> ALL_SOUND_EFFECTS = new();
	private static readonly List<SoundClip> POLYMORPHIC_ENTRIES = new();
	private static readonly List<float> POLYMORPHIC_VOLUMES = new();
	private static double polymorphicTime = 0.0;

	private float pitch = 1f;
	private float volume = 1f;

	private SoundEffect2D() {
		Voice = new AudioVoice();
		ALL_SOUND_EFFECTS.Add(this);
	}

	public static void UpdateAll() {
		foreach(SoundEffect2D se in ALL_SOUND_EFFECTS) {
			se.Update();
		}
		ALL_SOUND_EFFECTS.RemoveAll(se => se.Voice == null);
	}

	private void Update() {
		UpdateSource();
		if(Voice is { IsPlaying: false }) {
			Voice.Dispose();
			Voice = null;
		}
	}

	public AudioVoice? Voice { get; private set; }

	public float Pitch {
		get => pitch;
		set {
			pitch = value;
			UpdateSource();
		}
	}

	public float Volume {
		get => volume;
		set {
			volume = value;
			UpdateSource();
		}
	}

	public static SoundEffect2D PlayAtPitch(SoundClip sample, float pitch,
											float polymorphicMultiplier = 1f) {
		SoundEffect2D se = SpawnSound(sample, polymorphicMultiplier);
		se.Pitch = pitch;
		se.Voice!.Play();
		return se;
	}

	public static SoundEffect2D PlayAtVolume(SoundClip sample, float volume) {
		SoundEffect2D se = SpawnSound(sample, 1f);
		se.Volume = volume;
		se.Voice!.Play();
		return se;
	}

	public static SoundEffect2D PlayAtPitchAndVolume(SoundClip sample, float pitch, float volume) {
		SoundEffect2D se = SpawnSound(sample, 1f);
		se.Pitch = pitch;
		se.Volume = volume;
		se.Voice!.Play();
		return se;
	}

	public static SoundEffect2D Play(SoundClip sample, float polymorphicMultiplier = 0.5f) {
		SoundEffect2D se = SpawnSound(sample, polymorphicMultiplier);
		se.Voice!.Play();
		return se;
	}

	private static SoundEffect2D SpawnSound(SoundClip clip, float polymorphicMultiplier) {
		var se = new SoundEffect2D();
		se.Voice!.source = clip;
		se.UpdateSource();
		if(Time.time - polymorphicTime > 0.015f) {
			POLYMORPHIC_ENTRIES.Clear();
			POLYMORPHIC_VOLUMES.Clear();
			polymorphicTime = Time.time;
		}
		int index = POLYMORPHIC_ENTRIES.IndexOf(clip);
		if(index == -1) {
			POLYMORPHIC_ENTRIES.Add(clip);
			POLYMORPHIC_VOLUMES.Add(1f);
		}
		else {
			POLYMORPHIC_VOLUMES[index] *= polymorphicMultiplier;
			se.Volume *= POLYMORPHIC_VOLUMES[index];
		}
		return se;
	}

	private void UpdateSource() {
		if(Voice != null) {
			Voice.pitch = pitch;
			Voice.volume = Game.Settings.soundEnabled ? volume * Game.Settings.SoundLevel : 0f;
		}
	}
}
