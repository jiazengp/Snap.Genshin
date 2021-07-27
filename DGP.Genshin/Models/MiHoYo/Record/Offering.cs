using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Record
{
    internal class Offering
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("level")] public string Level { get; set; }
    }
}
