using DGP.Genshin.Control.GenshinElement.HutaoStatistic;
using DGP.Genshin.DataModel;
using DGP.Genshin.DataModel.HutaoAPI;
using DGP.Genshin.HutaoAPI.GetModel;
using DGP.Genshin.Service.Abstratcion;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
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

        private Overview? overview;
        private IEnumerable<IndexedListWrapper<Item<double>>>? avatarParticipations;
        //private IEnumerable<Item>? avatarReliquaryUsages;
        private IEnumerable<Item<IEnumerable<Item<double>>>>? teamCollocations;
        private IEnumerable<Item<IEnumerable<Item<double>>>>? weaponUsages;
        private bool shouldUIPresent;

        public Overview? Overview { get => overview; set => SetProperty(ref overview, value); }
        public IEnumerable<IndexedListWrapper<Item<double>>>? AvatarParticipations { get => avatarParticipations; set => SetProperty(ref avatarParticipations, value); }
        //public IEnumerable<Item>? AvatarReliquaryUsages { get => avatarReliquaryUsages; set => SetProperty(ref avatarReliquaryUsages, value); }
        public IEnumerable<Item<IEnumerable<Item<double>>>>? TeamCollocations { get => teamCollocations; set => SetProperty(ref teamCollocations, value); }
        public IEnumerable<Item<IEnumerable<Item<double>>>>? WeaponUsages { get => weaponUsages; set => SetProperty(ref weaponUsages, value); }
        public bool ShouldUIPresent { get => shouldUIPresent; set => SetProperty(ref shouldUIPresent, value); }

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
            }
            catch
            {
                return;
            }

            Overview = await hutaoStatisticService.GetOverviewAsync();

            AvatarParticipations = hutaoStatisticService.GetAvatarParticipations();
            TeamCollocations = hutaoStatisticService.GetTeamCollocations();
            WeaponUsages = hutaoStatisticService.GetWeaponUsages();

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
                    Content = "在获取数据时发生了致命错误。",
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
