using System;

namespace DGP.Genshin.DataModel.GachaStatistic.Item
{
    /// <summary>
    /// 用于展示的卡池物品
    /// </summary>
    public class SpecificItem
    {
        public string? StarUrl { get; set; }
        public string? Source { get; set; }
        public string? Name { get; set; }
        public string? Badge { get; set; }
        public DateTime Time { get; set; }

        public override string ToString()
        {
            return $"{Time}-{Name}";
        }

        public void CopyFromPrimitive(Primitive matched)
        {
            StarUrl = matched.Star;
            Source = matched.Source;
            Name = matched.Name;
            Badge = matched.GetBadge();
        }
    }
}
