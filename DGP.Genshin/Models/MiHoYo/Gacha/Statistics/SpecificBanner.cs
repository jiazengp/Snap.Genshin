using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha.Statistics
{
    /// <summary>
    /// 20或14天一轮的up卡池信息
    /// </summary>
    [InterModel]
    public class SpecificBanner : Banner
    {
        public string Type { get; set; }
        public List<SpecificItem> UpStar5List { get; set; }
        public List<SpecificItem> UpStar4List { get; set; }
        [JsonIgnore] public List<StatisticItem> StatisticList { get; set; }
        [JsonIgnore] public List<SpecificItem> Items { get; set; } = new List<SpecificItem>();
        public override string ToString() => $"{this.CurrentName} | {this.StartTime:yyyy.MM.dd HH:mm} - {this.EndTime:yyyy.MM.dd HH:mm}";
    }
}
