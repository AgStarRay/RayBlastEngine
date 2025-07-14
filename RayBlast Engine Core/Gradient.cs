namespace RayBlast;

public class Gradient {
    public Key[] keys = [];

    public ColorF Evaluate(float position) {
        if(keys.Length == 0)
            return ColorF.CLEAR;
        if(position < keys[0].position)
            return keys[0].color;
        for(int i = 1; i < keys.Length; i++) {
            if(position < keys[i].position)
                return ColorF.Lerp(keys[i - 1].color, keys[i].color, Mathd.InverseLerp(keys[i - 1].position, keys[i].position, position));
        }
        return keys.Last().color;
    }

    public struct Key {
        public ColorF color;
        public float position;

        public Key(ColorF color, float position) {
            this.color = color;
            this.position = position;
        }
    }
}
