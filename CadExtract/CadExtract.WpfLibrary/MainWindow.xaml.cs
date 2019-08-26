using CadExtract.Library.TablePatterns;
using System.Windows;

namespace CadExtract.WpfLibrary
{
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();
        public string DxfFilePath { get => view.DxfFilePath; set => view.DxfFilePath = value; }
        public TablePattern[] TablePatterns { get => view.TablePatterns; set => view.TablePatterns = value; }
        public void Load() => view.Load();
    }
}
