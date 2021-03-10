using DGP.Genshin.Pages;
using DGP.Genshin.Services;
using System.Windows;

namespace DGP.Genshin
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NavigationService NavigationService;

        public MainWindow()
        {
            //DataContext = this;
            this.InitializeComponent();
            this.NavigationService = new NavigationService(this, this.NavView, this.ContentFrame);
            this.NavigationService.Navigate<HomePage>(true);
        }
    }
}
