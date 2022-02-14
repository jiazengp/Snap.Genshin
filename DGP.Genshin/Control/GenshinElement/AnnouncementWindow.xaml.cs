using System;
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
            }
            catch
            {
                Close();
                return;
            }

            WebView.NavigateToString(targetContent);
        }
    }
}
