using DebugCanvasWpf.Library;
using System.Windows.Controls;

namespace CadExtract.WpfApp
{
    public partial class RawView : UserControl
    {
        public RawView()
        {
            InitializeComponent();
        }

        public DrawingData DrawingData => compDebugCanvas.DrawingData;

        public void Render() => compDebugCanvas.Render();
    }
}
