using DGP.Genshin.Services;
using DGP.Snap.Framework.Extensions.System;
using ModernWpf.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Navigation;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// GithubPage.xaml 的交互逻辑
    /// </summary>
    public partial class GithubPage : Page
    {
        [NotNull]
        internal GithubService? Service { get; set; } = new GithubService();
        public GithubPage()
        {
            this.DataContext = this.Service;
            this.InitializeComponent();
            this.Log("initialized");
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e) => await this.Service.InitializeAsync();

        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) =>
            await this.Service.AddRepository(args.QueryText);
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.UnInitialize();
            base.OnNavigatedFrom(e);
        }

        private void UnInitialize()
        {
            this.Log("uninitialized");
            this.Service.UnInitialize();
            this.Service = null;
        }
    }
}
