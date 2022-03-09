using System;

namespace DGP.Genshin.DataModel.GachaStatistic.Banner
{
    /// <summary>
    /// 单个卡池统计信息
    /// </summary>
    public class StatisticBanner : ProbabilityBanner
    {
        public int CountSinceLastStar5 { get; set; }
        public int CountSinceLastStar4 { get; set; }
        [Obsolete("不再为此属性设置值")] public int NextStar5PredictCount { get; set; }
        [Obsolete("不再为此属性设置值")] public int NextStar4PredictCount { get; set; }
        public double AverageGetStar5 { get; set; }
        public int MaxGetStar5Count { get; set; }
        public int MinGetStar5Count { get; set; }
        public string? NextGuaranteeType { get; set; }
        [Obsolete("不再为此属性设置值")] public string? Appraise { get; set; }
    }
}
