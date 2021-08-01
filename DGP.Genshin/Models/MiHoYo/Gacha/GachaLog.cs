using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha
{
    public class GachaLog
    {
        [JsonProperty("page")] public string Page { get; set; }
        [JsonProperty("size")] public string Size { get; set; }
        [Obsolete] [JsonProperty("total")] public string Total { get; set; }
        [JsonProperty("list")] public List<GachaLogItem> List { get; set; }
        [JsonProperty("region")] public string Region { get; set; }
        [JsonIgnore] public long NewestTimeId { get; set; }
    }
}
