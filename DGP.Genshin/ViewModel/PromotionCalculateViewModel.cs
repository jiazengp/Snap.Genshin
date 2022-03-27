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
            get => this.userGameRoles;

            set => this.SetProperty(ref this.userGameRoles, value);
        }
        public UserGameRole? SelectedUserGameRole
        {
            get => this.selectedUserGameRole;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.selectedUserGameRole, value, this.UpdateAvatarList);
        }
        [PropertyChangedCallback, SuppressMessage("", "VSTHRD100")]
        private async void UpdateAvatarList()
        {
            try
            {
                if (this.SelectedUserGameRole is UserGameRole selected)
                {
                    Requires.NotNull(selected.GameUid!, nameof(selected.GameUid));
                    Requires.NotNull(selected.Region!, nameof(selected.Region));
                    List<Avatar> avatars = await this.calculator.GetSyncedAvatarListAsync(new(selected.GameUid, selected.Region), true, this.CancellationToken);
                    int index = avatars.FindIndex(x => x.Name == "旅行者");
                    if (avatars.ExistsIndex(index))
                    {
                        avatars.RemoveAt(index);
                    }

                    this.Avatars = avatars;
                    this.SelectedAvatar = this.Avatars?.FirstOrDefault();
                }
            }
            catch (TaskCanceledException) { this.Log("UpdateAvatarList canceled by user switch page"); }
        }
        public IEnumerable<Avatar>? Avatars
        {
            get => this.avatars;

            set => this.SetProperty(ref this.avatars, value);
        }
        public Avatar? SelectedAvatar
        {
            get => this.selectedAvatar;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.selectedAvatar, value, this.UpdateAvatarDetailData);
        }
        [PropertyChangedCallback, SuppressMessage("", "VSTHRD100")]
        private async void UpdateAvatarDetailData()
        {
            try
            {
                this.Consumption = null;
                if (this.SelectedUserGameRole is not null && this.SelectedAvatar is not null)
                {
                    string? uid = this.SelectedUserGameRole.GameUid;
                    string? region = this.SelectedUserGameRole.Region;

                    Requires.NotNull(uid!, nameof(uid));
                    Requires.NotNull(region!, nameof(region));

                    int avatarId = this.SelectedAvatar.Id;

                    this.AvatarDetailData = await this.calculator.GetSyncedAvatarDetailDataAsync(avatarId, uid, region, this.CancellationToken);
                }
                if (this.SelectedAvatar is not null)
                {
                    this.SelectedAvatar.LevelTarget = this.SelectedAvatar.LevelCurrent;
                    this.AvatarDetailData?.SkillList?.ForEach(x => x.LevelTarget = x.LevelCurrent);
                    if (this.AvatarDetailData?.Weapon is not null)
                    {
                        this.AvatarDetailData.Weapon.LevelTarget = this.AvatarDetailData.Weapon.LevelCurrent;
                    }
                    this.AvatarDetailData?.ReliquaryList?.ForEach(x => x.LevelTarget = x.LevelCurrent);
                }
            }
            catch (TaskCanceledException) { this.Log("UpdateAvatarDetailData canceled"); }
        }
        public AvatarDetailData? AvatarDetailData
        {
            get => this.avatarDetailData;

            set => this.SetProperty(ref this.avatarDetailData, value);
        }
        public Consumption? Consumption
        {
            get => this.consumption;

            set => this.SetProperty(ref this.consumption, value);
        }

        public ICommand OpenUICommand { get; }
        public ICommand ComputeCommand { get; }
        #endregion

        #region Full Characters & Weapons
        public List<Avatar>? FullAvatars { get => this.fullAvatars; set => this.SetProperty(ref this.fullAvatars, value); }
        public Avatar? SelectedFullAvatar
        {
            get => this.selectedFullAvatar;
            set => this.SetPropertyAndCallbackOnCompletion(ref this.selectedFullAvatar, value, this.UpdateFullAvatarDetailData);
        }
        [PropertyChangedCallback, SuppressMessage("", "VSTHRD100")]
        private async void UpdateFullAvatarDetailData()
        {
            try
            {
                this.FullAvatarConsumption = null;
                if (this.SelectedFullAvatar is not null)
                {
                    this.FullAvatarDetailData = new()
                    {
                        SkillList = await this.calculator.GetAvatarSkillListAsync(this.SelectedFullAvatar, this.CancellationToken)
                    };
                    this.SelectedFullAvatar.LevelTarget = 90;
                    this.FullAvatarDetailData.SkillList.ForEach(x => x.LevelCurrent = 1);
                    this.FullAvatarDetailData.SkillList.ForEach(x => x.LevelTarget = 10);
                }
            }
            catch (TaskCanceledException) { this.Log("UpdateFullAvatarDetailData canceled"); }
        }
        public AvatarDetailData? FullAvatarDetailData { get => this.fullAvatarDetailData; set => this.SetProperty(ref this.fullAvatarDetailData, value); }
        public Consumption? FullAvatarConsumption { get => this.fullAvatarConsumption; set => this.SetProperty(ref this.fullAvatarConsumption, value); }
        public ICommand FullAvatarComputeCommand { get; }
        public ICommand AddFullCharacterMaterialCommand { get; }

        public List<Weapon>? FullWeapons { get => this.fullWeapons; set => this.SetProperty(ref this.fullWeapons, value); }
        public Weapon? SelectedFullWeapon
        {
            get => this.selectedFullWeapon;
            set => this.SetPropertyAndCallbackOnCompletion(ref this.selectedFullWeapon, value, this.UpdateFullWeaponAvatarDetailData);
        }
        [PropertyChangedCallback]
        private void UpdateFullWeaponAvatarDetailData()
        {
            this.FullWeaponConsumption = null;
            this.FullWeaponAvatarDetailData = new() { Weapon = SelectedFullWeapon };

            if (this.SelectedFullWeapon is not null && this.FullWeaponAvatarDetailData?.Weapon is not null)
            {
                this.FullWeaponAvatarDetailData.Weapon.LevelCurrent = 1;
                this.FullWeaponAvatarDetailData.Weapon.LevelTarget = this.SelectedFullWeapon.MaxLevel;
            }
        }
        public AvatarDetailData? FullWeaponAvatarDetailData { get => this.fullWeaponAvatarDetailData; set => this.SetProperty(ref this.fullWeaponAvatarDetailData, value); }
        public Consumption? FullWeaponConsumption { get => this.fullWeaponConsumption; set => this.SetProperty(ref this.fullWeaponConsumption, value); }
        public ICommand FullWeaponComputeCommand { get; }
        public ICommand AddFullWeaponMaterialCommand { get; }
        #endregion

        #region MaterialList
        public bool IsListEmpty { get => this.isListEmpty; set => this.SetProperty(ref this.isListEmpty, value); }
        public MaterialList? MaterialList
        {
            get => this.materialList;

            set => this.SetProperty(ref this.materialList, value);
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

            this.calculator = new(cookieService.CurrentCookie);
            this.userGameRoleProvider = new(cookieService.CurrentCookie);

            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
            this.CloseUICommand = new RelayCommand(this.CloseUI);

            this.ComputeCommand = asyncRelayCommandFactory.Create(this.ComputeAsync);
            this.FullAvatarComputeCommand = asyncRelayCommandFactory.Create(this.ComputerFullAvatarAsync);
            this.FullWeaponComputeCommand = asyncRelayCommandFactory.Create(this.ComputerFullWeaponAsync);

            this.AddCharacterMaterialCommand = asyncRelayCommandFactory.Create<string>(this.AddCharacterMaterialToListAsync);
            this.AddWeaponMaterialCommand = asyncRelayCommandFactory.Create(this.AddWeaponMaterialToListAsync);

            this.AddFullCharacterMaterialCommand = asyncRelayCommandFactory.Create<string>(this.AddFullCharacterMaterialToListAsync);

            this.AddFullWeaponMaterialCommand = asyncRelayCommandFactory.Create(this.AddFullWeaponMaterialToListAsync);

            this.RemoveMaterialCommand = asyncRelayCommandFactory.Create<CalculableConsume>(this.RemoveMaterialFromListAsync);
        }

        private async Task OpenUIAsync()
        {
            try
            {
                this.MaterialList = this.materialListService.Load();
                this.MaterialList.ForEach(item => item.RemoveCommand = this.RemoveMaterialCommand);

                this.IsListEmpty = this.MaterialList.IsEmpty();

                this.UserGameRoles = await this.userGameRoleProvider.GetUserGameRolesAsync(this.CancellationToken);
                this.SelectedUserGameRole = this.UserGameRoles.FirstOrDefault();

                List<Avatar> avatars = await this.calculator.GetAvatarListAsync(new(), cancellationToken: this.CancellationToken);
                this.FullAvatars = avatars.Where(x => x.Name != "旅行者").ToList();
                this.SelectedFullAvatar = this.FullAvatars.FirstOrDefault();

                this.FullWeapons = await this.calculator.GetWeaponListAsync(new(), cancellationToken: this.CancellationToken);
                this.SelectedFullWeapon = this.FullWeapons.FirstOrDefault();
            }
            catch (TaskCanceledException) { this.Log("Open UI cancelled"); }
        }
        private void CloseUI()
        {
            this.materialListService.Save(this.MaterialList);
        }
        private async Task ComputeAsync()
        {
            try
            {
                if (this.SelectedAvatar is not null && this.AvatarDetailData is not null)
                {
                    AvatarPromotionDelta delta = new()
                    {
                        AvatarId = this.SelectedAvatar.Id,
                        AvatarLevelCurrent = this.SelectedAvatar.LevelCurrent,
                        AvatarLevelTarget = this.SelectedAvatar.LevelTarget,
                        SkillList = this.AvatarDetailData.SkillList?.Select(s => s.ToPromotionDelta()),
                        Weapon = this.AvatarDetailData.Weapon?.ToPromotionDelta(),
                        ReliquaryList = this.AvatarDetailData.ReliquaryList?.Select(r => r.ToPromotionDelta())
                    };
                    this.Consumption = await this.calculator.ComputeAsync(delta, this.CancellationToken);
                }
            }
            catch (TaskCanceledException) { this.Log("ComputeAsync canceled by user switch page"); }
        }
        private async Task ComputerFullAvatarAsync()
        {
            try
            {
                if (this.SelectedFullAvatar is not null && this.FullAvatarDetailData is not null)
                {
                    AvatarPromotionDelta delta = new()
                    {
                        AvatarId = this.SelectedFullAvatar.Id,
                        AvatarLevelCurrent = this.SelectedFullAvatar.LevelCurrent,
                        AvatarLevelTarget = this.SelectedFullAvatar.LevelTarget,
                        SkillList = this.FullAvatarDetailData.SkillList?.Select(s => s.ToPromotionDelta()),
                    };
                    this.FullAvatarConsumption = await this.calculator.ComputeAsync(delta, this.CancellationToken);
                }
            }
            catch (TaskCanceledException) { this.Log("ComputerFullAvatarAsync canceled by user switch page"); }
        }
        private async Task ComputerFullWeaponAsync()
        {
            try
            {
                if (this.FullWeaponAvatarDetailData is not null)
                {
                    AvatarPromotionDelta delta = new()
                    {
                        Weapon = this.FullWeaponAvatarDetailData.Weapon?.ToPromotionDelta()
                    };
                    this.FullWeaponConsumption = await this.calculator.ComputeAsync(delta, this.CancellationToken);
                }
            }
            catch (TaskCanceledException) { this.Log("ComputerFullWeaponAsync canceled by user switch page"); }
        }
        private async Task AddCharacterMaterialToListAsync(string? option)
        {
            Requires.NotNull(this.SelectedAvatar!, nameof(this.SelectedAvatar));
            Calculable calculable = this.SelectedAvatar;
            List<ConsumeItem> items = option switch
            {
                AvatarTag => Requires.NotNull(this.Consumption?.AvatarConsume!, nameof(this.Consumption.AvatarConsume)),
                SkillTag => Requires.NotNull(this.Consumption?.AvatarSkillConsume!, nameof(this.Consumption.AvatarSkillConsume)),
                _ => throw Assumes.NotReachable()
            };

            string category = option switch
            {
                AvatarTag => "角色消耗",
                SkillTag => "天赋消耗",
                _ => throw Assumes.NotReachable()
            };

            if (await this.ConfirmAddAsync(calculable.Name!, category))
            {
                this.MaterialList?.Add(new(calculable, items) { RemoveCommand = RemoveMaterialCommand });
                this.IsListEmpty = this.MaterialList.IsEmpty();
            }
        }
        private async Task AddFullCharacterMaterialToListAsync(string? option)
        {
            Requires.NotNull(this.SelectedFullAvatar!, nameof(this.SelectedFullAvatar));
            Calculable calculable = this.SelectedFullAvatar;
            List<ConsumeItem> items = option switch
            {
                AvatarTag => Requires.NotNull(this.FullAvatarConsumption?.AvatarConsume!, nameof(this.FullAvatarConsumption.AvatarConsume)),
                SkillTag => Requires.NotNull(this.FullAvatarConsumption?.AvatarSkillConsume!, nameof(this.FullAvatarConsumption.AvatarSkillConsume)),
                _ => throw Assumes.NotReachable()
            };

            string category = option switch
            {
                AvatarTag => "角色消耗",
                SkillTag => "天赋消耗",
                _ => throw Assumes.NotReachable()
            };

            if (await this.ConfirmAddAsync(calculable.Name!, category))
            {
                this.MaterialList?.Add(new(calculable, items) { RemoveCommand = RemoveMaterialCommand });
                this.IsListEmpty = this.MaterialList.IsEmpty();
            }
        }
        private async Task AddWeaponMaterialToListAsync()
        {
            Calculable calculable = Requires.NotNull(this.AvatarDetailData?.Weapon!, nameof(this.AvatarDetailData.Weapon));
            List<ConsumeItem> items = Requires.NotNull(this.Consumption?.WeaponConsume!, nameof(this.Consumption.WeaponConsume));

            if (await this.ConfirmAddAsync(calculable.Name!, "武器消耗"))
            {
                this.MaterialList?.Add(new(calculable, items) { RemoveCommand = RemoveMaterialCommand });
                this.IsListEmpty = this.MaterialList.IsEmpty();
            }
        }
        private async Task AddFullWeaponMaterialToListAsync()
        {
            Calculable calculable = Requires.NotNull(this.FullWeaponAvatarDetailData?.Weapon!, nameof(this.AvatarDetailData.Weapon));
            List<ConsumeItem> items = Requires.NotNull(this.FullWeaponConsumption?.WeaponConsume!, nameof(this.Consumption.WeaponConsume));

            if (await this.ConfirmAddAsync(calculable.Name!, "武器消耗"))
            {
                this.MaterialList?.Add(new(calculable, items) { RemoveCommand = RemoveMaterialCommand });
                this.IsListEmpty = this.MaterialList.IsEmpty();
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
                this.MaterialList?.Remove(item);
                this.IsListEmpty = this.MaterialList.IsEmpty();
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
