using DGP.Genshin.Common.Core.Logging;
using DGP.Genshin.Common.Exceptions;
using DGP.Genshin.Common.Extensions.System.Collections.Generic;
using DGP.Genshin.DataModel.Helpers;
using DGP.Genshin.MiHoYoAPI.Gacha;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Services.GachaStatistics.Statistics
{
    /// <summary>
    /// 构造奖池统计信息的工厂类
    /// </summary>
    public static class StatisticFactory
    {
        private const double NonWeaponProb5 = 1.6 / 100.0;
        private const double NonWeaponProb4 = 13 / 100.0;
        private const int NonWeaponGranteeCount5 = 90;
        private const double WeaponProb5 = 1.85 / 100.0;
        private const double WeaponProb4 = 14.5 / 100.0;
        private const int WeaponGranteeCount5 = 80;

        private static readonly BannerConfigration NonWeaponConfig =
            new() { Prob5 = NonWeaponProb5, Prob4 = NonWeaponProb4, GranteeCount = NonWeaponGranteeCount5 };
        private static readonly BannerConfigration WeaponConfig =
            new() { Prob5 = WeaponProb5, Prob4 = WeaponProb4, GranteeCount = WeaponGranteeCount5 };

        /// <summary>
        /// 转换到统计
        /// </summary>
        /// <param name="data"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static Statistic ToStatistic(GachaData data, string uid)
        {
            Logger.LogStatic($"convert data of {uid} to statistic view");
            List<StatisticItem> characters = ToStatisticTotalCountList(data, "角色");
            List<StatisticItem> weapons = ToStatisticTotalCountList(data, "武器");
            Statistic statistic = new()
            {
                Uid = uid,
                Characters5 = characters.Where(i => i.StarUrl?.ToRank() == 5).ToList(),
                Characters4 = characters.Where(i => i.StarUrl?.ToRank() == 4).ToList(),
                Weapons5 = weapons.Where(i => i.StarUrl?.ToRank() == 5).ToList(),
                Weapons4 = weapons.Where(i => i.StarUrl?.ToRank() == 4).ToList(),
                Weapons3 = weapons.Where(i => i.StarUrl?.ToRank() == 3).ToList(),
                SpecificBanners = ToSpecificBanners(data)
            };
            statistic.Permanent = ToStatisticBanner(data, ConfigType.PermanentWish, "奔行世间", NonWeaponConfig);
            statistic.WeaponEvent = ToStatisticBanner(data, ConfigType.WeaponEventWish, "神铸赋形", WeaponConfig);
            statistic.CharacterEvent = ToStatisticBanner(data, ConfigType.CharacterEventWish, "角色活动", NonWeaponConfig);
            //statistic.CharacterEvent = ToCombinedStatisticBanner(data, ConfigType.CharacterEventWish, ConfigType.CharacterEventWish2, "角色活动", NonWeaponConfig);
            return statistic;
        }

        /// <summary>
        /// 转换到统计卡池
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="prob5"></param>
        /// <param name="prob4"></param>
        /// <param name="granteeCount"></param>
        /// <returns></returns>
        private static StatisticBanner ToStatisticBanner(GachaData data, string type, string name, BannerConfigration config)
        {
            List<GachaLogItem>? list = data[type];
            _ = list ?? throw new UnexceptedNullException($"卡池{type}:对应的卡池信息不应为 null");
            return BuildStatisticBanner(name, config, list);
        }

        private static StatisticBanner ToCombinedStatisticBanner(GachaData data, string type1, string type2, string name, BannerConfigration config)
        {
            List<GachaLogItem>? list1 = data[type1];
            _ = list1 ?? throw new UnexceptedNullException($"卡池{type1}:对应的卡池信息不应为 null");
            List<GachaLogItem>? list2 = data[type2];
            _ = list2 ?? throw new UnexceptedNullException($"卡池{type2}:对应的卡池信息不应为 null");

            List<GachaLogItem> list = Enumerable.Union(list1, list2).OrderByDescending(x => x.TimeId).ToList();
            return BuildStatisticBanner(name, config, list);
        }

        private static StatisticBanner BuildStatisticBanner(string name, BannerConfigration config, List<GachaLogItem> list)
        {
            //上次出货抽数
            int index5 = list.FindIndex(i => i.Rank == "5");
            int index4 = list.FindIndex(i => i.Rank == "4");

            StatisticBanner banner = new()
            {
                TotalCount = list.Count,
                CurrentName = name,
                CountSinceLastStar5 = index5 == -1 ? 0 : index5,
                CountSinceLastStar4 = index4 == -1 ? 0 : index4,

                Star5Count = list.Count(i => i.Rank == "5"),
                Star4Count = list.Count(i => i.Rank == "4"),
                Star3Count = list.Count(i => i.Rank == "3"),
            };
            if (list.Count > 0)
            {
                banner.StartTime = list.Last().Time;
                banner.EndTime = list.First().Time;
            }
            banner.Star5List = ListOutStatisticStar5(list, banner.Star5Count);
            //确保至少有一个五星才进入
            if (banner.Star5List.Count > 0)
            {
                banner.AverageGetStar5 = banner.Star5List.Sum(i => i.Count) * 1.0 / banner.Star5List.Count;
                banner.MaxGetStar5Count = banner.Star5List.Max(i => i.Count);
                banner.MinGetStar5Count = banner.Star5List.Min(i => i.Count);
                banner.NextGuaranteeType = banner.Star5List.First().IsUp ? "小保底" : "大保底";
            }
            else//while no 5 star get
            {
                banner.AverageGetStar5 = 0.0;
                banner.MaxGetStar5Count = 0;
                banner.MinGetStar5Count = 0;
                banner.NextGuaranteeType = "小保底";
            }

            int predicatedCount5 = (int)(Math.Round((banner.Star5Count + 1) / config.Prob5) - banner.TotalCount);
            banner.NextStar5PredictCount = RestrictPredicatedCount5(predicatedCount5, banner, config.GranteeCount);
            int predicatedCount4 = (int)(Math.Round((banner.Star4Count + 1) / config.Prob4) - banner.TotalCount);
            banner.NextStar4PredictCount = RestrictPredicatedCount4(predicatedCount4, banner);

            banner.Star5Prob = banner.Star5Count * 1.0 / banner.TotalCount;
            banner.Star4Prob = banner.Star4Count * 1.0 / banner.TotalCount;
            banner.Star3Prob = banner.Star3Count * 1.0 / banner.TotalCount;
            return banner;
        }

        /// <summary>
        /// 总体出货数量统计
        /// </summary>
        /// <param name="data"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        private static List<StatisticItem> ToStatisticTotalCountList(GachaData data, string itemType)
        {
            CounterOf<StatisticItem> counter = new();
            foreach (List<GachaLogItem>? list in data.Values)
            {
                _ = list ?? throw new UnexceptedNullException("卡池列表不应为 null");
                foreach (GachaLogItem i in list)
                {
                    if (i.ItemType == itemType)
                    {
                        _ = i.Name ?? throw new UnexceptedNullException("卡池物品名称不应为 null");
                        if (!counter.ContainsKey(i.Name))
                        {
                            _ = i.Rank ?? throw new UnexceptedNullException("卡池物品稀有度不应为 null");
                            counter[i.Name] = new StatisticItem()
                            {
                                Count = 0,
                                Name = i.Name,
                                StarUrl = StarHelper.FromRank(int.Parse(i.Rank))
                            };
                            if (itemType == "武器")
                            {
                                DataModel.Weapons.Weapon? weapon = MetadataService.Instance.Weapons?.First(w => w.Name == i.Name);
                                counter[i.Name].Source = weapon?.Source;
                                counter[i.Name].Badge = weapon?.Type;
                            }
                            else if (itemType == "角色")
                            {
                                DataModel.Characters.Character? character = MetadataService.Instance.Characters?.First(c => c.Name == i.Name);
                                counter[i.Name].Source = character?.Source;
                                counter[i.Name].Badge = character?.Element;
                            }
                            else
                            {
                                throw new SnapGenshinInternalException("不支持的物品类型");
                            }
                        }
                        counter[i.Name].Count += 1;
                    }
                }
            }
            return counter.Select(k => k.Value).OrderByDescending(i => i.StarUrl?.ToRank()).ThenByDescending(i => i.Count).ToList();
        }
        private static List<StatisticItem> ToSpecificTotalCountList(List<SpecificItem> list)
        {
            CounterOf<StatisticItem> counter = new();
            foreach (SpecificItem i in list)
            {
                if (i.Name is null)
                {
                    continue;
                }
                if (!counter.ContainsKey(i.Name))
                {
                    counter[i.Name] = new StatisticItem()
                    {
                        Count = 0,
                        Name = i.Name,
                        StarUrl = i.StarUrl,
                        Source = i.Source,
                        Badge = i.Badge,
                        Time = i.Time
                    };
                }
                counter[i.Name].Count += 1;
            }
            return counter.Select(k => k.Value)
                .OrderByDescending(i => i.StarUrl?.ToRank())
                .ThenByDescending(i => i.Count).ToList();
        }
        private static List<StatisticItem5Star> ListOutStatisticStar5(IEnumerable<GachaLogItem> items, int star5Count)
        {
            //prevent modify the items and simplify the algorithm
            //search from the earliest time
            List<GachaLogItem> reversedItems = items.Reverse().ToList();
            List<StatisticItem5Star> counter = new();
            for (int i = 0; i < star5Count; i++)
            {
                GachaLogItem currentStar5 = reversedItems.First(i => i.Rank == "5");
                int count = reversedItems.IndexOf(currentStar5) + 1;
                bool isBigGuarantee = counter.Count > 0 && !counter.Last().IsUp;

                SpecificBanner? matchedBanner = MetadataService.Instance.SpecificBanners?.Find(b =>
                    //match type first
                    b.Type == currentStar5.GachaType &&
                    currentStar5.Time >= b.StartTime &&
                    currentStar5.Time <= b.EndTime);

                string? gType = currentStar5.GachaType;
                counter.Add(new StatisticItem5Star()
                {
                    GachaTypeName = gType is null ? gType : ConfigType.Known[gType],
                    Source = MetadataService.Instance.FindSourceByName(currentStar5.Name),
                    Name = currentStar5.Name,
                    Count = count,
                    Time = currentStar5.Time,
                    IsUp = matchedBanner?.UpStar5List is not null && matchedBanner.UpStar5List.Exists(i => i.Name == currentStar5.Name),
                    IsBigGuarantee = matchedBanner is not null && isBigGuarantee
                });
                reversedItems.RemoveRange(0, count);
            }
            counter.Reverse();
            return counter;
        }
        private static int RestrictPredicatedCount5(int predicatedCount, StatisticBanner banner, int granteeCount)
        {
            if (predicatedCount < 1)
            {
                banner.Appraise = "非";
                return 1;
            }
            int predicatedSum = predicatedCount + banner.CountSinceLastStar5;
            if (predicatedSum > granteeCount)
            {
                banner.Appraise = "欧";
                predicatedCount = granteeCount - banner.CountSinceLastStar5;
            }
            else
            {
                banner.Appraise = "正";
            }
            return predicatedCount;
        }
        private static int RestrictPredicatedCount4(int predicatedCount, StatisticBanner banner, int granteeCount = 10)
        {
            if (predicatedCount < 1)
            {
                return 1;
            }
            int predicatedSum = predicatedCount + banner.CountSinceLastStar4;
            if (predicatedSum > granteeCount)
            {
                predicatedCount = granteeCount - banner.CountSinceLastStar4;
            }
            return predicatedCount;
        }
        private static List<SpecificBanner> ToSpecificBanners(GachaData data)
        {
            //clone from metadata
            List<SpecificBanner>? clonedBanners = MetadataService.Instance.SpecificBanners?.ClonePartially();
            _ = clonedBanners ?? throw new SnapGenshinInternalException("无可用的卡池信息");

            clonedBanners.ForEach(b => b.ClearItemAndStar5List());
            SpecificBanner permanent = new() { CurrentName = "奔行世间" };
            clonedBanners.Add(permanent);

            foreach (string type in data.Keys)
            {
                if (type is ConfigType.NoviceWishes)
                {
                    //skip these banner
                    continue;
                }
                if (type is ConfigType.PermanentWish)
                {
                    if (data[type] is List<GachaLogItem> plist)
                    {
                        foreach (GachaLogItem item in plist)
                        {
                            AddItemToSpecificBanner(item, permanent);
                        }
                    }
                }
                if (data[type] is List<GachaLogItem> list)
                {
                    foreach (GachaLogItem item in list)
                    {
                        SpecificBanner? banner = clonedBanners.Find(b => b.Type == type && item.Time >= b.StartTime && item.Time <= b.EndTime);
                        AddItemToSpecificBanner(item, banner);
                    }
                }
            }

            CalculateSpecificBannerDetails(clonedBanners);

            return clonedBanners
                .Where(b => b.TotalCount > 0)
                .OrderByDescending(b => b.StartTime)
                .ThenBy(b => b.Type)
                .ToList();
        }
        private static void AddItemToSpecificBanner(GachaLogItem item, SpecificBanner? banner)
        {
            DataModel.Characters.Character? isc = MetadataService.Instance.Characters?.FirstOrDefault(c => c.Name == item.Name);
            DataModel.Weapons.Weapon? isw = MetadataService.Instance.Weapons?.FirstOrDefault(w => w.Name == item.Name);
            SpecificItem ni = new()
            {
                Time = item.Time
            };

            if (isc is not null)
            {
                ni.StarUrl = isc.Star;
                ni.Source = isc.Source;
                ni.Name = isc.Name;
                ni.Badge = isc.Element;
            }
            else if (isw is not null)
            {
                ni.StarUrl = isw.Star;
                ni.Source = isw.Source;
                ni.Name = isw.Name;
                ni.Badge = isw.Type;
            }
            else//both null
            {
                ni.Name = item.Name;
                ni.StarUrl = item.Rank is null ? null : StarHelper.FromRank(int.Parse(item.Rank));
                Logger.LogStatic($"an unsupported item:{item.Name} is found while converting to {nameof(SpecificBanner)}");
            }
            //? fix issue where crashes when no banner exists
            banner?.Items.Add(ni);
        }
        private static void CalculateSpecificBannerDetails(List<SpecificBanner> specificBanners)
        {
            foreach (SpecificBanner banner in specificBanners)
            {
                banner.TotalCount = banner.Items.Count;
                if (banner.TotalCount == 0)
                {
                    continue;
                }

                banner.Star5Count = banner.Items.Count(i => i.StarUrl.IsOfRank(5));
                banner.Star4Count = banner.Items.Count(i => i.StarUrl.IsOfRank(4));
                banner.Star3Count = banner.Items.Count(i => i.StarUrl.IsOfRank(3));

                List<StatisticItem> statisticList = ToSpecificTotalCountList(banner.Items);
                banner.StatisticList5 = statisticList.Where(i => i.StarUrl.IsOfRank(5)).ToList();
                banner.StatisticList4 = statisticList.Where(i => i.StarUrl.IsOfRank(4)).ToList();
                banner.StatisticList3 = statisticList.Where(i => i.StarUrl.IsOfRank(3)).ToList();
            }
        }
    }
}