using System.Numerics;

namespace RayBlast;

public class Bezier {
    public static Vector2 Interpolate(Vector2 start, Vector2 end,
                                           Vector2 guide, float t) {
        Vector2 a = Vector2.Lerp(start, guide, t);
        Vector2 b = Vector2.Lerp(guide, end, t);
        return Vector2.Lerp(a, b, t);
    }
}
