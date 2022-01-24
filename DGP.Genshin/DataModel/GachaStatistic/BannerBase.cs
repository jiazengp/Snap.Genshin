using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DGP.Genshin.DataModel.GachaStatistic
{
    public abstract class BannerBase
    {
        [JsonIgnore] public int TotalCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? CurrentName { get; set; }
        [JsonIgnore] public List<StatisticItem5Star>? Star5List { get; set; }
        [JsonIgnore] public int Star5Count { get; set; }
        [JsonIgnore] public int Star4Count { get; set; }
        [JsonIgnore] public int Star3Count { get; set; }
    }
}