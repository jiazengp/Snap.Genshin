using DGP.Genshin.Control.GenshinElement.HutaoStatistic;
using DGP.Genshin.Control.Infrastructure.Concurrent;
using DGP.Genshin.DataModel.HutaoAPI;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Helper.Extension;
using DGP.Genshin.HutaoAPI.GetModel;
using DGP.Genshin.HutaoAPI.PostModel;
using DGP.Genshin.MiHoYoAPI.Response;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class HutaoStatisticViewModel : ObservableObject, ISupportCancellation
    {
        private readonly ICookieService cookieService;
        private readonly IHutaoStatisticService hutaoStatisticService;

        public CancellationToken CancellationToken { get; set; }

        private bool shouldUIPresent;
        private Overview? overview;
        private IList<Indexed<int, Item<double>>>? avatarParticipations;
        private IList<Item<IList<NamedValue<Rate<IList<Item<int>>>>>>>? avatarReliquaryUsages;
        private IList<Item<IList<Item<double>>>>? teamCollocations;
        private IList<Item<IList<Item<double>>>>? weaponUsages;
        private IList<Rate<Item<IList<NamedValue<double>>>>>? avatarConstellations;
        private IList<Indexed<string, Rate<Two<IList<HutaoItem>>>>>? teamCombinations;

        public bool ShouldUIPresent
        {
            get => this.shouldUIPresent;

            set => this.SetProperty(ref this.shouldUIPresent, value);
        }
        public Overview? Overview
        {
            get => this.overview;

            set => this.SetProperty(ref this.overview, value);
        }
        public IList<Indexed<int, Item<double>>>? AvatarParticipations
        {
            get => this.avatarParticipations;

            set => this.SetProperty(ref this.avatarParticipations, value);
        }
        public IList<Item<IList<NamedValue<Rate<IList<Item<int>>>>>>>? AvatarReliquaryUsages
        {
            get => this.avatarReliquaryUsages;

            set => this.SetProperty(ref this.avatarReliquaryUsages, value);
        }
        public IList<Item<IList<Item<double>>>>? TeamCollocations
        {
            get => this.teamCollocations;

            set => this.SetProperty(ref this.teamCollocations, value);
        }
        public IList<Item<IList<Item<double>>>>? WeaponUsages
        {
            get => this.weaponUsages;

            set => this.SetProperty(ref this.weaponUsages, value);
        }
        public IList<Rate<Item<IList<NamedValue<double>>>>>? AvatarConstellations
        {
            get => this.avatarConstellations;

            set => this.SetProperty(ref this.avatarConstellations, value);
        }
        public IList<Indexed<string, Rate<Two<IList<HutaoItem>>>>>? TeamCombinations
        {
            get => this.teamCombinations;

            set => this.SetProperty(ref this.teamCombinations, value);
        }

        public ICommand OpenUICommand { get; }
        public ICommand UploadCommand { get; }

        public HutaoStatisticViewModel(ICookieService cookieService, IHutaoStatisticService hutaoStatisticService, IAsyncRelayCommandFactory asyncRelayCommandFactory)
        {
            this.cookieService = cookieService;
            this.hutaoStatisticService = hutaoStatisticService;

            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
            this.UploadCommand = asyncRelayCommandFactory.Create(this.UploadRecordsAsync);
        }

        public async Task OpenUIAsync()
        {
            try
            {
                await this.hutaoStatisticService.InitializeAsync(this.CancellationToken);

                this.Overview = await this.hutaoStatisticService.GetOverviewAsync(this.CancellationToken);
                //V1
                this.AvatarParticipations = this.hutaoStatisticService.GetAvatarParticipations();
                this.TeamCollocations = this.hutaoStatisticService.GetTeamCollocations();
                this.WeaponUsages = this.hutaoStatisticService.GetWeaponUsages();
                //V2
                this.AvatarReliquaryUsages = this.hutaoStatisticService.GetReliquaryUsages();
                this.AvatarConstellations = this.hutaoStatisticService.GetAvatarConstellations();
                this.TeamCombinations = this.hutaoStatisticService.GetTeamCombinations();
            }
            catch (TaskCanceledException) { this.Log("Open UI cancelled"); }
            catch (Exception e)
            {
                this.Log(e);
                await new ContentDialog()
                {
                    Title = "加载失败",
                    Content = e.Message,
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync().ConfigureAwait(false);
                return;
            }

            this.ShouldUIPresent = true;
        }
        public async Task UploadRecordsAsync()
        {
            this.ShouldUIPresent = false;
            try
            {
                await this.hutaoStatisticService.GetAllRecordsAndUploadAsync(this.cookieService.CurrentCookie, this.ConfirmAsync, this.HandleResponseAsync, this.CancellationToken);
            }
            catch (TaskCanceledException) { this.Log("upload data cancelled by user switch page"); }
            catch (Exception e)
            {
                this.Log(e);
                await new ContentDialog()
                {
                    Title = "提交记录失败",
                    Content = "发生了致命错误",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
            finally
            {
                this.ShouldUIPresent = true;
            }
        }

        private async Task<bool> ConfirmAsync(PlayerRecord record)
        {
            return ContentDialogResult.Primary == await this.ExecuteOnUIAsync(new UploadDialog(record).ShowAsync);
        }
        private async Task HandleResponseAsync(Response resonse)
        {
            await new ContentDialog()
            {
                Title = "提交记录",
                Content = resonse.Message,
                PrimaryButtonText = "确定",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
        }
    }
}
