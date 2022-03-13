using DGP.Genshin.MiHoYoAPI.Gacha;

namespace DGP.Genshin.Service.Abstraction.GachaStatistic
{
    /// <summary>
    /// 表示可导入的数据实体
    /// 仅能表示一个uid的数据
    /// </summary>
    public class ImportableGachaData
    {
        public GachaData? Data { get; set; }
        public string? Uid { get; set; }
    }
}