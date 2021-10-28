using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.UserInfo;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System;
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
        private CookieUserInfo? selectedCookieUserInfo;
        private ObservableCollection<CookieUserInfo> cookieUserInfos = new();

        public CookieUserInfo? SelectedCookieUserInfo
        {
            get => selectedCookieUserInfo; set
            {
                Set(ref selectedCookieUserInfo, value);
                CookieManager.ChangeOrIgnoreCurrentCookie(value?.Cookie);
            }
        }

        public ObservableCollection<CookieUserInfo> CookieUserInfos { get => cookieUserInfos; set => Set(ref cookieUserInfos, value); }

        public UserInfoTitleBarButton()
        {
            InitializeComponent();
            DataContext = this;

            CookieManager.Cookies.CookieAdded += OnCookiesAdded;
            CookieManager.Cookies.CookieRemoved += OnCookieRemoved;
        }

        internal async Task InitializeAsync()
        {
            CookieUserInfos.Clear();
            foreach (string cookie in CookieManager.Cookies)
            {
                UserInfo? info = await new UserInfoProvider(cookie).GetUserInfoAsync();
                if (info is not null)
                {
                    CookieUserInfos.Add(new CookieUserInfo(cookie, info));
                }
            }
            SelectedCookieUserInfo = CookieUserInfos.First();
        }

        private async void OnCookiesAdded(string newCookie)
        {
            UserInfo? newInfo = await new UserInfoProvider(newCookie).GetUserInfoAsync();
            if (newInfo is not null)
            {
                CookieUserInfos.Add(new CookieUserInfo(newCookie, newInfo));
            }
        }

        public void OnCookieRemoved(string cookie)
        {
            CookieUserInfo? prevSelected = SelectedCookieUserInfo;
            CookieUserInfo? currentRemoved = CookieUserInfos.First(u => u.Cookie == cookie);
            CookieUserInfos.Remove(currentRemoved);
            if (prevSelected == currentRemoved)
            {
                SelectedCookieUserInfo = CookieUserInfos.First();
            }
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
            if (SelectedCookieUserInfo is not null)
            {
                CookieManager.Cookies.Remove(SelectedCookieUserInfo.Cookie);
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
