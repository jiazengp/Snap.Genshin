using DGP.Genshin.Controls.TitleBarButtons;
using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Sign;
using DGP.Genshin.Services.Abstratcions;
using DGP.Genshin.Services.Settings;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf.Controls.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.ViewModels
{
    public class SignInViewModel : ObservableObject
    {
        //prevent multiple signin task
        private bool isSigningIn;
        private readonly ISettingService settingService;

        private SignInReward? signInReward;
        private SignInInfo? signInInfo;
        private UserGameRoleInfo? roleInfo;
        private UserGameRole? selectedRole;
        private IAsyncRelayCommand<TitleBarButton> initializeCommand;
        private IAsyncRelayCommand signInCommand;

        public SignInViewModel(ISettingService settingService)
        {
            this.settingService = settingService;
            InitializeCommand = new AsyncRelayCommand<TitleBarButton>(async t =>
            {
                if (t?.ShowAttachedFlyout<Grid>(this) == true)
                {
                    await InitializeSignInPanelDataAsync();
                }
            });
            SignInCommand = new AsyncRelayCommand(async() => {
                if (!isSigningIn)
                {
                    isSigningIn = true;
                    await App.Current.Dispatcher.InvokeAsync(SignInInternalAsync).Task.Unwrap();
                    isSigningIn = false;
                }
            });
            CookieManager.CookieChanged += CookieManagerCookieChanged;
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
                settingService[Setting.LastAutoSignInTime] = DateTime.Now;
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

        /// <summary>
        /// 初始化 <see cref="SignInReward"/> 与 <see cref="SignInInfo"/>
        /// </summary>
        /// <returns></returns>
        private async Task InitializeSignInPanelDataAsync()
        {
            SignInReward ??= await Task.Run(new SignInProvider(CookieManager.CurrentCookie).GetSignInRewardAsync);
            if (SignInInfo is null)
            {
                RoleInfo = await Task.Run(new UserGameRoleProvider(CookieManager.CurrentCookie).GetUserGameRolesAsync);
                SelectedRole = RoleInfo?.List?.FirstOrDefault(i => i.IsChosen);
            }
        }
        private void CookieManagerCookieChanged()
        {
            //reset sign in panel
            SignInReward = null;
            SignInInfo = null;
        }
        /// <summary>
        /// 签到奖励一览
        /// </summary>
        public SignInReward? SignInReward
        {
            get => signInReward;
            set => SetProperty(ref signInReward, value);
        }
        /// <summary>
        /// 当前签到状态信息
        /// </summary>
        public SignInInfo? SignInInfo
        {
            get => signInInfo;
            set => SetProperty(ref signInInfo, value);
        }
        /// <summary>
        /// 绑定的角色信息
        /// </summary>
        public UserGameRoleInfo? RoleInfo
        {
            get => roleInfo;
            set => SetProperty(ref roleInfo, value);
        }
        /// <summary>
        /// 选择的角色
        /// </summary>
        public UserGameRole? SelectedRole
        {
            get => selectedRole;
            set
            {
                SetProperty(ref selectedRole, value);
                OnSelectedRoleChanged();
            }
        }
        private async void OnSelectedRoleChanged()
        {
            if (SelectedRole is not null)
            {
                SignInInfo = await Task.Run(() => new SignInProvider(CookieManager.CurrentCookie).GetSignInInfoAsync(SelectedRole));
                SignInReward? reward = await Task.Run(new SignInProvider(CookieManager.CurrentCookie).GetSignInRewardAsync);
                ApplyItemOpacity(reward);
                SignInReward = reward;
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
        public IAsyncRelayCommand<TitleBarButton> InitializeCommand
        {
            get => initializeCommand;
            [MemberNotNull(nameof(initializeCommand))]
            set => SetProperty(ref initializeCommand, value);
        }
        public IAsyncRelayCommand SignInCommand
        {
            get => signInCommand; set => SetProperty(ref signInCommand, value);
        }
    }
}
