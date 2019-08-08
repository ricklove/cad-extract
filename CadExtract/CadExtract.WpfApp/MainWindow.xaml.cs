using CadExtract.Library.Importers;
using System.Windows;

namespace CadExtract.WpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

            var d = compRawView.DrawingData;
            d.ClearDrawings();

            for (int i = -100; i <= 100; i++)
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

            compRawView.Render();
        }
    }
}
