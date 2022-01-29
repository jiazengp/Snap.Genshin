using DGP.Genshin.ViewModel.Title;
using ModernWpf.Controls.Primitives;

namespace DGP.Genshin.Control.Title
{
    /// <summary>
    /// UserInfoTitleBarButton.xaml 的交互逻辑
    /// </summary>
    public partial class UserInfoTitleBarButton : TitleBarButton
    {
        public UserInfoTitleBarButton()
        {
            DataContext = App.AutoWired<UserInfoViewModel>();
            InitializeComponent();
        }

    }
}
