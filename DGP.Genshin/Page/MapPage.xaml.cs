using DGP.Genshin.Control;
using DGP.Genshin.Helper;
using Snap.Exception;
using System;
using System.Windows.Controls;

namespace DGP.Genshin.Page
{
    public partial class MapPage : System.Windows.Controls.Page, IDisposable
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
                try
                {
                    WebView?.Dispose();
                }
                catch { }

                disposed = true;
            }
        }
        ~MapPage()
        {
            Dispose(false);
        }
        #endregion
    }
}
