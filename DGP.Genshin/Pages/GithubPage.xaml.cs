using DGP.Genshin.Services;
using System.Windows;
using System.Windows.Navigation;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// GithubPage.xaml 的交互逻辑
    /// </summary>
    public partial class GithubPage : ModernWpf.Controls.Page
    {
        internal GithubService Service { get; set; } = new GithubService();
        public GithubPage()
        {
            this.DataContext = this.Service;
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e) => await this.Service.InitializeAsync();

        private async void AutoSuggestBox_QuerySubmitted(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxQuerySubmittedEventArgs args) => await this.Service.AddRepository(args.QueryText);
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            UnInitialize();
            base.OnNavigatedFrom(e);
        }

        private void UnInitialize()
        {
            this.Service.UnInitialize();
            this.Service = null;
        }
    }
}
