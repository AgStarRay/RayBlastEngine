using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace RayBlast;

public class SoundClip : ISoundFiniteSource {
	public static readonly SoundClip SILENCE;

	public readonly float length;

	// private readonly IntPtr buffer;
	// private readonly int channels;
	// private readonly SDL.AudioFormat format;
	internal readonly WaveStream? stream;
	internal readonly byte[]? audioData;

	static SoundClip() {
        byte[] silenceWav = [
            0x52, 0x49, 0x46, 0x46, 0x28, 0x00, 0x00, 0x00, 0x57, 0x41,
            0x56, 0x45, 0x66, 0x6D, 0x74, 0x20, 0x10, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x01, 0x00, 0x40, 0x1F, 0x00, 0x00, 0x80, 0x3E,
            0x00, 0x00, 0x02, 0x00, 0x10, 0x00, 0x64, 0x61, 0x74, 0x61,
            0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        ];
		var signalGenerator = new SignalGenerator {
			Gain = 0.0, Frequency = 1.0, Type = SignalGeneratorType.Square
		};
		ISampleProvider sampleProvider = signalGenerator.Take(TimeSpan.FromMilliseconds(1));
		SILENCE = new SoundClip("<silence>", sampleProvider, 0, silenceWav);
	}

	// internal SoundClip(string name, SDL.AudioSpec spec,
	//                    IntPtr buffer, uint length) {
	//     Name = name;
	//     this.buffer = buffer;
	//     SampleCount = (int)length;
	//     Frequency = spec.Freq;
	//     channels = spec.Channels;
	//     format = spec.Format;
	//     this.length = SampleCount / (float)Frequency;
	// }

	internal SoundClip(string name, ISampleProvider sampleProvider,
					   int sampleCount, WaveStream? instancingStream) {
		Name = name;
		SampleProvider = sampleProvider;
		stream = instancingStream;
		SampleCount = sampleCount;
		Frequency = sampleProvider.WaveFormat.SampleRate;
		length = sampleCount / (float)sampleProvider.WaveFormat.SampleRate;
	}

	internal SoundClip(string name, ISampleProvider sampleProvider,
					   int sampleCount, byte[] audioData) {
		Name = name;
		SampleProvider = sampleProvider;
		this.audioData = audioData;
		SampleCount = sampleCount;
		Frequency = sampleProvider.WaveFormat.SampleRate;
		length = sampleCount / (float)sampleProvider.WaveFormat.SampleRate;
	}

	public int SampleCount { get; }
	public int Frequency { get; }
	public string Name { get; set; }
	public ISampleProvider SampleProvider { get; }
}
