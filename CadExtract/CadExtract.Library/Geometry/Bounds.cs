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

        public static Bounds FromCenterSize(Vector2 center, Vector2 size) => new Bounds(center - size * 0.5f, center + size * 0.5f);

        public Bounds Translate(Vector2 vect) => new Bounds(Min + vect, Max + vect);

        public Bounds Intersect(Bounds bounds)
        {
            if (!Intersects(bounds)) { return new Bounds(); }

            var xMin = Math.Max(Min.X, bounds.Min.X);
            var yMin = Math.Max(Min.Y, bounds.Min.Y);
            var xMax = Math.Min(Max.X, bounds.Max.X);
            var yMax = Math.Min(Max.Y, bounds.Max.Y);

            return new Bounds(new Vector2(xMin, yMin), new Vector2(xMax, yMax));
        }

        public Bounds Union(Bounds bounds)
        {
            var xMin = Math.Min(Min.X, bounds.Min.X);
            var yMin = Math.Min(Min.Y, bounds.Min.Y);
            var xMax = Math.Max(Max.X, bounds.Max.X);
            var yMax = Math.Max(Max.Y, bounds.Max.Y);

            return new Bounds(new Vector2(xMin, yMin), new Vector2(xMax, yMax));
        }

        public bool Intersects(Bounds bounds, float rounding = 0f) =>
               Min.X <= bounds.Max.X + rounding
            && Max.X >= bounds.Min.X - rounding
            && Min.Y <= bounds.Max.Y + rounding
            && Max.Y >= bounds.Min.Y - rounding
            ;

        public bool Contains(Bounds bounds, float rounding = 0f) =>
               Min.X <= bounds.Min.X + rounding
            && Max.X >= bounds.Max.X - rounding
            && Min.Y <= bounds.Min.Y + rounding
            && Max.Y >= bounds.Max.Y - rounding
            ;

        public bool Contains(Vector2 center) => Contains(Bounds.FromCenterSize(center, new Vector2(0, 0)));

        public override string ToString() => $"({Min})->({Max})";
    }
}
