using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Models.MiHoYo
{
    public class GachaConfigData
    {
        [JsonProperty("gacha_type_list")] public List<GachaConfigType> GachaTypeList { get; set; }
        [JsonProperty("region")] public string Region { get; set; }
    }
}
