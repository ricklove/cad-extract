using CadExtract.Library.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace CadExtract.Library.TablePatterns
{
    public class TableData
    {
        public string TableName { get; set; }
        public List<TableDataColumn> Columns { get; set; }
        public List<TableDataRow> Rows { get; set; }
        public Bounds SourceBounds { get; set; }
        public Bounds SourceBounds_Cropped { get; set; }

        public override string ToString() => $"{TableName}";
    }

    public class TableDataColumn
    {
        public string Name { get; set; }
        public Bounds SourceBounds { get; set; }
        public string SourceHeaderText { get; set; }

        public override string ToString() => $"{Name}";
    }

    public class TableDataRow
    {
        public List<TableDataValue> Values { get; set; }

        public override string ToString() => Values.Select(x => $"{x.Column.Name}={x.Value}").ConcatString(" ");
    }

    public class TableDataValue
    {
        public TableDataColumn Column { get; set; }
        public string Value { get; set; }
        public Bounds SourceBounds { get; set; }
        public float FontHeight { get; set; }

        public override string ToString() => $"{Column.Name}={Value}";
    }

}
