using DGP.Genshin.DataModel;
using DGP.Genshin.DataModel.HutaoAPI;
using DGP.Genshin.HutaoAPI;
using DGP.Genshin.HutaoAPI.GetModel;
using DGP.Genshin.HutaoAPI.PostModel;
using DGP.Genshin.MiHoYoAPI.Response;
using DGP.Genshin.Service.Abstratcion;
using Snap.Core.DependencyInjection;
using Snap.Data.Primitive;
using Snap.Extenion.Enumerable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Service
{
    [Service(typeof(IHutaoStatisticService), InjectAs.Transient)]
    internal class HutaoStatisticService : IHutaoStatisticService
    {
        private readonly PlayerRecordClient playerRecordClient = new();

        private IEnumerable<GenshinItem> avatarMap = null!;
        private IEnumerable<GenshinItem> weaponMap = null!;
        private IEnumerable<GenshinItem> reliquaryMap = null!;

        private IEnumerable<AvatarParticipation> _avatarParticipations = null!;
        private IEnumerable<AvatarConstellationNum> _avatarConstellationNums = null!;
        private IEnumerable<TeamCollocation> _teamCollocations = null!;
        private IEnumerable<WeaponUsage> _weaponUsages = null!;
        private IEnumerable<AvatarReliquaryUsage> _avatarReliquaryUsages = null!;
        private IEnumerable<TeamCombination> _teamCombinations = null!;

        public async Task InitializeAsync()
        {
            await playerRecordClient.InitializeAsync();

            avatarMap = await playerRecordClient.GetAvatarMapAsync();
            weaponMap = await playerRecordClient.GetWeaponMapAsync();
            reliquaryMap = await playerRecordClient.GetReliquaryMapAsync();

            _avatarParticipations = await playerRecordClient.GetAvatarParticipationsAsync();
            _avatarConstellationNums = await playerRecordClient.GetAvatarConstellationsAsync();
            _teamCollocations = await playerRecordClient.GetTeamCollocationsAsync();
            _weaponUsages = await playerRecordClient.GetWeaponUsagesAsync();
            _avatarReliquaryUsages = await playerRecordClient.GetAvatarReliquaryUsagesAsync();
            _teamCombinations = await playerRecordClient.GetTeamCombinationsAsync();
        }

        public async Task<Overview?> GetOverviewAsync()
        {
            return await playerRecordClient.GetOverviewAsync();
        }

        public IEnumerable<IndexedListWrapper<Item<double>>> GetAvatarParticipations()
        {
            List<IndexedListWrapper<Item<double>>> avatarParticipationResults = new();
            //保证 12层在前
            foreach (AvatarParticipation avatarParticipation in _avatarParticipations.OrderByDescending(x => x.Floor))
            {
                IEnumerable<Item<double>> result = avatarParticipation.AvatarUsage
                    .Join(avatarMap.DistinctBy(a => a.Id), rate => rate.Id, avatar => avatar.Id,
                    (rate, avatar) => new Item<double>(avatar.Id, avatar.Name, avatar.Url, rate.Value));

                avatarParticipationResults
                    .Add(new IndexedListWrapper<Item<double>>(
                        avatarParticipation.Floor, 
                        result.OrderByDescending(x => x.Value)));
            }
            return avatarParticipationResults;
        }

        public IEnumerable<Item<IEnumerable<NamedValue<double>>>> GetAvatarConstellations()
        {
            List<Item<IEnumerable<NamedValue<double>>>> teamCollocationsResults = new();
            foreach (AvatarConstellationNum avatarConstellationNum in _avatarConstellationNums)
            {
                GenshinItem? matched = avatarMap.FirstOrDefault(x => x.Id == avatarConstellationNum.Avatar);
                if (matched != null)
                {
                    IEnumerable<NamedValue<double>> result = avatarConstellationNum.Rate
                        .Select(rate => new NamedValue<double>($"{rate.Id} 命", rate.Value));

                    teamCollocationsResults
                        .Add(new Item<IEnumerable<NamedValue<double>>>(
                            matched.Id, matched.Name, matched.Url,
                            result));
                }
            }
            return teamCollocationsResults.OrderByDescending(x => x.Id);
        }

        public IEnumerable<Item<IEnumerable<Item<double>>>> GetTeamCollocations()
        {
            List<Item<IEnumerable<Item<double>>>> teamCollocationsResults = new();
            foreach (TeamCollocation teamCollocation in _teamCollocations)
            {
                GenshinItem? matched = avatarMap.FirstOrDefault(x => x.Id == teamCollocation.Avater);
                if (matched != null)
                {
                    IEnumerable<Item<double>> result = teamCollocation.Collocations
                    .Join(avatarMap.DistinctBy(a => a.Id), rate => rate.Id, avatar => avatar.Id,
                    (rate, avatar) => new Item<double>(avatar.Id, avatar.Name, avatar.Url, rate.Value));

                    teamCollocationsResults
                        .Add(new Item<IEnumerable<Item<double>>>(
                            matched.Id, matched.Name, matched.Url, 
                            result.OrderByDescending(x => x.Value)));
                }
            }
            return teamCollocationsResults.OrderByDescending(x => x.Id);
        }

        public IEnumerable<Item<IEnumerable<Item<double>>>> GetWeaponUsages()
        {
            List<Item<IEnumerable<Item<double>>>> weaponUsagesResults = new();
            foreach (WeaponUsage weaponUsage in _weaponUsages)
            {
                GenshinItem? matchedAvatar = avatarMap.FirstOrDefault(x => x.Id == weaponUsage.Avatar);
                if (matchedAvatar != null)
                {
                    IEnumerable<Item<double>> result = weaponUsage.Weapons
                    .Join(weaponMap, rate => rate.Id, weapon => weapon.Id,
                    (rate, weapon) => new Item<double>(weapon.Id, weapon.Name, weapon.Url, rate.Value));

                    weaponUsagesResults
                        .Add(new Item<IEnumerable<Item<double>>>(
                            matchedAvatar.Id, matchedAvatar.Name, matchedAvatar.Url,
                            result.OrderByDescending(x => x.Value)));
                }
            }
            return weaponUsagesResults.OrderByDescending(x => x.Id);
        }

        public IEnumerable<Item<IEnumerable<NamedValue<Rate<IEnumerable<Item<int>>>>>>> GetReliquaryUsages()
        {
            List<Item<IEnumerable<NamedValue<Rate<IEnumerable<Item<int>>>>>>> reliquaryUsagesResults = new();
            foreach (AvatarReliquaryUsage reliquaryUsage in _avatarReliquaryUsages)
            {
                GenshinItem? matchedAvatar = avatarMap.FirstOrDefault(x => x.Id == reliquaryUsage.Avatar);
                if (matchedAvatar != null)
                {
                    List<NamedValue<Rate<IEnumerable<Item<int>>>>> result = new();
                    
                    foreach(Rate<string> usage in reliquaryUsage.ReliquaryUsage)
                    {
                        List<Item<int>> relicList = new();
                        StringBuilder nameBuilder = new();
                        string[] relicWithCountArray = usage.Id!.Split(';');
                        foreach(var relicAndCount in relicWithCountArray)
                        {
                            //0 id 1 count
                            string[]? relicSetIdAndCount = relicAndCount.Split('-');
                            GenshinItem? matchedRelic = reliquaryMap.FirstOrDefault(x => x.Id == int.Parse(relicSetIdAndCount[0]));
                            if(matchedRelic != null)
                            {
                                string count = relicSetIdAndCount[1];
                                nameBuilder.Append($"{count}×{matchedRelic.Name} ");
                                relicList.Add(new Item<int>(matchedRelic.Id, matchedRelic.Name, matchedRelic.Url, int.Parse(count)));
                            }
                        }
                        if(nameBuilder.Length > 0)
                        {
                            Rate<IEnumerable<Item<int>>> rate = new() { Id = relicList, Value = usage.Value };
                            //remove last space
                            NamedValue<Rate<IEnumerable<Item<int>>>> namedValue = new(nameBuilder.ToString()[0..^1], rate);
                            result.Add(namedValue);
                        }
                    }

                    reliquaryUsagesResults
                        .Add(new Item<IEnumerable<NamedValue<Rate<IEnumerable<Item<int>>>>>>(
                            matchedAvatar.Id, matchedAvatar.Name, matchedAvatar.Url,
                            result));
                }
            }
            return reliquaryUsagesResults.OrderByDescending(x => x.Id);
        }

        public IEnumerable<IndexedListWrapper<string, Rate<Two<IEnumerable<GenshinItem>>>>> GetTeamCombinations()
        {
            List<IndexedListWrapper<string, Rate<Two<IEnumerable<GenshinItem>>>>> teamCombinationResults = new();
            foreach (TeamCombination temaCombination in _teamCombinations.OrderByDescending(x => x.Level.Floor).ThenByDescending(x => x.Level.Index))
            {
                IEnumerable<Rate<Two<IEnumerable<GenshinItem>>>> teamRates = temaCombination.Teams
                .Select(team => new Rate<Two<IEnumerable<GenshinItem>>>
                {
                    Value = team.Value,
                    Id = new( team.Id!.GetUp().Select(id => avatarMap.FirstOrDefault(a => a.Id == id)).NotNull(),
                    team.Id!.GetDown().Select(id => avatarMap.FirstOrDefault(a => a.Id == id)).NotNull())
                });

                teamCombinationResults
                    .Add(new IndexedListWrapper<string, Rate<Two<IEnumerable<GenshinItem>>>>(
                        $"{temaCombination.Level.Floor}-{temaCombination.Level.Index}",
                        teamRates.OrderByDescending(x => x.Value).Take(16)));
            }
            return teamCombinationResults;
        }

        public async Task GetAllRecordsAndUploadAsync(string cookie, Func<PlayerRecord, Task<bool>> confirmFunc, Func<Response, Task> resultAsyncFunc)
        {
            await playerRecordClient.GetAllRecordsAndUploadAsync(cookie, confirmFunc, resultAsyncFunc);
        }
    }
}
