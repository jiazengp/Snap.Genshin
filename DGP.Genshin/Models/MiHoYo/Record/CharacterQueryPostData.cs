using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Record
{
    internal class CharacterQueryPostData
    {
        [JsonProperty("character_ids")] public List<int> CharacterIds { get; set; }
        [JsonProperty("role_id")] public string RoleId { get; set; }
        [JsonProperty("server")] public string Server { get; set; }
    }
}
