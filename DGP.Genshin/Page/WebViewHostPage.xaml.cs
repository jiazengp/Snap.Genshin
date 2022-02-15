using DGP.Genshin.Control;
using DGP.Genshin.DataModel.WebViewLobby;
using DGP.Genshin.Helper;
using DGP.Genshin.Helper.Notification;
using Microsoft.Toolkit.Uwp.Notifications;
using Snap.Core.Logging;
using Snap.Exception;
using System.Windows;
using System.Windows.Navigation;

namespace DGP.Genshin.Page
{
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
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            WebView?.Dispose();
            base.OnNavigatedFrom(e);
        }

        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await WebView.EnsureCoreWebView2Async();
                WebView.CoreWebView2.ProcessFailed += (s, e) => WebView?.Dispose();
            }
            catch
            {
                return;
            }

            if (entry is not null)
            {
                try
                {
                    WebView.CoreWebView2.Navigate(entry.NavigateUrl);
                }
                catch
                {
                    SecureToastNotificationContext.TryCatch(() =>
                    new ToastContentBuilder()
                    .AddText("无法导航到指定的网页")
                    .Show());
                }
            }
        }

        private async void WebViewNavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (entry?.JavaScript is not null)
            {
                this.Log("开始执行页面脚本");
                string? result = await WebView.CoreWebView2.ExecuteScriptAsync(entry.JavaScript);
                this.Log(result);
            }
        }
    }
}
