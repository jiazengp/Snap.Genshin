using DGP.Genshin.ViewModels;
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
            //suppress the databinding warning
            //PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
            InitializeComponent();
            DataContext = App.GetViewModel<UserInfoViewModel>();
        }

    }
}
