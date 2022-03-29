using CommunityToolkit.Mvvm.ComponentModel;
using DGP.Genshin.Control.GenshinElement.HutaoStatistic;
using DGP.Genshin.Control.Infrastructure.Concurrent;
using DGP.Genshin.DataModel.HutaoAPI;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Helper.Extension;
using DGP.Genshin.HutaoAPI.GetModel;
using DGP.Genshin.HutaoAPI.PostModel;
using DGP.Genshin.MiHoYoAPI.Response;
using DGP.Genshin.Service.Abstraction;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Data.Primitive;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    /// <summary>
    /// 胡桃数据库视图模型
    /// </summary>
    [ViewModel(InjectAs.Transient)]
    internal class HutaoStatisticViewModel : ObservableObject, ISupportCancellation
    {
        private readonly ICookieService cookieService;
        private readonly IHutaoStatisticService hutaoStatisticService;

        private bool shouldUIPresent;
        private Overview? overview;
        private IList<Indexed<int, Item<double>>>? avatarParticipations;
        private IList<Item<IList<NamedValue<Rate<IList<Item<int>>>>>>>? avatarReliquaryUsages;
        private IList<Item<IList<Item<double>>>>? teamCollocations;
        private IList<Item<IList<Item<double>>>>? weaponUsages;
        private IList<Rate<Item<IList<NamedValue<double>>>>>? avatarConstellations;
        private IList<Indexed<string, Rate<Two<IList<HutaoItem>>>>>? teamCombinations;

        /// <summary>
        /// 构造一个新的胡桃数据库视图模型
        /// </summary>
        /// <param name="cookieService">cookie服务</param>
        /// <param name="hutaoStatisticService">胡桃数据库服务</param>
        /// <param name="asyncRelayCommandFactory">异步命令工厂</param>
        public HutaoStatisticViewModel(ICookieService cookieService, IHutaoStatisticService hutaoStatisticService, IAsyncRelayCommandFactory asyncRelayCommandFactory)
        {
            this.cookieService = cookieService;
            this.hutaoStatisticService = hutaoStatisticService;

            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
            this.UploadCommand = asyncRelayCommandFactory.Create(this.UploadRecordsAsync);
        }

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// 指示数据界面是否可见
        /// </summary>
        public bool ShouldUIPresent
        {
            get => this.shouldUIPresent;

            set => this.SetProperty(ref this.shouldUIPresent, value);
        }

        /// <summary>
        /// 玩家计数总览
        /// </summary>
        public Overview? Overview
        {
            get => this.overview;

            set => this.SetProperty(ref this.overview, value);
        }

        /// <summary>
        /// 角色出场率
        /// </summary>
        public IList<Indexed<int, Item<double>>>? AvatarParticipations
        {
            get => this.avatarParticipations;

            set => this.SetProperty(ref this.avatarParticipations, value);
        }

        /// <summary>
        /// 角色圣遗物搭配
        /// </summary>
        public IList<Item<IList<NamedValue<Rate<IList<Item<int>>>>>>>? AvatarReliquaryUsages
        {
            get => this.avatarReliquaryUsages;

            set => this.SetProperty(ref this.avatarReliquaryUsages, value);
        }

        /// <summary>
        /// 角色搭配
        /// </summary>
        public IList<Item<IList<Item<double>>>>? TeamCollocations
        {
            get => this.teamCollocations;

            set => this.SetProperty(ref this.teamCollocations, value);
        }

        /// <summary>
        /// 武器使用
        /// </summary>
        public IList<Item<IList<Item<double>>>>? WeaponUsages
        {
            get => this.weaponUsages;

            set => this.SetProperty(ref this.weaponUsages, value);
        }

        /// <summary>
        /// 角色命座
        /// </summary>
        public IList<Rate<Item<IList<NamedValue<double>>>>>? AvatarConstellations
        {
            get => this.avatarConstellations;

            set => this.SetProperty(ref this.avatarConstellations, value);
        }

        /// <summary>
        /// 队伍出场
        /// </summary>
        public IList<Indexed<string, Rate<Two<IList<HutaoItem>>>>>? TeamCombinations
        {
            get => this.teamCombinations;

            set => this.SetProperty(ref this.teamCombinations, value);
        }

        /// <summary>
        /// 打开界面触发的命令
        /// </summary>
        public ICommand OpenUICommand { get; }

        /// <summary>
        /// 上传数据命令
        /// </summary>
        public ICommand UploadCommand { get; }

        private async Task OpenUIAsync()
        {
            try
            {
                await this.hutaoStatisticService.InitializeAsync(this.CancellationToken);

                this.Overview = await this.hutaoStatisticService.GetOverviewAsync(this.CancellationToken);

                // V1
                this.AvatarParticipations = this.hutaoStatisticService.GetAvatarParticipations();
                this.TeamCollocations = this.hutaoStatisticService.GetTeamCollocations();
                this.WeaponUsages = this.hutaoStatisticService.GetWeaponUsages();

                // V2
                this.AvatarReliquaryUsages = this.hutaoStatisticService.GetReliquaryUsages();
                this.AvatarConstellations = this.hutaoStatisticService.GetAvatarConstellations();
                this.TeamCombinations = this.hutaoStatisticService.GetTeamCombinations();
            }
            catch (TaskCanceledException)
            {
                this.Log("Open UI cancelled");
            }
            catch (Exception e)
            {
                this.Log(e);
                await new ContentDialog()
                {
                    Title = "加载失败",
                    Content = e.Message,
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary,
                }.ShowAsync().ConfigureAwait(false);
                return;
            }

            this.ShouldUIPresent = true;
        }

        private async Task UploadRecordsAsync()
        {
            this.ShouldUIPresent = false;
            try
            {
                await this.hutaoStatisticService.GetAllRecordsAndUploadAsync(this.cookieService.CurrentCookie, this.ConfirmAsync, this.HandleResponseAsync, this.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                this.Log("upload data cancelled by user switch page");
            }
            catch (Exception e)
            {
                this.Log(e);
                await new ContentDialog()
                {
                    Title = "提交记录失败",
                    Content = "发生了致命错误",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary,
                }.ShowAsync();
            }
            finally
            {
                this.ShouldUIPresent = true;
            }
        }

        private async Task<bool> ConfirmAsync(PlayerRecord record)
        {
            return await this.ExecuteOnUIAsync(new UploadDialog(record).ShowAsync) == ContentDialogResult.Primary;
        }

        private async Task HandleResponseAsync(Response resonse)
        {
            await new ContentDialog()
            {
                Title = "提交记录",
                Content = resonse.Message,
                PrimaryButtonText = "确定",
                DefaultButton = ContentDialogButton.Primary,
            }.ShowAsync();
        }
    }
}