using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;
using System;

namespace DGP.Genshin.Models.MiHoYo.Gacha.Statistics
{
    /// <summary>
    /// 用于展示的卡池物品
    /// </summary>
    [InterModel]
    public class SpecificItem
    {
        public string? StarUrl { get; set; }
        public string? Source { get; set; }
        public string? Name { get; set; }
        public string? Badge { get; set; }

        [JsonIgnore] public DateTime Time { get; set; }
        public override string ToString() => $"{this.Time}-{this.Name}";
    }
}
