using DGP.Genshin.Controls;
using DGP.Genshin.Helpers;
using Microsoft.Web.WebView2.Core;
using Snap.Core.Logging;
using Snap.Exception;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    public partial class MapPage : Page, IDisposable
    {
        public MapPage()
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

        #region Standard Disopse
        private bool disposed;
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //managed
                }
                //WebView2 can still be null
                App.Current.Dispatcher.Invoke(() => WebView?.Dispose());

                disposed = true;
            }
        }
        ~MapPage()
        {
            Dispose(false);
        }
        #endregion

        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await Task.Delay(2000);
            //在此处操作WebView，移除右下角 二维码 Banner
            string result = await WebView.ExecuteScriptAsync(
                "var divs=document.getElementsByClassName(\"bbs-qr\");" +
                "for(i=0;i<divs.length;i++){if(divs[i]!=null)divs[i].parentNode.removeChild(divs[i])}");
            this.Log(result);
        }
    }
}
