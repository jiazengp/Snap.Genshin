using DGP.Genshin.Common.Exceptions;
using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.Calculation;
using DGP.Genshin.MiHoYoAPI.GameRole;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// PromotionCalculatePage.xaml 的交互逻辑
    /// </summary>
    public partial class PromotionCalculatePage : Page, INotifyPropertyChanged
    {
        private Calculator calculator;
        private UserGameRoleProvider userGameRoleProvider;

        private IEnumerable<UserGameRole>? userGameRoles;
        private UserGameRole? selectedUserGameRole;
        private IEnumerable<Avatar>? avatars;
        private Avatar? selectedAvatar;
        private AvatarDetailData? avatarDetailData;
        private Consumption? consumption = new();

        public IEnumerable<UserGameRole>? UserGameRoles { get => userGameRoles; set => Set(ref userGameRoles, value); }
        public UserGameRole? SelectedUserGameRole { get => selectedUserGameRole; set { Set(ref selectedUserGameRole, value); RefreshAvatarListAsync(); } }
        public IEnumerable<Avatar>? Avatars { get => avatars; set => Set(ref avatars, value); }
        public Avatar? SelectedAvatar { get => selectedAvatar; set { Set(ref selectedAvatar, value); RefreshAvatarDetailDataAsync(); } }
        public AvatarDetailData? AvatarDetailData { get => avatarDetailData; set => Set(ref avatarDetailData, value); }
        public Consumption? Consumption { get => consumption; set => Set(ref consumption, value); }

        private async void RefreshAvatarDetailDataAsync()
        {
            if (SelectedUserGameRole is not null && SelectedAvatar is not null)
            {
                string uid = SelectedUserGameRole.GameUid ?? throw new UnexceptedNullException("uid 不应为 null");
                string region = SelectedUserGameRole.Region ?? throw new UnexceptedNullException("region 不应为 null");
                int avatarId = SelectedAvatar.Id;

                AvatarDetailData = await calculator.GetSyncedAvatarDetailDataAsync(avatarId, uid, region);
            }
            if (SelectedAvatar is not null)
            {
                SelectedAvatar.LevelTarget = SelectedAvatar.MaxLevel;
                AvatarDetailData?.SkillList?.ForEach(x => x.LevelTarget = x.MaxLevel);
                if (AvatarDetailData?.Weapon is not null)
                {
                    AvatarDetailData.Weapon.LevelTarget = AvatarDetailData.Weapon.MaxLevel;
                }
                AvatarDetailData?.ReliquaryList?.ForEach(x => x.LevelTarget = x.MaxLevel);
            }
        }

        /// <summary>
        /// 准备角色列表
        /// </summary>
        /// <exception cref="UnexceptedNullException"></exception>
        private async void RefreshAvatarListAsync()
        {
            if (SelectedUserGameRole is not null)
            {
                string uid = SelectedUserGameRole.GameUid ?? throw new UnexceptedNullException("uid 不应为 null");
                string region = SelectedUserGameRole.Region ?? throw new UnexceptedNullException("region 不应为 null");
                Avatars = await calculator.GetSyncedAvatarListAsync(new(uid, region), true);
                SelectedAvatar = Avatars?.FirstOrDefault();
            }
        }

        private async void ComputeAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            if (SelectedAvatar is not null)
            {
                if (AvatarDetailData != null)
                {
                    AvatarPromotionDelta delta = new()
                    {
                        AvatarId = SelectedAvatar.Id,
                        AvatarLevelCurrent = SelectedAvatar.LevelCurrent,
                        AvatarLevelTarget = SelectedAvatar.LevelTarget,
                        SkillList = AvatarDetailData.SkillList?.Select(s => s.ToPromotionDelta()) ?? new List<PromotionDelta>(),
                        Weapon = AvatarDetailData.Weapon?.ToPromotionDelta(),
                        ReliquaryList = AvatarDetailData.ReliquaryList?.Select(r => r.ToPromotionDelta()) ?? new List<PromotionDelta>()
                    };
                    Consumption = await calculator.ComputeAsync(delta);
                }
            }
        }

        public PromotionCalculatePage()
        {
            DataContext = this;
            InitializeComponent();
            calculator = new(CookieManager.CurrentCookie);
            userGameRoleProvider = new(CookieManager.CurrentCookie);
            CookieManager.CookieChanged += CookieManagerCookieChanged;
        }

        private async void CookieManagerCookieChanged()
        {
            calculator = new(CookieManager.CurrentCookie);
            userGameRoleProvider = new(CookieManager.CurrentCookie);

            UserGameRoleInfo? gameRoles = await userGameRoleProvider.GetUserGameRolesAsync();
            if (gameRoles?.List is not null)
            {
                UserGameRoles = gameRoles.List;
            }
            SelectedUserGameRole = UserGameRoles?.FirstOrDefault();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UserGameRoleInfo? gameRoles = await userGameRoleProvider.GetUserGameRolesAsync();
            if (gameRoles?.List is not null)
            {
                UserGameRoles = gameRoles.List;
            }
            SelectedUserGameRole = UserGameRoles?.FirstOrDefault();
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

        #region NumberBoxDeleteButtonRemover
        private void NumberBoxValueChanged(ModernWpf.Controls.NumberBox sender, ModernWpf.Controls.NumberBoxValueChangedEventArgs args)
        {
            List<Button> buttons = new();
            FindChildren(sender, buttons);
            if (buttons.Count > 0)
            {
                ((Grid)buttons[0].Parent).Children.Remove(buttons[0]);
            }
        }

        private void NumberBoxGotFocus(object sender, RoutedEventArgs e)
        {
            List<Button> buttons = new();
            FindChildren((ModernWpf.Controls.NumberBox)sender, buttons);
            if (buttons.Count > 0)
            {
                ((Grid)buttons[0].Parent).Children.Remove(buttons[0]);
            }
        }

        internal static void FindChildren<T>(DependencyObject parent, List<T> results) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject current = VisualTreeHelper.GetChild(parent, i);
                if ((current.GetType()).Equals(typeof(T)) /*|| (current.GetType().GetTypeInfo().IsSubclassOf(typeof(T)))*/)
                {
                    T asType = (T)current;
                    results.Add(asType);
                }
                FindChildren<T>(current, results);
            }
        }
        #endregion
    }
}
