using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DGP.Genshin.Controls.Cookies
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public sealed partial class CookieWindow : Window, IDisposable
    {
        public string? Cookie { get; private set; }
        public bool IsLoggedIn { get; private set; }

        public CookieWindow()
        {
            InitializeComponent();
            WebView.CoreWebView2InitializationCompleted += WebViewCoreWebView2InitializationCompleted;
        }

        private void WebViewCoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                ContinueButton.IsEnabled = true;
            }
        }

        private async void ContinueButtonClick(object sender, RoutedEventArgs e)
        {
            if (WebView.IsInitialized)
            {
                List<CoreWebView2Cookie> cookies = await WebView.CoreWebView2.CookieManager.GetCookiesAsync("https://bbs.mihoyo.com");
                string[] cookiesString = cookies.Select(c => $"{c.Name}={c.Value};").ToArray();
                Cookie = string.Concat(cookiesString);
                if (Cookie.Contains("account_id"))
                {
                    IsLoggedIn = true;
                    Close();
                }
            }
        }

        public void Dispose()
        {
            ((IDisposable)WebView).Dispose();
        }
    }
}
