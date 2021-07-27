using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Gacha
{
    public class GachaConfigData
    {
        [JsonProperty("gacha_type_list")] public List<GachaConfigType> GachaTypeList { get; set; }
        [JsonProperty("region")] public string Region { get; set; }
    }
}
