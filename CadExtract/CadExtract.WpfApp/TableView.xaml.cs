using CadExtract.Library.Layout;
using System.Linq;
using System.Windows.Controls;

namespace CadExtract.WpfApp
{
    /// <summary>
    /// Interaction logic for TableView.xaml
    /// </summary>
    public partial class TableView : UserControl
    {

        public TableView() => InitializeComponent();

        private LineTable _table;
        public LineTable Table { get => _table; set { _table = value; Populate(); } }

        private void Populate()
        {
            using (var d = Dispatcher.DisableProcessing())
            {
                root.ColumnDefinitions.Clear();
                root.RowDefinitions.Clear();
                root.Children.Clear();

                var cMax = _table.LineBoxes.Max(x => x.Column_Max);
                var rMax = _table.LineBoxes.Max(x => x.Row_Max);

                for (var i = 0; i <= cMax; i++)
                {
                    root.ColumnDefinitions.Add(new ColumnDefinition() { MaxWidth = 100 });
                }

                for (var i = 0; i <= rMax; i++)
                {
                    root.RowDefinitions.Add(new RowDefinition());
                }

                foreach (var b in _table.LineBoxes)
                {
                    var t = new TextBox() { Text = b.CellText, TextWrapping = System.Windows.TextWrapping.Wrap, MaxWidth = 100 * b.Column_Span };
                    t.SetValue(Grid.RowProperty, rMax - b.Row_Max);
                    t.SetValue(Grid.ColumnProperty, b.Column_Min);
                    t.SetValue(Grid.RowSpanProperty, b.Row_Span);
                    t.SetValue(Grid.ColumnSpanProperty, b.Column_Span);
                    root.Children.Add(t);
                }
            }
        }
    }
}
