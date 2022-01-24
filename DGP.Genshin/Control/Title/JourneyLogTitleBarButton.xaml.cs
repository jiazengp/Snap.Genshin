using DGP.Genshin.ViewModel.Title;
using ModernWpf.Controls.Primitives;

namespace DGP.Genshin.Control.Title
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
