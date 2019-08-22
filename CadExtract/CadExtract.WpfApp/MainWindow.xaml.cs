using CadExtract.Library;
using CadExtract.Library.Geometry;
using CadExtract.Library.Layout;
using CadExtract.Library.Process;
using CadExtract.Library.TablePatterns;
using DebugCanvasWpf.DotNetFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;

namespace CadExtract.WpfApp
{
    public partial class MainWindow : Window
    {
        private ExtractionData _data;

        public MainWindow() => InitializeComponent();

        private void OnWorldBoundsChanged(object sender, EventArgs e)
        {
            var worldBounds = compRawView.WorldBounds;
            if (sender == compRawView) { worldBounds = compRawView.WorldBounds; }
            if (sender == compBoxesView) { worldBounds = compBoxesView.WorldBounds; }
            if (sender == compBoxNeighborsView) { worldBounds = compBoxNeighborsView.WorldBounds; }
            if (sender == compTablesInDrawing) { worldBounds = compTablesInDrawing.WorldBounds; }
            if (sender == compTableDataInDrawing) { worldBounds = compTableDataInDrawing.WorldBounds; }

            if (worldBounds != compRawView.WorldBounds) { compRawView.WorldBounds = worldBounds; compRawView.Render(); }
            if (worldBounds != compBoxesView.WorldBounds) { compBoxesView.WorldBounds = worldBounds; compBoxesView.Render(); }
            if (worldBounds != compBoxNeighborsView.WorldBounds) { compBoxNeighborsView.WorldBounds = worldBounds; compBoxNeighborsView.Render(); }
            if (worldBounds != compTablesInDrawing.WorldBounds) { compTablesInDrawing.WorldBounds = worldBounds; compTablesInDrawing.Render(); }
            if (worldBounds != compTableDataInDrawing.WorldBounds) { compTableDataInDrawing.WorldBounds = worldBounds; compTableDataInDrawing.Render(); }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var d = new Microsoft.Win32.OpenFileDialog() { Title = "Open File to Extract", Filter = "Dxf or Pdf files (*.dxf, *.pdf)|*.dxf;*.pdf|All files (*.*)|*.*", Multiselect = false };
            if (d.ShowDialog() == false) { return; }

            txtFilePath.Text = d.FileName;
            LoadFile(txtFilePath.Text);
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e) => LoadFile(txtFilePath.Text);
        private void LoadFile(string filePath)
        {
            var patterns = new[] { TablePattern_Samples.BomPattern, TablePattern_Samples.WireHarnessPattern };
            var data = ExtractionProcess.ExtractData(filePath, patterns);
            var cadData = data.CadData;
            var lineTableData = data.LineTableData;
            var dataTables = data.DataTables;

            // var dataRowInfo = lineTableData.LineTables.Select(x => TableDataRowFinder.FindDataRowsAndColumns(x)).ToList();

            Draw_RawView(compRawView, cadData);
            Draw_BoxesView(compBoxesView, cadData, lineTableData);
            Draw_BoxNeighborsView(compBoxNeighborsView, cadData, lineTableData);
            Draw_TablesInDrawing(compTablesInDrawing, cadData, lineTableData);
            Draw_TableDataInDrawing(compTableDataInDrawing, data.MissingTexts, dataTables);

            compTablesUncondensed.Items.Clear();
            lineTableData.LineTables_Uncondensed.ForEach(x => compTablesUncondensed.Items.Add(new System.Windows.Controls.TabItem() { Header = $"Table {x.TableId}", Content = new LineTableView() { Table = x } }));

            compTables.Items.Clear();
            lineTableData.LineTables.ForEach(x => compTables.Items.Add(new System.Windows.Controls.TabItem() { Header = $"Table {x.TableId}", Content = new LineTableView() { Table = x } }));

            compTablesData.Items.Clear();
            dataTables.ForEach(x => compTablesData.Items.Add(new System.Windows.Controls.TabItem() { Header = $"Table {x.TableName}", Content = new TableDataView() { Table = x } }));

            _data = data;
        }

        private void Draw_RawView(DebugCanvasComponent view, CadData cadData)
        {
            var d = view.DrawingData;
            d.ClearDrawings();

            //for (var i = -100; i <= 100; i++)
            //{
            //    d.DrawLine(new System.Numerics.Vector2(i, -100), new System.Numerics.Vector2(i, 100), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
            //    d.DrawLine(new System.Numerics.Vector2(-100, i), new System.Numerics.Vector2(100, i), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
            //}

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

            d.ZoomToContents();
            view.Render();
        }

