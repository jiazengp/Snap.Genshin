using DGP.Snap.Framework.Attributes.DataModel;
using DGP.Snap.Framework.Core;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha.Statistics
{
    /// <summary>
    /// 20或14天一轮的up卡池信息
    /// </summary>
    [InterModel]
    public class SpecificBanner : Banner, IPartiallyCloneable<SpecificBanner>
    {
        public string Type { get; set; }
        public List<SpecificItem> UpStar5List { get; set; }
        public List<SpecificItem> UpStar4List { get; set; }
        [JsonIgnore] public List<StatisticItem> StatisticList { get; set; }
        [JsonIgnore] public List<StatisticItem> StatisticList5 { get; set; }
        [JsonIgnore] public List<StatisticItem> StatisticList4 { get; set; }
        [JsonIgnore] public List<StatisticItem> StatisticList3 { get; set; }
        [JsonIgnore] public bool IsWeaponBanner { get; set; }
        [JsonIgnore] public List<SpecificItem> Items { get; set; } = new List<SpecificItem>();

        public SpecificBanner ClonePartially()
        {
            return new SpecificBanner
            {
                Type = this.Type,
                UpStar5List = this.UpStar5List,
                UpStar4List = this.UpStar4List,
                CurrentName = this.CurrentName,
                StartTime = this.StartTime,
                EndTime = this.EndTime
            };
        }

        public override string ToString() => $"{this.CurrentName} | {this.StartTime:yyyy.MM.dd HH:mm} - {this.EndTime:yyyy.MM.dd HH:mm}";
    }


}
