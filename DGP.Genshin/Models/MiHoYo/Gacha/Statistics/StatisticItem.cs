using DGP.Snap.Framework.Attributes.DataModel;

namespace DGP.Genshin.Models.MiHoYo.Gacha.Statistics
{
    /// <summary>
    /// 带有个数统计的奖池统计物品
    /// </summary>
    [InterModel]
    public class StatisticItem : SpecificItem
    {
        public int Count { get; set; }
    }
}
