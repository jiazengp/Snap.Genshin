using DGP.Genshin.DataModel;
using DGP.Genshin.DataModel.HutaoAPI;
using DGP.Genshin.HutaoAPI;
using DGP.Genshin.HutaoAPI.GetModel;
using DGP.Genshin.HutaoAPI.PostModel;
using DGP.Genshin.MiHoYoAPI.Calculation;
using DGP.Genshin.MiHoYoAPI.Response;
using DGP.Genshin.Service.Abstratcion;
using Snap.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DataWeapon = DGP.Genshin.MiHoYoAPI.Calculation.Weapon;

namespace DGP.Genshin.Service
{
    [Service(typeof(IHutaoStatisticService), InjectAs.Transient)]
    internal class HutaoStatisticService : IHutaoStatisticService
    {
        private readonly PlayerRecordClient playerRecordClient = new();
        private readonly Calculator calculator;

        private List<Avatar> avatars = null!;
        private List<DataWeapon> weapons = null!;

        private IEnumerable<AvatarParticipation> _avatarParticipations = null!;
        private IEnumerable<TeamCollocation> _teamCollocations = null!;
        private IEnumerable<WeaponUsage> _weaponUsages = null!;

        public HutaoStatisticService(ICookieService cookieService)
        {
            calculator = new Calculator(cookieService.CurrentCookie);
        }

        public async Task InitializeAsync()
        {
            await playerRecordClient.InitializeAsync();

            avatars = await calculator.GetAvatarListAsync(new());
            weapons = await calculator.GetWeaponListAsync(new());

            _avatarParticipations = await playerRecordClient.GetAvatarParticipationsAsync();
            _teamCollocations = await playerRecordClient.GetTeamCollocationsAsync();
            _weaponUsages = await playerRecordClient.GetWeaponUsagesAsync();
        }

        public async Task<Overview?> GetOverviewAsync()
        {
            return await playerRecordClient.GetOverviewAsync();
        }

        public IEnumerable<IndexedListWrapper<Item<double>>> GetAvatarParticipations()
        {
            List<IndexedListWrapper<Item<double>>> avatarParticipationResults = new();
            foreach (AvatarParticipation avatarParticipation in _avatarParticipations.Reverse())
            {
                IEnumerable<Item<double>> result = avatarParticipation.AvatarUsage
                    .Join(avatars.DistinctBy(a => a.Id), rate => rate.Id, avatar => avatar.Id,
                    (rate, avatar) => new Item<double>(avatar.Id, avatar.Name, avatar.Icon, rate.Value));

                avatarParticipationResults.Add(new IndexedListWrapper<Item<double>>(avatarParticipation.Floor, result.OrderByDescending(x => x.Value)));
            }
            return avatarParticipationResults;
        }

        public IEnumerable<Item<IEnumerable<Item<double>>>> GetTeamCollocations()
        {
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
            return teamCollocationsResults.OrderByDescending(x => x.Id);
        }

        public IEnumerable<Item<IEnumerable<Item<double>>>> GetWeaponUsages()
        {
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
            return weaponUsagesResults.OrderByDescending(x => x.Id);
        }

        public async Task GetAllRecordsAndUploadAsync(string cookie, Func<PlayerRecord, Task<bool>> confirmFunc, Func<Response, Task> resultAsyncFunc)
        {
            await playerRecordClient.GetAllRecordsAndUploadAsync(cookie, confirmFunc, resultAsyncFunc);
        }
    }
}
