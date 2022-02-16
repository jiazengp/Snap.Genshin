using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin.Control.GenshinElement
{
    public sealed partial class AnnouncementWindow : Window, IDisposable
    {
        private readonly string? targetContent;
        public AnnouncementWindow(string? content)
        {
            //不需要在此处检查WebView2可用性，由使用方代为检查
            targetContent = content;
            InitializeComponent();
        }

        public void Dispose()
        {
            WebView?.Dispose();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await WebView.EnsureCoreWebView2Async();
                WebView.CoreWebView2.ProcessFailed += (s, e) => WebView?.Dispose();
                await MockMiHoYoGameJSSDK();
            }
            catch
            {
                Close();
                return;
            }

            WebView.NavigateToString(targetContent);
        }

        private async Task MockMiHoYoGameJSSDK()
        {
            WebView.CoreWebView2.WebMessageReceived += (o, args) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo(new Uri(args.TryGetWebMessageAsString()).AbsoluteUri)
                    {
                        UseShellExecute = true
                    });
                }
                catch
                {
                    // ignored
                }
            };
            await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
                "window.miHoYoGameJSSDK = {openInBrowser: function(url){ window.chrome.webview.postMessage(url); }}");
        }
    }
}
