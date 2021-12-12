using DGP.Genshin.Services.Launching;
using ModernWpf.Controls.Primitives;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    /// <summary>
    /// LaunchTitleBarButton.xaml 的交互逻辑
    /// </summary>
    public partial class LaunchTitleBarButton : TitleBarButton
    {
        public LaunchTitleBarButton()
        {
            DataContext = App.GetViewModel<LaunchViewModel>();
            InitializeComponent();
        }
    }
}
