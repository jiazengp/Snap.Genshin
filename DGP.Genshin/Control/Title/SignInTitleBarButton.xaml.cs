using DGP.Genshin.ViewModel.Title;
using ModernWpf.Controls.Primitives;

namespace DGP.Genshin.Control.Title
{
    public partial class SignInTitleBarButton : TitleBarButton
    {
        public SignInTitleBarButton()
        {
            DataContext = App.GetViewModel<SignInViewModel>();
            InitializeComponent();
        }
    }
}
