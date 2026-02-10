using NAudio.Wave;

namespace RayBlast;

public interface ISoundSource {
	internal ISampleProvider SampleProvider { get; }
	string Name { get; }
	int Frequency { get; }
}
