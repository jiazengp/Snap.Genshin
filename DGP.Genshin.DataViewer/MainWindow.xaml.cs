using System.Windows;

namespace DGP.Genshin.DataViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.DataContext = this;
            this.InitializeComponent();
            this.DirectoryView.ExcelSplitView = this.ExcelDataView;
            
        }

    }
}
