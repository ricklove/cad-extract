using System;
using System.Numerics;

namespace CadExtract.Library.Geometry
{
    public struct Circle
    {
        public Vector2 Center { get; }
        public float Radius { get; }
        public Bounds Bounds { get; }

        public Circle(Vector2 center, float radius)
            : this()
        {
            Center = center;
            Radius = radius = Math.Abs(radius);
            Bounds = new Bounds(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);
        }

        public Circle(float x1, float y1, float radius)
            : this(new Vector2(x1, y1), radius)
        {
        }

        public override string ToString() => string.Format("({0:f2}-{1:f2},r{2:f2})", Center.X, Center.X, Radius);
    }
}
