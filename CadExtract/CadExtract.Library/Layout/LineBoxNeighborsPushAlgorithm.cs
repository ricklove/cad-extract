using CadExtract.Library.Geometry;
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
                // Push next neighbor ahead
                foreach (var b in boxes)
                {
                    foreach (var n in b.Neighbors_Above)
                    {
                        if (n.Row_Max <= b.Row_Max) { n.Row_Max = b.Row_Max + 1; }
                    }

                    foreach (var n in b.Neighbors_Right)
                    {
                        if (n.Column_Max <= b.Column_Max) { n.Column_Max = b.Column_Max + 1; }
                    }
                }

                // Push Sideways Neighbors to be at least equal
                foreach (var b in boxes)
                {
                    var n = b;

                    n = b.Neighbors_Above.OrderByDescending(x => x.Column_Max).FirstOrDefault();
                    if (n != null && n.Column_Max < b.Column_Max) { n.Column_Max = b.Column_Max; }

                    n = b.Neighbors_Below.OrderByDescending(x => x.Column_Max).FirstOrDefault();
                    if (n != null && n.Column_Max < b.Column_Max) { n.Column_Max = b.Column_Max; }

                    n = b.Neighbors__Left.OrderByDescending(x => x.Row_Max).FirstOrDefault();
                    if (n != null && n.Row_Max < b.Row_Max) { n.Row_Max = b.Row_Max; }

                    n = b.Neighbors_Right.OrderByDescending(x => x.Row_Max).FirstOrDefault();
                    if (n != null && n.Row_Max < b.Row_Max) { n.Row_Max = b.Row_Max; }
                }
            }

            // Set Max Col, Row
            foreach (var b in boxes)
            {
                b.Row_Min = new int[] { b.Row_Max }
                    .Concat(b.Neighbors_Below.Select(n => n.Row_Max + 1))
                    .Concat(b.Neighbors_Right.Select(n => n.Row_Max))
                    .Concat(b.Neighbors__Left.Select(n => n.Row_Max))
                    .Min();
                b.Column_Min = new int[] { b.Column_Max }
                    .Concat(b.Neighbors__Left.Select(n => n.Column_Max + 1))
                    .Concat(b.Neighbors_Above.Select(n => n.Column_Max))
                    .Concat(b.Neighbors_Below.Select(n => n.Column_Max))
                    .Min();
            }

            // Assert that min and max are in correct order
            foreach (var b in boxes)
            {
                if (b.Column_Min > b.Column_Max
                    || b.Row_Min > b.Row_Max)
                {
                    throw new Exception("The algorithm failed");
                }
            }

        }

        private static void AssignTables(List<LineBoxNeighbors> boxes, int maxIterations = 100)
        {
            for (var i = 0; i < boxes.Count; i++)
            {
                boxes[i].TableId = i;
            }

            for (var i = 0; i < maxIterations; i++)
            {
                foreach (var b in boxes)
                {
                    AssignTableIfAlignedBounds(b, b.Neighbors_Above);
                    AssignTableIfAlignedBounds(b, b.Neighbors_Below);
                    AssignTableIfAlignedBounds(b, b.Neighbors__Left);
                    AssignTableIfAlignedBounds(b, b.Neighbors_Right);
                }
            }

            bool IsAlignedTableBounds(Bounds a, Bounds b)
            {
                // Aligned (X or Y)
                var minDiff = a.Min - b.Min;
                var maxDiff = a.Max - b.Max;

                return Math.Abs(minDiff.X - maxDiff.X) < 0.1f
                    || Math.Abs(minDiff.Y - maxDiff.Y) < 0.1f;
            }

            void AssignTableIfAlignedBounds(LineBoxNeighbors box, List<LineBoxNeighbors> neighbors)
            {
                if (neighbors.GroupBy(x => x.TableId).Count() != 1) { return; }
                if (!IsAlignedTableBounds(box.Bounds, neighbors.Select(x => x.Bounds).UnionBounds())) { return; }

                var id = Math.Min(neighbors.First().TableId, box.TableId);
                box.TableId = id;
                foreach (var x in neighbors)
                {
                    x.TableId = id;
                }
            }
        }


    }
}
