using DGP.Genshin.Models.MiHoYo.Post;
using DGP.Genshin.Services;
using DGP.Snap.Framework.Extensions.System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
            this.DataContext = this;
            InitializeComponent();
            this.Log("initialized");
        }
        public List<Post> Posts { get => this.posts; set => Set(ref this.posts, value); }

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
    public class IdToPostConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            new Uri($@"https://bbs.mihoyo.com/ys/article/{value}");

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
