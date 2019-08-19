using CadExtract.Library;
using CadExtract.Library.TablePatterns;
using System.Windows.Controls;
using System.Windows.Media;

namespace CadExtract.WpfApp
{
    /// <summary>
    /// Interaction logic for TableDataView.xaml
    /// </summary>
    public partial class TableDataView : UserControl
    {
        public TableDataView() => InitializeComponent();

        private TableData _table;
        public TableData Table { get => _table; set { _table = value; Populate(); } }

        private void Populate()
        {
            using (var d = Dispatcher.DisableProcessing())
            {
                root.ColumnDefinitions.Clear();
                root.RowDefinitions.Clear();
                root.Children.Clear();

                var cMax = _table.Columns.Count - 1;
                var rMax = _table.Rows.Count - 1;

                for (var i = 0; i <= cMax + 1; i++)
                {
                    root.ColumnDefinitions.Add(new ColumnDefinition() { MaxWidth = 100 });

                    if (i > 0)
                    {
                        var t = new TextBox() { Text = _table.Columns[i - 1].Name, Background = Brushes.LightGray };
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

                for (var r = 0; r < _table.Rows.Count; r++)
                {
                    var row = _table.Rows[r];
                    for (var i = 0; i < row.Values.Count; i++)
                    {
                        var v = row.Values[i];
                        var c = _table.Columns.IndexOf(v.Column);
                        var text = $"{(v.MergeId.IsNullOrEmpty() ? "" : $"[{v.MergeId}] ")}{v.Value}";
                        var t = new TextBox() { Text = text, TextWrapping = System.Windows.TextWrapping.Wrap, MaxWidth = 100 };
                        t.SetValue(Grid.RowProperty, rMax - r + 1);
                        t.SetValue(Grid.ColumnProperty, c + 1);
                        //t.SetValue(Grid.RowSpanProperty, b.Row_Span);
                        //t.SetValue(Grid.ColumnSpanProperty, b.Col_Span);
                        root.Children.Add(t);
                    }
                }
            }
        }
    }
}
