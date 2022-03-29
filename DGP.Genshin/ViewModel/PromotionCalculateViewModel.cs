using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DGP.Genshin.Control.Infrastructure.Concurrent;
using DGP.Genshin.DataModel.Promotion;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.MiHoYoAPI.Calculation;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.Service.Abstraction;
using Microsoft.VisualStudio.Threading;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Extenion.Enumerable;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    /// <summary>
    /// 养成计算视图模型
    /// </summary>
    [ViewModel(InjectAs.Transient)]
    internal class PromotionCalculateViewModel : ObservableRecipient2, ISupportCancellation
    {
        private const string AvatarTag = "Avatar";
        private const string SkillTag = "Skill";

        private readonly IMaterialListService materialListService;

        private readonly Calculator calculator;
        private readonly UserGameRoleProvider userGameRoleProvider;

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

        /// <summary>
        /// 构造一个新的养成计算视图模型
        /// </summary>
        /// <param name="materialListService">材料清单服务</param>
        /// <param name="cookieService">cookie服务</param>
        /// <param name="asyncRelayCommandFactory">异步命令工厂</param>
        /// <param name="messenger">消息器</param>
        public PromotionCalculateViewModel(
            IMaterialListService materialListService,
            ICookieService cookieService,
            IAsyncRelayCommandFactory asyncRelayCommandFactory,
            IMessenger messenger)
            : base(messenger)
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

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// 玩家集合
        /// </summary>
        public IEnumerable<UserGameRole>? UserGameRoles
        {
            get => this.userGameRoles;

            set => this.SetProperty(ref this.userGameRoles, value);
        }

        /// <summary>
        /// 当前选中的玩家
        /// </summary>
        public UserGameRole? SelectedUserGameRole
        {
            get => this.selectedUserGameRole;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.selectedUserGameRole, value, this.UpdateAvatarListAsync);
        }

        /// <summary>
        /// 角色列表
        /// </summary>
        public IEnumerable<Avatar>? Avatars
        {
            get => this.avatars;

            set => this.SetProperty(ref this.avatars, value);
        }

        /// <summary>
        /// 选中的角色
        /// </summary>
        public Avatar? SelectedAvatar
        {
            get => this.selectedAvatar;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.selectedAvatar, value, this.UpdateAvatarDetailDataAsync);
        }

        /// <summary>
        /// 角色详细数据
        /// </summary>
        public AvatarDetailData? AvatarDetailData
        {
            get => this.avatarDetailData;

            set => this.SetProperty(ref this.avatarDetailData, value);
        }

        /// <summary>
        /// 材料消耗
        /// </summary>
        public Consumption? Consumption
        {
            get => this.consumption;

            set => this.SetProperty(ref this.consumption, value);
        }

        /// <summary>
        /// 打开界面时触发的命令
        /// </summary>
        public ICommand OpenUICommand { get; }

        /// <summary>
        /// 计算命令
        /// </summary>
        public ICommand ComputeCommand { get; }

        /// <summary>
        /// 全角色列表
        /// </summary>
        public List<Avatar>? FullAvatars
        {
            get => this.fullAvatars;
            set => this.SetProperty(ref this.fullAvatars, value);
        }

        /// <summary>
        /// 选中的全角色
        /// </summary>
        public Avatar? SelectedFullAvatar
        {
            get => this.selectedFullAvatar;
            set => this.SetPropertyAndCallbackOnCompletion(ref this.selectedFullAvatar, value, this.UpdateFullAvatarDetailData);
        }

        /// <summary>
        /// 全角色详细数据
        /// </summary>
        public AvatarDetailData? FullAvatarDetailData
        {
            get => this.fullAvatarDetailData;
            set => this.SetProperty(ref this.fullAvatarDetailData, value);
        }

        /// <summary>
        /// 全角色消耗
        /// </summary>
        public Consumption? FullAvatarConsumption
        {
            get => this.fullAvatarConsumption;
            set => this.SetProperty(ref this.fullAvatarConsumption, value);
        }

        /// <summary>
        /// 全角色计算命令
        /// </summary>
        public ICommand FullAvatarComputeCommand { get; }

        /// <summary>
        /// 全角色材料添加命令
        /// </summary>
        public ICommand AddFullCharacterMaterialCommand { get; }

        /// <summary>
        /// 全武器列表
        /// </summary>
        public List<Weapon>? FullWeapons
        {
            get => this.fullWeapons;
            set => this.SetProperty(ref this.fullWeapons, value);
        }

        /// <summary>
        /// 选中的全武器
        /// </summary>
        public Weapon? SelectedFullWeapon
        {
            get => this.selectedFullWeapon;
            set => this.SetPropertyAndCallbackOnCompletion(ref this.selectedFullWeapon, value, this.UpdateFullWeaponAvatarDetailData);
        }

        /// <summary>
        /// 全武器详细数据
        /// </summary>
        public AvatarDetailData? FullWeaponAvatarDetailData
        {
            get => this.fullWeaponAvatarDetailData;
            set => this.SetProperty(ref this.fullWeaponAvatarDetailData, value);
        }

        /// <summary>
        /// 全武器消耗
        /// </summary>
        public Consumption? FullWeaponConsumption
        {
            get => this.fullWeaponConsumption;
            set => this.SetProperty(ref this.fullWeaponConsumption, value);
        }

        /// <summary>
        /// 全武器计算命令
        /// </summary>
        public ICommand FullWeaponComputeCommand { get; }

        /// <summary>
        /// 全武器添加材料命令
        /// </summary>
        public ICommand AddFullWeaponMaterialCommand { get; }

        /// <summary>
        /// 材料清单是否为空
        /// </summary>
        public bool IsListEmpty { get => this.isListEmpty; set => this.SetProperty(ref this.isListEmpty, value); }

        /// <summary>
        /// 材料清单
        /// </summary>
        public MaterialList? MaterialList
        {
            get => this.materialList;

            set => this.SetProperty(ref this.materialList, value);
        }

        /// <summary>
        /// 添加角色材料命令
        /// </summary>
        public ICommand AddCharacterMaterialCommand { get; }

        /// <summary>
        /// 添加武器材料命令
        /// </summary>
        public ICommand AddWeaponMaterialCommand { get; }

        /// <summary>
        /// 移除材料命令
        /// </summary>
        public ICommand RemoveMaterialCommand { get; }

        /// <summary>
        /// 退出界面时触发的命令
        /// </summary>
        public ICommand CloseUICommand { get; }

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
            catch (TaskCanceledException)
            {
                this.Log("Open UI cancelled");
            }
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
                        ReliquaryList = this.AvatarDetailData.ReliquaryList?.Select(r => r.ToPromotionDelta()),
                    };
                    this.Consumption = await this.calculator.ComputeAsync(delta, this.CancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                this.Log("ComputeAsync canceled by user switch page");
            }
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
            catch (TaskCanceledException)
            {
                this.Log("ComputerFullAvatarAsync canceled by user switch page");
            }
        }

        private async Task ComputerFullWeaponAsync()
        {
            try
            {
                if (this.FullWeaponAvatarDetailData is not null)
                {
                    AvatarPromotionDelta delta = new()
                    {
                        Weapon = this.FullWeaponAvatarDetailData.Weapon?.ToPromotionDelta(),
                    };
                    this.FullWeaponConsumption = await this.calculator.ComputeAsync(delta, this.CancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                this.Log("ComputerFullWeaponAsync canceled by user switch page");
            }
        }

        private async Task AddCharacterMaterialToListAsync(string? option)
        {
            Requires.NotNull(this.SelectedAvatar!, nameof(this.SelectedAvatar));
            Calculable calculable = this.SelectedAvatar;
            List<ConsumeItem> items = option switch
            {
                AvatarTag => Requires.NotNull(this.Consumption?.AvatarConsume!, nameof(this.Consumption.AvatarConsume)),
                SkillTag => Requires.NotNull(this.Consumption?.AvatarSkillConsume!, nameof(this.Consumption.AvatarSkillConsume)),
                _ => throw Assumes.NotReachable(),
            };

            string category = option switch
            {
                AvatarTag => "角色消耗",
                SkillTag => "天赋消耗",
                _ => throw Assumes.NotReachable(),
            };

            if (await this.ConfirmAddAsync(calculable.Name!, category))
            {
                this.MaterialList?.Add(new(calculable, items) { RemoveCommand = this.RemoveMaterialCommand });
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
                _ => throw Assumes.NotReachable(),
            };

            string category = option switch
            {
                AvatarTag => "角色消耗",
                SkillTag => "天赋消耗",
                _ => throw Assumes.NotReachable(),
            };

            if (await this.ConfirmAddAsync(calculable.Name!, category))
            {
                this.MaterialList?.Add(new(calculable, items) { RemoveCommand = this.RemoveMaterialCommand });
                this.IsListEmpty = this.MaterialList.IsEmpty();
            }
        }

        private async Task AddWeaponMaterialToListAsync()
        {
            Calculable calculable = Requires.NotNull(this.AvatarDetailData?.Weapon!, nameof(this.AvatarDetailData.Weapon));
            List<ConsumeItem> items = Requires.NotNull(this.Consumption?.WeaponConsume!, nameof(this.Consumption.WeaponConsume));

            if (await this.ConfirmAddAsync(calculable.Name!, "武器消耗"))
            {
                this.MaterialList?.Add(new(calculable, items) { RemoveCommand = this.RemoveMaterialCommand });
                this.IsListEmpty = this.MaterialList.IsEmpty();
            }
        }

        private async Task AddFullWeaponMaterialToListAsync()
        {
            Calculable calculable = Requires.NotNull(this.FullWeaponAvatarDetailData?.Weapon!, nameof(this.AvatarDetailData.Weapon));
            List<ConsumeItem> items = Requires.NotNull(this.FullWeaponConsumption?.WeaponConsume!, nameof(this.Consumption.WeaponConsume));

            if (await this.ConfirmAddAsync(calculable.Name!, "武器消耗"))
            {
                this.MaterialList?.Add(new(calculable, items) { RemoveCommand = this.RemoveMaterialCommand });
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
                DefaultButton = ContentDialogButton.Close,
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
                DefaultButton = ContentDialogButton.Primary,
            }.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        [PropertyChangedCallback]
        private async Task UpdateAvatarListAsync(UserGameRole? selected)
        {
            try
            {
                if (selected is not null)
                {
                    Requires.NotNull(selected.GameUid!, nameof(selected.GameUid));
                    Requires.NotNull(selected.Region!, nameof(selected.Region));
                    List<Avatar> avatars = await this.calculator.GetSyncedAvatarListAsync(
                        new(selected.GameUid, selected.Region),
                        true,
                        this.CancellationToken);
                    int index = avatars.FindIndex(x => x.Name == "旅行者");
                    if (avatars.ExistsIndex(index))
                    {
                        avatars.RemoveAt(index);
                    }

                    this.Avatars = avatars;
                    this.SelectedAvatar = this.Avatars?.FirstOrDefault();
                }
            }
            catch (TaskCanceledException)
            {
                this.Log("UpdateAvatarList canceled by user switch page");
            }
        }

        [PropertyChangedCallback]
        private async Task UpdateAvatarDetailDataAsync(Avatar? avatar)
        {
            try
            {
                this.Consumption = null;
                Requires.NotNull(this.SelectedUserGameRole!, nameof(this.SelectedUserGameRole));

                if (avatar is not null)
                {
                    string? uid = this.SelectedUserGameRole.GameUid;
                    string? region = this.SelectedUserGameRole.Region;

                    Requires.NotNull(uid!, nameof(uid));
                    Requires.NotNull(region!, nameof(region));

                    int avatarId = avatar.Id;

                    this.AvatarDetailData = await this.calculator.GetSyncedAvatarDetailDataAsync(avatarId, uid, region, this.CancellationToken);

                    avatar.LevelTarget = avatar.LevelCurrent;
                    this.AvatarDetailData?.SkillList?.ForEach(x => x.LevelTarget = x.LevelCurrent);
                    if (this.AvatarDetailData?.Weapon is not null)
                    {
                        this.AvatarDetailData.Weapon.LevelTarget = this.AvatarDetailData.Weapon.LevelCurrent;
                    }

                    this.AvatarDetailData?.ReliquaryList?.ForEach(x => x.LevelTarget = x.LevelCurrent);
                }
            }
            catch (TaskCanceledException)
            {
                this.Log("UpdateAvatarDetailData canceled");
            }
        }

        [PropertyChangedCallback]
        [SuppressMessage("", "VSTHRD100")]
        private async void UpdateFullAvatarDetailData(Avatar? avatar)
        {
            try
            {
                this.FullAvatarConsumption = null;
                if (avatar is not null)
                {
                    this.FullAvatarDetailData = new()
                    {
                        SkillList = await this.calculator.GetAvatarSkillListAsync(avatar, this.CancellationToken),
                    };
                    avatar.LevelTarget = 90;
                    this.FullAvatarDetailData.SkillList.ForEach(x => x.LevelCurrent = 1);
                    this.FullAvatarDetailData.SkillList.ForEach(x => x.LevelTarget = 10);
                }
            }
            catch (TaskCanceledException)
            {
                this.Log("UpdateFullAvatarDetailData canceled");
            }
        }

        [PropertyChangedCallback]
        private void UpdateFullWeaponAvatarDetailData()
        {
            this.FullWeaponConsumption = null;
            this.FullWeaponAvatarDetailData = new() { Weapon = this.SelectedFullWeapon };

            if (this.SelectedFullWeapon is not null && this.FullWeaponAvatarDetailData?.Weapon is not null)
            {
                this.FullWeaponAvatarDetailData.Weapon.LevelCurrent = 1;
                this.FullWeaponAvatarDetailData.Weapon.LevelTarget = this.SelectedFullWeapon.MaxLevel;
            }
        }
    }
}