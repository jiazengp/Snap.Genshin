using DGP.Genshin.ViewModels;
using ModernWpf.Controls.Primitives;
using System.Windows;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    /// <summary>
    /// SignInTitleBarButton.xaml 的交互逻辑
    /// </summary>
    public partial class SignInTitleBarButton : TitleBarButton
    {
        public SignInTitleBarButton()
        {
            DataContext = App.GetViewModel<SignInViewModel>();
            InitializeComponent();
        }
    }
}
