using DGP.Genshin.DataModel.GachaStatistic.Item;
using Newtonsoft.Json;
using Snap.Data.Primitive;
using System;
using System.Collections.Generic;

namespace DGP.Genshin.DataModel.GachaStatistic.Banner
{
    /// <summary>
    /// 20或14天一轮的up卡池信息
    /// </summary>
    public class SpecificBanner : BannerBase, IPartiallyCloneable<SpecificBanner>
    {
        public string? Type { get; set; }
        public List<SpecificItem>? UpStar5List { get; set; }
        public List<SpecificItem>? UpStar4List { get; set; }
        public List<StatisticItem>? StatisticList5 { get; set; }
        public List<StatisticItem>? StatisticList4 { get; set; }
        public List<StatisticItem>? StatisticList3 { get; set; }
        public List<SpecificItem> Items { get; set; } = new();
        public List<List<SpecificItem>> Slices { get; set; } = new();

        public SpecificBanner ClonePartially()
        {
            return new SpecificBanner
            {
                Type = Type,
                UpStar5List = UpStar5List,
                UpStar4List = UpStar4List,
                CurrentName = CurrentName,
                StartTime = StartTime,
                EndTime = EndTime
            };
        }
        public void ClearItemsAndStar5List()
        {
            Items?.Clear();
            Star5List?.Clear();
        }
    }
}
