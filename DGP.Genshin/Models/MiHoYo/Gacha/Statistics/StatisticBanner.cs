using System;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha.Statistics
{
    /// <summary>
    /// 单个奖池统计信息
    /// </summary>
    public class StatisticBanner
    {
        public int TotalCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string CurrentName { get; set; }

        public int CountSinceLastStar5 { get; set; }
        public List<StatisticItem5Star> Star5List { get; set; }
        public double Star5Prob { get; set; }
        public int Star5Count { get; set; }
        public double Star4Prob { get; set; }
        public int Star4Count { get; set; }
        public double Star3Prob { get; set; }
        public int Star3Count { get; set; }

        public int NextStar5PredictCount { get; set; }

        public double AverageGetStar5 { get; set; }
        public int MaxGetStar5Count { get; set; }
        public int MinGetStar5Count { get; set; }
    }
}
