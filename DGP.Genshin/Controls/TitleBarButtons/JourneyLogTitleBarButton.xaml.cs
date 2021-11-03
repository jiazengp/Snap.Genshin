using DGP.Genshin.Services;
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
        private readonly JourneyService journeyService;
        public JourneyLogTitleBarButton()
        {
            journeyService = new JourneyService();
            InitializeComponent();
        }

        private async void JourneyLogTitleBarButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender.ShowAttachedFlyout<Grid>(journeyService))
            {
                await journeyService.InitializeAsync();
            }
        }
    }
}
