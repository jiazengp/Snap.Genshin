using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.Calculation;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.Threading;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Exception;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    public class PromotionCalculateViewModel : ObservableRecipient2, IRecipient<CookieChangedMessage>
    {
        private readonly ICookieService cookieService;

        private Calculator calculator;
        private UserGameRoleProvider userGameRoleProvider;

        private IEnumerable<UserGameRole>? userGameRoles;
        private UserGameRole? selectedUserGameRole;
        private IEnumerable<Avatar>? avatars;
        private Avatar? selectedAvatar;
        private AvatarDetailData? avatarDetailData;
        private Consumption? consumption = new();

        public IEnumerable<UserGameRole>? UserGameRoles
        {
            get => userGameRoles;
            set => SetProperty(ref userGameRoles, value);
        }
        public UserGameRole? SelectedUserGameRole
        {
            get => selectedUserGameRole;
            set => SetPropertyAndCallbackOnCompletion(ref selectedUserGameRole, value, UpdateAvatarList);
        }
        [PropertyChangedCallback]
        private async void UpdateAvatarList()
        {
            if (SelectedUserGameRole is UserGameRole selected)
            {
                string uid = selected.GameUid ?? throw new UnexpectedNullException("uid 不应为 null");
                string region = selected.Region ?? throw new UnexpectedNullException("region 不应为 null");
                Avatars = await calculator.GetSyncedAvatarListAsync(new(uid, region), true);
                SelectedAvatar = Avatars?.FirstOrDefault();
            }
        }
        public IEnumerable<Avatar>? Avatars
        {
            get => avatars;
            set => SetProperty(ref avatars, value);
        }
        public Avatar? SelectedAvatar
        {
            get => selectedAvatar;
            set => SetPropertyAndCallbackOnCompletion(ref selectedAvatar, value, UpdateAvatarDetailData);
        }
        [PropertyChangedCallback]
        private async void UpdateAvatarDetailData()
        {
            if (SelectedUserGameRole is not null && SelectedAvatar is not null)
            {
                string uid = SelectedUserGameRole.GameUid ?? throw new UnexpectedNullException("uid 不应为 null");
                string region = SelectedUserGameRole.Region ?? throw new UnexpectedNullException("region 不应为 null");
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
        public AvatarDetailData? AvatarDetailData
        {
            get => avatarDetailData;
            set => SetProperty(ref avatarDetailData, value);
        }
        public Consumption? Consumption
        {
            get => consumption;
            set => SetProperty(ref consumption, value);
        }

        public ICommand OpenUICommand { get; }
        public ICommand ComputeCommand { get; }

        public PromotionCalculateViewModel(ICookieService cookieService, IMessenger messenger) : base(messenger)
        {
            this.cookieService = cookieService;

            calculator = new(cookieService.CurrentCookie);
            userGameRoleProvider = new(cookieService.CurrentCookie);

            OpenUICommand = new AsyncRelayCommand(OpenUIAsync);
            ComputeCommand = new AsyncRelayCommand(ComputeAsync);
        }

        private async Task OpenUIAsync()
        {
            UserGameRoles = await userGameRoleProvider.GetUserGameRolesAsync();
            SelectedUserGameRole = UserGameRoles?.FirstOrDefault();
        }
        private async Task ComputeAsync()
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

        /// <summary>
        /// Cookie 改变
        /// </summary>
        /// <param name="message"></param>
        public void Receive(CookieChangedMessage message)
        {
            UpdateUserGameRolesAsync().Forget();
        }

        private async Task UpdateUserGameRolesAsync()
        {
            calculator = new(cookieService.CurrentCookie);
            userGameRoleProvider = new(cookieService.CurrentCookie);

            UserGameRoles = await userGameRoleProvider.GetUserGameRolesAsync();
            SelectedUserGameRole = UserGameRoles?.FirstOrDefault();
        }
    }
}
