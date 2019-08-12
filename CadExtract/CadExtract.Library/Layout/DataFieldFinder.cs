using System.Collections.Generic;
using System.Linq;

namespace CadExtract.Library.Layout
{
    public static class DataRowFinder
    {
        public class DataColInfo
        {
            public int Col { get; set; }
            public List<LineBoxCell> Cells { get; set; }

            public bool HasLinearProgression { get; set; }
        }

        public class DataRowInfo
        {
            public int Row { get; set; }
            public List<LineBoxCell> Cells { get; set; }

            public bool IsDataRow { get; set; }
        }

        public static List<DataRowInfo> FindDataRows(LineTable table)
        {
            var rMax = table.LineBoxes.Max(x => x.Row_Max);
            var cMax = table.LineBoxes.Max(x => x.Col_Max);

            var rows = Enumerable.Range(0, rMax + 1).Select(r => new DataRowInfo { Row = r, Cells = table.LineBoxes.Where(x => r >= x.Row_Min && r <= x.Row_Max).OrderBy(x => x.Col_Min).ToList() }).ToList();
            var cols = Enumerable.Range(0, cMax + 1).Select(c => new DataColInfo { Col = c, Cells = table.LineBoxes.Where(x => c >= x.Col_Min && c <= x.Col_Max).OrderBy(x => x.Row_Min).ToList() }).ToList();


            // var rowMatches = rows.Select((row, i) => new { row, matchingRows = rows.Skip(i).Where(r2 => IsSimilarRow(row, r2)).ToList() }).ToList();


            var rowClusters = rows.Select(x => new List<DataRowInfo>() { x }).ToList();

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

            var topCluster = rowClusters.OrderBy(x => x.Count).FirstOrDefault();

            if (topCluster.Count > 1)
            {
                topCluster.ForEach(x => x.IsDataRow = true);
                topCluster.ForEach(x => x.Cells.ForEach(y => y.IsDataCell = true));
            }

            return rows;
        }

        private static bool IsSimilarRow(DataRowInfo aRow, DataRowInfo bRow)
        {
            var cMax = aRow.Cells.Concat(bRow.Cells).Max(x => x.Col_Max);

            var matchCount = 0;
            var attemptCount = 0;

            for (var c = 0; c < cMax; c++)
            {
                var a = aRow.Cells.Where(x => c >= x.Col_Min && c <= x.Col_Max).FirstOrDefault();
                var b = bRow.Cells.Where(x => c >= x.Col_Min && c <= x.Col_Max).FirstOrDefault();

                if (a == null || b == null) { continue; }

                attemptCount++;

                if (IsSimilarValue(a.CellText, b.CellText))
                {
                    matchCount++;
                }
            }

            return matchCount * 1.0f / attemptCount > 0.75f;
        }

        private static readonly char[] _ignoreSymbols = ",.".ToCharArray();
        private static bool IsSimilarValue(string a, string b)
        {
            if (int.TryParse(a, out _) && int.TryParse(b, out _)) { return true; }

            if (a.Any(x => char.IsDigit(x))
                != b.Any(x => char.IsDigit(x))) { return false; }

            if (a.Except(_ignoreSymbols).Any(x => char.IsSymbol(x))
                != b.Except(_ignoreSymbols).Any(x => char.IsSymbol(x))) { return false; }

            if (a.Any(x => char.IsUpper(x))
                != b.Any(x => char.IsUpper(x))) { return false; }

            if (a.Any(x => char.IsLower(x))
                != b.Any(x => char.IsLower(x))) { return false; }

            return true;
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
