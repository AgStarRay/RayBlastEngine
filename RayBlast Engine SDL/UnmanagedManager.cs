using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using NAudio.Wave;

namespace RayBlast;

internal static class UnmanagedManager {
	internal static int mainThreadId;
    private static readonly ConcurrentQueue<Stream> STREAMS_TO_UNLOAD = new();

	internal static void RecoverResources() {
        while(STREAMS_TO_UNLOAD.TryDequeue(out Stream? stream)) {
            stream.Dispose();
        }
	}

	internal static void AssertMainThread() {
		#if RAYBLAST_DEBUG
        if(Environment.CurrentManagedThreadId != mainThreadId)
            throw new RayBlastEngineException("Not on main thread!");
		#endif
	}

	internal static unsafe void AssertProperPointer(sbyte* utf8Ptr) {
		#if RAYBLAST_DEBUG
        if(utf8Ptr < (void*)0x100)
            throw new RayBlastEngineException($"Bad pointer! {utf8Ptr->ToString()}");
		#endif
	}

	public static void EnqueueUnloadStream(Stream stream) {
		STREAMS_TO_UNLOAD.Enqueue(stream);
	}
}
