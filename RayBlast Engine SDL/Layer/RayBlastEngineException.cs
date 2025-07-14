namespace RayBlast;

internal class RayBlastEngineException : Exception {
	public RayBlastEngineException(string message) : base(message) {
	}

	public RayBlastEngineException(string message, Exception? innerException) : base(message, innerException) {
	}
}
