using DGP.Genshin.ViewModel;
using System.Windows.Controls;

namespace DGP.Genshin.Control
{
    public sealed partial class SplashView : UserControl
    {
        public SplashView()
        {
            DataContext = App.AutoWired<SplashViewModel>();
            InitializeComponent();
        }
    }
}