        private void Draw_BoxesView(DebugCanvasComponent view, CadData cadData, LineTableData tableData)
        {
            var d = view.DrawingData;
            d.ClearDrawings();

            //for (var i = -100; i <= 100; i++)
            //{
            //    d.DrawLine(new System.Numerics.Vector2(i, -100), new System.Numerics.Vector2(i, 100), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
            //    d.DrawLine(new System.Numerics.Vector2(-100, i), new System.Numerics.Vector2(100, i), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
            //}

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

            d.ZoomToContents();
            view.Render();
        }

        private void Draw_BoxNeighborsView(DebugCanvasComponent view, CadData cadData, LineTableData tableData)
        {
            var d = view.DrawingData;
            d.ClearDrawings();

            //for (var i = -100; i <= 100; i++)
            //{
            //    d.DrawLine(new System.Numerics.Vector2(i, -100), new System.Numerics.Vector2(i, 100), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
            //    d.DrawLine(new System.Numerics.Vector2(-100, i), new System.Numerics.Vector2(100, i), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
            //}

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

            d.ZoomToContents();
            view.Render();
        }


        private void Draw_TablesInDrawing(DebugCanvasComponent view, CadData cadData, LineTableData tableData, LineTable highlightTable = null)
        {
            var d = view.DrawingData;
            d.ClearDrawings();

            //for (var i = -100; i <= 100; i++)
            //{
            //    d.DrawLine(new System.Numerics.Vector2(i, -100), new System.Numerics.Vector2(i, 100), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
            //    d.DrawLine(new System.Numerics.Vector2(-100, i), new System.Numerics.Vector2(100, i), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
            //}

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
                var alphaMult = highlightTable == null ? 4 : highlightTable == t ? 4 : 1;

                var tBounds = t.Bounds;
                var color = ColorExtensions.RandomColor(t.TableId.GetHashCode());
                d.DrawBox(tBounds.Center, size: tBounds.Size, color: System.Drawing.Color.FromArgb(12 * alphaMult, color), shouldFill: true);
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

            d.ZoomToContents();
            view.Render();
        }

