using DGP.Genshin.Control.GenshinElement.HutaoStatistic;
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
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class HutaoStatisticViewModel : ObservableObject
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

        public bool ShouldUIPresent
        {
            get => shouldUIPresent;

            set => SetProperty(ref shouldUIPresent, value);
        }
        public Overview? Overview
        {
            get => overview;

            set => SetProperty(ref overview, value);
        }
        public IList<Indexed<int, Item<double>>>? AvatarParticipations
        {
            get => avatarParticipations;

            set => SetProperty(ref avatarParticipations, value);
        }
        public IList<Item<IList<NamedValue<Rate<IList<Item<int>>>>>>>? AvatarReliquaryUsages
        {
            get => avatarReliquaryUsages;

            set => SetProperty(ref avatarReliquaryUsages, value);
        }
        public IList<Item<IList<Item<double>>>>? TeamCollocations
        {
            get => teamCollocations;

            set => SetProperty(ref teamCollocations, value);
        }
        public IList<Item<IList<Item<double>>>>? WeaponUsages
        {
            get => weaponUsages;

            set => SetProperty(ref weaponUsages, value);
        }
        public IList<Rate<Item<IList<NamedValue<double>>>>>? AvatarConstellations
        {
            get => avatarConstellations;

            set => SetProperty(ref avatarConstellations, value);
        }
        public IList<Indexed<string, Rate<Two<IList<HutaoItem>>>>>? TeamCombinations
        {
            get => teamCombinations;

            set => SetProperty(ref teamCombinations, value);
        }

        public ICommand OpenUICommand { get; }
        public ICommand UploadCommand { get; }
        public HutaoStatisticViewModel(ICookieService cookieService, IHutaoStatisticService hutaoStatisticService, IAsyncRelayCommandFactory asyncRelayCommandFactory)
        {
            this.cookieService = cookieService;
            this.hutaoStatisticService = hutaoStatisticService;

            OpenUICommand = asyncRelayCommandFactory.Create(OpenUIAsync);
            UploadCommand = asyncRelayCommandFactory.Create(UploadRecordsAsync);
        }

        public async Task OpenUIAsync()
        {
            try
            {
                await hutaoStatisticService.InitializeAsync();

                Overview = await hutaoStatisticService.GetOverviewAsync();
                //V1
                AvatarParticipations = hutaoStatisticService.GetAvatarParticipations();
                TeamCollocations = hutaoStatisticService.GetTeamCollocations();
                WeaponUsages = hutaoStatisticService.GetWeaponUsages();
                //V2
                AvatarReliquaryUsages = hutaoStatisticService.GetReliquaryUsages();
                AvatarConstellations = hutaoStatisticService.GetAvatarConstellations();
                TeamCombinations = hutaoStatisticService.GetTeamCombinations();
            }
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

            ShouldUIPresent = true;
        }
        public async Task UploadRecordsAsync()
        {
            ShouldUIPresent = false;
            try
            {
                await hutaoStatisticService.GetAllRecordsAndUploadAsync(cookieService.CurrentCookie, ConfirmAsync, HandleResponseAsync);
            }
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
                ShouldUIPresent = true;
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
