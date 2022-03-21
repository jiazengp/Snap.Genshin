namespace DGP.Genshin.DataModel.GachaStatistic.Banner
{
    /// <summary>
    /// 单个卡池统计信息
    /// </summary>
    public class StatisticBanner : ProbabilityBanner
    {
        public int CountSinceLastStar5 { get; set; }
        public int CountSinceLastStar4 { get; set; }
        public double AverageGetStar5 { get; set; }
        public int MaxGetStar5Count { get; set; }
        public int MinGetStar5Count { get; set; }
        public string? NextGuaranteeType { get; set; }
    }
}
