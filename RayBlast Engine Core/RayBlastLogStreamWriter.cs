using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RayBlast;

public class RayBlastLogStreamWriter : TextWriter {
	private Encoding encoding = Encoding.Default;
	private readonly TextWriter consoleWriter = Console.Out;
	private readonly TextWriter fileStreamWriter;

	public bool criticalErrors = true;
	private long lastFileFlush;
	private int flushesAttempted;

	public RayBlastLogStreamWriter(TextWriter fileStreamWriter) {
		this.fileStreamWriter = fileStreamWriter;
	}

	public override Encoding Encoding => encoding;

	public void SetEncoding(Encoding value) {
		encoding = value;
	}

	[AllowNull]
	public override string NewLine {
		get => base.NewLine;
		set {
			consoleWriter.NewLine = fileStreamWriter.NewLine = value;
			base.NewLine = value;
		}
	}

	public override void Close() {
		consoleWriter.Close();
		fileStreamWriter.Close();
		base.Close();
	}

	protected override void Dispose(bool disposing) {
		if(disposing) {
			consoleWriter.Dispose();
			fileStreamWriter.Dispose();
		}
		base.Dispose(disposing);
	}
	//
	// public override void Flush() {
	// 	if((++flushesAttempted & 15) == 0) {
	// 		consoleWriter.Flush();
	// 		fileStreamWriter.Flush();
	// 	}
	// }

	public void FlushNow() {
		fileStreamWriter.Flush();
	}

	//
	// The write operators
	//

	public override void Write(bool value) {
		consoleWriter.Write(value);
		fileStreamWriter.Write(value);
	}

	public override void Write(char value) {
		consoleWriter.Write(value);
		fileStreamWriter.Write(value);
	}

	public override void Write(char[]? buffer) {
		consoleWriter.Write(buffer);
		fileStreamWriter.Write(buffer);
	}

	public override void Write(decimal value) {
		consoleWriter.Write(value);
		fileStreamWriter.Write(value);
	}

	public override void Write(double value) {
		consoleWriter.Write(value);
		fileStreamWriter.Write(value);
	}

	public override void Write(float value) {
		consoleWriter.Write(value);
		fileStreamWriter.Write(value);
	}

	public override void Write(int value) {
		consoleWriter.Write(value);
		fileStreamWriter.Write(value);
	}

	public override void Write(long value) {
		consoleWriter.Write(value);
		fileStreamWriter.Write(value);
	}

	public override void Write(object? value) {
		consoleWriter.Write(value);
		fileStreamWriter.Write(value);
	}

	public override void Write(string? value) {
		consoleWriter.Write(value.AsSpan());
		fileStreamWriter.Write(value.AsSpan());
	}

	public override void Write(StringBuilder? value) {
		consoleWriter.Write(value);
		fileStreamWriter.Write(value);
	}

	public override void Write(ReadOnlySpan<char> buffer) {
		consoleWriter.Write(buffer);
		fileStreamWriter.Write(buffer);
	}

	public override void Write(uint value) {
		consoleWriter.Write(value);
		fileStreamWriter.Write(value);
	}

	public override void Write(ulong value) {
		consoleWriter.Write(value);
		fileStreamWriter.Write(value);
	}

	public override void Write(string format, object? arg0) {
		consoleWriter.Write(format, arg0);
		fileStreamWriter.Write(format, arg0);
	}

	public override void Write(string format, params object?[] arg) {
		consoleWriter.Write(format, arg);
		fileStreamWriter.Write(format, arg);
	}

	public override void Write(char[] buffer, int index,
							   int count) {
		consoleWriter.Write(buffer, index, count);
		fileStreamWriter.Write(buffer, index, count);
	}

	public override void Write(string format, object? arg0,
							   object? arg1) {
		consoleWriter.Write(format, arg0, arg1);
		fileStreamWriter.Write(format, arg0, arg1);
	}

	public override void Write(string format, object? arg0,
							   object? arg1, object? arg2) {
		consoleWriter.Write(format, arg0, arg1, arg2);
		fileStreamWriter.Write(format, arg0, arg1, arg2);
	}

	public override void WriteLine() {
		consoleWriter.WriteLine();
		fileStreamWriter.WriteLine();
	}

