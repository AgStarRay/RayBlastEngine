using System.Runtime.InteropServices;
using NAudio.Vorbis;
using NAudio.Wave;
using NVorbis;
using SDL3;

namespace RayBlast;

public class RayBlastSoundHttp : RayBlastHttp {
	private readonly Uri uri;
	private readonly SoundFileType soundFileType;
	private Thread? waveThread;
	private readonly ulong bytesToLoad;
	private Exception? caughtException;
	private ISampleProvider? retrievedSampleProvider;
	private int retrievedSampleCount;
	private WaveStream? retrievedStream;
	private byte[]? retrievedAudioData;
	// private SDL.AudioSpec retrievedSpec;
	// private IntPtr retrievedBuffer;
	// private uint retrievedLength;

	private RayBlastSoundHttp(Uri uri, SoundFileType soundFileType) {
		this.uri = uri;
		this.soundFileType = soundFileType;
		if(uri.IsFile)
			bytesToLoad = (ulong)new FileInfo(uri.LocalPath).Length;
	}

	public bool LoadAsStreamable { get; set; }

	public override bool IsDone => (uri.IsFile && ThreadFinished) || base.IsDone;
	public override Result State => uri.IsFile ? ThreadFinished ? Result.Success : Result.InProgress : base.State;
	public override ulong DownloadedByteCount => uri.IsFile ? bytesToLoad : base.DownloadedByteCount;
	public override float DownloadProgress => uri.IsFile ? ThreadFinished ? 1f : 0.5f : base.DownloadProgress;

	private bool ThreadFinished => waveThread is { IsAlive: false };

	public SoundClip GetSound() {
		if(uri.IsFile) {
			int lastSlashIndex = uri.LocalPath.LastIndexOf('\\');
			if(waveThread == null)
				throw new RayBlastEngineException("Sound did not start loading, use SendRequest() first");
			waveThread?.Join(5000);
			string name = uri.LocalPath[(lastSlashIndex + 1)..];
			if(caughtException != null) {
				throw new RayBlastEngineException($"Failed to load {name}", caughtException);
			}
			if(retrievedSampleProvider == null)
				throw new RayBlastEngineException($"Failed to get a sample provider for {name}");
			if(retrievedAudioData != null)
				return new SoundClip(name, retrievedSampleProvider, retrievedSampleCount, retrievedAudioData);
			if(retrievedStream != null)
				return new SoundClip(name, retrievedSampleProvider, retrievedSampleCount, retrievedStream);
			throw new RayBlastEngineException($"Failed to get the stream or sample data for {name}");
		}
		throw new NotImplementedException();
	}

	public static RayBlastSoundHttp CreateSoundGet(Uri uri, SoundFileType soundType) {
		return new RayBlastSoundHttp(uri, soundType);
	}

	public override void SendRequest() {
		if(uri.IsFile || uri.IsLoopback) {
			if(waveThread == null) {
				waveThread = new Thread(LoadLocalWave);
				waveThread.Start();
			}
			else
				throw new RayBlastEngineException("Request already sent");
		}
		else
			base.SendRequest();
	}

	private unsafe void LoadLocalWave() {
		try {
			var fi = new FileInfo(uri.LocalPath);
			if(!fi.Exists)
				throw new FileNotFoundException(null, uri.LocalPath);
			Debug.LogDebug($"Load Wave {uri.LocalPath}");
			if(fi.Extension == ".ogg") {
				var vorbisReader = new VorbisWaveReader(uri.LocalPath);
				retrievedSampleProvider = vorbisReader;
				int bytesPerSample = vorbisReader.WaveFormat.Channels * vorbisReader.WaveFormat.BitsPerSample / 8;
				retrievedSampleCount = (int)(vorbisReader.Length / bytesPerSample);
				retrievedStream = vorbisReader;
				// 	using var stream = new MemoryStream();
				// 	WaveFileWriter.WriteWavFileToStream(stream, vorbisReader);
				// 	fixed(void* ptr = stream.GetBuffer()) {
				// 	    if(!SDL.LoadWAVIO(SDL.IOFromMem((IntPtr)ptr, (ulong)stream.Length), true, out retrievedSpec, out retrievedBuffer, out retrievedLength))
				// 	        throw new RayBlastEngineException($"Failed to load OGG {uri.LocalPath}: {SDL.GetError()}");
				// 	}
			}
			else {
				var audioFileReader = new AudioFileReader(uri.LocalPath);
				retrievedSampleProvider = audioFileReader;
				int bytesPerSample = audioFileReader.WaveFormat.Channels * audioFileReader.WaveFormat.BitsPerSample / 8;
				retrievedSampleCount = (int)(audioFileReader.Length / bytesPerSample);
				retrievedStream = audioFileReader;
				// 	if(!SDL.LoadWAV(uri.LocalPath, out retrievedSpec, out retrievedBuffer, out retrievedLength))
				// 	    throw new RayBlastEngineException($"Failed to load WAV {uri.LocalPath}: {SDL.GetError()}");
			}
			if(!LoadAsStreamable) {
				var memoryStream = new MemoryStream();
				WaveFileWriter.WriteWavFileToStream(memoryStream, retrievedSampleProvider.ToWaveProvider());
				retrievedAudioData = memoryStream.ToArray();
				retrievedStream = null;
			}
		}
		catch(Exception e) {
			caughtException = e;
		}
	}
}
