using System;
using System.Numerics;

namespace CadExtract.Library.Geometry
{
    public struct Bounds
    {
        public Vector2 Min { get; }
        public Vector2 Max { get; }
        public Vector2 Center => (Max + Min) * 0.5f;
        public Vector2 Size => Max - Min;

        public Bounds(Vector2 a, Vector2 b)
        {
            Min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
            Max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        public Bounds(float xMin, float yMin, float xMax, float yMax)
                    : this(new Vector2(xMin, yMin), new Vector2(xMax, yMax))
        {
        }

        public static Bounds FromCenterSize(Vector2 center, Vector2 size)
        {
            return new Bounds(center - size * 0.5f, center + size * 0.5f);
        }

        public Bounds Translate(Vector2 vect) => new Bounds(Min + vect, Max + vect);

        public override string ToString() => $"({Min})->({Max})";
    }
}
