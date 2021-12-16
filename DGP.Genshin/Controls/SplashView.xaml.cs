using DGP.Genshin.ViewModels;
using System.Windows.Controls;

namespace DGP.Genshin.Controls
{
    public partial class SplashView : UserControl
    {
        public SplashView()
        {
            DataContext = App.GetViewModel<SplashViewModel>();
            InitializeComponent();
        }
    }
}
