using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.Blackboard;
using DGP.Genshin.MiHoYoAPI.Post;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// TO-DO:
    /// 实现Post的刷新
    /// </summary>
    public partial class HomePage : System.Windows.Controls.Page, INotifyPropertyChanged
    {
        public HomePage()
        {
            DataContext = this;
            InitializeComponent();
            this.Log("initialized");
        }

        private IEnumerable<Post>? posts;
        private IEnumerable<BlackboardEvent>? blackboardEvents;

        public IEnumerable<Post>? Posts { get => posts; set => Set(ref posts, value); }
        public IEnumerable<BlackboardEvent>? BlackboardEvents { get => blackboardEvents; set => Set(ref blackboardEvents, value); }

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
            Posts = (await new PostProvider(CookieManager.CurrentCookie).GetOfficialRecommendedPostsAsync())?
                .OrderBy(p => p.OfficialType);
            BlackboardEvents = (await new BlackboardProvider().GetBlackboardEventsAsync())?
                .Where(b => b.Kind == "1");
        }
    }

}
