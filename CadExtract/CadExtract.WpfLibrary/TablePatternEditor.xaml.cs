using CadExtract.Library.TablePatterns;
using System;
using System.Linq;
using System.Windows.Controls;

namespace CadExtract.WpfLibrary
{
    /// <summary>
    /// Interaction logic for TablePatternEditor.xaml
    /// </summary>
    public partial class TablePatternEditor : UserControl
    {


        public TablePatternEditor() => InitializeComponent();

        public event EventHandler TablePatternChanged;

        private TablePattern[] _tablePatterns;
        public TablePattern[] TablePatterns { get => _tablePatterns; set { _tablePatterns = value; PopulateTablePatterns(); } }

        private void PopulateTablePatterns()
        {
            TabItem CreateTablePatternTab(TablePattern x)
            {
                var textBox = new TextBox() { Text = x.PatternDocument, TextWrapping = System.Windows.TextWrapping.Wrap, AcceptsReturn = true, AcceptsTab = true };
                textBox.TextChanged += (s, e) =>
                {
                    x.UpdatePatternDocument(textBox.Text);
                    btnSave.Visibility = System.Windows.Visibility.Visible;
                };

                return new TabItem() { Header = x.Name, Content = textBox };
            }

            var tabs = TablePatterns.Select(CreateTablePatternTab).ToList();

            var addItemButton = new Button() { Content = "+" };
            addItemButton.Click += (s, e) =>
            {
                var newTablePattern = TablePatternParser.ParseDataPattern("New Table Pattern", "# Column1");
                _tablePatterns = _tablePatterns.Concat(new[] { newTablePattern }).ToArray();
                tabs.Insert(tabs.Count - 1, CreateTablePatternTab(newTablePattern));
                tabItems.ItemsSource = null;
                tabItems.ItemsSource = tabs;
                tabItems.SelectedIndex = tabs.Count - 2;
            };
            var addItemTab = new TabItem() { Header = addItemButton };
            tabs.Add(addItemTab);

            tabItems.ItemsSource = tabs;
        }

        private void BtnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TablePatternChanged(this, new EventArgs());
            btnSave.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
