using System.Numerics;

namespace RayBlast;

public struct ParticleBurst {
	public double time;
	public int particleCount;
	public Vector3 center;

	public ParticleBurst() {
	}

	public ParticleBurst(int particleCount) {
		this.particleCount = particleCount;
	}

	public ParticleBurst(double time, int particleCount) {
		this.time = time;
		this.particleCount = particleCount;
	}

	public ParticleBurst(double time, int particleCount, Vector3 center) {
		this.time = time;
		this.particleCount = particleCount;
		this.center = center;
	}
}
