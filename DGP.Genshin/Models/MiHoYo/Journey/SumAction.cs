using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Journey
{
    public class SumAction : JourneyAction
    {
        [JsonProperty("percent")] public int Percent { get; set; }
    }

}
