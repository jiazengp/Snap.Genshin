using DGP.Genshin.ViewModels;
using ModernWpf.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;

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
