using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.BBSAPI
{
    [JsonModel]
    public class Certification
    {
        [JsonProperty("type")] public int Type { get; set; }
        [JsonProperty("label")] public string Label { get; set; }
    }

}
