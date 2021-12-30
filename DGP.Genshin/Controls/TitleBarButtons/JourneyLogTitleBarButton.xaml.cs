using DGP.Genshin.ViewModels.TitleBarButtons;
using ModernWpf.Controls.Primitives;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    /// <summary>
    /// JourneyLogTitleBarButton.xaml 的交互逻辑
    /// </summary>
    public partial class JourneyLogTitleBarButton : TitleBarButton
    {
        public JourneyLogTitleBarButton()
        {
            DataContext = App.GetViewModel<JourneyViewModel>();
            InitializeComponent();
        }
    }
}
