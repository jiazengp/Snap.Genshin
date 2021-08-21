using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha
{
    /// <summary>
    /// 表示获取的卡池片段信息
    /// </summary>
    [JsonModel]
    public class GachaLog
    {
        [JsonProperty("page")] public string Page { get; set; }
        [JsonProperty("size")] public string Size { get; set; }
        [Obsolete("获取的数据中的此属性与实际收到的数据不会相符")] [JsonProperty("total")] public string Total { get; set; }
        [JsonProperty("list")] public List<GachaLogItem> List { get; set; }
        [JsonProperty("region")] public string Region { get; set; }
        [JsonIgnore] public long NewestTimeId { get; set; }
    }
}
