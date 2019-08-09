using System;
using System.Numerics;

namespace CadExtract.Library
{
    public static class VectorExtensions
    {
        public static bool IsVertical(this Vector2 v, float allowedSlope_inverse = 0.1f)
        {
            if (v.X == 0 && v.Y != 0) { return true; }
            var slope_inverse = v.X / v.Y;
            return Math.Abs(slope_inverse) < allowedSlope_inverse;
        }

        public static bool IsHorizontal(this Vector2 v, float allowedSlope = 0.1f)
        {
            if (v.Y == 0 && v.X != 0) { return true; }
            var slope = v.Y / v.X;
            return Math.Abs(slope) < allowedSlope;
        }
    }
}
