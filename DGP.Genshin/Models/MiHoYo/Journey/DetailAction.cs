using Newtonsoft.Json;
using System;

namespace DGP.Genshin.Models.MiHoYo.Journey
{
    public class DetailAction : JourneyAction
    {
        [JsonProperty("time")] public DateTime Percent { get; set; }
    }

}
