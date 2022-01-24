using Newtonsoft.Json;
using System;

namespace DGP.Genshin.DataModel.GachaStatistic
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

        [JsonIgnore] public DateTime Time { get; set; }
        public override string ToString()
        {
            return $"{Time}-{Name}";
        }
    }
}
