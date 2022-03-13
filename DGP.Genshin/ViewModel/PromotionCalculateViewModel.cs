using DGP.Genshin.DataModel.Promotion;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.Calculation;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.Threading;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using Snap.Exception;
using Snap.Extenion.Enumerable;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    public class PromotionCalculateViewModel : ObservableRecipient2, IRecipient<CookieChangedMessage>
    {
        private const string AvatarTag = "Avatar";
        private const string SkillTag = "Skill";
        private readonly ICookieService cookieService;
        private readonly IMaterialListService materialListService;

        private Calculator calculator;
        private UserGameRoleProvider userGameRoleProvider;

        #region Sync Calculation
        private IEnumerable<UserGameRole>? userGameRoles;
        private UserGameRole? selectedUserGameRole;
        private IEnumerable<Avatar>? avatars;
        private Avatar? selectedAvatar;
        private AvatarDetailData? avatarDetailData;
        private Consumption? consumption = new();
        private MaterialList? materialList;
        private IEnumerable<ConsumeItem>? totalConsumption;

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
        [PropertyChangedCallback, SuppressMessage("", "VSTHRD100")]
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
        [PropertyChangedCallback, SuppressMessage("", "VSTHRD100")]
        private async void UpdateAvatarDetailData()
        {
            Consumption = null;
            if (SelectedUserGameRole is not null && SelectedAvatar is not null)
            {
                string uid = SelectedUserGameRole.GameUid ?? throw new UnexpectedNullException("uid 不应为 null");
                string region = SelectedUserGameRole.Region ?? throw new UnexpectedNullException("region 不应为 null");
                int avatarId = SelectedAvatar.Id;

                AvatarDetailData = await calculator.GetSyncedAvatarDetailDataAsync(avatarId, uid, region);
            }
            if (SelectedAvatar is not null)
            {
                SelectedAvatar.LevelTarget = SelectedAvatar.LevelCurrent;
                AvatarDetailData?.SkillList?.ForEach(x => x.LevelTarget = x.LevelCurrent);
                if (AvatarDetailData?.Weapon is not null)
                {
                    AvatarDetailData.Weapon.LevelTarget = AvatarDetailData.Weapon.LevelCurrent;
                }
                AvatarDetailData?.ReliquaryList?.ForEach(x => x.LevelTarget = x.LevelCurrent);
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
        #endregion

        #region MaterialList
        public MaterialList? MaterialList
        {
            get => materialList;

            set => SetProperty(ref materialList, value);
        }

        public IEnumerable<ConsumeItem>? TotalConsumption
        {
            get => totalConsumption;

            set => SetProperty(ref totalConsumption, value);
        }

        public ICommand AddCharacterMaterialCommand { get; }
        public ICommand AddWeaponMaterialCommand { get; }
        public ICommand RemoveMaterialCommand { get; }
        public ICommand CloseCommand { get; }
        #endregion

        public PromotionCalculateViewModel(IMaterialListService materialListService, ICookieService cookieService, IAsyncRelayCommandFactory asyncRelayCommandFactory, IMessenger messenger) : base(messenger)
        {
            this.cookieService = cookieService;
            this.materialListService = materialListService;

            calculator = new(cookieService.CurrentCookie);
            userGameRoleProvider = new(cookieService.CurrentCookie);

            OpenUICommand = asyncRelayCommandFactory.Create(OpenUIAsync);
            CloseCommand = new RelayCommand(CloseUI);
            ComputeCommand = asyncRelayCommandFactory.Create(ComputeAsync);

            AddCharacterMaterialCommand = asyncRelayCommandFactory.Create<string>(AddCharacterMaterialToListAsync);
            AddWeaponMaterialCommand = asyncRelayCommandFactory.Create(AddWeaponMaterialToListAsync);

            RemoveMaterialCommand = asyncRelayCommandFactory.Create<CalculableConsume>(RemoveMaterialFromListAsync);
        }

        private async Task OpenUIAsync()
        {
            MaterialList = materialListService.Load();
            MaterialList.ForEach(item => item.RemoveCommand = RemoveMaterialCommand);
            TotalConsumption = materialListService.GetTotalConsumption(MaterialList);

            UserGameRoles = await userGameRoleProvider.GetUserGameRolesAsync();
            SelectedUserGameRole = UserGameRoles?.FirstOrDefault();
        }
        private void CloseUI()
        {
            materialListService.Save(MaterialList);
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
            SelectedUserGameRole = UserGameRoles?.MatchedOrFirst(r => r.IsChosen);
        }

        private async Task AddCharacterMaterialToListAsync(string? option)
        {
            Calculable calculable = SelectedAvatar ?? throw new SnapGenshinInternalException($"{nameof(SelectedAvatar)} 不应为 null");
            List<ConsumeItem> items = option switch
            {
                AvatarTag => Consumption?.AvatarConsume ?? throw new SnapGenshinInternalException($"{nameof(Consumption.AvatarConsume)} 不应为 null"),
                SkillTag => Consumption?.AvatarSkillConsume ?? throw new SnapGenshinInternalException($"{nameof(Consumption.AvatarSkillConsume)} 不应为 null"),
                _ => throw new SnapGenshinInternalException($"{nameof(option)} 参数不正确")
            };

            string category = option switch
            {
                AvatarTag => "角色消耗",
                SkillTag => "天赋消耗",
                _ => throw new SnapGenshinInternalException($"{nameof(option)} 参数不正确")
            };

            if (await ConfirmAddAsync(calculable.Name!, category))
            {
                MaterialList?.Add(new(calculable, items) { RemoveCommand = RemoveMaterialCommand });
                TotalConsumption = materialListService.GetTotalConsumption(MaterialList);
            }
        }
        private async Task AddWeaponMaterialToListAsync()
        {
            Calculable calculable = AvatarDetailData?.Weapon ?? throw new SnapGenshinInternalException($"{nameof(AvatarDetailData.Weapon)} 不应为 null");
            List<ConsumeItem> items = Consumption?.WeaponConsume ?? throw new SnapGenshinInternalException($"{nameof(Consumption.WeaponConsume)} 不应为 null");

            if (await ConfirmAddAsync(calculable.Name!, "武器消耗"))
            {
                MaterialList?.Add(new(calculable, items) { RemoveCommand = RemoveMaterialCommand });
                TotalConsumption = materialListService.GetTotalConsumption(MaterialList);
            }
        }
        private async Task RemoveMaterialFromListAsync(CalculableConsume? item)
        {
            ContentDialogResult result = await new ContentDialog()
            {
                Title = $"从清单中删除？",
                Content = "该操作不可撤销",
                PrimaryButtonText = "确认",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Close
            }.ShowAsync();

            if (result == ContentDialogResult.Primary && item is not null)
            {
                MaterialList?.Remove(item);
            }
        }

        private async Task<bool> ConfirmAddAsync(string name, string category)
        {
            ContentDialogResult result = await new ContentDialog()
            {
                Title = $"添加 {name} 的 {category} 到清单？",
                Content = "稍后可以切换到材料清单查看",
                PrimaryButtonText = "确认",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
            return result == ContentDialogResult.Primary;
        }
    }
}
