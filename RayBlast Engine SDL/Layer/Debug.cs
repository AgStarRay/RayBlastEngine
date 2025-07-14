using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using Cysharp.Text;

namespace RayBlast;

public static class Debug {
	public static event Action<string, string, LogLevel>? logMessageReceived = null;
	public static event Action<string, string>? fatalMessageReceived = null;
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void LogTrace(string? message) {
		#if TRACE
		if(message != null)
			RayBlastEngine.RecordLog(LogLevel.Trace, message, null);
		#endif
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void LogDebug(string? message, bool includeStackTrace = true) {
		#if DEBUG
        if(includeStackTrace || message != null) {
            ReadOnlySpan<char> stackTrace = includeStackTrace ? TrimmedStackTrace() : null;
            RayBlastEngine.RecordLog(LogLevel.Debug, message ?? "<blank>", stackTrace);
        }
		#endif
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Log(string? message, bool includeStackTrace = false) {
		if(includeStackTrace || message != null) {
			ReadOnlySpan<char> stackTrace = includeStackTrace ? TrimmedStackTrace() : null;
			RayBlastEngine.RecordLog(LogLevel.Info, message ?? "<blank>", stackTrace);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Log(StringBuilder builder, bool includeStackTrace = false) {
		ReadOnlySpan<char> stackTrace = includeStackTrace ? TrimmedStackTrace() : null;
		RayBlastEngine.RecordLog(LogLevel.Info, builder, stackTrace);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Log(Utf8ValueStringBuilder builder, bool includeStackTrace = false) {
		ReadOnlySpan<char> stackTrace = includeStackTrace ? TrimmedStackTrace() : null;
		RayBlastEngine.RecordLog(LogLevel.Info, builder, stackTrace);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Log(Utf16ValueStringBuilder builder, bool includeStackTrace = false) {
		ReadOnlySpan<char> stackTrace = includeStackTrace ? TrimmedStackTrace() : null;
		RayBlastEngine.RecordLog(LogLevel.Info, builder.AsSpan(), stackTrace);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogWarning(string? message, bool includeStackTrace = false) {
		if(includeStackTrace || message != null) {
			ReadOnlySpan<char> stackTrace = includeStackTrace ? TrimmedStackTrace() : null;
			RayBlastEngine.RecordLog(LogLevel.Warning, message ?? "<blank>", stackTrace);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogWarning(StringBuilder builder, bool includeStackTrace = false) {
		ReadOnlySpan<char> stackTrace = includeStackTrace ? TrimmedStackTrace() : null;
		RayBlastEngine.RecordLog(LogLevel.Warning, builder, stackTrace);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogWarning(Utf8ValueStringBuilder builder, bool includeStackTrace = false) {
		ReadOnlySpan<char> stackTrace = includeStackTrace ? TrimmedStackTrace() : null;
		RayBlastEngine.RecordLog(LogLevel.Warning, builder, stackTrace);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogWarning(Utf16ValueStringBuilder builder, bool includeStackTrace = false) {
		ReadOnlySpan<char> stackTrace = includeStackTrace ? TrimmedStackTrace() : null;
		RayBlastEngine.RecordLog(LogLevel.Warning, builder.AsSpan(), stackTrace);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogError(string? message, bool includeStackTrace = true) {
		if(includeStackTrace || message != null) {
			ReadOnlySpan<char> stackTrace = includeStackTrace ? TrimmedStackTrace() : null;
			RayBlastEngine.RecordLog(LogLevel.Error, message ?? "<blank>", stackTrace);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogError(StringBuilder builder, bool includeStackTrace = true) {
		ReadOnlySpan<char> stackTrace = includeStackTrace ? TrimmedStackTrace() : null;
		RayBlastEngine.RecordLog(LogLevel.Error, builder, stackTrace);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogError(Utf8ValueStringBuilder builder, bool includeStackTrace = true) {
		ReadOnlySpan<char> stackTrace = includeStackTrace ? TrimmedStackTrace() : null;
		RayBlastEngine.RecordLog(LogLevel.Error, builder, stackTrace);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogError(Utf16ValueStringBuilder builder, bool includeStackTrace = true) {
		ReadOnlySpan<char> stackTrace = includeStackTrace ? TrimmedStackTrace() : null;
		RayBlastEngine.RecordLog(LogLevel.Error, builder.AsSpan(), stackTrace);
	}

	public static void LogException(Exception exception) {
		RayBlastEngine.RecordLog(LogLevel.Fatal, exception.Message, exception.StackTrace);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static ReadOnlySpan<char> TrimmedStackTrace() {
		#if RAYBLAST_DEBUG
			string fullStackTrace = Environment.StackTrace;
			int firstNewline = fullStackTrace.IndexOf('\n') + 1;
			return fullStackTrace.AsSpan(Math.Max(firstNewline, fullStackTrace.IndexOf('\n', firstNewline) + 1));
		#else
		using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();
		var stackTrace = new StackTrace();
		StackFrame[] frames = stackTrace.GetFrames();
		const int firstIndex = 2;
		int endIndex = Math.Min(Math.Min(Math.Max(frames.Length - 2, firstIndex + 3), firstIndex + 5), frames.Length);
		for(int i = firstIndex; i < endIndex; i++) {
			StackFrame sf = frames[i];
			MethodBase? mb = sf.GetMethod();
			if(i > firstIndex)
				builder.AppendLine();
			if(mb != null && ShowInStackTrace(mb)) {
				builder.Append("  at ");
				Type? declaringType = mb.DeclaringType;
				string methodName = mb.Name;
				bool methodChanged = false;
				if(declaringType != null && declaringType.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)) {
					bool isAsync = declaringType.IsAssignableTo(typeof(IAsyncStateMachine));
					if(isAsync || declaringType.IsAssignableTo(typeof(IEnumerator))) {
						methodChanged = TryResolveStateMachineMethod(ref mb, out declaringType);
					}
				}

				if(declaringType != null) {
					string fullName = declaringType.FullName!;
					foreach(char ch in fullName) {
						builder.Append(ch == '+' ? '.' : ch);
					}
					builder.Append('.');
				}
				builder.Append(mb.Name);

				if(mb is MethodInfo { IsGenericMethod: true } mi) {
					Type[] typars = mi.GetGenericArguments();
					builder.Append('[');
					int k = 0;
					bool fFirstTyParam = true;
					while(k < typars.Length) {
						if(!fFirstTyParam)
							builder.Append(',');
						else
							fFirstTyParam = false;

						builder.Append(typars[k].Name);
						k++;
					}
					builder.Append(']');
				}

				ParameterInfo[]? pi = null;
				try {
					pi = mb.GetParameters();
				}
				catch {
					// The parameter info cannot be loaded, so we don't
					// append the parameter list.
				}
				if(pi != null) {
					builder.Append('(');
					bool firstParam = true;
					foreach(ParameterInfo t in pi) {
						if(!firstParam)
							builder.Append(", ");
						else
							firstParam = false;

						// ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
						string typeName = t.ParameterType?.Name ?? "<UnknownType>";
						builder.Append(typeName);
						string? parameterName = t.Name;
						if(parameterName != null) {
							builder.Append(' ');
							builder.Append(parameterName);
						}
					}
					builder.Append(')');
				}

				if(methodChanged) {
					builder.Append('+');
					builder.Append(methodName);
					builder.Append('(');
					builder.Append(')');
				}
			}
			else
				builder.Append("  at <unknown>");
		}
		return builder.AsSpan();
		#endif
	}

	#if !RAYBLAST_DEBUG
	private static bool ShowInStackTrace(MethodBase mb) {
		// ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
		if((mb.MethodImplementationFlags & MethodImplAttributes.AggressiveInlining) != 0)
			return false;
		try {
			if(mb.IsDefined(typeof(StackTraceHiddenAttribute), inherit: false))
				return false;

			Type? declaringType = mb.DeclaringType;
			// Methods don't always have containing types, for example dynamic RefEmit generated methods.
			if(declaringType != null &&
			   declaringType.IsDefined(typeof(StackTraceHiddenAttribute), inherit: false))
				return false;
		}
		catch {
			// Getting the StackTraceHiddenAttribute has failed, behave as if it was not present.
			// One of the reasons can be that the method mb or its declaring type use attributes
			// defined in an assembly that is missing.
		}

		return true;
	}

	private static bool TryResolveStateMachineMethod(ref MethodBase method, out Type declaringType) {
		System.Diagnostics.Debug.Assert(method.DeclaringType != null);

		declaringType = method.DeclaringType;

		Type? parentType = declaringType.DeclaringType;
		if(parentType == null) {
			return false;
		}

		IEnumerable<MethodInfo> methods = GetDeclaredMethods(parentType);

		foreach(MethodInfo candidateMethod in methods) {
			StateMachineAttribute[] attributes =
				(StateMachineAttribute[])Attribute.GetCustomAttributes(candidateMethod, typeof(StateMachineAttribute), inherit: false);
			bool foundAttribute = false, foundIteratorAttribute = false;
			foreach(StateMachineAttribute asma in attributes) {
				if(asma.StateMachineType == declaringType) {
					foundAttribute = true;
					foundIteratorAttribute |= asma is IteratorStateMachineAttribute || asma is AsyncIteratorStateMachineAttribute;
				}
			}

			if(foundAttribute) {
				method = candidateMethod;
				declaringType = candidateMethod.DeclaringType!;
				return foundIteratorAttribute;
			}
		}

		return false;

		[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070:UnrecognizedReflectionPattern",
									  Justification =
										  "Using Reflection to find the state machine's corresponding method is safe because the corresponding method is the only "
										+ "caller of the state machine. If the state machine is present, the corresponding method will be, too.")]
		static IEnumerable<MethodInfo> GetDeclaredMethods(IReflect type) =>
			type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
	}
	#endif

	public static string SystemProcessorName {
		get {
			if(!X86Base.IsSupported)
				return "Unknown (no CPUID)";
			// ReSharper disable once IdentifierTypo
			(int eax, int _, int _, int _) = X86Base.CpuId(unchecked((int)0x80000000), 0);
			if(eax < unchecked((int)0x80000004))
				return "Unknown (no brand string)";
			Span<int> raw = stackalloc int[12];
			(raw[0], raw[1], raw[2], raw[3]) = X86Base.CpuId(unchecked((int)0x80000002), 0);
			(raw[4], raw[5], raw[6], raw[7]) = X86Base.CpuId(unchecked((int)0x80000003), 0);
			(raw[8], raw[9], raw[10], raw[11]) = X86Base.CpuId(unchecked((int)0x80000004), 0);
			Span<byte> bytes = MemoryMarshal.AsBytes(raw);
			return Encoding.UTF8.GetString(bytes).TrimEnd('\0').Trim();
		}
	}

	public static void HandleMessageReceived(string logMessage, ReadOnlySpan<char> stackTrace,
											 LogLevel msgType) {
		logMessageReceived?.Invoke(logMessage, stackTrace.ToString(), msgType);
		if(msgType == LogLevel.Fatal)
			fatalMessageReceived?.Invoke(logMessage, stackTrace.ToString());
	}

	public static void HandleMessageReceived(StringBuilder logMessage, ReadOnlySpan<char> stackTrace,
											 LogLevel msgType) {
		logMessageReceived?.Invoke(logMessage.ToString(), stackTrace.ToString(), msgType);
		if(msgType == LogLevel.Fatal)
			fatalMessageReceived?.Invoke(logMessage.ToString(), stackTrace.ToString());
	}

	public static void HandleMessageReceived(ReadOnlySpan<char> logMessage, ReadOnlySpan<char> stackTrace,
											 LogLevel msgType) {
		logMessageReceived?.Invoke(logMessage.ToString(), stackTrace.ToString(), msgType);
		if(msgType == LogLevel.Fatal)
			fatalMessageReceived?.Invoke(logMessage.ToString(), stackTrace.ToString());
	}

	public static void HandleMessageReceived(Utf8ValueStringBuilder logMessage, ReadOnlySpan<char> stackTrace,
											 LogLevel msgType) {
		logMessageReceived?.Invoke(logMessage.ToString(), stackTrace.ToString(), msgType);
		if(msgType == LogLevel.Fatal)
			fatalMessageReceived?.Invoke(logMessage.ToString(), stackTrace.ToString());
	}
}
