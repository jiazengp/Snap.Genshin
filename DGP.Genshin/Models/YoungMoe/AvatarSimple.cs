using Newtonsoft.Json;

namespace DGP.Genshin.Models.YoungMoe
{
    public class AvatarSimple
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("cnname")] public string? CnName { get; set; }
        [JsonProperty("enname")] public string? EnName { get; set; }
        [JsonProperty("link")] public string? Link { get; set; }
    }
}
