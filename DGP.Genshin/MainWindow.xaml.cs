using DGP.Genshin.Pages;
using DGP.Genshin.Services;
using DGP.Genshin.Services.Settings;
using ModernWpf.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;

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
            this.InitializeComponent();
            this.NavigationService = new NavigationService(this, this.NavView, this.ContentFrame);
        }
        private void SplashInitializeCompleted()
        {
            this.NavigationService.Navigate<HomePage>(true);
        }
    }
}
