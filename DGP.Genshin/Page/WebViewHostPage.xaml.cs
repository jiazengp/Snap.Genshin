using DGP.Genshin.Control;
using DGP.Genshin.Core.Notification;
using DGP.Genshin.DataModel.WebViewLobby;
using DGP.Genshin.Helper;
using Microsoft;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.VisualStudio.Threading;
using Microsoft.Web.WebView2.Core;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class WebViewHostPage : ModernWpf.Controls.Page
    {
        private WebViewEntry? entry;
        public WebViewHostPage()
        {
            if (WebView2Helper.IsSupported)
            {
                this.InitializeComponent();
            }
            else
            {
                new WebView2RuntimeWindow().ShowDialog();
                Verify.FailOperation("未找到可用的 WebView2运行时 安装");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.entry = e.ExtraData as WebViewEntry;
            base.OnNavigatedTo(e);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.WebView?.Dispose();
            base.OnNavigatedFrom(e);
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            this.OpenTargetUrlAsync().Forget();
        }
        private void WebViewNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            this.ExecuteJavaScriptAsync().Forget();
        }

        private async Task OpenTargetUrlAsync()
        {
            try
            {
                await this.WebView.EnsureCoreWebView2Async();
                this.WebView.CoreWebView2.ProcessFailed += (s, e) => this.WebView?.Dispose();
            }
            catch
            {
                return;
            }

            if (this.entry is not null)
            {
                try
                {
                    this.WebView.CoreWebView2.Navigate(this.entry.NavigateUrl);
                }
                catch
                {
                    new ToastContentBuilder()
                    .AddText("无法导航到指定的网页")
                    .SafeShow();
                }
            }
        }
        private async Task ExecuteJavaScriptAsync()
        {
            if (this.entry?.JavaScript is not null)
            {
                this.Log("开始执行脚本");
                string? result = await this.WebView.CoreWebView2.ExecuteScriptAsync(this.entry.JavaScript);
                this.Log($"执行完成:{result}");
            }
        }
    }
}
