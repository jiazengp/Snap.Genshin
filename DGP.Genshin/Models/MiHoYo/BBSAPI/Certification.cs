using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.BBSAPI
{
    public class Certification
    {
        [JsonProperty("type")] public int Type { get; set; }
        [JsonProperty("label")] public string Label { get; set; }
    }

}
