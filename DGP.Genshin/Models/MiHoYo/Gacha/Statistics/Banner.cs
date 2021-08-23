using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha.Statistics
{
    /// <summary>
    /// 单个卡池信息
    /// </summary>
    [InterModel]
    public abstract class Banner
    {
        [JsonIgnore] public int TotalCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string CurrentName { get; set; }
        [JsonIgnore] public List<StatisticItem5Star> Star5List { get; set; }
        [JsonIgnore] public double Star5Prob { get; set; }
        [JsonIgnore] public int Star5Count { get; set; }
        [JsonIgnore] public double Star4Prob { get; set; }
        [JsonIgnore] public int Star4Count { get; set; }
        [JsonIgnore] public double Star3Prob { get; set; }
        [JsonIgnore] public int Star3Count { get; set; }
    }
}
