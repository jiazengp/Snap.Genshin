using DGP.Genshin.Services;
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
            this.journeyService = new JourneyService();
            this.DataContext = this.journeyService;
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e) =>
            await this.journeyService.InitializeAsync();
    }
}
