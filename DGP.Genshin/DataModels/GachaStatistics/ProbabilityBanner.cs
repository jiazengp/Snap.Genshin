using Newtonsoft.Json;

namespace DGP.Genshin.DataModels.GachaStatistics
{
    /// <summary>
    /// 单个卡池信息
    /// </summary>
    public abstract class ProbabilityBanner : BannerBase
    {
        [JsonIgnore] public double Star5Prob { get; set; }
        [JsonIgnore] public double Star4Prob { get; set; }
        [JsonIgnore] public double Star3Prob { get; set; }
    }
}
