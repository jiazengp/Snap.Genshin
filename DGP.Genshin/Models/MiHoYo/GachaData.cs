using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo
{
    public class GachaData
    {
        [JsonProperty("gachaType")] public IEnumerable<GachaConfigType> Types { get; set; }
        [JsonProperty("gachaLog")] public IDictionary<string, IEnumerable<GachaLogItem>> GachaLogs { get; set; }
        [JsonProperty("uid")] public string Uid { get; set; }
        [JsonProperty("url")] public string Url { get; set; }

    }
}
