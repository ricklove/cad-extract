using CadExtract.Library.Geometry;
using System.Collections.Generic;
using System.Numerics;

namespace CadExtract.Library
{
    public class CadText
    {
        public Bounds Bounds { get; set; }
        public string Text { get; set; }
        public string Context { get; set; }
        public float FontHeight { get; set; }

        public object Debug_Raw { get; set; }
        public override string ToString() => $"{Text} {Bounds}";
    }

    public class CadLine
    {
        public Vector2 Start { get; set; }
        public Vector2 End { get; set; }
        public string Context { get; set; }
        public object Debug_Raw { get; set; }

        public override string ToString() => $"{Start} {End}";
    }

    public class CadCircle
    {
        public Circle Circle { get; set; }
        public string Context { get; set; }
        public object Debug_Raw { get; set; }

        public override string ToString() => $"{Circle}";
    }

    public class CadData
    {
        public List<CadText> Texts { get; set; }
        public List<CadLine> Lines { get; set; }
        public List<CadCircle> Circles { get; set; }
    }
}
