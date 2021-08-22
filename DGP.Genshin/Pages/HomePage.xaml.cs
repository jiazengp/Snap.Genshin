using DGP.Genshin.Models.MiHoYo.BBSAPI.Post;
using DGP.Genshin.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page, INotifyPropertyChanged
    {
        private List<Post> posts;

        public HomePage()
        {
            DataContext = this;
            InitializeComponent();
        }
        public List<Post> Posts { get => posts; set => Set(ref posts, value); }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Posts = (await new MiHoYoBBSService().GetOfficialRecommendedPostsAsync())
                .OrderBy(p => p.OfficialType).ToList();
        }

        private void PostButtonClick(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            Process.Start($@"https://bbs.mihoyo.com/ys/article/{b.Tag}");
        }
    }
}
