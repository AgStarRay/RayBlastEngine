using NAudio.Wave;

namespace RayBlast;

public class LoopingWaveStream : WaveStream {
    public readonly WaveStream baseStream;
    public long skipFrom;
    public long skipTo;

    public LoopingWaveStream(WaveStream baseStream) {
        this.baseStream = baseStream;
        try {
            baseStream.Position = baseStream.Position;
        }
        catch(Exception) {
            throw new RayBlastEngineException($"Base stream provided {baseStream.GetType().Name} is not seekable");
        }
    }

    public override int Read(byte[] buffer, int offset,
                             int count) {
        int blockAlign = WaveFormat.BlockAlign;
        if(skipTo == skipFrom || blockAlign == 0)
            return baseStream.Read(buffer, offset, count);
        long alignedSkipFrom = skipFrom * blockAlign;
        long trackedPosition = Position;
        if(trackedPosition > alignedSkipFrom)
            return baseStream.Read(buffer, offset, count);
        long alignedSkipTo = skipTo * blockAlign;
        var total = 0;
        int remaining = count;
        while(trackedPosition <= alignedSkipFrom && trackedPosition + remaining >= alignedSkipFrom) {
            int pass = baseStream.Read(buffer, offset + total, (int)(alignedSkipFrom - trackedPosition));
            if(pass == 0)
                return total;
            trackedPosition = Position = alignedSkipTo;
            total += pass;
            remaining -= pass;
        }
        total += baseStream.Read(buffer, offset + total, count - total);
        return total;
    }

    public override WaveFormat WaveFormat => baseStream.WaveFormat;

    public override long Length => baseStream.Length;

    public override long Position {
        get => baseStream.Position;
        set => baseStream.Position = value;
    }
}
