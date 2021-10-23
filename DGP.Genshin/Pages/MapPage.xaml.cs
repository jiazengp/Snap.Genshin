using DGP.Snap.Framework.Extensions.System;
using Microsoft.Web.WebView2.Core;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// MapPage.xaml 的交互逻辑
    /// </summary>
    public partial class MapPage : Page
    {
        public MapPage()
        {
            InitializeComponent();
        }

        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            //在此处操作WebView，移除右下角 二维码 Banner
            string result = await WebView.ExecuteScriptAsync(
                "var divs=document.getElementsByClassName(\"bbs-qr\");for(i=0;i<divs.length;i++){if(divs[i]!=null)divs[i].parentNode.removeChild(divs[i])}");
            this.Log(result);
        }
    }
}
