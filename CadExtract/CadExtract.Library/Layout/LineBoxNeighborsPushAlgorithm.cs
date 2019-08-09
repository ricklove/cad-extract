using System;
using System.Collections.Generic;
using System.Linq;

namespace CadExtract.Library.Layout
{
    public static class LineBoxNeighborsPushAlgorithm
    {
        public static List<LineBoxNeighbors> Solve(List<LineBox> boxes, float rounding = 0f)
        {
            var boxes_withNeighbors = boxes.Select(x => new LineBoxNeighbors(x)).ToList();

            FindNeighbors(boxes_withNeighbors, rounding);
            SolveIndexes(boxes_withNeighbors);
            AssignTables(boxes_withNeighbors);

            return boxes_withNeighbors;
        }

        private static void FindNeighbors(List<LineBoxNeighbors> boxes, float rounding)
        {
            for (var i = 0; i < boxes.Count; i++)
            {
                var a = boxes[i];

                for (var j = i + 1; j < boxes.Count; j++)
                {
                    var b = boxes[j];
                    var intersect = a.Bounds.Intersect(b.Bounds);
                    if (intersect.Size.X <= 0 && intersect.Size.Y <= 0) { continue; }

                    if (a.Bounds.Contains(b.Bounds, rounding)
                        || b.Bounds.Contains(a.Bounds, rounding))
                    {
                        a.Neighbors_Overlapping.Add(b);
                        b.Neighbors_Overlapping.Add(a);
                        continue;
                    }

                    if (a.Bounds.Max.X <= b.Bounds.Min.X + rounding) { a.Neighbors_Right.Add(b); b.Neighbors__Left.Add(a); continue; }
                    if (b.Bounds.Max.X <= a.Bounds.Min.X + rounding) { b.Neighbors_Right.Add(a); a.Neighbors__Left.Add(b); continue; }
                    if (a.Bounds.Max.Y <= b.Bounds.Min.Y + rounding) { a.Neighbors_Above.Add(b); b.Neighbors_Below.Add(a); continue; }
                    if (b.Bounds.Max.Y <= a.Bounds.Min.Y + rounding) { b.Neighbors_Above.Add(a); a.Neighbors_Below.Add(b); continue; }
                }
            }
        }

        private static void SolveIndexes(List<LineBoxNeighbors> boxes, int maxIterations = 100)
        {
            for (var i = 0; i < maxIterations; i++)
            {
                foreach (var b in boxes)
                {
                    // Make beyond previous neighbor
                    foreach (var n in b.Neighbors_Below)
                    {
                        if (b.Row_Max <= n.Row_Max) { b.Row_Max = n.Row_Max + 1; }
                    }

                    foreach (var n in b.Neighbors__Left)
                    {
                        if (b.Column_Max <= n.Column_Max) { b.Column_Max = n.Column_Max + 1; }
                    }

                    // Push Neighbors
                    if (b.Neighbors_Above.Count == 1)
                    {
                        var n = b.Neighbors_Above[0];
                        if (n.Column_Max < b.Column_Max) { n.Column_Max = b.Column_Max; }
                    }
                    if (b.Neighbors_Below.Count == 1)
                    {
                        var n = b.Neighbors_Below[0];
                        if (n.Column_Max < b.Column_Max) { n.Column_Max = b.Column_Max; }
                    }

                    if (b.Neighbors__Left.Count == 1)
                    {
                        var n = b.Neighbors__Left[0];
                        if (n.Row_Max < b.Row_Max) { n.Row_Max = b.Row_Max; }
                    }
                    if (b.Neighbors_Right.Count == 1)
                    {
                        var n = b.Neighbors_Right[0];
                        if (n.Row_Max < b.Row_Max) { n.Row_Max = b.Row_Max; }
                    }
                }
            }

            // Solve Max Col, Row
            for (var i = 0; i < maxIterations; i++)
            {
                foreach (var b in boxes)
                {
                    b.Row_Min = b.Neighbors_Below.Count > 0 ? b.Neighbors_Below.Max(x => x.Row_Max) + 1 : b.Row_Max;
                    b.Column_Min = b.Neighbors__Left.Count > 0 ? b.Neighbors__Left.DefaultIfEmpty(b).Max(x => x.Column_Max) + 1 : b.Column_Max;

                    // Push Neighbors
                    if (b.Neighbors_Above.Count == 1)
                    {
                        var n = b.Neighbors_Above[0];
                        if (n.Column_Min > b.Column_Min) { n.Column_Min = b.Column_Min; }
                    }
                    if (b.Neighbors_Below.Count == 1)
                    {
                        var n = b.Neighbors_Below[0];
                        if (n.Column_Min > b.Column_Min) { n.Column_Min = b.Column_Min; }
                    }

                    if (b.Neighbors__Left.Count == 1)
                    {
                        var n = b.Neighbors__Left[0];
                        if (n.Row_Min > b.Row_Min) { n.Row_Min = b.Row_Min; }
                    }
                    if (b.Neighbors_Right.Count == 1)
                    {
                        var n = b.Neighbors_Right[0];
                        if (n.Row_Min > b.Row_Min) { n.Row_Min = b.Row_Min; }
                    }
                }
            }

        }

        private static void AssignTables(List<LineBoxNeighbors> boxes)
        {
            for (var i = 0; i < boxes.Count; i++)
            {
                boxes[i].TableId = i;
            }

            foreach (var b in boxes)
            {
                b.Neighbors_Above.ForEach(x => x.TableId = Math.Min(b.TableId, x.TableId));
                b.Neighbors_Below.ForEach(x => x.TableId = Math.Min(b.TableId, x.TableId));
                b.Neighbors__Left.ForEach(x => x.TableId = Math.Min(b.TableId, x.TableId));
                b.Neighbors_Right.ForEach(x => x.TableId = Math.Min(b.TableId, x.TableId));
            }
        }
    }
}
