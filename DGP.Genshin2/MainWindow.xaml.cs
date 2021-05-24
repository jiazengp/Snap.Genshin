using DGP.Genshin2.Data.GHHW;
using System.Windows;

namespace DGP.Genshin2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await new HtmlParser().GetBaseContainerAsync();
        }
    }
}
