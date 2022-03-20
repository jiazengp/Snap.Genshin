using DGP.Genshin.DataModel.GachaStatistic.Item;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DGP.Genshin.DataModel.GachaStatistic.Banner
{
    /// <summary>
    /// 卡池共有基本信息
    /// </summary>
    public abstract class BannerBase
    {
        public int TotalCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? CurrentName { get; set; }
        public List<StatisticItem5Star>? Star5List { get; set; }
        public int Star5Count { get; set; }
        public int Star4Count { get; set; }
        public int Star3Count { get; set; }
    }
}