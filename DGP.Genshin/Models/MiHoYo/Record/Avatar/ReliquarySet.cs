using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Record.Avatar
{
    internal class ReliquarySet
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("affixes")] public List<ReliquaryAffix> Affixes { get; set; }
    }
}
