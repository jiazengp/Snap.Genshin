using Newtonsoft.Json;
using System;

namespace DGP.Genshin.Models.MiHoYo.Record
{
    [Obsolete]
    internal class CityExploration
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("level")] public int Level { get; set; }
        [JsonProperty("exploration_percentage")] public int ExplorationPercentage { get; set; }
        [JsonProperty("icon")] public string Icon { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
    }
}
