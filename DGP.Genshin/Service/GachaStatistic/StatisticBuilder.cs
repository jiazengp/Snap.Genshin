using DGP.Genshin.DataModel;
using DGP.Genshin.DataModel.GachaStatistic;
using DGP.Genshin.DataModel.GachaStatistic.Banner;
using DGP.Genshin.DataModel.GachaStatistic.Item;
using DGP.Genshin.DataModel.Helper;
using DGP.Genshin.Helper;
using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.Service.Abstraction.Setting;
using DGP.Genshin.ViewModel;
using Microsoft;
using Snap.Core.Logging;
using Snap.Exception;
using Snap.Extenion.Enumerable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Service.GachaStatistic
{
    /// <summary>
    /// 构造奖池统计信息的建造器
    /// very dirty
    /// </summary>
    internal class StatisticBuilder
    {
        private const double NonWeaponProb5 = 1.6 / 100.0;
        private const double NonWeaponProb4 = 13 / 100.0;
        private const int NonWeaponGranteeCount5 = 90;
        private const double WeaponProb5 = 1.85 / 100.0;
        private const double WeaponProb4 = 14.5 / 100.0;
        private const int WeaponGranteeCount5 = 80;
        private const string RankFive = "5";
        private const string RankFour = "4";
        private const string RankThree = "3";
        private const string MinGuarantee = "小保底";
        private const string MaxGuarantee = "大保底";

        /// <summary>
        /// 卡池概率配置选项
        /// </summary>
        private record BannerInformation
        {
            public double Prob5 { get; set; }
            public double Prob4 { get; set; }
            public int GranteeCount { get; set; }
        }

        /// <summary>
        /// 表示一个对 <see cref="T"/> 类型的计数器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class CounterOf<T> : Dictionary<string, T> { }

        private static readonly BannerInformation NonWeaponInfo = new()
        {
            Prob5 = NonWeaponProb5,
            Prob4 = NonWeaponProb4,
            GranteeCount = NonWeaponGranteeCount5
        };
        private static readonly BannerInformation WeaponInfo = new()
        {
            Prob5 = WeaponProb5,
            Prob4 = WeaponProb4,
            GranteeCount = WeaponGranteeCount5
        };

        private readonly MetadataViewModel metadataViewModel;
        public StatisticBuilder(MetadataViewModel metadataViewModel)
        {
            this.metadataViewModel = metadataViewModel;
        }

        /// <summary>
        /// 转换到统计
        /// </summary>
        /// <param name="data"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public Statistic ToStatistic(GachaData data, string uid)
        {
            Logger.LogStatic($"convert data of {uid} to statistic view");
            List<StatisticItem> characters = ToStatisticTotalCountList(data, "角色");
            List<StatisticItem> weapons = ToStatisticTotalCountList(data, "武器");
            Statistic statistic = new()
            {
                Uid = uid,
                Characters5 = characters.Where(i => i.StarUrl?.ToInt32Rank() == 5).ToList(),
                Characters4 = characters.Where(i => i.StarUrl?.ToInt32Rank() == 4).ToList(),
                Weapons5 = weapons.Where(i => i.StarUrl?.ToInt32Rank() == 5).ToList(),
                Weapons4 = weapons.Where(i => i.StarUrl?.ToInt32Rank() == 4).ToList(),
                Weapons3 = weapons.Where(i => i.StarUrl?.ToInt32Rank() == 3).ToList(),
                Permanent = ToStatisticBanner(data, ConfigType.PermanentWish, "奔行世间", NonWeaponInfo),
                WeaponEvent = ToStatisticBanner(data, ConfigType.WeaponEventWish, "神铸赋形", WeaponInfo),
                CharacterEvent = ToStatisticBanner(data, ConfigType.CharacterEventWish, "角色活动", NonWeaponInfo),
                SpecificBanners = ToSpecificBanners(data)
            };
            return statistic;
        }

        /// <summary>
        /// 转换到统计卡池
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private StatisticBanner ToStatisticBanner(GachaData data, string type, string name, BannerInformation info)
        {
            return data.TryGetValue(type, out List<GachaLogItem>? list)
                ? BuildStatisticBanner(name, list!)
                : (new() { CurrentName = name });
        }

        private StatisticBanner BuildStatisticBanner(string name, List<GachaLogItem> list)
        {
            //上次出货抽数
            int index5 = list.FindIndex(i => i.Rank == RankFive);
            int index4 = list.FindIndex(i => i.Rank == RankFour);

            StatisticBanner banner = new()
            {
                TotalCount = list.Count,
                CurrentName = name,
                CountSinceLastStar5 = index5 == -1 ? 0 : index5,
                CountSinceLastStar4 = index4 == -1 ? 0 : index4,

                Star5Count = list.Count(i => i.Rank == RankFive),
                Star4Count = list.Count(i => i.Rank == RankFour),
                Star3Count = list.Count(i => i.Rank == RankThree),
            };
            if (list.Count > 0)
            {
                banner.StartTime = list.Last().Time;
                banner.EndTime = list.First().Time;
            }
            banner.Star5List = ListOutStatisticStar5(list, banner.Star5Count);
            //确保至少有一个五星
            if (banner.Star5List.Count > 0)
            {
                banner.AverageGetStar5 = banner.Star5List.Sum(i => i.Count) * 1.0 / banner.Star5List.Count;
                banner.MaxGetStar5Count = banner.Star5List.Max(i => i.Count);
                banner.MinGetStar5Count = banner.Star5List.Min(i => i.Count);
                banner.NextGuaranteeType = banner.Star5List.First().IsUp ? MinGuarantee : MaxGuarantee;
            }
            else//while no 5 star get
            {
                banner.AverageGetStar5 = 0.0;
                banner.MaxGetStar5Count = 0;
                banner.MinGetStar5Count = 0;
                banner.NextGuaranteeType = MinGuarantee;
            }

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
        private List<StatisticItem> ToStatisticTotalCountList(GachaData data, string itemType)
        {
            CounterOf<StatisticItem> counter = new();
            foreach (List<GachaLogItem>? list in data.Values)
            {
                _ = list ?? throw new UnexpectedNullException("卡池列表不应为 null");
                foreach (GachaLogItem i in list)
                {
                    if (i.ItemType == itemType)
                    {
                        _ = i.Name ?? throw new UnexpectedNullException("卡池物品名称不应为 null");
                        if (!counter.ContainsKey(i.Name))
                        {
                            _ = i.Rank ?? throw new UnexpectedNullException("卡池物品稀有度不应为 null");
                            counter[i.Name] = new StatisticItem()
                            {
                                Count = 0,
                                Name = i.Name,
                                StarUrl = StarHelper.FromInt32Rank(int.Parse(i.Rank))
                            };
                            Primitive? item = metadataViewModel.FindPrimitiveByName(i.Name);
                            counter[i.Name].Source = item?.Source;
                            counter[i.Name].Badge = item?.GetBadge();
                        }
                        counter[i.Name].Count += 1;
                    }
                }
            }
            return counter
                .Select(k => k.Value)
                .OrderByDescending(i => i.StarUrl?.ToInt32Rank())
                .ThenByDescending(i => i.Count)
                .ToList();
        }
        private List<StatisticItem> ToSpecificTotalCountList(List<SpecificItem> list)
        {
            CounterOf<StatisticItem> counter = new();
            foreach (SpecificItem item in list)
            {
                if (item.Name is not null)
                {
                    if (!counter.ContainsKey(item.Name))
                    {
                        counter[item.Name] = new StatisticItem()
                        {
                            Count = 0,
                            Name = item.Name,
                            StarUrl = item.StarUrl,
                            Source = item.Source,
                            Badge = item.Badge,
                            Time = item.Time
                        };
                    }
                    counter[item.Name].Count += 1;
                }
            }
            return counter.Select(k => k.Value)
                .OrderByDescending(i => i.StarUrl?.ToInt32Rank())
                .ThenByDescending(i => i.Count)
                .ToList();
        }
        private List<StatisticItem5Star> ListOutStatisticStar5(IEnumerable<GachaLogItem> items, int star5Count)
        {
            //prevent modify the items and simplify the algorithm
            //search from the earliest time
            List<GachaLogItem> reversedItems = items.Reverse().ToList();
            List<StatisticItem5Star> counter = new();
            for (int i = 0; i < star5Count; i++)
            {
                GachaLogItem currentStar5 = reversedItems.First(i => i.Rank == RankFive);
                int count = reversedItems.IndexOf(currentStar5) + 1;
                bool isBigGuarantee = counter.Count > 0 && !counter.Last().IsUp;

                SpecificBanner? matchedBanner = metadataViewModel.SpecificBanners?.Find(b =>
                    //match type first
                    b.Type == currentStar5.GachaType &&
                    currentStar5.Time >= b.StartTime &&
                    currentStar5.Time <= b.EndTime);

                string? gachTypeId = currentStar5.GachaType;
                counter.Add(new StatisticItem5Star()
                {
                    GachaTypeName = gachTypeId is null ? gachTypeId : ConfigType.Known[gachTypeId],
                    Source = metadataViewModel.FindSourceByName(currentStar5.Name),
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
        private List<SpecificBanner> ToSpecificBanners(GachaData data)
        {
            //clone from metadata
            List<SpecificBanner>? clonedBanners = metadataViewModel.SpecificBanners?.ClonePartially();
            Requires.NotNull(clonedBanners!, nameof(clonedBanners));

            clonedBanners.ForEach(b => b.ClearItemsAndStar5List());
            //Type = ConfigType.PermanentWish fix order problem
            SpecificBanner permanent = new() { CurrentName = "奔行世间", Type = ConfigType.PermanentWish };
            clonedBanners.Add(permanent);

            foreach (string type in data.Keys)
            {
                if (type is ConfigType.NoviceWish)
                {
                    //skip this banner
                    continue;
                }
                if (type is ConfigType.PermanentWish)
                {
                    if (data[type] is List<GachaLogItem> permanentlist)
                    {
                        //set init time
                        permanent.StartTime = permanentlist[0].Time;
                        permanent.EndTime = permanentlist[0].Time;
                        foreach (GachaLogItem item in permanentlist)
                        {
                            if (item.Time < permanent.StartTime)
                            {
                                permanent.StartTime = item.Time;
                            }
                            if (item.Time > permanent.EndTime)
                            {
                                permanent.EndTime = item.Time;
                            }
                            AddItemToSpecificBanner(item, permanent);
                        }
                    }
                    continue;
                }
                if (data[type] is List<GachaLogItem> list)
                {
                    foreach (GachaLogItem item in list)
                    {
                        //add item.GachaType compare to compat 2.3+ gacha
                        SpecificBanner? banner = clonedBanners
                            .Find(b => b.Type == item.GachaType && item.Time >= b.StartTime && item.Time <= b.EndTime);
                        AddItemToSpecificBanner(item, banner);
                    }
                }
            }

            CalculateSpecificBannersDetail(clonedBanners);

            List<SpecificBanner> resultList = clonedBanners
                .WhereWhen(b => b.TotalCount > 0, !Setting2.IsBannerWithNoItemVisible.Get())
                .OrderByDescending(b => b.StartTime)
                .ThenByDescending(b => ConfigType.Order[b.Type!])
                .ToList();
            //after ordering this permanant banner probably in the middle of the list
            resultList.Remove(permanent);
            //add back to the last
            resultList.Add(permanent);
            return resultList;
        }
        private void AddItemToSpecificBanner(GachaLogItem item, SpecificBanner? banner)
        {
            Primitive? matched = metadataViewModel.FindPrimitiveByName(item.Name);

            SpecificItem newItem = new() { Time = item.Time };

            if (matched is not null)
            {
                newItem.StarUrl = matched.Star;
                newItem.Source = matched.Source;
                newItem.Name = matched.Name;
                newItem.Badge = matched.GetBadge();
            }
            else
            {
                newItem.Name = item.Name;
                newItem.StarUrl = item.Rank is null ? null : StarHelper.FromInt32Rank(int.Parse(item.Rank));
                new Event("Unsupported Item", item.Name ?? "No name").TrackAs(Event.GachaStatistic);
            }
            //? fix issue where crashes when no banner exists
            banner?.Items.Add(newItem);
        }

        /// <summary>
        /// 统计特定卡池的总数、345星数
        /// </summary>
        /// <param name="specificBanners"></param>
        private void CalculateSpecificBannersDetail(List<SpecificBanner> specificBanners)
        {
            foreach (SpecificBanner banner in specificBanners)
            {
                banner.TotalCount = banner.Items.Count;
                if (banner.TotalCount == 0)
                {
                    continue;
                }

                BuildSpecificBannerSlices(banner);

                banner.Star5Count = banner.Items.Count(i => i.StarUrl.IsRankAs(5));
                banner.Star4Count = banner.Items.Count(i => i.StarUrl.IsRankAs(4));
                banner.Star3Count = banner.Items.Count(i => i.StarUrl.IsRankAs(3));

                List<StatisticItem> statisticList = ToSpecificTotalCountList(banner.Items);

                banner.StatisticList5 = statisticList.Where(i => i.StarUrl.IsRankAs(5)).ToList();
                banner.StatisticList4 = statisticList.Where(i => i.StarUrl.IsRankAs(4)).ToList();
                banner.StatisticList3 = statisticList.Where(i => i.StarUrl.IsRankAs(3)).ToList();
            }
        }

        private void BuildSpecificBannerSlices(SpecificBanner banner)
        {
            int sliceIndex = 0;
            foreach (SpecificItem item in Enumerable.Reverse(banner.Items))
            {
                bool postIncreaseFlag = false;
                if (item.StarUrl.ToInt32Rank() == 4)
                {
                    postIncreaseFlag = true;
                }
                //prepare new list
                if (banner.Slices.Count <= sliceIndex)
                {
                    banner.Slices.Add(new());
                }
                banner.Slices[sliceIndex].Add(item);
                if (postIncreaseFlag)
                {
                    sliceIndex++;
                }
            }
            banner.Slices.Reverse();
        }
    }
}