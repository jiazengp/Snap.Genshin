using Snap.Data.Primitive;

namespace DGP.Genshin.DataModel.GachaStatistic.Item
{
    /// <summary>
    /// 带有个数统计的奖池统计物品
    /// </summary>
    public class StatisticItem : SpecificItem, IPartiallyCloneable<StatisticItem>
    {
        public int Count { get; set; }

        /// <summary>
        /// 隐藏了数量的克隆
        /// </summary>
        /// <returns></returns>
        public StatisticItem ClonePartially()
        {
            return new()
            {
                StarUrl = StarUrl,
                Source = Source,
                Name = Name,
                Badge = Badge,
                Time = Time
            };
        }
    }
}
