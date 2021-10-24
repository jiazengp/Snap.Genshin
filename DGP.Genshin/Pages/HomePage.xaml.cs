using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.Post;
using DGP.Genshin.Services.Launching;
using DGP.Snap.Framework.Extensions.System;
using ModernWpf.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// TO-DO:
    /// 实现Post的刷新
    /// </summary>
    public partial class HomePage : System.Windows.Controls.Page, INotifyPropertyChanged
    {
        private List<Post>? posts;

        public HomePage()
        {
            Launcher = LaunchService.Instance;
            DataContext = this;
            InitializeComponent();
            this.Log("initialized");
        }

        public List<Post>? Posts { get => posts; set => Set(ref posts, value); }
        public LaunchService Launcher { get; set; }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Posts = await Task.Run(() =>
            new PostProvider(CookieManager.Cookie)
            .GetOfficialRecommendedPosts()?
            .OrderBy(p => p.OfficialType).ToList());
        }
        private void LaunchButtonClick(object sender, RoutedEventArgs e)
        {
            Launcher.Launch(Launcher.CurrentScheme, async ex =>
            {
                await new ContentDialog()
                {
                    Title = "启动失败",
                    Content = ex.Message,
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            });
        }
    }
}
