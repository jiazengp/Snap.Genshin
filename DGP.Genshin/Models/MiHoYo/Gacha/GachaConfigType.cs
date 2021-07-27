using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Gacha
{
    public class GachaConfigType
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("key")] public string Key { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
    }
}
