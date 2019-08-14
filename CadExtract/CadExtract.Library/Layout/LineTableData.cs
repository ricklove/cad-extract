using CadExtract.Library.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CadExtract.Library.Layout
{
    public class LineTableData
    {
        public List<LineBox> LineBoxes { get; set; }
        public List<LineBoxNeighbors> LineBoxNeighbors { get; set; }
        public List<LineTable> LineTables_Uncondensed { get; set; }
        public List<LineTable> LineTables { get; set; }
    }

    public class LineBox
    {
        public Bounds Bounds { get; }

        public List<CadText> Texts { get; } = new List<CadText>();

        public string CellText
        {
            get
            {
                if (Texts.Count <= 0) { return ""; }

                var fontHeight = Texts.Min(x => x.FontHeight);
                return Texts.OrderByDescending(x => Math.Round(x.Bounds.Center.Y / fontHeight) * fontHeight).ThenBy(x => x.Bounds.Center.X).Select(x => x.Text).ConcatString(" ");
            }
        }

        public LineBox(Bounds bounds) => Bounds = bounds;

        public override string ToString() => $"{Bounds} {CellText}";
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

        public int Col_Max { get; set; }
        public int Row_Max { get; set; }

        public int Col_Min { get; set; }
        public int Row_Min { get; set; }
        public string ColumnRowText => $"C{Col_Min}{(Col_Max != Col_Min ? $"-{Col_Max}" : "")} R{Row_Min}{(Row_Max != Row_Min ? $"-{Row_Max}" : "")}";

        public int Row_Span => Row_Max - Row_Min + 1;
        public int Col_Span => Col_Max - Col_Min + 1;

        public int TableId { get; set; }
    }

    public class LineBoxCell
    {
        public LineBoxCell(LineBoxNeighbors box)
        {
            _debug_box = box;
            Box = box.Box;
            Row_Min = box.Row_Min;
            Col_Min = box.Col_Min;
            Row_Max = box.Row_Max;
            Col_Max = box.Col_Max;
            TableId = box.TableId;
        }

        private readonly LineBoxNeighbors _debug_box;

        public LineBox Box { get; }
        public string CellText => Box.CellText;

        public int Col_Max { get; set; }
        public int Row_Max { get; set; }

        public int Col_Min { get; set; }
        public int Row_Min { get; set; }

        public int Row_Span => Row_Max - Row_Min + 1;
        public int Col_Span => Col_Max - Col_Min + 1;

        public string ColumnRowText => $"C{Col_Min}{(Col_Max != Col_Min ? $"-{Col_Max}" : "")} R{Row_Min}{(Row_Max != Row_Min ? $"-{Row_Max}" : "")}";

        public int TableId { get; set; }
        public bool IsDataCell { get; set; }

        public override string ToString() => $"{CellText} ({ColumnRowText})";
    }

    public class LineTable
    {
        public int TableId { get; set; }
        public List<LineBoxCell> LineBoxes { get; set; }
        public Bounds Bounds { get; set; }
    }
}
