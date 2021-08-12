using Newtonsoft.Json;
using System;

namespace DGP.Genshin.Models.MiHoYo.Gacha
{
    /// <summary>
    /// 表示获取的单个卡池物品信息
    /// </summary>
    public class GachaLogItem
    {
        [JsonProperty("uid")] public string Uid { get; set; }
        [JsonProperty("gacha_type")] public string GachaType { get; set; }
        [JsonProperty("item_id")] public string ItemId { get; set; }
        [JsonProperty("count")] public string Count { get; set; }
        [JsonProperty("time")] public DateTime Time { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("lang")] public string Language { get; set; }
        [JsonProperty("item_type")] public string ItemType { get; set; }
        [JsonProperty("rank_type")] public string Rank { get; set; }
        [JsonProperty("id")] public string Id { get; set; }

        [JsonIgnore] public long TimeId => Int64.Parse(this.Id);
    }
}