        private void Draw_TableDataInDrawing(DebugCanvasComponent view, List<CadText> missingTexts, List<TableData> tableData, TableData highlightTable = null)
        {
            var d = view.DrawingData;
            d.ClearDrawings();

            //for (var i = -100; i <= 100; i++)
            //{
            //    d.DrawLine(new System.Numerics.Vector2(i, -100), new System.Numerics.Vector2(i, 100), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
            //    d.DrawLine(new System.Numerics.Vector2(-100, i), new System.Numerics.Vector2(100, i), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gray));
            //}

            //if (highlightTable == null)
            //{
            foreach (var t in missingTexts)
            {
                //if (highlightTable != null && highlightTable.SourceBounds.Contains(t.Bounds.Center)) { continue; }
                //var hasMatch = tableData.SelectMany(x => x.Rows.SelectMany(y => y.Values)).Where(v => v.Value.Contains(t.Text) && v.SourceBounds.Intersects(t.Bounds)).Any();
                //if (hasMatch) { continue; }

                d.DrawBox(t.Bounds.Center, size: t.Bounds.Size, color: System.Drawing.Color.FromArgb(50, System.Drawing.Color.Red));
                d.DrawBox(t.Bounds.Center, size: t.Bounds.Size, color: System.Drawing.Color.FromArgb(100, System.Drawing.Color.Red), shouldFill: false);
                d.DrawText(t.Text, t.Bounds.Center, size: t.Bounds.Size * new System.Numerics.Vector2(1.2f, 2), fontHeight: t.FontHeight, color: System.Drawing.Color.Red);
            }
            //}

            foreach (var t in tableData)
            {
                var alphaMult = highlightTable == null ? 4 : highlightTable == t ? 4 : 1;

                var tBounds = t.SourceBounds;
                var color = ColorExtensions.RandomColor(t.TableName.GetHashCode());
                d.DrawBox(tBounds.Center, size: tBounds.Size, color: System.Drawing.Color.FromArgb(12 * alphaMult, color), shouldFill: true);
                d.DrawBox(tBounds.Center, size: tBounds.Size, color: color, shouldFill: false);
                d.DrawBox(tBounds.Center, size: tBounds.Size + new System.Numerics.Vector2(0.1f, 0.1f), color: color, shouldFill: false);

                foreach (var col in t.Columns)
                {
                    var cellColor = System.Drawing.Color.LightBlue;
                    var values = t.Rows.SelectMany(row => row.Values).Where(v => v.Column == col).ToList();
                    if (!values.Any()) { continue; }

                    var dataBounds = values.Select(x => x.SourceBounds).UnionBounds();
                    var bounds = new Bounds(new Vector2(dataBounds.Min.X, dataBounds.Max.Y), new Vector2(dataBounds.Max.X, t.SourceBounds.Max.Y));
                    // var bounds = col.SourceBounds;
                    var fontHeight = values.Average(x => x.FontHeight);

                    d.DrawBox(bounds.Center, size: bounds.Size, color: System.Drawing.Color.FromArgb(50, cellColor));
                    d.DrawBox(bounds.Center, size: bounds.Size, color: System.Drawing.Color.FromArgb(100, cellColor), shouldFill: false);
                    d.DrawText(col.Name.Replace("_", " "), bounds.Center, size: bounds.Size, fontHeight: fontHeight * 0.5f, color: cellColor);
                }

                foreach (var row in t.Rows)
                {
                    foreach (var val in row.Values)
                    {
                        var cellColor = System.Drawing.Color.White;
                        var bounds = val.SourceBounds;

                        d.DrawBox(bounds.Center, size: bounds.Size, color: System.Drawing.Color.FromArgb(50, cellColor));
                        d.DrawBox(bounds.Center, size: bounds.Size, color: System.Drawing.Color.FromArgb(100, cellColor), shouldFill: false);
                        d.DrawText(val.Value, bounds.Center, size: bounds.Size, fontHeight: val.FontHeight * 0.8f, color: cellColor);
                    }
                }
            }

            d.ZoomToContents();
            view.Render();
        }

        private void OnDrawingClick(object sender, DebugCanvasComponent.ClickEventArgs e)
        {
            if (sender != compTablesInDrawing
                && sender != compTableDataInDrawing) { return; }

            var closestTable = _data.LineTableData.LineTables.Where(x => x.Bounds.Contains(e.WorldPosition)).OrderBy(x => (x.Bounds.Center - e.WorldPosition).LengthSquared()).FirstOrDefault();
            Draw_TablesInDrawing(compTablesInDrawing, _data.CadData, _data.LineTableData, closestTable);

            var closestTableData = _data.DataTables.Where(x => x.SourceBounds.Contains(e.WorldPosition)).OrderBy(x => (x.SourceBounds.Center - e.WorldPosition).LengthSquared()).FirstOrDefault();
            Draw_TableDataInDrawing(compTableDataInDrawing, _data.MissingTexts, _data.DataTables, closestTableData);
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e) => OnCopyCommand();
        private void OnCopy(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) => OnCopyCommand();
        private void OnCopyCommand()
        {

            var worldBounds = compTableDataInDrawing.DrawingData.ScreenWorldBounds;
            var worldCenter = new Vector2(worldBounds.X, worldBounds.Y) + new Vector2(worldBounds.Size.Width, worldBounds.Size.Height) * 0.5f;

            if (_data == null) { return; }

            if (compTableDataInDrawing.IsVisible)
            {
                var closestTableData = _data.DataTables.Where(x => x.SourceBounds.Contains(worldCenter)).OrderBy(x => (x.SourceBounds_Cropped.Center - worldCenter).LengthSquared()).FirstOrDefault();

                if (closestTableData == null) { return; }
                Clipboard.SetText(closestTableData.ToClipboard_HtmlTableFormat());
                return;
            }

            // Otherwise use raw
            if (true)
            {
                var closestTable = _data.LineTableData.LineTables.Where(x => x.Bounds.Contains(worldCenter)).OrderBy(x => (x.Bounds.Center - worldCenter).LengthSquared()).FirstOrDefault();

                if (closestTable == null) { return; }
                Clipboard.SetText(closestTable.ToClipboard_HtmlTableFormat());
                return;
            }
        }
    }
}
