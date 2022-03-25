using DGP.Genshin.Control.Infrastructure.Concurrent;
using DGP.Genshin.DataModel.Promotion;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.MiHoYoAPI.Calculation;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.Service.Abstraction;
using Microsoft;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.Threading;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Extenion.Enumerable;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class PromotionCalculateViewModel : ObservableRecipient2, ISupportCancellation
    {
        private const string AvatarTag = "Avatar";
        private const string SkillTag = "Skill";

        private readonly IMaterialListService materialListService;

        private readonly Calculator calculator;
        private readonly UserGameRoleProvider userGameRoleProvider;

        public CancellationToken CancellationToken { get; set; }

        #region Sync Calculation
        private IEnumerable<UserGameRole>? userGameRoles;
        private UserGameRole? selectedUserGameRole;
        private IEnumerable<Avatar>? avatars;
        private Avatar? selectedAvatar;
        private AvatarDetailData? avatarDetailData;
        private Consumption? consumption = new();
        private MaterialList? materialList;
        private bool isListEmpty;

        private List<Avatar>? fullAvatars;
        private Avatar? selectedFullAvatar;
        private List<Weapon>? fullWeapons;
        private AvatarDetailData? fullAvatarDetailData;
        private Consumption? fullAvatarConsumption;
        private Weapon? selectedFullWeapon;
        private AvatarDetailData? fullWeaponAvatarDetailData;
        private Consumption? fullWeaponConsumption;

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
            try
            {
                if (SelectedUserGameRole is UserGameRole selected)
                {
                    Requires.NotNull(selected.GameUid!, nameof(selected.GameUid));
                    Requires.NotNull(selected.Region!, nameof(selected.Region));
                    List<Avatar> avatars = await calculator.GetSyncedAvatarListAsync(new(selected.GameUid, selected.Region), true, CancellationToken);
                    int index = avatars.FindIndex(x => x.Name == "旅行者");
                    avatars.RemoveAt(index);
                    Avatars = avatars;
                    SelectedAvatar = Avatars?.FirstOrDefault();
                }
            }
            catch (TaskCanceledException) { this.Log("UpdateAvatarList canceled by user switch page"); }
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
            try
            {
                Consumption = null;
                if (SelectedUserGameRole is not null && SelectedAvatar is not null)
                {
                    string? uid = SelectedUserGameRole.GameUid;
                    string? region = SelectedUserGameRole.Region;

                    Requires.NotNull(uid!, nameof(uid));
                    Requires.NotNull(region!, nameof(region));

                    int avatarId = SelectedAvatar.Id;

                    AvatarDetailData = await calculator.GetSyncedAvatarDetailDataAsync(avatarId, uid, region, CancellationToken);
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
            catch (TaskCanceledException) { this.Log("UpdateAvatarDetailData canceled"); }
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

        #region Full Characters & Weapons
        public List<Avatar>? FullAvatars { get => fullAvatars; set => SetProperty(ref fullAvatars, value); }
        public Avatar? SelectedFullAvatar
        {
            get => selectedFullAvatar;
            set => SetPropertyAndCallbackOnCompletion(ref selectedFullAvatar, value, UpdateFullAvatarDetailData);
        }
        [PropertyChangedCallback, SuppressMessage("", "VSTHRD100")]
        private async void UpdateFullAvatarDetailData()
        {
            try
            {
                FullAvatarConsumption = null;
                if (SelectedFullAvatar is not null)
                {
                    FullAvatarDetailData = new()
                    {
                        SkillList = await calculator.GetAvatarSkillListAsync(SelectedFullAvatar, CancellationToken)
                    };
                    SelectedFullAvatar.LevelTarget = 90;
                    FullAvatarDetailData.SkillList.ForEach(x => x.LevelCurrent = 1);
                    FullAvatarDetailData.SkillList.ForEach(x => x.LevelTarget = 10);
                }
            }
            catch (TaskCanceledException) { this.Log("UpdateFullAvatarDetailData canceled"); }
        }
        public AvatarDetailData? FullAvatarDetailData { get => fullAvatarDetailData; set => SetProperty(ref fullAvatarDetailData, value); }
        public Consumption? FullAvatarConsumption { get => fullAvatarConsumption; set => SetProperty(ref fullAvatarConsumption, value); }
        public ICommand FullAvatarComputeCommand { get; }
        public ICommand AddFullCharacterMaterialCommand { get; }

        public List<Weapon>? FullWeapons { get => fullWeapons; set => SetProperty(ref fullWeapons, value); }
        public Weapon? SelectedFullWeapon
        {
            get => selectedFullWeapon;
            set => SetPropertyAndCallbackOnCompletion(ref selectedFullWeapon, value, UpdateFullWeaponAvatarDetailData);
        }
        [PropertyChangedCallback]
        private void UpdateFullWeaponAvatarDetailData()
        {
            FullWeaponConsumption = null;
            FullWeaponAvatarDetailData = new() { Weapon = SelectedFullWeapon };

            if (SelectedFullWeapon is not null && FullWeaponAvatarDetailData?.Weapon is not null)
            {
                FullWeaponAvatarDetailData.Weapon.LevelCurrent = 1;
                FullWeaponAvatarDetailData.Weapon.LevelTarget = SelectedFullWeapon.MaxLevel;
            }
        }
        public AvatarDetailData? FullWeaponAvatarDetailData { get => fullWeaponAvatarDetailData; set => SetProperty(ref fullWeaponAvatarDetailData, value); }
        public Consumption? FullWeaponConsumption { get => fullWeaponConsumption; set => SetProperty(ref fullWeaponConsumption, value); }
        public ICommand FullWeaponComputeCommand { get; }
        public ICommand AddFullWeaponMaterialCommand { get; }
        #endregion

        #region MaterialList
        public bool IsListEmpty { get => isListEmpty; set => SetProperty(ref isListEmpty, value); }
        public MaterialList? MaterialList
        {
            get => materialList;

            set => SetProperty(ref materialList, value);
        }

        public ICommand AddCharacterMaterialCommand { get; }
        public ICommand AddWeaponMaterialCommand { get; }
        public ICommand RemoveMaterialCommand { get; }
        public ICommand CloseUICommand { get; }
        #endregion

        public PromotionCalculateViewModel(
            IMaterialListService materialListService,
            ICookieService cookieService,
            IAsyncRelayCommandFactory asyncRelayCommandFactory,
            IMessenger messenger) : base(messenger)
        {
            this.materialListService = materialListService;

            calculator = new(cookieService.CurrentCookie);
            userGameRoleProvider = new(cookieService.CurrentCookie);

            OpenUICommand = asyncRelayCommandFactory.Create(OpenUIAsync);
            CloseUICommand = new RelayCommand(CloseUI);

            ComputeCommand = asyncRelayCommandFactory.Create(ComputeAsync);
            FullAvatarComputeCommand = asyncRelayCommandFactory.Create(ComputerFullAvatarAsync);
            FullWeaponComputeCommand = asyncRelayCommandFactory.Create(ComputerFullWeaponAsync);

            AddCharacterMaterialCommand = asyncRelayCommandFactory.Create<string>(AddCharacterMaterialToListAsync);
            AddWeaponMaterialCommand = asyncRelayCommandFactory.Create(AddWeaponMaterialToListAsync);

            AddFullCharacterMaterialCommand = asyncRelayCommandFactory.Create<string>(AddFullCharacterMaterialToListAsync);

            AddFullWeaponMaterialCommand = asyncRelayCommandFactory.Create(AddFullWeaponMaterialToListAsync);

            RemoveMaterialCommand = asyncRelayCommandFactory.Create<CalculableConsume>(RemoveMaterialFromListAsync);
        }

        private async Task OpenUIAsync()
        {
            try
            {
                MaterialList = materialListService.Load();
                MaterialList.ForEach(item => item.RemoveCommand = RemoveMaterialCommand);

                IsListEmpty = MaterialList.IsEmpty();

                UserGameRoles = await userGameRoleProvider.GetUserGameRolesAsync(CancellationToken);
                SelectedUserGameRole = UserGameRoles.FirstOrDefault();

                List<Avatar> avatars = await calculator.GetAvatarListAsync(new(), cancellationToken: CancellationToken);
                FullAvatars = avatars.Where(x => x.Name != "旅行者").ToList();
                SelectedFullAvatar = FullAvatars.FirstOrDefault();

                FullWeapons = await calculator.GetWeaponListAsync(new(), cancellationToken: CancellationToken);
                SelectedFullWeapon = FullWeapons.FirstOrDefault();
            }
            catch (TaskCanceledException) { this.Log("Open UI cancelled"); }
        }
        private void CloseUI()
        {
            materialListService.Save(MaterialList);
        }
        private async Task ComputeAsync()
        {
            try
            {
                if (SelectedAvatar is not null && AvatarDetailData is not null)
                {
                    AvatarPromotionDelta delta = new()
                    {
                        AvatarId = SelectedAvatar.Id,
                        AvatarLevelCurrent = SelectedAvatar.LevelCurrent,
                        AvatarLevelTarget = SelectedAvatar.LevelTarget,
                        SkillList = AvatarDetailData.SkillList?.Select(s => s.ToPromotionDelta()),
                        Weapon = AvatarDetailData.Weapon?.ToPromotionDelta(),
                        ReliquaryList = AvatarDetailData.ReliquaryList?.Select(r => r.ToPromotionDelta())
                    };
                    Consumption = await calculator.ComputeAsync(delta, CancellationToken);
                }
            }
            catch (TaskCanceledException) { this.Log("ComputeAsync canceled by user switch page"); }
        }
        private async Task ComputerFullAvatarAsync()
        {
            try
            {
                if (SelectedFullAvatar is not null && FullAvatarDetailData is not null)
                {
                    AvatarPromotionDelta delta = new()
                    {
                        AvatarId = SelectedFullAvatar.Id,
                        AvatarLevelCurrent = SelectedFullAvatar.LevelCurrent,
                        AvatarLevelTarget = SelectedFullAvatar.LevelTarget,
                        SkillList = FullAvatarDetailData.SkillList?.Select(s => s.ToPromotionDelta()),
                    };
                    FullAvatarConsumption = await calculator.ComputeAsync(delta, CancellationToken);
                }
            }
            catch (TaskCanceledException) { this.Log("ComputerFullAvatarAsync canceled by user switch page"); }
        }
        private async Task ComputerFullWeaponAsync()
        {
            try
            {
                if (FullWeaponAvatarDetailData is not null)
                {
                    AvatarPromotionDelta delta = new()
                    {
                        Weapon = FullWeaponAvatarDetailData.Weapon?.ToPromotionDelta()
                    };
                    FullWeaponConsumption = await calculator.ComputeAsync(delta, CancellationToken);
                }
            }
            catch (TaskCanceledException) { this.Log("ComputerFullWeaponAsync canceled by user switch page"); }
        }
        private async Task AddCharacterMaterialToListAsync(string? option)
        {
            Requires.NotNull(SelectedAvatar!, nameof(SelectedAvatar));
            Calculable calculable = SelectedAvatar;
            List<ConsumeItem> items = option switch
            {
                AvatarTag => Requires.NotNull(Consumption?.AvatarConsume!, nameof(Consumption.AvatarConsume)),
                SkillTag => Requires.NotNull(Consumption?.AvatarSkillConsume!, nameof(Consumption.AvatarSkillConsume)),
                _ => throw Assumes.NotReachable()
            };

            string category = option switch
            {
                AvatarTag => "角色消耗",
                SkillTag => "天赋消耗",
                _ => throw Assumes.NotReachable()
            };

            if (await ConfirmAddAsync(calculable.Name!, category))
            {
                MaterialList?.Add(new(calculable, items) { RemoveCommand = RemoveMaterialCommand });
                IsListEmpty = MaterialList.IsEmpty();
            }
        }
        private async Task AddFullCharacterMaterialToListAsync(string? option)
        {
            Requires.NotNull(SelectedFullAvatar!, nameof(SelectedFullAvatar));
            Calculable calculable = SelectedFullAvatar;
            List<ConsumeItem> items = option switch
            {
                AvatarTag => Requires.NotNull(FullAvatarConsumption?.AvatarConsume!, nameof(FullAvatarConsumption.AvatarConsume)),
                SkillTag => Requires.NotNull(FullAvatarConsumption?.AvatarSkillConsume!, nameof(FullAvatarConsumption.AvatarSkillConsume)),
                _ => throw Assumes.NotReachable()
            };

            string category = option switch
            {
                AvatarTag => "角色消耗",
                SkillTag => "天赋消耗",
                _ => throw Assumes.NotReachable()
            };

            if (await ConfirmAddAsync(calculable.Name!, category))
            {
                MaterialList?.Add(new(calculable, items) { RemoveCommand = RemoveMaterialCommand });
                IsListEmpty = MaterialList.IsEmpty();
            }
        }
        private async Task AddWeaponMaterialToListAsync()
        {
            Calculable calculable = Requires.NotNull(AvatarDetailData?.Weapon!, nameof(AvatarDetailData.Weapon));
            List<ConsumeItem> items = Requires.NotNull(Consumption?.WeaponConsume!, nameof(Consumption.WeaponConsume));

            if (await ConfirmAddAsync(calculable.Name!, "武器消耗"))
            {
                MaterialList?.Add(new(calculable, items) { RemoveCommand = RemoveMaterialCommand });
                IsListEmpty = MaterialList.IsEmpty();
            }
        }
        private async Task AddFullWeaponMaterialToListAsync()
        {
            Calculable calculable = Requires.NotNull(FullWeaponAvatarDetailData?.Weapon!, nameof(AvatarDetailData.Weapon));
            List<ConsumeItem> items = Requires.NotNull(FullWeaponConsumption?.WeaponConsume!, nameof(Consumption.WeaponConsume));

            if (await ConfirmAddAsync(calculable.Name!, "武器消耗"))
            {
                MaterialList?.Add(new(calculable, items) { RemoveCommand = RemoveMaterialCommand });
                IsListEmpty = MaterialList.IsEmpty();
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
                IsListEmpty = MaterialList.IsEmpty();
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
