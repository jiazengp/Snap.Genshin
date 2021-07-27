using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha
{
    public class GachaLogData
    {
        [JsonProperty("page")] public string Page { get; set; }
        [JsonProperty("size")] public string Size { get; set; }
        [JsonProperty("total")] public string Total { get; set; }
        [JsonProperty("list")] public List<GachaLogItem> List { get; set; }
        [JsonProperty("region")] public string Region { get; set; }
    }
}
