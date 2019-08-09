using CadExtract.Library.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace CadExtract.Library.Layout
{
    public class TableData
    {
        public List<LineBox> LineBoxes { get; set; }
        public List<LineBoxNeighbors> LineBoxNeighbors { get; set; }
        public List<LineTable> LineTables { get; set; }
    }

    public class LineBox
    {
        public Bounds Bounds { get; }

        public List<CadText> Texts { get; } = new List<CadText>();
        public string CellText => Texts.OrderByDescending(x => x.Bounds.Center.Y).ThenBy(x => x.Bounds.Center.X).Select(x => x.Text).ConcatString(" ");

        public LineBox(Bounds bounds) => Bounds = bounds;
    }

    public class LineBoxNeighbors
    {
        public LineBoxNeighbors(LineBox box) => Box = box;
        public LineBox Box { get; }
        public Bounds Bounds => Box.Bounds;
        public string CellText => Box.CellText;

        public List<LineBoxNeighbors> Neighbors_Overlapping { get; set; } = new List<LineBoxNeighbors>();

        public List<LineBoxNeighbors> Neighbors_Below { get; set; } = new List<LineBoxNeighbors>();
        public List<LineBoxNeighbors> Neighbors_Above { get; set; } = new List<LineBoxNeighbors>();
        public List<LineBoxNeighbors> Neighbors__Left { get; set; } = new List<LineBoxNeighbors>();
        public List<LineBoxNeighbors> Neighbors_Right { get; set; } = new List<LineBoxNeighbors>();

        public int Column_Max { get; set; }
        public int Row_Max { get; set; }

        public int Column_Min { get; set; }
        public int Row_Min { get; set; }
        public string ColumnRowText => $"C{Column_Min}{(Column_Max != Column_Min ? $"-{Column_Max}" : "")} R{Row_Min}{(Row_Max != Row_Min ? $"-{Row_Max}" : "")}";

        public int Row_Span => Row_Max - Row_Min + 1;
        public int Column_Span => Column_Max - Column_Min + 1;

        public int TableId { get; set; }
    }

    public class LineBoxCell
    {
        public LineBoxCell(LineBoxNeighbors box)
        {
            _debug_box = box;
            Box = box.Box;
            Row_Min = box.Row_Min;
            Column_Min = box.Column_Min;
            Row_Max = box.Row_Max;
            Column_Max = box.Column_Max;
            TableId = box.TableId;
        }

        private readonly LineBoxNeighbors _debug_box;

        public LineBox Box { get; }
        public string CellText => Box.CellText;

        public int Column_Max { get; set; }
        public int Row_Max { get; set; }

        public int Column_Min { get; set; }
        public int Row_Min { get; set; }

        public int Row_Span => Row_Max - Row_Min + 1;
        public int Column_Span => Column_Max - Column_Min + 1;

        public int TableId { get; set; }
    }

    public class LineTable
    {
        public int TableId { get; set; }
        public List<LineBoxCell> LineBoxes { get; set; }
    }
}
