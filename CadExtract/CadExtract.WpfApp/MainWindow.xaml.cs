using CadExtract.Library;
using CadExtract.Library.Geometry;
using CadExtract.Library.Importers;
using CadExtract.Library.Layout;
using DebugCanvasWpf.DotNetFramework;
using System;
using System.Linq;
using System.Windows;

namespace CadExtract.WpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();

        private void OnWorldBoundsChanged(object sender, EventArgs e)
        {
            var worldBounds = compRawView.WorldBounds;
            if (sender == compRawView) { worldBounds = compRawView.WorldBounds; }
            if (sender == compBoxesView) { worldBounds = compBoxesView.WorldBounds; }
            if (sender == compBoxNeighborsView) { worldBounds = compBoxNeighborsView.WorldBounds; }
            if (sender == compTablesInDrawing) { worldBounds = compTablesInDrawing.WorldBounds; }

            if (worldBounds != compRawView.WorldBounds) { compRawView.WorldBounds = worldBounds; compRawView.Render(); }
            if (worldBounds != compBoxesView.WorldBounds) { compBoxesView.WorldBounds = worldBounds; compBoxesView.Render(); }
            if (worldBounds != compBoxNeighborsView.WorldBounds) { compBoxNeighborsView.WorldBounds = worldBounds; compBoxNeighborsView.Render(); }
            if (worldBounds != compTablesInDrawing.WorldBounds) { compTablesInDrawing.WorldBounds = worldBounds; compTablesInDrawing.Render(); }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var d = new Microsoft.Win32.OpenFileDialog() { Title = "Open File to Extract", Filter = "Dxf or Pdf files (*.dxf, *.pdf)|*.dxf;*.pdf|All files (*.*)|*.*", Multiselect = false };
            if (d.ShowDialog() == false) { return; }

            txtFilePath.Text = d.FileName;
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var filePath = txtFilePath.Text;
            var cadData = NetDxfImporter.Import(filePath);
            var tableData = new TableData() { LineBoxes = LineBoxFinder.FindLineBoxesWithTexts(cadData.Lines, cadData.Texts, cadData.Circles) };
            tableData.LineBoxNeighbors = LineBoxNeighborsPushAlgorithm.Solve(tableData.LineBoxes);
            tableData.LineTables_Uncondensed = LineTablesLayout.FindLineTables(tableData.LineBoxNeighbors, shouldCondense: false);
            tableData.LineTables = LineTablesLayout.FindLineTables(tableData.LineBoxNeighbors, shouldCondense: true);

            var dataRowInfo = tableData.LineTables.Select(x => DataRowFinder.FindDataRows(x)).ToList();

            Draw_RawView(compRawView, cadData);
            Draw_BoxesView(compBoxesView, cadData, tableData);
            Draw_BoxNeighborsView(compBoxNeighborsView, cadData, tableData);
            Draw_TablesInDrawing(compTablesInDrawing, cadData, tableData);

            compTablesUncondensed.Items.Clear();
            tableData.LineTables_Uncondensed.ForEach(x => compTablesUncondensed.Items.Add(new System.Windows.Controls.TabItem() { Header = $"Table {x.TableId}", Content = new TableView() { Table = x } }));

            compTables.Items.Clear();
            tableData.LineTables.ForEach(x => compTables.Items.Add(new System.Windows.Controls.TabItem() { Header = $"Table {x.TableId}", Content = new TableView() { Table = x } }));
        }

        private void Draw_RawView(DebugCanvasComponent view, CadData cadData)
        {
            var d = view.DrawingData;
            d.ClearDrawings();

            for (var i = -100; i <= 100; i++)
            {
                d.DrawLine(new System.Numerics.Vector2(i, -100), new System.Numerics.Vector2(i, 100), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
                d.DrawLine(new System.Numerics.Vector2(-100, i), new System.Numerics.Vector2(100, i), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
            }

            foreach (var l in cadData.Lines)
            {
                d.DrawLine(l.Start, l.End, color: System.Drawing.Color.White);
            }

            foreach (var l in cadData.Circles)
            {
                d.DrawBox(l.Circle.Center, size: l.Circle.Radius, color: System.Drawing.Color.Lime, shouldFill: false);
            }

            foreach (var t in cadData.Texts)
            {
                d.DrawBox(t.Bounds.Center, size: t.Bounds.Size, color: System.Drawing.Color.FromArgb(50, System.Drawing.Color.Wheat));
                d.DrawBox(t.Bounds.Center, size: t.Bounds.Size, color: System.Drawing.Color.FromArgb(100, System.Drawing.Color.Wheat), shouldFill: false);
                d.DrawText(t.Text, t.Bounds.Center, size: t.Bounds.Size * new System.Numerics.Vector2(1.2f, 2), fontHeight: t.FontHeight, color: System.Drawing.Color.Wheat);
            }

            view.Render();
        }

        private void Draw_BoxesView(DebugCanvasComponent view, CadData cadData, TableData tableData)
        {
            var d = view.DrawingData;
            d.ClearDrawings();

            for (var i = -100; i <= 100; i++)
            {
                d.DrawLine(new System.Numerics.Vector2(i, -100), new System.Numerics.Vector2(i, 100), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
                d.DrawLine(new System.Numerics.Vector2(-100, i), new System.Numerics.Vector2(100, i), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
            }

            foreach (var l in cadData.Lines)
            {
                d.DrawLine(l.Start, l.End, color: System.Drawing.Color.White);
            }

            //foreach (var l in cadData.Circles)
            //{
            //    d.DrawBox(l.Circle.Center, size: l.Circle.Radius, color: System.Drawing.Color.Lime, shouldFill: false);
            //}

            foreach (var b in tableData.LineBoxes)
            {
                d.DrawBox(b.Bounds.Center, size: b.Bounds.Size, color: System.Drawing.Color.FromArgb(25, System.Drawing.Color.Lime), shouldFill: true);
                d.DrawBox(b.Bounds.Center, size: b.Bounds.Size, color: System.Drawing.Color.Lime, shouldFill: false);
                d.DrawX(b.Bounds.Center, size: b.Bounds.Size, color: System.Drawing.Color.Lime);

                foreach (var t in b.Texts)
                {
                    d.DrawBox(t.Bounds.Center, size: t.Bounds.Size, color: System.Drawing.Color.FromArgb(50, System.Drawing.Color.Wheat));
                    d.DrawBox(t.Bounds.Center, size: t.Bounds.Size, color: System.Drawing.Color.FromArgb(100, System.Drawing.Color.Wheat), shouldFill: false);
                    d.DrawText(t.Text, t.Bounds.Center, size: t.Bounds.Size * new System.Numerics.Vector2(1.2f, 2), fontHeight: t.FontHeight, color: System.Drawing.Color.Wheat);
                }
            }

            //foreach (var t in cadData.Texts)
            //{
            //    d.DrawBox(t.Bounds.Center, size: t.Bounds.Size, color: System.Drawing.Color.FromArgb(50, System.Drawing.Color.Wheat));
            //    d.DrawBox(t.Bounds.Center, size: t.Bounds.Size, color: System.Drawing.Color.FromArgb(100, System.Drawing.Color.Wheat), shouldFill: false);
            //    d.DrawText(t.Text, t.Bounds.Center, size: t.Bounds.Size * new System.Numerics.Vector2(1.2f, 2), fontHeight: t.FontHeight, color: System.Drawing.Color.Wheat);
            //}

            view.Render();
        }

        private void Draw_BoxNeighborsView(DebugCanvasComponent view, CadData cadData, TableData tableData)
        {
            var d = view.DrawingData;
            d.ClearDrawings();

            for (var i = -100; i <= 100; i++)
            {
                d.DrawLine(new System.Numerics.Vector2(i, -100), new System.Numerics.Vector2(i, 100), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
                d.DrawLine(new System.Numerics.Vector2(-100, i), new System.Numerics.Vector2(100, i), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
            }

            foreach (var l in cadData.Lines)
            {
                d.DrawLine(l.Start, l.End, color: System.Drawing.Color.White);
            }

            //foreach (var l in cadData.Circles)
            //{
            //    d.DrawBox(l.Circle.Center, size: l.Circle.Radius, color: System.Drawing.Color.Lime, shouldFill: false);
            //}

            foreach (var b in tableData.LineBoxNeighbors)
            {
                var color = ColorExtensions.RandomColor(b.TableId.GetHashCode());
                d.DrawBox(b.Bounds.Center, size: b.Bounds.Size, color: System.Drawing.Color.FromArgb(100, color), shouldFill: true);
                d.DrawBox(b.Bounds.Center, size: b.Bounds.Size, color: color, shouldFill: false);
                // d.DrawX(b.Bounds.Center, size: b.Bounds.Size, color: System.Drawing.Color.Lime);

                foreach (var n in b.Neighbors_Below)
                {
                    d.DrawLine(b.Bounds.Center, n.Bounds.Center + new System.Numerics.Vector2(0.01f, 0.01f), System.Drawing.Color.Cyan);
                }

                foreach (var n in b.Neighbors_Above)
                {
                    d.DrawLine(b.Bounds.Center, n.Bounds.Center + new System.Numerics.Vector2(0.01f, 0.01f), System.Drawing.Color.Blue);
                }

                foreach (var n in b.Neighbors__Left)
                {
                    d.DrawLine(b.Bounds.Center, n.Bounds.Center + new System.Numerics.Vector2(0.01f, 0.01f), System.Drawing.Color.Magenta);
                }

                foreach (var n in b.Neighbors_Right)
                {
                    d.DrawLine(b.Bounds.Center, n.Bounds.Center + new System.Numerics.Vector2(0.01f, 0.01f), System.Drawing.Color.Red);
                }

                d.DrawText(b.ColumnRowText, b.Bounds.Center, System.Drawing.Color.Magenta, new System.Numerics.Vector2(1, 0.05f), fontHeight: 0.025f, shadow: System.Drawing.Color.Black);
            }

            foreach (var t in cadData.Texts)
            {
                d.DrawBox(t.Bounds.Center, size: t.Bounds.Size, color: System.Drawing.Color.FromArgb(50, System.Drawing.Color.Wheat));
                d.DrawBox(t.Bounds.Center, size: t.Bounds.Size, color: System.Drawing.Color.FromArgb(100, System.Drawing.Color.Wheat), shouldFill: false);
                d.DrawText(t.Text, t.Bounds.Center, size: t.Bounds.Size * new System.Numerics.Vector2(1.2f, 2), fontHeight: t.FontHeight, color: System.Drawing.Color.Wheat);
            }

            view.Render();
        }


        private void Draw_TablesInDrawing(DebugCanvasComponent view, CadData cadData, TableData tableData)
        {
            var d = view.DrawingData;
            d.ClearDrawings();

            for (var i = -100; i <= 100; i++)
            {
                d.DrawLine(new System.Numerics.Vector2(i, -100), new System.Numerics.Vector2(i, 100), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
                d.DrawLine(new System.Numerics.Vector2(-100, i), new System.Numerics.Vector2(100, i), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
            }

            //foreach (var l in cadData.Lines)
            //{
            //    d.DrawLine(l.Start, l.End, color: System.Drawing.Color.White);
            //}

            ////foreach (var l in cadData.Circles)
            ////{
            ////    d.DrawBox(l.Circle.Center, size: l.Circle.Radius, color: System.Drawing.Color.Lime, shouldFill: false);
            ////}

            //foreach (var b in tableData.LineBoxNeighbors)
            //{
            //    var color = ColorExtensions.RandomColor(b.TableId.GetHashCode());
            //    d.DrawBox(b.Bounds.Center, size: b.Bounds.Size, color: System.Drawing.Color.FromArgb(100, color), shouldFill: true);
            //    d.DrawBox(b.Bounds.Center, size: b.Bounds.Size, color: color, shouldFill: false);
            //    // d.DrawX(b.Bounds.Center, size: b.Bounds.Size, color: System.Drawing.Color.Lime);

            //    foreach (var n in b.Neighbors_Below)
            //    {
            //        d.DrawLine(b.Bounds.Center, n.Bounds.Center + new System.Numerics.Vector2(0.01f, 0.01f), System.Drawing.Color.Cyan);
            //    }

            //    foreach (var n in b.Neighbors_Above)
            //    {
            //        d.DrawLine(b.Bounds.Center, n.Bounds.Center + new System.Numerics.Vector2(0.01f, 0.01f), System.Drawing.Color.Blue);
            //    }

            //    foreach (var n in b.Neighbors__Left)
            //    {
            //        d.DrawLine(b.Bounds.Center, n.Bounds.Center + new System.Numerics.Vector2(0.01f, 0.01f), System.Drawing.Color.Magenta);
            //    }

            //    foreach (var n in b.Neighbors_Right)
            //    {
            //        d.DrawLine(b.Bounds.Center, n.Bounds.Center + new System.Numerics.Vector2(0.01f, 0.01f), System.Drawing.Color.Red);
            //    }

            //    d.DrawText(b.ColumnRowText, b.Bounds.Center, System.Drawing.Color.Magenta, new System.Numerics.Vector2(1, 0.05f), fontHeight: 0.025f, shadow: System.Drawing.Color.Black);
            //}

            //foreach (var t in cadData.Texts)
            //{
            //    d.DrawBox(t.Bounds.Center, size: t.Bounds.Size, color: System.Drawing.Color.FromArgb(50, System.Drawing.Color.Wheat));
            //    d.DrawBox(t.Bounds.Center, size: t.Bounds.Size, color: System.Drawing.Color.FromArgb(100, System.Drawing.Color.Wheat), shouldFill: false);
            //    d.DrawText(t.Text, t.Bounds.Center, size: t.Bounds.Size * new System.Numerics.Vector2(1.2f, 2), fontHeight: t.FontHeight, color: System.Drawing.Color.Wheat);
            //}

            foreach (var t in tableData.LineTables)
            {
                var tBounds = t.LineBoxes.Select(x => x.Box.Bounds).UnionBounds();
                var color = ColorExtensions.RandomColor(t.TableId.GetHashCode());
                d.DrawBox(tBounds.Center, size: tBounds.Size, color: System.Drawing.Color.FromArgb(50, color), shouldFill: true);
                d.DrawBox(tBounds.Center, size: tBounds.Size, color: color, shouldFill: false);
                d.DrawBox(tBounds.Center, size: tBounds.Size + new System.Numerics.Vector2(0.1f, 0.1f), color: color, shouldFill: false);

                foreach (var b in t.LineBoxes)
                {
                    var cellColor = b.IsDataCell ? System.Drawing.Color.White : System.Drawing.Color.LightBlue;

                    d.DrawBox(b.Box.Bounds.Center, size: b.Box.Bounds.Size, color: System.Drawing.Color.FromArgb(50, cellColor));
                    d.DrawBox(b.Box.Bounds.Center, size: b.Box.Bounds.Size, color: System.Drawing.Color.FromArgb(100, cellColor), shouldFill: false);
                    d.DrawText(b.CellText, b.Box.Bounds.Center, size: b.Box.Bounds.Size, fontHeight: b.Box.Texts.Min(x => x.FontHeight) * 0.8f, color: cellColor);
                }
            }

            view.Render();
        }
    }
}
