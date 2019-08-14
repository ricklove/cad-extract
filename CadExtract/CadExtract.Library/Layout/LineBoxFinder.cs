using CadExtract.Library.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CadExtract.Library.Layout
{
    public static class LineBoxFinder
    {
        public static List<LineBox> FindLineBoxesWithTexts(List<CadLine> lines, List<CadText> texts, List<CadCircle> circles_excludeText, float rounding = 0.025f)
        {
            var boxes = FindLineBoxes(lines, rounding);

            var boxes_smallestFirst = boxes.OrderBy(x => x.Bounds.Size.X * x.Bounds.Size.Y).ToList();

            texts.ForEach(t =>
            {
                foreach (var c in circles_excludeText)
                {
                    if (c.Circle.Bounds.Contains(t.Bounds.Center))
                    {
                        return;
                    }
                }

                foreach (var b in boxes_smallestFirst)
                {
                    if (b.Bounds.Contains(t.Bounds.Center))
                    {
                        b.Texts.Add(t);
                        return;
                    }
                }
            });

            return boxes;
        }

        public static List<LineBox> FindLineBoxes(List<CadLine> lines, float rounding = 0.01f)
        {
            // For each bottom left corner, find the smallest cell that encloses it
            // NOTE: Start from each corner to ensure no skipping

            var lines_vert = lines.Where(x => x.Direction.IsVertical()).Select(x => new LineBox(new Bounds(x.Start, x.End))).ToList();
            var lines_hori = lines.Where(x => x.Direction.IsHorizontal()).Select(x => new LineBox(new Bounds(x.Start, x.End))).ToList();

            // Join line segments that are part of the same line
            List<LineBox> MergeLines(List<LineBox> l)
            {
                l = l.ToList();
                for (var i = 0; i < l.Count; i++)
                {
                    for (var j = i + 1; j < l.Count; j++)
                    {
                        if (l[i].Bounds.Intersects(l[j].Bounds, 0.01f))
                        {
                            l[i] = new LineBox(l[i].Bounds.Union(l[j].Bounds));
                            l.RemoveAt(j);
                            j--;
                        }
                    }
                }
                return l;
            }

            List<LineBox> ExtendLines(List<LineBox> l, List<LineBox> crossLines, bool isVertical)
            {
                l = l.ToList();

                for (var i = 0; i < l.Count; i++)
                {
                    var line = l[i];
                    var center = line.Bounds.Center;
                    var cross_aligned = crossLines.Where(x =>
                        (isVertical && x.Bounds.Min.X <= center.X + rounding && x.Bounds.Max.X >= center.X - rounding)
                        || (!isVertical && x.Bounds.Min.Y <= center.Y + rounding && x.Bounds.Max.Y >= center.Y - rounding))
                        .ToList();

                    if (!cross_aligned.Any()) { continue; }

                    var cross_start = cross_aligned
                        .Where(x => isVertical ? x.Bounds.Min.Y < line.Bounds.Min.Y + rounding : x.Bounds.Min.X < line.Bounds.Min.X + rounding)
                        .OrderBy(x => isVertical ? Math.Abs(x.Bounds.Center.Y - line.Bounds.Min.Y) : Math.Abs(x.Bounds.Center.X - line.Bounds.Min.X))
                        .First();

                    var cross_end = cross_aligned
                        .Where(x => isVertical ? x.Bounds.Max.Y > line.Bounds.Max.Y - rounding : x.Bounds.Max.X > line.Bounds.Max.X - rounding)
                        .OrderBy(x => isVertical ? Math.Abs(x.Bounds.Center.Y - line.Bounds.Max.Y) : Math.Abs(x.Bounds.Center.X - line.Bounds.Max.X))
                        .First();

                    // Extend if not touching
                    if (!cross_start.Bounds.Intersects(line.Bounds))
                    {
                        var cx = !isVertical ? cross_start.Bounds.Center.X : center.X;
                        var cy = isVertical ? cross_start.Bounds.Center.Y : center.Y;
                        var cv = new Vector2(cx, cy);
                        var len = isVertical ? line.Bounds.Size.Y : line.Bounds.Size.X;
                        var dist = isVertical ? Math.Abs(cv.Y - line.Bounds.Min.Y) : Math.Abs(cv.X - line.Bounds.Min.X);
                        if (dist < len)
                        {
                            l[i] = new LineBox(l[i].Bounds.Union(new Bounds(cv, cv)));
                        }
                    }

                    if (!cross_end.Bounds.Intersects(line.Bounds))
                    {
                        var cx = !isVertical ? cross_end.Bounds.Center.X : center.X;
                        var cy = isVertical ? cross_end.Bounds.Center.Y : center.Y;
                        var cv = new Vector2(cx, cy);
                        var len = isVertical ? line.Bounds.Size.Y : line.Bounds.Size.X;
                        var dist = isVertical ? Math.Abs(cv.Y - line.Bounds.Max.Y) : Math.Abs(cv.X - line.Bounds.Max.X);

                        if (dist < len)
                        {
                            l[i] = new LineBox(l[i].Bounds.Union(new Bounds(new Vector2(cx, cy), new Vector2(cx, cy))));
                        }
                    }
                }

                return l;
            }

            lines_hori = MergeLines(lines_hori);
            lines_vert = MergeLines(lines_vert);

            lines_hori = lines_hori.OrderBy(x => x.Bounds.Center.Y).ToList();
            lines_vert = lines_vert.OrderBy(x => x.Bounds.Center.X).ToList();

            lines_hori = ExtendLines(lines_hori, lines_vert, isVertical: false);
            lines_vert = ExtendLines(lines_vert, lines_hori, isVertical: true);

            var boxes = new List<LineBox>();

            for (var iRevHor = 0; iRevHor < 2; iRevHor++)
            {
                lines_hori.Reverse();

                for (var iRevVer = 0; iRevVer < 2; iRevVer++)
                {
                    lines_vert.Reverse();

                    for (var iBottom = 0; iBottom < lines_hori.Count - 1; iBottom++)
                    {
                        var lineBottom = lines_hori[iBottom];

                        for (var iLeft = 0; iLeft < lines_vert.Count - 1; iLeft++)
                        {
                            var lineLeft = lines_vert[iLeft];
                            if (!lineLeft.Bounds.Intersects(lineBottom.Bounds, rounding)) { continue; }

                            var hasEnclosedBottomLeftCorner = false;

                            for (var iRight = iLeft + 1; iRight < lines_vert.Count; iRight++)
                            {
                                var lineRight = lines_vert[iRight];
                                if (!lineRight.Bounds.Intersects(lineBottom.Bounds, rounding)) { continue; }

                                for (var iTop = iBottom + 1; iTop < lines_hori.Count; iTop++)
                                {
                                    var lineTop = lines_hori[iTop];
                                    if (!lineLeft.Bounds.Intersects(lineTop.Bounds, rounding)) { continue; }
                                    if (!lineRight.Bounds.Intersects(lineTop.Bounds, rounding)) { continue; }


                                    var bounds = new Bounds(new Vector2(lineLeft.Bounds.Center.X, lineBottom.Bounds.Center.Y), new Vector2(lineRight.Bounds.Center.X, lineTop.Bounds.Center.Y));
                                    boxes.Add(new LineBox(bounds)
                                    {
                                        //Debug_LineLeft = lineLeft,
                                        //Debug_LineBottom = lineBottom,
                                        //Debug_LineRight = lineRight,
                                        //Debug_LineTop = lineTop,
                                    });

                                    hasEnclosedBottomLeftCorner = true;
                                    break;
                                }

                                if (hasEnclosedBottomLeftCorner) { break; }
                            }
                        }
                    }
                }
            }

            //// Remove boxes that contain other cells
            //var emptyBoxes = boxes.Where(x => !boxes.Any(y => x != y && x.Bounds.Contains(y.Bounds))).ToList();
            //return emptyBoxes;

            boxes = boxes.GroupBy(b => b.Bounds).Select(g => g.First()).ToList();

            return boxes;
        }

    }
}
