using System.Numerics;

namespace RayBlast; 

public struct ImmediateCamera {
	public Vector3 position;
    public Vector3 target;
    public Vector3 up;
    public float fieldOfView;
	public float nearPlane;
	public float farPlane;
    public bool orthographic;
}
