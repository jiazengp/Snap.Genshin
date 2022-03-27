using Microsoft.VisualStudio.Threading;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin.Control.Cookie
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public sealed partial class CookieWindow : Window, IDisposable
    {
        /// <summary>
        /// 构造一个新的Cookie窗体
        /// </summary>
        public CookieWindow()
        {
            this.InitializeComponent();
            this.WebView.CoreWebView2InitializationCompleted += this.WebViewCoreWebView2InitializationCompleted;
        }

        /// <summary>
        /// Cookie
        /// </summary>
        public string? Cookie { get; private set; }

        /// <summary>
        /// 是否登录成功
        /// </summary>
        public bool IsLoggedIn { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.WebView?.Dispose();
        }

        private void WebViewCoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            this.WebView.CoreWebView2.ProcessFailed += this.WebViewCoreWebView2ProcessFailed;
            if (e.IsSuccess)
            {
                this.ContinueButton.IsEnabled = true;
            }
        }

        private void WebViewCoreWebView2ProcessFailed(object? sender, CoreWebView2ProcessFailedEventArgs e)
        {
            this.ContinueButton.IsEnabled = false;
            this.WebView?.Dispose();
        }

        private void ContinueButtonClick(object sender, RoutedEventArgs e)
        {
            this.ContinueAsync().Forget();
        }

        private async Task ContinueAsync()
        {
            List<CoreWebView2Cookie> cookies = await this.WebView.CoreWebView2.CookieManager.GetCookiesAsync("https://bbs.mihoyo.com");
            string[] cookiesString = cookies.Select(c => $"{c.Name}={c.Value};").ToArray();
            this.Cookie = string.Concat(cookiesString);
            if (this.Cookie.Contains("account_id"))
            {
                this.IsLoggedIn = true;
                this.Close();
            }
        }
    }
}