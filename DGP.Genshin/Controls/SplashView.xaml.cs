using DGP.Genshin.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls
{
    /// <summary>
    /// SplashView.xaml 的交互逻辑
    /// </summary>
    public partial class SplashView : UserControl
    {
        public SplashView()
        {
            this.DataContext = MetaDataService.Instance;
            MetaDataService.Instance.CompleteStateChanged += async b =>
            {
                if (b)
                {
                    //wait for animation
                    await Task.Delay(1000);
                    InitializeCompleted?.Invoke();
                }
            };
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e) => await MetaDataService.Instance.CheckAllIntegrityAsync();

        public event Action InitializeCompleted;
    }
}
