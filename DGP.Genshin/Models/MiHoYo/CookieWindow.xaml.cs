using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DGP.Genshin.Models.MiHoYo
{
    /// <summary>
    /// CookieWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CookieWindow : Window,IDisposable
    {
        public string Cookie { get; set; }
        public bool IsLoggedIn { get; set; } = false;

        public CookieWindow()
        {
            InitializeComponent();
        }

        private async void ContinueButtonClick(object sender, RoutedEventArgs e)
        {
            List<CoreWebView2Cookie> cookies = await this.WebView.CoreWebView2.CookieManager.GetCookiesAsync("https://bbs.mihoyo.com");
            string[] cookiesString = cookies.Select(c => $"{c.Name}={c.Value};").ToArray();
            this.Cookie = String.Concat(cookiesString);
            if (this.Cookie.Contains("account_id"))
            {
                this.IsLoggedIn = true;
                Close();
            }
        }

        public void Dispose() => ((IDisposable)this.WebView).Dispose();
    }
}
