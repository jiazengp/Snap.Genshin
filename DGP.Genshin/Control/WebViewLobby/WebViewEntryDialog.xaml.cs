using DGP.Genshin.DataModel.WebViewLobby;
using Microsoft.VisualStudio.Threading;
using ModernWpf.Controls;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin.Control.WebViewLobby
{
    /// <summary>
    /// 自定义网页对话框
    /// </summary>
    public partial class WebViewEntryDialog : ContentDialog
    {
        private static readonly DependencyProperty NavigateUrlProperty = Property<WebViewEntryDialog>.Depend<string>(nameof(NavigateUrl));
        private static readonly DependencyProperty EntryNameProperty = Property<WebViewEntryDialog>.Depend<string>(nameof(EntryName));
        private static readonly DependencyProperty IconUrlProperty = Property<WebViewEntryDialog>.Depend<string>(nameof(IconUrl));
        private static readonly DependencyProperty JavaScriptProperty = Property<WebViewEntryDialog>.Depend<string>(nameof(JavaScript));
        private static readonly DependencyProperty ShowInNavViewProperty = Property<WebViewEntryDialog>.Depend(nameof(ShowInNavView), true);

        /// <summary>
        /// 构造一个新的自定义网页对话框
        /// </summary>
        /// <param name="entry">编辑的自定义网页入口</param>
        public WebViewEntryDialog(WebViewEntry? entry = null)
        {
            if (entry is not null)
            {
                this.NavigateUrl = entry.NavigateUrl;
                this.EntryName = entry.Name;
                this.IconUrl = entry.IconUrl;
                this.JavaScript = entry.JavaScript;
                this.ShowInNavView = entry.ShowInNavView;
            }

            this.DataContext = this;
            this.InitializeComponent();
        }

        /// <summary>
        /// 导航Url
        /// </summary>
        public string NavigateUrl
        {
            get => (string)this.GetValue(NavigateUrlProperty);

            set => this.SetValue(NavigateUrlProperty, value);
        }

        /// <summary>
        /// 入口名称
        /// </summary>
        public string EntryName
        {
            get => (string)this.GetValue(EntryNameProperty);

            set => this.SetValue(EntryNameProperty, value);
        }

        /// <summary>
        /// 图标Url
        /// </summary>
        [AllowNull]
        public string IconUrl
        {
            get => (string)this.GetValue(IconUrlProperty);

            set => this.SetValue(IconUrlProperty, value);
        }

        /// <summary>
        /// JS脚本
        /// </summary>
        [AllowNull]
        public string JavaScript
        {
            get => (string)this.GetValue(JavaScriptProperty);

            set => this.SetValue(JavaScriptProperty, value);
        }

        /// <summary>
        /// 指示是否在导航栏中显示
        /// </summary>
        public bool ShowInNavView
        {
            get => (bool)this.GetValue(ShowInNavViewProperty);

            set => this.SetValue(ShowInNavViewProperty, value);
        }

        /// <summary>
        /// 获取用户编辑后的入口对象
        /// </summary>
        /// <returns>编辑后的入口对象/returns>
        public async Task<WebViewEntry?> GetWebViewEntryAsync()
        {
            if (await this.ShowAsync() != ContentDialogResult.Secondary)
            {
                if (this.NavigateUrl is not null)
                {
                    if (this.JavaScript is not null)
                    {
                        this.JavaScript = new Regex("(\r\n|\r|\n)").Replace(this.JavaScript, " ");
                        this.JavaScript = new Regex(@"\s+").Replace(this.JavaScript, " ");
                    }

                    return new WebViewEntry(this);
                }
            }

            return null;
        }

        private void AutoTitleIconButtonClick(object sender, RoutedEventArgs e)
        {
            this.FindTitleIconAsync().Forget();
        }

        private async Task FindTitleIconAsync()
        {
            if (this.NavigateUrl is not null)
            {
                using (HttpClient client = new())
                {
                    if (Uri.TryCreate(this.NavigateUrl, UriKind.Absolute, out Uri? navigateUri))
                    {
                        string response;
                        try
                        {
                            response = await client.GetStringAsync(navigateUri);
                        }
                        catch
                        {
                            response = string.Empty;
                        }

                        Match m;

                        // 匹配标题
                        m = new Regex("(?<=<title>)(.*)(?=</title>)").Match(response);
                        this.EntryName = m.Success ? m.Value : "自动获取失败";

                        // 匹配图标
                        m = new Regex("(?<=rel[ =]+\"[shortcut icon]+\" href=\")(.*?)(?=\")").Match(response);

                        if (m.Success)
                        {
                            string? path = m.Value;
                            if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri? pathUri))
                            {
                                Uri iconUri = pathUri.IsAbsoluteUri
                                    ? pathUri
                                    : new Uri(new Uri(this.NavigateUrl), pathUri);
                                this.IconUrl = iconUri.ToString();
                            }
                        }
                    }
                    else
                    {
                        this.NavigateUrl = "该Url无效";
                    }
                }
            }
        }
    }
}