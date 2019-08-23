using CadExtract.Library;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CadExtract.WpfLibrary
{
    public static class ColorExtensions
    {
        private static readonly Random _rand = new Random();
        private static readonly Dictionary<int, Color?> _randomColors = new Dictionary<int, Color?>();
        public static Color RandomColor(int hashCode) => (Color)(_randomColors.GetOrNull(hashCode) ?? (_randomColors[hashCode] = Color.FromArgb(255, (byte)_rand.Next(), (byte)_rand.Next(), (byte)_rand.Next())));
    }
}
