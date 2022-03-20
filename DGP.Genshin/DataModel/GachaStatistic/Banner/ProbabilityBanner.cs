using Newtonsoft.Json;

namespace DGP.Genshin.DataModel.GachaStatistic.Banner
{
    /// <summary>
    /// 单个卡池信息
    /// 带有额外的3个星级的概率
    /// </summary>
    public abstract class ProbabilityBanner : BannerBase
    {
        public double Star5Prob { get; set; }
        public double Star4Prob { get; set; }
        public double Star3Prob { get; set; }
    }
}
