using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Models.MiHoYo
{
    public class GachaData
    {
        [JsonProperty("gachaType")] public IEnumerable<GachaConfigType> Types { get; set; }
        [JsonProperty("gachaLog")] public IDictionary<string,IEnumerable<GachaLogItem>> GachaLogs { get; set; }
        [JsonProperty("uid")] public string Uid { get; set; }
    }
}
