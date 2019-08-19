using CadExtract.Library.Geometry;
using CadExtract.Library.Layout;
using System.Collections.Generic;
using System.Linq;

namespace CadExtract.Library.TablePatterns
{
    public static class TablePatternDataExtraction
    {
        public static TableData ExtractTable(LineTable table, TablePattern pattern)
        {
            var tableInfo = TableDataRowFinder.FindDataRowsAndColumns(table);
            var columnPatterns = AssignColumns(pattern, tableInfo);

            if (columnPatterns.Any(x => x.Key.IsRequired && x.Value == null)) { return null; }

            var columns = columnPatterns.Select(x => new TableDataColumn()
            {
                Name = x.Key.Name,
                SourceBounds = x.Value?.Cells.Where(c => c.Col_Span == 1).Select(c => c.Box.Bounds).Where(b => b != new Bounds()).UnionBounds() ?? new Bounds(),
            }).ToList();

            var rows = tableInfo.Rows.Select(row =>
            {
                var rowOut = new TableDataRow()
                {
                    Values = columnPatterns.Select(col =>
                    {
                        var v = row.Values.Where(x => x.Cols.Contains(col.Value)).FirstOrDefault();
                        if (v == null) { return null; }

                        // Ensure value matches pattern
                        if (!col.Key.ValueMatchRegex.IsMatch(v.Cell.CellText)) { return null; }

                        var column = columns.Where(c => c.Name == col.Key.Name).FirstOrDefault();

                        return new TableDataValue()
                        {
                            Column = column,
                            Value = v.Cell.CellText,
                            MergeId = v.Cell.Col_Span > 1 || v.Cell.Row_Span > 1 ? "" + v.Cell.CellId : null as string,
                            SourceBounds = v.Cell.Box.Bounds,
                            FontHeight = v.Cell.Box.Texts.Average(x => x.FontHeight),
                        };
                    }).Where(x => x != null).ToList()
                };

                if (!columnPatterns.Keys.Where(x => x.IsRequired).All(x => rowOut.Values.Any(y => y.Column.Name == x.Name))) { return null; }

                return rowOut;
            }).Where(x => x != null).ToList();

            // columns.ForEach(col => col.SourceBounds = rows.SelectMany(x => x.Values).Where(v => v.Column == col).Select(x => x.SourceBounds).UnionBounds());
            // columns.ForEach(col => col.SourceHeaderText =  rows.SelectMany(x => x.Values).Where(v => v.Column == col).Select(x => x.SourceBounds).UnionBounds());

            return new TableData()
            {
                SourceBounds = table.Bounds,
                // SourceBounds = rows.SelectMany(x => x.Values.Select(v => v.SourceBounds)).UnionBounds(),
                SourceBounds_Cropped = rows.SelectMany(x => x.Values.Select(v => v.SourceBounds)).UnionBounds(),
                TableName = pattern.Name,
                Rows = rows,
                Columns = columns,
            };
        }

        private static Dictionary<TablePatternColumn, TableDataRowFinder.ColInfo> AssignColumns(TablePattern pattern, TableDataRowFinder.TableInfo tableInfo)
        {
            var remainingCols = tableInfo.Cols.ToList();

            var colMatches = pattern.Columns.Select(c =>
            {
                var column = remainingCols.Where(x => c.ColumnMatchRegex.IsMatch(x.AllCellText) && c.ValueMatchRegex.IsMatch(x.AllCellText)).FirstOrDefault();
                remainingCols.Remove(column);
                return new { cPattern = c, column };
            }).ToList();

            return colMatches.ToDictionary(x => x.cPattern, x => x.column);
        }
    }
}
