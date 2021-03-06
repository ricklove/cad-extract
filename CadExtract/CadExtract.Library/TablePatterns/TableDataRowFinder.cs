﻿using CadExtract.Library.Layout;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CadExtract.Library.TablePatterns
{
    public static class TableDataRowFinder
    {
        public class TableInfo
        {
            public List<ColInfo> Cols { get; set; }
            public List<RowInfo> Rows { get; set; }
        }

        public class ColInfo
        {
            public int Col { get; set; }
            public List<LineBoxCell> Cells { get; set; }

            public bool HasLinearProgression { get; set; }

            public string AllCellText => Cells.OrderByDescending(c => c.Row_Max).Select(x => x.CellText).ConcatString();
            public override string ToString() => AllCellText;
        }

        public class RowInfo
        {
            public int Row { get; set; }
            public List<LineBoxCell> Cells { get; set; }
            public List<ValueInfo> Values { get; set; }

            public bool IsDataRow { get; set; }

            public string AllCellText => Cells.OrderBy(c => c.Col_Min).Select(x => x.CellText).ConcatString();
            public override string ToString() => AllCellText;
        }

        public class ValueInfo
        {
            public List<ColInfo> Cols { get; set; }
            public LineBoxCell Cell { get; set; }
        }

        public static TableInfo FindDataRowsAndColumns(LineTable table)
        {
            var rMax = table.LineBoxes.Max(x => x.Row_Max);
            var cMax = table.LineBoxes.Max(x => x.Col_Max);

            var cols = Enumerable.Range(0, cMax + 1).Select(c => new ColInfo { Col = c, Cells = table.LineBoxes.Where(x => c >= x.Col_Min && c <= x.Col_Max).OrderBy(x => x.Row_Min).ToList() }).ToList();
            var rows = Enumerable.Range(0, rMax + 1).Select(r => new RowInfo { Row = r, Cells = table.LineBoxes.Where(x => r >= x.Row_Min && r <= x.Row_Max).OrderBy(x => x.Col_Min).ToList() }).ToList();

            // var rowMatches = rows.Select((row, i) => new { row, matchingRows = rows.Skip(i).Where(r2 => IsSimilarRow(row, r2)).ToList() }).ToList();

            // Add row values
            foreach (var row in rows)
            {
                row.Values = row.Cells.Select(x => new ValueInfo
                {
                    Cell = x,
                    Cols = cols.Where(y => y.Cells.Contains(x)).ToList()
                }).ToList();
            }

            // Identify Data Rows
            var rowClusters = rows.Select(x => new List<RowInfo>() { x }).ToList();

            for (var i = 0; i < rowClusters.Count; i++)
            {
                for (var j = i + 1; j < rowClusters.Count; j++)
                {
                    if (rowClusters[i].Any(x => rowClusters[j].Any(y => IsSimilarRow(x, y))))
                    {
                        rowClusters[i].AddRange(rowClusters[j]);
                        rowClusters.RemoveAt(j);
                        j--;
                    }
                }
            }

            var topCluster = rowClusters.OrderByDescending(x => x.Count).FirstOrDefault();

            if (topCluster.Count > 1)
            {
                topCluster.ForEach(x => x.IsDataRow = true);
                topCluster.ForEach(x => x.Cells.ForEach(y => y.IsDataCell = true));
            }

            return new TableInfo() { Cols = cols, Rows = rows };
        }

        private static bool IsSimilarRow(RowInfo aRow, RowInfo bRow)
        {
            var cMax = aRow.Cells.Concat(bRow.Cells).Max(x => x.Col_Max);

            var matchScore = 0.0f;
            var attemptCount = 0;

            for (var c = 0; c < cMax; c++)
            {
                var a = aRow.Cells.Where(x => c >= x.Col_Min && c <= x.Col_Max).FirstOrDefault();
                var b = bRow.Cells.Where(x => c >= x.Col_Min && c <= x.Col_Max).FirstOrDefault();

                if (a == null && b == null) { continue; }

                attemptCount++;
                matchScore += SimilarityScore(a?.CellText, b?.CellText);
            }

            return matchScore * 1.0f / attemptCount > 0.75f;
        }

        private static readonly char[] _ignoreSymbols = ",.".ToCharArray();
        public static float SimilarityScore(string a, string b)
        {
            if (a.IsNullOrEmpty() && b.IsNullOrEmpty()) { return 1f; }
            if (a.IsNullOrEmpty() || b.IsNullOrEmpty()) { return 0; }
            if (a == b) { return 3; }

            if (int.TryParse(a, out var aInt) && int.TryParse(b, out var bInt))
            {
                if (Math.Abs(aInt - bInt) <= 1) { return 5; }
                return 3;
            }

            if (decimal.TryParse(a, out _) && decimal.TryParse(b, out _))
            {
                return 3;
            }

            if (DateTime.TryParse(a, out _) && DateTime.TryParse(b, out _))
            {
                return 5;
            }

            if (a.Any(x => char.IsDigit(x))
                != b.Any(x => char.IsDigit(x))) { return 0; }

            //if (a.Except(_ignoreSymbols).Any(x => char.IsSymbol(x))
            //    != b.Except(_ignoreSymbols).Any(x => char.IsSymbol(x))) { return 0; }

            //if (a.Any(x => char.IsUpper(x))
            //    != b.Any(x => char.IsUpper(x))) { return 0; }

            //if (a.Any(x => char.IsLower(x))
            //    != b.Any(x => char.IsLower(x))) { return 0; }

            var lenDiff = Math.Abs(a.Length - b.Length);
            var lenDiffCutoff = Math.Max(5, Math.Min(a.Length / 2, b.Length / 2));

            return lenDiff <= lenDiffCutoff ? 1 : 0;
        }

        private static bool HasLinearProgression(IEnumerable<int> values)
        {
            var prevVal = -1;
            var progCount = 0;

            foreach (var v in values)
            {
                if (prevVal == v + 1
                    || prevVal == v - 1)
                {
                    progCount++;
                }


                prevVal = v;
            }

            return progCount > 3;
        }
    }
}
