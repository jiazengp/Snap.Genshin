using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Exceptions;
using DGP.Genshin.Messages;
using DGP.Genshin.MiHoYoAPI.Calculation;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(ViewModelType.Transient)]
    public class PromotionCalculateViewModel : ObservableRecipient, IRecipient<CookieChangedMessage>
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
        private IAsyncRelayCommand openUICommand;
        private IAsyncRelayCommand computeCommand;

        public IEnumerable<UserGameRole>? UserGameRoles
        {
            get => userGameRoles;
            set => SetProperty(ref userGameRoles, value);
        }
        public UserGameRole? SelectedUserGameRole
        {
            get => selectedUserGameRole;
            set
            {
                SetProperty(ref selectedUserGameRole, value);
                UpdateAvatarListAsync();
            }
        }
        private async void UpdateAvatarListAsync()
        {
            if (SelectedUserGameRole is not null)
            {
                string uid = SelectedUserGameRole.GameUid ?? throw new UnexceptedNullException("uid 不应为 null");
                string region = SelectedUserGameRole.Region ?? throw new UnexceptedNullException("region 不应为 null");
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
            set
            {
                SetProperty(ref selectedAvatar, value);
                UpdateAvatarDetailDataAsync();
            }
        }
        private async void UpdateAvatarDetailDataAsync()
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
        public IAsyncRelayCommand OpenUICommand
        {
            get => openUICommand;
            [MemberNotNull(nameof(openUICommand))]
            set => SetProperty(ref openUICommand, value);
        }
        public IAsyncRelayCommand ComputeCommand
        {
            get => computeCommand;
            [MemberNotNull(nameof(computeCommand))]
            set => SetProperty(ref computeCommand, value);
        }

        public PromotionCalculateViewModel(ICookieService cookieService, IMessenger messenger) : base(messenger)
        {
            this.cookieService = cookieService;

            calculator = new(cookieService.CurrentCookie);
            userGameRoleProvider = new(cookieService.CurrentCookie);

            OpenUICommand = new AsyncRelayCommand(OpenUIAsync);
            ComputeCommand = new AsyncRelayCommand(ComputeAsync);

            IsActive = true;
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
        public async void Receive(CookieChangedMessage message)
        {
            calculator = new(cookieService.CurrentCookie);
            userGameRoleProvider = new(cookieService.CurrentCookie);

            UserGameRoles = await userGameRoleProvider.GetUserGameRolesAsync();
            SelectedUserGameRole = UserGameRoles?.FirstOrDefault();
        }
    }
}
