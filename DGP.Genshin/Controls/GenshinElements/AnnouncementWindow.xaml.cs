using System;
using System.Windows;

namespace DGP.Genshin.Controls.GenshinElements
{
    /// <summary>
    /// AnnouncementWindow.xaml 的交互逻辑
    /// </summary>
    public sealed partial class AnnouncementWindow : Window, IDisposable
    {
        private readonly string? targetContent;
        public AnnouncementWindow(string? content)
        {
            targetContent = content;
            InitializeComponent();
        }

        public void Dispose()
        {
            ((IDisposable)WebView).Dispose();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await WebView.EnsureCoreWebView2Async();
            WebView.NavigateToString(targetContent);
        }
    }
}
