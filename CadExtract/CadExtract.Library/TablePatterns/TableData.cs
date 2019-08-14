using CadExtract.Library.Geometry;
using System.Collections.Generic;

namespace CadExtract.Library.TablePatterns
{
    public class TableData
    {
        public string TableName { get; set; }
        public List<TableDataColumn> Columns { get; set; }
        public List<TableDataRow> Rows { get; set; }
        public Bounds SourceBounds { get; set; }
        public Bounds SourceBounds_Cropped { get; set; }
    }

    public class TableDataColumn
    {
        public string Name { get; set; }
        public Bounds SourceBounds { get; set; }
        public string SourceHeaderText { get; set; }
    }

    public class TableDataRow
    {
        public List<TableDataValue> Values { get; set; }
    }

    public class TableDataValue
    {
        public TableDataColumn Column { get; set; }
        public string Value { get; set; }
        public Bounds SourceBounds { get; set; }
        public float FontHeight { get; set; }
    }

}
