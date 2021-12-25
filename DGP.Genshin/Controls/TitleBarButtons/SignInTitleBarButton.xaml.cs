using DGP.Genshin.ViewModels;
using ModernWpf.Controls.Primitives;

namespace DGP.Genshin.Controls.TitleBarButtons
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
