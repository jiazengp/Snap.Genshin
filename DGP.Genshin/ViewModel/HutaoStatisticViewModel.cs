using DGP.Genshin.Control.GenshinElement.HutaoStatistic;
using DGP.Genshin.DataModel.HutaoAPI;
using DGP.Genshin.HutaoAPI.GetModel;
using DGP.Genshin.HutaoAPI.PostModel;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
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
        private IEnumerable<Indexed<int, Item<double>>>? avatarParticipations;
        private IEnumerable<Item<IEnumerable<NamedValue<Rate<IEnumerable<Item<int>>>>>>>? avatarReliquaryUsages;
        private IEnumerable<Item<IEnumerable<Item<double>>>>? teamCollocations;
        private IEnumerable<Item<IEnumerable<Item<double>>>>? weaponUsages;
        private IEnumerable<Rate<Item<IEnumerable<NamedValue<double>>>>>? avatarConstellations;
        private IEnumerable<Indexed<string, Rate<Two<IEnumerable<HutaoItem>>>>>? teamCombinations;

        public bool ShouldUIPresent
        { get => shouldUIPresent; set => SetProperty(ref shouldUIPresent, value); }
        public Overview? Overview
        { get => overview; set => SetProperty(ref overview, value); }
        public IEnumerable<Indexed<int, Item<double>>>? AvatarParticipations
        { get => avatarParticipations; set => SetProperty(ref avatarParticipations, value); }
        public IEnumerable<Item<IEnumerable<NamedValue<Rate<IEnumerable<Item<int>>>>>>>? AvatarReliquaryUsages
        { get => avatarReliquaryUsages; set => SetProperty(ref avatarReliquaryUsages, value); }
        public IEnumerable<Item<IEnumerable<Item<double>>>>? TeamCollocations
        { get => teamCollocations; set => SetProperty(ref teamCollocations, value); }
        public IEnumerable<Item<IEnumerable<Item<double>>>>? WeaponUsages
        { get => weaponUsages; set => SetProperty(ref weaponUsages, value); }
        public IEnumerable<Rate<Item<IEnumerable<NamedValue<double>>>>>? AvatarConstellations
        { get => avatarConstellations; set => SetProperty(ref avatarConstellations, value); }
        public IEnumerable<Indexed<string, Rate<Two<IEnumerable<HutaoItem>>>>>? TeamCombinations
        { get => teamCombinations; set => SetProperty(ref teamCombinations, value); }

        public ICommand OpenUICommand { get; }
        public ICommand UploadCommand { get; }
        public HutaoStatisticViewModel(ICookieService cookieService, IHutaoStatisticService hutaoStatisticService)
        {
            this.cookieService = cookieService;
            this.hutaoStatisticService = hutaoStatisticService;

            OpenUICommand = new AsyncRelayCommand(OpenUIAsync);
            UploadCommand = new AsyncRelayCommand(UploadRecordsAsync);
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
                await App.Current.Dispatcher.InvokeAsync(async () => await new ContentDialog()
                {
                    Title = "加载失败",
                    Content = e.Message,
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync()).Task.Unwrap();
                return;
            }

            ShouldUIPresent = true;
        }
        public async Task UploadRecordsAsync()
        {
            ShouldUIPresent = false;
            try
            {
                await hutaoStatisticService.GetAllRecordsAndUploadAsync(cookieService.CurrentCookie,
                    async record => ContentDialogResult.Primary == await new UploadDialog(record).ShowAsync(),
                    async resp => await new ContentDialog()
                    {
                        Title = "提交记录",
                        Content = resp.Message,
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync());
            }
            catch
            {
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
    }
}
