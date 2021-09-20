using DGP.Genshin.Data.Helpers;
using DGP.Genshin.Services;
using DGP.Snap.Framework.Attributes.DataModel;
using DGP.Snap.Framework.Core.Logging;
using DGP.Snap.Framework.Extensions.System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Models.MiHoYo.Gacha.Statistics
{
    /// <summary>
    /// 构造奖池统计信息的工厂类
    /// </summary>
    [ModelFactory]
    public static class StatisticFactory
    {
        public static Statistic ToStatistic(GachaData data, string uid)
        {
            Logger.LogStatic(typeof(StatisticFactory), $"convert data of {uid} to statistic view");
            List<StatisticItem> characters = ToTotalCountList(data, "角色");
            List<StatisticItem> weapons = ToTotalCountList(data, "武器");
            return new Statistic()
            {
                Uid = uid,
                Permanent = ToStatisticBanner(data, ConfigType.PermanentWish, "奔行世间", 1.6 / 100.0, 13 / 100.0, 90),
                WeaponEvent = ToStatisticBanner(data, ConfigType.WeaponEventWish, "神铸赋形", 1.85 / 100.0, 14.5 / 100.0, 80),
                CharacterEvent = ToStatisticBanner(data, ConfigType.CharacterEventWish, "角色活动", 1.6 / 100.0, 13 / 100.0, 90),
                Characters5 = characters.Where(i => i.StarUrl.ToRank() == 5).ToList(),
                Characters4 = characters.Where(i => i.StarUrl.ToRank() == 4).ToList(),
                Weapons5 = weapons.Where(i => i.StarUrl.ToRank() == 5).ToList(),
                Weapons4 = weapons.Where(i => i.StarUrl.ToRank() == 4).ToList(),
                Weapons3 = weapons.Where(i => i.StarUrl.ToRank() == 3).ToList(),
                SpecificBanners = ToSpecificBanners(data)
            };
        }
        private static StatisticBanner ToStatisticBanner(GachaData data, string type, string name, double prob5, double prob4, int granteeCount)
        {
            List<GachaLogItem> list = data[type];
            int index5 = list.FindIndex(i => i.Rank == "5");
            int index4 = list.FindIndex(i => i.Rank == "4");

            StatisticBanner banner = new StatisticBanner()
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
            banner.Star5List = ListOutStar5(list, banner.Star5Count);
            
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

            banner.NextStar5PredictCount = RestrictPredicatedCount5(
                (int)(Math.Round((banner.Star5Count + 1) / prob5) - banner.TotalCount), banner, granteeCount);
            banner.NextStar4PredictCount = RestrictPredicatedCount4(
                (int)(Math.Round((banner.Star4Count + 1) / prob4) - banner.TotalCount), banner);

            banner.Star5Prob = banner.Star5Count * 1.0 / banner.TotalCount;
            banner.Star4Prob = banner.Star4Count * 1.0 / banner.TotalCount;
            banner.Star3Prob = banner.Star3Count * 1.0 / banner.TotalCount;
            return banner;
        }
        private static List<StatisticItem> ToTotalCountList(GachaData data, string itemType)
        {
            Dictionary<string, StatisticItem> counter = new Dictionary<string, StatisticItem>();
            foreach (List<GachaLogItem> list in data.Values)
            {
                foreach (GachaLogItem i in list)
                {
                    if (i.ItemType == itemType)
                    {
                        if (!counter.ContainsKey(i.Name))
                        {
                            counter[i.Name] = new StatisticItem()
                            {
                                Count = 0,
                                Name = i.Name,
                                StarUrl = StarHelper.FromRank(Int32.Parse(i.Rank))
                            };
                            if (itemType == "武器")
                            {
                                Data.Weapons.Weapon weapon = MetaDataService.Instance.Weapons.First(w => w.Name == i.Name);
                                counter[i.Name].Source = weapon.Source;
                                counter[i.Name].Badge = weapon.Type;
                            }
                            else//角色
                            {
                                Data.Characters.Character character = MetaDataService.Instance.Characters.First(c => c.Name == i.Name);
                                counter[i.Name].Source = character.Source;
                                counter[i.Name].Badge = character.Element;
                            }
                        }
                        counter[i.Name].Count += 1;
                    }
                }
            }
            return counter.Select(k => k.Value).OrderByDescending(i => i.StarUrl.ToRank()).ThenByDescending(i => i.Count).ToList();
        }
        private static List<StatisticItem> ToTotalCountList(List<SpecificItem> list)
        {
            Dictionary<string, StatisticItem> counter = new Dictionary<string, StatisticItem>();
            foreach (SpecificItem i in list)
            {
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
            return counter.Select(k => k.Value).OrderByDescending(i => i.StarUrl.ToRank()).ThenByDescending(i => i.Count).ToList();
        }
        private static List<StatisticItem5Star> ListOutStar5(IEnumerable<GachaLogItem> items, int star5Count)
        {
            //prevent modify the items and simplify the algorithm
            //search from the earliest time
            List<GachaLogItem> reversedItems = items.Reverse().ToList();
            List<StatisticItem5Star> counter = new List<StatisticItem5Star>();
            for (int i = 0; i < star5Count; i++)
            {
                GachaLogItem currentStar5 = reversedItems.First(i => i.Rank == "5");
                int count = reversedItems.IndexOf(currentStar5) + 1;
                bool isBigGuarantee = counter.Count > 0 && !counter.Last().IsUp;

                SpecificBanner banner = MetaDataService.Instance.SpecificBanners
                    .Find(b => b.Type == currentStar5.GachaType && currentStar5.Time >= b.StartTime && currentStar5.Time <= b.EndTime);

                counter.Add(new StatisticItem5Star()
                {
                    Name = currentStar5.Name,
                    Count = count,
                    Time = currentStar5.Time,
                    IsUp = banner == null || banner.UpStar5List.Exists(i =>
                    i.Name == currentStar5.Name) || banner.UpStar4List.Exists(i => i.Name == currentStar5.Name),
                    IsBigGuarantee = isBigGuarantee
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
                return 1;
            }
            int predicatedSum = predicatedCount + banner.CountSinceLastStar5;
            if (predicatedSum > granteeCount)
            {
                if (predicatedSum > granteeCount)
                {
                    predicatedCount = granteeCount - banner.CountSinceLastStar5;
                }
            }
            return predicatedCount;
        }
        private static int RestrictPredicatedCount4(int predicatedCount, StatisticBanner banner)
        {
            if (predicatedCount < 1)
            {
                return 1;
            }
            int predicatedSum = predicatedCount + banner.CountSinceLastStar4;
            if (predicatedSum > 10)
            {
                if (predicatedSum > 10)
                {
                    predicatedCount = 10 - banner.CountSinceLastStar4;
                }
            }
            return predicatedCount;
        }
        private static List<SpecificBanner> ToSpecificBanners(GachaData data)
        {
            List<SpecificBanner> results = MetaDataService.Instance.SpecificBanners.ClonePartially();

            foreach (SpecificBanner result in results)
            {
                result.Items?.Clear();
                result.Star5List?.Clear();
            }

            foreach (string type in data.Keys)
            {
                if (type is ConfigType.NoviceWishes or ConfigType.PermanentWish)
                {
                    continue;
                }
                foreach (GachaLogItem item in data[type])
                {
                    SpecificBanner banner = results.Find(b => b.Type == type && item.Time >= b.StartTime && item.Time <= b.EndTime);
                    Data.Characters.Character isc = MetaDataService.Instance.Characters.FirstOrDefault(c => c.Name == item.Name);
                    Data.Weapons.Weapon isw = MetaDataService.Instance.Weapons.FirstOrDefault(w => w.Name == item.Name);
                    SpecificItem ni = new SpecificItem
                    {
                        Time = item.Time
                    };
                    if (isc != null)
                    {
                        ni.StarUrl = isc.Star;
                        ni.Source = isc.Source;
                        ni.Name = isc.Name;
                        ni.Badge = isc.Element;
                    }
                    else if (isw != null)
                    {
                        ni.StarUrl = isw.Star;
                        ni.Source = isw.Source;
                        ni.Name = isw.Name;
                        ni.Badge = isw.Type;
                    }
                    else
                    {
                        ni.Name = item.Name;
                        ni.StarUrl = StarHelper.FromRank(Int32.Parse(item.Rank));
                        Logger.LogStatic(typeof(StatisticFactory),
                            $"a unsupported item:{item.Name} is found while converting {nameof(SpecificBanner)}");
                    }
                    //fix issue where crashes when no banner exists
                    banner?.Items.Add(ni);
                }
            }

            CalculateDetails(results);

            return results
                .Where(b => b.TotalCount > 0)
                .OrderByDescending(b => b.StartTime)
                .ThenBy(b => b.Type)
                .ToList();
        }
        private static void CalculateDetails(List<SpecificBanner> results)
        {
            foreach (SpecificBanner banner in results)
            {
                banner.TotalCount = banner.Items.Count;
                if (banner.TotalCount == 0)
                    continue;

                banner.Star5Count = banner.Items.Count(i => i.StarUrl.ToRank() == 5);
                banner.Star4Count = banner.Items.Count(i => i.StarUrl.ToRank() == 4);
                banner.Star3Count = banner.Items.Count(i => i.StarUrl.ToRank() == 3);

                List<StatisticItem> statisticList = ToTotalCountList(banner.Items);
                banner.StatisticList5 = statisticList.Where(i => i.StarUrl.ToRank() == 5).ToList();
                banner.StatisticList4 = statisticList.Where(i => i.StarUrl.ToRank() == 4).ToList();
                banner.StatisticList3 = statisticList.Where(i => i.StarUrl.ToRank() == 3).ToList();
            }
        }
    }
}