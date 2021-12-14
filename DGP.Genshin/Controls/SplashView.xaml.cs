using DGP.Genshin.ViewModels;
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
            DataContext = App.GetViewModel<SplashViewModel>();
            InitializeComponent();
        }
    }
}
