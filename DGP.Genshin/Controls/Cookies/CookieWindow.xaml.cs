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
        }

        private async void ContinueButtonClick(object sender, RoutedEventArgs e)
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

        public void Dispose()
        {
            ((IDisposable)WebView).Dispose();
        }
    }
}
