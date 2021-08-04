using DGP.Genshin.Data.Helpers;
using DGP.Genshin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Models.MiHoYo.Gacha.Statistics
{
    /// <summary>
    /// a very dirty builders
    /// </summary>
    public class StatisticFactory
    {
        public static Statistic ToStatistic(GachaData gachaData, string uid)
        {
            Dictionary<string, List<GachaLogItem>> dict = gachaData.GachaLogs;
            return new Statistic()
            {
                Uid = uid,
                Permanent = ToBanner(dict, ConfigType.PermanentWish, "奔行世间"),
                WeaponEvent = ToBanner(dict, ConfigType.WeaponEventWish, "神铸赋形"),
                CharacterEvent = ToBanner(dict, ConfigType.CharacterEventWish, "角色活动"),
                Characters = ToTotalCountList(dict, "角色"),
                Weapons = ToTotalCountList(dict, "武器")
            };
        }

        private static Banner ToBanner(Dictionary<string, List<GachaLogItem>> dict, string type, string name)
        {
            List<GachaLogItem> list = dict[type];
            Banner banner = new Banner()
            {
                TotalCount = list.Count,
                StartTime = list.Last().Time,
                EndTime = list.First().Time,
                CurrentName = name,
                CountSinceLastStar5 = list.FindIndex(i => i.Rank == "5"),
                Star5Count = list.Count(i => i.Rank == "5"),
                Star4Count = list.Count(i => i.Rank == "4"),
                Star3Count = list.Count(i => i.Rank == "3"),
            };
            banner.Star5List = CountStar5(list, banner.Star5Count);

            if (banner.Star5List.Count > 0)
            {
                banner.AverageGetStar5 = banner.Star5List.Sum(i => i.Count) * 1.0 / banner.Star5List.Count;
                banner.MaxGetStar5Count = banner.Star5List.Max(i => i.Count);
                banner.MinGetStar5Count = banner.Star5List.Min(i => i.Count);
            }
            else//while no 5 star get
            {
                banner.AverageGetStar5 = 0.0;
                banner.MaxGetStar5Count = 0;
                banner.MinGetStar5Count = 0;
            }

            banner.Star5Prob = banner.Star5Count * 1.0 / banner.TotalCount;
            banner.Star4Prob = banner.Star4Count * 1.0 / banner.TotalCount;
            banner.Star3Prob = banner.Star3Count * 1.0 / banner.TotalCount;
            return banner;
        }

        public static List<Item> ToTotalCountList(Dictionary<string, List<GachaLogItem>> dict, string itemType)
        {
            Dictionary<string, Item> counter = new Dictionary<string, Item>();
            foreach (List<GachaLogItem> list in dict.Values)
            {
                foreach (GachaLogItem i in list)
                {
                    if (i.ItemType == itemType)
                    {
                        if (!counter.ContainsKey(i.Name))
                        {
                            counter[i.Name] = new Item()
                            {
                                Count = 0,
                                Name = i.Name,
                                StarUrl = StarHelper.FromRank(Int32.Parse(i.Rank))
                            };
                            if (i.ItemType == "武器")
                            {
                                Data.Weapons.Weapon weapon = DataService.Instance.Weapons.First(w => w.Name == i.Name);
                                counter[i.Name].Source = weapon.Source;
                            }
                            else//角色
                            {
                                Data.Characters.Character character = DataService.Instance.Characters.First(c => c.Name == i.Name);
                                counter[i.Name].Source = character.Source;
                                counter[i.Name].Element = character.Element;
                            }
                        }
                        counter[i.Name].Count += 1;
                    }
                }
            }
            return counter.Select(k => k.Value).OrderByDescending(i => i.StarUrl.ToRank()).ThenByDescending(i => i.Count).ToList();
        }

        private static List<Star5Item> CountStar5(IEnumerable<GachaLogItem> items, int star5Count)
        {
            //prevent modify the items and simplify the algorithm
            List<GachaLogItem> reversedItems = items.Reverse().ToList();
            List<Star5Item> counter = new List<Star5Item>();
            for (int i = 0; i < star5Count; i++)
            {

                GachaLogItem currentStar5 = reversedItems.First(i => i.Rank == "5");
                int count = reversedItems.IndexOf(currentStar5) + 1;
                counter.Add(new Star5Item()
                {
                    Name = currentStar5.Name,
                    Count = count,
                    Time = currentStar5.Time
                });
                reversedItems.RemoveRange(0, count);
            }
            counter.Reverse();
            return counter;
        }
    }
}
