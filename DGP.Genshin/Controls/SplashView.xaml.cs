using DGP.Genshin.Services;
using System;
using System.Threading;
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
        internal DataService DataService => DataService.Instance;

        public SplashView()
        {
            this.DataContext = DataService.Instance;
            DataService.CompleteStateChanged += async b =>
            {
                if (b)
                {
                    //wait for animation
                    await Task.Run(() => Thread.Sleep(1000));
                    InitializeCompleted?.Invoke();
                }
            };
            this.InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await DataService.Instance.CheckAllIntegrityAsync();
        }

        public event Action InitializeCompleted;
    }
}
