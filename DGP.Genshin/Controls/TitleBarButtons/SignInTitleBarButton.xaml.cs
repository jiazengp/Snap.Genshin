using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.Sign;
using DGP.Genshin.MiHoYoAPI.User;
using DGP.Genshin.Services.Settings;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf.Controls.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    /// <summary>
    /// SignInTitleBarButton.xaml 的交互逻辑
    /// </summary>
    public partial class SignInTitleBarButton : TitleBarButton
    {
        public SignInTitleBarButton()
        {
            CookieManager.CookieChanged += CookieManagerCookieChanged;
            InitializeComponent();
            DataContext = this;
        }

        private void CookieManagerCookieChanged()
        {
            //reset sign in panel
            SignInReward = null;
            SignInInfo = null;
        }

        #region Observable
        /// <summary>
        /// 签到奖励一览
        /// </summary>
        public SignInReward? SignInReward
        {
            get => (SignInReward)GetValue(SignInRewardProperty);
            set => SetValue(SignInRewardProperty, value);
        }
        public static readonly DependencyProperty SignInRewardProperty =
            DependencyProperty.Register("SignInReward", typeof(SignInReward), typeof(SignInTitleBarButton), new PropertyMetadata(null));
        /// <summary>
        /// 当前签到状态信息
        /// </summary>
        public SignInInfo? SignInInfo
        {
            get => (SignInInfo)GetValue(SignInInfoProperty);
            set => SetValue(SignInInfoProperty, value);
        }
        public static readonly DependencyProperty SignInInfoProperty =
            DependencyProperty.Register("SignInInfo", typeof(SignInInfo), typeof(SignInTitleBarButton), new PropertyMetadata(null));
        /// <summary>
        /// 绑定的角色信息
        /// </summary>
        public UserGameRoleInfo? RoleInfo
        {
            get => (UserGameRoleInfo)GetValue(RoleInfoProperty);
            set => SetValue(RoleInfoProperty, value);
        }
        public static readonly DependencyProperty RoleInfoProperty =
            DependencyProperty.Register("RoleInfo", typeof(UserGameRoleInfo), typeof(SignInTitleBarButton), new PropertyMetadata(null));
        /// <summary>
        /// 选择的角色
        /// </summary>
        public UserGameRole? SelectedRole
        {
            get => (UserGameRole)GetValue(SelectedRoleProperty);
            set => SetValue(SelectedRoleProperty, value);
        }
        public static readonly DependencyProperty SelectedRoleProperty =
            DependencyProperty.Register("SelectedRole", typeof(UserGameRole), typeof(SignInTitleBarButton), new PropertyMetadata(null, OnSelectedRoleChanged));

        private static async void OnSelectedRoleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SignInTitleBarButton button = (SignInTitleBarButton)d;

            if (e.NewValue is UserGameRole role)
            {
                button.SignInInfo = await Task.Run(() => new SignInProvider(CookieManager.CurrentCookie).GetSignInInfoAsync(role));
                SignInReward? reward = await Task.Run(new SignInProvider(CookieManager.CurrentCookie).GetSignInRewardAsync);
                button.ApplyItemOpacity(reward);
                button.SignInReward = reward;
            }
        }
        #endregion

        private async void SignInTitleBarButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender.ShowAttachedFlyout<Grid>(this))
            {
                await InitializeSignInPanelDataAsync();
            }
        }

        /// <summary>
        /// 更新物品透明度
        /// </summary>
        private void ApplyItemOpacity(SignInReward? reward)
        {
            for (int i = 0; i < reward?.Awards?.Count; i++)
            {
                SignInAward item = reward.Awards[i];
                item.Opacity = (i + 1) <= SignInInfo?.TotalSignDay ? 0.2 : 1;
            }
        }

        /// <summary>
        /// 初始化 <see cref="SignInReward"/> 与 <see cref="SignInInfo"/>
        /// </summary>
        /// <returns></returns>
        private async Task InitializeSignInPanelDataAsync()
        {
            if (SignInReward is null)
            {
                SignInReward = await Task.Run(new SignInProvider(CookieManager.CurrentCookie).GetSignInRewardAsync);
            }
            if (SignInInfo is null)
            {
                RoleInfo = await Task.Run(new UserGameRoleProvider(CookieManager.CurrentCookie).GetUserGameRolesAsync);
                SelectedRole = RoleInfo?.List?.First();
            }
        }
        //prevent multiple signin task
        private bool isSigningIn;
        private async void SignInButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isSigningIn)
            {
                isSigningIn = true;
                await Application.Current.Dispatcher.InvokeAsync(SignInInternalAsync).Task.Unwrap();
                isSigningIn = false;
            }
        }

        private async Task SignInInternalAsync()
        {
            if (SelectedRole is null)
            {
                isSigningIn = false;
                throw new InvalidOperationException("无角色信息时不能签到");
            }

            SignInResult? result = await new SignInProvider(CookieManager.CurrentCookie).SignInAsync(SelectedRole);
            if (result is not null)
            {
                SettingService.Instance[Setting.LastAutoSignInTime] = DateTime.Now;
            }
            new ToastContentBuilder()
                    .AddText(result is null ? "签到失败" : "签到成功")
                    .AddText(SelectedRole.ToString())
                    .AddAttributionText("米游社每日签到")
                    .Show();
            SignInReward? reward = await new SignInProvider(CookieManager.CurrentCookie).GetSignInRewardAsync();
            ApplyItemOpacity(reward);
            SignInReward = reward;
            //refresh info
            SignInInfo = await new SignInProvider(CookieManager.CurrentCookie).GetSignInInfoAsync(SelectedRole);
        }
    }
}
