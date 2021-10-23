using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Record
{
    /// <summary>
    /// 供奉信息
    /// </summary>
    [JsonModel]
    public class Offering
    {
        [JsonProperty("name")] public string? Name { get; set; }
        [JsonProperty("level")] public string? Level { get; set; }
    }
}
