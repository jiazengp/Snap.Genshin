namespace DGP.Genshin.Services.GachaStatistics.Statistics
{
    /// <summary>
    /// 单个卡池统计信息
    /// </summary>
    public class StatisticBanner : Banner
    {
        public int CountSinceLastStar5 { get; set; }
        public int CountSinceLastStar4 { get; set; }
        public int NextStar5PredictCount { get; set; }
        public int NextStar4PredictCount { get; set; }
        public double AverageGetStar5 { get; set; }
        public int MaxGetStar5Count { get; set; }
        public int MinGetStar5Count { get; set; }
        public string? NextGuaranteeType { get; set; }
        public string? Appraise { get; set; }
    }
}
