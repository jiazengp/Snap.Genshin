using DGP.Genshin.HutaoAPI;
using DGP.Genshin.HutaoAPI.GetModel;
using DGP.Genshin.MiHoYoAPI.Calculation;
using DGP.Genshin.MiHoYoAPI.Response;
using DGP.Genshin.Service.Abstratcion;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class HutaoStatisticViewModel : ObservableObject
    {
        private readonly ICookieService cookieService;
        private readonly PlayerRecordClient playerRecordClient = new();
        private readonly Calculator calculator;

        private Overview? overview;
        private IEnumerable<Indexed<Item<double>>>? avatarParticipations;
        //private IEnumerable<Item>? avatarReliquaryUsages;
        private IEnumerable<Item<IEnumerable<Item<double>>>>? teamCollocations;
        private IEnumerable<Item<IEnumerable<Item<double>>>>? weaponUsages;
        private bool shouldUIPresent;

        public Overview? Overview { get => overview; set => SetProperty(ref overview, value); }
        public IEnumerable<Indexed<Item<double>>>? AvatarParticipations { get => avatarParticipations; set => SetProperty(ref avatarParticipations, value); }
        //public IEnumerable<Item>? AvatarReliquaryUsages { get => avatarReliquaryUsages; set => SetProperty(ref avatarReliquaryUsages, value); }
        public IEnumerable<Item<IEnumerable<Item<double>>>>? TeamCollocations { get => teamCollocations; set => SetProperty(ref teamCollocations, value); }
        public IEnumerable<Item<IEnumerable<Item<double>>>>? WeaponUsages { get => weaponUsages; set => SetProperty(ref weaponUsages, value); }
        public bool ShouldUIPresent { get => shouldUIPresent; set => SetProperty(ref shouldUIPresent, value); }

        public ICommand OpenUICommand { get; }
        public ICommand UploadCommand { get; }
        public HutaoStatisticViewModel(ICookieService cookieService)
        {
            this.cookieService = cookieService;

            calculator = new Calculator(cookieService.CurrentCookie);

            OpenUICommand = new AsyncRelayCommand(OpenUIAsync);
            UploadCommand = new AsyncRelayCommand(UploadRecordsAsync);
        }

        public class Item<T>
        {
            public Item(int id, string? name, string? icon, T value)
            {
                Id = id;
                Name = name;
                Icon = icon;
                Value = value;
            }

            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Icon { get; set; }
            public T Value { get; set; }
        }
        public class Indexed<T>
        {
            public Indexed(int index, IEnumerable<T> list)
            {
                Index = index;
                List = list;
            }

            public int Index { get; set; }
            public IEnumerable<T> List { get; set; }
        }

        public async Task OpenUIAsync()
        {
            try
            {
                await playerRecordClient.InitializeAsync();
            }
            catch
            {
                return;
            }

            Overview = await playerRecordClient.GetOverviewAsync();

            IEnumerable<AvatarParticipation> _avatarParticipations = await playerRecordClient.GetAvatarParticipationsAsync();
            //IEnumerable<AvatarReliquaryUsage> _avatarReliquaryUsages = await playerRecordClient.GetAvatarReliquaryUsagesAsync();
            IEnumerable<TeamCollocation> _teamCollocations = await playerRecordClient.GetTeamCollocationsAsync();
            IEnumerable<WeaponUsage> _weaponUsages = await playerRecordClient.GetWeaponUsagesAsync();

            List<Avatar> avatars = await calculator.GetAvatarListAsync(new());
            List<Weapon> weapons = await calculator.GetWeaponListAsync(new());
            //List<Reliquary> reliquaries = await calculator.GetReliquaryListAsync(new());

            //角色使用率
            List<Indexed<Item<double>>> avatarParticipationResults = new();
            foreach (AvatarParticipation avatarParticipation in _avatarParticipations)
            {
                IEnumerable<Item<double>> result = avatarParticipation.AvatarUsage
                    .Join(avatars.DistinctBy(a => a.Id), rate => rate.Id, avatar => avatar.Id,
                    (rate, avatar) => new Item<double>(avatar.Id, avatar.Name, avatar.Icon, rate.Value));

                avatarParticipationResults.Add(new Indexed<Item<double>>(avatarParticipation.Floor, result.OrderByDescending(x => x.Value)));
            }
            AvatarParticipations = avatarParticipationResults;
            //圣遗物使用率

            //队伍搭配
            List<Item<IEnumerable<Item<double>>>> teamCollocationsResults = new();
            foreach (TeamCollocation teamCollocation in _teamCollocations)
            {
                Avatar? matched = avatars.FirstOrDefault(x => x.Id == teamCollocation.Avater);
                if (matched != null)
                {
                    IEnumerable<Item<double>> result = teamCollocation.Collocations
                    .Join(avatars.DistinctBy(a => a.Id), rate => rate.Id, avatar => avatar.Id,
                    (rate, avatar) => new Item<double>(avatar.Id, avatar.Name, avatar.Icon, rate.Value));

                    teamCollocationsResults.Add(new Item<IEnumerable<Item<double>>>(matched.Id, matched.Name, matched.Icon, result.OrderByDescending(x => x.Value)));
                }
            }
            TeamCollocations = teamCollocationsResults.OrderByDescending(x => x.Id);

            //武器搭配
            List<Item<IEnumerable<Item<double>>>> weaponUsagesResults = new();
            foreach (WeaponUsage weaponUsage in _weaponUsages)
            {
                Avatar? matched = avatars.FirstOrDefault(x => x.Id == weaponUsage.Avatar);
                if (matched != null)
                {
                    IEnumerable<Item<double>> result = weaponUsage.Weapons
                    .Join(weapons, rate => rate.Id, weapon => weapon.Id,
                    (rate, weapon) => new Item<double>(weapon.Id, weapon.Name, weapon.Icon, rate.Value));

                    weaponUsagesResults.Add(new Item<IEnumerable<Item<double>>>(matched.Id, matched.Name, matched.Icon, result.OrderByDescending(x => x.Value)));
                }
            }
            WeaponUsages = weaponUsagesResults.OrderByDescending(x => x.Id);
            ShouldUIPresent = true;
        }
        public async Task UploadRecordsAsync()
        {
            ShouldUIPresent = false;
            try
            {
                List<Response> responses = await playerRecordClient.GetAllRecordsAndUploadAsync(cookieService.CurrentCookie);
                foreach (Response resp in responses)
                {
                    await new ContentDialog()
                    {
                        Title = "提交记录",
                        Content = resp.Message,
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
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
