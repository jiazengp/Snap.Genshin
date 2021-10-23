using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.UserInfo
{
    [JsonModel]
    public class Certification
    {
        [JsonProperty("type")] public int Type { get; set; }
        [JsonProperty("label")] public string? Label { get; set; }
    }
}
