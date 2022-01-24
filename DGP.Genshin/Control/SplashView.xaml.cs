using DGP.Genshin.ViewModel;
using System.Windows.Controls;

namespace DGP.Genshin.Control
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
