using CadExtract.Library.Layout;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace CadExtract.WpfApp
{
    /// <summary>
    /// Interaction logic for TableView.xaml
    /// </summary>
    public partial class LineTableView : UserControl
    {

        public LineTableView() => InitializeComponent();

        private LineTable _table;
        public LineTable Table { get => _table; set { _table = value; Populate(); } }

        private void Populate()
        {
            using (var d = Dispatcher.DisableProcessing())
            {
                root.ColumnDefinitions.Clear();
                root.RowDefinitions.Clear();
                root.Children.Clear();

                var cMax = _table.LineBoxes.Max(x => x.Col_Max);
                var rMax = _table.LineBoxes.Max(x => x.Row_Max);

                for (var i = 0; i <= cMax + 1; i++)
                {
                    root.ColumnDefinitions.Add(new ColumnDefinition() { MaxWidth = 100 });

                    if (i > 0)
                    {
                        var t = new TextBox() { Text = $"C{i - 1}", Background = Brushes.LightGray };
                        t.SetValue(Grid.ColumnProperty, i);
                        root.Children.Add(t);
                    }
                }

                for (var i = 0; i <= rMax + 1; i++)
                {
                    root.RowDefinitions.Add(new RowDefinition());

                    if (i > 0)
                    {
                        var t = new TextBox() { Text = $"R{i - 1}", Background = Brushes.LightGray };
                        t.SetValue(Grid.RowProperty, i);
                        root.Children.Add(t);
                    }
                }

                foreach (var b in _table.LineBoxes)
                {
                    var t = new TextBox() { Text = b.CellText, TextWrapping = System.Windows.TextWrapping.Wrap, MaxWidth = 100 * b.Col_Span };
                    t.SetValue(Grid.RowProperty, rMax - b.Row_Max + 1);
                    t.SetValue(Grid.ColumnProperty, b.Col_Min + 1);
                    t.SetValue(Grid.RowSpanProperty, b.Row_Span);
                    t.SetValue(Grid.ColumnSpanProperty, b.Col_Span);
                    root.Children.Add(t);
                }
            }
        }
    }
}
