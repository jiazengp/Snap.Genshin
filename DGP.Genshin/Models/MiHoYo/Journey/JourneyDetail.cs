using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Journey
{
    public class JourneyDetail : JourneyBase
    {
        [JsonProperty("page")] public int Page { get; set; }
        [JsonProperty("list")] public List<DetailAction> List { get; set; }
    }

}
