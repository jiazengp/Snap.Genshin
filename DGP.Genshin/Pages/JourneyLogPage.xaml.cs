using DGP.Genshin.Services;
using DGP.Genshin.Common.Extensions.System.Windows.Threading;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// JourneyLogPage.xaml 的交互逻辑
    /// </summary>
    public partial class JourneyLogPage : Page
    {
        private readonly JourneyService journeyService;
        public JourneyLogPage()
        {
            journeyService = new JourneyService();
            DataContext = journeyService;
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await journeyService.InitializeAsync();
        }
        ~JourneyLogPage()
        {
            App.Current.Invoke(() => DataContext = null);
        }
    }
}
