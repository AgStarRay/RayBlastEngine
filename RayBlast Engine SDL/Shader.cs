namespace RayBlast;

public struct Shader {
	// internal Raylib_cs.Shader internalShader;
	//
	// internal Shader(Raylib_cs.Shader internalShader) {
	//     this.internalShader = internalShader;
	// }

	public static Shader Load(string? vertexCode, string? fragmentCode) {
		Debug.LogDebug("Load ShaderFromMemory");
		throw new NotImplementedException();
	}

	public readonly int GetLocation(string uniformName) {
		throw new NotImplementedException();
	}

	public readonly void SetValue<T>(int uniformLocation, T value,
									 ShaderUniformType type) where T : unmanaged {
		throw new NotImplementedException();
	}
}

public enum ShaderUniformType {
	FLOAT,
	VEC2,
	VEC3,
	VEC4,
	INT,
	IVEC2,
	IVEC3,
	IVEC4,
	SAMPLER2D
}
