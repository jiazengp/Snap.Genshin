using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Record
{
    internal class PlayerInfo
    {
        [JsonProperty("avatars")] public List<Avatar.Avatar> Avatars { get; set; }
        [JsonProperty("stats")] public PlayerStats PlayerStat { get; set; }
        //[JsonProperty("city_explorations")] public List<CityExploration> CityExplorations { get; set; } = new List<CityExploration>();
        [JsonProperty("world_explorations")] public List<WorldExploration> WorldExplorations { get; set; }
        [JsonProperty("homes")] public List<Home> Homes { get; set; } = new List<Home>();
    }
}
