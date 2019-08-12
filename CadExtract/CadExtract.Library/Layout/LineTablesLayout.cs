using System.Collections.Generic;
using System.Linq;

namespace CadExtract.Library.Layout
{
    public static class LineTablesLayout
    {
        public static List<LineTable> FindLineTables(List<LineBoxNeighbors> boxes, bool shouldCondense)
        {
            var tables = boxes.GroupBy(x => x.TableId).Select(g => new LineTable()
            {
                LineBoxes = g.Where(x => !x.CellText.IsNullOrEmpty()).Select(x => new LineBoxCell(x)).ToList(),
                TableId = g.Key
            }).Where(x => x.LineBoxes.Count > 0).ToList();

            if (shouldCondense)
            {
                tables.ForEach(x => CondenseTable(x));
            }
            return tables;
        }

        private static void CondenseTable(LineTable table)
        {
            var cMax = table.LineBoxes.Max(x => x.Column_Max);
            var rMax = table.LineBoxes.Max(x => x.Row_Max);

            //var keep_col_single = table.LineBoxes.Where(x => x.Column_Span == 1).Select(x => x.Column_Min);
            //var keep_row_single = table.LineBoxes.Where(x => x.Row_Span == 1).Select(x => x.Row_Min);

            //var keep_col_mult = table.LineBoxes.Where(x => x.Column_Span > 1).Where(x => !keep_col_single.Any(c => c >= x.Column_Min || c <= x.Column_Max)).Select(x => x.Column_Min);
            //var keep_row_mult = table.LineBoxes.Where(x => x.Row_Span > 1).Where(x => !keep_row_single.Any(c => c >= x.Row_Min || c <= x.Row_Max)).Select(x => x.Row_Min);

            //var keep_col = keep_col_mult.Concat(keep_col_single).Distinct().ToList();
            //var keep_row = keep_row_mult.Concat(keep_row_single).Distinct().ToList();

            var keep_col = table.LineBoxes.SelectMany(x => new[] { x.Column_Min, x.Column_Max }).Distinct();
            var keep_row = table.LineBoxes.SelectMany(x => new[] { x.Row_Min, x.Row_Max }).Distinct();

            var remove_col = Enumerable.Range(0, cMax + 1).Except(keep_col).ToList();
            var remove_row = Enumerable.Range(0, rMax + 1).Except(keep_row).ToList();

            table.LineBoxes.ForEach(x =>
            {
                x.Column_Min -= remove_col.Count(y => y < x.Column_Min);
                x.Column_Max -= remove_col.Count(y => y < x.Column_Max);
                x.Row_Min -= remove_row.Count(y => y < x.Row_Min);
                x.Row_Max -= remove_row.Count(y => y < x.Row_Max);
            });
        }
    }
}
