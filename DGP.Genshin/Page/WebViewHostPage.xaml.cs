using DGP.Genshin.Control;
using DGP.Genshin.DataModel.WebViewLobby;
using DGP.Genshin.Helper;
using Snap.Core.Logging;
using Snap.Exception;
using System.Windows;
using System.Windows.Navigation;

namespace DGP.Genshin.Page
{
    /// <summary>
    /// WebViewHostPage.xaml 的交互逻辑
    /// </summary>
    public partial class WebViewHostPage : ModernWpf.Controls.Page
    {
        private WebViewEntry? entry;
        public WebViewHostPage()
        {
            if (WebView2Helper.IsSupported)
            {
                InitializeComponent();
            }
            else
            {
                new WebView2RuntimeWindow().ShowDialog();
                throw new SnapGenshinInternalException("未找到可用的 WebView2运行时 安装");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            entry = e.ExtraData as WebViewEntry;
            base.OnNavigatedTo(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await WebView.EnsureCoreWebView2Async();
            }
            catch
            {
                return;
            }

            if(entry is not null)
            {
                WebView.CoreWebView2.Navigate(entry.NavigateUrl);
            }
        }

        private async void WebViewNavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (entry?.JavaScript is not null)
            {
                this.Log("开始执行页面脚本");
                var result = await WebView.CoreWebView2.ExecuteScriptAsync(entry.JavaScript);
                this.Log(result);
            }
        }
    }
}
