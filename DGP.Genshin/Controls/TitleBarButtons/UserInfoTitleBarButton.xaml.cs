using DGP.Genshin.ViewModels.TitleBarButtons;
using ModernWpf.Controls.Primitives;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    /// <summary>
    /// UserInfoTitleBarButton.xaml 的交互逻辑
    /// </summary>
    public partial class UserInfoTitleBarButton : TitleBarButton
    {
        public UserInfoTitleBarButton()
        {
            DataContext = App.GetViewModel<UserInfoViewModel>();
            InitializeComponent();
        }

    }
}
