using DGP.Genshin.Common.Exceptions;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Helpers;
using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// MapPage.xaml 的交互逻辑
    /// </summary>
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
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                //WebView2 can still be null
                App.Current.Dispatcher.Invoke(() => WebView?.Dispose());

                // Note disposing has been done.
                disposed = true;
            }
        }
        ~MapPage()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(disposing: false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }
        #endregion

        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await Task.Delay(2000);
            //在此处操作WebView，移除右下角 二维码 Banner
            string result = await WebView.ExecuteScriptAsync(
                "var divs=document.getElementsByClassName(\"bbs-qr\");for(i=0;i<divs.length;i++){if(divs[i]!=null)divs[i].parentNode.removeChild(divs[i])}");
            this.Log(result);
        }
    }
}
