//TODO_AFTER: Reimplement if any 3D sounds exist
// namespace RayBlast; 
//
// /// <summary>
// /// Environmental AudioVoice that plays a sound once and automatically disposes itself
// /// </summary>
// public class SoundEffect3D {
// 	public const float SPEED_OF_SOUND_IN_AIR = 343f;
// 	private const float MAX_RANGE_MULTIPLIER = 10f;
// 	private const float INSTANT_RANGE = 100f;
//
// 	private static readonly List<SoundClip> POLYMORPHIC_ENTRIES = new();
// 	private static readonly List<float> POLYMORPHIC_VOLUMES = new();
// 	private static float polymorphicTime = 0f;
//
// 	private float timeLeft = 0.06f;
// 	private float pitch = 1f;
// 	private float volume = 1f;
// 	private float minimumRange = 10f;
// 	private float distanceTraveled = 0f;
// 	private bool enteredListener = false;
// 	
// 	internal AudioVoice Voice { get; private set; } = null!;
//
// 	void Awake() {
// 		Voice = gameObject.AddComponent<AudioVoice>();
// 		Voice.spatialBlend = 1f;
// 		Voice.playOnAwake = false;
// 		Voice.spread = 180f;
// 		Voice.minDistance = 10f;
// 		Voice.maxDistance = 500f;
// 	}
//
// 	void Update() {
// 		if(!enteredListener)
// 			CloseInOnListener();
// 		UpdateVoice();
//
// 		if(enteredListener && !Voice.isPlaying) {
// 			timeLeft -= Time.deltaTime * Pitch;
// 			if(timeLeft <= 0f)
// 				Destroy(gameObject);
// 		}
// 	}
//
// 	public float Pitch {
// 		get => pitch;
// 		set {
// 			pitch = value;
// 			UpdateVoice();
// 		}
// 	}
//
// 	public float Volume {
// 		get => volume;
// 		set {
// 			volume = value;
// 			UpdateVoice();
// 		}
// 	}
//
// 	public float MinimumRange {
// 		get => minimumRange;
// 		set {
// 			minimumRange = value;
// 			UpdateVoice();
// 		}
// 	}
//
// 	public float DopplerLevel {
// 		get => Voice.dopplerLevel;
// 		set => Voice.dopplerLevel = value;
// 	}
//
// 	public static SoundEffect3D PlayAtPitch(AudioClip clip, float pitch,
// 										  float polymorphicMultiplier = 1f) {
// 		SoundEffect3D se = SpawnSound(clip, Vector3.zero, polymorphicMultiplier);
// 		se.Pitch = pitch;
// 		se.Voice.Play();
// 		se.enteredListener = true;
// 		return se;
// 	}
//
// 	public static SoundEffect3D PlayAtVolume(AudioClip clip, float volume) {
// 		SoundEffect3D se = SpawnSound(clip, Vector3.zero, 1f);
// 		se.Volume = volume;
// 		se.Voice.Play();
// 		se.enteredListener = true;
// 		return se;
// 	}
//
// 	public static SoundEffect3D Play(AudioClip clip, float polymorphicMultiplier = 0.5f) {
// 		return Play(clip, Vector3.zero, polymorphicMultiplier);
// 	}
//
// 	public static SoundEffect3D Play(AudioClip clip, Vector3 position,
// 												float polymorphicMultiplier = 0.5f) {
// 		SoundEffect3D se = SpawnSound(clip, position, polymorphicMultiplier);
// 		if(clip != null)
// 			se.timeLeft = 3f;
// 		se.UpdateVoice();
// 		float listenerDistance = (Listener.transform.position - se.transform.position).magnitude;
// 		if(listenerDistance < INSTANT_RANGE) {
// 			se.Voice.Play();
// 			se.enteredListener = true;
// 		}
// 		return se;
// 	}
//
// 	public static SoundEffect3D Play(AudioClip clip, Vector3d position,
// 												float polymorphicMultiplier = 0.5f) {
// 		return Play(clip, (Vector3)position, polymorphicMultiplier);
// 	}
//
// 	private static SoundEffect3D SpawnSound(AudioClip clip, Vector3 position,
// 										  float polymorphicMultiplier) {
// 		var go = new GameObject("Sound Effect 3D");
// 		go.transform.position = position;
// 		var se = go.AddComponent<SoundEffect3D>();
// 		if(clip != null) {
// 			se.Voice.clip = clip;
// 			se.UpdateVoice();
// 			if((Time.time - polymorphicTime) > 0.015f) {
// 				POLYMORPHIC_ENTRIES.Clear();
// 				POLYMORPHIC_VOLUMES.Clear();
// 				polymorphicTime = Time.time;
// 			}
// 			int index = POLYMORPHIC_ENTRIES.IndexOf(clip);
// 			if(index == -1) {
// 				POLYMORPHIC_ENTRIES.Add(clip);
// 				POLYMORPHIC_VOLUMES.Add(1f);
// 			}
// 			else {
// 				POLYMORPHIC_VOLUMES[index] *= polymorphicMultiplier;
// 				se.Volume *= POLYMORPHIC_VOLUMES[index];
// 			}
// 		}
// 		return se;
// 	}
//
// 	private void CloseInOnListener() {
// 		float listenerDistance = (Listener.transform.position - transform.position).magnitude;
// 		if(distanceTraveled > listenerDistance + INSTANT_RANGE) {
// 			enteredListener = true;
// 			Voice.Play();
// 			Voice.time = (distanceTraveled - listenerDistance) / SPEED_OF_SOUND_IN_AIR;
// 		}
// 		else {
// 			distanceTraveled += SPEED_OF_SOUND_IN_AIR * Time.deltaTime;
// 			if(distanceTraveled >= listenerDistance) {
// 				enteredListener = true;
// 				Voice.Play();
// 			}
// 		}
// 	}
//
// 	private void UpdateVoice() {
// 		Voice.pitch = Time.timeScale * pitch;
// 		Voice.volume = (Game.Settings.soundEnabled ? volume * Game.Settings.SoundLevel : 0f);
// 		Voice.minDistance = minimumRange;
// 		Voice.maxDistance = minimumRange * MAX_RANGE_MULTIPLIER;
// 	}
// }

