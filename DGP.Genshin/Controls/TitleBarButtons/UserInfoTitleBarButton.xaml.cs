using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.UserInfo;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    /// <summary>
    /// UserInfoTitleBarButton.xaml 的交互逻辑
    /// </summary>
    public partial class UserInfoTitleBarButton : TitleBarButton, INotifyPropertyChanged
    {
        private readonly Dictionary<string, UserInfo> userInfoCookieMap = new();
        private UserInfo userInfo;
        private ObservableCollection<UserInfo> userInfos;

        public UserInfo UserInfo { get => userInfo; set => Set(ref userInfo, value); }

        public ObservableCollection<UserInfo> UserInfos { get => userInfos; set => Set(ref userInfos, value); }

        public UserInfoTitleBarButton()
        {
            InitializeComponent();
            DataContext = this;
            CookieManager.CookieChanged += RefreshOnCurrentCookieChange;
            CookieManager.Cookies.CookieAdded += OnCookiesAdded;
            CookieManager.Cookies.CookieRemoved += OnCookieRemoved;
            RefreshOnCurrentCookieChange();
        }

        private async void OnCookiesAdded(string cookie)
        {
            string? prevUid = UserInfo?.Uid;
            UserInfo? info = await new UserInfoProvider(cookie).GetUserInfoAsync();
            if (info is not null)
            {
                UserInfos.Add(info);
                userInfoCookieMap.Add(cookie, info);
            }
        }

        public async void OnCookieRemoved(string cookie)
        {
            UserInfos.Remove(userInfoCookieMap[cookie]);
            await RefreshUserInfosAsync(cookie);
        }

        public async void RefreshOnCurrentCookieChange()
        {
            await RefreshUserInfosAsync();
        }

        public async Task RefreshUserInfosAsync(string? newCookie = null)
        {
            string? prevUid = UserInfo?.Uid;
            UserInfos.Clear();
            userInfoCookieMap.Clear();
            foreach (string cookie in CookieManager.Cookies)
            {
                UserInfo? info = await new UserInfoProvider(cookie).GetUserInfoAsync();
                if (info is not null)
                {
                    UserInfos.Add(info);
                    userInfoCookieMap.Add(cookie, info);
                }
            }
            UserInfo = UserInfos.FirstOrDefault(u => u.Uid == prevUid) ?? UserInfos.First();
            CookieManager.ChangeCurrentCookie(userInfoCookieMap[UserInfo]);
        }

        private void TitleBarButton_Click(object sender, RoutedEventArgs e)
        {
            sender.ShowAttachedFlyout<System.Windows.Controls.Grid>(this);
        }

        private async void RemoveCookieAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            if (CookieManager.Cookies.Count <= 1)
            {
                await new ContentDialog()
                {
                    Title = "删除用户失败",
                    Content = "我们需要至少一个用户的信息才能使程序正常运转"
                }.ShowAsync();
            }
            if (UserInfo is not null)
            {
                CookieManager.Cookies.Remove(userInfoCookieMap[UserInfo]);
            }
        }

        private async void AddCookieAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            await CookieManager.AddNewCookieToPoolAsync();
        }

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
    }
}
