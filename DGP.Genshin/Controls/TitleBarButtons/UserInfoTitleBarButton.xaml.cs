using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.UserInfo;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    /// <summary>
    /// UserInfoTitleBarButton.xaml 的交互逻辑
    /// </summary>
    public partial class UserInfoTitleBarButton : TitleBarButton
    {
        public UserInfo? UserInfo
        {
            get => (UserInfo)GetValue(UserInfoProperty);
            set => SetValue(UserInfoProperty, value);
        }
        public static readonly DependencyProperty UserInfoProperty =
            DependencyProperty.Register("UserInfo", typeof(UserInfo), typeof(UserInfoTitleBarButton), new PropertyMetadata(null));

        public UserInfoTitleBarButton()
        {
            InitializeComponent();
            DataContext = this;
            CookieManager.CookieRefreshed += Refresh;
        }

        public async void Refresh()
        {
            UserInfo = await Task.Run(new UserInfoProvider(CookieManager.Cookie).GetUserInfo);
        }

        public async Task RefreshAsync()
        {
            UserInfo = await Task.Run(new UserInfoProvider(CookieManager.Cookie).GetUserInfo);
        }

        private void TitleBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (FlyoutBase.GetAttachedFlyout((TitleBarButton)sender) is Flyout flyout)
            {
                if (flyout?.Content is Grid grid)
                {
                    grid.DataContext = this;
                    FlyoutBase.ShowAttachedFlyout((TitleBarButton)sender);
                }
            }
        }
    }
}