	public override void WriteLine(bool value) {
		consoleWriter.WriteLine(value);
		fileStreamWriter.WriteLine(value);
	}

	public override void WriteLine(char value) {
		consoleWriter.WriteLine(value);
		fileStreamWriter.WriteLine(value);
	}

	public override void WriteLine(char[]? buffer) {
		consoleWriter.WriteLine(buffer);
		fileStreamWriter.WriteLine(buffer);
	}

	public override void WriteLine(decimal value) {
		consoleWriter.WriteLine(value);
		fileStreamWriter.WriteLine(value);
	}

	public override void WriteLine(double value) {
		consoleWriter.WriteLine(value);
		fileStreamWriter.WriteLine(value);
	}

	public override void WriteLine(float value) {
		consoleWriter.WriteLine(value);
		fileStreamWriter.WriteLine(value);
	}

	public override void WriteLine(int value) {
		consoleWriter.WriteLine(value);
		fileStreamWriter.WriteLine(value);
	}

	public override void WriteLine(long value) {
		consoleWriter.WriteLine(value);
		fileStreamWriter.WriteLine(value);
	}

	public override void WriteLine(object? value) {
		consoleWriter.WriteLine(value);
		fileStreamWriter.WriteLine(value);
	}

	public override void WriteLine(string? value) {
		consoleWriter.WriteLine(value);
		fileStreamWriter.WriteLine(value);
	}

	public override void WriteLine(StringBuilder? value) {
		consoleWriter.WriteLine(value);
		fileStreamWriter.WriteLine(value);
	}

	public override void WriteLine(ReadOnlySpan<char> buffer) {
		consoleWriter.WriteLine(buffer);
		fileStreamWriter.WriteLine(buffer);
	}

	public override void WriteLine(uint value) {
		consoleWriter.WriteLine(value);
		fileStreamWriter.WriteLine(value);
	}

	public override void WriteLine(ulong value) {
		consoleWriter.WriteLine(value);
		fileStreamWriter.WriteLine(value);
	}

	public override void WriteLine(string format, object? arg0) {
		consoleWriter.WriteLine(format, arg0);
		fileStreamWriter.WriteLine(format, arg0);
	}

	public override void WriteLine(string format, params object?[] arg) {
		consoleWriter.WriteLine(format, arg);
		fileStreamWriter.WriteLine(format, arg);
	}

	public override void WriteLine(char[] buffer, int index,
								   int count) {
		consoleWriter.WriteLine(buffer, index, count);
		fileStreamWriter.WriteLine(buffer, index, count);
	}

	public override void WriteLine(string format, object? arg0,
								   object? arg1) {
		consoleWriter.WriteLine(format, arg0, arg1);
		fileStreamWriter.WriteLine(format, arg0, arg1);
	}

	public override void WriteLine(string format, object? arg0,
								   object? arg1, object? arg2) {
		consoleWriter.WriteLine(format, arg0, arg1, arg2);
		fileStreamWriter.WriteLine(format, arg0, arg1, arg2);
	}

	public void WriteLogLevel(LogLevel msgType) {
		consoleWriter.Write(msgType switch {
			LogLevel.All => "\x1b[37mALL ",
			LogLevel.Trace => "\x1b[32mTRC ",
			LogLevel.Debug => "\x1b[36mDEB ",
			LogLevel.Info => "\x1b[97mINF ",
			LogLevel.Warning => "\x1b[33mWRN ",
			LogLevel.Error => "\x1b[91mERR ",
			LogLevel.Fatal when criticalErrors => "\x1b[31mCRT ",
			LogLevel.Fatal => "\x1b[31mFTL ",
			_ => $"\x1b[35m? {msgType} "
		});
		fileStreamWriter.Write(msgType switch {
			LogLevel.All => "ALL ",
			LogLevel.Trace => "TRC ",
			LogLevel.Debug => "DEB ",
			LogLevel.Info => "INF ",
			LogLevel.Warning => "WRN ",
			LogLevel.Error => "ERR ",
			LogLevel.Fatal when criticalErrors => "CRT ",
			LogLevel.Fatal => "FTL ",
			_ => $"? {msgType} "
		});
	}

	public void ResetLogLevel() {
		consoleWriter.Write("\x1b[0m");
	}
}
